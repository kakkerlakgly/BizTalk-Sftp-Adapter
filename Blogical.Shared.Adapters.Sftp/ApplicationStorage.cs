using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;

using System.Collections.Concurrent;
using System.Linq;
using System.Xml.Serialization;

namespace Blogical.Shared.Adapters.Sftp
{
    /// <summary>
    /// Used for storing HostKeys in IsolatedStorage
    /// </summary>
    [Serializable]
    public class ApplicationStorage 
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ApplicationStorage() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host"></param>
        /// <param name="hostKey"></param>
        public ApplicationStorage(string host, string hostKey)
        {
            Host = host;
            HostKey = hostKey;
        }
        /// <summary>
        /// Name of the application host or server
        /// </summary>
        public readonly string Host;
        /// <summary>
        /// Unique id retrieved from server
        /// </summary>
        public readonly string HostKey;
    }
    /// <summary>
    /// 
    /// </summary>
    internal static class ApplicationStorageHelper
    {
        private const string SettingsFileName = "SftpHostFiles.config";
        private static readonly object Objlock = new object();
        /// <summary>
        /// Load all hostkeys from IsolatedStorage.
        /// Eg. \Document and Settings\[BizTalk Service User]\Local Settings\Application Data\IsolatedStorage\
        /// </summary>
        /// <returns></returns>
        public static IProducerConsumerCollection<ApplicationStorage> Load()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.GetFileNames(SettingsFileName).Length == 0)
            {
                return new ConcurrentBag<ApplicationStorage>();
            }

            // Read the stream from Isolated Storage.
            lock (Objlock)
            {
                XmlSerializer ser = new XmlSerializer(typeof(ApplicationStorage[]));
                Stream stream = null;
                try
                {
                    stream = new IsolatedStorageFileStream(SettingsFileName, FileMode.OpenOrCreate, isoStore);
                    {
                        using (TextReader reader = new StreamReader(stream))
                        {
                            stream = null;
                            ApplicationStorage[] arr = (ApplicationStorage[])ser.Deserialize(reader);
                        }
                    }
                }
                finally
                {
                    stream?.Dispose();
                }
            }
            return new ConcurrentBag<ApplicationStorage>();
        }

        public static IProducerConsumerCollection<ApplicationStorage> _Load()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.GetFileNames(SettingsFileName).Length == 0)
            {
                return new ConcurrentBag<ApplicationStorage>();
            }

            // Read the stream from Isolated Storage.
            XmlSerializer ser = new XmlSerializer(typeof(ApplicationStorage[]));
            Stream stream = null;
            try
            {
                stream = new IsolatedStorageFileStream(SettingsFileName, FileMode.OpenOrCreate, isoStore);
                using (TextReader reader = new StreamReader(stream))
                {
                    stream = null;
                    ApplicationStorage[] arr = (ApplicationStorage[]) ser.Deserialize(reader);
                }
            }
            finally
            {
                stream?.Dispose();
            }
            return new ConcurrentBag<ApplicationStorage>();
        }
        /// <summary>
        /// Save hostkeys to IsolatedStorage
        /// Eg. \Document and Settings\[BizTalk Service User]\Local Settings\Application Data\IsolatedStorage\
        /// </summary>
        /// <param name="applicationStorage"></param>
        public static void Save(IEnumerable<ApplicationStorage> applicationStorage)
        {
            // Open the stream from the IsolatedStorage.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            // Greg Sharp: Provide thread-safety around write access to the config file
            lock (Objlock)
            {
                XmlSerializer ser = new XmlSerializer(typeof(ApplicationStorage[]));
                Stream stream = null;
                try
                {
                    stream = new IsolatedStorageFileStream(SettingsFileName, FileMode.Create, isoStore);
                    using (TextWriter writer = new StreamWriter(stream))
                    {
                        stream = null;
                        ser.Serialize(writer, applicationStorage.ToArray());
                    }
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }
        /// <summary>
        /// Get the hostkey from application storage
        /// </summary>
        /// <param name="applicationStorage"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static string GetHostKey(IEnumerable<ApplicationStorage> applicationStorage, string host)
        {
            return applicationStorage.Where(apps => apps.Host == host).Select(apps => apps.HostKey).FirstOrDefault();
        }
    }


}
