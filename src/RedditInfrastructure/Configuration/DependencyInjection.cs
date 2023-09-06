using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reddit;

namespace RedditInfrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddRedditClient(this IServiceCollection services, string appId, string refreshToken)
    {


        services.AddTransient(sp => new RedditClient(appId, refreshToken));
        return services;
    }
}
