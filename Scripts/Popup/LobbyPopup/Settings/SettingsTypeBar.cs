using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe
{
    public class SettingsTypeBar : MonoBehaviour
    {
        [SerializeField] private SettingsType type;
        [SerializeField] private RectTransform layoutGroup;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private Button button;
        [SerializeField] private GameObject content;
        [SerializeField] private bool resetSupports;

        private readonly Subject<SettingsType> emitClick = new();

        private readonly CompositeDisposable compositeDisposable = new();

        public IObservable<SettingsType> EmitClick => emitClick;
        public bool ResetSupports => resetSupports;
        public SettingsType SettingsType => type;
        
        private void Start()
        {
            button.OnClickAsObservable().Subscribe(_ => emitClick.OnNext(type)).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }

        public void Setup(SettingsType type)
        {
            if (this.type == type)
            {
                content.gameObject.SetActive(true);
                button.interactable = false;
            }
            else
            {
                content.gameObject.SetActive(false);
                button.interactable = true;
            }
        }
    }
}