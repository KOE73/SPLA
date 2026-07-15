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
  // Tear-off windows carry their project in the URL (?project=…) — it must win over the server's
  // default, or a solo terminal/debug window from a non-default project would act on the wrong one.
  const urlProject = new URLSearchParams(location.search).get("project");
  setCurrentProject(urlProject || p.projectId || null, urlProject ? undefined : p.projectName);
  if (p.theme) store.theme = p.theme;
  client.send("chat.list");
});
client.on("appearance.changed", p => { if (p.theme) store.theme = p.theme; });

createApp(LayoutHost).mount("#mount");
