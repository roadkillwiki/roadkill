# Passer de Roadkill 2.x (.NET Framework) à Roadkill 3 (.NET 10)

*[English version](upgrade-v2-to-v3.md)*

La base de données **ne change pas** (mêmes tables, mêmes comptes, mêmes mots de passe) : il suffit de déployer la nouvelle version, d'y recopier quelques fichiers et de convertir la configuration.

## 1. Prérequis

- Sauvegarder la base de données et le dossier du site v2.
- Sur le serveur IIS : installer le **ASP.NET Core 10 Hosting Bundle**, et configurer le pool d'applications en **« Aucun code managé »**.
- Bases supportées : **SQL Server** et **Postgres** (testés). MongoDB est conservé : ses dépôts passent les tests d'intégration, mais un wiki sous MongoDB n'a **pas été essayé**. MySQL, SQLite/SQL CE et l'authentification Windows/AD ne sont plus supportés.

## 2. Publier la nouvelle version

```
dotnet publish src/Roadkill.Web -c Release -o <nouveau dossier du site>
```

Déployer dans un **nouveau dossier** (ne pas écraser le site v2). Le `web.config` IIS est généré par la publication : ne pas remettre l'ancien.

## 3. Convertir la configuration (`web.config` → `appsettings.json`)

```
dotnet run tools/ConvertWebConfig.cs -- <ancien site>\web.config <nouveau site>\appsettings.json
```

(ou `tools/ConvertWebConfig.linq` dans LINQPad). Le script lit la section `roadkill`, la chaîne de connexion (y compris `Roadkill.config` / `connectionStrings.config` via `configSource`), la langue (`uiCulture`) et le SMTP, puis écrit `ConnectionStrings:Roadkill` et la section `Roadkill` sans toucher au reste du fichier.

⚠️ **SQL Server** : le nouveau pilote chiffre la connexion par défaut. Sans certificat reconnu, ajouter `TrustServerCertificate=true` à la chaîne de connexion (le script le signale).

## 4. Dossiers et fichiers à recopier depuis le site v2

L'arborescence est la même qu'en v2 (à la racine du site publié) : **aucun dossier n'est à déplacer**, seulement à recopier.

| Depuis le site v2 | Vers le nouveau site | Quand |
|---|---|---|
| `App_Data\Attachments\` | `App_Data\Attachments\` | Toujours (pièces jointes). Si `attachmentsFolder` pointait ailleurs, rien à faire. |
| `App_Data\customvariables.xml` | idem | Si vous l'aviez modifié. |
| `App_Data\EmailTemplates\` | idem | Si vous aviez modifié les modèles d'e-mails. |
| `App_Data\NLog.config` | idem | Facultatif : l'ancien fonctionne sous Windows. Le nouveau écrit dans `App_Data\Logs\` via `${var:contentroot}`. |
| `App_Data\Internal\htmlwhitelist.xml` | — | **Ne pas recopier**, sauf si modifié : le nouveau autorise les balises Markdown GFM (`del`, `input`, `h6`…). Si vous l'aviez modifié, reporter vos ajouts dans le nouveau. |
| `Themes\<votre thème>\` (CSS, images) | `Themes\<votre thème>\` | Si vous avez un thème ou des CSS/images personnalisés (voir §5). |
| `Plugins\<vos plugins>\` | idem | Uniquement des plugins personnalisés (à recompiler pour .NET 10). |

Ne **pas** recopier : `bin\`, `Views\`, `App_Data\Internal\Search\` (index de recherche, à reconstruire), les `.config` XML.

## 5. Thème

Les CSS et images se reprennent tels quels. Un `Theme.cshtml` v2 doit être adapté sur quelques lignes (les « child actions » n'existent plus, et les liens vers l'accueil et la recherche doivent préciser `area = ""`, sinon ils pointent vers `/settings/...` depuis les pages de réglages) :

| v2 | v3 |
|---|---|
| `@Html.Action("LoggedInAs", "User")` | `@await Html.PartialAsync("~/Views/User/LoggedInAs.cshtml")` |
| `@Html.Action("NavMenu", "Home")` | `@Html.Raw(Context.RequestServices.GetRequiredService<Roadkill.Core.Services.IPageService>().GetMenu(RoadkillContext))` |
| `@Html.Action("BootstrapNavMenu", "Home")` | `@Html.Raw(Context.RequestServices.GetRequiredService<Roadkill.Core.Services.PageService>().GetBootStrapNavMenu(RoadkillContext))` |
| `Html.BeginForm("Search", "Home", FormMethod.Get)` | `Html.BeginForm("Search", "Home", new { area = "" }, FormMethod.Get)` |
| `Url.Action("Index", "Home")` | `Url.Action("Index", "Home", new { area = "" })` |

`@Html.Partial(...)` fonctionne encore (`@await Html.PartialAsync(...)` est recommandé). Le thème est compilé au démarrage, sans recompiler Roadkill. Les thèmes fournis (`Mediawiki`, `Responsive`…) sont déjà adaptés.

## 6. Après le premier démarrage

- **Paramètres du site > Outils > Rebuild search index** : le format de l'index Lucene a changé.
- **Paramètres du site > Plugins** : activer si besoin **Mermaid diagrams** (diagrammes ```` ```mermaid ````) et **Syntax Highlighter** (colore aussi les blocs ```` ```sql ````).
- reCAPTCHA : l'ancienne API v1 n'existe plus ; si elle était activée, saisir des clés **reCAPTCHA v2**.
- **Paramètres du site > Menu** : ajouter la ligne `* %tagswithpages%` (sous `* %categories%`) pour afficher la nouvelle page « Pages by tag » (tags avec leurs pages) ; le menu existant est conservé tel quel.

## Nouveautés Markdown (GitHub Flavored Markdown)

Tableaux en pipes, ~~barré~~, listes de tâches `- [ ]`, liens automatiques, notes de bas de page, blocs de code ```` ```langage ````, diagrammes Mermaid. La syntaxe Roadkill existante reste valable : `#Titre#` sans espace, `![img](a.png =250x100)`, liens internes `[texte](Ma-page)`, `[[[code lang=sql|...]]]`, `{TOC}`, jetons `customvariables.xml`.
