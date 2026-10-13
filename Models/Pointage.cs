namespace Pointage.Models;

public class EnregistrementPointage
{
    public int Id { get; set; }
    public int PersonnelId { get; set; }
    public DateTime DatePointage { get; set; }

    // Matin
    public TimeSpan? MatinArrivee { get; set; }
    public TimeSpan? MatinSortie { get; set; }

    // Après-midi
    public TimeSpan? ApresMidiArrivee { get; set; }
    public TimeSpan? ApresMidiSortie { get; set; }

    public string? Observation { get; set; }
    public string? NomComplet { get; set; }

    public bool MatinRempli => MatinArrivee.HasValue || MatinSortie.HasValue;
    public bool ApresMidiRempli => ApresMidiArrivee.HasValue || ApresMidiSortie.HasValue;
}