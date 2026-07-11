namespace SPLA.Service.Observability;

/// <summary>Self-contained HTML for the local stats dashboard — no external CSS/JS/fonts, no charting
/// library (the sparklines are hand-drawn inline SVG). Polls the JSON API for the "now" snapshot and
/// the per-period series; renders both a live view and an over-time chart.</summary>
internal static class StatsPage
{
    public static string Render(string who) => $$"""
        <!doctype html><html><head><meta charset=utf-8>
        <meta name=viewport content="width=device-width,initial-scale=1"><title>SPLA — Stats</title>
        <style>
          :root { color-scheme: light dark; --bg:#f6f7f9; --card:#fff; --fg:#1a1d21; --muted:#6b7280;
                  --border:#d9dde3; --accent:#2563eb; --ok:#16a34a; --danger:#dc2626; }
          @media (prefers-color-scheme:dark){ :root{ --bg:#0f1115; --card:#1a1d23; --fg:#e6e8eb;
                  --muted:#9aa1ab; --border:#2a2f37; --accent:#3b82f6; } }
          *{box-sizing:border-box} body{margin:0;font:14px/1.5 system-ui,Segoe UI,Roboto,sans-serif;background:var(--bg);color:var(--fg)}
          .wrap{max-width:1100px;margin:28px auto;padding:0 16px}
          .bar{display:flex;justify-content:space-between;align-items:center;margin-bottom:18px}
          h1{font-size:20px;margin:0}.muted{color:var(--muted);font-size:12px}
          a{color:var(--accent);text-decoration:none}
          .grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(160px,1fr));gap:12px;margin-bottom:18px}
          .card{background:var(--card);border:1px solid var(--border);border-radius:12px;padding:16px}
          .kpi .n{font-size:26px;font-weight:700}.kpi .l{color:var(--muted);font-size:12px;text-transform:uppercase;letter-spacing:.03em}
          .kpi .n.err{color:var(--danger)}
          h2{font-size:14px;margin:0 0 10px;color:var(--muted);text-transform:uppercase;letter-spacing:.03em}
          table{width:100%;border-collapse:collapse}th,td{text-align:left;padding:7px 8px;border-bottom:1px solid var(--border);font-size:13px}
          th{color:var(--muted);font-weight:600;font-size:11px;text-transform:uppercase}
          .row{display:flex;gap:8px;align-items:center;flex-wrap:wrap}
          select{padding:6px 9px;border:1px solid var(--border);border-radius:8px;background:var(--bg);color:var(--fg)}
          svg{display:block;width:100%;height:80px}
          .dot{display:inline-block;width:7px;height:7px;border-radius:50%;background:var(--accent);margin-right:6px}
        </style></head><body><div class=wrap>
          <div class=bar>
            <div><h1 id=title>Server statistics</h1><p class=muted id=uptime>—</p></div>
            <div class=row><span class=muted id=live>● live</span><span class=muted>{{who}}</span><a href="/">← App</a></div>
          </div>
          <div class=grid id=kpis></div>
          <div class=card id=chartcard style="margin-bottom:18px">
            <div class=bar style="margin-bottom:8px">
              <h2 style="margin:0">Over time</h2>
              <div class=row>
                <select id=metric></select>
                <select id=window><option value=60>last hour</option><option value=360>last 6h</option><option value=1440>last 24h</option></select>
              </div>
            </div>
            <svg id=chart viewBox="0 0 600 80" preserveAspectRatio=none></svg>
            <p class=muted id=chartlabel></p>
          </div>
          <div class=card>
            <h2>Recent activity</h2>
            <table><thead><tr><th>When</th><th>Operation</th><th>Tool</th><th>Duration</th></tr></thead>
            <tbody id=recent></tbody></table>
            <p class=muted id=norecent style="display:none">No activity yet.</p>
          </div>
        </div>
        <script>{{Script}}</script>
        </body></html>
        """;

    private const string Script = """
        const $ = s => document.querySelector(s);
        const j = async p => (await fetch('/stats/api' + p)).json();
        const fmt = n => n >= 1000 ? (n/1000).toFixed(n>=10000?0:1)+'k' : String(Math.round(n));
        const dur = ms => ms >= 1000 ? (ms/1000).toFixed(1)+'s' : Math.round(ms)+'ms';
        const esc = s => (s ?? '').replace(/[&<>]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;'}[c]));
        let metrics = [], scope = 'server', feed = [];

        function kpis(s) {
          const t = s.totals || {}, g = s.gauges || {};
          scope = s.scope || 'server';
          const admin = scope === 'server';
          $('#title').textContent = admin ? 'Server statistics' : 'Your activity';
          $('#chartcard').style.display = admin ? '' : 'none';
          const cards = [];
          if (admin) cards.push(['Active connections', fmt(g['connections.active'] ?? 0), false]);
          cards.push(
            ['Tool calls', fmt(t['spla.tool.calls'] ?? 0), false],
            ['Tool errors', fmt(t['spla.tool.errors'] ?? 0), (t['spla.tool.errors'] ?? 0) > 0],
            ['Prompt tokens', fmt(t['spla.tokens.prompt'] ?? 0), false],
            ['Completion tokens', fmt(t['spla.tokens.completion'] ?? 0), false]);
          $('#kpis').innerHTML = cards.map(([l,n,e]) =>
            `<div class="card kpi"><div class="n ${e?'err':''}">${n}</div><div class=l>${l}</div></div>`).join('');
          const up = s.uptimeSeconds || 0, h = Math.floor(up/3600), m = Math.floor((up%3600)/60);
          $('#uptime').textContent = `up ${h}h ${m}m · since ${new Date(s.startedUtc).toLocaleString()}`;
        }
        function renderFeed() {
          $('#norecent').style.display = feed.length ? 'none' : 'block';
          $('#recent').innerHTML = feed.slice(0, 40).map(e =>
            `<tr><td class=muted>${new Date(e.epochMs).toLocaleTimeString()}</td>
             <td>${esc(e.name)}</td><td>${e.tool ? esc(e.tool) : '<span class=muted>—</span>'}</td><td>${dur(e.durationMs)}</td></tr>`).join('');
        }
        function onSnapshot(s) { kpis(s); if (Array.isArray(s.recent)) { feed = s.recent; renderFeed(); } }
        function onEvent(e) { feed.unshift(e); if (feed.length > 60) feed.pop(); renderFeed(); flashLive(); }

        function drawChart(points, isCounter) {
          const w = 600, h = 80, n = points.length;
          const vals = points.map(p => isCounter ? p.sum : (p.count ? p.sum/p.count : 0));
          const max = Math.max(1, ...vals);
          const step = n > 1 ? w/(n-1) : w;
          const path = vals.map((v,i) => `${i?'L':'M'}${(i*step).toFixed(1)},${(h - v/max*(h-6) - 3).toFixed(1)}`).join(' ');
          $('#chart').innerHTML = `<path d="${path}" fill=none stroke="var(--accent)" stroke-width=1.5 vector-effect=non-scaling-stroke/>`;
          const total = vals.reduce((a,b)=>a+b,0);
          $('#chartlabel').textContent = isCounter
            ? `total ${fmt(total)} over ${n} min · peak ${fmt(max)}/min`
            : `avg ${dur(total/(vals.filter(v=>v).length||1))} · peak ${dur(max)}`;
        }
        async function loadChart() {
          if (scope !== 'server') return;
          if (!metrics.length) {
            metrics = await j('/metrics');
            $('#metric').innerHTML = metrics.map(m => `<option value="${m}">${m}</option>`).join('');
            $('#metric').value = metrics.includes('spla.tool.calls') ? 'spla.tool.calls' : (metrics[0]||'');
          }
          const metric = $('#metric').value; if (!metric) return;
          drawChart(await j(`/series?metric=${encodeURIComponent(metric)}&minutes=${+$('#window').value}`), !metric.includes('duration'));
        }
        $('#metric').addEventListener('change', loadChart);
        $('#window').addEventListener('change', loadChart);

        let liveT = null;
        function flashLive() { $('#live').style.opacity = '1'; clearTimeout(liveT); liveT = setTimeout(() => $('#live').style.opacity = '.4', 400); }

        // Real-time: subscribe to the push firehose; poll only as a fallback if the socket drops.
        let poll = null;
        function startPolling() { if (poll) return; const t = async () => { onSnapshot(await j('/snapshot')); await loadChart(); }; t(); poll = setInterval(t, 3000); }
        function stopPolling() { if (poll) { clearInterval(poll); poll = null; } }
        function connect() {
          const proto = location.protocol === 'https:' ? 'wss' : 'ws';
          let ws;
          try { ws = new WebSocket(`${proto}://${location.host}/stats/live`); }
          catch { startPolling(); return; }
          ws.onopen = () => { stopPolling(); $('#live').textContent = '● live'; };
          ws.onmessage = ev => {
            const m = JSON.parse(ev.data);
            if (m.type === 'snapshot') { onSnapshot(m.snapshot); loadChart(); }
            else if (m.type === 'event') onEvent(m.event);
          };
          ws.onclose = () => { $('#live').textContent = '○ reconnecting'; startPolling(); setTimeout(connect, 4000); };
          ws.onerror = () => { try { ws.close(); } catch {} };
        }
        connect();
        """;
}
