using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;

namespace IslandConfig.Controllers.UI
{
    internal class EnumDropdownControllerScript : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TextMeshProUGUI label;
        
        private IEnumDropdownDefinition _dropdownDefinition;

        public void Initialize(IEnumDropdownDefinition entry)
        {
            _dropdownDefinition = entry ?? throw new ArgumentNullException(nameof(entry));
            
            label.text = entry.Name;
            dropdown.ClearOptions();
            dropdown.AddOptions(entry.Options);
            dropdown.SetValueWithoutNotify(entry.SelectedIndex);
            
            entry.SettingChanged += OnSettingChanged;
        }

        private void OnEnable()
        {
            dropdown?.onValueChanged.AddListener(OnSelectedIndexChanged);
        }

        private void OnDisable()
        {
            dropdown?.onValueChanged.RemoveListener(OnSelectedIndexChanged);
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (dropdown.value == _dropdownDefinition.SelectedIndex) return;
            dropdown.SetValueWithoutNotify(_dropdownDefinition.SelectedIndex);
        }

        private void OnSelectedIndexChanged(int newIndex)
        {
            if (newIndex == _dropdownDefinition.SelectedIndex) return;
            _dropdownDefinition.SelectedIndex = newIndex;
        }

        private void OnDestroy()
        {
            if (_dropdownDefinition is not null)
            {
                _dropdownDefinition.SettingChanged -= OnSettingChanged;
            }
        }
    }
}