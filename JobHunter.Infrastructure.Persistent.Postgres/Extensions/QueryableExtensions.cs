using System.Linq.Expressions;
using JobHunter.Domain.Job.Dto;

namespace JobHunter.Infrastructure.Persistent.Postgres.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<SortModel> sortModels)
    {
        var expression = source.Expression;
        int count = 0;
        foreach (var item in sortModels.OrderBy(e=>e.Priority))
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var selector = Expression.PropertyOrField(parameter, item.FieldName);
            var method = string.Equals(item.Sort, "descend", StringComparison.OrdinalIgnoreCase) ?
                (count == 0 ? "OrderByDescending" : "ThenByDescending") :
                (count == 0 ? "OrderBy" : "ThenBy");
            expression = Expression.Call(typeof(Queryable), method,
                new Type[] { source.ElementType, selector.Type },
                expression, Expression.Quote(Expression.Lambda(selector, parameter)));
            count++;
        }
        return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
    }
}