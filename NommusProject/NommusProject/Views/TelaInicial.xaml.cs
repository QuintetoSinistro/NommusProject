using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Data.Sqlite;
using Nommus;
using NommusProject.Data;
using NommusProject.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NommusProject
{
    // Tela principal (Dashboard) do sistema, exibida após o login
    public partial class MainWindow : Window
    {
        // Repositório para acessar as transações (receitas/despesas) no banco de dados
        private readonly TransacaoRepository _transacaoRepo = new TransacaoRepository();

        // Controla se o popup do usuário está visível ou não
        private bool _popupAberto = false;

        // Propriedade pública que armazena o tipo do usuário (Basic, Premium, Adm) – não utilizada diretamente no XAML atual
        public object UsuarioTipoText { get; private set; }

        // Construtor: inicializa os componentes XAML, verifica se há usuário logado,
        // carrega dados do usuário, estatísticas e o gráfico.
        public MainWindow()
        {
            InitializeComponent();

            // Segurança: se ninguém estiver logado, redireciona para a tela de login
            if (SessaoUsuario.UsuarioLogado == null)
            {
                MessageBox.Show("Usuário não está logado!", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                new UserLogin().Show();
                this.Close();
                return;
            }

            // Carrega as informações do usuário logado na sidebar e no popup
            CarregarDadosUsuario();

            // Carrega os totais de receitas e despesas
            CarregarEstatisticas();

            // Carrega os dados para o gráfico de evolução financeira
            CarregarDadosGrafico();

            this.Closed += (s, e) => DatabaseBackup.CriarBackup();
        }

        // ============================================================
        // DADOS DO USUÁRIO
        // ============================================================

        // Ponto de entrada para carregar nome, tipo e saldo do usuário
        private void CarregarDadosUsuario()
        {
            var usuario = SessaoUsuario.UsuarioLogado;
            if (usuario == null) return;
            ConfigurarPerfilUsuario(usuario);
            ConfigurarSaldoUsuario(usuario);

            // Carrega a foto na sidebar
            CarregarFotoSidebar(usuario.FotoPerfil);
        }

        private void CarregarFotoSidebar(string caminhoFoto)
        {
            try
            {
                if (!string.IsNullOrEmpty(caminhoFoto) && System.IO.File.Exists(caminhoFoto))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(caminhoFoto, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    SidebarFotoBrush.ImageSource = bitmap;
                }
                else
                {
                    // Tenta imagem padrão do assembly
                    var defaultUri = new Uri("pack://application:,,,/Views/Images/user.png", UriKind.Absolute);
                    var defaultBitmap = new BitmapImage(defaultUri);
                    SidebarFotoBrush.ImageSource = defaultBitmap;
                }
            }
            catch
            {
                SidebarFotoBrush.ImageSource = null; // fallback para cor sólida
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            // Recarrega os dados do usuário do banco
            var usuario = Usuarios.BuscarPorId(SessaoUsuario.UsuarioLogado.Id);
            if (usuario != null)
            {
                SessaoUsuario.UsuarioLogado = usuario;
                ConfigurarSaldoUsuario(usuario);
            }
            // Recarrega estatísticas e gráfico (opcional)
            CarregarEstatisticas();
            CarregarDadosGrafico();
        }

        // Exibe o nome e o tipo do usuário na sidebar e no popup.
        // Aplica cor diferente conforme o tipo (Basic, Premium, Adm)
        private void ConfigurarPerfilUsuario(Usuarios usuario)
        {
            // Nome na sidebar
            if (UsuarioNomeText != null) UsuarioNomeText.Text = usuario.Nome;

            // Se existir um TextBlock para o tipo (não está no XAML atual, mas foi planejado)
            if (UsuarioTipoText is TextBlock tipoTb)
            {
                tipoTb.Text = usuario.Tipo.ToString();
                ConfigurarCorTipoUsuario(tipoTb, usuario);
            }

            // Preenche o popup do usuário (nome e email)
            if (PopupNomeText != null) PopupNomeText.Text = usuario.Nome;
            if (PopupEmailText != null) PopupEmailText.Text = usuario.Email;
        }

        // Define a cor do texto do tipo do usuário conforme o enum TipoUsuario
        private void ConfigurarCorTipoUsuario(TextBlock tipoTextBlock, Usuarios usuario)
        {
            if (tipoTextBlock == null) return;

            switch (usuario.Tipo)
            {
                case TipoUsuario.Basic:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(59, 130, 246)); // azul
                    break;
                case TipoUsuario.Premium:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11));  // laranja
                    break;
                case TipoUsuario.Adm:
                    tipoTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));   // vermelho
                    break;
            }
        }

        // Exibe o saldo do usuário (positivo verde, negativo vermelho)
        private void ConfigurarSaldoUsuario(Usuarios usuario)
        {
            if (BalanceText == null) return;
            BalanceText.Text = $"R$ {usuario.saldoDisponivel:F2}";
            BalanceText.Foreground = usuario.saldoDisponivel >= 0
                ? new SolidColorBrush(Color.FromRgb(34, 197, 94))  // verde
                : new SolidColorBrush(Color.FromRgb(239, 68, 68)); // vermelho
        }

        // ============================================================
        // ESTATÍSTICAS (totais de receitas e despesas)
        // ============================================================

        // Busca todas as transações do usuário e calcula os totais de receitas e despesas.
        // Executa em background (Task.Run) para não travar a interface.
        private async void CarregarEstatisticas()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;
            try
            {
                var transacoes = await Task.Run(() =>
                    _transacaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id));

                if (transacoes.Any())
                    CalcularTotaisTransacoes(transacoes);
                else
                    ExibirTotaisZerados();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar estatísticas: {ex.Message}");
                ExibirTotaisZerados();
            }
        }

        // Soma os valores das receitas e despesas e atualiza os TextBlocks
        private void CalcularTotaisTransacoes(List<Transacao> transacoes)
        {
            var totalReceitas = transacoes.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao);
            var totalDespesas = transacoes.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao);
            MaxReceitaText.Text = $"R$ {totalReceitas:F2}";
            MaxDespesaText.Text = $"R$ {totalDespesas:F2}";
        }

        // Caso não haja transações, exibe "R$ 0,00" em ambos os campos
        private void ExibirTotaisZerados()
        {
            MaxReceitaText.Text = "R$ 0,00";
            MaxDespesaText.Text = "R$ 0,00";
        }

        // ============================================================
        // GRÁFICO DE EVOLUÇÃO FINANCEIRA (LiveCharts)
        // ============================================================

        // Carrega as transações e as agrupa por mês para alimentar o gráfico.
        private async void CarregarDadosGrafico()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;
            try
            {
                var transacoes = await Task.Run(() =>
                    _transacaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id));

                var dadosPorMes = AgruparTransacoesPorMes(transacoes);

                // Se não houver dados, limpa as séries do gráfico
                if (!dadosPorMes.Any())
                {
                    FinanceChart.Series = new SeriesCollection();
                    // Exibir uma mensagem dentro do gráfico (texto centralizado)
                    var emptyMessage = new CartesianChart
                    {
                        Series = new SeriesCollection(),
                        AxisX = { new Axis { Labels = new[] { "" } } },
                        AxisY = { new Axis { LabelFormatter = value => "" } }
                    };
                    // Não há suporte direto, mas podemos adicionar um TextBlock sobreposto.
                    // Alternativa mais simples: mostrar MessageBox.
                    MessageBox.Show("Nenhuma transação encontrada para exibir no gráfico.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                AtualizarGrafico(dadosPorMes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar gráfico: {ex.Message}");
            }
        }

        // Agrupa as transações por mês/ano, somando receitas e despesas de cada período.
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

        // Cria as séries do gráfico (Saldo Acumulado, Receitas Mensais, Despesas Mensais)
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

            AtualizarEstadoGrafico(dadosPorMes.Any());
            ConfigurarEixosGrafico(labels);
        }

        // Corrija a definição de AtualizarEstadoGrafico para fora do método AtualizarGrafico:
        private void AtualizarEstadoGrafico(bool temDados)
        {
            GraficoVazioText.Visibility = temDados ? Visibility.Collapsed : Visibility.Visible;
            FinanceChart.Visibility = temDados ? Visibility.Visible : Visibility.Collapsed;
        }

        // Processa os dados mensais: calcula saldo acumulado, gera listas para o gráfico e os rótulos (labels)
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
                labels.Add(mes.Mes.ToString("MMM/yy")); // ex: "Jan/24"
            }

            return (saldoValues, receitaValues, despesaValues, labels);
        }

        // Configura os eixos X e Y do gráfico com título e formatação monetária
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

        // ============================================================
        // POPUP DO USUÁRIO (exibe perfil e opções ao clicar na sidebar)
        // ============================================================

        // Alterna a visibilidade do popup e do overlay (fundo semi-transparente)
        private void UsuarioCard_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = !_popupAberto;
            UserPopupCard.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            PopupOverlay.Visibility = _popupAberto ? Visibility.Visible : Visibility.Collapsed;
            e.Handled = true; // impede que o evento se propague
        }

        // Fecha o popup quando clicar no overlay (área escura fora do popup)
        private void FecharPopup_Click(object sender, MouseButtonEventArgs e)
        {
            _popupAberto = false;
            UserPopupCard.Visibility = Visibility.Collapsed;
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        // Abre a tela de configurações (ConfiguracoesWindow) e fecha a atual
        private void PopupConfiguracoes_Click(object sender, RoutedEventArgs e)
        {
            _popupAberto = false;
            UserPopupCard.Visibility = Visibility.Collapsed;
            PopupOverlay.Visibility = Visibility.Collapsed;
            var telaConfiguracoes = new ConfiguracoesWindow();
            telaConfiguracoes.Show();
            this.Close();
        }

        // Faz logout: limpa a sessão, abre a tela de login e fecha a atual
        private void PopupLogout_Click(object sender, RoutedEventArgs e)
        {
            SessaoUsuario.Logout();
            new UserLogin().Show();
            this.Close();
        }

        // ============================================================
        // NAVEGAÇÃO - SIDEBAR (botões do menu lateral)
        // ============================================================

        // Botão "Finanças" - atualmente sem ação (a própria tela inicial já é finanças)
        private void FinanceButton_Click(object sender, RoutedEventArgs e) { }

        // Abre a tela de cartões
        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            new NommusProject.Views.cartoes().Show();
            this.Close();
        }

        // Abre a tela de despesas (ExpensesWindow)
        private void ExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExpensesWindow().Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Abre a tela de receitas (IncomeWindow)
        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            new IncomeWindow().Show();
            this.Close();
        }

        // Abre a tela de metas (MetasWindow)
        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            new MetasWindow().Show();
            this.Close();
        }

        // Botão "Relatórios"
        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var transacoes = _transacaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id);
                if (!transacoes.Any())
                {
                    MessageBox.Show("Nenhuma transação para gerar relatório.", "Aviso",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"Relatorio_Financeiro_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog() == true)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Descrição;Tipo;Valor;Data;Categoria");
                    foreach (var t in transacoes)
                    {
                        sb.AppendLine($"{t.DescricaoTransacao};{t.TipoTransacao};{t.ValorTransacao:F2};{t.DataTransacao:dd/MM/yyyy};{t.CategoriaId}");
                    }
                    System.IO.File.WriteAllText(dialog.FileName, sb.ToString());

                    MessageBox.Show("Relatório CSV gerado com sucesso!", "Sucesso",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar relatório: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Classe auxiliar interna para armazenar dados agregados por mês (usada no gráfico)
    internal class DadosMes
    {
        public DateTime Mes { get; set; }      // Primeiro dia do mês
        public double Receitas { get; set; }  // Soma das receitas no mês
        public double Despesas { get; set; }  // Soma das despesas no mês
    }
}