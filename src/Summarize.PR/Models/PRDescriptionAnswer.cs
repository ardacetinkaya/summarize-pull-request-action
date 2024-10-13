namespace Summarize.PR.Models;

public record PRDescriptionAnswer(string Comment, IReadOnlyList<Todo> Todos);

public record Todo(string Title, string Code);