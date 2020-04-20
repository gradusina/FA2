using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    public class WorkerProfessionInfoConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            var workerId = System.Convert.ToInt32(values[0]);
            var factoryId = System.Convert.ToInt64(values[1]);

            StaffClass sc = null;

            App.BaseClass.GetStaffClass(ref sc);

            var workerProfessionsByWorkerId =
                (sc.WorkerProfessionsDataTable.AsEnumerable().Where(
                    r => r.Field<Int64>("WorkerID") == workerId));

            if (workerProfessionsByWorkerId.Count() > 1)
            {
                workerProfessionsByWorkerId =
                 (sc.WorkerProfessionsDataTable.AsEnumerable().Where(
                     r => r.Field<Int64>("WorkerID") == workerId &&
                          r.Field<Int64>("FactoryID") == factoryId));
            }

            if (!workerProfessionsByWorkerId.Any()) return null;

            if (parameter.ToString() == "ProfessionName")
            {
                var maxRate =
                    workerProfessionsByWorkerId.Max(m => m.Field<Decimal>("Rate"));
                workerProfessionsByWorkerId =
                    workerProfessionsByWorkerId.Where(r => r.Field<Decimal>("Rate") == maxRate);

                EnumerableRowCollection<DataRow> id = workerProfessionsByWorkerId;

                var professionNamesByWorkerId =
                    (sc.ProfessionsDataTable.AsEnumerable().Where(
                        pidt => id.AsEnumerable().Any(
                            x => x.Field<Int64>("ProfessionID") == pidt.Field<Int64>("ProfessionID"))));

                if (professionNamesByWorkerId.Count() != 0)
                {
                    var profName = professionNamesByWorkerId.CopyToDataTable();
                    string professionName = profName.Select()[0]["ProfessionName"].ToString();
                    result = professionName;
                }
            }

            if (parameter.ToString() == "Rate")
            {
                var workerProfessions = workerProfessionsByWorkerId.CopyToDataTable();

                decimal rate =
                    workerProfessions.DefaultView.Cast<DataRowView>()
                        .Where(
                            dataRow =>
                                dataRow["Rate"] != DBNull.Value)
                        .Aggregate<DataRowView, decimal>(0,
                            (current, dataRow) => current + System.Convert.ToDecimal(dataRow["Rate"]));

                result = rate.ToString(CultureInfo.InvariantCulture);
            }

            if (parameter.ToString() == "Category")
            {
                var maxRate =
                    workerProfessionsByWorkerId.Max(m => m.Field<Decimal>("Rate"));
                workerProfessionsByWorkerId =
                    workerProfessionsByWorkerId.Where(r => r.Field<Decimal>("Rate") == maxRate);

                if (!workerProfessionsByWorkerId.Any()) return result;

                var profName = workerProfessionsByWorkerId.CopyToDataTable();
                string category = profName.Select()[0]["Category"].ToString();
                result = category;
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
