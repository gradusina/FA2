using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class IdToNewsStatusConverter : IValueConverter
    {
        private NewsFeedClass _newsFeedClass;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return null;

            App.BaseClass.GetNewsFeedClass(ref _newsFeedClass);
            if (_newsFeedClass == null) return null;

            int newsStatusId;
            Int32.TryParse(value.ToString(), out newsStatusId);
            var query = _newsFeedClass.NewsStatuses.Where(ns => ns.NewsStatusId == newsStatusId);
            var newsStatuses = query as List<NewsStatus> ?? query.ToList();
            if (!newsStatuses.Any()) return null;

            switch (parameter.ToString())
            {
                case "NewsStatusName":
                    return newsStatuses.First().NewsStatusName;
                case "NewsStatusColor":
                    return newsStatuses.First().NewsStatusColor;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
