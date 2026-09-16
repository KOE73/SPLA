(function(){"use strict";try{if(typeof document<"u"){var e=document.createElement("style");e.appendChild(document.createTextNode(".geom-settings[data-v-3ec47264]{display:grid;gap:14px}section[data-v-3ec47264]{display:grid;gap:6px}h3[data-v-3ec47264],p[data-v-3ec47264]{margin:0}label[data-v-3ec47264]{display:flex;align-items:center;gap:8px;flex-wrap:wrap}label.check[data-v-3ec47264]{gap:6px}input[data-v-3ec47264]:not([type=checkbox]):not([type=color]){flex:1;min-width:110px}.colour[data-v-3ec47264]{display:flex;gap:6px;flex:1}.colour input[type=color][data-v-3ec47264]{width:34px;padding:0}.hint[data-v-3ec47264]{opacity:.65;font-size:.85em}")),document.head.appendChild(e)}}catch(a){console.error("vite-plugin-css-injected-by-js",a)}})();
/**
* @vue/shared v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Jn(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const n of e.split(",")) t[n] = 1;
  return (n) => n in t;
}
const K = {}, ft = [], Ie = () => {
}, Ys = () => !1, ln = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), fn = (e) => e.startsWith("onUpdate:"), se = Object.assign, zn = (e, t) => {
  const n = e.indexOf(t);
  n > -1 && e.splice(n, 1);
}, ii = Object.prototype.hasOwnProperty, j = (e, t) => ii.call(e, t), M = Array.isArray, ze = (e) => jt(e) === "[object Map]", dt = (e) => jt(e) === "[object Set]", _s = (e) => jt(e) === "[object Date]", R = (e) => typeof e == "function", k = (e) => typeof e == "string", Re = (e) => typeof e == "symbol", W = (e) => e !== null && typeof e == "object", ks = (e) => (W(e) || R(e)) && R(e.then) && R(e.catch), Xs = Object.prototype.toString, jt = (e) => Xs.call(e), oi = (e) => jt(e).slice(8, -1), Zs = (e) => jt(e) === "[object Object]", Gn = (e) => k(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, wt = /* @__PURE__ */ Jn(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), cn = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((n) => t[n] || (t[n] = e(n)));
}, li = /-\w/g, me = cn(
  (e) => e.replace(li, (t) => t.slice(1).toUpperCase())
), fi = /\B([A-Z])/g, it = cn(
  (e) => e.replace(fi, "-$1").toLowerCase()
), Qs = cn((e) => e.charAt(0).toUpperCase() + e.slice(1)), xn = cn(
  (e) => e ? `on${Qs(e)}` : ""
), Me = (e, t) => !Object.is(e, t), Yt = (e, ...t) => {
  for (let n = 0; n < e.length; n++)
    e[n](...t);
}, er = (e, t, n, s = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: s,
    value: n
  });
}, Yn = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let bs;
const un = () => bs || (bs = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function kn(e) {
  if (M(e)) {
    const t = {};
    for (let n = 0; n < e.length; n++) {
      const s = e[n], r = k(s) ? di(s) : kn(s);
      if (r)
        for (const i in r)
          t[i] = r[i];
    }
    return t;
  } else if (k(e) || W(e))
    return e;
}
const ci = /;(?![^(]*\))/g, ui = /:([^]+)/, ai = /\/\*[^]*?\*\//g;
function di(e) {
  const t = {};
  return e.replace(ai, "").split(ci).forEach((n) => {
    if (n) {
      const s = n.split(ui);
      s.length > 1 && (t[s[0].trim()] = s[1].trim());
    }
  }), t;
}
function Xn(e) {
  let t = "";
  if (k(e))
    t = e;
  else if (M(e))
    for (let n = 0; n < e.length; n++) {
      const s = Xn(e[n]);
      s && (t += s + " ");
    }
  else if (W(e))
    for (const n in e)
      e[n] && (t += n + " ");
  return t.trim();
}
const hi = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", pi = /* @__PURE__ */ Jn(hi);
function tr(e) {
  return !!e || e === "";
}
function gi(e, t) {
  if (e.length !== t.length) return !1;
  let n = !0;
  for (let s = 0; n && s < e.length; s++)
    n = pt(e[s], t[s]);
  return n;
}
function ys(e, t) {
  if (e.size !== t.size) return !1;
  const n = Array.from(t), s = new Uint8Array(n.length);
  for (const r of e) {
    let i = -1;
    for (let o = 0; o < n.length; o++)
      if (!s[o] && pt(r, n[o])) {
        i = o;
        break;
      }
    if (i < 0) return !1;
    s[i] = 1;
  }
  return !0;
}
function pt(e, t) {
  if (e === t) return !0;
  let n = _s(e), s = _s(t);
  if (n || s)
    return n && s ? e.getTime() === t.getTime() : !1;
  if (n = Re(e), s = Re(t), n || s)
    return e === t;
  if (n = M(e), s = M(t), n || s)
    return n && s ? gi(e, t) : !1;
  if (n = W(e), s = W(t), n || s) {
    if (!n || !s)
      return !1;
    if (n = ze(e), s = ze(t), n || s || (n = dt(e), s = dt(t), n || s))
      return n && s ? ys(e, t) : !1;
    const r = Object.keys(e).length, i = Object.keys(t).length;
    if (r !== i)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), c = t.hasOwnProperty(o);
      if (l && !c || !l && c || !pt(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function nr(e, t) {
  return e.findIndex((n) => pt(n, t));
}
const sr = (e) => !!(e && e.__v_isRef === !0), L = (e) => k(e) ? e : e == null ? "" : M(e) || W(e) && (e.toString === Xs || !R(e.toString)) ? sr(e) ? L(e.value) : JSON.stringify(e, rr, 2) : String(e), rr = (e, t) => sr(t) ? rr(e, t.value) : ze(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (n, [s, r], i) => (n[vn(s, i) + " =>"] = r, n),
    {}
  )
} : dt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((n) => vn(n))
} : Re(t) ? vn(t) : W(t) && !M(t) && !Zs(t) ? String(t) : t, vn = (e, t = "") => {
  var n;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Re(e) ? `Symbol(${(n = e.description) != null ? n : t})` : e
  );
};
/**
* @vue/reactivity v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let ne;
class mi {
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
      let t, n;
      if (this.scopes) {
        const s = this.scopes.slice();
        for (t = 0, n = s.length; t < n; t++)
          s[t].pause();
      }
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
      if (this.scopes) {
        const r = this.scopes.slice();
        for (t = 0, n = r.length; t < n; t++)
          r[t].resume();
      }
      const s = this.effects.slice();
      for (t = 0, n = s.length; t < n; t++)
        s[t].resume();
    }
  }
  run(t) {
    if (this._active) {
      const n = ne;
      try {
        return ne = this, t();
      } finally {
        ne = n;
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
      let n, s;
      for (n = 0, s = this.effects.length; n < s; n++)
        this.effects[n].stop();
      for (this.effects.length = 0, n = 0, s = this.cleanups.length; n < s; n++)
        this.cleanups[n]();
      if (this.cleanups.length = 0, this.scopes) {
        const r = this.scopes.slice();
        for (n = 0, s = r.length; n < s; n++)
          r[n].stop(!0);
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
function _i() {
  return ne;
}
let J;
const Sn = /* @__PURE__ */ new WeakSet();
class ir {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, ne && (ne.active ? ne.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, Sn.has(this) && (Sn.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || lr(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, xs(this), fr(this);
    const t = J, n = _e;
    J = this, _e = !0;
    try {
      return this.fn();
    } finally {
      cr(this), J = t, _e = n, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        es(t);
      this.deps = this.depsTail = void 0, xs(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? Sn.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Vn(this) && this.run();
  }
  get dirty() {
    return Vn(this);
  }
}
let or = 0, Ct, Tt;
function lr(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Tt, Tt = e;
    return;
  }
  e.next = Ct, Ct = e;
}
function Zn() {
  or++;
}
function Qn() {
  if (--or > 0)
    return;
  if (Tt) {
    let t = Tt;
    for (Tt = void 0; t; ) {
      const n = t.next;
      t.next = void 0, t.flags &= -9, t = n;
    }
  }
  let e;
  for (; Ct; ) {
    let t = Ct;
    for (Ct = void 0; t; ) {
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
function fr(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function cr(e) {
  let t, n = e.depsTail, s = n;
  for (; s; ) {
    const r = s.prevDep;
    s.version === -1 ? (s === n && (n = r), es(s), bi(s)) : t = s, s.dep.activeLink = s.prevActiveLink, s.prevActiveLink = void 0, s = r;
  }
  e.deps = t, e.depsTail = n;
}
function Vn(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (ur(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function ur(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Pt) || (e.globalVersion = Pt, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Vn(e))))
    return;
  e.flags |= 2;
  const t = e.dep, n = J, s = _e;
  J = e, _e = !0;
  try {
    fr(e);
    const r = e.fn(e._value);
    (t.version === 0 || Me(r, e._value)) && (e.flags |= 128, e._value = r, t.version++);
  } catch (r) {
    throw t.version++, r;
  } finally {
    J = n, _e = s, cr(e), e.flags &= -3;
  }
}
function es(e, t = !1) {
  const { dep: n, prevSub: s, nextSub: r } = e;
  if (s && (s.nextSub = r, e.prevSub = void 0), r && (r.prevSub = s, e.nextSub = void 0), n.subs === e && (n.subs = s, !s && n.computed)) {
    n.computed.flags &= -5;
    for (let i = n.computed.deps; i; i = i.nextDep)
      es(i, !0);
  }
  !t && !--n.sc && n.map && n.map.delete(n.key);
}
function bi(e) {
  const { prevDep: t, nextDep: n } = e;
  t && (t.nextDep = n, e.prevDep = void 0), n && (n.prevDep = t, e.nextDep = void 0);
}
let _e = !0;
const ar = [];
function $e() {
  ar.push(_e), _e = !1;
}
function Le() {
  const e = ar.pop();
  _e = e === void 0 ? !0 : e;
}
function xs(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const n = J;
    J = void 0;
    try {
      t();
    } finally {
      J = n;
    }
  }
}
let Pt = 0;
class yi {
  constructor(t, n) {
    this.sub = t, this.dep = n, this.version = n.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class ts {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!J || !_e || J === this.computed)
      return;
    let n = this.activeLink;
    if (n === void 0 || n.sub !== J)
      n = this.activeLink = new yi(J, this), J.deps ? (n.prevDep = J.depsTail, J.depsTail.nextDep = n, J.depsTail = n) : J.deps = J.depsTail = n, dr(n);
    else if (n.version === -1 && (n.version = this.version, n.nextDep)) {
      const s = n.nextDep;
      s.prevDep = n.prevDep, n.prevDep && (n.prevDep.nextDep = s), n.prevDep = J.depsTail, n.nextDep = void 0, J.depsTail.nextDep = n, J.depsTail = n, J.deps === n && (J.deps = s);
    }
    return n;
  }
  trigger(t) {
    this.version++, Pt++, this.notify(t);
  }
  notify(t) {
    Zn();
    try {
      for (let n = this.subs; n; n = n.prevSub)
        n.sub.notify() && n.sub.dep.notify();
    } finally {
      Qn();
    }
  }
}
function dr(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let s = t.deps; s; s = s.nextDep)
        dr(s);
    }
    const n = e.dep.subs;
    n !== e && (e.prevSub = n, n && (n.nextSub = e)), e.dep.subs = e;
  }
}
const Dn = /* @__PURE__ */ new WeakMap(), st = /* @__PURE__ */ Symbol(
  ""
), jn = /* @__PURE__ */ Symbol(
  ""
), Mt = /* @__PURE__ */ Symbol(
  ""
);
function ie(e, t, n) {
  if (_e && J) {
    let s = Dn.get(e);
    s || Dn.set(e, s = /* @__PURE__ */ new Map());
    let r = s.get(n);
    r || (s.set(n, r = new ts()), r.map = s, r.key = n), r.track();
  }
}
function Ne(e, t, n, s, r, i) {
  const o = Dn.get(e);
  if (!o) {
    Pt++;
    return;
  }
  const l = (c) => {
    c && c.trigger();
  };
  if (Zn(), t === "clear")
    o.forEach(l);
  else {
    const c = M(e), d = c && Gn(n);
    if (c && n === "length") {
      const a = Number(s);
      o.forEach((p, w) => {
        (w === "length" || w === Mt || !Re(w) && w >= a) && l(p);
      });
    } else
      switch ((n !== void 0 || o.has(void 0)) && l(o.get(n)), d && l(o.get(Mt)), t) {
        case "add":
          c ? d && l(o.get("length")) : (l(o.get(st)), ze(e) && l(o.get(jn)));
          break;
        case "delete":
          c || (l(o.get(st)), ze(e) && l(o.get(jn)));
          break;
        case "set":
          ze(e) && l(o.get(st));
          break;
      }
  }
  Qn();
}
function ot(e) {
  const t = /* @__PURE__ */ D(e);
  return t === e ? t : (ie(t, "iterate", Mt), /* @__PURE__ */ be(e) ? t : t.map(Ke));
}
function ns(e) {
  return ie(e = /* @__PURE__ */ D(e), "iterate", Mt), e;
}
function Ae(e, t) {
  return /* @__PURE__ */ Ge(e) ? It(/* @__PURE__ */ ct(e) ? Ke(t) : t) : Ke(t);
}
const xi = {
  __proto__: null,
  [Symbol.iterator]() {
    return wn(this, Symbol.iterator, (e) => Ae(this, e));
  },
  concat(...e) {
    return ot(this).concat(
      ...e.map((t) => M(t) ? ot(t) : t)
    );
  },
  entries() {
    return wn(this, "entries", (e) => (e[1] = Ae(this, e[1]), e));
  },
  every(e, t) {
    return Fe(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return Fe(
      this,
      "filter",
      e,
      t,
      (n) => n.map((s) => Ae(this, s)),
      arguments
    );
  },
  find(e, t) {
    return Fe(
      this,
      "find",
      e,
      t,
      (n) => Ae(this, n),
      arguments
    );
  },
  findIndex(e, t) {
    return Fe(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return Fe(
      this,
      "findLast",
      e,
      t,
      (n) => Ae(this, n),
      arguments
    );
  },
  findLastIndex(e, t) {
    return Fe(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return Fe(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return Cn(this, "includes", e);
  },
  indexOf(...e) {
    return Cn(this, "indexOf", e);
  },
  join(e) {
    return ot(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return Cn(this, "lastIndexOf", e);
  },
  map(e, t) {
    return Fe(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return yt(this, "pop");
  },
  push(...e) {
    return yt(this, "push", e);
  },
  reduce(e, ...t) {
    return vs(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return vs(this, "reduceRight", e, t);
  },
  shift() {
    return yt(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return Fe(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return yt(this, "splice", e);
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
    return yt(this, "unshift", e);
  },
  values() {
    return wn(this, "values", (e) => Ae(this, e));
  }
};
function wn(e, t, n) {
  const s = ns(e), r = s[t]();
  return s !== e && !/* @__PURE__ */ be(e) && (r._next = r.next, r.next = () => {
    const i = r._next();
    return i.done || (i.value = n(i.value)), i;
  }), r;
}
const vi = Array.prototype;
function Fe(e, t, n, s, r, i) {
  const o = ns(e), l = o !== e && !/* @__PURE__ */ be(e), c = o[t];
  if (c !== vi[t]) {
    const p = c.apply(e, i);
    return l ? Ke(p) : p;
  }
  let d = n;
  o !== e && (l ? d = function(p, w) {
    return n.call(this, Ae(e, p), w, e);
  } : n.length > 2 && (d = function(p, w) {
    return n.call(this, p, w, e);
  }));
  const a = c.call(o, d, s);
  return l && r ? r(a) : a;
}
function vs(e, t, n, s) {
  const r = ns(e), i = r !== e && !/* @__PURE__ */ be(e);
  let o = n, l = !1;
  r !== e && (i ? (l = s.length === 0, o = function(d, a, p) {
    return l && (l = !1, d = Ae(e, d)), n.call(this, d, Ae(e, a), p, e);
  }) : n.length > 3 && (o = function(d, a, p) {
    return n.call(this, d, a, p, e);
  }));
  const c = r[t](o, ...s);
  return l ? Ae(e, c) : c;
}
function Cn(e, t, n) {
  const s = /* @__PURE__ */ D(e);
  ie(s, "iterate", Mt);
  const r = s[t](...n);
  return (r === -1 || r === !1) && /* @__PURE__ */ is(n[0]) ? (n[0] = /* @__PURE__ */ D(n[0]), s[t](...n)) : r;
}
function yt(e, t, n = []) {
  $e(), Zn();
  const s = (/* @__PURE__ */ D(e))[t].apply(e, n);
  return Qn(), Le(), s;
}
const Si = /* @__PURE__ */ Jn("__proto__,__v_isRef,__isVue"), hr = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Re)
);
function wi(e) {
  Re(e) || (e = String(e));
  const t = /* @__PURE__ */ D(this);
  return ie(t, "has", e), t.hasOwnProperty(e);
}
class pr {
  constructor(t = !1, n = !1) {
    this._isReadonly = t, this._isShallow = n;
  }
  get(t, n, s) {
    if (n === "__v_skip") return t.__v_skip;
    const r = this._isReadonly, i = this._isShallow;
    if (n === "__v_isReactive")
      return !r;
    if (n === "__v_isReadonly")
      return r;
    if (n === "__v_isShallow")
      return i;
    if (n === "__v_raw")
      return s === (r ? i ? Fi : br : i ? _r : mr).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(s) ? t : void 0;
    const o = M(t);
    if (!r) {
      let c;
      if (o && (c = xi[n]))
        return c;
      if (n === "hasOwnProperty")
        return wi;
    }
    const l = Reflect.get(
      t,
      n,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ oe(t) ? t : s
    );
    if ((Re(n) ? hr.has(n) : Si(n)) || (r || ie(t, "get", n), i))
      return l;
    if (/* @__PURE__ */ oe(l)) {
      const c = o && Gn(n) ? l : l.value;
      return r && W(c) ? /* @__PURE__ */ Hn(c) : c;
    }
    return W(l) ? r ? /* @__PURE__ */ Hn(l) : /* @__PURE__ */ an(l) : l;
  }
}
class gr extends pr {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, n, s, r) {
    let i = t[n];
    const o = M(t) && Gn(n);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ Ge(i);
      if (!/* @__PURE__ */ be(s) && !/* @__PURE__ */ Ge(s) && (i = /* @__PURE__ */ D(i), s = /* @__PURE__ */ D(s)), !o && /* @__PURE__ */ oe(i) && !/* @__PURE__ */ oe(s))
        return d || (i.value = s), !0;
    }
    const l = o ? Number(n) < t.length : j(t, n), c = Reflect.set(
      t,
      n,
      s,
      /* @__PURE__ */ oe(t) ? t : r
    );
    return t === /* @__PURE__ */ D(r) && c && (l ? Me(s, i) && Ne(t, "set", n, s) : Ne(t, "add", n, s)), c;
  }
  deleteProperty(t, n) {
    const s = j(t, n);
    t[n];
    const r = Reflect.deleteProperty(t, n);
    return r && s && Ne(t, "delete", n, void 0), r;
  }
  has(t, n) {
    const s = Reflect.has(t, n);
    return (!Re(n) || !hr.has(n)) && ie(t, "has", n), s;
  }
  ownKeys(t) {
    return ie(
      t,
      "iterate",
      M(t) ? "length" : st
    ), Reflect.ownKeys(t);
  }
}
class Ci extends pr {
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
const Ti = /* @__PURE__ */ new gr(), Ei = /* @__PURE__ */ new Ci(), Oi = /* @__PURE__ */ new gr(!0);
const Nn = (e) => e, Bt = (e) => Reflect.getPrototypeOf(e);
function Ai(e, t, n) {
  return function(...s) {
    const r = this.__v_raw, i = /* @__PURE__ */ D(r), o = ze(i), l = e === "entries" || e === Symbol.iterator && o, c = e === "keys" && o, d = r[e](...s), a = n ? Nn : t ? It : Ke;
    return !t && ie(
      i,
      "iterate",
      c ? jn : st
    ), se(
      // inheriting all iterator properties
      Object.create(d),
      {
        // iterator protocol
        next() {
          const { value: p, done: w } = d.next();
          return w ? { value: p, done: w } : {
            value: l ? [a(p[0]), a(p[1])] : a(p),
            done: w
          };
        }
      }
    );
  };
}
function qt(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function Pi(e, t) {
  const n = {
    get(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ D(i), l = /* @__PURE__ */ D(r);
      e || (Me(r, l) && ie(o, "get", r), ie(o, "get", l));
      const { has: c } = Bt(o), d = t ? Nn : e ? It : Ke;
      if (c.call(o, r))
        return d(i.get(r));
      if (c.call(o, l))
        return d(i.get(l));
      i !== o && i.get(r);
    },
    get size() {
      const r = this.__v_raw;
      return !e && ie(/* @__PURE__ */ D(r), "iterate", st), r.size;
    },
    has(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ D(i), l = /* @__PURE__ */ D(r);
      return e || (Me(r, l) && ie(o, "has", r), ie(o, "has", l)), r === l ? i.has(r) : i.has(r) || i.has(l);
    },
    forEach(r, i) {
      const o = this, l = o.__v_raw, c = /* @__PURE__ */ D(l), d = t ? Nn : e ? It : Ke;
      return !e && ie(c, "iterate", st), l.forEach((a, p) => r.call(i, d(a), d(p), o));
    }
  };
  return se(
    n,
    e ? {
      add: qt("add"),
      set: qt("set"),
      delete: qt("delete"),
      clear: qt("clear")
    } : {
      add(r) {
        const i = /* @__PURE__ */ D(this), o = Bt(i), l = /* @__PURE__ */ D(r), c = !t && !/* @__PURE__ */ be(r) && !/* @__PURE__ */ Ge(r) ? l : r;
        return o.has.call(i, c) || Me(r, c) && o.has.call(i, r) || Me(l, c) && o.has.call(i, l) || (i.add(c), Ne(i, "add", c, c)), this;
      },
      set(r, i) {
        !t && !/* @__PURE__ */ be(i) && !/* @__PURE__ */ Ge(i) && (i = /* @__PURE__ */ D(i));
        const o = /* @__PURE__ */ D(this), { has: l, get: c } = Bt(o);
        let d = l.call(o, r);
        d || (r = /* @__PURE__ */ D(r), d = l.call(o, r));
        const a = c.call(o, r);
        return o.set(r, i), d ? Me(i, a) && Ne(o, "set", r, i) : Ne(o, "add", r, i), this;
      },
      delete(r) {
        const i = /* @__PURE__ */ D(this), { has: o, get: l } = Bt(i);
        let c = o.call(i, r);
        c || (r = /* @__PURE__ */ D(r), c = o.call(i, r)), l && l.call(i, r);
        const d = i.delete(r);
        return c && Ne(i, "delete", r, void 0), d;
      },
      clear() {
        const r = /* @__PURE__ */ D(this), i = r.size !== 0, o = r.clear();
        return i && Ne(
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
    n[r] = Ai(r, e, t);
  }), n;
}
function ss(e, t) {
  const n = Pi(e, t);
  return (s, r, i) => r === "__v_isReactive" ? !e : r === "__v_isReadonly" ? e : r === "__v_raw" ? s : Reflect.get(
    j(n, r) && r in s ? n : s,
    r,
    i
  );
}
const Mi = {
  get: /* @__PURE__ */ ss(!1, !1)
}, Ii = {
  get: /* @__PURE__ */ ss(!1, !0)
}, Ri = {
  get: /* @__PURE__ */ ss(!0, !1)
};
const mr = /* @__PURE__ */ new WeakMap(), _r = /* @__PURE__ */ new WeakMap(), br = /* @__PURE__ */ new WeakMap(), Fi = /* @__PURE__ */ new WeakMap();
function Vi(e) {
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
function an(e) {
  return /* @__PURE__ */ Ge(e) ? e : rs(
    e,
    !1,
    Ti,
    Mi,
    mr
  );
}
// @__NO_SIDE_EFFECTS__
function Di(e) {
  return rs(
    e,
    !1,
    Oi,
    Ii,
    _r
  );
}
// @__NO_SIDE_EFFECTS__
function Hn(e) {
  return rs(
    e,
    !0,
    Ei,
    Ri,
    br
  );
}
function rs(e, t, n, s, r) {
  if (!W(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const i = r.get(e);
  if (i)
    return i;
  const o = Vi(oi(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? s : n
  );
  return r.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function ct(e) {
  return /* @__PURE__ */ Ge(e) ? /* @__PURE__ */ ct(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function Ge(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function be(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function is(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function D(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ D(t) : e;
}
function ji(e) {
  return !j(e, "__v_skip") && Object.isExtensible(e) && er(e, "__v_skip", !0), e;
}
const Ke = (e) => W(e) ? /* @__PURE__ */ an(e) : e, It = (e) => W(e) ? /* @__PURE__ */ Hn(e) : e;
// @__NO_SIDE_EFFECTS__
function oe(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Ni(e) {
  return Hi(e, !1);
}
function Hi(e, t) {
  return /* @__PURE__ */ oe(e) ? e : new Ui(e, t);
}
class Ui {
  constructor(t, n) {
    this.dep = new ts(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = n ? t : /* @__PURE__ */ D(t), this._value = n ? t : Ke(t), this.__v_isShallow = n;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const n = this._rawValue, s = this.__v_isShallow || /* @__PURE__ */ be(t) || /* @__PURE__ */ Ge(t);
    t = s ? t : /* @__PURE__ */ D(t), Me(t, n) && (this._rawValue = t, this._value = s ? t : Ke(t), this.dep.trigger());
  }
}
function q(e) {
  return /* @__PURE__ */ oe(e) ? e.value : e;
}
const $i = {
  get: (e, t, n) => t === "__v_raw" ? e : q(Reflect.get(e, t, n)),
  set: (e, t, n, s) => {
    const r = e[t];
    return /* @__PURE__ */ oe(r) && !/* @__PURE__ */ oe(n) ? (r.value = n, !0) : Reflect.set(e, t, n, s);
  }
};
function yr(e) {
  return /* @__PURE__ */ ct(e) ? e : new Proxy(e, $i);
}
class Li {
  constructor(t, n, s) {
    this.fn = t, this.setter = n, this._value = void 0, this.dep = new ts(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Pt - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !n, this.isSSR = s;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    J !== this)
      return lr(this, !0), !0;
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
function Ki(e, t, n = !1) {
  let s, r;
  return R(e) ? s = e : (s = e.get, r = e.set), new Li(s, r, n);
}
const Jt = {}, Zt = /* @__PURE__ */ new WeakMap();
let et;
function Wi(e, t = !1, n = et) {
  if (n) {
    let s = Zt.get(n);
    s || Zt.set(n, s = []), s.push(e);
  }
}
function Bi(e, t, n = K) {
  const { immediate: s, deep: r, once: i, scheduler: o, augmentJob: l, call: c } = n, d = (O) => r ? O : /* @__PURE__ */ be(O) || r === !1 || r === 0 ? He(O, 1) : He(O);
  let a, p, w, C, N = !1, I = !1;
  if (/* @__PURE__ */ oe(e) ? (p = () => e.value, N = /* @__PURE__ */ be(e)) : /* @__PURE__ */ ct(e) ? (p = () => d(e), N = !0) : M(e) ? (I = !0, N = e.some((O) => /* @__PURE__ */ ct(O) || /* @__PURE__ */ be(O)), p = () => e.map((O) => {
    if (/* @__PURE__ */ oe(O))
      return O.value;
    if (/* @__PURE__ */ ct(O))
      return d(O);
    if (R(O))
      return c ? c(O, 2) : O();
  })) : R(e) ? t ? p = c ? () => c(e, 2) : e : p = () => {
    if (w) {
      $e();
      try {
        w();
      } finally {
        Le();
      }
    }
    const O = et;
    et = a;
    try {
      return c ? c(e, 3, [C]) : e(C);
    } finally {
      et = O;
    }
  } : p = Ie, t && r) {
    const O = p, Q = r === !0 ? 1 / 0 : r;
    p = () => He(O(), Q);
  }
  const X = _i(), G = () => {
    a.stop(), X && X.active && zn(X.effects, a);
  };
  if (i && t) {
    const O = t;
    t = (...Q) => {
      const xe = O(...Q);
      return G(), xe;
    };
  }
  let V = I ? new Array(e.length).fill(Jt) : Jt;
  const H = (O) => {
    if (!(!(a.flags & 1) || !a.dirty && !O))
      if (t) {
        const Q = a.run();
        if (O || r || N || (I ? Q.some((xe, ve) => Me(xe, V[ve])) : Me(Q, V))) {
          w && w();
          const xe = et;
          et = a;
          try {
            const ve = [
              Q,
              // pass undefined as the old value when it's changed for the first time
              V === Jt ? void 0 : I && V[0] === Jt ? [] : V,
              C
            ];
            V = Q, c ? c(t, 3, ve) : (
              // @ts-expect-error
              t(...ve)
            );
          } finally {
            et = xe;
          }
        }
      } else
        a.run();
  };
  return l && l(H), a = new ir(p), a.scheduler = o ? () => o(H, !1) : H, C = (O) => Wi(O, !1, a), w = a.onStop = () => {
    const O = Zt.get(a);
    if (O) {
      if (c)
        c(O, 4);
      else
        for (const Q of O) Q();
      Zt.delete(a);
    }
  }, t ? s ? H(!0) : V = a.run() : o ? o(H.bind(null, !0), !0) : a.run(), G.pause = a.pause.bind(a), G.resume = a.resume.bind(a), G.stop = G, G;
}
function He(e, t = 1 / 0, n) {
  if (t <= 0 || !W(e) || e.__v_skip || (n = n || /* @__PURE__ */ new Map(), (n.get(e) || 0) >= t))
    return e;
  if (n.set(e, t), t--, /* @__PURE__ */ oe(e))
    He(e.value, t, n);
  else if (M(e))
    for (let s = 0; s < e.length; s++)
      He(e[s], t, n);
  else if (dt(e) || ze(e))
    e.forEach((s) => {
      He(s, t, n);
    });
  else if (Zs(e)) {
    for (const s in e)
      He(e[s], t, n);
    for (const s of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, s) && He(e[s], t, n);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function Nt(e, t, n, s) {
  try {
    return s ? e(...s) : e();
  } catch (r) {
    dn(r, t, n);
  }
}
function ye(e, t, n, s) {
  if (R(e)) {
    const r = Nt(e, t, n, s);
    return r && ks(r) && r.catch((i) => {
      dn(i, t, n);
    }), r;
  }
  if (M(e)) {
    const r = [];
    for (let i = 0; i < e.length; i++)
      r.push(ye(e[i], t, n, s));
    return r;
  }
}
function dn(e, t, n, s = !0) {
  const r = t ? t.vnode : null, { errorHandler: i, throwUnhandledErrorInProduction: o } = t && t.appContext.config || K;
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
    if (i) {
      $e(), Nt(i, null, 10, [
        e,
        c,
        d
      ]), Le();
      return;
    }
  }
  qi(e, n, r, s, o);
}
function qi(e, t, n, s = !0, r = !1) {
  if (r)
    throw e;
  console.error(e);
}
const ce = [];
let Oe = -1;
const ut = [];
let Je = null, lt = 0;
const xr = /* @__PURE__ */ Promise.resolve();
let Qt = null;
function Ji(e) {
  const t = Qt || xr;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function zi(e) {
  let t = Oe + 1, n = ce.length;
  for (; t < n; ) {
    const s = t + n >>> 1, r = ce[s], i = Rt(r);
    i < e || i === e && r.flags & 2 ? t = s + 1 : n = s;
  }
  return t;
}
function os(e) {
  if (!(e.flags & 1)) {
    const t = Rt(e), n = ce[ce.length - 1];
    !n || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Rt(n) ? ce.push(e) : ce.splice(zi(t), 0, e), e.flags |= 1, vr();
  }
}
function vr() {
  Qt || (Qt = xr.then(wr));
}
function Gi(e) {
  if (!M(e))
    Je && e.id === -1 ? Je.splice(lt + 1, 0, e) : e.flags & 1 || (ut.push(e), e.flags |= 1);
  else
    for (let t = 0; t < e.length; t++)
      ut.push(e[t]);
  vr();
}
function Ss(e, t, n = Oe + 1) {
  for (; n < ce.length; n++) {
    const s = ce[n];
    if (s && s.flags & 2) {
      if (e && s.id !== e.uid)
        continue;
      ce.splice(n, 1), n--, s.flags & 4 && (s.flags &= -2), s(), s.flags & 4 || (s.flags &= -2);
    }
  }
}
function Sr(e) {
  if (ut.length) {
    const t = [...new Set(ut)].sort(
      (n, s) => Rt(n) - Rt(s)
    );
    if (ut.length = 0, Je) {
      for (let n = 0; n < t.length; n++)
        Je.push(t[n]);
      return;
    }
    for (Je = t, lt = 0; lt < Je.length; lt++) {
      const n = Je[lt];
      n.flags & 4 && (n.flags &= -2), n.flags & 8 || n(), n.flags &= -2;
    }
    Je = null, lt = 0;
  }
}
const Rt = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function wr(e) {
  try {
    for (Oe = 0; Oe < ce.length; Oe++) {
      const t = ce[Oe];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Nt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Oe < ce.length; Oe++) {
      const t = ce[Oe];
      t && (t.flags &= -2);
    }
    Oe = -1, ce.length = 0, Sr(), Qt = null, (ce.length || ut.length) && wr();
  }
}
let ge = null, Cr = null;
function en(e) {
  const t = ge;
  return ge = e, Cr = e && e.type.__scopeId || null, t;
}
function Yi(e, t = ge, n) {
  if (!t || e._n)
    return e;
  const s = (...r) => {
    s._d && Fs(-1);
    const i = en(t), o = rt.length;
    let l;
    try {
      l = e(...r);
    } finally {
      for (let c = rt.length; c > o; c--) Gr();
      en(i), s._d && Fs(1);
    }
    return l;
  };
  return s._n = !0, s._c = !0, s._d = !0, s;
}
function re(e, t) {
  if (ge === null)
    return e;
  const n = _n(ge), s = e.dirs || (e.dirs = []);
  for (let r = 0; r < t.length; r++) {
    let [i, o, l, c = K] = t[r];
    i && (R(i) && (i = {
      mounted: i,
      updated: i
    }), i.deep && He(o), s.push({
      dir: i,
      instance: n,
      value: o,
      oldValue: void 0,
      arg: l,
      modifiers: c
    }));
  }
  return e;
}
function Ze(e, t, n, s) {
  const r = e.dirs, i = t && t.dirs;
  for (let o = 0; o < r.length; o++) {
    const l = r[o];
    i && (l.oldValue = i[o].value);
    let c = l.dir[s];
    c && ($e(), ye(c, n, 8, [
      e.el,
      l,
      e,
      t
    ]), Le());
  }
}
function ki(e, t) {
  if (ue) {
    let n = ue.provides;
    const s = ue.parent && ue.parent.provides;
    s === n && (n = ue.provides = Object.create(s)), n[e] = t;
  }
}
function kt(e, t, n = !1) {
  const s = Xo();
  if (s || at) {
    let r = at ? at._context.provides : s ? s.parent == null || s.ce ? s.vnode.appContext && s.vnode.appContext.provides : s.parent.provides : void 0;
    if (r && e in r)
      return r[e];
    if (arguments.length > 1)
      return n && R(t) ? t.call(s && s.proxy) : t;
  }
}
const Xi = /* @__PURE__ */ Symbol.for("v-scx"), Zi = () => kt(Xi);
function Tn(e, t, n) {
  return Tr(e, t, n);
}
function Tr(e, t, n = K) {
  const { immediate: s, deep: r, flush: i, once: o } = n, l = se({}, n), c = t && s || !t && i !== "post";
  let d;
  if (Dt) {
    if (i === "sync") {
      const C = Zi();
      d = C.__watcherHandles || (C.__watcherHandles = []);
    } else if (!c) {
      const C = () => {
      };
      return C.stop = Ie, C.resume = Ie, C.pause = Ie, C;
    }
  }
  const a = ue;
  l.call = (C, N, I) => ye(C, a, N, I);
  let p = !1;
  i === "post" ? l.scheduler = (C) => {
    ae(C, a && a.suspense);
  } : i !== "sync" && (p = !0, l.scheduler = (C, N) => {
    N ? C() : os(C);
  }), l.augmentJob = (C) => {
    t && (C.flags |= 4), p && (C.flags |= 2, a && (C.id = a.uid, C.i = a));
  };
  const w = Bi(e, t, l);
  return Dt && (d ? d.push(w) : c && w()), w;
}
function Qi(e, t, n) {
  const s = this.proxy, r = k(e) ? e.includes(".") ? Er(s, e) : () => s[e] : e.bind(s, s);
  let i;
  R(t) ? i = t : (i = t.handler, n = t);
  const o = Ht(this), l = Tr(r, i.bind(s), n);
  return o(), l;
}
function Er(e, t) {
  const n = t.split(".");
  return () => {
    let s = e;
    for (let r = 0; r < n.length && s; r++)
      s = s[n[r]];
    return s;
  };
}
const eo = /* @__PURE__ */ Symbol("_vte"), hn = (e) => e.__isTeleport, En = /* @__PURE__ */ Symbol("_leaveCb");
function to(e) {
  let t = e[0];
  if (e.length > 1) {
    for (const n of e)
      if (n.type !== We) {
        t = n;
        break;
      }
  }
  return t;
}
function Or(e) {
  if (!fs(e))
    return hn(e.type) && e.children ? to(e.children) : e;
  if (e.component)
    return e.component.subTree;
  const { shapeFlag: t, children: n } = e;
  if (n) {
    if (t & 16)
      return n[0];
    if (t & 32 && R(n.default))
      return n.default();
  }
}
function ls(e, t) {
  if (e.shapeFlag & 6 && e.component) {
    e.transition = t;
    const n = e.component.subTree;
    ls(
      hn(n.type) && Or(n) || n,
      t
    );
  } else e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function no(e, t) {
  return R(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    se({ name: e.name }, t, { setup: e })
  ) : e;
}
function Ar(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function ws(e, t) {
  let n;
  return !!((n = Object.getOwnPropertyDescriptor(e, t)) && !n.configurable);
}
const tn = /* @__PURE__ */ new WeakMap();
function Et(e, t, n, s, r = !1) {
  if (M(e)) {
    e.forEach(
      (I, X) => Et(
        I,
        t && (M(t) ? t[X] : t),
        n,
        s,
        r
      )
    );
    return;
  }
  if (Ot(s) && !r) {
    s.shapeFlag & 512 && s.type.__asyncResolved && s.component.subTree.component && Et(e, t, n, s.component.subTree);
    return;
  }
  const i = s.shapeFlag & 4 ? _n(s.component) : s.el, o = r ? null : i, { i: l, r: c } = e, d = t && t.r, a = l.refs === K ? l.refs = {} : l.refs, p = l.setupState, w = /* @__PURE__ */ D(p), C = p === K ? Ys : (I) => ws(a, I) ? !1 : j(w, I), N = (I, X) => !(X && ws(a, X));
  if (d != null && d !== c) {
    if (Cs(t), k(d))
      a[d] = null, C(d) && (p[d] = null);
    else if (/* @__PURE__ */ oe(d)) {
      const I = t;
      N(d, I.k) && (d.value = null), I.k && (a[I.k] = null);
    }
  }
  if (R(c))
    Nt(c, l, 12, [o, a]);
  else {
    const I = k(c), X = /* @__PURE__ */ oe(c);
    if (I || X) {
      const G = () => {
        if (e.f) {
          const V = I ? C(c) ? p[c] : a[c] : N() || !e.k ? c.value : a[e.k];
          if (r)
            M(V) && zn(V, i);
          else if (M(V))
            V.includes(i) || V.push(i);
          else if (I)
            a[c] = [i], C(c) && (p[c] = a[c]);
          else {
            const H = [i];
            N(c, e.k) && (c.value = H), e.k && (a[e.k] = H);
          }
        } else I ? (a[c] = o, C(c) && (p[c] = o)) : X && (N(c, e.k) && (c.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const V = () => {
          G(), tn.delete(e);
        };
        V.id = -1, tn.set(e, V), ae(V, n);
      } else
        Cs(e), G();
    }
  }
}
function Cs(e) {
  const t = tn.get(e);
  t && (t.flags |= 8, tn.delete(e));
}
un().requestIdleCallback;
un().cancelIdleCallback;
const Ot = (e) => !!e.type.__asyncLoader, fs = (e) => e.type.__isKeepAlive;
function so(e, t) {
  Pr(e, "a", t);
}
function ro(e, t) {
  Pr(e, "da", t);
}
function Pr(e, t, n = ue) {
  const s = e.__wdc || (e.__wdc = () => {
    let r = n;
    for (; r; ) {
      if (r.isDeactivated)
        return;
      r = r.parent;
    }
    return e();
  });
  if (pn(t, s, n), n) {
    let r = n.parent;
    for (; r && r.parent; )
      fs(r.parent.vnode) && io(s, t, n, r), r = r.parent;
  }
}
function io(e, t, n, s) {
  const r = pn(
    t,
    e,
    s,
    !0
    /* prepend */
  );
  Mr(() => {
    zn(s[t], r);
  }, n);
}
function pn(e, t, n = ue, s = !1) {
  if (n) {
    const r = n[e] || (n[e] = []), i = t.__weh || (t.__weh = (...o) => {
      $e();
      const l = Ht(n), c = ye(t, n, e, o);
      return l(), Le(), c;
    });
    return s ? r.unshift(i) : r.push(i), i;
  }
}
const Be = (e) => (t, n = ue) => {
  (!Dt || e === "sp") && pn(e, (...s) => t(...s), n);
}, oo = Be("bm"), lo = Be("m"), fo = Be(
  "bu"
), co = Be("u"), uo = Be(
  "bum"
), Mr = Be("um"), ao = Be(
  "sp"
), ho = Be("rtg"), po = Be("rtc");
function go(e, t = ue) {
  pn("ec", e, t);
}
const mo = /* @__PURE__ */ Symbol.for("v-ndc"), Un = (e) => e ? Zr(e) ? _n(e) : Un(e.parent) : null, At = (
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
    $parent: (e) => Un(e.parent),
    $root: (e) => Un(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Rr(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      os(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = Ji.bind(e.proxy)),
    $watch: (e) => Qi.bind(e)
  })
), On = (e, t) => e !== K && !e.__isScriptSetup && j(e, t), _o = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: n, setupState: s, data: r, props: i, accessCache: o, type: l, appContext: c } = e;
    if (t[0] !== "$") {
      const w = o[t];
      if (w !== void 0)
        switch (w) {
          case 1:
            return s[t];
          case 2:
            return r[t];
          case 4:
            return n[t];
          case 3:
            return i[t];
        }
      else {
        if (On(s, t))
          return o[t] = 1, s[t];
        if (r !== K && j(r, t))
          return o[t] = 2, r[t];
        if (j(i, t))
          return o[t] = 3, i[t];
        if (n !== K && j(n, t))
          return o[t] = 4, n[t];
        $n && (o[t] = 0);
      }
    }
    const d = At[t];
    let a, p;
    if (d)
      return t === "$attrs" && ie(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (n !== K && j(n, t))
      return o[t] = 4, n[t];
    if (
      // global properties
      p = c.config.globalProperties, j(p, t)
    )
      return p[t];
  },
  set({ _: e }, t, n) {
    const { data: s, setupState: r, ctx: i } = e;
    return On(r, t) ? (r[t] = n, !0) : s !== K && j(s, t) ? (s[t] = n, !0) : j(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (i[t] = n, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: n, ctx: s, appContext: r, props: i, type: o }
  }, l) {
    let c;
    return !!(n[l] || e !== K && l[0] !== "$" && j(e, l) || On(t, l) || j(i, l) || j(s, l) || j(At, l) || j(r.config.globalProperties, l) || (c = o.__cssModules) && c[l]);
  },
  defineProperty(e, t, n) {
    return n.get != null ? e._.accessCache[t] = 0 : j(n, "value") && this.set(e, t, n.value, null), Reflect.defineProperty(e, t, n);
  }
};
function Ts(e) {
  return M(e) ? e.reduce(
    (t, n) => (t[n] = null, t),
    {}
  ) : e;
}
let $n = !0;
function bo(e) {
  const t = Rr(e), n = e.proxy, s = e.ctx;
  $n = !1, t.beforeCreate && Es(t.beforeCreate, e, "bc");
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
    mounted: w,
    beforeUpdate: C,
    updated: N,
    activated: I,
    deactivated: X,
    beforeDestroy: G,
    beforeUnmount: V,
    destroyed: H,
    unmounted: O,
    render: Q,
    renderTracked: xe,
    renderTriggered: ve,
    errorCaptured: qe,
    serverPrefetch: Ut,
    // public API
    expose: Ye,
    inheritAttrs: gt,
    // assets
    components: $t,
    directives: Lt,
    filters: bn
  } = t;
  if (d && yo(d, s, null), o)
    for (const Y in o) {
      const B = o[Y];
      R(B) && (s[Y] = B.bind(n));
    }
  if (r) {
    const Y = r.call(n, n);
    W(Y) && (e.data = /* @__PURE__ */ an(Y));
  }
  if ($n = !0, i)
    for (const Y in i) {
      const B = i[Y], ke = R(B) ? B.bind(n, n) : R(B.get) ? B.get.bind(n, n) : Ie, Kt = !R(B) && R(B.set) ? B.set.bind(n) : Ie, Xe = ei({
        get: ke,
        set: Kt
      });
      Object.defineProperty(s, Y, {
        enumerable: !0,
        configurable: !0,
        get: () => Xe.value,
        set: (Se) => Xe.value = Se
      });
    }
  if (l)
    for (const Y in l)
      Ir(l[Y], s, n, Y);
  if (c) {
    const Y = R(c) ? c.call(n) : c;
    Reflect.ownKeys(Y).forEach((B) => {
      ki(B, Y[B]);
    });
  }
  a && Es(a, e, "c");
  function le(Y, B) {
    M(B) ? B.forEach((ke) => Y(ke.bind(n))) : B && Y(B.bind(n));
  }
  if (le(oo, p), le(lo, w), le(fo, C), le(co, N), le(so, I), le(ro, X), le(go, qe), le(po, xe), le(ho, ve), le(uo, V), le(Mr, O), le(ao, Ut), M(Ye))
    if (Ye.length) {
      const Y = e.exposed || (e.exposed = {});
      Ye.forEach((B) => {
        Object.defineProperty(Y, B, {
          get: () => n[B],
          set: (ke) => n[B] = ke,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  Q && e.render === Ie && (e.render = Q), gt != null && (e.inheritAttrs = gt), $t && (e.components = $t), Lt && (e.directives = Lt), Ut && Ar(e);
}
function yo(e, t, n = Ie) {
  M(e) && (e = Ln(e));
  for (const s in e) {
    const r = e[s];
    let i;
    W(r) ? "default" in r ? i = kt(
      r.from || s,
      r.default,
      !0
    ) : i = kt(r.from || s) : i = kt(r), /* @__PURE__ */ oe(i) ? Object.defineProperty(t, s, {
      enumerable: !0,
      configurable: !0,
      get: () => i.value,
      set: (o) => i.value = o
    }) : t[s] = i;
  }
}
function Es(e, t, n) {
  ye(
    M(e) ? e.map((s) => s.bind(t.proxy)) : e.bind(t.proxy),
    t,
    n
  );
}
function Ir(e, t, n, s) {
  let r = s.includes(".") ? Er(n, s) : () => n[s];
  if (k(e)) {
    const i = t[e];
    R(i) && Tn(r, i);
  } else if (R(e))
    Tn(r, e.bind(n));
  else if (W(e))
    if (M(e))
      e.forEach((i) => Ir(i, t, n, s));
    else {
      const i = R(e.handler) ? e.handler.bind(n) : t[e.handler];
      R(i) && Tn(r, i, e);
    }
}
function Rr(e) {
  const t = e.type, { mixins: n, extends: s } = t, {
    mixins: r,
    optionsCache: i,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = i.get(t);
  let c;
  return l ? c = l : !r.length && !n && !s ? c = t : (c = {}, r.length && r.forEach(
    (d) => nn(c, d, o, !0)
  ), nn(c, t, o)), W(t) && i.set(t, c), c;
}
function nn(e, t, n, s = !1) {
  const { mixins: r, extends: i } = t;
  i && nn(e, i, n, !0), r && r.forEach(
    (o) => nn(e, o, n, !0)
  );
  for (const o in t)
    if (!(s && o === "expose")) {
      const l = xo[o] || n && n[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const xo = {
  data: Os,
  props: As,
  emits: As,
  // objects
  methods: vt,
  computed: vt,
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
  components: vt,
  directives: vt,
  // watch
  watch: So,
  // provide / inject
  provide: Os,
  inject: vo
};
function Os(e, t) {
  return t ? e ? function() {
    return se(
      R(e) ? e.call(this, this) : e,
      R(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function vo(e, t) {
  return vt(Ln(e), Ln(t));
}
function Ln(e) {
  if (M(e)) {
    const t = {};
    for (let n = 0; n < e.length; n++)
      t[e[n]] = e[n];
    return t;
  }
  return e;
}
function fe(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function vt(e, t) {
  return e ? se(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function As(e, t) {
  return e ? M(e) && M(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : se(
    /* @__PURE__ */ Object.create(null),
    Ts(e),
    Ts(t ?? {})
  ) : t;
}
function So(e, t) {
  if (!e) return t;
  if (!t) return e;
  const n = se(/* @__PURE__ */ Object.create(null), e);
  for (const s in t)
    n[s] = fe(e[s], t[s]);
  return n;
}
function Fr() {
  return {
    app: null,
    config: {
      isNativeTag: Ys,
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
let wo = 0;
function Co(e, t) {
  return function(s, r = null) {
    R(s) || (s = se({}, s)), r != null && !W(r) && (r = null);
    const i = Fr(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let c = !1;
    const d = i.app = {
      _uid: wo++,
      _component: s,
      _props: r,
      _container: null,
      _context: i,
      _instance: null,
      version: sl,
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
      mount(a, p, w) {
        if (!c) {
          const C = d._ceVNode || Ue(s, r);
          return C.appContext = i, w === !0 ? w = "svg" : w === !1 && (w = void 0), e(C, a, w), c = !0, d._container = a, a.__vue_app__ = d, _n(C.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        c && (ye(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, p) {
        return i.provides[a] = p, d;
      },
      runWithContext(a) {
        const p = at;
        at = d;
        try {
          return a();
        } finally {
          at = p;
        }
      }
    };
    return d;
  };
}
let at = null;
const To = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${me(t)}Modifiers`] || e[`${it(t)}Modifiers`];
function Eo(e, t, ...n) {
  if (e.isUnmounted) return;
  const s = e.vnode.props || K;
  let r = n;
  const i = t.startsWith("update:"), o = i && To(s, t.slice(7));
  o && (o.trim && (r = n.map((a) => k(a) ? a.trim() : a)), o.number && (r = r.map(Yn)));
  let l, c = s[l = xn(t)] || // also try camelCase event handler (#2249)
  s[l = xn(me(t))];
  !c && i && (c = s[l = xn(it(t))]), c && ye(
    c,
    e,
    6,
    r
  );
  const d = s[l + "Once"];
  if (d) {
    if (!e.emitted)
      e.emitted = {};
    else if (e.emitted[l])
      return;
    e.emitted[l] = !0, ye(
      d,
      e,
      6,
      r
    );
  }
}
const Oo = /* @__PURE__ */ new WeakMap();
function Vr(e, t, n = !1) {
  const s = n ? Oo : t.emitsCache, r = s.get(e);
  if (r !== void 0)
    return r;
  const i = e.emits;
  let o = {}, l = !1;
  if (!R(e)) {
    const c = (d) => {
      const a = Vr(d, t, !0);
      a && (l = !0, se(o, a));
    };
    !n && t.mixins.length && t.mixins.forEach(c), e.extends && c(e.extends), e.mixins && e.mixins.forEach(c);
  }
  return !i && !l ? (W(e) && s.set(e, null), null) : (M(i) ? i.forEach((c) => o[c] = null) : se(o, i), W(e) && s.set(e, o), o);
}
function gn(e, t) {
  return !e || !ln(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), j(e, t[0].toLowerCase() + t.slice(1)) || j(e, it(t)) || j(e, t));
}
function Ps(e) {
  const {
    type: t,
    vnode: n,
    proxy: s,
    withProxy: r,
    propsOptions: [i],
    slots: o,
    attrs: l,
    emit: c,
    render: d,
    renderCache: a,
    props: p,
    data: w,
    setupState: C,
    ctx: N,
    inheritAttrs: I
  } = e, X = en(e);
  let G, V;
  try {
    if (n.shapeFlag & 4) {
      const O = r || s, Q = O;
      G = Pe(
        d.call(
          Q,
          O,
          a,
          p,
          C,
          w,
          N
        )
      ), V = l;
    } else {
      const O = t;
      G = Pe(
        O.length > 1 ? O(
          p,
          { attrs: l, slots: o, emit: c }
        ) : O(
          p,
          null
        )
      ), V = t.props ? l : Ao(l);
    }
  } catch (O) {
    rt.length = 0, dn(O, e, 1), G = Ue(We);
  }
  let H = G;
  if (V && I !== !1) {
    const O = Object.keys(V), { shapeFlag: Q } = H;
    O.length && Q & 7 && (i && O.some(fn) && (V = Po(
      V,
      i
    )), H = ht(H, V, !1, !0));
  }
  if (n.dirs && (H = ht(H, null, !1, !0), H.dirs = H.dirs ? H.dirs.concat(n.dirs) : n.dirs), n.transition) {
    const O = hn(H.type) && Or(H) || H;
    ls(O, n.transition);
  }
  return G = H, en(X), G;
}
const Ao = (e) => {
  let t;
  for (const n in e)
    (n === "class" || n === "style" || ln(n)) && ((t || (t = {}))[n] = e[n]);
  return t;
}, Po = (e, t) => {
  const n = {};
  for (const s in e)
    (!fn(s) || !(s.slice(9) in t)) && (n[s] = e[s]);
  return n;
};
function Mo(e, t, n) {
  const { props: s, children: r, component: i } = e, { props: o, children: l, patchFlag: c } = t, d = i.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (n && c >= 0) {
    if (c & 1024)
      return !0;
    if (c & 16)
      return s ? Ms(s, o, d) : !!o;
    if (c & 8) {
      const a = t.dynamicProps;
      for (let p = 0; p < a.length; p++) {
        const w = a[p];
        if (Dr(o, s, w) && !gn(d, w))
          return !0;
      }
    }
  } else
    return (r || l) && (!l || !l.$stable) ? !0 : s === o ? !1 : s ? o ? Ms(s, o, d) : !0 : !!o;
  return !1;
}
function Ms(e, t, n) {
  const s = Object.keys(t);
  if (s.length !== Object.keys(e).length)
    return !0;
  for (let r = 0; r < s.length; r++) {
    const i = s[r];
    if (Dr(t, e, i) && !gn(n, i))
      return !0;
  }
  return !1;
}
function Dr(e, t, n) {
  const s = e[n], r = t[n];
  return n === "style" && W(s) && W(r) ? !pt(s, r) : s !== r;
}
function Io({ vnode: e, parent: t, suspense: n }, s) {
  for (; t; ) {
    const r = t.subTree;
    if (r.suspense && r.suspense.activeBranch === e && (r.suspense.vnode.el = r.el = s, e = r), r === e)
      (e = t.vnode).el = s, t = t.parent;
    else
      break;
  }
  n && n.activeBranch === e && (n.vnode.el = s);
}
const jr = {}, Nr = () => Object.create(jr), Hr = (e) => Object.getPrototypeOf(e) === jr;
function Ro(e, t, n, s = !1) {
  const r = {}, i = Nr();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), Ur(e, t, r, i);
  for (const o in e.propsOptions[0])
    o in r || (r[o] = void 0);
  n ? e.props = s ? r : /* @__PURE__ */ Di(r) : e.type.props ? e.props = r : e.props = i, e.attrs = i;
}
function Fo(e, t, n, s) {
  const {
    props: r,
    attrs: i,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ D(r), [c] = e.propsOptions;
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
        let w = a[p];
        if (gn(e.emitsOptions, w))
          continue;
        const C = t[w];
        if (c)
          if (j(i, w))
            C !== i[w] && (i[w] = C, d = !0);
          else {
            const N = me(w);
            r[N] = Kn(
              c,
              l,
              N,
              C,
              e,
              !1
            );
          }
        else
          C !== i[w] && (i[w] = C, d = !0);
      }
    }
  } else {
    Ur(e, t, r, i) && (d = !0);
    let a;
    for (const p in l)
      (!t || // for camelCase
      !j(t, p) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = it(p)) === p || !j(t, a))) && (c ? n && // for camelCase
      (n[p] !== void 0 || // for kebab-case
      n[a] !== void 0) && (r[p] = Kn(
        c,
        l,
        p,
        void 0,
        e,
        !0
      )) : delete r[p]);
    if (i !== l)
      for (const p in i)
        (!t || !j(t, p)) && (delete i[p], d = !0);
  }
  d && Ne(e.attrs, "set", "");
}
function Ur(e, t, n, s) {
  const [r, i] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let c in t) {
      if (wt(c))
        continue;
      const d = t[c];
      let a;
      r && j(r, a = me(c)) ? !i || !i.includes(a) ? n[a] = d : (l || (l = {}))[a] = d : gn(e.emitsOptions, c) || (!(c in s) || d !== s[c]) && (s[c] = d, o = !0);
    }
  if (i) {
    const c = /* @__PURE__ */ D(n), d = l || K;
    for (let a = 0; a < i.length; a++) {
      const p = i[a];
      n[p] = Kn(
        r,
        c,
        p,
        d[p],
        e,
        !j(d, p)
      );
    }
  }
  return o;
}
function Kn(e, t, n, s, r, i) {
  const o = e[n];
  if (o != null) {
    const l = j(o, "default");
    if (l && s === void 0) {
      const c = o.default;
      if (o.type !== Function && !o.skipFactory && R(c)) {
        const { propsDefaults: d } = r;
        if (n in d)
          s = d[n];
        else {
          const a = Ht(r);
          s = d[n] = c.call(
            null,
            t
          ), a();
        }
      } else
        s = c;
      r.ce && r.ce._setProp(n, s);
    }
    o[
      0
      /* shouldCast */
    ] && (i && !l ? s = !1 : o[
      1
      /* shouldCastTrue */
    ] && (s === "" || s === it(n)) && (s = !0));
  }
  return s;
}
const Vo = /* @__PURE__ */ new WeakMap();
function $r(e, t, n = !1) {
  const s = n ? Vo : t.propsCache, r = s.get(e);
  if (r)
    return r;
  const i = e.props, o = {}, l = [];
  let c = !1;
  if (!R(e)) {
    const a = (p) => {
      c = !0;
      const [w, C] = $r(p, t, !0);
      se(o, w), C && l.push(...C);
    };
    !n && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!i && !c)
    return W(e) && s.set(e, ft), ft;
  if (M(i))
    for (let a = 0; a < i.length; a++) {
      const p = me(i[a]);
      Is(p) && (o[p] = K);
    }
  else if (i)
    for (const a in i) {
      const p = me(a);
      if (Is(p)) {
        const w = i[a], C = o[p] = M(w) || R(w) ? { type: w } : se({}, w), N = C.type;
        let I = !1, X = !0;
        if (M(N))
          for (let G = 0; G < N.length; ++G) {
            const V = N[G], H = R(V) && V.name;
            if (H === "Boolean") {
              I = !0;
              break;
            } else H === "String" && (X = !1);
          }
        else
          I = R(N) && N.name === "Boolean";
        C[
          0
          /* shouldCast */
        ] = I, C[
          1
          /* shouldCastTrue */
        ] = X, (I || j(C, "default")) && l.push(p);
      }
    }
  const d = [o, l];
  return W(e) && s.set(e, d), d;
}
function Is(e) {
  return e[0] !== "$" && !wt(e);
}
const cs = (e) => e === "_" || e === "_ctx" || e === "$stable", us = (e) => M(e) ? e.map(Pe) : [Pe(e)], Do = (e, t, n) => {
  if (t._n)
    return t;
  const s = Yi((...r) => us(t(...r)), n);
  return s._c = !1, s;
}, Lr = (e, t, n) => {
  const s = e._ctx;
  for (const r in e) {
    if (cs(r)) continue;
    const i = e[r];
    if (R(i))
      t[r] = Do(r, i, s);
    else if (i != null) {
      const o = us(i);
      t[r] = () => o;
    }
  }
}, Kr = (e, t) => {
  const n = us(t);
  e.slots.default = () => n;
}, Wr = (e, t, n) => {
  for (const s in t)
    (n || !cs(s)) && (e[s] = t[s]);
}, jo = (e, t, n) => {
  const s = e.slots = Nr();
  if (e.vnode.shapeFlag & 32) {
    const r = t._;
    r ? (Wr(s, t, n), n && er(s, "_", r, !0)) : Lr(t, s);
  } else t && Kr(e, t);
}, No = (e, t, n) => {
  const { vnode: s, slots: r } = e;
  let i = !0, o = K;
  if (s.shapeFlag & 32) {
    const l = t._;
    l ? n && l === 1 ? i = !1 : Wr(r, t, n) : (i = !t.$stable, Lr(t, r)), o = t;
  } else t && (Kr(e, t), o = { default: 1 });
  if (i)
    for (const l in r)
      !cs(l) && o[l] == null && delete r[l];
}, ae = Ko;
function Ho(e) {
  return Uo(e);
}
function Uo(e, t) {
  const n = un();
  n.__VUE__ = !0;
  const {
    insert: s,
    remove: r,
    patchProp: i,
    createElement: o,
    createText: l,
    createComment: c,
    setText: d,
    setElementText: a,
    parentNode: p,
    nextSibling: w,
    setScopeId: C = Ie,
    insertStaticContent: N
  } = e, I = (f, u, h, b = null, _ = null, g = null, v = void 0, x = null, y = !!u.dynamicChildren) => {
    if (f === u)
      return;
    f && !xt(f, u) && (b = Wt(f), Se(f, _, g, !0), f = null), u.patchFlag === -2 && (y = !1, u.dynamicChildren = null);
    const { type: m, ref: E, shapeFlag: S } = u;
    switch (m) {
      case mn:
        X(f, u, h, b);
        break;
      case We:
        G(f, u, h, b);
        break;
      case Pn:
        f == null && V(u, h, b, v);
        break;
      case De:
        $t(
          f,
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
        S & 1 ? Q(
          f,
          u,
          h,
          b,
          _,
          g,
          v,
          x,
          y
        ) : S & 6 ? Lt(
          f,
          u,
          h,
          b,
          _,
          g,
          v,
          x,
          y
        ) : (S & 64 || S & 128) && m.process(
          f,
          u,
          h,
          b,
          _,
          g,
          v,
          x,
          y,
          _t
        );
    }
    E != null && _ ? Et(E, f && f.ref, g, u || f, !u) : E == null && f && f.ref != null && Et(f.ref, null, g, f, !0);
  }, X = (f, u, h, b) => {
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
  }, G = (f, u, h, b) => {
    f == null ? s(
      u.el = c(u.children || ""),
      h,
      b
    ) : u.el = f.el;
  }, V = (f, u, h, b) => {
    [f.el, f.anchor] = N(
      f.children,
      u,
      h,
      b,
      f.el,
      f.anchor
    );
  }, H = ({ el: f, anchor: u }, h, b) => {
    let _;
    for (; f && f !== u; )
      _ = w(f), s(f, h, b), f = _;
    s(u, h, b);
  }, O = ({ el: f, anchor: u }) => {
    let h;
    for (; f && f !== u; )
      h = w(f), r(f), f = h;
    r(u);
  }, Q = (f, u, h, b, _, g, v, x, y) => {
    if (u.type === "svg" ? v = "svg" : u.type === "math" && (v = "mathml"), f == null)
      xe(
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
      const m = f.el && f.el._isVueCE ? f.el : null;
      try {
        m && m._beginPatch(), Ut(
          f,
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
  }, xe = (f, u, h, b, _, g, v, x) => {
    let y, m;
    const { props: E, shapeFlag: S, transition: T, dirs: P } = f;
    if (y = f.el = o(
      f.type,
      g,
      E && E.is,
      E
    ), S & 8 ? a(y, f.children) : S & 16 && qe(
      f.children,
      y,
      null,
      b,
      _,
      An(f, g),
      v,
      x
    ), P && Ze(f, null, b, "created"), ve(y, f, f.scopeId, v, b), E) {
      for (const $ in E)
        $ !== "value" && !wt($) && i(y, $, null, E[$], g, b);
      "value" in E && i(y, "value", null, E.value, g), (m = E.onVnodeBeforeMount) && Ee(m, b, f);
    }
    P && Ze(f, null, b, "beforeMount");
    const F = $o(_, T);
    F && T.beforeEnter(y), s(y, u, h), ((m = E && E.onVnodeMounted) || F || P) && ae(() => {
      try {
        m && Ee(m, b, f), F && T.enter(y), P && Ze(f, null, b, "mounted");
      } finally {
      }
    }, _);
  }, ve = (f, u, h, b, _) => {
    if (h && C(f, h), b)
      for (let g = 0; g < b.length; g++)
        C(f, b[g]);
    if (_) {
      let g = _.subTree;
      if (u === g || zr(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const v = _.vnode;
        ve(
          f,
          v,
          v.scopeId,
          v.slotScopeIds,
          _.parent
        );
      }
    }
  }, qe = (f, u, h, b, _, g, v, x, y = 0) => {
    for (let m = y; m < f.length; m++) {
      const E = f[m] = x ? je(f[m]) : Pe(f[m]);
      I(
        null,
        E,
        u,
        h,
        b,
        _,
        g,
        v,
        x
      );
    }
  }, Ut = (f, u, h, b, _, g, v) => {
    const x = u.el = f.el;
    let { patchFlag: y, dynamicChildren: m, dirs: E } = u;
    y |= f.patchFlag & 16;
    const S = f.props || K, T = u.props || K;
    let P;
    if (h && Qe(h, !1), (P = T.onVnodeBeforeUpdate) && Ee(P, h, u, f), E && Ze(u, f, h, "beforeUpdate"), h && Qe(h, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    m && (!f.dynamicChildren || f.dynamicChildren.length !== m.length) && (y = 0, v = !1, m = null), (S.innerHTML && T.innerHTML == null || S.textContent && T.textContent == null) && a(x, ""), m ? Ye(
      f.dynamicChildren,
      m,
      x,
      h,
      b,
      An(u, _),
      g
    ) : v || B(
      f,
      u,
      x,
      null,
      h,
      b,
      An(u, _),
      g,
      !1
    ), y > 0) {
      if (y & 16)
        gt(x, S, T, h, _);
      else if (y & 2 && S.class !== T.class && i(x, "class", null, T.class, _), y & 4 && i(x, "style", S.style, T.style, _), y & 8) {
        const F = u.dynamicProps;
        for (let $ = 0; $ < F.length; $++) {
          const U = F[$], Z = S[U], ee = T[U];
          (ee !== Z || U === "value") && i(x, U, Z, ee, _, h);
        }
      }
      y & 1 && f.children !== u.children && a(x, u.children);
    } else !v && m == null && gt(x, S, T, h, _);
    ((P = T.onVnodeUpdated) || E) && ae(() => {
      P && Ee(P, h, u, f), E && Ze(u, f, h, "updated");
    }, b);
  }, Ye = (f, u, h, b, _, g, v) => {
    for (let x = 0; x < u.length; x++) {
      const y = f[x], m = u[x], E = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        y.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (y.type === De || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !xt(y, m) || // - In the case of a component, it could contain anything.
        y.shapeFlag & 198) ? p(y.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          h
        )
      );
      I(
        y,
        m,
        E,
        null,
        b,
        _,
        g,
        v,
        !0
      );
    }
  }, gt = (f, u, h, b, _) => {
    if (u !== h) {
      if (u !== K)
        for (const g in u)
          !wt(g) && !(g in h) && i(
            f,
            g,
            u[g],
            null,
            _,
            b
          );
      for (const g in h) {
        if (wt(g)) continue;
        const v = h[g], x = u[g];
        v !== x && g !== "value" && i(f, g, x, v, _, b);
      }
      "value" in h && i(f, "value", u.value, h.value, _);
    }
  }, $t = (f, u, h, b, _, g, v, x, y) => {
    const m = u.el = f ? f.el : l(""), E = u.anchor = f ? f.anchor : l("");
    let { patchFlag: S, dynamicChildren: T, slotScopeIds: P } = u;
    P && (x = x ? x.concat(P) : P), f == null ? (s(m, h, b), s(E, h, b), qe(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      h,
      E,
      _,
      g,
      v,
      x,
      y
    )) : S > 0 && S & 64 && T && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    f.dynamicChildren && f.dynamicChildren.length === T.length ? (Ye(
      f.dynamicChildren,
      T,
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
      f,
      u,
      !0
      /* shallow */
    )) : B(
      f,
      u,
      h,
      E,
      _,
      g,
      v,
      x,
      y
    );
  }, Lt = (f, u, h, b, _, g, v, x, y) => {
    u.slotScopeIds = x, f == null ? u.shapeFlag & 512 ? _.ctx.activate(
      u,
      h,
      b,
      v,
      y
    ) : bn(
      u,
      h,
      b,
      _,
      g,
      v,
      y
    ) : as(f, u, y);
  }, bn = (f, u, h, b, _, g, v) => {
    const x = f.component = ko(
      f,
      b,
      _
    );
    if (fs(f) && (x.ctx.renderer = _t), Zo(x, !1, v), x.asyncDep) {
      if (_ && _.registerDep(x, le, v), !f.el) {
        const y = x.subTree = Ue(We);
        G(null, y, u, h), f.placeholder = y.el;
      }
    } else
      le(
        x,
        f,
        u,
        h,
        _,
        g,
        v
      );
  }, as = (f, u, h) => {
    const b = u.component = f.component;
    if (Mo(f, u, h))
      if (b.asyncDep && !b.asyncResolved) {
        Y(b, u, h);
        return;
      } else
        b.next = u, b.update();
    else
      u.el = f.el, b.vnode = u;
  }, le = (f, u, h, b, _, g, v) => {
    const x = () => {
      if (f.isMounted) {
        let { next: S, bu: T, u: P, parent: F, vnode: $ } = f;
        {
          const Ce = qr(f);
          if (Ce) {
            S && (S.el = $.el, Y(f, S, v)), Ce.asyncDep.then(() => {
              ae(() => {
                f.isUnmounted || m();
              }, _);
            });
            return;
          }
        }
        let U = S, Z;
        Qe(f, !1), S ? (S.el = $.el, Y(f, S, v)) : S = $, T && Yt(T), (Z = S.props && S.props.onVnodeBeforeUpdate) && Ee(Z, F, S, $), Qe(f, !0);
        const ee = Ps(f), we = f.subTree;
        f.subTree = ee, I(
          we,
          ee,
          // parent may have changed if it's in a teleport
          p(we.el),
          // anchor may have changed if it's in a fragment
          Wt(we),
          f,
          _,
          g
        ), S.el = ee.el, U === null && Io(f, ee.el), P && ae(P, _), (Z = S.props && S.props.onVnodeUpdated) && ae(
          () => Ee(Z, F, S, $),
          _
        );
      } else {
        let S;
        const { el: T, props: P } = u, { bm: F, m: $, parent: U, root: Z, type: ee } = f, we = Ot(u);
        Qe(f, !1), F && Yt(F), !we && (S = P && P.onVnodeBeforeMount) && Ee(S, U, u), Qe(f, !0);
        {
          Z.ce && Z.ce._hasShadowRoot() && Z.ce._injectChildStyle(
            ee,
            f.parent ? f.parent.type : void 0
          );
          const Ce = f.subTree = Ps(f);
          I(
            null,
            Ce,
            h,
            b,
            f,
            _,
            g
          ), u.el = Ce.el;
        }
        if ($ && ae($, _), !we && (S = P && P.onVnodeMounted)) {
          const Ce = u;
          ae(
            () => Ee(S, U, Ce),
            _
          );
        }
        (u.shapeFlag & 256 || U && Ot(U.vnode) && U.vnode.shapeFlag & 256) && f.a && ae(f.a, _), f.isMounted = !0, u = h = b = null;
      }
    };
    f.scope.on();
    const y = f.effect = new ir(x);
    f.scope.off();
    const m = f.update = y.run.bind(y), E = f.job = y.runIfDirty.bind(y);
    E.i = f, E.id = f.uid, y.scheduler = () => os(E), Qe(f, !0), m();
  }, Y = (f, u, h) => {
    u.component = f;
    const b = f.vnode.props;
    f.vnode = u, f.next = null, Fo(f, u.props, b, h), No(f, u.children, h), $e(), Ss(f), Le();
  }, B = (f, u, h, b, _, g, v, x, y = !1) => {
    const m = f && f.children, E = f ? f.shapeFlag : 0, S = u.children, { patchFlag: T, shapeFlag: P } = u;
    if (T > 0) {
      if (T & 128) {
        Kt(
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
      } else if (T & 256) {
        ke(
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
    P & 8 ? (E & 16 && mt(m, _, g), S !== m && a(h, S)) : E & 16 ? P & 16 ? Kt(
      m,
      S,
      h,
      b,
      _,
      g,
      v,
      x,
      y
    ) : mt(m, _, g, !0) : (E & 8 && a(h, ""), P & 16 && qe(
      S,
      h,
      b,
      _,
      g,
      v,
      x,
      y
    ));
  }, ke = (f, u, h, b, _, g, v, x, y) => {
    f = f || ft, u = u || ft;
    const m = f.length, E = u.length, S = Math.min(m, E);
    let T;
    for (T = 0; T < S; T++) {
      const P = u[T] = y ? je(u[T]) : Pe(u[T]);
      I(
        f[T],
        P,
        h,
        null,
        _,
        g,
        v,
        x,
        y
      );
    }
    m > E ? mt(
      f,
      _,
      g,
      !0,
      !1,
      S
    ) : qe(
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
  }, Kt = (f, u, h, b, _, g, v, x, y) => {
    let m = 0;
    const E = u.length;
    let S = f.length - 1, T = E - 1;
    for (; m <= S && m <= T; ) {
      const P = f[m], F = u[m] = y ? je(u[m]) : Pe(u[m]);
      if (xt(P, F))
        I(
          P,
          F,
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
    for (; m <= S && m <= T; ) {
      const P = f[S], F = u[T] = y ? je(u[T]) : Pe(u[T]);
      if (xt(P, F))
        I(
          P,
          F,
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
      S--, T--;
    }
    if (m > S) {
      if (m <= T) {
        const P = T + 1, F = P < E ? u[P].el : b;
        for (; m <= T; )
          I(
            null,
            u[m] = y ? je(u[m]) : Pe(u[m]),
            h,
            F,
            _,
            g,
            v,
            x,
            y
          ), m++;
      }
    } else if (m > T)
      for (; m <= S; )
        Se(f[m], _, g, !0), m++;
    else {
      const P = m, F = m, $ = /* @__PURE__ */ new Map();
      for (m = F; m <= T; m++) {
        const de = u[m] = y ? je(u[m]) : Pe(u[m]);
        de.key != null && $.set(de.key, m);
      }
      let U, Z = 0;
      const ee = T - F + 1;
      let we = !1, Ce = 0;
      const bt = new Array(ee);
      for (m = 0; m < ee; m++) bt[m] = 0;
      for (m = P; m <= S; m++) {
        const de = f[m];
        if (Z >= ee) {
          Se(de, _, g, !0);
          continue;
        }
        let Te;
        if (de.key != null)
          Te = $.get(de.key);
        else
          for (U = F; U <= T; U++)
            if (bt[U - F] === 0 && xt(de, u[U])) {
              Te = U;
              break;
            }
        Te === void 0 ? Se(de, _, g, !0) : (bt[Te - F] = m + 1, Te >= Ce ? Ce = Te : we = !0, I(
          de,
          u[Te],
          h,
          null,
          _,
          g,
          v,
          x,
          y
        ), Z++);
      }
      const ps = we ? Lo(bt) : ft;
      for (U = ps.length - 1, m = ee - 1; m >= 0; m--) {
        const de = F + m, Te = u[de], gs = u[de + 1], ms = de + 1 < E ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          gs.el || Jr(gs)
        ) : b;
        bt[m] === 0 ? I(
          null,
          Te,
          h,
          ms,
          _,
          g,
          v,
          x,
          y
        ) : we && (U < 0 || m !== ps[U] ? Xe(Te, h, ms, 2) : U--);
      }
    }
  }, Xe = (f, u, h, b, _ = null) => {
    const { el: g, type: v, transition: x, children: y, shapeFlag: m } = f;
    if (m & 6) {
      Xe(f.component.subTree, u, h, b);
      return;
    }
    if (m & 128) {
      f.suspense.move(u, h, b);
      return;
    }
    if (m & 64) {
      v.move(f, u, h, _t);
      return;
    }
    if (v === De) {
      s(g, u, h);
      for (let S = 0; S < y.length; S++)
        Xe(y[S], u, h, b);
      s(f.anchor, u, h);
      return;
    }
    if (v === Pn) {
      H(f, u, h);
      return;
    }
    if (b !== 2 && m & 1 && x)
      if (b === 0)
        x.persisted && !g[En] ? s(g, u, h) : (x.beforeEnter(g), s(g, u, h), ae(() => x.enter(g), _));
      else {
        const { leave: S, delayLeave: T, afterLeave: P } = x, F = () => {
          f.ctx.isUnmounted ? r(g) : s(g, u, h);
        }, $ = () => {
          const U = g._isLeaving || !!g[En];
          g._isLeaving && g[En](
            !0
            /* cancelled */
          ), x.persisted && !U ? F() : S(g, () => {
            F(), P && P();
          });
        };
        T ? T(g, F, $) : $();
      }
    else
      s(g, u, h);
  }, Se = (f, u, h, b = !1, _ = !1) => {
    const {
      type: g,
      props: v,
      ref: x,
      children: y,
      dynamicChildren: m,
      shapeFlag: E,
      patchFlag: S,
      dirs: T,
      cacheIndex: P,
      memo: F
    } = f;
    if (S === -2 && (_ = !1), x != null && ($e(), Et(x, null, h, f, !0), Le()), P != null && (u.renderCache[P] = void 0), E & 256) {
      u.ctx.deactivate(f);
      return;
    }
    const $ = E & 1 && T, U = !Ot(f);
    let Z;
    if (U && (Z = v && v.onVnodeBeforeUnmount) && Ee(Z, u, f), E & 6)
      ri(f.component, h, b);
    else {
      if (E & 128) {
        f.suspense.unmount(h, b);
        return;
      }
      $ && Ze(f, null, u, "beforeUnmount"), E & 64 ? f.type.remove(
        f,
        u,
        h,
        _t,
        b
      ) : m && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !m.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== De || S > 0 && S & 64) ? mt(
        m,
        u,
        h,
        !1,
        !0
      ) : (g === De && S & 384 || !_ && E & 16) && mt(y, u, h), b && ds(f);
    }
    const ee = F != null && P == null;
    (U && (Z = v && v.onVnodeUnmounted) || $ || ee) && ae(() => {
      Z && Ee(Z, u, f), $ && Ze(f, null, u, "unmounted"), ee && (f.el = null);
    }, h);
  }, ds = (f) => {
    const { type: u, el: h, anchor: b, transition: _ } = f;
    if (u === De) {
      si(h, b);
      return;
    }
    if (u === Pn) {
      O(f);
      return;
    }
    const g = () => {
      r(h), _ && !_.persisted && _.afterLeave && _.afterLeave();
    };
    if (f.shapeFlag & 1 && _ && !_.persisted) {
      const { leave: v, delayLeave: x } = _, y = () => v(h, g);
      x ? x(f.el, g, y) : y();
    } else
      g();
  }, si = (f, u) => {
    let h;
    for (; f !== u; )
      h = w(f), r(f), f = h;
    r(u);
  }, ri = (f, u, h) => {
    const { bum: b, scope: _, job: g, subTree: v, um: x, m: y, a: m } = f;
    Rs(y), Rs(m), b && Yt(b), _.stop(), g && (g.flags |= 8, Se(v, f, u, h)), x && ae(x, u), ae(() => {
      f.isUnmounted = !0;
    }, u);
  }, mt = (f, u, h, b = !1, _ = !1, g = 0) => {
    for (let v = g; v < f.length; v++)
      Se(f[v], u, h, b, _);
  }, Wt = (f) => {
    if (f.shapeFlag & 6)
      return Wt(f.component.subTree);
    if (f.shapeFlag & 128)
      return f.suspense.next();
    const u = w(f.anchor || f.el), h = u && u[eo];
    return h ? w(h) : u;
  };
  let yn = !1;
  const hs = (f, u, h) => {
    let b;
    f == null ? u._vnode && (Se(u._vnode, null, null, !0), b = u._vnode.component) : I(
      u._vnode || null,
      f,
      u,
      null,
      null,
      null,
      h
    ), u._vnode = f, yn || (yn = !0, Ss(b), Sr(), yn = !1);
  }, _t = {
    p: I,
    um: Se,
    m: Xe,
    r: ds,
    mt: bn,
    mc: qe,
    pc: B,
    pbc: Ye,
    n: Wt,
    o: e
  };
  return {
    render: hs,
    hydrate: void 0,
    createApp: Co(hs)
  };
}
function An({ type: e, props: t }, n) {
  return n === "svg" && e === "foreignObject" || n === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : n;
}
function Qe({ effect: e, job: t }, n) {
  n ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function $o(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Br(e, t, n = !1) {
  const s = e.children, r = t.children;
  if (M(s) && M(r))
    for (let i = 0; i < s.length; i++) {
      const o = s[i];
      let l = r[i];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = r[i] = je(r[i]), l.el = o.el), !n && l.patchFlag !== -2 && Br(o, l)), l.type === mn && (l.patchFlag === -1 && (l = r[i] = je(l)), l.el = o.el), l.type === We && !l.el && (l.el = o.el);
    }
}
function Lo(e) {
  const t = e.slice(), n = [0];
  let s, r, i, o, l;
  const c = e.length;
  for (s = 0; s < c; s++) {
    const d = e[s];
    if (d !== 0) {
      if (r = n[n.length - 1], e[r] < d) {
        t[s] = r, n.push(s);
        continue;
      }
      for (i = 0, o = n.length - 1; i < o; )
        l = i + o >> 1, e[n[l]] < d ? i = l + 1 : o = l;
      d < e[n[i]] && (i > 0 && (t[s] = n[i - 1]), n[i] = s);
    }
  }
  for (i = n.length, o = n[i - 1]; i-- > 0; )
    n[i] = o, o = t[o];
  return n;
}
function qr(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : qr(t);
}
function Rs(e) {
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
const zr = (e) => e.__isSuspense;
function Ko(e, t) {
  t && t.pendingBranch ? M(e) ? t.effects.push(...e) : t.effects.push(e) : Gi(e);
}
const De = /* @__PURE__ */ Symbol.for("v-fgt"), mn = /* @__PURE__ */ Symbol.for("v-txt"), We = /* @__PURE__ */ Symbol.for("v-cmt"), Pn = /* @__PURE__ */ Symbol.for("v-stc"), rt = [];
let pe = null;
function Wn(e = !1) {
  rt.push(pe = e ? null : []);
}
function Gr() {
  rt.pop(), pe = rt[rt.length - 1] || null;
}
let Ft = 1;
function Fs(e, t = !1) {
  Ft += e, e < 0 && pe && t && (pe.hasOnce = !0);
}
function Yr(e) {
  return e.dynamicChildren = Ft > 0 ? pe || ft : null, Gr(), Ft > 0 && pe && pe.push(e), e;
}
function Vs(e, t, n, s, r, i) {
  return Yr(
    A(
      e,
      t,
      n,
      s,
      r,
      i,
      !0
    )
  );
}
function Wo(e, t, n, s, r) {
  return Yr(
    Ue(
      e,
      t,
      n,
      s,
      r,
      !0
    )
  );
}
function kr(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function xt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const Xr = ({ key: e }) => e ?? null, Xt = ({
  ref: e,
  ref_key: t,
  ref_for: n
}) => (typeof e == "number" && (e = "" + e), e != null ? k(e) || /* @__PURE__ */ oe(e) || R(e) ? { i: ge, r: e, k: t, f: !!n } : e : null);
function A(e, t = null, n = null, s = 0, r = null, i = e === De ? 0 : 1, o = !1, l = !1) {
  const c = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Xr(t),
    ref: t && Xt(t),
    scopeId: Cr,
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
    shapeFlag: i,
    patchFlag: s,
    dynamicProps: r,
    dynamicChildren: null,
    appContext: null,
    ctx: ge
  };
  return l ? (sn(c, n), i & 128 && e.normalize(c)) : n && (c.shapeFlag |= k(n) ? 8 : 16), Ft > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  pe && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (c.patchFlag > 0 || i & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  c.patchFlag !== 32 && pe.push(c), c;
}
const Ue = Bo;
function Bo(e, t = null, n = null, s = 0, r = null, i = !1) {
  if ((!e || e === mo) && (e = We), kr(e)) {
    const l = ht(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return n && sn(l, n), Ft > 0 && !i && pe && (l.shapeFlag & 6 ? pe[pe.indexOf(e)] = l : pe.push(l)), l.patchFlag = -2, l;
  }
  if (nl(e) && (e = e.__vccOpts), t) {
    t = qo(t);
    let { class: l, style: c } = t;
    l && !k(l) && (t.class = Xn(l)), W(c) && (/* @__PURE__ */ is(c) && !M(c) && (c = se({}, c)), t.style = kn(c));
  }
  const o = k(e) ? 1 : zr(e) ? 128 : hn(e) ? 64 : W(e) ? 4 : R(e) ? 2 : 0;
  return A(
    e,
    t,
    n,
    s,
    r,
    o,
    i,
    !0
  );
}
function qo(e) {
  return e ? /* @__PURE__ */ is(e) || Hr(e) ? se({}, e) : e : null;
}
function ht(e, t, n = !1, s = !1) {
  const { props: r, ref: i, patchFlag: o, children: l, transition: c } = e, d = t ? zo(r || {}, t) : r, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && Xr(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      n && i ? M(i) ? i.concat(Xt(t)) : [i, Xt(t)] : Xt(t)
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
    patchFlag: t && e.type !== De ? o === -1 ? 16 : o | 16 : o,
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
    ssContent: e.ssContent && ht(e.ssContent),
    ssFallback: e.ssFallback && ht(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return c && s && ls(
    a,
    c.clone(a)
  ), a;
}
function te(e = " ", t = 0) {
  return Ue(mn, null, e, t);
}
function Jo(e = "", t = !1) {
  return t ? (Wn(), Wo(We, null, e)) : Ue(We, null, e);
}
function Pe(e) {
  return e == null || typeof e == "boolean" ? Ue(We) : M(e) ? Ue(
    De,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : kr(e) ? je(e) : Ue(mn, null, String(e));
}
function je(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : ht(e);
}
function sn(e, t) {
  let n = 0;
  const { shapeFlag: s } = e;
  if (t == null)
    t = null;
  else if (M(t))
    n = 16;
  else if (typeof t == "object")
    if (s & 65) {
      const r = t.default;
      r && (r._c && (r._d = !1), sn(e, r()), r._c && (r._d = !0));
      return;
    } else {
      n = 32;
      const r = t._;
      !r && !Hr(t) ? t._ctx = ge : r === 3 && ge && (ge.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (R(t)) {
    if (s & 65) {
      sn(e, { default: t });
      return;
    }
    t = { default: t, _ctx: ge }, n = 32;
  } else
    t = String(t), s & 64 ? (n = 16, t = [te(t)]) : n = 8;
  e.children = t, e.shapeFlag |= n;
}
function zo(...e) {
  const t = {};
  for (let n = 0; n < e.length; n++) {
    const s = e[n];
    for (const r in s)
      if (r === "class")
        t.class !== s.class && (t.class = Xn([t.class, s.class]));
      else if (r === "style")
        t.style = kn([t.style, s.style]);
      else if (ln(r)) {
        const i = t[r], o = s[r];
        o && i !== o && !(M(i) && i.includes(o)) ? t[r] = i ? [].concat(i, o) : o : o == null && i == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !fn(r) && (t[r] = o);
      } else r !== "" && (t[r] = s[r]);
  }
  return t;
}
function Ee(e, t, n, s = null) {
  ye(e, t, 7, [
    n,
    s
  ]);
}
const Go = Fr();
let Yo = 0;
function ko(e, t, n) {
  const s = e.type, r = (t ? t.appContext : e.appContext) || Go, i = {
    uid: Yo++,
    vnode: e,
    type: s,
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
    scope: new mi(
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
    propsOptions: $r(s, r),
    emitsOptions: Vr(s, r),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: K,
    // inheritAttrs
    inheritAttrs: s.inheritAttrs,
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
  return i.ctx = { _: i }, i.root = t ? t.root : i, i.emit = Eo.bind(null, i), e.ce && e.ce(i), i;
}
let ue = null;
const Xo = () => ue || ge;
let rn, Vt;
{
  const e = un(), t = (n, s) => {
    let r;
    return (r = e[n]) || (r = e[n] = []), r.push(s), (i) => {
      r.length > 1 ? r.forEach((o) => o(i)) : r[0](i);
    };
  };
  rn = t(
    "__VUE_INSTANCE_SETTERS__",
    (n) => ue = n
  ), Vt = t(
    "__VUE_SSR_SETTERS__",
    (n) => Dt = n
  );
}
const Ht = (e) => {
  const t = ue;
  return rn(e), e.scope.on(), () => {
    e.scope.off(), rn(t);
  };
}, Ds = () => {
  ue && ue.scope.off(), rn(null);
};
function Zr(e) {
  return e.vnode.shapeFlag & 4;
}
let Dt = !1;
function Zo(e, t = !1, n = !1) {
  t && Vt(t);
  const { props: s, children: r } = e.vnode, i = Zr(e);
  Ro(e, s, i, t), jo(e, r, n || t);
  const o = i ? Qo(e, t) : void 0;
  return t && Vt(!1), o;
}
function Qo(e, t) {
  const n = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, _o);
  const { setup: s } = n;
  if (s) {
    $e();
    const r = e.setupContext = s.length > 1 ? tl(e) : null, i = Ht(e), o = Nt(
      s,
      e,
      0,
      [
        e.props,
        r
      ]
    ), l = ks(o);
    if (Le(), i(), (l || e.sp) && !Ot(e) && Ar(e), l) {
      if (o.then(Ds, Ds), t)
        return o.then((c) => {
          Vt(!0);
          try {
            js(e, c, t);
          } finally {
            Vt(!1);
          }
        }).catch((c) => {
          dn(c, e, 0);
        });
      e.asyncDep = o;
    } else
      js(e, o);
  } else
    Qr(e);
}
function js(e, t, n) {
  R(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : W(t) && (e.setupState = yr(t)), Qr(e);
}
function Qr(e, t, n) {
  const s = e.type;
  e.render || (e.render = s.render || Ie);
  {
    const r = Ht(e);
    $e();
    try {
      bo(e);
    } finally {
      Le(), r();
    }
  }
}
const el = {
  get(e, t) {
    return ie(e, "get", ""), e[t];
  }
};
function tl(e) {
  const t = (n) => {
    e.exposed = n || {};
  };
  return {
    attrs: new Proxy(e.attrs, el),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function _n(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(yr(ji(e.exposed)), {
    get(t, n) {
      if (n in t)
        return t[n];
      if (n in At)
        return At[n](e);
    },
    has(t, n) {
      return n in t || n in At;
    }
  })) : e.proxy;
}
function nl(e) {
  return R(e) && "__vccOpts" in e;
}
const ei = (e, t) => /* @__PURE__ */ Ki(e, t, Dt), sl = "3.5.42";
/**
* @vue/runtime-dom v3.5.42
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Bn;
const Ns = typeof window < "u" && window.trustedTypes;
if (Ns)
  try {
    Bn = /* @__PURE__ */ Ns.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const ti = Bn ? (e) => Bn.createHTML(e) : (e) => e, rl = "http://www.w3.org/2000/svg", il = "http://www.w3.org/1998/Math/MathML", Ve = typeof document < "u" ? document : null, Hs = Ve && /* @__PURE__ */ Ve.createElement("template"), ol = {
  insert: (e, t, n) => {
    t.insertBefore(e, n || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, n, s) => {
    const r = t === "svg" ? Ve.createElementNS(rl, e) : t === "mathml" ? Ve.createElementNS(il, e) : n ? Ve.createElement(e, { is: n }) : Ve.createElement(e);
    return e === "select" && s && s.multiple != null && r.setAttribute("multiple", s.multiple), r;
  },
  createText: (e) => Ve.createTextNode(e),
  createComment: (e) => Ve.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => Ve.querySelector(e),
  setScopeId(e, t) {
    e.setAttribute(t, "");
  },
  // __UNSAFE__
  // Reason: innerHTML.
  // Static content here can only come from compiled templates.
  // As long as the user only uses trusted templates, this is safe.
  insertStaticContent(e, t, n, s, r, i) {
    const o = n ? n.previousSibling : t.lastChild;
    if (r && (r === i || r.nextSibling))
      for (; t.insertBefore(r.cloneNode(!0), n), !(r === i || !(r = r.nextSibling)); )
        ;
    else {
      Hs.innerHTML = ti(
        s === "svg" ? `<svg>${e}</svg>` : s === "mathml" ? `<math>${e}</math>` : e
      );
      const l = Hs.content;
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
}, ll = /* @__PURE__ */ Symbol("_vtc");
function fl(e, t, n) {
  const s = e[ll];
  s && (t = (t ? [t, ...s] : [...s]).join(" ")), t == null ? e.removeAttribute("class") : n ? e.setAttribute("class", t) : e.className = t;
}
const Us = /* @__PURE__ */ Symbol("_vod"), cl = /* @__PURE__ */ Symbol("_vsh"), ul = /* @__PURE__ */ Symbol(""), al = /(?:^|;)\s*display\s*:/;
function dl(e, t, n) {
  const s = e.style, r = k(n);
  let i = !1;
  if (n && !r) {
    if (t)
      if (k(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          n[l] == null && St(s, l, "");
        }
      else
        for (const o in t)
          n[o] == null && St(s, o, "");
    for (const o in n) {
      o === "display" && (i = !0);
      const l = n[o];
      l != null ? pl(
        e,
        o,
        !k(t) && t ? t[o] : void 0,
        l
      ) || St(s, o, l) : St(s, o, "");
    }
  } else if (r) {
    if (t !== n) {
      const o = s[ul];
      o && (n += ";" + o), s.cssText = n, i = al.test(n);
    }
  } else t && e.removeAttribute("style");
  Us in e && (e[Us] = i ? s.display : "", e[cl] && (s.display = "none"));
}
const zt = /\s*!important$/;
function St(e, t, n) {
  if (M(n))
    n.forEach((s) => St(e, t, s));
  else if (n == null && (n = ""), t.startsWith("--"))
    zt.test(n) ? e.setProperty(t, n.replace(zt, ""), "important") : e.setProperty(t, n);
  else {
    const s = hl(e, t);
    zt.test(n) ? e.setProperty(
      it(s),
      n.replace(zt, ""),
      "important"
    ) : e[s] = n;
  }
}
const $s = ["Webkit", "Moz", "ms"], Mn = {};
function hl(e, t) {
  const n = Mn[t];
  if (n)
    return n;
  let s = me(t);
  if (s !== "filter" && s in e)
    return Mn[t] = s;
  s = Qs(s);
  for (let r = 0; r < $s.length; r++) {
    const i = $s[r] + s;
    if (i in e)
      return Mn[t] = i;
  }
  return t;
}
function pl(e, t, n, s) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && k(s) && n === s;
}
const Ls = "http://www.w3.org/1999/xlink";
function Ks(e, t, n, s, r, i = pi(t)) {
  s && t.startsWith("xlink:") ? n == null ? e.removeAttributeNS(Ls, t.slice(6, t.length)) : e.setAttributeNS(Ls, t, n) : n == null || i && !tr(n) ? e.removeAttribute(t) : e.setAttribute(
    t,
    i ? "" : Re(n) ? String(n) : n
  );
}
function Ws(e, t, n, s, r) {
  if (t === "innerHTML" || t === "textContent") {
    n != null && (e[t] = t === "innerHTML" ? ti(n) : n);
    return;
  }
  const i = e.tagName;
  if (t === "value" && i !== "PROGRESS" && // custom elements may use _value internally
  !i.includes("-")) {
    const l = i === "OPTION" ? e.getAttribute("value") || "" : e.value, c = n == null ? (
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
    l === "boolean" ? n = tr(n) : n == null && l === "string" ? (n = "", o = !0) : l === "number" && (n = 0, o = !0);
  }
  try {
    e[t] = n;
  } catch {
  }
  o && e.removeAttribute(r || t);
}
function tt(e, t, n, s) {
  e.addEventListener(t, n, s);
}
function gl(e, t, n, s) {
  e.removeEventListener(t, n, s);
}
const Bs = /* @__PURE__ */ Symbol("_vei");
function ml(e, t, n, s, r = null) {
  const i = e[Bs] || (e[Bs] = {}), o = i[t];
  if (s && o)
    o.value = s;
  else {
    const [l, c] = yl(t);
    if (s) {
      const d = i[t] = Sl(
        s,
        r
      );
      tt(e, l, d, c);
    } else o && (gl(e, l, o, c), i[t] = void 0);
  }
}
const _l = /(Once|Passive|Capture)$/, bl = /^on:?(?:Once|Passive|Capture)$/;
function yl(e) {
  let t, n;
  for (; (n = e.match(_l)) && !bl.test(e); )
    t || (t = {}), e = e.slice(0, e.length - n[1].length), t[n[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : it(e.slice(2)), t];
}
let In = 0;
const xl = /* @__PURE__ */ Promise.resolve(), vl = () => In || (xl.then(() => In = 0), In = Date.now());
function Sl(e, t) {
  const n = (s) => {
    if (!s._vts)
      s._vts = Date.now();
    else if (s._vts <= n.attached)
      return;
    const r = n.value;
    if (M(r)) {
      const i = s.stopImmediatePropagation;
      s.stopImmediatePropagation = () => {
        i.call(s), s._stopped = !0;
      };
      const o = r.slice(), l = [s];
      for (let c = 0; c < o.length && !s._stopped; c++) {
        const d = o[c];
        d && ye(
          d,
          t,
          5,
          l
        );
      }
    } else
      ye(
        r,
        t,
        5,
        [s]
      );
  };
  return n.value = e, n.attached = vl(), n;
}
const qs = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, wl = (e, t, n, s, r, i) => {
  const o = r === "svg";
  t === "class" ? fl(e, s, o) : t === "style" ? dl(e, n, s) : ln(t) ? fn(t) || ml(e, t, n, s, i) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : Cl(e, t, s, o)) ? (Ws(e, t, s), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Ks(e, t, s, o, i, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (Tl(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !k(s))) ? Ws(e, me(t), s, i, t) : (t === "true-value" ? e._trueValue = s : t === "false-value" && (e._falseValue = s), Ks(e, t, s, o));
};
function Cl(e, t, n, s) {
  if (s)
    return !!(t === "innerHTML" || t === "textContent" || t in e && qs(t) && R(n));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const r = e.tagName;
    if (r === "IMG" || r === "VIDEO" || r === "CANVAS" || r === "SOURCE")
      return !1;
  }
  return qs(t) && k(n) ? !1 : t in e;
}
function Tl(e, t) {
  const n = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!n)
    return !1;
  const s = me(t);
  return Array.isArray(n) ? n.some((r) => me(r) === s) : Object.keys(n).some((r) => me(r) === s);
}
const on = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return M(t) ? (n) => Yt(t, n) : t;
};
function El(e) {
  e.target.composing = !0;
}
function Js(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const nt = /* @__PURE__ */ Symbol("_assign"), Gt = /* @__PURE__ */ Symbol("_initialValue");
function Rn(e, t, n) {
  return t && (e = e.trim()), n && (e = Yn(e)), e;
}
const he = {
  created(e, { modifiers: { lazy: t, trim: n, number: s } }, r) {
    e.parentNode && (e.type === "text" ? e[Gt] = e.defaultValue.replace(/[\r\n]/g, "") : e.type === "textarea" && (e[Gt] = e.defaultValue.replace(/\r\n?/g, `
`))), e[nt] = on(r);
    const i = s || r.props && r.props.type === "number";
    tt(e, t ? "change" : "input", (o) => {
      o.target.composing || e[nt](Rn(e.value, n, i));
    }), (n || i) && tt(e, "change", () => {
      e.value = Rn(e.value, n, i);
    }), t || (tt(e, "compositionstart", El), tt(e, "compositionend", Js), tt(e, "change", Js));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t, modifiers: { trim: n, number: s } }) {
    const r = t ?? "", i = e[Gt];
    delete e[Gt], i !== void 0 && (e.type === "text" || e.type === "textarea") && e.value !== i ? e[nt](Rn(e.value, n, s)) : e.value = r;
  },
  beforeUpdate(e, { value: t, oldValue: n, modifiers: { lazy: s, trim: r, number: i } }, o) {
    if (e[nt] = on(o), e.composing) return;
    const l = (i || e.type === "number") && !/^0\d/.test(e.value) ? Yn(e.value) : e.value, c = t ?? "";
    if (l === c)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (s && t === n || r && e.value.trim() === c) || (e.value = c);
  }
}, Fn = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, n) {
    e[nt] = on(n), tt(e, "change", () => {
      const s = e._modelValue, r = Ol(e), i = e.checked, o = e[nt];
      if (M(s)) {
        const l = nr(s, r), c = l !== -1;
        if (i && !c)
          o(s.concat(r));
        else if (!i && c) {
          const d = [...s];
          d.splice(l, 1), o(d);
        }
      } else if (dt(s)) {
        const l = new Set(s);
        i ? l.add(r) : l.delete(r), o(l);
      } else
        o(ni(e, i));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: zs,
  beforeUpdate(e, t, n) {
    e[nt] = on(n), zs(e, t, n);
  }
};
function zs(e, { value: t, oldValue: n }, s) {
  e._modelValue = t;
  let r;
  if (M(t))
    r = nr(t, s.props.value) > -1;
  else if (dt(t))
    r = t.has(s.props.value);
  else {
    if (t === n) return;
    r = pt(t, ni(e, !0));
  }
  e.checked !== r && (e.checked = r);
}
function Ol(e) {
  return "_value" in e ? e._value : e.value;
}
function ni(e, t) {
  const n = t ? "_trueValue" : "_falseValue";
  return n in e ? e[n] : t;
}
const Al = /* @__PURE__ */ se({ patchProp: wl }, ol);
let Gs;
function Pl() {
  return Gs || (Gs = Ho(Al));
}
const Ml = ((...e) => {
  const t = Pl().createApp(...e), { mount: n } = t;
  return t.mount = (s) => {
    const r = Rl(s);
    if (!r) return;
    const i = t._component;
    !R(i) && !i.render && !i.template && (i.template = r.innerHTML), r.nodeType === 1 && (r.textContent = "");
    const o = n(r, !1, Il(r));
    return r instanceof Element && (r.removeAttribute("v-cloak"), r.setAttribute("data-v-app", "")), o;
  }, t;
});
function Il(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function Rl(e) {
  return k(e) ? document.querySelector(e) : e;
}
let qn = null;
function Fl(e) {
  qn = e ?? null;
}
function z(e, t) {
  return qn ? qn(e, t) : e;
}
const Vl = { class: "geom-settings" }, Dl = { class: "hint" }, jl = { class: "hint" }, Nl = { class: "check" }, Hl = { class: "hint" }, Ul = { class: "hint" }, $l = { class: "hint" }, Ll = { class: "colour" }, Kl = ["value"], Wl = { class: "check" }, Bl = { class: "check" }, ql = { class: "hint" }, Jl = {
  key: 0,
  role: "alert"
}, zl = /* @__PURE__ */ no({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const n = e, s = /* @__PURE__ */ an({
      render_max_side: 1024,
      line_width: 3,
      font_size: 16,
      crop_padding: 0.05,
      jpeg_quality: 0,
      render_history: 5,
      grid: !0,
      grid_step: 0,
      grid_min_step: 32,
      grid_lines: 24,
      grid_major_every: 4,
      grid_transparency: 10,
      grid_color: "#141414",
      edge_rulers: !0,
      ruler_labels: !0,
      ruler_transparency: 10
    }), r = /* @__PURE__ */ Ni("");
    try {
      Object.assign(s, JSON.parse(n.api.getJson() || "{}"));
    } catch (c) {
      r.value = String(c);
    }
    const i = ei(() => /^#[0-9a-fA-F]{6}$/.test(s.grid_color) ? s.grid_color : "#141414");
    function o(c) {
      s.grid_color = c;
    }
    function l() {
      return JSON.stringify(s);
    }
    return t({ toJson: l }), (c, d) => (Wn(), Vs("div", Vl, [
      A("section", null, [
        A("h3", null, L(q(z)("Render")), 1),
        A("label", null, [
          te(L(q(z)("Working size (longest side, px)")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[0] || (d[0] = (a) => s.render_max_side = a),
            min: "256",
            max: "4096"
          }, null, 512), [
            [
              he,
              s.render_max_side,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("p", Dl, L(q(z)("Every render is delivered at this size, and every coordinate the model passes is read in it.")), 1),
        A("label", null, [
          te(L(q(z)("Outline width (px)")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[1] || (d[1] = (a) => s.line_width = a),
            min: "1",
            max: "16"
          }, null, 512), [
            [
              he,
              s.line_width,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("label", null, [
          te(L(q(z)("Font size")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[2] || (d[2] = (a) => s.font_size = a),
            min: "8",
            max: "48"
          }, null, 512), [
            [
              he,
              s.font_size,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("p", jl, L(q(z)("Object names and every ruler number. A digit too small to read is read anyway, wrongly.")), 1),
        A("label", null, [
          te(L(q(z)("Crop padding (fraction of the box)")) + " ", 1),
          re(A("input", {
            type: "number",
            step: "0.01",
            "onUpdate:modelValue": d[3] || (d[3] = (a) => s.crop_padding = a),
            min: "0",
            max: "1"
          }, null, 512), [
            [
              he,
              s.crop_padding,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("label", null, [
          te(L(q(z)("JPEG quality (0 = PNG)")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[4] || (d[4] = (a) => s.jpeg_quality = a),
            min: "0",
            max: "100"
          }, null, 512), [
            [
              he,
              s.jpeg_quality,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("label", null, [
          te(L(q(z)("Renders kept per chat")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[5] || (d[5] = (a) => s.render_history = a),
            min: "1",
            max: "20"
          }, null, 512), [
            [
              he,
              s.render_history,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      A("section", null, [
        A("h3", null, L(q(z)("View grid")), 1),
        A("label", Nl, [
          re(A("input", {
            type: "checkbox",
            "onUpdate:modelValue": d[6] || (d[6] = (a) => s.grid = a)
          }, null, 512), [
            [Fn, s.grid]
          ]),
          te(" " + L(q(z)("Draw the grid by default")), 1)
        ]),
        A("label", null, [
          te(L(q(z)("Spacing (px, 0 = derive from the frame)")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[7] || (d[7] = (a) => s.grid_step = a),
            min: "0",
            max: "500"
          }, null, 512), [
            [
              he,
              s.grid_step,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("label", null, [
          te(L(q(z)("Finest step (px)")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[8] || (d[8] = (a) => s.grid_min_step = a),
            min: "4",
            max: "256"
          }, null, 512), [
            [
              he,
              s.grid_min_step,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("p", Hl, L(q(z)("The lattice everything stands on, rulers included. The model reads the frame in patches of roughly this size, so lines closer together than one patch cost legibility and measure nothing.")), 1),
        A("label", null, [
          te(L(q(z)("Fine lines across the frame")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[9] || (d[9] = (a) => s.grid_lines = a),
            min: "4",
            max: "100"
          }, null, 512), [
            [
              he,
              s.grid_lines,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("p", Ul, L(q(z)("What stays constant as the view is cropped: the density of the ruler, not its spacing.")), 1),
        A("label", null, [
          te(L(q(z)("Every Nth line is major and labelled")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[10] || (d[10] = (a) => s.grid_major_every = a),
            min: "1",
            max: "20"
          }, null, 512), [
            [
              he,
              s.grid_major_every,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("label", null, [
          te(L(q(z)("Transparency (%)")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[11] || (d[11] = (a) => s.grid_transparency = a),
            min: "0",
            max: "95"
          }, null, 512), [
            [
              he,
              s.grid_transparency,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("p", $l, L(q(z)("An instrument you have to hunt for gets guessed past instead of read. If the grid hides too much, coarsen the step rather than fade the line.")), 1),
        A("label", null, [
          te(L(q(z)("Colour")) + " ", 1),
          A("span", Ll, [
            A("input", {
              type: "color",
              value: i.value,
              onInput: d[12] || (d[12] = (a) => o(a.target.value))
            }, null, 40, Kl),
            re(A("input", {
              "onUpdate:modelValue": d[13] || (d[13] = (a) => s.grid_color = a),
              spellcheck: "false"
            }, null, 512), [
              [he, s.grid_color]
            ])
          ])
        ])
      ]),
      A("section", null, [
        A("h3", null, L(q(z)("Edge rulers")), 1),
        A("label", Wl, [
          re(A("input", {
            type: "checkbox",
            "onUpdate:modelValue": d[14] || (d[14] = (a) => s.edge_rulers = a)
          }, null, 512), [
            [Fn, s.edge_rulers]
          ]),
          te(" " + L(q(z)("Draw the rulers by default")), 1)
        ]),
        A("label", Bl, [
          re(A("input", {
            type: "checkbox",
            "onUpdate:modelValue": d[15] || (d[15] = (a) => s.ruler_labels = a)
          }, null, 512), [
            [Fn, s.ruler_labels]
          ]),
          te(" " + L(q(z)("Number every ruler line")), 1)
        ]),
        A("label", null, [
          te(L(q(z)("Transparency (%)")) + " ", 1),
          re(A("input", {
            type: "number",
            "onUpdate:modelValue": d[16] || (d[16] = (a) => s.ruler_transparency = a),
            min: "0",
            max: "95"
          }, null, 512), [
            [
              he,
              s.ruler_transparency,
              void 0,
              { number: !0 }
            ]
          ])
        ]),
        A("p", ql, L(q(z)("Solid lines are inside the box, dashed ones outside it; both step on the finest step above. Their spacing is not a setting — it is computed from the box.")), 1)
      ]),
      r.value ? (Wn(), Vs("p", Jl, L(r.value), 1)) : Jo("", !0)
    ]));
  }
}), Gl = (e, t) => {
  const n = e.__vccOpts || e;
  for (const [s, r] of t)
    n[s] = r;
  return n;
}, Yl = /* @__PURE__ */ Gl(zl, [["__scopeId", "data-v-3ec47264"]]);
function Xl(e, t) {
  var r;
  Fl((r = t.t) == null ? void 0 : r.bind(t));
  const n = Ml(Yl, { api: t }), s = n.mount(e);
  return { save: () => s.toJson(), destroy: () => n.unmount() };
}
export {
  Xl as mount
};
