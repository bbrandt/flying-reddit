using MassTransit;
using Microsoft.Extensions.Options;
using Reddit;
using RedditInfrastructure;
using RedditInfrastructure.Configuration;

namespace SubredditIngestionService.ApplicationServices;

internal class IngestionService : BackgroundService
{
    private readonly RedditClient _redditClient;
    private readonly IOptionsSnapshot<SubredditSubscription> _subredditSubscription;
    private readonly IBus _bus;
    private readonly List<SubredditMonitoring> _subredditMonitors = new List<SubredditMonitoring>();

    public IngestionService(RedditClient redditClient, IOptionsSnapshot<SubredditSubscription> subredditSubscription, IBus bus)
    {
        ArgumentNullException.ThrowIfNull(redditClient);
        ArgumentNullException.ThrowIfNull(subredditSubscription);

        _redditClient = redditClient;
        _subredditSubscription = subredditSubscription;
        _bus = bus;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SubscribeToConfiguredSubreddits();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }

        UnsubscribeFromSubreddits();
    }

    private void SubscribeToConfiguredSubreddits()
    {
        var subreddits = _subredditSubscription.Value?.DelimitedSubredditNames?.Split("|") ?? new[] { "funny" };

        foreach (var subreddit in subreddits)
        {
            SubscribeToSubreddit(subreddit);
        }
    }

    private void SubscribeToSubreddit(string subredditName)
    {
        var monitor = new SubredditMonitoring(_bus, _redditClient, subredditName);
        monitor.StartMonitoring();
        _subredditMonitors.Add(monitor);
    }

    private void UnsubscribeFromSubreddits()
    {
        foreach (var monitor in _subredditMonitors)
        {
            monitor.StopMonitoring();
        }
    }
}
