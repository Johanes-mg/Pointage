using Pointage.Models;
using Pointage.Services;

namespace Pointage.Forms;

public class BilanForm : Form
{
    private DateTime _moisAffiche;
    private Personnel? _personnelSelectionne;

    private ListBox _listePersonnel = null!;
    private Label _lblNomSelectionne = null!;
    private Label _lblTitre = null!;
    private FlowLayoutPanel _panelJours = null!;
    private Label _lblResumeMois = null!;
    private bool _enCoursDeSelection = false;
    private bool _ratioApplique = false;

    public BilanForm()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Fond;
        Icon = Theme.ChargerIcone("logo.ico");

        var maintenant = DateTime.Today;
        _moisAffiche = new DateTime(maintenant.Year, maintenant.Month, 1);

        ConstruireInterface();
        ChargerPersonnel();
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
            Text = "Bilan",
            Font = new Font(Theme.FamillePolice, 18F, FontStyle.Bold),
            ForeColor = Theme.Primaire,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BackColor = Theme.Fond
        };
        split.Panel1.Padding = new Padding(15, 15, 5, 20);
        split.Panel2.Padding = new Padding(5, 15, 20, 20);

        split.SizeChanged += (_, _) =>
        {
            if (!_ratioApplique && split.Width > 400)
            {
                try
                {
                    split.SplitterDistance = (int)(split.Width * 0.25);
                    _ratioApplique = true;
                }
                catch { }
            }
        };
        split.HandleCreated += (_, _) =>
        {
            try
            {
                split.SplitterDistance = (int)(split.Width * 0.25);
                _ratioApplique = true;
            }
            catch { }
        };

        var lblListeTitre = new Label
        {
            Text = "Personnel",
            Dock = DockStyle.Top,
            Height = 35,
            Font = new Font(Theme.FamillePolice, 13F, FontStyle.Bold),
            ForeColor = Theme.Primaire,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _listePersonnel = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font(Theme.FamillePolice, 11F, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Theme.FondCarte,
            ForeColor = Theme.Texte,
            ItemHeight = 42,
            IntegralHeight = false,
            DrawMode = DrawMode.OwnerDrawFixed,
            HorizontalScrollbar = true
        };
        _listePersonnel.DrawItem += ListePersonnel_DrawItem;
        _listePersonnel.SelectedIndexChanged += ListePersonnel_SelectedIndexChanged;

        split.Panel1.Controls.Add(_listePersonnel);
        split.Panel1.Controls.Add(lblListeTitre);

        var panelDroit = new Panel { Dock = DockStyle.Fill };

        _lblNomSelectionne = new Label
        {
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font(Theme.FamillePolice, 15F, FontStyle.Bold),
            ForeColor = Theme.Primaire,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "Sélectionnez un personnel à gauche"
        };

        var panelNav = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = Theme.FondCarte
        };

        var btnPrecAnnee = Theme.CreerBoutonBlancIconeSeule("annee_gauche.png", 38, 20);
        btnPrecAnnee.Location = new Point(10, 9);
        btnPrecAnnee.Click += (_, _) => { _moisAffiche = _moisAffiche.AddYears(-1); RafraichirCalendrier(); };

        var btnPrecMois = Theme.CreerBoutonBlancIconeSeule("mois_gauche.png", 38, 20);
        btnPrecMois.Location = new Point(55, 9);
        btnPrecMois.Click += (_, _) => { _moisAffiche = _moisAffiche.AddMonths(-1); RafraichirCalendrier(); };

        var btnSuivMois = Theme.CreerBoutonBlancIconeSeule("mois_droite.png", 38, 20);
        btnSuivMois.Location = new Point(100, 9);
        btnSuivMois.Click += (_, _) => { _moisAffiche = _moisAffiche.AddMonths(1); RafraichirCalendrier(); };

        var btnSuivAnnee = Theme.CreerBoutonBlancIconeSeule("annee_droite.png", 38, 20);
        btnSuivAnnee.Location = new Point(145, 9);
        btnSuivAnnee.Click += (_, _) => { _moisAffiche = _moisAffiche.AddYears(1); RafraichirCalendrier(); };

        _lblTitre = new Label
        {
            Location = new Point(200, 18),
            AutoSize = true,
            Font = new Font(Theme.FamillePolice, 14F, FontStyle.Bold),
            ForeColor = Theme.Primaire
        };

        panelNav.Controls.AddRange(new Control[]
        {
            btnPrecAnnee, btnPrecMois, btnSuivMois, btnSuivAnnee, _lblTitre
        });

        _lblResumeMois = new Label
        {
            Dock = DockStyle.Top,
            Height = 35,
            Font = Theme.MoyenGras,
            ForeColor = Theme.Texte,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };

        _panelJours = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(10),
            BackColor = Theme.FondCarte
        };

        panelDroit.Controls.Add(_panelJours);
        panelDroit.Controls.Add(_lblResumeMois);
        panelDroit.Controls.Add(panelNav);
        panelDroit.Controls.Add(_lblNomSelectionne);

        split.Panel2.Controls.Add(panelDroit);

        Controls.Add(split);
        Controls.Add(panelEntete);
    }

    private void ChargerPersonnel()
    {
        try
        {
            var liste = Database.GetPersonnel();

            _enCoursDeSelection = true;
            _listePersonnel.DataSource = null;
            _listePersonnel.DataSource = liste;
            _listePersonnel.DisplayMember = "NomComplet";
            _listePersonnel.ClearSelected();
            _enCoursDeSelection = false;

            _personnelSelectionne = null;
            _lblNomSelectionne.Text = "Sélectionnez un personnel à gauche";
            RafraichirCalendrier();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur de chargement : " + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ListePersonnel_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var item = _listePersonnel.Items[e.Index] as Personnel;
        if (item == null) return;

        bool selectionne = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

        var fond = selectionne ? Theme.Accent : Theme.FondCarte;
        var texte = selectionne ? Color.White : Theme.Texte;

        using var brushFond = new SolidBrush(fond);
        e.Graphics.FillRectangle(brushFond, e.Bounds);

        using var brushTexte = new SolidBrush(texte);
        using var font = new Font(Theme.FamillePolice, 11F, FontStyle.Regular);

        var rect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 11,
            e.Bounds.Width - 20, e.Bounds.Height - 22);

        var format = new StringFormat
        {
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };

        e.Graphics.DrawString(item.NomComplet, font, brushTexte, rect, format);
    }

    private void ListePersonnel_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_enCoursDeSelection) return;

        _personnelSelectionne = _listePersonnel.SelectedItem as Personnel;
        if (_personnelSelectionne == null)
        {
            _lblNomSelectionne.Text = "Sélectionnez un personnel à gauche";
        }
        else
        {
            _lblNomSelectionne.Text = _personnelSelectionne.NomComplet;
        }
        RafraichirCalendrier();
    }

    private void RafraichirCalendrier()
    {
        try
        {
            _lblTitre.Text = _moisAffiche
                .ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
                .ToUpper();

            _panelJours.Controls.Clear();

            if (_personnelSelectionne == null)
            {
                _lblResumeMois.Text = "";
                return;
            }

            int nbJours = DateTime.DaysInMonth(_moisAffiche.Year, _moisAffiche.Month);

            var joursPointes = Database.GetJoursPointesDuMois(
                _personnelSelectionne.Id, _moisAffiche.Year, _moisAffiche.Month);

            _lblResumeMois.Text = $"Jours pointés ce mois : {joursPointes.Count} / {nbJours}";

            for (int jour = 1; jour <= nbJours; jour++)
            {
                var date = new DateTime(_moisAffiche.Year, _moisAffiche.Month, jour);
                bool estPointe = joursPointes.Contains(jour);
                bool estAujourdhui = date.Date == DateTime.Today;

                Color fond = estPointe
                    ? Color.FromArgb(200, 240, 210)
                    : (estAujourdhui ? Theme.SelectionJour : Theme.Fond);

                Color couleurJour = estPointe ? Theme.Succes : Theme.Primaire;

                var carte = new Panel
                {
                    Width = 105,
                    Height = 105,
                    Margin = new Padding(5),
                    BackColor = fond
                };

                var lblJour = new Label
                {
                    Text = jour.ToString(),
                    Font = new Font(Theme.FamillePolice, 20F, FontStyle.Bold),
                    ForeColor = couleurJour,
                    Location = new Point(10, 10),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                var lblStatut = new Label
                {
                    Text = estPointe ? "pointé" : "non pointé",
                    Font = Theme.Petit,
                    ForeColor = estPointe ? Theme.Succes : Theme.TexteSecondaire,
                    Location = new Point(10, 65),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                carte.Controls.Add(lblJour);
                carte.Controls.Add(lblStatut);
                _panelJours.Controls.Add(carte);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur de chargement du bilan : " + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}