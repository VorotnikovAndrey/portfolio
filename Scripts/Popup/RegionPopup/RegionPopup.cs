using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace PlayVibe.RegionPopup
{
    public class RegionPopup : AbstractBasePopup
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Button confirmButton;
        
        protected override UniTask OnShow(object data = null)
        {
            if (!PlayerPrefs.HasKey(Constants.PlayerPrefs.User.PreRegion))
            {
                PlayerPrefs.SetString(Constants.PlayerPrefs.User.PreRegion, PhotonRegionExtensions.DefaultRegion);
            }
            
            PopulateRegionDropdown();
            
            confirmButton.OnClickAsObservable().Subscribe(_ =>
            {
                var selectedRegion = dropdown.options[dropdown.value].text;
                PhotonRegionExtensions.ConnectToRegion(selectedRegion);
                Hide().Forget();
            }).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        private void PopulateRegionDropdown()
        {
            dropdown.ClearOptions();
            var options = new List<string>(PhotonRegionExtensions.RegionCodes.Keys);
            dropdown.AddOptions(options);

            if (PlayerPrefs.HasKey(Constants.PlayerPrefs.User.PreRegion))
            {
                SetDropdownValue(PlayerPrefs.GetString(Constants.PlayerPrefs.User.PreRegion));
            }
        }
        
        private void SetDropdownValue(string regionName)
        {
            var index = dropdown.options.FindIndex(option => option.text == regionName);

            if (index >= 0)
            {
                dropdown.value = index;
                dropdown.RefreshShownValue();
            }
            else
            {
                Debug.LogWarning("Region name not found in dropdown options.");
            }
        }
    }
}