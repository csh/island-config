using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IslandConfig.Controllers.UI
{
    internal class TextControllerScript : MonoBehaviour, IPointerEnterHandler
    {
        [Header("UI References")] 
        [SerializeField] private TMP_InputField textInput;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI hoverName;
        [SerializeField] private TextMeshProUGUI hoverDesc;

        private ITextInputDefinition _definition;
        private Color _defaultTextColour;
        
        public void Initialize(ITextInputDefinition definition, TextMeshProUGUI hoverNameTarget,
            TextMeshProUGUI hoverDescTarget)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            hoverName = hoverNameTarget;
            hoverDesc = hoverDescTarget;
            label.text = definition.Name;
            textInput.SetTextWithoutNotify(definition.Value);
            definition.SettingChanged += OnConfigEntryChanged;
        }

        private void Awake()
        {
            _defaultTextColour = textInput.textComponent.color;
        }

        private void OnEnable()
        {
            textInput?.onValueChanged.AddListener(OnTextInputChanged);
        }

        private void OnDisable()
        {
            textInput?.onValueChanged.RemoveListener(OnTextInputChanged);
        }

        private void OnConfigEntryChanged(object sender, EventArgs e)
        {
            textInput?.SetTextWithoutNotify(_definition.Value);
        }

        private void OnTextInputChanged(string value)
        {
            if (_definition is INumericTextDefinition numeric)
            {
                if (!numeric.ValidateInput(value))
                {
                    textInput.textComponent.color = Color.red;
                    return;
                }
                textInput.textComponent.color = _defaultTextColour;
            }
            _definition.Value = value;
        }

        private void OnDestroy()
        {
            if (_definition is not null)
            {
                _definition.SettingChanged -= OnConfigEntryChanged;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverName is not null) hoverName.text = _definition.Name;
            if (hoverDesc is not null) hoverDesc.text = _definition.Description;
        }
    }
}
