using PlayVibe;
using UnityEngine.SceneManagement;

namespace Services.Gameplay.Scenes
{
    public class ScenesService
    {
        public ScenesService(EventAggregator eventAggregator)
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                eventAggregator.SendEvent(new SceneLoadedEvent
                {
                    Scene = scene
                });
            };
            
            return;

            SceneManager.sceneUnloaded += scene =>
            {
                eventAggregator.SendEvent(new SceneUnloadedEvent
                {
                    Scene = scene
                });
            };
        }
    }
}