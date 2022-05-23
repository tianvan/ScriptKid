namespace ScriptKid;

internal static class TaskExtensions
{
    private struct Void { }

    public static async Task<TResult?> WithCancellation<TResult>(this Task<TResult?> originalTask, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<Void>();

        using (cancellationToken.Register(tcs => (tcs as TaskCompletionSource<Void>)!.TrySetResult(default), tcs))
        {
            Task any = await Task.WhenAny(originalTask, tcs.Task).ConfigureAwait(false);
            if (any == tcs.Task)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            return await originalTask.ConfigureAwait(false);
        }
    }
}
