using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Security;
using System.Threading;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Blogical.Shared.Adapters.Common;
using Blogical.Shared.Adapters.Sftp;
using Microsoft.BizTalk.Scheduler;
using System.Reflection;
using System.Data;
using System.Linq;
using Blogical.Shared.Adapters.Sftp.ConnectionPool;
using Blogical.Shared.Adapters.Common.Schedules;

namespace Blogical.Shared.Adapters.Sftp
{
    /// <summary>
    /// This class corresponds to a Receive Location/URI.  It handles polling the
    /// given folder for new messages. 
    /// </summary>
    /// <history>2013-11-10 Greg Sharp, Add X.509 identity certificate support</history>
    internal class SftpReceiverEndpoint : ReceiverEndpoint
    {
        #region Constants
        private const string MESSAGE_BODY = "body";
        private const string REMOTEFILENAME = "FileName";
        #endregion

        private bool isInitialized = false;

        #region Constructor
        public SftpReceiverEndpoint()
        {
        }
        #endregion
        #region Public Methods (Adapter Service Control)
        /// <summary>
        /// This method is called when a Receive Location is enabled.
        /// </summary>
        public override void Open(
            string uri,
            IPropertyBag config,
            IPropertyBag bizTalkConfig,
            IPropertyBag handlerPropertyBag,
            IBTTransportProxy transportProxy,
            string transportType,
            string propertyNamespace,
            ControlledTermination control)
        {

            _errorCount = 0;

            _properties = new SftpReceiveProperties();
            _properties.LocationConfiguration(config, bizTalkConfig);

            //  Location properties - possibly override some Handler properties
            XmlDocument locationConfigDom = ConfigProperties.ExtractConfigDom(config);
            _properties.ReadLocationConfiguration(locationConfigDom);

            //  this is our handle back to the EPM
            _transportProxy = transportProxy;

            // used to create new messages / message parts etc.
            _messageFactory = _transportProxy.GetMessageFactory();

            //  used in the creation of messages
            _transportType = transportType;

            //  used in the creation of messages
            _propertyNamespace = propertyNamespace;

            // used to track inflight work for shutting down properly
            _controlledTermination = control;

            //  create and schedule a new the task
            _taskController = new TaskController(
                new ScheduledTask(_properties.Uri,
                new ScheduledTask.TaskDelegate(ControlledEndpointTask)),
                _properties.Schedule);

            _taskController.StateChanged += new StateChangedEventHandler(OnStateChanged);

            // start the task
            Start();
        }

        /// <summary>
        /// This method is called when the configuration for this receive location is modified.
        /// The Location will be stopped while configurations are made.
        /// </summary>
        public override void Update(IPropertyBag config, IPropertyBag bizTalkConfig, IPropertyBag handlerPropertyBag)
        {
            TraceMessage("[SftpReceiverEndpoint] Configuration Updates ");

            lock (this)
            {
                Stop();

                _errorCount = 0;

                //  keep handles to these property bags until we are ready
                _updatedConfig = config;
                _updatedBizTalkConfig = bizTalkConfig;
                _updatedHandlerPropertyBag = handlerPropertyBag;

                if (_updatedConfig != null)
                {
                    XmlDocument locationConfigDom = ConfigProperties.ExtractConfigDom(config);
                    _properties.ReadLocationConfiguration(locationConfigDom);

                    _taskController = new TaskController(
                                        new ScheduledTask(_properties.Uri,
                                        new ScheduledTask.TaskDelegate(ControlledEndpointTask)),
                                        _properties.Schedule);
                    isInitialized = false;
                }

                //Schedule the polling event
                Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            TraceMessage("[SftpReceiverEndpoint] Dispose called");

            //  stop the schedule
            Stop();
        }

        /// <summary>
        /// The Location is Enabled
        /// </summary>
        private void Start()
        {
            _taskController.Enabled = true;
            _taskController.Start();
            TraceMessage("[SftpReceiverEndpoint] Start called");
        }

        /// <summary>
        /// The Location is Disabled 
        /// </summary>
        private void Stop()
        {
            TraceMessage("[SftpReceiverEndpoint] Stop called");
            try
            {
                _taskController.Stop();
                _taskController.Enabled = false;
                _taskController.Dispose();
                // this.timer.Dispose();
            }
            catch (Exception e)
            {
                TraceMessage("[SftpReceiverEndpoint] Stop EXCEPTION");
                throw ExceptionHandling.HandleComponentException(MethodBase.GetCurrentMethod(),
                    new SftpException("Unable to stop receiverEndpoint", e));
            }
        }

        /// <summary>
        /// this method is called from the task scheduler when the polling interval has elapsed.
        /// </summary>
        public void ControlledEndpointTask()
        {
            // The taskController always starts the task when initializing, so here we ignore this first controller.StartTask() call:
            if (!isInitialized)
            {
                isInitialized = true;
                return;
            }
            if (_controlledTermination.Enter())
            {
                TraceMessage("[SftpReceiverEndpoint] ControlledTermination.Enter()");
                try
                {
                    lock (this)
                    {
                        EndpointTask();
                    }
                    GC.Collect();
                }
                finally
                {
                    TraceMessage("[SftpReceiverEndpoint] ControlledTermination.Leave()");
                    _controlledTermination.Leave();
                }
            }
        }
        
        private void OnStateChanged(object sender, StateChangedEventArgs args)
        {
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Handle the work to be performed each polling interval
        /// </summary>
        private void EndpointTask()
        {
            try
            {
                // PickupFilesAndSubmit will be called as long as there
                // are files left in the directory.
                while (PickupFilesAndSubmit()) ;

                //Success, reset the error count
                _errorCount = 0;
            }
            catch (Exception ex)
            {
                //Track number of failures
                _errorCount++;
                TraceMessage("[SftpReceiverEndpoint] Exception #" + _errorCount.ToString());
                _transportProxy.SetErrorInfo(ex);

                throw ExceptionHandling.HandleComponentException(
                    EventLogEventIDs.GeneralUnknownError,
                    MethodBase.GetCurrentMethod(),
                        new Exception("Uri:" + _properties.Uri + "", ex));


            }
            finally
            {
                CheckErrorThreshold();
            }

        }

        /// <summary>
        /// If ErrorThreshold exeeds number of exceptions the location will be stopped.
        /// </summary>
        /// <returns></returns>
        private bool CheckErrorThreshold()
        {
            if ((0 != _properties.ErrorThreshold) && (_errorCount > _properties.ErrorThreshold))
            {
                _transportProxy.ReceiverShuttingdown(_properties.Uri, new ErrorThresholdExceeded());

                //Stop the timer.
                Stop();

                ExceptionHandling.CreateEventLogMessage(
                    String.Format("[SftpReceiverEndpoint] Error threshold exceeded {0}. Port is shutting down.\nURI: {1}", _properties.ErrorThreshold.ToString(), _properties.Uri),
                    EventLogEventIDs.GeneralUnknownError,
                    0,
                    EventLogEntryType.Warning);

                return false;
            }
            return true;
        }

        /// <summary>
        ///  The algorithm implemented here splits the list of files according to the
        ///  batch tuning parameters (number of bytes and number of files) because the
        ///  list is randomly ordered it is possible to have non-optimal batches. It might
        ///  be a slight optimization to order by increasing size and then cut the batches.
        /// 
        ///  If there are files left in the folder after the execution of PickupFilesAndSubmit,
        ///  the method will return true.
        /// </summary>
        /// <returns></returns>
        private bool PickupFilesAndSubmit()
        {
            ISftp sftp = null;
            BatchHandler batchHandler = null;
            long bytesInBatch = 0;
            List<FileEntry> fileEntries = null;

            try
            {
                TraceMessage("[SftpReceiverEndpoint] PickupFilesAndSubmit called [" + _properties.Uri + "]");


                if (string.IsNullOrEmpty(_properties.ProxyHost))
                {
                    sftp = new SharpSsh.Sftp(_properties.SSHHost,
                                        _properties.SSHUser,
                                        _properties.SSHPasswordProperty,
                                        _properties.SSHIdentityFile,
                                        _properties.SSHIdentityThumbprint,
                                        _properties.SSHPort,
                                        _properties.SSHPassphrase,
                                        _properties.DebugTrace);
                }
                else
                {
                    sftp = new SharpSsh.Sftp(_properties.SSHHost,
                                        _properties.SSHUser,
                                        _properties.SSHPasswordProperty,
                                        _properties.SSHIdentityFile,
                                        _properties.SSHIdentityThumbprint,
                                        _properties.SSHPort,
                                        _properties.SSHPassphrase,
                                        _properties.DebugTrace,
                                        _properties.ProxyHost,
                                        _properties.ProxyPort,
                                        _properties.ProxyUserName,
                                        _properties.ProxyPassword);

                }

                string uri = _properties.UseLoadBalancing ? _properties.Uri : null;

                lock (_filesInProcess)
                {
                    // Get a list of all files. If the batch is limited in size (MaximumNumberOfFiles>0), the directory listing 
                    // will quite after the set number of files has been listed.
                    if (_properties.MaximumNumberOfFiles > 0)
                        fileEntries = sftp.Dir(CommonFunctions.CombinePath(_properties.RemotePath, _properties.FileMask),
                            uri, _properties.MaximumNumberOfFiles, _filesInProcess.Cast<string>(), _properties.DebugTrace);
                    else
                        fileEntries = sftp.Dir(CommonFunctions.CombinePath(_properties.RemotePath, _properties.FileMask),
                            uri, _filesInProcess.Cast<string>(), _properties.DebugTrace);
                }
                // If batch has file enties create a BatchHandler and a new sftp connection.
                if (fileEntries.Count > 0)
                {
                    batchHandler = new BatchHandler(sftp, _propertyNamespace, _transportType, _transportProxy, _properties.DebugTrace, _properties.UseLoadBalancing);
                    // Used for Connection pool: batchHandler.BatchComplete += new BatchHandler.BatchHandlerDelegate(batchHandler_BatchComplete);
                }
                // If the NotifyOnEmptyBatch property is set to true, and the batch is empty,
                // an "emty batch message" is created and added to the batch. This message can
                // later be picked up by the pipeline components.
                else if (fileEntries.Count == 0 && _properties.NotifyOnEmptyBatch)
                {
                    Trace.WriteLine(string.Format("[SftpReceiverEndpoint] Sending notification on empty batch.", fileEntries.Count - 1));
                    batchHandler = new BatchHandler(sftp, _propertyNamespace, _transportType, _transportProxy, _properties.DebugTrace, _properties.UseLoadBalancing);
                    batchHandler.CreateEmptyBatchMessage(_properties.Uri);
                    // Used for Connection pool: batchHandler.BatchComplete += new BatchHandler.BatchHandlerDelegate(batchHandler_BatchComplete);
                }
                else
                {
                    // Greg Sharp: Disconnect not needed here (see finally block)
                    //sftp.Disconnect();
                    //sftp.Dispose();
                    //sftp = null;
                    // Used for Connection pool: SftpConnectionPool.GetHostByName(this._properties.SSHHost, this._properties.DebugTrace).ReleaseConnection(sftp);
                    return false;
                }
                lock (_filesInProcess)
                {
                    foreach (FileEntry fileEntry in fileEntries)
                    {
                        // If the file is empty (or a directory) or if it's being processed by another
                        // process, we will just move on.
                        if (fileEntry.Size == 0 ||
                            _filesInProcess.Contains(CommonFunctions.CombinePath(_properties.RemotePath,
                                fileEntry.FileName)))
                            continue;

                        if (_properties.MaximumNumberOfFiles == batchHandler.Files.Count &&
                            _properties.MaximumNumberOfFiles != 0)
                            break;

                        if (_properties.MaximumBatchSize < bytesInBatch &&
                            _properties.MaximumBatchSize != 0)
                            break;

                        string fileNamePath =
                            CommonFunctions.CombinePath(_properties.RemotePath, fileEntry.FileName);

                        // Prevent other processes to work with the same file by adding the filename to filesInProcess
                        _filesInProcess.Add(fileNamePath);

                        // Create and add message to batch
                        IBaseMessage msg = batchHandler.CreateMessage(fileNamePath, _properties.Uri,
                            fileEntry.Size,
                            _properties.AfterGet, _properties.AfterGetFilename);

                        if (null == msg)
                        {
                            // If the creation was unsuccessful, remove the file from the files to be processed
                            // so an other process can pick up the file.
                            _filesInProcess.Remove(fileNamePath);
                            continue;
                        }

                        // Keep a running total for the current batch
                        // Greg Sharp: Add stream length not file size because this value may be stale
                        if (msg.BodyPart.Data.CanSeek)
                            bytesInBatch += msg.BodyPart.Data.Length;
                        else
                            bytesInBatch += fileEntry.Size;
                    }

                }

                // Submit all messages to BTS
                batchHandler.SubmitFiles(_controlledTermination, _filesInProcess);

                // Check if we have been asked to stop - if so don't start another batch
                if (_controlledTermination.TerminateCalled)
                    return false;

                return sftp.Exists(CommonFunctions.CombinePath(_properties.RemotePath, _properties.FileMask));
            }
            catch (Exception)
            {
                // Greg Sharp : Test if sftp is null before using
                if (sftp != null)
                {
                    sftp.Disconnect();
                    sftp.Dispose();
                    sftp = null;
                }
                // Used for Connection pool: SftpConnectionPool.GetHostByName(this._properties.SSHHost, this._properties.DebugTrace).ReleaseConnection(sftp);

                // Greg Sharp: Preserve the stack trace
                //throw ex;
                throw;
            }
            finally
            {
                if (sftp != null)
                {
                    sftp.Disconnect();
                    sftp.Dispose();
                    sftp = null;
                }
            }
        }
        
        private void TraceMessage(string message)
        {
            if (_properties.DebugTrace)
                Trace.WriteLine(message);
        }
        #endregion
        #region Events
        void batchHandler_BatchComplete(ISftp sftp)
        {
            try
            {
                sftp.Disconnect();
                sftp.Dispose();
                sftp = null;
                //SftpConnectionPool.GetHostByName(this._properties.SSHHost).ReleaseConnection(sftp);
            }
            catch
            {
                throw ExceptionHandling.HandleComponentException(MethodBase.GetCurrentMethod(),
                        new Exception("Unable to release Sftp connection"));
            }
        }
        #endregion
        #region Private Members

        // The workload of files
        private ArrayList _filesInProcess = ArrayList.Synchronized(new ArrayList());

        //  Receive adapter properties
        private SftpReceiveProperties _properties;

        // The IBTTransportProxy interface is used by the adapter to interact with the BizTalk server at run time. 
        private IBTTransportProxy _transportProxy;

        // used to create new messages / message parts etc.
        private IBaseMessageFactory _messageFactory;

        //  used in the creation of messages
        private string _transportType;

        //  used in the creation of messages
        private string _propertyNamespace;

        // used to track inflight work
        private ControlledTermination _controlledTermination;

        //  error count for comparison with the error threshold
        int _errorCount;

        private TaskController _taskController;

        //  support for Update
        IPropertyBag _updatedConfig;
        IPropertyBag _updatedBizTalkConfig;
        IPropertyBag _updatedHandlerPropertyBag;

        #endregion
    }
}
