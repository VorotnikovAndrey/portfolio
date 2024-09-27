using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlayVibe
{
    public static class UniTaskExtensions
    {
        public static async UniTask WithCancellation(this UniTask task, CancellationToken cancellationToken)
        {
            var tcs = new UniTaskCompletionSource();

            await using (cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                try
                {
                    await UniTask.WhenAny(task, tcs.Task);
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
            }
        }
    
        public static async UniTask<T> WithCancellation<T>(this UniTask<T> task, CancellationToken cancellationToken)
        {
            var tcs = new UniTaskCompletionSource<T>();
            
            await using (cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                try
                {
                    await UniTask.WhenAny(task, tcs.Task);

                    return await task;
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
            }
        }
    }
}