using ExitGames.Client.Photon;
using Gameplay.Items;
using Gameplay.Network;
using Gameplay.Player.Effects;
using Gameplay.Player.Minigames;
using Gameplay.Player.SpawnPoint;
using Gameplay.Player.Spells;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.MapPopup;
using Services;
using Services.Gameplay;
using Services.Gameplay.Craft;
using Services.Gameplay.Delay;
using Services.Gameplay.Scenes;
using Services.Gameplay.TimeDay;
using Services.Gameplay.Warp;
using UnityEngine;
using Zenject;

namespace Gameplay.Installers
{
    public class GameplayInstaller : MonoInstaller, HColor
    {
        [HideInInspector] [SerializeField] private HColorData hColorData;
        
        [SerializeField] private ViewsHandler viewsHandler;
        [SerializeField] private SpawnPointHandler spawnPointHandler;
        [SerializeField] private MinigamesHandler minigamesHandler;
        [Space]
        [SerializeField] private CraftBank craftBank;
        [SerializeField] private PrintRecipeBank printRecipeBank;
        [SerializeField] private MinigamesSettings minigamesSettings;
        [SerializeField] private MapCameraController mapCameraController;
        [SerializeField] private MapCanvas mapCanvas;
        [SerializeField] private FloorsHandler floorsHandler;
        [SerializeField] private EffectsSettings effectsSettings;
        [SerializeField] private SpellsSettings spellsSettings;

        public static DiContainer DiContainer { get; private set; }
        
        public HColorData HColorData => hColorData;

        private void OnValidate()
        {
            hColorData.TextColor = Color.yellow;
        }

        public override void Start()
        {
            base.Start();
            
            var eventCode = PhotonPeerEvents.GameplayControllerInitialized;
            var raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, PhotonNetwork.LocalPlayer.ActorNumber, raiseEventOptions, SendOptions.SendReliable);
        }

        public override void InstallBindings()
        {
            DiContainer = Container;

            BindInstances();
            BindBase();
            BindFactories();
        }

        private void BindBase()
        {
            Container.Bind<ScenesService>().AsSingle().NonLazy();
            Container.Bind<GameplayController>().AsSingle().NonLazy();
            Container.Bind<ItemTransitionService>().AsSingle().NonLazy();
            Container.Bind<WarpService>().AsSingle().NonLazy();
            Container.Bind<DelayService>().AsSingle().NonLazy();
            Container.Bind<TimeDayService>().AsSingle().NonLazy();
            Container.Bind<StatisticService>().AsSingle().NonLazy();
            Container.Bind<UseItemBehaviorHandler>().AsSingle().NonLazy();
        }

        private void BindInstances()
        {
            Container.Bind<SpawnPointHandler>().FromInstance(spawnPointHandler).AsSingle().NonLazy();
            Container.Bind<ViewsHandler>().FromInstance(viewsHandler).AsSingle().NonLazy();
            Container.Bind<MinigamesHandler>().FromInstance(minigamesHandler).AsSingle().NonLazy();
            Container.Bind<CraftBank>().FromInstance(craftBank).AsSingle().NonLazy();
            Container.Bind<PrintRecipeBank>().FromInstance(printRecipeBank).AsSingle().NonLazy();
            Container.Bind<MinigamesSettings>().FromInstance(minigamesSettings).AsSingle().NonLazy();
            Container.Bind<MapCameraController>().FromInstance(mapCameraController).AsSingle().NonLazy();
            Container.Bind<MapCanvas>().FromInstance(mapCanvas).AsSingle().NonLazy();
            Container.Bind<FloorsHandler>().FromInstance(floorsHandler).AsSingle().NonLazy();
            Container.Bind<EffectsSettings>().FromInstance(effectsSettings).AsSingle().NonLazy();
            Container.Bind<SpellsSettings>().FromInstance(spellsSettings).AsSingle().NonLazy();
        }
        
        private void BindFactories()
        {
            Container.BindFactory<ItemModel, ItemFactory>().AsSingle();
            Container.BindFactory<EffectModel, EffectsFactory>().AsSingle();
        }
    }
}