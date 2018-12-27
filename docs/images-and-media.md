# Images and media
Roadkill allows you to upload images and other files which you can reference in your pages, or in the case of images, show on the page.

To upload files, navigate to the "Manage Files" link in the menu. For editors, this will allow you to create folders and upload files. Admin users can additionally delete files, and folders if they are empty.

## Restricting file extensions
Admins can restrict the type of files that can be uploaded in the Site Settings page by specifying the file extensions that can be uploaded, in comma separated format.

## Maximum file size
This is set to 50mb by default. If you need to increase or decrease this amount, you should edit the web.config file and look for "50mb" - you will need to change the setting in two places, one is in Kb another is in bytes.

If the file size exceeds the limit no error message is shown on the page.

## Setting up mime types for custom extensions
By default Roadkill serves files using the mime-type that is configured in IIS. If access to IIS is not available, a static list of mime types is used (the full list can be [found here](https://raw.githubusercontent.com/roadkillwiki/roadkill/master/src/Roadkill.Core/Attachments/MimeTypes.cs)).

If you need to add a file extension that doesn't have a mime type setup in IIS (for example, a .sql file), you can either set this up in IIS or inside the Roadkill web.config. An example of doing this is in the web.config file but commented out.

## Adding an image to the page
You can reference existing images by clicking on the image button, finding the image you want to show on the page and clicking the bottom right "select" button. This will display a small preview of the image first.

When you add an image, the markup will automatically be added to the editor text box for your page. You will probably need to tweak this for your needs, for details on the markup see the help for each markup type via the question mark button

## Linking to uploaded files
You can link to uploaded files by using "attachment:" or "~/" followed by the full path to the file inside the link tag. This should include a slash at the start. For example in the Creole markup:

`This is my Excel file: [[attachment:/Spreadsheets/Sheet1.xls|excel file]] and this is my [[~/Word/doc.docx|Word document]]`

## Atttachments folder
Files are uploaded to the folder you set in the installer, which you can also change as an admin under Site settings->configuration. This defaults to ~/App_Data/Attachments.

The folder can be a file share if needed, however the folder should have read and write access to the same user that the application pool for the site is running under.