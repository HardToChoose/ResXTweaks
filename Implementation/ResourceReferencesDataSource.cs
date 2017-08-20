using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell.FindAllReferences;
using Microsoft.VisualStudio.Shell.TableManager;

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace ResXTweaks
{
    internal class ResourceSearchResultsDataSource : TableDataSource<ResourceSearchResult>
    {
        private readonly Document _document;
        private readonly ISymbol _designerPropertySymbol;

        public ResourceSearchResultsDataSource(Document document, ISymbol designerPropertySymbol, IFindAllReferencesWindow referencesWindow)
            : base(referencesWindow.Title, referencesWindow)
        {
            _document = document;
            _designerPropertySymbol = designerPropertySymbol;

            GroupByFields(StandardTableKeyNames.ProjectName);
        }

        protected override void InternalLoadData(IProducerConsumerCollection<ResourceSearchResult> result, CancellationToken cancellationToken)
        {
            result.TryAdd(CreateFromSymbol(_designerPropertySymbol));

            string resourceName = _designerPropertySymbol.Name;
            INamedTypeSymbol designerClass = _designerPropertySymbol.ContainingType;

            SyntaxTree syntaxTree = designerClass.Locations.First().SourceTree;
            string filePath = syntaxTree?.FilePath;

            foreach (string resx in DocumentHelper.GetRelatedResXFiles(filePath, designerClass.Name))
            {
                var dataElement = ResXHelper.FindResourceEntryElement(resx, resourceName);
                var lineInfo = dataElement as IXmlLineInfo;

                if (lineInfo != null && lineInfo.HasLineInfo())
                    result.TryAdd(CreateFromXmlElement(resx, dataElement));
            }
        }

        private ResourceSearchResult CreateFromSymbol(ISymbol symbol)
        {
            Location location = symbol.Locations.First();
            FileLinePositionSpan lineLocation = location.GetLineSpan();
            SourceText sourceText = location.SourceTree.GetText();

            int start = sourceText.Lines[lineLocation.StartLinePosition.Line].Start;
            int end = sourceText.Lines[lineLocation.EndLinePosition.Line].End;
            string code = sourceText.GetSubText(TextSpan.FromBounds(start, end)).ToString();

            return new ResourceSearchResult(location.SourceTree.FilePath)
            {
                Code = TrimWhiteCharacters(code),
                Column = lineLocation.StartLinePosition.Character,
                Line = lineLocation.StartLinePosition.Line,
                Project = _document.Project.Name
            };
        }

        private ResourceSearchResult CreateFromXmlElement<TElement>(string xmlFilePath, TElement element)
            where TElement : XElement, IXmlLineInfo
        {
            return new ResourceSearchResult(xmlFilePath)
            {
                Definition = TrimWhiteCharacters(element.ToString()),
                Code = TrimWhiteCharacters(element.Value),
                Column = element.LinePosition,
                Line = element.LineNumber,
                Project = _document.Project.Name
            };
        }

        private static string TrimWhiteCharacters(string str)
        {
            return str?.Trim(' ', '\t', '\r', '\n');
        }
    }
}
