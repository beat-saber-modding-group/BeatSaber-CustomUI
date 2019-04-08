using CustomUI.BeatSaber;
using CustomUI.Utilities;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRUI;
using Image = UnityEngine.UI.Image;

namespace CustomUI.BeatSaber
{
    public class CustomMenu : MonoBehaviour
    {
        private enum ViewControllerPosition
        {
            Left,
            Right,
            Bottom
        }

        public string title;
        public CustomFlowCoordinator customFlowCoordinator
        {
            get { return _masterFlowCoordinator as CustomFlowCoordinator; }
            private set { _masterFlowCoordinator = value; }
        }
        public CustomViewController mainViewController = null;
        public CustomViewController leftViewController = null;
        public CustomViewController rightViewController = null;
        public CustomViewController bottomViewController = null;

        private FlowCoordinator _masterFlowCoordinator;
        private Action<bool> _dismissCustom = null;
        private Dictionary<ViewControllerPosition, List<VRUIViewController>> _viewControllerStacks = new Dictionary<ViewControllerPosition, List<VRUIViewController>>();


        public void SetMainViewController(CustomViewController viewController, bool includeBackButton, Action<bool, VRUIViewController.ActivationType> DidActivate = null, Action<VRUIViewController.DeactivationType> DidDeactivate = null)
        {
            mainViewController = viewController;
            mainViewController.includeBackButton = includeBackButton;
            if(DidActivate != null)
                mainViewController.DidActivateEvent += DidActivate;
            if(DidDeactivate != null)
                mainViewController.DidDeactivateEvent += DidDeactivate;
        }

        public void SetLeftViewController(CustomViewController viewController, bool includeBackButton, Action<bool, VRUIViewController.ActivationType> DidActivate = null, Action<VRUIViewController.DeactivationType> DidDeactivate = null)
        {
            leftViewController = viewController;
            leftViewController.includeBackButton = includeBackButton;
            if (DidActivate != null)
                leftViewController.DidActivateEvent += DidActivate;
            if (DidDeactivate != null)
                leftViewController.DidDeactivateEvent += DidDeactivate;
        }

        public void SetRightViewController(CustomViewController viewController, bool includeBackButton, Action<bool, VRUIViewController.ActivationType> DidActivate = null, Action<VRUIViewController.DeactivationType> DidDeactivate = null)
        {
            rightViewController = viewController;
            rightViewController.includeBackButton = includeBackButton;
            if (DidActivate != null)
                rightViewController.DidActivateEvent += DidActivate;
            if (DidDeactivate != null)
                rightViewController.DidDeactivateEvent += DidDeactivate;
        }

        public void SetBottomViewController(CustomViewController viewController, bool includeBackButton, Action<bool, VRUIViewController.ActivationType> DidActivate = null, Action<VRUIViewController.DeactivationType> DidDeactivate = null)
        {
            bottomViewController = viewController;
            bottomViewController.includeBackButton = includeBackButton;
            if (DidActivate != null)
                bottomViewController.DidActivateEvent += DidActivate;
            if (DidDeactivate != null)
                bottomViewController.DidDeactivateEvent += DidDeactivate;
        }

        public bool Present(bool immediately = false)
        {
            var _activeFlowCoordinator = GetActiveFlowCoordinator();
            if (_activeFlowCoordinator == null || _activeFlowCoordinator == customFlowCoordinator) return false;

            if (mainViewController != null)
            {
                if (customFlowCoordinator == null)
                {
                    customFlowCoordinator = new GameObject("CustomFlowCoordinator").AddComponent<CustomFlowCoordinator>();
                    DontDestroyOnLoad(customFlowCoordinator.gameObject);
                    customFlowCoordinator.customPanel = this;
                    _dismissCustom = customFlowCoordinator.Dismiss;
                }
                customFlowCoordinator.parentFlowCoordinator = _activeFlowCoordinator;
                ReflectionUtil.InvokePrivateMethod(_activeFlowCoordinator, "PresentFlowCoordinator", new object[] { customFlowCoordinator, null, immediately, false });
            }
            else
            {
                _dismissCustom = null;
                if (leftViewController)
                    SetScreen(_activeFlowCoordinator, leftViewController, _activeFlowCoordinator.leftScreenViewController, ViewControllerPosition.Left, immediately);
                if (rightViewController)
                    SetScreen(_activeFlowCoordinator, rightViewController, _activeFlowCoordinator.rightScreenViewController, ViewControllerPosition.Right, immediately);
                if(bottomViewController)
                    SetScreen(_activeFlowCoordinator, bottomViewController, _activeFlowCoordinator.bottomScreenViewController, ViewControllerPosition.Bottom, immediately);
                _masterFlowCoordinator = _activeFlowCoordinator;
            }
            return true;
        }

        public void Present()
        {
            Present(false);
        }

        public void Dismiss(bool immediately = false)
        {
            _dismissCustom?.Invoke(immediately);
        }
        public void Dismiss()
        {
            Dismiss(false);
        }

        private FlowCoordinator GetActiveFlowCoordinator()
        {
            if (GameScenesManagerSO.IsInTransition) return null;

            FlowCoordinator[] flowCoordinators = Resources.FindObjectsOfTypeAll<FlowCoordinator>();
            foreach (FlowCoordinator f in flowCoordinators)
            {
                if (f.isActivated)
                    return f;
            }
            return null;
        }

        private VRUIViewController PopViewControllerStack(ViewControllerPosition pos)
        {
            VRUIViewController viewController = _viewControllerStacks[pos].Last();
            _viewControllerStacks[pos].Remove(viewController);
            return viewController; 
        }

        private void SetViewController(FlowCoordinator flowCoordinator, VRUIViewController viewController, ViewControllerPosition pos, bool immediate)
        {
            string method = string.Empty;
            switch(pos)
            {
                case ViewControllerPosition.Left:
                    method = "SetLeftScreenViewController";
                    break;
                case ViewControllerPosition.Right:
                    method = "SetRightScreenViewController";
                    break;
                case ViewControllerPosition.Bottom:
                    method = "SetBottomScreenViewController";
                    break;
                default:
                    return;
            }
            flowCoordinator.InvokePrivateMethod(method, new object[] { viewController, immediate });
        }

        private void SetScreen(FlowCoordinator _activeFlowCoordinator, CustomViewController newViewController, VRUIViewController origViewController, ViewControllerPosition pos, bool immediately)
        {
            Action<bool> backAction = (immediate) => SetViewController(_activeFlowCoordinator, PopViewControllerStack(pos), pos, immediate);
            _dismissCustom += backAction;  // custom back button behavior
            if (!newViewController.isActivated)
            {
                if (!_viewControllerStacks.ContainsKey(pos))
                    _viewControllerStacks[pos] = new List<VRUIViewController>();

                _viewControllerStacks[pos].Add(origViewController);
                if (newViewController.includeBackButton)
                {
                    newViewController.ClearBackButtonCallbacks();
                    newViewController.backButtonPressed += () => { backAction.Invoke(false); }; // default back button behavior
                }
                SetViewController(_activeFlowCoordinator, newViewController, pos, immediately);
            }
        }
    }
}
