using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private List<(string modGuid, string modName)> _allMods;

#if UNITY_EDITOR
        private static IEnumerable<(string modGuid, string modName)> GetDummyMods()
        {
            yield return ("com.smrkn.island-config", "IslandConfig");
            yield return ("com.smrkn.example-mod-1", "Example Mod");
            yield return ("com.smrkn.example-mod-2", "Example Mod 2");
        }

        [ContextMenu("Debug: Populate Mod List")]
        private void EditorPopulateModList()
        {
            if (!Application.isPlaying)
                ClearModList();

            _allMods = GetDummyMods().ToList();
            PopulateModList(_allMods);
        }

        [SerializeField] private SliderControllerScript sliderControllerPrefab;

        [ContextMenu("Debug: Populate Settings Panel")]
        private void EditorPopulateSettingsPanel()
        {
            const int startIndex = 5;
            if (!Application.isPlaying)
            {
                for (var i = settingsList.childCount - 1; i >= startIndex; i--)
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
            
            var config = new ConfigFile(cfgPath, false);
            var sliders = new List<INumericSliderDefinition>
            {
                new ByteSliderConfigItem(
                    Bind<byte>(5, 0, 10)
                ),
                
                new ShortSliderConfigItem(
                    Bind<short>(5, 0, 100)
                ),
                new IntSliderConfigItem(
                    Bind(0, -1000, 1000)
                ),
                new FloatSliderConfigItem(
                    Bind(0f, -1f, 1f)
                ),
                new DoubleSliderConfigItem(
                    Bind(0.0, -10.0, 10.0)
                ),
                new DecimalSliderConfigItem(
                    Bind(0m, -100m, 100m)
                )
            };

            foreach (var def in sliders)
            {
                var sliderGameObject = Instantiate(sliderControllerPrefab, settingsList);
                sliderGameObject.Initialize(def);
            }

            return;

            ConfigEntry<T> Bind<T>(T defaultValue, T minValue, T maxValue) where T : IComparable
            {
                return config.Bind("Debug", typeof(T).Name, defaultValue, new ConfigDescription($"{typeof(T).Name} test slider", new AcceptableValueRange<T>(minValue, maxValue)));    
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
#else
            IslandConfig.GenerateConfigs();

            modList = Chainloader.PluginInfos.Values.Where(pi =>
                    IslandConfig.ConfigsByPlugin.TryGetValue(pi, out var wrappers) && wrappers.Count > 0)
                .Select(pi => (pi.Metadata.GUID, pi.Metadata.Name));
#endif
            _allMods = modList.OrderBy(tuple => tuple.modName, StringComparer.OrdinalIgnoreCase).ToList();
            PopulateModList(_allMods);
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
            Debug.Log($"Display the settings for {guid}");
        }
    }
}