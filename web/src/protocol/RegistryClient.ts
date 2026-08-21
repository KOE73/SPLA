/**
 * Talks to a registry hub.
 *
 * Deliberately NOT SplaClient. The hub speaks its own small vocabulary — register, status, stop,
 * focus — and has no chat protocol at all; pointing the chat client at it would open a socket to an
 * endpoint that does not exist and then report the agent as lost. The two protocols were kept apart
 * on the wire for exactly this reason (see RegistryFrames in the C# contracts), and the client side
 * follows that split rather than papering over it.
 *
 * Served BY the hub, so every address here is relative: the page and the routes are the same origin
 * by construction.
 */

export interface KnownProject {
  projectId: string;
  name?: string;
  exists: boolean;
  /** Agent state, or null/undefined when nothing holds this project. */
  state?: string | null;
  instanceId?: string | null;
  windows: number;
  /** True when the agent published an address — /mcp is dialable. False for a stdio-only holder
   *  (`spla mcp`'s own-body mode, a bare REPL): it has the project but nothing to point a second
   *  MCP client at. */
  mcpAvailable?: boolean;
}

type ProjectsListener = (projects: KnownProject[]) => void;
type ConnListener = (up: boolean) => void;

export class RegistryClient {
  private ws?: WebSocket;
  private reconnectTimer = 0;
  private projectsListeners = new Set<ProjectsListener>();
  private connListeners = new Set<ConnListener>();

  /** The known-projects list. Re-fetched rather than patched whenever the hub says anything changed:
   *  it is short, it is cheap, and a list rebuilt from the source cannot drift from it. */
  async projects(): Promise<KnownProject[]> {
    const res = await fetch("/registry/projects");
    if (!res.ok) throw new Error(`projects: ${res.status}`);
    const body = await res.json();
    return body.projects ?? [];
  }

  /**
   * Watches the hub's instance listing. The payload is ignored — what matters is that SOMETHING
   * changed, which is the cue to re-read the projects list. The listing alone could not drive this
   * page anyway: it cannot represent a project with nothing running, and that is half the rows.
   */
  watch(): void {
    const proto = location.protocol === "https:" ? "wss://" : "ws://";
    this.ws = new WebSocket(proto + location.host + "/registry/watch");

    this.ws.onopen = () => this.connListeners.forEach(fn => fn(true));
    this.ws.onmessage = () => { void this.refresh(); };
    this.ws.onclose = () => {
      this.connListeners.forEach(fn => fn(false));
      clearTimeout(this.reconnectTimer);
      // A hub going away is far less alarming than an agent going away: nothing is lost with it, and
      // it comes back the moment anything starts one. A steady retry is the whole response.
      this.reconnectTimer = window.setTimeout(() => this.watch(), 2000);
    };
  }

  async refresh(): Promise<void> {
    try {
      const projects = await this.projects();
      this.projectsListeners.forEach(fn => fn(projects));
    } catch {
      /* the hub answered badly or went away; the watch socket's own retry covers it */
    }
  }

  onProjects(fn: ProjectsListener): () => void {
    this.projectsListeners.add(fn);
    return () => { this.projectsListeners.delete(fn); };
  }

  onConn(fn: ConnListener): () => void {
    this.connListeners.add(fn);
    return () => { this.connListeners.delete(fn); };
  }

  /** Starts an agent on a project. Resolves to an error string, or null when it worked (already
   *  running counts as worked — the caller asked for the project to be up). */
  async start(projectId: string): Promise<string | null> {
    const res = await fetch(`/registry/start?project=${encodeURIComponent(projectId)}`, { method: "POST" });
    if (res.status === 501) return "This hub is not allowed to start projects.";
    if (res.ok) return null;
    try {
      const body = await res.json();
      return body.error || `Could not start it (${res.status}).`;
    } catch { return `Could not start it (${res.status}).`; }
  }

  /** Closes everything on a project — agent and windows together. `force` discards a running turn. */
  async close(projectId: string, force: boolean): Promise<string | null> {
    const res = await fetch(
      `/registry/stop-project?project=${encodeURIComponent(projectId)}&force=${force}`, { method: "POST" });
    if (res.status === 404) return "Nothing is running on it.";
    return res.ok ? null : `The hub refused (${res.status}).`;
  }

  /** Raises a window that already has the project. */
  async focus(instanceId: string): Promise<boolean> {
    const res = await fetch(`/registry/focus?instance=${encodeURIComponent(instanceId)}`, { method: "POST" });
    return res.ok;
  }

  /** Drops a project from the remembered list. Forgets the memory, never the project. */
  async forget(projectId: string): Promise<boolean> {
    const res = await fetch(`/registry/forget?project=${encodeURIComponent(projectId)}`, { method: "POST" });
    return res.ok;
  }

  dispose(): void {
    clearTimeout(this.reconnectTimer);
    this.ws?.close();
  }
}

export const registryClient = new RegistryClient();
