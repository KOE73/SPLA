<!--
  Compact TABLE renderer for arrays of flat objects — restores the spreadsheet-style
  form the legacy React editor had (@jsonforms/vanilla-renderers renders object arrays
  as <table>; the Vue vanilla pack only ships an accordion ArrayListRenderer).

  Each array item = one row; each primitive property = one column with a header.
  Cell editing reuses JsonForms' own <dispatch-renderer> so two-way binding, validation
  and defaults all work exactly like the built-in controls.

  Tester gates to arrays whose items are objects with only primitive properties, so
  nested/complex arrays still fall back to the vanilla accordion (rank 2).
-->
<template>
  <div class="tarr">
    <div class="tarr-head">
      <span class="tarr-title">{{ control.label }}</span>
      <span class="tarr-count">{{ rows.length }}</span>
      <button class="tarr-add" type="button" :disabled="!control.enabled" @click="addRow">
        + Add
      </button>
    </div>

    <div class="tarr-scroll">
      <table>
        <thead>
          <tr>
            <th class="idx">#</th>
            <th v-for="col in columns" :key="col.prop" :style="col.style">{{ col.title }}</th>
            <th class="act" />
          </tr>
        </thead>
        <tbody>
          <tr v-for="(_, index) in rows" :key="index">
            <td class="idx">{{ index + 1 }}</td>
            <td v-for="col in columns" :key="col.prop" class="cell">
              <dispatch-renderer
                :schema="control.schema"
                :uischema="cellUiSchema(col.prop)"
                :path="composePaths(control.path, `${index}`)"
                :enabled="control.enabled"
                :renderers="control.renderers"
                :cells="control.cells"
              />
            </td>
            <td class="act">
              <button
                v-if="control.enabled"
                class="tarr-del"
                type="button"
                title="Remove row"
                @click="removeRow(index)"
              >
                ✕
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <div v-if="rows.length === 0" class="tarr-empty">No rows — click “+ Add”.</div>
    </div>
  </div>
</template>

<script lang="ts">
import {
  composePaths,
  createDefaultValue,
  isObjectArrayControl,
  JsonFormsRendererRegistryEntry,
  rankWith,
  ControlElement,
  UISchemaElement,
} from "@jsonforms/core";
import { defineComponent } from "vue";
import {
  DispatchRenderer,
  rendererProps,
  RendererProps,
  useJsonFormsArrayControl,
} from "@jsonforms/vue";

const renderer = defineComponent({
  name: "TableArrayRenderer",
  components: { DispatchRenderer },
  props: { ...rendererProps<ControlElement>() },
  setup(props: RendererProps<ControlElement>) {
    return useJsonFormsArrayControl(props);
  },
  computed: {
    rows(): unknown[] {
      return (this.control.data as unknown[]) ?? [];
    },
    // Columns derived from the item schema's primitive properties, in declared order.
    columns(): { prop: string; title: string; style: Record<string, string> }[] {
      const items = (this.control.schema as any)?.properties ?? {};
      return Object.keys(items).map((prop) => {
        const p = items[prop] ?? {};
        // Widen description-ish columns, keep short ones tight.
        const wide = prop === "desc" || prop === "note" || prop === "description";
        const style: Record<string, string> = wide
          ? { minWidth: "220px" }
          : { width: "1%", whiteSpace: "nowrap" };
        return { prop, title: (p.title ?? prop) as string, style };
      });
    },
  },
  methods: {
    composePaths,
    cellUiSchema(prop: string): UISchemaElement {
      // A label-less Control targeting a single property → renders just the input cell.
      return {
        type: "Control",
        scope: `#/properties/${prop}`,
        options: { label: false },
      } as UISchemaElement;
    },
    addRow() {
      this.addItem(
        this.control.path,
        createDefaultValue(this.control.schema, this.control.rootSchema)
      )();
    },
    removeRow(index: number) {
      this.removeItems?.(this.control.path, [index])();
    },
  },
});

export default renderer;

export const entry: JsonFormsRendererRegistryEntry = {
  renderer,
  // Rank above the vanilla ArrayListRenderer (rank 2) for object arrays.
  tester: rankWith(4, isObjectArrayControl),
};
</script>

<style scoped>
.tarr {
  display: flex;
  flex-direction: column;
  min-height: 0;
  width: 100%;
}

.tarr-head {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}
.tarr-title {
  font-size: var(--fs-sm);
  font-weight: 600;
  color: var(--text);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.tarr-count {
  font-size: var(--fs-xs);
  color: var(--muted);
  background: color-mix(in srgb, var(--text) 8%, transparent);
  border-radius: 10px;
  padding: 0 7px;
}
.tarr-add {
  margin-left: auto;
  background: var(--accent-soft);
  color: var(--accent);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  font-size: var(--fs-sm);
  padding: 2px 10px;
  cursor: pointer;
}
.tarr-add:hover:not(:disabled) { background: var(--accent); color: #fff; border-color: var(--accent); }
.tarr-add:disabled { opacity: 0.5; cursor: default; }

.tarr-scroll { overflow: auto; border: 1px solid var(--border); border-radius: var(--radius-sm); }

table {
  border-collapse: collapse;
  width: 100%;
  font-size: var(--fs-sm);
}
th {
  position: sticky;
  top: 0;
  background: var(--panel);
  color: var(--muted);
  font-size: var(--fs-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  text-align: left;
  padding: 5px 8px;
  border-bottom: 1px solid var(--border);
  white-space: nowrap;
}
td {
  border-bottom: 1px solid var(--border);
  padding: 1px 4px;
  vertical-align: middle;
}
tbody tr:hover td { background: color-mix(in srgb, var(--text) 4%, transparent); }

.idx {
  width: 1%;
  text-align: right;
  color: var(--muted);
  font-size: var(--fs-xs);
  padding-right: 8px;
  user-select: none;
}
.act { width: 1%; text-align: center; }

.tarr-del {
  background: transparent;
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  color: var(--muted);
  cursor: pointer;
  font-size: var(--fs-sm);
  padding: 1px 6px;
}
.tarr-del:hover { color: #e55; border-color: var(--border); background: color-mix(in srgb, #e55 10%, transparent); }

.tarr-empty {
  padding: 12px;
  text-align: center;
  color: var(--muted);
  font-size: var(--fs-sm);
}

/* Strip the built-in control chrome so each cell is just a bare input. */
:deep(.control) { margin: 0; display: block; }
:deep(.control > label) { display: none; }
:deep(.wrapper) { margin: 0; }
:deep(.input-description) { display: none; }
:deep(input),
:deep(select),
:deep(textarea) {
  background: var(--panel);
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  color: var(--text);
  font-family: var(--font);
  font-size: var(--fs-sm);
  padding: 3px 6px;
  width: 100%;
  box-sizing: border-box;
  outline: none;
}
:deep(input:hover),
:deep(select:hover),
:deep(textarea:hover) { border-color: var(--border); }
:deep(input:focus),
:deep(select:focus),
:deep(textarea:focus) { border-color: var(--accent); background: var(--bg, var(--panel)); }
</style>
