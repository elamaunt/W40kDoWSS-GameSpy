dotnet publish -c Release -o Deploy/
scp -r Deploy/* root@139.59.210.74:/root/gamespy/
ssh root@139.59.210.74 systemctl restart gamespy
