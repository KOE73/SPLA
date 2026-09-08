(function(){"use strict";try{if(typeof document<"u"){var a=document.createElement("style");a.appendChild(document.createTextNode(".cred-slot[data-v-17c11f13]{min-width:0;flex:1}.w-260[data-v-17c11f13]{width:260px}.list-panel{display:flex;flex-direction:column;gap:var(--gap, 10px);--card-bd: 1px solid var(--border, #444);--card-bg: var(--panel, transparent);--card-r: var(--radius, 6px)}.list-empty{color:var(--muted, #888);font-size:var(--fs-sm, 12px);padding:4px 0;font-style:italic}.list-card{border:var(--card-bd);border-radius:var(--card-r);background:var(--card-bg)}.list-card-head{display:flex;align-items:center;gap:var(--gap, 10px);padding:var(--pad, 12px);cursor:pointer}.list-card-head{font-weight:600}.list-card-title{display:flex;align-items:center;gap:6px;min-width:0}.list-card-summary{font-weight:400;color:var(--muted, #888);font-size:var(--fs-sm, 12px);white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.list-card-actions{display:flex;align-items:center;gap:4px;font-weight:600;margin-left:auto}.list-card-head>.gbtn:first-child{margin-left:-3px}.list-card-body{padding:0 var(--pad, 12px) var(--pad, 12px);display:flex;flex-direction:column;gap:var(--gap, 10px)}.list-panel.flat{gap:0;--card-bd: 0;--card-bg: transparent;--card-r: 0}.list-panel.flat>.list-card+.list-card{border-top:1px solid var(--border, #444)}.list-panel.flat>.list-add{margin-top:var(--gap, 10px)}.list-add{align-self:flex-start;background:transparent;color:var(--muted, #888);border:1px solid var(--border, #444);border-radius:var(--radius-sm, 5px);padding:6px 12px;font-weight:600;font-size:inherit;font-family:inherit;cursor:pointer}.list-add:hover:not(:disabled){opacity:.9}.list-add:disabled{opacity:.5;cursor:default}.list-add-glyph{margin-right:5px;opacity:.8}.gbtn{display:inline-flex;align-items:center;justify-content:center;gap:3px;background:transparent;border:1px solid transparent;color:var(--muted, #888);border-radius:var(--radius-sm, 5px);padding:2px 5px;min-width:1.7em;height:1.7em;font:inherit;line-height:1;cursor:pointer}.gbtn:hover:not(:disabled){color:var(--text, inherit);border-color:var(--border, #444);background:var(--elevated, transparent)}.gbtn:disabled{opacity:.45;cursor:default}.gbtn-danger:hover:not(:disabled){color:var(--danger, #e05555);border-color:var(--danger, #e05555);background:transparent}.gbtn.on{color:var(--accent, inherit)}.sql-set[data-v-9c1f470c]{display:flex;flex-direction:column;gap:var(--gap, 10px);font-size:var(--fs-sm, 12px);color:var(--text, inherit)}.muted[data-v-9c1f470c]{color:var(--muted, #888)}.row[data-v-9c1f470c]{display:flex;gap:8px;align-items:center;flex-wrap:wrap}.grow[data-v-9c1f470c]{flex:1}.w-70[data-v-9c1f470c]{width:70px}.w-90[data-v-9c1f470c]{width:90px}.w-130[data-v-9c1f470c]{width:130px}.w-140[data-v-9c1f470c]{width:140px}.w-160[data-v-9c1f470c]{width:160px}.w-220[data-v-9c1f470c]{width:220px}.w-400[data-v-9c1f470c]{width:400px}.chk[data-v-9c1f470c]{cursor:pointer}.chk input[data-v-9c1f470c]{height:auto}label[data-v-9c1f470c]{display:flex;gap:6px;align-items:center}input[data-v-9c1f470c],select[data-v-9c1f470c]{height:24px;padding:2px 6px;color:var(--text, inherit);background:var(--bg, transparent);border:1px solid var(--border, #444);border-radius:var(--radius-sm, 5px);font-family:inherit;font-size:inherit}button[data-v-9c1f470c]:not(.gbtn){padding:2px 10px;color:var(--text, inherit);background:var(--panel, transparent);border:1px solid var(--border, #444);border-radius:var(--radius-sm, 5px);cursor:pointer;font-size:inherit}button[data-v-9c1f470c]:not(.gbtn):hover:not(:disabled){border-color:var(--muted, #888)}button[data-v-9c1f470c]:not(.gbtn):disabled{opacity:.5;cursor:default}")),document.head.appendChild(a)}}catch(r){console.error("vite-plugin-css-injected-by-js",r)}})();
/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Qn(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const n of e.split(",")) t[n] = 1;
  return (n) => n in t;
}
const W = {}, pt = [], Ve = () => {
}, ni = () => !1, mn = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), _n = (e) => e.startsWith("onUpdate:"), re = Object.assign, Zn = (e, t) => {
  const n = e.indexOf(t);
  n > -1 && e.splice(n, 1);
}, pr = Object.prototype.hasOwnProperty, H = (e, t) => pr.call(e, t), M = Array.isArray, gt = (e) => Wt(e) === "[object Map]", St = (e) => Wt(e) === "[object Set]", xs = (e) => Wt(e) === "[object Date]", F = (e) => typeof e == "function", ee = (e) => typeof e == "string", Se = (e) => typeof e == "symbol", K = (e) => e !== null && typeof e == "object", si = (e) => (K(e) || F(e)) && F(e.then) && F(e.catch), ii = Object.prototype.toString, Wt = (e) => ii.call(e), gr = (e) => Wt(e).slice(8, -1), ri = (e) => Wt(e) === "[object Object]", es = (e) => ee(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, Rt = /* @__PURE__ */ Qn(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), bn = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((n) => t[n] || (t[n] = e(n)));
}, mr = /-\w/g, ve = bn(
  (e) => e.replace(mr, (t) => t.slice(1).toUpperCase())
), _r = /\B([A-Z])/g, at = bn(
  (e) => e.replace(_r, "-$1").toLowerCase()
), oi = bn((e) => e.charAt(0).toUpperCase() + e.slice(1)), Pn = bn(
  (e) => e ? `on${oi(e)}` : ""
), Fe = (e, t) => !Object.is(e, t), sn = (e, ...t) => {
  for (let n = 0; n < e.length; n++)
    e[n](...t);
}, li = (e, t, n, s = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: s,
    value: n
  });
}, yn = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let Ss;
const vn = () => Ss || (Ss = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function ts(e) {
  if (M(e)) {
    const t = {};
    for (let n = 0; n < e.length; n++) {
      const s = e[n], i = ee(s) ? xr(s) : ts(s);
      if (i)
        for (const r in i)
          t[r] = i[r];
    }
    return t;
  } else if (ee(e) || K(e))
    return e;
}
const br = /;(?![^(]*\))/g, yr = /:([^]+)/, vr = /\/\*[^]*?\*\//g;
function xr(e) {
  const t = {};
  return e.replace(vr, "").split(br).forEach((n) => {
    if (n) {
      const s = n.split(yr);
      s.length > 1 && (t[s[0].trim()] = s[1].trim());
    }
  }), t;
}
function qt(e) {
  let t = "";
  if (ee(e))
    t = e;
  else if (M(e))
    for (let n = 0; n < e.length; n++) {
      const s = qt(e[n]);
      s && (t += s + " ");
    }
  else if (K(e))
    for (const n in e)
      e[n] && (t += n + " ");
  return t.trim();
}
const Sr = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", wr = /* @__PURE__ */ Qn(Sr);
function ci(e) {
  return !!e || e === "";
}
function Cr(e, t) {
  if (e.length !== t.length) return !1;
  let n = !0;
  for (let s = 0; n && s < e.length; s++)
    n = wt(e[s], t[s]);
  return n;
}
function wt(e, t) {
  if (e === t) return !0;
  let n = xs(e), s = xs(t);
  if (n || s)
    return n && s ? e.getTime() === t.getTime() : !1;
  if (n = Se(e), s = Se(t), n || s)
    return e === t;
  if (n = M(e), s = M(t), n || s)
    return n && s ? Cr(e, t) : !1;
  if (n = K(e), s = K(t), n || s) {
    if (!n || !s)
      return !1;
    const i = Object.keys(e).length, r = Object.keys(t).length;
    if (i !== r)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), c = t.hasOwnProperty(o);
      if (l && !c || !l && c || !wt(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function ns(e, t) {
  return e.findIndex((n) => wt(n, t));
}
const fi = (e) => !!(e && e.__v_isRef === !0), k = (e) => ee(e) ? e : e == null ? "" : M(e) || K(e) && (e.toString === ii || !F(e.toString)) ? fi(e) ? k(e.value) : JSON.stringify(e, ui, 2) : String(e), ui = (e, t) => fi(t) ? ui(e, t.value) : gt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (n, [s, i], r) => (n[Mn(s, r) + " =>"] = i, n),
    {}
  )
} : St(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((n) => Mn(n))
} : Se(t) ? Mn(t) : K(t) && !M(t) && !ri(t) ? String(t) : t, Mn = (e, t = "") => {
  var n;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Se(e) ? `Symbol(${(n = e.description) != null ? n : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let ie;
class Tr {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && ie && (ie.active ? (this.parent = ie, this.index = (ie.scopes || (ie.scopes = [])).push(
      this
    ) - 1) : (this._active = !1, this._warnOnRun = !1));
  }
  get active() {
    return this._active;
  }
  pause() {
    if (this._active) {
      this._isPaused = !0;
      let t, n;
      if (this.scopes)
        for (t = 0, n = this.scopes.length; t < n; t++)
          this.scopes[t].pause();
      for (t = 0, n = this.effects.length; t < n; t++)
        this.effects[t].pause();
    }
  }
  /**
   * Resumes the effect scope, including all child scopes and effects.
   */
  resume() {
    if (this._active && this._isPaused) {
      this._isPaused = !1;
      let t, n;
      if (this.scopes)
        for (t = 0, n = this.scopes.length; t < n; t++)
          this.scopes[t].resume();
      for (t = 0, n = this.effects.length; t < n; t++)
        this.effects[t].resume();
    }
  }
  run(t) {
    if (this._active) {
      const n = ie;
      try {
        return ie = this, t();
      } finally {
        ie = n;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = ie, ie = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (ie === this)
        ie = this.prevScope;
      else {
        let t = ie;
        for (; t; ) {
          if (t.prevScope === this) {
            t.prevScope = this.prevScope;
            break;
          }
          t = t.prevScope;
        }
      }
      this.prevScope = void 0;
    }
  }
  stop(t) {
    if (this._active) {
      this._active = !1;
      let n, s;
      for (n = 0, s = this.effects.length; n < s; n++)
        this.effects[n].stop();
      for (this.effects.length = 0, n = 0, s = this.cleanups.length; n < s; n++)
        this.cleanups[n]();
      if (this.cleanups.length = 0, this.scopes) {
        for (n = 0, s = this.scopes.length; n < s; n++)
          this.scopes[n].stop(!0);
        this.scopes.length = 0;
      }
      if (!this.detached && this.parent && !t) {
        const i = this.parent.scopes.pop();
        i && i !== this && (this.parent.scopes[this.index] = i, i.index = this.index);
      }
      this.parent = void 0;
    }
  }
}
function Or() {
  return ie;
}
let G;
const In = /* @__PURE__ */ new WeakSet();
class ai {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, ie && (ie.active ? ie.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, In.has(this) && (In.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || hi(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, ws(this), pi(this);
    const t = G, n = xe;
    G = this, xe = !0;
    try {
      return this.fn();
    } finally {
      gi(this), G = t, xe = n, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        rs(t);
      this.deps = this.depsTail = void 0, ws(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? In.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Hn(this) && this.run();
  }
  get dirty() {
    return Hn(this);
  }
}
let di = 0, $t, Ft;
function hi(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Ft, Ft = e;
    return;
  }
  e.next = $t, $t = e;
}
function ss() {
  di++;
}
function is() {
  if (--di > 0)
    return;
  if (Ft) {
    let t = Ft;
    for (Ft = void 0; t; ) {
      const n = t.next;
      t.next = void 0, t.flags &= -9, t = n;
    }
  }
  let e;
  for (; $t; ) {
    let t = $t;
    for ($t = void 0; t; ) {
      const n = t.next;
      if (t.next = void 0, t.flags &= -9, t.flags & 1)
        try {
          t.trigger();
        } catch (s) {
          e || (e = s);
        }
      t = n;
    }
  }
  if (e) throw e;
}
function pi(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function gi(e) {
  let t, n = e.depsTail, s = n;
  for (; s; ) {
    const i = s.prevDep;
    s.version === -1 ? (s === n && (n = i), rs(s), Er(s)) : t = s, s.dep.activeLink = s.prevActiveLink, s.prevActiveLink = void 0, s = i;
  }
  e.deps = t, e.depsTail = n;
}
function Hn(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (mi(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function mi(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === jt) || (e.globalVersion = jt, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Hn(e))))
    return;
  e.flags |= 2;
  const t = e.dep, n = G, s = xe;
  G = e, xe = !0;
  try {
    pi(e);
    const i = e.fn(e._value);
    (t.version === 0 || Fe(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    G = n, xe = s, gi(e), e.flags &= -3;
  }
}
function rs(e, t = !1) {
  const { dep: n, prevSub: s, nextSub: i } = e;
  if (s && (s.nextSub = i, e.prevSub = void 0), i && (i.prevSub = s, e.nextSub = void 0), n.subs === e && (n.subs = s, !s && n.computed)) {
    n.computed.flags &= -5;
    for (let r = n.computed.deps; r; r = r.nextDep)
      rs(r, !0);
  }
  !t && !--n.sc && n.map && n.map.delete(n.key);
}
function Er(e) {
  const { prevDep: t, nextDep: n } = e;
  t && (t.nextDep = n, e.prevDep = void 0), n && (n.prevDep = t, e.nextDep = void 0);
}
let xe = !0;
const _i = [];
function De() {
  _i.push(xe), xe = !1;
}
function Ne() {
  const e = _i.pop();
  xe = e === void 0 ? !0 : e;
}
function ws(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const n = G;
    G = void 0;
    try {
      t();
    } finally {
      G = n;
    }
  }
}
let jt = 0;
class Ar {
  constructor(t, n) {
    this.sub = t, this.dep = n, this.version = n.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class os {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!G || !xe || G === this.computed)
      return;
    let n = this.activeLink;
    if (n === void 0 || n.sub !== G)
      n = this.activeLink = new Ar(G, this), G.deps ? (n.prevDep = G.depsTail, G.depsTail.nextDep = n, G.depsTail = n) : G.deps = G.depsTail = n, bi(n);
    else if (n.version === -1 && (n.version = this.version, n.nextDep)) {
      const s = n.nextDep;
      s.prevDep = n.prevDep, n.prevDep && (n.prevDep.nextDep = s), n.prevDep = G.depsTail, n.nextDep = void 0, G.depsTail.nextDep = n, G.depsTail = n, G.deps === n && (G.deps = s);
    }
    return n;
  }
  trigger(t) {
    this.version++, jt++, this.notify(t);
  }
  notify(t) {
    ss();
    try {
      for (let n = this.subs; n; n = n.prevSub)
        n.sub.notify() && n.sub.dep.notify();
    } finally {
      is();
    }
  }
}
function bi(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let s = t.deps; s; s = s.nextDep)
        bi(s);
    }
    const n = e.dep.subs;
    n !== e && (e.prevSub = n, n && (n.nextSub = e)), e.dep.subs = e;
  }
}
const Ln = /* @__PURE__ */ new WeakMap(), ct = /* @__PURE__ */ Symbol(
  ""
), Kn = /* @__PURE__ */ Symbol(
  ""
), Ut = /* @__PURE__ */ Symbol(
  ""
);
function oe(e, t, n) {
  if (xe && G) {
    let s = Ln.get(e);
    s || Ln.set(e, s = /* @__PURE__ */ new Map());
    let i = s.get(n);
    i || (s.set(n, i = new os()), i.map = s, i.key = n), i.track();
  }
}
function Le(e, t, n, s, i, r) {
  const o = Ln.get(e);
  if (!o) {
    jt++;
    return;
  }
  const l = (c) => {
    c && c.trigger();
  };
  if (ss(), t === "clear")
    o.forEach(l);
  else {
    const c = M(e), d = c && es(n);
    if (c && n === "length") {
      const a = Number(s);
      o.forEach((p, T) => {
        (T === "length" || T === Ut || !Se(T) && T >= a) && l(p);
      });
    } else
      switch ((n !== void 0 || o.has(void 0)) && l(o.get(n)), d && l(o.get(Ut)), t) {
        case "add":
          c ? d && l(o.get("length")) : (l(o.get(ct)), gt(e) && l(o.get(Kn)));
          break;
        case "delete":
          c || (l(o.get(ct)), gt(e) && l(o.get(Kn)));
          break;
        case "set":
          gt(e) && l(o.get(ct));
          break;
      }
  }
  is();
}
function dt(e) {
  const t = /* @__PURE__ */ U(e);
  return t === e ? t : (oe(t, "iterate", Ut), /* @__PURE__ */ ye(e) ? t : t.map(we));
}
function xn(e) {
  return oe(e = /* @__PURE__ */ U(e), "iterate", Ut), e;
}
function Re(e, t) {
  return /* @__PURE__ */ We(e) ? yt(/* @__PURE__ */ ft(e) ? we(t) : t) : we(t);
}
const Pr = {
  __proto__: null,
  [Symbol.iterator]() {
    return Rn(this, Symbol.iterator, (e) => Re(this, e));
  },
  concat(...e) {
    return dt(this).concat(
      ...e.map((t) => M(t) ? dt(t) : t)
    );
  },
  entries() {
    return Rn(this, "entries", (e) => (e[1] = Re(this, e[1]), e));
  },
  every(e, t) {
    return je(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return je(
      this,
      "filter",
      e,
      t,
      (n) => n.map((s) => Re(this, s)),
      arguments
    );
  },
  find(e, t) {
    return je(
      this,
      "find",
      e,
      t,
      (n) => Re(this, n),
      arguments
    );
  },
  findIndex(e, t) {
    return je(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return je(
      this,
      "findLast",
      e,
      t,
      (n) => Re(this, n),
      arguments
    );
  },
  findLastIndex(e, t) {
    return je(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return je(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return $n(this, "includes", e);
  },
  indexOf(...e) {
    return $n(this, "indexOf", e);
  },
  join(e) {
    return dt(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return $n(this, "lastIndexOf", e);
  },
  map(e, t) {
    return je(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return At(this, "pop");
  },
  push(...e) {
    return At(this, "push", e);
  },
  reduce(e, ...t) {
    return Cs(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return Cs(this, "reduceRight", e, t);
  },
  shift() {
    return At(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return je(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return At(this, "splice", e);
  },
  toReversed() {
    return dt(this).toReversed();
  },
  toSorted(e) {
    return dt(this).toSorted(e);
  },
  toSpliced(...e) {
    return dt(this).toSpliced(...e);
  },
  unshift(...e) {
    return At(this, "unshift", e);
  },
  values() {
    return Rn(this, "values", (e) => Re(this, e));
  }
};
function Rn(e, t, n) {
  const s = xn(e), i = s[t]();
  return s !== e && !/* @__PURE__ */ ye(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = n(r.value)), r;
  }), i;
}
const Mr = Array.prototype;
function je(e, t, n, s, i, r) {
  const o = xn(e), l = o !== e && !/* @__PURE__ */ ye(e), c = o[t];
  if (c !== Mr[t]) {
    const p = c.apply(e, r);
    return l ? we(p) : p;
  }
  let d = n;
  o !== e && (l ? d = function(p, T) {
    return n.call(this, Re(e, p), T, e);
  } : n.length > 2 && (d = function(p, T) {
    return n.call(this, p, T, e);
  }));
  const a = c.call(o, d, s);
  return l && i ? i(a) : a;
}
function Cs(e, t, n, s) {
  const i = xn(e), r = i !== e && !/* @__PURE__ */ ye(e);
  let o = n, l = !1;
  i !== e && (r ? (l = s.length === 0, o = function(d, a, p) {
    return l && (l = !1, d = Re(e, d)), n.call(this, d, Re(e, a), p, e);
  }) : n.length > 3 && (o = function(d, a, p) {
    return n.call(this, d, a, p, e);
  }));
  const c = i[t](o, ...s);
  return l ? Re(e, c) : c;
}
function $n(e, t, n) {
  const s = /* @__PURE__ */ U(e);
  oe(s, "iterate", Ut);
  const i = s[t](...n);
  return (i === -1 || i === !1) && /* @__PURE__ */ fs(n[0]) ? (n[0] = /* @__PURE__ */ U(n[0]), s[t](...n)) : i;
}
function At(e, t, n = []) {
  De(), ss();
  const s = (/* @__PURE__ */ U(e))[t].apply(e, n);
  return is(), Ne(), s;
}
const Ir = /* @__PURE__ */ Qn("__proto__,__v_isRef,__isVue"), yi = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Se)
);
function Rr(e) {
  Se(e) || (e = String(e));
  const t = /* @__PURE__ */ U(this);
  return oe(t, "has", e), t.hasOwnProperty(e);
}
class vi {
  constructor(t = !1, n = !1) {
    this._isReadonly = t, this._isShallow = n;
  }
  get(t, n, s) {
    if (n === "__v_skip") return t.__v_skip;
    const i = this._isReadonly, r = this._isShallow;
    if (n === "__v_isReactive")
      return !i;
    if (n === "__v_isReadonly")
      return i;
    if (n === "__v_isShallow")
      return r;
    if (n === "__v_raw")
      return s === (i ? r ? Kr : Ci : r ? wi : Si).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(s) ? t : void 0;
    const o = M(t);
    if (!i) {
      let c;
      if (o && (c = Pr[n]))
        return c;
      if (n === "hasOwnProperty")
        return Rr;
    }
    const l = Reflect.get(
      t,
      n,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ fe(t) ? t : s
    );
    if ((Se(n) ? yi.has(n) : Ir(n)) || (i || oe(t, "get", n), r))
      return l;
    if (/* @__PURE__ */ fe(l)) {
      const c = o && es(n) ? l : l.value;
      return i && K(c) ? /* @__PURE__ */ kn(c) : c;
    }
    return K(l) ? i ? /* @__PURE__ */ kn(l) : /* @__PURE__ */ Ht(l) : l;
  }
}
class xi extends vi {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, n, s, i) {
    let r = t[n];
    const o = M(t) && es(n);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ We(r);
      if (!/* @__PURE__ */ ye(s) && !/* @__PURE__ */ We(s) && (r = /* @__PURE__ */ U(r), s = /* @__PURE__ */ U(s)), !o && /* @__PURE__ */ fe(r) && !/* @__PURE__ */ fe(s))
        return d || (r.value = s), !0;
    }
    const l = o ? Number(n) < t.length : H(t, n), c = Reflect.set(
      t,
      n,
      s,
      /* @__PURE__ */ fe(t) ? t : i
    );
    return t === /* @__PURE__ */ U(i) && c && (l ? Fe(s, r) && Le(t, "set", n, s) : Le(t, "add", n, s)), c;
  }
  deleteProperty(t, n) {
    const s = H(t, n);
    t[n];
    const i = Reflect.deleteProperty(t, n);
    return i && s && Le(t, "delete", n, void 0), i;
  }
  has(t, n) {
    const s = Reflect.has(t, n);
    return (!Se(n) || !yi.has(n)) && oe(t, "has", n), s;
  }
  ownKeys(t) {
    return oe(
      t,
      "iterate",
      M(t) ? "length" : ct
    ), Reflect.ownKeys(t);
  }
}
class $r extends vi {
  constructor(t = !1) {
    super(!0, t);
  }
  set(t, n) {
    return !0;
  }
  deleteProperty(t, n) {
    return !0;
  }
}
const Fr = /* @__PURE__ */ new xi(), Vr = /* @__PURE__ */ new $r(), Dr = /* @__PURE__ */ new xi(!0);
const Bn = (e) => e, en = (e) => Reflect.getPrototypeOf(e);
function Nr(e, t, n) {
  return function(...s) {
    const i = this.__v_raw, r = /* @__PURE__ */ U(i), o = gt(r), l = e === "entries" || e === Symbol.iterator && o, c = e === "keys" && o, d = i[e](...s), a = n ? Bn : t ? yt : we;
    return !t && oe(
      r,
      "iterate",
      c ? Kn : ct
    ), re(
      // inheriting all iterator properties
      Object.create(d),
      {
        // iterator protocol
        next() {
          const { value: p, done: T } = d.next();
          return T ? { value: p, done: T } : {
            value: l ? [a(p[0]), a(p[1])] : a(p),
            done: T
          };
        }
      }
    );
  };
}
function tn(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function jr(e, t) {
  const n = {
    get(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ U(r), l = /* @__PURE__ */ U(i);
      e || (Fe(i, l) && oe(o, "get", i), oe(o, "get", l));
      const { has: c } = en(o), d = t ? Bn : e ? yt : we;
      if (c.call(o, i))
        return d(r.get(i));
      if (c.call(o, l))
        return d(r.get(l));
      r !== o && r.get(i);
    },
    get size() {
      const i = this.__v_raw;
      return !e && oe(/* @__PURE__ */ U(i), "iterate", ct), i.size;
    },
    has(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ U(r), l = /* @__PURE__ */ U(i);
      return e || (Fe(i, l) && oe(o, "has", i), oe(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, c = /* @__PURE__ */ U(l), d = t ? Bn : e ? yt : we;
      return !e && oe(c, "iterate", ct), l.forEach((a, p) => i.call(r, d(a), d(p), o));
    }
  };
  return re(
    n,
    e ? {
      add: tn("add"),
      set: tn("set"),
      delete: tn("delete"),
      clear: tn("clear")
    } : {
      add(i) {
        const r = /* @__PURE__ */ U(this), o = en(r), l = /* @__PURE__ */ U(i), c = !t && !/* @__PURE__ */ ye(i) && !/* @__PURE__ */ We(i) ? l : i;
        return o.has.call(r, c) || Fe(i, c) && o.has.call(r, i) || Fe(l, c) && o.has.call(r, l) || (r.add(c), Le(r, "add", c, c)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ ye(r) && !/* @__PURE__ */ We(r) && (r = /* @__PURE__ */ U(r));
        const o = /* @__PURE__ */ U(this), { has: l, get: c } = en(o);
        let d = l.call(o, i);
        d || (i = /* @__PURE__ */ U(i), d = l.call(o, i));
        const a = c.call(o, i);
        return o.set(i, r), d ? Fe(r, a) && Le(o, "set", i, r) : Le(o, "add", i, r), this;
      },
      delete(i) {
        const r = /* @__PURE__ */ U(this), { has: o, get: l } = en(r);
        let c = o.call(r, i);
        c || (i = /* @__PURE__ */ U(i), c = o.call(r, i)), l && l.call(r, i);
        const d = r.delete(i);
        return c && Le(r, "delete", i, void 0), d;
      },
      clear() {
        const i = /* @__PURE__ */ U(this), r = i.size !== 0, o = i.clear();
        return r && Le(
          i,
          "clear",
          void 0,
          void 0
        ), o;
      }
    }
  ), [
    "keys",
    "values",
    "entries",
    Symbol.iterator
  ].forEach((i) => {
    n[i] = Nr(i, e, t);
  }), n;
}
function ls(e, t) {
  const n = jr(e, t);
  return (s, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? s : Reflect.get(
    H(n, i) && i in s ? n : s,
    i,
    r
  );
}
const Ur = {
  get: /* @__PURE__ */ ls(!1, !1)
}, Hr = {
  get: /* @__PURE__ */ ls(!1, !0)
}, Lr = {
  get: /* @__PURE__ */ ls(!0, !1)
};
const Si = /* @__PURE__ */ new WeakMap(), wi = /* @__PURE__ */ new WeakMap(), Ci = /* @__PURE__ */ new WeakMap(), Kr = /* @__PURE__ */ new WeakMap();
function Br(e) {
  switch (e) {
    case "Object":
    case "Array":
      return 1;
    case "Map":
    case "Set":
    case "WeakMap":
    case "WeakSet":
      return 2;
    default:
      return 0;
  }
}
// @__NO_SIDE_EFFECTS__
function Ht(e) {
  return /* @__PURE__ */ We(e) ? e : cs(
    e,
    !1,
    Fr,
    Ur,
    Si
  );
}
// @__NO_SIDE_EFFECTS__
function kr(e) {
  return cs(
    e,
    !1,
    Dr,
    Hr,
    wi
  );
}
// @__NO_SIDE_EFFECTS__
function kn(e) {
  return cs(
    e,
    !0,
    Vr,
    Lr,
    Ci
  );
}
function cs(e, t, n, s, i) {
  if (!K(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const r = i.get(e);
  if (r)
    return r;
  const o = Br(gr(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? s : n
  );
  return i.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function ft(e) {
  return /* @__PURE__ */ We(e) ? /* @__PURE__ */ ft(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function We(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function ye(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function fs(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function U(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ U(t) : e;
}
function Wr(e) {
  return !H(e, "__v_skip") && Object.isExtensible(e) && li(e, "__v_skip", !0), e;
}
const we = (e) => K(e) ? /* @__PURE__ */ Ht(e) : e, yt = (e) => K(e) ? /* @__PURE__ */ kn(e) : e;
// @__NO_SIDE_EFFECTS__
function fe(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function cn(e) {
  return qr(e, !1);
}
function qr(e, t) {
  return /* @__PURE__ */ fe(e) ? e : new Jr(e, t);
}
class Jr {
  constructor(t, n) {
    this.dep = new os(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = n ? t : /* @__PURE__ */ U(t), this._value = n ? t : we(t), this.__v_isShallow = n;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const n = this._rawValue, s = this.__v_isShallow || /* @__PURE__ */ ye(t) || /* @__PURE__ */ We(t);
    t = s ? t : /* @__PURE__ */ U(t), Fe(t, n) && (this._rawValue = t, this._value = s ? t : we(t), this.dep.trigger());
  }
}
function z(e) {
  return /* @__PURE__ */ fe(e) ? e.value : e;
}
const Gr = {
  get: (e, t, n) => t === "__v_raw" ? e : z(Reflect.get(e, t, n)),
  set: (e, t, n, s) => {
    const i = e[t];
    return /* @__PURE__ */ fe(i) && !/* @__PURE__ */ fe(n) ? (i.value = n, !0) : Reflect.set(e, t, n, s);
  }
};
function Ti(e) {
  return /* @__PURE__ */ ft(e) ? e : new Proxy(e, Gr);
}
class Yr {
  constructor(t, n, s) {
    this.fn = t, this.setter = n, this._value = void 0, this.dep = new os(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = jt - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !n, this.isSSR = s;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    G !== this)
      return hi(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return mi(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function zr(e, t, n = !1) {
  let s, i;
  return F(e) ? s = e : (s = e.get, i = e.set), new Yr(s, i, n);
}
const nn = {}, fn = /* @__PURE__ */ new WeakMap();
let ot;
function Xr(e, t = !1, n = ot) {
  if (n) {
    let s = fn.get(n);
    s || fn.set(n, s = []), s.push(e);
  }
}
function Qr(e, t, n = W) {
  const { immediate: s, deep: i, once: r, scheduler: o, augmentJob: l, call: c } = n, d = (y) => i ? y : /* @__PURE__ */ ye(y) || i === !1 || i === 0 ? Ke(y, 1) : Ke(y);
  let a, p, T, E, j = !1, R = !1;
  if (/* @__PURE__ */ fe(e) ? (p = () => e.value, j = /* @__PURE__ */ ye(e)) : /* @__PURE__ */ ft(e) ? (p = () => d(e), j = !0) : M(e) ? (R = !0, j = e.some((y) => /* @__PURE__ */ ft(y) || /* @__PURE__ */ ye(y)), p = () => e.map((y) => {
    if (/* @__PURE__ */ fe(y))
      return y.value;
    if (/* @__PURE__ */ ft(y))
      return d(y);
    if (F(y))
      return c ? c(y, 2) : y();
  })) : F(e) ? t ? p = c ? () => c(e, 2) : e : p = () => {
    if (T) {
      De();
      try {
        T();
      } finally {
        Ne();
      }
    }
    const y = ot;
    ot = a;
    try {
      return c ? c(e, 3, [E]) : e(E);
    } finally {
      ot = y;
    }
  } : p = Ve, t && i) {
    const y = p, O = i === !0 ? 1 / 0 : i;
    p = () => Ke(y(), O);
  }
  const Z = Or(), q = () => {
    a.stop(), Z && Z.active && Zn(Z.effects, a);
  };
  if (r && t) {
    const y = t;
    t = (...O) => {
      const be = y(...O);
      return q(), be;
    };
  }
  let V = R ? new Array(e.length).fill(nn) : nn;
  const w = (y) => {
    if (!(!(a.flags & 1) || !a.dirty && !y))
      if (t) {
        const O = a.run();
        if (y || i || j || (R ? O.some((be, N) => Fe(be, V[N])) : Fe(O, V))) {
          T && T();
          const be = ot;
          ot = a;
          try {
            const N = [
              O,
              // pass undefined as the old value when it's changed for the first time
              V === nn ? void 0 : R && V[0] === nn ? [] : V,
              E
            ];
            V = O, c ? c(t, 3, N) : (
              // @ts-expect-error
              t(...N)
            );
          } finally {
            ot = be;
          }
        }
      } else
        a.run();
  };
  return l && l(w), a = new ai(p), a.scheduler = o ? () => o(w, !1) : w, E = (y) => Xr(y, !1, a), T = a.onStop = () => {
    const y = fn.get(a);
    if (y) {
      if (c)
        c(y, 4);
      else
        for (const O of y) O();
      fn.delete(a);
    }
  }, t ? s ? w(!0) : V = a.run() : o ? o(w.bind(null, !0), !0) : a.run(), q.pause = a.pause.bind(a), q.resume = a.resume.bind(a), q.stop = q, q;
}
function Ke(e, t = 1 / 0, n) {
  if (t <= 0 || !K(e) || e.__v_skip || (n = n || /* @__PURE__ */ new Map(), (n.get(e) || 0) >= t))
    return e;
  if (n.set(e, t), t--, /* @__PURE__ */ fe(e))
    Ke(e.value, t, n);
  else if (M(e))
    for (let s = 0; s < e.length; s++)
      Ke(e[s], t, n);
  else if (St(e) || gt(e))
    e.forEach((s) => {
      Ke(s, t, n);
    });
  else if (ri(e)) {
    for (const s in e)
      Ke(e[s], t, n);
    for (const s of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, s) && Ke(e[s], t, n);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function Jt(e, t, n, s) {
  try {
    return s ? e(...s) : e();
  } catch (i) {
    Sn(i, t, n);
  }
}
function Ce(e, t, n, s) {
  if (F(e)) {
    const i = Jt(e, t, n, s);
    return i && si(i) && i.catch((r) => {
      Sn(r, t, n);
    }), i;
  }
  if (M(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(Ce(e[r], t, n, s));
    return i;
  }
}
function Sn(e, t, n, s = !0) {
  const i = t ? t.vnode : null, { errorHandler: r, throwUnhandledErrorInProduction: o } = t && t.appContext.config || W;
  if (t) {
    let l = t.parent;
    const c = t.proxy, d = `https://vuejs.org/error-reference/#runtime-${n}`;
    for (; l; ) {
      const a = l.ec;
      if (a) {
        for (let p = 0; p < a.length; p++)
          if (a[p](e, c, d) === !1)
            return;
      }
      l = l.parent;
    }
    if (r) {
      De(), Jt(r, null, 10, [
        e,
        c,
        d
      ]), Ne();
      return;
    }
  }
  Zr(e, n, i, s, o);
}
function Zr(e, t, n, s = !0, i = !1) {
  if (i)
    throw e;
  console.error(e);
}
const de = [];
let Ie = -1;
const mt = [];
let Ye = null, ht = 0;
const Oi = /* @__PURE__ */ Promise.resolve();
let un = null;
function Ei(e) {
  const t = un || Oi;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function eo(e) {
  let t = Ie + 1, n = de.length;
  for (; t < n; ) {
    const s = t + n >>> 1, i = de[s], r = Lt(i);
    r < e || r === e && i.flags & 2 ? t = s + 1 : n = s;
  }
  return t;
}
function us(e) {
  if (!(e.flags & 1)) {
    const t = Lt(e), n = de[de.length - 1];
    !n || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Lt(n) ? de.push(e) : de.splice(eo(t), 0, e), e.flags |= 1, Ai();
  }
}
function Ai() {
  un || (un = Oi.then(Mi));
}
function to(e) {
  M(e) ? mt.push(...e) : Ye && e.id === -1 ? Ye.splice(ht + 1, 0, e) : e.flags & 1 || (mt.push(e), e.flags |= 1), Ai();
}
function Ts(e, t, n = Ie + 1) {
  for (; n < de.length; n++) {
    const s = de[n];
    if (s && s.flags & 2) {
      if (e && s.id !== e.uid)
        continue;
      de.splice(n, 1), n--, s.flags & 4 && (s.flags &= -2), s(), s.flags & 4 || (s.flags &= -2);
    }
  }
}
function Pi(e) {
  if (mt.length) {
    const t = [...new Set(mt)].sort(
      (n, s) => Lt(n) - Lt(s)
    );
    if (mt.length = 0, Ye) {
      Ye.push(...t);
      return;
    }
    for (Ye = t, ht = 0; ht < Ye.length; ht++) {
      const n = Ye[ht];
      n.flags & 4 && (n.flags &= -2), n.flags & 8 || n(), n.flags &= -2;
    }
    Ye = null, ht = 0;
  }
}
const Lt = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function Mi(e) {
  try {
    for (Ie = 0; Ie < de.length; Ie++) {
      const t = de[Ie];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Jt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Ie < de.length; Ie++) {
      const t = de[Ie];
      t && (t.flags &= -2);
    }
    Ie = -1, de.length = 0, Pi(), un = null, (de.length || mt.length) && Mi();
  }
}
let ce = null, Ii = null;
function an(e) {
  const t = ce;
  return ce = e, Ii = e && e.type.__scopeId || null, t;
}
function lt(e, t = ce, n) {
  if (!t || e._n)
    return e;
  const s = (...i) => {
    s._d && Ns(-1);
    const r = an(t);
    let o;
    try {
      o = e(...i);
    } finally {
      an(r), s._d && Ns(1);
    }
    return o;
  };
  return s._n = !0, s._c = !0, s._d = !0, s;
}
function Pe(e, t) {
  if (ce === null)
    return e;
  const n = On(ce), s = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [r, o, l, c = W] = t[i];
    r && (F(r) && (r = {
      mounted: r,
      updated: r
    }), r.deep && Ke(o), s.push({
      dir: r,
      instance: n,
      value: o,
      oldValue: void 0,
      arg: l,
      modifiers: c
    }));
  }
  return e;
}
function st(e, t, n, s) {
  const i = e.dirs, r = t && t.dirs;
  for (let o = 0; o < i.length; o++) {
    const l = i[o];
    r && (l.oldValue = r[o].value);
    let c = l.dir[s];
    c && (De(), Ce(c, n, 8, [
      e.el,
      l,
      e,
      t
    ]), Ne());
  }
}
function no(e, t) {
  if (he) {
    let n = he.provides;
    const s = he.parent && he.parent.provides;
    s === n && (n = he.provides = Object.create(s)), n[e] = t;
  }
}
function rn(e, t, n = !1) {
  const s = Zo();
  if (s || bt) {
    let i = bt ? bt._context.provides : s ? s.parent == null || s.ce ? s.vnode.appContext && s.vnode.appContext.provides : s.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return n && F(t) ? t.call(s && s.proxy) : t;
  }
}
const so = /* @__PURE__ */ Symbol.for("v-scx"), io = () => rn(so);
function on(e, t, n) {
  return Ri(e, t, n);
}
function Ri(e, t, n = W) {
  const { immediate: s, deep: i, flush: r, once: o } = n, l = re({}, n), c = t && s || !t && r !== "post";
  let d;
  if (Bt) {
    if (r === "sync") {
      const E = io();
      d = E.__watcherHandles || (E.__watcherHandles = []);
    } else if (!c) {
      const E = () => {
      };
      return E.stop = Ve, E.resume = Ve, E.pause = Ve, E;
    }
  }
  const a = he;
  l.call = (E, j, R) => Ce(E, a, j, R);
  let p = !1;
  r === "post" ? l.scheduler = (E) => {
    pe(E, a && a.suspense);
  } : r !== "sync" && (p = !0, l.scheduler = (E, j) => {
    j ? E() : us(E);
  }), l.augmentJob = (E) => {
    t && (E.flags |= 4), p && (E.flags |= 2, a && (E.id = a.uid, E.i = a));
  };
  const T = Qr(e, t, l);
  return Bt && (d ? d.push(T) : c && T()), T;
}
function ro(e, t, n) {
  const s = this.proxy, i = ee(e) ? e.includes(".") ? $i(s, e) : () => s[e] : e.bind(s, s);
  let r;
  F(t) ? r = t : (r = t.handler, n = t);
  const o = Gt(this), l = Ri(i, r.bind(s), n);
  return o(), l;
}
function $i(e, t) {
  const n = t.split(".");
  return () => {
    let s = e;
    for (let i = 0; i < n.length && s; i++)
      s = s[n[i]];
    return s;
  };
}
const oo = /* @__PURE__ */ Symbol("_vte"), lo = (e) => e.__isTeleport, Fn = /* @__PURE__ */ Symbol("_leaveCb");
function as(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, as(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Ze(e, t) {
  return F(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    re({ name: e.name }, t, { setup: e })
  ) : e;
}
function Fi(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function Os(e, t) {
  let n;
  return !!((n = Object.getOwnPropertyDescriptor(e, t)) && !n.configurable);
}
const dn = /* @__PURE__ */ new WeakMap();
function Vt(e, t, n, s, i = !1) {
  if (M(e)) {
    e.forEach(
      (R, Z) => Vt(
        R,
        t && (M(t) ? t[Z] : t),
        n,
        s,
        i
      )
    );
    return;
  }
  if (_t(s) && !i) {
    s.shapeFlag & 512 && s.type.__asyncResolved && s.component.subTree.component && Vt(e, t, n, s.component.subTree);
    return;
  }
  const r = s.shapeFlag & 4 ? On(s.component) : s.el, o = i ? null : r, { i: l, r: c } = e, d = t && t.r, a = l.refs === W ? l.refs = {} : l.refs, p = l.setupState, T = /* @__PURE__ */ U(p), E = p === W ? ni : (R) => Os(a, R) ? !1 : H(T, R), j = (R, Z) => !(Z && Os(a, Z));
  if (d != null && d !== c) {
    if (Es(t), ee(d))
      a[d] = null, E(d) && (p[d] = null);
    else if (/* @__PURE__ */ fe(d)) {
      const R = t;
      j(d, R.k) && (d.value = null), R.k && (a[R.k] = null);
    }
  }
  if (F(c)) {
    De();
    try {
      Jt(c, l, 12, [o, a]);
    } finally {
      Ne();
    }
  } else {
    const R = ee(c), Z = /* @__PURE__ */ fe(c);
    if (R || Z) {
      const q = () => {
        if (e.f) {
          const V = R ? E(c) ? p[c] : a[c] : j() || !e.k ? c.value : a[e.k];
          if (i)
            M(V) && Zn(V, r);
          else if (M(V))
            V.includes(r) || V.push(r);
          else if (R)
            a[c] = [r], E(c) && (p[c] = a[c]);
          else {
            const w = [r];
            j(c, e.k) && (c.value = w), e.k && (a[e.k] = w);
          }
        } else R ? (a[c] = o, E(c) && (p[c] = o)) : Z && (j(c, e.k) && (c.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const V = () => {
          q(), dn.delete(e);
        };
        V.id = -1, dn.set(e, V), pe(V, n);
      } else
        Es(e), q();
    }
  }
}
function Es(e) {
  const t = dn.get(e);
  t && (t.flags |= 8, dn.delete(e));
}
vn().requestIdleCallback;
vn().cancelIdleCallback;
const _t = (e) => !!e.type.__asyncLoader, Vi = (e) => e.type.__isKeepAlive;
function co(e, t) {
  Di(e, "a", t);
}
function fo(e, t) {
  Di(e, "da", t);
}
function Di(e, t, n = he) {
  const s = e.__wdc || (e.__wdc = () => {
    let i = n;
    for (; i; ) {
      if (i.isDeactivated)
        return;
      i = i.parent;
    }
    return e();
  });
  if (wn(t, s, n), n) {
    let i = n.parent;
    for (; i && i.parent; )
      Vi(i.parent.vnode) && uo(s, t, n, i), i = i.parent;
  }
}
function uo(e, t, n, s) {
  const i = wn(
    t,
    e,
    s,
    !0
    /* prepend */
  );
  Ui(() => {
    Zn(s[t], i);
  }, n);
}
function wn(e, t, n = he, s = !1) {
  if (n) {
    const i = n[e] || (n[e] = []), r = t.__weh || (t.__weh = (...o) => {
      De();
      const l = Gt(n), c = Ce(t, n, e, o);
      return l(), Ne(), c;
    });
    return s ? i.unshift(r) : i.push(r), r;
  }
}
const Je = (e) => (t, n = he) => {
  (!Bt || e === "sp") && wn(e, (...s) => t(...s), n);
}, ao = Je("bm"), Ni = Je("m"), ho = Je(
  "bu"
), po = Je("u"), ji = Je(
  "bum"
), Ui = Je("um"), go = Je(
  "sp"
), mo = Je("rtg"), _o = Je("rtc");
function bo(e, t = he) {
  wn("ec", e, t);
}
const yo = /* @__PURE__ */ Symbol.for("v-ndc");
function As(e, t, n, s) {
  let i;
  const r = n, o = M(e);
  if (o || ee(e)) {
    const l = o && /* @__PURE__ */ ft(e);
    let c = !1, d = !1;
    l && (c = !/* @__PURE__ */ ye(e), d = /* @__PURE__ */ We(e), e = xn(e)), i = new Array(e.length);
    for (let a = 0, p = e.length; a < p; a++)
      i[a] = t(
        c ? d ? yt(we(e[a])) : we(e[a]) : e[a],
        a,
        void 0,
        r
      );
  } else if (typeof e == "number") {
    i = new Array(e);
    for (let l = 0; l < e; l++)
      i[l] = t(l + 1, l, void 0, r);
  } else if (K(e))
    if (e[Symbol.iterator])
      i = Array.from(
        e,
        (l, c) => t(l, c, void 0, r)
      );
    else {
      const l = Object.keys(e);
      i = new Array(l.length);
      for (let c = 0, d = l.length; c < d; c++) {
        const a = l[c];
        i[c] = t(e[a], a, c, r);
      }
    }
  else
    i = [];
  return i;
}
function Xe(e, t, n = {}, s, i) {
  if (ce.ce || ce.parent && _t(ce.parent) && ce.parent.ce) {
    const d = Object.keys(n).length > 0;
    return t !== "default" && (n.name = t), Y(), Qe(
      le,
      null,
      [_e("slot", n, s && s())],
      d ? -2 : 64
    );
  }
  let r = e[t];
  r && r._c && (r._d = !1), Y();
  const o = r && Hi(r(n)), l = n.key || // slot content array of a dynamic conditional slot may have a branch
  // key attached in the `createSlots` helper, respect that
  o && o.key, c = Qe(
    le,
    {
      key: (l && !Se(l) ? l : `_${t}`) + // #7256 force differentiate fallback content from actual content
      (!o && s ? "_fb" : "")
    },
    o || (s ? s() : []),
    o && e._ === 1 ? 64 : -2
  );
  return c.scopeId && (c.slotScopeIds = [c.scopeId + "-s"]), r && r._c && (r._d = !0), c;
}
function Hi(e) {
  return e.some((t) => ps(t) ? !(t.type === qe || t.type === le && !Hi(t.children)) : !0) ? e : null;
}
const Wn = (e) => e ? or(e) ? On(e) : Wn(e.parent) : null, Dt = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ re(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => e.props,
    $attrs: (e) => e.attrs,
    $slots: (e) => e.slots,
    $refs: (e) => e.refs,
    $parent: (e) => Wn(e.parent),
    $root: (e) => Wn(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Ki(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      us(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = Ei.bind(e.proxy)),
    $watch: (e) => ro.bind(e)
  })
), Vn = (e, t) => e !== W && !e.__isScriptSetup && H(e, t), vo = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: n, setupState: s, data: i, props: r, accessCache: o, type: l, appContext: c } = e;
    if (t[0] !== "$") {
      const T = o[t];
      if (T !== void 0)
        switch (T) {
          case 1:
            return s[t];
          case 2:
            return i[t];
          case 4:
            return n[t];
          case 3:
            return r[t];
        }
      else {
        if (Vn(s, t))
          return o[t] = 1, s[t];
        if (i !== W && H(i, t))
          return o[t] = 2, i[t];
        if (H(r, t))
          return o[t] = 3, r[t];
        if (n !== W && H(n, t))
          return o[t] = 4, n[t];
        qn && (o[t] = 0);
      }
    }
    const d = Dt[t];
    let a, p;
    if (d)
      return t === "$attrs" && oe(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (n !== W && H(n, t))
      return o[t] = 4, n[t];
    if (
      // global properties
      p = c.config.globalProperties, H(p, t)
    )
      return p[t];
  },
  set({ _: e }, t, n) {
    const { data: s, setupState: i, ctx: r } = e;
    return Vn(i, t) ? (i[t] = n, !0) : s !== W && H(s, t) ? (s[t] = n, !0) : H(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = n, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: n, ctx: s, appContext: i, props: r, type: o }
  }, l) {
    let c;
    return !!(n[l] || e !== W && l[0] !== "$" && H(e, l) || Vn(t, l) || H(r, l) || H(s, l) || H(Dt, l) || H(i.config.globalProperties, l) || (c = o.__cssModules) && c[l]);
  },
  defineProperty(e, t, n) {
    return n.get != null ? e._.accessCache[t] = 0 : H(n, "value") && this.set(e, t, n.value, null), Reflect.defineProperty(e, t, n);
  }
};
function Ps(e) {
  return M(e) ? e.reduce(
    (t, n) => (t[n] = null, t),
    {}
  ) : e;
}
let qn = !0;
function xo(e) {
  const t = Ki(e), n = e.proxy, s = e.ctx;
  qn = !1, t.beforeCreate && Ms(t.beforeCreate, e, "bc");
  const {
    // state
    data: i,
    computed: r,
    methods: o,
    watch: l,
    provide: c,
    inject: d,
    // lifecycle
    created: a,
    beforeMount: p,
    mounted: T,
    beforeUpdate: E,
    updated: j,
    activated: R,
    deactivated: Z,
    beforeDestroy: q,
    beforeUnmount: V,
    destroyed: w,
    unmounted: y,
    render: O,
    renderTracked: be,
    renderTriggered: N,
    errorCaptured: Ge,
    serverPrefetch: Yt,
    // public API
    expose: et,
    inheritAttrs: Ct,
    // assets
    components: zt,
    directives: Xt,
    filters: En
  } = t;
  if (d && So(d, s, null), o)
    for (const Q in o) {
      const J = o[Q];
      F(J) && (s[Q] = J.bind(n));
    }
  if (i) {
    const Q = i.call(n, n);
    K(Q) && (e.data = /* @__PURE__ */ Ht(Q));
  }
  if (qn = !0, r)
    for (const Q in r) {
      const J = r[Q], tt = F(J) ? J.bind(n, n) : F(J.get) ? J.get.bind(n, n) : Ve, Qt = !F(J) && F(J.set) ? J.set.bind(n) : Ve, nt = rl({
        get: tt,
        set: Qt
      });
      Object.defineProperty(s, Q, {
        enumerable: !0,
        configurable: !0,
        get: () => nt.value,
        set: (Te) => nt.value = Te
      });
    }
  if (l)
    for (const Q in l)
      Li(l[Q], s, n, Q);
  if (c) {
    const Q = F(c) ? c.call(n) : c;
    Reflect.ownKeys(Q).forEach((J) => {
      no(J, Q[J]);
    });
  }
  a && Ms(a, e, "c");
  function ue(Q, J) {
    M(J) ? J.forEach((tt) => Q(tt.bind(n))) : J && Q(J.bind(n));
  }
  if (ue(ao, p), ue(Ni, T), ue(ho, E), ue(po, j), ue(co, R), ue(fo, Z), ue(bo, Ge), ue(_o, be), ue(mo, N), ue(ji, V), ue(Ui, y), ue(go, Yt), M(et))
    if (et.length) {
      const Q = e.exposed || (e.exposed = {});
      et.forEach((J) => {
        Object.defineProperty(Q, J, {
          get: () => n[J],
          set: (tt) => n[J] = tt,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  O && e.render === Ve && (e.render = O), Ct != null && (e.inheritAttrs = Ct), zt && (e.components = zt), Xt && (e.directives = Xt), Yt && Fi(e);
}
function So(e, t, n = Ve) {
  M(e) && (e = Jn(e));
  for (const s in e) {
    const i = e[s];
    let r;
    K(i) ? "default" in i ? r = rn(
      i.from || s,
      i.default,
      !0
    ) : r = rn(i.from || s) : r = rn(i), /* @__PURE__ */ fe(r) ? Object.defineProperty(t, s, {
      enumerable: !0,
      configurable: !0,
      get: () => r.value,
      set: (o) => r.value = o
    }) : t[s] = r;
  }
}
function Ms(e, t, n) {
  Ce(
    M(e) ? e.map((s) => s.bind(t.proxy)) : e.bind(t.proxy),
    t,
    n
  );
}
function Li(e, t, n, s) {
  let i = s.includes(".") ? $i(n, s) : () => n[s];
  if (ee(e)) {
    const r = t[e];
    F(r) && on(i, r);
  } else if (F(e))
    on(i, e.bind(n));
  else if (K(e))
    if (M(e))
      e.forEach((r) => Li(r, t, n, s));
    else {
      const r = F(e.handler) ? e.handler.bind(n) : t[e.handler];
      F(r) && on(i, r, e);
    }
}
function Ki(e) {
  const t = e.type, { mixins: n, extends: s } = t, {
    mixins: i,
    optionsCache: r,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = r.get(t);
  let c;
  return l ? c = l : !i.length && !n && !s ? c = t : (c = {}, i.length && i.forEach(
    (d) => hn(c, d, o, !0)
  ), hn(c, t, o)), K(t) && r.set(t, c), c;
}
function hn(e, t, n, s = !1) {
  const { mixins: i, extends: r } = t;
  r && hn(e, r, n, !0), i && i.forEach(
    (o) => hn(e, o, n, !0)
  );
  for (const o in t)
    if (!(s && o === "expose")) {
      const l = wo[o] || n && n[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const wo = {
  data: Is,
  props: Rs,
  emits: Rs,
  // objects
  methods: Mt,
  computed: Mt,
  // lifecycle
  beforeCreate: ae,
  created: ae,
  beforeMount: ae,
  mounted: ae,
  beforeUpdate: ae,
  updated: ae,
  beforeDestroy: ae,
  beforeUnmount: ae,
  destroyed: ae,
  unmounted: ae,
  activated: ae,
  deactivated: ae,
  errorCaptured: ae,
  serverPrefetch: ae,
  // assets
  components: Mt,
  directives: Mt,
  // watch
  watch: To,
  // provide / inject
  provide: Is,
  inject: Co
};
function Is(e, t) {
  return t ? e ? function() {
    return re(
      F(e) ? e.call(this, this) : e,
      F(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function Co(e, t) {
  return Mt(Jn(e), Jn(t));
}
function Jn(e) {
  if (M(e)) {
    const t = {};
    for (let n = 0; n < e.length; n++)
      t[e[n]] = e[n];
    return t;
  }
  return e;
}
function ae(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function Mt(e, t) {
  return e ? re(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Rs(e, t) {
  return e ? M(e) && M(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : re(
    /* @__PURE__ */ Object.create(null),
    Ps(e),
    Ps(t ?? {})
  ) : t;
}
function To(e, t) {
  if (!e) return t;
  if (!t) return e;
  const n = re(/* @__PURE__ */ Object.create(null), e);
  for (const s in t)
    n[s] = ae(e[s], t[s]);
  return n;
}
function Bi() {
  return {
    app: null,
    config: {
      isNativeTag: ni,
      performance: !1,
      globalProperties: {},
      optionMergeStrategies: {},
      errorHandler: void 0,
      warnHandler: void 0,
      compilerOptions: {}
    },
    mixins: [],
    components: {},
    directives: {},
    provides: /* @__PURE__ */ Object.create(null),
    optionsCache: /* @__PURE__ */ new WeakMap(),
    propsCache: /* @__PURE__ */ new WeakMap(),
    emitsCache: /* @__PURE__ */ new WeakMap()
  };
}
let Oo = 0;
function Eo(e, t) {
  return function(s, i = null) {
    F(s) || (s = re({}, s)), i != null && !K(i) && (i = null);
    const r = Bi(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let c = !1;
    const d = r.app = {
      _uid: Oo++,
      _component: s,
      _props: i,
      _container: null,
      _context: r,
      _instance: null,
      version: ol,
      get config() {
        return r.config;
      },
      set config(a) {
      },
      use(a, ...p) {
        return o.has(a) || (a && F(a.install) ? (o.add(a), a.install(d, ...p)) : F(a) && (o.add(a), a(d, ...p))), d;
      },
      mixin(a) {
        return r.mixins.includes(a) || r.mixins.push(a), d;
      },
      component(a, p) {
        return p ? (r.components[a] = p, d) : r.components[a];
      },
      directive(a, p) {
        return p ? (r.directives[a] = p, d) : r.directives[a];
      },
      mount(a, p, T) {
        if (!c) {
          const E = d._ceVNode || _e(s, i);
          return E.appContext = r, T === !0 ? T = "svg" : T === !1 && (T = void 0), e(E, a, T), c = !0, d._container = a, a.__vue_app__ = d, On(E.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        c && (Ce(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, p) {
        return r.provides[a] = p, d;
      },
      runWithContext(a) {
        const p = bt;
        bt = d;
        try {
          return a();
        } finally {
          bt = p;
        }
      }
    };
    return d;
  };
}
let bt = null;
const Ao = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${ve(t)}Modifiers`] || e[`${at(t)}Modifiers`];
function Po(e, t, ...n) {
  if (e.isUnmounted) return;
  const s = e.vnode.props || W;
  let i = n;
  const r = t.startsWith("update:"), o = r && Ao(s, t.slice(7));
  o && (o.trim && (i = n.map((a) => ee(a) ? a.trim() : a)), o.number && (i = n.map(yn)));
  let l, c = s[l = Pn(t)] || // also try camelCase event handler (#2249)
  s[l = Pn(ve(t))];
  !c && r && (c = s[l = Pn(at(t))]), c && Ce(
    c,
    e,
    6,
    i
  );
  const d = s[l + "Once"];
  if (d) {
    if (!e.emitted)
      e.emitted = {};
    else if (e.emitted[l])
      return;
    e.emitted[l] = !0, Ce(
      d,
      e,
      6,
      i
    );
  }
}
const Mo = /* @__PURE__ */ new WeakMap();
function ki(e, t, n = !1) {
  const s = n ? Mo : t.emitsCache, i = s.get(e);
  if (i !== void 0)
    return i;
  const r = e.emits;
  let o = {}, l = !1;
  if (!F(e)) {
    const c = (d) => {
      const a = ki(d, t, !0);
      a && (l = !0, re(o, a));
    };
    !n && t.mixins.length && t.mixins.forEach(c), e.extends && c(e.extends), e.mixins && e.mixins.forEach(c);
  }
  return !r && !l ? (K(e) && s.set(e, null), null) : (M(r) ? r.forEach((c) => o[c] = null) : re(o, r), K(e) && s.set(e, o), o);
}
function Cn(e, t) {
  return !e || !mn(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), H(e, t[0].toLowerCase() + t.slice(1)) || H(e, at(t)) || H(e, t));
}
function $s(e) {
  const {
    type: t,
    vnode: n,
    proxy: s,
    withProxy: i,
    propsOptions: [r],
    slots: o,
    attrs: l,
    emit: c,
    render: d,
    renderCache: a,
    props: p,
    data: T,
    setupState: E,
    ctx: j,
    inheritAttrs: R
  } = e, Z = an(e);
  let q, V;
  try {
    if (n.shapeFlag & 4) {
      const y = i || s, O = y;
      q = $e(
        d.call(
          O,
          y,
          a,
          p,
          E,
          T,
          j
        )
      ), V = l;
    } else {
      const y = t;
      q = $e(
        y.length > 1 ? y(
          p,
          { attrs: l, slots: o, emit: c }
        ) : y(
          p,
          null
        )
      ), V = t.props ? l : Io(l);
    }
  } catch (y) {
    Nt.length = 0, Sn(y, e, 1), q = _e(qe);
  }
  let w = q;
  if (V && R !== !1) {
    const y = Object.keys(V), { shapeFlag: O } = w;
    y.length && O & 7 && (r && y.some(_n) && (V = Ro(
      V,
      r
    )), w = vt(w, V, !1, !0));
  }
  return n.dirs && (w = vt(w, null, !1, !0), w.dirs = w.dirs ? w.dirs.concat(n.dirs) : n.dirs), n.transition && as(w, n.transition), q = w, an(Z), q;
}
const Io = (e) => {
  let t;
  for (const n in e)
    (n === "class" || n === "style" || mn(n)) && ((t || (t = {}))[n] = e[n]);
  return t;
}, Ro = (e, t) => {
  const n = {};
  for (const s in e)
    (!_n(s) || !(s.slice(9) in t)) && (n[s] = e[s]);
  return n;
};
function $o(e, t, n) {
  const { props: s, children: i, component: r } = e, { props: o, children: l, patchFlag: c } = t, d = r.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (n && c >= 0) {
    if (c & 1024)
      return !0;
    if (c & 16)
      return s ? Fs(s, o, d) : !!o;
    if (c & 8) {
      const a = t.dynamicProps;
      for (let p = 0; p < a.length; p++) {
        const T = a[p];
        if (Wi(o, s, T) && !Cn(d, T))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : s === o ? !1 : s ? o ? Fs(s, o, d) : !0 : !!o;
  return !1;
}
function Fs(e, t, n) {
  const s = Object.keys(t);
  if (s.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < s.length; i++) {
    const r = s[i];
    if (Wi(t, e, r) && !Cn(n, r))
      return !0;
  }
  return !1;
}
function Wi(e, t, n) {
  const s = e[n], i = t[n];
  return n === "style" && K(s) && K(i) ? !wt(s, i) : s !== i;
}
function Fo({ vnode: e, parent: t, suspense: n }, s) {
  for (; t; ) {
    const i = t.subTree;
    if (i.suspense && i.suspense.activeBranch === e && (i.suspense.vnode.el = i.el = s, e = i), i === e)
      (e = t.vnode).el = s, t = t.parent;
    else
      break;
  }
  n && n.activeBranch === e && (n.vnode.el = s);
}
const qi = {}, Ji = () => Object.create(qi), Gi = (e) => Object.getPrototypeOf(e) === qi;
function Vo(e, t, n, s = !1) {
  const i = {}, r = Ji();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), Yi(e, t, i, r);
  for (const o in e.propsOptions[0])
    o in i || (i[o] = void 0);
  n ? e.props = s ? i : /* @__PURE__ */ kr(i) : e.type.props ? e.props = i : e.props = r, e.attrs = r;
}
function Do(e, t, n, s) {
  const {
    props: i,
    attrs: r,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ U(i), [c] = e.propsOptions;
  let d = !1;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    (s || o > 0) && !(o & 16)
  ) {
    if (o & 8) {
      const a = e.vnode.dynamicProps;
      for (let p = 0; p < a.length; p++) {
        let T = a[p];
        if (Cn(e.emitsOptions, T))
          continue;
        const E = t[T];
        if (c)
          if (H(r, T))
            E !== r[T] && (r[T] = E, d = !0);
          else {
            const j = ve(T);
            i[j] = Gn(
              c,
              l,
              j,
              E,
              e,
              !1
            );
          }
        else
          E !== r[T] && (r[T] = E, d = !0);
      }
    }
  } else {
    Yi(e, t, i, r) && (d = !0);
    let a;
    for (const p in l)
      (!t || // for camelCase
      !H(t, p) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = at(p)) === p || !H(t, a))) && (c ? n && // for camelCase
      (n[p] !== void 0 || // for kebab-case
      n[a] !== void 0) && (i[p] = Gn(
        c,
        l,
        p,
        void 0,
        e,
        !0
      )) : delete i[p]);
    if (r !== l)
      for (const p in r)
        (!t || !H(t, p)) && (delete r[p], d = !0);
  }
  d && Le(e.attrs, "set", "");
}
function Yi(e, t, n, s) {
  const [i, r] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let c in t) {
      if (Rt(c))
        continue;
      const d = t[c];
      let a;
      i && H(i, a = ve(c)) ? !r || !r.includes(a) ? n[a] = d : (l || (l = {}))[a] = d : Cn(e.emitsOptions, c) || (!(c in s) || d !== s[c]) && (s[c] = d, o = !0);
    }
  if (r) {
    const c = /* @__PURE__ */ U(n), d = l || W;
    for (let a = 0; a < r.length; a++) {
      const p = r[a];
      n[p] = Gn(
        i,
        c,
        p,
        d[p],
        e,
        !H(d, p)
      );
    }
  }
  return o;
}
function Gn(e, t, n, s, i, r) {
  const o = e[n];
  if (o != null) {
    const l = H(o, "default");
    if (l && s === void 0) {
      const c = o.default;
      if (o.type !== Function && !o.skipFactory && F(c)) {
        const { propsDefaults: d } = i;
        if (n in d)
          s = d[n];
        else {
          const a = Gt(i);
          s = d[n] = c.call(
            null,
            t
          ), a();
        }
      } else
        s = c;
      i.ce && i.ce._setProp(n, s);
    }
    o[
      0
      /* shouldCast */
    ] && (r && !l ? s = !1 : o[
      1
      /* shouldCastTrue */
    ] && (s === "" || s === at(n)) && (s = !0));
  }
  return s;
}
const No = /* @__PURE__ */ new WeakMap();
function zi(e, t, n = !1) {
  const s = n ? No : t.propsCache, i = s.get(e);
  if (i)
    return i;
  const r = e.props, o = {}, l = [];
  let c = !1;
  if (!F(e)) {
    const a = (p) => {
      c = !0;
      const [T, E] = zi(p, t, !0);
      re(o, T), E && l.push(...E);
    };
    !n && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!r && !c)
    return K(e) && s.set(e, pt), pt;
  if (M(r))
    for (let a = 0; a < r.length; a++) {
      const p = ve(r[a]);
      Vs(p) && (o[p] = W);
    }
  else if (r)
    for (const a in r) {
      const p = ve(a);
      if (Vs(p)) {
        const T = r[a], E = o[p] = M(T) || F(T) ? { type: T } : re({}, T), j = E.type;
        let R = !1, Z = !0;
        if (M(j))
          for (let q = 0; q < j.length; ++q) {
            const V = j[q], w = F(V) && V.name;
            if (w === "Boolean") {
              R = !0;
              break;
            } else w === "String" && (Z = !1);
          }
        else
          R = F(j) && j.name === "Boolean";
        E[
          0
          /* shouldCast */
        ] = R, E[
          1
          /* shouldCastTrue */
        ] = Z, (R || H(E, "default")) && l.push(p);
      }
    }
  const d = [o, l];
  return K(e) && s.set(e, d), d;
}
function Vs(e) {
  return e[0] !== "$" && !Rt(e);
}
const ds = (e) => e === "_" || e === "_ctx" || e === "$stable", hs = (e) => M(e) ? e.map($e) : [$e(e)], jo = (e, t, n) => {
  if (t._n)
    return t;
  const s = lt((...i) => hs(t(...i)), n);
  return s._c = !1, s;
}, Xi = (e, t, n) => {
  const s = e._ctx;
  for (const i in e) {
    if (ds(i)) continue;
    const r = e[i];
    if (F(r))
      t[i] = jo(i, r, s);
    else if (r != null) {
      const o = hs(r);
      t[i] = () => o;
    }
  }
}, Qi = (e, t) => {
  const n = hs(t);
  e.slots.default = () => n;
}, Zi = (e, t, n) => {
  for (const s in t)
    (n || !ds(s)) && (e[s] = t[s]);
}, Uo = (e, t, n) => {
  const s = e.slots = Ji();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Zi(s, t, n), n && li(s, "_", i, !0)) : Xi(t, s);
  } else t && Qi(e, t);
}, Ho = (e, t, n) => {
  const { vnode: s, slots: i } = e;
  let r = !0, o = W;
  if (s.shapeFlag & 32) {
    const l = t._;
    l ? n && l === 1 ? r = !1 : Zi(i, t, n) : (r = !t.$stable, Xi(t, i)), o = t;
  } else t && (Qi(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !ds(l) && o[l] == null && delete i[l];
}, pe = Wo;
function Lo(e) {
  return Ko(e);
}
function Ko(e, t) {
  const n = vn();
  n.__VUE__ = !0;
  const {
    insert: s,
    remove: i,
    patchProp: r,
    createElement: o,
    createText: l,
    createComment: c,
    setText: d,
    setElementText: a,
    parentNode: p,
    nextSibling: T,
    setScopeId: E = Ve,
    insertStaticContent: j
  } = e, R = (f, u, h, b = null, _ = null, g = null, S = void 0, x = null, v = !!u.dynamicChildren) => {
    if (f === u)
      return;
    f && !Pt(f, u) && (b = Zt(f), Te(f, _, g, !0), f = null), u.patchFlag === -2 && (v = !1, u.dynamicChildren = null);
    const { type: m, ref: P, shapeFlag: C } = u;
    switch (m) {
      case Tn:
        Z(f, u, h, b);
        break;
      case qe:
        q(f, u, h, b);
        break;
      case Nn:
        f == null && V(u, h, b, S);
        break;
      case le:
        zt(
          f,
          u,
          h,
          b,
          _,
          g,
          S,
          x,
          v
        );
        break;
      default:
        C & 1 ? O(
          f,
          u,
          h,
          b,
          _,
          g,
          S,
          x,
          v
        ) : C & 6 ? Xt(
          f,
          u,
          h,
          b,
          _,
          g,
          S,
          x,
          v
        ) : (C & 64 || C & 128) && m.process(
          f,
          u,
          h,
          b,
          _,
          g,
          S,
          x,
          v,
          Ot
        );
    }
    P != null && _ ? Vt(P, f && f.ref, g, u || f, !u) : P == null && f && f.ref != null && Vt(f.ref, null, g, f, !0);
  }, Z = (f, u, h, b) => {
    if (f == null)
      s(
        u.el = l(u.children),
        h,
        b
      );
    else {
      const _ = u.el = f.el;
      u.children !== f.children && d(_, u.children);
    }
  }, q = (f, u, h, b) => {
    f == null ? s(
      u.el = c(u.children || ""),
      h,
      b
    ) : u.el = f.el;
  }, V = (f, u, h, b) => {
    [f.el, f.anchor] = j(
      f.children,
      u,
      h,
      b,
      f.el,
      f.anchor
    );
  }, w = ({ el: f, anchor: u }, h, b) => {
    let _;
    for (; f && f !== u; )
      _ = T(f), s(f, h, b), f = _;
    s(u, h, b);
  }, y = ({ el: f, anchor: u }) => {
    let h;
    for (; f && f !== u; )
      h = T(f), i(f), f = h;
    i(u);
  }, O = (f, u, h, b, _, g, S, x, v) => {
    if (u.type === "svg" ? S = "svg" : u.type === "math" && (S = "mathml"), f == null)
      be(
        u,
        h,
        b,
        _,
        g,
        S,
        x,
        v
      );
    else {
      const m = f.el && f.el._isVueCE ? f.el : null;
      try {
        m && m._beginPatch(), Yt(
          f,
          u,
          _,
          g,
          S,
          x,
          v
        );
      } finally {
        m && m._endPatch();
      }
    }
  }, be = (f, u, h, b, _, g, S, x) => {
    let v, m;
    const { props: P, shapeFlag: C, transition: A, dirs: I } = f;
    if (v = f.el = o(
      f.type,
      g,
      P && P.is,
      P
    ), C & 8 ? a(v, f.children) : C & 16 && Ge(
      f.children,
      v,
      null,
      b,
      _,
      Dn(f, g),
      S,
      x
    ), I && st(f, null, b, "created"), N(v, f, f.scopeId, S, b), P) {
      for (const B in P)
        B !== "value" && !Rt(B) && r(v, B, null, P[B], g, b);
      "value" in P && r(v, "value", null, P.value, g), (m = P.onVnodeBeforeMount) && Me(m, b, f);
    }
    I && st(f, null, b, "beforeMount");
    const D = Bo(_, A);
    D && A.beforeEnter(v), s(v, u, h), ((m = P && P.onVnodeMounted) || D || I) && pe(() => {
      try {
        m && Me(m, b, f), D && A.enter(v), I && st(f, null, b, "mounted");
      } finally {
      }
    }, _);
  }, N = (f, u, h, b, _) => {
    if (h && E(f, h), b)
      for (let g = 0; g < b.length; g++)
        E(f, b[g]);
    if (_) {
      let g = _.subTree;
      if (u === g || sr(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const S = _.vnode;
        N(
          f,
          S,
          S.scopeId,
          S.slotScopeIds,
          _.parent
        );
      }
    }
  }, Ge = (f, u, h, b, _, g, S, x, v = 0) => {
    for (let m = v; m < f.length; m++) {
      const P = f[m] = x ? He(f[m]) : $e(f[m]);
      R(
        null,
        P,
        u,
        h,
        b,
        _,
        g,
        S,
        x
      );
    }
  }, Yt = (f, u, h, b, _, g, S) => {
    const x = u.el = f.el;
    let { patchFlag: v, dynamicChildren: m, dirs: P } = u;
    v |= f.patchFlag & 16;
    const C = f.props || W, A = u.props || W;
    let I;
    if (h && it(h, !1), (I = A.onVnodeBeforeUpdate) && Me(I, h, u, f), P && st(u, f, h, "beforeUpdate"), h && it(h, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    m && (!f.dynamicChildren || f.dynamicChildren.length !== m.length) && (v = 0, S = !1, m = null), (C.innerHTML && A.innerHTML == null || C.textContent && A.textContent == null) && a(x, ""), m ? et(
      f.dynamicChildren,
      m,
      x,
      h,
      b,
      Dn(u, _),
      g
    ) : S || J(
      f,
      u,
      x,
      null,
      h,
      b,
      Dn(u, _),
      g,
      !1
    ), v > 0) {
      if (v & 16)
        Ct(x, C, A, h, _);
      else if (v & 2 && C.class !== A.class && r(x, "class", null, A.class, _), v & 4 && r(x, "style", C.style, A.style, _), v & 8) {
        const D = u.dynamicProps;
        for (let B = 0; B < D.length; B++) {
          const L = D[B], te = C[L], se = A[L];
          (se !== te || L === "value") && r(x, L, te, se, _, h);
        }
      }
      v & 1 && f.children !== u.children && a(x, u.children);
    } else !S && m == null && Ct(x, C, A, h, _);
    ((I = A.onVnodeUpdated) || P) && pe(() => {
      I && Me(I, h, u, f), P && st(u, f, h, "updated");
    }, b);
  }, et = (f, u, h, b, _, g, S) => {
    for (let x = 0; x < u.length; x++) {
      const v = f[x], m = u[x], P = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        v.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (v.type === le || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !Pt(v, m) || // - In the case of a component, it could contain anything.
        v.shapeFlag & 198) ? p(v.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          h
        )
      );
      R(
        v,
        m,
        P,
        null,
        b,
        _,
        g,
        S,
        !0
      );
    }
  }, Ct = (f, u, h, b, _) => {
    if (u !== h) {
      if (u !== W)
        for (const g in u)
          !Rt(g) && !(g in h) && r(
            f,
            g,
            u[g],
            null,
            _,
            b
          );
      for (const g in h) {
        if (Rt(g)) continue;
        const S = h[g], x = u[g];
        S !== x && g !== "value" && r(f, g, x, S, _, b);
      }
      "value" in h && r(f, "value", u.value, h.value, _);
    }
  }, zt = (f, u, h, b, _, g, S, x, v) => {
    const m = u.el = f ? f.el : l(""), P = u.anchor = f ? f.anchor : l("");
    let { patchFlag: C, dynamicChildren: A, slotScopeIds: I } = u;
    I && (x = x ? x.concat(I) : I), f == null ? (s(m, h, b), s(P, h, b), Ge(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      h,
      P,
      _,
      g,
      S,
      x,
      v
    )) : C > 0 && C & 64 && A && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    f.dynamicChildren && f.dynamicChildren.length === A.length ? (et(
      f.dynamicChildren,
      A,
      h,
      _,
      g,
      S,
      x
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || _ && u === _.subTree) && er(
      f,
      u,
      !0
      /* shallow */
    )) : J(
      f,
      u,
      h,
      P,
      _,
      g,
      S,
      x,
      v
    );
  }, Xt = (f, u, h, b, _, g, S, x, v) => {
    u.slotScopeIds = x, f == null ? u.shapeFlag & 512 ? _.ctx.activate(
      u,
      h,
      b,
      S,
      v
    ) : En(
      u,
      h,
      b,
      _,
      g,
      S,
      v
    ) : gs(f, u, v);
  }, En = (f, u, h, b, _, g, S) => {
    const x = f.component = Qo(
      f,
      b,
      _
    );
    if (Vi(f) && (x.ctx.renderer = Ot), el(x, !1, S), x.asyncDep) {
      if (_ && _.registerDep(x, ue, S), !f.el) {
        const v = x.subTree = _e(qe);
        q(null, v, u, h), f.placeholder = v.el;
      }
    } else
      ue(
        x,
        f,
        u,
        h,
        _,
        g,
        S
      );
  }, gs = (f, u, h) => {
    const b = u.component = f.component;
    if ($o(f, u, h))
      if (b.asyncDep && !b.asyncResolved) {
        Q(b, u, h);
        return;
      } else
        b.next = u, b.update();
    else
      u.el = f.el, b.vnode = u;
  }, ue = (f, u, h, b, _, g, S) => {
    const x = () => {
      if (f.isMounted) {
        let { next: C, bu: A, u: I, parent: D, vnode: B } = f;
        {
          const Ee = tr(f);
          if (Ee) {
            C && (C.el = B.el, Q(f, C, S)), Ee.asyncDep.then(() => {
              pe(() => {
                f.isUnmounted || m();
              }, _);
            });
            return;
          }
        }
        let L = C, te;
        it(f, !1), C ? (C.el = B.el, Q(f, C, S)) : C = B, A && sn(A), (te = C.props && C.props.onVnodeBeforeUpdate) && Me(te, D, C, B), it(f, !0);
        const se = $s(f), Oe = f.subTree;
        f.subTree = se, R(
          Oe,
          se,
          // parent may have changed if it's in a teleport
          p(Oe.el),
          // anchor may have changed if it's in a fragment
          Zt(Oe),
          f,
          _,
          g
        ), C.el = se.el, L === null && Fo(f, se.el), I && pe(I, _), (te = C.props && C.props.onVnodeUpdated) && pe(
          () => Me(te, D, C, B),
          _
        );
      } else {
        let C;
        const { el: A, props: I } = u, { bm: D, m: B, parent: L, root: te, type: se } = f, Oe = _t(u);
        it(f, !1), D && sn(D), !Oe && (C = I && I.onVnodeBeforeMount) && Me(C, L, u), it(f, !0);
        {
          te.ce && te.ce._hasShadowRoot() && te.ce._injectChildStyle(
            se,
            f.parent ? f.parent.type : void 0
          );
          const Ee = f.subTree = $s(f);
          R(
            null,
            Ee,
            h,
            b,
            f,
            _,
            g
          ), u.el = Ee.el;
        }
        if (B && pe(B, _), !Oe && (C = I && I.onVnodeMounted)) {
          const Ee = u;
          pe(
            () => Me(C, L, Ee),
            _
          );
        }
        (u.shapeFlag & 256 || L && _t(L.vnode) && L.vnode.shapeFlag & 256) && f.a && pe(f.a, _), f.isMounted = !0, u = h = b = null;
      }
    };
    f.scope.on();
    const v = f.effect = new ai(x);
    f.scope.off();
    const m = f.update = v.run.bind(v), P = f.job = v.runIfDirty.bind(v);
    P.i = f, P.id = f.uid, v.scheduler = () => us(P), it(f, !0), m();
  }, Q = (f, u, h) => {
    u.component = f;
    const b = f.vnode.props;
    f.vnode = u, f.next = null, Do(f, u.props, b, h), Ho(f, u.children, h), De(), Ts(f), Ne();
  }, J = (f, u, h, b, _, g, S, x, v = !1) => {
    const m = f && f.children, P = f ? f.shapeFlag : 0, C = u.children, { patchFlag: A, shapeFlag: I } = u;
    if (A > 0) {
      if (A & 128) {
        Qt(
          m,
          C,
          h,
          b,
          _,
          g,
          S,
          x,
          v
        );
        return;
      } else if (A & 256) {
        tt(
          m,
          C,
          h,
          b,
          _,
          g,
          S,
          x,
          v
        );
        return;
      }
    }
    I & 8 ? (P & 16 && Tt(m, _, g), C !== m && a(h, C)) : P & 16 ? I & 16 ? Qt(
      m,
      C,
      h,
      b,
      _,
      g,
      S,
      x,
      v
    ) : Tt(m, _, g, !0) : (P & 8 && a(h, ""), I & 16 && Ge(
      C,
      h,
      b,
      _,
      g,
      S,
      x,
      v
    ));
  }, tt = (f, u, h, b, _, g, S, x, v) => {
    f = f || pt, u = u || pt;
    const m = f.length, P = u.length, C = Math.min(m, P);
    let A;
    for (A = 0; A < C; A++) {
      const I = u[A] = v ? He(u[A]) : $e(u[A]);
      R(
        f[A],
        I,
        h,
        null,
        _,
        g,
        S,
        x,
        v
      );
    }
    m > P ? Tt(
      f,
      _,
      g,
      !0,
      !1,
      C
    ) : Ge(
      u,
      h,
      b,
      _,
      g,
      S,
      x,
      v,
      C
    );
  }, Qt = (f, u, h, b, _, g, S, x, v) => {
    let m = 0;
    const P = u.length;
    let C = f.length - 1, A = P - 1;
    for (; m <= C && m <= A; ) {
      const I = f[m], D = u[m] = v ? He(u[m]) : $e(u[m]);
      if (Pt(I, D))
        R(
          I,
          D,
          h,
          null,
          _,
          g,
          S,
          x,
          v
        );
      else
        break;
      m++;
    }
    for (; m <= C && m <= A; ) {
      const I = f[C], D = u[A] = v ? He(u[A]) : $e(u[A]);
      if (Pt(I, D))
        R(
          I,
          D,
          h,
          null,
          _,
          g,
          S,
          x,
          v
        );
      else
        break;
      C--, A--;
    }
    if (m > C) {
      if (m <= A) {
        const I = A + 1, D = I < P ? u[I].el : b;
        for (; m <= A; )
          R(
            null,
            u[m] = v ? He(u[m]) : $e(u[m]),
            h,
            D,
            _,
            g,
            S,
            x,
            v
          ), m++;
      }
    } else if (m > A)
      for (; m <= C; )
        Te(f[m], _, g, !0), m++;
    else {
      const I = m, D = m, B = /* @__PURE__ */ new Map();
      for (m = D; m <= A; m++) {
        const ge = u[m] = v ? He(u[m]) : $e(u[m]);
        ge.key != null && B.set(ge.key, m);
      }
      let L, te = 0;
      const se = A - D + 1;
      let Oe = !1, Ee = 0;
      const Et = new Array(se);
      for (m = 0; m < se; m++) Et[m] = 0;
      for (m = I; m <= C; m++) {
        const ge = f[m];
        if (te >= se) {
          Te(ge, _, g, !0);
          continue;
        }
        let Ae;
        if (ge.key != null)
          Ae = B.get(ge.key);
        else
          for (L = D; L <= A; L++)
            if (Et[L - D] === 0 && Pt(ge, u[L])) {
              Ae = L;
              break;
            }
        Ae === void 0 ? Te(ge, _, g, !0) : (Et[Ae - D] = m + 1, Ae >= Ee ? Ee = Ae : Oe = !0, R(
          ge,
          u[Ae],
          h,
          null,
          _,
          g,
          S,
          x,
          v
        ), te++);
      }
      const bs = Oe ? ko(Et) : pt;
      for (L = bs.length - 1, m = se - 1; m >= 0; m--) {
        const ge = D + m, Ae = u[ge], ys = u[ge + 1], vs = ge + 1 < P ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          ys.el || nr(ys)
        ) : b;
        Et[m] === 0 ? R(
          null,
          Ae,
          h,
          vs,
          _,
          g,
          S,
          x,
          v
        ) : Oe && (L < 0 || m !== bs[L] ? nt(Ae, h, vs, 2) : L--);
      }
    }
  }, nt = (f, u, h, b, _ = null) => {
    const { el: g, type: S, transition: x, children: v, shapeFlag: m } = f;
    if (m & 6) {
      nt(f.component.subTree, u, h, b);
      return;
    }
    if (m & 128) {
      f.suspense.move(u, h, b);
      return;
    }
    if (m & 64) {
      S.move(f, u, h, Ot);
      return;
    }
    if (S === le) {
      s(g, u, h);
      for (let C = 0; C < v.length; C++)
        nt(v[C], u, h, b);
      s(f.anchor, u, h);
      return;
    }
    if (S === Nn) {
      w(f, u, h);
      return;
    }
    if (b !== 2 && m & 1 && x)
      if (b === 0)
        x.persisted && !g[Fn] ? s(g, u, h) : (x.beforeEnter(g), s(g, u, h), pe(() => x.enter(g), _));
      else {
        const { leave: C, delayLeave: A, afterLeave: I } = x, D = () => {
          f.ctx.isUnmounted ? i(g) : s(g, u, h);
        }, B = () => {
          const L = g._isLeaving || !!g[Fn];
          g._isLeaving && g[Fn](
            !0
            /* cancelled */
          ), x.persisted && !L ? D() : C(g, () => {
            D(), I && I();
          });
        };
        A ? A(g, D, B) : B();
      }
    else
      s(g, u, h);
  }, Te = (f, u, h, b = !1, _ = !1) => {
    const {
      type: g,
      props: S,
      ref: x,
      children: v,
      dynamicChildren: m,
      shapeFlag: P,
      patchFlag: C,
      dirs: A,
      cacheIndex: I,
      memo: D
    } = f;
    if (C === -2 && (_ = !1), x != null && (De(), Vt(x, null, h, f, !0), Ne()), I != null && (u.renderCache[I] = void 0), P & 256) {
      u.ctx.deactivate(f);
      return;
    }
    const B = P & 1 && A, L = !_t(f);
    let te;
    if (L && (te = S && S.onVnodeBeforeUnmount) && Me(te, u, f), P & 6)
      hr(f.component, h, b);
    else {
      if (P & 128) {
        f.suspense.unmount(h, b);
        return;
      }
      B && st(f, null, u, "beforeUnmount"), P & 64 ? f.type.remove(
        f,
        u,
        h,
        Ot,
        b
      ) : m && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !m.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== le || C > 0 && C & 64) ? Tt(
        m,
        u,
        h,
        !1,
        !0
      ) : (g === le && C & 384 || !_ && P & 16) && Tt(v, u, h), b && ms(f);
    }
    const se = D != null && I == null;
    (L && (te = S && S.onVnodeUnmounted) || B || se) && pe(() => {
      te && Me(te, u, f), B && st(f, null, u, "unmounted"), se && (f.el = null);
    }, h);
  }, ms = (f) => {
    const { type: u, el: h, anchor: b, transition: _ } = f;
    if (u === le) {
      dr(h, b);
      return;
    }
    if (u === Nn) {
      y(f);
      return;
    }
    const g = () => {
      i(h), _ && !_.persisted && _.afterLeave && _.afterLeave();
    };
    if (f.shapeFlag & 1 && _ && !_.persisted) {
      const { leave: S, delayLeave: x } = _, v = () => S(h, g);
      x ? x(f.el, g, v) : v();
    } else
      g();
  }, dr = (f, u) => {
    let h;
    for (; f !== u; )
      h = T(f), i(f), f = h;
    i(u);
  }, hr = (f, u, h) => {
    const { bum: b, scope: _, job: g, subTree: S, um: x, m: v, a: m } = f;
    Ds(v), Ds(m), b && sn(b), _.stop(), g && (g.flags |= 8, Te(S, f, u, h)), x && pe(x, u), pe(() => {
      f.isUnmounted = !0;
    }, u);
  }, Tt = (f, u, h, b = !1, _ = !1, g = 0) => {
    for (let S = g; S < f.length; S++)
      Te(f[S], u, h, b, _);
  }, Zt = (f) => {
    if (f.shapeFlag & 6)
      return Zt(f.component.subTree);
    if (f.shapeFlag & 128)
      return f.suspense.next();
    const u = T(f.anchor || f.el), h = u && u[oo];
    return h ? T(h) : u;
  };
  let An = !1;
  const _s = (f, u, h) => {
    let b;
    f == null ? u._vnode && (Te(u._vnode, null, null, !0), b = u._vnode.component) : R(
      u._vnode || null,
      f,
      u,
      null,
      null,
      null,
      h
    ), u._vnode = f, An || (An = !0, Ts(b), Pi(), An = !1);
  }, Ot = {
    p: R,
    um: Te,
    m: nt,
    r: ms,
    mt: En,
    mc: Ge,
    pc: J,
    pbc: et,
    n: Zt,
    o: e
  };
  return {
    render: _s,
    hydrate: void 0,
    createApp: Eo(_s)
  };
}
function Dn({ type: e, props: t }, n) {
  return n === "svg" && e === "foreignObject" || n === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : n;
}
function it({ effect: e, job: t }, n) {
  n ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Bo(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function er(e, t, n = !1) {
  const s = e.children, i = t.children;
  if (M(s) && M(i))
    for (let r = 0; r < s.length; r++) {
      const o = s[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = He(i[r]), l.el = o.el), !n && l.patchFlag !== -2 && er(o, l)), l.type === Tn && (l.patchFlag === -1 && (l = i[r] = He(l)), l.el = o.el), l.type === qe && !l.el && (l.el = o.el);
    }
}
function ko(e) {
  const t = e.slice(), n = [0];
  let s, i, r, o, l;
  const c = e.length;
  for (s = 0; s < c; s++) {
    const d = e[s];
    if (d !== 0) {
      if (i = n[n.length - 1], e[i] < d) {
        t[s] = i, n.push(s);
        continue;
      }
      for (r = 0, o = n.length - 1; r < o; )
        l = r + o >> 1, e[n[l]] < d ? r = l + 1 : o = l;
      d < e[n[r]] && (r > 0 && (t[s] = n[r - 1]), n[r] = s);
    }
  }
  for (r = n.length, o = n[r - 1]; r-- > 0; )
    n[r] = o, o = t[o];
  return n;
}
function tr(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : tr(t);
}
function Ds(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function nr(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? nr(t.subTree) : null;
}
const sr = (e) => e.__isSuspense;
function Wo(e, t) {
  t && t.pendingBranch ? M(e) ? t.effects.push(...e) : t.effects.push(e) : to(e);
}
const le = /* @__PURE__ */ Symbol.for("v-fgt"), Tn = /* @__PURE__ */ Symbol.for("v-txt"), qe = /* @__PURE__ */ Symbol.for("v-cmt"), Nn = /* @__PURE__ */ Symbol.for("v-stc"), Nt = [];
let me = null;
function Y(e = !1) {
  Nt.push(me = e ? null : []);
}
function qo() {
  Nt.pop(), me = Nt[Nt.length - 1] || null;
}
let Kt = 1;
function Ns(e, t = !1) {
  Kt += e, e < 0 && me && t && (me.hasOnce = !0);
}
function ir(e) {
  return e.dynamicChildren = Kt > 0 ? me || pt : null, qo(), Kt > 0 && me && me.push(e), e;
}
function ne(e, t, n, s, i, r) {
  return ir(
    $(
      e,
      t,
      n,
      s,
      i,
      r,
      !0
    )
  );
}
function Qe(e, t, n, s, i) {
  return ir(
    _e(
      e,
      t,
      n,
      s,
      i,
      !0
    )
  );
}
function ps(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function Pt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const rr = ({ key: e }) => e ?? null, ln = ({
  ref: e,
  ref_key: t,
  ref_for: n
}) => (typeof e == "number" && (e = "" + e), e != null ? ee(e) || /* @__PURE__ */ fe(e) || F(e) ? { i: ce, r: e, k: t, f: !!n } : e : null);
function $(e, t = null, n = null, s = 0, i = null, r = e === le ? 0 : 1, o = !1, l = !1) {
  const c = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && rr(t),
    ref: t && ln(t),
    scopeId: Ii,
    slotScopeIds: null,
    children: n,
    component: null,
    suspense: null,
    ssContent: null,
    ssFallback: null,
    dirs: null,
    transition: null,
    el: null,
    anchor: null,
    target: null,
    targetStart: null,
    targetAnchor: null,
    staticCount: 0,
    shapeFlag: r,
    patchFlag: s,
    dynamicProps: i,
    dynamicChildren: null,
    appContext: null,
    ctx: ce
  };
  return l ? (pn(c, n), r & 128 && e.normalize(c)) : n && (c.shapeFlag |= ee(n) ? 8 : 16), Kt > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  me && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (c.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  c.patchFlag !== 32 && me.push(c), c;
}
const _e = Jo;
function Jo(e, t = null, n = null, s = 0, i = null, r = !1) {
  if ((!e || e === yo) && (e = qe), ps(e)) {
    const l = vt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return n && pn(l, n), Kt > 0 && !r && me && (l.shapeFlag & 6 ? me[me.indexOf(e)] = l : me.push(l)), l.patchFlag = -2, l;
  }
  if (il(e) && (e = e.__vccOpts), t) {
    t = Go(t);
    let { class: l, style: c } = t;
    l && !ee(l) && (t.class = qt(l)), K(c) && (/* @__PURE__ */ fs(c) && !M(c) && (c = re({}, c)), t.style = ts(c));
  }
  const o = ee(e) ? 1 : sr(e) ? 128 : lo(e) ? 64 : K(e) ? 4 : F(e) ? 2 : 0;
  return $(
    e,
    t,
    n,
    s,
    i,
    o,
    r,
    !0
  );
}
function Go(e) {
  return e ? /* @__PURE__ */ fs(e) || Gi(e) ? re({}, e) : e : null;
}
function vt(e, t, n = !1, s = !1) {
  const { props: i, ref: r, patchFlag: o, children: l, transition: c } = e, d = t ? Yo(i || {}, t) : i, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && rr(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      n && r ? M(r) ? r.concat(ln(t)) : [r, ln(t)] : ln(t)
    ) : r,
    scopeId: e.scopeId,
    slotScopeIds: e.slotScopeIds,
    children: l,
    target: e.target,
    targetStart: e.targetStart,
    targetAnchor: e.targetAnchor,
    staticCount: e.staticCount,
    shapeFlag: e.shapeFlag,
    // if the vnode is cloned with extra props, we can no longer assume its
    // existing patch flag to be reliable and need to add the FULL_PROPS flag.
    // note: preserve flag for fragments since they use the flag for children
    // fast paths only.
    patchFlag: t && e.type !== le ? o === -1 ? 16 : o | 16 : o,
    dynamicProps: e.dynamicProps,
    dynamicChildren: e.dynamicChildren,
    appContext: e.appContext,
    dirs: e.dirs,
    transition: c,
    // These should technically only be non-null on mounted VNodes. However,
    // they *should* be copied for kept-alive vnodes. So we just always copy
    // them since them being non-null during a mount doesn't affect the logic as
    // they will simply be overwritten.
    component: e.component,
    suspense: e.suspense,
    ssContent: e.ssContent && vt(e.ssContent),
    ssFallback: e.ssFallback && vt(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return c && s && as(
    a,
    c.clone(a)
  ), a;
}
function ut(e = " ", t = 0) {
  return _e(Tn, null, e, t);
}
function Be(e = "", t = !1) {
  return t ? (Y(), Qe(qe, null, e)) : _e(qe, null, e);
}
function $e(e) {
  return e == null || typeof e == "boolean" ? _e(qe) : M(e) ? _e(
    le,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : ps(e) ? He(e) : _e(Tn, null, String(e));
}
function He(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : vt(e);
}
function pn(e, t) {
  let n = 0;
  const { shapeFlag: s } = e;
  if (t == null)
    t = null;
  else if (M(t))
    n = 16;
  else if (typeof t == "object")
    if (s & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), pn(e, i()), i._c && (i._d = !0));
      return;
    } else {
      n = 32;
      const i = t._;
      !i && !Gi(t) ? t._ctx = ce : i === 3 && ce && (ce.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (F(t)) {
    if (s & 65) {
      pn(e, { default: t });
      return;
    }
    t = { default: t, _ctx: ce }, n = 32;
  } else
    t = String(t), s & 64 ? (n = 16, t = [ut(t)]) : n = 8;
  e.children = t, e.shapeFlag |= n;
}
function Yo(...e) {
  const t = {};
  for (let n = 0; n < e.length; n++) {
    const s = e[n];
    for (const i in s)
      if (i === "class")
        t.class !== s.class && (t.class = qt([t.class, s.class]));
      else if (i === "style")
        t.style = ts([t.style, s.style]);
      else if (mn(i)) {
        const r = t[i], o = s[i];
        o && r !== o && !(M(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !_n(i) && (t[i] = o);
      } else i !== "" && (t[i] = s[i]);
  }
  return t;
}
function Me(e, t, n, s = null) {
  Ce(e, t, 7, [
    n,
    s
  ]);
}
const zo = Bi();
let Xo = 0;
function Qo(e, t, n) {
  const s = e.type, i = (t ? t.appContext : e.appContext) || zo, r = {
    uid: Xo++,
    vnode: e,
    type: s,
    parent: t,
    appContext: i,
    root: null,
    // to be immediately set
    next: null,
    subTree: null,
    // will be set synchronously right after creation
    effect: null,
    update: null,
    // will be set synchronously right after creation
    job: null,
    scope: new Tr(
      !0
      /* detached */
    ),
    render: null,
    proxy: null,
    exposed: null,
    exposeProxy: null,
    withProxy: null,
    provides: t ? t.provides : Object.create(i.provides),
    ids: t ? t.ids : ["", 0, 0],
    accessCache: null,
    renderCache: [],
    // local resolved assets
    components: null,
    directives: null,
    // resolved props and emits options
    propsOptions: zi(s, i),
    emitsOptions: ki(s, i),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: W,
    // inheritAttrs
    inheritAttrs: s.inheritAttrs,
    // state
    ctx: W,
    data: W,
    props: W,
    attrs: W,
    slots: W,
    refs: W,
    setupState: W,
    setupContext: null,
    // suspense related
    suspense: n,
    suspenseId: n ? n.pendingId : 0,
    asyncDep: null,
    asyncResolved: !1,
    // lifecycle hooks
    // not using enums here because it results in computed properties
    isMounted: !1,
    isUnmounted: !1,
    isDeactivated: !1,
    bc: null,
    c: null,
    bm: null,
    m: null,
    bu: null,
    u: null,
    um: null,
    bum: null,
    da: null,
    a: null,
    rtg: null,
    rtc: null,
    ec: null,
    sp: null
  };
  return r.ctx = { _: r }, r.root = t ? t.root : r, r.emit = Po.bind(null, r), e.ce && e.ce(r), r;
}
let he = null;
const Zo = () => he || ce;
let gn, Yn;
{
  const e = vn(), t = (n, s) => {
    let i;
    return (i = e[n]) || (i = e[n] = []), i.push(s), (r) => {
      i.length > 1 ? i.forEach((o) => o(r)) : i[0](r);
    };
  };
  gn = t(
    "__VUE_INSTANCE_SETTERS__",
    (n) => he = n
  ), Yn = t(
    "__VUE_SSR_SETTERS__",
    (n) => Bt = n
  );
}
const Gt = (e) => {
  const t = he;
  return gn(e), e.scope.on(), () => {
    e.scope.off(), gn(t);
  };
}, js = () => {
  he && he.scope.off(), gn(null);
};
function or(e) {
  return e.vnode.shapeFlag & 4;
}
let Bt = !1;
function el(e, t = !1, n = !1) {
  t && Yn(t);
  const { props: s, children: i } = e.vnode, r = or(e);
  Vo(e, s, r, t), Uo(e, i, n || t);
  const o = r ? tl(e, t) : void 0;
  return t && Yn(!1), o;
}
function tl(e, t) {
  const n = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, vo);
  const { setup: s } = n;
  if (s) {
    De();
    const i = e.setupContext = s.length > 1 ? sl(e) : null, r = Gt(e), o = Jt(
      s,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = si(o);
    if (Ne(), r(), (l || e.sp) && !_t(e) && Fi(e), l) {
      if (o.then(js, js), t)
        return o.then((c) => {
          Us(e, c);
        }).catch((c) => {
          Sn(c, e, 0);
        });
      e.asyncDep = o;
    } else
      Us(e, o);
  } else
    lr(e);
}
function Us(e, t, n) {
  F(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : K(t) && (e.setupState = Ti(t)), lr(e);
}
function lr(e, t, n) {
  const s = e.type;
  e.render || (e.render = s.render || Ve);
  {
    const i = Gt(e);
    De();
    try {
      xo(e);
    } finally {
      Ne(), i();
    }
  }
}
const nl = {
  get(e, t) {
    return oe(e, "get", ""), e[t];
  }
};
function sl(e) {
  const t = (n) => {
    e.exposed = n || {};
  };
  return {
    attrs: new Proxy(e.attrs, nl),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function On(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(Ti(Wr(e.exposed)), {
    get(t, n) {
      if (n in t)
        return t[n];
      if (n in Dt)
        return Dt[n](e);
    },
    has(t, n) {
      return n in t || n in Dt;
    }
  })) : e.proxy;
}
function il(e) {
  return F(e) && "__vccOpts" in e;
}
const rl = (e, t) => /* @__PURE__ */ zr(e, t, Bt), ol = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let zn;
const Hs = typeof window < "u" && window.trustedTypes;
if (Hs)
  try {
    zn = /* @__PURE__ */ Hs.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const cr = zn ? (e) => zn.createHTML(e) : (e) => e, ll = "http://www.w3.org/2000/svg", cl = "http://www.w3.org/1998/Math/MathML", Ue = typeof document < "u" ? document : null, Ls = Ue && /* @__PURE__ */ Ue.createElement("template"), fl = {
  insert: (e, t, n) => {
    t.insertBefore(e, n || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, n, s) => {
    const i = t === "svg" ? Ue.createElementNS(ll, e) : t === "mathml" ? Ue.createElementNS(cl, e) : n ? Ue.createElement(e, { is: n }) : Ue.createElement(e);
    return e === "select" && s && s.multiple != null && i.setAttribute("multiple", s.multiple), i;
  },
  createText: (e) => Ue.createTextNode(e),
  createComment: (e) => Ue.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => Ue.querySelector(e),
  setScopeId(e, t) {
    e.setAttribute(t, "");
  },
  // __UNSAFE__
  // Reason: innerHTML.
  // Static content here can only come from compiled templates.
  // As long as the user only uses trusted templates, this is safe.
  insertStaticContent(e, t, n, s, i, r) {
    const o = n ? n.previousSibling : t.lastChild;
    if (i && (i === r || i.nextSibling))
      for (; t.insertBefore(i.cloneNode(!0), n), !(i === r || !(i = i.nextSibling)); )
        ;
    else {
      Ls.innerHTML = cr(
        s === "svg" ? `<svg>${e}</svg>` : s === "mathml" ? `<math>${e}</math>` : e
      );
      const l = Ls.content;
      if (s === "svg" || s === "mathml") {
        const c = l.firstChild;
        for (; c.firstChild; )
          l.appendChild(c.firstChild);
        l.removeChild(c);
      }
      t.insertBefore(l, n);
    }
    return [
      // first
      o ? o.nextSibling : t.firstChild,
      // last
      n ? n.previousSibling : t.lastChild
    ];
  }
}, ul = /* @__PURE__ */ Symbol("_vtc");
function al(e, t, n) {
  const s = e[ul];
  s && (t = (t ? [t, ...s] : [...s]).join(" ")), t == null ? e.removeAttribute("class") : n ? e.setAttribute("class", t) : e.className = t;
}
const Ks = /* @__PURE__ */ Symbol("_vod"), dl = /* @__PURE__ */ Symbol("_vsh"), hl = /* @__PURE__ */ Symbol(""), pl = /(?:^|;)\s*display\s*:/;
function gl(e, t, n) {
  const s = e.style, i = ee(n);
  let r = !1;
  if (n && !i) {
    if (t)
      if (ee(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          n[l] == null && It(s, l, "");
        }
      else
        for (const o in t)
          n[o] == null && It(s, o, "");
    for (const o in n) {
      o === "display" && (r = !0);
      const l = n[o];
      l != null ? _l(
        e,
        o,
        !ee(t) && t ? t[o] : void 0,
        l
      ) || It(s, o, l) : It(s, o, "");
    }
  } else if (i) {
    if (t !== n) {
      const o = s[hl];
      o && (n += ";" + o), s.cssText = n, r = pl.test(n);
    }
  } else t && e.removeAttribute("style");
  Ks in e && (e[Ks] = r ? s.display : "", e[dl] && (s.display = "none"));
}
const Bs = /\s*!important$/;
function It(e, t, n) {
  if (M(n))
    n.forEach((s) => It(e, t, s));
  else if (n == null && (n = ""), t.startsWith("--"))
    e.setProperty(t, n);
  else {
    const s = ml(e, t);
    Bs.test(n) ? e.setProperty(
      at(s),
      n.replace(Bs, ""),
      "important"
    ) : e[s] = n;
  }
}
const ks = ["Webkit", "Moz", "ms"], jn = {};
function ml(e, t) {
  const n = jn[t];
  if (n)
    return n;
  let s = ve(t);
  if (s !== "filter" && s in e)
    return jn[t] = s;
  s = oi(s);
  for (let i = 0; i < ks.length; i++) {
    const r = ks[i] + s;
    if (r in e)
      return jn[t] = r;
  }
  return t;
}
function _l(e, t, n, s) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && ee(s) && n === s;
}
const Ws = "http://www.w3.org/1999/xlink";
function qs(e, t, n, s, i, r = wr(t)) {
  s && t.startsWith("xlink:") ? n == null ? e.removeAttributeNS(Ws, t.slice(6, t.length)) : e.setAttributeNS(Ws, t, n) : n == null || r && !ci(n) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : Se(n) ? String(n) : n
  );
}
function Js(e, t, n, s, i) {
  if (t === "innerHTML" || t === "textContent") {
    n != null && (e[t] = t === "innerHTML" ? cr(n) : n);
    return;
  }
  const r = e.tagName;
  if (t === "value" && r !== "PROGRESS" && // custom elements may use _value internally
  !r.includes("-")) {
    const l = r === "OPTION" ? e.getAttribute("value") || "" : e.value, c = n == null ? (
      // #11647: value should be set as empty string for null and undefined,
      // but <input type="checkbox"> should be set as 'on'.
      e.type === "checkbox" ? "on" : ""
    ) : String(n);
    (l !== c || !("_value" in e)) && (e.value = c), n == null && e.removeAttribute(t), e._value = n;
    return;
  }
  let o = !1;
  if (n === "" || n == null) {
    const l = typeof e[t];
    l === "boolean" ? n = ci(n) : n == null && l === "string" ? (n = "", o = !0) : l === "number" && (n = 0, o = !0);
  }
  try {
    e[t] = n;
  } catch {
  }
  o && e.removeAttribute(i || t);
}
function ze(e, t, n, s) {
  e.addEventListener(t, n, s);
}
function bl(e, t, n, s) {
  e.removeEventListener(t, n, s);
}
const Gs = /* @__PURE__ */ Symbol("_vei");
function yl(e, t, n, s, i = null) {
  const r = e[Gs] || (e[Gs] = {}), o = r[t];
  if (s && o)
    o.value = s;
  else {
    const [l, c] = Sl(t);
    if (s) {
      const d = r[t] = Tl(
        s,
        i
      );
      ze(e, l, d, c);
    } else o && (bl(e, l, o, c), r[t] = void 0);
  }
}
const vl = /(Once|Passive|Capture)$/, xl = /^on:?(?:Once|Passive|Capture)$/;
function Sl(e) {
  let t, n;
  for (; (n = e.match(vl)) && !xl.test(e); )
    t || (t = {}), e = e.slice(0, e.length - n[1].length), t[n[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : at(e.slice(2)), t];
}
let Un = 0;
const wl = /* @__PURE__ */ Promise.resolve(), Cl = () => Un || (wl.then(() => Un = 0), Un = Date.now());
function Tl(e, t) {
  const n = (s) => {
    if (!s._vts)
      s._vts = Date.now();
    else if (s._vts <= n.attached)
      return;
    const i = n.value;
    if (M(i)) {
      const r = s.stopImmediatePropagation;
      s.stopImmediatePropagation = () => {
        r.call(s), s._stopped = !0;
      };
      const o = i.slice(), l = [s];
      for (let c = 0; c < o.length && !s._stopped; c++) {
        const d = o[c];
        d && Ce(
          d,
          t,
          5,
          l
        );
      }
    } else
      Ce(
        i,
        t,
        5,
        [s]
      );
  };
  return n.value = e, n.attached = Cl(), n;
}
const Ys = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, Ol = (e, t, n, s, i, r) => {
  const o = i === "svg";
  t === "class" ? al(e, s, o) : t === "style" ? gl(e, n, s) : mn(t) ? _n(t) || yl(e, t, n, s, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : El(e, t, s, o)) ? (Js(e, t, s), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && qs(e, t, s, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (Al(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !ee(s))) ? Js(e, ve(t), s, r, t) : (t === "true-value" ? e._trueValue = s : t === "false-value" && (e._falseValue = s), qs(e, t, s, o));
};
function El(e, t, n, s) {
  if (s)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Ys(t) && F(n));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return Ys(t) && ee(n) ? !1 : t in e;
}
function Al(e, t) {
  const n = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!n)
    return !1;
  const s = ve(t);
  return Array.isArray(n) ? n.some((i) => ve(i) === s) : Object.keys(n).some((i) => ve(i) === s);
}
const xt = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return M(t) ? (n) => sn(t, n) : t;
};
function Pl(e) {
  e.target.composing = !0;
}
function zs(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const ke = /* @__PURE__ */ Symbol("_assign");
function Xs(e, t, n) {
  return t && (e = e.trim()), n && (e = yn(e)), e;
}
const rt = {
  created(e, { modifiers: { lazy: t, trim: n, number: s } }, i) {
    e[ke] = xt(i);
    const r = s || i.props && i.props.type === "number";
    ze(e, t ? "change" : "input", (o) => {
      o.target.composing || e[ke](Xs(e.value, n, r));
    }), (n || r) && ze(e, "change", () => {
      e.value = Xs(e.value, n, r);
    }), t || (ze(e, "compositionstart", Pl), ze(e, "compositionend", zs), ze(e, "change", zs));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: n, modifiers: { lazy: s, trim: i, number: r } }, o) {
    if (e[ke] = xt(o), e.composing) return;
    const l = (r || e.type === "number") && !/^0\d/.test(e.value) ? yn(e.value) : e.value, c = t ?? "";
    if (l === c)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (s && t === n || i && e.value.trim() === c) || (e.value = c);
  }
}, Ml = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, n) {
    e[ke] = xt(n), ze(e, "change", () => {
      const s = e._modelValue, i = kt(e), r = e.checked, o = e[ke];
      if (M(s)) {
        const l = ns(s, i), c = l !== -1;
        if (r && !c)
          o(s.concat(i));
        else if (!r && c) {
          const d = [...s];
          d.splice(l, 1), o(d);
        }
      } else if (St(s)) {
        const l = new Set(s);
        r ? l.add(i) : l.delete(i), o(l);
      } else
        o(fr(e, r));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: Qs,
  beforeUpdate(e, t, n) {
    e[ke] = xt(n), Qs(e, t, n);
  }
};
function Qs(e, { value: t, oldValue: n }, s) {
  e._modelValue = t;
  let i;
  if (M(t))
    i = ns(t, s.props.value) > -1;
  else if (St(t))
    i = t.has(s.props.value);
  else {
    if (t === n) return;
    i = wt(t, fr(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const Zs = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: n } }, s) {
    const i = St(t);
    ze(e, "change", () => {
      const r = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => n ? yn(kt(o)) : kt(o)
      );
      e[ke](
        e.multiple ? i ? new Set(r) : r : r[0]
      ), e._assigning = !0, Ei(() => {
        e._assigning = !1;
      });
    }), e[ke] = xt(s);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    ei(e, t);
  },
  beforeUpdate(e, t, n) {
    e[ke] = xt(n);
  },
  updated(e, { value: t }) {
    e._assigning || ei(e, t);
  }
};
function ei(e, t) {
  const n = e.multiple, s = M(t);
  if (!(n && !s && !St(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = kt(o);
      if (n)
        if (s) {
          const c = typeof l;
          c === "string" || c === "number" ? o.selected = t.some((d) => String(d) === String(l)) : o.selected = ns(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (wt(kt(o), t)) {
        e.selectedIndex !== i && (e.selectedIndex = i);
        return;
      }
    }
    !n && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function kt(e) {
  return "_value" in e ? e._value : e.value;
}
function fr(e, t) {
  const n = t ? "_trueValue" : "_falseValue";
  return n in e ? e[n] : t;
}
const Il = ["ctrl", "shift", "alt", "meta"], Rl = {
  stop: (e) => e.stopPropagation(),
  prevent: (e) => e.preventDefault(),
  self: (e) => e.target !== e.currentTarget,
  ctrl: (e) => !e.ctrlKey,
  shift: (e) => !e.shiftKey,
  alt: (e) => !e.altKey,
  meta: (e) => !e.metaKey,
  left: (e) => "button" in e && e.button !== 0,
  middle: (e) => "button" in e && e.button !== 1,
  right: (e) => "button" in e && e.button !== 2,
  exact: (e, t) => Il.some((n) => e[`${n}Key`] && !t.includes(n))
}, $l = (e, t) => {
  if (!e) return e;
  const n = e._withMods || (e._withMods = {}), s = t.join(".");
  return n[s] || (n[s] = ((i, ...r) => {
    for (let o = 0; o < t.length; o++) {
      const l = Rl[t[o]];
      if (l && l(i, t)) return;
    }
    return e(i, ...r);
  }));
}, Fl = /* @__PURE__ */ re({ patchProp: Ol }, fl);
let ti;
function Vl() {
  return ti || (ti = Lo(Fl));
}
const Dl = ((...e) => {
  const t = Vl().createApp(...e), { mount: n } = t;
  return t.mount = (s) => {
    const i = jl(s);
    if (!i) return;
    const r = t._component;
    !F(r) && !r.render && !r.template && (r.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
    const o = n(i, !1, Nl(i));
    return i instanceof Element && (i.removeAttribute("v-cloak"), i.setAttribute("data-v-app", "")), o;
  }, t;
});
function Nl(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function jl(e) {
  return ee(e) ? document.querySelector(e) : e;
}
let Xn = null;
function Ul(e) {
  Xn = e ?? null;
}
function X(e, t) {
  return Xn ? Xn(e, t) : e;
}
const Hl = ["value", "placeholder"], Ll = /* @__PURE__ */ Ze({
  __name: "CredentialSlot",
  props: {
    api: {},
    modelValue: {}
  },
  emits: ["update:modelValue"],
  setup(e, { emit: t }) {
    const n = e, s = t, i = /* @__PURE__ */ cn(null), r = /* @__PURE__ */ cn(!1);
    let o = null;
    return Ni(() => {
      !i.value || !n.api.mountCredentialField || (o = n.api.mountCredentialField(i.value, {
        value: n.modelValue,
        noneLabel: "(none — use fields below)",
        onChange: (l) => s("update:modelValue", l)
      }), r.value = !0);
    }), on(() => n.modelValue, (l) => o == null ? void 0 : o.setValue(l)), ji(() => o == null ? void 0 : o.destroy()), (l, c) => (Y(), ne("div", {
      ref_key: "el",
      ref: i,
      class: "cred-slot"
    }, [
      r.value ? Be("", !0) : (Y(), ne("input", {
        key: 0,
        value: e.modelValue,
        class: "w-260",
        spellcheck: "false",
        placeholder: z(X)("secret:<scope>:<entry>"),
        onInput: c[0] || (c[0] = (d) => s("update:modelValue", d.target.value))
      }, null, 40, Hl))
    ], 512));
  }
}), ur = (e, t) => {
  const n = e.__vccOpts || e;
  for (const [s, i] of t)
    n[s] = i;
  return n;
}, Kl = /* @__PURE__ */ ur(Ll, [["__scopeId", "data-v-17c11f13"]]), Bl = ["disabled"], kl = /* @__PURE__ */ Ze({
  __name: "AddButton",
  props: {
    label: { default: "Add" },
    disabled: { type: Boolean, default: !1 }
  },
  emits: ["click"],
  setup(e) {
    return (t, n) => (Y(), ne("button", {
      type: "button",
      class: "btn ghost list-add",
      disabled: e.disabled,
      onClick: n[0] || (n[0] = (s) => t.$emit("click", s))
    }, [
      n[1] || (n[1] = $("span", {
        class: "list-add-glyph",
        "aria-hidden": "true"
      }, "＋", -1)),
      Xe(t.$slots, "default", {}, () => [
        ut(k(e.label), 1)
      ])
    ], 8, Bl));
  }
}), Wl = { class: "list-panel" }, ql = {
  key: 0,
  class: "list-empty"
}, Jl = /* @__PURE__ */ Ze({
  __name: "ListPanel",
  props: {
    empty: { type: Boolean },
    emptyText: { default: "Nothing here yet." },
    addLabel: { default: "" },
    addDisabled: { type: Boolean, default: !1 }
  },
  emits: ["add"],
  setup(e) {
    return (t, n) => (Y(), ne("div", Wl, [
      e.empty ? (Y(), ne("div", ql, [
        Xe(t.$slots, "empty", {}, () => [
          ut(k(e.emptyText), 1)
        ])
      ])) : Be("", !0),
      Xe(t.$slots, "default"),
      e.addLabel ? (Y(), Qe(kl, {
        key: 1,
        label: e.addLabel,
        disabled: e.addDisabled,
        onClick: n[0] || (n[0] = (s) => t.$emit("add"))
      }, null, 8, ["label", "disabled"])) : Be("", !0)
    ]));
  }
}), Gl = ["disabled", "title"], ar = /* @__PURE__ */ Ze({
  __name: "IconGlyphButton",
  props: {
    title: {},
    variant: { default: "plain" },
    disabled: { type: Boolean, default: !1 },
    on: { type: Boolean, default: !1 }
  },
  emits: ["click"],
  setup(e) {
    return (t, n) => (Y(), ne("button", {
      type: "button",
      class: qt(["gbtn", [`gbtn-${e.variant}`, { on: e.on }]]),
      disabled: e.disabled,
      title: e.title,
      onClick: n[0] || (n[0] = $l((s) => t.$emit("click", s), ["stop"]))
    }, [
      Xe(t.$slots, "default")
    ], 10, Gl));
  }
}), Yl = /* @__PURE__ */ Ze({
  __name: "ExpandButton",
  props: {
    open: { type: Boolean }
  },
  emits: ["update:open"],
  setup(e) {
    return (t, n) => (Y(), Qe(ar, {
      variant: "plain",
      on: e.open,
      title: e.open ? "Collapse" : "Expand",
      onClick: n[0] || (n[0] = (s) => t.$emit("update:open", !e.open))
    }, {
      default: lt(() => [
        ut(k(e.open ? "▾" : "▸"), 1)
      ]),
      _: 1
    }, 8, ["on", "title"]));
  }
}), zl = { class: "list-card-title" }, Xl = {
  key: 1,
  class: "list-card-summary"
}, Ql = { class: "list-card-actions" }, Zl = {
  key: 0,
  class: "list-card-body"
}, ec = /* @__PURE__ */ Ze({
  __name: "ListCard",
  props: {
    open: { type: Boolean, default: !1 },
    title: { default: "" },
    titleOpen: { default: "" },
    summary: { default: "" },
    noToggleOnClick: { type: Boolean, default: !1 },
    collapsible: { type: Boolean, default: !0 }
  },
  emits: ["update:open"],
  setup(e, { emit: t }) {
    const n = e, s = t;
    function i() {
      !n.noToggleOnClick && n.collapsible && s("update:open", !n.open);
    }
    return (r, o) => (Y(), ne("div", {
      class: qt(["list-card", { open: e.open }])
    }, [
      $("div", {
        class: "list-card-head",
        onClick: i
      }, [
        e.collapsible ? (Y(), Qe(Yl, {
          key: 0,
          open: e.open,
          "onUpdate:open": o[0] || (o[0] = (l) => s("update:open", l))
        }, null, 8, ["open"])) : Be("", !0),
        $("div", zl, [
          Xe(r.$slots, "title", { open: e.open }, () => [
            ut(k(e.open && e.titleOpen ? e.titleOpen : e.title), 1)
          ])
        ]),
        !e.open && (e.summary || r.$slots.summary) ? (Y(), ne("div", Xl, [
          Xe(r.$slots, "summary", {}, () => [
            ut(k(e.summary), 1)
          ])
        ])) : Be("", !0),
        $("div", Ql, [
          Xe(r.$slots, "actions", { open: e.open })
        ])
      ]),
      e.open && r.$slots.body ? (Y(), ne("div", Zl, [
        Xe(r.$slots, "body")
      ])) : Be("", !0)
    ], 2));
  }
}), tc = /* @__PURE__ */ Ze({
  __name: "DeleteButton",
  props: {
    title: { default: "Delete" },
    disabled: { type: Boolean, default: !1 }
  },
  emits: ["click"],
  setup(e) {
    return (t, n) => (Y(), Qe(ar, {
      variant: "danger",
      title: e.title,
      disabled: e.disabled,
      onClick: n[0] || (n[0] = (s) => t.$emit("click", s))
    }, {
      default: lt(() => [...n[1] || (n[1] = [
        ut("🗑︎", -1)
      ])]),
      _: 1
    }, 8, ["title", "disabled"]));
  }
}), nc = { class: "sql-set" }, sc = { class: "muted" }, ic = { class: "row" }, rc = { class: "muted" }, oc = { value: "" }, lc = ["value"], cc = { class: "muted" }, fc = { class: "muted" }, uc = { class: "row" }, ac = { class: "muted w-70" }, dc = ["onUpdate:modelValue"], hc = { class: "muted" }, pc = ["onUpdate:modelValue"], gc = { value: "mssql" }, mc = { value: "postgres" }, _c = { value: "sqlite" }, bc = {
  key: 0,
  class: "row"
}, yc = { class: "muted w-70" }, vc = ["onUpdate:modelValue", "placeholder"], xc = { class: "muted w-70" }, Sc = ["onUpdate:modelValue"], wc = {
  key: 1,
  class: "row"
}, Cc = { class: "muted w-70" }, Tc = ["onUpdate:modelValue", "placeholder"], Oc = { class: "row" }, Ec = {
  key: 0,
  class: "chk"
}, Ac = ["onUpdate:modelValue"], Pc = { class: "row" }, Mc = { class: "muted w-70" }, Ic = { class: "row" }, Rc = { class: "muted w-70" }, $c = ["onUpdate:modelValue", "placeholder"], Fc = { class: "row" }, Vc = { class: "muted w-70" }, Dc = ["onUpdate:modelValue", "placeholder"], Nc = { class: "row" }, jc = ["disabled", "onClick"], Uc = { class: "muted" }, Hc = /* @__PURE__ */ Ze({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const n = e;
    let s = 0;
    function i(w, y) {
      return {
        key: s++,
        name: w,
        provider: y.provider || "mssql",
        path: y.provider === "sqlite" ? y.file || "" : y.server || "",
        database: y.database || "",
        user: y.user || "",
        credential: y.credential || "",
        trustedConnection: y.trusted_connection ?? !0,
        description: y.description || "",
        testing: !1,
        testStatus: ""
      };
    }
    function r(w) {
      return {
        provider: w.provider,
        server: w.provider === "sqlite" ? void 0 : w.path || void 0,
        file: w.provider === "sqlite" && w.path || void 0,
        database: w.database || void 0,
        user: w.user || void 0,
        credential: w.credential || void 0,
        trusted_connection: w.provider === "mssql" ? w.trustedConnection : void 0,
        description: w.description || void 0
        // NOTE: no `password` — literals are written to the secret store via secret.set, never here.
      };
    }
    const o = (() => {
      try {
        return JSON.parse(n.api.getJson() || "null") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ cn(o.default_connection || ""), c = /* @__PURE__ */ cn(o.default_limit || 10), d = /* @__PURE__ */ Ht(
      Object.entries(o.connections || {}).map(([w, y]) => i(w, y))
    ), a = /* @__PURE__ */ Ht(/* @__PURE__ */ new Set()), p = (w) => a.has(w);
    function T(w) {
      a.delete(w) || a.add(w);
    }
    function E(w) {
      if (w.provider === "sqlite") return w.path || "(no file)";
      const y = w.path || "(no server)";
      return w.database ? `${y} / ${w.database}` : y;
    }
    function j() {
      const w = i(`db${d.length + 1}`, { provider: "mssql" });
      d.push(w), a.add(w.key);
    }
    function R(w) {
      const [y] = d.splice(w, 1);
      y && a.delete(y.key);
    }
    async function Z(w) {
      w.testing = !0, w.testStatus = "Connecting...";
      try {
        const y = await n.api.invoke("plugin.action", {
          pluginId: "sql",
          action: "testConnection",
          valueJson: JSON.stringify(q(w))
        });
        if (y.ok && y.resultJson) {
          const O = JSON.parse(y.resultJson);
          w.testStatus = O.message;
        } else
          w.testStatus = "Failed: " + (y.error || "unknown error");
      } catch (y) {
        w.testStatus = "Failed: " + (y instanceof Error ? y.message : String(y));
      } finally {
        w.testing = !1;
      }
    }
    function q(w) {
      const y = r(w);
      return {
        provider: y.provider,
        server: y.server,
        database: y.database,
        user: y.user,
        credential: y.credential,
        trustedConnection: y.trusted_connection,
        file: y.file,
        description: y.description
      };
    }
    function V() {
      const w = {
        default_connection: l.value || void 0,
        default_limit: c.value || 10,
        connections: Object.fromEntries(
          d.filter((y) => y.name.trim()).map((y) => [y.name.trim(), r(y)])
        )
      };
      return JSON.stringify(w);
    }
    return t({ toJson: V }), (w, y) => (Y(), ne("div", nc, [
      $("div", sc, k(z(X)("Named database connections available to the SQL agent. Passwords live in the secret store (Settings → Secrets); a connection only references an entry by name. Stored in the .spla project file.")), 1),
      $("div", ic, [
        $("label", null, [
          $("span", rc, k(z(X)("Default connection")), 1),
          Pe($("select", {
            "onUpdate:modelValue": y[0] || (y[0] = (O) => l.value = O)
          }, [
            $("option", oc, k(z(X)("(none)")), 1),
            (Y(!0), ne(le, null, As(d, (O) => (Y(), ne("option", {
              key: O.key,
              value: O.name
            }, k(O.name), 9, lc))), 128))
          ], 512), [
            [Zs, l.value]
          ])
        ]),
        $("label", null, [
          $("span", cc, k(z(X)("Default row limit")), 1),
          Pe($("input", {
            "onUpdate:modelValue": y[1] || (y[1] = (O) => c.value = O),
            type: "number",
            min: "1",
            class: "w-90"
          }, null, 512), [
            [
              rt,
              c.value,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      _e(Jl, {
        empty: !d.length,
        "empty-text": z(X)("No connections yet."),
        "add-label": z(X)("Connection"),
        onAdd: j
      }, {
        default: lt(() => [
          (Y(!0), ne(le, null, As(d, (O, be) => (Y(), Qe(ec, {
            key: O.key,
            open: p(O.key),
            summary: E(O),
            "onUpdate:open": (N) => T(O.key)
          }, {
            title: lt(() => [
              $("b", null, k(O.name || z(X)("(new connection)")), 1),
              $("span", fc, k(O.provider), 1)
            ]),
            actions: lt(() => [
              _e(tc, {
                onClick: (N) => R(be)
              }, null, 8, ["onClick"])
            ]),
            body: lt(() => [
              $("div", uc, [
                $("span", ac, k(z(X)("Name")), 1),
                Pe($("input", {
                  "onUpdate:modelValue": (N) => O.name = N,
                  class: "w-140",
                  spellcheck: "false"
                }, null, 8, dc), [
                  [rt, O.name]
                ]),
                $("span", hc, k(z(X)("Provider")), 1),
                Pe($("select", {
                  "onUpdate:modelValue": (N) => O.provider = N
                }, [
                  $("option", gc, k(z(X)("mssql")), 1),
                  $("option", mc, k(z(X)("postgres")), 1),
                  $("option", _c, k(z(X)("sqlite")), 1)
                ], 8, pc), [
                  [Zs, O.provider]
                ])
              ]),
              O.provider !== "sqlite" ? (Y(), ne("div", bc, [
                $("span", yc, k(z(X)("Server")), 1),
                Pe($("input", {
                  "onUpdate:modelValue": (N) => O.path = N,
                  placeholder: z(X)("sql01 or 192.168.1.10"),
                  class: "w-220",
                  spellcheck: "false"
                }, null, 8, vc), [
                  [rt, O.path]
                ]),
                $("span", xc, k(z(X)("Database")), 1),
                Pe($("input", {
                  "onUpdate:modelValue": (N) => O.database = N,
                  class: "w-160",
                  spellcheck: "false"
                }, null, 8, Sc), [
                  [rt, O.database]
                ])
              ])) : (Y(), ne("div", wc, [
                $("span", Cc, k(z(X)("File")), 1),
                Pe($("input", {
                  "onUpdate:modelValue": (N) => O.path = N,
                  placeholder: z(X)("C:\\data\\mydb.sqlite"),
                  class: "w-400",
                  spellcheck: "false"
                }, null, 8, Tc), [
                  [rt, O.path]
                ])
              ])),
              O.provider !== "sqlite" ? (Y(), ne(le, { key: 2 }, [
                $("div", Oc, [
                  O.provider === "mssql" ? (Y(), ne("label", Ec, [
                    Pe($("input", {
                      type: "checkbox",
                      "onUpdate:modelValue": (N) => O.trustedConnection = N
                    }, null, 8, Ac), [
                      [Ml, O.trustedConnection]
                    ]),
                    $("span", null, k(z(X)("Windows Auth (domain)")), 1)
                  ])) : Be("", !0)
                ]),
                !O.trustedConnection || O.provider !== "mssql" ? (Y(), ne(le, { key: 0 }, [
                  $("div", Pc, [
                    $("span", Mc, k(z(X)("Credential")), 1),
                    _e(Kl, {
                      api: e.api,
                      modelValue: O.credential,
                      "onUpdate:modelValue": (N) => O.credential = N
                    }, null, 8, ["api", "modelValue", "onUpdate:modelValue"])
                  ]),
                  $("div", Ic, [
                    $("span", Rc, k(z(X)("User")), 1),
                    Pe($("input", {
                      "onUpdate:modelValue": (N) => O.user = N,
                      placeholder: O.credential ? "(from credential)" : "login",
                      class: "w-130",
                      spellcheck: "false"
                    }, null, 8, $c), [
                      [rt, O.user]
                    ])
                  ])
                ], 64)) : Be("", !0)
              ], 64)) : Be("", !0),
              $("div", Fc, [
                $("span", Vc, k(z(X)("Description")), 1),
                Pe($("input", {
                  "onUpdate:modelValue": (N) => O.description = N,
                  placeholder: z(X)("Shown to the AI — what this database contains"),
                  class: "grow"
                }, null, 8, Dc), [
                  [rt, O.description]
                ])
              ]),
              $("div", Nc, [
                $("button", {
                  type: "button",
                  disabled: O.testing,
                  onClick: (N) => Z(O)
                }, k(z(X)("Test Connection")), 9, jc),
                $("span", Uc, k(O.testStatus), 1)
              ])
            ]),
            _: 2
          }, 1032, ["open", "summary", "onUpdate:open"]))), 128))
        ]),
        _: 1
      }, 8, ["empty", "empty-text", "add-label"])
    ]));
  }
}), Lc = /* @__PURE__ */ ur(Hc, [["__scopeId", "data-v-9c1f470c"]]);
function Bc(e, t) {
  var i;
  Ul((i = t.t) == null ? void 0 : i.bind(t));
  let n = Dl(Lc, { api: t });
  const s = n.mount(e);
  return {
    save: () => s.toJson(),
    destroy: () => {
      n == null || n.unmount(), n = null;
    }
  };
}
export {
  Bc as mount
};
