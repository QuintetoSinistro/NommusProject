using Nommus;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace NommusProject
{
    public partial class ExpensesWindow : Window
    {
        private ObservableCollection<Transacao> _despesasCollection;

        public ExpensesWindow()
        {
            InitializeComponent();
            _despesasCollection = new ObservableCollection<Transacao>();
            DespesasItemsControl.ItemsSource = _despesasCollection;

            CarregarDadosUsuario();
            CarregarDespesas();
        }

        private void CarregarDadosUsuario()
        {
            if (SessaoUsuario.UsuarioLogado != null)
            {
                var usuario = SessaoUsuario.UsuarioLogado;

                // Personalizar perfil do usuário
                if (UsuarioNomeText != null)
                    UsuarioNomeText.Text = usuario.Nome;

                if (UsuarioTipoText != null)
                {
                    UsuarioTipoText.Text = usuario.Tipo.ToString();
                    // Personalizar cor baseada no tipo
                    switch (usuario.Tipo)
                    {
                        case TipoUsuario.Basic:
                            UsuarioTipoText.Foreground = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(59, 130, 246)); // Azul
                            break;
                        case TipoUsuario.Premium:
                            UsuarioTipoText.Foreground = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(245, 158, 11)); // Dourado
                            break;
                        case TipoUsuario.Adm:
                            UsuarioTipoText.Foreground = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(239, 68, 68)); // Vermelho
                            break;
                    }
                }
            }
        }

        private async void CarregarDespesas()
        {
            try
            {
                if (SessaoUsuario.UsuarioLogado != null)
                {
                    var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(SessaoUsuario.UsuarioLogado.Id);
                    var despesas = transacoes.Where(t => t.TipoTransacao == "Despesa")
                                           .OrderByDescending(r => r.DataTransacao)
                                           .ToList();

                    _despesasCollection.Clear();
                    foreach (var despesa in despesas)
                    {
                        _despesasCollection.Add(despesa);
                    }

                    // Controle de mensagem vazia
                    var stackPanel = DespesasItemsControl.Parent as StackPanel;
                    if (stackPanel != null)
                    {
                        var emptyTextBlock = stackPanel.Children
                            .OfType<TextBlock>()
                            .FirstOrDefault(tb => tb.Text.Contains("Nenhum gasto"));

                        if (!despesas.Any() && emptyTextBlock == null)
                        {
                            var emptyText = new TextBlock
                            {
                                Text = "Nenhum gasto registrado ainda...",
                                Foreground = new System.Windows.Media.SolidColorBrush(
                                    System.Windows.Media.Color.FromRgb(148, 163, 184)),
                                FontSize = 14,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 20, 0, 0)
                            };
                            stackPanel.Children.Add(emptyText);
                        }
                        else if (despesas.Any() && emptyTextBlock != null)
                        {
                            stackPanel.Children.Remove(emptyTextBlock);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar despesas: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Por favor, insira uma descrição para o gasto.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ValueTextBox.Text) || ValueTextBox.Text == "0,00")
            {
                MessageBox.Show("Por favor, insira um valor válido para o gasto.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Converter valor
                decimal valorDecimal = Convert.ToDecimal(ValueTextBox.Text.Replace("R$", "").Trim());
                double valor = (double)valorDecimal;

                // Obter categoria selecionada
                string categoria = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Outros";

                // Obter tipo de gasto
                string tipoGasto = "Fixo";
                if (VariableRadio.IsChecked == true) tipoGasto = "Variável";
                if (EmergencyRadio.IsChecked == true) tipoGasto = "Emergência";

                // Converter data
                DateTime data = DateTime.Parse(DateTextBox.Text);

                // ✅ DEIXAR APENAS o AdicionarTransacaoAsync cuidar do saldo
                double saldoAntes = SessaoUsuario.UsuarioLogado.saldoDisponivel;

                // Criar e salvar despesa
                var despesa = new Despesa
                {
                    DescricaoTransacao = DescriptionTextBox.Text,
                    ValorTransacao = valor,
                    CategoriaId = categoria.ToLower().Replace(" ", "-"),
                    DataTransacao = data,
                    DespesaRecorrente = tipoGasto == "Fixo",
                    DespesaEssencial = tipoGasto != "Emergência",
                    UsuarioId = SessaoUsuario.UsuarioLogado.Id,
                    TipoTransacao = "Despesa"
                };

                // ⚠️ APENAS ESTA LINHA atualiza o saldo
                await despesa.AdicionarTransacaoAsync();

                // ✅ AGORA: Recarregar usuário ATUALIZADO do arquivo
                var usuarioAtualizado = await Usuario.BuscarUsuarioPorIdAsync(SessaoUsuario.UsuarioLogado.Id);
                SessaoUsuario.UsuarioLogado = usuarioAtualizado;

                MessageBox.Show($"Gasto adicionado com sucesso!\n" +
                               $"Saldo antes: R$ {saldoAntes:F2}\n" +
                               $"Saldo atual: R$ {usuarioAtualizado.saldoDisponivel:F2}",
                               "Sucesso",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpar campos
                DescriptionTextBox.Text = "";
                ValueTextBox.Text = "0,00";
                CategoryComboBox.SelectedIndex = -1;
                FixedRadio.IsChecked = true;
                DateTextBox.Text = DateTime.Today.ToString("dd/MM/yyyy");

                // Atualizar lista
                _despesasCollection.Insert(0, despesa);
            }
            catch (FormatException)
            {
                MessageBox.Show("Por favor, insira um valor numérico válido.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar gasto: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RemoverDespesa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                try
                {
                    if (button.DataContext is Transacao transacao)
                    {
                        var resultado = MessageBox.Show("Tem certeza que deseja remover este gasto?",
                                                      "Confirmar Remoção",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

                        if (resultado == MessageBoxResult.Yes)
                        {
                            // ✅ DEIXAR APENAS o ExcluirTransacaoAsync cuidar do saldo
                            double saldoAntes = SessaoUsuario.UsuarioLogado.saldoDisponivel;

                            // ⚠️ APENAS ESTA LINHA atualiza o saldo
                            await transacao.ExcluirTransacaoAsync();

                            // ✅ AGORA: Recarregar usuário ATUALIZADO do arquivo
                            var usuarioAtualizado = await Usuario.BuscarUsuarioPorIdAsync(SessaoUsuario.UsuarioLogado.Id);
                            SessaoUsuario.UsuarioLogado = usuarioAtualizado;

                            // Recarregar a lista
                            CarregarDespesas();

                            MessageBox.Show($"Gasto removido com sucesso!\n" +
                                          $"Saldo antes: R$ {saldoAntes:F2}\n" +
                                          $"Saldo atual: R$ {usuarioAtualizado.saldoDisponivel:F2}",
                                          "Sucesso",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao remover gasto: {ex.Message}", "Erro",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PlanFutureExpense_Click(object sender, RoutedEventArgs e)
        {
            if (FuturePlanningPanel.Visibility == Visibility.Visible)
            {
                FuturePlanningPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                FuturePlanningPanel.Visibility = Visibility.Visible;
            }
        }

        private void SaveFuturePlan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FutureValueTextBox.Text) || FutureValueTextBox.Text == "0,00")
            {
                MessageBox.Show("Por favor, insira um valor válido para o planejamento futuro.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(FutureDateTextBox.Text) || FutureDateTextBox.Text == "dd/MM/aaaa")
            {
                MessageBox.Show("Por favor, insira uma data válida para o planejamento futuro.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Gasto futuro planejado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpar campos do planejamento futuro
            FutureValueTextBox.Text = "0,00";
            FutureDateTextBox.Text = "dd/MM/aaaa";
            FuturePlanningPanel.Visibility = Visibility.Collapsed;
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Relatório de gastos exportado com sucesso!", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FilterExpenses_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Filtro aplicado aos gastos!", "Filtrar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tem certeza que deseja limpar todos os gastos?\nEsta ação não pode ser desfeita.",
                                        "Confirmar Limpeza",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Todos os gastos foram removidos!", "Limpeza Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ... (mantenha os outros métodos existentes: BackToDashboard_Click, PlanFutureExpense_Click, etc.)

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao voltar para dashboard: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Finanças: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Cartões
            MessageBox.Show("Navegar para Cartões", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            // Já está na tela de gastos
            MessageBox.Show("Você já está na tela de Gastos", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                {
                    IncomeWindow incomeWindow = new IncomeWindow();
                    incomeWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar para Receitas: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Metas
            MessageBox.Show("Navegar para Metas", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}