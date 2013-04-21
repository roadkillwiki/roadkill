# To install this script:
#sudo wget --no-check-certificate --no-cache https://gist.github.com/yetanotherchris/5426167/raw/bd961563314748e4a9c2cdb876a9114347a45df1/monoinstall.sh
#sudo sh monoinstall.sh

# Update
sudo apt-get -y update

# Install Apache, Mono and mod_mono. 
# THIS SECTION WILL FREEZE (from a libapache2 package issue.)
# CLOSE YOUR SSH SESSION, LOG BACK IN AND RUN THE SCRIPT AGAIN.
sudo apt-get -y install apache2
sudo /etc/init.d/apache2 stop
sudo apt-get -y install mono-devel mono-runtime
sudo timeout 30s apt-get -y install mono-apache-server4
sudo timeout 30s apt-get -y install libapache2-mod-mono
sudo rm /var/www/index.html

# Get Roadkill and unzip it
sudo rm -R /var/www/roadkill/
sudo wget --no-check-certificate --no-cache https://bitbucket.org/mrshrinkray/roadkill/downloads/Roadkill.mono.1.6.zip
sudo apt-get -y install unzip
sudo unzip Roadkill.mono.1.6.zip -d /var/www

# Move roadkill to the default apache directory, update permissions for r,w,exec for user + group
sudo mv /var/www/roadkill/* /var/www/
sudo chgrp -R www-data /var/www
sudo chown -R www-data /var/www
sudo chmod -R 755 /var/www

# Replace the default apache site settings
sudo mv /var/www/default.txt /etc/apache2/sites-available/default
sudo /etc/init.d/apache2 restart

# Install MongoDB, add a user to the default collection
sudo apt-get -y install mongodb
sudo mongo local --eval "db.addUser('roadkill','password');"

# Set Roadkill as installed (web.config needs touching or apache restarting)
sudo wget --no-cache http://localhost/Install/MonoInstall
touch /var/www/Web.config

# Install UFW (uncomplicated firewall)
sudo apt-get -y install ufw

# Allow SSH and HTTP traffic
sudo ufw enable
sudo ufw allow 22
sudo ufw allow 80
sudo ufw allow proto tcp to any port 135
sudo ufw allow proto udp to any port 137
sudo ufw allow proto udp to any port 138
sudo ufw allow proto tcp to any port 139
sudo ufw allow proto tcp to any port 445