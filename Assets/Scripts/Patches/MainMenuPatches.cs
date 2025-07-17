using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandConfig.Patches
{
    internal static class MainMenuPatches
    {
        public class HierarchyDumper : MonoBehaviour
        {
            [ContextMenu("Dump Hierarchy")]
            public void Dump() => DumpRecursive(this.transform, 0);

            private void DumpRecursive(Transform t, int depth)
            {
                IslandConfigPlugin.Logger.LogError($"{new string(' ', depth * 2)}{t.name}");
                foreach (Transform child in t)
                    DumpRecursive(child, depth + 1);
            }
        }
        
        public static Transform FindDeepChildByName(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                var result = child.FindDeepChildByName(name);
                if (result != null)
                    return result;
            }
            return null;
        }
        
        public class ComponentDumper : MonoBehaviour
        {
            public bool includeChildren = true;

            private void Awake()
            {
                if (includeChildren)
                {
                    foreach (Transform child in GetComponentsInChildren<Transform>(true))
                    {
                        DumpForObject(child.gameObject);
                    }
                }
                else
                {
                    DumpForObject(gameObject);
                }
            }

            private void DumpForObject(GameObject obj)
            {
                IslandConfigPlugin.Logger.LogInfo($"[{obj.name}] has the following components:");
                foreach (var component in obj.GetComponents<Component>())
                {
                    if (component == null)
                    {
                        IslandConfigPlugin.Logger.LogWarning($" - Missing/NULL component on {obj.name}");
                        continue;
                    }

                    IslandConfigPlugin.Logger.LogInfo($" - {component.GetType().FullName}");
                }
            }
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(MainMenu), nameof(MainMenu.EnableMainMenuGroup))]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal static void InjectMenuButton(MainMenu __instance)
        {
            __instance.StartCoroutine(TryInjectButon(__instance));
        }

        /*
         * Responsibly break the UI. I'm sure nothing bad will happen.
         */
        private static IEnumerator TryInjectButon(MainMenu menu)
        {
            yield return null;

            var buttons = menu.GetComponentsInChildren<Button>();

            if (buttons.Any(b => b.name.Equals("Mod Settings Button")))
            {
                yield break;
            }
            
            var settingsButton = buttons.FirstOrDefault(b => b.name.TrimEnd().Equals("Settings Button", StringComparison.OrdinalIgnoreCase));

            if (settingsButton is null)
            {
                IslandConfigPlugin.Logger.LogWarning("Could not find Settings button.");
                yield break;
            }
            
            settingsButton.transform.parent.gameObject.AddComponent<ComponentDumper>();
            
            IslandConfigPlugin.Logger.LogInfo($"Injecting settings button, cloning {settingsButton.name} as a template");

            var modSettingsButton = UnityEngine.Object.Instantiate(settingsButton.gameObject, settingsButton.transform.parent);
            
            modSettingsButton.name = "Mod Settings Button";

            var originalRt = settingsButton.GetComponent<RectTransform>();
            var rt = modSettingsButton.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = originalRt.anchorMin;
            rt.anchorMax = originalRt.anchorMax;
            rt.pivot = originalRt.pivot;
            rt.sizeDelta = originalRt.sizeDelta;
            rt.anchoredPosition = Vector2.zero;
            modSettingsButton.transform.SetSiblingIndex(settingsButton.transform.GetSiblingIndex() + 1);
            
            var creditsButton = buttons.FirstOrDefault(b => b.name.TrimEnd().Equals("Credits Button", StringComparison.OrdinalIgnoreCase));

            if (creditsButton is not null)
            {
                creditsButton.transform.SetParent(settingsButton.transform.parent, worldPositionStays: false);
             
                var creditsRt = creditsButton.GetComponent<RectTransform>();
                creditsRt.localScale = originalRt.localScale;
                creditsRt.anchorMin = originalRt.anchorMin;
                creditsRt.anchorMax = originalRt.anchorMax;
                creditsRt.pivot = originalRt.pivot;
                creditsRt.sizeDelta = originalRt.sizeDelta;
                creditsRt.anchoredPosition = Vector2.zero;

                creditsButton.transform.SetSiblingIndex(modSettingsButton.transform.GetSiblingIndex() + 1);
                
                var creditsTMP = creditsButton.GetComponentInChildren<TextMeshProUGUI>();
                var creditsTextRT = creditsTMP?.GetComponent<RectTransform>();
                var modSettingsTMP = modSettingsButton.GetComponentInChildren<TextMeshProUGUI>();
                var modSettingsRT = modSettingsTMP?.GetComponent<RectTransform>();
                if (creditsTextRT is not null && modSettingsRT is not null)
                {
                    creditsTextRT.anchorMin = modSettingsRT.anchorMin;
                    creditsTextRT.anchorMax = modSettingsRT.anchorMax;
                    creditsTextRT.offsetMin = modSettingsRT.offsetMin;
                    creditsTextRT.offsetMax = modSettingsRT.offsetMax;
                    creditsTextRT.pivot = modSettingsRT.pivot;
                }
                else
                {
                    IslandConfigPlugin.Logger.LogWarning("Could not adjust Credits Button TMP RT");
                }
                
                if (creditsTMP is not null && modSettingsTMP is not null)
                {
                    creditsTMP.fontSize = modSettingsTMP.fontSize;
                    creditsTMP.fontSizeMin = modSettingsTMP.fontSizeMin;
                    creditsTMP.fontSizeMax = modSettingsTMP.fontSizeMax;
                    creditsTMP.enableAutoSizing = true;
                }
            }
            
            var quitButton = buttons.FirstOrDefault(b => b.name.Contains("Quit", StringComparison.OrdinalIgnoreCase));
            if (quitButton is not null && creditsButton is not null)
            {
                quitButton.transform.SetParent(settingsButton.transform.parent, worldPositionStays: false);

                var quitRT = quitButton.GetComponent<RectTransform>();
                quitRT.localScale = Vector3.one;
                quitRT.anchorMin = originalRt.anchorMin;
                quitRT.anchorMax = originalRt.anchorMax;
                quitRT.pivot = originalRt.pivot;
                quitRT.sizeDelta = originalRt.sizeDelta;
                quitRT.anchoredPosition = Vector2.zero;

                quitButton.transform.SetSiblingIndex(creditsButton.transform.GetSiblingIndex() + 1);
            }
            
            var socialButtons = menu.transform.FindDeepChildByName("Social Buttons");
            if (socialButtons is not null)
            {
                var t = socialButtons.transform;
                t.localPosition += new Vector3(0f, -2f, 0f);
            }
            else
            {
                IslandConfigPlugin.Logger.LogWarning("Could not find Toggle Social Buttons group.");
            }

            var layoutGroup = settingsButton.transform.parent.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup is not null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            }
            
            var tmp = modSettingsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = "Mod Settings";

            modSettingsButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            modSettingsButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                IslandConfigPlugin.Logger.LogInfo("Mod Settings button clicked!");
            });
        }
    }
}