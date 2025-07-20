using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using IslandConfig.Patches;
using System.IO;
using System.Reflection;

namespace IslandConfig
{
    internal static class IslandConfigPluginInfo
    {
        public const string Guid = "com.smrkn.island-config";
        public const string Name = "IslandConfig";
        public const string Version = "0.1.0";
    }
    
    [BepInPlugin(IslandConfigPluginInfo.Guid, IslandConfigPluginInfo.Name, IslandConfigPluginInfo.Version)]
    internal class IslandConfigPlugin : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;
        
#if UNITY_EDITOR
        static IslandConfigPlugin()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("IslandConfigEditor");
            Logger.LogEvent += (_, args) =>
            {
                switch (args.Level)
                {
                    case LogLevel.Error:
                    case LogLevel.Fatal:
                        Debug.LogError(args.ToString());
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(args.ToString());
                        break;
                    case LogLevel.Debug:
                    case LogLevel.Info:
                    case LogLevel.Message:
                        Debug.Log(args.ToString());
                        break;
                    case LogLevel.None:
                    case LogLevel.All:
                    default:
                        return;
                }
            };
        }
#endif
        
        
        private Harmony _harmony;
        
        private void Awake()
        {
            Logger = base.Logger;
            
            Logger.LogInfo($"{IslandConfigPluginInfo.Name} is initializing.");
            PluginConfig.Init(Config);

            var assemblyFolder = Assembly.GetExecutingAssembly().Location;
            var bundlePath = Path.Combine(Path.GetDirectoryName(assemblyFolder)!, "islandconfigui");
            var bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                Logger.LogError("Failed to load UI AssetBundle!");
                return;
            }
            
            Logger.LogInfo("Loading UI assets");
            IslandConfigAssets.Init(bundle);
            
            _harmony = new Harmony(IslandConfigPluginInfo.Guid);
            _harmony.PatchAll(typeof(PauseMenuPatches));
            _harmony.PatchAll(typeof(MainMenuPatches));
            
            
            var count = 0;
            foreach (var patchedMethod in _harmony.GetPatchedMethods())
            {
                Logger.LogDebug($"Successfully patched {patchedMethod.DeclaringType?.Name}::{patchedMethod.Name}");
                count++;
            }
            Logger.LogInfo($"Applied {count} patches.");
            Logger.LogInfo($"{IslandConfigPluginInfo.Name} initialized.");
        }

        private void Start()
        {
            Logger.LogDebug("Attempting to register configuration UI");
            PluginConfig.InitUserInterface();
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            IslandConfig.Unregister();
        }
    }
}