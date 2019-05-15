using CustomUI.Utilities;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.BeatSaber
{
    public class CustomListViewController : CustomViewController, TableView.IDataSource
    {
        /// <summary>
        /// Whether or not to include a back button when the ViewController is activated.
        /// </summary>
        public bool includePageButtons = true;

        /// <summary>
        /// A reference to the page up button, if it exists.
        /// </summary>
        public Button _pageUpButton;

        /// <summary>
        /// A reference to the page down button, if it exists.
        /// </summary>
        public Button _pageDownButton;

        /// <summary>
        /// The TableView associated with the current CustomListViewController.
        /// </summary>
        public TableView _customListTableView;

        /// <summary>
        /// The data to be displayed in the table.
        /// </summary>
        public List<CustomCellInfo> Data = new List<CustomCellInfo>();

        /// <summary>
        /// An event fired when the user selects a cell in the TableView.
        /// </summary>
        public Action<TableView, int> DidSelectRowEvent;

        /// <summary>
        /// The reuse identifier, which is used to recycle cells instead of instantiating new instances of them.
        /// </summary>
        public string reuseIdentifier = "CustomUIListTableCell";

        /// <summary>
        /// The prefab used to instantiate new table cells.
        /// </summary>
        private LevelListTableCell _songListTableCellInstance;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            try
            {
                if (firstActivation)
                {
                    _songListTableCellInstance = Resources.FindObjectsOfTypeAll<LevelListTableCell>().First(x => (x.name == "LevelListTableCell"));

                    RectTransform container = new GameObject("CustomListContainer", typeof(RectTransform)).transform as RectTransform;
                    container.SetParent(rectTransform, false);
                    container.sizeDelta = new Vector2(60f, 0f);

                    var newGameObj = new GameObject("CustomListTableView");
                    // Disable the new gameobject containing the tableview to avoid HMUI.Init error spam in the output_log
                    newGameObj.SetActive(false);
                    _customListTableView = newGameObj.AddComponent<TableView>();
                    _customListTableView.gameObject.AddComponent<RectMask2D>();
                    _customListTableView.transform.SetParent(container, false);

                    (_customListTableView.transform as RectTransform).anchorMin = new Vector2(0f, 0f);
                    (_customListTableView.transform as RectTransform).anchorMax = new Vector2(1f, 1f);
                    (_customListTableView.transform as RectTransform).sizeDelta = new Vector2(0f, 60f);
                    (_customListTableView.transform as RectTransform).anchoredPosition = new Vector3(0f, 0f);
                    
                    _customListTableView.SetPrivateField("_preallocatedCells", new TableView.CellsGroup[0]);
                    _customListTableView.SetPrivateField("_isInitialized", false);
                    _customListTableView.dataSource = this;
                    newGameObj.SetActive(true);

                    _customListTableView.didSelectCellWithIdxEvent += _customListTableView_didSelectRowEvent;

                    if (includePageButtons)
                    {
                        if (_pageUpButton == null)
                        {
                            _pageUpButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageUpButton")), container, false);
                            (_pageUpButton.transform as RectTransform).anchoredPosition = new Vector2(0f, 30f);
                            _pageUpButton.interactable = true;
                            _pageUpButton.onClick.AddListener(delegate ()
                            {
                                _customListTableView.PageScrollUp();
                            });
                        }

                        if (_pageDownButton == null)
                        {
                            _pageDownButton = Instantiate(Resources.FindObjectsOfTypeAll<Button>().Last(x => (x.name == "PageDownButton")), container, false);
                            (_pageDownButton.transform as RectTransform).anchoredPosition = new Vector2(0f, -30f);
                            _pageDownButton.interactable = true;
                            _pageDownButton.onClick.AddListener(delegate ()
                            {
                                _customListTableView.PageScrollDown();
                            });
                        }
                    }
                }
                base.DidActivate(firstActivation, type);
            }
            catch (Exception e)
            {
                Plugin.Log(e.ToString(), IPA.Logging.Logger.Level.Error);
            }
        }

        protected override void DidDeactivate(DeactivationType type)
        {
            base.DidDeactivate(type);
        }

        private void _customListTableView_didSelectRowEvent(TableView arg1, int arg2)
        {
            DidSelectRowEvent?.Invoke(arg1, arg2);
        }
        
        public virtual float CellSize()
        {
            return 8.5f;
        }

        public virtual int NumberOfCells()
        {
            return Data.Count;
        }

        /// <summary>
        /// Instantiates a new LevelListTableCell (or recycles an old one).
        /// </summary>
        /// <param name="row">This param does nothing, and was left in by mistake.</param>
        /// <param name="beatmapCharacteristicImages">If set to false, the BeatmapCharacteristicImages will be destroyed.</param>
        /// <returns></returns>
        [Obsolete("Use the GetTableCell function with no row param")]
        public LevelListTableCell GetTableCell(int row, bool beatmapCharacteristicImages = false)
        {
            return GetTableCell(beatmapCharacteristicImages);
        }

        /// <summary>
        /// Instantiates a new LevelListTableCell (or recycles an old one).
        /// </summary>
        /// <param name="beatmapCharacteristicImages">If set to false, the BeatmapCharacteristicImages will be destroyed.</param>
        /// <returns></returns>
        public LevelListTableCell GetTableCell(bool beatmapCharacteristicImages = false)
        {
            LevelListTableCell _tableCell = (LevelListTableCell)_customListTableView.DequeueReusableCellForIdentifier(reuseIdentifier);
            if (!_tableCell)
                _tableCell = Instantiate(_songListTableCellInstance);

            if (!beatmapCharacteristicImages)
            {
                foreach (UnityEngine.UI.Image i in _tableCell.GetPrivateField<UnityEngine.UI.Image[]>("_beatmapCharacteristicImages"))
                    i.enabled = false;
            }
            _tableCell.SetPrivateField("_beatmapCharacteristicAlphas", new float[0]);
            _tableCell.SetPrivateField("_beatmapCharacteristicImages", new UnityEngine.UI.Image[0]);
            _tableCell.reuseIdentifier = reuseIdentifier;
            return _tableCell;
        }

        public virtual TableCell CellForIdx(int idx)
        {
            LevelListTableCell _tableCell = GetTableCell();

            _tableCell.SetText(Data[idx].text);
            _tableCell.SetSubText(Data[idx].subtext);
            _tableCell.SetIcon(Data[idx].icon == null ? UIUtilities.BlankSprite : Data[idx].icon);

            return _tableCell;
        }
    }
}
