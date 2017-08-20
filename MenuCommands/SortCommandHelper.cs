using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ResXTweaks
{
    internal static class SortCommandHelper
    {
        public static SortCommandState GetCommandState(IVsMonitorSelection monitorSelection)
        {
            IntPtr hierarchyPointer, selectionContainerPointer;
            IVsMultiItemSelect multiItemSelect;
            uint itemId;

            monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                 out itemId,
                                                 out multiItemSelect,
                                                 out selectionContainerPointer);

            var selectedHierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPointer, typeof(IVsHierarchy)) as IVsHierarchy;
            Marshal.Release(hierarchyPointer);

            object selectedObject;
            selectedHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out selectedObject);

            var projectItems = new List<ProjectItem>();
            var activeDocumentProjectItem = selectedObject as ProjectItem;

            // Context menu invoked on the active document or on an item in Solution Explorer
            if (activeDocumentProjectItem != null)
                projectItems.Add(activeDocumentProjectItem);

            // Context menu was invoked on multiple selected items in Solution Explorer
            else if (multiItemSelect != null)
            {
                uint itemCount;
                int isSingleHierarchy;

                multiItemSelect.GetSelectionInfo(out itemCount, out isSingleHierarchy);
                var selectedItems = new VSITEMSELECTION[itemCount];
                multiItemSelect.GetSelectedItems(0, itemCount, selectedItems);

                foreach (var item in selectedItems)
                {
                    selectedHierarchy.GetProperty(item.itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out selectedObject);

                    var projectItem = selectedObject as ProjectItem;
                    if (projectItem != null)
                        projectItems.Add(projectItem);
                }
            }

            return new SortCommandState
            {
                IsVisible = (projectItems.Count > 0) && projectItems.TrueForAll(DocumentHelper.IsResXProjectItem),
                IsEnabled = (projectItems.Count > 0) && projectItems.TrueForAll(item => item.Document?.ReadOnly != true),
                SelectedFiles = projectItems.Select(item => item.FileNames[0]).ToArray()
            };
        }

        public static void SortResXFiles(IServiceProvider serviceProvider, string[] resxFiles, bool sortRelated)
        {
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            var filesToSort = (sortRelated == false) ? resxFiles : resxFiles
                .SelectMany(path => DocumentHelper.GetRelatedResXFiles(path))
                .ToArray();

            foreach (string resxFile in filesToSort)
            {
                var document = DocumentHelper.OpenResXFile(dte, resxFile);

                if (document.ReadOnly == false && DocumentHelper.IsResXDocument(document))
                {
                    if (document.Saved || document.Save() == vsSaveStatus.vsSaveSucceeded)
                        System.Threading.Tasks.Task.Run(() => SortResourceEntries(serviceProvider, document));
                }
            }
        }

        public static void SortResourceEntries(IServiceProvider serviceProvider, Document document)
        {
            var textDocument = (TextDocument)document.Object("TextDocument");

            EditPoint startPoint = textDocument.StartPoint.CreateEditPoint();
            EditPoint endPoint = textDocument.EndPoint.CreateEditPoint();

            string content = startPoint.GetText(textDocument.EndPoint);
            try
            {
                content = ResXHelper.SortResX(content);
                startPoint.ReplaceText(endPoint, content, 0);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                ErrorHandler.ShowErrorMessage(serviceProvider, "See log for details..");
            }
        }
    }
}
