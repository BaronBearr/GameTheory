using GameTheory.AllClass;
using HandyControl.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace GameTheory.AllPages
{
    public partial class MainPage : Page
    {
        private double[,] payoffMatrix;
        private List<CriterionResult> criteriaResults = new List<CriterionResult>();

        private string[] strategies;
        private string[] states;
        public MainPage()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            HurwiczPanel.Visibility = Visibility.Collapsed;
            BayesLaplacePanel.Visibility = Visibility.Collapsed;

            if (RadioHurwicz.IsChecked == true)
            {
                HurwiczPanel.Visibility = Visibility.Visible;

                AlphaTextBox.Text = string.Empty;
            }

            if (RadioBayesLaplace.IsChecked == true)
            {
                BayesLaplacePanel.Visibility = Visibility.Visible;

                ProbabilitiesTextBox.Text = string.Empty;
            }
        }
        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    if (lines.Length == 0)
                    {
                        Growl.Error("Файл пуст. Пожалуйста, выберите файл с данными.");

                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(5);
                        timer.Tick += (s, args) =>
                        {
                            Growl.Clear();
                            timer.Stop();
                        };
                        timer.Start();
                        return;
                    }
                    if (!ValidateMatrixFormat(lines))
                    {
                        Growl.Error("Ошибка формата файла. Убедитесь, что все строки содержат одинаковое количество столбцов и значения корректны.");

                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(5);
                        timer.Tick += (s, args) =>
                        {
                            Growl.Clear();
                            timer.Stop();
                        };
                        timer.Start();
                        return;
                    }
                    criteriaResults.Clear();
                    TxtResult.Text = string.Empty;
                    TxtOptimalStrategy.Text = string.Empty;
                    ParseMatrixWithHeaders(lines);
                    DisplayMatrix();
                }
                catch (Exception ex)
                {
                    Growl.Error($"Произошла ошибка при обработке файла: {ex.Message}");

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(5);
                    timer.Tick += (s, args) =>
                    {
                        Growl.Clear();
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
        }
        private bool ValidateMatrixFormat(string[] lines)
        {
            var header = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int colCount = header.Length;
            string numberPattern = @"^\d+[,]?\d{0,2}$";

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Length != colCount + 1)
                {
                    return false;
                }

                for (int j = 1; j < values.Length; j++)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(values[j], numberPattern))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void ParseMatrixWithHeaders(string[] lines)
        {
            states = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int rowCount = lines.Length - 1;
            int colCount = states.Length;
            payoffMatrix = new double[rowCount, colCount]; 
            strategies = new string[rowCount];

            for (int i = 1; i <= rowCount; i++)
            {
                var values = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                strategies[i - 1] = values[0];
                for (int j = 1; j <= colCount; j++)
                {
                    payoffMatrix[i - 1, j - 1] = double.Parse(values[j].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }


        // Отображение данных в таблице
        private void DisplayMatrix()
        {
            PayoffGrid.Children.Clear();
            PayoffGrid.RowDefinitions.Clear();
            PayoffGrid.ColumnDefinitions.Clear();

            int rows = payoffMatrix.GetLength(0);
            int cols = payoffMatrix.GetLength(1);

            PayoffGrid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < rows; i++)
            {
                PayoffGrid.RowDefinitions.Add(new RowDefinition());
            }

            PayoffGrid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int j = 0; j < cols; j++)
            {
                PayoffGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int j = 0; j < cols; j++)
            {
                TextBlock tb = new TextBlock
                {
                    Text = states[j],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(tb, 0);
                Grid.SetColumn(tb, j + 1);
                PayoffGrid.Children.Add(tb);
            }

            for (int i = 0; i < rows; i++)
            {
                TextBlock strategyBlock = new TextBlock
                {
                    Text = strategies[i],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(strategyBlock, i + 1);
                Grid.SetColumn(strategyBlock, 0);
                PayoffGrid.Children.Add(strategyBlock);

                for (int j = 0; j < cols; j++)
                {
                    TextBlock tb = new TextBlock
                    {
                        Text = payoffMatrix[i, j].ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(tb, i + 1);
                    Grid.SetColumn(tb, j + 1);
                    PayoffGrid.Children.Add(tb);
                }
            }
        }
        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {
            if (payoffMatrix == null)
            {
                Growl.Error("Сначала загрузите матрицу.");

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += (s, args) =>
                {
                    Growl.Clear();
                    timer.Stop();
                };
                timer.Start();
                return;
            }

            if (RadioWald.IsChecked == true)
            {
                SolveWald();
            }
            else if (RadioMax.IsChecked == true)
            {
                SolveMax();
            }
            else if (RadioHurwicz.IsChecked == true)
            {
                string alphaInput = AlphaTextBox.Text;
                if (System.Text.RegularExpressions.Regex.IsMatch(alphaInput, @"^\d+,(\d)?$") &&  double.TryParse(AlphaTextBox.Text, out double alpha) && alpha >= 0 && alpha <= 1)
                {
                    SolveHurwicz(alpha);
                }
                else
                {
                    Growl.Info("Введите корректный коэффициент α (от 0 до 1).");

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(5);
                    timer.Tick += (s, args) =>
                    {
                        Growl.Clear();
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            else if (RadioSavage.IsChecked == true)
            {
                SolveSavage();
            }
            else if (RadioLaplace.IsChecked == true)
            {
                SolveLaplace();
            }
            else if (RadioBayesLaplace.IsChecked == true)
            {
               SolveBayesLaplace();
            }


            if (payoffMatrix == null || payoffMatrix.Length == 0)
            {
                Growl.Info("Данные для матрицы выплат отсутствуют.");

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += (s, args) =>
                {
                    Growl.Clear();
                    timer.Stop();
                };
                timer.Start();
                return;
            }

            if (criteriaResults == null || !criteriaResults.Any())
            {
                Growl.Info("Нет данных о результатах критериев.");

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += (s, args) =>
                {
                    Growl.Clear();
                    timer.Stop();
                };
                timer.Start();
                return;
            }

            DisplayPieChart(criteriaResults, strategies);
            DisplayOptimalStrategy(criteriaResults, strategies);
        }

        private void DisplayOptimalStrategy(List<CriterionResult> criteriaResults, string[] strategies)
        {
            var optimalStrategyCounts = criteriaResults
                .GroupBy(r => r.OptimalStrategyIndex)
                .Select(g => new { Strategy = strategies[g.Key - 1], Count = g.Count() })
                .ToList();

            var bestStrategy = optimalStrategyCounts.OrderByDescending(sc => sc.Count).FirstOrDefault();

            if (bestStrategy != null)
            {
                TxtOptimalStrategy.Text = $"Самая оптимальная стратегия: {bestStrategy.Strategy} (выбрана {bestStrategy.Count} раз(а))";
            }
            else
            {
                TxtOptimalStrategy.Text = "Не удалось определить оптимальную стратегию.";
            }
        }


        private void DisplayPieChart(List<CriterionResult> criteriaResults, string[] strategies)
        {

            var strategyCounts = criteriaResults
                .GroupBy(r => r.OptimalStrategyIndex)
                .Select(g => new { Strategy = strategies[g.Key - 1], Count = g.Count() })
                .ToList();

            SeriesCollection pieChartData = new SeriesCollection();
            Random random = new Random();
            foreach (var strategyCount in strategyCounts)
            {
                pieChartData.Add(new PieSeries
                {
                    Title = strategyCount.Strategy,  
                    Values = new ChartValues<int> { strategyCount.Count },  
                    DataLabels = true, 
                    LabelPoint = chartPoint => $"{strategyCount.Strategy} ({chartPoint.Y})",  
                    Fill = new SolidColorBrush(Color.FromArgb(255,
                    (byte)random.Next(50,256),
                    (byte)random.Next(50, 256),
                    (byte)random.Next(50, 256))),
                    FontSize = 14 
                });
            }

            OptimalStrategiesChart.Series = pieChartData;
        }
        private void SolveWald()
        {
            var minValues = Enumerable.Range(0, payoffMatrix.GetLength(0))
                                      .Select(i => Enumerable.Range(0, payoffMatrix.GetLength(1)).Min(j => payoffMatrix[i, j]))
                                      .ToArray();
            var optimalValue = minValues.Max();
            int optimalStrategyIndex = Array.IndexOf(minValues, optimalValue) + 1;

            TxtResult.Text = $"Оптимальная стратегия по критерию Вальда: A{optimalStrategyIndex}, значение: {optimalValue:F2}";

            if (!criteriaResults.Any(r => r.CriterionName == "Вальда"))
            {
                criteriaResults.Add(new CriterionResult("Вальда", optimalStrategyIndex));
            }
        }
        private void SolveMax()
        {
            var maxValues = Enumerable.Range(0, payoffMatrix.GetLength(0))
                                      .Select(i => Enumerable.Range(0, payoffMatrix.GetLength(1)).Max(j => payoffMatrix[i, j]))
                                      .ToArray();
            var optimalValue = maxValues.Max();
            int optimalStrategyIndex = Array.IndexOf(maxValues, optimalValue) + 1;

            TxtResult.Text = $"Оптимальная стратегия по критерию максимума: A{optimalStrategyIndex}, значение: {optimalValue:F2}";

            if (!criteriaResults.Any(r => r.CriterionName == "Максимума"))
            {
                criteriaResults.Add(new CriterionResult("Максимума", optimalStrategyIndex));
            }
        }
        private void SolveHurwicz(double alpha)
        {
            var hurwiczValues = Enumerable.Range(0, payoffMatrix.GetLength(0))
                                          .Select(i =>
                                          {
                                              var minValue = Enumerable.Range(0, payoffMatrix.GetLength(1)).Min(j => payoffMatrix[i, j]);
                                              var maxValue = Enumerable.Range(0, payoffMatrix.GetLength(1)).Max(j => payoffMatrix[i, j]);
                                              return alpha * minValue + (1 - alpha) * maxValue;
                                          })
                                          .ToArray();
            var optimalValue = hurwiczValues.Max();

            int optimalStrategyIndex = Array.IndexOf(hurwiczValues, optimalValue) + 1; 
            TxtResult.Text = $"Оптимальная стратегия по критерию Гурвица: A{optimalStrategyIndex}, значение: {optimalValue}";
            if (!criteriaResults.Any(r => r.CriterionName == "Гурвица"))
            {
                criteriaResults.Add(new CriterionResult("Гурвица", optimalStrategyIndex));
            }


        }
        private void SolveSavage()
        {
            int rows = payoffMatrix.GetLength(0);
            int cols = payoffMatrix.GetLength(1);

            double[] maxValuesPerState = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                maxValuesPerState[j] = Enumerable.Range(0, rows).Max(i => payoffMatrix[i, j]);
            }

            double[,] regretMatrix = new double[rows, cols]; 
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    regretMatrix[i, j] = maxValuesPerState[j] - payoffMatrix[i, j];
                }
            }

            double[] maxRegretPerStrategy = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                maxRegretPerStrategy[i] = Enumerable.Range(0, cols).Max(j => regretMatrix[i, j]);
            }

            double optimalRegret = maxRegretPerStrategy.Min(); 
            int optimalStrategyIndex = Array.IndexOf(maxRegretPerStrategy, optimalRegret) + 1;
            if (!criteriaResults.Any(r => r.CriterionName == "Сэвиджа"))
            {
                criteriaResults.Add(new CriterionResult("Сэвиджа", optimalStrategyIndex));
            }

            TxtResult.Text = $"Оптимальная стратегия по критерию Сэвиджа: A{optimalStrategyIndex}, с наименьшим максимальным риском: {optimalRegret:F2}";
        }
        private void SolveLaplace()
        {
            int rows = payoffMatrix.GetLength(0);
            int cols = payoffMatrix.GetLength(1);

            double[] laplaceValues = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                laplaceValues[i] = Enumerable.Range(0, cols).Average(j => payoffMatrix[i, j]);
            }

            double optimalLaplaceValue = laplaceValues.Max();
            int optimalStrategyIndex = Array.IndexOf(laplaceValues, optimalLaplaceValue) + 1;

            TxtResult.Text = $"Оптимальная стратегия по критерию Лапласа: A{optimalStrategyIndex}, среднее значение: {optimalLaplaceValue:F2}";

            if (!criteriaResults.Any(r => r.CriterionName == "Лапласа"))
            {
                criteriaResults.Add(new CriterionResult("Лапласа", optimalStrategyIndex));
            }
        }
        private void SolveBayesLaplace()
        {
            int cols = payoffMatrix.GetLength(1);

            string[] probabilityStrings = ProbabilitiesTextBox.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (probabilityStrings.Length != cols)
            {
                Growl.Info($"Количество введённых вероятностей ({probabilityStrings.Length}) не совпадает с количеством столбцов ({cols}) в матрице.");

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += (s, args) =>
                {
                    Growl.Clear();
                    timer.Stop();
                };
                timer.Start();
                return;
            }
            if (!TryParseProbabilities(probabilityStrings, out double[] probabilities))
            {
                Growl.Info("Проверьте, чтобы вероятности были разделены пробелами \nи введены через запятую.");

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += (s, args) =>
                {
                    Growl.Clear();
                    timer.Stop();
                };
                timer.Start();
                return;
            }

            int rows = payoffMatrix.GetLength(0);

            double[] bayesLaplaceValues = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                bayesLaplaceValues[i] = 0;
                for (int j = 0; j < cols; j++)
                {
                    bayesLaplaceValues[i] += payoffMatrix[i, j] * probabilities[j];
                }
            }

            double optimalValue = bayesLaplaceValues.Max();
            int optimalStrategyIndex = Array.IndexOf(bayesLaplaceValues, optimalValue) + 1;

            TxtResult.Text = $"Оптимальная стратегия по критерию Байеса-Лапласа: A{optimalStrategyIndex}, математическое ожидание: {optimalValue:F2}";

            if (!criteriaResults.Any(r => r.CriterionName == "Байеса-Лапласа"))
            {
                criteriaResults.Add(new CriterionResult("Байеса-Лапласа", optimalStrategyIndex));
            }
        }
        private bool TryParseProbabilities(string[] probabilityStrings, out double[] probabilities)
        {
            probabilities = new double[probabilityStrings.Length];
            double sum = 0;

            for (int i = 0; i < probabilityStrings.Length; i++)
            {
                if (!double.TryParse(probabilityStrings[i], out double probability) || probability < 0 || probability > 1)
                {
                    return false;
                }

                probabilities[i] = probability;
                sum += probability;
            }

            return Math.Abs(sum - 1.0) < 0.0001;
        }
        private void Clear_Click(object sender, EventArgs e)
        {
            Growl.Ask("Желаете очистить всю информацию по данной задаче?", isConfirmed =>
            {
                if (isConfirmed)
                {
                    OptimalStrategiesChart.Series = null;
                    criteriaResults.Clear();
                    TxtResult.Text = string.Empty;
                    TxtOptimalStrategy.Text = string.Empty;
                }

                Growl.Info(isConfirmed.ToString());

                return true;
            });

        }

    }
}
