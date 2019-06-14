using CustomUI.BeatSaber;
using CustomUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUI.UIElements
{
    public class ColorPicker : Selectable, IEventSystemHandler
    {
        public static ColorPickerPreview ColorPickerPreview;
        public ColorPickerCore ColorPickerCore;
        private CustomSlider _sliderR, _sliderG, _sliderB, _sliderA;
        private Button _okButton, _cancelButton;
        private Color _originalColor, _currentColor;

        public void Initialize(CustomMenu customMenu, Color color)
        {
            RectTransform colorContainer = new GameObject("ColorPickerContainer", typeof(RectTransform)).transform as RectTransform;
            colorContainer.SetParent(customMenu.mainViewController.rectTransform, false);
            colorContainer.sizeDelta = new Vector2(60f, 0f);
            //ColorPickerPreview initialization
            ColorPickerPreview = new GameObject("ColorPickerPreview").AddComponent<ColorPickerPreview>();
            if (ColorPickerPreview != null)
            {
                ColorPickerPreview.transform.SetParent(colorContainer, false);
                (ColorPickerPreview.transform as RectTransform).sizeDelta = new Vector2(8.5f, 8.5f);
                (ColorPickerPreview.transform as RectTransform).localPosition = new Vector3(55f, -20, 0);
                TextMeshProUGUI previewText = BeatSaberUI.CreateText(colorContainer, "Preview", new Vector2(0, 0), new Vector2(0, 0));
                previewText.enableWordWrapping = false;
                previewText.rectTransform.position = ColorPickerPreview.transform.TransformPoint(new Vector3(-previewText.preferredWidth * 1.5f - 2f, 2.5f, 0));
            }
            //ColorPickerCore initialization
            ColorPickerCore = new GameObject("ColorPickerCore").AddComponent<ColorPickerCore>();
            if (ColorPickerCore != null)
            {
                ColorPickerCore.ColorPickerPreview = ColorPickerPreview;
                ColorPickerCore.Initialize(SetPreviewColor);
                ColorPickerCore.transform.SetParent(transform, false);
                (ColorPickerCore.transform as RectTransform).sizeDelta = new Vector2(60, 60);
                (ColorPickerCore.transform as RectTransform).localPosition = new Vector3(-50f, 15f);
            }

            var previewImg = ColorPickerPreview.ImagePreview;
            _sliderR = BeatSaberUI.CreateUISlider(transform, 0, 1000, 1, true, (val) =>
            {
                previewImg.color = new Color(val / 255f, _currentColor.g, _currentColor.b, _currentColor.a);
                _currentColor = previewImg.color;
            });
            (_sliderR.Scrollbar.transform as RectTransform).sizeDelta = new Vector2(100f, 8f);
            (_sliderR.Scrollbar.transform as RectTransform).localPosition = new Vector3(70f, 28, 0);
            (_sliderR.Scrollbar.transform as RectTransform).anchoredPosition = new Vector2(60f, 35f);
            TextMeshProUGUI rText = BeatSaberUI.CreateText(colorContainer, "R", new Vector2(0, 0), new Vector2(0, 0));
            rText.rectTransform.position = _sliderR.Scrollbar.transform.TransformPoint(new Vector3(-55, 3.74f, 0));
            _sliderG = BeatSaberUI.CreateUISlider(transform, 0, 1000, 1, true, (val) =>
            {
                previewImg.color = new Color(_currentColor.r, val / 255f, _currentColor.b, _currentColor.a);
                _currentColor = previewImg.color;
            });
            (_sliderG.Scrollbar.transform as RectTransform).sizeDelta = new Vector2(100f, 8f);
            (_sliderG.Scrollbar.transform as RectTransform).localPosition = new Vector3(70f, 20, 0);
            (_sliderG.Scrollbar.transform as RectTransform).anchoredPosition = new Vector2(60f, 24f);
            TextMeshProUGUI gText = BeatSaberUI.CreateText(colorContainer, "G", new Vector2(0, 0), new Vector2(0, 0));
            gText.rectTransform.position = _sliderG.Scrollbar.transform.TransformPoint(new Vector3(-55, 3.74f, 0));

            _sliderB = BeatSaberUI.CreateUISlider(transform, 0, 1000, 1, true, (val) =>
            {
                previewImg.color = new Color(_currentColor.r, _currentColor.g, val / 255f, _currentColor.a);
                _currentColor = previewImg.color;
            }); 
            (_sliderB.Scrollbar.transform as RectTransform).sizeDelta = new Vector2(100f, 8f);
            (_sliderB.Scrollbar.transform as RectTransform).localPosition = new Vector3(70f, 12, 0);
            (_sliderB.Scrollbar.transform as RectTransform).anchoredPosition = new Vector2(60f, 12f);
            TextMeshProUGUI bText = BeatSaberUI.CreateText(colorContainer, "B", new Vector2(0, 0), new Vector2(0, 0));
            bText.rectTransform.position = _sliderB.Scrollbar.transform.TransformPoint(new Vector3(-55, 3.74f, 0));
            _sliderA = BeatSaberUI.CreateUISlider(transform, 0, 255, 1, true, (val) =>
            {
                previewImg.color = new Color(_currentColor.r, _currentColor.g, _currentColor.b, val / 255f);
                _currentColor = previewImg.color;
            });
            (_sliderA.Scrollbar.transform as RectTransform).sizeDelta = new Vector2(100f, 8f);
            (_sliderA.Scrollbar.transform as RectTransform).localPosition = new Vector3(70f, 4, 0);
            (_sliderA.Scrollbar.transform as RectTransform).anchoredPosition = new Vector2(60f, 0f);
            TextMeshProUGUI aText = BeatSaberUI.CreateText(colorContainer, "A", new Vector2(0, 0), new Vector2(0, 0));
            aText.rectTransform.position = _sliderA.Scrollbar.transform.TransformPoint(new Vector3(-55, 3.74f, 0));
       //     if (MenuButton.MenuButtonUI.Instance.bottomPanel == null) MenuButton.MenuButtonUI.Instance.bottomPanel = GameObject.Find("MainMenuViewController/BottomPanel")?.transform as RectTransform;
       //     if (MenuButton.MenuButtonUI.Instance.menuButtonsOriginal == null) MenuButton.MenuButtonUI.Instance.menuButtonsOriginal = MenuButton.MenuButtonUI.Instance.bottomPanel.Find("Buttons") as RectTransform;

            /*
            var rowTransform = Instantiate(MenuButton.MenuButtonUI.Instance.menuButtonsOriginal, colorContainer);
            rowTransform.anchorMin = Vector2.zero;
            rowTransform.anchorMax = Vector2.one;
            rowTransform.anchoredPosition = new Vector2(0f, -30f);
            rowTransform.sizeDelta = new Vector2(0f, 10f);
            Destroy(rowTransform.GetComponent<StartMiddleEndButtonsGroup>());
            Console.WriteLine("1");
            foreach (Transform child in rowTransform)
            {
                child.name = string.Empty;
                Destroy(child.gameObject);
            }
            */
            // Ok button
            _okButton = BeatSaberUI.CreateUIButton(colorContainer, "CreditsButton", new Vector2(-25f,-30f), new Vector2(30f, 8f));
            //Instantiate(Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "CreditsButton")), colorContainer, false);
            _okButton.ToggleWordWrapping(false);
            _okButton.SetButtonText("Ok");
            _okButton.onClick.RemoveAllListeners();
            _okButton.onClick.AddListener(delegate ()
            {
                customMenu.Dismiss();
            });
            _okButton.GetComponent<StartMiddleEndButtonBackgroundController>().SetMiddleSprite();
            // Cancel button
            _cancelButton = BeatSaberUI.CreateUIButton(colorContainer, "CreditsButton", new Vector2(25f, -30f), new Vector2(30f, 8f));
                //Instantiate(Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == "CreditsButton")), colorContainer, false);
            _cancelButton.ToggleWordWrapping(false);
            _cancelButton.SetButtonText("Cancel");
            _cancelButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.AddListener(delegate ()
            {
                SetPreviewColor(_originalColor);
                customMenu.Dismiss();
            });
            _cancelButton.GetComponent<StartMiddleEndButtonBackgroundController>().SetMiddleSprite();
            SetPreviewColor(color);
        }
        
        public void DidActivate(Color color)
        {
            _originalColor = color;
        }

        /// <summary>
        /// Sets the <see cref="ColorPickerPreview"/> color while also updating all associated <see cref="CustomSlider"/> components.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to set the preview image</param>
        public void SetPreviewColor(Color color)
        {
            _currentColor = color;
            ColorPickerPreview.ImagePreview.color = color;
            _sliderR.CurrentValue = color.r * 255f;
            _sliderR.Scrollbar.Set(_sliderR.GetPercentageFromCurrentValue());
            _sliderG.CurrentValue = color.g * 255f;
            _sliderG.Scrollbar.Set(_sliderG.GetPercentageFromCurrentValue());
            _sliderB.CurrentValue = color.b * 255f;
            _sliderB.Scrollbar.Set(_sliderB.GetPercentageFromCurrentValue());
            _sliderA.CurrentValue = color.a * 255f;
            _sliderA.Scrollbar.Set(_sliderA.GetPercentageFromCurrentValue());
        }

        /// <summary>
        /// Get the color of a sprite contained in an <see cref="HMUI.Image"/> on pointer click
        /// </summary>
        /// <param name="pointerData">The <see cref="PointerEventData"/> given by OnPointerDown</param>
        /// <param name="image">The <see cref="HMUI.Image"/> instance</param>
        public static Color GetSelectedColorFromImage(PointerEventData pointerData, HMUI.Image image)
        {
            RectTransform rectTransform = image.transform as RectTransform;
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerData.position, pointerData.pressEventCamera, out localCursor))
                return (new Color(0, 0, 0, 0));
            localCursor.x += Math.Abs(rectTransform.rect.x);
            localCursor.y += Math.Abs(rectTransform.rect.y);
            localCursor.x *= (image.sprite.texture.width / rectTransform.rect.width);
            localCursor.y *= (image.sprite.texture.height / rectTransform.rect.height);
            if (localCursor.x < 0 || localCursor.y < 0)
                return (new Color(0, 0, 0, 0));

            return (image.sprite.texture.GetPixel((int)(localCursor.x), (int)(localCursor.y)));
        }
    }
}