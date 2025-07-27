using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IslandConfig.Controllers.UI
{
    [DisallowMultipleComponent]
    public abstract class SettingsControllerBase : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] protected TextMeshProUGUI label;
        [SerializeField] protected TextMeshProUGUI hoverName;
        [SerializeField] protected TextMeshProUGUI hoverDesc;
        [SerializeField] protected Button resetButton;
        
        public abstract void ForceUpdateElement();
    }
    
    public abstract class SettingControllerBase<T> : SettingsControllerBase, IPointerEnterHandler where T: IGenericConfigurable
    {
        protected T Definition;

        public void Initialize(T definition, TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            hoverName = hoverNameTarget;
            hoverDesc = hoverDescTarget;
            label.text = definition.Name;
            
            Definition.SettingChanged += OnSettingChanged;
        }

        private void Awake()
        {
            resetButton?.onClick.AddListener(SetToDefault);
        }

        protected void OnDestroy()
        {
            if (Definition is not null)
            {
                Definition.SettingChanged -= OnSettingChanged;
            }
        }

        protected abstract void OnSettingChanged(object sender, EventArgs e);

        private void SetToDefault()
        {
            Definition.ResetToDefault();
            ForceUpdateElement();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverName is not null) hoverName.text = Definition.Name;
            if (hoverDesc is not null) hoverDesc.text = Definition.Description;
        }
    }
}