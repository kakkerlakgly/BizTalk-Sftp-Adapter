namespace Blogical.Shared.Adapters.Common
{
    /// <summary>
    /// This class contains shared constants that can/will be used in different projects.
    /// </summary>
    public static class Constants
    {

        #region Public Constants
        
        /// <summary>
        /// The namespace to BizTalk file context properties.
        /// </summary>
        public const string BiztalkFilePropertiesNamespace = "http://schemas.microsoft.com/BizTalk/2003/file-properties";

        /// <summary>
        /// The namespace to BizTalk system context properties.
        /// </summary>
        public const string BiztalkSystemPropertiesNamespace = "http://schemas.microsoft.com/BizTalk/2003/system-properties";

        /// <summary>
        /// The namespace to SFTP adapter properties.
        /// </summary>
        public const string SftpAdapterPropertiesNamespace = "http://schemas.microsoft.com/BizTalk/2006/sftp-properties";

        #endregion

        #region BizTalkSystemPropertyNames

        /// <summary>
        /// This class contains BizTalk message context system property-names.
        /// </summary>
        public static class BizTalkSystemPropertyNames
        {
            /// <summary>
            /// Send port name.
            /// </summary>
            public const string SendPortName = "SPName";
            /// <summary>
            /// ReceivePortID.
            /// </summary>
            public const string ReceivePortId = "ReceivePortID";

            /// <summary>
            /// Inbound transport location
            /// </summary>
            public const string Inboundtransportlocation = "InboundTransportLocation";

            /// <summary>
            /// Interchange ID
            /// </summary>
            public const string InterchangeId = "InterchangeID";

            /// <summary>
            /// Actual retry count
            /// </summary>
            public const string ActualRetryCount = "ActualRetryCount";

            /// <summary>
            /// ACK / NACK
            /// </summary>
            public const string AckType = "AckType";

            /// <summary>
            /// Failure description
            /// </summary>
            public const string AckDescription = "AckDescription";

            /// <summary>
            /// Originating send port 
            /// </summary>
            public const string AckSendPortName = "AckSendPortName";

            /// <summary>
            /// Receive port name
            /// </summary>
            public const string AckReceivePortName = "AckReceivePortName";

            /// <summary>
            /// SendPortID.
            /// </summary>
            public const string SendPortId = "SPID";
        }

        #endregion
    }

    /// <summary>
    /// Class containing constants holding connection-string keys.
    /// </summary>
    public static class ConnectionStringKeys
    {
        #region ConnectionStringConstants
        /// <summary>
        /// Key to connection-string for tracking database
        /// </summary>
        public const string Blogicaldb = "BlogicalDB";

        #endregion
    }

    /// <summary>
    /// IMPORTANT!
    /// The Sources that are added to this class, must also be added in the installer above.
    /// </summary>
    public static class EventLogSources
    {
        public const string SftpAdapter = "SFTPAdapter";
    }

    public static class EventLogEventIDs
    {
        // GeneralError
        public const int GeneralUnknownError = 0;

        // Adapters are 1xxx
        public const int UnableToConnectToHost = 1001;
        public const int UnableToListDirectory = 1002;
        public const int UnableToGetFile = 1003;
        public const int UnableToRenameFile = 1004;
        public const int UnableToWriteFile = 1005;

        public const int UnableToCreateBizTalkMessage = 1010;
        public const int UnableToSubmitBizTalkMessage = 1011;

    }

}
