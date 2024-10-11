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

System.Console.WriteLine($"PR Id:{settings.PullRequestId}");

IChatClient client = new ChatCompletionsClient(
    endpoint: new Uri(settings.URI),
    credential: new AzureKeyCredential(settings.APIKey)
    ).AsChatClient(settings.ModelId);

var messages = new List<ChatMessage>(){
    new(Microsoft.Extensions.AI.ChatRole.System, $$"""
    You are a software developer who knoe C# very well.
    """)
};
System.Console.WriteLine(settings.PAT);
System.Console.WriteLine("PAT");
var repository = new GitHubRepository(settings.PAT);
var diff = await repository.GetPRDiff("ardacetinkaya", "pull-request-action", settings.PullRequestId);
System.Console.WriteLine(diff);
messages.Add(new ChatMessage()
{
    Role = Microsoft.Extensions.AI.ChatRole.User,
    Text = $$"""
    Tell me about the following changes so that when I read the code, it help to understand better. 
    List them in correct order.

    <code>
    {{diff}}
    </code>
    """,
});

var result = await client.CompleteAsync(messages);

System.Console.WriteLine(result);