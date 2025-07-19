using UnityEngine;

namespace IslandConfig.Patches
{
    public class ComponentDumper : MonoBehaviour
    {
        public bool includeChildren = true;

        private void Awake()
        {
            if (includeChildren)
            {
                foreach (Transform child in GetComponentsInChildren<Transform>(true))
                {
                    DumpForObject(child.gameObject);
                }
            }
            else
            {
                DumpForObject(gameObject);
            }
        }

        private void DumpForObject(GameObject obj)
        {
            IslandConfigPlugin.Logger.LogInfo($"[{obj.name}] has the following components:");
            foreach (var component in obj.GetComponents<Component>())
            {
                if (component == null)
                {
                    IslandConfigPlugin.Logger.LogWarning($" - Missing/NULL component on {obj.name}");
                    continue;
                }

                IslandConfigPlugin.Logger.LogInfo($" - {component.GetType().FullName}");
            }
        }
    }
}