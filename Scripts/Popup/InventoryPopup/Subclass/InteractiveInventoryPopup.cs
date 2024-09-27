using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Character;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.Subclass
{
    public abstract class InteractiveInventoryPopup : InventoryPopup
    {
        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected Button hideButton;
        
        protected abstract AbstractInteractiveObject InteractiveObject { get; set; }

        protected override UniTask OnShow(object data = null)
        {
            base.OnShow(data);
            
            gameplayStage.LocalGameplayData.CharacterView.AddBusy(Constants.Keys.Busy.InInteractiveInventoryPopup);
            
            UpdateTitle();
            BeginObservablePositionHandle();

            return UniTask.CompletedTask;
        }
        
        protected override UniTask OnHide()
        {
            gameplayStage.LocalGameplayData.CharacterView.RemoveBusy(Constants.Keys.Busy.InInteractiveInventoryPopup);
            
            base.OnHide();
                
            return UniTask.CompletedTask;
        }
        
        protected override void Subscribes()
        {
            base.Subscribes();
            
            hideButton.OnClickAsObservable().Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
        }
        
        protected override IEnumerable<ItemModel> GetItemsFromSource()
        {
            return popupData.Items;
        }
        
        protected virtual void BeginObservablePositionHandle()
        {
            if (InteractiveObject == null)
            {
                Hide().Forget();
                
                return;
            }
            
            var view = gameplayStage.LocalGameplayData.CharacterView;
            var radius = (view as CharacterView).InteractiveRadius;

            Observable.EveryUpdate().Where(_ =>
            {
                var colliders = Physics.OverlapSphere(view.Center.position, radius, balance.Interactive.InteractiveLayer, QueryTriggerInteraction.Collide);
                
                return !colliders.Contains(InteractiveObject.InteractiveCollider);
                
            }).Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
        }
        
        protected virtual void UpdateTitle()
        {
            title.text = $"InteractiveInventory [id:{popupData.OwnerId}]";
        }
    }
}