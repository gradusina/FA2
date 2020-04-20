using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class StimulationsConverter : IValueConverter
    {
        StimulationClass _stc;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value != DBNull.Value)
            {
                App.BaseClass.GetStimulationClass(ref _stc);

                if (_stc != null)
                {
                    switch (parameter.ToString())
                    {
                        case "StimulationType":
                        {
                            var stimTypeId = System.Convert.ToInt32(value);
                            var custView = new DataView(_stc.StimTypesDataTable, "", "StimulationTypeID",
                                DataViewRowState.CurrentRows);

                            var foundRows = custView.FindRows(stimTypeId);

                            if (foundRows.Count() != 0)
                            {
                                return foundRows[0].Row["StimulationTypeName"].ToString().ToUpperInvariant();
                            }
                        }
                            break;

                        case "StimulationName":
                        {
                            var stimId = System.Convert.ToInt32(value);
                            var custView = new DataView(_stc.StimDataTable, "", "StimulationID",
                                DataViewRowState.CurrentRows);

                            var foundRows = custView.FindRows(stimId);

                            if (foundRows.Count() != 0)
                            {
                                return foundRows[0].Row["StimulationName"].ToString();
                            }
                        }
                            break;

                        case "StimulationUnit":
                        {
                            var stimUnitId = System.Convert.ToInt32(value);
                            var custView = new DataView(_stc.StimUnitsDataTable, "", "StimulationUnitID",
                                DataViewRowState.CurrentRows);

                            var foundRows = custView.FindRows(stimUnitId);

                            if (foundRows.Count() != 0)
                            {
                                return foundRows[0].Row["StimulationUnitName"].ToString();
                            }
                        }
                            break;

                        case "StimulationSize":
                            return System.Convert.ToInt32(value) == 1 ? "часов" : "рублей";

                        case "StimulationNotes":
                        {
                            var stimId = System.Convert.ToInt32(value);
                            var custView = new DataView(_stc.StimDataTable, "", "StimulationID",
                                DataViewRowState.CurrentRows);

                            var foundRows = custView.FindRows(stimId);

                            if (foundRows.Count() != 0)
                            {
                                return foundRows[0].Row["StimulationNotes"].ToString();
                            }
                        }
                            break;

                        case "StimulationNotesWidth":
                        {
                            var stimId = System.Convert.ToInt32(value);
                            var custView = new DataView(_stc.StimDataTable, "", "StimulationID",
                                DataViewRowState.CurrentRows);

                            var foundRows = custView.FindRows(stimId);

                            if (foundRows.Count() != 0)
                            {
                                return string.IsNullOrEmpty(foundRows[0].Row["StimulationNotes"].ToString())
                                    ? 0
                                    : double.MaxValue;
                            }
                        }
                            break;

                        case "WorkerNotesWidth":
                            return string.IsNullOrEmpty(value.ToString()) ? 0 : double.MaxValue;
                    }
                }
            }
            else if (value == DBNull.Value && parameter.ToString() == "StimulationSize")
            {
                return "Предупреждение";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
