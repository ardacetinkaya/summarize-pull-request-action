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
    You are a software developer who knoe C# very well.
    """)
};

messages.Add(new ChatMessage(){
    Role= Microsoft.Extensions.AI.ChatRole.User,
    Text = "Tell me about .NET 9"
});
 var result = await client.CompleteAsync(messages);

 System.Console.WriteLine(result);

