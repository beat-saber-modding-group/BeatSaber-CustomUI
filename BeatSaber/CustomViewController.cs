using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VRUI;

namespace CustomUI.BeatSaber
{
    public class CustomViewController : VRUIViewController
    {
        /// <summary>
        /// The event that's fired when the back button is pressed.
        /// </summary>
        public Action backButtonPressed;

        /// <summary>
        /// A reference to the current back button, if it exists.
        /// </summary>
        public Button _backButton;

        /// <summary>
        /// When set to true, a back button will be automatically generated.
        /// </summary>
        public bool includeBackButton;

        /// <summary>
        /// The event that's fired when the CustomViewController is activated (when you open it).
        /// </summary>
        public Action<bool, VRUIViewController.ActivationType> DidActivateEvent;

        /// <summary>
        /// The event that's fired when the CustomViewController is deactivated (when you close it).
        /// </summary>
        public Action<VRUIViewController.DeactivationType> DidDeactivateEvent;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (firstActivation)
            {
                if (includeBackButton && _backButton == null)
                {
                    _backButton = BeatSaberUI.CreateBackButton(rectTransform as RectTransform);
                    _backButton.onClick.AddListener(delegate ()
                    {
                        backButtonPressed?.Invoke();
                    });
                }
            }

            DidActivateEvent?.Invoke(firstActivation, type);
        }

        protected override void DidDeactivate(DeactivationType type)
        {
            DidDeactivateEvent?.Invoke(type);
        }

        /// <summary>
        /// Clears any back button callbacks.
        /// </summary>
        public void ClearBackButtonCallbacks()
        {
            backButtonPressed = null;
        }
    }

    public class CustomCellInfo
    {
        public string text;
        public string subtext;
        public Sprite icon;

        public CustomCellInfo(string text, string subtext, Sprite icon = null)
        {
            this.text = text;
            this.subtext = subtext;
            this.icon = icon;
        }
    };
}
