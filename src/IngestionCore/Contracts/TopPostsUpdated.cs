using System.Collections.Immutable;

namespace IngestionCore.Contracts;

public record class TopPostsUpdated(IEnumerable<TopPost> TopPosts);
