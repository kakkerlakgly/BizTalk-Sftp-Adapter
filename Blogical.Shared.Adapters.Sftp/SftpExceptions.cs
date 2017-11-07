using System;

namespace Blogical.Shared.Adapters.Sftp
{
    [Serializable]
    internal class SftpException : ApplicationException
	{
	    public SftpException (string msg) : base(msg) { }

	    public SftpException (string msg, Exception e) : base(msg, e) { }
	}
}

