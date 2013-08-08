# setting up some paths
$nuget_script_dir = [System.IO.Path]::GetDirectoryName(
						[System.IO.Path]::GetFullPath($MyInvocation.MyCommand.Definition)
					)
$distrex_path = [System.IO.Path]::GetFullPath(
					$nuget_script_dir + "\.."
				)
$settings_cmd = $nuget_script_dir + "\settings.ps1"
$nuget_cmd = [System.IO.Path]::GetFullPath($distrex_path + "..\.nuget\NuGet.exe")

$solution_file = [System.IO.Path]::GetFullPath($distrex_path + "\DistrEx.sln")

# sourcing settings
. $settings_cmd

#different compiler runs
$compiler_options_x86 = "/t:Rebuild /p:Configuration=Release /p:Platform=x86 /p:OutDir=bin\Release_x86\" 
$compiler_options_x64 = "/t:Rebuild /p:Configuration=Release /p:Platform=x86 /p:OutDir=bin\Release_x64\" 

Invoke-Expression "$compiler_cmd $solution_file $compiler_options_x86"
Invoke-Expression "$compiler_cmd $solution_file $compiler_options_x64"