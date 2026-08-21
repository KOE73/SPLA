import { reactive } from "vue";
import type { ChatSummary, ProjectDescriptor } from "../protocol/types";

/**
 * Application state: what is true of this window and this connection, never of one chat.
 *
 * Per-chat state (log, draft, attachments, in-flight turn, context budget, active skill, tool sets,
 * doubt) lives in state/chatSessions — one session per chat, so a background chat's traffic has
 * somewhere of its own to land instead of overwriting whatever is on screen.
 */
export const store = reactive({
  connected: false,
  /**
   * True once retrying has failed often enough that the agent is presumed gone rather than briefly
   * away. Kept apart from `connected` because they call for different things on screen: a blip is
   * worth a quiet indicator, a stopped agent is worth saying out loud — a window that silently
   * retried forever was the whole "orphan window" complaint.
   */
  connectionLost: false,
  currentChat: null as string | null,
  chats: [] as ChatSummary[],
  workspacePath: null as string | null,
  /** Authenticated user (server mode); null on local/embedded → identity row hidden. */
  userName: null as string | null,
  /** Non-null only when this build was published from a branch other than main. */
  branch: null as string | null,
  theme: (localStorage.getItem("spla.theme") || "dark") as string,

  // ── Project focus (the server binds the connection's project; project.open rebinds it) ──
  /** null = this connection's default project (single-project usage never sets this). */
  currentProjectId: null as string | null,
  currentProjectName: null as string | null,
  /** Known projects (from project.list/project.recent) — populated when the picker opens. */
  projects: [] as ProjectDescriptor[],
  projectPickerOpen: false,
});
