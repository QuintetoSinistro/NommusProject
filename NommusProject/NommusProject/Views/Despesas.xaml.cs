using Microsoft.Data.Sqlite;
using Nommus;           // ATENÇÃO: namespace inconsistente (deveria ser NommusProject)
using NommusProject.Data;
using NommusProject.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
namespace NommusProject
{
    // Tela de gerenciamento de despesas (gastos)
    public partial class ExpensesWindow : Window
    {
        // Controla se o popup do usuário está visível
        private bool _popupAberto = false;

        // Repositório de transações para acesso ao banco de dados
        private TransacaoRepository _transacaoRepo = new TransacaoRepository();

        // Lista em memória das despesas carregadas (usada para exibição e exclusão em lote)
        private List<Transacao> _despesas = new List<Transacao>();

        // Construtor: inicializa os componentes, carrega dados do usuário,
        // categorias, despesas e cartões.
        public ExpensesWindow()
        {
            InitializeComponent();
            Utils.MaskHelper.AplicarMascaraValor(ValueTextBox);
            CarregarDadosUsuario();
            CarregarCategorias();
            CarregarDespesas();
            CarregarCartoes();
        }

        // ============================================================
        // DADOS DO USUÁRIO (sidebar e popup)
        // ============================================================

        // Carrega nome e email do usuário logado nos controles da tela
        private void CarregarDadosUsuario()
        {
            var usuario = SessaoUsuario.UsuarioLogado;
            if (usuario == null) return;
            UsuarioNomeText.Text = usuario.Nome;
            PopupNomeText.Text = usuario.Nome;
            PopupEmailText.Text = usuario.Email;

            // Carrega a foto
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
                    var defaultUri = new Uri("pack://application:,,,/Views/Images/user.png", UriKind.Absolute);
                    SidebarFotoBrush.ImageSource = new BitmapImage(defaultUri);
                }
            }
            catch { SidebarFotoBrush.ImageSource = null; }
        }

        // ============================================================
        // CARREGAMENTO DE CATEGORIAS (do banco de dados)
        // ============================================================

        // Preenche o ComboBox de categorias com os registros da tabela Categorias.
        // Se não houver categorias, adiciona "Outros" como padrão.
        private void CarregarCategorias()
        {
            CategoryComboBox.Items.Clear();
            // Opção padrão "Outros" com Tag = null (representa categoria não informada)
            CategoryComboBox.Items.Add(new ComboBoxItem { Content = "Outros", Tag = null });

            try
            {
                using var conn = new SqliteConnection($"Data Source={DatabaseInitializer.DbPath}");
                conn.Open();
                using var cmd = new SqliteCommand("SELECT IdCategoria, NomeCategoria FROM Categorias ORDER BY NomeCategoria", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var nome = reader.GetString(1);
                    // Armazena o IdCategoria no Tag do item
                    CategoryComboBox.Items.Add(new ComboBoxItem { Content = nome, Tag = id });
                }
            }
            catch
            {
                // Se a tabela Categorias não existir ou der erro, apenas ignora
            }

            // Garante que haja pelo menos um item
            if (CategoryComboBox.Items.Count == 0)
                CategoryComboBox.Items.Add(new ComboBoxItem { Content = "Outros", Tag = null });

            CategoryComboBox.SelectedIndex = 0;
        }

        // ============================================================
        // CARREGAMENTO DE DESPESAS
        // ============================================================

        // Busca todas as despesas do usuário no banco e as exibe no ItemsControl
        private void CarregarDespesas()
        {
            var usuarioId = SessaoUsuario.UsuarioLogado?.Id ?? 0;
            _despesas = _transacaoRepo.GetByUsuarioAndTipo(usuarioId, "Despesa");
            DespesasItemsControl.ItemsSource = _despesas;
        }

        // ============================================================
        // CARREGAMENTO DE CARTÕES (para pagamento com crédito)
        // ============================================================

        // Preenche o ComboBox de cartões com os cartões cadastrados pelo usuário.
        // O primeiro item é "Nenhum" (Tag = null), indicando que não será associado a cartão.
        private void CarregarCartoes()
        {
            CardComboBox.Items.Clear();
            CardComboBox.Items.Add(new ComboBoxItem { Content = "Nenhum", Tag = null });

            var cartaoRepo = new CartaoRepository();
            var cartoes = cartaoRepo.GetByUsuario(SessaoUsuario.UsuarioLogado.Id);
            foreach (var c in cartoes)
            {
                CardComboBox.Items.Add(new ComboBoxItem { Content = c.NomeCartao, Tag = c.IdCartao });
            }
            CardComboBox.SelectedIndex = 0;
        }

        // ============================================================
        // INTERAÇÃO ENTRE MEIO DE PAGAMENTO E SELEÇÃO DE CARTÃO
        // ============================================================

        // Quando o usuário seleciona "Crédito" como meio de pagamento, habilita o ComboBox de cartões;
        // caso contrário, desabilita e seleciona "Nenhum".
        private void PaymentMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaymentMethodComboBox.SelectedItem is ComboBoxItem item)
            {
                bool isCredito = item.Content.ToString() == "Crédito";
                CardComboBox.IsEnabled = isCredito;
                if (!isCredito) CardComboBox.SelectedIndex = 0;
            }
        }

        // ============================================================
        // ADIÇÃO DE NOVA DESPESA
        // ============================================================

        // Evento do botão "Adicionar Gasto": valida os campos, cria um objeto Transacao
        // e salva no banco de dados.
        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            // Validação da descrição
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Informe a descrição.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validação do valor (deve ser número positivo, formato "100,50")
            if (!double.TryParse(ValueTextBox.Text, out double valor) || valor <= 0)
            {
                MessageBox.Show("Valor inválido.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validação da data (se inválida, usa a data atual)
            if (!DateTime.TryParse(DateTextBox.Text, out DateTime data))
                data = DateTime.Today;

            // Obtém o ID da categoria selecionada (Tag do ComboBoxItem)
            // Se não for um número inteiro ou não houver seleção, categoriaId permanece null
            int? categoriaId = null;
            if (CategoryComboBox.SelectedItem is ComboBoxItem catItem && catItem.Tag is int catId)
            {
                categoriaId = catId;
            }

            // Define o tipo de gasto com base nos RadioButtons
            string tipoGasto = FixedRadio.IsChecked == true ? "Fixo" : (VariableRadio.IsChecked == true ? "Variável" : "Emergência");

            // Forma de pagamento selecionada
            string formaPagamento = (PaymentMethodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Débito";

            // Se for crédito e houver um cartão válido (Tag > 0), associa o cartão
            int? cartaoId = null;
            if (formaPagamento == "Crédito" && CardComboBox.SelectedItem is ComboBoxItem cardItem && cardItem.Tag is int cardId && cardId > 0)
            {
                cartaoId = cardId;
            }

            // Cria o objeto Transacao do tipo Despesa
            var despesa = new Transacao
            {
                DescricaoTransacao = DescriptionTextBox.Text,
                TipoTransacao = "Despesa",
                ValorTransacao = valor,
                DataTransacao = data,
                FormaPagamento = formaPagamento,
                CategoriaId = categoriaId,          // Pode ser null (NULL no banco)
                CartaoId = cartaoId,                // Pode ser null
                UsuarioId = SessaoUsuario.UsuarioLogado.Id,
                CondicaoPagamento = tipoGasto
            };

            // Salva no banco via repositório
            _transacaoRepo.Add(despesa);

            // Limpa o formulário e recarrega a lista
            LimparFormulario();
            CarregarDespesas();
        }

        // ============================================================
        // EXCLUSÃO DE DESPESA INDIVIDUAL
        // ============================================================

        // Evento disparado pelo botão "✕" em cada item da lista.
        // O Tag do botão contém o IdTransacao a ser removido.
        private void RemoverDespesa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (MessageBox.Show("Remover esta despesa?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _transacaoRepo.Delete(id);
                    CarregarDespesas();
                }
            }
        }

        // ============================================================
        // EXCLUSÃO DE TODAS AS DESPESAS
        // ============================================================

        // Remove todas as despesas do usuário (com confirmação)
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Excluir TODAS as despesas?", "Atenção", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach (var d in _despesas)
                    _transacaoRepo.Delete(d.IdTransacao);
                CarregarDespesas();
            }
        }

        // ============================================================
        // FILTRO E EXPORTAÇÃO
        // ============================================================

        // Abre uma tela de filtro
        private void FilterExpenses_Click(object sender, RoutedEventArgs e)
        {
            // Popup simples para selecionar período
            var inputDialog = new Window
            {
                Title = "Filtrar Despesas",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(10),
                    Children =
            {
                new TextBlock { Text = "Data inicial (dd/MM/yyyy):", Margin = new Thickness(0,0,0,5) },
                new TextBox { Name = "DataInicioBox", Text = DateTime.Now.AddMonths(-1).ToString("dd/MM/yyyy") },
                new TextBlock { Text = "Data final (dd/MM/yyyy):", Margin = new Thickness(0,10,0,5) },
                new TextBox { Name = "DataFimBox", Text = DateTime.Now.ToString("dd/MM/yyyy") },
                new Button { Content = "Filtrar", Height = 30, Margin = new Thickness(0,15,0,0) }
            }
                }
            };
            var btn = (inputDialog.Content as StackPanel).Children[4] as Button;
            btn.Click += (s, args) =>
            {
                var dataInicioBox = (inputDialog.Content as StackPanel).Children[1] as TextBox;
                var dataFimBox = (inputDialog.Content as StackPanel).Children[3] as TextBox;

                if (DateTime.TryParseExact(dataInicioBox?.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var inicio) &&
                    DateTime.TryParseExact(dataFimBox?.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fim))
                {
                    _despesas = _despesas.Where(d => d.DataTransacao >= inicio && d.DataTransacao <= fim).ToList();
                    DespesasItemsControl.ItemsSource = _despesas;
                    inputDialog.Close();
                }
                else
                    MessageBox.Show("Datas inválidas.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            };
            inputDialog.Owner = this;
            inputDialog.ShowDialog();
        }

        // Exporta a lista de despesas para um arquivo CSV
        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.FileName = $"Despesas_{DateTime.Now:yyyyMMddHHmmss}.csv";
            if (dialog.ShowDialog() == true)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Descrição;Valor;Data;Categoria");
                foreach (var d in _despesas)
                    sb.AppendLine($"{d.DescricaoTransacao};{d.ValorTransacao:F2};{d.DataTransacao:dd/MM/yyyy};{d.CategoriaId}");
                System.IO.File.WriteAllText(dialog.FileName, sb.ToString());
                MessageBox.Show("Relatório exportado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ============================================================
        // PLANEJAMENTO DE GASTOS FUTUROS (simulado)
        // ============================================================

        // Salva um plano de gasto futuro (apenas simulação – não persiste no banco)
        private void SaveFuturePlan_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Planejamento futuro salvo (simulação).", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ============================================================
        // UTILITÁRIOS
        // ============================================================

        // Limpa os campos do formulário de adição de despesa
        private void LimparFormulario()
        {
            DescriptionTextBox.Text = "";
            ValueTextBox.Text = "0,00";
            DateTextBox.Text = DateTime.Today.ToString("dd/MM/yyyy");
            CategoryComboBox.SelectedIndex = 0;      // Seleciona "Outros"
            PaymentMethodComboBox.SelectedIndex = 0;  // Seleciona "Selecione..." (ou primeiro item)
            FixedRadio.IsChecked = true;              // Marca "Fixo" como padrão
        }

        // ============================================================
        // POPUP DO USUÁRIO E NAVEGAÇÃO
        // ============================================================

        // Os métodos abaixo são responsáveis pelo popup de perfil e navegação entre telas.
        // (Alguns corpos estão vazios neste trecho, mas devem ser preenchidos conforme necessário)

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
        private void CardsButton_Click(object sender, RoutedEventArgs e) { new Views.cartoes().Show(); Close(); }
        private void ExpensesButton_Click(object sender, RoutedEventArgs e) { }
        private void CreditsButton_Click(object sender, RoutedEventArgs e) { new IncomeWindow().Show(); Close(); }
        private void GoalsButton_Click(object sender, RoutedEventArgs e) { new MetasWindow().Show(); Close(); }
        private void BackToDashboard_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); Close(); }
        private void PlanFutureExpense_Click(object sender, RoutedEventArgs e)
        {
            // Alterna a visibilidade do painel de planejamento futuro
            FuturePlanningPanel.Visibility = FuturePlanningPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}