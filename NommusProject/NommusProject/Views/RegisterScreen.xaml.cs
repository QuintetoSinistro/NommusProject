using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NommusProject
{
    public partial class RegisterScreen : Window
    {
        public RegisterScreen()
        {
            InitializeComponent();

            // TextBoxes
            NameTextBox.TextChanged += (s, e) => TogglePlaceholder(NameTextBox, NamePlaceholder);
            CpfTextBox.TextChanged += (s, e) => TogglePlaceholder(CpfTextBox, CpfPlaceholder);
            PhoneTextBox.TextChanged += (s, e) => TogglePlaceholder(PhoneTextBox, PhonePlaceholder);
            EmailTextBox.TextChanged += (s, e) => TogglePlaceholder(EmailTextBox, EmailPlaceholder);

            // PasswordBoxes
            PasswordBox.PasswordChanged += (s, e) => TogglePlaceholder(PasswordBox, PasswordPlaceholder);
            ConfirmPasswordBox.PasswordChanged += (s, e) => TogglePlaceholder(ConfirmPasswordBox, ConfirmPasswordPlaceholder);
        }

        private void TogglePlaceholder(TextBox box, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(box.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void TogglePlaceholder(PasswordBox box, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(box.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        private void Login_click(object sender, RoutedEventArgs e)
        {
            UserLogin userLogin = new UserLogin();
            userLogin.Show();
            this.Close();
        }

        // ADICIONAR ESTE MÉTODO - evento de clique do botão Registrar
        private async void RegistrarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDados())
                return;

            try
            {
                // Verificar se email já existe
                var usuarioExistente = await Usuario.BuscarUsuarioPorEmailAsync(EmailTextBox.Text.Trim());
                if (usuarioExistente != null)
                {
                    MessageBox.Show("Este email já está cadastrado.", "Atenção",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Criar novo usuário
                var novoUsuario = new Usuario
                {
                    Nome = NameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    telefone = PhoneTextBox.Text.Trim(),
                    senha = PasswordBox.Password, // Sem criptografia por enquanto
                    Tipo = TipoUsuario.Basic,
                    saldoDisponivel = 0
                };

                // Salvar usuário
                bool salvou = await novoUsuario.SalvarUsuarioAsync();

                if (salvou)
                {
                    MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Ir para tela de login
                    UserLogin loginWindow = new UserLogin();
                    loginWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Erro ao cadastrar usuário. Tente novamente.", "Erro",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ADICIONAR ESTE MÉTODO - validações
        private bool ValidarDados()
        {
            // 1. Campos obrigatórios
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(CpfTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Preencha todos os campos obrigatórios.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 2. CPF com 11 dígitos numéricos
            string cpf = CpfTextBox.Text.Trim();
            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            {
                MessageBox.Show("CPF deve conter exatamente 11 dígitos numéricos.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 3. Email válido
            string email = EmailTextBox.Text.Trim();
            if (!ValidarEmail(email))
            {
                MessageBox.Show("Digite um email válido.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 4. Senhas coincidem
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("As senhas não coincidem.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 5. Senha tem pelo menos 6 caracteres
            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("A senha deve ter pelo menos 6 caracteres.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 6. Termos aceitos
            if (TermsCheckBox.IsChecked != true)
            {
                MessageBox.Show("Você deve aceitar os termos de serviço.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // ADICIONAR ESTE MÉTODO - validação de email
        private bool ValidarEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}