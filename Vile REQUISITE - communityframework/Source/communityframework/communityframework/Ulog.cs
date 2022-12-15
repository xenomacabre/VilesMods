using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CF
{
    /// <summary>
    /// Utility Log. Convenient logging methods which automatically prefix
    /// themselves for identifiability, plus debug-only messages.
    /// </summary>
    class ULog
    {
        /// <summary>
        /// The name of our mod, which will be prefixed to our log messages.
        /// </summary>
        public static readonly string MOD_ID = "Community Framework";

        
        /// <summary>
        /// The prefix to start log messages with, so that end-users know which
        /// messages are caused by us.
        /// </summary>
        public static string Prefix => "[" + MOD_ID + "] ";

        // Whether or not the player has chosen to run the framework in debug
        // mode. This property is private since it's just a shorthand for a
        // property from another utility.
        private static bool DEBUG => CFSettings.DEBUG;

        /// <summary>
        /// Calls <c>Verse.Log.Message</c>, but prefixes the name of the
        /// framework for the benefit of the end-user.
        /// </summary>
        /// <param name="s">The message to write to the log.</param>
        public static void Message(string s)
        {
            Log.Message(Prefix + s);
        }

        /// <summary>
        /// Calls <c>Verse.Log.Warning</c>, but prefixes the name of the
        /// framework for the benefit of the end-user.
        /// </summary>
        /// <param name="s">The message to write to the log.</param>
        public static void Warning(string s)
        {
            Log.Warning(Prefix + s);
        }

        /// <summary>
        /// Calls <c>Verse.Log.Error</c>, but prefixes the name of the
        /// framework for the benefit of the end-user.
        /// </summary>
        /// <param name="s">The message to write to the log.</param>
        public static void Error(string s)
        {
            Log.Error(Prefix + s);
        }

        /// <summary>
        /// Calls <c>Verse.Log.Message</c>, but only if the framework is set to
        /// run in debug mode. Useful for displaying the current state of a 
        /// potentially-problematic system. Prefixes the name of the framework
        /// for the benefit of the end-user, if <c>addPrefix</c> is
        /// <c>true</c>.
        /// </summary>
        /// <param name="s">The message to write to the log.</param>
        /// <param name="addPrefix">
        /// If <c>true</c>, the message displayed will be prefixed with the
        /// name of the framework.
        /// </param>
        public static void DebugMessage(string s, bool addPrefix = true)
        {
            if (DEBUG) Log.Message((addPrefix ? Prefix : "") + s);
        }
    }
}
