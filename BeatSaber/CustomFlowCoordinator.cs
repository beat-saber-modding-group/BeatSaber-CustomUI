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
        public FlowCoordinator parentFlowCoordinator;
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

            // Provide all our viewcontrollers as initial viewcontrollers when our FlowCoordinator is added to the sene hierarchy
            if (activationType == FlowCoordinator.ActivationType.AddedToHierarchy)
                ProvideInitialViewControllers(customPanel.mainViewController, customPanel.leftViewController, customPanel.rightViewController, customPanel.bottomViewController);
        }

        protected override void DidDeactivate(DeactivationType deactivationType)
        {
        }

        public void Dismiss(bool immediately)
        {
            if(this.isActivated)
                parentFlowCoordinator.InvokePrivateMethod("DismissFlowCoordinator", new object[] { this, null, immediately });
        }

        public void Dismiss()
        {
            Dismiss(false);
        }
    }
}
