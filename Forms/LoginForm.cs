using Pointage.Models;
using Pointage.Services;

namespace Pointage.Forms;

public class LoginForm : Form
{
    private TextBox _txtNomComplet = null!;
    private TextBox _txtUtilisateur = null!;
    private TextBox _txtMotDePasse = null!;
    private TextBox _txtMotDePasseConfirm = null!;

    private PictureBox _picVoirMdp = null!;
    private PictureBox _picVoirConfirm = null!;

    private Button _btnPrincipal = null!;
    private Button _btnSecondaire = null!;

    private Label _lblTitre = null!;
    private Label _lblMessage = null!;
    private Label _lblNomComplet = null!;
    private Label _lblConfirm = null!;
    private Label _lblUtilisateur = null!;
    private Label _lblMotDePasse = null!;

    private bool _modeCreation;
    private bool _mdpVisible;
    private bool _confirmVisible;

    public Utilisateur? UtilisateurConnecte { get; private set; }

    public LoginForm()
    {
        Text = "Pointage - Connexion";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(500, 620);
        BackColor = Theme.FondCarte;
        Font = Theme.TexteNormal;
        Icon = Theme.ChargerIcone("login.ico");

        bool premierLancement;
        try
        {
            premierLancement = Database.AucunUtilisateur();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Impossible de se connecter à la base de données.\n\n" + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
            return;
        }

        _modeCreation = premierLancement;

        ConstruireInterface();
        MettreAJourMode();
    }

    private void ConstruireInterface()
    {
        var panelTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 140,
            BackColor = Theme.Primaire
        };

        var logo = Theme.ChargerImageRedimensionnee("logo.png", 60, 60);
        if (logo != null)
        {
            panelTop.Controls.Add(new PictureBox
            {
                Image = logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 60,
                Height = 60,
                Left = 50,
                Top = 40
            });
        }

        _lblTitre = new Label
        {
            Text = "Connexion",
            Font = new Font(Theme.FamillePolice, 22F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(130, 55),
            AutoSize = true
        };
        panelTop.Controls.Add(_lblTitre);

        Controls.Add(panelTop);

        // ---- Nom complet ----
        _lblNomComplet = CreerLabel("Nom complet");
        _txtNomComplet = CreerTextBox();

        // ---- Nom d'utilisateur ----
        _lblUtilisateur = CreerLabel("Nom d'utilisateur");
        _txtUtilisateur = CreerTextBox();

        // ---- Mot de passe ----
        _lblMotDePasse = CreerLabel("Mot de passe");
        _txtMotDePasse = CreerTextBox();
        _txtMotDePasse.UseSystemPasswordChar = true;

        _picVoirMdp = CreerPictureBoxOeil();
        _picVoirMdp.Click += (_, _) =>
        {
            _mdpVisible = !_mdpVisible;
            _txtMotDePasse.UseSystemPasswordChar = !_mdpVisible;
            MettreAJourIconeOeil(_picVoirMdp, _mdpVisible);
        };

        // ---- Confirmer mot de passe ----
        _lblConfirm = CreerLabel("Confirmer le mot de passe");
        _txtMotDePasseConfirm = CreerTextBox();
        _txtMotDePasseConfirm.UseSystemPasswordChar = true;

        _picVoirConfirm = CreerPictureBoxOeil();
        _picVoirConfirm.Click += (_, _) =>
        {
            _confirmVisible = !_confirmVisible;
            _txtMotDePasseConfirm.UseSystemPasswordChar = !_confirmVisible;
            MettreAJourIconeOeil(_picVoirConfirm, _confirmVisible);
        };

        // ---- Message d'erreur ----
        _lblMessage = new Label
        {
            Width = 400,
            Height = 40,
            Font = Theme.Petit,
            ForeColor = Theme.Danger,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // ---- Bouton principal ----
        _btnPrincipal = new Button
        {
            Width = 400,
            Height = 46,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Accent,
            ForeColor = Color.White,
            Font = new Font(Theme.FamillePolice, 12F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnPrincipal.FlatAppearance.BorderSize = 0;
        _btnPrincipal.Click += BtnPrincipal_Click;

        // ---- Bouton basculer ----
        _btnSecondaire = new Button
        {
            Width = 400,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Theme.Accent,
            Font = Theme.Bouton,
            Cursor = Cursors.Hand
        };
        _btnSecondaire.FlatAppearance.BorderColor = Theme.Accent;
        _btnSecondaire.FlatAppearance.BorderSize = 1;
        _btnSecondaire.Click += (_, _) =>
        {
            _modeCreation = !_modeCreation;
            MettreAJourMode();
        };

        Controls.Add(_lblNomComplet);
        Controls.Add(_txtNomComplet);
        Controls.Add(_lblUtilisateur);
        Controls.Add(_txtUtilisateur);
        Controls.Add(_lblMotDePasse);
        Controls.Add(_txtMotDePasse);
        Controls.Add(_picVoirMdp);
        Controls.Add(_lblConfirm);
        Controls.Add(_txtMotDePasseConfirm);
        Controls.Add(_picVoirConfirm);
        Controls.Add(_lblMessage);
        Controls.Add(_btnPrincipal);
        Controls.Add(_btnSecondaire);

        // Touche Entrée = champ suivant, et sur le dernier champ = valider
        _txtNomComplet.KeyDown += (s, e) => PasserAuSuivant(s, e, _txtUtilisateur);
        _txtUtilisateur.KeyDown += (s, e) => PasserAuSuivant(s, e, _txtMotDePasse);
        _txtMotDePasse.KeyDown += (s, e) => PasserAuSuivant(s, e,
            _modeCreation ? _txtMotDePasseConfirm : (Control)_btnPrincipal);
        _txtMotDePasseConfirm.KeyDown += (s, e) => PasserAuSuivant(s, e, _btnPrincipal);

        _picVoirMdp.BringToFront();
        _picVoirConfirm.BringToFront();
    }

    private Label CreerLabel(string texte)
    {
        return new Label
        {
            Text = texte,
            Font = Theme.PetitGras,
            ForeColor = Theme.TexteSecondaire,
            AutoSize = true
        };
    }

    private TextBox CreerTextBox()
    {
        return new TextBox
        {
            Width = 400,
            Height = 34,
            Font = new Font(Theme.FamillePolice, 11F),
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private PictureBox CreerPictureBoxOeil()
    {
        var pic = new PictureBox
        {
            Width = 20,
            Height = 20,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };

        var img = Theme.ChargerImageRedimensionnee("non-cacher.png", 16, 16);
        if (img != null) pic.Image = img;

        return pic;
    }

    private void MettreAJourIconeOeil(PictureBox pic, bool visible)
    {
        var nom = visible ? "cacher.png" : "non-cacher.png";
        var img = Theme.ChargerImageRedimensionnee(nom, 16, 16);
        pic.Image = img;
        pic.Invalidate();
    }

    private void PasserAuSuivant(object? sender, KeyEventArgs e, Control suivant)
    {
        if (e.KeyCode != Keys.Enter) return;

        e.SuppressKeyPress = true;
        e.Handled = true;

        if (suivant == _btnPrincipal)
        {
            BtnPrincipal_Click(null, EventArgs.Empty);
            return;
        }

        suivant.Focus();
    }

    private void MettreAJourMode()
    {
        _lblMessage.Text = "";

        if (_modeCreation)
        {
            Text = "Pointage - Créer un compte";
            _lblTitre.Text = "Créer un compte";
            _btnPrincipal.Text = "Créer le compte";
            _btnSecondaire.Text = "J'ai déjà un compte";

            _lblNomComplet.Visible = true;
            _txtNomComplet.Visible = true;
            _lblConfirm.Visible = true;
            _txtMotDePasseConfirm.Visible = true;
            _picVoirConfirm.Visible = true;
        }
        else
        {
            Text = "Pointage - Connexion";
            _lblTitre.Text = "Connexion";
            _btnPrincipal.Text = "Se connecter";
            _btnSecondaire.Text = "Créer un compte";

            _lblNomComplet.Visible = false;
            _txtNomComplet.Visible = false;
            _lblConfirm.Visible = false;
            _txtMotDePasseConfirm.Visible = false;
            _picVoirConfirm.Visible = false;
        }

        Repositionner();
    }

    private void Repositionner()
    {
        int x = 50;
        int y = 175;

        if (_modeCreation)
        {
            _lblNomComplet.Location = new Point(x, y);
            _txtNomComplet.Location = new Point(x, y + 22);
            y += 70;
        }

        _lblUtilisateur.Location = new Point(x, y);
        _txtUtilisateur.Location = new Point(x, y + 22);
        y += 70;

        _lblMotDePasse.Location = new Point(x, y);
        _txtMotDePasse.Location = new Point(x, y + 22);
        _picVoirMdp.Location = new Point(x + 374, y + 29);
        y += 70;

        if (_modeCreation)
        {
            _lblConfirm.Location = new Point(x, y);
            _txtMotDePasseConfirm.Location = new Point(x, y + 22);
            _picVoirConfirm.Location = new Point(x + 374, y + 29);
            y += 70;
        }

        _lblMessage.Location = new Point(x, y + 5);
        _btnPrincipal.Location = new Point(x, y + 55);
        _btnSecondaire.Location = new Point(x, y + 115);

        ClientSize = new Size(500, y + 175);

        _picVoirMdp.BringToFront();
        _picVoirConfirm.BringToFront();
    }

    private void BtnPrincipal_Click(object? sender, EventArgs e)
    {
        _lblMessage.Text = "";

        if (string.IsNullOrWhiteSpace(_txtUtilisateur.Text))
        {
            _lblMessage.Text = "Le nom d'utilisateur est obligatoire.";
            _txtUtilisateur.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtMotDePasse.Text))
        {
            _lblMessage.Text = "Le mot de passe est obligatoire.";
            _txtMotDePasse.Focus();
            return;
        }

        try
        {
            if (_modeCreation)
                CreerCompte();
            else
                Connecter();
        }
        catch (Exception ex)
        {
            _lblMessage.Text = "Erreur : " + ex.Message;
        }
    }

    private void Connecter()
    {
        var u = Database.Authentifier(_txtUtilisateur.Text.Trim(), _txtMotDePasse.Text);
        if (u == null)
        {
            _lblMessage.Text = "Nom d'utilisateur ou mot de passe incorrect.";
            _txtMotDePasse.Clear();
            _txtMotDePasse.Focus();
            return;
        }

        UtilisateurConnecte = u;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void CreerCompte()
    {
        string nom = _txtUtilisateur.Text.Trim();
        string mdp = _txtMotDePasse.Text;
        string confirm = _txtMotDePasseConfirm.Text;
        string complet = _txtNomComplet.Text.Trim();

        if (string.IsNullOrWhiteSpace(complet))
        {
            _lblMessage.Text = "Le nom complet est obligatoire.";
            _txtNomComplet.Focus();
            return;
        }

        if (mdp.Length < 4)
        {
            _lblMessage.Text = "Le mot de passe doit contenir au moins 4 caractères.";
            _txtMotDePasse.Focus();
            return;
        }

        if (mdp != confirm)
        {
            _lblMessage.Text = "Les deux mots de passe ne correspondent pas.";
            _txtMotDePasseConfirm.Clear();
            _txtMotDePasseConfirm.Focus();
            return;
        }

        if (Database.NomUtilisateurExiste(nom))
        {
            _lblMessage.Text = "Ce nom d'utilisateur est déjà pris.";
            _txtUtilisateur.Focus();
            return;
        }

        int id = Database.CreerUtilisateur(nom, mdp, complet);

        UtilisateurConnecte = new Utilisateur
        {
            Id = id,
            NomUtilisateur = nom,
            NomComplet = complet,
            Role = "operateur"
        };

        MessageBox.Show(
            "Compte créé avec succès. Bienvenue !",
            "Bienvenue", MessageBoxButtons.OK, MessageBoxIcon.Information);

        DialogResult = DialogResult.OK;
        Close();
    }
}