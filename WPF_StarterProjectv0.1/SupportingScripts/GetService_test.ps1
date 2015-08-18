
Param(
    [Parameter()]
    [pscredential]$Creds,
    [string]$Identity,
	$Context
)

$username = $Context.UserName
Write-Output "Username is $username"
Write-Output "Identity is $Identity"

while ($Context.Progress -le 100.0) {
	$Context.Progress += 1.0
	Start-Sleep -Milliseconds 50
}

Get-Service

write-Output $Error