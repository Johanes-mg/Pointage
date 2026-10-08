using Pointage.Models;
using Pointage.Services;

namespace Pointage.Forms;

public class PersonnelForm : Form
{
    private DataGridView _grid = null!;
    private TextBox _txtNom = null!;
    private TextBox _txtPrenom = null!;
    private Button _btnAjouter = null!;
    private Button _btnModifier = null!;
    private Button _btnSupprimer = null!;
    private int? _selectedId = null;
    private bool _enCoursDeDeselection = false;

    public PersonnelForm()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.Fond;
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
            Text = "Gestion du personnel",
            Font = new Font(Theme.FamillePolice, 18F, FontStyle.Bold),
            ForeColor = Theme.Primaire,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var panelForm = new Panel
        {
            Dock = DockStyle.Top,
            Height = 180,
            BackColor = Theme.FondCarte,
            Padding = new Padding(40, 25, 40, 25)
        };

        var lblNom = new Label
        {
            Text = "Nom",
            Font = Theme.PetitGras,
            ForeColor = Theme.TexteSecondaire,
            Location = new Point(40, 25),
            AutoSize = true
        };
        _txtNom = new TextBox
        {
            Location = new Point(40, 48),
            Width = 320,
            Height = 32,
            Font = Theme.TexteNormal,
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblPrenom = new Label
        {
            Text = "Prénom",
            Font = Theme.PetitGras,
            ForeColor = Theme.TexteSecondaire,
            Location = new Point(390, 25),
            AutoSize = true
        };
        _txtPrenom = new TextBox
        {
            Location = new Point(390, 48),
            Width = 320,
            Height = 32,
            Font = Theme.TexteNormal,
            BorderStyle = BorderStyle.FixedSingle
        };

        _btnAjouter = Theme.CreerBoutonBlancArrondi("ajouter.png", "Ajouter");
        _btnAjouter.Location = new Point(40, 115);

        _btnModifier = Theme.CreerBoutonBlancArrondi("modifier.png", "Modifier");
        _btnModifier.Location = new Point(225, 115);
        _btnModifier.Enabled = false;

        _btnSupprimer = Theme.CreerBoutonBlancArrondi("effacer.png", "Supprimer");
        _btnSupprimer.Location = new Point(410, 115);
        _btnSupprimer.Enabled = false;

        _btnAjouter.Click += BtnAjouter_Click;
        _btnModifier.Click += BtnModifier_Click;
        _btnSupprimer.Click += BtnSupprimer_Click;

        panelForm.Controls.AddRange(new Control[]
        {
            lblNom, _txtNom, lblPrenom, _txtPrenom,
            _btnAjouter, _btnModifier, _btnSupprimer
        });

        var panelGrille = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40, 15, 40, 30),
            BackColor = Theme.Fond
        };

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Theme.FondCarte,
            BorderStyle = BorderStyle.None,
            Font = new Font(Theme.FamillePolice, 12F, FontStyle.Regular),
            GridColor = Theme.Bordure,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 60,
            RowTemplate = { Height = 70 },
            AllowUserToResizeRows = false,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            AllowUserToResizeColumns = true
        };
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Primaire;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font(Theme.FamillePolice, 12F, FontStyle.Bold);
        _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(20, 0, 20, 0);
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Primaire;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Theme.LigneAlternee;
        _grid.DefaultCellStyle.SelectionBackColor = Theme.Accent;
        _grid.DefaultCellStyle.SelectionForeColor = Color.White;
        _grid.DefaultCellStyle.Padding = new Padding(20, 0, 20, 0);

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "Id", Visible = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Nom",
            HeaderText = "NOM",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 50
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Prenom",
            HeaderText = "PRÉNOM",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 50
        });

        _grid.DataBindingComplete += (_, _) => NettoyerSelection();
        _grid.CellClick += Grid_CellClick;

        panelGrille.Controls.Add(_grid);

        Controls.Add(panelGrille);
        Controls.Add(panelForm);
        Controls.Add(panelEntete);
    }

    private void NettoyerSelection()
    {
        _enCoursDeDeselection = true;
        _grid.ClearSelection();
        _grid.CurrentCell = null;
        _enCoursDeDeselection = false;
    }

    private void ChargerDonnees()
    {
        try
        {
            _grid.DataSource = null;
            _grid.DataSource = Database.GetPersonnel();

            NettoyerSelection();
            ViderChampsSansClear();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur de chargement : " + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (_enCoursDeDeselection) return;
        if (e.RowIndex < 0) return;

        var item = _grid.Rows[e.RowIndex].DataBoundItem as Personnel;
        if (item == null) return;

        if (_selectedId.HasValue && _selectedId.Value == item.Id)
        {
            ViderChamps();
        }
        else
        {
            _selectedId = item.Id;
            _txtNom.Text = item.Nom;
            _txtPrenom.Text = item.Prenom;
            _btnModifier.Enabled = true;
            _btnSupprimer.Enabled = true;
        }
    }

    private void BtnAjouter_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtNom.Text) || string.IsNullOrWhiteSpace(_txtPrenom.Text))
        {
            MessageBox.Show("Le nom et le prénom sont obligatoires.",
                "Champs requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Database.AjouterPersonnel(new Personnel
            {
                Nom = _txtNom.Text.Trim(),
                Prenom = _txtPrenom.Text.Trim()
            });

            ViderChamps();
            ChargerDonnees();
            _txtNom.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur lors de l'ajout : " + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnModifier_Click(object? sender, EventArgs e)
    {
        if (_selectedId == null)
        {
            MessageBox.Show("Veuillez d'abord sélectionner une personne.",
                "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtNom.Text) || string.IsNullOrWhiteSpace(_txtPrenom.Text))
        {
            MessageBox.Show("Le nom et le prénom sont obligatoires.",
                "Champs requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Database.ModifierPersonnel(new Personnel
            {
                Id = _selectedId.Value,
                Nom = _txtNom.Text.Trim(),
                Prenom = _txtPrenom.Text.Trim()
            });

            ViderChamps();
            ChargerDonnees();
            _txtNom.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur lors de la modification : " + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSupprimer_Click(object? sender, EventArgs e)
    {
        if (_selectedId == null)
        {
            MessageBox.Show("Veuillez d'abord sélectionner une personne.",
                "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var rep = MessageBox.Show(
            "Supprimer définitivement cette personne et tous ses pointages ?",
            "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (rep != DialogResult.Yes) return;

        try
        {
            Database.SupprimerPersonnel(_selectedId.Value);
            ViderChamps();
            ChargerDonnees();
            _txtNom.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur lors de la suppression : " + ex.Message,
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ViderChamps()
    {
        _enCoursDeDeselection = true;

        _selectedId = null;
        _txtNom.Clear();
        _txtPrenom.Clear();
        _btnModifier.Enabled = false;
        _btnSupprimer.Enabled = false;

        _grid.ClearSelection();
        _grid.CurrentCell = null;

        _enCoursDeDeselection = false;
    }

    private void ViderChampsSansClear()
    {
        _selectedId = null;
        _txtNom.Clear();
        _txtPrenom.Clear();
        _btnModifier.Enabled = false;
        _btnSupprimer.Enabled = false;
    }
}