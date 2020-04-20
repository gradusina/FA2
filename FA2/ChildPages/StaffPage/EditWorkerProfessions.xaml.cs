using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;

namespace FA2.ChildPages.StaffPage
{
    /// <summary>
    /// Логика взаимодействия для EditWorkerProfessions.xaml
    /// </summary>
    public partial class EditWorkerProfessions : Page
    {
        private StaffClass _sc;
        private int _workerId;

        public EditWorkerProfessions(int workerId)
        {
            InitializeComponent();
            App.BaseClass.GetStaffClass(ref _sc);
            _workerId = workerId;
            BindingData();
        }

        private void BindingData()
        {
            WorkerProfCategoryCombobox.ItemsSource = _sc.GetTariffRates();

            BindingWorkerProfFactoryComboBox();
            BindingWorkerProfComboBox();
            BindingWorkerProfGroupComboBox();
            var workerProfessionsView = new BindingListCollectionView(_sc.GetWorkerProfessions());
            workerProfessionsView.CustomFilter = string.Format("WorkerID = {0}", _workerId);
            workerProfessionsView.GroupDescriptions.Add(new PropertyGroupDescription("FactoryID"));
            WorkerProfessionsListBox.ItemsSource = workerProfessionsView;


            BindingAdditWorkerProfComboBox();
            BindingAdditWorkerGroupComboBox();

            var additionalWorkerProfessionsView = _sc.GetAdditionalWorkerProfessions();
            additionalWorkerProfessionsView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            AdditionalWorkerProfessionsListBox.ItemsSource = additionalWorkerProfessionsView;
            if (AdditionalWorkerProfessionsListBox.HasItems)
                AdditionalWorkerProfessionsListBox.SelectedIndex = 0;
        }

        private void BindingWorkerProfFactoryComboBox()
        {
            WorkerProfFactoryComboBox.ItemsSource = _sc.GetFactories();
            WorkerProfFactoryComboBox.DisplayMemberPath = "FactoryName";
            WorkerProfFactoryComboBox.SelectedValuePath = "FactoryID";
        }

        private void BindingWorkerProfComboBox()
        {
            WorkerProfComboBox.ItemsSource = _sc.GetProfessions();
            WorkerProfComboBox.DisplayMemberPath = "ProfessionName";
            WorkerProfComboBox.SelectedValuePath = "ProfessionID";
            if (WorkerProfComboBox.Items.Count != 0)
                WorkerProfComboBox.SelectedIndex = 0;
        }

        private void BindingWorkerProfGroupComboBox()
        {
            WorkerProfGroupComboBox.ItemsSource = _sc.GetWorkerGroups();
            WorkerProfGroupComboBox.DisplayMemberPath = "WorkerGroupName";
            WorkerProfGroupComboBox.SelectedValuePath = "WorkerGroupID";
            if (WorkerProfGroupComboBox.Items.Count != 0)
                WorkerProfGroupComboBox.SelectedIndex = 0;
        }

        private void BindingAdditWorkerProfComboBox()
        {
            AdditionalWorkerProfComboBox.ItemsSource = _sc.GetProfessions();
            AdditionalWorkerProfComboBox.DisplayMemberPath = "ProfessionName";
            AdditionalWorkerProfComboBox.SelectedValuePath = "ProfessionID";
        }

        private void BindingAdditWorkerGroupComboBox()
        {
            AdditionalWorkerProfGroupComboBox.ItemsSource = _sc.GetWorkerGroups();
            AdditionalWorkerProfGroupComboBox.DisplayMemberPath = "WorkerGroupName";
            AdditionalWorkerProfGroupComboBox.SelectedValuePath = "WorkerGroupID";
        }

        private void CancelAddProfButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }


        #region WorkerProfessions

        private void WorkerProfGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox) sender).SelectedValue == null) return;

            int workerGroupId = Convert.ToInt32(((ComboBox) sender).SelectedValue);
            WorkerProfComboBox.ItemsSource = ProfessionsGroupFilter(workerGroupId);
            if (WorkerProfComboBox.HasItems)
                WorkerProfComboBox.SelectedIndex = 0;
        }

        private DataView ProfessionsGroupFilter(int groupId)
        {
            var professionsByGroup =
                (_sc.ProfessionsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkerGroupID") == groupId && r.Field<Boolean>("Enable")));

            if (professionsByGroup.Count() != 0)
            {
                DataTable wNamesDt = professionsByGroup.CopyToDataTable();
                return wNamesDt.DefaultView;
            }
            return null;
        }

        private void AddWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkerProfFactoryComboBox.SelectedValue == null || WorkerProfGroupComboBox.SelectedValue == null ||
                WorkerProfComboBox.SelectedValue == null || WorkerProfRateControl.Value == null || 
                WorkerProfCategoryCombobox.SelectedIndex == 0) return;

            int factoryId = Convert.ToInt32(WorkerProfFactoryComboBox.SelectedValue);
            int workerGroupId = Convert.ToInt32(WorkerProfGroupComboBox.SelectedValue);
            int professionId = Convert.ToInt32(WorkerProfComboBox.SelectedValue);
            decimal rate = Convert.ToDecimal(WorkerProfRateControl.Value);
            var category = WorkerProfCategoryCombobox.SelectedValue.ToString();


            long basesSalary = Convert.ToInt64(BasesSalaryControl.Value);
            decimal perIncByContract = Convert.ToDecimal(perIncreasingSalaryByContractControl.Value);
            long incByContract = Convert.ToInt64(IncreasingSalaryByContractControl.Value);
            decimal perIncByPost = Convert.ToDecimal(perIncreasingSalaryByPostControl.Value);
            long incByPost = Convert.ToInt64(IncreasingSalaryByPostControl.Value);
            decimal perIncByOther = Convert.ToDecimal(perIncreasingSalaryByOtherControl.Value);
            long incByOther = Convert.ToInt64(IncreasingSalaryByOtherControl.Value);
            long additWages = Convert.ToInt64(AdditionalWagesContractControl.Value);
            long permWages = Convert.ToInt64(PermanentPartWagesControl.Value);



            _sc.AddNewWorkerProfession(_workerId, factoryId, workerGroupId, professionId, rate, category, basesSalary,
                perIncByContract, incByContract, perIncByPost, incByPost, perIncByOther, incByOther, additWages,
                permWages);
            HideSecondWorkerProfButton_Click(null, null);
        }

        private void ShowSecondWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    AddProfProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    WorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                WorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                AddProfProcedure();
            }
        }

        private void AddProfProcedure()
        {
            WorkerProfViewGrid.Visibility = Visibility.Hidden;
            WorkerProfRedactorGrid.Visibility = Visibility.Visible;
            AddWorkerProfButton.Visibility = Visibility.Visible;
            ChangeWorkerProfButton.Visibility = Visibility.Hidden;
            //WorkerProfessionsFocus.Visibility = Visibility.Visible;
            //NewProfessionTextBlock.Visibility = Visibility.Visible;

            if (WorkerProfFactoryComboBox.HasItems) WorkerProfFactoryComboBox.SelectedIndex = 0;
            if (WorkerProfGroupComboBox.HasItems) WorkerProfGroupComboBox.SelectedIndex = 0;
            if (WorkerProfComboBox.HasItems) WorkerProfComboBox.SelectedIndex = 0;
            WorkerProfRateControl.Value = decimal.Zero;
            WorkerProfCategoryCombobox.SelectedIndex = 0;
        }

        private void HideSecondWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    CancelProfProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    WorkerProfOpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
                };

                WorkerProfOpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            }
            else
            {
                CancelProfProcedure();
            }

            WorkerProfessionsListBox.IsEnabled = true;
        }

        private void CancelProfProcedure()
        {
            WorkerProfViewGrid.Visibility = Visibility.Visible;
            WorkerProfRedactorGrid.Visibility = Visibility.Hidden;
            WorkerProfessionsListBox.IsEnabled = true;
            if (WorkerProfessionsListBox.HasItems)
                WorkerProfessionsListBox.SelectedIndex = 0;
        }

        private void ChangeWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            var workerProfession = WorkerProfessionsListBox.SelectedItem as DataRowView;
            if (workerProfession == null) return;

            if (WorkerProfFactoryComboBox.SelectedValue == null || WorkerProfGroupComboBox.SelectedValue == null ||
                WorkerProfComboBox.SelectedValue == null || WorkerProfRateControl.Value == null ||
                WorkerProfCategoryCombobox.SelectedIndex == -1) return;

            var workerProfessionId = Convert.ToInt64(workerProfession["WorkerProfessionID"]);
            int factoryId = Convert.ToInt32(WorkerProfFactoryComboBox.SelectedValue);
            int workerGroupId = Convert.ToInt32(WorkerProfGroupComboBox.SelectedValue);
            int professionId = Convert.ToInt32(WorkerProfComboBox.SelectedValue);
            decimal rate = Convert.ToDecimal(WorkerProfRateControl.Value);
            var category = WorkerProfCategoryCombobox.SelectedValue.ToString();

            long basesSalary = Convert.ToInt64(BasesSalaryControl.Value);
            decimal perIncByContract = Convert.ToDecimal(perIncreasingSalaryByContractControl.Value);
            long incByContract = Convert.ToInt64(IncreasingSalaryByContractControl.Value);
            decimal perIncByPost = Convert.ToDecimal(perIncreasingSalaryByPostControl.Value);
            long incByPost = Convert.ToInt64(IncreasingSalaryByPostControl.Value);
            decimal perIncByOther = Convert.ToDecimal(perIncreasingSalaryByOtherControl.Value);
            long incByOther = Convert.ToInt64(IncreasingSalaryByOtherControl.Value);
            long additWages = Convert.ToInt64(AdditionalWagesContractControl.Value);
            long permWages = Convert.ToInt64(PermanentPartWagesControl.Value);

            _sc.ChangeWorkerProfession(workerProfessionId, factoryId, workerGroupId, professionId, rate, category,
                basesSalary, perIncByContract, incByContract, perIncByPost, incByPost, perIncByOther, incByOther, additWages, permWages);
            
            HideSecondWorkerProfButton_Click(null, null);
        }

        private void DeleteWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            var workerProfession = WorkerProfessionsListBox.SelectedItem as DataRowView;
            if (workerProfession == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранную профессию?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var workerProfessionId = Convert.ToInt64(workerProfession["WorkerProfessionID"]);
            _sc.DeleteWorkerProfession(workerProfessionId);
        }

        private void EditWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkerProfessionsListBox.SelectedItem == null) return;

            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    EditProfProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    WorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                WorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                EditProfProcedure();
            }

            WorkerProfessionsListBox.IsEnabled = false;
        }

        private void EditProfProcedure()
        {
            WorkerProfViewGrid.Visibility = Visibility.Hidden;
            WorkerProfRedactorGrid.Visibility = Visibility.Visible;
            AddWorkerProfButton.Visibility = Visibility.Hidden;
            ChangeWorkerProfButton.Visibility = Visibility.Visible;
            WorkerProfGrid.DataContext = WorkerProfessionsListBox.SelectedItem;

            var drv = (DataRowView) WorkerProfessionsListBox.SelectedItem;

            WorkerProfGroupComboBox.SelectionChanged -= WorkerProfGroupComboBox_SelectionChanged;
            WorkerProfGroupComboBox.SelectedValue = drv["WorkerGroupID"];
            WorkerProfGroupComboBox_SelectionChanged(WorkerProfGroupComboBox, null);

            WorkerProfComboBox.ItemsSource = ProfessionsGroupFilter(Convert.ToInt32(drv["WorkerGroupID"]));
            WorkerProfComboBox.SelectedValue = drv["ProfessionID"];
            WorkerProfGroupComboBox.SelectionChanged += WorkerProfGroupComboBox_SelectionChanged;
            WorkerProfRateControl.Value = Convert.ToDecimal(((DataRowView) drv)["Rate"]);

            decimal category;
            WorkerProfCategoryCombobox.SelectedValue = decimal.TryParse(drv["Category"].ToString(), out category)
                ? category
                : decimal.Zero;

            BasesSalaryControl.Value = drv["BasesSalary"] != DBNull.Value ? Convert.ToDecimal(drv["BasesSalary"]) : 0;

            perIncreasingSalaryByContractControl.Value = drv["perIncByContract"] != DBNull.Value
                ? Convert.ToDecimal(drv["perIncByContract"])
                : 0;

            IncreasingSalaryByContractControl.Value = drv["BasesSalary"] != DBNull.Value
                ? Convert.ToDecimal(drv["IncByContract"])
                : 0;

            perIncreasingSalaryByPostControl.Value = drv["BasesSalary"] != DBNull.Value
                ? Convert.ToDecimal(drv["perIncByPost"])
                : 0;

            IncreasingSalaryByPostControl.Value = drv["BasesSalary"] != DBNull.Value
                ? Convert.ToDecimal(drv["IncByPost"])
                : 0;

            perIncreasingSalaryByOtherControl.Value = drv["BasesSalary"] != DBNull.Value
                ? Convert.ToDecimal(drv["perIncByOther"])
                : 0;

            IncreasingSalaryByOtherControl.Value = drv["BasesSalary"] != DBNull.Value
                ? Convert.ToDecimal(drv["IncByOther"])
                : 0;

            AdditionalWagesContractControl.Value = drv["BasesSalary"] != DBNull.Value
                ? Convert.ToDecimal(drv["AdditWages"])
                : 0;

            PermanentPartWagesControl.Value = drv["BasesSalary"] != DBNull.Value
                ? Convert.ToDecimal(drv["PermWages"])
                : 0;
        }

        private void WorkerProfessionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditWorkerProfButton_Click(null, null);
        }

        #endregion


        #region AdditionalWorkerProfessions

        private void ShowSecondAdditionalWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                AdditionalWorkerProfViewGrid.Visibility = Visibility.Hidden;
                AdditionalWorkerProfRedactor.Visibility = Visibility.Visible;
                //AdditionalWorkerProfessionsFocus.Visibility = Visibility.Visible;
                //NewAdditionalProfessionTextBlock.Visibility = Visibility.Visible;
                AddAdditionalWorkerProfButton.Visibility = Visibility.Visible;
                ChangeAdditionalWorkerProfButton.Visibility = Visibility.Hidden;

                if (AdditionalWorkerProfGroupComboBox.HasItems) AdditionalWorkerProfGroupComboBox.SelectedIndex = 0;
                if (AdditionalWorkerProfComboBox.HasItems) AdditionalWorkerProfComboBox.SelectedIndex = 0;

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                AdditionalWorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            };

            AdditionalWorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
        }

        private void HideSecondAdditionalWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                AdditionalWorkerProfViewGrid.Visibility = Visibility.Visible;
                AdditionalWorkerProfRedactor.Visibility = Visibility.Hidden;
                AdditionalWorkerProfessionsListBox.IsEnabled = true;
                if (AdditionalWorkerProfessionsListBox.HasItems)
                    AdditionalWorkerProfessionsListBox.SelectedIndex = 0;

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                AdditionalWorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            };

            AdditionalWorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);

            AdditionalWorkerProfessionsListBox.IsEnabled = true;
        }

        private void AddAdditionalWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdditionalWorkerProfComboBox.SelectedValue == null) return;

            int professionId = Convert.ToInt32(AdditionalWorkerProfComboBox.SelectedValue);
            var workerAdditProfessions = 
                _sc.AdditionalWorkerProfessionsDataTable.
                    Select(string.Format("WorkerID = {0} AND ProfessionID = {1}", _workerId, professionId));
            if(workerAdditProfessions.Any())
            {
                MessageBox.Show("Невозможно добавить. \nДанная профессия уже существует в качестве дополнительной у этого работника!");
                return;
            }

            _sc.AddNewAdditWorkerProfession(_workerId, professionId);
            HideSecondAdditionalWorkerProfButton_Click(null, null);
        }

        private void ChangeAdditionalWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            var additWorkerProfession = AdditionalWorkerProfessionsListBox.SelectedItem as DataRowView;
            if (additWorkerProfession == null) return;

            var additWorkerProfId = Convert.ToInt64(additWorkerProfession["AdditionalWorkerProfessionID"]);
            var professionId = Convert.ToInt32(AdditionalWorkerProfComboBox.SelectedValue);

            _sc.ChangeWorkerAdditProfession(additWorkerProfId, professionId);

            HideSecondAdditionalWorkerProfButton_Click(null, null);
        }

        private void AdditionalWorkerProfGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox) sender).SelectedValue == null) return;

            int workerGroupId = Convert.ToInt32(((ComboBox) sender).SelectedValue);
            AdditionalWorkerProfComboBox.ItemsSource = ProfessionsGroupFilter(workerGroupId);
            if (AdditionalWorkerProfComboBox.Items.Count != 0)
                AdditionalWorkerProfComboBox.SelectedIndex = 0;
        }

        private void DeleteAdditionalWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            var additWorkerProfession = AdditionalWorkerProfessionsListBox.SelectedItem as DataRowView;
            if (additWorkerProfession == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранную профессию?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var additWorkerProfId = Convert.ToInt64(additWorkerProfession["AdditionalWorkerProfessionID"]);
            _sc.DeleteWorkerAdditProfession(additWorkerProfId);
        }

        private void EditAdditionalWorkerProfButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdditionalWorkerProfessionsListBox.SelectedItem == null) return;

            var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
            opacityAnnimation.Completed += (s, args) =>
            {
                AdditionalWorkerProfViewGrid.Visibility = Visibility.Hidden;
                AdditionalWorkerProfRedactor.Visibility = Visibility.Visible;
                AddAdditionalWorkerProfButton.Visibility = Visibility.Hidden;
                ChangeAdditionalWorkerProfButton.Visibility = Visibility.Visible;

                AdditionalWorkerProfComboBox.ItemsSource = _sc.GetProfessions();

                var drv = (DataRowView) AdditionalWorkerProfessionsListBox.SelectedItem;
                AdditionalWorkerProfComboBox.SelectedValue = drv["ProfessionID"];

                opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                AdditionalWorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            };

            AdditionalWorkerProfOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);

            AdditionalWorkerProfessionsListBox.IsEnabled = false;
        }

        private void AdditionalWorkerProfessionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditAdditionalWorkerProfButton_Click(null, null);
        }

        #endregion


        private void WorkerProfRateControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateBasesSalary();
        }

        private void TariffRankControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateBasesSalary();
        }

        private void TariffRate_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateBasesSalary();
        }

        private void CalculateBasesSalary()
        {
            BasesSalaryControl.Value = Convert.ToDecimal(WorkerProfRateControl.Value)*
                                       Convert.ToDecimal(TariffRankControl.Value)*
                                       Convert.ToDecimal(TariffRate.Value);
        }

        private void perIncreasingSalaryByContractControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateIncreasingSalaryByContract();
            CalculatePermanentPartWages();
        }

        private void CalculateIncreasingSalaryByContract()
        {
            IncreasingSalaryByContractControl.Value = Convert.ToDecimal(BasesSalaryControl.Value)/100*
                                                      Convert.ToDecimal(perIncreasingSalaryByContractControl.Value);
        }

        private void perIncreasingSalaryByPostControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateIncreasingSalaryByPost();
            CalculatePermanentPartWages();
        }

        private void CalculateIncreasingSalaryByPost()
        {
            IncreasingSalaryByPostControl.Value = Convert.ToDecimal(BasesSalaryControl.Value)/100*
                                                  Convert.ToDecimal(perIncreasingSalaryByPostControl.Value);
        }

        private void perIncreasingSalaryByOtherControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateIncreasingSalaryByOther();
            CalculatePermanentPartWages();
        }

        private void CalculateIncreasingSalaryByOther()
        {
            IncreasingSalaryByOtherControl.Value = Convert.ToDecimal(BasesSalaryControl.Value)/100*
                                                   Convert.ToDecimal(perIncreasingSalaryByOtherControl.Value);
        }

        private void BasesSalaryControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculateIncreasingSalaryByContract();
            CalculateIncreasingSalaryByPost();
            CalculateIncreasingSalaryByOther();
            CalculatePermanentPartWages();
        }

        private void AdditionalWagesContractControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            CalculatePermanentPartWages();
        }

        private void CalculatePermanentPartWages()
        {
            PermanentPartWagesControl.Value = Convert.ToDecimal(BasesSalaryControl.Value) +
                                              Convert.ToDecimal(IncreasingSalaryByContractControl.Value) +
                                              Convert.ToDecimal(IncreasingSalaryByPostControl.Value) +
                                              Convert.ToDecimal(IncreasingSalaryByOtherControl.Value) +
                                              Convert.ToDecimal(AdditionalWagesContractControl.Value);
        }

    }
}
