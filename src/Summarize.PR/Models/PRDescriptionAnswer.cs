namespace Summarize.PR.Models;

public record PRDescriptionAnswer
{
    public required string Comment { get; set; }

    public List<string>? Todos { get; set; }
}