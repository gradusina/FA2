using System.Windows;
using System.Windows.Controls;
using FA2.Classes;
using System.Data;
using FA2.XamlFiles;
using System.Linq;
using System;

namespace FA2.ChildPages.ProdRoomsPage
{
    /// <summary>
    /// Логика взаимодействия для RaportPage.xaml
    /// </summary>
    public partial class RaportPage : Page
    {
        private ProdRoomsClass _prodRoomsClass;
        private int _actionStatusId;

        public RaportPage(int actionStatusId)
        {
            InitializeComponent();

            _actionStatusId = actionStatusId;
            App.BaseClass.GetProdRoomsClass(ref _prodRoomsClass);
            SetBindings(actionStatusId);
        }

        private void SetBindings(int actionStatusId)
        {
            var newTable = _prodRoomsClass.Actions.Table.Copy();
            var column = new DataColumn("IsDoneAction", typeof(bool)) { DefaultValue = false };
            newTable.Columns.Add(column);

            var view = newTable.AsDataView();
            view.RowFilter = string.Format("Visible = 'True' AND ActionStatus = {0}", actionStatusId);
            view.Sort = "ActionNumber";

            ActionsItemsControl.ItemsSource = view;
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void OnSaveRaportButtonClick(object sender, RoutedEventArgs e)
        {
            var actionsView = ActionsItemsControl.ItemsSource as DataView;
            if (actionsView == null) return;

            if (actionsView.Table.AsEnumerable().All(r => !r.Field<bool>("IsDoneAction")))
                return;

            var workerId = AdministrationClass.CurrentWorkerId;
            var currentDate = App.BaseClass.GetDateFromSqlServer();
            var addInfo = AdditionalInfoTextBox.Text.Trim();

            foreach (DataRowView actionView in actionsView)
            {
                var actionId = Convert.ToInt32(actionView["ActionID"]);
                var actionStatusId = Convert.ToInt32(actionView["ActionStatus"]);
                var isDoneAction = Convert.ToBoolean(actionView["IsDoneAction"]);
                _prodRoomsClass.AddWorkerReport(workerId, actionId, actionStatusId, currentDate, addInfo, isDoneAction);
            }

            if (_actionStatusId == 1)
                AdministrationClass.AddNewAction(74);
            else
                AdministrationClass.AddNewAction(73);

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                var mainFrame = mainWindow.MainFrame;
                var prodRoomsPage = mainFrame.Content as XamlFiles.ProdRoomsPage;
                if (prodRoomsPage != null)
                    prodRoomsPage.SetRaportComboBoxItemsAvailability();
            }

            OnCancelButtonClick(null, null);
        }
    }
}
