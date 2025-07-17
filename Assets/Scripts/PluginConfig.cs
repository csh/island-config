using BepInEx.Configuration;

namespace IslandConfig
{
    internal static class PluginConfig
    {
        public static ConfigEntry<bool> AutoGenerateUI;

        public static void Init(ConfigFile config)
        {
            AutoGenerateUI = config.Bind("IslandConfig", "AutoUI", true);
        }

        internal static void InitUserInterface()
        {
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