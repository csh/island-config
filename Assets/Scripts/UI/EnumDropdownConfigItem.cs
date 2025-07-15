using System;
using BepInEx.Configuration;

namespace IslandConfig.UI
{
    public class EnumDropdownConfigItem<T> : BepInConfigWrapper<T> where T: Enum
    {
        public EnumDropdownConfigItem(ConfigEntry<T> configEntry) : base(configEntry)
        {
        }
    }
}