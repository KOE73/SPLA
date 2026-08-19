/**
 * The browser half of the secret store — ONE place that talks `secret.*`, shared by the Secrets
 * settings panel, the credential picker and (through the plugin mount API) every plugin panel.
 * Nothing else in web/ may call `secret.list/set/delete` directly: a second caller is a second
 * chance to mistake a rejection for success.
 *
 * One thing the raw protocol makes easy to get wrong, fixed here once:
 *  - `secret.*` never rejects. The server always answers `secret.result` and reports refusals in
 *    `error` (scope missing, no project open, ACL). `client.invoke` resolves on any answer, so a
 *    caller that only try/catches sees a failed write as a success. Every call below THROWS on
 *    `error` — callers get one honest failure path.
 *
 * Values are write-only, exactly as the protocol promises: they go browser → server and the state
 * below only ever holds keys, scopes and field NAMES.
 */
import { reactive } from "vue";
import { client } from "../protocol/SplaClient";
import type { SecretEntryDto, SecretListResultPayload, SecretScopeId } from "../protocol/types";

/** Conventional field names (mirrors SecretFields in the domain). Anything else via "custom…". */
export const FIELD_NAMES = ["password", "user", "token", "private_key", "passphrase"];

/** Field names whose value is a multi-line blob and needs a textarea rather than one input. */
const MULTILINE_FIELDS = ["private_key"];
export const isMultilineField = (name: string) => MULTILINE_FIELDS.includes(name.trim().toLowerCase());

export interface ScopeMeta { id: SecretScopeId; label: string; sub: string }

/** Presented in this order everywhere, with the same wording — the scope is part of the identity of
 * an entry, not a detail, so the picker and the panel must not describe it differently. */
export const SCOPES: ScopeMeta[] = [
  { id: "user", label: "User", sub: "mine only — never shared with anyone" },
  { id: "project", label: "Project", sub: "<workspace>/.spla — travels with the project" },
  { id: "shared", label: "Shared", sub: "administered — visible only to those it is granted to" }
];

export const secrets = reactive({
  user: [] as SecretEntryDto[],
  project: [] as SecretEntryDto[],
  shared: [] as SecretEntryDto[],
  projectOpen: false,
  loaded: false
});

/** Entries of one scope; `project` is empty (and writes are refused) when no project is open. */
export function entriesOf(scope: SecretScopeId): SecretEntryDto[] {
  return secrets[scope];
}

/** All entries, in scope order — what a picker lists. */
export function allEntries(): SecretEntryDto[] {
  return SCOPES.flatMap(s => secrets[s.id]);
}

export function findByReference(reference: string): SecretEntryDto | undefined {
  const bare = reference.split("#")[0];
  return allEntries().find(e => e.reference === bare);
}

/** True when a scope cannot be written to right now (project scope without an open project). */
export function scopeDisabled(scope: SecretScopeId) {
  return scope === "project" && !secrets.projectOpen;
}

function apply(p: SecretListResultPayload) {
  secrets.user = p.user || [];
  secrets.project = p.project || [];
  secrets.shared = p.shared || [];
  secrets.projectOpen = !!p.projectOpen;
  secrets.loaded = true;
}

/** Every `secret.*` call goes through here: state refreshed, refusal turned into a throw. */
async function call(type: string, payload?: unknown): Promise<void> {
  const p = await client.invoke<SecretListResultPayload>(type, payload);
  apply(p);
  if (p.error) throw new Error(p.error);
}

export const loadSecrets = () => call("secret.list");

/** Writes fields into one entry. The server MERGES over the existing record, so writing one field
 * neither wipes its siblings nor needs a read first — and re-writing an existing name is how a
 * value gets edited. */
export const setSecret = (scope: SecretScopeId, key: string, fields: Record<string, string>) =>
  call("secret.set", { key, fields, scope });

/** Whole entry when `field` is omitted, one field otherwise (the server drops an emptied entry). */
export const deleteSecret = (scope: SecretScopeId, key: string, field?: string) =>
  call("secret.delete", { key, field, scope });

/** Loads once per session for consumers that just need the list (pickers). */
export function ensureLoaded(): Promise<void> {
  return secrets.loaded ? Promise.resolve() : loadSecrets();
}

// A reconnect invalidates everything we know: re-read rather than show a stale list.
client.on("welcome", () => { secrets.loaded = false; });
