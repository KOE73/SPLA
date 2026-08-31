#!/usr/bin/env node
/**
 * One-shot migration of diagram projects from contract v2 to v3.
 *
 * What it does, per project directory:
 *   1. project.json  — drops `baseLanguage` (there is no base language any more),
 *                      keeps only the languages that still have a file, stamps
 *                      contractVersion 3.
 *   2. text.<lang>   — wraps every bare string into a value carrying provenance:
 *                      { v, at, origin: "authored" }. Nothing is marked
 *                      `translated`: no translation relationship was ever
 *                      recorded, and inventing one would make the first check
 *                      report a clean slate that isn't one.
 *   3. relations.json — moves non-empty `label` into the text catalogue under the
 *                      relation's own id, then drops the key entirely. Empty
 *                      labels (every generated relation has one) just go.
 *   4. relation-types.json — collected from the distinct `type` strings in use.
 *                      Ids stay as they are (`call`, `data-flow`), because
 *                      styles.json matches a style to an element by `type`;
 *                      their text lives under the key `rt_<id>`.
 *   5. views/*.view.json — stamps the classification axis this view's containers
 *                      classify by. Axes are supplied below and were confirmed
 *                      by the owner; a view without one no longer loads.
 *
 * Idempotent: running it twice changes nothing the second time.
 *
 * See ADR_20260831_diagrams_text-provenance-and-view-axes.
 */

import { readFileSync, writeFileSync, existsSync, rmSync, readdirSync } from "node:fs";
import { join } from "node:path";

const ROOT = process.argv[2] ?? "docs/diagrams/projects";
const NOW = new Date().toISOString().replace(/\.\d{3}Z$/, "Z");
const TEXT_FIELDS = ["name", "title", "description", "doc"];

/**
 * The axis each existing view classifies by.
 *
 * `axis_subsystem` — which subsystem of the product the element belongs to.
 * `axis_layer`     — which layer of the running system it lives in.
 * `axis_stage`     — which stage of the pipeline it participates in.
 * `axis_demo`      — a fixture project for editor features, not architecture.
 */
const AXES = {
  core: "axis_subsystem",
  full_core: "axis_subsystem",
  plugins: "axis_subsystem",
  spla_system: "axis_layer",
  llm_pipeline: "axis_stage",
  features: "axis_demo",
};

const read = (p) => JSON.parse(readFileSync(p, "utf8"));
const write = (p, data) => writeFileSync(p, JSON.stringify(data, null, 2) + "\n", "utf8");

let changed = 0;
const log = [];

for (const project of readdirSync(ROOT, { withFileTypes: true }).filter((d) => d.isDirectory())) {
  const dir = join(ROOT, project.name);
  const manifestPath = join(dir, "project.json");
  if (!existsSync(manifestPath)) continue;

  const manifest = read(manifestPath);
  const notes = [];

  // ---------------------------------------------------------------- languages
  delete manifest.baseLanguage;
  const languages = (manifest.languages ?? ["ru"]).filter((lang) =>
    existsSync(join(dir, `text.${lang}.json`)),
  );
  manifest.languages = languages.length > 0 ? languages : ["ru"];
  manifest.contractVersion = 3;

  // -------------------------------------------------------------------- texts
  const catalogues = {};
  for (const lang of manifest.languages) {
    const path = join(dir, `text.${lang}.json`);
    const raw = existsSync(path) ? read(path) : { entries: {} };
    const entries = {};

    for (const [id, entry] of Object.entries(raw.entries ?? {})) {
      if (typeof entry !== "object" || entry === null) continue;
      const out = {};
      for (const field of TEXT_FIELDS) {
        const value = entry[field];
        if (typeof value === "string" && value.length > 0) {
          out[field] = { v: value, at: NOW, origin: "authored" };
        } else if (value && typeof value === "object" && typeof value.v === "string") {
          out[field] = value; // already migrated
        }
      }
      if (Object.keys(out).length > 0) entries[id] = out;
    }

    catalogues[lang] = { contractVersion: 3, language: lang, entries };
  }

  // ---------------------------------------------------------------- relations
  const relationsPath = join(dir, "relations.json");
  const relationTypes = new Map();
  let movedLabels = 0;
  let droppedLabels = 0;

  if (existsSync(relationsPath)) {
    const catalogue = read(relationsPath);
    const primary = manifest.languages[0];

    for (const relation of catalogue.relations ?? []) {
      const type = relation.type ?? relation.relation;
      if (type && !relationTypes.has(type)) {
        relationTypes.set(type, relation.origin === "code" ? "code" : "authored");
      }
      // A type is generated only if every relation using it is.
      if (type && relation.origin !== "code") relationTypes.set(type, "authored");

      if (!("label" in relation)) continue;
      const label = (relation.label ?? "").trim();
      if (label.length > 0) {
        const entries = catalogues[primary].entries;
        entries[relation.id] = { ...entries[relation.id], name: { v: label, at: NOW, origin: "authored" } };
        movedLabels++;
      } else {
        droppedLabels++;
      }
      delete relation.label;
    }

    write(relationsPath, catalogue);
    notes.push(`связи: ${movedLabels} меток в каталог, ${droppedLabels} пустых удалено`);
  }

  // ----------------------------------------------------------- relation types
  if (relationTypes.size > 0) {
    const typesPath = join(dir, "relation-types.json");
    const existing = existsSync(typesPath) ? read(typesPath) : { relationTypes: [] };
    const known = new Map((existing.relationTypes ?? []).map((t) => [t.id, t]));
    const merged = [...relationTypes.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([id, origin]) => known.get(id) ?? { id, origin });
    write(typesPath, { contractVersion: 3, relationTypes: merged });
    notes.push(`типов связей: ${merged.length}`);
  }

  // ------------------------------------------------------------------- views
  const viewsDir = join(dir, "views");
  if (existsSync(viewsDir)) {
    const axis = AXES[project.name];
    for (const file of readdirSync(viewsDir).filter((f) => f.endsWith(".json"))) {
      const path = join(viewsDir, file);
      const view = read(path);
      if (!axis && !view.axis) {
        notes.push(`ВНИМАНИЕ: для вида ${file} не задана ось`);
        continue;
      }
      view.axis = view.axis ?? axis;
      view.contractVersion = 3;
      for (const edge of view.edges ?? []) delete edge.label; // prose left the layout
      write(path, view);
    }
    if (axis) notes.push(`ось: ${axis}`);
  }

  // ------------------------------------------------------------------- write
  for (const [lang, catalogue] of Object.entries(catalogues)) {
    write(join(dir, `text.${lang}.json`), catalogue);
  }

  // Languages dropped from the manifest lose their files: a catalogue nobody
  // declares is a catalogue nothing checks.
  for (const file of readdirSync(dir).filter((f) => /^text\.[a-z-]+\.json$/.test(f))) {
    const lang = file.slice(5, -5);
    if (!manifest.languages.includes(lang)) {
      rmSync(join(dir, file));
      notes.push(`удалён ${file}`);
    }
  }

  write(manifestPath, manifest);
  changed++;
  log.push(`${project.name}: ${notes.join("; ")}`);
}

console.log(`Мигрировано проектов: ${changed}\n`);
for (const line of log) console.log("  " + line);
