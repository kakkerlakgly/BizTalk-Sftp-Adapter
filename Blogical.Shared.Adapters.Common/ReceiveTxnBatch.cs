//---------------------------------------------------------------------
// File: ReceiveTxnBatch.cs
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
using System.Transactions;
using System.Threading;
using System.Diagnostics;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace Blogical.Shared.Adapters.Common
{
	//  Anything fails in this batch we abort the lot!
	public sealed class AbortOnFailureReceiveTxnBatch : TxnBatch
	{
		public delegate void TxnAborted ();

		private readonly TxnAborted _txnAborted;

        public AbortOnFailureReceiveTxnBatch(IBTTransportProxy transportProxy, ControlledTermination control, CommittableTransaction transaction, ManualResetEvent orderedEvent, TxnAborted txnAborted) : base(transportProxy, control, transaction, orderedEvent, false)
        { 
			_txnAborted = txnAborted;
		}
        public AbortOnFailureReceiveTxnBatch(IBTTransportProxy transportProxy, ControlledTermination control, IDtcTransaction comTxn, CommittableTransaction transaction, ManualResetEvent orderedEvent, TxnAborted txnAborted)
            : base(transportProxy, control, comTxn, transaction, orderedEvent, false)
        {
            _txnAborted = txnAborted;
        }
        protected override void StartBatchComplete (int hrBatchComplete)
        {
            if (HrStatus >= 0)
            {
                SetComplete();
            }
        }
        protected override void StartProcessFailures ()
		{
			SetAbort();
		    _txnAborted?.Invoke();
		}
	}

	//  Anything fails in this batch we abort the lot - even the case where we get an "S_FALSE" because the EPM has handled the error
	public sealed class AbortOnAllFailureReceiveTxnBatch : TxnBatch
	{
		public delegate void StopProcessing ();

		private readonly StopProcessing _stopProcessing;

        public AbortOnAllFailureReceiveTxnBatch(IBTTransportProxy transportProxy, ControlledTermination control, CommittableTransaction transaction, ManualResetEvent orderedEvent, StopProcessing stopProcessing) : base(transportProxy, control, transaction, orderedEvent, false)
        {
			_stopProcessing = stopProcessing;
		}
		protected override void StartBatchComplete (int hrBatchComplete)
		{
            if (HrStatus != 0)
            {
                SetAbort();
                _stopProcessing?.Invoke();
            }
            else
            {
                SetComplete();
            }
		}
	}

	//  Submit fails we MoveToSuspendQ
	public sealed class SingleMessageReceiveTxnBatch : TxnBatch
	{
        public SingleMessageReceiveTxnBatch(IBTTransportProxy transportProxy, ControlledTermination control, CommittableTransaction transaction, ManualResetEvent orderedEvent) : base(transportProxy, control, transaction, orderedEvent, true)
        { }
		protected override void StartProcessFailures ()
		{
			if (!OverallSuccess)
			{
				_innerBatch = new AbortOnFailureReceiveTxnBatch(TransportProxy, Control, ComTxn, Transaction, OrderedEvent, null);
				_innerBatchCount = 0;
			}
		}
		protected override void SubmitFailure (IBaseMessage message, Int32 hrStatus, object userData)
		{
			if (_innerBatch != null)
			{
                try
                {
                    Stream originalStream = message.BodyPart.GetOriginalDataStream();
				    originalStream.Seek(0, SeekOrigin.Begin);
				    message.BodyPart.Data = originalStream;
                    _innerBatch.MoveToSuspendQ(message);
					_innerBatchCount++;
				}
				catch (Exception e)
				{
                    Trace.WriteLine("SingleMessageReceiveTxnBatch.SubmitFailure Exception: {0}", e.Message);
					_innerBatch = null;
					SetAbort();
				}
			}
		}
        protected override void SubmitSuccess(IBaseMessage message, Int32 hrStatus, object userData)
        {
            SetComplete();
        }
		protected override void EndProcessFailures ()
		{
			if (_innerBatch != null && _innerBatchCount > 0)
			{
				try
				{
					_innerBatch.Done();
					SetPendingWork();
				}
				catch (Exception)
				{
					SetAbort();
				}
			}
		}
		private Batch _innerBatch;
		private int _innerBatchCount;
	}
}

