using System.Text.Json;

namespace SPLA.Service.Auth;

/// <summary>Persistence for local accounts. Kept behind an interface so the file store can be swapped
/// for a database one later without touching <see cref="LocalAccountService"/> — but no second
/// implementation is introduced speculatively.</summary>
public interface IUserStore
{
    IReadOnlyList<LocalUser> All();
    LocalUser? FindByUsername(string username);
    LocalUser? FindByKey(string userKey);
    void Upsert(LocalUser user);
    bool Delete(string userKey);
}

/// <summary>
/// A JSON-file-backed user store: the whole set is held in memory and rewritten atomically on every
/// mutation (write-to-temp + move), guarded by a lock. Ample for the home/workgroup scale this auth
/// mode targets (tens of accounts), and consistent with the file-store pattern used for chats and
/// secrets — no database dependency.
/// </summary>
public sealed class JsonUserStore : IUserStore
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _path;
    private readonly Lock _gate = new();
    private readonly List<LocalUser> _users = [];

    public JsonUserStore(string path)
    {
        _path = path;
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_path)) return;
        try
        {
            var users = JsonSerializer.Deserialize<List<LocalUser>>(File.ReadAllText(_path), Json);
            if (users != null)
            {
                _users.Clear();
                _users.AddRange(users);
            }
        }
        catch
        {
            // A corrupt store must not crash the host; start empty (the seed-admin path re-creates one).
        }
    }

    private void Save()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(_users, Json));
        File.Move(tmp, _path, overwrite: true);
    }

    public IReadOnlyList<LocalUser> All()
    {
        lock (_gate) return _users.ToList();
    }

    public LocalUser? FindByUsername(string username)
    {
        lock (_gate)
            return _users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public LocalUser? FindByKey(string userKey)
    {
        lock (_gate) return _users.FirstOrDefault(u => u.UserKey == userKey);
    }

    public void Upsert(LocalUser user)
    {
        lock (_gate)
        {
            _users.RemoveAll(u => u.UserKey == user.UserKey);
            _users.Add(user);
            Save();
        }
    }

    public bool Delete(string userKey)
    {
        lock (_gate)
        {
            var removed = _users.RemoveAll(u => u.UserKey == userKey) > 0;
            if (removed) Save();
            return removed;
        }
    }
}
