namespace Summarize.PR.Repository;

using Summarize.PR.Models;
using System.Net.Http.Json;

public class GitHubRepository(IHttpClientFactory clientFactory) : IGitHubRepository
{
	private readonly HttpClient _client = clientFactory.CreateClient("GitHub");
	/// <summary>
	/// Fetches the commit changes from the GitHub API using the commit SHA.
	/// </summary>
	/// <param name="commitChanges">Commit changes model.</param>
	public async Task<string> GetCommitChangesAsync(CommitChanges commitChanges)
	{
		// Build the request URL using string interpolation and send GET request to GitHub API.
		using var response = await _client.GetAsync(
			$"repos/{commitChanges.RepositoryAccount}/{commitChanges.RepositoryName}/commits/{commitChanges.CommitSHA}"
		);

		// Ensure the response is successful (throws an exception if the status code is not 2xx).
		response.EnsureSuccessStatusCode();

		// Read and return the response content as a string (the diff of the commit).
		return await response.Content.ReadAsStringAsync();
	}

	/// <summary>
	/// Posts a comment on a GitHub pull request.
	/// </summary>
	/// <param name="commitComment">Commit comment model</param>
	public async Task PostCommentAsync(CommitComment commitComment)
	{
		// Create a new review object with the comment body.
		Review review = new(commitComment.Comment);

		// Send a POST request to GitHub API with the review object in JSON format.
		using var response = await _client.PostAsJsonAsync(
			$"repos/{commitComment.RepositoryAccount}/{commitComment.RepositoryName}/issues/{commitComment.PullRequestId}/comments",
			review
		);

		// Ensure the response is successful.
		response.EnsureSuccessStatusCode();
	}
}