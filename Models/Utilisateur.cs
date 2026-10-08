namespace Pointage.Models;

public class Utilisateur
{
    public int Id { get; set; }
    public string NomUtilisateur { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    public string Sel { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Role { get; set; } = "operateur";
    public DateTime DateCreation { get; set; }
}