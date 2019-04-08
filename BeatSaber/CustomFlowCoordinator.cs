using CustomUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRUI;

namespace CustomUI.BeatSaber
{
    public class CustomFlowCoordinator : FlowCoordinator
    {
        /// <summary>
        /// The FlowCoordinator that presented this FlowCoordinator
        /// </summary>
        public FlowCoordinator parentFlowCoordinator;

        /// <summary>
        /// The CustomMenu this FlowCoordinator is representing.
        /// </summary>
        public CustomMenu customPanel;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            // Set the panel title
            title = customPanel.title;

            // Setup all our back button callbacks
            if (customPanel.mainViewController) customPanel.mainViewController.backButtonPressed += Dismiss;
            if (customPanel.leftViewController) customPanel.leftViewController.backButtonPressed += Dismiss;
            if (customPanel.rightViewController) customPanel.rightViewController.backButtonPressed += Dismiss;
            if (customPanel.bottomViewController) customPanel.bottomViewController.backButtonPressed += Dismiss;

            // Provide all our viewcontrollers as initial viewcontrollers when our FlowCoordinator is added to the scene hierarchy
            if (activationType == FlowCoordinator.ActivationType.AddedToHierarchy)
                ProvideInitialViewControllers(customPanel.mainViewController, customPanel.leftViewController, customPanel.rightViewController, customPanel.bottomViewController);
        }

        protected override void DidDeactivate(DeactivationType deactivationType)
        {
        }

        /// <summary>
        /// Back out to the previous flow coordinator.
        /// </summary>
        /// <param name="immediately">If set to true, no animation will be shown and the transition will be instant.</param>
        public void Dismiss(bool immediately)
        {
            if(this.isActivated)
                parentFlowCoordinator.InvokePrivateMethod("DismissFlowCoordinator", new object[] { this, null, immediately });
        }

        /// <summary>
        /// Back out to the previous flow coordinator with an animation.
        /// </summary>
        public void Dismiss()
        {
            Dismiss(false);
        }
    }
}
