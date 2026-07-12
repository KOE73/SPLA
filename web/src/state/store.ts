import { reactive } from "vue";
import type { ChatSummary, ProjectDescriptor } from "../protocol/types";

export const store = reactive({
  connected: false,
  currentChat: null as string | null,
  chats: [] as ChatSummary[],
  attachments: [] as string[],
  workspacePath: null as string | null,
  /** Authenticated user (server mode); null on local/embedded → identity badge hidden. */
  userName: null as string | null,
  theme: (localStorage.getItem("spla.theme") || "dark") as string,

  // ── Project focus (Phase 2.2 protocol: ProjectId rides on every chat-scoped envelope) ──
  /** null = this connection's default project (single-project usage never sets this). */
  currentProjectId: null as string | null,
  currentProjectName: null as string | null,
  /** Known projects (from project.list/project.recent) — populated when the picker opens. */
  projects: [] as ProjectDescriptor[],
  projectPickerOpen: false,
});
