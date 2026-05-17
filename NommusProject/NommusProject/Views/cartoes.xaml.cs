using NommusProject.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NommusProject.Views
{
    public partial class cartoes : Window
    {
        private bool _popupAberto = false;
        private List<CartaoViewModel> _cartoes = new List<CartaoViewModel>();
        private CartaoRepository _cartaoRepo = new CartaoRepository();

        private readonly (string Inicio, string Fim)[] _paletas =
        {
            ("#1E3A8A", "#3B82F6"),
            ("#064E3B", "#10B981"),
            ("#4C1D95", "#8B5CF6"),
            ("#7F1D1D", "#EF4444"),
            ("#78350F", "#F59E0B"),
            ("#0C4A6E", "#0EA5E9"),
            ("#1E1B4B", "#6366F1"),
        };

        public cartoes()
        {
            InitializeComponent();
            CarregarUsuario();
            CarregarCartoesDoBanco();
        }

        private void CarregarUsuario()
        {
            var u = SessaoUsuario.UsuarioLogado;
            if (u == null) return;
            UsuarioNomeText.Text = u.Nome;
            PopupNomeText.Text = u.Nome;
            PopupEmailText.Text = u.Email;
            CarregarFotoSidebar(u.FotoPerfil);
        }

        private void CarregarFotoSidebar(string caminhoFoto)
        {
            try
            {
                if (!string.IsNullOrEmpty(caminhoFoto) && System.IO.File.Exists(caminhoFoto))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new System.IO.FileStream(caminhoFoto, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.StreamSource.Dispose();
                    SidebarFotoBrush.ImageSource = bitmap;
                }
                else
                {
                    SidebarFotoBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Views/Images/user.png"));
                }
            }
            catch { SidebarFotoBrush.ImageSource = null; }
        }

        private void CarregarCartoesDoBanco()
        {
            var cartoesDB = _cartaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id);
            _cartoes = cartoesDB.Select(c => new CartaoViewModel
            {
                Id = c.IdCartao,
                Banco = c.NomeCartao,
                NumeroFormatado = $"•••• •••• •••• {(c.NumeroCartao?.Length >= 4 ? c.NumeroCartao.Substring(c.NumeroCartao.Length - 4) : "0000")}",
                Bandeira = c.BandeiraCartao,
                CorInicio = _paletas[c.IdCartao % _paletas.Length].Inicio,
                CorFim = _paletas[c.IdCartao % _paletas.Length].Fim
            }).ToList();

            AtualizarLista();
        }

        private void AtualizarLista()
        {
            ListaCartoes.ItemsSource = null;
            ListaCartoes.ItemsSource = _cartoes;
            bool temCartoes = _cartoes.Any();
            EstadoVazioPanel.Visibility = temCartoes ? Visibility.Collapsed : Visibility.Visible;
            ListaCartoes.Visibility = temCartoes ? Visibility.Visible : Visibility.Collapsed;
            SubtituloCartoes.Text = $"{_cartoes.Count} cartão(ões) cadastrado(s)";
        }

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

        private void NumeroCartao_TextChanged(object sender, TextChangedEventArgs e)
        {
            var raw = NumeroCartaoBox.Text.Replace(" ", "").Replace("-", "");
            var formatado = "";
            for (int i = 0; i < raw.Length && i < 16; i++)
            {
                if (i > 0 && i % 4 == 0) formatado += " ";
                formatado += raw[i];
            }

            if (NumeroCartaoBox.Text != formatado)
            {
                NumeroCartaoBox.TextChanged -= NumeroCartao_TextChanged;
                NumeroCartaoBox.Text = formatado;
                NumeroCartaoBox.CaretIndex = formatado.Length;
                NumeroCartaoBox.TextChanged += NumeroCartao_TextChanged;
            }

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
                BancoCustomBox.Visibility = banco == "Outro" ? Visibility.Visible : Visibility.Collapsed;
                if (banco != "Outro") BancoCustomBox.Text = "";
            }
        }

        private void BancoCustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(BancoCustomBox.Text))
                PreviewBanco.Text = BancoCustomBox.Text;
        }

        private void SalvarCartao_Click(object sender, RoutedEventArgs e)
        {
            ErroText.Visibility = Visibility.Collapsed;
            var numeroRaw = NumeroCartaoBox.Text.Replace(" ", "");
            if (numeroRaw.Length != 16)
            {
                ErroText.Text = "Digite os 16 dígitos do cartão.";
                ErroText.Visibility = Visibility.Visible;
                return;
            }
            if (!numeroRaw.All(char.IsDigit))
            {
                ErroText.Text = "Número deve conter apenas dígitos.";
                ErroText.Visibility = Visibility.Visible;
                return;
            }

            string banco;
            if (BancoComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                banco = selectedItem.Content.ToString() == "Outro" ? BancoCustomBox.Text.Trim() : selectedItem.Content.ToString();
            }
            else
            {
                ErroText.Text = "Selecione o banco.";
                ErroText.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(banco))
            {
                ErroText.Text = "Informe o banco.";
                ErroText.Visibility = Visibility.Visible;
                return;
            }

            var novoCartao = new Cartao
            {
                NomeCartao = banco,
                BandeiraCartao = DetectarBandeira(numeroRaw),
                LimiteCartao = 0,
                DataVencimento = DateTime.Now.AddYears(3),
                IdUsuario = SessaoUsuario.UsuarioLogado.Id,
                NumeroCartao = numeroRaw
            };

            _cartaoRepo.Add(novoCartao);
            CarregarCartoesDoBanco();
            FecharNovoCartao_Click(sender, e);
        }

        private async void ExcluirCartao_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (MessageBox.Show("Remover este cartão?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    await _cartaoRepo.Delete(id);
                    CarregarCartoesDoBanco();
                }
            }
        }

        private string DetectarBandeira(string numero)
        {
            if (numero.StartsWith("4")) return "Visa";
            if (numero.StartsWith("5")) return "Mastercard";
            if (numero.StartsWith("34") || numero.StartsWith("37")) return "Amex";
            if (numero.StartsWith("6011")) return "Discover";
            if (numero.StartsWith("636880") || numero.StartsWith("606282")) return "Hipercard";
            if (numero.StartsWith("4011") || numero.StartsWith("4312") || numero.StartsWith("4514")) return "Elo";
            return "Cartão";
        }

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

        private void PopupConfiguracoes_Click(object sender, RoutedEventArgs e) { new ConfiguracoesWindow().Show(); Close(); }
        private void PopupLogout_Click(object sender, RoutedEventArgs e) { SessaoUsuario.Logout(); new UserLogin().Show(); Close(); }

        private void FinanceButton_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); Close(); }
        private void CardsButton_Click(object sender, RoutedEventArgs e) { }
        private void ExpensesButton_Click(object sender, RoutedEventArgs e) { new ExpensesWindow().Show(); Close(); }
        private void CreditsButton_Click(object sender, RoutedEventArgs e) { new IncomeWindow().Show(); Close(); }
        private void GoalsButton_Click(object sender, RoutedEventArgs e) { new MetasWindow().Show(); Close(); }
    }

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