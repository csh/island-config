using BepInEx.Configuration;
using UnityEngine;

namespace IslandConfig.UI
{
    public class TextConfigItem : BepInConfigWrapper<string>
    {
        public TextConfigItem(ConfigEntry<string> configEntry) : base(configEntry)
        {
        }
        
#if UNITY_EDITOR
        internal override GameObject CreatePrefab()
        {
            var obj = Object.Instantiate(IslandConfigAssets.EditorTextPrefab);
            obj.Initialize(this);
            return obj.gameObject;
        }
#endif
    }
}