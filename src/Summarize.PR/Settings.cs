public sealed class Settings
{
    public required string APIKey { get; set; }
    public required string ModelId { get; set; }
    public required string URI { get; set; }
    public required string RepositoryAccount { get; set; }
    public required string RepositoryName { get; set; }
    public required string CommitSHA { get; set; }
    public required string PullRequestId { get; set; }
    public required string PAT { get; set; }
}
