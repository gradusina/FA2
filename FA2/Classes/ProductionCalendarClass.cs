using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using FAIIControlLibrary;
using MySql.Data.MySqlClient;

namespace FA2.Classes
{
    public class ProductionCalendarClass
    {
        #region Объявление ..

        private MySqlDataAdapter _prodCalendarDataAdapter;
        public DataTable ProdCalendarDataTable;

        //public BindingListCollectionView ProdCalendarViewSource = null;

        public DataTable ViewProdCalendarDataTable;

        private MySqlDataAdapter _holidaysDataAdapter;
        public DataTable HolidaysDataTable;

        //public BindingListCollectionView HolidaysViewSource = null;

        private readonly string _connectionString;

        #endregion

        public ProductionCalendarClass(string tConnectionString)
        {
            _connectionString = tConnectionString;
            Initialize();
        }

        private void Initialize()
        {
            Create();
            Fill();
            CreateViewProdCalendarDataTableColumns();
        }

        private void Create()
        {
            ProdCalendarDataTable = new DataTable();

            HolidaysDataTable = new DataTable();

            ViewProdCalendarDataTable = new DataTable();
        }

        private void Fill()
        {
            FillProdCalendar();
            FillHolidays();
        }

        #region Fill_tables

        private void FillProdCalendar()
        {
            ProdCalendarDataTable.DefaultView.RowFilter = string.Empty;

            try
            {
                _prodCalendarDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT ProductionCalendarID, Date, HalfYearNumber, QuarterYearNumber, CalendarDaysCount, " +
                        "NormalWorkingDaysCount, PreholidaysCount, WeekendCount, HolidaysCount, Standart40Time, " +
                        "Standart35Time FROM FAIIProdCalendar.ProductionCalendar ORDER BY Month(Date)",
                        _connectionString);
                new MySqlCommandBuilder(_prodCalendarDataAdapter);
                _prodCalendarDataAdapter.Fill(ProdCalendarDataTable);

                ProdCalendarDataTable.DefaultView.Sort = "Date";
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillHolidays()
        {
            try
            {
                _holidaysDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT HolidayID, Date, HolidayName FROM FAIIProdCalendar.Holidays ORDER BY Date",
                        _connectionString);
                new MySqlCommandBuilder(_holidaysDataAdapter);
                _holidaysDataAdapter.Fill(HolidaysDataTable);
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(
                    exp.Message +
                    "\n\n Попробуйте перезапустить приложение. В случае повторения ошибки обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        public DataView GetProdCalendar()
        {
            return ProdCalendarDataTable.AsDataView();
        }

        public DataView GetHolidays()
        {
            return HolidaysDataTable.AsDataView();
        }

        public DataView GetViewProdCalendar()
        {
            return ViewProdCalendarDataTable.AsDataView();
        }


        

        private void CreateViewProdCalendarDataTableColumns()
        {
            ViewProdCalendarDataTable.Columns.Add("Date", typeof(String));
            ViewProdCalendarDataTable.Columns.Add("HalfYearNumber", typeof(Int32));
            ViewProdCalendarDataTable.Columns.Add("QuarterYearNumber", typeof(Int32));
            ViewProdCalendarDataTable.Columns.Add("CalendarDaysCount", typeof(Int32));
            ViewProdCalendarDataTable.Columns.Add("NormalWorkingDaysCount", typeof(Int32));
            ViewProdCalendarDataTable.Columns.Add("PreholidaysCount", typeof(Int32));
            ViewProdCalendarDataTable.Columns.Add("WeekendCount", typeof(Int32));
            ViewProdCalendarDataTable.Columns.Add("HolidaysCount", typeof(Int32));
            ViewProdCalendarDataTable.Columns.Add("Standart40Time", typeof(Int32));
            ViewProdCalendarDataTable.Columns.Add("Standart35Time", typeof(Int32));
        }

        public void AddMonthDaysInfo(int year, int monthNumber, int halfYearNumber, int quarterYearNumber,
            int calendarDaysCount,
            int normalWorkingDaysCount, int preholidaysCount, int weekendCount, int holidaysCount, int standart40Time,
            int standart35Time)
        {
            DataRow dr = ProdCalendarDataTable.NewRow();

            var date = new DateTime(year, monthNumber, 1);

            dr["Date"] = date;
            dr["HalfYearNumber"] = halfYearNumber;
            dr["QuarterYearNumber"] = quarterYearNumber;
            dr["CalendarDaysCount"] = calendarDaysCount;
            dr["NormalWorkingDaysCount"] = normalWorkingDaysCount;
            dr["PreholidaysCount"] = preholidaysCount;
            dr["WeekendCount"] = weekendCount;
            dr["HolidaysCount"] = holidaysCount;
            dr["Standart40Time"] = standart40Time;
            dr["Standart35Time"] = standart35Time;

            ProdCalendarDataTable.Rows.Add(dr);
            SaveProdCalendar();
        }


        public void DeleteMonthDaysInfo(DataRow dr)
        {
            ProdCalendarDataTable.Rows.Remove(dr);
            SaveProdCalendar();
        }

        public void SaveProdCalendar()
        {
            _prodCalendarDataAdapter.Update(ProdCalendarDataTable);  
            ProdCalendarDataTable.Clear();
            ViewProdCalendarDataTable.Clear();
            
            FillProdCalendar();
        }

        public void AddHoliday(DateTime holidayDate, string holidayName)
        {
            DataRow dr = HolidaysDataTable.NewRow();
            dr["Date"] = holidayDate;
            dr["HolidayName"] = holidayName;
            HolidaysDataTable.Rows.Add(dr);

            _holidaysDataAdapter.Update(HolidaysDataTable);
            HolidaysDataTable.Clear();
            FillHolidays();

        }

        public void DeleteHoliday(DataRow dr)
        {
            HolidaysDataTable.Rows.Remove(dr);
            _holidaysDataAdapter.Update(HolidaysDataTable);
        }

        public DataTable CreateViewProdCalendarDataTable(int year)
        {
            ViewProdCalendarDataTable.Clear();

            if ((ProdCalendarDataTable == null) || (ProdCalendarDataTable.Rows.Count == 0))
            {
                return ViewProdCalendarDataTable;
            }

            DataTable tProdCalendarDataTable = ProdCalendarDataTable.Copy();

            string yearFilter = String.Format(CultureInfo.InvariantCulture.DateTimeFormat,
                "Date >= #{0}# AND Date <=#{1}#", new DateTime(year, 1, 1), new DateTime(year, 12, 30));

            tProdCalendarDataTable.DefaultView.RowFilter = yearFilter;

            if (tProdCalendarDataTable.DefaultView.Count != 0)
            {
                DataRow dr = ViewProdCalendarDataTable.NewRow();
                dr["Date"] = "Год " + year;
                dr["CalendarDaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                    .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["CalendarDaysCount"]));
                dr["NormalWorkingDaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                    .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["NormalWorkingDaysCount"]));
                dr["PreholidaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                    .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["PreholidaysCount"]));
                dr["WeekendCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                    .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["WeekendCount"]));
                dr["HolidaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                    .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["HolidaysCount"]));
                dr["Standart40Time"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                    .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["Standart40Time"]));
                dr["Standart35Time"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                    .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["Standart35Time"]));

                ViewProdCalendarDataTable.Rows.Add(dr);

                for (int i = 1; i < 3; i++)
                {
                    tProdCalendarDataTable.DefaultView.RowFilter = yearFilter + " AND HalfYearNumber=" + i;

                    DataRow vdr = ViewProdCalendarDataTable.NewRow();
                    vdr["Date"] = i + " полугодие";
                    vdr["CalendarDaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                        .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["CalendarDaysCount"]));
                    vdr["NormalWorkingDaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                        .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["NormalWorkingDaysCount"]));
                    vdr["PreholidaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                        .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["PreholidaysCount"]));
                    vdr["WeekendCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                        .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["WeekendCount"]));
                    vdr["HolidaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                        .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["HolidaysCount"]));
                    vdr["Standart40Time"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                        .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["Standart40Time"]));
                    vdr["Standart35Time"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                        .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["Standart35Time"]));

                    ViewProdCalendarDataTable.Rows.Add(vdr);

                    var qn =
                        (tProdCalendarDataTable.DefaultView.ToTable()
                            .AsEnumerable()
                            .Select(names => new {QuarterYearNumber = names.Field<Int32>("QuarterYearNumber")}))
                            .Distinct().ToArray();

                    foreach (var quarterYearNumber in qn)
                    {
                        tProdCalendarDataTable.DefaultView.RowFilter = yearFilter + " AND HalfYearNumber=" + i +
                                                                      " AND QuarterYearNumber=" +
                                                                      quarterYearNumber.QuarterYearNumber;

                        DataRow qDR = ViewProdCalendarDataTable.NewRow();
                        qDR["Date"] = quarterYearNumber.QuarterYearNumber + " квартал";
                        qDR["CalendarDaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                            .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["CalendarDaysCount"]));
                        qDR["NormalWorkingDaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                            .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["NormalWorkingDaysCount"]));
                        qDR["PreholidaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                            .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["PreholidaysCount"]));
                        qDR["WeekendCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                            .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["WeekendCount"]));
                        qDR["HolidaysCount"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                            .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["HolidaysCount"]));
                        qDR["Standart40Time"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                            .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["Standart40Time"]));
                        qDR["Standart35Time"] = tProdCalendarDataTable.DefaultView.Cast<DataRowView>()
                            .Aggregate(0, (current, pcdr) => current + Convert.ToInt32(pcdr["Standart35Time"]));
                        ViewProdCalendarDataTable.Rows.Add(qDR);

                        foreach (DataRowView drv in tProdCalendarDataTable.DefaultView)
                        {
                            DataRow mDR = ViewProdCalendarDataTable.NewRow();
                            mDR["Date"] = drv["Date"];
                            mDR["CalendarDaysCount"] = drv["CalendarDaysCount"];
                            mDR["NormalWorkingDaysCount"] = drv["NormalWorkingDaysCount"];
                            mDR["PreholidaysCount"] = drv["PreholidaysCount"];
                            mDR["WeekendCount"] = drv["WeekendCount"];
                            mDR["HolidaysCount"] = drv["HolidaysCount"];
                            mDR["Standart40Time"] = drv["Standart40Time"];
                            mDR["Standart35Time"] = drv["Standart35Time"];

                            ViewProdCalendarDataTable.Rows.Add(mDR);
                        }
                    }


                }

            }


            ProdCalendarDataTable.DefaultView.RowFilter = "";

            return ViewProdCalendarDataTable;
        }




    }
}
