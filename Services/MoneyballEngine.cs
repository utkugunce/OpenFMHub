using System;
using System.Collections.Generic;
using System.Linq;
using OpenFMHub.Models;

namespace OpenFMHub.Services
{
    public class RadarMetrics
    {
        public double Scoring { get; set; }     // Bitiricilik / xG
        public double Creation { get; set; }    // Yaratıcılık / Kilit Pas
        public double Possession { get; set; }  // Top Kontrolü / Pas
        public double Defending { get; set; }   // Savunma / Top Kapma
        public double Physical { get; set; }    // Fiziksel / Hız & Dayanıklılık
        public double Mental { get; set; }      // Zihinsel / Karar & Kararlılık
    }

    public static class MoneyballEngine
    {
        public static (double score, string roleName) CalculateBestRole(PlayerRecord p)
        {
            var roles = new Dictionary<string, double>();

            // 1. Pasör Stoper (BPD)
            double bpd = (p.Tackling * 0.20 + p.Passing * 0.15 + p.Vision * 0.10 + p.Decisions * 0.15 +
                          p.Strength * 0.15 + p.Pace * 0.10 + p.Composure * 0.15) * 5.0;
            roles["Pasör Stoper"] = Math.Clamp(bpd, 0, 100);

            // 2. Kanat Bek (WB)
            double wb = (p.Pace * 0.20 + p.Acceleration * 0.15 + p.Stamina * 0.20 + p.Tackling * 0.15 +
                         p.Passing * 0.15 + p.Dribbling * 0.15) * 5.0;
            roles["İki Yönlü Bek"] = Math.Clamp(wb, 0, 100);

            // 3. Geriden Oyun Kurucu (DLP)
            double dlp = (p.Passing * 0.25 + p.Vision * 0.25 + p.Decisions * 0.20 + p.Composure * 0.15 +
                          p.WorkRate * 0.15) * 5.0;
            roles["Regista / DLP"] = Math.Clamp(dlp, 0, 100);

            // 4. İki Yönlü Orta Saha (BBM)
            double bbm = (p.Stamina * 0.20 + p.WorkRate * 0.20 + p.Passing * 0.15 + p.Tackling * 0.15 +
                          p.Finishing * 0.10 + p.Decisions * 0.10 + p.Pace * 0.10) * 5.0;
            roles["Kutuya Kutu (BBM)"] = Math.Clamp(bbm, 0, 100);

            // 5. Kanat Forvet (IF)
            double ifw = (p.Pace * 0.20 + p.Acceleration * 0.20 + p.Dribbling * 0.20 + p.Finishing * 0.15 +
                          p.Decisions * 0.15 + p.Passing * 0.10) * 5.0;
            roles["Kanat Forvet"] = Math.Clamp(ifw, 0, 100);

            // 6. Gelişmiş Forvet (AF)
            double af = (p.Finishing * 0.25 + p.Pace * 0.20 + p.Acceleration * 0.15 + p.Composure * 0.15 +
                         p.Decisions * 0.15 + p.Strength * 0.10) * 5.0;
            roles["Gelişmiş Forvet"] = Math.Clamp(af, 0, 100);

            var best = roles.OrderByDescending(r => r.Value).First();
            return (Math.Round(best.Value, 1), best.Key);
        }

        public static RadarMetrics CalculateRadar(PlayerRecord p)
        {
            return new RadarMetrics
            {
                Scoring = Math.Clamp((p.Finishing * 0.6 + (p.ShotsPer90 * 2.5) + (p.Goals * 1.5)) / 20.0 * 100.0, 10, 99),
                Creation = Math.Clamp((p.Vision * 0.5 + (p.KeyPassesPer90 * 4.0) + p.Passing * 0.3) / 20.0 * 100.0, 10, 99),
                Possession = Math.Clamp((p.Passing * 0.4 + (p.PassCompPct * 0.15) + p.Dribbling * 0.3) / 20.0 * 100.0, 10, 99),
                Defending = Math.Clamp((p.Tackling * 0.5 + (p.TacklesWonPer90 * 3.5) + (p.InterceptionsPer90 * 3.0)) / 20.0 * 100.0, 10, 99),
                Physical = Math.Clamp((p.Pace * 0.35 + p.Acceleration * 0.35 + p.Stamina * 0.30) / 20.0 * 100.0, 10, 99),
                Mental = Math.Clamp((p.Decisions * 0.35 + p.Composure * 0.35 + p.WorkRate * 0.30) / 20.0 * 100.0, 10, 99)
            };
        }

        public static string EvaluateWageEfficiency(PlayerRecord p)
        {
            if (p.WageWeeklyEuro > 80_000 && p.AvgRating < 6.80)
            {
                return "⚠️ Yüksek Maaş / Düşük Verim (Maaş Yükü)";
            }
            if (p.WageWeeklyEuro < 20_000 && p.AvgRating >= 7.10)
            {
                return "💎 Kelepir Fırsat / Yüksek Verim (Moneyball)";
            }
            if (p.ContractExpiryYear <= 2026)
            {
                return "🚨 Sözleşme Bitiyor (Bedelsiz Ayrılma Riski)";
            }
            return "✅ Dengeli Kontrat";
        }
    }
}
