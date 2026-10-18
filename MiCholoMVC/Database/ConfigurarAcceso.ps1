param([string]$Usuario='micholo@gmail.com')
$ErrorActionPreference='Stop'
$ruta=Join-Path (Split-Path $PSScriptRoot -Parent) 'App_Data/acceso.json'
if(Test-Path -LiteralPath $ruta){throw 'El acceso ya está configurado. Cambie la contraseña desde la aplicación.'}
$segura=Read-Host 'Nueva contraseña administrativa (10 o más caracteres con letras y números)' -AsSecureString
$ptr=[Runtime.InteropServices.Marshal]::SecureStringToBSTR($segura)
try {
 $texto=[Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
 if($texto.Length -lt 10 -or $texto.Length -gt 128 -or $texto -notmatch '[0-9]' -or $texto -notmatch '[a-zA-Z]'){throw 'La contraseña no cumple los requisitos.'}
 $salt=[Security.Cryptography.RandomNumberGenerator]::GetBytes(16)
 $key=[Security.Cryptography.Rfc2898DeriveBytes]::Pbkdf2($texto,$salt,210000,[Security.Cryptography.HashAlgorithmName]::SHA256,32)
 New-Item -ItemType Directory -Force (Split-Path $ruta -Parent) | Out-Null
 @{Usuario=$Usuario;Hash=([Convert]::ToBase64String($salt)+'.'+[Convert]::ToBase64String($key))} | ConvertTo-Json | Set-Content -LiteralPath $ruta -Encoding utf8
 Write-Output 'Acceso configurado. La contraseña no se guardó en texto plano.'
}finally{[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr);Remove-Variable texto -ErrorAction SilentlyContinue}
