using System;
using System.IO;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace Blogical.Shared.Adapters.Common
{
    /// <summary>
    /// This class encapsulates the typical behavior we want in a Transmit Adapter running asynchronously.
    /// In summary our policy is:
    /// (1) on a resubmit failure Move to the next transport
    /// (2) on a move to next transport failure move to the suspend queue
    /// Otherwise:
    /// TODO: we should use SetErrorInfo on the transportProxy to log the error appropriately 
    /// </summary>
    public sealed class TransmitResponseBatch : Batch
    {
        public delegate void AllWorkDoneDelegate();

        private AllWorkDoneDelegate _allWorkDoneDelegate;

        public TransmitResponseBatch(IBTTransportProxy transportProxy, AllWorkDoneDelegate allWorkDoneDelegate)
            : base(transportProxy, true)
        {
            _allWorkDoneDelegate = allWorkDoneDelegate;
        }

        public override void SubmitResponseMessage(IBaseMessage solicitDocSent, IBaseMessage responseDocToSubmit)
        {
            IBaseMessagePart bodyPart = responseDocToSubmit.BodyPart;
            if (bodyPart == null)
                throw new InvalidOperationException("The message does not contain body part");

            Stream stream = bodyPart.GetOriginalDataStream();

            if (stream == null || stream.CanSeek == false)
                throw new InvalidOperationException("Message body stream is null or it is not seekable");
            
            base.SubmitResponseMessage(solicitDocSent, responseDocToSubmit, solicitDocSent);
        }

        // This method is typically used during process shutdown
        public void Resubmit(IBaseMessage[] msgs, DateTime timeStamp)
        {
            foreach (IBaseMessage message in msgs)
                base.Resubmit(message, timeStamp);
        }

        public void Resubmit(IBaseMessage msg, bool preserveRetryCount, object userData)
        {
            SystemMessageContext context = new SystemMessageContext(msg.Context);

            if (preserveRetryCount)
            {
                UpdateProperty[] updates =
                {
                    new UpdateProperty
                    {
                        Name = RetryCountProp.Name.Name,
                        NameSpace = RetryCountProp.Name.Namespace,
                        Value = context.RetryCount++
                    }
                };

                context.UpdateProperties(updates);

                // If preserveRetryCount is true, ignore RetryInterval
                // Request the redelivery immediately!!
                base.Resubmit(msg, DateTime.Now, userData);
            }
            else
            {
                // This is retry in case of error/failure (i.e. normal retry)
                if (context.RetryCount > 0)
                {
                    DateTime retryAt = DateTime.Now.AddMinutes(context.RetryInterval);
                    base.Resubmit(msg, retryAt, userData);
                }
                else
                {
                    MoveToNextTransport(msg, userData);
                }
            }
        }

        protected override void StartBatchComplete(int hrBatchComplete)
        {
            _batchFailed = (HrStatus < 0);
        }

        protected override void StartProcessFailures()
        {
            if (_batchFailed)
            {
                // Retry should happen outside the transaction scope
                _batch = new TransmitResponseBatch(TransportProxy, _allWorkDoneDelegate);
                _allWorkDoneDelegate = null;
            }
        }

        protected override void EndProcessFailures()
        {
            if (_batch != null)
            {
                if (!_batch.IsEmpty)
                {
                    _batch.Done(null);
                }
                else
                {
                    // If suspend or delete fails, then there is nothing adapter can do!
                    _batch.Dispose();
                }
            }

        }

        protected override void EndBatchComplete()
        {
            _allWorkDoneDelegate?.Invoke();
        }

        // This is for submit-response
        protected override void SubmitSuccess(IBaseMessage message, Int32 hrStatus, object userData)
        {
            if (_batchFailed)
            {
                // Previous submit operation might have moved the stream position
                // Seek the stream position back to zero before submitting again!
                IBaseMessage solicit = userData as IBaseMessage;
                if (solicit == null)
                    throw new InvalidOperationException("Response message does not have corresponding request message");

                IBaseMessagePart responseBodyPart = message.BodyPart;

                if (responseBodyPart != null )
                {
                    Stream stream = responseBodyPart.GetOriginalDataStream();
                    stream.Position = 0;
                }
                _batch.SubmitResponseMessage(solicit, message);
            }
        }

        protected override void SubmitFailure(IBaseMessage message, Int32 hrStatus, object userData)
        {
            // If response cannot be submitted, then Resubmit the original message?
            // this.batch.Resubmit(message, false, null);
            _batch.MoveToSuspendQ(message);
        }

        protected override void DeleteSuccess(IBaseMessage message, Int32 hrStatus, object userData)
        {
            if (_batchFailed)
            {
                _batch.DeleteMessage(message);
            }
        }

        // No action required when delete fails!

        protected override void ResubmitSuccess(IBaseMessage message, Int32 hrStatus, object userData)
        {
            if (_batchFailed)
            {
                SystemMessageContext context = new SystemMessageContext(message.Context);
                DateTime dt = DateTime.Now.AddMinutes(context.RetryInterval);
                _batch.Resubmit(message, dt);
            }
        }

        protected override void ResubmitFailure(IBaseMessage message, Int32 hrStatus, object userData)
        {
            _batch.MoveToNextTransport(message);
        }

        protected override void MoveToNextTransportSuccess(IBaseMessage message, Int32 hrStatus, object userData)
        {
            if (_batchFailed)
            {
                _batch.MoveToNextTransport(message);
            }
        }

        protected override void MoveToNextTransportFailure(IBaseMessage message, Int32 hrStatus, object userData)
        {
            _batch.MoveToSuspendQ(message);
        }

        protected override void MoveToSuspendQSuccess(IBaseMessage message, Int32 hrStatus, object userData)
        {
            if (_batchFailed)
            {
                _batch.MoveToSuspendQ(message);
            }
        }

        // Nothing can be done if suspend fails

        private TransmitResponseBatch _batch;
        private bool _batchFailed;
        private static readonly BTS.RetryCount RetryCountProp = new BTS.RetryCount();
    }
}
