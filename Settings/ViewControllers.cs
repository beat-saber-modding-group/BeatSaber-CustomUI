using CustomUI.UIElements;
using CustomUI.BeatSaber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using HMUI;

namespace CustomUI.Settings
{
    public interface CustomSetting
    {
        void Init();
        
        void ApplySettings();

        void CancelSettings();

        bool IsInitialized { get; set; }
    }

    public class SubMenuSettingsController : CustomSetting
    {
        public bool IsInitialized { get; set; } = false;
        public void ApplySettings() { }
        public void CancelSettings() { }
        public void Init() { }
    }

    public class BoolViewController : SwitchSettingsController, CustomSetting
    {
        public delegate bool GetBool();
        public event GetBool GetValue;

        public delegate void SetBool(bool value);
        public event SetBool SetValue;

        public string EnabledText = "ON";
        public string DisabledText = "OFF";
        public bool applyImmediately = false;

        public bool IsInitialized { get; set; } = false;

        bool lastValue;

        protected override void OnEnable()
        {
            if (IsInitialized)
                base.OnEnable();
        }

        protected override bool GetInitValue()
        {
            bool value = false;
            if (GetValue != null)
            {
                value = GetValue();
            }
            return value;
        }

        protected override void ApplyValue(bool value)
        {
            lastValue = value;
            if (applyImmediately) ApplySettings();
        }

        public void ApplySettings()
        {
            SetValue?.Invoke(lastValue);
        }

        public void CancelSettings()
        {
            if (GetValue != null)
                lastValue = GetValue();
        }

        protected override string TextForValue(bool value)
        {
            return (value) ? EnabledText : DisabledText;
        }

        public void Init()
        {
            OnDisable();
            IsInitialized = true;
            OnEnable();
        }
    }

    public abstract class IntSettingsController : IncDecSettingsController, CustomSetting
    {
        private int _value;
        protected int _min;
        protected int _max;
        protected int _increment = 1;
        public bool applyImmediately = false;

        public bool IsInitialized { get; set; } = false;

        protected abstract int GetInitValue();
        protected abstract void ApplyValue(int value);
        protected abstract string TextForValue(int value);
        
        public void Init()
        {
            _value = GetInitValue();
            RefreshUI();
            IsInitialized = true;
        }
        public void ApplySettings()
        {
            ApplyValue(_value);
        }

        public void CancelSettings()
        {
            _value = GetInitValue();
        }

        private void RefreshUI()
        {
            text = this.TextForValue(this._value);
            enableDec = _value > _min;
            enableInc = _value < _max;
        }

        public override void IncButtonPressed()
        {
            _value += _increment;
            if (_value > _max) _value = _max;
            RefreshUI();
            if (applyImmediately)
                ApplySettings();
        }

        public override void DecButtonPressed()
        {
            _value -= _increment;
            if (_value < _min) _value = _min;
            RefreshUI();
            if (applyImmediately)
                ApplySettings();
        }
    }

    public class IntViewController : IntSettingsController
    {
        public delegate int GetInt();
        public event GetInt GetValue;

        public delegate void SetInt(int value);
        public event SetInt SetValue;

        public void SetValues(int min, int max, int increment)
        {
            _min = min;
            _max = max;
            if (increment < 1)
                increment = 1;
            _increment = increment;
        }

        public void UpdateIncrement(int increment)
        {
            if (increment < 1)
                increment = 1;
            _increment = increment;
        }

        private int FixValue(int value)
        {
            if (value % _increment != 0)
                value -= (value % _increment);

            if (value > _max) value = _max;
            if (value < _min) value = _min;
            return value;
        }

        protected override int GetInitValue()
        {
            int value = 0;
            if (GetValue != null)
            {
                value = FixValue(GetValue());
            }
            return value;
        }

        protected override void ApplyValue(int value)
        {
            SetValue?.Invoke(FixValue(value));
        }

        protected override string TextForValue(int value)
        {
            return value.ToString();
        }
    }
    
    public class StringViewController : ListSettingsController, CustomSetting
    {
        public Func<string> GetValue = () => String.Empty;
        public Action<string> SetValue = (_) => { };
        public string value = String.Empty;
        public bool applyImmediately = false;

        public bool IsInitialized { get; set; } = false;

        private bool _hasInited;

        protected override void OnEnable()
        {
            if (IsInitialized)
                base.OnEnable();
        }

        protected override void GetInitValues(out int idx, out int numberOfElements)
        {
            numberOfElements = 2;
            idx = 0;
            if (_hasInited) return;
            _hasInited = true;
            value = GetValue();
        }

        protected override void ApplyValue(int idx)
        {
            
        }

        protected override string TextForValue(int idx)
        {
            if (value != String.Empty)
                return value;
            else
                return "<color=#ffffff66>Empty</color>";
        }

        public override void IncButtonPressed()
        {
            BeatSaberUI.DisplayKeyboard("Enter Text Below", value, (text) => { }, (text) => { value = text; base.IncButtonPressed(); base.DecButtonPressed(); });
        }
        public override void DecButtonPressed()
        {
            if (applyImmediately)
                ApplySettings();
        }

        public void ApplySettings()
        {
            SetValue(value);
        }

        public void CancelSettings()
        {
            value = GetValue();
        }

        public void Init()
        {
            OnDisable();
            IsInitialized = true;
            OnEnable();
        }
    }

    public class ListViewController : ListSettingsController, CustomSetting
    {
        public Func<float> GetValue = () => 0f;
        public Action<float> SetValue = (_) => { };
        public Func<float, string> GetTextForValue = (_) => "?";

        public delegate string StringForValue(float value);
        public event StringForValue FormatValue;

        public List<float> values = new List<float>();
        public bool applyImmediately = false;

        public bool IsInitialized { get; set; } = false;

        int lastidx;

        protected override void OnEnable()
        {
            if (IsInitialized)
                base.OnEnable();
        }

        protected override void GetInitValues(out int idx, out int numberOfElements)
        {
            numberOfElements = values.Count();
            var value = GetValue();
            idx = values.FindIndex(v => v == value);
            if (idx == -1)
                idx = 0;
        }

        protected override void ApplyValue(int idx)
        {
            lastidx = idx;
            if (applyImmediately)
                ApplySettings();
        }

        protected override string TextForValue(int idx)
        {
            if (FormatValue != null)
                return FormatValue(values[idx]);

            return GetTextForValue(values[idx]);
        }

        public override void IncButtonPressed()
        {
            base.IncButtonPressed();
        }

        public override void DecButtonPressed()
        {
            base.DecButtonPressed();
        }

        public void ApplySettings()
        {
            SetValue(values[lastidx]);
        }

        public void CancelSettings()
        {
            GetInitValues(out var idx, out var numElems);
            lastidx = idx;
        }

        public void Init()
        {
            OnDisable();
            IsInitialized = true;
            OnEnable();
        }
    }

    public class TupleViewController<T> : ListSettingsController, CustomSetting
    {
        public Func<T> GetValue = () => default(T);
        public Action<T> SetValue = (_) => { };
        public Func<T, string> GetTextForValue = (_) => "?";

        public List<T> values;

        public bool IsInitialized { get; set; } = false;

        int lastidx;

        protected override void OnEnable()
        {
            if (IsInitialized)
                base.OnEnable();
        }

        protected override void GetInitValues(out int idx, out int numberOfElements)
        {
            numberOfElements = values.Count;
            var value = GetValue();

            numberOfElements = values.Count();
            idx = values.FindIndex(v => v.Equals(value));
        }

        protected override void ApplyValue(int idx)
        {
            lastidx = idx;
        }

        protected override string TextForValue(int idx)
        {
            return GetTextForValue(values[idx]);
        }

        public void ApplySettings()
        {
            SetValue(values[lastidx]);
        }

        public void CancelSettings()
        {
            GetInitValues(out var idx, out var numElems);
            lastidx = idx;
        }

        public void Init()
        {
            OnDisable();
            IsInitialized = true;
            OnEnable();
        }
    }

    public class SliderViewController : IncDecSettingsController, CustomSetting
    {
        public delegate float GetFloat();
        public event GetFloat GetValue;

        public delegate void SetFloat(float value);
        public event SetFloat SetValue;

        private float _min;
        private float _max;
        private bool _intValues;

        private CustomSlider _sliderInst;
        private TMPro.TextMeshProUGUI _textInst;

        public bool IsInitialized { get; set; } = false;

        private float lastVal;

        public void Init()
        {
            _sliderInst = transform.GetComponent<CustomSlider>();
            _sliderInst.CurrentValue = GetInitValue();
            lastVal = GetInitValue();
            _textInst = _sliderInst.Scrollbar.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            _sliderInst.Scrollbar.value = _sliderInst.GetPercentageFromValue(_sliderInst.CurrentValue);
            _sliderInst.Scrollbar.onValueChanged.AddListener(delegate (float value) {
                _sliderInst.SetCurrentValueFromPercentage(value);
                ApplyValue(_sliderInst.CurrentValue);
                RefreshUI();
            });
            RefreshUI();
            IsInitialized = true;
        }

        public void ApplySettings()
        {
            SetValue?.Invoke((_intValues) ? ((float)Math.Floor(lastVal)) : (lastVal));
        }

        private void RefreshUI()
        {
            _textInst.text = TextForValue(_sliderInst.CurrentValue);
        }

        public override void IncButtonPressed()
        {

        }

        public override void DecButtonPressed()
        {

        }

        public void SetValues(float min, float max, bool intValues)
        {
            _min = min;
            _max = max;
            _intValues = intValues;
        }

        protected float GetInitValue()
        {
            float value = 0;
            if (GetValue == null)
                value = _min;
            else
                value = GetValue();
            return value;
        }

        protected void ApplyValue(float value)
        {
            lastVal = value;
        }

        protected string TextForValue(float value)
        {
            if (_intValues)
                return Math.Floor(value).ToString("N0");
            return value.ToString("N1");
        }

        public void CancelSettings()
        {
            lastVal = GetInitValue();
            _sliderInst.Scrollbar.value = _sliderInst.GetPercentageFromValue(lastVal);
        }
    }

    public class ColorPickerViewController : MonoBehaviour, CustomSetting
    {
        public delegate Color GetColor();
        public event GetColor GetValue;

        public delegate void SetColor(Color value);
        public event SetColor SetValue;

        private ColorPickerPreviewClickable _ColorPickerPreviewClickableInst;

        public bool IsInitialized { get; set; } = false;

        public void Init()
        {
            _ColorPickerPreviewClickableInst.ImagePreview.color = GetInitValue();
            IsInitialized = true;
        }

        protected Color GetInitValue()
        {
            Color color = new Color(1, 1, 1, 1);
            if (GetValue != null)
                color = GetValue();
            return color;
        }

        public void ApplySettings()
        {
            SetValue?.Invoke(_ColorPickerPreviewClickableInst.ImagePreview.color);
        }

        public void CancelSettings()
        {
            _ColorPickerPreviewClickableInst.ImagePreview.color = GetInitValue();
        }

        public void SetPreviewInstance(ColorPickerPreviewClickable instance)
        {
            _ColorPickerPreviewClickableInst = instance;
        }

        public void SetValues(Color color)
        {
            _ColorPickerPreviewClickableInst.ImagePreview.color = color;
        }
    }

    public class SegmentedControlViewController : ListSettingsController, CustomSetting
    {
        public delegate int GetInt();
        public event GetInt GetValue;

        public delegate void SetInt(int value);
        public event SetInt SetValue;

        public SegmentedControl segmentedControl;

        public bool IsInitialized { get; set; } = false;


        private int lastVal;

        public void Init()
        {
            segmentedControl.SelectCellWithNumber(GetInitValue());
            lastVal = GetInitValue();

            segmentedControl.didSelectCellEvent += (sender, idx) => {
                ApplyValue(idx);
            };

            IsInitialized = true;
        }

        public void ApplySettings()
        {
            SetValue?.Invoke(lastVal);
        }

        public override void IncButtonPressed()
        {

        }

        public override void DecButtonPressed()
        {

        }

        protected int GetInitValue()
        {
            int value = 0;
            if (GetValue == null)
                value = 0;
            else
                value = GetValue();
            return value;
        }

        public void CancelSettings()
        {
            lastVal = GetInitValue();
            segmentedControl.SelectCellWithNumber(lastVal);
        }

        protected override void GetInitValues(out int idx, out int numberOfElements)
        {
            numberOfElements = segmentedControl.dataSource.NumberOfCells();
            idx = GetValue();
            if (idx == -1)
                idx = 0;
        }

        protected override void ApplyValue(int idx)
        {
            lastVal = idx;
            ApplySettings();
        }

        protected override string TextForValue(int idx)
        {
            return "";
        }
    }
}
