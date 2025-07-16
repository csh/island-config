using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using IslandConfig.Controllers.UI;
using IslandConfig.UI;
using TMPro;
using UnityEngine;

#if !UNITY_EDITOR
using BepInEx.Bootstrap;
#endif

namespace IslandConfig.Controllers
{
    public class ModSettingsWindowController : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        private RectTransform modListContent;

        [SerializeField] private ModListEntryController modListEntryControllerPrefab;
        [SerializeField] private RectTransform settingsList;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private TextMeshProUGUI hoverText;

        private List<(string modGuid, string modName)> _allMods;

#if UNITY_EDITOR
        private enum TestEnum
        {
            Lorem,
            Ipsum
        }

        private static IEnumerable<(string modGuid, string modName)> GetDummyMods()
        {
            yield return (IslandConfigPluginInfo.Guid, IslandConfigPluginInfo.Name);
            yield return ("com.smrkn.debug-mod-1", "Debug Mod 1");
            yield return ("com.smrkn.debug-mod-2", "Debug Mod 2");
        }

        private static readonly Dictionary<string, List<BepInConfigWrapper>> DebugModSettings = new();
        private static ConfigFile _config;

        [ContextMenu("Debug: Populate Prefabs")]
        private void EditorPopulatePrefabs()
        {
            IslandConfigAssets.EditorDropdownPrefab = dropdownControllerPrefab;
            IslandConfigAssets.EditorCheckboxPrefab = checkboxControllerPrefab;
            IslandConfigAssets.EditorSliderPrefab = sliderControllerPrefab;
            IslandConfigAssets.EditorTextPrefab = textControllerPrefab;
            Debug.Log("Prefab list populated");
        }

        [ContextMenu("Debug: Populate Mod List")]
        private void EditorPopulateModList()
        {
            if (!Application.isPlaying)
                ClearModList();

            _allMods = GetDummyMods().ToList();
            PopulateModList(_allMods);
        }

        [SerializeField] private DropdownControllerScript dropdownControllerPrefab;
        [SerializeField] private CheckboxControllerScript checkboxControllerPrefab;
        [SerializeField] private SliderControllerScript sliderControllerPrefab;
        [SerializeField] private TextControllerScript textControllerPrefab;

        [ContextMenu("Debug: Populate Settings Panel")]
        private void EditorPopulateSettingsPanel()
        {
            if (!Application.isPlaying)
            {
                for (var i = settingsList.childCount - 1; i >= 0; i--)
                {
                    var child = settingsList.GetChild(i).gameObject;
                    DestroyImmediate(child);
                }
            }

            var pathsType = typeof(Paths);
            var propertyInfo = pathsType.GetProperty(nameof(Paths.BepInExConfigPath));

            if (propertyInfo is null)
            {
                throw new NullReferenceException("Failed to get Paths.BepInExConfigPath property info");
            }

            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var tempDir = Path.Combine(projectRoot!, "Temp");
            var rootCfgPath = Path.Combine(tempDir, "bepinex.cfg");
            var cfgPath = Path.Combine(tempDir, "debug.cfg");
            Debug.Log($"Using debug config at: {cfgPath}");

            propertyInfo.SetValue(null, rootCfgPath);

            _config = new ConfigFile(cfgPath, false);

            DebugModSettings[IslandConfigPluginInfo.Guid] = new List<BepInConfigWrapper>()
            {
                new TextConfigItem(_config.Bind("General", "Plugin Name", "Island Config")),
                new CheckboxConfigItem(_config.Bind("General", "Enable", true)),
                new IntSliderConfigItem(BindNumericSlider(0, 0, 10)),
                new ByteSliderConfigItem(BindNumericSlider<byte>(0, 1, 3)),
                
                new EnumDropdownConfigItem<TestEnum>(_config.Bind("Dropdowns", "Enum", TestEnum.Lorem)),
                
                new DropdownConfigItem<string>(_config.Bind("Dropdowns", "Generic String", "Foo",
                    new ConfigDescription("A generic dropdown with acceptable values",
                        new AcceptableValueList<string>("Foo", "Bar")))),
                
                new DropdownConfigItem<int>(_config.Bind("Dropdowns", "Generic Number", 15,
                    new ConfigDescription("A generic dropdown with acceptable values",
                        new AcceptableValueList<int>(10, 15, 25))))
            };

            DebugModSettings["com.smrkn.debug-mod-1"] = new List<BepInConfigWrapper>()
            {
                new ByteSliderConfigItem(
                    BindNumericSlider<byte>(5, 0, 10)
                ),
                new ShortSliderConfigItem(
                    BindNumericSlider<short>(5, 0, 100)
                ),
                new IntSliderConfigItem(
                    BindNumericSlider(0, -1000, 1000)
                ),
                new FloatSliderConfigItem(
                    BindNumericSlider(0f, -1f, 1f)
                ),
                new DoubleSliderConfigItem(
                    BindNumericSlider(0.0, -10.0, 10.0)
                ),
                new DecimalSliderConfigItem(
                    BindNumericSlider(0m, -100m, 100m)
                ),
            };

            DebugModSettings["com.smrkn.debug-mod-2"] = new List<BepInConfigWrapper>()
            {
                new TextConfigItem(_config.Bind("General", "Something Practical", "Dummy Config")),
                new IntTextInput(BindNumericTextInput(5)),
                new ByteTextInput(BindNumericTextInput<byte>(10)),
                new ShortTextInput(BindNumericTextInput<short>(15)),
                new FloatTextInput(BindNumericTextInput(20f)),
                new DoubleTextInput(BindNumericTextInput(25d)),
                new DecimalTextInput(BindNumericTextInput<decimal>(30)),
            };

            return;

            ConfigEntry<T> BindNumericSlider<T>(T defaultValue, T minValue, T maxValue) where T : IComparable
            {
                return _config.Bind("Debug", $"Debug {typeof(T).Name}", defaultValue,
                    new ConfigDescription($"{typeof(T).Name} test slider",
                        new AcceptableValueRange<T>(minValue, maxValue)));
            }

            ConfigEntry<T> BindNumericTextInput<T>(T defaultValue) where T : IConvertible
            {
                return _config.Bind("Numeric Input", $"{typeof(T).Name} Text Input", defaultValue);
            }
        }
#endif

        private void OnEnable()
        {
            searchInput?.onValueChanged.AddListener(OnSearchInputUpdated);

            // ReSharper disable once JoinDeclarationAndInitializer
            IEnumerable<(string modGuid, string modName)> modList;
#if UNITY_EDITOR
            modList = GetDummyMods();
            EditorPopulatePrefabs();
            EditorPopulateSettingsPanel();
#else
            IslandConfig.GenerateConfigs();

            modList = Chainloader.PluginInfos.Values.Where(pi =>
                    IslandConfig.ConfigsByPlugin.TryGetValue(pi, out var wrappers) && wrappers.Count > 0)
                .Select(pi => (pi.Metadata.GUID, pi.Metadata.Name));
#endif
            _allMods = modList.OrderBy(tuple => tuple.modName, StringComparer.OrdinalIgnoreCase).ToList();
            PopulateModList(_allMods);

            OnModSelected(IslandConfigPluginInfo.Guid);
        }

        private void OnDisable()
        {
            searchInput?.onValueChanged.RemoveListener(OnSearchInputUpdated);
        }

        private void ClearModList()
        {
            Debug.Log("Attempting to clear mod list");
            for (var i = modListContent.childCount - 1; i >= 0; i--)
            {
                var child = modListContent.GetChild(i).gameObject;
                var modListEntry = child.GetComponent<ModListEntryController>();

                if (modListEntry is not null)
                {
                    modListEntry.OnClicked -= OnModSelected;
                }

#if UNITY_EDITOR
                DestroyImmediate(child);
#else
                Destroy(child);
#endif
            }
        }

        private void PopulateModList(IEnumerable<(string modGuid, string modName)> modList)
        {
            ClearModList();

            Debug.Log("Populating mod list");
            foreach (var (modGuid, modName) in modList)
            {
                var entry = Instantiate(modListEntryControllerPrefab, modListContent);
                entry.Initialize(modGuid, modName);
                entry.OnClicked += OnModSelected;
            }
        }

        private void OnSearchInputUpdated(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName))
            {
                PopulateModList(_allMods);
                return;
            }

            var filtered = _allMods.Where(x =>
                x.modName.IndexOf(modName, StringComparison.OrdinalIgnoreCase) != -1);

            PopulateModList(filtered);
        }

        private void OnModSelected(string guid)
        {
            // TODO: Check for dirty config wrappers before navigating away.

            Debug.Log($"Display the settings for {guid}");
            for (var i = settingsList.childCount - 1; i >= 0; i--)
            {
                var child = settingsList.GetChild(i).gameObject;
#if UNITY_EDITOR
                DestroyImmediate(child);
#else
                Destroy(child);
#endif
            }

#if UNITY_EDITOR
            if (!DebugModSettings.TryGetValue(guid, out var wrappers))
            {
                Debug.LogWarning($"[IslandConfig] Could not find config wrappers for {guid}");
                return;
            }
#else
            if (!Chainloader.PluginInfos.TryGetValue(guid, out var info))
            {
                Debug.LogWarning($"[IslandConfig] Could not find PluginInfo for {guid}");
                return;
            }

            if (!IslandConfig.ConfigsByPlugin.TryGetValue(info, out var wrappers))
            {
                Debug.LogWarning($"[IslandConfig] Could not find config wrappers for {guid}");
                return;
            }
#endif

            var grouped = wrappers.GroupBy(wrapper => wrapper.Section).OrderBy(wrapper => wrapper.Key);

            foreach (var group in grouped)
            {
                foreach (var wrapper in group)
                {
                    var configItem = wrapper.CreatePrefab(hoverText);

                    configItem.transform.SetParent(settingsList, false);
                }
            }
        }
    }
}