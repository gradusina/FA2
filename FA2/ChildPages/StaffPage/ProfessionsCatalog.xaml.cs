using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary;

namespace FA2.ChildPages.StaffPage
{
    /// <summary>
    /// Логика взаимодействия для ProfessionsCatalog.xaml
    /// </summary>
    public partial class ProfessionsCatalog : Page
    {
        private StaffClass _sc;

        public ProfessionsCatalog()
        {
            InitializeComponent();
            App.BaseClass.GetStaffClass(ref _sc);
            BindingData();
        }

        private void BindingData()
        {
            WorkerGroupsComboBox.DisplayMemberPath = "WorkerGroupName";
            WorkerGroupsComboBox.SelectedValuePath = "WorkerGroupID";
            WorkerGroupsComboBox.ItemsSource = _sc.GetWorkerGroups();

            ProfessionsListBox.DisplayMemberPath = "ProfessionName";
            ProfessionsListBox.SelectedValuePath = "ProfessionID";
            var view = new BindingListCollectionView(_sc.GetProfessions());
            view.GroupDescriptions.Add(new PropertyGroupDescription("WorkerGroupID"));
            view.SortDescriptions.Add(new System.ComponentModel.SortDescription("ProfessionName",
                System.ComponentModel.ListSortDirection.Ascending));
            ProfessionsListBox.ItemsSource = view;
            if (ProfessionsListBox.Items.Count != 0)
                ProfessionsListBox.SelectedIndex = 0;
        }

        // Close catalog
        private void ProfessionsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void ProfessionsAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                var animation = new DoubleAnimation(350, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                animation.Completed += (s, args) =>
                {
                    AddProcedure();

                    opacityAnimation.To = 1;
                    OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
                };

                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
                BeginAnimation(Control.HeightProperty, animation);
            }
            else
            {
                AddProcedure();
            }
        }

        private void AddProcedure()
        {
            ProfessionsViewGrid.Visibility = Visibility.Hidden;
            ProfessionsRedactorGrid.Visibility = Visibility.Visible;
            ProfessionsRedactorGrid.DataContext = null;
            ProfessionsOkButton.Visibility = Visibility.Visible;
            ProfessionsSaveButton.Visibility = Visibility.Hidden;

            if (WorkerGroupsComboBox.Items.Count != 0)
                WorkerGroupsComboBox.SelectedIndex = 0;

            NewProfessionTextBlock.Visibility = Visibility.Visible;
            FocusRectangle.Visibility = Visibility.Visible;
        }

        // Change selected profession
        private void ProfessionsSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProfessionsNameTextBox.Text) || ProfessionsListBox.SelectedItem == null ||
                TRFCTextBox.Value == null ||
                WorkerGroupsComboBox.SelectedItem == null) return;

            int professionId = Convert.ToInt32(ProfessionsListBox.SelectedValue);
            string profName = ProfessionsNameTextBox.Text;
            int workerGroupId = Convert.ToInt32(WorkerGroupsComboBox.SelectedValue);
            long trfc = Convert.ToInt64(TRFCTextBox.Value);

            string filterStr =
                string.Format(
                    "ProfessionName = '{0}' AND WorkerGroupID = {1} AND ProfessionID <> {2} AND Enable = 'True'",
                    profName, workerGroupId, professionId);

            if (!ContainsProfessionName(filterStr))
            {
                _sc.ChangeProfession(professionId, profName, workerGroupId, trfc);
                _sc.ChangeWorkerProfessionsSalaries(professionId, trfc);
                ProfessionsDontAddButton_Click(null, null);
            }
            else
            {
                MetroMessageBox.Show("Данная профессия существует в базе!", "Внимание", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private bool ContainsProfessionName(string filterStr)
        {
            var findRows = _sc.ProfessionsDataTable.Select(filterStr);
            return findRows.Any();
        }

        // Delete selected profession
        private void ProfessionsDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfessionsListBox.SelectedItem == null) return;

            if (MetroMessageBox.Show("Вы действительно хотите удалить выбранную профессию?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var professionId = Convert.ToInt32(ProfessionsListBox.SelectedValue);
                _sc.DeleteProfession(professionId);

                if (ProfessionsListBox.Items.Count != 0)
                    ProfessionsListBox.SelectedIndex = 0;
            }
        }

        // Add new profession
        private void ProfessionsOkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ProfessionsNameTextBox.Text) && WorkerGroupsComboBox.SelectedItem != null && TRFCTextBox.Value != null)
            {
                string professionName = ProfessionsNameTextBox.Text;
                int workerGroupId = Convert.ToInt32(WorkerGroupsComboBox.SelectedValue);
                string filterStr =
                    string.Format("ProfessionName = '{0}' AND WorkerGroupID = {1} AND Enable = 'True'",
                        professionName, workerGroupId);

                long trfc = Convert.ToInt64(TRFCTextBox.Value);

                if (!ContainsProfessionName(filterStr))
                {
                    _sc.AddNewProfession(professionName, workerGroupId, trfc);
                    ProfessionsDontAddButton_Click(null, null);
                }
                else
                {
                    MetroMessageBox.Show("Данная профессия существует в базе!", "Внимание", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }

        // Add cancel
        private void ProfessionsDontAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                var animation = new DoubleAnimation(470, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                animation.Completed += (s, args) =>
                {
                    CancelProcedure();

                    opacityAnimation.To = 1;
                    OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
                };

                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
                BeginAnimation(Control.HeightProperty, animation);
            }
            else
            {
                CancelProcedure();
            }

            ProfessionsListBox.IsEnabled = true;
        }

        private void CancelProcedure()
        {
            ProfessionsViewGrid.Visibility = Visibility.Visible;
            ProfessionsRedactorGrid.Visibility = Visibility.Hidden;

            if (ProfessionsListBox.Items.Count != 0)
                ProfessionsListBox.SelectedIndex = 0;

            NewProfessionTextBlock.Visibility = Visibility.Hidden;
            FocusRectangle.Visibility = Visibility.Hidden;
        }

        private void ProfessionsChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfessionsListBox.SelectedItem == null) return;

            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                var animation = new DoubleAnimation(350, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                animation.Completed += (s, args) =>
                {
                    EditProcedure();

                    opacityAnimation.To = 1;
                    OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
                };

                OpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnimation);
                BeginAnimation(Control.HeightProperty, animation);
            }
            else
            {
                EditProcedure();
            }

            ProfessionsListBox.IsEnabled = false;
        }

        private void EditProcedure()
        {
            ProfessionsViewGrid.Visibility = Visibility.Hidden;
            ProfessionsRedactorGrid.Visibility = Visibility.Visible;
            ProfessionsRedactorGrid.DataContext = ProfessionsListBox.SelectedItem;
            ProfessionsOkButton.Visibility = Visibility.Hidden;
            ProfessionsSaveButton.Visibility = Visibility.Visible;

            NewProfessionTextBlock.Visibility = Visibility.Hidden;
            FocusRectangle.Visibility = Visibility.Hidden;
        }

        private void ProfessionsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ProfessionsChangeButton_Click(null, null);
        }

        private void WorkerGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkerGroupsComboBox.SelectedItem == null ||
                ((DataRowView) WorkerGroupsComboBox.SelectedItem)["TRFC"] == DBNull.Value ||
                ProfessionsRedactorGrid.DataContext != null) return;



            TRFCTextBox.Value = Convert.ToInt32(((DataRowView) WorkerGroupsComboBox.SelectedItem)["TRFC"]);
        }
    }
}
