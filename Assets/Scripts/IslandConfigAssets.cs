using IslandConfig.Controllers.UI;

namespace IslandConfig
{
    internal static class IslandConfigAssets
    {
#if UNITY_EDITOR
        public static EnumDropdownControllerScript EditorEnumDropdownPrefab;
        public static CheckboxControllerScript EditorCheckboxPrefab;
        public static SliderControllerScript EditorSliderPrefab;
        public static TextControllerScript EditorTextPrefab;
#endif
    }
}