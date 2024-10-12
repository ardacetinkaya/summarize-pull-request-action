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
    System.Console.WriteLine($"Repository account: {setting.RepositoryAccount}");
    System.Console.WriteLine($"Repository name: {setting.RepositoryName}");
    System.Console.WriteLine($"Commit: {setting.CommitSHA}");

    IChatClient client = new ChatCompletionsClient(
        endpoint: new Uri("https://models.inference.ai.azure.com"),
        credential: new AzureKeyCredential(settings.APIKey)
        ).AsChatClient(settings.ModelId);

    var messages = new List<ChatMessage>(){
        new(
            Microsoft.Extensions.AI.ChatRole.System,
            $$"""
            You are a software developer. You describe code changes for commits.
            Your descriptions are simple and clear so that they help developers to understand changes.
            Because you describe briefly, if there is more than 7 file changes, just describe 7 files.
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
        Describe the following commit and group descriptions per file.

        <code>
        {{diff}}
        </code>
        """,
    });

    var result = await client.CompleteAsync(messages);

    await repository.PostComment(result.Message.Text,
        settings.RepositoryAccount,
        settings.RepositoryName,
        settings.PullRequestId);

    System.Console.WriteLine("Commit changes are summarized");
}else{
    System.Console.WriteLine("Commit SHA is not provided, summarization is skipped");
}