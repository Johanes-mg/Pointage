using ClosedXML.Excel;
using Pointage.Models;

namespace Pointage.Services;

public static class ExcelExport
{
    public static string ExporterTout(DateTime debut, DateTime fin)
    {
        var dossier = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Pointage Exports");

        if (!Directory.Exists(dossier))
            Directory.CreateDirectory(dossier);

        string periode = debut.Date == fin.Date
            ? debut.ToString("yyyy-MM-dd")
            : $"{debut:yyyy-MM-dd}_{fin:yyyy-MM-dd}";

        string nomFichier = $"pointage_{periode}.xlsx";
        string chemin = Path.Combine(dossier, nomFichier);

        using var workbook = new XLWorkbook();

        var wsPersonnel = workbook.Worksheets.Add("Personnel");
        wsPersonnel.Cell(1, 1).Value = "MATRICULE";
        wsPersonnel.Cell(1, 2).Value = "NOM";
        wsPersonnel.Cell(1, 3).Value = "PRÉNOM";
        wsPersonnel.Cell(1, 4).Value = "TÉLÉPHONE";
        wsPersonnel.Cell(1, 5).Value = "ADRESSE";
        wsPersonnel.Cell(1, 6).Value = "EMAIL";
        wsPersonnel.Cell(1, 7).Value = "STATUT";
        wsPersonnel.Cell(1, 8).Value = "GRADE";
        wsPersonnel.Cell(1, 9).Value = "SITUATION FAMILIALE";

        var personnel = Database.GetPersonnel();
        int ligne = 2;
        foreach (var p in personnel)
        {
            wsPersonnel.Cell(ligne, 1).Value = p.Matricule ?? "";
            wsPersonnel.Cell(ligne, 2).Value = p.Nom;
            wsPersonnel.Cell(ligne, 3).Value = p.Prenom;
            wsPersonnel.Cell(ligne, 4).Value = p.Telephone ?? "";
            wsPersonnel.Cell(ligne, 5).Value = p.Adresse ?? "";
            wsPersonnel.Cell(ligne, 6).Value = p.Email ?? "";
            wsPersonnel.Cell(ligne, 7).Value = p.Statut ?? "";
            wsPersonnel.Cell(ligne, 8).Value = p.Grade ?? "";
            wsPersonnel.Cell(ligne, 9).Value = p.SituationFamiliale ?? "";
            ligne++;
        }

        StyliserEntetes(wsPersonnel, 9);
        wsPersonnel.Columns().AdjustToContents();

        var wsPointages = workbook.Worksheets.Add("Pointages");
        wsPointages.Cell(1, 1).Value = "MATRICULE";
        wsPointages.Cell(1, 2).Value = "NOM";
        wsPointages.Cell(1, 3).Value = "PRÉNOM";
        wsPointages.Cell(1, 4).Value = "DATE";
        wsPointages.Cell(1, 5).Value = "MATIN ARRIVÉE";
        wsPointages.Cell(1, 6).Value = "MATIN SORTIE";
        wsPointages.Cell(1, 7).Value = "APRÈS-MIDI ARRIVÉE";
        wsPointages.Cell(1, 8).Value = "APRÈS-MIDI SORTIE";
        wsPointages.Cell(1, 9).Value = "TOTAL JOUR";
        wsPointages.Cell(1, 10).Value = "OBSERVATION";

        var matricules = personnel.ToDictionary(p => p.Id, p => p.Matricule ?? "");

        var pointages = Database.GetPointagesPeriode(debut, fin);
        ligne = 2;
        foreach (var pt in pointages)
        {
            var nomPrenom = (pt.NomComplet ?? "").Split(' ');
            wsPointages.Cell(ligne, 1).Value = matricules.TryGetValue(pt.PersonnelId, out var m) ? m : "";
            wsPointages.Cell(ligne, 2).Value = nomPrenom.Length > 0 ? nomPrenom[0] : "";
            wsPointages.Cell(ligne, 3).Value = nomPrenom.Length > 1 ? nomPrenom[1] : "";
            wsPointages.Cell(ligne, 4).Value = pt.DatePointage.ToString("dd/MM/yyyy");
            wsPointages.Cell(ligne, 5).Value = pt.MatinArrivee?.ToString(@"hh\:mm") ?? "";
            wsPointages.Cell(ligne, 6).Value = pt.MatinSortie?.ToString(@"hh\:mm") ?? "";
            wsPointages.Cell(ligne, 7).Value = pt.ApresMidiArrivee?.ToString(@"hh\:mm") ?? "";
            wsPointages.Cell(ligne, 8).Value = pt.ApresMidiSortie?.ToString(@"hh\:mm") ?? "";

            TimeSpan total = TimeSpan.Zero;

            if (pt.MatinArrivee.HasValue && pt.MatinSortie.HasValue)
                total += pt.MatinSortie.Value - pt.MatinArrivee.Value;

            if (pt.ApresMidiArrivee.HasValue && pt.ApresMidiSortie.HasValue)
                total += pt.ApresMidiSortie.Value - pt.ApresMidiArrivee.Value;

            wsPointages.Cell(ligne, 9).Value = total.TotalSeconds > 0
                ? $"{(int)total.TotalHours}h{total.Minutes:D2}"
                : "";

            wsPointages.Cell(ligne, 10).Value = pt.Observation ?? "";
            ligne++;
        }

        StyliserEntetes(wsPointages, 10);
        wsPointages.Columns().AdjustToContents();

        var wsBilan = workbook.Worksheets.Add("Bilan");
        wsBilan.Cell(1, 1).Value = "PERSONNE";
        wsBilan.Cell(1, 2).Value = "JOURS PRÉSENT";
        wsBilan.Cell(1, 3).Value = "MATINS";
        wsBilan.Cell(1, 4).Value = "APRÈS-MIDIS";
        wsBilan.Cell(1, 5).Value = "TOTAL HEURES";
        wsBilan.Cell(1, 6).Value = "ARRIVÉE MOYENNE";

        var bilans = Database.GetBilanPersonnel(debut, fin);
        ligne = 2;
        foreach (var b in bilans)
        {
            wsBilan.Cell(ligne, 1).Value = b.Nom;
            wsBilan.Cell(ligne, 2).Value = b.JoursPresents;
            wsBilan.Cell(ligne, 3).Value = b.MatinsPresents;
            wsBilan.Cell(ligne, 4).Value = b.ApresMidisPresents;
            wsBilan.Cell(ligne, 5).Value = b.TotalHeures;
            wsBilan.Cell(ligne, 6).Value = b.MoyenneArrivee;
            ligne++;
        }

        StyliserEntetes(wsBilan, 6);
        wsBilan.Columns().AdjustToContents();

        var wsInfo = workbook.Worksheets.Add("Période");
        wsInfo.Cell(1, 1).Value = "Service";
        wsInfo.Cell(1, 2).Value = Database.GetNomEntreprise();
        wsInfo.Cell(2, 1).Value = "Du";
        wsInfo.Cell(2, 2).Value = debut.ToString("dd/MM/yyyy");
        wsInfo.Cell(3, 1).Value = "Au";
        wsInfo.Cell(3, 2).Value = fin.ToString("dd/MM/yyyy");
        wsInfo.Cell(4, 1).Value = "Exporté le";
        wsInfo.Cell(4, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        wsInfo.Column(1).Style.Font.Bold = true;
        wsInfo.Columns().AdjustToContents();

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