//---------------------------------------------------------------------
// File: ReceiveBatch.cs
// 
// Summary: Implementation of an adapter framework sample adapter. 
// This class constitutes one of the BaseAdapter classes, which, are
// a set of generic re-usable set of classes to help adapter writers.
//
// Sample: Base Adapter Class Library v1.0.2
//
// Description: Batching logic intended for Receive side adapters - supports submitting messages
//
//---------------------------------------------------------------------
// This file is part of the Microsoft BizTalk Server 2006 SDK
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// This source code is intended only as a supplement to Microsoft BizTalk
// Server 2006 release and/or on-line documentation. See these other
// materials for detailed information regarding Microsoft code samples.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
// PURPOSE.
//---------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.BizTalk.Message.Interop;

using System.Collections.Generic;

namespace Blogical.Shared.Adapters.Common
{
    public delegate void ReceiveBatchCompleteHandler(bool overallStatus);

    public class ReceiveBatch : Batch
    {
        public ReceiveBatch (IBTTransportProxy transportProxy, ControlledTermination control, ManualResetEvent orderedEvent, int depth) : base(transportProxy, true)
        {
            _control = control;
            _orderedEvent = orderedEvent;
            _innerBatch = null;
            _depth = depth;
        }

        public ReceiveBatch(IBTTransportProxy transportProxy, ControlledTermination control, ReceiveBatchCompleteHandler callback, int depth) : base(transportProxy, true)
        {
            _control = control;

            if( callback != null )
            {
                ReceiveBatchComplete += callback;
            }

            _innerBatch = null;
            _depth = depth;
        }

        protected override void StartProcessFailures ()
        {
            // Keep a recusive batch depth so we stop trying at some point.
            if (!OverallSuccess && _depth > 0)
            {
                //  we don't at this point care about ordering with respect to failures
                if (_orderedEvent != null)
                {
                    _innerBatch =
                        new ReceiveBatch(TransportProxy, _control, _orderedEvent, _depth - 1)
                        {
                            ReceiveBatchComplete = ReceiveBatchComplete
                        };
                }
                else
                {
                    _innerBatch = new ReceiveBatch(TransportProxy, _control, ReceiveBatchComplete, _depth - 1);
                }
                _innerBatchCount = 0;
            }
        }
        protected override void EndProcessFailures ()
        {
            if (_innerBatch != null && _innerBatchCount > 0)
            {
                try
                {
                    _innerBatch.Done(null);
                    _needToLeave = false;
                }
                catch (Exception e)
                {
                    Trace.WriteLine("ReceiveBatch.EndProcessFailures Exception: {0}", e.Message);
                    _innerBatch = null;
                }
            }
        }
        protected override void EndBatchComplete ()
        {
            if (_needToLeave)
                _control.Leave();

            //  if there is no pending work and we have been given an event to set then set it!
            if (_innerBatch == null)
            {
                // Theoretically, suspend should never fail unless DB is down/not-reachable
                // or the stream is not seekable. In such cases, there is a chance of duplicates
                // but that's safer than deleting messages that are not in the DB.
                ReceiveBatchComplete?.Invoke(OverallSuccess && !_suspendFailed);

                _orderedEvent?.Set();
            }
        }

        protected override void SubmitFailure (IBaseMessage message, Int32 hrStatus, object userData)
        {
            _failedMessages.Add(new FailedMessage(message, hrStatus));
            Stream originalStream = message.BodyPart.GetOriginalDataStream();

            if (_innerBatch != null)
            {
                try
                {
                    originalStream.Seek(0, SeekOrigin.Begin);
                    message.BodyPart.Data = originalStream;
                    _innerBatch.MoveToSuspendQ(message, userData);
                    _innerBatchCount++;
                }
                catch (Exception e)
                {
                    Trace.WriteLine("ReceiveBatch.SubmitFailure Exception: {0}", e.Message);
                    _innerBatch = null;
                }
            }
        }
        protected override void SubmitSuccess (IBaseMessage message, Int32 hrStatus, object userData)
        {
            Stream originalStream = message.BodyPart.GetOriginalDataStream();

            if (_innerBatch != null)
            {
                _failedMessages.Add(new FailedMessage(message, hrStatus));
                // this good message was caught up with some bad ones - it needs to be submitted again
                try
                {
                    originalStream.Seek(0, SeekOrigin.Begin);
                    message.BodyPart.Data = originalStream;
                    _innerBatch.SubmitMessage(message, userData);
                    _innerBatchCount++;
                }
                catch(Exception e)
                {
                    Trace.WriteLine("ReceiveBatch.SubmitSuccess Exception: {0}", e.Message);
                    _innerBatch = null;
                }
            }
            else
            {
                originalStream.Close();
            }
        }

        protected override void SubmitRequestFailure(IBaseMessage message, int hrStatus, object userData)
        {
            _failedMessages.Add(new FailedMessage(message, hrStatus));
            Stream originalStream = message.BodyPart.GetOriginalDataStream();

            if (_innerBatch != null)
            {
                try
                {
                    originalStream.Seek(0, SeekOrigin.Begin);
                    message.BodyPart.Data = originalStream;
                    _innerBatch.MoveToSuspendQ(message, userData);
                    _innerBatchCount++;
                }
                catch (Exception e)
                {
                    Trace.WriteLine("ReceiveBatch.SubmitFailure Exception: {0}", e.Message);
                    _innerBatch = null;
                }
            }
        }

        protected override void SubmitRequestSuccess(IBaseMessage message, int hrStatus, object userData)
        {
            Stream originalStream = message.BodyPart.GetOriginalDataStream();

            if (_innerBatch != null)
            {
                _failedMessages.Add(new FailedMessage(message, hrStatus));
                try
                {
                    originalStream.Seek(0, SeekOrigin.Begin);
                    message.BodyPart.Data = originalStream;
                    _innerBatch.SubmitMessage(message, userData);
                    _innerBatchCount++;
                }
                catch (Exception e)
                {
                    Trace.WriteLine("ReceiveBatch.SubmitSuccess Exception: {0}", e.Message);
                    _innerBatch = null;
                }
            }
            else
            {
                originalStream.Close();
            }
        }

        protected override void MoveToSuspendQFailure (IBaseMessage message, Int32 hrStatus, object userData)
        {
            _suspendFailed = true;

            Stream originalStream = message.BodyPart.GetOriginalDataStream();
            originalStream.Close();
        }

        protected override void MoveToSuspendQSuccess (IBaseMessage message, Int32 hrStatus, object userData)
        {
            Stream originalStream = message.BodyPart.GetOriginalDataStream();

            //  We may not be done: so if we have successful suspends from last time then suspend them again
            if (_innerBatch != null)
            {
                try
                {
                    originalStream.Seek(0, SeekOrigin.Begin);
                    message.BodyPart.Data = originalStream;
                    _innerBatch.MoveToSuspendQ(message, userData);
                    _innerBatchCount++;
                }
                catch (Exception e)
                {
                    Trace.WriteLine("ReceiveBatch.MoveToSuspendQSuccess Exception: {0}", e.Message);
                    _innerBatch = null;
                }
            }
            else
            {
                originalStream.Close();
            }
        }
        private bool _needToLeave = true;
        private readonly ControlledTermination _control;
        private ReceiveBatch _innerBatch;
        private int _innerBatchCount;
        private readonly ManualResetEvent _orderedEvent;
        private readonly int _depth;
        private bool _suspendFailed;

        private IList<FailedMessage> _failedMessages = new List<FailedMessage>();

        public IList<FailedMessage> FailedMessages
        {
            get { return _failedMessages; }
            set { _failedMessages = value; }
        }

        public event ReceiveBatchCompleteHandler ReceiveBatchComplete;
    }

    public class FailedMessage
    {
        public IBaseMessage Message { get; set; }

        public int Status { get; set; }

        public FailedMessage(IBaseMessage message, int status)
        {
            Message = message;
            Status = status;
        }
    }
}
