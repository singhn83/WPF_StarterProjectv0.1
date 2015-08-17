
Param(
    [Parameter()]
    [pscredential]$Creds,
    [string]$Identity,
	$class
)

$username = $Creds.UserName
Write-Output "Username is $username"
Write-Output "Identity is $Identity"

Get-Service

write-Output $Error