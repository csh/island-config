using BepInEx.Configuration;

namespace IslandConfig.UI
{
    public class CheckboxConfigItem : BepInConfigWrapper<bool>
    {
        public CheckboxConfigItem(ConfigEntry<bool> configEntry) : base(configEntry)
        {
        }
    }
}