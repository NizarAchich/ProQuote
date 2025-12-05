# User Stories – ProQuote Application

Ce document regroupe les principales user stories issues de la conception fonctionnelle de ProQuote (ERP de soumission / costing).  
Les rôles principaux sont :

- **MAdmin** (Platform Admin)
- **Owner** (propriétaire de l’organisation)
- **Configurator** (PowerUser, responsable des modèles / règles)
- **Estimator Senior**
- **Estimator**
- **Viewer**

---

## 1. Authentification & Gestion des utilisateurs (plateforme)

### US-AU-001 – Créer un compte utilisateur (AppUser)
**En tant que** MAdmin  
**Je veux** créer un compte utilisateur global (AppUser) avec email et mot de passe  
**Afin de** permettre à cette personne d’accéder à une ou plusieurs organisations dans ProQuote.

**Critères d’acceptation :**
- Je peux créer un utilisateur avec email unique, nom, et statut actif.
- Le système envoie (optionnellement) un email d’invitation pour définir son mot de passe.
- Le mot de passe est stocké de façon sécurisée (hash).

---

### US-AU-002 – Se connecter à l’application
**En tant que** utilisateur (AppUser)  
**Je veux** me connecter avec mon email et mot de passe  
**Afin de** accéder à mon espace et aux organisations dont je fais partie.

**Critères d’acceptation :**
- Si les identifiants sont corrects, je suis authentifié.
- Si j’appartiens à plusieurs organisations, le système me demande de choisir laquelle activer.
- Si mon compte est désactivé, je ne peux pas me connecter.

---

### US-AU-003 – Gestion des AppUsers par MAdmin
**En tant que** MAdmin  
**Je veux** visualiser, filtrer, désactiver et réactiver des AppUsers  
**Afin de** administrer les comptes au niveau plateforme.

**Critères d’acceptation :**
- Je peux voir la liste de tous les AppUsers avec email, date de création, statut.
- Je peux désactiver un AppUser (IsActive/IsDeleted) sans le supprimer physiquement.
- Je peux promouvoir ou rétrograder un AppUser en MAdmin.

---

## 2. Organisations & Rôles internes

### US-OR-001 – Créer une organisation (tenant)
**En tant que** MAdmin  
**Je veux** créer une nouvelle organisation (client)  
**Afin de** permettre à ce client d’utiliser ProQuote avec ses propres données et utilisateurs.

**Critères d’acceptation :**
- Je peux définir le nom de l’organisation et ses métadonnées (raison sociale, etc.).
- Une fois créée, l’organisation n’est visible que par ses membres et les MAdmin.

---

### US-OR-002 – Gérer les membres d’une organisation
**En tant que** Owner  
**Je veux** inviter, gérer et désactiver des membres (Membership) de mon organisation  
**Afin de** contrôler qui a accès aux données de mon entreprise.

**Critères d’acceptation :**
- Je peux inviter un nouvel utilisateur via son email.
- Je peux assigner un rôle (Owner, Configurator, EstimatorSenior, Estimator, Viewer).
- Je peux désactiver un membership sans supprimer l’AppUser global.

---

### US-OR-003 – Rôle Estimator Senior
**En tant que** Owner  
**Je veux** pouvoir promouvoir certains estimateurs au rôle **Estimator Senior**  
**Afin de** leur donner des droits de validation et de gestion avancée des soumissions, sans leur donner un accès complet aux templates ou aux réglages de l’organisation.

**Critères d’acceptation :**
- Un Estimator Senior peut approuver des soumissions, envoyer au client, gérer des révisions.
- Un Estimator Senior ne peut pas modifier les templates structurels (forms, costing, soumission) sauf permission explicite.

---

## 3. Plans & abonnements

### US-PL-001 – Gérer les plans de la plateforme
**En tant que** MAdmin  
**Je veux** créer, modifier et désactiver des Plans (nom, prix, limites)  
**Afin de** proposer différents niveaux d’abonnement aux organisations.

**Critères d’acceptation :**
- Je peux définir les limites (MaxUsers, MaxProjects, MaxStorageMb, etc.).
- Un Plan peut être actif ou inactif.
- Une organisation ne peut avoir qu’un seul Plan actif à la fois.

---

### US-PL-002 – Assigner un plan à une organisation
**En tant que** MAdmin  
**Je veux** associer un Plan et une Subscription à une organisation  
**Afin de** activer les fonctionnalités et limites correspondantes.

**Critères d’acceptation :**
- Je peux associer un plan avec une date de début et éventuellement de fin.
- Le système applique les limites du plan (ex : nombre max d’utilisateurs).

---

## 4. Profils de stockage & PDF de soumission

### US-ST-001 – Configurer le stockage des documents
**En tant que** Owner  
**Je veux** configurer un **StorageProfile** (type de stockage + chemin racine / container) pour mon organisation  
**Afin de** contrôler l’endroit où les PDFs de soumission et autres documents sont sauvegardés.

**Critères d’acceptation :**
- Je peux choisir un type de stockage (LocalFileSystem, NetworkShare, etc.).
- Je peux définir un chemin racine (ex : `C:\ProQuoteData\MonOrga` ou `\\Serveur\Soumissions`).
- Je peux tester la configuration et être alerté en cas d’erreur d’accès.

---

### US-ST-002 – Générer un PDF de soumission
**En tant que** Estimator ou Estimator Senior  
**Je veux** cliquer sur un bouton “Générer la soumission” pour une Bid  
**Afin de** produire un PDF à partir d’un template de soumission, et l’enregistrer dans le stockage configuré.

**Critères d’acceptation :**
- Le système génère un PDF basé sur le SubmissionTemplate sélectionné.
- Le PDF est stocké via `IDocumentStorage` et un `BidDocument` est créé en base.
- Je peux ensuite télécharger le PDF depuis l’interface de la soumission.

---

### US-ST-003 – Lister les documents d’une soumission
**En tant que** Estimator / Estimator Senior / Configurator  
**Je veux** voir la liste des documents associés à une soumission (Bid)  
**Afin de** accéder aux différentes révisions de PDF, annexes, etc.

**Critères d’acceptation :**
- Je vois les documents avec : nom, type, date, utilisateur d’origine.
- Je peux télécharger chaque document individuellement.
- Les droits d’accès respectent les rôles (Viewer en lecture seule).

---

## 5. Messagerie & Mail Builder

### US-ML-001 – Configurer un profil d’envoi d’email
**En tant que** Owner  
**Je veux** configurer un profil d’email (**MailProfile**) pour mon organisation (Outlook, SMTP, etc.)  
**Afin de** envoyer les soumissions et communications client depuis une adresse d’entreprise.

**Critères d’acceptation :**
- Je peux choisir un provider (OutlookGraph, SMTP).
- Je peux saisir les informations de connexion et les tester.
- Le système utilise ensuite ce profil pour tous les envois d’emails liés aux soumissions de l’organisation.

---

### US-ML-002 – Construire un email via le Mail Builder
**En tant que** Estimator Senior  
**Je veux** disposer d’une interface de **Mail Builder** pour préparer un email de soumission  
**Afin de** personnaliser le sujet, le texte et les destinataires avant envoi.

**Critères d’acceptation :**
- Le mail builder propose : To (contact client), Cc (utilisateur connecté, autres), Subject, Body HTML, pièces jointes.
- Je peux insérer des variables (ex : NomClient, NuméroSoumission, Total).
- Le message est envoyé via le `MailProfile` de l’organisation.
- Un enregistrement (log) des emails envoyés est stocké et lié à la soumission.

---

### US-ML-003 – Envoyer un email de soumission
**En tant que** Estimator Senior  
**Je veux** envoyer le PDF de soumission au client directement depuis ProQuote  
**Afin de** garder un historique centralisé des envois et éviter les manipulations manuelles.

**Critères d’acceptation :**
- Le système associe automatiquement la soumission, le client et le contact.
- Le PDF de la soumission est joint automatiquement, selon la dernière révision.
- L’email envoyé est tracé (date, destinataire, sujet, ID du document).

---

## 6. Templates : Forms, Costing, Soumission

### US-TM-001 – Créer un FormTemplate par étapes
**En tant que** Configurator  
**Je veux** définir un **FormTemplate** composé de plusieurs étapes (steps) et de champs typés  
**Afin de** guider les estimateurs dans la saisie des informations nécessaires au costing et à la soumission.

**Critères d’acceptation :**
- Je peux ajouter des steps et les ordonner.
- Je peux ajouter des champs (text, number, date, dropdown, checkbox, etc.) avec des codes de variables.
- Je peux définir des règles de validation simples (obligatoire, min/max, etc.).
- Le formulaire peut être utilisé pour créer ou modifier une BidInstance.

---

### US-TM-002 – Créer un CostingTemplate avec formules métier
**En tant que** Configurator  
**Je veux** créer un **CostingTemplate** qui définit les calculs de prix à partir des variables du formulaire, de tables métiers et de formules pseudo-code  
**Afin de** standardiser la logique de costing pour un type d’élément (ex : SV10, SB30).

**Critères d’acceptation :**
- Je peux définir des tables de costing (lignes, colonnes, totaux).
- Je peux définir des formules dans un DSL métier (IF, SUM, LOOKUP, etc., sans C#/SQL brut).
- Les formules peuvent utiliser les variables du formulaire et les données des Custom Tables.
- Je peux tester le template with des valeurs d’essai et voir le résultat du calcul.

---

### US-TM-003 – Créer un SubmissionTemplate avec variables
**En tant que** Configurator  
**Je veux** créer un **SubmissionTemplate** (éditeur type WYSIWYG) avec texte, tableaux, sections et variables  
**Afin de** générer des documents de soumission cohérents et professionnels à partir des données de la Bid.

**Critères d’acceptation :**
- Je peux insérer des variables comme {{Client.Name}}, {{Bid.Total}}, {{Project.Address}}.
- Je peux gérer des sections répétitives (boucles) pour les items de la soumission.
- Je peux prévisualiser le rendu HTML avant génération PDF.
- Le template est versionné et réutilisable pour d’autres soumissions.

---

## 7. Tables métiers & Custom Tables

### US-CT-001 – Créer une Custom Table métier
**En tant que** Configurator  
**Je veux** créer une **Custom Table** (ex : Prix_Profiles_Speciaux, Taux_Installation) avec des colonnes dynamiques  
**Afin de** stocker des données métier utilisées par les formules de costing.

**Critères d’acceptation :**
- Je peux définir le nom de la table et les colonnes (nom, type de données).
- Je peux ajouter, modifier, supprimer des lignes de données.
- La table est disponible dans le DSL via des fonctions type LOOKUP / FILTER.

---

### US-CT-002 – Utiliser une Custom Table dans une formule
**En tant que** Configurator  
**Je veux** pouvoir écrire des formules qui lisent dans les Custom Tables  
**Afin de** calculer les coûts et coefficients en fonction de codes, types, zones, dates, etc.

**Critères d’acceptation :**
- Je peux utiliser des fonctions comme `LOOKUP("Prix_Profiles_Speciaux", CodeProfile = ProfileCode AND Finition = SelectedFinish)`.
- Le moteur de formule résout ces lookups de façon sécurisée, sans exposer SQL.
- Les erreurs (clé introuvable, condition ambiguë) sont remontées clairement dans l’UI.

---

## 8. Workflows d’automation

### US-AF-001 – Créer un Flow d’automation pour les soumissions
**En tant que** Configurator  
**Je veux** créer un **Flow** d’automation basé sur des triggers (ex : soumission acceptée, statut changé) et des actions (ex : envoyer un email, créer un document, notifier quelqu’un)  
**Afin de** automatiser les tâches récurrentes autour des soumissions.

**Critères d’acceptation :**
- Je peux choisir un trigger (OnBidCreated, OnBidStatusChanged, etc.).
- Je peux enchaîner plusieurs actions (SendEmail, CreateTask, UpdateBidStatus, etc.).
- Je peux activer/désactiver un Flow.
- L’exécution des flows est tracée (log minimal).

---

### US-AF-002 – Envoyer automatiquement un email à l’acceptation
**En tant que** Configurator  
**Je veux** configurer un Flow qui envoie automatiquement un email de confirmation lorsque la soumission passe au statut **Accepted**  
**Afin de** sécuriser le processus de communication avec le client.

**Critères d’acceptation :**
- Lorsque le statut passe à Accepted, la Flow Action “SendEmail” utilise le MailProfile de l’organisation.
- Le PDF de la soumission acceptée est joint automatiquement.
- Un log d’email est créé et lié à la soumission.

---

## 9. Workflow de soumission (Bid Lifecycle)

### US-BD-001 – Gérer le cycle de vie d’une soumission
**En tant que** Estimator / Estimator Senior  
**Je veux** voir et modifier le **statut** d’une soumission parmi : Draft, PendingApproval, Sent, Negotiation, Accepted, Rejected, Expired  
**Afin de** refléter la réalité du processus de vente.

**Critères d’acceptation :**
- Les statuts disponibles sont limités et cohérents (pas de transitions impossibles).
- Certaines transitions sont limitées par le rôle (ex : seul Estimator Senior/Configurator/Owner peut passer en Sent).
- Le statut est historisé (qui a changé, quand, de quoi à quoi).

---

### US-BD-002 – Soumettre une Bid pour validation interne
**En tant que** Estimator  
**Je veux** pouvoir soumettre une soumission en statut **PendingApproval**  
**Afin de** la faire vérifier par un Estimator Senior / Configurator / Owner avant envoi au client.

**Critères d’acceptation :**
- Je peux déclencher la demande de validation uniquement depuis Draft / Negotiation.
- La soumission passe en PendingApproval.
- Les utilisateurs habilités peuvent approuver ou rejeter la soumission.

---

### US-BD-003 – Créer une révision (R01, R02…) d’une soumission
**En tant que** Estimator Senior  
**Je veux** créer une nouvelle révision d’une soumission existante (R00 → R01 → R02)  
**Afin de** proposer une version ajustée au client tout en conservant l’historique.

**Critères d’acceptation :**
- Une nouvelle révision est créée en copiant la précédente.
- La révision reçoit un identifiant (R01, R02, etc.).
- La précédente révision reste consultable mais verrouillée en modification.
- Le PDF généré est lié à la bonne révision.

---

### US-BD-004 – Gérer des options et alternatives
**En tant que** Configurator / Estimator Senior  
**Je veux** définir des **options** (A, B, C…) regroupées en **OptionGroups** (Exclusives ou Cumulatives)  
**Afin de** proposer des scénarios alternatifs au client sans chevauchement incohérent.

**Critères d’acceptation :**
- Un OptionGroup peut être de type Exclusive (choix unique) ou Cumulative (choix multiples).
- Le système empêche la sélection de plusieurs options exclusives du même groupe.
- Chaque option calcule son total de manière indépendante pour éviter les doublons.

---

## 10. Relevé sur plan PDF (Takeoff)

### US-PF-001 – Charger un plan PDF et définir une échelle
**En tant que** Estimator / Estimator Senior  
**Je veux** charger un plan PDF dans une interface de visualisation et définir l’échelle du plan  
**Afin de** effectuer des relevés de longueur, surface ou quantités directement sur le plan.

**Critères d’acceptation :**
- Je peux charger un PDF multi-pages.
- Je peux définir l’échelle via un facteur (ex : 1:50) ou en traçant une distance de référence connue.
- L’échelle est stockée pour ce plan / cette page.

---

### US-PF-002 – Créer des tags de relevé (lignes, surfaces, points)
**En tant que** Estimator / Estimator Senior  
**Je veux** dessiner des tags (polylignes, polygones, points) sur le plan  
**Afin de** mesurer des longueurs, surfaces et quantités directement sur le PDF.

**Critères d’acceptation :**
- Je peux choisir un type de tag (ligne/polyligne, surface/polygone, point/compte).
- Le système calcule la quantité automatiquement en fonction de l’échelle (m, m², pcs).
- Chaque tag a un nom, une couleur, un type de quantité et une unité.

---

### US-PF-003 – Lier un tag à une variable métier
**En tant que** Estimator / Estimator Senior  
**Je veux** lier un tag de relevé à une variable (ex : SV10.Zone1.Length)  
**Afin de** réutiliser cette quantité dans les formules de costing et les templates de soumission.

**Critères d’acceptation :**
- Je peux sélectionner une variable disponible (exposée par le système) et la relier au tag.
- La valeur de la variable est automatiquement mise à jour si le tag est modifié.
- Les formules de costing peuvent consommer ces variables comme n’importe quelle autre donnée.

---

### US-PF-004 – Naviguer vers un détail sur le plan depuis une quantité
**En tant que** Estimator / Estimator Senior  
**Je veux** pouvoir cliquer sur une quantité ou un tag dans une liste ou un rapport et être automatiquement centré sur la zone correspondante du plan PDF  
**Afin de** vérifier rapidement l’origine de la mesure.

**Critères d’acceptation :**
- Chaque tag stocke la page et une bounding box normalisée (X, Y, Width, Height).
- Depuis un tableau de quantités, je peux cliquer sur “Voir sur plan”.
- Le viewer PDF affiche la bonne page et centre/zoome sur la zone du tag, en le surlignant temporairement.

---

## 11. Dashboards & Reporting

### US-DB-001 – Créer un dashboard personnalisé
**En tant que** Configurator  
**Je veux** créer un dashboard composé de widgets (KPI, charts, tables, gauges, etc.)  
**Afin de** visualiser les informations clés pour le suivi des soumissions, projets et performances.

**Critères d’acceptation :**
- Je peux ajouter/supprimer des widgets sur un dashboard.
- Je peux définir le layout (position, taille) de chaque widget.
- Je peux enregistrer et publier le dashboard pour mon organisation.

---

### US-DB-002 – Configurer la source de données d’un widget
**En tant que** Configurator  
**Je veux** définir une **DataSource** pour chaque widget (ex : agrégation de Bids par statut, top clients, montant total gagné ce mois-ci)  
**Afin de** afficher des indicateurs pertinents sans écrire de SQL brut.

**Critères d’acceptation :**
- Je peux choisir l’entité (Bids, Projects, Clients, etc.).
- Je peux définir des filtres (statut, dates, rôle, client).
- Je peux définir le type d’agrégation (COUNT, SUM, GROUP BY).
- Le moteur de requête garantit la sécurité (filtrage par organisation, pas de SQL direct).

---

### US-DB-003 – Gérer la visibilité des widgets par rôle
**En tant que** Configurator  
**Je veux** contrôler quels rôles peuvent voir chaque widget (Owner, Configurator, EstimatorSenior, Estimator, Viewer)  
**Afin de** exposer les bonnes informations au bon niveau de responsabilité.

**Critères d’acceptation :**
- Je peux définir une liste de rôles autorisés par widget.
- Lors de l’affichage du dashboard, seuls les widgets accessibles au rôle de l’utilisateur sont rendus.
- Les widgets contenant des données sensibles (marge, coût détaillé) peuvent être limités aux rôles supérieurs.

---
