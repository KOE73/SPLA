import { createApp } from "vue";
import LayoutHost from "./layouts/LayoutHost.vue";
import { client } from "./protocol/SplaClient";
import { store } from "./state/store";
import { bootAppearance } from "./state/appearance";

bootAppearance();
client.connect();
client.on("conn", p => { store.connected = p.on; });
client.on("chat.opened", p => { store.currentChat = p.chatId; });
client.on("focus.changed", p => { store.currentChat = p.chatId; });
client.on("welcome", p => {
  store.workspacePath = p.workspacePath ?? null;
  store.currentProjectId = p.projectId ?? null;
  store.currentProjectName = p.projectName ?? null;
  if (p.theme) store.theme = p.theme;
  client.send("chat.list");
});
client.on("appearance.changed", p => { if (p.theme) store.theme = p.theme; });

createApp(LayoutHost).mount("#mount");
