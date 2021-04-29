
using Guesser.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Guesser.Data.Repositories
{
    /// <summary>
    ///     Product repository.
    /// </summary>
    public class LoggingRepository : ILoggingRepository
    {
        private readonly ILogger<LoggingRepository> _log;

        public LoggingRepository(ILogger<LoggingRepository> log)
        {
            _log = log;
        }

        /// <summary>
        ///     Create a new log entry.
        /// </summary>
        /// <param name="log">The message to be logged.</param>
        /// <returns><see cref="Product"/>.</returns>
        public void CreateLogEntry(string message)
        {
            _log.LogInformation(message);
        }
    }
}
