  Get-Host | Write-Output
  if((get-host).version.Major -gt 3)
 
   {  
      Write-Output "Info: Dependency Check passed. You can proceed with the Executing Tasks"
      Return 0
   } 
                           
  else
  
   { 
   
   Write-Output "Error: Dependency check failed. Please consult your SA"
   Return 1
   
   }