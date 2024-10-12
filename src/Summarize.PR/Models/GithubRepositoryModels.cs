namespace Summarize.PR.Models;

public record Review(string body);

public record CommitChanges
{
	public required string CommitSHA { get; set; }
	public required string RepositoryName { get; set; }
	public required string RepositoryAccount { get; set; }
}

public record CommitComment
{
	public required string Comment { get; set; }
	public required string PullRequestId { get; set; }
	public required string RepositoryName { get; set; }
	public required string RepositoryAccount { get; set; }
}

public record Settings
{
	public required string PAT { get; set; }
	public required string APIKey { get; set; }
	public required string ModelId { get; set; }
	public required string CommitSHA { get; set; }
	public required string PullRequestId { get; set; }
	public required string RepositoryName { get; set; }
	public required string RepositoryAccount { get; set; }
}