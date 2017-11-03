//---------------------------------------------------------------------
// File: TxnBatch.cs
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

using System.Transactions;
using System.Threading;
using Microsoft.BizTalk.TransportProxy.Interop;

namespace Blogical.Shared.Adapters.Common
{
	public class TxnBatch : Batch
	{
        public TxnBatch(IBTTransportProxy transportProxy, ControlledTermination control, CommittableTransaction transaction, ManualResetEvent orderedEvent, bool makeSuccessCall) : base(transportProxy, makeSuccessCall)
        {
			this.Control = control;

            ComTxn = TransactionInterop.GetDtcTransaction(transaction);

            //  the System.Transactions transaction - must be the original transaction - only that can be used to commit
            this.Transaction = transaction;

			this.OrderedEvent = orderedEvent;
		}
        public TxnBatch(IBTTransportProxy transportProxy, ControlledTermination control, IDtcTransaction comTxn, CommittableTransaction transaction, ManualResetEvent orderedEvent, bool makeSuccessCall) : base(transportProxy, makeSuccessCall)
        {
            this.Control = control;
            this.ComTxn = comTxn;
            this.Transaction = transaction;
            this.OrderedEvent = orderedEvent;
        }
        public override void Done ()
		{
            CommitConfirm = base.Done(ComTxn);
		}
		protected override void EndBatchComplete ()
		{
			if (_pendingWork)
			{
				return;
			}
			try
			{
				if (_needToAbort)
				{
                    Transaction.Rollback();

                    CommitConfirm.DTCCommitConfirm(ComTxn, false); 
				}
				else
				{
                    Transaction.Commit();

                    CommitConfirm.DTCCommitConfirm(ComTxn, true); 
				}
			}
			catch
			{
				try
				{
					CommitConfirm.DTCCommitConfirm(ComTxn, false); 
				}
				catch
				{
				}
			}
			//  note the pending work check at the top of this function removes the need to check a needToLeave flag
			Control.Leave();

		    OrderedEvent?.Set();
		}
        protected void SetAbort()
        {
            _needToAbort = true;
        }
        protected void SetComplete()
        {
            _needToAbort = false;
        }
        protected void SetPendingWork()
		{
			_pendingWork = true;
		}
		protected IBTDTCCommitConfirm CommitConfirm
		{
			set
			{
				_commitConfirm = value;
				_commitConfirmEvent.Set();
			}
			get
			{
				_commitConfirmEvent.WaitOne();
				return _commitConfirm;
			}
		}
        protected readonly IDtcTransaction ComTxn;
        protected readonly CommittableTransaction Transaction;
        protected readonly ControlledTermination Control;
	    private IBTDTCCommitConfirm _commitConfirm;
		protected readonly ManualResetEvent OrderedEvent;
		private readonly ManualResetEvent _commitConfirmEvent = new ManualResetEvent(false);
		private bool _needToAbort = true;
		private bool _pendingWork;
	}
}
