using BepInEx.Configuration;

namespace IslandConfig
{
    internal static class PluginConfig
    {
        public static ConfigEntry<bool> AutoGenerateUI;

        public static void Init(ConfigFile config)
        {
            IslandConfigPlugin.Logger.LogDebug("Setting up configuration bindings.");
            
            AutoGenerateUI = config.Bind("IslandConfig", "AutoUI", true);
        }

        internal static void InitUserInterface()
        {
            IslandConfigPlugin.Logger.LogDebug("Registering configuration UI.");
            
            IslandConfig.Register(builder =>
            {
                builder.WithCheckbox(
                    AutoGenerateUI,
                    label: "Generate Config UI",
                    description: "Automatically generate configuration UIs for BepInEx plugins that are not using IslandConfig"
                );
            });
        }
    }
}