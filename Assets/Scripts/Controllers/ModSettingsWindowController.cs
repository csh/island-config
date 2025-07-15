using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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
            PopulateModList(GetDummyMods());
        }
#endif

        private void Awake()
        {
            ClearModList();
            
#if UNITY_EDITOR
            _allMods = GetDummyMods().ToList();
            PopulateModList(_allMods);
#else
            IslandConfig.GenerateConfigs();
#endif

            searchInput?.onValueChanged.AddListener(OnSearchInputUpdated);
        }

        private void OnDestroy()
        {
            searchInput?.onValueChanged.RemoveListener(OnSearchInputUpdated);
        }

        private void ClearModList()
        {
            Debug.Log("Attempting to clear mod list");
            for (var i = modListContent.childCount-1; i >= 0; i--)
            {
                var child = modListContent.GetChild(i).gameObject;
#if UNITY_EDITOR
                DestroyImmediate(child);
#else
                Destroy(child);
#endif
            }
        }

        public void PopulateModList(IEnumerable<(string modGuid, string modName)> modList)
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