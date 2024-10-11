using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

public class GitHubRepository : IRepository
{
    private readonly HttpClient _client;
    
    public GitHubRepository(string token)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.diff");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<string> GetPRDiff(string user, string repository, string pullRequestId)
    {
        var response = await _client.GetAsync($"https://api.github.com/repos/{user}/{repository}/pulls/{pullRequestId}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        return result;
    }
}