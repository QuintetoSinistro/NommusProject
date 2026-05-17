using Nommus;                      
using System;
using System.Windows;
using System.Windows.Controls;
using NommusProject.Data;          
using Microsoft.Data.Sqlite;      
using System.Collections.Generic; 
using System.Linq;              

namespace NommusProject
{
    public partial class UserLogin : Window
    {
        public UserLogin()
        {
            InitializeComponent();
        }

        // ============================================================
        // MÉTODO PRINCIPAL DE LOGIN
        // ============================================================
        // Disparado ao clicar no botão "Acessar"
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Obtém email e senha digitados
            string email = UsernameTextBox.Text.Trim();
            string senha = PasswordBox.Password;

            // Valida campos vazios
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Preencha email e senha.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Busca o usuário no banco de dados pelo email (método estático assíncrono)
                var usuario = await Usuarios.BuscarUsuarioPorEmailAsync(email);

                // Se não encontrou, exibe erro
                if (usuario == null)
                {
                    MessageBox.Show("Email não cadastrado.", "Erro de Login",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Verifica a senha usando BCrypt
                if (!usuario.VerificarSenha(senha))
                {
                    MessageBox.Show("Senha incorreta.", "Erro de Login",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Login bem-sucedido
                MessageBox.Show($"Bem-vindo, {usuario.Nome}!", "Login Sucesso",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Armazena o usuário na sessão global (acessível em toda a aplicação)
                SessaoUsuario.UsuarioLogado = usuario;

                // Abre a janela principal do sistema (MainWindow)
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Fecha a janela de login
                this.Close();
            }
            catch (Exception ex)
            {
                // Exibe qualquer erro inesperado (ex: falha de conexão com o banco)
                MessageBox.Show($"Erro ao fazer login: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        // MANIPULAÇÃO DE PLACEHOLDER DO CAMPO "USUÁRIO/EMAIL"
        // ============================================================
        // Quando o texto do campo muda, esconde ou mostra o placeholder
        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UsernamePlaceholder.Visibility = string.IsNullOrEmpty(UsernameTextBox.Text)
                ? Visibility.Visible   // mostra "Usuário" se vazio
                : Visibility.Collapsed; // esconde se tem texto
        }

        // Quando o campo ganha foco, esconde o placeholder
        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsernamePlaceholder.Visibility = Visibility.Collapsed;
        }

        // Quando o campo perde o foco, mostra o placeholder se estiver vazio
        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameTextBox.Text))
                UsernamePlaceholder.Visibility = Visibility.Visible;
        }

        // ============================================================
        // MANIPULAÇÃO DE PLACEHOLDER DO CAMPO "SENHA"
        // ============================================================
        // Quando a senha muda, esconde ou mostra o placeholder
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Ao ganhar foco, esconde o placeholder
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        // Ao perder foco, mostra o placeholder se a senha estiver vazia
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        // ============================================================
        // NAVEGAÇÃO PARA OUTRAS TELAS
        // ============================================================
        // Abre a tela de cadastro (RegisterScreen) e fecha a atual
        private void CadastrarSe_Click(object sender, RoutedEventArgs e)
        {
            RegisterScreen registerScreen = new RegisterScreen();
            registerScreen.Show();
            this.Close();
        }

        // Abre a tela de recuperação de senha (ForgotPasswordWindow) e fecha a atual
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ForgotPasswordWindow forgotSenhaScreen = new ForgotPasswordWindow();
            forgotSenhaScreen.Show();
            this.Close();
        }
    }
}