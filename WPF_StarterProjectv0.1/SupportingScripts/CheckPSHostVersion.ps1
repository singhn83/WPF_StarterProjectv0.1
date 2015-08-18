Param(
	$Window #this is the MainWindow object
)
 
try
{
	do {
	    #increment the MainProgress property in the MainWindow class
	    $Window.MainProgress += 1;
		Start-Sleep -m 50
	} while($Window.MainProgress -le 100)
 
    if((get-host).version.Major -gt 3)
	{  
	    Get-Host | Out-String
        Return 0
	} else {
	    Throw "Error: Dependency check failed. Please consult your SA"
	}
}
catch [Exception]
{
	$failMessage = "$($_.Exception.ToString()).$($_.InvocationInfo.PositionMessage)"
	Write-Error $failMessage
	return 1
}
   
