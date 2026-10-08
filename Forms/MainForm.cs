using Pointage.Forms;
using Pointage.Models;
using Pointage.Services;

namespace Pointage;

public class MainForm : Form
{
    private Panel _panelContenu = null!;
    private Panel _panelTop = null!;
    private FlowLayoutPanel _panelNav = null!;
    private readonly Utilisateur _utilisateur;

    public MainForm(Utilisateur utilisateur)
    {
        _utilisateur = utilisateur;

        Text = "Pointage";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
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
            Height = 75,
            BackColor = Theme.Primaire
        };

        var logo = Theme.ChargerImageRedimensionnee("logo.png", 50, 50);
        if (logo != null)
        {
            _panelTop.Controls.Add(new PictureBox
            {
                Image = logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 50,
                Height = 50,
                Left = 25,
                Top = 12
            });
        }

        _panelNav = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Top = 17,
            Left = 100,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        var btnAccueil = CreerBoutonNav("accueil.png", "Accueil");
        var btnPersonnel = CreerBoutonNav("personnel.png", "Personnel");
        var btnPointage = CreerBoutonNav("pointage.png", "Pointage");
        var btnBilan = CreerBoutonNav("bilan.png", "Bilan");

        btnAccueil.Click += (_, _) => Afficher(new AccueilForm(), btnAccueil);
        btnPersonnel.Click += (_, _) => Afficher(new PersonnelForm(), btnPersonnel);
        btnPointage.Click += (_, _) => Afficher(new PointageForm(), btnPointage);
        btnBilan.Click += (_, _) => Afficher(new BilanForm(), btnBilan);

        _panelNav.Controls.AddRange(new Control[]
        {
            btnAccueil, btnPersonnel, btnPointage, btnBilan
        });

        _panelTop.Controls.Add(_panelNav);

        var lblUtil = new Label
        {
            Text = $"Connecté : {_utilisateur.NomComplet}",
            Font = new Font(Theme.FamillePolice, 10F, FontStyle.Regular),
            ForeColor = Color.White,
            AutoSize = true,
            Top = 26,
            Left = 0,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _panelTop.Controls.Add(lblUtil);

        var btnDeconnexion = new Button
        {
            Width = 48,
            Height = 48,
            Top = 14,
            Left = 0,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Primaire,
            Cursor = Cursors.Hand,
            TextImageRelation = TextImageRelation.ImageAboveText
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
            btnDeconnexion.Text = "Déco";
            btnDeconnexion.ForeColor = Color.White;
            btnDeconnexion.Font = new Font(Theme.FamillePolice, 8F);
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
        _panelTop.Controls.Add(btnDeconnexion);

        MarquerActif(btnAccueil);

        _panelTop.Resize += (_, _) =>
        {
            int droite = _panelTop.ClientSize.Width - 25;

            btnDeconnexion.Left = droite - btnDeconnexion.Width;

            int lblLeft = btnDeconnexion.Left - lblUtil.Width - 15;
            lblUtil.Left = Math.Max(100, lblLeft);

            int navLeft = lblUtil.Left - _panelNav.Width - 20;
            _panelNav.Left = Math.Max(100, navLeft);
        };
    }

    private Button CreerBoutonNav(string fichierImage, string texte)
    {
        var btn = new Button
        {
            Text = "  " + texte,
            Width = 150,
            Height = 40,
            Margin = new Padding(4, 0, 4, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Primaire,
            ForeColor = Color.White,
            Font = new Font(Theme.FamillePolice, 11F, FontStyle.Regular),
            Cursor = Cursors.Hand,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Theme.PrimaireClair;
        btn.FlatAppearance.MouseDownBackColor = Theme.Accent;

        var img = Theme.ChargerImageRedimensionnee(fichierImage, 22, 22);
        if (img != null)
            btn.Image = img;

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