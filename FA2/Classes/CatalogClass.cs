using System;
using System.Data;
using System.Windows;
using FAIIControlLibrary;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class CatalogClass
    {
        private readonly string _connectionString;

        #region Bindings_Declaration

        private MySqlDataAdapter _workUnitsDataAdapter;
        public DataTable WorkUnitsDataTable;

        private MySqlDataAdapter _workSectionsDataAdapter;
        public DataTable WorkSectionsDataTable;

        private MySqlDataAdapter _workSubsectionsDataAdapter;
        public DataTable WorkSubsectionsDataTable;

        private MySqlDataAdapter _workSubsectionsGroupsDataAdapter;
        public DataTable WorkSubsectionsGroupsDataTable;

        private MySqlDataAdapter _workOperationsDataAdapter;
        public DataTable WorkOperationsDataTable;

        private MySqlDataAdapter _workerGroupsDataAdapter;
        public DataTable WorkerGroupsDataTable;

        private MySqlDataAdapter _workersGroupsTitlesDataAdapter;
        public DataTable WorkersGroupsTitlesDataTable;

        private MySqlDataAdapter _factoriesDataAdapter;
        public DataTable FactoriesDataTable;

        private MySqlDataAdapter _machinesDataAdapter;
        public DataTable MachinesDataTable;

        private MySqlDataAdapter _machinesOperationsDataAdapter;
        public DataTable MachinesOperationsDataTable;

        private MySqlDataAdapter _measureUnitsDataAdapter;
        public DataTable MeasureUnitsDataTable;

        private MySqlDataAdapter _operationGroupsDataAdapter;
        public DataTable OperationGroupsDataTable;
        
        //private MySqlDataAdapter _additionalOperationsDataAdapter;
        //public DataTable AdditionalOperationsDataTable;

        #endregion

        public CatalogClass()
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
            WorkUnitsDataTable = new DataTable();
            WorkSectionsDataTable = new DataTable();
            WorkSubsectionsDataTable = new DataTable();
            WorkSubsectionsGroupsDataTable = new DataTable();
            WorkOperationsDataTable = new DataTable();
            WorkerGroupsDataTable = new DataTable();
            WorkersGroupsTitlesDataTable = new DataTable();
            FactoriesDataTable = new DataTable();

            MachinesDataTable = new DataTable();
            MachinesOperationsDataTable = new DataTable();

            MeasureUnitsDataTable = new DataTable();

            OperationGroupsDataTable = new DataTable();

            //AdditionalOperationsDataTable = new DataTable();
        }

        private void Fill()
        {
            FillWorkUnits();
            FillWorkSections();
            FillWorkSubsections();
            FillWorkSubsectionsGroups();
            FillWorkOperations();
            FillWorkersGroupsTitles();
            FillWorkersGroups();
            FillFactories();
            FillMachines();
            FillMachinesOperations();
            //FillAdditionalOperations();
            FillMeasureUnits();
            FillOperationGroups();
        }

        #region Fill_tables

        private void FillFactories()
        {
            try
            {
                _factoriesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT FactoryID, FactoryName, Visible FROM FAIICatalog.Factories WHERE Visible = True",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_factoriesDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _factoriesDataAdapter.Fill(FactoriesDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0001]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillOperationGroups()
        {
            try
            {
                _operationGroupsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT OperationGroupID, OperationGroupName, Visible, Locked FROM FAIICatalog.OperationGroups WHERE Visible = True",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_operationGroupsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _operationGroupsDataAdapter.Fill(OperationGroupsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0011]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkUnits()
        {
            try
            {
                _workUnitsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkUnitID, FactoryID, WorkerGroupID, WorkUnitName, Visible FROM FAIICatalog.WorkUnits WHERE Visible = True",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_workUnitsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _workUnitsDataAdapter.Fill(WorkUnitsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkersGroups()
        {
            try
            {
                _workerGroupsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkerGroupID, WorkerGroupName, UnitToFactory, Visible FROM FAIIStaff.WorkerGroups WHERE Visible = True",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_workerGroupsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _workerGroupsDataAdapter.Fill(WorkerGroupsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0003]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkersGroupsTitles()
        {
            try
            {
                _workersGroupsTitlesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkersGroupsTitleID, WorkersGroupID, UnitsTitle, SectionsTitle, SubsectionsTitle, OperationsTitle, UnitsPropertiesTitle, SectionsPropertiesTitle, SubsectionsPropertiesTitle, OperationsPropertiesTitle FROM FAIICatalog.WorkersGroupsTitles",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_workersGroupsTitlesDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _workersGroupsTitlesDataAdapter.Fill(WorkersGroupsTitlesDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0004]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkSections()
        {
            try
            {
                _workSectionsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkSectionID, WorkUnitID, WorkSectionName, Visible FROM FAIICatalog.WorkSections  WHERE Visible = True",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_workSectionsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _workSectionsDataAdapter.Fill(WorkSectionsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[CC0005] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkSubsections()
        {
            try
            {
                _workSubsectionsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkSubsectionID, WorkSectionID, SubsectionGroupID, WorkSubsectionName, Visible , HasAdditOperations, OldID FROM FAIICatalog.WorkSubsections",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_workSubsectionsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _workSubsectionsDataAdapter.Fill(WorkSubsectionsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[CC0006] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkSubsectionsGroups()
        {
            try
            {
                _workSubsectionsGroupsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkSubsectionsGroupID, WorkSubsectionsGroupName, ShowAdditProp FROM FAIICatalog.WorkSubsectionsGroups",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_workSubsectionsGroupsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _workSubsectionsGroupsDataAdapter.Fill(WorkSubsectionsGroupsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0007]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkOperations()
        {
            try
            {
              _workOperationsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkOperationID, WorkSubsectionID, WorkSubsectionsGroupID, WorkOperationName, Visible, OldID, OperationTypeID, OperationGroupID FROM FAIICatalog.WorkOperations ORDER BY WorkOperationName",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_workOperationsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _workOperationsDataAdapter.Fill(WorkOperationsDataTable);

            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0008]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillMachines()
        {
            try
            {
                _machinesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT MachineID, WorkSubsectionID, MachineName, MachineSchemeNumber, MachineWorkPlacesCount, Width, Length, FactoryID, IsVisible " +
                        "FROM FAIICatalog.Machines",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_machinesDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _machinesDataAdapter.Fill(MachinesDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0009]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillMachinesOperations()
        {
            try
            {
                _machinesOperationsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT MachinesOperationID, WorkOperationID, OperationCode, WorkerQualificationCode, MinWorkerCategory, OperationPlaceNumber, Productivity, MeasureUnitID, Insalubrity, InsalubrityRate FROM FAIICatalog.MachinesOperations",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_machinesOperationsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _machinesOperationsDataAdapter.Fill(MachinesOperationsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n[CC0010] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void FillAdditionalOperations()
        //{
        //    try
        //    {
        //        _additionalOperationsDataAdapter =
        //            new MySqlDataAdapter(
        //                "SELECT AdditOperationID, AdditOperationName, Visible, OperationTypeID FROM faiicatalog.additionaloperations",
        //                _connectionString);
        //        // ReSharper disable ObjectCreationAsStatement
        //        new MySqlCommandBuilder(_additionalOperationsDataAdapter);
        //        // ReSharper restore ObjectCreationAsStatement
        //        _additionalOperationsDataAdapter.Fill(AdditionalOperationsDataTable);
        //    }
        //    catch (Exception exp)
        //    {
        //        MetroMessageBox.Show(
        //            exp.Message +
        //            "\n\n [CC0011]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
        //            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void FillMeasureUnits()
        {
            try
            {
                _measureUnitsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT MeasureUnitID, MeasureUnitName FROM FAIICatalog.MeasureUnits",
                        _connectionString);
                // ReSharper disable ObjectCreationAsStatement
                new MySqlCommandBuilder(_measureUnitsDataAdapter);
                // ReSharper restore ObjectCreationAsStatement
                _measureUnitsDataAdapter.Fill(MeasureUnitsDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n [CC0012]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Get_DataView

        public DataView GetFactories()
        {
            var dv = FactoriesDataTable.AsDataView();
            dv.RowFilter = "Visible = 'True'";
            return dv;
        }

        public DataView GetWorkUnits()
        {
            var dv = WorkUnitsDataTable.AsDataView();
            dv.RowFilter = "Visible = 'True'";
            return dv;
        }

        public DataView GetWorkSections()
        {
            var dv = WorkSectionsDataTable.AsDataView();
            dv.RowFilter = "Visible = 'True'";
            return dv;
        }

        public DataView GetWorkSubsections()
        {
            var dv = WorkSubsectionsDataTable.AsDataView();
            dv.RowFilter = "Visible = 'True'";
            return dv;
        }

        public DataView GetWorkOperations()
        {
            var dv = WorkOperationsDataTable.AsDataView();
            dv.RowFilter = "Visible = 'True'";
            return dv;
        }

        public DataView GetWorkSubsectionsGroups()
        {
            return WorkSubsectionsGroupsDataTable.AsDataView();
        }

        public DataView GetWorkersGroupsTitles()
        {
            return WorkersGroupsTitlesDataTable.AsDataView();
        }

        public DataView GetWorkersGroups()
        {
            var dv = WorkerGroupsDataTable.AsDataView();
            dv.RowFilter = "Visible = 'True'";
            return dv;
        }

        public DataView GetMachines()
        {
            return MachinesDataTable.AsDataView();
        }

        public DataView GetMachinesOperations()
        {
            return MachinesOperationsDataTable.AsDataView();
        }

        public DataView GetMeasureUnits()
        {
            return MeasureUnitsDataTable.AsDataView();
        }

        public DataView GetOperationGroups()
        {
            return OperationGroupsDataTable.AsDataView();
        }

        //public DataView GetAdditionalOperations()
        //{
        //    var dv = AdditionalOperationsDataTable.AsDataView();
        //    dv.RowFilter = "Visible = 'True'";
        //    return dv;
        //}

        #endregion

        public DataRowView GetTitles(int workersGroupId)
        {
            WorkersGroupsTitlesDataTable.DefaultView.RowFilter = "WorkersGroupID=" + workersGroupId;

            if(WorkersGroupsTitlesDataTable.DefaultView.Count == 0)
                WorkersGroupsTitlesDataTable.DefaultView.RowFilter = null;

            return WorkersGroupsTitlesDataTable.DefaultView[0];
        }

        public DataRowView GetMachineInfo(int workSubsectionId)
        {
            MachinesDataTable.DefaultView.RowFilter = "WorkSubSectionID=" + workSubsectionId;

            if (MachinesDataTable.DefaultView.Count == 0)
                return null;
                //MachinesDataTable.DefaultView.RowFilter = null;

            return MachinesDataTable.DefaultView[0];
        }

        public DataRowView GetMachineOperationInfo(int workOperationId)
        {
            MachinesOperationsDataTable.DefaultView.RowFilter = "WorkOperationID=" + workOperationId;

            if (MachinesOperationsDataTable.DefaultView.Count == 0)
                return null;

            return MachinesOperationsDataTable.DefaultView[0];
        }

        public void AddWorkUnit(int workerGroupId, string workUnitName, int factoryId)
        {
            DataRow dr = WorkUnitsDataTable.NewRow();

            if (factoryId != -1)
                dr["FactoryID"] = factoryId;

            dr["WorkerGroupID"] = workerGroupId;
            dr["WorkUnitName"] = workUnitName;
            dr["Visible"] = true;

            WorkUnitsDataTable.Rows.Add(dr);

            SaveWorkUnits();
            ReFillWorkUnits();
        }

        public void SaveWorkUnits()
        {
            _workUnitsDataAdapter.Update(WorkUnitsDataTable);
        }

        public void ReFillWorkUnits()
        {
            WorkUnitsDataTable.Clear();
            FillWorkUnits();
        }


        public void AddWorkSection(int workUnitID, string workSectionName)
        {
            DataRow dr = WorkSectionsDataTable.NewRow();

            dr["WorkUnitID"] = workUnitID;
            dr["WorkSectionName"] = workSectionName;
            dr["Visible"] = true;

            WorkSectionsDataTable.Rows.Add(dr);

            SaveWorkSections();
            ReFillWorkSections();
        }

        public void SaveWorkSections()
        {
            _workSectionsDataAdapter.Update(WorkSectionsDataTable);
        }

        public void ReFillWorkSections()
        {
            WorkSectionsDataTable.Clear();
            FillWorkSections();
        }


        public void AddWorkSubsection(int workSectionID, string workSubsectionName)
        {
            DataRow dr = WorkSubsectionsDataTable.NewRow();

            dr["WorkSectionID"] = workSectionID;
            dr["SubsectionGroupID"] = 1;
            dr["WorkSubsectionName"] = workSubsectionName;
            dr["Visible"] = true;

            WorkSubsectionsDataTable.Rows.Add(dr);

            _workSubsectionsDataAdapter.Update(WorkSubsectionsDataTable);

            ReFillWorkSubsection();
        }

        public void SaveWorkSubsection()
        {
           _machinesDataAdapter.Update(MachinesDataTable);

           _workSubsectionsDataAdapter.Update(WorkSubsectionsDataTable);
        }

        public void ReFillWorkSubsection()
        {
            WorkSubsectionsDataTable.Clear();
            FillWorkSubsections();
        }

        public void AddWorkOperation(int workSubsectionID,int workSubsectionsGroupID, string workOperationName)
        {
            DataRow dr = WorkOperationsDataTable.NewRow();

            dr["WorkSubsectionID"] = workSubsectionID;
            dr["WorkSubsectionsGroupID"] = workSubsectionsGroupID;
            dr["WorkOperationName"] = workOperationName;
            dr["Visible"] = true;

            WorkOperationsDataTable.Rows.Add(dr);

            _workOperationsDataAdapter.Update(WorkOperationsDataTable);

            ReFillWorkOperation();
        }

        public void SaveWorkOperation()
        {
            _machinesOperationsDataAdapter.Update(MachinesOperationsDataTable);

            _workOperationsDataAdapter.Update(WorkOperationsDataTable);
        }

        public void ReFillWorkOperation()
        {
            WorkOperationsDataTable.Clear();
            FillWorkOperations();
        }

        public void AddAdditOperation(string additOperationName)
        {
            DataRow dr = WorkOperationsDataTable.NewRow();

            dr["WorkSubsectionID"] = -1;
            dr["WorkSubsectionsGroupID"] = -1;
            dr["WorkOperationName"] = additOperationName;
            dr["Visible"] = true;
            dr["OperationTypeID"] = 2;

            
            WorkOperationsDataTable.Rows.Add(dr);

            _workOperationsDataAdapter.Update(WorkOperationsDataTable);

            ReFillWorkOperation();
        }

        public void AddFactory(string factoryName)
        {
            DataRow dr = FactoriesDataTable.NewRow();

            dr["FactoryName"] = factoryName;
            dr["Visible"] = true;

            FactoriesDataTable.Rows.Add(dr);

            _factoriesDataAdapter.Update(FactoriesDataTable);
            RefillFactories();
        }

        public void SaveFactories()
        {
            _factoriesDataAdapter.Update(FactoriesDataTable);
        }

        public void RefillFactories()
        {
            FactoriesDataTable.Clear();
            FillFactories();
        }


        public void AddOperationGroup(string operationGroupName)
        {
            DataRow dr = OperationGroupsDataTable.NewRow();

            dr["OperationGroupName"] = operationGroupName;
            dr["Visible"] = true;

            OperationGroupsDataTable.Rows.Add(dr);

            SaveOperationGroups();
            RefillOperationGroups();
        }

        public void SaveOperationGroups()
        {
            _operationGroupsDataAdapter.Update(OperationGroupsDataTable);
        }

        public void RefillOperationGroups()
        {
            OperationGroupsDataTable.Clear();
            FillOperationGroups();
        }

        public void AddWorkerGroup(string workerGroupName, bool unitToFactory)
        {
            DataRow dr = WorkerGroupsDataTable.NewRow();

            dr["WorkerGroupName"] = workerGroupName;
            dr["Visible"] = true;
            dr["UnitToFactory"] = unitToFactory;
            

            WorkerGroupsDataTable.Rows.Add(dr);

            _workerGroupsDataAdapter.Update(WorkerGroupsDataTable);
            RefillWorkerGroups();
        }

        public void SaveWorkerGroups()
        {
            _workerGroupsDataAdapter.Update(WorkerGroupsDataTable);
        }

        public void RefillWorkerGroups()
        {
            WorkerGroupsDataTable.Clear();
            FillWorkersGroups();
        }


        //public void AddMachine(int workSubsectionsId, string workSubsectionName, object machineSchemeNumber, object machineWorkPlacesCount, object machineWidth, object machineLength)
        //{
        //    DataRow mdr = MachinesDataTable.NewRow();

        //    mdr["WorkSubsectionID"] = workSubsectionsId;
        //    mdr["MachineName"] =
        //        ((DataRowView)WorkSubsectionsListBox.SelectedItem).Row["WorkSubsectionName"].ToString();

        //    mdr["MachineSchemeNumber"] = MachineSchemeNumberNumericControl.Value;
        //    mdr["MachineWorkPlacesCount"] = MachineWorkPlacesCountNumericControl.Value;

        //    mdr["Width"] = MachineWidthNumericControl.Value;
        //    mdr["Length"] = MachineLengthNumericControl.Value;
        //    mdr["IsVisible"] = true;

        //    MachinesDataTable.Rows.Add(mdr);

        //    SaveMachines();
        //    ReFillMachines();
        //}

        //public void SaveMachines()
        //{
        //    _machinesDataAdapter.Update(MachinesDataTable);
        //}

        //public void ReFillMachines()
        //{
        //    MachinesDataTable.Clear();
        //    FillMachines();
        //}




    }
}
