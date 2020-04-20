using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class StaffClass
    {
        private readonly string _connectionString;

        public BitmapImage StaffPhoto = new BitmapImage();

        #region Bindings_Declaration

        // PersonalInfo
        private MySqlDataAdapter _staffPersonalInfoDataAdapter;
        public DataTable StaffPersonalInfoDataTable;

        // WorkerGroups
        private MySqlDataAdapter _workerGroupsDataAdapter;
        public DataTable WorkerGroupsDataTable;

        // Factories
        private MySqlDataAdapter _factoriesDataAdapter;
        public DataTable FactoriesDataTable;

        // WorkerStatuses
        private MySqlDataAdapter _workerStatusesDataAdapter;
        public DataTable WorkerStatusesDataTable;

        // Professions
        private MySqlDataAdapter _professionsDataAdapter;
        public DataTable ProfessionsDataTable;

        // WorkerProfessions
        private MySqlDataAdapter _workerProfessionsDataAdapter;
        public DataTable WorkerProfessionsDataTable;

        // AdditionalWorkerProfessions
        private MySqlDataAdapter _additionalWorkerProfessionsDataAdapter;
        public DataTable AdditionalWorkerProfessionsDataTable;

        // StaffAdreses
        private MySqlDataAdapter _staffAdressesDataAdapter;
        public DataTable StaffAdressesDataTable;

        // AdressTypes
        private MySqlDataAdapter _staffAdressTypesDataAdapter;
        public DataTable StaffAdressTypesDataTable;

        // StaffEducation
        private MySqlDataAdapter _staffEducationDataAdapter;
        public DataTable StaffEducationDataTable;

        // EducationInstitutionTypes
        private MySqlDataAdapter _educationInstitutionTypesDataAdapter;
        public DataTable EducationInstitutionTypesDataTable;

        // ContactTypes
        private MySqlDataAdapter _contactTypesDataAdapter;
        public DataTable ContactTypesDataTable;

        // StaffContacts
        private MySqlDataAdapter _staffContactsDataAdapter;
        public DataTable StaffContactsDataTable;

        // ProductionStatuses
        private MySqlDataAdapter _productionStatusesDataAdapter;
        public DataTable ProductionStatusesDataTable;

        // WorkerProductionStatuses
        private MySqlDataAdapter _workerProdStatusesDataAdapter;
        public DataTable WorkerProdStatusesDataTable;

        // MartialStatuses
        private MySqlDataAdapter _martialStatusesDataAdapter;
        public DataTable MartialStatusesDataTable;

        // MartialStatuses
        private MySqlDataAdapter _tariffRatesDataAdapter;
        public DataTable TariffRatesDataTable;

        #endregion

        public StaffClass()
        {
            _connectionString = App.ConnectionInfo.ConnectionString;
            Initialize();
        }

        private void Initialize()
        {
            Create();
            Fill();
            //Binding();
        }

        private void Create()
        {
            FactoriesDataTable = new DataTable();
            ProfessionsDataTable = new DataTable();
            WorkerGroupsDataTable = new DataTable();
            WorkerStatusesDataTable = new DataTable();

            StaffPersonalInfoDataTable = new DataTable();

            WorkerProfessionsDataTable = new DataTable();
            AdditionalWorkerProfessionsDataTable = new DataTable();

            StaffAdressesDataTable = new DataTable();
            StaffAdressTypesDataTable = new DataTable();

            EducationInstitutionTypesDataTable = new DataTable();
            StaffEducationDataTable = new DataTable();

            ContactTypesDataTable = new DataTable();
            StaffContactsDataTable = new DataTable();

            ProductionStatusesDataTable = new DataTable();
            WorkerProdStatusesDataTable = new DataTable();

            MartialStatusesDataTable = new DataTable();

            TariffRatesDataTable = new DataTable();
        }

        private void Fill()
        {
            FillFactories();
            FillProfessions();
            FillWorkerGroups();
            FillWorkerStatuses();
            FillPersonalInfo();
            FillWorkerProfessions();
            FillAdditionalWorkerProfessions();
            FillStaffAdresses();
            FillStaffAdressTypes();
            FillEducationInstitutionTypes();
            FillStaffEducation();

            FillContactTypes();
            FillStaffContacts();

            FillProductionStatuses();
            FillWorkerProdStatuses();

            FillMartialStatuses();

            FillTariffRates();
        }

        #region Fill_tables

        private void FillFactories()
        {
            try
            {
                _factoriesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT FactoryID, FactoryName, Visible FROM FAIICatalog.Factories",
                        _connectionString);
                new MySqlCommandBuilder(_factoriesDataAdapter);
                _factoriesDataAdapter.Fill(FactoriesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n [SC0001]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillProfessions()
        {
            try
            {
                _professionsDataAdapter =
                    new MySqlDataAdapter(
                        @"SELECT ProfessionID, ProfessionName, WorkerGroupID, Enable, TRFC  
                          FROM FAIIStaff.Professions WHERE Enable = True ORDER BY ProfessionName",
                        _connectionString);
                new MySqlCommandBuilder(_professionsDataAdapter);
                _professionsDataAdapter.Fill(ProfessionsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n [SC0002]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkerGroups()
        {
            try
            {
                _workerGroupsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkerGroupID, WorkerGroupName, UnitToFactory, TRFC FROM FAIIStaff.WorkerGroups",
                        _connectionString);
                new MySqlCommandBuilder(_workerGroupsDataAdapter);
                _workerGroupsDataAdapter.Fill(WorkerGroupsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n [SC0003]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkerStatuses()
        {
            try
            {
                _workerStatusesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkerStatusID, WorkerStatusName, Enable, AvailableInList FROM FAIIStaff.WorkerStatuses WHERE Enable = True ORDER BY WorkerStatusName",
                        _connectionString);
                new MySqlCommandBuilder(_workerStatusesDataAdapter);
                _workerStatusesDataAdapter.Fill(WorkerStatusesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0004] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillPersonalInfo()
        {
            try
            {
                _staffPersonalInfoDataAdapter =
                    new MySqlDataAdapter(
                        @"SELECT WorkerID, Name, Birth, MartialStatusID, PassportSeries, PassportNumber, PassportAuthorityIssuing, 
                                 PassportIssueDate, MainProfessionLenghtWork, TotalLenghtWork, ContinuousLengthWork, EmpDate,
                                 StatusID, AvailableInList, Password, W26, IsHeadWorker 
                          FROM FAIIStaff.StaffPersonalInfo ORDER BY Name",
                        _connectionString);
                new MySqlCommandBuilder(_staffPersonalInfoDataAdapter);

                StaffPersonalInfoDataTable.Clear();
                _staffPersonalInfoDataAdapter.Fill(StaffPersonalInfoDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n [WC0023]Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkerProfessions()
        {
            try
            {
                const string selectCom = @"SELECT WorkerProfessionID
                                      ,WorkerID
                                      ,FactoryID
                                      ,Rate
                                      ,ProfessionID
                                      ,Category
                                      ,WorkerGroupID
                                      ,BasesSalary
                                      ,perIncByContract
                                      ,IncByContract
                                      ,perIncByPost
                                      ,IncByPost
                                      ,perIncByOther
                                      ,IncByOther
                                      ,AdditWages
                                      ,PermWages
                                  FROM FAIIStaff.WorkerProfessions
                                  ORDER BY Rate DESC";

                _workerProfessionsDataAdapter =
                    new MySqlDataAdapter(selectCom, _connectionString);
                new MySqlCommandBuilder(_workerProfessionsDataAdapter);
                _workerProfessionsDataAdapter.Fill(WorkerProfessionsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillAdditionalWorkerProfessions()
        {
            try
            {
                _additionalWorkerProfessionsDataAdapter =
                    new MySqlDataAdapter(
                        @"SELECT AdditionalWorkerProfessionID, WorkerID, ProfessionID, WorkerGroupID FROM FAIIStaff.AdditionalWorkerProfessions",
                        _connectionString);
                new MySqlCommandBuilder(_additionalWorkerProfessionsDataAdapter);
                _additionalWorkerProfessionsDataAdapter.Fill(AdditionalWorkerProfessionsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillStaffAdresses()
        {
            try
            {
                _staffAdressesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT StaffAdressID, WorkerID, StaffAdress, StaffAdressTypeID FROM FAIIStaff.StaffAdresses ORDER BY StaffAdressTypeID",
                        _connectionString);
                new MySqlCommandBuilder(_staffAdressesDataAdapter);
                _staffAdressesDataAdapter.Fill(StaffAdressesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillStaffAdressTypes()
        {
            try
            {
                _staffAdressTypesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT StaffAdressTypeID, StaffAdressTypeName, Enabled FROM FAIIStaff.StaffAdressTypes",
                        _connectionString);
                new MySqlCommandBuilder(_staffAdressTypesDataAdapter);
                _staffAdressTypesDataAdapter.Fill(StaffAdressTypesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillEducationInstitutionTypes()
        {
            try
            {
                _educationInstitutionTypesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT InstitutionTypeID, InstitutionTypeName, Enabled FROM FAIIStaff.EducationInstitutionTypes",
                        _connectionString);
                new MySqlCommandBuilder(_educationInstitutionTypesDataAdapter);
                _educationInstitutionTypesDataAdapter.Fill(EducationInstitutionTypesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillStaffEducation()
        {
            try
            {
                _staffEducationDataAdapter =
                    new MySqlDataAdapter(
                        @"SELECT StaffEducationID, WorkerID, InstitutionTypeID, InstitutionName, YearGraduation, SpecialtyName, QualificationName 
                          FROM FAIIStaff.StaffEducation",
                        _connectionString);
                new MySqlCommandBuilder(_staffEducationDataAdapter);
                _staffEducationDataAdapter.Fill(StaffEducationDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillContactTypes()
        {
            try
            {
                _contactTypesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT ContactTypeID ,ContactTypeName, ContactImage FROM FAIIStaff.ContactTypes",
                        _connectionString);
                new MySqlCommandBuilder(_contactTypesDataAdapter);
                _contactTypesDataAdapter.Fill(ContactTypesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //DataRow r1 = ContactTypesDataTable.NewRow();
            //r1["ContactTypeName"] = "Рабочий мобильный";
            //r1["ContactImage"] = ImageToByte(System.Drawing.Image.FromFile(@"d:\FAII\Resources\ContactIcons\iPhone.png"));
            //ContactTypesDataTable.Rows.Add(r1);

            //DataRow r2 = ContactTypesDataTable.NewRow();
            //r2["ContactTypeName"] = "Личный мобильный";
            //r2["ContactImage"] = ImageToByte(System.Drawing.Image.FromFile(@"d:\FAII\Resources\ContactIcons\iPhone.png"));
            //ContactTypesDataTable.Rows.Add(r2);

            //DataRow r3 = ContactTypesDataTable.NewRow();
            //r3["ContactTypeName"] = "Внутренний телефон";
            //r3["ContactImage"] = ImageToByte(System.Drawing.Image.FromFile(@"d:\FAII\Resources\ContactIcons\work-phone.png"));
            //ContactTypesDataTable.Rows.Add(r3);

            //DataRow r4 = ContactTypesDataTable.NewRow();
            //r4["ContactTypeName"] = "Рабочий городской";
            //r4["ContactImage"] = ImageToByte(System.Drawing.Image.FromFile(@"d:\FAII\Resources\ContactIcons\Phone.png"));
            //ContactTypesDataTable.Rows.Add(r4);

            //DataRow r5 = ContactTypesDataTable.NewRow();
            //r5["ContactTypeName"] = "Рабочий городской";
            //r5["ContactImage"] = ImageToByte(System.Drawing.Image.FromFile(@"d:\FAII\Resources\ContactIcons\Phone.png"));
            //ContactTypesDataTable.Rows.Add(r5);

            //DataRow r6 = ContactTypesDataTable.NewRow();
            //r6["ContactTypeName"] = "ICQ";
            //r6["ContactImage"] = ImageToByte(System.Drawing.Image.FromFile(@"d:\FAII\Resources\ContactIcons\ICQ.png"));
            //ContactTypesDataTable.Rows.Add(r6);

            //DataRow r7 = ContactTypesDataTable.NewRow();
            //r7["ContactTypeName"] = "Skype";
            //r7["ContactImage"] = ImageToByte(System.Drawing.Image.FromFile(@"d:\FAII\Resources\ContactIcons\Skype.png"));
            //ContactTypesDataTable.Rows.Add(r7);

            //DataRow r8 = ContactTypesDataTable.NewRow();
            //r8["ContactTypeName"] = "Электронная почта";
            //r8["ContactImage"] = ImageToByte(System.Drawing.Image.FromFile(@"d:\FAII\Resources\ContactIcons\Mail.png"));
            //ContactTypesDataTable.Rows.Add(r8);

            //ContactTypesDataAdapter.Update(ContactTypesDataTable);
        }

        private void FillStaffContacts()
        {
            try
            {
                _staffContactsDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT StaffContactID, ContactTypeID, WorkerID, ContactInfo FROM FAIIStaff.StaffContacts",
                        _connectionString);
                new MySqlCommandBuilder(_staffContactsDataAdapter);
                _staffContactsDataAdapter.Fill(StaffContactsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillProductionStatuses()
        {
            try
            {
                _productionStatusesDataAdapter =
                    new MySqlDataAdapter(
                        @"SELECT ProdStatusID, ProdStatusName, ProdStatusColor, ProdStatusNotes, Enable 
                          FROM FAIIStaff.ProductionStatuses WHERE Enable = True ORDER BY ProdStatusName",
                        _connectionString);
                new MySqlCommandBuilder(_productionStatusesDataAdapter);
                _productionStatusesDataAdapter.Fill(ProductionStatusesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillWorkerProdStatuses()
        {
            try
            {
                _workerProdStatusesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT WorkerProdStatusID, WorkerID, Date, ProdStatusID FROM FAIIStaff.WorkerProdStatuses",
                        _connectionString);
                new MySqlCommandBuilder(_workerProdStatusesDataAdapter);
                _workerProdStatusesDataAdapter.Fill(WorkerProdStatusesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0024] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillMartialStatuses()
        {
            try
            {
                _martialStatusesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT MartialStatusID, MartialStatusName, Enabled FROM FAIIStaff.MartialStatuses",
                        _connectionString);
                new MySqlCommandBuilder(_martialStatusesDataAdapter);
                _martialStatusesDataAdapter.Fill(MartialStatusesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0025] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillTariffRates()
        {
            try
            {
                _tariffRatesDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT TariffRateID, Category, TariffRate FROM FAIIStaff.TariffRates",
                        _connectionString);
                new MySqlCommandBuilder(_tariffRatesDataAdapter);
                _tariffRatesDataAdapter.Fill(TariffRatesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[0026] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Get_DataView

        public DataView GetTariffRates()
        {
            return TariffRatesDataTable.AsDataView();
        }

        public DataView GetStaffPersonalInfo()
        {
            return StaffPersonalInfoDataTable.AsDataView();
        }

        public DataView GetFactories()
        {
            return FactoriesDataTable.AsDataView();
        }

        public DataView GetWorkerGroups()
        {
            return WorkerGroupsDataTable.AsDataView();
        }

        public DataView GetWorkerStatuses()
        {
            var dv = WorkerStatusesDataTable.AsDataView();
            dv.RowFilter = "Enable = 'TRUE'";
            return dv;
        }

        public DataView GetProfessions()
        {
            var dv = ProfessionsDataTable.AsDataView();
            dv.RowFilter = "Enable = 'True'";
            return dv;
        }

        public DataView GetWorkerProfessions()
        {
            return WorkerProfessionsDataTable.AsDataView();
        }

        public DataView GetAdditionalWorkerProfessions()
        {
            return AdditionalWorkerProfessionsDataTable.AsDataView();
        }

        public DataView GetStaffAdresses()
        {
            return StaffAdressesDataTable.AsDataView();
        }

        public DataView GetStaffAdressTypes()
        {
            return StaffAdressTypesDataTable.AsDataView();
        }

        public DataView GetStaffEducation()
        {
            return StaffEducationDataTable.AsDataView();
        }

        public DataView GetEducationInstitutionTypes()
        {
            var dv = EducationInstitutionTypesDataTable.AsDataView();
            dv.RowFilter = "Enabled = 'True'";
            return dv;
        }

        public DataView GetStaffContacts()
        {
            return StaffContactsDataTable.AsDataView();
        }

        public DataView GetStaffContactTypes()
        {
            return ContactTypesDataTable.AsDataView();
        }

        public DataView GetProductionStatuses()
        {
            var dv = ProductionStatusesDataTable.AsDataView();
            dv.RowFilter = "Enable = 'True'";
            return dv;
        }

        public DataView GetWorkerProdStatuses()
        {
            return WorkerProdStatusesDataTable.AsDataView();
        }

        public DataView GetMartialStatuses()
        {
            return MartialStatusesDataTable.AsDataView();
        }

        #endregion

        //public BitmapImage GetPhotoFromDataBase(int workerID)
        //{
        //    var resultImage = new BitmapImage(new Uri("pack://application:,,,/Resources/nophoto.jpg", UriKind.Absolute));

        //    var sqlCon = new SqlConnection {ConnectionString = _connectionString};
        //    sqlCon.Open();
        //    var ds = new DataSet();
        //    var sqa = new SqlDataAdapter
        //        ("SELECT [Photo] FROM [FAIIStaff].[dbo].[StaffPersonalInfo] WHERE WorkerID= '" + workerID + "'", sqlCon);
        //    sqa.Fill(ds);
        //    sqlCon.Close();

        //    if (ds.Tables[0].Rows.Count == 0 || ds.Tables[0].Rows[0][0] == DBNull.Value)
        //    {
        //        StaffPhoto = resultImage;
        //        return resultImage;
        //    }
        //    using (var stream = new MemoryStream((byte[]) ds.Tables[0].Rows[0][0]))
        //    {
        //        var bitmap = new BitmapImage();
        //        bitmap.BeginInit();
        //        bitmap.StreamSource = stream;
        //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmap.EndInit();
        //        bitmap.Freeze();
        //        resultImage = bitmap;
        //    }
        //    StaffPhoto = resultImage;
        //    return resultImage;
        //}


        public object GetObjectPhotoFromDataBase(long workerId)
        {
            object photo = null;
            const string commandText = "SELECT Photo FROM FAIIStaff.StaffPersonalInfo WHERE WorkerID = @WorkerID";
            using (var sqlCon = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(commandText, sqlCon);
                command.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                try
                {
                    sqlCon.Open();
                    photo = command.ExecuteScalar();
                    sqlCon.Close();
                }
                catch (Exception)
                {
                    sqlCon.Close();
                }
            }

            return photo;
        }

        public DataTable FilterWorkers(bool groupFilter, int workerGroupID, bool factoryFilter,
            int factoryID, bool statusFilter, int statusID)
        {
            IEnumerable<DataRow> workerProfessionsByGroup = null;
            IEnumerable<DataRow> workersNamesByGroups = null;

            if (groupFilter && !factoryFilter)
            {
                workerProfessionsByGroup =
                    (WorkerProfessionsDataTable.AsEnumerable().Where(
                        r => r.Field<Int64>("WorkerGroupID") == workerGroupID));
            }

            if (!groupFilter && factoryFilter)
            {
                workerProfessionsByGroup =
                    (WorkerProfessionsDataTable.AsEnumerable().Where(
                        r => r.Field<Int64>("FactoryID") == factoryID));
            }

            if (groupFilter && factoryFilter)
            {
                workerProfessionsByGroup =
                    (WorkerProfessionsDataTable.AsEnumerable().Where(
                        r => r.Field<Int64>("WorkerGroupID") == workerGroupID)).Where(
                            r => r.Field<Int64>("FactoryID") == factoryID);
            }

            if (groupFilter || factoryFilter)
            {
                if (statusFilter)
                {
                    workersNamesByGroups =
                        (StaffPersonalInfoDataTable.AsEnumerable()
                        .Where(r => r.Field<Boolean>("AvailableInList"))
                            .Where(pidt => workerProfessionsByGroup.AsEnumerable().Any(
                                x => x.Field<Int64>("WorkerID") == pidt.Field<Int64>("WorkerID")))
                            .Where(r => r.Field<Int64>("StatusID") == statusID));
                }
                else
                {
                    workersNamesByGroups =
                        (StaffPersonalInfoDataTable.AsEnumerable()
                        .Where(r => r.Field<Boolean>("AvailableInList"))
                            .Where(pidt => workerProfessionsByGroup.AsEnumerable().Any(
                                x => x.Field<Int64>("WorkerID") == pidt.Field<Int64>("WorkerID"))));
                }
            }
            else
            {
                workersNamesByGroups =
                    (StaffPersonalInfoDataTable.AsEnumerable()
                    //.Where(r => r.Field<Boolean>("AvailableInList"))
                    .Where(r => r.Field<Int64>("StatusID") == statusID));
            }

            return workersNamesByGroups.Count() != 0
                ? workersNamesByGroups.CopyToDataTable()
                : null;
        }

        public DataTable FilterWorkers(int workerGroupID, int factoryID)
        {
            IEnumerable<DataRow> workerProfessionsByGroup = (WorkerProfessionsDataTable.AsEnumerable()
                .Where(r => r.Field<Int64>("WorkerGroupID") == workerGroupID))
                .Where( r => r.Field<Int64>("FactoryID") == factoryID);

            IEnumerable<DataRow> workersNamesByGroups = (StaffPersonalInfoDataTable.AsEnumerable()
                .Where(r => r.Field<Boolean>("AvailableInList"))
                .Where(pidt => workerProfessionsByGroup.AsEnumerable().Any(
                    x => x.Field<Int64>("WorkerID") == pidt.Field<Int64>("WorkerID"))));

            return workersNamesByGroups.Count() != 0
                ? workersNamesByGroups.CopyToDataTable()
                : null;
        }


        #region StaffAdresses

        public void AddNewStaffAdress(int workerId, int adressTypeId, string adressText)
        {
            var newRow = StaffAdressesDataTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["StaffAdressTypeID"] = adressTypeId;
            newRow["StaffAdress"] = adressText;
            StaffAdressesDataTable.Rows.Add(newRow);

            UpdateStaffAdresses();

            var staffAdressId = GetStaffAdressId(workerId, adressTypeId, adressText);
            newRow["StaffAdressID"] = staffAdressId.HasValue ? staffAdressId.Value : -1;
            newRow.AcceptChanges();
        }

        private static long? GetStaffAdressId(long workerId, int adressTypeId, string adressText)
        {
            long? staffAdressId = null;

            const string sqlCommandText = @"SELECT StaffAdressID FROM FAIIStaff.StaffAdresses
                                            WHERE WorkerID = @WorkerID AND StaffAdressTypeID = @StaffAdressTypeID
                                                  AND StaffAdress = @StaffAdress";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@StaffAdressTypeID", MySqlDbType.Int64).Value = adressTypeId;
                sqlCommand.Parameters.Add("@StaffAdress", MySqlDbType.LongText).Value = adressText;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                        staffAdressId = Convert.ToInt64(sqlResult);
                }
                catch (MySqlException) { }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }

            return staffAdressId;
        }

        public void ChangeStaffAdress(long staffAdressId, int adressTypeId, string adressText)
        {
            var dataRows = StaffAdressesDataTable.Select(string.Format("StaffAdressID = {0}", staffAdressId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow["StaffAdressTypeID"] = adressTypeId;
            dataRow["StaffAdress"] = adressText;

            UpdateStaffAdresses();
        }

        public void DeleteStaffAdress(long staffAdressId)
        {
            var dataRows = StaffAdressesDataTable.Select(string.Format("StaffAdressID = {0}", staffAdressId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow.Delete();

            UpdateStaffAdresses();
        }

        private void UpdateStaffAdresses()
        {
            try
            {
                _staffAdressesDataAdapter.Update(StaffAdressesDataTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0010] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region StaffContacts

        public void AddNewStaffContact(int workerId, int contactTypeId, string contactInfo)
        {
            var newRow = StaffContactsDataTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["ContactTypeID"] = contactTypeId;
            newRow["ContactInfo"] = contactInfo;
            StaffContactsDataTable.Rows.Add(newRow);

            UpdateStaffContacts();

            var staffContactId = GetStaffContactId(workerId, contactTypeId, contactInfo);
            newRow["StaffContactID"] = staffContactId.HasValue ? staffContactId.Value : -1;
            newRow.AcceptChanges();
        }

        private static long? GetStaffContactId(long workerId, int contactTypeId, string contactInfo)
        {
            long? staffContactId = null;

            const string sqlCommandText = @"SELECT StaffContactID FROM FAIIStaff.StaffContacts
                                            WHERE WorkerID = @WorkerID AND ContactTypeID = @ContactTypeID
                                                  AND ContactInfo = @ContactInfo";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@ContactTypeID", MySqlDbType.Int64).Value = contactTypeId;
                sqlCommand.Parameters.Add("@ContactInfo", MySqlDbType.LongText).Value = contactInfo;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                        staffContactId = Convert.ToInt64(sqlResult);
                }
                catch (MySqlException) { }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }

            return staffContactId;
        }      

        public void ChangeStaffContact(long staffContactId, int contactTypeId, string contactInfo)
        {
            var dataRows = StaffContactsDataTable.Select(string.Format("StaffContactID = {0}", staffContactId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow["ContactTypeID"] = contactTypeId;
            dataRow["ContactInfo"] = contactInfo;

            UpdateStaffContacts();
        }

        public void DeleteStaffContact(long staffContactId)
        {
            var dataRows = StaffContactsDataTable.Select(string.Format("StaffContactID = {0}", staffContactId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow.Delete();

            UpdateStaffContacts();
        }

        private void UpdateStaffContacts()
        {
            try
            {
                _staffContactsDataAdapter.Update(StaffContactsDataTable);
            }
            catch (MySqlException exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0011] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region WorkerStatuses

        public void AddNewWorkerStatus(string statusName, bool availableInList)
        {
            var newRow = WorkerStatusesDataTable.NewRow();
            newRow["WorkerStatusName"] = statusName;
            newRow["AvailableInList"] = availableInList;
            newRow["Enable"] = true;
            WorkerStatusesDataTable.Rows.Add(newRow);

            UpdateWorkerStatuses();
            WorkerStatusesDataTable.Clear();
            FillWorkerStatuses();
        }

        private void UpdateWorkerStatuses()
        {
            try
            {
                _workerStatusesDataAdapter.Update(WorkerStatusesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0007] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveStatus(int workerStatusId, string WorkerStatusName, bool AvailableInList)
        {
            var dataRows = WorkerStatusesDataTable.Select(string.Format("WorkerStatusID = {0}", workerStatusId));
            if (dataRows.Length != 0)
            {
                var dataRow = dataRows[0];
                dataRow["AvailableInList"] = AvailableInList;
                dataRow["WorkerStatusName"] = WorkerStatusName;
                UpdateWorkerStatuses();
            }
        }

        public void DeleteStatus(int workerStatusId)
        {
            var dataRows = WorkerStatusesDataTable.Select(string.Format("WorkerStatusID = {0}", workerStatusId));
            if (dataRows.Length != 0)
            {
                var dataRow = dataRows[0];
                dataRow["Enable"] = false;
                UpdateWorkerStatuses();
            }
        }

        #endregion


        #region Professions

        public void AddNewProfession(string professionName, int workerGroupId, long trfc)
        {
            var newRow = ProfessionsDataTable.NewRow();
            newRow["ProfessionName"] = professionName;
            newRow["WorkerGroupID"] = workerGroupId;
            newRow["Enable"] = true;
            newRow["TRFC"] = trfc;

            ProfessionsDataTable.Rows.Add(newRow);

            UpdateProfessions();
            ProfessionsDataTable.Clear();
            FillProfessions();
        }

        private void UpdateProfessions()
        {
            try
            {
                _professionsDataAdapter.Update(ProfessionsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0008] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DeleteProfession(int professionId)
        {
            var dataRows = ProfessionsDataTable.Select(string.Format("ProfessionID = {0}", professionId));
            if (dataRows.Length != 0)
            {
                var dataRow = dataRows[0];
                dataRow["Enable"] = false;
                UpdateProfessions();
            }
        }

        public void ChangeProfession(int professionId, string professionName, int workerGroupId, long trfc)
        {
            var dataRows = ProfessionsDataTable.Select(string.Format("ProfessionID = {0}", professionId));
            if (dataRows.Length != 0)
            {
                var dataRow = dataRows[0];
                dataRow["ProfessionName"] = professionName;
                dataRow["WorkerGroupID"] = workerGroupId;
                dataRow["TRFC"] = trfc;

                UpdateProfessions();
            }
        }

        public void ChangeWorkerProfessionsSalaries(long professionId, long trfc)
        {
            foreach(var workerProfession in WorkerProfessionsDataTable.AsEnumerable().
                Where(r => r.Field<Int64>("ProfessionID") == professionId))
            {
                var workerProfessionId = Convert.ToInt64(workerProfession["WorkerProfessionID"]);
                var rate = Convert.ToDecimal(workerProfession["Rate"]);
                var category = workerProfession["Category"].ToString();

                var tariffRates = TariffRatesDataTable.AsEnumerable().Where(r => r.Field<string>("Category") == category);
                if (!tariffRates.Any()) continue;

                var tariffRate = Convert.ToDecimal(tariffRates.Last()["TariffRate"]);
                var basesSalary = rate * tariffRate * trfc;

                var incByContract = workerProfession["IncByContract"] != DBNull.Value 
                    ? Convert.ToInt64(workerProfession["IncByContract"])
                    : 0;
                var incByPost = workerProfession["IncByPost"] != DBNull.Value
                    ? Convert.ToInt64(workerProfession["IncByPost"])
                    : 0;
                var incByOther = workerProfession["IncByOther"] != DBNull.Value
                    ? Convert.ToInt64(workerProfession["IncByOther"])
                    : 0;
                var additWages = workerProfession["AdditWages"] != DBNull.Value
                    ? Convert.ToInt64(workerProfession["AdditWages"])
                    : 0;

                var permanentPartWages = basesSalary + incByContract + incByPost + incByOther + additWages;
                ChangeWorkerProfession(workerProfessionId, Convert.ToInt64(basesSalary), Convert.ToInt64(permanentPartWages));
            }
        }

        #endregion


        #region ProductionStatuses

        public void AddNewProdStatus(string prodStatusName, object prodStatusColor, string prodStatusNotes)
        {
            DataRow newDr = ProductionStatusesDataTable.NewRow();
            newDr["ProdStatusName"] = prodStatusName;
            newDr["ProdStatusColor"] = prodStatusColor;
            newDr["ProdStatusNotes"] = prodStatusNotes;
            newDr["Enable"] = true;
            ProductionStatusesDataTable.Rows.Add(newDr);

            UpdateProdStatuses();
            ProductionStatusesDataTable.Clear();
            FillProductionStatuses();
        }

        private void UpdateProdStatuses()
        {
            try
            {
                _productionStatusesDataAdapter.Update(ProductionStatusesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0009] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ChangeProdStatus(int prodStatusId, string prodStatusName, string prodStatusNotes,
            object prodStatusColor)
        {
            DataRow[] dataRows = ProductionStatusesDataTable.Select(string.Format("ProdStatusID = {0}", prodStatusId));
            if (dataRows.Length == 0) return;

            DataRow dataRow = dataRows[0];

            dataRow["ProdStatusName"] = prodStatusName;
            dataRow["ProdStatusNotes"] = prodStatusNotes;
            dataRow["ProdStatusColor"] = prodStatusColor;

            UpdateProdStatuses();
        }

        public void DeleteProdStatus(int prodStatusId)
        {
            DataRow[] dataRows = ProductionStatusesDataTable.Select(string.Format("ProdStatusID = {0}", prodStatusId));
            if (dataRows.Length == 0) return;

            DataRow dataRow = dataRows[0];

            dataRow["Enable"] = false;

            UpdateProdStatuses();
        }

        #endregion


        #region WorkerProfessions

        public void AddNewWorkerProfession(int workerId, int factoryId, int workerGroupId, int professionId,
            decimal rate, string category, long basesSalary, decimal perIncByContract, long incByContract,
            decimal perIncByPost, long incByPost, decimal perIncByOther, long incByOther, long additWages,
            long permWages)
        {
            DataRow newWorkerProfessionDr = WorkerProfessionsDataTable.NewRow();

            newWorkerProfessionDr["WorkerID"] = workerId;
            newWorkerProfessionDr["FactoryID"] = factoryId;
            newWorkerProfessionDr["Rate"] = rate;
            newWorkerProfessionDr["ProfessionID"] = professionId;
            newWorkerProfessionDr["Category"] = category;
            newWorkerProfessionDr["WorkerGroupID"] = workerGroupId;

            newWorkerProfessionDr["BasesSalary"] = basesSalary;
            newWorkerProfessionDr["perIncByContract"] = perIncByContract;
            newWorkerProfessionDr["IncByContract"] = incByContract;
            newWorkerProfessionDr["perIncByPost"] = perIncByPost;
            newWorkerProfessionDr["IncByPost"] = incByPost;
            newWorkerProfessionDr["perIncByOther"] = perIncByOther;
            newWorkerProfessionDr["IncByOther"] = incByOther;
            newWorkerProfessionDr["AdditWages"] = additWages;
            newWorkerProfessionDr["PermWages"] = permWages;

            WorkerProfessionsDataTable.Rows.Add(newWorkerProfessionDr);

            UpdateWorkerProfessions();

            var workerProfessionId = GetWorkerProfessionId(workerId, factoryId, workerGroupId, professionId);
            newWorkerProfessionDr["WorkerProfessionID"] = workerProfessionId.HasValue ? workerProfessionId.Value : -1;
            newWorkerProfessionDr.AcceptChanges();
        }

        private static long? GetWorkerProfessionId(long workerId, long factoryId, long workerGroupId, long professionId)
        {
            long? workerProfessionId = null;

            const string sqlCommandText = @"SELECT WorkerProfessionID FROM FAIIStaff.WorkerProfessions
                                            WHERE WorkerID = @WorkerID AND FactoryID = @FactoryID
                                                  AND WorkerGroupID = @WorkerGroupID AND ProfessionID = @ProfessionID";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@FactoryID", MySqlDbType.Int64).Value = factoryId;
                sqlCommand.Parameters.Add("@WorkerGroupID", MySqlDbType.Int64).Value = workerGroupId;
                sqlCommand.Parameters.Add("@ProfessionID", MySqlDbType.Int64).Value = professionId;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                        workerProfessionId = Convert.ToInt64(sqlResult);
                }
                catch (MySqlException) { }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }

            return workerProfessionId;
        }

        public void ChangeWorkerProfession(long workerProfessionId, int factoryId, int workerGroupId, int professionId,
            decimal rate, string category, long basesSalary, decimal perIncByContract, long incByContract,
            decimal perIncByPost, long incByPost, decimal perIncByOther, long incByOther, long additWages,
            long permWages)
        {
            var dataRows = WorkerProfessionsDataTable.Select(string.Format("WorkerProfessionID = {0}", workerProfessionId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow["FactoryID"] = factoryId;
            dataRow["Rate"] = rate;
            dataRow["ProfessionID"] = professionId;
            dataRow["Category"] = category;
            dataRow["WorkerGroupID"] = workerGroupId;

            dataRow["BasesSalary"] = basesSalary;
            dataRow["perIncByContract"] = perIncByContract;
            dataRow["IncByContract"] = incByContract;
            dataRow["perIncByPost"] = perIncByPost;
            dataRow["IncByPost"] = incByPost;
            dataRow["perIncByOther"] = perIncByOther;
            dataRow["IncByOther"] = incByOther;
            dataRow["AdditWages"] = additWages;
            dataRow["PermWages"] = permWages;

            UpdateWorkerProfessions();
        }

        public void ChangeWorkerProfession(long workerProfessionId, long basesSalary, long permWages)
        {
            var dataRows = WorkerProfessionsDataTable.Select(string.Format("WorkerProfessionID = {0}", workerProfessionId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow["BasesSalary"] = basesSalary;
            dataRow["PermWages"] = permWages;

            UpdateWorkerProfessions();
        }

        public void DeleteWorkerProfession(long workerProfessionId)
        {
            var dataRows = WorkerProfessionsDataTable.Select(string.Format("WorkerProfessionID = {0}", workerProfessionId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow.Delete();

            UpdateWorkerProfessions();
        }

        private void UpdateWorkerProfessions()
        {
            try
            {
                _workerProfessionsDataAdapter.Update(WorkerProfessionsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0012] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region AdditionalWorkerProfessions

        public void AddNewAdditWorkerProfession(int workerId, int professionId)
        {
            DataRow newAdditionalWorkerProfessionDr = AdditionalWorkerProfessionsDataTable.NewRow();
            newAdditionalWorkerProfessionDr["WorkerID"] = workerId;
            newAdditionalWorkerProfessionDr["ProfessionID"] = professionId;
            AdditionalWorkerProfessionsDataTable.Rows.Add(newAdditionalWorkerProfessionDr);

            UpdateWorkerAdditionalProfessions();

            var additWorkerProfId = GetWorkerAdditProfessionId(workerId, professionId);
            newAdditionalWorkerProfessionDr["AdditionalWorkerProfessionID"] = additWorkerProfId.HasValue ? additWorkerProfId.Value : -1;
            newAdditionalWorkerProfessionDr.AcceptChanges();
        }

        private static long? GetWorkerAdditProfessionId(long workerId, long professionId)
        {
            long? additWorkerProfessionId = null;

            const string sqlCommandText = @"SELECT AdditionalWorkerProfessionID FROM FAIIStaff.AdditionalWorkerProfessions 
                                            WHERE WorkerID = @WorkerID AND ProfessionID = @ProfessionID 
                                            ORDER BY AdditionalWorkerProfessionID DESC";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@ProfessionID", MySqlDbType.Int64).Value = professionId;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                        additWorkerProfessionId = Convert.ToInt64(sqlResult);
                }
                catch (MySqlException) { }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }

            return additWorkerProfessionId;
        }

        public void ChangeWorkerAdditProfession(long workerAdditProfessionId, int professionId)
        {
            var dataRows = AdditionalWorkerProfessionsDataTable.Select(string.Format("AdditionalWorkerProfessionID = {0}", workerAdditProfessionId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow["ProfessionID"] = professionId;

            UpdateWorkerAdditionalProfessions();
        }

        public void DeleteWorkerAdditProfession(long workerAdditProfessionId)
        {
            var dataRows = AdditionalWorkerProfessionsDataTable.Select(string.Format("AdditionalWorkerProfessionID = {0}", workerAdditProfessionId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow.Delete();

            UpdateWorkerAdditionalProfessions();
        }

        private void UpdateWorkerAdditionalProfessions()
        {
            try
            {
                _additionalWorkerProfessionsDataAdapter.Update(AdditionalWorkerProfessionsDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0013] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region WorkerProdStatuses

        public void AddNewWorkerProdStatus(int workerId, int prodStatusId, DateTime date)
        {
            var newRow = WorkerProdStatusesDataTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["Date"] = date;
            newRow["ProdStatusID"] = prodStatusId;
            WorkerProdStatusesDataTable.Rows.Add(newRow);

            UpdateWorkerProdStatuses();

            var workerProdStatusId = GetWorkerProdStatusId(workerId, prodStatusId, date);
            newRow["WorkerProdStatusID"] = workerProdStatusId.HasValue ? workerProdStatusId.Value : -1;
            newRow.AcceptChanges();
        }

        private static long? GetWorkerProdStatusId(long workerId, int prodStatusId, DateTime date)
        {
            long? workerProdStatusId = null;

            const string sqlCommandText = @"SELECT WorkerProdStatusID FROM FAIIStaff.WorkerProdStatuses 
                                            WHERE WorkerID = @WorkerID AND ProdStatusID = @ProdStatusID 
                                                  AND Date = @Date
                                            ORDER BY WorkerProdStatusID DESC";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@ProdStatusID", MySqlDbType.Int64).Value = prodStatusId;
                sqlCommand.Parameters.Add("@Date", MySqlDbType.Date).Value = date;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                        workerProdStatusId = Convert.ToInt64(sqlResult);
                }
                catch (MySqlException) { }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }

            return workerProdStatusId;
        }

        public void ChangeWorkerProdStatus(long workerProdStatusId, int prodStatusId, DateTime date)
        {
            var dataRows = WorkerProdStatusesDataTable.Select(string.Format("WorkerProdStatusID = {0}", workerProdStatusId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow["ProdStatusID"] = prodStatusId;
            dataRow["Date"] = date;

            UpdateWorkerProdStatuses();
        }

        public void DeleteWorkerProdStatus(long workerProdStatusId)
        {
            var dataRows = WorkerProdStatusesDataTable.Select(string.Format("WorkerProdStatusID = {0}", workerProdStatusId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow.Delete();

            UpdateWorkerProdStatuses();
        }

        private void UpdateWorkerProdStatuses()
        {
            try
            {
                _workerProdStatusesDataAdapter.Update(WorkerProdStatusesDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0014] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region StaffEducation

        public void AddNewStaffEducation(int workerId, int institutionTypeId, string institutionName,
            int yearGraduation, string specialtyName, string qualificationName)
        {
            var newRow = StaffEducationDataTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["InstitutionTypeID"] = institutionTypeId;
            newRow["InstitutionName"] = institutionName;
            newRow["YearGraduation"] = yearGraduation;
            newRow["SpecialtyName"] = specialtyName;
            newRow["QualificationName"] = qualificationName;
            StaffEducationDataTable.Rows.Add(newRow);

            UpdateStaffEducation();

            var staffEducationId = GetStaffEducationId(workerId, institutionTypeId, institutionName);
            newRow["StaffEducationID"] = staffEducationId.HasValue ? staffEducationId.Value : -1;
            newRow.AcceptChanges();
        }

        private static long? GetStaffEducationId(long workerId, int institutionTypeId, string institutionName)
        {
            long? staffEducationId = null;

            const string sqlCommandText = @"SELECT StaffEducationID FROM FAIIStaff.StaffEducation
                                            WHERE WorkerID = @WorkerID AND InstitutionTypeID = @InstitutionTypeID
                                                  AND InstitutionName = @InstitutionName
                                            ORDER BY StaffEducationID DESC";
            using (var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerID", MySqlDbType.Int64).Value = workerId;
                sqlCommand.Parameters.Add("@InstitutionTypeID", MySqlDbType.Int64).Value = institutionTypeId;
                sqlCommand.Parameters.Add("@InstitutionName", MySqlDbType.LongText).Value = institutionName;

                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                        staffEducationId = Convert.ToInt64(sqlResult);
                }
                catch (MySqlException) { }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }

            return staffEducationId;
        }

        public void ChangeStaffEducation(long staffEducationId, int institutionTypeId, string institutionName,
            int yearGraduation, string specialtyName, string qualificationName)
        {
            var dataRows = StaffEducationDataTable.Select(string.Format("StaffEducationID = {0}", staffEducationId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow["InstitutionTypeID"] = institutionTypeId;
            dataRow["InstitutionName"] = institutionName;
            dataRow["YearGraduation"] = yearGraduation;
            dataRow["SpecialtyName"] = specialtyName;
            dataRow["QualificationName"] = qualificationName;

            UpdateStaffEducation();
        }

        public void DeleteStaffEducation(long staffEducationId)
        {
            var dataRows = StaffEducationDataTable.Select(string.Format("StaffEducationID = {0}", staffEducationId));
            if (dataRows.Length == 0) return;

            var dataRow = dataRows.First();
            dataRow.Delete();

            UpdateStaffEducation();
        }

        private void UpdateStaffEducation()
        {
            try
            {
                _staffEducationDataAdapter.Update(StaffEducationDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0015] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region WorkerInfo

        private string GetMD5(string text)
        {
            using (MD5 hasher = MD5.Create())
            {
                byte[] data = hasher.ComputeHash(Encoding.Default.GetBytes(text));

                var sBuilder = new StringBuilder();

                //преобразование в HEX
                foreach (byte t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public void AddNewWorker(string workerName)
        {
            var newRow = StaffPersonalInfoDataTable.NewRow();
            newRow["Name"] = workerName;
            newRow["AvailableInList"] = true;
            newRow["StatusID"] = 1;
            newRow["Password"] = GetMD5("2002");
            StaffPersonalInfoDataTable.Rows.Add(newRow);

            UpdateWorkers();

            var workerId = GetWorkerId(workerName);
            newRow["WorkerID"] = workerId.HasValue ? workerId.Value : -1;
            newRow.AcceptChanges();
        }

        private static long? GetWorkerId(string workerName)
        {
            long? workerId = null;
            const string sqlCommandText = @"SELECT WorkerID FROM FAIIStaff.StaffPersonalInfo 
                                            WHERE Name = @WorkerName";
            using(var sqlConn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var sqlCommand = new MySqlCommand(sqlCommandText, sqlConn);
                sqlCommand.Parameters.Add("@WorkerName", MySqlDbType.VarChar).Value = workerName;
                try
                {
                    sqlConn.Open();
                    var sqlResult = sqlCommand.ExecuteScalar();
                    if (sqlResult != null && sqlResult != DBNull.Value)
                        workerId = Convert.ToInt64(sqlResult);
                }
                catch (MySqlException)
                {
                }
                finally
                {
                    sqlConn.Close();
                    sqlCommand.Dispose();
                }
            }

            return workerId;
        }

        private void UpdateWorkers()
        {
            try
            {
                _staffPersonalInfoDataAdapter.Update(StaffPersonalInfoDataTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0016] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ChangeWorkerInfo(long workerId, string workerName, DateTime birthDate, int martialStatusId,
            string passportNumber, DateTime passportIssueDate, string passportAuthorityIssuing)
        {
            var dataRows = StaffPersonalInfoDataTable.Select(string.Format("WorkerID = {0}", workerId));
            if (dataRows.Length == 0) return;

            var worker = dataRows.First();

            worker["Name"] = workerName;
            worker["Birth"] = birthDate;
            worker["MartialStatusID"] = martialStatusId;
            worker["PassportNumber"] = passportNumber;
            worker["PassportIssueDate"] = passportIssueDate;
            worker["PassportAuthorityIssuing"] = passportAuthorityIssuing;

            UpdateWorkers();
        }

        public void ChangeWorkerStatus(long workerId, int statusId, bool availableInList)
        {
            var dataRows = StaffPersonalInfoDataTable.Select(string.Format("WorkerID = {0}", workerId));
            if (dataRows.Length == 0) return;

            var worker = dataRows.First();
            worker["StatusID"] = statusId;
            worker["AvailableInList"] = availableInList;

            UpdateWorkers();
        }

        #endregion


        public bool SelectPhoto(int workerId)
        {
            const bool Result = false;

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.* "
            };

            if (dlg.ShowDialog() == true)
            {
                const int newX = 200;

                byte[] photoByte;

                System.Drawing.Bitmap resizedImage;
                using (System.Drawing.Image originalImage = System.Drawing.Image.FromFile(dlg.FileName))
                {
                    double x = originalImage.Width;
                    double y = originalImage.Height;
                    double newYd = newX*(y/x);
                    int newY = Convert.ToInt32(newYd);

                    resizedImage = new System.Drawing.Bitmap(originalImage, newX, newY);
                }

                using (var stream = new MemoryStream())
                {
                    resizedImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                    stream.Position = 0;
                    photoByte = new byte[stream.Length];
                    stream.Read(photoByte, 0, (int) stream.Length);
                    stream.Close();
                }

                ChangePhoto(photoByte, workerId);

                resizedImage.Dispose();

                return true;
            }
            return Result;
        }

        private void ChangePhoto(byte[] photoByte, int workerId)
        {
            try
            {
                var sqlCon = new MySqlConnection { ConnectionString = _connectionString };
                sqlCon.Open();
                var command =
                    new MySqlCommand(
                        "UPDATE FAIIStaff.StaffPersonalInfo SET Photo = @Photo WHERE WorkerID= '" + workerId +
                        "'", sqlCon);
                command.Parameters.Add("@Photo", MySqlDbType.Blob, photoByte.Length).Value = photoByte;
                command.ExecuteNonQuery();
                sqlCon.Close();
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0017] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CheckPassword(string password, int workerId)
        {
            var result = false;

            var custView = new DataView(StaffPersonalInfoDataTable, "", "WorkerID",
                DataViewRowState.CurrentRows);

            DataRowView[] foundRows = custView.FindRows(workerId);

            if (!foundRows.Any()) return result;

            if (foundRows[0].Row["Password"].ToString() == GetMD5(password))
                result = true;

            return result;
        }

        public int ChangePassword(string password, string newPassword, string newPassword2, int workerId)
        {
            //if (!CheckPassword(password, workerId)) return 1;

            //if (newPassword != newPassword2) return 2;

            //if (newPassword.Length < 4) return 5;

            if (!SetPassword(workerId, GetMD5(newPassword))) return 3;

            return 4;
        }

        private bool SetPassword(int workerId, string value)
        {
            var custView = new DataView(StaffPersonalInfoDataTable, "", "WorkerID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(workerId);

            if (!foundRows.Any()) return false;

            foundRows[0].Row["Password"] = value;

            UpdateWorkers();

            return true;
        }

        public bool SetHeadWorker(int workerId, bool value)
        {
            var custView = new DataView(StaffPersonalInfoDataTable, "", "WorkerID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(workerId);

            if (!foundRows.Any()) return false;

            foundRows[0].Row["IsHeadWorker"] = value;

            UpdateWorkers();

            return true;
        }

        public string GetWorkerName(int workerId, bool shortName = false)
        {
            string name = string.Empty;

            var custView = new DataView(StaffPersonalInfoDataTable, "", "WorkerID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(workerId);

            if (!foundRows.Any()) return name;

            name = foundRows[0].Row["Name"].ToString();

            if (!shortName)
            return name;

            return GetShortName(name);
        }

        private string GetShortName(string fullName)
        {
            string shortName = string.Empty;
            string[] fio = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool first = true;

            foreach (string s in fio.Where(s => s.Length > 1))
            {
                if (!first)
                    shortName += " " + s.Remove(1) + ".";
                else
                {
                    shortName += s;
                    first = false;
                }
            }

            return shortName;
        }

        public bool SetW26(int workerId, string value)
        {
            int wId = 0;

            if (!CheckW26(value, ref wId))
            {
                return WriteW26(workerId, value);
            }

            if (wId == workerId)
                return true;

            var dialogResult = MessageBox.Show(
                "Данный пропуск закреплен за '" + GetWorkerName(wId) + "'.\n Изменить владельца пропуска на '" +
                GetWorkerName(workerId) + "' ?", "Замена",
                MessageBoxButton.YesNo);

            if (dialogResult == MessageBoxResult.Yes)
            {
                ClearW26(wId);

                return WriteW26(workerId, value);
            }

            return false;
        }

        private bool WriteW26(int workerId, string value)
        {
            var custView = new DataView(StaffPersonalInfoDataTable, "", "WorkerID",
                   DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(workerId);

            if (!foundRows.Any()) return false;

            foundRows[0].Row["W26"] = value;

            UpdateWorkers();

            return true;
        }

        public bool CheckW26(string w26, ref int workerId)
        {
            var result = false;

            var custView = new DataView(StaffPersonalInfoDataTable, "", "W26",
                DataViewRowState.CurrentRows);

            DataRowView[] foundRows = custView.FindRows(w26);

            if (foundRows.Any())
            {
                workerId = Convert.ToInt32(foundRows[0]["WorkerID"]);
                result = true;
            }

            return result;
        }

        public bool ClearW26(int workerId)
        {
            var custView = new DataView(StaffPersonalInfoDataTable, "", "WorkerID",
                DataViewRowState.CurrentRows);

            var foundRows = custView.FindRows(workerId);

            if (!foundRows.Any()) return false;

            foundRows[0].Row["W26"] = string.Empty;

            UpdateWorkers();

            return true;
        }
        
        //public DataView GetPersonalInfoByGroups(int workerGroupId, int factoryId)
        //{
        //    // Get all worker professions with chosen ID`s of workerGroup and factory.
        //    var workerProfessionsByGroup =
        //        (WorkerProfessionsDataTable.AsEnumerable().Where(
        //            r => r.Field<Int64>("WorkerGroupID") == workerGroupId &&
        //                r.Field<Int64>("FactoryID") == factoryId));

        //    // Get workers from worker professions.
        //    var workersByGroups =
        //        (StaffPersonalInfoDataTable.AsEnumerable()
        //            .Where(pidt => workerProfessionsByGroup.AsEnumerable().Any(
        //                x => x.Field<Int64>("WorkerID") == pidt.Field<Int64>("WorkerID"))));

        //    if (workersByGroups.Count() == 0)
        //        return null;

        //    // Return independent view.
        //    DataTable table = workersByGroups.CopyToDataTable();

        //    return table.AsDataView();
        //}


        #region WorkerFiles

        private DataTable _workerFilesTable;
        private MySqlDataAdapter _workerFilesAdapter;

        public DataTable WorkerFilesTable
        {
            set { _workerFilesTable = value; }
            get
            {
                if (_workerFilesTable == null)
                {
                    _workerFilesTable = new DataTable();
                    FillWorkerFiles();
                }

                return _workerFilesTable;
            }
        }

        public DataView WorkerFilesView
        {
            get { return WorkerFilesTable.AsDataView(); }
        }

        private void FillWorkerFiles()
        {
            const string sqlCommand = @"SELECT WorkerFileID, WorkerID, FileName, MainWorkerID, AddingDate, DeletingNeeded 
                                        FROM FAIIStaff.WorkerFiles WHERE DeletingNeeded = 'FALSE'";
            _workerFilesAdapter = new MySqlDataAdapter(sqlCommand, _connectionString);
            new MySqlCommandBuilder(_workerFilesAdapter);
            _workerFilesTable.Clear();

            try
            {
                _workerFilesAdapter.Fill(_workerFilesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0005] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddFileToWorker(int workerId, string fileName, int mainWorkerId, DateTime date)
        {
            var newRow = WorkerFilesTable.NewRow();
            newRow["WorkerID"] = workerId;
            newRow["FileName"] = fileName;
            newRow["MainWorkerID"] = mainWorkerId;
            newRow["AddingDate"] = date;
            WorkerFilesTable.Rows.Add(newRow);

            UpdateWorkerFiles();
            FillWorkerFiles();
        }

        private void UpdateWorkerFiles()
        {
            try
            {
                _workerFilesAdapter.Update(WorkerFilesTable);
            }
            catch (Exception exp)
            {
                MessageBox.Show(
                    exp.Message +
                    "\n\n[SC0006] Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DeleteWorkerFile(int workerFileId)
        {
            var rows = WorkerFilesTable.AsEnumerable().Where(r => r.Field<Int64>("WorkerFileID") == workerFileId);
            if (!rows.Any()) return;

            var deletingRow = rows.First();
            deletingRow.Delete();

            UpdateWorkerFiles();
        }

        public void MarkWorkerFileForDelete(int workerFileId)
        {
            var rows = WorkerFilesTable.AsEnumerable().Where(r => r.Field<Int64>("WorkerFileID") == workerFileId);
            if (!rows.Any()) return;

            var markedRow = rows.First();
            markedRow["DeletingNeeded"] = true;

            UpdateWorkerFiles();
            FillWorkerFiles();
        }

        public static void DeleteNeededWorkerFile()
        {
            //const string sqlCommandText = "SELECT [WorkerFileID], [WorkerID], [FileName], [MainWorkerID], " +
            //                              "[AddingDate], [DeletingNeeded] FROM [FAIIStaff].[dbo].[WorkerFiles] " +
            //                              "WHERE [DeletingNeeded] = 'TRUE'";
            var table = new DataTable();
            //var dataAdapter = new SqlDataAdapter(sqlCommandText, App.ConnectionInfo.ConnectionString);
            //new SqlCommandBuilder(dataAdapter);
            //try
            //{
            //    dataAdapter.Fill(table);
            //}
            //catch
            //{ }

            foreach (var row in table.AsEnumerable().Where(r => r.Field<bool>("DeletingNeeded")))
            {
                try
                {
                    var workerId = Convert.ToInt32(row["WorkerID"]);
                    var workerPath = GetWorkerPath(workerId);

                    var path = Path.Combine(workerPath, row["FileName"].ToString());
                    if (File.Exists(path))
                        File.Delete(path);
                    row.Delete();
                }
                catch (Exception) { }
            }

            //try
            //{
            //    dataAdapter.Update(table);
            //}
            //catch { }
        }

        public static string GetRenamedFileName(string fileName)
        {
            var renamedFileName = fileName;
            const string str = @"?:<>/|\""";
            foreach (var symbol in renamedFileName)
            {
                if (str.Any(s => s == symbol))
                    renamedFileName = renamedFileName.Replace(symbol, '_');
            }
            return renamedFileName.Trim();
        }

        public static string GetWorkerPath(int workerId)
        {
            var directoryInfo = new DirectoryInfo(@"FAIIFileStorage\WorkerFiles");
            if (!directoryInfo.Exists) return string.Empty;

            var workerFolders = directoryInfo.GetDirectories(string.Format("*[id]{0}", workerId));
            return !workerFolders.Any() ? string.Empty : workerFolders.First().FullName;
        }

        #endregion
    }




    public class Worker : INotifyPropertyChanged
    {
        private Int64 _workerId;

        public Int64 WorkerID
        {
            get { return _workerId; }
            set
            {
                _workerId = value;
                RaisePropertyChanged("WorkerID");
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private ImageSource _photo;

        public ImageSource Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                RaisePropertyChanged("Photo");
            }
        }

        public Worker(Int64 wokerId, string name, ImageSource photo)
        {
            WorkerID = wokerId;
            Name = name;
            Photo = photo;
        }


        #region INotifyPropertyChanged

        /// <summary>
        /// Событие, которое мы должны вызывать каждый раз когда хотим сообщить об изменении данных.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Метод для вызова события об изменении свойства ViewModel.
        /// </summary>
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
