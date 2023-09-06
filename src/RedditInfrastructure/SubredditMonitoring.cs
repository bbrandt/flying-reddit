using IngestionCore.Contracts;
using MassTransit;
using Reddit;
using Reddit.Controllers;
using System.Collections.Immutable;

namespace RedditInfrastructure;

/// <summary>
/// Subscribe to a subreddit using the Reddit.NET library.  
/// Reddit.NET uses some older .NET patterns and seems to be designed primarily 
/// for desktop apps so this is likely causing more plumbing code to have to be 
/// written compared to using HttpClient directly against the API.  Reddit.NET
/// was primarily selected because multiple subscriptions using the same Reddit
/// app are able to share the same rate limiting information.
/// </summary>
public class SubredditMonitoring
{
    private readonly IBus _bus;
    private readonly string _subredditName;
    private readonly SubredditPosts _posts;

    private bool _monitoringEnabled = false;

    public SubredditMonitoring(IBus bus, RedditClient redditClient, string subredditName)
    {
        _bus = bus;
        _subredditName = subredditName;
        var subreddit = redditClient.Subreddit(subredditName);
        _posts = subreddit.Posts;

    }

    public void StartMonitoring()
    {
        if (_monitoringEnabled)
        {
            throw new InvalidOperationException($"Monitoring is already running and cannot be restarted for {_subredditName}.");
        }

        // It seems with Reddit.NET's monitor behavior we are not able to accomplish the requirement of only including 
        // posts from after service start time and may need to take a look at a different approach or submit a PR to
        // the Reddit.NET repo.
        var topPosts = _posts.GetTop("day");
        PublishTopPosts(topPosts);
        _posts.TopUpdated += Posts_TopUpdated;

        _posts.MonitorTop();

        _monitoringEnabled = true;
    }

    private void PublishTopPosts(List<Post> topPosts)
    {
        var payload = topPosts.Select(p => new TopPost(p.Id, p.Title, p.Permalink, p.UpVotes)).ToImmutableList();

        _bus.Publish(new TopPostsUpdated(payload));
    }

    public void StopMonitoring()
    {
        if (!_monitoringEnabled)
        {
            throw new InvalidOperationException($"Monitoring is already stopped for {_subredditName}.");
        }

        _posts.TopUpdated -= Posts_TopUpdated;

        // Not clear naming, but calling this method again will toggle monitoring back off.
        _posts.MonitorTop();

        _monitoringEnabled = false;
    }

    private void Posts_TopUpdated(object? sender, Reddit.Controllers.EventArgs.PostsUpdateEventArgs e)
    {
        PublishTopPosts(e.NewPosts);
    }
}
