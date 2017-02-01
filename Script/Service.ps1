Param(
  [string]$server,
  [string]$service,
  [string]$status,
  [string]$ppath
)

#read-host -assecurestring | convertfrom-securestring | out-file ppath

$p = cat $ppath | ConvertTo-SecureString
$u = "kad\Administrator"
$c = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $u,$p

Invoke-Command -ComputerName $server -ScriptBlock{ Set-Service $args[0] -Status $args[1] } -Credential $c -ArgumentList $service,$status
