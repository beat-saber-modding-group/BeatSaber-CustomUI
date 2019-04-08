using CustomUI.BeatSaber;
using CustomUI.Utilities;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace CustomUI.Settings
{
    public class CustomSettingsListViewController : CustomListViewController
    {
        private readonly int _rowHeight = 7;
        private readonly int _maxOptionsPerPage = 7;
        private readonly float _settingsViewControllerPadding = 5f;
        private readonly float _settingsViewControllerWidth = 100f;
        private List<GameObject> _submenuOptions = new List<GameObject>();

        private static TableCell _settingsTableCellInstance;
        private static Button pageUpButton, pageDownButton;
        
        protected override void DidActivate(bool firstActivation, ActivationType type)
        { 
            try
            {
                if (firstActivation)
                {
                    _submenuOptions.ForEach(s => s.transform.SetParent(null, false));
                    if (_settingsTableCellInstance == null)
                    {
                        var settingsListItem = new GameObject("SettingsTableCell");

                        var listItemRectTransform = settingsListItem.AddComponent<RectTransform>();
                        listItemRectTransform.anchorMin = new Vector2(0f, 0f);
                        listItemRectTransform.anchorMax = new Vector2(1f, 1f);

                        var horiz = settingsListItem.AddComponent<HorizontalLayoutGroup>();
                        horiz.spacing = 2f;
                        horiz.childControlHeight = true;
                        horiz.childControlWidth = false;
                        horiz.childForceExpandWidth = false;
                        horiz.childAlignment = TextAnchor.MiddleLeft;
                        (horiz.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
                        horiz.padding = new RectOffset(2, 2, _rowHeight/2, _rowHeight/2);

                        _settingsTableCellInstance = settingsListItem.gameObject.AddComponent<TableCell>();
                        _settingsTableCellInstance.reuseIdentifier = "CustomUISettingsTableCell";
                    }
                }
                base.DidActivate(firstActivation, type);
                
                int numOptions = _submenuOptions.Count() > _maxOptionsPerPage ? _maxOptionsPerPage : _submenuOptions.Count();
                float listHeight = numOptions * _rowHeight;
               
                if (firstActivation)
                {
                    // Use one set of page up/down buttons for all the CustomSettingsListViewControllers, as we might  
                    // have a lot of settings pages and theres no reason to use separate buttons for each one
                    if (pageUpButton == null)
                    {
                        pageUpButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageUpButton")), rectTransform.parent, false);
                        pageUpButton.transform.localScale /= 1.4f;
                        pageUpButton.interactable = true;
                    }
                    if (pageDownButton == null)
                    {
                        pageDownButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageDownButton")), rectTransform.parent, false);
                        pageDownButton.transform.localScale /= 1.4f;
                        pageDownButton.interactable = true;
                    }
                    
                    // Set the size of the listTableView (this is the area where list items are displayed)
                    (_customListTableView.transform as RectTransform).sizeDelta = new Vector2(0f, listHeight);
                    (_customListTableView.transform as RectTransform).localPosition += new Vector3(0f, _settingsViewControllerPadding);

                    // Destroy the content size fitter so we can manually resize the black rounded background image to fit our list
                    var content = (rectTransform.Find("Content") as RectTransform);
                    DestroyImmediate(content.gameObject.GetComponent<ContentSizeFitter>());
                    var image = content.GetComponent<Image>();
                    image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, listHeight - 77);
                    image.rectTransform.localPosition = new Vector2(image.rectTransform.localPosition.x, 5);

                    // Fit the tableview to our window
                    (_customListTableView.transform.parent as RectTransform).sizeDelta = new Vector2(_settingsViewControllerWidth, 0);

                }
                // Attach the page buttons to the current settings page
                pageUpButton.transform.SetParent(rectTransform.Find("CustomListContainer"));
                pageDownButton.transform.SetParent(rectTransform.Find("CustomListContainer"));

                // Move the page buttons depending on how many options there are
                (pageUpButton.transform as RectTransform).anchoredPosition = new Vector2(0f, listHeight / 2 + 1.25f + _settingsViewControllerPadding);
                (pageDownButton.transform as RectTransform).anchoredPosition = new Vector2(0f, -listHeight / 2 - 1.25f + _settingsViewControllerPadding);


                pageUpButton.onClick.RemoveAllListeners();
                pageUpButton.onClick.AddListener(() => _customListTableView.PageScrollUp());
                pageDownButton.onClick.RemoveAllListeners();
                pageDownButton.onClick.AddListener(() => _customListTableView.PageScrollDown());

                // And finally, show/hide the buttons depending on whether or not we have enough menu options
                pageUpButton.gameObject.SetActive(_submenuOptions.Count > _maxOptionsPerPage);
                pageDownButton.gameObject.SetActive(_submenuOptions.Count > _maxOptionsPerPage);
            }
            catch (Exception e)
            {
                Plugin.Log(e.ToString(), IPA.Logging.Logger.Level.Error);
            }
        }

        public void AddSubmenuOption(GameObject option)
        {
            option.transform.SetParent(transform.Find("Content").Find("SettingsContainer"));
            _submenuOptions.Add(option);
        }

        protected override void DidDeactivate(DeactivationType type)
        {
            base.DidDeactivate(type);
        }

        public override float CellSize()
        {
            return _rowHeight;
        }

        public override int NumberOfCells()
        {
            return _submenuOptions.Count();
        }

        public override TableCell CellForIdx(int row)
        {
            Vector2 cellSize = new Vector2(_settingsViewControllerWidth - _settingsViewControllerPadding*2, _rowHeight);
            TableCell _tableCell = _customListTableView.DequeueReusableCellForIdentifier("CustomUISettingsTableCell");
            if(!_tableCell)
                _tableCell = Instantiate(_settingsTableCellInstance);
            
            RectTransform container = new GameObject("container", typeof(RectTransform)).GetComponent<RectTransform>();
            container.SetParent(_tableCell.transform);
            container.sizeDelta = cellSize;
            
            (_submenuOptions[row].transform as RectTransform).anchoredPosition = new Vector2(_settingsViewControllerWidth/2, _rowHeight / 2);
            (_submenuOptions[row].transform as RectTransform).sizeDelta = cellSize;
            _submenuOptions[row].transform.SetParent(container, false);

            _tableCell.reuseIdentifier = "CustomUISettingsTableCell";


            return _tableCell;
        }
    }
}
