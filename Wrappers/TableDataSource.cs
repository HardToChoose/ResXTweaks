using Microsoft.VisualStudio.Shell.FindAllReferences;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResXTweaks
{
    internal abstract class TableDataSource<T> : ITableDataSource, ITableEntriesSnapshotFactory where T : TableEntry
    {
        private readonly IFindAllReferencesWindow _referencesWindow;
        protected readonly ConcurrentBag<T> _entries;

        public string SourceTypeIdentifier => typeof(T).FullName;
        public string Identifier { get; }
        public string DisplayName { get; }

        public int CurrentVersionNumber => 0;

        protected abstract void InternalLoadData(IProducerConsumerCollection<T> result, CancellationToken cancellationToken);

        protected TableDataSource(string name, IFindAllReferencesWindow referencesWindow)
        {
            _referencesWindow = referencesWindow;
            DisplayName = name;
            Identifier = string.Format("{0}_{1}", name, typeof(T).FullName);
            _entries = new ConcurrentBag<T>();

            referencesWindow.Manager.AddSource(this);
        }

        public void Dispose()
        {
            _referencesWindow.Manager.RemoveSource(this);
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            sink.AddFactory(this, true);
            sink.IsStable = true;

            return this;
        }

        public ITableEntriesSnapshot GetCurrentSnapshot()
        {
            return new TableEntriesSnapshot<T>(_entries.ToArray(), CurrentVersionNumber);
        }

        public ITableEntriesSnapshot GetSnapshot(int versionNumber)
        {
            return GetCurrentSnapshot();
        }

        public Task LoadData(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    InternalLoadData(_entries, cancellationToken);
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }, cancellationToken);
        }

        protected void GroupByFields(params string[] fieldNames)
        {
            var tableControl = (IWpfTableControl2)_referencesWindow.TableControl;
            var columnStates = tableControl.ColumnStates
                .OfType<ColumnState2>()
                .Select(columnState => new ColumnState2(
                    columnState.Name,
                    columnState.IsVisible,
                    columnState.Width,
                    columnState.SortPriority,
                    columnState.DescendingSort,
                    Array.IndexOf(fieldNames, columnState.Name) == -1 ? 0 : 1
                ))
                .ToList();

            tableControl.SetColumnStates(columnStates);
        }
    }
}
