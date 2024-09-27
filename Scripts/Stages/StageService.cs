using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public sealed class StageService
    {
        public IStage CurrentStage { get; private set; }
        public DiContainer Container { get; }

        public StageService(DiContainer container)
        {
            Container = container;
        }

        public async UniTask SetStageAsync<TStage>(object data = null) where TStage : AbstractStageBase
        {
            var resolve = Container.Resolve<TStage>();
            
            if (resolve == null)
            {
                Debug.LogError($"{nameof(TStage)} is not found!".AddColorTag(Color.red));
                return;
            }

            if (CurrentStage != null)
            {
                await CurrentStage.DeInitialize();
            }
            
            CurrentStage = resolve;
            
            await CurrentStage.Initialize(data);
        }
    }
}