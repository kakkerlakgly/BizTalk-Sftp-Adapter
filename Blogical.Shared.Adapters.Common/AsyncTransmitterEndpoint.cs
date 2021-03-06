//---------------------------------------------------------------------
// File: AsyncTransmitterEndpoint.cs
// 
// Summary: Implementation of an adapter framework sample adapter. 
// This class constitutes one of the BaseAdapter classes, which, are
// a set of generic re-usable set of classes to help adapter writers.
//
// Sample: Base Adapter Class Library v1.0.2
//
// Description: TODO:
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
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;

namespace Blogical.Shared.Adapters.Common
{
    public abstract class EndpointParameters
    {
        public abstract string SessionKey { get; }
        public string OutboundLocation { get; }

        protected EndpointParameters (string outboundLocation)
        {
            OutboundLocation = outboundLocation;
        }
    }

    internal class DefaultEndpointParameters : EndpointParameters
    {
        public override string SessionKey 
        {
            //  the SessionKey is the outboundLocation in the default case
            get { return OutboundLocation; }
        }
        public DefaultEndpointParameters (string outboundLocation) : base(outboundLocation)
        {
        }
    }

    public interface IAsyncTransmitterEndpoint : IDisposable
    {
        bool ReuseEndpoint();
        void Open (EndpointParameters endpointParameters, IPropertyBag handlerPropertyBag, string propertyNamespace);
        IBaseMessage ProcessMessage (IBaseMessage message);
    }
}
