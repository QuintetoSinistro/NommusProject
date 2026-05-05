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
using System.Windows.Shapes;

namespace NommusProject.Views
{
    /// <summary>
    /// Lógica interna para cartoes.xaml
    /// </summary>
    public partial class cartoes : Window
    {
        public cartoes()
        {
            InitializeComponent();
        }
<<<<<<< Updated upstream
=======

        // ============================================================
        // DADOS DO USUÁRIO (sidebar e popup)
        // ============================================================

        // Carrega nome e email do usuário logado nos controles da tela
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
                    var defaultUri = new Uri("pack://application:,,,/Views/Images/user.png", UriKind.Absolute);
                    SidebarFotoBrush.ImageSource = new BitmapImage(defaultUri);
                }
            }
            catch { SidebarFotoBrush.ImageSource = null; }
        }

        // ============================================================
        // CARREGAMENTO DOS CARTÕES DO BANCO DE DADOS
        // ============================================================

        // Busca os cartões do usuário no banco, converte para ViewModel e exibe na lista
        private void CarregarCartoesDoBanco()
        {
            // Obtém a lista de cartões associados ao usuário logado
            var cartoesDB = _cartaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id);

            // Converte cada Cartao (modelo de dados) para CartaoViewModel (modelo de exibição)
            _cartoes = cartoesDB.Select(c => new CartaoViewModel
            {
                Id = c.IdCartao,
                Banco = c.NomeCartao,

                // Exibe o número do cartão mascarado: mostra apenas os 4 últimos dígitos da bandeira
                NumeroFormatado = $"•••• •••• •••• {(c.NumeroCartao?.Length >= 4 ?
                c.NumeroCartao.Substring(c.NumeroCartao.Length - 4) : "0000")}",
                Bandeira = c.BandeiraCartao,

                // Define as cores do gradiente com base no ID do cartão (rotaciona a paleta)
                CorInicio = _paletas[c.IdCartao % _paletas.Length].Inicio,
                CorFim = _paletas[c.IdCartao % _paletas.Length].Fim
            }).ToList();

            AtualizarLista();
        }

        // ============================================================
        // EXIBIÇÃO DA LISTA E ESTADO VAZIO
        // ============================================================

        // Atualiza o ItemsControl com a lista de cartões e controla a visibilidade do estado vazio
        private void AtualizarLista()
        {
            ListaCartoes.ItemsSource = null;
            ListaCartoes.ItemsSource = _cartoes;
            bool temCartoes = _cartoes.Any();
            EstadoVazioPanel.Visibility = temCartoes ? Visibility.Collapsed : Visibility.Visible;
            ListaCartoes.Visibility = temCartoes ? Visibility.Visible : Visibility.Collapsed;
            SubtituloCartoes.Text = $"{_cartoes.Count} cartão(ões) cadastrado(s)";
        }

        // ============================================================
        // POPUP PARA ADICIONAR NOVO CARTÃO (ABRIR/FECHAR)
        // ============================================================

        // Botão "+ Adicionar Cartão": limpa os campos e exibe o popup
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

        // Fecha o popup (cancelar ou após salvar)
        private void FecharNovoCartao_Click(object sender, RoutedEventArgs e)
        {
            NovoCartaoPopup.Visibility = Visibility.Collapsed;
            NovoCartaoOverlay.Visibility = Visibility.Collapsed;
        }

        // ============================================================
        // FORMATAÇÃO DO NÚMERO DO CARTÃO (AO DIGITAR)
        // ============================================================

        // Evento disparado a cada alteração no campo de número do cartão.
        // Formata automaticamente em grupos de 4 dígitos e atualiza o preview.
        private void NumeroCartao_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Remove espaços e hífens para obter apenas os dígitos brutos
            var raw = NumeroCartaoBox.Text.Replace(" ", "").Replace("-", "");

            // Formata em grupos de 4 (ex: "1234 5678 9012 3456")
            var formatado = "";
            for (int i = 0; i < raw.Length && i < 16; i++)
            {
                if (i > 0 && i % 4 == 0) formatado += " ";
                formatado += raw[i];
            }

            // Evita loop infinito (atualiza o TextBox sem disparar o evento novamente)
            if (NumeroCartaoBox.Text != formatado)
            {
                NumeroCartaoBox.TextChanged -= NumeroCartao_TextChanged;
                NumeroCartaoBox.Text = formatado;
                NumeroCartaoBox.CaretIndex = formatado.Length;
                NumeroCartaoBox.TextChanged += NumeroCartao_TextChanged;
            }

            // Cria o preview do cartão: mostra os dígitos digitados e preenche o restante com "•"
            var preview = "";
            for (int i = 0; i < 16; i++)
            {
                if (i > 0 && i % 4 == 0) preview += " ";
                preview += i < raw.Length ? raw[i] : '•';
            }
            PreviewNumero.Text = preview;
        }

        // ============================================================
        // SELEÇÃO DO BANCO (COM OPÇÃO "OUTRO")
        // ============================================================

        // Quando o usuário seleciona um banco no ComboBox, atualiza o preview e
        // mostra/esconde o campo de texto personalizado para "Outro".
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

        // Atualiza o preview quando o usuário digita o nome do banco personalizado
        private void BancoCustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(BancoCustomBox.Text))
                PreviewBanco.Text = BancoCustomBox.Text;
        }

        // ============================================================
        // SALVAR CARTÃO (VALIDAÇÃO E PERSISTÊNCIA)
        // ============================================================

        // Botão "Salvar Cartão": valida os dados e insere no banco de dados
        private void SalvarCartao_Click(object sender, RoutedEventArgs e)
        {
            ErroText.Visibility = Visibility.Collapsed;

            // Valida número do cartão (16 dígitos numéricos)
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

            // Obtém o nome do banco (selecionado ou personalizado)
            string banco;
            if (BancoComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                banco = selectedItem.Content.ToString() == "Outro"
                    ? BancoCustomBox.Text.Trim()
                    : selectedItem.Content.ToString();
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

            // Cria o objeto Cartao (modelo) com dados limitados (limite e vencimento fictícios)
            var novoCartao = new Cartao
            {
                NomeCartao = banco,
                BandeiraCartao = DetectarBandeira(numeroRaw),
                LimiteCartao = 0,                     // pode ser editado depois
                DataVencimento = DateTime.Now.AddYears(3), // valor fictício
                IdUsuario = SessaoUsuario.UsuarioLogado.Id,
                NumeroCartao = numeroRaw
            };

            // Salva no banco via repositório
            _cartaoRepo.Add(novoCartao);

            // Recarrega a lista de cartões para exibir o novo
            CarregarCartoesDoBanco();

            // Fecha o popup
            FecharNovoCartao_Click(sender, e);
        }

        // ============================================================
        // EXCLUSÃO DE CARTÃO
        // ============================================================

        // Evento disparado pelo botão "✕" no card do cartão. O Tag contém o IdCartao
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

        // ============================================================
        // DETECÇÃO DE BANDEIRA (BASEADA NOS PRIMEIROS DÍGITOS)
        // ============================================================

        // Retorna o nome da bandeira com base no número do cartão (regras comuns)
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

        // ============================================================
        // POPUP DO USUÁRIO (MENU DE PERFIL)
        // ============================================================

        // Alterna a visibilidade do popup ao clicar no card do usuário na sidebar
        private void UsuarioCard_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = !_popupAberto;
            UserPopupCard.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            PopupOverlay.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            e.Handled = true;
        }

        // Fecha o popup quando o overlay (fundo escuro) é clicado
        private void FecharPopup_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = false;
            UserPopupCard.Visibility = Visibility.Collapsed;
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        // Abre a tela de configurações e fecha a atual
        private void PopupConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            new ConfiguracoesWindow().Show();
            Close();
        }

        // Faz logout, limpa a sessão e volta para a tela de login
        private void PopupLogout_Click(object sender, RoutedEventArgs e)
        {
            SessaoUsuario.Logout();
            new UserLogin().Show();
            Close();
        }

        // ============================================================
        // NAVEGAÇÃO PELA SIDEBAR
        // ============================================================

        private void FinanceButton_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); Close(); }
        private void CardsButton_Click(object sender, RoutedEventArgs e) { /* já está na tela de cartões */ }
        private void ExpensesButton_Click(object sender, RoutedEventArgs e) { new ExpensesWindow().Show(); Close(); }
        private void CreditsButton_Click(object sender, RoutedEventArgs e) { new IncomeWindow().Show(); Close(); }
        private void GoalsButton_Click(object sender, RoutedEventArgs e) { new MetasWindow().Show(); Close(); }
>>>>>>> Stashed changes
    }
}
