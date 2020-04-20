using System;
using System.Windows;
using System.Windows.Controls;

namespace FA2.Classes
{
    public class WeekNumberClass : Label
    {
        public static readonly DependencyProperty CalendarProperty =
            DependencyProperty.Register("Calendar",
                typeof(Calendar),
                typeof(WeekNumberClass),
                new FrameworkPropertyMetadata(OnCalendarChanged));

        public Calendar Calendar
        {
            set { SetValue(CalendarProperty, value); }
            get { return (Calendar)GetValue(CalendarProperty); }
        }

        static void OnCalendarChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as WeekNumberClass).OnCalendarChanged(args);
        }

        void OnCalendarChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue != null)
            {
                (args.OldValue as Calendar).DisplayModeChanged -= OnCalendarDisplayModeChanged;
                (args.OldValue as Calendar).DisplayDateChanged -= OnCalendarDisplayDateChanged;
                Content = "";
            }
            if (args.NewValue != null)
            {
                (args.NewValue as Calendar).DisplayModeChanged += OnCalendarDisplayModeChanged;
                (args.NewValue as Calendar).DisplayDateChanged += OnCalendarDisplayDateChanged;
                SetText();
            }
        }

        void OnCalendarDisplayModeChanged(object sender, CalendarModeChangedEventArgs args)
        {
            SetText();
        }

        void OnCalendarDisplayDateChanged(object sender, CalendarDateChangedEventArgs args)
        {
            SetText();
        }

        void SetText()
        {
            if (Calendar.DisplayMode != CalendarMode.Month)
            {
                Content = "";
                return;
            }
            DateTime dtCalendar = Calendar.DisplayDate;
            DateTime dtYearBegin = new DateTime(dtCalendar.Year, 1, 1);
            DateTime dtMonthBegin = new DateTime(dtCalendar.Year, dtCalendar.Month, 1);

            // Basic calculation
            int firstWeekNumber = (dtMonthBegin.DayOfYear + (int)dtYearBegin.DayOfWeek - 1) / 7;

            // Fix for month beginning on Sunday
            if (dtMonthBegin.DayOfWeek == DayOfWeek.Sunday)
                firstWeekNumber--;

            int gridRow = (int)GetValue(Grid.RowProperty);

            // fix for first week of the year
            if (dtCalendar.Month == 1 && dtMonthBegin.DayOfWeek == DayOfWeek.Sunday && gridRow == 1)
            {
                Content = "53";
            }
            // Fix for first week of next year
            else if (dtCalendar.Month == 12 && (int)dtMonthBegin.DayOfWeek < 6 && gridRow == 6)
            {
                Content = "1";
            }
            else
            {
                Content = (firstWeekNumber + gridRow).ToString();
            }
        }
    }
}
