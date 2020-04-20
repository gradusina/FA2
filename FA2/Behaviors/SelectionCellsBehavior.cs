using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FA2.Behaviors
{
    public class SelectionCellsBehavior : Behavior<DataGrid>
    {
        private object _selectedRow;

        private bool _firstRun = true;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectedCellsChanged += AssociatedObjectSelectedCellsChanged;
            AssociatedObject.Loaded += AssociatedObjectLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectedCellsChanged -= AssociatedObjectSelectedCellsChanged;
            AssociatedObject.Loaded -= AssociatedObjectLoaded;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            if (_firstRun)
            {
                foreach (var column in AssociatedObject.Columns)
                {
                    column.CellStyle = GetNewStyle(column.CellStyle);
                }
                AssociatedObject.CellStyle = GetNewStyle(AssociatedObject.CellStyle);
                _firstRun = false;
            }
        }

        private Style GetNewStyle(Style baseStyle)
        {
            Style newStyle;

            if (baseStyle != null)
            {
                newStyle = new Style(typeof(DataGridCell), baseStyle);
                newStyle.Setters.Add(new EventSetter(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(CellMouseDown)));
            }
            else
            {
                newStyle = new Style(typeof(DataGridCell));
                newStyle.Setters.Add(new EventSetter(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(CellMouseDown)));
            }

            return newStyle;
        }

        private void CellMouseDown(object sender, RoutedEventArgs e)
        {
            var info = new DataGridCellInfo((DataGridCell)sender);
            _selectedRow = info.Item;
        }

        private void AssociatedObjectSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (((DataGrid) sender).SelectedCells.Count != 0)
                foreach (var dataGridCellInfo in ((DataGrid) sender).SelectedCells.Where(r => r.Item != _selectedRow))
                {
                    var f = GetDataGridCell(dataGridCellInfo);
                    f.IsSelected = false;
                }
        }

        public DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            if (cellContent != null)
                return (DataGridCell)cellContent.Parent;

            return null;
        }
    }
}
