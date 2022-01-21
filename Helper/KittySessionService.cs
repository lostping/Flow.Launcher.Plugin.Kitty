using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.Kitty.Helper
{
    public class KittySessionService : IKittySessionService
    {
        /// <summary>
        /// Returns a List of all Kitty Sessions
        /// </summary>
        /// <returns>A List of all Kitty Sessions</returns>
        public IEnumerable<KittySession> GetAll(Settings settings, PluginInitContext context)
        {

            var results = new List<KittySession>();


            if (settings.IsKittyPortable)
            {
                string sessionsPath = Path.Combine(Path.GetDirectoryName(settings.KittyExePath), "Sessions"); // live version
                // string sessionsPath = Path.Combine(settings.KittyExePath, "Sessions"); // debug only
                // let's read the portable Session folder if it exists
                if (Directory.Exists(sessionsPath))
                {
                    try
                    {
                        string[] files = Directory.GetFiles(sessionsPath);
                        foreach (var file in files)
                        {
                            string filename = Path.GetFileName(file);

                            // skip default settings entry
                            if(filename.Contains("Default%20Settings", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            try
                            {
                                string SessionFileContent = File.ReadAllText(Path.Combine(sessionsPath, file));
                                Match matchesH = KittyRegex.theHostnameIP.Match(SessionFileContent);
                                Match matchesP = KittyRegex.theProtocol.Match(SessionFileContent);
                                Match matchesU = KittyRegex.theUsername.Match(SessionFileContent);
                                results.Add(new KittySession
                                {
                                    Identifier = filename,
                                    Protocol = matchesP.Groups["Protocol"].ToString(),
                                    Username = matchesU.Groups["UserName"].ToString(),
                                    Hostname = matchesH.Groups["HostName"].ToString(),
                                });
                            }
                            catch (Exception)
                            {
                                // If there is any exception related to the file access, just do nothing for that key, but don't let the whole results fails.
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // If there's an exception no files could be found in the session directory
                        string trError = context.API.GetTranslation("flowlauncher_plugin_kitty_error");
                        context.API.ShowMsg(trError, sessionsPath, "");
                    }
                }
                else
                {
                    // no session folder found
                    string trSessMiss = context.API.GetTranslation("flowlauncher_plugin_kitty_sessionFolderMissing");
                    context.API.ShowMsg(trSessMiss, sessionsPath, "");
                }
            }
            else
            {
                switch (settings.PuttyInsteadOfKitty)
                {
                    case true:
                        // let's read the registry key for Putty sessions
                        using (var root = Registry.CurrentUser.OpenSubKey("Software\\SimonTatham\\PuTTY\\Sessions"))
                        {
                            if (root == null)
                            {
                                return results;
                            }

                            foreach (var subKey in root.GetSubKeyNames())
                            {
                                using (var KittySessionSubKey = root.OpenSubKey(subKey))
                                {
                                    if (KittySessionSubKey == null)
                                    {
                                        continue;
                                    }

                                    // skip default settings
                                    if (subKey.Contains("Default%20Settings", StringComparison.OrdinalIgnoreCase))
                                    {
                                        continue;
                                    }

                                    try
                                    {
                                        results.Add(new KittySession
                                        {
                                            Identifier = Uri.UnescapeDataString(subKey),
                                            Protocol = KittySessionSubKey.GetValue("Protocol").ToString(),
                                            Username = KittySessionSubKey.GetValue("UserName").ToString(),
                                            Hostname = KittySessionSubKey.GetValue("HostName").ToString(),
                                        });
                                    }
                                    catch (Exception)
                                    {
                                        // If there is any exception related to the registry access, just do nothing for that key, but don't let the whole results fails.
                                    }
                                }
                            }
                        }
                        break;
                    case false:
                        // let's read the registry key for Kitty sessions
                        using (var root = Registry.CurrentUser.OpenSubKey("Software\\9bis.com\\KiTTY\\Sessions"))
                        {
                            if (root == null)
                            {
                                return results;
                            }

                            foreach (var subKey in root.GetSubKeyNames())
                            {
                                using (var KittySessionSubKey = root.OpenSubKey(subKey))
                                {
                                    if (KittySessionSubKey == null)
                                    {
                                        continue;
                                    }

                                    // skip default settings
                                    if (subKey.Contains("Default%20Settings", StringComparison.OrdinalIgnoreCase))
                                    {
                                        continue;
                                    }

                                    try
                                    {
                                        results.Add(new KittySession
                                        {
                                            Identifier = Uri.UnescapeDataString(subKey),
                                            Protocol = KittySessionSubKey.GetValue("Protocol").ToString(),
                                            Username = KittySessionSubKey.GetValue("UserName").ToString(),
                                            Hostname = KittySessionSubKey.GetValue("HostName").ToString(),
                                        });
                                    }
                                    catch (Exception)
                                    {
                                        // If there is any exception related to the registry access, just do nothing for that key, but don't let the whole results fails.
                                    }
                                }
                            }
                        }
                        break;
                    default:
                }
            }

            return results;
        }
    }
}
