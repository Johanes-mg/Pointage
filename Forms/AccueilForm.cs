using Pointage.Services;

namespace Pointage.Forms;

public class AccueilForm : Form
{
    private Label _lblJour = null!;
    private Label _lblHeure = null!;
    private Label _lblDate = null!;
    private System.Windows.Forms.Timer _timer = null!;

    public AccueilForm()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Fond;
        Icon = Theme.ChargerIcone("logo.ico");

        ConstruireInterface();
        MettreAJour();
        DemarrerTimer();
    }

    private void ConstruireInterface()
    {
        var grille = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.Fond
        };
        grille.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        grille.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        grille.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grille.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        var bloc = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 4,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Anchor = AnchorStyles.None,
            BackColor = Theme.Fond
        };
        bloc.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bloc.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        bloc.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        bloc.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        bloc.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _lblJour = new Label
        {
            Text = "Jeudi",
            Font = new Font(Theme.FamillePolice, 28F, FontStyle.Bold),
            ForeColor = Theme.Primaire,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 0, 0, 5)
        };

        _lblHeure = new Label
        {
            Text = "00:00:00",
            Font = new Font(Theme.FamillePolice, 72F, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 0, 0, 10)
        };

        _lblDate = new Label
        {
            Text = "1 janvier 2026",
            Font = new Font(Theme.FamillePolice, 18F, FontStyle.Regular),
            ForeColor = Theme.TexteSecondaire,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 0, 0, 25)
        };

        var btnExport = Theme.CreerBoutonBlancArrondi("exporter.png", "Exporter en Excel", 32);
        btnExport.Width = 360;
        btnExport.Height = 70;
        btnExport.Font = new Font(Theme.FamillePolice, 14F, FontStyle.Bold);
        btnExport.Anchor = AnchorStyles.None;
        btnExport.Margin = new Padding(0);
        btnExport.Padding = new Padding(0);
        btnExport.ImageAlign = ContentAlignment.MiddleRight;
        btnExport.TextAlign = ContentAlignment.MiddleLeft;
        btnExport.TextImageRelation = TextImageRelation.TextBeforeImage;
        btnExport.Click += BtnExport_Click;

        bloc.Controls.Add(_lblJour, 0, 0);
        bloc.Controls.Add(_lblHeure, 0, 1);
        bloc.Controls.Add(_lblDate, 0, 2);
        bloc.Controls.Add(btnExport, 0, 3);

        grille.Controls.Add(bloc, 0, 1);

        Controls.Add(grille);
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        try
        {
            Cursor = Cursors.WaitCursor;

            var debut = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fin = DateTime.Today;

            string chemin = ExcelExport.ExporterTout();

            Cursor = Cursors.Default;

            var reponse = MessageBox.Show(
                $"Export réussi !\n\n" +
                $"Fichier : {Path.GetFileName(chemin)}\n" +
                $"Dossier : {Path.GetDirectoryName(chemin)}\n\n" +
                $"Voulez-vous ouvrir le dossier ?",
                "Export Excel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (reponse == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Path.GetDirectoryName(chemin),
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            Cursor = Cursors.Default;
            MessageBox.Show(
                "Erreur lors de l'export Excel :\n\n" + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DemarrerTimer()
    {
        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += (_, _) => MettreAJour();
        _timer.Start();
    }

    private void MettreAJour()
    {
        var maintenant = DateTime.Now;
        var cultureFr = new System.Globalization.CultureInfo("fr-FR");

        string jourBrut = maintenant.ToString("dddd", cultureFr);
        if (jourBrut.Length > 0)
        {
            _lblJour.Text = char.ToUpper(jourBrut[0]) + jourBrut.Substring(1).ToLower();
        }

        _lblHeure.Text = maintenant.ToString("HH:mm:ss");
        _lblDate.Text = maintenant.ToString("d MMMM yyyy", cultureFr);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        ArreterTimer();
        base.OnFormClosed(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            ArreterTimer();
        base.Dispose(disposing);
    }

    private void ArreterTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= null;
            _timer.Dispose();
            _timer = null!;
        }
    }
}