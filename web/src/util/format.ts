/** Compact token counts: "18.5k", "1.2M". Both status bars are narrow, so raw counts never fit. */
export function formatCompact(n: number): string {
  if (n >= 1_000_000) return trimZero(n / 1_000_000) + "M";
  if (n >= 1_000) return trimZero(n / 1_000) + "k";
  return String(n);
}

function trimZero(v: number): string {
  return v.toFixed(1).replace(/\.0$/, "");
}
