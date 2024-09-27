using Gameplay.Character;
using Gameplay.Network;
using PlayVibe;
using TMPro;
using UnityEngine;
using Zenject;

namespace Gameplay.Player.LootBox
{
    public abstract class PersonalLootBoxView : AbstractInteractiveObject
    {
        [SerializeField] protected TMP_Text title;

        protected virtual void Start()
        {
            title.text = $"Personal LootBox id:{photonView.OwnerActorNr}";
            gameplayStage.GameplayDataDic[photonView.OwnerActorNr].LootBoxView = this;
        }
        
        public override void TryInteractive(CharacterView view)
        {
            SuccessfulInteractive();
        }

        protected abstract void SuccessfulInteractive();
        protected abstract void FailedInteractive();
    }
}