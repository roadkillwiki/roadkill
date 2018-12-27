# Searching

## Introduction
The Roadkill search engine is powered by Lucene.net. The index is updated each time a page is created or edited. Roadkill indexes the full content of the page along with the following fields:

- id (the unique number each page is assigned)
- content
- title
- tags
- createdby (the username of the person who created the page)
- createdon (the date the page was created)

The search engine displays the first 150 characters of each page's content to display in the search summary. When you search in the textbox, the content field is searched by default. You can restrict the page to certain fields using the usual syntax found on search engines like as Google. For example:

- title:"my page"
- tags:dotnet, vb createdby:editor

## Index storage
Lucene stores index files for its search data in the App_Data/Search folder. The worker process/application pool will have rights to read and write from this folder by default, so there shouldn't be any issues with permissions providing the website root folder permissions are setup correctly.

The folder will contains 10 or more files in it, which Lucene may or may not contain file locks for while the site is running.