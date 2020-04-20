using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;

namespace FA2.ChildPages.StaffPage
{
    /// <summary>
    /// Логика взаимодействия для EditStaffContact.xaml
    /// </summary>
    public partial class EditStaffContact : Page
    {
        private StaffClass _sc;
        int _workerId;

        public EditStaffContact(int workerId)
        {
            InitializeComponent();
            App.BaseClass.GetStaffClass(ref _sc);
            _workerId = workerId;
            BindingData();
        }

        private void BindingData()
        {
            var adressesView = _sc.GetStaffAdresses();
            adressesView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            AdressesListBox.ItemsSource = adressesView;

            BindingAdressesTypesComboBox();

            var contactsView = _sc.GetStaffContacts();
            contactsView.RowFilter = string.Format("WorkerID = {0}", _workerId);
            ContactsListBox.ItemsSource = contactsView;

            BindingContactsTypesComboBox();

            if (AdressesListBox.Items.Count != 0)
                AdressesListBox.SelectedIndex = 0;
            if (ContactsListBox.Items.Count != 0)
                ContactsListBox.SelectedIndex = 0;
        }

        private void BindingAdressesTypesComboBox()
        {
            AdressesTypeComboBox.ItemsSource = _sc.GetStaffAdressTypes();
            AdressesTypeComboBox.SelectedValuePath = "StaffAdressTypeID";
            AdressesTypeComboBox.DisplayMemberPath = "StaffAdressTypeName";
        }

        private void BindingContactsTypesComboBox()
        {
            ContactsTypeComboBox.ItemsSource = _sc.GetStaffContactTypes();
            ContactsTypeComboBox.SelectedValuePath = "ContactTypeID";
            ContactsTypeComboBox.DisplayMemberPath = "ContactTypeName";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if(mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }



        #region Adresses

        private void SaveAdressesButton_Click(object sender, RoutedEventArgs e)
        {
            var workerAdress = AdressesListBox.SelectedItem as DataRowView;
            if (workerAdress == null) return;

            if (AdressesTypeComboBox.SelectedItem == null || string.IsNullOrEmpty(AdressTextBox.Text)) return;

            var staffAdressId = Convert.ToInt64(workerAdress["StaffAdressID"]);
            var adress = AdressTextBox.Text;
            var adressTypeId = Convert.ToInt32(AdressesTypeComboBox.SelectedValue);

            _sc.ChangeStaffAdress(staffAdressId, adressTypeId, adress);

            CancelAdressButton_Click(null, null);
        }

        private void DeleteAdressButton_Click(object sender, RoutedEventArgs e)
        {
            var workerAdress = AdressesListBox.SelectedItem as DataRowView;
            if (workerAdress == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранную запись?", "Удаление",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var staffAdressId = Convert.ToInt64(workerAdress["StaffAdressID"]);
            _sc.DeleteStaffAdress(staffAdressId);
        }

        private void AddAdressButton_Click(object sender, RoutedEventArgs e)
        {
            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    AddAdressProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    AdressesOpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
                };

                AdressesOpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            }
            else
            {
                AddAdressProcedure();
            }
        }

        private void AddAdressProcedure()
        {
            AdressesViewGrid.Visibility = Visibility.Hidden;
            AdressesRedactorGrid.Visibility = Visibility.Visible;
            OkAdressButton.Visibility = Visibility.Visible;
            SaveAdressesButton.Visibility = Visibility.Hidden;
            AdressesRedactorGrid.DataContext = null;
            var operations = BindingOperations.GetBindingExpression(AdressTextBox, TextBox.TextProperty);
            if (operations != null)
                operations.UpdateTarget();
            if (AdressesTypeComboBox.HasItems)
                AdressesTypeComboBox.SelectedIndex = 0;
        }

        private void CancelAdressButton_Click(object sender, RoutedEventArgs e)
        {
            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    CancelProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    AdressesOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                AdressesOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                CancelProcedure();
            }
        }

        private void CancelProcedure()
        {
            AdressesViewGrid.Visibility = Visibility.Visible;
            AdressesRedactorGrid.Visibility = Visibility.Hidden;
            AdressesListBox.IsEnabled = true;
            if (AdressesListBox.HasItems)
                AdressesListBox.SelectedIndex = 0;
        }

        private void OkAdressButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdressesTypeComboBox.SelectedItem == null || string.IsNullOrEmpty(AdressTextBox.Text)) return;

            int adressTypeId = Convert.ToInt32(AdressesTypeComboBox.SelectedValue);
            string adressText = AdressTextBox.Text;
            _sc.AddNewStaffAdress(_workerId, adressTypeId, adressText);

            CancelAdressButton_Click(null, null);
        }

        private void EditAdresButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdressesListBox.SelectedItem == null) return;

            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    EditProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    AdressesOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                AdressesOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                EditProcedure();
            }
            AdressesListBox.IsEnabled = false;
        }

        private void EditProcedure()
        {
            AdressesViewGrid.Visibility = Visibility.Hidden;
            AdressesRedactorGrid.Visibility = Visibility.Visible;
            OkAdressButton.Visibility = Visibility.Hidden;
            SaveAdressesButton.Visibility = Visibility.Visible;
            AdressesRedactorGrid.DataContext = AdressesListBox.SelectedItem;
        }

        private void AdressesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditAdresButton_Click(null, null);
        }

        #endregion


        #region Contacts

        private void AddContactButton_Click(object sender, RoutedEventArgs e)
        {
            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                var heightAnnimation = new DoubleAnimation(350, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                heightAnnimation.Completed += (s, args) =>
                {
                    AddContactProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    ContacntsOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                BeginAnimation(Control.HeightProperty, heightAnnimation);
                ContacntsOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                AddContactProcedure();
            }
        }

        private void AddContactProcedure()
        {
            ContactsViewGrid.Visibility = Visibility.Hidden;
            ContactsRedactorGrid.Visibility = Visibility.Visible;
            OkContactButton.Visibility = Visibility.Visible;
            SaveContactsButton.Visibility = Visibility.Hidden;
            ContactsRedactorGrid.DataContext = null;
            var operations = BindingOperations.GetBindingExpression(ContactTextBox, TextBox.TextProperty);
            if (operations != null)
                operations.UpdateTarget();
            if (ContactsTypeComboBox.HasItems)
                ContactsTypeComboBox.SelectedIndex = 0;
        }

        private void CancelContactButton_Click(object sender, RoutedEventArgs e)
        {
            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                var heightAnnimation = new DoubleAnimation(450, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                heightAnnimation.Completed += (s, args) =>
                {
                    CancelContactProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    ContacntsOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                BeginAnimation(Control.HeightProperty, heightAnnimation);
                ContacntsOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                CancelContactProcedure();
            }
        }

        private void CancelContactProcedure()
        {
            ContactsViewGrid.Visibility = Visibility.Visible;
            ContactsRedactorGrid.Visibility = Visibility.Hidden;
            if (ContactsListBox.HasItems)
                ContactsListBox.SelectedIndex = 0;
            ContactsListBox.IsEnabled = true;
        }

        private void SaveContactsButton_Click(object sender, RoutedEventArgs e)
        {
            var staffContact = ContactsListBox.SelectedItem as DataRowView;
            if (staffContact == null) return;

            if (ContactsTypeComboBox.SelectedItem == null || string.IsNullOrEmpty(ContactTextBox.Text)) return;

            var staffContactId = Convert.ToInt64(staffContact["StaffContactID"]);
            var contactTypeId = Convert.ToInt32(ContactsTypeComboBox.SelectedValue);
            var contactText = ContactTextBox.Text;

            _sc.ChangeStaffContact(staffContactId, contactTypeId, contactText);

            CancelContactButton_Click(null, null);
        }

        private void DeleteContactButton_Click(object sender, RoutedEventArgs e)
        {
            var staffContact = ContactsListBox.SelectedItem as DataRowView;
            if (staffContact == null) return;

            if (MessageBox.Show("Вы действительно хотите удалить выбранную запись?", "Удаление", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var staffContactId = Convert.ToInt64(staffContact["StaffContactID"]);
            _sc.DeleteStaffContact(staffContactId);
        }

        private void OkContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContactsTypeComboBox.SelectedItem == null || string.IsNullOrEmpty(ContactTextBox.Text)) return;

            int contactTypeId = Convert.ToInt32(ContactsTypeComboBox.SelectedValue);
            string contactText = ContactTextBox.Text;
            _sc.AddNewStaffContact(_workerId, contactTypeId, contactText);

            CancelContactButton_Click(null, null);
        }

        private void EditContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContactsListBox.SelectedItem == null) return;

            if(AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                var heightAnnimation = new DoubleAnimation(350, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                heightAnnimation.Completed += (s, args) =>
                {
                    EditContactProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    ContacntsOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
                };

                BeginAnimation(Control.HeightProperty, heightAnnimation);
                ContacntsOpacityGrid.BeginAnimation(Control.OpacityProperty, opacityAnnimation);
            }
            else
            {
                EditContactProcedure();
            }
            
            ContactsListBox.IsEnabled = false;
        }

        private void EditContactProcedure()
        {
            ContactsViewGrid.Visibility = Visibility.Hidden;
            ContactsRedactorGrid.Visibility = Visibility.Visible;
            OkContactButton.Visibility = Visibility.Hidden;
            SaveContactsButton.Visibility = Visibility.Visible;
            ContactsRedactorGrid.DataContext = ContactsListBox.SelectedItem;
            if (ContactsTypeComboBox.HasItems)
                ContactsTypeComboBox.SelectedIndex = 0;
        }

        private void ContactsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditContactButton_Click(null, null);
        }

        #endregion
    }
}
