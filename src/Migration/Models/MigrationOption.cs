using System.Collections.Generic;

namespace MongoDB.Extensions.Migration;

public record MigrationOption(List<EntityOption> EntityOptions);
