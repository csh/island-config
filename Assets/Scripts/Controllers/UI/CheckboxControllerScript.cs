using System;
using BepInEx.Configuration;
using IslandConfig.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IslandConfig.Controllers.UI
{
    public class CheckboxControllerScript : MonoBehaviour, IPointerEnterHandler
    {
        [Header("UI References")] 
        [SerializeField] private Toggle toggle;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI hoverText;
        
#if UNITY_EDITOR
        [Header("Debugging")]
        [InspectorName("Label Text")]
        [SerializeField] private string labelText = "Settings Label";
        
        [InspectorName("Is Checked")]
        [SerializeField] private bool isChecked = true;

        private void OnValidate()
        {
            if (label is not null)
            {
                label.text = labelText;
            }

            if (toggle is not null)
            {
                toggle.isOn = isChecked;
            }
        }
#endif

        private CheckboxConfigItem _entry;

        public void Initialize(CheckboxConfigItem entry, TextMeshProUGUI hoverTextTarget)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            hoverText = hoverTextTarget;
            label.text = entry.Name;
            toggle.isOn = entry.Value;
            entry.ConfigEntry.SettingChanged += OnSettingChanged;
        }

        private void OnEnable()
        {
            toggle?.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            toggle?.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (toggle.isOn == _entry.Value || _entry is null) return;
            toggle.isOn = _entry.Value;
        }

        private void OnToggleValueChanged(bool value)
        {
            if (_entry.Value == value) return;
            _entry.Value = value;
        }

        private void OnDestroy()
        {
            if (_entry is not null)
            {
                _entry.ConfigEntry.SettingChanged -= OnSettingChanged;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverText is null) return;
            hoverText.text = _entry.Description ?? "No description provided.";
        }
    }
}
