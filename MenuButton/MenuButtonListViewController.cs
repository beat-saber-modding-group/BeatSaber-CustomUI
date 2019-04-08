using CustomUI.BeatSaber;
using CustomUI.Utilities;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRUI;
using TMPro;

namespace CustomUI.MenuButton
{
    class MenuButtonListViewController : CustomListViewController, TableView.IDataSource
    {
        private List<MenuButton> buttons;

        readonly int buttonsPerRow = 3;
        readonly Vector2 buttonSize = new Vector2(40, 8);
        AnimationClip _normalClip;
        AnimationClip _highlightedClip;
        
        TableCell _modListTableCellInstance;
        public event Action<MenuButton> pinButtonPushed;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            try
            {
                if (firstActivation)
                {
                    if (_modListTableCellInstance == null)
                    {
                        var modMenuListItem = new GameObject("ModMenuListTableCell");

                        var listItemRectTransform = modMenuListItem.AddComponent<RectTransform>();
                        listItemRectTransform.anchorMin = new Vector2(0f, 0f);
                        listItemRectTransform.anchorMax = new Vector2(1f, 1f);

                        var horiz = modMenuListItem.AddComponent<HorizontalLayoutGroup>();
                        horiz.spacing = 2f;
                        horiz.childControlHeight = false;
                        horiz.childControlWidth = false;
                        horiz.childForceExpandWidth = false;
                        horiz.childAlignment = TextAnchor.MiddleCenter;
                        (horiz.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
                        horiz.padding = new RectOffset(0, 0, 4, 4);

                        _modListTableCellInstance = modMenuListItem.gameObject.AddComponent<TableCell>();

                        BeatSaberUI.CreateBackButton(rectTransform, () => backButtonPressed?.Invoke());
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log(e.ToString(), IPA.Logging.Logger.Level.Error);
            }
            base.DidActivate(firstActivation, type);

            RectTransform container = (transform.Find("CustomListContainer") as RectTransform);
            container.sizeDelta = new Vector2(150, 0);
            container.anchoredPosition = new Vector2(0, 0);
        }

        internal void SetData(List<MenuButton> buttonData)
        {
            buttons = buttonData;
        }
        

        public override float CellSize()
        {
            return 10f;
        }

        public override int NumberOfCells()
        {
            return (int)Math.Ceiling((float)buttons.Count / buttonsPerRow);
        }

        public override TableCell CellForIdx(int row)
        {
            TableCell _tableCell = Instantiate(_modListTableCellInstance);

            for (int i = 0; i < buttonsPerRow; i++)
            {
                int index = row * buttonsPerRow + i;
                if (buttons.Count > index)
                {
                    var menuButton = buttons.ElementAt(index);

                    RectTransform container = new GameObject("container", typeof(RectTransform)).GetComponent<RectTransform>();
                    container.SetParent(_tableCell.transform);
                    container.sizeDelta = buttonSize;

                    Button newButton = BeatSaberUI.CreateUIButton(container, "QuitButton", new Vector2(0,0), buttonSize ,menuButton.onClick, menuButton.text, menuButton.icon);
                    newButton.GetComponentInChildren<HorizontalLayoutGroup>().padding = new RectOffset(6, 8, 0, 0);
                    newButton.GetComponentInChildren<TextMeshProUGUI>().lineSpacing = -65;
                    newButton.name = menuButton.text;
                    if (menuButton.hintText != String.Empty)
                        BeatSaberUI.AddHintText(newButton.transform as RectTransform, menuButton.hintText);
                    menuButton.buttons.Add(newButton);
                    newButton.interactable = menuButton.interactable;

                    //  sub button
                    var pinButton = BeatSaberUI.CreateUIButton(container, "QuitButton", new Vector2(-6,0), new Vector2(8,8), null, "", null);

                    if (_highlightedClip == null) _highlightedClip = pinButton.GetComponent<ButtonStaticAnimations>().GetPrivateField<AnimationClip>("_highlightedClip");
                    if (_normalClip == null) _normalClip = pinButton.GetComponent<ButtonStaticAnimations>().GetPrivateField<AnimationClip>("_normalClip");

                    PinButtonPushEffect(pinButton, menuButton);
                    (pinButton.transform as RectTransform).anchorMin = new Vector2(1, 0.5f);
                    (pinButton.transform as RectTransform).anchorMax = new Vector2(1, 0.5f);
                    (pinButton.transform as RectTransform).anchoredPosition = new Vector2(-4, 0);
                    pinButton.GetComponentInChildren<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
                    pinButton.onClick.AddListener( () => { pinButtonPushed?.Invoke(menuButton); PinButtonPushEffect(pinButton, menuButton); });
                }
            }

            return _tableCell;
        }

        void PinButtonPushEffect(Button pinButton, MenuButton menuButton)
        {   
            if (menuButton.pinned)
            {
                pinButton.SetButtonText("+");
                pinButton.SetButtonTextSize(8);
                pinButton.transform.Find("Wrapper/Stroke").gameObject.SetActive(true);
                (pinButton.transform as RectTransform).Rotate(0, 0, 45);
                pinButton.GetComponent<ButtonStaticAnimations>()?.SetPrivateField("_normalClip", _highlightedClip);
                pinButton.GetComponent<ButtonStaticAnimations>()?.SetPrivateField("_highlightedClip", _normalClip);
            } else
            {
                pinButton.SetButtonText("+");
                pinButton.SetButtonTextSize(6);
                pinButton.transform.Find("Wrapper/Stroke").gameObject.SetActive(false);
                (pinButton.transform as RectTransform).localRotation = Quaternion.identity;
                pinButton.GetComponent<ButtonStaticAnimations>()?.SetPrivateField("_normalClip", _normalClip);
                pinButton.GetComponent<ButtonStaticAnimations>()?.SetPrivateField("_highlightedClip", _highlightedClip);
            }
        }
    }
}
