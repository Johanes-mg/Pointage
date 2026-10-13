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

    public static string GetCheminBase() => CheminDb;
    public static string GetDossierDonnees() => DossierDonnees;

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
                    Prenom TEXT NOT NULL,
                    Matricule TEXT,
                    Telephone TEXT,
                    Adresse TEXT,
                    Email TEXT,
                    Statut TEXT,
                    Grade TEXT,
                    SituationFamiliale TEXT
                );";
            cmd.ExecuteNonQuery();
        }

        MigrerPersonnel(conn);

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS EnregistrementPointage (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PersonnelId INTEGER NOT NULL,
                    DatePointage TEXT NOT NULL,
                    MatinArrivee TEXT,
                    MatinSortie TEXT,
                    ApresMidiArrivee TEXT,
                    ApresMidiSortie TEXT,
                    Observation TEXT,
                    FOREIGN KEY (PersonnelId) REFERENCES Personnel(Id) ON DELETE CASCADE,
                    UNIQUE (PersonnelId, DatePointage)
                );";
            cmd.ExecuteNonQuery();
        }

        MigrerPointage(conn);

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Parametre (
                    Cle TEXT PRIMARY KEY,
                    Valeur TEXT
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

        InitialiserParametresParDefaut(conn);

        _initialise = true;
    }

    private static void InitialiserParametresParDefaut(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR IGNORE INTO Parametre (Cle, Valeur) VALUES 
                ('NomEntreprise', 'Pointage');";
        cmd.ExecuteNonQuery();
    }

    private static void MigrerPersonnel(SqliteConnection conn)
    {
        var colonnesExistantes = new HashSet<string>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info(Personnel);";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                colonnesExistantes.Add(reader.GetString(1));
            }
        }

        var colonnesAajouter = new List<(string nom, string type)>
        {
            ("Matricule", "TEXT"),
            ("Telephone", "TEXT"),
            ("Adresse", "TEXT"),
            ("Email", "TEXT"),
            ("Statut", "TEXT"),
            ("Grade", "TEXT"),
            ("SituationFamiliale", "TEXT")
        };

        foreach (var (nom, type) in colonnesAajouter)
        {
            if (!colonnesExistantes.Contains(nom))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"ALTER TABLE Personnel ADD COLUMN {nom} {type};";
                cmd.ExecuteNonQuery();
            }
        }
    }

    private static void MigrerPointage(SqliteConnection conn)
    {
        var colonnesExistantes = new HashSet<string>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info(EnregistrementPointage);";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                colonnesExistantes.Add(reader.GetString(1));
            }
        }

        var colonnesAajouter = new List<(string nom, string type)>
        {
            ("MatinArrivee", "TEXT"),
            ("MatinSortie", "TEXT"),
            ("ApresMidiArrivee", "TEXT"),
            ("ApresMidiSortie", "TEXT")
        };

        foreach (var (nom, type) in colonnesAajouter)
        {
            if (!colonnesExistantes.Contains(nom))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"ALTER TABLE EnregistrementPointage ADD COLUMN {nom} {type};";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static void TesterConnexion() => Initialiser();

    // ============ PARAMETRES ============

    public static string GetParametre(string cle, string valeurDefaut = "")
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "SELECT Valeur FROM Parametre WHERE Cle = @c", conn);
        cmd.Parameters.AddWithValue("@c", cle);

        var result = cmd.ExecuteScalar();
        return result?.ToString() ?? valeurDefaut;
    }

    public static void SetParametre(string cle, string valeur)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "INSERT INTO Parametre (Cle, Valeur) VALUES (@c, @v) " +
            "ON CONFLICT(Cle) DO UPDATE SET Valeur = @v", conn);
        cmd.Parameters.AddWithValue("@c", cle);
        cmd.Parameters.AddWithValue("@v", valeur);
        cmd.ExecuteNonQuery();
    }

    public static string GetNomEntreprise() => GetParametre("NomEntreprise", "Pointage");

    public static void SetNomEntreprise(string nom) => SetParametre("NomEntreprise", nom);

    // ============ UTILISATEURS ============

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

    public static bool ChangerMotDePasse(int utilisateurId, string ancienMotDePasse, string nouveauMotDePasse)
    {
        Initialiser();
        using var conn = GetConnection();

        string hashStocke, sel;
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT MotDePasseHash, Sel FROM Utilisateur WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", utilisateurId);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return false;
            hashStocke = reader.GetString(0);
            sel = reader.GetString(1);
        }

        if (!Securite.VerifierMotDePasse(ancienMotDePasse, sel, hashStocke))
            return false;

        string nouveauSel = Securite.GenererSel();
        string nouveauHash = Securite.HacherMotDePasse(nouveauMotDePasse, nouveauSel);

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "UPDATE Utilisateur SET MotDePasseHash = @h, Sel = @s WHERE Id = @id";
            cmd.Parameters.AddWithValue("@h", nouveauHash);
            cmd.Parameters.AddWithValue("@s", nouveauSel);
            cmd.Parameters.AddWithValue("@id", utilisateurId);
            cmd.ExecuteNonQuery();
        }

        return true;
    }

    public static void MettreAJourNomComplet(int utilisateurId, string nomComplet)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "UPDATE Utilisateur SET NomComplet = @n WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@n", nomComplet);
        cmd.Parameters.AddWithValue("@id", utilisateurId);
        cmd.ExecuteNonQuery();
    }

    // ============ IMPORT / EXPORT BASE ============

    public static void ExporterBase(string cheminDestination)
    {
        Initialiser();

        var dossier = Path.GetDirectoryName(cheminDestination);
        if (!string.IsNullOrEmpty(dossier) && !Directory.Exists(dossier))
            Directory.CreateDirectory(dossier);

        using var conn = GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"VACUUM INTO '{cheminDestination.Replace("'", "''")}';";
        cmd.ExecuteNonQuery();
    }

    public static void ImporterBase(string cheminSource)
    {
        if (!File.Exists(cheminSource))
            throw new FileNotFoundException("Fichier introuvable.", cheminSource);

        _initialise = false;

        AssurerDossier();

        if (File.Exists(CheminDb))
            File.Delete(CheminDb);

        var walPath = CheminDb + "-wal";
        var shmPath = CheminDb + "-shm";
        if (File.Exists(walPath)) File.Delete(walPath);
        if (File.Exists(shmPath)) File.Delete(shmPath);

        File.Copy(cheminSource, CheminDb, overwrite: true);

        Initialiser();
    }

    // ============ PERSONNEL ============

    public static List<Personnel> GetPersonnel()
    {
        Initialiser();
        var liste = new List<Personnel>();
        using var conn = GetConnection();

        using var cmd = new SqliteCommand(
            "SELECT Id, Nom, Prenom, Matricule, Telephone, Adresse, Email, Statut, Grade, SituationFamiliale " +
            "FROM Personnel ORDER BY Nom, Prenom", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new Personnel
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Prenom = reader.GetString(2),
                Matricule = reader.IsDBNull(3) ? null : reader.GetString(3),
                Telephone = reader.IsDBNull(4) ? null : reader.GetString(4),
                Adresse = reader.IsDBNull(5) ? null : reader.GetString(5),
                Email = reader.IsDBNull(6) ? null : reader.GetString(6),
                Statut = reader.IsDBNull(7) ? null : reader.GetString(7),
                Grade = reader.IsDBNull(8) ? null : reader.GetString(8),
                SituationFamiliale = reader.IsDBNull(9) ? null : reader.GetString(9)
            });
        }
        return liste;
    }

    public static int AjouterPersonnel(Personnel p)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "INSERT INTO Personnel " +
            "  (Nom, Prenom, Matricule, Telephone, Adresse, Email, Statut, Grade, SituationFamiliale) " +
            "VALUES (@n, @p, @m, @tel, @a, @e, @s, @g, @sf); SELECT last_insert_rowid();", conn);
        cmd.Parameters.AddWithValue("@n", p.Nom);
        cmd.Parameters.AddWithValue("@p", p.Prenom);
        cmd.Parameters.AddWithValue("@m", (object?)p.Matricule ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tel", (object?)p.Telephone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@a", (object?)p.Adresse ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@e", (object?)p.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@s", (object?)p.Statut ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@g", (object?)p.Grade ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@sf", (object?)p.SituationFamiliale ?? DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void ModifierPersonnel(Personnel p)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "UPDATE Personnel SET " +
            "  Nom=@n, Prenom=@p, Matricule=@m, Telephone=@tel, Adresse=@a, Email=@e, " +
            "  Statut=@s, Grade=@g, SituationFamiliale=@sf " +
            "WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@n", p.Nom);
        cmd.Parameters.AddWithValue("@p", p.Prenom);
        cmd.Parameters.AddWithValue("@m", (object?)p.Matricule ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@tel", (object?)p.Telephone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@a", (object?)p.Adresse ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@e", (object?)p.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@s", (object?)p.Statut ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@g", (object?)p.Grade ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@sf", (object?)p.SituationFamiliale ?? DBNull.Value);
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

    // ============ POINTAGE ============

    private static EnregistrementPointage LirePointage(SqliteDataReader reader)
    {
        return new EnregistrementPointage
        {
            Id = reader.GetInt32(0),
            PersonnelId = reader.GetInt32(1),
            DatePointage = DateTime.Parse(reader.GetString(2)),
            MatinArrivee = reader.IsDBNull(3) ? null : TimeSpan.Parse(reader.GetString(3)),
            MatinSortie = reader.IsDBNull(4) ? null : TimeSpan.Parse(reader.GetString(4)),
            ApresMidiArrivee = reader.IsDBNull(5) ? null : TimeSpan.Parse(reader.GetString(5)),
            ApresMidiSortie = reader.IsDBNull(6) ? null : TimeSpan.Parse(reader.GetString(6)),
            Observation = reader.IsDBNull(7) ? null : reader.GetString(7),
            NomComplet = reader.GetString(8)
        };
    }

    private const string SelectPointage =
        "SELECT ep.Id, ep.PersonnelId, ep.DatePointage, " +
        "       ep.MatinArrivee, ep.MatinSortie, " +
        "       ep.ApresMidiArrivee, ep.ApresMidiSortie, " +
        "       ep.Observation, (p.Nom || ' ' || p.Prenom) AS NomComplet " +
        "FROM EnregistrementPointage ep " +
        "INNER JOIN Personnel p ON p.Id = ep.PersonnelId ";

    public static List<EnregistrementPointage> GetPointagesDuJour(DateTime date)
    {
        Initialiser();
        var liste = new List<EnregistrementPointage>();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            SelectPointage + "WHERE ep.DatePointage = @d ORDER BY p.Nom, p.Prenom", conn);
        cmd.Parameters.AddWithValue("@d", date.ToString("yyyy-MM-dd"));
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(LirePointage(reader));
        }
        return liste;
    }

    public static List<EnregistrementPointage> GetPointagesPeriode(DateTime debut, DateTime fin)
    {
        Initialiser();
        var liste = new List<EnregistrementPointage>();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            SelectPointage + "WHERE ep.DatePointage BETWEEN @d1 AND @d2 " +
            "ORDER BY ep.DatePointage DESC, p.Nom, p.Prenom", conn);
        cmd.Parameters.AddWithValue("@d1", debut.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@d2", fin.ToString("yyyy-MM-dd"));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(LirePointage(reader));
        }
        return liste;
    }

    public static void EnregistrerPointage(int personnelId, DateTime date,
        TimeSpan? matinArrivee, TimeSpan? matinSortie,
        TimeSpan? apresMidiArrivee, TimeSpan? apresMidiSortie,
        string? observation)
    {
        Initialiser();
        using var conn = GetConnection();
        using var cmd = new SqliteCommand(
            "INSERT INTO EnregistrementPointage " +
            "  (PersonnelId, DatePointage, MatinArrivee, MatinSortie, ApresMidiArrivee, ApresMidiSortie, Observation) " +
            "VALUES (@pid, @d, @ma, @ms, @aa, @as, @o) " +
            "ON CONFLICT(PersonnelId, DatePointage) DO UPDATE SET " +
            "  MatinArrivee = excluded.MatinArrivee, " +
            "  MatinSortie = excluded.MatinSortie, " +
            "  ApresMidiArrivee = excluded.ApresMidiArrivee, " +
            "  ApresMidiSortie = excluded.ApresMidiSortie, " +
            "  Observation = excluded.Observation", conn);
        cmd.Parameters.AddWithValue("@pid", personnelId);
        cmd.Parameters.AddWithValue("@d", date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@ma", matinArrivee.HasValue ? matinArrivee.Value.ToString(@"hh\:mm\:ss") : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ms", matinSortie.HasValue ? matinSortie.Value.ToString(@"hh\:mm\:ss") : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@aa", apresMidiArrivee.HasValue ? apresMidiArrivee.Value.ToString(@"hh\:mm\:ss") : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@as", apresMidiSortie.HasValue ? apresMidiSortie.Value.ToString(@"hh\:mm\:ss") : (object)DBNull.Value);
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
                   SUM(CASE WHEN ep.MatinArrivee IS NOT NULL OR ep.MatinSortie IS NOT NULL THEN 1 ELSE 0 END) AS MatinsPresents,
                   SUM(CASE WHEN ep.ApresMidiArrivee IS NOT NULL OR ep.ApresMidiSortie IS NOT NULL THEN 1 ELSE 0 END) AS ApresMidisPresents,
                   SUM(
                       COALESCE(
                           (CAST(strftime('%H', ep.MatinSortie) AS INTEGER) * 3600 +
                            CAST(strftime('%M', ep.MatinSortie) AS INTEGER) * 60 +
                            CAST(strftime('%S', ep.MatinSortie) AS INTEGER))
                           -
                           (CAST(strftime('%H', ep.MatinArrivee) AS INTEGER) * 3600 +
                            CAST(strftime('%M', ep.MatinArrivee) AS INTEGER) * 60 +
                            CAST(strftime('%S', ep.MatinArrivee) AS INTEGER)),
                           0
                       )
                       +
                       COALESCE(
                           (CAST(strftime('%H', ep.ApresMidiSortie) AS INTEGER) * 3600 +
                            CAST(strftime('%M', ep.ApresMidiSortie) AS INTEGER) * 60 +
                            CAST(strftime('%S', ep.ApresMidiSortie) AS INTEGER))
                           -
                           (CAST(strftime('%H', ep.ApresMidiArrivee) AS INTEGER) * 3600 +
                            CAST(strftime('%M', ep.ApresMidiArrivee) AS INTEGER) * 60 +
                            CAST(strftime('%S', ep.ApresMidiArrivee) AS INTEGER)),
                           0
                       )
                   ) AS TotalSecondes,
                   AVG(
                       CAST(strftime('%H', ep.MatinArrivee) AS INTEGER) * 3600 +
                       CAST(strftime('%M', ep.MatinArrivee) AS INTEGER) * 60 +
                       CAST(strftime('%S', ep.MatinArrivee) AS INTEGER)
                   ) AS MoyenneArriveeSec
            FROM Personnel p
            LEFT JOIN EnregistrementPointage ep
                ON ep.PersonnelId = p.Id
               AND ep.DatePointage BETWEEN @d1 AND @d2
            GROUP BY p.Id, p.Nom, p.Prenom
            ORDER BY p.Nom, p.Prenom";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@d1", debut.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@d2", fin.ToString("yyyy-MM-dd"));
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int jours = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
            int matins = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
            int apresMidis = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);

            string totalHeures = "-";
            if (!reader.IsDBNull(5))
            {
                long sec = (long)reader.GetDouble(5);
                var ts = TimeSpan.FromSeconds(sec);
                totalHeures = $"{(int)ts.TotalHours}h{ts.Minutes:D2}";
            }

            string moyArr = "-";
            if (!reader.IsDBNull(6))
            {
                double sec = reader.GetDouble(6);
                var ts = TimeSpan.FromSeconds(sec);
                moyArr = $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }

            liste.Add(new BilanPersonnel
            {
                Nom = $"{reader.GetString(0)} {reader.GetString(1)}".Trim(),
                JoursPresents = jours,
                MatinsPresents = matins,
                ApresMidisPresents = apresMidis,
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