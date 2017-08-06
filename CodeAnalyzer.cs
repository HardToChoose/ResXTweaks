using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.FindAllReferences;
using Microsoft.VisualStudio.Text;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResXTweaks
{
    internal class CodeAnalyzer
    {
        private readonly IServiceProvider _serviceProvider;

        public CodeAnalyzer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<(bool result, Document document, ISymbol designerProperty)> IsResourceDesignerProperty(SnapshotPoint caretPosition)
        {
            var document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document.SupportsSemanticModel)
            {
                var semanticModel = await document.GetSemanticModelAsync();
                var symbol = await SymbolFinder.FindSymbolAtPositionAsync(semanticModel, caretPosition.Position, document.Project.Solution.Workspace);

                if (symbol.Kind == SymbolKind.Property && IsDesignerType(symbol.ContainingType))
                    return (true, document, symbol);
            }
            return (false, null, null);
        }

        public int FindResourceReferences(Document document, ISymbol designerProperty)
        {
            var findAllReferencesService = (IFindAllReferencesService)_serviceProvider.GetService(typeof(SVsFindAllReferences));

            var windowTitle = $"'{designerProperty.Name}' resource definitions";
            var referencesWindow = findAllReferencesService.StartSearch(windowTitle);

            var dataSource = new ResourceSearchResultsDataSource(document, designerProperty, referencesWindow);
            dataSource.LoadData(CancellationToken.None);

            return VSConstants.S_OK;
        }

        private bool IsDesignerType(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol != null &&
                   typeSymbol.BaseType.SpecialType == SpecialType.System_Object &&
                   typeSymbol.Locations.FirstOrDefault()?.SourceTree?.FilePath?.EndsWith(".Designer.cs") == true;
        }
    }
}
