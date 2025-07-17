using System;
using IslandConfig.UI;
using TMPro;
using UnityEngine;

namespace IslandConfig.Controllers.UI
{
    internal class TextControllerScript : SettingControllerBase<ITextInputDefinition>
    {
        [SerializeField] private TMP_InputField textInput;
        private Color _defaultTextColour;
        
        internal override void Initialize(ITextInputDefinition definition, TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
            base.Initialize(definition, hoverNameTarget, hoverDescTarget);
            textInput.SetTextWithoutNotify(definition.Value);
        }
        
        private void Awake() => _defaultTextColour = textInput.textComponent.color;
        
        private void OnEnable() => textInput?.onValueChanged.AddListener(OnTextChanged);

        private void OnDisable() => textInput?.onValueChanged.RemoveListener(OnTextChanged);

        private void OnTextChanged(string value)
        {
            if (Definition is null || string.Equals(Definition.Value, value, StringComparison.InvariantCulture)) return;
            
            if (Definition is INumericTextDefinition numeric)
            {
                if (!numeric.ValidateInput(value))
                {
                    textInput.textComponent.color = Color.red;
                    return;
                }
                textInput.textComponent.color = _defaultTextColour;
            }
            Definition.Value = value;
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            if (textInput is null || string.Equals(textInput.text,  Definition.Value, StringComparison.InvariantCulture)) return;
            textInput.SetTextWithoutNotify(Definition.Value);
        }

        internal override void ForceUpdateElement()
        {
            if (textInput is null || Definition is null) return;
            textInput.SetTextWithoutNotify(Definition.Value);
        }
    }
}
