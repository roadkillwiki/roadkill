---- Mono support ----

NOTE: the instructions below are for testing/development boxes. You should harden the security of your machine if
you are intending to use it for anything other than testing and development.

Roadkill works with Mono 2.10.8.1 on Ubuntu 12.10 x64. Use MongoDB, mySQL or Postgres for the easiest friction free install
- MongoDB is lightening fast and the only one tested so far.

### Configuring the Roadkill build
In order to use it, there are a couple of things you will need to do:

- Publish the project using the Mono configuration.
- Delete Microsoft.Web.Infrastructure.dll, Microsoft.Web.Administration.dll from the bin folder.
- Delete any .txt files from the root folder (they interfere with the MVC RouteTables).

- SQL Server, SQL Server Express, SQL Server CE are obviously not supported.
- SQLite isn't supported, as it would required 3 different binaries to float around.
- Copy lib/LightSpeed/Mindscape.LightSpeed.MetaData.dll to the bin folder
- Copy lib/LightSpeed/Providers/log4net.dll to the bin folder
- Copy lib/LightSpeed/Providers/Memcached.ClientLibrary.dll to the bin folder

### Setting up the server
Run the following commands:

sudo apt-get update
sudo apt-get install ufw
sudo ufw enable
sudo ufw allow 22
sudo ufw allow 80
sudo ufw allow proto tcp to any port 135
sudo ufw allow proto udp to any port 137
sudo ufw allow proto udp to any port 138
sudo ufw allow proto tcp to any port 139
sudo ufw allow proto tcp to any port 445

### Installing Mono
sudo apt-get install apache2 libapache2-mod-mono mono-apache-server4  mono-devel mono-runtime
nano /etc/apache2/sites-available/default

Replace the text with the following config, which assumes you have no hosts on your server and are
using it to run a single site from /var/www/. Using a hostname is bit more complicated.

<VirtualHost *:80>
        ServerAdmin webmaster@localhost
        AddHandler mono .aspx ascx .asax ashx .config .cs asmx .axd
        MonoDebug montest true
        MonoSetEnv default MONO_IOMAP=all
        MonoApplications "/:/var/www"
        MonoServerPath default /usr/bin/mod-mono-server4

        DocumentRoot /var/www
        <Directory />
                Options FollowSymLinks
                AllowOverride None
        </Directory>
        <Directory /var/www/>
                Options Indexes FollowSymLinks MultiViews
                AllowOverride None
                Order allow,deny
                allow from all
                SetHandler mono
        </Directory>

        <DirectoryMatch "/(bin|App_Code|App_Data|App_GlobalResources|App_LocalResources)/">
                Order deny,allow
                Deny from all
        </DirectoryMatch>

        ScriptAlias /cgi-bin/ /usr/lib/cgi-bin/
        <Directory "/usr/lib/cgi-bin">
                AllowOverride None
                Options +ExecCGI -MultiViews +SymLinksIfOwnerMatch
                Order allow,deny
                Allow from all
        </Directory>

        ErrorLog ${APACHE_LOG_DIR}/error.log

        # Possible values include: debug, info, notice, warn, error, crit,
        # alert, emerg.
        LogLevel warn

        CustomLog ${APACHE_LOG_DIR}/access.log combined
</VirtualHost>

sudo /etc/init.d/apache2 restart

### Installing MongoDB:
Run the following commands:

sudo apt-get install mongodb
mongo
use local
db.addUser("roadkill","password");
exit

Then use "mongodb://roadkill:password@localhost/local" for the connection string.


### Uploading/FTPing
Download WinScp if you're on Windows
Upload the Roadkill mono directory to /var/www

Make sure the web.config has the correct permissions to be written to, e.g. chmod 777 web.config.
Do the same for the App_Data folder.

Navigate to your site - you should now get the installer screen.

### Known issues

* Sometimes the installer gets stuck (from what looks like a cache issue with Mono). Restart Apache to get around this:
sudo /etc/init.d/apache2 restart


### References
A lot of help came from:
http://www.toptensoftware.com/Articles/6/Setting-up-a-Ubuntu-Apache-MySQL-Mono-ASP-NET-MVC-2-Development-Server
http://devblog.rayonnant.net/2012/11/mvc3-working-in-mono-ubuntu-1210.html