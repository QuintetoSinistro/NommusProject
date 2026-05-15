using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NommusProject.Views
{
    public partial class cartoes : Window
    {
        private bool _popupAberto = false;
        private List<CartaoViewModel> _cartoes = new List<CartaoViewModel>();
        private int _proximoId = 1;

        // Paleta de cores dos cartões — rotaciona automaticamente
        private readonly (string Inicio, string Fim)[] _paletas =
        {
            ("#1E3A8A", "#3B82F6"),  // azul
            ("#064E3B", "#10B981"),  // verde
            ("#4C1D95", "#8B5CF6"),  // roxo
            ("#7F1D1D", "#EF4444"),  // vermelho
            ("#78350F", "#F59E0B"),  // âmbar
            ("#0C4A6E", "#0EA5E9"),  // ciano
            ("#1E1B4B", "#6366F1"),  // índigo
        };

        public cartoes()
        {
            InitializeComponent();
            CarregarUsuario();
        }

        // ============================================================
        // USUÁRIO
        // ============================================================

        private void CarregarUsuario()
        {
            var u = SessaoUsuario.UsuarioLogado;
            if (u == null) return;
            UsuarioNomeText.Text = u.Nome;
            PopupNomeText.Text = u.Nome;
            PopupEmailText.Text = u.Email;
        }

        // ============================================================
        // ABRIR / FECHAR POPUP NOVO CARTÃO
        // ============================================================

        private void AdicionarCartao_Click(object sender, RoutedEventArgs e)
        {
            NumeroCartaoBox.Text = "";
            BancoComboBox.SelectedIndex = -1;
            BancoCustomBox.Text = "";
            BancoCustomBox.Visibility = Visibility.Collapsed;
            PreviewNumero.Text = "•••• •••• •••• ••••";
            PreviewBanco.Text = "Nome do banco";
            ErroText.Visibility = Visibility.Collapsed;

            NovoCartaoPopup.Visibility = Visibility.Visible;
            NovoCartaoOverlay.Visibility = Visibility.Visible;
        }

        private void FecharNovoCartao_Click(object sender, RoutedEventArgs e)
        {
            NovoCartaoPopup.Visibility = Visibility.Collapsed;
            NovoCartaoOverlay.Visibility = Visibility.Collapsed;
        }

        // ============================================================
        // PREVIEW AO DIGITAR
        // ============================================================

        private void NumeroCartao_TextChanged(object sender, TextChangedEventArgs e)
        {
            var raw = NumeroCartaoBox.Text.Replace(" ", "").Replace("-", "");

            // Formata em grupos de 4
            var formatado = "";
            for (int i = 0; i < raw.Length && i < 16; i++)
            {
                if (i > 0 && i % 4 == 0) formatado += " ";
                formatado += raw[i];
            }

            // Evita loop de TextChanged
            if (NumeroCartaoBox.Text != formatado)
            {
                NumeroCartaoBox.TextChanged -= NumeroCartao_TextChanged;
                NumeroCartaoBox.Text = formatado;
                NumeroCartaoBox.CaretIndex = formatado.Length;
                NumeroCartaoBox.TextChanged += NumeroCartao_TextChanged;
            }

            // Atualiza preview: mostra dígitos reais, máscara o resto
            var preview = "";
            for (int i = 0; i < 16; i++)
            {
                if (i > 0 && i % 4 == 0) preview += " ";
                preview += i < raw.Length ? raw[i] : '•';
            }
            PreviewNumero.Text = preview;
        }

        private void Banco_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BancoComboBox.SelectedItem is ComboBoxItem item)
            {
                var banco = item.Content.ToString();
                PreviewBanco.Text = banco;

                // Mostra campo personalizado se "Outro"
                BancoCustomBox.Visibility = banco == "Outro"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if (banco != "Outro")
                    BancoCustomBox.Text = "";
            }
        }

        private void BancoCustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(BancoCustomBox.Text))
                PreviewBanco.Text = BancoCustomBox.Text;
        }

        // ============================================================
        // SALVAR CARTÃO
        // ============================================================

        private void SalvarCartao_Click(object sender, RoutedEventArgs e)
        {
            ErroText.Visibility = Visibility.Collapsed;

            // Validações
            var numeroRaw = NumeroCartaoBox.Text.Replace(" ", "");
            if (numeroRaw.Length != 16)
            {
                ErroText.Text = "Digite os 16 dígitos do cartão.";
                ErroText.Visibility = Visibility.Visible;
                return;
            }

            if (!numeroRaw.All(char.IsDigit))
            {
                ErroText.Text = "O número do cartão deve conter apenas dígitos.";
                ErroText.Visibility = Visibility.Visible;
                return;
            }

            string banco;
            if (BancoComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                banco = selectedItem.Content.ToString() == "Outro"
                    ? BancoCustomBox.Text.Trim()
                    : selectedItem.Content.ToString();
            }
            else
            {
                ErroText.Text = "Selecione o banco do cartão.";
                ErroText.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrEmpty(banco))
            {
                ErroText.Text = "Digite o nome do banco.";
                ErroText.Visibility = Visibility.Visible;
                return;
            }

            // Cria o cartão
            var paleta = _paletas[(_cartoes.Count) % _paletas.Length];
            var cartao = new CartaoViewModel
            {
                Id = _proximoId++,
                Banco = banco,
                NumeroFormatado = FormatarNumero(numeroRaw),
                Bandeira = DetectarBandeira(numeroRaw),
                CorInicio = paleta.Inicio,
                CorFim = paleta.Fim
            };

            _cartoes.Add(cartao);
            AtualizarLista();

            NovoCartaoPopup.Visibility = Visibility.Collapsed;
            NovoCartaoOverlay.Visibility = Visibility.Collapsed;
        }

        // ============================================================
        // EXCLUIR CARTÃO
        // ============================================================

        private void ExcluirCartao_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var resultado = MessageBox.Show(
                    "Deseja remover este cartão?", "Confirmar exclusão",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    _cartoes.RemoveAll(c => c.Id == id);
                    AtualizarLista();
                }
            }
        }

        // ============================================================
        // ATUALIZAR LISTA
        // ============================================================

        private void AtualizarLista()
        {
            ListaCartoes.ItemsSource = null;
            ListaCartoes.ItemsSource = _cartoes;

            var temCartoes = _cartoes.Any();
            EstadoVazioPanel.Visibility = temCartoes ? Visibility.Collapsed : Visibility.Visible;
            ListaCartoes.Visibility = temCartoes ? Visibility.Visible : Visibility.Collapsed;
            SubtituloCartoes.Text = $"{_cartoes.Count} cartão(ões) cadastrado(s)";
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private string FormatarNumero(string raw)
        {
            // Mostra só os últimos 4, mascara o resto
            return $"•••• •••• •••• {raw.Substring(12, 4)}";
        }

        private string DetectarBandeira(string numero)
        {
            if (numero.StartsWith("4")) return "Visa";
            if (numero.StartsWith("5")) return "Mastercard";
            if (numero.StartsWith("34") ||
                numero.StartsWith("37")) return "Amex";
            if (numero.StartsWith("6011")) return "Discover";
            if (numero.StartsWith("636880") ||
                numero.StartsWith("606282")) return "Hipercard";
            if (numero.StartsWith("4011") ||
                numero.StartsWith("4312") ||
                numero.StartsWith("4514")) return "Elo";
            return "Cartão";
        }

        // ============================================================
        // POPUP USUÁRIO
        // ============================================================

        private void UsuarioCard_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = !_popupAberto;
            UserPopupCard.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            PopupOverlay.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            e.Handled = true;
        }

        private void FecharPopup_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = false;
            UserPopupCard.Visibility = Visibility.Collapsed;
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        private void PopupConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            _popupAberto = false;
            UserPopupCard.Visibility = Visibility.Collapsed;
            PopupOverlay.Visibility = Visibility.Collapsed;
            new ConfiguracoesWindow().Show();
            this.Close();
        }

        private void PopupLogout_Click(object sender, RoutedEventArgs e)
        {
            SessaoUsuario.Logout();
            new UserLogin().Show();
            this.Close();
        }

        // ============================================================
        // NAVEGAÇÃO
        // ============================================================

        private void FinanceButton_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }
        private void CardsButton_Click(object sender, RoutedEventArgs e) { /* já está aqui */ }
        private void ExpensesButton_Click(object sender, RoutedEventArgs e) { new ExpensesWindow().Show(); this.Close(); }
        private void CreditsButton_Click(object sender, RoutedEventArgs e) { new IncomeWindow().Show(); this.Close(); }
        private void GoalsButton_Click(object sender, RoutedEventArgs e) { new MetasWindow().Show(); this.Close(); }
    }

    // ============================================================
    // VIEW MODEL
    // ============================================================

    public class CartaoViewModel
    {
        public int Id { get; set; }
        public string Banco { get; set; }
        public string NumeroFormatado { get; set; }
        public string Bandeira { get; set; }
        public string CorInicio { get; set; }
        public string CorFim { get; set; }
    }
}