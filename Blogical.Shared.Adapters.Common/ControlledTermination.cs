//---------------------------------------------------------------------
// File: ControlledTermination.cs
// 
// Summary: Implementation of an adapter framework sample adapter. 
// This class constitutes one of the BaseAdapter classes, which, are
// a set of generic re-usable set of classes to help adapter writers.
//
// Sample: Base Adapter Class Library v1.0.1
//
// Description: This class is used to keep count of work in flight, an
// adapter should not return from terminate if it has work outstanding
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
using System.Threading;

namespace Blogical.Shared.Adapters.Common
{
    public class ControlledTermination : IDisposable
    {
        private readonly AutoResetEvent _e = new AutoResetEvent(false);
        private int _activityCount;
        private bool _terminate;

        //  to be called at the start of the activity
        //  returns false if terminate has been called
        public bool Enter ()
        {
            lock (this)
            {
                if (_terminate)
                {
                    return false;
                }

                _activityCount++;
            }
            return true;
        }

        //  to be called at the end of the activity
        public void Leave ()
        {
            lock (this)
            {
                _activityCount--;

                // Set the event only if Terminate() is called
                if (_activityCount == 0 && _terminate)
                    _e.Set();
            }
        }

        //  this method blocks waiting for any activity to complete
        public void Terminate ()
        {
            bool result;

            lock (this)
            {
                _terminate = true;
                result = _activityCount == 0;
            }

            // If activity count was not zero, wait for pending activities
            if (!result)
            {
                _e.WaitOne();
            }
        }

        public bool TerminateCalled
        {
            get
            { 
                lock (this)
                {
                    return _terminate;
                }
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _e.Dispose();
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
