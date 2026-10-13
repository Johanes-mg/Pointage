using Pointage.Models;
using Pointage.Services;

namespace Pointage.Forms;

public class CompteForm : Form
{
    private readonly Utilisateur _utilisateur;

    private TextBox _txtNomComplet = null!;
    private TextBox _txtAncienMdp = null!;
    private TextBox _txtNouveauMdp = null!;
    private TextBox _txtConfirmMdp = null!;

    private Label _lblMessageMdp = null!;
    private Label _lblMessageProfil = null!;

    public CompteForm(Utilisateur utilisateur)
    {
        _utilisateur = utilisateur;

        Dock = DockStyle.Fill;
        BackColor = Theme.Fond;
        Icon = Theme.ChargerIcone("logo.ico");

        ConstruireInterface();
        ChargerDonnees();
    }

    private void ConstruireInterface()
    {
        var panelEntete = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Theme.Fond,
            Padding = new Padding(40, 20, 40, 0)
        };
        panelEntete.Controls.Add(new Label
        {
            Text = "Mon compte",
            Font = new Font(Theme.FamillePolice, 18F, FontStyle.Bold),
            ForeColor = Theme.Primaire,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(40, 10, 40, 30),
            BackColor = Theme.Fond
        };

        var pile = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Theme.Fond,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        pile.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        pile.Controls.Add(CreerCarteProfil(), 0, 0);
        pile.Controls.Add(CreerCarteMotDePasse(), 0, 1);
        pile.Controls.Add(CreerCarteBase(), 0, 2);

        scroll.Controls.Add(pile);

        Controls.Add(scroll);
        Controls.Add(panelEntete);
    }

    private Panel CreerCarteProfil()
    {
        var carte = CreerCarte("Mon profil", 180);

        var lbl = CreerLabel("Nom complet", 30, 60);
        _txtNomComplet = CreerTextBox(30, 82, 500);
        carte.Controls.Add(lbl);
        carte.Controls.Add(_txtNomComplet);

        var btnEnregistrer = Theme.CreerBoutonBlancArrondi("enregistrer.png", "Enregistrer");
        btnEnregistrer.Location = new Point(550, 79);
        btnEnregistrer.Width = 170;
        btnEnregistrer.Click += BtnEnregistrerProfil_Click;
        carte.Controls.Add(btnEnregistrer);

        _lblMessageProfil = new Label
        {
            Location = new Point(30, 125),
            AutoSize = true,
            Font = Theme.Petit,
            ForeColor = Theme.Succes
        };
        carte.Controls.Add(_lblMessageProfil);

        return carte;
    }

    private Panel CreerCarteMotDePasse()
    {
        var carte = CreerCarte("Changer le mot de passe", 320);

        var lblAncien = CreerLabel("Ancien mot de passe", 30, 60);
        _txtAncienMdp = CreerTextBox(30, 82, 500);
        _txtAncienMdp.UseSystemPasswordChar = true;
        carte.Controls.Add(lblAncien);
        carte.Controls.Add(_txtAncienMdp);

        var lblNouveau = CreerLabel("Nouveau mot de passe", 30, 120);
        _txtNouveauMdp = CreerTextBox(30, 142, 500);
        _txtNouveauMdp.UseSystemPasswordChar = true;
        carte.Controls.Add(lblNouveau);
        carte.Controls.Add(_txtNouveauMdp);

        var lblConfirm = CreerLabel("Confirmer le nouveau mot de passe", 30, 180);
        _txtConfirmMdp = CreerTextBox(30, 202, 500);
        _txtConfirmMdp.UseSystemPasswordChar = true;
        carte.Controls.Add(lblConfirm);
        carte.Controls.Add(_txtConfirmMdp);

        var btnChanger = Theme.CreerBoutonBlancArrondi("enregistrer.png", "Changer le mot de passe");
        btnChanger.Location = new Point(30, 250);
        btnChanger.Width = 280;
        btnChanger.Click += BtnChangerMdp_Click;
        carte.Controls.Add(btnChanger);

        _lblMessageMdp = new Label
        {
            Location = new Point(330, 258),
            AutoSize = true,
            Font = Theme.Petit
        };
        carte.Controls.Add(_lblMessageMdp);

        return carte;
    }

    private Panel CreerCarteBase()
    {
        var carte = CreerCarte("Base de données (import / export)", 230);

        var lblInfo = new Label
        {
            Text = "Emplacement de la base de données :",
            Location = new Point(30, 60),
            AutoSize = true,
            Font = Theme.PetitGras,
            ForeColor = Theme.TexteSecondaire
        };
        carte.Controls.Add(lblInfo);

        var lblChemin = new Label
        {
            Text = Database.GetCheminBase(),
            Location = new Point(30, 82),
            Width = 900,
            Height = 22,
            Font = Theme.Petit,
            ForeColor = Theme.Texte,
            AutoEllipsis = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        carte.Controls.Add(lblChemin);

        var btnExporter = Theme.CreerBoutonBlancArrondi("exporter.png", "Exporter la base (.db)");
        btnExporter.Location = new Point(30, 115);
        btnExporter.Width = 280;
        btnExporter.Click += BtnExporterBase_Click;
        carte.Controls.Add(btnExporter);

        var btnImporter = Theme.CreerBoutonBlancArrondi("ajouter.png", "Importer une base (.db)");
        btnImporter.Location = new Point(330, 115);
        btnImporter.Width = 280;
        btnImporter.Click += BtnImporterBase_Click;
        carte.Controls.Add(btnImporter);

        var lblAvert = new Label
        {
            Text = "Attention : l'import remplace TOUTES les données actuelles. Un redémarrage est nécessaire.",
            Location = new Point(30, 170),
            AutoSize = true,
            Font = Theme.PetitGras,
            ForeColor = Theme.Danger
        };
        carte.Controls.Add(lblAvert);

        return carte;
    }

    private Panel CreerCarte(string titre, int hauteur)
    {
        var carte = new Panel
        {
            Dock = DockStyle.Top,
            Height = hauteur,
            BackColor = Theme.FondCarte,
            Margin = new Padding(0, 0, 0, 20),
            Padding = new Padding(0)
        };

        carte.Controls.Add(new Label
        {
            Text = titre,
            Location = new Point(30, 20),
            AutoSize = true,
            Font = new Font(Theme.FamillePolice, 14F, FontStyle.Bold),
            ForeColor = Theme.Primaire
        });

        carte.ParentChanged += (_, _) =>
        {
            if (carte.Parent != null)
                carte.Width = Math.Max(700, carte.Parent.ClientSize.Width);
        };

        return carte;
    }

    private Label CreerLabel(string texte, int x, int y)
    {
        return new Label
        {
            Text = texte,
            Location = new Point(x, y),
            AutoSize = true,
            Font = Theme.PetitGras,
            ForeColor = Theme.TexteSecondaire
        };
    }

    private TextBox CreerTextBox(int x, int y, int largeur)
    {
        return new TextBox
        {
            Location = new Point(x, y),
            Width = largeur,
            Height = 30,
            Font = new Font(Theme.FamillePolice, 11F),
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private void ChargerDonnees()
    {
        _txtNomComplet.Text = _utilisateur.NomComplet;
    }

    private void BtnEnregistrerProfil_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtNomComplet.Text))
        {
            _lblMessageProfil.ForeColor = Theme.Danger;
            _lblMessageProfil.Text = "Le nom complet est obligatoire.";
            return;
        }

        try
        {
            Database.MettreAJourNomComplet(_utilisateur.Id, _txtNomComplet.Text.Trim());
            _utilisateur.NomComplet = _txtNomComplet.Text.Trim();

            _lblMessageProfil.ForeColor = Theme.Succes;
            _lblMessageProfil.Text = "Profil mis à jour. Redémarrez pour voir le changement dans la barre du haut.";
        }
        catch (Exception ex)
        {
            _lblMessageProfil.ForeColor = Theme.Danger;
            _lblMessageProfil.Text = "Erreur : " + ex.Message;
        }
    }

    private void BtnChangerMdp_Click(object? sender, EventArgs e)
    {
        _lblMessageMdp.Text = "";

        if (string.IsNullOrWhiteSpace(_txtAncienMdp.Text) ||
            string.IsNullOrWhiteSpace(_txtNouveauMdp.Text) ||
            string.IsNullOrWhiteSpace(_txtConfirmMdp.Text))
        {
            _lblMessageMdp.ForeColor = Theme.Danger;
            _lblMessageMdp.Text = "Tous les champs sont obligatoires.";
            return;
        }

        if (_txtNouveauMdp.Text.Length < 4)
        {
            _lblMessageMdp.ForeColor = Theme.Danger;
            _lblMessageMdp.Text = "Le nouveau mot de passe doit contenir au moins 4 caractères.";
            return;
        }

        if (_txtNouveauMdp.Text != _txtConfirmMdp.Text)
        {
            _lblMessageMdp.ForeColor = Theme.Danger;
            _lblMessageMdp.Text = "Les deux mots de passe ne correspondent pas.";
            return;
        }

        try
        {
            bool ok = Database.ChangerMotDePasse(
                _utilisateur.Id,
                _txtAncienMdp.Text,
                _txtNouveauMdp.Text);

            if (!ok)
            {
                _lblMessageMdp.ForeColor = Theme.Danger;
                _lblMessageMdp.Text = "L'ancien mot de passe est incorrect.";
                return;
            }

            _txtAncienMdp.Clear();
            _txtNouveauMdp.Clear();
            _txtConfirmMdp.Clear();

            _lblMessageMdp.ForeColor = Theme.Succes;
            _lblMessageMdp.Text = "Mot de passe changé avec succès.";
        }
        catch (Exception ex)
        {
            _lblMessageMdp.ForeColor = Theme.Danger;
            _lblMessageMdp.Text = "Erreur : " + ex.Message;
        }
    }

    private void BtnExporterBase_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Exporter la base de données",
                Filter = "Base SQLite (*.db)|*.db|Tous les fichiers (*.*)|*.*",
                FileName = $"pointage_{DateTime.Now:yyyy-MM-dd}.db",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            Database.ExporterBase(dialog.FileName);

            MessageBox.Show(
                $"Base exportée avec succès.\n\n{dialog.FileName}",
                "Export réussi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Erreur lors de l'export :\n\n" + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnImporterBase_Click(object? sender, EventArgs e)
    {
        try
        {
            var reponse = MessageBox.Show(
                "ATTENTION !\n\n" +
                "L'import remplacera TOUTES les données actuelles " +
                "(personnel, pointages, utilisateurs, paramètres).\n\n" +
                "Cette action est irréversible. Continuer ?",
                "Confirmer l'import",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (reponse != DialogResult.Yes) return;

            using var dialog = new OpenFileDialog
            {
                Title = "Sélectionner une base à importer",
                Filter = "Base SQLite (*.db)|*.db|Tous les fichiers (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            Database.ImporterBase(dialog.FileName);

            MessageBox.Show(
                "Base importée avec succès.\n\n" +
                "L'application va redémarrer pour appliquer les changements.",
                "Import réussi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Restart();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Erreur lors de l'import :\n\n" + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}