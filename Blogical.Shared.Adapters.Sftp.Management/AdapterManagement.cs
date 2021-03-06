using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Xml;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Adapter.Framework;

namespace Blogical.Shared.Adapters.Sftp.Management
{
    /// <summary>
    /// This class is used for administrating both the Receive and the Send adapter.
    /// </summary>
    /// <history>2013-11-10 Greg Sharp, Add X.509 identity certificate support</history>
    public class StaticAdapterManagement : IStaticAdapterConfig, IAdapterConfigValidation
    {
        private static readonly ResourceManager ResourceManager = new ResourceManager("Blogical.Shared.Adapters.Sftp.Management.SftpResource", Assembly.GetExecutingAssembly());

        /// <summary>
        /// Populating properties from property schemas
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="resourceManager"></param>
        /// <returns></returns>
        protected string LocalizeSchema(string schema, ResourceManager resourceManager)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(schema);

            XmlNodeList nodes = document.SelectNodes("/descendant::*[@_locID]");
            foreach (XmlNode node in nodes)
            {
                string locId = node.Attributes["_locID"].Value;
                //node.InnerText = resourceManager.GetString(locID);
            }

            StringWriter writer = new StringWriter();
            document.WriteTo(new XmlTextWriter(writer));

            string localizedSchema = writer.ToString();
            return localizedSchema;
        }

        /// <summary>
        /// Returns the configuration schema as a string.
        /// (Implements IAdapterConfig)
        /// </summary>
        /// <param name="type">Configuration schema to return</param>
        /// <returns>Selected xsd schema as a string</returns>
        public string GetConfigSchema(ConfigType type)
        {
            switch (type)
            {
                case ConfigType.ReceiveHandler:
                    return LocalizeSchema(GetResource("Blogical.Shared.Adapters.Sftp.Management.ReceiveHandler.xsd"), ResourceManager);

                case ConfigType.ReceiveLocation:
                    return LocalizeSchema(GetResource("Blogical.Shared.Adapters.Sftp.Management.ReceiveLocation.xsd"), ResourceManager);

                case ConfigType.TransmitHandler:
                    return LocalizeSchema(GetResource("Blogical.Shared.Adapters.Sftp.Management.TransmitHandler.xsd"), ResourceManager);

                case ConfigType.TransmitLocation:
                    return LocalizeSchema(GetResource("Blogical.Shared.Adapters.Sftp.Management.TransmitLocation.xsd"), ResourceManager);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the WSDL file name for the selected WSDL
        /// </summary>
        /// <param name="wsdls">place holder</param>
        /// <returns>An empty string[]</returns>
        public string[] GetServiceDescription(string[] wsdls)
        {
            string[] result = new string[1];
            result[0] = GetResource("Blogical.Shared.Adapters.Sftp.Management.service1.wsdl");
            return result;
        }

        /// <summary>
        /// Gets the XML instance of TreeView that needs to be rendered
        /// </summary>
        /// <param name="endPointConfiguration"></param>
        /// <param name="nodeIdentifier"></param>
        /// <returns>Location of TreeView xml instance</returns>
        public string GetServiceOrganization(IPropertyBag endPointConfiguration,
                                             string nodeIdentifier)
        {
            string result = GetResource("Blogical.Shared.Adapters.Sftp.Management.CategorySchema.xml");
            return result;
        }


        /// <summary>
        /// Acquire externally referenced xsd's
        /// </summary>
        /// <param name="xsdLocation">Location of schema</param>
        /// <param name="xsdNamespace">Namespace</param>
        /// <param name="xsdSchema">Schmea file name (return)</param>
        /// <returns>Outcome of acquisition</returns>
        public Result GetSchema(string xsdLocation,
                                string xsdNamespace,
                                out string xsdSchema)
        {
            xsdSchema = null;
            return Result.Continue;
        }

        /// <summary>
        /// Validate xmlInstance against configuration.
        /// </summary>
        /// <param name="configType">Type of port or location being configured</param>
        /// <param name="xmlInstance">Instance value to be validated</param>
        /// <returns>Validated configuration.</returns>
        public string ValidateConfiguration(ConfigType configType, string xmlInstance)
        {
            switch (configType)
            {
                case ConfigType.ReceiveLocation:
                    return ValidateReceiveLocation(xmlInstance);

                case ConfigType.TransmitLocation:
                    return ValidateTransmitLocation(xmlInstance);
                default:
                    return xmlInstance;
            }
        }

        /// <summary>
        /// Helper to get resource from manafest.  Replace with 
        /// ResourceManager.GetString if .resources or
        /// .resx files are used for managing this assemblies resources.
        /// </summary>
        /// <param name="resource">Full resource name</param>
        /// <returns>Resource value</returns>
        private string GetResource(string resource)
        {
            string value = null;
            if (null != resource)
            {
                Assembly assem = GetType().Assembly;
                Stream stream = assem.GetManifestResourceStream(resource);

                using (var reader = new StreamReader(stream))
                {
                    value = reader.ReadToEnd();
                }
            }

            return value;
        }

        /// <summary>
        /// Generate uri entry based on directory and fileMask values
        /// </summary>
        /// <param name="xmlInstance">Instance value to be validated</param>
        /// <returns>Validated configuration.</returns>
        internal static string ValidateReceiveLocation(string xmlInstance)
        {
            XmlDocument document1 = new XmlDocument();
            document1.LoadXml(xmlInstance);
            XmlNode nodeHost = GetNode(document1, "uri");
            nodeHost.InnerText = ValidateReceiveLocation(document1);
            return document1.OuterXml;
        }

        internal static string ValidateReceiveLocation(XmlDocument doc)
        {

            XmlNode nodeSchedule = GetRequiredNode(doc, "schedule", "You must specify the Scheduling properties.");
            XmlNode nodeHost = GetRequiredNode(doc, "host", "You must specify an SSH host.");
            XmlNode nodePort = GetNode(doc, "port", false);
            XmlNode nodeRemotepath = GetPathProp(doc, "remotepath", false);
            XmlNode nodeRemotetempdir = GetPathProp(doc, "remotetempdir", false);
            XmlNode nodeFileMask = GetNode(doc, "filemask", "*");

            XmlNode nodePassword = GetNode(doc, "password", false);
            XmlNode nodeIdentityFile = GetNode(doc, "identityfile", false);
            XmlNode nodeIdentityThumbprint = GetNode(doc, "identitythumbprint", false);
            XmlNode nodeSsoApplication = GetNode(doc, "ssoapplication", false);

            if ((nodePassword == null || nodePassword.InnerText.Length == 0) &&
                (nodeIdentityFile == null || nodeIdentityFile.InnerText.Length == 0) &&
                (nodeIdentityThumbprint == null || nodeIdentityThumbprint.InnerText.Length == 0) &&
                (nodeSsoApplication == null || nodeSsoApplication.InnerText.Length == 0))
                throw new Exception("You must specify either Password, IdentityFile, IdentityThumbprint, or SSO Application");

            // Default values for After Get Actions:
            string textAfterGetAction = "";
            XmlNode nodeAfterGetAction = GetNode(doc, "aftergetaction", false);
            if (nodeAfterGetAction != null && nodeAfterGetAction.InnerText.Length > 0)
            {
                textAfterGetAction = nodeAfterGetAction.InnerText;
            }
            // Check for invalid actions: (the Enum.IsDefined doesn't have an overload for ignore case, as Enum.Parse has)
            // Empty string is allowed, see SftpReceiveProperties for details.
            if (!String.IsNullOrEmpty(textAfterGetAction))
            {
                SftpReceiveProperties.AfterGetActions afterGetAction;
                if (!Enum.TryParse<SftpReceiveProperties.AfterGetActions>(textAfterGetAction, true, out afterGetAction))
                {
                    throw new Exception(
                        "You must specify an After Get Action: Delete, Rename or DoNothing. Empty field equals DoNothing.");
                }
            }

            StringBuilder builder1 = new StringBuilder("SFTP://" + nodeHost.InnerText);
            if (nodePort != null && nodePort.InnerText.Length > 0)
            {
                builder1.Append(":" + nodePort.InnerText);
            }
            string text1 = nodeRemotepath?.InnerText ?? "/";
            if (!text1.StartsWith("/"))
            {
                text1 = "/" + text1;
            }
            builder1.Append(text1 + nodeFileMask.InnerText);
            return builder1.ToString();
        }

        /// <summary>
        /// Generate uri entry based on directory and fileName values
        /// </summary>
        /// <param name="xmlInstance">Instance value to be validated</param>
        /// <returns>Validated configuration.</returns>
        internal static string ValidateTransmitLocation(string xmlInstance)
        {
            XmlDocument document1 = new XmlDocument();
            document1.LoadXml(xmlInstance);
            XmlNode nodeHost = GetNode(document1, "uri");
            nodeHost.InnerText = ValidateTransmitLocation(document1);
            return document1.OuterXml;
        }
        internal static string ValidateTransmitLocation(XmlDocument doc)
        {
            XmlNode nodeHost = GetRequiredNode(doc, "host", "You must specify an SSH host.");
            XmlNode nodePort = GetNode(doc, "port", false);
            XmlNode nodeRemotepath = GetPathProp(doc, "remotepath", false);
            XmlNode nodeRemotefile = GetRequiredNode(doc, "remotefile", "You must specify a remote file.");

            XmlNode nodeApplySecurityPermissions = GetNode(doc, "applySecurityPermissions", "");
            if (!String.IsNullOrEmpty(nodeApplySecurityPermissions.InnerText))
            {
                int value;
                if (!int.TryParse(nodeApplySecurityPermissions.InnerText, out value))
                {
                    throw new Exception("The SSH Apply Security Permissions must be empty or a numeric value.");
                }
            }
            XmlNode nodePassword = GetNode(doc, "password", false);
            XmlNode nodeIdentityFile = GetNode(doc, "identityfile", false);
            XmlNode nodeIdentityThumbprint = GetNode(doc, "identitythumbprint", false);
            XmlNode nodeSsoApplication = GetNode(doc, "ssoapplication", false);

            if ((nodePassword == null || nodePassword.InnerText.Length == 0) &&
                (nodeIdentityFile == null || nodeIdentityFile.InnerText.Length == 0) &&
                (nodeIdentityThumbprint == null || nodeIdentityThumbprint.InnerText.Length == 0) &&
                (nodeSsoApplication == null || nodeSsoApplication.InnerText.Length == 0))
                throw new Exception("You must specify either Password, IdentityFile or IdentityThumbprint or SSO Application");

            StringBuilder builder1 = new StringBuilder("SFTP://" + nodeHost.InnerText);
            if (nodePort.InnerText.Length > 0)
            {
                builder1.Append(":" + nodePort.InnerText);
            }
            string text1 = nodeRemotepath?.InnerText ?? "/";
            if (!text1.StartsWith("/"))
            {
                text1 = "/" + text1;
            }
            builder1.Append(text1 + nodeRemotefile.InnerText);
            return builder1.ToString();
        }

        internal static XmlNode GetRequiredNode(XmlDocument doc, string field, string error)
        {
            XmlNode nodeHost = doc.SelectSingleNode("/Config/" + field);
            if (String.IsNullOrEmpty(nodeHost?.InnerText.Trim()))
            {
                throw new Exception(error);
            }
            return nodeHost;
        }
        internal static XmlNode GetPathProp(XmlDocument doc, string xpath, bool create)
        {
            XmlNode nodeHost = GetNode(doc, "remotepath", false);
            if (nodeHost != null)
            {
                nodeHost.InnerText = nodeHost.InnerText.Replace(@"\", "/");
                if (!nodeHost.InnerText.EndsWith("/"))
                {
                    nodeHost.InnerText = nodeHost.InnerText + "/";
                }
            }
            return nodeHost;
        }
        internal static XmlNode GetNode(XmlDocument doc, string field, bool create = true)
        {
            //bool create = true;
            XmlNode nodeHost = doc.SelectSingleNode("/Config/" + field);
            if (nodeHost == null && create)
            {
                nodeHost = doc.CreateElement(field);
                doc.DocumentElement.AppendChild(nodeHost);
            }
            return nodeHost;
        }
        internal static XmlNode GetNode(XmlDocument doc, string field, string defValue)
        {
            XmlNode nodeHost = GetNode(doc, field);
            if (nodeHost.InnerText.Length == 0)
            {
                nodeHost.InnerText = defValue;
            }
            return nodeHost;
        }
    }
}
