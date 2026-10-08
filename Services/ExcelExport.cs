using ClosedXML.Excel;
using Pointage.Models;

namespace Pointage.Services;

public static class ExcelExport
{
    public static string ExporterTout()
    {
        var dossier = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Pointage Exports");

        if (!Directory.Exists(dossier))
            Directory.CreateDirectory(dossier);

        string nomFichier = $"pointage_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
        string chemin = Path.Combine(dossier, nomFichier);

        using var workbook = new XLWorkbook();

        // ---------- Feuille 1 : Personnel ----------
        var wsPersonnel = workbook.Worksheets.Add("Personnel");
        wsPersonnel.Cell(1, 1).Value = "NOM";
        wsPersonnel.Cell(1, 2).Value = "PRÉNOM";

        var personnel = Database.GetPersonnel();
        int ligne = 2;
        foreach (var p in personnel)
        {
            wsPersonnel.Cell(ligne, 1).Value = p.Nom;
            wsPersonnel.Cell(ligne, 2).Value = p.Prenom;
            ligne++;
        }

        StyliserEntetes(wsPersonnel, 2);
        wsPersonnel.Columns().AdjustToContents();

        // ---------- Feuille 2 : Pointages ----------
        var wsPointages = workbook.Worksheets.Add("Pointages");
        wsPointages.Cell(1, 1).Value = "NOM";
        wsPointages.Cell(1, 2).Value = "PRÉNOM";
        wsPointages.Cell(1, 3).Value = "DATE";
        wsPointages.Cell(1, 4).Value = "ARRIVÉE";
        wsPointages.Cell(1, 5).Value = "SORTIE";
        wsPointages.Cell(1, 6).Value = "DURÉE";
        wsPointages.Cell(1, 7).Value = "OBSERVATION";

        var tousPointages = Database.GetTousPointages();
        ligne = 2;
        foreach (var pt in tousPointages)
        {
            var nomPrenom = (pt.NomComplet ?? "").Split(' ');
            wsPointages.Cell(ligne, 1).Value = nomPrenom.Length > 0 ? nomPrenom[0] : "";
            wsPointages.Cell(ligne, 2).Value = nomPrenom.Length > 1 ? nomPrenom[1] : "";
            wsPointages.Cell(ligne, 3).Value = pt.DatePointage.ToString("dd/MM/yyyy");
            wsPointages.Cell(ligne, 4).Value = pt.HeureArrivee?.ToString(@"hh\:mm") ?? "";
            wsPointages.Cell(ligne, 5).Value = pt.HeureSortie?.ToString(@"hh\:mm") ?? "";

            if (pt.HeureArrivee.HasValue && pt.HeureSortie.HasValue)
            {
                var duree = pt.HeureSortie.Value - pt.HeureArrivee.Value;
                wsPointages.Cell(ligne, 6).Value = $"{(int)duree.TotalHours}h{duree.Minutes:D2}";
            }
            else
            {
                wsPointages.Cell(ligne, 6).Value = "";
            }

            wsPointages.Cell(ligne, 7).Value = pt.Observation ?? "";
            ligne++;
        }

        StyliserEntetes(wsPointages, 7);
        wsPointages.Columns().AdjustToContents();

        workbook.SaveAs(chemin);
        return chemin;
    }

    public static string ExporterBilanPersonnel(DateTime debut, DateTime fin)
    {
        var dossier = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Pointage Exports");

        if (!Directory.Exists(dossier))
            Directory.CreateDirectory(dossier);

        string nomFichier = $"bilan_personnel_{debut:yyyy-MM-dd}_{fin:yyyy-MM-dd}.xlsx";
        string chemin = Path.Combine(dossier, nomFichier);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bilan Personnel");

        ws.Cell(1, 1).Value = "PERSONNE";
        ws.Cell(1, 2).Value = "JOURS PRÉSENT";
        ws.Cell(1, 3).Value = "TOTAL HEURES";
        ws.Cell(1, 4).Value = "ARRIVÉE MOYENNE";

        var bilans = Database.GetBilanPersonnel(debut, fin);
        int ligne = 2;
        foreach (var b in bilans)
        {
            ws.Cell(ligne, 1).Value = b.Nom;
            ws.Cell(ligne, 2).Value = b.JoursPresents;
            ws.Cell(ligne, 3).Value = b.TotalHeures;
            ws.Cell(ligne, 4).Value = b.MoyenneArrivee;
            ligne++;
        }

        StyliserEntetes(ws, 4);
        ws.Columns().AdjustToContents();

        workbook.SaveAs(chemin);
        return chemin;
    }

    public static string ExporterBilanJours(DateTime debut, DateTime fin)
    {
        var dossier = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Pointage Exports");

        if (!Directory.Exists(dossier))
            Directory.CreateDirectory(dossier);

        string nomFichier = $"bilan_jours_{debut:yyyy-MM-dd}_{fin:yyyy-MM-dd}.xlsx";
        string chemin = Path.Combine(dossier, nomFichier);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Bilan Jours");

        ws.Cell(1, 1).Value = "DATE";
        ws.Cell(1, 2).Value = "PRÉSENTS";
        ws.Cell(1, 3).Value = "ABSENTS";

        var bilans = Database.GetBilanJours(debut, fin);
        int ligne = 2;
        foreach (var b in bilans)
        {
            ws.Cell(ligne, 1).Value = b.Date;
            ws.Cell(ligne, 2).Value = b.Presents;
            ws.Cell(ligne, 3).Value = b.Absents;
            ligne++;
        }

        StyliserEntetes(ws, 3);
        ws.Columns().AdjustToContents();

        workbook.SaveAs(chemin);
        return chemin;
    }

    private static void StyliserEntetes(IXLWorksheet ws, int nbColonnes)
    {
        var plage = ws.Range(1, 1, 1, nbColonnes);
        plage.Style.Font.Bold = true;
        plage.Style.Fill.BackgroundColor = XLColor.FromArgb(21, 43, 84);
        plage.Style.Font.FontColor = XLColor.White;
        plage.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        plage.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(1).Height = 24;
    }
}