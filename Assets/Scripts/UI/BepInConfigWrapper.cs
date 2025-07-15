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

        private string _customName;
        private string _customSection;
        private string _customDescription;
        
        internal BepInConfigWrapper(ConfigEntryBase configEntry)
        {
            ConfigEntry = configEntry;
            _currentBoxedValue = configEntry.BoxedValue;
        }
        
        internal PluginInfo Owner { get; set; }
        
        internal string Name
        {
            get => _customName ?? ConfigEntry.Definition.Section;
            set => _customName = value;
        }

        internal string Section
        {
            get => _customSection ?? ConfigEntry.Definition.Section;
            set => _customSection = value;
        }

        internal string Description
        {
            get => _customDescription ?? ConfigEntry.Description?.Description ?? string.Empty;
            set => _customDescription = value;
        }

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