#if UNITY_EDITOR && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[InitializeOnLoad]
public static class ThunderKitInputSystemEditorPatch
{
    static ThunderKitInputSystemEditorPatch()
    {
        try
        {
            // Prevent crashes in editor caused by InputSystem update type
            InputSystem.onBeforeUpdate = updateType =>
            {
                if (updateType == InputUpdateType.Editor)
                {
                    // Skip update to avoid crash
                    return;
                }
            };

            InputSystem.onUpdate = updateType =>
            {
                if (updateType == InputUpdateType.Editor)
                {
                    // Skip update to avoid crash
                    return;
                }
            };
        }
        catch
        {
            // Suppress exceptions in case types are missing or corrupted
        }
    }
}
#endif
