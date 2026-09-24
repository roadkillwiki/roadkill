# Migration vers .NET 10 + Markdown GFM — suivi

Ce fichier trace les décisions et l'avancement de la migration (branche `claude/migration_net10`).

## Décisions validées

| Sujet | Décision |
|---|---|
| Framework cible | .NET 10 (LTS), ASP.NET Core MVC |
| Interface | Vues Razor portées en ASP.NET Core MVC (pas de SPA) |
| Accès aux données | **Dapper** (dépôts déjà présents mais non branchés) pour SQL Server (priorité) et Postgres ; MongoDB conservé (driver 3.x) |
| Supprimé | ORM LightSpeed (propriétaire, .NET Framework uniquement), MySQL, authentification Windows / Active Directory |
| Configuration | `web.config` remplacé par `appsettings.json` (section `Roadkill`, réécrite par l'installeur) |
| Markdown | MarkdownSharp remplacé par **Markdig** + extensions GitHub Flavored Markdown (tableaux en pipes, etc.) |
| Mermaid | Support des blocs ```` ```mermaid ```` |

## Exigences de compatibilité Markdown (existant à conserver)

Inventaire fait sur le parseur actuel (`Text/Parsers/Markdown.cs`, MarkdownSharp modifié) et les plugins :

- Événements `LinkParsed` / `ImageParsed` (liens internes wiki, `attachment:`, `Special:`, `~/`, classe `missing-page-link`, `rel="nofollow"` sur les liens externes).
- Images : `class="img-responsive"`, syntaxe de taille Roadkill `![alt](url =250x350)` / `=250x`.
- Liens internes avec `-` à la place des espaces : `[texte](Ma-page)`.
- Plugin SyntaxHighlighter : `[[[code lang=sql|...]]]` (trois crochets).
- Jetons personnalisés `customvariables.xml` : `(warningbox:...)`, `(infobox:...)`, etc.
- Plugins texte : `{TOC}`, Jumbotron, MathJax, images cliquables, liens externes en nouvelle fenêtre…
- Titres ATX sans espace (`#Titre`) acceptés par l'ancien parseur.
- HTML brut autorisé (filtré ensuite par la liste blanche HtmlSanitizer).

## Avancement

- [x] Inventaire du code et des dépendances
- [x] SDK .NET 10, suppression LightSpeed / MySQL / AD
- [x] Couche données Dapper (SQL Server, Postgres) + MongoDB (conservé ; dépôts couverts par les tests d'intégration, wiki sous MongoDB **non essayé**)
- [x] Infrastructure Core (appsettings.json, DI, cookies, cache, Lucene.Net 4.8, fichiers, SMTP, NLog)
- [x] Contrôleurs, filtres, vues Razor, hôte web, API REST + Swagger
- [x] Parseur Markdig + GFM + plugin Mermaid, tests de non-régression
- [x] Tests portés (NUnit 4) : 672 réussis, 2 ignorés (spécifiques Windows / connus) au dernier passage complet
- [x] Tests d'intégration autonomes : SQL Server, Postgres et MongoDB démarrés dans des conteneurs jetables (Testcontainers,
  Docker ou podman machine), sans variable d'environnement ni secret ; SGBD à tester dans `src/Roadkill.Tests/appsettings.json`
- [x] Mode d'emploi de migration : [docs/migration-v2-vers-v3.md](docs/migration-v2-vers-v3.md), version anglaise [docs/upgrade-v2-to-v3.md](docs/upgrade-v2-to-v3.md), script `tools/ConvertWebConfig.cs` / `.linq`
- [x] CI : GitHub Actions (`.github/workflows/ci.yml`), AppVeyor/Travis et scripts de build .NET Framework supprimés
- [x] Solution au format `Roadkill.slnx`
- [x] `Assets/Scripts/roadkill.js` (bibliothèques + TypeScript compilé) régénéré à chaque build par une cible MSBuild
  (remplace la tâche Grunt de la v2, qui ne tournait plus : le fichier versionné de la v2 était servi tel quel) ;
  `gruntfile.js`/`package.json` supprimés. Le SCSS n'est plus compilé : `roadkill.css` reste le fichier versionné.
- [x] Page d'édition : les scripts des plugins (MathJax, Mermaid, coloration syntaxique) sont chargés et l'aperçu est
  re-rendu après chaque mise à jour (ne fonctionnait pas non plus en v2)
- [x] MathJax servi par l'application (MathJax 3.2.2 dans `Plugins/MathJax`, ~1,6 Mo) au lieu du CDN `cdn.mathjax.org`
  (MathJax 2) ; mêmes délimiteurs. En Markdown, `\(...\)` perd son `\` (échappement Markdown, déjà le cas en v2) :
  écrire `\\(...\\)` pour une formule en ligne. Plus aucun CDN utilisé.

## Vérifications effectuées

- Base SQL Server 2022 créée avec le script v2 d'origine (`lib/Test-databases/roadkill-sqlserver.sql`) et branchée **sans installeur** :
  connexion avec les comptes v2 (hachages SHA1 identiques), affichage/édition de pages, historique, pièces jointes, recherche, administration.
- Tests d'intégration Dapper sur SQL Server 2022 et Postgres 16 (94 tests).
- Parcours navigateur (Playwright/Chromium) : tableaux GFM, cases à cocher, Mermaid (SVG), coloration syntaxique, gestionnaire de fichiers, sans erreur JavaScript.
- Version publiée (`dotnet publish`) : thème personnalisé déposé dans `Themes/` compilé à la volée.

## Non vérifié

- Wiki complet sous MongoDB (seuls ses dépôts sont testés), stockage Azure Blob, envoi SMTP réseau, reCAPTCHA v2, hébergement IIS réel sous Windows.
- Podman machine sous Windows : les tests passent avec podman 4.9 sous Linux (API compatible Docker), pas avec une podman machine Windows.

## MongoDB : bugs trouvés par les tests d'intégration (corrigés)

- Régression de la migration : les nouveaux documents dont l'identifiant était vide (ex. nouvel utilisateur) n'en recevaient pas,
  et s'écrasaient donc les uns les autres (l'ancien `MongoCollection.Save` le générait).
- Hérité de la v2 : `GetUserByEmail(email, isActivated)` comparait avec `isActivated.HasValue` au lieu de `isActivated.Value`.
- `GetPageByTitle` sensible à la casse (contrairement à SQL Server et Postgres).
- Installation : une chaîne de connexion invalide levait `ArgumentNullException` au lieu de `DatabaseException` lors de la création de l'admin.

Consigne : pas d'effort supplémentaire sur MongoDB pour l'instant, hormis des corrections très simples ; consigner ici les bugs constatés.

## État au 24/09/2026 (fin de session)

- Branche `claude/migration_net10`, PR ouverte : https://github.com/AFract/roadkill-fork/pull/1 (non fusionnée ; l'utilisateur
  teste la migration sur ses données avant). CI GitHub Actions verte au dernier passage vérifié. Aucune surveillance active.
- Tout est commité et poussé ; rien en cours.

### Corrigé suite aux essais de l'utilisateur (après l'ouverture de la PR)

- Avertissements de compilation (0 restant) ; `WikiController.NotFound` renommé `PageNotFound` (masquait `ControllerBase.NotFound`) ;
  `RepositoryInfo` : comparaisons avec `null`.
- Nom de base `SqlServer2008` → `SqlServer` (SQL Server 2022 minimum) ; les anciens noms (SqlServer2008/2012, SqlAzure) restent lus.
  `Drop.sql` SQL Server : `DROP TABLE IF EXISTS`.
- `tools/ConvertWebConfig` (.cs/.linq) : écrit `appsettings.json` dans le dossier du script, affiche son chemin, liste les XML lus
  (supprimables) et les fichiers à reprendre du site v2.
- MathJax / Mermaid / coloration syntaxique dans l'aperçu de la page d'édition ; `roadkill.js` régénéré au build (voir plus haut).
- Page « Markup help » complétée : GFM, liens (page, web, `attachment:`, `~/`, `Special:`, `mailto:`), images (taille, lien),
  plugins, langages de la coloration syntaxique (liste v2 + alias).
- Message « Unable to save the page: » affiché à tort (résumé de validation rendu par ASP.NET Core même sans erreur).
- Outils : exports zip (`PhysicalFile` au lieu de `File`) ; `ActionLink(texte, action, null, new { @class })` interprété
  différemment par ASP.NET Core (classe passée en paramètre d'URL) : 8 appels corrigés.

### Propositions en attente d'une décision de l'utilisateur (rien n'est fait)

- Schéma SQL Server : contraintes `UNIQUE` sur `roadkill_users.Email`/`Username` (+ script optionnel pour une base existante) ;
  `datetime2` jugé sans intérêt (divergence avec les bases migrées).
- Coloration syntaxique : SyntaxHighlighter 3.0.83 (25 langages, liste figée) ; d'autres langages (JSON, YAML, TypeScript, Go…)
  demanderaient de changer de bibliothèque (highlight.js, Prism…).
- MathJax : ajouter `$…$` comme délimiteur en ligne (risque : un `$` isolé dans le texte).
- Page d'aide : n'afficher que les plugins activés.
- Exports zip : supprimer le fichier de `App_Data/Export` après téléchargement (ils s'accumulent, comme en v2).

### Pistes connues non traitées

- Pièges de migration MVC 5 → ASP.NET Core du même type que ceux corrigés ci-dessus (surcharges `ActionLink`, `File(...)`,
  rendu des helpers) : d'autres cas peuvent subsister dans des écrans peu utilisés ; les signaler au fil des essais.
- Le SCSS n'est plus compilé (`roadkill.css` versionné) : à traiter seulement si le SCSS doit évoluer.

### Consignes de l'utilisateur (à respecter)

- Répondre en français ; distinguer faits et hypothèses ; indiquer « Niveau de confiance » après une vérification.
- Poser des questions pour les arbitrages plutôt que prendre des initiatives.
- Pas de dépendance à des variables d'environnement ni à des secrets utilisateur.
- MongoDB : pas d'effort supplémentaire (corrections très simples seulement), consigner les bugs.
- Ne pas relancer toute la suite de tests pour des modifications minimes ou hors du code testé.
- Ne pas surveiller la PR inutilement ; ne pas fusionner.

### Notes pratiques pour reprendre

- Build : `dotnet build Roadkill.slnx` ; tests : `dotnet test src/Roadkill.Tests` (Docker ou podman machine requis pour
  les tests d'intégration ; Docker Hub peut limiter les téléchargements d'images, erreur 429).
- Lancer le site localement : renseigner `ConnectionStrings:Roadkill` et `Roadkill:Installed=true` dans
  `src/Roadkill.Web/appsettings.json` (ne pas commiter ces valeurs), puis `dotnet run --project src/Roadkill.Web`.
  Comptes de la base de test v2 (`lib/Test-databases/roadkill-sqlserver.sql`) : `admin@localhost` / `password`.
