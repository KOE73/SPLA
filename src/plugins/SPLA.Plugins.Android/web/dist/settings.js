(function(){"use strict";try{if(typeof document<"u"){var a=document.createElement("style");a.appendChild(document.createTextNode(".android-settings[data-v-0bc23ba1]{display:grid;gap:12px}section[data-v-0bc23ba1]{display:grid;gap:8px}h3[data-v-0bc23ba1],p[data-v-0bc23ba1]{margin:0}label[data-v-0bc23ba1]{display:flex;align-items:center;gap:8px;flex-wrap:wrap}input[data-v-0bc23ba1]:not([type=checkbox]){flex:1;min-width:120px}.actions[data-v-0bc23ba1]{display:flex;gap:8px;flex-wrap:wrap}.folder[data-v-0bc23ba1]{overflow-wrap:anywhere;opacity:.7}")),document.head.appendChild(a)}}catch(e){console.error("vite-plugin-css-injected-by-js",e)}})();
/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Bs(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const K = {}, ct = [], je = () => {
}, Yn = () => !1, ls = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), os = (e) => e.startsWith("onUpdate:"), ee = Object.assign, Ws = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, ni = Object.prototype.hasOwnProperty, H = (e, t) => ni.call(e, t), M = Array.isArray, ut = (e) => Ht(e) === "[object Map]", fs = (e) => Ht(e) === "[object Set]", pn = (e) => Ht(e) === "[object Date]", R = (e) => typeof e == "function", Y = (e) => typeof e == "string", Ne = (e) => typeof e == "symbol", $ = (e) => e !== null && typeof e == "object", zn = (e) => ($(e) || R(e)) && R(e.then) && R(e.catch), Xn = Object.prototype.toString, Ht = (e) => Xn.call(e), ri = (e) => Ht(e).slice(8, -1), Zn = (e) => Ht(e) === "[object Object]", ks = (e) => Y(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, Ct = /* @__PURE__ */ Bs(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), cs = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, ii = /-\w/g, xe = cs(
  (e) => e.replace(ii, (t) => t.slice(1).toUpperCase())
), li = /\B([A-Z])/g, lt = cs(
  (e) => e.replace(li, "-$1").toLowerCase()
), Qn = cs((e) => e.charAt(0).toUpperCase() + e.slice(1)), ys = cs(
  (e) => e ? `on${Qn(e)}` : ""
), De = (e, t) => !Object.is(e, t), Yt = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, er = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, Js = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let gn;
const us = () => gn || (gn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function qs(e) {
  if (M(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], r = Y(n) ? ui(n) : qs(n);
      if (r)
        for (const i in r)
          t[i] = r[i];
    }
    return t;
  } else if (Y(e) || $(e))
    return e;
}
const oi = /;(?![^(]*\))/g, fi = /:([^]+)/, ci = /\/\*[^]*?\*\//g;
function ui(e) {
  const t = {};
  return e.replace(ci, "").split(oi).forEach((s) => {
    if (s) {
      const n = s.split(fi);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function Gs(e) {
  let t = "";
  if (Y(e))
    t = e;
  else if (M(e))
    for (let s = 0; s < e.length; s++) {
      const n = Gs(e[s]);
      n && (t += n + " ");
    }
  else if ($(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const ai = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", di = /* @__PURE__ */ Bs(ai);
function tr(e) {
  return !!e || e === "";
}
function hi(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = Vt(e[n], t[n]);
  return s;
}
function Vt(e, t) {
  if (e === t) return !0;
  let s = pn(e), n = pn(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Ne(e), n = Ne(t), s || n)
    return e === t;
  if (s = M(e), n = M(t), s || n)
    return s && n ? hi(e, t) : !1;
  if (s = $(e), n = $(t), s || n) {
    if (!s || !n)
      return !1;
    const r = Object.keys(e).length, i = Object.keys(t).length;
    if (r !== i)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), c = t.hasOwnProperty(o);
      if (l && !c || !l && c || !Vt(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function sr(e, t) {
  return e.findIndex((s) => Vt(s, t));
}
const nr = (e) => !!(e && e.__v_isRef === !0), G = (e) => Y(e) ? e : e == null ? "" : M(e) || $(e) && (e.toString === Xn || !R(e.toString)) ? nr(e) ? G(e.value) : JSON.stringify(e, rr, 2) : String(e), rr = (e, t) => nr(t) ? rr(e, t.value) : ut(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, r], i) => (s[vs(n, i) + " =>"] = r, s),
    {}
  )
} : fs(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => vs(s))
} : Ne(t) ? vs(t) : $(t) && !M(t) && !Zn(t) ? String(t) : t, vs = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Ne(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Q;
class pi {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && Q && (Q.active ? (this.parent = Q, this.index = (Q.scopes || (Q.scopes = [])).push(
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
      const s = Q;
      try {
        return Q = this, t();
      } finally {
        Q = s;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = Q, Q = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (Q === this)
        Q = this.prevScope;
      else {
        let t = Q;
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
        const r = this.parent.scopes.pop();
        r && r !== this && (this.parent.scopes[this.index] = r, r.index = this.index);
      }
      this.parent = void 0;
    }
  }
}
function gi() {
  return Q;
}
let k;
const xs = /* @__PURE__ */ new WeakSet();
class ir {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, Q && (Q.active ? Q.effects.push(this) : this.flags &= -2);
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
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || or(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, _n(this), fr(this);
    const t = k, s = Se;
    k = this, Se = !0;
    try {
      return this.fn();
    } finally {
      cr(this), k = t, Se = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        Xs(t);
      this.deps = this.depsTail = void 0, _n(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? xs.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
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
let lr = 0, Tt, Et;
function or(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Et, Et = e;
    return;
  }
  e.next = Tt, Tt = e;
}
function Ys() {
  lr++;
}
function zs() {
  if (--lr > 0)
    return;
  if (Et) {
    let t = Et;
    for (Et = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; Tt; ) {
    let t = Tt;
    for (Tt = void 0; t; ) {
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
function fr(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function cr(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const r = n.prevDep;
    n.version === -1 ? (n === s && (s = r), Xs(n), _i(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = r;
  }
  e.deps = t, e.depsTail = s;
}
function Rs(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (ur(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function ur(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === It) || (e.globalVersion = It, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Rs(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = k, n = Se;
  k = e, Se = !0;
  try {
    fr(e);
    const r = e.fn(e._value);
    (t.version === 0 || De(r, e._value)) && (e.flags |= 128, e._value = r, t.version++);
  } catch (r) {
    throw t.version++, r;
  } finally {
    k = s, Se = n, cr(e), e.flags &= -3;
  }
}
function Xs(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: r } = e;
  if (n && (n.nextSub = r, e.prevSub = void 0), r && (r.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let i = s.computed.deps; i; i = i.nextDep)
      Xs(i, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function _i(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let Se = !0;
const ar = [];
function He() {
  ar.push(Se), Se = !1;
}
function Ve() {
  const e = ar.pop();
  Se = e === void 0 ? !0 : e;
}
function _n(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const s = k;
    k = void 0;
    try {
      t();
    } finally {
      k = s;
    }
  }
}
let It = 0;
class mi {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class Zs {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!k || !Se || k === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== k)
      s = this.activeLink = new mi(k, this), k.deps ? (s.prevDep = k.depsTail, k.depsTail.nextDep = s, k.depsTail = s) : k.deps = k.depsTail = s, dr(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = k.depsTail, s.nextDep = void 0, k.depsTail.nextDep = s, k.depsTail = s, k.deps === s && (k.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, It++, this.notify(t);
  }
  notify(t) {
    Ys();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      zs();
    }
  }
}
function dr(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let n = t.deps; n; n = n.nextDep)
        dr(n);
    }
    const s = e.dep.subs;
    s !== e && (e.prevSub = s, s && (s.nextSub = e)), e.dep.subs = e;
  }
}
const Fs = /* @__PURE__ */ new WeakMap(), rt = /* @__PURE__ */ Symbol(
  ""
), Ds = /* @__PURE__ */ Symbol(
  ""
), Rt = /* @__PURE__ */ Symbol(
  ""
);
function te(e, t, s) {
  if (Se && k) {
    let n = Fs.get(e);
    n || Fs.set(e, n = /* @__PURE__ */ new Map());
    let r = n.get(s);
    r || (n.set(s, r = new Zs()), r.map = n, r.key = s), r.track();
  }
}
function Ke(e, t, s, n, r, i) {
  const o = Fs.get(e);
  if (!o) {
    It++;
    return;
  }
  const l = (c) => {
    c && c.trigger();
  };
  if (Ys(), t === "clear")
    o.forEach(l);
  else {
    const c = M(e), d = c && ks(s);
    if (c && s === "length") {
      const a = Number(n);
      o.forEach((p, C) => {
        (C === "length" || C === Rt || !Ne(C) && C >= a) && l(p);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(Rt)), t) {
        case "add":
          c ? d && l(o.get("length")) : (l(o.get(rt)), ut(e) && l(o.get(Ds)));
          break;
        case "delete":
          c || (l(o.get(rt)), ut(e) && l(o.get(Ds)));
          break;
        case "set":
          ut(e) && l(o.get(rt));
          break;
      }
  }
  zs();
}
function ot(e) {
  const t = /* @__PURE__ */ N(e);
  return t === e ? t : (te(t, "iterate", Rt), /* @__PURE__ */ me(e) ? t : t.map(we));
}
function as(e) {
  return te(e = /* @__PURE__ */ N(e), "iterate", Rt), e;
}
function Re(e, t) {
  return /* @__PURE__ */ ke(e) ? pt(/* @__PURE__ */ it(e) ? we(t) : t) : we(t);
}
const bi = {
  __proto__: null,
  [Symbol.iterator]() {
    return Ss(this, Symbol.iterator, (e) => Re(this, e));
  },
  concat(...e) {
    return ot(this).concat(
      ...e.map((t) => M(t) ? ot(t) : t)
    );
  },
  entries() {
    return Ss(this, "entries", (e) => (e[1] = Re(this, e[1]), e));
  },
  every(e, t) {
    return $e(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return $e(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => Re(this, n)),
      arguments
    );
  },
  find(e, t) {
    return $e(
      this,
      "find",
      e,
      t,
      (s) => Re(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return $e(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return $e(
      this,
      "findLast",
      e,
      t,
      (s) => Re(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return $e(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return $e(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return ws(this, "includes", e);
  },
  indexOf(...e) {
    return ws(this, "indexOf", e);
  },
  join(e) {
    return ot(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return ws(this, "lastIndexOf", e);
  },
  map(e, t) {
    return $e(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return vt(this, "pop");
  },
  push(...e) {
    return vt(this, "push", e);
  },
  reduce(e, ...t) {
    return mn(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return mn(this, "reduceRight", e, t);
  },
  shift() {
    return vt(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return $e(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return vt(this, "splice", e);
  },
  toReversed() {
    return ot(this).toReversed();
  },
  toSorted(e) {
    return ot(this).toSorted(e);
  },
  toSpliced(...e) {
    return ot(this).toSpliced(...e);
  },
  unshift(...e) {
    return vt(this, "unshift", e);
  },
  values() {
    return Ss(this, "values", (e) => Re(this, e));
  }
};
function Ss(e, t, s) {
  const n = as(e), r = n[t]();
  return n !== e && !/* @__PURE__ */ me(e) && (r._next = r.next, r.next = () => {
    const i = r._next();
    return i.done || (i.value = s(i.value)), i;
  }), r;
}
const yi = Array.prototype;
function $e(e, t, s, n, r, i) {
  const o = as(e), l = o !== e && !/* @__PURE__ */ me(e), c = o[t];
  if (c !== yi[t]) {
    const p = c.apply(e, i);
    return l ? we(p) : p;
  }
  let d = s;
  o !== e && (l ? d = function(p, C) {
    return s.call(this, Re(e, p), C, e);
  } : s.length > 2 && (d = function(p, C) {
    return s.call(this, p, C, e);
  }));
  const a = c.call(o, d, n);
  return l && r ? r(a) : a;
}
function mn(e, t, s, n) {
  const r = as(e), i = r !== e && !/* @__PURE__ */ me(e);
  let o = s, l = !1;
  r !== e && (i ? (l = n.length === 0, o = function(d, a, p) {
    return l && (l = !1, d = Re(e, d)), s.call(this, d, Re(e, a), p, e);
  }) : s.length > 3 && (o = function(d, a, p) {
    return s.call(this, d, a, p, e);
  }));
  const c = r[t](o, ...n);
  return l ? Re(e, c) : c;
}
function ws(e, t, s) {
  const n = /* @__PURE__ */ N(e);
  te(n, "iterate", Rt);
  const r = n[t](...s);
  return (r === -1 || r === !1) && /* @__PURE__ */ tn(s[0]) ? (s[0] = /* @__PURE__ */ N(s[0]), n[t](...s)) : r;
}
function vt(e, t, s = []) {
  He(), Ys();
  const n = (/* @__PURE__ */ N(e))[t].apply(e, s);
  return zs(), Ve(), n;
}
const vi = /* @__PURE__ */ Bs("__proto__,__v_isRef,__isVue"), hr = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Ne)
);
function xi(e) {
  Ne(e) || (e = String(e));
  const t = /* @__PURE__ */ N(this);
  return te(t, "has", e), t.hasOwnProperty(e);
}
class pr {
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
      return n === (r ? i ? Ii : br : i ? mr : _r).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = M(t);
    if (!r) {
      let c;
      if (o && (c = bi[s]))
        return c;
      if (s === "hasOwnProperty")
        return xi;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ se(t) ? t : n
    );
    if ((Ne(s) ? hr.has(s) : vi(s)) || (r || te(t, "get", s), i))
      return l;
    if (/* @__PURE__ */ se(l)) {
      const c = o && ks(s) ? l : l.value;
      return r && $(c) ? /* @__PURE__ */ Ns(c) : c;
    }
    return $(l) ? r ? /* @__PURE__ */ Ns(l) : /* @__PURE__ */ Ft(l) : l;
  }
}
class gr extends pr {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, r) {
    let i = t[s];
    const o = M(t) && ks(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ ke(i);
      if (!/* @__PURE__ */ me(n) && !/* @__PURE__ */ ke(n) && (i = /* @__PURE__ */ N(i), n = /* @__PURE__ */ N(n)), !o && /* @__PURE__ */ se(i) && !/* @__PURE__ */ se(n))
        return d || (i.value = n), !0;
    }
    const l = o ? Number(s) < t.length : H(t, s), c = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ se(t) ? t : r
    );
    return t === /* @__PURE__ */ N(r) && c && (l ? De(n, i) && Ke(t, "set", s, n) : Ke(t, "add", s, n)), c;
  }
  deleteProperty(t, s) {
    const n = H(t, s);
    t[s];
    const r = Reflect.deleteProperty(t, s);
    return r && n && Ke(t, "delete", s, void 0), r;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Ne(s) || !hr.has(s)) && te(t, "has", s), n;
  }
  ownKeys(t) {
    return te(
      t,
      "iterate",
      M(t) ? "length" : rt
    ), Reflect.ownKeys(t);
  }
}
class Si extends pr {
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
const wi = /* @__PURE__ */ new gr(), Ci = /* @__PURE__ */ new Si(), Ti = /* @__PURE__ */ new gr(!0);
const js = (e) => e, kt = (e) => Reflect.getPrototypeOf(e);
function Ei(e, t, s) {
  return function(...n) {
    const r = this.__v_raw, i = /* @__PURE__ */ N(r), o = ut(i), l = e === "entries" || e === Symbol.iterator && o, c = e === "keys" && o, d = r[e](...n), a = s ? js : t ? pt : we;
    return !t && te(
      i,
      "iterate",
      c ? Ds : rt
    ), ee(
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
function Jt(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function Oi(e, t) {
  const s = {
    get(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ N(i), l = /* @__PURE__ */ N(r);
      e || (De(r, l) && te(o, "get", r), te(o, "get", l));
      const { has: c } = kt(o), d = t ? js : e ? pt : we;
      if (c.call(o, r))
        return d(i.get(r));
      if (c.call(o, l))
        return d(i.get(l));
      i !== o && i.get(r);
    },
    get size() {
      const r = this.__v_raw;
      return !e && te(/* @__PURE__ */ N(r), "iterate", rt), r.size;
    },
    has(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ N(i), l = /* @__PURE__ */ N(r);
      return e || (De(r, l) && te(o, "has", r), te(o, "has", l)), r === l ? i.has(r) : i.has(r) || i.has(l);
    },
    forEach(r, i) {
      const o = this, l = o.__v_raw, c = /* @__PURE__ */ N(l), d = t ? js : e ? pt : we;
      return !e && te(c, "iterate", rt), l.forEach((a, p) => r.call(i, d(a), d(p), o));
    }
  };
  return ee(
    s,
    e ? {
      add: Jt("add"),
      set: Jt("set"),
      delete: Jt("delete"),
      clear: Jt("clear")
    } : {
      add(r) {
        const i = /* @__PURE__ */ N(this), o = kt(i), l = /* @__PURE__ */ N(r), c = !t && !/* @__PURE__ */ me(r) && !/* @__PURE__ */ ke(r) ? l : r;
        return o.has.call(i, c) || De(r, c) && o.has.call(i, r) || De(l, c) && o.has.call(i, l) || (i.add(c), Ke(i, "add", c, c)), this;
      },
      set(r, i) {
        !t && !/* @__PURE__ */ me(i) && !/* @__PURE__ */ ke(i) && (i = /* @__PURE__ */ N(i));
        const o = /* @__PURE__ */ N(this), { has: l, get: c } = kt(o);
        let d = l.call(o, r);
        d || (r = /* @__PURE__ */ N(r), d = l.call(o, r));
        const a = c.call(o, r);
        return o.set(r, i), d ? De(i, a) && Ke(o, "set", r, i) : Ke(o, "add", r, i), this;
      },
      delete(r) {
        const i = /* @__PURE__ */ N(this), { has: o, get: l } = kt(i);
        let c = o.call(i, r);
        c || (r = /* @__PURE__ */ N(r), c = o.call(i, r)), l && l.call(i, r);
        const d = i.delete(r);
        return c && Ke(i, "delete", r, void 0), d;
      },
      clear() {
        const r = /* @__PURE__ */ N(this), i = r.size !== 0, o = r.clear();
        return i && Ke(
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
    s[r] = Ei(r, e, t);
  }), s;
}
function Qs(e, t) {
  const s = Oi(e, t);
  return (n, r, i) => r === "__v_isReactive" ? !e : r === "__v_isReadonly" ? e : r === "__v_raw" ? n : Reflect.get(
    H(s, r) && r in n ? s : n,
    r,
    i
  );
}
const Ai = {
  get: /* @__PURE__ */ Qs(!1, !1)
}, Pi = {
  get: /* @__PURE__ */ Qs(!1, !0)
}, Mi = {
  get: /* @__PURE__ */ Qs(!0, !1)
};
const _r = /* @__PURE__ */ new WeakMap(), mr = /* @__PURE__ */ new WeakMap(), br = /* @__PURE__ */ new WeakMap(), Ii = /* @__PURE__ */ new WeakMap();
function Ri(e) {
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
function Ft(e) {
  return /* @__PURE__ */ ke(e) ? e : en(
    e,
    !1,
    wi,
    Ai,
    _r
  );
}
// @__NO_SIDE_EFFECTS__
function Fi(e) {
  return en(
    e,
    !1,
    Ti,
    Pi,
    mr
  );
}
// @__NO_SIDE_EFFECTS__
function Ns(e) {
  return en(
    e,
    !0,
    Ci,
    Mi,
    br
  );
}
function en(e, t, s, n, r) {
  if (!$(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const i = r.get(e);
  if (i)
    return i;
  const o = Ri(ri(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? n : s
  );
  return r.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function it(e) {
  return /* @__PURE__ */ ke(e) ? /* @__PURE__ */ it(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function ke(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function me(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function tn(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function N(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ N(t) : e;
}
function Di(e) {
  return !H(e, "__v_skip") && Object.isExtensible(e) && er(e, "__v_skip", !0), e;
}
const we = (e) => $(e) ? /* @__PURE__ */ Ft(e) : e, pt = (e) => $(e) ? /* @__PURE__ */ Ns(e) : e;
// @__NO_SIDE_EFFECTS__
function se(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Cs(e) {
  return ji(e, !1);
}
function ji(e, t) {
  return /* @__PURE__ */ se(e) ? e : new Ni(e, t);
}
class Ni {
  constructor(t, s) {
    this.dep = new Zs(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ N(t), this._value = s ? t : we(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ me(t) || /* @__PURE__ */ ke(t);
    t = n ? t : /* @__PURE__ */ N(t), De(t, s) && (this._rawValue = t, this._value = n ? t : we(t), this.dep.trigger());
  }
}
function ae(e) {
  return /* @__PURE__ */ se(e) ? e.value : e;
}
const Hi = {
  get: (e, t, s) => t === "__v_raw" ? e : ae(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const r = e[t];
    return /* @__PURE__ */ se(r) && !/* @__PURE__ */ se(s) ? (r.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function yr(e) {
  return /* @__PURE__ */ it(e) ? e : new Proxy(e, Hi);
}
class Vi {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Zs(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = It - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    k !== this)
      return or(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return ur(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function $i(e, t, s = !1) {
  let n, r;
  return R(e) ? n = e : (n = e.get, r = e.set), new Vi(n, r, s);
}
const qt = {}, Zt = /* @__PURE__ */ new WeakMap();
let tt;
function Li(e, t = !1, s = tt) {
  if (s) {
    let n = Zt.get(s);
    n || Zt.set(s, n = []), n.push(e);
  }
}
function Ui(e, t, s = K) {
  const { immediate: n, deep: r, once: i, scheduler: o, augmentJob: l, call: c } = s, d = (_) => r ? _ : /* @__PURE__ */ me(_) || r === !1 || r === 0 ? Be(_, 1) : Be(_);
  let a, p, C, T, j = !1, P = !1;
  if (/* @__PURE__ */ se(e) ? (p = () => e.value, j = /* @__PURE__ */ me(e)) : /* @__PURE__ */ it(e) ? (p = () => d(e), j = !0) : M(e) ? (P = !0, j = e.some((_) => /* @__PURE__ */ it(_) || /* @__PURE__ */ me(_)), p = () => e.map((_) => {
    if (/* @__PURE__ */ se(_))
      return _.value;
    if (/* @__PURE__ */ it(_))
      return d(_);
    if (R(_))
      return c ? c(_, 2) : _();
  })) : R(e) ? t ? p = c ? () => c(e, 2) : e : p = () => {
    if (C) {
      He();
      try {
        C();
      } finally {
        Ve();
      }
    }
    const _ = tt;
    tt = a;
    try {
      return c ? c(e, 3, [T]) : e(T);
    } finally {
      tt = _;
    }
  } : p = je, t && r) {
    const _ = p, L = r === !0 ? 1 / 0 : r;
    p = () => Be(_(), L);
  }
  const J = gi(), B = () => {
    a.stop(), J && J.active && Ws(J.effects, a);
  };
  if (i && t) {
    const _ = t;
    t = (...L) => {
      const fe = _(...L);
      return B(), fe;
    };
  }
  let O = P ? new Array(e.length).fill(qt) : qt;
  const F = (_) => {
    if (!(!(a.flags & 1) || !a.dirty && !_))
      if (t) {
        const L = a.run();
        if (_ || r || j || (P ? L.some((fe, ce) => De(fe, O[ce])) : De(L, O))) {
          C && C();
          const fe = tt;
          tt = a;
          try {
            const ce = [
              L,
              // pass undefined as the old value when it's changed for the first time
              O === qt ? void 0 : P && O[0] === qt ? [] : O,
              T
            ];
            O = L, c ? c(t, 3, ce) : (
              // @ts-expect-error
              t(...ce)
            );
          } finally {
            tt = fe;
          }
        }
      } else
        a.run();
  };
  return l && l(F), a = new ir(p), a.scheduler = o ? () => o(F, !1) : F, T = (_) => Li(_, !1, a), C = a.onStop = () => {
    const _ = Zt.get(a);
    if (_) {
      if (c)
        c(_, 4);
      else
        for (const L of _) L();
      Zt.delete(a);
    }
  }, t ? n ? F(!0) : O = a.run() : o ? o(F.bind(null, !0), !0) : a.run(), B.pause = a.pause.bind(a), B.resume = a.resume.bind(a), B.stop = B, B;
}
function Be(e, t = 1 / 0, s) {
  if (t <= 0 || !$(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ se(e))
    Be(e.value, t, s);
  else if (M(e))
    for (let n = 0; n < e.length; n++)
      Be(e[n], t, s);
  else if (fs(e) || ut(e))
    e.forEach((n) => {
      Be(n, t, s);
    });
  else if (Zn(e)) {
    for (const n in e)
      Be(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && Be(e[n], t, s);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function $t(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (r) {
    ds(r, t, s);
  }
}
function Ce(e, t, s, n) {
  if (R(e)) {
    const r = $t(e, t, s, n);
    return r && zn(r) && r.catch((i) => {
      ds(i, t, s);
    }), r;
  }
  if (M(e)) {
    const r = [];
    for (let i = 0; i < e.length; i++)
      r.push(Ce(e[i], t, s, n));
    return r;
  }
}
function ds(e, t, s, n = !0) {
  const r = t ? t.vnode : null, { errorHandler: i, throwUnhandledErrorInProduction: o } = t && t.appContext.config || K;
  if (t) {
    let l = t.parent;
    const c = t.proxy, d = `https://vuejs.org/error-reference/#runtime-${s}`;
    for (; l; ) {
      const a = l.ec;
      if (a) {
        for (let p = 0; p < a.length; p++)
          if (a[p](e, c, d) === !1)
            return;
      }
      l = l.parent;
    }
    if (i) {
      He(), $t(i, null, 10, [
        e,
        c,
        d
      ]), Ve();
      return;
    }
  }
  Ki(e, s, r, n, o);
}
function Ki(e, t, s, n = !0, r = !1) {
  if (r)
    throw e;
  console.error(e);
}
const ie = [];
let Ie = -1;
const at = [];
let Ge = null, ft = 0;
const vr = /* @__PURE__ */ Promise.resolve();
let Qt = null;
function Bi(e) {
  const t = Qt || vr;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Wi(e) {
  let t = Ie + 1, s = ie.length;
  for (; t < s; ) {
    const n = t + s >>> 1, r = ie[n], i = Dt(r);
    i < e || i === e && r.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function sn(e) {
  if (!(e.flags & 1)) {
    const t = Dt(e), s = ie[ie.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Dt(s) ? ie.push(e) : ie.splice(Wi(t), 0, e), e.flags |= 1, xr();
  }
}
function xr() {
  Qt || (Qt = vr.then(wr));
}
function ki(e) {
  M(e) ? at.push(...e) : Ge && e.id === -1 ? Ge.splice(ft + 1, 0, e) : e.flags & 1 || (at.push(e), e.flags |= 1), xr();
}
function bn(e, t, s = Ie + 1) {
  for (; s < ie.length; s++) {
    const n = ie[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      ie.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function Sr(e) {
  if (at.length) {
    const t = [...new Set(at)].sort(
      (s, n) => Dt(s) - Dt(n)
    );
    if (at.length = 0, Ge) {
      Ge.push(...t);
      return;
    }
    for (Ge = t, ft = 0; ft < Ge.length; ft++) {
      const s = Ge[ft];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    Ge = null, ft = 0;
  }
}
const Dt = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function wr(e) {
  try {
    for (Ie = 0; Ie < ie.length; Ie++) {
      const t = ie[Ie];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), $t(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Ie < ie.length; Ie++) {
      const t = ie[Ie];
      t && (t.flags &= -2);
    }
    Ie = -1, ie.length = 0, Sr(), Qt = null, (ie.length || at.length) && wr();
  }
}
let _e = null, Cr = null;
function es(e) {
  const t = _e;
  return _e = e, Cr = e && e.type.__scopeId || null, t;
}
function Ji(e, t = _e, s) {
  if (!t || e._n)
    return e;
  const n = (...r) => {
    n._d && Mn(-1);
    const i = es(t);
    let o;
    try {
      o = e(...r);
    } finally {
      es(i), n._d && Mn(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function Gt(e, t) {
  if (_e === null)
    return e;
  const s = _s(_e), n = e.dirs || (e.dirs = []);
  for (let r = 0; r < t.length; r++) {
    let [i, o, l, c = K] = t[r];
    i && (R(i) && (i = {
      mounted: i,
      updated: i
    }), i.deep && Be(o), n.push({
      dir: i,
      instance: s,
      value: o,
      oldValue: void 0,
      arg: l,
      modifiers: c
    }));
  }
  return e;
}
function Qe(e, t, s, n) {
  const r = e.dirs, i = t && t.dirs;
  for (let o = 0; o < r.length; o++) {
    const l = r[o];
    i && (l.oldValue = i[o].value);
    let c = l.dir[n];
    c && (He(), Ce(c, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Ve());
  }
}
function qi(e, t) {
  if (le) {
    let s = le.provides;
    const n = le.parent && le.parent.provides;
    n === s && (s = le.provides = Object.create(n)), s[e] = t;
  }
}
function zt(e, t, s = !1) {
  const n = Jl();
  if (n || dt) {
    let r = dt ? dt._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (r && e in r)
      return r[e];
    if (arguments.length > 1)
      return s && R(t) ? t.call(n && n.proxy) : t;
  }
}
const Gi = /* @__PURE__ */ Symbol.for("v-scx"), Yi = () => zt(Gi);
function Ts(e, t, s) {
  return Tr(e, t, s);
}
function Tr(e, t, s = K) {
  const { immediate: n, deep: r, flush: i, once: o } = s, l = ee({}, s), c = t && n || !t && i !== "post";
  let d;
  if (Nt) {
    if (i === "sync") {
      const T = Yi();
      d = T.__watcherHandles || (T.__watcherHandles = []);
    } else if (!c) {
      const T = () => {
      };
      return T.stop = je, T.resume = je, T.pause = je, T;
    }
  }
  const a = le;
  l.call = (T, j, P) => Ce(T, a, j, P);
  let p = !1;
  i === "post" ? l.scheduler = (T) => {
    oe(T, a && a.suspense);
  } : i !== "sync" && (p = !0, l.scheduler = (T, j) => {
    j ? T() : sn(T);
  }), l.augmentJob = (T) => {
    t && (T.flags |= 4), p && (T.flags |= 2, a && (T.id = a.uid, T.i = a));
  };
  const C = Ui(e, t, l);
  return Nt && (d ? d.push(C) : c && C()), C;
}
function zi(e, t, s) {
  const n = this.proxy, r = Y(e) ? e.includes(".") ? Er(n, e) : () => n[e] : e.bind(n, n);
  let i;
  R(t) ? i = t : (i = t.handler, s = t);
  const o = Lt(this), l = Tr(r, i.bind(n), s);
  return o(), l;
}
function Er(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let r = 0; r < s.length && n; r++)
      n = n[s[r]];
    return n;
  };
}
const Xi = /* @__PURE__ */ Symbol("_vte"), Zi = (e) => e.__isTeleport, Es = /* @__PURE__ */ Symbol("_leaveCb");
function nn(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, nn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Qi(e, t) {
  return R(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    ee({ name: e.name }, t, { setup: e })
  ) : e;
}
function Or(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function yn(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const ts = /* @__PURE__ */ new WeakMap();
function Ot(e, t, s, n, r = !1) {
  if (M(e)) {
    e.forEach(
      (P, J) => Ot(
        P,
        t && (M(t) ? t[J] : t),
        s,
        n,
        r
      )
    );
    return;
  }
  if (At(n) && !r) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Ot(e, t, s, n.component.subTree);
    return;
  }
  const i = n.shapeFlag & 4 ? _s(n.component) : n.el, o = r ? null : i, { i: l, r: c } = e, d = t && t.r, a = l.refs === K ? l.refs = {} : l.refs, p = l.setupState, C = /* @__PURE__ */ N(p), T = p === K ? Yn : (P) => yn(a, P) ? !1 : H(C, P), j = (P, J) => !(J && yn(a, J));
  if (d != null && d !== c) {
    if (vn(t), Y(d))
      a[d] = null, T(d) && (p[d] = null);
    else if (/* @__PURE__ */ se(d)) {
      const P = t;
      j(d, P.k) && (d.value = null), P.k && (a[P.k] = null);
    }
  }
  if (R(c)) {
    He();
    try {
      $t(c, l, 12, [o, a]);
    } finally {
      Ve();
    }
  } else {
    const P = Y(c), J = /* @__PURE__ */ se(c);
    if (P || J) {
      const B = () => {
        if (e.f) {
          const O = P ? T(c) ? p[c] : a[c] : j() || !e.k ? c.value : a[e.k];
          if (r)
            M(O) && Ws(O, i);
          else if (M(O))
            O.includes(i) || O.push(i);
          else if (P)
            a[c] = [i], T(c) && (p[c] = a[c]);
          else {
            const F = [i];
            j(c, e.k) && (c.value = F), e.k && (a[e.k] = F);
          }
        } else P ? (a[c] = o, T(c) && (p[c] = o)) : J && (j(c, e.k) && (c.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const O = () => {
          B(), ts.delete(e);
        };
        O.id = -1, ts.set(e, O), oe(O, s);
      } else
        vn(e), B();
    }
  }
}
function vn(e) {
  const t = ts.get(e);
  t && (t.flags |= 8, ts.delete(e));
}
us().requestIdleCallback;
us().cancelIdleCallback;
const At = (e) => !!e.type.__asyncLoader, Ar = (e) => e.type.__isKeepAlive;
function el(e, t) {
  Pr(e, "a", t);
}
function tl(e, t) {
  Pr(e, "da", t);
}
function Pr(e, t, s = le) {
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
      Ar(r.parent.vnode) && sl(n, t, s, r), r = r.parent;
  }
}
function sl(e, t, s, n) {
  const r = hs(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  rn(() => {
    Ws(n[t], r);
  }, s);
}
function hs(e, t, s = le, n = !1) {
  if (s) {
    const r = s[e] || (s[e] = []), i = t.__weh || (t.__weh = (...o) => {
      He();
      const l = Lt(s), c = Ce(t, s, e, o);
      return l(), Ve(), c;
    });
    return n ? r.unshift(i) : r.push(i), i;
  }
}
const Je = (e) => (t, s = le) => {
  (!Nt || e === "sp") && hs(e, (...n) => t(...n), s);
}, nl = Je("bm"), Mr = Je("m"), rl = Je(
  "bu"
), il = Je("u"), ll = Je(
  "bum"
), rn = Je("um"), ol = Je(
  "sp"
), fl = Je("rtg"), cl = Je("rtc");
function ul(e, t = le) {
  hs("ec", e, t);
}
const al = /* @__PURE__ */ Symbol.for("v-ndc");
function xn(e, t, s, n) {
  let r;
  const i = s, o = M(e);
  if (o || Y(e)) {
    const l = o && /* @__PURE__ */ it(e);
    let c = !1, d = !1;
    l && (c = !/* @__PURE__ */ me(e), d = /* @__PURE__ */ ke(e), e = as(e)), r = new Array(e.length);
    for (let a = 0, p = e.length; a < p; a++)
      r[a] = t(
        c ? d ? pt(we(e[a])) : we(e[a]) : e[a],
        a,
        void 0,
        i
      );
  } else if (typeof e == "number") {
    r = new Array(e);
    for (let l = 0; l < e; l++)
      r[l] = t(l + 1, l, void 0, i);
  } else if ($(e))
    if (e[Symbol.iterator])
      r = Array.from(
        e,
        (l, c) => t(l, c, void 0, i)
      );
    else {
      const l = Object.keys(e);
      r = new Array(l.length);
      for (let c = 0, d = l.length; c < d; c++) {
        const a = l[c];
        r[c] = t(e[a], a, c, i);
      }
    }
  else
    r = [];
  return r;
}
const Hs = (e) => e ? Xr(e) ? _s(e) : Hs(e.parent) : null, Pt = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ ee(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => e.props,
    $attrs: (e) => e.attrs,
    $slots: (e) => e.slots,
    $refs: (e) => e.refs,
    $parent: (e) => Hs(e.parent),
    $root: (e) => Hs(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Rr(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      sn(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = Bi.bind(e.proxy)),
    $watch: (e) => zi.bind(e)
  })
), Os = (e, t) => e !== K && !e.__isScriptSetup && H(e, t), dl = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: r, props: i, accessCache: o, type: l, appContext: c } = e;
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
        if (Os(n, t))
          return o[t] = 1, n[t];
        if (r !== K && H(r, t))
          return o[t] = 2, r[t];
        if (H(i, t))
          return o[t] = 3, i[t];
        if (s !== K && H(s, t))
          return o[t] = 4, s[t];
        Vs && (o[t] = 0);
      }
    }
    const d = Pt[t];
    let a, p;
    if (d)
      return t === "$attrs" && te(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== K && H(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      p = c.config.globalProperties, H(p, t)
    )
      return p[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: r, ctx: i } = e;
    return Os(r, t) ? (r[t] = s, !0) : n !== K && H(n, t) ? (n[t] = s, !0) : H(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (i[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: r, props: i, type: o }
  }, l) {
    let c;
    return !!(s[l] || e !== K && l[0] !== "$" && H(e, l) || Os(t, l) || H(i, l) || H(n, l) || H(Pt, l) || H(r.config.globalProperties, l) || (c = o.__cssModules) && c[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : H(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function Sn(e) {
  return M(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let Vs = !0;
function hl(e) {
  const t = Rr(e), s = e.proxy, n = e.ctx;
  Vs = !1, t.beforeCreate && wn(t.beforeCreate, e, "bc");
  const {
    // state
    data: r,
    computed: i,
    methods: o,
    watch: l,
    provide: c,
    inject: d,
    // lifecycle
    created: a,
    beforeMount: p,
    mounted: C,
    beforeUpdate: T,
    updated: j,
    activated: P,
    deactivated: J,
    beforeDestroy: B,
    beforeUnmount: O,
    destroyed: F,
    unmounted: _,
    render: L,
    renderTracked: fe,
    renderTriggered: ce,
    errorCaptured: be,
    serverPrefetch: ze,
    // public API
    expose: ye,
    inheritAttrs: _t,
    // assets
    components: Ut,
    directives: Kt,
    filters: ms
  } = t;
  if (d && pl(d, n, null), o)
    for (const q in o) {
      const W = o[q];
      R(W) && (n[q] = W.bind(s));
    }
  if (r) {
    const q = r.call(s, s);
    $(q) && (e.data = /* @__PURE__ */ Ft(q));
  }
  if (Vs = !0, i)
    for (const q in i) {
      const W = i[q], Xe = R(W) ? W.bind(s, s) : R(W.get) ? W.get.bind(s, s) : je, Bt = !R(W) && R(W.set) ? W.set.bind(s) : je, Ze = Zl({
        get: Xe,
        set: Bt
      });
      Object.defineProperty(n, q, {
        enumerable: !0,
        configurable: !0,
        get: () => Ze.value,
        set: (Te) => Ze.value = Te
      });
    }
  if (l)
    for (const q in l)
      Ir(l[q], n, s, q);
  if (c) {
    const q = R(c) ? c.call(s) : c;
    Reflect.ownKeys(q).forEach((W) => {
      qi(W, q[W]);
    });
  }
  a && wn(a, e, "c");
  function ne(q, W) {
    M(W) ? W.forEach((Xe) => q(Xe.bind(s))) : W && q(W.bind(s));
  }
  if (ne(nl, p), ne(Mr, C), ne(rl, T), ne(il, j), ne(el, P), ne(tl, J), ne(ul, be), ne(cl, fe), ne(fl, ce), ne(ll, O), ne(rn, _), ne(ol, ze), M(ye))
    if (ye.length) {
      const q = e.exposed || (e.exposed = {});
      ye.forEach((W) => {
        Object.defineProperty(q, W, {
          get: () => s[W],
          set: (Xe) => s[W] = Xe,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  L && e.render === je && (e.render = L), _t != null && (e.inheritAttrs = _t), Ut && (e.components = Ut), Kt && (e.directives = Kt), ze && Or(e);
}
function pl(e, t, s = je) {
  M(e) && (e = $s(e));
  for (const n in e) {
    const r = e[n];
    let i;
    $(r) ? "default" in r ? i = zt(
      r.from || n,
      r.default,
      !0
    ) : i = zt(r.from || n) : i = zt(r), /* @__PURE__ */ se(i) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => i.value,
      set: (o) => i.value = o
    }) : t[n] = i;
  }
}
function wn(e, t, s) {
  Ce(
    M(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Ir(e, t, s, n) {
  let r = n.includes(".") ? Er(s, n) : () => s[n];
  if (Y(e)) {
    const i = t[e];
    R(i) && Ts(r, i);
  } else if (R(e))
    Ts(r, e.bind(s));
  else if ($(e))
    if (M(e))
      e.forEach((i) => Ir(i, t, s, n));
    else {
      const i = R(e.handler) ? e.handler.bind(s) : t[e.handler];
      R(i) && Ts(r, i, e);
    }
}
function Rr(e) {
  const t = e.type, { mixins: s, extends: n } = t, {
    mixins: r,
    optionsCache: i,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = i.get(t);
  let c;
  return l ? c = l : !r.length && !s && !n ? c = t : (c = {}, r.length && r.forEach(
    (d) => ss(c, d, o, !0)
  ), ss(c, t, o)), $(t) && i.set(t, c), c;
}
function ss(e, t, s, n = !1) {
  const { mixins: r, extends: i } = t;
  i && ss(e, i, s, !0), r && r.forEach(
    (o) => ss(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = gl[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const gl = {
  data: Cn,
  props: Tn,
  emits: Tn,
  // objects
  methods: St,
  computed: St,
  // lifecycle
  beforeCreate: re,
  created: re,
  beforeMount: re,
  mounted: re,
  beforeUpdate: re,
  updated: re,
  beforeDestroy: re,
  beforeUnmount: re,
  destroyed: re,
  unmounted: re,
  activated: re,
  deactivated: re,
  errorCaptured: re,
  serverPrefetch: re,
  // assets
  components: St,
  directives: St,
  // watch
  watch: ml,
  // provide / inject
  provide: Cn,
  inject: _l
};
function Cn(e, t) {
  return t ? e ? function() {
    return ee(
      R(e) ? e.call(this, this) : e,
      R(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function _l(e, t) {
  return St($s(e), $s(t));
}
function $s(e) {
  if (M(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++)
      t[e[s]] = e[s];
    return t;
  }
  return e;
}
function re(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function St(e, t) {
  return e ? ee(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Tn(e, t) {
  return e ? M(e) && M(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : ee(
    /* @__PURE__ */ Object.create(null),
    Sn(e),
    Sn(t ?? {})
  ) : t;
}
function ml(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = ee(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = re(e[n], t[n]);
  return s;
}
function Fr() {
  return {
    app: null,
    config: {
      isNativeTag: Yn,
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
let bl = 0;
function yl(e, t) {
  return function(n, r = null) {
    R(n) || (n = ee({}, n)), r != null && !$(r) && (r = null);
    const i = Fr(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let c = !1;
    const d = i.app = {
      _uid: bl++,
      _component: n,
      _props: r,
      _container: null,
      _context: i,
      _instance: null,
      version: Ql,
      get config() {
        return i.config;
      },
      set config(a) {
      },
      use(a, ...p) {
        return o.has(a) || (a && R(a.install) ? (o.add(a), a.install(d, ...p)) : R(a) && (o.add(a), a(d, ...p))), d;
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
        if (!c) {
          const T = d._ceVNode || We(n, r);
          return T.appContext = i, C === !0 ? C = "svg" : C === !1 && (C = void 0), e(T, a, C), c = !0, d._container = a, a.__vue_app__ = d, _s(T.component);
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
        return i.provides[a] = p, d;
      },
      runWithContext(a) {
        const p = dt;
        dt = d;
        try {
          return a();
        } finally {
          dt = p;
        }
      }
    };
    return d;
  };
}
let dt = null;
const vl = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${xe(t)}Modifiers`] || e[`${lt(t)}Modifiers`];
function xl(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || K;
  let r = s;
  const i = t.startsWith("update:"), o = i && vl(n, t.slice(7));
  o && (o.trim && (r = s.map((a) => Y(a) ? a.trim() : a)), o.number && (r = s.map(Js)));
  let l, c = n[l = ys(t)] || // also try camelCase event handler (#2249)
  n[l = ys(xe(t))];
  !c && i && (c = n[l = ys(lt(t))]), c && Ce(
    c,
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
    e.emitted[l] = !0, Ce(
      d,
      e,
      6,
      r
    );
  }
}
const Sl = /* @__PURE__ */ new WeakMap();
function Dr(e, t, s = !1) {
  const n = s ? Sl : t.emitsCache, r = n.get(e);
  if (r !== void 0)
    return r;
  const i = e.emits;
  let o = {}, l = !1;
  if (!R(e)) {
    const c = (d) => {
      const a = Dr(d, t, !0);
      a && (l = !0, ee(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(c), e.extends && c(e.extends), e.mixins && e.mixins.forEach(c);
  }
  return !i && !l ? ($(e) && n.set(e, null), null) : (M(i) ? i.forEach((c) => o[c] = null) : ee(o, i), $(e) && n.set(e, o), o);
}
function ps(e, t) {
  return !e || !ls(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), H(e, t[0].toLowerCase() + t.slice(1)) || H(e, lt(t)) || H(e, t));
}
function En(e) {
  const {
    type: t,
    vnode: s,
    proxy: n,
    withProxy: r,
    propsOptions: [i],
    slots: o,
    attrs: l,
    emit: c,
    render: d,
    renderCache: a,
    props: p,
    data: C,
    setupState: T,
    ctx: j,
    inheritAttrs: P
  } = e, J = es(e);
  let B, O;
  try {
    if (s.shapeFlag & 4) {
      const _ = r || n, L = _;
      B = Fe(
        d.call(
          L,
          _,
          a,
          p,
          T,
          C,
          j
        )
      ), O = l;
    } else {
      const _ = t;
      B = Fe(
        _.length > 1 ? _(
          p,
          { attrs: l, slots: o, emit: c }
        ) : _(
          p,
          null
        )
      ), O = t.props ? l : wl(l);
    }
  } catch (_) {
    Mt.length = 0, ds(_, e, 1), B = We(Ye);
  }
  let F = B;
  if (O && P !== !1) {
    const _ = Object.keys(O), { shapeFlag: L } = F;
    _.length && L & 7 && (i && _.some(os) && (O = Cl(
      O,
      i
    )), F = gt(F, O, !1, !0));
  }
  return s.dirs && (F = gt(F, null, !1, !0), F.dirs = F.dirs ? F.dirs.concat(s.dirs) : s.dirs), s.transition && nn(F, s.transition), B = F, es(J), B;
}
const wl = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || ls(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, Cl = (e, t) => {
  const s = {};
  for (const n in e)
    (!os(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function Tl(e, t, s) {
  const { props: n, children: r, component: i } = e, { props: o, children: l, patchFlag: c } = t, d = i.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (s && c >= 0) {
    if (c & 1024)
      return !0;
    if (c & 16)
      return n ? On(n, o, d) : !!o;
    if (c & 8) {
      const a = t.dynamicProps;
      for (let p = 0; p < a.length; p++) {
        const C = a[p];
        if (jr(o, n, C) && !ps(d, C))
          return !0;
      }
    }
  } else
    return (r || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? On(n, o, d) : !0 : !!o;
  return !1;
}
function On(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let r = 0; r < n.length; r++) {
    const i = n[r];
    if (jr(t, e, i) && !ps(s, i))
      return !0;
  }
  return !1;
}
function jr(e, t, s) {
  const n = e[s], r = t[s];
  return s === "style" && $(n) && $(r) ? !Vt(n, r) : n !== r;
}
function El({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const r = t.subTree;
    if (r.suspense && r.suspense.activeBranch === e && (r.suspense.vnode.el = r.el = n, e = r), r === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const Nr = {}, Hr = () => Object.create(Nr), Vr = (e) => Object.getPrototypeOf(e) === Nr;
function Ol(e, t, s, n = !1) {
  const r = {}, i = Hr();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), $r(e, t, r, i);
  for (const o in e.propsOptions[0])
    o in r || (r[o] = void 0);
  s ? e.props = n ? r : /* @__PURE__ */ Fi(r) : e.type.props ? e.props = r : e.props = i, e.attrs = i;
}
function Al(e, t, s, n) {
  const {
    props: r,
    attrs: i,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ N(r), [c] = e.propsOptions;
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
        const T = t[C];
        if (c)
          if (H(i, C))
            T !== i[C] && (i[C] = T, d = !0);
          else {
            const j = xe(C);
            r[j] = Ls(
              c,
              l,
              j,
              T,
              e,
              !1
            );
          }
        else
          T !== i[C] && (i[C] = T, d = !0);
      }
    }
  } else {
    $r(e, t, r, i) && (d = !0);
    let a;
    for (const p in l)
      (!t || // for camelCase
      !H(t, p) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = lt(p)) === p || !H(t, a))) && (c ? s && // for camelCase
      (s[p] !== void 0 || // for kebab-case
      s[a] !== void 0) && (r[p] = Ls(
        c,
        l,
        p,
        void 0,
        e,
        !0
      )) : delete r[p]);
    if (i !== l)
      for (const p in i)
        (!t || !H(t, p)) && (delete i[p], d = !0);
  }
  d && Ke(e.attrs, "set", "");
}
function $r(e, t, s, n) {
  const [r, i] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let c in t) {
      if (Ct(c))
        continue;
      const d = t[c];
      let a;
      r && H(r, a = xe(c)) ? !i || !i.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : ps(e.emitsOptions, c) || (!(c in n) || d !== n[c]) && (n[c] = d, o = !0);
    }
  if (i) {
    const c = /* @__PURE__ */ N(s), d = l || K;
    for (let a = 0; a < i.length; a++) {
      const p = i[a];
      s[p] = Ls(
        r,
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
function Ls(e, t, s, n, r, i) {
  const o = e[s];
  if (o != null) {
    const l = H(o, "default");
    if (l && n === void 0) {
      const c = o.default;
      if (o.type !== Function && !o.skipFactory && R(c)) {
        const { propsDefaults: d } = r;
        if (s in d)
          n = d[s];
        else {
          const a = Lt(r);
          n = d[s] = c.call(
            null,
            t
          ), a();
        }
      } else
        n = c;
      r.ce && r.ce._setProp(s, n);
    }
    o[
      0
      /* shouldCast */
    ] && (i && !l ? n = !1 : o[
      1
      /* shouldCastTrue */
    ] && (n === "" || n === lt(s)) && (n = !0));
  }
  return n;
}
const Pl = /* @__PURE__ */ new WeakMap();
function Lr(e, t, s = !1) {
  const n = s ? Pl : t.propsCache, r = n.get(e);
  if (r)
    return r;
  const i = e.props, o = {}, l = [];
  let c = !1;
  if (!R(e)) {
    const a = (p) => {
      c = !0;
      const [C, T] = Lr(p, t, !0);
      ee(o, C), T && l.push(...T);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!i && !c)
    return $(e) && n.set(e, ct), ct;
  if (M(i))
    for (let a = 0; a < i.length; a++) {
      const p = xe(i[a]);
      An(p) && (o[p] = K);
    }
  else if (i)
    for (const a in i) {
      const p = xe(a);
      if (An(p)) {
        const C = i[a], T = o[p] = M(C) || R(C) ? { type: C } : ee({}, C), j = T.type;
        let P = !1, J = !0;
        if (M(j))
          for (let B = 0; B < j.length; ++B) {
            const O = j[B], F = R(O) && O.name;
            if (F === "Boolean") {
              P = !0;
              break;
            } else F === "String" && (J = !1);
          }
        else
          P = R(j) && j.name === "Boolean";
        T[
          0
          /* shouldCast */
        ] = P, T[
          1
          /* shouldCastTrue */
        ] = J, (P || H(T, "default")) && l.push(p);
      }
    }
  const d = [o, l];
  return $(e) && n.set(e, d), d;
}
function An(e) {
  return e[0] !== "$" && !Ct(e);
}
const ln = (e) => e === "_" || e === "_ctx" || e === "$stable", on = (e) => M(e) ? e.map(Fe) : [Fe(e)], Ml = (e, t, s) => {
  if (t._n)
    return t;
  const n = Ji((...r) => on(t(...r)), s);
  return n._c = !1, n;
}, Ur = (e, t, s) => {
  const n = e._ctx;
  for (const r in e) {
    if (ln(r)) continue;
    const i = e[r];
    if (R(i))
      t[r] = Ml(r, i, n);
    else if (i != null) {
      const o = on(i);
      t[r] = () => o;
    }
  }
}, Kr = (e, t) => {
  const s = on(t);
  e.slots.default = () => s;
}, Br = (e, t, s) => {
  for (const n in t)
    (s || !ln(n)) && (e[n] = t[n]);
}, Il = (e, t, s) => {
  const n = e.slots = Hr();
  if (e.vnode.shapeFlag & 32) {
    const r = t._;
    r ? (Br(n, t, s), s && er(n, "_", r, !0)) : Ur(t, n);
  } else t && Kr(e, t);
}, Rl = (e, t, s) => {
  const { vnode: n, slots: r } = e;
  let i = !0, o = K;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? i = !1 : Br(r, t, s) : (i = !t.$stable, Ur(t, r)), o = t;
  } else t && (Kr(e, t), o = { default: 1 });
  if (i)
    for (const l in r)
      !ln(l) && o[l] == null && delete r[l];
}, oe = Hl;
function Fl(e) {
  return Dl(e);
}
function Dl(e, t) {
  const s = us();
  s.__VUE__ = !0;
  const {
    insert: n,
    remove: r,
    patchProp: i,
    createElement: o,
    createText: l,
    createComment: c,
    setText: d,
    setElementText: a,
    parentNode: p,
    nextSibling: C,
    setScopeId: T = je,
    insertStaticContent: j
  } = e, P = (f, u, h, y = null, b = null, g = null, S = void 0, x = null, v = !!u.dynamicChildren) => {
    if (f === u)
      return;
    f && !xt(f, u) && (y = Wt(f), Te(f, b, g, !0), f = null), u.patchFlag === -2 && (v = !1, u.dynamicChildren = null);
    const { type: m, ref: A, shapeFlag: w } = u;
    switch (m) {
      case gs:
        J(f, u, h, y);
        break;
      case Ye:
        B(f, u, h, y);
        break;
      case Ps:
        f == null && O(u, h, y, S);
        break;
      case de:
        Ut(
          f,
          u,
          h,
          y,
          b,
          g,
          S,
          x,
          v
        );
        break;
      default:
        w & 1 ? L(
          f,
          u,
          h,
          y,
          b,
          g,
          S,
          x,
          v
        ) : w & 6 ? Kt(
          f,
          u,
          h,
          y,
          b,
          g,
          S,
          x,
          v
        ) : (w & 64 || w & 128) && m.process(
          f,
          u,
          h,
          y,
          b,
          g,
          S,
          x,
          v,
          bt
        );
    }
    A != null && b ? Ot(A, f && f.ref, g, u || f, !u) : A == null && f && f.ref != null && Ot(f.ref, null, g, f, !0);
  }, J = (f, u, h, y) => {
    if (f == null)
      n(
        u.el = l(u.children),
        h,
        y
      );
    else {
      const b = u.el = f.el;
      u.children !== f.children && d(b, u.children);
    }
  }, B = (f, u, h, y) => {
    f == null ? n(
      u.el = c(u.children || ""),
      h,
      y
    ) : u.el = f.el;
  }, O = (f, u, h, y) => {
    [f.el, f.anchor] = j(
      f.children,
      u,
      h,
      y,
      f.el,
      f.anchor
    );
  }, F = ({ el: f, anchor: u }, h, y) => {
    let b;
    for (; f && f !== u; )
      b = C(f), n(f, h, y), f = b;
    n(u, h, y);
  }, _ = ({ el: f, anchor: u }) => {
    let h;
    for (; f && f !== u; )
      h = C(f), r(f), f = h;
    r(u);
  }, L = (f, u, h, y, b, g, S, x, v) => {
    if (u.type === "svg" ? S = "svg" : u.type === "math" && (S = "mathml"), f == null)
      fe(
        u,
        h,
        y,
        b,
        g,
        S,
        x,
        v
      );
    else {
      const m = f.el && f.el._isVueCE ? f.el : null;
      try {
        m && m._beginPatch(), ze(
          f,
          u,
          b,
          g,
          S,
          x,
          v
        );
      } finally {
        m && m._endPatch();
      }
    }
  }, fe = (f, u, h, y, b, g, S, x) => {
    let v, m;
    const { props: A, shapeFlag: w, transition: E, dirs: I } = f;
    if (v = f.el = o(
      f.type,
      g,
      A && A.is,
      A
    ), w & 8 ? a(v, f.children) : w & 16 && be(
      f.children,
      v,
      null,
      y,
      b,
      As(f, g),
      S,
      x
    ), I && Qe(f, null, y, "created"), ce(v, f, f.scopeId, S, y), A) {
      for (const U in A)
        U !== "value" && !Ct(U) && i(v, U, null, A[U], g, y);
      "value" in A && i(v, "value", null, A.value, g), (m = A.onVnodeBeforeMount) && Pe(m, y, f);
    }
    I && Qe(f, null, y, "beforeMount");
    const D = jl(b, E);
    D && E.beforeEnter(v), n(v, u, h), ((m = A && A.onVnodeMounted) || D || I) && oe(() => {
      try {
        m && Pe(m, y, f), D && E.enter(v), I && Qe(f, null, y, "mounted");
      } finally {
      }
    }, b);
  }, ce = (f, u, h, y, b) => {
    if (h && T(f, h), y)
      for (let g = 0; g < y.length; g++)
        T(f, y[g]);
    if (b) {
      let g = b.subTree;
      if (u === g || qr(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const S = b.vnode;
        ce(
          f,
          S,
          S.scopeId,
          S.slotScopeIds,
          b.parent
        );
      }
    }
  }, be = (f, u, h, y, b, g, S, x, v = 0) => {
    for (let m = v; m < f.length; m++) {
      const A = f[m] = x ? Ue(f[m]) : Fe(f[m]);
      P(
        null,
        A,
        u,
        h,
        y,
        b,
        g,
        S,
        x
      );
    }
  }, ze = (f, u, h, y, b, g, S) => {
    const x = u.el = f.el;
    let { patchFlag: v, dynamicChildren: m, dirs: A } = u;
    v |= f.patchFlag & 16;
    const w = f.props || K, E = u.props || K;
    let I;
    if (h && et(h, !1), (I = E.onVnodeBeforeUpdate) && Pe(I, h, u, f), A && Qe(u, f, h, "beforeUpdate"), h && et(h, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    m && (!f.dynamicChildren || f.dynamicChildren.length !== m.length) && (v = 0, S = !1, m = null), (w.innerHTML && E.innerHTML == null || w.textContent && E.textContent == null) && a(x, ""), m ? ye(
      f.dynamicChildren,
      m,
      x,
      h,
      y,
      As(u, b),
      g
    ) : S || W(
      f,
      u,
      x,
      null,
      h,
      y,
      As(u, b),
      g,
      !1
    ), v > 0) {
      if (v & 16)
        _t(x, w, E, h, b);
      else if (v & 2 && w.class !== E.class && i(x, "class", null, E.class, b), v & 4 && i(x, "style", w.style, E.style, b), v & 8) {
        const D = u.dynamicProps;
        for (let U = 0; U < D.length; U++) {
          const V = D[U], z = w[V], Z = E[V];
          (Z !== z || V === "value") && i(x, V, z, Z, b, h);
        }
      }
      v & 1 && f.children !== u.children && a(x, u.children);
    } else !S && m == null && _t(x, w, E, h, b);
    ((I = E.onVnodeUpdated) || A) && oe(() => {
      I && Pe(I, h, u, f), A && Qe(u, f, h, "updated");
    }, y);
  }, ye = (f, u, h, y, b, g, S) => {
    for (let x = 0; x < u.length; x++) {
      const v = f[x], m = u[x], A = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        v.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (v.type === de || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !xt(v, m) || // - In the case of a component, it could contain anything.
        v.shapeFlag & 198) ? p(v.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          h
        )
      );
      P(
        v,
        m,
        A,
        null,
        y,
        b,
        g,
        S,
        !0
      );
    }
  }, _t = (f, u, h, y, b) => {
    if (u !== h) {
      if (u !== K)
        for (const g in u)
          !Ct(g) && !(g in h) && i(
            f,
            g,
            u[g],
            null,
            b,
            y
          );
      for (const g in h) {
        if (Ct(g)) continue;
        const S = h[g], x = u[g];
        S !== x && g !== "value" && i(f, g, x, S, b, y);
      }
      "value" in h && i(f, "value", u.value, h.value, b);
    }
  }, Ut = (f, u, h, y, b, g, S, x, v) => {
    const m = u.el = f ? f.el : l(""), A = u.anchor = f ? f.anchor : l("");
    let { patchFlag: w, dynamicChildren: E, slotScopeIds: I } = u;
    I && (x = x ? x.concat(I) : I), f == null ? (n(m, h, y), n(A, h, y), be(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      h,
      A,
      b,
      g,
      S,
      x,
      v
    )) : w > 0 && w & 64 && E && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    f.dynamicChildren && f.dynamicChildren.length === E.length ? (ye(
      f.dynamicChildren,
      E,
      h,
      b,
      g,
      S,
      x
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || b && u === b.subTree) && Wr(
      f,
      u,
      !0
      /* shallow */
    )) : W(
      f,
      u,
      h,
      A,
      b,
      g,
      S,
      x,
      v
    );
  }, Kt = (f, u, h, y, b, g, S, x, v) => {
    u.slotScopeIds = x, f == null ? u.shapeFlag & 512 ? b.ctx.activate(
      u,
      h,
      y,
      S,
      v
    ) : ms(
      u,
      h,
      y,
      b,
      g,
      S,
      v
    ) : fn(f, u, v);
  }, ms = (f, u, h, y, b, g, S) => {
    const x = f.component = kl(
      f,
      y,
      b
    );
    if (Ar(f) && (x.ctx.renderer = bt), ql(x, !1, S), x.asyncDep) {
      if (b && b.registerDep(x, ne, S), !f.el) {
        const v = x.subTree = We(Ye);
        B(null, v, u, h), f.placeholder = v.el;
      }
    } else
      ne(
        x,
        f,
        u,
        h,
        b,
        g,
        S
      );
  }, fn = (f, u, h) => {
    const y = u.component = f.component;
    if (Tl(f, u, h))
      if (y.asyncDep && !y.asyncResolved) {
        q(y, u, h);
        return;
      } else
        y.next = u, y.update();
    else
      u.el = f.el, y.vnode = u;
  }, ne = (f, u, h, y, b, g, S) => {
    const x = () => {
      if (f.isMounted) {
        let { next: w, bu: E, u: I, parent: D, vnode: U } = f;
        {
          const Oe = kr(f);
          if (Oe) {
            w && (w.el = U.el, q(f, w, S)), Oe.asyncDep.then(() => {
              oe(() => {
                f.isUnmounted || m();
              }, b);
            });
            return;
          }
        }
        let V = w, z;
        et(f, !1), w ? (w.el = U.el, q(f, w, S)) : w = U, E && Yt(E), (z = w.props && w.props.onVnodeBeforeUpdate) && Pe(z, D, w, U), et(f, !0);
        const Z = En(f), Ee = f.subTree;
        f.subTree = Z, P(
          Ee,
          Z,
          // parent may have changed if it's in a teleport
          p(Ee.el),
          // anchor may have changed if it's in a fragment
          Wt(Ee),
          f,
          b,
          g
        ), w.el = Z.el, V === null && El(f, Z.el), I && oe(I, b), (z = w.props && w.props.onVnodeUpdated) && oe(
          () => Pe(z, D, w, U),
          b
        );
      } else {
        let w;
        const { el: E, props: I } = u, { bm: D, m: U, parent: V, root: z, type: Z } = f, Ee = At(u);
        et(f, !1), D && Yt(D), !Ee && (w = I && I.onVnodeBeforeMount) && Pe(w, V, u), et(f, !0);
        {
          z.ce && z.ce._hasShadowRoot() && z.ce._injectChildStyle(
            Z,
            f.parent ? f.parent.type : void 0
          );
          const Oe = f.subTree = En(f);
          P(
            null,
            Oe,
            h,
            y,
            f,
            b,
            g
          ), u.el = Oe.el;
        }
        if (U && oe(U, b), !Ee && (w = I && I.onVnodeMounted)) {
          const Oe = u;
          oe(
            () => Pe(w, V, Oe),
            b
          );
        }
        (u.shapeFlag & 256 || V && At(V.vnode) && V.vnode.shapeFlag & 256) && f.a && oe(f.a, b), f.isMounted = !0, u = h = y = null;
      }
    };
    f.scope.on();
    const v = f.effect = new ir(x);
    f.scope.off();
    const m = f.update = v.run.bind(v), A = f.job = v.runIfDirty.bind(v);
    A.i = f, A.id = f.uid, v.scheduler = () => sn(A), et(f, !0), m();
  }, q = (f, u, h) => {
    u.component = f;
    const y = f.vnode.props;
    f.vnode = u, f.next = null, Al(f, u.props, y, h), Rl(f, u.children, h), He(), bn(f), Ve();
  }, W = (f, u, h, y, b, g, S, x, v = !1) => {
    const m = f && f.children, A = f ? f.shapeFlag : 0, w = u.children, { patchFlag: E, shapeFlag: I } = u;
    if (E > 0) {
      if (E & 128) {
        Bt(
          m,
          w,
          h,
          y,
          b,
          g,
          S,
          x,
          v
        );
        return;
      } else if (E & 256) {
        Xe(
          m,
          w,
          h,
          y,
          b,
          g,
          S,
          x,
          v
        );
        return;
      }
    }
    I & 8 ? (A & 16 && mt(m, b, g), w !== m && a(h, w)) : A & 16 ? I & 16 ? Bt(
      m,
      w,
      h,
      y,
      b,
      g,
      S,
      x,
      v
    ) : mt(m, b, g, !0) : (A & 8 && a(h, ""), I & 16 && be(
      w,
      h,
      y,
      b,
      g,
      S,
      x,
      v
    ));
  }, Xe = (f, u, h, y, b, g, S, x, v) => {
    f = f || ct, u = u || ct;
    const m = f.length, A = u.length, w = Math.min(m, A);
    let E;
    for (E = 0; E < w; E++) {
      const I = u[E] = v ? Ue(u[E]) : Fe(u[E]);
      P(
        f[E],
        I,
        h,
        null,
        b,
        g,
        S,
        x,
        v
      );
    }
    m > A ? mt(
      f,
      b,
      g,
      !0,
      !1,
      w
    ) : be(
      u,
      h,
      y,
      b,
      g,
      S,
      x,
      v,
      w
    );
  }, Bt = (f, u, h, y, b, g, S, x, v) => {
    let m = 0;
    const A = u.length;
    let w = f.length - 1, E = A - 1;
    for (; m <= w && m <= E; ) {
      const I = f[m], D = u[m] = v ? Ue(u[m]) : Fe(u[m]);
      if (xt(I, D))
        P(
          I,
          D,
          h,
          null,
          b,
          g,
          S,
          x,
          v
        );
      else
        break;
      m++;
    }
    for (; m <= w && m <= E; ) {
      const I = f[w], D = u[E] = v ? Ue(u[E]) : Fe(u[E]);
      if (xt(I, D))
        P(
          I,
          D,
          h,
          null,
          b,
          g,
          S,
          x,
          v
        );
      else
        break;
      w--, E--;
    }
    if (m > w) {
      if (m <= E) {
        const I = E + 1, D = I < A ? u[I].el : y;
        for (; m <= E; )
          P(
            null,
            u[m] = v ? Ue(u[m]) : Fe(u[m]),
            h,
            D,
            b,
            g,
            S,
            x,
            v
          ), m++;
      }
    } else if (m > E)
      for (; m <= w; )
        Te(f[m], b, g, !0), m++;
    else {
      const I = m, D = m, U = /* @__PURE__ */ new Map();
      for (m = D; m <= E; m++) {
        const ue = u[m] = v ? Ue(u[m]) : Fe(u[m]);
        ue.key != null && U.set(ue.key, m);
      }
      let V, z = 0;
      const Z = E - D + 1;
      let Ee = !1, Oe = 0;
      const yt = new Array(Z);
      for (m = 0; m < Z; m++) yt[m] = 0;
      for (m = I; m <= w; m++) {
        const ue = f[m];
        if (z >= Z) {
          Te(ue, b, g, !0);
          continue;
        }
        let Ae;
        if (ue.key != null)
          Ae = U.get(ue.key);
        else
          for (V = D; V <= E; V++)
            if (yt[V - D] === 0 && xt(ue, u[V])) {
              Ae = V;
              break;
            }
        Ae === void 0 ? Te(ue, b, g, !0) : (yt[Ae - D] = m + 1, Ae >= Oe ? Oe = Ae : Ee = !0, P(
          ue,
          u[Ae],
          h,
          null,
          b,
          g,
          S,
          x,
          v
        ), z++);
      }
      const an = Ee ? Nl(yt) : ct;
      for (V = an.length - 1, m = Z - 1; m >= 0; m--) {
        const ue = D + m, Ae = u[ue], dn = u[ue + 1], hn = ue + 1 < A ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          dn.el || Jr(dn)
        ) : y;
        yt[m] === 0 ? P(
          null,
          Ae,
          h,
          hn,
          b,
          g,
          S,
          x,
          v
        ) : Ee && (V < 0 || m !== an[V] ? Ze(Ae, h, hn, 2) : V--);
      }
    }
  }, Ze = (f, u, h, y, b = null) => {
    const { el: g, type: S, transition: x, children: v, shapeFlag: m } = f;
    if (m & 6) {
      Ze(f.component.subTree, u, h, y);
      return;
    }
    if (m & 128) {
      f.suspense.move(u, h, y);
      return;
    }
    if (m & 64) {
      S.move(f, u, h, bt);
      return;
    }
    if (S === de) {
      n(g, u, h);
      for (let w = 0; w < v.length; w++)
        Ze(v[w], u, h, y);
      n(f.anchor, u, h);
      return;
    }
    if (S === Ps) {
      F(f, u, h);
      return;
    }
    if (y !== 2 && m & 1 && x)
      if (y === 0)
        x.persisted && !g[Es] ? n(g, u, h) : (x.beforeEnter(g), n(g, u, h), oe(() => x.enter(g), b));
      else {
        const { leave: w, delayLeave: E, afterLeave: I } = x, D = () => {
          f.ctx.isUnmounted ? r(g) : n(g, u, h);
        }, U = () => {
          const V = g._isLeaving || !!g[Es];
          g._isLeaving && g[Es](
            !0
            /* cancelled */
          ), x.persisted && !V ? D() : w(g, () => {
            D(), I && I();
          });
        };
        E ? E(g, D, U) : U();
      }
    else
      n(g, u, h);
  }, Te = (f, u, h, y = !1, b = !1) => {
    const {
      type: g,
      props: S,
      ref: x,
      children: v,
      dynamicChildren: m,
      shapeFlag: A,
      patchFlag: w,
      dirs: E,
      cacheIndex: I,
      memo: D
    } = f;
    if (w === -2 && (b = !1), x != null && (He(), Ot(x, null, h, f, !0), Ve()), I != null && (u.renderCache[I] = void 0), A & 256) {
      u.ctx.deactivate(f);
      return;
    }
    const U = A & 1 && E, V = !At(f);
    let z;
    if (V && (z = S && S.onVnodeBeforeUnmount) && Pe(z, u, f), A & 6)
      si(f.component, h, y);
    else {
      if (A & 128) {
        f.suspense.unmount(h, y);
        return;
      }
      U && Qe(f, null, u, "beforeUnmount"), A & 64 ? f.type.remove(
        f,
        u,
        h,
        bt,
        y
      ) : m && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !m.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== de || w > 0 && w & 64) ? mt(
        m,
        u,
        h,
        !1,
        !0
      ) : (g === de && w & 384 || !b && A & 16) && mt(v, u, h), y && cn(f);
    }
    const Z = D != null && I == null;
    (V && (z = S && S.onVnodeUnmounted) || U || Z) && oe(() => {
      z && Pe(z, u, f), U && Qe(f, null, u, "unmounted"), Z && (f.el = null);
    }, h);
  }, cn = (f) => {
    const { type: u, el: h, anchor: y, transition: b } = f;
    if (u === de) {
      ti(h, y);
      return;
    }
    if (u === Ps) {
      _(f);
      return;
    }
    const g = () => {
      r(h), b && !b.persisted && b.afterLeave && b.afterLeave();
    };
    if (f.shapeFlag & 1 && b && !b.persisted) {
      const { leave: S, delayLeave: x } = b, v = () => S(h, g);
      x ? x(f.el, g, v) : v();
    } else
      g();
  }, ti = (f, u) => {
    let h;
    for (; f !== u; )
      h = C(f), r(f), f = h;
    r(u);
  }, si = (f, u, h) => {
    const { bum: y, scope: b, job: g, subTree: S, um: x, m: v, a: m } = f;
    Pn(v), Pn(m), y && Yt(y), b.stop(), g && (g.flags |= 8, Te(S, f, u, h)), x && oe(x, u), oe(() => {
      f.isUnmounted = !0;
    }, u);
  }, mt = (f, u, h, y = !1, b = !1, g = 0) => {
    for (let S = g; S < f.length; S++)
      Te(f[S], u, h, y, b);
  }, Wt = (f) => {
    if (f.shapeFlag & 6)
      return Wt(f.component.subTree);
    if (f.shapeFlag & 128)
      return f.suspense.next();
    const u = C(f.anchor || f.el), h = u && u[Xi];
    return h ? C(h) : u;
  };
  let bs = !1;
  const un = (f, u, h) => {
    let y;
    f == null ? u._vnode && (Te(u._vnode, null, null, !0), y = u._vnode.component) : P(
      u._vnode || null,
      f,
      u,
      null,
      null,
      null,
      h
    ), u._vnode = f, bs || (bs = !0, bn(y), Sr(), bs = !1);
  }, bt = {
    p: P,
    um: Te,
    m: Ze,
    r: cn,
    mt: ms,
    mc: be,
    pc: W,
    pbc: ye,
    n: Wt,
    o: e
  };
  return {
    render: un,
    hydrate: void 0,
    createApp: yl(un)
  };
}
function As({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function et({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function jl(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Wr(e, t, s = !1) {
  const n = e.children, r = t.children;
  if (M(n) && M(r))
    for (let i = 0; i < n.length; i++) {
      const o = n[i];
      let l = r[i];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = r[i] = Ue(r[i]), l.el = o.el), !s && l.patchFlag !== -2 && Wr(o, l)), l.type === gs && (l.patchFlag === -1 && (l = r[i] = Ue(l)), l.el = o.el), l.type === Ye && !l.el && (l.el = o.el);
    }
}
function Nl(e) {
  const t = e.slice(), s = [0];
  let n, r, i, o, l;
  const c = e.length;
  for (n = 0; n < c; n++) {
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
function kr(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : kr(t);
}
function Pn(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function Jr(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? Jr(t.subTree) : null;
}
const qr = (e) => e.__isSuspense;
function Hl(e, t) {
  t && t.pendingBranch ? M(e) ? t.effects.push(...e) : t.effects.push(e) : ki(e);
}
const de = /* @__PURE__ */ Symbol.for("v-fgt"), gs = /* @__PURE__ */ Symbol.for("v-txt"), Ye = /* @__PURE__ */ Symbol.for("v-cmt"), Ps = /* @__PURE__ */ Symbol.for("v-stc"), Mt = [];
let he = null;
function ge(e = !1) {
  Mt.push(he = e ? null : []);
}
function Vl() {
  Mt.pop(), he = Mt[Mt.length - 1] || null;
}
let jt = 1;
function Mn(e, t = !1) {
  jt += e, e < 0 && he && t && (he.hasOnce = !0);
}
function Gr(e) {
  return e.dynamicChildren = jt > 0 ? he || ct : null, Vl(), jt > 0 && he && he.push(e), e;
}
function ve(e, t, s, n, r, i) {
  return Gr(
    X(
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
function $l(e, t, s, n, r) {
  return Gr(
    We(
      e,
      t,
      s,
      n,
      r,
      !0
    )
  );
}
function Yr(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function xt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const zr = ({ key: e }) => e ?? null, Xt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? Y(e) || /* @__PURE__ */ se(e) || R(e) ? { i: _e, r: e, k: t, f: !!s } : e : null);
function X(e, t = null, s = null, n = 0, r = null, i = e === de ? 0 : 1, o = !1, l = !1) {
  const c = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && zr(t),
    ref: t && Xt(t),
    scopeId: Cr,
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
    ctx: _e
  };
  return l ? (ns(c, s), i & 128 && e.normalize(c)) : s && (c.shapeFlag |= Y(s) ? 8 : 16), jt > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  he && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (c.patchFlag > 0 || i & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  c.patchFlag !== 32 && he.push(c), c;
}
const We = Ll;
function Ll(e, t = null, s = null, n = 0, r = null, i = !1) {
  if ((!e || e === al) && (e = Ye), Yr(e)) {
    const l = gt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ns(l, s), jt > 0 && !i && he && (l.shapeFlag & 6 ? he[he.indexOf(e)] = l : he.push(l)), l.patchFlag = -2, l;
  }
  if (Xl(e) && (e = e.__vccOpts), t) {
    t = Ul(t);
    let { class: l, style: c } = t;
    l && !Y(l) && (t.class = Gs(l)), $(c) && (/* @__PURE__ */ tn(c) && !M(c) && (c = ee({}, c)), t.style = qs(c));
  }
  const o = Y(e) ? 1 : qr(e) ? 128 : Zi(e) ? 64 : $(e) ? 4 : R(e) ? 2 : 0;
  return X(
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
function Ul(e) {
  return e ? /* @__PURE__ */ tn(e) || Vr(e) ? ee({}, e) : e : null;
}
function gt(e, t, s = !1, n = !1) {
  const { props: r, ref: i, patchFlag: o, children: l, transition: c } = e, d = t ? Kl(r || {}, t) : r, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && zr(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && i ? M(i) ? i.concat(Xt(t)) : [i, Xt(t)] : Xt(t)
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
    transition: c,
    // These should technically only be non-null on mounted VNodes. However,
    // they *should* be copied for kept-alive vnodes. So we just always copy
    // them since them being non-null during a mount doesn't affect the logic as
    // they will simply be overwritten.
    component: e.component,
    suspense: e.suspense,
    ssContent: e.ssContent && gt(e.ssContent),
    ssFallback: e.ssFallback && gt(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return c && n && nn(
    a,
    c.clone(a)
  ), a;
}
function Me(e = " ", t = 0) {
  return We(gs, null, e, t);
}
function qe(e = "", t = !1) {
  return t ? (ge(), $l(Ye, null, e)) : We(Ye, null, e);
}
function Fe(e) {
  return e == null || typeof e == "boolean" ? We(Ye) : M(e) ? We(
    de,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Yr(e) ? Ue(e) : We(gs, null, String(e));
}
function Ue(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : gt(e);
}
function ns(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (M(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const r = t.default;
      r && (r._c && (r._d = !1), ns(e, r()), r._c && (r._d = !0));
      return;
    } else {
      s = 32;
      const r = t._;
      !r && !Vr(t) ? t._ctx = _e : r === 3 && _e && (_e.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (R(t)) {
    if (n & 65) {
      ns(e, { default: t });
      return;
    }
    t = { default: t, _ctx: _e }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [Me(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function Kl(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const r in n)
      if (r === "class")
        t.class !== n.class && (t.class = Gs([t.class, n.class]));
      else if (r === "style")
        t.style = qs([t.style, n.style]);
      else if (ls(r)) {
        const i = t[r], o = n[r];
        o && i !== o && !(M(i) && i.includes(o)) ? t[r] = i ? [].concat(i, o) : o : o == null && i == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !os(r) && (t[r] = o);
      } else r !== "" && (t[r] = n[r]);
  }
  return t;
}
function Pe(e, t, s, n = null) {
  Ce(e, t, 7, [
    s,
    n
  ]);
}
const Bl = Fr();
let Wl = 0;
function kl(e, t, s) {
  const n = e.type, r = (t ? t.appContext : e.appContext) || Bl, i = {
    uid: Wl++,
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
    scope: new pi(
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
    propsOptions: Lr(n, r),
    emitsOptions: Dr(n, r),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: K,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: K,
    data: K,
    props: K,
    attrs: K,
    slots: K,
    refs: K,
    setupState: K,
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
  return i.ctx = { _: i }, i.root = t ? t.root : i, i.emit = xl.bind(null, i), e.ce && e.ce(i), i;
}
let le = null;
const Jl = () => le || _e;
let rs, Us;
{
  const e = us(), t = (s, n) => {
    let r;
    return (r = e[s]) || (r = e[s] = []), r.push(n), (i) => {
      r.length > 1 ? r.forEach((o) => o(i)) : r[0](i);
    };
  };
  rs = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => le = s
  ), Us = t(
    "__VUE_SSR_SETTERS__",
    (s) => Nt = s
  );
}
const Lt = (e) => {
  const t = le;
  return rs(e), e.scope.on(), () => {
    e.scope.off(), rs(t);
  };
}, In = () => {
  le && le.scope.off(), rs(null);
};
function Xr(e) {
  return e.vnode.shapeFlag & 4;
}
let Nt = !1;
function ql(e, t = !1, s = !1) {
  t && Us(t);
  const { props: n, children: r } = e.vnode, i = Xr(e);
  Ol(e, n, i, t), Il(e, r, s || t);
  const o = i ? Gl(e, t) : void 0;
  return t && Us(!1), o;
}
function Gl(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, dl);
  const { setup: n } = s;
  if (n) {
    He();
    const r = e.setupContext = n.length > 1 ? zl(e) : null, i = Lt(e), o = $t(
      n,
      e,
      0,
      [
        e.props,
        r
      ]
    ), l = zn(o);
    if (Ve(), i(), (l || e.sp) && !At(e) && Or(e), l) {
      if (o.then(In, In), t)
        return o.then((c) => {
          Rn(e, c);
        }).catch((c) => {
          ds(c, e, 0);
        });
      e.asyncDep = o;
    } else
      Rn(e, o);
  } else
    Zr(e);
}
function Rn(e, t, s) {
  R(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : $(t) && (e.setupState = yr(t)), Zr(e);
}
function Zr(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || je);
  {
    const r = Lt(e);
    He();
    try {
      hl(e);
    } finally {
      Ve(), r();
    }
  }
}
const Yl = {
  get(e, t) {
    return te(e, "get", ""), e[t];
  }
};
function zl(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, Yl),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function _s(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(yr(Di(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in Pt)
        return Pt[s](e);
    },
    has(t, s) {
      return s in t || s in Pt;
    }
  })) : e.proxy;
}
function Xl(e) {
  return R(e) && "__vccOpts" in e;
}
const Zl = (e, t) => /* @__PURE__ */ $i(e, t, Nt), Ql = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Ks;
const Fn = typeof window < "u" && window.trustedTypes;
if (Fn)
  try {
    Ks = /* @__PURE__ */ Fn.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Qr = Ks ? (e) => Ks.createHTML(e) : (e) => e, eo = "http://www.w3.org/2000/svg", to = "http://www.w3.org/1998/Math/MathML", Le = typeof document < "u" ? document : null, Dn = Le && /* @__PURE__ */ Le.createElement("template"), so = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const r = t === "svg" ? Le.createElementNS(eo, e) : t === "mathml" ? Le.createElementNS(to, e) : s ? Le.createElement(e, { is: s }) : Le.createElement(e);
    return e === "select" && n && n.multiple != null && r.setAttribute("multiple", n.multiple), r;
  },
  createText: (e) => Le.createTextNode(e),
  createComment: (e) => Le.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => Le.querySelector(e),
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
      Dn.innerHTML = Qr(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = Dn.content;
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
}, no = /* @__PURE__ */ Symbol("_vtc");
function ro(e, t, s) {
  const n = e[no];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const jn = /* @__PURE__ */ Symbol("_vod"), io = /* @__PURE__ */ Symbol("_vsh"), lo = /* @__PURE__ */ Symbol(""), oo = /(?:^|;)\s*display\s*:/;
function fo(e, t, s) {
  const n = e.style, r = Y(s);
  let i = !1;
  if (s && !r) {
    if (t)
      if (Y(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && wt(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && wt(n, o, "");
    for (const o in s) {
      o === "display" && (i = !0);
      const l = s[o];
      l != null ? uo(
        e,
        o,
        !Y(t) && t ? t[o] : void 0,
        l
      ) || wt(n, o, l) : wt(n, o, "");
    }
  } else if (r) {
    if (t !== s) {
      const o = n[lo];
      o && (s += ";" + o), n.cssText = s, i = oo.test(s);
    }
  } else t && e.removeAttribute("style");
  jn in e && (e[jn] = i ? n.display : "", e[io] && (n.display = "none"));
}
const Nn = /\s*!important$/;
function wt(e, t, s) {
  if (M(s))
    s.forEach((n) => wt(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = co(e, t);
    Nn.test(s) ? e.setProperty(
      lt(n),
      s.replace(Nn, ""),
      "important"
    ) : e[n] = s;
  }
}
const Hn = ["Webkit", "Moz", "ms"], Ms = {};
function co(e, t) {
  const s = Ms[t];
  if (s)
    return s;
  let n = xe(t);
  if (n !== "filter" && n in e)
    return Ms[t] = n;
  n = Qn(n);
  for (let r = 0; r < Hn.length; r++) {
    const i = Hn[r] + n;
    if (i in e)
      return Ms[t] = i;
  }
  return t;
}
function uo(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && Y(n) && s === n;
}
const Vn = "http://www.w3.org/1999/xlink";
function $n(e, t, s, n, r, i = di(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Vn, t.slice(6, t.length)) : e.setAttributeNS(Vn, t, s) : s == null || i && !tr(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    i ? "" : Ne(s) ? String(s) : s
  );
}
function Ln(e, t, s, n, r) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? Qr(s) : s);
    return;
  }
  const i = e.tagName;
  if (t === "value" && i !== "PROGRESS" && // custom elements may use _value internally
  !i.includes("-")) {
    const l = i === "OPTION" ? e.getAttribute("value") || "" : e.value, c = s == null ? (
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
    l === "boolean" ? s = tr(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(r || t);
}
function nt(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function ao(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const Un = /* @__PURE__ */ Symbol("_vei");
function ho(e, t, s, n, r = null) {
  const i = e[Un] || (e[Un] = {}), o = i[t];
  if (n && o)
    o.value = n;
  else {
    const [l, c] = _o(t);
    if (n) {
      const d = i[t] = yo(
        n,
        r
      );
      nt(e, l, d, c);
    } else o && (ao(e, l, o, c), i[t] = void 0);
  }
}
const po = /(Once|Passive|Capture)$/, go = /^on:?(?:Once|Passive|Capture)$/;
function _o(e) {
  let t, s;
  for (; (s = e.match(po)) && !go.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : lt(e.slice(2)), t];
}
let Is = 0;
const mo = /* @__PURE__ */ Promise.resolve(), bo = () => Is || (mo.then(() => Is = 0), Is = Date.now());
function yo(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const r = s.value;
    if (M(r)) {
      const i = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        i.call(n), n._stopped = !0;
      };
      const o = r.slice(), l = [n];
      for (let c = 0; c < o.length && !n._stopped; c++) {
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
        r,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = bo(), s;
}
const Kn = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, vo = (e, t, s, n, r, i) => {
  const o = r === "svg";
  t === "class" ? ro(e, n, o) : t === "style" ? fo(e, s, n) : ls(t) ? os(t) || ho(e, t, s, n, i) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : xo(e, t, n, o)) ? (Ln(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && $n(e, t, n, o, i, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (So(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !Y(n))) ? Ln(e, xe(t), n, i, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), $n(e, t, n, o));
};
function xo(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Kn(t) && R(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const r = e.tagName;
    if (r === "IMG" || r === "VIDEO" || r === "CANVAS" || r === "SOURCE")
      return !1;
  }
  return Kn(t) && Y(s) ? !1 : t in e;
}
function So(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = xe(t);
  return Array.isArray(s) ? s.some((r) => xe(r) === n) : Object.keys(s).some((r) => xe(r) === n);
}
const is = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return M(t) ? (s) => Yt(t, s) : t;
};
function wo(e) {
  e.target.composing = !0;
}
function Bn(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const ht = /* @__PURE__ */ Symbol("_assign");
function Wn(e, t, s) {
  return t && (e = e.trim()), s && (e = Js(e)), e;
}
const kn = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, r) {
    e[ht] = is(r);
    const i = n || r.props && r.props.type === "number";
    nt(e, t ? "change" : "input", (o) => {
      o.target.composing || e[ht](Wn(e.value, s, i));
    }), (s || i) && nt(e, "change", () => {
      e.value = Wn(e.value, s, i);
    }), t || (nt(e, "compositionstart", wo), nt(e, "compositionend", Bn), nt(e, "change", Bn));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: r, number: i } }, o) {
    if (e[ht] = is(o), e.composing) return;
    const l = (i || e.type === "number") && !/^0\d/.test(e.value) ? Js(e.value) : e.value, c = t ?? "";
    if (l === c)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (n && t === s || r && e.value.trim() === c) || (e.value = c);
  }
}, Jn = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, s) {
    e[ht] = is(s), nt(e, "change", () => {
      const n = e._modelValue, r = Co(e), i = e.checked, o = e[ht];
      if (M(n)) {
        const l = sr(n, r), c = l !== -1;
        if (i && !c)
          o(n.concat(r));
        else if (!i && c) {
          const d = [...n];
          d.splice(l, 1), o(d);
        }
      } else if (fs(n)) {
        const l = new Set(n);
        i ? l.add(r) : l.delete(r), o(l);
      } else
        o(ei(e, i));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: qn,
  beforeUpdate(e, t, s) {
    e[ht] = is(s), qn(e, t, s);
  }
};
function qn(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let r;
  if (M(t))
    r = sr(t, n.props.value) > -1;
  else if (fs(t))
    r = t.has(n.props.value);
  else {
    if (t === s) return;
    r = Vt(t, ei(e, !0));
  }
  e.checked !== r && (e.checked = r);
}
function Co(e) {
  return "_value" in e ? e._value : e.value;
}
function ei(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
}
const To = /* @__PURE__ */ ee({ patchProp: vo }, so);
let Gn;
function Eo() {
  return Gn || (Gn = Fl(To));
}
const Oo = ((...e) => {
  const t = Eo().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const r = Po(n);
    if (!r) return;
    const i = t._component;
    !R(i) && !i.render && !i.template && (i.template = r.innerHTML), r.nodeType === 1 && (r.textContent = "");
    const o = s(r, !1, Ao(r));
    return r instanceof Element && (r.removeAttribute("v-cloak"), r.setAttribute("data-v-app", "")), o;
  }, t;
});
function Ao(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function Po(e) {
  return Y(e) ? document.querySelector(e) : e;
}
let st;
function Mo(e) {
  st = e;
}
const Io = {
  "Component folder": "Папка компонента",
  "Not installed": "Не установлено",
  Installed: "Установлено",
  Verified: "Хеш проверен",
  "Not verified": "Хеш не проверен",
  "Experimental version": "Экспериментальная версия",
  Install: "Установить",
  "Install latest (experimental)": "Установить latest (экспериментально)",
  Stream: "Видеопоток",
  "Maximum size (0 = native)": "Максимальный размер (0 = родной)",
  "Maximum FPS": "Максимальная частота кадров",
  "Bit rate": "Битрейт",
  "Settle time (ms)": "Время стабильности (мс)",
  "Settle timeout (ms)": "Ожидание стабильности (мс)",
  "Keep screen awake": "Не гасить экран",
  "Screenshot after action": "Снимок после действия",
  "Idle disconnect (minutes)": "Отключение по простою (минуты)",
  "ADB server port (0 = default)": "Порт сервера ADB (0 = стандартный)",
  Refresh: "Обновить",
  downloading: "скачивание",
  verifying: "проверка хеша",
  extracting: "распаковка",
  probing: "проверка файлов",
  done: "готово",
  failed: "ошибка"
};
function pe(e) {
  const t = st == null ? void 0 : st(e);
  return t && t !== e ? t : document.documentElement.lang.startsWith("ru") || (st == null ? void 0 : st("Settings")) === "Настройки" ? Io[e] ?? e : e;
}
const Ro = { class: "android-settings" }, Fo = ["onUpdate:modelValue", "placeholder", "disabled"], Do = { key: 0 }, jo = { key: 1 }, No = {
  key: 1,
  class: "folder"
}, Ho = {
  key: 2,
  role: "alert"
}, Vo = { class: "actions" }, $o = ["disabled", "onClick"], Lo = ["disabled", "onClick"], Uo = {
  key: 3,
  "aria-live": "polite"
}, Ko = {
  key: 0,
  role: "alert"
}, Bo = ["disabled"], Wo = ["onUpdate:modelValue", "min", "max"], ko = /* @__PURE__ */ Qi({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e, n = ["adb", "scrcpy"], r = /* @__PURE__ */ Ft({ adb_path: "", scrcpy_path: "", adb_server_port: 0, max_size: 1280, max_fps: 30, video_bit_rate: 8e6, stay_awake: !0, screenshot_after_action: !0, settle_ms: 300, settle_timeout_ms: 3e3, idle_disconnect_minutes: 15 }), i = /* @__PURE__ */ Cs("");
    try {
      Object.assign(r, JSON.parse(s.api.getJson() || "{}"));
    } catch (O) {
      i.value = String(O);
    }
    const o = [
      { key: "max_size", label: "Maximum size (0 = native)", min: 0, max: 4096 },
      { key: "max_fps", label: "Maximum FPS", min: 1, max: 120 },
      { key: "video_bit_rate", label: "Bit rate", min: 5e5, max: 1e8 },
      { key: "settle_ms", label: "Settle time (ms)", min: 0, max: 5e3 },
      { key: "settle_timeout_ms", label: "Settle timeout (ms)", min: 0, max: 3e4 },
      { key: "idle_disconnect_minutes", label: "Idle disconnect (minutes)", min: 1, max: 1440 },
      { key: "adb_server_port", label: "ADB server port (0 = default)", min: 0, max: 65535 }
    ], l = /* @__PURE__ */ Cs(), c = /* @__PURE__ */ Cs(!1), d = /* @__PURE__ */ Ft({ adb: !1, scrcpy: !1 });
    let a, p = !1;
    const C = (O) => (O / 1048576).toFixed(1);
    function T(O) {
      var _, L;
      const F = (L = (_ = l.value) == null ? void 0 : _[O].job) == null ? void 0 : L.stage;
      return d[O] || !!F && F !== "done" && F !== "failed";
    }
    async function j(O, F = {}) {
      const _ = await s.api.invoke("plugin.action", { pluginId: "android", action: O, valueJson: JSON.stringify({ ...r, ...F }) });
      if (!_.ok || !_.resultJson) throw new Error(_.error || "Android action failed");
      return JSON.parse(_.resultJson);
    }
    async function P() {
      if (!(c.value || p)) {
        clearTimeout(a), c.value = !0;
        try {
          l.value = await j("runtimeStatus"), i.value = "";
        } catch (O) {
          i.value = O instanceof Error ? O.message : String(O);
        } finally {
          c.value = !1, !p && n.some(T) && (a = setTimeout(P, 1e3));
        }
      }
    }
    async function J(O, F) {
      d[O] = !0;
      try {
        const _ = await j("install", { component: O, channel: F });
        l.value && (l.value[O].job = _), i.value = "";
      } catch (_) {
        i.value = _ instanceof Error ? _.message : String(_);
      } finally {
        d[O] = !1, p || (a = setTimeout(P, 1e3));
      }
    }
    function B() {
      return JSON.stringify(r);
    }
    return t({ toJson: B }), Mr(P), rn(() => {
      p = !0, clearTimeout(a);
    }), (O, F) => (ge(), ve("div", Ro, [
      (ge(), ve(de, null, xn(n, (_) => {
        var L, fe, ce, be, ze;
        return X("section", { key: _ }, [
          X("h3", null, G(_ === "adb" ? "ADB" : "scrcpy"), 1),
          X("label", null, [
            Me(G(ae(pe)("Component folder")) + " ", 1),
            Gt(X("input", {
              "onUpdate:modelValue": (ye) => r[_ + "_path"] = ye,
              placeholder: (L = l.value) == null ? void 0 : L[_].defaultFolder,
              disabled: T(_),
              onChange: P,
              spellcheck: "false"
            }, null, 40, Fo), [
              [kn, r[_ + "_path"]]
            ])
          ]),
          l.value ? (ge(), ve("p", Do, [
            Me(G(ae(pe)(l.value[_].installed ? "Installed" : "Not installed")) + " ", 1),
            l.value[_].installed ? (ge(), ve(de, { key: 0 }, [
              Me(" · " + G(l.value[_].version) + " · " + G(l.value[_].channel) + " · " + G(ae(pe)(l.value[_].verified ? "Verified" : "Not verified")), 1)
            ], 64)) : qe("", !0),
            l.value[_].experimental ? (ge(), ve("span", jo, " · " + G(ae(pe)("Experimental version")), 1)) : qe("", !0)
          ])) : qe("", !0),
          (fe = l.value) != null && fe[_].folder ? (ge(), ve("p", No, G(l.value[_].folder), 1)) : qe("", !0),
          (ce = l.value) != null && ce[_].error ? (ge(), ve("p", Ho, G(l.value[_].error), 1)) : qe("", !0),
          X("div", Vo, [
            X("button", {
              disabled: !l.value || T(_) || !!l.value[_].error,
              onClick: (ye) => J(_, "pinned")
            }, G(ae(pe)("Install")) + " " + G((be = l.value) == null ? void 0 : be.pinned[_]), 9, $o),
            X("button", {
              disabled: !l.value || T(_) || !!l.value[_].error,
              onClick: (ye) => J(_, "latest")
            }, G(ae(pe)("Install latest (experimental)")), 9, Lo)
          ]),
          (ze = l.value) != null && ze[_].job ? (ge(), ve("p", Uo, [
            Me(G(ae(pe)(l.value[_].job.stage)) + " · " + G(C(l.value[_].job.bytesDone)) + " MB ", 1),
            l.value[_].job.bytesTotal ? (ge(), ve(de, { key: 0 }, [
              Me(" / " + G(C(l.value[_].job.bytesTotal)) + " MB", 1)
            ], 64)) : qe("", !0),
            Me(" " + G(l.value[_].job.error), 1)
          ])) : qe("", !0)
        ]);
      }), 64)),
      i.value ? (ge(), ve("p", Ko, G(i.value), 1)) : qe("", !0),
      X("button", {
        onClick: P,
        disabled: c.value
      }, G(ae(pe)("Refresh")), 9, Bo),
      X("section", null, [
        X("h3", null, G(ae(pe)("Stream")), 1),
        (ge(), ve(de, null, xn(o, (_) => X("label", {
          key: _.key
        }, [
          Me(G(ae(pe)(_.label)) + " ", 1),
          Gt(X("input", {
            type: "number",
            "onUpdate:modelValue": (L) => r[_.key] = L,
            min: _.min,
            max: _.max
          }, null, 8, Wo), [
            [
              kn,
              r[_.key],
              void 0,
              { number: !0 }
            ]
          ])
        ])), 64)),
        X("label", null, [
          Gt(X("input", {
            type: "checkbox",
            "onUpdate:modelValue": F[0] || (F[0] = (_) => r.stay_awake = _)
          }, null, 512), [
            [Jn, r.stay_awake]
          ]),
          Me(" " + G(ae(pe)("Keep screen awake")), 1)
        ]),
        X("label", null, [
          Gt(X("input", {
            type: "checkbox",
            "onUpdate:modelValue": F[1] || (F[1] = (_) => r.screenshot_after_action = _)
          }, null, 512), [
            [Jn, r.screenshot_after_action]
          ]),
          Me(" " + G(ae(pe)("Screenshot after action")), 1)
        ])
      ])
    ]));
  }
}), Jo = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, r] of t)
    s[n] = r;
  return s;
}, qo = /* @__PURE__ */ Jo(ko, [["__scopeId", "data-v-0bc23ba1"]]);
function Yo(e, t) {
  var r;
  Mo((r = t.t) == null ? void 0 : r.bind(t));
  const s = Oo(qo, { api: t }), n = s.mount(e);
  return { save: () => n.toJson(), destroy: () => s.unmount() };
}
export {
  Yo as mount
};
