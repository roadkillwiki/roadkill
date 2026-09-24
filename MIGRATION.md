# Migration vers .NET 10 + Markdown GFM — suivi

Ce fichier trace les décisions et l'avancement de la migration (branche `claude/keen-clarke-0gumj5`).

## Décisions validées

| Sujet | Décision |
|---|---|
| Framework cible | .NET 10 (LTS), ASP.NET Core MVC |
| Interface | Vues Razor portées en ASP.NET Core MVC (pas de SPA) |
| Accès aux données | **Dapper** (dépôts déjà présents mais non branchés) pour SQL Server (priorité) et Postgres ; MongoDB conservé (driver 3.x) |
| Supprimé | ORM LightSpeed (propriétaire, .NET Framework uniquement), MySQL, authentification Windows / Active Directory |
| Configuration | `web.config` remplacé par `roadkill.json` (réécrit par l'installeur) |
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
- [x] Couche données Dapper (SQL Server, Postgres) + MongoDB (conservé, **non testé**)
- [x] Infrastructure Core (appsettings.json, DI, cookies, cache, Lucene.Net 4.8, fichiers, SMTP, NLog)
- [x] Contrôleurs, filtres, vues Razor, hôte web, API REST + Swagger
- [x] Parseur Markdig + GFM + plugin Mermaid, tests de non-régression
- [x] Tests portés (NUnit 4) : 618 réussis, 49 ignorés (MongoDB, test spécifique Windows)
- [x] Mode d'emploi de migration : [docs/migration-v2-vers-v3.md](docs/migration-v2-vers-v3.md), script `tools/ConvertWebConfig.cs` / `.linq`
- [ ] CI : `appveyor.yml` et `.travis.yml` ciblent encore .NET Framework (à remplacer ou supprimer)

## Vérifications effectuées

- Base SQL Server 2022 créée avec le script v2 d'origine (`lib/Test-databases/roadkill-sqlserver.sql`) et branchée **sans installeur** :
  connexion avec les comptes v2 (hachages SHA1 identiques), affichage/édition de pages, historique, pièces jointes, recherche, administration.
- Tests d'intégration Dapper sur SQL Server 2022 et Postgres 16 (94 tests).
- Parcours navigateur (Playwright/Chromium) : tableaux GFM, cases à cocher, Mermaid (SVG), coloration syntaxique, gestionnaire de fichiers, sans erreur JavaScript.
- Version publiée (`dotnet publish`) : thème personnalisé déposé dans `Themes/` compilé à la volée.

## Non vérifié

- MongoDB, stockage Azure Blob, envoi SMTP réseau, reCAPTCHA v2, hébergement IIS réel sous Windows.
