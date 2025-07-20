<img src="icon.png"/>

# IslandConfig

An *attempt* at creating a nicely styled mod configuration utility for [Len's Island]. 

*Disclaimer: I have never worked with Unity's UI system before, so this could be completely crap.*

## For Users

[Manual installation of BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) is required until mod managers add support for [Len's Island].

To use IslandConfig for configuring your BepInEx mods ingame:
1. Download the [latest release](https://github.com/csh/island-config/releases/latest).
2. Extract the `plugins` folder to the game `BepInEx` folder.

You only need the `plugins` folder from the ZIP file for manual installation.

<details>
<summary>
<h2>For Developers</h2>
</summary>

By default, IslandConfig will generate appropriate UI elements matching the structure of your BepInEx config bindings.

You **do not** need to add a dependency on this package unless you wish to have control over how the UI gets generated.

### Add NuGet Feed

```sh
dotnet nuget add source https://nuget.pkg.github.com/csh/index.json --name "github/csh" --username <GITHUB_USERNAME> --password <GITHUB_PAT>
```

You must generate a [personal access token (classic)](https://github.com/settings/tokens) with the `read:packages` scope to use this feed.

### Add PackageReference

You can run the command below or add a `<PackageReference>` to your csproj file, whatever floats your boat.

#### CLI

```sh
dotnet add package IslandConfig --version 0.1.0
```

#### PackageReference

```xml
<PackageReference Include="IslandConfig" Version="0.*" PrivateAssets="all" />
```

### Consuming the API

First, you will need to add *at least* a soft dependency on IslandConfig to ensure BepInEx loads things in the correct order.

If you are using a soft dependency you will need to check the [`Chainloader`](https://docs.bepinex.dev/api/BepInEx.Bootstrap.Chainloader.html) for the presence of IslandConfig at runtime.

```csharp
[BepInDependency("com.smrkn.island-config",  BepInDependency.DependencyFlags.SoftDependency)]
```

The example code below uses a hard dependency for simplicity.

```csharp
[BepInPlugin("com.smrkn.example-mod", "IslandConfig Example Mod", "0.1.0")]
[BepInDependency("com.smrkn.island-config")]
public class ExampleMod : BaseUnityPlugin
{
    public static ConfigEntry<bool> EnableCoolFeature;
    public static ConfigEntry<bool> EnableDebugMode;

    private void Awake()
    {
        /*
         * Create a configuration file that looks like this:
         *
         * [General]
         * Debug = false
         * CoolFeature1 = true
         */
        EnableDebugMode = Config.Bind("General", "Debug", false);
        EnableCoolFeature = Config.Bind("General", "CoolFeature1", true);

        // If changing a value requires more complex logic than an if check somewhere
        // then you should subscribe to the SettingChanged event
        EnableCoolFeature.SettingChanged += ToggleCoolFeature;
        
        IslandConfig.Register(builder =>
        {
            // Only CoolFeature1 will be featured in the generated config UI
            builder.WithCheckbox(
                EnableCoolFeature,
                // Define a custom section for grouping settings in the UI
                section: "My First Mod",
                // Define a custom label that will appear on the UI
                label: "Enable Cool Feature 1", 
                // Define a custom description that will appear when the setting is hovered in the UI
                description: "Enabling this will make something cool happen."
            );
        });
    
        // Your init logic
    }

    private void ToggleCoolFeature(object sender, EventArgs e)
    {
        // Do something in response to the config changing, maybe enable/disable a specific Harmony patch.
    }

    private void OnDestroy()
    {
        /*
         * Notify IslandConfig that the plugin is being unloaded.
         *
         * IslandConfig tracks plugins and configuration entries using weak references so that if
         * a plugin is unloaded the relevant config entries are disposed of automatically.
         *
         * IslandConfig was designed with ScriptEngine from BepInEx.Debug in mind, allowing developers
         * to rapidly iterate with hot-reload capabilities, however this relies on Unity's garbage collector
         * disposing of the plugin object before the hot-reload completes to ensure correctness.
         *
         * Therefore manually unregistering is preferred, but not *strictly* necessary.
         */
        IslandConfig.Unregister();
        EnableCoolFeature.SettingChanged -= ToggleCoolFeature;
    }
}
```

*More examples will be added in future as the API expands.*
</details>

## Attribution

[Island icon created by Freepik - Flaticon](https://www.flaticon.com/free-icons/island "island icons")

[Len's Island]: https://store.steampowered.com/app/1335830/Lens_Island/