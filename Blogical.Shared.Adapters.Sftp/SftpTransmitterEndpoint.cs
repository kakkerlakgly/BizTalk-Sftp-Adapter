
using System;
using System.IO;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Blogical.Shared.Adapters.Common;
using System.Diagnostics;
using Blogical.Shared.Adapters.Sftp.ConnectionPool;

namespace Blogical.Shared.Adapters.Sftp
{
    /// <summary>
	/// There is one instance of HttpTransmitterEndpoint class for each every static send port.
	/// Messages will be forwarded to this class by AsyncTransmitterBatch
	/// </summary>
	public class SftpTransmitterEndpoint : IAsyncTransmitterEndpoint 
    {
        #region Private Fields
        private bool _shutdownRequested;
        private SftpTransmitProperties _properties;
        private readonly AsyncTransmitter _asyncTransmitter;
        private string _propertyNamespace;
        private int _errorCount; //  error count for comparison with the error threshold
        #endregion
        #region Construktor
        public SftpTransmitterEndpoint(AsyncTransmitter asyncTransmitter)
		{
			_asyncTransmitter = asyncTransmitter;

            Trace.WriteLine("[SftpTransmitterEndpoint] Created...");    
		}

        #endregion
        #region Public Methods

        public bool ReuseEndpoint()
        {
            return true;
        }

        /// <summary>
        /// This method is called when a Send Location is enabled.
        /// </summary>
        public void Open(
            EndpointParameters endpointParameters, 
            IPropertyBag handlerPropertyBag, 
            string propertyNamespace)
        {
            _propertyNamespace = propertyNamespace;
        }
		/// <summary>
        /// Implementation for AsyncTransmitterEndpoint::ProcessMessage
        /// Transmit the message and optionally moves the file from RemoteTempDir to RemotePath
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
        public IBaseMessage ProcessMessage(IBaseMessage message)
        {
            
            _properties = new SftpTransmitProperties(message, _propertyNamespace);
            ISftp sftp = SftpConnectionPool.GetHostByName(_properties).GetConnection(_properties,_shutdownRequested);

            try
            {
                if (!_shutdownRequested)
                {
                    ProcessMessageInternal(message, sftp);
                }
            }
            catch (Exception ex)
            {
                //CheckErrorThreshold();
                TraceMessage("[SftpTransmitterEndpoint] Exception: " + ex.Message);
                throw ExceptionHandling.HandleComponentException(System.Reflection.MethodBase.GetCurrentMethod(),ex);
            }
            finally
            {
                SftpConnectionPool.GetHostByName(_properties).ReleaseConnection(sftp);
            }
            return null;
        }


        #endregion
        #region Private Methods
        private IBaseMessage ProcessMessageInternal(IBaseMessage message, ISftp sftp)
		{
            string filePath = "";
            
            try
            {
                Stream source = message.BodyPart.Data;
                source.Position = 0;

                if (_properties.RemoteTempFile.Trim().Length > 0) // Temp dir + Temp file
                    filePath = SftpTransmitProperties.CreateFileName(message, CommonFunctions.CombinePath(_properties.RemoteTempDir, _properties.RemoteTempFile));
                else if (_properties.RemoteTempDir.Trim().Length > 0) // Temp dir + file
                    filePath = SftpTransmitProperties.CreateFileName(message, CommonFunctions.CombinePath(_properties.RemoteTempDir, _properties.RemoteFile));
                else // dir + file
                    filePath = SftpTransmitProperties.CreateFileName(message, CommonFunctions.CombinePath(_properties.RemotePath, _properties.RemoteFile));

                TraceMessage("[SftpTransmitterEndpoint] Sftp.Put " + filePath);
                sftp.Put(source, filePath);

                // If the RemoteTempDir is set then move the file to the RemotePath
                if (_properties.RemoteTempDir.Trim().Length > 0)
                {
                    if (_properties.VerifyFileSize)
                        VerifyFileSize(sftp, filePath, source.Length); // throws exception if sizes do not match

                    string toPath = SftpTransmitProperties.CreateFileName(message, 
                        CommonFunctions.CombinePath(_properties.RemotePath, _properties.RemoteFile));
                    sftp.Rename(filePath, toPath);
                    sftp.ApplySecurityPermissions(_properties.ApplySecurityPermissions, toPath);
                }
                else
                    sftp.ApplySecurityPermissions(_properties.ApplySecurityPermissions, filePath);
                
                return null;
            }
            catch (Exception ex)
            {
                string innerEx = ex.InnerException?.Message ?? "";
                innerEx += @". Changing any Send Port Transport properties might require the host to be restarted, as the connection pool might still have connections";

                throw new SftpException("[SftpTransmitterEndpoint] Unable to transmit file " + filePath + ".\nInner Exception:\n" + ex.Message + "\n" + innerEx, ex);
            }
            
		}
        private void VerifyFileSize(ISftp sftp, string filePath, long expectedFileSize)
        {
            FileEntry f = sftp.GetFileEntry(filePath, _properties.DebugTrace);
            if (f.Size != expectedFileSize)
            {
                try
                {
                    sftp.Delete(filePath);
                }
                catch (Exception ex)
                {
                    throw new Exception("Exception during Delete currupt file.", ex);
                }

                throw new SftpException("Corrupt file " + filePath +
                    ". Expected  " + expectedFileSize + " bytes, actual was " + f.Size + " bytes. " +
                    "File deleted from remote system. " +
                    "Will retry if BizTalk configured retry attempts are not exhausted.");
            }
        }

        private void TraceMessage(string message)
        {
            if (_properties.DebugTrace)
                Trace.WriteLine(message);
        }
        /// <summary>
        /// If ErrorThreshold exeeds number of exceptions the location will be stopped.
        /// </summary>
        /// <returns></returns>
        private bool CheckErrorThreshold()
        {
            _errorCount++;
            if ((0 != _properties.ErrorThreshold) && (_errorCount > _properties.ErrorThreshold))
            {
                _asyncTransmitter.Terminate();

                ExceptionHandling.CreateEventLogMessage(
                    String.Format("[SftpTransmitterEndpoint] Error threshold exceeded {0}. Port is shutting down.\nURI: {1}", _properties.ErrorThreshold.ToString(), _properties.Uri),
                    EventLogEventIDs.GeneralUnknownError,
                    0,
                    EventLogEntryType.Warning);

                return false;
            }
            return true;
        }

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Trace.WriteLine("[SftpTransmitterEndpoint] Disposing...");
                    _shutdownRequested = true;
                    Trace.WriteLine("[SftpTransmitterEndpoint] Disposed...");
                }

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
