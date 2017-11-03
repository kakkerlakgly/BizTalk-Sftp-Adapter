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
        class BatchMessage
        {
            public IBaseMessage message;
            public object userData;

            public string correlationToken;

            public BatchMessage (IBaseMessage message, object userData)
            {
                this.message = message;
                this.userData = userData;
            }
            public BatchMessage (string correlationToken, object userData)
            {
                this.correlationToken = correlationToken;
                this.userData = userData;
            }
        }

        public virtual bool OverallSuccess
        {
            get { return (hrStatus >= 0); }    
        }

        public Int32 HRStatus
        {
            get { return hrStatus; }
        }


        // IBTBatchCallBack
        public void BatchComplete (Int32 hrStatus, Int16 nOpCount, BTBatchOperationStatus[] pOperationStatus, Object vCallbackCookie)
        {
            //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.SegmentLifeTime, "CoreAdapter: Entering Batch.BatchComplete");
    
            if (0 != hrStatus)
            {
                //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.Error, "CoreAdapter: Batch.BatchComplete hrStatus: {0}", hrStatus);
            }

            this.hrStatus = hrStatus;

            StartBatchComplete(hrStatus);

            //  nothing at all failed in this batch so we are done
            if (hrStatus < 0 || makeSuccessCall)
            {
                //BT.Trace.Tracer.TraceMessage(BT.TraceLevel.Info, "CoreAdapter: Batch.BatchComplete (hrStatus < 0 || this.makeSuccessCall)");

                StartProcessFailures();

                foreach (BTBatchOperationStatus status in pOperationStatus)
                {
                    //  is this the correct behavior?
                    if (status.Status >= 0 && !makeSuccessCall)
                        continue;

                    switch (status.OperationType)
                    {
                        case BatchOperationType.Submit:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = null;
                                if (submitIsForSubmitResponse)
                                {
                                    batchMessage = (BatchMessage)submitResponseMessageArray[i];
                                }
                                else
                                {
                                    batchMessage = (BatchMessage)submitArray[i];
                                }

                                if (status.MessageStatus[i] < 0)
                                    SubmitFailure(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                                else if (makeSuccessCall)
                                    SubmitSuccess(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                            }
                            break;
                        }
                        case BatchOperationType.Delete:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = (BatchMessage)deleteArray[i];
                                if (status.MessageStatus[i] < 0)
                                    DeleteFailure(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                                else if (makeSuccessCall)
                                    DeleteSuccess(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                            }
                            break;
                        }
                        case BatchOperationType.Resubmit:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = (BatchMessage)resubmitArray[i];
                                if (status.MessageStatus[i] < 0)
                                    ResubmitFailure(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                                else if (makeSuccessCall)
                                    ResubmitSuccess(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                            }
                            break;
                        }
                        case BatchOperationType.MoveToSuspendQ:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = (BatchMessage)moveToSuspendQArray[i];
                                if (status.MessageStatus[i] < 0)
                                    MoveToSuspendQFailure(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                                else if (makeSuccessCall)
                                    MoveToSuspendQSuccess(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                            }
                            break;
                        }
                        case BatchOperationType.MoveToNextTransport:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = (BatchMessage)moveToNextTransportArray[i];
                                if (status.MessageStatus[i] < 0)
                                    MoveToNextTransportFailure(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                                else if (makeSuccessCall)
                                    MoveToNextTransportSuccess(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                            }
                            break;
                        }
                        case BatchOperationType.SubmitRequest:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = (BatchMessage)submitRequestArray[i];
                                if (status.MessageStatus[i] < 0)
                                    SubmitRequestFailure(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                                else if (makeSuccessCall)
                                    SubmitRequestSuccess(batchMessage.message, status.MessageStatus[i], batchMessage.userData);
                            }
                            break;
                        }
                        case BatchOperationType.CancelRequestForResponse:
                        {
                            for (int i=0; i<status.MessageCount; i++)
                            {
                                BatchMessage batchMessage = (BatchMessage)cancelResponseMessageArray[i];
                                if (status.MessageStatus[i] < 0)
                                    CancelResponseMessageFailure(batchMessage.correlationToken, status.MessageStatus[i], batchMessage.userData);
                                else if (makeSuccessCall)
                                    CancelResponseMessageSuccess(batchMessage.correlationToken, status.MessageStatus[i], batchMessage.userData);
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
            hrStatus = -1;
            this.transportProxy = transportProxy;
            transportBatch = this.transportProxy.GetBatch(this, null);
            this.makeSuccessCall = makeSuccessCall;
        }

        public void SubmitMessage (IBaseMessage message, object userData)
        {
            if( submitResponseMessageArray != null )
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

            transportBatch.SubmitMessage(message);
            if (null == submitArray)
                submitArray = new List<BatchMessage>();
            submitArray.Add(new BatchMessage(message, userData));

            workToBeDone = true;
        }

        public void DeleteMessage (IBaseMessage message, object userData)                   
        { 
            transportBatch.DeleteMessage(message);
            if (null == deleteArray)
                deleteArray = new List<BatchMessage>();
            deleteArray.Add(new BatchMessage(message, userData));

            workToBeDone = true;
        }

        public void Resubmit (IBaseMessage message, DateTime t, object userData)                
        {
            transportBatch.Resubmit(message, t);
            if (null == resubmitArray)
                resubmitArray = new List<BatchMessage>();
            resubmitArray.Add(new BatchMessage(message, userData));

            workToBeDone = true;
        }

        public void MoveToSuspendQ (IBaseMessage message, object userData)           
        {
            transportBatch.MoveToSuspendQ(message);
            if (null == moveToSuspendQArray)
                moveToSuspendQArray = new List<BatchMessage>();
            moveToSuspendQArray.Add(new BatchMessage(message, userData));

            workToBeDone = true;
        }

        public void MoveToNextTransport (IBaseMessage message, object userData)      
        {
            transportBatch.MoveToNextTransport(message);
            if (null == moveToNextTransportArray)
                moveToNextTransportArray = new List<BatchMessage>();
            moveToNextTransportArray.Add(new BatchMessage(message, userData));

            workToBeDone = true;
        }

        public void SubmitRequestMessage (IBaseMessage requestMsg, string correlationToken, bool firstResponseOnly, DateTime expirationTime, IBTTransmitter responseCallback, object userData)            
        {
            transportBatch.SubmitRequestMessage(requestMsg, correlationToken, firstResponseOnly, expirationTime, responseCallback);
            if (null == submitRequestArray)
                submitRequestArray = new List<BatchMessage>();
            submitRequestArray.Add(new BatchMessage(requestMsg, userData));

            workToBeDone = true;
        }

        public void CancelResponseMessage (string correlationToken, object userData) 
        {
            transportBatch.CancelResponseMessage(correlationToken);
            if (null == cancelResponseMessageArray)
                cancelResponseMessageArray = new List<BatchMessage>();
            cancelResponseMessageArray.Add(new BatchMessage(correlationToken, userData));

            workToBeDone = true;
        }

        public void SubmitResponseMessage (IBaseMessage solicitDocSent, IBaseMessage responseDocToSubmit, object userData) 
        {
            if (submitArray != null)
                throw new InvalidOperationException("SubmitResponseMessage and SubmitMessage operations cannot be in the same batch");

            transportBatch.SubmitResponseMessage(solicitDocSent, responseDocToSubmit);
            if (null == submitResponseMessageArray)
                submitResponseMessageArray = new List<BatchMessage>();
            submitResponseMessageArray.Add(new BatchMessage(responseDocToSubmit, userData));

            workToBeDone = true;
            submitIsForSubmitResponse = true;
        }

        public void SubmitMessage (IBaseMessage message)
        {
            SubmitMessage(message, null);
        }
        public void DeleteMessage (IBaseMessage message)                  
        {
            DeleteMessage(message, null);
        }
        public void Resubmit (IBaseMessage message, DateTime t)
        {
            Resubmit(message, t, null);
        }
        public void MoveToSuspendQ (IBaseMessage message)
        {
            MoveToSuspendQ(message, null);
        }
        public void MoveToNextTransport (IBaseMessage message)
        {
            MoveToNextTransport(message, null);
        }
        public void SubmitRequestMessage (IBaseMessage requestMsg, string correlationToken, bool firstResponseOnly, DateTime expirationTime, IBTTransmitter responseCallback)
        {
            SubmitRequestMessage(requestMsg, correlationToken, firstResponseOnly, expirationTime, responseCallback, null);
        }
        public void CancelResponseMessage (string correlationToken)
        {
            CancelResponseMessage(correlationToken, null);
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
                if (workToBeDone)
                {
                    IBTDTCCommitConfirm commitConfirm = transportBatch.Done(transaction);
                    return commitConfirm;
                }
                else
                {
                    // This condition should never occur on the production box 
                    // (unless there is a product bug). So, this string need not be localized
                    Exception ex = new InvalidOperationException("Adapter is trying to submit an empty batch to EPM. Source = " + 
                                    GetType().ToString());
                    transportProxy.SetErrorInfo(ex);
                    throw ex;
                }
            }
            finally
            {
                //  undo cyclical reference through COM
                if (Marshal.IsComObject(transportBatch))
                {
                    Marshal.FinalReleaseComObject(transportBatch);
                    GC.SuppressFinalize(transportBatch);
                    transportBatch = null;
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
            if (transportBatch != null)
            {
                if (Marshal.IsComObject(transportBatch))
                {
                    Marshal.FinalReleaseComObject(transportBatch);
                    GC.SuppressFinalize(transportBatch);
                    transportBatch = null;
                }
            }
        }

        public bool IsEmpty
        {
            get { return !workToBeDone; }
        }

        public IBTTransportProxy TransportProxy { get { return transportProxy; } }
        public IBTTransportBatch TransportBatch { get { return transportBatch; } }

        private Int32 hrStatus;
        private IBTTransportProxy transportProxy;
        private IBTTransportBatch transportBatch;
        private bool makeSuccessCall;

        private IList<BatchMessage> submitArray;
        private IList<BatchMessage> deleteArray;
        private IList<BatchMessage> resubmitArray;
        private IList<BatchMessage> moveToSuspendQArray;
        private IList<BatchMessage> moveToNextTransportArray;
        private IList<BatchMessage> submitRequestArray;
        private IList<BatchMessage> cancelResponseMessageArray;
        private IList<BatchMessage> submitResponseMessageArray;

        private bool workToBeDone = false;
        private bool submitIsForSubmitResponse = false;
    }
}
