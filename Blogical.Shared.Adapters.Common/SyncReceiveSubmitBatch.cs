//---------------------------------------------------------------------
// File: SyncReceiveSubmitBatch.cs
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

using System.Threading;
using Microsoft.BizTalk.TransportProxy.Interop;

namespace Blogical.Shared.Adapters.Common
{
    public class SyncReceiveSubmitBatch : ReceiveBatch
    {
        private readonly ManualResetEvent _workDone;
        private bool _overallSuccess;
        private readonly ControlledTermination _control;

        public SyncReceiveSubmitBatch(IBTTransportProxy transportProxy, ControlledTermination control, int depth)
            : this(transportProxy, control, new ManualResetEvent(false), depth) { }

        private SyncReceiveSubmitBatch(IBTTransportProxy transportProxy, ControlledTermination control,
                                        ManualResetEvent submitComplete, int depth)
            : base(transportProxy, control, submitComplete, depth)
        {
            _control = control;
            _workDone = submitComplete;
            ReceiveBatchComplete += OnBatchComplete;
        }

        private void OnBatchComplete(bool overallSuccess)
        {
            _overallSuccess = overallSuccess;
        }

        public override void Done()
        {
            bool needToLeave = _control.Enter();

            try
            {
                base.Done();
            }
            catch
            {
                if (needToLeave)
                    _control.Leave();

                throw;
            }
        }

        public bool Wait()
        {
            _workDone.WaitOne();

            return _overallSuccess;
        }
    }
}
