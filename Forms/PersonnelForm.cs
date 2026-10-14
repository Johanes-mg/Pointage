using Pointage.Models;
using Pointage.Services;

namespace Pointage.Forms;

public class PersonnelForm : Form
{
    private DataGridView _grid = null!;
    private TextBox _txtNom = null!;
    private TextBox _txtPrenom = null!;
    private TextBox _txtMatricule = null!;
    private TextBox _txtTelephone = null!;
    private TextBox _txtAdresse = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtStatut = null!;
    private TextBox _txtGrade = null!;
    private ComboBox _cboSituation = null!;
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
            Height = 380,
            BackColor = Theme.FondCarte,
            Padding = new Padding(40, 20, 40, 20)
        };

        var lblNom = CreerLabel("Nom", 40, 20);
        _txtNom = CreerTextBox(40, 42, 260);
        var lblPrenom = CreerLabel("Prénom", 320, 20);
        _txtPrenom = CreerTextBox(320, 42, 260);
        var lblMatricule = CreerLabel("N° Matricule (IM)", 600, 20);
        _txtMatricule = CreerTextBox(600, 42, 260);

        var lblTelephone = CreerLabel("Téléphone", 40, 85);
        _txtTelephone = CreerTextBox(40, 107, 260);
        var lblEmail = CreerLabel("Email", 320, 85);
        _txtEmail = CreerTextBox(320, 107, 260);

        var lblAdresse = CreerLabel("Adresse", 40, 150);
        _txtAdresse = CreerTextBox(40, 172, 820);

        var lblStatut = CreerLabel("Statut", 40, 215);
        _txtStatut = CreerTextBox(40, 237, 260);

        var lblGrade = CreerLabel("Grade", 320, 215);
        _txtGrade = CreerTextBox(320, 237, 260);

        var lblSituation = CreerLabel("Situation familiale", 600, 215);
        _cboSituation = new ComboBox
        {
            Location = new Point(600, 237),
            Width = 260,
            Height = 32,
            Font = Theme.TexteNormal,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cboSituation.Items.AddRange(new object[]
        {
            "",
            "Célibataire",
            "Marié(e)",
            "Divorcé(e)",
            "Veuf(ve)"
        });
        _cboSituation.SelectedIndex = 0;

        _btnAjouter = Theme.CreerBoutonBlancArrondi("ajouter.png", "Ajouter");
        _btnAjouter.Location = new Point(40, 300);
        _btnAjouter.Width = 170;

        _btnModifier = Theme.CreerBoutonBlancArrondi("modifier.png", "Modifier");
        _btnModifier.Location = new Point(225, 300);
        _btnModifier.Width = 170;
        _btnModifier.Enabled = false;

        _btnSupprimer = Theme.CreerBoutonBlancArrondi("effacer.png", "Supprimer");
        _btnSupprimer.Location = new Point(410, 300);
        _btnSupprimer.Width = 170;
        _btnSupprimer.Enabled = false;

        _btnAjouter.Click += BtnAjouter_Click;
        _btnModifier.Click += BtnModifier_Click;
        _btnSupprimer.Click += BtnSupprimer_Click;

        panelForm.Controls.AddRange(new Control[]
        {
            lblNom, _txtNom, lblPrenom, _txtPrenom, lblMatricule, _txtMatricule,
            lblTelephone, _txtTelephone, lblEmail, _txtEmail,
            lblAdresse, _txtAdresse,
            lblStatut, _txtStatut, lblGrade, _txtGrade, lblSituation, _cboSituation,
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
            Font = new Font(Theme.FamillePolice, 11F, FontStyle.Regular),
            GridColor = Theme.Bordure,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 55,
            RowTemplate = { Height = 55 },
            AllowUserToResizeRows = false,
            AllowUserToResizeColumns = true,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            ScrollBars = ScrollBars.Both,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        };
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Primaire;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font(Theme.FamillePolice, 11F, FontStyle.Bold);
        _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(15, 0, 15, 0);
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Primaire;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Theme.LigneAlternee;
        _grid.DefaultCellStyle.SelectionBackColor = Theme.Accent;
        _grid.DefaultCellStyle.SelectionForeColor = Color.White;
        _grid.DefaultCellStyle.Padding = new Padding(15, 0, 15, 0);

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Id",
            Visible = false
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Matricule",
            HeaderText = "MATRICULE",
            Width = 110
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Nom",
            HeaderText = "NOM",
            Width = 180
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Prenom",
            HeaderText = "PRÉNOM",
            Width = 180
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Telephone",
            HeaderText = "TÉLÉPHONE",
            Width = 140
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Adresse",
            HeaderText = "ADRESSE",
            Width = 350
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Email",
            HeaderText = "EMAIL",
            Width = 240
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Statut",
            HeaderText = "STATUT",
            Width = 130
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Grade",
            HeaderText = "GRADE",
            Width = 180
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "SituationFamiliale",
            HeaderText = "SITUATION",
            Width = 130
        });

        _grid.DataBindingComplete += (_, _) => NettoyerSelection();
        _grid.CellClick += Grid_CellClick;

        panelGrille.Controls.Add(_grid);

        Controls.Add(panelGrille);
        Controls.Add(panelForm);
        Controls.Add(panelEntete);
    }

    private Label CreerLabel(string texte, int x, int y)
    {
        return new Label
        {
            Text = texte,
            Font = Theme.PetitGras,
            ForeColor = Theme.TexteSecondaire,
            Location = new Point(x, y),
            AutoSize = true
        };
    }

    private TextBox CreerTextBox(int x, int y, int largeur)
    {
        return new TextBox
        {
            Location = new Point(x, y),
            Width = largeur,
            Height = 28,
            Font = Theme.TexteNormal,
            BorderStyle = BorderStyle.FixedSingle
        };
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
            _txtMatricule.Text = item.Matricule ?? "";
            _txtTelephone.Text = item.Telephone ?? "";
            _txtAdresse.Text = item.Adresse ?? "";
            _txtEmail.Text = item.Email ?? "";
            _txtStatut.Text = item.Statut ?? "";
            _txtGrade.Text = item.Grade ?? "";
            _cboSituation.SelectedItem = item.SituationFamiliale ?? "";
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
            Database.AjouterPersonnel(LireFormulaire());
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
            var p = LireFormulaire();
            p.Id = _selectedId.Value;
            Database.ModifierPersonnel(p);
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

    private Personnel LireFormulaire()
    {
        string situation = _cboSituation.SelectedItem?.ToString() ?? "";

        return new Personnel
        {
            Nom = _txtNom.Text.Trim(),
            Prenom = _txtPrenom.Text.Trim(),
            Matricule = string.IsNullOrWhiteSpace(_txtMatricule.Text) ? null : _txtMatricule.Text.Trim(),
            Telephone = string.IsNullOrWhiteSpace(_txtTelephone.Text) ? null : _txtTelephone.Text.Trim(),
            Adresse = string.IsNullOrWhiteSpace(_txtAdresse.Text) ? null : _txtAdresse.Text.Trim(),
            Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim(),
            Statut = string.IsNullOrWhiteSpace(_txtStatut.Text) ? null : _txtStatut.Text.Trim(),
            Grade = string.IsNullOrWhiteSpace(_txtGrade.Text) ? null : _txtGrade.Text.Trim(),
            SituationFamiliale = string.IsNullOrWhiteSpace(situation) ? null : situation
        };
    }

    private void ViderChamps()
    {
        _enCoursDeDeselection = true;

        _selectedId = null;
        _txtNom.Clear();
        _txtPrenom.Clear();
        _txtMatricule.Clear();
        _txtTelephone.Clear();
        _txtAdresse.Clear();
        _txtEmail.Clear();
        _txtStatut.Clear();
        _txtGrade.Clear();
        _cboSituation.SelectedIndex = 0;
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
        _txtMatricule.Clear();
        _txtTelephone.Clear();
        _txtAdresse.Clear();
        _txtEmail.Clear();
        _txtStatut.Clear();
        _txtGrade.Clear();
        _cboSituation.SelectedIndex = 0;
        _btnModifier.Enabled = false;
        _btnSupprimer.Enabled = false;
    }
}