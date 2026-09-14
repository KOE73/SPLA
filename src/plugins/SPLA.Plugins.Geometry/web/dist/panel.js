(function(){"use strict";try{if(typeof document<"u"){var a=document.createElement("style");a.appendChild(document.createTextNode(".geom[data-v-96f138f3]{display:flex;flex-direction:column;gap:8px;height:100%;min-height:0;padding:8px}.head[data-v-96f138f3]{display:flex;align-items:baseline;gap:8px}.title[data-v-96f138f3]{font-weight:600}.muted[data-v-96f138f3]{color:var(--muted);font-size:var(--fs-xs)}.empty[data-v-96f138f3]{padding:16px 4px;color:var(--muted)}.stage[data-v-96f138f3]{flex:1;min-height:120px;display:grid;place-items:center;overflow:hidden;background:var(--bg);border:1px solid var(--border);border-radius:4px}.stage img[data-v-96f138f3]{display:block;max-width:100%;max-height:100%;object-fit:contain}.strip[data-v-96f138f3]{display:flex;align-items:center;gap:6px;overflow-x:auto;padding-bottom:2px}.thumb[data-v-96f138f3]{position:relative;flex:0 0 auto;width:86px;height:64px;padding:0;overflow:hidden;background:var(--bg);border:1px solid var(--border);border-radius:4px;cursor:pointer}.thumb img[data-v-96f138f3]{width:100%;height:100%;object-fit:contain}.thumb.sel[data-v-96f138f3]{outline:2px solid var(--accent, #4a9);outline-offset:-2px}.thumb.live[data-v-96f138f3]{border-color:var(--accent, #4a9)}.thumb .ph[data-v-96f138f3]{color:var(--muted)}.thumb .no[data-v-96f138f3]{position:absolute;right:2px;bottom:1px;padding:0 3px;color:var(--text);background:var(--panel);border-radius:3px;font-size:var(--fs-xs)}.latest[data-v-96f138f3]{flex:0 0 auto;height:24px;padding:0 8px;color:var(--text);background:transparent;border:1px solid var(--border);border-radius:4px;cursor:pointer}.facts[data-v-96f138f3]{display:flex;flex-direction:column;gap:4px;font-size:var(--fs-sm)}.row[data-v-96f138f3]{display:flex;align-items:baseline;gap:8px;flex-wrap:wrap}.objects[data-v-96f138f3]{width:100%;border-collapse:collapse}.objects td[data-v-96f138f3]{padding:1px 6px 1px 0;vertical-align:baseline}.objects tr.away[data-v-96f138f3]{opacity:.55}.name[data-v-96f138f3]{font-weight:600}.ok[data-v-96f138f3]{color:var(--muted)}.edit[data-v-96f138f3]{color:var(--accent, #4a9)}code[data-v-96f138f3]{font-size:var(--fs-xs)}")),document.head.appendChild(a)}}catch(t){console.error("vite-plugin-css-injected-by-js",t)}})();
/**
* @vue/shared v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Ws(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const B = {}, lt = [], Re = () => {
}, Wn = () => !1, is = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), os = (e) => e.startsWith("onUpdate:"), se = Object.assign, Vs = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, Xr = Object.prototype.hasOwnProperty, N = (e, t) => Xr.call(e, t), R = Array.isArray, Ge = (e) => jt(e) === "[object Map]", Xt = (e) => jt(e) === "[object Set]", dn = (e) => jt(e) === "[object Date]", F = (e) => typeof e == "function", Y = (e) => typeof e == "string", Fe = (e) => typeof e == "symbol", K = (e) => e !== null && typeof e == "object", Vn = (e) => (K(e) || F(e)) && F(e.then) && F(e.catch), Bn = Object.prototype.toString, jt = (e) => Bn.call(e), Zr = (e) => jt(e).slice(8, -1), kn = (e) => jt(e) === "[object Object]", Bs = (e) => Y(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, xt = /* @__PURE__ */ Ws(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), ls = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, Qr = /-\w/g, me = ls(
  (e) => e.replace(Qr, (t) => t.slice(1).toUpperCase())
), ei = /\B([A-Z])/g, nt = ls(
  (e) => e.replace(ei, "-$1").toLowerCase()
), qn = ls((e) => e.charAt(0).toUpperCase() + e.slice(1)), bs = ls(
  (e) => e ? `on${qn(e)}` : ""
), Ie = (e, t) => !Object.is(e, t), ys = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Gn = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, ti = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let hn;
const cs = () => hn || (hn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function ks(e) {
  if (R(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], r = Y(n) ? ii(n) : ks(n);
      if (r)
        for (const i in r)
          t[i] = r[i];
    }
    return t;
  } else if (Y(e) || K(e))
    return e;
}
const si = /;(?![^(]*\))/g, ni = /:([^]+)/, ri = /\/\*[^]*?\*\//g;
function ii(e) {
  const t = {};
  return e.replace(ri, "").split(si).forEach((s) => {
    if (s) {
      const n = s.split(ni);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function ct(e) {
  let t = "";
  if (Y(e))
    t = e;
  else if (R(e))
    for (let s = 0; s < e.length; s++) {
      const n = ct(e[s]);
      n && (t += n + " ");
    }
  else if (K(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const oi = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", li = /* @__PURE__ */ Ws(oi);
function Jn(e) {
  return !!e || e === "";
}
function ci(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = fs(e[n], t[n]);
  return s;
}
function pn(e, t) {
  if (e.size !== t.size) return !1;
  const s = Array.from(t), n = new Uint8Array(s.length);
  for (const r of e) {
    let i = -1;
    for (let o = 0; o < s.length; o++)
      if (!n[o] && fs(r, s[o])) {
        i = o;
        break;
      }
    if (i < 0) return !1;
    n[i] = 1;
  }
  return !0;
}
function fs(e, t) {
  if (e === t) return !0;
  let s = dn(e), n = dn(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Fe(e), n = Fe(t), s || n)
    return e === t;
  if (s = R(e), n = R(t), s || n)
    return s && n ? ci(e, t) : !1;
  if (s = K(e), n = K(t), s || n) {
    if (!s || !n)
      return !1;
    if (s = Ge(e), n = Ge(t), s || n || (s = Xt(e), n = Xt(t), s || n))
      return s && n ? pn(e, t) : !1;
    const r = Object.keys(e).length, i = Object.keys(t).length;
    if (r !== i)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), f = t.hasOwnProperty(o);
      if (l && !f || !l && f || !fs(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
const Yn = (e) => !!(e && e.__v_isRef === !0), J = (e) => Y(e) ? e : e == null ? "" : R(e) || K(e) && (e.toString === Bn || !F(e.toString)) ? Yn(e) ? J(e.value) : JSON.stringify(e, zn, 2) : String(e), zn = (e, t) => Yn(t) ? zn(e, t.value) : Ge(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, r], i) => (s[vs(n, i) + " =>"] = r, s),
    {}
  )
} : Xt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => vs(s))
} : Fe(t) ? vs(t) : K(t) && !R(t) && !kn(t) ? String(t) : t, vs = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Fe(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let te;
class fi {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && te && (te.active ? (this.parent = te, this.index = (te.scopes || (te.scopes = [])).push(
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
      if (this.scopes) {
        const n = this.scopes.slice();
        for (t = 0, s = n.length; t < s; t++)
          n[t].pause();
      }
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
      if (this.scopes) {
        const r = this.scopes.slice();
        for (t = 0, s = r.length; t < s; t++)
          r[t].resume();
      }
      const n = this.effects.slice();
      for (t = 0, s = n.length; t < s; t++)
        n[t].resume();
    }
  }
  run(t) {
    if (this._active) {
      const s = te;
      try {
        return te = this, t();
      } finally {
        te = s;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = te, te = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (te === this)
        te = this.prevScope;
      else {
        let t = te;
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
        const r = this.scopes.slice();
        for (s = 0, n = r.length; s < n; s++)
          r[s].stop(!0);
        this.scopes.length = 0;
      }
      if (!this.detached && this.parent && !t) {
        const r = this.parent.scopes.pop();
        r && r !== this && (this.parent.scopes[this.index] = r, r.index = this.index);
      }
      this.parent = void 0;
    }
  }
}
function ui() {
  return te;
}
let V;
const xs = /* @__PURE__ */ new WeakSet();
class Xn {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, te && (te.active ? te.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, xs.has(this) && (xs.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || Qn(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, gn(this), er(this);
    const t = V, s = be;
    V = this, be = !0;
    try {
      return this.fn();
    } finally {
      tr(this), V = t, be = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        Js(t);
      this.deps = this.depsTail = void 0, gn(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? xs.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Ms(this) && this.run();
  }
  get dirty() {
    return Ms(this);
  }
}
let Zn = 0, St, wt;
function Qn(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = wt, wt = e;
    return;
  }
  e.next = St, St = e;
}
function qs() {
  Zn++;
}
function Gs() {
  if (--Zn > 0)
    return;
  if (wt) {
    let t = wt;
    for (wt = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; St; ) {
    let t = St;
    for (St = void 0; t; ) {
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
function er(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function tr(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const r = n.prevDep;
    n.version === -1 ? (n === s && (s = r), Js(n), ai(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = r;
  }
  e.deps = t, e.depsTail = s;
}
function Ms(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (sr(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function sr(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Ot) || (e.globalVersion = Ot, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Ms(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = V, n = be;
  V = e, be = !0;
  try {
    er(e);
    const r = e.fn(e._value);
    (t.version === 0 || Ie(r, e._value)) && (e.flags |= 128, e._value = r, t.version++);
  } catch (r) {
    throw t.version++, r;
  } finally {
    V = s, be = n, tr(e), e.flags &= -3;
  }
}
function Js(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: r } = e;
  if (n && (n.nextSub = r, e.prevSub = void 0), r && (r.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let i = s.computed.deps; i; i = i.nextDep)
      Js(i, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function ai(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let be = !0;
const nr = [];
function Le() {
  nr.push(be), be = !1;
}
function Ke() {
  const e = nr.pop();
  be = e === void 0 ? !0 : e;
}
function gn(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const s = V;
    V = void 0;
    try {
      t();
    } finally {
      V = s;
    }
  }
}
let Ot = 0;
class di {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class Ys {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!V || !be || V === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== V)
      s = this.activeLink = new di(V, this), V.deps ? (s.prevDep = V.depsTail, V.depsTail.nextDep = s, V.depsTail = s) : V.deps = V.depsTail = s, rr(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = V.depsTail, s.nextDep = void 0, V.depsTail.nextDep = s, V.depsTail = s, V.deps === s && (V.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, Ot++, this.notify(t);
  }
  notify(t) {
    qs();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      Gs();
    }
  }
}
function rr(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let n = t.deps; n; n = n.nextDep)
        rr(n);
    }
    const s = e.dep.subs;
    s !== e && (e.prevSub = s, s && (s.nextSub = e)), e.dep.subs = e;
  }
}
const Rs = /* @__PURE__ */ new WeakMap(), et = /* @__PURE__ */ Symbol(
  ""
), Fs = /* @__PURE__ */ Symbol(
  ""
), At = /* @__PURE__ */ Symbol(
  ""
);
function ne(e, t, s) {
  if (be && V) {
    let n = Rs.get(e);
    n || Rs.set(e, n = /* @__PURE__ */ new Map());
    let r = n.get(s);
    r || (n.set(s, r = new Ys()), r.map = n, r.key = s), r.track();
  }
}
function He(e, t, s, n, r, i) {
  const o = Rs.get(e);
  if (!o) {
    Ot++;
    return;
  }
  const l = (f) => {
    f && f.trigger();
  };
  if (qs(), t === "clear")
    o.forEach(l);
  else {
    const f = R(e), d = f && Bs(s);
    if (f && s === "length") {
      const a = Number(n);
      o.forEach((p, C) => {
        (C === "length" || C === At || !Fe(C) && C >= a) && l(p);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(At)), t) {
        case "add":
          f ? d && l(o.get("length")) : (l(o.get(et)), Ge(e) && l(o.get(Fs)));
          break;
        case "delete":
          f || (l(o.get(et)), Ge(e) && l(o.get(Fs)));
          break;
        case "set":
          Ge(e) && l(o.get(et));
          break;
      }
  }
  Gs();
}
function rt(e) {
  const t = /* @__PURE__ */ H(e);
  return t === e ? t : (ne(t, "iterate", At), /* @__PURE__ */ ge(e) ? t : t.map(ye));
}
function us(e) {
  return ne(e = /* @__PURE__ */ H(e), "iterate", At), e;
}
function Ae(e, t) {
  return /* @__PURE__ */ Ue(e) ? at(/* @__PURE__ */ tt(e) ? ye(t) : t) : ye(t);
}
const hi = {
  __proto__: null,
  [Symbol.iterator]() {
    return Ss(this, Symbol.iterator, (e) => Ae(this, e));
  },
  concat(...e) {
    return rt(this).concat(
      ...e.map((t) => R(t) ? rt(t) : t)
    );
  },
  entries() {
    return Ss(this, "entries", (e) => (e[1] = Ae(this, e[1]), e));
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
      (s) => s.map((n) => Ae(this, n)),
      arguments
    );
  },
  find(e, t) {
    return je(
      this,
      "find",
      e,
      t,
      (s) => Ae(this, s),
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
      (s) => Ae(this, s),
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
    return ws(this, "includes", e);
  },
  indexOf(...e) {
    return ws(this, "indexOf", e);
  },
  join(e) {
    return rt(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return ws(this, "lastIndexOf", e);
  },
  map(e, t) {
    return je(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return mt(this, "pop");
  },
  push(...e) {
    return mt(this, "push", e);
  },
  reduce(e, ...t) {
    return _n(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return _n(this, "reduceRight", e, t);
  },
  shift() {
    return mt(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return je(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return mt(this, "splice", e);
  },
  toReversed() {
    return rt(this).toReversed();
  },
  toSorted(e) {
    return rt(this).toSorted(e);
  },
  toSpliced(...e) {
    return rt(this).toSpliced(...e);
  },
  unshift(...e) {
    return mt(this, "unshift", e);
  },
  values() {
    return Ss(this, "values", (e) => Ae(this, e));
  }
};
function Ss(e, t, s) {
  const n = us(e), r = n[t]();
  return n !== e && !/* @__PURE__ */ ge(e) && (r._next = r.next, r.next = () => {
    const i = r._next();
    return i.done || (i.value = s(i.value)), i;
  }), r;
}
const pi = Array.prototype;
function je(e, t, s, n, r, i) {
  const o = us(e), l = o !== e && !/* @__PURE__ */ ge(e), f = o[t];
  if (f !== pi[t]) {
    const p = f.apply(e, i);
    return l ? ye(p) : p;
  }
  let d = s;
  o !== e && (l ? d = function(p, C) {
    return s.call(this, Ae(e, p), C, e);
  } : s.length > 2 && (d = function(p, C) {
    return s.call(this, p, C, e);
  }));
  const a = f.call(o, d, n);
  return l && r ? r(a) : a;
}
function _n(e, t, s, n) {
  const r = us(e), i = r !== e && !/* @__PURE__ */ ge(e);
  let o = s, l = !1;
  r !== e && (i ? (l = n.length === 0, o = function(d, a, p) {
    return l && (l = !1, d = Ae(e, d)), s.call(this, d, Ae(e, a), p, e);
  }) : s.length > 3 && (o = function(d, a, p) {
    return s.call(this, d, a, p, e);
  }));
  const f = r[t](o, ...n);
  return l ? Ae(e, f) : f;
}
function ws(e, t, s) {
  const n = /* @__PURE__ */ H(e);
  ne(n, "iterate", At);
  const r = n[t](...s);
  return (r === -1 || r === !1) && /* @__PURE__ */ Zs(s[0]) ? (s[0] = /* @__PURE__ */ H(s[0]), n[t](...s)) : r;
}
function mt(e, t, s = []) {
  Le(), qs();
  const n = (/* @__PURE__ */ H(e))[t].apply(e, s);
  return Gs(), Ke(), n;
}
const gi = /* @__PURE__ */ Ws("__proto__,__v_isRef,__isVue"), ir = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Fe)
);
function _i(e) {
  Fe(e) || (e = String(e));
  const t = /* @__PURE__ */ H(this);
  return ne(t, "has", e), t.hasOwnProperty(e);
}
class or {
  constructor(t = !1, s = !1) {
    this._isReadonly = t, this._isShallow = s;
  }
  get(t, s, n) {
    if (s === "__v_skip") return t.__v_skip;
    const r = this._isReadonly, i = this._isShallow;
    if (s === "__v_isReactive")
      return !r;
    if (s === "__v_isReadonly")
      return r;
    if (s === "__v_isShallow")
      return i;
    if (s === "__v_raw")
      return n === (r ? i ? Ei : ur : i ? fr : cr).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = R(t);
    if (!r) {
      let f;
      if (o && (f = hi[s]))
        return f;
      if (s === "hasOwnProperty")
        return _i;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ re(t) ? t : n
    );
    if ((Fe(s) ? ir.has(s) : gi(s)) || (r || ne(t, "get", s), i))
      return l;
    if (/* @__PURE__ */ re(l)) {
      const f = o && Bs(s) ? l : l.value;
      return r && K(f) ? /* @__PURE__ */ Ds(f) : f;
    }
    return K(l) ? r ? /* @__PURE__ */ Ds(l) : /* @__PURE__ */ Pt(l) : l;
  }
}
class lr extends or {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, r) {
    let i = t[s];
    const o = R(t) && Bs(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ Ue(i);
      if (!/* @__PURE__ */ ge(n) && !/* @__PURE__ */ Ue(n) && (i = /* @__PURE__ */ H(i), n = /* @__PURE__ */ H(n)), !o && /* @__PURE__ */ re(i) && !/* @__PURE__ */ re(n))
        return d || (i.value = n), !0;
    }
    const l = o ? Number(s) < t.length : N(t, s), f = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ re(t) ? t : r
    );
    return t === /* @__PURE__ */ H(r) && f && (l ? Ie(n, i) && He(t, "set", s, n) : He(t, "add", s, n)), f;
  }
  deleteProperty(t, s) {
    const n = N(t, s);
    t[s];
    const r = Reflect.deleteProperty(t, s);
    return r && n && He(t, "delete", s, void 0), r;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Fe(s) || !ir.has(s)) && ne(t, "has", s), n;
  }
  ownKeys(t) {
    return ne(
      t,
      "iterate",
      R(t) ? "length" : et
    ), Reflect.ownKeys(t);
  }
}
class mi extends or {
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
const bi = /* @__PURE__ */ new lr(), yi = /* @__PURE__ */ new mi(), vi = /* @__PURE__ */ new lr(!0);
const js = (e) => e, Wt = (e) => Reflect.getPrototypeOf(e);
function xi(e, t, s) {
  return function(...n) {
    const r = this.__v_raw, i = /* @__PURE__ */ H(r), o = Ge(i), l = e === "entries" || e === Symbol.iterator && o, f = e === "keys" && o, d = r[e](...n), a = s ? js : t ? at : ye;
    return !t && ne(
      i,
      "iterate",
      f ? Fs : et
    ), se(
      // inheriting all iterator properties
      Object.create(d),
      {
        // iterator protocol
        next() {
          const { value: p, done: C } = d.next();
          return C ? { value: p, done: C } : {
            value: l ? [a(p[0]), a(p[1])] : a(p),
            done: C
          };
        }
      }
    );
  };
}
function Vt(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function Si(e, t) {
  const s = {
    get(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ H(i), l = /* @__PURE__ */ H(r);
      e || (Ie(r, l) && ne(o, "get", r), ne(o, "get", l));
      const { has: f } = Wt(o), d = t ? js : e ? at : ye;
      if (f.call(o, r))
        return d(i.get(r));
      if (f.call(o, l))
        return d(i.get(l));
      i !== o && i.get(r);
    },
    get size() {
      const r = this.__v_raw;
      return !e && ne(/* @__PURE__ */ H(r), "iterate", et), r.size;
    },
    has(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ H(i), l = /* @__PURE__ */ H(r);
      return e || (Ie(r, l) && ne(o, "has", r), ne(o, "has", l)), r === l ? i.has(r) : i.has(r) || i.has(l);
    },
    forEach(r, i) {
      const o = this, l = o.__v_raw, f = /* @__PURE__ */ H(l), d = t ? js : e ? at : ye;
      return !e && ne(f, "iterate", et), l.forEach((a, p) => r.call(i, d(a), d(p), o));
    }
  };
  return se(
    s,
    e ? {
      add: Vt("add"),
      set: Vt("set"),
      delete: Vt("delete"),
      clear: Vt("clear")
    } : {
      add(r) {
        const i = /* @__PURE__ */ H(this), o = Wt(i), l = /* @__PURE__ */ H(r), f = !t && !/* @__PURE__ */ ge(r) && !/* @__PURE__ */ Ue(r) ? l : r;
        return o.has.call(i, f) || Ie(r, f) && o.has.call(i, r) || Ie(l, f) && o.has.call(i, l) || (i.add(f), He(i, "add", f, f)), this;
      },
      set(r, i) {
        !t && !/* @__PURE__ */ ge(i) && !/* @__PURE__ */ Ue(i) && (i = /* @__PURE__ */ H(i));
        const o = /* @__PURE__ */ H(this), { has: l, get: f } = Wt(o);
        let d = l.call(o, r);
        d || (r = /* @__PURE__ */ H(r), d = l.call(o, r));
        const a = f.call(o, r);
        return o.set(r, i), d ? Ie(i, a) && He(o, "set", r, i) : He(o, "add", r, i), this;
      },
      delete(r) {
        const i = /* @__PURE__ */ H(this), { has: o, get: l } = Wt(i);
        let f = o.call(i, r);
        f || (r = /* @__PURE__ */ H(r), f = o.call(i, r)), l && l.call(i, r);
        const d = i.delete(r);
        return f && He(i, "delete", r, void 0), d;
      },
      clear() {
        const r = /* @__PURE__ */ H(this), i = r.size !== 0, o = r.clear();
        return i && He(
          r,
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
  ].forEach((r) => {
    s[r] = xi(r, e, t);
  }), s;
}
function zs(e, t) {
  const s = Si(e, t);
  return (n, r, i) => r === "__v_isReactive" ? !e : r === "__v_isReadonly" ? e : r === "__v_raw" ? n : Reflect.get(
    N(s, r) && r in n ? s : n,
    r,
    i
  );
}
const wi = {
  get: /* @__PURE__ */ zs(!1, !1)
}, Ci = {
  get: /* @__PURE__ */ zs(!1, !0)
}, Ti = {
  get: /* @__PURE__ */ zs(!0, !1)
};
const cr = /* @__PURE__ */ new WeakMap(), fr = /* @__PURE__ */ new WeakMap(), ur = /* @__PURE__ */ new WeakMap(), Ei = /* @__PURE__ */ new WeakMap();
function Oi(e) {
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
function Pt(e) {
  return /* @__PURE__ */ Ue(e) ? e : Xs(
    e,
    !1,
    bi,
    wi,
    cr
  );
}
// @__NO_SIDE_EFFECTS__
function Ai(e) {
  return Xs(
    e,
    !1,
    vi,
    Ci,
    fr
  );
}
// @__NO_SIDE_EFFECTS__
function Ds(e) {
  return Xs(
    e,
    !0,
    yi,
    Ti,
    ur
  );
}
function Xs(e, t, s, n, r) {
  if (!K(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const i = r.get(e);
  if (i)
    return i;
  const o = Oi(Zr(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? n : s
  );
  return r.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function tt(e) {
  return /* @__PURE__ */ Ue(e) ? /* @__PURE__ */ tt(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function Ue(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function ge(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function Zs(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function H(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ H(t) : e;
}
function Pi(e) {
  return !N(e, "__v_skip") && Object.isExtensible(e) && Gn(e, "__v_skip", !0), e;
}
const ye = (e) => K(e) ? /* @__PURE__ */ Pt(e) : e, at = (e) => K(e) ? /* @__PURE__ */ Ds(e) : e;
// @__NO_SIDE_EFFECTS__
function re(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Ii(e) {
  return Mi(e, !1);
}
function Mi(e, t) {
  return /* @__PURE__ */ re(e) ? e : new Ri(e, t);
}
class Ri {
  constructor(t, s) {
    this.dep = new Ys(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ H(t), this._value = s ? t : ye(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ ge(t) || /* @__PURE__ */ Ue(t);
    t = n ? t : /* @__PURE__ */ H(t), Ie(t, s) && (this._rawValue = t, this._value = n ? t : ye(t), this.dep.trigger());
  }
}
function le(e) {
  return /* @__PURE__ */ re(e) ? e.value : e;
}
const Fi = {
  get: (e, t, s) => t === "__v_raw" ? e : le(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const r = e[t];
    return /* @__PURE__ */ re(r) && !/* @__PURE__ */ re(s) ? (r.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function ar(e) {
  return /* @__PURE__ */ tt(e) ? e : new Proxy(e, Fi);
}
class ji {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Ys(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Ot - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    V !== this)
      return Qn(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return sr(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function Di(e, t, s = !1) {
  let n, r;
  return F(e) ? n = e : (n = e.get, r = e.set), new ji(n, r, s);
}
const Bt = {}, Zt = /* @__PURE__ */ new WeakMap();
let Qe;
function $i(e, t = !1, s = Qe) {
  if (s) {
    let n = Zt.get(s);
    n || Zt.set(s, n = []), n.push(e);
  }
}
function Hi(e, t, s = B) {
  const { immediate: n, deep: r, once: i, scheduler: o, augmentJob: l, call: f } = s, d = (w) => r ? w : /* @__PURE__ */ ge(w) || r === !1 || r === 0 ? qe(w, 1) : qe(w);
  let a, p, C, E, $ = !1, M = !1;
  if (/* @__PURE__ */ re(e) ? (p = () => e.value, $ = /* @__PURE__ */ ge(e)) : /* @__PURE__ */ tt(e) ? (p = () => d(e), $ = !0) : R(e) ? (M = !0, $ = e.some((w) => /* @__PURE__ */ tt(w) || /* @__PURE__ */ ge(w)), p = () => e.map((w) => {
    if (/* @__PURE__ */ re(w))
      return w.value;
    if (/* @__PURE__ */ tt(w))
      return d(w);
    if (F(w))
      return f ? f(w, 2) : w();
  })) : F(e) ? t ? p = f ? () => f(e, 2) : e : p = () => {
    if (C) {
      Le();
      try {
        C();
      } finally {
        Ke();
      }
    }
    const w = Qe;
    Qe = a;
    try {
      return f ? f(e, 3, [E]) : e(E);
    } finally {
      Qe = w;
    }
  } : p = Re, t && r) {
    const w = p, G = r === !0 ? 1 / 0 : r;
    p = () => qe(w(), G);
  }
  const k = ui(), T = () => {
    a.stop(), k && k.active && Vs(k.effects, a);
  };
  if (i && t) {
    const w = t;
    t = (...G) => {
      const _e = w(...G);
      return T(), _e;
    };
  }
  let A = M ? new Array(e.length).fill(Bt) : Bt;
  const j = (w) => {
    if (!(!(a.flags & 1) || !a.dirty && !w))
      if (t) {
        const G = a.run();
        if (w || r || $ || (M ? G.some((_e, xe) => Ie(_e, A[xe])) : Ie(G, A))) {
          C && C();
          const _e = Qe;
          Qe = a;
          try {
            const xe = [
              G,
              // pass undefined as the old value when it's changed for the first time
              A === Bt ? void 0 : M && A[0] === Bt ? [] : A,
              E
            ];
            A = G, f ? f(t, 3, xe) : (
              // @ts-expect-error
              t(...xe)
            );
          } finally {
            Qe = _e;
          }
        }
      } else
        a.run();
  };
  return l && l(j), a = new Xn(p), a.scheduler = o ? () => o(j, !1) : j, E = (w) => $i(w, !1, a), C = a.onStop = () => {
    const w = Zt.get(a);
    if (w) {
      if (f)
        f(w, 4);
      else
        for (const G of w) G();
      Zt.delete(a);
    }
  }, t ? n ? j(!0) : A = a.run() : o ? o(j.bind(null, !0), !0) : a.run(), T.pause = a.pause.bind(a), T.resume = a.resume.bind(a), T.stop = T, T;
}
function qe(e, t = 1 / 0, s) {
  if (t <= 0 || !K(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ re(e))
    qe(e.value, t, s);
  else if (R(e))
    for (let n = 0; n < e.length; n++)
      qe(e[n], t, s);
  else if (Xt(e) || Ge(e))
    e.forEach((n) => {
      qe(n, t, s);
    });
  else if (kn(e)) {
    for (const n in e)
      qe(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && qe(e[n], t, s);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function Dt(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (r) {
    as(r, t, s);
  }
}
function ve(e, t, s, n) {
  if (F(e)) {
    const r = Dt(e, t, s, n);
    return r && Vn(r) && r.catch((i) => {
      as(i, t, s);
    }), r;
  }
  if (R(e)) {
    const r = [];
    for (let i = 0; i < e.length; i++)
      r.push(ve(e[i], t, s, n));
    return r;
  }
}
function as(e, t, s, n = !0) {
  const r = t ? t.vnode : null, { errorHandler: i, throwUnhandledErrorInProduction: o } = t && t.appContext.config || B;
  if (t) {
    let l = t.parent;
    const f = t.proxy, d = `https://vuejs.org/error-reference/#runtime-${s}`;
    for (; l; ) {
      const a = l.ec;
      if (a) {
        for (let p = 0; p < a.length; p++)
          if (a[p](e, f, d) === !1)
            return;
      }
      l = l.parent;
    }
    if (i) {
      Le(), Dt(i, null, 10, [
        e,
        f,
        d
      ]), Ke();
      return;
    }
  }
  Ni(e, s, r, n, o);
}
function Ni(e, t, s, n = !0, r = !1) {
  if (r)
    throw e;
  console.error(e);
}
const fe = [];
let Oe = -1;
const ft = [];
let ke = null, ot = 0;
const dr = /* @__PURE__ */ Promise.resolve();
let Qt = null;
function Li(e) {
  const t = Qt || dr;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Ki(e) {
  let t = Oe + 1, s = fe.length;
  for (; t < s; ) {
    const n = t + s >>> 1, r = fe[n], i = It(r);
    i < e || i === e && r.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function Qs(e) {
  if (!(e.flags & 1)) {
    const t = It(e), s = fe[fe.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= It(s) ? fe.push(e) : fe.splice(Ki(t), 0, e), e.flags |= 1, hr();
  }
}
function hr() {
  Qt || (Qt = dr.then(gr));
}
function Ui(e) {
  if (!R(e))
    ke && e.id === -1 ? ke.splice(ot + 1, 0, e) : e.flags & 1 || (ft.push(e), e.flags |= 1);
  else
    for (let t = 0; t < e.length; t++)
      ft.push(e[t]);
  hr();
}
function mn(e, t, s = Oe + 1) {
  for (; s < fe.length; s++) {
    const n = fe[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      fe.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function pr(e) {
  if (ft.length) {
    const t = [...new Set(ft)].sort(
      (s, n) => It(s) - It(n)
    );
    if (ft.length = 0, ke) {
      for (let s = 0; s < t.length; s++)
        ke.push(t[s]);
      return;
    }
    for (ke = t, ot = 0; ot < ke.length; ot++) {
      const s = ke[ot];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    ke = null, ot = 0;
  }
}
const It = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function gr(e) {
  try {
    for (Oe = 0; Oe < fe.length; Oe++) {
      const t = fe[Oe];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Dt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Oe < fe.length; Oe++) {
      const t = fe[Oe];
      t && (t.flags &= -2);
    }
    Oe = -1, fe.length = 0, pr(), Qt = null, (fe.length || ft.length) && gr();
  }
}
let Me = null, _r = null;
function es(e) {
  const t = Me;
  return Me = e, _r = e && e.type.__scopeId || null, t;
}
function Wi(e, t = Me, s) {
  if (!t || e._n)
    return e;
  const n = (...r) => {
    n._d && Pn(-1);
    const i = es(t), o = st.length;
    let l;
    try {
      l = e(...r);
    } finally {
      for (let f = st.length; f > o; f--) Wr();
      es(i), n._d && Pn(1);
    }
    return l;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function Xe(e, t, s, n) {
  const r = e.dirs, i = t && t.dirs;
  for (let o = 0; o < r.length; o++) {
    const l = r[o];
    i && (l.oldValue = i[o].value);
    let f = l.dir[n];
    f && (Le(), ve(f, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Ke());
  }
}
function Vi(e, t) {
  if (ue) {
    let s = ue.provides;
    const n = ue.parent && ue.parent.provides;
    n === s && (s = ue.provides = Object.create(n)), s[e] = t;
  }
}
function Gt(e, t, s = !1) {
  const n = Uo();
  if (n || ut) {
    let r = ut ? ut._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (r && e in r)
      return r[e];
    if (arguments.length > 1)
      return s && F(t) ? t.call(n && n.proxy) : t;
  }
}
const Bi = /* @__PURE__ */ Symbol.for("v-scx"), ki = () => Gt(Bi);
function Cs(e, t, s) {
  return mr(e, t, s);
}
function mr(e, t, s = B) {
  const { immediate: n, deep: r, flush: i, once: o } = s, l = se({}, s), f = t && n || !t && i !== "post";
  let d;
  if (Ft) {
    if (i === "sync") {
      const E = ki();
      d = E.__watcherHandles || (E.__watcherHandles = []);
    } else if (!f) {
      const E = () => {
      };
      return E.stop = Re, E.resume = Re, E.pause = Re, E;
    }
  }
  const a = ue;
  l.call = (E, $, M) => ve(E, a, $, M);
  let p = !1;
  i === "post" ? l.scheduler = (E) => {
    ae(E, a && a.suspense);
  } : i !== "sync" && (p = !0, l.scheduler = (E, $) => {
    $ ? E() : Qs(E);
  }), l.augmentJob = (E) => {
    t && (E.flags |= 4), p && (E.flags |= 2, a && (E.id = a.uid, E.i = a));
  };
  const C = Hi(e, t, l);
  return Ft && (d ? d.push(C) : f && C()), C;
}
function qi(e, t, s) {
  const n = this.proxy, r = Y(e) ? e.includes(".") ? br(n, e) : () => n[e] : e.bind(n, n);
  let i;
  F(t) ? i = t : (i = t.handler, s = t);
  const o = $t(this), l = mr(r, i.bind(n), s);
  return o(), l;
}
function br(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let r = 0; r < s.length && n; r++)
      n = n[s[r]];
    return n;
  };
}
const Gi = /* @__PURE__ */ Symbol("_vte"), ds = (e) => e.__isTeleport, Ts = /* @__PURE__ */ Symbol("_leaveCb");
function Ji(e) {
  let t = e[0];
  if (e.length > 1) {
    for (const s of e)
      if (s.type !== We) {
        t = s;
        break;
      }
  }
  return t;
}
function yr(e) {
  if (!tn(e))
    return ds(e.type) && e.children ? Ji(e.children) : e;
  if (e.component)
    return e.component.subTree;
  const { shapeFlag: t, children: s } = e;
  if (s) {
    if (t & 16)
      return s[0];
    if (t & 32 && F(s.default))
      return s.default();
  }
}
function en(e, t) {
  if (e.shapeFlag & 6 && e.component) {
    e.transition = t;
    const s = e.component.subTree;
    en(
      ds(s.type) && yr(s) || s,
      t
    );
  } else e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Yi(e, t) {
  return F(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    se({ name: e.name }, t, { setup: e })
  ) : e;
}
function vr(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function bn(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const ts = /* @__PURE__ */ new WeakMap();
function Ct(e, t, s, n, r = !1) {
  if (R(e)) {
    e.forEach(
      (M, k) => Ct(
        M,
        t && (R(t) ? t[k] : t),
        s,
        n,
        r
      )
    );
    return;
  }
  if (Tt(n) && !r) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Ct(e, t, s, n.component.subTree);
    return;
  }
  const i = n.shapeFlag & 4 ? rn(n.component) : n.el, o = r ? null : i, { i: l, r: f } = e, d = t && t.r, a = l.refs === B ? l.refs = {} : l.refs, p = l.setupState, C = /* @__PURE__ */ H(p), E = p === B ? Wn : (M) => bn(a, M) ? !1 : N(C, M), $ = (M, k) => !(k && bn(a, k));
  if (d != null && d !== f) {
    if (yn(t), Y(d))
      a[d] = null, E(d) && (p[d] = null);
    else if (/* @__PURE__ */ re(d)) {
      const M = t;
      $(d, M.k) && (d.value = null), M.k && (a[M.k] = null);
    }
  }
  if (F(f))
    Dt(f, l, 12, [o, a]);
  else {
    const M = Y(f), k = /* @__PURE__ */ re(f);
    if (M || k) {
      const T = () => {
        if (e.f) {
          const A = M ? E(f) ? p[f] : a[f] : $() || !e.k ? f.value : a[e.k];
          if (r)
            R(A) && Vs(A, i);
          else if (R(A))
            A.includes(i) || A.push(i);
          else if (M)
            a[f] = [i], E(f) && (p[f] = a[f]);
          else {
            const j = [i];
            $(f, e.k) && (f.value = j), e.k && (a[e.k] = j);
          }
        } else M ? (a[f] = o, E(f) && (p[f] = o)) : k && ($(f, e.k) && (f.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const A = () => {
          T(), ts.delete(e);
        };
        A.id = -1, ts.set(e, A), ae(A, s);
      } else
        yn(e), T();
    }
  }
}
function yn(e) {
  const t = ts.get(e);
  t && (t.flags |= 8, ts.delete(e));
}
cs().requestIdleCallback;
cs().cancelIdleCallback;
const Tt = (e) => !!e.type.__asyncLoader, tn = (e) => e.type.__isKeepAlive;
function zi(e, t) {
  xr(e, "a", t);
}
function Xi(e, t) {
  xr(e, "da", t);
}
function xr(e, t, s = ue) {
  const n = e.__wdc || (e.__wdc = () => {
    let r = s;
    for (; r; ) {
      if (r.isDeactivated)
        return;
      r = r.parent;
    }
    return e();
  });
  if (hs(t, n, s), s) {
    let r = s.parent;
    for (; r && r.parent; )
      tn(r.parent.vnode) && Zi(n, t, s, r), r = r.parent;
  }
}
function Zi(e, t, s, n) {
  const r = hs(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Cr(() => {
    Vs(n[t], r);
  }, s);
}
function hs(e, t, s = ue, n = !1) {
  if (s) {
    const r = s[e] || (s[e] = []), i = t.__weh || (t.__weh = (...o) => {
      Le();
      const l = $t(s), f = ve(t, s, e, o);
      return l(), Ke(), f;
    });
    return n ? r.unshift(i) : r.push(i), i;
  }
}
const Ve = (e) => (t, s = ue) => {
  (!Ft || e === "sp") && hs(e, (...n) => t(...n), s);
}, Qi = Ve("bm"), Sr = Ve("m"), eo = Ve(
  "bu"
), to = Ve("u"), wr = Ve(
  "bum"
), Cr = Ve("um"), so = Ve(
  "sp"
), no = Ve("rtg"), ro = Ve("rtc");
function io(e, t = ue) {
  hs("ec", e, t);
}
const oo = /* @__PURE__ */ Symbol.for("v-ndc");
function vn(e, t, s, n) {
  let r;
  const i = s, o = R(e);
  if (o || Y(e)) {
    const l = o && /* @__PURE__ */ tt(e);
    let f = !1, d = !1;
    l && (f = !/* @__PURE__ */ ge(e), d = /* @__PURE__ */ Ue(e), e = us(e)), r = new Array(e.length);
    for (let a = 0, p = e.length; a < p; a++)
      r[a] = t(
        f ? d ? at(ye(e[a])) : ye(e[a]) : e[a],
        a,
        void 0,
        i
      );
  } else if (typeof e == "number") {
    r = new Array(e);
    for (let l = 0; l < e; l++)
      r[l] = t(l + 1, l, void 0, i);
  } else if (K(e))
    if (e[Symbol.iterator])
      r = Array.from(
        e,
        (l, f) => t(l, f, void 0, i)
      );
    else {
      const l = Object.keys(e);
      r = new Array(l.length);
      for (let f = 0, d = l.length; f < d; f++) {
        const a = l[f];
        r[f] = t(e[a], a, f, i);
      }
    }
  else
    r = [];
  return r;
}
const $s = (e) => e ? qr(e) ? rn(e) : $s(e.parent) : null, Et = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ se(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => e.props,
    $attrs: (e) => e.attrs,
    $slots: (e) => e.slots,
    $refs: (e) => e.refs,
    $parent: (e) => $s(e.parent),
    $root: (e) => $s(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Er(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      Qs(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = Li.bind(e.proxy)),
    $watch: (e) => qi.bind(e)
  })
), Es = (e, t) => e !== B && !e.__isScriptSetup && N(e, t), lo = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: r, props: i, accessCache: o, type: l, appContext: f } = e;
    if (t[0] !== "$") {
      const C = o[t];
      if (C !== void 0)
        switch (C) {
          case 1:
            return n[t];
          case 2:
            return r[t];
          case 4:
            return s[t];
          case 3:
            return i[t];
        }
      else {
        if (Es(n, t))
          return o[t] = 1, n[t];
        if (r !== B && N(r, t))
          return o[t] = 2, r[t];
        if (N(i, t))
          return o[t] = 3, i[t];
        if (s !== B && N(s, t))
          return o[t] = 4, s[t];
        Hs && (o[t] = 0);
      }
    }
    const d = Et[t];
    let a, p;
    if (d)
      return t === "$attrs" && ne(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== B && N(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      p = f.config.globalProperties, N(p, t)
    )
      return p[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: r, ctx: i } = e;
    return Es(r, t) ? (r[t] = s, !0) : n !== B && N(n, t) ? (n[t] = s, !0) : N(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (i[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: r, props: i, type: o }
  }, l) {
    let f;
    return !!(s[l] || e !== B && l[0] !== "$" && N(e, l) || Es(t, l) || N(i, l) || N(n, l) || N(Et, l) || N(r.config.globalProperties, l) || (f = o.__cssModules) && f[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : N(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function xn(e) {
  return R(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let Hs = !0;
function co(e) {
  const t = Er(e), s = e.proxy, n = e.ctx;
  Hs = !1, t.beforeCreate && Sn(t.beforeCreate, e, "bc");
  const {
    // state
    data: r,
    computed: i,
    methods: o,
    watch: l,
    provide: f,
    inject: d,
    // lifecycle
    created: a,
    beforeMount: p,
    mounted: C,
    beforeUpdate: E,
    updated: $,
    activated: M,
    deactivated: k,
    beforeDestroy: T,
    beforeUnmount: A,
    destroyed: j,
    unmounted: w,
    render: G,
    renderTracked: _e,
    renderTriggered: xe,
    errorCaptured: Be,
    serverPrefetch: Ht,
    // public API
    expose: Je,
    inheritAttrs: ht,
    // assets
    components: Nt,
    directives: Lt,
    filters: _s
  } = t;
  if (d && fo(d, n, null), o)
    for (const q in o) {
      const W = o[q];
      F(W) && (n[q] = W.bind(s));
    }
  if (r) {
    const q = r.call(s, s);
    K(q) && (e.data = /* @__PURE__ */ Pt(q));
  }
  if (Hs = !0, i)
    for (const q in i) {
      const W = i[q], Ye = F(W) ? W.bind(s, s) : F(W.get) ? W.get.bind(s, s) : Re, Kt = !F(W) && F(W.set) ? W.set.bind(s) : Re, ze = zt({
        get: Ye,
        set: Kt
      });
      Object.defineProperty(n, q, {
        enumerable: !0,
        configurable: !0,
        get: () => ze.value,
        set: (Se) => ze.value = Se
      });
    }
  if (l)
    for (const q in l)
      Tr(l[q], n, s, q);
  if (f) {
    const q = F(f) ? f.call(s) : f;
    Reflect.ownKeys(q).forEach((W) => {
      Vi(W, q[W]);
    });
  }
  a && Sn(a, e, "c");
  function ie(q, W) {
    R(W) ? W.forEach((Ye) => q(Ye.bind(s))) : W && q(W.bind(s));
  }
  if (ie(Qi, p), ie(Sr, C), ie(eo, E), ie(to, $), ie(zi, M), ie(Xi, k), ie(io, Be), ie(ro, _e), ie(no, xe), ie(wr, A), ie(Cr, w), ie(so, Ht), R(Je))
    if (Je.length) {
      const q = e.exposed || (e.exposed = {});
      Je.forEach((W) => {
        Object.defineProperty(q, W, {
          get: () => s[W],
          set: (Ye) => s[W] = Ye,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  G && e.render === Re && (e.render = G), ht != null && (e.inheritAttrs = ht), Nt && (e.components = Nt), Lt && (e.directives = Lt), Ht && vr(e);
}
function fo(e, t, s = Re) {
  R(e) && (e = Ns(e));
  for (const n in e) {
    const r = e[n];
    let i;
    K(r) ? "default" in r ? i = Gt(
      r.from || n,
      r.default,
      !0
    ) : i = Gt(r.from || n) : i = Gt(r), /* @__PURE__ */ re(i) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => i.value,
      set: (o) => i.value = o
    }) : t[n] = i;
  }
}
function Sn(e, t, s) {
  ve(
    R(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Tr(e, t, s, n) {
  let r = n.includes(".") ? br(s, n) : () => s[n];
  if (Y(e)) {
    const i = t[e];
    F(i) && Cs(r, i);
  } else if (F(e))
    Cs(r, e.bind(s));
  else if (K(e))
    if (R(e))
      e.forEach((i) => Tr(i, t, s, n));
    else {
      const i = F(e.handler) ? e.handler.bind(s) : t[e.handler];
      F(i) && Cs(r, i, e);
    }
}
function Er(e) {
  const t = e.type, { mixins: s, extends: n } = t, {
    mixins: r,
    optionsCache: i,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = i.get(t);
  let f;
  return l ? f = l : !r.length && !s && !n ? f = t : (f = {}, r.length && r.forEach(
    (d) => ss(f, d, o, !0)
  ), ss(f, t, o)), K(t) && i.set(t, f), f;
}
function ss(e, t, s, n = !1) {
  const { mixins: r, extends: i } = t;
  i && ss(e, i, s, !0), r && r.forEach(
    (o) => ss(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = uo[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const uo = {
  data: wn,
  props: Cn,
  emits: Cn,
  // objects
  methods: yt,
  computed: yt,
  // lifecycle
  beforeCreate: ce,
  created: ce,
  beforeMount: ce,
  mounted: ce,
  beforeUpdate: ce,
  updated: ce,
  beforeDestroy: ce,
  beforeUnmount: ce,
  destroyed: ce,
  unmounted: ce,
  activated: ce,
  deactivated: ce,
  errorCaptured: ce,
  serverPrefetch: ce,
  // assets
  components: yt,
  directives: yt,
  // watch
  watch: ho,
  // provide / inject
  provide: wn,
  inject: ao
};
function wn(e, t) {
  return t ? e ? function() {
    return se(
      F(e) ? e.call(this, this) : e,
      F(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function ao(e, t) {
  return yt(Ns(e), Ns(t));
}
function Ns(e) {
  if (R(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++)
      t[e[s]] = e[s];
    return t;
  }
  return e;
}
function ce(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function yt(e, t) {
  return e ? se(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Cn(e, t) {
  return e ? R(e) && R(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : se(
    /* @__PURE__ */ Object.create(null),
    xn(e),
    xn(t ?? {})
  ) : t;
}
function ho(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = se(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = ce(e[n], t[n]);
  return s;
}
function Or() {
  return {
    app: null,
    config: {
      isNativeTag: Wn,
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
let po = 0;
function go(e, t) {
  return function(n, r = null) {
    F(n) || (n = se({}, n)), r != null && !K(r) && (r = null);
    const i = Or(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let f = !1;
    const d = i.app = {
      _uid: po++,
      _component: n,
      _props: r,
      _container: null,
      _context: i,
      _instance: null,
      version: Go,
      get config() {
        return i.config;
      },
      set config(a) {
      },
      use(a, ...p) {
        return o.has(a) || (a && F(a.install) ? (o.add(a), a.install(d, ...p)) : F(a) && (o.add(a), a(d, ...p))), d;
      },
      mixin(a) {
        return i.mixins.includes(a) || i.mixins.push(a), d;
      },
      component(a, p) {
        return p ? (i.components[a] = p, d) : i.components[a];
      },
      directive(a, p) {
        return p ? (i.directives[a] = p, d) : i.directives[a];
      },
      mount(a, p, C) {
        if (!f) {
          const E = d._ceVNode || Ne(n, r);
          return E.appContext = i, C === !0 ? C = "svg" : C === !1 && (C = void 0), e(E, a, C), f = !0, d._container = a, a.__vue_app__ = d, rn(E.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        f && (ve(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, p) {
        return i.provides[a] = p, d;
      },
      runWithContext(a) {
        const p = ut;
        ut = d;
        try {
          return a();
        } finally {
          ut = p;
        }
      }
    };
    return d;
  };
}
let ut = null;
const _o = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${me(t)}Modifiers`] || e[`${nt(t)}Modifiers`];
function mo(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || B;
  let r = s;
  const i = t.startsWith("update:"), o = i && _o(n, t.slice(7));
  o && (o.trim && (r = s.map((a) => Y(a) ? a.trim() : a)), o.number && (r = r.map(ti)));
  let l, f = n[l = bs(t)] || // also try camelCase event handler (#2249)
  n[l = bs(me(t))];
  !f && i && (f = n[l = bs(nt(t))]), f && ve(
    f,
    e,
    6,
    r
  );
  const d = n[l + "Once"];
  if (d) {
    if (!e.emitted)
      e.emitted = {};
    else if (e.emitted[l])
      return;
    e.emitted[l] = !0, ve(
      d,
      e,
      6,
      r
    );
  }
}
const bo = /* @__PURE__ */ new WeakMap();
function Ar(e, t, s = !1) {
  const n = s ? bo : t.emitsCache, r = n.get(e);
  if (r !== void 0)
    return r;
  const i = e.emits;
  let o = {}, l = !1;
  if (!F(e)) {
    const f = (d) => {
      const a = Ar(d, t, !0);
      a && (l = !0, se(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  return !i && !l ? (K(e) && n.set(e, null), null) : (R(i) ? i.forEach((f) => o[f] = null) : se(o, i), K(e) && n.set(e, o), o);
}
function ps(e, t) {
  return !e || !is(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), N(e, t[0].toLowerCase() + t.slice(1)) || N(e, nt(t)) || N(e, t));
}
function Tn(e) {
  const {
    type: t,
    vnode: s,
    proxy: n,
    withProxy: r,
    propsOptions: [i],
    slots: o,
    attrs: l,
    emit: f,
    render: d,
    renderCache: a,
    props: p,
    data: C,
    setupState: E,
    ctx: $,
    inheritAttrs: M
  } = e, k = es(e);
  let T, A;
  try {
    if (s.shapeFlag & 4) {
      const w = r || n, G = w;
      T = Pe(
        d.call(
          G,
          w,
          a,
          p,
          E,
          C,
          $
        )
      ), A = l;
    } else {
      const w = t;
      T = Pe(
        w.length > 1 ? w(
          p,
          { attrs: l, slots: o, emit: f }
        ) : w(
          p,
          null
        )
      ), A = t.props ? l : yo(l);
    }
  } catch (w) {
    st.length = 0, as(w, e, 1), T = Ne(We);
  }
  let j = T;
  if (A && M !== !1) {
    const w = Object.keys(A), { shapeFlag: G } = j;
    w.length && G & 7 && (i && w.some(os) && (A = vo(
      A,
      i
    )), j = dt(j, A, !1, !0));
  }
  if (s.dirs && (j = dt(j, null, !1, !0), j.dirs = j.dirs ? j.dirs.concat(s.dirs) : s.dirs), s.transition) {
    const w = ds(j.type) && yr(j) || j;
    en(w, s.transition);
  }
  return T = j, es(k), T;
}
const yo = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || is(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, vo = (e, t) => {
  const s = {};
  for (const n in e)
    (!os(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function xo(e, t, s) {
  const { props: n, children: r, component: i } = e, { props: o, children: l, patchFlag: f } = t, d = i.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (s && f >= 0) {
    if (f & 1024)
      return !0;
    if (f & 16)
      return n ? En(n, o, d) : !!o;
    if (f & 8) {
      const a = t.dynamicProps;
      for (let p = 0; p < a.length; p++) {
        const C = a[p];
        if (Pr(o, n, C) && !ps(d, C))
          return !0;
      }
    }
  } else
    return (r || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? En(n, o, d) : !0 : !!o;
  return !1;
}
function En(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let r = 0; r < n.length; r++) {
    const i = n[r];
    if (Pr(t, e, i) && !ps(s, i))
      return !0;
  }
  return !1;
}
function Pr(e, t, s) {
  const n = e[s], r = t[s];
  return s === "style" && K(n) && K(r) ? !fs(n, r) : n !== r;
}
function So({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const r = t.subTree;
    if (r.suspense && r.suspense.activeBranch === e && (r.suspense.vnode.el = r.el = n, e = r), r === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const Ir = {}, Mr = () => Object.create(Ir), Rr = (e) => Object.getPrototypeOf(e) === Ir;
function wo(e, t, s, n = !1) {
  const r = {}, i = Mr();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), Fr(e, t, r, i);
  for (const o in e.propsOptions[0])
    o in r || (r[o] = void 0);
  s ? e.props = n ? r : /* @__PURE__ */ Ai(r) : e.type.props ? e.props = r : e.props = i, e.attrs = i;
}
function Co(e, t, s, n) {
  const {
    props: r,
    attrs: i,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ H(r), [f] = e.propsOptions;
  let d = !1;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    (n || o > 0) && !(o & 16)
  ) {
    if (o & 8) {
      const a = e.vnode.dynamicProps;
      for (let p = 0; p < a.length; p++) {
        let C = a[p];
        if (ps(e.emitsOptions, C))
          continue;
        const E = t[C];
        if (f)
          if (N(i, C))
            E !== i[C] && (i[C] = E, d = !0);
          else {
            const $ = me(C);
            r[$] = Ls(
              f,
              l,
              $,
              E,
              e,
              !1
            );
          }
        else
          E !== i[C] && (i[C] = E, d = !0);
      }
    }
  } else {
    Fr(e, t, r, i) && (d = !0);
    let a;
    for (const p in l)
      (!t || // for camelCase
      !N(t, p) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = nt(p)) === p || !N(t, a))) && (f ? s && // for camelCase
      (s[p] !== void 0 || // for kebab-case
      s[a] !== void 0) && (r[p] = Ls(
        f,
        l,
        p,
        void 0,
        e,
        !0
      )) : delete r[p]);
    if (i !== l)
      for (const p in i)
        (!t || !N(t, p)) && (delete i[p], d = !0);
  }
  d && He(e.attrs, "set", "");
}
function Fr(e, t, s, n) {
  const [r, i] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let f in t) {
      if (xt(f))
        continue;
      const d = t[f];
      let a;
      r && N(r, a = me(f)) ? !i || !i.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : ps(e.emitsOptions, f) || (!(f in n) || d !== n[f]) && (n[f] = d, o = !0);
    }
  if (i) {
    const f = /* @__PURE__ */ H(s), d = l || B;
    for (let a = 0; a < i.length; a++) {
      const p = i[a];
      s[p] = Ls(
        r,
        f,
        p,
        d[p],
        e,
        !N(d, p)
      );
    }
  }
  return o;
}
function Ls(e, t, s, n, r, i) {
  const o = e[s];
  if (o != null) {
    const l = N(o, "default");
    if (l && n === void 0) {
      const f = o.default;
      if (o.type !== Function && !o.skipFactory && F(f)) {
        const { propsDefaults: d } = r;
        if (s in d)
          n = d[s];
        else {
          const a = $t(r);
          n = d[s] = f.call(
            null,
            t
          ), a();
        }
      } else
        n = f;
      r.ce && r.ce._setProp(s, n);
    }
    o[
      0
      /* shouldCast */
    ] && (i && !l ? n = !1 : o[
      1
      /* shouldCastTrue */
    ] && (n === "" || n === nt(s)) && (n = !0));
  }
  return n;
}
const To = /* @__PURE__ */ new WeakMap();
function jr(e, t, s = !1) {
  const n = s ? To : t.propsCache, r = n.get(e);
  if (r)
    return r;
  const i = e.props, o = {}, l = [];
  let f = !1;
  if (!F(e)) {
    const a = (p) => {
      f = !0;
      const [C, E] = jr(p, t, !0);
      se(o, C), E && l.push(...E);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!i && !f)
    return K(e) && n.set(e, lt), lt;
  if (R(i))
    for (let a = 0; a < i.length; a++) {
      const p = me(i[a]);
      On(p) && (o[p] = B);
    }
  else if (i)
    for (const a in i) {
      const p = me(a);
      if (On(p)) {
        const C = i[a], E = o[p] = R(C) || F(C) ? { type: C } : se({}, C), $ = E.type;
        let M = !1, k = !0;
        if (R($))
          for (let T = 0; T < $.length; ++T) {
            const A = $[T], j = F(A) && A.name;
            if (j === "Boolean") {
              M = !0;
              break;
            } else j === "String" && (k = !1);
          }
        else
          M = F($) && $.name === "Boolean";
        E[
          0
          /* shouldCast */
        ] = M, E[
          1
          /* shouldCastTrue */
        ] = k, (M || N(E, "default")) && l.push(p);
      }
    }
  const d = [o, l];
  return K(e) && n.set(e, d), d;
}
function On(e) {
  return e[0] !== "$" && !xt(e);
}
const sn = (e) => e === "_" || e === "_ctx" || e === "$stable", nn = (e) => R(e) ? e.map(Pe) : [Pe(e)], Eo = (e, t, s) => {
  if (t._n)
    return t;
  const n = Wi((...r) => nn(t(...r)), s);
  return n._c = !1, n;
}, Dr = (e, t, s) => {
  const n = e._ctx;
  for (const r in e) {
    if (sn(r)) continue;
    const i = e[r];
    if (F(i))
      t[r] = Eo(r, i, n);
    else if (i != null) {
      const o = nn(i);
      t[r] = () => o;
    }
  }
}, $r = (e, t) => {
  const s = nn(t);
  e.slots.default = () => s;
}, Hr = (e, t, s) => {
  for (const n in t)
    (s || !sn(n)) && (e[n] = t[n]);
}, Oo = (e, t, s) => {
  const n = e.slots = Mr();
  if (e.vnode.shapeFlag & 32) {
    const r = t._;
    r ? (Hr(n, t, s), s && Gn(n, "_", r, !0)) : Dr(t, n);
  } else t && $r(e, t);
}, Ao = (e, t, s) => {
  const { vnode: n, slots: r } = e;
  let i = !0, o = B;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? i = !1 : Hr(r, t, s) : (i = !t.$stable, Dr(t, r)), o = t;
  } else t && ($r(e, t), o = { default: 1 });
  if (i)
    for (const l in r)
      !sn(l) && o[l] == null && delete r[l];
}, ae = Fo;
function Po(e) {
  return Io(e);
}
function Io(e, t) {
  const s = cs();
  s.__VUE__ = !0;
  const {
    insert: n,
    remove: r,
    patchProp: i,
    createElement: o,
    createText: l,
    createComment: f,
    setText: d,
    setElementText: a,
    parentNode: p,
    nextSibling: C,
    setScopeId: E = Re,
    insertStaticContent: $
  } = e, M = (c, u, h, b = null, m = null, g = null, x = void 0, v = null, y = !!u.dynamicChildren) => {
    if (c === u)
      return;
    c && !bt(c, u) && (b = Ut(c), Se(c, m, g, !0), c = null), u.patchFlag === -2 && (y = !1, u.dynamicChildren = null);
    const { type: _, ref: P, shapeFlag: S } = u;
    switch (_) {
      case gs:
        k(c, u, h, b);
        break;
      case We:
        T(c, u, h, b);
        break;
      case As:
        c == null && A(u, h, b, x);
        break;
      case de:
        Nt(
          c,
          u,
          h,
          b,
          m,
          g,
          x,
          v,
          y
        );
        break;
      default:
        S & 1 ? G(
          c,
          u,
          h,
          b,
          m,
          g,
          x,
          v,
          y
        ) : S & 6 ? Lt(
          c,
          u,
          h,
          b,
          m,
          g,
          x,
          v,
          y
        ) : (S & 64 || S & 128) && _.process(
          c,
          u,
          h,
          b,
          m,
          g,
          x,
          v,
          y,
          gt
        );
    }
    P != null && m ? Ct(P, c && c.ref, g, u || c, !u) : P == null && c && c.ref != null && Ct(c.ref, null, g, c, !0);
  }, k = (c, u, h, b) => {
    if (c == null)
      n(
        u.el = l(u.children),
        h,
        b
      );
    else {
      const m = u.el = c.el;
      u.children !== c.children && d(m, u.children);
    }
  }, T = (c, u, h, b) => {
    c == null ? n(
      u.el = f(u.children || ""),
      h,
      b
    ) : u.el = c.el;
  }, A = (c, u, h, b) => {
    [c.el, c.anchor] = $(
      c.children,
      u,
      h,
      b,
      c.el,
      c.anchor
    );
  }, j = ({ el: c, anchor: u }, h, b) => {
    let m;
    for (; c && c !== u; )
      m = C(c), n(c, h, b), c = m;
    n(u, h, b);
  }, w = ({ el: c, anchor: u }) => {
    let h;
    for (; c && c !== u; )
      h = C(c), r(c), c = h;
    r(u);
  }, G = (c, u, h, b, m, g, x, v, y) => {
    if (u.type === "svg" ? x = "svg" : u.type === "math" && (x = "mathml"), c == null)
      _e(
        u,
        h,
        b,
        m,
        g,
        x,
        v,
        y
      );
    else {
      const _ = c.el && c.el._isVueCE ? c.el : null;
      try {
        _ && _._beginPatch(), Ht(
          c,
          u,
          m,
          g,
          x,
          v,
          y
        );
      } finally {
        _ && _._endPatch();
      }
    }
  }, _e = (c, u, h, b, m, g, x, v) => {
    let y, _;
    const { props: P, shapeFlag: S, transition: O, dirs: I } = c;
    if (y = c.el = o(
      c.type,
      g,
      P && P.is,
      P
    ), S & 8 ? a(y, c.children) : S & 16 && Be(
      c.children,
      y,
      null,
      b,
      m,
      Os(c, g),
      x,
      v
    ), I && Xe(c, null, b, "created"), xe(y, c, c.scopeId, x, b), P) {
      for (const U in P)
        U !== "value" && !xt(U) && i(y, U, null, P[U], g, b);
      "value" in P && i(y, "value", null, P.value, g), (_ = P.onVnodeBeforeMount) && Ee(_, b, c);
    }
    I && Xe(c, null, b, "beforeMount");
    const D = Mo(m, O);
    D && O.beforeEnter(y), n(y, u, h), ((_ = P && P.onVnodeMounted) || D || I) && ae(() => {
      try {
        _ && Ee(_, b, c), D && O.enter(y), I && Xe(c, null, b, "mounted");
      } finally {
      }
    }, m);
  }, xe = (c, u, h, b, m) => {
    if (h && E(c, h), b)
      for (let g = 0; g < b.length; g++)
        E(c, b[g]);
    if (m) {
      let g = m.subTree;
      if (u === g || Ur(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const x = m.vnode;
        xe(
          c,
          x,
          x.scopeId,
          x.slotScopeIds,
          m.parent
        );
      }
    }
  }, Be = (c, u, h, b, m, g, x, v, y = 0) => {
    for (let _ = y; _ < c.length; _++) {
      const P = c[_] = v ? $e(c[_]) : Pe(c[_]);
      M(
        null,
        P,
        u,
        h,
        b,
        m,
        g,
        x,
        v
      );
    }
  }, Ht = (c, u, h, b, m, g, x) => {
    const v = u.el = c.el;
    let { patchFlag: y, dynamicChildren: _, dirs: P } = u;
    y |= c.patchFlag & 16;
    const S = c.props || B, O = u.props || B;
    let I;
    if (h && Ze(h, !1), (I = O.onVnodeBeforeUpdate) && Ee(I, h, u, c), P && Xe(u, c, h, "beforeUpdate"), h && Ze(h, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    _ && (!c.dynamicChildren || c.dynamicChildren.length !== _.length) && (y = 0, x = !1, _ = null), (S.innerHTML && O.innerHTML == null || S.textContent && O.textContent == null) && a(v, ""), _ ? Je(
      c.dynamicChildren,
      _,
      v,
      h,
      b,
      Os(u, m),
      g
    ) : x || W(
      c,
      u,
      v,
      null,
      h,
      b,
      Os(u, m),
      g,
      !1
    ), y > 0) {
      if (y & 16)
        ht(v, S, O, h, m);
      else if (y & 2 && S.class !== O.class && i(v, "class", null, O.class, m), y & 4 && i(v, "style", S.style, O.style, m), y & 8) {
        const D = u.dynamicProps;
        for (let U = 0; U < D.length; U++) {
          const L = D[U], z = S[L], Q = O[L];
          (Q !== z || L === "value") && i(v, L, z, Q, m, h);
        }
      }
      y & 1 && c.children !== u.children && a(v, u.children);
    } else !x && _ == null && ht(v, S, O, h, m);
    ((I = O.onVnodeUpdated) || P) && ae(() => {
      I && Ee(I, h, u, c), P && Xe(u, c, h, "updated");
    }, b);
  }, Je = (c, u, h, b, m, g, x) => {
    for (let v = 0; v < u.length; v++) {
      const y = c[v], _ = u[v], P = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        y.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (y.type === de || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !bt(y, _) || // - In the case of a component, it could contain anything.
        y.shapeFlag & 198) ? p(y.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          h
        )
      );
      M(
        y,
        _,
        P,
        null,
        b,
        m,
        g,
        x,
        !0
      );
    }
  }, ht = (c, u, h, b, m) => {
    if (u !== h) {
      if (u !== B)
        for (const g in u)
          !xt(g) && !(g in h) && i(
            c,
            g,
            u[g],
            null,
            m,
            b
          );
      for (const g in h) {
        if (xt(g)) continue;
        const x = h[g], v = u[g];
        x !== v && g !== "value" && i(c, g, v, x, m, b);
      }
      "value" in h && i(c, "value", u.value, h.value, m);
    }
  }, Nt = (c, u, h, b, m, g, x, v, y) => {
    const _ = u.el = c ? c.el : l(""), P = u.anchor = c ? c.anchor : l("");
    let { patchFlag: S, dynamicChildren: O, slotScopeIds: I } = u;
    I && (v = v ? v.concat(I) : I), c == null ? (n(_, h, b), n(P, h, b), Be(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      h,
      P,
      m,
      g,
      x,
      v,
      y
    )) : S > 0 && S & 64 && O && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    c.dynamicChildren && c.dynamicChildren.length === O.length ? (Je(
      c.dynamicChildren,
      O,
      h,
      m,
      g,
      x,
      v
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || m && u === m.subTree) && Nr(
      c,
      u,
      !0
      /* shallow */
    )) : W(
      c,
      u,
      h,
      P,
      m,
      g,
      x,
      v,
      y
    );
  }, Lt = (c, u, h, b, m, g, x, v, y) => {
    u.slotScopeIds = v, c == null ? u.shapeFlag & 512 ? m.ctx.activate(
      u,
      h,
      b,
      x,
      y
    ) : _s(
      u,
      h,
      b,
      m,
      g,
      x,
      y
    ) : on(c, u, y);
  }, _s = (c, u, h, b, m, g, x) => {
    const v = c.component = Ko(
      c,
      b,
      m
    );
    if (tn(c) && (v.ctx.renderer = gt), Wo(v, !1, x), v.asyncDep) {
      if (m && m.registerDep(v, ie, x), !c.el) {
        const y = v.subTree = Ne(We);
        T(null, y, u, h), c.placeholder = y.el;
      }
    } else
      ie(
        v,
        c,
        u,
        h,
        m,
        g,
        x
      );
  }, on = (c, u, h) => {
    const b = u.component = c.component;
    if (xo(c, u, h))
      if (b.asyncDep && !b.asyncResolved) {
        q(b, u, h);
        return;
      } else
        b.next = u, b.update();
    else
      u.el = c.el, b.vnode = u;
  }, ie = (c, u, h, b, m, g, x) => {
    const v = () => {
      if (c.isMounted) {
        let { next: S, bu: O, u: I, parent: D, vnode: U } = c;
        {
          const Ce = Lr(c);
          if (Ce) {
            S && (S.el = U.el, q(c, S, x)), Ce.asyncDep.then(() => {
              ae(() => {
                c.isUnmounted || _();
              }, m);
            });
            return;
          }
        }
        let L = S, z;
        Ze(c, !1), S ? (S.el = U.el, q(c, S, x)) : S = U, O && ys(O), (z = S.props && S.props.onVnodeBeforeUpdate) && Ee(z, D, S, U), Ze(c, !0);
        const Q = Tn(c), we = c.subTree;
        c.subTree = Q, M(
          we,
          Q,
          // parent may have changed if it's in a teleport
          p(we.el),
          // anchor may have changed if it's in a fragment
          Ut(we),
          c,
          m,
          g
        ), S.el = Q.el, L === null && So(c, Q.el), I && ae(I, m), (z = S.props && S.props.onVnodeUpdated) && ae(
          () => Ee(z, D, S, U),
          m
        );
      } else {
        let S;
        const { el: O, props: I } = u, { bm: D, m: U, parent: L, root: z, type: Q } = c, we = Tt(u);
        Ze(c, !1), D && ys(D), !we && (S = I && I.onVnodeBeforeMount) && Ee(S, L, u), Ze(c, !0);
        {
          z.ce && z.ce._hasShadowRoot() && z.ce._injectChildStyle(
            Q,
            c.parent ? c.parent.type : void 0
          );
          const Ce = c.subTree = Tn(c);
          M(
            null,
            Ce,
            h,
            b,
            c,
            m,
            g
          ), u.el = Ce.el;
        }
        if (U && ae(U, m), !we && (S = I && I.onVnodeMounted)) {
          const Ce = u;
          ae(
            () => Ee(S, L, Ce),
            m
          );
        }
        (u.shapeFlag & 256 || L && Tt(L.vnode) && L.vnode.shapeFlag & 256) && c.a && ae(c.a, m), c.isMounted = !0, u = h = b = null;
      }
    };
    c.scope.on();
    const y = c.effect = new Xn(v);
    c.scope.off();
    const _ = c.update = y.run.bind(y), P = c.job = y.runIfDirty.bind(y);
    P.i = c, P.id = c.uid, y.scheduler = () => Qs(P), Ze(c, !0), _();
  }, q = (c, u, h) => {
    u.component = c;
    const b = c.vnode.props;
    c.vnode = u, c.next = null, Co(c, u.props, b, h), Ao(c, u.children, h), Le(), mn(c), Ke();
  }, W = (c, u, h, b, m, g, x, v, y = !1) => {
    const _ = c && c.children, P = c ? c.shapeFlag : 0, S = u.children, { patchFlag: O, shapeFlag: I } = u;
    if (O > 0) {
      if (O & 128) {
        Kt(
          _,
          S,
          h,
          b,
          m,
          g,
          x,
          v,
          y
        );
        return;
      } else if (O & 256) {
        Ye(
          _,
          S,
          h,
          b,
          m,
          g,
          x,
          v,
          y
        );
        return;
      }
    }
    I & 8 ? (P & 16 && pt(_, m, g), S !== _ && a(h, S)) : P & 16 ? I & 16 ? Kt(
      _,
      S,
      h,
      b,
      m,
      g,
      x,
      v,
      y
    ) : pt(_, m, g, !0) : (P & 8 && a(h, ""), I & 16 && Be(
      S,
      h,
      b,
      m,
      g,
      x,
      v,
      y
    ));
  }, Ye = (c, u, h, b, m, g, x, v, y) => {
    c = c || lt, u = u || lt;
    const _ = c.length, P = u.length, S = Math.min(_, P);
    let O;
    for (O = 0; O < S; O++) {
      const I = u[O] = y ? $e(u[O]) : Pe(u[O]);
      M(
        c[O],
        I,
        h,
        null,
        m,
        g,
        x,
        v,
        y
      );
    }
    _ > P ? pt(
      c,
      m,
      g,
      !0,
      !1,
      S
    ) : Be(
      u,
      h,
      b,
      m,
      g,
      x,
      v,
      y,
      S
    );
  }, Kt = (c, u, h, b, m, g, x, v, y) => {
    let _ = 0;
    const P = u.length;
    let S = c.length - 1, O = P - 1;
    for (; _ <= S && _ <= O; ) {
      const I = c[_], D = u[_] = y ? $e(u[_]) : Pe(u[_]);
      if (bt(I, D))
        M(
          I,
          D,
          h,
          null,
          m,
          g,
          x,
          v,
          y
        );
      else
        break;
      _++;
    }
    for (; _ <= S && _ <= O; ) {
      const I = c[S], D = u[O] = y ? $e(u[O]) : Pe(u[O]);
      if (bt(I, D))
        M(
          I,
          D,
          h,
          null,
          m,
          g,
          x,
          v,
          y
        );
      else
        break;
      S--, O--;
    }
    if (_ > S) {
      if (_ <= O) {
        const I = O + 1, D = I < P ? u[I].el : b;
        for (; _ <= O; )
          M(
            null,
            u[_] = y ? $e(u[_]) : Pe(u[_]),
            h,
            D,
            m,
            g,
            x,
            v,
            y
          ), _++;
      }
    } else if (_ > O)
      for (; _ <= S; )
        Se(c[_], m, g, !0), _++;
    else {
      const I = _, D = _, U = /* @__PURE__ */ new Map();
      for (_ = D; _ <= O; _++) {
        const he = u[_] = y ? $e(u[_]) : Pe(u[_]);
        he.key != null && U.set(he.key, _);
      }
      let L, z = 0;
      const Q = O - D + 1;
      let we = !1, Ce = 0;
      const _t = new Array(Q);
      for (_ = 0; _ < Q; _++) _t[_] = 0;
      for (_ = I; _ <= S; _++) {
        const he = c[_];
        if (z >= Q) {
          Se(he, m, g, !0);
          continue;
        }
        let Te;
        if (he.key != null)
          Te = U.get(he.key);
        else
          for (L = D; L <= O; L++)
            if (_t[L - D] === 0 && bt(he, u[L])) {
              Te = L;
              break;
            }
        Te === void 0 ? Se(he, m, g, !0) : (_t[Te - D] = _ + 1, Te >= Ce ? Ce = Te : we = !0, M(
          he,
          u[Te],
          h,
          null,
          m,
          g,
          x,
          v,
          y
        ), z++);
      }
      const fn = we ? Ro(_t) : lt;
      for (L = fn.length - 1, _ = Q - 1; _ >= 0; _--) {
        const he = D + _, Te = u[he], un = u[he + 1], an = he + 1 < P ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          un.el || Kr(un)
        ) : b;
        _t[_] === 0 ? M(
          null,
          Te,
          h,
          an,
          m,
          g,
          x,
          v,
          y
        ) : we && (L < 0 || _ !== fn[L] ? ze(Te, h, an, 2) : L--);
      }
    }
  }, ze = (c, u, h, b, m = null) => {
    const { el: g, type: x, transition: v, children: y, shapeFlag: _ } = c;
    if (_ & 6) {
      ze(c.component.subTree, u, h, b);
      return;
    }
    if (_ & 128) {
      c.suspense.move(u, h, b);
      return;
    }
    if (_ & 64) {
      x.move(c, u, h, gt);
      return;
    }
    if (x === de) {
      n(g, u, h);
      for (let S = 0; S < y.length; S++)
        ze(y[S], u, h, b);
      n(c.anchor, u, h);
      return;
    }
    if (x === As) {
      j(c, u, h);
      return;
    }
    if (b !== 2 && _ & 1 && v)
      if (b === 0)
        v.persisted && !g[Ts] ? n(g, u, h) : (v.beforeEnter(g), n(g, u, h), ae(() => v.enter(g), m));
      else {
        const { leave: S, delayLeave: O, afterLeave: I } = v, D = () => {
          c.ctx.isUnmounted ? r(g) : n(g, u, h);
        }, U = () => {
          const L = g._isLeaving || !!g[Ts];
          g._isLeaving && g[Ts](
            !0
            /* cancelled */
          ), v.persisted && !L ? D() : S(g, () => {
            D(), I && I();
          });
        };
        O ? O(g, D, U) : U();
      }
    else
      n(g, u, h);
  }, Se = (c, u, h, b = !1, m = !1) => {
    const {
      type: g,
      props: x,
      ref: v,
      children: y,
      dynamicChildren: _,
      shapeFlag: P,
      patchFlag: S,
      dirs: O,
      cacheIndex: I,
      memo: D
    } = c;
    if (S === -2 && (m = !1), v != null && (Le(), Ct(v, null, h, c, !0), Ke()), I != null && (u.renderCache[I] = void 0), P & 256) {
      u.ctx.deactivate(c);
      return;
    }
    const U = P & 1 && O, L = !Tt(c);
    let z;
    if (L && (z = x && x.onVnodeBeforeUnmount) && Ee(z, u, c), P & 6)
      zr(c.component, h, b);
    else {
      if (P & 128) {
        c.suspense.unmount(h, b);
        return;
      }
      U && Xe(c, null, u, "beforeUnmount"), P & 64 ? c.type.remove(
        c,
        u,
        h,
        gt,
        b
      ) : _ && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !_.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== de || S > 0 && S & 64) ? pt(
        _,
        u,
        h,
        !1,
        !0
      ) : (g === de && S & 384 || !m && P & 16) && pt(y, u, h), b && ln(c);
    }
    const Q = D != null && I == null;
    (L && (z = x && x.onVnodeUnmounted) || U || Q) && ae(() => {
      z && Ee(z, u, c), U && Xe(c, null, u, "unmounted"), Q && (c.el = null);
    }, h);
  }, ln = (c) => {
    const { type: u, el: h, anchor: b, transition: m } = c;
    if (u === de) {
      Yr(h, b);
      return;
    }
    if (u === As) {
      w(c);
      return;
    }
    const g = () => {
      r(h), m && !m.persisted && m.afterLeave && m.afterLeave();
    };
    if (c.shapeFlag & 1 && m && !m.persisted) {
      const { leave: x, delayLeave: v } = m, y = () => x(h, g);
      v ? v(c.el, g, y) : y();
    } else
      g();
  }, Yr = (c, u) => {
    let h;
    for (; c !== u; )
      h = C(c), r(c), c = h;
    r(u);
  }, zr = (c, u, h) => {
    const { bum: b, scope: m, job: g, subTree: x, um: v, m: y, a: _ } = c;
    An(y), An(_), b && ys(b), m.stop(), g && (g.flags |= 8, Se(x, c, u, h)), v && ae(v, u), ae(() => {
      c.isUnmounted = !0;
    }, u);
  }, pt = (c, u, h, b = !1, m = !1, g = 0) => {
    for (let x = g; x < c.length; x++)
      Se(c[x], u, h, b, m);
  }, Ut = (c) => {
    if (c.shapeFlag & 6)
      return Ut(c.component.subTree);
    if (c.shapeFlag & 128)
      return c.suspense.next();
    const u = C(c.anchor || c.el), h = u && u[Gi];
    return h ? C(h) : u;
  };
  let ms = !1;
  const cn = (c, u, h) => {
    let b;
    c == null ? u._vnode && (Se(u._vnode, null, null, !0), b = u._vnode.component) : M(
      u._vnode || null,
      c,
      u,
      null,
      null,
      null,
      h
    ), u._vnode = c, ms || (ms = !0, mn(b), pr(), ms = !1);
  }, gt = {
    p: M,
    um: Se,
    m: ze,
    r: ln,
    mt: _s,
    mc: Be,
    pc: W,
    pbc: Je,
    n: Ut,
    o: e
  };
  return {
    render: cn,
    hydrate: void 0,
    createApp: go(cn)
  };
}
function Os({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function Ze({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Mo(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Nr(e, t, s = !1) {
  const n = e.children, r = t.children;
  if (R(n) && R(r))
    for (let i = 0; i < n.length; i++) {
      const o = n[i];
      let l = r[i];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = r[i] = $e(r[i]), l.el = o.el), !s && l.patchFlag !== -2 && Nr(o, l)), l.type === gs && (l.patchFlag === -1 && (l = r[i] = $e(l)), l.el = o.el), l.type === We && !l.el && (l.el = o.el);
    }
}
function Ro(e) {
  const t = e.slice(), s = [0];
  let n, r, i, o, l;
  const f = e.length;
  for (n = 0; n < f; n++) {
    const d = e[n];
    if (d !== 0) {
      if (r = s[s.length - 1], e[r] < d) {
        t[n] = r, s.push(n);
        continue;
      }
      for (i = 0, o = s.length - 1; i < o; )
        l = i + o >> 1, e[s[l]] < d ? i = l + 1 : o = l;
      d < e[s[i]] && (i > 0 && (t[n] = s[i - 1]), s[i] = n);
    }
  }
  for (i = s.length, o = s[i - 1]; i-- > 0; )
    s[i] = o, o = t[o];
  return s;
}
function Lr(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : Lr(t);
}
function An(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function Kr(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? Kr(t.subTree) : null;
}
const Ur = (e) => e.__isSuspense;
function Fo(e, t) {
  t && t.pendingBranch ? R(e) ? t.effects.push(...e) : t.effects.push(e) : Ui(e);
}
const de = /* @__PURE__ */ Symbol.for("v-fgt"), gs = /* @__PURE__ */ Symbol.for("v-txt"), We = /* @__PURE__ */ Symbol.for("v-cmt"), As = /* @__PURE__ */ Symbol.for("v-stc"), st = [];
let pe = null;
function X(e = !1) {
  st.push(pe = e ? null : []);
}
function Wr() {
  st.pop(), pe = st[st.length - 1] || null;
}
let Mt = 1;
function Pn(e, t = !1) {
  Mt += e, e < 0 && pe && t && (pe.hasOnce = !0);
}
function Vr(e) {
  return e.dynamicChildren = Mt > 0 ? pe || lt : null, Wr(), Mt > 0 && pe && pe.push(e), e;
}
function Z(e, t, s, n, r, i) {
  return Vr(
    ee(
      e,
      t,
      s,
      n,
      r,
      i,
      !0
    )
  );
}
function jo(e, t, s, n, r) {
  return Vr(
    Ne(
      e,
      t,
      s,
      n,
      r,
      !0
    )
  );
}
function Br(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function bt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const kr = ({ key: e }) => e ?? null, Jt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? Y(e) || /* @__PURE__ */ re(e) || F(e) ? { i: Me, r: e, k: t, f: !!s } : e : null);
function ee(e, t = null, s = null, n = 0, r = null, i = e === de ? 0 : 1, o = !1, l = !1) {
  const f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && kr(t),
    ref: t && Jt(t),
    scopeId: _r,
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
    shapeFlag: i,
    patchFlag: n,
    dynamicProps: r,
    dynamicChildren: null,
    appContext: null,
    ctx: Me
  };
  return l ? (ns(f, s), i & 128 && e.normalize(f)) : s && (f.shapeFlag |= Y(s) ? 8 : 16), Mt > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  pe && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (f.patchFlag > 0 || i & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  f.patchFlag !== 32 && pe.push(f), f;
}
const Ne = Do;
function Do(e, t = null, s = null, n = 0, r = null, i = !1) {
  if ((!e || e === oo) && (e = We), Br(e)) {
    const l = dt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ns(l, s), Mt > 0 && !i && pe && (l.shapeFlag & 6 ? pe[pe.indexOf(e)] = l : pe.push(l)), l.patchFlag = -2, l;
  }
  if (qo(e) && (e = e.__vccOpts), t) {
    t = $o(t);
    let { class: l, style: f } = t;
    l && !Y(l) && (t.class = ct(l)), K(f) && (/* @__PURE__ */ Zs(f) && !R(f) && (f = se({}, f)), t.style = ks(f));
  }
  const o = Y(e) ? 1 : Ur(e) ? 128 : ds(e) ? 64 : K(e) ? 4 : F(e) ? 2 : 0;
  return ee(
    e,
    t,
    s,
    n,
    r,
    o,
    i,
    !0
  );
}
function $o(e) {
  return e ? /* @__PURE__ */ Zs(e) || Rr(e) ? se({}, e) : e : null;
}
function dt(e, t, s = !1, n = !1) {
  const { props: r, ref: i, patchFlag: o, children: l, transition: f } = e, d = t ? Ho(r || {}, t) : r, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && kr(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && i ? R(i) ? i.concat(Jt(t)) : [i, Jt(t)] : Jt(t)
    ) : i,
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
    patchFlag: t && e.type !== de ? o === -1 ? 16 : o | 16 : o,
    dynamicProps: e.dynamicProps,
    dynamicChildren: e.dynamicChildren,
    appContext: e.appContext,
    dirs: e.dirs,
    transition: f,
    // These should technically only be non-null on mounted VNodes. However,
    // they *should* be copied for kept-alive vnodes. So we just always copy
    // them since them being non-null during a mount doesn't affect the logic as
    // they will simply be overwritten.
    component: e.component,
    suspense: e.suspense,
    ssContent: e.ssContent && dt(e.ssContent),
    ssFallback: e.ssFallback && dt(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return f && n && en(
    a,
    f.clone(a)
  ), a;
}
function Yt(e = " ", t = 0) {
  return Ne(gs, null, e, t);
}
function it(e = "", t = !1) {
  return t ? (X(), jo(We, null, e)) : Ne(We, null, e);
}
function Pe(e) {
  return e == null || typeof e == "boolean" ? Ne(We) : R(e) ? Ne(
    de,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Br(e) ? $e(e) : Ne(gs, null, String(e));
}
function $e(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : dt(e);
}
function ns(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (R(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const r = t.default;
      r && (r._c && (r._d = !1), ns(e, r()), r._c && (r._d = !0));
      return;
    } else {
      s = 32;
      const r = t._;
      !r && !Rr(t) ? t._ctx = Me : r === 3 && Me && (Me.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (F(t)) {
    if (n & 65) {
      ns(e, { default: t });
      return;
    }
    t = { default: t, _ctx: Me }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [Yt(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function Ho(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const r in n)
      if (r === "class")
        t.class !== n.class && (t.class = ct([t.class, n.class]));
      else if (r === "style")
        t.style = ks([t.style, n.style]);
      else if (is(r)) {
        const i = t[r], o = n[r];
        o && i !== o && !(R(i) && i.includes(o)) ? t[r] = i ? [].concat(i, o) : o : o == null && i == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !os(r) && (t[r] = o);
      } else r !== "" && (t[r] = n[r]);
  }
  return t;
}
function Ee(e, t, s, n = null) {
  ve(e, t, 7, [
    s,
    n
  ]);
}
const No = Or();
let Lo = 0;
function Ko(e, t, s) {
  const n = e.type, r = (t ? t.appContext : e.appContext) || No, i = {
    uid: Lo++,
    vnode: e,
    type: n,
    parent: t,
    appContext: r,
    root: null,
    // to be immediately set
    next: null,
    subTree: null,
    // will be set synchronously right after creation
    effect: null,
    update: null,
    // will be set synchronously right after creation
    job: null,
    scope: new fi(
      !0
      /* detached */
    ),
    render: null,
    proxy: null,
    exposed: null,
    exposeProxy: null,
    withProxy: null,
    provides: t ? t.provides : Object.create(r.provides),
    ids: t ? t.ids : ["", 0, 0],
    accessCache: null,
    renderCache: [],
    // local resolved assets
    components: null,
    directives: null,
    // resolved props and emits options
    propsOptions: jr(n, r),
    emitsOptions: Ar(n, r),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: B,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: B,
    data: B,
    props: B,
    attrs: B,
    slots: B,
    refs: B,
    setupState: B,
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
  return i.ctx = { _: i }, i.root = t ? t.root : i, i.emit = mo.bind(null, i), e.ce && e.ce(i), i;
}
let ue = null;
const Uo = () => ue || Me;
let rs, Rt;
{
  const e = cs(), t = (s, n) => {
    let r;
    return (r = e[s]) || (r = e[s] = []), r.push(n), (i) => {
      r.length > 1 ? r.forEach((o) => o(i)) : r[0](i);
    };
  };
  rs = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => ue = s
  ), Rt = t(
    "__VUE_SSR_SETTERS__",
    (s) => Ft = s
  );
}
const $t = (e) => {
  const t = ue;
  return rs(e), e.scope.on(), () => {
    e.scope.off(), rs(t);
  };
}, In = () => {
  ue && ue.scope.off(), rs(null);
};
function qr(e) {
  return e.vnode.shapeFlag & 4;
}
let Ft = !1;
function Wo(e, t = !1, s = !1) {
  t && Rt(t);
  const { props: n, children: r } = e.vnode, i = qr(e);
  wo(e, n, i, t), Oo(e, r, s || t);
  const o = i ? Vo(e, t) : void 0;
  return t && Rt(!1), o;
}
function Vo(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, lo);
  const { setup: n } = s;
  if (n) {
    Le();
    const r = e.setupContext = n.length > 1 ? ko(e) : null, i = $t(e), o = Dt(
      n,
      e,
      0,
      [
        e.props,
        r
      ]
    ), l = Vn(o);
    if (Ke(), i(), (l || e.sp) && !Tt(e) && vr(e), l) {
      if (o.then(In, In), t)
        return o.then((f) => {
          Rt(!0);
          try {
            Mn(e, f, t);
          } finally {
            Rt(!1);
          }
        }).catch((f) => {
          as(f, e, 0);
        });
      e.asyncDep = o;
    } else
      Mn(e, o);
  } else
    Gr(e);
}
function Mn(e, t, s) {
  F(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : K(t) && (e.setupState = ar(t)), Gr(e);
}
function Gr(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Re);
  {
    const r = $t(e);
    Le();
    try {
      co(e);
    } finally {
      Ke(), r();
    }
  }
}
const Bo = {
  get(e, t) {
    return ne(e, "get", ""), e[t];
  }
};
function ko(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, Bo),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function rn(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(ar(Pi(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in Et)
        return Et[s](e);
    },
    has(t, s) {
      return s in t || s in Et;
    }
  })) : e.proxy;
}
function qo(e) {
  return F(e) && "__vccOpts" in e;
}
const zt = (e, t) => /* @__PURE__ */ Di(e, t, Ft), Go = "3.5.42";
/**
* @vue/runtime-dom v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Ks;
const Rn = typeof window < "u" && window.trustedTypes;
if (Rn)
  try {
    Ks = /* @__PURE__ */ Rn.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Jr = Ks ? (e) => Ks.createHTML(e) : (e) => e, Jo = "http://www.w3.org/2000/svg", Yo = "http://www.w3.org/1998/Math/MathML", De = typeof document < "u" ? document : null, Fn = De && /* @__PURE__ */ De.createElement("template"), zo = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const r = t === "svg" ? De.createElementNS(Jo, e) : t === "mathml" ? De.createElementNS(Yo, e) : s ? De.createElement(e, { is: s }) : De.createElement(e);
    return e === "select" && n && n.multiple != null && r.setAttribute("multiple", n.multiple), r;
  },
  createText: (e) => De.createTextNode(e),
  createComment: (e) => De.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => De.querySelector(e),
  setScopeId(e, t) {
    e.setAttribute(t, "");
  },
  // __UNSAFE__
  // Reason: innerHTML.
  // Static content here can only come from compiled templates.
  // As long as the user only uses trusted templates, this is safe.
  insertStaticContent(e, t, s, n, r, i) {
    const o = s ? s.previousSibling : t.lastChild;
    if (r && (r === i || r.nextSibling))
      for (; t.insertBefore(r.cloneNode(!0), s), !(r === i || !(r = r.nextSibling)); )
        ;
    else {
      Fn.innerHTML = Jr(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = Fn.content;
      if (n === "svg" || n === "mathml") {
        const f = l.firstChild;
        for (; f.firstChild; )
          l.appendChild(f.firstChild);
        l.removeChild(f);
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
}, Xo = /* @__PURE__ */ Symbol("_vtc");
function Zo(e, t, s) {
  const n = e[Xo];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const jn = /* @__PURE__ */ Symbol("_vod"), Qo = /* @__PURE__ */ Symbol("_vsh"), el = /* @__PURE__ */ Symbol(""), tl = /(?:^|;)\s*display\s*:/;
function sl(e, t, s) {
  const n = e.style, r = Y(s);
  let i = !1;
  if (s && !r) {
    if (t)
      if (Y(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && vt(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && vt(n, o, "");
    for (const o in s) {
      o === "display" && (i = !0);
      const l = s[o];
      l != null ? rl(
        e,
        o,
        !Y(t) && t ? t[o] : void 0,
        l
      ) || vt(n, o, l) : vt(n, o, "");
    }
  } else if (r) {
    if (t !== s) {
      const o = n[el];
      o && (s += ";" + o), n.cssText = s, i = tl.test(s);
    }
  } else t && e.removeAttribute("style");
  jn in e && (e[jn] = i ? n.display : "", e[Qo] && (n.display = "none"));
}
const kt = /\s*!important$/;
function vt(e, t, s) {
  if (R(s))
    s.forEach((n) => vt(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    kt.test(s) ? e.setProperty(t, s.replace(kt, ""), "important") : e.setProperty(t, s);
  else {
    const n = nl(e, t);
    kt.test(s) ? e.setProperty(
      nt(n),
      s.replace(kt, ""),
      "important"
    ) : e[n] = s;
  }
}
const Dn = ["Webkit", "Moz", "ms"], Ps = {};
function nl(e, t) {
  const s = Ps[t];
  if (s)
    return s;
  let n = me(t);
  if (n !== "filter" && n in e)
    return Ps[t] = n;
  n = qn(n);
  for (let r = 0; r < Dn.length; r++) {
    const i = Dn[r] + n;
    if (i in e)
      return Ps[t] = i;
  }
  return t;
}
function rl(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && Y(n) && s === n;
}
const $n = "http://www.w3.org/1999/xlink";
function Hn(e, t, s, n, r, i = li(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS($n, t.slice(6, t.length)) : e.setAttributeNS($n, t, s) : s == null || i && !Jn(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    i ? "" : Fe(s) ? String(s) : s
  );
}
function Nn(e, t, s, n, r) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? Jr(s) : s);
    return;
  }
  const i = e.tagName;
  if (t === "value" && i !== "PROGRESS" && // custom elements may use _value internally
  !i.includes("-")) {
    const l = i === "OPTION" ? e.getAttribute("value") || "" : e.value, f = s == null ? (
      // #11647: value should be set as empty string for null and undefined,
      // but <input type="checkbox"> should be set as 'on'.
      e.type === "checkbox" ? "on" : ""
    ) : String(s);
    (l !== f || !("_value" in e)) && (e.value = f), s == null && e.removeAttribute(t), e._value = s;
    return;
  }
  let o = !1;
  if (s === "" || s == null) {
    const l = typeof e[t];
    l === "boolean" ? s = Jn(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(r || t);
}
function il(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function ol(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const Ln = /* @__PURE__ */ Symbol("_vei");
function ll(e, t, s, n, r = null) {
  const i = e[Ln] || (e[Ln] = {}), o = i[t];
  if (n && o)
    o.value = n;
  else {
    const [l, f] = ul(t);
    if (n) {
      const d = i[t] = hl(
        n,
        r
      );
      il(e, l, d, f);
    } else o && (ol(e, l, o, f), i[t] = void 0);
  }
}
const cl = /(Once|Passive|Capture)$/, fl = /^on:?(?:Once|Passive|Capture)$/;
function ul(e) {
  let t, s;
  for (; (s = e.match(cl)) && !fl.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : nt(e.slice(2)), t];
}
let Is = 0;
const al = /* @__PURE__ */ Promise.resolve(), dl = () => Is || (al.then(() => Is = 0), Is = Date.now());
function hl(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const r = s.value;
    if (R(r)) {
      const i = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        i.call(n), n._stopped = !0;
      };
      const o = r.slice(), l = [n];
      for (let f = 0; f < o.length && !n._stopped; f++) {
        const d = o[f];
        d && ve(
          d,
          t,
          5,
          l
        );
      }
    } else
      ve(
        r,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = dl(), s;
}
const Kn = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, pl = (e, t, s, n, r, i) => {
  const o = r === "svg";
  t === "class" ? Zo(e, n, o) : t === "style" ? sl(e, s, n) : is(t) ? os(t) || ll(e, t, s, n, i) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : gl(e, t, n, o)) ? (Nn(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Hn(e, t, n, o, i, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (_l(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !Y(n))) ? Nn(e, me(t), n, i, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), Hn(e, t, n, o));
};
function gl(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Kn(t) && F(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const r = e.tagName;
    if (r === "IMG" || r === "VIDEO" || r === "CANVAS" || r === "SOURCE")
      return !1;
  }
  return Kn(t) && Y(s) ? !1 : t in e;
}
function _l(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = me(t);
  return Array.isArray(s) ? s.some((r) => me(r) === n) : Object.keys(s).some((r) => me(r) === n);
}
const ml = /* @__PURE__ */ se({ patchProp: pl }, zo);
let Un;
function bl() {
  return Un || (Un = Po(ml));
}
const yl = ((...e) => {
  const t = bl().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const r = xl(n);
    if (!r) return;
    const i = t._component;
    !F(i) && !i.render && !i.template && (i.template = r.innerHTML), r.nodeType === 1 && (r.textContent = "");
    const o = s(r, !1, vl(r));
    return r instanceof Element && (r.removeAttribute("v-cloak"), r.setAttribute("data-v-app", "")), o;
  }, t;
});
function vl(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function xl(e) {
  return Y(e) ? document.querySelector(e) : e;
}
let Us = null;
function Sl(e) {
  Us = e ?? null;
}
function oe(e, t) {
  return Us ? Us(e, t) : e;
}
const wl = { class: "geom" }, Cl = { class: "head" }, Tl = { class: "title" }, El = {
  key: 0,
  class: "muted"
}, Ol = {
  key: 0,
  class: "empty"
}, Al = { class: "stage" }, Pl = ["src", "alt"], Il = {
  key: 1,
  class: "muted"
}, Ml = {
  key: 0,
  class: "strip"
}, Rl = ["title", "onClick"], Fl = ["src"], jl = {
  key: 1,
  class: "ph"
}, Dl = { class: "no" }, $l = { class: "facts" }, Hl = { class: "row" }, Nl = { class: "muted" }, Ll = {
  key: 0,
  class: "muted"
}, Kl = {
  key: 0,
  class: "objects"
}, Ul = { class: "name" }, Wl = { class: "muted" }, Vl = { class: "muted" }, Bl = {
  key: 1,
  class: "muted"
}, qt = "geometry-session-main", kl = /* @__PURE__ */ Yi({
  __name: "GeometryPanel",
  props: {
    api: {}
  },
  setup(e) {
    const t = e, s = /* @__PURE__ */ Pt({ chatId: null, hasSession: !1 }), n = /* @__PURE__ */ Pt({}), r = /* @__PURE__ */ Ii(null), i = [];
    let o = !1;
    const l = (T) => `${T.name}@${T.at}`, f = zt(() => s.renders ?? []), d = zt(() => {
      var T;
      return r.value ?? s.current ?? ((T = f.value.at(-1)) == null ? void 0 : T.name) ?? null;
    }), a = zt(() => {
      const T = f.value.find((A) => A.name === d.value);
      return T ? n[l(T)] ?? "" : "";
    }), p = (T) => {
      r.value = T;
    }, C = () => {
      r.value = null;
    }, E = (T, A) => `${A + 1}. ${T.name}${T.name === s.current ? ` — ${oe("latest")}` : ""}`;
    function $(T, A = {}) {
      t.api.send("plugin.panel.input", { panelId: qt, inputType: T, data: A });
    }
    function M() {
      for (const T of f.value) n[l(T)] || $("render", { name: T.name });
    }
    function k() {
      o || (o = t.api.send("plugin.panel.open", {
        panelId: qt,
        panelType: "geometry.session",
        parameters: { chatId: t.api.currentChatId() ?? "" }
      }));
    }
    return Sr(() => {
      k(), i.push(t.api.on("conn", ((T) => {
        T != null && T.on && (o = !1, k());
      }))), i.push(t.api.on("plugin.panel.event", ((T) => {
        if (!(T.panelId !== qt || !T.data)) {
          if (T.eventType === "state") {
            const A = T.data;
            Object.assign(
              s,
              { source: void 0, view: void 0, objects: [], renders: [], current: null },
              A
            ), r.value && !f.value.some((j) => j.name === r.value) && (r.value = null), M();
          } else if (T.eventType === "render") {
            const A = T.data;
            if (A.missing || !A.base64) return;
            n[`${A.name}@${A.at}`] = `data:${A.mimeType || "image/png"};base64,${A.base64}`;
          }
        }
      }))), i.push(t.api.onChatChange((T) => {
        for (const A of Object.keys(n)) delete n[A];
        r.value = null, $("bind", { chatId: T ?? "" });
      }));
    }), wr(() => {
      t.api.send("plugin.panel.close", { panelId: qt }), i.forEach((T) => T());
    }), (T, A) => {
      var j;
      return X(), Z("div", wl, [
        ee("div", Cl, [
          ee("span", Tl, J(le(oe)("Geometry")), 1),
          s.hasSession && s.view ? (X(), Z("span", El, [
            Yt(J(s.view.id) + " — " + J(s.view.width) + "×" + J(s.view.height) + " px ", 1),
            s.view.fromBox ? (X(), Z(de, { key: 0 }, [
              Yt(", " + J(le(oe)("from box")) + " “" + J(s.view.fromBox) + "”", 1)
            ], 64)) : it("", !0),
            s.view.deskewed ? (X(), Z(de, { key: 1 }, [
              Yt(", " + J(le(oe)("deskewed")), 1)
            ], 64)) : it("", !0)
          ])) : it("", !0)
        ]),
        s.hasSession ? (X(), Z(de, { key: 1 }, [
          ee("div", Al, [
            a.value ? (X(), Z("img", {
              key: 0,
              src: a.value,
              alt: le(oe)("Marked-up frame"),
              draggable: "false"
            }, null, 8, Pl)) : (X(), Z("div", Il, J(le(oe)("Waiting for the first render…")), 1))
          ]),
          f.value.length ? (X(), Z("div", Ml, [
            (X(!0), Z(de, null, vn(f.value, (w, G) => (X(), Z("button", {
              key: w.name + w.at,
              type: "button",
              class: ct(["thumb", { sel: w.name === d.value, live: w.name === s.current }]),
              title: E(w, G),
              onClick: (_e) => p(w.name)
            }, [
              n[l(w)] ? (X(), Z("img", {
                key: 0,
                src: n[l(w)],
                alt: ""
              }, null, 8, Fl)) : (X(), Z("span", jl, "…")),
              ee("span", Dl, J(G + 1), 1)
            ], 10, Rl))), 128)),
            d.value !== s.current ? (X(), Z("button", {
              key: 0,
              type: "button",
              class: "latest",
              onClick: A[0] || (A[0] = (w) => C())
            }, J(le(oe)("Latest")), 1)) : it("", !0)
          ])) : it("", !0),
          ee("div", $l, [
            ee("div", Hl, [
              ee("span", Nl, J(le(oe)("Source")), 1),
              ee("code", null, J(s.source), 1),
              s.sourceWidth ? (X(), Z("span", Ll, J(s.sourceWidth) + "×" + J(s.sourceHeight) + " px", 1)) : it("", !0)
            ]),
            (j = s.objects) != null && j.length ? (X(), Z("table", Kl, [
              ee("tbody", null, [
                (X(!0), Z(de, null, vn(s.objects, (w) => (X(), Z("tr", {
                  key: w.name,
                  class: ct({ away: !w.visible })
                }, [
                  ee("td", Ul, J(w.name), 1),
                  ee("td", Wl, J(w.kind === "box" ? le(oe)("box") : le(oe)("point")), 1),
                  ee("td", null, [
                    ee("code", null, J(w.where), 1)
                  ]),
                  ee("td", {
                    class: ct(w.status === "accepted" ? "ok" : "edit")
                  }, J(w.status === "accepted" ? le(oe)("accepted") : le(oe)("editing")), 3),
                  ee("td", Vl, J(w.visible ? "" : le(oe)("outside this view")), 1)
                ], 2))), 128))
              ])
            ])) : (X(), Z("div", Bl, J(le(oe)("Nothing marked yet.")), 1))
          ])
        ], 64)) : (X(), Z("div", Ol, J(le(oe)("No image is open in this chat. The panel fills in as soon as the agent opens one.")), 1))
      ]);
    };
  }
}), ql = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, r] of t)
    s[n] = r;
  return s;
}, Gl = /* @__PURE__ */ ql(kl, [["__scopeId", "data-v-96f138f3"]]);
function Yl(e, t) {
  var n;
  Sl((n = t.t) == null ? void 0 : n.bind(t));
  let s = yl(Gl, { api: t });
  return s.mount(e), { destroy: () => {
    s == null || s.unmount(), s = null;
  } };
}
export {
  Yl as mount
};
