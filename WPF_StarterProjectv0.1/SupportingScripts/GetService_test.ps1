
Param(
    [Parameter()]
    [pscredential]$Creds,
    [string]$Identity,
	$Window # this is the MainWindow object
)

try 
{
    #you can access the members in the MainWindow class
	$username = $Window.UserName
	Write-Output "Username is $username"
	Write-Output "Identity is $Identity"

    #increment the Progress variable, which (in C#) triggers a PropertyChanged event, causing the progressbar to be updated
	while ($Window.Progress -le 100.0) {
		$Window.Progress += 1.0
		Start-Sleep -Milliseconds 50
	}

    Get-Service

	return 0
} 
catch [Exception]
{
    # create message containing the Exception message as well as a stack trace (PositionMessasge)
    $failMessage = "$($_.Exception.ToString()).$($_.InvocationInfo.PositionMessage)"
	Write-Error $failMessage
	return 1
}
