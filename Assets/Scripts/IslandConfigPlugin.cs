using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

#if !UNITY_EDITOR
using IslandConfig.Patches;
using System.IO;
using System.Reflection;
#endif

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
        
        internal static IslandConfigPlugin Instance { get; private set; }
        
        private Harmony _harmony;
        
        private void Awake()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("IslandConfigPlugin is already initialized");
            }
            
            Instance = this;
            Logger = base.Logger;
            _harmony = new Harmony(IslandConfigPluginInfo.Guid);

            PluginConfig.Init(Config);
#if !UNITY_EDITOR
            var assemblyFolder = Assembly.GetExecutingAssembly().Location;
            var bundlePath = Path.Combine(Path.GetDirectoryName(assemblyFolder)!, "islandconfigui");
            var bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                Logger.LogError("Failed to load UI AssetBundle!");
                return;
            }
            IslandConfigAssets.Init(bundle);
            _harmony.PatchAll(typeof(MainMenuPatches));
#endif
            
            Logger.LogInfo($"Loaded {IslandConfigPluginInfo.Name}");
            
            var count = 0;
            foreach (var patchedMethod in _harmony.GetPatchedMethods())
            {
                Logger.LogDebug($"Successfully patched {patchedMethod.DeclaringType?.Name}::{patchedMethod.Name}");
                count++;
            }
            Logger.LogInfo($"Applied {count} patches.");
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