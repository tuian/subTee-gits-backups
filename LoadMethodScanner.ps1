# Author: Matthew Graeber (@mattifestation)
# Load dnlib with Add-Type first
# dnlib can be obtained here: https://github.com/0xd4d/dnlib
# Example: ls C:\ -Recurse | Get-AssemblyLoadReference
filter Get-AssemblyLoadReference {
    param (
        [Parameter(Mandatory = $True, ValueFromPipelineByPropertyName = $True)]
        [Alias('FullName')]
        [String]
        [ValidateNotNullOrEmpty()]
        $Path
    )
 
    $FullPath = Resolve-Path $Path
 
    $Module = $null
 
    try {
        $Module = [dnlib.DotNet.ModuleDefMD]::Load($FullPath)
    } catch {
        return
    }
 
    $listMemberRefMD = $Module.GetType().GetFields('NonPublic, Instance') | ? { $_.Name -eq 'listMemberRefMD' }
    $MemberRefList = $listMemberRefMD.GetValue($Module)
 
    $GenericParamContext = New-Object -TypeName dnlib.DotNet.GenericParamContext
 
    $AssemblyLoadList = New-Object -TypeName 'System.Collections.Generic.List[System.Object]'
 
    for ($i = 0; $i -lt $MemberRefList.Length; $i++) {
        $MemberRefDefinition = $MemberRefList.Item($i, $GenericParamContext)
 
        if (($MemberRefDefinition.Name.String -eq 'Load') -and
            ($MemberRefDefinition.ReturnType.FullName -eq 'System.Reflection.Assembly') -and
            ($MemberRefDefinition.MethodSig.Params.FullName -contains 'System.Byte[]')) {
 
            <# The assembly "imports" a Load method that:
                1) Is called "Load"
                2) Returns a System.Reflection.Assembly instance
                3) Has at least one parameter that accepts an argument of type System.Byte[]
            #>
            $AssemblyLoadList.Add($MemberRefDefinition)
        }
    }
 
    if ($AssemblyLoadList.Count) {
        [PSCustomObject] @{
            AssemblyPath = $FullPath
            LoadMethodImports = $AssemblyLoadList
        }
    }
}