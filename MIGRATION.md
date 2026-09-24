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
- [ ] Couche données Dapper + MongoDB
- [ ] Infrastructure Core (config, DI, auth cookies, cache, Lucene.Net 4.8, fichiers)
- [ ] Contrôleurs, filtres, vues Razor, hôte web
- [ ] Parseur Markdig + GFM + Mermaid, tests de non-régression
- [ ] Portage des tests, exécution
- [ ] Documentation, CI

**État actuel : travail en cours — la solution ne compile pas encore.**
