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
			this.control = control;

            comTxn = TransactionInterop.GetDtcTransaction(transaction);

            //  the System.Transactions transaction - must be the original transaction - only that can be used to commit
            this.transaction = transaction;

			this.orderedEvent = orderedEvent;
		}
        public TxnBatch(IBTTransportProxy transportProxy, ControlledTermination control, IDtcTransaction comTxn, CommittableTransaction transaction, ManualResetEvent orderedEvent, bool makeSuccessCall) : base(transportProxy, makeSuccessCall)
        {
            this.control = control;
            this.comTxn = comTxn;
            this.transaction = transaction;
            this.orderedEvent = orderedEvent;
        }
        public override void Done ()
		{
            CommitConfirm = base.Done(comTxn);
		}
		protected override void EndBatchComplete ()
		{
			if (pendingWork)
			{
				return;
			}
			try
			{
				if (needToAbort)
				{
                    transaction.Rollback();

                    CommitConfirm.DTCCommitConfirm(comTxn, false); 
				}
				else
				{
                    transaction.Commit();

                    CommitConfirm.DTCCommitConfirm(comTxn, true); 
				}
			}
			catch
			{
				try
				{
					CommitConfirm.DTCCommitConfirm(comTxn, false); 
				}
				catch
				{
				}
			}
			//  note the pending work check at the top of this function removes the need to check a needToLeave flag
			control.Leave();

		    orderedEvent?.Set();
		}
        protected void SetAbort()
        {
            needToAbort = true;
        }
        protected void SetComplete()
        {
            needToAbort = false;
        }
        protected void SetPendingWork()
		{
			pendingWork = true;
		}
		protected IBTDTCCommitConfirm CommitConfirm
		{
			set
			{
				commitConfirm = value;
				commitConfirmEvent.Set();
			}
			get
			{
				commitConfirmEvent.WaitOne();
				return commitConfirm;
			}
		}
        protected IDtcTransaction comTxn;
        protected CommittableTransaction transaction;
        protected ControlledTermination control;
		protected IBTDTCCommitConfirm commitConfirm;
		protected ManualResetEvent orderedEvent;
		private ManualResetEvent commitConfirmEvent = new ManualResetEvent(false);
		private bool needToAbort = true;
		private bool pendingWork;
	}
}
