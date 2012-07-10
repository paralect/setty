using System.Runtime.InteropServices;
using SettyVsAddin;

namespace Setty.VsAddin
{
	/// <summary>
    /// This class implements the tool window SettyConfigurationToolWindow exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("d63f139c-580a-4bc8-96f9-5da3bd97ea04")]
    public class SettyConfigurationToolWindow : SettyConfigurationToolWindowBase
    {

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public SettyConfigurationToolWindow()
        {
            base.Content = new SettyConfigurationControl();
        }

	}
}