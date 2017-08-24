using Microsoft.VisualStudio.Text.Editor;

using System;

namespace ResXTweaks
{
    internal struct CommandContext
    {
        public IServiceProvider ServiceProvider;
        public IWpfTextView WpfTextView;
    }
}
