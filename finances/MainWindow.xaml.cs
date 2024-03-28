using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private TextBox? _amountInput;
    private ComboBox? comboBoxTransactionType;
    private ComboBox? _comboBoxTransactionType;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        _amountInput = FindName(name: "amountInput") as TextBox;
        _comboBoxTransactionType = FindName(name: "comboBoxTransactionType") as ComboBox;
    }

    private void InitializeComponent()
    {
        throw new NotImplementedException();
    }

    private void ButtonAddTransaction_Click(object sender, RoutedEventArgs e)
    {
        double amount;
        if (double.TryParse(_amountInput.Text, out amount))
        {
            ComboBoxItem selectedItem = (ComboBoxItem)_comboBoxTransactionType.SelectedItem;
            if (selectedItem.Content.Equals("Expense"))
            {
                _viewModel.Expenses.Add(amount);
            }
            else if (selectedItem.Content.Equals("Income"))
            {
                _viewModel.Incomes.Add(amount);
            }
        }
        else
        {
            MessageBox.Show("Invalid amount. Please enter a valid number.");
        }
    }
}

public class MainWindowViewModel : INotifyPropertyChanged
{
    private ObservableCollection<double>? _expenses;
    public ObservableCollection<double>? Expenses
    {
        get { return _expenses; }
        set
        {
            _expenses = value;
            OnPropertyChanged("Expenses");
        }
    }

    private ObservableCollection<double>? _incomes;
    public ObservableCollection<double>? Incomes
    {
        get { return _incomes; }
        set
        {
            _incomes = value;
            OnPropertyChanged("Incomes");
        }
    }

    public MainWindowViewModel()
    {
        Expenses = new ObservableCollection<double>();
        Incomes = new ObservableCollection<double>();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}