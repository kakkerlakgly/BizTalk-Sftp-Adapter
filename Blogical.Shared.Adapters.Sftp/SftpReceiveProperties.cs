using System;
using Blogical.Shared.Adapters.Common;
using Microsoft.BizTalk.Component.Interop;
using System.Xml;
using Blogical.Shared.Adapters.Common.Schedules;
using System.Diagnostics;

namespace Blogical.Shared.Adapters.Sftp
{
    /// <summary>
    /// The SftpReceiveProperties class represents the properties defined in
    /// Blogical.Shared.Adapters.Sftp.Management.ReceiveLocation.xsd
    /// </summary>
    /// <history>2013-11-10 Greg Sharp, Add identity certificate support</history>
    public class SftpReceiveProperties : ConfigProperties
    {
        #region Constructor

        #endregion
        #region Private Fields

        private string _sshHost             = String.Empty;
        private string _sshPasswordProperty = String.Empty;
        private int _sshPort                = 22;
        private string _sshUser             = String.Empty;
        private string _sshIdentityFile     = String.Empty;
        private string _sshIdentityThumbprint = String.Empty;
        private string _ssoApplication      = String.Empty;

        private string _sshFileMask         = String.Empty;
        private string _sshRemotePath       = String.Empty;
        private bool _sshtrace;
        private bool _notifyOnEmptyBatch;
        private int _sshErrorThreshold      = 10;
        private int _maximumnumberoffiles;
        private int _maximumbatchsize;
        private bool _useLoadBalancing      = true;

        private Schedule _schedule;

        // Proxy settings
        private string _proxyHost           = String.Empty;

        private int _proxyPort              = 80;
        private string _proxyUsername       = String.Empty;
        private string _proxyPassword       = String.Empty;

        #endregion
        #region Public Members
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
        /// Indicates the type of files to download from the server
        /// </summary>
        public string FileMask
        {
            get { return _sshFileMask; }
        }
        /// <summary>
        /// The current path to the SFTP server
        /// </summary>
        public string RemotePath
        {
            get 
            {
               if(_sshRemotePath.EndsWith("/"))
                   return _sshRemotePath; 
                else
                   return _sshRemotePath+"/"; 
            }
            set
            {
                _sshRemotePath = value;
            }
        }
        /// <summary>
        /// Writes a message to the trace listeners
        /// </summary>
        public bool DebugTrace
        {
            get { return _sshtrace; }
        }
        /// <summary>
        /// Causes the adapter to send a notification message if the batch is empty.
        /// </summary>
        public bool NotifyOnEmptyBatch
        {
            get { return _notifyOnEmptyBatch; }
        }
        /// <summary>
        /// The number of errors before the adapter shuts down  
        /// </summary>
        public int ErrorThreshold
        {
            get { return _sshErrorThreshold; }
        }
        /// <summary>
        /// Enter the maximum number of files to be submitted in a single BizTalk batch (0 indicates no limit).
        /// </summary>
        public int MaximumNumberOfFiles
        {
            get {return _maximumnumberoffiles; }
        }
        /// <summary>
        /// Enter the maximum number of bytes to be submitted in a single BizTalk batch (0 indicates no limit).
        /// </summary>
        public int MaximumBatchSize
        {
            get { return _maximumbatchsize; }
        }
        /// <summary>
        /// Complete path and filemask. sftp://[host]:[port]/[remotepath]/[filemsk]
        /// </summary>
        public string Uri
        {
            get { return CommonFunctions.CombinePath("SFTP://" + SSHHost + ":" + SSHPort, RemotePath, FileMask); }
        }
        /// <summary>
        /// 
        /// </summary>
        public Schedule Schedule { get { return _schedule; } }
        /// <summary>
        /// Indicates the Receive location is executed on more then one server node. 
        /// </summary>
        public bool UseLoadBalancing
        {
            get { return _useLoadBalancing; }
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

        // Greg Killins 2010/06/07 - added the third valid option of "DoNothing" 
        // as the AfterGetActions.       
        public enum AfterGetActions
        {
            Delete = 1,
            Rename = 2,
            DoNothing = 3
        }

        private AfterGetActions _afterGet = AfterGetActions.Delete;

        /// <summary>
        /// Indicates what should happen after we have trasnferred the file. Default is delete.
        /// </summary>
        public AfterGetActions AfterGet
        {
            get { return _afterGet; }
            set { _afterGet = value; }
        }

        private string _afterGetFilename;

        public string AfterGetFilename
        {
            get { return _afterGetFilename; }
            set { _afterGetFilename = value; }
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
        /// Read the Blogical.Shared.Adapters.Sftp.Management.ReceiveLocation.xsd and populate 
        /// all properties
        /// </summary>
        /// <param name="config"></param>
        /// <param name="bizTalkConfig"></param>
        public void LocationConfiguration(IPropertyBag config, IPropertyBag bizTalkConfig)
        {
            TraceMessage("[SftpReceiverEndpoint] LocationConfiguration called");
            
            XmlDocument endpointConfig = ExtractConfigDomImpl(config, true);

            _ssoApplication = IfExistsExtract(endpointConfig, "/Config/ssoapplication", String.Empty);

            if (!String.IsNullOrEmpty(_ssoApplication))
            {
                TraceMessage("[SftpReceiverEndpoint] SSO Authentication");
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
                _sshUser = Extract(endpointConfig, "/Config/user", String.Empty);
                _sshPasswordProperty = IfExistsExtract(endpointConfig, "/Config/password", String.Empty);
            }

            _sshHost = Extract(endpointConfig, "/Config/host", String.Empty);
            _sshPort = ExtractInt(endpointConfig, "/Config/port");
            _sshIdentityFile = IfExistsExtract(endpointConfig, "/Config/identityfile", String.Empty);
            _sshIdentityThumbprint = IfExistsExtract(endpointConfig, "/Config/identitythumbprint", String.Empty);

            _sshFileMask = Extract(endpointConfig, "/Config/filemask", String.Empty);
            _sshRemotePath = Extract(endpointConfig, "/Config/remotepath", String.Empty);
            _sshtrace = ExtractBool(endpointConfig, "/Config/trace");
            _notifyOnEmptyBatch = ExtractBool(endpointConfig, "/Config/notifyOnEmptyBatch");
            _sshErrorThreshold = ExtractInt(endpointConfig, "/Config/errorThreshold");
            _useLoadBalancing = ExtractBool(endpointConfig, "/Config/useLoadBalancing");

            _maximumnumberoffiles = ExtractInt(endpointConfig, "/Config/maximumnumberoffiles");
            _maximumbatchsize = ExtractInt(endpointConfig, "/Config/maximumbatchsize");

            // create the schedule
            XmlDocument scheduleXml = new XmlDocument();
            scheduleXml.LoadXml(Schedule.Extract(endpointConfig, "/Config/schedule", true));
            ScheduleType type = Schedule.ExtractScheduleType(scheduleXml);
            switch (type)
            {
                case ScheduleType.Daily:
                    _schedule = new DaySchedule(scheduleXml.OuterXml);
                    break;
                case ScheduleType.Weekly:
                    _schedule = new WeekSchedule(scheduleXml.OuterXml);
                    break;
                case ScheduleType.Monthly:
                    _schedule = new MonthSchedule(scheduleXml.OuterXml);
                    break;
                default:
                    _schedule = new TimeSchedule(scheduleXml.OuterXml);
                    break;
            }

            // Greg Killins 2010/06/07 - Pass default value of empty string instead of "Delete" when
            // extracting AfterGetActions value, since empty string is now a valid value which
            // represents "DoNothing".
            // If the extracted value is empty string, then explicitly assign "DoNothing" to
            // the AfterGetActions.
            string afterget = IfExistsExtract(endpointConfig, "/Config/aftergetaction", string.Empty);
            if (afterget.Trim() == String.Empty)
            {
                _afterGet = AfterGetActions.DoNothing;
            }
            else
            {
                try
                {
                    _afterGet = (AfterGetActions)Enum.Parse(typeof(AfterGetActions), afterget, true);
                }
                catch (ArgumentException)
                {
                    throw new Exception("You must specify an After Get Action: Delete, Rename or DoNothing. Empty field equals DoNothing.");
                }
            }
            _afterGetFilename = IfExistsExtract(endpointConfig, "/Config/aftergetfilename", string.Empty);

            _sshPassphrase = IfExistsExtract(endpointConfig, "/Config/passphrase", string.Empty);

            // Proxy Settings
            _proxyHost = IfExistsExtract(endpointConfig, "/Config/proxyserver", String.Empty);
            _proxyPort = ExtractInt(endpointConfig, "/Config/proxyport"); 
            _proxyUsername = IfExistsExtract(endpointConfig, "/Config/proxyusername", String.Empty);
            _proxyPassword = IfExistsExtract(endpointConfig, "/Config/proxypassword", String.Empty);

        }
        /// <summary>
        /// Read the Blogical.Shared.Adapters.Sftp.Management.ReceiveLocation.xsd and populate 
        /// all properties
        /// </summary>
        /// <param name="endpointConfig"></param>
        public void ReadLocationConfiguration(XmlDocument endpointConfig)
        {
            TraceMessage("[SftpReceiverEndpoint] ReadLocationConfiguration called");

            _ssoApplication = IfExistsExtract(endpointConfig, "/Config/ssoapplication", String.Empty);

            if (!String.IsNullOrEmpty(_ssoApplication))
            {
                TraceMessage("[SftpReceiverEndpoint] SSO Authentication");
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
                TraceMessage("[SftpReceiverEndpoint] Username/Password Authentication");
                _sshUser = Extract(endpointConfig, "/Config/user", String.Empty);
                _sshPasswordProperty = IfExistsExtract(endpointConfig, "/Config/password", String.Empty);
            }

            _sshHost = Extract(endpointConfig, "/Config/host", String.Empty);
            _sshPort = ExtractInt(endpointConfig, "/Config/port");
            _sshIdentityFile = IfExistsExtract(endpointConfig, "/Config/identityfile", String.Empty);
            _sshIdentityThumbprint = IfExistsExtract(endpointConfig, "/Config/identitythumbprint", String.Empty);
   
            _sshFileMask = Extract(endpointConfig, "/Config/filemask", String.Empty);
            _sshRemotePath = Extract(endpointConfig, "/Config/remotepath", String.Empty);
            _sshtrace = ExtractBool(endpointConfig, "/Config/trace");
            _notifyOnEmptyBatch = ExtractBool(endpointConfig, "/Config/notifyOnEmptyBatch");
            _sshErrorThreshold = ExtractInt(endpointConfig, "/Config/errorThreshold");

            _maximumnumberoffiles = ExtractInt(endpointConfig, "/Config/maximumnumberoffiles");
            _maximumbatchsize = ExtractInt(endpointConfig, "/Config/maximumbatchsize");
            _useLoadBalancing = ExtractBool(endpointConfig, "/Config/useLoadBalancing");
           
            // create the schedule
            XmlDocument scheduleXml = new XmlDocument();
            scheduleXml.LoadXml(Schedule.Extract(endpointConfig, "/Config/schedule", true));
            ScheduleType type = Schedule.ExtractScheduleType(scheduleXml);
            switch (type)
            {
                case ScheduleType.Daily:
                    _schedule = new DaySchedule(scheduleXml.OuterXml);
                    break;
                case ScheduleType.Weekly:
                    _schedule = new WeekSchedule(scheduleXml.OuterXml);
                    break;
                case ScheduleType.Monthly:
                    _schedule = new MonthSchedule(scheduleXml.OuterXml);
                    break;
                default:
                    _schedule = new TimeSchedule(scheduleXml.OuterXml);
                    break;
            }

            // Greg Killins 2010/06/07 - Pass default value of empty string instead of "Delete" when
            // extracting AfterGetActions value, since empty string is now a valid value which
            // represents "DoNothing".
            // If the extracted value is empty string, then explicitly assign "DoNothing" to
            // the AfterGetActions.
            string afterget = IfExistsExtract(endpointConfig, "/Config/aftergetaction", string.Empty);
            if (afterget.Trim() == String.Empty)
            {
                _afterGet = AfterGetActions.DoNothing;
            }
            else
            {
                try
                {
                    _afterGet = (AfterGetActions)Enum.Parse(typeof(AfterGetActions), afterget, true);
                }
                catch (ArgumentException)
                {
                    throw new Exception("You must specify an After Get Action: Delete, Rename or DoNothing. Empty field equals DoNothing.");
                }
            }
            _afterGetFilename = IfExistsExtract(endpointConfig, "/Config/aftergetfilename", string.Empty);

            _sshPassphrase = IfExistsExtract(endpointConfig, "/Config/passphrase", string.Empty);

            // Proxy Settings
            _proxyHost = IfExistsExtract(endpointConfig, "/Config/proxyserver", String.Empty);
            _proxyPort = ExtractInt(endpointConfig, "/Config/proxyport");
            _proxyUsername = IfExistsExtract(endpointConfig, "/Config/proxyusername", String.Empty);
            _proxyPassword = IfExistsExtract(endpointConfig, "/Config/proxypassword", String.Empty);
        }

        /// <summary>
        /// Read the Blogical.Shared.Adapters.Sftp.Management.ReceiveHandler.xsd and populate 
        /// all properties
        /// </summary>
        /// <param name="configDom"></param>
        public static void ReceiveHandlerConfiguration(XmlDocument configDom)
        {
            // Handler properties
            //handlerPollingInterval = ExtractPollingInterval(configDOM);
            //handlerMaximumNumberOfFiles = IfExistsExtractInt(configDOM, "/Config/maximumNumberOfFiles", handlerMaximumNumberOfFiles);
        }
        #endregion
        #region Private Members
        private void TraceMessage(string message)
        {
            if (DebugTrace)
                Trace.WriteLine(message);
        }
        #endregion
    }
    


}
