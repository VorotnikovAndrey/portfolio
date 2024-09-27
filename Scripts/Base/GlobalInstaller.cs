using Cysharp.Threading.Tasks;
using Gameplay;
using Services;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public class GlobalInstaller : MonoInstaller
    {
        [SerializeField] private Balance balance;
        [SerializeField] private ItemsSettings itemsSettings;
        [SerializeField] private ChatColors chatColors;
        
        public override void InstallBindings()
        {
            BindScriptableObject();
            BindBase();
            BindFactories();
            BindServices();
            BindStages();
        }

        private void BindScriptableObject()
        {
            Container.Bind<Balance>().FromInstance(balance).AsSingle();
            Container.Bind<ChatColors>().FromInstance(chatColors).AsSingle();
            Container.Bind<ItemsSettings>().FromInstance(itemsSettings).AsSingle();
        }
        
        private void BindBase()
        {
            Container.Bind<ControlSettings>().AsSingle().NonLazy();
            Container.Bind<ControlSettingsManager>().AsSingle().NonLazy();
            Container.Bind<EventAggregator>().AsSingle().NonLazy();
        }

        private void BindFactories()
        {
            Container.BindFactory<ScreenFaderBase, ScreenFaderFactory>().AsSingle();
        }
        
        private void BindServices()
        {
            Container.Bind<PhotonPeerService>().AsSingle().NonLazy();
            Container.Bind<StartupService>().AsSingle().NonLazy();
            Container.Bind<ObjectPoolService>().AsSingle().NonLazy();
            Container.Bind<PopupService>().AsSingle().NonLazy();
            Container.Bind<EscService>().AsSingle().NonLazy();
        }
        
        private void BindStages()
        {
           Container.BindStageService()
                .BindStage<MainStage>()
                .BindStage<GameplayStage>()
                .SetStageAsync<MainStage>().Forget();
        }
    }
}