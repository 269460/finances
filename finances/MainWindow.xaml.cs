using finances;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

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
        public CurrencyService CurrencyService { get; private set; }
        private string _currencyRate;
        public string CurrencyRate
        {
            get => _currencyRate;
            set
            {
                if (_currencyRate != value)
                {
                    _currencyRate = value;
                    OnPropertyChanged(nameof(CurrencyRate));
                }
            }
        }

        public MainWindowViewModel()
        {
            Expenses = new ObservableCollection<Transaction>();
            Incomes = new ObservableCollection<Transaction>();
            CurrencyService = new CurrencyService("2d078771a0033dc8207c2f1861e80ba0", Dispatcher.CurrentDispatcher);
            CurrencyService.OnRateUpdated += HandleRateUpdated;
            InitializeCurrencyRate();
        }

        private async void InitializeCurrencyRate()
        {
            try
            {
                await CurrencyService.GetExchangeRateAsync("USD", "EUR"); // Example base and target currencies
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving currency rate: " + ex.Message);
            }
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

        private void HandleRateUpdated(double rate)
        {
            CurrencyRate = $"USD to EUR Rate: {rate:N2}";
            OnPropertyChanged(nameof(CurrencyRate));
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
        //private CurrencyService _currencyService;
        private TextBlock _rateDisplay;


        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
           // _currencyService = new CurrencyService();
            //_currencyService.OnRateUpdated += UpdateCurrencyDisplay;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeFields();
            using (var dbContext = new FinancialDbContext())
            {
                var financialTransactions = dbContext.FinancialTransactions.ToList<FinancialTransaction>();
                foreach (FinancialTransaction transaction in financialTransactions)
                {
                    if (transaction.Type == TransactionType.Income) { _viewModel.AddIncome(transaction.Amount, transaction.TransactionDate); }
                    if (transaction.Type == TransactionType.Expense) { _viewModel.AddExpense(transaction.Amount, transaction.TransactionDate); }
                    
                }
            }
            UpdateChart();
            // UpdateCurrencyRate();
        }

        private void InitializeFields()
        {
            _amountInput = (TextBox)FindName("amountInput");
            _comboBoxTransactionType = (ComboBox)FindName("comboBoxTransactionType");
            _datePickerTransactionDate = (DatePicker)FindName("datePickerTransactionDate");
            _lineChart = (CartesianChart)FindName("lineChart");
            _rateDisplay = (TextBlock)FindName("RateDisplay");

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
                        FinancialTransaction financialTransaction = new FinancialTransaction();
                        financialTransaction.Amount = amount;
                        financialTransaction.TransactionDate = selectedDate;
                        financialTransaction.Description = "transaction";
                        if (selectedItem.Content.ToString() == "Expense")
                        {
                            _viewModel.AddExpense(amount, selectedDate);
                            financialTransaction.Type = TransactionType.Expense;
                        }
                        else if (selectedItem.Content.ToString() == "Income")
                        {
                            _viewModel.AddIncome(amount, selectedDate);
                            financialTransaction.Type = TransactionType.Income;
                        }
                        
                        using (var dbContext = new FinancialDbContext())
                        {
                            // Dodaj lub zmodyfikuj dane
                            dbContext.FinancialTransactions.Add(financialTransaction);
                            dbContext.SaveChanges(); // Zapisz zmiany w bazie danych
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
                    Values = new ChartValues<ObservableValue>(
                        _viewModel.Expenses
                            .GroupBy(x => x.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new ObservableValue(g.Sum(t => t.Amount)))
                    ),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    Stroke = Brushes.Red
                };

                var incomeSeries = new LineSeries
                {
                    Title = "Incomes",
                    Values = new ChartValues<ObservableValue>(
                        _viewModel.Incomes
                            .GroupBy(x => x.Date.Date)
                            .OrderBy(g => g.Key)
                            .Select(g => new ObservableValue(g.Sum(t => t.Amount)))
                    ),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    Stroke = Brushes.Green
                };

                _lineChart.Series.Add(expenseSeries);
                _lineChart.Series.Add(incomeSeries);

                var dates = _viewModel.Expenses.Concat(_viewModel.Incomes).Select(x => x.Date.Date).Distinct().OrderBy(x => x);
                var labels = dates.Select(date => date.ToString("dd MMM yy")).ToArray();

                _lineChart.AxisX.Clear();
                _lineChart.AxisX.Add(new Axis
                {
                    Title = "Date",
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

        private void UpdateCurrencyDisplay(double rate)
        {
            Dispatcher.Invoke(() =>
            {
                _rateDisplay.Text = $"USD to EUR Rate: {rate:N2}";
            });
        }

        private async void UpdateCurrencyRate()
        {
            try
            {
                // Poprawka: poprawna sygnatura metody GetExchangeRateAsync
                double rate = await _viewModel.CurrencyService.GetExchangeRateAsync("USD", "EUR");
                Dispatcher.Invoke(() =>
                {
                    _rateDisplay.Text = $"USD to EUR Rate: {rate:N2}";  // Update the display with the rate
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving currency rate: " + ex.Message);
            }
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
            //UpdateCurrencyRate();  // Fetch and display the currency rate
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
