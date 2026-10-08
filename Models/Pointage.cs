namespace Pointage.Models;

public class EnregistrementPointage
{
    public int Id { get; set; }
    public int PersonnelId { get; set; }
    public DateTime DatePointage { get; set; }
    public TimeSpan? HeureArrivee { get; set; }
    public TimeSpan? HeureSortie { get; set; }
    public string? Observation { get; set; }
    public string? NomComplet { get; set; }
}