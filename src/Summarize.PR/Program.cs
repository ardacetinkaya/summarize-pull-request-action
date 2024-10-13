using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Summarize.PR.Models;
using Summarize.PR.Repository;
using System.Net.Http.Headers;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

IConfiguration config = builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

builder.Services.Configure<Settings>(config);


builder.Services.AddHttpClient<GitHubRepository>("GitHub", (sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<Settings>>().Value;

    client.BaseAddress = new Uri("https://api.github.com/");

    // These headers are necessary for the GitHub API to recognize the request.
    client.DefaultRequestHeaders.Add("User-Agent", "SummarizePRAction");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.diff");

    // Authorization header with the Bearer token for authentication.
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.PAT.Trim());
});
builder.Services.AddTransient<IGitHubRepository, GitHubRepository>();

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<Settings>>().Value;

    return new ChatCompletionsClient(
        endpoint: new Uri("https://models.inference.ai.azure.com"),
        credential: new AzureKeyCredential(settings.APIKey)
    ).AsChatClient(settings.ModelId);
});

var app = builder.Build();

var settings = app.Services.GetRequiredService<IOptions<Settings>>().Value;

if (string.IsNullOrEmpty(settings.CommitSHA))
{
    Console.WriteLine("Commit SHA is not provided, summarization is skipped.");

    return;
}

Console.WriteLine($"Repository account: {settings.RepositoryAccount}");
Console.WriteLine($"Repository name: {settings.RepositoryName}");
Console.WriteLine($"Commit: {settings.CommitSHA}");
Console.WriteLine($"Model: {settings.ModelId}");

var client = app.Services.GetRequiredService<IChatClient>();

var repository = app.Services.GetRequiredService<IGitHubRepository>();

var messages = new List<ChatMessage>
{
    new(
        Microsoft.Extensions.AI.ChatRole.System,
        @"
            You are a software developer. You describe code changes for commits.
            Your descriptions are simple and clear so that they help developers to understand changes.
            Because you describe briefly, if there is more than 7 file changes, just describe 7 files.
            You do descriptions in an order.
        "
    )
};

var commitChanges = new CommitChanges
{
    CommitSHA = settings.CommitSHA,
    RepositoryName = settings.RepositoryName,
    RepositoryAccount = settings.RepositoryAccount
};

var diff = await repository.GetCommitChangesAsync(commitChanges);

messages.Add(new()
{
    Role = Microsoft.Extensions.AI.ChatRole.User,
    Text = $$"""
    Describe the following commit and group descriptions per file. 
    If there are some TODO notes in the commit also add them into your response.
    And also suggest some brief code for the TODO. 
    When suggesting the code also explain it in code description and also underline that it is just a suggestion and pseudo-code

    <code>
    {{diff}}
    </code>

    Response the description in this JSON format

    {
        "Comment": "___DESCRIPTION___",
        "Todos": [
            { 
                "Title":"___TODO_MESSAGE___",
                "Description": "___SUGGESTED_CODE_DESCRIPTION___",
                "Code":"___CODE_IN_MARKDOWN___"
            },
        ]
    }
    """,
});

var result = await client.CompleteAsync(messages, new ChatOptions
{
    ResponseFormat = ChatResponseFormat.Json,
    Temperature = 0
});

if (string.IsNullOrEmpty(result.Message.Text))
{
    Console.WriteLine("The commit message could not be retrieved by AI. Summarization is skipped.");

    return;
}

var answer = JsonSerializer.Deserialize<PRDescriptionAnswer>(result.Message.Text);
if (answer == null)
{
    Console.WriteLine("Invalid answer, summarization is skipped.");
    return;
}

var commitComment = new CommitComment
{
    Comment = answer.Comment,
    PullRequestId = settings.PullRequestId,
    RepositoryName = settings.RepositoryName,
    RepositoryAccount = settings.RepositoryAccount,
};

await repository.PostCommentAsync(commitComment);

Console.WriteLine("Commit changes are summarized.");

if (answer.Todos != null && answer.Todos.Count != 0)
{
    foreach (var todo in answer.Todos)
    {
        Console.WriteLine(todo.Title);
        Console.WriteLine(todo.Code);
        await repository.AddIssueAsync(new Issue
        {
            Title = todo.Title,
            Detail = @$"This is an auto-generated issue due to PR #{settings.PullRequestId}

{todo.Code}",
            RepositoryName = settings.RepositoryName,
            RepositoryAccount = settings.RepositoryAccount,
        });
        Console.WriteLine("There is some TODO(s) in commit, an issue is created to follow it");
    }
}