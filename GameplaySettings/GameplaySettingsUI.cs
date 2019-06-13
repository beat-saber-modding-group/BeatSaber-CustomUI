using CustomUI.BeatSaber;
using CustomUI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomUI.GameplaySettings
{
    public class GameplaySettingsUI : MonoBehaviour
    {
        private static Sprite _backButton = null;

        public bool initialized = false;
        private Button _pageDownButton = null;
        private Button _pageUpButton = null;
        private int _pageIndex = 0;
        private string _currentSubmenu = "MainMenu";
        private GameplaySettingsPanels panel;
        private Dictionary<string, MenuInfo> _customMenus = new Dictionary<string, MenuInfo>();
        private List<Transform> _defaultSeparators = new List<Transform>();
        private List<GameOption> _currentOptions = null;
        private RectTransform _panelContainer = null;
        private Transform[] _defaultOptions = new Transform[5];
        private int numDefaultOptions = 4;

        private class MenuInfo {
            public List<GameOption> options = new List<GameOption>();
            public string _previousMenu = string.Empty;
        };

        private static Dictionary<GameplaySettingsPanels, GameplaySettingsUI> _instances = new Dictionary<GameplaySettingsPanels, GameplaySettingsUI>();
        public static Dictionary<GameplaySettingsPanels, GameplaySettingsUI> Instance
        {
            get
            {
                for (int i = 0; i < 4; i++)
                    CreateInstance((GameplaySettingsPanels)i);

                return _instances;
            }
            private set
            {
                _instances = value;
            }
        }

        private static void CreateInstance(GameplaySettingsPanels panel)
        {
            if (_instances.ContainsKey(panel) && _instances[panel] != null) return;

            _instances[panel] = new GameObject("GameplaySettingsUI").AddComponent<GameplaySettingsUI>();
            _instances[panel].panel = panel;
        }
        
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            _customMenus[_currentSubmenu] = new MenuInfo();
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene from, Scene to)
        {
            if (to.name == "EmptyTransition")
            {
                Destroy(this.gameObject);
                initialized = false;
            }
            else if (to.name == "MenuCore")
            {
                StartCoroutine(Build());
            }
        }

        //Returns a list of options for the current page index
        private List<GameOption> GetOptionsForPage(int page)
        {
            if (_currentSubmenu == "MainMenu")
            {
                //Default options
                if (page == 0) return null;

                page--; //If the page isn't 0, we should pick from the 0th pagination of our list
            }
            
            //Get  custom options and return them
            return _customMenus[_currentSubmenu].options.Skip(numDefaultOptions * page).Take(numDefaultOptions).ToList();
        }

        //Sets the active value for our game options depending on the active page
        private void ChangePage(int page, Transform container, params Transform[] defaults)
        {
            _currentOptions = Instance[panel].GetOptionsForPage(Instance[panel]._pageIndex);
            bool defaultsActive = _currentOptions == null;
            defaults?.ToList().ForEach(x => x?.gameObject.SetActive(defaultsActive));
            
            if (defaultsActive)
            {
                for (int i = 0; i < _defaultSeparators.Count; i++)
                    _defaultSeparators[i].gameObject.SetActive(true);

                _customMenus.Values.ToList().ForEach(m => m.options.ForEach(o => o.separator.SetActive(false)));
            }
            else
            {
                foreach (MenuInfo subMenu in _customMenus.Values)
                    foreach (GameOption g in subMenu.options)
                        g.separator.SetActive(false);

                _currentOptions[_currentOptions.Count-1].separator.SetActive(false);
                for (int i = 0; i < _currentOptions.Count-1; i++)
                    _currentOptions[i].separator.SetActive(true);

                _defaultSeparators.ForEach(s => s.gameObject.SetActive(false));
            }
            
            //Custom options
            _customMenus.Values.ToList().ForEach(m => m.options.ForEach(x => x.gameObject.SetActive(false)));
            if (!defaultsActive) _currentOptions?.ToList().ForEach(x => x.gameObject.SetActive(true));
            
            RefreshScrollButtons();
        }

        public void RefreshScrollButtons()
        {
            int index = (_pageIndex) * 5;
            if (_currentSubmenu == "MainMenu")
                index -= 5;

            if (index + 5 < _customMenus[_currentSubmenu].options.Count) _pageDownButton.gameObject.SetActive(true);
            else _pageDownButton.gameObject.SetActive(false);
            
            if (_pageIndex <= 0) _pageUpButton.gameObject.SetActive(false);
            else _pageUpButton.gameObject.SetActive(true);
        }

        public static void EnterSubmenu(GameplaySettingsPanels panel, string menuName)
        {
            lock (Instance[panel])
            {
                var instance = Instance[panel];
                if (menuName == "!PREVIOUSMENU!")
                    menuName = instance._customMenus[instance._currentSubmenu]._previousMenu;
                else
                    instance._customMenus[menuName]._previousMenu = instance._currentSubmenu;

                if (instance._customMenus.ContainsKey(menuName))
                {
                    instance._currentSubmenu = menuName;
                    instance._pageIndex = 0;
                    instance.ChangePage(0, instance._panelContainer, instance._defaultOptions);
                }
            }
        }

        private static void CreateBackButton(GameplaySettingsPanels panel, string submenuName)
        {
            if (submenuName == "MainMenu") return;
            if (_backButton == null)
                _backButton = UIUtilities.LoadSpriteFromResources("CustomUI.Resources.Back Button.png");
            CreateSubmenuOption(panel, "Back", submenuName, "!PREVIOUSMENU!", "Go Back to Previous Menu", _backButton);
        }

        public static MultiSelectOption CreateListOption(GameplaySettingsPanels panel, string optionName, string submenuName, string hintText = "")
        {
            lock (Instance[panel])
            {
                MultiSelectOption ret = new MultiSelectOption(panel, optionName, hintText);
                if (!Instance[panel]._customMenus.ContainsKey(submenuName))
                {
                    Instance[panel]._customMenus[submenuName] = new MenuInfo();
                    CreateBackButton(panel, submenuName);
                }

                Instance[panel]._customMenus[submenuName].options.Add(ret);
                return ret;
            }
        }

        public static MultiSelectOption CreateListOption(GameplaySettingsPanels panel, string optionName, string hintText = "")
        {
            return CreateListOption(panel, optionName, "MainMenu", hintText);
        }

        public static MultiSelectOption CreateListOption(string optionName, string hintText)
        {
            return CreateListOption(GameplaySettingsPanels.ModifiersRight, optionName, hintText);
        }

        public static MultiSelectOption CreateListOption(string optionName)
        {
            return CreateListOption(optionName, "");
        }

        public static ToggleOption CreateToggleOption(GameplaySettingsPanels panel, string optionName, string submenuName, string hintText = "", Sprite optionIcon = null, float multiplier = 0f)
        {
            lock (Instance[panel])
            {
                ToggleOption ret = new ToggleOption(panel, optionName, hintText, optionIcon, multiplier);
                if (!Instance[panel]._customMenus.ContainsKey(submenuName))
                {
                    Instance[panel]._customMenus[submenuName] = new MenuInfo();
                    CreateBackButton(panel, submenuName);
                }

                Instance[panel]._customMenus[submenuName].options.Add(ret);
                return ret;
            }
        }

        public static ToggleOption CreateToggleOption(GameplaySettingsPanels panel, string optionName, string hintText = "", Sprite optionIcon = null, float multiplier = 0f)
        {
            return CreateToggleOption(panel, optionName, "MainMenu", hintText, optionIcon, multiplier);
        }

        public static ToggleOption CreateToggleOption(string optionName, string hintText, Sprite optionIcon, float multiplier)
        {
            return CreateToggleOption(GameplaySettingsPanels.ModifiersRight, optionName, hintText, optionIcon, multiplier);
        }

        public static ToggleOption CreateToggleOption(string optionName, string hintText, Sprite optionIcon)
        {
            return CreateToggleOption(optionName, hintText, optionIcon, 0f);
        }

        public static ToggleOption CreateToggleOption(string optionName, string hintText)
        {
            return CreateToggleOption(optionName, hintText, null, 0f);
        }

        public static ToggleOption CreateToggleOption(string optionName)
        {
            return CreateToggleOption(optionName, "", null, 0f);
        }

        public static ToggleOption CreateSubmenuOption(GameplaySettingsPanels panel, string optionName, string submenuName, string submenuToEnter, string hintText = "", Sprite optionIcon = null)
        {
            lock (Instance[panel])
            {
                SubmenuOption ret = new SubmenuOption(panel, optionName, hintText, optionIcon);
                if (!Instance[panel]._customMenus.ContainsKey(submenuName))
                {
                    Instance[panel]._customMenus[submenuName] = new MenuInfo();
                    CreateBackButton(panel, submenuName);
                }

                ret.OnToggle += (e) => GameplaySettingsUI.EnterSubmenu(panel, submenuToEnter);

                Instance[panel]._customMenus[submenuName].options.Add(ret);
                return ret;
            }
        }
         
        public IEnumerator Build()
        {
            yield return new WaitForSeconds(0.1f);

            string pageName = String.Empty, panelName = String.Empty;
            GameOption.GetPanelNames(panel, ref pageName, ref panelName);
            GameOption.GetNumDefaultOptionsForPanel(panel, ref numDefaultOptions);

            //Grab necessary references
            SoloFreePlayFlowCoordinator sfpfc = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            GameplaySetupViewController gsvc = sfpfc.GetField<GameplaySetupViewController>("_gameplaySetupViewController");
            
            //Get reference to the switch container
            RectTransform page = (RectTransform)gsvc.transform.Find(pageName);
            Destroy(page.gameObject.GetComponent<HorizontalLayoutGroup>());
            Destroy(page.gameObject.GetComponent<ContentSizeFitter>());
            _panelContainer = (RectTransform)page.Find(panelName);
            
            if (!_panelContainer.gameObject.GetComponent<ContentSizeFitter>())
            {
                var fitter = _panelContainer.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            // Make the player height option the same height as other options
            var height = _panelContainer.Find("PlayerHeight");
            if (height)
            {
                var staticLights = _panelContainer.Find("StaticLights");
                (height as RectTransform).sizeDelta = (staticLights as RectTransform).sizeDelta;
                var title = height.Find("Title");
                if (title)
                {
                    (title as RectTransform).sizeDelta = new Vector2((title as RectTransform).sizeDelta.x, 1);
                    title.localPosition = new Vector3(-12.5f, 4.5f, 0f);
                    var text = title.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                    text.alignment = TextAlignmentOptions.MidlineLeft;
                    text.enableWordWrapping = false;
                }

                var measure = height.Find("MeassureButton");
                measure.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                measure.localPosition = new Vector3(10f, 0f, 0f);
                var reset = height.Find("ResetButton");
                reset.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }
           
            if (!initialized)
            {
                //Get references to the original switches, so we can later duplicate then destroy them
                GameOption.GetOptionTransforms(panel, _panelContainer, ref _defaultOptions[0], ref _defaultOptions[1], ref _defaultOptions[2], ref _defaultOptions[3], ref _defaultOptions[4]);
                
                foreach (Transform t in _panelContainer)
                {
                    if (t.name.StartsWith("Separator"))
                        _defaultSeparators.Add(t);
                }
                _defaultSeparators.Reverse();
                
                //Create up button
                _pageUpButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageUpButton")), _panelContainer);
                _pageUpButton.transform.SetParent(_panelContainer.parent);
                _pageUpButton.transform.localScale = Vector3.one / 2;
                _pageUpButton.transform.localPosition = new Vector3(_pageUpButton.transform.localPosition.x,  -2.2f, _pageUpButton.transform.localPosition.z);
                _pageUpButton.interactable = true;
                //(_pageUpButton.transform as RectTransform).sizeDelta = new Vector2((_pageUpButton.transform.parent as RectTransform).sizeDelta.x, 3.5f);
                _pageUpButton.onClick.RemoveAllListeners();
                _pageUpButton.onClick.AddListener(delegate ()
                {
                    Instance[panel].ChangePage(--Instance[panel]._pageIndex, _panelContainer, Instance[panel]._defaultOptions);
                });

                //Create down button
                _pageDownButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageDownButton")), _panelContainer);
                _pageDownButton.transform.SetParent(_panelContainer.parent);
                _pageDownButton.transform.localScale = Vector3.one / 2;
                _pageDownButton.transform.localPosition = new Vector3(_pageDownButton.transform.localPosition.x, -(numDefaultOptions * 8.5f), _pageDownButton.transform.localPosition.z);
                _pageDownButton.interactable = true;
                //(_pageDownButton.transform as RectTransform).sizeDelta = new Vector2((_pageDownButton.transform.parent as RectTransform).sizeDelta.x, (_pageDownButton.transform as RectTransform).sizeDelta.y);
                _pageDownButton.onClick.RemoveAllListeners();
                _pageDownButton.onClick.AddListener(delegate ()
                {
                    Instance[panel].ChangePage(++Instance[panel]._pageIndex, _panelContainer, Instance[panel]._defaultOptions);
                });

                RefreshScrollButtons();
                Instance[panel]._pageIndex = 0;
                initialized = true;
            }

            //Create custom options
            foreach (MenuInfo menu in Instance[panel]._customMenus.Values)
            {
                foreach (GameOption option in menu.options)
                {
                    if (!option.initialized)
                        option.Instantiate();
                }
            }
            
            // Then add conflict text
            foreach (MenuInfo menu in Instance[panel]._customMenus.Values)
            {
                foreach (GameOption option in menu.options)
                {
                    if (option is ToggleOption && option.initialized)
                        (option as ToggleOption).SetupConflictText();
                }
            }
        }
    }
}
