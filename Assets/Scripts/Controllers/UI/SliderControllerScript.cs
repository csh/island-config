using System;
using IslandConfig.UI;
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

        private INumericSliderDefinition _sliderDefinition;

        public void Initialize(INumericSliderDefinition entry)
        {
            _sliderDefinition = entry ?? throw new ArgumentNullException(nameof(entry));

            label.text = entry.Name;
            slider.minValue = entry.Min;
            slider.maxValue = entry.Max;
            slider.wholeNumbers = entry.IsWholeNumberType;
            slider.SetValueWithoutNotify(entry.FloatValue);
            entry.AddChangeHandler(OnSettingChanged);
        }

        private void OnEnable()
        {
            slider?.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnDisable()
        {
            slider?.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
        
        private void OnDestroy()
        {
            _sliderDefinition?.RemoveChangeHandler(OnSettingChanged);
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            if (_sliderDefinition is null || Mathf.Approximately(slider.value, _sliderDefinition.FloatValue)) return;
            slider.value = _sliderDefinition.FloatValue;
        }

        private void OnSliderValueChanged(float value)
        {
            Debug.Log($"Slider value: {value}");
            if (_sliderDefinition is null || Mathf.Approximately(_sliderDefinition.FloatValue, value)) return;
            _sliderDefinition.FloatValue = value;
        }
    }
}
