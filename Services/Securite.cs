using System.Security.Cryptography;
using System.Text;

namespace Pointage.Services;

public static class Securite
{
    /// <summary>
    /// Genere un sel aleatoire de 16 octets encode en base64.
    /// </summary>
    public static string GenererSel()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Hash SHA256 du mot de passe + sel, encode en base64.
    /// </summary>
    public static string HacherMotDePasse(string motDePasse, string sel)
    {
        using var sha = SHA256.Create();
        var entree = Encoding.UTF8.GetBytes(motDePasse + sel);
        var hash = sha.ComputeHash(entree);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifierMotDePasse(string motDePasse, string sel, string hashAttendu)
    {
        var hash = HacherMotDePasse(motDePasse, sel);
        return hash == hashAttendu;
    }
}