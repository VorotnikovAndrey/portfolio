using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Steamworks;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class NicknamePopup : AbstractBasePopup
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button confirmButton;

        [Inject] private PopupService popupService;
        
        protected override UniTask OnShow(object data = null)
        {
            InputDisabler.Clear();
            
            PhotonNetwork.NickName = SteamFriends.GetPersonaName();;
            inputField.text = PhotonNetwork.NickName;

            confirmButton.OnClickAsObservable().Subscribe(_ => Confirm()).AddTo(CompositeDisposable);
            inputField.OnSubmitAsObservable().Subscribe(_ => Confirm()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        private void Confirm()
        {
            if (!ValidateNickname(inputField.text))
            {
                return;
            }

            InputDisabler.Disable();
            
            PhotonNetwork.NickName = inputField.text;

            Hide().Forget();
        }
        
        private void ShowInfoPopup(string message)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = message
            }, PopupGroup.System)).Forget();
        }
        
        private bool ValidateNickname(string nickname)
        {
            // Проверка на наличие символов
            if (string.IsNullOrEmpty(nickname))
            {
                ShowInfoPopup($"Nickname cannot be empty");
                return false;
            }

            // Проверка на минимальную длину
            if (nickname.Length < 4)
            {
                ShowInfoPopup($"Nickname must be at least 4 characters long");
                return false;
            }

            // Проверка на начало с цифры
            if (char.IsDigit(nickname[0]))
            {
                ShowInfoPopup($"Nickname cannot start with a digit");
                return false;
            }

            // Проверка на допустимые символы
            if (!nickname.All(char.IsLetterOrDigit))
            {
                ShowInfoPopup($"Nickname can only contain letters and digits");
                return false;
            }

            // Проверка на максимальную длину
            if (nickname.Length > 20)
            {
                ShowInfoPopup($"Nickname cannot exceed 20 characters");
                return false;
            }

            // Проверка на наличие пробелов
            if (nickname.Contains(" "))
            {
                ShowInfoPopup($"Nickname cannot contain spaces");
                return false;
            }

            return true;
        }
    }
}