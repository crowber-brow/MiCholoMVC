using System.Security.Cryptography;
using System.Text.Json;
namespace MiCholoMVC.Services;
public sealed class AccesoService
{
    private readonly string archivo;
    private readonly object sync = new();
    private readonly Dictionary<string, (int Intentos, DateTime Bloqueado)> intentos = new();
    public AccesoService(IWebHostEnvironment env) { archivo = Path.Combine(env.ContentRootPath, "App_Data", "acceso.json"); }
    public record Configuracion(string Usuario, string Hash);
    public string VersionCredencial()
    {
        lock(sync) { return File.Exists(archivo) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(archivo))) : ""; }
    }
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(key);
    }
    public static bool Verificar(string password, string hash)
    {
        try {
            var parts = hash.Split('.');
            var key = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(parts[0]), 210000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(key, Convert.FromBase64String(parts[1]));
        } catch (FormatException) { return false; } catch (IndexOutOfRangeException) { return false; }
    }
    public bool Autenticar(string usuario, string password, string ip)
    {
        lock(sync)
        {
            var ahora = DateTime.UtcNow;
            // Límite global de la única cuenta y límite por origen.
            if (new[] { ip, "cuenta" }.Any(k => intentos.TryGetValue(k, out var a) && a.Bloqueado > ahora)) return false;
            if (!File.Exists(archivo)) return false;
            var config = JsonSerializer.Deserialize<Configuracion>(File.ReadAllText(archivo))!;
            var valido = Verificar(password, config.Hash) && string.Equals(usuario.Trim(), config.Usuario, StringComparison.OrdinalIgnoreCase);
            foreach (var k in new[] { ip, "cuenta" }) {
                if (valido) { intentos.Remove(k); continue; }
                var anterior = intentos.GetValueOrDefault(k);
                var cantidad = anterior.Bloqueado != default && anterior.Bloqueado <= ahora ? 1 : anterior.Intentos + 1;
                intentos[k] = (cantidad, cantidad >= 5 ? ahora.AddMinutes(5) : default);
            }
            return valido;
        }
    }
    public void Cambiar(string actual, string nueva)
    {
        lock(sync)
        {
            if (nueva.Length < 10 || nueva.Length > 128 || !nueva.Any(char.IsDigit) || !nueva.Any(char.IsLetter)) throw new InvalidOperationException("Use de 10 a 128 caracteres, con letras y números.");
            var config = JsonSerializer.Deserialize<Configuracion>(File.ReadAllText(archivo))!;
            if (!Verificar(actual, config.Hash)) throw new InvalidOperationException("La contraseña actual es incorrecta.");
            var temporal = archivo + ".tmp"; File.WriteAllText(temporal, JsonSerializer.Serialize(config with { Hash = Hash(nueva) })); File.Move(temporal, archivo, true);
        }
    }
}
