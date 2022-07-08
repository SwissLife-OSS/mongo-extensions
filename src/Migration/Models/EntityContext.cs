using Microsoft.Extensions.Logging;

namespace MongoDB.Extensions.Migration;

public record EntityContext(EntityOption Option, ILoggerFactory LoggerFactory);
