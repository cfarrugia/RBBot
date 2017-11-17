[void][System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.ConnectionInfo');
[void][System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.Management.Sdk.Sfc');
[void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO")
[void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMOExtended")

$serverName = "localhost\sql2016"
$scriptPath = "C:\Projects\RBBot\RBBot.Core\Database\"
$databaseName = "RBBOT"
$sqlServer = new-object("Microsoft.SqlServer.Management.Smo.Server") $serverName
$sqlDb = $sqlServer.Databases[$databasename]


$options = new-object ("Microsoft.SqlServer.Management.Smo.ScriptingOptions")
$options.ExtendedProperties = $true
$options.DRIAll = $true
$options.Indexes = $true
$options.Triggers = $true
$options.ScriptBatchTerminator = $true
$options.Filename = $scriptPath + "\db-schema.sql"
$options.IncludeHeaders = $true
$options.ToFileOnly = $true

$transfer = new-object ("Microsoft.SqlServer.Management.Smo.Transfer") $sqlDb
$transfer.options = $options
$transfer.ScriptTransfer()

# Schema is done!

$Objects = $sqlDb.Tables


$dataFile = $scriptPath + "db-lookupdata.sql"
New-Item $dataFile -type file -force

#before starting disable all check constraints!

$disableChecks = "EXEC sp_msforeachtable ""ALTER TABLE ? NOCHECK CONSTRAINT all"""
$go = "GO"
$disableChecks >> $dataFile
$go >> $dataFile

foreach ($tb in $Objects | where {!($_.IsSystemObject) }) {
    # Tables to be excluded!
    if (
        $tb.Name -eq "MarketPrice" -or 
        $tb.Name -eq "TradeOpportunity" -or
        $tb.Name -eq "TradeOpportunityRequirement" -or
        $tb.Name -eq "TradeOpportunityTransaction" -or
        $tb.Name -eq "TradeOpportunityValue"
        )
    {
        continue
    }

    $info = "Currently writing " + $tb.Schema + "." + $tb.Name
    Write-Host $info

    
    $scriptr = new-object ('Microsoft.SqlServer.Management.Smo.Scripter') ($sqlServer)
    $scriptr.Options.AppendToFile = $True
    $scriptr.Options.AllowSystemObjects = $False
    $scriptr.Options.ScriptData = $True
    $scriptr.Options.ToFileOnly = $True
    $scriptr.Options.ScriptSchema = $False
    $scriptr.Options.Indexes = $True

    $scriptr.Options.FileName = $dataFile
    $scriptr.EnumScript($tb)

   
}

#now reenable

$enableChecks = "EXEC sp_msforeachtable ""ALTER TABLE ?  WITH CHECK CHECK CONSTRAINT all"""
$go = "GO"

$go >> $dataFile
$enableChecks >> $dataFile
$go >> $dataFile