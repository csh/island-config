using System.IO;
using UnityEditor;

namespace Editor
{
    public static class AssetBundleBuilder
    {
        [MenuItem("Build/AssetBundles/Build UI Bundle")]
        public static void BuildUIBundle()
        {
            var outputPath = Path.Combine("AssetBundles");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildPipeline.BuildAssetBundles(
                outputPath,
                BuildAssetBundleOptions.ChunkBasedCompression,
                BuildTarget.StandaloneWindows64
            );

            UnityEngine.Debug.Log($"AssetBundles built to: {Path.GetFullPath(outputPath)}");
        }
    }
}