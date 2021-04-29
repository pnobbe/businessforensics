
namespace Guesser.Core.Interfaces.Repositories
{
    /// <summary>
    ///     Operations for the Logging repository.
    /// </summary>
    public interface ILoggingRepository
    {
        void CreateLogEntry(string message);
    }
}
