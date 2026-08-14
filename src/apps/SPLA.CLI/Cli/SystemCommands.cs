using SPLA.Service;

namespace SPLA.CLI;

/// <summary>
/// <c>spla system register-association</c> — re-registers the .spla file extension to whichever
/// SPLA.UI.Avalonia.exe sits next to this CLI (see <see cref="SystemOps.RegisterFileAssociation"/>).
/// PublishAll.ps1 calls this after every publish, so the build you just ran is the build Explorer
/// opens next — no running service needed, and no stale registration pointing at an old folder.
/// </summary>
internal static class SystemCommands
{
    public static int Run(string[] args)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        switch (sub)
        {
            case "register-association":
                var result = SystemOps.RegisterFileAssociation();
                Console.WriteLine(result.Message);
                return result.Ok ? 0 : 1;

            default:
                Console.Error.WriteLine("Usage: spla system register-association");
                return 2;
        }
    }
}
