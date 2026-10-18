param([string]$Servidor='localhost\SQLEXPRESS',[string]$Base='MiCholoDB')
$ErrorActionPreference='Stop'
if($Base -notmatch '^[A-Za-z0-9_]+$'){throw 'Nombre de base inválido.'}
$conn=New-Object System.Data.SqlClient.SqlConnection "Server=$Servidor;Database=master;Integrated Security=True;TrustServerCertificate=True"
$conn.Open()
try {
 $cmd=$conn.CreateCommand();$cmd.CommandText='SELECT DB_ID(@nombre)';[void]$cmd.Parameters.AddWithValue('@nombre',$Base)
 if($cmd.ExecuteScalar() -isnot [DBNull]){throw 'La base ya existe. Use Actualizar.ps1 para conservar sus datos.'}
 $cmd.Parameters.Clear();$cmd.CommandText='CREATE DATABASE ['+$Base+']';[void]$cmd.ExecuteNonQuery()
}finally{$conn.Close()}
$conn=New-Object System.Data.SqlClient.SqlConnection "Server=$Servidor;Database=$Base;Integrated Security=True;TrustServerCertificate=True"
$conn.Open()
try {
 $cmd=$conn.CreateCommand();$cmd.CommandTimeout=120;$script=Get-Content (Join-Path $PSScriptRoot 'Crear.sql') -Raw
 foreach($batch in [regex]::Split($script,'(?m)^GO\s*$')){if($batch.Trim()){$cmd.CommandText=$batch;[void]$cmd.ExecuteNonQuery()}}
}finally{$conn.Close()}
& (Join-Path $PSScriptRoot 'Actualizar.ps1') -Servidor $Servidor -Base $Base
