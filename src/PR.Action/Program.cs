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

IChatClient client = new ChatCompletionsClient(
    endpoint: new Uri(settings.URI),
    credential: new AzureKeyCredential(settings.APIKey)
    ).AsChatClient(settings.ModelId);

var messages = new List<ChatMessage>(){
    new(Microsoft.Extensions.AI.ChatRole.System, $$"""
    You are a software developer who know C# very well.
    """)
};

var repository = new GitHubRepository(settings.PAT);
if (!string.IsNullOrEmpty(settings.CommitSHA))
{

    var diff = await repository.GetCommitChanges(
        settings.RepositoryAccount,
        settings.RepositoryName,
        settings.CommitSHA);

    messages.Add(new ChatMessage()
    {
        Role = Microsoft.Extensions.AI.ChatRole.User,
        Text = $$"""
    Tell me about the following changes so that when I read the code, it helps me to understand better.
    Just tell me changes in c# files. If there are more than 5 c# files, just do this for 5 files. 
    List them in correct order.

    And do this for just files in given folder.

    <folder>sample-app</folder>

    <code>
    {{diff}}
    </code>
    """,
    });

    var result = await client.CompleteAsync(messages);

    System.Console.WriteLine(result);
}