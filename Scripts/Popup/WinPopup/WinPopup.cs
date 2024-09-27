using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.WinPopup
{
    public class WinPopup : AbstractBasePopup
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button exitButton;
        
        [Inject] private GameplayStage gameplayStage;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is WinPopupData winPopupData)
            {
                resultText.text = $"{winPopupData.WinRole.ToString()} WIN!";
            }
            else
            {
                resultText.text = string.Empty;
            }
            
            exitButton.OnClickAsObservable().Subscribe(_ => OnReturnToLobbyClick()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            gameplayStage.ReturnToLobby();
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        private void OnReturnToLobbyClick()
        {
            Hide().Forget();
        }
    }
}