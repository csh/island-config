using System;
using BepInEx.Configuration;
using IslandConfig.Controllers.UI;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IslandConfig.UI
{
    public class TextConfigItem : BepInConfigWrapper<string>, ITextInputDefinition
    {
        public TextConfigItem(ConfigEntry<string> configEntry) : base(configEntry)
        {
        }
        
        internal override GameObject CreatePrefab(TextMeshProUGUI hoverText)
        {
#if UNITY_EDITOR
            var prefab = Object.Instantiate(IslandConfigAssets.EditorTextPrefab).gameObject;
#else
            var prefab = Object.Instantiate(IslandConfigAssets.TextPrefab);
#endif
            var controller = prefab.GetComponent<TextControllerScript>();
            controller.Initialize(this, hoverText);
            return prefab.gameObject;
        }

        string IGenericConfigurable.Name => Name;
        string IGenericConfigurable.Section => Section;
        string IGenericConfigurable.Description => Description;
        event EventHandler IGenericConfigurable.SettingChanged
        {
            add => ConfigEntry.SettingChanged += value;
            remove => ConfigEntry.SettingChanged -= value;
        }

        string ITextInputDefinition.Value
        {
            get => Value;
            set => Value = value;
        }
    }
}