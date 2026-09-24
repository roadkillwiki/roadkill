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
- [x] Tests portés (NUnit 4) : 667 réussis, 2 ignorés (spécifiques Windows / connus)
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
