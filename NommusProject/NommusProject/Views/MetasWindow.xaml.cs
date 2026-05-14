using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using NommusProject.Data;

namespace NommusProject
{
    public partial class MetasWindow : Window
    {
        private readonly TransacaoRepository _repo = new TransacaoRepository();
        private bool _popupAberto = false;
        private int _diasFiltro = 30;
        private List<MetaViewModel> _metas = new List<MetaViewModel>();

        public MetasWindow()
        {
            InitializeComponent();
            CarregarUsuario();
            CarregarMetricas();
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
        // MÉTRICAS
        // ============================================================

        private async void CarregarMetricas()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;

            var dataInicio = DateTime.Now.AddDays(-_diasFiltro);
            var transacoes = await Task.Run(() =>
                _repo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id)
                     .Where(t => t.DataTransacao >= dataInicio)
                     .ToList());

            AtualizarCards(transacoes);
            AtualizarGraficoBarras(transacoes);
            AtualizarGraficoPizza(transacoes);
            AtualizarTopGastos(transacoes);
        }

        private void AtualizarCards(List<Transacao> transacoes)
        {
            var receitas = transacoes.Where(t => t.TipoTransacao == "Receita").ToList();
            var despesas = transacoes.Where(t => t.TipoTransacao == "Despesa").ToList();
            var totRec = receitas.Sum(t => t.ValorTransacao);
            var totDesp = despesas.Sum(t => t.ValorTransacao);
            var saldo = totRec - totDesp;
            var taxa = totRec > 0 ? (saldo / totRec) * 100 : 0;

            CardReceitaTotal.Text = totRec.ToString("C");
            CardReceitaQtd.Text = $"{receitas.Count} transações";
            CardDespesaTotal.Text = totDesp.ToString("C");
            CardDespesaQtd.Text = $"{despesas.Count} transações";
            CardSaldoPeriodo.Text = saldo.ToString("C");
            CardSaldoPeriodo.Foreground = saldo >= 0
                ? new SolidColorBrush(Color.FromRgb(59, 130, 246))
                : new SolidColorBrush(Color.FromRgb(239, 68, 68));
            CardTaxaEconomia.Text = $"{Math.Max(0, taxa):F1}%";
        }

        private void AtualizarGraficoBarras(List<Transacao> transacoes)
        {
            var porMes = transacoes
                .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month).ToList();

            var receitasBar = new ChartValues<double>();
            var despesasBar = new ChartValues<double>();
            var labels = new List<string>();

            foreach (var mes in porMes)
            {
                receitasBar.Add(mes.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao));
                despesasBar.Add(mes.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao));
                labels.Add(new DateTime(mes.Key.Year, mes.Key.Month, 1).ToString("MMM/yy"));
            }

            
        }

        private void AtualizarGraficoPizza(List<Transacao> transacoes)
        {
            var totRec = transacoes.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao);
            var totDesp = transacoes.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao);

            GraficoPizza.Series = new SeriesCollection
            {
                new PieSeries { Title = "Receitas", Values = new ChartValues<double> { totRec },  Fill = new SolidColorBrush(Color.FromRgb(16,185,129)), DataLabels = false },
                new PieSeries { Title = "Despesas", Values = new ChartValues<double> { totDesp }, Fill = new SolidColorBrush(Color.FromRgb(239,68,68)),  DataLabels = false }
            };
        }

        private void AtualizarTopGastos(List<Transacao> transacoes)
        {
            var top5 = transacoes
                .Where(t => t.TipoTransacao == "Despesa")
                .OrderByDescending(t => t.ValorTransacao)
                .Take(5)
                .Select((t, i) => new TopGastoViewModel
                {
                    Posicao = (i + 1).ToString(),
                    Descricao = t.DescricaoTransacao,
                    Data = t.DataTransacao.ToString("dd/MM/yyyy"),
                    Valor = t.ValorTransacao
                }).ToList();

            ListaTopGastos.ItemsSource = top5;
            TopGastosVazio.Visibility = top5.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        // ============================================================
        // FILTROS
        // ============================================================

        private void Filtro30_Click(object sender, RoutedEventArgs e) => AplicarFiltro(30);
        private void Filtro6M_Click(object sender, RoutedEventArgs e) => AplicarFiltro(180);
        private void Filtro1A_Click(object sender, RoutedEventArgs e) => AplicarFiltro(365);

        private void AplicarFiltro(int dias)
        {
            _diasFiltro = dias;
            BtnFiltro30.Style = dias == 30 ? (Style)FindResource("FiltroAtivoButtonStyle") : (Style)FindResource("FiltroButtonStyle");
            BtnFiltro6M.Style = dias == 180 ? (Style)FindResource("FiltroAtivoButtonStyle") : (Style)FindResource("FiltroButtonStyle");
            BtnFiltro1A.Style = dias == 365 ? (Style)FindResource("FiltroAtivoButtonStyle") : (Style)FindResource("FiltroButtonStyle");
            SubtituloText.Text = dias == 30 ? "Últimos 30 dias" : dias == 180 ? "Últimos 6 meses" : "Último ano";
            CarregarMetricas();
        }

        // ============================================================
        // METAS
        // ============================================================

        private void NovaMeta_Click(object sender, RoutedEventArgs e)
        {
            MetaNomeBox.Text = MetaObjetivoBox.Text = MetaAtualBox.Text = "";
            NovaMetaPopup.Visibility = Visibility.Visible;
            NovaMetaOverlay.Visibility = Visibility.Visible;
        }

        private void FecharNovaMeta_Click(object sender, RoutedEventArgs e)
        {
            NovaMetaPopup.Visibility = Visibility.Collapsed;
            NovaMetaOverlay.Visibility = Visibility.Collapsed;
        }

        private void CriarMeta_Click(object sender, RoutedEventArgs e)
        {
            var nome = MetaNomeBox.Text.Trim();
            if (!double.TryParse(MetaObjetivoBox.Text.Replace(",", "."), out double objetivo) || objetivo <= 0)
            { MessageBox.Show("Informe um valor objetivo válido.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            double.TryParse(MetaAtualBox.Text.Replace(",", "."), out double atual);

            _metas.Add(new MetaViewModel
            {
                Nome = string.IsNullOrEmpty(nome) ? "Meta sem nome" : nome,
                Objetivo = objetivo,
                Atual = Math.Min(atual, objetivo)
            });

            ListaMetas.ItemsSource = null;
            ListaMetas.ItemsSource = _metas;
            MetasVazioPanel.Visibility = _metas.Any() ? Visibility.Collapsed : Visibility.Visible;

            NovaMetaPopup.Visibility = Visibility.Collapsed;
            NovaMetaOverlay.Visibility = Visibility.Collapsed;
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
        private void CardsButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Cartões em breve.", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information); }
        private void ExpensesButton_Click(object sender, RoutedEventArgs e) { new ExpensesWindow().Show(); this.Close(); }
        private void CreditsButton_Click(object sender, RoutedEventArgs e) { new IncomeWindow().Show(); this.Close(); }
        private void GoalsButton_Click(object sender, RoutedEventArgs e) { /* já está aqui */ }

        internal class Show
        {
            public Show()
            {
            }
        }
    }

    // ============================================================
    // VIEW MODELS
    // ============================================================

    public class MetaViewModel
    {
        public string Nome { get; set; }
        public double Objetivo { get; set; }
        public double Atual { get; set; }
        public double Percentual => Objetivo > 0 ? (Atual / Objetivo) * 100 : 0;
        public string PercentualTexto => $"{Percentual:F1}% concluído";
        public double LarguraProgresso => Math.Min(Atual / (Objetivo > 0 ? Objetivo : 1), 1.0) * 600;
    }

    public class TopGastoViewModel
    {
        public string Posicao { get; set; }
        public string Descricao { get; set; }
        public string Data { get; set; }
        public double Valor { get; set; }
    }
}