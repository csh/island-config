using System;
using System.Globalization;
using BepInEx.Configuration;
using IslandConfig.Controllers.UI;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IslandConfig.UI
{
    internal interface ITextInputDefinition : IGenericConfigurable
    {
        string Value { get; set; }
    }
    
    internal interface INumericTextDefinition : ITextInputDefinition
    {
        bool ValidateInput(string value);
    }
    
    public abstract class NumericTextConfigItem<T> : BepInConfigWrapper<T>, INumericTextDefinition where T: IConvertible
    {
        protected NumericTextConfigItem(ConfigEntry<T> configEntry) : base(configEntry)
        {
        }

        string ITextInputDefinition.Value
        {
            get => Value.ToString();
            set => base.Value = (T) Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public abstract bool ValidateInput(string value);

        internal override GameObject CreatePrefab(TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
#if UNITY_EDITOR
            var prefab = Object.Instantiate(IslandConfigAssets.EditorTextPrefab).gameObject;
#else
            var prefab = Object.Instantiate(IslandConfigAssets.TextPrefab);
#endif
            var controller = prefab.GetComponent<TextControllerScript>();
            controller.Initialize(this, hoverNameTarget, hoverDescTarget);
            return prefab.gameObject;
        }
    }

    public class FloatTextInput : NumericTextConfigItem<float>
    {
        public FloatTextInput(ConfigEntry<float> configEntry) : base(configEntry)
        {
        }

        public override bool ValidateInput(string value)
        {
            return float.TryParse(value, out _);
        }
    }

    public class DoubleTextInput : NumericTextConfigItem<double>
    {
        public DoubleTextInput(ConfigEntry<double> configEntry) : base(configEntry)
        {
        }

        public override bool ValidateInput(string value)
        {
            return double.TryParse(value, out _);
        }
    }

    public class DecimalTextInput : NumericTextConfigItem<decimal>
    {
        public DecimalTextInput(ConfigEntry<decimal> configEntry) : base(configEntry)
        {
        }

        public override bool ValidateInput(string value)
        {
            return decimal.TryParse(value, out _);
        }
    }

    public class ByteTextInput : NumericTextConfigItem<byte>
    {
        public ByteTextInput(ConfigEntry<byte> configEntry) : base(configEntry)
        {
        }

        public override bool ValidateInput(string value)
        {
            return byte.TryParse(value, out _);
        }
    }
    
    public class ShortTextInput : NumericTextConfigItem<short>
    {
        public ShortTextInput(ConfigEntry<short> configEntry) : base(configEntry)
        {
        }

        public override bool ValidateInput(string value)
        {
            return short.TryParse(value, out _);
        }
    }
    
    public class IntTextInput : NumericTextConfigItem<int>
    {
        public IntTextInput(ConfigEntry<int> configEntry) : base(configEntry)
        {
        }

        public override bool ValidateInput(string value)
        {
            return int.TryParse(value, out _);
        }
    }
}