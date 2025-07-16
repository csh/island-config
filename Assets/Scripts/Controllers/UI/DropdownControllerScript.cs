using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IslandConfig.Controllers.UI
{
    internal class DropdownControllerScript : MonoBehaviour, IPointerEnterHandler
    {
        [Header("UI References")] 
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI hoverText;
        
        private IDropdownDefinition _dropdownDefinition;

        public void Initialize(IDropdownDefinition entry, TextMeshProUGUI hoverTextTarget)
        {
            _dropdownDefinition = entry ?? throw new ArgumentNullException(nameof(entry));
            hoverText = hoverTextTarget;
            label.text = entry.Name;
            dropdown.ClearOptions();
            dropdown.AddOptions(entry.Labels);
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverText is null) return;
            hoverText.text = _dropdownDefinition.Description ?? "No description provided.";
        }
    }
}