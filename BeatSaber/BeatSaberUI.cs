using CustomUI.UIElements;
using CustomUI.Utilities;
using HMUI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRUI;
using Image = UnityEngine.UI.Image;

namespace CustomUI.BeatSaber
{
    public class BeatSaberUI : MonoBehaviour
    {
        private static Button _backButtonInstance;
        private static GameObject _loadingIndicatorInstance;
        private static CustomMenu _keyboardMenu = null;
        private static CustomUIKeyboard _keyboard = null;
        private static Action<string> _textChangedEvent;
        private static Action<string> _textEntrySuccessEvent = null;
        private static Action _textEntryCancelledEvent = null;
        private static bool _isKeyboardOpen = false;
        private static string _initialValue;
        private static TextMeshProUGUI _inputText;
        
        /// <summary>
        /// Display a keyboard interface to accept user input.
        /// </summary>
        /// <param name="title">The title to be displayed above the keyboard.</param>
        /// <param name="initialValue">The starting value of the keyboard.</param>
        /// <param name="TextChangedEvent">Callback when the text is modified by the user (when any key is pressed basically).</param>
        /// <param name="TextEntrySuccessEvent">Callback when the user successfully submits the changed text.</param>
        /// <param name="TextEntryCancelledEvent">Callback when the user presses the cancel button.</param>
        /// <returns></returns>
        public static bool DisplayKeyboard(string title, string initialValue, Action<string> TextChangedEvent = null, Action<string> TextEntrySuccessEvent = null, Action TextEntryCancelledEvent = null)
        {
            if (_isKeyboardOpen) return false;

            if (_keyboardMenu == null)
            {
                _keyboardMenu = CreateCustomMenu<CustomMenu>(title);
                var mainViewController = CreateViewController<CustomViewController>();
                _keyboardMenu.SetMainViewController(mainViewController, false, (firstActivation, type) =>
                {
                    if (firstActivation)
                    {
                        var _customKeyboardGO = Instantiate(Resources.FindObjectsOfTypeAll<UIKeyboard>().First(x => x.name != "CustomUIKeyboard"), mainViewController.rectTransform, false).gameObject;
                        Destroy(_customKeyboardGO.GetComponent<UIKeyboard>());
                        _keyboard = _customKeyboardGO.AddComponent<CustomUIKeyboard>();

                        _inputText = CreateText(mainViewController.rectTransform, String.Empty, new Vector2(0f, 22f));
                        _inputText.alignment = TextAlignmentOptions.Center;
                        _inputText.fontSize = 6f;

                        _keyboard.okButtonWasPressedEvent += () =>
                        {
                            _textEntrySuccessEvent?.Invoke(_inputText.text);
                            _inputText.text = String.Empty;
                            _keyboard.OkButtonInteractivity = false;
                            _keyboardMenu.Dismiss();
                            _isKeyboardOpen = false;
                        };
                        _keyboard.cancelButtonWasPressedEvent += () =>
                        {
                            _textEntryCancelledEvent?.Invoke();
                            _inputText.text = String.Empty;
                            _keyboard.OkButtonInteractivity = false;
                            _keyboardMenu.Dismiss();
                            _isKeyboardOpen = false;
                        };
                        _keyboard.textKeyWasPressedEvent += (key) =>
                        {
                            _inputText.text += key;
                            _textChangedEvent?.Invoke(_inputText.text);

                            if (_inputText.text.Length > 0)
                                _keyboard.OkButtonInteractivity = true;
                        };
                        _keyboard.deleteButtonWasPressedEvent += () =>
                        {
                            if (_inputText.text.Length > 0)
                            {
                                _inputText.text = _inputText.text.Substring(0, _inputText.text.Length - 1);
                                _textChangedEvent?.Invoke(_inputText.text);
                            }
                            if (_inputText.text.Length == 0)
                                _keyboard.OkButtonInteractivity = false;
                        };
                    }
                    _inputText.text = _initialValue;
                    _keyboard.OkButtonInteractivity = _inputText.text.Length > 0;
                });
            }
            _keyboardMenu.title = title == null ? String.Empty : title;
            _initialValue = initialValue == null ? String.Empty : initialValue;
            _textChangedEvent = TextChangedEvent;
            _textEntrySuccessEvent = TextEntrySuccessEvent;
            _textEntryCancelledEvent = TextEntryCancelledEvent;

            if (!_keyboardMenu.Present(false))
                return false;

            _isKeyboardOpen = true;
            return true;
        }

        /// <summary>
        /// Creates a copy of a template button and returns it.
        /// </summary>
        /// <param name="parent">The transform to parent the button to.</param>
        /// <param name="buttonTemplate">The name of the button to make a copy of. Example: "QuitButton", "PlayButton", etc.</param>
        /// <param name="anchoredPosition">The position the button should be anchored to.</param>
        /// <param name="sizeDelta">The size of the buttons RectTransform.</param>
        /// <param name="onClick">Callback for when the button is pressed.</param>
        /// <param name="buttonText">The text that should be shown on the button.</param>
        /// <param name="icon">The icon that should be shown on the button.</param>
        /// <returns>The newly created button.</returns>
        public static Button CreateUIButton(RectTransform parent, string buttonTemplate, Vector2 anchoredPosition, Vector2 sizeDelta, UnityAction onClick = null, string buttonText = "BUTTON", Sprite icon = null)
        {
            Button btn = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == buttonTemplate)), parent, false);
            btn.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
                btn.onClick.AddListener(onClick);
            btn.name = "CustomUIButton";

            (btn.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
            (btn.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            (btn.transform as RectTransform).anchoredPosition = anchoredPosition;
            (btn.transform as RectTransform).sizeDelta = sizeDelta;

            btn.SetButtonText(buttonText);
            if (icon != null)
                btn.SetButtonIcon(icon);

            return btn;
        }


        /// <summary>
        /// Creates a copy of a template button and returns it.
        /// </summary>
        /// <param name="parent">The transform to parent the button to.</param>
        /// <param name="buttonTemplate">The name of the button to make a copy of. Example: "QuitButton", "PlayButton", etc.</param>
        /// <param name="anchoredPosition">The position the button should be anchored to.</param>
        /// <param name="onClick">Callback for when the button is pressed.</param>
        /// <param name="buttonText">The text that should be shown on the button.</param>
        /// <param name="icon">The icon that should be shown on the button.</param>
        /// <returns>The newly created button.</returns>
        public static Button CreateUIButton(RectTransform parent, string buttonTemplate, Vector2 anchoredPosition, UnityAction onClick = null, string buttonText = "BUTTON", Sprite icon = null)
        {
            Button btn = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == buttonTemplate)), parent, false);
            btn.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
                btn.onClick.AddListener(onClick);
            btn.name = "CustomUIButton";

            (btn.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
            (btn.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            (btn.transform as RectTransform).anchoredPosition = anchoredPosition;

            btn.SetButtonText(buttonText);
            if (icon != null)
                btn.SetButtonIcon(icon);

            return btn;
        }


        /// <summary>
        /// Creates a copy of a template button and returns it.
        /// </summary>
        /// <param name="parent">The transform to parent the button to.</param>
        /// <param name="buttonTemplate">The name of the button to make a copy of. Example: "QuitButton", "PlayButton", etc.</param>
        /// <param name="onClick">Callback for when the button is pressed.</param>
        /// <param name="buttonText">The text that should be shown on the button.</param>
        /// <param name="icon">The icon that should be shown on the button.</param>
        /// <returns>The newly created button.</returns>
        public static Button CreateUIButton(RectTransform parent, string buttonTemplate, UnityAction onClick = null, string buttonText = "BUTTON", Sprite icon = null)
        {
            Button btn = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == buttonTemplate)), parent, false);
            btn.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
                btn.onClick.AddListener(onClick);
            btn.name = "CustomUIButton";

            (btn.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
            (btn.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            btn.SetButtonText(buttonText);
            if (icon != null)
                btn.SetButtonIcon(icon);
            return btn;
        }

        /// <summary>
        /// Creates a copy of a back button.
        /// </summary>
        /// <param name="parent">The transform to parent the new button to.</param>
        /// <param name="onClick">Callback for when the button is pressed.</param>
        /// <returns>The newly created back button.</returns>
        public static Button CreateBackButton(RectTransform parent, UnityAction onClick = null)
        {
            if (_backButtonInstance == null)
            {
                try
                {
                    _backButtonInstance = Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "BackArrowButton"));
                }
                catch
                {
                    return null;
                }
            }
            
            Button btn = Instantiate(_backButtonInstance, parent, false);
            btn.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
                btn.onClick.AddListener(onClick);
            btn.name = "CustomUIButton";
            
            return btn;
        }

        /// <summary>
        /// Creates a VRUIViewController of type T, and marks it to not be destroyed.
        /// </summary>
        /// <typeparam name="T">The variation of VRUIViewController you want to create.</typeparam>
        /// <returns>The newly created VRUIViewController of type T.</returns>
        public static T CreateViewController<T>() where T : VRUIViewController
        {
            T vc = new GameObject("CustomViewController").AddComponent<T>();
            DontDestroyOnLoad(vc.gameObject);

            vc.rectTransform.anchorMin = new Vector2(0f, 0f);
            vc.rectTransform.anchorMax = new Vector2(1f, 1f);
            vc.rectTransform.sizeDelta = new Vector2(0f, 0f);
            vc.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            
            return vc;
        }

        /// <summary>
        /// Creates a CustomMenu, which is basically a custom panel that handles UI transitions for you automatically.
        /// </summary>
        /// <typeparam name="T">The type of CustomMenu to instantiate.</typeparam>
        /// <param name="title">The title of the new CustomMenu.</param>
        /// <returns>The newly created CustomMenu of type T.</returns>
        public static T CreateCustomMenu<T>(string title) where T: CustomMenu
        {
            T customMenu = new GameObject("CustomUIMenu").AddComponent<T>();
            customMenu.title = title;
            return customMenu;
        }

        /// <summary>
        /// Creates a loading spinner.
        /// </summary>
        /// <param name="parent">The transform to parent the new loading spinner to.</param>
        /// <returns>The newly created loading spinner.</returns>
        public static GameObject CreateLoadingSpinner(Transform parent)
        {
            if (_loadingIndicatorInstance == null)
            {
                try
                {
                    _loadingIndicatorInstance = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name == "LoadingIndicator").First();
                }
                catch
                {
                    return null;
                }
            }

            GameObject loadingSpinner = Instantiate(_loadingIndicatorInstance, parent, false);
            loadingSpinner.name = "CustomUILoadingSpinner";

            return loadingSpinner;
        }

        /// <summary>
        /// Creates a TextMeshProUGUI component.
        /// </summary>
        /// <param name="parent">Thet ransform to parent the new TextMeshProUGUI component to.</param>
        /// <param name="text">The text to be displayed.</param>
        /// <param name="anchoredPosition">The position the button should be anchored to.</param>
        /// <returns>The newly created TextMeshProUGUI component.</returns>
        public static TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 anchoredPosition)
        {
            return CreateText(parent, text, anchoredPosition, new Vector2(60f, 10f));
        }

        /// <summary>
        /// Creates a TextMeshProUGUI component.
        /// </summary>
        /// <param name="parent">Thet transform to parent the new TextMeshProUGUI component to.</param>
        /// <param name="text">The text to be displayed.</param>
        /// <param name="anchoredPosition">The position the text component should be anchored to.</param>
        /// <param name="sizeDelta">The size of the text components RectTransform.</param>
        /// <returns>The newly created TextMeshProUGUI component.</returns>
        public static TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject gameObj = new GameObject("CustomUIText");
            gameObj.SetActive(false);

            TextMeshProUGUI textMesh = gameObj.AddComponent<TextMeshProUGUI>();
            textMesh.font = Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(t => t.name == "Teko-Medium SDF No Glow"));
            textMesh.rectTransform.SetParent(parent, false);
            textMesh.text = text;
            textMesh.fontSize = 4;
            textMesh.color = Color.white;

            textMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.sizeDelta = sizeDelta;
            textMesh.rectTransform.anchoredPosition = anchoredPosition;

            gameObj.SetActive(true);
            return textMesh;
        }

        /// <summary>
        /// Adds hint text to any component that handles pointer events.
        /// </summary>
        /// <param name="parent">Thet transform to parent the new HoverHint component to.</param>
        /// <param name="text">The text to be displayed on the HoverHint panel.</param>
        /// <returns>The newly created HoverHint component.</returns>
        public static HoverHint AddHintText(RectTransform parent, string text)
        {
            var hoverHint = parent.gameObject.AddComponent<HoverHint>();
            hoverHint.text = text;
            //hoverHint.name = "CustomHintText";
            HoverHintController hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            hoverHint.SetPrivateField("_hoverHintController", hoverHintController);
            return hoverHint;
        }

        /// <summary>
        /// Creates a custom slider.
        /// </summary>
        /// <param name="parent">Thet transform to parent the new slider component to.</param>
        /// <param name="min">The minimum value of the slider.</param>
        /// <param name="max">The maximum value of the slider.</param>
        /// <param name="increment">The amount to increment the slider by.</param>
        /// <param name="intValues">True if the value represented by the slider is an int, false if it's a float.</param>
        /// <param name="onValueChanged">Callback when the sliders value changes.</param>
        /// <returns></returns>
        public static CustomSlider CreateUISlider(Transform parent, float min, float max, float increment, bool intValues, UnityAction<float> onValueChanged = null)
        {
            CustomSlider slider = new GameObject("CustomUISlider").AddComponent<CustomSlider>();
            GameObject.DontDestroyOnLoad(slider.gameObject);
            slider.Scrollbar = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<HMUI.Scrollbar>().First(s => s.name != "CustomUISlider"), parent, false);
            slider.Scrollbar.name = "CustomUISlider";
            slider.Scrollbar.transform.SetParent(parent, false);

            slider.Scrollbar.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = false;
            slider.Scrollbar.numberOfSteps = (int)((max - min) / increment) + 1;
            slider.MinValue = min;
            slider.MaxValue = max;
            slider.IsIntValue = intValues;
            slider.SetCurrentValueFromPercentage(slider.Scrollbar.value);
            slider.Scrollbar.GetComponentInChildren<TextMeshProUGUI>().text = slider.CurrentValue.ToString("N1");
            slider.Scrollbar.onValueChanged.RemoveAllListeners();
            slider.Scrollbar.onValueChanged.AddListener(delegate (float value) {
                TextMeshProUGUI valueLabel = slider.Scrollbar.GetComponentInChildren<TextMeshProUGUI>();
                valueLabel.enableWordWrapping = false;
                slider.SetCurrentValueFromPercentage(value);
                valueLabel.text = slider.CurrentValue.ToString("N1");
            });
            if (onValueChanged != null)
                slider.Scrollbar.onValueChanged.AddListener((percent) => 
                {
                    onValueChanged?.Invoke(slider.GetValueFromPercentage(percent));
                });
            return slider;
        }

        /// <summary>
        /// Creates a color picker.
        /// </summary>
        /// <param name="parent">Thet transform to parent the new color picker to.</param>
        /// <param name="anchoredPosition">The position the color picker should be anchored to.</param>
        /// <param name="sizeDelta">The size of the color picker's RectTransform.</param>
        /// <returns></returns>
        public static ColorPicker CreateColorPicker(RectTransform parent, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            ColorPicker colorPicker = new GameObject("ColorPicker").AddComponent<ColorPicker>();

            colorPicker.transform.localScale = new Vector3(sizeDelta.x, sizeDelta.y, colorPicker.transform.localScale.z);
            colorPicker.transform.SetParent(parent, false);
            colorPicker.transform.localPosition = new Vector3(anchoredPosition.x, anchoredPosition.y);

            return colorPicker;
        }

        /// <summary>
        /// Creates a custom TextSegmentedControl component.
        /// </summary>
        /// <param name="parent">Thet transform to parent the new TextSegmentedControl component to.</param>
        /// <param name="anchoredPosition">The position the TextSegmentedControl component should be anchored to.</param>
        /// <param name="sizeDelta">The size of the TextSegmentedControl component RectTransform.</param>
        /// <param name="onValueChanged">Callback when the user clicks on one of the segments.</param>
        /// <param name="fontSize">Size of text in segments.</param>
        /// <param name="padding">Size of padding in segments.</param>
        /// <returns>The newly created TextSegmentedControl component.</returns>
        public static TextSegmentedControl CreateTextSegmentedControl(RectTransform parent, Vector2 anchoredPosition, Vector2 sizeDelta, UnityAction<int> onValueChanged = null, float fontSize = 4f, float padding = 8f)
        {
            var segmentedControl = new GameObject("CustomTextSegmentedControl", typeof(RectTransform)).AddComponent<TextSegmentedControl>();
            segmentedControl.gameObject.AddComponent<HorizontalLayoutGroup>();

            TextSegmentedControlCellNew[] _segments = Resources.FindObjectsOfTypeAll<TextSegmentedControlCellNew>();

            segmentedControl.SetPrivateField("_singleCellPrefab", _segments.First(x => x.name == "HSingleTextSegmentedControlCell"));
            segmentedControl.SetPrivateField("_firstCellPrefab", _segments.First(x => x.name == "LeftTextSegmentedControlCell"));
            segmentedControl.SetPrivateField("_middleCellPrefab", _segments.First(x => x.name == "HMiddleTextSegmentedControlCell"));
            segmentedControl.SetPrivateField("_lastCellPrefab", _segments.First(x => x.name == "RightTextSegmentedControlCell"));

            segmentedControl.SetPrivateField("_container", Resources.FindObjectsOfTypeAll<TextSegmentedControl>().Select(x => x.GetPrivateField<object>("_container")).First(x => x != null));

            segmentedControl.transform.SetParent(parent, false);
            (segmentedControl.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            (segmentedControl.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
            (segmentedControl.transform as RectTransform).anchoredPosition = anchoredPosition;
            (segmentedControl.transform as RectTransform).sizeDelta = sizeDelta;

            segmentedControl.SetPrivateField("_fontSize", 4f);
            segmentedControl.SetPrivateField("_padding", 8f);

            if (onValueChanged != null)
                segmentedControl.didSelectCellEvent += (sender, index) => { onValueChanged(index); };

            return segmentedControl;
        }

        /// <summary>
        /// Creates a custom IconSegmentedControl component.
        /// </summary>
        /// <param name="parent">Thet transform to parent the new IconSegmentedControl component to.</param>
        /// <param name="anchoredPosition">The position the IconSegmentedControl component should be anchored to.</param>
        /// <param name="sizeDelta">The size of the IconSegmentedControl component RectTransform.</param>
        /// <param name="onValueChanged">Callback when the user clicks on one of the segments.</param>
        /// <returns>The newly created IconSegmentedControl component.</returns>
        public static IconSegmentedControl CreateIconSegmentedControl(RectTransform parent, Vector2 anchoredPosition, Vector2 sizeDelta, UnityAction<int> onValueChanged = null)
        {
            var segmentedControl = new GameObject("CustomIconSegmentedControl", typeof(RectTransform)).AddComponent<IconSegmentedControl>();
            segmentedControl.gameObject.AddComponent<HorizontalLayoutGroup>();

            IconSegmentedControlCell[] _segments = Resources.FindObjectsOfTypeAll<IconSegmentedControlCell>();

            segmentedControl.SetPrivateField("_singleCellPrefab", _segments.First(x => x.name == "SingleIconSegmentedControlCell"));
            segmentedControl.SetPrivateField("_firstCellPrefab", _segments.First(x => x.name == "LeftIconSegmentedControlCell"));
            segmentedControl.SetPrivateField("_middleCellPrefab", _segments.First(x => x.name == "HMiddleIconSegmentedControlCell"));
            segmentedControl.SetPrivateField("_lastCellPrefab", _segments.First(x => x.name == "RightIconSegmentedControlCell"));

            segmentedControl.SetPrivateField("_container", Resources.FindObjectsOfTypeAll<IconSegmentedControl>().Select(x => x.GetPrivateField<object>("_container")).First(x => x != null));

            segmentedControl.transform.SetParent(parent, false);
            (segmentedControl.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            (segmentedControl.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
            (segmentedControl.transform as RectTransform).anchoredPosition = anchoredPosition;
            (segmentedControl.transform as RectTransform).sizeDelta = sizeDelta;

            if (onValueChanged != null)
                segmentedControl.didSelectCellEvent += (sender, index) => { onValueChanged(index); };

            return segmentedControl;
        }
    }
}
