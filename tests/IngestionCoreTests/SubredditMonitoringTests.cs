using FluentAssertions;
using MassTransit.Testing;
using Reddit;
using RedditInfrastructure;

namespace IngestionCoreTests
{
    public class SubredditMonitoringTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void StopMonitoringWhenInactiveShouldThrow()
        {
            var harness = new InMemoryTestHarness();
            var redditClient = new RedditClient();
            var monitor = new SubredditMonitoring(harness.Bus, redditClient, "funny");

            monitor.Invoking(x => x.StopMonitoring())
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void DoubleSubscribeShouldThrow()
        {
            var harness = new InMemoryTestHarness();
            var redditClient = new RedditClient();
            var monitor = new SubredditMonitoring(harness.Bus, redditClient, "funny");

            monitor.StartMonitoring();

            monitor.Invoking(x => x.StartMonitoring())
                .Should().Throw<InvalidOperationException>();
        }
    }
}