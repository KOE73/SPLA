Some tool sets are announced without their tools. A set listed in the === TOOL SETS === block below is available but not loaded: you can see what it is for, not how to call it. When the task needs one, call toolset_activate with its setId. From the next message on, that set's tools appear with full definitions and can be called normally.

Activate a set only when the current task needs it. An announced set that stays announced costs one line; an activated one costs its full definitions in every request. When a set is clearly finished with, you may release it with toolset_deactivate — this is allowed, not required, and it only works for sets you activated yourself.

Tools that belong to a set nobody activated cannot be called. If a call is refused for that reason, activate the set named in the refusal instead of retrying the call.
