using System;
using System.Globalization;
using System.Windows.Data;

namespace FA2.Converters
{
    class BirthToAgeConverter : IValueConverter
    {
        public object Convert(object value)
        {
            DateTime birthDate;
            var currentDate = App.BaseClass.GetDateFromSqlServer();

            String age;

            if (DateTime.TryParse(value.ToString(), out birthDate))
            {
                int yearsPassed = currentDate.Year - birthDate.Year;
                if (currentDate.Month < birthDate.Month ||
                    (currentDate.Month == birthDate.Month && currentDate.Day < birthDate.Day))
                {
                    yearsPassed--;
                }

                int t = yearsPassed % 10;

                string measure = t != 1 ? (t >= 2 && t <= 4 ? "года" : "лет") : "год";

                age = "Дата рождения: " + birthDate.ToShortDateString() + " (" + yearsPassed + " " + measure + ")";
            }
            else
            {
                age = "Дата рождения: -";
            }

            return age;
        }

       public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
       {
           DateTime birthDate;
           var currentDate = App.BaseClass.GetDateFromSqlServer();

           String age;

           if (DateTime.TryParse(value.ToString(), out birthDate))
           {
               int yearsPassed = currentDate.Year - birthDate.Year;
               if (currentDate.Month < birthDate.Month ||
                   (currentDate.Month == birthDate.Month && currentDate.Day < birthDate.Day))
               {
                   yearsPassed--;
               }

               int t = yearsPassed%10;

               string measure = t != 1 ? (t >= 2 && t <= 4 ? "года" : "лет") : "год";

               age = "Дата рождения: " + birthDate.ToShortDateString() + " (" + yearsPassed + " " + measure + ")";
           }
           else
           {
               age = "Дата рождения: -";
           }

           return age;
       }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
