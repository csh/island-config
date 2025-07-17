using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

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
            Logger.LogEvent += (sender, args) =>
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

            var assemblyFolder = Assembly.GetExecutingAssembly().Location;
            var bundlePath = Path.Combine(Path.GetDirectoryName(assemblyFolder)!, "islandconfigui");
            var bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                Logger.LogError("Failed to load UI AssetBundle!");
                return;
            }
            
#if !UNITY_EDITOR
            IslandConfigAssets.Init(bundle);
#endif
            
            PluginConfig.Init(Config);
            
            _harmony = new Harmony(IslandConfigPluginInfo.Guid);
            _harmony.PatchAll();
            
            Logger.LogInfo($"Loaded {IslandConfigPluginInfo.Name}");

            var count = 0;
            foreach (var patchedMethod in _harmony.GetPatchedMethods())
            {
                Logger.LogDebug($"Successfully patched {patchedMethod.DeclaringType?.Name}::{patchedMethod.Name}");
                count++;
            }
            Logger.LogInfo($"Applied {count} patches.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            IslandConfig.Unregister();
        }
    }
}