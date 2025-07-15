using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandConfig
{
    [DisallowMultipleComponent]
    public class ModListEntry : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;

#if UNITY_EDITOR
        private void OnValidate()
        {
            Initialize(modGuid ?? "com.smrkn.island-config", modName ?? "Island Config");
        }
#endif
        
        public event Action<string> OnClicked;

#if UNITY_EDITOR
        [Header("Debugging")]
        [Tooltip("Mod GUID")]
        [SerializeField]
#endif
        private string modGuid;

#if UNITY_EDITOR
        [Tooltip("Mod GUID")]
        [SerializeField]
#endif
        private string modName;

        public void Initialize(string modGuid, string modName)
        {
            this.modGuid = modGuid;
            this.modName = modName;

            if (label is not null)
            {
                label.text = modName;
            }
        }
        
        private void Awake()
        {
            label ??= GetComponentInChildren<TMP_Text>();
            button ??= GetComponent<Button>();
            
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnClicked?.Invoke(modGuid);
        }

        private void OnDestroy()
        {
            button?.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
