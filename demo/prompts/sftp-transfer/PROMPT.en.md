# Prompt — exercising the SFTP tools and containers

[Русский](PROMPT.ru.md) · [Rig setup](SETUP.en.md)

Paste the whole text between the rules into the agent's chat. Replace the host names with your own:
`my-host` is the one that may be written to (`allow_write: true`), `my-readonly-host` is read-only —
leave its settings alone, it exists precisely to prove the refusal.

Many steps **must end in an error** — that is the check. The agent is told so outright, otherwise it
starts "fixing" things and working around them.

---

Test the file transfer tools: `sftp_ls`, `sftp_download`, `sftp_upload`, `sftp_write_file`,
`tar_list`, `tar_read`, `tar_write`. The writable host is `my-host`, the read-only host is `my-readonly-host`.

IMPORTANT: roughly half of these steps MUST END IN A REFUSAL. A refusal where one is expected is a
PASS for that step. Do not work around a refusal, do not try other paths, do not fall back to ssh_run
to move the file anyway. Your job is to record what the tool actually answered. For every step write
down: which tool you called, with which arguments, and what came back (quote error text verbatim —
the text IS the result).

Preparation (on my-host, via ssh_session_exec in a single live session):
  mkdir -p /tmp/spla_sftp/sub
  echo "upper" > /tmp/spla_sftp/README
  echo "lower" > /tmp/spla_sftp/readme
  echo "colon" > /tmp/spla_sftp/'od:d'
  echo "normal" > /tmp/spla_sftp/app.conf
  echo "deep"   > /tmp/spla_sftp/sub/nested.yml
  ln -sf app.conf /tmp/spla_sftp/link.conf
  ls -la /tmp/spla_sftp

Step 1. `sftp_ls` on `/tmp/spla_sftp`, recursive=true.
  Expected: success. The files, the sub directory, and link.conf marked as a link.

Step 2. `sftp_ls` on `~`.
  Expected: success, the user's home directory.

Step 3. `sftp_ls` on `/tmp/spla_sftp/*.conf`.
  Expected: REFUSAL — the path cannot contain wildcards, use 'include' instead.

Step 4. `sftp_download`: remote_path=`/tmp/spla_sftp/app.conf`, local_path=`check/app.conf`.
  Expected: success, one file.

Step 5. `sftp_download`: remote_path=`/tmp/spla_sftp`, local_path=`check/plain`, recursive=true.
  Expected: REFUSAL. The reason must name the actual problem: names Windows cannot store (`od:d`),
  and/or the README versus readme collision, and/or the symbolic link. Quote the text in full.

Step 6. The same, but local_path=`check/set.tar`.
  Expected: success — the container takes what the folder would not.

Step 7. `tar_list` on `check/set.tar`.
  Expected: success. Check that `README` and `readme` are BOTH there as separate files, that `od:d`
  is there, and that `link.conf` shows as a symboliclink with target `-> app.conf`.
  Permissions (mode) are visible on the entries.

Step 8. `tar_read`: path=`check/set.tar`, entry=`tmp/spla_sftp/app.conf`.
  Expected: success, content "normal". If the entry path differs, take the exact one from step 7.

Step 9. The same `tar_read`, but output=`blob`.
  Expected: success, a blob:<handle> came back. Note the handle.

Step 10. `tar_write` into the same container, entry=`tmp/spla_sftp/app.conf`,
  content — edited text (e.g. "normal + edited"), mode=`600`.
  Expected: success. Then `tar_read` the same entry — the content changed, while `tar_list` shows the
  other entries still present and their count not reduced.

Step 11. `tar_write` with delete=true on `tmp/spla_sftp/sub/nested.yml`.
  Expected: success, the entry is gone; `tar_list` confirms only that one went.

Step 12. `tar_write` with delete=true on a non-existent entry `nope.conf`.
  Expected: a message saying there is no such entry. A clear answer, not a thrown exception.

Step 13. `sftp_download`: remote_path=`/`, local_path=`check/everything.tar`, recursive=true.
  Expected: REFUSAL, and QUICKLY, before any transfer starts — on any of the limits: single file
  size, total size, or entry count. Note which limit fired, roughly how long it took, and whether
  everything.tar appeared (it must not).

Step 14. `sftp_download` with local_path=`../escape.tar`.
  Expected: REFUSAL — the path leaves the project.

Step 15. `sftp_download` with local_path=`C:\Temp\escape.tar`.
  Expected: REFUSAL — the path must be relative to the project.

Step 16. `sftp_write_file` to host `my-readonly-host`: remote_path=`/tmp/spla_probe.txt`, content=`x`.
  Expected: REFUSAL — the host is read-only, allow_write is required. Do not try to write there some
  other way.

Step 17. `sftp_write_file` to `my-host`: remote_path=`/tmp/spla_sftp/uploaded.conf`,
  content — the blob handle from step 9.
  Expected: success, with the parent directories created for you. Verify with `sftp_ls` that the file
  appeared and the size matches.

Step 18. Repeat step 17 without overwrite.
  Expected: REFUSAL — the file already exists.

Step 19. Repeat with overwrite=true.
  Expected: success.

Step 20. Create a folder `send/` in the project with three files — `a.conf`, `b.yml`, `sub/c.conf` —
  then `sftp_upload`: local_path=`send`, remote_path=`/tmp/spla_sftp/tree`.
  Expected: ONE call, three files, the subdirectory created. If the agent uploaded one file per call,
  record that separately as a defect.

Step 21. Repeat step 20 with on_conflict=`abort`.
  Expected: REFUSAL, NOT ONE file touched, and the text says how many already exist.

Step 22. Repeat with on_conflict=`skip`.
  Expected: success, 0 transferred, "left as they were: 3".

Step 23. Change only `send/b.yml` locally and repeat with on_conflict=`newer`.
  Expected: exactly one file sent, the others left as same-age or newer.

Step 24. `sftp_upload`: local_path=`send`, remote_path=`/tmp/spla_sftp/tree`, include=`*.conf`.
  Expected: `b.yml` is not sent.

Step 25. `sftp_upload` the container from step 6: local_path=`check/etc.tar`,
  remote_path=`/tmp/spla_sftp/back`.
  Expected: the tree is unpacked, symlinks are symlinks again (check with `sftp_ls`), and the modes
  recorded in the archive are in place.

Step 26. `sftp_upload` with local_path=`../escape`, and with local_path=`send/no-such-file`.
  Expected: REFUSAL in both cases — leaving the project, and "no such file or directory".

Cleanup: remove /tmp/spla_sftp on my-host.

Finish with a table: step | expected (success/refusal) | what happened | ok/fail.
End with the words "done" or "not done".

---

## What each step checks

| Step | Checks |
|---|---|
| 1–2 | listing, type detection, `~` expanded with no shell involved |
| 3 | the absence of globbing is explained, rather than reported as "file not found" |
| 4 | a single file, written atomically via `.part` |
| 5 | **pre-flight**: illegal names, case collisions, symlinks — refusal BEFORE any transfer |
| 6–7 | why the container exists: the same files arrive whole, links are stored as links |
| 8–9 | reading an entry; handing it over through the data channel, bypassing context |
| 10–12 | append-with-replace and delete (a full archive rewrite), nothing else lost |
| 13 | limits fire before the first byte |
| 14–15 | the local path stays inside the project; absolute Windows paths are refused |
| 16 | SFTP bypasses the read-only guard, so writing is gated on `allow_write` — the door is shut |
| 17–19 | writing content from a blob, parent directories created, overwrite semantics |
| 20 | **a whole tree in one call** — the reason upload mirrors download at all |
| 21–23 | the four conflict modes are distinguishable: abort touches nothing, skip counts, newer picks |
| 24 | include/exclude behave the same on the way out as on the way in |
| 25 | a container unpacks back: links and modes as recorded |
| 26 | project boundaries, and a clear "no such file" instead of quietly sending nothing |

## What to look for in the agent's answers

- Refusal texts must stand on their own: name the culprit (which file, which name, how many
  megabytes) and point at the way out ("name the destination .tar"). A vague refusal is a defect in
  the message or the prompt, not in the behaviour.
- After step 5 no partially downloaded files should remain in `check/plain`.
- After step 13 neither `everything.tar` nor `everything.tar.part` should remain.
- If the agent reached for `ssh_run`/`scp` after a refusal, that is a defect in the plugin prompt —
  record it.
