using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;

namespace IslandConfig.Controllers.UI
{
    public class TextControllerScript : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private TMP_InputField textInput;
        [SerializeField] private TextMeshProUGUI label;
        
        private TextConfigItem _entry;

        public void Initialize(TextConfigItem entry)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            label.text = entry.Name;
            textInput.text = entry.Value;
            _entry.ConfigEntry.SettingChanged += OnConfigEntryChanged;
        }

        private void OnConfigEntryChanged(object sender, EventArgs e)
        {
            textInput?.SetTextWithoutNotify(_entry.Value);
        }

        private void OnEnable()
        {
            textInput?.onValueChanged.AddListener(OnTextInputChanged);
        }

        private void OnDisable()
        {
            textInput?.onValueChanged.RemoveListener(OnTextInputChanged);
        }

        private void OnTextInputChanged(string value)
        {
            _entry.Value = value;
        }

        private void OnDestroy()
        {
            if (_entry is not null)
            {
                _entry.ConfigEntry.SettingChanged -= OnConfigEntryChanged;
            }
        }
    }
}
