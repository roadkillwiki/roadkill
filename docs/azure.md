# Azure website deployments
If you have a Microsoft Azure account, you can deploy a new Roadkill wiki site very quickly using the website deployment option.

## Pre-requisites
- You'll need a Githubaccount before you can use the deployment option in Azure.
- You'll need to fork Roadkill on either Github or Bitbucket. You can do this via the website. You should fork the "v2.0" branch for stability, or "master" if you prefer the bleeding edge.

## Deployment steps:
1. Create a new website
2. Go to the "configure" page in the top menu
3. Navigate to the "app settings" section
4. Login using your Github details
5. Choose Roadkill from your list of repositories.
6. You will need to FTP (username/password is in the publish settings file) and download the roadkill.config file. Change the "installed=true" to "installed=false" in this file.
This setting is true by default to avoid automatic deployments from showing their installer screen when a new commit is made to Roadkill.

You will need to create a SQL or mySQL database in Azure too, copying the connection string details.