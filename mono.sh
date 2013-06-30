# To install this script:
#sudo wget --no-check-certificate --no-cache https://gist.github.com/yetanotherchris/5426167/raw
#sudo sh raw

# Update
sudo apt-get -y update

# Install Apache, Mono and mod_mono. 
# THIS SECTION MAY FREEZE (from a libapache2 package issue.) - IF THIS HAPPENS CLOSE YOUR SSH SESSION, LOG BACK IN AND RUN THE SCRIPT AGAIN.
sudo apt-get -y install apache2
sudo /etc/init.d/apache2 stop
sudo apt-get -y install mono-devel mono-runtime
sudo apt-get -y install mono-apache-server4
sudo apt-get -y install libapache2-mod-mono
sudo rm /var/www/index.html

# Get Roadkill and unzip it
sudo rm -R /var/www/roadkill/
sudo wget --no-check-certificate --no-cache https://bitbucket.org/mrshrinkray/roadkill/downloads/Roadkill.mono.1.7.zip
sudo apt-get -y install unzip
sudo unzip Roadkill.mono.1.6.zip -d /var/www

# Move roadkill to the default apache directory, update permissions for r,w,exec for user + group
sudo mv /var/www/roadkill/* /var/www/
sudo chgrp -R www-data /var/www
sudo chown -R www-data /var/www
sudo chmod -R 755 /var/www

# Replace the default apache site settings
sudo mv /var/www/apache.txt /etc/apache2/sites-available/default
sudo /etc/init.d/apache2 restart

# Install MongoDB, add a user to the default collection
sudo apt-get -y install mongodb
sudo mongo local --eval "db.addUser('roadkill','password');"

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