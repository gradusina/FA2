using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary;

namespace FA2.ChildPages.AdministrationPage
{
    /// <summary>
    /// Логика взаимодействия для ActionsRedactor.xaml
    /// </summary>
    public partial class ActionsRedactor
    {
        private readonly AdministrationClass _ac;

        public ActionsRedactor()
        {
            InitializeComponent();
            App.BaseClass.GetAdministrationClass(ref _ac);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if(_ac.ActionsTypesView == null) return;

            ActionsViewListBox.ItemsSource = GetDetailActionView();
            if (ActionsViewListBox.HasItems)
                ActionsViewListBox.SelectedIndex = 0;

            ModulesComboBox.ItemsSource = _ac.ModulesView;
        }   

        private void EditActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionsViewListBox.SelectedItem == null) return;

            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var animation = new DoubleAnimation(300, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            animation.Completed += (s, args) =>
            {
                ActionsViewGrid.Visibility = Visibility.Hidden;
                ActionsRedactorGrid.Visibility = Visibility.Visible;
                ChangeActionButton.Visibility = Visibility.Visible;
                AddNewActionButton.Visibility = Visibility.Hidden;

                ActionsRedactorGrid.DataContext = null;
                ActionsRedactorGrid.DataContext = ActionsViewListBox.SelectedItem;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, animation);

            ActionsViewListBox.IsEnabled = false;
        }

        private void ActionsViewListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditActionButton_Click(null, null);
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var animation = new DoubleAnimation(300, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            animation.Completed += (s, args) =>
            {
                ActionsViewGrid.Visibility = Visibility.Hidden;
                ActionsRedactorGrid.Visibility = Visibility.Visible;
                ChangeActionButton.Visibility = Visibility.Hidden;
                AddNewActionButton.Visibility = Visibility.Visible;

                ActionsRedactorGrid.DataContext = null;
                if (ModulesComboBox.HasItems) ModulesComboBox.SelectedIndex = 0;

                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
            BeginAnimation(Control.HeightProperty, animation);
        }

        private void DeleteActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionsViewListBox.SelectedItem == null) return;

            if (MetroMessageBox.Show("Вы действительно хотите удалить выбранное действие?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var actionTypeId = Convert.ToInt32(ActionsViewListBox.SelectedValue);
                _ac.DeleteAction(actionTypeId);

                //Refill items source
                ActionsViewListBox.ItemsSource = GetDetailActionView();
                if (ActionsViewListBox.HasItems)
                    ActionsViewListBox.SelectedIndex = 0;
            }
        }

        private void AddNewActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModulesComboBox.SelectedItem == null || string.IsNullOrEmpty(ActionNameTextBox.Text)) return;

            var moduleId = Convert.ToInt32(ModulesComboBox.SelectedValue);
            var actionName = ActionNameTextBox.Text;
            if (_ac.ActionTypesTable.AsEnumerable().Any(r => r.Field<Int64>("ModuleID") == moduleId &&
                r.Field<string>("ActionName") == actionName))
            {
                MetroMessageBox.Show("Данное действие уже присудствует в этом модуле.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _ac.AddNewAction(moduleId, actionName);

            //Refill items source
            ActionsViewListBox.ItemsSource = GetDetailActionView();
            if (ActionsViewListBox.HasItems)
                ActionsViewListBox.SelectedIndex = 0;

            CancelEditActionButton_Click(null, null);
        }

        private void ChangeActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionsViewListBox.SelectedItem == null || ModulesComboBox.SelectedItem == null ||
                string.IsNullOrEmpty(ActionNameTextBox.Text)) return;

            var actionTypeId = Convert.ToInt32(ActionsViewListBox.SelectedValue);
            var moduleId = Convert.ToInt32(ModulesComboBox.SelectedValue);
            var actionName = ActionNameTextBox.Text;
            if (_ac.ActionTypesTable.AsEnumerable().Any(r => r.Field<Int64>("ModuleID") == moduleId &&
                r.Field<string>("ActionName") == actionName))
            {
                MetroMessageBox.Show("Данное действие уже присудствует в этом модуле.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _ac.ChangeAction(actionTypeId, moduleId, actionName);

            //Refill items source
            ActionsViewListBox.ItemsSource = GetDetailActionView();
            if (ActionsViewListBox.HasItems)
                ActionsViewListBox.SelectedIndex = 0;

            CancelEditActionButton_Click(null, null);
        }

        private void CancelEditActionButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            var animation = new DoubleAnimation(450, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            animation.Completed += (s, args) =>
            {
                ActionsViewGrid.Visibility = Visibility.Visible;
                ActionsRedactorGrid.Visibility = Visibility.Hidden;
                ActionNameTextBox.SetCurrentValue(TextBox.TextProperty, string.Empty);
                ActionNameTextBox.IsEmphasized = false;
                
                opacityAnimation.To = 1;
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
            BeginAnimation(HeightProperty, animation);

            ActionsViewListBox.IsEnabled = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void ExportActionsListButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionsViewListBox.HasItems)
                ExportToExcel.GenerateProdScheduleReport((BindingListCollectionView)ActionsViewListBox.ItemsSource);
        }

        private BindingListCollectionView GetDetailActionView()
        {
            var table = new DataTable();
            table.Columns.Add("ActionTypeID", typeof(Int64));
            table.Columns.Add("ModuleID", typeof(Int64));
            table.Columns.Add("ModuleName", typeof(string));
            table.Columns.Add("ActionName", typeof(string));

            var joinTable = _ac.ActionTypesTable.AsEnumerable().Join(_ac.ModulesTable.AsEnumerable(),
                outer => outer["ModuleID"],
                inner => inner["ModuleID"],
                (outer, inner) =>
                {
                    var newRow = table.NewRow();
                    newRow["ActionTypeID"] = outer["ActionTypeID"];
                    newRow["ModuleID"] = outer["ModuleID"];
                    newRow["ModuleName"] = inner["ModuleName"];
                    newRow["ActionName"] = outer["ActionName"];
                    return newRow;
                }).CopyToDataTable();

            var view = new BindingListCollectionView(joinTable.DefaultView);
            view.GroupDescriptions.Add(new PropertyGroupDescription("ModuleID"));
            view.SortDescriptions.Add(new SortDescription("ModuleName", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("ActionName", ListSortDirection.Ascending));

            return view;
        }
    }
}
