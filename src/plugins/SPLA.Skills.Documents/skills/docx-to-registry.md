---
id: documents.docx-to-registry
description: Read a Word document and record what it says as a row in a spreadsheet registry. Trigger on: add this document to the registry, log this request in the spreadsheet, extract fields from the docx into Excel, record these documents in the table.
---

# Document to registry

Take one or more source documents, pull the facts out of them, and add one row per document to a
spreadsheet — without inventing values and without rewriting anything already in the sheet.

## Tool availability

Use only tools listed in your context: `document_extract`, `spreadsheet_inspect`,
`spreadsheet_read_rows`, `spreadsheet_append_rows`. If they are absent, say so instead of falling
back to reading a `.docx` with a filesystem tool — as a file, a `.docx` is a zip archive and reading
it that way produces nothing usable.

---

## Step 1 — Learn the target before reading the source

Call `spreadsheet_inspect` on the target file FIRST.

It returns the sheet names, the exact column headers and the row count. Those column headers are the
keys every appended row must use, and the only way to know their exact spelling is to have read
them. Do not guess them from the file name or from what a registry "usually" has.

If the target does not exist yet, decide the columns from the task, and pass `create: true` on the
append. Say which columns you are creating.

## Step 2 — Read the source for its meaning

Call `document_extract` on each source document.

- Default (`as: "markdown"`) for reading it yourself.
- `as: "json"` when the document is long and you only need one table or one section — the block tree
  lets you pick that part out instead of re-reading the whole text.
- `output: "blob"` for a document you will pass on rather than read.

## Step 3 — Map facts to columns, and mark what is missing

For each column of the target sheet, find the value in the extracted content.

- Copy values as they are written in the document. Do not reformat, round, or translate them.
- Numbers go in as numbers (`1250000`), not as pre-formatted text (`"1 250 000"`). Dates go in as
  `YYYY-MM-DD` unless the sheet's existing rows clearly use another form — `spreadsheet_read_rows`
  with a small `limit` shows you what the existing rows look like.
- A column you cannot fill from the document is left out of the row object; it stays empty.
- Never put a placeholder, a guess, or a "TBD" into a cell. An empty cell is a fact; an invented
  value is a defect that outlives the conversation.

If a column the user clearly cares about cannot be filled, ask before appending, naming the column
and the document.

## Step 4 — Append

Call `spreadsheet_append_rows` with one object per document, keyed by column header.

```json
{
  "path": "registry.xlsx",
  "sheet": "Requests",
  "rows": [
    { "Date": "2026-08-24", "Company": "Romashka LLC", "Amount": 1250000, "Document": "request.docx" }
  ]
}
```

- Append all rows in ONE call when you have several documents: one call is one write to the file.
- An unknown column header is refused, and the refusal lists the sheet's real columns. Fix the keys
  and call again — never work around it by creating a new file.

## Step 5 — Confirm what landed

Read the tail of the sheet back (`spreadsheet_read_rows` with an `offset` near the end) and report:
how many rows were added, which document each came from, and which cells were left empty and why.

---

## Rules

- One source document produces one row unless the user asked otherwise.
- Never modify existing rows. This procedure only appends.
- If the same document has clearly been recorded already (its identifying column is present in the
  sheet), report that instead of adding a duplicate.
- Report the values you recorded. A registry entry nobody checked is a registry entry nobody trusts.
