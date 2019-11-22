$invocation = (Get-Variable MyInvocation).Value
$directorypath = Split-Path $invocation.MyCommand.Path
$certPath = $directorypath + "\profilebot.cer"
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certPath)
$rootStore = Get-Item cert:\LocalMachine\Root
$rootStore.Open("ReadWrite")
$rootStore.Add($cert)
$rootStore.Close()