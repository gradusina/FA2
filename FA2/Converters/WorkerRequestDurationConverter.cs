using FA2.Classes.WorkerRequestsEnums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class WorkerRequestDurationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null || values[2] == null) return null;

            var intervalTypeId = 1;
            Int32.TryParse(values[0].ToString(), out intervalTypeId);

            var requestFromDate = DateTime.MinValue;
            DateTime.TryParse(values[1].ToString(), out requestFromDate);

            var requestToDate = DateTime.MinValue;
            DateTime.TryParse(values[2].ToString(), out requestToDate);

            if (parameter != null && parameter.ToString() == "AdditionalInfo")
                return GetTimeDuration((IntervalType)intervalTypeId, requestFromDate, requestToDate);

            return GetDateDuration((IntervalType)intervalTypeId, requestFromDate, requestToDate);
        }



        public static string GetDateDuration(IntervalType intervalType, DateTime requestFromDate, DateTime requestToDate)
        {
            return intervalType == IntervalType.DurringSomeDays
                ? requestFromDate.Year == requestToDate.Year
                    ? string.Format("{0:dd.MM} - {1:dd.MM.yyyy}", requestFromDate, requestToDate)
                    : string.Format("{0:dd.MM.yyyy} - {1:dd.MM.yyyy}", requestFromDate, requestToDate)
                : requestFromDate.ToString("dd.MM.yyyy");
        }

        public static string GetTimeDuration(IntervalType intervalType, DateTime requestFromDate, DateTime requestToDate)
        {
            switch(intervalType)
            {
                case IntervalType.DurringSomeHours:
                    return string.Format("{0:HH:mm} - {1:HH:mm}", requestFromDate, requestToDate);
                case IntervalType.DurringWorkingDay:
                    return "Целый рабочий день";
                case IntervalType.DurringSomeDays:
                    var duration = requestToDate.AddDays(1).Date.Subtract(requestFromDate.Date);
                    return string.Format("{0} календарных дня(ей)", duration.Days);
            }

            return null;
        }



        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
