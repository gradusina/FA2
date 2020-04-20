using System;
using FA2;
using FA2.Classes;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace FAII.Classes
{
    public class BaseClass
    {
        private TimeTrackingClass _ttc;
        private TimeControlClass _tcc;


        private ExportToExcel _expToExcel;

        private ProductionCalendarClass _pcc;

        private ServiceEquipmentClass _sec;

        private PlannedScheduleClass _psc;

        private TimeSheetClass _tsc;

        private ProdRoomsClass _prc;

        private StimulationClass _stc;

        private StaffClass _sc;

        private AdministrationClass _ac;
        private CatalogClass _cc;

        private NewsFeedClass _nfc;

        private TechnologyProblemClass _tpr;

        //private CompanyStructureClass _csc;
        
        //private ProductionScheduleClass _prodShClass;

        private WorkshopMapClass _wmt;

        //private CatalogClass _cc;

        private MyWorkersClass _mwc;

        private TaskClass _tc;

        private WorkerRequestsClass _workerRequestsClass;

        private AdmissionsClass _admClass;

        private PlannedWorksClass _plannedWorksClass;

        public ObservableCollection<int> DiffBetweenTwoDeviationRowsCollection = new ObservableCollection<int>();

        public bool GetTaskClass(ref TaskClass tc, bool refreshClass = false)
        {
            bool isFill = false;

            if (_tc == null || refreshClass)
            {
                _tc = new TaskClass();
                isFill = true;
            }
            tc = _tc;

            return isFill;
        }

        public bool GetWorkshopMapClass(ref WorkshopMapClass wmt, bool refreshClass = false)
        {
            bool isFill = false;

            if (_wmt == null || refreshClass)
            {
                _wmt = new WorkshopMapClass();
                isFill = true;
            }
            wmt = _wmt;

            return isFill;
        }


        public bool GetExportToExcelClass(ref ExportToExcel expToExcel, bool refreshClass = false)
        {
            bool isFill = false;

            if (_expToExcel == null || refreshClass)
            {
                _expToExcel = new ExportToExcel();
                isFill = true;
            }
            expToExcel = _expToExcel;

            return isFill;
        }

        public bool GetNewsFeedClass(ref NewsFeedClass nfc)
        {
            bool isFill = false;

            if (_nfc == null)
            {
                _nfc = new NewsFeedClass();
                isFill = true;
            }
            nfc = _nfc;

            return isFill;
        }

        public bool GetProductionCalendarClass(ref ProductionCalendarClass pcc)
        {
            bool isFill = false;

            if (_pcc == null)
            {
                _pcc = new ProductionCalendarClass(App.ConnectionInfo.ConnectionString);
                isFill = true;
            }
            pcc = _pcc;

            return isFill;
        }

        public bool GetServiceEquipmentClass(ref ServiceEquipmentClass sec, bool refreshClass = false)
        {
            bool isFill = false;

            if (_sec == null || refreshClass)
            {
                _sec = new ServiceEquipmentClass(App.ConnectionInfo.ConnectionString);
                isFill = true;
            }
            sec = _sec;

            return isFill;
        }

        public bool GetPlannedScheduleClass(ref PlannedScheduleClass psc)
        {
            bool isFill = false;

            if (_psc == null)
            {
                _psc = new PlannedScheduleClass(App.ConnectionInfo.ConnectionString);
                isFill = true;
            }
            psc = _psc;

            return isFill;
        }

        public bool GetTimeSheetClass(ref TimeSheetClass tsc)
        {
            bool isFill = false;

            if (_tsc == null)
            {
                _tsc = new TimeSheetClass(App.ConnectionInfo.ConnectionString);
                isFill = true;
            }
            tsc = _tsc;

            return isFill;
        }



        public bool GetStimulationClass(ref StimulationClass stc, bool refreshClass = false)
        {
            bool isFill = false;

            if (_stc == null || refreshClass)
            {
                _stc = new StimulationClass(App.ConnectionInfo.ConnectionString);
                isFill = true;
            }
            stc = _stc;

            return isFill;
        }

        public bool GetStaffClass(ref StaffClass sc, bool refreshClass = false)
        {
            var isFill = false;

            if (_sc == null || refreshClass)
            {
                _sc = new StaffClass();
                isFill = true;
            }

            sc = _sc;

            return isFill;
        }

        public bool GetTimeTrackingClass(ref TimeTrackingClass ttc, bool refreshClass = false)
        {
            bool isFill = false;

            if (_ttc == null || refreshClass)
            {
                _ttc = new TimeTrackingClass();
                isFill = true;
            }
            ttc = _ttc;

            return isFill;
        }

        public bool GetTimeControlClass(ref TimeControlClass tcc, bool refreshClass = false)
        {
            bool isFill = false;

            if (_tcc == null || refreshClass)
            {
                _tcc = new TimeControlClass();
                isFill = true;
            }
            tcc = _tcc;

            return isFill;
        }

        public bool GetAdministrationClass(ref AdministrationClass ac, bool refreshClass = false)
        {
            bool isFill = false;

            if (_ac == null || refreshClass)
            {
                _ac = new AdministrationClass(App.ConnectionInfo.ConnectionString);
                isFill = true;
            }
            ac = _ac;

            return isFill;
        }


        public bool GetCatalogClass(ref CatalogClass cc, bool refreshClass = false)
        {
            bool isFill = false;

            if (_cc == null || refreshClass)
            {
                _cc = new CatalogClass();
                isFill = true;
            }
            cc = _cc;

            return isFill;
        }

        public bool GetTechnologyProblemClass(ref TechnologyProblemClass tpr, bool refreshClass = false)
        {
            bool isFill = false;

            if (_tpr == null || refreshClass)
            {
                _tpr = new TechnologyProblemClass(App.ConnectionInfo.ConnectionString);
                isFill = true;
            }
            tpr = _tpr;

            return isFill;
        }

        //public bool GetCompanyStructureClass(ref CompanyStructureClass csc)
        //{
        //    bool isFill = false;

        //    if (_csc == null)
        //    {
        //        _csc = new CompanyStructureClass();
        //        isFill = true;
        //    }
        //    csc = _csc;

        //    return isFill;
        //}
        
        //public bool GetProductionScheduleClass(ref ProductionScheduleClass psc)
        //{
        //    bool isFill = false;

        //    if (_prodShClass == null)
        //    {
        //        _prodShClass = new ProductionScheduleClass(App.ConnectionInfo.ConnectionString);
        //        isFill = true;
        //    }
        //    psc = _prodShClass;

        //    return isFill;
        //}

        public bool GetProdRoomsClass(ref ProdRoomsClass prc, bool refreshClass = false)
        {
            bool isFill = false;

            if (_prc == null || refreshClass)
            {
                _prc = new ProdRoomsClass(App.ConnectionInfo.ConnectionString);
                isFill = true;
            }
            prc = _prc;

            return isFill;
        }

        public bool GetWorkerRequestsClass(ref WorkerRequestsClass workerRequestsClass, bool refreshClass = false)
        {
            bool isFill = false;

            if (_workerRequestsClass == null || refreshClass)
            {
                _workerRequestsClass = new WorkerRequestsClass();
                isFill = true;
            }
            workerRequestsClass = _workerRequestsClass;

            return isFill;
        }

        public bool GetAdmissionsClass(ref AdmissionsClass admissionClass, bool refreshClass = false)
        {
            bool isFill = false;

            if (_admClass == null || refreshClass)
            {
                _admClass = new AdmissionsClass();
                isFill = true;
            }
            admissionClass = _admClass;

            return isFill;
        }

        public bool GetPlannedWorksClass(ref PlannedWorksClass plannedWorksClass, bool refreshClass = false)
        {
            bool isFill = false;

            if (_plannedWorksClass == null || refreshClass)
            {
                _plannedWorksClass = new PlannedWorksClass();
                isFill = true;
            }
            plannedWorksClass = _plannedWorksClass;

            return isFill;
        }

        public bool GetMyWorkersClass(ref MyWorkersClass mwc, bool refreshClass = false)
        {
            bool isFill = false;

            if (_mwc == null || refreshClass)
            {
                _mwc = new MyWorkersClass();
                isFill = true;
            }
            mwc = _mwc;

            return isFill;
        }

        public DateTime GetDateFromSqlServer()
        {
            DateTime newProdId;
            const string sql = "SELECT NOW()";

            using (var conn = new MySqlConnection(App.ConnectionInfo.ConnectionString))
            {
                var cmd = new MySqlCommand(sql, conn);
                conn.Open();
                newProdId = (DateTime) cmd.ExecuteScalar();
                conn.Close();
            }
            return newProdId;
        }
    }
}
