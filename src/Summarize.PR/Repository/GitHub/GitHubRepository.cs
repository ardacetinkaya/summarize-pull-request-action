using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

public class GitHubRepository
{
    private readonly HttpClient _client;

    public GitHubRepository(string token)
    {
        _client = new HttpClient();
        //User-Agent should be defined or actions worker does not allow for a request
        _client.DefaultRequestHeaders.Add("User-Agent", "SummarizePRAction");
        _client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.diff");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
    }

    public async Task<string> GetCommitChanges(string repositoryAccount, string repositoryName, string commitSHA)
    {
        var response = await _client.GetAsync($"https://api.github.com/repos/{repositoryAccount}/{repositoryName}/commits/{commitSHA}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        return result;
    }

    public async Task PostComment(string comment, string repositoryAccount, string repositoryName, string pullRequestId)
    {

        var result = await _client.PostAsJsonAsync<Review>($"https://api.github.com/repos/{repositoryAccount}/{repositoryName}/issues/{pullRequestId}/comments", new Review
        {
            Body = comment
        });

        result.EnsureSuccessStatusCode();
    }
}

file class Review
{
    public string Body { get; set; }
}