iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex
choco install -y ruby
choco install -y chromedriver

echo "You will need to install MongoDb and Postgres for some tests."
echo "The best way to do this is via Docker for Windows."
echo ""
echo "Docker requires Windows 10, build 10586 upwards."
echo "You can install it by typing choco install docker."