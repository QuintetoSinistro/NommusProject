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

    // ALTERAR ESTE MÉTODO - Login
    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        string email = UsernameTextBox.Text.Trim();
        string senha = PasswordBox.Password;

        // Validações básicas
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            MessageBox.Show("Preencha email e senha.", "Atenção",
                          MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Buscar usuário pelo email
            var usuario = await Usuario.BuscarUsuarioPorEmailAsync(email);

            if (usuario == null)
            {
                MessageBox.Show("Email não cadastrado.", "Erro de Login",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Verificar senha (sem criptografia por enquanto)
            if (usuario.senha != senha)
            {
                MessageBox.Show("Senha incorreta.", "Erro de Login",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Login bem-sucedido
            MessageBox.Show($"Bem-vindo, {usuario.Nome}!", "Login Sucesso",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            // Abrir tela principal - AJUSTE CONFORME SUA TELA INICIAL
            Nommus.MainWindow mainWindow = new Nommus.MainWindow();
            mainWindow.Show();
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao fazer login: {ex.Message}", "Erro",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
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