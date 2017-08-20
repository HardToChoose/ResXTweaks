using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ResXTweaks
{
    static class ResXHelper
    {
        private static readonly XName ResourceEntryTag = XName.Get("data");
        private static readonly XName ResourceEntryNameAttr = XName.Get("name");

        public static XElement FindResourceEntryElement(string resxFile, string resourceName)
        {
            var document = XDocument.Load(resxFile, LoadOptions.SetLineInfo);
            return document.Root
                .Elements(ResourceEntryTag)
                .FirstOrDefault(data => data.Attribute(ResourceEntryNameAttr)?.Value == resourceName);
        }

        /// <summary>
        /// Sort resource file entries preserving formatting
        /// </summary>
        /// <param name="xml">ResX file context</param>
        /// <returns>Sorted ResX file</returns>
        public static string SortResX(string xml)
        {
            var document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            var root = document.Root;
            var nodes = root.Nodes().ToArray();

            var removedNodeChains = new List<XNode[]>(nodes.Length / 2);
            int lastDataElement = -1;

            for (int k = 0; k < nodes.Length; k++)
            {
                var dataElement = nodes[k] as XElement;
                if (dataElement?.Name == ResourceEntryTag)
                {
                    var chain = new List<XNode> { dataElement };
                    FindPrecedingTextNodes(dataElement, chain);

                    chain.Reverse();
                    chain.ForEach(node => node.Remove());

                    removedNodeChains.Add(chain.ToArray());
                    lastDataElement = k;
                }
            }

            if (lastDataElement == -1)
                return xml;

            var trailingTextNodes = nodes.Skip(lastDataElement + 1).ToArray();
            Array.ForEach(trailingTextNodes, node => node.Remove());

            removedNodeChains.Sort(CompareNodeChains);
            removedNodeChains.ForEach(root.Add);
            root.Add(trailingTextNodes);

            return document.ToString(SaveOptions.DisableFormatting);

            #region Local functions

            void FindPrecedingTextNodes(XNode current, List<XNode> result)
            {
                var previousNode = current.PreviousNode;
                var pnt = previousNode.NodeType;

                if (pnt == XmlNodeType.Comment ||
                    pnt == XmlNodeType.Text ||
                    pnt == XmlNodeType.SignificantWhitespace ||
                    pnt == XmlNodeType.Whitespace)
                {
                    result.Add(previousNode);
                    FindPrecedingTextNodes(previousNode, result);
                }
            }

            int CompareNodeChains(XNode[] x, XNode[] y)
            {
                return string.CompareOrdinal(
                    ((XElement)x.Last()).Attribute(ResourceEntryNameAttr).Value,
                    ((XElement)y.Last()).Attribute(ResourceEntryNameAttr).Value
                );
            }

            #endregion Local functions
        }
    }
}
