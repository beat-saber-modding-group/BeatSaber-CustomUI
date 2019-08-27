using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CustomUI.BeatSaber;
using CustomUI.Utilities;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using VRUI;
using IPA.Config;

namespace CustomUI.MenuButton
{
    public class MenuButtonUI : MonoBehaviour
    {
        private static readonly WaitUntil _bottomPanelExists = new WaitUntil(() => GameObject.Find("MainMenuViewController/BottomPanel"));
        const int ButtonsPerRow = 4;
        const float RowSeparator = 9f;

        internal RectTransform bottomPanel;
        internal RectTransform menuButtonsOriginal;
        
        private RectTransform currentRow;
        private List<RectTransform> rows;
        private int buttonsInCurrentRow = ButtonsPerRow;

        private List<MenuButton> buttonData;
        private List<String> pinnedButtons;
        private MenuButtonListViewController _menuButtonListViewController;

        private static MenuButtonUI _instance = null;
        public static MenuButtonUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("MenuButtonUI").AddComponent<MenuButtonUI>();
                    _instance.Init();
                }
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
                if (Instance)
                {
                    Instance.StopAllCoroutines();
                    Destroy(Instance.gameObject);
                }
                Instance = null;
            }
        }

        private void Init()
        {
            buttonData = new List<MenuButton>();
            rows = new List<RectTransform>();
            pinnedButtons = ModPrefs.GetString("CustomUI", "PinnedMenuButtons", "", true).Split(',').ToList();
            
            StartCoroutine(AddMenuButtonListButton());
        }
        
        private static IEnumerator AddMenuButtonListButton()
        {
            yield return _bottomPanelExists;

            lock (Instance)
            {
                if (Instance.bottomPanel == null) Instance.bottomPanel = GameObject.Find("MainMenuViewController/BottomPanel").transform as RectTransform;
                if (Instance.menuButtonsOriginal == null) Instance.menuButtonsOriginal = Instance.bottomPanel.Find("Buttons") as RectTransform;

                Button newButton = BeatSaberUI.CreateUIButton(Instance.menuButtonsOriginal as RectTransform, "CreditsButton", () => Instance.PresentList(), "Mods", null);
                Instance.menuButtonsOriginal.Find("QuitButton").SetAsLastSibling();
            }
        }

        private void PresentList()
        {
            if (_menuButtonListViewController == null)
            {
                _menuButtonListViewController = BeatSaberUI.CreateViewController<MenuButtonListViewController>();
                _menuButtonListViewController.pinButtonPushed += PinButtonWasPushed;
                _menuButtonListViewController.backButtonPressed += ListBackPressed;
            }
            _menuButtonListViewController.SetData(buttonData);

            // Using MainFlowCoordinator instead of CustomMenu for compatibility with mods
            FlowCoordinator flowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();
            flowCoordinator.InvokePrivateMethod("PresentViewController", new object[] { _menuButtonListViewController, null, false });
            flowCoordinator.SetProperty("title", "Mods");
        }

        private void ListBackPressed()
        {
            FlowCoordinator flowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();
            flowCoordinator.InvokePrivateMethod("DismissViewController", new object[] { _menuButtonListViewController, null, false });
            flowCoordinator.SetProperty("title", string.Empty);
        }

        private static IEnumerator AddButtonToMainMenuCoroutine(MenuButton button)
        {
            yield return _bottomPanelExists;

            if (Instance.bottomPanel == null) Instance.bottomPanel = GameObject.Find("MainMenuViewController/BottomPanel").transform as RectTransform;
            if (Instance.menuButtonsOriginal == null) Instance.menuButtonsOriginal = Instance.bottomPanel.Find("Buttons") as RectTransform;

            lock (Instance)
            {
                Instance.AddButtonToMainMenu(button);
            }
        }

        private void AddButtonToMainMenu(MenuButton button)
        {
            if (buttonsInCurrentRow >= ButtonsPerRow)
            {
                AddRow();
            }
            Button newButton = BeatSaberUI.CreateUIButton(currentRow, "QuitButton", button.onClick, button.text, button.icon);
         //   newButton.GetComponentInChildren<HorizontalLayoutGroup>()?.padding = new RectOffset(6, 6, 0, 0);
            newButton.name = button.text;
            if (!string.IsNullOrWhiteSpace(button.hintText))
                BeatSaberUI.AddHintText(newButton.transform as RectTransform, button.hintText);
            button.buttons.Add(newButton);
            newButton.interactable = button.interactable;
            buttonsInCurrentRow++;
        }

        public static MenuButton AddButton(string buttonText, string hintText, UnityAction onClick, Sprite icon = null)
        {
            bool pin = Instance.pinnedButtons.Contains(buttonText);

            MenuButton menuButton = new MenuButton(buttonText, hintText, onClick, icon, pin);
            Instance.buttonData.Add(menuButton);
            if (menuButton.pinned)
            {
                Instance.StartCoroutine(AddButtonToMainMenuCoroutine(menuButton));
            }
            return menuButton;
        }

        public static void AddButton(string buttonText, UnityAction onClick, Sprite icon = null)
        {
            AddButton(buttonText, String.Empty, onClick, icon);
        }
        
        void PinButton(MenuButton menuButton)
        {
            if (menuButton.pinned) return;
            menuButton.pinned = true;
            if (!pinnedButtons.Contains(menuButton.text)) pinnedButtons.Add(menuButton.text);
            ModPrefs.SetString("CustomUI", "PinnedMenuButtons", string.Join(",", pinnedButtons));
            AddButtonToMainMenu(menuButton);
        }

        void UnpinButton(MenuButton menuButton)
        {
            if (!menuButton.pinned) return;
            menuButton.pinned = false;
            if(pinnedButtons.Contains(menuButton.text)) pinnedButtons.Remove(menuButton.text);
            ModPrefs.SetString("CustomUI", "PinnedMenuButtons", string.Join(",", pinnedButtons));
            RemoveButtonFromMainMenu(menuButton);
            //Rebuild();
        }

        void PinButtonWasPushed(MenuButton button)
        {
            if (button.pinned)
            {
                UnpinButton(button);
            } else
            {
                // todo: max pins
                PinButton(button);
            }
        }

        private void AddRow()
        {
            RectTransform newRow = Instantiate(menuButtonsOriginal, bottomPanel);
            rows.Add(newRow);
            currentRow = newRow;
            buttonsInCurrentRow = 0;
            newRow.name = "CustomMenuButtonsRow" + rows.Count.ToString();

            foreach (Transform child in newRow)
            {
                child.name = String.Empty;
                Destroy(child.gameObject);
            }
            newRow.anchorMin = new Vector3(-0.03f, 0.175f, 0);
            newRow.anchorMax = new Vector3(1.03f, 0.1f, 0);
            newRow.anchoredPosition += Vector2.up * RowSeparator * rows.Count;
        }

        private void RemoveButtonFromMainMenu(MenuButton menuButton)
        {
            var tmpRows = rows.ToArray();
            // Cycle through each row in our main menu buttons
            for(int i=0; i<tmpRows.Length; i++)
            {
                // For each row, get a listing of the buttons
                var buttons = tmpRows[i].GetComponentsInChildren<Button>();
                foreach(Button currentButton in buttons.ToArray())
                {
                    // Check if any of our buttons in the MenuButton button array are this button
                    if (!menuButton.buttons.Any(tmpBut => tmpBut == currentButton)) continue;
                    
                    // If the currentButton is found, destroy it
                    DestroyImmediate(currentButton.gameObject);
                    
                    // Shift all the existing buttons forward if need be to fill the empty spot we may have just created
                    for (int l=1; l<tmpRows.Length; l++)
                    {
                        var prevRowButtons = tmpRows[l - 1].GetComponentsInChildren<Button>();
                        var curRowButtons = tmpRows[l].GetComponentsInChildren<Button>();
                        if (prevRowButtons.Count() < 4 && curRowButtons.Count() > 0)
                            curRowButtons.First().transform.SetParent(tmpRows[l - 1], false);
                    }
                    // Decrement buttonsInCurrentRow, since it must be changing as we destroyed a button
                    buttonsInCurrentRow--;

                    // If there are still buttons in this row, we're done.
                    if (buttonsInCurrentRow > 0) return;

                    // Otherwise, destroy the row
                    Destroy(currentRow.gameObject);
                    rows.Remove(rows.Last());

                    // Finally, set the current row reference to the correct row
                    if (rows.Count > 0)
                        currentRow = menuButtonsOriginal.parent.Find($"CustomMenuButtonsRow{rows.Count}") as RectTransform;
                    buttonsInCurrentRow = ButtonsPerRow;

                    // No need to proceed any further, we found the button in question and dealt with it
                    return;
                }
            }
        }
    }
}
