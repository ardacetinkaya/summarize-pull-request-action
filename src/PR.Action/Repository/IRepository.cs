public interface IRepository
{
    Task<string> GetCommitChanges(string repositoryAccount, string repositoryName, string commitSHA);
}