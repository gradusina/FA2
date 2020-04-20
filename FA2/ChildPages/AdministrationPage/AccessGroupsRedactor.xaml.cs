using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary;

namespace FA2.ChildPages.AdministrationPage
{
    /// <summary>
    /// Логика взаимодействия для AccessGroupsRedactor.xaml
    /// </summary>
    public partial class AccessGroupsRedactor
    {
        private readonly AdministrationClass _admc;

        public AccessGroupsRedactor()
        {
            InitializeComponent();
            App.BaseClass.GetAdministrationClass(ref _admc);
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            AccessGroupViewListBox.ItemsSource = _admc.AccessGroupsView;
        }

        private void OnSaveChangesButtonClick(object sender, RoutedEventArgs e)
        {
            if (AccessGroupViewListBox.SelectedItem == null) return;

            if (string.IsNullOrEmpty(AccessGroupNameTextBox.Text))
            {
                return;
            }

            var moduleAccess = AccessGroupAvailablesItemsControl.ItemsSource as DataView;
            if (moduleAccess == null) return;

            var accessGroupName = AccessGroupNameTextBox.Text;
            var accessGroupId = Convert.ToInt32(AccessGroupViewListBox.SelectedValue);

            var groups =
                _admc.AccessGroupsTable.Select(string.Format("AccessGroupName = '{0}' AND AccessGroupID <> {1}",
                    accessGroupName, accessGroupId));
            if (groups.Length != 0)
            {
                MetroMessageBox.Show("Группа с таким названием уже существует!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get all workers in access group
            var workers = (from structure in
                _admc.AccessGroupStructureTable.AsEnumerable()
                    .Where(s => s.Field<Int64>("AccessGroupID") == accessGroupId)
                select structure.Field<Int64>("WorkerID")).Distinct();

            // Get base available modules for access group
            var availableModules =
                _admc.AvailableModulesTable.AsEnumerable().Where(r => r.Field<Int64>("AccessGroupID") == accessGroupId);

            var workersList = workers as IList<long> ?? workers.ToList();
            if (availableModules.Any())
            {
                foreach (var row in availableModules.CopyToDataTable().AsEnumerable())
                {
                    var moduleId = Convert.ToInt32(row["ModuleID"]);

                    // Delete all non access modules
                    if (moduleAccess.Table.AsEnumerable()
                        .Any(r => !r.Field<bool>("Access") && r.Field<Int64>("ModuleID") == moduleId))
                    {
                        _admc.DeleteAvailableModule(accessGroupId, moduleId);

                        // Delete available for workers in access group
                        foreach (var worker in workersList)
                        {
                            _admc.DeleteWorkerAccess((int) worker, moduleId);
                        }
                    }
                    else
                    {
                        if (moduleAccess.Table.AsEnumerable().All(r => r.Field<Int64>("ModuleID") != moduleId))
                            continue;

                        var baseFullAccess = Convert.ToBoolean(row["FullAccess"]);
                        var fullAccess =
                            moduleAccess.Table.AsEnumerable()
                                .First(r => r.Field<Int64>("ModuleID") == moduleId)
                                .Field<bool>("FullAccess");

                        // Set full access if old and new values are different
                        if (baseFullAccess != fullAccess)
                        {
                            _admc.SetModuleFullAccessForAccessGroup(accessGroupId, moduleId, fullAccess);
                            // Set full access for workers in access group
                            foreach (var worker in workersList)
                            {
                                _admc.SetFullAccessForWorker((int) worker, moduleId, fullAccess);
                            }
                        }
                    }
                }
            }

            // Add all new available modules
            foreach (var row in moduleAccess.Table.AsEnumerable()
                .Where(r =>
                    r.Field<bool>("Access") &&
                    availableModules.AsEnumerable().All(a => a.Field<Int64>("ModuleID") != r.Field<Int64>("ModuleID"))))
            {
                var moduleId = Convert.ToInt32(row["ModuleID"]);
                var fullAccess = Convert.ToBoolean(row["FullAccess"]);
                _admc.AddNewAvailableModule(accessGroupId, moduleId, fullAccess);

                // Add available for workers in access group
                foreach (var worker in workersList)
                {
                    _admc.AddWorkerAccess((int)worker, moduleId, fullAccess);
                }
            }

            _admc.RefillAvailableModules();
            _admc.RefillWorkerAccess();

            var checkSum = CalculateCheckSum();
            _admc.SetCheckSum(accessGroupId, checkSum);
            _admc.ChangeAccessGroupName(accessGroupId, accessGroupName);

            AdministrationClass.AddNewAction(102);
            OnCancelAddAccessGroupButtonClick(null, null);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnAddAccessGroupButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnimation.Completed += (s, args) =>
                                          {
                                              AccessGroupRedactorGrid.DataContext = null;
                                              AccessGroupViewGrid.Visibility = Visibility.Hidden;
                                              AccessGroupRedactorGrid.Visibility = Visibility.Visible;
                                              SaveChangesButton.Visibility = Visibility.Hidden;
                                              AddNewAccessGroupButton.Visibility = Visibility.Visible;

                                              opacityAnimation = new DoubleAnimation(1,
                                                  new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                                              OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
                                          };

            // Fill empty access group
            var view = GetModulesAccessForAccessGroup(-1).AsDataView();
            view.Sort = "Access DESC, ModuleName";
            AccessGroupAvailablesItemsControl.ItemsSource = view;

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void OnCancelAddAccessGroupButtonClick(object sender, RoutedEventArgs e)
        {
            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnimation.Completed += (s, args) =>
            {
                AccessGroupViewGrid.Visibility = Visibility.Visible;
                AccessGroupRedactorGrid.Visibility = Visibility.Hidden;
                AccessGroupAvailablesItemsControl.ItemsSource = null;
                AccessGroupNameTextBox.IsEmphasized = false;

                opacityAnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
            };

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void OnAccessGroupItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnEditAccessGroupButtonClick(null, null);
        }

        private void OnEditAccessGroupButtonClick(object sender, RoutedEventArgs e)
        {
            if (AccessGroupViewListBox.SelectedItem == null) return;
            var accessGroupId = Convert.ToInt32(AccessGroupViewListBox.SelectedValue);

            var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnimation.Completed += (s, args) =>
                                          {
                                              AccessGroupViewGrid.Visibility = Visibility.Hidden;
                                              AccessGroupRedactorGrid.Visibility = Visibility.Visible;
                                              SaveChangesButton.Visibility = Visibility.Visible;
                                              AddNewAccessGroupButton.Visibility = Visibility.Hidden;

                                              opacityAnimation = new DoubleAnimation(1,
                                                  new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                                              OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
                                          };

            // Fill selected access group
            AccessGroupRedactorGrid.DataContext = null;
            AccessGroupRedactorGrid.DataContext = AccessGroupViewListBox.SelectedItem;
            var view = GetModulesAccessForAccessGroup(accessGroupId).AsDataView();
            view.Sort = "Access DESC, ModuleName";
            AccessGroupAvailablesItemsControl.ItemsSource = view;

            OpacityGrid.BeginAnimation(OpacityProperty, opacityAnimation);
        }

        private void OnAddNewAccessGroupButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AccessGroupNameTextBox.Text))
            {
                return;
            }

            var moduleAccess = AccessGroupAvailablesItemsControl.ItemsSource as DataView;
            if (moduleAccess == null) return;

            var accessGroupName = AccessGroupNameTextBox.Text;

            var groups = _admc.AccessGroupsTable.Select(string.Format("AccessGroupName = '{0}'", accessGroupName));
            if (groups.Length != 0)
            {
                MetroMessageBox.Show("Группа с таким названием уже существует!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // List of checked tiles
            var moduleIds = new List<int>();
            var fullAccessList = new List<bool>();
            foreach (
                var availableModule in
                    moduleAccess.Cast<DataRowView>().Where(availableModule => (bool) availableModule["Access"]))
            {
                var moduleId = Convert.ToInt32(availableModule["ModuleID"]);
                var fullAccess = Convert.ToBoolean(availableModule["FullAccess"]);

                moduleIds.Add(moduleId);
                fullAccessList.Add(fullAccess);
            }

            _admc.AddNewAccessGroup(accessGroupName, moduleIds.ToArray(), fullAccessList.ToArray());

            AdministrationClass.AddNewAction(100);
            OnCancelAddAccessGroupButtonClick(null, null);
        }

        private void OnDeleteAccessGroupButtonClick(object sender, RoutedEventArgs e)
        {
            if (AccessGroupViewListBox.SelectedItem == null) return;

            if (MetroMessageBox.Show("Вы действительно хотите удалить данную группу?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            var accessGroupId = Convert.ToInt32(AccessGroupViewListBox.SelectedValue);
            _admc.DeleteAccessGroup(accessGroupId);
            AdministrationClass.AddNewAction(101);
        }


        private int CalculateCheckSum()
        {
            var checkSum = 0;
            var workerAccess = AccessGroupAvailablesItemsControl.ItemsSource as DataView;
            if (workerAccess == null) return 0;

            foreach (var row in workerAccess.Table.AsEnumerable().Where(r => r.Field<bool>("Access")))
            {
                var moduleId = Convert.ToInt32(row["ModuleID"]);
                checkSum += moduleId * 10;
                var fullAccess = Convert.ToBoolean(row["FullAccess"]);
                if (fullAccess)
                    checkSum += 1;
            }

            return checkSum;
        }

        private DataTable GetModulesAccessForAccessGroup(int accessGroupId)
        {
            //Result table
            var table = new DataTable();
            table.Columns.Add("ModuleID", typeof (Int64));
            table.Columns.Add("ModuleName", typeof (string));
            table.Columns.Add("ModuleDescription", typeof (string));
            table.Columns.Add("ModuleIcon", typeof (byte[]));
            table.Columns.Add("ModuleColor", typeof (string));
            table.Columns.Add("FullAccess", typeof (bool));
            table.Columns.Add("Access", typeof (bool));

            var availables = _admc.AvailableModulesTable.AsEnumerable().
                Where(r => r.Field<Int64>("AccessGroupID") == accessGroupId);
            var modules = _admc.ModulesTable.AsEnumerable().
                Where(m => m.Field<bool>("IsEnabled") && m.Field<bool>("ShowInFileStorage"));

            foreach (var module in modules)
            {
                var availableModule =
                    availables.FirstOrDefault(a => a.Field<Int64>("ModuleID") == Convert.ToInt64(module["ModuleID"]));

                var newRow = table.NewRow();
                newRow["ModuleID"] = module["ModuleID"];
                newRow["ModuleName"] = module["ModuleName"];
                newRow["ModuleDescription"] = module["ModuleDescription"];
                newRow["ModuleIcon"] = module["ModuleIcon"];
                newRow["ModuleColor"] = module["ModuleColor"];
                if (availableModule != null)
                {
                    newRow["FullAccess"] = availableModule["FullAccess"];
                    newRow["Access"] = true;
                }
                else
                {
                    newRow["FullAccess"] = false;
                    newRow["Access"] = false;
                }

                table.Rows.Add(newRow);
            }

            return table;
        }
    }
}
