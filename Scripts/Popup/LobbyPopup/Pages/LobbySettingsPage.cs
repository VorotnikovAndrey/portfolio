using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.Pages
{
    [Serializable]
    public class LobbySettingsPage : AbstractLobbyPopupPage
    {
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button backButton;
        [Space]
        [SerializeField] private TMP_Dropdown screenModeDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [Space] 
        [SerializeField] private List<SettingsTypeBar> settingsBar;
        [Space] 
        [SerializeField] private List<ControlContainer> controlContainers;

        [Inject] private ControlSettingsManager controlSettingsManager;
        [Inject] private ControlSettings controlSettings;

        private FullScreenMode[] screenModes = { FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
        private Resolution[] availableResolutions;
        private ControlSettings tempControlSettings;

        protected override UniTask OnInitialize()
        {
            applyButton.OnClickAsObservable().Subscribe(_ => OnApplyButtonClick()).AddTo(CompositeDisposable);
            resetButton.OnClickAsObservable().Subscribe(_ => OnResetButtonClick()).AddTo(CompositeDisposable);
            backButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(CompositeDisposable);

            foreach (var element in settingsBar)
            {
                element.EmitClick.Subscribe(HandleSettingsBar).AddTo(CompositeDisposable);
            }
            
            foreach (var element in controlContainers)
            {
                element.EmitValueChanged.Subscribe(HandleControl).AddTo(CompositeDisposable);
            }

            return UniTask.CompletedTask;
        }

        protected override UniTask OnDeinitialize()
        {
            return UniTask.CompletedTask;
        }
        
        protected override UniTask OnShow()
        {
            Setup();
            HandleSettingsBar(SettingsType.Screen);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }

        private void Setup()
        {
            SetupScreenMode();
            SetupResolution();
            SetupControls();
        }

        private void HandleSettingsBar(SettingsType type)
        {
            settingsBar.ForEach(x => x.Setup(type));
            resetButton.gameObject.SetActive(settingsBar.FirstOrDefault(x => x.SettingsType == type)?.ResetSupports ?? false);
        }

        private void HandleControl(ControlContainer container)
        {
            tempControlSettings.Data[container.ControlType] = container.KeyCode;

            FindDuplicateControl();
        }

        private void FindDuplicateControl()
        {
            var keyCodeCount = new Dictionary<KeyCode, int>();

            foreach (var element in controlContainers)
            {
                if (!keyCodeCount.TryAdd(element.KeyCode, 1))
                {
                    keyCodeCount[element.KeyCode]++;
                }
            }

            applyButton.interactable = true;

            foreach (var element in controlContainers)
            {
                var result = keyCodeCount[element.KeyCode] > 1;

                if (result)
                {
                    applyButton.interactable = false;
                }
                
                element.SetDuplicateState(result);
            }
        }
        
        private bool HasDuplicateKeyCodes(Dictionary<ControlType, KeyCode> data)
        {
            var seenKeyCodes = new HashSet<KeyCode>();

            foreach (var entry in data.Values)
            {
                if (!seenKeyCodes.Add(entry))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnApplyButtonClick()
        {
            var screenModeIndex = screenModeDropdown.value;
            Screen.SetResolution(availableResolutions[resolutionDropdown.value].width, availableResolutions[resolutionDropdown.value].height, screenModes[screenModeIndex]);
            
            controlSettings.Data = new Dictionary<ControlType, KeyCode>(tempControlSettings.Data);
            controlSettingsManager.SaveControlSettings();

            ShowInfoPopup($"Settings applied");
        }
        
        private void OnResetButtonClick()
        {
            var resetSettings = new ControlSettings();
            
            controlSettings.Data = new Dictionary<ControlType, KeyCode>(resetSettings.Data);
            controlSettingsManager.SaveControlSettings();
            
            SetupControls();
            
            ShowInfoPopup($"Settings reset");
        }

        private void OnBackButtonClick()
        {
            LobbyPopup.SetPage(LobbyPopupPageType.Menu);
        }

        private void SetupScreenMode()
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(new List<string> { "Fullscreen", "Borderless", "Windowed" });
            
            SetCurrentScreenMode();
        }

        private void SetupResolution()
        {
            availableResolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(availableResolutions.Select(res => $"{res.width}x{res.height} @ {res.refreshRate}Hz").ToList());
            
            SetCurrentResolution();
        }

        private void SetCurrentScreenMode()
        {
            var currentMode = Screen.fullScreenMode;
            var currentModeIndex = Array.IndexOf(screenModes, currentMode);
            
            if (currentModeIndex >= 0)
            {
                screenModeDropdown.value = currentModeIndex;
            }
        }

        private void SetCurrentResolution()
        {
            var currentWidth = Screen.width;
            var currentHeight = Screen.height;
            var currentResolutionIndex = Array.FindIndex(availableResolutions, res => res.width == currentWidth && res.height == currentHeight);

            if (currentResolutionIndex >= 0)
            {
                resolutionDropdown.value = currentResolutionIndex;
            }
            else
            {
                Debug.Log("The current resolution was not found in the list of available resolutions.".AddColorTag(Color.red));
            }
        }
        
        private void SetupControls()
        {
            tempControlSettings = new ControlSettings
            {
                Data = new Dictionary<ControlType, KeyCode>(controlSettings.Data)
            };

            foreach (var container in controlContainers)
            {
                container.Initialize(tempControlSettings.Data[container.ControlType]);
            }

            FindDuplicateControl();
        }
    }
}
