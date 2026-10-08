# Application de Pointage du Personnel

Application de gestion de pointage de présence du personnel développée en **C# (.NET 8 / Windows Forms)** avec base de données **SQLite** intégrée.

---

## 🚀 Fonctionnalités principales

- **Gestion des utilisateurs & Sécurité** :
  - Inscription et authentification sécurisée.
  - Hachage des mots de passe en SHA-256 avec salage (`Salt`).
- **Gestion du personnel** :
  - Ajout, modification et suppression des membres du personnel.
- **Gestion des pointages (Suivi quotidien)** :
  - Calendrier mensuel interactif.
  - Saisie des heures d'arrivée et de sortie par personne.
  - Ajout d'observations par jour/personne.
  - Résumé du bilan journalier (taux de présence, total des présents et absents).
- **Bilan mensuel et statistiques** :
  - Visualisation des jours pointés par membre du personnel.
  - Statistiques consolidées (heures totales effectuées, moyenne d'arrivée).
- **Exportation de données** :
  - Exportation complète de la base et des bilans au format **Microsoft Excel (`.xlsx`)** via ClosedXML.
- **Installeur Windows** :
  - Script Inno Setup inclus pour générer un fichier d'installation (`PointageSetup.exe`).

---

## 🛠️ Technologies & Bibliothèques

- **Langage** : C# (.NET 8.0 Windows Forms)
- **Base de données** : SQLite (`Microsoft.Data.Sqlite` v10.0.12)
- **Gestion Excel** : `ClosedXML` (v0.105.1)
- **Déploiement / Installeur** : Inno Setup 6

---

## 📁 Structure du Projet

```text
Pointage/
├── Forms/                   # Interfaces utilisateur (WinForms)
│   ├── AccueilForm.cs       # Écran d'accueil et horloge en temps réel
│   ├── BilanForm.cs         # Vue synthétique par employé et par mois
│   ├── LoginForm.cs         # Écran de connexion / création de compte
│   ├── MainForm.cs          # Fenêtre principale et navigation
│   ├── PersonnelForm.cs     # Formulaire CRUD du personnel
│   └── PointageForm.cs      # Interface de pointage quotidien
├── Models/                  # Modèles de données (POCO)
│   ├── Bilan.cs             # DTO pour les bilans
│   ├── Personnel.cs         # Modèle Personnel
│   ├── Pointage.cs          # Modèle EnregistrementPointage
│   └── Utilisateur.cs       # Modèle Utilisateur
├── Services/                # Logique métier et accès aux données
│   ├── Database.cs          # Gestion SQLite et requêtes SQL
│   ├── ExcelExport.cs       # Génération des fichiers Excel
│   ├── Securite.cs          # Génération de sel et hachage SHA-256
│   └── Theme.cs             # Couleurs, polices et composants UI personnalisés
├── Images/                  # Icônes et ressources graphiques (PNG/ICO)
├── installer.iss            # Script de compilation Inno Setup
├── Pointage.csproj          # Fichier de projet .NET
└── Program.cs               # Point d'entrée de l'application
```

## ⚙️ Configuration & Compilation

### Prérequis

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 ou Visual Studio Code (avec l'extension C#)
- _(Optionnel)_ [Inno Setup](https://jrsoftware.org/isinfo.php) pour générer l'installeur exécutable.
