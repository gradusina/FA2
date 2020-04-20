using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class TechnologyProblemConveter : IValueConverter
    {
        private TechnologyProblemClass _techProblemClass;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            App.BaseClass.GetTechnologyProblemClass(ref _techProblemClass);

            if (_techProblemClass != null && parameter != null)
            {
                switch (parameter.ToString())
                {
                    case "NoteResponsibilities":
                    {
                        var techProblemNoteId = System.Convert.ToInt32(value);

                        var workers =
                            from responsibilities in
                                _techProblemClass.TechnologyProblemNotesResponsibilitiesTable.AsEnumerable()
                                    .Where(r => r.Field<Int64>("TechnologyProblemNoteID") == techProblemNoteId)
                            let workerName = new IdToNameConverter().Convert(responsibilities["WorkerID"], "ShortName")
                            select workerName;

                        if (workers.Any())
                        {
                            var resultString = workers.Aggregate(string.Empty,
                                (current, workerName) => current + workerName + ", ");
                            return resultString.Remove(resultString.Length - 2);
                        }

                        return string.Empty;
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
