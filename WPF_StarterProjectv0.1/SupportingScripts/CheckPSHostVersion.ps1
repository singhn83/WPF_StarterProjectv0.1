Param(
	$Context
)
 

do {
    $Context.MainProgress += 1;
    Start-Sleep -m 50
   }
 while($Context.MainProgress -le 100)
 
 
  if((get-host).version.Major -gt 3)
 
   {  

	  Return 0
   } 
                           
  else
  
   { 
   
   Write-Output "Error: Dependency check failed. Please consult your SA"
   Write-Output $ProgressBar
   Return 1
   
   }
   
