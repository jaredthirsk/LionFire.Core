﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Applications.Hosting
{
    public static class IAppHostExtensions
    {
        /// <summary>
        /// Adds a task to the application
        /// </summary>
        public static IAppHost Add(this IAppHost host, Action action, Func<bool> tryInitialize = null)
        {
            host.Add(new AppTask(action, tryInitialize));
            return host;
        }

        public static IAppHost AddConfig(this IAppHost host, Action<IAppHost> tryInitialize)
        {
            host.Add(new AppConfigurer(tryInitialize));
            return host;
        }

        public static IAppHost AddInit(this IAppHost host, Func<IAppHost, bool> tryInitialize)
        {
            host.Add(new AppInitializer(tryInitialize));
            return host;
        }
        public static IAppHost AddInit(this IAppHost host, Action<IAppHost> initialize)
        {
            host.Add(new AppInitializer(initialize));
            return host;
        }

        /// <summary>
        /// Start application and wait until all ApplicationTasks with WaitForComplete = true to complete.
        /// </summary>
        public static void RunAndWait(this IAppHost host, Func<Task> runMethod = null)
        {
            if (runMethod != null) host.Add(new AppTask(() => runMethod().Wait()));
            host.Run().Wait();
        }

        public static Task Run(this IAppHost host, Func<Task> runMethod)
        {
            if (runMethod != null) host.Add(new AppTask(() => runMethod().Wait()));
            return host.Run();
        }

        /// <summary>
        /// Initialize the services registered so far that are required for futher app composition
        /// </summary>
        //public static IAppHost Bootstrap(this IAppHost host)
        //{
        //    host.Bootstrap();
        //    return host;
        //}
        
    }

}