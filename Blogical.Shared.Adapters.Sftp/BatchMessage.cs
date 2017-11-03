using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace Blogical.Shared.Adapters.Sftp
{
    /// <summary>
    /// A BatchMessage represents a file within a batch. Each BatchMessage is added to the BatchHandler.Files
    /// in the BatchHandler.CreateMessage method.
    /// </summary>
	internal class BatchMessage
    {
        #region Private Members

        #endregion
        #region Internal Members
        internal IBaseMessage Message { get; }

        internal object UserData { get; }

        internal string CorrelationToken { get; }

        internal BatchOperationType OperationType { get; }

        internal SftpReceiveProperties.AfterGetActions AfterGetAction { get; }

        internal string AfterGetFilename { get; }

        #endregion
        #region Constructors
        internal BatchMessage(IBaseMessage message, object userData, BatchOperationType oppType)
		{
			Message = message;
			UserData = userData;
			OperationType = oppType;
		}
        internal BatchMessage(IBaseMessage message, object userData, BatchOperationType oppType, 
            SftpReceiveProperties.AfterGetActions afterGetAction, string afterGetFilename)
        {
            Message = message;
            UserData = userData;
            OperationType = oppType;
            AfterGetAction = afterGetAction;
            AfterGetFilename = afterGetFilename;
        }
        internal BatchMessage(string correlationToken, object userData, BatchOperationType oppType)
		{
			CorrelationToken = correlationToken;
			UserData = userData;
			OperationType = oppType;
        }
        #endregion      
    }
}
