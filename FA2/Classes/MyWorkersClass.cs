using System;
using System.Data;
using System.Linq;
using System.Windows;
using FAIIControlLibrary;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class MyWorkersClass
    {
        private readonly string _connectionString;

        private MySqlDataAdapter _myWorkersDataAdapter;
        public DataTable MyWorkersDataTable;

        private MySqlDataAdapter _myWorkersGroupsDataAdapter;
        public DataTable MyWorkersGroupsDataTable;

        private MySqlDataAdapter _mainWorkersDataAdapter;
        public DataTable MainWorkersDataTable;


        public MyWorkersClass()
        {
            _connectionString = App.ConnectionInfo.ConnectionString;
            Initialize(); 
        }

        private void Initialize()
        {
            Create();
            Fill();
        }

        private void Create()
        {
            MyWorkersDataTable = new DataTable();
            MyWorkersGroupsDataTable = new DataTable();

            MainWorkersDataTable = new DataTable();
        }

        private void Fill()
        {
            FillMyWorkers();

            FillMyWorkersGroups();


            FillMainWorkers();
        }


        private void FillMyWorkers()
        {
            try
            {
                if (MyWorkersDataTable != null) MyWorkersDataTable.Clear();

                _myWorkersDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT MyWorkerID, MainWorkerID, MyWorkersGroupID, WorkerID, " +
                        "WorkerProfessionID, IsEnable  FROM FAIIStaff.MyWorkers WHERE IsEnable= True",
                        _connectionString);
                new MySqlCommandBuilder(_myWorkersDataAdapter);
                _myWorkersDataAdapter.Fill(MyWorkersDataTable);

            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [MWC0001]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillMyWorkersGroups()
        {
            try
            {

                if (MyWorkersGroupsDataTable != null) MyWorkersGroupsDataTable.Clear();

                _myWorkersGroupsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT MyWorkersGroupID, MainWorkerID, MyWorkerGroupName, IsEnable FROM FAIIStaff.MyWorkersGroups WHERE IsEnable = True",
                        _connectionString);
                new MySqlCommandBuilder(_myWorkersGroupsDataAdapter);
                _myWorkersGroupsDataAdapter.Fill(MyWorkersGroupsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [MWC0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void FillMainWorkers()
        {
            try
            {
                if (MainWorkersDataTable != null) MainWorkersDataTable.Clear();

                _mainWorkersDataAdapter =
                    new MySqlDataAdapter(
                        @"SELECT COUNT(MainWorkerID) AS CountM, MainWorkerID
                          FROM FAIIStaff.MyWorkers
                          WHERE (IsEnable = True)
                          GROUP BY MainWorkerID
                          ORDER BY CountM DESC",
                        _connectionString);
                new MySqlCommandBuilder(_mainWorkersDataAdapter);
                _mainWorkersDataAdapter.Fill(MainWorkersDataTable);

            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [MWC0003]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public DataView GetMyWorkers()
        {
            return MyWorkersDataTable.AsDataView();
        }

        public DataView GetMyWorkersGroups()
        {
            return MyWorkersGroupsDataTable.AsDataView();
        }

        public DataView GetMainWorkers()
        {
            return MainWorkersDataTable.AsDataView();
        }

        public void AddMyWorker(int workerID, int mainWorkerID, int myWorkersGroupID, int workerProfessionID)
        {
            DataRow dr = MyWorkersDataTable.NewRow();

            dr["WorkerID"] = workerID;
            dr["MainWorkerID"] = mainWorkerID;
            dr["MyWorkersGroupID"] = myWorkersGroupID;
            dr["WorkerProfessionID"] = workerProfessionID;
            dr["IsEnable"] = true;

            MyWorkersDataTable.Rows.Add(dr);


        }

        public void SaveMyWorkers()
        {
            _myWorkersDataAdapter.Update(MyWorkersDataTable);

            FillMyWorkers();
        }

        public void AddMyWorkersGroup(int mainWorkerID, string myWorkerGroupName)
        {
            DataRow dr = MyWorkersGroupsDataTable.NewRow();

            dr["MainWorkerID"] = mainWorkerID;
            dr["MyWorkerGroupName"] = myWorkerGroupName;
            dr["IsEnable"] = true;

            MyWorkersGroupsDataTable.Rows.Add(dr);

            SaveMyWorkersGroups();

            MyWorkersGroupsDataTable.Clear();

            FillMyWorkersGroups();
        }

        public void SaveMyWorkersGroups()
        {
            _myWorkersGroupsDataAdapter.Update(MyWorkersGroupsDataTable);
        }

        #region using
        public bool HaveMyWorkers(int mainWorkerID)
        {
            var result = false;

            var custView = new DataView(MyWorkersDataTable, "", "MainWorkerID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(mainWorkerID);

            if (foundRows.Count() != 0)
            {
                result = true;
            }

            custView.Dispose();
            
            return result;
        }





        #endregion
    }
}
