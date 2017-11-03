//---------------------------------------------------------------------
// File: Adapter.cs
// 
// Summary: Implementation of an adapter framework sample adapter. 
// This class constitutes one of the BaseAdapter classes, which, are
// a set of generic re-usable set of classes to help adapter writers.
//
// Sample: Base Adapter Class Library v1.0.2
//
// Description: This class is the root object for an adapter, it 
// implements IBTTransport, IBTTransportControl, IPersistPropertyBag
// which are required for all adapters, though IPersistPropertyBag is
// optional
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
using System.Diagnostics;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;

namespace Blogical.Shared.Adapters.Common
{
	/// <summary>
	/// Summary description for Adapter.
	/// </summary>
	public abstract class Adapter :
		IBTTransport,
		IBTTransportControl,
		IPersistPropertyBag
	{
		//  core member data

	    //  member data for implementing IBTTransport

	    protected Adapter (
			string name,
			string version,
			string description,
			string transportType,
			Guid clsid,
			string propertyNamespace)
		{
			Trace.WriteLine(String.Format("Adapter.Adapter name: {0}", name));

			TransportProxy     = null;
			HandlerPropertyBag = null;
			Initialized        = false;

			this.Name               = name;
			this.Version            = version;
			this.Description        = description;
			this.TransportType      = transportType;
			this.ClassID              = clsid;

			this.PropertyNamespace  = propertyNamespace;
		}

		protected string            PropertyNamespace { get; }
	    public IBTTransportProxy    TransportProxy { get; private set; }
	    protected IPropertyBag      HandlerPropertyBag { get; private set; }
	    protected bool              Initialized { get; private set; }

	    //  IBTTransport
		public string Name { get; }

	    public string Version { get; }
	    public string Description { get; }
	    public string TransportType { get; }
	    public Guid ClassID { get; }

	    //  IBTransportControl
		public virtual void Initialize (IBTTransportProxy transportProxy)
		{
            Trace.WriteLine("Adapter.Initialize");

			//  this is a Singleton and this should only ever be called once
			if (Initialized)
				throw new AlreadyInitialized();				

			TransportProxy = transportProxy;
			Initialized = true;
		}
		public virtual void Terminate ()
		{
            Trace.WriteLine("Adapter.Terminate");

			if (!Initialized)
				throw new NotInitialized();
			
			TransportProxy = null;
		}

		protected virtual void HandlerPropertyBagLoaded ()
		{
			// let any derived classes know the property bag has now been loaded
		}

		// IPersistPropertyBag
		public void GetClassID (out Guid classid) { classid = ClassID; }
		public void InitNew () { }
		public void Load (IPropertyBag pb, int pErrorLog)
		{
            Trace.WriteLine("Adapter.Load");
            
            HandlerPropertyBag = pb;
			HandlerPropertyBagLoaded();
		}
		public void Save (IPropertyBag pb, bool fClearDirty, bool fSaveAllProperties) { }
	}
}
