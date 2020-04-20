using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FA2.Classes;

namespace FA2.Converters
{
    class CommentsConverter : IValueConverter
    {
        private NewsFeedClass _newsFeedClass;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            App.BaseClass.GetNewsFeedClass(ref _newsFeedClass);

            if (_newsFeedClass != null && value != null)
            {
                if (parameter != null)
                {
                    switch (parameter.ToString())
                    {
                        case "CommentAttachmentsList":
                        {
                            int commentId;
                            var success = Int32.TryParse(value.ToString(), out commentId);
                            if (success)
                            {
                                var commentAttachmentsView = _newsFeedClass.CommentsAttachments.AsDataView();
                                commentAttachmentsView.RowFilter = string.Format("CommentID = {0}", commentId);
                                return commentAttachmentsView;
                            }
                            break;
                        }

                        case "EditCommentButtonsEnable":
                        {
                            int commentId;
                            var success = Int32.TryParse(value.ToString(), out commentId);
                            if (success)
                            {
                                var rows =
                                    _newsFeedClass.Comments.AsEnumerable()
                                        .Where(c => c.Field<Int64>("CommentID") == commentId);
                                if (rows.Any())
                                {
                                    var comment = rows.First();
                                    var workerId = System.Convert.ToInt32(comment["WorkerID"]);
                                    return workerId == AdministrationClass.CurrentWorkerId;
                                }
                            }
                            break;
                        }

                        case "IsNewComment":
                        {
                            var periodForNewComment = TimeSpan.FromMinutes(2);
                            int commentId;
                            var success = Int32.TryParse(value.ToString(), out commentId);
                            if (success)
                            {
                                var rows =
                                    _newsFeedClass.Comments.AsEnumerable()
                                        .Where(c => c.Field<Int64>("CommentID") == commentId);
                                if (rows.Any())
                                {
                                    var comment = rows.First();
                                    var workerId = System.Convert.ToInt32(comment["WorkerID"]);
                                    if (workerId != AdministrationClass.CurrentWorkerId)
                                    {
                                        var commentDate = System.Convert.ToDateTime(comment["CommentDate"]);
                                        var currentModuleId = AdministrationClass.CurrentModuleId;
                                        var lastUpdate = AdministrationClass.LastModuleExit(currentModuleId);
                                        var updateTimeLimit = lastUpdate != DateTime.MinValue
                                            ? lastUpdate.Subtract(periodForNewComment)
                                            : DateTime.MinValue;

                                        return commentDate > updateTimeLimit;
                                    }
                                }
                            }
                            break;
                        }
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
