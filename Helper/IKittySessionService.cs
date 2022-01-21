using System.Collections.Generic;
using Flow.Launcher.Plugin.Kitty.Helper;

namespace Flow.Launcher.Plugin.Kitty.Helper
{
    public interface IKittySessionService
    {
        /// <summary>
        /// Returns a List of all Kitty Sessions
        /// </summary>
        /// <returns>A List of all Kitty Sessions</returns>
        IEnumerable<KittySession> GetAll(Settings settings, PluginInitContext context);
    }
}
