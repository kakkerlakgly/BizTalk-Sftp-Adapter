using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.BizTalk.TransportProxy.Interop;

using Blogical.Shared.Adapters.Common;
using Microsoft.BizTalk.Message.Interop;
using System.Diagnostics;
using System.IO;
using Microsoft.BizTalk.Streaming;

namespace Blogical.Shared.Adapters.Sftp
{
    /// <summary>
    /// The BatchHandler is only used by the SftpReceiverEndpoint for preparing and submitting the batch to BizTalk
    /// </summary>
    internal class BatchHandler
    {
        #region Delegates
        public delegate void BatchHandlerDelegate(ISftp sftp);
        #endregion
        #region Constants
        private const string MessageBody = "body";
        private const string Remotefilename = "FileName";
        private const string Emptybatchfilename = "EmptyBatch.xml";
        #endregion
        #region Private Fields

        private readonly ISftp _sftp;
        private readonly string _transportType;
        private readonly string _propertyNamespace;
        private readonly bool _useLoadBalancing;
        private readonly IBTTransportProxy _transportProxy;
        private IList _filesInProcess;
        private readonly bool _traceFlag;

        #endregion
        #region Constructor
        internal BatchHandler(ISftp sftp, string propertyNamespace, string transportType, IBTTransportProxy transportProxy, bool traceFlag, bool useLoadBalancing)
        {
            _sftp = sftp;
            _propertyNamespace = propertyNamespace;
            _transportType = transportType;
            _transportProxy = transportProxy;
            _traceFlag = traceFlag;
            _useLoadBalancing = useLoadBalancing;
        }
        #endregion
        #region Public Members
        public IList<BatchMessage> Files { get; } = new List<BatchMessage>();
        #endregion
        #region Public Methods
        internal void SubmitFiles(ControlledTermination control, IList filesInProcess)
        {
            if (Files == null || Files.Count == 0)
                return;

            _filesInProcess = filesInProcess;

            try
            {
                using (SyncReceiveSubmitBatch batch = new SyncReceiveSubmitBatch(_transportProxy, control, Files.Count))
                {
                    foreach (BatchMessage file in Files)
                    {
                        batch.SubmitMessage(file.Message, file.UserData);
                    }
                    batch.Done();

                    TraceMessage("[SftpReceiverEndpoint] SubmitFiles (firstAttempt) about to wait on BatchComplete");
                    if (batch.Wait())
                    {
                        TraceMessage("[SftpReceiverEndpoint] SubmitFiles (firstAttempt) overall success");
                        OnBatchComplete(this, new StatusEventArgs {OverallStatus = true});
                    }
                }
                TraceMessage("[SftpReceiverEndpoint] Leaving SubmitFiles");
            }
            catch (Exception ex)
            {
                throw ExceptionHandling.HandleComponentException(
                    EventLogEventIDs.UnableToSubmitBizTalkMessage,
                    System.Reflection.MethodBase.GetCurrentMethod(), ex);
            }
        }
        /// <summary>
        /// Submitting a batch of files to BizTalk
        /// </summary>
        /// <param name="control"></param>
        /// <param name="filesInProcess"></param>
        internal void _SubmitFiles(ControlledTermination control, IList filesInProcess)
        {
            try
            {
                if (Files == null || Files.Count == 0)
                    return;

                _filesInProcess = filesInProcess;

                TraceMessage(string.Format("[SftpReceiverEndpoint] SubmitFiles called. Submitting a batch of {0} files to BizTalk.", Files.Count));

                // This class is used to track the files associated with this ReceiveBatch. The
                // OnBatchComplete will be raised when BizTalk has consumed the message.
                using (ReceiveBatch batch = new ReceiveBatch(_transportProxy, control, OnBatchComplete, Files.Count))
                {
                    foreach (BatchMessage file in Files)
                    {
                        // Submit file to batch
                        batch.SubmitMessage(file.Message, file.UserData);

                    }
                    batch.Done(null);
                }
            }
            catch (Exception e)
            {
                throw ExceptionHandling.HandleComponentException(System.Reflection.MethodBase.GetCurrentMethod(),
                    new SftpException("Could not submit files to BTS", e));
            }
        }

        /// <summary>
        /// (1) Gets the file from the sftp host
        /// (2) Creates a IBaseMessage
        /// (3) Sets varius properties such as uri, messagepart, transporttype etc
        /// (4) Adds the message to the batch
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="uri"></param>
        /// <param name="size"></param>
        /// <param name="afterGetAction"></param>
        /// <param name="afterGetFilename"></param>
        /// <returns></returns>
        internal IBaseMessage CreateMessage(string fileName, string uri, long size,
            SftpReceiveProperties.AfterGetActions afterGetAction, string afterGetFilename)
        {
            try
            {
                TraceMessage("[SftpReceiverEndpoint] Reading file to stream " + fileName);

                // Retrieves the message from sftp server.
                var stream = _sftp.Get(fileName);
                stream.Position = 0;


                // Creates new message
                IBaseMessageFactory messageFactory = _transportProxy.GetMessageFactory();
                IBaseMessagePart part = messageFactory.CreateMessagePart();
                part.Data = stream;
                var message = messageFactory.CreateMessage();
                message.AddPart(MessageBody, part, true);

                // Setting metadata
                SystemMessageContext context =
                    new SystemMessageContext(message.Context)
                    {
                        InboundTransportLocation = uri,
                        InboundTransportType = _transportType
                    };

                // Write/Promote any adapter specific properties on the message context
                message.Context.Write(Remotefilename, _propertyNamespace, fileName);

                SetReceivedFileName(message, fileName);

                message.Context.Write("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/" +
                    _transportType.ToLower() + "-properties", fileName);

                message.Context.Write("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/file-properties", fileName);

                // Add the file to the batch
                Files.Add(new BatchMessage(message, fileName, BatchOperationType.Submit, afterGetAction, afterGetFilename));

                // Greg Sharp: Let the caller set this as the file size may be stale
                // Add the size of the file to the stream
                //if (message.BodyPart.Data.CanWrite)
                //    message.BodyPart.Data.SetLength(size);

                return message;
            }
            catch (Exception ex)
            {
                TraceMessage("[SftpReceiverEndpoint] Error Adding file [" + fileName + "]to batch. Error: " + ex.Message);

                if (_useLoadBalancing)
                    DataBaseHelper.CheckInFile(uri, Path.GetFileName(fileName), _traceFlag);

                return null;
            }
        }

        /// <summary>
        /// Creates a new message with some notification description, 
        /// and adds it to the BatchMessage
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal IBaseMessage CreateEmptyBatchMessage(string uri)
        {
            try
            {

                //string errorMessageFormat = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Error message=\"Empty Batch\" datetime=\"{0}\" source=\"{1}\"/>";
                string errorMessageFormat = "<bLogical:EmptyBatch message=\"Empty Batch\" datetime=\"{0}\" source=\"{1}\" xmlns:bLogical=\"http://Blogical.Shared.Adapters.Sftp.Schemas.EmptyBatch\" />";
                string errorMessage = String.Format(errorMessageFormat, DateTime.Now.ToString(), uri);

                UTF8Encoding utf8Encoding = new UTF8Encoding();
                byte[] messageBuffer = utf8Encoding.GetBytes(errorMessage);

                MemoryStream ms = new MemoryStream(messageBuffer.Length);
                ms.Write(messageBuffer, 0, messageBuffer.Length);
                ms.Position = 0;

                ReadOnlySeekableStream ross = new ReadOnlySeekableStream(ms);

                IBaseMessageFactory messageFactory = _transportProxy.GetMessageFactory();
                IBaseMessagePart part = messageFactory.CreateMessagePart();
                part.Data = ross;
                var message = messageFactory.CreateMessage();
                message.AddPart(MessageBody, part, true);

                SystemMessageContext context =
                    new SystemMessageContext(message.Context)
                    {
                        InboundTransportLocation = uri,
                        InboundTransportType = _transportType
                    };

                //Write/Promote any adapter specific properties on the message context
                message.Context.Write(Remotefilename, _propertyNamespace, Emptybatchfilename);

                // Add the file to the batch
                Files.Add(new BatchMessage(message, Emptybatchfilename, BatchOperationType.Submit));

                // Add the size of the file to the stream
                message.BodyPart.Data.SetLength(ms.Length);
                ms.Close();
                return message;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion
        #region Private Methods

        private void SetReceivedFileName(IBaseMessage pInMsg, string receivedFilename)
        {
            SystemMessageContext messageContext = new SystemMessageContext(pInMsg.Context);

            pInMsg.Context.Write("ReceivedFileName",
                Constants.BiztalkFilePropertiesNamespace, receivedFilename);

            pInMsg.Context.Write("ReceivedFileName",
                Constants.SftpAdapterPropertiesNamespace, receivedFilename);

            pInMsg.Context.Write("ReceivedFileName",
                "http://schemas.microsoft.com/BizTalk/2003/" +
                    messageContext.InboundTransportType.ToLower() + "-properties",
                receivedFilename);
        }
        private void TraceMessage(string message)
        {
            if (_traceFlag)
                Trace.WriteLine(message);
        }
        #endregion
        #region Events

        /// <summary>
        /// Called when the BizTalk Batch has been submitted.  If all the messages were submitted (good or suspended)
        /// we delete the files from the folder
        /// </summary>
        internal void OnBatchComplete(object sender, StatusEventArgs e)
        {
            string fileName = "Could not get fileName";
            try
            {
                if (e.OverallStatus) //Batch completed
                {
                    lock (_filesInProcess)
                    {

                        //Delete the files
                        foreach (BatchMessage batchMessage in Files)
                        {
                            try
                            {
                                //Close the stream so we can delete this file
                                batchMessage.Message.BodyPart.Data.Close();
                                fileName = batchMessage.UserData.ToString();

                                // Delete orginal file  
                                if (fileName != Emptybatchfilename)
                                {
                                    if (batchMessage.AfterGetAction == SftpReceiveProperties.AfterGetActions.Delete)
                                        _sftp.Delete(fileName);
                                    // Greg Killins 2010/06/07 - originally the following line was simply an "else" and
                                    // and assumed the AfterGetAction would be "Rename".
                                    // I added the explicit check to see if it is "Rename" because now there is the
                                    // the third valid option of "DoNothing" as the AfterGetAction.
                                    else if (batchMessage.AfterGetAction == SftpReceiveProperties.AfterGetActions.Rename)
                                    {
                                        string renameFileName = CommonFunctions.CombinePath(Path.GetDirectoryName(fileName), batchMessage.AfterGetFilename);
                                        renameFileName = renameFileName.Replace("%SourceFileName%", Path.GetFileName(fileName));
                                        /* John C. Vestal 2010/04/07 - Added DateTime and UniversalDateTime to macro list. */
                                        if (renameFileName.IndexOf("%DateTime%") > -1)
                                        {
                                            string dateTime = DateTime.Now.ToString();
                                            dateTime = dateTime.Replace("/", "-");
                                            dateTime = dateTime.Replace(":", "");
                                            renameFileName = renameFileName.Replace("%DateTime%", dateTime);
                                        }
                                        if (renameFileName.IndexOf("%UniversalDateTime%") > -1)
                                        {
                                            string dateTime = DateTime.Now.ToUniversalTime().ToString();
                                            dateTime = dateTime.Replace("/", "-");
                                            dateTime = dateTime.Replace(":", "");
                                            renameFileName = renameFileName.Replace("%UniversalDateTime%", dateTime);   
                                        }
                                        // Peter Lindgren 2014-05-22: Added datetime macro that works exactly as the corresponding macro in the standard FTP and FILE adapters.
                                        if (renameFileName.IndexOf("%datetime%") > -1)
                                        {
                                            string dateTime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHHmmss");
                                            renameFileName = renameFileName.Replace("%datetime%", dateTime);
                                        }

                                        _sftp.Rename(fileName, renameFileName);
                                    }
                                }

                                // Remove filename from _filesInProcess
                                _filesInProcess.Remove(fileName);

                                if (_useLoadBalancing)
                                {
                                    string uri = batchMessage.Message.Context.Read(Constants.BizTalkSystemPropertyNames.Inboundtransportlocation, Constants.BiztalkSystemPropertiesNamespace).ToString();
                                    DataBaseHelper.CheckInFile(uri, Path.GetFileName(fileName), _traceFlag);
                                }
                            }
                            catch (Exception ex)
                            {
                                TraceMessage(string.Format("[SftpReceiverEndpoint] ERROR: Could not remove {0} from its location.", ex.Message));
                            }
                        }
                    }


                    TraceMessage(string.Format("[SftpReceiverEndpoint] OnBatchComplete called. overallStatus == {0}.", true));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[SftpReceiverEndpoint] OnBatchComplete EXCEPTION!");
                _filesInProcess.Remove(fileName);
                throw ExceptionHandling.HandleComponentException(System.Reflection.MethodBase.GetCurrentMethod(), ex);
            }
        }
        #endregion
    }
}
