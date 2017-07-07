$VM_URL = "profilebot.northeurope.cloudapp.azure.com"
$pw = ConvertTo-SecureString $env:VM_PW -AsPlainText -Force
$cred = new-object -Typename System.Management.Automation.PSCredential -ArgumentList $env:VM_USER, $pw
$session = New-PSSession -ComputerName $env:VM_URL -Credential $cred -UseSSL
Invoke-Command -Session $session {docker stop profilebot}
Invoke-Command -Session $session {docker container prune -f}
Invoke-Command -Session $session {docker pull hjerpbakk/profilebot}
Invoke-Command -Session $session {docker run -d --name profilebot hjerpbakk/profilebot} 