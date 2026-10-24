using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using OpenFMHub.Models;

namespace OpenFMHub.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;

        public DatabaseService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = Path.Combine(appData, "OpenFMHub");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "openfm_hub.db");

            InitializeDatabase();
        }

        private SqliteConnection GetConnection()
        {
            var cs = new SqliteConnectionStringBuilder
            {
                DataSource = _dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            var conn = new SqliteConnection(cs);
            conn.Open();
            return conn;
        }

        private void InitializeDatabase()
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS players (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    season TEXT NOT NULL,
                    name TEXT NOT NULL,
                    age INTEGER,
                    club TEXT,
                    nationality TEXT,
                    position TEXT,
                    value_euro INTEGER,
                    wage_euro INTEGER,
                    contract_year INTEGER,
                    apps INTEGER,
                    minutes INTEGER,
                    goals INTEGER,
                    assists INTEGER,
                    xg REAL,
                    xa REAL,
                    avg_rating REAL,
                    pass_comp_pct REAL,
                    key_passes REAL,
                    tackles_won REAL,
                    interceptions REAL,
                    dribbles REAL,
                    shots REAL,
                    pace INTEGER,
                    acceleration INTEGER,
                    stamina INTEGER,
                    strength INTEGER,
                    finishing INTEGER,
                    passing INTEGER,
                    dribbling INTEGER,
                    tackling INTEGER,
                    vision INTEGER,
                    decisions INTEGER,
                    work_rate INTEGER,
                    composure INTEGER,
                    ca INTEGER,
                    pa INTEGER,
                    consistency INTEGER,
                    big_matches INTEGER,
                    injury_proneness INTEGER
                );
            ";
            cmd.ExecuteNonQuery();

            // Kayıt yoksa demo verileri ekle
            using var countCmd = conn.CreateCommand();
            countCmd.CommandText = "SELECT count(*) FROM players";
            long count = Convert.ToInt64(countCmd.ExecuteScalar());
            if (count == 0)
            {
                SeedDemoData(conn);
            }
        }

        private void SeedDemoData(SqliteConnection conn)
        {
            var seedList = new List<PlayerRecord>
            {
                // 2024/25 Sezonu
                new PlayerRecord { Season = "2024/25", Name = "Semih Kılıçsoy", Age = 19, Club = "Beşiktaş", Position = "ST", ValueEuro = 18_000_000, WageWeeklyEuro = 25_000, ContractExpiryYear = 2028, Apps = 32, Minutes = 2450, Goals = 14, Assists = 4, ExpectedGoals = 12.8, ExpectedAssists = 3.2, AvgRating = 7.18, Pace = 15, Acceleration = 15, Finishing = 14, Dribbling = 14, Decisions = 12, CurrentAbility = 132, PotentialAbility = 168 },
                new PlayerRecord { Season = "2024/25", Name = "Barış Alper Yılmaz", Age = 24, Club = "Galatasaray", Position = "AMR/ST", ValueEuro = 24_000_000, WageWeeklyEuro = 45_000, ContractExpiryYear = 2027, Apps = 38, Minutes = 2980, Goals = 12, Assists = 8, ExpectedGoals = 10.5, ExpectedAssists = 7.1, AvgRating = 7.24, Pace = 17, Acceleration = 17, Stamina = 18, WorkRate = 18, CurrentAbility = 142, PotentialAbility = 155 },
                new PlayerRecord { Season = "2024/25", Name = "Ferdi Kadıoğlu", Age = 24, Club = "Fenerbahçe", Position = "DL/DR", ValueEuro = 30_000_000, WageWeeklyEuro = 55_000, ContractExpiryYear = 2028, Apps = 36, Minutes = 3120, Goals = 3, Assists = 6, ExpectedGoals = 2.1, ExpectedAssists = 5.8, AvgRating = 7.35, Pace = 15, Acceleration = 15, Stamina = 17, Passing = 14, Tackling = 13, WorkRate = 17, CurrentAbility = 146, PotentialAbility = 160 },
                new PlayerRecord { Season = "2024/25", Name = "Arda Güler", Age = 19, Club = "Real Madrid", Position = "AMC/AMR", ValueEuro = 45_000_000, WageWeeklyEuro = 85_000, ContractExpiryYear = 2029, Apps = 24, Minutes = 1420, Goals = 7, Assists = 5, ExpectedGoals = 5.2, ExpectedAssists = 4.8, AvgRating = 7.28, Pace = 13, Acceleration = 14, Vision = 17, Passing = 16, Technique = 17, Finishing = 14, CurrentAbility = 144, PotentialAbility = 182 },
                new PlayerRecord { Season = "2024/25", Name = "Kenan Yıldız", Age = 19, Club = "Juventus", Position = "AML/ST", ValueEuro = 35_000_000, WageWeeklyEuro = 40_000, ContractExpiryYear = 2029, Apps = 28, Minutes = 1850, Goals = 8, Assists = 4, ExpectedGoals = 7.1, ExpectedAssists = 3.9, AvgRating = 7.15, Pace = 15, Acceleration = 15, Dribbling = 16, Finishing = 13, CurrentAbility = 138, PotentialAbility = 175 },

                // 2025/26 Sezonu (Gelişim Eğrisi Örneği)
                new PlayerRecord { Season = "2025/26", Name = "Semih Kılıçsoy", Age = 20, Club = "Beşiktaş", Position = "ST", ValueEuro = 32_000_000, WageWeeklyEuro = 45_000, ContractExpiryYear = 2028, Apps = 36, Minutes = 2890, Goals = 21, Assists = 7, ExpectedGoals = 18.9, ExpectedAssists = 5.4, AvgRating = 7.42, Pace = 16, Acceleration = 16, Finishing = 16, Dribbling = 15, Decisions = 14, CurrentAbility = 145, PotentialAbility = 168 },
                new PlayerRecord { Season = "2025/26", Name = "Arda Güler", Age = 20, Club = "Real Madrid", Position = "AMC/AMR", ValueEuro = 75_000_000, WageWeeklyEuro = 120_000, ContractExpiryYear = 2029, Apps = 35, Minutes = 2650, Goals = 14, Assists = 13, ExpectedGoals = 11.2, ExpectedAssists = 12.4, AvgRating = 7.55, Pace = 14, Acceleration = 15, Vision = 18, Passing = 17, Finishing = 15, CurrentAbility = 156, PotentialAbility = 182 },
                new PlayerRecord { Season = "2025/26", Name = "Can Uzun", Age = 20, Club = "Eintracht Frankfurt", Position = "AMC/ST", ValueEuro = 28_000_000, WageWeeklyEuro = 35_000, ContractExpiryYear = 2029, Apps = 31, Minutes = 2210, Goals = 11, Assists = 6, ExpectedGoals = 9.8, ExpectedAssists = 5.2, AvgRating = 7.19, Pace = 14, Acceleration = 14, Finishing = 14, Vision = 15, CurrentAbility = 139, PotentialAbility = 170 },
                new PlayerRecord { Season = "2025/26", Name = "Ahmetcan Kaplan", Age = 23, Club = "Ajax", Position = "DC", ValueEuro = 22_000_000, WageWeeklyEuro = 30_000, ContractExpiryYear = 2027, Apps = 29, Minutes = 2540, Goals = 2, Assists = 1, ExpectedGoals = 1.4, ExpectedAssists = 0.8, AvgRating = 7.26, Tackling = 16, Heading = 16, Decisions = 15, Passing = 14, CurrentAbility = 140, PotentialAbility = 162 },
                new PlayerRecord { Season = "2025/26", Name = "Eren Dinkçi", Age = 24, Club = "Freiburg", Position = "AMR/ST", ValueEuro = 16_000_000, WageWeeklyEuro = 28_000, ContractExpiryYear = 2028, Apps = 30, Minutes = 2100, Goals = 9, Assists = 5, ExpectedGoals = 8.1, ExpectedAssists = 4.3, AvgRating = 7.08, Pace = 18, Acceleration = 18, Finishing = 12, CurrentAbility = 135, PotentialAbility = 148 }
            };

            foreach (var p in seedList)
            {
                SavePlayer(p, conn);
            }
        }

        public void SavePlayer(PlayerRecord p, SqliteConnection? conn = null)
        {
            bool closeConn = false;
            if (conn == null)
            {
                conn = GetConnection();
                closeConn = true;
            }

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO players (
                        season, name, age, club, nationality, position, value_euro, wage_euro, contract_year,
                        apps, minutes, goals, assists, xg, xa, avg_rating, pass_comp_pct, key_passes,
                        tackles_won, interceptions, dribbles, shots, pace, acceleration, stamina, strength,
                        finishing, passing, dribbling, tackling, vision, decisions, work_rate, composure,
                        ca, pa, consistency, big_matches, injury_proneness
                    ) VALUES (
                        @season, @name, @age, @club, @nationality, @position, @value_euro, @wage_euro, @contract_year,
                        @apps, @minutes, @goals, @assists, @xg, @xa, @avg_rating, @pass_comp_pct, @key_passes,
                        @tackles_won, @interceptions, @dribbles, @shots, @pace, @acceleration, @stamina, @strength,
                        @finishing, @passing, @dribbling, @tackling, @vision, @decisions, @work_rate, @composure,
                        @ca, @pa, @consistency, @big_matches, @injury_proneness
                    );
                ";

                cmd.Parameters.AddWithValue("@season", p.Season);
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@age", p.Age);
                cmd.Parameters.AddWithValue("@club", p.Club);
                cmd.Parameters.AddWithValue("@nationality", p.Nationality);
                cmd.Parameters.AddWithValue("@position", p.Position);
                cmd.Parameters.AddWithValue("@value_euro", p.ValueEuro);
                cmd.Parameters.AddWithValue("@wage_euro", p.WageWeeklyEuro);
                cmd.Parameters.AddWithValue("@contract_year", p.ContractExpiryYear);
                cmd.Parameters.AddWithValue("@apps", p.Apps);
                cmd.Parameters.AddWithValue("@minutes", p.Minutes);
                cmd.Parameters.AddWithValue("@goals", p.Goals);
                cmd.Parameters.AddWithValue("@assists", p.Assists);
                cmd.Parameters.AddWithValue("@xg", p.ExpectedGoals);
                cmd.Parameters.AddWithValue("@xa", p.ExpectedAssists);
                cmd.Parameters.AddWithValue("@avg_rating", p.AvgRating);
                cmd.Parameters.AddWithValue("@pass_comp_pct", p.PassCompPct);
                cmd.Parameters.AddWithValue("@key_passes", p.KeyPassesPer90);
                cmd.Parameters.AddWithValue("@tackles_won", p.TacklesWonPer90);
                cmd.Parameters.AddWithValue("@interceptions", p.InterceptionsPer90);
                cmd.Parameters.AddWithValue("@dribbles", p.DribblesPer90);
                cmd.Parameters.AddWithValue("@shots", p.ShotsPer90);
                cmd.Parameters.AddWithValue("@pace", p.Pace);
                cmd.Parameters.AddWithValue("@acceleration", p.Acceleration);
                cmd.Parameters.AddWithValue("@stamina", p.Stamina);
                cmd.Parameters.AddWithValue("@strength", p.Strength);
                cmd.Parameters.AddWithValue("@finishing", p.Finishing);
                cmd.Parameters.AddWithValue("@passing", p.Passing);
                cmd.Parameters.AddWithValue("@dribbling", p.Dribbling);
                cmd.Parameters.AddWithValue("@tackling", p.Tackling);
                cmd.Parameters.AddWithValue("@vision", p.Vision);
                cmd.Parameters.AddWithValue("@decisions", p.Decisions);
                cmd.Parameters.AddWithValue("@work_rate", p.WorkRate);
                cmd.Parameters.AddWithValue("@composure", p.Composure);
                cmd.Parameters.AddWithValue("@ca", p.CurrentAbility);
                cmd.Parameters.AddWithValue("@pa", p.PotentialAbility);
                cmd.Parameters.AddWithValue("@consistency", p.Consistency);
                cmd.Parameters.AddWithValue("@big_matches", p.BigMatches);
                cmd.Parameters.AddWithValue("@injury_proneness", p.InjuryProneness);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (closeConn) conn.Dispose();
            }
        }

        public List<PlayerRecord> GetPlayers(string season = "All")
        {
            var list = new List<PlayerRecord>();
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();

            if (season == "All")
            {
                cmd.CommandText = "SELECT * FROM players ORDER BY season DESC, avg_rating DESC";
            }
            else
            {
                cmd.CommandText = "SELECT * FROM players WHERE season = @season ORDER BY avg_rating DESC";
                cmd.Parameters.AddWithValue("@season", season);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var p = new PlayerRecord
                {
                    Id = reader.GetInt32(0),
                    Season = reader.GetString(1),
                    Name = reader.GetString(2),
                    Age = reader.GetInt32(3),
                    Club = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Nationality = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Position = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    ValueEuro = reader.GetInt32(7),
                    WageWeeklyEuro = reader.GetInt32(8),
                    ContractExpiryYear = reader.GetInt32(9),
                    Apps = reader.GetInt32(10),
                    Minutes = reader.GetInt32(11),
                    Goals = reader.GetInt32(12),
                    Assists = reader.GetInt32(13),
                    ExpectedGoals = reader.GetDouble(14),
                    ExpectedAssists = reader.GetDouble(15),
                    AvgRating = reader.GetDouble(16),
                    PassCompPct = reader.GetDouble(17),
                    KeyPassesPer90 = reader.GetDouble(18),
                    TacklesWonPer90 = reader.GetDouble(19),
                    InterceptionsPer90 = reader.GetDouble(20),
                    DribblesPer90 = reader.GetDouble(21),
                    ShotsPer90 = reader.GetDouble(22),
                    Pace = reader.GetInt32(23),
                    Acceleration = reader.GetInt32(24),
                    Stamina = reader.GetInt32(25),
                    Strength = reader.GetInt32(26),
                    Finishing = reader.GetInt32(27),
                    Passing = reader.GetInt32(28),
                    Dribbling = reader.GetInt32(29),
                    Tackling = reader.GetInt32(30),
                    Vision = reader.GetInt32(31),
                    Decisions = reader.GetInt32(32),
                    WorkRate = reader.GetInt32(33),
                    Composure = reader.GetInt32(34),
                    CurrentAbility = reader.GetInt32(35),
                    PotentialAbility = reader.GetInt32(36),
                    Consistency = reader.GetInt32(37),
                    BigMatches = reader.GetInt32(38),
                    InjuryProneness = reader.GetInt32(39)
                };

                var (roleScore, roleName) = MoneyballEngine.CalculateBestRole(p);
                p.RoleScore = roleScore;
                p.BestRoleName = roleName;

                list.Add(p);
            }

            return list;
        }

        public List<string> GetSeasons()
        {
            var seasons = new List<string> { "Tüm Sezonlar" };
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT season FROM players ORDER BY season DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                seasons.Add(reader.GetString(0));
            }
            return seasons;
        }
    }
}
