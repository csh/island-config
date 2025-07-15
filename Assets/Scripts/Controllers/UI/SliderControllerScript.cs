using System;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandConfig.Controllers.UI
{
    internal class SliderControllerScript : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private Slider slider;
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
        }
#endif

        private ConfigEntry<float> _entry;

        public void Initialize(ConfigEntry<float> entry)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));

            label.text = entry.Definition.Key;
            slider.value = entry.Value;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            entry.SettingChanged += OnSettingChanged;

            var acceptableRange = (AcceptableValueRange<float>)entry.Description.AcceptableValues;
            if (acceptableRange is null)
            {
                slider.minValue = 0.0f;
                slider.maxValue = 1.0f;
            }
            else
            {
                slider.minValue = acceptableRange.MinValue;
                slider.maxValue = acceptableRange.MaxValue;
            }
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (_entry is null || Mathf.Approximately(slider.value, _entry.Value)) return;
            slider.value = _entry.Value;
        }

        private void OnSliderValueChanged(float value)
        {
            Debug.Log($"Slider value: {value}");
            if (_entry is null || Mathf.Approximately(_entry.Value, value)) return;
            _entry.Value = value;
        }
        
        private void OnDestroy()
        {
            if (slider is not null)
            {
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
            
            if (_entry is not null)
            {
                _entry.SettingChanged -= OnSettingChanged;
            }
        }
    }
}
