using System;
using BepInEx.Configuration;
using IslandConfig.Controllers.UI;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IslandConfig.UI
{
    internal interface INumericSliderDefinition : IGenericConfigurable
    {
        bool IsWholeNumberType { get; }
        float Min { get; }
        float Max { get; }
        float FloatValue { get; set; }
    }
    
    public abstract class NumericSliderConfigItem<T> : BepInConfigWrapper<T>, INumericSliderDefinition where T : IComparable, IConvertible
    {
        private T MinValue { get; }
        private T MaxValue { get; }

        protected NumericSliderConfigItem(ConfigEntry<T> configEntry, bool isWholeNumberType, T defaultMinValue, T defaultMaxValue) : base(configEntry)
        {
            IsWholeNumberType = isWholeNumberType;

            switch (configEntry.Description.AcceptableValues)
            {
                case AcceptableValueRange<T> range:
                    MinValue = range.MinValue;
                    MaxValue = range.MaxValue;
                    break;
                // TODO: Default to generic numeric input in the event of absence of an acceptable value range?
                case null:
                    MinValue = defaultMinValue;
                    MaxValue = defaultMaxValue;
                    break;
                default:
                    throw new ArgumentException(
                        $"{configEntry.Definition.Key} has incorrect constraint type, expected {nameof(AcceptableValueRange<T>)}",
                        nameof(configEntry));
            }
        }

        private bool UpdateFromFloat(float value)
        {
            var min = Convert.ToSingle(MinValue);
            var max = Convert.ToSingle(MaxValue);
            var clamped = Mathf.Clamp(value, min, max);
            
            if (IsWholeNumberType)
                clamped = Mathf.Round(clamped);
            
            var raw = Convert.ChangeType(clamped, typeof(T));
            if (Equals(raw, Value))
            {
                return false;
            }
            
            CurrentBoxedValue = raw;
            return true;
        }

        public bool IsWholeNumberType { get; }
        public float Min => Convert.ToSingle(MinValue);
        public float Max => Convert.ToSingle(MaxValue);
        public float FloatValue
        {
            get => Convert.ToSingle(CurrentBoxedValue);
            set => UpdateFromFloat(value);
        }
        
        internal override GameObject CreatePrefab(TextMeshProUGUI hoverNameTarget, TextMeshProUGUI hoverDescTarget)
        {
#if UNITY_EDITOR
            var prefab = Object.Instantiate(IslandConfigAssets.EditorSliderPrefab).gameObject;
#else
            var prefab = Object.Instantiate(IslandConfigAssets.SliderPrefab);
#endif
            var controller = prefab.GetComponent<SliderControllerScript>();
            controller.Initialize(this, hoverNameTarget, hoverDescTarget);
            return prefab.gameObject;
        }
    }

    public class FloatSliderConfigItem : NumericSliderConfigItem<float>
    {
        public FloatSliderConfigItem(ConfigEntry<float> configEntry) : base(configEntry, false, float.MinValue,
            float.MaxValue)
        {
        }
    }

    public class DoubleSliderConfigItem : NumericSliderConfigItem<double>
    {
        public DoubleSliderConfigItem(ConfigEntry<double> configEntry) : base(configEntry, false, double.MinValue,
            double.MaxValue)
        {
        }
    }

    public class DecimalSliderConfigItem : NumericSliderConfigItem<decimal>
    {
        public DecimalSliderConfigItem(ConfigEntry<decimal> configEntry) : base(configEntry, false, decimal.MinValue,
            decimal.MaxValue)
        {
        }
    }

    public class ByteSliderConfigItem : NumericSliderConfigItem<byte>
    {
        public ByteSliderConfigItem(ConfigEntry<byte> configEntry) : base(configEntry, true, byte.MinValue,
            byte.MaxValue)
        {
        }
    }

    public class ShortSliderConfigItem : NumericSliderConfigItem<short>
    {
        public ShortSliderConfigItem(ConfigEntry<short> configEntry) : base(configEntry, true, short.MinValue,
            short.MaxValue)
        {
        }
    }

    public class IntSliderConfigItem : NumericSliderConfigItem<int>
    {
        public IntSliderConfigItem(ConfigEntry<int> configEntry) : base(configEntry, true, int.MinValue, int.MaxValue)
        {
        }
    }
}