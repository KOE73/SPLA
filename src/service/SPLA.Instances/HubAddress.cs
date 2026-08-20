namespace SPLA.Instances;

/// <summary>
/// Where the machine's hub lives, and how that is decided.
///
/// <para><b>Why a shared resolver.</b> Two parties have to agree on this number without talking to
/// each other: the CLI running <c>spla hub</c> and the desktop shell looking for one. They are in
/// different assemblies and different processes, and if they ever disagree the symptom is not an
/// error but a silence — the shell starts a hub nobody registers with, or joins a port nothing is
/// on. One function, so disagreement is not expressible.</para>
///
/// <para><b>Order.</b> An explicit <c>--port</c> beats the environment, which beats the built-in
/// default. The usual order, and the useful one: the variable sets the machine's habit, the flag
/// overrides it for one run.</para>
/// </summary>
public static class HubAddress
{
    /// <summary>Overrides the built-in default for this machine or this shell session.</summary>
    public const string PortVariable = "SPLA_HUB_PORT";

    /// <summary>
    /// The conventional port. Fixed rather than ephemeral because everything else publishes its
    /// address, while the thing addresses are published *to* has to be findable without being told.
    ///
    /// <para><b>Was 5060; do not put it back.</b> That is SIP's port and browsers refuse to open it
    /// (ERR_UNSAFE_PORT). It cost nothing while the hub only answered machines and broke the moment it
    /// began serving the project manager to a person — see <see cref="BrowserBlocked"/>.</para>
    /// </summary>
    public const int DefaultPort = 5077;

    /// <summary>
    /// Resolves the port to use, and says what was wrong with the attempt when something was.
    /// </summary>
    /// <param name="explicitPort">A port named on the command line, or null when none was.</param>
    /// <param name="warning">Null when everything was fine. Set when the environment variable held
    /// something unusable, or when the chosen port is one browsers will not open — both are worth
    /// saying out loud rather than silently working around, because both produce failures that look
    /// like nothing happening.</param>
    public static int ResolvePort(int? explicitPort, out string? warning)
    {
        warning = null;

        int port;
        if (explicitPort is { } given)
        {
            port = given;
        }
        else
        {
            var raw = Environment.GetEnvironmentVariable(PortVariable);
            if (string.IsNullOrWhiteSpace(raw))
            {
                port = DefaultPort;
            }
            else if (int.TryParse(raw, out var parsed) && parsed is > 0 and < 65536)
            {
                port = parsed;
            }
            else
            {
                // Falling back silently would leave someone certain they had moved the hub while
                // everything quietly used the old number.
                warning = $"{PortVariable} is set to '{raw}', which is not a usable port. Using {DefaultPort}.";
                return DefaultPort;
            }
        }

        if (BrowserBlocked.Contains(port))
        {
            warning = $"Port {port} is on the browsers' blocked list, so the project manager page " +
                      "cannot be opened there (ERR_UNSAFE_PORT). Everything else still works.";
        }

        return port;
    }

    /// <summary>
    /// Ports Chromium and Firefox refuse to make HTTP requests to.
    ///
    /// <para>Kept as data rather than as a comment saying "beware" because the one that bit us — 5060 —
    /// looked like a perfectly sensible service port right up to the moment somebody opened a browser.
    /// A hub is free to run on any of these; it just cannot serve its page to a person there.</para>
    /// </summary>
    private static readonly HashSet<int> BrowserBlocked =
    [
        1, 7, 9, 11, 13, 15, 17, 19, 20, 21, 22, 23, 25, 37, 42, 43, 53, 69, 77, 79, 87, 95,
        101, 102, 103, 104, 109, 110, 111, 113, 115, 117, 119, 123, 135, 137, 138, 139, 143,
        161, 179, 389, 427, 465, 512, 513, 514, 515, 526, 530, 531, 532, 540, 548, 554, 556,
        563, 587, 601, 636, 989, 990, 993, 995, 1719, 1720, 1723, 2049, 3659, 4045, 4190,
        5060, 5061, 6000, 6566, 6665, 6666, 6667, 6668, 6669, 6679, 6697, 10080
    ];
}
