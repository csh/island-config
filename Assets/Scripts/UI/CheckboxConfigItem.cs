using BepInEx.Configuration;
using UnityEngine;

namespace IslandConfig.UI
{
    public class CheckboxConfigItem : BepInConfigWrapper<bool>
    {
        public CheckboxConfigItem(ConfigEntry<bool> configEntry) : base(configEntry)
        {
        }
        
#if UNITY_EDITOR
        internal override GameObject CreatePrefab()
        {
            var obj = Object.Instantiate(IslandConfigAssets.EditorCheckboxPrefab);
            obj.Initialize(this);    
            return obj.gameObject;
        }
#endif
    }
}