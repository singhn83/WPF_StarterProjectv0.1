 # Get-Host | Write-Output | Out-String
 
 $CheckProgress=0;

do {
    $CheckProgress += 1;
    Write-Host $CheckProgress    
    Start-Sleep -m 100
   }
 while($CheckProgress -lt 100)
 
 
  if((get-host).version.Major -gt 3)
 
   {  
      Get-Service
      Write-Output $ProgressBar
	   $ProgressBar.Value=20
	  Return 0
   } 
                           
  else
  
   { 
   
   Write-Output "Error: Dependency check failed. Please consult your SA"
   Write-Output $ProgressBar
   Return 1
   
   }
   
