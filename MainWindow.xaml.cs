using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;
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
        private Process? _engineProcess;
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };

        public MainWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
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

            CheckFmProcess();
            LoadSeasons();
            LoadPlayers();

            await InitializeLiveEngineAsync();
        }

        private void CheckFmProcess()
        {
            try
            {
                var fmProcs = Process.GetProcessesByName("fm");
                if (fmProcs.Length > 0)
                {
                    TxtFmDot.Foreground = Brush("#3FB950");
                    TxtFmStatus.Text = $"FM 2026 Aktif (PID: {fmProcs[0].Id})";
                    TxtFmStatus.Foreground = Brush("#58A6FF");
                }
                else
                {
                    TxtFmDot.Foreground = Brush("#8B949E");
                    TxtFmStatus.Text = "FM 2026 Bekleniyor";
                    TxtFmStatus.Foreground = Brush("#8B949E");
                }
            }
            catch { }
        }

        private async Task InitializeLiveEngineAsync()
        {
            try
            {
                // Motorun çalışıp çalışmadığını kontrol et
                bool isServerUp = false;
                try
                {
                    var resp = await _httpClient.GetAsync("http://127.0.0.1:8080/actuator/health");
                    isServerUp = resp.IsSuccessStatusCode;
                }
                catch { }

                if (!isServerUp)
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string engineExe = Path.Combine(baseDir, "fm-ai-assistent-windows-x64.exe");

                    if (!File.Exists(engineExe))
                    {
                        engineExe = @"C:\Users\utkug\.gemini\antigravity\scratch\OpenFMHub\fm-ai-assistent-windows-x64.exe";
                    }

                    if (File.Exists(engineExe))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = engineExe,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        _engineProcess = Process.Start(psi);
                    }

                    // Sunucunun ayağa kalkmasını bekle
                    for (int i = 0; i < 15; i++)
                    {
                        await Task.Delay(1000);
                        try
                        {
                            var resp = await _httpClient.GetAsync("http://127.0.0.1:8080/actuator/health");
                            if (resp.IsSuccessStatusCode)
                            {
                                isServerUp = true;
                                break;
                            }
                        }
                        catch { }
                    }
                }

                // WebView2 Başlat
                await MainWebView.EnsureCoreWebView2Async();
                MainWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                MainWebView.CoreWebView2.Navigate("http://127.0.0.1:8080");
            }
            catch (Exception ex)
            {
                LiveLoadingOverlay.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Canlı motor başlatılırken uyarı: {ex.Message}\nFM26 açık olduğunda doğrudan belleğe bağlanacaktır.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MainWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            LiveLoadingOverlay.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            try
            {
                if (_engineProcess != null && !_engineProcess.HasExited)
                {
                    _engineProcess.Kill();
                }
            }
            catch { }
        }

        // ================= NAVİGASYON =================
        private void BtnNavLive_Click(object sender, RoutedEventArgs e)
        {
            ViewArchive.Visibility = Visibility.Collapsed;
            ViewLive.Visibility = Visibility.Visible;
            BtnNavLive.Foreground = Brush("#58A6FF");
            BtnNavArchive.Foreground = Brush("#8B949E");
            TxtStatusInfo.Text = "Canlı FM 2026 Bellek Görünümü Aktif";
        }

        private void BtnNavArchive_Click(object sender, RoutedEventArgs e)
        {
            ViewLive.Visibility = Visibility.Collapsed;
            ViewArchive.Visibility = Visibility.Visible;
            BtnNavArchive.Foreground = Brush("#58A6FF");
            BtnNavLive.Foreground = Brush("#8B949E");
            TxtStatusInfo.Text = "Çoklu Sezon Arşivi & Moneyball Radarı Aktif";
        }

        // ================= SEZON & OYUNCU ARŞİVİ =================
        private void LoadSeasons()
        {
            var seasons = _dbService.GetSeasons();
            CmbSeason.ItemsSource = seasons;
            CmbSeason.SelectedIndex = seasons.Count > 1 ? 1 : 0;
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
            string search = TxtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Club.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Position.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            GridPlayers.ItemsSource = list.OrderByDescending(p => p.AvgRating).ToList();
        }

        private void CmbSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) LoadPlayers();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
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

            // Rehber ağları
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

            // Eksenler
            for (int i = 0; i < 6; i++)
            {
                double angle = (i * 60 - 90) * Math.PI / 180.0;
                double x = cx + (radius * Math.Cos(angle));
                double y = cy + (radius * Math.Sin(angle));

                RadarCanvas.Children.Add(new Line
                {
                    X1 = cx,
                    Y1 = cy,
                    X2 = x,
                    Y2 = y,
                    Stroke = Brush("#21262D"),
                    StrokeThickness = 1
                });

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

            // Poligon
            var playerPoints = new PointCollection();
            for (int i = 0; i < 6; i++)
            {
                double pct = Math.Clamp(values[i] / 100.0, 0.1, 1.0);
                double angle = (i * 60 - 90) * Math.PI / 180.0;
                double x = cx + (radius * pct * Math.Cos(angle));
                double y = cy + (radius * pct * Math.Sin(angle));
                playerPoints.Add(new Point(x, y));
            }

            RadarCanvas.Children.Add(new Polygon
            {
                Points = playerPoints,
                Stroke = Brush("#38BDF8"),
                StrokeThickness = 2,
                Fill = Brush("#3338BDF8")
            });

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

        // ================= PENCERE KONTROLLERİ =================
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMaximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
