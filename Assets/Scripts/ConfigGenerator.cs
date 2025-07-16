using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using IslandConfig.UI;

namespace IslandConfig
{
    internal static class ConfigGenerator
    {
        internal static IEnumerable<BepInConfigWrapper> Generate(PluginInfo pluginInfo, ConfigFile config)
        {
            foreach (var definition in config.Keys)
            {
                var wrapped = WrapEntry(config[definition]);
                wrapped.Owner = pluginInfo;
                yield return wrapped;
            }
        }

        private static BepInConfigWrapper WrapEntry(ConfigEntryBase configEntry)
        {
            var type = configEntry.SettingType;

            return type switch
            {
                not null when type.IsEquivalentTo(typeof(bool)) => new CheckboxConfigItem(configEntry as ConfigEntry<bool>),
                not null when type.IsEquivalentTo(typeof(string)) => new TextConfigItem(configEntry as ConfigEntry<string>),
                
                #region Numeric sliders

                not null when type.IsEquivalentTo(typeof(int)) && HasRangeConstraint(configEntry) => new IntSliderConfigItem(configEntry as ConfigEntry<int>),
                not null when type.IsEquivalentTo(typeof(byte)) && HasRangeConstraint(configEntry) => new ByteSliderConfigItem(configEntry as ConfigEntry<byte>),
                not null when type.IsEquivalentTo(typeof(short)) && HasRangeConstraint(configEntry) => new ShortSliderConfigItem(configEntry as ConfigEntry<short>),
                not null when type.IsEquivalentTo(typeof(float)) && HasRangeConstraint(configEntry) => new FloatSliderConfigItem(configEntry as ConfigEntry<float>),
                not null when type.IsEquivalentTo(typeof(double)) && HasRangeConstraint(configEntry) => new DoubleSliderConfigItem(configEntry as ConfigEntry<double>),
                not null when type.IsEquivalentTo(typeof(decimal)) && HasRangeConstraint(configEntry) => new DecimalSliderConfigItem(configEntry as ConfigEntry<decimal>),
                
                #endregion
                
                not null when type.IsEnum == false && HasListConstraint(configEntry) => CreateGenericDropdown(configEntry),
                not null when type.IsEnum => CreateEnumConfigItem(configEntry),
                _ => throw new NotImplementedException($"{type!.Name} not supported yet")
            };
        }

        private static bool HasListConstraint(ConfigEntryBase configEntry)
        {
            return configEntry.Description?.AcceptableValues != null &&
                   configEntry.Description.AcceptableValues.GetType().IsAssignableFrom(typeof(AcceptableValueList<>));
        }

        private static bool HasRangeConstraint(ConfigEntryBase configEntry)
        {
            return configEntry.Description?.AcceptableValues != null &&
                   configEntry.Description.AcceptableValues.GetType().IsAssignableFrom(typeof(AcceptableValueRange<>));
        }

        private static BepInConfigWrapper CreateGenericDropdown(ConfigEntryBase configEntry)
        {
            var generified = typeof(DropdownConfigItem<>).MakeGenericType(configEntry.SettingType);
            return (BepInConfigWrapper)Activator.CreateInstance(generified);
        }
        
        private static BepInConfigWrapper CreateEnumConfigItem(ConfigEntryBase configEntry)
        {
            var generified = typeof(EnumDropdownConfigItem<>).MakeGenericType(configEntry.SettingType);
            return (BepInConfigWrapper)Activator.CreateInstance(generified);
        }
    }
}