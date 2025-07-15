using BepInEx.Configuration;

namespace IslandConfig.UI
{
    public class TextConfigItem : BepInConfigWrapper<string>
    {
        public TextConfigItem(ConfigEntry<string> configEntry) : base(configEntry)
        {
        }
    }
}