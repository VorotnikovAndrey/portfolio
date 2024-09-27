using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayVibe
{
    [CreateAssetMenu(fileName = "New ScreenFaderProfile", menuName = "ScreenFaderProfile", order = 51)]
    public class ScreenFaderProfile : ScriptableObject
    {
        [SerializeField] protected ScreenFaderType screenFaderType;
        [Space]
        [SerializeField] protected Ease openEasing;
        [SerializeField] protected float openFadeDuration = 1;
        [Space]
        [SerializeField] protected Ease closeEasing;
        [SerializeField] protected float closeFadeDuration = 1;
        [Space]
        [ShowIf("IsScale")] 
        [SerializeField] protected ScaleModel scaleModel;
        [ShowIf("IsMove")] 
        [SerializeField] protected MoveModel moveModel;

        public bool IsFade => screenFaderType == ScreenFaderType.Fade;
        public bool IsScale => screenFaderType == ScreenFaderType.Scale;
        public bool IsMove => screenFaderType == ScreenFaderType.Move;
        
        public ScaleModel Scale => scaleModel;
        public MoveModel Move => moveModel;
        public ScreenFaderType ScreenFaderType => screenFaderType;
        public Ease EasingOpen => openEasing;
        public Ease EasingClose => closeEasing;
        public float FadeDurationOpen => openFadeDuration;
        public float FadeDurationClose => closeFadeDuration;

        [Serializable]
        public class ScaleModel
        {
            public Vector3 ShowScaleA = Vector3.zero;
            public Vector3 ShowScaleB = Vector3.one;
            public Vector3 HideScaleA = Vector3.one;
            public Vector3 HideScaleB = Vector3.zero;
        }
        
        [Serializable]
        public class MoveModel
        {
            public MoveInSide OpenDirection;
            public MoveInSide CloseDirection;
        }
    }
}
