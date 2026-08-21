using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace AuraCommerce.Tests;

internal sealed class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _values = new(StringComparer.Ordinal);

    public bool IsAvailable => true;
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public IEnumerable<string> Keys => _values.Keys;

    public void Clear() => _values.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _values.Remove(key);
    public void Set(string key, byte[] value) => _values[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _values.TryGetValue(key, out value!);
}

internal sealed class TestSessionFeature : ISessionFeature
{
    public ISession Session { get; set; } = new TestSession();
}

internal static class TestHttpContext
{
    public static DefaultHttpContext WithSession(TestSession session)
    {
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        return context;
    }
}
