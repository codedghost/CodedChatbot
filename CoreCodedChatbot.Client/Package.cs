using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Client.Interfaces;
using CoreCodedChatbot.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodedChatbot.Client
{
    public static class Package
    {
        public static IServiceCollection AddGuessingGameServices(this IServiceCollection services)
        {
            services.AddSingleton<IGuessingGameService, GuessingGameService>();

            return services;
        }
    }
}
