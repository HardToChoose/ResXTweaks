using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using System.ComponentModel.Composition;

namespace ResXTweaks
{
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Export(typeof(IVsTextViewCreationListener))]
    public class Listener : IVsTextViewCreationListener
    {
        [Import(typeof(IVsEditorAdaptersFactoryService))]
        internal IVsEditorAdaptersFactoryService EditorFactory;

        [Import(typeof(SVsServiceProvider))]
        internal SVsServiceProvider ServiceProvider;

        void IVsTextViewCreationListener.VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var commandContext = new CommandContext()
            {
                ServiceProvider = ServiceProvider,
                WpfTextView = EditorFactory.GetWpfTextView(textViewAdapter)
            };

            IOleCommandTarget nextTarget;
            var filter = new TextViewCommandFilter(commandContext);

            if (textViewAdapter.AddCommandFilter(filter, out nextTarget) == VSConstants.S_OK)
            {
                filter.SetNextCommandTarget(nextTarget);
            }
        }
    }
}
