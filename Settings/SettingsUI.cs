using CustomUI.UIElements;
using CustomUI.BeatSaber;
using CustomUI.Utilities;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRUI;
using Image = UnityEngine.UI.Image;
using static HMUI.Scrollbar;
using System.Collections;

namespace CustomUI.Settings
{
    public class SettingsUI : MonoBehaviour
    {
        private MainMenuViewController _mainMenuViewController = null;
        private SettingsNavigationController settingsMenu = null;
        private MainSettingsMenuViewController mainSettingsMenu = null;
        private MainSettingsTableView _mainSettingsTableView = null;
        private TableView subMenuTableView = null;
        private TableViewHelper subMenuTableViewHelper = null;
        private Transform othersSubmenu = null;
        private SimpleDialogPromptViewController prompt = null;

        private Button _pageUpButton = null;
        private Button _pageDownButton = null;
        private Vector2 buttonOffset = new Vector2(24, 0);
        private bool initialized = false;

        private static SettingsUI _instance = null;
        public static SettingsUI Instance
        {
            get
            {
                if (!_instance)
                    _instance = new GameObject("SettingsUI").AddComponent<SettingsUI>();
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            BSEvents.menuSceneLoadedFresh += BSEvents_menuSceneLoadedFresh;
            StartCoroutine(InitSettings());

        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            BSEvents.menuSceneLoadedFresh -= BSEvents_menuSceneLoadedFresh;
        }

        private void BSEvents_menuSceneLoadedFresh()
        {
            StartCoroutine(InitSettings());
        }

        public void SceneManagerOnActiveSceneChanged(Scene from, Scene to)
        {
            if (to.name == "EmptyTransition")
            {
                if (Instance)
                    Destroy(Instance.gameObject);
                initialized = false;
            }
        }
        public IEnumerator InitSettings()
        {
            yield return new WaitForSeconds(0.1f);
            //Init settings
            foreach (CustomSetting customSetting in SubMenu.needsInit)
            {
                try
                {
                    customSetting.Init();
                }
                catch (Exception ex)
                {
                    Plugin.Log(ex.ToString(), IPA.Logging.Logger.Level.Error);
                }
            }
            SubMenu.needsInit.Clear();
        }

        private void SetupUI()
        {
            if (initialized) return;

            try
            {
                var _menuMasterViewController = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();
                prompt = ReflectionUtil.GetPrivateField<SimpleDialogPromptViewController>(_menuMasterViewController, "_simpleDialogPromptViewController");

                _mainMenuViewController = Resources.FindObjectsOfTypeAll<MainMenuViewController>().First();
                settingsMenu = Resources.FindObjectsOfTypeAll<SettingsNavigationController>().FirstOrDefault();
                mainSettingsMenu = Resources.FindObjectsOfTypeAll<MainSettingsMenuViewController>().FirstOrDefault();
                _mainSettingsTableView = mainSettingsMenu.GetPrivateField<MainSettingsTableView>("_mainSettingsTableView");
                subMenuTableView = _mainSettingsTableView.GetPrivateField<TableView>("_tableView");
                subMenuTableViewHelper = subMenuTableView.gameObject.AddComponent<TableViewHelper>();
                othersSubmenu = settingsMenu.transform.Find("OtherSettings");
                
                initialized = true;
            }
            catch (Exception ex)
            {
                Plugin.Log(ex.ToString(), IPA.Logging.Logger.Level.Error);
            }
        }

        private void AddPageButtons()
        {
            try
            {
                RectTransform viewport = _mainSettingsTableView.GetComponentsInChildren<RectTransform>().First(x => x.name == "Viewport");
                viewport.anchorMin = new Vector2(0f, 0.5f);
                viewport.anchorMax = new Vector2(1f, 0.5f);
                viewport.sizeDelta = new Vector2(0f, 48f);
                viewport.anchoredPosition = new Vector2(0f, 0f);

                RectTransform container = (RectTransform)_mainSettingsTableView.transform;
                

                if (_pageUpButton == null)
                {
                    _pageUpButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageUpButton")), container);

                    _pageUpButton.transform.SetParent(container.parent);
                    _pageUpButton.transform.localScale /= 1.4f;
                    _pageUpButton.transform.localPosition += new Vector3(0, 4f);
                    //_pageUpButton.interactable = false;

                    _pageUpButton.onClick.RemoveAllListeners();
                    _pageUpButton.onClick.AddListener(() =>
                    {
                        subMenuTableView.GetPrivateField<RectTransform>("_scrollRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 48);
                        subMenuTableViewHelper.PageScrollUp();
                    });
                }

                if (_pageDownButton == null)
                {
                    _pageDownButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageDownButton")), container);

                    _pageDownButton.transform.SetParent(container.parent);
                    _pageDownButton.transform.localScale /= 1.4f;
                    _pageDownButton.transform.localPosition -= new Vector3(0, 5f);
                    //_pageDownButton.interactable = false;

                    _pageDownButton.onClick.RemoveAllListeners();
                    _pageDownButton.onClick.AddListener(() =>
                    {
                        subMenuTableView.GetPrivateField<RectTransform>("_scrollRectTransform").SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 48);
                        subMenuTableViewHelper.PageScrollDown();
                    });
                }
            }
            catch (Exception ex)
            {
                Plugin.Log(ex.ToString(), IPA.Logging.Logger.Level.Error);
            }
        }


        public static SubMenu CreateSubMenu(string name)
        {
            return CreateSubMenu(name, true);
        }

        public static SubMenu CreateSubMenu(string name, bool showInList)
        {
            lock(Instance) {
                Instance.SetupUI();

                var subMenuGameObject = Instantiate(Instance.othersSubmenu.gameObject, Instance.othersSubmenu.transform.parent);
                subMenuGameObject.name = name.Replace(" ", "");
                Transform mainContainer = CleanScreen(subMenuGameObject.transform);

                DestroyImmediate(subMenuGameObject.GetComponent<VRUIViewController>());
                var customSettingsViewController = subMenuGameObject.AddComponent<CustomSettingsListViewController>();
                customSettingsViewController.name = name.Replace(" ", "");
                customSettingsViewController.includePageButtons = false;

                if (showInList)
                {
                    var newSubMenuInfo = new SettingsSubMenuInfo();
                    newSubMenuInfo.SetPrivateField("_menuName", name);
                    newSubMenuInfo.SetPrivateField("_viewController", customSettingsViewController);

                    var subMenuInfos = Instance.mainSettingsMenu.GetPrivateField<SettingsSubMenuInfo[]>("_settingsSubMenuInfos").ToList();
                    subMenuInfos.Add(newSubMenuInfo);
                    Instance.mainSettingsMenu.SetPrivateField("_settingsSubMenuInfos", subMenuInfos.ToArray());
                    //Instance._mainSettingsTableView.SetPrivateField("_settingsSubMenuInfos", subMenuInfos.ToArray());

                    if (subMenuInfos.Count > 6)
                        Instance.AddPageButtons();
                }
                
                SubMenu menu = new SubMenu(customSettingsViewController);
                return menu;
            }
        }
        
        static Transform CleanScreen(Transform screen)
        {
            var container = screen.Find("Content").Find("SettingsContainer");
            var tempList = container.Cast<Transform>().ToList();
            foreach (var child in tempList)
            {
                DestroyImmediate(child.gameObject);
            }
            return container;
        }
    }
}
