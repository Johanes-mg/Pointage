using Pointage.Services;

namespace Pointage.Forms;

public class ExportDialogForm : Form
{
    private RadioButton _radAujourdhui = null!;
    private RadioButton _radPeriode = null!;
    private DateTimePicker _dtDebut = null!;
    private DateTimePicker _dtFin = null!;
    private Label _lblErreur = null!;

    public DateTime DateDebut { get; private set; }
    public DateTime DateFin { get; private set; }
    public bool ExportAnnule { get; private set; } = true;

    public ExportDialogForm()
    {
        Text = "Exporter en Excel";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 360);
        BackColor = Theme.FondCarte;
        Font = Theme.TexteNormal;
        Icon = Theme.ChargerIcone("logo.ico");

        ConstruireInterface();
    }

    private void ConstruireInterface()
    {
        var panelTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 90,
            BackColor = Theme.Primaire,
            Padding = new Padding(20)
        };

        panelTop.Controls.Add(new Label
        {
            Text = "Exporter en Excel",
            Font = new Font(Theme.FamillePolice, 18F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 20),
            AutoSize = true
        });

        panelTop.Controls.Add(new Label
        {
            Text = "Choisissez la période à exporter",
            Font = new Font(Theme.FamillePolice, 11F, FontStyle.Regular),
            ForeColor = Color.FromArgb(220, 230, 245),
            Location = new Point(20, 52),
            AutoSize = true
        });

        Controls.Add(panelTop);

        _radAujourdhui = new RadioButton
        {
            Text = "Aujourd'hui seulement",
            Font = new Font(Theme.FamillePolice, 12F, FontStyle.Regular),
            ForeColor = Theme.Texte,
            Location = new Point(40, 130),
            AutoSize = true,
            Checked = true
        };
        _radAujourdhui.CheckedChanged += (_, _) => MettreAJourEtat();
        Controls.Add(_radAujourdhui);

        _radPeriode = new RadioButton
        {
            Text = "Période personnalisée",
            Font = new Font(Theme.FamillePolice, 12F, FontStyle.Regular),
            ForeColor = Theme.Texte,
            Location = new Point(40, 175),
            AutoSize = true
        };
        _radPeriode.CheckedChanged += (_, _) => MettreAJourEtat();
        Controls.Add(_radPeriode);

        var lblDebut = new Label
        {
            Text = "Du",
            Font = Theme.PetitGras,
            ForeColor = Theme.TexteSecondaire,
            Location = new Point(70, 220),
            AutoSize = true
        };
        Controls.Add(lblDebut);

        _dtDebut = new DateTimePicker
        {
            Location = new Point(120, 217),
            Width = 140,
            Format = DateTimePickerFormat.Short,
            Font = Theme.TexteNormal,
            Enabled = false,
            Value = DateTime.Today
        };
        Controls.Add(_dtDebut);

        var lblFin = new Label
        {
            Text = "Au",
            Font = Theme.PetitGras,
            ForeColor = Theme.TexteSecondaire,
            Location = new Point(280, 220),
            AutoSize = true
        };
        Controls.Add(lblFin);

        _dtFin = new DateTimePicker
        {
            Location = new Point(320, 217),
            Width = 140,
            Format = DateTimePickerFormat.Short,
            Font = Theme.TexteNormal,
            Enabled = false,
            Value = DateTime.Today
        };
        Controls.Add(_dtFin);

        _lblErreur = new Label
        {
            Location = new Point(40, 255),
            Width = 400,
            Height = 30,
            Font = Theme.Petit,
            ForeColor = Theme.Danger,
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(_lblErreur);

        var btnAnnuler = new Button
        {
            Text = "Annuler",
            Location = new Point(160, 300),
            Width = 130,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Theme.Texte,
            Font = Theme.Bouton,
            Cursor = Cursors.Hand
        };
        btnAnnuler.FlatAppearance.BorderColor = Theme.Bordure;
        btnAnnuler.FlatAppearance.BorderSize = 1;
        btnAnnuler.Click += (_, _) =>
        {
            ExportAnnule = true;
            Close();
        };
        Controls.Add(btnAnnuler);

        var btnExporter = new Button
        {
            Text = "Exporter",
            Location = new Point(305, 300),
            Width = 130,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Accent,
            ForeColor = Color.White,
            Font = new Font(Theme.FamillePolice, 11F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnExporter.FlatAppearance.BorderSize = 0;
        btnExporter.Click += BtnExporter_Click;
        Controls.Add(btnExporter);
    }

    private void MettreAJourEtat()
    {
        bool periodeActive = _radPeriode.Checked;

        _dtDebut.Enabled = periodeActive;
        _dtFin.Enabled = periodeActive;
        _lblErreur.Text = "";
    }

    private void BtnExporter_Click(object? sender, EventArgs e)
    {
        if (_radAujourdhui.Checked)
        {
            DateDebut = DateTime.Today;
            DateFin = DateTime.Today;
        }
        else
        {
            if (_dtFin.Value.Date < _dtDebut.Value.Date)
            {
                _lblErreur.Text = "La date de fin doit être postérieure ou égale à la date de début.";
                return;
            }

            DateDebut = _dtDebut.Value.Date;
            DateFin = _dtFin.Value.Date;
        }

        ExportAnnule = false;
        DialogResult = DialogResult.OK;
        Close();
    }
}