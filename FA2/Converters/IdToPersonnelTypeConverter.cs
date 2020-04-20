using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FA2.Converters
{
    class IdToPersonnelTypeConverter : IValueConverter
    {
       public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
       {
           int personnelTypeID;

           if (Int32.TryParse(value.ToString(), out personnelTypeID))
           {

               if (parameter.ToString() == "PersonnelTypeName")
               {
                   switch (personnelTypeID)
                   {
                       case 1:
                           return "Начальник/Бригадир";
                       case 2:
                           return "Заместитель начальника/бригадира";
                       case 3:
                           return "";
                   }
               }
               else if (parameter.ToString() == "PersonnelTypeColor")
               {
                   switch (personnelTypeID)
                   {
                       case 1:
                           return new BrushConverter().ConvertFrom("#FFD84A35");
                       case 2:
                           return new BrushConverter().ConvertFrom("#3FA646");
                       case 3:
                           return new BrushConverter().ConvertFrom("#FF3366CC");
                   }  
               }
           }
           return null;
       }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        
    }
}