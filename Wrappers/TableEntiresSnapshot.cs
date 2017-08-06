using Microsoft.VisualStudio.Shell.TableManager;

using System.Collections.Generic;

namespace ResXTweaks
{
    internal class TableEntriesSnapshot<T> : ITableEntriesSnapshot where T : TableEntry
    {
        private readonly int _version;
        private readonly IReadOnlyList<T> _entries;

        public int Count => _entries.Count;
        public int VersionNumber => _version;

        public TableEntriesSnapshot(IReadOnlyList<T> entries, int version)
        {
            _version = version;
            _entries = entries;
        }

        public int IndexOf(int currentIndex, ITableEntriesSnapshot newSnapshot)
        {
            return currentIndex;
        }

        public bool TryGetValue(int index, string keyName, out object content)
        {
            return _entries[index].GetFieldValue(keyName, out content);
        }

        public void StartCaching() {}
        public void StopCaching() {}
        public void Dispose() {}
    }
}
