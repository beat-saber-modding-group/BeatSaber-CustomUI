using CustomUI.BeatSaber;
using CustomUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUI.UIElements
{
    public class ColorPickerCore : Selectable, IEventSystemHandler
    {
        public ColorPickerPreview ColorPickerPreview;

        private HMUI.Image _Image;
        private PointerEventData _PointerData;
        private Action<Color> SetPreviewColor;

        /// <summary>
        /// Initialize the <see cref="ColorPickerCore"/> (should be called after assigning the <see cref="ColorPickerPreview"/> variable)
        /// </summary>
        public void Initialize(Action<Color> SetPreviewColor)
        {
            this.SetPreviewColor = SetPreviewColor;
            _Image = gameObject.AddComponent<HMUI.Image>();
            if (_Image != null)
            {
                _Image.material = Instantiate(UIUtilities.NoGlowMaterial);
                _Image.sprite = UIUtilities.ColorPickerBase;
                _Image.sprite.texture.wrapMode = TextureWrapMode.Clamp;
                _Image.material.SetTexture("_MainTex", _Image.sprite.texture);
            }
        }

        private void Update()
        {
            if (_PointerData != null)
                SetPreviewColor?.Invoke(ColorPicker.GetSelectedColorFromImage(_PointerData, _Image));
        }

        /// <summary>
        /// Method called when the pointer is clicked inside the <see cref="ColorPickerCore"/>
        /// </summary>
        /// <param name="eventData">Some informations about the pointer</param>
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            _PointerData = eventData;
        }

        /// <summary>
        /// Method called when the pointer is released inside the <see cref="ColorPickerCore"/>
        /// </summary>
        /// <param name="eventData">Some informations about the pointer</param>
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            _PointerData = null;
        }

        /// <summary>
        /// Method called when the pointer is exiting the <see cref="ColorPickerCore"/>
        /// </summary>
        /// <param name="eventData">Some informations about the pointer</param>
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            _PointerData = null;
        }
    }
}
