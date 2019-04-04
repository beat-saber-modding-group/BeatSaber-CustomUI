using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using CustomUI.Utilities;
using CustomUI.Settings;
using CustomUI.BeatSaber;
using UnityEngine.UI;

namespace CustomUI.GameplaySettings
{
    public enum GameplaySettingsPanels
    {
        ModifiersRight,
        ModifiersLeft,
        PlayerSettingsRight,
        PlayerSettingsLeft
    };

    public abstract class GameOption
    {
        public GameObject gameObject;
        public string optionName;
        public Sprite optionIcon;
        public string hintText;
        public bool initialized;
        public GameObject separator;

        protected string panelName;
        protected string pageName;
        protected List<string> conflicts = new List<string>();

        protected RectTransform Container
        {
            get
            {
                SoloFreePlayFlowCoordinator sfpfc = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
                GameplaySetupViewController gsvc = sfpfc.GetField<GameplaySetupViewController>("_gameplaySetupViewController");
                return (RectTransform)gsvc.transform.Find(pageName).Find(panelName);
            }
        }

        public abstract void Instantiate();
        public void AddConflict(string modifierName)
        {
            if (modifierName == optionName) return;
            conflicts.Add(modifierName);
        }

        protected void AddSeparator(RectTransform container)
        {
            if (separator != null) return;
            foreach (Transform t in container)
            {
                if (!t.name.StartsWith("Separator")) continue;

                separator = UnityEngine.Object.Instantiate(t.gameObject, container);
                separator.name = "ExtraSeparator";
                separator.SetActive(false);
                break;
            }
        }

        public static void GetNumDefaultOptionsForPanel(GameplaySettingsPanels panel, ref int numDefaultOptions)
        {
            switch (panel)
            {
                case GameplaySettingsPanels.ModifiersRight:
                case GameplaySettingsPanels.ModifiersLeft:
                    numDefaultOptions = 5;
                    break;
                case GameplaySettingsPanels.PlayerSettingsRight:
                case GameplaySettingsPanels.PlayerSettingsLeft:
                    numDefaultOptions = 4;
                    break;
            }
        }

        public static void GetPanelNames(GameplaySettingsPanels panel, ref string pageName, ref string panelName)
        {
            switch (panel)
            {
                case GameplaySettingsPanels.ModifiersRight:
                    pageName = "GameplayModifiers";
                    panelName = "RightColumn";
                    break;
                case GameplaySettingsPanels.ModifiersLeft:
                    pageName = "GameplayModifiers";
                    panelName = "LeftColumn";
                    break;
                case GameplaySettingsPanels.PlayerSettingsRight:
                    pageName = "PlayerSettings";
                    panelName = "RightPanel";
                    break;
                case GameplaySettingsPanels.PlayerSettingsLeft:
                    pageName = "PlayerSettings";
                    panelName = "LeftPanel";
                    break;
            }
        }

        public static void GetOptionTransforms(GameplaySettingsPanels panel, RectTransform container, ref Transform option1, ref Transform option2, ref Transform option3, ref Transform option4, ref Transform option5)
        {
            switch (panel)
            {
                case GameplaySettingsPanels.ModifiersRight:
                    option1 = container.Find("NoFail");
                    option2 = container.Find("NoObstacles");
                    option3 = container.Find("NoBombs");
                    option4 = container.Find("SlowerSong");
                    option5 = container.Find("NoArrows");
                    break;
                case GameplaySettingsPanels.ModifiersLeft:
                    option1 = container.Find("InstaFail");
                    option2 = container.Find("BatteryEnergy");
                    option3 = container.Find("DisappearingArrows");
                    option4 = container.Find("GhostNotes");
                    option5 = container.Find("FasterSong");
                    break;
                case GameplaySettingsPanels.PlayerSettingsRight:
                    option1 = container.Find("NoTextsAndHUDs");
                    option2 = container.Find("AdvancedHUD");
                    option3 = container.Find("SoundFX");
                    option4 = container.Find("ReduceDebris");
                    option5 = null;
                    break;
                case GameplaySettingsPanels.PlayerSettingsLeft:
                    option1 = container.Find("LeftHanded");
                    option2 = container.Find("SwapColors");
                    option3 = container.Find("StaticLights");
                    option4 = container.Find("PlayerHeight");
                    option5 = null;
                    break;
            }
        }
    }

    public class SubmenuOption : ToggleOption
    {
        public SubmenuOption(GameplaySettingsPanels panel, string optionName, string hintText, Sprite optionIcon) : base(panel, optionName, hintText, optionIcon, 0f)
        {
            this.optionName = optionName;
            this.hintText = hintText;
            this.optionIcon = optionIcon;
            GetPanelNames(panel, ref pageName, ref panelName);
        }

        public override void Instantiate()
        {
            base.Instantiate();

            var currentToggle = gameObject.GetComponent<GameplayModifierToggle>();
            if (currentToggle != null)
            {
                GameObject.Destroy(currentToggle.transform.Find("BG").gameObject);
            }
        }
    }

    public class ToggleOption : GameOption
    {
        public event Action<bool> OnToggle;
        public bool GetValue = false;
        public float multiplier;
        public GameplayModifierToggle currentToggle;

        private readonly string ConflictText = "\r\n\r\n<size=60%><color=#ff0000ff><b>Conflicts </b></color>";

        public class ToggleNameOverride : MonoBehaviour
        {
            private GameplayModifierToggle _toggle;
            private TextMeshProUGUI _tmpText;
            void Awake()
            {
                _toggle = GetComponent<GameplayModifierToggle>();
                _tmpText = _toggle.GetPrivateField<TextMeshProUGUI>("_nameText");
            }

            void Update()
            {
                if (_tmpText.text != "!NOTSET!")
                {
                    _tmpText.text = _toggle.gameplayModifier.modifierName;
                    Destroy(this);
                }
            }
        }
        
        public void SetToggleState(bool state)
        {
            if(currentToggle)
                currentToggle.GetComponent<Toggle>().isOn = state;
        }

        public ToggleOption(GameplaySettingsPanels panel, string optionName, string hintText, Sprite optionIcon, float multiplier)
        {
            this.optionName = optionName;
            this.hintText = hintText;
            this.optionIcon = optionIcon;
            this.multiplier = multiplier;
            GetPanelNames(panel, ref pageName, ref panelName);
        }

        public override void Instantiate()
        {
            gameObject = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<GameplayModifierToggle>().Where(g => g.transform.Find("BG"))?.Last().gameObject, Container);
            gameObject.name = optionName;
            gameObject.layer = Container.gameObject.layer;
            gameObject.transform.SetParent(Container);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.SetActive(false);

            // Add a separator for this menu option
            AddSeparator(Container);

            // Remove the multiplier text if the toggle has no multiplier
            if (multiplier == 0)
                gameObject.AddComponent<ToggleNameOverride>();

            currentToggle = gameObject.GetComponent<GameplayModifierToggle>();
            if (currentToggle != null)
            {
                currentToggle.toggle.isOn = GetValue;
                currentToggle.toggle.onValueChanged.RemoveAllListeners();
                currentToggle.toggle.onValueChanged.AddListener((bool e) => OnToggle?.Invoke(e));
                currentToggle.name = optionName;

                GameplayModifierParamsSO _gameplayModifier = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
                _gameplayModifier.SetPrivateField("_modifierName", optionName);
                _gameplayModifier.SetPrivateField("_hintText", hintText);
                _gameplayModifier.SetPrivateField("_multiplier", multiplier);
                _gameplayModifier.SetPrivateField("_icon", optionIcon == null ? UIUtilities.BlankSprite : optionIcon);
                currentToggle.SetPrivateField("_gameplayModifier", _gameplayModifier);
                
                if (hintText != String.Empty)
                {
                    HoverHint hoverHint = currentToggle.GetPrivateField<HoverHint>("_hoverHint");
                    hoverHint.text = hintText;
                    hoverHint.name = optionName;
                    HoverHintController hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
                    hoverHint.SetPrivateField("_hoverHintController", hoverHintController);
                }
            }
            initialized = true;
        }

        private void InitializeConflict(GameplayModifierToggle toMod, GameplayModifierToggle conflict, bool recursive = true)
        {
            string conflictingDisplayName = $"\uE069{conflict.gameplayModifier.modifierName}\uE069";
            if (!toMod.gameplayModifier.hintText.Contains(ConflictText))
                toMod.gameplayModifier.SetPrivateField("_hintText", toMod.gameplayModifier.hintText + ConflictText);

            if (!toMod.gameplayModifier.hintText.Contains(conflictingDisplayName))
            {
                toMod.gameplayModifier.SetPrivateField("_hintText", toMod.gameplayModifier.hintText + conflictingDisplayName);
                toMod.toggle.onValueChanged.AddListener((e) => { if (e) conflict.toggle.isOn = false; });
            }
            if (recursive)
                InitializeConflict(conflict, toMod, false);
        }

        public void SetupConflictText()
        {
            GameplayModifierToggle currentToggle = gameObject.GetComponent<GameplayModifierToggle>();
            if (currentToggle == null)
                return;

            foreach (GameplayModifierToggle conflictToggle in Resources.FindObjectsOfTypeAll<GameplayModifierToggle>())
            {
                if (!conflicts.Contains(conflictToggle.gameplayModifier.modifierName)) continue;

                // Setup our hint text and onValueChanged callbacks for the conflicting modifiers
                InitializeConflict(conflictToggle, currentToggle);
            }
        }
    }
    
    public class MultiSelectOption : GameOption
    {
        private Dictionary<float, string> _options = new Dictionary<float, string>();
        public Func<float> GetValue;
        public event Action<float> OnChange;
        public ListViewController multiSelectController;

        public void InvokeIncButtonPress()
        {
            multiSelectController?.IncButtonPressed();
        }

        public void InvokeDecButtonPress()
        {
            multiSelectController?.DecButtonPressed();
        }
        
        public MultiSelectOption(GameplaySettingsPanels panel, string optionName, string hintText)
        {
            this.optionName = optionName;
            this.hintText = hintText;
            GetPanelNames(panel, ref pageName, ref panelName);
        }

        public override void Instantiate()
        {
            var volumeSettings = Resources.FindObjectsOfTypeAll<FormattedFloatListSettingsController>().FirstOrDefault();
            gameObject = UnityEngine.Object.Instantiate(volumeSettings.gameObject, Container);
            gameObject.name = optionName;
            gameObject.GetComponentInChildren<TMP_Text>().text = optionName;

            // Add a separator for this menu option
            AddSeparator(Container);

            //This magical nonsense is courtesy of Taz and his SettingsUI class
            ListSettingsController volume = gameObject.GetComponent<ListSettingsController>();
            multiSelectController = (ListViewController)ReflectionUtil.CopyComponent(volume, typeof(ListSettingsController), typeof(ListViewController), gameObject);
            UnityEngine.Object.DestroyImmediate(volume);

            multiSelectController.applyImmediately = true;
            multiSelectController.values = _options.Keys.ToList();
            multiSelectController.SetValue = OnChange;
            multiSelectController.GetValue = () =>
            {
                if (GetValue != null) return GetValue.Invoke();
                return _options.Keys.ElementAt(0);
            };
            multiSelectController.GetTextForValue = (v) =>
            {
                if (_options.ContainsKey(v)) return _options[v] != null ? _options[v] : v.ToString();
                return "UNKNOWN";
            };
            //Initialize the controller, as if we had just opened the settings menu
            multiSelectController.Init();
            var value = multiSelectController.gameObject.transform.Find("Value");
            var valueText = value.Find("ValueText");
            TMP_Text valueTextObject = valueText.GetComponent<TMP_Text>();
            valueTextObject.lineSpacing = -50;
            valueTextObject.alignment = TextAlignmentOptions.CenterGeoAligned;

            var nameText = multiSelectController.gameObject.transform.Find("NameText");
            nameText.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            value.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            if (hintText != String.Empty)
                BeatSaberUI.AddHintText(nameText as RectTransform, hintText);

            var dec = value.Find("DecButton");
            dec.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            var inc = value.Find("IncButton");
            inc.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            value.localPosition -= new Vector3(8, 0.3f);

            gameObject.SetActive(false);
            initialized = true;
        }

        public void AddOption(float value)
        {
            _options.Add(value, Convert.ToString(value));
        }

        public void AddOption(float value, string option)
        {
            _options.Add(value, option);
        }
    }

}
