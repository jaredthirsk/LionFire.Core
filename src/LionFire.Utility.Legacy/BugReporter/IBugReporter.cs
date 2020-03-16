﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Text;
using System.Threading.Tasks;


namespace LionFire.Applications
{
    public interface IBugReporter
    {
        bool IsEnabled { get; set; }

#if !UNITY
        /// <summary>
        /// Return true if handled, false otherwise.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        bool OnApplicationDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs args);
#endif
    }

    public class NullBugReporter : IBugReporter
    {
        public bool IsEnabled { get; set; }

#if !UNITY
        bool OnApplicationDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs args) { return IsEnabled; }


        bool IBugReporter.OnApplicationDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            return IsEnabled;
        }
#endif
    }
}
