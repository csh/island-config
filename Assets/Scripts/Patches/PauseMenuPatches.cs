using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using IslandConfig.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandConfig.Patches
{
    internal static class PauseMenuPatches
    {
        
        [HarmonyPostfix, HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.OnPause))]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal static void InjectMenuButton(PauseMenu __instance)
        {
            __instance.StartCoroutine(TryInjectPauseButton(__instance));
        }

        private static IEnumerator TryInjectPauseButton(PauseMenu menu)
        {
            const string modSettingsButtonName = "Mod Settings Button";
            
            yield return null;

            var buttons = menu.GetComponentsInChildren<Button>();

            if (buttons.Any(button => string.Equals(button.name, modSettingsButtonName)))
            {
                IslandConfigPlugin.Logger.LogInfo("Mod settings button is already injected");
                yield break;
            }

            var settingsButton = buttons.FirstOrDefault(button => button.name.TrimEnd().Equals("Settings Button"));

            if (settingsButton is null)
            {
                IslandConfigPlugin.Logger.LogError("Could not find Settings Button to clone");
                yield break;
            }

            IslandConfigPlugin.Logger.LogInfo($"Injecting mod settings button into pause menu, using \"{settingsButton.name}\" as template");
            
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

            var buttonListRt = (RectTransform)buttonList;
            var buttonListSize = buttonListRt.sizeDelta;
            buttonListSize.y += 35;
            buttonListRt.sizeDelta = buttonListSize;
            
            IslandConfigPlugin.Logger.LogDebug("Adjusting pause menu height");
            for (int i = 0; i < menu.mainPauseDialg.transform.childCount; i++)
            {
                var child =  menu.mainPauseDialg.transform.GetChild(i);
                if (!child.name.Equals("Background")) continue;
                if (!child.gameObject.TryGetComponent<RectTransform>(out var childRt)) continue;
                IslandConfigPlugin.Logger.LogDebug("Adjusting background height to accomodate our injected button");
                var delta = childRt.sizeDelta;
                delta.y += 40f;
                childRt.sizeDelta = delta;
            }

            var tmp = modSettingsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp is not null)
                tmp.text = "Mod Settings";
            
            modSettingsButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            
            modSettingsButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                IslandConfigPlugin.Logger.LogInfo("Mod Settings button clicked!");
                
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

                menu.resetPositionConfirmationWindow.SetActive(false);
                menu.pauseGroup.SetActive(false);

                modSettingsWindow.CloseHandler = () =>
                {
                    menu.pauseGroup.SetActive(true);
                };
            });
        }
    }
}