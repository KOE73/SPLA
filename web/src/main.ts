import { createApp } from "vue";
import LayoutHost from "./layouts/LayoutHost.vue";
import { client } from "./protocol/SplaClient";
import { store } from "./state/store";
import { bootAppearance } from "./state/appearance";
import { setCurrentProject } from "./state/project";
// Imported for its side effect: the chat-event demultiplexer subscribes on load, and it must be
// listening before the socket opens — a chat.opened that arrives with no session to land in is lost.
import "./state/chatSessions";

bootAppearance();

// The hub surface is served BY the registry hub, which holds no project and has no /ws at all. Opening
// the chat socket there would fail forever and, worse, raise the "the agent stopped answering" banner
// about an agent that was never supposed to exist. It talks to the hub through RegistryClient instead.
const isHubSurface = new URLSearchParams(location.search).get("surface") === "hub";
if (!isHubSurface) client.connect();
client.on("conn", p => { store.connected = p.on; store.connectionLost = !!p.lost; });
client.on("chat.opened", p => { store.currentChat = p.chatId; });
// focus.changed is deliberately NOT applied here. It is broadcast to every connection, so honouring
// it in the main window let any other window retarget this one's chat — including between reading the
// chat for a click and sending the command it produced. Windows that genuinely follow focus (the
// tear-off debug panel) subscribe to it themselves.
client.on("welcome", p => {
  store.workspacePath = p.workspacePath ?? null;
  store.userName = p.userName || null;
  store.branch = p.branch || null;
  // Tear-off windows carry their project in the URL (?project=…) — it must win over the server's
  // default, or a solo terminal/debug window from a non-default project would act on the wrong one.
  const urlProject = new URLSearchParams(location.search).get("project");
  setCurrentProject(urlProject || p.projectId || null, urlProject ? undefined : p.projectName);
  if (p.theme) store.theme = p.theme;
  client.send("chat.list");
});
client.on("appearance.changed", p => { if (p.theme) store.theme = p.theme; });

createApp(LayoutHost).mount("#mount");
