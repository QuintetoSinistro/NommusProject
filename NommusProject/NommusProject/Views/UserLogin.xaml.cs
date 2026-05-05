using Nommus;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public partial class UserLogin : Window
    {
        public UserLogin()
        {
            InitializeComponent();
        }

        // Método principal de login - precisa ser revisado
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string email = UsernameTextBox.Text.Trim();
            string senha = PasswordBox.Password;

            // Valida se os campos estão preenchidos
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Preencha email e senha.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Busca usuário no banco de dados pelo email
                var usuario = await Usuarios.BuscarUsuarioPorEmailAsync(email);

                // Verifica se usuário existe
                if (usuario == null)
                {
                    MessageBox.Show("Email não cadastrado.", "Erro de Login",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Verifica se a senha está correta (sem criptografia por enquanto)
                if (usuario.senha != senha)
                {
                    MessageBox.Show("Senha incorreta.", "Erro de Login",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Login bem-sucedido - verificação adicional de segurança
                if (usuario != null && usuario.senha == senha)
                {
                    MessageBox.Show($"Bem-vindo, {usuario.Nome}!", "Login Sucesso",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Define usuário na sessão e abre a tela principal
                    SessaoUsuario.UsuarioLogado = usuario;
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao fazer login: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Lógica para o Usuário
        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UsernamePlaceholder.Visibility = string.IsNullOrEmpty(UsernameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsernamePlaceholder.Visibility = Visibility.Collapsed;
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameTextBox.Text))
                UsernamePlaceholder.Visibility = Visibility.Visible;
        }

        // Lógica para a Senha
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }


        // Abre a tela de cadastro para novos usuários
        private void CadastrarSe_Click(object sender, RoutedEventArgs e)
        {
            RegisterScreen registerScreen = new RegisterScreen();
            registerScreen.Show();
            this.Close();
        }
    }
}