using Nommus;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace NommusProject
{
    public partial class IncomeWindow : Window
    {
        private ObservableCollection<Transacao> _receitasCollection;
        public IncomeWindow()
        {
            InitializeComponent();
            _receitasCollection = new ObservableCollection<Transacao>();
            ReceitasItemsControl.ItemsSource = _receitasCollection;

            CarregarDadosUsuario();
            CarregarReceitas();
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
        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ✅ SOLUÇÃO SIMPLES: Criar NOVA MainWindow para forçar recarregamento
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao voltar para dashboard: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void AddIncome_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Por favor, insira uma descrição para a receita.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ValueTextBox.Text) || ValueTextBox.Text == "0,00")
            {
                MessageBox.Show("Por favor, insira um valor válido para a receita.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Converter valor
                decimal valorDecimal = Convert.ToDecimal(ValueTextBox.Text.Replace("R$", "").Trim());
                double valor = (double)valorDecimal;

                // Obter categoria selecionada
                string categoria = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Outros";

                // Obter tipo de receita
                string tipoReceita = "Fixa";
                if (VariableRadio.IsChecked == true) tipoReceita = "Variável";
                if (ExtraRadio.IsChecked == true) tipoReceita = "Extra";

                // Converter data
                DateTime data = DateTime.Parse(DateTextBox.Text);

                // ✅ OPÇÃO A: DEIXAR APENAS o AdicionarTransacaoAsync cuidar do saldo
                double saldoAntes = SessaoUsuario.UsuarioLogado.saldoDisponivel;

                // Criar e salvar receita
                var receita = new Receita
                {
                    DescricaoTransacao = DescriptionTextBox.Text,
                    ValorTransacao = valor,
                    CategoriaId = categoria.ToLower().Replace(" ", "-"),
                    DataTransacao = data,
                    FonteReceita = categoria,
                    ReceitaRecorrente = tipoReceita == "Fixa",
                    UsuarioId = SessaoUsuario.UsuarioLogado.Id,
                    TipoTransacao = "Receita"
                };

                // ⚠️ APENAS ESTA LINHA atualiza o saldo
                await receita.AdicionarTransacaoAsync();

                // ✅ AGORA: Recarregar usuário ATUALIZADO do arquivo
                var usuarioAtualizado = await Usuario.BuscarUsuarioPorIdAsync(SessaoUsuario.UsuarioLogado.Id);
                SessaoUsuario.UsuarioLogado = usuarioAtualizado;

                MessageBox.Show($"Receita adicionada com sucesso!\n" +
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
                _receitasCollection.Insert(0, receita);
            }
            catch (FormatException)
            {
                MessageBox.Show("Por favor, insira um valor numérico válido.", "Atenção",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar receita: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CarregarReceitas()
        {
            try
            {
                if (SessaoUsuario.UsuarioLogado != null)
                {
                    var transacoes = await Transacao.CarregarTransacoesPorUsuarioAsync(SessaoUsuario.UsuarioLogado.Id);
                    var receitas = transacoes.Where(t => t.TipoTransacao == "Receita")
                                           .OrderByDescending(r => r.DataTransacao)
                                           .ToList();

                    // SOLUÇÃO DEFINITIVA: Usar ObservableCollection
                    if (_receitasCollection == null)
                    {
                        _receitasCollection = new ObservableCollection<Transacao>();
                        ReceitasItemsControl.ItemsSource = _receitasCollection;
                    }

                    _receitasCollection.Clear();
                    foreach (var receita in receitas)
                    {
                        _receitasCollection.Add(receita);
                    }

                    // Controle de mensagem vazia
                    var stackPanel = ReceitasItemsControl.Parent as StackPanel;
                    if (stackPanel != null)
                    {
                        var emptyTextBlock = stackPanel.Children
                            .OfType<TextBlock>()
                            .FirstOrDefault(tb => tb.Text.Contains("Nenhuma receita"));

                        if (!receitas.Any() && emptyTextBlock == null)
                        {
                            var emptyText = new TextBlock
                            {
                                Text = "Nenhuma receita registrada ainda...",
                                Foreground = new System.Windows.Media.SolidColorBrush(
                                    System.Windows.Media.Color.FromRgb(148, 163, 184)),
                                FontSize = 14,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 20, 0, 0)
                            };
                            stackPanel.Children.Add(emptyText);
                        }
                        else if (receitas.Any() && emptyTextBlock != null)
                        {
                            stackPanel.Children.Remove(emptyTextBlock);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar receitas: {ex.Message}", "Erro",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void RemoverReceita_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                try
                {
                    if (button.DataContext is Transacao transacao)
                    {
                        var resultado = MessageBox.Show("Tem certeza que deseja remover esta receita?",
                                                      "Confirmar Remoção",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

                        if (resultado == MessageBoxResult.Yes)
                        {
                            // ✅ OPÇÃO A: DEIXAR APENAS o ExcluirTransacaoAsync cuidar do saldo
                            double saldoAntes = SessaoUsuario.UsuarioLogado.saldoDisponivel;

                            // ⚠️ APENAS ESTA LINHA atualiza o saldo
                            await transacao.ExcluirTransacaoAsync();

                            // ✅ AGORA: Recarregar usuário ATUALIZADO do arquivo
                            var usuarioAtualizado = await Usuario.BuscarUsuarioPorIdAsync(SessaoUsuario.UsuarioLogado.Id);
                            SessaoUsuario.UsuarioLogado = usuarioAtualizado;

                            // Recarregar a lista
                            CarregarReceitas();

                            MessageBox.Show($"Receita removida com sucesso!\n" +
                                          $"Saldo antes: R$ {saldoAntes:F2}\n" +
                                          $"Saldo atual: R$ {usuarioAtualizado.saldoDisponivel:F2}",
                                          "Sucesso",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao remover receita: {ex.Message}", "Erro",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PlanFutureIncome_Click(object sender, RoutedEventArgs e)
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

            MessageBox.Show("Receita futura planejada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpar campos do planejamento futuro
            FutureValueTextBox.Text = "0,00";
            FutureDateTextBox.Text = "dd/MM/aaaa";
            FuturePlanningPanel.Visibility = Visibility.Collapsed;
        }

        // Navigation methods
        // Navigation methods
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

        private void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Já está na tela de receitas
            MessageBox.Show("Você já está na tela de Receitas", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar tela de Metas
            MessageBox.Show("Navegar para Metas", "Navegação", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}