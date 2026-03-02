# Quick SQLite Query Script
# Usage: .\query-db.ps1 "SELECT * FROM Users"

param(
    [Parameter(Mandatory=$true)]
    [string]$Query
)

$dbPath = "C:\Users\Zyon\Projects\form-maker\FormMaker.Api\formmaker.db"

Add-Type -AssemblyName "System.Data"

$connection = New-Object System.Data.SQLite.SQLiteConnection
$connection.ConnectionString = "Data Source=$dbPath"
$connection.Open()

$command = $connection.CreateCommand()
$command.CommandText = $Query

$adapter = New-Object System.Data.SQLite.SQLiteDataAdapter($command)
$dataset = New-Object System.Data.DataSet

$adapter.Fill($dataset) | Out-Null

$connection.Close()

$dataset.Tables[0] | Format-Table -AutoSize
