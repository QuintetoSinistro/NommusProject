using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NommusProject;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class UserLogin : Window
{
    public UserLogin()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }

    private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        UsernamePlaceholder.Visibility = Visibility.Collapsed;
    }

    private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            UsernamePlaceholder.Visibility = Visibility.Visible;
    }

    private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
    {
        PasswordPlaceholder.Visibility = Visibility.Collapsed;
    }

    private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            PasswordPlaceholder.Visibility = Visibility.Visible;
    }

    private void CadastrarSe_Click(object sender, RoutedEventArgs e)
    {
        RegisterScreen registerScreen = new RegisterScreen();
        registerScreen.Show();
        this.Close();

    }



}