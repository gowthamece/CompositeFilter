using System.Linq.Expressions;

namespace CompositeFilter.Utility
{
    public class CompositeFilter<T>
    {
    
            private const string eq = "eq";

            public static IQueryable<T> ApplyFilter(IQueryable<T> query, RootFilter filter)
            {
                if (filter == null || filter.Filters == null || !filter.Filters.Any())
                    return query;

                Expression<Func<T, bool>> compositeFilterExpression = null;

                if (filter.Logic?.ToLower() == "and")
                {
                    compositeFilterExpression = GetAndFilterExpression(filter.Filters);
                }
                else if (filter.Logic?.ToLower() == "or")
                {
                    compositeFilterExpression = GetOrFilterExpression(filter.Filters);
                }

                return compositeFilterExpression != null
                    ? query.Where(compositeFilterExpression)
                    : query;
            }

            private static Expression<Func<T, bool>> GetAndFilterExpression(List<Filter> filters)
            {
                if (filters == null || !filters.Any())
                    return null;

                var parameter = Expression.Parameter(typeof(T), "x");
                Expression andExpression = null;

                foreach (var filter in filters)
                {
                    var filterExpression = BuildFilterExpression(filter, parameter);
                    if (filterExpression != null)
                    {
                        if (andExpression == null)
                        {
                            andExpression = filterExpression;
                        }
                        else
                        {
                            andExpression = Expression.AndAlso(andExpression, filterExpression);
                        }
                    }
                }
                if (andExpression == null)
                {
                    // Return default expression that always evaluates to false
                    andExpression = Expression.Constant(false);
                }

                return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
            }

            private static Expression<Func<T, bool>> GetOrFilterExpression(List<Filter> filters)
            {
                if (filters == null || !filters.Any())
                    return null;

                var parameter = Expression.Parameter(typeof(T), "x");
                Expression orExpression = null;

                foreach (var filter in filters)
                {
                    var filterExpression = BuildFilterExpression(filter, parameter);
                    if (filterExpression != null)
                    {
                        if (orExpression == null)
                        {
                            orExpression = filterExpression;
                        }
                        else
                        {
                            orExpression = Expression.OrElse(orExpression, filterExpression);
                        }
                    }
                }
                if (orExpression == null)
                {
                    // Return default expression that always evaluates to false
                    orExpression = Expression.Constant(false);
                }

                return Expression.Lambda<Func<T, bool>>(orExpression, parameter);
            }

            private static Expression BuildFilterExpression(Filter filter, ParameterExpression parameter)
            {

                if (filter.Filters != null && filter.Filters.Any())
                {
                    if (filter.Logic?.ToLower() == "and")
                    {
                        var andFilters = filter.Filters.Select(f => BuildFilterExpression(f, parameter));
                        return andFilters.Aggregate(Expression.AndAlso);
                    }
                    else if (filter.Logic?.ToLower() == "or")
                    {
                        var orFilters = filter.Filters.Select(f => BuildFilterExpression(f, parameter));
                        return orFilters.Aggregate(Expression.OrElse);
                    }
                }
                if (filter.Value == null || string.IsNullOrWhiteSpace(filter.Value.ToString()))
                    return null;
                var property = Expression.Property(parameter, filter.Field);
                var constant = Expression.Constant(filter.Value);

                switch (filter.Operator.ToLower())
                {
                    case CompositeFilter<T>.eq:
                        return Expression.Equal(property, constant);
                    case "neq":
                        return Expression.NotEqual(property, constant);
                    case "lt":
                        return Expression.LessThan(property, constant);
                    case "lte":
                        return Expression.LessThanOrEqual(property, constant);
                    case "gt":
                        return Expression.GreaterThan(property, constant);
                    case "gte":
                        return Expression.GreaterThanOrEqual(property, constant);
                    case "contains":
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        return Expression.Call(property, containsMethod, constant);
                    case "startswith":
                        var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) });

                        // Convert the constant value to lowercase for case-insensitive comparison
                        var constantLower = Expression.Call(constant, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

                        return Expression.Call(property, startsWithMethod, constantLower, Expression.Constant(StringComparison.OrdinalIgnoreCase));


                    // Add more operators as needed...
                    default:
                        throw new ArgumentException($"Unsupported operator: {filter.Operator}");
                }
            }
        }
}
