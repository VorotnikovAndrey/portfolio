using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public abstract class AbstractStageBase : IStage
    {
        public abstract string StageType { get; }

        public Dictionary<object, IStage> SubStages { get; } = new ();

        [Inject] protected EventAggregator eventAggregator;

        public virtual UniTask Initialize(object data = null)
        {
            Debug.Log($"{StageType.AddColorTag(Color.yellow)} Initialized".AddColorTag(Color.cyan));

            foreach (IStage value in SubStages.Values)
            {
                value.Initialize(data);
            }
            
            return UniTask.CompletedTask;
        }

        public virtual UniTask DeInitialize()
        {
            Debug.Log($"{StageType.AddColorTag(Color.yellow)} DeInitialized".AddColorTag(Color.cyan));

            foreach (IStage value in SubStages.Values)
            {
                value.DeInitialize();
            }
            
            return UniTask.CompletedTask;
        }
    }
}
