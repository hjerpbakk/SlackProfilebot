$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $env:VM_USER,$$env:VM_PW
Enter-PSSession -ComputerName $env:VM_URL -Credential $cred -UseSSL
docker stop profilebot
docker container prune -f
docker pull hjerpbakk/profilebot
docker run --name profilebot hjerpbakk/profilebot
Exit