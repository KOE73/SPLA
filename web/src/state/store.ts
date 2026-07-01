import { reactive } from "vue";
import type { ChatSummary } from "../protocol/types";

export const store = reactive({
  connected: false,
  currentChat: null as string | null,
  chats: [] as ChatSummary[],
  attachments: [] as string[],
  workspacePath: null as string | null,
  theme: (localStorage.getItem("spla.theme") || "dark") as string,
  layout: (localStorage.getItem("spla.layout") || "default") as string,
});
