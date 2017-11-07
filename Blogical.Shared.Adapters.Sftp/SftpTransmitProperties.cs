using System;
using Blogical.Shared.Adapters.Common;
using System.Xml;
using Microsoft.BizTalk.Message.Interop;
using System.Diagnostics;
using System.IO;

namespace Blogical.Shared.Adapters.Sftp
{
    /// <summary>
    /// The SftpTransmitProperties class represents the properties defined in
    /// Blogical.Shared.Adapters.Sftp.Management.TransmitLocation.xsd
    /// </summary>
    /// <history>2013-11-10 Greg Sharp, Add X.509 identity certificate support</history>
    public class SftpTransmitProperties : ConfigProperties
    {
        #region Private Fields
        private static int _handlerSendBatchSize    = 20;

        private string _sshHost                             = String.Empty;
        private string _sshPasswordProperty                 = String.Empty;
        private int _sshPort                                = 22;
        private string _sshUser                             = String.Empty;
        private string _sshIdentityFile                     = String.Empty;
        private string _sshIdentityThumbprint               = String.Empty;
        private string _ssoApplication                      = String.Empty;
        private bool _sshtrace;

        private string _sshRemotePath                       = String.Empty;
        private string _sshRemoteTempDir                    = String.Empty;
        private string _sshRemoteFile                       = String.Empty;
        private int _sshErrorThreshold                      = 10;
        private int _connectionLimit                        = 10;
        private string _applySecurityPermissions            = String.Empty;
        private bool _verifyFileSize;

        // Proxy settings
        private string _proxyHost                           = String.Empty;

        private int _proxyPort                              = 80;
        private string _proxyUsername                       = String.Empty;
        private string _proxyPassword                       = String.Empty;

        #endregion
        #region Public Properties
        //public static int BufferSize { get { return _handlerbufferSize; } }
        //public static int ThreadsPerCPU { get { return _handlerthreadsPerCPU; } }

        /// <summary>
        /// Size of entire batch
        /// </summary>
        public static int BatchSize { get { return _handlerSendBatchSize; } }
        /// <summary>
        /// The address of the SSH host
        /// </summary>
        public string SSHHost
        {
            get { return _sshHost; }
        }
        /// <summary>
        /// The password for SSH password-based authentication
        /// </summary>
        public string SSHPasswordProperty
        {
            get { return _sshPasswordProperty; }
        }
        /// <summary>
        /// The port in the SSH server where the SSH service is running; by default 22.
        /// </summary>
        public int SSHPort
        {
            get { return _sshPort; }
        }
        /// <summary>
        /// The username for SSH authentication.
        /// </summary>
        public string SSHUser
        {
            get { return _sshUser; }
        }
        /// <summary>
        /// The certificate to use for client authentication during the SSH handshake.
        /// </summary>
        public string SSHIdentityFile
        {
            get { return _sshIdentityFile; }
        }
        /// <summary>
        /// The certificate to use for client authentication during the SSH handshake.
        /// </summary>
        public string SSHIdentityThumbprint
        {
            get { return _sshIdentityThumbprint; }
        }
        /// <summary>
        /// The Single Sign On (SSO) Affiliate Application
        /// </summary>
        public string SSOApplication
        {
            get { return _ssoApplication; }
        }
        /// <summary>
        /// The current path to the SFTP server
        /// </summary>
        public string RemotePath
        {
            get
            {
                if (_sshRemotePath.EndsWith("/"))
                    return _sshRemotePath;
                else
                    return _sshRemotePath + "/";
            }
        }
        /// <summary>
        /// A temporary directory on the server to store files before moving them to Remote path
        /// </summary>
        public string RemoteTempDir
        {
            get
            {
                if (_sshRemoteTempDir.Length == 0)
                    return _sshRemoteTempDir;
                else if (_sshRemoteTempDir.EndsWith("/"))
                    return _sshRemoteTempDir;
                else
                    return _sshRemoteTempDir + "/";
            }
        }
        /// <summary>
        /// SSH Remote file name
        /// </summary>
        public string RemoteFile
        {
            get { return _sshRemoteFile; }
        }
        /// <summary>
        /// The number of errors before the adapter shuts down 
        /// </summary>
        public int ErrorThreshold
        {
            get { return _sshErrorThreshold; }
        }
        /// <summary>
        /// Writes a message to the trace listeners
        /// </summary>
        public bool DebugTrace
        {
            get { return _sshtrace; }
        }
        /// <summary>
        /// Uri
        /// </summary>
        public string Uri
        {
            get
            {
                return CommonFunctions.CombinePath("SFTP://" + SSHHost + ":" + SSHPort, RemotePath, RemoteFile);
            }
        }
        /// <summary>
        /// Maximum number of concurrentSftp connections that can be opened to the server. 10 is default.
        /// </summary>
        public int ConnectionLimit
        {
            get { return _connectionLimit; }
        }
        /// <summary>
        /// A numerical representing a permission matrix. These permissions are overridden on Windows platforms, and are therefore useless on such a host. Default value on UNIX platforms are 644. If left empty, no permissioins will be applied.
        /// </summary>
        public string ApplySecurityPermissions
        {
            get { return _applySecurityPermissions; }
        }

        // Proxy Settings
        /// <summary>
        /// The URI to the HTTP Proxy server
        /// </summary>
        public string ProxyHost
        {
            get { return _proxyHost; }
        }
        /// <summary>
        /// The port on which the HTTP proxy is running on; by default 80.
        /// </summary>
        public int ProxyPort
        {
            get { return _proxyPort; }
        }
        /// <summary>
        /// The username used for proxy authentication.
        /// </summary>
        public string ProxyUserName
        {
            get { return _proxyUsername; }
        }
        /// <summary>
        /// The password used for proxy authentication.
        /// </summary>
        public string ProxyPassword
        {
            get { return _proxyPassword; }
        }


        public bool VerifyFileSize
        {
            get { return _verifyFileSize; }
            set { _verifyFileSize = value; }
        }

        private string _sshRemoteTempFile;

        public string RemoteTempFile
        {
            get { return _sshRemoteTempFile; }
            set { _sshRemoteTempFile = value; }
        }

        private string _sshPassphrase;

        public string SSHPassphrase
        {
            get { return _sshPassphrase; }
            set { _sshPassphrase = value; }
        }

        #endregion
        #region Public Methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="propertyNamespace"></param>
        public SftpTransmitProperties(IBaseMessage message, string propertyNamespace)
        {
            //  get the adapter configuration off the message
            IBaseMessageContext context = message.Context;
            string config = (string)context.Read("AdapterConfig", propertyNamespace);

            if (null != config)
            {
                var locationConfigDom = new XmlDocument();
                locationConfigDom.LoadXml(config);

                ReadLocationConfiguration(locationConfigDom);
            }
            else //  the config can be null all that means is that we are doing a dynamic send
            {
                ReadLocationConfiguration(message.Context);
            }
        }

        /// <summary>
        /// Read the Blogical.Shared.Adapters.Sftp.Management.TransmitLocation.xsd and populate 
        /// all properties 
        /// </summary>
        /// <param name="endpointConfig"></param>
        public void ReadLocationConfiguration(XmlDocument endpointConfig)
        {
            _sshtrace = ExtractBool(endpointConfig, "/Config/trace");

            TraceMessage("[SftpTransmitProperties] ReadLocationConfiguration called");

            _ssoApplication = IfExistsExtract(endpointConfig, "/Config/ssoapplication", String.Empty);

            if (!String.IsNullOrEmpty(_ssoApplication))
            {
                TraceMessage("[SftpTransmitProperties] SSO Authentication");
                try
                {
                    SSOConfigHelper.Credentials credentials = SSOConfigHelper.GetCredentials(_ssoApplication);
                    _sshUser = credentials.Username;
                    _sshPasswordProperty = credentials.Password;
                }
                catch (Exception e)
                {
                    throw new Exception(@"Unable to read properties from SSO database. Make sure to use ""UserName"" and ""Password"" as fields", e);
                }
            }
            else
            {
                TraceMessage("[SftpTransmitProperties] Username/Password Authentication");

                _sshUser = Extract(endpointConfig, "/Config/user", String.Empty);
                _sshPasswordProperty = IfExistsExtract(endpointConfig, "/Config/password", String.Empty);
            }

            //this._sshUser = Extract(endpointConfig, "/Config/user", String.Empty);
            //this._sshPasswordProperty = IfExistsExtract(endpointConfig, "/Config/password", String.Empty);

            _sshHost = Extract(endpointConfig, "/Config/host", String.Empty);
            _sshPort = ExtractInt(endpointConfig, "/Config/port");
            _sshIdentityFile = IfExistsExtract(endpointConfig, "/Config/identityfile", String.Empty);
            _sshIdentityThumbprint = IfExistsExtract(endpointConfig, "/Config/identitythumbprint", String.Empty);

            _sshRemotePath = Extract(endpointConfig, "/Config/remotepath", String.Empty);
            _sshRemoteTempDir = IfNotEmptyExtract(endpointConfig, "/Config/remotetempdir", false, String.Empty);
            _sshRemoteFile = Extract(endpointConfig, "/Config/remotefile", String.Empty);
            _sshErrorThreshold = ExtractInt(endpointConfig, "/Config/errorThreshold");
            _connectionLimit = ExtractInt(endpointConfig, "/Config/connectionlimit");
            _applySecurityPermissions = IfExistsExtract(endpointConfig, "/Config/applySecurityPermissions", String.Empty);
            _verifyFileSize = IfExistsExtractBool(endpointConfig, "/Config/verifyFileSize", false);
            _sshRemoteTempFile = IfExistsExtract(endpointConfig, "/Config/remotetempfile", String.Empty);
            _sshPassphrase = IfExistsExtract(endpointConfig, "/Config/passphrase", String.Empty);

            // Proxy Settings
            _proxyHost = IfExistsExtract(endpointConfig, "/Config/proxyserver", String.Empty);
            _proxyPort = ExtractInt(endpointConfig, "/Config/proxyport");
            _proxyUsername = IfExistsExtract(endpointConfig, "/Config/proxyusername", String.Empty);
            _proxyPassword = IfExistsExtract(endpointConfig, "/Config/proxypassword", String.Empty);
        }

        /// <summary>
        /// Read the Blogical.Shared.Adapters.Sftp.Management.TransmitLocation.xsd from message context 
        /// and populate properties 
        /// </summary>
        /// <param name="context"></param>
        public void ReadLocationConfiguration(IBaseMessageContext context)
        {
            string propertyNS = "Blogical.Shared.Adapters.Sftp.TransmitLocation.v1";
            _sshtrace = (bool)Extract(context, "trace", propertyNS, false, false);

            TraceMessage("[SftpTransmitProperties] ReadLocationConfiguration called");

            _ssoApplication = (string)Extract(context, "ssoapplication", propertyNS, String.Empty, false);

            if (!String.IsNullOrEmpty(_ssoApplication))
            {
                TraceMessage("[SftpTransmitProperties] SSO Authentication");
                try
                {
                    SSOConfigHelper.Credentials credentials = SSOConfigHelper.GetCredentials(_ssoApplication);
                    _sshUser = credentials.Username;
                    _sshPasswordProperty = credentials.Password;
                }
                catch (Exception e)
                {
                    throw new Exception(@"Unable to read properties from SSO database. Make sure to use ""UserName"" and ""Password"" as fields", e);
                }
            }
            else
            {
                TraceMessage("[SftpTransmitProperties] Username/Password Authentication");
                _sshUser = (string)Extract(context, "user", propertyNS, String.Empty, true);
                _sshPasswordProperty = (string)Extract(context, "password", propertyNS, String.Empty, false);
            }

            // this._sshUser = (string)Extract(context, "user", propertyNS, String.Empty, true);
            // this._sshPasswordProperty = (string)Extract(context, "password", propertyNS, String.Empty, false);

            _sshHost = (string)Extract(context, "host", propertyNS, String.Empty, true);
            _sshPort = (int)Extract(context, "portno", propertyNS, 22, true);
            _sshIdentityFile = (string)Extract(context, "identityfile", propertyNS, String.Empty, false);
            _sshIdentityThumbprint = (string)Extract(context, "identitythumbprint", propertyNS, String.Empty, false);
            _sshRemotePath = (string)Extract(context, "remotepath", propertyNS, String.Empty, false);
            _sshRemoteTempDir = (string)Extract(context, "remotetempdir", propertyNS, String.Empty, false);
            _sshRemoteFile = (string)Extract(context, "remotefile", propertyNS, String.Empty, true);
            _connectionLimit = (int)Extract(context, "connectionlimit", propertyNS, 10, false);
            _applySecurityPermissions = (string)Extract(context, "applySecurityPermissions", propertyNS, String.Empty, false);
            _verifyFileSize = (bool)Extract(context, "verifyFileSize", propertyNS, false, false);
            _sshRemoteTempFile = (string)Extract(context, "remotetempfile", propertyNS, String.Empty, false);
            _sshPassphrase = (string)Extract(context, "passphrase", propertyNS, string.Empty, false);

            // Proxy Settings
            _proxyHost = (string)Extract(context, "proxyserver", propertyNS, string.Empty, false);
            _proxyPort = (int)Extract(context, "proxyport", propertyNS,80, false);
            _proxyUsername = (string)Extract(context, "proxyusername", propertyNS, string.Empty, false);
            _proxyPassword = (string)Extract(context, "proxypassword", propertyNS, string.Empty, false); 
        }

        /// <summary>
        /// Load the Transmit Handler configuration settings
        /// </summary>
        public static void ReadTransmitHandlerConfiguration(XmlDocument configDom)
        {
            // Handler properties
            _handlerSendBatchSize = ExtractInt(configDom, "/Config/sendBatchSize");
        }
        /// <summary>
        /// Determines the name of the file that should be created for a transmitted message
        /// replaces %MessageID% with the message's Guid if specified.
        /// </summary>
        /// <param name="message">The Message to transmit</param>
        /// <param name="uri">The address of the message.  May contain "%MessageID%" </param>
        /// <returns>The name of the file to write to</returns>
        public static string CreateFileName(IBaseMessage message, string uri)
        {
            //string uriNew = ReplaceMessageID(message, uri);
            return ReplaceMacros(message, uri);
        }
        #endregion
        #region Private Methods
        private static string ReplaceMacros(IBaseMessage message, string uri)
        {
            if (uri.IndexOf("%MessageID%") > -1)
            {
                Guid msgId = message.MessageID;

                uri = uri.Replace("%MessageID%", msgId.ToString());
            }
            if (uri.IndexOf("%SourceFileName%") > -1)
            {
                string sourceFileName;
                try
                {
                    string filePath = GetReceivedFileName(message);
                    sourceFileName = Path.GetFileName(filePath);
                }
                catch
                {
                    throw new Exception("The %SourceFileName% macro can only be used with the " + Constants.SftpAdapterPropertiesNamespace + " namespace.");
                }
                uri = uri.Replace("%SourceFileName%", sourceFileName);
            }

            /* John C. Vestal 2010/04/07 - Added DateTime and UniversalDateTime to macro list. */
            if (uri.IndexOf("%DateTime%") > -1)
            {
                string dateTime = DateTime.Now.ToString();
                dateTime = dateTime.Replace("/", "-");
                dateTime = dateTime.Replace(":", "");
                uri = uri.Replace("%DateTime%", dateTime);
            }
            if (uri.IndexOf("%UniversalDateTime%") > -1)
            {
                string dateTime = DateTime.Now.ToUniversalTime().ToString();
                dateTime = dateTime.Replace("/", "-");
                dateTime = dateTime.Replace(":", "");
                uri = uri.Replace("%UniversalDateTime%", dateTime);
            }
            // Peter Lindgren 2014-02-12: Added datetime macro that works exactly as the corresponding macro in the standard FTP and FILE adapters.
            if (uri.IndexOf("%datetime%") > -1)
            {
                string dateTime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHHmmss");
                uri = uri.Replace("%datetime%", dateTime);
            }

            return uri;
        }
        private static string GetReceivedFileName(IBaseMessage pInMsg)
        {
            SystemMessageContext messageContext = new SystemMessageContext(pInMsg.Context);

            string receivedFileName =
                pInMsg.Context.Read("ReceivedFileName",
                                "http://schemas.microsoft.com/BizTalk/2003/file-properties") as string;

            if (receivedFileName == null)
            {
                if (messageContext.InboundTransportType.ToUpper().Equals("SQL"))
                {
                    // If data is retrieved via the SQL-adapter  
                    // set receivedFileName to ReceivePortName
                    if (pInMsg.Context.Read("ReceivedFileName",
                          "http://schemas.microsoft.com/BizTalk/2003/sql-properties") == null)
                    {
                        receivedFileName = pInMsg.Context.Read("ReceivePortName",
                            "http://schemas.microsoft.com/BizTalk/2003/system-properties").ToString();
                    }
                    else
                    {
                        receivedFileName = pInMsg.Context.Read("ReceivedFileName",
                          "http://schemas.microsoft.com/BizTalk/2003/sql-properties").ToString();
                    }
                }
                else
                {
                    // BizTalk OOTB pipelines use this pattern
                    // The SFTP adapter has also been coded to use this pattern - other adapters might not!!!
                    receivedFileName = pInMsg.Context.Read("ReceivedFileName",
                         "http://schemas.microsoft.com/BizTalk/2003/" +
                             messageContext.InboundTransportType.ToLower() + "-properties").ToString();
                }
            }
            return receivedFileName;
        }
        private static string ReplaceMessageId(IBaseMessage message, string uri)
        {
            Guid msgId = message.MessageID;

            return uri.Replace("%MessageID%", msgId.ToString());
        }
        private void TraceMessage(string message)
        {
            if (DebugTrace)
                Trace.WriteLine(message);
        }
        private object Extract(IBaseMessageContext context, string prop, string propNs, object fallback, bool isRequired)
        {
            Object o = context.Read(prop, propNs);
            if (!isRequired && null == o)
                return fallback;
            if (null == o)
                throw new NoSuchProperty(propNs + "#" + prop);
            return o;
        }

        #endregion
    }
}
