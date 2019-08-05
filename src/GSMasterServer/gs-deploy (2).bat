dotnet publish -c Debug -o Deploy/
scp -r Deploy/* root@134.209.198.2:/root/gamespy/
ssh root@134.209.198.2 systemctl restart gamespy