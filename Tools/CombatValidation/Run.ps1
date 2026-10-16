param([string]$UnityPath)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$UnityPath) {
    $versionLine = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') | Select-Object -First 1
    $version = $versionLine -replace '^m_EditorVersion:\s*', ''
    $UnityPath = "C:/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
}
if (!(Test-Path -LiteralPath $UnityPath)) { throw "Unity editor not found: $UnityPath" }

# Isolate tests from the open project and from the sample scene bootstrap.
$validationRoot = Join-Path $projectRoot 'Temp/CombatValidation'
New-Item -ItemType Directory -Force -Path "$validationRoot/Assets/Editor", "$validationRoot/Packages", "$validationRoot/ProjectSettings" | Out-Null
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets/Scripts') -Destination "$validationRoot/Assets" -Recurse -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets/Resources') -Destination "$validationRoot/Assets" -Recurse -Force
New-Item -ItemType Directory -Force -Path "$validationRoot/Assets/Fonts" | Out-Null
Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets/TextMesh Pro/Fonts/SBAggroL.otf'), (Join-Path $projectRoot 'Assets/TextMesh Pro/Fonts/SBAggroL.otf.meta') -Destination "$validationRoot/Assets/Fonts" -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Destination "$validationRoot/ProjectSettings" -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CombatValidation.cs') -Destination "$validationRoot/Assets/Editor" -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PlayerDesignValidation.cs') -Destination "$validationRoot/Assets/Editor" -Force
foreach ($suffix in @('.cs', '.cs.meta')) {
    $bootstrap = Join-Path $validationRoot "Assets/Scripts/Player/PlayerSystemTestBootstrap$suffix"
    if (Test-Path -LiteralPath $bootstrap) { Remove-Item -LiteralPath $bootstrap }
}
@'
{"dependencies":{"com.unity.inputsystem":"1.18.0","com.unity.ugui":"2.0.0","com.unity.modules.physics2d":"1.0.0","com.unity.modules.ui":"1.0.0","com.unity.modules.audio":"1.0.0","com.unity.modules.imgui":"1.0.0"}}
'@ | Set-Content -LiteralPath "$validationRoot/Packages/manifest.json"

$logPath = Join-Path $validationRoot 'validation.log'
$unityArguments = @('-batchmode', '-nographics', '-projectPath', "`"$validationRoot`"", '-executeMethod', 'CombatValidation.Run', '-logFile', "`"$logPath`"")
$process = Start-Process -FilePath $UnityPath -ArgumentList $unityArguments -WindowStyle Hidden -PassThru
if (!$process.WaitForExit(300000)) {
    Stop-Process -Id $process.Id
    throw "Validation timed out. See $logPath"
}
$success = Select-String -LiteralPath $logPath -Pattern 'COMBAT_VALIDATION_SUCCESS:'
if ($process.ExitCode -ne 0 -or !$success) { throw "Combat validation failed. See $logPath" }
$success.Line
