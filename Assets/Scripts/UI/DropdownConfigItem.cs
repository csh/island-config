using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace IslandConfig.UI
{
    public interface IEnumDropdownDefinition
    {
        string Name { get; }
        string Section { get; }
        string Description { get; }
        int SelectedIndex { get; set; }
        List<string> Options { get; }
        event EventHandler SettingChanged;
    }
    
    public class EnumDropdownConfigItem<T> : BepInConfigWrapper<T>, IEnumDropdownDefinition where T: Enum
    { 
        private readonly List<string> _options;
        
        public EnumDropdownConfigItem(ConfigEntry<T> configEntry) : base(configEntry)
        {
            _options = Enum.GetNames(typeof(T)).ToList();
        }

        string IEnumDropdownDefinition.Name => Name;
        string IEnumDropdownDefinition.Section => Section;
        string IEnumDropdownDefinition.Description => Description;
        List<string> IEnumDropdownDefinition.Options => _options;
        
        int IEnumDropdownDefinition.SelectedIndex 
        {
            get
            {
                var currentName = Value!.ToString();
                return _options.IndexOf(currentName);
            }
            set
            {
                if (value < 0 || value >= _options.Count)
                    throw new ArgumentOutOfRangeException(nameof(value));
                Value = (T)Enum.Parse(typeof(T), _options[value]);
            }
        }
        
        event EventHandler IEnumDropdownDefinition.SettingChanged
        {
            add => ConfigEntry.SettingChanged += value;
            remove => ConfigEntry.SettingChanged -= value;
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