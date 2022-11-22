using System;
using MongoDB.Driver;

namespace MongoDB.Prime.Extensions;

public static class FindFluentExtensions
{
    [Obsolete("Depreceated, use ToQueryString instead.")]
    public static string PrintQuery<TDocument, TProjection>(
        this IFindFluent<TDocument, TProjection> findFluent)
    {
        return findFluent.ToString() ?? string.Empty;
    }

    public static string ToQueryString<TDocument, TProjection>(
        this IFindFluent<TDocument, TProjection> findFluent)
    {
        return findFluent!.ToString() ?? string.Empty;        
    }
}
