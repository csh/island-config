using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace IslandConfig.UI
{
    public class EnumDropdownConfigItem<T> : BepInConfigWrapper<T> where T: Enum
    {
        public EnumDropdownConfigItem(ConfigEntry<T> configEntry) : base(configEntry)
        {
        }
    }

    public class DropdownConfigItem<T> : BepInConfigWrapper<T> where T : IEquatable<T>
    {
        protected readonly IList<T> Options;

        protected DropdownConfigItem(ConfigEntry<T> configEntry) : base(configEntry)
        {
            if (configEntry.Description.AcceptableValues is null) return;
            var constraints = ConfigEntry.Description.AcceptableValues;

            if (constraints is not AcceptableValueList<T> validOptions)
            {
                throw new ArgumentException(
                    $"{nameof(ConfigEntry<T>)} has incorrect constraint type; expected {nameof(AcceptableValueList<T>)}",
                    nameof(configEntry));
            }

            Options = new List<T>(validOptions.AcceptableValues);
        }
    }
}