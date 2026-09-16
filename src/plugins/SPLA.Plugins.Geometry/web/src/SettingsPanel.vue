<!--
  The knobs of the markup workspace, in the window instead of in a hand-edited .spla.

  Everything here is a number the owner turns against a live model and then looks at the render: how
  dense the ruler is, how much of the print it covers, how large a digit has to be before the model
  reads it instead of guessing it. That loop is the whole reason these are settings at all
  (src/plugins/SPLA.Plugins.Geometry/AGENTS.md), and a loop that costs a text editor and a restart is
  a loop nobody runs. So the panel's job is not to look clever — it is to make one number cheap to
  change and to say, next to each one, what changing it does.
-->
<template>
  <div class="geom-settings">
    <section>
      <h3>{{ t('Render') }}</h3>
      <label>{{ t('Working size (longest side, px)') }}
        <input type="number" v-model.number="form.render_max_side" min="256" max="4096">
      </label>
      <p class="hint">{{ t('Every render is delivered at this size, and every coordinate the model passes is read in it.') }}</p>
      <label>{{ t('Outline width (px)') }}
        <input type="number" v-model.number="form.line_width" min="1" max="16">
      </label>
      <label>{{ t('Font size') }}
        <input type="number" v-model.number="form.font_size" min="8" max="48">
      </label>
      <p class="hint">{{ t('Object names and every ruler number. A digit too small to read is read anyway, wrongly.') }}</p>
      <label>{{ t('Crop padding (fraction of the box)') }}
        <input type="number" step="0.01" v-model.number="form.crop_padding" min="0" max="1">
      </label>
      <label>{{ t('JPEG quality (0 = PNG)') }}
        <input type="number" v-model.number="form.jpeg_quality" min="0" max="100">
      </label>
      <label>{{ t('Renders kept per chat') }}
        <input type="number" v-model.number="form.render_history" min="1" max="20">
      </label>
    </section>

    <section>
      <h3>{{ t('View grid') }}</h3>
      <label class="check"><input type="checkbox" v-model="form.grid"> {{ t('Draw the grid by default') }}</label>
      <label>{{ t('Spacing (px, 0 = derive from the frame)') }}
        <input type="number" v-model.number="form.grid_step" min="0" max="500">
      </label>
      <label>{{ t('Finest step (px)') }}
        <input type="number" v-model.number="form.grid_min_step" min="4" max="256">
      </label>
      <p class="hint">{{ t('The lattice everything stands on, rulers included. The model reads the frame in patches of roughly this size, so lines closer together than one patch cost legibility and measure nothing.') }}</p>
      <label>{{ t('Fine lines across the frame') }}
        <input type="number" v-model.number="form.grid_lines" min="4" max="100">
      </label>
      <p class="hint">{{ t('What stays constant as the view is cropped: the density of the ruler, not its spacing.') }}</p>
      <label>{{ t('Every Nth line is major and labelled') }}
        <input type="number" v-model.number="form.grid_major_every" min="1" max="20">
      </label>
      <label>{{ t('Transparency (%)') }}
        <input type="number" v-model.number="form.grid_transparency" min="0" max="95">
      </label>
      <p class="hint">{{ t('An instrument you have to hunt for gets guessed past instead of read. If the grid hides too much, coarsen the step rather than fade the line.') }}</p>
      <label>{{ t('Colour') }}
        <span class="colour">
          <input type="color" :value="colourHex" @input="setColour(($event.target as HTMLInputElement).value)">
          <input v-model="form.grid_color" spellcheck="false">
        </span>
      </label>
    </section>

    <section>
      <h3>{{ t('Edge rulers') }}</h3>
      <label class="check"><input type="checkbox" v-model="form.edge_rulers"> {{ t('Draw the rulers by default') }}</label>
      <label class="check"><input type="checkbox" v-model="form.ruler_labels"> {{ t('Number every ruler line') }}</label>
      <label>{{ t('Transparency (%)') }}
        <input type="number" v-model.number="form.ruler_transparency" min="0" max="95">
      </label>
      <p class="hint">{{ t('Solid lines are inside the box, dashed ones outside it; both step on the finest step above. Their spacing is not a setting — it is computed from the box.') }}</p>
    </section>

    <p v-if="error" role="alert">{{ error }}</p>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import type { MountApi } from './settings';
import { t } from './i18n';

const props = defineProps<{ api: MountApi }>();

// The defaults are GeometrySettings' own. They are repeated rather than fetched because the blob is
// empty until the first save, and a panel of blank boxes would then write zeros over every default
// the moment the owner touched one field.
const form = reactive({
  render_max_side: 1024,
  line_width: 3,
  font_size: 16,
  crop_padding: 0.05,
  jpeg_quality: 0,
  render_history: 5,
  grid: true,
  grid_step: 0,
  grid_min_step: 32,
  grid_lines: 24,
  grid_major_every: 4,
  grid_transparency: 10,
  grid_color: '#141414',
  edge_rulers: true,
  ruler_labels: true,
  ruler_transparency: 10
});

const error = ref('');
try { Object.assign(form, JSON.parse(props.api.getJson() || '{}')); }
catch (e) { error.value = String(e); }

// The stored value may carry alpha (#AARRGGBB), which no colour picker understands; the text field
// next to it stays the authority, and the swatch only ever writes plain #RRGGBB.
const colourHex = computed(() => /^#[0-9a-fA-F]{6}$/.test(form.grid_color) ? form.grid_color : '#141414');
function setColour(value: string) { form.grid_color = value; }

function toJson() { return JSON.stringify(form); }
defineExpose({ toJson });
</script>

<style scoped>
.geom-settings { display: grid; gap: 14px; }
section { display: grid; gap: 6px; }
h3, p { margin: 0; }
label { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
label.check { gap: 6px; }
input:not([type=checkbox]):not([type=color]) { flex: 1; min-width: 110px; }
.colour { display: flex; gap: 6px; flex: 1; }
.colour input[type=color] { width: 34px; padding: 0; }
.hint { opacity: .65; font-size: .85em; }
</style>
