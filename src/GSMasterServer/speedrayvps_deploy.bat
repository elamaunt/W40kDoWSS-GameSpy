dotnet publish -c Release -o Deploy/
scp -r Deploy/* root@134.209.227.145:/root/gamespy/
ssh root@134.209.227.145 systemctl restart gamespy