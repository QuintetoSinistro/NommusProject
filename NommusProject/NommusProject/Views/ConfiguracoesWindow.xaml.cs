using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NommusProject.Data;

namespace NommusProject
{
    public partial class ConfiguracoesWindow : Window
    {
        private readonly UsuarioRepository _usuarioRepo = new UsuarioRepository();

        private bool _editandoNome = false;
        private bool _editandoTelefone = false;
        private bool _editandoSenha = false;

        public ConfiguracoesWindow()
        {
            InitializeComponent();
            CarregarDadosUsuario();
        }

        // ============================================================
        // CARREGAR DADOS
        // ============================================================

        private void CarregarDadosUsuario()
        {
            var usuario = SessaoUsuario.UsuarioLogado;
            if (usuario == null) return;

            if (SidebarNomeText != null)
                SidebarNomeText.Text = usuario.Nome;

            NomeVisualizacao.Text = usuario.Nome;
            TelefoneVisualizacao.Text = string.IsNullOrEmpty(usuario.telefone)
                                        ? "Não informado"
                                        : usuario.telefone;
        }

        // ============================================================
        // EDITAR NOME
        // ============================================================

        private void EditarNome_Click(object sender, RoutedEventArgs e)
        {
            if (!_editandoNome)
            {
                _editandoNome = true;
                NomeEdicao.Text = NomeVisualizacao.Text;
                NomeVisualizacao.Visibility = Visibility.Collapsed;
                NomeEdicao.Visibility = Visibility.Visible;
                BtnEditarNome.Content = "Salvar";
                NomeEdicao.Focus();
            }
            else
            {
                SalvarNome();
            }
        }

        private void SalvarNome()
        {
            var novoNome = NomeEdicao.Text.Trim();
            if (string.IsNullOrEmpty(novoNome)) { MostrarFeedback("Nome não pode ser vazio.", erro: true); return; }

            try
            {
                var usuario = SessaoUsuario.UsuarioLogado;
                usuario.Nome = novoNome;
                usuario.Salvar();

                NomeVisualizacao.Text = novoNome;
                SidebarNomeText.Text = novoNome;
                NomeVisualizacao.Visibility = Visibility.Visible;
                NomeEdicao.Visibility = Visibility.Collapsed;
                BtnEditarNome.Content = "Editar";
                _editandoNome = false;
                MostrarFeedback("Nome atualizado com sucesso!", erro: false);
            }
            catch (Exception ex) { MostrarFeedback($"Erro: {ex.Message}", erro: true); }
        }

        // ============================================================
        // EDITAR TELEFONE
        // ============================================================

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

        private void SalvarTelefone()
        {
            try
            {
                var usuario = SessaoUsuario.UsuarioLogado;
                usuario.telefone = TelefoneEdicao.Text.Trim();
                usuario.Salvar();

                TelefoneVisualizacao.Text = string.IsNullOrEmpty(usuario.telefone) ? "Não informado" : usuario.telefone;
                TelefoneVisualizacao.Visibility = Visibility.Visible;
                TelefoneEdicao.Visibility = Visibility.Collapsed;
                BtnEditarTelefone.Content = "Editar";
                _editandoTelefone = false;
                MostrarFeedback("Telefone atualizado com sucesso!", erro: false);
            }
            catch (Exception ex) { MostrarFeedback($"Erro: {ex.Message}", erro: true); }
        }

        // ============================================================
        // EDITAR SENHA
        // ============================================================

        private void EditarSenha_Click(object sender, RoutedEventArgs e)
        {
            if (!_editandoSenha)
            {
                _editandoSenha = true;
                SenhaVisualizacao.Visibility = Visibility.Collapsed;
                SenhaEdicaoPanel.Visibility = Visibility.Visible;
                BtnEditarSenha.Content = "Salvar";
                SenhaAtualBox.Focus();
            }
            else
            {
                SalvarSenha();
            }
        }

        private void SalvarSenha()
        {
            if (SenhaAtualBox.Password != SessaoUsuario.UsuarioLogado?.senha)
            { MostrarFeedback("Senha atual incorreta.", erro: true); return; }

            if (string.IsNullOrEmpty(SenhaNovaBox.Password) || SenhaNovaBox.Password.Length < 6)
            { MostrarFeedback("Nova senha deve ter pelo menos 6 caracteres.", erro: true); return; }

            try
            {
                var usuario = SessaoUsuario.UsuarioLogado;
                usuario.senha = SenhaNovaBox.Password;
                usuario.Salvar();

                SenhaVisualizacao.Visibility = Visibility.Visible;
                SenhaEdicaoPanel.Visibility = Visibility.Collapsed;
                BtnEditarSenha.Content = "Editar";
                SenhaAtualBox.Clear();
                SenhaNovaBox.Clear();
                _editandoSenha = false;
                MostrarFeedback("Senha atualizada com sucesso!", erro: false);
            }
            catch (Exception ex) { MostrarFeedback($"Erro: {ex.Message}", erro: true); }
        }

        // ============================================================
        // ALTERAR FOTO
        // ============================================================

        private void AlterarFoto_Click(object sender, RoutedEventArgs e)
        {
            MostrarFeedback("Funcionalidade de foto em breve.", erro: false);
        }

        // ============================================================
        // FEEDBACK
        // ============================================================

        private async void MostrarFeedback(string mensagem, bool erro)
        {
            FeedbackText.Text = mensagem;
            FeedbackText.Foreground = erro
                ? new SolidColorBrush(Color.FromRgb(239, 68, 68))
                : new SolidColorBrush(Color.FromRgb(34, 197, 94));
            FeedbackText.Visibility = Visibility.Visible;
            await System.Threading.Tasks.Task.Delay(3000);
            FeedbackText.Visibility = Visibility.Collapsed;
        }

        // ============================================================
        // FECHAR - volta para TelaInicial
        // ============================================================

        private void FecharTela_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // ============================================================
        // NAVEGAÇÃO SIDEBAR
        // ============================================================

        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        { new MainWindow().Show(); this.Close(); }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        { MessageBox.Show("Navegar para Cartões", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information); }

        private void ExpensesButton_Click(object sender, RoutedEventArgs e)
        { new ExpensesWindow().Show(); this.Close(); }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        { new IncomeWindow().Show(); this.Close(); }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        { MessageBox.Show("Navegar para Metas", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information); }
    }
}