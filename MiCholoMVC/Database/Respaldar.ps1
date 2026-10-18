param([string]$Servidor='localhost\SQLEXPRESS',[string]$Base='MiCholoDB')
$ErrorActionPreference='Stop'
$conn=New-Object System.Data.SqlClient.SqlConnection "Server=$Servidor;Database=$Base;Integrated Security=True;TrustServerCertificate=True"
$conn.Open()
try {
 $cmd=$conn.CreateCommand();$cmd.CommandTimeout=120;$cmd.CommandText="SELECT SERVERPROPERTY('InstanceDefaultBackupPath')";$carpeta=[string]$cmd.ExecuteScalar()
 $destino=Join-Path $carpeta ($Base+'_'+(Get-Date -Format 'yyyyMMdd_HHmmss')+'.bak')
 $cmd.CommandText='BACKUP DATABASE '+('['+$Base.Replace(']',']]')+']')+' TO DISK=@ruta WITH COPY_ONLY,CHECKSUM';[void]$cmd.Parameters.AddWithValue('@ruta',$destino);[void]$cmd.ExecuteNonQuery()
 $cmd.CommandText='RESTORE VERIFYONLY FROM DISK=@ruta WITH CHECKSUM';[void]$cmd.ExecuteNonQuery()
 Write-Output "Respaldo creado y verificado: $destino"
}finally{$conn.Close()}
