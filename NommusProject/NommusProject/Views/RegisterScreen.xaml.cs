using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public partial class RegisterScreen : Window
    {
        public RegisterScreen()
        {
            InitializeComponent();
            ConfigurarPlaceholders();
        }

        // Configura os eventos para mostrar/ocultar placeholders
        private void ConfigurarPlaceholders()
        {
            // TextBoxes
            NameTextBox.TextChanged += (s, e) => TogglePlaceholder(NameTextBox, NamePlaceholder);
            CpfTextBox.TextChanged += (s, e) => TogglePlaceholder(CpfTextBox, CpfPlaceholder);
            PhoneTextBox.TextChanged += (s, e) => TogglePlaceholder(PhoneTextBox, PhonePlaceholder);
            EmailTextBox.TextChanged += (s, e) => TogglePlaceholder(EmailTextBox, EmailPlaceholder);

            // PasswordBoxes
            PasswordBox.PasswordChanged += (s, e) => TogglePlaceholder(PasswordBox, PasswordPlaceholder);
            ConfirmPasswordBox.PasswordChanged += (s, e) => TogglePlaceholder(ConfirmPasswordBox, ConfirmPasswordPlaceholder);
        }

        // Controla a visibilidade do placeholder para TextBox
        private void TogglePlaceholder(TextBox box, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(box.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        // Controla a visibilidade do placeholder para PasswordBox
        private void TogglePlaceholder(PasswordBox box, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(box.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        // Navega para a tela de login
        private void Login_click(object sender, RoutedEventArgs e)
        {
            UserLogin userLogin = new UserLogin();
            userLogin.Show();
            this.Close();
        }

        // Processa o cadastro do novo usuário
        private async void RegistrarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDados())
                return;

            await ProcessarCadastro();
        }

        // Executa o processo de cadastro do usuário
        private async System.Threading.Tasks.Task ProcessarCadastro()
        {
            try
            {
                // Verifica se o email já está cadastrado
                var usuarioExistente = await Usuarios.BuscarUsuarioPorEmailAsync(EmailTextBox.Text.Trim());
                if (usuarioExistente != null)
                {
                    MessageBox.Show("Este email já está cadastrado.", "Atenção",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Cria novo usuário
                var novoUsuario = new Usuarios
                {
                    Nome = NameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    telefone = PhoneTextBox.Text.Trim(),
                    senha = PasswordBox.Password, // TODO: Implementar criptografia
                    Tipo = TipoUsuario.Basic,
                    saldoDisponivel = 0
                };

                // Salva o usuário no banco de dados
                bool salvou = await novoUsuario.SalvarAsync();

                if (salvou)
                {
                    MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Redireciona para tela de login
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

        // Valida todos os dados do formulário de cadastro
        private bool ValidarDados()
        {
            // Verifica campos obrigatórios
            if (!ValidarCamposObrigatorios())
                return false;

            // Valida formato do CPF
            if (!ValidarCpf())
                return false;

            // Valida formato do email
            if (!ValidarEmail())
                return false;

            // Valida senhas
            if (!ValidarSenhas())
                return false;

            // Verifica aceitação dos termos
            if (!ValidarTermos())
                return false;

            return true;
        }

        // Valida se todos os campos obrigatórios foram preenchidos
        private bool ValidarCamposObrigatorios()
        {
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
            return true;
        }

        // Valida o formato do CPF
        private bool ValidarCpf()
        {
            string cpf = CpfTextBox.Text.Trim();
            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            {
                MessageBox.Show("CPF deve conter exatamente 11 dígitos numéricos.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        // Valida o formato do email
        private bool ValidarEmail()
        {
            string email = EmailTextBox.Text.Trim();
            if (!ValidarFormatoEmail(email))
            {
                MessageBox.Show("Digite um email válido.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        // Valida as senhas informadas
        private bool ValidarSenhas()
        {
            // Verifica se as senhas coincidem
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("As senhas não coincidem.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Verifica tamanho mínimo da senha
            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("A senha deve ter pelo menos 6 caracteres.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Valida se os termos foram aceitos
        private bool ValidarTermos()
        {
            if (TermsCheckBox.IsChecked != true)
            {
                MessageBox.Show("Você deve aceitar os termos de serviço.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        // Valida o formato do email usando MailAddress
        private bool ValidarFormatoEmail(string email)
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