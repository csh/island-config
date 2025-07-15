using System;
using BepInEx.Configuration;

namespace IslandConfig.UI
{
    public abstract class NumericSliderConfigItem<T> : BepInConfigWrapper<T> where T : IComparable
    {
        internal readonly bool IsWholeNumberType;

        public T MinValue { get; protected set; }
        public T MaxValue { get; protected set; }

        protected NumericSliderConfigItem(ConfigEntry<T> configEntry, bool isWholeNumberType, T defaultMinValue, T defaultMaxValue) : base(configEntry)
        {
            IsWholeNumberType = isWholeNumberType;

            switch (configEntry.Description.AcceptableValues)
            {
                case AcceptableValueRange<T> range:
                    MinValue = range.MinValue;
                    MaxValue = range.MaxValue;
                    break;
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