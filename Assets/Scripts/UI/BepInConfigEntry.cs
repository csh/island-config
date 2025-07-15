using System;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace IslandConfig.UI
{
    public abstract class BepInConfigWrapper
    {
        protected readonly ConfigEntryBase ConfigEntry;

        protected object _currentBoxedValue;
        
        internal BepInConfigWrapper(ConfigEntryBase configEntry)
        {
            ConfigEntry = configEntry;
            _currentBoxedValue = configEntry.BoxedValue;
        }
        
        internal PluginInfo Owner { get; set; }
        internal string Name => ConfigEntry.Definition.Section;
        internal string Section => ConfigEntry.Definition.Section;
        internal string Description => ConfigEntry.Description?.Description ?? string.Empty;
        internal bool IsDirty => Equals(_currentBoxedValue, ConfigEntry.BoxedValue) == false;
        
        internal abstract GameObject CreatePrefab();

        internal void Commit()
        {
            if (ConfigEntry is null) return;
            ConfigEntry.BoxedValue = _currentBoxedValue;
        }

        internal void Cancel()
        {
            if (ConfigEntry is null) return;
            _currentBoxedValue = ConfigEntry.BoxedValue;
        }

        internal string GetGameObjectName()
        {
            return $"{Section}.{Name}";
        }
    }

    public abstract class BepInConfigWrapper<T> : BepInConfigWrapper
    {
        protected BepInConfigWrapper(ConfigEntry<T> configEntry) : base(configEntry)
        {
            if (configEntry is not null)
            {
                configEntry.SettingChanged += OnSettingChanged;
            }
        }

        protected new ConfigEntry<T> ConfigEntry => (ConfigEntry<T>)base.ConfigEntry;

        protected T Value => ConfigEntry.Value;

        private void OnSettingChanged(object sender, EventArgs e)
        {
            _currentBoxedValue = Value;
        }

        internal override GameObject CreatePrefab()
        {
            throw new NotImplementedException("TODO: Prefab loading and instantiation");
        }

        ~BepInConfigWrapper()
        {
            if (ConfigEntry is not null)
            {
                ConfigEntry.SettingChanged -= OnSettingChanged;
            }
        }
    }
}