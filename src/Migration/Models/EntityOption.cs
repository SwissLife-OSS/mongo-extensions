using System;
using System.Collections.Generic;

namespace MongoDB.Extensions.Migration;

public record EntityOption(
    Type Type,
    int CurrentVersion,
    List<IMigration> Migrations);
