#Copyright 2022 Chuongmep.com ＼（＾_＾）／
param ($Configuration, $ProjectDir, $SolutionDir)
Write-Host "Solution Directory:" $SolutionDir
Write-Host "Project Directory:" $ProjectDir
Write-Host "Configuration Name Current:" $Configuration
$bundle = "C:\ProgramData\Autodesk\ApplicationPlugins\CadAddinManager.bundle\"
$content = "PackageContents.xml"
$AutodeskProcessName = "acad"
if($Configuration -match 'Debug A\d\d')
{
	$proc = Get-Process $AutodeskProcessName -ErrorAction SilentlyContinue
	if($proc)
	{
		Write-Host "Warning : Please Close Autocad Or Civil3D To Auto Copy Resouce And Debug ＼（＾_＾）／"
	}
	else
	{
		Write-Host "************Start Create Folder And Check File ＼（＾.＾）／"
		if(Test-Path $bundle){
			Write-Host "Exits Path, So Remove All File Exits"
			Remove-Item ($bundle) -Recurse
			Write-Host "Removed All File Exist"
		}
		Write-Host "************ Start Copy New File"
		xcopy ($SolutionDir + $content) $bundle /Y
		xcopy ($ProjectDir + "*.*") $bundle /Y /I /E /R
		Write-Host "************ Oh my got ! Copy Complete! Chuongmep.com ＼（＾_＾）／"
	}
}
else
{
	Write-Host "Please Toggle To Debug Model If You Want Copy File And Debug ＼（＾_＾）／ , config in postbuild.ps1"
}
