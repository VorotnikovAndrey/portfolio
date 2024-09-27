using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Steamworks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.FriendsPopup
{
    public class FriendsPopup : AbstractBasePopup
    {
        [SerializeField] private Button hideButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;

        private CancellationTokenSource refreshCancellationTokenSource;
        private CancellationTokenSource containerCancellationTokenSource;

        private readonly List<FriendContainer> containers = new();
        
        protected override UniTask OnShow(object data = null)
        {
            if (!SteamManager.Initialized)
            {
                Hide(true).Forget();
                return UniTask.CompletedTask;
            }
            
            scrollRect.verticalNormalizedPosition = 1f;
            hideButton.OnClickAsObservable().Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);

            refreshCancellationTokenSource = new CancellationTokenSource();
            StartPeriodicRefresh(refreshCancellationTokenSource.Token).Forget();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            refreshCancellationTokenSource?.Dispose();
            refreshCancellationTokenSource = null;
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            Clear();
        }
        
        private async UniTaskVoid StartPeriodicRefresh(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Refresh();

                await UniTask.Delay(5000, cancellationToken: cancellationToken);
            }
        }

        private void Refresh()
        {
            Clear();
            
            containerCancellationTokenSource = new CancellationTokenSource();
            
            Create(containerCancellationTokenSource.Token).Forget();
        }

        private async UniTask Create(CancellationToken token)
        {
            var friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            
            for (var i = 0; i < friendCount; i++)
            {
                var cSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                
                var view = await objectPoolService.GetOrCreateView<FriendContainer>(Constants.Views.FriendContainer, content);

                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(view);
                
                    return;
                }
            
                diContainer.Inject(view);
                containers.Add(view);
            
                view.Setup(ref cSteamID);
                view.gameObject.SetActive(true);
            }

            SortContainers();
        }
        
        private void SortContainers()
        {
            containers.Sort((a, b) => a.Status.CompareTo(b.Status));

            for (var i = 0; i < containers.Count; i++)
            {
                containers[i].transform.SetSiblingIndex(i);
            }
        }

        private void Clear()
        {
            containerCancellationTokenSource?.Dispose();
            containerCancellationTokenSource = null;
            
            foreach (var view in containers)
            {
                objectPoolService.ReturnToPool(view);
            }
            
            containers.Clear();
        }
    }
}