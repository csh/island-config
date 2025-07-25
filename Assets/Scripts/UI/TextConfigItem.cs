﻿using BepInEx.Configuration;
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
        
        internal override GameObject CreatePrefab(TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
#if UNITY_EDITOR
            var prefab = Object.Instantiate(IslandConfigAssets.EditorTextPrefab).gameObject;
#else
            var prefab = Object.Instantiate(IslandConfigAssets.TextPrefab);
#endif
            var controller = prefab.GetComponent<TextControllerScript>();
            controller.Initialize(this, hoverNameTarget, hoverDescTarget);
            return prefab.gameObject;
        }

        string ITextInputDefinition.Value
        {
            get => Value;
            set => Value = value;
        }
    }
}