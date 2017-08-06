using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using System;

namespace ResXTweaks
{
    internal class CommandFilter : IOleCommandTarget
    {
        private readonly CommandContext _context;
        private IOleCommandTarget _nextCommandTarget;

        public CommandFilter(CommandContext context)
        {
            _context = context;
        }

        public void SetNextCommandTarget(IOleCommandTarget nextCommandTarget)
        {
            _nextCommandTarget = nextCommandTarget;
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _nextCommandTarget?.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText) ?? VSConstants.S_OK;
        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == Constants.VS2017CmdSet && nCmdID == Constants.GotoImplementationCmdId)
            {
                var caretPosition = _context.WpfTextView.Caret.Position.BufferPosition;
                var lyzer = new CodeAnalyzer(_context.ServiceProvider);

                var result = lyzer.IsResourceDesignerProperty(caretPosition).Result;
                if (result.result)
                    return lyzer.FindResourceReferences(result.document, result.designerProperty);
            }
            return _nextCommandTarget?.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut) ?? VSConstants.S_OK;
        }
    }
}
