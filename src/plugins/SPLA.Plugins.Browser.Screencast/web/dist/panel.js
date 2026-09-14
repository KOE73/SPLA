(function(){"use strict";try{if(typeof document<"u"){var e=document.createElement("style");e.appendChild(document.createTextNode(".screencast-surface[data-v-f9b5c84e]{display:flex;flex-direction:column;width:100%;height:100%;min-height:0;background:#111}.browser-bar[data-v-f9b5c84e]{display:flex;align-items:center;gap:5px;height:32px;padding:3px 6px;background:var(--panel);border-bottom:1px solid var(--border)}.browser-bar input[data-v-f9b5c84e]{flex:1;min-width:80px;height:24px;padding:2px 7px;color:var(--text);background:var(--bg);border:1px solid var(--border);border-radius:4px}.browser-bar button[data-v-f9b5c84e]{height:24px;color:var(--text);background:transparent;border:1px solid var(--border);border-radius:4px}.state[data-v-f9b5c84e]{color:var(--muted);font-size:var(--fs-xs)}.browser-viewport[data-v-f9b5c84e]{flex:1;min-height:0;display:grid;place-items:center;overflow:hidden}.browser-viewport img[data-v-f9b5c84e]{display:block;max-width:100%;max-height:100%;object-fit:contain;-webkit-user-select:none;user-select:none}.browser-empty[data-v-f9b5c84e]{color:var(--muted)}")),document.head.appendChild(e)}}catch(r){console.error("vite-plugin-css-injected-by-js",r)}})();
/**
* @vue/shared v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Bs(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const U = {}, st = [], Ce = () => {
}, Jn = () => !1, rs = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), is = (e) => e.startsWith("onUpdate:"), Q = Object.assign, Ws = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, ti = Object.prototype.hasOwnProperty, K = (e, t) => ti.call(e, t), R = Array.isArray, Ke = (e) => It(e) === "[object Map]", Xt = (e) => It(e) === "[object Set]", mn = (e) => It(e) === "[object Date]", F = (e) => typeof e == "function", J = (e) => typeof e == "string", Te = (e) => typeof e == "symbol", B = (e) => e !== null && typeof e == "object", Yn = (e) => (B(e) || F(e)) && F(e.then) && F(e.catch), zn = Object.prototype.toString, It = (e) => zn.call(e), si = (e) => It(e).slice(8, -1), Xn = (e) => It(e) === "[object Object]", qs = (e) => J(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, bt = /* @__PURE__ */ Bs(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), os = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, ni = /-\w/g, fe = os(
  (e) => e.replace(ni, (t) => t.slice(1).toUpperCase())
), ri = /\B([A-Z])/g, Ze = os(
  (e) => e.replace(ri, "-$1").toLowerCase()
), Zn = os((e) => e.charAt(0).toUpperCase() + e.slice(1)), _s = os(
  (e) => e ? `on${Zn(e)}` : ""
), we = (e, t) => !Object.is(e, t), Gt = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Qn = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, Gs = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let _n;
const ls = () => _n || (_n = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function Js(e) {
  if (R(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], r = J(n) ? ci(n) : Js(n);
      if (r)
        for (const i in r)
          t[i] = r[i];
    }
    return t;
  } else if (J(e) || B(e))
    return e;
}
const ii = /;(?![^(]*\))/g, oi = /:([^]+)/, li = /\/\*[^]*?\*\//g;
function ci(e) {
  const t = {};
  return e.replace(li, "").split(ii).forEach((s) => {
    if (s) {
      const n = s.split(oi);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function Ys(e) {
  let t = "";
  if (J(e))
    t = e;
  else if (R(e))
    for (let s = 0; s < e.length; s++) {
      const n = Ys(e[s]);
      n && (t += n + " ");
    }
  else if (B(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const fi = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", ui = /* @__PURE__ */ Bs(fi);
function kn(e) {
  return !!e || e === "";
}
function ai(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = cs(e[n], t[n]);
  return s;
}
function bn(e, t) {
  if (e.size !== t.size) return !1;
  const s = Array.from(t), n = new Uint8Array(s.length);
  for (const r of e) {
    let i = -1;
    for (let o = 0; o < s.length; o++)
      if (!n[o] && cs(r, s[o])) {
        i = o;
        break;
      }
    if (i < 0) return !1;
    n[i] = 1;
  }
  return !0;
}
function cs(e, t) {
  if (e === t) return !0;
  let s = mn(e), n = mn(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Te(e), n = Te(t), s || n)
    return e === t;
  if (s = R(e), n = R(t), s || n)
    return s && n ? ai(e, t) : !1;
  if (s = B(e), n = B(t), s || n) {
    if (!s || !n)
      return !1;
    if (s = Ke(e), n = Ke(t), s || n || (s = Xt(e), n = Xt(t), s || n))
      return s && n ? bn(e, t) : !1;
    const r = Object.keys(e).length, i = Object.keys(t).length;
    if (r !== i)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), f = t.hasOwnProperty(o);
      if (l && !f || !l && f || !cs(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
const er = (e) => !!(e && e.__v_isRef === !0), Jt = (e) => J(e) ? e : e == null ? "" : R(e) || B(e) && (e.toString === zn || !F(e.toString)) ? er(e) ? Jt(e.value) : JSON.stringify(e, tr, 2) : String(e), tr = (e, t) => er(t) ? tr(e, t.value) : Ke(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, r], i) => (s[bs(n, i) + " =>"] = r, s),
    {}
  )
} : Xt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => bs(s))
} : Te(t) ? bs(t) : B(t) && !R(t) && !Xn(t) ? String(t) : t, bs = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Te(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Z;
class di {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && Z && (Z.active ? (this.parent = Z, this.index = (Z.scopes || (Z.scopes = [])).push(
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
      const s = Z;
      try {
        return Z = this, t();
      } finally {
        Z = s;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = Z, Z = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (Z === this)
        Z = this.prevScope;
      else {
        let t = Z;
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
function hi() {
  return Z;
}
let q;
const ys = /* @__PURE__ */ new WeakSet();
class sr {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, Z && (Z.active ? Z.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, ys.has(this) && (ys.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || rr(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, yn(this), ir(this);
    const t = q, s = ue;
    q = this, ue = !0;
    try {
      return this.fn();
    } finally {
      or(this), q = t, ue = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        Zs(t);
      this.deps = this.depsTail = void 0, yn(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? ys.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Rs(this) && this.run();
  }
  get dirty() {
    return Rs(this);
  }
}
let nr = 0, yt, xt;
function rr(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = xt, xt = e;
    return;
  }
  e.next = yt, yt = e;
}
function zs() {
  nr++;
}
function Xs() {
  if (--nr > 0)
    return;
  if (xt) {
    let t = xt;
    for (xt = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; yt; ) {
    let t = yt;
    for (yt = void 0; t; ) {
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
function ir(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function or(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const r = n.prevDep;
    n.version === -1 ? (n === s && (s = r), Zs(n), pi(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = r;
  }
  e.deps = t, e.depsTail = s;
}
function Rs(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (lr(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function lr(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Ct) || (e.globalVersion = Ct, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Rs(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = q, n = ue;
  q = e, ue = !0;
  try {
    ir(e);
    const r = e.fn(e._value);
    (t.version === 0 || we(r, e._value)) && (e.flags |= 128, e._value = r, t.version++);
  } catch (r) {
    throw t.version++, r;
  } finally {
    q = s, ue = n, or(e), e.flags &= -3;
  }
}
function Zs(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: r } = e;
  if (n && (n.nextSub = r, e.prevSub = void 0), r && (r.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let i = s.computed.deps; i; i = i.nextDep)
      Zs(i, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function pi(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let ue = !0;
const cr = [];
function Re() {
  cr.push(ue), ue = !1;
}
function Fe() {
  const e = cr.pop();
  ue = e === void 0 ? !0 : e;
}
function yn(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const s = q;
    q = void 0;
    try {
      t();
    } finally {
      q = s;
    }
  }
}
let Ct = 0;
class gi {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class Qs {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!q || !ue || q === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== q)
      s = this.activeLink = new gi(q, this), q.deps ? (s.prevDep = q.depsTail, q.depsTail.nextDep = s, q.depsTail = s) : q.deps = q.depsTail = s, fr(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = q.depsTail, s.nextDep = void 0, q.depsTail.nextDep = s, q.depsTail = s, q.deps === s && (q.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, Ct++, this.notify(t);
  }
  notify(t) {
    zs();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      Xs();
    }
  }
}
function fr(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let n = t.deps; n; n = n.nextDep)
        fr(n);
    }
    const s = e.dep.subs;
    s !== e && (e.prevSub = s, s && (s.nextSub = e)), e.dep.subs = e;
  }
}
const Fs = /* @__PURE__ */ new WeakMap(), Je = /* @__PURE__ */ Symbol(
  ""
), Ds = /* @__PURE__ */ Symbol(
  ""
), Tt = /* @__PURE__ */ Symbol(
  ""
);
function k(e, t, s) {
  if (ue && q) {
    let n = Fs.get(e);
    n || Fs.set(e, n = /* @__PURE__ */ new Map());
    let r = n.get(s);
    r || (n.set(s, r = new Qs()), r.map = n, r.key = s), r.track();
  }
}
function Me(e, t, s, n, r, i) {
  const o = Fs.get(e);
  if (!o) {
    Ct++;
    return;
  }
  const l = (f) => {
    f && f.trigger();
  };
  if (zs(), t === "clear")
    o.forEach(l);
  else {
    const f = R(e), d = f && qs(s);
    if (f && s === "length") {
      const a = Number(n);
      o.forEach((p, C) => {
        (C === "length" || C === Tt || !Te(C) && C >= a) && l(p);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(Tt)), t) {
        case "add":
          f ? d && l(o.get("length")) : (l(o.get(Je)), Ke(e) && l(o.get(Ds)));
          break;
        case "delete":
          f || (l(o.get(Je)), Ke(e) && l(o.get(Ds)));
          break;
        case "set":
          Ke(e) && l(o.get(Je));
          break;
      }
  }
  Xs();
}
function Qe(e) {
  const t = /* @__PURE__ */ $(e);
  return t === e ? t : (k(t, "iterate", Tt), /* @__PURE__ */ ae(e) ? t : t.map(De));
}
function ks(e) {
  return k(e = /* @__PURE__ */ $(e), "iterate", Tt), e;
}
function ve(e, t) {
  return /* @__PURE__ */ Le(e) ? Et(/* @__PURE__ */ nt(e) ? De(t) : t) : De(t);
}
const mi = {
  __proto__: null,
  [Symbol.iterator]() {
    return xs(this, Symbol.iterator, (e) => ve(this, e));
  },
  concat(...e) {
    return Qe(this).concat(
      ...e.map((t) => R(t) ? Qe(t) : t)
    );
  },
  entries() {
    return xs(this, "entries", (e) => (e[1] = ve(this, e[1]), e));
  },
  every(e, t) {
    return Ee(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return Ee(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => ve(this, n)),
      arguments
    );
  },
  find(e, t) {
    return Ee(
      this,
      "find",
      e,
      t,
      (s) => ve(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return Ee(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return Ee(
      this,
      "findLast",
      e,
      t,
      (s) => ve(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return Ee(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return Ee(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return vs(this, "includes", e);
  },
  indexOf(...e) {
    return vs(this, "indexOf", e);
  },
  join(e) {
    return Qe(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return vs(this, "lastIndexOf", e);
  },
  map(e, t) {
    return Ee(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return at(this, "pop");
  },
  push(...e) {
    return at(this, "push", e);
  },
  reduce(e, ...t) {
    return xn(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return xn(this, "reduceRight", e, t);
  },
  shift() {
    return at(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return Ee(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return at(this, "splice", e);
  },
  toReversed() {
    return Qe(this).toReversed();
  },
  toSorted(e) {
    return Qe(this).toSorted(e);
  },
  toSpliced(...e) {
    return Qe(this).toSpliced(...e);
  },
  unshift(...e) {
    return at(this, "unshift", e);
  },
  values() {
    return xs(this, "values", (e) => ve(this, e));
  }
};
function xs(e, t, s) {
  const n = ks(e), r = n[t]();
  return n !== e && !/* @__PURE__ */ ae(e) && (r._next = r.next, r.next = () => {
    const i = r._next();
    return i.done || (i.value = s(i.value)), i;
  }), r;
}
const _i = Array.prototype;
function Ee(e, t, s, n, r, i) {
  const o = ks(e), l = o !== e && !/* @__PURE__ */ ae(e), f = o[t];
  if (f !== _i[t]) {
    const p = f.apply(e, i);
    return l ? De(p) : p;
  }
  let d = s;
  o !== e && (l ? d = function(p, C) {
    return s.call(this, ve(e, p), C, e);
  } : s.length > 2 && (d = function(p, C) {
    return s.call(this, p, C, e);
  }));
  const a = f.call(o, d, n);
  return l && r ? r(a) : a;
}
function xn(e, t, s, n) {
  const r = ks(e), i = r !== e && !/* @__PURE__ */ ae(e);
  let o = s, l = !1;
  r !== e && (i ? (l = n.length === 0, o = function(d, a, p) {
    return l && (l = !1, d = ve(e, d)), s.call(this, d, ve(e, a), p, e);
  }) : s.length > 3 && (o = function(d, a, p) {
    return s.call(this, d, a, p, e);
  }));
  const f = r[t](o, ...n);
  return l ? ve(e, f) : f;
}
function vs(e, t, s) {
  const n = /* @__PURE__ */ $(e);
  k(n, "iterate", Tt);
  const r = n[t](...s);
  return (r === -1 || r === !1) && /* @__PURE__ */ nn(s[0]) ? (s[0] = /* @__PURE__ */ $(s[0]), n[t](...s)) : r;
}
function at(e, t, s = []) {
  Re(), zs();
  const n = (/* @__PURE__ */ $(e))[t].apply(e, s);
  return Xs(), Fe(), n;
}
const bi = /* @__PURE__ */ Bs("__proto__,__v_isRef,__isVue"), ur = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Te)
);
function yi(e) {
  Te(e) || (e = String(e));
  const t = /* @__PURE__ */ $(this);
  return k(t, "has", e), t.hasOwnProperty(e);
}
class ar {
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
      return n === (r ? i ? Pi : gr : i ? pr : hr).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = R(t);
    if (!r) {
      let f;
      if (o && (f = mi[s]))
        return f;
      if (s === "hasOwnProperty")
        return yi;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ ee(t) ? t : n
    );
    if ((Te(s) ? ur.has(s) : bi(s)) || (r || k(t, "get", s), i))
      return l;
    if (/* @__PURE__ */ ee(l)) {
      const f = o && qs(s) ? l : l.value;
      return r && B(f) ? /* @__PURE__ */ Hs(f) : f;
    }
    return B(l) ? r ? /* @__PURE__ */ Hs(l) : /* @__PURE__ */ tn(l) : l;
  }
}
class dr extends ar {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, r) {
    let i = t[s];
    const o = R(t) && qs(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ Le(i);
      if (!/* @__PURE__ */ ae(n) && !/* @__PURE__ */ Le(n) && (i = /* @__PURE__ */ $(i), n = /* @__PURE__ */ $(n)), !o && /* @__PURE__ */ ee(i) && !/* @__PURE__ */ ee(n))
        return d || (i.value = n), !0;
    }
    const l = o ? Number(s) < t.length : K(t, s), f = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ ee(t) ? t : r
    );
    return t === /* @__PURE__ */ $(r) && f && (l ? we(n, i) && Me(t, "set", s, n) : Me(t, "add", s, n)), f;
  }
  deleteProperty(t, s) {
    const n = K(t, s);
    t[s];
    const r = Reflect.deleteProperty(t, s);
    return r && n && Me(t, "delete", s, void 0), r;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Te(s) || !ur.has(s)) && k(t, "has", s), n;
  }
  ownKeys(t) {
    return k(
      t,
      "iterate",
      R(t) ? "length" : Je
    ), Reflect.ownKeys(t);
  }
}
class xi extends ar {
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
const vi = /* @__PURE__ */ new dr(), Si = /* @__PURE__ */ new xi(), wi = /* @__PURE__ */ new dr(!0);
const js = (e) => e, Kt = (e) => Reflect.getPrototypeOf(e);
function Ci(e, t, s) {
  return function(...n) {
    const r = this.__v_raw, i = /* @__PURE__ */ $(r), o = Ke(i), l = e === "entries" || e === Symbol.iterator && o, f = e === "keys" && o, d = r[e](...n), a = s ? js : t ? Et : De;
    return !t && k(
      i,
      "iterate",
      f ? Ds : Je
    ), Q(
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
function Lt(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function Ti(e, t) {
  const s = {
    get(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ $(i), l = /* @__PURE__ */ $(r);
      e || (we(r, l) && k(o, "get", r), k(o, "get", l));
      const { has: f } = Kt(o), d = t ? js : e ? Et : De;
      if (f.call(o, r))
        return d(i.get(r));
      if (f.call(o, l))
        return d(i.get(l));
      i !== o && i.get(r);
    },
    get size() {
      const r = this.__v_raw;
      return !e && k(/* @__PURE__ */ $(r), "iterate", Je), r.size;
    },
    has(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ $(i), l = /* @__PURE__ */ $(r);
      return e || (we(r, l) && k(o, "has", r), k(o, "has", l)), r === l ? i.has(r) : i.has(r) || i.has(l);
    },
    forEach(r, i) {
      const o = this, l = o.__v_raw, f = /* @__PURE__ */ $(l), d = t ? js : e ? Et : De;
      return !e && k(f, "iterate", Je), l.forEach((a, p) => r.call(i, d(a), d(p), o));
    }
  };
  return Q(
    s,
    e ? {
      add: Lt("add"),
      set: Lt("set"),
      delete: Lt("delete"),
      clear: Lt("clear")
    } : {
      add(r) {
        const i = /* @__PURE__ */ $(this), o = Kt(i), l = /* @__PURE__ */ $(r), f = !t && !/* @__PURE__ */ ae(r) && !/* @__PURE__ */ Le(r) ? l : r;
        return o.has.call(i, f) || we(r, f) && o.has.call(i, r) || we(l, f) && o.has.call(i, l) || (i.add(f), Me(i, "add", f, f)), this;
      },
      set(r, i) {
        !t && !/* @__PURE__ */ ae(i) && !/* @__PURE__ */ Le(i) && (i = /* @__PURE__ */ $(i));
        const o = /* @__PURE__ */ $(this), { has: l, get: f } = Kt(o);
        let d = l.call(o, r);
        d || (r = /* @__PURE__ */ $(r), d = l.call(o, r));
        const a = f.call(o, r);
        return o.set(r, i), d ? we(i, a) && Me(o, "set", r, i) : Me(o, "add", r, i), this;
      },
      delete(r) {
        const i = /* @__PURE__ */ $(this), { has: o, get: l } = Kt(i);
        let f = o.call(i, r);
        f || (r = /* @__PURE__ */ $(r), f = o.call(i, r)), l && l.call(i, r);
        const d = i.delete(r);
        return f && Me(i, "delete", r, void 0), d;
      },
      clear() {
        const r = /* @__PURE__ */ $(this), i = r.size !== 0, o = r.clear();
        return i && Me(
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
    s[r] = Ci(r, e, t);
  }), s;
}
function en(e, t) {
  const s = Ti(e, t);
  return (n, r, i) => r === "__v_isReactive" ? !e : r === "__v_isReadonly" ? e : r === "__v_raw" ? n : Reflect.get(
    K(s, r) && r in n ? s : n,
    r,
    i
  );
}
const Ei = {
  get: /* @__PURE__ */ en(!1, !1)
}, Oi = {
  get: /* @__PURE__ */ en(!1, !0)
}, Ai = {
  get: /* @__PURE__ */ en(!0, !1)
};
const hr = /* @__PURE__ */ new WeakMap(), pr = /* @__PURE__ */ new WeakMap(), gr = /* @__PURE__ */ new WeakMap(), Pi = /* @__PURE__ */ new WeakMap();
function Mi(e) {
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
function tn(e) {
  return /* @__PURE__ */ Le(e) ? e : sn(
    e,
    !1,
    vi,
    Ei,
    hr
  );
}
// @__NO_SIDE_EFFECTS__
function Ii(e) {
  return sn(
    e,
    !1,
    wi,
    Oi,
    pr
  );
}
// @__NO_SIDE_EFFECTS__
function Hs(e) {
  return sn(
    e,
    !0,
    Si,
    Ai,
    gr
  );
}
function sn(e, t, s, n, r) {
  if (!B(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const i = r.get(e);
  if (i)
    return i;
  const o = Mi(si(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? n : s
  );
  return r.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function nt(e) {
  return /* @__PURE__ */ Le(e) ? /* @__PURE__ */ nt(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function Le(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function ae(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function nn(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function $(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ $(t) : e;
}
function Ri(e) {
  return !K(e, "__v_skip") && Object.isExtensible(e) && Qn(e, "__v_skip", !0), e;
}
const De = (e) => B(e) ? /* @__PURE__ */ tn(e) : e, Et = (e) => B(e) ? /* @__PURE__ */ Hs(e) : e;
// @__NO_SIDE_EFFECTS__
function ee(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function dt(e) {
  return Fi(e, !1);
}
function Fi(e, t) {
  return /* @__PURE__ */ ee(e) ? e : new Di(e, t);
}
class Di {
  constructor(t, s) {
    this.dep = new Qs(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ $(t), this._value = s ? t : De(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ ae(t) || /* @__PURE__ */ Le(t);
    t = n ? t : /* @__PURE__ */ $(t), we(t, s) && (this._rawValue = t, this._value = n ? t : De(t), this.dep.trigger());
  }
}
function gt(e) {
  return /* @__PURE__ */ ee(e) ? e.value : e;
}
const ji = {
  get: (e, t, s) => t === "__v_raw" ? e : gt(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const r = e[t];
    return /* @__PURE__ */ ee(r) && !/* @__PURE__ */ ee(s) ? (r.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function mr(e) {
  return /* @__PURE__ */ nt(e) ? e : new Proxy(e, ji);
}
class Hi {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Qs(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Ct - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    q !== this)
      return rr(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return lr(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function Ni(e, t, s = !1) {
  let n, r;
  return F(e) ? n = e : (n = e.get, r = e.set), new Hi(n, r, s);
}
const Vt = {}, Zt = /* @__PURE__ */ new WeakMap();
let Ge;
function $i(e, t = !1, s = Ge) {
  if (s) {
    let n = Zt.get(s);
    n || Zt.set(s, n = []), n.push(e);
  }
}
function Ki(e, t, s = U) {
  const { immediate: n, deep: r, once: i, scheduler: o, augmentJob: l, call: f } = s, d = (P) => r ? P : /* @__PURE__ */ ae(P) || r === !1 || r === 0 ? Ie(P, 1) : Ie(P);
  let a, p, C, E, T = !1, w = !1;
  if (/* @__PURE__ */ ee(e) ? (p = () => e.value, T = /* @__PURE__ */ ae(e)) : /* @__PURE__ */ nt(e) ? (p = () => d(e), T = !0) : R(e) ? (w = !0, T = e.some((P) => /* @__PURE__ */ nt(P) || /* @__PURE__ */ ae(P)), p = () => e.map((P) => {
    if (/* @__PURE__ */ ee(P))
      return P.value;
    if (/* @__PURE__ */ nt(P))
      return d(P);
    if (F(P))
      return f ? f(P, 2) : P();
  })) : F(e) ? t ? p = f ? () => f(e, 2) : e : p = () => {
    if (C) {
      Re();
      try {
        C();
      } finally {
        Fe();
      }
    }
    const P = Ge;
    Ge = a;
    try {
      return f ? f(e, 3, [E]) : e(E);
    } finally {
      Ge = P;
    }
  } : p = Ce, t && r) {
    const P = p, z = r === !0 ? 1 / 0 : r;
    p = () => Ie(P(), z);
  }
  const D = hi(), M = () => {
    a.stop(), D && D.active && Ws(D.effects, a);
  };
  if (i && t) {
    const P = t;
    t = (...z) => {
      const he = P(...z);
      return M(), he;
    };
  }
  let j = w ? new Array(e.length).fill(Vt) : Vt;
  const H = (P) => {
    if (!(!(a.flags & 1) || !a.dirty && !P))
      if (t) {
        const z = a.run();
        if (P || r || T || (w ? z.some((he, pe) => we(he, j[pe])) : we(z, j))) {
          C && C();
          const he = Ge;
          Ge = a;
          try {
            const pe = [
              z,
              // pass undefined as the old value when it's changed for the first time
              j === Vt ? void 0 : w && j[0] === Vt ? [] : j,
              E
            ];
            j = z, f ? f(t, 3, pe) : (
              // @ts-expect-error
              t(...pe)
            );
          } finally {
            Ge = he;
          }
        }
      } else
        a.run();
  };
  return l && l(H), a = new sr(p), a.scheduler = o ? () => o(H, !1) : H, E = (P) => $i(P, !1, a), C = a.onStop = () => {
    const P = Zt.get(a);
    if (P) {
      if (f)
        f(P, 4);
      else
        for (const z of P) z();
      Zt.delete(a);
    }
  }, t ? n ? H(!0) : j = a.run() : o ? o(H.bind(null, !0), !0) : a.run(), M.pause = a.pause.bind(a), M.resume = a.resume.bind(a), M.stop = M, M;
}
function Ie(e, t = 1 / 0, s) {
  if (t <= 0 || !B(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ ee(e))
    Ie(e.value, t, s);
  else if (R(e))
    for (let n = 0; n < e.length; n++)
      Ie(e[n], t, s);
  else if (Xt(e) || Ke(e))
    e.forEach((n) => {
      Ie(n, t, s);
    });
  else if (Xn(e)) {
    for (const n in e)
      Ie(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && Ie(e[n], t, s);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function Rt(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (r) {
    fs(r, t, s);
  }
}
function de(e, t, s, n) {
  if (F(e)) {
    const r = Rt(e, t, s, n);
    return r && Yn(r) && r.catch((i) => {
      fs(i, t, s);
    }), r;
  }
  if (R(e)) {
    const r = [];
    for (let i = 0; i < e.length; i++)
      r.push(de(e[i], t, s, n));
    return r;
  }
}
function fs(e, t, s, n = !0) {
  const r = t ? t.vnode : null, { errorHandler: i, throwUnhandledErrorInProduction: o } = t && t.appContext.config || U;
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
      Re(), Rt(i, null, 10, [
        e,
        f,
        d
      ]), Fe();
      return;
    }
  }
  Li(e, s, r, n, o);
}
function Li(e, t, s, n = !0, r = !1) {
  if (r)
    throw e;
  console.error(e);
}
const ne = [];
let xe = -1;
const rt = [];
let Ne = null, et = 0;
const _r = /* @__PURE__ */ Promise.resolve();
let Qt = null;
function Vi(e) {
  const t = Qt || _r;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Ui(e) {
  let t = xe + 1, s = ne.length;
  for (; t < s; ) {
    const n = t + s >>> 1, r = ne[n], i = Ot(r);
    i < e || i === e && r.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function rn(e) {
  if (!(e.flags & 1)) {
    const t = Ot(e), s = ne[ne.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Ot(s) ? ne.push(e) : ne.splice(Ui(t), 0, e), e.flags |= 1, br();
  }
}
function br() {
  Qt || (Qt = _r.then(xr));
}
function Bi(e) {
  if (!R(e))
    Ne && e.id === -1 ? Ne.splice(et + 1, 0, e) : e.flags & 1 || (rt.push(e), e.flags |= 1);
  else
    for (let t = 0; t < e.length; t++)
      rt.push(e[t]);
  br();
}
function vn(e, t, s = xe + 1) {
  for (; s < ne.length; s++) {
    const n = ne[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      ne.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function yr(e) {
  if (rt.length) {
    const t = [...new Set(rt)].sort(
      (s, n) => Ot(s) - Ot(n)
    );
    if (rt.length = 0, Ne) {
      for (let s = 0; s < t.length; s++)
        Ne.push(t[s]);
      return;
    }
    for (Ne = t, et = 0; et < Ne.length; et++) {
      const s = Ne[et];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    Ne = null, et = 0;
  }
}
const Ot = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function xr(e) {
  try {
    for (xe = 0; xe < ne.length; xe++) {
      const t = ne[xe];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Rt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; xe < ne.length; xe++) {
      const t = ne[xe];
      t && (t.flags &= -2);
    }
    xe = -1, ne.length = 0, yr(), Qt = null, (ne.length || rt.length) && xr();
  }
}
let ce = null, vr = null;
function kt(e) {
  const t = ce;
  return ce = e, vr = e && e.type.__scopeId || null, t;
}
function Wi(e, t = ce, s) {
  if (!t || e._n)
    return e;
  const n = (...r) => {
    n._d && Rn(-1);
    const i = kt(t), o = Ye.length;
    let l;
    try {
      l = e(...r);
    } finally {
      for (let f = Ye.length; f > o; f--) Jr();
      kt(i), n._d && Rn(1);
    }
    return l;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function qi(e, t) {
  if (ce === null)
    return e;
  const s = ps(ce), n = e.dirs || (e.dirs = []);
  for (let r = 0; r < t.length; r++) {
    let [i, o, l, f = U] = t[r];
    i && (F(i) && (i = {
      mounted: i,
      updated: i
    }), i.deep && Ie(o), n.push({
      dir: i,
      instance: s,
      value: o,
      oldValue: void 0,
      arg: l,
      modifiers: f
    }));
  }
  return e;
}
function We(e, t, s, n) {
  const r = e.dirs, i = t && t.dirs;
  for (let o = 0; o < r.length; o++) {
    const l = r[o];
    i && (l.oldValue = i[o].value);
    let f = l.dir[n];
    f && (Re(), de(f, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Fe());
  }
}
function Gi(e, t) {
  if (re) {
    let s = re.provides;
    const n = re.parent && re.parent.provides;
    n === s && (s = re.provides = Object.create(n)), s[e] = t;
  }
}
function Yt(e, t, s = !1) {
  const n = qo();
  if (n || it) {
    let r = it ? it._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (r && e in r)
      return r[e];
    if (arguments.length > 1)
      return s && F(t) ? t.call(n && n.proxy) : t;
  }
}
const Ji = /* @__PURE__ */ Symbol.for("v-scx"), Yi = () => Yt(Ji);
function Ss(e, t, s) {
  return Sr(e, t, s);
}
function Sr(e, t, s = U) {
  const { immediate: n, deep: r, flush: i, once: o } = s, l = Q({}, s), f = t && n || !t && i !== "post";
  let d;
  if (Mt) {
    if (i === "sync") {
      const E = Yi();
      d = E.__watcherHandles || (E.__watcherHandles = []);
    } else if (!f) {
      const E = () => {
      };
      return E.stop = Ce, E.resume = Ce, E.pause = Ce, E;
    }
  }
  const a = re;
  l.call = (E, T, w) => de(E, a, T, w);
  let p = !1;
  i === "post" ? l.scheduler = (E) => {
    ie(E, a && a.suspense);
  } : i !== "sync" && (p = !0, l.scheduler = (E, T) => {
    T ? E() : rn(E);
  }), l.augmentJob = (E) => {
    t && (E.flags |= 4), p && (E.flags |= 2, a && (E.id = a.uid, E.i = a));
  };
  const C = Ki(e, t, l);
  return Mt && (d ? d.push(C) : f && C()), C;
}
function zi(e, t, s) {
  const n = this.proxy, r = J(e) ? e.includes(".") ? wr(n, e) : () => n[e] : e.bind(n, n);
  let i;
  F(t) ? i = t : (i = t.handler, s = t);
  const o = Ft(this), l = Sr(r, i.bind(n), s);
  return o(), l;
}
function wr(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let r = 0; r < s.length && n; r++)
      n = n[s[r]];
    return n;
  };
}
const Xi = /* @__PURE__ */ Symbol("_vte"), us = (e) => e.__isTeleport, ws = /* @__PURE__ */ Symbol("_leaveCb");
function Zi(e) {
  let t = e[0];
  if (e.length > 1) {
    for (const s of e)
      if (s.type !== Xe) {
        t = s;
        break;
      }
  }
  return t;
}
function Cr(e) {
  if (!ln(e))
    return us(e.type) && e.children ? Zi(e.children) : e;
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
function on(e, t) {
  if (e.shapeFlag & 6 && e.component) {
    e.transition = t;
    const s = e.component.subTree;
    on(
      us(s.type) && Cr(s) || s,
      t
    );
  } else e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Qi(e, t) {
  return F(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    Q({ name: e.name }, t, { setup: e })
  ) : e;
}
function Tr(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function Sn(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const es = /* @__PURE__ */ new WeakMap();
function vt(e, t, s, n, r = !1) {
  if (R(e)) {
    e.forEach(
      (w, D) => vt(
        w,
        t && (R(t) ? t[D] : t),
        s,
        n,
        r
      )
    );
    return;
  }
  if (St(n) && !r) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && vt(e, t, s, n.component.subTree);
    return;
  }
  const i = n.shapeFlag & 4 ? ps(n.component) : n.el, o = r ? null : i, { i: l, r: f } = e, d = t && t.r, a = l.refs === U ? l.refs = {} : l.refs, p = l.setupState, C = /* @__PURE__ */ $(p), E = p === U ? Jn : (w) => Sn(a, w) ? !1 : K(C, w), T = (w, D) => !(D && Sn(a, D));
  if (d != null && d !== f) {
    if (wn(t), J(d))
      a[d] = null, E(d) && (p[d] = null);
    else if (/* @__PURE__ */ ee(d)) {
      const w = t;
      T(d, w.k) && (d.value = null), w.k && (a[w.k] = null);
    }
  }
  if (F(f))
    Rt(f, l, 12, [o, a]);
  else {
    const w = J(f), D = /* @__PURE__ */ ee(f);
    if (w || D) {
      const M = () => {
        if (e.f) {
          const j = w ? E(f) ? p[f] : a[f] : T() || !e.k ? f.value : a[e.k];
          if (r)
            R(j) && Ws(j, i);
          else if (R(j))
            j.includes(i) || j.push(i);
          else if (w)
            a[f] = [i], E(f) && (p[f] = a[f]);
          else {
            const H = [i];
            T(f, e.k) && (f.value = H), e.k && (a[e.k] = H);
          }
        } else w ? (a[f] = o, E(f) && (p[f] = o)) : D && (T(f, e.k) && (f.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const j = () => {
          M(), es.delete(e);
        };
        j.id = -1, es.set(e, j), ie(j, s);
      } else
        wn(e), M();
    }
  }
}
function wn(e) {
  const t = es.get(e);
  t && (t.flags |= 8, es.delete(e));
}
ls().requestIdleCallback;
ls().cancelIdleCallback;
const St = (e) => !!e.type.__asyncLoader, ln = (e) => e.type.__isKeepAlive;
function ki(e, t) {
  Er(e, "a", t);
}
function eo(e, t) {
  Er(e, "da", t);
}
function Er(e, t, s = re) {
  const n = e.__wdc || (e.__wdc = () => {
    let r = s;
    for (; r; ) {
      if (r.isDeactivated)
        return;
      r = r.parent;
    }
    return e();
  });
  if (as(t, n, s), s) {
    let r = s.parent;
    for (; r && r.parent; )
      ln(r.parent.vnode) && to(n, t, s, r), r = r.parent;
  }
}
function to(e, t, s, n) {
  const r = as(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Pr(() => {
    Ws(n[t], r);
  }, s);
}
function as(e, t, s = re, n = !1) {
  if (s) {
    const r = s[e] || (s[e] = []), i = t.__weh || (t.__weh = (...o) => {
      Re();
      const l = Ft(s), f = de(t, s, e, o);
      return l(), Fe(), f;
    });
    return n ? r.unshift(i) : r.push(i), i;
  }
}
const je = (e) => (t, s = re) => {
  (!Mt || e === "sp") && as(e, (...n) => t(...n), s);
}, so = je("bm"), Or = je("m"), no = je(
  "bu"
), ro = je("u"), Ar = je(
  "bum"
), Pr = je("um"), io = je(
  "sp"
), oo = je("rtg"), lo = je("rtc");
function co(e, t = re) {
  as("ec", e, t);
}
const fo = /* @__PURE__ */ Symbol.for("v-ndc"), Ns = (e) => e ? Xr(e) ? ps(e) : Ns(e.parent) : null, wt = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ Q(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => e.props,
    $attrs: (e) => e.attrs,
    $slots: (e) => e.slots,
    $refs: (e) => e.refs,
    $parent: (e) => Ns(e.parent),
    $root: (e) => Ns(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Ir(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      rn(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = Vi.bind(e.proxy)),
    $watch: (e) => zi.bind(e)
  })
), Cs = (e, t) => e !== U && !e.__isScriptSetup && K(e, t), uo = {
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
        if (Cs(n, t))
          return o[t] = 1, n[t];
        if (r !== U && K(r, t))
          return o[t] = 2, r[t];
        if (K(i, t))
          return o[t] = 3, i[t];
        if (s !== U && K(s, t))
          return o[t] = 4, s[t];
        $s && (o[t] = 0);
      }
    }
    const d = wt[t];
    let a, p;
    if (d)
      return t === "$attrs" && k(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== U && K(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      p = f.config.globalProperties, K(p, t)
    )
      return p[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: r, ctx: i } = e;
    return Cs(r, t) ? (r[t] = s, !0) : n !== U && K(n, t) ? (n[t] = s, !0) : K(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (i[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: r, props: i, type: o }
  }, l) {
    let f;
    return !!(s[l] || e !== U && l[0] !== "$" && K(e, l) || Cs(t, l) || K(i, l) || K(n, l) || K(wt, l) || K(r.config.globalProperties, l) || (f = o.__cssModules) && f[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : K(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function Cn(e) {
  return R(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let $s = !0;
function ao(e) {
  const t = Ir(e), s = e.proxy, n = e.ctx;
  $s = !1, t.beforeCreate && Tn(t.beforeCreate, e, "bc");
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
    updated: T,
    activated: w,
    deactivated: D,
    beforeDestroy: M,
    beforeUnmount: j,
    destroyed: H,
    unmounted: P,
    render: z,
    renderTracked: he,
    renderTriggered: pe,
    errorCaptured: He,
    serverPrefetch: Dt,
    // public API
    expose: Ve,
    inheritAttrs: lt,
    // assets
    components: jt,
    directives: Ht,
    filters: gs
  } = t;
  if (d && ho(d, n, null), o)
    for (const G in o) {
      const W = o[G];
      F(W) && (n[G] = W.bind(s));
    }
  if (r) {
    const G = r.call(s, s);
    B(G) && (e.data = /* @__PURE__ */ tn(G));
  }
  if ($s = !0, i)
    for (const G in i) {
      const W = i[G], Ue = F(W) ? W.bind(s, s) : F(W.get) ? W.get.bind(s, s) : Ce, Nt = !F(W) && F(W.set) ? W.set.bind(s) : Ce, Be = Zo({
        get: Ue,
        set: Nt
      });
      Object.defineProperty(n, G, {
        enumerable: !0,
        configurable: !0,
        get: () => Be.value,
        set: (ge) => Be.value = ge
      });
    }
  if (l)
    for (const G in l)
      Mr(l[G], n, s, G);
  if (f) {
    const G = F(f) ? f.call(s) : f;
    Reflect.ownKeys(G).forEach((W) => {
      Gi(W, G[W]);
    });
  }
  a && Tn(a, e, "c");
  function te(G, W) {
    R(W) ? W.forEach((Ue) => G(Ue.bind(s))) : W && G(W.bind(s));
  }
  if (te(so, p), te(Or, C), te(no, E), te(ro, T), te(ki, w), te(eo, D), te(co, He), te(lo, he), te(oo, pe), te(Ar, j), te(Pr, P), te(io, Dt), R(Ve))
    if (Ve.length) {
      const G = e.exposed || (e.exposed = {});
      Ve.forEach((W) => {
        Object.defineProperty(G, W, {
          get: () => s[W],
          set: (Ue) => s[W] = Ue,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  z && e.render === Ce && (e.render = z), lt != null && (e.inheritAttrs = lt), jt && (e.components = jt), Ht && (e.directives = Ht), Dt && Tr(e);
}
function ho(e, t, s = Ce) {
  R(e) && (e = Ks(e));
  for (const n in e) {
    const r = e[n];
    let i;
    B(r) ? "default" in r ? i = Yt(
      r.from || n,
      r.default,
      !0
    ) : i = Yt(r.from || n) : i = Yt(r), /* @__PURE__ */ ee(i) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => i.value,
      set: (o) => i.value = o
    }) : t[n] = i;
  }
}
function Tn(e, t, s) {
  de(
    R(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Mr(e, t, s, n) {
  let r = n.includes(".") ? wr(s, n) : () => s[n];
  if (J(e)) {
    const i = t[e];
    F(i) && Ss(r, i);
  } else if (F(e))
    Ss(r, e.bind(s));
  else if (B(e))
    if (R(e))
      e.forEach((i) => Mr(i, t, s, n));
    else {
      const i = F(e.handler) ? e.handler.bind(s) : t[e.handler];
      F(i) && Ss(r, i, e);
    }
}
function Ir(e) {
  const t = e.type, { mixins: s, extends: n } = t, {
    mixins: r,
    optionsCache: i,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = i.get(t);
  let f;
  return l ? f = l : !r.length && !s && !n ? f = t : (f = {}, r.length && r.forEach(
    (d) => ts(f, d, o, !0)
  ), ts(f, t, o)), B(t) && i.set(t, f), f;
}
function ts(e, t, s, n = !1) {
  const { mixins: r, extends: i } = t;
  i && ts(e, i, s, !0), r && r.forEach(
    (o) => ts(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = po[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const po = {
  data: En,
  props: On,
  emits: On,
  // objects
  methods: mt,
  computed: mt,
  // lifecycle
  beforeCreate: se,
  created: se,
  beforeMount: se,
  mounted: se,
  beforeUpdate: se,
  updated: se,
  beforeDestroy: se,
  beforeUnmount: se,
  destroyed: se,
  unmounted: se,
  activated: se,
  deactivated: se,
  errorCaptured: se,
  serverPrefetch: se,
  // assets
  components: mt,
  directives: mt,
  // watch
  watch: mo,
  // provide / inject
  provide: En,
  inject: go
};
function En(e, t) {
  return t ? e ? function() {
    return Q(
      F(e) ? e.call(this, this) : e,
      F(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function go(e, t) {
  return mt(Ks(e), Ks(t));
}
function Ks(e) {
  if (R(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++)
      t[e[s]] = e[s];
    return t;
  }
  return e;
}
function se(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function mt(e, t) {
  return e ? Q(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function On(e, t) {
  return e ? R(e) && R(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : Q(
    /* @__PURE__ */ Object.create(null),
    Cn(e),
    Cn(t ?? {})
  ) : t;
}
function mo(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = Q(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = se(e[n], t[n]);
  return s;
}
function Rr() {
  return {
    app: null,
    config: {
      isNativeTag: Jn,
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
let _o = 0;
function bo(e, t) {
  return function(n, r = null) {
    F(n) || (n = Q({}, n)), r != null && !B(r) && (r = null);
    const i = Rr(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let f = !1;
    const d = i.app = {
      _uid: _o++,
      _component: n,
      _props: r,
      _container: null,
      _context: i,
      _instance: null,
      version: Qo,
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
          const E = d._ceVNode || ze(n, r);
          return E.appContext = i, C === !0 ? C = "svg" : C === !1 && (C = void 0), e(E, a, C), f = !0, d._container = a, a.__vue_app__ = d, ps(E.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        f && (de(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, p) {
        return i.provides[a] = p, d;
      },
      runWithContext(a) {
        const p = it;
        it = d;
        try {
          return a();
        } finally {
          it = p;
        }
      }
    };
    return d;
  };
}
let it = null;
const yo = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${fe(t)}Modifiers`] || e[`${Ze(t)}Modifiers`];
function xo(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || U;
  let r = s;
  const i = t.startsWith("update:"), o = i && yo(n, t.slice(7));
  o && (o.trim && (r = s.map((a) => J(a) ? a.trim() : a)), o.number && (r = r.map(Gs)));
  let l, f = n[l = _s(t)] || // also try camelCase event handler (#2249)
  n[l = _s(fe(t))];
  !f && i && (f = n[l = _s(Ze(t))]), f && de(
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
    e.emitted[l] = !0, de(
      d,
      e,
      6,
      r
    );
  }
}
const vo = /* @__PURE__ */ new WeakMap();
function Fr(e, t, s = !1) {
  const n = s ? vo : t.emitsCache, r = n.get(e);
  if (r !== void 0)
    return r;
  const i = e.emits;
  let o = {}, l = !1;
  if (!F(e)) {
    const f = (d) => {
      const a = Fr(d, t, !0);
      a && (l = !0, Q(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  return !i && !l ? (B(e) && n.set(e, null), null) : (R(i) ? i.forEach((f) => o[f] = null) : Q(o, i), B(e) && n.set(e, o), o);
}
function ds(e, t) {
  return !e || !rs(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), K(e, t[0].toLowerCase() + t.slice(1)) || K(e, Ze(t)) || K(e, t));
}
function An(e) {
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
    ctx: T,
    inheritAttrs: w
  } = e, D = kt(e);
  let M, j;
  try {
    if (s.shapeFlag & 4) {
      const P = r || n, z = P;
      M = Se(
        d.call(
          z,
          P,
          a,
          p,
          E,
          C,
          T
        )
      ), j = l;
    } else {
      const P = t;
      M = Se(
        P.length > 1 ? P(
          p,
          { attrs: l, slots: o, emit: f }
        ) : P(
          p,
          null
        )
      ), j = t.props ? l : So(l);
    }
  } catch (P) {
    Ye.length = 0, fs(P, e, 1), M = ze(Xe);
  }
  let H = M;
  if (j && w !== !1) {
    const P = Object.keys(j), { shapeFlag: z } = H;
    P.length && z & 7 && (i && P.some(is) && (j = wo(
      j,
      i
    )), H = ot(H, j, !1, !0));
  }
  if (s.dirs && (H = ot(H, null, !1, !0), H.dirs = H.dirs ? H.dirs.concat(s.dirs) : s.dirs), s.transition) {
    const P = us(H.type) && Cr(H) || H;
    on(P, s.transition);
  }
  return M = H, kt(D), M;
}
const So = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || rs(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, wo = (e, t) => {
  const s = {};
  for (const n in e)
    (!is(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function Co(e, t, s) {
  const { props: n, children: r, component: i } = e, { props: o, children: l, patchFlag: f } = t, d = i.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (s && f >= 0) {
    if (f & 1024)
      return !0;
    if (f & 16)
      return n ? Pn(n, o, d) : !!o;
    if (f & 8) {
      const a = t.dynamicProps;
      for (let p = 0; p < a.length; p++) {
        const C = a[p];
        if (Dr(o, n, C) && !ds(d, C))
          return !0;
      }
    }
  } else
    return (r || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? Pn(n, o, d) : !0 : !!o;
  return !1;
}
function Pn(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let r = 0; r < n.length; r++) {
    const i = n[r];
    if (Dr(t, e, i) && !ds(s, i))
      return !0;
  }
  return !1;
}
function Dr(e, t, s) {
  const n = e[s], r = t[s];
  return s === "style" && B(n) && B(r) ? !cs(n, r) : n !== r;
}
function To({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const r = t.subTree;
    if (r.suspense && r.suspense.activeBranch === e && (r.suspense.vnode.el = r.el = n, e = r), r === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const jr = {}, Hr = () => Object.create(jr), Nr = (e) => Object.getPrototypeOf(e) === jr;
function Eo(e, t, s, n = !1) {
  const r = {}, i = Hr();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), $r(e, t, r, i);
  for (const o in e.propsOptions[0])
    o in r || (r[o] = void 0);
  s ? e.props = n ? r : /* @__PURE__ */ Ii(r) : e.type.props ? e.props = r : e.props = i, e.attrs = i;
}
function Oo(e, t, s, n) {
  const {
    props: r,
    attrs: i,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ $(r), [f] = e.propsOptions;
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
        if (ds(e.emitsOptions, C))
          continue;
        const E = t[C];
        if (f)
          if (K(i, C))
            E !== i[C] && (i[C] = E, d = !0);
          else {
            const T = fe(C);
            r[T] = Ls(
              f,
              l,
              T,
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
    $r(e, t, r, i) && (d = !0);
    let a;
    for (const p in l)
      (!t || // for camelCase
      !K(t, p) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = Ze(p)) === p || !K(t, a))) && (f ? s && // for camelCase
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
        (!t || !K(t, p)) && (delete i[p], d = !0);
  }
  d && Me(e.attrs, "set", "");
}
function $r(e, t, s, n) {
  const [r, i] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let f in t) {
      if (bt(f))
        continue;
      const d = t[f];
      let a;
      r && K(r, a = fe(f)) ? !i || !i.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : ds(e.emitsOptions, f) || (!(f in n) || d !== n[f]) && (n[f] = d, o = !0);
    }
  if (i) {
    const f = /* @__PURE__ */ $(s), d = l || U;
    for (let a = 0; a < i.length; a++) {
      const p = i[a];
      s[p] = Ls(
        r,
        f,
        p,
        d[p],
        e,
        !K(d, p)
      );
    }
  }
  return o;
}
function Ls(e, t, s, n, r, i) {
  const o = e[s];
  if (o != null) {
    const l = K(o, "default");
    if (l && n === void 0) {
      const f = o.default;
      if (o.type !== Function && !o.skipFactory && F(f)) {
        const { propsDefaults: d } = r;
        if (s in d)
          n = d[s];
        else {
          const a = Ft(r);
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
    ] && (n === "" || n === Ze(s)) && (n = !0));
  }
  return n;
}
const Ao = /* @__PURE__ */ new WeakMap();
function Kr(e, t, s = !1) {
  const n = s ? Ao : t.propsCache, r = n.get(e);
  if (r)
    return r;
  const i = e.props, o = {}, l = [];
  let f = !1;
  if (!F(e)) {
    const a = (p) => {
      f = !0;
      const [C, E] = Kr(p, t, !0);
      Q(o, C), E && l.push(...E);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!i && !f)
    return B(e) && n.set(e, st), st;
  if (R(i))
    for (let a = 0; a < i.length; a++) {
      const p = fe(i[a]);
      Mn(p) && (o[p] = U);
    }
  else if (i)
    for (const a in i) {
      const p = fe(a);
      if (Mn(p)) {
        const C = i[a], E = o[p] = R(C) || F(C) ? { type: C } : Q({}, C), T = E.type;
        let w = !1, D = !0;
        if (R(T))
          for (let M = 0; M < T.length; ++M) {
            const j = T[M], H = F(j) && j.name;
            if (H === "Boolean") {
              w = !0;
              break;
            } else H === "String" && (D = !1);
          }
        else
          w = F(T) && T.name === "Boolean";
        E[
          0
          /* shouldCast */
        ] = w, E[
          1
          /* shouldCastTrue */
        ] = D, (w || K(E, "default")) && l.push(p);
      }
    }
  const d = [o, l];
  return B(e) && n.set(e, d), d;
}
function Mn(e) {
  return e[0] !== "$" && !bt(e);
}
const cn = (e) => e === "_" || e === "_ctx" || e === "$stable", fn = (e) => R(e) ? e.map(Se) : [Se(e)], Po = (e, t, s) => {
  if (t._n)
    return t;
  const n = Wi((...r) => fn(t(...r)), s);
  return n._c = !1, n;
}, Lr = (e, t, s) => {
  const n = e._ctx;
  for (const r in e) {
    if (cn(r)) continue;
    const i = e[r];
    if (F(i))
      t[r] = Po(r, i, n);
    else if (i != null) {
      const o = fn(i);
      t[r] = () => o;
    }
  }
}, Vr = (e, t) => {
  const s = fn(t);
  e.slots.default = () => s;
}, Ur = (e, t, s) => {
  for (const n in t)
    (s || !cn(n)) && (e[n] = t[n]);
}, Mo = (e, t, s) => {
  const n = e.slots = Hr();
  if (e.vnode.shapeFlag & 32) {
    const r = t._;
    r ? (Ur(n, t, s), s && Qn(n, "_", r, !0)) : Lr(t, n);
  } else t && Vr(e, t);
}, Io = (e, t, s) => {
  const { vnode: n, slots: r } = e;
  let i = !0, o = U;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? i = !1 : Ur(r, t, s) : (i = !t.$stable, Lr(t, r)), o = t;
  } else t && (Vr(e, t), o = { default: 1 });
  if (i)
    for (const l in r)
      !cn(l) && o[l] == null && delete r[l];
}, ie = Ho;
function Ro(e) {
  return Fo(e);
}
function Fo(e, t) {
  const s = ls();
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
    setScopeId: E = Ce,
    insertStaticContent: T
  } = e, w = (c, u, h, b = null, _ = null, g = null, v = void 0, x = null, y = !!u.dynamicChildren) => {
    if (c === u)
      return;
    c && !ht(c, u) && (b = $t(c), ge(c, _, g, !0), c = null), u.patchFlag === -2 && (y = !1, u.dynamicChildren = null);
    const { type: m, ref: A, shapeFlag: S } = u;
    switch (m) {
      case hs:
        D(c, u, h, b);
        break;
      case Xe:
        M(c, u, h, b);
        break;
      case Es:
        c == null && j(u, h, b, v);
        break;
      case Ae:
        jt(
          c,
          u,
          h,
          b,
          _,
          g,
          v,
          x,
          y
        );
        break;
      default:
        S & 1 ? z(
          c,
          u,
          h,
          b,
          _,
          g,
          v,
          x,
          y
        ) : S & 6 ? Ht(
          c,
          u,
          h,
          b,
          _,
          g,
          v,
          x,
          y
        ) : (S & 64 || S & 128) && m.process(
          c,
          u,
          h,
          b,
          _,
          g,
          v,
          x,
          y,
          ft
        );
    }
    A != null && _ ? vt(A, c && c.ref, g, u || c, !u) : A == null && c && c.ref != null && vt(c.ref, null, g, c, !0);
  }, D = (c, u, h, b) => {
    if (c == null)
      n(
        u.el = l(u.children),
        h,
        b
      );
    else {
      const _ = u.el = c.el;
      u.children !== c.children && d(_, u.children);
    }
  }, M = (c, u, h, b) => {
    c == null ? n(
      u.el = f(u.children || ""),
      h,
      b
    ) : u.el = c.el;
  }, j = (c, u, h, b) => {
    [c.el, c.anchor] = T(
      c.children,
      u,
      h,
      b,
      c.el,
      c.anchor
    );
  }, H = ({ el: c, anchor: u }, h, b) => {
    let _;
    for (; c && c !== u; )
      _ = C(c), n(c, h, b), c = _;
    n(u, h, b);
  }, P = ({ el: c, anchor: u }) => {
    let h;
    for (; c && c !== u; )
      h = C(c), r(c), c = h;
    r(u);
  }, z = (c, u, h, b, _, g, v, x, y) => {
    if (u.type === "svg" ? v = "svg" : u.type === "math" && (v = "mathml"), c == null)
      he(
        u,
        h,
        b,
        _,
        g,
        v,
        x,
        y
      );
    else {
      const m = c.el && c.el._isVueCE ? c.el : null;
      try {
        m && m._beginPatch(), Dt(
          c,
          u,
          _,
          g,
          v,
          x,
          y
        );
      } finally {
        m && m._endPatch();
      }
    }
  }, he = (c, u, h, b, _, g, v, x) => {
    let y, m;
    const { props: A, shapeFlag: S, transition: O, dirs: I } = c;
    if (y = c.el = o(
      c.type,
      g,
      A && A.is,
      A
    ), S & 8 ? a(y, c.children) : S & 16 && He(
      c.children,
      y,
      null,
      b,
      _,
      Ts(c, g),
      v,
      x
    ), I && We(c, null, b, "created"), pe(y, c, c.scopeId, v, b), A) {
      for (const V in A)
        V !== "value" && !bt(V) && i(y, V, null, A[V], g, b);
      "value" in A && i(y, "value", null, A.value, g), (m = A.onVnodeBeforeMount) && ye(m, b, c);
    }
    I && We(c, null, b, "beforeMount");
    const N = Do(_, O);
    N && O.beforeEnter(y), n(y, u, h), ((m = A && A.onVnodeMounted) || N || I) && ie(() => {
      try {
        m && ye(m, b, c), N && O.enter(y), I && We(c, null, b, "mounted");
      } finally {
      }
    }, _);
  }, pe = (c, u, h, b, _) => {
    if (h && E(c, h), b)
      for (let g = 0; g < b.length; g++)
        E(c, b[g]);
    if (_) {
      let g = _.subTree;
      if (u === g || Gr(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const v = _.vnode;
        pe(
          c,
          v,
          v.scopeId,
          v.slotScopeIds,
          _.parent
        );
      }
    }
  }, He = (c, u, h, b, _, g, v, x, y = 0) => {
    for (let m = y; m < c.length; m++) {
      const A = c[m] = x ? Pe(c[m]) : Se(c[m]);
      w(
        null,
        A,
        u,
        h,
        b,
        _,
        g,
        v,
        x
      );
    }
  }, Dt = (c, u, h, b, _, g, v) => {
    const x = u.el = c.el;
    let { patchFlag: y, dynamicChildren: m, dirs: A } = u;
    y |= c.patchFlag & 16;
    const S = c.props || U, O = u.props || U;
    let I;
    if (h && qe(h, !1), (I = O.onVnodeBeforeUpdate) && ye(I, h, u, c), A && We(u, c, h, "beforeUpdate"), h && qe(h, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    m && (!c.dynamicChildren || c.dynamicChildren.length !== m.length) && (y = 0, v = !1, m = null), (S.innerHTML && O.innerHTML == null || S.textContent && O.textContent == null) && a(x, ""), m ? Ve(
      c.dynamicChildren,
      m,
      x,
      h,
      b,
      Ts(u, _),
      g
    ) : v || W(
      c,
      u,
      x,
      null,
      h,
      b,
      Ts(u, _),
      g,
      !1
    ), y > 0) {
      if (y & 16)
        lt(x, S, O, h, _);
      else if (y & 2 && S.class !== O.class && i(x, "class", null, O.class, _), y & 4 && i(x, "style", S.style, O.style, _), y & 8) {
        const N = u.dynamicProps;
        for (let V = 0; V < N.length; V++) {
          const L = N[V], Y = S[L], X = O[L];
          (X !== Y || L === "value") && i(x, L, Y, X, _, h);
        }
      }
      y & 1 && c.children !== u.children && a(x, u.children);
    } else !v && m == null && lt(x, S, O, h, _);
    ((I = O.onVnodeUpdated) || A) && ie(() => {
      I && ye(I, h, u, c), A && We(u, c, h, "updated");
    }, b);
  }, Ve = (c, u, h, b, _, g, v) => {
    for (let x = 0; x < u.length; x++) {
      const y = c[x], m = u[x], A = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        y.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (y.type === Ae || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !ht(y, m) || // - In the case of a component, it could contain anything.
        y.shapeFlag & 198) ? p(y.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          h
        )
      );
      w(
        y,
        m,
        A,
        null,
        b,
        _,
        g,
        v,
        !0
      );
    }
  }, lt = (c, u, h, b, _) => {
    if (u !== h) {
      if (u !== U)
        for (const g in u)
          !bt(g) && !(g in h) && i(
            c,
            g,
            u[g],
            null,
            _,
            b
          );
      for (const g in h) {
        if (bt(g)) continue;
        const v = h[g], x = u[g];
        v !== x && g !== "value" && i(c, g, x, v, _, b);
      }
      "value" in h && i(c, "value", u.value, h.value, _);
    }
  }, jt = (c, u, h, b, _, g, v, x, y) => {
    const m = u.el = c ? c.el : l(""), A = u.anchor = c ? c.anchor : l("");
    let { patchFlag: S, dynamicChildren: O, slotScopeIds: I } = u;
    I && (x = x ? x.concat(I) : I), c == null ? (n(m, h, b), n(A, h, b), He(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      h,
      A,
      _,
      g,
      v,
      x,
      y
    )) : S > 0 && S & 64 && O && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    c.dynamicChildren && c.dynamicChildren.length === O.length ? (Ve(
      c.dynamicChildren,
      O,
      h,
      _,
      g,
      v,
      x
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || _ && u === _.subTree) && Br(
      c,
      u,
      !0
      /* shallow */
    )) : W(
      c,
      u,
      h,
      A,
      _,
      g,
      v,
      x,
      y
    );
  }, Ht = (c, u, h, b, _, g, v, x, y) => {
    u.slotScopeIds = x, c == null ? u.shapeFlag & 512 ? _.ctx.activate(
      u,
      h,
      b,
      v,
      y
    ) : gs(
      u,
      h,
      b,
      _,
      g,
      v,
      y
    ) : un(c, u, y);
  }, gs = (c, u, h, b, _, g, v) => {
    const x = c.component = Wo(
      c,
      b,
      _
    );
    if (ln(c) && (x.ctx.renderer = ft), Go(x, !1, v), x.asyncDep) {
      if (_ && _.registerDep(x, te, v), !c.el) {
        const y = x.subTree = ze(Xe);
        M(null, y, u, h), c.placeholder = y.el;
      }
    } else
      te(
        x,
        c,
        u,
        h,
        _,
        g,
        v
      );
  }, un = (c, u, h) => {
    const b = u.component = c.component;
    if (Co(c, u, h))
      if (b.asyncDep && !b.asyncResolved) {
        G(b, u, h);
        return;
      } else
        b.next = u, b.update();
    else
      u.el = c.el, b.vnode = u;
  }, te = (c, u, h, b, _, g, v) => {
    const x = () => {
      if (c.isMounted) {
        let { next: S, bu: O, u: I, parent: N, vnode: V } = c;
        {
          const _e = Wr(c);
          if (_e) {
            S && (S.el = V.el, G(c, S, v)), _e.asyncDep.then(() => {
              ie(() => {
                c.isUnmounted || m();
              }, _);
            });
            return;
          }
        }
        let L = S, Y;
        qe(c, !1), S ? (S.el = V.el, G(c, S, v)) : S = V, O && Gt(O), (Y = S.props && S.props.onVnodeBeforeUpdate) && ye(Y, N, S, V), qe(c, !0);
        const X = An(c), me = c.subTree;
        c.subTree = X, w(
          me,
          X,
          // parent may have changed if it's in a teleport
          p(me.el),
          // anchor may have changed if it's in a fragment
          $t(me),
          c,
          _,
          g
        ), S.el = X.el, L === null && To(c, X.el), I && ie(I, _), (Y = S.props && S.props.onVnodeUpdated) && ie(
          () => ye(Y, N, S, V),
          _
        );
      } else {
        let S;
        const { el: O, props: I } = u, { bm: N, m: V, parent: L, root: Y, type: X } = c, me = St(u);
        qe(c, !1), N && Gt(N), !me && (S = I && I.onVnodeBeforeMount) && ye(S, L, u), qe(c, !0);
        {
          Y.ce && Y.ce._hasShadowRoot() && Y.ce._injectChildStyle(
            X,
            c.parent ? c.parent.type : void 0
          );
          const _e = c.subTree = An(c);
          w(
            null,
            _e,
            h,
            b,
            c,
            _,
            g
          ), u.el = _e.el;
        }
        if (V && ie(V, _), !me && (S = I && I.onVnodeMounted)) {
          const _e = u;
          ie(
            () => ye(S, L, _e),
            _
          );
        }
        (u.shapeFlag & 256 || L && St(L.vnode) && L.vnode.shapeFlag & 256) && c.a && ie(c.a, _), c.isMounted = !0, u = h = b = null;
      }
    };
    c.scope.on();
    const y = c.effect = new sr(x);
    c.scope.off();
    const m = c.update = y.run.bind(y), A = c.job = y.runIfDirty.bind(y);
    A.i = c, A.id = c.uid, y.scheduler = () => rn(A), qe(c, !0), m();
  }, G = (c, u, h) => {
    u.component = c;
    const b = c.vnode.props;
    c.vnode = u, c.next = null, Oo(c, u.props, b, h), Io(c, u.children, h), Re(), vn(c), Fe();
  }, W = (c, u, h, b, _, g, v, x, y = !1) => {
    const m = c && c.children, A = c ? c.shapeFlag : 0, S = u.children, { patchFlag: O, shapeFlag: I } = u;
    if (O > 0) {
      if (O & 128) {
        Nt(
          m,
          S,
          h,
          b,
          _,
          g,
          v,
          x,
          y
        );
        return;
      } else if (O & 256) {
        Ue(
          m,
          S,
          h,
          b,
          _,
          g,
          v,
          x,
          y
        );
        return;
      }
    }
    I & 8 ? (A & 16 && ct(m, _, g), S !== m && a(h, S)) : A & 16 ? I & 16 ? Nt(
      m,
      S,
      h,
      b,
      _,
      g,
      v,
      x,
      y
    ) : ct(m, _, g, !0) : (A & 8 && a(h, ""), I & 16 && He(
      S,
      h,
      b,
      _,
      g,
      v,
      x,
      y
    ));
  }, Ue = (c, u, h, b, _, g, v, x, y) => {
    c = c || st, u = u || st;
    const m = c.length, A = u.length, S = Math.min(m, A);
    let O;
    for (O = 0; O < S; O++) {
      const I = u[O] = y ? Pe(u[O]) : Se(u[O]);
      w(
        c[O],
        I,
        h,
        null,
        _,
        g,
        v,
        x,
        y
      );
    }
    m > A ? ct(
      c,
      _,
      g,
      !0,
      !1,
      S
    ) : He(
      u,
      h,
      b,
      _,
      g,
      v,
      x,
      y,
      S
    );
  }, Nt = (c, u, h, b, _, g, v, x, y) => {
    let m = 0;
    const A = u.length;
    let S = c.length - 1, O = A - 1;
    for (; m <= S && m <= O; ) {
      const I = c[m], N = u[m] = y ? Pe(u[m]) : Se(u[m]);
      if (ht(I, N))
        w(
          I,
          N,
          h,
          null,
          _,
          g,
          v,
          x,
          y
        );
      else
        break;
      m++;
    }
    for (; m <= S && m <= O; ) {
      const I = c[S], N = u[O] = y ? Pe(u[O]) : Se(u[O]);
      if (ht(I, N))
        w(
          I,
          N,
          h,
          null,
          _,
          g,
          v,
          x,
          y
        );
      else
        break;
      S--, O--;
    }
    if (m > S) {
      if (m <= O) {
        const I = O + 1, N = I < A ? u[I].el : b;
        for (; m <= O; )
          w(
            null,
            u[m] = y ? Pe(u[m]) : Se(u[m]),
            h,
            N,
            _,
            g,
            v,
            x,
            y
          ), m++;
      }
    } else if (m > O)
      for (; m <= S; )
        ge(c[m], _, g, !0), m++;
    else {
      const I = m, N = m, V = /* @__PURE__ */ new Map();
      for (m = N; m <= O; m++) {
        const oe = u[m] = y ? Pe(u[m]) : Se(u[m]);
        oe.key != null && V.set(oe.key, m);
      }
      let L, Y = 0;
      const X = O - N + 1;
      let me = !1, _e = 0;
      const ut = new Array(X);
      for (m = 0; m < X; m++) ut[m] = 0;
      for (m = I; m <= S; m++) {
        const oe = c[m];
        if (Y >= X) {
          ge(oe, _, g, !0);
          continue;
        }
        let be;
        if (oe.key != null)
          be = V.get(oe.key);
        else
          for (L = N; L <= O; L++)
            if (ut[L - N] === 0 && ht(oe, u[L])) {
              be = L;
              break;
            }
        be === void 0 ? ge(oe, _, g, !0) : (ut[be - N] = m + 1, be >= _e ? _e = be : me = !0, w(
          oe,
          u[be],
          h,
          null,
          _,
          g,
          v,
          x,
          y
        ), Y++);
      }
      const hn = me ? jo(ut) : st;
      for (L = hn.length - 1, m = X - 1; m >= 0; m--) {
        const oe = N + m, be = u[oe], pn = u[oe + 1], gn = oe + 1 < A ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          pn.el || qr(pn)
        ) : b;
        ut[m] === 0 ? w(
          null,
          be,
          h,
          gn,
          _,
          g,
          v,
          x,
          y
        ) : me && (L < 0 || m !== hn[L] ? Be(be, h, gn, 2) : L--);
      }
    }
  }, Be = (c, u, h, b, _ = null) => {
    const { el: g, type: v, transition: x, children: y, shapeFlag: m } = c;
    if (m & 6) {
      Be(c.component.subTree, u, h, b);
      return;
    }
    if (m & 128) {
      c.suspense.move(u, h, b);
      return;
    }
    if (m & 64) {
      v.move(c, u, h, ft);
      return;
    }
    if (v === Ae) {
      n(g, u, h);
      for (let S = 0; S < y.length; S++)
        Be(y[S], u, h, b);
      n(c.anchor, u, h);
      return;
    }
    if (v === Es) {
      H(c, u, h);
      return;
    }
    if (b !== 2 && m & 1 && x)
      if (b === 0)
        x.persisted && !g[ws] ? n(g, u, h) : (x.beforeEnter(g), n(g, u, h), ie(() => x.enter(g), _));
      else {
        const { leave: S, delayLeave: O, afterLeave: I } = x, N = () => {
          c.ctx.isUnmounted ? r(g) : n(g, u, h);
        }, V = () => {
          const L = g._isLeaving || !!g[ws];
          g._isLeaving && g[ws](
            !0
            /* cancelled */
          ), x.persisted && !L ? N() : S(g, () => {
            N(), I && I();
          });
        };
        O ? O(g, N, V) : V();
      }
    else
      n(g, u, h);
  }, ge = (c, u, h, b = !1, _ = !1) => {
    const {
      type: g,
      props: v,
      ref: x,
      children: y,
      dynamicChildren: m,
      shapeFlag: A,
      patchFlag: S,
      dirs: O,
      cacheIndex: I,
      memo: N
    } = c;
    if (S === -2 && (_ = !1), x != null && (Re(), vt(x, null, h, c, !0), Fe()), I != null && (u.renderCache[I] = void 0), A & 256) {
      u.ctx.deactivate(c);
      return;
    }
    const V = A & 1 && O, L = !St(c);
    let Y;
    if (L && (Y = v && v.onVnodeBeforeUnmount) && ye(Y, u, c), A & 6)
      ei(c.component, h, b);
    else {
      if (A & 128) {
        c.suspense.unmount(h, b);
        return;
      }
      V && We(c, null, u, "beforeUnmount"), A & 64 ? c.type.remove(
        c,
        u,
        h,
        ft,
        b
      ) : m && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !m.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== Ae || S > 0 && S & 64) ? ct(
        m,
        u,
        h,
        !1,
        !0
      ) : (g === Ae && S & 384 || !_ && A & 16) && ct(y, u, h), b && an(c);
    }
    const X = N != null && I == null;
    (L && (Y = v && v.onVnodeUnmounted) || V || X) && ie(() => {
      Y && ye(Y, u, c), V && We(c, null, u, "unmounted"), X && (c.el = null);
    }, h);
  }, an = (c) => {
    const { type: u, el: h, anchor: b, transition: _ } = c;
    if (u === Ae) {
      kr(h, b);
      return;
    }
    if (u === Es) {
      P(c);
      return;
    }
    const g = () => {
      r(h), _ && !_.persisted && _.afterLeave && _.afterLeave();
    };
    if (c.shapeFlag & 1 && _ && !_.persisted) {
      const { leave: v, delayLeave: x } = _, y = () => v(h, g);
      x ? x(c.el, g, y) : y();
    } else
      g();
  }, kr = (c, u) => {
    let h;
    for (; c !== u; )
      h = C(c), r(c), c = h;
    r(u);
  }, ei = (c, u, h) => {
    const { bum: b, scope: _, job: g, subTree: v, um: x, m: y, a: m } = c;
    In(y), In(m), b && Gt(b), _.stop(), g && (g.flags |= 8, ge(v, c, u, h)), x && ie(x, u), ie(() => {
      c.isUnmounted = !0;
    }, u);
  }, ct = (c, u, h, b = !1, _ = !1, g = 0) => {
    for (let v = g; v < c.length; v++)
      ge(c[v], u, h, b, _);
  }, $t = (c) => {
    if (c.shapeFlag & 6)
      return $t(c.component.subTree);
    if (c.shapeFlag & 128)
      return c.suspense.next();
    const u = C(c.anchor || c.el), h = u && u[Xi];
    return h ? C(h) : u;
  };
  let ms = !1;
  const dn = (c, u, h) => {
    let b;
    c == null ? u._vnode && (ge(u._vnode, null, null, !0), b = u._vnode.component) : w(
      u._vnode || null,
      c,
      u,
      null,
      null,
      null,
      h
    ), u._vnode = c, ms || (ms = !0, vn(b), yr(), ms = !1);
  }, ft = {
    p: w,
    um: ge,
    m: Be,
    r: an,
    mt: gs,
    mc: He,
    pc: W,
    pbc: Ve,
    n: $t,
    o: e
  };
  return {
    render: dn,
    hydrate: void 0,
    createApp: bo(dn)
  };
}
function Ts({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function qe({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Do(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Br(e, t, s = !1) {
  const n = e.children, r = t.children;
  if (R(n) && R(r))
    for (let i = 0; i < n.length; i++) {
      const o = n[i];
      let l = r[i];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = r[i] = Pe(r[i]), l.el = o.el), !s && l.patchFlag !== -2 && Br(o, l)), l.type === hs && (l.patchFlag === -1 && (l = r[i] = Pe(l)), l.el = o.el), l.type === Xe && !l.el && (l.el = o.el);
    }
}
function jo(e) {
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
function Wr(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : Wr(t);
}
function In(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function qr(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? qr(t.subTree) : null;
}
const Gr = (e) => e.__isSuspense;
function Ho(e, t) {
  t && t.pendingBranch ? R(e) ? t.effects.push(...e) : t.effects.push(e) : Bi(e);
}
const Ae = /* @__PURE__ */ Symbol.for("v-fgt"), hs = /* @__PURE__ */ Symbol.for("v-txt"), Xe = /* @__PURE__ */ Symbol.for("v-cmt"), Es = /* @__PURE__ */ Symbol.for("v-stc"), Ye = [];
let le = null;
function Os(e = !1) {
  Ye.push(le = e ? null : []);
}
function Jr() {
  Ye.pop(), le = Ye[Ye.length - 1] || null;
}
let At = 1;
function Rn(e, t = !1) {
  At += e, e < 0 && le && t && (le.hasOnce = !0);
}
function No(e) {
  return e.dynamicChildren = At > 0 ? le || st : null, Jr(), At > 0 && le && le.push(e), e;
}
function As(e, t, s, n, r, i) {
  return No(
    $e(
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
function Yr(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function ht(e, t) {
  return e.type === t.type && e.key === t.key;
}
const zr = ({ key: e }) => e ?? null, zt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? J(e) || /* @__PURE__ */ ee(e) || F(e) ? { i: ce, r: e, k: t, f: !!s } : e : null);
function $e(e, t = null, s = null, n = 0, r = null, i = e === Ae ? 0 : 1, o = !1, l = !1) {
  const f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && zr(t),
    ref: t && zt(t),
    scopeId: vr,
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
    ctx: ce
  };
  return l ? (ss(f, s), i & 128 && e.normalize(f)) : s && (f.shapeFlag |= J(s) ? 8 : 16), At > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  le && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (f.patchFlag > 0 || i & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  f.patchFlag !== 32 && le.push(f), f;
}
const ze = $o;
function $o(e, t = null, s = null, n = 0, r = null, i = !1) {
  if ((!e || e === fo) && (e = Xe), Yr(e)) {
    const l = ot(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ss(l, s), At > 0 && !i && le && (l.shapeFlag & 6 ? le[le.indexOf(e)] = l : le.push(l)), l.patchFlag = -2, l;
  }
  if (Xo(e) && (e = e.__vccOpts), t) {
    t = Ko(t);
    let { class: l, style: f } = t;
    l && !J(l) && (t.class = Ys(l)), B(f) && (/* @__PURE__ */ nn(f) && !R(f) && (f = Q({}, f)), t.style = Js(f));
  }
  const o = J(e) ? 1 : Gr(e) ? 128 : us(e) ? 64 : B(e) ? 4 : F(e) ? 2 : 0;
  return $e(
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
function Ko(e) {
  return e ? /* @__PURE__ */ nn(e) || Nr(e) ? Q({}, e) : e : null;
}
function ot(e, t, s = !1, n = !1) {
  const { props: r, ref: i, patchFlag: o, children: l, transition: f } = e, d = t ? Vo(r || {}, t) : r, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && zr(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && i ? R(i) ? i.concat(zt(t)) : [i, zt(t)] : zt(t)
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
    patchFlag: t && e.type !== Ae ? o === -1 ? 16 : o | 16 : o,
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
    ssContent: e.ssContent && ot(e.ssContent),
    ssFallback: e.ssFallback && ot(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return f && n && on(
    a,
    f.clone(a)
  ), a;
}
function Lo(e = " ", t = 0) {
  return ze(hs, null, e, t);
}
function Se(e) {
  return e == null || typeof e == "boolean" ? ze(Xe) : R(e) ? ze(
    Ae,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Yr(e) ? Pe(e) : ze(hs, null, String(e));
}
function Pe(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : ot(e);
}
function ss(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (R(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const r = t.default;
      r && (r._c && (r._d = !1), ss(e, r()), r._c && (r._d = !0));
      return;
    } else {
      s = 32;
      const r = t._;
      !r && !Nr(t) ? t._ctx = ce : r === 3 && ce && (ce.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (F(t)) {
    if (n & 65) {
      ss(e, { default: t });
      return;
    }
    t = { default: t, _ctx: ce }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [Lo(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function Vo(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const r in n)
      if (r === "class")
        t.class !== n.class && (t.class = Ys([t.class, n.class]));
      else if (r === "style")
        t.style = Js([t.style, n.style]);
      else if (rs(r)) {
        const i = t[r], o = n[r];
        o && i !== o && !(R(i) && i.includes(o)) ? t[r] = i ? [].concat(i, o) : o : o == null && i == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !is(r) && (t[r] = o);
      } else r !== "" && (t[r] = n[r]);
  }
  return t;
}
function ye(e, t, s, n = null) {
  de(e, t, 7, [
    s,
    n
  ]);
}
const Uo = Rr();
let Bo = 0;
function Wo(e, t, s) {
  const n = e.type, r = (t ? t.appContext : e.appContext) || Uo, i = {
    uid: Bo++,
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
    scope: new di(
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
    propsOptions: Kr(n, r),
    emitsOptions: Fr(n, r),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: U,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: U,
    data: U,
    props: U,
    attrs: U,
    slots: U,
    refs: U,
    setupState: U,
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
  return i.ctx = { _: i }, i.root = t ? t.root : i, i.emit = xo.bind(null, i), e.ce && e.ce(i), i;
}
let re = null;
const qo = () => re || ce;
let ns, Pt;
{
  const e = ls(), t = (s, n) => {
    let r;
    return (r = e[s]) || (r = e[s] = []), r.push(n), (i) => {
      r.length > 1 ? r.forEach((o) => o(i)) : r[0](i);
    };
  };
  ns = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => re = s
  ), Pt = t(
    "__VUE_SSR_SETTERS__",
    (s) => Mt = s
  );
}
const Ft = (e) => {
  const t = re;
  return ns(e), e.scope.on(), () => {
    e.scope.off(), ns(t);
  };
}, Fn = () => {
  re && re.scope.off(), ns(null);
};
function Xr(e) {
  return e.vnode.shapeFlag & 4;
}
let Mt = !1;
function Go(e, t = !1, s = !1) {
  t && Pt(t);
  const { props: n, children: r } = e.vnode, i = Xr(e);
  Eo(e, n, i, t), Mo(e, r, s || t);
  const o = i ? Jo(e, t) : void 0;
  return t && Pt(!1), o;
}
function Jo(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, uo);
  const { setup: n } = s;
  if (n) {
    Re();
    const r = e.setupContext = n.length > 1 ? zo(e) : null, i = Ft(e), o = Rt(
      n,
      e,
      0,
      [
        e.props,
        r
      ]
    ), l = Yn(o);
    if (Fe(), i(), (l || e.sp) && !St(e) && Tr(e), l) {
      if (o.then(Fn, Fn), t)
        return o.then((f) => {
          Pt(!0);
          try {
            Dn(e, f, t);
          } finally {
            Pt(!1);
          }
        }).catch((f) => {
          fs(f, e, 0);
        });
      e.asyncDep = o;
    } else
      Dn(e, o);
  } else
    Zr(e);
}
function Dn(e, t, s) {
  F(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : B(t) && (e.setupState = mr(t)), Zr(e);
}
function Zr(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Ce);
  {
    const r = Ft(e);
    Re();
    try {
      ao(e);
    } finally {
      Fe(), r();
    }
  }
}
const Yo = {
  get(e, t) {
    return k(e, "get", ""), e[t];
  }
};
function zo(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, Yo),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function ps(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(mr(Ri(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in wt)
        return wt[s](e);
    },
    has(t, s) {
      return s in t || s in wt;
    }
  })) : e.proxy;
}
function Xo(e) {
  return F(e) && "__vccOpts" in e;
}
const Zo = (e, t) => /* @__PURE__ */ Ni(e, t, Mt), Qo = "3.5.42";
/**
* @vue/runtime-dom v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Vs;
const jn = typeof window < "u" && window.trustedTypes;
if (jn)
  try {
    Vs = /* @__PURE__ */ jn.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Qr = Vs ? (e) => Vs.createHTML(e) : (e) => e, ko = "http://www.w3.org/2000/svg", el = "http://www.w3.org/1998/Math/MathML", Oe = typeof document < "u" ? document : null, Hn = Oe && /* @__PURE__ */ Oe.createElement("template"), tl = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const r = t === "svg" ? Oe.createElementNS(ko, e) : t === "mathml" ? Oe.createElementNS(el, e) : s ? Oe.createElement(e, { is: s }) : Oe.createElement(e);
    return e === "select" && n && n.multiple != null && r.setAttribute("multiple", n.multiple), r;
  },
  createText: (e) => Oe.createTextNode(e),
  createComment: (e) => Oe.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => Oe.querySelector(e),
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
      Hn.innerHTML = Qr(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = Hn.content;
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
}, sl = /* @__PURE__ */ Symbol("_vtc");
function nl(e, t, s) {
  const n = e[sl];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const Nn = /* @__PURE__ */ Symbol("_vod"), rl = /* @__PURE__ */ Symbol("_vsh"), il = /* @__PURE__ */ Symbol(""), ol = /(?:^|;)\s*display\s*:/;
function ll(e, t, s) {
  const n = e.style, r = J(s);
  let i = !1;
  if (s && !r) {
    if (t)
      if (J(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && _t(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && _t(n, o, "");
    for (const o in s) {
      o === "display" && (i = !0);
      const l = s[o];
      l != null ? fl(
        e,
        o,
        !J(t) && t ? t[o] : void 0,
        l
      ) || _t(n, o, l) : _t(n, o, "");
    }
  } else if (r) {
    if (t !== s) {
      const o = n[il];
      o && (s += ";" + o), n.cssText = s, i = ol.test(s);
    }
  } else t && e.removeAttribute("style");
  Nn in e && (e[Nn] = i ? n.display : "", e[rl] && (n.display = "none"));
}
const Ut = /\s*!important$/;
function _t(e, t, s) {
  if (R(s))
    s.forEach((n) => _t(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    Ut.test(s) ? e.setProperty(t, s.replace(Ut, ""), "important") : e.setProperty(t, s);
  else {
    const n = cl(e, t);
    Ut.test(s) ? e.setProperty(
      Ze(n),
      s.replace(Ut, ""),
      "important"
    ) : e[n] = s;
  }
}
const $n = ["Webkit", "Moz", "ms"], Ps = {};
function cl(e, t) {
  const s = Ps[t];
  if (s)
    return s;
  let n = fe(t);
  if (n !== "filter" && n in e)
    return Ps[t] = n;
  n = Zn(n);
  for (let r = 0; r < $n.length; r++) {
    const i = $n[r] + n;
    if (i in e)
      return Ps[t] = i;
  }
  return t;
}
function fl(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && J(n) && s === n;
}
const Kn = "http://www.w3.org/1999/xlink";
function Ln(e, t, s, n, r, i = ui(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Kn, t.slice(6, t.length)) : e.setAttributeNS(Kn, t, s) : s == null || i && !kn(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    i ? "" : Te(s) ? String(s) : s
  );
}
function Vn(e, t, s, n, r) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? Qr(s) : s);
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
    l === "boolean" ? s = kn(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(r || t);
}
function tt(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function ul(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const Un = /* @__PURE__ */ Symbol("_vei");
function al(e, t, s, n, r = null) {
  const i = e[Un] || (e[Un] = {}), o = i[t];
  if (n && o)
    o.value = n;
  else {
    const [l, f] = pl(t);
    if (n) {
      const d = i[t] = _l(
        n,
        r
      );
      tt(e, l, d, f);
    } else o && (ul(e, l, o, f), i[t] = void 0);
  }
}
const dl = /(Once|Passive|Capture)$/, hl = /^on:?(?:Once|Passive|Capture)$/;
function pl(e) {
  let t, s;
  for (; (s = e.match(dl)) && !hl.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : Ze(e.slice(2)), t];
}
let Ms = 0;
const gl = /* @__PURE__ */ Promise.resolve(), ml = () => Ms || (gl.then(() => Ms = 0), Ms = Date.now());
function _l(e, t) {
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
        d && de(
          d,
          t,
          5,
          l
        );
      }
    } else
      de(
        r,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = ml(), s;
}
const Bn = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, bl = (e, t, s, n, r, i) => {
  const o = r === "svg";
  t === "class" ? nl(e, n, o) : t === "style" ? ll(e, s, n) : rs(t) ? is(t) || al(e, t, s, n, i) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : yl(e, t, n, o)) ? (Vn(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Ln(e, t, n, o, i, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (xl(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !J(n))) ? Vn(e, fe(t), n, i, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), Ln(e, t, n, o));
};
function yl(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Bn(t) && F(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const r = e.tagName;
    if (r === "IMG" || r === "VIDEO" || r === "CANVAS" || r === "SOURCE")
      return !1;
  }
  return Bn(t) && J(s) ? !1 : t in e;
}
function xl(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = fe(t);
  return Array.isArray(s) ? s.some((r) => fe(r) === n) : Object.keys(s).some((r) => fe(r) === n);
}
const Wn = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return R(t) ? (s) => Gt(t, s) : t;
};
function vl(e) {
  e.target.composing = !0;
}
function qn(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const Bt = /* @__PURE__ */ Symbol("_assign"), Wt = /* @__PURE__ */ Symbol("_initialValue");
function Is(e, t, s) {
  return t && (e = e.trim()), s && (e = Gs(e)), e;
}
const Sl = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, r) {
    e.parentNode && (e.type === "text" ? e[Wt] = e.defaultValue.replace(/[\r\n]/g, "") : e.type === "textarea" && (e[Wt] = e.defaultValue.replace(/\r\n?/g, `
`))), e[Bt] = Wn(r);
    const i = n || r.props && r.props.type === "number";
    tt(e, t ? "change" : "input", (o) => {
      o.target.composing || e[Bt](Is(e.value, s, i));
    }), (s || i) && tt(e, "change", () => {
      e.value = Is(e.value, s, i);
    }), t || (tt(e, "compositionstart", vl), tt(e, "compositionend", qn), tt(e, "change", qn));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t, modifiers: { trim: s, number: n } }) {
    const r = t ?? "", i = e[Wt];
    delete e[Wt], i !== void 0 && (e.type === "text" || e.type === "textarea") && e.value !== i ? e[Bt](Is(e.value, s, n)) : e.value = r;
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: r, number: i } }, o) {
    if (e[Bt] = Wn(o), e.composing) return;
    const l = (i || e.type === "number") && !/^0\d/.test(e.value) ? Gs(e.value) : e.value, f = t ?? "";
    if (l === f)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (n && t === s || r && e.value.trim() === f) || (e.value = f);
  }
}, wl = ["ctrl", "shift", "alt", "meta"], Cl = {
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
  exact: (e, t) => wl.some((s) => e[`${s}Key`] && !t.includes(s))
}, ke = (e, t) => {
  if (!e) return e;
  const s = e._withMods || (e._withMods = {}), n = t.join(".");
  return s[n] || (s[n] = ((r, ...i) => {
    for (let o = 0; o < t.length; o++) {
      const l = Cl[t[o]];
      if (l && l(r, t)) return;
    }
    return e(r, ...i);
  }));
}, Tl = /* @__PURE__ */ Q({ patchProp: bl }, tl);
let Gn;
function El() {
  return Gn || (Gn = Ro(Tl));
}
const Ol = ((...e) => {
  const t = El().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const r = Pl(n);
    if (!r) return;
    const i = t._component;
    !F(i) && !i.render && !i.template && (i.template = r.innerHTML), r.nodeType === 1 && (r.textContent = "");
    const o = s(r, !1, Al(r));
    return r instanceof Element && (r.removeAttribute("v-cloak"), r.setAttribute("data-v-app", "")), o;
  }, t;
});
function Al(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function Pl(e) {
  return J(e) ? document.querySelector(e) : e;
}
let Us = null;
function Ml(e) {
  Us = e ?? null;
}
function qt(e, t) {
  return Us ? Us(e, t) : e;
}
const Il = { class: "screencast-surface" }, Rl = ["title"], Fl = ["aria-label", "placeholder"], Dl = { type: "submit" }, jl = { class: "state" }, Hl = ["src"], Nl = {
  key: 1,
  class: "browser-empty"
}, pt = "browser-screencast-main", $l = /* @__PURE__ */ Qi({
  __name: "ScreencastPanel",
  props: {
    api: {}
  },
  setup(e) {
    const t = e, s = /* @__PURE__ */ dt(null), n = /* @__PURE__ */ dt("about:blank"), r = /* @__PURE__ */ dt(""), i = /* @__PURE__ */ dt("connecting"), o = /* @__PURE__ */ dt(""), l = [];
    function f(T, w) {
      t.api.send("plugin.panel.input", { panelId: pt, inputType: T, data: w });
    }
    function d() {
      f("navigate", { url: n.value });
    }
    function a(T) {
      const w = T.currentTarget, D = w.getBoundingClientRect();
      return {
        x: (T.clientX - D.left) * w.naturalWidth / D.width,
        y: (T.clientY - D.top) * w.naturalHeight / D.height,
        button: T.button
      };
    }
    function p(T, w) {
      var D;
      (D = s.value) == null || D.focus(), T === "pointerDown" && w.currentTarget.setPointerCapture(w.pointerId), f(T, a(w));
    }
    function C(T) {
      if (T.key.length === 1 && !T.ctrlKey && !T.altKey && !T.metaKey) {
        f("text", { text: T.key });
        return;
      }
      const D = [...[T.ctrlKey && "Control", T.altKey && "Alt", T.shiftKey && "Shift", T.metaKey && "Meta"].filter(Boolean), T.key].join("+");
      f("key", { key: D });
    }
    function E(T) {
      f("wheel", { deltaX: T.deltaX, deltaY: T.deltaY });
    }
    return Or(() => {
      let T = !1;
      const w = () => {
        T || !t.api.send("plugin.panel.open", { panelId: pt, panelType: "browser.screencast", parameters: {} }) || (T = !0);
      };
      w(), l.push(t.api.on("conn", (M) => {
        M != null && M.on && w();
      })), l.push(t.api.on("plugin.panel.opened", (M) => {
        (M == null ? void 0 : M.panelId) === pt && (i.value = "live");
      })), l.push(t.api.on("plugin.panel.event", (M) => {
        var j, H;
        (M == null ? void 0 : M.panelId) === pt && (M.eventType === "frame" && ((j = M.data) != null && j.base64) ? (r.value = `data:${M.data.mimeType || "image/jpeg"};base64,${M.data.base64}`, M.data.url && (n.value = M.data.url)) : M.eventType === "error" && (o.value = ((H = M.data) == null ? void 0 : H.message) || "Browser error", i.value = "error"));
      }));
      const D = new ResizeObserver((M) => {
        var H;
        const j = (H = M[0]) == null ? void 0 : H.contentRect;
        j && j.width > 0 && j.height > 0 && f("resize", { width: Math.max(320, Math.round(j.width)), height: Math.max(200, Math.round(j.height)) });
      });
      s.value && D.observe(s.value), l.push(() => D.disconnect());
    }), Ar(() => {
      t.api.send("plugin.panel.close", { panelId: pt }), l.forEach((T) => T());
    }), (T, w) => (Os(), As("div", Il, [
      $e("form", {
        class: "browser-bar",
        onSubmit: ke(d, ["prevent"])
      }, [
        $e("button", {
          type: "button",
          title: gt(qt)("Reload"),
          onClick: d
        }, "↻", 8, Rl),
        qi($e("input", {
          "onUpdate:modelValue": w[0] || (w[0] = (D) => n.value = D),
          "aria-label": gt(qt)("Browser address"),
          placeholder: gt(qt)("Enter URL")
        }, null, 8, Fl), [
          [Sl, n.value]
        ]),
        $e("button", Dl, Jt(gt(qt)("Go")), 1),
        $e("span", jl, Jt(i.value), 1)
      ], 32),
      $e("div", {
        ref_key: "viewport",
        ref: s,
        class: "browser-viewport",
        tabindex: "0",
        onKeydown: ke(C, ["prevent"]),
        onWheel: ke(E, ["prevent"])
      }, [
        r.value ? (Os(), As("img", {
          key: 0,
          src: r.value,
          draggable: "false",
          alt: "Browser screencast",
          onPointerdown: w[1] || (w[1] = ke((D) => p("pointerDown", D), ["prevent"])),
          onPointermove: w[2] || (w[2] = ke((D) => p("pointerMove", D), ["prevent"])),
          onPointerup: w[3] || (w[3] = ke((D) => p("pointerUp", D), ["prevent"]))
        }, null, 40, Hl)) : (Os(), As("div", Nl, Jt(o.value || "Starting experimental browser…"), 1))
      ], 544)
    ]));
  }
}), Kl = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, r] of t)
    s[n] = r;
  return s;
}, Ll = /* @__PURE__ */ Kl($l, [["__scopeId", "data-v-f9b5c84e"]]);
function Ul(e, t) {
  var n;
  Ml((n = t.t) == null ? void 0 : n.bind(t));
  let s = Ol(Ll, { api: t });
  return s.mount(e), { destroy: () => {
    s == null || s.unmount(), s = null;
  } };
}
export {
  Ul as mount
};
