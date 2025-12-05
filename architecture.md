# Architecture – ProQuote

Ce document décrit l’architecture technique globale de l’application **ProQuote** (ERP de soumission / costing), incluant :
- la structure des projets (.NET),
- les responsabilités par couche,
- le modèle de communication (HTTP + SignalR),
- la gestion des environnements et de la configuration,
- les rôles métier principaux (vision technique).

---

## 1. Vue d’ensemble

ProQuote est une application **.NET** (ciblant **.NET 8.0 / net8.0**) modulaire, construite autour des principes suivants :
- **Séparation claire** entre domaine (Core), persistance (Database), backend (API & services), UI (Front), automatisation (Automation), intégrations externes (Integrations) et extensions (Plugins).
- **Backend HTTP** exposant des API REST, utilisé comme contrat stable entre le front et le serveur.
- **Couche temps réel** basée sur **SignalR**, utilisée de manière ciblée pour les fonctionnalités nécessitant du push (notifications, dashboards, automation, takeoff collaboratif).
- **Blazor** pour le front-end (Blazor Server), intégrant MudBlazor comme librairie UI principale.

Structure des projets :

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

---

## 2. Projets et responsabilités

### 2.1. `ProQuote.Build`

**Rôle :** Outillage de build, qualité et CI/CD.

- Scripts de build, formatage, lint et exécution des tests (PowerShell/Bash).
- Fichiers de configuration partagés (`.editorconfig`, règles d’analyse, templates GitHub Actions).
- Point d’entrée pour automatiser localement les vérifications avant push.

Le projet ne contient pas de logique métier.

---

### 2.2. `ProQuote.Core`

**Rôle :** Couche **Domaine**, cœur métier de ProQuote.

- Entités et Value Objects :
  - Identité & organisation : `AppUser`, `Organization`, `Membership`, `Plan`, `Subscription`.
  - Stockage & email : `StorageProfile`, `MailProfile`.
  - Client & projet : `Client`, `Project`.
  - Soumission : `Bid`, `BidRevision`, `BidStatus`, `BidOption`, `OptionGroup`, `BidDocument`, `BidEmailLog`.
  - Templates : `FormTemplate`, `FormStep`, `FormField`, `CostingTemplate`, `CostingTable`, `CostingFormula`, `SubmissionTemplate`.
  - Tables métier : `CustomTable`, `CustomTableColumn`, `CustomTableRow`.
  - Takeoff PDF : `PlanDocument`, `TakeoffTag`.
  - Dashboards : `Dashboard`, `DashboardWidget`, `WidgetVisibilityRule`.
  - Automation : `AutomationFlow`, `AutomationTrigger`, `AutomationAction`, `AutomationRunLog`.
- Enums :
  - `OrganizationRole` (Owner, Configurator, EstimatorSenior, Estimator, Viewer).
  - `BidStatus`, `OptionGroupType`, `TakeoffQuantityType`, etc.
- Interfaces (contrats) :
  - `IDocumentStorage`, `IMailSender`, `ICostingEngine`, `ISubmissionRenderer`, `IAutomationEngine`, etc.
- Logique métier de base (méthodes sur les entités, règles simples).

**Dépendances :**  
Ne dépend d’aucun autre projet applicatif. Dépend uniquement du framework .NET.

---

### 2.3. `ProQuote.Database`

**Rôle :** Couche de **persistance** basée sur Entity Framework Core.

- `ProQuoteDbContext` :
  - `DbSet<AppUser>`, `DbSet<Organization>`, `DbSet<Bid>`, etc.
- Configurations EF Core (fichiers `EntityTypeConfiguration`).
- Migrations EF Core (dossier `Migrations/`).
- (Optionnel) mécanisme de seed de données de base (plans, rôles).

**Dépendances :**
- Référence `ProQuote.Core`.
- EF Core + provider de base de données (SQL Server ou PostgreSQL).

---

### 2.4. `ProQuote.Backend`

**Rôle :** Couche **Application & API**, incluant HTTP et SignalR.

- Application ASP.NET Core :
  - `Program.cs` / configuration DI, authentification, autorisation, EF, logging.
- **API HTTP (REST / minimal APIs)** :
  - Endpoints pour : Organisations, Users, Bids, Templates, CustomTables, Dashboards, Takeoff, Automation, etc.
  - DTOs / mappers entre domaine (`Core`) et contrats API.
- **Hubs SignalR** (temps réel) :
  - `NotificationHub` : notifications globales (changement de statut de Bid, nouvel email envoyé, etc.).
  - `DashboardHub` : push de mise à jour de KPIs / widgets.
  - `TakeoffHub` : synchronisation temps réel des tags/annotations sur plan (collaboration).
  - `AutomationHub` : suivi en direct de l’exécution des flows.
- Services d’application :
  - `BidService`, `TemplateService`, `OrganizationService`, `TakeoffService`, `DashboardService`, `AutomationOrchestrator`, etc.
  - Orchestration des entités du domaine (`Core`), de la persistance (`Database`), des intégrations (`Integrations`) et de l’automation (`Automation`).

**Dépendances :**  
Références vers :
- `ProQuote.Core`
- `ProQuote.Database`
- `ProQuote.Automation`
- `ProQuote.Integrations`

---

### 2.5. `ProQuote.Front`

**Rôle :** Interface utilisateur principale (Blazor).

- **Blazor Server** (recommandé dans la V1) :
  - Utilise SignalR sous le capot pour la communication UI ↔ serveur.
  - Consomme les API HTTP exposées par `ProQuote.Backend` pour charger/persister les données.
  - Établit des connexions SignalR vers les hubs (`NotificationHub`, `DashboardHub`, `TakeoffHub`, `AutomationHub`) pour recevoir des événements temps réel.
- Pages :
  - Authentification, sélection d’organisation.
  - Gestion des clients, projets, soumissions.
  - Éditeurs de templates (Form / Costing / Submission).
  - Module de relevé sur plan PDF.
  - Dashboards & reporting.
  - Configuration (stockage, mail, utilisateurs, rôles).
- Composants :
  - Basés sur **MudBlazor** (inputs, tables, dialogues, charts, gauge, etc.).
  - Composants de widgets pour dashboards.
  - Composants d’édition DSL (Monaco Editor via JS interop).

**Dépendances :**
- Référence `ProQuote.Core` pour les modèles partagés (enums, contracts simples).
- Ne référence pas `ProQuote.Database`.

---

### 2.6. `ProQuote.Automation`

**Rôle :** Moteur d’**automation** (flows no-code / low-code).

- Modèle d’automation (lié au domaine) :
  - `AutomationFlow` (définition d’un flow pour une organisation).
  - `AutomationTrigger` (événement : OnBidCreated, OnBidStatusChanged, OnBidAccepted, etc.).
  - `AutomationAction` (action : SendEmail, UpdateStatus, CreateTask, etc.).
  - `AutomationRunLog` (log d’exécution).  
- Moteur d’exécution :
  - `AutomationEngine` implémentant `IAutomationEngine` (défini dans `Core`).
  - API interne appelée par `ProQuote.Backend` lors d’événements métier.
  - Capacité à publier des événements vers les Hubs SignalR (via backend) pour afficher l’état d’exécution en temps réel.

**Dépendances :**
- Référence `ProQuote.Core`.
- Référence `ProQuote.Integrations` (pour les actions utilisant des services externes : email, stockage, etc.).

---

### 2.7. `ProQuote.Plugins`

**Rôle :** Point d’extension pour des **plugins** et logiques spécifiques.

- Interfaces de plug-ins :
  - `ICostingPlugin`, `ISubmissionPlugin`, `IImportPlugin`, etc.
- Implémentations spécifiques (par client ou vertical métier) :
  - Exemple : `AlumidekPlugin` avec règles spécifiques de costing, mapping de profils, etc.

**Dépendances :**
- Référence `ProQuote.Core`.

Les plugins sont consommés via DI par `ProQuote.Backend` et/ou `ProQuote.Automation`.

---

### 2.8. `ProQuote.Integrations`

**Rôle :** Intégrations externes (email, stockage, IA…).

- Implémentations des contrats `Core` :
  - `IMailSender` :
    - `SmtpMailSender` (SMTP générique via MailKit).
    - `OutlookGraphMailSender` (Microsoft Graph).
  - `IDocumentStorage` :
    - `FileSystemDocumentStorage` (stockage local / partage réseau).
    - plus tard `AzureBlobDocumentStorage`, `S3DocumentStorage`, etc.
  - Clients IA (assistant d’estimation, génération de texte de soumission, etc.).
- Gestion des secrets et configuration via `IOptions<T>` et les settings par environnement.

**Dépendances :**
- Référence `ProQuote.Core`.

---

### 2.9. Projets de tests

**Rôle :** Garantir la qualité par couche.

- `ProQuote.Core.Tests` :
  - Tests unitaires sur les entités, règles métier, DSL de formules, etc.
- `ProQuote.Backend.Tests` :
  - Tests sur les services d’application, endpoints API, logique d’auth/autorisation.
- `ProQuote.Automation.Tests` :
  - Tests sur le moteur d’automation et la logique de flows.
- `ProQuote.Front.Tests` :
  - Tests des composants Blazor (via bUnit).

---

## 3. Modèle de communication : HTTP + SignalR

### 3.1. HTTP (API REST)

- **Usage principal :**
  - CRUD classique (clients, projets, bids, templates, tables métier, dashboards, automation flows).
  - Actions ponctuelles : génération de PDF, calcul de costing, exécution d’un flow manuel, etc.
- **Avantages :**
  - Contrat clair, stable et documentable (Swagger/OpenAPI).
  - Réutilisable par d’autres consommateurs (applications tierces, scripts, intégrations futures).
  - Facile à tester (Postman, Swagger UI).

Les endpoints HTTP sont exposés par `ProQuote.Backend` et consommés par `ProQuote.Front` (et potentiellement d’autres clients).

### 3.2. SignalR (temps réel)

SignalR est utilisé comme **couche complémentaire**, au-dessus des API HTTP, pour gérer les besoins temps réel.

**Cas d’usage principaux :**

1. **Notifications métier :**
   - Changement de statut d’une Bid (PendingApproval → Accepted).
   - Nouvelle Bid créée / assignée à un utilisateur.
   - Résultats d’exécution d’un flow d’automation.

2. **Dashboards dynamiques :**
   - Mise à jour en direct des KPIs (montant total soumis, taux d’acceptation, etc.).
   - Actualisation des widgets sans rechargement global ou polling agressif.

3. **Suivi des flows d’automation :**
   - Affichage en temps réel de l’état des flows (en cours, succès, échec).
   - Feedback immédiat après un événement (par ex. BidAccepted).

4. **Takeoff PDF collaboratif (phase ultérieure) :**
   - Synchronisation des tags de relevé lorsqu’un utilisateur modifie un plan.
   - Highlight des zones modifiées chez les utilisateurs connectés au même projet.

**Hubs prévus :**

- `NotificationHub` : événements globaux liés aux organisations, bids, etc.
- `DashboardHub` : push ciblé d’événements pour rafraîchir des widgets spécifiques.
- `TakeoffHub` : événements de modification des tags sur les plans (takeoff collaboratif).
- `AutomationHub` : suivi temps réel des flows (logs, statut, erreurs).

Le **pattern** est le suivant :

1. Un événement métier se produit (ex: BidStatusChanged).
2. `ProQuote.Backend` :
   - Met à jour la base via EF Core (`Database`).
   - Déclenche l’exécution d’un Flow via `ProQuote.Automation` si applicable.
   - Envoie une notification via le Hub SignalR concerné.
3. `ProQuote.Front` :
   - Consomme l’API HTTP pour charger les données structurées.
   - Écoute SignalR pour être notifié des changements et rafraîchir l’UI de manière ciblée.

---

## 4. Gestion des environnements & configuration

ProQuote distingue plusieurs environnements :

- **Development**
- **Staging** (préproduction, optionnel)
- **Production**

### 4.1. Configuration

Utilisation du système de configuration ASP.NET Core :

- `appsettings.json`
- `appsettings.Development.json`
- `appsettings.Staging.json`
- `appsettings.Production.json`
- Variables d’environnement
- Secrets utilisateurs en développement (`dotnet user-secrets`) pour :
  - chaîne de connexion base de données,
  - credentials SMTP / Outlook / IA,
  - clés des services externes.

Exemple (Backend) :

```jsonc
{
  "ConnectionStrings": {
    "Default": "Server=...;Database=ProQuote;User Id=...;Password=...;"
  },
  "Storage": {
    "Type": "LocalFileSystem",
    "RootPath": "C:\\ProQuoteData"
  },
  "Mail": {
    "Provider": "Smtp",
    "Host": "smtp.server.com",
    "Port": 587,
    "UseSsl": true
  }
}
```

Les profils spécifiques par organisation (`StorageProfile`, `MailProfile`) sont enregistrés en base et appliqués via les services du `Backend` & `Integrations`.

---

## 5. Rôles métier – Vision technique

Les rôles métier sont utilisés principalement dans :

- `ProQuote.Core` : via `OrganizationRole` et `Membership` pour modéliser les droits.
- `ProQuote.Backend` : via les politiques d’autorisation et claims.
- `ProQuote.Front` : pour contrôler l’affichage et l’accès aux composants.

### 5.1. MAdmin (Platform Admin)

- Niveau : plateforme.
- Implémentation : propriété `IsPlatformAdmin` dans `AppUser`.
- Droits :
  - gérer les `AppUser` (création, désactivation, promotion MAdmin),
  - gérer les `Organization`, `Plan`, `Subscription`,
  - accéder aux métriques globales (usage plateforme).

### 5.2. Owner (par organisation)

- Niveau : organisation.
- Implémentation : `Membership.OrganizationRole = Owner`.
- Droits :
  - gérer les membres (ajouter/supprimer des `Membership`),
  - attribuer les rôles internes (Configurator, EstimatorSenior, Estimator, Viewer),
  - configurer `StorageProfile`, `MailProfile`, templates par défaut,
  - accès complet aux données fonctionnelles de l’organisation.

### 5.3. Configurator (PowerUser)

- Implémentation : `Membership.OrganizationRole = Configurator`.
- Droits :
  - créer/éditer les `FormTemplate`, `CostingTemplate`, `SubmissionTemplate`,
  - créer/éditer les `CustomTable` (tables métier),
  - créer/éditer les `Dashboard` et `DashboardWidget`,
  - définir les `AutomationFlow` (triggers, actions),
  - contrôler la visibilité des widgets par rôle.

### 5.4. Estimator Senior

- Implémentation : `Membership.OrganizationRole = EstimatorSenior`.
- Droits :
  - créer et modifier des `Bid` et `BidRevision`,
  - gérer le **cycle de vie** des soumissions (statuts, révisions),
  - envoyer des emails via le **Mail Builder**,
  - valider et corriger les relevés sur plans (takeoff),
  - accéder à des dashboards avancés (marge, ratio de succès, etc.).

### 5.5. Estimator

- Implémentation : `Membership.OrganizationRole = Estimator`.
- Droits :
  - utiliser les templates configurés pour saisir les données (forms, costing),
  - créer des Bids en statut **Draft**,
  - soumettre des Bids en **PendingApproval**,
  - effectuer des relevés sur plans PDF (tags de takeoff),
  - consulter des dashboards opérationnels.

### 5.6. Viewer

- Implémentation : `Membership.OrganizationRole = Viewer`.
- Droits :
  - consommation en lecture seule des Bids, projets, clients, documents,
  - accès limité aux dashboards,
  - aucune modification des données critiques ni des configurations.

---

## 6. Synthèse

- **HTTP** reste la base de l’architecture de communication :  
  standard, testable, interopérable.
- **SignalR** vient en **complément ciblé** pour les fonctionnalités temps réel :  
  notifications, dashboards live, automation en direct, collaboration sur plans.
- La séparation **Core / Database / Backend / Front / Automation / Integrations / Plugins** permet :
  - testabilité,
  - évolutivité,
  - isolation claire des responsabilités,
  - possibilité de faire évoluer certaines briques (ex : stockage cloud, IA) sans casser le reste.

Ce document sert de référence globale pour l’architecture technique de ProQuote, et pourra être enrichi au fur et à mesure (détails sur la sécurité, versioning d’API, multi-tenant avancé, etc.).
