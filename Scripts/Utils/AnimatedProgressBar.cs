using System;
using DG.Tweening;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI
{
    public class AnimatedProgressBar : MonoBehaviour
    {
        public event Action<int> OnAppend;

        [Header("Components:")] [SerializeField]
        private Image colorImage = default;

        [SerializeField] private Image slowColorImage = default;
        [SerializeField] private TextMeshProUGUI progressText = default;

        [Header("Animation:")] [SerializeField]
        private float duration = 1f;

        [SerializeField] private Ease ease = Ease.Unset;
        [SerializeField] private float slowColorSpeed = 10f;
        [SerializeField] private float slowColorDelay = 0;
        private float _slowPassedDelay = 0;
        [Header("Colors:")] [SerializeField] private bool useChangeColor = false;
        [SerializeField] private Color minColor = Color.yellow;
        [SerializeField] private Color maxColor = Color.green;

        protected AnimatedProgress animatedProgress = new AnimatedProgress();

        public virtual float Progress => animatedProgress.Progress;
        protected float _currentValue;

        protected virtual void Awake()
        {
            animatedProgress.OnValueChanged += UpdateInfo;
        }

        protected virtual void Update()
        {
            UpdateSlowColorImage();
        }

        protected virtual void UpdateSlowColorImage()
        {
            if (slowColorImage == null) return;


            if (Math.Abs(slowColorImage.fillAmount - colorImage.fillAmount) > 0.001f && _slowPassedDelay <= 0)
            {
                slowColorImage.fillAmount = Mathf.Lerp(slowColorImage.fillAmount, colorImage.fillAmount,
                    slowColorSpeed * Time.deltaTime);
            }
            else
            {
                _slowPassedDelay -= Time.deltaTime;
            }
        }

        protected virtual void UpdateInfo(float value)
        {
            value = Mathf.Clamp01(value);

            colorImage.fillAmount = value;

            if (progressText != null)
            {
                progressText.text = $"{(int) (value * 100f)}%";
            }

            if (useChangeColor)
            {
                colorImage.color = Color.Lerp(minColor, maxColor, value);
            }
        }

        protected virtual void ResetProgress()
        {
            if (slowColorImage != null)
            {
                slowColorImage.fillAmount = 0f;
            }
            
            animatedProgress.ResetProgress();
        }

        public virtual Tweener SetValue(float value, bool immediate = false, Action action = null)
        {
            animatedProgress.DropTweener();

            if (_slowPassedDelay <= 0 && _currentValue - value > 0.01f)
            {
                _slowPassedDelay = slowColorDelay;
            }

            _currentValue = value;
            return animatedProgress.ApplyProgress(Mathf.Clamp01(value), immediate ? 0f : duration, ease, action);
        }

        public virtual void SetValue(float value, int append, bool immediate = false, Action action = null)
        {
            if (append > 0)
            {
                SetValue(1f, immediate, action).onKill += () =>
                {
                    ResetProgress();
                    
                    OnAppend?.Invoke(append - 1);

                    SetValue(0, true);
                    SetValue(value, append - 1, immediate, action);
                };
            }
            else
            {
                SetValue(value, immediate, action);
            }
        }

        protected virtual void OnDestroy()
        {
            animatedProgress.OnValueChanged -= UpdateInfo;
        }
    }
}