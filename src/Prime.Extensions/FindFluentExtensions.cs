using MongoDB.Driver;

namespace MongoDB.Prime.Extensions
{
    public static class FindFluentExtensions
    {
        public static string PrintQuery<TDocument, TProjection>(
            this IFindFluent<TDocument, TProjection> findFluent)
        {
            return findFluent.ToString();
        }
    }
}
