# Themes

## Introduction
Roadkill supports a fairly comprehensive theming framework that allows you to completely re-skin your wiki if needed. The Roadkill installation comes with four themes: Responsive (default), Mediawiki, Blackbar and Plain. The Responsive theme has been tested across devices while the others are intended for desktop use.

In order to create a theme for Roadkill, you will need to understand the ASP.NET Razor templating language.

## Creating a theme in 3 steps

1. **Create the theme folder**.
The first step for theme creation is to create a new folder under the "Themes" folder. Name this folder with the name of your theme, without spaces.

2. **Copy an existing theme across**.
It's easiest to get started by simply copying the "Theme.cshtml" and "Theme.css" files from the "Blackbar" theme folder to your new theme folder. This way you a skeleton to work with, and can just strip out the parts you don't want.

3. **Edit the theme.css**.
Roadkill uses Bootstrap 3 - so any class or style available in Bootstrap can be used in your theme. Theme specific styles are declared inside the theme's "Theme.css" file which can include font styles, heading styles, anchor styles and tag cloud styles.

## Built in classes
.searchresult, #historytable, #tagcloud, .wikitable

## Razor sections
The following razor sections should be declared in your theme for it to be compatible with text plugins:

- **PluginPreContainer:** renders any HTML that plugins provide prior to the container div.
- **PluginPostContainer:** renders any HTML that plugins provide after the container div.

## Action methods
To render the menu bar, you should use the NavMenu or BootStrapNavMenu action methods:

```
@Html.Action("BootstrapNavMenu", "Home")
```

You can of course create your own menu if needed, but you will need to render the urls manually.

To render the login status, you should use the 'LoggedInAs' action method:

```
@Html.Action("LoggedInAs", "User")
```

## Extension methods
There are a number of extension method to the HtmlHelper and UrlHelper classes that Roadkill provides. You should automatically pick the intellisense up for these in Visual Studio when you create a theme, from the configuration in the web.config file.

Your view will inherit from `RoadkillView' which gives you a few extra properties you can use inside your theme:

- ApplicationSettings - application settings.
- RoadkillContext - the currently logged in user
- MarkupConverter - the convert for the current markup type.
- SettingsService - a service for retrieve the site preferences.