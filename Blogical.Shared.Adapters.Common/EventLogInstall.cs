using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Blogical.Shared.Adapters.Common
{
    [RunInstaller(true)]
    [ComVisible(false)]
    public class MyEventLogInstaller : Installer
    {
        public MyEventLogInstaller()
        {
            EventLogInstaller installer = new EventLogInstaller
            {
                Log = "Application",
                Source = EventLogSources.SftpAdapter
            };

            Installers.Add(installer);
        }
    }
}