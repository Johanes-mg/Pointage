using Pointage.Forms;
using Pointage.Models;
using Pointage.Services;

namespace Pointage;

public class MainForm : Form
{
    private Panel _panelContenu = null!;
    private Panel _panelTop = null!;
    private FlowLayoutPanel _panelNav = null!;
    private Panel _panelCentreNav = null!;
    private Label _lblUtil = null!;
    private readonly Utilisateur _utilisateur;

    public MainForm(Utilisateur utilisateur)
    {
        _utilisateur = utilisateur;

        Text = "Pointage";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 650);
        BackColor = Theme.Fond;
        Font = Theme.TexteNormal;
        Icon = Theme.ChargerIcone("logo.ico");

        ConstruireTopbar();

        _panelContenu = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Fond };
        Controls.Add(_panelContenu);
        Controls.Add(_panelTop);

        Afficher(new AccueilForm(), null);
    }

    private void ConstruireTopbar()
    {
        _panelTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Theme.Primaire
        };

        var panelGauche = new Panel
        {
            Dock = DockStyle.Left,
            Width = 320,
            BackColor = Theme.Primaire
        };

        var logo = Theme.ChargerImageRedimensionnee("logo.png", 44, 44);
        if (logo != null)
        {
            panelGauche.Controls.Add(new PictureBox
            {
                Image = logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 44,
                Height = 44,
                Left = 15,
                Top = 18
            });
        }

        var lblTitre = new Label
        {
            Text = "Pointage",
            Font = new Font(Theme.FamillePolice, 13F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(72, 14),
            AutoSize = false,
            Width = 230,
            Height = 24,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        panelGauche.Controls.Add(lblTitre);

        _lblUtil = new Label
        {
            Text = _utilisateur.NomComplet,
            Font = new Font(Theme.FamillePolice, 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(200, 215, 235),
            Location = new Point(72, 42),
            AutoSize = false,
            Width = 230,
            Height = 20,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        panelGauche.Controls.Add(_lblUtil);

        _panelTop.Controls.Add(panelGauche);

        var panelDroite = new Panel
        {
            Dock = DockStyle.Right,
            Width = 70,
            BackColor = Theme.Primaire
        };

        var btnDeconnexion = new Button
        {
            Width = 50,
            Height = 50,
            Location = new Point(10, 15),
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Primaire,
            Cursor = Cursors.Hand
        };
        btnDeconnexion.FlatAppearance.BorderSize = 0;
        btnDeconnexion.FlatAppearance.MouseOverBackColor = Theme.PrimaireClair;

        var imgDeco = Theme.ChargerImageRedimensionnee("deconnecter.png", 30, 30);
        if (imgDeco != null)
        {
            btnDeconnexion.Image = imgDeco;
            btnDeconnexion.ImageAlign = ContentAlignment.MiddleCenter;
        }
        else
        {
            btnDeconnexion.Text = "X";
            btnDeconnexion.ForeColor = Color.White;
            btnDeconnexion.Font = new Font(Theme.FamillePolice, 12F, FontStyle.Bold);
        }

        btnDeconnexion.Click += (_, _) =>
        {
            var r = MessageBox.Show("Se déconnecter ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            Hide();
            using var login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK && login.UtilisateurConnecte != null)
            {
                Application.Restart();
            }
            else
            {
                Application.Exit();
            }
        };
        panelDroite.Controls.Add(btnDeconnexion);
        _panelTop.Controls.Add(panelDroite);

        _panelCentreNav = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Primaire
        };

        _panelNav = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Theme.Primaire,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        var btnAccueil = CreerBoutonNav("accueil.png", "Accueil");
        var btnPersonnel = CreerBoutonNav("personnel.png", "Personnel");
        var btnPointage = CreerBoutonNav("pointage.png", "Pointage");
        var btnBilan = CreerBoutonNav("bilan.png", "Bilan");
        var btnCompte = CreerBoutonNav("compte.png", "Compte");

        btnAccueil.Click += (_, _) => Afficher(new AccueilForm(), btnAccueil);
        btnPersonnel.Click += (_, _) => Afficher(new PersonnelForm(), btnPersonnel);
        btnPointage.Click += (_, _) => Afficher(new PointageForm(), btnPointage);
        btnBilan.Click += (_, _) => Afficher(new BilanForm(), btnBilan);
        btnCompte.Click += (_, _) => Afficher(new CompteForm(_utilisateur), btnCompte);

        _panelNav.Controls.AddRange(new Control[]
        {
            btnAccueil, btnPersonnel, btnPointage, btnBilan, btnCompte
        });

        _panelCentreNav.Controls.Add(_panelNav);

        _panelCentreNav.Resize += (_, _) => CentrerNav();
        _panelCentreNav.Layout += (_, _) => CentrerNav();
        _panelNav.SizeChanged += (_, _) => CentrerNav();
        _panelCentreNav.HandleCreated += (_, _) => CentrerNav();

        _panelTop.Controls.Add(_panelCentreNav);

        MarquerActif(btnAccueil);
    }

    private void CentrerNav()
    {
        if (_panelNav == null || _panelCentreNav == null) return;
        if (_panelNav.Width <= 0 || _panelCentreNav.ClientSize.Width <= 0) return;

        int x = (_panelCentreNav.ClientSize.Width - _panelNav.Width) / 2;
        int y = (_panelCentreNav.ClientSize.Height - _panelNav.Height) / 2;

        if (x < 0) x = 0;
        if (y < 0) y = 0;

        _panelNav.Location = new Point(x, y);
    }

    private Button CreerBoutonNav(string fichierImage, string texte)
    {
        const int largeurBouton = 140;
        const int hauteurBouton = 44;

        var btn = new Button
        {
            Text = texte,
            Width = largeurBouton,
            Height = hauteurBouton,
            Margin = new Padding(4, 0, 4, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Primaire,
            ForeColor = Color.White,
            Font = new Font(Theme.FamillePolice, 11F, FontStyle.Regular),
            Cursor = Cursors.Hand,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(8, 0, 8, 0),
            UseVisualStyleBackColor = false
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Theme.PrimaireClair;
        btn.FlatAppearance.MouseDownBackColor = Theme.Accent;

        var img = Theme.ChargerImageRedimensionnee(fichierImage, 20, 20);
        if (img != null)
        {
            btn.Image = img;
            btn.ImageAlign = ContentAlignment.MiddleLeft;
        }

        return btn;
    }

    private void MarquerActif(Button btn)
    {
        foreach (Control c in _panelNav.Controls)
        {
            if (c is Button b)
                b.BackColor = Theme.Primaire;
        }
        btn.BackColor = Theme.Accent;
    }

    private void Afficher(Form page, Button? source)
    {
        _panelContenu.Controls.Clear();
        page.TopLevel = false;
        page.FormBorderStyle = FormBorderStyle.None;
        page.Dock = DockStyle.Fill;
        _panelContenu.Controls.Add(page);
        page.Show();

        if (source != null)
            MarquerActif(source);
    }
}