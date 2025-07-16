using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using IslandConfig.Controllers.UI;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IslandConfig.UI
{
    public interface IDropdownDefinition : IGenericConfigurable
    {
        int SelectedIndex { get; set; }
        List<string> Labels { get; }
    }

    public class DropdownBaseItem<T> : BepInConfigWrapper<T>, IDropdownDefinition
    {
        protected readonly List<string> Labels;
        protected readonly List<T> Options;

        protected DropdownBaseItem(ConfigEntry<T> configEntry) : base(configEntry)
        {
            Labels = new List<string>();
            Options = new List<T>();
        }

        internal override GameObject CreatePrefab(TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
#if UNITY_EDITOR
            var prefab = Object.Instantiate(IslandConfigAssets.EditorDropdownPrefab).gameObject;
#else
            var prefab = Object.Instantiate(IslandConfigAssets.DropdownPrefab);
#endif
            var controller = prefab.GetComponent<DropdownControllerScript>();
            controller.Initialize(this, hoverNameTarget, hoverDescTarget);
            return prefab.gameObject;
        }

        string IGenericConfigurable.Name => Name;
        string IGenericConfigurable.Section => Section;
        string IGenericConfigurable.Description => Description;
        event EventHandler IGenericConfigurable.SettingChanged
        {
            add => ConfigEntry.SettingChanged += value;
            remove => ConfigEntry.SettingChanged -= value;
        }

        int IDropdownDefinition.SelectedIndex
        {
            get => Options.IndexOf(Value);
            set {
                if (value < 0 || value >= Options.Count) return;
                Value = Options[value];
            }
        }

        List<string> IDropdownDefinition.Labels => Labels;
    }

    public class EnumDropdownConfigItem<T> : DropdownBaseItem<T> where T: Enum
    {
        public EnumDropdownConfigItem(ConfigEntry<T> configEntry) : base(configEntry)
        {
            Options.AddRange(Enum.GetValues(typeof(T)).Cast<T>());
            Labels.AddRange(Options.Select(e => Enum.GetName(typeof(T), e)));
        }
    }

    public class DropdownConfigItem<T> : DropdownBaseItem<T> where T : IEquatable<T>
    {
        public DropdownConfigItem(ConfigEntry<T> configEntry) : base(configEntry)
        {
            if (configEntry.Description.AcceptableValues is AcceptableValueList<T> list)
            {
                Options.AddRange(list.AcceptableValues);
                Labels.AddRange(list.AcceptableValues.Select(v => v.ToString()));
            }
            else
            {
                throw new ArgumentException($"Dropdown for \"{Section}/{Name}\" entry requires {nameof(AcceptableValueList<T>)}", nameof(configEntry));
            }
        }
    }
}