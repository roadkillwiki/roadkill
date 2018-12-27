# Common issues

## Unable to load DLL 'SQLite.Interop.dll': The specified module could not be found. (Exception from HRESULT: 0x8007007E)

Copy this DLL from app_data folder to the bin folder.

## Search doesn't return the latest pages
Occasionally the Lucene index gets out of sync with the site from an error or other reason. This can also happen when a page is deleted - the whole index is updated when a page is created or deleted. If this happens you should login as the administrator for the site and head to Site settings->Tools and use the "Rebuild search index" option.

This is the best thing to do for any search related problems, as it 99% of the time it will resolve the issue you're having.