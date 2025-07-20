using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandConfig.Controllers.UI
{
    internal class SliderControllerScript : SettingControllerBase<INumericSliderDefinition>
    {
        [SerializeField] private Slider slider;

        private void Start()
        {
            slider.minValue = Definition.Min;
            slider.maxValue = Definition.Max;
            slider.wholeNumbers = Definition.IsWholeNumberType;
            slider.SetValueWithoutNotify(Definition.FloatValue);
        }
        
        private void OnEnable() => slider?.onValueChanged.AddListener(OnSliderChanged);
        private void OnDisable() => slider?.onValueChanged.RemoveListener(OnSliderChanged);

        private void OnSliderChanged(float value)
        {
            if (Definition is null || Mathf.Approximately(value, Definition.FloatValue)) return;
            Definition.FloatValue = value;
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            if (slider is null || Mathf.Approximately(slider.value, Definition.FloatValue)) return;
            slider.SetValueWithoutNotify(Definition.FloatValue);
        }

        public override void ForceUpdateElement()
        {
            if (slider is null || Definition is null) return;
            slider.SetValueWithoutNotify(Definition.FloatValue);
        }
    }
}
