using CustomUI.BeatSaber;
using CustomUI.UIElements;
using CustomUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SettingsNavigationController;
using Image = UnityEngine.UI.Image;

namespace CustomUI.Settings
{
    public class SubMenu
    {
        public static List<CustomSetting> needsInit = new List<CustomSetting>();
        public static SettingsNavigationController navInstance;
        public Transform transform;
        public CustomSettingsListViewController viewController;
        private Dictionary<string, SubMenu> _subSubMenus = new Dictionary<string, SubMenu>();

        public SubMenu(CustomSettingsListViewController viewController)
        {
            this.viewController = viewController;
            this.transform = viewController.transform;
        }

        public SubMenu(Transform transform)
        {
            this.transform = transform;
        }

        public BoolViewController AddBool(string name)
        {
            return AddBool(name, "");
        }
        public BoolViewController AddBool(string name, string hintText)
        {
            return AddToggleSetting<BoolViewController>(name, hintText);
        }

        public IntViewController AddInt(string name, int min, int max, int increment)
        {
            return AddInt(name, "", min, max, increment);
        }
        public IntViewController AddInt(string name, string hintText, int min, int max, int increment)
        {
            var view = AddIntSetting<IntViewController>(name, hintText);
            view.SetValues(min, max, increment);
            return view;
        }

        public ListViewController AddList(string name, float[] values)
        {
            return AddList(name, values, "");
        }
        public ListViewController AddList(string name, float[] values, string hintText)
        {
            var view = AddListSetting<ListViewController>(name, hintText);
            view.values = values.ToList();
            return view;
        }

        public StringViewController AddString(string name, string hintText = null)
        {
            var view = AddStringSetting<StringViewController>(name, hintText);
            return view;
        }

        public SliderViewController AddSlider(string name, string hintText, float min, float max, float increment, bool intValues)
        {
            var view = AddSliderSetting<SliderViewController>(name, hintText, min, max, increment, intValues);
            view.SetValues(min, max, intValues);
            return view;
        }
        public ColorPickerViewController AddColorPicker(string name, string hintText, Color color)
        {
            var view = AddColorPickerSetting<ColorPickerViewController>(name, hintText, color, out var clickablePreview);
            view.SetPreviewInstance(clickablePreview);
            view.SetValues(color);
            return view;
        }
        
        public T AddListSetting<T>(string name) where T : ListSettingsController
        {
            return AddListSetting<T>(name, "");
        }
        public T AddListSetting<T>(string name, string hintText) where T : ListSettingsController
        {
            var volumeSettings = GetVolumeSettings();
            GameObject newSettingsObject = MonoBehaviour.Instantiate(volumeSettings.gameObject, transform);
            newSettingsObject.name = name;

            var incBg = newSettingsObject.transform.Find("Value").Find("IncButton").Find("BG").gameObject.GetComponent<Image>();
            (incBg.transform as RectTransform).localScale *= new Vector2(0.8f, 0.8f);
            var decBg = newSettingsObject.transform.Find("Value").Find("DecButton").Find("BG").gameObject.GetComponent<Image>();
            (decBg.transform as RectTransform).localScale *= new Vector2(0.8f, 0.8f);

            ListSettingsController volume = newSettingsObject.GetComponent<ListSettingsController>();
            T newListSettingsController = (T)ReflectionUtil.CopyComponent(volume, typeof(ListSettingsController), typeof(T), newSettingsObject);
            MonoBehaviour.DestroyImmediate(volume);

            var tmpText = newSettingsObject.GetComponentInChildren<TMP_Text>();
            tmpText.text = name;
            if (hintText != String.Empty)
                BeatSaberUI.AddHintText(tmpText.rectTransform, hintText);

            viewController?.AddSubmenuOption(newSettingsObject);
            AddHooks(newListSettingsController);
            return newListSettingsController;
        }

        public T AddStringSetting<T>(string name) where T : ListSettingsController
        {
            return AddStringSetting<T>(name, "");
        }
        public T AddStringSetting<T>(string name, string hintText) where T : ListSettingsController
        {
            var volumeSettings = GetVolumeSettings();
            GameObject newSettingsObject = MonoBehaviour.Instantiate(volumeSettings.gameObject, transform);
            newSettingsObject.name = name;
            newSettingsObject.transform.Find("Value").gameObject.GetComponent<HorizontalLayoutGroup>().spacing += 2;
            newSettingsObject.transform.Find("Value").Find("DecButton").gameObject.SetActive(false);
            var bgIcon = newSettingsObject.transform.Find("Value").Find("IncButton").Find("BG").gameObject.GetComponent<Image>();
            (bgIcon.transform as RectTransform).localScale *= new Vector2(0.8f, 0.8f);
            var arrowIcon = newSettingsObject.transform.Find("Value").Find("IncButton").Find("Arrow").gameObject.GetComponent<Image>();
            arrowIcon.sprite = UIUtilities.EditIcon;
            var valueText = newSettingsObject.transform.Find("Value").Find("ValueText").gameObject.GetComponent<TextMeshProUGUI>();
            valueText.alignment = TextAlignmentOptions.MidlineRight;
            valueText.enableWordWrapping = false;
            BeatSaberUI.AddHintText(valueText.rectTransform, hintText);

            ListSettingsController volume = newSettingsObject.GetComponent<ListSettingsController>();
            T newListSettingsController = (T)ReflectionUtil.CopyComponent(volume, typeof(ListSettingsController), typeof(T), newSettingsObject);
            MonoBehaviour.DestroyImmediate(volume);

            var tmpText = newSettingsObject.GetComponentInChildren<TMP_Text>();
            tmpText.text = name;
            if (hintText != String.Empty)
                BeatSaberUI.AddHintText(tmpText.rectTransform, hintText);

            viewController?.AddSubmenuOption(newSettingsObject);
            AddHooks(newListSettingsController);
            return newListSettingsController;
        }

        public T AddToggleSetting<T>(string name) where T : SwitchSettingsController
        {
            return AddToggleSetting<T>(name, "");
        }
        public T AddToggleSetting<T>(string name, string hintText) where T : SwitchSettingsController
        {
            var volumeSettings = GetWindowSettings();
            GameObject newSettingsObject = MonoBehaviour.Instantiate(volumeSettings.gameObject, transform);
            newSettingsObject.name = name;

            var incBg = newSettingsObject.transform.Find("Value").Find("IncButton").Find("BG").gameObject.GetComponent<Image>();
            (incBg.transform as RectTransform).localScale *= new Vector2(0.8f, 0.8f);
            var decBg = newSettingsObject.transform.Find("Value").Find("DecButton").Find("BG").gameObject.GetComponent<Image>();
            (decBg.transform as RectTransform).localScale *= new Vector2(0.8f, 0.8f);

            SwitchSettingsController volume = newSettingsObject.GetComponent<SwitchSettingsController>();
            T newToggleSettingsController = (T)ReflectionUtil.CopyComponent(volume, typeof(SwitchSettingsController), typeof(T), newSettingsObject);
            MonoBehaviour.DestroyImmediate(volume);

            var tmpText = newSettingsObject.GetComponentInChildren<TMP_Text>();
            tmpText.text = name;
            if (hintText != String.Empty)
                BeatSaberUI.AddHintText(tmpText.rectTransform, hintText);

            viewController?.AddSubmenuOption(newSettingsObject);
            AddHooks(newToggleSettingsController);
            return newToggleSettingsController;
        }

        public T AddIntSetting<T>(string name) where T : IntSettingsController
        {
            return AddIntSetting<T>(name, "");
        }
        public T AddIntSetting<T>(string name, string hintText) where T : IntSettingsController
        {
            var volumeSettings = GetWindowSettings();
            GameObject newSettingsObject = MonoBehaviour.Instantiate(volumeSettings.gameObject, transform);
            newSettingsObject.name = name;

            var incBg = newSettingsObject.transform.Find("Value").Find("IncButton").Find("BG").gameObject.GetComponent<Image>();
            (incBg.transform as RectTransform).localScale *= new Vector2(0.8f, 0.8f);
            var decBg = newSettingsObject.transform.Find("Value").Find("DecButton").Find("BG").gameObject.GetComponent<Image>();
            (decBg.transform as RectTransform).localScale *= new Vector2(0.8f, 0.8f);

            SwitchSettingsController volume = newSettingsObject.GetComponent<SwitchSettingsController>();
            T newToggleSettingsController = (T)ReflectionUtil.CopyComponent(volume, typeof(IncDecSettingsController), typeof(T), newSettingsObject);
            MonoBehaviour.DestroyImmediate(volume);

            var tmpText = newSettingsObject.GetComponentInChildren<TMP_Text>();
            tmpText.text = name;
            if (hintText != String.Empty)
                BeatSaberUI.AddHintText(tmpText.rectTransform, hintText);

            viewController?.AddSubmenuOption(newSettingsObject);
            AddHooks(newToggleSettingsController);
            return newToggleSettingsController;
        }

        public T AddSliderSetting<T>(string name, string hintText, float min, float max, float increment, bool intValues) where T : IncDecSettingsController
        {
            var volumeSettings = GetWindowSettings();
            GameObject newSettingsObject = MonoBehaviour.Instantiate(volumeSettings.gameObject, transform);
            newSettingsObject.name = name;

            SwitchSettingsController volume = newSettingsObject.GetComponent<SwitchSettingsController>();
            T newSliderSettingsController = (T)ReflectionUtil.CopyComponent(volume, typeof(IncDecSettingsController), typeof(T), newSettingsObject);
            MonoBehaviour.DestroyImmediate(volume);

            GameObject.Destroy(newSettingsObject.transform.Find("Value").Find("DecButton").gameObject);
            GameObject.Destroy(newSettingsObject.transform.Find("Value").Find("ValueText").gameObject);
            GameObject.Destroy(newSettingsObject.transform.Find("Value").Find("IncButton").gameObject);

            CustomSlider slider = newSliderSettingsController.gameObject.AddComponent<CustomSlider>();
            slider.Scrollbar = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<HMUI.Scrollbar>().First(s => s.name != "CustomUISlider"), newSettingsObject.transform.Find("Value"), false);
            slider.Scrollbar.name = "CustomUISlider";
            slider.Scrollbar.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = false;
            (slider.Scrollbar.transform as RectTransform).sizeDelta = new Vector2(39.5f, 4.5f);
            (slider.Scrollbar.transform as RectTransform).anchorMin = new Vector2(0, 0.5f);


            slider.Scrollbar.numberOfSteps = (int)((max - min) / increment) + 1;
            slider.MinValue = min;
            slider.MaxValue = max;
            slider.IsIntValue = intValues;

            var tmpText = newSettingsObject.GetComponentInChildren<TMP_Text>();
            tmpText.text = name;
            if (hintText != String.Empty)
                BeatSaberUI.AddHintText(tmpText.rectTransform, hintText);

            viewController?.AddSubmenuOption(newSettingsObject);
            AddHooks(newSliderSettingsController);
            return newSliderSettingsController;
        }
        public T AddColorPickerSetting<T>(string name, Color color, out ColorPickerPreviewClickable clickablePreview) where T : MonoBehaviour
        {
            return AddColorPickerSetting<T>(name, "", color, out clickablePreview);
        }
        public T AddColorPickerSetting<T>(string name, string hintText, Color color, out ColorPickerPreviewClickable clickablePreview) where T : MonoBehaviour
        {
            var volumeSettings = Resources.FindObjectsOfTypeAll<SwitchSettingsController>().FirstOrDefault();
            GameObject newSettingsObject = MonoBehaviour.Instantiate(volumeSettings.gameObject, transform);
            newSettingsObject.name = name;

            SwitchSettingsController volume = newSettingsObject.GetComponent<SwitchSettingsController>();
            T newColorPickerSettingsController = (T)ReflectionUtil.CopyComponent(volume, typeof(MonoBehaviour), typeof(T), newSettingsObject);
            MonoBehaviour.DestroyImmediate(volume);

            GameObject.Destroy(newSettingsObject.GetComponentInChildren<HorizontalLayoutGroup>());
            GameObject.Destroy(newSettingsObject.transform.Find("Value").Find("DecButton").gameObject);
            GameObject.Destroy(newSettingsObject.transform.Find("Value").Find("ValueText").gameObject);
            GameObject.Destroy(newSettingsObject.transform.Find("Value").Find("IncButton").gameObject);

            ColorPickerPreviewClickable cppc = new GameObject("ColorPickerPreviewClickable").AddComponent<ColorPickerPreviewClickable>();
            cppc.transform.SetParent(newSettingsObject.transform.Find("Value"), false);
            cppc.ImagePreview.color = color;
            (cppc.transform as RectTransform).localScale = new Vector2(0.06f, 0.06f);
            (cppc.transform as RectTransform).localPosition += new Vector3(3f, 0.1f);
            clickablePreview = cppc;

            var tmpText = newSettingsObject.GetComponentInChildren<TMP_Text>();
            tmpText.text = name;
            if (hintText != String.Empty)
                BeatSaberUI.AddHintText(tmpText.rectTransform, hintText);

            viewController?.AddSubmenuOption(newSettingsObject);
            AddHooks(newColorPickerSettingsController);
            return newColorPickerSettingsController;
        }

        public SubMenu AddSubMenu(string text, string hintText, SubMenu subMenu = null)
        {
            return AddSubMenu_Internal(text, hintText, false, subMenu);
        }

        public SubMenu AddSubMenu(string text, string hintText, bool addBackButton)
        {
            return AddSubMenu_Internal(text, hintText, addBackButton);
        }

        private SubMenu AddSubMenu_Internal(string text, string hintText, bool addBackButton, SubMenu subMenu = null) 
        {
            if (viewController == null)
                throw new Exception("You cannot add a submenu using this method outside of the settings menu.");
            
            if (subMenu == null)
            {
                _subSubMenus.TryGetValue(text, out subMenu);
                if (subMenu == null)
                {
                    subMenu = SettingsUI.CreateSubMenu(text, false);
                    _subSubMenus[text] = subMenu;
                    if(addBackButton)
                        subMenu.AddSubMenu("Back", "Go back to the previous menu", this);
                }
            }

            GameObject gameObj = new GameObject("CustomUIText");
            gameObj.SetActive(false);

            ClickableText clickableText = gameObj.AddComponent<ClickableText>();
            clickableText.font = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(t => t.name == "Teko-Medium SDF No Glow"));
            clickableText.rectTransform.SetParent(transform, false);
            clickableText.text = text;
            clickableText.fontSize = 5;
            clickableText.color = Color.white;

            clickableText.rectTransform.anchorMin = new Vector2(0f, -3);
            clickableText.rectTransform.anchorMax = new Vector2(0f, -3);
            //clickableText.rectTransform.anchoredPosition = new Vector2(0f, -1f);

            clickableText.OnClickEvent += (eventData) => {
                var settingsFlowCoordinator = Resources.FindObjectsOfTypeAll<SettingsFlowCoordinator>().FirstOrDefault();
                if (settingsFlowCoordinator) {
                    var navigationController = settingsFlowCoordinator.GetPrivateField<SettingsNavigationController>("_settingsNavigationController");
                    settingsFlowCoordinator.InvokePrivateMethod("PopViewControllerFromNavigationController", new object[] { navigationController, null, true });
                    settingsFlowCoordinator.InvokePrivateMethod("PushViewControllerToNavigationController", new object[] { navigationController, subMenu.viewController, null, true });
                }
            };

            gameObj.SetActive(true);
            
            if (hintText != String.Empty)
                BeatSaberUI.AddHintText(clickableText.rectTransform, hintText);

            viewController?.AddSubmenuOption(clickableText.gameObject);
            return subMenu;
        }

        private void AddHooks(object obj)
        {
            if (navInstance == null)
            {
                navInstance = Resources.FindObjectsOfTypeAll<SettingsNavigationController>().First();
            }
            Action<FinishAction> del = null;
            needsInit.Add(obj as CustomSetting);
            del = delegate (FinishAction finishAction)
            {
                try
                {
                    if (obj is CustomSetting)
                    {
                        CustomSetting customSetting = (obj as CustomSetting);

                        // Only call our apply/cancel callbacks if the setting was properly initialized
                        if (customSetting.IsInitialized)
                        {
                            switch (finishAction)
                            {
                                case FinishAction.Apply:
                                case FinishAction.Ok:
                                    customSetting.ApplySettings();
                                    break;
                                case FinishAction.Cancel:
                                    customSetting.CancelSettings();
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log(ex.ToString(), IPA.Logging.Logger.Level.Error);
                }
                navInstance.didFinishEvent -= del;
            };
            navInstance.didFinishEvent += del;
        }
        private ListSettingsController GetVolumeSettings()
        {
            return Resources.FindObjectsOfTypeAll<FormattedFloatListSettingsController>().FirstOrDefault();
        }
        private SwitchSettingsController GetWindowSettings()
        {
            return Resources.FindObjectsOfTypeAll<BoolSettingsController>().FirstOrDefault();
        }
    }
}
