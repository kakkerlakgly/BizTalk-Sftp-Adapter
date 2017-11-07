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
        private const string Objlock = "lock";
        /// <summary>
        /// Load all hostkeys from IsolatedStorage.
        /// Eg. \Document and Settings\[BizTalk Service User]\Local Settings\Application Data\IsolatedStorage\
        /// </summary>
        /// <returns></returns>
        public static IProducerConsumerCollection<ApplicationStorage> Load()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            var applicationStorage = new ConcurrentBag<ApplicationStorage>();
            if (isoStore.GetFileNames(SettingsFileName).Length == 0)
            {
                return applicationStorage;
            }

            // Read the stream from Isolated Storage.
            lock (Objlock)
            {
                using (Stream stream = new IsolatedStorageFileStream(SettingsFileName, FileMode.OpenOrCreate, isoStore))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(ApplicationStorage[]));
                    using (TextReader reader = new StreamReader(stream))
                    {
                        ApplicationStorage[] arr = (ApplicationStorage[])ser.Deserialize(reader);
                    }
                    applicationStorage = new ConcurrentBag<ApplicationStorage>();
                }
            }
            return applicationStorage;
        }

        public static IProducerConsumerCollection<ApplicationStorage> _Load()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            var applicationStorage = new ConcurrentBag<ApplicationStorage>();
            if (isoStore.GetFileNames(SettingsFileName).Length == 0)
            {
                return applicationStorage;
            }

            // Read the stream from Isolated Storage.
            using (Stream stream = new IsolatedStorageFileStream(SettingsFileName, FileMode.OpenOrCreate, isoStore))
            {
                XmlSerializer ser = new XmlSerializer(typeof(ApplicationStorage[]));
                using (TextReader reader = new StreamReader(stream))
                {
                    ApplicationStorage[]arr= (ApplicationStorage[])ser.Deserialize(reader);
                }
                applicationStorage = new ConcurrentBag<ApplicationStorage>();
            }
            return applicationStorage;
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
                using (Stream stream = new IsolatedStorageFileStream(SettingsFileName, FileMode.Create, isoStore))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(ApplicationStorage[]));
                    using (TextWriter writer = new StreamWriter(stream))
                    {
                        ser.Serialize(writer, applicationStorage.ToArray());
                    }
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
            foreach (ApplicationStorage apps in applicationStorage)
            {
                if (apps.Host == host)
                    return apps.HostKey;
            }
            return null;
        }
    }


}
