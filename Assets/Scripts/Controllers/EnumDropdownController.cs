using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace IslandConfig.Controllers
{
    internal class EnumDropdownController : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TextMeshProUGUI label;
        
#if UNITY_EDITOR
        [Header("Debugging")]
        [InspectorName("Label Text")]
        [SerializeField] private string labelText = "Settings Label";
        
        private void OnValidate()
        {
            if (label is not null)
            {
                label.text = labelText;
            }

            if (dropdown is not null)
            {
                PopulatePreview();
            }
        }
        
        private enum PreviewEnum { Alpha, Bravo, Charlie }
        
        [SerializeField] private PreviewEnum previewValue = PreviewEnum.Alpha;

        private void PopulatePreview()
        {
            // force-populate the dropdown from our PreviewEnum
            _enumType   = typeof(PreviewEnum);
            _enumNames  = Enum.GetNames(_enumType).ToList();
            dropdown.ClearOptions();
            dropdown.AddOptions(_enumNames);

            var idx = _enumNames.IndexOf(previewValue.ToString());
            dropdown.SetValueWithoutNotify(idx);
            
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(i =>
            {
                previewValue = (PreviewEnum) Enum.Parse(_enumType, _enumNames[i]);
                UnityEditor.EditorUtility.SetDirty(this);
            });
        }
#endif

        private ConfigEntryBase _entry;
        private Type _enumType;
        private List<string> _enumNames;
        private Action _unregister;
        
        public void Initialize<T>(ConfigEntry<T> entry) where T: Enum
        {
            _entry = entry;
            _enumType = entry.SettingType;
            label.text = entry.Definition.Key;

            _enumNames = Enum.GetNames(_enumType).ToList();
            var selectedIndex = _enumNames.FindIndex(member => member == Enum.GetName(_enumType, _entry.BoxedValue));
            dropdown.ClearOptions();
            dropdown.AddOptions(_enumNames);
            dropdown.SetValueWithoutNotify(selectedIndex);
            
            dropdown.onValueChanged.AddListener(OnSelectedIndexChanged);
            entry.SettingChanged += OnSettingChanged;
            _unregister = () => entry.SettingChanged -= OnSettingChanged;
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            var selectedIndex = _enumNames.FindIndex(member => member == Enum.GetName(_enumType, _entry.BoxedValue));
            dropdown.SetValueWithoutNotify(selectedIndex);
        }

        private void OnSelectedIndexChanged(int newIndex)
        {
            if (_entry is null) return;
            _entry.BoxedValue = Enum.Parse(_enumType, _enumNames[newIndex]);
        }

        private void OnDestroy()
        {
            dropdown?.onValueChanged.RemoveListener(OnSelectedIndexChanged);

            if (_entry is not null && _unregister is not null)
            {
                _unregister();
            }
        }
    }
}