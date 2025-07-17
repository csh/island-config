using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandConfig.Controllers.UI
{
    internal class CheckboxControllerScript : SettingControllerBase<CheckboxConfigItem>
    {
        [SerializeField] private Toggle toggle;
        
        internal override void Initialize(CheckboxConfigItem definition, TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
            base.Initialize(definition, hoverNameTarget, hoverDescTarget);
            toggle.isOn = definition.Value;
        }

        private void OnEnable() => toggle?.onValueChanged.AddListener(OnToggleChanged);

        private void OnDisable() => toggle?.onValueChanged.RemoveListener(OnToggleChanged);

        private void OnToggleChanged(bool isOn)
        {
            if (Definition is null || Definition.Value == isOn) return;
            Definition.Value = isOn;
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            if (toggle is null || toggle.isOn == Definition.Value) return;
            toggle.isOn = Definition.Value;
        }

        internal override void ForceUpdateElement()
        {
            if (toggle is null || Definition is null) return;
            toggle.SetIsOnWithoutNotify(Definition.Value);
        }
    }
}
