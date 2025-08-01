﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using IslandConfig.UI;

namespace IslandConfig
{
    public static class IslandConfig
    {
        internal static readonly ConditionalWeakTable<PluginInfo, List<BepInConfigWrapper>> ConfigsByPlugin = new();
        internal static readonly ConditionalWeakTable<PluginInfo, Dictionary<Type, Func<ConfigEntryBase, BepInConfigWrapper>>> CustomConfigElements = new();

        private static bool _shouldGenerateConfigs = true;
        
        private static bool ShouldGenerateConfigs
        {
            get => Chainloader.PluginInfos?["com.bepis.bepinex.scriptengine"] is not null || _shouldGenerateConfigs;
            set => _shouldGenerateConfigs = value;
        }

        internal static void GenerateConfigs()
        {
            if (!ShouldGenerateConfigs) return;

            foreach (var pluginInfo in Chainloader.PluginInfos.Values)
            {
                if (pluginInfo.Instance.Config.Count < 1)
                {
                    IslandConfigPlugin.Logger.LogDebug($"Plugin {pluginInfo.Metadata.Name} has no configs to generate entries for");
                    continue;
                }

                if (ConfigsByPlugin.TryGetValue(pluginInfo, out _))
                {
                    IslandConfigPlugin.Logger.LogDebug($"Plugin {pluginInfo.Metadata.Name} has already registered configurations");
                    continue;
                }

                IslandConfigPlugin.Logger.LogDebug($"Generating configs for {pluginInfo.Metadata.Name}");
                var wrapped = ConfigGenerator.Generate(pluginInfo, pluginInfo.Instance.Config);
                ConfigsByPlugin.Add(pluginInfo, wrapped.ToList());
            }

            ShouldGenerateConfigs = false;
        }

        public static void Register(Action<ConfigBuilder> build)
        {
            var caller = Assembly.GetCallingAssembly();

            var pluginInfo =
                Chainloader.PluginInfos.Values.FirstOrDefault(pluginInfos =>
                    pluginInfos.Instance?.GetType().Assembly == caller);

            if (pluginInfo == null)
            {
                throw new InvalidOperationException(
                    $"Could not find a BaseUnityPlugin with [BepInPlugin] in {caller.FullName}");
            }

            var pluginMeta = pluginInfo.Metadata;
            IslandConfigPlugin.Logger.LogInfo(
                $"Registering config UI for plugin {pluginMeta.Name} ({pluginMeta.GUID}@{pluginMeta.Version})");

            var builder = new ConfigBuilder();
            build(builder);

            var list = ConfigsByPlugin.GetOrCreateValue(pluginInfo);
            list.Clear();
            list.AddRange(builder.Configs.Values);
        }

        public static void RegisterCustomConfigProvider<T, TWrapper>(Func<ConfigEntry<T>, TWrapper> factory) where TWrapper: BepInConfigWrapper<T>
        {
            var caller = Assembly.GetCallingAssembly();
            var pluginInfo =
                Chainloader.PluginInfos.Values.FirstOrDefault(pluginInfos =>
                    pluginInfos.Instance.GetType().Assembly == caller);
            if (pluginInfo is null) return;

            var providerRegistry = CustomConfigElements.GetOrCreateValue(pluginInfo);
            providerRegistry[typeof(T)] = configEntry =>
            {
                var typedEntry = (ConfigEntry<T>)configEntry;
                return factory(typedEntry);
            };
        }

        public static void Unregister()
        {
            var caller = Assembly.GetCallingAssembly();
            var pluginInfo =
                Chainloader.PluginInfos.Values.FirstOrDefault(pluginInfos =>
                    pluginInfos.Instance.GetType().Assembly == caller);
            if (pluginInfo is null) return;

            if (CustomConfigElements.Remove(pluginInfo))
            {
                IslandConfigPlugin.Logger.LogDebug($"Unregistered custom configuration UI elements provided by {pluginInfo.Metadata.Name}");
            }
            
            // Remove config wrappers owned by this plugin
            if (ConfigsByPlugin.TryGetValue(pluginInfo, out var configs))
            {
                foreach (var wrapper in configs)
                {
                    try
                    {
                        wrapper.Dispose();
                    }
                    catch (Exception ex)
                    {
                        IslandConfigPlugin.Logger?.LogError("Failed to unregister event handlers for config wrapper");
                        IslandConfigPlugin.Logger?.LogError(ex);
                    }
                }
                ConfigsByPlugin.Remove(pluginInfo);
            }

            // Unregister config wrappers provided by the unloading assembly
            var unloadedWrappers = 0;
            foreach (var pair in ConfigsByPlugin)
            {
                var wrappers = pair.Value;
                for (var i = wrappers.Count - 1; i >= 0; i--)
                {
                    var wrapper = wrappers[i];
                    if (wrapper.GetType().Assembly != caller) continue;
                    
                    try
                    {
                        wrapper.Dispose();
                    }
                    catch (Exception ex)
                    {
                        IslandConfigPlugin.Logger?.LogError($"Failed to unregister event handler for \"{wrapper.GetType().FullName}\" provided by {pluginInfo.Metadata.Name}");
                        IslandConfigPlugin.Logger?.LogError(ex);
                    }
                    finally
                    {
                        wrappers.RemoveAt(i);
                        unloadedWrappers += 1;
                    }
                }
            }
            IslandConfigPlugin.Logger?.LogDebug($"Unloaded {unloadedWrappers} config wrappers provided by \"{pluginInfo.Metadata.Name}\"");
        }
    }

    public class ConfigBuilder
    {
        internal readonly IDictionary<ConfigEntryBase, BepInConfigWrapper> Configs =
            new Dictionary<ConfigEntryBase, BepInConfigWrapper>();

        public ConfigBuilder WithCheckbox(ConfigEntry<bool> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new CheckboxConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithDropdown<T>(ConfigEntry<T> configEntry, string section = null, string label = null,
            string description = null) where T : IEquatable<T>
        {
            var wrapper = new DropdownConfigItem<T>(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }
        
        public ConfigBuilder WithEnumDropdown<T>(ConfigEntry<T> configEntry, string section = null, string label = null,
            string description = null) where T : Enum
        {
            var wrapper = new EnumDropdownConfigItem<T>(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithTextInput(ConfigEntry<string> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new TextConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        #region Numeric text inputs

        public ConfigBuilder WithNumericInput(ConfigEntry<byte> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new ByteTextInput(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithNumericInput(ConfigEntry<short> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new ShortTextInput(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithNumericInput(ConfigEntry<int> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new IntTextInput(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithNumericInput(ConfigEntry<long> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new LongTextInput(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithNumericInput(ConfigEntry<float> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new FloatTextInput(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithNumericInput(ConfigEntry<double> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new DoubleTextInput(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithNumericInput(ConfigEntry<decimal> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new DecimalTextInput(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        #endregion

        
        #region Numeric sliders

        public ConfigBuilder WithSlider(ConfigEntry<byte> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new ByteSliderConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithSlider(ConfigEntry<short> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new ShortSliderConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithSlider(ConfigEntry<int> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new IntSliderConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithSlider(ConfigEntry<long> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new LongSliderConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithSlider(ConfigEntry<float> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new FloatSliderConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithSlider(ConfigEntry<double> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new DoubleSliderConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        public ConfigBuilder WithSlider(ConfigEntry<decimal> configEntry, string section = null, string label = null,
            string description = null)
        {
            var wrapper = new DecimalSliderConfigItem(configEntry);
            SetCustomAttributes(wrapper, section, label, description);
            Configs[configEntry] = wrapper;
            return this;
        }

        #endregion

        private static void SetCustomAttributes(BepInConfigWrapper wrapper, string section, string label,
            string description)
        {
            if (string.IsNullOrEmpty(label) == false)
            {
                wrapper.Name = label;
            }

            if (string.IsNullOrEmpty(section) == false)
            {
                wrapper.Section = section;
            }

            if (string.IsNullOrEmpty(description) == false)
            {
                wrapper.Description = description;
            }
        }
    }
}