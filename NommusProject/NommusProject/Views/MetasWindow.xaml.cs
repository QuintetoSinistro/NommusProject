using LiveCharts;
using LiveCharts.Wpf;
using NommusProject.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NommusProject
{
    // Tela de Métricas e Metas de Economia
    public partial class MetasWindow : Window
    {
        // Repositórios para acessar transações e metas no banco de dados
        private readonly TransacaoRepository _transacaoRepo = new TransacaoRepository();
        private readonly MetasRepository _metaRepo = new MetasRepository();

        // Controla se o popup do usuário está visível
        private bool _popupAberto = false;

        // Dias do filtro atual (30, 180 ou 365)
        private int _diasFiltro = 30;

        // Lista em memória das metas carregadas do banco
        private List<Metas> _metas = new List<Metas>();

        // Construtor: inicializa os componentes XAML, carrega dados do usuário,
        // lista de metas e as métricas/gráficos.
        public MetasWindow()
        {
            InitializeComponent();
            CarregarUsuario();
            CarregarMetas();
            CarregarMetricas();
        }

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
                    bitmap.UriSource = new Uri(caminhoFoto, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
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
        // METAS DE ECONOMIA (persistência no banco)
        // ============================================================

        // Carrega todas as metas do usuário e as exibe no ItemsControl,
        // convertendo cada Metas para MetaViewModel (que possui propriedades
        // de progresso como Percentual e LarguraProgresso).
        private void CarregarMetas()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;
            _metas = _metaRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id);

            // Converte para ViewModel para exibição na interface
            var metasVM = _metas.Select(m => new MetaViewModel
            {
                Id = m.IdMeta,
                Nome = m.NomeMeta,
                Objetivo = m.ValorMeta,
                Atual = m.ValorAtual   // valor economizado até o momento
            }).ToList();

            ListaMetas.ItemsSource = metasVM;
            MetasVazioPanel.Visibility = metasVM.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        // Abre o popup para criação de uma nova meta
        private void NovaMeta_Click(object sender, RoutedEventArgs e)
        {
            MetaNomeBox.Text = "";
            MetaObjetivoBox.Text = "";
            MetaAtualBox.Text = "";
            NovaMetaPopup.Visibility = Visibility.Visible;
            NovaMetaOverlay.Visibility = Visibility.Visible;
        }

        // Fecha o popup de nova meta (cancelar)
        private void FecharNovaMeta_Click(object sender, RoutedEventArgs e)
        {
            NovaMetaPopup.Visibility = Visibility.Collapsed;
            NovaMetaOverlay.Visibility = Visibility.Collapsed;
        }

        // Cria uma nova meta com os dados informados e salva no banco
        private void CriarMeta_Click(object sender, RoutedEventArgs e)
        {
            // Validações
            var nome = MetaNomeBox.Text.Trim();
            if (string.IsNullOrEmpty(nome))
            {
                MessageBox.Show("Informe o nome da meta.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Valida o valor objetivo (deve ser número positivo)
            if (!double.TryParse(MetaObjetivoBox.Text.Replace(",", "."), out double objetivo) || objetivo <= 0)
            {
                MessageBox.Show("Informe um valor objetivo válido (ex: 1000,00).", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Valor atual economizado (opcional, pode ser zero)
            double.TryParse(MetaAtualBox.Text.Replace(",", "."), out double atual);
            if (atual > objetivo) atual = objetivo;

            // Cria o objeto Meta
            var novaMeta = new Metas
            {
                NomeMeta = nome,
                ValorMeta = objetivo,
                DataInicial = DateTime.Now,
                DataFinal = DateTime.Now.AddMonths(6),   // prazo padrão de 6 meses (pode ser editado depois)
                StatusMeta = false,
                IdUsuario = SessaoUsuario.UsuarioLogado.Id,
                ValorAtual = atual
            };

            // Salva no banco via repositório
            _metaRepo.Add(novaMeta);

            // Recarrega a lista e fecha o popup
            CarregarMetas();
            FecharNovaMeta_Click(sender, e);
            MessageBox.Show("Meta criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AdicionarEconomia_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idMeta)
            {
                var meta = _metas.FirstOrDefault(m => m.IdMeta == idMeta);
                if (meta == null) return;

                var inputDialog = new Window
                {
                    Title = "Adicionar Economia",
                    Width = 320,
                    Height = 180,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    Content = new StackPanel
                    {
                        Margin = new Thickness(15),
                        Children =
                {
                    new TextBlock { Text = $"Meta: {meta.NomeMeta}", FontWeight = FontWeights.Bold, Margin = new Thickness(0,0,0,10) },
                    new TextBlock { Text = $"Valor atual: {meta.ValorAtual:C}", Margin = new Thickness(0,0,0,5) },
                    new TextBlock { Text = $"Faltam: {meta.ValorMeta - meta.ValorAtual:C}", Margin = new Thickness(0,0,0,10) },
                    new TextBlock { Text = "Valor a adicionar:", Margin = new Thickness(0,0,0,5) },
                    new TextBox { Name = "ValorBox", Text = "0,00", Margin = new Thickness(0,0,0,15) },
                    new Button { Content = "Adicionar", Height = 35, IsDefault = true }
                }
                    }
                };

                var btnOk = (inputDialog.Content as StackPanel).Children[5] as Button;
                btnOk.Click += (s, args) =>
                {
                    var txtValor = (inputDialog.Content as StackPanel).Children[4] as TextBox;
                    if (double.TryParse(txtValor.Text, out double valor) && valor > 0)
                    {
                        double novoAtual = Math.Min(meta.ValorAtual + valor, meta.ValorMeta);
                        meta.ValorAtual = novoAtual;
                        _metaRepo.Update(meta);
                        CarregarMetas();
                        inputDialog.Close();
                    }
                    else
                        MessageBox.Show("Valor inválido. Use formato como 100,50", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                };
                inputDialog.Owner = this;
                inputDialog.ShowDialog();
            }
        }

        // ============================================================
        // MÉTRICAS E GRÁFICOS (receitas, despesas, saldo, pizza, top gastos)
        // ============================================================

        // Carrega as transações do período selecionado e atualiza os cards, gráficos e top gastos
        private async void CarregarMetricas()
        {
            if (SessaoUsuario.UsuarioLogado == null) return;

            var dataInicio = DateTime.Now.AddDays(-_diasFiltro);
            // Executa em background para não travar a interface
            var transacoes = await System.Threading.Tasks.Task.Run(() =>
                _transacaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id)
                     .Where(t => t.DataTransacao >= dataInicio)
                     .ToList());

            AtualizarCards(transacoes);
            AtualizarGraficoBarras(transacoes);   // atualmente vazio (não há gráfico de barras no XAML)
            AtualizarGraficoPizza(transacoes);
            AtualizarTopGastos(transacoes);
        }

        // Atualiza os cards de resumo: Receitas, Despesas, Saldo no período e Taxa de economia
        private void AtualizarCards(List<Transacao> transacoes)
        {
            var receitas = transacoes.Where(t => t.TipoTransacao == "Receita").ToList();
            var despesas = transacoes.Where(t => t.TipoTransacao == "Despesa").ToList();
            var totRec = receitas.Sum(t => t.ValorTransacao);
            var totDesp = despesas.Sum(t => t.ValorTransacao);
            var saldo = totRec - totDesp;
            var taxa = totRec > 0 ? (saldo / totRec) * 100 : 0;

            // Formata os valores como moeda (R$)
            CardReceitaTotal.Text = totRec.ToString("C");
            CardReceitaQtd.Text = $"{receitas.Count} transações";
            CardDespesaTotal.Text = totDesp.ToString("C");
            CardDespesaQtd.Text = $"{despesas.Count} transações";
            CardSaldoPeriodo.Text = saldo.ToString("C");
            CardSaldoPeriodo.Foreground = saldo >= 0
                ? new SolidColorBrush(Color.FromRgb(59, 130, 246))  // azul se positivo
                : new SolidColorBrush(Color.FromRgb(239, 68, 68));    // vermelho se negativo
            CardTaxaEconomia.Text = $"{Math.Max(0, taxa):F1}%";
        }

        // Método reservado para gráfico de barras (não implementado porque o XAML não tem CartesianChart nesta tela)
        private void AtualizarGraficoBarras(List<Transacao> transacoes)
        {
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
                GraficoBarras.Series = new SeriesCollection();
                return;
            }

            var receitasValues = new ChartValues<double>();
            var despesasValues = new ChartValues<double>();
            var labels = new List<string>();

            foreach (var mes in dadosPorMes)
            {
                receitasValues.Add(mes.Receitas);
                despesasValues.Add(mes.Despesas);
                labels.Add(mes.Mes.ToString("MMM/yy"));
            }

            GraficoBarras.Series = new SeriesCollection
    {
        new ColumnSeries { Title = "Receitas", Values = receitasValues, Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129)) },
        new ColumnSeries { Title = "Despesas", Values = despesasValues, Fill = new SolidColorBrush(Color.FromRgb(239, 68, 68)) }
    };

            // Configurar eixo X
            GraficoBarras.AxisX.Clear();
            GraficoBarras.AxisX.Add(new Axis
            {
                Title = "Mês",
                Labels = labels.ToArray(),
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                Separator = DefaultAxes.CleanSeparator
            });

            // Configurar eixo Y com formatação de moeda
            GraficoBarras.AxisY.Clear();
            GraficoBarras.AxisY.Add(new Axis
            {
                Title = "R$",
                LabelFormatter = value => value.ToString("C"), // Lambda em C# resolve a formatação
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                Separator = DefaultAxes.CleanSeparator
            });
        }

        // Atualiza o gráfico de pizza (donut) com a proporção Receitas vs Despesas
        private void AtualizarGraficoPizza(List<Transacao> transacoes)
        {
            var totRec = transacoes.Where(t => t.TipoTransacao == "Receita").Sum(t => t.ValorTransacao);
            var totDesp = transacoes.Where(t => t.TipoTransacao == "Despesa").Sum(t => t.ValorTransacao);

            GraficoPizza.Series = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Receitas",
                    Values = new ChartValues<double> { totRec },
                    Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129)), // verde
                    DataLabels = false
                },
                new PieSeries
                {
                    Title = "Despesas",
                    Values = new ChartValues<double> { totDesp },
                    Fill = new SolidColorBrush(Color.FromRgb(239, 68, 68)),   // vermelho
                    DataLabels = false
                }
            };
        }

        // Exibe as 5 maiores despesas do período no ItemsControl "Top 5 Gastos"
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
        // FILTROS DE PERÍODO (30 dias, 6 meses, 1 ano)
        // ============================================================

        private void Filtro30_Click(object sender, RoutedEventArgs e) => AplicarFiltro(30);
        private void Filtro6M_Click(object sender, RoutedEventArgs e) => AplicarFiltro(180);
        private void Filtro1A_Click(object sender, RoutedEventArgs e) => AplicarFiltro(365);

        // Aplica o filtro alterando a variável de dias, atualizando o estilo dos botões,
        // o texto do subtítulo e recarregando as métricas.
        private void AplicarFiltro(int dias)
        {
            _diasFiltro = dias;
            // Atualiza o estilo dos botões para destacar o ativo
            BtnFiltro30.Style = dias == 30 ? (Style)FindResource("FiltroAtivoButtonStyle") : (Style)FindResource("FiltroButtonStyle");
            BtnFiltro6M.Style = dias == 180 ? (Style)FindResource("FiltroAtivoButtonStyle") : (Style)FindResource("FiltroButtonStyle");
            BtnFiltro1A.Style = dias == 365 ? (Style)FindResource("FiltroAtivoButtonStyle") : (Style)FindResource("FiltroButtonStyle");
            SubtituloText.Text = dias == 30 ? "Últimos 30 dias" : dias == 180 ? "Últimos 6 meses" : "Último ano";
            CarregarMetricas();
        }

        // ============================================================
        // POPUP DO USUÁRIO (menu de perfil)
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
        // NAVEGAÇÃO PELOS BOTÕES DA SIDEBAR
        // ============================================================

        private void FinanceButton_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); this.Close(); }
        private void CardsButton_Click(object sender, RoutedEventArgs e) { new Views.cartoes().Show(); this.Close(); }
        private void ExpensesButton_Click(object sender, RoutedEventArgs e) { new ExpensesWindow().Show(); this.Close(); }
        private void CreditsButton_Click(object sender, RoutedEventArgs e) { new IncomeWindow().Show(); this.Close(); }
        private void GoalsButton_Click(object sender, RoutedEventArgs e) { /* já está na tela de metas */ }
    }

    // ============================================================
    // VIEW MODELS (modelos para exibição na interface)
    // ============================================================

    // ViewModel para exibir uma meta de economia na lista
    public class MetaViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public double Objetivo { get; set; }
        public double Atual { get; set; }

        // Percentual de progresso (calculado automaticamente)
        public double Percentual => Objetivo > 0 ? (Atual / Objetivo) * 100 : 0;

        // Texto formatado do percentual (ex: "30.5% concluído")
        public string PercentualTexto => $"{Percentual:F1}% concluído";

        // Largura da barra de progresso (máximo 600px)
        public double LarguraProgresso => Math.Min(Atual / (Objetivo > 0 ? Objetivo : 1), 1.0) * 600;
    }

    // ViewModel para exibir um item do top 5 maiores gastos
    public class TopGastoViewModel
    {
        public string Posicao { get; set; }   // "1", "2", "3", ...
        public string Descricao { get; set; } // Descrição da transação
        public string Data { get; set; }      // Data formatada
        public double Valor { get; set; }     // Valor da despesa
    }
}