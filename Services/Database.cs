using Microsoft.Data.Sqlite;
using Pointage.Models;

namespace Pointage.Services;

public static class Database
{
    private static readonly string DossierDonnees =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Pointage");

    private static readonly string CheminDb =
        Path.Combine(DossierDonnees, "pointage.db");

    private static string ConnStr => $"Data Source={CheminDb}";

    private static bool _initialise = false;

    private static void AssurerDossier()
    {
        try
        {
            if (!Directory.Exists(DossierDonnees))
                Directory.CreateDirectory(DossierDonnees);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Impossible de creer le dossier de donnees :\n{DossierDonnees}\n\n{ex.Message}", ex);
        }
    }

    public static SqliteConnection GetConnection()
    {
        AssurerDossier();

        var conn = new SqliteConnection(ConnStr);
        conn.Open();

        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL;";
        pragma.ExecuteNonQuery();

        return conn;
    }

    public static void Initialiser()
    {
        if (_initialise) return;

        AssurerDossier();

        using var conn = GetConnection();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Utilisateur (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    NomUtilisateur TEXT NOT NULL UNIQUE,
                    MotDePasseHash TEXT NOT NULL,
                    Sel TEXT NOT NULL,
                    NomComplet TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'operateur',
                    DateCreation TEXT DEFAULT (datetime('now', 'localtime'))
                );";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Personnel (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nom TEXT NOT NULL,
                    Prenom TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS EnregistrementPointage (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PersonnelId INTEGER NOT NULL,
                    DatePointage TEXT NOT NULL,
                    HeureArrivee TEXT,
                    HeureSortie TEXT,
                    Observation TEXT,
                    FOREIGN KEY (PersonnelId) REFERENCES Personnel(Id) ON DELETE CASCADE,
                    UNIQUE (PersonnelId, DatePointage)
                );";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE INDEX IF NOT EXISTS idx_pointage_date 
                    ON EnregistrementPointage(DatePointage);
                CREATE INDEX IF NOT EXISTS idx_pointage_personnel 
                    ON EnregistrementPointage(PersonnelId);
                CREATE INDEX IF NOT EXISTS idx_pointage_personnel_date 
                    ON EnregistrementPointage(PersonnelId, DatePointage);";
            cmd.ExecuteNonQuery();
        }

        _initialise = true;
    }

    public static void TesterConnexion()
    {
        Initialiser();
    }

    public static string GetCheminBase() => CheminDb;

    public static bool AucunUtilisateur()
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand("SELECT COUNT(*) FROM Utilisateur", conn);
        return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
    }

    public static bool NomUtilisateurExiste(string nomUtilisateur)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "SELECT COUNT(*) FROM Utilisateur WHERE NomUtilisateur = @u", conn);
        cmd.Parameters.AddWithValue("@u", nomUtilisateur);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public static int CreerUtilisateur(string nomUtilisateur, string motDePasse,
        string nomComplet, string role = "operateur")
    {
        Initialiser();
        string sel = Securite.GenererSel();
        string hash = Securite.HacherMotDePasse(motDePasse, sel);

        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "INSERT INTO Utilisateur (NomUtilisateur, MotDePasseHash, Sel, NomComplet, Role) " +
            "VALUES (@u, @h, @s, @n, @r); SELECT last_insert_rowid();", conn);
        cmd.Parameters.AddWithValue("@u", nomUtilisateur);
        cmd.Parameters.AddWithValue("@h", hash);
        cmd.Parameters.AddWithValue("@s", sel);
        cmd.Parameters.AddWithValue("@n", nomComplet);
        cmd.Parameters.AddWithValue("@r", role);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static Utilisateur? Authentifier(string nomUtilisateur, string motDePasse)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "SELECT Id, NomUtilisateur, MotDePasseHash, Sel, NomComplet, Role, DateCreation " +
            "FROM Utilisateur WHERE NomUtilisateur = @u", conn);
        cmd.Parameters.AddWithValue("@u", nomUtilisateur);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        string hashStocke = reader.GetString(2);
        string sel = reader.GetString(3);

        if (!Securite.VerifierMotDePasse(motDePasse, sel, hashStocke))
            return null;

        return new Utilisateur
        {
            Id = reader.GetInt32(0),
            NomUtilisateur = reader.GetString(1),
            MotDePasseHash = hashStocke,
            Sel = sel,
            NomComplet = reader.GetString(4),
            Role = reader.GetString(5),
            DateCreation = DateTime.Parse(reader.GetString(6))
        };
    }

    public static List<Personnel> GetPersonnel()
    {
        Initialiser();
        var liste = new List<Personnel>();
        using var conn = GetConnection();

        using var cmd = new SqliteCommand(
            "SELECT Id, Nom, Prenom FROM Personnel ORDER BY Nom, Prenom", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new Personnel
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Prenom = reader.GetString(2)
            });
        }
        return liste;
    }

    public static int AjouterPersonnel(Personnel p)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "INSERT INTO Personnel (Nom, Prenom) VALUES (@n, @p); SELECT last_insert_rowid();", conn);
        cmd.Parameters.AddWithValue("@n", p.Nom);
        cmd.Parameters.AddWithValue("@p", p.Prenom);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void ModifierPersonnel(Personnel p)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "UPDATE Personnel SET Nom=@n, Prenom=@p WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@n", p.Nom);
        cmd.Parameters.AddWithValue("@p", p.Prenom);
        cmd.Parameters.AddWithValue("@id", p.Id);
        cmd.ExecuteNonQuery();
    }

    public static void SupprimerPersonnel(int id)
    {
        Initialiser();
        using var conn = GetConnection();
        using var trx = conn.BeginTransaction();
        try
        {
            using (var cmd1 = new SqliteCommand(
                "DELETE FROM EnregistrementPointage WHERE PersonnelId = @id", conn, trx))
            {
                cmd1.Parameters.AddWithValue("@id", id);
                cmd1.ExecuteNonQuery();
            }

            using (var cmd2 = new SqliteCommand(
                "DELETE FROM Personnel WHERE Id = @id", conn, trx))
            {
                cmd2.Parameters.AddWithValue("@id", id);
                cmd2.ExecuteNonQuery();
            }

            trx.Commit();
        }
        catch
        {
            trx.Rollback();
            throw;
        }
    }

    public static List<EnregistrementPointage> GetPointagesDuJour(DateTime date)
    {
        Initialiser();
        var liste = new List<EnregistrementPointage>();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "SELECT ep.Id, ep.PersonnelId, ep.DatePointage, ep.HeureArrivee, ep.HeureSortie, " +
            "       ep.Observation, (p.Nom || ' ' || p.Prenom) AS NomComplet " +
            "FROM EnregistrementPointage ep " +
            "INNER JOIN Personnel p ON p.Id = ep.PersonnelId " +
            "WHERE ep.DatePointage = @d " +
            "ORDER BY p.Nom, p.Prenom", conn);
        cmd.Parameters.AddWithValue("@d", date.ToString("yyyy-MM-dd"));
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new EnregistrementPointage
            {
                Id = reader.GetInt32(0),
                PersonnelId = reader.GetInt32(1),
                DatePointage = DateTime.Parse(reader.GetString(2)),
                HeureArrivee = reader.IsDBNull(3) ? null : TimeSpan.Parse(reader.GetString(3)),
                HeureSortie = reader.IsDBNull(4) ? null : TimeSpan.Parse(reader.GetString(4)),
                Observation = reader.IsDBNull(5) ? null : reader.GetString(5),
                NomComplet = reader.GetString(6)
            });
        }
        return liste;
    }

    public static List<EnregistrementPointage> GetTousPointages()
    {
        Initialiser();
        var liste = new List<EnregistrementPointage>();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "SELECT ep.Id, ep.PersonnelId, ep.DatePointage, ep.HeureArrivee, ep.HeureSortie, " +
            "       ep.Observation, (p.Nom || ' ' || p.Prenom) AS NomComplet " +
            "FROM EnregistrementPointage ep " +
            "INNER JOIN Personnel p ON p.Id = ep.PersonnelId " +
            "ORDER BY ep.DatePointage DESC, p.Nom, p.Prenom", conn);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new EnregistrementPointage
            {
                Id = reader.GetInt32(0),
                PersonnelId = reader.GetInt32(1),
                DatePointage = DateTime.Parse(reader.GetString(2)),
                HeureArrivee = reader.IsDBNull(3) ? null : TimeSpan.Parse(reader.GetString(3)),
                HeureSortie = reader.IsDBNull(4) ? null : TimeSpan.Parse(reader.GetString(4)),
                Observation = reader.IsDBNull(5) ? null : reader.GetString(5),
                NomComplet = reader.GetString(6)
            });
        }
        return liste;
    }

    public static void EnregistrerPointage(int personnelId, DateTime date,
        TimeSpan? arrivee, TimeSpan? sortie, string? observation)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "INSERT INTO EnregistrementPointage " +
            "  (PersonnelId, DatePointage, HeureArrivee, HeureSortie, Observation) " +
            "VALUES (@pid, @d, @a, @s, @o) " +
            "ON CONFLICT(PersonnelId, DatePointage) DO UPDATE SET " +
            "  HeureArrivee = excluded.HeureArrivee, " +
            "  HeureSortie = excluded.HeureSortie, " +
            "  Observation = excluded.Observation", conn);
        cmd.Parameters.AddWithValue("@pid", personnelId);
        cmd.Parameters.AddWithValue("@d", date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@a", arrivee.HasValue ? arrivee.Value.ToString(@"hh\:mm\:ss") : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@s", sortie.HasValue ? sortie.Value.ToString(@"hh\:mm\:ss") : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@o", (object?)observation ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public static void SupprimerPointage(int personnelId, DateTime date)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "DELETE FROM EnregistrementPointage WHERE PersonnelId = @pid AND DatePointage = @d", conn);
        cmd.Parameters.AddWithValue("@pid", personnelId);
        cmd.Parameters.AddWithValue("@d", date.ToString("yyyy-MM-dd"));
        cmd.ExecuteNonQuery();
    }

    public static Dictionary<int, int> GetStatistiquesMois(int annee, int mois)
    {
        Initialiser();
        var stats = new Dictionary<int, int>();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "SELECT CAST(strftime('%d', DatePointage) AS INTEGER) AS Jour, COUNT(*) AS Nb " +
            "FROM EnregistrementPointage " +
            "WHERE strftime('%Y', DatePointage) = @y AND strftime('%m', DatePointage) = @m " +
            "GROUP BY Jour", conn);
        cmd.Parameters.AddWithValue("@y", annee.ToString("D4"));
        cmd.Parameters.AddWithValue("@m", mois.ToString("D2"));
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            stats[reader.GetInt32(0)] = reader.GetInt32(1);
        }
        return stats;
    }

    public static HashSet<int> GetJoursPointesDuMois(int personnelId, int annee, int mois)
    {
        Initialiser();
        var jours = new HashSet<int>();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "SELECT DISTINCT CAST(strftime('%d', DatePointage) AS INTEGER) AS Jour " +
            "FROM EnregistrementPointage " +
            "WHERE PersonnelId = @pid " +
            "AND strftime('%Y', DatePointage) = @y " +
            "AND strftime('%m', DatePointage) = @m", conn);
        cmd.Parameters.AddWithValue("@pid", personnelId);
        cmd.Parameters.AddWithValue("@y", annee.ToString("D4"));
        cmd.Parameters.AddWithValue("@m", mois.ToString("D2"));
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            jours.Add(reader.GetInt32(0));
        }
        return jours;
    }

    public static List<BilanPersonnel> GetBilanPersonnel(DateTime debut, DateTime fin)
    {
        Initialiser();
        var liste = new List<BilanPersonnel>();
        using var conn = GetConnection();

        const string sql = @"
            SELECT p.Nom, p.Prenom,
                   COUNT(ep.Id) AS JoursPresents,
                   SUM(
                       (CAST(strftime('%H', ep.HeureSortie) AS INTEGER) * 3600 +
                        CAST(strftime('%M', ep.HeureSortie) AS INTEGER) * 60 +
                        CAST(strftime('%S', ep.HeureSortie) AS INTEGER))
                       -
                       (CAST(strftime('%H', ep.HeureArrivee) AS INTEGER) * 3600 +
                        CAST(strftime('%M', ep.HeureArrivee) AS INTEGER) * 60 +
                        CAST(strftime('%S', ep.HeureArrivee) AS INTEGER))
                   ) AS TotalSecondes,
                   AVG(
                       CAST(strftime('%H', ep.HeureArrivee) AS INTEGER) * 3600 +
                       CAST(strftime('%M', ep.HeureArrivee) AS INTEGER) * 60 +
                       CAST(strftime('%S', ep.HeureArrivee) AS INTEGER)
                   ) AS MoyenneArriveeSec
            FROM Personnel p
            LEFT JOIN EnregistrementPointage ep
                ON ep.PersonnelId = p.Id
               AND ep.DatePointage BETWEEN @d1 AND @d2
               AND ep.HeureArrivee IS NOT NULL
               AND ep.HeureSortie IS NOT NULL
            GROUP BY p.Id, p.Nom, p.Prenom
            ORDER BY p.Nom, p.Prenom";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@d1", debut.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@d2", fin.ToString("yyyy-MM-dd"));
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int jours = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

            string totalHeures = "-";
            if (!reader.IsDBNull(3))
            {
                long sec = (long)reader.GetDouble(3);
                var ts = TimeSpan.FromSeconds(sec);
                totalHeures = $"{(int)ts.TotalHours}h{ts.Minutes:D2}";
            }

            string moyArr = "-";
            if (!reader.IsDBNull(4))
            {
                double sec = reader.GetDouble(4);
                var ts = TimeSpan.FromSeconds(sec);
                moyArr = $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }

            liste.Add(new BilanPersonnel
            {
                Nom = $"{reader.GetString(0)} {reader.GetString(1)}".Trim(),
                JoursPresents = jours,
                TotalHeures = totalHeures,
                MoyenneArrivee = moyArr
            });
        }
        return liste;
    }

    public static List<BilanJour> GetBilanJours(DateTime debut, DateTime fin)
    {
        Initialiser();
        var liste = new List<BilanJour>();
        using var conn = GetConnection();

        int totalPersonnels;
        using (var cmdA = new SqliteCommand("SELECT COUNT(*) FROM Personnel", conn))
        {
            totalPersonnels = Convert.ToInt32(cmdA.ExecuteScalar());
        }

        const string sql = @"
            SELECT DatePointage, COUNT(*) AS Nb
            FROM EnregistrementPointage
            WHERE DatePointage BETWEEN @d1 AND @d2
            GROUP BY DatePointage
            ORDER BY DatePointage";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@d1", debut.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@d2", fin.ToString("yyyy-MM-dd"));
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int presents = reader.GetInt32(1);
            var date = DateTime.Parse(reader.GetString(0));
            liste.Add(new BilanJour
            {
                Date = date.ToString("dd/MM/yyyy"),
                Presents = presents,
                Absents = Math.Max(0, totalPersonnels - presents)
            });
        }
        return liste;
    }
}