## Security Modes

SPLA uses several access levels. The mode defines which tools and actions are available to the agent within the current project.

This lets you choose the balance between safety and autonomy depending on the task.

| Mode         | Purpose                            | File reads | File changes | OS commands | Network  | Autonomy |
| ------------ | ---------------------------------- | ---------- | ------------ | ----------- | -------- | -------- |
| **Chat**     | Questions and discussion           | No         | No           | No          | No       | Minimal  |
| **Research** | Documentation and project analysis | Yes        | No           | No          | Optional | Low      |
| **Inspect**  | Diagnostics and system inspection  | Yes        | No           | Limited     | Yes      | Medium   |
| **Edit**     | Project changes                    | Yes        | Yes          | Limited     | Yes      | Medium   |
| **Agent**    | Autonomous task execution          | Yes        | Yes          | Yes         | Yes      | Maximum  |

### Chat

The safest mode.

The agent works only as a conversational assistant and has no access to files, tools, the network, or the operating system.

Use it for discussing ideas, architecture, planning, and consultations.

---

### Research

Analysis mode.

The agent can read project files, documentation, and other permitted data sources, but cannot modify them.

Use it for studying an existing project, preparing reviews, finding information, and explaining code.

Example tasks:

* Explain the project architecture.
* Find all implementations of an interface.
* Prepare a documentation overview.
* Find usages of a class.

---

### Inspect

Diagnostics mode.

The agent gets access to diagnostic tools and can inspect the state of the system, network, and project without making changes.

Use it for finding problems and analyzing infrastructure.

Example tasks:

* Check server availability.
* Run ping and traceroute.
* Find open ports.
* Analyze build results.

---

### Edit

Development mode.

The agent can modify files inside the allowed project workspace.

Use it for writing code, fixing bugs, updating documentation, and refactoring.

Example tasks:

* Fix a compilation error.
* Update documentation.
* Refactor a class.
* Create a new module.

---

### Agent

The most autonomous mode.

The agent can plan a sequence of actions, use tools, run commands, analyze results, and continue working until it reaches the requested goal within the project permissions.

Use it for complex multi-step tasks.

Example tasks:

* Find and fix a problem in the project.
* Build the solution and resolve errors.
* Analyze the system and prepare a report.
* Explore the codebase and implement a new feature.
* Complete a task from start to finish without step-by-step user involvement.

Even in Agent mode, the agent is limited by project settings, allowed tools, the workspace, and security policies.
