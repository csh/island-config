﻿using BepInEx.Configuration;
using UnityEngine;
using IslandConfig.Controllers.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace IslandConfig.UI
{
    public class CheckboxConfigItem : BepInConfigWrapper<bool>
    {
        public CheckboxConfigItem(ConfigEntry<bool> configEntry) : base(configEntry)
        {
        }

        internal override GameObject CreatePrefab(TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
#if UNITY_EDITOR
            var prefab = Object.Instantiate(IslandConfigAssets.EditorCheckboxPrefab).gameObject;
#else
            var prefab = Object.Instantiate(IslandConfigAssets.CheckboxPrefab);
#endif
            var controller = prefab.GetComponent<CheckboxControllerScript>();
            controller.Initialize(this, hoverNameTarget, hoverDescTarget);
            return prefab;
        }
    }
}