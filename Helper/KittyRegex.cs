using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.Kitty.Helper
{
    internal class KittyRegex
    {
        /// <summary>
        ///  A description of the regular expression:
        ///  
        ///  Match expression but don't capture it. [HostName]
        ///      HostName
        ///          HostName
        ///  Literal \
        ///  [HostName]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  Literal \
        ///  
        ///
        /// </summary>
        public static Regex theHostnameIP = new Regex(
              "(?:HostName)\\\\(?<HostName>.*)\\\\",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

        /// <summary>
        ///  Regular expression built for C# on: Fr, Jan 14, 2022, 12:00:19 
        ///  Using Expresso Version: 3.0.4750, http://www.ultrapico.com
        ///  
        ///  A description of the regular expression:
        ///  
        ///  Match expression but don't capture it. [Protocol]
        ///      Protocol
        ///          Protocol
        ///  Match expression but don't capture it. [\\]
        ///      Literal \
        ///  [Protocol]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  Match expression but don't capture it. [\\]
        ///      Literal \
        ///  
        ///
        /// </summary>
        public static Regex theProtocol = new Regex(
              "(?:Protocol)(?:\\\\)(?<Protocol>.*)(?:\\\\)",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );


        /// <summary>
        ///  Regular expression built for C# on: Fr, Jan 14, 2022, 12:02:01 
        ///  Using Expresso Version: 3.0.4750, http://www.ultrapico.com
        ///  
        ///  A description of the regular expression:
        ///  
        ///  Match expression but don't capture it. [UserName]
        ///      UserName
        ///          UserName
        ///  Match expression but don't capture it. [\\]
        ///      Literal \
        ///  [Username]: A named capture group. [.*]
        ///      Any character, any number of repetitions
        ///  Match expression but don't capture it. [\\]
        ///      Literal \
        ///  
        ///
        /// </summary>
        public static Regex theUsername = new Regex(
              "(?:UserName)(?:\\\\)(?<Username>.*)(?:\\\\)",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
    }
}
