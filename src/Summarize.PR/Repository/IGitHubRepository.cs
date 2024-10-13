namespace Summarize.PR.Repository;

using Summarize.PR.Models;

public interface IGitHubRepository
{
	Task PostCommentAsync(CommitComment commitComment);

	Task<string> GetCommitChangesAsync(CommitChanges commitChanges);
}