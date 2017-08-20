using EnvDTE;
using System.IO;

namespace ResXTweaks
{
    static class DocumentHelper
    {
        public static bool IsResXFile(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() == ".resx";
        }

        public static bool IsResXDocument(Document document)
        {
            return document != null &&
                   document.Language.ToLowerInvariant() == "xml" &&
                   IsResXFile(document.FullName);
        }

        public static bool IsResXProjectItem(ProjectItem projectItem)
        {
            if (projectItem.Document != null)
                return IsResXDocument(projectItem.Document);

            return IsResXFile(projectItem.Name) &&
                   (projectItem.FileCodeModel == null ||
                    projectItem.FileCodeModel.Language.ToLowerInvariant() == "xml");
        }

        public static string[] GetRelatedResXFiles(string filePath, string designerClassName = null)
        {
            if (designerClassName == null)
            {
                var fileName = Path.GetFileName(filePath);
                designerClassName = fileName.Substring(0, fileName.IndexOf('.'));
            }
            return Directory.GetFiles(Path.GetDirectoryName(filePath), $"{designerClassName}.*resx");
        }

        public static Document OpenResXFile(DTE dte, string resxFile)
        {
            if (dte.ItemOperations.IsFileOpen(resxFile, EnvDTE.Constants.vsViewKindTextView))
            {
                for (int k = 1; k <= dte.Documents.Count; k++)
                {
                    var document = dte.Documents.Item(k);
                    if (document.FullName == resxFile)
                        return document;
                }
            }
            return dte.ItemOperations.OpenFile(resxFile, EnvDTE.Constants.vsViewKindTextView).Document;
        }
    }
}
