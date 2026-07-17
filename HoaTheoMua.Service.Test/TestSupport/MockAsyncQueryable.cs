using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HoaTheoMua.Service.Test.TestSupport;

// Minimal in-memory IQueryable/IAsyncEnumerable shims so a plain List<T> can back a
// mocked DbSet<T> and support EF Core's async query operators (ToListAsync,
// FirstOrDefaultAsync, CountAsync, ...). Shared by the hand-rolled BuildMockDbSet
// helpers so this infrastructure no longer has to be copy-pasted into every test file.

// Strips EF-only operators (AsNoTracking/Include/ThenInclude) out of the expression
// tree, since a plain in-memory provider cannot translate them. It is a no-op for
// queries that don't use those operators, so a single provider serves every test.
internal class EfExpressionCleaner : ExpressionVisitor
{
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) &&
            (node.Method.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTracking) ||
             node.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include) ||
             node.Method.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude)))
        {
            return Visit(node.Arguments[0])!;
        }

        return base.VisitMethodCall(node);
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    private static Expression CleanExpression(Expression expression)
    {
        return new EfExpressionCleaner().Visit(expression)!;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(CleanExpression(expression));
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(CleanExpression(expression));
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(CleanExpression(expression));
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(CleanExpression(expression));
    }

    public TResult ExecuteAsync<TResult>(
        Expression expression,
        CancellationToken cancellationToken = default)
    {
        var cleanedExpression = CleanExpression(expression);
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];

        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .First(method =>
                method.Name == nameof(IQueryProvider.Execute) &&
                method.IsGenericMethod &&
                method.GetParameters().Length == 1)
            .MakeGenericMethod(expectedResultType)
            .Invoke(_inner, new object[] { cleanedExpression });

        return (TResult)typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> :
    EnumerableQuery<T>,
    IAsyncEnumerable<T>,
    IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression) : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider =>
        new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}
