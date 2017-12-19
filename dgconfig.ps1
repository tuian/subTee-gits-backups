$s1 = (gwmi -List Win32_ShadowCopy).Create("C:\", "ClientAccessible")
$s2 = gwmi Win32_ShadowCopy | ? { $_.ID -eq $s1.ShadowID }
$d  = $s2.DeviceObject + "\"
cmd /c mklink /d C:\scpy "$d"
New-CIPolicy -Level LeafCertificate -FilePath C:\BasePolicy.xml -ScanPath C:\scpy -UserPEs
$s2.Delete()
Remove-Item -Path C:\scpy -Force
Set-RuleOption –option 3 –FilePath C:\BasePolicy.xml
ConvertFrom-CIPolicy C:\BasePolicy.xml C:\BasePolicy.bin
Move-Item C:\BasePolicy.bin c:\Windows\System32\CodeIntegrity\SIPolicy.p7b -force
# Reboot
  
# Update after use
New-CIPolicy -Level LeafCertificate -f C:\AuditPolicy.xml -Audit -UserPEs -Fallback Hash
Merge-CIPolicy –OutputFilePath C:\MergedPolicy.xml –PolicyPaths C:\AuditPolicy.xml,C:\BasePolicy.xml
Set-RuleOption –option 3 –FilePath C:\MergedPolicy.xml
ConvertFrom-CIPolicy C:\MergedPolicy.xml C:\MergedPolicy.bin
Move-Item C:\MergedPolicy.bin c:\Windows\System32\CodeIntegrity\SIPolicy.p7b -force
#reboot