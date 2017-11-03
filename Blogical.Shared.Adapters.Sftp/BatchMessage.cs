using System;

using System.Runtime.InteropServices;
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
        private IBaseMessage _message;
		private object _userData;
		private string _correlationToken;
		private BatchOperationType _operationType;
        private SftpReceiveProperties.AfterGetActions _aftergetaction;
        private string _aftergetfilename;
        #endregion
        #region Internal Members
        internal IBaseMessage Message
        {
            get { return _message; }
        }
        internal object UserData
        {
            get { return _userData; }
        }
        internal string CorrelationToken
        {
            get { return _correlationToken; }
        }
        internal BatchOperationType OperationType
        {
            get { return _operationType; }
        }
        internal SftpReceiveProperties.AfterGetActions AfterGetAction
        {
            get { return _aftergetaction; }
        }
        internal string AfterGetFilename
        {
            get { return _aftergetfilename; }
        }
        #endregion
        #region Constructors
        internal BatchMessage(IBaseMessage message, object userData, BatchOperationType oppType)
		{
			_message = message;
			_userData = userData;
			_operationType = oppType;
		}
        internal BatchMessage(IBaseMessage message, object userData, BatchOperationType oppType, 
            SftpReceiveProperties.AfterGetActions afterGetAction, string afterGetFilename)
        {
            _message = message;
            _userData = userData;
            _operationType = oppType;
            _aftergetaction = afterGetAction;
            _aftergetfilename = afterGetFilename;
        }
        internal BatchMessage(string correlationToken, object userData, BatchOperationType oppType)
		{
			_correlationToken = correlationToken;
			_userData = userData;
			_operationType = oppType;
        }
        #endregion      
    }
}
