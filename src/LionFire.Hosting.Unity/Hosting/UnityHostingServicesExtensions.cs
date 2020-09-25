﻿using LionFire.Dispatching;
using LionFire.Services;
using LionFire.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LionFire.Hosting.Unity
{
    public static class UnityHostingServicesExtensions
    {
        //public static IHostBuilder AddUnityRuntime(this IHostBuilder hostBuilder, MonoBehaviour monoBehaviour)
        //    => hostBuilder.ConfigureServices(services => services.AddUnityRuntime(monoBehaviour));

        public static IServiceCollection AddUnityRuntime(this IServiceCollection services, MonoBehaviour monoBehaviour, bool logStartStop = false)
            => services
                //.AddUnityEngine()
                .AddSingleton<IDispatcher>(_ => new UnityThreadDispatcherWrapper())
                .If(logStartStop, s => s.AddHostedService<UnityStartStopLogger>())
                .If(monoBehaviour != null, s =>
                {
                    //monoBehaviour.gameObject.AddComponent<UnityDispatcher>(); // OLD - Not needed - UnityDispatcher creates itself
                    s.AddSingleton<MonoBehaviour>(monoBehaviour);
                }) // Useful for registering coroutines/repeating methods?
                ;
    }
}
