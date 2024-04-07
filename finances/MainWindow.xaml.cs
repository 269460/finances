using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace finances
{
    public class Transaction
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Transaction> Expenses { get; set; }
        public ObservableCollection<Transaction> Incomes { get; set; }

        public MainWindowViewModel()
        {
            Expenses = new ObservableCollection<Transaction>();
            Incomes = new ObservableCollection<Transaction>();
        }

        public void AddExpense(double amount, DateTime date)
        {
            Expenses.Add(new Transaction { Amount = amount, Date = date });
            OnPropertyChanged(nameof(Expenses));
        }

        public void AddIncome(double amount, DateTime date)
        {
            Incomes.Add(new Transaction { Amount = amount, Date = date });
            OnPropertyChanged(nameof(Incomes));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private TextBox? _amountInput;
        private ComboBox? _comboBoxTransactionType;
        private DatePicker? _datePickerTransactionDate;
        private CartesianChart _lineChart;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeFields();
            UpdateChart();
        }

        private void InitializeFields()
        {
            _amountInput = (TextBox)FindName("amountInput");
            _comboBoxTransactionType = (ComboBox)FindName("comboBoxTransactionType");
            _datePickerTransactionDate = (DatePicker)FindName("datePickerTransactionDate");
            _lineChart = (CartesianChart)FindName("lineChart");

            Button addButton = (Button)FindName("buttonAddTransaction");
            addButton.Click += ButtonAddTransaction_Click;
        }

        private void ButtonAddTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (_amountInput != null && _comboBoxTransactionType != null && _datePickerTransactionDate?.SelectedDate != null)
            {
                if (double.TryParse(_amountInput.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double amount))
                {
                    ComboBoxItem selectedItem = (ComboBoxItem)_comboBoxTransactionType.SelectedItem;
                    if (selectedItem != null)
                    {
                        DateTime selectedDate = _datePickerTransactionDate.SelectedDate.Value;
                        if (selectedItem.Content.ToString() == "Expense")
                        {
                            _viewModel.AddExpense(amount, selectedDate);
                        }
                        else if (selectedItem.Content.ToString() == "Income")
                        {
                            _viewModel.AddIncome(amount, selectedDate);
                        }
                    }
                    UpdateChart();
                    // Czyszczenie formularza
                    _amountInput.Text = "";
                    _comboBoxTransactionType.SelectedIndex = -1;
                    _datePickerTransactionDate.SelectedDate = null;
                }
                else
                {
                    MessageBox.Show("Invalid amount. Please enter a valid number.");
                }
            }
        }

        private void UpdateChart()
        {
            Dispatcher.Invoke(() =>
            {
                _lineChart.Series.Clear();

                var expenseSeries = new LineSeries
                {
                    Title = "Expenses",
                    Values = new ChartValues<double>(_viewModel.Expenses.Select(x => x.Amount)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    Stroke = Brushes.Red
                };

                var incomeSeries = new LineSeries
                {
                    Title = "Incomes",
                    Values = new ChartValues<double>(_viewModel.Incomes.Select(x => x.Amount)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    Stroke = Brushes.Green
                };

                _lineChart.Series.Add(expenseSeries);
                _lineChart.Series.Add(incomeSeries);

                // Ustawienie zakresu dla osi X na podstawie dat transakcji
                var dates = _viewModel.Expenses.Select(x => x.Date).Union(_viewModel.Incomes.Select(x => x.Date)).Distinct();
                var labels = dates.OrderBy(date => date).Select(date => date.ToString("MMM yy")).ToArray();

                _lineChart.AxisX.Clear();
                _lineChart.AxisX.Add(new Axis
                {
                    Title = "Month",
                    Labels = labels,
                    LabelsRotation = 45,
                    Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false }
                });

                _lineChart.AxisY.Clear();
                _lineChart.AxisY.Add(new Axis
                {
                    Title = "Amount",
                    MinValue = 0,
                    LabelFormatter = value => value.ToString("C")
                });

                _lineChart.LegendLocation = LegendLocation.Right;
                _lineChart.InvalidateVisual();
            });
        }
    }
}


public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<double> Expenses { get; set; }
        public ObservableCollection<double> Incomes { get; set; }
        public SeriesCollection SeriesCollection { get; set; }

        public MainWindowViewModel()
        {
            Expenses = new ObservableCollection<double>();
            Incomes = new ObservableCollection<double>();
            SeriesCollection = new SeriesCollection();
           // Expenses.CollectionChanged += (s, e) => UpdateChart();
          //  Incomes.CollectionChanged += (s, e) => UpdateChart();
        }

        public void AddExpense(double amount)
        {
            Expenses.Add(amount);
        }

        public void AddIncome(double amount)
        {
            Incomes.Add(amount);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
