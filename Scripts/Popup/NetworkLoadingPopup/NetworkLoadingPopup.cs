using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlayVibe
{
    public class NetworkLoadingPopup : AbstractBasePopup
    {
        private CancellationTokenSource cancellationTokenSource;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is NetworkLoadingPopupData networkLoadingPopupData)
            {
                WaitTask(networkLoadingPopupData.Source).Forget();
            }

            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }

        private async UniTask WaitTask(UniTaskCompletionSource source)
        {
            if (source != null)
            {
                cancellationTokenSource = new CancellationTokenSource();
                await source.Task.WithCancellation(cancellationTokenSource.Token);
            }
            
            Hide().Forget();
        }
    }
}