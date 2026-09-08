// What the dictionary is missing, and what it holds that nobody asks for any more.
//
// Run: npm run i18n:scan  (add --json for machine-readable output)
//
// Scans every t('…') / t("…") call in the web client AND in the plugin bundles — plugins borrow the
// host's translator, so their strings look up in the same table — then compares that set against
// each dictionary in src/i18n/*.json. Nothing here fails a build: a missing key renders as English,
// which is a degraded UI, not a broken one. It is a checklist, not a gate.
import { readFileSync, readdirSync, statSync } from "node:fs";
import { join, relative, resolve } from "node:path";

const webRoot = resolve(import.meta.dirname, "..");
const repoRoot = resolve(webRoot, "..");
const scanRoots = [
  join(webRoot, "src"),
  join(repoRoot, "src", "plugins"),
];

/** Every .vue/.ts under a root, skipping build output, dependencies and tests. */
function walk(dir, out = []) {
  let entries;
  try { entries = readdirSync(dir); } catch { return out; }
  for (const name of entries) {
    if (name === "node_modules" || name === "dist" || name === "bin" || name === "obj") continue;
    const full = join(dir, name);
    if (statSync(full).isDirectory()) { walk(full, out); continue; }
    if (!/\.(vue|ts)$/.test(name) || name.includes(".test.")) continue;
    if (/i18n[\\/]index\.ts$|[\\/]i18n\.ts$/.test(full)) continue;   // the machinery itself
    out.push(full);
  }
  return out;
}

// t('…') and t("…"), including a call whose literal is split across concatenated lines. Deliberately
// simple: a key built at runtime (t(someVariable)) cannot be found by reading the source, and is
// meant to be — those strings are English source text held elsewhere, and show up here only when
// their own literal is wrapped where it is written.
const CALL = /\bt\(\s*('(?:[^'\\]|\\.)*'|"(?:[^"\\]|\\.)*")((?:\s*\+\s*(?:'(?:[^'\\]|\\.)*'|"(?:[^"\\]|\\.)*"))*)/g;

function literalValue(text) {
  const parts = text.match(/'(?:[^'\\]|\\.)*'|"(?:[^"\\]|\\.)*"/g) ?? [];
  return parts.map(p => p.slice(1, -1).replace(/\\n/g, "\n").replace(/\\'/g, "'").replace(/\\"/g, '"')).join("");
}

// A key written inside an attribute (v-html="t('…')") is HTML-decoded once before Vue compiles the
// expression, so the string that actually reaches t() — and therefore the dictionary key — is the
// decoded one. Decode here too, or the report chases keys that never exist at runtime.
function decodeAttribute(key) {
  return key
    .replace(/&quot;/g, '"')
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&amp;/g, "&");
}

const used = new Map();  // key -> [files]
for (const root of scanRoots) {
  for (const file of walk(root)) {
    const src = readFileSync(file, "utf8");
    for (const m of src.matchAll(CALL)) {
      const inAttribute = src.slice(Math.max(0, m.index - 10), m.index).includes('="');
      let key = literalValue(m[1] + (m[2] ?? ""));
      if (inAttribute) key = decodeAttribute(key);
      if (!key.trim()) continue;
      if (!used.has(key)) used.set(key, []);
      used.get(key).push(relative(repoRoot, file));
    }
  }
}

const dictDir = join(webRoot, "src", "i18n");
const dicts = readdirSync(dictDir).filter(f => f.endsWith(".json"));
const report = { total: used.size, locales: {} };

for (const file of dicts) {
  const dict = JSON.parse(readFileSync(join(dictDir, file), "utf8"));
  const missing = [...used.keys()].filter(k => !(k in dict)).sort();
  const stale = Object.keys(dict).filter(k => !used.has(k)).sort();
  report.locales[file.replace(/\.json$/, "")] = { missing, stale };
}

if (process.argv.includes("--json")) {
  console.log(JSON.stringify(report, null, 1));
} else {
  console.log(`${report.total} strings wrapped for translation\n`);
  for (const [locale, { missing, stale }] of Object.entries(report.locales)) {
    console.log(`── ${locale}: ${report.total - missing.length}/${report.total} translated`);
    if (missing.length) {
      console.log(`   ${missing.length} missing (these render in English):`);
      for (const k of missing) console.log(`     · ${JSON.stringify(k)}`);
    }
    // Not necessarily dead: a key reached through a variable — a panel title from the catalog, a
    // scope label from the store, a button label passed as a prop — is translated at render and is
    // invisible to a source scan. Read this list as "check these", not "delete these".
    if (stale.length) {
      console.log(`   ${stale.length} not found as a literal (indirect keys, or genuinely unused):`);
      for (const k of stale) console.log(`     · ${JSON.stringify(k)}`);
    }
    console.log("");
  }
}
