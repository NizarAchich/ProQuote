# Routing – ProQuote

Ce document définit les conventions de **routing** pour l’application ProQuote, côté **Front (Blazor)** et côté **Backend (API HTTP)**, dans le contexte multi-organisation avec rôles.

Objectifs :

- URLs **stables, lisibles et partageables**.
- Routing compatible avec notre architecture **HTTP + SignalR**.
- Sécurité basée sur le **serveur**, pas sur la “discrétion” des URLs.
- Support du **multi-tenant** (plusieurs organisations) dans les routes.

---

## 1. Principes généraux

1. **L’URL représente la ressource ou l’écran**, pas la logique de sécurité.
2. La **sécurité est toujours validée côté Backend** :
   - authentification (qui est l’utilisateur),
   - autorisation (rôle dans l’organisation, ownership),
   - appartenance de la ressource à l’organisation.
3. Les routes incluent toujours un contexte d’**organisation** :
   - soit un `organizationId` (GUID),
   - soit un `orgSlug` lisible.
4. Les routes Front (Blazor) sont alignées avec les routes Backend (API) :
   - `/org/{orgId}/bids/{bidId}` (front)
   - `/api/org/{orgId}/bids/{bidId}` (backend)
5. Le routing doit être **prévisible** :
   - facile à comprendre pour un développeur,
   - documentable (README, Swagger).

---

## 2. Conventions de base

### 2.1. Préfixes principaux

- **Front Blazor :**

  - Base : `/`
  - Espace organisation : `/org/{orgId}` ou `/org/{orgSlug}`

- **API Backend :**

  - Base : `/api`
  - Espace organisation : `/api/org/{orgId}`

### 2.2. Identifiants

- `orgId` : GUID (ou slug lisible plus tard).
- `projectId`, `bidId`, `planId`, etc. : GUID.
- Les IDs ne sont jamais “devinés” côté client : ils proviennent toujours des données récupérées via l’API.

---

## 3. Routing côté Front (Blazor)

Les pages Blazor utilisent l’attribut `@page` pour définir leurs routes.

### 3.1. Authentification & sélection d’organisation

- Page de login :

  ```razor
  @page "/login"
  ```

- Page de sélection d’organisation (si l’utilisateur a plusieurs memberships) :

  ```razor
  @page "/select-organization"
  ```

  - Charge la liste des organisations via `/api/me/organizations`.
  - Quand l’utilisateur choisit une orga, on le redirige vers :  
    `/org/{orgId}/dashboard`.

---

### 3.2. Dashboard

- Dashboard principal de l’organisation :

  ```razor
  @page "/org/{organizationId:guid}/dashboard"
  ```

  - Charge les widgets du dashboard.
  - Se connecte au `DashboardHub` (SignalR) pour les mises à jour live.

---

### 3.3. Clients & Projets

- Liste des clients :

  ```razor
  @page "/org/{organizationId:guid}/clients"
  ```

- Détail d’un client + liste de projets :

  ```razor
  @page "/org/{organizationId:guid}/clients/{clientId:guid}"
  ```

- Liste des projets :

  ```razor
  @page "/org/{organizationId:guid}/projects"
  ```

- Détail d’un projet :

  ```razor
  @page "/org/{organizationId:guid}/projects/{projectId:guid}"
  ```

---

### 3.4. Soumissions (Bids)

- Liste des Bids (pipeline) :

  ```razor
  @page "/org/{organizationId:guid}/bids"
  ```

- Détail d’une Bid (vue générale, toutes révisions) :

  ```razor
  @page "/org/{organizationId:guid}/bids/{bidId:guid}"
  ```

- Vue d’une révision spécifique :

  ```razor
  @page "/org/{organizationId:guid}/bids/{bidId:guid}/revisions/{revisionId:guid}"
  ```

- Optionnel : vue des options d’une révision :

  ```razor
  @page "/org/{organizationId:guid}/bids/{bidId:guid}/revisions/{revisionId:guid}/options"
  ```

---

### 3.5. Templates (Form / Costing / Submission)

- Liste des FormTemplates :

  ```razor
  @page "/org/{organizationId:guid}/templates/forms"
  ```

- Édition d’un FormTemplate existant :

  ```razor
  @page "/org/{organizationId:guid}/templates/forms/{formTemplateId:guid}"
  ```

- Liste des CostingTemplates :

  ```razor
  @page "/org/{organizationId:guid}/templates/costing"
  ```

- Liste des SubmissionTemplates :

  ```razor
  @page "/org/{organizationId:guid}/templates/submissions"
  ```

Etc.

---

### 3.6. Custom Tables

- Liste des tables :

  ```razor
  @page "/org/{organizationId:guid}/custom-tables"
  ```

- Édition d’une table :

  ```razor
  @page "/org/{organizationId:guid}/custom-tables/{tableId:guid}"
  ```

---

### 3.7. Takeoff PDF

- Liste des plans d’un projet :

  ```razor
  @page "/org/{organizationId:guid}/projects/{projectId:guid}/plans"
  ```

- Vue takeoff d’un plan :

  ```razor
  @page "/org/{organizationId:guid}/projects/{projectId:guid}/plans/{planId:guid}/takeoff"
  ```

  - Charge le PDF (via API/stockage).
  - Charge les tags `TakeoffTag` via `/api/org/{orgId}/projects/{projectId}/plans/{planId}/tags`.
  - Se connecte au `TakeoffHub` si mode collaboratif temps réel.

---

### 3.8. Automation

- Liste des flows d’automation :

  ```razor
  @page "/org/{organizationId:guid}/automation/flows"
  ```

- Édition d’un flow :

  ```razor
  @page "/org/{organizationId:guid}/automation/flows/{flowId:guid}"
  ```

- Logs d’exécution d’un flow :

  ```razor
  @page "/org/{organizationId:guid}/automation/flows/{flowId:guid}/logs"
  ```

---

### 3.9. Administration d’organisation

- Gestion des membres & rôles :

  ```razor
  @page "/org/{organizationId:guid}/admin/members"
  ```

- Configuration du stockage :

  ```razor
  @page "/org/{organizationId:guid}/admin/storage"
  ```

- Configuration mail :

  ```razor
  @page "/org/{organizationId:guid}/admin/mail"
  ```

---

## 4. Routing côté Backend (API HTTP)

Les routes API suivent en général le schéma :

```txt
/api/org/{organizationId}/[resource]/[id]
```

L’`organizationId` peut être :

- Soit pris depuis l’URL,
- Soit inféré à partir du contexte (orga active dans le token ou le claim).

### 4.1. Exemples de routes API

- Bids :

  - `GET    /api/org/{orgId}/bids`
  - `GET    /api/org/{orgId}/bids/{bidId}`
  - `POST   /api/org/{orgId}/bids`
  - `PUT    /api/org/{orgId}/bids/{bidId}`
  - `POST   /api/org/{orgId}/bids/{bidId}/status`

- Templates :

  - `GET    /api/org/{orgId}/templates/forms`
  - `GET    /api/org/{orgId}/templates/forms/{formTemplateId}`
  - `POST   /api/org/{orgId}/templates/forms`

- Takeoff :

  - `GET    /api/org/{orgId}/projects/{projectId}/plans/{planId}/tags`
  - `POST   /api/org/{orgId}/projects/{projectId}/plans/{planId}/tags`
  - `PUT    /api/org/{orgId}/projects/{projectId}/plans/{planId}/tags/{tagId}`

- Automation :

  - `GET    /api/org/{orgId}/automation/flows`
  - `GET    /api/org/{orgId}/automation/flows/{flowId}`
  - `POST   /api/org/{orgId}/automation/flows`
  - `POST   /api/org/{orgId}/automation/flows/{flowId}/run` (exécution manuelle)

---

## 5. Navigation & sécurité

### 5.1. Règles de sécurité

1. **Ne jamais faire confiance à l’URL seule** :
   - L’URL indique seulement ce que l’utilisateur ESSAIE de voir.
   - Le backend décide s’il en a le droit.

2. **Toujours vérifier côté Backend** :
   - L’utilisateur est membre de l’organisation (`Membership`).
   - Le rôle (`OrganizationRole`) autorise l’accès à la ressource / action.
   - La ressource (Bid, Project, Plan…) appartient bien à cette organisation.

3. **Utiliser les attributs `[Authorize]` et les policies dans ASP.NET** :
   - Exemples :
     - `[Authorize(Policy = "OrgMember")]`
     - `[Authorize(Policy = "OrgOwnerOrConfigurator")]`
     - `[Authorize(Policy = "EstimatorOrAbove")]`

4. En cas d’accès non autorisé :
   - Retourner **403 Forbidden** ou **404 NotFound**.
   - Côté Blazor, rediriger vers une page “Accès refusé” ou la page d’accueil de l’orga.

---

### 5.2. Navigation gardée côté Front

En plus de la sécurité serveur, le Front peut :

- Intercepter la navigation (`OnNavigateAsync` dans Blazor),
- Vérifier que :
  - une organisation est bien sélectionnée,
  - l’utilisateur a complété certaines étapes (ex : wizard),
- Rediriger vers la bonne page si l’état local n’est pas cohérent.

Mais cela reste un **confort utilisateur**.  
La vraie sécurité reste toujours côté Backend.

---

## 6. Résumé

- Le routage de ProQuote repose sur des **URLs structurées** avec un contexte d’organisation : `/org/{orgId}/...`.
- Le **Front Blazor** utilise ces URLs comme source de vérité pour afficher les bons écrans, tout en consommant les API HTTP et en écoutant SignalR.
- Le **Backend** expose des routes `/api/org/{orgId}/...` et contrôle la sécurité via :
  - authentification,
  - memberships,
  - rôles,
  - appartenance organisationnelle de chaque ressource.
- L’utilisateur peut taper une URL à la main, mais il n’a accès qu’aux ressources pour lesquelles il est **autorisé**.

Ce fichier `routing.md` sert de référence pour définir les routes, les conventions et les bonnes pratiques de navigation dans ProQuote.