//---------------------------------------------------------------------
// File: Batch.cs
// 
// Summary: Implementation of an adapter framework sample adapter. 
// This class constitutes one of the BaseAdapter classes, which, are
// a set of generic re-usable set of classes to help adapter writers.
//
// Sample: Base Adapter Class Library v1.0.2
//
// Description: TODO:
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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace Blogical.Shared.Adapters.Common
{
    /// <summary>
    /// This class completely wraps the calls to a transportProxy batch. It does this so it can keep
    /// a trail of the messages that were submitted to that batch. As it has this it can then tie the
    /// async batch callback to a series of virtual function calls - all parameterized with the correct
    /// assocaited message object. Derived classes can then decide what they want to do in each case.
    /// </summary>
    public class Batch : IBTBatchCallBack, IDisposable
    {
        private class BatchMessage
        {
            public readonly IBaseMessage Message;
            public readonly object UserData;

            public readonly string CorrelationToken;

            public BatchMessage (IBaseMessage message, object userData)
            {
                Message = message;
                UserData = userData;
            }
            public BatchMessage (string correlationToken, object userData)
            {
                CorrelationToken = correlationToken;
                UserData = userData;
            }
        }

        public virtual bool OverallSuccess
        {
            get { return (_hrStatus >= 0); }    
        }

        public Int32 HrStatus
        {
            get { return _hrStatus; }
        }


        // IBTBatchCallBack
        public void BatchComplete (Int32 hrStatus, Int16 nOpCount, BTBatchOperationStatus[] pOperationStatus, Object vCallbackCookie)
        {
            //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.SegmentLifeTime, "CoreAdapter: Entering Batch.BatchComplete");
    
            if (0 != hrStatus)
            {
                //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.Error, "CoreAdapter: Batch.BatchComplete hrStatus: {0}", hrStatus);
            }

            _hrStatus = hrStatus;

            StartBatchComplete(hrStatus);

            //  nothing at all failed in this batch so we are done
            if (hrStatus < 0 || _makeSuccessCall)
            {
                //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.Info, "CoreAdapter: Batch.BatchComplete (hrStatus < 0 || this.makeSuccessCall)");

                StartProcessFailures();

                foreach (BTBatchOperationStatus status in pOperationStatus)
                {
                    //  is this the correct behavior?
                    if (status.Status >= 0 && !_makeSuccessCall)
                        continue;

                    switch (status.OperationType)
                    {
                        case BatchOperationType.Submit:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                var batchMessage = _submitIsForSubmitResponse ? _submitResponseMessageArray[i] : _submitArray[i];

                                if (status.MessageStatus[i] < 0)
                                    SubmitFailure(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                                else if (_makeSuccessCall)
                                    SubmitSuccess(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                            }
                            break;
                        }
                        case BatchOperationType.Delete:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = _deleteArray[i];
                                if (status.MessageStatus[i] < 0)
                                    DeleteFailure(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                                else if (_makeSuccessCall)
                                    DeleteSuccess(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                            }
                            break;
                        }
                        case BatchOperationType.Resubmit:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = _resubmitArray[i];
                                if (status.MessageStatus[i] < 0)
                                    ResubmitFailure(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                                else if (_makeSuccessCall)
                                    ResubmitSuccess(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                            }
                            break;
                        }
                        case BatchOperationType.MoveToSuspendQ:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = _moveToSuspendQArray[i];
                                if (status.MessageStatus[i] < 0)
                                    MoveToSuspendQFailure(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                                else if (_makeSuccessCall)
                                    MoveToSuspendQSuccess(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                            }
                            break;
                        }
                        case BatchOperationType.MoveToNextTransport:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = _moveToNextTransportArray[i];
                                if (status.MessageStatus[i] < 0)
                                    MoveToNextTransportFailure(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                                else if (_makeSuccessCall)
                                    MoveToNextTransportSuccess(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                            }
                            break;
                        }
                        case BatchOperationType.SubmitRequest:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = _submitRequestArray[i];
                                if (status.MessageStatus[i] < 0)
                                    SubmitRequestFailure(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                                else if (_makeSuccessCall)
                                    SubmitRequestSuccess(batchMessage.Message, status.MessageStatus[i], batchMessage.UserData);
                            }
                            break;
                        }
                        case BatchOperationType.CancelRequestForResponse:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = _cancelResponseMessageArray[i];
                                if (status.MessageStatus[i] < 0)
                                    CancelResponseMessageFailure(batchMessage.CorrelationToken, status.MessageStatus[i], batchMessage.UserData);
                                else if (_makeSuccessCall)
                                    CancelResponseMessageSuccess(batchMessage.CorrelationToken, status.MessageStatus[i], batchMessage.UserData);
                            }
                            break;
                        }
                    } // end switch
                } // end foreach

                EndProcessFailures();
            } // end if

            EndBatchComplete();

            //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.SegmentLifeTime, "CoreAdapter: Leaving Batch.BatchComplete");
        }

        protected virtual void StartBatchComplete (int hrBatchStatus) { }
        protected virtual void EndBatchComplete () { }

        protected virtual void StartProcessFailures() { }
        protected virtual void EndProcessFailures () { }

        protected virtual void SubmitFailure (IBaseMessage message, Int32 hrStatus, object userData)                      { }
        protected virtual void DeleteFailure (IBaseMessage message, Int32 hrStatus, object userData)                      { }
        protected virtual void ResubmitFailure (IBaseMessage message, Int32 hrStatus, object userData)                    { }
        protected virtual void MoveToSuspendQFailure (IBaseMessage message, Int32 hrStatus, object userData)              { }
        protected virtual void MoveToNextTransportFailure (IBaseMessage message, Int32 hrStatus, object userData)         { }
        protected virtual void SubmitRequestFailure (IBaseMessage message, Int32 hrStatus, object userData)               { }
        protected virtual void CancelResponseMessageFailure (string correlationToken, Int32 hrStatus, object userData) { }

        protected virtual void SubmitSuccess (IBaseMessage message, Int32 hrStatus, object userData)                      { }
        protected virtual void DeleteSuccess (IBaseMessage message, Int32 hrStatus, object userData)                      { }
        protected virtual void ResubmitSuccess (IBaseMessage message, Int32 hrStatus, object userData)                    { }
        protected virtual void MoveToSuspendQSuccess (IBaseMessage message, Int32 hrStatus, object userData)              { }
        protected virtual void MoveToNextTransportSuccess (IBaseMessage message, Int32 hrStatus, object userData)         { }
        protected virtual void SubmitRequestSuccess (IBaseMessage message, Int32 hrStatus, object userData)               { }
        protected virtual void CancelResponseMessageSuccess (string correlationToken, Int32 hrStatus, object userData) { }

        public Batch (IBTTransportProxy transportProxy, bool makeSuccessCall)
        {
            _hrStatus = -1;
            _transportProxy = transportProxy;
            _transportBatch = _transportProxy.GetBatch(this, null);
            _makeSuccessCall = makeSuccessCall;
        }

        public void SubmitMessage (IBaseMessage message, object userData = null)
        {
            if( _submitResponseMessageArray != null )
                throw new InvalidOperationException("SubmitResponseMessage and SubmitMessage operations cannot be in the same batch");

            // We need to have data (body part) to handle batch failures.
            IBaseMessagePart bodyPart = message.BodyPart;
            if (bodyPart == null)
                throw new InvalidOperationException("The message doesn't contain body part");

            Stream stream = bodyPart.GetOriginalDataStream();
            if (stream == null || stream.CanSeek == false)
            {
                throw new InvalidOperationException("Cannot submit empty body or body with non-seekable stream");
            }

            _transportBatch.SubmitMessage(message);
            if (null == _submitArray)
                _submitArray = new List<BatchMessage>();
            _submitArray.Add(new BatchMessage(message, userData));

            _workToBeDone = true;
        }

        public void DeleteMessage (IBaseMessage message, object userData = null)                   
        { 
            _transportBatch.DeleteMessage(message);
            if (null == _deleteArray)
                _deleteArray = new List<BatchMessage>();
            _deleteArray.Add(new BatchMessage(message, userData));

            _workToBeDone = true;
        }

        public void Resubmit (IBaseMessage message, DateTime t, object userData = null)                
        {
            _transportBatch.Resubmit(message, t);
            if (null == _resubmitArray)
                _resubmitArray = new List<BatchMessage>();
            _resubmitArray.Add(new BatchMessage(message, userData));

            _workToBeDone = true;
        }

        public void MoveToSuspendQ (IBaseMessage message, object userData = null)           
        {
            _transportBatch.MoveToSuspendQ(message);
            if (null == _moveToSuspendQArray)
                _moveToSuspendQArray = new List<BatchMessage>();
            _moveToSuspendQArray.Add(new BatchMessage(message, userData));

            _workToBeDone = true;
        }

        public void MoveToNextTransport (IBaseMessage message, object userData = null)      
        {
            _transportBatch.MoveToNextTransport(message);
            if (null == _moveToNextTransportArray)
                _moveToNextTransportArray = new List<BatchMessage>();
            _moveToNextTransportArray.Add(new BatchMessage(message, userData));

            _workToBeDone = true;
        }

        public void SubmitRequestMessage (IBaseMessage requestMsg, string correlationToken, bool firstResponseOnly, DateTime expirationTime, IBTTransmitter responseCallback, object userData = null)            
        {
            _transportBatch.SubmitRequestMessage(requestMsg, correlationToken, firstResponseOnly, expirationTime, responseCallback);
            if (null == _submitRequestArray)
                _submitRequestArray = new List<BatchMessage>();
            _submitRequestArray.Add(new BatchMessage(requestMsg, userData));

            _workToBeDone = true;
        }

        public void CancelResponseMessage (string correlationToken, object userData = null) 
        {
            _transportBatch.CancelResponseMessage(correlationToken);
            if (null == _cancelResponseMessageArray)
                _cancelResponseMessageArray = new List<BatchMessage>();
            _cancelResponseMessageArray.Add(new BatchMessage(correlationToken, userData));

            _workToBeDone = true;
        }

        public void SubmitResponseMessage (IBaseMessage solicitDocSent, IBaseMessage responseDocToSubmit, object userData) 
        {
            if (_submitArray != null)
                throw new InvalidOperationException("SubmitResponseMessage and SubmitMessage operations cannot be in the same batch");

            _transportBatch.SubmitResponseMessage(solicitDocSent, responseDocToSubmit);
            if (null == _submitResponseMessageArray)
                _submitResponseMessageArray = new List<BatchMessage>();
            _submitResponseMessageArray.Add(new BatchMessage(responseDocToSubmit, userData));

            _workToBeDone = true;
            _submitIsForSubmitResponse = true;
        }

        // This implementation passes userData as null. Other implementations could override this to pass
        // more significant information like solicitDocSent
        public virtual void SubmitResponseMessage(IBaseMessage solicitDocSent, IBaseMessage responseDocToSubmit)
        {
            SubmitResponseMessage(solicitDocSent, responseDocToSubmit, null);
        }

        public IBTDTCCommitConfirm Done (object transaction)
        {
            //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.SegmentLifeTime, "CoreAdapter: Entering Batch.Done");

            try
            {
                if (_workToBeDone)
                {
                    IBTDTCCommitConfirm commitConfirm = _transportBatch.Done(transaction);
                    return commitConfirm;
                }
                else
                {
                    // This condition should never occur on the production box 
                    // (unless there is a product bug). So, this string need not be localized
                    Exception ex = new InvalidOperationException("Adapter is trying to submit an empty batch to EPM. Source = " + 
                                    GetType());
                    _transportProxy.SetErrorInfo(ex);
                    throw ex;
                }
            }
            finally
            {
                //  undo cyclical reference through COM
                if (Marshal.IsComObject(_transportBatch))
                {
                    Marshal.FinalReleaseComObject(_transportBatch);
                    GC.SuppressFinalize(_transportBatch);
                    _transportBatch = null;
                }

                //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.SegmentLifeTime, "CoreAdapter: Leaving Batch.Done");
            }
        }

        public virtual void Done ()
        {
            Done(null);
        }

        public virtual void Dispose()
        {
            if (_transportBatch != null)
            {
                if (Marshal.IsComObject(_transportBatch))
                {
                    Marshal.FinalReleaseComObject(_transportBatch);
                    GC.SuppressFinalize(_transportBatch);
                    _transportBatch = null;
                }
            }
        }

        public bool IsEmpty
        {
            get { return !_workToBeDone; }
        }

        public IBTTransportProxy TransportProxy { get { return _transportProxy; } }
        public IBTTransportBatch TransportBatch { get { return _transportBatch; } }

        private Int32 _hrStatus;
        private readonly IBTTransportProxy _transportProxy;
        private IBTTransportBatch _transportBatch;
        private readonly bool _makeSuccessCall;

        private IList<BatchMessage> _submitArray;
        private IList<BatchMessage> _deleteArray;
        private IList<BatchMessage> _resubmitArray;
        private IList<BatchMessage> _moveToSuspendQArray;
        private IList<BatchMessage> _moveToNextTransportArray;
        private IList<BatchMessage> _submitRequestArray;
        private IList<BatchMessage> _cancelResponseMessageArray;
        private IList<BatchMessage> _submitResponseMessageArray;

        private bool _workToBeDone;
        private bool _submitIsForSubmitResponse;
    }
}
