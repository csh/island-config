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
                not null when type.IsEnum => CreateEnumConfigItem(configEntry),
                _ => throw new NotImplementedException($"{type!.Name} not supported yet")
            };
        }

        private static BepInConfigWrapper CreateEnumConfigItem(ConfigEntryBase configEntry)
        {
            var generified = typeof(EnumDropdownConfigItem<>).MakeGenericType(configEntry.SettingType);
            return (BepInConfigWrapper)Activator.CreateInstance(generified);
        }
    }
}