namespace Pointage.Models;

public class Personnel
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Matricule { get; set; }
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? Email { get; set; }
    public string? Statut { get; set; }
    public string? Grade { get; set; }
    public string? SituationFamiliale { get; set; }

    public string NomComplet => $"{Nom} {Prenom}".Trim();
}