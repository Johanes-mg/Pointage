namespace Pointage.Models;

public class BilanPersonnel
{
    public string Nom { get; set; } = string.Empty;
    public int JoursPresents { get; set; }
    public int MatinsPresents { get; set; }
    public int ApresMidisPresents { get; set; }
    public string TotalHeures { get; set; } = "-";
    public string MoyenneArrivee { get; set; } = "-";
}

public class BilanJour
{
    public string Date { get; set; } = string.Empty;
    public int Presents { get; set; }
    public int Absents { get; set; }
}