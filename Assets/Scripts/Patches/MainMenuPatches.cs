using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using IslandConfig.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace IslandConfig.Patches
{
    internal static partial class MainMenuPatches
    {
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
            const string modSettingsButtonName = "Mod Settings Button";
            
            yield return null;

            var buttons = menu.GetComponentsInChildren<Button>();

            if (buttons.Any(b => b.name.Equals(modSettingsButtonName)))
            {
                yield break;
            }
            
            var settingsButton = buttons.FirstOrDefault(b => b.name.TrimEnd().Equals("Settings Button", StringComparison.OrdinalIgnoreCase));

            if (settingsButton is null)
            {
                IslandConfigPlugin.Logger.LogWarning("Could not find Settings button.");
                yield break;
            }
            
            IslandConfigPlugin.Logger.LogDebug($"Injecting settings button, cloning {settingsButton.name} as a template");

            var buttonList = settingsButton.transform.parent;
            var modSettingsButton = Object.Instantiate(settingsButton.gameObject, buttonList);
            
            modSettingsButton.name = modSettingsButtonName;

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
            if (tmp is not null)
                tmp.text = "Mod Settings";

            modSettingsButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            modSettingsButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                var existing = GameObject.Find("ModSettingsPanel(Clone)");
                if (existing is not null)
                {
                    existing.SetActive(!existing.activeSelf);
                }
                else
                {
#if UNITY_EDITOR
                    GameObject prefab = null;
#else
                    var prefab = IslandConfigAssets.SettingsWindowPrefab;
#endif
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (!prefab) return;

                    var canvas = buttonList.GetComponentInParent<Canvas>();
                    if (canvas is null)
                    {
                        IslandConfigPlugin.Logger.LogError("Failed to inject mod settings menu");
                        return;
                    }
                    existing = Object.Instantiate(prefab, canvas.transform, false);
                    existing.name = "ModSettingsPanel(Clone)";
                }
                
                if (!existing.TryGetComponent<ModSettingsWindowController>(out var modSettingsWindow))
                {
                    IslandConfigPlugin.Logger.LogError($"Failed to get {nameof(ModSettingsWindowController)}");
                    return;
                }
                
                menu.DisableMainMenuGroup(true);
                modSettingsWindow.CloseHandler = menu.EnableMainMenuGroup;
            });
        }
    }
}
