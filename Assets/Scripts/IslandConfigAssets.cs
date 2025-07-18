#if UNITY_EDITOR
using IslandConfig.Controllers.UI;
#else
using System;
using UnityEngine;
#endif

namespace IslandConfig
{
    internal static class IslandConfigAssets
    {
#if UNITY_EDITOR
        public static DropdownControllerScript EditorDropdownPrefab;
        public static CheckboxControllerScript EditorCheckboxPrefab;
        public static SliderControllerScript EditorSliderPrefab;
        public static TextControllerScript EditorTextPrefab;
#else
        private static AssetBundle _bundle;

        internal static void Init(AssetBundle bundle)
        {
            if (_bundle is not null)
            {
                throw new InvalidOperationException("IslandConfig AssetBundle has already been set!");
            }
            _bundle = bundle;
        }

        internal static GameObject SettingsWindowPrefab => _bundle.LoadAsset<GameObject>("Mod Settings Window");
        internal static GameObject DropdownPrefab => _bundle.LoadAsset<GameObject>("Dropdown Config Item");
        internal static GameObject CheckboxPrefab => _bundle.LoadAsset<GameObject>("Toggle Config Item");
        internal static GameObject SliderPrefab => _bundle.LoadAsset<GameObject>("Slider Config Item");
        internal static GameObject TextPrefab => _bundle.LoadAsset<GameObject>("Text Config Item");
#endif
    }
}