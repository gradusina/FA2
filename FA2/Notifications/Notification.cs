using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;

namespace FAII.Notifications
{
    public class Notification : INotifyPropertyChanged
    {
        private string message;
        public string Message
        {
            get { return message; }

            set
            {
                if (message == value) return;
                message = value;
                OnPropertyChanged("Message");
            }
        }

        private int id;
        public int Id
        {
            get { return id; }

            set
            {
                if (id == value) return;
                id = value;
                OnPropertyChanged("Id");
            }
        }

        private Brush brush;

        public Brush Brush
        {
            get { return brush; }
            set
            {
                if(brush == value) return;
                brush = value;
                OnPropertyChanged("Brush");
            }
        }

        private ImageSource image;
        public ImageSource Image
        {
            get { return image; }

            set
            {
                if (image == value) return;
                image = value;
                OnPropertyChanged("Image");
            }
        }

        private string title;
        public string Title
        {
            get { return title; }

            set
            {
                if (title == value) return;
                title = value;
                OnPropertyChanged("Title");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Notifications : ObservableCollection<Notification> { }
}