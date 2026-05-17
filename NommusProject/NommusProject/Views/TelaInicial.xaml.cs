using NommusProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace Nommus
{
    public partial class MainWindow : Window
    {
        private Usuario _usuarioLogado;

        public MainWindow()
        {
            InitializeComponent();

            // Obtém usuário da sessão e carrega dados
            _usuarioLogado = SessaoUsuario.UsuarioLogado;
            CarregarDadosUsuario();
            CarregarEstatisticas();
            CarregarDadosGrafico();
        }

        // Classe para gerenciar dados do gráfico
        public class ChartViewModel
        {
            public SeriesCollection SeriesCollection { get; set; }
            public string[] Labels { get; set; }
            public Func<double, string> Formatter { get; set; }

            public ChartViewModel()
            {
                Formatter = value => value.ToString("C");
            }
        }

        // Carrega e exibe os dados do usuário logado
        private void CarregarDadosUsuario()
        {
            if (_usuarioLogado != null)
            {
                ConfigurarPerfilUsuario();
                ConfigurarSaldoUsuario();
            }
            else
            {
                MessageBox.Show("Usuário não encontrado na sessão!", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Configura o perfil do usuário na sidebar
        private void ConfigurarPerfilUsuario()
        {
            var nomeTextBlock = FindName("UsuarioNomeText") as TextBlock;
            var tipoTextBlock = FindName("UsuarioTipoText") as TextBlock;

            if (nomeTextBlock != null)
                nomeTextBlock.Text = _usuarioLogado.Nome;

            if (tipoTextBlock != null)
            {
                tipoTextBlock.Text = _usuarioLogado.Tipo.ToString();
                ConfigurarCorTipoUsuario(tipoTextBlock);
            }
        }

        // Define a cor do tipo de usuário
        private void ConfigurarCorTipoUsuario(TextBlock tipoTextBlock)
        {
            switch (_usuarioLogado.Tipo)
            {
                case TipoUsuario.Basic:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246)); // Azul
                    break;
                case TipoUsuario.Premium:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Dourado
                    break;
                case TipoUsuario.Adm:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Vermelho
                    break;
            }
        }

        // Configura a exibição do saldo do usuário
        private void ConfigurarSaldoUsuario()
        {
            if (BalanceText != null)
            {
                BalanceText.Text = $"R$ {_usuarioLogado.saldoDisponivel:F2}";
                ConfigurarCorSaldo();
            }
            else
            {
                MessageBox.Show("BalanceText não encontrado!", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Define a cor do saldo (verde para positivo, vermelho para negativo)
        private void ConfigurarCorSaldo()
        {
            if (_usuarioLogado.saldoDisponivel >= 0)
            {
                BalanceText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Verde
            }
            else
            {
                BalanceText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Vermelho
            }
        }

        // Carrega estatísticas de receitas e despesas
        private async void CarregarEstatisticas()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;

            try
            {
                var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(SessaoUsuario.UsuarioLogado.Id);

                if (transacoes.Any())
                {
                    CalcularTotaisTransacoes(transacoes);
                }
                else
                {
                    ExibirTotaisZerados();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estatísticas: {ex.Message}");
                ExibirTotaisZerados();
            }
        }

        // Calcula totais de receitas e despesas
        private void CalcularTotaisTransacoes(List<Transacao> transacoes)
        {
            var totalReceitas = transacoes
                .Where(t => t.TipoTransacao == "Receita")
                .Sum(t => t.ValorTransacao);

            var totalDespesas = transacoes
                .Where(t => t.TipoTransacao == "Despesa")
                .Sum(t => t.ValorTransacao);

            MaxReceitaText.Text = $"R$ {totalReceitas:F2}";
            MaxDespesaText.Text = $"R$ {totalDespesas:F2}";
        }

        // Exibe valores zerados quando não há transações
        private void ExibirTotaisZerados()
        {
            MaxReceitaText.Text = "R$ 0,00";
            MaxDespesaText.Text = "R$ 0,00";
        }

        // Carrega dados para o gráfico de evolução financeira
        private async void CarregarDadosGrafico()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;

            try
            {
                var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(SessaoUsuario.UsuarioLogado.Id);
                var dadosPorMes = AgruparTransacoesPorMes(transacoes);

                if (!dadosPorMes.Any())
                {
                    FinanceChart.Series = new SeriesCollection();
                    return;
                }

                AtualizarGrafico(dadosPorMes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar gráfico: {ex.Message}");
            }
        }

        // Agrupa transações por mês para o gráfico
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
                .OrderBy(d => d.Mes)
                .ToList();
        }

        // Atualiza o gráfico com os dados processados
        private void AtualizarGrafico(List<DadosMes> dadosPorMes)
        {
            var (saldoValues, receitaValues, despesaValues, labels) = ProcessarDadosGrafico(dadosPorMes);

            FinanceChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Saldo",
                    Values = saldoValues,
                    Stroke = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                    StrokeThickness = 3,
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Receitas",
                    Values = receitaValues,
                    Stroke = new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                    StrokeThickness = 2,
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Despesas",
                    Values = despesaValues,
                    Stroke = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                    StrokeThickness = 2,
                    PointGeometry = null
                }
            };

            ConfigurarEixosGrafico(labels);
        }

        // Processa dados para o gráfico calculando saldo acumulado
        private (ChartValues<double>, ChartValues<double>, ChartValues<double>, List<string>)
            ProcessarDadosGrafico(List<DadosMes> dadosPorMes)
        {
            var saldoValues = new ChartValues<double>();
            var receitaValues = new ChartValues<double>();
            var despesaValues = new ChartValues<double>();
            var labels = new List<string>();

            double saldoAcumulado = 0;

            foreach (var mes in dadosPorMes)
            {
                saldoAcumulado += (mes.Receitas - mes.Despesas);

                saldoValues.Add(saldoAcumulado);
                receitaValues.Add(mes.Receitas);
                despesaValues.Add(mes.Despesas);
                labels.Add(mes.Mes.ToString("MMM/yy"));
            }

            return (saldoValues, receitaValues, despesaValues, labels);
        }

        // Configura os eixos X e Y do gráfico
        private void ConfigurarEixosGrafico(List<string> labels)
        {
            FinanceChart.AxisX.Clear();
            FinanceChart.AxisX.Add(new Axis
            {
                Title = "Mês",
                Labels = labels.ToArray(),
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184))
            });

            FinanceChart.AxisY.Clear();
            FinanceChart.AxisY.Add(new Axis
            {
                Title = "R$",
                LabelFormatter = value => value.ToString("C"),
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184))
            });
        }

        // Métodos de navegação entre telas

        // Navegação para Finanças (já está na tela principal)
        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Você já está na tela principal", "Informação",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Navegação para Cartões (a implementar)
        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navegar para Cartões", "Navegação",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Navegação para Despesas
        private void ExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExpensesWindow expensesWindow = new ExpensesWindow();
                expensesWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Gastos: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Navegação para Receitas
        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            IncomeWindow incomeWindow = new IncomeWindow();
            incomeWindow.Show();
            this.Close();
        }

        // Navegação para Metas (a implementar)
        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navegar para Metas", "Navegação",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Navegação para Relatórios (a implementar)
        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Relatórios
        }
    }

    // Classe auxiliar para agrupar dados mensais
    internal class DadosMes
    {
        public DateTime Mes { get; set; }
        public double Receitas { get; set; }
        public double Despesas { get; set; }
    }
}