namespace Pointage.Models;

/// <summary>
/// Bilan agrege pour une personne sur une periode donnee.
/// </summary>
public class BilanPersonnel
{
    public string Nom { get; set; } = string.Empty;
    public int JoursPresents { get; set; }
    public string TotalHeures { get; set; } = "-";
    public string MoyenneArrivee { get; set; } = "-";
}

/// <summary>
/// Bilan agrege pour un jour donne (nombre de presents / absents).
/// </summary>
public class BilanJour
{
    public string Date { get; set; } = string.Empty;
    public int Presents { get; set; }
    public int Absents { get; set; }
}