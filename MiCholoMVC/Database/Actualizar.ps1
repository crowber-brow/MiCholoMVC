param([string]$Servidor='localhost\SQLEXPRESS',[string]$Base='MiCholoDB')
$ErrorActionPreference='Stop'
$conn=New-Object System.Data.SqlClient.SqlConnection "Server=$Servidor;Database=$Base;Integrated Security=True;TrustServerCertificate=True"
$conn.Open()
try {
 $cmd=$conn.CreateCommand();$cmd.CommandTimeout=120
 $cmd.CommandText="SELECT SERVERPROPERTY('InstanceDefaultBackupPath')";$backupRoot=[string]$cmd.ExecuteScalar()
 $backup=Join-Path $backupRoot ($Base+'_antes_pos_'+(Get-Date -Format 'yyyyMMdd_HHmmss')+'.bak')
 $cmd.CommandText='BACKUP DATABASE '+('['+$Base.Replace(']',']]')+']')+' TO DISK=@ruta WITH COPY_ONLY, CHECKSUM';[void]$cmd.Parameters.AddWithValue('@ruta',$backup);[void]$cmd.ExecuteNonQuery();$cmd.Parameters.Clear()
 $cmd.CommandText="IF OBJECT_ID('MiCholoVersion') IS NULL CREATE TABLE MiCholoVersion(Version int PRIMARY KEY,Fecha datetime2 NOT NULL); SELECT COUNT(*) FROM MiCholoVersion WHERE Version=2;"
 if([int]$cmd.ExecuteScalar() -eq 0){
  $tx=$conn.BeginTransaction();$cmd.Transaction=$tx
  try {
   $sql=Get-Content (Join-Path $PSScriptRoot 'Actualizar.sql') -Raw
   foreach($batch in [regex]::Split($sql,'(?m)^GO\s*$')){if($batch.Trim()){$cmd.CommandText=$batch;[void]$cmd.ExecuteNonQuery()}}
   $cmd.CommandText='INSERT INTO MiCholoVersion VALUES(2,SYSDATETIME())';[void]$cmd.ExecuteNonQuery();$tx.Commit()
  }catch{$tx.Rollback();throw}
 }
 Write-Output "Base actualizada. Respaldo: $backup"
}finally{$conn.Close()}
