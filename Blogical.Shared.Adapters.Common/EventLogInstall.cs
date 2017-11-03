using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace Blogical.Shared.Adapters.Common
{
    [RunInstaller(true)]
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