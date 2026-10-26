param([string]$UnityPath)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$UnityPath) {
    $versionLine = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') | Select-Object -First 1
    $version = $versionLine -replace '^m_EditorVersion:\s*', ''
    $UnityPath = "C:/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
}
if (!(Test-Path -LiteralPath $UnityPath)) { throw "Unity editor not found: $UnityPath" }

# Use the real assets and scenes in a separate project; saves use a test company/product.
$validationRoot = Join-Path $projectRoot 'Temp/CombatValidation'
New-Item -ItemType Directory -Force -Path "$validationRoot/Assets/Editor", "$validationRoot/Packages", "$validationRoot/ProjectSettings" | Out-Null
foreach ($file in @('SBAggroL.otf', 'SBAggroL.otf.meta')) {
    $oldFont = Join-Path $validationRoot "Assets/Fonts/$file"
    if (Test-Path -LiteralPath $oldFont) { Remove-Item -LiteralPath $oldFont }
}
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets') | Copy-Item -Destination "$validationRoot/Assets" -Recurse -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Destination "$validationRoot/ProjectSettings" -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CombatValidation.cs') -Destination "$validationRoot/Assets/Editor" -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PlayerDesignValidation.cs') -Destination "$validationRoot/Assets/Editor" -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'RoomIntegrationValidation.cs') -Destination "$validationRoot/Assets/Editor" -Force
$sourceManifest = Get-Content -LiteralPath (Join-Path $projectRoot 'Packages/manifest.json') -Raw | ConvertFrom-Json
$runtimeDependencies = [ordered]@{}
foreach ($entry in $sourceManifest.dependencies.PSObject.Properties) {
    if ($entry.Name -like 'com.unity.modules.*' -or $entry.Name -in @('com.unity.inputsystem', 'com.unity.ugui', 'com.unity.render-pipelines.universal')) {
        $runtimeDependencies[$entry.Name] = $entry.Value
    }
}
@{dependencies = $runtimeDependencies} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath "$validationRoot/Packages/manifest.json"
Copy-Item -LiteralPath (Join-Path $projectRoot 'ProjectSettings/TagManager.asset'), (Join-Path $projectRoot 'ProjectSettings/Physics2DSettings.asset') -Destination "$validationRoot/ProjectSettings" -Force

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
