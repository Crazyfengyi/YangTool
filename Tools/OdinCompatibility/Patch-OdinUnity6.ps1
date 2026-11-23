param([Parameter(Mandatory=$true)][string]$OdinDirectory,[Parameter(Mandatory=$true)][string]$UnityDataDirectory)
$ErrorActionPreference='Stop'
# Unity 6000.5 将 IMGUI 回调成员迁移到 UIElementsIMGUIUtility
Add-Type -Path (Join-Path $UnityDataDirectory 'il2cpp/build/deploy/Mono.Cecil.dll')
$unityAssembly=[Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $UnityDataDirectory 'Managed/UnityEngine/UnityEngine.UIElementsModule.dll'))
$target=@($unityAssembly.MainModule.Types | Where-Object FullName -eq 'UnityEngine.UIElements.UIElementsIMGUIUtility')
if($target.Count -ne 1 -or @($target[0].Fields | Where-Object Name -Match '^s_(Begin|End)ContainerCallback$').Count -ne 2){throw 'Unity IMGUI callback layout does not match this patch'}
$unityAssembly.Dispose()
$assemblyPath=Join-Path $OdinDirectory 'Assemblies/Sirenix.Reflection.Editor.dll'
$assembly=[Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyPath)
$references=@($assembly.MainModule.GetTypeReferences() | Where-Object FullName -eq 'UnityEngine.UIElements.UIElementsUtility')
if($references.Count -ne 1){throw 'Expected exactly one original UIElementsUtility type reference'}
$members=@($assembly.MainModule.GetMemberReferences() | Where-Object {$_.DeclaringType -eq $references[0]})
if(@($members | Where-Object Name -NotIn @('s_BeginContainerCallback','s_EndContainerCallback','GetCurrentIMGUIContainer')).Count){throw 'Unexpected UIElementsUtility references'}
$references[0].Name='UIElementsIMGUIUtility'
$tempPath=$assemblyPath+'.patched'
$assembly.Write($tempPath)
$assembly.Dispose()
Move-Item -LiteralPath $tempPath -Destination $assemblyPath -Force
Write-Output 'Patched Odin IMGUI member references for Unity 6000.5'
