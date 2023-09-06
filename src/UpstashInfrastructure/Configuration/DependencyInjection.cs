using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZiggyCreatures.Caching.Fusion;

namespace UpstashInfrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddCaching(this IServiceCollection services, string redisConnectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "SubredditIngestion";
        });

        services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(2)
            });
        return services;
    }

    public static IServiceCollection AddDatastore(this IServiceCollection services, string redisConnectionString)
    {
        return services;
    }
}
