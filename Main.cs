using Flow.Launcher.Plugin.Kitty.Helper;
using Flow.Launcher.Plugin.Kitty.ViewModels;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System;

namespace Flow.Launcher.Plugin.Kitty
{
    public class Kitty : IPlugin, ISettingProvider, IPluginI18n
    {
        private PluginInitContext _context;
        private Settings _settings;
        public IKittySessionService KittySessionService { get; set; }
        public enum KittyEntryType { None, KittyDefault, UserEntry, UnknownHost}

        public Kitty()
        {
            KittySessionService = new KittySessionService();
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            // Add empty entry if option to add kitty exe to results was checked
            if (_settings.AddKittyExeToResults)
                results.Add(CreateResult(_settings.PuttyInsteadOfKitty, KittyEntryType.None));

            // Add all session entries if no search query was entered
            var querySearch = query.Search;
            if (string.IsNullOrEmpty(querySearch))
            {
                var allKittySessions =
                    KittySessionService
                    .GetAll(_settings, _context)
                    .Select(kittySession => CreateResult(_settings.PuttyInsteadOfKitty, KittyEntryType.UserEntry, kittySession.Identifier, kittySession.ToString(), kittySession:kittySession));

                return results.Concat(allKittySessions).ToList();
            }

            // Filter sessions for matches
            var queryKittySessions =
                KittySessionService.GetAll(_settings, _context)
                .Where(session => session.Identifier.ToLowerInvariant().Contains(querySearch.ToLowerInvariant()));

            // If no filtered elements found return result to connect kitty to the entered hostname (or IP)
            if (!queryKittySessions.Any())
            {
                var session = _context.API.GetTranslation("flowlauncher_plugin_kitty_settings_startSession");
                results.Add(CreateResult(_settings.PuttyInsteadOfKitty, KittyEntryType.UnknownHost, querySearch, $"{session} {querySearch}", 60));

                return results;
            }

            // If filtered sessions matched add them to the results
            foreach (var kittySession in queryKittySessions)
            {
                // process default kitty session slightly different than the others
                if (kittySession.Identifier == "Default%20Settings")
                {
                    results.Add(CreateResult(_settings.PuttyInsteadOfKitty, KittyEntryType.KittyDefault, "", kittySession.ToString(), kittySession:kittySession));
                } 
                else
                {
                    // silently skip sessions without hostname or ip address
                    if (!string.IsNullOrEmpty(kittySession.Hostname))
                    {
                        results.Add(CreateResult(_settings.PuttyInsteadOfKitty, KittyEntryType.UserEntry, kittySession.Identifier, kittySession.ToString(), kittySession: kittySession));
                    }
                }
            }

            return results;
        }

        public string GetTranslatedPluginTitle()
        {
            return _context.API.GetTranslation("flowlauncher_plugin_kitty_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return _context.API.GetTranslation("flowlauncher_plugin_kitty_plugin_description");
        }

        public Control CreateSettingPanel()
        {
            return new KittySettings(_context, _settings, new SettingsViewModel(_settings, _context));
        }

        /// <summary>
        /// Creates a new Result item
        /// </summary>
        /// <returns>A Result object containing the KittySession identifier and its connection string</returns>
        private Result CreateResult(
            bool PuttyOrKitty, KittyEntryType ket, string title = "", string subTitle = "", int score = 50, KittySession kittySession = null)
        {

            #region translations
            if (string.IsNullOrEmpty(title))
            {
                title = _context.API.GetTranslation("flowlauncher_plugin_kitty_settings");
            }
            if (string.IsNullOrEmpty(subTitle))
            {
                subTitle = _context.API.GetTranslation("flowlauncher_plugin_kitty_openConfigs");
            }

            string appName = "";
            appName = PuttyOrKitty ? "Putty" : "Kitty";

            string trStart = _context.API.GetTranslation("flowlauncher_plugin_kitty_start");
            string trOpen = _context.API.GetTranslation("flowlauncher_plugin_kitty_open");
            string trWo = _context.API.GetTranslation("flowlauncher_plugin_kitty_without");
            string trDefault = _context.API.GetTranslation("flowlauncher_plugin_kitty_default");
            string trSession = _context.API.GetTranslation("flowlauncher_plugin_kitty_session");
            string trSettings = _context.API.GetTranslation("flowlauncher_plugin_kitty_settings");

            // fix defaults settings i18n display in results
            if (title == "Default%20Settings")
            {
                title = trDefault + " " + trSettings;
                subTitle = String.Join(" ", trOpen, trDefault, appName, trSession);
            }
            #endregion

            switch (ket)
            {
                case KittyEntryType.None:
                    return new Result
                    {
                        Title = $"{trStart} {appName}",
                        SubTitle = $"{trOpen} {appName} {trWo} {trSession}",
                        IcoPath = "icon.png",
                        Action = context => LaunchKittySession(PuttyOrKitty, ket, string.Empty),
                        Score = score = 0
                    };
                case KittyEntryType.KittyDefault:
                    // KittyDefault starts Default Session
                    return new Result
                    {
                        Title = $"{trDefault} {trSettings}",
                        SubTitle = $"{trOpen} {trDefault} {appName} {trSession}",
                        IcoPath = "icon.png",
                        Action = context => LaunchKittySession(PuttyOrKitty, ket, "Default%20Settings"),
                        Score = score = 0
                    };
                case KittyEntryType.UserEntry:
                    // UserEntry starts selected session
                    return new Result
                    {
                        Title = title,
                        SubTitle = subTitle,
                        IcoPath = "icon.png",
                        Action = context => LaunchKittySession(PuttyOrKitty, ket, title, kittySession),
                        Score = score
                    };
                case KittyEntryType.UnknownHost:
                    // UnknownHost starts session with given hostname / ip address
                    return new Result
                    {
                        Title = title,
                        SubTitle = subTitle,
                        IcoPath = "icon.png",
                        Action = context => LaunchKittySession(PuttyOrKitty, ket, title),
                        Score = score
                    };
                default:
                    // this case shouldn't happen, at least we planned not to - so we skip silently to default
                    return new Result
                    {
                        Title = $"{trStart} {appName}",
                        SubTitle = $"{trOpen} {appName} {trWo} {trSession}",
                        IcoPath = "icon.png",
                        Action = context => LaunchKittySession(PuttyOrKitty, ket, string.Empty),
                        Score = score = 0
                    };
            }
        }

        /// <summary>
        /// Launches a new Putty session
        /// </summary>
        /// <returns>If launching Putty succeeded</returns>
        private bool LaunchKittySession(bool PuttyOrKitty, KittyEntryType ket, string session, KittySession kittySession = null)
        {
            try
            {
                var kittyPath = _settings.KittyExePath;
                string sessionsPath = Path.Combine(Path.GetDirectoryName(_settings.KittyExePath), "Sessions");
                if (string.IsNullOrEmpty(_settings.KittyExePath))
                {
                    // output a message if path was not set to explain why next message shows up (exception)
                    string trPathNotSet = _context.API.GetTranslation("flowlauncher_plugin_kitty_pathNotSet");
                    string trConfigPlugin = _context.API.GetTranslation("flowlauncher_plugin_kitty_configPlugin");
                    _context.API.ShowMsg(trPathNotSet, trConfigPlugin, "");
                }

                var p = new Process { StartInfo = { FileName = kittyPath } };


                switch (ket)
                {
                    case KittyEntryType.None:
                        p.StartInfo.Arguments = "";
                        break;
                    case KittyEntryType.KittyDefault:
                        if (PuttyOrKitty)
                        {
                            p.StartInfo.Arguments = "-load \"Settings\""; // start session the putty way (registry)
                        } 
                        else
                        {
                            if (_settings.IsKittyPortable)
                            {
                                p.StartInfo.Arguments = "-kload \"" + Path.Combine(sessionsPath, "Default%20Settings") + "\""; // start session the kitty way (portable)
                            }
                            else
                            {
                                p.StartInfo.Arguments = "-kload \"Default%20Settings\""; // start session the kitty way (registry)
                            }
                        }
                        break;
                    case KittyEntryType.UserEntry:
                        if (PuttyOrKitty)
                        {
                            p.StartInfo.Arguments = "-load \"" + kittySession.Identifier + "\""; // start session the putty way (registry)
                        } 
                        else
                        {
                            if (_settings.IsKittyPortable)
                            {
                                p.StartInfo.Arguments = "-kload \"" + Path.Combine(sessionsPath, kittySession.Identifier) + "\""; // start session the kitty way (portable)
                            }
                            else
                            {
                                p.StartInfo.Arguments = "-kload \"" + kittySession.Identifier + "\""; // start session the kitty way (registry)
                            }
                        }
                        break;
                    case KittyEntryType.UnknownHost:
                        p.StartInfo.Arguments = "-ssh \"" + session + "\""; // unknown host defaults to ssh (sorry, if you want another protocol, create a session and save it)
                        break;
                    default:
                        break;
                }

                if (_settings.OpenKittySessionFullscreen)
                {
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                }

                p.Start();

                return true;

            }
            catch (Exception ex)
            {
                string trError = _context.API.GetTranslation("flowlauncher_plugin_kitty_error");
                // Report the exception to the user. No further actions required
                _context.API.ShowMsg(trError + ": " + kittySession?.Identifier ?? session + " (" + _settings.KittyExePath + ") ", ex.Message, "");

                return false;
            }
        }
    }
}