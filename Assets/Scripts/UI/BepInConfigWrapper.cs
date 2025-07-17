using System;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace IslandConfig.UI
{
    public abstract class BepInConfigWrapper : IDisposable
    {
        protected object CurrentBoxedValue;
        private readonly WeakReference<ConfigEntryBase> _configEntry;
        
        private string _customName;
        private string _customSection;
        private string _customDescription;
        private WeakReference<PluginInfo> _owner;

        internal BepInConfigWrapper(ConfigEntryBase configEntry)
        {
            _configEntry = new WeakReference<ConfigEntryBase>(configEntry);
            CurrentBoxedValue = configEntry.BoxedValue;
        }
        
        protected ConfigEntryBase ConfigEntry
        {
            get
            {
                if (_configEntry.TryGetTarget(out var configEntry))
                {
                    return configEntry;
                }
                throw new InvalidOperationException("Underlying ConfigEntry has been garbage collected");
            }
        }

        internal PluginInfo Owner
        {
            get
            {
                if (_owner.TryGetTarget(out var owner))
                {
                    return owner;
                }
                throw new InvalidOperationException("Owner of a configuration item has gone out of scope");
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                
                _owner = new WeakReference<PluginInfo>(value);
            }
        }

        internal string Name
        {
            get => _customName ?? ConfigEntry.Definition.Key;
            set => _customName = value;
        }

        internal string Section
        {
            get => _customSection ?? ConfigEntry.Definition.Section;
            set => _customSection = value;
        }

        internal string Description
        {
            get
            {
                var desc = _customDescription ?? ConfigEntry.Description?.Description;
                if (string.IsNullOrEmpty(desc)) return "No description provided.";
                
                var builder = new StringBuilder();
                builder.AppendLine(desc);

                if (ConfigEntry.Description?.AcceptableValues is not { } acceptable) return builder.ToString();
                
                var acceptableValuesDescription = acceptable.ToDescriptionString();
                
                // An *attempt* at guarding against potential BepInEx changes.
                // This is probably fine.
                if (acceptableValuesDescription[0] == '#')
                {
                    acceptableValuesDescription = acceptableValuesDescription[1..].TrimStart();
                }
                builder.AppendLine();
                builder.AppendLine(acceptableValuesDescription);

                return builder.ToString();
            }
            set => _customDescription = value;
        }

        internal bool IsDirty => Equals(CurrentBoxedValue, ConfigEntry.BoxedValue) == false;
        
        internal abstract GameObject CreatePrefab(TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget);

        internal void Commit()
        {
            if (ConfigEntry is null) return;
            ConfigEntry.BoxedValue = CurrentBoxedValue;
        }

        internal void Cancel()
        {
            if (ConfigEntry is null) return;
            CurrentBoxedValue = ConfigEntry.BoxedValue;
        }

        public abstract void Dispose();
    }

    public abstract class BepInConfigWrapper<T> : BepInConfigWrapper, IGenericConfigurable
    {
        private bool _disposed;
        
        protected BepInConfigWrapper(ConfigEntry<T> configEntry) : base(configEntry)
        {
            if (configEntry is not null)
            {
                configEntry.SettingChanged += OnSettingChanged;
            }
        }
        
        internal new ConfigEntry<T> ConfigEntry => (ConfigEntry<T>)base.ConfigEntry;

        internal T Value
        {
            get => (T)CurrentBoxedValue;
            set => CurrentBoxedValue = value;
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            CurrentBoxedValue = ConfigEntry.BoxedValue;
        }
        
        string IGenericConfigurable.Name => Name;
        string IGenericConfigurable.Section => Section;
        string IGenericConfigurable.Description => Description;
        event System.EventHandler IGenericConfigurable.SettingChanged
        {
            add => ConfigEntry.SettingChanged += value;
            remove => ConfigEntry.SettingChanged -= value;
        }

        internal override GameObject CreatePrefab(TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
            throw new NotImplementedException("TODO: Prefab loading and instantiation");
        }

        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            if (ConfigEntry is not null)
            {
                ConfigEntry.SettingChanged -= OnSettingChanged;
            }
            
            GC.SuppressFinalize(this);
        }

        ~BepInConfigWrapper()
        {
            Dispose();
        }
    }

    public interface IGenericConfigurable
    {
        string Name { get; }
        string Section { get; }
        string Description { get; }
        event System.EventHandler SettingChanged;
    }
}