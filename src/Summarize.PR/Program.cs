using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Summarize.PR.Models;
using Summarize.PR.Repository.GitHub;

IConfigurationRoot config = new ConfigurationBuilder()
	.AddEnvironmentVariables()
	.AddCommandLine(args)
	.Build();

Settings settings = config.Get<Settings>()
	?? throw new Exception("Invalid configuration.");

if (!string.IsNullOrEmpty(settings.CommitSHA))
{
	Console.WriteLine($"Repository account: {settings.RepositoryAccount}");
	Console.WriteLine($"Repository name: {settings.RepositoryName}");
	Console.WriteLine($"Commit: {settings.CommitSHA}");
	Console.WriteLine($"Model: {settings.ModelId}");

	if (!string.IsNullOrEmpty(settings.APIKey))
	{
		IChatClient client = new ChatCompletionsClient(
			endpoint: new("https://models.inference.ai.azure.com"),
			credential: new AzureKeyCredential(settings.APIKey)
		).AsChatClient(settings.ModelId);

		List<ChatMessage> messages = [
			new(
				Microsoft.Extensions.AI.ChatRole.System,
				@"
					You are a software developer. You describe code changes for commits.
					Your descriptions are simple and clear so that they help developers to understand changes.
					Because you describe briefly, if there is more than 7 file changes, just describe 7 files.
					You do descriptions in an order.
				"
			)
		];

		GitHubRepository repository = new(settings.PAT);

		CommitChanges commitChanges = new()
		{
			CommitSHA = settings.CommitSHA,
			RepositoryName = settings.RepositoryName,
			RepositoryAccount = settings.RepositoryAccount
		};

		string diff = await repository.GetCommitChangesAsync(commitChanges);

		messages.Add(new()
		{
			Role = Microsoft.Extensions.AI.ChatRole.User,
			Text = $$"""
			Describe the following commit and group descriptions per file.

			<code>
			{{diff}}
			</code>
			""",
		});

		ChatCompletion? result = await client.CompleteAsync(messages);

		CommitComment commitComment = new()
		{
			Comment = result.Message.Text ?? "",
			PullRequestId = settings.PullRequestId,
			RepositoryName = settings.RepositoryName,
			RepositoryAccount = settings.RepositoryAccount,
		};

		await repository.PostCommentAsync(commitComment);

		Console.WriteLine("Commit changes are summarized.");
	}
	else
	{
		Console.WriteLine("API Key is not provided, ChatClient invocations are skipped.");
	}
}
else
{
	Console.WriteLine("Commit SHA is not provided, summarization is skipped.");
}
