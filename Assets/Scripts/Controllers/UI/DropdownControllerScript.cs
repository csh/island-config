using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;

namespace IslandConfig.Controllers.UI
{
    internal class DropdownControllerScript : SettingControllerBase<IDropdownDefinition>
    {
        [SerializeField] private TMP_Dropdown dropdown;
        
        private void Start()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(Definition.Labels);
            dropdown.SetValueWithoutNotify(Definition.SelectedIndex);
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

        public override void ForceUpdateElement()
        {
            if (dropdown is null || Definition is null) return;
            dropdown.SetValueWithoutNotify(Definition.SelectedIndex);
        }
    }
}