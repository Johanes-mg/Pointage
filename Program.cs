using Pointage.Services;

namespace Pointage;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetDefaultFont(new Font("Segoe UI", 9F));

        try
        {
            Database.Initialiser();
        }
        catch (Exception ex)
        {
            string chemin = "(inconnu)";
            try { chemin = Database.GetCheminBase(); } catch { }

            MessageBox.Show(
                "Impossible de créer ou d'ouvrir la base de données.\n\n" +
                $"Chemin : {chemin}\n\n" +
                $"Erreur : {ex.Message}",
                "Erreur fatale",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        using var login = new Forms.LoginForm();
        if (login.ShowDialog() != DialogResult.OK || login.UtilisateurConnecte == null)
        {
            return;
        }

        Application.Run(new MainForm(login.UtilisateurConnecte));
    }
}