using System.Drawing.Drawing2D;
using System.Reflection;

namespace Pointage.Services;

public static class Theme
{
    public static readonly Color Primaire = Color.FromArgb(21, 43, 84);
    public static readonly Color PrimaireClair = Color.FromArgb(38, 66, 120);
    public static readonly Color Accent = Color.FromArgb(0, 122, 204);
    public static readonly Color AccentHover = Color.FromArgb(0, 145, 235);
    public static readonly Color Fond = Color.FromArgb(244, 246, 249);
    public static readonly Color FondCarte = Color.White;
    public static readonly Color FondBilan = Color.FromArgb(235, 242, 250);
    public static readonly Color Bordure = Color.FromArgb(222, 226, 232);
    public static readonly Color BordureNoire = Color.FromArgb(60, 60, 60);
    public static readonly Color Texte = Color.FromArgb(30, 35, 45);
    public static readonly Color TexteSecondaire = Color.FromArgb(110, 118, 130);
    public static readonly Color Succes = Color.FromArgb(34, 148, 84);
    public static readonly Color Danger = Color.FromArgb(200, 55, 55);
    public static readonly Color Warning = Color.FromArgb(220, 145, 20);
    public static readonly Color LigneAlternee = Color.FromArgb(250, 251, 253);
    public static readonly Color SelectionJour = Color.FromArgb(220, 235, 252);
    public static readonly Color FondPointage = Color.FromArgb(230, 248, 236);

    public const string FamillePolice = "Segoe UI";

    public static Font Titre => new(FamillePolice, 20F, FontStyle.Bold);
    public static Font SousTitre => new(FamillePolice, 13F, FontStyle.Regular);
    public static Font Bouton => new(FamillePolice, 10.5F, FontStyle.Regular);
    public static Font TexteNormal => new(FamillePolice, 10.5F, FontStyle.Regular);
    public static Font Petit => new(FamillePolice, 9.5F, FontStyle.Regular);
    public static Font PetitGras => new(FamillePolice, 9.5F, FontStyle.Bold);
    public static Font MoyenGras => new(FamillePolice, 11F, FontStyle.Bold);

    private static readonly Dictionary<string, Image?> _cache = new();
    private static readonly Dictionary<string, Icon> _cacheIcones = new();

    private static string ConstruireNomRessource(string nomFichier)
    {
        string nomNormalise = nomFichier
            .Replace('/', '.')
            .Replace('\\', '.');
        return $"Pointage.Images.{nomNormalise}";
    }

    public static Image? ChargerImage(string nomFichier)
    {
        string cle = nomFichier + "|source";
        if (_cache.TryGetValue(cle, out var cached))
            return cached;

        Image? img = null;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            string ressourceName = ConstruireNomRessource(nomFichier);

            using var stream = assembly.GetManifestResourceStream(ressourceName);
            if (stream != null)
            {
                using var original = Image.FromStream(stream);
                img = new Bitmap(original);
            }
        }
        catch
        {
            img = null;
        }

        if (img == null)
        {
            try
            {
                string baseDir = AppContext.BaseDirectory;
                string chemin = Path.Combine(baseDir, "Images", nomFichier);

                if (File.Exists(chemin))
                {
                    using var original = Image.FromFile(chemin);
                    img = new Bitmap(original);
                }
            }
            catch
            {
                img = null;
            }
        }

        _cache[cle] = img;
        return img;
    }

    public static Icon ChargerIcone(string nomFichier)
    {
        if (_cacheIcones.TryGetValue(nomFichier, out var cached))
            return cached;

        Icon icone = SystemIcons.Application;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            string ressourceName = ConstruireNomRessource(nomFichier);

            using var stream = assembly.GetManifestResourceStream(ressourceName);
            if (stream != null)
            {
                icone = new Icon(stream);
            }
        }
        catch
        {
            icone = SystemIcons.Application;
        }

        if (icone == SystemIcons.Application)
        {
            try
            {
                string baseDir = AppContext.BaseDirectory;
                string chemin = Path.Combine(baseDir, "Images", nomFichier);

                if (File.Exists(chemin))
                {
                    icone = new Icon(chemin);
                }
            }
            catch
            {
                icone = SystemIcons.Application;
            }
        }

        _cacheIcones[nomFichier] = icone;
        return icone;
    }

    public static Image? ChargerImageRedimensionnee(string nomFichier, int largeur, int hauteur)
    {
        string cle = $"{nomFichier}|{largeur}x{hauteur}";
        if (_cache.TryGetValue(cle, out var cached))
            return cached;

        var source = ChargerImage(nomFichier);
        if (source == null)
        {
            _cache[cle] = null;
            return null;
        }

        var bmp = new Bitmap(largeur, hauteur);
        using var g = Graphics.FromImage(bmp);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.DrawImage(source, 0, 0, largeur, hauteur);

        _cache[cle] = bmp;
        return bmp;
    }

    public static Image? Redimensionner(Image? source, int largeur, int hauteur)
    {
        if (source == null) return null;

        var bmp = new Bitmap(largeur, hauteur);
        using var g = Graphics.FromImage(bmp);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.DrawImage(source, 0, 0, largeur, hauteur);
        return bmp;
    }

    public static Button CreerBoutonBlancArrondi(string fichierImage, string texte, int tailleIcone = 22)
    {
        var btn = new BoutonArrondi
        {
            Text = "   " + texte,
            Height = 42,
            Width = 170,
            Font = Bouton,
            ForeColor = Texte,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };

        var img = ChargerImageRedimensionnee(fichierImage, tailleIcone, tailleIcone);
        if (img != null)
            btn.Image = img;

        return btn;
    }

    public static Button CreerBoutonBlancIconeSeule(string fichierImage, int taille = 44, int tailleIcone = 26)
    {
        var btn = new BoutonArrondi
        {
            Width = taille,
            Height = taille,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };

        var img = ChargerImageRedimensionnee(fichierImage, tailleIcone, tailleIcone);
        if (img != null)
        {
            btn.Image = img;
            btn.ImageAlign = ContentAlignment.MiddleCenter;
        }

        return btn;
    }

    public static Button CreerBoutonBlancArrondiTexte(string texte)
    {
        return new BoutonArrondi
        {
            Text = texte,
            Height = 42,
            Font = Bouton,
            ForeColor = Texte,
            BackColor = Color.White,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
    }
}

public class BoutonArrondi : Button
{
    public int Rayon { get; set; } = 10;
    public Color CouleurBordure { get; set; } = Color.FromArgb(60, 60, 60);

    public BoutonArrondi()
    {
        SetStyle(ControlStyles.UserPaint
               | ControlStyles.AllPaintingInWmPaint
               | ControlStyles.OptimizedDoubleBuffer
               | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.White);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = CreerCheminArrondi(rect, Rayon);

        using (var fond = new SolidBrush(BackColor))
            g.FillPath(fond, path);

        using (var pinceau = new Pen(CouleurBordure, 1))
            g.DrawPath(pinceau, path);

        string texte = string.IsNullOrEmpty(Text) ? "" : Text.Trim();
        int largeurIcone = Image?.Width ?? 0;
        int espaceIconeTexte = (Image != null && !string.IsNullOrEmpty(texte)) ? 12 : 0;
        Size tailleTexte = string.IsNullOrEmpty(texte)
            ? Size.Empty
            : TextRenderer.MeasureText(texte, Font);
        int largeurTotale = largeurIcone + espaceIconeTexte + tailleTexte.Width;

        int departX = (Width - largeurTotale) / 2;
        if (departX < 8) departX = 8;
        int centreY = Height / 2;

        if (Image != null)
        {
            int imgY = centreY - Image.Height / 2;
            g.DrawImage(Image, departX, imgY, Image.Width, Image.Height);
            departX += largeurIcone + espaceIconeTexte;
        }

        if (!string.IsNullOrEmpty(texte))
        {
            using var pinceau = new SolidBrush(ForeColor);
            var format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Near
            };
            var rectTexte = new Rectangle(departX, 0, Width - departX - 8, Height);
            g.DrawString(texte, Font, pinceau, rectTexte, format);
        }
    }

    private static GraphicsPath CreerCheminArrondi(Rectangle rect, int rayon)
    {
        var path = new GraphicsPath();
        int d = rayon * 2;

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }
}