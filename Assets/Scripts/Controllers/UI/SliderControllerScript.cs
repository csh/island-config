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

        internal override void Initialize(INumericSliderDefinition definition, TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
            base.Initialize(definition, hoverNameTarget, hoverDescTarget);
            
            slider.minValue = definition.Min;
            slider.maxValue = definition.Max;
            slider.wholeNumbers = definition.IsWholeNumberType;
            slider.SetValueWithoutNotify(definition.FloatValue);
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

        internal override void ForceUpdateElement()
        {
            if (slider is null || Definition is null) return;
            slider.SetValueWithoutNotify(Definition.FloatValue);
        }
    }
}
