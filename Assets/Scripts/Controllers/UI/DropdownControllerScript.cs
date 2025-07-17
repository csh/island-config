using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;

namespace IslandConfig.Controllers.UI
{
    internal class DropdownControllerScript : SettingControllerBase<IDropdownDefinition>
    {
        [SerializeField] private TMP_Dropdown dropdown;
        
        internal override void Initialize(IDropdownDefinition entry, TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
            base.Initialize(entry, hoverNameTarget, hoverDescTarget);

            dropdown.ClearOptions();
            dropdown.AddOptions(entry.Labels);
            dropdown.SetValueWithoutNotify(entry.SelectedIndex);
        }

        private void OnEnable() => dropdown?.onValueChanged.AddListener(OnDropdownChanged);
        private void OnDisable() => dropdown?.onValueChanged.RemoveListener(OnDropdownChanged);

        private void OnDropdownChanged(int index)
        {
            if (Definition is null || Definition.SelectedIndex == index) return;
            Definition.SelectedIndex = index;
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            if (dropdown is null || dropdown.value == Definition.SelectedIndex) return;
            dropdown.SetValueWithoutNotify(Definition.SelectedIndex);
        }

        internal override void ForceUpdateElement()
        {
            if (dropdown is null || Definition is null) return;
            dropdown.SetValueWithoutNotify(Definition.SelectedIndex);
        }
    }
}