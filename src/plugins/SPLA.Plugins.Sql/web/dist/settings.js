/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function gi(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const Q = {}, jt = [], Ge = () => {
}, qr = () => !1, fn = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), un = (e) => e.startsWith("onUpdate:"), ae = Object.assign, mi = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, Fl = Object.prototype.hasOwnProperty, W = (e, t) => Fl.call(e, t), B = Array.isArray, Bt = (e) => Cs(e) === "[object Map]", Yt = (e) => Cs(e) === "[object Set]", er = (e) => Cs(e) === "[object Date]", U = (e) => typeof e == "function", se = (e) => typeof e == "string", Qe = (e) => typeof e == "symbol", Y = (e) => e !== null && typeof e == "object", Hr = (e) => (Y(e) || U(e)) && U(e.then) && U(e.catch), Wr = Object.prototype.toString, Cs = (e) => Wr.call(e), Kl = (e) => Cs(e).slice(8, -1), Jr = (e) => Cs(e) === "[object Object]", yi = (e) => se(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, hs = /* @__PURE__ */ gi(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), hn = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, xl = /-\w/g, Pe = hn(
  (e) => e.replace(xl, (t) => t.slice(1).toUpperCase())
), Ul = /\B([A-Z])/g, At = hn(
  (e) => e.replace(Ul, "-$1").toLowerCase()
), Yr = hn((e) => e.charAt(0).toUpperCase() + e.slice(1)), Ln = hn(
  (e) => e ? `on${Yr(e)}` : ""
), Ye = (e, t) => !Object.is(e, t), Js = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Gr = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, dn = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let tr;
const pn = () => tr || (tr = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function bi(e) {
  if (B(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], i = se(n) ? Wl(n) : bi(n);
      if (i)
        for (const r in i)
          t[r] = i[r];
    }
    return t;
  } else if (se(e) || Y(e))
    return e;
}
const Vl = /;(?![^(]*\))/g, ql = /:([^]+)/, Hl = /\/\*[^]*?\*\//g;
function Wl(e) {
  const t = {};
  return e.replace(Hl, "").split(Vl).forEach((s) => {
    if (s) {
      const n = s.split(ql);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function wi(e) {
  let t = "";
  if (se(e))
    t = e;
  else if (B(e))
    for (let s = 0; s < e.length; s++) {
      const n = wi(e[s]);
      n && (t += n + " ");
    }
  else if (Y(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const Jl = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", Yl = /* @__PURE__ */ gi(Jl);
function Qr(e) {
  return !!e || e === "";
}
function Gl(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = Gt(e[n], t[n]);
  return s;
}
function Gt(e, t) {
  if (e === t) return !0;
  let s = er(e), n = er(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Qe(e), n = Qe(t), s || n)
    return e === t;
  if (s = B(e), n = B(t), s || n)
    return s && n ? Gl(e, t) : !1;
  if (s = Y(e), n = Y(t), s || n) {
    if (!s || !n)
      return !1;
    const i = Object.keys(e).length, r = Object.keys(t).length;
    if (i !== r)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), c = t.hasOwnProperty(o);
      if (l && !c || !l && c || !Gt(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function Si(e, t) {
  return e.findIndex((s) => Gt(s, t));
}
const Xr = (e) => !!(e && e.__v_isRef === !0), $t = (e) => se(e) ? e : e == null ? "" : B(e) || Y(e) && (e.toString === Wr || !U(e.toString)) ? Xr(e) ? $t(e.value) : JSON.stringify(e, zr, 2) : String(e), zr = (e, t) => Xr(t) ? zr(e, t.value) : Bt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, i], r) => (s[$n(n, r) + " =>"] = i, s),
    {}
  )
} : Yt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => $n(s))
} : Qe(t) ? $n(t) : Y(t) && !B(t) && !Jr(t) ? String(t) : t, $n = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Qe(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let ce;
class Ql {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && ce && (ce.active ? (this.parent = ce, this.index = (ce.scopes || (ce.scopes = [])).push(
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
      const s = ce;
      try {
        return ce = this, t();
      } finally {
        ce = s;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = ce, ce = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (ce === this)
        ce = this.prevScope;
      else {
        let t = ce;
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
function Xl() {
  return ce;
}
let z;
const Mn = /* @__PURE__ */ new WeakSet();
class Zr {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, ce && (ce.active ? ce.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, Mn.has(this) && (Mn.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || to(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, sr(this), so(this);
    const t = z, s = De;
    z = this, De = !0;
    try {
      return this.fn();
    } finally {
      no(this), z = t, De = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        ki(t);
      this.deps = this.depsTail = void 0, sr(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? Mn.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    zn(this) && this.run();
  }
  get dirty() {
    return zn(this);
  }
}
let eo = 0, ds, ps;
function to(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = ps, ps = e;
    return;
  }
  e.next = ds, ds = e;
}
function _i() {
  eo++;
}
function vi() {
  if (--eo > 0)
    return;
  if (ps) {
    let t = ps;
    for (ps = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; ds; ) {
    let t = ds;
    for (ds = void 0; t; ) {
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
function so(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function no(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const i = n.prevDep;
    n.version === -1 ? (n === s && (s = i), ki(n), zl(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = i;
  }
  e.deps = t, e.depsTail = s;
}
function zn(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (io(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function io(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === _s) || (e.globalVersion = _s, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !zn(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = z, n = De;
  z = e, De = !0;
  try {
    so(e);
    const i = e.fn(e._value);
    (t.version === 0 || Ye(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    z = s, De = n, no(e), e.flags &= -3;
  }
}
function ki(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: i } = e;
  if (n && (n.nextSub = i, e.prevSub = void 0), i && (i.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let r = s.computed.deps; r; r = r.nextDep)
      ki(r, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function zl(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let De = !0;
const ro = [];
function Xe() {
  ro.push(De), De = !1;
}
function ze() {
  const e = ro.pop();
  De = e === void 0 ? !0 : e;
}
function sr(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const s = z;
    z = void 0;
    try {
      t();
    } finally {
      z = s;
    }
  }
}
let _s = 0;
class Zl {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class Oi {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!z || !De || z === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== z)
      s = this.activeLink = new Zl(z, this), z.deps ? (s.prevDep = z.depsTail, z.depsTail.nextDep = s, z.depsTail = s) : z.deps = z.depsTail = s, oo(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = z.depsTail, s.nextDep = void 0, z.depsTail.nextDep = s, z.depsTail = s, z.deps === s && (z.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, _s++, this.notify(t);
  }
  notify(t) {
    _i();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      vi();
    }
  }
}
function oo(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let n = t.deps; n; n = n.nextDep)
        oo(n);
    }
    const s = e.dep.subs;
    s !== e && (e.prevSub = s, s && (s.nextSub = e)), e.dep.subs = e;
  }
}
const Zn = /* @__PURE__ */ new WeakMap(), Ot = /* @__PURE__ */ Symbol(
  ""
), ei = /* @__PURE__ */ Symbol(
  ""
), vs = /* @__PURE__ */ Symbol(
  ""
);
function he(e, t, s) {
  if (De && z) {
    let n = Zn.get(e);
    n || Zn.set(e, n = /* @__PURE__ */ new Map());
    let i = n.get(s);
    i || (n.set(s, i = new Oi()), i.map = n, i.key = s), i.track();
  }
}
function nt(e, t, s, n, i, r) {
  const o = Zn.get(e);
  if (!o) {
    _s++;
    return;
  }
  const l = (c) => {
    c && c.trigger();
  };
  if (_i(), t === "clear")
    o.forEach(l);
  else {
    const c = B(e), a = c && yi(s);
    if (c && s === "length") {
      const f = Number(n);
      o.forEach((u, g) => {
        (g === "length" || g === vs || !Qe(g) && g >= f) && l(u);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), a && l(o.get(vs)), t) {
        case "add":
          c ? a && l(o.get("length")) : (l(o.get(Ot)), Bt(e) && l(o.get(ei)));
          break;
        case "delete":
          c || (l(o.get(Ot)), Bt(e) && l(o.get(ei)));
          break;
        case "set":
          Bt(e) && l(o.get(Ot));
          break;
      }
  }
  vi();
}
function Ct(e) {
  const t = /* @__PURE__ */ H(e);
  return t === e ? t : (he(t, "iterate", vs), /* @__PURE__ */ Ie(e) ? t : t.map(Re));
}
function gn(e) {
  return he(e = /* @__PURE__ */ H(e), "iterate", vs), e;
}
function We(e, t) {
  return /* @__PURE__ */ at(e) ? Vt(/* @__PURE__ */ Tt(e) ? Re(t) : t) : Re(t);
}
const ec = {
  __proto__: null,
  [Symbol.iterator]() {
    return Pn(this, Symbol.iterator, (e) => We(this, e));
  },
  concat(...e) {
    return Ct(this).concat(
      ...e.map((t) => B(t) ? Ct(t) : t)
    );
  },
  entries() {
    return Pn(this, "entries", (e) => (e[1] = We(this, e[1]), e));
  },
  every(e, t) {
    return et(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return et(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => We(this, n)),
      arguments
    );
  },
  find(e, t) {
    return et(
      this,
      "find",
      e,
      t,
      (s) => We(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return et(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return et(
      this,
      "findLast",
      e,
      t,
      (s) => We(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return et(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return et(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return Dn(this, "includes", e);
  },
  indexOf(...e) {
    return Dn(this, "indexOf", e);
  },
  join(e) {
    return Ct(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return Dn(this, "lastIndexOf", e);
  },
  map(e, t) {
    return et(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return is(this, "pop");
  },
  push(...e) {
    return is(this, "push", e);
  },
  reduce(e, ...t) {
    return nr(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return nr(this, "reduceRight", e, t);
  },
  shift() {
    return is(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return et(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return is(this, "splice", e);
  },
  toReversed() {
    return Ct(this).toReversed();
  },
  toSorted(e) {
    return Ct(this).toSorted(e);
  },
  toSpliced(...e) {
    return Ct(this).toSpliced(...e);
  },
  unshift(...e) {
    return is(this, "unshift", e);
  },
  values() {
    return Pn(this, "values", (e) => We(this, e));
  }
};
function Pn(e, t, s) {
  const n = gn(e), i = n[t]();
  return n !== e && !/* @__PURE__ */ Ie(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = s(r.value)), r;
  }), i;
}
const tc = Array.prototype;
function et(e, t, s, n, i, r) {
  const o = gn(e), l = o !== e && !/* @__PURE__ */ Ie(e), c = o[t];
  if (c !== tc[t]) {
    const u = c.apply(e, r);
    return l ? Re(u) : u;
  }
  let a = s;
  o !== e && (l ? a = function(u, g) {
    return s.call(this, We(e, u), g, e);
  } : s.length > 2 && (a = function(u, g) {
    return s.call(this, u, g, e);
  }));
  const f = c.call(o, a, n);
  return l && i ? i(f) : f;
}
function nr(e, t, s, n) {
  const i = gn(e), r = i !== e && !/* @__PURE__ */ Ie(e);
  let o = s, l = !1;
  i !== e && (r ? (l = n.length === 0, o = function(a, f, u) {
    return l && (l = !1, a = We(e, a)), s.call(this, a, We(e, f), u, e);
  }) : s.length > 3 && (o = function(a, f, u) {
    return s.call(this, a, f, u, e);
  }));
  const c = i[t](o, ...n);
  return l ? We(e, c) : c;
}
function Dn(e, t, s) {
  const n = /* @__PURE__ */ H(e);
  he(n, "iterate", vs);
  const i = n[t](...s);
  return (i === -1 || i === !1) && /* @__PURE__ */ Ai(s[0]) ? (s[0] = /* @__PURE__ */ H(s[0]), n[t](...s)) : i;
}
function is(e, t, s = []) {
  Xe(), _i();
  const n = (/* @__PURE__ */ H(e))[t].apply(e, s);
  return vi(), ze(), n;
}
const sc = /* @__PURE__ */ gi("__proto__,__v_isRef,__isVue"), lo = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Qe)
);
function nc(e) {
  Qe(e) || (e = String(e));
  const t = /* @__PURE__ */ H(this);
  return he(t, "has", e), t.hasOwnProperty(e);
}
class co {
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
      return n === (i ? r ? dc : ho : r ? uo : fo).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = B(t);
    if (!i) {
      let c;
      if (o && (c = ec[s]))
        return c;
      if (s === "hasOwnProperty")
        return nc;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ de(t) ? t : n
    );
    if ((Qe(s) ? lo.has(s) : sc(s)) || (i || he(t, "get", s), r))
      return l;
    if (/* @__PURE__ */ de(l)) {
      const c = o && yi(s) ? l : l.value;
      return i && Y(c) ? /* @__PURE__ */ si(c) : c;
    }
    return Y(l) ? i ? /* @__PURE__ */ si(l) : /* @__PURE__ */ mn(l) : l;
  }
}
class ao extends co {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, i) {
    let r = t[s];
    const o = B(t) && yi(s);
    if (!this._isShallow) {
      const a = /* @__PURE__ */ at(r);
      if (!/* @__PURE__ */ Ie(n) && !/* @__PURE__ */ at(n) && (r = /* @__PURE__ */ H(r), n = /* @__PURE__ */ H(n)), !o && /* @__PURE__ */ de(r) && !/* @__PURE__ */ de(n))
        return a || (r.value = n), !0;
    }
    const l = o ? Number(s) < t.length : W(t, s), c = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ de(t) ? t : i
    );
    return t === /* @__PURE__ */ H(i) && c && (l ? Ye(n, r) && nt(t, "set", s, n) : nt(t, "add", s, n)), c;
  }
  deleteProperty(t, s) {
    const n = W(t, s);
    t[s];
    const i = Reflect.deleteProperty(t, s);
    return i && n && nt(t, "delete", s, void 0), i;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Qe(s) || !lo.has(s)) && he(t, "has", s), n;
  }
  ownKeys(t) {
    return he(
      t,
      "iterate",
      B(t) ? "length" : Ot
    ), Reflect.ownKeys(t);
  }
}
class ic extends co {
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
const rc = /* @__PURE__ */ new ao(), oc = /* @__PURE__ */ new ic(), lc = /* @__PURE__ */ new ao(!0);
const ti = (e) => e, Fs = (e) => Reflect.getPrototypeOf(e);
function cc(e, t, s) {
  return function(...n) {
    const i = this.__v_raw, r = /* @__PURE__ */ H(i), o = Bt(r), l = e === "entries" || e === Symbol.iterator && o, c = e === "keys" && o, a = i[e](...n), f = s ? ti : t ? Vt : Re;
    return !t && he(
      r,
      "iterate",
      c ? ei : Ot
    ), ae(
      // inheriting all iterator properties
      Object.create(a),
      {
        // iterator protocol
        next() {
          const { value: u, done: g } = a.next();
          return g ? { value: u, done: g } : {
            value: l ? [f(u[0]), f(u[1])] : f(u),
            done: g
          };
        }
      }
    );
  };
}
function Ks(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function ac(e, t) {
  const s = {
    get(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ H(r), l = /* @__PURE__ */ H(i);
      e || (Ye(i, l) && he(o, "get", i), he(o, "get", l));
      const { has: c } = Fs(o), a = t ? ti : e ? Vt : Re;
      if (c.call(o, i))
        return a(r.get(i));
      if (c.call(o, l))
        return a(r.get(l));
      r !== o && r.get(i);
    },
    get size() {
      const i = this.__v_raw;
      return !e && he(/* @__PURE__ */ H(i), "iterate", Ot), i.size;
    },
    has(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ H(r), l = /* @__PURE__ */ H(i);
      return e || (Ye(i, l) && he(o, "has", i), he(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, c = /* @__PURE__ */ H(l), a = t ? ti : e ? Vt : Re;
      return !e && he(c, "iterate", Ot), l.forEach((f, u) => i.call(r, a(f), a(u), o));
    }
  };
  return ae(
    s,
    e ? {
      add: Ks("add"),
      set: Ks("set"),
      delete: Ks("delete"),
      clear: Ks("clear")
    } : {
      add(i) {
        const r = /* @__PURE__ */ H(this), o = Fs(r), l = /* @__PURE__ */ H(i), c = !t && !/* @__PURE__ */ Ie(i) && !/* @__PURE__ */ at(i) ? l : i;
        return o.has.call(r, c) || Ye(i, c) && o.has.call(r, i) || Ye(l, c) && o.has.call(r, l) || (r.add(c), nt(r, "add", c, c)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ Ie(r) && !/* @__PURE__ */ at(r) && (r = /* @__PURE__ */ H(r));
        const o = /* @__PURE__ */ H(this), { has: l, get: c } = Fs(o);
        let a = l.call(o, i);
        a || (i = /* @__PURE__ */ H(i), a = l.call(o, i));
        const f = c.call(o, i);
        return o.set(i, r), a ? Ye(r, f) && nt(o, "set", i, r) : nt(o, "add", i, r), this;
      },
      delete(i) {
        const r = /* @__PURE__ */ H(this), { has: o, get: l } = Fs(r);
        let c = o.call(r, i);
        c || (i = /* @__PURE__ */ H(i), c = o.call(r, i)), l && l.call(r, i);
        const a = r.delete(i);
        return c && nt(r, "delete", i, void 0), a;
      },
      clear() {
        const i = /* @__PURE__ */ H(this), r = i.size !== 0, o = i.clear();
        return r && nt(
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
    s[i] = cc(i, e, t);
  }), s;
}
function Ti(e, t) {
  const s = ac(e, t);
  return (n, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? n : Reflect.get(
    W(s, i) && i in n ? s : n,
    i,
    r
  );
}
const fc = {
  get: /* @__PURE__ */ Ti(!1, !1)
}, uc = {
  get: /* @__PURE__ */ Ti(!1, !0)
}, hc = {
  get: /* @__PURE__ */ Ti(!0, !1)
};
const fo = /* @__PURE__ */ new WeakMap(), uo = /* @__PURE__ */ new WeakMap(), ho = /* @__PURE__ */ new WeakMap(), dc = /* @__PURE__ */ new WeakMap();
function pc(e) {
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
function mn(e) {
  return /* @__PURE__ */ at(e) ? e : Ni(
    e,
    !1,
    rc,
    fc,
    fo
  );
}
// @__NO_SIDE_EFFECTS__
function gc(e) {
  return Ni(
    e,
    !1,
    lc,
    uc,
    uo
  );
}
// @__NO_SIDE_EFFECTS__
function si(e) {
  return Ni(
    e,
    !0,
    oc,
    hc,
    ho
  );
}
function Ni(e, t, s, n, i) {
  if (!Y(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const r = i.get(e);
  if (r)
    return r;
  const o = pc(Kl(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? n : s
  );
  return i.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function Tt(e) {
  return /* @__PURE__ */ at(e) ? /* @__PURE__ */ Tt(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function at(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function Ie(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function Ai(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function H(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ H(t) : e;
}
function mc(e) {
  return !W(e, "__v_skip") && Object.isExtensible(e) && Gr(e, "__v_skip", !0), e;
}
const Re = (e) => Y(e) ? /* @__PURE__ */ mn(e) : e, Vt = (e) => Y(e) ? /* @__PURE__ */ si(e) : e;
// @__NO_SIDE_EFFECTS__
function de(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Rn(e) {
  return yc(e, !1);
}
function yc(e, t) {
  return /* @__PURE__ */ de(e) ? e : new bc(e, t);
}
class bc {
  constructor(t, s) {
    this.dep = new Oi(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ H(t), this._value = s ? t : Re(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ Ie(t) || /* @__PURE__ */ at(t);
    t = n ? t : /* @__PURE__ */ H(t), Ye(t, s) && (this._rawValue = t, this._value = n ? t : Re(t), this.dep.trigger());
  }
}
function wc(e) {
  return /* @__PURE__ */ de(e) ? e.value : e;
}
const Sc = {
  get: (e, t, s) => t === "__v_raw" ? e : wc(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const i = e[t];
    return /* @__PURE__ */ de(i) && !/* @__PURE__ */ de(s) ? (i.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function po(e) {
  return /* @__PURE__ */ Tt(e) ? e : new Proxy(e, Sc);
}
class _c {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Oi(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = _s - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    z !== this)
      return to(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return io(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function vc(e, t, s = !1) {
  let n, i;
  return U(e) ? n = e : (n = e.get, i = e.set), new _c(n, i, s);
}
const xs = {}, Zs = /* @__PURE__ */ new WeakMap();
let St;
function kc(e, t = !1, s = St) {
  if (s) {
    let n = Zs.get(s);
    n || Zs.set(s, n = []), n.push(e);
  }
}
function Oc(e, t, s = Q) {
  const { immediate: n, deep: i, once: r, scheduler: o, augmentJob: l, call: c } = s, a = (A) => i ? A : /* @__PURE__ */ Ie(A) || i === !1 || i === 0 ? it(A, 1) : it(A);
  let f, u, g, p, k = !1, m = !1;
  if (/* @__PURE__ */ de(e) ? (u = () => e.value, k = /* @__PURE__ */ Ie(e)) : /* @__PURE__ */ Tt(e) ? (u = () => a(e), k = !0) : B(e) ? (m = !0, k = e.some((A) => /* @__PURE__ */ Tt(A) || /* @__PURE__ */ Ie(A)), u = () => e.map((A) => {
    if (/* @__PURE__ */ de(A))
      return A.value;
    if (/* @__PURE__ */ Tt(A))
      return a(A);
    if (U(A))
      return c ? c(A, 2) : A();
  })) : U(e) ? t ? u = c ? () => c(e, 2) : e : u = () => {
    if (g) {
      Xe();
      try {
        g();
      } finally {
        ze();
      }
    }
    const A = St;
    St = f;
    try {
      return c ? c(e, 3, [p]) : e(p);
    } finally {
      St = A;
    }
  } : u = Ge, t && i) {
    const A = u, N = i === !0 ? 1 / 0 : i;
    u = () => it(A(), N);
  }
  const _ = Xl(), b = () => {
    f.stop(), _ && _.active && mi(_.effects, f);
  };
  if (r && t) {
    const A = t;
    t = (...N) => {
      const x = A(...N);
      return b(), x;
    };
  }
  let y = m ? new Array(e.length).fill(xs) : xs;
  const S = (A) => {
    if (!(!(f.flags & 1) || !f.dirty && !A))
      if (t) {
        const N = f.run();
        if (A || i || k || (m ? N.some((x, D) => Ye(x, y[D])) : Ye(N, y))) {
          g && g();
          const x = St;
          St = f;
          try {
            const D = [
              N,
              // pass undefined as the old value when it's changed for the first time
              y === xs ? void 0 : m && y[0] === xs ? [] : y,
              p
            ];
            y = N, c ? c(t, 3, D) : (
              // @ts-expect-error
              t(...D)
            );
          } finally {
            St = x;
          }
        }
      } else
        f.run();
  };
  return l && l(S), f = new Zr(u), f.scheduler = o ? () => o(S, !1) : S, p = (A) => kc(A, !1, f), g = f.onStop = () => {
    const A = Zs.get(f);
    if (A) {
      if (c)
        c(A, 4);
      else
        for (const N of A) N();
      Zs.delete(f);
    }
  }, t ? n ? S(!0) : y = f.run() : o ? o(S.bind(null, !0), !0) : f.run(), b.pause = f.pause.bind(f), b.resume = f.resume.bind(f), b.stop = b, b;
}
function it(e, t = 1 / 0, s) {
  if (t <= 0 || !Y(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ de(e))
    it(e.value, t, s);
  else if (B(e))
    for (let n = 0; n < e.length; n++)
      it(e[n], t, s);
  else if (Yt(e) || Bt(e))
    e.forEach((n) => {
      it(n, t, s);
    });
  else if (Jr(e)) {
    for (const n in e)
      it(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && it(e[n], t, s);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function Is(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (i) {
    yn(i, t, s);
  }
}
function je(e, t, s, n) {
  if (U(e)) {
    const i = Is(e, t, s, n);
    return i && Hr(i) && i.catch((r) => {
      yn(r, t, s);
    }), i;
  }
  if (B(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(je(e[r], t, s, n));
    return i;
  }
}
function yn(e, t, s, n = !0) {
  const i = t ? t.vnode : null, { errorHandler: r, throwUnhandledErrorInProduction: o } = t && t.appContext.config || Q;
  if (t) {
    let l = t.parent;
    const c = t.proxy, a = `https://vuejs.org/error-reference/#runtime-${s}`;
    for (; l; ) {
      const f = l.ec;
      if (f) {
        for (let u = 0; u < f.length; u++)
          if (f[u](e, c, a) === !1)
            return;
      }
      l = l.parent;
    }
    if (r) {
      Xe(), Is(r, null, 10, [
        e,
        c,
        a
      ]), ze();
      return;
    }
  }
  Tc(e, s, i, n, o);
}
function Tc(e, t, s, n = !0, i = !1) {
  if (i)
    throw e;
  console.error(e);
}
const we = [];
let He = -1;
const Ft = [];
let ut = null, Mt = 0;
const go = /* @__PURE__ */ Promise.resolve();
let en = null;
function mo(e) {
  const t = en || go;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Nc(e) {
  let t = He + 1, s = we.length;
  for (; t < s; ) {
    const n = t + s >>> 1, i = we[n], r = ks(i);
    r < e || r === e && i.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function Ei(e) {
  if (!(e.flags & 1)) {
    const t = ks(e), s = we[we.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= ks(s) ? we.push(e) : we.splice(Nc(t), 0, e), e.flags |= 1, yo();
  }
}
function yo() {
  en || (en = go.then(wo));
}
function Ac(e) {
  B(e) ? Ft.push(...e) : ut && e.id === -1 ? ut.splice(Mt + 1, 0, e) : e.flags & 1 || (Ft.push(e), e.flags |= 1), yo();
}
function ir(e, t, s = He + 1) {
  for (; s < we.length; s++) {
    const n = we[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      we.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function bo(e) {
  if (Ft.length) {
    const t = [...new Set(Ft)].sort(
      (s, n) => ks(s) - ks(n)
    );
    if (Ft.length = 0, ut) {
      ut.push(...t);
      return;
    }
    for (ut = t, Mt = 0; Mt < ut.length; Mt++) {
      const s = ut[Mt];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    ut = null, Mt = 0;
  }
}
const ks = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function wo(e) {
  try {
    for (He = 0; He < we.length; He++) {
      const t = we[He];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Is(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; He < we.length; He++) {
      const t = we[He];
      t && (t.flags &= -2);
    }
    He = -1, we.length = 0, bo(), en = null, (we.length || Ft.length) && wo();
  }
}
let Ce = null, So = null;
function tn(e) {
  const t = Ce;
  return Ce = e, So = e && e.type.__scopeId || null, t;
}
function Ec(e, t = Ce, s) {
  if (!t || e._n)
    return e;
  const n = (...i) => {
    n._d && gr(-1);
    const r = tn(t);
    let o;
    try {
      o = e(...i);
    } finally {
      tn(r), n._d && gr(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function ve(e, t) {
  if (Ce === null)
    return e;
  const s = _n(Ce), n = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [r, o, l, c = Q] = t[i];
    r && (U(r) && (r = {
      mounted: r,
      updated: r
    }), r.deep && it(o), n.push({
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
function bt(e, t, s, n) {
  const i = e.dirs, r = t && t.dirs;
  for (let o = 0; o < i.length; o++) {
    const l = i[o];
    r && (l.oldValue = r[o].value);
    let c = l.dir[n];
    c && (Xe(), je(c, s, 8, [
      e.el,
      l,
      e,
      t
    ]), ze());
  }
}
function Cc(e, t) {
  if (Se) {
    let s = Se.provides;
    const n = Se.parent && Se.parent.provides;
    n === s && (s = Se.provides = Object.create(n)), s[e] = t;
  }
}
function Ys(e, t, s = !1) {
  const n = Ca();
  if (n || Kt) {
    let i = Kt ? Kt._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return s && U(t) ? t.call(n && n.proxy) : t;
  }
}
const Ic = /* @__PURE__ */ Symbol.for("v-scx"), Lc = () => Ys(Ic);
function jn(e, t, s) {
  return _o(e, t, s);
}
function _o(e, t, s = Q) {
  const { immediate: n, deep: i, flush: r, once: o } = s, l = ae({}, s), c = t && n || !t && r !== "post";
  let a;
  if (Ts) {
    if (r === "sync") {
      const p = Lc();
      a = p.__watcherHandles || (p.__watcherHandles = []);
    } else if (!c) {
      const p = () => {
      };
      return p.stop = Ge, p.resume = Ge, p.pause = Ge, p;
    }
  }
  const f = Se;
  l.call = (p, k, m) => je(p, f, k, m);
  let u = !1;
  r === "post" ? l.scheduler = (p) => {
    Oe(p, f && f.suspense);
  } : r !== "sync" && (u = !0, l.scheduler = (p, k) => {
    k ? p() : Ei(p);
  }), l.augmentJob = (p) => {
    t && (p.flags |= 4), u && (p.flags |= 2, f && (p.id = f.uid, p.i = f));
  };
  const g = Oc(e, t, l);
  return Ts && (a ? a.push(g) : c && g()), g;
}
function $c(e, t, s) {
  const n = this.proxy, i = se(e) ? e.includes(".") ? vo(n, e) : () => n[e] : e.bind(n, n);
  let r;
  U(t) ? r = t : (r = t.handler, s = t);
  const o = Ls(this), l = _o(i, r.bind(n), s);
  return o(), l;
}
function vo(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let i = 0; i < s.length && n; i++)
      n = n[s[i]];
    return n;
  };
}
const Mc = /* @__PURE__ */ Symbol("_vte"), Pc = (e) => e.__isTeleport, Bn = /* @__PURE__ */ Symbol("_leaveCb");
function Ci(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, Ci(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Dc(e, t) {
  return U(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    ae({ name: e.name }, t, { setup: e })
  ) : e;
}
function ko(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function rr(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const sn = /* @__PURE__ */ new WeakMap();
function gs(e, t, s, n, i = !1) {
  if (B(e)) {
    e.forEach(
      (m, _) => gs(
        m,
        t && (B(t) ? t[_] : t),
        s,
        n,
        i
      )
    );
    return;
  }
  if (ms(n) && !i) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && gs(e, t, s, n.component.subTree);
    return;
  }
  const r = n.shapeFlag & 4 ? _n(n.component) : n.el, o = i ? null : r, { i: l, r: c } = e, a = t && t.r, f = l.refs === Q ? l.refs = {} : l.refs, u = l.setupState, g = /* @__PURE__ */ H(u), p = u === Q ? qr : (m) => rr(f, m) ? !1 : W(g, m), k = (m, _) => !(_ && rr(f, _));
  if (a != null && a !== c) {
    if (or(t), se(a))
      f[a] = null, p(a) && (u[a] = null);
    else if (/* @__PURE__ */ de(a)) {
      const m = t;
      k(a, m.k) && (a.value = null), m.k && (f[m.k] = null);
    }
  }
  if (U(c)) {
    Xe();
    try {
      Is(c, l, 12, [o, f]);
    } finally {
      ze();
    }
  } else {
    const m = se(c), _ = /* @__PURE__ */ de(c);
    if (m || _) {
      const b = () => {
        if (e.f) {
          const y = m ? p(c) ? u[c] : f[c] : k() || !e.k ? c.value : f[e.k];
          if (i)
            B(y) && mi(y, r);
          else if (B(y))
            y.includes(r) || y.push(r);
          else if (m)
            f[c] = [r], p(c) && (u[c] = f[c]);
          else {
            const S = [r];
            k(c, e.k) && (c.value = S), e.k && (f[e.k] = S);
          }
        } else m ? (f[c] = o, p(c) && (u[c] = o)) : _ && (k(c, e.k) && (c.value = o), e.k && (f[e.k] = o));
      };
      if (o) {
        const y = () => {
          b(), sn.delete(e);
        };
        y.id = -1, sn.set(e, y), Oe(y, s);
      } else
        or(e), b();
    }
  }
}
function or(e) {
  const t = sn.get(e);
  t && (t.flags |= 8, sn.delete(e));
}
pn().requestIdleCallback;
pn().cancelIdleCallback;
const ms = (e) => !!e.type.__asyncLoader, Oo = (e) => e.type.__isKeepAlive;
function Rc(e, t) {
  To(e, "a", t);
}
function jc(e, t) {
  To(e, "da", t);
}
function To(e, t, s = Se) {
  const n = e.__wdc || (e.__wdc = () => {
    let i = s;
    for (; i; ) {
      if (i.isDeactivated)
        return;
      i = i.parent;
    }
    return e();
  });
  if (bn(t, n, s), s) {
    let i = s.parent;
    for (; i && i.parent; )
      Oo(i.parent.vnode) && Bc(n, t, s, i), i = i.parent;
  }
}
function Bc(e, t, s, n) {
  const i = bn(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Ao(() => {
    mi(n[t], i);
  }, s);
}
function bn(e, t, s = Se, n = !1) {
  if (s) {
    const i = s[e] || (s[e] = []), r = t.__weh || (t.__weh = (...o) => {
      Xe();
      const l = Ls(s), c = je(t, s, e, o);
      return l(), ze(), c;
    });
    return n ? i.unshift(r) : i.push(r), r;
  }
}
const ft = (e) => (t, s = Se) => {
  (!Ts || e === "sp") && bn(e, (...n) => t(...n), s);
}, Fc = ft("bm"), No = ft("m"), Kc = ft(
  "bu"
), xc = ft("u"), Uc = ft(
  "bum"
), Ao = ft("um"), Vc = ft(
  "sp"
), qc = ft("rtg"), Hc = ft("rtc");
function Wc(e, t = Se) {
  bn("ec", e, t);
}
const Jc = /* @__PURE__ */ Symbol.for("v-ndc");
function Fn(e, t, s, n) {
  let i;
  const r = s, o = B(e);
  if (o || se(e)) {
    const l = o && /* @__PURE__ */ Tt(e);
    let c = !1, a = !1;
    l && (c = !/* @__PURE__ */ Ie(e), a = /* @__PURE__ */ at(e), e = gn(e)), i = new Array(e.length);
    for (let f = 0, u = e.length; f < u; f++)
      i[f] = t(
        c ? a ? Vt(Re(e[f])) : Re(e[f]) : e[f],
        f,
        void 0,
        r
      );
  } else if (typeof e == "number") {
    i = new Array(e);
    for (let l = 0; l < e; l++)
      i[l] = t(l + 1, l, void 0, r);
  } else if (Y(e))
    if (e[Symbol.iterator])
      i = Array.from(
        e,
        (l, c) => t(l, c, void 0, r)
      );
    else {
      const l = Object.keys(e);
      i = new Array(l.length);
      for (let c = 0, a = l.length; c < a; c++) {
        const f = l[c];
        i[c] = t(e[f], f, c, r);
      }
    }
  else
    i = [];
  return i;
}
const ni = (e) => e ? Yo(e) ? _n(e) : ni(e.parent) : null, ys = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ ae(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => e.props,
    $attrs: (e) => e.attrs,
    $slots: (e) => e.slots,
    $refs: (e) => e.refs,
    $parent: (e) => ni(e.parent),
    $root: (e) => ni(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Co(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      Ei(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = mo.bind(e.proxy)),
    $watch: (e) => $c.bind(e)
  })
), Kn = (e, t) => e !== Q && !e.__isScriptSetup && W(e, t), Yc = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: i, props: r, accessCache: o, type: l, appContext: c } = e;
    if (t[0] !== "$") {
      const g = o[t];
      if (g !== void 0)
        switch (g) {
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
        if (Kn(n, t))
          return o[t] = 1, n[t];
        if (i !== Q && W(i, t))
          return o[t] = 2, i[t];
        if (W(r, t))
          return o[t] = 3, r[t];
        if (s !== Q && W(s, t))
          return o[t] = 4, s[t];
        ii && (o[t] = 0);
      }
    }
    const a = ys[t];
    let f, u;
    if (a)
      return t === "$attrs" && he(e.attrs, "get", ""), a(e);
    if (
      // css module (injected by vue-loader)
      (f = l.__cssModules) && (f = f[t])
    )
      return f;
    if (s !== Q && W(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      u = c.config.globalProperties, W(u, t)
    )
      return u[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: i, ctx: r } = e;
    return Kn(i, t) ? (i[t] = s, !0) : n !== Q && W(n, t) ? (n[t] = s, !0) : W(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: i, props: r, type: o }
  }, l) {
    let c;
    return !!(s[l] || e !== Q && l[0] !== "$" && W(e, l) || Kn(t, l) || W(r, l) || W(n, l) || W(ys, l) || W(i.config.globalProperties, l) || (c = o.__cssModules) && c[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : W(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function lr(e) {
  return B(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let ii = !0;
function Gc(e) {
  const t = Co(e), s = e.proxy, n = e.ctx;
  ii = !1, t.beforeCreate && cr(t.beforeCreate, e, "bc");
  const {
    // state
    data: i,
    computed: r,
    methods: o,
    watch: l,
    provide: c,
    inject: a,
    // lifecycle
    created: f,
    beforeMount: u,
    mounted: g,
    beforeUpdate: p,
    updated: k,
    activated: m,
    deactivated: _,
    beforeDestroy: b,
    beforeUnmount: y,
    destroyed: S,
    unmounted: A,
    render: N,
    renderTracked: x,
    renderTriggered: D,
    errorCaptured: M,
    serverPrefetch: q,
    // public API
    expose: ee,
    inheritAttrs: pe,
    // assets
    components: fe,
    directives: ue,
    filters: Et
  } = t;
  if (a && Qc(a, n, null), o)
    for (const te in o) {
      const X = o[te];
      U(X) && (n[te] = X.bind(s));
    }
  if (i) {
    const te = i.call(s, s);
    Y(te) && (e.data = /* @__PURE__ */ mn(te));
  }
  if (ii = !0, r)
    for (const te in r) {
      const X = r[te], mt = U(X) ? X.bind(s, s) : U(X.get) ? X.get.bind(s, s) : Ge, js = !U(X) && U(X.set) ? X.set.bind(s) : Ge, yt = Da({
        get: mt,
        set: js
      });
      Object.defineProperty(n, te, {
        enumerable: !0,
        configurable: !0,
        get: () => yt.value,
        set: (Fe) => yt.value = Fe
      });
    }
  if (l)
    for (const te in l)
      Eo(l[te], n, s, te);
  if (c) {
    const te = U(c) ? c.call(s) : c;
    Reflect.ownKeys(te).forEach((X) => {
      Cc(X, te[X]);
    });
  }
  f && cr(f, e, "c");
  function ge(te, X) {
    B(X) ? X.forEach((mt) => te(mt.bind(s))) : X && te(X.bind(s));
  }
  if (ge(Fc, u), ge(No, g), ge(Kc, p), ge(xc, k), ge(Rc, m), ge(jc, _), ge(Wc, M), ge(Hc, x), ge(qc, D), ge(Uc, y), ge(Ao, A), ge(Vc, q), B(ee))
    if (ee.length) {
      const te = e.exposed || (e.exposed = {});
      ee.forEach((X) => {
        Object.defineProperty(te, X, {
          get: () => s[X],
          set: (mt) => s[X] = mt,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  N && e.render === Ge && (e.render = N), pe != null && (e.inheritAttrs = pe), fe && (e.components = fe), ue && (e.directives = ue), q && ko(e);
}
function Qc(e, t, s = Ge) {
  B(e) && (e = ri(e));
  for (const n in e) {
    const i = e[n];
    let r;
    Y(i) ? "default" in i ? r = Ys(
      i.from || n,
      i.default,
      !0
    ) : r = Ys(i.from || n) : r = Ys(i), /* @__PURE__ */ de(r) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => r.value,
      set: (o) => r.value = o
    }) : t[n] = r;
  }
}
function cr(e, t, s) {
  je(
    B(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Eo(e, t, s, n) {
  let i = n.includes(".") ? vo(s, n) : () => s[n];
  if (se(e)) {
    const r = t[e];
    U(r) && jn(i, r);
  } else if (U(e))
    jn(i, e.bind(s));
  else if (Y(e))
    if (B(e))
      e.forEach((r) => Eo(r, t, s, n));
    else {
      const r = U(e.handler) ? e.handler.bind(s) : t[e.handler];
      U(r) && jn(i, r, e);
    }
}
function Co(e) {
  const t = e.type, { mixins: s, extends: n } = t, {
    mixins: i,
    optionsCache: r,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = r.get(t);
  let c;
  return l ? c = l : !i.length && !s && !n ? c = t : (c = {}, i.length && i.forEach(
    (a) => nn(c, a, o, !0)
  ), nn(c, t, o)), Y(t) && r.set(t, c), c;
}
function nn(e, t, s, n = !1) {
  const { mixins: i, extends: r } = t;
  r && nn(e, r, s, !0), i && i.forEach(
    (o) => nn(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = Xc[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const Xc = {
  data: ar,
  props: fr,
  emits: fr,
  // objects
  methods: cs,
  computed: cs,
  // lifecycle
  beforeCreate: me,
  created: me,
  beforeMount: me,
  mounted: me,
  beforeUpdate: me,
  updated: me,
  beforeDestroy: me,
  beforeUnmount: me,
  destroyed: me,
  unmounted: me,
  activated: me,
  deactivated: me,
  errorCaptured: me,
  serverPrefetch: me,
  // assets
  components: cs,
  directives: cs,
  // watch
  watch: Zc,
  // provide / inject
  provide: ar,
  inject: zc
};
function ar(e, t) {
  return t ? e ? function() {
    return ae(
      U(e) ? e.call(this, this) : e,
      U(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function zc(e, t) {
  return cs(ri(e), ri(t));
}
function ri(e) {
  if (B(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++)
      t[e[s]] = e[s];
    return t;
  }
  return e;
}
function me(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function cs(e, t) {
  return e ? ae(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function fr(e, t) {
  return e ? B(e) && B(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : ae(
    /* @__PURE__ */ Object.create(null),
    lr(e),
    lr(t ?? {})
  ) : t;
}
function Zc(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = ae(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = me(e[n], t[n]);
  return s;
}
function Io() {
  return {
    app: null,
    config: {
      isNativeTag: qr,
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
let ea = 0;
function ta(e, t) {
  return function(n, i = null) {
    U(n) || (n = ae({}, n)), i != null && !Y(i) && (i = null);
    const r = Io(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let c = !1;
    const a = r.app = {
      _uid: ea++,
      _component: n,
      _props: i,
      _container: null,
      _context: r,
      _instance: null,
      version: Ra,
      get config() {
        return r.config;
      },
      set config(f) {
      },
      use(f, ...u) {
        return o.has(f) || (f && U(f.install) ? (o.add(f), f.install(a, ...u)) : U(f) && (o.add(f), f(a, ...u))), a;
      },
      mixin(f) {
        return r.mixins.includes(f) || r.mixins.push(f), a;
      },
      component(f, u) {
        return u ? (r.components[f] = u, a) : r.components[f];
      },
      directive(f, u) {
        return u ? (r.directives[f] = u, a) : r.directives[f];
      },
      mount(f, u, g) {
        if (!c) {
          const p = a._ceVNode || ot(n, i);
          return p.appContext = r, g === !0 ? g = "svg" : g === !1 && (g = void 0), e(p, f, g), c = !0, a._container = f, f.__vue_app__ = a, _n(p.component);
        }
      },
      onUnmount(f) {
        l.push(f);
      },
      unmount() {
        c && (je(
          l,
          a._instance,
          16
        ), e(null, a._container), delete a._container.__vue_app__);
      },
      provide(f, u) {
        return r.provides[f] = u, a;
      },
      runWithContext(f) {
        const u = Kt;
        Kt = a;
        try {
          return f();
        } finally {
          Kt = u;
        }
      }
    };
    return a;
  };
}
let Kt = null;
const sa = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${Pe(t)}Modifiers`] || e[`${At(t)}Modifiers`];
function na(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || Q;
  let i = s;
  const r = t.startsWith("update:"), o = r && sa(n, t.slice(7));
  o && (o.trim && (i = s.map((f) => se(f) ? f.trim() : f)), o.number && (i = s.map(dn)));
  let l, c = n[l = Ln(t)] || // also try camelCase event handler (#2249)
  n[l = Ln(Pe(t))];
  !c && r && (c = n[l = Ln(At(t))]), c && je(
    c,
    e,
    6,
    i
  );
  const a = n[l + "Once"];
  if (a) {
    if (!e.emitted)
      e.emitted = {};
    else if (e.emitted[l])
      return;
    e.emitted[l] = !0, je(
      a,
      e,
      6,
      i
    );
  }
}
const ia = /* @__PURE__ */ new WeakMap();
function Lo(e, t, s = !1) {
  const n = s ? ia : t.emitsCache, i = n.get(e);
  if (i !== void 0)
    return i;
  const r = e.emits;
  let o = {}, l = !1;
  if (!U(e)) {
    const c = (a) => {
      const f = Lo(a, t, !0);
      f && (l = !0, ae(o, f));
    };
    !s && t.mixins.length && t.mixins.forEach(c), e.extends && c(e.extends), e.mixins && e.mixins.forEach(c);
  }
  return !r && !l ? (Y(e) && n.set(e, null), null) : (B(r) ? r.forEach((c) => o[c] = null) : ae(o, r), Y(e) && n.set(e, o), o);
}
function wn(e, t) {
  return !e || !fn(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), W(e, t[0].toLowerCase() + t.slice(1)) || W(e, At(t)) || W(e, t));
}
function ur(e) {
  const {
    type: t,
    vnode: s,
    proxy: n,
    withProxy: i,
    propsOptions: [r],
    slots: o,
    attrs: l,
    emit: c,
    render: a,
    renderCache: f,
    props: u,
    data: g,
    setupState: p,
    ctx: k,
    inheritAttrs: m
  } = e, _ = tn(e);
  let b, y;
  try {
    if (s.shapeFlag & 4) {
      const A = i || n, N = A;
      b = Je(
        a.call(
          N,
          A,
          f,
          u,
          p,
          g,
          k
        )
      ), y = l;
    } else {
      const A = t;
      b = Je(
        A.length > 1 ? A(
          u,
          { attrs: l, slots: o, emit: c }
        ) : A(
          u,
          null
        )
      ), y = t.props ? l : ra(l);
    }
  } catch (A) {
    bs.length = 0, yn(A, e, 1), b = ot(gt);
  }
  let S = b;
  if (y && m !== !1) {
    const A = Object.keys(y), { shapeFlag: N } = S;
    A.length && N & 7 && (r && A.some(un) && (y = oa(
      y,
      r
    )), S = qt(S, y, !1, !0));
  }
  return s.dirs && (S = qt(S, null, !1, !0), S.dirs = S.dirs ? S.dirs.concat(s.dirs) : s.dirs), s.transition && Ci(S, s.transition), b = S, tn(_), b;
}
const ra = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || fn(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, oa = (e, t) => {
  const s = {};
  for (const n in e)
    (!un(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function la(e, t, s) {
  const { props: n, children: i, component: r } = e, { props: o, children: l, patchFlag: c } = t, a = r.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (s && c >= 0) {
    if (c & 1024)
      return !0;
    if (c & 16)
      return n ? hr(n, o, a) : !!o;
    if (c & 8) {
      const f = t.dynamicProps;
      for (let u = 0; u < f.length; u++) {
        const g = f[u];
        if ($o(o, n, g) && !wn(a, g))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? hr(n, o, a) : !0 : !!o;
  return !1;
}
function hr(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < n.length; i++) {
    const r = n[i];
    if ($o(t, e, r) && !wn(s, r))
      return !0;
  }
  return !1;
}
function $o(e, t, s) {
  const n = e[s], i = t[s];
  return s === "style" && Y(n) && Y(i) ? !Gt(n, i) : n !== i;
}
function ca({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const i = t.subTree;
    if (i.suspense && i.suspense.activeBranch === e && (i.suspense.vnode.el = i.el = n, e = i), i === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const Mo = {}, Po = () => Object.create(Mo), Do = (e) => Object.getPrototypeOf(e) === Mo;
function aa(e, t, s, n = !1) {
  const i = {}, r = Po();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), Ro(e, t, i, r);
  for (const o in e.propsOptions[0])
    o in i || (i[o] = void 0);
  s ? e.props = n ? i : /* @__PURE__ */ gc(i) : e.type.props ? e.props = i : e.props = r, e.attrs = r;
}
function fa(e, t, s, n) {
  const {
    props: i,
    attrs: r,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ H(i), [c] = e.propsOptions;
  let a = !1;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    (n || o > 0) && !(o & 16)
  ) {
    if (o & 8) {
      const f = e.vnode.dynamicProps;
      for (let u = 0; u < f.length; u++) {
        let g = f[u];
        if (wn(e.emitsOptions, g))
          continue;
        const p = t[g];
        if (c)
          if (W(r, g))
            p !== r[g] && (r[g] = p, a = !0);
          else {
            const k = Pe(g);
            i[k] = oi(
              c,
              l,
              k,
              p,
              e,
              !1
            );
          }
        else
          p !== r[g] && (r[g] = p, a = !0);
      }
    }
  } else {
    Ro(e, t, i, r) && (a = !0);
    let f;
    for (const u in l)
      (!t || // for camelCase
      !W(t, u) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((f = At(u)) === u || !W(t, f))) && (c ? s && // for camelCase
      (s[u] !== void 0 || // for kebab-case
      s[f] !== void 0) && (i[u] = oi(
        c,
        l,
        u,
        void 0,
        e,
        !0
      )) : delete i[u]);
    if (r !== l)
      for (const u in r)
        (!t || !W(t, u)) && (delete r[u], a = !0);
  }
  a && nt(e.attrs, "set", "");
}
function Ro(e, t, s, n) {
  const [i, r] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let c in t) {
      if (hs(c))
        continue;
      const a = t[c];
      let f;
      i && W(i, f = Pe(c)) ? !r || !r.includes(f) ? s[f] = a : (l || (l = {}))[f] = a : wn(e.emitsOptions, c) || (!(c in n) || a !== n[c]) && (n[c] = a, o = !0);
    }
  if (r) {
    const c = /* @__PURE__ */ H(s), a = l || Q;
    for (let f = 0; f < r.length; f++) {
      const u = r[f];
      s[u] = oi(
        i,
        c,
        u,
        a[u],
        e,
        !W(a, u)
      );
    }
  }
  return o;
}
function oi(e, t, s, n, i, r) {
  const o = e[s];
  if (o != null) {
    const l = W(o, "default");
    if (l && n === void 0) {
      const c = o.default;
      if (o.type !== Function && !o.skipFactory && U(c)) {
        const { propsDefaults: a } = i;
        if (s in a)
          n = a[s];
        else {
          const f = Ls(i);
          n = a[s] = c.call(
            null,
            t
          ), f();
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
    ] && (n === "" || n === At(s)) && (n = !0));
  }
  return n;
}
const ua = /* @__PURE__ */ new WeakMap();
function jo(e, t, s = !1) {
  const n = s ? ua : t.propsCache, i = n.get(e);
  if (i)
    return i;
  const r = e.props, o = {}, l = [];
  let c = !1;
  if (!U(e)) {
    const f = (u) => {
      c = !0;
      const [g, p] = jo(u, t, !0);
      ae(o, g), p && l.push(...p);
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  if (!r && !c)
    return Y(e) && n.set(e, jt), jt;
  if (B(r))
    for (let f = 0; f < r.length; f++) {
      const u = Pe(r[f]);
      dr(u) && (o[u] = Q);
    }
  else if (r)
    for (const f in r) {
      const u = Pe(f);
      if (dr(u)) {
        const g = r[f], p = o[u] = B(g) || U(g) ? { type: g } : ae({}, g), k = p.type;
        let m = !1, _ = !0;
        if (B(k))
          for (let b = 0; b < k.length; ++b) {
            const y = k[b], S = U(y) && y.name;
            if (S === "Boolean") {
              m = !0;
              break;
            } else S === "String" && (_ = !1);
          }
        else
          m = U(k) && k.name === "Boolean";
        p[
          0
          /* shouldCast */
        ] = m, p[
          1
          /* shouldCastTrue */
        ] = _, (m || W(p, "default")) && l.push(u);
      }
    }
  const a = [o, l];
  return Y(e) && n.set(e, a), a;
}
function dr(e) {
  return e[0] !== "$" && !hs(e);
}
const Ii = (e) => e === "_" || e === "_ctx" || e === "$stable", Li = (e) => B(e) ? e.map(Je) : [Je(e)], ha = (e, t, s) => {
  if (t._n)
    return t;
  const n = Ec((...i) => Li(t(...i)), s);
  return n._c = !1, n;
}, Bo = (e, t, s) => {
  const n = e._ctx;
  for (const i in e) {
    if (Ii(i)) continue;
    const r = e[i];
    if (U(r))
      t[i] = ha(i, r, n);
    else if (r != null) {
      const o = Li(r);
      t[i] = () => o;
    }
  }
}, Fo = (e, t) => {
  const s = Li(t);
  e.slots.default = () => s;
}, Ko = (e, t, s) => {
  for (const n in t)
    (s || !Ii(n)) && (e[n] = t[n]);
}, da = (e, t, s) => {
  const n = e.slots = Po();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Ko(n, t, s), s && Gr(n, "_", i, !0)) : Bo(t, n);
  } else t && Fo(e, t);
}, pa = (e, t, s) => {
  const { vnode: n, slots: i } = e;
  let r = !0, o = Q;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? r = !1 : Ko(i, t, s) : (r = !t.$stable, Bo(t, i)), o = t;
  } else t && (Fo(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !Ii(l) && o[l] == null && delete i[l];
}, Oe = wa;
function ga(e) {
  return ma(e);
}
function ma(e, t) {
  const s = pn();
  s.__VUE__ = !0;
  const {
    insert: n,
    remove: i,
    patchProp: r,
    createElement: o,
    createText: l,
    createComment: c,
    setText: a,
    setElementText: f,
    parentNode: u,
    nextSibling: g,
    setScopeId: p = Ge,
    insertStaticContent: k
  } = e, m = (h, d, w, E = null, T = null, v = null, L = void 0, I = null, C = !!d.dynamicChildren) => {
    if (h === d)
      return;
    h && !rs(h, d) && (E = Bs(h), Fe(h, T, v, !0), h = null), d.patchFlag === -2 && (C = !1, d.dynamicChildren = null);
    const { type: O, ref: j, shapeFlag: $ } = d;
    switch (O) {
      case Sn:
        _(h, d, w, E);
        break;
      case gt:
        b(h, d, w, E);
        break;
      case Un:
        h == null && y(d, w, E, L);
        break;
      case Te:
        fe(
          h,
          d,
          w,
          E,
          T,
          v,
          L,
          I,
          C
        );
        break;
      default:
        $ & 1 ? N(
          h,
          d,
          w,
          E,
          T,
          v,
          L,
          I,
          C
        ) : $ & 6 ? ue(
          h,
          d,
          w,
          E,
          T,
          v,
          L,
          I,
          C
        ) : ($ & 64 || $ & 128) && O.process(
          h,
          d,
          w,
          E,
          T,
          v,
          L,
          I,
          C,
          ss
        );
    }
    j != null && T ? gs(j, h && h.ref, v, d || h, !d) : j == null && h && h.ref != null && gs(h.ref, null, v, h, !0);
  }, _ = (h, d, w, E) => {
    if (h == null)
      n(
        d.el = l(d.children),
        w,
        E
      );
    else {
      const T = d.el = h.el;
      d.children !== h.children && a(T, d.children);
    }
  }, b = (h, d, w, E) => {
    h == null ? n(
      d.el = c(d.children || ""),
      w,
      E
    ) : d.el = h.el;
  }, y = (h, d, w, E) => {
    [h.el, h.anchor] = k(
      h.children,
      d,
      w,
      E,
      h.el,
      h.anchor
    );
  }, S = ({ el: h, anchor: d }, w, E) => {
    let T;
    for (; h && h !== d; )
      T = g(h), n(h, w, E), h = T;
    n(d, w, E);
  }, A = ({ el: h, anchor: d }) => {
    let w;
    for (; h && h !== d; )
      w = g(h), i(h), h = w;
    i(d);
  }, N = (h, d, w, E, T, v, L, I, C) => {
    if (d.type === "svg" ? L = "svg" : d.type === "math" && (L = "mathml"), h == null)
      x(
        d,
        w,
        E,
        T,
        v,
        L,
        I,
        C
      );
    else {
      const O = h.el && h.el._isVueCE ? h.el : null;
      try {
        O && O._beginPatch(), q(
          h,
          d,
          T,
          v,
          L,
          I,
          C
        );
      } finally {
        O && O._endPatch();
      }
    }
  }, x = (h, d, w, E, T, v, L, I) => {
    let C, O;
    const { props: j, shapeFlag: $, transition: P, dirs: F } = h;
    if (C = h.el = o(
      h.type,
      v,
      j && j.is,
      j
    ), $ & 8 ? f(C, h.children) : $ & 16 && M(
      h.children,
      C,
      null,
      E,
      T,
      xn(h, v),
      L,
      I
    ), F && bt(h, null, E, "created"), D(C, h, h.scopeId, L, E), j) {
      for (const G in j)
        G !== "value" && !hs(G) && r(C, G, null, j[G], v, E);
      "value" in j && r(C, "value", null, j.value, v), (O = j.onVnodeBeforeMount) && Ve(O, E, h);
    }
    F && bt(h, null, E, "beforeMount");
    const V = ya(T, P);
    V && P.beforeEnter(C), n(C, d, w), ((O = j && j.onVnodeMounted) || V || F) && Oe(() => {
      try {
        O && Ve(O, E, h), V && P.enter(C), F && bt(h, null, E, "mounted");
      } finally {
      }
    }, T);
  }, D = (h, d, w, E, T) => {
    if (w && p(h, w), E)
      for (let v = 0; v < E.length; v++)
        p(h, E[v]);
    if (T) {
      let v = T.subTree;
      if (d === v || qo(v.type) && (v.ssContent === d || v.ssFallback === d)) {
        const L = T.vnode;
        D(
          h,
          L,
          L.scopeId,
          L.slotScopeIds,
          T.parent
        );
      }
    }
  }, M = (h, d, w, E, T, v, L, I, C = 0) => {
    for (let O = C; O < h.length; O++) {
      const j = h[O] = I ? st(h[O]) : Je(h[O]);
      m(
        null,
        j,
        d,
        w,
        E,
        T,
        v,
        L,
        I
      );
    }
  }, q = (h, d, w, E, T, v, L) => {
    const I = d.el = h.el;
    let { patchFlag: C, dynamicChildren: O, dirs: j } = d;
    C |= h.patchFlag & 16;
    const $ = h.props || Q, P = d.props || Q;
    let F;
    if (w && wt(w, !1), (F = P.onVnodeBeforeUpdate) && Ve(F, w, d, h), j && bt(d, h, w, "beforeUpdate"), w && wt(w, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    O && (!h.dynamicChildren || h.dynamicChildren.length !== O.length) && (C = 0, L = !1, O = null), ($.innerHTML && P.innerHTML == null || $.textContent && P.textContent == null) && f(I, ""), O ? ee(
      h.dynamicChildren,
      O,
      I,
      w,
      E,
      xn(d, T),
      v
    ) : L || X(
      h,
      d,
      I,
      null,
      w,
      E,
      xn(d, T),
      v,
      !1
    ), C > 0) {
      if (C & 16)
        pe(I, $, P, w, T);
      else if (C & 2 && $.class !== P.class && r(I, "class", null, P.class, T), C & 4 && r(I, "style", $.style, P.style, T), C & 8) {
        const V = d.dynamicProps;
        for (let G = 0; G < V.length; G++) {
          const J = V[G], oe = $[J], le = P[J];
          (le !== oe || J === "value") && r(I, J, oe, le, T, w);
        }
      }
      C & 1 && h.children !== d.children && f(I, d.children);
    } else !L && O == null && pe(I, $, P, w, T);
    ((F = P.onVnodeUpdated) || j) && Oe(() => {
      F && Ve(F, w, d, h), j && bt(d, h, w, "updated");
    }, E);
  }, ee = (h, d, w, E, T, v, L) => {
    for (let I = 0; I < d.length; I++) {
      const C = h[I], O = d[I], j = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        C.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (C.type === Te || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !rs(C, O) || // - In the case of a component, it could contain anything.
        C.shapeFlag & 198) ? u(C.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          w
        )
      );
      m(
        C,
        O,
        j,
        null,
        E,
        T,
        v,
        L,
        !0
      );
    }
  }, pe = (h, d, w, E, T) => {
    if (d !== w) {
      if (d !== Q)
        for (const v in d)
          !hs(v) && !(v in w) && r(
            h,
            v,
            d[v],
            null,
            T,
            E
          );
      for (const v in w) {
        if (hs(v)) continue;
        const L = w[v], I = d[v];
        L !== I && v !== "value" && r(h, v, I, L, T, E);
      }
      "value" in w && r(h, "value", d.value, w.value, T);
    }
  }, fe = (h, d, w, E, T, v, L, I, C) => {
    const O = d.el = h ? h.el : l(""), j = d.anchor = h ? h.anchor : l("");
    let { patchFlag: $, dynamicChildren: P, slotScopeIds: F } = d;
    F && (I = I ? I.concat(F) : F), h == null ? (n(O, w, E), n(j, w, E), M(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      d.children || [],
      w,
      j,
      T,
      v,
      L,
      I,
      C
    )) : $ > 0 && $ & 64 && P && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    h.dynamicChildren && h.dynamicChildren.length === P.length ? (ee(
      h.dynamicChildren,
      P,
      w,
      T,
      v,
      L,
      I
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (d.key != null || T && d === T.subTree) && xo(
      h,
      d,
      !0
      /* shallow */
    )) : X(
      h,
      d,
      w,
      j,
      T,
      v,
      L,
      I,
      C
    );
  }, ue = (h, d, w, E, T, v, L, I, C) => {
    d.slotScopeIds = I, h == null ? d.shapeFlag & 512 ? T.ctx.activate(
      d,
      w,
      E,
      L,
      C
    ) : Et(
      d,
      w,
      E,
      T,
      v,
      L,
      C
    ) : Yi(h, d, C);
  }, Et = (h, d, w, E, T, v, L) => {
    const I = h.component = Ea(
      h,
      E,
      T
    );
    if (Oo(h) && (I.ctx.renderer = ss), Ia(I, !1, L), I.asyncDep) {
      if (T && T.registerDep(I, ge, L), !h.el) {
        const C = I.subTree = ot(gt);
        b(null, C, d, w), h.placeholder = C.el;
      }
    } else
      ge(
        I,
        h,
        d,
        w,
        T,
        v,
        L
      );
  }, Yi = (h, d, w) => {
    const E = d.component = h.component;
    if (la(h, d, w))
      if (E.asyncDep && !E.asyncResolved) {
        te(E, d, w);
        return;
      } else
        E.next = d, E.update();
    else
      d.el = h.el, E.vnode = d;
  }, ge = (h, d, w, E, T, v, L) => {
    const I = () => {
      if (h.isMounted) {
        let { next: $, bu: P, u: F, parent: V, vnode: G } = h;
        {
          const xe = Uo(h);
          if (xe) {
            $ && ($.el = G.el, te(h, $, L)), xe.asyncDep.then(() => {
              Oe(() => {
                h.isUnmounted || O();
              }, T);
            });
            return;
          }
        }
        let J = $, oe;
        wt(h, !1), $ ? ($.el = G.el, te(h, $, L)) : $ = G, P && Js(P), (oe = $.props && $.props.onVnodeBeforeUpdate) && Ve(oe, V, $, G), wt(h, !0);
        const le = ur(h), Ke = h.subTree;
        h.subTree = le, m(
          Ke,
          le,
          // parent may have changed if it's in a teleport
          u(Ke.el),
          // anchor may have changed if it's in a fragment
          Bs(Ke),
          h,
          T,
          v
        ), $.el = le.el, J === null && ca(h, le.el), F && Oe(F, T), (oe = $.props && $.props.onVnodeUpdated) && Oe(
          () => Ve(oe, V, $, G),
          T
        );
      } else {
        let $;
        const { el: P, props: F } = d, { bm: V, m: G, parent: J, root: oe, type: le } = h, Ke = ms(d);
        wt(h, !1), V && Js(V), !Ke && ($ = F && F.onVnodeBeforeMount) && Ve($, J, d), wt(h, !0);
        {
          oe.ce && oe.ce._hasShadowRoot() && oe.ce._injectChildStyle(
            le,
            h.parent ? h.parent.type : void 0
          );
          const xe = h.subTree = ur(h);
          m(
            null,
            xe,
            w,
            E,
            h,
            T,
            v
          ), d.el = xe.el;
        }
        if (G && Oe(G, T), !Ke && ($ = F && F.onVnodeMounted)) {
          const xe = d;
          Oe(
            () => Ve($, J, xe),
            T
          );
        }
        (d.shapeFlag & 256 || J && ms(J.vnode) && J.vnode.shapeFlag & 256) && h.a && Oe(h.a, T), h.isMounted = !0, d = w = E = null;
      }
    };
    h.scope.on();
    const C = h.effect = new Zr(I);
    h.scope.off();
    const O = h.update = C.run.bind(C), j = h.job = C.runIfDirty.bind(C);
    j.i = h, j.id = h.uid, C.scheduler = () => Ei(j), wt(h, !0), O();
  }, te = (h, d, w) => {
    d.component = h;
    const E = h.vnode.props;
    h.vnode = d, h.next = null, fa(h, d.props, E, w), pa(h, d.children, w), Xe(), ir(h), ze();
  }, X = (h, d, w, E, T, v, L, I, C = !1) => {
    const O = h && h.children, j = h ? h.shapeFlag : 0, $ = d.children, { patchFlag: P, shapeFlag: F } = d;
    if (P > 0) {
      if (P & 128) {
        js(
          O,
          $,
          w,
          E,
          T,
          v,
          L,
          I,
          C
        );
        return;
      } else if (P & 256) {
        mt(
          O,
          $,
          w,
          E,
          T,
          v,
          L,
          I,
          C
        );
        return;
      }
    }
    F & 8 ? (j & 16 && ts(O, T, v), $ !== O && f(w, $)) : j & 16 ? F & 16 ? js(
      O,
      $,
      w,
      E,
      T,
      v,
      L,
      I,
      C
    ) : ts(O, T, v, !0) : (j & 8 && f(w, ""), F & 16 && M(
      $,
      w,
      E,
      T,
      v,
      L,
      I,
      C
    ));
  }, mt = (h, d, w, E, T, v, L, I, C) => {
    h = h || jt, d = d || jt;
    const O = h.length, j = d.length, $ = Math.min(O, j);
    let P;
    for (P = 0; P < $; P++) {
      const F = d[P] = C ? st(d[P]) : Je(d[P]);
      m(
        h[P],
        F,
        w,
        null,
        T,
        v,
        L,
        I,
        C
      );
    }
    O > j ? ts(
      h,
      T,
      v,
      !0,
      !1,
      $
    ) : M(
      d,
      w,
      E,
      T,
      v,
      L,
      I,
      C,
      $
    );
  }, js = (h, d, w, E, T, v, L, I, C) => {
    let O = 0;
    const j = d.length;
    let $ = h.length - 1, P = j - 1;
    for (; O <= $ && O <= P; ) {
      const F = h[O], V = d[O] = C ? st(d[O]) : Je(d[O]);
      if (rs(F, V))
        m(
          F,
          V,
          w,
          null,
          T,
          v,
          L,
          I,
          C
        );
      else
        break;
      O++;
    }
    for (; O <= $ && O <= P; ) {
      const F = h[$], V = d[P] = C ? st(d[P]) : Je(d[P]);
      if (rs(F, V))
        m(
          F,
          V,
          w,
          null,
          T,
          v,
          L,
          I,
          C
        );
      else
        break;
      $--, P--;
    }
    if (O > $) {
      if (O <= P) {
        const F = P + 1, V = F < j ? d[F].el : E;
        for (; O <= P; )
          m(
            null,
            d[O] = C ? st(d[O]) : Je(d[O]),
            w,
            V,
            T,
            v,
            L,
            I,
            C
          ), O++;
      }
    } else if (O > P)
      for (; O <= $; )
        Fe(h[O], T, v, !0), O++;
    else {
      const F = O, V = O, G = /* @__PURE__ */ new Map();
      for (O = V; O <= P; O++) {
        const Ne = d[O] = C ? st(d[O]) : Je(d[O]);
        Ne.key != null && G.set(Ne.key, O);
      }
      let J, oe = 0;
      const le = P - V + 1;
      let Ke = !1, xe = 0;
      const ns = new Array(le);
      for (O = 0; O < le; O++) ns[O] = 0;
      for (O = F; O <= $; O++) {
        const Ne = h[O];
        if (oe >= le) {
          Fe(Ne, T, v, !0);
          continue;
        }
        let Ue;
        if (Ne.key != null)
          Ue = G.get(Ne.key);
        else
          for (J = V; J <= P; J++)
            if (ns[J - V] === 0 && rs(Ne, d[J])) {
              Ue = J;
              break;
            }
        Ue === void 0 ? Fe(Ne, T, v, !0) : (ns[Ue - V] = O + 1, Ue >= xe ? xe = Ue : Ke = !0, m(
          Ne,
          d[Ue],
          w,
          null,
          T,
          v,
          L,
          I,
          C
        ), oe++);
      }
      const Xi = Ke ? ba(ns) : jt;
      for (J = Xi.length - 1, O = le - 1; O >= 0; O--) {
        const Ne = V + O, Ue = d[Ne], zi = d[Ne + 1], Zi = Ne + 1 < j ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          zi.el || Vo(zi)
        ) : E;
        ns[O] === 0 ? m(
          null,
          Ue,
          w,
          Zi,
          T,
          v,
          L,
          I,
          C
        ) : Ke && (J < 0 || O !== Xi[J] ? yt(Ue, w, Zi, 2) : J--);
      }
    }
  }, yt = (h, d, w, E, T = null) => {
    const { el: v, type: L, transition: I, children: C, shapeFlag: O } = h;
    if (O & 6) {
      yt(h.component.subTree, d, w, E);
      return;
    }
    if (O & 128) {
      h.suspense.move(d, w, E);
      return;
    }
    if (O & 64) {
      L.move(h, d, w, ss);
      return;
    }
    if (L === Te) {
      n(v, d, w);
      for (let $ = 0; $ < C.length; $++)
        yt(C[$], d, w, E);
      n(h.anchor, d, w);
      return;
    }
    if (L === Un) {
      S(h, d, w);
      return;
    }
    if (E !== 2 && O & 1 && I)
      if (E === 0)
        I.persisted && !v[Bn] ? n(v, d, w) : (I.beforeEnter(v), n(v, d, w), Oe(() => I.enter(v), T));
      else {
        const { leave: $, delayLeave: P, afterLeave: F } = I, V = () => {
          h.ctx.isUnmounted ? i(v) : n(v, d, w);
        }, G = () => {
          const J = v._isLeaving || !!v[Bn];
          v._isLeaving && v[Bn](
            !0
            /* cancelled */
          ), I.persisted && !J ? V() : $(v, () => {
            V(), F && F();
          });
        };
        P ? P(v, V, G) : G();
      }
    else
      n(v, d, w);
  }, Fe = (h, d, w, E = !1, T = !1) => {
    const {
      type: v,
      props: L,
      ref: I,
      children: C,
      dynamicChildren: O,
      shapeFlag: j,
      patchFlag: $,
      dirs: P,
      cacheIndex: F,
      memo: V
    } = h;
    if ($ === -2 && (T = !1), I != null && (Xe(), gs(I, null, w, h, !0), ze()), F != null && (d.renderCache[F] = void 0), j & 256) {
      d.ctx.deactivate(h);
      return;
    }
    const G = j & 1 && P, J = !ms(h);
    let oe;
    if (J && (oe = L && L.onVnodeBeforeUnmount) && Ve(oe, d, h), j & 6)
      Bl(h.component, w, E);
    else {
      if (j & 128) {
        h.suspense.unmount(w, E);
        return;
      }
      G && bt(h, null, d, "beforeUnmount"), j & 64 ? h.type.remove(
        h,
        d,
        w,
        ss,
        E
      ) : O && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !O.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (v !== Te || $ > 0 && $ & 64) ? ts(
        O,
        d,
        w,
        !1,
        !0
      ) : (v === Te && $ & 384 || !T && j & 16) && ts(C, d, w), E && Gi(h);
    }
    const le = V != null && F == null;
    (J && (oe = L && L.onVnodeUnmounted) || G || le) && Oe(() => {
      oe && Ve(oe, d, h), G && bt(h, null, d, "unmounted"), le && (h.el = null);
    }, w);
  }, Gi = (h) => {
    const { type: d, el: w, anchor: E, transition: T } = h;
    if (d === Te) {
      jl(w, E);
      return;
    }
    if (d === Un) {
      A(h);
      return;
    }
    const v = () => {
      i(w), T && !T.persisted && T.afterLeave && T.afterLeave();
    };
    if (h.shapeFlag & 1 && T && !T.persisted) {
      const { leave: L, delayLeave: I } = T, C = () => L(w, v);
      I ? I(h.el, v, C) : C();
    } else
      v();
  }, jl = (h, d) => {
    let w;
    for (; h !== d; )
      w = g(h), i(h), h = w;
    i(d);
  }, Bl = (h, d, w) => {
    const { bum: E, scope: T, job: v, subTree: L, um: I, m: C, a: O } = h;
    pr(C), pr(O), E && Js(E), T.stop(), v && (v.flags |= 8, Fe(L, h, d, w)), I && Oe(I, d), Oe(() => {
      h.isUnmounted = !0;
    }, d);
  }, ts = (h, d, w, E = !1, T = !1, v = 0) => {
    for (let L = v; L < h.length; L++)
      Fe(h[L], d, w, E, T);
  }, Bs = (h) => {
    if (h.shapeFlag & 6)
      return Bs(h.component.subTree);
    if (h.shapeFlag & 128)
      return h.suspense.next();
    const d = g(h.anchor || h.el), w = d && d[Mc];
    return w ? g(w) : d;
  };
  let In = !1;
  const Qi = (h, d, w) => {
    let E;
    h == null ? d._vnode && (Fe(d._vnode, null, null, !0), E = d._vnode.component) : m(
      d._vnode || null,
      h,
      d,
      null,
      null,
      null,
      w
    ), d._vnode = h, In || (In = !0, ir(E), bo(), In = !1);
  }, ss = {
    p: m,
    um: Fe,
    m: yt,
    r: Gi,
    mt: Et,
    mc: M,
    pc: X,
    pbc: ee,
    n: Bs,
    o: e
  };
  return {
    render: Qi,
    hydrate: void 0,
    createApp: ta(Qi)
  };
}
function xn({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function wt({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function ya(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function xo(e, t, s = !1) {
  const n = e.children, i = t.children;
  if (B(n) && B(i))
    for (let r = 0; r < n.length; r++) {
      const o = n[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = st(i[r]), l.el = o.el), !s && l.patchFlag !== -2 && xo(o, l)), l.type === Sn && (l.patchFlag === -1 && (l = i[r] = st(l)), l.el = o.el), l.type === gt && !l.el && (l.el = o.el);
    }
}
function ba(e) {
  const t = e.slice(), s = [0];
  let n, i, r, o, l;
  const c = e.length;
  for (n = 0; n < c; n++) {
    const a = e[n];
    if (a !== 0) {
      if (i = s[s.length - 1], e[i] < a) {
        t[n] = i, s.push(n);
        continue;
      }
      for (r = 0, o = s.length - 1; r < o; )
        l = r + o >> 1, e[s[l]] < a ? r = l + 1 : o = l;
      a < e[s[r]] && (r > 0 && (t[n] = s[r - 1]), s[r] = n);
    }
  }
  for (r = s.length, o = s[r - 1]; r-- > 0; )
    s[r] = o, o = t[o];
  return s;
}
function Uo(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : Uo(t);
}
function pr(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function Vo(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? Vo(t.subTree) : null;
}
const qo = (e) => e.__isSuspense;
function wa(e, t) {
  t && t.pendingBranch ? B(e) ? t.effects.push(...e) : t.effects.push(e) : Ac(e);
}
const Te = /* @__PURE__ */ Symbol.for("v-fgt"), Sn = /* @__PURE__ */ Symbol.for("v-txt"), gt = /* @__PURE__ */ Symbol.for("v-cmt"), Un = /* @__PURE__ */ Symbol.for("v-stc"), bs = [];
let Ae = null;
function ye(e = !1) {
  bs.push(Ae = e ? null : []);
}
function Sa() {
  bs.pop(), Ae = bs[bs.length - 1] || null;
}
let Os = 1;
function gr(e, t = !1) {
  Os += e, e < 0 && Ae && t && (Ae.hasOnce = !0);
}
function Ho(e) {
  return e.dynamicChildren = Os > 0 ? Ae || jt : null, Sa(), Os > 0 && Ae && Ae.push(e), e;
}
function ke(e, t, s, n, i, r) {
  return Ho(
    R(
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
function _a(e, t, s, n, i) {
  return Ho(
    ot(
      e,
      t,
      s,
      n,
      i,
      !0
    )
  );
}
function Wo(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function rs(e, t) {
  return e.type === t.type && e.key === t.key;
}
const Jo = ({ key: e }) => e ?? null, Gs = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? se(e) || /* @__PURE__ */ de(e) || U(e) ? { i: Ce, r: e, k: t, f: !!s } : e : null);
function R(e, t = null, s = null, n = 0, i = null, r = e === Te ? 0 : 1, o = !1, l = !1) {
  const c = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Jo(t),
    ref: t && Gs(t),
    scopeId: So,
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
    ctx: Ce
  };
  return l ? (rn(c, s), r & 128 && e.normalize(c)) : s && (c.shapeFlag |= se(s) ? 8 : 16), Os > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  Ae && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (c.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  c.patchFlag !== 32 && Ae.push(c), c;
}
const ot = va;
function va(e, t = null, s = null, n = 0, i = null, r = !1) {
  if ((!e || e === Jc) && (e = gt), Wo(e)) {
    const l = qt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && rn(l, s), Os > 0 && !r && Ae && (l.shapeFlag & 6 ? Ae[Ae.indexOf(e)] = l : Ae.push(l)), l.patchFlag = -2, l;
  }
  if (Pa(e) && (e = e.__vccOpts), t) {
    t = ka(t);
    let { class: l, style: c } = t;
    l && !se(l) && (t.class = wi(l)), Y(c) && (/* @__PURE__ */ Ai(c) && !B(c) && (c = ae({}, c)), t.style = bi(c));
  }
  const o = se(e) ? 1 : qo(e) ? 128 : Pc(e) ? 64 : Y(e) ? 4 : U(e) ? 2 : 0;
  return R(
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
function ka(e) {
  return e ? /* @__PURE__ */ Ai(e) || Do(e) ? ae({}, e) : e : null;
}
function qt(e, t, s = !1, n = !1) {
  const { props: i, ref: r, patchFlag: o, children: l, transition: c } = e, a = t ? Ta(i || {}, t) : i, f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: a,
    key: a && Jo(a),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && r ? B(r) ? r.concat(Gs(t)) : [r, Gs(t)] : Gs(t)
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
    patchFlag: t && e.type !== Te ? o === -1 ? 16 : o | 16 : o,
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
    ssContent: e.ssContent && qt(e.ssContent),
    ssFallback: e.ssFallback && qt(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return c && n && Ci(
    f,
    c.clone(f)
  ), f;
}
function Oa(e = " ", t = 0) {
  return ot(Sn, null, e, t);
}
function os(e = "", t = !1) {
  return t ? (ye(), _a(gt, null, e)) : ot(gt, null, e);
}
function Je(e) {
  return e == null || typeof e == "boolean" ? ot(gt) : B(e) ? ot(
    Te,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Wo(e) ? st(e) : ot(Sn, null, String(e));
}
function st(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : qt(e);
}
function rn(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (B(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), rn(e, i()), i._c && (i._d = !0));
      return;
    } else {
      s = 32;
      const i = t._;
      !i && !Do(t) ? t._ctx = Ce : i === 3 && Ce && (Ce.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (U(t)) {
    if (n & 65) {
      rn(e, { default: t });
      return;
    }
    t = { default: t, _ctx: Ce }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [Oa(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function Ta(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const i in n)
      if (i === "class")
        t.class !== n.class && (t.class = wi([t.class, n.class]));
      else if (i === "style")
        t.style = bi([t.style, n.style]);
      else if (fn(i)) {
        const r = t[i], o = n[i];
        o && r !== o && !(B(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !un(i) && (t[i] = o);
      } else i !== "" && (t[i] = n[i]);
  }
  return t;
}
function Ve(e, t, s, n = null) {
  je(e, t, 7, [
    s,
    n
  ]);
}
const Na = Io();
let Aa = 0;
function Ea(e, t, s) {
  const n = e.type, i = (t ? t.appContext : e.appContext) || Na, r = {
    uid: Aa++,
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
    scope: new Ql(
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
    propsOptions: jo(n, i),
    emitsOptions: Lo(n, i),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: Q,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: Q,
    data: Q,
    props: Q,
    attrs: Q,
    slots: Q,
    refs: Q,
    setupState: Q,
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
  return r.ctx = { _: r }, r.root = t ? t.root : r, r.emit = na.bind(null, r), e.ce && e.ce(r), r;
}
let Se = null;
const Ca = () => Se || Ce;
let on, li;
{
  const e = pn(), t = (s, n) => {
    let i;
    return (i = e[s]) || (i = e[s] = []), i.push(n), (r) => {
      i.length > 1 ? i.forEach((o) => o(r)) : i[0](r);
    };
  };
  on = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => Se = s
  ), li = t(
    "__VUE_SSR_SETTERS__",
    (s) => Ts = s
  );
}
const Ls = (e) => {
  const t = Se;
  return on(e), e.scope.on(), () => {
    e.scope.off(), on(t);
  };
}, mr = () => {
  Se && Se.scope.off(), on(null);
};
function Yo(e) {
  return e.vnode.shapeFlag & 4;
}
let Ts = !1;
function Ia(e, t = !1, s = !1) {
  t && li(t);
  const { props: n, children: i } = e.vnode, r = Yo(e);
  aa(e, n, r, t), da(e, i, s || t);
  const o = r ? La(e, t) : void 0;
  return t && li(!1), o;
}
function La(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, Yc);
  const { setup: n } = s;
  if (n) {
    Xe();
    const i = e.setupContext = n.length > 1 ? Ma(e) : null, r = Ls(e), o = Is(
      n,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = Hr(o);
    if (ze(), r(), (l || e.sp) && !ms(e) && ko(e), l) {
      if (o.then(mr, mr), t)
        return o.then((c) => {
          yr(e, c);
        }).catch((c) => {
          yn(c, e, 0);
        });
      e.asyncDep = o;
    } else
      yr(e, o);
  } else
    Go(e);
}
function yr(e, t, s) {
  U(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : Y(t) && (e.setupState = po(t)), Go(e);
}
function Go(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Ge);
  {
    const i = Ls(e);
    Xe();
    try {
      Gc(e);
    } finally {
      ze(), i();
    }
  }
}
const $a = {
  get(e, t) {
    return he(e, "get", ""), e[t];
  }
};
function Ma(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, $a),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function _n(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(po(mc(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in ys)
        return ys[s](e);
    },
    has(t, s) {
      return s in t || s in ys;
    }
  })) : e.proxy;
}
function Pa(e) {
  return U(e) && "__vccOpts" in e;
}
const Da = (e, t) => /* @__PURE__ */ vc(e, t, Ts), Ra = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let ci;
const br = typeof window < "u" && window.trustedTypes;
if (br)
  try {
    ci = /* @__PURE__ */ br.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Qo = ci ? (e) => ci.createHTML(e) : (e) => e, ja = "http://www.w3.org/2000/svg", Ba = "http://www.w3.org/1998/Math/MathML", tt = typeof document < "u" ? document : null, wr = tt && /* @__PURE__ */ tt.createElement("template"), Fa = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const i = t === "svg" ? tt.createElementNS(ja, e) : t === "mathml" ? tt.createElementNS(Ba, e) : s ? tt.createElement(e, { is: s }) : tt.createElement(e);
    return e === "select" && n && n.multiple != null && i.setAttribute("multiple", n.multiple), i;
  },
  createText: (e) => tt.createTextNode(e),
  createComment: (e) => tt.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => tt.querySelector(e),
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
      wr.innerHTML = Qo(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = wr.content;
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
}, Ka = /* @__PURE__ */ Symbol("_vtc");
function xa(e, t, s) {
  const n = e[Ka];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const Sr = /* @__PURE__ */ Symbol("_vod"), Ua = /* @__PURE__ */ Symbol("_vsh"), Va = /* @__PURE__ */ Symbol(""), qa = /(?:^|;)\s*display\s*:/;
function Ha(e, t, s) {
  const n = e.style, i = se(s);
  let r = !1;
  if (s && !i) {
    if (t)
      if (se(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && as(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && as(n, o, "");
    for (const o in s) {
      o === "display" && (r = !0);
      const l = s[o];
      l != null ? Ja(
        e,
        o,
        !se(t) && t ? t[o] : void 0,
        l
      ) || as(n, o, l) : as(n, o, "");
    }
  } else if (i) {
    if (t !== s) {
      const o = n[Va];
      o && (s += ";" + o), n.cssText = s, r = qa.test(s);
    }
  } else t && e.removeAttribute("style");
  Sr in e && (e[Sr] = r ? n.display : "", e[Ua] && (n.display = "none"));
}
const _r = /\s*!important$/;
function as(e, t, s) {
  if (B(s))
    s.forEach((n) => as(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = Wa(e, t);
    _r.test(s) ? e.setProperty(
      At(n),
      s.replace(_r, ""),
      "important"
    ) : e[n] = s;
  }
}
const vr = ["Webkit", "Moz", "ms"], Vn = {};
function Wa(e, t) {
  const s = Vn[t];
  if (s)
    return s;
  let n = Pe(t);
  if (n !== "filter" && n in e)
    return Vn[t] = n;
  n = Yr(n);
  for (let i = 0; i < vr.length; i++) {
    const r = vr[i] + n;
    if (r in e)
      return Vn[t] = r;
  }
  return t;
}
function Ja(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && se(n) && s === n;
}
const kr = "http://www.w3.org/1999/xlink";
function Or(e, t, s, n, i, r = Yl(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(kr, t.slice(6, t.length)) : e.setAttributeNS(kr, t, s) : s == null || r && !Qr(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : Qe(s) ? String(s) : s
  );
}
function Tr(e, t, s, n, i) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? Qo(s) : s);
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
    l === "boolean" ? s = Qr(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(i || t);
}
function dt(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function Ya(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const Nr = /* @__PURE__ */ Symbol("_vei");
function Ga(e, t, s, n, i = null) {
  const r = e[Nr] || (e[Nr] = {}), o = r[t];
  if (n && o)
    o.value = n;
  else {
    const [l, c] = za(t);
    if (n) {
      const a = r[t] = tf(
        n,
        i
      );
      dt(e, l, a, c);
    } else o && (Ya(e, l, o, c), r[t] = void 0);
  }
}
const Qa = /(Once|Passive|Capture)$/, Xa = /^on:?(?:Once|Passive|Capture)$/;
function za(e) {
  let t, s;
  for (; (s = e.match(Qa)) && !Xa.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : At(e.slice(2)), t];
}
let qn = 0;
const Za = /* @__PURE__ */ Promise.resolve(), ef = () => qn || (Za.then(() => qn = 0), qn = Date.now());
function tf(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const i = s.value;
    if (B(i)) {
      const r = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        r.call(n), n._stopped = !0;
      };
      const o = i.slice(), l = [n];
      for (let c = 0; c < o.length && !n._stopped; c++) {
        const a = o[c];
        a && je(
          a,
          t,
          5,
          l
        );
      }
    } else
      je(
        i,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = ef(), s;
}
const Ar = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, sf = (e, t, s, n, i, r) => {
  const o = i === "svg";
  t === "class" ? xa(e, n, o) : t === "style" ? Ha(e, s, n) : fn(t) ? un(t) || Ga(e, t, s, n, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : nf(e, t, n, o)) ? (Tr(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Or(e, t, n, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (rf(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !se(n))) ? Tr(e, Pe(t), n, r, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), Or(e, t, n, o));
};
function nf(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Ar(t) && U(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return Ar(t) && se(s) ? !1 : t in e;
}
function rf(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = Pe(t);
  return Array.isArray(s) ? s.some((i) => Pe(i) === n) : Object.keys(s).some((i) => Pe(i) === n);
}
const Ht = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return B(t) ? (s) => Js(t, s) : t;
};
function of(e) {
  e.target.composing = !0;
}
function Er(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const lt = /* @__PURE__ */ Symbol("_assign");
function Cr(e, t, s) {
  return t && (e = e.trim()), s && (e = dn(e)), e;
}
const qe = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, i) {
    e[lt] = Ht(i);
    const r = n || i.props && i.props.type === "number";
    dt(e, t ? "change" : "input", (o) => {
      o.target.composing || e[lt](Cr(e.value, s, r));
    }), (s || r) && dt(e, "change", () => {
      e.value = Cr(e.value, s, r);
    }), t || (dt(e, "compositionstart", of), dt(e, "compositionend", Er), dt(e, "change", Er));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: i, number: r } }, o) {
    if (e[lt] = Ht(o), e.composing) return;
    const l = (r || e.type === "number") && !/^0\d/.test(e.value) ? dn(e.value) : e.value, c = t ?? "";
    if (l === c)
      return;
    const a = e.getRootNode();
    (a instanceof Document || a instanceof ShadowRoot) && a.activeElement === e && e.type !== "range" && (n && t === s || i && e.value.trim() === c) || (e.value = c);
  }
}, lf = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, s) {
    e[lt] = Ht(s), dt(e, "change", () => {
      const n = e._modelValue, i = Ns(e), r = e.checked, o = e[lt];
      if (B(n)) {
        const l = Si(n, i), c = l !== -1;
        if (r && !c)
          o(n.concat(i));
        else if (!r && c) {
          const a = [...n];
          a.splice(l, 1), o(a);
        }
      } else if (Yt(n)) {
        const l = new Set(n);
        r ? l.add(i) : l.delete(i), o(l);
      } else
        o(Xo(e, r));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: Ir,
  beforeUpdate(e, t, s) {
    e[lt] = Ht(s), Ir(e, t, s);
  }
};
function Ir(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let i;
  if (B(t))
    i = Si(t, n.props.value) > -1;
  else if (Yt(t))
    i = t.has(n.props.value);
  else {
    if (t === s) return;
    i = Gt(t, Xo(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const Hn = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const i = Yt(t);
    dt(e, "change", () => {
      const r = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? dn(Ns(o)) : Ns(o)
      );
      e[lt](
        e.multiple ? i ? new Set(r) : r : r[0]
      ), e._assigning = !0, mo(() => {
        e._assigning = !1;
      });
    }), e[lt] = Ht(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    Lr(e, t);
  },
  beforeUpdate(e, t, s) {
    e[lt] = Ht(s);
  },
  updated(e, { value: t }) {
    e._assigning || Lr(e, t);
  }
};
function Lr(e, t) {
  const s = e.multiple, n = B(t);
  if (!(s && !n && !Yt(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = Ns(o);
      if (s)
        if (n) {
          const c = typeof l;
          c === "string" || c === "number" ? o.selected = t.some((a) => String(a) === String(l)) : o.selected = Si(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (Gt(Ns(o), t)) {
        e.selectedIndex !== i && (e.selectedIndex = i);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function Ns(e) {
  return "_value" in e ? e._value : e.value;
}
function Xo(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
}
const cf = /* @__PURE__ */ ae({ patchProp: sf }, Fa);
let $r;
function af() {
  return $r || ($r = ga(cf));
}
const ff = ((...e) => {
  const t = af().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const i = hf(n);
    if (!i) return;
    const r = t._component;
    !U(r) && !r.render && !r.template && (r.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
    const o = s(i, !1, uf(i));
    return i instanceof Element && (i.removeAttribute("v-cloak"), i.setAttribute("data-v-app", "")), o;
  }, t;
});
function uf(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function hf(e) {
  return se(e) ? document.querySelector(e) : e;
}
const $i = Symbol.for("yaml.alias"), ai = Symbol.for("yaml.document"), pt = Symbol.for("yaml.map"), zo = Symbol.for("yaml.pair"), Ze = Symbol.for("yaml.scalar"), Qt = Symbol.for("yaml.seq"), $e = Symbol.for("yaml.node.type"), Xt = (e) => !!e && typeof e == "object" && e[$e] === $i, $s = (e) => !!e && typeof e == "object" && e[$e] === ai, Ms = (e) => !!e && typeof e == "object" && e[$e] === pt, re = (e) => !!e && typeof e == "object" && e[$e] === zo, Z = (e) => !!e && typeof e == "object" && e[$e] === Ze, Ps = (e) => !!e && typeof e == "object" && e[$e] === Qt;
function ne(e) {
  if (e && typeof e == "object")
    switch (e[$e]) {
      case pt:
      case Qt:
        return !0;
    }
  return !1;
}
function ie(e) {
  if (e && typeof e == "object")
    switch (e[$e]) {
      case $i:
      case pt:
      case Ze:
      case Qt:
        return !0;
    }
  return !1;
}
const Zo = (e) => (Z(e) || ne(e)) && !!e.anchor, _t = Symbol("break visit"), df = Symbol("skip children"), ws = Symbol("remove node");
function zt(e, t) {
  const s = pf(t);
  $s(e) ? Pt(null, e.contents, s, Object.freeze([e])) === ws && (e.contents = null) : Pt(null, e, s, Object.freeze([]));
}
zt.BREAK = _t;
zt.SKIP = df;
zt.REMOVE = ws;
function Pt(e, t, s, n) {
  const i = gf(e, t, s, n);
  if (ie(i) || re(i))
    return mf(e, n, i), Pt(e, i, s, n);
  if (typeof i != "symbol") {
    if (ne(t)) {
      n = Object.freeze(n.concat(t));
      for (let r = 0; r < t.items.length; ++r) {
        const o = Pt(r, t.items[r], s, n);
        if (typeof o == "number")
          r = o - 1;
        else {
          if (o === _t)
            return _t;
          o === ws && (t.items.splice(r, 1), r -= 1);
        }
      }
    } else if (re(t)) {
      n = Object.freeze(n.concat(t));
      const r = Pt("key", t.key, s, n);
      if (r === _t)
        return _t;
      r === ws && (t.key = null);
      const o = Pt("value", t.value, s, n);
      if (o === _t)
        return _t;
      o === ws && (t.value = null);
    }
  }
  return i;
}
function pf(e) {
  return typeof e == "object" && (e.Collection || e.Node || e.Value) ? Object.assign({
    Alias: e.Node,
    Map: e.Node,
    Scalar: e.Node,
    Seq: e.Node
  }, e.Value && {
    Map: e.Value,
    Scalar: e.Value,
    Seq: e.Value
  }, e.Collection && {
    Map: e.Collection,
    Seq: e.Collection
  }, e) : e;
}
function gf(e, t, s, n) {
  var i, r, o, l, c;
  if (typeof s == "function")
    return s(e, t, n);
  if (Ms(t))
    return (i = s.Map) == null ? void 0 : i.call(s, e, t, n);
  if (Ps(t))
    return (r = s.Seq) == null ? void 0 : r.call(s, e, t, n);
  if (re(t))
    return (o = s.Pair) == null ? void 0 : o.call(s, e, t, n);
  if (Z(t))
    return (l = s.Scalar) == null ? void 0 : l.call(s, e, t, n);
  if (Xt(t))
    return (c = s.Alias) == null ? void 0 : c.call(s, e, t, n);
}
function mf(e, t, s) {
  const n = t[t.length - 1];
  if (ne(n))
    n.items[e] = s;
  else if (re(n))
    e === "key" ? n.key = s : n.value = s;
  else if ($s(n))
    n.contents = s;
  else {
    const i = Xt(n) ? "alias" : "scalar";
    throw new Error(`Cannot replace node with ${i} parent`);
  }
}
const yf = {
  "!": "%21",
  ",": "%2C",
  "[": "%5B",
  "]": "%5D",
  "{": "%7B",
  "}": "%7D"
}, bf = (e) => e.replace(/[!,[\]{}]/g, (t) => yf[t]);
class be {
  constructor(t, s) {
    this.docStart = null, this.docEnd = !1, this.yaml = Object.assign({}, be.defaultYaml, t), this.tags = Object.assign({}, be.defaultTags, s);
  }
  clone() {
    const t = new be(this.yaml, this.tags);
    return t.docStart = this.docStart, t;
  }
  /**
   * During parsing, get a Directives instance for the current document and
   * update the stream state according to the current version's spec.
   */
  atDocument() {
    const t = new be(this.yaml, this.tags);
    switch (this.yaml.version) {
      case "1.1":
        this.atNextDocument = !0;
        break;
      case "1.2":
        this.atNextDocument = !1, this.yaml = {
          explicit: be.defaultYaml.explicit,
          version: "1.2"
        }, this.tags = Object.assign({}, be.defaultTags);
        break;
    }
    return t;
  }
  /**
   * @param onError - May be called even if the action was successful
   * @returns `true` on success
   */
  add(t, s) {
    this.atNextDocument && (this.yaml = { explicit: be.defaultYaml.explicit, version: "1.1" }, this.tags = Object.assign({}, be.defaultTags), this.atNextDocument = !1);
    const n = t.trim().split(/[ \t]+/), i = n.shift();
    switch (i) {
      case "%TAG": {
        if (n.length !== 2 && (s(0, "%TAG directive should contain exactly two parts"), n.length < 2))
          return !1;
        const [r, o] = n;
        return this.tags[r] = o, !0;
      }
      case "%YAML": {
        if (this.yaml.explicit = !0, n.length !== 1)
          return s(0, "%YAML directive should contain exactly one part"), !1;
        const [r] = n;
        if (r === "1.1" || r === "1.2")
          return this.yaml.version = r, !0;
        {
          const o = /^\d+\.\d+$/.test(r);
          return s(6, `Unsupported YAML version ${r}`, o), !1;
        }
      }
      default:
        return s(0, `Unknown directive ${i}`, !0), !1;
    }
  }
  /**
   * Resolves a tag, matching handles to those defined in %TAG directives.
   *
   * @returns Resolved tag, which may also be the non-specific tag `'!'` or a
   *   `'!local'` tag, or `null` if unresolvable.
   */
  tagName(t, s) {
    if (t === "!")
      return "!";
    if (t[0] !== "!")
      return s(`Not a valid tag: ${t}`), null;
    if (t[1] === "<") {
      const o = t.slice(2, -1);
      return o === "!" || o === "!!" ? (s(`Verbatim tags aren't resolved, so ${t} is invalid.`), null) : (t[t.length - 1] !== ">" && s("Verbatim tags must end with a >"), o);
    }
    const [, n, i] = t.match(/^(.*!)([^!]*)$/s);
    i || s(`The ${t} tag has no suffix`);
    const r = this.tags[n];
    if (r)
      try {
        return r + decodeURIComponent(i);
      } catch (o) {
        return s(String(o)), null;
      }
    return n === "!" ? t : (s(`Could not resolve tag: ${t}`), null);
  }
  /**
   * Given a fully resolved tag, returns its printable string form,
   * taking into account current tag prefixes and defaults.
   */
  tagString(t) {
    for (const [s, n] of Object.entries(this.tags))
      if (t.startsWith(n))
        return s + bf(t.substring(n.length));
    return t[0] === "!" ? t : `!<${t}>`;
  }
  toString(t) {
    const s = this.yaml.explicit ? [`%YAML ${this.yaml.version || "1.2"}`] : [], n = Object.entries(this.tags);
    let i;
    if (t && n.length > 0 && ie(t.contents)) {
      const r = {};
      zt(t.contents, (o, l) => {
        ie(l) && l.tag && (r[l.tag] = !0);
      }), i = Object.keys(r);
    } else
      i = [];
    for (const [r, o] of n)
      r === "!!" && o === "tag:yaml.org,2002:" || (!t || i.some((l) => l.startsWith(o))) && s.push(`%TAG ${r} ${o}`);
    return s.join(`
`);
  }
}
be.defaultYaml = { explicit: !1, version: "1.2" };
be.defaultTags = { "!!": "tag:yaml.org,2002:" };
function el(e) {
  if (/[\x00-\x19\s,[\]{}]/.test(e)) {
    const s = `Anchor must not contain whitespace or control characters: ${JSON.stringify(e)}`;
    throw new Error(s);
  }
  return !0;
}
function tl(e) {
  const t = /* @__PURE__ */ new Set();
  return zt(e, {
    Value(s, n) {
      n.anchor && t.add(n.anchor);
    }
  }), t;
}
function sl(e, t) {
  for (let s = 1; ; ++s) {
    const n = `${e}${s}`;
    if (!t.has(n))
      return n;
  }
}
function wf(e, t) {
  const s = [], n = /* @__PURE__ */ new Map();
  let i = null;
  return {
    onAnchor: (r) => {
      s.push(r), i ?? (i = tl(e));
      const o = sl(t, i);
      return i.add(o), o;
    },
    /**
     * With circular references, the source node is only resolved after all
     * of its child nodes are. This is why anchors are set only after all of
     * the nodes have been created.
     */
    setAnchors: () => {
      for (const r of s) {
        const o = n.get(r);
        if (typeof o == "object" && o.anchor && (Z(o.node) || ne(o.node)))
          o.node.anchor = o.anchor;
        else {
          const l = new Error("Failed to resolve repeated object (this should not happen)");
          throw l.source = r, l;
        }
      }
    },
    sourceObjects: n
  };
}
function Dt(e, t, s, n) {
  if (n && typeof n == "object")
    if (Array.isArray(n))
      for (let i = 0, r = n.length; i < r; ++i) {
        const o = n[i], l = Dt(e, n, String(i), o);
        l === void 0 ? delete n[i] : l !== o && (n[i] = l);
      }
    else if (n instanceof Map)
      for (const i of Array.from(n.keys())) {
        const r = n.get(i), o = Dt(e, n, i, r);
        o === void 0 ? n.delete(i) : o !== r && n.set(i, o);
      }
    else if (n instanceof Set)
      for (const i of Array.from(n)) {
        const r = Dt(e, n, i, i);
        r === void 0 ? n.delete(i) : r !== i && (n.delete(i), n.add(r));
      }
    else
      for (const [i, r] of Object.entries(n)) {
        const o = Dt(e, n, i, r);
        o === void 0 ? delete n[i] : o !== r && (n[i] = o);
      }
  return e.call(t, s, n);
}
function Le(e, t, s) {
  if (Array.isArray(e))
    return e.map((n, i) => Le(n, String(i), s));
  if (e && typeof e.toJSON == "function") {
    if (!s || !Zo(e))
      return e.toJSON(t, s);
    const n = { aliasCount: 0, count: 1, res: void 0 };
    s.anchors.set(e, n), s.onCreate = (r) => {
      n.res = r, delete s.onCreate;
    };
    const i = e.toJSON(t, s);
    return s.onCreate && s.onCreate(i), i;
  }
  return typeof e == "bigint" && !(s != null && s.keep) ? Number(e) : e;
}
class Mi {
  constructor(t) {
    Object.defineProperty(this, $e, { value: t });
  }
  /** Create a copy of this node.  */
  clone() {
    const t = Object.create(Object.getPrototypeOf(this), Object.getOwnPropertyDescriptors(this));
    return this.range && (t.range = this.range.slice()), t;
  }
  /** A plain JavaScript representation of this node. */
  toJS(t, { mapAsMap: s, maxAliasCount: n, onAnchor: i, reviver: r } = {}) {
    if (!$s(t))
      throw new TypeError("A document argument is required");
    const o = {
      anchors: /* @__PURE__ */ new Map(),
      doc: t,
      keep: !0,
      mapAsMap: s === !0,
      mapKeyWarned: !1,
      maxAliasCount: typeof n == "number" ? n : 100
    }, l = Le(this, "", o);
    if (typeof i == "function")
      for (const { count: c, res: a } of o.anchors.values())
        i(a, c);
    return typeof r == "function" ? Dt(r, { "": l }, "", l) : l;
  }
}
class Pi extends Mi {
  constructor(t) {
    super($i), this.source = t, Object.defineProperty(this, "tag", {
      set() {
        throw new Error("Alias nodes cannot have tags");
      }
    });
  }
  /**
   * Resolve the value of this alias within `doc`, finding the last
   * instance of the `source` anchor before this node.
   */
  resolve(t, s) {
    if ((s == null ? void 0 : s.maxAliasCount) === 0)
      throw new ReferenceError("Alias resolution is disabled");
    let n;
    s != null && s.aliasResolveCache ? n = s.aliasResolveCache : (n = [], zt(t, {
      Node: (r, o) => {
        (Xt(o) || Zo(o)) && n.push(o);
      }
    }), s && (s.aliasResolveCache = n));
    let i;
    for (const r of n) {
      if (r === this)
        break;
      r.anchor === this.source && (i = r);
    }
    return i;
  }
  toJSON(t, s) {
    if (!s)
      return { source: this.source };
    const { anchors: n, doc: i, maxAliasCount: r } = s, o = this.resolve(i, s);
    if (!o) {
      const c = `Unresolved alias (the anchor must be set before the alias): ${this.source}`;
      throw new ReferenceError(c);
    }
    let l = n.get(o);
    if (l || (Le(o, null, s), l = n.get(o)), (l == null ? void 0 : l.res) === void 0) {
      const c = "This should not happen: Alias anchor was not resolved?";
      throw new ReferenceError(c);
    }
    if (r >= 0 && (l.count += 1, l.aliasCount === 0 && (l.aliasCount = Qs(i, o, n)), l.count * l.aliasCount > r)) {
      const c = "Excessive alias count indicates a resource exhaustion attack";
      throw new ReferenceError(c);
    }
    return l.res;
  }
  toString(t, s, n) {
    const i = `*${this.source}`;
    if (t) {
      if (el(this.source), t.options.verifyAliasOrder && !t.anchors.has(this.source)) {
        const r = `Unresolved alias (the anchor must be set before the alias): ${this.source}`;
        throw new Error(r);
      }
      if (t.implicitKey)
        return `${i} `;
    }
    return i;
  }
}
function Qs(e, t, s) {
  if (Xt(t)) {
    const n = t.resolve(e), i = s && n && s.get(n);
    return i ? i.count * i.aliasCount : 0;
  } else if (ne(t)) {
    let n = 0;
    for (const i of t.items) {
      const r = Qs(e, i, s);
      r > n && (n = r);
    }
    return n;
  } else if (re(t)) {
    const n = Qs(e, t.key, s), i = Qs(e, t.value, s);
    return Math.max(n, i);
  }
  return 1;
}
const nl = (e) => !e || typeof e != "function" && typeof e != "object";
class K extends Mi {
  constructor(t) {
    super(Ze), this.value = t;
  }
  toJSON(t, s) {
    return s != null && s.keep ? this.value : Le(this.value, t, s);
  }
  toString() {
    return String(this.value);
  }
}
K.BLOCK_FOLDED = "BLOCK_FOLDED";
K.BLOCK_LITERAL = "BLOCK_LITERAL";
K.PLAIN = "PLAIN";
K.QUOTE_DOUBLE = "QUOTE_DOUBLE";
K.QUOTE_SINGLE = "QUOTE_SINGLE";
const Sf = "tag:yaml.org,2002:";
function _f(e, t, s) {
  if (t) {
    const n = s.filter((r) => r.tag === t), i = n.find((r) => !r.format) ?? n[0];
    if (!i)
      throw new Error(`Tag ${t} not found`);
    return i;
  }
  return s.find((n) => {
    var i;
    return ((i = n.identify) == null ? void 0 : i.call(n, e)) && !n.format;
  });
}
function As(e, t, s) {
  var u, g, p;
  if ($s(e) && (e = e.contents), ie(e))
    return e;
  if (re(e)) {
    const k = (g = (u = s.schema[pt]).createNode) == null ? void 0 : g.call(u, s.schema, null, s);
    return k.items.push(e), k;
  }
  (e instanceof String || e instanceof Number || e instanceof Boolean || typeof BigInt < "u" && e instanceof BigInt) && (e = e.valueOf());
  const { aliasDuplicateObjects: n, onAnchor: i, onTagObj: r, schema: o, sourceObjects: l } = s;
  let c;
  if (n && e && typeof e == "object") {
    if (c = l.get(e), c)
      return c.anchor ?? (c.anchor = i(e)), new Pi(c.anchor);
    c = { anchor: null, node: null }, l.set(e, c);
  }
  t != null && t.startsWith("!!") && (t = Sf + t.slice(2));
  let a = _f(e, t, o.tags);
  if (!a) {
    if (e && typeof e.toJSON == "function" && (e = e.toJSON()), !e || typeof e != "object") {
      const k = new K(e);
      return c && (c.node = k), k;
    }
    a = e instanceof Map ? o[pt] : Symbol.iterator in Object(e) ? o[Qt] : o[pt];
  }
  r && (r(a), delete s.onTagObj);
  const f = a != null && a.createNode ? a.createNode(s.schema, e, s) : typeof ((p = a == null ? void 0 : a.nodeClass) == null ? void 0 : p.from) == "function" ? a.nodeClass.from(s.schema, e, s) : new K(e);
  return t ? f.tag = t : a.default || (f.tag = a.tag), c && (c.node = f), f;
}
function ln(e, t, s) {
  let n = s;
  for (let i = t.length - 1; i >= 0; --i) {
    const r = t[i];
    if (typeof r == "number" && Number.isInteger(r) && r >= 0) {
      const o = [];
      o[r] = n, n = o;
    } else
      n = /* @__PURE__ */ new Map([[r, n]]);
  }
  return As(n, void 0, {
    aliasDuplicateObjects: !1,
    keepUndefined: !1,
    onAnchor: () => {
      throw new Error("This should not happen, please report a bug.");
    },
    schema: e,
    sourceObjects: /* @__PURE__ */ new Map()
  });
}
const fs = (e) => e == null || typeof e == "object" && !!e[Symbol.iterator]().next().done;
class il extends Mi {
  constructor(t, s) {
    super(t), Object.defineProperty(this, "schema", {
      value: s,
      configurable: !0,
      enumerable: !1,
      writable: !0
    });
  }
  /**
   * Create a copy of this collection.
   *
   * @param schema - If defined, overwrites the original's schema
   */
  clone(t) {
    const s = Object.create(Object.getPrototypeOf(this), Object.getOwnPropertyDescriptors(this));
    return t && (s.schema = t), s.items = s.items.map((n) => ie(n) || re(n) ? n.clone(t) : n), this.range && (s.range = this.range.slice()), s;
  }
  /**
   * Adds a value to the collection. For `!!map` and `!!omap` the value must
   * be a Pair instance or a `{ key, value }` object, which may not have a key
   * that already exists in the map.
   */
  addIn(t, s) {
    if (fs(t))
      this.add(s);
    else {
      const [n, ...i] = t, r = this.get(n, !0);
      if (ne(r))
        r.addIn(i, s);
      else if (r === void 0 && this.schema)
        this.set(n, ln(this.schema, i, s));
      else
        throw new Error(`Expected YAML collection at ${n}. Remaining path: ${i}`);
    }
  }
  /**
   * Removes a value from the collection.
   * @returns `true` if the item was found and removed.
   */
  deleteIn(t) {
    const [s, ...n] = t;
    if (n.length === 0)
      return this.delete(s);
    const i = this.get(s, !0);
    if (ne(i))
      return i.deleteIn(n);
    throw new Error(`Expected YAML collection at ${s}. Remaining path: ${n}`);
  }
  /**
   * Returns item at `key`, or `undefined` if not found. By default unwraps
   * scalar values from their surrounding node; to disable set `keepScalar` to
   * `true` (collections are always returned intact).
   */
  getIn(t, s) {
    const [n, ...i] = t, r = this.get(n, !0);
    return i.length === 0 ? !s && Z(r) ? r.value : r : ne(r) ? r.getIn(i, s) : void 0;
  }
  hasAllNullValues(t) {
    return this.items.every((s) => {
      if (!re(s))
        return !1;
      const n = s.value;
      return n == null || t && Z(n) && n.value == null && !n.commentBefore && !n.comment && !n.tag;
    });
  }
  /**
   * Checks if the collection includes a value with the key `key`.
   */
  hasIn(t) {
    const [s, ...n] = t;
    if (n.length === 0)
      return this.has(s);
    const i = this.get(s, !0);
    return ne(i) ? i.hasIn(n) : !1;
  }
  /**
   * Sets a value in this collection. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   */
  setIn(t, s) {
    const [n, ...i] = t;
    if (i.length === 0)
      this.set(n, s);
    else {
      const r = this.get(n, !0);
      if (ne(r))
        r.setIn(i, s);
      else if (r === void 0 && this.schema)
        this.set(n, ln(this.schema, i, s));
      else
        throw new Error(`Expected YAML collection at ${n}. Remaining path: ${i}`);
    }
  }
}
const vf = (e) => e.replace(/^(?!$)(?: $)?/gm, "#");
function rt(e, t) {
  return /^\n+$/.test(e) ? e.substring(1) : t ? e.replace(/^(?! *$)/gm, t) : e;
}
const vt = (e, t, s) => e.endsWith(`
`) ? rt(s, t) : s.includes(`
`) ? `
` + rt(s, t) : (e.endsWith(" ") ? "" : " ") + s, rl = "flow", fi = "block", Xs = "quoted";
function vn(e, t, s = "flow", { indentAtStart: n, lineWidth: i = 80, minContentWidth: r = 20, onFold: o, onOverflow: l } = {}) {
  if (!i || i < 0)
    return e;
  i < r && (r = 0);
  const c = Math.max(1 + r, 1 + i - t.length);
  if (e.length <= c)
    return e;
  const a = [], f = {};
  let u = i - t.length;
  typeof n == "number" && (n > i - Math.max(2, r) ? a.push(0) : u = i - n);
  let g, p, k = !1, m = -1, _ = -1, b = -1;
  s === fi && (m = Mr(e, m, t.length), m !== -1 && (u = m + c));
  for (let S; S = e[m += 1]; ) {
    if (s === Xs && S === "\\") {
      switch (_ = m, e[m + 1]) {
        case "x":
          m += 3;
          break;
        case "u":
          m += 5;
          break;
        case "U":
          m += 9;
          break;
        default:
          m += 1;
      }
      b = m;
    }
    if (S === `
`)
      s === fi && (m = Mr(e, m, t.length)), u = m + t.length + c, g = void 0;
    else {
      if (S === " " && p && p !== " " && p !== `
` && p !== "	") {
        const A = e[m + 1];
        A && A !== " " && A !== `
` && A !== "	" && (g = m);
      }
      if (m >= u)
        if (g)
          a.push(g), u = g + c, g = void 0;
        else if (s === Xs) {
          for (; p === " " || p === "	"; )
            p = S, S = e[m += 1], k = !0;
          const A = m > b + 1 ? m - 2 : _ - 1;
          if (f[A])
            return e;
          a.push(A), f[A] = !0, u = A + c, g = void 0;
        } else
          k = !0;
    }
    p = S;
  }
  if (k && l && l(), a.length === 0)
    return e;
  o && o();
  let y = e.slice(0, a[0]);
  for (let S = 0; S < a.length; ++S) {
    const A = a[S], N = a[S + 1] || e.length;
    A === 0 ? y = `
${t}${e.slice(0, N)}` : (s === Xs && f[A] && (y += `${e[A]}\\`), y += `
${t}${e.slice(A + 1, N)}`);
  }
  return y;
}
function Mr(e, t, s) {
  let n = t, i = t + 1, r = e[i];
  for (; r === " " || r === "	"; )
    if (t < i + s)
      r = e[++t];
    else {
      do
        r = e[++t];
      while (r && r !== `
`);
      n = t, i = t + 1, r = e[i];
    }
  return n;
}
const kn = (e, t) => ({
  indentAtStart: t ? e.indent.length : e.indentAtStart,
  lineWidth: e.options.lineWidth,
  minContentWidth: e.options.minContentWidth
}), On = (e) => /^(%|---|\.\.\.)/m.test(e);
function kf(e, t, s) {
  if (!t || t < 0)
    return !1;
  const n = t - s, i = e.length;
  if (i <= n)
    return !1;
  for (let r = 0, o = 0; r < i; ++r)
    if (e[r] === `
`) {
      if (r - o > n)
        return !0;
      if (o = r + 1, i - o <= n)
        return !1;
    }
  return !0;
}
function Ss(e, t) {
  const s = JSON.stringify(e);
  if (t.options.doubleQuotedAsJSON)
    return s;
  const { implicitKey: n } = t, i = t.options.doubleQuotedMinMultiLineLength, r = t.indent || (On(e) ? "  " : "");
  let o = "", l = 0;
  for (let c = 0, a = s[c]; a; a = s[++c])
    if (a === " " && s[c + 1] === "\\" && s[c + 2] === "n" && (o += s.slice(l, c) + "\\ ", c += 1, l = c, a = "\\"), a === "\\")
      switch (s[c + 1]) {
        case "u":
          {
            o += s.slice(l, c);
            const f = s.substr(c + 2, 4);
            switch (f) {
              case "0000":
                o += "\\0";
                break;
              case "0007":
                o += "\\a";
                break;
              case "000b":
                o += "\\v";
                break;
              case "001b":
                o += "\\e";
                break;
              case "0085":
                o += "\\N";
                break;
              case "00a0":
                o += "\\_";
                break;
              case "2028":
                o += "\\L";
                break;
              case "2029":
                o += "\\P";
                break;
              default:
                f.substr(0, 2) === "00" ? o += "\\x" + f.substr(2) : o += s.substr(c, 6);
            }
            c += 5, l = c + 1;
          }
          break;
        case "n":
          if (n || s[c + 2] === '"' || s.length < i)
            c += 1;
          else {
            for (o += s.slice(l, c) + `

`; s[c + 2] === "\\" && s[c + 3] === "n" && s[c + 4] !== '"'; )
              o += `
`, c += 2;
            o += r, s[c + 2] === " " && (o += "\\"), c += 1, l = c + 1;
          }
          break;
        default:
          c += 1;
      }
  return o = l ? o + s.slice(l) : s, n ? o : vn(o, r, Xs, kn(t, !1));
}
function ui(e, t) {
  if (t.options.singleQuote === !1 || t.implicitKey && e.includes(`
`) || /[ \t]\n|\n[ \t]/.test(e))
    return Ss(e, t);
  const s = t.indent || (On(e) ? "  " : ""), n = "'" + e.replace(/'/g, "''").replace(/\n+/g, `$&
${s}`) + "'";
  return t.implicitKey ? n : vn(n, s, rl, kn(t, !1));
}
function Rt(e, t) {
  const { singleQuote: s } = t.options;
  let n;
  if (s === !1)
    n = Ss;
  else {
    const i = e.includes('"'), r = e.includes("'");
    i && !r ? n = ui : r && !i ? n = Ss : n = s ? ui : Ss;
  }
  return n(e, t);
}
let hi;
try {
  hi = new RegExp(`(^|(?<!
))
+(?!
|$)`, "g");
} catch {
  hi = /\n+(?!\n|$)/g;
}
function zs({ comment: e, type: t, value: s }, n, i, r) {
  const { blockQuote: o, commentString: l, lineWidth: c } = n.options;
  if (!o || /\n[\t ]+$/.test(s))
    return Rt(s, n);
  const a = n.indent || (n.forceBlockIndent || On(s) ? "  " : ""), f = o === "literal" ? !0 : o === "folded" || t === K.BLOCK_FOLDED ? !1 : t === K.BLOCK_LITERAL ? !0 : !kf(s, c, a.length);
  if (!s)
    return f ? `|
` : `>
`;
  let u, g;
  for (g = s.length; g > 0; --g) {
    const N = s[g - 1];
    if (N !== `
` && N !== "	" && N !== " ")
      break;
  }
  let p = s.substring(g);
  const k = p.indexOf(`
`);
  k === -1 ? u = "-" : s === p || k !== p.length - 1 ? (u = "+", r && r()) : u = "", p && (s = s.slice(0, -p.length), p[p.length - 1] === `
` && (p = p.slice(0, -1)), p = p.replace(hi, `$&${a}`));
  let m = !1, _, b = -1;
  for (_ = 0; _ < s.length; ++_) {
    const N = s[_];
    if (N === " ")
      m = !0;
    else if (N === `
`)
      b = _;
    else
      break;
  }
  let y = s.substring(0, b < _ ? b + 1 : _);
  y && (s = s.substring(y.length), y = y.replace(/\n+/g, `$&${a}`));
  let A = (m ? a ? "2" : "1" : "") + u;
  if (e && (A += " " + l(e.replace(/ ?[\r\n]+/g, " ")), i && i()), !f) {
    const N = s.replace(/\n+/g, `
$&`).replace(/(?:^|\n)([\t ].*)(?:([\n\t ]*)\n(?![\n\t ]))?/g, "$1$2").replace(/\n+/g, `$&${a}`);
    let x = !1;
    const D = kn(n, !0);
    o !== "folded" && t !== K.BLOCK_FOLDED && (D.onOverflow = () => {
      x = !0;
    });
    const M = vn(`${y}${N}${p}`, a, fi, D);
    if (!x)
      return `>${A}
${a}${M}`;
  }
  return s = s.replace(/\n+/g, `$&${a}`), `|${A}
${a}${y}${s}${p}`;
}
function Of(e, t, s, n) {
  const { type: i, value: r } = e, { actualString: o, implicitKey: l, indent: c, indentStep: a, inFlow: f } = t;
  if (l && r.includes(`
`) || f && /[[\]{},]/.test(r))
    return Rt(r, t);
  if (/^[\n\t ,[\]{}#&*!|>'"%@`]|^[?-]$|^[?-][ \t]|[\n:][ \t]|[ \t]\n|[\n\t ]#|[\n\t :]$/.test(r))
    return l || f || !r.includes(`
`) ? Rt(r, t) : zs(e, t, s, n);
  if (!l && !f && i !== K.PLAIN && r.includes(`
`))
    return zs(e, t, s, n);
  if (On(r)) {
    if (c === "")
      return t.forceBlockIndent = !0, zs(e, t, s, n);
    if (l && c === a)
      return Rt(r, t);
  }
  const u = r.replace(/\n+/g, `$&
${c}`);
  if (o) {
    const g = (m) => {
      var _;
      return m.default && m.tag !== "tag:yaml.org,2002:str" && ((_ = m.test) == null ? void 0 : _.test(u));
    }, { compat: p, tags: k } = t.doc.schema;
    if (k.some(g) || p != null && p.some(g))
      return Rt(r, t);
  }
  return l ? u : vn(u, c, rl, kn(t, !1));
}
function Di(e, t, s, n) {
  const { implicitKey: i, inFlow: r } = t, o = typeof e.value == "string" ? e : Object.assign({}, e, { value: String(e.value) });
  let { type: l } = e;
  l !== K.QUOTE_DOUBLE && /[\x00-\x08\x0b-\x1f\x7f-\x9f\u{D800}-\u{DFFF}]/u.test(o.value) && (l = K.QUOTE_DOUBLE);
  const c = (f) => {
    switch (f) {
      case K.BLOCK_FOLDED:
      case K.BLOCK_LITERAL:
        return i || r ? Rt(o.value, t) : zs(o, t, s, n);
      case K.QUOTE_DOUBLE:
        return Ss(o.value, t);
      case K.QUOTE_SINGLE:
        return ui(o.value, t);
      case K.PLAIN:
        return Of(o, t, s, n);
      default:
        return null;
    }
  };
  let a = c(l);
  if (a === null) {
    const { defaultKeyType: f, defaultStringType: u } = t.options, g = i && f || u;
    if (a = c(g), a === null)
      throw new Error(`Unsupported default string type ${g}`);
  }
  return a;
}
function ol(e, t) {
  const s = Object.assign({
    blockQuote: !0,
    commentString: vf,
    defaultKeyType: null,
    defaultStringType: "PLAIN",
    directives: null,
    doubleQuotedAsJSON: !1,
    doubleQuotedMinMultiLineLength: 40,
    falseStr: "false",
    flowCollectionPadding: !0,
    indentSeq: !0,
    lineWidth: 80,
    minContentWidth: 20,
    nullStr: "null",
    simpleKeys: !1,
    singleQuote: null,
    trailingComma: !1,
    trueStr: "true",
    verifyAliasOrder: !0
  }, e.schema.toStringOptions, t);
  let n;
  switch (s.collectionStyle) {
    case "block":
      n = !1;
      break;
    case "flow":
      n = !0;
      break;
    default:
      n = null;
  }
  return {
    anchors: /* @__PURE__ */ new Set(),
    doc: e,
    flowCollectionPadding: s.flowCollectionPadding ? " " : "",
    indent: "",
    indentStep: typeof s.indent == "number" ? " ".repeat(s.indent) : "  ",
    inFlow: n,
    options: s
  };
}
function Tf(e, t) {
  var i;
  if (t.tag) {
    const r = e.filter((o) => o.tag === t.tag);
    if (r.length > 0)
      return r.find((o) => o.format === t.format) ?? r[0];
  }
  let s, n;
  if (Z(t)) {
    n = t.value;
    let r = e.filter((o) => {
      var l;
      return (l = o.identify) == null ? void 0 : l.call(o, n);
    });
    if (r.length > 1) {
      const o = r.filter((l) => l.test);
      o.length > 0 && (r = o);
    }
    s = r.find((o) => o.format === t.format) ?? r.find((o) => !o.format);
  } else
    n = t, s = e.find((r) => r.nodeClass && n instanceof r.nodeClass);
  if (!s) {
    const r = ((i = n == null ? void 0 : n.constructor) == null ? void 0 : i.name) ?? (n === null ? "null" : typeof n);
    throw new Error(`Tag not resolved for ${r} value`);
  }
  return s;
}
function Nf(e, t, { anchors: s, doc: n }) {
  if (!n.directives)
    return "";
  const i = [], r = (Z(e) || ne(e)) && e.anchor;
  r && el(r) && (s.add(r), i.push(`&${r}`));
  const o = e.tag ?? (t.default ? null : t.tag);
  return o && i.push(n.directives.tagString(o)), i.join(" ");
}
function Wt(e, t, s, n) {
  var c;
  if (re(e))
    return e.toString(t, s, n);
  if (Xt(e)) {
    if (t.doc.directives)
      return e.toString(t);
    if ((c = t.resolvedAliases) != null && c.has(e))
      throw new TypeError("Cannot stringify circular structure without alias nodes");
    t.resolvedAliases ? t.resolvedAliases.add(e) : t.resolvedAliases = /* @__PURE__ */ new Set([e]), e = e.resolve(t.doc);
  }
  let i;
  const r = ie(e) ? e : t.doc.createNode(e, { onTagObj: (a) => i = a });
  i ?? (i = Tf(t.doc.schema.tags, r));
  const o = Nf(r, i, t);
  o.length > 0 && (t.indentAtStart = (t.indentAtStart ?? 0) + o.length + 1);
  const l = typeof i.stringify == "function" ? i.stringify(r, t, s, n) : Z(r) ? Di(r, t, s, n) : r.toString(t, s, n);
  return o ? Z(r) || l[0] === "{" || l[0] === "[" ? `${o} ${l}` : `${o}
${t.indent}${l}` : l;
}
function Af({ key: e, value: t }, s, n, i) {
  const { allNullValues: r, doc: o, indent: l, indentStep: c, options: { commentString: a, indentSeq: f, simpleKeys: u } } = s;
  let g = ie(e) && e.comment || null;
  if (u) {
    if (g)
      throw new Error("With simple keys, key nodes cannot have comments");
    if (ne(e) || !ie(e) && typeof e == "object") {
      const D = "With simple keys, collection cannot be used as a key value";
      throw new Error(D);
    }
  }
  let p = !u && (!e || g && t == null && !s.inFlow || ne(e) || (Z(e) ? e.type === K.BLOCK_FOLDED || e.type === K.BLOCK_LITERAL : typeof e == "object"));
  s = Object.assign({}, s, {
    allNullValues: !1,
    implicitKey: !p && (u || !r),
    indent: l + c
  });
  let k = !1, m = !1, _ = Wt(e, s, () => k = !0, () => m = !0);
  if (!p && !s.inFlow && _.length > 1024) {
    if (u)
      throw new Error("With simple keys, single line scalar must not span more than 1024 characters");
    p = !0;
  }
  if (s.inFlow) {
    if (r || t == null)
      return k && n && n(), _ === "" ? "?" : p ? `? ${_}` : _;
  } else if (r && !u || t == null && p)
    return _ = `? ${_}`, g && !k ? _ += vt(_, s.indent, a(g)) : m && i && i(), _;
  k && (g = null), p ? (g && (_ += vt(_, s.indent, a(g))), _ = `? ${_}
${l}:`) : (_ = `${_}:`, g && (_ += vt(_, s.indent, a(g))));
  let b, y, S;
  ie(t) ? (b = !!t.spaceBefore, y = t.commentBefore, S = t.comment) : (b = !1, y = null, S = null, t && typeof t == "object" && (t = o.createNode(t))), s.implicitKey = !1, !p && !g && Z(t) && (s.indentAtStart = _.length + 1), m = !1, !f && c.length >= 2 && !s.inFlow && !p && Ps(t) && !t.flow && !t.tag && !t.anchor && (s.indent = s.indent.substring(2));
  let A = !1;
  const N = Wt(t, s, () => A = !0, () => m = !0);
  let x = " ";
  if (g || b || y) {
    if (x = b ? `
` : "", y) {
      const D = a(y);
      x += `
${rt(D, s.indent)}`;
    }
    N === "" && !s.inFlow ? x === `
` && S && (x = `

`) : x += `
${s.indent}`;
  } else if (!p && ne(t)) {
    const D = N[0], M = N.indexOf(`
`), q = M !== -1, ee = s.inFlow ?? t.flow ?? t.items.length === 0;
    if (q || !ee) {
      let pe = !1;
      if (q && (D === "&" || D === "!")) {
        let fe = N.indexOf(" ");
        D === "&" && fe !== -1 && fe < M && N[fe + 1] === "!" && (fe = N.indexOf(" ", fe + 1)), (fe === -1 || M < fe) && (pe = !0);
      }
      pe || (x = `
${s.indent}`);
    }
  } else (N === "" || N[0] === `
`) && (x = "");
  return _ += x + N, s.inFlow ? A && n && n() : S && !A ? _ += vt(_, s.indent, a(S)) : m && i && i(), _;
}
function ll(e, t) {
  (e === "debug" || e === "warn") && console.warn(t);
}
const Us = "<<", ct = {
  identify: (e) => e === Us || typeof e == "symbol" && e.description === Us,
  default: "key",
  tag: "tag:yaml.org,2002:merge",
  test: /^<<$/,
  resolve: () => Object.assign(new K(Symbol(Us)), {
    addToJSMap: cl
  }),
  stringify: () => Us
}, Ef = (e, t) => (ct.identify(t) || Z(t) && (!t.type || t.type === K.PLAIN) && ct.identify(t.value)) && (e == null ? void 0 : e.doc.schema.tags.some((s) => s.tag === ct.tag && s.default));
function cl(e, t, s) {
  const n = al(e, s);
  if (Ps(n))
    for (const i of n.items)
      Wn(e, t, i);
  else if (Array.isArray(n))
    for (const i of n)
      Wn(e, t, i);
  else
    Wn(e, t, n);
}
function Wn(e, t, s) {
  const n = al(e, s);
  if (!Ms(n))
    throw new Error("Merge sources must be maps or map aliases");
  const i = n.toJSON(null, e, Map);
  for (const [r, o] of i)
    t instanceof Map ? t.has(r) || t.set(r, o) : t instanceof Set ? t.add(r) : Object.prototype.hasOwnProperty.call(t, r) || Object.defineProperty(t, r, {
      value: o,
      writable: !0,
      enumerable: !0,
      configurable: !0
    });
  return t;
}
function al(e, t) {
  return e && Xt(t) ? t.resolve(e.doc, e) : t;
}
function fl(e, t, { key: s, value: n }) {
  if (ie(s) && s.addToJSMap)
    s.addToJSMap(e, t, n);
  else if (Ef(e, s))
    cl(e, t, n);
  else {
    const i = Le(s, "", e);
    if (t instanceof Map)
      t.set(i, Le(n, i, e));
    else if (t instanceof Set)
      t.add(i);
    else {
      const r = Cf(s, i, e), o = Le(n, r, e);
      r in t ? Object.defineProperty(t, r, {
        value: o,
        writable: !0,
        enumerable: !0,
        configurable: !0
      }) : t[r] = o;
    }
  }
  return t;
}
function Cf(e, t, s) {
  if (t === null)
    return "";
  if (typeof t != "object")
    return String(t);
  if (ie(e) && (s != null && s.doc)) {
    const n = ol(s.doc, {});
    n.anchors = /* @__PURE__ */ new Set();
    for (const r of s.anchors.keys())
      n.anchors.add(r.anchor);
    n.inFlow = !0, n.inStringifyKey = !0;
    const i = e.toString(n);
    if (!s.mapKeyWarned) {
      let r = JSON.stringify(i);
      r.length > 40 && (r = r.substring(0, 36) + '..."'), ll(s.doc.options.logLevel, `Keys with collection values will be stringified due to JS Object restrictions: ${r}. Set mapAsMap: true to use object keys.`), s.mapKeyWarned = !0;
    }
    return i;
  }
  return JSON.stringify(t);
}
function Ri(e, t, s) {
  const n = As(e, void 0, s), i = As(t, void 0, s);
  return new _e(n, i);
}
class _e {
  constructor(t, s = null) {
    Object.defineProperty(this, $e, { value: zo }), this.key = t, this.value = s;
  }
  clone(t) {
    let { key: s, value: n } = this;
    return ie(s) && (s = s.clone(t)), ie(n) && (n = n.clone(t)), new _e(s, n);
  }
  toJSON(t, s) {
    const n = s != null && s.mapAsMap ? /* @__PURE__ */ new Map() : {};
    return fl(s, n, this);
  }
  toString(t, s, n) {
    return t != null && t.doc ? Af(this, t, s, n) : JSON.stringify(this);
  }
}
function ul(e, t, s) {
  return (t.inFlow ?? e.flow ? Lf : If)(e, t, s);
}
function If({ comment: e, items: t }, s, { blockItemPrefix: n, flowChars: i, itemIndent: r, onChompKeep: o, onComment: l }) {
  const { indent: c, options: { commentString: a } } = s, f = Object.assign({}, s, { indent: r, type: null });
  let u = !1;
  const g = [];
  for (let k = 0; k < t.length; ++k) {
    const m = t[k];
    let _ = null;
    if (ie(m))
      !u && m.spaceBefore && g.push(""), cn(s, g, m.commentBefore, u), m.comment && (_ = m.comment);
    else if (re(m)) {
      const y = ie(m.key) ? m.key : null;
      y && (!u && y.spaceBefore && g.push(""), cn(s, g, y.commentBefore, u));
    }
    u = !1;
    let b = Wt(m, f, () => _ = null, () => u = !0);
    _ && (b += vt(b, r, a(_))), u && _ && (u = !1), g.push(n + b);
  }
  let p;
  if (g.length === 0)
    p = i.start + i.end;
  else {
    p = g[0];
    for (let k = 1; k < g.length; ++k) {
      const m = g[k];
      p += m ? `
${c}${m}` : `
`;
    }
  }
  return e ? (p += `
` + rt(a(e), c), l && l()) : u && o && o(), p;
}
function Lf({ items: e }, t, { flowChars: s, itemIndent: n }) {
  const { indent: i, indentStep: r, flowCollectionPadding: o, options: { commentString: l } } = t;
  n += r;
  const c = Object.assign({}, t, {
    indent: n,
    inFlow: !0,
    type: null
  });
  let a = !1, f = 0;
  const u = [];
  for (let k = 0; k < e.length; ++k) {
    const m = e[k];
    let _ = null;
    if (ie(m))
      m.spaceBefore && u.push(""), cn(t, u, m.commentBefore, !1), m.comment && (_ = m.comment);
    else if (re(m)) {
      const y = ie(m.key) ? m.key : null;
      y && (y.spaceBefore && u.push(""), cn(t, u, y.commentBefore, !1), y.comment && (a = !0));
      const S = ie(m.value) ? m.value : null;
      S ? (S.comment && (_ = S.comment), S.commentBefore && (a = !0)) : m.value == null && (y != null && y.comment) && (_ = y.comment);
    }
    _ && (a = !0);
    let b = Wt(m, c, () => _ = null);
    a || (a = u.length > f || b.includes(`
`)), k < e.length - 1 ? b += "," : t.options.trailingComma && (t.options.lineWidth > 0 && (a || (a = u.reduce((y, S) => y + S.length + 2, 2) + (b.length + 2) > t.options.lineWidth)), a && (b += ",")), _ && (b += vt(b, n, l(_))), u.push(b), f = u.length;
  }
  const { start: g, end: p } = s;
  if (u.length === 0)
    return g + p;
  if (!a) {
    const k = u.reduce((m, _) => m + _.length + 2, 2);
    a = t.options.lineWidth > 0 && k > t.options.lineWidth;
  }
  if (a) {
    let k = g;
    for (const m of u)
      k += m ? `
${r}${i}${m}` : `
`;
    return `${k}
${i}${p}`;
  } else
    return `${g}${o}${u.join(" ")}${o}${p}`;
}
function cn({ indent: e, options: { commentString: t } }, s, n, i) {
  if (n && i && (n = n.replace(/^\n+/, "")), n) {
    const r = rt(t(n), e);
    s.push(r.trimStart());
  }
}
function kt(e, t) {
  const s = Z(t) ? t.value : t;
  for (const n of e)
    if (re(n) && (n.key === t || n.key === s || Z(n.key) && n.key.value === s))
      return n;
}
class Ee extends il {
  static get tagName() {
    return "tag:yaml.org,2002:map";
  }
  constructor(t) {
    super(pt, t), this.items = [];
  }
  /**
   * A generic collection parsing method that can be extended
   * to other node classes that inherit from YAMLMap
   */
  static from(t, s, n) {
    const { keepUndefined: i, replacer: r } = n, o = new this(t), l = (c, a) => {
      if (typeof r == "function")
        a = r.call(s, c, a);
      else if (Array.isArray(r) && !r.includes(c))
        return;
      (a !== void 0 || i) && o.items.push(Ri(c, a, n));
    };
    if (s instanceof Map)
      for (const [c, a] of s)
        l(c, a);
    else if (s && typeof s == "object")
      for (const c of Object.keys(s))
        l(c, s[c]);
    return typeof t.sortMapEntries == "function" && o.items.sort(t.sortMapEntries), o;
  }
  /**
   * Adds a value to the collection.
   *
   * @param overwrite - If not set `true`, using a key that is already in the
   *   collection will throw. Otherwise, overwrites the previous value.
   */
  add(t, s) {
    var o;
    let n;
    re(t) ? n = t : !t || typeof t != "object" || !("key" in t) ? n = new _e(t, t == null ? void 0 : t.value) : n = new _e(t.key, t.value);
    const i = kt(this.items, n.key), r = (o = this.schema) == null ? void 0 : o.sortMapEntries;
    if (i) {
      if (!s)
        throw new Error(`Key ${n.key} already set`);
      Z(i.value) && nl(n.value) ? i.value.value = n.value : i.value = n.value;
    } else if (r) {
      const l = this.items.findIndex((c) => r(n, c) < 0);
      l === -1 ? this.items.push(n) : this.items.splice(l, 0, n);
    } else
      this.items.push(n);
  }
  delete(t) {
    const s = kt(this.items, t);
    return s ? this.items.splice(this.items.indexOf(s), 1).length > 0 : !1;
  }
  get(t, s) {
    const n = kt(this.items, t), i = n == null ? void 0 : n.value;
    return (!s && Z(i) ? i.value : i) ?? void 0;
  }
  has(t) {
    return !!kt(this.items, t);
  }
  set(t, s) {
    this.add(new _e(t, s), !0);
  }
  /**
   * @param ctx - Conversion context, originally set in Document#toJS()
   * @param {Class} Type - If set, forces the returned collection type
   * @returns Instance of Type, Map, or Object
   */
  toJSON(t, s, n) {
    const i = n ? new n() : s != null && s.mapAsMap ? /* @__PURE__ */ new Map() : {};
    s != null && s.onCreate && s.onCreate(i);
    for (const r of this.items)
      fl(s, i, r);
    return i;
  }
  toString(t, s, n) {
    if (!t)
      return JSON.stringify(this);
    for (const i of this.items)
      if (!re(i))
        throw new Error(`Map items must all be pairs; found ${JSON.stringify(i)} instead`);
    return !t.allNullValues && this.hasAllNullValues(!1) && (t = Object.assign({}, t, { allNullValues: !0 })), ul(this, t, {
      blockItemPrefix: "",
      flowChars: { start: "{", end: "}" },
      itemIndent: t.indent || "",
      onChompKeep: n,
      onComment: s
    });
  }
}
const Zt = {
  collection: "map",
  default: !0,
  nodeClass: Ee,
  tag: "tag:yaml.org,2002:map",
  resolve(e, t) {
    return Ms(e) || t("Expected a mapping for this tag"), e;
  },
  createNode: (e, t, s) => Ee.from(e, t, s)
};
class Nt extends il {
  static get tagName() {
    return "tag:yaml.org,2002:seq";
  }
  constructor(t) {
    super(Qt, t), this.items = [];
  }
  add(t) {
    this.items.push(t);
  }
  /**
   * Removes a value from the collection.
   *
   * `key` must contain a representation of an integer for this to succeed.
   * It may be wrapped in a `Scalar`.
   *
   * @returns `true` if the item was found and removed.
   */
  delete(t) {
    const s = Vs(t);
    return typeof s != "number" ? !1 : this.items.splice(s, 1).length > 0;
  }
  get(t, s) {
    const n = Vs(t);
    if (typeof n != "number")
      return;
    const i = this.items[n];
    return !s && Z(i) ? i.value : i;
  }
  /**
   * Checks if the collection includes a value with the key `key`.
   *
   * `key` must contain a representation of an integer for this to succeed.
   * It may be wrapped in a `Scalar`.
   */
  has(t) {
    const s = Vs(t);
    return typeof s == "number" && s < this.items.length;
  }
  /**
   * Sets a value in this collection. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   *
   * If `key` does not contain a representation of an integer, this will throw.
   * It may be wrapped in a `Scalar`.
   */
  set(t, s) {
    const n = Vs(t);
    if (typeof n != "number")
      throw new Error(`Expected a valid index, not ${t}.`);
    const i = this.items[n];
    Z(i) && nl(s) ? i.value = s : this.items[n] = s;
  }
  toJSON(t, s) {
    const n = [];
    s != null && s.onCreate && s.onCreate(n);
    let i = 0;
    for (const r of this.items)
      n.push(Le(r, String(i++), s));
    return n;
  }
  toString(t, s, n) {
    return t ? ul(this, t, {
      blockItemPrefix: "- ",
      flowChars: { start: "[", end: "]" },
      itemIndent: (t.indent || "") + "  ",
      onChompKeep: n,
      onComment: s
    }) : JSON.stringify(this);
  }
  static from(t, s, n) {
    const { replacer: i } = n, r = new this(t);
    if (s && Symbol.iterator in Object(s)) {
      let o = 0;
      for (let l of s) {
        if (typeof i == "function") {
          const c = s instanceof Set ? l : String(o++);
          l = i.call(s, c, l);
        }
        r.items.push(As(l, void 0, n));
      }
    }
    return r;
  }
}
function Vs(e) {
  let t = Z(e) ? e.value : e;
  return t && typeof t == "string" && (t = Number(t)), typeof t == "number" && Number.isInteger(t) && t >= 0 ? t : null;
}
const es = {
  collection: "seq",
  default: !0,
  nodeClass: Nt,
  tag: "tag:yaml.org,2002:seq",
  resolve(e, t) {
    return Ps(e) || t("Expected a sequence for this tag"), e;
  },
  createNode: (e, t, s) => Nt.from(e, t, s)
}, Tn = {
  identify: (e) => typeof e == "string",
  default: !0,
  tag: "tag:yaml.org,2002:str",
  resolve: (e) => e,
  stringify(e, t, s, n) {
    return t = Object.assign({ actualString: !0 }, t), Di(e, t, s, n);
  }
}, Nn = {
  identify: (e) => e == null,
  createNode: () => new K(null),
  default: !0,
  tag: "tag:yaml.org,2002:null",
  test: /^(?:~|[Nn]ull|NULL)?$/,
  resolve: () => new K(null),
  stringify: ({ source: e }, t) => typeof e == "string" && Nn.test.test(e) ? e : t.options.nullStr
}, ji = {
  identify: (e) => typeof e == "boolean",
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:[Tt]rue|TRUE|[Ff]alse|FALSE)$/,
  resolve: (e) => new K(e[0] === "t" || e[0] === "T"),
  stringify({ source: e, value: t }, s) {
    if (e && ji.test.test(e)) {
      const n = e[0] === "t" || e[0] === "T";
      if (t === n)
        return e;
    }
    return t ? s.options.trueStr : s.options.falseStr;
  }
};
function Be({ format: e, minFractionDigits: t, tag: s, value: n }) {
  if (typeof n == "bigint")
    return String(n);
  const i = typeof n == "number" ? n : Number(n);
  if (!isFinite(i))
    return isNaN(i) ? ".nan" : i < 0 ? "-.inf" : ".inf";
  let r = Object.is(n, -0) ? "-0" : JSON.stringify(n);
  if (!e && t && (!s || s === "tag:yaml.org,2002:float") && /^-?\d/.test(r) && !r.includes("e")) {
    let o = r.indexOf(".");
    o < 0 && (o = r.length, r += ".");
    let l = t - (r.length - o - 1);
    for (; l-- > 0; )
      r += "0";
  }
  return r;
}
const hl = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^(?:[-+]?\.(?:inf|Inf|INF)|\.nan|\.NaN|\.NAN)$/,
  resolve: (e) => e.slice(-3).toLowerCase() === "nan" ? NaN : e[0] === "-" ? Number.NEGATIVE_INFINITY : Number.POSITIVE_INFINITY,
  stringify: Be
}, dl = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "EXP",
  test: /^[-+]?(?:\.[0-9]+|[0-9]+(?:\.[0-9]*)?)[eE][-+]?[0-9]+$/,
  resolve: (e) => parseFloat(e),
  stringify(e) {
    const t = Number(e.value);
    return isFinite(t) ? t.toExponential() : Be(e);
  }
}, pl = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^[-+]?(?:\.[0-9]+|[0-9]+\.[0-9]*)$/,
  resolve(e) {
    const t = new K(parseFloat(e)), s = e.indexOf(".");
    return s !== -1 && e[e.length - 1] === "0" && (t.minFractionDigits = e.length - s - 1), t;
  },
  stringify: Be
}, An = (e) => typeof e == "bigint" || Number.isInteger(e), Bi = (e, t, s, { intAsBigInt: n }) => n ? BigInt(e) : parseInt(e.substring(t), s);
function gl(e, t, s) {
  const { value: n } = e;
  return An(n) && n >= 0 ? s + n.toString(t) : Be(e);
}
const ml = {
  identify: (e) => An(e) && e >= 0,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "OCT",
  test: /^0o[0-7]+$/,
  resolve: (e, t, s) => Bi(e, 2, 8, s),
  stringify: (e) => gl(e, 8, "0o")
}, yl = {
  identify: An,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  test: /^[-+]?[0-9]+$/,
  resolve: (e, t, s) => Bi(e, 0, 10, s),
  stringify: Be
}, bl = {
  identify: (e) => An(e) && e >= 0,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "HEX",
  test: /^0x[0-9a-fA-F]+$/,
  resolve: (e, t, s) => Bi(e, 2, 16, s),
  stringify: (e) => gl(e, 16, "0x")
}, $f = [
  Zt,
  es,
  Tn,
  Nn,
  ji,
  ml,
  yl,
  bl,
  hl,
  dl,
  pl
];
function Pr(e) {
  return typeof e == "bigint" || Number.isInteger(e);
}
const qs = ({ value: e }) => JSON.stringify(e), Mf = [
  {
    identify: (e) => typeof e == "string",
    default: !0,
    tag: "tag:yaml.org,2002:str",
    resolve: (e) => e,
    stringify: qs
  },
  {
    identify: (e) => e == null,
    createNode: () => new K(null),
    default: !0,
    tag: "tag:yaml.org,2002:null",
    test: /^null$/,
    resolve: () => null,
    stringify: qs
  },
  {
    identify: (e) => typeof e == "boolean",
    default: !0,
    tag: "tag:yaml.org,2002:bool",
    test: /^true$|^false$/,
    resolve: (e) => e === "true",
    stringify: qs
  },
  {
    identify: Pr,
    default: !0,
    tag: "tag:yaml.org,2002:int",
    test: /^-?(?:0|[1-9][0-9]*)$/,
    resolve: (e, t, { intAsBigInt: s }) => s ? BigInt(e) : parseInt(e, 10),
    stringify: ({ value: e }) => Pr(e) ? e.toString() : JSON.stringify(e)
  },
  {
    identify: (e) => typeof e == "number",
    default: !0,
    tag: "tag:yaml.org,2002:float",
    test: /^-?(?:0|[1-9][0-9]*)(?:\.[0-9]*)?(?:[eE][-+]?[0-9]+)?$/,
    resolve: (e) => parseFloat(e),
    stringify: qs
  }
], Pf = {
  default: !0,
  tag: "",
  test: /^/,
  resolve(e, t) {
    return t(`Unresolved plain scalar ${JSON.stringify(e)}`), e;
  }
}, Df = [Zt, es].concat(Mf, Pf), Fi = {
  identify: (e) => e instanceof Uint8Array,
  // Buffer inherits from Uint8Array
  default: !1,
  tag: "tag:yaml.org,2002:binary",
  /**
   * Returns a Buffer in node and an Uint8Array in browsers
   *
   * To use the resulting buffer as an image, you'll want to do something like:
   *
   *   const blob = new Blob([buffer], { type: 'image/jpeg' })
   *   document.querySelector('#photo').src = URL.createObjectURL(blob)
   */
  resolve(e, t) {
    if (typeof atob == "function") {
      const s = atob(e.replace(/[\n\r]/g, "")), n = new Uint8Array(s.length);
      for (let i = 0; i < s.length; ++i)
        n[i] = s.charCodeAt(i);
      return n;
    } else
      return t("This environment does not support reading binary tags; either Buffer or atob is required"), e;
  },
  stringify({ comment: e, type: t, value: s }, n, i, r) {
    if (!s)
      return "";
    const o = s;
    let l;
    if (typeof btoa == "function") {
      let c = "";
      for (let a = 0; a < o.length; ++a)
        c += String.fromCharCode(o[a]);
      l = btoa(c);
    } else
      throw new Error("This environment does not support writing binary tags; either Buffer or btoa is required");
    if (t ?? (t = K.BLOCK_LITERAL), t !== K.QUOTE_DOUBLE) {
      const c = Math.max(n.options.lineWidth - n.indent.length, n.options.minContentWidth), a = Math.ceil(l.length / c), f = new Array(a);
      for (let u = 0, g = 0; u < a; ++u, g += c)
        f[u] = l.substr(g, c);
      l = f.join(t === K.BLOCK_LITERAL ? `
` : " ");
    }
    return Di({ comment: e, type: t, value: l }, n, i, r);
  }
};
function wl(e, t) {
  if (Ps(e))
    for (let s = 0; s < e.items.length; ++s) {
      let n = e.items[s];
      if (!re(n)) {
        if (Ms(n)) {
          n.items.length > 1 && t("Each pair must have its own sequence indicator");
          const i = n.items[0] || new _e(new K(null));
          if (n.commentBefore && (i.key.commentBefore = i.key.commentBefore ? `${n.commentBefore}
${i.key.commentBefore}` : n.commentBefore), n.comment) {
            const r = i.value ?? i.key;
            r.comment = r.comment ? `${n.comment}
${r.comment}` : n.comment;
          }
          n = i;
        }
        e.items[s] = re(n) ? n : new _e(n);
      }
    }
  else
    t("Expected a sequence for this tag");
  return e;
}
function Sl(e, t, s) {
  const { replacer: n } = s, i = new Nt(e);
  i.tag = "tag:yaml.org,2002:pairs";
  let r = 0;
  if (t && Symbol.iterator in Object(t))
    for (let o of t) {
      typeof n == "function" && (o = n.call(t, String(r++), o));
      let l, c;
      if (Array.isArray(o))
        if (o.length === 2)
          l = o[0], c = o[1];
        else
          throw new TypeError(`Expected [key, value] tuple: ${o}`);
      else if (o && o instanceof Object) {
        const a = Object.keys(o);
        if (a.length === 1)
          l = a[0], c = o[l];
        else
          throw new TypeError(`Expected tuple with one key, not ${a.length} keys`);
      } else
        l = o;
      i.items.push(Ri(l, c, s));
    }
  return i;
}
const Ki = {
  collection: "seq",
  default: !1,
  tag: "tag:yaml.org,2002:pairs",
  resolve: wl,
  createNode: Sl
};
class xt extends Nt {
  constructor() {
    super(), this.add = Ee.prototype.add.bind(this), this.delete = Ee.prototype.delete.bind(this), this.get = Ee.prototype.get.bind(this), this.has = Ee.prototype.has.bind(this), this.set = Ee.prototype.set.bind(this), this.tag = xt.tag;
  }
  /**
   * If `ctx` is given, the return type is actually `Map<unknown, unknown>`,
   * but TypeScript won't allow widening the signature of a child method.
   */
  toJSON(t, s) {
    if (!s)
      return super.toJSON(t);
    const n = /* @__PURE__ */ new Map();
    s != null && s.onCreate && s.onCreate(n);
    for (const i of this.items) {
      let r, o;
      if (re(i) ? (r = Le(i.key, "", s), o = Le(i.value, r, s)) : r = Le(i, "", s), n.has(r))
        throw new Error("Ordered maps must not include duplicate keys");
      n.set(r, o);
    }
    return n;
  }
  static from(t, s, n) {
    const i = Sl(t, s, n), r = new this();
    return r.items = i.items, r;
  }
}
xt.tag = "tag:yaml.org,2002:omap";
const xi = {
  collection: "seq",
  identify: (e) => e instanceof Map,
  nodeClass: xt,
  default: !1,
  tag: "tag:yaml.org,2002:omap",
  resolve(e, t) {
    const s = wl(e, t), n = [];
    for (const { key: i } of s.items)
      Z(i) && (n.includes(i.value) ? t(`Ordered maps must not include duplicate keys: ${i.value}`) : n.push(i.value));
    return Object.assign(new xt(), s);
  },
  createNode: (e, t, s) => xt.from(e, t, s)
};
function _l({ value: e, source: t }, s) {
  return t && (e ? vl : kl).test.test(t) ? t : e ? s.options.trueStr : s.options.falseStr;
}
const vl = {
  identify: (e) => e === !0,
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:Y|y|[Yy]es|YES|[Tt]rue|TRUE|[Oo]n|ON)$/,
  resolve: () => new K(!0),
  stringify: _l
}, kl = {
  identify: (e) => e === !1,
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:N|n|[Nn]o|NO|[Ff]alse|FALSE|[Oo]ff|OFF)$/,
  resolve: () => new K(!1),
  stringify: _l
}, Rf = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^(?:[-+]?\.(?:inf|Inf|INF)|\.nan|\.NaN|\.NAN)$/,
  resolve: (e) => e.slice(-3).toLowerCase() === "nan" ? NaN : e[0] === "-" ? Number.NEGATIVE_INFINITY : Number.POSITIVE_INFINITY,
  stringify: Be
}, jf = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "EXP",
  test: /^[-+]?(?:[0-9][0-9_]*)?(?:\.[0-9_]*)?[eE][-+]?[0-9]+$/,
  resolve: (e) => parseFloat(e.replace(/_/g, "")),
  stringify(e) {
    const t = Number(e.value);
    return isFinite(t) ? t.toExponential() : Be(e);
  }
}, Bf = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^[-+]?(?:[0-9][0-9_]*)?\.[0-9_]*$/,
  resolve(e) {
    const t = new K(parseFloat(e.replace(/_/g, ""))), s = e.indexOf(".");
    if (s !== -1) {
      const n = e.substring(s + 1).replace(/_/g, "");
      n[n.length - 1] === "0" && (t.minFractionDigits = n.length);
    }
    return t;
  },
  stringify: Be
}, Ds = (e) => typeof e == "bigint" || Number.isInteger(e);
function En(e, t, s, { intAsBigInt: n }) {
  const i = e[0];
  if ((i === "-" || i === "+") && (t += 1), e = e.substring(t).replace(/_/g, ""), n) {
    switch (s) {
      case 2:
        e = `0b${e}`;
        break;
      case 8:
        e = `0o${e}`;
        break;
      case 16:
        e = `0x${e}`;
        break;
    }
    const o = BigInt(e);
    return i === "-" ? BigInt(-1) * o : o;
  }
  const r = parseInt(e, s);
  return i === "-" ? -1 * r : r;
}
function Ui(e, t, s) {
  const { value: n } = e;
  if (Ds(n)) {
    const i = n.toString(t);
    return n < 0 ? "-" + s + i.substr(1) : s + i;
  }
  return Be(e);
}
const Ff = {
  identify: Ds,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "BIN",
  test: /^[-+]?0b[0-1_]+$/,
  resolve: (e, t, s) => En(e, 2, 2, s),
  stringify: (e) => Ui(e, 2, "0b")
}, Kf = {
  identify: Ds,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "OCT",
  test: /^[-+]?0[0-7_]+$/,
  resolve: (e, t, s) => En(e, 1, 8, s),
  stringify: (e) => Ui(e, 8, "0")
}, xf = {
  identify: Ds,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  test: /^[-+]?[0-9][0-9_]*$/,
  resolve: (e, t, s) => En(e, 0, 10, s),
  stringify: Be
}, Uf = {
  identify: Ds,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "HEX",
  test: /^[-+]?0x[0-9a-fA-F_]+$/,
  resolve: (e, t, s) => En(e, 2, 16, s),
  stringify: (e) => Ui(e, 16, "0x")
};
class Ut extends Ee {
  constructor(t) {
    super(t), this.tag = Ut.tag;
  }
  add(t) {
    let s;
    re(t) ? s = t : t && typeof t == "object" && "key" in t && "value" in t && t.value === null ? s = new _e(t.key, null) : s = new _e(t, null), kt(this.items, s.key) || this.items.push(s);
  }
  /**
   * If `keepPair` is `true`, returns the Pair matching `key`.
   * Otherwise, returns the value of that Pair's key.
   */
  get(t, s) {
    const n = kt(this.items, t);
    return !s && re(n) ? Z(n.key) ? n.key.value : n.key : n;
  }
  set(t, s) {
    if (typeof s != "boolean")
      throw new Error(`Expected boolean value for set(key, value) in a YAML set, not ${typeof s}`);
    const n = kt(this.items, t);
    n && !s ? this.items.splice(this.items.indexOf(n), 1) : !n && s && this.items.push(new _e(t));
  }
  toJSON(t, s) {
    return super.toJSON(t, s, Set);
  }
  toString(t, s, n) {
    if (!t)
      return JSON.stringify(this);
    if (this.hasAllNullValues(!0))
      return super.toString(Object.assign({}, t, { allNullValues: !0 }), s, n);
    throw new Error("Set items must all have null values");
  }
  static from(t, s, n) {
    const { replacer: i } = n, r = new this(t);
    if (s && Symbol.iterator in Object(s))
      for (let o of s)
        typeof i == "function" && (o = i.call(s, o, o)), r.items.push(Ri(o, null, n));
    return r;
  }
}
Ut.tag = "tag:yaml.org,2002:set";
const Vi = {
  collection: "map",
  identify: (e) => e instanceof Set,
  nodeClass: Ut,
  default: !1,
  tag: "tag:yaml.org,2002:set",
  createNode: (e, t, s) => Ut.from(e, t, s),
  resolve(e, t) {
    if (Ms(e)) {
      if (e.hasAllNullValues(!0))
        return Object.assign(new Ut(), e);
      t("Set items must all have null values");
    } else
      t("Expected a mapping for this tag");
    return e;
  }
};
function qi(e, t) {
  const s = e[0], n = s === "-" || s === "+" ? e.substring(1) : e, i = (o) => t ? BigInt(o) : Number(o), r = n.replace(/_/g, "").split(":").reduce((o, l) => o * i(60) + i(l), i(0));
  return s === "-" ? i(-1) * r : r;
}
function Ol(e) {
  let { value: t } = e, s = (o) => o;
  if (typeof t == "bigint")
    s = (o) => BigInt(o);
  else if (isNaN(t) || !isFinite(t))
    return Be(e);
  let n = "";
  t < 0 && (n = "-", t *= s(-1));
  const i = s(60), r = [t % i];
  return t < 60 ? r.unshift(0) : (t = (t - r[0]) / i, r.unshift(t % i), t >= 60 && (t = (t - r[0]) / i, r.unshift(t))), n + r.map((o) => String(o).padStart(2, "0")).join(":").replace(/000000\d*$/, "");
}
const Tl = {
  identify: (e) => typeof e == "bigint" || Number.isInteger(e),
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "TIME",
  test: /^[-+]?[0-9][0-9_]*(?::[0-5]?[0-9])+$/,
  resolve: (e, t, { intAsBigInt: s }) => qi(e, s),
  stringify: Ol
}, Nl = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "TIME",
  test: /^[-+]?[0-9][0-9_]*(?::[0-5]?[0-9])+\.[0-9_]*$/,
  resolve: (e) => qi(e, !1),
  stringify: Ol
}, Cn = {
  identify: (e) => e instanceof Date,
  default: !0,
  tag: "tag:yaml.org,2002:timestamp",
  // If the time zone is omitted, the timestamp is assumed to be specified in UTC. The time part
  // may be omitted altogether, resulting in a date format. In such a case, the time part is
  // assumed to be 00:00:00Z (start of day, UTC).
  test: RegExp("^([0-9]{4})-([0-9]{1,2})-([0-9]{1,2})(?:(?:t|T|[ \\t]+)([0-9]{1,2}):([0-9]{1,2}):([0-9]{1,2}(\\.[0-9]+)?)(?:[ \\t]*(Z|[-+][012]?[0-9](?::[0-9]{2})?))?)?$"),
  resolve(e) {
    const t = e.match(Cn.test);
    if (!t)
      throw new Error("!!timestamp expects a date, starting with yyyy-mm-dd");
    const [, s, n, i, r, o, l] = t.map(Number), c = t[7] ? Number((t[7] + "00").substr(1, 3)) : 0;
    let a = Date.UTC(s, n - 1, i, r || 0, o || 0, l || 0, c);
    const f = t[8];
    if (f && f !== "Z") {
      let u = qi(f, !1);
      Math.abs(u) < 30 && (u *= 60), a -= 6e4 * u;
    }
    return new Date(a);
  },
  stringify: ({ value: e }) => (e == null ? void 0 : e.toISOString().replace(/(T00:00:00)?\.000Z$/, "")) ?? ""
}, Dr = [
  Zt,
  es,
  Tn,
  Nn,
  vl,
  kl,
  Ff,
  Kf,
  xf,
  Uf,
  Rf,
  jf,
  Bf,
  Fi,
  ct,
  xi,
  Ki,
  Vi,
  Tl,
  Nl,
  Cn
], Rr = /* @__PURE__ */ new Map([
  ["core", $f],
  ["failsafe", [Zt, es, Tn]],
  ["json", Df],
  ["yaml11", Dr],
  ["yaml-1.1", Dr]
]), jr = {
  binary: Fi,
  bool: ji,
  float: pl,
  floatExp: dl,
  floatNaN: hl,
  floatTime: Nl,
  int: yl,
  intHex: bl,
  intOct: ml,
  intTime: Tl,
  map: Zt,
  merge: ct,
  null: Nn,
  omap: xi,
  pairs: Ki,
  seq: es,
  set: Vi,
  timestamp: Cn
}, Vf = {
  "tag:yaml.org,2002:binary": Fi,
  "tag:yaml.org,2002:merge": ct,
  "tag:yaml.org,2002:omap": xi,
  "tag:yaml.org,2002:pairs": Ki,
  "tag:yaml.org,2002:set": Vi,
  "tag:yaml.org,2002:timestamp": Cn
};
function Jn(e, t, s) {
  const n = Rr.get(t);
  if (n && !e)
    return s && !n.includes(ct) ? n.concat(ct) : n.slice();
  let i = n;
  if (!i)
    if (Array.isArray(e))
      i = [];
    else {
      const r = Array.from(Rr.keys()).filter((o) => o !== "yaml11").map((o) => JSON.stringify(o)).join(", ");
      throw new Error(`Unknown schema "${t}"; use one of ${r} or define customTags array`);
    }
  if (Array.isArray(e))
    for (const r of e)
      i = i.concat(r);
  else typeof e == "function" && (i = e(i.slice()));
  return s && (i = i.concat(ct)), i.reduce((r, o) => {
    const l = typeof o == "string" ? jr[o] : o;
    if (!l) {
      const c = JSON.stringify(o), a = Object.keys(jr).map((f) => JSON.stringify(f)).join(", ");
      throw new Error(`Unknown custom tag ${c}; use one of ${a}`);
    }
    return r.includes(l) || r.push(l), r;
  }, []);
}
const qf = (e, t) => e.key < t.key ? -1 : e.key > t.key ? 1 : 0;
class Hi {
  constructor({ compat: t, customTags: s, merge: n, resolveKnownTags: i, schema: r, sortMapEntries: o, toStringDefaults: l }) {
    this.compat = Array.isArray(t) ? Jn(t, "compat") : t ? Jn(null, t) : null, this.name = typeof r == "string" && r || "core", this.knownTags = i ? Vf : {}, this.tags = Jn(s, this.name, n), this.toStringOptions = l ?? null, Object.defineProperty(this, pt, { value: Zt }), Object.defineProperty(this, Ze, { value: Tn }), Object.defineProperty(this, Qt, { value: es }), this.sortMapEntries = typeof o == "function" ? o : o === !0 ? qf : null;
  }
  clone() {
    const t = Object.create(Hi.prototype, Object.getOwnPropertyDescriptors(this));
    return t.tags = this.tags.slice(), t;
  }
}
function Hf(e, t) {
  var c;
  const s = [];
  let n = t.directives === !0;
  if (t.directives !== !1 && e.directives) {
    const a = e.directives.toString(e);
    a ? (s.push(a), n = !0) : e.directives.docStart && (n = !0);
  }
  n && s.push("---");
  const i = ol(e, t), { commentString: r } = i.options;
  if (e.commentBefore) {
    s.length !== 1 && s.unshift("");
    const a = r(e.commentBefore);
    s.unshift(rt(a, ""));
  }
  let o = !1, l = null;
  if (e.contents) {
    if (ie(e.contents)) {
      if (e.contents.spaceBefore && n && s.push(""), e.contents.commentBefore) {
        const u = r(e.contents.commentBefore);
        s.push(rt(u, ""));
      }
      i.forceBlockIndent = !!e.comment, l = e.contents.comment;
    }
    const a = l ? void 0 : () => o = !0;
    let f = Wt(e.contents, i, () => l = null, a);
    l && (f += vt(f, "", r(l))), (f[0] === "|" || f[0] === ">") && s[s.length - 1] === "---" ? s[s.length - 1] = `--- ${f}` : s.push(f);
  } else
    s.push(Wt(e.contents, i));
  if ((c = e.directives) != null && c.docEnd)
    if (e.comment) {
      const a = r(e.comment);
      a.includes(`
`) ? (s.push("..."), s.push(rt(a, ""))) : s.push(`... ${a}`);
    } else
      s.push("...");
  else {
    let a = e.comment;
    a && o && (a = a.replace(/^\n+/, "")), a && ((!o || l) && s[s.length - 1] !== "" && s.push(""), s.push(rt(r(a), "")));
  }
  return s.join(`
`) + `
`;
}
let Wi = class Al {
  constructor(t, s, n) {
    this.commentBefore = null, this.comment = null, this.errors = [], this.warnings = [], Object.defineProperty(this, $e, { value: ai });
    let i = null;
    typeof s == "function" || Array.isArray(s) ? i = s : n === void 0 && s && (n = s, s = void 0);
    const r = Object.assign({
      intAsBigInt: !1,
      keepSourceTokens: !1,
      logLevel: "warn",
      prettyErrors: !0,
      strict: !0,
      stringKeys: !1,
      uniqueKeys: !0,
      version: "1.2"
    }, n);
    this.options = r;
    let { version: o } = r;
    n != null && n._directives ? (this.directives = n._directives.atDocument(), this.directives.yaml.explicit && (o = this.directives.yaml.version)) : this.directives = new be({ version: o }), this.setSchema(o, n), this.contents = t === void 0 ? null : this.createNode(t, i, n);
  }
  /**
   * Create a deep copy of this Document and its contents.
   *
   * Custom Node values that inherit from `Object` still refer to their original instances.
   */
  clone() {
    const t = Object.create(Al.prototype, {
      [$e]: { value: ai }
    });
    return t.commentBefore = this.commentBefore, t.comment = this.comment, t.errors = this.errors.slice(), t.warnings = this.warnings.slice(), t.options = Object.assign({}, this.options), this.directives && (t.directives = this.directives.clone()), t.schema = this.schema.clone(), t.contents = ie(this.contents) ? this.contents.clone(t.schema) : this.contents, this.range && (t.range = this.range.slice()), t;
  }
  /** Adds a value to the document. */
  add(t) {
    It(this.contents) && this.contents.add(t);
  }
  /** Adds a value to the document. */
  addIn(t, s) {
    It(this.contents) && this.contents.addIn(t, s);
  }
  /**
   * Create a new `Alias` node, ensuring that the target `node` has the required anchor.
   *
   * If `node` already has an anchor, `name` is ignored.
   * Otherwise, the `node.anchor` value will be set to `name`,
   * or if an anchor with that name is already present in the document,
   * `name` will be used as a prefix for a new unique anchor.
   * If `name` is undefined, the generated anchor will use 'a' as a prefix.
   */
  createAlias(t, s) {
    if (!t.anchor) {
      const n = tl(this);
      t.anchor = // eslint-disable-next-line @typescript-eslint/prefer-nullish-coalescing
      !s || n.has(s) ? sl(s || "a", n) : s;
    }
    return new Pi(t.anchor);
  }
  createNode(t, s, n) {
    let i;
    if (typeof s == "function")
      t = s.call({ "": t }, "", t), i = s;
    else if (Array.isArray(s)) {
      const _ = (y) => typeof y == "number" || y instanceof String || y instanceof Number, b = s.filter(_).map(String);
      b.length > 0 && (s = s.concat(b)), i = s;
    } else n === void 0 && s && (n = s, s = void 0);
    const { aliasDuplicateObjects: r, anchorPrefix: o, flow: l, keepUndefined: c, onTagObj: a, tag: f } = n ?? {}, { onAnchor: u, setAnchors: g, sourceObjects: p } = wf(
      this,
      // eslint-disable-next-line @typescript-eslint/prefer-nullish-coalescing
      o || "a"
    ), k = {
      aliasDuplicateObjects: r ?? !0,
      keepUndefined: c ?? !1,
      onAnchor: u,
      onTagObj: a,
      replacer: i,
      schema: this.schema,
      sourceObjects: p
    }, m = As(t, f, k);
    return l && ne(m) && (m.flow = !0), g(), m;
  }
  /**
   * Convert a key and a value into a `Pair` using the current schema,
   * recursively wrapping all values as `Scalar` or `Collection` nodes.
   */
  createPair(t, s, n = {}) {
    const i = this.createNode(t, null, n), r = this.createNode(s, null, n);
    return new _e(i, r);
  }
  /**
   * Removes a value from the document.
   * @returns `true` if the item was found and removed.
   */
  delete(t) {
    return It(this.contents) ? this.contents.delete(t) : !1;
  }
  /**
   * Removes a value from the document.
   * @returns `true` if the item was found and removed.
   */
  deleteIn(t) {
    return fs(t) ? this.contents == null ? !1 : (this.contents = null, !0) : It(this.contents) ? this.contents.deleteIn(t) : !1;
  }
  /**
   * Returns item at `key`, or `undefined` if not found. By default unwraps
   * scalar values from their surrounding node; to disable set `keepScalar` to
   * `true` (collections are always returned intact).
   */
  get(t, s) {
    return ne(this.contents) ? this.contents.get(t, s) : void 0;
  }
  /**
   * Returns item at `path`, or `undefined` if not found. By default unwraps
   * scalar values from their surrounding node; to disable set `keepScalar` to
   * `true` (collections are always returned intact).
   */
  getIn(t, s) {
    return fs(t) ? !s && Z(this.contents) ? this.contents.value : this.contents : ne(this.contents) ? this.contents.getIn(t, s) : void 0;
  }
  /**
   * Checks if the document includes a value with the key `key`.
   */
  has(t) {
    return ne(this.contents) ? this.contents.has(t) : !1;
  }
  /**
   * Checks if the document includes a value at `path`.
   */
  hasIn(t) {
    return fs(t) ? this.contents !== void 0 : ne(this.contents) ? this.contents.hasIn(t) : !1;
  }
  /**
   * Sets a value in this document. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   */
  set(t, s) {
    this.contents == null ? this.contents = ln(this.schema, [t], s) : It(this.contents) && this.contents.set(t, s);
  }
  /**
   * Sets a value in this document. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   */
  setIn(t, s) {
    fs(t) ? this.contents = s : this.contents == null ? this.contents = ln(this.schema, Array.from(t), s) : It(this.contents) && this.contents.setIn(t, s);
  }
  /**
   * Change the YAML version and schema used by the document.
   * A `null` version disables support for directives, explicit tags, anchors, and aliases.
   * It also requires the `schema` option to be given as a `Schema` instance value.
   *
   * Overrides all previously set schema options.
   */
  setSchema(t, s = {}) {
    typeof t == "number" && (t = String(t));
    let n;
    switch (t) {
      case "1.1":
        this.directives ? this.directives.yaml.version = "1.1" : this.directives = new be({ version: "1.1" }), n = { resolveKnownTags: !1, schema: "yaml-1.1" };
        break;
      case "1.2":
      case "next":
        this.directives ? this.directives.yaml.version = t : this.directives = new be({ version: t }), n = { resolveKnownTags: !0, schema: "core" };
        break;
      case null:
        this.directives && delete this.directives, n = null;
        break;
      default: {
        const i = JSON.stringify(t);
        throw new Error(`Expected '1.1', '1.2' or null as first argument, but found: ${i}`);
      }
    }
    if (s.schema instanceof Object)
      this.schema = s.schema;
    else if (n)
      this.schema = new Hi(Object.assign(n, s));
    else
      throw new Error("With a null YAML version, the { schema: Schema } option is required");
  }
  // json & jsonArg are only used from toJSON()
  toJS({ json: t, jsonArg: s, mapAsMap: n, maxAliasCount: i, onAnchor: r, reviver: o } = {}) {
    const l = {
      anchors: /* @__PURE__ */ new Map(),
      doc: this,
      keep: !t,
      mapAsMap: n === !0,
      mapKeyWarned: !1,
      maxAliasCount: typeof i == "number" ? i : 100
    }, c = Le(this.contents, s ?? "", l);
    if (typeof r == "function")
      for (const { count: a, res: f } of l.anchors.values())
        r(f, a);
    return typeof o == "function" ? Dt(o, { "": c }, "", c) : c;
  }
  /**
   * A JSON representation of the document `contents`.
   *
   * @param jsonArg Used by `JSON.stringify` to indicate the array index or
   *   property name.
   */
  toJSON(t, s) {
    return this.toJS({ json: !0, jsonArg: t, mapAsMap: !1, onAnchor: s });
  }
  /** A YAML representation of the document. */
  toString(t = {}) {
    if (this.errors.length > 0)
      throw new Error("Document with errors cannot be stringified");
    if ("indent" in t && (!Number.isInteger(t.indent) || Number(t.indent) <= 0)) {
      const s = JSON.stringify(t.indent);
      throw new Error(`"indent" option must be a positive integer, not ${s}`);
    }
    return Hf(this, t);
  }
};
function It(e) {
  if (ne(e))
    return !0;
  throw new Error("Expected a YAML collection as document contents");
}
class El extends Error {
  constructor(t, s, n, i) {
    super(), this.name = t, this.code = n, this.message = i, this.pos = s;
  }
}
class us extends El {
  constructor(t, s, n) {
    super("YAMLParseError", t, s, n);
  }
}
class Wf extends El {
  constructor(t, s, n) {
    super("YAMLWarning", t, s, n);
  }
}
const Br = (e, t) => (s) => {
  if (s.pos[0] === -1)
    return;
  s.linePos = s.pos.map((l) => t.linePos(l));
  const { line: n, col: i } = s.linePos[0];
  s.message += ` at line ${n}, column ${i}`;
  let r = i - 1, o = e.substring(t.lineStarts[n - 1], t.lineStarts[n]).replace(/[\n\r]+$/, "");
  if (r >= 60 && o.length > 80) {
    const l = Math.min(r - 39, o.length - 79);
    o = "…" + o.substring(l), r -= l - 1;
  }
  if (o.length > 80 && (o = o.substring(0, 79) + "…"), n > 1 && /^ *$/.test(o.substring(0, r))) {
    let l = e.substring(t.lineStarts[n - 2], t.lineStarts[n - 1]);
    l.length > 80 && (l = l.substring(0, 79) + `…
`), o = l + o;
  }
  if (/[^ ]/.test(o)) {
    let l = 1;
    const c = s.linePos[1];
    (c == null ? void 0 : c.line) === n && c.col > i && (l = Math.max(1, Math.min(c.col - i, 80 - r)));
    const a = " ".repeat(r) + "^".repeat(l);
    s.message += `:

${o}
${a}
`;
  }
};
function Jt(e, { flow: t, indicator: s, next: n, offset: i, onError: r, parentIndent: o, startOnNewline: l }) {
  let c = !1, a = l, f = l, u = "", g = "", p = !1, k = !1, m = null, _ = null, b = null, y = null, S = null, A = null, N = null;
  for (const M of e)
    switch (k && (M.type !== "space" && M.type !== "newline" && M.type !== "comma" && r(M.offset, "MISSING_CHAR", "Tags and anchors must be separated from the next token by white space"), k = !1), m && (a && M.type !== "comment" && M.type !== "newline" && r(m, "TAB_AS_INDENT", "Tabs are not allowed as indentation"), m = null), M.type) {
      case "space":
        !t && (s !== "doc-start" || (n == null ? void 0 : n.type) !== "flow-collection") && M.source.includes("	") && (m = M), f = !0;
        break;
      case "comment": {
        f || r(M, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters");
        const q = M.source.substring(1) || " ";
        u ? u += g + q : u = q, g = "", a = !1;
        break;
      }
      case "newline":
        a ? u ? u += M.source : (!A || s !== "seq-item-ind") && (c = !0) : g += M.source, a = !0, p = !0, (_ || b) && (y = M), f = !0;
        break;
      case "anchor":
        _ && r(M, "MULTIPLE_ANCHORS", "A node can have at most one anchor"), M.source.endsWith(":") && r(M.offset + M.source.length - 1, "BAD_ALIAS", "Anchor ending in : is ambiguous", !0), _ = M, N ?? (N = M.offset), a = !1, f = !1, k = !0;
        break;
      case "tag": {
        b && r(M, "MULTIPLE_TAGS", "A node can have at most one tag"), b = M, N ?? (N = M.offset), a = !1, f = !1, k = !0;
        break;
      }
      case s:
        (_ || b) && r(M, "BAD_PROP_ORDER", `Anchors and tags must be after the ${M.source} indicator`), A && r(M, "UNEXPECTED_TOKEN", `Unexpected ${M.source} in ${t ?? "collection"}`), A = M, a = s === "seq-item-ind" || s === "explicit-key-ind", f = !1;
        break;
      case "comma":
        if (t) {
          S && r(M, "UNEXPECTED_TOKEN", `Unexpected , in ${t}`), S = M, a = !1, f = !1;
          break;
        }
      // else fallthrough
      default:
        r(M, "UNEXPECTED_TOKEN", `Unexpected ${M.type} token`), a = !1, f = !1;
    }
  const x = e[e.length - 1], D = x ? x.offset + x.source.length : i;
  return k && n && n.type !== "space" && n.type !== "newline" && n.type !== "comma" && (n.type !== "scalar" || n.source !== "") && r(n.offset, "MISSING_CHAR", "Tags and anchors must be separated from the next token by white space"), m && (a && m.indent <= o || (n == null ? void 0 : n.type) === "block-map" || (n == null ? void 0 : n.type) === "block-seq") && r(m, "TAB_AS_INDENT", "Tabs are not allowed as indentation"), {
    comma: S,
    found: A,
    spaceBefore: c,
    comment: u,
    hasNewline: p,
    anchor: _,
    tag: b,
    newlineAfterProp: y,
    end: D,
    start: N ?? D
  };
}
function Es(e) {
  if (!e)
    return null;
  switch (e.type) {
    case "alias":
    case "scalar":
    case "double-quoted-scalar":
    case "single-quoted-scalar":
      if (e.source.includes(`
`))
        return !0;
      if (e.end) {
        for (const t of e.end)
          if (t.type === "newline")
            return !0;
      }
      return !1;
    case "flow-collection":
      for (const t of e.items) {
        for (const s of t.start)
          if (s.type === "newline")
            return !0;
        if (t.sep) {
          for (const s of t.sep)
            if (s.type === "newline")
              return !0;
        }
        if (Es(t.key) || Es(t.value))
          return !0;
      }
      return !1;
    default:
      return !0;
  }
}
function di(e, t, s) {
  if ((t == null ? void 0 : t.type) === "flow-collection") {
    const n = t.end[0];
    n.indent === e && (n.source === "]" || n.source === "}") && Es(t) && s(n, "BAD_INDENT", "Flow end indicator should be more indented than parent", !0);
  }
}
function Cl(e, t, s) {
  const { uniqueKeys: n } = e.options;
  if (n === !1)
    return !1;
  const i = typeof n == "function" ? n : (r, o) => r === o || Z(r) && Z(o) && r.value === o.value;
  return t.some((r) => i(r.key, s));
}
const Fr = "All mapping items must start at the same column";
function Jf({ composeNode: e, composeEmptyNode: t }, s, n, i, r) {
  var f;
  const o = (r == null ? void 0 : r.nodeClass) ?? Ee, l = new o(s.schema);
  s.atRoot && (s.atRoot = !1);
  let c = n.offset, a = null;
  for (const u of n.items) {
    const { start: g, key: p, sep: k, value: m } = u, _ = Jt(g, {
      indicator: "explicit-key-ind",
      next: p ?? (k == null ? void 0 : k[0]),
      offset: c,
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !0
    }), b = !_.found;
    if (b) {
      if (p && (p.type === "block-seq" ? i(c, "BLOCK_AS_IMPLICIT_KEY", "A block sequence may not be used as an implicit map key") : "indent" in p && p.indent !== n.indent && i(c, "BAD_INDENT", Fr)), !_.anchor && !_.tag && !k) {
        a = _.end, _.comment && (l.comment ? l.comment += `
` + _.comment : l.comment = _.comment);
        continue;
      }
      (_.newlineAfterProp || Es(p)) && i(p ?? g[g.length - 1], "MULTILINE_IMPLICIT_KEY", "Implicit keys need to be on a single line");
    } else ((f = _.found) == null ? void 0 : f.indent) !== n.indent && i(c, "BAD_INDENT", Fr);
    s.atKey = !0;
    const y = _.end, S = p ? e(s, p, _, i) : t(s, y, g, null, _, i);
    s.schema.compat && di(n.indent, p, i), s.atKey = !1, Cl(s, l.items, S) && i(y, "DUPLICATE_KEY", "Map keys must be unique");
    const A = Jt(k ?? [], {
      indicator: "map-value-ind",
      next: m,
      offset: S.range[2],
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !p || p.type === "block-scalar"
    });
    if (c = A.end, A.found) {
      b && ((m == null ? void 0 : m.type) === "block-map" && !A.hasNewline && i(c, "BLOCK_AS_IMPLICIT_KEY", "Nested mappings are not allowed in compact mappings"), s.options.strict && _.start < A.found.offset - 1024 && i(S.range, "KEY_OVER_1024_CHARS", "The : indicator must be at most 1024 chars after the start of an implicit block mapping key"));
      const N = m ? e(s, m, A, i) : t(s, c, k, null, A, i);
      s.schema.compat && di(n.indent, m, i), c = N.range[2];
      const x = new _e(S, N);
      s.options.keepSourceTokens && (x.srcToken = u), l.items.push(x);
    } else {
      b && i(S.range, "MISSING_CHAR", "Implicit map keys need to be followed by map values"), A.comment && (S.comment ? S.comment += `
` + A.comment : S.comment = A.comment);
      const N = new _e(S);
      s.options.keepSourceTokens && (N.srcToken = u), l.items.push(N);
    }
  }
  return a && a < c && i(a, "IMPOSSIBLE", "Map comment with trailing content"), l.range = [n.offset, c, a ?? c], l;
}
function Yf({ composeNode: e, composeEmptyNode: t }, s, n, i, r) {
  const o = (r == null ? void 0 : r.nodeClass) ?? Nt, l = new o(s.schema);
  s.atRoot && (s.atRoot = !1), s.atKey && (s.atKey = !1);
  let c = n.offset, a = null;
  for (const { start: f, value: u } of n.items) {
    const g = Jt(f, {
      indicator: "seq-item-ind",
      next: u,
      offset: c,
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !0
    });
    if (!g.found)
      if (g.anchor || g.tag || u)
        (u == null ? void 0 : u.type) === "block-seq" ? i(g.end, "BAD_INDENT", "All sequence items must start at the same column") : i(c, "MISSING_CHAR", "Sequence item without - indicator");
      else {
        a = g.end, g.comment && (l.comment = g.comment);
        continue;
      }
    const p = u ? e(s, u, g, i) : t(s, g.end, f, null, g, i);
    s.schema.compat && di(n.indent, u, i), c = p.range[2], l.items.push(p);
  }
  return l.range = [n.offset, c, a ?? c], l;
}
function Rs(e, t, s, n) {
  let i = "";
  if (e) {
    let r = !1, o = "";
    for (const l of e) {
      const { source: c, type: a } = l;
      switch (a) {
        case "space":
          r = !0;
          break;
        case "comment": {
          s && !r && n(l, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters");
          const f = c.substring(1) || " ";
          i ? i += o + f : i = f, o = "";
          break;
        }
        case "newline":
          i && (o += c), r = !0;
          break;
        default:
          n(l, "UNEXPECTED_TOKEN", `Unexpected ${a} at node end`);
      }
      t += c.length;
    }
  }
  return { comment: i, offset: t };
}
const Yn = "Block collections are not allowed within flow collections", Gn = (e) => e && (e.type === "block-map" || e.type === "block-seq");
function Gf({ composeNode: e, composeEmptyNode: t }, s, n, i, r) {
  var _;
  const o = n.start.source === "{", l = o ? "flow map" : "flow sequence", c = (r == null ? void 0 : r.nodeClass) ?? (o ? Ee : Nt), a = new c(s.schema);
  a.flow = !0;
  const f = s.atRoot;
  f && (s.atRoot = !1), s.atKey && (s.atKey = !1);
  let u = n.offset + n.start.source.length;
  for (let b = 0; b < n.items.length; ++b) {
    const y = n.items[b], { start: S, key: A, sep: N, value: x } = y, D = Jt(S, {
      flow: l,
      indicator: "explicit-key-ind",
      next: A ?? (N == null ? void 0 : N[0]),
      offset: u,
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !1
    });
    if (!D.found) {
      if (!D.anchor && !D.tag && !N && !x) {
        b === 0 && D.comma ? i(D.comma, "UNEXPECTED_TOKEN", `Unexpected , in ${l}`) : b < n.items.length - 1 && i(D.start, "UNEXPECTED_TOKEN", `Unexpected empty item in ${l}`), D.comment && (a.comment ? a.comment += `
` + D.comment : a.comment = D.comment), u = D.end;
        continue;
      }
      !o && s.options.strict && Es(A) && i(
        A,
        // checked by containsNewline()
        "MULTILINE_IMPLICIT_KEY",
        "Implicit keys of flow sequence pairs need to be on a single line"
      );
    }
    if (b === 0)
      D.comma && i(D.comma, "UNEXPECTED_TOKEN", `Unexpected , in ${l}`);
    else if (D.comma || i(D.start, "MISSING_CHAR", `Missing , between ${l} items`), D.comment) {
      let M = "";
      e: for (const q of S)
        switch (q.type) {
          case "comma":
          case "space":
            break;
          case "comment":
            M = q.source.substring(1);
            break e;
          default:
            break e;
        }
      if (M) {
        let q = a.items[a.items.length - 1];
        re(q) && (q = q.value ?? q.key), q.comment ? q.comment += `
` + M : q.comment = M, D.comment = D.comment.substring(M.length + 1);
      }
    }
    if (!o && !N && !D.found) {
      const M = x ? e(s, x, D, i) : t(s, D.end, N, null, D, i);
      a.items.push(M), u = M.range[2], Gn(x) && i(M.range, "BLOCK_IN_FLOW", Yn);
    } else {
      s.atKey = !0;
      const M = D.end, q = A ? e(s, A, D, i) : t(s, M, S, null, D, i);
      Gn(A) && i(q.range, "BLOCK_IN_FLOW", Yn), s.atKey = !1;
      const ee = Jt(N ?? [], {
        flow: l,
        indicator: "map-value-ind",
        next: x,
        offset: q.range[2],
        onError: i,
        parentIndent: n.indent,
        startOnNewline: !1
      });
      if (ee.found) {
        if (!o && !D.found && s.options.strict) {
          if (N)
            for (const ue of N) {
              if (ue === ee.found)
                break;
              if (ue.type === "newline") {
                i(ue, "MULTILINE_IMPLICIT_KEY", "Implicit keys of flow sequence pairs need to be on a single line");
                break;
              }
            }
          D.start < ee.found.offset - 1024 && i(ee.found, "KEY_OVER_1024_CHARS", "The : indicator must be at most 1024 chars after the start of an implicit flow sequence key");
        }
      } else x && ("source" in x && ((_ = x.source) == null ? void 0 : _[0]) === ":" ? i(x, "MISSING_CHAR", `Missing space after : in ${l}`) : i(ee.start, "MISSING_CHAR", `Missing , or : between ${l} items`));
      const pe = x ? e(s, x, ee, i) : ee.found ? t(s, ee.end, N, null, ee, i) : null;
      pe ? Gn(x) && i(pe.range, "BLOCK_IN_FLOW", Yn) : ee.comment && (q.comment ? q.comment += `
` + ee.comment : q.comment = ee.comment);
      const fe = new _e(q, pe);
      if (s.options.keepSourceTokens && (fe.srcToken = y), o) {
        const ue = a;
        Cl(s, ue.items, q) && i(M, "DUPLICATE_KEY", "Map keys must be unique"), ue.items.push(fe);
      } else {
        const ue = new Ee(s.schema);
        ue.flow = !0, ue.items.push(fe);
        const Et = (pe ?? q).range;
        ue.range = [q.range[0], Et[1], Et[2]], a.items.push(ue);
      }
      u = pe ? pe.range[2] : ee.end;
    }
  }
  const g = o ? "}" : "]", [p, ...k] = n.end;
  let m = u;
  if ((p == null ? void 0 : p.source) === g)
    m = p.offset + p.source.length;
  else {
    const b = l[0].toUpperCase() + l.substring(1), y = f ? `${b} must end with a ${g}` : `${b} in block collection must be sufficiently indented and end with a ${g}`;
    i(u, f ? "MISSING_CHAR" : "BAD_INDENT", y), p && p.source.length !== 1 && k.unshift(p);
  }
  if (k.length > 0) {
    const b = Rs(k, m, s.options.strict, i);
    b.comment && (a.comment ? a.comment += `
` + b.comment : a.comment = b.comment), a.range = [n.offset, m, b.offset];
  } else
    a.range = [n.offset, m, m];
  return a;
}
function Qn(e, t, s, n, i, r) {
  const o = s.type === "block-map" ? Jf(e, t, s, n, r) : s.type === "block-seq" ? Yf(e, t, s, n, r) : Gf(e, t, s, n, r), l = o.constructor;
  return i === "!" || i === l.tagName ? (o.tag = l.tagName, o) : (i && (o.tag = i), o);
}
function Qf(e, t, s, n, i) {
  var g;
  const r = n.tag, o = r ? t.directives.tagName(r.source, (p) => i(r, "TAG_RESOLVE_FAILED", p)) : null;
  if (s.type === "block-seq") {
    const { anchor: p, newlineAfterProp: k } = n, m = p && r ? p.offset > r.offset ? p : r : p ?? r;
    m && (!k || k.offset < m.offset) && i(m, "MISSING_CHAR", "Missing newline after block sequence props");
  }
  const l = s.type === "block-map" ? "map" : s.type === "block-seq" ? "seq" : s.start.source === "{" ? "map" : "seq";
  if (!r || !o || o === "!" || o === Ee.tagName && l === "map" || o === Nt.tagName && l === "seq")
    return Qn(e, t, s, i, o);
  let c = t.schema.tags.find((p) => p.tag === o && p.collection === l);
  if (!c) {
    const p = t.schema.knownTags[o];
    if ((p == null ? void 0 : p.collection) === l)
      t.schema.tags.push(Object.assign({}, p, { default: !1 })), c = p;
    else
      return p ? i(r, "BAD_COLLECTION_TYPE", `${p.tag} used for ${l} collection, but expects ${p.collection ?? "scalar"}`, !0) : i(r, "TAG_RESOLVE_FAILED", `Unresolved tag: ${o}`, !0), Qn(e, t, s, i, o);
  }
  const a = Qn(e, t, s, i, o, c), f = ((g = c.resolve) == null ? void 0 : g.call(c, a, (p) => i(r, "TAG_RESOLVE_FAILED", p), t.options)) ?? a, u = ie(f) ? f : new K(f);
  return u.range = a.range, u.tag = o, c != null && c.format && (u.format = c.format), u;
}
function Xf(e, t, s) {
  const n = t.offset, i = zf(t, e.options.strict, s);
  if (!i)
    return { value: "", type: null, comment: "", range: [n, n, n] };
  const r = i.mode === ">" ? K.BLOCK_FOLDED : K.BLOCK_LITERAL, o = t.source ? Zf(t.source) : [];
  let l = o.length;
  for (let m = o.length - 1; m >= 0; --m) {
    const _ = o[m][1];
    if (_ === "" || _ === "\r")
      l = m;
    else
      break;
  }
  if (l === 0) {
    const m = i.chomp === "+" && o.length > 0 ? `
`.repeat(Math.max(1, o.length - 1)) : "";
    let _ = n + i.length;
    return t.source && (_ += t.source.length), { value: m, type: r, comment: i.comment, range: [n, _, _] };
  }
  let c = t.indent + i.indent, a = t.offset + i.length, f = 0;
  for (let m = 0; m < l; ++m) {
    const [_, b] = o[m];
    if (b === "" || b === "\r")
      i.indent === 0 && _.length > c && (c = _.length);
    else {
      _.length < c && s(a + _.length, "MISSING_CHAR", "Block scalars with more-indented leading empty lines must use an explicit indentation indicator"), i.indent === 0 && (c = _.length), f = m, c === 0 && !e.atRoot && s(a, "BAD_INDENT", "Block scalar values in collections must be indented");
      break;
    }
    a += _.length + b.length + 1;
  }
  for (let m = o.length - 1; m >= l; --m)
    o[m][0].length > c && (l = m + 1);
  let u = "", g = "", p = !1;
  for (let m = 0; m < f; ++m)
    u += o[m][0].slice(c) + `
`;
  for (let m = f; m < l; ++m) {
    let [_, b] = o[m];
    a += _.length + b.length + 1;
    const y = b[b.length - 1] === "\r";
    if (y && (b = b.slice(0, -1)), b && _.length < c) {
      const A = `Block scalar lines must not be less indented than their ${i.indent ? "explicit indentation indicator" : "first line"}`;
      s(a - b.length - (y ? 2 : 1), "BAD_INDENT", A), _ = "";
    }
    r === K.BLOCK_LITERAL ? (u += g + _.slice(c) + b, g = `
`) : _.length > c || b[0] === "	" ? (g === " " ? g = `
` : !p && g === `
` && (g = `

`), u += g + _.slice(c) + b, g = `
`, p = !0) : b === "" ? g === `
` ? u += `
` : g = `
` : (u += g + b, g = " ", p = !1);
  }
  switch (i.chomp) {
    case "-":
      break;
    case "+":
      for (let m = l; m < o.length; ++m)
        u += `
` + o[m][0].slice(c);
      u[u.length - 1] !== `
` && (u += `
`);
      break;
    default:
      u += `
`;
  }
  const k = n + i.length + t.source.length;
  return { value: u, type: r, comment: i.comment, range: [n, k, k] };
}
function zf({ offset: e, props: t }, s, n) {
  if (t[0].type !== "block-scalar-header")
    return n(t[0], "IMPOSSIBLE", "Block scalar header not found"), null;
  const { source: i } = t[0], r = i[0];
  let o = 0, l = "", c = -1;
  for (let g = 1; g < i.length; ++g) {
    const p = i[g];
    if (!l && (p === "-" || p === "+"))
      l = p;
    else {
      const k = Number(p);
      !o && k ? o = k : c === -1 && (c = e + g);
    }
  }
  c !== -1 && n(c, "UNEXPECTED_TOKEN", `Block scalar header includes extra characters: ${i}`);
  let a = !1, f = "", u = i.length;
  for (let g = 1; g < t.length; ++g) {
    const p = t[g];
    switch (p.type) {
      case "space":
        a = !0;
      // fallthrough
      case "newline":
        u += p.source.length;
        break;
      case "comment":
        s && !a && n(p, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters"), u += p.source.length, f = p.source.substring(1);
        break;
      case "error":
        n(p, "UNEXPECTED_TOKEN", p.message), u += p.source.length;
        break;
      /* istanbul ignore next should not happen */
      default: {
        const k = `Unexpected token in block scalar header: ${p.type}`;
        n(p, "UNEXPECTED_TOKEN", k);
        const m = p.source;
        m && typeof m == "string" && (u += m.length);
      }
    }
  }
  return { mode: r, indent: o, chomp: l, comment: f, length: u };
}
function Zf(e) {
  const t = e.split(/\n( *)/), s = t[0], n = s.match(/^( *)/), r = [n != null && n[1] ? [n[1], s.slice(n[1].length)] : ["", s]];
  for (let o = 1; o < t.length; o += 2)
    r.push([t[o], t[o + 1]]);
  return r;
}
function eu(e, t, s) {
  const { offset: n, type: i, source: r, end: o } = e;
  let l, c;
  const a = (g, p, k) => s(n + g, p, k);
  switch (i) {
    case "scalar":
      l = K.PLAIN, c = tu(r, a);
      break;
    case "single-quoted-scalar":
      l = K.QUOTE_SINGLE, c = su(r, a);
      break;
    case "double-quoted-scalar":
      l = K.QUOTE_DOUBLE, c = nu(r, a);
      break;
    /* istanbul ignore next should not happen */
    default:
      return s(e, "UNEXPECTED_TOKEN", `Expected a flow scalar value, but found: ${i}`), {
        value: "",
        type: null,
        comment: "",
        range: [n, n + r.length, n + r.length]
      };
  }
  const f = n + r.length, u = Rs(o, f, t, s);
  return {
    value: c,
    type: l,
    comment: u.comment,
    range: [n, f, u.offset]
  };
}
function tu(e, t) {
  let s = "";
  switch (e[0]) {
    /* istanbul ignore next should not happen */
    case "	":
      s = "a tab character";
      break;
    case ",":
      s = "flow indicator character ,";
      break;
    case "%":
      s = "directive indicator character %";
      break;
    case "|":
    case ">": {
      s = `block scalar indicator ${e[0]}`;
      break;
    }
    case "@":
    case "`": {
      s = `reserved character ${e[0]}`;
      break;
    }
  }
  return s && t(0, "BAD_SCALAR_START", `Plain value cannot start with ${s}`), Il(e);
}
function su(e, t) {
  return (e[e.length - 1] !== "'" || e.length === 1) && t(e.length, "MISSING_CHAR", "Missing closing 'quote"), Il(e.slice(1, -1)).replace(/''/g, "'");
}
function Il(e) {
  let t, s;
  try {
    t = new RegExp(`(.*?)(?<![ 	])[ 	]*\r?
`, "sy"), s = new RegExp(`[ 	]*(.*?)(?:(?<![ 	])[ 	]*)?\r?
`, "sy");
  } catch {
    t = /(.*?)[ \t]*\r?\n/sy, s = /[ \t]*(.*?)[ \t]*\r?\n/sy;
  }
  let n = t.exec(e);
  if (!n)
    return e;
  let i = n[1], r = " ", o = t.lastIndex;
  for (s.lastIndex = o; n = s.exec(e); )
    n[1] === "" ? r === `
` ? i += r : r = `
` : (i += r + n[1], r = " "), o = s.lastIndex;
  const l = /[ \t]*(.*)/sy;
  return l.lastIndex = o, n = l.exec(e), i + r + ((n == null ? void 0 : n[1]) ?? "");
}
function nu(e, t) {
  let s = "";
  for (let n = 1; n < e.length - 1; ++n) {
    const i = e[n];
    if (!(i === "\r" && e[n + 1] === `
`))
      if (i === `
`) {
        const { fold: r, offset: o } = iu(e, n);
        s += r, n = o;
      } else if (i === "\\") {
        let r = e[++n];
        const o = ru[r];
        if (o)
          s += o;
        else if (r === `
`)
          for (r = e[n + 1]; r === " " || r === "	"; )
            r = e[++n + 1];
        else if (r === "\r" && e[n + 1] === `
`)
          for (r = e[++n + 1]; r === " " || r === "	"; )
            r = e[++n + 1];
        else if (r === "x" || r === "u" || r === "U") {
          const l = r === "x" ? 2 : r === "u" ? 4 : 8;
          s += ou(e, n + 1, l, t), n += l;
        } else {
          const l = e.substr(n - 1, 2);
          t(n - 1, "BAD_DQ_ESCAPE", `Invalid escape sequence ${l}`), s += l;
        }
      } else if (i === " " || i === "	") {
        const r = n;
        let o = e[n + 1];
        for (; o === " " || o === "	"; )
          o = e[++n + 1];
        o !== `
` && !(o === "\r" && e[n + 2] === `
`) && (s += n > r ? e.slice(r, n + 1) : i);
      } else
        s += i;
  }
  return (e[e.length - 1] !== '"' || e.length === 1) && t(e.length, "MISSING_CHAR", 'Missing closing "quote'), s;
}
function iu(e, t) {
  let s = "", n = e[t + 1];
  for (; (n === " " || n === "	" || n === `
` || n === "\r") && !(n === "\r" && e[t + 2] !== `
`); )
    n === `
` && (s += `
`), t += 1, n = e[t + 1];
  return s || (s = " "), { fold: s, offset: t };
}
const ru = {
  0: "\0",
  // null character
  a: "\x07",
  // bell character
  b: "\b",
  // backspace
  e: "\x1B",
  // escape character
  f: "\f",
  // form feed
  n: `
`,
  // line feed
  r: "\r",
  // carriage return
  t: "	",
  // horizontal tab
  v: "\v",
  // vertical tab
  N: "",
  // Unicode next line
  _: " ",
  // Unicode non-breaking space
  L: "\u2028",
  // Unicode line separator
  P: "\u2029",
  // Unicode paragraph separator
  " ": " ",
  '"': '"',
  "/": "/",
  "\\": "\\",
  "	": "	"
};
function ou(e, t, s, n) {
  const i = e.substr(t, s), o = i.length === s && /^[0-9a-fA-F]+$/.test(i) ? parseInt(i, 16) : NaN;
  try {
    return String.fromCodePoint(o);
  } catch {
    const l = e.substr(t - 2, s + 2);
    return n(t - 2, "BAD_DQ_ESCAPE", `Invalid escape sequence ${l}`), l;
  }
}
function Ll(e, t, s, n) {
  const { value: i, type: r, comment: o, range: l } = t.type === "block-scalar" ? Xf(e, t, n) : eu(t, e.options.strict, n), c = s ? e.directives.tagName(s.source, (u) => n(s, "TAG_RESOLVE_FAILED", u)) : null;
  let a;
  e.options.stringKeys && e.atKey ? a = e.schema[Ze] : c ? a = lu(e.schema, i, c, s, n) : t.type === "scalar" ? a = cu(e, i, t, n) : a = e.schema[Ze];
  let f;
  try {
    const u = a.resolve(i, (g) => n(s ?? t, "TAG_RESOLVE_FAILED", g), e.options);
    f = Z(u) ? u : new K(u);
  } catch (u) {
    const g = u instanceof Error ? u.message : String(u);
    n(s ?? t, "TAG_RESOLVE_FAILED", g), f = new K(i);
  }
  return f.range = l, f.source = i, r && (f.type = r), c && (f.tag = c), a.format && (f.format = a.format), o && (f.comment = o), f;
}
function lu(e, t, s, n, i) {
  var l;
  if (s === "!")
    return e[Ze];
  const r = [];
  for (const c of e.tags)
    if (!c.collection && c.tag === s)
      if (c.default && c.test)
        r.push(c);
      else
        return c;
  for (const c of r)
    if ((l = c.test) != null && l.test(t))
      return c;
  const o = e.knownTags[s];
  return o && !o.collection ? (e.tags.push(Object.assign({}, o, { default: !1, test: void 0 })), o) : (i(n, "TAG_RESOLVE_FAILED", `Unresolved tag: ${s}`, s !== "tag:yaml.org,2002:str"), e[Ze]);
}
function cu({ atKey: e, directives: t, schema: s }, n, i, r) {
  const o = s.tags.find((l) => {
    var c;
    return (l.default === !0 || e && l.default === "key") && ((c = l.test) == null ? void 0 : c.test(n));
  }) || s[Ze];
  if (s.compat) {
    const l = s.compat.find((c) => {
      var a;
      return c.default && ((a = c.test) == null ? void 0 : a.test(n));
    }) ?? s[Ze];
    if (o.tag !== l.tag) {
      const c = t.tagString(o.tag), a = t.tagString(l.tag), f = `Value may be parsed as either ${c} or ${a}`;
      r(i, "TAG_RESOLVE_FAILED", f, !0);
    }
  }
  return o;
}
function au(e, t, s) {
  if (t) {
    s ?? (s = t.length);
    for (let n = s - 1; n >= 0; --n) {
      let i = t[n];
      switch (i.type) {
        case "space":
        case "comment":
        case "newline":
          e -= i.source.length;
          continue;
      }
      for (i = t[++n]; (i == null ? void 0 : i.type) === "space"; )
        e += i.source.length, i = t[++n];
      break;
    }
  }
  return e;
}
const fu = { composeNode: $l, composeEmptyNode: Ji };
function $l(e, t, s, n) {
  const i = e.atKey, { spaceBefore: r, comment: o, anchor: l, tag: c } = s;
  let a, f = !0;
  switch (t.type) {
    case "alias":
      a = uu(e, t, n), (l || c) && n(t, "ALIAS_PROPS", "An alias node must not specify any properties");
      break;
    case "scalar":
    case "single-quoted-scalar":
    case "double-quoted-scalar":
    case "block-scalar":
      a = Ll(e, t, c, n), l && (a.anchor = l.source.substring(1));
      break;
    case "block-map":
    case "block-seq":
    case "flow-collection":
      try {
        a = Qf(fu, e, t, s, n), l && (a.anchor = l.source.substring(1));
      } catch (u) {
        const g = u instanceof Error ? u.message : String(u);
        n(t, "RESOURCE_EXHAUSTION", g);
      }
      break;
    default: {
      const u = t.type === "error" ? t.message : `Unsupported token (type: ${t.type})`;
      n(t, "UNEXPECTED_TOKEN", u), f = !1;
    }
  }
  return a ?? (a = Ji(e, t.offset, void 0, null, s, n)), l && a.anchor === "" && n(l, "BAD_ALIAS", "Anchor cannot be an empty string"), i && e.options.stringKeys && (!Z(a) || typeof a.value != "string" || a.tag && a.tag !== "tag:yaml.org,2002:str") && n(c ?? t, "NON_STRING_KEY", "With stringKeys, all keys must be strings"), r && (a.spaceBefore = !0), o && (t.type === "scalar" && t.source === "" ? a.comment = o : a.commentBefore = o), e.options.keepSourceTokens && f && (a.srcToken = t), a;
}
function Ji(e, t, s, n, { spaceBefore: i, comment: r, anchor: o, tag: l, end: c }, a) {
  const f = {
    type: "scalar",
    offset: au(t, s, n),
    indent: -1,
    source: ""
  }, u = Ll(e, f, l, a);
  return o && (u.anchor = o.source.substring(1), u.anchor === "" && a(o, "BAD_ALIAS", "Anchor cannot be an empty string")), i && (u.spaceBefore = !0), r && (u.comment = r, u.range[2] = c), u;
}
function uu({ options: e }, { offset: t, source: s, end: n }, i) {
  const r = new Pi(s.substring(1));
  r.source === "" && i(t, "BAD_ALIAS", "Alias cannot be an empty string"), r.source.endsWith(":") && i(t + s.length - 1, "BAD_ALIAS", "Alias ending in : is ambiguous", !0);
  const o = t + s.length, l = Rs(n, o, e.strict, i);
  return r.range = [t, o, l.offset], l.comment && (r.comment = l.comment), r;
}
function hu(e, t, { offset: s, start: n, value: i, end: r }, o) {
  const l = Object.assign({ _directives: t }, e), c = new Wi(void 0, l), a = {
    atKey: !1,
    atRoot: !0,
    directives: c.directives,
    options: c.options,
    schema: c.schema
  }, f = Jt(n, {
    indicator: "doc-start",
    next: i ?? (r == null ? void 0 : r[0]),
    offset: s,
    onError: o,
    parentIndent: 0,
    startOnNewline: !0
  });
  f.found && (c.directives.docStart = !0, i && (i.type === "block-map" || i.type === "block-seq") && !f.hasNewline && o(f.end, "MISSING_CHAR", "Block collection cannot start on same line with directives-end marker")), c.contents = i ? $l(a, i, f, o) : Ji(a, f.end, n, null, f, o);
  const u = c.contents.range[2], g = Rs(r, u, !1, o);
  return g.comment && (c.comment = g.comment), c.range = [s, u, g.offset], c;
}
function ls(e) {
  if (typeof e == "number")
    return [e, e + 1];
  if (Array.isArray(e))
    return e.length === 2 ? e : [e[0], e[1]];
  const { offset: t, source: s } = e;
  return [t, t + (typeof s == "string" ? s.length : 1)];
}
function Kr(e) {
  var i;
  let t = "", s = !1, n = !1;
  for (let r = 0; r < e.length; ++r) {
    const o = e[r];
    switch (o[0]) {
      case "#":
        t += (t === "" ? "" : n ? `

` : `
`) + (o.substring(1) || " "), s = !0, n = !1;
        break;
      case "%":
        ((i = e[r + 1]) == null ? void 0 : i[0]) !== "#" && (r += 1), s = !1;
        break;
      default:
        s || (n = !0), s = !1;
    }
  }
  return { comment: t, afterEmptyLine: n };
}
class du {
  constructor(t = {}) {
    this.doc = null, this.atDirectives = !1, this.prelude = [], this.errors = [], this.warnings = [], this.onError = (s, n, i, r) => {
      const o = ls(s);
      r ? this.warnings.push(new Wf(o, n, i)) : this.errors.push(new us(o, n, i));
    }, this.directives = new be({ version: t.version || "1.2" }), this.options = t;
  }
  decorate(t, s) {
    const { comment: n, afterEmptyLine: i } = Kr(this.prelude);
    if (n) {
      const r = t.contents;
      if (s)
        t.comment = t.comment ? `${t.comment}
${n}` : n;
      else if (i || t.directives.docStart || !r)
        t.commentBefore = n;
      else if (ne(r) && !r.flow && r.items.length > 0) {
        let o = r.items[0];
        re(o) && (o = o.key);
        const l = o.commentBefore;
        o.commentBefore = l ? `${n}
${l}` : n;
      } else {
        const o = r.commentBefore;
        r.commentBefore = o ? `${n}
${o}` : n;
      }
    }
    if (s) {
      for (let r = 0; r < this.errors.length; ++r)
        t.errors.push(this.errors[r]);
      for (let r = 0; r < this.warnings.length; ++r)
        t.warnings.push(this.warnings[r]);
    } else
      t.errors = this.errors, t.warnings = this.warnings;
    this.prelude = [], this.errors = [], this.warnings = [];
  }
  /**
   * Current stream status information.
   *
   * Mostly useful at the end of input for an empty stream.
   */
  streamInfo() {
    return {
      comment: Kr(this.prelude).comment,
      directives: this.directives,
      errors: this.errors,
      warnings: this.warnings
    };
  }
  /**
   * Compose tokens into documents.
   *
   * @param forceDoc - If the stream contains no document, still emit a final document including any comments and directives that would be applied to a subsequent document.
   * @param endOffset - Should be set if `forceDoc` is also set, to set the document range end and to indicate errors correctly.
   */
  *compose(t, s = !1, n = -1) {
    for (const i of t)
      yield* this.next(i);
    yield* this.end(s, n);
  }
  /** Advance the composer by one CST token. */
  *next(t) {
    switch (t.type) {
      case "directive":
        this.directives.add(t.source, (s, n, i) => {
          const r = ls(t);
          r[0] += s, this.onError(r, "BAD_DIRECTIVE", n, i);
        }), this.prelude.push(t.source), this.atDirectives = !0;
        break;
      case "document": {
        const s = hu(this.options, this.directives, t, this.onError);
        this.atDirectives && !s.directives.docStart && this.onError(t, "MISSING_CHAR", "Missing directives-end/doc-start indicator line"), this.decorate(s, !1), this.doc && (yield this.doc), this.doc = s, this.atDirectives = !1;
        break;
      }
      case "byte-order-mark":
      case "space":
        break;
      case "comment":
      case "newline":
        this.prelude.push(t.source);
        break;
      case "error": {
        const s = t.source ? `${t.message}: ${JSON.stringify(t.source)}` : t.message, n = new us(ls(t), "UNEXPECTED_TOKEN", s);
        this.atDirectives || !this.doc ? this.errors.push(n) : this.doc.errors.push(n);
        break;
      }
      case "doc-end": {
        if (!this.doc) {
          const n = "Unexpected doc-end without preceding document";
          this.errors.push(new us(ls(t), "UNEXPECTED_TOKEN", n));
          break;
        }
        this.doc.directives.docEnd = !0;
        const s = Rs(t.end, t.offset + t.source.length, this.doc.options.strict, this.onError);
        if (this.decorate(this.doc, !0), s.comment) {
          const n = this.doc.comment;
          this.doc.comment = n ? `${n}
${s.comment}` : s.comment;
        }
        this.doc.range[2] = s.offset;
        break;
      }
      default:
        this.errors.push(new us(ls(t), "UNEXPECTED_TOKEN", `Unsupported token ${t.type}`));
    }
  }
  /**
   * Call at end of input to yield any remaining document.
   *
   * @param forceDoc - If the stream contains no document, still emit a final document including any comments and directives that would be applied to a subsequent document.
   * @param endOffset - Should be set if `forceDoc` is also set, to set the document range end and to indicate errors correctly.
   */
  *end(t = !1, s = -1) {
    if (this.doc)
      this.decorate(this.doc, !0), yield this.doc, this.doc = null;
    else if (t) {
      const n = Object.assign({ _directives: this.directives }, this.options), i = new Wi(void 0, n);
      this.atDirectives && this.onError(s, "MISSING_CHAR", "Missing directives-end indicator line"), i.range = [0, s, s], this.decorate(i, !1), yield i;
    }
  }
}
const Ml = "\uFEFF", Pl = "", Dl = "", pi = "";
function pu(e) {
  switch (e) {
    case Ml:
      return "byte-order-mark";
    case Pl:
      return "doc-mode";
    case Dl:
      return "flow-error-end";
    case pi:
      return "scalar";
    case "---":
      return "doc-start";
    case "...":
      return "doc-end";
    case "":
    case `
`:
    case `\r
`:
      return "newline";
    case "-":
      return "seq-item-ind";
    case "?":
      return "explicit-key-ind";
    case ":":
      return "map-value-ind";
    case "{":
      return "flow-map-start";
    case "}":
      return "flow-map-end";
    case "[":
      return "flow-seq-start";
    case "]":
      return "flow-seq-end";
    case ",":
      return "comma";
  }
  switch (e[0]) {
    case " ":
    case "	":
      return "space";
    case "#":
      return "comment";
    case "%":
      return "directive-line";
    case "*":
      return "alias";
    case "&":
      return "anchor";
    case "!":
      return "tag";
    case "'":
      return "single-quoted-scalar";
    case '"':
      return "double-quoted-scalar";
    case "|":
    case ">":
      return "block-scalar-header";
  }
  return null;
}
function Me(e) {
  switch (e) {
    case void 0:
    case " ":
    case `
`:
    case "\r":
    case "	":
      return !0;
    default:
      return !1;
  }
}
const xr = new Set("0123456789ABCDEFabcdef"), gu = new Set("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-#;/?:@&=+$_.!~*'()"), Hs = new Set(",[]{}"), mu = new Set(` ,[]{}
\r	`), Xn = (e) => !e || mu.has(e);
class yu {
  constructor() {
    this.atEnd = !1, this.blockScalarIndent = -1, this.blockScalarKeep = !1, this.buffer = "", this.flowKey = !1, this.flowLevel = 0, this.indentNext = 0, this.indentValue = 0, this.lineEndPos = null, this.next = null, this.pos = 0;
  }
  /**
   * Generate YAML tokens from the `source` string. If `incomplete`,
   * a part of the last line may be left as a buffer for the next call.
   *
   * @returns A generator of lexical tokens
   */
  *lex(t, s = !1) {
    if (t) {
      if (typeof t != "string")
        throw TypeError("source is not a string");
      this.buffer = this.buffer ? this.buffer + t : t, this.lineEndPos = null;
    }
    this.atEnd = !s;
    let n = this.next ?? "stream";
    for (; n && (s || this.hasChars(1)); )
      n = yield* this.parseNext(n);
  }
  atLineEnd() {
    let t = this.pos, s = this.buffer[t];
    for (; s === " " || s === "	"; )
      s = this.buffer[++t];
    return !s || s === "#" || s === `
` ? !0 : s === "\r" ? this.buffer[t + 1] === `
` : !1;
  }
  charAt(t) {
    return this.buffer[this.pos + t];
  }
  continueScalar(t) {
    let s = this.buffer[t];
    if (this.indentNext > 0) {
      let n = 0;
      for (; s === " "; )
        s = this.buffer[++n + t];
      if (s === "\r") {
        const i = this.buffer[n + t + 1];
        if (i === `
` || !i && !this.atEnd)
          return t + n + 1;
      }
      return s === `
` || n >= this.indentNext || !s && !this.atEnd ? t + n : -1;
    }
    if (s === "-" || s === ".") {
      const n = this.buffer.substr(t, 3);
      if ((n === "---" || n === "...") && Me(this.buffer[t + 3]))
        return -1;
    }
    return t;
  }
  getLine() {
    let t = this.lineEndPos;
    return (typeof t != "number" || t !== -1 && t < this.pos) && (t = this.buffer.indexOf(`
`, this.pos), this.lineEndPos = t), t === -1 ? this.atEnd ? this.buffer.substring(this.pos) : null : (this.buffer[t - 1] === "\r" && (t -= 1), this.buffer.substring(this.pos, t));
  }
  hasChars(t) {
    return this.pos + t <= this.buffer.length;
  }
  setNext(t) {
    return this.buffer = this.buffer.substring(this.pos), this.pos = 0, this.lineEndPos = null, this.next = t, null;
  }
  peek(t) {
    return this.buffer.substr(this.pos, t);
  }
  *parseNext(t) {
    switch (t) {
      case "stream":
        return yield* this.parseStream();
      case "line-start":
        return yield* this.parseLineStart();
      case "block-start":
        return yield* this.parseBlockStart();
      case "doc":
        return yield* this.parseDocument();
      case "flow":
        return yield* this.parseFlowCollection();
      case "quoted-scalar":
        return yield* this.parseQuotedScalar();
      case "block-scalar":
        return yield* this.parseBlockScalar();
      case "plain-scalar":
        return yield* this.parsePlainScalar();
    }
  }
  *parseStream() {
    let t = this.getLine();
    if (t === null)
      return this.setNext("stream");
    if (t[0] === Ml && (yield* this.pushCount(1), t = t.substring(1)), t[0] === "%") {
      let s = t.length, n = t.indexOf("#");
      for (; n !== -1; ) {
        const r = t[n - 1];
        if (r === " " || r === "	") {
          s = n - 1;
          break;
        } else
          n = t.indexOf("#", n + 1);
      }
      for (; ; ) {
        const r = t[s - 1];
        if (r === " " || r === "	")
          s -= 1;
        else
          break;
      }
      const i = (yield* this.pushCount(s)) + (yield* this.pushSpaces(!0));
      return yield* this.pushCount(t.length - i), this.pushNewline(), "stream";
    }
    if (this.atLineEnd()) {
      const s = yield* this.pushSpaces(!0);
      return yield* this.pushCount(t.length - s), yield* this.pushNewline(), "stream";
    }
    return yield Pl, yield* this.parseLineStart();
  }
  *parseLineStart() {
    const t = this.charAt(0);
    if (!t && !this.atEnd)
      return this.setNext("line-start");
    if (t === "-" || t === ".") {
      if (!this.atEnd && !this.hasChars(4))
        return this.setNext("line-start");
      const s = this.peek(3);
      if ((s === "---" || s === "...") && Me(this.charAt(3)))
        return yield* this.pushCount(3), this.indentValue = 0, this.indentNext = 0, s === "---" ? "doc" : "stream";
    }
    return this.indentValue = yield* this.pushSpaces(!1), this.indentNext > this.indentValue && !Me(this.charAt(1)) && (this.indentNext = this.indentValue), yield* this.parseBlockStart();
  }
  *parseBlockStart() {
    const [t, s] = this.peek(2);
    if (!s && !this.atEnd)
      return this.setNext("block-start");
    if ((t === "-" || t === "?" || t === ":") && Me(s)) {
      const n = (yield* this.pushCount(1)) + (yield* this.pushSpaces(!0));
      return this.indentNext = this.indentValue + 1, this.indentValue += n, "block-start";
    }
    return "doc";
  }
  *parseDocument() {
    yield* this.pushSpaces(!0);
    const t = this.getLine();
    if (t === null)
      return this.setNext("doc");
    let s = yield* this.pushIndicators();
    switch (t[s]) {
      case "#":
        yield* this.pushCount(t.length - s);
      // fallthrough
      case void 0:
        return yield* this.pushNewline(), yield* this.parseLineStart();
      case "{":
      case "[":
        return yield* this.pushCount(1), this.flowKey = !1, this.flowLevel = 1, "flow";
      case "}":
      case "]":
        return yield* this.pushCount(1), "doc";
      case "*":
        return yield* this.pushUntil(Xn), "doc";
      case '"':
      case "'":
        return yield* this.parseQuotedScalar();
      case "|":
      case ">":
        return s += yield* this.parseBlockScalarHeader(), s += yield* this.pushSpaces(!0), yield* this.pushCount(t.length - s), yield* this.pushNewline(), yield* this.parseBlockScalar();
      default:
        return yield* this.parsePlainScalar();
    }
  }
  *parseFlowCollection() {
    let t, s, n = -1;
    do
      t = yield* this.pushNewline(), t > 0 ? (s = yield* this.pushSpaces(!1), this.indentValue = n = s) : s = 0, s += yield* this.pushSpaces(!0);
    while (t + s > 0);
    const i = this.getLine();
    if (i === null)
      return this.setNext("flow");
    if ((n !== -1 && n < this.indentNext && i[0] !== "#" || n === 0 && (i.startsWith("---") || i.startsWith("...")) && Me(i[3])) && !(n === this.indentNext - 1 && this.flowLevel === 1 && (i[0] === "]" || i[0] === "}")))
      return this.flowLevel = 0, yield Dl, yield* this.parseLineStart();
    let r = 0;
    for (; i[r] === ","; )
      r += yield* this.pushCount(1), r += yield* this.pushSpaces(!0), this.flowKey = !1;
    switch (r += yield* this.pushIndicators(), i[r]) {
      case void 0:
        return "flow";
      case "#":
        return yield* this.pushCount(i.length - r), "flow";
      case "{":
      case "[":
        return yield* this.pushCount(1), this.flowKey = !1, this.flowLevel += 1, "flow";
      case "}":
      case "]":
        return yield* this.pushCount(1), this.flowKey = !0, this.flowLevel -= 1, this.flowLevel ? "flow" : "doc";
      case "*":
        return yield* this.pushUntil(Xn), "flow";
      case '"':
      case "'":
        return this.flowKey = !0, yield* this.parseQuotedScalar();
      case ":": {
        const o = this.charAt(1);
        if (this.flowKey || Me(o) || o === ",")
          return this.flowKey = !1, yield* this.pushCount(1), yield* this.pushSpaces(!0), "flow";
      }
      // fallthrough
      default:
        return this.flowKey = !1, yield* this.parsePlainScalar();
    }
  }
  *parseQuotedScalar() {
    const t = this.charAt(0);
    let s = this.buffer.indexOf(t, this.pos + 1);
    if (t === "'")
      for (; s !== -1 && this.buffer[s + 1] === "'"; )
        s = this.buffer.indexOf("'", s + 2);
    else
      for (; s !== -1; ) {
        let r = 0;
        for (; this.buffer[s - 1 - r] === "\\"; )
          r += 1;
        if (r % 2 === 0)
          break;
        s = this.buffer.indexOf('"', s + 1);
      }
    const n = this.buffer.substring(0, s);
    let i = n.indexOf(`
`, this.pos);
    if (i !== -1) {
      for (; i !== -1; ) {
        const r = this.continueScalar(i + 1);
        if (r === -1)
          break;
        i = n.indexOf(`
`, r);
      }
      i !== -1 && (s = i - (n[i - 1] === "\r" ? 2 : 1));
    }
    if (s === -1) {
      if (!this.atEnd)
        return this.setNext("quoted-scalar");
      s = this.buffer.length;
    }
    return yield* this.pushToIndex(s + 1, !1), this.flowLevel ? "flow" : "doc";
  }
  *parseBlockScalarHeader() {
    this.blockScalarIndent = -1, this.blockScalarKeep = !1;
    let t = this.pos;
    for (; ; ) {
      const s = this.buffer[++t];
      if (s === "+")
        this.blockScalarKeep = !0;
      else if (s > "0" && s <= "9")
        this.blockScalarIndent = Number(s) - 1;
      else if (s !== "-")
        break;
    }
    return yield* this.pushUntil((s) => Me(s) || s === "#");
  }
  *parseBlockScalar() {
    let t = this.pos - 1, s = 0, n;
    e: for (let r = this.pos; n = this.buffer[r]; ++r)
      switch (n) {
        case " ":
          s += 1;
          break;
        case `
`:
          t = r, s = 0;
          break;
        case "\r": {
          const o = this.buffer[r + 1];
          if (!o && !this.atEnd)
            return this.setNext("block-scalar");
          if (o === `
`)
            break;
        }
        // fallthrough
        default:
          break e;
      }
    if (!n && !this.atEnd)
      return this.setNext("block-scalar");
    if (s >= this.indentNext) {
      this.blockScalarIndent === -1 ? this.indentNext = s : this.indentNext = this.blockScalarIndent + (this.indentNext === 0 ? 1 : this.indentNext);
      do {
        const r = this.continueScalar(t + 1);
        if (r === -1)
          break;
        t = this.buffer.indexOf(`
`, r);
      } while (t !== -1);
      if (t === -1) {
        if (!this.atEnd)
          return this.setNext("block-scalar");
        t = this.buffer.length;
      }
    }
    let i = t + 1;
    for (n = this.buffer[i]; n === " "; )
      n = this.buffer[++i];
    if (n === "	") {
      for (; n === "	" || n === " " || n === "\r" || n === `
`; )
        n = this.buffer[++i];
      t = i - 1;
    } else if (!this.blockScalarKeep)
      do {
        let r = t - 1, o = this.buffer[r];
        o === "\r" && (o = this.buffer[--r]);
        const l = r;
        for (; o === " "; )
          o = this.buffer[--r];
        if (o === `
` && r >= this.pos && r + 1 + s > l)
          t = r;
        else
          break;
      } while (!0);
    return yield pi, yield* this.pushToIndex(t + 1, !0), yield* this.parseLineStart();
  }
  *parsePlainScalar() {
    const t = this.flowLevel > 0;
    let s = this.pos - 1, n = this.pos - 1, i;
    for (; i = this.buffer[++n]; )
      if (i === ":") {
        const r = this.buffer[n + 1];
        if (Me(r) || t && Hs.has(r))
          break;
        s = n;
      } else if (Me(i)) {
        let r = this.buffer[n + 1];
        if (i === "\r" && (r === `
` ? (n += 1, i = `
`, r = this.buffer[n + 1]) : s = n), r === "#" || t && Hs.has(r))
          break;
        if (i === `
`) {
          const o = this.continueScalar(n + 1);
          if (o === -1)
            break;
          n = Math.max(n, o - 2);
        }
      } else {
        if (t && Hs.has(i))
          break;
        s = n;
      }
    return !i && !this.atEnd ? this.setNext("plain-scalar") : (yield pi, yield* this.pushToIndex(s + 1, !0), t ? "flow" : "doc");
  }
  *pushCount(t) {
    return t > 0 ? (yield this.buffer.substr(this.pos, t), this.pos += t, t) : 0;
  }
  *pushToIndex(t, s) {
    const n = this.buffer.slice(this.pos, t);
    return n ? (yield n, this.pos += n.length, n.length) : (s && (yield ""), 0);
  }
  *pushIndicators() {
    let t = 0;
    e: for (; ; ) {
      switch (this.charAt(0)) {
        case "!":
          t += yield* this.pushTag(), t += yield* this.pushSpaces(!0);
          continue e;
        case "&":
          t += yield* this.pushUntil(Xn), t += yield* this.pushSpaces(!0);
          continue e;
        case "-":
        // this is an error
        case "?":
        // this is an error outside flow collections
        case ":": {
          const s = this.flowLevel > 0, n = this.charAt(1);
          if (Me(n) || s && Hs.has(n)) {
            s ? this.flowKey && (this.flowKey = !1) : this.indentNext = this.indentValue + 1, t += yield* this.pushCount(1), t += yield* this.pushSpaces(!0);
            continue e;
          }
        }
      }
      break e;
    }
    return t;
  }
  *pushTag() {
    if (this.charAt(1) === "<") {
      let t = this.pos + 2, s = this.buffer[t];
      for (; !Me(s) && s !== ">"; )
        s = this.buffer[++t];
      return yield* this.pushToIndex(s === ">" ? t + 1 : t, !1);
    } else {
      let t = this.pos + 1, s = this.buffer[t];
      for (; s; )
        if (gu.has(s))
          s = this.buffer[++t];
        else if (s === "%" && xr.has(this.buffer[t + 1]) && xr.has(this.buffer[t + 2]))
          s = this.buffer[t += 3];
        else
          break;
      return yield* this.pushToIndex(t, !1);
    }
  }
  *pushNewline() {
    const t = this.buffer[this.pos];
    return t === `
` ? yield* this.pushCount(1) : t === "\r" && this.charAt(1) === `
` ? yield* this.pushCount(2) : 0;
  }
  *pushSpaces(t) {
    let s = this.pos - 1, n;
    do
      n = this.buffer[++s];
    while (n === " " || t && n === "	");
    const i = s - this.pos;
    return i > 0 && (yield this.buffer.substr(this.pos, i), this.pos = s), i;
  }
  *pushUntil(t) {
    let s = this.pos, n = this.buffer[s];
    for (; !t(n); )
      n = this.buffer[++s];
    return yield* this.pushToIndex(s, !1);
  }
}
class bu {
  constructor() {
    this.lineStarts = [], this.addNewLine = (t) => this.lineStarts.push(t), this.linePos = (t) => {
      let s = 0, n = this.lineStarts.length;
      for (; s < n; ) {
        const r = s + n >> 1;
        this.lineStarts[r] < t ? s = r + 1 : n = r;
      }
      if (this.lineStarts[s] === t)
        return { line: s + 1, col: 1 };
      if (s === 0)
        return { line: 0, col: t };
      const i = this.lineStarts[s - 1];
      return { line: s, col: t - i + 1 };
    };
  }
}
function ht(e, t) {
  for (let s = 0; s < e.length; ++s)
    if (e[s].type === t)
      return !0;
  return !1;
}
function Ur(e) {
  for (let t = 0; t < e.length; ++t)
    switch (e[t].type) {
      case "space":
      case "comment":
      case "newline":
        break;
      default:
        return t;
    }
  return -1;
}
function Rl(e) {
  switch (e == null ? void 0 : e.type) {
    case "alias":
    case "scalar":
    case "single-quoted-scalar":
    case "double-quoted-scalar":
    case "flow-collection":
      return !0;
    default:
      return !1;
  }
}
function Ws(e) {
  switch (e.type) {
    case "document":
      return e.start;
    case "block-map": {
      const t = e.items[e.items.length - 1];
      return t.sep ?? t.start;
    }
    case "block-seq":
      return e.items[e.items.length - 1].start;
    /* istanbul ignore next should not happen */
    default:
      return [];
  }
}
function Lt(e) {
  var s;
  if (e.length === 0)
    return [];
  let t = e.length;
  e: for (; --t >= 0; )
    switch (e[t].type) {
      case "doc-start":
      case "explicit-key-ind":
      case "map-value-ind":
      case "seq-item-ind":
      case "newline":
        break e;
    }
  for (; ((s = e[++t]) == null ? void 0 : s.type) === "space"; )
    ;
  return e.splice(t, e.length);
}
function an(e, t) {
  if (t.length < 1e5)
    Array.prototype.push.apply(e, t);
  else
    for (let s = 0; s < t.length; ++s)
      e.push(t[s]);
}
function Vr(e) {
  if (e.start.type === "flow-seq-start")
    for (const t of e.items)
      t.sep && !t.value && !ht(t.start, "explicit-key-ind") && !ht(t.sep, "map-value-ind") && (t.key && (t.value = t.key), delete t.key, Rl(t.value) ? t.value.end ? an(t.value.end, t.sep) : t.value.end = t.sep : an(t.start, t.sep), delete t.sep);
}
class wu {
  /**
   * @param onNewLine - If defined, called separately with the start position of
   *   each new line (in `parse()`, including the start of input).
   */
  constructor(t) {
    this.atNewLine = !0, this.atScalar = !1, this.indent = 0, this.offset = 0, this.onKeyLine = !1, this.stack = [], this.source = "", this.type = "", this.lexer = new yu(), this.onNewLine = t;
  }
  /**
   * Parse `source` as a YAML stream.
   * If `incomplete`, a part of the last line may be left as a buffer for the next call.
   *
   * Errors are not thrown, but yielded as `{ type: 'error', message }` tokens.
   *
   * @returns A generator of tokens representing each directive, document, and other structure.
   */
  *parse(t, s = !1) {
    this.onNewLine && this.offset === 0 && this.onNewLine(0);
    for (const n of this.lexer.lex(t, s))
      yield* this.next(n);
    s || (yield* this.end());
  }
  /**
   * Advance the parser by the `source` of one lexical token.
   */
  *next(t) {
    if (this.source = t, this.atScalar) {
      this.atScalar = !1, yield* this.step(), this.offset += t.length;
      return;
    }
    const s = pu(t);
    if (s)
      if (s === "scalar")
        this.atNewLine = !1, this.atScalar = !0, this.type = "scalar";
      else {
        switch (this.type = s, yield* this.step(), s) {
          case "newline":
            this.atNewLine = !0, this.indent = 0, this.onNewLine && this.onNewLine(this.offset + t.length);
            break;
          case "space":
            this.atNewLine && t[0] === " " && (this.indent += t.length);
            break;
          case "explicit-key-ind":
          case "map-value-ind":
          case "seq-item-ind":
            this.atNewLine && (this.indent += t.length);
            break;
          case "doc-mode":
          case "flow-error-end":
            return;
          default:
            this.atNewLine = !1;
        }
        this.offset += t.length;
      }
    else {
      const n = `Not a YAML token: ${t}`;
      yield* this.pop({ type: "error", offset: this.offset, message: n, source: t }), this.offset += t.length;
    }
  }
  /** Call at end of input to push out any remaining constructions */
  *end() {
    for (; this.stack.length > 0; )
      yield* this.pop();
  }
  get sourceToken() {
    return {
      type: this.type,
      offset: this.offset,
      indent: this.indent,
      source: this.source
    };
  }
  *step() {
    const t = this.peek(1);
    if (this.type === "doc-end" && (t == null ? void 0 : t.type) !== "doc-end") {
      for (; this.stack.length > 0; )
        yield* this.pop();
      this.stack.push({
        type: "doc-end",
        offset: this.offset,
        source: this.source
      });
      return;
    }
    if (!t)
      return yield* this.stream();
    switch (t.type) {
      case "document":
        return yield* this.document(t);
      case "alias":
      case "scalar":
      case "single-quoted-scalar":
      case "double-quoted-scalar":
        return yield* this.scalar(t);
      case "block-scalar":
        return yield* this.blockScalar(t);
      case "block-map":
        return yield* this.blockMap(t);
      case "block-seq":
        return yield* this.blockSequence(t);
      case "flow-collection":
        return yield* this.flowCollection(t);
      case "doc-end":
        return yield* this.documentEnd(t);
    }
    yield* this.pop();
  }
  peek(t) {
    return this.stack[this.stack.length - t];
  }
  *pop(t) {
    const s = t ?? this.stack.pop();
    if (!s)
      yield { type: "error", offset: this.offset, source: "", message: "Tried to pop an empty stack" };
    else if (this.stack.length === 0)
      yield s;
    else {
      const n = this.peek(1);
      switch (s.type === "block-scalar" ? s.indent = "indent" in n ? n.indent : 0 : s.type === "flow-collection" && n.type === "document" && (s.indent = 0), s.type === "flow-collection" && Vr(s), n.type) {
        case "document":
          n.value = s;
          break;
        case "block-scalar":
          n.props.push(s);
          break;
        case "block-map": {
          const i = n.items[n.items.length - 1];
          if (i.value) {
            n.items.push({ start: [], key: s, sep: [] }), this.onKeyLine = !0;
            return;
          } else if (i.sep)
            i.value = s;
          else {
            Object.assign(i, { key: s, sep: [] }), this.onKeyLine = !i.explicitKey;
            return;
          }
          break;
        }
        case "block-seq": {
          const i = n.items[n.items.length - 1];
          i.value ? n.items.push({ start: [], value: s }) : i.value = s;
          break;
        }
        case "flow-collection": {
          const i = n.items[n.items.length - 1];
          !i || i.value ? n.items.push({ start: [], key: s, sep: [] }) : i.sep ? i.value = s : Object.assign(i, { key: s, sep: [] });
          return;
        }
        /* istanbul ignore next should not happen */
        default:
          yield* this.pop(), yield* this.pop(s);
      }
      if ((n.type === "document" || n.type === "block-map" || n.type === "block-seq") && (s.type === "block-map" || s.type === "block-seq")) {
        const i = s.items[s.items.length - 1];
        i && !i.sep && !i.value && i.start.length > 0 && Ur(i.start) === -1 && (s.indent === 0 || i.start.every((r) => r.type !== "comment" || r.indent < s.indent)) && (n.type === "document" ? n.end = i.start : n.items.push({ start: i.start }), s.items.splice(-1, 1));
      }
    }
  }
  *stream() {
    switch (this.type) {
      case "directive-line":
        yield { type: "directive", offset: this.offset, source: this.source };
        return;
      case "byte-order-mark":
      case "space":
      case "comment":
      case "newline":
        yield this.sourceToken;
        return;
      case "doc-mode":
      case "doc-start": {
        const t = {
          type: "document",
          offset: this.offset,
          start: []
        };
        this.type === "doc-start" && t.start.push(this.sourceToken), this.stack.push(t);
        return;
      }
    }
    yield {
      type: "error",
      offset: this.offset,
      message: `Unexpected ${this.type} token in YAML stream`,
      source: this.source
    };
  }
  *document(t) {
    if (t.value)
      return yield* this.lineEnd(t);
    switch (this.type) {
      case "doc-start": {
        Ur(t.start) !== -1 ? (yield* this.pop(), yield* this.step()) : t.start.push(this.sourceToken);
        return;
      }
      case "anchor":
      case "tag":
      case "space":
      case "comment":
      case "newline":
        t.start.push(this.sourceToken);
        return;
    }
    const s = this.startBlockValue(t);
    s ? this.stack.push(s) : yield {
      type: "error",
      offset: this.offset,
      message: `Unexpected ${this.type} token in YAML document`,
      source: this.source
    };
  }
  *scalar(t) {
    if (this.type === "map-value-ind") {
      const s = Ws(this.peek(2)), n = Lt(s);
      let i;
      t.end ? (i = t.end, i.push(this.sourceToken), delete t.end) : i = [this.sourceToken];
      const r = {
        type: "block-map",
        offset: t.offset,
        indent: t.indent,
        items: [{ start: n, key: t, sep: i }]
      };
      this.onKeyLine = !0, this.stack[this.stack.length - 1] = r;
    } else
      yield* this.lineEnd(t);
  }
  *blockScalar(t) {
    switch (this.type) {
      case "space":
      case "comment":
      case "newline":
        t.props.push(this.sourceToken);
        return;
      case "scalar":
        if (t.source = this.source, this.atNewLine = !0, this.indent = 0, this.onNewLine) {
          let s = this.source.indexOf(`
`) + 1;
          for (; s !== 0; )
            this.onNewLine(this.offset + s), s = this.source.indexOf(`
`, s) + 1;
        }
        yield* this.pop();
        break;
      /* istanbul ignore next should not happen */
      default:
        yield* this.pop(), yield* this.step();
    }
  }
  *blockMap(t) {
    var n;
    const s = t.items[t.items.length - 1];
    switch (this.type) {
      case "newline":
        if (this.onKeyLine = !1, s.value) {
          const i = "end" in s.value ? s.value.end : void 0, r = Array.isArray(i) ? i[i.length - 1] : void 0;
          (r == null ? void 0 : r.type) === "comment" ? i == null || i.push(this.sourceToken) : t.items.push({ start: [this.sourceToken] });
        } else s.sep ? s.sep.push(this.sourceToken) : s.start.push(this.sourceToken);
        return;
      case "space":
      case "comment":
        if (s.value)
          t.items.push({ start: [this.sourceToken] });
        else if (s.sep)
          s.sep.push(this.sourceToken);
        else {
          if (this.atIndentedComment(s.start, t.indent)) {
            const i = t.items[t.items.length - 2], r = (n = i == null ? void 0 : i.value) == null ? void 0 : n.end;
            if (Array.isArray(r)) {
              an(r, s.start), r.push(this.sourceToken), t.items.pop();
              return;
            }
          }
          s.start.push(this.sourceToken);
        }
        return;
    }
    if (this.indent >= t.indent) {
      const i = !this.onKeyLine && this.indent === t.indent, r = i && (s.sep || s.explicitKey) && this.type !== "seq-item-ind";
      let o = [];
      if (r && s.sep && !s.value) {
        const l = [];
        for (let c = 0; c < s.sep.length; ++c) {
          const a = s.sep[c];
          switch (a.type) {
            case "newline":
              l.push(c);
              break;
            case "space":
              break;
            case "comment":
              a.indent > t.indent && (l.length = 0);
              break;
            default:
              l.length = 0;
          }
        }
        l.length >= 2 && (o = s.sep.splice(l[1]));
      }
      switch (this.type) {
        case "anchor":
        case "tag":
          r || s.value ? (o.push(this.sourceToken), t.items.push({ start: o }), this.onKeyLine = !0) : s.sep ? s.sep.push(this.sourceToken) : s.start.push(this.sourceToken);
          return;
        case "explicit-key-ind":
          !s.sep && !s.explicitKey ? (s.start.push(this.sourceToken), s.explicitKey = !0) : r || s.value ? (o.push(this.sourceToken), t.items.push({ start: o, explicitKey: !0 })) : this.stack.push({
            type: "block-map",
            offset: this.offset,
            indent: this.indent,
            items: [{ start: [this.sourceToken], explicitKey: !0 }]
          }), this.onKeyLine = !0;
          return;
        case "map-value-ind":
          if (s.explicitKey)
            if (s.sep)
              if (s.value)
                t.items.push({ start: [], key: null, sep: [this.sourceToken] });
              else if (ht(s.sep, "map-value-ind"))
                this.stack.push({
                  type: "block-map",
                  offset: this.offset,
                  indent: this.indent,
                  items: [{ start: o, key: null, sep: [this.sourceToken] }]
                });
              else if (Rl(s.key) && !ht(s.sep, "newline")) {
                const l = Lt(s.start), c = s.key, a = s.sep;
                a.push(this.sourceToken), delete s.key, delete s.sep, this.stack.push({
                  type: "block-map",
                  offset: this.offset,
                  indent: this.indent,
                  items: [{ start: l, key: c, sep: a }]
                });
              } else o.length > 0 ? s.sep = s.sep.concat(o, this.sourceToken) : s.sep.push(this.sourceToken);
            else if (ht(s.start, "newline"))
              Object.assign(s, { key: null, sep: [this.sourceToken] });
            else {
              const l = Lt(s.start);
              this.stack.push({
                type: "block-map",
                offset: this.offset,
                indent: this.indent,
                items: [{ start: l, key: null, sep: [this.sourceToken] }]
              });
            }
          else
            s.sep ? s.value || r ? t.items.push({ start: o, key: null, sep: [this.sourceToken] }) : ht(s.sep, "map-value-ind") ? this.stack.push({
              type: "block-map",
              offset: this.offset,
              indent: this.indent,
              items: [{ start: [], key: null, sep: [this.sourceToken] }]
            }) : s.sep.push(this.sourceToken) : Object.assign(s, { key: null, sep: [this.sourceToken] });
          this.onKeyLine = !0;
          return;
        case "alias":
        case "scalar":
        case "single-quoted-scalar":
        case "double-quoted-scalar": {
          const l = this.flowScalar(this.type);
          r || s.value ? (t.items.push({ start: o, key: l, sep: [] }), this.onKeyLine = !0) : s.sep ? this.stack.push(l) : (Object.assign(s, { key: l, sep: [] }), this.onKeyLine = !0);
          return;
        }
        default: {
          const l = this.startBlockValue(t);
          if (l) {
            if (l.type === "block-seq") {
              if (!s.explicitKey && s.sep && !ht(s.sep, "newline")) {
                yield* this.pop({
                  type: "error",
                  offset: this.offset,
                  message: "Unexpected block-seq-ind on same line with key",
                  source: this.source
                });
                return;
              }
            } else i && t.items.push({ start: o });
            this.stack.push(l);
            return;
          }
        }
      }
    }
    yield* this.pop(), yield* this.step();
  }
  *blockSequence(t) {
    var n;
    const s = t.items[t.items.length - 1];
    switch (this.type) {
      case "newline":
        if (s.value) {
          const i = "end" in s.value ? s.value.end : void 0, r = Array.isArray(i) ? i[i.length - 1] : void 0;
          (r == null ? void 0 : r.type) === "comment" ? i == null || i.push(this.sourceToken) : t.items.push({ start: [this.sourceToken] });
        } else
          s.start.push(this.sourceToken);
        return;
      case "space":
      case "comment":
        if (s.value)
          t.items.push({ start: [this.sourceToken] });
        else {
          if (this.atIndentedComment(s.start, t.indent)) {
            const i = t.items[t.items.length - 2], r = (n = i == null ? void 0 : i.value) == null ? void 0 : n.end;
            if (Array.isArray(r)) {
              an(r, s.start), r.push(this.sourceToken), t.items.pop();
              return;
            }
          }
          s.start.push(this.sourceToken);
        }
        return;
      case "anchor":
      case "tag":
        if (s.value || this.indent <= t.indent)
          break;
        s.start.push(this.sourceToken);
        return;
      case "seq-item-ind":
        if (this.indent !== t.indent)
          break;
        s.value || ht(s.start, "seq-item-ind") ? t.items.push({ start: [this.sourceToken] }) : s.start.push(this.sourceToken);
        return;
    }
    if (this.indent > t.indent) {
      const i = this.startBlockValue(t);
      if (i) {
        this.stack.push(i);
        return;
      }
    }
    yield* this.pop(), yield* this.step();
  }
  *flowCollection(t) {
    const s = t.items[t.items.length - 1];
    if (this.type === "flow-error-end") {
      let n;
      do
        yield* this.pop(), n = this.peek(1);
      while ((n == null ? void 0 : n.type) === "flow-collection");
    } else if (t.end.length === 0) {
      switch (this.type) {
        case "comma":
        case "explicit-key-ind":
          !s || s.sep ? t.items.push({ start: [this.sourceToken] }) : s.start.push(this.sourceToken);
          return;
        case "map-value-ind":
          !s || s.value ? t.items.push({ start: [], key: null, sep: [this.sourceToken] }) : s.sep ? s.sep.push(this.sourceToken) : Object.assign(s, { key: null, sep: [this.sourceToken] });
          return;
        case "space":
        case "comment":
        case "newline":
        case "anchor":
        case "tag":
          !s || s.value ? t.items.push({ start: [this.sourceToken] }) : s.sep ? s.sep.push(this.sourceToken) : s.start.push(this.sourceToken);
          return;
        case "alias":
        case "scalar":
        case "single-quoted-scalar":
        case "double-quoted-scalar": {
          const i = this.flowScalar(this.type);
          !s || s.value ? t.items.push({ start: [], key: i, sep: [] }) : s.sep ? this.stack.push(i) : Object.assign(s, { key: i, sep: [] });
          return;
        }
        case "flow-map-end":
        case "flow-seq-end":
          t.end.push(this.sourceToken);
          return;
      }
      const n = this.startBlockValue(t);
      n ? this.stack.push(n) : (yield* this.pop(), yield* this.step());
    } else {
      const n = this.peek(2);
      if (n.type === "block-map" && (this.type === "map-value-ind" && n.indent === t.indent || this.type === "newline" && !n.items[n.items.length - 1].sep))
        yield* this.pop(), yield* this.step();
      else if (this.type === "map-value-ind" && n.type !== "flow-collection") {
        const i = Ws(n), r = Lt(i);
        Vr(t);
        const o = t.end.splice(1, t.end.length);
        o.push(this.sourceToken);
        const l = {
          type: "block-map",
          offset: t.offset,
          indent: t.indent,
          items: [{ start: r, key: t, sep: o }]
        };
        this.onKeyLine = !0, this.stack[this.stack.length - 1] = l;
      } else
        yield* this.lineEnd(t);
    }
  }
  flowScalar(t) {
    if (this.onNewLine) {
      let s = this.source.indexOf(`
`) + 1;
      for (; s !== 0; )
        this.onNewLine(this.offset + s), s = this.source.indexOf(`
`, s) + 1;
    }
    return {
      type: t,
      offset: this.offset,
      indent: this.indent,
      source: this.source
    };
  }
  startBlockValue(t) {
    switch (this.type) {
      case "alias":
      case "scalar":
      case "single-quoted-scalar":
      case "double-quoted-scalar":
        return this.flowScalar(this.type);
      case "block-scalar-header":
        return {
          type: "block-scalar",
          offset: this.offset,
          indent: this.indent,
          props: [this.sourceToken],
          source: ""
        };
      case "flow-map-start":
      case "flow-seq-start":
        return {
          type: "flow-collection",
          offset: this.offset,
          indent: this.indent,
          start: this.sourceToken,
          items: [],
          end: []
        };
      case "seq-item-ind":
        return {
          type: "block-seq",
          offset: this.offset,
          indent: this.indent,
          items: [{ start: [this.sourceToken] }]
        };
      case "explicit-key-ind": {
        this.onKeyLine = !0;
        const s = Ws(t), n = Lt(s);
        return n.push(this.sourceToken), {
          type: "block-map",
          offset: this.offset,
          indent: this.indent,
          items: [{ start: n, explicitKey: !0 }]
        };
      }
      case "map-value-ind": {
        this.onKeyLine = !0;
        const s = Ws(t), n = Lt(s);
        return {
          type: "block-map",
          offset: this.offset,
          indent: this.indent,
          items: [{ start: n, key: null, sep: [this.sourceToken] }]
        };
      }
    }
    return null;
  }
  atIndentedComment(t, s) {
    return this.type !== "comment" || this.indent <= s ? !1 : t.every((n) => n.type === "newline" || n.type === "space");
  }
  *documentEnd(t) {
    this.type !== "doc-mode" && (t.end ? t.end.push(this.sourceToken) : t.end = [this.sourceToken], this.type === "newline" && (yield* this.pop()));
  }
  *lineEnd(t) {
    switch (this.type) {
      case "comma":
      case "doc-start":
      case "doc-end":
      case "flow-seq-end":
      case "flow-map-end":
      case "map-value-ind":
        yield* this.pop(), yield* this.step();
        break;
      case "newline":
        this.onKeyLine = !1;
      // fallthrough
      case "space":
      case "comment":
      default:
        t.end ? t.end.push(this.sourceToken) : t.end = [this.sourceToken], this.type === "newline" && (yield* this.pop());
    }
  }
}
function Su(e) {
  const t = e.prettyErrors !== !1;
  return { lineCounter: e.lineCounter || t && new bu() || null, prettyErrors: t };
}
function _u(e, t = {}) {
  const { lineCounter: s, prettyErrors: n } = Su(t), i = new wu(s == null ? void 0 : s.addNewLine), r = new du(t);
  let o = null;
  for (const l of r.compose(i.parse(e), !0, e.length))
    if (!o)
      o = l;
    else if (o.options.logLevel !== "silent") {
      o.errors.push(new us(l.range.slice(0, 2), "MULTIPLE_DOCS", "Source contains multiple documents; please use YAML.parseAllDocuments()"));
      break;
    }
  return n && s && (o.errors.forEach(Br(e, s)), o.warnings.forEach(Br(e, s))), o;
}
function vu(e, t, s) {
  let n;
  const i = _u(e, s);
  if (!i)
    return null;
  if (i.warnings.forEach((r) => ll(i.options.logLevel, r)), i.errors.length > 0) {
    if (i.options.logLevel !== "silent")
      throw i.errors[0];
    i.errors = [];
  }
  return i.toJS(Object.assign({ reviver: n }, s));
}
function ku(e, t, s) {
  let n = null;
  if (Array.isArray(t) && (n = t), e === void 0) {
    const { keepUndefined: i } = {};
    if (!i)
      return;
  }
  return $s(e) && !n ? e.toString(s) : new Wi(e, n, s).toString(s);
}
const Ou = { class: "sql-set" }, Tu = { class: "row" }, Nu = ["value"], Au = {
  key: 0,
  class: "muted empty"
}, Eu = { class: "row spread" }, Cu = { class: "row" }, Iu = ["onUpdate:modelValue"], Lu = ["onUpdate:modelValue"], $u = ["onClick"], Mu = {
  key: 0,
  class: "row"
}, Pu = ["onUpdate:modelValue"], Du = ["onUpdate:modelValue"], Ru = {
  key: 1,
  class: "row"
}, ju = ["onUpdate:modelValue"], Bu = { class: "row" }, Fu = {
  key: 0,
  class: "chk"
}, Ku = ["onUpdate:modelValue"], xu = { class: "row" }, Uu = ["onUpdate:modelValue"], Vu = ["value"], qu = ["onClick"], Hu = {
  key: 0,
  class: "row new-cred"
}, Wu = ["onUpdate:modelValue"], Ju = ["onUpdate:modelValue"], Yu = ["onUpdate:modelValue"], Gu = ["disabled", "onClick"], Qu = { class: "muted" }, Xu = { class: "row" }, zu = ["onUpdate:modelValue", "placeholder"], Zu = { class: "row" }, eh = ["onUpdate:modelValue"], th = { class: "row" }, sh = ["disabled", "onClick"], nh = { class: "muted" }, ih = /* @__PURE__ */ Dc({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function i(b, y) {
      return {
        key: n++,
        name: b,
        provider: y.provider || "mssql",
        path: y.provider === "sqlite" ? y.file || "" : y.server || "",
        database: y.database || "",
        user: y.user || "",
        credential: y.credential || "",
        trustedConnection: y.trusted_connection ?? !0,
        description: y.description || "",
        newCred: !1,
        credName: "",
        credUser: "",
        credPassword: "",
        credStatus: "",
        testing: !1,
        testStatus: ""
      };
    }
    function r(b) {
      return {
        provider: b.provider,
        server: b.provider === "sqlite" ? void 0 : b.path || void 0,
        file: b.provider === "sqlite" && b.path || void 0,
        database: b.database || void 0,
        user: b.user || void 0,
        credential: b.credential || void 0,
        trusted_connection: b.provider === "mssql" ? b.trustedConnection : void 0,
        description: b.description || void 0
        // NOTE: no `password` — literals are written to the secret store via secret.set, never here.
      };
    }
    const o = (() => {
      try {
        return vu(s.api.getYaml() || "") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ Rn(o.default_connection || ""), c = /* @__PURE__ */ Rn(o.default_limit || 10), a = /* @__PURE__ */ mn(
      Object.entries(o.connections || {}).map(([b, y]) => i(b, y))
    ), f = /* @__PURE__ */ Rn([]);
    async function u() {
      try {
        const b = await s.api.invoke("secret.list");
        f.value = [...new Set([...b.machine || [], ...b.project || []].map((y) => y.key))].sort();
      } catch {
      }
    }
    No(u);
    function g() {
      a.push(i(`db${a.length + 1}`, { provider: "mssql" }));
    }
    async function p(b) {
      b.credStatus = "Saving…";
      try {
        const y = { password: b.credPassword };
        b.credUser && (y.user = b.credUser), await s.api.invoke("secret.set", { key: b.credName.trim(), fields: y, scope: "machine" }), b.credential = b.credName.trim(), b.newCred = !1, b.credName = "", b.credUser = "", b.credPassword = "", b.credStatus = "", await u();
      } catch (y) {
        b.credStatus = "Failed: " + (y instanceof Error ? y.message : String(y));
      }
    }
    async function k(b) {
      b.testing = !0, b.testStatus = "Connecting...";
      try {
        const y = await s.api.invoke("plugin.action", {
          pluginId: "sql",
          action: "testConnection",
          valueJson: JSON.stringify(m(b))
        });
        if (y.ok && y.resultJson) {
          const S = JSON.parse(y.resultJson);
          b.testStatus = S.message;
        } else
          b.testStatus = "Failed: " + (y.error || "unknown error");
      } catch (y) {
        b.testStatus = "Failed: " + (y instanceof Error ? y.message : String(y));
      } finally {
        b.testing = !1;
      }
    }
    function m(b) {
      const y = r(b);
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
    function _() {
      const b = {
        default_connection: l.value || void 0,
        default_limit: c.value || 10,
        connections: Object.fromEntries(
          a.filter((y) => y.name.trim()).map((y) => [y.name.trim(), r(y)])
        )
      };
      return ku(b);
    }
    return t({ toYaml: _ }), (b, y) => (ye(), ke("div", Ou, [
      y[17] || (y[17] = R("div", { class: "muted" }, " Named database connections available to the SQL agent. Passwords live in the secret store (Settings → Secrets); a connection only references an entry by name. Stored in the .spla project file. ", -1)),
      R("div", Tu, [
        R("label", null, [
          y[3] || (y[3] = R("span", { class: "muted" }, "Default connection", -1)),
          ve(R("select", {
            "onUpdate:modelValue": y[0] || (y[0] = (S) => l.value = S)
          }, [
            y[2] || (y[2] = R("option", { value: "" }, "(none)", -1)),
            (ye(!0), ke(Te, null, Fn(a, (S) => (ye(), ke("option", {
              key: S.key,
              value: S.name
            }, $t(S.name), 9, Nu))), 128))
          ], 512), [
            [Hn, l.value]
          ])
        ]),
        R("label", null, [
          y[4] || (y[4] = R("span", { class: "muted" }, "Default row limit", -1)),
          ve(R("input", {
            "onUpdate:modelValue": y[1] || (y[1] = (S) => c.value = S),
            type: "number",
            min: "1",
            class: "w-90"
          }, null, 512), [
            [
              qe,
              c.value,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      R("button", {
        type: "button",
        class: "self-start",
        onClick: g
      }, "+ Add Connection"),
      a.length ? os("", !0) : (ye(), ke("div", Au, 'No connections yet. Click "+ Add Connection".')),
      (ye(!0), ke(Te, null, Fn(a, (S, A) => (ye(), ke("div", {
        key: S.key,
        class: "conn-card"
      }, [
        R("div", Eu, [
          R("div", Cu, [
            y[6] || (y[6] = R("span", { class: "muted" }, "Name", -1)),
            ve(R("input", {
              "onUpdate:modelValue": (N) => S.name = N,
              class: "w-140",
              spellcheck: "false"
            }, null, 8, Iu), [
              [qe, S.name]
            ]),
            y[7] || (y[7] = R("span", { class: "muted" }, "Provider", -1)),
            ve(R("select", {
              "onUpdate:modelValue": (N) => S.provider = N
            }, [...y[5] || (y[5] = [
              R("option", { value: "mssql" }, "mssql", -1),
              R("option", { value: "postgres" }, "postgres", -1),
              R("option", { value: "sqlite" }, "sqlite", -1)
            ])], 8, Lu), [
              [Hn, S.provider]
            ])
          ]),
          R("button", {
            type: "button",
            onClick: (N) => a.splice(A, 1)
          }, "✕ Remove", 8, $u)
        ]),
        S.provider !== "sqlite" ? (ye(), ke("div", Mu, [
          y[8] || (y[8] = R("span", { class: "muted w-70" }, "Server", -1)),
          ve(R("input", {
            "onUpdate:modelValue": (N) => S.path = N,
            placeholder: "sql01 or 192.168.1.10",
            class: "w-220",
            spellcheck: "false"
          }, null, 8, Pu), [
            [qe, S.path]
          ]),
          y[9] || (y[9] = R("span", { class: "muted w-70" }, "Database", -1)),
          ve(R("input", {
            "onUpdate:modelValue": (N) => S.database = N,
            class: "w-160",
            spellcheck: "false"
          }, null, 8, Du), [
            [qe, S.database]
          ])
        ])) : (ye(), ke("div", Ru, [
          y[10] || (y[10] = R("span", { class: "muted w-70" }, "File", -1)),
          ve(R("input", {
            "onUpdate:modelValue": (N) => S.path = N,
            placeholder: "C:\\data\\mydb.sqlite",
            class: "w-400",
            spellcheck: "false"
          }, null, 8, ju), [
            [qe, S.path]
          ])
        ])),
        S.provider !== "sqlite" ? (ye(), ke(Te, { key: 2 }, [
          R("div", Bu, [
            S.provider === "mssql" ? (ye(), ke("label", Fu, [
              ve(R("input", {
                type: "checkbox",
                "onUpdate:modelValue": (N) => S.trustedConnection = N
              }, null, 8, Ku), [
                [lf, S.trustedConnection]
              ]),
              y[11] || (y[11] = R("span", null, "Windows Auth (domain)", -1))
            ])) : os("", !0)
          ]),
          !S.trustedConnection || S.provider !== "mssql" ? (ye(), ke(Te, { key: 0 }, [
            R("div", xu, [
              y[13] || (y[13] = R("span", { class: "muted w-70" }, "Credential", -1)),
              ve(R("select", {
                "onUpdate:modelValue": (N) => S.credential = N
              }, [
                y[12] || (y[12] = R("option", { value: "" }, "(none — use fields below)", -1)),
                (ye(!0), ke(Te, null, Fn(f.value, (N) => (ye(), ke("option", {
                  key: N,
                  value: N
                }, $t(N), 9, Vu))), 128))
              ], 8, Uu), [
                [Hn, S.credential]
              ]),
              R("button", {
                type: "button",
                onClick: (N) => S.newCred = !S.newCred
              }, $t(S.newCred ? "cancel" : "new…"), 9, qu),
              y[14] || (y[14] = R("span", { class: "muted" }, "entry in the secret store: user + password", -1))
            ]),
            S.newCred ? (ye(), ke("div", Hu, [
              ve(R("input", {
                "onUpdate:modelValue": (N) => S.credName = N,
                placeholder: "entry name",
                class: "w-140",
                spellcheck: "false"
              }, null, 8, Wu), [
                [qe, S.credName]
              ]),
              ve(R("input", {
                "onUpdate:modelValue": (N) => S.credUser = N,
                placeholder: "user",
                class: "w-120",
                spellcheck: "false"
              }, null, 8, Ju), [
                [qe, S.credUser]
              ]),
              ve(R("input", {
                "onUpdate:modelValue": (N) => S.credPassword = N,
                type: "password",
                placeholder: "password",
                class: "w-140",
                autocomplete: "new-password"
              }, null, 8, Yu), [
                [qe, S.credPassword]
              ]),
              R("button", {
                type: "button",
                disabled: !S.credName || !S.credPassword,
                onClick: (N) => p(S)
              }, "Save to store", 8, Gu),
              R("span", Qu, $t(S.credStatus), 1)
            ])) : os("", !0),
            R("div", Xu, [
              y[15] || (y[15] = R("span", { class: "muted w-70" }, "User", -1)),
              ve(R("input", {
                "onUpdate:modelValue": (N) => S.user = N,
                placeholder: S.credential ? "(from credential)" : "login",
                class: "w-130",
                spellcheck: "false"
              }, null, 8, zu), [
                [qe, S.user]
              ])
            ])
          ], 64)) : os("", !0)
        ], 64)) : os("", !0),
        R("div", Zu, [
          y[16] || (y[16] = R("span", { class: "muted w-70" }, "Description", -1)),
          ve(R("input", {
            "onUpdate:modelValue": (N) => S.description = N,
            placeholder: "Shown to the AI — what this database contains",
            class: "grow"
          }, null, 8, eh), [
            [qe, S.description]
          ])
        ]),
        R("div", th, [
          R("button", {
            type: "button",
            disabled: S.testing,
            onClick: (N) => k(S)
          }, "Test Connection", 8, sh),
          R("span", nh, $t(S.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
}), rh = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, i] of t)
    s[n] = i;
  return s;
}, oh = /* @__PURE__ */ rh(ih, [["__scopeId", "data-v-4827b3ec"]]);
function ch(e, t) {
  let s = ff(oh, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toYaml(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  ch as mount
};
