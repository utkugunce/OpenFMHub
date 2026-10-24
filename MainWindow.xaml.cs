using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using OpenFMHub.Models;
using OpenFMHub.Services;

using Point = System.Windows.Point;
using Canvas = System.Windows.Controls.Canvas;
using Path = System.IO.Path;

namespace OpenFMHub
{
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

        private readonly DatabaseService _dbService;
        private List<PlayerRecord> _currentPlayers = new();
        private string _activeTab = "Overview";

        public MainWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                int darkMode = 1;
                DwmSetWindowAttribute(helper.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
                int cornerPref = 2; // DWMWCP_ROUND
                DwmSetWindowAttribute(helper.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));
            }
            catch { }

            LoadSeasons();
            LoadPlayers();
        }

        private void LoadSeasons()
        {
            var seasons = _dbService.GetSeasons();
            CmbSeason.ItemsSource = seasons;
            if (seasons.Count > 1)
            {
                CmbSeason.SelectedIndex = 1; // En son sezon (Örn: 2025/26)
            }
            else
            {
                CmbSeason.SelectedIndex = 0;
            }
        }

        private void LoadPlayers()
        {
            string selectedSeason = CmbSeason.SelectedItem?.ToString() ?? "Tüm Sezonlar";
            string querySeason = selectedSeason == "Tüm Sezonlar" ? "All" : selectedSeason;

            _currentPlayers = _dbService.GetPlayers(querySeason);
            ApplyFilters();

            if (GridPlayers.Items.Count > 0)
            {
                GridPlayers.SelectedIndex = 0;
            }
        }

        private void ApplyFilters()
        {
            var list = _currentPlayers.AsEnumerable();

            // Arama filtresi
            string search = TxtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Club.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Position.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Sekme filtresi
            switch (_activeTab)
            {
                case "Moneyball":
                    list = list.OrderByDescending(p => p.RoleScore);
                    break;
                case "Progression":
                    // Çoklu sezonu olan oyuncuları grupla ve gelişim gösterenleri öne al
                    list = list.OrderByDescending(p => p.AvgRating);
                    break;
                case "Contracts":
                    list = list.Where(p => p.ContractExpiryYear <= 2026 || (p.WageWeeklyEuro > 50_000 && p.AvgRating < 6.85));
                    break;
                default:
                    list = list.OrderByDescending(p => p.AvgRating);
                    break;
            }

            var result = list.ToList();
            GridPlayers.ItemsSource = result;
            TxtStatusInfo.Text = $"Sezon: {CmbSeason.SelectedItem} • {result.Count} oyuncu listeleniyor";
        }

        private void GridPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridPlayers.SelectedItem is PlayerRecord p)
            {
                DisplayPlayerCard(p);
            }
        }

        private void DisplayPlayerCard(PlayerRecord p)
        {
            CardPlayerName.Text = p.Name;
            CardPlayerMeta.Text = $"{p.Age} Yaş • {p.Club} • {p.Position} • {p.Nationality}";
            CardRoleScoreBadge.Text = $"{p.RoleScore:F1}";
            CardCa.Text = $"{p.CurrentAbility} / 200";
            CardPa.Text = $"{p.PotentialAbility} / 200";
            CardConsistency.Text = $"{p.Consistency} / 20";

            CardWageEval.Text = MoneyballEngine.EvaluateWageEfficiency(p);
            CardWageDetail.Text = $"Maaş: {p.FormattedWage} • Bitiş: {p.ContractExpiryYear} • Değer: {p.FormattedValue}";

            DrawRadar(p);
        }

        private static SolidColorBrush Brush(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        private void DrawRadar(PlayerRecord p)
        {
            RadarCanvas.Children.Clear();

            double w = RadarCanvas.ActualWidth > 0 ? RadarCanvas.ActualWidth : 290;
            double h = RadarCanvas.ActualHeight > 0 ? RadarCanvas.ActualHeight : 190;
            double cx = w / 2.0;
            double cy = h / 2.0;
            double radius = Math.Min(cx, cy) - 24;

            var metrics = MoneyballEngine.CalculateRadar(p);
            double[] values = { metrics.Scoring, metrics.Creation, metrics.Possession, metrics.Defending, metrics.Physical, metrics.Mental };
            string[] labels = { "ŞUT", "YARATICILIK", "PAS", "SAVUNMA", "FİZİK", "ZİHİN" };

            // 1. Konsantrik rehber ağları (%25, %50, %75, %100)
            for (double level = 0.25; level <= 1.0; level += 0.25)
            {
                var gridPoly = new Polygon
                {
                    Stroke = Brush("#21262D"),
                    StrokeThickness = 1
                };
                for (int i = 0; i < 6; i++)
                {
                    double angle = (i * 60 - 90) * Math.PI / 180.0;
                    double x = cx + (radius * level * Math.Cos(angle));
                    double y = cy + (radius * level * Math.Sin(angle));
                    gridPoly.Points.Add(new Point(x, y));
                }
                RadarCanvas.Children.Add(gridPoly);
            }

            // 2. Eksen çizgileri ve etiketler
            for (int i = 0; i < 6; i++)
            {
                double angle = (i * 60 - 90) * Math.PI / 180.0;
                double x = cx + (radius * Math.Cos(angle));
                double y = cy + (radius * Math.Sin(angle));

                var axisLine = new Line
                {
                    X1 = cx,
                    Y1 = cy,
                    X2 = x,
                    Y2 = y,
                    Stroke = Brush("#21262D"),
                    StrokeThickness = 1
                };
                RadarCanvas.Children.Add(axisLine);

                // Etiket
                double lx = cx + ((radius + 14) * Math.Cos(angle));
                double ly = cy + ((radius + 14) * Math.Sin(angle));
                var tb = new TextBlock
                {
                    Text = labels[i],
                    Foreground = Brush("#8B949E"),
                    FontSize = 8,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(tb, lx - 18);
                Canvas.SetTop(tb, ly - 6);
                RadarCanvas.Children.Add(tb);
            }

            // 3. Oyuncu poligonu
            var playerPoints = new PointCollection();
            for (int i = 0; i < 6; i++)
            {
                double pct = Math.Clamp(values[i] / 100.0, 0.1, 1.0);
                double angle = (i * 60 - 90) * Math.PI / 180.0;
                double x = cx + (radius * pct * Math.Cos(angle));
                double y = cy + (radius * pct * Math.Sin(angle));
                playerPoints.Add(new Point(x, y));
            }

            var playerPoly = new Polygon
            {
                Points = playerPoints,
                Stroke = Brush("#38BDF8"),
                StrokeThickness = 2,
                Fill = Brush("#3338BDF8")
            };
            RadarCanvas.Children.Add(playerPoly);

            // 4. Noktalar
            foreach (var pt in playerPoints)
            {
                var el = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brush("#38BDF8"),
                    Stroke = Brush("#0284C7"),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(el, pt.X - 2.5);
                Canvas.SetTop(el, pt.Y - 2.5);
                RadarCanvas.Children.Add(el);
            }
        }

        private void CmbSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                LoadPlayers();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        // ================= SEKME SEÇİMLERİ =================
        private void ResetTabButtons()
        {
            TabBtnOverview.Foreground = Brush("#8B949E");
            TabBtnMoneyball.Foreground = Brush("#8B949E");
            TabBtnProgression.Foreground = Brush("#8B949E");
            TabBtnContracts.Foreground = Brush("#8B949E");
        }

        private void TabOverview_Click(object sender, RoutedEventArgs e)
        {
            _activeTab = "Overview";
            ResetTabButtons();
            TabBtnOverview.Foreground = Brush("#58A6FF");
            ApplyFilters();
        }

        private void TabMoneyball_Click(object sender, RoutedEventArgs e)
        {
            _activeTab = "Moneyball";
            ResetTabButtons();
            TabBtnMoneyball.Foreground = Brush("#58A6FF");
            ApplyFilters();
        }

        private void TabProgression_Click(object sender, RoutedEventArgs e)
        {
            _activeTab = "Progression";
            ResetTabButtons();
            TabBtnProgression.Foreground = Brush("#58A6FF");
            ApplyFilters();
        }

        private void TabContracts_Click(object sender, RoutedEventArgs e)
        {
            _activeTab = "Contracts";
            ResetTabButtons();
            TabBtnContracts.Foreground = Brush("#58A6FF");
            ApplyFilters();
        }

        // ================= İÇE AKTARMA & TARAMA =================
        private void BtnImportHtml_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "FM Web / HTML Çıktıları (*.html;*.htm)|*.html;*.htm|Tüm Dosyalar (*.*)|*.*",
                Title = "Football Manager Kadro veya Gözlem Çıktısını Seçin (Ctrl+P)"
            };

            if (dlg.ShowDialog() == true)
            {
                string season = CmbSeason.SelectedItem?.ToString() ?? "2025/26";
                if (season == "Tüm Sezonlar") season = "2025/26";

                var imported = FmExportParser.ParseHtml(dlg.FileName, season);
                if (imported.Count > 0)
                {
                    foreach (var p in imported)
                    {
                        _dbService.SavePlayer(p);
                    }
                    MessageBox.Show($"{imported.Count} oyuncu '{season}' sezonuna başarıyla aktarıldı ve arşivlendi!", "İçe Aktarma Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSeasons();
                    LoadPlayers();
                }
                else
                {
                    MessageBox.Show("HTML dosyasında geçerli bir Football Manager oyuncu tablosu bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnAutoScan_Click(object sender, RoutedEventArgs e)
        {
            string fmDocs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Sports Interactive\Football Manager 26");
            if (!Directory.Exists(fmDocs))
            {
                fmDocs = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\OneDrive\Belgeler\Sports Interactive\Football Manager 26");
            }

            if (Directory.Exists(fmDocs))
            {
                var files = Directory.GetFiles(fmDocs, "*.html", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    MessageBox.Show($"FM 2026 klasörünüzde {files.Length} adet dışa aktarılmış tablo dosyası bulundu:\n\n{string.Join("\n", files.Take(3))}", "FM26 Otomatik Tarama", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"FM 2026 klasörünüz bulundu:\n{fmDocs}\n\nHenüz dışa aktarılmış bir HTML dosyası yok.\nOyunda herhangi bir ekranda (Kadro, Gözlemcilik vb.) Ctrl+P yaparak 'Web Sayfası' olarak kaydedebilirsiniz.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Football Manager 26 klasörü bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ================= BAŞLIK ÇUBUĞU =================
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
