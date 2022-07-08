using Microsoft.Extensions.Logging;

namespace MongoDB.Extensions.Migration;

public record MigrationContext(MigrationOption Option, ILoggerFactory LoggerFactory);
