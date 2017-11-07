using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Configuration;
using System.Xml;
using Blogical.Shared.Adapters.Common;

namespace Blogical.Shared.Adapters.Sftp.ConnectionPool
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public static class SftpConnectionPool 
    {
        static SftpConnectionPool()
        {
            Trace.WriteLine("[SftpConnectionPool] Started...");

            try
            {
                XmlNode node = (XmlNode)ConfigurationManager.GetSection("Blogical.Shared.Adapters.Sftp");
                if (node != null)
                    Load(node);
            }
            catch 
            {
            }
        }
        

        /// <summary>
        /// Prepopulates the SftpConnectionPool with servers defined in config file.
        /// </summary>
        /// <param name="section"></param>
        public static void Load(XmlNode section)
        {
            try
            {
                DefaultConnectionLimit = int.Parse(section.SelectSingleNode("SftpConnectionPool").Attributes["defaultConnectionLimit"].Value);
                Trace.WriteLine("[SftpConnectionPool] DefaultConnectionLimit set to " +DefaultConnectionLimit.ToString());

                foreach (XmlNode node in section.SelectNodes("SftpConnectionPool/Host"))
                {
                    string name = node.Attributes["hostName"].Value;
                    int connLimit = int.Parse(node.Attributes["connectionLimit"].Value);
                    Hosts.Add(new SftpHost(name, connLimit, true));
                    Trace.WriteLine("[SftpConnectionPool] A limited connections("+connLimit.ToString()+") given to "+ name+".");
                }
                Trace.WriteLine("[SftpConnectionPool] SftpConnectionPool was loaded with " + Hosts.Count.ToString() + " hosts.");
            }
            catch (Exception e)
            {
                throw ExceptionHandling.HandleComponentException(System.Reflection.MethodBase.GetCurrentMethod(),
                        new Exception("SftpConnectionPool Load Configuration failed", e));
            }

        }

        private static readonly ConcurrentBag<SftpHost> Hosts = new ConcurrentBag<SftpHost>();
        /// <summary>
        /// Default number of connections per server
        /// </summary>
        public static int DefaultConnectionLimit = 60;

        /// <summary>
        /// Returns a SftpHost 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static SftpHost GetHostByName(SftpTransmitProperties properties)//string hostName, bool trace, int connectionLimit)
        {
            lock (Hosts)
            {
                foreach (SftpHost host in Hosts)
                {
                    if (host.HostName == properties.SSHHost)
                    {
                        if (host.ConnectionLimit != properties.ConnectionLimit)
                        {
                            host.ConnectionLimit = properties.ConnectionLimit;
                            
                            Trace.WriteLineIf(properties.DebugTrace, "[SftpConnectionPool] Overriding connection pool settings");
                        }
                        return host;
                    }
                }

                SftpHost newHost = new SftpHost(properties.SSHHost, properties.ConnectionLimit, properties.DebugTrace);
                Hosts.Add(newHost);

                return newHost;
            }
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public static void Dispose()
        {
            lock (Hosts)
            {
                SftpHost host;
                while (Hosts.TryTake(out host))
                {
                    ISftp sftp;
                    while (host.Connections.TryPop(out sftp))
                    {
                        sftp.Disconnect();
                        sftp.Dispose();
                        Trace.WriteLine("[SftpTransmitterEndpoint] Sftp.Disconnect from " + host);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <history>2013-11-10 Greg Sharp, Add X.509 identity certificate support</history>
    [Serializable]
    public class SftpHost
    {
        #region Private Members

        private int _currentCount;
        private readonly bool _trace;
        #endregion
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="maxNumberOfConnections"></param>
        /// <param name="trace"></param>
        public SftpHost(string hostName, int maxNumberOfConnections, bool trace)
        {
            ConnectionLimit = maxNumberOfConnections;
            HostName = hostName;
            _trace = trace;
        }
        #endregion
        #region Public Members
        /// <summary>
        /// Connection pool
        /// </summary>
        public readonly ConcurrentStack<ISftp> Connections = new ConcurrentStack<ISftp>();
        /// <summary>
        /// Connection limit per server
        /// </summary>
        public int ConnectionLimit;
        /// <summary>
        /// Server name
        /// </summary>
        public readonly string HostName;
        #endregion
        #region Public Methods

        /// <summary>
        /// Returns a new or  free connection from the pool
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="shutdownRequested"></param>
        /// <returns></returns>
        public ISftp GetConnection(SftpTransmitProperties properties, bool shutdownRequested)//.SSHHoststring username, string password, string identityFile, int port, bool shutdownRequested, string passphrase)
        {
            while (!shutdownRequested)
            {
                if (ConnectionLimit == 0)
                {
                    TraceMessage(
                        "[SftpConnectionPool] GetConnectionFromPool creating a new connection (not from pool)");
                    //ISftp sftp = new Sftp(this.HostName, username, password, identityFile, port, passphrase, this._trace);
                    ISftp sftp;
                    if (string.IsNullOrEmpty(properties.ProxyHost))
                    {
                        sftp = new SharpSsh.Sftp(properties.SSHHost,
                            properties.SSHUser,
                            properties.SSHPasswordProperty,
                            properties.SSHIdentityFile,
                            properties.SSHIdentityThumbprint,
                            properties.SSHPort,
                            properties.SSHPassphrase,
                            properties.DebugTrace);

                    }
                    else
                    {
                        sftp = new SharpSsh.Sftp(properties.SSHHost,
                            properties.SSHUser,
                            properties.SSHPasswordProperty,
                            properties.SSHIdentityFile,
                            properties.SSHIdentityThumbprint,
                            properties.SSHPort,
                            properties.SSHPassphrase,
                            properties.DebugTrace,
                            properties.ProxyHost,
                            properties.ProxyPort,
                            properties.ProxyUserName,
                            properties.ProxyPassword);
                    }

                    return sftp;
                }
                ISftp connection;
                if (Connections.TryPop(out connection))
                {
                    TraceMessage("[SftpConnectionPool] GetConnectionFromPool found a free connection in the pool");
                    return connection;
                }
                if (_currentCount < ConnectionLimit)
                {
                    TraceMessage("[SftpConnectionPool] GetConnectionFromPool creating a new connection for pool");
                    //ISftp sftp = new SharpSsh.Sftp(this.HostName, username, password, identityFile, port, passphrase, this._trace);

                    ISftp sftp;
                    if (string.IsNullOrEmpty(properties.ProxyHost))
                    {
                        sftp = new SharpSsh.Sftp(properties.SSHHost,
                            properties.SSHUser,
                            properties.SSHPasswordProperty,
                            properties.SSHIdentityFile,
                            properties.SSHIdentityThumbprint,
                            properties.SSHPort,
                            properties.SSHPassphrase,
                            properties.DebugTrace);
                        //(this.HostName, username, password, identityFile, port, passphrase, this._trace);
                    }
                    else
                    {
                        sftp = new SharpSsh.Sftp(properties.SSHHost,
                            properties.SSHUser,
                            properties.SSHPasswordProperty,
                            properties.SSHIdentityFile,
                            properties.SSHIdentityThumbprint,
                            properties.SSHPort,
                            properties.SSHPassphrase,
                            properties.DebugTrace,
                            properties.ProxyHost,
                            properties.ProxyPort,
                            properties.ProxyUserName,
                            properties.ProxyPassword);
                    }

                    _currentCount++;
                    return sftp;
                }
            }
            return null;

        }
        /// <summary>
        /// Release sftp connection to pool
        /// </summary>
        /// <param name="conn"></param>
        public void ReleaseConnection(ISftp conn)
        {
            if (conn != null)
            {
                if (ConnectionLimit == 0)
                {
                    TraceMessage("[SftpConnectionPool] Disposing connection object (no connection pool is used)");
                    conn.Disconnect();
                    conn.Dispose();
                    return;
                }

                if (_currentCount > ConnectionLimit)
                {
                    TraceMessage("[SftpConnectionPool] ReleaseConnectionToPool disposing connection object");
                    conn.Disconnect();
                    conn.Dispose();
                    _currentCount--;
                }
                else
                {
                    TraceMessage("[SftpConnectionPool] ReleaseConnectionToPool releasing connection to pool");
                    //conn.Disconnect();
                    Connections.Push(conn);
                }
            }

        }
        private void TraceMessage(string message)
        {
            if (_trace)
                Trace.WriteLine(message);
        }
        #endregion
    }
}
