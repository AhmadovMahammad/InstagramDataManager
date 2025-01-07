$exePath = "C:\Users\mahammada\source\repos\SideProjects\InstagramDataManager\DataManager\bin\Debug\net8.0\DataManager.exe"
$startupFolder = [System.Environment]::GetFolderPath('Startup')
$shortcut = "$startupFolder\InstagramAutomationTool.lnk"
$wshShell = New-Object -ComObject WScript.Shell
$shortcutObject = $wshShell.CreateShortcut($shortcut)
$shortcutObject.TargetPath = $exePath
$shortcutObject.Save()
