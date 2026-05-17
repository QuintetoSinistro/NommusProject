using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using NommusProject.Data;
using NommusProject.Utils;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NommusProject
{
    // Tela de configurações do perfil do usuário (nome, telefone, senha, foto)
    public partial class ConfiguracoesWindow : Window
    {
        // Repositório de usuários para acesso ao banco de dados
        private readonly UsuarioRepository _usuarioRepo = new UsuarioRepository();

        // Flags de controle para saber se cada campo está em modo de edição
        private bool _editandoNome = false;
        private bool _editandoTelefone = false;
        private bool _editandoSenha = false;

        // Construtor: inicializa os componentes XAML e carrega os dados do usuário logado
        public ConfiguracoesWindow()
        {
            InitializeComponent();
            CarregarDadosUsuario();
        }

        // ============================================================
        // CARREGAR DADOS DO USUÁRIO NA TELA
        // ============================================================

        // Exibe nome (na sidebar e no campo de visualização) e telefone
        private void CarregarDadosUsuario()
        {
            var usuario = SessaoUsuario.UsuarioLogado;
            if (usuario == null) return;

            // Sidebar (nome do usuário)
            if (SidebarNomeText != null)
                SidebarNomeText.Text = usuario.Nome;

            // Nome (modo visualização)
            NomeVisualizacao.Text = usuario.Nome;

            // Telefone (modo visualização) – se vazio, exibe "Não informado"
            TelefoneVisualizacao.Text = string.IsNullOrEmpty(usuario.telefone) ? "Não informado" : usuario.telefone;

            try
            {
                var imagePath = "pack://application:,,,/Views/Images/user.png";
                FotoPerfilBrush.ImageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            catch
            {
                // Fallback: cor sólida ou mensagem
                FotoPerfilBrush.ImageSource = null;
            }
            
            CarregarFotoPerfil(usuario.FotoPerfil);
        }

        private void CarregarFotoPerfil(string caminho)
        {
            try
            {
                if (!string.IsNullOrEmpty(caminho) && File.Exists(caminho))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(caminho, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    FotoPerfilBrush.ImageSource = bitmap;
                    FotoFallbackText.Visibility = Visibility.Collapsed; // esconde o ícone se a imagem carregar
                }
                else
                {
                    FotoPerfilBrush.ImageSource = null;
                    FotoFallbackText.Visibility = Visibility.Visible; // mostra o ícone
                }
            }
            catch
            {
                FotoPerfilBrush.ImageSource = null;
                FotoFallbackText.Visibility = Visibility.Visible;
            }
        }

        // ============================================================
        // ALTERAR FOTO 
        // ============================================================

        private void AlterarFoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp";
            if (dialog.ShowDialog() == true)
            {
                string destFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NommusApp", "Fotos");
                if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
                string destFile = Path.Combine(destFolder, $"user_{SessaoUsuario.UsuarioLogado.Id}_{DateTime.Now.Ticks}.jpg");
                File.Copy(dialog.FileName, destFile, overwrite: true);

                var usuario = SessaoUsuario.UsuarioLogado;
                usuario.FotoPerfil = destFile;
                usuario.Salvar();

                CarregarFotoPerfil(destFile);
                MostrarFeedback("Foto atualizada com sucesso!", erro: false);
            }
        }

        // ============================================================
        // EDITAR NOME
        // ============================================================

        // Botão "Editar/Salvar" do campo Nome
        private void EditarNome_Click(object sender, RoutedEventArgs e)
        {
            if (!_editandoNome)
            {
                // Entra em modo de edição
                _editandoNome = true;
                NomeEdicao.Text = NomeVisualizacao.Text;          // Copia o valor atual
                NomeVisualizacao.Visibility = Visibility.Collapsed; // Esconde o texto estático
                NomeEdicao.Visibility = Visibility.Visible;         // Mostra a caixa de texto
                BtnEditarNome.Content = "Salvar";                  // Muda texto do botão
                NomeEdicao.Focus();                                // Foca no campo
            }
            else
            {
                // Sai do modo de edição e salva
                SalvarNome();
            }
        }

        // Salva o novo nome no banco de dados e atualiza a interface
        private void SalvarNome()
        {
            var novoNome = NomeEdicao.Text.Trim();
            if (string.IsNullOrEmpty(novoNome))
            {
                MostrarFeedback("Nome não pode ser vazio.", erro: true);
                return;
            }

            try
            {
                var usuario = SessaoUsuario.UsuarioLogado;
                usuario.Nome = novoNome;
                usuario.Salvar();   // Persiste no banco via repositório

                // Atualiza os controles de visualização
                NomeVisualizacao.Text = novoNome;
                SidebarNomeText.Text = novoNome;

                // Restaura o modo de visualização
                NomeVisualizacao.Visibility = Visibility.Visible;
                NomeEdicao.Visibility = Visibility.Collapsed;
                BtnEditarNome.Content = "Editar";
                _editandoNome = false;

                MostrarFeedback("Nome atualizado com sucesso!", erro: false);
            }
            catch (Exception ex)
            {
                MostrarFeedback($"Erro: {ex.Message}", erro: true);
            }
        }

        // ============================================================
        // EDITAR TELEFONE
        // ============================================================

        // Botão "Editar/Salvar" do campo Telefone
        private void EditarTelefone_Click(object sender, RoutedEventArgs e)
        {
            if (!_editandoTelefone)
            {
                _editandoTelefone = true;
                TelefoneEdicao.Text = SessaoUsuario.UsuarioLogado?.telefone ?? "";
                TelefoneVisualizacao.Visibility = Visibility.Collapsed;
                TelefoneEdicao.Visibility = Visibility.Visible;
                BtnEditarTelefone.Content = "Salvar";
                TelefoneEdicao.Focus();
            }
            else
            {
                SalvarTelefone();
            }
        }

        // Salva o novo telefone no banco
        private void SalvarTelefone()
        {
            try
            {
                var usuario = SessaoUsuario.UsuarioLogado;
                usuario.telefone = TelefoneEdicao.Text.Trim();
                usuario.Salvar();

                // Atualiza visualização
                TelefoneVisualizacao.Text = string.IsNullOrEmpty(usuario.telefone)
                                            ? "Não informado"
                                            : usuario.telefone;
                TelefoneVisualizacao.Visibility = Visibility.Visible;
                TelefoneEdicao.Visibility = Visibility.Collapsed;
                BtnEditarTelefone.Content = "Editar";
                _editandoTelefone = false;

                MostrarFeedback("Telefone atualizado com sucesso!", erro: false);
            }
            catch (Exception ex)
            {
                MostrarFeedback($"Erro: {ex.Message}", erro: true);
            }
        }

        // ============================================================
        // EDITAR SENHA
        // ============================================================

        // Botão "Editar/Salvar" do campo Senha
        private void EditarSenha_Click(object sender, RoutedEventArgs e)
        {
            if (!_editandoSenha)
            {
                _editandoSenha = true;
                SenhaVisualizacao.Visibility = Visibility.Collapsed;   // Esconde os asteriscos
                SenhaEdicaoPanel.Visibility = Visibility.Visible;      // Mostra os campos de senha
                BtnEditarSenha.Content = "Salvar";
                SenhaAtualBox.Focus();
            }
            else
            {
                SalvarSenha();
            }
        }

        // Salva a nova senha após validar a senha atual e as regras
        private void SalvarSenha()
        {
            // Verifica se a senha atual está correta
            if (SenhaAtualBox.Password != SessaoUsuario.UsuarioLogado?.senha)
            {
                MostrarFeedback("Senha atual incorreta.", erro: true);
                return;
            }

            // Verifica se a nova senha tem pelo menos 6 caracteres
            if (string.IsNullOrEmpty(SenhaNovaBox.Password) || SenhaNovaBox.Password.Length < 6)
            {
                MostrarFeedback("Nova senha deve ter pelo menos 6 caracteres.", erro: true);
                return;
            }

            try
            {
                var usuario = SessaoUsuario.UsuarioLogado;
                usuario.DefinirSenha(SenhaNovaBox.Password);
                usuario.Salvar();   // Persiste a nova senha

                // Restaura o modo de visualização
                SenhaVisualizacao.Visibility = Visibility.Visible;
                SenhaEdicaoPanel.Visibility = Visibility.Collapsed;
                BtnEditarSenha.Content = "Editar";
                SenhaAtualBox.Clear();
                SenhaNovaBox.Clear();
                _editandoSenha = false;

                MostrarFeedback("Senha atualizada com sucesso!", erro: false);
            }
            catch (Exception ex)
            {
                MostrarFeedback($"Erro: {ex.Message}", erro: true);
            }
        }

        // ============================================================
        // FEEDBACK VISUAL (mensagens temporárias)
        // ============================================================

        // Exibe uma mensagem de feedback (sucesso/erro) por 3 segundos
        private async void MostrarFeedback(string mensagem, bool erro)
        {
            FeedbackText.Text = mensagem;
            FeedbackText.Foreground = erro
                ? new SolidColorBrush(Color.FromRgb(239, 68, 68))   // vermelho para erro
                : new SolidColorBrush(Color.FromRgb(34, 197, 94));  // verde para sucesso
            FeedbackText.Visibility = Visibility.Visible;
            await System.Threading.Tasks.Task.Delay(3000);  // Aguarda 3 segundos
            FeedbackText.Visibility = Visibility.Collapsed;
        }

        // ============================================================
        // FECHAR – VOLTA PARA A TELA INICIAL
        // ============================================================

        // Botão "✕" (fechar a tela de configurações e mostrar a dashboard)
        private void FecharTela_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // ============================================================
        // NAVEGAÇÃO PELA SIDEBAR (menu lateral)
        // ============================================================

        // Os botões da sidebar permitem navegar para outras telas.
        // Cada método fecha a tela atual e abre a tela correspondente.

        // Botão "Finanças" → abre MainWindow
        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // Botão "Cartões" → abre a tela de cartões (namespace Views.cartoes)
        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            new NommusProject.Views.cartoes().Show();
            this.Close();
        }

        // Botão "Despesas" → abre ExpensesWindow
        private void ExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            new ExpensesWindow().Show();
            this.Close();
        }

        // Botão "Receitas" → abre IncomeWindow
        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            new IncomeWindow().Show();
            this.Close();
        }

        // Botão "Metas" → abre MetasWindow
        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            new MetasWindow().Show();
            this.Close();
        }
    }
}