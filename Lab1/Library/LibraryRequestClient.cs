using System.Collections.Concurrent;

namespace Library;

public class LibraryRequestClient : IRequestClient, ILibraryOperationHandler
{
    private readonly ILibraryOperationService _service;

    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>> _pendingOperations =
        new ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>>();

    public LibraryRequestClient(ILibraryOperationService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return await Task.FromCanceled<ResponseModel>(cancellationToken).ConfigureAwait(false);
        }

        var requestId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<ResponseModel>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingOperations.TryAdd(requestId, tcs);

        try
        {
            await using (cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                _service.BeginOperation(requestId, request, cancellationToken);
                return await tcs.Task.ConfigureAwait(false);
            }
        }
        finally
        {
            _pendingOperations.TryRemove(requestId, out _);
        }
    }

    public void HandleOperationResult(Guid requestId, byte[] data)
    {
        if (_pendingOperations.TryGetValue(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.SetResult(new ResponseModel(data));
            }
        }
    }

    public void HandleOperationError(Guid requestId, Exception exception)
    {
        if (_pendingOperations.TryGetValue(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.SetException(exception);
            }
        }
    }
}