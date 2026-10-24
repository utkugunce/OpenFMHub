using System;

namespace OpenFMHub.Models
{
    public class PlayerRecord
    {
        public int Id { get; set; }
        public string Season { get; set; } = "2025/26";
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Club { get; set; } = "";
        public string Nationality { get; set; } = "Türkiye";
        public string Position { get; set; } = "ST";
        public int ValueEuro { get; set; }
        public int WageWeeklyEuro { get; set; }
        public int ContractExpiryYear { get; set; } = 2027;

        // Maç Performansı
        public int Apps { get; set; }
        public int Minutes { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public double ExpectedGoals { get; set; } // xG
        public double ExpectedAssists { get; set; } // xA
        public double AvgRating { get; set; } = 6.85;

        // 90 Dk Başına Metrikler (Per 90)
        public double PassCompPct { get; set; } = 82.0; // Pas İsabeti %
        public double KeyPassesPer90 { get; set; } = 1.2; // Kilit Pas
        public double TacklesWonPer90 { get; set; } = 1.8; // Top Kapma
        public double InterceptionsPer90 { get; set; } = 1.1; // Pas Arası
        public double DribblesPer90 { get; set; } = 1.4; // Dripling
        public double ShotsPer90 { get; set; } = 2.1; // Şut

        // Temel Nitelikler (1-20)
        public int Pace { get; set; } = 13;
        public int Acceleration { get; set; } = 13;
        public int Stamina { get; set; } = 14;
        public int Strength { get; set; } = 12;
        public int Finishing { get; set; } = 12;
        public int Passing { get; set; } = 12;
        public int Dribbling { get; set; } = 13;
        public int Tackling { get; set; } = 10;
        public int Heading { get; set; } = 12;
        public int Technique { get; set; } = 13;
        public int Vision { get; set; } = 12;
        public int Decisions { get; set; } = 13;
        public int WorkRate { get; set; } = 14;
        public int Composure { get; set; } = 12;

        // Gizli Nitelikler
        public int CurrentAbility { get; set; } = 130; // CA (1-200)
        public int PotentialAbility { get; set; } = 155; // PA (1-200)
        public int Consistency { get; set; } = 14; // Süreklilik (1-20)
        public int BigMatches { get; set; } = 13; // Önemli Maçlar (1-20)
        public int InjuryProneness { get; set; } = 7; // Sakatlık Eğilimi (1-20, düşük iyi)

        // Hesaplanan Değerler
        public double RoleScore { get; set; } = 75.0; // 0-100 Moneyball Puanı
        public string BestRoleName { get; set; } = "Santrfor";
        public string FormattedValue => ValueEuro >= 1_000_000 ? $"€{ValueEuro / 1_000_000.0:F1}M" : $"€{ValueEuro / 1_000.0:F0}K";
        public string FormattedWage => $"€{WageWeeklyEuro:N0}/hf";
        public bool IsContractExpiringSoon => ContractExpiryYear <= 2026;
    }
}
