using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NommusProject.Data;
using Microsoft.Data.Sqlite;

namespace NommusProject
{
    // Tela de cadastro de novos usuários
    public partial class RegisterScreen : Window
    {
        // Construtor: inicializa os componentes XAML e configura os placeholders dos campos
        public RegisterScreen()
        {
            InitializeComponent();
            ConfigurarPlaceholders();
        }

        // ============================================================
        // CONFIGURAÇÃO DOS PLACEHOLDERS (textos internos que somem ao digitar)
        // ============================================================

        // Associa os eventos de mudança de texto (TextChanged / PasswordChanged)
        // para mostrar/ocultar os placeholders de cada campo.
        private void ConfigurarPlaceholders()
        {
            // TextBoxes (nome, CPF, telefone, email)
            NameTextBox.TextChanged += (s, e) => TogglePlaceholder(NameTextBox, NamePlaceholder);
            CpfTextBox.TextChanged += (s, e) => TogglePlaceholder(CpfTextBox, CpfPlaceholder);
            PhoneTextBox.TextChanged += (s, e) => TogglePlaceholder(PhoneTextBox, PhonePlaceholder);
            EmailTextBox.TextChanged += (s, e) => TogglePlaceholder(EmailTextBox, EmailPlaceholder);

            // PasswordBoxes (senha e confirmação)
            PasswordBox.PasswordChanged += (s, e) => TogglePlaceholder(PasswordBox, PasswordPlaceholder);
            ConfirmPasswordBox.PasswordChanged += (s, e) => TogglePlaceholder(ConfirmPasswordBox, ConfirmPasswordPlaceholder);
        }

        // Controla a visibilidade do placeholder de um TextBox:
        // visível se o campo estiver vazio, oculto se houver texto.
        private void TogglePlaceholder(TextBox box, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(box.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        // Controla a visibilidade do placeholder de um PasswordBox:
        // visível se a senha estiver vazia, oculto se houver caracteres.
        private void TogglePlaceholder(PasswordBox box, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(box.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        // ============================================================
        // NAVEGAÇÃO
        // ============================================================

        // Redireciona para a tela de login (fecha a tela de cadastro)
        private void Login_click(object sender, RoutedEventArgs e)
        {
            UserLogin userLogin = new UserLogin();
            userLogin.Show();
            this.Close();
        }

        // ============================================================
        // PROCESSO PRINCIPAL DE CADASTRO
        // ============================================================

        // Evento do botão "Registrar": valida os dados e, se tudo estiver correto,
        // inicia o processo assíncrono de cadastro.
        private async void RegistrarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDados())
                return;

            await ProcessarCadastro();
        }

        // Executa a criação do usuário no banco de dados.
        // Verifica duplicidade de email, cria o objeto e chama o repositório.
        private async System.Threading.Tasks.Task ProcessarCadastro()
        {
            try
            {
                // Verifica se o email já está cadastrado (método estático assíncrono)
                var usuarioExistente = await Usuarios.BuscarUsuarioPorEmailAsync(EmailTextBox.Text.Trim());
                if (usuarioExistente != null)
                {
                    MessageBox.Show("Este email já está cadastrado.", "Atenção",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Cria um novo objeto Usuarios com os dados do formulário
                var novoUsuario = new Usuarios
                {
                    Nome = NameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    telefone = PhoneTextBox.Text.Trim(),
                    Tipo = TipoUsuario.Basic,
                    saldoDisponivel = 0
                };
                novoUsuario.DefinirSenha(PasswordBox.Password); // em vez de atribuir diretamente

                // Tenta salvar no banco (método assíncrono)
                bool salvou = await novoUsuario.SalvarAsync();

                if (salvou)
                {
                    MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Redireciona para a tela de login e fecha a atual
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

        // ============================================================
        // VALIDAÇÕES DOS CAMPOS
        // ============================================================

        // Valida todos os campos do formulário em sequência.
        // Retorna true apenas se todas as validações passarem.
        private bool ValidarDados()
        {
            if (!ValidarCamposObrigatorios()) return false;
            if (!ValidarCpf()) return false;
            if (!ValidarEmail()) return false;
            if (!ValidarSenhas()) return false;
            if (!ValidarTermos()) return false;
            return true;
        }

        // Verifica se nenhum campo obrigatório está vazio.
        // Campos obrigatórios: Nome, CPF, Telefone, Email, Senha, Confirmar Senha.
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

        // Valida o formato do CPF: exatamente 11 dígitos numéricos.
        // (Não faz cálculo de dígito verificador – pode ser melhorado depois)
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

        // Valida o formato do email usando a classe MailAddress.
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

        // Valida as senhas: 
        // - Devem ser iguais entre si.
        // - Devem ter no mínimo 6 caracteres.
        private bool ValidarSenhas()
        {
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("As senhas não coincidem.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("A senha deve ter pelo menos 6 caracteres.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Verifica se o checkbox de termos foi marcado.
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

        // Função auxiliar que verifica se uma string é um email válido
        // usando a classe System.Net.Mail.MailAddress.
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