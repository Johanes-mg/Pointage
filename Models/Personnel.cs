namespace Pointage.Models;

public class Personnel
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;

    public string NomComplet => $"{Nom} {Prenom}".Trim();
}