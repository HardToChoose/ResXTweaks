using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

using System.IO;

namespace ResXTweaks
{
    internal class ResourceSearchResult : TableEntry
    {
        public string Definition { get; set; }
        public string Code { get; set; }

        public string FilePath { get; }
        public string FileDir { get; }

        public int Column { get; set; }
        public int Line { get; set; }
        public string Project { get; set; }

        public ResourceSearchResult(string filePath)
        {
            FilePath = filePath;
            FileDir = Path.GetDirectoryName(filePath);
        }

        public override bool GetFieldValue(string fieldName, out object value)
        {
            value = null;
            switch (fieldName)
            {
                case StandardTableKeyNames.Text:
                    value = Code;
                    break;

                case StandardTableKeyNames.DocumentName:
                    value = FilePath;
                    break;

                case StandardTableKeyNames.Column:
                    value = Column;
                    break;

                case StandardTableKeyNames.Line:
                    value = Line;
                    break;

                case StandardTableKeyNames.ProjectName:
                    value = Project;
                    break;

                case StandardTableKeyNames2.Definition:
                    value = Definition;
                    break;

                case StandardTableKeyNames2.Path:
                    value = FileDir;
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}
