#Set-ExecutionPolicy RemoteSigned | Set-ExecutionPolicy Unrestricted | Get-ExecutionPolicy
    
cd ..\

$loc = Get-Location
$loc = Get-Location

$file = $loc.path + "\CGJsonEditorWPF\AssemblyVersion.cs"

$AssemblyVersion = Get-content -Path $file 

$list = $AssemblyVersion -split "`r`n"

$RevisonLine =  $list[6]-split"="  # Revision

[int]$Revison = $revisonLine[1] -replace ".$"

$NewRevisonLine = $RevisonLine[0] +"= "+ ($Revison+1) +";"

$AssemblyVersion = $AssemblyVersion -replace   $list[6],$NewRevisonLine

if ($list.Length -gt 6)
    {
        Set-Content -Path $file -value $AssemblyVersion

    }
