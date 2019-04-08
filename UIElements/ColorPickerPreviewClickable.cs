using CustomUI.BeatSaber;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUI.UIElements
{
    public class ColorPickerPreviewClickable : ColorPickerPreview, IEventSystemHandler
    {
        private static CustomMenu _CustomMenu;
        private static CustomViewController _CustomViewController;
        private static ColorPicker _ColorPickerSettings;

        private new void Start()
        {
            base.Start();
            _CustomMenu = BeatSaberUI.CreateCustomMenu<CustomMenu>("Pick a color");
            _CustomViewController = BeatSaberUI.CreateViewController<CustomViewController>();
        }

        private IEnumerator _WaitForPresenting()
        {
            yield return new WaitUntil(() => { return _CustomMenu.Present(false); });
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (_ColorPickerSettings != null)
                _ColorPickerSettings.SetPreviewColor(ImagePreview.color);
            if (_CustomMenu != null && _CustomViewController != null)
            {
                _CustomMenu.SetMainViewController(_CustomViewController, false, (firstActivation, type) =>
                {
                    if (firstActivation && type == VRUI.VRUIViewController.ActivationType.AddedToHierarchy)
                    {
                        _ColorPickerSettings = _CustomViewController.CreateColorPicker(new Vector2(0, -5), new Vector2(0.7f, 0.7f));
                        if (_ColorPickerSettings != null)
                        {
                            _ColorPickerSettings.Initialize(_CustomMenu, ImagePreview.color);
                        }
                    }
                    _ColorPickerSettings.DidActivate(ImagePreview.color);
                });
                _CustomViewController.didDeactivateEvent += _UpdatingPreviewClickableColor;
                StartCoroutine(_WaitForPresenting());
            }
        }

        private void _UpdatingPreviewClickableColor(VRUI.VRUIViewController.DeactivationType deactivationType)
        {
            if (ImagePreview != null && _ColorPickerSettings != null && ColorPicker.ColorPickerPreview != null && ColorPicker.ColorPickerPreview.ImagePreview != null)
                ImagePreview.color = ColorPicker.ColorPickerPreview.ImagePreview.color;
            _CustomViewController.didDeactivateEvent -= _UpdatingPreviewClickableColor;

        }
    }
}
