using NommusProject;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Linq;

namespace Nommus
{
    public partial class MainWindow : Window
    {
        private bool _isMouseOverPopup = false;
        private Button _lastClickedButton;
        private Usuario _usuarioLogado;

        public MainWindow()
        {
            InitializeComponent();
            _usuarioLogado = SessaoUsuario.UsuarioLogado;

            // DEBUG: Verificar se está carregando saldo correto
            if (_usuarioLogado != null)
            {
                Console.WriteLine($"MainWindow - Saldo carregado: R$ {_usuarioLogado.saldoDisponivel:F2}");
            }

            CarregarDadosUsuario();
            CarregarEstatisticas();
            CarregarDadosGrafico();
        }

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
        private void CarregarDadosUsuario()
        {
            if (_usuarioLogado != null)
            {
                // Personalizar perfil do usuário
                var nomeTextBlock = FindName("UsuarioNomeText") as TextBlock;
                var tipoTextBlock = FindName("UsuarioTipoText") as TextBlock;

                if (nomeTextBlock != null)
                    nomeTextBlock.Text = _usuarioLogado.Nome;

                if (tipoTextBlock != null)
                {
                    tipoTextBlock.Text = _usuarioLogado.Tipo.ToString();
                    // Personalizar cor baseada no tipo
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

                // ✅✅✅ GARANTIR que o saldo está sendo mostrado corretamente
                if (BalanceText != null)
                {
                    BalanceText.Text = $"R$ {_usuarioLogado.saldoDisponivel:F2}";

                    // Personalizar cor do saldo
                    if (_usuarioLogado.saldoDisponivel >= 0)
                    {
                        BalanceText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Verde
                    }
                    else
                    {
                        BalanceText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Vermelho
                    }
                }
                else
                {
                    // DEBUG: Se BalanceText for null
                    MessageBox.Show("BalanceText não encontrado!", "Erro",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // DEBUG: Se usuário for null
                MessageBox.Show("Usuário não encontrado na sessão!", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CarregarEstatisticas()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;

            try
            {
                var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(SessaoUsuario.UsuarioLogado.Id);

                if (transacoes.Any())
                {
                    // TOTAL de Receitas
                    var totalReceitas = transacoes
                        .Where(t => t.TipoTransacao == "Receita")
                        .Sum(t => t.ValorTransacao);

                    MaxReceitaText.Text = $"R$ {totalReceitas:F2}";

                    // TOTAL de Despesas
                    var totalDespesas = transacoes
                        .Where(t => t.TipoTransacao == "Despesa")
                        .Sum(t => t.ValorTransacao);

                    MaxDespesaText.Text = $"R$ {totalDespesas:F2}";
                }
                else
                {
                    MaxReceitaText.Text = "R$ 0,00";
                    MaxDespesaText.Text = "R$ 0,00";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estatísticas: {ex.Message}");
                MaxReceitaText.Text = "R$ 0,00";
                MaxDespesaText.Text = "R$ 0,00";
            }
        }
        private async void CarregarDadosGrafico()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;

            try
            {
                var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(SessaoUsuario.UsuarioLogado.Id);

                // Agrupar transações por mês
                var dadosPorMes = transacoes
                    .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                    .Select(g => new
                    {
                        Mes = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Receitas = g.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao),
                        Despesas = g.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao)
                    })
                    .OrderBy(d => d.Mes)
                    .ToList();

                if (!dadosPorMes.Any())
                {
                    // Se não há dados, mostrar gráfico vazio
                    FinanceChart.Series = new SeriesCollection();
                    return;
                }

                // Preparar dados para o gráfico
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

                // Atualizar gráfico
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
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar gráfico: {ex.Message}");
            }
        }
        // Navigation button click handlers
        // Navigation methods
        // Navigation button click handlers
        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            // Já está na tela principal
            MessageBox.Show("Você já está na tela principal", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Cartões
            MessageBox.Show("Navegar para Cartões", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
                MessageBox.Show($"Erro ao navegar para Gastos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            IncomeWindow incomeWindow = new IncomeWindow();
            incomeWindow.Show();
            this.Close();
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Metas
            MessageBox.Show("Navegar para Metas", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // Quando o canvas é carregado, redesenhar o gráfico com as dimensões corretas
    }
}