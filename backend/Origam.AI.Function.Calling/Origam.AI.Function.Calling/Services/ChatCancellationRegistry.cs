using System.Collections.Concurrent;

namespace Origam.AI.Function.Calling.Services;

public class ChatCancellationRegistry
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> pendingRequests = new(
        StringComparer.Ordinal
    );

    public CancellationTokenSource Register(string requestId, CancellationToken requestAborted)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(requestAborted);
        pendingRequests[requestId] = source;
        return source;
    }

    public void Release(string requestId)
    {
        pendingRequests.TryRemove(requestId, out _);
    }

    public bool Cancel(string requestId)
    {
        if (!pendingRequests.TryGetValue(requestId, out var source))
        {
            return false;
        }

        try
        {
            source.Cancel();
        }
        catch (ObjectDisposedException)
        {
            return false;
        }

        return true;
    }
}
