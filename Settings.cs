using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Kitty
{
    public class Settings
    {
        public bool AddKittyExeToResults { get; set; } = true;
        public bool IsKittyPortable { get; set; } = false;
        public bool OpenKittySessionFullscreen { get; set; } = false;
        public bool PuttyInsteadOfKitty { get; set; } = false;
        public string KittyExePath { get; set; }
    }
}
