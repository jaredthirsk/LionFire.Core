﻿using LionFire.Applications;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Services
{
    public static class LionFireAppServicesExtensions
    {
        public static IServiceCollection AddLionFireApp(this IServiceCollection services)
        {
            services
                .AddSingleton<ILionFireApp, LionFireApp>()
                .AddHostedService(p => p.GetRequiredService<ILionFireApp>())
                .AddHostedService<ApplicationTelemetry>()
                ;

            return services;
        }
    }
}
