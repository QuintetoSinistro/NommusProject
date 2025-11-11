using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Nommus
{
    public partial class MainWindow : Window
    {
        private bool _isMouseOverPopup = false;
        private Button _lastClickedButton;

        public MainWindow()
        {
            InitializeComponent();
            DrawDynamicChart();
        }

        private void DrawDynamicChart()
        {
            ChartCanvas.Children.Clear();
            DrawGridLines();

            // Dados de exemplo que ocupam toda a largura
            double[] financialData = { 50, 80, -30, 120, -60, 100, -20, 150, -40, 120, 50, -30, 90, 110, -50, 130, -20, 100 };

            DrawColoredChart(financialData);
        }

        private void DrawGridLines()
        {
            // Grid lines que vão até o final
            for (int i = 0; i <= 250; i += 50)
            {
                Line gridLine = new Line
                {
                    X1 = 0,
                    Y1 = i,
                    X2 = 800, // Largura fixa generosa
                    Y2 = i,
                    Stroke = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                    StrokeThickness = 1
                };
                ChartCanvas.Children.Add(gridLine);
            }
        }

        private void DrawColoredChart(double[] data)
        {
            // Calcular escala para ocupar toda a largura disponível
            double availableWidth = 780; // Largura menos margens
            double xScale = availableWidth / (data.Length - 1);
            double yBase = 125; // Base line (zero point)
            double yScale = 0.8; // Escala vertical

            Point previousPoint = new Point(10, yBase);

            for (int i = 0; i < data.Length; i++)
            {
                double x = 10 + (i * xScale);
                double y = yBase - (data[i] * yScale);

                // Garantir que os pontos não ultrapassem os limites
                if (x > 790) x = 790;
                if (y < 10) y = 10;
                if (y > 240) y = 240;

                Point currentPoint = new Point(x, y);

                Brush lineColor = data[i] >= 0 ?
                    new SolidColorBrush(Color.FromRgb(16, 185, 129)) :
                    new SolidColorBrush(Color.FromRgb(239, 68, 68));

                // Desenhar segmento da linha
                if (i > 0) // Não desenhar linha do primeiro ponto
                {
                    Line segment = new Line
                    {
                        X1 = previousPoint.X,
                        Y1 = previousPoint.Y,
                        X2 = currentPoint.X,
                        Y2 = currentPoint.Y,
                        Stroke = lineColor,
                        StrokeThickness = 3
                    };
                    ChartCanvas.Children.Add(segment);
                }

                // Desenhar ponto
                Ellipse point = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = lineColor
                };

                Canvas.SetLeft(point, currentPoint.X - 3);
                Canvas.SetTop(point, currentPoint.Y - 3);
                ChartCanvas.Children.Add(point);

                previousPoint = currentPoint;
            }
        }

        // Main button click handlers
        private void ExpensesMainButton_Click(object sender, RoutedEventArgs e)
        {
            _lastClickedButton = (Button)sender;
            ShowPopup("Expenses");
        }

        private void IncomeMainButton_Click(object sender, RoutedEventArgs e)
        {
            _lastClickedButton = (Button)sender;
            ShowPopup("Income");
        }

        private void ShowPopup(string popupType)
        {
            // Esconder todos os popups primeiro
            ExpensesPopup.Visibility = Visibility.Collapsed;
            IncomePopup.Visibility = Visibility.Collapsed;

            // Posicionar o popup perto do botão clicado
            if (_lastClickedButton != null)
            {
                var buttonPosition = _lastClickedButton.PointToScreen(new Point(0, 0));
                var gridPosition = ChartGrid.PointToScreen(new Point(0, 0));

                double relativeX = buttonPosition.X - gridPosition.X;
                double relativeY = buttonPosition.Y - gridPosition.Y + _lastClickedButton.ActualHeight + 5;

                // Ajustar para não sair da tela
                if (relativeX + 200 > ChartGrid.ActualWidth)
                {
                    relativeX = ChartGrid.ActualWidth - 220;
                }

                if (popupType == "Expenses")
                {
                    ExpensesPopup.Margin = new Thickness(relativeX, relativeY, 0, 0);
                    ExpensesPopup.Visibility = Visibility.Visible;
                }
                else if (popupType == "Income")
                {
                    IncomePopup.Margin = new Thickness(relativeX, relativeY, 0, 0);
                    IncomePopup.Visibility = Visibility.Visible;
                }
            }

            _isMouseOverPopup = true;
        }

        private void HideAllPopups()
        {
            ExpensesPopup.Visibility = Visibility.Collapsed;
            IncomePopup.Visibility = Visibility.Collapsed;
            _isMouseOverPopup = false;
        }

        // Eventos de mouse para os pop-ups
        private void Popup_MouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOverPopup = true;
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOverPopup = false;
            // Esconder pop-up após um pequeno delay
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                if (!_isMouseOverPopup)
                {
                    HideAllPopups();
                }
            };
            timer.Start();
        }

        // Evento de movimento do mouse na janela principal
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            // Se não está sobre nenhum pop-up, verificar se deve escondê-los
            if (!_isMouseOverPopup && (ExpensesPopup.Visibility == Visibility.Visible || IncomePopup.Visibility == Visibility.Visible))
            {
                var mousePos = e.GetPosition(ChartGrid);

                // Verificar se o mouse está longe dos pop-ups
                bool isMouseNearExpensesPopup = ExpensesPopup.Visibility == Visibility.Visible &&
                    mousePos.X >= ExpensesPopup.Margin.Left - 50 &&
                    mousePos.X <= ExpensesPopup.Margin.Left + ExpensesPopup.Width + 50 &&
                    mousePos.Y >= ExpensesPopup.Margin.Top - 50 &&
                    mousePos.Y <= ExpensesPopup.Margin.Top + ExpensesPopup.Height + 50;

                bool isMouseNearIncomePopup = IncomePopup.Visibility == Visibility.Visible &&
                    mousePos.X >= IncomePopup.Margin.Left - 50 &&
                    mousePos.X <= IncomePopup.Margin.Left + IncomePopup.Width + 50 &&
                    mousePos.Y >= IncomePopup.Margin.Top - 50 &&
                    mousePos.Y <= IncomePopup.Margin.Top + IncomePopup.Height + 50;

                if (!isMouseNearExpensesPopup && !isMouseNearIncomePopup)
                {
                    HideAllPopups();
                }
            }
        }

        // Sub menu button click handlers
        private void AddExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
            MessageBox.Show("Abrir tela para inserir novo gasto");
        }

        private void ViewExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
            MessageBox.Show("Abrir tela para visualizar gastos");
        }

        private void AddIncomeButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
            MessageBox.Show("Abrir tela para inserir nova receita");
        }

        private void ViewIncomeButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
            MessageBox.Show("Abrir tela para visualizar receitas");
        }

        // Navigation button click handlers
        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
        }

        private void ExpensesButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPopups();
        }

        // Quando o canvas é carregado, redesenhar o gráfico com as dimensões corretas
        private void ChartCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DrawDynamicChart();
        }
    }
}