using BepInEx.Configuration;
using IslandConfig.Controllers.UI;
using UnityEngine;

namespace IslandConfig.UI
{
    public class TextConfigItem : BepInConfigWrapper<string>
    {
        public TextConfigItem(ConfigEntry<string> configEntry) : base(configEntry)
        {
        }
        
        internal override GameObject CreatePrefab()
        {
#if UNITY_EDITOR
            var prefab = Object.Instantiate(IslandConfigAssets.EditorTextPrefab).gameObject;
#else
            var prefab = Object.Instantiate(IslandConfigAssets.TextPrefab);
#endif
            var controller = prefab.GetComponent<TextControllerScript>();
            controller.Initialize(this);
            return prefab.gameObject;
        }
    }
}