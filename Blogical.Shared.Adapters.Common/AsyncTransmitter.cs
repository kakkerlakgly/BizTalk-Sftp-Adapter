//---------------------------------------------------------------------
// File: AsyncTransmitter.cs
// 
// Summary: Implementation of an adapter framework sample adapter. 
// This class constitutes one of the BaseAdapter classes, which, are
// a set of generic re-usable set of classes to help adapter writers.
//
// Sample: Base Adapter Class Library v1.0.2
//
// Description: Base class for send side (transmitter) adapters
//
//---------------------------------------------------------------------
// This file is part of the Microsoft BizTalk Server 2006 SDK
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// This source code is intended only as a supplement to Microsoft BizTalk
// Server 2006 release and/or on-line documentation. See these other
// materials for detailed information regarding Microsoft code samples.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
// PURPOSE.
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;

namespace Blogical.Shared.Adapters.Common
{
    /// <summary>
    /// This is a singleton class for the send adapter. This send adapter support batched
    /// transmission. Messages will be delivered to this adapter for transmission by the 
    /// messaging engine using adapter batch. Adapter batch (IBTBatchTransmitter) should not
    /// be confused with messaging engine's batch (IBTTransportBatch).
    /// 
    /// Send adapter should have an endpoint class deriving AsyncTransmitterEndpoint class.
    /// When a message is given to the adapter, adapter needs to decide which endpoint the
    /// message should go to. The default implementation of the adapter routes the message
    /// based on "OutboundTransmitLocation" property. However, it can be customized to include 
    /// other properties by overriding CreateEndPointParameters() in this class.
    /// 
    /// See "AsyncTransmitterBatch" class for more details on batching.
    /// </summary>
    public class AsyncTransmitter :
        Adapter,
        IDisposable,
        IBTBatchTransmitter
    {
        //  default magic number
        private const int DefaultBatchSize = 50;

        //  members to initialize the batch with
        private readonly IDictionary<string, IAsyncTransmitterEndpoint> _endpoints = new Dictionary<string, IAsyncTransmitterEndpoint>();

        protected AsyncTransmitter (
            string name,
            string version,
            string description,
            string transportType,
            Guid clsid,
            string propertyNamespace,
            Type endpointType,
            int maxBatchSize)
            : base(
            name,
            version,
            description,
            transportType,
            clsid,
            propertyNamespace)
        {
            EndpointType = endpointType;
            MaxBatchSize = maxBatchSize;
            _controlledTermination = new ControlledTermination();
        }

        protected virtual int MaxBatchSize { get; }

        protected Type EndpointType { get; }

        private readonly ControlledTermination _controlledTermination;

        protected ControlledTermination ControlledTermination
        {
            get { return _controlledTermination; }
        }

        // IBTBatchTransmitter
        public IBTTransmitterBatch GetBatch ()
        { 
            IBTTransmitterBatch tb = CreateAsyncTransmitterBatch();

            return tb;
        }

        protected virtual IBTTransmitterBatch CreateAsyncTransmitterBatch ()
        {
            return new AsyncTransmitterBatch(
                MaxBatchSize,
                EndpointType,
                PropertyNamespace,
                HandlerPropertyBag,
                TransportProxy,
                this);
        }

        // Endpoint management is the responsibility of the transmitter
        protected virtual EndpointParameters CreateEndpointParameters(IBaseMessage message)
        {
            SystemMessageContext context = new SystemMessageContext(message.Context);
            return new DefaultEndpointParameters(context.OutboundTransportLocation);
        }

        public virtual IAsyncTransmitterEndpoint GetEndpoint(IBaseMessage message)
        {
            // Provide a virtual "CreateEndpointParameters" method to map message to endpoint
            EndpointParameters endpointParameters = CreateEndpointParameters(message);

            lock (_endpoints)
            {
                IAsyncTransmitterEndpoint endpoint;
                
                if (_endpoints.TryGetValue(endpointParameters.SessionKey, out endpoint))
                {
                    return endpoint;
                }
                //  we haven't seen this location so far this batch so make a new endpoint
                endpoint = (IAsyncTransmitterEndpoint)Activator.CreateInstance(EndpointType, this);

                endpoint.Open(endpointParameters, HandlerPropertyBag, PropertyNamespace);

                if (endpoint.ReuseEndpoint())
                {
                    _endpoints[endpointParameters.SessionKey] = endpoint;
                }
                return endpoint;
            }
        }

        public override void Terminate ()
        {
            try
            {
                Trace.WriteLine("[AsyncTransmitter] Terminate");
                //  Block until we are done...
                // Let all endpoints finish the work they are doing before disposing them
                _controlledTermination.Terminate();

                lock (_endpoints)
                {
                    foreach (IAsyncTransmitterEndpoint endpoint in _endpoints.Values)
                    {
                        //  clean up and potentially close any endpoints
                        try
                        {
                            endpoint.Dispose();
                        }
                        catch (Exception e)
                        {
                            TransportProxy.SetErrorInfo(e);
                        }
                    }
                }

                base.Terminate();
            }
            finally
            {
                Dispose();
            }
        }

        public bool Enter ()
        {
            Trace.WriteLine("[AsyncTransmitter] Enter");
            return _controlledTermination.Enter();
        }

        public void Leave ()
        {
            Trace.WriteLine("[AsyncTransmitter] Leave");
            _controlledTermination.Leave();
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _controlledTermination.Dispose();
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
