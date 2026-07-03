import { createApp } from "vue";
import LayoutHost from "./layouts/LayoutHost.vue";
import { client } from "./protocol/SplaClient";
import { store } from "./state/store";
import { bootAppearance } from "./state/appearance";
import { setCurrentProject } from "./state/project";

bootAppearance();
client.connect();
client.on("conn", p => { store.connected = p.on; });
client.on("chat.opened", p => { store.currentChat = p.chatId; });
client.on("focus.changed", p => { store.currentChat = p.chatId; });
client.on("welcome", p => {
  store.workspacePath = p.workspacePath ?? null;
  store.userName = p.userName || null;
  setCurrentProject(p.projectId ?? null, p.projectName);
  if (p.theme) store.theme = p.theme;
  client.send("chat.list");
});
client.on("appearance.changed", p => { if (p.theme) store.theme = p.theme; });

createApp(LayoutHost).mount("#mount");
