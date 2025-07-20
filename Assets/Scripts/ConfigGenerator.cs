using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using IslandConfig.UI;

namespace IslandConfig
{
    internal static class ConfigGenerator
    {
        internal static IEnumerable<BepInConfigWrapper> Generate(PluginInfo pluginInfo, ConfigFile config)
        {
            foreach (var pair in config)
            {
                var wrapped = WrapEntry(pluginInfo.Metadata, pair.Value);
                if (wrapped == null) continue;
                wrapped.Owner = pluginInfo;
                yield return wrapped;
            }
        }

        private static BepInConfigWrapper WrapEntry(BepInPlugin pluginMeta, ConfigEntryBase configEntry)
        {
            var type = configEntry.SettingType;

            return type switch
            {
                // TODO: Implement keyboard shortcut binding.
                not null when type.IsEquivalentTo(typeof(KeyboardShortcut)) => null,
                not null when type.IsEquivalentTo(typeof(UnityEngine.Rect)) => null,
                not null when type.IsEquivalentTo(typeof(bool)) => new CheckboxConfigItem(configEntry as ConfigEntry<bool>),
                not null when type.IsEquivalentTo(typeof(string)) && !HasListConstraint(configEntry) => new TextConfigItem(configEntry as ConfigEntry<string>),
                
                #region Numeric sliders

                not null when type.IsEquivalentTo(typeof(int)) && HasRangeConstraint(configEntry) => new IntSliderConfigItem(configEntry as ConfigEntry<int>),
                not null when type.IsEquivalentTo(typeof(byte)) && HasRangeConstraint(configEntry) => new ByteSliderConfigItem(configEntry as ConfigEntry<byte>),
                not null when type.IsEquivalentTo(typeof(long)) && HasRangeConstraint(configEntry) => new LongSliderConfigItem(configEntry as ConfigEntry<long>),
                not null when type.IsEquivalentTo(typeof(short)) && HasRangeConstraint(configEntry) => new ShortSliderConfigItem(configEntry as ConfigEntry<short>),
                not null when type.IsEquivalentTo(typeof(float)) && HasRangeConstraint(configEntry) => new FloatSliderConfigItem(configEntry as ConfigEntry<float>),
                not null when type.IsEquivalentTo(typeof(double)) && HasRangeConstraint(configEntry) => new DoubleSliderConfigItem(configEntry as ConfigEntry<double>),
                not null when type.IsEquivalentTo(typeof(decimal)) && HasRangeConstraint(configEntry) => new DecimalSliderConfigItem(configEntry as ConfigEntry<decimal>),
                
                #endregion
                
                #region Numeric text inputs
                
                not null when type.IsEquivalentTo(typeof(int)) && !HasListConstraint(configEntry) => new IntTextInput(configEntry as ConfigEntry<int>),
                not null when type.IsEquivalentTo(typeof(byte)) && !HasListConstraint(configEntry) => new ByteTextInput(configEntry as ConfigEntry<byte>),
                not null when type.IsEquivalentTo(typeof(long)) && !HasListConstraint(configEntry) => new LongTextInput(configEntry as ConfigEntry<long>),
                not null when type.IsEquivalentTo(typeof(short)) && !HasListConstraint(configEntry) => new ShortTextInput(configEntry as ConfigEntry<short>),
                not null when type.IsEquivalentTo(typeof(float)) && !HasListConstraint(configEntry) => new FloatTextInput(configEntry as ConfigEntry<float>),
                not null when type.IsEquivalentTo(typeof(double)) && !HasListConstraint(configEntry) => new DoubleTextInput(configEntry as ConfigEntry<double>),
                not null when type.IsEquivalentTo(typeof(decimal)) && !HasListConstraint(configEntry) => new DecimalTextInput(configEntry as ConfigEntry<decimal>),
                
                #endregion
                
                not null when type.IsEnum == false && HasListConstraint(configEntry) => CreateGenericDropdown(configEntry),
                not null when type.IsEnum => CreateEnumConfigItem(configEntry),
                _ => CreateCustomOrThrow(pluginMeta, configEntry)
            };
        }

        private static BepInConfigWrapper CreateCustomOrThrow(BepInPlugin pluginMeta, ConfigEntryBase configEntry)
        {
            foreach (var (_, typeRegistry) in IslandConfig.CustomConfigElements)
            {
                if (typeRegistry.TryGetValue(configEntry.SettingType, out var factory))
                {
                    return factory(configEntry);
                }
            }
            throw new UnsupportedBindingException(pluginMeta, configEntry);
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
            return (BepInConfigWrapper)Activator.CreateInstance(generified, configEntry);
        }
        
        private static BepInConfigWrapper CreateEnumConfigItem(ConfigEntryBase configEntry)
        {
            var generified = typeof(EnumDropdownConfigItem<>).MakeGenericType(configEntry.SettingType);
            return (BepInConfigWrapper)Activator.CreateInstance(generified, configEntry);
        }

        private class UnsupportedBindingException : NotImplementedException
        {
            public UnsupportedBindingException(BepInPlugin pluginMeta, ConfigEntryBase configEntry) : base(BuildMessage(pluginMeta, configEntry))
            {
            }

            private static string BuildMessage(BepInPlugin pluginMeta, ConfigEntryBase entry)
            {
                var builder = new StringBuilder("Setting type is not supported yet.");
                builder.AppendLine();
                builder.AppendLine($"Plugin: {pluginMeta.Name} ({pluginMeta.GUID}");
                builder.AppendLine($"Config path: {entry.Definition.Section}/{entry.Definition.Key}");
                builder.AppendLine($"Config type: {entry.SettingType.FullName}");
                return builder.ToString();
            }
        }
    }
}