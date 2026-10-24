using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using OpenFMHub.Models;

namespace OpenFMHub.Services
{
    public static class FmExportParser
    {
        public static List<PlayerRecord> ParseHtml(string htmlFilePath, string season)
        {
            var players = new List<PlayerRecord>();
            if (!File.Exists(htmlFilePath)) return players;

            string content = File.ReadAllText(htmlFilePath);

            // Basit ve hızlı regex tabanlı HTML tablo ayrıştırıcı
            var rowMatches = Regex.Matches(content, @"<tr[^>]*>(.*?)</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (rowMatches.Count < 2) return players;

            // Başlık satırı
            var headerCols = new List<string>();
            var firstRowCells = Regex.Matches(rowMatches[0].Value, @"<(?:th|td)[^>]*>(.*?)</(?:th|td)>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            foreach (Match m in firstRowCells)
            {
                headerCols.Add(CleanHtml(m.Groups[1].Value).Trim().ToLowerInvariant());
            }

            // Veri satırları
            for (int i = 1; i < rowMatches.Count; i++)
            {
                var cells = Regex.Matches(rowMatches[i].Value, @"<td[^>]*>(.*?)</td>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (cells.Count == 0) continue;

                var p = new PlayerRecord { Season = season };
                for (int c = 0; c < Math.Min(cells.Count, headerCols.Count); c++)
                {
                    string h = headerCols[c];
                    string val = CleanHtml(cells[c].Groups[1].Value).Trim();

                    if (h.Contains("ad") || h.Contains("name") || h.Contains("oyuncu"))
                    {
                        p.Name = val;
                    }
                    else if (h.Contains("kulüp") || h.Contains("club") || h.Contains("takım"))
                    {
                        p.Club = val;
                    }
                    else if (h.Contains("poz") || h.Contains("position") || h.Contains("mevki"))
                    {
                        p.Position = val;
                    }
                    else if (h.Contains("yaş") || h.Contains("age"))
                    {
                        if (int.TryParse(val, out int age)) p.Age = age;
                    }
                    else if (h.Contains("gol") || h.Contains("gls"))
                    {
                        if (int.TryParse(val, out int g)) p.Goals = g;
                    }
                    else if (h.Contains("ast") || h.Contains("asist"))
                    {
                        if (int.TryParse(val, out int a)) p.Assists = a;
                    }
                    else if (h.Contains("ort") || h.Contains("rat"))
                    {
                        if (double.TryParse(val.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double r)) p.AvgRating = r;
                    }
                    else if (h.Contains("xg"))
                    {
                        if (double.TryParse(val.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double xg)) p.ExpectedGoals = xg;
                    }
                    else if (h.Contains("değer") || h.Contains("value"))
                    {
                        p.ValueEuro = ParseMoney(val);
                    }
                    else if (h.Contains("maaş") || h.Contains("wage"))
                    {
                        p.WageWeeklyEuro = ParseMoney(val);
                    }
                }

                if (!string.IsNullOrEmpty(p.Name))
                {
                    players.Add(p);
                }
            }

            return players;
        }

        private static string CleanHtml(string input)
        {
            return Regex.Replace(input, @"<[^>]*>", "").Replace("&nbsp;", " ").Trim();
        }

        private static int ParseMoney(string text)
        {
            text = text.Replace("€", "").Replace("£", "").Replace("$", "").Replace(".", "").Trim();
            if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(text[..^1], NumberStyles.Any, CultureInfo.InvariantCulture, out double m)) return (int)(m * 1_000_000);
            }
            if (text.EndsWith("k", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(text[..^1], NumberStyles.Any, CultureInfo.InvariantCulture, out double k)) return (int)(k * 1_000);
            }
            int.TryParse(text, out int val);
            return val;
        }
    }
}
