using System.Net;

namespace SPLA.Service.Auth;

/// <summary>Self-contained HTML for the local-auth pages (login, register, self-service account, and
/// the admin panel). Everything is inlined — no external CSS/JS/fonts — so the pages render before the
/// user has any session and never depend on the embedded web-client bundle.</summary>
internal static class AuthPages
{
    private const string Style = """
        <style>
          :root { color-scheme: light dark; --bg:#f6f7f9; --card:#fff; --fg:#1a1d21; --muted:#6b7280;
                  --border:#d9dde3; --accent:#2563eb; --accent-fg:#fff; --danger:#dc2626; --ok:#16a34a; }
          @media (prefers-color-scheme: dark) {
            :root { --bg:#0f1115; --card:#1a1d23; --fg:#e6e8eb; --muted:#9aa1ab;
                    --border:#2a2f37; --accent:#3b82f6; } }
          * { box-sizing:border-box; }
          body { margin:0; font:14px/1.5 system-ui,-apple-system,Segoe UI,Roboto,sans-serif;
                 background:var(--bg); color:var(--fg); }
          .wrap { max-width:420px; margin:8vh auto; padding:0 16px; }
          .admin-wrap { max-width:1100px; margin:32px auto; padding:0 16px; }
          .card { background:var(--card); border:1px solid var(--border); border-radius:12px;
                  padding:28px; box-shadow:0 1px 3px rgba(0,0,0,.06); }
          h1 { font-size:20px; margin:0 0 4px; } h2 { font-size:15px; margin:24px 0 8px; }
          .sub { color:var(--muted); margin:0 0 20px; }
          label { display:block; font-weight:600; margin:14px 0 4px; font-size:13px; }
          input[type=text],input[type=password],input[type=search],select {
            width:100%; padding:9px 11px; border:1px solid var(--border); border-radius:8px;
            background:var(--bg); color:var(--fg); font-size:14px; }
          button { cursor:pointer; border:0; border-radius:8px; padding:9px 14px; font-size:14px;
                   font-weight:600; background:var(--accent); color:var(--accent-fg); }
          button.secondary { background:transparent; color:var(--fg); border:1px solid var(--border); }
          button.danger { background:var(--danger); color:#fff; }
          button.small { padding:5px 9px; font-size:12px; }
          .row { display:flex; gap:8px; align-items:center; flex-wrap:wrap; }
          .full { width:100%; margin-top:20px; }
          a { color:var(--accent); text-decoration:none; } a:hover { text-decoration:underline; }
          .err { background:color-mix(in srgb,var(--danger) 12%,transparent); color:var(--danger);
                 border:1px solid color-mix(in srgb,var(--danger) 35%,transparent); border-radius:8px;
                 padding:9px 11px; margin:16px 0 0; font-size:13px; }
          .muted { color:var(--muted); font-size:12px; }
          table { width:100%; border-collapse:collapse; margin-top:8px; }
          th,td { text-align:left; padding:9px 10px; border-bottom:1px solid var(--border); font-size:13px; vertical-align:middle; }
          th { color:var(--muted); font-weight:600; font-size:12px; text-transform:uppercase; letter-spacing:.03em; }
          .tag { display:inline-block; padding:1px 7px; border-radius:999px; font-size:11px; font-weight:600;
                 border:1px solid var(--border); margin:1px; }
          .tag.admin { background:color-mix(in srgb,var(--accent) 15%,transparent); border-color:var(--accent); }
          .pill { font-size:11px; font-weight:700; } .pill.on { color:var(--ok); } .pill.off { color:var(--danger); }
          .bar { display:flex; justify-content:space-between; align-items:center; margin-bottom:16px; }
          dialog { border:1px solid var(--border); border-radius:12px; background:var(--card); color:var(--fg);
                   padding:24px; max-width:420px; width:92%; }
          dialog::backdrop { background:rgba(0,0,0,.4); }
        </style>
        """;

    private static string Page(string title, string body)
        => $"<!doctype html><html><head><meta charset=utf-8><meta name=viewport content=\"width=device-width,initial-scale=1\"><title>{title}</title>{Style}</head><body>{body}</body></html>";

    private static string Enc(string? s) => WebUtility.HtmlEncode(s ?? "");

    public static string Login(string? returnUrl, string? error, bool allowRegister)
    {
        var err = string.IsNullOrEmpty(error) ? "" : $"<div class=err>{Enc(error)}</div>";
        var reg = allowRegister
            ? $"<p class=muted style=\"margin-top:18px\">No account? <a href=\"/register?returnUrl={Enc(Uri.EscapeDataString(returnUrl ?? "/"))}\">Register</a></p>"
            : "";
        var body = $"""
            <div class=wrap><div class=card>
              <h1>Sign in to SPLA</h1>
              <p class=sub>Local account</p>
              <form method=post action="/login">
                <input type=hidden name=returnUrl value="{Enc(returnUrl)}">
                <label for=u>Username</label>
                <input id=u name=username type=text autocomplete=username autofocus required>
                <label for=p>Password</label>
                <input id=p name=password type=password autocomplete=current-password required>
                {err}
                <button class=full type=submit>Sign in</button>
              </form>
              {reg}
            </div></div>
            """;
        return Page("Sign in — SPLA", body);
    }

    public static string Register(string? returnUrl, string? error)
    {
        var err = string.IsNullOrEmpty(error) ? "" : $"<div class=err>{Enc(error)}</div>";
        var body = $"""
            <div class=wrap><div class=card>
              <h1>Create a SPLA account</h1>
              <p class=sub>Choose a username and password</p>
              <form method=post action="/register">
                <input type=hidden name=returnUrl value="{Enc(returnUrl)}">
                <label for=d>Display name</label>
                <input id=d name=displayName type=text autocomplete=name>
                <label for=u>Username</label>
                <input id=u name=username type=text autocomplete=username required>
                <label for=p>Password</label>
                <input id=p name=password type=password autocomplete=new-password required>
                {err}
                <button class=full type=submit>Create account</button>
              </form>
              <p class=muted style="margin-top:18px"><a href="/login">Back to sign in</a></p>
            </div></div>
            """;
        return Page("Register — SPLA", body);
    }

    public static string AccessDenied()
        => Page("Access denied — SPLA", """
            <div class=wrap><div class=card>
              <h1>Access denied</h1>
              <p class=sub>Your account doesn't have permission to view this page (administrator role required).</p>
              <div class=row><a href="/"><button class=secondary>Open app</button></a><a href="/account"><button class=secondary>My account</button></a></div>
            </div></div>
            """);

    public static string Account(string displayName, string username, string? notice)
    {
        var note = string.IsNullOrEmpty(notice) ? "" : $"<div class=err style=\"color:var(--ok);border-color:var(--ok)\">{Enc(notice)}</div>";
        var body = $"""
            <div class=wrap><div class=card>
              <div class=bar><h1>Your account</h1><a href="/">← App</a></div>
              <p class=sub>{Enc(displayName)} — <span class=muted>{Enc(username)}</span></p>
              <h2>Change password</h2>
              <form method=post action="/account/password">
                <label for=c>Current password</label>
                <input id=c name=current type=password autocomplete=current-password required>
                <label for=n>New password</label>
                <input id=n name=next type=password autocomplete=new-password required>
                {note}
                <button class=full type=submit>Update password</button>
              </form>
              <p class=muted style="margin-top:18px"><a href="/logout">Sign out</a></p>
            </div></div>
            """;
        return Page("Account — SPLA", body);
    }

    public static string Admin(string adminName)
        => Page("Admin — SPLA", $$"""
            <div class=admin-wrap>
              <div class=bar>
                <div><h1>User administration</h1><p class=muted>Signed in as {{Enc(adminName)}}</p></div>
                <div class=row>
                  <a href="/stats"><button class=secondary>Statistics</button></a>
                  <a href="/account"><button class=secondary>My account</button></a>
                  <a href="/"><button class=secondary>Open app</button></a>
                  <a href="/logout"><button class=secondary>Sign out</button></a>
                </div>
              </div>
              <div class=card>
                <div class=bar>
                  <input id=filter type=search placeholder="Filter users…" style="max-width:260px">
                  <button id=newBtn>+ New user</button>
                </div>
                <table><thead><tr>
                  <th>User</th><th>Username</th><th>Roles</th><th>Groups</th><th>Status</th><th>Last login</th><th></th>
                </tr></thead><tbody id=rows></tbody></table>
                <p id=empty class=muted style="display:none">No users.</p>
              </div>
            </div>

            <dialog id=editDlg><form id=editForm method=dialog>
              <h1 id=dlgTitle>New user</h1>
              <input type=hidden id=f_key>
              <label>Display name</label><input id=f_display type=text>
              <label>Username</label><input id=f_username type=text>
              <label id=pwLabel>Password</label><input id=f_password type=password autocomplete=new-password>
              <label>Roles <span class=muted>(comma-separated, e.g. admin, user)</span></label><input id=f_roles type=text>
              <label>Groups <span class=muted>(comma-separated)</span></label><input id=f_groups type=text>
              <div id=dlgErr class=err style="display:none"></div>
              <div class=row style="margin-top:20px;justify-content:flex-end">
                <button type=button class=secondary id=cancelBtn>Cancel</button>
                <button type=button id=saveBtn>Save</button>
              </div>
            </form></dialog>

            <script>{{AdminScript}}</script>
            """);

    private const string AdminScript = """
        const $ = id => document.getElementById(id);
        const api = (p, opt) => fetch('/admin/api' + p, Object.assign({ headers: { 'content-type': 'application/json' } }, opt));
        let users = [];
        const esc = s => (s ?? '').replace(/[&<>"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));

        async function load() {
          const r = await api('/users'); users = await r.json();
          render();
        }
        function render() {
          const f = ($('filter').value || '').toLowerCase();
          const tb = $('rows'); tb.innerHTML = '';
          const shown = users.filter(u => !f || u.username.toLowerCase().includes(f) || (u.displayName||'').toLowerCase().includes(f));
          $('empty').style.display = shown.length ? 'none' : 'block';
          for (const u of shown) {
            const roles = (u.roles||[]).map(r => `<span class="tag ${r==='admin'?'admin':''}">${esc(r)}</span>`).join('');
            const groups = (u.groups||[]).map(g => `<span class=tag>${esc(g)}</span>`).join('') || '<span class=muted>—</span>';
            const status = u.enabled ? '<span class="pill on">● enabled</span>' : '<span class="pill off">● disabled</span>';
            const last = u.lastLoginUtc ? new Date(u.lastLoginUtc).toLocaleString() : '<span class=muted>never</span>';
            const tr = document.createElement('tr');
            tr.innerHTML = `<td>${esc(u.displayName)}</td><td>${esc(u.username)}</td><td>${roles}</td><td>${groups}</td>
              <td>${status}</td><td>${last}</td>
              <td><div class=row>
                <button class="small secondary" data-edit="${u.userKey}">Edit</button>
                <button class="small secondary" data-toggle="${u.userKey}">${u.enabled?'Disable':'Enable'}</button>
                <button class="small danger" data-del="${u.userKey}">Delete</button>
              </div></td>`;
            tb.appendChild(tr);
          }
        }
        function openDlg(u) {
          $('dlgTitle').textContent = u ? 'Edit user' : 'New user';
          $('f_key').value = u ? u.userKey : '';
          $('f_display').value = u ? u.displayName : '';
          $('f_username').value = u ? u.username : '';
          $('f_username').disabled = !!u;
          $('f_password').value = '';
          $('pwLabel').textContent = u ? 'New password (leave blank to keep)' : 'Password';
          $('f_roles').value = u ? (u.roles||[]).join(', ') : 'user';
          $('f_groups').value = u ? (u.groups||[]).join(', ') : '';
          $('dlgErr').style.display = 'none';
          $('editDlg').showModal();
        }
        function fail(msg) { $('dlgErr').textContent = msg; $('dlgErr').style.display = 'block'; }
        const split = v => v.split(',').map(s => s.trim()).filter(Boolean);

        async function save() {
          const key = $('f_key').value;
          const body = {
            username: $('f_username').value.trim(),
            displayName: $('f_display').value.trim(),
            password: $('f_password').value,
            roles: split($('f_roles').value),
            groups: split($('f_groups').value)
          };
          let r;
          if (!key) r = await api('/users', { method:'POST', body: JSON.stringify(body) });
          else r = await api('/users/' + encodeURIComponent(key), { method:'PUT', body: JSON.stringify(body) });
          if (!r.ok) { fail((await r.json()).error || 'Failed.'); return; }
          $('editDlg').close(); load();
        }
        document.addEventListener('click', async e => {
          const t = e.target;
          if (t.id === 'newBtn') openDlg(null);
          else if (t.id === 'saveBtn') save();
          else if (t.id === 'cancelBtn') $('editDlg').close();
          else if (t.dataset.edit) openDlg(users.find(u => u.userKey === t.dataset.edit));
          else if (t.dataset.toggle) {
            const u = users.find(x => x.userKey === t.dataset.toggle);
            const r = await api('/users/' + encodeURIComponent(u.userKey) + '/enabled', { method:'POST', body: JSON.stringify({ enabled: !u.enabled }) });
            if (!r.ok) alert((await r.json()).error || 'Failed.'); load();
          } else if (t.dataset.del) {
            const u = users.find(x => x.userKey === t.dataset.del);
            if (!confirm('Delete user "' + u.username + '"?')) return;
            const r = await api('/users/' + encodeURIComponent(u.userKey), { method:'DELETE' });
            if (!r.ok) alert((await r.json()).error || 'Failed.'); load();
          }
        });
        $('filter').addEventListener('input', render);
        load();
        """;
}
