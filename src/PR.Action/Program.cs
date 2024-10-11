using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;


IConfigurationRoot config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

Settings settings = config.Get<Settings>()
    ?? throw new Exception("Invalid Configuration");

if (!string.IsNullOrEmpty(settings.CommitSHA))
{
    IChatClient client = new ChatCompletionsClient(
        endpoint: new Uri(settings.URI),
        credential: new AzureKeyCredential(settings.APIKey)
        ).AsChatClient(settings.ModelId);

    var messages = new List<ChatMessage>(){
        new(
            Microsoft.Extensions.AI.ChatRole.System, 
            $$"""
            You are a software developer who know C# very well. You describe code changes for commits.
            Your descriptions are simple and clear so that they help developers to understand changes.
            Because you are a C# developer, you mainly focused on C# code and project file changes.
            Because you describe briefly, if there is more than 6 C# related file changes, just describe 6 files.
            You do descriptions in an order.
            """
        )
    };

    var repository = new GitHubRepository(settings.PAT);

    var diff = await repository.GetCommitChanges(
        settings.RepositoryAccount,
        settings.RepositoryName,
        settings.CommitSHA);

    messages.Add(new ChatMessage()
    {
        Role = Microsoft.Extensions.AI.ChatRole.User,
        Text = $$"""
        Describe the following commit and do for just files in given folder.

        <folder>sample-app</folder>

        <code>
        {{diff}}
        </code>
        """,
    });

    var result = await client.CompleteAsync(messages);

    System.Console.WriteLine(result);
}