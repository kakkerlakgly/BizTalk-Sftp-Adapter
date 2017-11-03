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
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
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
        private const int MAX_BATCH_SIZE = 50;

        //  members to initialize the batch with
        private int maxBatchSize = MAX_BATCH_SIZE;
        private Type endpointType;
        private IDictionary<string, AsyncTransmitterEndpoint> endpoints = new Dictionary<string, AsyncTransmitterEndpoint>();

        private ControlledTermination control;

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
            this.endpointType = endpointType;
            this.maxBatchSize = maxBatchSize;
            control = new ControlledTermination();
        }

        protected virtual int MaxBatchSize
        {
            get { return maxBatchSize; }
        }

        protected Type EndpointType
        {
            get { return endpointType; }
        }

        protected ControlledTermination ControlledTermination { get { return control; } }

        public void Dispose()
        {
            control.Dispose();
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

        public virtual AsyncTransmitterEndpoint GetEndpoint(IBaseMessage message)
        {
            // Provide a virtual "CreateEndpointParameters" method to map message to endpoint
            EndpointParameters endpointParameters = CreateEndpointParameters(message);

            lock (endpoints)
            {
                AsyncTransmitterEndpoint endpoint = endpoints[endpointParameters.SessionKey];
                if (null == endpoint)
                {
                    //  we haven't seen this location so far this batch so make a new endpoint
                    endpoint = (AsyncTransmitterEndpoint)Activator.CreateInstance(endpointType, new object[] { this });

                    if (null == endpoint)
                        throw new CreateEndpointFailed(endpointType.FullName, endpointParameters.OutboundLocation);

                    endpoint.Open(endpointParameters, HandlerPropertyBag, PropertyNamespace);

                    if (endpoint.ReuseEndpoint)
                    {
                        endpoints[endpointParameters.SessionKey] = endpoint;
                    }
                }
                return endpoint;
            }
        }

        public override void Terminate ()
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("[AsyncTransmitter] Terminate");
                //  Block until we are done...
                // Let all endpoints finish the work they are doing before disposing them
                control.Terminate();

                foreach (AsyncTransmitterEndpoint endpoint in endpoints.Values)
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

                base.Terminate();
            }
            finally
            {
                Dispose();
            }
        }

        public bool Enter ()
        {
            System.Diagnostics.Trace.WriteLine("[AsyncTransmitter] Enter");
            return control.Enter();
        }

        public void Leave ()
        {
            System.Diagnostics.Trace.WriteLine("[AsyncTransmitter] Leave");
            control.Leave();
        }
    }
}
