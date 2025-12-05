# Project Structure – ProQuote

Ce document décrit la structure technique de la solution ProQuote (ERP de soumission / costing), les projets (.NET), leurs responsabilités et les rôles métier principaux.

---

## 1. Vue d’ensemble de la solution

```text
ProQuote.sln
├── src/
│   ├── ProQuote.Build
│   ├── ProQuote.Core
│   ├── ProQuote.Database
│   ├── ProQuote.Backend
│   ├── ProQuote.Front
│   ├── ProQuote.Automation
│   ├── ProQuote.Plugins
│   └── ProQuote.Integrations
└── tests/
    ├── ProQuote.Core.Tests
    ├── ProQuote.Backend.Tests
    ├── ProQuote.Automation.Tests
    └── ProQuote.Front.Tests
```

- Le dossier **`src/`** contient le code de production.
- Le dossier **`tests/`** contient les projets de tests unitaires et d’intégration.

---

## 2. Projets (packages) – Rôles & Contenu

### 2.1. `ProQuote.Build`

**Type :** Projet utilitaire ou simple dossier de scripts (PowerShell / Bash).  

**Rôle :**  
Centraliser tout ce qui concerne le **build**, le **lint** et la **qualité du code**.

**Contenu typique :**
- Scripts de build : `build.ps1`, `build.sh`
- Scripts de lint / format : `lint.ps1`, `format.ps1`
- Script pour lancer tous les tests : `run-tests.ps1`
- Fichiers de configuration globaux :
  - `.editorconfig`
  - configuration des analyzers Roslyn / StyleCop
  - templates de pipelines (GitHub Actions YAML, etc.)

**Dépendances :**
- Ne dépend d’aucun autre projet applicatif.
- Utilise le SDK .NET et les outils CLI.

---

### 2.2. `ProQuote.Core`

**Type :** Class Library (.NET)  
**Rôle :**  
Cœur **métier** et **modèle de domaine** de l’application. C’est la couche la plus stable et la plus indépendante.  

**Responsabilités :**
- Définir les **entités métier** et les **value objects** :
  - `AppUser`, `Organization`, `Membership`
  - `Plan`, `Subscription`
  - `Bid`, `BidRevision`, `BidStatus`, `BidOption`, `OptionGroup`
  - `StorageProfile`, `BidDocument`
  - `FormTemplate`, `FormStep`, `FormField`
  - `CostingTemplate`, `CostingTable`, `CostingFormula`
  - `SubmissionTemplate`, `SubmissionBlock`, variables de template
  - `CustomTable`, `CustomTableColumn`, `CustomTableRow`
  - `Dashboard`, `DashboardWidget`, `DataSourceDefinition`
  - `PlanDocument`, `TakeoffTag`, etc.
- Définir les **enums et types** :
  - `OrganizationRole` (Owner, Configurator, EstimatorSenior, Estimator, Viewer)
  - `BidStatus`, `OptionGroupType`, `TakeoffQuantityType`, etc.
- Définir les **interfaces** (contrats) :
  - `IDocumentStorage`
  - `IMailSender`
  - `ICostingEngine`
  - `ISubmissionRenderer`
  - `IAutomationEngine`
  - Contracts pour le DSL de formules (parseur / évaluateur).
- Contenir la logique métier de base (méthodes sur les entités, validation simple).

**Dépendances :**
- Ne dépend d’aucun autre projet de la solution.
- Dépend uniquement de packages .NET de base (ex : `System.*`, éventuellement un package de validation générique).

---

### 2.3. `ProQuote.Database`

**Type :** Class Library (.NET) + Entity Framework Core  
**Rôle :**  
Implémenter la **persistance** des entités définies dans `ProQuote.Core` via EF Core.

**Responsabilités :**
- Fournir le `ProQuoteDbContext` :
  - `DbSet<AppUser>`, `DbSet<Organization>`, `DbSet<Bid>`, etc.
- Configurer le mapping :
  - Fichiers `EntityTypeConfiguration` par entité principale (`BidConfiguration`, `OrganizationConfiguration`, etc.).
- Gérer les **migrations EF Core** :
  - Dossier `Migrations/` contenant l’historique de schéma.
- (Optionnel) Contenir un mécanisme minimal de “seed” de données (plans par défaut, rôles initiaux…).

**Dépendances :**
- Référence `ProQuote.Core`.
- NuGet : `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, provider de base de données (SQL Server, PostgreSQL, etc.).

---

### 2.4. `ProQuote.Backend`

**Type :** ASP.NET Core Web API (ou Server avec endpoints API)  
**Rôle :**  
Exposer la couche **Application** et les **API** consommées par le front-end et les services externes.

**Responsabilités :**
- Héberger l’application ASP.NET Core :
  - `Program.cs` / `Startup` ou équivalent minimal hosting.
- Configurer :
  - DI (Dependency Injection)
  - EF Core (`ProQuoteDbContext`)
  - ASP.NET Core Identity avec `AppUser`
  - Authentification et autorisation (JWT / cookies).
- Exposer les **Controllers / Endpoints** :
  - Authentification / Account
  - Gestion des Organisations & Memberships
  - Gestion des Bids & Workflow de soumission
  - Gestion des Templates (Form, Costing, Submission)
  - Module Relevé PDF (Takeoff)
  - Module Dashboards & Reporting
  - Module Automation (déclenchement manuels, visualisation des runs, etc.).
- Implémenter les **services d’application** (dossier `Application/`) :
  - Orchestration du domaine (`Core`) + persistance (`Database`) + intégrations (`Integrations`, `Automation`, `Plugins`).
- Implémenter certains contrats du `Core` lorsque nécessaire côté serveur :
  - ex : adaptateurs vers `IDocumentStorage`, `IMailSender`, etc.

**Dépendances :**
- Référence `ProQuote.Core`
- Référence `ProQuote.Database`
- Référence `ProQuote.Automation`
- Référence `ProQuote.Integrations`
- NuGet : `Microsoft.AspNetCore.*`, `Swashbuckle` (Swagger), `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, etc.

---

### 2.5. `ProQuote.Front`

**Type :** Blazor Server (recommandé ici)  
**Rôle :**  
Fournir l’**interface utilisateur** (UI) de ProQuote.

**Responsabilités :**
- Pages Blazor :
  - Authentification, sélection d’organisation
  - Dashboard principal
  - Gestion des Clients / Projets
  - Gestion des Bids (liste, détail, workflow, options, révisions)
  - Éditeurs de templates (Form, Costing, Soumission)
  - Module Relevé PDF (viewer + dessin de tags)
  - Pages de configuration (stockage, mail, utilisateurs, rôles)
- Composants Blazor :
  - Composants basés sur **MudBlazor** (inputs, tables, dialogs, charts…)
  - Widgets pour dashboards (KPI, graphiques, tableaux, gauges)
  - Composants d’édition DSL (Monaco editor ou équivalent JS interop)
- Services côté front :
  - `ApiClient` pour consommer les API de `ProQuote.Backend`
  - `AuthStateProvider` pour la gestion de la session côté Blazor
  - Services d’état UI (filtres, contextes, navigation).

**Dépendances :**
- Référence `ProQuote.Core` (pour réutiliser les modèles / enums partagés).
- Ne référence pas `ProQuote.Database`.
- NuGet : `MudBlazor`, `Microsoft.AspNetCore.Components.*`, bibliothèques de PDF viewer / JS interop si nécessaire.

---

### 2.6. `ProQuote.Automation`

**Type :** Class Library (.NET)  
**Rôle :**  
Gérer les **flows d’automation** no-code / low-code et leur exécution.

**Responsabilités :**
- Définir les modèles d’automation (ou en coordination avec `Core`) :
  - `AutomationFlow`, `AutomationTrigger`, `AutomationAction`, `AutomationRunLog`, etc.
- Implémenter le **moteur d’automation** :
  - `AutomationEngine` : interprétation des triggers (OnBidCreated, OnBidStatusChanged, OnBidAccepted, etc.)
  - exécution des actions (`SendEmail`, `UpdateBidStatus`, `CreateTask`, etc.).
- Fournir une API interne consommable par `ProQuote.Backend` :
  - ex : `IAutomationEngine.RunAsync(eventContext)`.

**Dépendances :**
- Référence `ProQuote.Core` (contrats, modèles).
- Référence `ProQuote.Integrations` (pour actions nécessitant des intégrations externes, comme l’envoi d’email).
- Ne dépend pas de `ProQuote.Front` ni directement de `ProQuote.Database` (les données nécessaires sont fournies par le Backend ou des services).

---

### 2.7. `ProQuote.Plugins`

**Type :** Class Library (.NET)  
**Rôle :**  
Fournir un mécanisme d’**extensions / plugins** pour personnaliser le comportement métier sans toucher au cœur de l’application.

**Responsabilités :**
- Définir des interfaces de plugins :
  - `ICostingPlugin`
  - `ISubmissionPlugin`
  - `IImportPlugin`
  - etc.
- Contenir des **implémentations concrètes** pour certains clients ou contextes :
  - ex : `AlumidekPlugin` avec logiques de costing spécifiques.
- Permettre à `ProQuote.Backend` et `ProQuote.Automation` de consommer ces plugins via DI.

**Dépendances :**
- Référence `ProQuote.Core`.
- Optionnellement, dépend de certaines intégrations spécifiques si un plugin le requiert.

---

### 2.8. `ProQuote.Integrations`

**Type :** Class Library (.NET)  
**Rôle :**  
Centraliser toutes les **intégrations externes** : email, stockage externe, IA, etc.

**Responsabilités :**
- Implémenter les contrats définis dans `Core` :
  - `IMailSender` : `OutlookGraphMailSender`, `SmtpMailSender`, etc.
  - `IDocumentStorage` : `FileSystemDocumentStorage`, plus tard `AzureBlobDocumentStorage`, etc.
  - Services IA pour assistance à l’estimation ou génération de contenu.
- Gérer la configuration via les options typées (`IOptions<T>`).

**Dépendances :**
- Référence `ProQuote.Core`.
- NuGet :
  - ex : `Microsoft.Graph` pour Outlook
  - clients HTTP / SDK IA
  - drivers de stockage si nécessaire (Azure, AWS…).

---

### 2.9. Projets de tests (`tests/`)

**Objectif :**  
Isoler les tests par couche pour garder une architecture propre.

- `ProQuote.Core.Tests` :
  - Tests unitaires sur les entités, services métier, DSL, etc.
- `ProQuote.Backend.Tests` :
  - Tests unitaires et tests d’intégration (controllers, services d’application, API).
- `ProQuote.Automation.Tests` :
  - Tests sur le moteur d’automation et les flows.
- `ProQuote.Front.Tests` :
  - Tests des composants Blazor (via bUnit ou équivalent).

---

## 3. Rôles métier & Responsabilités (vision technique)

Les rôles métier sont principalement utilisés dans `ProQuote.Core` (via `OrganizationRole`) et implémentés dans `ProQuote.Backend` (authz) et `ProQuote.Front` (contrôle d’UI).

### 3.1. `MAdmin` (Platform Admin)

**Niveau :** plateforme (global, au-dessus des organisations)  
**Représentation technique :**  
Flag sur `AppUser` (ex. `IsPlatformAdmin = true`).

**Responsabilités :**
- Gérer les `AppUser` (création, désactivation, promotion MAdmin).
- Gérer les `Organization` (création, configuration globale).
- Gérer les `Plan` et `Subscription`.
- Accéder à des dashboards transverses plateforme.

---

### 3.2. `Owner` (par organisation)

**Niveau :** organisation (tenant).  
**Représentation :**  
Role dans `Membership.OrganizationRole = Owner`.

**Responsabilités :**
- Gérer les membres de son organisation (inviter, désactiver, changer les rôles).
- Configurer les paramètres clés :
  - `StorageProfile` (stockage des documents)
  - `MailProfile` (envoi des emails)
  - Template par défaut, règles de validation, etc.
- Accéder à tous les dashboards de son organisation.

---

### 3.3. `Configurator` (PowerUser / Template Admin)

**Représentation :**  
`Membership.OrganizationRole = Configurator`.

**Responsabilités :**
- Créer / modifier :
  - `FormTemplate` (forms par étapes)
  - `CostingTemplate` (DSL de calcul, tables de costing)
  - `SubmissionTemplate` (modèles de documents)
  - `CustomTable` (tables métier dynamiques)
- Configurer :
  - Flows d’automation (triggers, actions)
  - Dashboards (widgets, datasources, visibilité par rôle)
- Publier les templates pour usage par les `Estimator` / `EstimatorSenior`.

---

### 3.4. `Estimator Senior`

**Représentation :**  
`Membership.OrganizationRole = EstimatorSenior`.

**Responsabilités :**
- Créer / modifier des `Bid` (soumissions).
- Gérer les **révisions** :
  - R00, R01, R02…
  - figer les versions, choisir quoi envoyer au client.
- Gérer le **cycle de vie** :
  - transitions de statut (Draft → PendingApproval → Sent → Negotiation → Accepted / Rejected / Expired).
- Envoyer les soumissions via le **Mail Builder**.
- Valider les relevés sur plans (takeoff) réalisés sur les PDF.
- Accéder aux dashboards avancés (marge, ratio de conversion…).

---

### 3.5. `Estimator`

**Représentation :**  
`Membership.OrganizationRole = Estimator`.

**Responsabilités :**
- Utiliser les templates configurés pour :
  - saisir des données dans les FormTemplates
  - lancer les calculs via CostingTemplates
  - préparer des Bids en **Draft**.
- Soumettre des Bids en **PendingApproval** pour validation interne.
- Réaliser des relevés de quantités sur plans PDF (création de tags).
- Consulter des dashboards opérationnels (pipeline de soumissions, statut).

---

### 3.6. `Viewer`

**Représentation :**  
`Membership.OrganizationRole = Viewer`.

**Responsabilités :**
- Consulter :
  - Bids, Clients, Projets
  - Documents de soumission (PDF)
  - Dashboards autorisés
- Aucun droit d’édition sur :
  - templates
  - flows d’automation
  - configuration d’organisation
  - données critiques (marge, costing détaillé), selon la configuration de visibilité.

---

Ce `project_structure.md` sert de base pour organiser ta solution Visual Studio / .NET, documenter ce que contient chaque projet, et clarifier le lien entre les paquets techniques et les rôles métier principaux.
