using MassTransit;
using Microsoft.Extensions.Configuration;
using RedditInfrastructure.Configuration;
using SubredditIngestionService.ApplicationServices;
using UpstashInfrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<IngestionService>();

var appId = builder.Configuration["REDDIT_APP_ID"];
var refreshToken = builder.Configuration["REDDIT_REFRESH_TOKEN"];

ArgumentNullException.ThrowIfNull(appId);
ArgumentNullException.ThrowIfNull(refreshToken);

builder.Services.Configure<SubredditSubscription>(myOptions =>
{
    myOptions.DelimitedSubredditNames = builder.Configuration.GetValue("DELIMITED_SUBREDDIT_NAMES", string.Empty);
});

builder.Services.AddRedditClient(appId, refreshToken);

builder.Services.AddMassTransit(x =>
{
    // Replace with reliable/persistent transport in production
    x.UsingInMemory();
});

var redisConnectionString = builder.Configuration["REDIS_CONNECTION_STRING"];

ArgumentNullException.ThrowIfNull(redisConnectionString);

builder.Services.AddCaching(redisConnectionString);
builder.Services.AddDatastore(redisConnectionString);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapHealthChecks("/ingestion/healthz");

app.Run();
