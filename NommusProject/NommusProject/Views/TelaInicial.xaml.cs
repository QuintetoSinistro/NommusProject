using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NommusProject.Data;
using Nommus;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace NommusProject
{
    public partial class MainWindow : Window
    {
        private readonly TransacaoRepository _transacaoRepo = new TransacaoRepository();
        private bool _popupAberto = false;

        public object UsuarioTipoText { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            if (SessaoUsuario.UsuarioLogado == null)
            {
                MessageBox.Show("Usuário não está logado!", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                new UserLogin().Show();
                this.Close();
                return;
            }

            CarregarDadosUsuario();
            CarregarEstatisticas();
            CarregarDadosGrafico();
        }

        // ============================================================
        // DADOS DO USUÁRIO
        // ============================================================

        private void CarregarDadosUsuario()
        {
            var usuario = SessaoUsuario.UsuarioLogado;
            if (usuario == null) return;
            ConfigurarPerfilUsuario(usuario);
            ConfigurarSaldoUsuario(usuario);
        }

        private void ConfigurarPerfilUsuario(Usuarios usuario)
        {
            if (UsuarioNomeText != null) UsuarioNomeText.Text = usuario.Nome;
            if (UsuarioTipoText is TextBlock tipoTb)
            {
                tipoTb.Text = usuario.Tipo.ToString();
                ConfigurarCorTipoUsuario(tipoTb, usuario);
            }
            // Preenche o popup dinamicamente
            if (PopupNomeText != null) PopupNomeText.Text = usuario.Nome;
            if (PopupEmailText != null) PopupEmailText.Text = usuario.Email;
        }

        private void ConfigurarCorTipoUsuario(object usuarioTipoText, Usuarios usuario)
        {
            throw new NotImplementedException();
        }

        private void ConfigurarCorTipoUsuario(TextBlock tipoTextBlock, Usuarios usuario)
        {
            if (tipoTextBlock == null) return;

            switch (usuario.Tipo)
            {
                case TipoUsuario.Basic:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246)); break;
                case TipoUsuario.Premium:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11)); break;
                case TipoUsuario.Adm:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); break;
            }
        }

        private void ConfigurarSaldoUsuario(Usuarios usuario)
        {
            if (BalanceText == null) return;
            BalanceText.Text = $"R$ {usuario.saldoDisponivel:F2}";
            BalanceText.Foreground = usuario.saldoDisponivel >= 0
                ? new SolidColorBrush(Color.FromRgb(34, 197, 94))
                : new SolidColorBrush(Color.FromRgb(239, 68, 68));
        }

        // ============================================================
        // ESTATÍSTICAS
        // ============================================================

        private async void CarregarEstatisticas()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;
            try
            {
                var transacoes = await Task.Run(() =>
                    _transacaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id));
                if (transacoes.Any()) CalcularTotaisTransacoes(transacoes);
                else ExibirTotaisZerados();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estatísticas: {ex.Message}");
                ExibirTotaisZerados();
            }
        }

        private void CalcularTotaisTransacoes(List<Transacao> transacoes)
        {
            var totalReceitas = transacoes.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao);
            var totalDespesas = transacoes.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao);
            MaxReceitaText.Text = $"R$ {totalReceitas:F2}";
            MaxDespesaText.Text = $"R$ {totalDespesas:F2}";
        }

        private void ExibirTotaisZerados()
        {
            MaxReceitaText.Text = "R$ 0,00";
            MaxDespesaText.Text = "R$ 0,00";
        }

        // ============================================================
        // GRÁFICO
        // ============================================================

        private async void CarregarDadosGrafico()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;
            try
            {
                var transacoes = await Task.Run(() =>
                    _transacaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id));
                var dadosPorMes = AgruparTransacoesPorMes(transacoes);
                if (!dadosPorMes.Any()) { FinanceChart.Series = new SeriesCollection(); return; }
                AtualizarGrafico(dadosPorMes);
            }
            catch (Exception ex) { Console.WriteLine($"Erro ao carregar gráfico: {ex.Message}"); }
        }

        private List<DadosMes> AgruparTransacoesPorMes(List<Transacao> transacoes)
        {
            return transacoes
                .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                .Select(g => new DadosMes
                {
                    Mes = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Receitas = g.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao),
                    Despesas = g.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao)
                })
                .OrderBy(d => d.Mes).ToList();
        }

        private void AtualizarGrafico(List<DadosMes> dadosPorMes)
        {
            var (saldoValues, receitaValues, despesaValues, labels) = ProcessarDadosGrafico(dadosPorMes);
            FinanceChart.Series = new SeriesCollection
            {
                new LineSeries { Title = "Saldo",    Values = saldoValues,   Stroke = new SolidColorBrush(Color.FromRgb(59, 130, 246)), StrokeThickness = 3, PointGeometry = null },
                new LineSeries { Title = "Receitas", Values = receitaValues, Stroke = new SolidColorBrush(Color.FromRgb(16, 185, 129)), StrokeThickness = 2, PointGeometry = null },
                new LineSeries { Title = "Despesas", Values = despesaValues, Stroke = new SolidColorBrush(Color.FromRgb(239, 68, 68)),  StrokeThickness = 2, PointGeometry = null }
            };
            ConfigurarEixosGrafico(labels);
        }

        private (ChartValues<double>, ChartValues<double>, ChartValues<double>, List<string>)
            ProcessarDadosGrafico(List<DadosMes> dadosPorMes)
        {
            var saldoValues = new ChartValues<double>();
            var receitaValues = new ChartValues<double>();
            var despesaValues = new ChartValues<double>();
            var labels = new List<string>();
            double saldoAcum = 0;
            foreach (var mes in dadosPorMes)
            {
                saldoAcum += (mes.Receitas - mes.Despesas);
                saldoValues.Add(saldoAcum);
                receitaValues.Add(mes.Receitas);
                despesaValues.Add(mes.Despesas);
                labels.Add(mes.Mes.ToString("MMM/yy"));
            }
            return (saldoValues, receitaValues, despesaValues, labels);
        }

        private void ConfigurarEixosGrafico(List<string> labels)
        {
            FinanceChart.AxisX.Clear();
            FinanceChart.AxisX.Add(new Axis { Title = "Mês", Labels = labels.ToArray(), Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)) });
            FinanceChart.AxisY.Clear();
            FinanceChart.AxisY.Add(new Axis { Title = "R$", LabelFormatter = value => value.ToString("C"), Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)) });
        }

        // ============================================================
        // POPUP DO USUÁRIO
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
            var telaConfiguracoes = new ConfiguracoesWindow();
            telaConfiguracoes.Show();
            this.Close();
        }

        private void PopupLogout_Click(object sender, RoutedEventArgs e)
        {
            SessaoUsuario.Logout();
            new UserLogin().Show();
            this.Close();
        }

        // ============================================================
        // NAVEGAÇÃO - SIDEBAR
        // ============================================================

        private void FinanceButton_Click(object sender, RoutedEventArgs e) { }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navegar para Cartões", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            try { new ExpensesWindow().Show(); this.Close(); }
            catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            new IncomeWindow().Show();
            this.Close();
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navegar para Metas", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e) { }
    }

    internal class DadosMes
    {
        public DateTime Mes { get; set; }
        public double Receitas { get; set; }
        public double Despesas { get; set; }
    }
}