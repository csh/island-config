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
        [SerializeField] private TextMeshProUGUI hoverName;
        [SerializeField] private TextMeshProUGUI hoverDesc;
        
        private IDropdownDefinition _dropdownDefinition;

        public void Initialize(IDropdownDefinition entry, TextMeshProUGUI hoverNameTarget,
            TextMeshProUGUI hoverDescTarget)
        {
            _dropdownDefinition = entry ?? throw new ArgumentNullException(nameof(entry));
            hoverName = hoverNameTarget;
            hoverDesc = hoverDescTarget;
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
            if (hoverName is not null) hoverName.text = _dropdownDefinition.Name;
            if (hoverDesc is not null) hoverDesc.text = _dropdownDefinition.Description;
        }
    }
}