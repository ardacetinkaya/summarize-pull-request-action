public interface IRepository
{
    Task<string> GetPRDiff(string user, string repository, string pullRequestId);
}