using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace PlayVibe
{
    public class InfoPopup : AbstractBasePopup
    {
        [SerializeField] private TextMeshProUGUI messageText;
        
        protected override UniTask OnShow(object data = null)
        {
            var infoPopupData = data as InfoPopupData;

            if (infoPopupData == null)
            {
                Hide().Forget();
                return UniTask.CompletedTask;
            }
            
            messageText.text = infoPopupData.Message;

            LifeTimeProcess(infoPopupData.LifeTime).Forget();
            
            return UniTask.CompletedTask;
        }

        private async UniTask LifeTimeProcess(float lifeTime)
        {
            await UniTask.WaitForSeconds(lifeTime);
            
            Hide().Forget();
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
    }
}