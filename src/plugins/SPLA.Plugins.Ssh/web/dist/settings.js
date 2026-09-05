(function(){"use strict";try{if(typeof document<"u"){var a=document.createElement("style");a.appendChild(document.createTextNode(".cred-slot[data-v-17c11f13]{min-width:0;flex:1}.w-260[data-v-17c11f13]{width:260px}.list-panel{display:flex;flex-direction:column;gap:var(--gap, 10px);--card-bd: 1px solid var(--border, #444);--card-bg: var(--panel, transparent);--card-r: var(--radius, 6px)}.list-empty{color:var(--muted, #888);font-size:var(--fs-sm, 12px);padding:4px 0;font-style:italic}.list-card{border:var(--card-bd);border-radius:var(--card-r);background:var(--card-bg)}.list-card-head{display:flex;align-items:center;gap:var(--gap, 10px);padding:var(--pad, 12px);cursor:pointer}.list-card-head{font-weight:600}.list-card-title{display:flex;align-items:center;gap:6px;min-width:0}.list-card-summary{font-weight:400;color:var(--muted, #888);font-size:var(--fs-sm, 12px);white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.list-card-actions{display:flex;align-items:center;gap:4px;font-weight:600;margin-left:auto}.list-card-head>.gbtn:first-child{margin-left:-3px}.list-card-body{padding:0 var(--pad, 12px) var(--pad, 12px);display:flex;flex-direction:column;gap:var(--gap, 10px)}.list-panel.flat{gap:0;--card-bd: 0;--card-bg: transparent;--card-r: 0}.list-panel.flat>.list-card+.list-card{border-top:1px solid var(--border, #444)}.list-panel.flat>.list-add{margin-top:var(--gap, 10px)}.list-add{align-self:flex-start;background:transparent;color:var(--muted, #888);border:1px solid var(--border, #444);border-radius:var(--radius-sm, 5px);padding:6px 12px;font-weight:600;font-size:inherit;font-family:inherit;cursor:pointer}.list-add:hover:not(:disabled){opacity:.9}.list-add:disabled{opacity:.5;cursor:default}.list-add-glyph{margin-right:5px;opacity:.8}.gbtn{display:inline-flex;align-items:center;justify-content:center;gap:3px;background:transparent;border:1px solid transparent;color:var(--muted, #888);border-radius:var(--radius-sm, 5px);padding:2px 5px;min-width:1.7em;height:1.7em;font:inherit;line-height:1;cursor:pointer}.gbtn:hover:not(:disabled){color:var(--text, inherit);border-color:var(--border, #444);background:var(--elevated, transparent)}.gbtn:disabled{opacity:.45;cursor:default}.gbtn-danger:hover:not(:disabled){color:var(--danger, #e05555);border-color:var(--danger, #e05555);background:transparent}.gbtn.on{color:var(--accent, inherit)}.ssh-set[data-v-595f0b9f]{display:flex;flex-direction:column;gap:var(--gap, 10px);font-size:var(--fs-sm, 12px);color:var(--text, inherit)}.muted[data-v-595f0b9f]{color:var(--muted, #888)}.row[data-v-595f0b9f]{display:flex;gap:8px;align-items:center;flex-wrap:wrap}.grow[data-v-595f0b9f]{flex:1}.w-label[data-v-595f0b9f]{width:80px}.w-70[data-v-595f0b9f]{width:70px}.w-120[data-v-595f0b9f]{width:120px}.w-180[data-v-595f0b9f]{width:180px}.w-260[data-v-595f0b9f]{width:260px}.chk[data-v-595f0b9f]{cursor:pointer}.chk input[data-v-595f0b9f]{height:auto}label[data-v-595f0b9f]{display:flex;gap:6px;align-items:center}input[data-v-595f0b9f],select[data-v-595f0b9f]{height:24px;padding:2px 6px;color:var(--text, inherit);background:var(--bg, transparent);border:1px solid var(--border, #444);border-radius:var(--radius-sm, 5px);font-family:inherit;font-size:inherit}button[data-v-595f0b9f]:not(.gbtn){padding:2px 10px;color:var(--text, inherit);background:var(--panel, transparent);border:1px solid var(--border, #444);border-radius:var(--radius-sm, 5px);cursor:pointer;font-size:inherit}button[data-v-595f0b9f]:not(.gbtn):hover:not(:disabled){border-color:var(--muted, #888)}button[data-v-595f0b9f]:not(.gbtn):disabled{opacity:.5;cursor:default}")),document.head.appendChild(a)}}catch(r){console.error("vite-plugin-css-injected-by-js",r)}})();
/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Xs(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const k = {}, dt = [], Fe = () => {
}, ti = () => !1, gs = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), ms = (e) => e.startsWith("onUpdate:"), ie = Object.assign, Zs = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, pr = Object.prototype.hasOwnProperty, N = (e, t) => pr.call(e, t), M = Array.isArray, pt = (e) => Wt(e) === "[object Map]", St = (e) => Wt(e) === "[object Set]", xn = (e) => Wt(e) === "[object Date]", $ = (e) => typeof e == "function", X = (e) => typeof e == "string", xe = (e) => typeof e == "symbol", L = (e) => e !== null && typeof e == "object", si = (e) => (L(e) || $(e)) && $(e.then) && $(e.catch), ni = Object.prototype.toString, Wt = (e) => ni.call(e), hr = (e) => Wt(e).slice(8, -1), ii = (e) => Wt(e) === "[object Object]", Qs = (e) => X(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, Rt = /* @__PURE__ */ Xs(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), _s = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, gr = /-\w/g, ye = _s(
  (e) => e.replace(gr, (t) => t.slice(1).toUpperCase())
), mr = /\B([A-Z])/g, ft = _s(
  (e) => e.replace(mr, "-$1").toLowerCase()
), ri = _s((e) => e.charAt(0).toUpperCase() + e.slice(1)), As = _s(
  (e) => e ? `on${ri(e)}` : ""
), $e = (e, t) => !Object.is(e, t), ns = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, oi = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, bs = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let Sn;
const ys = () => Sn || (Sn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function en(e) {
  if (M(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], i = X(n) ? vr(n) : en(n);
      if (i)
        for (const r in i)
          t[r] = i[r];
    }
    return t;
  } else if (X(e) || L(e))
    return e;
}
const _r = /;(?![^(]*\))/g, br = /:([^]+)/, yr = /\/\*[^]*?\*\//g;
function vr(e) {
  const t = {};
  return e.replace(yr, "").split(_r).forEach((s) => {
    if (s) {
      const n = s.split(br);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function Jt(e) {
  let t = "";
  if (X(e))
    t = e;
  else if (M(e))
    for (let s = 0; s < e.length; s++) {
      const n = Jt(e[s]);
      n && (t += n + " ");
    }
  else if (L(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const xr = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", Sr = /* @__PURE__ */ Xs(xr);
function li(e) {
  return !!e || e === "";
}
function wr(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = wt(e[n], t[n]);
  return s;
}
function wt(e, t) {
  if (e === t) return !0;
  let s = xn(e), n = xn(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = xe(e), n = xe(t), s || n)
    return e === t;
  if (s = M(e), n = M(t), s || n)
    return s && n ? wr(e, t) : !1;
  if (s = L(e), n = L(t), s || n) {
    if (!s || !n)
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
function tn(e, t) {
  return e.findIndex((s) => wt(s, t));
}
const ci = (e) => !!(e && e.__v_isRef === !0), z = (e) => X(e) ? e : e == null ? "" : M(e) || L(e) && (e.toString === ni || !$(e.toString)) ? ci(e) ? z(e.value) : JSON.stringify(e, fi, 2) : String(e), fi = (e, t) => ci(t) ? fi(e, t.value) : pt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, i], r) => (s[Ps(n, r) + " =>"] = i, s),
    {}
  )
} : St(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => Ps(s))
} : xe(t) ? Ps(t) : L(t) && !M(t) && !ii(t) ? String(t) : t, Ps = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    xe(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let ne;
class Cr {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && ne && (ne.active ? (this.parent = ne, this.index = (ne.scopes || (ne.scopes = [])).push(
      this
    ) - 1) : (this._active = !1, this._warnOnRun = !1));
  }
  get active() {
    return this._active;
  }
  pause() {
    if (this._active) {
      this._isPaused = !0;
      let t, s;
      if (this.scopes)
        for (t = 0, s = this.scopes.length; t < s; t++)
          this.scopes[t].pause();
      for (t = 0, s = this.effects.length; t < s; t++)
        this.effects[t].pause();
    }
  }
  /**
   * Resumes the effect scope, including all child scopes and effects.
   */
  resume() {
    if (this._active && this._isPaused) {
      this._isPaused = !1;
      let t, s;
      if (this.scopes)
        for (t = 0, s = this.scopes.length; t < s; t++)
          this.scopes[t].resume();
      for (t = 0, s = this.effects.length; t < s; t++)
        this.effects[t].resume();
    }
  }
  run(t) {
    if (this._active) {
      const s = ne;
      try {
        return ne = this, t();
      } finally {
        ne = s;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = ne, ne = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (ne === this)
        ne = this.prevScope;
      else {
        let t = ne;
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
      let s, n;
      for (s = 0, n = this.effects.length; s < n; s++)
        this.effects[s].stop();
      for (this.effects.length = 0, s = 0, n = this.cleanups.length; s < n; s++)
        this.cleanups[s]();
      if (this.cleanups.length = 0, this.scopes) {
        for (s = 0, n = this.scopes.length; s < n; s++)
          this.scopes[s].stop(!0);
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
function Tr() {
  return ne;
}
let J;
const Ms = /* @__PURE__ */ new WeakSet();
class ui {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, ne && (ne.active ? ne.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, Ms.has(this) && (Ms.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || di(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, wn(this), pi(this);
    const t = J, s = ve;
    J = this, ve = !0;
    try {
      return this.fn();
    } finally {
      hi(this), J = t, ve = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        rn(t);
      this.deps = this.depsTail = void 0, wn(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? Ms.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    js(this) && this.run();
  }
  get dirty() {
    return js(this);
  }
}
let ai = 0, $t, Ft;
function di(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Ft, Ft = e;
    return;
  }
  e.next = $t, $t = e;
}
function sn() {
  ai++;
}
function nn() {
  if (--ai > 0)
    return;
  if (Ft) {
    let t = Ft;
    for (Ft = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; $t; ) {
    let t = $t;
    for ($t = void 0; t; ) {
      const s = t.next;
      if (t.next = void 0, t.flags &= -9, t.flags & 1)
        try {
          t.trigger();
        } catch (n) {
          e || (e = n);
        }
      t = s;
    }
  }
  if (e) throw e;
}
function pi(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function hi(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const i = n.prevDep;
    n.version === -1 ? (n === s && (s = i), rn(n), Or(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = i;
  }
  e.deps = t, e.depsTail = s;
}
function js(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (gi(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function gi(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Nt) || (e.globalVersion = Nt, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !js(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = J, n = ve;
  J = e, ve = !0;
  try {
    pi(e);
    const i = e.fn(e._value);
    (t.version === 0 || $e(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    J = s, ve = n, hi(e), e.flags &= -3;
  }
}
function rn(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: i } = e;
  if (n && (n.nextSub = i, e.prevSub = void 0), i && (i.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let r = s.computed.deps; r; r = r.nextDep)
      rn(r, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function Or(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let ve = !0;
const mi = [];
function Ve() {
  mi.push(ve), ve = !1;
}
function De() {
  const e = mi.pop();
  ve = e === void 0 ? !0 : e;
}
function wn(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const s = J;
    J = void 0;
    try {
      t();
    } finally {
      J = s;
    }
  }
}
let Nt = 0;
class Er {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class on {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!J || !ve || J === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== J)
      s = this.activeLink = new Er(J, this), J.deps ? (s.prevDep = J.depsTail, J.depsTail.nextDep = s, J.depsTail = s) : J.deps = J.depsTail = s, _i(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = J.depsTail, s.nextDep = void 0, J.depsTail.nextDep = s, J.depsTail = s, J.deps === s && (J.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, Nt++, this.notify(t);
  }
  notify(t) {
    sn();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      nn();
    }
  }
}
function _i(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let n = t.deps; n; n = n.nextDep)
        _i(n);
    }
    const s = e.dep.subs;
    s !== e && (e.prevSub = s, s && (s.nextSub = e)), e.dep.subs = e;
  }
}
const Us = /* @__PURE__ */ new WeakMap(), ot = /* @__PURE__ */ Symbol(
  ""
), Ls = /* @__PURE__ */ Symbol(
  ""
), jt = /* @__PURE__ */ Symbol(
  ""
);
function re(e, t, s) {
  if (ve && J) {
    let n = Us.get(e);
    n || Us.set(e, n = /* @__PURE__ */ new Map());
    let i = n.get(s);
    i || (n.set(s, i = new on()), i.map = n, i.key = s), i.track();
  }
}
function Le(e, t, s, n, i, r) {
  const o = Us.get(e);
  if (!o) {
    Nt++;
    return;
  }
  const l = (c) => {
    c && c.trigger();
  };
  if (sn(), t === "clear")
    o.forEach(l);
  else {
    const c = M(e), d = c && Qs(s);
    if (c && s === "length") {
      const a = Number(n);
      o.forEach((h, T) => {
        (T === "length" || T === jt || !xe(T) && T >= a) && l(h);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(jt)), t) {
        case "add":
          c ? d && l(o.get("length")) : (l(o.get(ot)), pt(e) && l(o.get(Ls)));
          break;
        case "delete":
          c || (l(o.get(ot)), pt(e) && l(o.get(Ls)));
          break;
        case "set":
          pt(e) && l(o.get(ot));
          break;
      }
  }
  nn();
}
function ut(e) {
  const t = /* @__PURE__ */ H(e);
  return t === e ? t : (re(t, "iterate", jt), /* @__PURE__ */ be(e) ? t : t.map(Se));
}
function vs(e) {
  return re(e = /* @__PURE__ */ H(e), "iterate", jt), e;
}
function Ie(e, t) {
  return /* @__PURE__ */ Be(e) ? yt(/* @__PURE__ */ lt(e) ? Se(t) : t) : Se(t);
}
const Ar = {
  __proto__: null,
  [Symbol.iterator]() {
    return Is(this, Symbol.iterator, (e) => Ie(this, e));
  },
  concat(...e) {
    return ut(this).concat(
      ...e.map((t) => M(t) ? ut(t) : t)
    );
  },
  entries() {
    return Is(this, "entries", (e) => (e[1] = Ie(this, e[1]), e));
  },
  every(e, t) {
    return He(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return He(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => Ie(this, n)),
      arguments
    );
  },
  find(e, t) {
    return He(
      this,
      "find",
      e,
      t,
      (s) => Ie(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return He(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return He(
      this,
      "findLast",
      e,
      t,
      (s) => Ie(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return He(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return He(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return Rs(this, "includes", e);
  },
  indexOf(...e) {
    return Rs(this, "indexOf", e);
  },
  join(e) {
    return ut(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return Rs(this, "lastIndexOf", e);
  },
  map(e, t) {
    return He(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return At(this, "pop");
  },
  push(...e) {
    return At(this, "push", e);
  },
  reduce(e, ...t) {
    return Cn(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return Cn(this, "reduceRight", e, t);
  },
  shift() {
    return At(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return He(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return At(this, "splice", e);
  },
  toReversed() {
    return ut(this).toReversed();
  },
  toSorted(e) {
    return ut(this).toSorted(e);
  },
  toSpliced(...e) {
    return ut(this).toSpliced(...e);
  },
  unshift(...e) {
    return At(this, "unshift", e);
  },
  values() {
    return Is(this, "values", (e) => Ie(this, e));
  }
};
function Is(e, t, s) {
  const n = vs(e), i = n[t]();
  return n !== e && !/* @__PURE__ */ be(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = s(r.value)), r;
  }), i;
}
const Pr = Array.prototype;
function He(e, t, s, n, i, r) {
  const o = vs(e), l = o !== e && !/* @__PURE__ */ be(e), c = o[t];
  if (c !== Pr[t]) {
    const h = c.apply(e, r);
    return l ? Se(h) : h;
  }
  let d = s;
  o !== e && (l ? d = function(h, T) {
    return s.call(this, Ie(e, h), T, e);
  } : s.length > 2 && (d = function(h, T) {
    return s.call(this, h, T, e);
  }));
  const a = c.call(o, d, n);
  return l && i ? i(a) : a;
}
function Cn(e, t, s, n) {
  const i = vs(e), r = i !== e && !/* @__PURE__ */ be(e);
  let o = s, l = !1;
  i !== e && (r ? (l = n.length === 0, o = function(d, a, h) {
    return l && (l = !1, d = Ie(e, d)), s.call(this, d, Ie(e, a), h, e);
  }) : s.length > 3 && (o = function(d, a, h) {
    return s.call(this, d, a, h, e);
  }));
  const c = i[t](o, ...n);
  return l ? Ie(e, c) : c;
}
function Rs(e, t, s) {
  const n = /* @__PURE__ */ H(e);
  re(n, "iterate", jt);
  const i = n[t](...s);
  return (i === -1 || i === !1) && /* @__PURE__ */ fn(s[0]) ? (s[0] = /* @__PURE__ */ H(s[0]), n[t](...s)) : i;
}
function At(e, t, s = []) {
  Ve(), sn();
  const n = (/* @__PURE__ */ H(e))[t].apply(e, s);
  return nn(), De(), n;
}
const Mr = /* @__PURE__ */ Xs("__proto__,__v_isRef,__isVue"), bi = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(xe)
);
function Ir(e) {
  xe(e) || (e = String(e));
  const t = /* @__PURE__ */ H(this);
  return re(t, "has", e), t.hasOwnProperty(e);
}
class yi {
  constructor(t = !1, s = !1) {
    this._isReadonly = t, this._isShallow = s;
  }
  get(t, s, n) {
    if (s === "__v_skip") return t.__v_skip;
    const i = this._isReadonly, r = this._isShallow;
    if (s === "__v_isReactive")
      return !i;
    if (s === "__v_isReadonly")
      return i;
    if (s === "__v_isShallow")
      return r;
    if (s === "__v_raw")
      return n === (i ? r ? Lr : wi : r ? Si : xi).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = M(t);
    if (!i) {
      let c;
      if (o && (c = Ar[s]))
        return c;
      if (s === "hasOwnProperty")
        return Ir;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ le(t) ? t : n
    );
    if ((xe(s) ? bi.has(s) : Mr(s)) || (i || re(t, "get", s), r))
      return l;
    if (/* @__PURE__ */ le(l)) {
      const c = o && Qs(s) ? l : l.value;
      return i && L(c) ? /* @__PURE__ */ ks(c) : c;
    }
    return L(l) ? i ? /* @__PURE__ */ ks(l) : /* @__PURE__ */ Ut(l) : l;
  }
}
class vi extends yi {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, i) {
    let r = t[s];
    const o = M(t) && Qs(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ Be(r);
      if (!/* @__PURE__ */ be(n) && !/* @__PURE__ */ Be(n) && (r = /* @__PURE__ */ H(r), n = /* @__PURE__ */ H(n)), !o && /* @__PURE__ */ le(r) && !/* @__PURE__ */ le(n))
        return d || (r.value = n), !0;
    }
    const l = o ? Number(s) < t.length : N(t, s), c = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ le(t) ? t : i
    );
    return t === /* @__PURE__ */ H(i) && c && (l ? $e(n, r) && Le(t, "set", s, n) : Le(t, "add", s, n)), c;
  }
  deleteProperty(t, s) {
    const n = N(t, s);
    t[s];
    const i = Reflect.deleteProperty(t, s);
    return i && n && Le(t, "delete", s, void 0), i;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!xe(s) || !bi.has(s)) && re(t, "has", s), n;
  }
  ownKeys(t) {
    return re(
      t,
      "iterate",
      M(t) ? "length" : ot
    ), Reflect.ownKeys(t);
  }
}
class Rr extends yi {
  constructor(t = !1) {
    super(!0, t);
  }
  set(t, s) {
    return !0;
  }
  deleteProperty(t, s) {
    return !0;
  }
}
const $r = /* @__PURE__ */ new vi(), Fr = /* @__PURE__ */ new Rr(), Vr = /* @__PURE__ */ new vi(!0);
const Ks = (e) => e, es = (e) => Reflect.getPrototypeOf(e);
function Dr(e, t, s) {
  return function(...n) {
    const i = this.__v_raw, r = /* @__PURE__ */ H(i), o = pt(r), l = e === "entries" || e === Symbol.iterator && o, c = e === "keys" && o, d = i[e](...n), a = s ? Ks : t ? yt : Se;
    return !t && re(
      r,
      "iterate",
      c ? Ls : ot
    ), ie(
      // inheriting all iterator properties
      Object.create(d),
      {
        // iterator protocol
        next() {
          const { value: h, done: T } = d.next();
          return T ? { value: h, done: T } : {
            value: l ? [a(h[0]), a(h[1])] : a(h),
            done: T
          };
        }
      }
    );
  };
}
function ts(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function Hr(e, t) {
  const s = {
    get(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ H(r), l = /* @__PURE__ */ H(i);
      e || ($e(i, l) && re(o, "get", i), re(o, "get", l));
      const { has: c } = es(o), d = t ? Ks : e ? yt : Se;
      if (c.call(o, i))
        return d(r.get(i));
      if (c.call(o, l))
        return d(r.get(l));
      r !== o && r.get(i);
    },
    get size() {
      const i = this.__v_raw;
      return !e && re(/* @__PURE__ */ H(i), "iterate", ot), i.size;
    },
    has(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ H(r), l = /* @__PURE__ */ H(i);
      return e || ($e(i, l) && re(o, "has", i), re(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, c = /* @__PURE__ */ H(l), d = t ? Ks : e ? yt : Se;
      return !e && re(c, "iterate", ot), l.forEach((a, h) => i.call(r, d(a), d(h), o));
    }
  };
  return ie(
    s,
    e ? {
      add: ts("add"),
      set: ts("set"),
      delete: ts("delete"),
      clear: ts("clear")
    } : {
      add(i) {
        const r = /* @__PURE__ */ H(this), o = es(r), l = /* @__PURE__ */ H(i), c = !t && !/* @__PURE__ */ be(i) && !/* @__PURE__ */ Be(i) ? l : i;
        return o.has.call(r, c) || $e(i, c) && o.has.call(r, i) || $e(l, c) && o.has.call(r, l) || (r.add(c), Le(r, "add", c, c)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ be(r) && !/* @__PURE__ */ Be(r) && (r = /* @__PURE__ */ H(r));
        const o = /* @__PURE__ */ H(this), { has: l, get: c } = es(o);
        let d = l.call(o, i);
        d || (i = /* @__PURE__ */ H(i), d = l.call(o, i));
        const a = c.call(o, i);
        return o.set(i, r), d ? $e(r, a) && Le(o, "set", i, r) : Le(o, "add", i, r), this;
      },
      delete(i) {
        const r = /* @__PURE__ */ H(this), { has: o, get: l } = es(r);
        let c = o.call(r, i);
        c || (i = /* @__PURE__ */ H(i), c = o.call(r, i)), l && l.call(r, i);
        const d = r.delete(i);
        return c && Le(r, "delete", i, void 0), d;
      },
      clear() {
        const i = /* @__PURE__ */ H(this), r = i.size !== 0, o = i.clear();
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
    s[i] = Dr(i, e, t);
  }), s;
}
function ln(e, t) {
  const s = Hr(e, t);
  return (n, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? n : Reflect.get(
    N(s, i) && i in n ? s : n,
    i,
    r
  );
}
const Nr = {
  get: /* @__PURE__ */ ln(!1, !1)
}, jr = {
  get: /* @__PURE__ */ ln(!1, !0)
}, Ur = {
  get: /* @__PURE__ */ ln(!0, !1)
};
const xi = /* @__PURE__ */ new WeakMap(), Si = /* @__PURE__ */ new WeakMap(), wi = /* @__PURE__ */ new WeakMap(), Lr = /* @__PURE__ */ new WeakMap();
function Kr(e) {
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
function Ut(e) {
  return /* @__PURE__ */ Be(e) ? e : cn(
    e,
    !1,
    $r,
    Nr,
    xi
  );
}
// @__NO_SIDE_EFFECTS__
function kr(e) {
  return cn(
    e,
    !1,
    Vr,
    jr,
    Si
  );
}
// @__NO_SIDE_EFFECTS__
function ks(e) {
  return cn(
    e,
    !0,
    Fr,
    Ur,
    wi
  );
}
function cn(e, t, s, n, i) {
  if (!L(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const r = i.get(e);
  if (r)
    return r;
  const o = Kr(hr(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? n : s
  );
  return i.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function lt(e) {
  return /* @__PURE__ */ Be(e) ? /* @__PURE__ */ lt(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function Be(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function be(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function fn(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function H(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ H(t) : e;
}
function Br(e) {
  return !N(e, "__v_skip") && Object.isExtensible(e) && oi(e, "__v_skip", !0), e;
}
const Se = (e) => L(e) ? /* @__PURE__ */ Ut(e) : e, yt = (e) => L(e) ? /* @__PURE__ */ ks(e) : e;
// @__NO_SIDE_EFFECTS__
function le(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function ls(e) {
  return Wr(e, !1);
}
function Wr(e, t) {
  return /* @__PURE__ */ le(e) ? e : new Jr(e, t);
}
class Jr {
  constructor(t, s) {
    this.dep = new on(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ H(t), this._value = s ? t : Se(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ be(t) || /* @__PURE__ */ Be(t);
    t = n ? t : /* @__PURE__ */ H(t), $e(t, s) && (this._rawValue = t, this._value = n ? t : Se(t), this.dep.trigger());
  }
}
function Y(e) {
  return /* @__PURE__ */ le(e) ? e.value : e;
}
const qr = {
  get: (e, t, s) => t === "__v_raw" ? e : Y(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const i = e[t];
    return /* @__PURE__ */ le(i) && !/* @__PURE__ */ le(s) ? (i.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function Ci(e) {
  return /* @__PURE__ */ lt(e) ? e : new Proxy(e, qr);
}
class Gr {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new on(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Nt - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    J !== this)
      return di(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return gi(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function Yr(e, t, s = !1) {
  let n, i;
  return $(e) ? n = e : (n = e.get, i = e.set), new Gr(n, i, s);
}
const ss = {}, cs = /* @__PURE__ */ new WeakMap();
let rt;
function zr(e, t = !1, s = rt) {
  if (s) {
    let n = cs.get(s);
    n || cs.set(s, n = []), n.push(e);
  }
}
function Xr(e, t, s = k) {
  const { immediate: n, deep: i, once: r, scheduler: o, augmentJob: l, call: c } = s, d = (y) => i ? y : /* @__PURE__ */ be(y) || i === !1 || i === 0 ? Ke(y, 1) : Ke(y);
  let a, h, T, E, D = !1, R = !1;
  if (/* @__PURE__ */ le(e) ? (h = () => e.value, D = /* @__PURE__ */ be(e)) : /* @__PURE__ */ lt(e) ? (h = () => d(e), D = !0) : M(e) ? (R = !0, D = e.some((y) => /* @__PURE__ */ lt(y) || /* @__PURE__ */ be(y)), h = () => e.map((y) => {
    if (/* @__PURE__ */ le(y))
      return y.value;
    if (/* @__PURE__ */ lt(y))
      return d(y);
    if ($(y))
      return c ? c(y, 2) : y();
  })) : $(e) ? t ? h = c ? () => c(e, 2) : e : h = () => {
    if (T) {
      Ve();
      try {
        T();
      } finally {
        De();
      }
    }
    const y = rt;
    rt = a;
    try {
      return c ? c(e, 3, [E]) : e(E);
    } finally {
      rt = y;
    }
  } : h = Fe, t && i) {
    const y = h, Z = i === !0 ? 1 / 0 : i;
    h = () => Ke(y(), Z);
  }
  const G = Tr(), B = () => {
    a.stop(), G && G.active && Zs(G.effects, a);
  };
  if (r && t) {
    const y = t;
    t = (...Z) => {
      const j = y(...Z);
      return B(), j;
    };
  }
  let v = R ? new Array(e.length).fill(ss) : ss;
  const O = (y) => {
    if (!(!(a.flags & 1) || !a.dirty && !y))
      if (t) {
        const Z = a.run();
        if (y || i || D || (R ? Z.some((j, Ce) => $e(j, v[Ce])) : $e(Z, v))) {
          T && T();
          const j = rt;
          rt = a;
          try {
            const Ce = [
              Z,
              // pass undefined as the old value when it's changed for the first time
              v === ss ? void 0 : R && v[0] === ss ? [] : v,
              E
            ];
            v = Z, c ? c(t, 3, Ce) : (
              // @ts-expect-error
              t(...Ce)
            );
          } finally {
            rt = j;
          }
        }
      } else
        a.run();
  };
  return l && l(O), a = new ui(h), a.scheduler = o ? () => o(O, !1) : O, E = (y) => zr(y, !1, a), T = a.onStop = () => {
    const y = cs.get(a);
    if (y) {
      if (c)
        c(y, 4);
      else
        for (const Z of y) Z();
      cs.delete(a);
    }
  }, t ? n ? O(!0) : v = a.run() : o ? o(O.bind(null, !0), !0) : a.run(), B.pause = a.pause.bind(a), B.resume = a.resume.bind(a), B.stop = B, B;
}
function Ke(e, t = 1 / 0, s) {
  if (t <= 0 || !L(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ le(e))
    Ke(e.value, t, s);
  else if (M(e))
    for (let n = 0; n < e.length; n++)
      Ke(e[n], t, s);
  else if (St(e) || pt(e))
    e.forEach((n) => {
      Ke(n, t, s);
    });
  else if (ii(e)) {
    for (const n in e)
      Ke(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && Ke(e[n], t, s);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function qt(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (i) {
    xs(i, t, s);
  }
}
function we(e, t, s, n) {
  if ($(e)) {
    const i = qt(e, t, s, n);
    return i && si(i) && i.catch((r) => {
      xs(r, t, s);
    }), i;
  }
  if (M(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(we(e[r], t, s, n));
    return i;
  }
}
function xs(e, t, s, n = !0) {
  const i = t ? t.vnode : null, { errorHandler: r, throwUnhandledErrorInProduction: o } = t && t.appContext.config || k;
  if (t) {
    let l = t.parent;
    const c = t.proxy, d = `https://vuejs.org/error-reference/#runtime-${s}`;
    for (; l; ) {
      const a = l.ec;
      if (a) {
        for (let h = 0; h < a.length; h++)
          if (a[h](e, c, d) === !1)
            return;
      }
      l = l.parent;
    }
    if (r) {
      Ve(), qt(r, null, 10, [
        e,
        c,
        d
      ]), De();
      return;
    }
  }
  Zr(e, s, i, n, o);
}
function Zr(e, t, s, n = !0, i = !1) {
  if (i)
    throw e;
  console.error(e);
}
const ue = [];
let Me = -1;
const ht = [];
let Ge = null, at = 0;
const Ti = /* @__PURE__ */ Promise.resolve();
let fs = null;
function Oi(e) {
  const t = fs || Ti;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Qr(e) {
  let t = Me + 1, s = ue.length;
  for (; t < s; ) {
    const n = t + s >>> 1, i = ue[n], r = Lt(i);
    r < e || r === e && i.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function un(e) {
  if (!(e.flags & 1)) {
    const t = Lt(e), s = ue[ue.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Lt(s) ? ue.push(e) : ue.splice(Qr(t), 0, e), e.flags |= 1, Ei();
  }
}
function Ei() {
  fs || (fs = Ti.then(Pi));
}
function eo(e) {
  M(e) ? ht.push(...e) : Ge && e.id === -1 ? Ge.splice(at + 1, 0, e) : e.flags & 1 || (ht.push(e), e.flags |= 1), Ei();
}
function Tn(e, t, s = Me + 1) {
  for (; s < ue.length; s++) {
    const n = ue[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      ue.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function Ai(e) {
  if (ht.length) {
    const t = [...new Set(ht)].sort(
      (s, n) => Lt(s) - Lt(n)
    );
    if (ht.length = 0, Ge) {
      Ge.push(...t);
      return;
    }
    for (Ge = t, at = 0; at < Ge.length; at++) {
      const s = Ge[at];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    Ge = null, at = 0;
  }
}
const Lt = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function Pi(e) {
  try {
    for (Me = 0; Me < ue.length; Me++) {
      const t = ue[Me];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), qt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Me < ue.length; Me++) {
      const t = ue[Me];
      t && (t.flags &= -2);
    }
    Me = -1, ue.length = 0, Ai(), fs = null, (ue.length || ht.length) && Pi();
  }
}
let oe = null, Mi = null;
function us(e) {
  const t = oe;
  return oe = e, Mi = e && e.type.__scopeId || null, t;
}
function gt(e, t = oe, s) {
  if (!t || e._n)
    return e;
  const n = (...i) => {
    n._d && Hn(-1);
    const r = us(t);
    let o;
    try {
      o = e(...i);
    } finally {
      us(r), n._d && Hn(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function Ne(e, t) {
  if (oe === null)
    return e;
  const s = Ts(oe), n = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [r, o, l, c = k] = t[i];
    r && ($(r) && (r = {
      mounted: r,
      updated: r
    }), r.deep && Ke(o), n.push({
      dir: r,
      instance: s,
      value: o,
      oldValue: void 0,
      arg: l,
      modifiers: c
    }));
  }
  return e;
}
function st(e, t, s, n) {
  const i = e.dirs, r = t && t.dirs;
  for (let o = 0; o < i.length; o++) {
    const l = i[o];
    r && (l.oldValue = r[o].value);
    let c = l.dir[n];
    c && (Ve(), we(c, s, 8, [
      e.el,
      l,
      e,
      t
    ]), De());
  }
}
function to(e, t) {
  if (ae) {
    let s = ae.provides;
    const n = ae.parent && ae.parent.provides;
    n === s && (s = ae.provides = Object.create(n)), s[e] = t;
  }
}
function is(e, t, s = !1) {
  const n = Zo();
  if (n || _t) {
    let i = _t ? _t._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return s && $(t) ? t.call(n && n.proxy) : t;
  }
}
const so = /* @__PURE__ */ Symbol.for("v-scx"), no = () => is(so);
function rs(e, t, s) {
  return Ii(e, t, s);
}
function Ii(e, t, s = k) {
  const { immediate: n, deep: i, flush: r, once: o } = s, l = ie({}, s), c = t && n || !t && r !== "post";
  let d;
  if (kt) {
    if (r === "sync") {
      const E = no();
      d = E.__watcherHandles || (E.__watcherHandles = []);
    } else if (!c) {
      const E = () => {
      };
      return E.stop = Fe, E.resume = Fe, E.pause = Fe, E;
    }
  }
  const a = ae;
  l.call = (E, D, R) => we(E, a, D, R);
  let h = !1;
  r === "post" ? l.scheduler = (E) => {
    de(E, a && a.suspense);
  } : r !== "sync" && (h = !0, l.scheduler = (E, D) => {
    D ? E() : un(E);
  }), l.augmentJob = (E) => {
    t && (E.flags |= 4), h && (E.flags |= 2, a && (E.id = a.uid, E.i = a));
  };
  const T = Xr(e, t, l);
  return kt && (d ? d.push(T) : c && T()), T;
}
function io(e, t, s) {
  const n = this.proxy, i = X(e) ? e.includes(".") ? Ri(n, e) : () => n[e] : e.bind(n, n);
  let r;
  $(t) ? r = t : (r = t.handler, s = t);
  const o = Gt(this), l = Ii(i, r.bind(n), s);
  return o(), l;
}
function Ri(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let i = 0; i < s.length && n; i++)
      n = n[s[i]];
    return n;
  };
}
const ro = /* @__PURE__ */ Symbol("_vte"), oo = (e) => e.__isTeleport, $s = /* @__PURE__ */ Symbol("_leaveCb");
function an(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, an(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Ze(e, t) {
  return $(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    ie({ name: e.name }, t, { setup: e })
  ) : e;
}
function $i(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function On(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const as = /* @__PURE__ */ new WeakMap();
function Vt(e, t, s, n, i = !1) {
  if (M(e)) {
    e.forEach(
      (R, G) => Vt(
        R,
        t && (M(t) ? t[G] : t),
        s,
        n,
        i
      )
    );
    return;
  }
  if (mt(n) && !i) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Vt(e, t, s, n.component.subTree);
    return;
  }
  const r = n.shapeFlag & 4 ? Ts(n.component) : n.el, o = i ? null : r, { i: l, r: c } = e, d = t && t.r, a = l.refs === k ? l.refs = {} : l.refs, h = l.setupState, T = /* @__PURE__ */ H(h), E = h === k ? ti : (R) => On(a, R) ? !1 : N(T, R), D = (R, G) => !(G && On(a, G));
  if (d != null && d !== c) {
    if (En(t), X(d))
      a[d] = null, E(d) && (h[d] = null);
    else if (/* @__PURE__ */ le(d)) {
      const R = t;
      D(d, R.k) && (d.value = null), R.k && (a[R.k] = null);
    }
  }
  if ($(c)) {
    Ve();
    try {
      qt(c, l, 12, [o, a]);
    } finally {
      De();
    }
  } else {
    const R = X(c), G = /* @__PURE__ */ le(c);
    if (R || G) {
      const B = () => {
        if (e.f) {
          const v = R ? E(c) ? h[c] : a[c] : D() || !e.k ? c.value : a[e.k];
          if (i)
            M(v) && Zs(v, r);
          else if (M(v))
            v.includes(r) || v.push(r);
          else if (R)
            a[c] = [r], E(c) && (h[c] = a[c]);
          else {
            const O = [r];
            D(c, e.k) && (c.value = O), e.k && (a[e.k] = O);
          }
        } else R ? (a[c] = o, E(c) && (h[c] = o)) : G && (D(c, e.k) && (c.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const v = () => {
          B(), as.delete(e);
        };
        v.id = -1, as.set(e, v), de(v, s);
      } else
        En(e), B();
    }
  }
}
function En(e) {
  const t = as.get(e);
  t && (t.flags |= 8, as.delete(e));
}
ys().requestIdleCallback;
ys().cancelIdleCallback;
const mt = (e) => !!e.type.__asyncLoader, Fi = (e) => e.type.__isKeepAlive;
function lo(e, t) {
  Vi(e, "a", t);
}
function co(e, t) {
  Vi(e, "da", t);
}
function Vi(e, t, s = ae) {
  const n = e.__wdc || (e.__wdc = () => {
    let i = s;
    for (; i; ) {
      if (i.isDeactivated)
        return;
      i = i.parent;
    }
    return e();
  });
  if (Ss(t, n, s), s) {
    let i = s.parent;
    for (; i && i.parent; )
      Fi(i.parent.vnode) && fo(n, t, s, i), i = i.parent;
  }
}
function fo(e, t, s, n) {
  const i = Ss(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Ni(() => {
    Zs(n[t], i);
  }, s);
}
function Ss(e, t, s = ae, n = !1) {
  if (s) {
    const i = s[e] || (s[e] = []), r = t.__weh || (t.__weh = (...o) => {
      Ve();
      const l = Gt(s), c = we(t, s, e, o);
      return l(), De(), c;
    });
    return n ? i.unshift(r) : i.push(r), r;
  }
}
const Je = (e) => (t, s = ae) => {
  (!kt || e === "sp") && Ss(e, (...n) => t(...n), s);
}, uo = Je("bm"), Di = Je("m"), ao = Je(
  "bu"
), po = Je("u"), Hi = Je(
  "bum"
), Ni = Je("um"), ho = Je(
  "sp"
), go = Je("rtg"), mo = Je("rtc");
function _o(e, t = ae) {
  Ss("ec", e, t);
}
const bo = /* @__PURE__ */ Symbol.for("v-ndc");
function An(e, t, s, n) {
  let i;
  const r = s, o = M(e);
  if (o || X(e)) {
    const l = o && /* @__PURE__ */ lt(e);
    let c = !1, d = !1;
    l && (c = !/* @__PURE__ */ be(e), d = /* @__PURE__ */ Be(e), e = vs(e)), i = new Array(e.length);
    for (let a = 0, h = e.length; a < h; a++)
      i[a] = t(
        c ? d ? yt(Se(e[a])) : Se(e[a]) : e[a],
        a,
        void 0,
        r
      );
  } else if (typeof e == "number") {
    i = new Array(e);
    for (let l = 0; l < e; l++)
      i[l] = t(l + 1, l, void 0, r);
  } else if (L(e))
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
function ze(e, t, s = {}, n, i) {
  if (oe.ce || oe.parent && mt(oe.parent) && oe.parent.ce) {
    const d = Object.keys(s).length > 0;
    return t !== "default" && (s.name = t), ee(), Xe(
      pe,
      null,
      [_e("slot", s, n && n())],
      d ? -2 : 64
    );
  }
  let r = e[t];
  r && r._c && (r._d = !1), ee();
  const o = r && ji(r(s)), l = s.key || // slot content array of a dynamic conditional slot may have a branch
  // key attached in the `createSlots` helper, respect that
  o && o.key, c = Xe(
    pe,
    {
      key: (l && !xe(l) ? l : `_${t}`) + // #7256 force differentiate fallback content from actual content
      (!o && n ? "_fb" : "")
    },
    o || (n ? n() : []),
    o && e._ === 1 ? 64 : -2
  );
  return c.scopeId && (c.slotScopeIds = [c.scopeId + "-s"]), r && r._c && (r._d = !0), c;
}
function ji(e) {
  return e.some((t) => hn(t) ? !(t.type === We || t.type === pe && !ji(t.children)) : !0) ? e : null;
}
const Bs = (e) => e ? rr(e) ? Ts(e) : Bs(e.parent) : null, Dt = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ ie(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => e.props,
    $attrs: (e) => e.attrs,
    $slots: (e) => e.slots,
    $refs: (e) => e.refs,
    $parent: (e) => Bs(e.parent),
    $root: (e) => Bs(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Li(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      un(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = Oi.bind(e.proxy)),
    $watch: (e) => io.bind(e)
  })
), Fs = (e, t) => e !== k && !e.__isScriptSetup && N(e, t), yo = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: i, props: r, accessCache: o, type: l, appContext: c } = e;
    if (t[0] !== "$") {
      const T = o[t];
      if (T !== void 0)
        switch (T) {
          case 1:
            return n[t];
          case 2:
            return i[t];
          case 4:
            return s[t];
          case 3:
            return r[t];
        }
      else {
        if (Fs(n, t))
          return o[t] = 1, n[t];
        if (i !== k && N(i, t))
          return o[t] = 2, i[t];
        if (N(r, t))
          return o[t] = 3, r[t];
        if (s !== k && N(s, t))
          return o[t] = 4, s[t];
        Ws && (o[t] = 0);
      }
    }
    const d = Dt[t];
    let a, h;
    if (d)
      return t === "$attrs" && re(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== k && N(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      h = c.config.globalProperties, N(h, t)
    )
      return h[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: i, ctx: r } = e;
    return Fs(i, t) ? (i[t] = s, !0) : n !== k && N(n, t) ? (n[t] = s, !0) : N(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: i, props: r, type: o }
  }, l) {
    let c;
    return !!(s[l] || e !== k && l[0] !== "$" && N(e, l) || Fs(t, l) || N(r, l) || N(n, l) || N(Dt, l) || N(i.config.globalProperties, l) || (c = o.__cssModules) && c[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : N(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function Pn(e) {
  return M(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let Ws = !0;
function vo(e) {
  const t = Li(e), s = e.proxy, n = e.ctx;
  Ws = !1, t.beforeCreate && Mn(t.beforeCreate, e, "bc");
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
    beforeMount: h,
    mounted: T,
    beforeUpdate: E,
    updated: D,
    activated: R,
    deactivated: G,
    beforeDestroy: B,
    beforeUnmount: v,
    destroyed: O,
    unmounted: y,
    render: Z,
    renderTracked: j,
    renderTriggered: Ce,
    errorCaptured: qe,
    serverPrefetch: Yt,
    // public API
    expose: Qe,
    inheritAttrs: Ct,
    // assets
    components: zt,
    directives: Xt,
    filters: Os
  } = t;
  if (d && xo(d, n, null), o)
    for (const q in o) {
      const W = o[q];
      $(W) && (n[q] = W.bind(s));
    }
  if (i) {
    const q = i.call(s, s);
    L(q) && (e.data = /* @__PURE__ */ Ut(q));
  }
  if (Ws = !0, r)
    for (const q in r) {
      const W = r[q], et = $(W) ? W.bind(s, s) : $(W.get) ? W.get.bind(s, s) : Fe, Zt = !$(W) && $(W.set) ? W.set.bind(s) : Fe, tt = il({
        get: et,
        set: Zt
      });
      Object.defineProperty(n, q, {
        enumerable: !0,
        configurable: !0,
        get: () => tt.value,
        set: (Te) => tt.value = Te
      });
    }
  if (l)
    for (const q in l)
      Ui(l[q], n, s, q);
  if (c) {
    const q = $(c) ? c.call(s) : c;
    Reflect.ownKeys(q).forEach((W) => {
      to(W, q[W]);
    });
  }
  a && Mn(a, e, "c");
  function ce(q, W) {
    M(W) ? W.forEach((et) => q(et.bind(s))) : W && q(W.bind(s));
  }
  if (ce(uo, h), ce(Di, T), ce(ao, E), ce(po, D), ce(lo, R), ce(co, G), ce(_o, qe), ce(mo, j), ce(go, Ce), ce(Hi, v), ce(Ni, y), ce(ho, Yt), M(Qe))
    if (Qe.length) {
      const q = e.exposed || (e.exposed = {});
      Qe.forEach((W) => {
        Object.defineProperty(q, W, {
          get: () => s[W],
          set: (et) => s[W] = et,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  Z && e.render === Fe && (e.render = Z), Ct != null && (e.inheritAttrs = Ct), zt && (e.components = zt), Xt && (e.directives = Xt), Yt && $i(e);
}
function xo(e, t, s = Fe) {
  M(e) && (e = Js(e));
  for (const n in e) {
    const i = e[n];
    let r;
    L(i) ? "default" in i ? r = is(
      i.from || n,
      i.default,
      !0
    ) : r = is(i.from || n) : r = is(i), /* @__PURE__ */ le(r) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => r.value,
      set: (o) => r.value = o
    }) : t[n] = r;
  }
}
function Mn(e, t, s) {
  we(
    M(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Ui(e, t, s, n) {
  let i = n.includes(".") ? Ri(s, n) : () => s[n];
  if (X(e)) {
    const r = t[e];
    $(r) && rs(i, r);
  } else if ($(e))
    rs(i, e.bind(s));
  else if (L(e))
    if (M(e))
      e.forEach((r) => Ui(r, t, s, n));
    else {
      const r = $(e.handler) ? e.handler.bind(s) : t[e.handler];
      $(r) && rs(i, r, e);
    }
}
function Li(e) {
  const t = e.type, { mixins: s, extends: n } = t, {
    mixins: i,
    optionsCache: r,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = r.get(t);
  let c;
  return l ? c = l : !i.length && !s && !n ? c = t : (c = {}, i.length && i.forEach(
    (d) => ds(c, d, o, !0)
  ), ds(c, t, o)), L(t) && r.set(t, c), c;
}
function ds(e, t, s, n = !1) {
  const { mixins: i, extends: r } = t;
  r && ds(e, r, s, !0), i && i.forEach(
    (o) => ds(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = So[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const So = {
  data: In,
  props: Rn,
  emits: Rn,
  // objects
  methods: Mt,
  computed: Mt,
  // lifecycle
  beforeCreate: fe,
  created: fe,
  beforeMount: fe,
  mounted: fe,
  beforeUpdate: fe,
  updated: fe,
  beforeDestroy: fe,
  beforeUnmount: fe,
  destroyed: fe,
  unmounted: fe,
  activated: fe,
  deactivated: fe,
  errorCaptured: fe,
  serverPrefetch: fe,
  // assets
  components: Mt,
  directives: Mt,
  // watch
  watch: Co,
  // provide / inject
  provide: In,
  inject: wo
};
function In(e, t) {
  return t ? e ? function() {
    return ie(
      $(e) ? e.call(this, this) : e,
      $(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function wo(e, t) {
  return Mt(Js(e), Js(t));
}
function Js(e) {
  if (M(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++)
      t[e[s]] = e[s];
    return t;
  }
  return e;
}
function fe(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function Mt(e, t) {
  return e ? ie(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Rn(e, t) {
  return e ? M(e) && M(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : ie(
    /* @__PURE__ */ Object.create(null),
    Pn(e),
    Pn(t ?? {})
  ) : t;
}
function Co(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = ie(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = fe(e[n], t[n]);
  return s;
}
function Ki() {
  return {
    app: null,
    config: {
      isNativeTag: ti,
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
let To = 0;
function Oo(e, t) {
  return function(n, i = null) {
    $(n) || (n = ie({}, n)), i != null && !L(i) && (i = null);
    const r = Ki(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let c = !1;
    const d = r.app = {
      _uid: To++,
      _component: n,
      _props: i,
      _container: null,
      _context: r,
      _instance: null,
      version: rl,
      get config() {
        return r.config;
      },
      set config(a) {
      },
      use(a, ...h) {
        return o.has(a) || (a && $(a.install) ? (o.add(a), a.install(d, ...h)) : $(a) && (o.add(a), a(d, ...h))), d;
      },
      mixin(a) {
        return r.mixins.includes(a) || r.mixins.push(a), d;
      },
      component(a, h) {
        return h ? (r.components[a] = h, d) : r.components[a];
      },
      directive(a, h) {
        return h ? (r.directives[a] = h, d) : r.directives[a];
      },
      mount(a, h, T) {
        if (!c) {
          const E = d._ceVNode || _e(n, i);
          return E.appContext = r, T === !0 ? T = "svg" : T === !1 && (T = void 0), e(E, a, T), c = !0, d._container = a, a.__vue_app__ = d, Ts(E.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        c && (we(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, h) {
        return r.provides[a] = h, d;
      },
      runWithContext(a) {
        const h = _t;
        _t = d;
        try {
          return a();
        } finally {
          _t = h;
        }
      }
    };
    return d;
  };
}
let _t = null;
const Eo = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${ye(t)}Modifiers`] || e[`${ft(t)}Modifiers`];
function Ao(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || k;
  let i = s;
  const r = t.startsWith("update:"), o = r && Eo(n, t.slice(7));
  o && (o.trim && (i = s.map((a) => X(a) ? a.trim() : a)), o.number && (i = s.map(bs)));
  let l, c = n[l = As(t)] || // also try camelCase event handler (#2249)
  n[l = As(ye(t))];
  !c && r && (c = n[l = As(ft(t))]), c && we(
    c,
    e,
    6,
    i
  );
  const d = n[l + "Once"];
  if (d) {
    if (!e.emitted)
      e.emitted = {};
    else if (e.emitted[l])
      return;
    e.emitted[l] = !0, we(
      d,
      e,
      6,
      i
    );
  }
}
const Po = /* @__PURE__ */ new WeakMap();
function ki(e, t, s = !1) {
  const n = s ? Po : t.emitsCache, i = n.get(e);
  if (i !== void 0)
    return i;
  const r = e.emits;
  let o = {}, l = !1;
  if (!$(e)) {
    const c = (d) => {
      const a = ki(d, t, !0);
      a && (l = !0, ie(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(c), e.extends && c(e.extends), e.mixins && e.mixins.forEach(c);
  }
  return !r && !l ? (L(e) && n.set(e, null), null) : (M(r) ? r.forEach((c) => o[c] = null) : ie(o, r), L(e) && n.set(e, o), o);
}
function ws(e, t) {
  return !e || !gs(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), N(e, t[0].toLowerCase() + t.slice(1)) || N(e, ft(t)) || N(e, t));
}
function $n(e) {
  const {
    type: t,
    vnode: s,
    proxy: n,
    withProxy: i,
    propsOptions: [r],
    slots: o,
    attrs: l,
    emit: c,
    render: d,
    renderCache: a,
    props: h,
    data: T,
    setupState: E,
    ctx: D,
    inheritAttrs: R
  } = e, G = us(e);
  let B, v;
  try {
    if (s.shapeFlag & 4) {
      const y = i || n, Z = y;
      B = Re(
        d.call(
          Z,
          y,
          a,
          h,
          E,
          T,
          D
        )
      ), v = l;
    } else {
      const y = t;
      B = Re(
        y.length > 1 ? y(
          h,
          { attrs: l, slots: o, emit: c }
        ) : y(
          h,
          null
        )
      ), v = t.props ? l : Mo(l);
    }
  } catch (y) {
    Ht.length = 0, xs(y, e, 1), B = _e(We);
  }
  let O = B;
  if (v && R !== !1) {
    const y = Object.keys(v), { shapeFlag: Z } = O;
    y.length && Z & 7 && (r && y.some(ms) && (v = Io(
      v,
      r
    )), O = vt(O, v, !1, !0));
  }
  return s.dirs && (O = vt(O, null, !1, !0), O.dirs = O.dirs ? O.dirs.concat(s.dirs) : s.dirs), s.transition && an(O, s.transition), B = O, us(G), B;
}
const Mo = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || gs(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, Io = (e, t) => {
  const s = {};
  for (const n in e)
    (!ms(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function Ro(e, t, s) {
  const { props: n, children: i, component: r } = e, { props: o, children: l, patchFlag: c } = t, d = r.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (s && c >= 0) {
    if (c & 1024)
      return !0;
    if (c & 16)
      return n ? Fn(n, o, d) : !!o;
    if (c & 8) {
      const a = t.dynamicProps;
      for (let h = 0; h < a.length; h++) {
        const T = a[h];
        if (Bi(o, n, T) && !ws(d, T))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? Fn(n, o, d) : !0 : !!o;
  return !1;
}
function Fn(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < n.length; i++) {
    const r = n[i];
    if (Bi(t, e, r) && !ws(s, r))
      return !0;
  }
  return !1;
}
function Bi(e, t, s) {
  const n = e[s], i = t[s];
  return s === "style" && L(n) && L(i) ? !wt(n, i) : n !== i;
}
function $o({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const i = t.subTree;
    if (i.suspense && i.suspense.activeBranch === e && (i.suspense.vnode.el = i.el = n, e = i), i === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const Wi = {}, Ji = () => Object.create(Wi), qi = (e) => Object.getPrototypeOf(e) === Wi;
function Fo(e, t, s, n = !1) {
  const i = {}, r = Ji();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), Gi(e, t, i, r);
  for (const o in e.propsOptions[0])
    o in i || (i[o] = void 0);
  s ? e.props = n ? i : /* @__PURE__ */ kr(i) : e.type.props ? e.props = i : e.props = r, e.attrs = r;
}
function Vo(e, t, s, n) {
  const {
    props: i,
    attrs: r,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ H(i), [c] = e.propsOptions;
  let d = !1;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    (n || o > 0) && !(o & 16)
  ) {
    if (o & 8) {
      const a = e.vnode.dynamicProps;
      for (let h = 0; h < a.length; h++) {
        let T = a[h];
        if (ws(e.emitsOptions, T))
          continue;
        const E = t[T];
        if (c)
          if (N(r, T))
            E !== r[T] && (r[T] = E, d = !0);
          else {
            const D = ye(T);
            i[D] = qs(
              c,
              l,
              D,
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
    Gi(e, t, i, r) && (d = !0);
    let a;
    for (const h in l)
      (!t || // for camelCase
      !N(t, h) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = ft(h)) === h || !N(t, a))) && (c ? s && // for camelCase
      (s[h] !== void 0 || // for kebab-case
      s[a] !== void 0) && (i[h] = qs(
        c,
        l,
        h,
        void 0,
        e,
        !0
      )) : delete i[h]);
    if (r !== l)
      for (const h in r)
        (!t || !N(t, h)) && (delete r[h], d = !0);
  }
  d && Le(e.attrs, "set", "");
}
function Gi(e, t, s, n) {
  const [i, r] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let c in t) {
      if (Rt(c))
        continue;
      const d = t[c];
      let a;
      i && N(i, a = ye(c)) ? !r || !r.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : ws(e.emitsOptions, c) || (!(c in n) || d !== n[c]) && (n[c] = d, o = !0);
    }
  if (r) {
    const c = /* @__PURE__ */ H(s), d = l || k;
    for (let a = 0; a < r.length; a++) {
      const h = r[a];
      s[h] = qs(
        i,
        c,
        h,
        d[h],
        e,
        !N(d, h)
      );
    }
  }
  return o;
}
function qs(e, t, s, n, i, r) {
  const o = e[s];
  if (o != null) {
    const l = N(o, "default");
    if (l && n === void 0) {
      const c = o.default;
      if (o.type !== Function && !o.skipFactory && $(c)) {
        const { propsDefaults: d } = i;
        if (s in d)
          n = d[s];
        else {
          const a = Gt(i);
          n = d[s] = c.call(
            null,
            t
          ), a();
        }
      } else
        n = c;
      i.ce && i.ce._setProp(s, n);
    }
    o[
      0
      /* shouldCast */
    ] && (r && !l ? n = !1 : o[
      1
      /* shouldCastTrue */
    ] && (n === "" || n === ft(s)) && (n = !0));
  }
  return n;
}
const Do = /* @__PURE__ */ new WeakMap();
function Yi(e, t, s = !1) {
  const n = s ? Do : t.propsCache, i = n.get(e);
  if (i)
    return i;
  const r = e.props, o = {}, l = [];
  let c = !1;
  if (!$(e)) {
    const a = (h) => {
      c = !0;
      const [T, E] = Yi(h, t, !0);
      ie(o, T), E && l.push(...E);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!r && !c)
    return L(e) && n.set(e, dt), dt;
  if (M(r))
    for (let a = 0; a < r.length; a++) {
      const h = ye(r[a]);
      Vn(h) && (o[h] = k);
    }
  else if (r)
    for (const a in r) {
      const h = ye(a);
      if (Vn(h)) {
        const T = r[a], E = o[h] = M(T) || $(T) ? { type: T } : ie({}, T), D = E.type;
        let R = !1, G = !0;
        if (M(D))
          for (let B = 0; B < D.length; ++B) {
            const v = D[B], O = $(v) && v.name;
            if (O === "Boolean") {
              R = !0;
              break;
            } else O === "String" && (G = !1);
          }
        else
          R = $(D) && D.name === "Boolean";
        E[
          0
          /* shouldCast */
        ] = R, E[
          1
          /* shouldCastTrue */
        ] = G, (R || N(E, "default")) && l.push(h);
      }
    }
  const d = [o, l];
  return L(e) && n.set(e, d), d;
}
function Vn(e) {
  return e[0] !== "$" && !Rt(e);
}
const dn = (e) => e === "_" || e === "_ctx" || e === "$stable", pn = (e) => M(e) ? e.map(Re) : [Re(e)], Ho = (e, t, s) => {
  if (t._n)
    return t;
  const n = gt((...i) => pn(t(...i)), s);
  return n._c = !1, n;
}, zi = (e, t, s) => {
  const n = e._ctx;
  for (const i in e) {
    if (dn(i)) continue;
    const r = e[i];
    if ($(r))
      t[i] = Ho(i, r, n);
    else if (r != null) {
      const o = pn(r);
      t[i] = () => o;
    }
  }
}, Xi = (e, t) => {
  const s = pn(t);
  e.slots.default = () => s;
}, Zi = (e, t, s) => {
  for (const n in t)
    (s || !dn(n)) && (e[n] = t[n]);
}, No = (e, t, s) => {
  const n = e.slots = Ji();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Zi(n, t, s), s && oi(n, "_", i, !0)) : zi(t, n);
  } else t && Xi(e, t);
}, jo = (e, t, s) => {
  const { vnode: n, slots: i } = e;
  let r = !0, o = k;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? r = !1 : Zi(i, t, s) : (r = !t.$stable, zi(t, i)), o = t;
  } else t && (Xi(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !dn(l) && o[l] == null && delete i[l];
}, de = Bo;
function Uo(e) {
  return Lo(e);
}
function Lo(e, t) {
  const s = ys();
  s.__VUE__ = !0;
  const {
    insert: n,
    remove: i,
    patchProp: r,
    createElement: o,
    createText: l,
    createComment: c,
    setText: d,
    setElementText: a,
    parentNode: h,
    nextSibling: T,
    setScopeId: E = Fe,
    insertStaticContent: D
  } = e, R = (f, u, p, b = null, _ = null, g = null, w = void 0, S = null, x = !!u.dynamicChildren) => {
    if (f === u)
      return;
    f && !Pt(f, u) && (b = Qt(f), Te(f, _, g, !0), f = null), u.patchFlag === -2 && (x = !1, u.dynamicChildren = null);
    const { type: m, ref: P, shapeFlag: C } = u;
    switch (m) {
      case Cs:
        G(f, u, p, b);
        break;
      case We:
        B(f, u, p, b);
        break;
      case Ds:
        f == null && v(u, p, b, w);
        break;
      case pe:
        zt(
          f,
          u,
          p,
          b,
          _,
          g,
          w,
          S,
          x
        );
        break;
      default:
        C & 1 ? Z(
          f,
          u,
          p,
          b,
          _,
          g,
          w,
          S,
          x
        ) : C & 6 ? Xt(
          f,
          u,
          p,
          b,
          _,
          g,
          w,
          S,
          x
        ) : (C & 64 || C & 128) && m.process(
          f,
          u,
          p,
          b,
          _,
          g,
          w,
          S,
          x,
          Ot
        );
    }
    P != null && _ ? Vt(P, f && f.ref, g, u || f, !u) : P == null && f && f.ref != null && Vt(f.ref, null, g, f, !0);
  }, G = (f, u, p, b) => {
    if (f == null)
      n(
        u.el = l(u.children),
        p,
        b
      );
    else {
      const _ = u.el = f.el;
      u.children !== f.children && d(_, u.children);
    }
  }, B = (f, u, p, b) => {
    f == null ? n(
      u.el = c(u.children || ""),
      p,
      b
    ) : u.el = f.el;
  }, v = (f, u, p, b) => {
    [f.el, f.anchor] = D(
      f.children,
      u,
      p,
      b,
      f.el,
      f.anchor
    );
  }, O = ({ el: f, anchor: u }, p, b) => {
    let _;
    for (; f && f !== u; )
      _ = T(f), n(f, p, b), f = _;
    n(u, p, b);
  }, y = ({ el: f, anchor: u }) => {
    let p;
    for (; f && f !== u; )
      p = T(f), i(f), f = p;
    i(u);
  }, Z = (f, u, p, b, _, g, w, S, x) => {
    if (u.type === "svg" ? w = "svg" : u.type === "math" && (w = "mathml"), f == null)
      j(
        u,
        p,
        b,
        _,
        g,
        w,
        S,
        x
      );
    else {
      const m = f.el && f.el._isVueCE ? f.el : null;
      try {
        m && m._beginPatch(), Yt(
          f,
          u,
          _,
          g,
          w,
          S,
          x
        );
      } finally {
        m && m._endPatch();
      }
    }
  }, j = (f, u, p, b, _, g, w, S) => {
    let x, m;
    const { props: P, shapeFlag: C, transition: A, dirs: I } = f;
    if (x = f.el = o(
      f.type,
      g,
      P && P.is,
      P
    ), C & 8 ? a(x, f.children) : C & 16 && qe(
      f.children,
      x,
      null,
      b,
      _,
      Vs(f, g),
      w,
      S
    ), I && st(f, null, b, "created"), Ce(x, f, f.scopeId, w, b), P) {
      for (const K in P)
        K !== "value" && !Rt(K) && r(x, K, null, P[K], g, b);
      "value" in P && r(x, "value", null, P.value, g), (m = P.onVnodeBeforeMount) && Pe(m, b, f);
    }
    I && st(f, null, b, "beforeMount");
    const V = Ko(_, A);
    V && A.beforeEnter(x), n(x, u, p), ((m = P && P.onVnodeMounted) || V || I) && de(() => {
      try {
        m && Pe(m, b, f), V && A.enter(x), I && st(f, null, b, "mounted");
      } finally {
      }
    }, _);
  }, Ce = (f, u, p, b, _) => {
    if (p && E(f, p), b)
      for (let g = 0; g < b.length; g++)
        E(f, b[g]);
    if (_) {
      let g = _.subTree;
      if (u === g || sr(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const w = _.vnode;
        Ce(
          f,
          w,
          w.scopeId,
          w.slotScopeIds,
          _.parent
        );
      }
    }
  }, qe = (f, u, p, b, _, g, w, S, x = 0) => {
    for (let m = x; m < f.length; m++) {
      const P = f[m] = S ? Ue(f[m]) : Re(f[m]);
      R(
        null,
        P,
        u,
        p,
        b,
        _,
        g,
        w,
        S
      );
    }
  }, Yt = (f, u, p, b, _, g, w) => {
    const S = u.el = f.el;
    let { patchFlag: x, dynamicChildren: m, dirs: P } = u;
    x |= f.patchFlag & 16;
    const C = f.props || k, A = u.props || k;
    let I;
    if (p && nt(p, !1), (I = A.onVnodeBeforeUpdate) && Pe(I, p, u, f), P && st(u, f, p, "beforeUpdate"), p && nt(p, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    m && (!f.dynamicChildren || f.dynamicChildren.length !== m.length) && (x = 0, w = !1, m = null), (C.innerHTML && A.innerHTML == null || C.textContent && A.textContent == null) && a(S, ""), m ? Qe(
      f.dynamicChildren,
      m,
      S,
      p,
      b,
      Vs(u, _),
      g
    ) : w || W(
      f,
      u,
      S,
      null,
      p,
      b,
      Vs(u, _),
      g,
      !1
    ), x > 0) {
      if (x & 16)
        Ct(S, C, A, p, _);
      else if (x & 2 && C.class !== A.class && r(S, "class", null, A.class, _), x & 4 && r(S, "style", C.style, A.style, _), x & 8) {
        const V = u.dynamicProps;
        for (let K = 0; K < V.length; K++) {
          const U = V[K], te = C[U], se = A[U];
          (se !== te || U === "value") && r(S, U, te, se, _, p);
        }
      }
      x & 1 && f.children !== u.children && a(S, u.children);
    } else !w && m == null && Ct(S, C, A, p, _);
    ((I = A.onVnodeUpdated) || P) && de(() => {
      I && Pe(I, p, u, f), P && st(u, f, p, "updated");
    }, b);
  }, Qe = (f, u, p, b, _, g, w) => {
    for (let S = 0; S < u.length; S++) {
      const x = f[S], m = u[S], P = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        x.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (x.type === pe || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !Pt(x, m) || // - In the case of a component, it could contain anything.
        x.shapeFlag & 198) ? h(x.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          p
        )
      );
      R(
        x,
        m,
        P,
        null,
        b,
        _,
        g,
        w,
        !0
      );
    }
  }, Ct = (f, u, p, b, _) => {
    if (u !== p) {
      if (u !== k)
        for (const g in u)
          !Rt(g) && !(g in p) && r(
            f,
            g,
            u[g],
            null,
            _,
            b
          );
      for (const g in p) {
        if (Rt(g)) continue;
        const w = p[g], S = u[g];
        w !== S && g !== "value" && r(f, g, S, w, _, b);
      }
      "value" in p && r(f, "value", u.value, p.value, _);
    }
  }, zt = (f, u, p, b, _, g, w, S, x) => {
    const m = u.el = f ? f.el : l(""), P = u.anchor = f ? f.anchor : l("");
    let { patchFlag: C, dynamicChildren: A, slotScopeIds: I } = u;
    I && (S = S ? S.concat(I) : I), f == null ? (n(m, p, b), n(P, p, b), qe(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      p,
      P,
      _,
      g,
      w,
      S,
      x
    )) : C > 0 && C & 64 && A && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    f.dynamicChildren && f.dynamicChildren.length === A.length ? (Qe(
      f.dynamicChildren,
      A,
      p,
      _,
      g,
      w,
      S
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || _ && u === _.subTree) && Qi(
      f,
      u,
      !0
      /* shallow */
    )) : W(
      f,
      u,
      p,
      P,
      _,
      g,
      w,
      S,
      x
    );
  }, Xt = (f, u, p, b, _, g, w, S, x) => {
    u.slotScopeIds = S, f == null ? u.shapeFlag & 512 ? _.ctx.activate(
      u,
      p,
      b,
      w,
      x
    ) : Os(
      u,
      p,
      b,
      _,
      g,
      w,
      x
    ) : gn(f, u, x);
  }, Os = (f, u, p, b, _, g, w) => {
    const S = f.component = Xo(
      f,
      b,
      _
    );
    if (Fi(f) && (S.ctx.renderer = Ot), Qo(S, !1, w), S.asyncDep) {
      if (_ && _.registerDep(S, ce, w), !f.el) {
        const x = S.subTree = _e(We);
        B(null, x, u, p), f.placeholder = x.el;
      }
    } else
      ce(
        S,
        f,
        u,
        p,
        _,
        g,
        w
      );
  }, gn = (f, u, p) => {
    const b = u.component = f.component;
    if (Ro(f, u, p))
      if (b.asyncDep && !b.asyncResolved) {
        q(b, u, p);
        return;
      } else
        b.next = u, b.update();
    else
      u.el = f.el, b.vnode = u;
  }, ce = (f, u, p, b, _, g, w) => {
    const S = () => {
      if (f.isMounted) {
        let { next: C, bu: A, u: I, parent: V, vnode: K } = f;
        {
          const Ee = er(f);
          if (Ee) {
            C && (C.el = K.el, q(f, C, w)), Ee.asyncDep.then(() => {
              de(() => {
                f.isUnmounted || m();
              }, _);
            });
            return;
          }
        }
        let U = C, te;
        nt(f, !1), C ? (C.el = K.el, q(f, C, w)) : C = K, A && ns(A), (te = C.props && C.props.onVnodeBeforeUpdate) && Pe(te, V, C, K), nt(f, !0);
        const se = $n(f), Oe = f.subTree;
        f.subTree = se, R(
          Oe,
          se,
          // parent may have changed if it's in a teleport
          h(Oe.el),
          // anchor may have changed if it's in a fragment
          Qt(Oe),
          f,
          _,
          g
        ), C.el = se.el, U === null && $o(f, se.el), I && de(I, _), (te = C.props && C.props.onVnodeUpdated) && de(
          () => Pe(te, V, C, K),
          _
        );
      } else {
        let C;
        const { el: A, props: I } = u, { bm: V, m: K, parent: U, root: te, type: se } = f, Oe = mt(u);
        nt(f, !1), V && ns(V), !Oe && (C = I && I.onVnodeBeforeMount) && Pe(C, U, u), nt(f, !0);
        {
          te.ce && te.ce._hasShadowRoot() && te.ce._injectChildStyle(
            se,
            f.parent ? f.parent.type : void 0
          );
          const Ee = f.subTree = $n(f);
          R(
            null,
            Ee,
            p,
            b,
            f,
            _,
            g
          ), u.el = Ee.el;
        }
        if (K && de(K, _), !Oe && (C = I && I.onVnodeMounted)) {
          const Ee = u;
          de(
            () => Pe(C, U, Ee),
            _
          );
        }
        (u.shapeFlag & 256 || U && mt(U.vnode) && U.vnode.shapeFlag & 256) && f.a && de(f.a, _), f.isMounted = !0, u = p = b = null;
      }
    };
    f.scope.on();
    const x = f.effect = new ui(S);
    f.scope.off();
    const m = f.update = x.run.bind(x), P = f.job = x.runIfDirty.bind(x);
    P.i = f, P.id = f.uid, x.scheduler = () => un(P), nt(f, !0), m();
  }, q = (f, u, p) => {
    u.component = f;
    const b = f.vnode.props;
    f.vnode = u, f.next = null, Vo(f, u.props, b, p), jo(f, u.children, p), Ve(), Tn(f), De();
  }, W = (f, u, p, b, _, g, w, S, x = !1) => {
    const m = f && f.children, P = f ? f.shapeFlag : 0, C = u.children, { patchFlag: A, shapeFlag: I } = u;
    if (A > 0) {
      if (A & 128) {
        Zt(
          m,
          C,
          p,
          b,
          _,
          g,
          w,
          S,
          x
        );
        return;
      } else if (A & 256) {
        et(
          m,
          C,
          p,
          b,
          _,
          g,
          w,
          S,
          x
        );
        return;
      }
    }
    I & 8 ? (P & 16 && Tt(m, _, g), C !== m && a(p, C)) : P & 16 ? I & 16 ? Zt(
      m,
      C,
      p,
      b,
      _,
      g,
      w,
      S,
      x
    ) : Tt(m, _, g, !0) : (P & 8 && a(p, ""), I & 16 && qe(
      C,
      p,
      b,
      _,
      g,
      w,
      S,
      x
    ));
  }, et = (f, u, p, b, _, g, w, S, x) => {
    f = f || dt, u = u || dt;
    const m = f.length, P = u.length, C = Math.min(m, P);
    let A;
    for (A = 0; A < C; A++) {
      const I = u[A] = x ? Ue(u[A]) : Re(u[A]);
      R(
        f[A],
        I,
        p,
        null,
        _,
        g,
        w,
        S,
        x
      );
    }
    m > P ? Tt(
      f,
      _,
      g,
      !0,
      !1,
      C
    ) : qe(
      u,
      p,
      b,
      _,
      g,
      w,
      S,
      x,
      C
    );
  }, Zt = (f, u, p, b, _, g, w, S, x) => {
    let m = 0;
    const P = u.length;
    let C = f.length - 1, A = P - 1;
    for (; m <= C && m <= A; ) {
      const I = f[m], V = u[m] = x ? Ue(u[m]) : Re(u[m]);
      if (Pt(I, V))
        R(
          I,
          V,
          p,
          null,
          _,
          g,
          w,
          S,
          x
        );
      else
        break;
      m++;
    }
    for (; m <= C && m <= A; ) {
      const I = f[C], V = u[A] = x ? Ue(u[A]) : Re(u[A]);
      if (Pt(I, V))
        R(
          I,
          V,
          p,
          null,
          _,
          g,
          w,
          S,
          x
        );
      else
        break;
      C--, A--;
    }
    if (m > C) {
      if (m <= A) {
        const I = A + 1, V = I < P ? u[I].el : b;
        for (; m <= A; )
          R(
            null,
            u[m] = x ? Ue(u[m]) : Re(u[m]),
            p,
            V,
            _,
            g,
            w,
            S,
            x
          ), m++;
      }
    } else if (m > A)
      for (; m <= C; )
        Te(f[m], _, g, !0), m++;
    else {
      const I = m, V = m, K = /* @__PURE__ */ new Map();
      for (m = V; m <= A; m++) {
        const he = u[m] = x ? Ue(u[m]) : Re(u[m]);
        he.key != null && K.set(he.key, m);
      }
      let U, te = 0;
      const se = A - V + 1;
      let Oe = !1, Ee = 0;
      const Et = new Array(se);
      for (m = 0; m < se; m++) Et[m] = 0;
      for (m = I; m <= C; m++) {
        const he = f[m];
        if (te >= se) {
          Te(he, _, g, !0);
          continue;
        }
        let Ae;
        if (he.key != null)
          Ae = K.get(he.key);
        else
          for (U = V; U <= A; U++)
            if (Et[U - V] === 0 && Pt(he, u[U])) {
              Ae = U;
              break;
            }
        Ae === void 0 ? Te(he, _, g, !0) : (Et[Ae - V] = m + 1, Ae >= Ee ? Ee = Ae : Oe = !0, R(
          he,
          u[Ae],
          p,
          null,
          _,
          g,
          w,
          S,
          x
        ), te++);
      }
      const bn = Oe ? ko(Et) : dt;
      for (U = bn.length - 1, m = se - 1; m >= 0; m--) {
        const he = V + m, Ae = u[he], yn = u[he + 1], vn = he + 1 < P ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          yn.el || tr(yn)
        ) : b;
        Et[m] === 0 ? R(
          null,
          Ae,
          p,
          vn,
          _,
          g,
          w,
          S,
          x
        ) : Oe && (U < 0 || m !== bn[U] ? tt(Ae, p, vn, 2) : U--);
      }
    }
  }, tt = (f, u, p, b, _ = null) => {
    const { el: g, type: w, transition: S, children: x, shapeFlag: m } = f;
    if (m & 6) {
      tt(f.component.subTree, u, p, b);
      return;
    }
    if (m & 128) {
      f.suspense.move(u, p, b);
      return;
    }
    if (m & 64) {
      w.move(f, u, p, Ot);
      return;
    }
    if (w === pe) {
      n(g, u, p);
      for (let C = 0; C < x.length; C++)
        tt(x[C], u, p, b);
      n(f.anchor, u, p);
      return;
    }
    if (w === Ds) {
      O(f, u, p);
      return;
    }
    if (b !== 2 && m & 1 && S)
      if (b === 0)
        S.persisted && !g[$s] ? n(g, u, p) : (S.beforeEnter(g), n(g, u, p), de(() => S.enter(g), _));
      else {
        const { leave: C, delayLeave: A, afterLeave: I } = S, V = () => {
          f.ctx.isUnmounted ? i(g) : n(g, u, p);
        }, K = () => {
          const U = g._isLeaving || !!g[$s];
          g._isLeaving && g[$s](
            !0
            /* cancelled */
          ), S.persisted && !U ? V() : C(g, () => {
            V(), I && I();
          });
        };
        A ? A(g, V, K) : K();
      }
    else
      n(g, u, p);
  }, Te = (f, u, p, b = !1, _ = !1) => {
    const {
      type: g,
      props: w,
      ref: S,
      children: x,
      dynamicChildren: m,
      shapeFlag: P,
      patchFlag: C,
      dirs: A,
      cacheIndex: I,
      memo: V
    } = f;
    if (C === -2 && (_ = !1), S != null && (Ve(), Vt(S, null, p, f, !0), De()), I != null && (u.renderCache[I] = void 0), P & 256) {
      u.ctx.deactivate(f);
      return;
    }
    const K = P & 1 && A, U = !mt(f);
    let te;
    if (U && (te = w && w.onVnodeBeforeUnmount) && Pe(te, u, f), P & 6)
      dr(f.component, p, b);
    else {
      if (P & 128) {
        f.suspense.unmount(p, b);
        return;
      }
      K && st(f, null, u, "beforeUnmount"), P & 64 ? f.type.remove(
        f,
        u,
        p,
        Ot,
        b
      ) : m && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !m.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== pe || C > 0 && C & 64) ? Tt(
        m,
        u,
        p,
        !1,
        !0
      ) : (g === pe && C & 384 || !_ && P & 16) && Tt(x, u, p), b && mn(f);
    }
    const se = V != null && I == null;
    (U && (te = w && w.onVnodeUnmounted) || K || se) && de(() => {
      te && Pe(te, u, f), K && st(f, null, u, "unmounted"), se && (f.el = null);
    }, p);
  }, mn = (f) => {
    const { type: u, el: p, anchor: b, transition: _ } = f;
    if (u === pe) {
      ar(p, b);
      return;
    }
    if (u === Ds) {
      y(f);
      return;
    }
    const g = () => {
      i(p), _ && !_.persisted && _.afterLeave && _.afterLeave();
    };
    if (f.shapeFlag & 1 && _ && !_.persisted) {
      const { leave: w, delayLeave: S } = _, x = () => w(p, g);
      S ? S(f.el, g, x) : x();
    } else
      g();
  }, ar = (f, u) => {
    let p;
    for (; f !== u; )
      p = T(f), i(f), f = p;
    i(u);
  }, dr = (f, u, p) => {
    const { bum: b, scope: _, job: g, subTree: w, um: S, m: x, a: m } = f;
    Dn(x), Dn(m), b && ns(b), _.stop(), g && (g.flags |= 8, Te(w, f, u, p)), S && de(S, u), de(() => {
      f.isUnmounted = !0;
    }, u);
  }, Tt = (f, u, p, b = !1, _ = !1, g = 0) => {
    for (let w = g; w < f.length; w++)
      Te(f[w], u, p, b, _);
  }, Qt = (f) => {
    if (f.shapeFlag & 6)
      return Qt(f.component.subTree);
    if (f.shapeFlag & 128)
      return f.suspense.next();
    const u = T(f.anchor || f.el), p = u && u[ro];
    return p ? T(p) : u;
  };
  let Es = !1;
  const _n = (f, u, p) => {
    let b;
    f == null ? u._vnode && (Te(u._vnode, null, null, !0), b = u._vnode.component) : R(
      u._vnode || null,
      f,
      u,
      null,
      null,
      null,
      p
    ), u._vnode = f, Es || (Es = !0, Tn(b), Ai(), Es = !1);
  }, Ot = {
    p: R,
    um: Te,
    m: tt,
    r: mn,
    mt: Os,
    mc: qe,
    pc: W,
    pbc: Qe,
    n: Qt,
    o: e
  };
  return {
    render: _n,
    hydrate: void 0,
    createApp: Oo(_n)
  };
}
function Vs({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function nt({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Ko(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Qi(e, t, s = !1) {
  const n = e.children, i = t.children;
  if (M(n) && M(i))
    for (let r = 0; r < n.length; r++) {
      const o = n[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = Ue(i[r]), l.el = o.el), !s && l.patchFlag !== -2 && Qi(o, l)), l.type === Cs && (l.patchFlag === -1 && (l = i[r] = Ue(l)), l.el = o.el), l.type === We && !l.el && (l.el = o.el);
    }
}
function ko(e) {
  const t = e.slice(), s = [0];
  let n, i, r, o, l;
  const c = e.length;
  for (n = 0; n < c; n++) {
    const d = e[n];
    if (d !== 0) {
      if (i = s[s.length - 1], e[i] < d) {
        t[n] = i, s.push(n);
        continue;
      }
      for (r = 0, o = s.length - 1; r < o; )
        l = r + o >> 1, e[s[l]] < d ? r = l + 1 : o = l;
      d < e[s[r]] && (r > 0 && (t[n] = s[r - 1]), s[r] = n);
    }
  }
  for (r = s.length, o = s[r - 1]; r-- > 0; )
    s[r] = o, o = t[o];
  return s;
}
function er(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : er(t);
}
function Dn(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function tr(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? tr(t.subTree) : null;
}
const sr = (e) => e.__isSuspense;
function Bo(e, t) {
  t && t.pendingBranch ? M(e) ? t.effects.push(...e) : t.effects.push(e) : eo(e);
}
const pe = /* @__PURE__ */ Symbol.for("v-fgt"), Cs = /* @__PURE__ */ Symbol.for("v-txt"), We = /* @__PURE__ */ Symbol.for("v-cmt"), Ds = /* @__PURE__ */ Symbol.for("v-stc"), Ht = [];
let ge = null;
function ee(e = !1) {
  Ht.push(ge = e ? null : []);
}
function Wo() {
  Ht.pop(), ge = Ht[Ht.length - 1] || null;
}
let Kt = 1;
function Hn(e, t = !1) {
  Kt += e, e < 0 && ge && t && (ge.hasOnce = !0);
}
function nr(e) {
  return e.dynamicChildren = Kt > 0 ? ge || dt : null, Wo(), Kt > 0 && ge && ge.push(e), e;
}
function me(e, t, s, n, i, r) {
  return nr(
    F(
      e,
      t,
      s,
      n,
      i,
      r,
      !0
    )
  );
}
function Xe(e, t, s, n, i) {
  return nr(
    _e(
      e,
      t,
      s,
      n,
      i,
      !0
    )
  );
}
function hn(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function Pt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const ir = ({ key: e }) => e ?? null, os = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? X(e) || /* @__PURE__ */ le(e) || $(e) ? { i: oe, r: e, k: t, f: !!s } : e : null);
function F(e, t = null, s = null, n = 0, i = null, r = e === pe ? 0 : 1, o = !1, l = !1) {
  const c = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && ir(t),
    ref: t && os(t),
    scopeId: Mi,
    slotScopeIds: null,
    children: s,
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
    patchFlag: n,
    dynamicProps: i,
    dynamicChildren: null,
    appContext: null,
    ctx: oe
  };
  return l ? (ps(c, s), r & 128 && e.normalize(c)) : s && (c.shapeFlag |= X(s) ? 8 : 16), Kt > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  ge && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (c.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  c.patchFlag !== 32 && ge.push(c), c;
}
const _e = Jo;
function Jo(e, t = null, s = null, n = 0, i = null, r = !1) {
  if ((!e || e === bo) && (e = We), hn(e)) {
    const l = vt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ps(l, s), Kt > 0 && !r && ge && (l.shapeFlag & 6 ? ge[ge.indexOf(e)] = l : ge.push(l)), l.patchFlag = -2, l;
  }
  if (nl(e) && (e = e.__vccOpts), t) {
    t = qo(t);
    let { class: l, style: c } = t;
    l && !X(l) && (t.class = Jt(l)), L(c) && (/* @__PURE__ */ fn(c) && !M(c) && (c = ie({}, c)), t.style = en(c));
  }
  const o = X(e) ? 1 : sr(e) ? 128 : oo(e) ? 64 : L(e) ? 4 : $(e) ? 2 : 0;
  return F(
    e,
    t,
    s,
    n,
    i,
    o,
    r,
    !0
  );
}
function qo(e) {
  return e ? /* @__PURE__ */ fn(e) || qi(e) ? ie({}, e) : e : null;
}
function vt(e, t, s = !1, n = !1) {
  const { props: i, ref: r, patchFlag: o, children: l, transition: c } = e, d = t ? Go(i || {}, t) : i, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && ir(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && r ? M(r) ? r.concat(os(t)) : [r, os(t)] : os(t)
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
    patchFlag: t && e.type !== pe ? o === -1 ? 16 : o | 16 : o,
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
  return c && n && an(
    a,
    c.clone(a)
  ), a;
}
function ct(e = " ", t = 0) {
  return _e(Cs, null, e, t);
}
function bt(e = "", t = !1) {
  return t ? (ee(), Xe(We, null, e)) : _e(We, null, e);
}
function Re(e) {
  return e == null || typeof e == "boolean" ? _e(We) : M(e) ? _e(
    pe,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : hn(e) ? Ue(e) : _e(Cs, null, String(e));
}
function Ue(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : vt(e);
}
function ps(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (M(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), ps(e, i()), i._c && (i._d = !0));
      return;
    } else {
      s = 32;
      const i = t._;
      !i && !qi(t) ? t._ctx = oe : i === 3 && oe && (oe.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if ($(t)) {
    if (n & 65) {
      ps(e, { default: t });
      return;
    }
    t = { default: t, _ctx: oe }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [ct(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function Go(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const i in n)
      if (i === "class")
        t.class !== n.class && (t.class = Jt([t.class, n.class]));
      else if (i === "style")
        t.style = en([t.style, n.style]);
      else if (gs(i)) {
        const r = t[i], o = n[i];
        o && r !== o && !(M(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !ms(i) && (t[i] = o);
      } else i !== "" && (t[i] = n[i]);
  }
  return t;
}
function Pe(e, t, s, n = null) {
  we(e, t, 7, [
    s,
    n
  ]);
}
const Yo = Ki();
let zo = 0;
function Xo(e, t, s) {
  const n = e.type, i = (t ? t.appContext : e.appContext) || Yo, r = {
    uid: zo++,
    vnode: e,
    type: n,
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
    scope: new Cr(
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
    propsOptions: Yi(n, i),
    emitsOptions: ki(n, i),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: k,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: k,
    data: k,
    props: k,
    attrs: k,
    slots: k,
    refs: k,
    setupState: k,
    setupContext: null,
    // suspense related
    suspense: s,
    suspenseId: s ? s.pendingId : 0,
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
  return r.ctx = { _: r }, r.root = t ? t.root : r, r.emit = Ao.bind(null, r), e.ce && e.ce(r), r;
}
let ae = null;
const Zo = () => ae || oe;
let hs, Gs;
{
  const e = ys(), t = (s, n) => {
    let i;
    return (i = e[s]) || (i = e[s] = []), i.push(n), (r) => {
      i.length > 1 ? i.forEach((o) => o(r)) : i[0](r);
    };
  };
  hs = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => ae = s
  ), Gs = t(
    "__VUE_SSR_SETTERS__",
    (s) => kt = s
  );
}
const Gt = (e) => {
  const t = ae;
  return hs(e), e.scope.on(), () => {
    e.scope.off(), hs(t);
  };
}, Nn = () => {
  ae && ae.scope.off(), hs(null);
};
function rr(e) {
  return e.vnode.shapeFlag & 4;
}
let kt = !1;
function Qo(e, t = !1, s = !1) {
  t && Gs(t);
  const { props: n, children: i } = e.vnode, r = rr(e);
  Fo(e, n, r, t), No(e, i, s || t);
  const o = r ? el(e, t) : void 0;
  return t && Gs(!1), o;
}
function el(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, yo);
  const { setup: n } = s;
  if (n) {
    Ve();
    const i = e.setupContext = n.length > 1 ? sl(e) : null, r = Gt(e), o = qt(
      n,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = si(o);
    if (De(), r(), (l || e.sp) && !mt(e) && $i(e), l) {
      if (o.then(Nn, Nn), t)
        return o.then((c) => {
          jn(e, c);
        }).catch((c) => {
          xs(c, e, 0);
        });
      e.asyncDep = o;
    } else
      jn(e, o);
  } else
    or(e);
}
function jn(e, t, s) {
  $(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : L(t) && (e.setupState = Ci(t)), or(e);
}
function or(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Fe);
  {
    const i = Gt(e);
    Ve();
    try {
      vo(e);
    } finally {
      De(), i();
    }
  }
}
const tl = {
  get(e, t) {
    return re(e, "get", ""), e[t];
  }
};
function sl(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, tl),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function Ts(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(Ci(Br(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in Dt)
        return Dt[s](e);
    },
    has(t, s) {
      return s in t || s in Dt;
    }
  })) : e.proxy;
}
function nl(e) {
  return $(e) && "__vccOpts" in e;
}
const il = (e, t) => /* @__PURE__ */ Yr(e, t, kt), rl = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Ys;
const Un = typeof window < "u" && window.trustedTypes;
if (Un)
  try {
    Ys = /* @__PURE__ */ Un.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const lr = Ys ? (e) => Ys.createHTML(e) : (e) => e, ol = "http://www.w3.org/2000/svg", ll = "http://www.w3.org/1998/Math/MathML", je = typeof document < "u" ? document : null, Ln = je && /* @__PURE__ */ je.createElement("template"), cl = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const i = t === "svg" ? je.createElementNS(ol, e) : t === "mathml" ? je.createElementNS(ll, e) : s ? je.createElement(e, { is: s }) : je.createElement(e);
    return e === "select" && n && n.multiple != null && i.setAttribute("multiple", n.multiple), i;
  },
  createText: (e) => je.createTextNode(e),
  createComment: (e) => je.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => je.querySelector(e),
  setScopeId(e, t) {
    e.setAttribute(t, "");
  },
  // __UNSAFE__
  // Reason: innerHTML.
  // Static content here can only come from compiled templates.
  // As long as the user only uses trusted templates, this is safe.
  insertStaticContent(e, t, s, n, i, r) {
    const o = s ? s.previousSibling : t.lastChild;
    if (i && (i === r || i.nextSibling))
      for (; t.insertBefore(i.cloneNode(!0), s), !(i === r || !(i = i.nextSibling)); )
        ;
    else {
      Ln.innerHTML = lr(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = Ln.content;
      if (n === "svg" || n === "mathml") {
        const c = l.firstChild;
        for (; c.firstChild; )
          l.appendChild(c.firstChild);
        l.removeChild(c);
      }
      t.insertBefore(l, s);
    }
    return [
      // first
      o ? o.nextSibling : t.firstChild,
      // last
      s ? s.previousSibling : t.lastChild
    ];
  }
}, fl = /* @__PURE__ */ Symbol("_vtc");
function ul(e, t, s) {
  const n = e[fl];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const Kn = /* @__PURE__ */ Symbol("_vod"), al = /* @__PURE__ */ Symbol("_vsh"), dl = /* @__PURE__ */ Symbol(""), pl = /(?:^|;)\s*display\s*:/;
function hl(e, t, s) {
  const n = e.style, i = X(s);
  let r = !1;
  if (s && !i) {
    if (t)
      if (X(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && It(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && It(n, o, "");
    for (const o in s) {
      o === "display" && (r = !0);
      const l = s[o];
      l != null ? ml(
        e,
        o,
        !X(t) && t ? t[o] : void 0,
        l
      ) || It(n, o, l) : It(n, o, "");
    }
  } else if (i) {
    if (t !== s) {
      const o = n[dl];
      o && (s += ";" + o), n.cssText = s, r = pl.test(s);
    }
  } else t && e.removeAttribute("style");
  Kn in e && (e[Kn] = r ? n.display : "", e[al] && (n.display = "none"));
}
const kn = /\s*!important$/;
function It(e, t, s) {
  if (M(s))
    s.forEach((n) => It(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = gl(e, t);
    kn.test(s) ? e.setProperty(
      ft(n),
      s.replace(kn, ""),
      "important"
    ) : e[n] = s;
  }
}
const Bn = ["Webkit", "Moz", "ms"], Hs = {};
function gl(e, t) {
  const s = Hs[t];
  if (s)
    return s;
  let n = ye(t);
  if (n !== "filter" && n in e)
    return Hs[t] = n;
  n = ri(n);
  for (let i = 0; i < Bn.length; i++) {
    const r = Bn[i] + n;
    if (r in e)
      return Hs[t] = r;
  }
  return t;
}
function ml(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && X(n) && s === n;
}
const Wn = "http://www.w3.org/1999/xlink";
function Jn(e, t, s, n, i, r = Sr(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Wn, t.slice(6, t.length)) : e.setAttributeNS(Wn, t, s) : s == null || r && !li(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : xe(s) ? String(s) : s
  );
}
function qn(e, t, s, n, i) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? lr(s) : s);
    return;
  }
  const r = e.tagName;
  if (t === "value" && r !== "PROGRESS" && // custom elements may use _value internally
  !r.includes("-")) {
    const l = r === "OPTION" ? e.getAttribute("value") || "" : e.value, c = s == null ? (
      // #11647: value should be set as empty string for null and undefined,
      // but <input type="checkbox"> should be set as 'on'.
      e.type === "checkbox" ? "on" : ""
    ) : String(s);
    (l !== c || !("_value" in e)) && (e.value = c), s == null && e.removeAttribute(t), e._value = s;
    return;
  }
  let o = !1;
  if (s === "" || s == null) {
    const l = typeof e[t];
    l === "boolean" ? s = li(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(i || t);
}
function Ye(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function _l(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const Gn = /* @__PURE__ */ Symbol("_vei");
function bl(e, t, s, n, i = null) {
  const r = e[Gn] || (e[Gn] = {}), o = r[t];
  if (n && o)
    o.value = n;
  else {
    const [l, c] = xl(t);
    if (n) {
      const d = r[t] = Cl(
        n,
        i
      );
      Ye(e, l, d, c);
    } else o && (_l(e, l, o, c), r[t] = void 0);
  }
}
const yl = /(Once|Passive|Capture)$/, vl = /^on:?(?:Once|Passive|Capture)$/;
function xl(e) {
  let t, s;
  for (; (s = e.match(yl)) && !vl.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : ft(e.slice(2)), t];
}
let Ns = 0;
const Sl = /* @__PURE__ */ Promise.resolve(), wl = () => Ns || (Sl.then(() => Ns = 0), Ns = Date.now());
function Cl(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const i = s.value;
    if (M(i)) {
      const r = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        r.call(n), n._stopped = !0;
      };
      const o = i.slice(), l = [n];
      for (let c = 0; c < o.length && !n._stopped; c++) {
        const d = o[c];
        d && we(
          d,
          t,
          5,
          l
        );
      }
    } else
      we(
        i,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = wl(), s;
}
const Yn = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, Tl = (e, t, s, n, i, r) => {
  const o = i === "svg";
  t === "class" ? ul(e, n, o) : t === "style" ? hl(e, s, n) : gs(t) ? ms(t) || bl(e, t, s, n, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : Ol(e, t, n, o)) ? (qn(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Jn(e, t, n, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (El(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !X(n))) ? qn(e, ye(t), n, r, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), Jn(e, t, n, o));
};
function Ol(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Yn(t) && $(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return Yn(t) && X(s) ? !1 : t in e;
}
function El(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = ye(t);
  return Array.isArray(s) ? s.some((i) => ye(i) === n) : Object.keys(s).some((i) => ye(i) === n);
}
const xt = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return M(t) ? (s) => ns(t, s) : t;
};
function Al(e) {
  e.target.composing = !0;
}
function zn(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const ke = /* @__PURE__ */ Symbol("_assign");
function Xn(e, t, s) {
  return t && (e = e.trim()), s && (e = bs(e)), e;
}
const it = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, i) {
    e[ke] = xt(i);
    const r = n || i.props && i.props.type === "number";
    Ye(e, t ? "change" : "input", (o) => {
      o.target.composing || e[ke](Xn(e.value, s, r));
    }), (s || r) && Ye(e, "change", () => {
      e.value = Xn(e.value, s, r);
    }), t || (Ye(e, "compositionstart", Al), Ye(e, "compositionend", zn), Ye(e, "change", zn));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: i, number: r } }, o) {
    if (e[ke] = xt(o), e.composing) return;
    const l = (r || e.type === "number") && !/^0\d/.test(e.value) ? bs(e.value) : e.value, c = t ?? "";
    if (l === c)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (n && t === s || i && e.value.trim() === c) || (e.value = c);
  }
}, Pl = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, s) {
    e[ke] = xt(s), Ye(e, "change", () => {
      const n = e._modelValue, i = Bt(e), r = e.checked, o = e[ke];
      if (M(n)) {
        const l = tn(n, i), c = l !== -1;
        if (r && !c)
          o(n.concat(i));
        else if (!r && c) {
          const d = [...n];
          d.splice(l, 1), o(d);
        }
      } else if (St(n)) {
        const l = new Set(n);
        r ? l.add(i) : l.delete(i), o(l);
      } else
        o(cr(e, r));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: Zn,
  beforeUpdate(e, t, s) {
    e[ke] = xt(s), Zn(e, t, s);
  }
};
function Zn(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let i;
  if (M(t))
    i = tn(t, n.props.value) > -1;
  else if (St(t))
    i = t.has(n.props.value);
  else {
    if (t === s) return;
    i = wt(t, cr(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const Ml = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const i = St(t);
    Ye(e, "change", () => {
      const r = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? bs(Bt(o)) : Bt(o)
      );
      e[ke](
        e.multiple ? i ? new Set(r) : r : r[0]
      ), e._assigning = !0, Oi(() => {
        e._assigning = !1;
      });
    }), e[ke] = xt(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    Qn(e, t);
  },
  beforeUpdate(e, t, s) {
    e[ke] = xt(s);
  },
  updated(e, { value: t }) {
    e._assigning || Qn(e, t);
  }
};
function Qn(e, t) {
  const s = e.multiple, n = M(t);
  if (!(s && !n && !St(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = Bt(o);
      if (s)
        if (n) {
          const c = typeof l;
          c === "string" || c === "number" ? o.selected = t.some((d) => String(d) === String(l)) : o.selected = tn(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (wt(Bt(o), t)) {
        e.selectedIndex !== i && (e.selectedIndex = i);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function Bt(e) {
  return "_value" in e ? e._value : e.value;
}
function cr(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
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
  exact: (e, t) => Il.some((s) => e[`${s}Key`] && !t.includes(s))
}, $l = (e, t) => {
  if (!e) return e;
  const s = e._withMods || (e._withMods = {}), n = t.join(".");
  return s[n] || (s[n] = ((i, ...r) => {
    for (let o = 0; o < t.length; o++) {
      const l = Rl[t[o]];
      if (l && l(i, t)) return;
    }
    return e(i, ...r);
  }));
}, Fl = /* @__PURE__ */ ie({ patchProp: Tl }, cl);
let ei;
function Vl() {
  return ei || (ei = Uo(Fl));
}
const Dl = ((...e) => {
  const t = Vl().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const i = Nl(n);
    if (!i) return;
    const r = t._component;
    !$(r) && !r.render && !r.template && (r.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
    const o = s(i, !1, Hl(i));
    return i instanceof Element && (i.removeAttribute("v-cloak"), i.setAttribute("data-v-app", "")), o;
  }, t;
});
function Hl(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function Nl(e) {
  return X(e) ? document.querySelector(e) : e;
}
let zs = null;
function jl(e) {
  zs = e ?? null;
}
function Q(e, t) {
  return zs ? zs(e, t) : e;
}
const Ul = ["value", "placeholder"], Ll = /* @__PURE__ */ Ze({
  __name: "CredentialSlot",
  props: {
    api: {},
    modelValue: {}
  },
  emits: ["update:modelValue"],
  setup(e, { emit: t }) {
    const s = e, n = t, i = /* @__PURE__ */ ls(null), r = /* @__PURE__ */ ls(!1);
    let o = null;
    return Di(() => {
      !i.value || !s.api.mountCredentialField || (o = s.api.mountCredentialField(i.value, {
        value: s.modelValue,
        noneLabel: "(none — use fields below)",
        onChange: (l) => n("update:modelValue", l)
      }), r.value = !0);
    }), rs(() => s.modelValue, (l) => o == null ? void 0 : o.setValue(l)), Hi(() => o == null ? void 0 : o.destroy()), (l, c) => (ee(), me("div", {
      ref_key: "el",
      ref: i,
      class: "cred-slot"
    }, [
      r.value ? bt("", !0) : (ee(), me("input", {
        key: 0,
        value: e.modelValue,
        class: "w-260",
        spellcheck: "false",
        placeholder: Y(Q)("secret:<scope>:<entry>"),
        onInput: c[0] || (c[0] = (d) => n("update:modelValue", d.target.value))
      }, null, 40, Ul))
    ], 512));
  }
}), fr = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, i] of t)
    s[n] = i;
  return s;
}, Kl = /* @__PURE__ */ fr(Ll, [["__scopeId", "data-v-17c11f13"]]), kl = ["disabled"], Bl = /* @__PURE__ */ Ze({
  __name: "AddButton",
  props: {
    label: { default: "Add" },
    disabled: { type: Boolean, default: !1 }
  },
  emits: ["click"],
  setup(e) {
    return (t, s) => (ee(), me("button", {
      type: "button",
      class: "btn ghost list-add",
      disabled: e.disabled,
      onClick: s[0] || (s[0] = (n) => t.$emit("click", n))
    }, [
      s[1] || (s[1] = F("span", {
        class: "list-add-glyph",
        "aria-hidden": "true"
      }, "＋", -1)),
      ze(t.$slots, "default", {}, () => [
        ct(z(e.label), 1)
      ])
    ], 8, kl));
  }
}), Wl = { class: "list-panel" }, Jl = {
  key: 0,
  class: "list-empty"
}, ql = /* @__PURE__ */ Ze({
  __name: "ListPanel",
  props: {
    empty: { type: Boolean },
    emptyText: { default: "Nothing here yet." },
    addLabel: { default: "" },
    addDisabled: { type: Boolean, default: !1 }
  },
  emits: ["add"],
  setup(e) {
    return (t, s) => (ee(), me("div", Wl, [
      e.empty ? (ee(), me("div", Jl, [
        ze(t.$slots, "empty", {}, () => [
          ct(z(e.emptyText), 1)
        ])
      ])) : bt("", !0),
      ze(t.$slots, "default"),
      e.addLabel ? (ee(), Xe(Bl, {
        key: 1,
        label: e.addLabel,
        disabled: e.addDisabled,
        onClick: s[0] || (s[0] = (n) => t.$emit("add"))
      }, null, 8, ["label", "disabled"])) : bt("", !0)
    ]));
  }
}), Gl = ["disabled", "title"], ur = /* @__PURE__ */ Ze({
  __name: "IconGlyphButton",
  props: {
    title: {},
    variant: { default: "plain" },
    disabled: { type: Boolean, default: !1 },
    on: { type: Boolean, default: !1 }
  },
  emits: ["click"],
  setup(e) {
    return (t, s) => (ee(), me("button", {
      type: "button",
      class: Jt(["gbtn", [`gbtn-${e.variant}`, { on: e.on }]]),
      disabled: e.disabled,
      title: e.title,
      onClick: s[0] || (s[0] = $l((n) => t.$emit("click", n), ["stop"]))
    }, [
      ze(t.$slots, "default")
    ], 10, Gl));
  }
}), Yl = /* @__PURE__ */ Ze({
  __name: "ExpandButton",
  props: {
    open: { type: Boolean }
  },
  emits: ["update:open"],
  setup(e) {
    return (t, s) => (ee(), Xe(ur, {
      variant: "plain",
      on: e.open,
      title: e.open ? "Collapse" : "Expand",
      onClick: s[0] || (s[0] = (n) => t.$emit("update:open", !e.open))
    }, {
      default: gt(() => [
        ct(z(e.open ? "▾" : "▸"), 1)
      ]),
      _: 1
    }, 8, ["on", "title"]));
  }
}), zl = { class: "list-card-title" }, Xl = {
  key: 1,
  class: "list-card-summary"
}, Zl = { class: "list-card-actions" }, Ql = {
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
    const s = e, n = t;
    function i() {
      !s.noToggleOnClick && s.collapsible && n("update:open", !s.open);
    }
    return (r, o) => (ee(), me("div", {
      class: Jt(["list-card", { open: e.open }])
    }, [
      F("div", {
        class: "list-card-head",
        onClick: i
      }, [
        e.collapsible ? (ee(), Xe(Yl, {
          key: 0,
          open: e.open,
          "onUpdate:open": o[0] || (o[0] = (l) => n("update:open", l))
        }, null, 8, ["open"])) : bt("", !0),
        F("div", zl, [
          ze(r.$slots, "title", { open: e.open }, () => [
            ct(z(e.open && e.titleOpen ? e.titleOpen : e.title), 1)
          ])
        ]),
        !e.open && (e.summary || r.$slots.summary) ? (ee(), me("div", Xl, [
          ze(r.$slots, "summary", {}, () => [
            ct(z(e.summary), 1)
          ])
        ])) : bt("", !0),
        F("div", Zl, [
          ze(r.$slots, "actions", { open: e.open })
        ])
      ]),
      e.open && r.$slots.body ? (ee(), me("div", Ql, [
        ze(r.$slots, "body")
      ])) : bt("", !0)
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
    return (t, s) => (ee(), Xe(ur, {
      variant: "danger",
      title: e.title,
      disabled: e.disabled,
      onClick: s[0] || (s[0] = (n) => t.$emit("click", n))
    }, {
      default: gt(() => [...s[1] || (s[1] = [
        ct("🗑︎", -1)
      ])]),
      _: 1
    }, 8, ["title", "disabled"]));
  }
}), sc = { class: "ssh-set" }, nc = { class: "muted" }, ic = { class: "row" }, rc = { class: "muted" }, oc = { value: "" }, lc = ["value"], cc = { class: "muted" }, fc = { class: "row" }, uc = { class: "muted w-label" }, ac = ["onUpdate:modelValue"], dc = { class: "muted" }, pc = ["onUpdate:modelValue", "placeholder"], hc = { class: "muted" }, gc = ["onUpdate:modelValue"], mc = { class: "row" }, _c = { class: "muted w-label" }, bc = { class: "row" }, yc = { class: "muted w-label" }, vc = ["onUpdate:modelValue", "placeholder"], xc = { class: "muted" }, Sc = ["onUpdate:modelValue", "placeholder"], wc = { class: "row" }, Cc = { class: "muted w-label" }, Tc = ["onUpdate:modelValue", "placeholder"], Oc = { class: "row" }, Ec = { class: "chk" }, Ac = ["onUpdate:modelValue"], Pc = { class: "muted" }, Mc = { class: "row" }, Ic = ["disabled", "onClick"], Rc = { class: "muted" }, $c = /* @__PURE__ */ Ze({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function i(v, O) {
      return {
        key: n++,
        name: v,
        host: O.host || "",
        port: O.port || 22,
        user: O.user || "",
        credential: O.credential || "",
        password: O.password || "",
        keyFile: O.key_file || "",
        keyPassphrase: O.key_passphrase || "",
        description: O.description || "",
        allowWrite: !!O.allow_write,
        testing: !1,
        testStatus: ""
      };
    }
    function r(v) {
      return {
        host: v.host || void 0,
        port: v.port === 22 ? void 0 : v.port,
        user: v.user || void 0,
        credential: v.credential || void 0,
        // Legacy single-value pointers stay untouched if they were in the blob and no credential is set.
        password: v.credential ? void 0 : v.password || void 0,
        key_file: v.keyFile || void 0,
        key_passphrase: v.credential ? void 0 : v.keyPassphrase || void 0,
        description: v.description || void 0,
        allow_write: v.allowWrite || void 0
      };
    }
    const o = (() => {
      try {
        return JSON.parse(s.api.getJson() || "null") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ ls(o.default_host || ""), c = /* @__PURE__ */ ls(o.timeout_seconds || 20), d = /* @__PURE__ */ Ut(
      Object.entries(o.hosts || {}).map(([v, O]) => i(v, O))
    ), a = /* @__PURE__ */ Ut(/* @__PURE__ */ new Set()), h = (v) => a.has(v);
    function T(v) {
      a.delete(v) || a.add(v);
    }
    function E(v) {
      const O = v.host || "(no address)", y = v.port && v.port !== 22 ? `${O}:${v.port}` : O;
      return v.user ? `${v.user}@${y}` : y;
    }
    function D() {
      const v = i(`host${d.length + 1}`, {});
      d.push(v), a.add(v.key);
    }
    function R(v) {
      const [O] = d.splice(v, 1);
      O && a.delete(O.key);
    }
    async function G(v) {
      v.testing = !0, v.testStatus = "Connecting…";
      try {
        const O = await s.api.invoke("plugin.action", {
          pluginId: "ssh",
          action: "testHost",
          valueJson: JSON.stringify({
            host: v.host,
            port: v.port,
            user: v.user || void 0,
            credential: v.credential || void 0,
            password: v.credential ? void 0 : v.password || void 0,
            keyFile: v.keyFile || void 0
          })
        });
        if (O.ok && O.resultJson) {
          const y = JSON.parse(O.resultJson);
          v.testStatus = y.message;
        } else
          v.testStatus = "Failed: " + (O.error || "unknown error");
      } catch (O) {
        v.testStatus = "Failed: " + (O instanceof Error ? O.message : String(O));
      } finally {
        v.testing = !1;
      }
    }
    function B() {
      const v = {
        default_host: l.value || void 0,
        timeout_seconds: c.value || 20,
        hosts: Object.fromEntries(
          d.filter((O) => O.name.trim()).map((O) => [O.name.trim(), r(O)])
        )
      };
      return JSON.stringify(v);
    }
    return t({ toJson: B }), (v, O) => (ee(), me("div", sc, [
      F("div", nc, z(Y(Q)("Named SSH hosts available to the agent and the terminal. Passwords/keys live in the secret store (Settings → Secrets); a host only references an entry by name.")), 1),
      F("div", ic, [
        F("label", null, [
          F("span", rc, z(Y(Q)("Default host")), 1),
          Ne(F("select", {
            "onUpdate:modelValue": O[0] || (O[0] = (y) => l.value = y)
          }, [
            F("option", oc, z(Y(Q)("(none)")), 1),
            (ee(!0), me(pe, null, An(d, (y) => (ee(), me("option", {
              key: y.key,
              value: y.name
            }, z(y.name), 9, lc))), 128))
          ], 512), [
            [Ml, l.value]
          ])
        ]),
        F("label", null, [
          F("span", cc, z(Y(Q)("Timeout, s")), 1),
          Ne(F("input", {
            "onUpdate:modelValue": O[1] || (O[1] = (y) => c.value = y),
            type: "number",
            min: "5",
            max: "120",
            class: "w-70"
          }, null, 512), [
            [
              it,
              c.value,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      _e(ql, {
        empty: !d.length,
        "empty-text": Y(Q)("No hosts yet."),
        "add-label": Y(Q)("Host"),
        onAdd: D
      }, {
        default: gt(() => [
          (ee(!0), me(pe, null, An(d, (y, Z) => (ee(), Xe(ec, {
            key: y.key,
            open: h(y.key),
            title: y.name || Y(Q)("(new host)"),
            summary: E(y),
            "onUpdate:open": (j) => T(y.key)
          }, {
            actions: gt(() => [
              _e(tc, {
                onClick: (j) => R(Z)
              }, null, 8, ["onClick"])
            ]),
            body: gt(() => [
              F("div", fc, [
                F("span", uc, z(Y(Q)("Name")), 1),
                Ne(F("input", {
                  "onUpdate:modelValue": (j) => y.name = j,
                  class: "w-120",
                  spellcheck: "false"
                }, null, 8, ac), [
                  [it, y.name]
                ]),
                F("span", dc, z(Y(Q)("Host")), 1),
                Ne(F("input", {
                  "onUpdate:modelValue": (j) => y.host = j,
                  placeholder: Y(Q)("10.0.0.5 or box.local"),
                  class: "w-180",
                  spellcheck: "false"
                }, null, 8, pc), [
                  [it, y.host]
                ]),
                F("span", hc, z(Y(Q)("Port")), 1),
                Ne(F("input", {
                  "onUpdate:modelValue": (j) => y.port = j,
                  type: "number",
                  min: "1",
                  max: "65535",
                  class: "w-70"
                }, null, 8, gc), [
                  [
                    it,
                    y.port,
                    void 0,
                    { number: !0 }
                  ]
                ])
              ]),
              F("div", mc, [
                F("span", _c, z(Y(Q)("Credential")), 1),
                _e(Kl, {
                  api: e.api,
                  modelValue: y.credential,
                  "onUpdate:modelValue": (j) => y.credential = j
                }, null, 8, ["api", "modelValue", "onUpdate:modelValue"])
              ]),
              F("div", bc, [
                F("span", yc, z(Y(Q)("User")), 1),
                Ne(F("input", {
                  "onUpdate:modelValue": (j) => y.user = j,
                  placeholder: y.credential ? "(from credential)" : "login",
                  class: "w-120",
                  spellcheck: "false"
                }, null, 8, vc), [
                  [it, y.user]
                ]),
                F("span", xc, z(Y(Q)("Key file")), 1),
                Ne(F("input", {
                  "onUpdate:modelValue": (j) => y.keyFile = j,
                  placeholder: Y(Q)("optional: C:\\Users\\me\\.ssh\\id_ed25519"),
                  class: "w-260",
                  spellcheck: "false"
                }, null, 8, Sc), [
                  [it, y.keyFile]
                ])
              ]),
              F("div", wc, [
                F("span", Cc, z(Y(Q)("Description")), 1),
                Ne(F("input", {
                  "onUpdate:modelValue": (j) => y.description = j,
                  placeholder: Y(Q)("Shown to the AI — what this host is"),
                  class: "grow"
                }, null, 8, Tc), [
                  [it, y.description]
                ])
              ]),
              F("div", Oc, [
                F("label", Ec, [
                  Ne(F("input", {
                    "onUpdate:modelValue": (j) => y.allowWrite = j,
                    type: "checkbox"
                  }, null, 8, Ac), [
                    [Pl, y.allowWrite]
                  ]),
                  F("span", null, z(Y(Q)("Allow the agent to write (apt, systemctl, edit files…)")), 1)
                ]),
                F("span", Pc, z(Y(Q)("off = read-only guard blocks mutating commands; human terminal is never guarded")), 1)
              ]),
              F("div", Mc, [
                F("button", {
                  type: "button",
                  disabled: y.testing,
                  onClick: (j) => G(y)
                }, z(Y(Q)("Test connection")), 9, Ic),
                F("span", Rc, z(y.testStatus), 1)
              ])
            ]),
            _: 2
          }, 1032, ["open", "title", "summary", "onUpdate:open"]))), 128))
        ]),
        _: 1
      }, 8, ["empty", "empty-text", "add-label"])
    ]));
  }
}), Fc = /* @__PURE__ */ fr($c, [["__scopeId", "data-v-595f0b9f"]]);
function Dc(e, t) {
  var i;
  jl((i = t.t) == null ? void 0 : i.bind(t));
  let s = Dl(Fc, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toJson(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  Dc as mount
};
