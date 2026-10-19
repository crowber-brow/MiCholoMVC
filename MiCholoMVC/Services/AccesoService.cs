using System.Security.Cryptography;
using System.Text.Json;

namespace MiCholoMVC.Services;

public sealed class AccesoService
{
    private readonly string archivo;
    private readonly object sync = new();

    private readonly Dictionary<string, (int Intentos, DateTime Bloqueado)>
        intentos = new();

    public record Configuracion(string Usuario, string Hash);

    public AccesoService(IWebHostEnvironment env)
    {
        archivo = Path.Combine(
            env.ContentRootPath,
            "App_Data",
            "acceso.json"
        );

        // Crear la carpeta de configuración si no existe.
        Directory.CreateDirectory(Path.GetDirectoryName(archivo)!);

        // Crear las credenciales iniciales solo si no hay configuración.
        if (!File.Exists(archivo))
        {
            var config = new Configuracion(
                "micholo@gmail.com",
                Hash("cholo123")
            );

            var temporal = archivo + ".tmp";

            File.WriteAllText(
                temporal,
                JsonSerializer.Serialize(config)
            );

            File.Move(temporal, archivo, true);
        }
    }

    public string VersionCredencial()
    {
        lock (sync)
        {
            return File.Exists(archivo)
                ? Convert.ToHexString(
                    SHA256.HashData(File.ReadAllBytes(archivo))
                  )
                : "";
        }
    }

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        var key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            210000,
            HashAlgorithmName.SHA256,
            32
        );

        return Convert.ToBase64String(salt)
            + "."
            + Convert.ToBase64String(key);
    }

    public static bool Verificar(string password, string hash)
    {
        try
        {
            var parts = hash.Split('.');

            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hashGuardado = Convert.FromBase64String(parts[1]);

            var key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                210000,
                HashAlgorithmName.SHA256,
                32
            );

            return CryptographicOperations.FixedTimeEquals(
                key,
                hashGuardado
            );
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public bool Autenticar(string usuario, string password, string ip)
    {
        lock (sync)
        {
            var ahora = DateTime.UtcNow;
            var claves = new[] { ip, "cuenta" };

            // Bloquear durante cinco minutos tras cinco intentos fallidos.
            if (claves.Any(k =>
                intentos.TryGetValue(k, out var a) &&
                a.Bloqueado > ahora))
            {
                return false;
            }

            if (!File.Exists(archivo))
                return false;

            var config =
                JsonSerializer.Deserialize<Configuracion>(
                    File.ReadAllText(archivo)
                );

            if (config is null)
                return false;

            var valido =
                string.Equals(
                    usuario.Trim(),
                    config.Usuario,
                    StringComparison.OrdinalIgnoreCase
                )
                && Verificar(password, config.Hash);

            foreach (var k in claves)
            {
                if (valido)
                {
                    intentos.Remove(k);
                    continue;
                }

                var anterior = intentos.GetValueOrDefault(k);

                var cantidad =
                    anterior.Bloqueado != default &&
                    anterior.Bloqueado <= ahora
                        ? 1
                        : anterior.Intentos + 1;

                intentos[k] = (
                    cantidad,
                    cantidad >= 5
                        ? ahora.AddMinutes(5)
                        : default
                );
            }

            return valido;
        }
    }

    public void Cambiar(string actual, string nueva)
    {
        lock (sync)
        {
            if (nueva.Length < 10 ||
                nueva.Length > 128 ||
                !nueva.Any(char.IsDigit) ||
                !nueva.Any(char.IsLetter))
            {
                throw new InvalidOperationException(
                    "Use de 10 a 128 caracteres, con letras y números."
                );
            }

            if (!File.Exists(archivo))
            {
                throw new InvalidOperationException(
                    "No existe la configuración de acceso."
                );
            }

            var config =
                JsonSerializer.Deserialize<Configuracion>(
                    File.ReadAllText(archivo)
                );

            if (config is null)
            {
                throw new InvalidOperationException(
                    "La configuración de acceso no es válida."
                );
            }

            if (!Verificar(actual, config.Hash))
            {
                throw new InvalidOperationException(
                    "La contraseña actual es incorrecta."
                );
            }

            var temporal = archivo + ".tmp";

            File.WriteAllText(
                temporal,
                JsonSerializer.Serialize(
                    config with { Hash = Hash(nueva) }
                )
            );

            File.Move(temporal, archivo, true);
        }
    }
}