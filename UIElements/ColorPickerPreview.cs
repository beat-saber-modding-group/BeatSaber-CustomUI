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
    public class ColorPickerPreview : Selectable, IEventSystemHandler
    {
        public HMUI.Image ImagePreview;

        private new void Awake()
        {
            base.Awake();
            ImagePreview = gameObject.AddComponent<HMUI.Image>();
            if (ImagePreview != null)
            {
                ImagePreview.material = Instantiate(UIUtilities.NoGlowMaterial);
                ImagePreview.sprite = UIUtilities.RoundedRectangle;
            }
        }
    }
}
