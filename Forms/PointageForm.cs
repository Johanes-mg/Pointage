using Pointage.Models;
using Pointage.Services;

namespace Pointage.Forms;

public class PointageForm : Form
{
    private DateTime _moisAffiche;
    private DateTime _dateSelectionnee;
    private Label _lblTitre = null!;
    private FlowLayoutPanel _panelJours = null!;
    private Panel _panelDetail = null!;
    private FlowLayoutPanel _flowPersonnes = null!;

    public PointageForm()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Fond;
        Icon = Theme.ChargerIcone("logo.ico");

        var aujourdhui = DateTime.Today;
        _moisAffiche = new DateTime(aujourdhui.Year, aujourdhui.Month, 1);
        _dateSelectionnee = aujourdhui;

        ConstruireInterface();
        RafraichirCalendrier();
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
            Text = "Pointage",
            Font = new Font(Theme.FamillePolice, 18F, FontStyle.Bold),
            ForeColor = Theme.Primaire,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var panelNav = new Panel
        {
            Dock = DockStyle.Top,
            Height = 90,
            BackColor = Theme.FondCarte
        };

        var btnPrecAnnee = Theme.CreerBoutonBlancIconeSeule("annee_gauche.png", 44, 26);
        btnPrecAnnee.Location = new Point(40, 23);
        btnPrecAnnee.Click += (_, _) => { _moisAffiche = _moisAffiche.AddYears(-1); RafraichirCalendrier(); };

        var btnPrecMois = Theme.CreerBoutonBlancIconeSeule("mois_gauche.png", 44, 26);
        btnPrecMois.Location = new Point(96, 23);
        btnPrecMois.Click += (_, _) => { _moisAffiche = _moisAffiche.AddMonths(-1); RafraichirCalendrier(); };

        var btnSuivMois = Theme.CreerBoutonBlancIconeSeule("mois_droite.png", 44, 26);
        btnSuivMois.Location = new Point(152, 23);
        btnSuivMois.Click += (_, _) => { _moisAffiche = _moisAffiche.AddMonths(1); RafraichirCalendrier(); };

        var btnSuivAnnee = Theme.CreerBoutonBlancIconeSeule("annee_droite.png", 44, 26);
        btnSuivAnnee.Location = new Point(208, 23);
        btnSuivAnnee.Click += (_, _) => { _moisAffiche = _moisAffiche.AddYears(1); RafraichirCalendrier(); };

        var btnAujourdhui = Theme.CreerBoutonBlancArrondiTexte("Aujourd'hui");
        btnAujourdhui.Location = new Point(280, 23);
        btnAujourdhui.Width = 140;
        btnAujourdhui.Height = 44;
        btnAujourdhui.Click += (_, _) =>
        {
            var t = DateTime.Today;
            _moisAffiche = new DateTime(t.Year, t.Month, 1);
            _dateSelectionnee = t;
            RafraichirCalendrier();
        };

        _lblTitre = new Label
        {
            Location = new Point(450, 32),
            AutoSize = true,
            Font = new Font(Theme.FamillePolice, 17F, FontStyle.Bold),
            ForeColor = Theme.Primaire
        };

        panelNav.Controls.AddRange(new Control[]
        {
            btnPrecAnnee, btnPrecMois, btnSuivMois, btnSuivAnnee, btnAujourdhui, _lblTitre
        });

        var corps = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40, 20, 40, 30),
            BackColor = Theme.Fond
        };

        _panelJours = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 660,
            AutoScroll = true,
            Padding = new Padding(25),
            BackColor = Theme.FondCarte
        };

        _panelDetail = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.FondCarte,
            AutoScroll = true,
            Padding = new Padding(30)
        };

        _flowPersonnes = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0)
        };

        _panelDetail.Controls.Add(_flowPersonnes);

        corps.Controls.Add(_panelDetail);
        corps.Controls.Add(_panelJours);

        Controls.Add(corps);
        Controls.Add(panelNav);
        Controls.Add(panelEntete);
    }

    private void RafraichirCalendrier()
    {
        try
        {
            _lblTitre.Text = _moisAffiche
                .ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
                .ToUpper();

            _panelJours.Controls.Clear();

            var stats = Database.GetStatistiquesMois(_moisAffiche.Year, _moisAffiche.Month);
            int nbJours = DateTime.DaysInMonth(_moisAffiche.Year, _moisAffiche.Month);

            for (int jour = 1; jour <= nbJours; jour++)
            {
                var date = new DateTime(_moisAffiche.Year, _moisAffiche.Month, jour);
                int nb = stats.TryGetValue(jour, out var v) ? v : 0;
                bool estAujourdhui = date.Date == DateTime.Today;
                bool estSelectionnee = date.Date == _dateSelectionnee.Date;

                var couleurFond = estSelectionnee
                    ? Theme.Accent
                    : (estAujourdhui ? Theme.SelectionJour : Theme.Fond);

                var carte = new Panel
                {
                    Width = 120,
                    Height = 120,
                    Margin = new Padding(7),
                    BackColor = couleurFond,
                    Cursor = Cursors.Hand,
                    Tag = date
                };

                var lblJour = new Label
                {
                    Text = jour.ToString(),
                    Font = new Font(Theme.FamillePolice, 22F, FontStyle.Bold),
                    ForeColor = estSelectionnee ? Color.White
                        : (estAujourdhui ? Theme.Accent : Theme.Primaire),
                    Location = new Point(12, 12),
                    AutoSize = true,
                    Cursor = Cursors.Hand,
                    Tag = date,
                    BackColor = Color.Transparent
                };

                var lblInfo = new Label
                {
                    Text = nb == 0 ? "aucun" : $"{nb} pointé",
                    Font = Theme.Petit,
                    ForeColor = estSelectionnee ? Color.White
                        : (nb == 0 ? Theme.TexteSecondaire : Theme.Succes),
                    Location = new Point(12, 78),
                    AutoSize = true,
                    Cursor = Cursors.Hand,
                    Tag = date,
                    BackColor = Color.Transparent
                };

                EventHandler clic = (s, _) =>
                {
                    if (s is Control c && c.Tag is DateTime d)
                    {
                        _dateSelectionnee = d;
                        RafraichirCalendrier();
                    }
                };
                carte.Click += clic;
                lblJour.Click += clic;
                lblInfo.Click += clic;

                carte.Controls.Add(lblJour);
                carte.Controls.Add(lblInfo);
                _panelJours.Controls.Add(carte);
            }

            RafraichirDetail();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur de chargement du calendrier : " + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RafraichirDetail()
    {
        _flowPersonnes.Controls.Clear();
        _flowPersonnes.SuspendLayout();

        try
        {
            var panelInfo = new Panel
            {
                Width = _flowPersonnes.ClientSize.Width - 20,
                Height = 170,
                BackColor = Theme.FondCarte,
                Margin = new Padding(0, 0, 0, 15)
            };

            var lblDate = new Label
            {
                Text = _dateSelectionnee.ToString(
                    "dddd dd MMMM yyyy",
                    new System.Globalization.CultureInfo("fr-FR")),
                Font = new Font(Theme.FamillePolice, 16F, FontStyle.Bold),
                ForeColor = Theme.Primaire,
                Location = new Point(0, 5),
                AutoSize = true
            };
            panelInfo.Controls.Add(lblDate);

            var sep = new Panel
            {
                Location = new Point(0, 45),
                Height = 1,
                Width = panelInfo.Width,
                BackColor = Theme.Bordure,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panelInfo.Controls.Add(sep);

            var personnes = Database.GetPersonnel();
            var pointages = Database.GetPointagesDuJour(_dateSelectionnee);

            int total = personnes.Count;
            int presents = pointages.Count;
            int absents = Math.Max(0, total - presents);
            double taux = total > 0 ? (presents * 100.0 / total) : 0;

            var panelBilan = new Panel
            {
                Location = new Point(0, 60),
                Height = 90,
                Width = panelInfo.Width,
                BackColor = Theme.FondBilan,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            panelBilan.Controls.Add(new Label
            {
                Text = "BILAN DU JOUR",
                Font = Theme.PetitGras,
                ForeColor = Theme.TexteSecondaire,
                Location = new Point(25, 12),
                AutoSize = true
            });
            panelBilan.Controls.Add(CreerLabelBilan($"Présents : {presents}", Theme.Succes, 25, 42));
            panelBilan.Controls.Add(CreerLabelBilan($"Absents : {absents}", Theme.Danger, 230, 42));
            panelBilan.Controls.Add(CreerLabelBilan($"Total : {total}", Theme.Texte, 435, 42));
            panelBilan.Controls.Add(CreerLabelBilan($"Taux : {taux:F0} %", Theme.Primaire, 640, 42));

            panelInfo.Controls.Add(panelBilan);

            _flowPersonnes.Controls.Add(panelInfo);

            if (total == 0)
            {
                _flowPersonnes.Controls.Add(new Label
                {
                    Text = "Aucun personnel enregistré. Ajoutez-en dans la page Personnel.",
                    AutoSize = true,
                    ForeColor = Theme.TexteSecondaire,
                    Font = Theme.TexteNormal,
                    Margin = new Padding(0, 10, 0, 0)
                });
                return;
            }

            var existants = pointages.ToDictionary(p => p.PersonnelId);

            foreach (var p in personnes)
            {
                bool estPointe = existants.ContainsKey(p.Id);

                // Ligne de chaque personne
                var ligne = new Panel
                {
                    Width = _flowPersonnes.ClientSize.Width - 20,
                    Height = 210,
                    BackColor = estPointe ? Theme.FondPointage : Theme.Fond,
                    Margin = new Padding(0, 0, 0, 12)
                };

                // Nom
                var lblNom = new Label
                {
                    Text = p.NomComplet,
                    Location = new Point(20, 10),
                    Width = ligne.Width - 40,
                    Height = 28,
                    Font = new Font(Theme.FamillePolice, 13F, FontStyle.Bold),
                    ForeColor = Theme.Texte,
                    AutoSize = false
                };
                ligne.Controls.Add(lblNom);

                // ===== BLOC MATIN =====
                var panelMatin = new Panel
                {
                    Location = new Point(20, 45),
                    Width = 380,
                    Height = 110,
                    BackColor = Color.FromArgb(245, 250, 255)
                };

                panelMatin.Controls.Add(new Label
                {
                    Text = "MATIN",
                    Location = new Point(10, 5),
                    AutoSize = true,
                    Font = Theme.PetitGras,
                    ForeColor = Theme.Primaire
                });

                var chkMatin = new CheckBox
                {
                    Text = "Pointer",
                    Location = new Point(290, 5),
                    AutoSize = true,
                    Font = Theme.PetitGras,
                    ForeColor = Theme.Accent,
                    Checked = estPointe && existants[p.Id].MatinRempli
                };
                panelMatin.Controls.Add(chkMatin);

                panelMatin.Controls.Add(new Label
                {
                    Text = "Arrivée",
                    Location = new Point(10, 35),
                    AutoSize = true,
                    Font = Theme.Petit,
                    ForeColor = Theme.TexteSecondaire
                });
                var dtMA = new DateTimePicker
                {
                    Location = new Point(10, 55),
                    Width = 130,
                    Height = 32,
                    Format = DateTimePickerFormat.Time,
                    ShowUpDown = true,
                    Font = Theme.TexteNormal
                };
                panelMatin.Controls.Add(dtMA);

                panelMatin.Controls.Add(new Label
                {
                    Text = "Sortie",
                    Location = new Point(160, 35),
                    AutoSize = true,
                    Font = Theme.Petit,
                    ForeColor = Theme.TexteSecondaire
                });
                var dtMS = new DateTimePicker
                {
                    Location = new Point(160, 55),
                    Width = 130,
                    Height = 32,
                    Format = DateTimePickerFormat.Time,
                    ShowUpDown = true,
                    Font = Theme.TexteNormal
                };
                panelMatin.Controls.Add(dtMS);

                ligne.Controls.Add(panelMatin);

                // ===== BLOC APRES-MIDI =====
                var panelAprem = new Panel
                {
                    Location = new Point(410, 45),
                    Width = 380,
                    Height = 110,
                    BackColor = Color.FromArgb(255, 250, 245)
                };

                panelAprem.Controls.Add(new Label
                {
                    Text = "APRÈS-MIDI",
                    Location = new Point(10, 5),
                    AutoSize = true,
                    Font = Theme.PetitGras,
                    ForeColor = Theme.Primaire
                });

                var chkAprem = new CheckBox
                {
                    Text = "Pointer",
                    Location = new Point(290, 5),
                    AutoSize = true,
                    Font = Theme.PetitGras,
                    ForeColor = Theme.Accent,
                    Checked = estPointe && existants[p.Id].ApresMidiRempli
                };
                panelAprem.Controls.Add(chkAprem);

                panelAprem.Controls.Add(new Label
                {
                    Text = "Arrivée",
                    Location = new Point(10, 35),
                    AutoSize = true,
                    Font = Theme.Petit,
                    ForeColor = Theme.TexteSecondaire
                });
                var dtAA = new DateTimePicker
                {
                    Location = new Point(10, 55),
                    Width = 130,
                    Height = 32,
                    Format = DateTimePickerFormat.Time,
                    ShowUpDown = true,
                    Font = Theme.TexteNormal
                };
                panelAprem.Controls.Add(dtAA);

                panelAprem.Controls.Add(new Label
                {
                    Text = "Sortie",
                    Location = new Point(160, 35),
                    AutoSize = true,
                    Font = Theme.Petit,
                    ForeColor = Theme.TexteSecondaire
                });
                var dtAS = new DateTimePicker
                {
                    Location = new Point(160, 55),
                    Width = 130,
                    Height = 32,
                    Format = DateTimePickerFormat.Time,
                    ShowUpDown = true,
                    Font = Theme.TexteNormal
                };
                panelAprem.Controls.Add(dtAS);

                ligne.Controls.Add(panelAprem);

                // ===== OBSERVATION =====
                var lblObs = new Label
                {
                    Text = "Observation",
                    Location = new Point(20, 165),
                    AutoSize = true,
                    Font = Theme.Petit,
                    ForeColor = Theme.TexteSecondaire
                };
                ligne.Controls.Add(lblObs);

                var txtObs = new TextBox
                {
                    Location = new Point(110, 162),
                    Width = 480,
                    Height = 28,
                    Font = Theme.TexteNormal,
                    BorderStyle = BorderStyle.FixedSingle
                };
                ligne.Controls.Add(txtObs);

                // Charger valeurs existantes
                if (estPointe)
                {
                    var pt = existants[p.Id];

                    if (pt.MatinArrivee.HasValue)
                        dtMA.Value = DateTime.Today.Add(pt.MatinArrivee.Value);
                    if (pt.MatinSortie.HasValue)
                        dtMS.Value = DateTime.Today.Add(pt.MatinSortie.Value);
                    if (pt.ApresMidiArrivee.HasValue)
                        dtAA.Value = DateTime.Today.Add(pt.ApresMidiArrivee.Value);
                    if (pt.ApresMidiSortie.HasValue)
                        dtAS.Value = DateTime.Today.Add(pt.ApresMidiSortie.Value);

                    txtObs.Text = pt.Observation ?? "";
                }
                else
                {
                    dtMA.Value = DateTime.Today.AddHours(8);
                    dtMS.Value = DateTime.Today.AddHours(12);
                    dtAA.Value = DateTime.Today.AddHours(14);
                    dtAS.Value = DateTime.Today.AddHours(17);
                }

                // ===== BOUTON INTERACTIF =====
                var btnToggle = Theme.CreerBoutonBlancIconeSeule(
                    estPointe ? "non-enregistrer.png" : "enregistrer.png", 55, 32);
                btnToggle.Location = new Point(610, 162);

                int pid = p.Id;
                DateTime dateCible = _dateSelectionnee;
                string nomComplet = p.NomComplet;
                bool etaitPointe = estPointe;

                btnToggle.Click += (sender, ev) =>
                {
                    try
                    {
                        if (etaitPointe)
                        {
                            var rep = MessageBox.Show(
                                $"Supprimer le pointage de {nomComplet} pour le {dateCible:dd/MM/yyyy} ?",
                                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (rep != DialogResult.Yes) return;

                            Database.SupprimerPointage(pid, dateCible);
                        }
                        else
                        {
                            TimeSpan? matinArr = chkMatin.Checked ? dtMA.Value.TimeOfDay : null;
                            TimeSpan? matinSor = chkMatin.Checked ? dtMS.Value.TimeOfDay : null;
                            TimeSpan? apremArr = chkAprem.Checked ? dtAA.Value.TimeOfDay : null;
                            TimeSpan? apremSor = chkAprem.Checked ? dtAS.Value.TimeOfDay : null;
                            var obs = string.IsNullOrWhiteSpace(txtObs.Text) ? null : txtObs.Text.Trim();

                            if (matinArr == null && apremArr == null)
                            {
                                MessageBox.Show(
                                    "Veuillez cocher au moins 'Pointer' pour le matin ou l'après-midi.",
                                    "Aucune plage sélectionnée",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            Database.EnregistrerPointage(
                                pid, dateCible,
                                matinArr, matinSor,
                                apremArr, apremSor,
                                obs);
                        }

                        RafraichirCalendrier();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur : " + ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                ligne.Controls.Add(btnToggle);

                _flowPersonnes.Controls.Add(ligne);
            }
        }
        finally
        {
            _flowPersonnes.ResumeLayout();
        }
    }

    private Label CreerLabelBilan(string texte, Color couleur, int x, int y)
    {
        return new Label
        {
            Text = texte,
            Location = new Point(x, y),
            AutoSize = true,
            Font = Theme.MoyenGras,
            ForeColor = couleur
        };
    }
}