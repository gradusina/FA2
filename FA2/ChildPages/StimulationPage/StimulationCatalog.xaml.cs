using System;
using System.Windows;
using System.Windows.Media.Animation;
using FA2.Classes;
using FA2.XamlFiles;
using FAIIControlLibrary;

namespace FA2.ChildPages.StimulationPage
{
    /// <summary>
    /// Логика взаимодействия для StimulationCatalog.xaml
    /// </summary>
    public partial class StimulationCatalog
    {
        private readonly StimulationClass _stc;

        public StimulationCatalog()
        {
            InitializeComponent();
            App.BaseClass.GetStimulationClass(ref _stc);
            FillBindings();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _stc.StimView.CustomFilter = "Available = 'TRUE'";
            StimulationsListBox.SelectedIndex = -1;
            if (StimulationsListBox.Items.Count != 0)
                StimulationsListBox.SelectedIndex = 0;
        }

        private void FillBindings()
        {
            StimulationsComboBox.ItemsSource = _stc.StimTypesDataTable.DefaultView;
            StimulationsComboBox.DisplayMemberPath = "StimulationTypeName";
            StimulationsComboBox.SelectedValuePath = "StimulationTypeID";

            StimulationsListBox.ItemsSource = _stc.StimView;
            StimulationsListBox.DisplayMemberPath = "StimulationName";
            StimulationsListBox.SelectedValuePath = "StimulationID";
        }

        private void CloseStimulationButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.HideCatalogGrid();
            }
        }

        private void StimulationsAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    AddProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
                };

                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            }
            else
            {
                AddProcedure();
            }
        }

        private void AddProcedure()
        {
            ViewGrid.Visibility = Visibility.Hidden;
            RedactorGrid.Visibility = Visibility.Visible;
            RedactorGrid.DataContext = null;
            OkStimulationButton.Visibility = Visibility.Visible;
            SaveStimulationButton.Visibility = Visibility.Hidden;

            if (StimulationsComboBox.HasItems)
                StimulationsComboBox.SelectedIndex = 0;

            NewStimulationTextBlock.Visibility = Visibility.Visible;
            FocusRectangle.Visibility = Visibility.Visible;
        }

        private void CancelStimulationButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    CancelProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
                };

                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            }
            else
            {
                CancelProcedure();
            }

            StimulationsListBox.IsEnabled = true;
        }

        private void CancelProcedure()
        {
            ViewGrid.Visibility = Visibility.Visible;
            RedactorGrid.Visibility = Visibility.Hidden;

            //if (StimulationsListBox.HasItems)
            //    StimulationsListBox.SelectedIndex = 0;

            NewStimulationTextBlock.Visibility = Visibility.Hidden;
            FocusRectangle.Visibility = Visibility.Hidden;
        }

        private void SaveStimulationButton_Click(object sender, RoutedEventArgs e)
        {
            if (StimulationsListBox.SelectedValue == null || string.IsNullOrEmpty(StimulationNameTextBox.Text) ||
                StimulationsComboBox.SelectedValue == null) return;

            var stimName = StimulationNameTextBox.Text;
            var stimTypeId = Convert.ToInt32(StimulationsComboBox.SelectedValue);
            var stimNotes = StimulationNoteTextBox.Text;

            _stc.ChangeStimulation(stimName, stimTypeId, stimNotes);
            CancelStimulationButton_Click(null, null);
        }

        private void DeleteStimulationButton_Click(object sender, RoutedEventArgs e)
        {
            if (StimulationsListBox.SelectedValue == null) return;

            if (MetroMessageBox.Show("Вы действительно хотите удалить выбранный объект?", "Удаление",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                _stc.DeleteStimulation();
        }

        private void OkStimulationButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StimulationNameTextBox.Text) || StimulationsComboBox.SelectedValue == null) return;

            var stimName = StimulationNameTextBox.Text;
            var stimTypeId = Convert.ToInt32(StimulationsComboBox.SelectedValue);
            var stimNotes = StimulationNoteTextBox.Text;

            _stc.AddStimulation(stimName, stimTypeId, stimNotes);
            CancelStimulationButton_Click(null, null);
        }

        private void ChangeStimulationButton_Click(object sender, RoutedEventArgs e)
        {
            if (StimulationsListBox.SelectedItem == null) return;

            if (AdministrationClass.AllowAnnimations)
            {
                var opacityAnnimation = new DoubleAnimation(0, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                opacityAnnimation.Completed += (s, args) =>
                {
                    EditProcedure();

                    opacityAnnimation = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 0, 0, 150)));
                    OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
                };

                OpacityGrid.BeginAnimation(OpacityProperty, opacityAnnimation);
            }
            else
            {
                EditProcedure();
            }

            StimulationsListBox.IsEnabled = false;
        }

        private void EditProcedure()
        {
            ViewGrid.Visibility = Visibility.Hidden;
            RedactorGrid.Visibility = Visibility.Visible;
            RedactorGrid.DataContext = null;
            RedactorGrid.DataContext = StimulationsListBox.SelectedItem;
            OkStimulationButton.Visibility = Visibility.Hidden;
            SaveStimulationButton.Visibility = Visibility.Visible;

            NewStimulationTextBlock.Visibility = Visibility.Hidden;
            FocusRectangle.Visibility = Visibility.Hidden;
        }

        private void StimulationsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeStimulationButton_Click(null, null);
        }
    }
}
