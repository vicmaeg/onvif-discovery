namespace OnvifDiscovery.Common;

internal static class ExtensionMethods
{
    /// <summary>
    ///     Used to provide cancellation possibility to any Async Methods returning a Task
    /// </summary>
    internal static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        await using (cancellationToken.Register(s => (s as TaskCompletionSource<bool>)?.TrySetResult(true), tcs))
        {
            if (task != await Task.WhenAny(task, tcs.Task))
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        return await task;
    }
}