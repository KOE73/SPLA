/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function ui(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const Q = {}, Dt = [], Je = () => {
}, Fr = () => !1, an = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), fn = (e) => e.startsWith("onUpdate:"), ae = Object.assign, hi = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, Rl = Object.prototype.hasOwnProperty, W = (e, t) => Rl.call(e, t), R = Array.isArray, Rt = (e) => Ns(e) === "[object Map]", Jt = (e) => Ns(e) === "[object Set]", Qi = (e) => Ns(e) === "[object Date]", F = (e) => typeof e == "function", se = (e) => typeof e == "string", Ye = (e) => typeof e == "symbol", Y = (e) => e !== null && typeof e == "object", Ur = (e) => (Y(e) || F(e)) && F(e.then) && F(e.catch), Vr = Object.prototype.toString, Ns = (e) => Vr.call(e), jl = (e) => Ns(e).slice(8, -1), qr = (e) => Ns(e) === "[object Object]", di = (e) => se(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, fs = /* @__PURE__ */ ui(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), un = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, Bl = /-\w/g, $e = un(
  (e) => e.replace(Bl, (t) => t.slice(1).toUpperCase())
), Kl = /\B([A-Z])/g, Nt = un(
  (e) => e.replace(Kl, "-$1").toLowerCase()
), Hr = un((e) => e.charAt(0).toUpperCase() + e.slice(1)), In = un(
  (e) => e ? `on${Hr(e)}` : ""
), We = (e, t) => !Object.is(e, t), Ws = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Wr = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, hn = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let Xi;
const dn = () => Xi || (Xi = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function pi(e) {
  if (R(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], i = se(n) ? ql(n) : pi(n);
      if (i)
        for (const r in i)
          t[r] = i[r];
    }
    return t;
  } else if (se(e) || Y(e))
    return e;
}
const Fl = /;(?![^(]*\))/g, Ul = /:([^]+)/, Vl = /\/\*[^]*?\*\//g;
function ql(e) {
  const t = {};
  return e.replace(Vl, "").split(Fl).forEach((s) => {
    if (s) {
      const n = s.split(Ul);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function gi(e) {
  let t = "";
  if (se(e))
    t = e;
  else if (R(e))
    for (let s = 0; s < e.length; s++) {
      const n = gi(e[s]);
      n && (t += n + " ");
    }
  else if (Y(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const Hl = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", Wl = /* @__PURE__ */ ui(Hl);
function Jr(e) {
  return !!e || e === "";
}
function Jl(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = Yt(e[n], t[n]);
  return s;
}
function Yt(e, t) {
  if (e === t) return !0;
  let s = Qi(e), n = Qi(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Ye(e), n = Ye(t), s || n)
    return e === t;
  if (s = R(e), n = R(t), s || n)
    return s && n ? Jl(e, t) : !1;
  if (s = Y(e), n = Y(t), s || n) {
    if (!s || !n)
      return !1;
    const i = Object.keys(e).length, r = Object.keys(t).length;
    if (i !== r)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), c = t.hasOwnProperty(o);
      if (l && !c || !l && c || !Yt(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function mi(e, t) {
  return e.findIndex((s) => Yt(s, t));
}
const Yr = (e) => !!(e && e.__v_isRef === !0), Gr = (e) => se(e) ? e : e == null ? "" : R(e) || Y(e) && (e.toString === Vr || !F(e.toString)) ? Yr(e) ? Gr(e.value) : JSON.stringify(e, Qr, 2) : String(e), Qr = (e, t) => Yr(t) ? Qr(e, t.value) : Rt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, i], r) => (s[Ln(n, r) + " =>"] = i, s),
    {}
  )
} : Jt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => Ln(s))
} : Ye(t) ? Ln(t) : Y(t) && !R(t) && !qr(t) ? String(t) : t, Ln = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Ye(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let ce;
class Yl {
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
function Gl() {
  return ce;
}
let z;
const $n = /* @__PURE__ */ new WeakSet();
class Xr {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, ce && (ce.active ? ce.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, $n.has(this) && ($n.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || Zr(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, zi(this), eo(this);
    const t = z, s = Me;
    z = this, Me = !0;
    try {
      return this.fn();
    } finally {
      to(this), z = t, Me = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        wi(t);
      this.deps = this.depsTail = void 0, zi(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? $n.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Yn(this) && this.run();
  }
  get dirty() {
    return Yn(this);
  }
}
let zr = 0, us, hs;
function Zr(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = hs, hs = e;
    return;
  }
  e.next = us, us = e;
}
function yi() {
  zr++;
}
function bi() {
  if (--zr > 0)
    return;
  if (hs) {
    let t = hs;
    for (hs = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; us; ) {
    let t = us;
    for (us = void 0; t; ) {
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
function eo(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function to(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const i = n.prevDep;
    n.version === -1 ? (n === s && (s = i), wi(n), Ql(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = i;
  }
  e.deps = t, e.depsTail = s;
}
function Yn(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (so(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function so(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === ws) || (e.globalVersion = ws, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Yn(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = z, n = Me;
  z = e, Me = !0;
  try {
    eo(e);
    const i = e.fn(e._value);
    (t.version === 0 || We(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    z = s, Me = n, to(e), e.flags &= -3;
  }
}
function wi(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: i } = e;
  if (n && (n.nextSub = i, e.prevSub = void 0), i && (i.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let r = s.computed.deps; r; r = r.nextDep)
      wi(r, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function Ql(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let Me = !0;
const no = [];
function Ge() {
  no.push(Me), Me = !1;
}
function Qe() {
  const e = no.pop();
  Me = e === void 0 ? !0 : e;
}
function zi(e) {
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
let ws = 0;
class Xl {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class Si {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!z || !Me || z === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== z)
      s = this.activeLink = new Xl(z, this), z.deps ? (s.prevDep = z.depsTail, z.depsTail.nextDep = s, z.depsTail = s) : z.deps = z.depsTail = s, io(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = z.depsTail, s.nextDep = void 0, z.depsTail.nextDep = s, z.depsTail = s, z.deps === s && (z.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, ws++, this.notify(t);
  }
  notify(t) {
    yi();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      bi();
    }
  }
}
function io(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let n = t.deps; n; n = n.nextDep)
        io(n);
    }
    const s = e.dep.subs;
    s !== e && (e.prevSub = s, s && (s.nextSub = e)), e.dep.subs = e;
  }
}
const Gn = /* @__PURE__ */ new WeakMap(), Ot = /* @__PURE__ */ Symbol(
  ""
), Qn = /* @__PURE__ */ Symbol(
  ""
), Ss = /* @__PURE__ */ Symbol(
  ""
);
function he(e, t, s) {
  if (Me && z) {
    let n = Gn.get(e);
    n || Gn.set(e, n = /* @__PURE__ */ new Map());
    let i = n.get(s);
    i || (n.set(s, i = new Si()), i.map = n, i.key = s), i.track();
  }
}
function nt(e, t, s, n, i, r) {
  const o = Gn.get(e);
  if (!o) {
    ws++;
    return;
  }
  const l = (c) => {
    c && c.trigger();
  };
  if (yi(), t === "clear")
    o.forEach(l);
  else {
    const c = R(e), a = c && di(s);
    if (c && s === "length") {
      const f = Number(n);
      o.forEach((u, m) => {
        (m === "length" || m === Ss || !Ye(m) && m >= f) && l(u);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), a && l(o.get(Ss)), t) {
        case "add":
          c ? a && l(o.get("length")) : (l(o.get(Ot)), Rt(e) && l(o.get(Qn)));
          break;
        case "delete":
          c || (l(o.get(Ot)), Rt(e) && l(o.get(Qn)));
          break;
        case "set":
          Rt(e) && l(o.get(Ot));
          break;
      }
  }
  bi();
}
function Ct(e) {
  const t = /* @__PURE__ */ H(e);
  return t === e ? t : (he(t, "iterate", Ss), /* @__PURE__ */ Ae(e) ? t : t.map(Pe));
}
function pn(e) {
  return he(e = /* @__PURE__ */ H(e), "iterate", Ss), e;
}
function qe(e, t) {
  return /* @__PURE__ */ at(e) ? Ut(/* @__PURE__ */ Tt(e) ? Pe(t) : t) : Pe(t);
}
const zl = {
  __proto__: null,
  [Symbol.iterator]() {
    return Mn(this, Symbol.iterator, (e) => qe(this, e));
  },
  concat(...e) {
    return Ct(this).concat(
      ...e.map((t) => R(t) ? Ct(t) : t)
    );
  },
  entries() {
    return Mn(this, "entries", (e) => (e[1] = qe(this, e[1]), e));
  },
  every(e, t) {
    return ze(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return ze(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => qe(this, n)),
      arguments
    );
  },
  find(e, t) {
    return ze(
      this,
      "find",
      e,
      t,
      (s) => qe(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return ze(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return ze(
      this,
      "findLast",
      e,
      t,
      (s) => qe(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return ze(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return ze(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return Pn(this, "includes", e);
  },
  indexOf(...e) {
    return Pn(this, "indexOf", e);
  },
  join(e) {
    return Ct(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return Pn(this, "lastIndexOf", e);
  },
  map(e, t) {
    return ze(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return ns(this, "pop");
  },
  push(...e) {
    return ns(this, "push", e);
  },
  reduce(e, ...t) {
    return Zi(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return Zi(this, "reduceRight", e, t);
  },
  shift() {
    return ns(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return ze(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return ns(this, "splice", e);
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
    return ns(this, "unshift", e);
  },
  values() {
    return Mn(this, "values", (e) => qe(this, e));
  }
};
function Mn(e, t, s) {
  const n = pn(e), i = n[t]();
  return n !== e && !/* @__PURE__ */ Ae(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = s(r.value)), r;
  }), i;
}
const Zl = Array.prototype;
function ze(e, t, s, n, i, r) {
  const o = pn(e), l = o !== e && !/* @__PURE__ */ Ae(e), c = o[t];
  if (c !== Zl[t]) {
    const u = c.apply(e, r);
    return l ? Pe(u) : u;
  }
  let a = s;
  o !== e && (l ? a = function(u, m) {
    return s.call(this, qe(e, u), m, e);
  } : s.length > 2 && (a = function(u, m) {
    return s.call(this, u, m, e);
  }));
  const f = c.call(o, a, n);
  return l && i ? i(f) : f;
}
function Zi(e, t, s, n) {
  const i = pn(e), r = i !== e && !/* @__PURE__ */ Ae(e);
  let o = s, l = !1;
  i !== e && (r ? (l = n.length === 0, o = function(a, f, u) {
    return l && (l = !1, a = qe(e, a)), s.call(this, a, qe(e, f), u, e);
  }) : s.length > 3 && (o = function(a, f, u) {
    return s.call(this, a, f, u, e);
  }));
  const c = i[t](o, ...n);
  return l ? qe(e, c) : c;
}
function Pn(e, t, s) {
  const n = /* @__PURE__ */ H(e);
  he(n, "iterate", Ss);
  const i = n[t](...s);
  return (i === -1 || i === !1) && /* @__PURE__ */ ki(s[0]) ? (s[0] = /* @__PURE__ */ H(s[0]), n[t](...s)) : i;
}
function ns(e, t, s = []) {
  Ge(), yi();
  const n = (/* @__PURE__ */ H(e))[t].apply(e, s);
  return bi(), Qe(), n;
}
const ec = /* @__PURE__ */ ui("__proto__,__v_isRef,__isVue"), ro = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Ye)
);
function tc(e) {
  Ye(e) || (e = String(e));
  const t = /* @__PURE__ */ H(this);
  return he(t, "has", e), t.hasOwnProperty(e);
}
class oo {
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
      return n === (i ? r ? uc : fo : r ? ao : co).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = R(t);
    if (!i) {
      let c;
      if (o && (c = zl[s]))
        return c;
      if (s === "hasOwnProperty")
        return tc;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ de(t) ? t : n
    );
    if ((Ye(s) ? ro.has(s) : ec(s)) || (i || he(t, "get", s), r))
      return l;
    if (/* @__PURE__ */ de(l)) {
      const c = o && di(s) ? l : l.value;
      return i && Y(c) ? /* @__PURE__ */ zn(c) : c;
    }
    return Y(l) ? i ? /* @__PURE__ */ zn(l) : /* @__PURE__ */ gn(l) : l;
  }
}
class lo extends oo {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, i) {
    let r = t[s];
    const o = R(t) && di(s);
    if (!this._isShallow) {
      const a = /* @__PURE__ */ at(r);
      if (!/* @__PURE__ */ Ae(n) && !/* @__PURE__ */ at(n) && (r = /* @__PURE__ */ H(r), n = /* @__PURE__ */ H(n)), !o && /* @__PURE__ */ de(r) && !/* @__PURE__ */ de(n))
        return a || (r.value = n), !0;
    }
    const l = o ? Number(s) < t.length : W(t, s), c = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ de(t) ? t : i
    );
    return t === /* @__PURE__ */ H(i) && c && (l ? We(n, r) && nt(t, "set", s, n) : nt(t, "add", s, n)), c;
  }
  deleteProperty(t, s) {
    const n = W(t, s);
    t[s];
    const i = Reflect.deleteProperty(t, s);
    return i && n && nt(t, "delete", s, void 0), i;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Ye(s) || !ro.has(s)) && he(t, "has", s), n;
  }
  ownKeys(t) {
    return he(
      t,
      "iterate",
      R(t) ? "length" : Ot
    ), Reflect.ownKeys(t);
  }
}
class sc extends oo {
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
const nc = /* @__PURE__ */ new lo(), ic = /* @__PURE__ */ new sc(), rc = /* @__PURE__ */ new lo(!0);
const Xn = (e) => e, Rs = (e) => Reflect.getPrototypeOf(e);
function oc(e, t, s) {
  return function(...n) {
    const i = this.__v_raw, r = /* @__PURE__ */ H(i), o = Rt(r), l = e === "entries" || e === Symbol.iterator && o, c = e === "keys" && o, a = i[e](...n), f = s ? Xn : t ? Ut : Pe;
    return !t && he(
      r,
      "iterate",
      c ? Qn : Ot
    ), ae(
      // inheriting all iterator properties
      Object.create(a),
      {
        // iterator protocol
        next() {
          const { value: u, done: m } = a.next();
          return m ? { value: u, done: m } : {
            value: l ? [f(u[0]), f(u[1])] : f(u),
            done: m
          };
        }
      }
    );
  };
}
function js(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function lc(e, t) {
  const s = {
    get(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ H(r), l = /* @__PURE__ */ H(i);
      e || (We(i, l) && he(o, "get", i), he(o, "get", l));
      const { has: c } = Rs(o), a = t ? Xn : e ? Ut : Pe;
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
      return e || (We(i, l) && he(o, "has", i), he(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, c = /* @__PURE__ */ H(l), a = t ? Xn : e ? Ut : Pe;
      return !e && he(c, "iterate", Ot), l.forEach((f, u) => i.call(r, a(f), a(u), o));
    }
  };
  return ae(
    s,
    e ? {
      add: js("add"),
      set: js("set"),
      delete: js("delete"),
      clear: js("clear")
    } : {
      add(i) {
        const r = /* @__PURE__ */ H(this), o = Rs(r), l = /* @__PURE__ */ H(i), c = !t && !/* @__PURE__ */ Ae(i) && !/* @__PURE__ */ at(i) ? l : i;
        return o.has.call(r, c) || We(i, c) && o.has.call(r, i) || We(l, c) && o.has.call(r, l) || (r.add(c), nt(r, "add", c, c)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ Ae(r) && !/* @__PURE__ */ at(r) && (r = /* @__PURE__ */ H(r));
        const o = /* @__PURE__ */ H(this), { has: l, get: c } = Rs(o);
        let a = l.call(o, i);
        a || (i = /* @__PURE__ */ H(i), a = l.call(o, i));
        const f = c.call(o, i);
        return o.set(i, r), a ? We(r, f) && nt(o, "set", i, r) : nt(o, "add", i, r), this;
      },
      delete(i) {
        const r = /* @__PURE__ */ H(this), { has: o, get: l } = Rs(r);
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
    s[i] = oc(i, e, t);
  }), s;
}
function _i(e, t) {
  const s = lc(e, t);
  return (n, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? n : Reflect.get(
    W(s, i) && i in n ? s : n,
    i,
    r
  );
}
const cc = {
  get: /* @__PURE__ */ _i(!1, !1)
}, ac = {
  get: /* @__PURE__ */ _i(!1, !0)
}, fc = {
  get: /* @__PURE__ */ _i(!0, !1)
};
const co = /* @__PURE__ */ new WeakMap(), ao = /* @__PURE__ */ new WeakMap(), fo = /* @__PURE__ */ new WeakMap(), uc = /* @__PURE__ */ new WeakMap();
function hc(e) {
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
function gn(e) {
  return /* @__PURE__ */ at(e) ? e : vi(
    e,
    !1,
    nc,
    cc,
    co
  );
}
// @__NO_SIDE_EFFECTS__
function dc(e) {
  return vi(
    e,
    !1,
    rc,
    ac,
    ao
  );
}
// @__NO_SIDE_EFFECTS__
function zn(e) {
  return vi(
    e,
    !0,
    ic,
    fc,
    fo
  );
}
function vi(e, t, s, n, i) {
  if (!Y(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const r = i.get(e);
  if (r)
    return r;
  const o = hc(jl(e));
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
function Ae(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function ki(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function H(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ H(t) : e;
}
function pc(e) {
  return !W(e, "__v_skip") && Object.isExtensible(e) && Wr(e, "__v_skip", !0), e;
}
const Pe = (e) => Y(e) ? /* @__PURE__ */ gn(e) : e, Ut = (e) => Y(e) ? /* @__PURE__ */ zn(e) : e;
// @__NO_SIDE_EFFECTS__
function de(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function er(e) {
  return gc(e, !1);
}
function gc(e, t) {
  return /* @__PURE__ */ de(e) ? e : new mc(e, t);
}
class mc {
  constructor(t, s) {
    this.dep = new Si(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ H(t), this._value = s ? t : Pe(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ Ae(t) || /* @__PURE__ */ at(t);
    t = n ? t : /* @__PURE__ */ H(t), We(t, s) && (this._rawValue = t, this._value = n ? t : Pe(t), this.dep.trigger());
  }
}
function yc(e) {
  return /* @__PURE__ */ de(e) ? e.value : e;
}
const bc = {
  get: (e, t, s) => t === "__v_raw" ? e : yc(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const i = e[t];
    return /* @__PURE__ */ de(i) && !/* @__PURE__ */ de(s) ? (i.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function uo(e) {
  return /* @__PURE__ */ Tt(e) ? e : new Proxy(e, bc);
}
class wc {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Si(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = ws - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    z !== this)
      return Zr(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return so(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function Sc(e, t, s = !1) {
  let n, i;
  return F(e) ? n = e : (n = e.get, i = e.set), new wc(n, i, s);
}
const Bs = {}, zs = /* @__PURE__ */ new WeakMap();
let St;
function _c(e, t = !1, s = St) {
  if (s) {
    let n = zs.get(s);
    n || zs.set(s, n = []), n.push(e);
  }
}
function vc(e, t, s = Q) {
  const { immediate: n, deep: i, once: r, scheduler: o, augmentJob: l, call: c } = s, a = (A) => i ? A : /* @__PURE__ */ Ae(A) || i === !1 || i === 0 ? it(A, 1) : it(A);
  let f, u, m, g, b = !1, h = !1;
  if (/* @__PURE__ */ de(e) ? (u = () => e.value, b = /* @__PURE__ */ Ae(e)) : /* @__PURE__ */ Tt(e) ? (u = () => a(e), b = !0) : R(e) ? (h = !0, b = e.some((A) => /* @__PURE__ */ Tt(A) || /* @__PURE__ */ Ae(A)), u = () => e.map((A) => {
    if (/* @__PURE__ */ de(A))
      return A.value;
    if (/* @__PURE__ */ Tt(A))
      return a(A);
    if (F(A))
      return c ? c(A, 2) : A();
  })) : F(e) ? t ? u = c ? () => c(e, 2) : e : u = () => {
    if (m) {
      Ge();
      try {
        m();
      } finally {
        Qe();
      }
    }
    const A = St;
    St = f;
    try {
      return c ? c(e, 3, [g]) : e(g);
    } finally {
      St = A;
    }
  } : u = Je, t && i) {
    const A = u, $ = i === !0 ? 1 / 0 : i;
    u = () => it(A(), $);
  }
  const y = Gl(), O = () => {
    f.stop(), y && y.active && hi(y.effects, f);
  };
  if (r && t) {
    const A = t;
    t = (...$) => {
      const K = A(...$);
      return O(), K;
    };
  }
  let S = h ? new Array(e.length).fill(Bs) : Bs;
  const I = (A) => {
    if (!(!(f.flags & 1) || !f.dirty && !A))
      if (t) {
        const $ = f.run();
        if (A || i || b || (h ? $.some((K, x) => We(K, S[x])) : We($, S))) {
          m && m();
          const K = St;
          St = f;
          try {
            const x = [
              $,
              // pass undefined as the old value when it's changed for the first time
              S === Bs ? void 0 : h && S[0] === Bs ? [] : S,
              g
            ];
            S = $, c ? c(t, 3, x) : (
              // @ts-expect-error
              t(...x)
            );
          } finally {
            St = K;
          }
        }
      } else
        f.run();
  };
  return l && l(I), f = new Xr(u), f.scheduler = o ? () => o(I, !1) : I, g = (A) => _c(A, !1, f), m = f.onStop = () => {
    const A = zs.get(f);
    if (A) {
      if (c)
        c(A, 4);
      else
        for (const $ of A) $();
      zs.delete(f);
    }
  }, t ? n ? I(!0) : S = f.run() : o ? o(I.bind(null, !0), !0) : f.run(), O.pause = f.pause.bind(f), O.resume = f.resume.bind(f), O.stop = O, O;
}
function it(e, t = 1 / 0, s) {
  if (t <= 0 || !Y(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ de(e))
    it(e.value, t, s);
  else if (R(e))
    for (let n = 0; n < e.length; n++)
      it(e[n], t, s);
  else if (Jt(e) || Rt(e))
    e.forEach((n) => {
      it(n, t, s);
    });
  else if (qr(e)) {
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
function Es(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (i) {
    mn(i, t, s);
  }
}
function xe(e, t, s, n) {
  if (F(e)) {
    const i = Es(e, t, s, n);
    return i && Ur(i) && i.catch((r) => {
      mn(r, t, s);
    }), i;
  }
  if (R(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(xe(e[r], t, s, n));
    return i;
  }
}
function mn(e, t, s, n = !0) {
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
      Ge(), Es(r, null, 10, [
        e,
        c,
        a
      ]), Qe();
      return;
    }
  }
  kc(e, s, i, n, o);
}
function kc(e, t, s, n = !0, i = !1) {
  if (i)
    throw e;
  console.error(e);
}
const be = [];
let Ve = -1;
const jt = [];
let ut = null, $t = 0;
const ho = /* @__PURE__ */ Promise.resolve();
let Zs = null;
function po(e) {
  const t = Zs || ho;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Oc(e) {
  let t = Ve + 1, s = be.length;
  for (; t < s; ) {
    const n = t + s >>> 1, i = be[n], r = _s(i);
    r < e || r === e && i.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function Oi(e) {
  if (!(e.flags & 1)) {
    const t = _s(e), s = be[be.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= _s(s) ? be.push(e) : be.splice(Oc(t), 0, e), e.flags |= 1, go();
  }
}
function go() {
  Zs || (Zs = ho.then(yo));
}
function Tc(e) {
  R(e) ? jt.push(...e) : ut && e.id === -1 ? ut.splice($t + 1, 0, e) : e.flags & 1 || (jt.push(e), e.flags |= 1), go();
}
function tr(e, t, s = Ve + 1) {
  for (; s < be.length; s++) {
    const n = be[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      be.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function mo(e) {
  if (jt.length) {
    const t = [...new Set(jt)].sort(
      (s, n) => _s(s) - _s(n)
    );
    if (jt.length = 0, ut) {
      ut.push(...t);
      return;
    }
    for (ut = t, $t = 0; $t < ut.length; $t++) {
      const s = ut[$t];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    ut = null, $t = 0;
  }
}
const _s = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function yo(e) {
  try {
    for (Ve = 0; Ve < be.length; Ve++) {
      const t = be[Ve];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Es(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Ve < be.length; Ve++) {
      const t = be[Ve];
      t && (t.flags &= -2);
    }
    Ve = -1, be.length = 0, mo(), Zs = null, (be.length || jt.length) && yo();
  }
}
let Te = null, bo = null;
function en(e) {
  const t = Te;
  return Te = e, bo = e && e.type.__scopeId || null, t;
}
function Ac(e, t = Te, s) {
  if (!t || e._n)
    return e;
  const n = (...i) => {
    n._d && hr(-1);
    const r = en(t);
    let o;
    try {
      o = e(...i);
    } finally {
      en(r), n._d && hr(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function Ce(e, t) {
  if (Te === null)
    return e;
  const s = Sn(Te), n = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [r, o, l, c = Q] = t[i];
    r && (F(r) && (r = {
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
    c && (Ge(), xe(c, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Qe());
  }
}
function Nc(e, t) {
  if (we) {
    let s = we.provides;
    const n = we.parent && we.parent.provides;
    n === s && (s = we.provides = Object.create(n)), s[e] = t;
  }
}
function Js(e, t, s = !1) {
  const n = Ea();
  if (n || Bt) {
    let i = Bt ? Bt._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return s && F(t) ? t.call(n && n.proxy) : t;
  }
}
const Ec = /* @__PURE__ */ Symbol.for("v-scx"), Cc = () => Js(Ec);
function xn(e, t, s) {
  return wo(e, t, s);
}
function wo(e, t, s = Q) {
  const { immediate: n, deep: i, flush: r, once: o } = s, l = ae({}, s), c = t && n || !t && r !== "post";
  let a;
  if (ks) {
    if (r === "sync") {
      const g = Cc();
      a = g.__watcherHandles || (g.__watcherHandles = []);
    } else if (!c) {
      const g = () => {
      };
      return g.stop = Je, g.resume = Je, g.pause = Je, g;
    }
  }
  const f = we;
  l.call = (g, b, h) => xe(g, f, b, h);
  let u = !1;
  r === "post" ? l.scheduler = (g) => {
    _e(g, f && f.suspense);
  } : r !== "sync" && (u = !0, l.scheduler = (g, b) => {
    b ? g() : Oi(g);
  }), l.augmentJob = (g) => {
    t && (g.flags |= 4), u && (g.flags |= 2, f && (g.id = f.uid, g.i = f));
  };
  const m = vc(e, t, l);
  return ks && (a ? a.push(m) : c && m()), m;
}
function Ic(e, t, s) {
  const n = this.proxy, i = se(e) ? e.includes(".") ? So(n, e) : () => n[e] : e.bind(n, n);
  let r;
  F(t) ? r = t : (r = t.handler, s = t);
  const o = Cs(this), l = wo(i, r.bind(n), s);
  return o(), l;
}
function So(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let i = 0; i < s.length && n; i++)
      n = n[s[i]];
    return n;
  };
}
const Lc = /* @__PURE__ */ Symbol("_vte"), $c = (e) => e.__isTeleport, Dn = /* @__PURE__ */ Symbol("_leaveCb");
function Ti(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, Ti(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Mc(e, t) {
  return F(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    ae({ name: e.name }, t, { setup: e })
  ) : e;
}
function _o(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function sr(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const tn = /* @__PURE__ */ new WeakMap();
function ds(e, t, s, n, i = !1) {
  if (R(e)) {
    e.forEach(
      (h, y) => ds(
        h,
        t && (R(t) ? t[y] : t),
        s,
        n,
        i
      )
    );
    return;
  }
  if (ps(n) && !i) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && ds(e, t, s, n.component.subTree);
    return;
  }
  const r = n.shapeFlag & 4 ? Sn(n.component) : n.el, o = i ? null : r, { i: l, r: c } = e, a = t && t.r, f = l.refs === Q ? l.refs = {} : l.refs, u = l.setupState, m = /* @__PURE__ */ H(u), g = u === Q ? Fr : (h) => sr(f, h) ? !1 : W(m, h), b = (h, y) => !(y && sr(f, y));
  if (a != null && a !== c) {
    if (nr(t), se(a))
      f[a] = null, g(a) && (u[a] = null);
    else if (/* @__PURE__ */ de(a)) {
      const h = t;
      b(a, h.k) && (a.value = null), h.k && (f[h.k] = null);
    }
  }
  if (F(c)) {
    Ge();
    try {
      Es(c, l, 12, [o, f]);
    } finally {
      Qe();
    }
  } else {
    const h = se(c), y = /* @__PURE__ */ de(c);
    if (h || y) {
      const O = () => {
        if (e.f) {
          const S = h ? g(c) ? u[c] : f[c] : b() || !e.k ? c.value : f[e.k];
          if (i)
            R(S) && hi(S, r);
          else if (R(S))
            S.includes(r) || S.push(r);
          else if (h)
            f[c] = [r], g(c) && (u[c] = f[c]);
          else {
            const I = [r];
            b(c, e.k) && (c.value = I), e.k && (f[e.k] = I);
          }
        } else h ? (f[c] = o, g(c) && (u[c] = o)) : y && (b(c, e.k) && (c.value = o), e.k && (f[e.k] = o));
      };
      if (o) {
        const S = () => {
          O(), tn.delete(e);
        };
        S.id = -1, tn.set(e, S), _e(S, s);
      } else
        nr(e), O();
    }
  }
}
function nr(e) {
  const t = tn.get(e);
  t && (t.flags |= 8, tn.delete(e));
}
dn().requestIdleCallback;
dn().cancelIdleCallback;
const ps = (e) => !!e.type.__asyncLoader, vo = (e) => e.type.__isKeepAlive;
function Pc(e, t) {
  ko(e, "a", t);
}
function xc(e, t) {
  ko(e, "da", t);
}
function ko(e, t, s = we) {
  const n = e.__wdc || (e.__wdc = () => {
    let i = s;
    for (; i; ) {
      if (i.isDeactivated)
        return;
      i = i.parent;
    }
    return e();
  });
  if (yn(t, n, s), s) {
    let i = s.parent;
    for (; i && i.parent; )
      vo(i.parent.vnode) && Dc(n, t, s, i), i = i.parent;
  }
}
function Dc(e, t, s, n) {
  const i = yn(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Oo(() => {
    hi(n[t], i);
  }, s);
}
function yn(e, t, s = we, n = !1) {
  if (s) {
    const i = s[e] || (s[e] = []), r = t.__weh || (t.__weh = (...o) => {
      Ge();
      const l = Cs(s), c = xe(t, s, e, o);
      return l(), Qe(), c;
    });
    return n ? i.unshift(r) : i.push(r), r;
  }
}
const ft = (e) => (t, s = we) => {
  (!ks || e === "sp") && yn(e, (...n) => t(...n), s);
}, Rc = ft("bm"), jc = ft("m"), Bc = ft(
  "bu"
), Kc = ft("u"), Fc = ft(
  "bum"
), Oo = ft("um"), Uc = ft(
  "sp"
), Vc = ft("rtg"), qc = ft("rtc");
function Hc(e, t = we) {
  yn("ec", e, t);
}
const Wc = /* @__PURE__ */ Symbol.for("v-ndc");
function Jc(e, t, s, n) {
  let i;
  const r = s, o = R(e);
  if (o || se(e)) {
    const l = o && /* @__PURE__ */ Tt(e);
    let c = !1, a = !1;
    l && (c = !/* @__PURE__ */ Ae(e), a = /* @__PURE__ */ at(e), e = pn(e)), i = new Array(e.length);
    for (let f = 0, u = e.length; f < u; f++)
      i[f] = t(
        c ? a ? Ut(Pe(e[f])) : Pe(e[f]) : e[f],
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
const Zn = (e) => e ? Wo(e) ? Sn(e) : Zn(e.parent) : null, gs = (
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
    $parent: (e) => Zn(e.parent),
    $root: (e) => Zn(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Ao(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      Oi(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = po.bind(e.proxy)),
    $watch: (e) => Ic.bind(e)
  })
), Rn = (e, t) => e !== Q && !e.__isScriptSetup && W(e, t), Yc = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: i, props: r, accessCache: o, type: l, appContext: c } = e;
    if (t[0] !== "$") {
      const m = o[t];
      if (m !== void 0)
        switch (m) {
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
        if (Rn(n, t))
          return o[t] = 1, n[t];
        if (i !== Q && W(i, t))
          return o[t] = 2, i[t];
        if (W(r, t))
          return o[t] = 3, r[t];
        if (s !== Q && W(s, t))
          return o[t] = 4, s[t];
        ei && (o[t] = 0);
      }
    }
    const a = gs[t];
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
    return Rn(i, t) ? (i[t] = s, !0) : n !== Q && W(n, t) ? (n[t] = s, !0) : W(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: i, props: r, type: o }
  }, l) {
    let c;
    return !!(s[l] || e !== Q && l[0] !== "$" && W(e, l) || Rn(t, l) || W(r, l) || W(n, l) || W(gs, l) || W(i.config.globalProperties, l) || (c = o.__cssModules) && c[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : W(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function ir(e) {
  return R(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let ei = !0;
function Gc(e) {
  const t = Ao(e), s = e.proxy, n = e.ctx;
  ei = !1, t.beforeCreate && rr(t.beforeCreate, e, "bc");
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
    mounted: m,
    beforeUpdate: g,
    updated: b,
    activated: h,
    deactivated: y,
    beforeDestroy: O,
    beforeUnmount: S,
    destroyed: I,
    unmounted: A,
    render: $,
    renderTracked: K,
    renderTriggered: x,
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
      F(X) && (n[te] = X.bind(s));
    }
  if (i) {
    const te = i.call(s, s);
    Y(te) && (e.data = /* @__PURE__ */ gn(te));
  }
  if (ei = !0, r)
    for (const te in r) {
      const X = r[te], mt = F(X) ? X.bind(s, s) : F(X.get) ? X.get.bind(s, s) : Je, xs = !F(X) && F(X.set) ? X.set.bind(s) : Je, yt = Pa({
        get: mt,
        set: xs
      });
      Object.defineProperty(n, te, {
        enumerable: !0,
        configurable: !0,
        get: () => yt.value,
        set: (Re) => yt.value = Re
      });
    }
  if (l)
    for (const te in l)
      To(l[te], n, s, te);
  if (c) {
    const te = F(c) ? c.call(s) : c;
    Reflect.ownKeys(te).forEach((X) => {
      Nc(X, te[X]);
    });
  }
  f && rr(f, e, "c");
  function ge(te, X) {
    R(X) ? X.forEach((mt) => te(mt.bind(s))) : X && te(X.bind(s));
  }
  if (ge(Rc, u), ge(jc, m), ge(Bc, g), ge(Kc, b), ge(Pc, h), ge(xc, y), ge(Hc, M), ge(qc, K), ge(Vc, x), ge(Fc, S), ge(Oo, A), ge(Uc, q), R(ee))
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
  $ && e.render === Je && (e.render = $), pe != null && (e.inheritAttrs = pe), fe && (e.components = fe), ue && (e.directives = ue), q && _o(e);
}
function Qc(e, t, s = Je) {
  R(e) && (e = ti(e));
  for (const n in e) {
    const i = e[n];
    let r;
    Y(i) ? "default" in i ? r = Js(
      i.from || n,
      i.default,
      !0
    ) : r = Js(i.from || n) : r = Js(i), /* @__PURE__ */ de(r) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => r.value,
      set: (o) => r.value = o
    }) : t[n] = r;
  }
}
function rr(e, t, s) {
  xe(
    R(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function To(e, t, s, n) {
  let i = n.includes(".") ? So(s, n) : () => s[n];
  if (se(e)) {
    const r = t[e];
    F(r) && xn(i, r);
  } else if (F(e))
    xn(i, e.bind(s));
  else if (Y(e))
    if (R(e))
      e.forEach((r) => To(r, t, s, n));
    else {
      const r = F(e.handler) ? e.handler.bind(s) : t[e.handler];
      F(r) && xn(i, r, e);
    }
}
function Ao(e) {
  const t = e.type, { mixins: s, extends: n } = t, {
    mixins: i,
    optionsCache: r,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = r.get(t);
  let c;
  return l ? c = l : !i.length && !s && !n ? c = t : (c = {}, i.length && i.forEach(
    (a) => sn(c, a, o, !0)
  ), sn(c, t, o)), Y(t) && r.set(t, c), c;
}
function sn(e, t, s, n = !1) {
  const { mixins: i, extends: r } = t;
  r && sn(e, r, s, !0), i && i.forEach(
    (o) => sn(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = Xc[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const Xc = {
  data: or,
  props: lr,
  emits: lr,
  // objects
  methods: os,
  computed: os,
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
  components: os,
  directives: os,
  // watch
  watch: Zc,
  // provide / inject
  provide: or,
  inject: zc
};
function or(e, t) {
  return t ? e ? function() {
    return ae(
      F(e) ? e.call(this, this) : e,
      F(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function zc(e, t) {
  return os(ti(e), ti(t));
}
function ti(e) {
  if (R(e)) {
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
function os(e, t) {
  return e ? ae(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function lr(e, t) {
  return e ? R(e) && R(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : ae(
    /* @__PURE__ */ Object.create(null),
    ir(e),
    ir(t ?? {})
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
function No() {
  return {
    app: null,
    config: {
      isNativeTag: Fr,
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
    F(n) || (n = ae({}, n)), i != null && !Y(i) && (i = null);
    const r = No(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let c = !1;
    const a = r.app = {
      _uid: ea++,
      _component: n,
      _props: i,
      _container: null,
      _context: r,
      _instance: null,
      version: xa,
      get config() {
        return r.config;
      },
      set config(f) {
      },
      use(f, ...u) {
        return o.has(f) || (f && F(f.install) ? (o.add(f), f.install(a, ...u)) : F(f) && (o.add(f), f(a, ...u))), a;
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
      mount(f, u, m) {
        if (!c) {
          const g = a._ceVNode || ot(n, i);
          return g.appContext = r, m === !0 ? m = "svg" : m === !1 && (m = void 0), e(g, f, m), c = !0, a._container = f, f.__vue_app__ = a, Sn(g.component);
        }
      },
      onUnmount(f) {
        l.push(f);
      },
      unmount() {
        c && (xe(
          l,
          a._instance,
          16
        ), e(null, a._container), delete a._container.__vue_app__);
      },
      provide(f, u) {
        return r.provides[f] = u, a;
      },
      runWithContext(f) {
        const u = Bt;
        Bt = a;
        try {
          return f();
        } finally {
          Bt = u;
        }
      }
    };
    return a;
  };
}
let Bt = null;
const sa = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${$e(t)}Modifiers`] || e[`${Nt(t)}Modifiers`];
function na(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || Q;
  let i = s;
  const r = t.startsWith("update:"), o = r && sa(n, t.slice(7));
  o && (o.trim && (i = s.map((f) => se(f) ? f.trim() : f)), o.number && (i = s.map(hn)));
  let l, c = n[l = In(t)] || // also try camelCase event handler (#2249)
  n[l = In($e(t))];
  !c && r && (c = n[l = In(Nt(t))]), c && xe(
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
    e.emitted[l] = !0, xe(
      a,
      e,
      6,
      i
    );
  }
}
const ia = /* @__PURE__ */ new WeakMap();
function Eo(e, t, s = !1) {
  const n = s ? ia : t.emitsCache, i = n.get(e);
  if (i !== void 0)
    return i;
  const r = e.emits;
  let o = {}, l = !1;
  if (!F(e)) {
    const c = (a) => {
      const f = Eo(a, t, !0);
      f && (l = !0, ae(o, f));
    };
    !s && t.mixins.length && t.mixins.forEach(c), e.extends && c(e.extends), e.mixins && e.mixins.forEach(c);
  }
  return !r && !l ? (Y(e) && n.set(e, null), null) : (R(r) ? r.forEach((c) => o[c] = null) : ae(o, r), Y(e) && n.set(e, o), o);
}
function bn(e, t) {
  return !e || !an(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), W(e, t[0].toLowerCase() + t.slice(1)) || W(e, Nt(t)) || W(e, t));
}
function cr(e) {
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
    data: m,
    setupState: g,
    ctx: b,
    inheritAttrs: h
  } = e, y = en(e);
  let O, S;
  try {
    if (s.shapeFlag & 4) {
      const A = i || n, $ = A;
      O = He(
        a.call(
          $,
          A,
          f,
          u,
          g,
          m,
          b
        )
      ), S = l;
    } else {
      const A = t;
      O = He(
        A.length > 1 ? A(
          u,
          { attrs: l, slots: o, emit: c }
        ) : A(
          u,
          null
        )
      ), S = t.props ? l : ra(l);
    }
  } catch (A) {
    ms.length = 0, mn(A, e, 1), O = ot(gt);
  }
  let I = O;
  if (S && h !== !1) {
    const A = Object.keys(S), { shapeFlag: $ } = I;
    A.length && $ & 7 && (r && A.some(fn) && (S = oa(
      S,
      r
    )), I = Vt(I, S, !1, !0));
  }
  return s.dirs && (I = Vt(I, null, !1, !0), I.dirs = I.dirs ? I.dirs.concat(s.dirs) : s.dirs), s.transition && Ti(I, s.transition), O = I, en(y), O;
}
const ra = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || an(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, oa = (e, t) => {
  const s = {};
  for (const n in e)
    (!fn(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
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
      return n ? ar(n, o, a) : !!o;
    if (c & 8) {
      const f = t.dynamicProps;
      for (let u = 0; u < f.length; u++) {
        const m = f[u];
        if (Co(o, n, m) && !bn(a, m))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? ar(n, o, a) : !0 : !!o;
  return !1;
}
function ar(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < n.length; i++) {
    const r = n[i];
    if (Co(t, e, r) && !bn(s, r))
      return !0;
  }
  return !1;
}
function Co(e, t, s) {
  const n = e[s], i = t[s];
  return s === "style" && Y(n) && Y(i) ? !Yt(n, i) : n !== i;
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
const Io = {}, Lo = () => Object.create(Io), $o = (e) => Object.getPrototypeOf(e) === Io;
function aa(e, t, s, n = !1) {
  const i = {}, r = Lo();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), Mo(e, t, i, r);
  for (const o in e.propsOptions[0])
    o in i || (i[o] = void 0);
  s ? e.props = n ? i : /* @__PURE__ */ dc(i) : e.type.props ? e.props = i : e.props = r, e.attrs = r;
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
        let m = f[u];
        if (bn(e.emitsOptions, m))
          continue;
        const g = t[m];
        if (c)
          if (W(r, m))
            g !== r[m] && (r[m] = g, a = !0);
          else {
            const b = $e(m);
            i[b] = si(
              c,
              l,
              b,
              g,
              e,
              !1
            );
          }
        else
          g !== r[m] && (r[m] = g, a = !0);
      }
    }
  } else {
    Mo(e, t, i, r) && (a = !0);
    let f;
    for (const u in l)
      (!t || // for camelCase
      !W(t, u) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((f = Nt(u)) === u || !W(t, f))) && (c ? s && // for camelCase
      (s[u] !== void 0 || // for kebab-case
      s[f] !== void 0) && (i[u] = si(
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
function Mo(e, t, s, n) {
  const [i, r] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let c in t) {
      if (fs(c))
        continue;
      const a = t[c];
      let f;
      i && W(i, f = $e(c)) ? !r || !r.includes(f) ? s[f] = a : (l || (l = {}))[f] = a : bn(e.emitsOptions, c) || (!(c in n) || a !== n[c]) && (n[c] = a, o = !0);
    }
  if (r) {
    const c = /* @__PURE__ */ H(s), a = l || Q;
    for (let f = 0; f < r.length; f++) {
      const u = r[f];
      s[u] = si(
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
function si(e, t, s, n, i, r) {
  const o = e[s];
  if (o != null) {
    const l = W(o, "default");
    if (l && n === void 0) {
      const c = o.default;
      if (o.type !== Function && !o.skipFactory && F(c)) {
        const { propsDefaults: a } = i;
        if (s in a)
          n = a[s];
        else {
          const f = Cs(i);
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
    ] && (n === "" || n === Nt(s)) && (n = !0));
  }
  return n;
}
const ua = /* @__PURE__ */ new WeakMap();
function Po(e, t, s = !1) {
  const n = s ? ua : t.propsCache, i = n.get(e);
  if (i)
    return i;
  const r = e.props, o = {}, l = [];
  let c = !1;
  if (!F(e)) {
    const f = (u) => {
      c = !0;
      const [m, g] = Po(u, t, !0);
      ae(o, m), g && l.push(...g);
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  if (!r && !c)
    return Y(e) && n.set(e, Dt), Dt;
  if (R(r))
    for (let f = 0; f < r.length; f++) {
      const u = $e(r[f]);
      fr(u) && (o[u] = Q);
    }
  else if (r)
    for (const f in r) {
      const u = $e(f);
      if (fr(u)) {
        const m = r[f], g = o[u] = R(m) || F(m) ? { type: m } : ae({}, m), b = g.type;
        let h = !1, y = !0;
        if (R(b))
          for (let O = 0; O < b.length; ++O) {
            const S = b[O], I = F(S) && S.name;
            if (I === "Boolean") {
              h = !0;
              break;
            } else I === "String" && (y = !1);
          }
        else
          h = F(b) && b.name === "Boolean";
        g[
          0
          /* shouldCast */
        ] = h, g[
          1
          /* shouldCastTrue */
        ] = y, (h || W(g, "default")) && l.push(u);
      }
    }
  const a = [o, l];
  return Y(e) && n.set(e, a), a;
}
function fr(e) {
  return e[0] !== "$" && !fs(e);
}
const Ai = (e) => e === "_" || e === "_ctx" || e === "$stable", Ni = (e) => R(e) ? e.map(He) : [He(e)], ha = (e, t, s) => {
  if (t._n)
    return t;
  const n = Ac((...i) => Ni(t(...i)), s);
  return n._c = !1, n;
}, xo = (e, t, s) => {
  const n = e._ctx;
  for (const i in e) {
    if (Ai(i)) continue;
    const r = e[i];
    if (F(r))
      t[i] = ha(i, r, n);
    else if (r != null) {
      const o = Ni(r);
      t[i] = () => o;
    }
  }
}, Do = (e, t) => {
  const s = Ni(t);
  e.slots.default = () => s;
}, Ro = (e, t, s) => {
  for (const n in t)
    (s || !Ai(n)) && (e[n] = t[n]);
}, da = (e, t, s) => {
  const n = e.slots = Lo();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Ro(n, t, s), s && Wr(n, "_", i, !0)) : xo(t, n);
  } else t && Do(e, t);
}, pa = (e, t, s) => {
  const { vnode: n, slots: i } = e;
  let r = !0, o = Q;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? r = !1 : Ro(i, t, s) : (r = !t.$stable, xo(t, i)), o = t;
  } else t && (Do(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !Ai(l) && o[l] == null && delete i[l];
}, _e = wa;
function ga(e) {
  return ma(e);
}
function ma(e, t) {
  const s = dn();
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
    nextSibling: m,
    setScopeId: g = Je,
    insertStaticContent: b
  } = e, h = (d, p, w, T = null, k = null, _ = null, C = void 0, E = null, N = !!p.dynamicChildren) => {
    if (d === p)
      return;
    d && !is(d, p) && (T = Ds(d), Re(d, k, _, !0), d = null), p.patchFlag === -2 && (N = !1, p.dynamicChildren = null);
    const { type: v, ref: D, shapeFlag: L } = p;
    switch (v) {
      case wn:
        y(d, p, w, T);
        break;
      case gt:
        O(d, p, w, T);
        break;
      case Bn:
        d == null && S(p, w, T, C);
        break;
      case Le:
        fe(
          d,
          p,
          w,
          T,
          k,
          _,
          C,
          E,
          N
        );
        break;
      default:
        L & 1 ? $(
          d,
          p,
          w,
          T,
          k,
          _,
          C,
          E,
          N
        ) : L & 6 ? ue(
          d,
          p,
          w,
          T,
          k,
          _,
          C,
          E,
          N
        ) : (L & 64 || L & 128) && v.process(
          d,
          p,
          w,
          T,
          k,
          _,
          C,
          E,
          N,
          ts
        );
    }
    D != null && k ? ds(D, d && d.ref, _, p || d, !p) : D == null && d && d.ref != null && ds(d.ref, null, _, d, !0);
  }, y = (d, p, w, T) => {
    if (d == null)
      n(
        p.el = l(p.children),
        w,
        T
      );
    else {
      const k = p.el = d.el;
      p.children !== d.children && a(k, p.children);
    }
  }, O = (d, p, w, T) => {
    d == null ? n(
      p.el = c(p.children || ""),
      w,
      T
    ) : p.el = d.el;
  }, S = (d, p, w, T) => {
    [d.el, d.anchor] = b(
      d.children,
      p,
      w,
      T,
      d.el,
      d.anchor
    );
  }, I = ({ el: d, anchor: p }, w, T) => {
    let k;
    for (; d && d !== p; )
      k = m(d), n(d, w, T), d = k;
    n(p, w, T);
  }, A = ({ el: d, anchor: p }) => {
    let w;
    for (; d && d !== p; )
      w = m(d), i(d), d = w;
    i(p);
  }, $ = (d, p, w, T, k, _, C, E, N) => {
    if (p.type === "svg" ? C = "svg" : p.type === "math" && (C = "mathml"), d == null)
      K(
        p,
        w,
        T,
        k,
        _,
        C,
        E,
        N
      );
    else {
      const v = d.el && d.el._isVueCE ? d.el : null;
      try {
        v && v._beginPatch(), q(
          d,
          p,
          k,
          _,
          C,
          E,
          N
        );
      } finally {
        v && v._endPatch();
      }
    }
  }, K = (d, p, w, T, k, _, C, E) => {
    let N, v;
    const { props: D, shapeFlag: L, transition: P, dirs: j } = d;
    if (N = d.el = o(
      d.type,
      _,
      D && D.is,
      D
    ), L & 8 ? f(N, d.children) : L & 16 && M(
      d.children,
      N,
      null,
      T,
      k,
      jn(d, _),
      C,
      E
    ), j && bt(d, null, T, "created"), x(N, d, d.scopeId, C, T), D) {
      for (const G in D)
        G !== "value" && !fs(G) && r(N, G, null, D[G], _, T);
      "value" in D && r(N, "value", null, D.value, _), (v = D.onVnodeBeforeMount) && Fe(v, T, d);
    }
    j && bt(d, null, T, "beforeMount");
    const V = ya(k, P);
    V && P.beforeEnter(N), n(N, p, w), ((v = D && D.onVnodeMounted) || V || j) && _e(() => {
      try {
        v && Fe(v, T, d), V && P.enter(N), j && bt(d, null, T, "mounted");
      } finally {
      }
    }, k);
  }, x = (d, p, w, T, k) => {
    if (w && g(d, w), T)
      for (let _ = 0; _ < T.length; _++)
        g(d, T[_]);
    if (k) {
      let _ = k.subTree;
      if (p === _ || Fo(_.type) && (_.ssContent === p || _.ssFallback === p)) {
        const C = k.vnode;
        x(
          d,
          C,
          C.scopeId,
          C.slotScopeIds,
          k.parent
        );
      }
    }
  }, M = (d, p, w, T, k, _, C, E, N = 0) => {
    for (let v = N; v < d.length; v++) {
      const D = d[v] = E ? st(d[v]) : He(d[v]);
      h(
        null,
        D,
        p,
        w,
        T,
        k,
        _,
        C,
        E
      );
    }
  }, q = (d, p, w, T, k, _, C) => {
    const E = p.el = d.el;
    let { patchFlag: N, dynamicChildren: v, dirs: D } = p;
    N |= d.patchFlag & 16;
    const L = d.props || Q, P = p.props || Q;
    let j;
    if (w && wt(w, !1), (j = P.onVnodeBeforeUpdate) && Fe(j, w, p, d), D && bt(p, d, w, "beforeUpdate"), w && wt(w, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    v && (!d.dynamicChildren || d.dynamicChildren.length !== v.length) && (N = 0, C = !1, v = null), (L.innerHTML && P.innerHTML == null || L.textContent && P.textContent == null) && f(E, ""), v ? ee(
      d.dynamicChildren,
      v,
      E,
      w,
      T,
      jn(p, k),
      _
    ) : C || X(
      d,
      p,
      E,
      null,
      w,
      T,
      jn(p, k),
      _,
      !1
    ), N > 0) {
      if (N & 16)
        pe(E, L, P, w, k);
      else if (N & 2 && L.class !== P.class && r(E, "class", null, P.class, k), N & 4 && r(E, "style", L.style, P.style, k), N & 8) {
        const V = p.dynamicProps;
        for (let G = 0; G < V.length; G++) {
          const J = V[G], oe = L[J], le = P[J];
          (le !== oe || J === "value") && r(E, J, oe, le, k, w);
        }
      }
      N & 1 && d.children !== p.children && f(E, p.children);
    } else !C && v == null && pe(E, L, P, w, k);
    ((j = P.onVnodeUpdated) || D) && _e(() => {
      j && Fe(j, w, p, d), D && bt(p, d, w, "updated");
    }, T);
  }, ee = (d, p, w, T, k, _, C) => {
    for (let E = 0; E < p.length; E++) {
      const N = d[E], v = p[E], D = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        N.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (N.type === Le || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !is(N, v) || // - In the case of a component, it could contain anything.
        N.shapeFlag & 198) ? u(N.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          w
        )
      );
      h(
        N,
        v,
        D,
        null,
        T,
        k,
        _,
        C,
        !0
      );
    }
  }, pe = (d, p, w, T, k) => {
    if (p !== w) {
      if (p !== Q)
        for (const _ in p)
          !fs(_) && !(_ in w) && r(
            d,
            _,
            p[_],
            null,
            k,
            T
          );
      for (const _ in w) {
        if (fs(_)) continue;
        const C = w[_], E = p[_];
        C !== E && _ !== "value" && r(d, _, E, C, k, T);
      }
      "value" in w && r(d, "value", p.value, w.value, k);
    }
  }, fe = (d, p, w, T, k, _, C, E, N) => {
    const v = p.el = d ? d.el : l(""), D = p.anchor = d ? d.anchor : l("");
    let { patchFlag: L, dynamicChildren: P, slotScopeIds: j } = p;
    j && (E = E ? E.concat(j) : j), d == null ? (n(v, w, T), n(D, w, T), M(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      p.children || [],
      w,
      D,
      k,
      _,
      C,
      E,
      N
    )) : L > 0 && L & 64 && P && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    d.dynamicChildren && d.dynamicChildren.length === P.length ? (ee(
      d.dynamicChildren,
      P,
      w,
      k,
      _,
      C,
      E
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (p.key != null || k && p === k.subTree) && jo(
      d,
      p,
      !0
      /* shallow */
    )) : X(
      d,
      p,
      w,
      D,
      k,
      _,
      C,
      E,
      N
    );
  }, ue = (d, p, w, T, k, _, C, E, N) => {
    p.slotScopeIds = E, d == null ? p.shapeFlag & 512 ? k.ctx.activate(
      p,
      w,
      T,
      C,
      N
    ) : Et(
      p,
      w,
      T,
      k,
      _,
      C,
      N
    ) : qi(d, p, N);
  }, Et = (d, p, w, T, k, _, C) => {
    const E = d.component = Na(
      d,
      T,
      k
    );
    if (vo(d) && (E.ctx.renderer = ts), Ca(E, !1, C), E.asyncDep) {
      if (k && k.registerDep(E, ge, C), !d.el) {
        const N = E.subTree = ot(gt);
        O(null, N, p, w), d.placeholder = N.el;
      }
    } else
      ge(
        E,
        d,
        p,
        w,
        k,
        _,
        C
      );
  }, qi = (d, p, w) => {
    const T = p.component = d.component;
    if (la(d, p, w))
      if (T.asyncDep && !T.asyncResolved) {
        te(T, p, w);
        return;
      } else
        T.next = p, T.update();
    else
      p.el = d.el, T.vnode = p;
  }, ge = (d, p, w, T, k, _, C) => {
    const E = () => {
      if (d.isMounted) {
        let { next: L, bu: P, u: j, parent: V, vnode: G } = d;
        {
          const Be = Bo(d);
          if (Be) {
            L && (L.el = G.el, te(d, L, C)), Be.asyncDep.then(() => {
              _e(() => {
                d.isUnmounted || v();
              }, k);
            });
            return;
          }
        }
        let J = L, oe;
        wt(d, !1), L ? (L.el = G.el, te(d, L, C)) : L = G, P && Ws(P), (oe = L.props && L.props.onVnodeBeforeUpdate) && Fe(oe, V, L, G), wt(d, !0);
        const le = cr(d), je = d.subTree;
        d.subTree = le, h(
          je,
          le,
          // parent may have changed if it's in a teleport
          u(je.el),
          // anchor may have changed if it's in a fragment
          Ds(je),
          d,
          k,
          _
        ), L.el = le.el, J === null && ca(d, le.el), j && _e(j, k), (oe = L.props && L.props.onVnodeUpdated) && _e(
          () => Fe(oe, V, L, G),
          k
        );
      } else {
        let L;
        const { el: P, props: j } = p, { bm: V, m: G, parent: J, root: oe, type: le } = d, je = ps(p);
        wt(d, !1), V && Ws(V), !je && (L = j && j.onVnodeBeforeMount) && Fe(L, J, p), wt(d, !0);
        {
          oe.ce && oe.ce._hasShadowRoot() && oe.ce._injectChildStyle(
            le,
            d.parent ? d.parent.type : void 0
          );
          const Be = d.subTree = cr(d);
          h(
            null,
            Be,
            w,
            T,
            d,
            k,
            _
          ), p.el = Be.el;
        }
        if (G && _e(G, k), !je && (L = j && j.onVnodeMounted)) {
          const Be = p;
          _e(
            () => Fe(L, J, Be),
            k
          );
        }
        (p.shapeFlag & 256 || J && ps(J.vnode) && J.vnode.shapeFlag & 256) && d.a && _e(d.a, k), d.isMounted = !0, p = w = T = null;
      }
    };
    d.scope.on();
    const N = d.effect = new Xr(E);
    d.scope.off();
    const v = d.update = N.run.bind(N), D = d.job = N.runIfDirty.bind(N);
    D.i = d, D.id = d.uid, N.scheduler = () => Oi(D), wt(d, !0), v();
  }, te = (d, p, w) => {
    p.component = d;
    const T = d.vnode.props;
    d.vnode = p, d.next = null, fa(d, p.props, T, w), pa(d, p.children, w), Ge(), tr(d), Qe();
  }, X = (d, p, w, T, k, _, C, E, N = !1) => {
    const v = d && d.children, D = d ? d.shapeFlag : 0, L = p.children, { patchFlag: P, shapeFlag: j } = p;
    if (P > 0) {
      if (P & 128) {
        xs(
          v,
          L,
          w,
          T,
          k,
          _,
          C,
          E,
          N
        );
        return;
      } else if (P & 256) {
        mt(
          v,
          L,
          w,
          T,
          k,
          _,
          C,
          E,
          N
        );
        return;
      }
    }
    j & 8 ? (D & 16 && es(v, k, _), L !== v && f(w, L)) : D & 16 ? j & 16 ? xs(
      v,
      L,
      w,
      T,
      k,
      _,
      C,
      E,
      N
    ) : es(v, k, _, !0) : (D & 8 && f(w, ""), j & 16 && M(
      L,
      w,
      T,
      k,
      _,
      C,
      E,
      N
    ));
  }, mt = (d, p, w, T, k, _, C, E, N) => {
    d = d || Dt, p = p || Dt;
    const v = d.length, D = p.length, L = Math.min(v, D);
    let P;
    for (P = 0; P < L; P++) {
      const j = p[P] = N ? st(p[P]) : He(p[P]);
      h(
        d[P],
        j,
        w,
        null,
        k,
        _,
        C,
        E,
        N
      );
    }
    v > D ? es(
      d,
      k,
      _,
      !0,
      !1,
      L
    ) : M(
      p,
      w,
      T,
      k,
      _,
      C,
      E,
      N,
      L
    );
  }, xs = (d, p, w, T, k, _, C, E, N) => {
    let v = 0;
    const D = p.length;
    let L = d.length - 1, P = D - 1;
    for (; v <= L && v <= P; ) {
      const j = d[v], V = p[v] = N ? st(p[v]) : He(p[v]);
      if (is(j, V))
        h(
          j,
          V,
          w,
          null,
          k,
          _,
          C,
          E,
          N
        );
      else
        break;
      v++;
    }
    for (; v <= L && v <= P; ) {
      const j = d[L], V = p[P] = N ? st(p[P]) : He(p[P]);
      if (is(j, V))
        h(
          j,
          V,
          w,
          null,
          k,
          _,
          C,
          E,
          N
        );
      else
        break;
      L--, P--;
    }
    if (v > L) {
      if (v <= P) {
        const j = P + 1, V = j < D ? p[j].el : T;
        for (; v <= P; )
          h(
            null,
            p[v] = N ? st(p[v]) : He(p[v]),
            w,
            V,
            k,
            _,
            C,
            E,
            N
          ), v++;
      }
    } else if (v > P)
      for (; v <= L; )
        Re(d[v], k, _, !0), v++;
    else {
      const j = v, V = v, G = /* @__PURE__ */ new Map();
      for (v = V; v <= P; v++) {
        const ve = p[v] = N ? st(p[v]) : He(p[v]);
        ve.key != null && G.set(ve.key, v);
      }
      let J, oe = 0;
      const le = P - V + 1;
      let je = !1, Be = 0;
      const ss = new Array(le);
      for (v = 0; v < le; v++) ss[v] = 0;
      for (v = j; v <= L; v++) {
        const ve = d[v];
        if (oe >= le) {
          Re(ve, k, _, !0);
          continue;
        }
        let Ke;
        if (ve.key != null)
          Ke = G.get(ve.key);
        else
          for (J = V; J <= P; J++)
            if (ss[J - V] === 0 && is(ve, p[J])) {
              Ke = J;
              break;
            }
        Ke === void 0 ? Re(ve, k, _, !0) : (ss[Ke - V] = v + 1, Ke >= Be ? Be = Ke : je = !0, h(
          ve,
          p[Ke],
          w,
          null,
          k,
          _,
          C,
          E,
          N
        ), oe++);
      }
      const Ji = je ? ba(ss) : Dt;
      for (J = Ji.length - 1, v = le - 1; v >= 0; v--) {
        const ve = V + v, Ke = p[ve], Yi = p[ve + 1], Gi = ve + 1 < D ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          Yi.el || Ko(Yi)
        ) : T;
        ss[v] === 0 ? h(
          null,
          Ke,
          w,
          Gi,
          k,
          _,
          C,
          E,
          N
        ) : je && (J < 0 || v !== Ji[J] ? yt(Ke, w, Gi, 2) : J--);
      }
    }
  }, yt = (d, p, w, T, k = null) => {
    const { el: _, type: C, transition: E, children: N, shapeFlag: v } = d;
    if (v & 6) {
      yt(d.component.subTree, p, w, T);
      return;
    }
    if (v & 128) {
      d.suspense.move(p, w, T);
      return;
    }
    if (v & 64) {
      C.move(d, p, w, ts);
      return;
    }
    if (C === Le) {
      n(_, p, w);
      for (let L = 0; L < N.length; L++)
        yt(N[L], p, w, T);
      n(d.anchor, p, w);
      return;
    }
    if (C === Bn) {
      I(d, p, w);
      return;
    }
    if (T !== 2 && v & 1 && E)
      if (T === 0)
        E.persisted && !_[Dn] ? n(_, p, w) : (E.beforeEnter(_), n(_, p, w), _e(() => E.enter(_), k));
      else {
        const { leave: L, delayLeave: P, afterLeave: j } = E, V = () => {
          d.ctx.isUnmounted ? i(_) : n(_, p, w);
        }, G = () => {
          const J = _._isLeaving || !!_[Dn];
          _._isLeaving && _[Dn](
            !0
            /* cancelled */
          ), E.persisted && !J ? V() : L(_, () => {
            V(), j && j();
          });
        };
        P ? P(_, V, G) : G();
      }
    else
      n(_, p, w);
  }, Re = (d, p, w, T = !1, k = !1) => {
    const {
      type: _,
      props: C,
      ref: E,
      children: N,
      dynamicChildren: v,
      shapeFlag: D,
      patchFlag: L,
      dirs: P,
      cacheIndex: j,
      memo: V
    } = d;
    if (L === -2 && (k = !1), E != null && (Ge(), ds(E, null, w, d, !0), Qe()), j != null && (p.renderCache[j] = void 0), D & 256) {
      p.ctx.deactivate(d);
      return;
    }
    const G = D & 1 && P, J = !ps(d);
    let oe;
    if (J && (oe = C && C.onVnodeBeforeUnmount) && Fe(oe, p, d), D & 6)
      Dl(d.component, w, T);
    else {
      if (D & 128) {
        d.suspense.unmount(w, T);
        return;
      }
      G && bt(d, null, p, "beforeUnmount"), D & 64 ? d.type.remove(
        d,
        p,
        w,
        ts,
        T
      ) : v && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !v.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (_ !== Le || L > 0 && L & 64) ? es(
        v,
        p,
        w,
        !1,
        !0
      ) : (_ === Le && L & 384 || !k && D & 16) && es(N, p, w), T && Hi(d);
    }
    const le = V != null && j == null;
    (J && (oe = C && C.onVnodeUnmounted) || G || le) && _e(() => {
      oe && Fe(oe, p, d), G && bt(d, null, p, "unmounted"), le && (d.el = null);
    }, w);
  }, Hi = (d) => {
    const { type: p, el: w, anchor: T, transition: k } = d;
    if (p === Le) {
      xl(w, T);
      return;
    }
    if (p === Bn) {
      A(d);
      return;
    }
    const _ = () => {
      i(w), k && !k.persisted && k.afterLeave && k.afterLeave();
    };
    if (d.shapeFlag & 1 && k && !k.persisted) {
      const { leave: C, delayLeave: E } = k, N = () => C(w, _);
      E ? E(d.el, _, N) : N();
    } else
      _();
  }, xl = (d, p) => {
    let w;
    for (; d !== p; )
      w = m(d), i(d), d = w;
    i(p);
  }, Dl = (d, p, w) => {
    const { bum: T, scope: k, job: _, subTree: C, um: E, m: N, a: v } = d;
    ur(N), ur(v), T && Ws(T), k.stop(), _ && (_.flags |= 8, Re(C, d, p, w)), E && _e(E, p), _e(() => {
      d.isUnmounted = !0;
    }, p);
  }, es = (d, p, w, T = !1, k = !1, _ = 0) => {
    for (let C = _; C < d.length; C++)
      Re(d[C], p, w, T, k);
  }, Ds = (d) => {
    if (d.shapeFlag & 6)
      return Ds(d.component.subTree);
    if (d.shapeFlag & 128)
      return d.suspense.next();
    const p = m(d.anchor || d.el), w = p && p[Lc];
    return w ? m(w) : p;
  };
  let Cn = !1;
  const Wi = (d, p, w) => {
    let T;
    d == null ? p._vnode && (Re(p._vnode, null, null, !0), T = p._vnode.component) : h(
      p._vnode || null,
      d,
      p,
      null,
      null,
      null,
      w
    ), p._vnode = d, Cn || (Cn = !0, tr(T), mo(), Cn = !1);
  }, ts = {
    p: h,
    um: Re,
    m: yt,
    r: Hi,
    mt: Et,
    mc: M,
    pc: X,
    pbc: ee,
    n: Ds,
    o: e
  };
  return {
    render: Wi,
    hydrate: void 0,
    createApp: ta(Wi)
  };
}
function jn({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function wt({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function ya(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function jo(e, t, s = !1) {
  const n = e.children, i = t.children;
  if (R(n) && R(i))
    for (let r = 0; r < n.length; r++) {
      const o = n[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = st(i[r]), l.el = o.el), !s && l.patchFlag !== -2 && jo(o, l)), l.type === wn && (l.patchFlag === -1 && (l = i[r] = st(l)), l.el = o.el), l.type === gt && !l.el && (l.el = o.el);
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
function Bo(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : Bo(t);
}
function ur(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function Ko(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? Ko(t.subTree) : null;
}
const Fo = (e) => e.__isSuspense;
function wa(e, t) {
  t && t.pendingBranch ? R(e) ? t.effects.push(...e) : t.effects.push(e) : Tc(e);
}
const Le = /* @__PURE__ */ Symbol.for("v-fgt"), wn = /* @__PURE__ */ Symbol.for("v-txt"), gt = /* @__PURE__ */ Symbol.for("v-cmt"), Bn = /* @__PURE__ */ Symbol.for("v-stc"), ms = [];
let ke = null;
function Ue(e = !1) {
  ms.push(ke = e ? null : []);
}
function Sa() {
  ms.pop(), ke = ms[ms.length - 1] || null;
}
let vs = 1;
function hr(e, t = !1) {
  vs += e, e < 0 && ke && t && (ke.hasOnce = !0);
}
function Uo(e) {
  return e.dynamicChildren = vs > 0 ? ke || Dt : null, Sa(), vs > 0 && ke && ke.push(e), e;
}
function Ze(e, t, s, n, i, r) {
  return Uo(
    U(
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
  return Uo(
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
function Vo(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function is(e, t) {
  return e.type === t.type && e.key === t.key;
}
const qo = ({ key: e }) => e ?? null, Ys = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? se(e) || /* @__PURE__ */ de(e) || F(e) ? { i: Te, r: e, k: t, f: !!s } : e : null);
function U(e, t = null, s = null, n = 0, i = null, r = e === Le ? 0 : 1, o = !1, l = !1) {
  const c = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && qo(t),
    ref: t && Ys(t),
    scopeId: bo,
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
    ctx: Te
  };
  return l ? (nn(c, s), r & 128 && e.normalize(c)) : s && (c.shapeFlag |= se(s) ? 8 : 16), vs > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  ke && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (c.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  c.patchFlag !== 32 && ke.push(c), c;
}
const ot = va;
function va(e, t = null, s = null, n = 0, i = null, r = !1) {
  if ((!e || e === Wc) && (e = gt), Vo(e)) {
    const l = Vt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && nn(l, s), vs > 0 && !r && ke && (l.shapeFlag & 6 ? ke[ke.indexOf(e)] = l : ke.push(l)), l.patchFlag = -2, l;
  }
  if (Ma(e) && (e = e.__vccOpts), t) {
    t = ka(t);
    let { class: l, style: c } = t;
    l && !se(l) && (t.class = gi(l)), Y(c) && (/* @__PURE__ */ ki(c) && !R(c) && (c = ae({}, c)), t.style = pi(c));
  }
  const o = se(e) ? 1 : Fo(e) ? 128 : $c(e) ? 64 : Y(e) ? 4 : F(e) ? 2 : 0;
  return U(
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
  return e ? /* @__PURE__ */ ki(e) || $o(e) ? ae({}, e) : e : null;
}
function Vt(e, t, s = !1, n = !1) {
  const { props: i, ref: r, patchFlag: o, children: l, transition: c } = e, a = t ? Oa(i || {}, t) : i, f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: a,
    key: a && qo(a),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && r ? R(r) ? r.concat(Ys(t)) : [r, Ys(t)] : Ys(t)
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
    patchFlag: t && e.type !== Le ? o === -1 ? 16 : o | 16 : o,
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
    ssContent: e.ssContent && Vt(e.ssContent),
    ssFallback: e.ssFallback && Vt(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return c && n && Ti(
    f,
    c.clone(f)
  ), f;
}
function Ho(e = " ", t = 0) {
  return ot(wn, null, e, t);
}
function Ks(e = "", t = !1) {
  return t ? (Ue(), _a(gt, null, e)) : ot(gt, null, e);
}
function He(e) {
  return e == null || typeof e == "boolean" ? ot(gt) : R(e) ? ot(
    Le,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Vo(e) ? st(e) : ot(wn, null, String(e));
}
function st(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : Vt(e);
}
function nn(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (R(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), nn(e, i()), i._c && (i._d = !0));
      return;
    } else {
      s = 32;
      const i = t._;
      !i && !$o(t) ? t._ctx = Te : i === 3 && Te && (Te.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (F(t)) {
    if (n & 65) {
      nn(e, { default: t });
      return;
    }
    t = { default: t, _ctx: Te }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [Ho(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function Oa(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const i in n)
      if (i === "class")
        t.class !== n.class && (t.class = gi([t.class, n.class]));
      else if (i === "style")
        t.style = pi([t.style, n.style]);
      else if (an(i)) {
        const r = t[i], o = n[i];
        o && r !== o && !(R(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !fn(i) && (t[i] = o);
      } else i !== "" && (t[i] = n[i]);
  }
  return t;
}
function Fe(e, t, s, n = null) {
  xe(e, t, 7, [
    s,
    n
  ]);
}
const Ta = No();
let Aa = 0;
function Na(e, t, s) {
  const n = e.type, i = (t ? t.appContext : e.appContext) || Ta, r = {
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
    scope: new Yl(
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
    propsOptions: Po(n, i),
    emitsOptions: Eo(n, i),
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
let we = null;
const Ea = () => we || Te;
let rn, ni;
{
  const e = dn(), t = (s, n) => {
    let i;
    return (i = e[s]) || (i = e[s] = []), i.push(n), (r) => {
      i.length > 1 ? i.forEach((o) => o(r)) : i[0](r);
    };
  };
  rn = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => we = s
  ), ni = t(
    "__VUE_SSR_SETTERS__",
    (s) => ks = s
  );
}
const Cs = (e) => {
  const t = we;
  return rn(e), e.scope.on(), () => {
    e.scope.off(), rn(t);
  };
}, dr = () => {
  we && we.scope.off(), rn(null);
};
function Wo(e) {
  return e.vnode.shapeFlag & 4;
}
let ks = !1;
function Ca(e, t = !1, s = !1) {
  t && ni(t);
  const { props: n, children: i } = e.vnode, r = Wo(e);
  aa(e, n, r, t), da(e, i, s || t);
  const o = r ? Ia(e, t) : void 0;
  return t && ni(!1), o;
}
function Ia(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, Yc);
  const { setup: n } = s;
  if (n) {
    Ge();
    const i = e.setupContext = n.length > 1 ? $a(e) : null, r = Cs(e), o = Es(
      n,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = Ur(o);
    if (Qe(), r(), (l || e.sp) && !ps(e) && _o(e), l) {
      if (o.then(dr, dr), t)
        return o.then((c) => {
          pr(e, c);
        }).catch((c) => {
          mn(c, e, 0);
        });
      e.asyncDep = o;
    } else
      pr(e, o);
  } else
    Jo(e);
}
function pr(e, t, s) {
  F(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : Y(t) && (e.setupState = uo(t)), Jo(e);
}
function Jo(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Je);
  {
    const i = Cs(e);
    Ge();
    try {
      Gc(e);
    } finally {
      Qe(), i();
    }
  }
}
const La = {
  get(e, t) {
    return he(e, "get", ""), e[t];
  }
};
function $a(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, La),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function Sn(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(uo(pc(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in gs)
        return gs[s](e);
    },
    has(t, s) {
      return s in t || s in gs;
    }
  })) : e.proxy;
}
function Ma(e) {
  return F(e) && "__vccOpts" in e;
}
const Pa = (e, t) => /* @__PURE__ */ Sc(e, t, ks), xa = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let ii;
const gr = typeof window < "u" && window.trustedTypes;
if (gr)
  try {
    ii = /* @__PURE__ */ gr.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Yo = ii ? (e) => ii.createHTML(e) : (e) => e, Da = "http://www.w3.org/2000/svg", Ra = "http://www.w3.org/1998/Math/MathML", tt = typeof document < "u" ? document : null, mr = tt && /* @__PURE__ */ tt.createElement("template"), ja = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const i = t === "svg" ? tt.createElementNS(Da, e) : t === "mathml" ? tt.createElementNS(Ra, e) : s ? tt.createElement(e, { is: s }) : tt.createElement(e);
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
      mr.innerHTML = Yo(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = mr.content;
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
}, Ba = /* @__PURE__ */ Symbol("_vtc");
function Ka(e, t, s) {
  const n = e[Ba];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const yr = /* @__PURE__ */ Symbol("_vod"), Fa = /* @__PURE__ */ Symbol("_vsh"), Ua = /* @__PURE__ */ Symbol(""), Va = /(?:^|;)\s*display\s*:/;
function qa(e, t, s) {
  const n = e.style, i = se(s);
  let r = !1;
  if (s && !i) {
    if (t)
      if (se(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && ls(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && ls(n, o, "");
    for (const o in s) {
      o === "display" && (r = !0);
      const l = s[o];
      l != null ? Wa(
        e,
        o,
        !se(t) && t ? t[o] : void 0,
        l
      ) || ls(n, o, l) : ls(n, o, "");
    }
  } else if (i) {
    if (t !== s) {
      const o = n[Ua];
      o && (s += ";" + o), n.cssText = s, r = Va.test(s);
    }
  } else t && e.removeAttribute("style");
  yr in e && (e[yr] = r ? n.display : "", e[Fa] && (n.display = "none"));
}
const br = /\s*!important$/;
function ls(e, t, s) {
  if (R(s))
    s.forEach((n) => ls(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = Ha(e, t);
    br.test(s) ? e.setProperty(
      Nt(n),
      s.replace(br, ""),
      "important"
    ) : e[n] = s;
  }
}
const wr = ["Webkit", "Moz", "ms"], Kn = {};
function Ha(e, t) {
  const s = Kn[t];
  if (s)
    return s;
  let n = $e(t);
  if (n !== "filter" && n in e)
    return Kn[t] = n;
  n = Hr(n);
  for (let i = 0; i < wr.length; i++) {
    const r = wr[i] + n;
    if (r in e)
      return Kn[t] = r;
  }
  return t;
}
function Wa(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && se(n) && s === n;
}
const Sr = "http://www.w3.org/1999/xlink";
function _r(e, t, s, n, i, r = Wl(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Sr, t.slice(6, t.length)) : e.setAttributeNS(Sr, t, s) : s == null || r && !Jr(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : Ye(s) ? String(s) : s
  );
}
function vr(e, t, s, n, i) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? Yo(s) : s);
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
    l === "boolean" ? s = Jr(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
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
function Ja(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const kr = /* @__PURE__ */ Symbol("_vei");
function Ya(e, t, s, n, i = null) {
  const r = e[kr] || (e[kr] = {}), o = r[t];
  if (n && o)
    o.value = n;
  else {
    const [l, c] = Xa(t);
    if (n) {
      const a = r[t] = ef(
        n,
        i
      );
      dt(e, l, a, c);
    } else o && (Ja(e, l, o, c), r[t] = void 0);
  }
}
const Ga = /(Once|Passive|Capture)$/, Qa = /^on:?(?:Once|Passive|Capture)$/;
function Xa(e) {
  let t, s;
  for (; (s = e.match(Ga)) && !Qa.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : Nt(e.slice(2)), t];
}
let Fn = 0;
const za = /* @__PURE__ */ Promise.resolve(), Za = () => Fn || (za.then(() => Fn = 0), Fn = Date.now());
function ef(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const i = s.value;
    if (R(i)) {
      const r = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        r.call(n), n._stopped = !0;
      };
      const o = i.slice(), l = [n];
      for (let c = 0; c < o.length && !n._stopped; c++) {
        const a = o[c];
        a && xe(
          a,
          t,
          5,
          l
        );
      }
    } else
      xe(
        i,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = Za(), s;
}
const Or = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, tf = (e, t, s, n, i, r) => {
  const o = i === "svg";
  t === "class" ? Ka(e, n, o) : t === "style" ? qa(e, s, n) : an(t) ? fn(t) || Ya(e, t, s, n, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : sf(e, t, n, o)) ? (vr(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && _r(e, t, n, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (nf(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !se(n))) ? vr(e, $e(t), n, r, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), _r(e, t, n, o));
};
function sf(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Or(t) && F(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return Or(t) && se(s) ? !1 : t in e;
}
function nf(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = $e(t);
  return Array.isArray(s) ? s.some((i) => $e(i) === n) : Object.keys(s).some((i) => $e(i) === n);
}
const qt = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return R(t) ? (s) => Ws(t, s) : t;
};
function rf(e) {
  e.target.composing = !0;
}
function Tr(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const lt = /* @__PURE__ */ Symbol("_assign");
function Ar(e, t, s) {
  return t && (e = e.trim()), s && (e = hn(e)), e;
}
const et = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, i) {
    e[lt] = qt(i);
    const r = n || i.props && i.props.type === "number";
    dt(e, t ? "change" : "input", (o) => {
      o.target.composing || e[lt](Ar(e.value, s, r));
    }), (s || r) && dt(e, "change", () => {
      e.value = Ar(e.value, s, r);
    }), t || (dt(e, "compositionstart", rf), dt(e, "compositionend", Tr), dt(e, "change", Tr));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: i, number: r } }, o) {
    if (e[lt] = qt(o), e.composing) return;
    const l = (r || e.type === "number") && !/^0\d/.test(e.value) ? hn(e.value) : e.value, c = t ?? "";
    if (l === c)
      return;
    const a = e.getRootNode();
    (a instanceof Document || a instanceof ShadowRoot) && a.activeElement === e && e.type !== "range" && (n && t === s || i && e.value.trim() === c) || (e.value = c);
  }
}, of = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, s) {
    e[lt] = qt(s), dt(e, "change", () => {
      const n = e._modelValue, i = Os(e), r = e.checked, o = e[lt];
      if (R(n)) {
        const l = mi(n, i), c = l !== -1;
        if (r && !c)
          o(n.concat(i));
        else if (!r && c) {
          const a = [...n];
          a.splice(l, 1), o(a);
        }
      } else if (Jt(n)) {
        const l = new Set(n);
        r ? l.add(i) : l.delete(i), o(l);
      } else
        o(Go(e, r));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: Nr,
  beforeUpdate(e, t, s) {
    e[lt] = qt(s), Nr(e, t, s);
  }
};
function Nr(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let i;
  if (R(t))
    i = mi(t, n.props.value) > -1;
  else if (Jt(t))
    i = t.has(n.props.value);
  else {
    if (t === s) return;
    i = Yt(t, Go(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const lf = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const i = Jt(t);
    dt(e, "change", () => {
      const r = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? hn(Os(o)) : Os(o)
      );
      e[lt](
        e.multiple ? i ? new Set(r) : r : r[0]
      ), e._assigning = !0, po(() => {
        e._assigning = !1;
      });
    }), e[lt] = qt(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    Er(e, t);
  },
  beforeUpdate(e, t, s) {
    e[lt] = qt(s);
  },
  updated(e, { value: t }) {
    e._assigning || Er(e, t);
  }
};
function Er(e, t) {
  const s = e.multiple, n = R(t);
  if (!(s && !n && !Jt(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = Os(o);
      if (s)
        if (n) {
          const c = typeof l;
          c === "string" || c === "number" ? o.selected = t.some((a) => String(a) === String(l)) : o.selected = mi(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (Yt(Os(o), t)) {
        e.selectedIndex !== i && (e.selectedIndex = i);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function Os(e) {
  return "_value" in e ? e._value : e.value;
}
function Go(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
}
const cf = /* @__PURE__ */ ae({ patchProp: tf }, ja);
let Cr;
function af() {
  return Cr || (Cr = ga(cf));
}
const ff = ((...e) => {
  const t = af().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const i = hf(n);
    if (!i) return;
    const r = t._component;
    !F(r) && !r.render && !r.template && (r.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
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
const Ei = Symbol.for("yaml.alias"), ri = Symbol.for("yaml.document"), pt = Symbol.for("yaml.map"), Qo = Symbol.for("yaml.pair"), Xe = Symbol.for("yaml.scalar"), Gt = Symbol.for("yaml.seq"), Ee = Symbol.for("yaml.node.type"), Qt = (e) => !!e && typeof e == "object" && e[Ee] === Ei, Is = (e) => !!e && typeof e == "object" && e[Ee] === ri, Ls = (e) => !!e && typeof e == "object" && e[Ee] === pt, re = (e) => !!e && typeof e == "object" && e[Ee] === Qo, Z = (e) => !!e && typeof e == "object" && e[Ee] === Xe, $s = (e) => !!e && typeof e == "object" && e[Ee] === Gt;
function ne(e) {
  if (e && typeof e == "object")
    switch (e[Ee]) {
      case pt:
      case Gt:
        return !0;
    }
  return !1;
}
function ie(e) {
  if (e && typeof e == "object")
    switch (e[Ee]) {
      case Ei:
      case pt:
      case Xe:
      case Gt:
        return !0;
    }
  return !1;
}
const Xo = (e) => (Z(e) || ne(e)) && !!e.anchor, _t = Symbol("break visit"), df = Symbol("skip children"), ys = Symbol("remove node");
function Xt(e, t) {
  const s = pf(t);
  Is(e) ? Mt(null, e.contents, s, Object.freeze([e])) === ys && (e.contents = null) : Mt(null, e, s, Object.freeze([]));
}
Xt.BREAK = _t;
Xt.SKIP = df;
Xt.REMOVE = ys;
function Mt(e, t, s, n) {
  const i = gf(e, t, s, n);
  if (ie(i) || re(i))
    return mf(e, n, i), Mt(e, i, s, n);
  if (typeof i != "symbol") {
    if (ne(t)) {
      n = Object.freeze(n.concat(t));
      for (let r = 0; r < t.items.length; ++r) {
        const o = Mt(r, t.items[r], s, n);
        if (typeof o == "number")
          r = o - 1;
        else {
          if (o === _t)
            return _t;
          o === ys && (t.items.splice(r, 1), r -= 1);
        }
      }
    } else if (re(t)) {
      n = Object.freeze(n.concat(t));
      const r = Mt("key", t.key, s, n);
      if (r === _t)
        return _t;
      r === ys && (t.key = null);
      const o = Mt("value", t.value, s, n);
      if (o === _t)
        return _t;
      o === ys && (t.value = null);
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
  if (Ls(t))
    return (i = s.Map) == null ? void 0 : i.call(s, e, t, n);
  if ($s(t))
    return (r = s.Seq) == null ? void 0 : r.call(s, e, t, n);
  if (re(t))
    return (o = s.Pair) == null ? void 0 : o.call(s, e, t, n);
  if (Z(t))
    return (l = s.Scalar) == null ? void 0 : l.call(s, e, t, n);
  if (Qt(t))
    return (c = s.Alias) == null ? void 0 : c.call(s, e, t, n);
}
function mf(e, t, s) {
  const n = t[t.length - 1];
  if (ne(n))
    n.items[e] = s;
  else if (re(n))
    e === "key" ? n.key = s : n.value = s;
  else if (Is(n))
    n.contents = s;
  else {
    const i = Qt(n) ? "alias" : "scalar";
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
class ye {
  constructor(t, s) {
    this.docStart = null, this.docEnd = !1, this.yaml = Object.assign({}, ye.defaultYaml, t), this.tags = Object.assign({}, ye.defaultTags, s);
  }
  clone() {
    const t = new ye(this.yaml, this.tags);
    return t.docStart = this.docStart, t;
  }
  /**
   * During parsing, get a Directives instance for the current document and
   * update the stream state according to the current version's spec.
   */
  atDocument() {
    const t = new ye(this.yaml, this.tags);
    switch (this.yaml.version) {
      case "1.1":
        this.atNextDocument = !0;
        break;
      case "1.2":
        this.atNextDocument = !1, this.yaml = {
          explicit: ye.defaultYaml.explicit,
          version: "1.2"
        }, this.tags = Object.assign({}, ye.defaultTags);
        break;
    }
    return t;
  }
  /**
   * @param onError - May be called even if the action was successful
   * @returns `true` on success
   */
  add(t, s) {
    this.atNextDocument && (this.yaml = { explicit: ye.defaultYaml.explicit, version: "1.1" }, this.tags = Object.assign({}, ye.defaultTags), this.atNextDocument = !1);
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
      Xt(t.contents, (o, l) => {
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
ye.defaultYaml = { explicit: !1, version: "1.2" };
ye.defaultTags = { "!!": "tag:yaml.org,2002:" };
function zo(e) {
  if (/[\x00-\x19\s,[\]{}]/.test(e)) {
    const s = `Anchor must not contain whitespace or control characters: ${JSON.stringify(e)}`;
    throw new Error(s);
  }
  return !0;
}
function Zo(e) {
  const t = /* @__PURE__ */ new Set();
  return Xt(e, {
    Value(s, n) {
      n.anchor && t.add(n.anchor);
    }
  }), t;
}
function el(e, t) {
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
      s.push(r), i ?? (i = Zo(e));
      const o = el(t, i);
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
function Pt(e, t, s, n) {
  if (n && typeof n == "object")
    if (Array.isArray(n))
      for (let i = 0, r = n.length; i < r; ++i) {
        const o = n[i], l = Pt(e, n, String(i), o);
        l === void 0 ? delete n[i] : l !== o && (n[i] = l);
      }
    else if (n instanceof Map)
      for (const i of Array.from(n.keys())) {
        const r = n.get(i), o = Pt(e, n, i, r);
        o === void 0 ? n.delete(i) : o !== r && n.set(i, o);
      }
    else if (n instanceof Set)
      for (const i of Array.from(n)) {
        const r = Pt(e, n, i, i);
        r === void 0 ? n.delete(i) : r !== i && (n.delete(i), n.add(r));
      }
    else
      for (const [i, r] of Object.entries(n)) {
        const o = Pt(e, n, i, r);
        o === void 0 ? delete n[i] : o !== r && (n[i] = o);
      }
  return e.call(t, s, n);
}
function Ne(e, t, s) {
  if (Array.isArray(e))
    return e.map((n, i) => Ne(n, String(i), s));
  if (e && typeof e.toJSON == "function") {
    if (!s || !Xo(e))
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
class Ci {
  constructor(t) {
    Object.defineProperty(this, Ee, { value: t });
  }
  /** Create a copy of this node.  */
  clone() {
    const t = Object.create(Object.getPrototypeOf(this), Object.getOwnPropertyDescriptors(this));
    return this.range && (t.range = this.range.slice()), t;
  }
  /** A plain JavaScript representation of this node. */
  toJS(t, { mapAsMap: s, maxAliasCount: n, onAnchor: i, reviver: r } = {}) {
    if (!Is(t))
      throw new TypeError("A document argument is required");
    const o = {
      anchors: /* @__PURE__ */ new Map(),
      doc: t,
      keep: !0,
      mapAsMap: s === !0,
      mapKeyWarned: !1,
      maxAliasCount: typeof n == "number" ? n : 100
    }, l = Ne(this, "", o);
    if (typeof i == "function")
      for (const { count: c, res: a } of o.anchors.values())
        i(a, c);
    return typeof r == "function" ? Pt(r, { "": l }, "", l) : l;
  }
}
class Ii extends Ci {
  constructor(t) {
    super(Ei), this.source = t, Object.defineProperty(this, "tag", {
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
    s != null && s.aliasResolveCache ? n = s.aliasResolveCache : (n = [], Xt(t, {
      Node: (r, o) => {
        (Qt(o) || Xo(o)) && n.push(o);
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
    if (l || (Ne(o, null, s), l = n.get(o)), (l == null ? void 0 : l.res) === void 0) {
      const c = "This should not happen: Alias anchor was not resolved?";
      throw new ReferenceError(c);
    }
    if (r >= 0 && (l.count += 1, l.aliasCount === 0 && (l.aliasCount = Gs(i, o, n)), l.count * l.aliasCount > r)) {
      const c = "Excessive alias count indicates a resource exhaustion attack";
      throw new ReferenceError(c);
    }
    return l.res;
  }
  toString(t, s, n) {
    const i = `*${this.source}`;
    if (t) {
      if (zo(this.source), t.options.verifyAliasOrder && !t.anchors.has(this.source)) {
        const r = `Unresolved alias (the anchor must be set before the alias): ${this.source}`;
        throw new Error(r);
      }
      if (t.implicitKey)
        return `${i} `;
    }
    return i;
  }
}
function Gs(e, t, s) {
  if (Qt(t)) {
    const n = t.resolve(e), i = s && n && s.get(n);
    return i ? i.count * i.aliasCount : 0;
  } else if (ne(t)) {
    let n = 0;
    for (const i of t.items) {
      const r = Gs(e, i, s);
      r > n && (n = r);
    }
    return n;
  } else if (re(t)) {
    const n = Gs(e, t.key, s), i = Gs(e, t.value, s);
    return Math.max(n, i);
  }
  return 1;
}
const tl = (e) => !e || typeof e != "function" && typeof e != "object";
class B extends Ci {
  constructor(t) {
    super(Xe), this.value = t;
  }
  toJSON(t, s) {
    return s != null && s.keep ? this.value : Ne(this.value, t, s);
  }
  toString() {
    return String(this.value);
  }
}
B.BLOCK_FOLDED = "BLOCK_FOLDED";
B.BLOCK_LITERAL = "BLOCK_LITERAL";
B.PLAIN = "PLAIN";
B.QUOTE_DOUBLE = "QUOTE_DOUBLE";
B.QUOTE_SINGLE = "QUOTE_SINGLE";
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
function Ts(e, t, s) {
  var u, m, g;
  if (Is(e) && (e = e.contents), ie(e))
    return e;
  if (re(e)) {
    const b = (m = (u = s.schema[pt]).createNode) == null ? void 0 : m.call(u, s.schema, null, s);
    return b.items.push(e), b;
  }
  (e instanceof String || e instanceof Number || e instanceof Boolean || typeof BigInt < "u" && e instanceof BigInt) && (e = e.valueOf());
  const { aliasDuplicateObjects: n, onAnchor: i, onTagObj: r, schema: o, sourceObjects: l } = s;
  let c;
  if (n && e && typeof e == "object") {
    if (c = l.get(e), c)
      return c.anchor ?? (c.anchor = i(e)), new Ii(c.anchor);
    c = { anchor: null, node: null }, l.set(e, c);
  }
  t != null && t.startsWith("!!") && (t = Sf + t.slice(2));
  let a = _f(e, t, o.tags);
  if (!a) {
    if (e && typeof e.toJSON == "function" && (e = e.toJSON()), !e || typeof e != "object") {
      const b = new B(e);
      return c && (c.node = b), b;
    }
    a = e instanceof Map ? o[pt] : Symbol.iterator in Object(e) ? o[Gt] : o[pt];
  }
  r && (r(a), delete s.onTagObj);
  const f = a != null && a.createNode ? a.createNode(s.schema, e, s) : typeof ((g = a == null ? void 0 : a.nodeClass) == null ? void 0 : g.from) == "function" ? a.nodeClass.from(s.schema, e, s) : new B(e);
  return t ? f.tag = t : a.default || (f.tag = a.tag), c && (c.node = f), f;
}
function on(e, t, s) {
  let n = s;
  for (let i = t.length - 1; i >= 0; --i) {
    const r = t[i];
    if (typeof r == "number" && Number.isInteger(r) && r >= 0) {
      const o = [];
      o[r] = n, n = o;
    } else
      n = /* @__PURE__ */ new Map([[r, n]]);
  }
  return Ts(n, void 0, {
    aliasDuplicateObjects: !1,
    keepUndefined: !1,
    onAnchor: () => {
      throw new Error("This should not happen, please report a bug.");
    },
    schema: e,
    sourceObjects: /* @__PURE__ */ new Map()
  });
}
const cs = (e) => e == null || typeof e == "object" && !!e[Symbol.iterator]().next().done;
class sl extends Ci {
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
    if (cs(t))
      this.add(s);
    else {
      const [n, ...i] = t, r = this.get(n, !0);
      if (ne(r))
        r.addIn(i, s);
      else if (r === void 0 && this.schema)
        this.set(n, on(this.schema, i, s));
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
        this.set(n, on(this.schema, i, s));
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
` + rt(s, t) : (e.endsWith(" ") ? "" : " ") + s, nl = "flow", oi = "block", Qs = "quoted";
function _n(e, t, s = "flow", { indentAtStart: n, lineWidth: i = 80, minContentWidth: r = 20, onFold: o, onOverflow: l } = {}) {
  if (!i || i < 0)
    return e;
  i < r && (r = 0);
  const c = Math.max(1 + r, 1 + i - t.length);
  if (e.length <= c)
    return e;
  const a = [], f = {};
  let u = i - t.length;
  typeof n == "number" && (n > i - Math.max(2, r) ? a.push(0) : u = i - n);
  let m, g, b = !1, h = -1, y = -1, O = -1;
  s === oi && (h = Ir(e, h, t.length), h !== -1 && (u = h + c));
  for (let I; I = e[h += 1]; ) {
    if (s === Qs && I === "\\") {
      switch (y = h, e[h + 1]) {
        case "x":
          h += 3;
          break;
        case "u":
          h += 5;
          break;
        case "U":
          h += 9;
          break;
        default:
          h += 1;
      }
      O = h;
    }
    if (I === `
`)
      s === oi && (h = Ir(e, h, t.length)), u = h + t.length + c, m = void 0;
    else {
      if (I === " " && g && g !== " " && g !== `
` && g !== "	") {
        const A = e[h + 1];
        A && A !== " " && A !== `
` && A !== "	" && (m = h);
      }
      if (h >= u)
        if (m)
          a.push(m), u = m + c, m = void 0;
        else if (s === Qs) {
          for (; g === " " || g === "	"; )
            g = I, I = e[h += 1], b = !0;
          const A = h > O + 1 ? h - 2 : y - 1;
          if (f[A])
            return e;
          a.push(A), f[A] = !0, u = A + c, m = void 0;
        } else
          b = !0;
    }
    g = I;
  }
  if (b && l && l(), a.length === 0)
    return e;
  o && o();
  let S = e.slice(0, a[0]);
  for (let I = 0; I < a.length; ++I) {
    const A = a[I], $ = a[I + 1] || e.length;
    A === 0 ? S = `
${t}${e.slice(0, $)}` : (s === Qs && f[A] && (S += `${e[A]}\\`), S += `
${t}${e.slice(A + 1, $)}`);
  }
  return S;
}
function Ir(e, t, s) {
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
const vn = (e, t) => ({
  indentAtStart: t ? e.indent.length : e.indentAtStart,
  lineWidth: e.options.lineWidth,
  minContentWidth: e.options.minContentWidth
}), kn = (e) => /^(%|---|\.\.\.)/m.test(e);
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
function bs(e, t) {
  const s = JSON.stringify(e);
  if (t.options.doubleQuotedAsJSON)
    return s;
  const { implicitKey: n } = t, i = t.options.doubleQuotedMinMultiLineLength, r = t.indent || (kn(e) ? "  " : "");
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
  return o = l ? o + s.slice(l) : s, n ? o : _n(o, r, Qs, vn(t, !1));
}
function li(e, t) {
  if (t.options.singleQuote === !1 || t.implicitKey && e.includes(`
`) || /[ \t]\n|\n[ \t]/.test(e))
    return bs(e, t);
  const s = t.indent || (kn(e) ? "  " : ""), n = "'" + e.replace(/'/g, "''").replace(/\n+/g, `$&
${s}`) + "'";
  return t.implicitKey ? n : _n(n, s, nl, vn(t, !1));
}
function xt(e, t) {
  const { singleQuote: s } = t.options;
  let n;
  if (s === !1)
    n = bs;
  else {
    const i = e.includes('"'), r = e.includes("'");
    i && !r ? n = li : r && !i ? n = bs : n = s ? li : bs;
  }
  return n(e, t);
}
let ci;
try {
  ci = new RegExp(`(^|(?<!
))
+(?!
|$)`, "g");
} catch {
  ci = /\n+(?!\n|$)/g;
}
function Xs({ comment: e, type: t, value: s }, n, i, r) {
  const { blockQuote: o, commentString: l, lineWidth: c } = n.options;
  if (!o || /\n[\t ]+$/.test(s))
    return xt(s, n);
  const a = n.indent || (n.forceBlockIndent || kn(s) ? "  " : ""), f = o === "literal" ? !0 : o === "folded" || t === B.BLOCK_FOLDED ? !1 : t === B.BLOCK_LITERAL ? !0 : !kf(s, c, a.length);
  if (!s)
    return f ? `|
` : `>
`;
  let u, m;
  for (m = s.length; m > 0; --m) {
    const $ = s[m - 1];
    if ($ !== `
` && $ !== "	" && $ !== " ")
      break;
  }
  let g = s.substring(m);
  const b = g.indexOf(`
`);
  b === -1 ? u = "-" : s === g || b !== g.length - 1 ? (u = "+", r && r()) : u = "", g && (s = s.slice(0, -g.length), g[g.length - 1] === `
` && (g = g.slice(0, -1)), g = g.replace(ci, `$&${a}`));
  let h = !1, y, O = -1;
  for (y = 0; y < s.length; ++y) {
    const $ = s[y];
    if ($ === " ")
      h = !0;
    else if ($ === `
`)
      O = y;
    else
      break;
  }
  let S = s.substring(0, O < y ? O + 1 : y);
  S && (s = s.substring(S.length), S = S.replace(/\n+/g, `$&${a}`));
  let A = (h ? a ? "2" : "1" : "") + u;
  if (e && (A += " " + l(e.replace(/ ?[\r\n]+/g, " ")), i && i()), !f) {
    const $ = s.replace(/\n+/g, `
$&`).replace(/(?:^|\n)([\t ].*)(?:([\n\t ]*)\n(?![\n\t ]))?/g, "$1$2").replace(/\n+/g, `$&${a}`);
    let K = !1;
    const x = vn(n, !0);
    o !== "folded" && t !== B.BLOCK_FOLDED && (x.onOverflow = () => {
      K = !0;
    });
    const M = _n(`${S}${$}${g}`, a, oi, x);
    if (!K)
      return `>${A}
${a}${M}`;
  }
  return s = s.replace(/\n+/g, `$&${a}`), `|${A}
${a}${S}${s}${g}`;
}
function Of(e, t, s, n) {
  const { type: i, value: r } = e, { actualString: o, implicitKey: l, indent: c, indentStep: a, inFlow: f } = t;
  if (l && r.includes(`
`) || f && /[[\]{},]/.test(r))
    return xt(r, t);
  if (/^[\n\t ,[\]{}#&*!|>'"%@`]|^[?-]$|^[?-][ \t]|[\n:][ \t]|[ \t]\n|[\n\t ]#|[\n\t :]$/.test(r))
    return l || f || !r.includes(`
`) ? xt(r, t) : Xs(e, t, s, n);
  if (!l && !f && i !== B.PLAIN && r.includes(`
`))
    return Xs(e, t, s, n);
  if (kn(r)) {
    if (c === "")
      return t.forceBlockIndent = !0, Xs(e, t, s, n);
    if (l && c === a)
      return xt(r, t);
  }
  const u = r.replace(/\n+/g, `$&
${c}`);
  if (o) {
    const m = (h) => {
      var y;
      return h.default && h.tag !== "tag:yaml.org,2002:str" && ((y = h.test) == null ? void 0 : y.test(u));
    }, { compat: g, tags: b } = t.doc.schema;
    if (b.some(m) || g != null && g.some(m))
      return xt(r, t);
  }
  return l ? u : _n(u, c, nl, vn(t, !1));
}
function Li(e, t, s, n) {
  const { implicitKey: i, inFlow: r } = t, o = typeof e.value == "string" ? e : Object.assign({}, e, { value: String(e.value) });
  let { type: l } = e;
  l !== B.QUOTE_DOUBLE && /[\x00-\x08\x0b-\x1f\x7f-\x9f\u{D800}-\u{DFFF}]/u.test(o.value) && (l = B.QUOTE_DOUBLE);
  const c = (f) => {
    switch (f) {
      case B.BLOCK_FOLDED:
      case B.BLOCK_LITERAL:
        return i || r ? xt(o.value, t) : Xs(o, t, s, n);
      case B.QUOTE_DOUBLE:
        return bs(o.value, t);
      case B.QUOTE_SINGLE:
        return li(o.value, t);
      case B.PLAIN:
        return Of(o, t, s, n);
      default:
        return null;
    }
  };
  let a = c(l);
  if (a === null) {
    const { defaultKeyType: f, defaultStringType: u } = t.options, m = i && f || u;
    if (a = c(m), a === null)
      throw new Error(`Unsupported default string type ${m}`);
  }
  return a;
}
function il(e, t) {
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
function Af(e, t, { anchors: s, doc: n }) {
  if (!n.directives)
    return "";
  const i = [], r = (Z(e) || ne(e)) && e.anchor;
  r && zo(r) && (s.add(r), i.push(`&${r}`));
  const o = e.tag ?? (t.default ? null : t.tag);
  return o && i.push(n.directives.tagString(o)), i.join(" ");
}
function Ht(e, t, s, n) {
  var c;
  if (re(e))
    return e.toString(t, s, n);
  if (Qt(e)) {
    if (t.doc.directives)
      return e.toString(t);
    if ((c = t.resolvedAliases) != null && c.has(e))
      throw new TypeError("Cannot stringify circular structure without alias nodes");
    t.resolvedAliases ? t.resolvedAliases.add(e) : t.resolvedAliases = /* @__PURE__ */ new Set([e]), e = e.resolve(t.doc);
  }
  let i;
  const r = ie(e) ? e : t.doc.createNode(e, { onTagObj: (a) => i = a });
  i ?? (i = Tf(t.doc.schema.tags, r));
  const o = Af(r, i, t);
  o.length > 0 && (t.indentAtStart = (t.indentAtStart ?? 0) + o.length + 1);
  const l = typeof i.stringify == "function" ? i.stringify(r, t, s, n) : Z(r) ? Li(r, t, s, n) : r.toString(t, s, n);
  return o ? Z(r) || l[0] === "{" || l[0] === "[" ? `${o} ${l}` : `${o}
${t.indent}${l}` : l;
}
function Nf({ key: e, value: t }, s, n, i) {
  const { allNullValues: r, doc: o, indent: l, indentStep: c, options: { commentString: a, indentSeq: f, simpleKeys: u } } = s;
  let m = ie(e) && e.comment || null;
  if (u) {
    if (m)
      throw new Error("With simple keys, key nodes cannot have comments");
    if (ne(e) || !ie(e) && typeof e == "object") {
      const x = "With simple keys, collection cannot be used as a key value";
      throw new Error(x);
    }
  }
  let g = !u && (!e || m && t == null && !s.inFlow || ne(e) || (Z(e) ? e.type === B.BLOCK_FOLDED || e.type === B.BLOCK_LITERAL : typeof e == "object"));
  s = Object.assign({}, s, {
    allNullValues: !1,
    implicitKey: !g && (u || !r),
    indent: l + c
  });
  let b = !1, h = !1, y = Ht(e, s, () => b = !0, () => h = !0);
  if (!g && !s.inFlow && y.length > 1024) {
    if (u)
      throw new Error("With simple keys, single line scalar must not span more than 1024 characters");
    g = !0;
  }
  if (s.inFlow) {
    if (r || t == null)
      return b && n && n(), y === "" ? "?" : g ? `? ${y}` : y;
  } else if (r && !u || t == null && g)
    return y = `? ${y}`, m && !b ? y += vt(y, s.indent, a(m)) : h && i && i(), y;
  b && (m = null), g ? (m && (y += vt(y, s.indent, a(m))), y = `? ${y}
${l}:`) : (y = `${y}:`, m && (y += vt(y, s.indent, a(m))));
  let O, S, I;
  ie(t) ? (O = !!t.spaceBefore, S = t.commentBefore, I = t.comment) : (O = !1, S = null, I = null, t && typeof t == "object" && (t = o.createNode(t))), s.implicitKey = !1, !g && !m && Z(t) && (s.indentAtStart = y.length + 1), h = !1, !f && c.length >= 2 && !s.inFlow && !g && $s(t) && !t.flow && !t.tag && !t.anchor && (s.indent = s.indent.substring(2));
  let A = !1;
  const $ = Ht(t, s, () => A = !0, () => h = !0);
  let K = " ";
  if (m || O || S) {
    if (K = O ? `
` : "", S) {
      const x = a(S);
      K += `
${rt(x, s.indent)}`;
    }
    $ === "" && !s.inFlow ? K === `
` && I && (K = `

`) : K += `
${s.indent}`;
  } else if (!g && ne(t)) {
    const x = $[0], M = $.indexOf(`
`), q = M !== -1, ee = s.inFlow ?? t.flow ?? t.items.length === 0;
    if (q || !ee) {
      let pe = !1;
      if (q && (x === "&" || x === "!")) {
        let fe = $.indexOf(" ");
        x === "&" && fe !== -1 && fe < M && $[fe + 1] === "!" && (fe = $.indexOf(" ", fe + 1)), (fe === -1 || M < fe) && (pe = !0);
      }
      pe || (K = `
${s.indent}`);
    }
  } else ($ === "" || $[0] === `
`) && (K = "");
  return y += K + $, s.inFlow ? A && n && n() : I && !A ? y += vt(y, s.indent, a(I)) : h && i && i(), y;
}
function rl(e, t) {
  (e === "debug" || e === "warn") && console.warn(t);
}
const Fs = "<<", ct = {
  identify: (e) => e === Fs || typeof e == "symbol" && e.description === Fs,
  default: "key",
  tag: "tag:yaml.org,2002:merge",
  test: /^<<$/,
  resolve: () => Object.assign(new B(Symbol(Fs)), {
    addToJSMap: ol
  }),
  stringify: () => Fs
}, Ef = (e, t) => (ct.identify(t) || Z(t) && (!t.type || t.type === B.PLAIN) && ct.identify(t.value)) && (e == null ? void 0 : e.doc.schema.tags.some((s) => s.tag === ct.tag && s.default));
function ol(e, t, s) {
  const n = ll(e, s);
  if ($s(n))
    for (const i of n.items)
      Un(e, t, i);
  else if (Array.isArray(n))
    for (const i of n)
      Un(e, t, i);
  else
    Un(e, t, n);
}
function Un(e, t, s) {
  const n = ll(e, s);
  if (!Ls(n))
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
function ll(e, t) {
  return e && Qt(t) ? t.resolve(e.doc, e) : t;
}
function cl(e, t, { key: s, value: n }) {
  if (ie(s) && s.addToJSMap)
    s.addToJSMap(e, t, n);
  else if (Ef(e, s))
    ol(e, t, n);
  else {
    const i = Ne(s, "", e);
    if (t instanceof Map)
      t.set(i, Ne(n, i, e));
    else if (t instanceof Set)
      t.add(i);
    else {
      const r = Cf(s, i, e), o = Ne(n, r, e);
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
    const n = il(s.doc, {});
    n.anchors = /* @__PURE__ */ new Set();
    for (const r of s.anchors.keys())
      n.anchors.add(r.anchor);
    n.inFlow = !0, n.inStringifyKey = !0;
    const i = e.toString(n);
    if (!s.mapKeyWarned) {
      let r = JSON.stringify(i);
      r.length > 40 && (r = r.substring(0, 36) + '..."'), rl(s.doc.options.logLevel, `Keys with collection values will be stringified due to JS Object restrictions: ${r}. Set mapAsMap: true to use object keys.`), s.mapKeyWarned = !0;
    }
    return i;
  }
  return JSON.stringify(t);
}
function $i(e, t, s) {
  const n = Ts(e, void 0, s), i = Ts(t, void 0, s);
  return new Se(n, i);
}
class Se {
  constructor(t, s = null) {
    Object.defineProperty(this, Ee, { value: Qo }), this.key = t, this.value = s;
  }
  clone(t) {
    let { key: s, value: n } = this;
    return ie(s) && (s = s.clone(t)), ie(n) && (n = n.clone(t)), new Se(s, n);
  }
  toJSON(t, s) {
    const n = s != null && s.mapAsMap ? /* @__PURE__ */ new Map() : {};
    return cl(s, n, this);
  }
  toString(t, s, n) {
    return t != null && t.doc ? Nf(this, t, s, n) : JSON.stringify(this);
  }
}
function al(e, t, s) {
  return (t.inFlow ?? e.flow ? Lf : If)(e, t, s);
}
function If({ comment: e, items: t }, s, { blockItemPrefix: n, flowChars: i, itemIndent: r, onChompKeep: o, onComment: l }) {
  const { indent: c, options: { commentString: a } } = s, f = Object.assign({}, s, { indent: r, type: null });
  let u = !1;
  const m = [];
  for (let b = 0; b < t.length; ++b) {
    const h = t[b];
    let y = null;
    if (ie(h))
      !u && h.spaceBefore && m.push(""), ln(s, m, h.commentBefore, u), h.comment && (y = h.comment);
    else if (re(h)) {
      const S = ie(h.key) ? h.key : null;
      S && (!u && S.spaceBefore && m.push(""), ln(s, m, S.commentBefore, u));
    }
    u = !1;
    let O = Ht(h, f, () => y = null, () => u = !0);
    y && (O += vt(O, r, a(y))), u && y && (u = !1), m.push(n + O);
  }
  let g;
  if (m.length === 0)
    g = i.start + i.end;
  else {
    g = m[0];
    for (let b = 1; b < m.length; ++b) {
      const h = m[b];
      g += h ? `
${c}${h}` : `
`;
    }
  }
  return e ? (g += `
` + rt(a(e), c), l && l()) : u && o && o(), g;
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
  for (let b = 0; b < e.length; ++b) {
    const h = e[b];
    let y = null;
    if (ie(h))
      h.spaceBefore && u.push(""), ln(t, u, h.commentBefore, !1), h.comment && (y = h.comment);
    else if (re(h)) {
      const S = ie(h.key) ? h.key : null;
      S && (S.spaceBefore && u.push(""), ln(t, u, S.commentBefore, !1), S.comment && (a = !0));
      const I = ie(h.value) ? h.value : null;
      I ? (I.comment && (y = I.comment), I.commentBefore && (a = !0)) : h.value == null && (S != null && S.comment) && (y = S.comment);
    }
    y && (a = !0);
    let O = Ht(h, c, () => y = null);
    a || (a = u.length > f || O.includes(`
`)), b < e.length - 1 ? O += "," : t.options.trailingComma && (t.options.lineWidth > 0 && (a || (a = u.reduce((S, I) => S + I.length + 2, 2) + (O.length + 2) > t.options.lineWidth)), a && (O += ",")), y && (O += vt(O, n, l(y))), u.push(O), f = u.length;
  }
  const { start: m, end: g } = s;
  if (u.length === 0)
    return m + g;
  if (!a) {
    const b = u.reduce((h, y) => h + y.length + 2, 2);
    a = t.options.lineWidth > 0 && b > t.options.lineWidth;
  }
  if (a) {
    let b = m;
    for (const h of u)
      b += h ? `
${r}${i}${h}` : `
`;
    return `${b}
${i}${g}`;
  } else
    return `${m}${o}${u.join(" ")}${o}${g}`;
}
function ln({ indent: e, options: { commentString: t } }, s, n, i) {
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
class Oe extends sl {
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
      (a !== void 0 || i) && o.items.push($i(c, a, n));
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
    re(t) ? n = t : !t || typeof t != "object" || !("key" in t) ? n = new Se(t, t == null ? void 0 : t.value) : n = new Se(t.key, t.value);
    const i = kt(this.items, n.key), r = (o = this.schema) == null ? void 0 : o.sortMapEntries;
    if (i) {
      if (!s)
        throw new Error(`Key ${n.key} already set`);
      Z(i.value) && tl(n.value) ? i.value.value = n.value : i.value = n.value;
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
    this.add(new Se(t, s), !0);
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
      cl(s, i, r);
    return i;
  }
  toString(t, s, n) {
    if (!t)
      return JSON.stringify(this);
    for (const i of this.items)
      if (!re(i))
        throw new Error(`Map items must all be pairs; found ${JSON.stringify(i)} instead`);
    return !t.allNullValues && this.hasAllNullValues(!1) && (t = Object.assign({}, t, { allNullValues: !0 })), al(this, t, {
      blockItemPrefix: "",
      flowChars: { start: "{", end: "}" },
      itemIndent: t.indent || "",
      onChompKeep: n,
      onComment: s
    });
  }
}
const zt = {
  collection: "map",
  default: !0,
  nodeClass: Oe,
  tag: "tag:yaml.org,2002:map",
  resolve(e, t) {
    return Ls(e) || t("Expected a mapping for this tag"), e;
  },
  createNode: (e, t, s) => Oe.from(e, t, s)
};
class At extends sl {
  static get tagName() {
    return "tag:yaml.org,2002:seq";
  }
  constructor(t) {
    super(Gt, t), this.items = [];
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
    const s = Us(t);
    return typeof s != "number" ? !1 : this.items.splice(s, 1).length > 0;
  }
  get(t, s) {
    const n = Us(t);
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
    const s = Us(t);
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
    const n = Us(t);
    if (typeof n != "number")
      throw new Error(`Expected a valid index, not ${t}.`);
    const i = this.items[n];
    Z(i) && tl(s) ? i.value = s : this.items[n] = s;
  }
  toJSON(t, s) {
    const n = [];
    s != null && s.onCreate && s.onCreate(n);
    let i = 0;
    for (const r of this.items)
      n.push(Ne(r, String(i++), s));
    return n;
  }
  toString(t, s, n) {
    return t ? al(this, t, {
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
        r.items.push(Ts(l, void 0, n));
      }
    }
    return r;
  }
}
function Us(e) {
  let t = Z(e) ? e.value : e;
  return t && typeof t == "string" && (t = Number(t)), typeof t == "number" && Number.isInteger(t) && t >= 0 ? t : null;
}
const Zt = {
  collection: "seq",
  default: !0,
  nodeClass: At,
  tag: "tag:yaml.org,2002:seq",
  resolve(e, t) {
    return $s(e) || t("Expected a sequence for this tag"), e;
  },
  createNode: (e, t, s) => At.from(e, t, s)
}, On = {
  identify: (e) => typeof e == "string",
  default: !0,
  tag: "tag:yaml.org,2002:str",
  resolve: (e) => e,
  stringify(e, t, s, n) {
    return t = Object.assign({ actualString: !0 }, t), Li(e, t, s, n);
  }
}, Tn = {
  identify: (e) => e == null,
  createNode: () => new B(null),
  default: !0,
  tag: "tag:yaml.org,2002:null",
  test: /^(?:~|[Nn]ull|NULL)?$/,
  resolve: () => new B(null),
  stringify: ({ source: e }, t) => typeof e == "string" && Tn.test.test(e) ? e : t.options.nullStr
}, Mi = {
  identify: (e) => typeof e == "boolean",
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:[Tt]rue|TRUE|[Ff]alse|FALSE)$/,
  resolve: (e) => new B(e[0] === "t" || e[0] === "T"),
  stringify({ source: e, value: t }, s) {
    if (e && Mi.test.test(e)) {
      const n = e[0] === "t" || e[0] === "T";
      if (t === n)
        return e;
    }
    return t ? s.options.trueStr : s.options.falseStr;
  }
};
function De({ format: e, minFractionDigits: t, tag: s, value: n }) {
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
const fl = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^(?:[-+]?\.(?:inf|Inf|INF)|\.nan|\.NaN|\.NAN)$/,
  resolve: (e) => e.slice(-3).toLowerCase() === "nan" ? NaN : e[0] === "-" ? Number.NEGATIVE_INFINITY : Number.POSITIVE_INFINITY,
  stringify: De
}, ul = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "EXP",
  test: /^[-+]?(?:\.[0-9]+|[0-9]+(?:\.[0-9]*)?)[eE][-+]?[0-9]+$/,
  resolve: (e) => parseFloat(e),
  stringify(e) {
    const t = Number(e.value);
    return isFinite(t) ? t.toExponential() : De(e);
  }
}, hl = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^[-+]?(?:\.[0-9]+|[0-9]+\.[0-9]*)$/,
  resolve(e) {
    const t = new B(parseFloat(e)), s = e.indexOf(".");
    return s !== -1 && e[e.length - 1] === "0" && (t.minFractionDigits = e.length - s - 1), t;
  },
  stringify: De
}, An = (e) => typeof e == "bigint" || Number.isInteger(e), Pi = (e, t, s, { intAsBigInt: n }) => n ? BigInt(e) : parseInt(e.substring(t), s);
function dl(e, t, s) {
  const { value: n } = e;
  return An(n) && n >= 0 ? s + n.toString(t) : De(e);
}
const pl = {
  identify: (e) => An(e) && e >= 0,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "OCT",
  test: /^0o[0-7]+$/,
  resolve: (e, t, s) => Pi(e, 2, 8, s),
  stringify: (e) => dl(e, 8, "0o")
}, gl = {
  identify: An,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  test: /^[-+]?[0-9]+$/,
  resolve: (e, t, s) => Pi(e, 0, 10, s),
  stringify: De
}, ml = {
  identify: (e) => An(e) && e >= 0,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "HEX",
  test: /^0x[0-9a-fA-F]+$/,
  resolve: (e, t, s) => Pi(e, 2, 16, s),
  stringify: (e) => dl(e, 16, "0x")
}, $f = [
  zt,
  Zt,
  On,
  Tn,
  Mi,
  pl,
  gl,
  ml,
  fl,
  ul,
  hl
];
function Lr(e) {
  return typeof e == "bigint" || Number.isInteger(e);
}
const Vs = ({ value: e }) => JSON.stringify(e), Mf = [
  {
    identify: (e) => typeof e == "string",
    default: !0,
    tag: "tag:yaml.org,2002:str",
    resolve: (e) => e,
    stringify: Vs
  },
  {
    identify: (e) => e == null,
    createNode: () => new B(null),
    default: !0,
    tag: "tag:yaml.org,2002:null",
    test: /^null$/,
    resolve: () => null,
    stringify: Vs
  },
  {
    identify: (e) => typeof e == "boolean",
    default: !0,
    tag: "tag:yaml.org,2002:bool",
    test: /^true$|^false$/,
    resolve: (e) => e === "true",
    stringify: Vs
  },
  {
    identify: Lr,
    default: !0,
    tag: "tag:yaml.org,2002:int",
    test: /^-?(?:0|[1-9][0-9]*)$/,
    resolve: (e, t, { intAsBigInt: s }) => s ? BigInt(e) : parseInt(e, 10),
    stringify: ({ value: e }) => Lr(e) ? e.toString() : JSON.stringify(e)
  },
  {
    identify: (e) => typeof e == "number",
    default: !0,
    tag: "tag:yaml.org,2002:float",
    test: /^-?(?:0|[1-9][0-9]*)(?:\.[0-9]*)?(?:[eE][-+]?[0-9]+)?$/,
    resolve: (e) => parseFloat(e),
    stringify: Vs
  }
], Pf = {
  default: !0,
  tag: "",
  test: /^/,
  resolve(e, t) {
    return t(`Unresolved plain scalar ${JSON.stringify(e)}`), e;
  }
}, xf = [zt, Zt].concat(Mf, Pf), xi = {
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
    if (t ?? (t = B.BLOCK_LITERAL), t !== B.QUOTE_DOUBLE) {
      const c = Math.max(n.options.lineWidth - n.indent.length, n.options.minContentWidth), a = Math.ceil(l.length / c), f = new Array(a);
      for (let u = 0, m = 0; u < a; ++u, m += c)
        f[u] = l.substr(m, c);
      l = f.join(t === B.BLOCK_LITERAL ? `
` : " ");
    }
    return Li({ comment: e, type: t, value: l }, n, i, r);
  }
};
function yl(e, t) {
  if ($s(e))
    for (let s = 0; s < e.items.length; ++s) {
      let n = e.items[s];
      if (!re(n)) {
        if (Ls(n)) {
          n.items.length > 1 && t("Each pair must have its own sequence indicator");
          const i = n.items[0] || new Se(new B(null));
          if (n.commentBefore && (i.key.commentBefore = i.key.commentBefore ? `${n.commentBefore}
${i.key.commentBefore}` : n.commentBefore), n.comment) {
            const r = i.value ?? i.key;
            r.comment = r.comment ? `${n.comment}
${r.comment}` : n.comment;
          }
          n = i;
        }
        e.items[s] = re(n) ? n : new Se(n);
      }
    }
  else
    t("Expected a sequence for this tag");
  return e;
}
function bl(e, t, s) {
  const { replacer: n } = s, i = new At(e);
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
      i.items.push($i(l, c, s));
    }
  return i;
}
const Di = {
  collection: "seq",
  default: !1,
  tag: "tag:yaml.org,2002:pairs",
  resolve: yl,
  createNode: bl
};
class Kt extends At {
  constructor() {
    super(), this.add = Oe.prototype.add.bind(this), this.delete = Oe.prototype.delete.bind(this), this.get = Oe.prototype.get.bind(this), this.has = Oe.prototype.has.bind(this), this.set = Oe.prototype.set.bind(this), this.tag = Kt.tag;
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
      if (re(i) ? (r = Ne(i.key, "", s), o = Ne(i.value, r, s)) : r = Ne(i, "", s), n.has(r))
        throw new Error("Ordered maps must not include duplicate keys");
      n.set(r, o);
    }
    return n;
  }
  static from(t, s, n) {
    const i = bl(t, s, n), r = new this();
    return r.items = i.items, r;
  }
}
Kt.tag = "tag:yaml.org,2002:omap";
const Ri = {
  collection: "seq",
  identify: (e) => e instanceof Map,
  nodeClass: Kt,
  default: !1,
  tag: "tag:yaml.org,2002:omap",
  resolve(e, t) {
    const s = yl(e, t), n = [];
    for (const { key: i } of s.items)
      Z(i) && (n.includes(i.value) ? t(`Ordered maps must not include duplicate keys: ${i.value}`) : n.push(i.value));
    return Object.assign(new Kt(), s);
  },
  createNode: (e, t, s) => Kt.from(e, t, s)
};
function wl({ value: e, source: t }, s) {
  return t && (e ? Sl : _l).test.test(t) ? t : e ? s.options.trueStr : s.options.falseStr;
}
const Sl = {
  identify: (e) => e === !0,
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:Y|y|[Yy]es|YES|[Tt]rue|TRUE|[Oo]n|ON)$/,
  resolve: () => new B(!0),
  stringify: wl
}, _l = {
  identify: (e) => e === !1,
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:N|n|[Nn]o|NO|[Ff]alse|FALSE|[Oo]ff|OFF)$/,
  resolve: () => new B(!1),
  stringify: wl
}, Df = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^(?:[-+]?\.(?:inf|Inf|INF)|\.nan|\.NaN|\.NAN)$/,
  resolve: (e) => e.slice(-3).toLowerCase() === "nan" ? NaN : e[0] === "-" ? Number.NEGATIVE_INFINITY : Number.POSITIVE_INFINITY,
  stringify: De
}, Rf = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "EXP",
  test: /^[-+]?(?:[0-9][0-9_]*)?(?:\.[0-9_]*)?[eE][-+]?[0-9]+$/,
  resolve: (e) => parseFloat(e.replace(/_/g, "")),
  stringify(e) {
    const t = Number(e.value);
    return isFinite(t) ? t.toExponential() : De(e);
  }
}, jf = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^[-+]?(?:[0-9][0-9_]*)?\.[0-9_]*$/,
  resolve(e) {
    const t = new B(parseFloat(e.replace(/_/g, ""))), s = e.indexOf(".");
    if (s !== -1) {
      const n = e.substring(s + 1).replace(/_/g, "");
      n[n.length - 1] === "0" && (t.minFractionDigits = n.length);
    }
    return t;
  },
  stringify: De
}, Ms = (e) => typeof e == "bigint" || Number.isInteger(e);
function Nn(e, t, s, { intAsBigInt: n }) {
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
function ji(e, t, s) {
  const { value: n } = e;
  if (Ms(n)) {
    const i = n.toString(t);
    return n < 0 ? "-" + s + i.substr(1) : s + i;
  }
  return De(e);
}
const Bf = {
  identify: Ms,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "BIN",
  test: /^[-+]?0b[0-1_]+$/,
  resolve: (e, t, s) => Nn(e, 2, 2, s),
  stringify: (e) => ji(e, 2, "0b")
}, Kf = {
  identify: Ms,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "OCT",
  test: /^[-+]?0[0-7_]+$/,
  resolve: (e, t, s) => Nn(e, 1, 8, s),
  stringify: (e) => ji(e, 8, "0")
}, Ff = {
  identify: Ms,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  test: /^[-+]?[0-9][0-9_]*$/,
  resolve: (e, t, s) => Nn(e, 0, 10, s),
  stringify: De
}, Uf = {
  identify: Ms,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "HEX",
  test: /^[-+]?0x[0-9a-fA-F_]+$/,
  resolve: (e, t, s) => Nn(e, 2, 16, s),
  stringify: (e) => ji(e, 16, "0x")
};
class Ft extends Oe {
  constructor(t) {
    super(t), this.tag = Ft.tag;
  }
  add(t) {
    let s;
    re(t) ? s = t : t && typeof t == "object" && "key" in t && "value" in t && t.value === null ? s = new Se(t.key, null) : s = new Se(t, null), kt(this.items, s.key) || this.items.push(s);
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
    n && !s ? this.items.splice(this.items.indexOf(n), 1) : !n && s && this.items.push(new Se(t));
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
        typeof i == "function" && (o = i.call(s, o, o)), r.items.push($i(o, null, n));
    return r;
  }
}
Ft.tag = "tag:yaml.org,2002:set";
const Bi = {
  collection: "map",
  identify: (e) => e instanceof Set,
  nodeClass: Ft,
  default: !1,
  tag: "tag:yaml.org,2002:set",
  createNode: (e, t, s) => Ft.from(e, t, s),
  resolve(e, t) {
    if (Ls(e)) {
      if (e.hasAllNullValues(!0))
        return Object.assign(new Ft(), e);
      t("Set items must all have null values");
    } else
      t("Expected a mapping for this tag");
    return e;
  }
};
function Ki(e, t) {
  const s = e[0], n = s === "-" || s === "+" ? e.substring(1) : e, i = (o) => t ? BigInt(o) : Number(o), r = n.replace(/_/g, "").split(":").reduce((o, l) => o * i(60) + i(l), i(0));
  return s === "-" ? i(-1) * r : r;
}
function vl(e) {
  let { value: t } = e, s = (o) => o;
  if (typeof t == "bigint")
    s = (o) => BigInt(o);
  else if (isNaN(t) || !isFinite(t))
    return De(e);
  let n = "";
  t < 0 && (n = "-", t *= s(-1));
  const i = s(60), r = [t % i];
  return t < 60 ? r.unshift(0) : (t = (t - r[0]) / i, r.unshift(t % i), t >= 60 && (t = (t - r[0]) / i, r.unshift(t))), n + r.map((o) => String(o).padStart(2, "0")).join(":").replace(/000000\d*$/, "");
}
const kl = {
  identify: (e) => typeof e == "bigint" || Number.isInteger(e),
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "TIME",
  test: /^[-+]?[0-9][0-9_]*(?::[0-5]?[0-9])+$/,
  resolve: (e, t, { intAsBigInt: s }) => Ki(e, s),
  stringify: vl
}, Ol = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "TIME",
  test: /^[-+]?[0-9][0-9_]*(?::[0-5]?[0-9])+\.[0-9_]*$/,
  resolve: (e) => Ki(e, !1),
  stringify: vl
}, En = {
  identify: (e) => e instanceof Date,
  default: !0,
  tag: "tag:yaml.org,2002:timestamp",
  // If the time zone is omitted, the timestamp is assumed to be specified in UTC. The time part
  // may be omitted altogether, resulting in a date format. In such a case, the time part is
  // assumed to be 00:00:00Z (start of day, UTC).
  test: RegExp("^([0-9]{4})-([0-9]{1,2})-([0-9]{1,2})(?:(?:t|T|[ \\t]+)([0-9]{1,2}):([0-9]{1,2}):([0-9]{1,2}(\\.[0-9]+)?)(?:[ \\t]*(Z|[-+][012]?[0-9](?::[0-9]{2})?))?)?$"),
  resolve(e) {
    const t = e.match(En.test);
    if (!t)
      throw new Error("!!timestamp expects a date, starting with yyyy-mm-dd");
    const [, s, n, i, r, o, l] = t.map(Number), c = t[7] ? Number((t[7] + "00").substr(1, 3)) : 0;
    let a = Date.UTC(s, n - 1, i, r || 0, o || 0, l || 0, c);
    const f = t[8];
    if (f && f !== "Z") {
      let u = Ki(f, !1);
      Math.abs(u) < 30 && (u *= 60), a -= 6e4 * u;
    }
    return new Date(a);
  },
  stringify: ({ value: e }) => (e == null ? void 0 : e.toISOString().replace(/(T00:00:00)?\.000Z$/, "")) ?? ""
}, $r = [
  zt,
  Zt,
  On,
  Tn,
  Sl,
  _l,
  Bf,
  Kf,
  Ff,
  Uf,
  Df,
  Rf,
  jf,
  xi,
  ct,
  Ri,
  Di,
  Bi,
  kl,
  Ol,
  En
], Mr = /* @__PURE__ */ new Map([
  ["core", $f],
  ["failsafe", [zt, Zt, On]],
  ["json", xf],
  ["yaml11", $r],
  ["yaml-1.1", $r]
]), Pr = {
  binary: xi,
  bool: Mi,
  float: hl,
  floatExp: ul,
  floatNaN: fl,
  floatTime: Ol,
  int: gl,
  intHex: ml,
  intOct: pl,
  intTime: kl,
  map: zt,
  merge: ct,
  null: Tn,
  omap: Ri,
  pairs: Di,
  seq: Zt,
  set: Bi,
  timestamp: En
}, Vf = {
  "tag:yaml.org,2002:binary": xi,
  "tag:yaml.org,2002:merge": ct,
  "tag:yaml.org,2002:omap": Ri,
  "tag:yaml.org,2002:pairs": Di,
  "tag:yaml.org,2002:set": Bi,
  "tag:yaml.org,2002:timestamp": En
};
function Vn(e, t, s) {
  const n = Mr.get(t);
  if (n && !e)
    return s && !n.includes(ct) ? n.concat(ct) : n.slice();
  let i = n;
  if (!i)
    if (Array.isArray(e))
      i = [];
    else {
      const r = Array.from(Mr.keys()).filter((o) => o !== "yaml11").map((o) => JSON.stringify(o)).join(", ");
      throw new Error(`Unknown schema "${t}"; use one of ${r} or define customTags array`);
    }
  if (Array.isArray(e))
    for (const r of e)
      i = i.concat(r);
  else typeof e == "function" && (i = e(i.slice()));
  return s && (i = i.concat(ct)), i.reduce((r, o) => {
    const l = typeof o == "string" ? Pr[o] : o;
    if (!l) {
      const c = JSON.stringify(o), a = Object.keys(Pr).map((f) => JSON.stringify(f)).join(", ");
      throw new Error(`Unknown custom tag ${c}; use one of ${a}`);
    }
    return r.includes(l) || r.push(l), r;
  }, []);
}
const qf = (e, t) => e.key < t.key ? -1 : e.key > t.key ? 1 : 0;
class Fi {
  constructor({ compat: t, customTags: s, merge: n, resolveKnownTags: i, schema: r, sortMapEntries: o, toStringDefaults: l }) {
    this.compat = Array.isArray(t) ? Vn(t, "compat") : t ? Vn(null, t) : null, this.name = typeof r == "string" && r || "core", this.knownTags = i ? Vf : {}, this.tags = Vn(s, this.name, n), this.toStringOptions = l ?? null, Object.defineProperty(this, pt, { value: zt }), Object.defineProperty(this, Xe, { value: On }), Object.defineProperty(this, Gt, { value: Zt }), this.sortMapEntries = typeof o == "function" ? o : o === !0 ? qf : null;
  }
  clone() {
    const t = Object.create(Fi.prototype, Object.getOwnPropertyDescriptors(this));
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
  const i = il(e, t), { commentString: r } = i.options;
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
    let f = Ht(e.contents, i, () => l = null, a);
    l && (f += vt(f, "", r(l))), (f[0] === "|" || f[0] === ">") && s[s.length - 1] === "---" ? s[s.length - 1] = `--- ${f}` : s.push(f);
  } else
    s.push(Ht(e.contents, i));
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
let Ui = class Tl {
  constructor(t, s, n) {
    this.commentBefore = null, this.comment = null, this.errors = [], this.warnings = [], Object.defineProperty(this, Ee, { value: ri });
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
    n != null && n._directives ? (this.directives = n._directives.atDocument(), this.directives.yaml.explicit && (o = this.directives.yaml.version)) : this.directives = new ye({ version: o }), this.setSchema(o, n), this.contents = t === void 0 ? null : this.createNode(t, i, n);
  }
  /**
   * Create a deep copy of this Document and its contents.
   *
   * Custom Node values that inherit from `Object` still refer to their original instances.
   */
  clone() {
    const t = Object.create(Tl.prototype, {
      [Ee]: { value: ri }
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
      const n = Zo(this);
      t.anchor = // eslint-disable-next-line @typescript-eslint/prefer-nullish-coalescing
      !s || n.has(s) ? el(s || "a", n) : s;
    }
    return new Ii(t.anchor);
  }
  createNode(t, s, n) {
    let i;
    if (typeof s == "function")
      t = s.call({ "": t }, "", t), i = s;
    else if (Array.isArray(s)) {
      const y = (S) => typeof S == "number" || S instanceof String || S instanceof Number, O = s.filter(y).map(String);
      O.length > 0 && (s = s.concat(O)), i = s;
    } else n === void 0 && s && (n = s, s = void 0);
    const { aliasDuplicateObjects: r, anchorPrefix: o, flow: l, keepUndefined: c, onTagObj: a, tag: f } = n ?? {}, { onAnchor: u, setAnchors: m, sourceObjects: g } = wf(
      this,
      // eslint-disable-next-line @typescript-eslint/prefer-nullish-coalescing
      o || "a"
    ), b = {
      aliasDuplicateObjects: r ?? !0,
      keepUndefined: c ?? !1,
      onAnchor: u,
      onTagObj: a,
      replacer: i,
      schema: this.schema,
      sourceObjects: g
    }, h = Ts(t, f, b);
    return l && ne(h) && (h.flow = !0), m(), h;
  }
  /**
   * Convert a key and a value into a `Pair` using the current schema,
   * recursively wrapping all values as `Scalar` or `Collection` nodes.
   */
  createPair(t, s, n = {}) {
    const i = this.createNode(t, null, n), r = this.createNode(s, null, n);
    return new Se(i, r);
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
    return cs(t) ? this.contents == null ? !1 : (this.contents = null, !0) : It(this.contents) ? this.contents.deleteIn(t) : !1;
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
    return cs(t) ? !s && Z(this.contents) ? this.contents.value : this.contents : ne(this.contents) ? this.contents.getIn(t, s) : void 0;
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
    return cs(t) ? this.contents !== void 0 : ne(this.contents) ? this.contents.hasIn(t) : !1;
  }
  /**
   * Sets a value in this document. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   */
  set(t, s) {
    this.contents == null ? this.contents = on(this.schema, [t], s) : It(this.contents) && this.contents.set(t, s);
  }
  /**
   * Sets a value in this document. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   */
  setIn(t, s) {
    cs(t) ? this.contents = s : this.contents == null ? this.contents = on(this.schema, Array.from(t), s) : It(this.contents) && this.contents.setIn(t, s);
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
        this.directives ? this.directives.yaml.version = "1.1" : this.directives = new ye({ version: "1.1" }), n = { resolveKnownTags: !1, schema: "yaml-1.1" };
        break;
      case "1.2":
      case "next":
        this.directives ? this.directives.yaml.version = t : this.directives = new ye({ version: t }), n = { resolveKnownTags: !0, schema: "core" };
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
      this.schema = new Fi(Object.assign(n, s));
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
    }, c = Ne(this.contents, s ?? "", l);
    if (typeof r == "function")
      for (const { count: a, res: f } of l.anchors.values())
        r(f, a);
    return typeof o == "function" ? Pt(o, { "": c }, "", c) : c;
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
class Al extends Error {
  constructor(t, s, n, i) {
    super(), this.name = t, this.code = n, this.message = i, this.pos = s;
  }
}
class as extends Al {
  constructor(t, s, n) {
    super("YAMLParseError", t, s, n);
  }
}
class Wf extends Al {
  constructor(t, s, n) {
    super("YAMLWarning", t, s, n);
  }
}
const xr = (e, t) => (s) => {
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
function Wt(e, { flow: t, indicator: s, next: n, offset: i, onError: r, parentIndent: o, startOnNewline: l }) {
  let c = !1, a = l, f = l, u = "", m = "", g = !1, b = !1, h = null, y = null, O = null, S = null, I = null, A = null, $ = null;
  for (const M of e)
    switch (b && (M.type !== "space" && M.type !== "newline" && M.type !== "comma" && r(M.offset, "MISSING_CHAR", "Tags and anchors must be separated from the next token by white space"), b = !1), h && (a && M.type !== "comment" && M.type !== "newline" && r(h, "TAB_AS_INDENT", "Tabs are not allowed as indentation"), h = null), M.type) {
      case "space":
        !t && (s !== "doc-start" || (n == null ? void 0 : n.type) !== "flow-collection") && M.source.includes("	") && (h = M), f = !0;
        break;
      case "comment": {
        f || r(M, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters");
        const q = M.source.substring(1) || " ";
        u ? u += m + q : u = q, m = "", a = !1;
        break;
      }
      case "newline":
        a ? u ? u += M.source : (!A || s !== "seq-item-ind") && (c = !0) : m += M.source, a = !0, g = !0, (y || O) && (S = M), f = !0;
        break;
      case "anchor":
        y && r(M, "MULTIPLE_ANCHORS", "A node can have at most one anchor"), M.source.endsWith(":") && r(M.offset + M.source.length - 1, "BAD_ALIAS", "Anchor ending in : is ambiguous", !0), y = M, $ ?? ($ = M.offset), a = !1, f = !1, b = !0;
        break;
      case "tag": {
        O && r(M, "MULTIPLE_TAGS", "A node can have at most one tag"), O = M, $ ?? ($ = M.offset), a = !1, f = !1, b = !0;
        break;
      }
      case s:
        (y || O) && r(M, "BAD_PROP_ORDER", `Anchors and tags must be after the ${M.source} indicator`), A && r(M, "UNEXPECTED_TOKEN", `Unexpected ${M.source} in ${t ?? "collection"}`), A = M, a = s === "seq-item-ind" || s === "explicit-key-ind", f = !1;
        break;
      case "comma":
        if (t) {
          I && r(M, "UNEXPECTED_TOKEN", `Unexpected , in ${t}`), I = M, a = !1, f = !1;
          break;
        }
      // else fallthrough
      default:
        r(M, "UNEXPECTED_TOKEN", `Unexpected ${M.type} token`), a = !1, f = !1;
    }
  const K = e[e.length - 1], x = K ? K.offset + K.source.length : i;
  return b && n && n.type !== "space" && n.type !== "newline" && n.type !== "comma" && (n.type !== "scalar" || n.source !== "") && r(n.offset, "MISSING_CHAR", "Tags and anchors must be separated from the next token by white space"), h && (a && h.indent <= o || (n == null ? void 0 : n.type) === "block-map" || (n == null ? void 0 : n.type) === "block-seq") && r(h, "TAB_AS_INDENT", "Tabs are not allowed as indentation"), {
    comma: I,
    found: A,
    spaceBefore: c,
    comment: u,
    hasNewline: g,
    anchor: y,
    tag: O,
    newlineAfterProp: S,
    end: x,
    start: $ ?? x
  };
}
function As(e) {
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
        if (As(t.key) || As(t.value))
          return !0;
      }
      return !1;
    default:
      return !0;
  }
}
function ai(e, t, s) {
  if ((t == null ? void 0 : t.type) === "flow-collection") {
    const n = t.end[0];
    n.indent === e && (n.source === "]" || n.source === "}") && As(t) && s(n, "BAD_INDENT", "Flow end indicator should be more indented than parent", !0);
  }
}
function Nl(e, t, s) {
  const { uniqueKeys: n } = e.options;
  if (n === !1)
    return !1;
  const i = typeof n == "function" ? n : (r, o) => r === o || Z(r) && Z(o) && r.value === o.value;
  return t.some((r) => i(r.key, s));
}
const Dr = "All mapping items must start at the same column";
function Jf({ composeNode: e, composeEmptyNode: t }, s, n, i, r) {
  var f;
  const o = (r == null ? void 0 : r.nodeClass) ?? Oe, l = new o(s.schema);
  s.atRoot && (s.atRoot = !1);
  let c = n.offset, a = null;
  for (const u of n.items) {
    const { start: m, key: g, sep: b, value: h } = u, y = Wt(m, {
      indicator: "explicit-key-ind",
      next: g ?? (b == null ? void 0 : b[0]),
      offset: c,
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !0
    }), O = !y.found;
    if (O) {
      if (g && (g.type === "block-seq" ? i(c, "BLOCK_AS_IMPLICIT_KEY", "A block sequence may not be used as an implicit map key") : "indent" in g && g.indent !== n.indent && i(c, "BAD_INDENT", Dr)), !y.anchor && !y.tag && !b) {
        a = y.end, y.comment && (l.comment ? l.comment += `
` + y.comment : l.comment = y.comment);
        continue;
      }
      (y.newlineAfterProp || As(g)) && i(g ?? m[m.length - 1], "MULTILINE_IMPLICIT_KEY", "Implicit keys need to be on a single line");
    } else ((f = y.found) == null ? void 0 : f.indent) !== n.indent && i(c, "BAD_INDENT", Dr);
    s.atKey = !0;
    const S = y.end, I = g ? e(s, g, y, i) : t(s, S, m, null, y, i);
    s.schema.compat && ai(n.indent, g, i), s.atKey = !1, Nl(s, l.items, I) && i(S, "DUPLICATE_KEY", "Map keys must be unique");
    const A = Wt(b ?? [], {
      indicator: "map-value-ind",
      next: h,
      offset: I.range[2],
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !g || g.type === "block-scalar"
    });
    if (c = A.end, A.found) {
      O && ((h == null ? void 0 : h.type) === "block-map" && !A.hasNewline && i(c, "BLOCK_AS_IMPLICIT_KEY", "Nested mappings are not allowed in compact mappings"), s.options.strict && y.start < A.found.offset - 1024 && i(I.range, "KEY_OVER_1024_CHARS", "The : indicator must be at most 1024 chars after the start of an implicit block mapping key"));
      const $ = h ? e(s, h, A, i) : t(s, c, b, null, A, i);
      s.schema.compat && ai(n.indent, h, i), c = $.range[2];
      const K = new Se(I, $);
      s.options.keepSourceTokens && (K.srcToken = u), l.items.push(K);
    } else {
      O && i(I.range, "MISSING_CHAR", "Implicit map keys need to be followed by map values"), A.comment && (I.comment ? I.comment += `
` + A.comment : I.comment = A.comment);
      const $ = new Se(I);
      s.options.keepSourceTokens && ($.srcToken = u), l.items.push($);
    }
  }
  return a && a < c && i(a, "IMPOSSIBLE", "Map comment with trailing content"), l.range = [n.offset, c, a ?? c], l;
}
function Yf({ composeNode: e, composeEmptyNode: t }, s, n, i, r) {
  const o = (r == null ? void 0 : r.nodeClass) ?? At, l = new o(s.schema);
  s.atRoot && (s.atRoot = !1), s.atKey && (s.atKey = !1);
  let c = n.offset, a = null;
  for (const { start: f, value: u } of n.items) {
    const m = Wt(f, {
      indicator: "seq-item-ind",
      next: u,
      offset: c,
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !0
    });
    if (!m.found)
      if (m.anchor || m.tag || u)
        (u == null ? void 0 : u.type) === "block-seq" ? i(m.end, "BAD_INDENT", "All sequence items must start at the same column") : i(c, "MISSING_CHAR", "Sequence item without - indicator");
      else {
        a = m.end, m.comment && (l.comment = m.comment);
        continue;
      }
    const g = u ? e(s, u, m, i) : t(s, m.end, f, null, m, i);
    s.schema.compat && ai(n.indent, u, i), c = g.range[2], l.items.push(g);
  }
  return l.range = [n.offset, c, a ?? c], l;
}
function Ps(e, t, s, n) {
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
const qn = "Block collections are not allowed within flow collections", Hn = (e) => e && (e.type === "block-map" || e.type === "block-seq");
function Gf({ composeNode: e, composeEmptyNode: t }, s, n, i, r) {
  var y;
  const o = n.start.source === "{", l = o ? "flow map" : "flow sequence", c = (r == null ? void 0 : r.nodeClass) ?? (o ? Oe : At), a = new c(s.schema);
  a.flow = !0;
  const f = s.atRoot;
  f && (s.atRoot = !1), s.atKey && (s.atKey = !1);
  let u = n.offset + n.start.source.length;
  for (let O = 0; O < n.items.length; ++O) {
    const S = n.items[O], { start: I, key: A, sep: $, value: K } = S, x = Wt(I, {
      flow: l,
      indicator: "explicit-key-ind",
      next: A ?? ($ == null ? void 0 : $[0]),
      offset: u,
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !1
    });
    if (!x.found) {
      if (!x.anchor && !x.tag && !$ && !K) {
        O === 0 && x.comma ? i(x.comma, "UNEXPECTED_TOKEN", `Unexpected , in ${l}`) : O < n.items.length - 1 && i(x.start, "UNEXPECTED_TOKEN", `Unexpected empty item in ${l}`), x.comment && (a.comment ? a.comment += `
` + x.comment : a.comment = x.comment), u = x.end;
        continue;
      }
      !o && s.options.strict && As(A) && i(
        A,
        // checked by containsNewline()
        "MULTILINE_IMPLICIT_KEY",
        "Implicit keys of flow sequence pairs need to be on a single line"
      );
    }
    if (O === 0)
      x.comma && i(x.comma, "UNEXPECTED_TOKEN", `Unexpected , in ${l}`);
    else if (x.comma || i(x.start, "MISSING_CHAR", `Missing , between ${l} items`), x.comment) {
      let M = "";
      e: for (const q of I)
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
` + M : q.comment = M, x.comment = x.comment.substring(M.length + 1);
      }
    }
    if (!o && !$ && !x.found) {
      const M = K ? e(s, K, x, i) : t(s, x.end, $, null, x, i);
      a.items.push(M), u = M.range[2], Hn(K) && i(M.range, "BLOCK_IN_FLOW", qn);
    } else {
      s.atKey = !0;
      const M = x.end, q = A ? e(s, A, x, i) : t(s, M, I, null, x, i);
      Hn(A) && i(q.range, "BLOCK_IN_FLOW", qn), s.atKey = !1;
      const ee = Wt($ ?? [], {
        flow: l,
        indicator: "map-value-ind",
        next: K,
        offset: q.range[2],
        onError: i,
        parentIndent: n.indent,
        startOnNewline: !1
      });
      if (ee.found) {
        if (!o && !x.found && s.options.strict) {
          if ($)
            for (const ue of $) {
              if (ue === ee.found)
                break;
              if (ue.type === "newline") {
                i(ue, "MULTILINE_IMPLICIT_KEY", "Implicit keys of flow sequence pairs need to be on a single line");
                break;
              }
            }
          x.start < ee.found.offset - 1024 && i(ee.found, "KEY_OVER_1024_CHARS", "The : indicator must be at most 1024 chars after the start of an implicit flow sequence key");
        }
      } else K && ("source" in K && ((y = K.source) == null ? void 0 : y[0]) === ":" ? i(K, "MISSING_CHAR", `Missing space after : in ${l}`) : i(ee.start, "MISSING_CHAR", `Missing , or : between ${l} items`));
      const pe = K ? e(s, K, ee, i) : ee.found ? t(s, ee.end, $, null, ee, i) : null;
      pe ? Hn(K) && i(pe.range, "BLOCK_IN_FLOW", qn) : ee.comment && (q.comment ? q.comment += `
` + ee.comment : q.comment = ee.comment);
      const fe = new Se(q, pe);
      if (s.options.keepSourceTokens && (fe.srcToken = S), o) {
        const ue = a;
        Nl(s, ue.items, q) && i(M, "DUPLICATE_KEY", "Map keys must be unique"), ue.items.push(fe);
      } else {
        const ue = new Oe(s.schema);
        ue.flow = !0, ue.items.push(fe);
        const Et = (pe ?? q).range;
        ue.range = [q.range[0], Et[1], Et[2]], a.items.push(ue);
      }
      u = pe ? pe.range[2] : ee.end;
    }
  }
  const m = o ? "}" : "]", [g, ...b] = n.end;
  let h = u;
  if ((g == null ? void 0 : g.source) === m)
    h = g.offset + g.source.length;
  else {
    const O = l[0].toUpperCase() + l.substring(1), S = f ? `${O} must end with a ${m}` : `${O} in block collection must be sufficiently indented and end with a ${m}`;
    i(u, f ? "MISSING_CHAR" : "BAD_INDENT", S), g && g.source.length !== 1 && b.unshift(g);
  }
  if (b.length > 0) {
    const O = Ps(b, h, s.options.strict, i);
    O.comment && (a.comment ? a.comment += `
` + O.comment : a.comment = O.comment), a.range = [n.offset, h, O.offset];
  } else
    a.range = [n.offset, h, h];
  return a;
}
function Wn(e, t, s, n, i, r) {
  const o = s.type === "block-map" ? Jf(e, t, s, n, r) : s.type === "block-seq" ? Yf(e, t, s, n, r) : Gf(e, t, s, n, r), l = o.constructor;
  return i === "!" || i === l.tagName ? (o.tag = l.tagName, o) : (i && (o.tag = i), o);
}
function Qf(e, t, s, n, i) {
  var m;
  const r = n.tag, o = r ? t.directives.tagName(r.source, (g) => i(r, "TAG_RESOLVE_FAILED", g)) : null;
  if (s.type === "block-seq") {
    const { anchor: g, newlineAfterProp: b } = n, h = g && r ? g.offset > r.offset ? g : r : g ?? r;
    h && (!b || b.offset < h.offset) && i(h, "MISSING_CHAR", "Missing newline after block sequence props");
  }
  const l = s.type === "block-map" ? "map" : s.type === "block-seq" ? "seq" : s.start.source === "{" ? "map" : "seq";
  if (!r || !o || o === "!" || o === Oe.tagName && l === "map" || o === At.tagName && l === "seq")
    return Wn(e, t, s, i, o);
  let c = t.schema.tags.find((g) => g.tag === o && g.collection === l);
  if (!c) {
    const g = t.schema.knownTags[o];
    if ((g == null ? void 0 : g.collection) === l)
      t.schema.tags.push(Object.assign({}, g, { default: !1 })), c = g;
    else
      return g ? i(r, "BAD_COLLECTION_TYPE", `${g.tag} used for ${l} collection, but expects ${g.collection ?? "scalar"}`, !0) : i(r, "TAG_RESOLVE_FAILED", `Unresolved tag: ${o}`, !0), Wn(e, t, s, i, o);
  }
  const a = Wn(e, t, s, i, o, c), f = ((m = c.resolve) == null ? void 0 : m.call(c, a, (g) => i(r, "TAG_RESOLVE_FAILED", g), t.options)) ?? a, u = ie(f) ? f : new B(f);
  return u.range = a.range, u.tag = o, c != null && c.format && (u.format = c.format), u;
}
function Xf(e, t, s) {
  const n = t.offset, i = zf(t, e.options.strict, s);
  if (!i)
    return { value: "", type: null, comment: "", range: [n, n, n] };
  const r = i.mode === ">" ? B.BLOCK_FOLDED : B.BLOCK_LITERAL, o = t.source ? Zf(t.source) : [];
  let l = o.length;
  for (let h = o.length - 1; h >= 0; --h) {
    const y = o[h][1];
    if (y === "" || y === "\r")
      l = h;
    else
      break;
  }
  if (l === 0) {
    const h = i.chomp === "+" && o.length > 0 ? `
`.repeat(Math.max(1, o.length - 1)) : "";
    let y = n + i.length;
    return t.source && (y += t.source.length), { value: h, type: r, comment: i.comment, range: [n, y, y] };
  }
  let c = t.indent + i.indent, a = t.offset + i.length, f = 0;
  for (let h = 0; h < l; ++h) {
    const [y, O] = o[h];
    if (O === "" || O === "\r")
      i.indent === 0 && y.length > c && (c = y.length);
    else {
      y.length < c && s(a + y.length, "MISSING_CHAR", "Block scalars with more-indented leading empty lines must use an explicit indentation indicator"), i.indent === 0 && (c = y.length), f = h, c === 0 && !e.atRoot && s(a, "BAD_INDENT", "Block scalar values in collections must be indented");
      break;
    }
    a += y.length + O.length + 1;
  }
  for (let h = o.length - 1; h >= l; --h)
    o[h][0].length > c && (l = h + 1);
  let u = "", m = "", g = !1;
  for (let h = 0; h < f; ++h)
    u += o[h][0].slice(c) + `
`;
  for (let h = f; h < l; ++h) {
    let [y, O] = o[h];
    a += y.length + O.length + 1;
    const S = O[O.length - 1] === "\r";
    if (S && (O = O.slice(0, -1)), O && y.length < c) {
      const A = `Block scalar lines must not be less indented than their ${i.indent ? "explicit indentation indicator" : "first line"}`;
      s(a - O.length - (S ? 2 : 1), "BAD_INDENT", A), y = "";
    }
    r === B.BLOCK_LITERAL ? (u += m + y.slice(c) + O, m = `
`) : y.length > c || O[0] === "	" ? (m === " " ? m = `
` : !g && m === `
` && (m = `

`), u += m + y.slice(c) + O, m = `
`, g = !0) : O === "" ? m === `
` ? u += `
` : m = `
` : (u += m + O, m = " ", g = !1);
  }
  switch (i.chomp) {
    case "-":
      break;
    case "+":
      for (let h = l; h < o.length; ++h)
        u += `
` + o[h][0].slice(c);
      u[u.length - 1] !== `
` && (u += `
`);
      break;
    default:
      u += `
`;
  }
  const b = n + i.length + t.source.length;
  return { value: u, type: r, comment: i.comment, range: [n, b, b] };
}
function zf({ offset: e, props: t }, s, n) {
  if (t[0].type !== "block-scalar-header")
    return n(t[0], "IMPOSSIBLE", "Block scalar header not found"), null;
  const { source: i } = t[0], r = i[0];
  let o = 0, l = "", c = -1;
  for (let m = 1; m < i.length; ++m) {
    const g = i[m];
    if (!l && (g === "-" || g === "+"))
      l = g;
    else {
      const b = Number(g);
      !o && b ? o = b : c === -1 && (c = e + m);
    }
  }
  c !== -1 && n(c, "UNEXPECTED_TOKEN", `Block scalar header includes extra characters: ${i}`);
  let a = !1, f = "", u = i.length;
  for (let m = 1; m < t.length; ++m) {
    const g = t[m];
    switch (g.type) {
      case "space":
        a = !0;
      // fallthrough
      case "newline":
        u += g.source.length;
        break;
      case "comment":
        s && !a && n(g, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters"), u += g.source.length, f = g.source.substring(1);
        break;
      case "error":
        n(g, "UNEXPECTED_TOKEN", g.message), u += g.source.length;
        break;
      /* istanbul ignore next should not happen */
      default: {
        const b = `Unexpected token in block scalar header: ${g.type}`;
        n(g, "UNEXPECTED_TOKEN", b);
        const h = g.source;
        h && typeof h == "string" && (u += h.length);
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
  const a = (m, g, b) => s(n + m, g, b);
  switch (i) {
    case "scalar":
      l = B.PLAIN, c = tu(r, a);
      break;
    case "single-quoted-scalar":
      l = B.QUOTE_SINGLE, c = su(r, a);
      break;
    case "double-quoted-scalar":
      l = B.QUOTE_DOUBLE, c = nu(r, a);
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
  const f = n + r.length, u = Ps(o, f, t, s);
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
  return s && t(0, "BAD_SCALAR_START", `Plain value cannot start with ${s}`), El(e);
}
function su(e, t) {
  return (e[e.length - 1] !== "'" || e.length === 1) && t(e.length, "MISSING_CHAR", "Missing closing 'quote"), El(e.slice(1, -1)).replace(/''/g, "'");
}
function El(e) {
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
function Cl(e, t, s, n) {
  const { value: i, type: r, comment: o, range: l } = t.type === "block-scalar" ? Xf(e, t, n) : eu(t, e.options.strict, n), c = s ? e.directives.tagName(s.source, (u) => n(s, "TAG_RESOLVE_FAILED", u)) : null;
  let a;
  e.options.stringKeys && e.atKey ? a = e.schema[Xe] : c ? a = lu(e.schema, i, c, s, n) : t.type === "scalar" ? a = cu(e, i, t, n) : a = e.schema[Xe];
  let f;
  try {
    const u = a.resolve(i, (m) => n(s ?? t, "TAG_RESOLVE_FAILED", m), e.options);
    f = Z(u) ? u : new B(u);
  } catch (u) {
    const m = u instanceof Error ? u.message : String(u);
    n(s ?? t, "TAG_RESOLVE_FAILED", m), f = new B(i);
  }
  return f.range = l, f.source = i, r && (f.type = r), c && (f.tag = c), a.format && (f.format = a.format), o && (f.comment = o), f;
}
function lu(e, t, s, n, i) {
  var l;
  if (s === "!")
    return e[Xe];
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
  return o && !o.collection ? (e.tags.push(Object.assign({}, o, { default: !1, test: void 0 })), o) : (i(n, "TAG_RESOLVE_FAILED", `Unresolved tag: ${s}`, s !== "tag:yaml.org,2002:str"), e[Xe]);
}
function cu({ atKey: e, directives: t, schema: s }, n, i, r) {
  const o = s.tags.find((l) => {
    var c;
    return (l.default === !0 || e && l.default === "key") && ((c = l.test) == null ? void 0 : c.test(n));
  }) || s[Xe];
  if (s.compat) {
    const l = s.compat.find((c) => {
      var a;
      return c.default && ((a = c.test) == null ? void 0 : a.test(n));
    }) ?? s[Xe];
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
const fu = { composeNode: Il, composeEmptyNode: Vi };
function Il(e, t, s, n) {
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
      a = Cl(e, t, c, n), l && (a.anchor = l.source.substring(1));
      break;
    case "block-map":
    case "block-seq":
    case "flow-collection":
      try {
        a = Qf(fu, e, t, s, n), l && (a.anchor = l.source.substring(1));
      } catch (u) {
        const m = u instanceof Error ? u.message : String(u);
        n(t, "RESOURCE_EXHAUSTION", m);
      }
      break;
    default: {
      const u = t.type === "error" ? t.message : `Unsupported token (type: ${t.type})`;
      n(t, "UNEXPECTED_TOKEN", u), f = !1;
    }
  }
  return a ?? (a = Vi(e, t.offset, void 0, null, s, n)), l && a.anchor === "" && n(l, "BAD_ALIAS", "Anchor cannot be an empty string"), i && e.options.stringKeys && (!Z(a) || typeof a.value != "string" || a.tag && a.tag !== "tag:yaml.org,2002:str") && n(c ?? t, "NON_STRING_KEY", "With stringKeys, all keys must be strings"), r && (a.spaceBefore = !0), o && (t.type === "scalar" && t.source === "" ? a.comment = o : a.commentBefore = o), e.options.keepSourceTokens && f && (a.srcToken = t), a;
}
function Vi(e, t, s, n, { spaceBefore: i, comment: r, anchor: o, tag: l, end: c }, a) {
  const f = {
    type: "scalar",
    offset: au(t, s, n),
    indent: -1,
    source: ""
  }, u = Cl(e, f, l, a);
  return o && (u.anchor = o.source.substring(1), u.anchor === "" && a(o, "BAD_ALIAS", "Anchor cannot be an empty string")), i && (u.spaceBefore = !0), r && (u.comment = r, u.range[2] = c), u;
}
function uu({ options: e }, { offset: t, source: s, end: n }, i) {
  const r = new Ii(s.substring(1));
  r.source === "" && i(t, "BAD_ALIAS", "Alias cannot be an empty string"), r.source.endsWith(":") && i(t + s.length - 1, "BAD_ALIAS", "Alias ending in : is ambiguous", !0);
  const o = t + s.length, l = Ps(n, o, e.strict, i);
  return r.range = [t, o, l.offset], l.comment && (r.comment = l.comment), r;
}
function hu(e, t, { offset: s, start: n, value: i, end: r }, o) {
  const l = Object.assign({ _directives: t }, e), c = new Ui(void 0, l), a = {
    atKey: !1,
    atRoot: !0,
    directives: c.directives,
    options: c.options,
    schema: c.schema
  }, f = Wt(n, {
    indicator: "doc-start",
    next: i ?? (r == null ? void 0 : r[0]),
    offset: s,
    onError: o,
    parentIndent: 0,
    startOnNewline: !0
  });
  f.found && (c.directives.docStart = !0, i && (i.type === "block-map" || i.type === "block-seq") && !f.hasNewline && o(f.end, "MISSING_CHAR", "Block collection cannot start on same line with directives-end marker")), c.contents = i ? Il(a, i, f, o) : Vi(a, f.end, n, null, f, o);
  const u = c.contents.range[2], m = Ps(r, u, !1, o);
  return m.comment && (c.comment = m.comment), c.range = [s, u, m.offset], c;
}
function rs(e) {
  if (typeof e == "number")
    return [e, e + 1];
  if (Array.isArray(e))
    return e.length === 2 ? e : [e[0], e[1]];
  const { offset: t, source: s } = e;
  return [t, t + (typeof s == "string" ? s.length : 1)];
}
function Rr(e) {
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
      const o = rs(s);
      r ? this.warnings.push(new Wf(o, n, i)) : this.errors.push(new as(o, n, i));
    }, this.directives = new ye({ version: t.version || "1.2" }), this.options = t;
  }
  decorate(t, s) {
    const { comment: n, afterEmptyLine: i } = Rr(this.prelude);
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
      comment: Rr(this.prelude).comment,
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
          const r = rs(t);
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
        const s = t.source ? `${t.message}: ${JSON.stringify(t.source)}` : t.message, n = new as(rs(t), "UNEXPECTED_TOKEN", s);
        this.atDirectives || !this.doc ? this.errors.push(n) : this.doc.errors.push(n);
        break;
      }
      case "doc-end": {
        if (!this.doc) {
          const n = "Unexpected doc-end without preceding document";
          this.errors.push(new as(rs(t), "UNEXPECTED_TOKEN", n));
          break;
        }
        this.doc.directives.docEnd = !0;
        const s = Ps(t.end, t.offset + t.source.length, this.doc.options.strict, this.onError);
        if (this.decorate(this.doc, !0), s.comment) {
          const n = this.doc.comment;
          this.doc.comment = n ? `${n}
${s.comment}` : s.comment;
        }
        this.doc.range[2] = s.offset;
        break;
      }
      default:
        this.errors.push(new as(rs(t), "UNEXPECTED_TOKEN", `Unsupported token ${t.type}`));
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
      const n = Object.assign({ _directives: this.directives }, this.options), i = new Ui(void 0, n);
      this.atDirectives && this.onError(s, "MISSING_CHAR", "Missing directives-end indicator line"), i.range = [0, s, s], this.decorate(i, !1), yield i;
    }
  }
}
const Ll = "\uFEFF", $l = "", Ml = "", fi = "";
function pu(e) {
  switch (e) {
    case Ll:
      return "byte-order-mark";
    case $l:
      return "doc-mode";
    case Ml:
      return "flow-error-end";
    case fi:
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
function Ie(e) {
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
const jr = new Set("0123456789ABCDEFabcdef"), gu = new Set("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-#;/?:@&=+$_.!~*'()"), qs = new Set(",[]{}"), mu = new Set(` ,[]{}
\r	`), Jn = (e) => !e || mu.has(e);
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
      if ((n === "---" || n === "...") && Ie(this.buffer[t + 3]))
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
    if (t[0] === Ll && (yield* this.pushCount(1), t = t.substring(1)), t[0] === "%") {
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
    return yield $l, yield* this.parseLineStart();
  }
  *parseLineStart() {
    const t = this.charAt(0);
    if (!t && !this.atEnd)
      return this.setNext("line-start");
    if (t === "-" || t === ".") {
      if (!this.atEnd && !this.hasChars(4))
        return this.setNext("line-start");
      const s = this.peek(3);
      if ((s === "---" || s === "...") && Ie(this.charAt(3)))
        return yield* this.pushCount(3), this.indentValue = 0, this.indentNext = 0, s === "---" ? "doc" : "stream";
    }
    return this.indentValue = yield* this.pushSpaces(!1), this.indentNext > this.indentValue && !Ie(this.charAt(1)) && (this.indentNext = this.indentValue), yield* this.parseBlockStart();
  }
  *parseBlockStart() {
    const [t, s] = this.peek(2);
    if (!s && !this.atEnd)
      return this.setNext("block-start");
    if ((t === "-" || t === "?" || t === ":") && Ie(s)) {
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
        return yield* this.pushUntil(Jn), "doc";
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
    if ((n !== -1 && n < this.indentNext && i[0] !== "#" || n === 0 && (i.startsWith("---") || i.startsWith("...")) && Ie(i[3])) && !(n === this.indentNext - 1 && this.flowLevel === 1 && (i[0] === "]" || i[0] === "}")))
      return this.flowLevel = 0, yield Ml, yield* this.parseLineStart();
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
        return yield* this.pushUntil(Jn), "flow";
      case '"':
      case "'":
        return this.flowKey = !0, yield* this.parseQuotedScalar();
      case ":": {
        const o = this.charAt(1);
        if (this.flowKey || Ie(o) || o === ",")
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
    return yield* this.pushUntil((s) => Ie(s) || s === "#");
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
    return yield fi, yield* this.pushToIndex(t + 1, !0), yield* this.parseLineStart();
  }
  *parsePlainScalar() {
    const t = this.flowLevel > 0;
    let s = this.pos - 1, n = this.pos - 1, i;
    for (; i = this.buffer[++n]; )
      if (i === ":") {
        const r = this.buffer[n + 1];
        if (Ie(r) || t && qs.has(r))
          break;
        s = n;
      } else if (Ie(i)) {
        let r = this.buffer[n + 1];
        if (i === "\r" && (r === `
` ? (n += 1, i = `
`, r = this.buffer[n + 1]) : s = n), r === "#" || t && qs.has(r))
          break;
        if (i === `
`) {
          const o = this.continueScalar(n + 1);
          if (o === -1)
            break;
          n = Math.max(n, o - 2);
        }
      } else {
        if (t && qs.has(i))
          break;
        s = n;
      }
    return !i && !this.atEnd ? this.setNext("plain-scalar") : (yield fi, yield* this.pushToIndex(s + 1, !0), t ? "flow" : "doc");
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
          t += yield* this.pushUntil(Jn), t += yield* this.pushSpaces(!0);
          continue e;
        case "-":
        // this is an error
        case "?":
        // this is an error outside flow collections
        case ":": {
          const s = this.flowLevel > 0, n = this.charAt(1);
          if (Ie(n) || s && qs.has(n)) {
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
      for (; !Ie(s) && s !== ">"; )
        s = this.buffer[++t];
      return yield* this.pushToIndex(s === ">" ? t + 1 : t, !1);
    } else {
      let t = this.pos + 1, s = this.buffer[t];
      for (; s; )
        if (gu.has(s))
          s = this.buffer[++t];
        else if (s === "%" && jr.has(this.buffer[t + 1]) && jr.has(this.buffer[t + 2]))
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
function Br(e) {
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
function Pl(e) {
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
function Hs(e) {
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
function cn(e, t) {
  if (t.length < 1e5)
    Array.prototype.push.apply(e, t);
  else
    for (let s = 0; s < t.length; ++s)
      e.push(t[s]);
}
function Kr(e) {
  if (e.start.type === "flow-seq-start")
    for (const t of e.items)
      t.sep && !t.value && !ht(t.start, "explicit-key-ind") && !ht(t.sep, "map-value-ind") && (t.key && (t.value = t.key), delete t.key, Pl(t.value) ? t.value.end ? cn(t.value.end, t.sep) : t.value.end = t.sep : cn(t.start, t.sep), delete t.sep);
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
      switch (s.type === "block-scalar" ? s.indent = "indent" in n ? n.indent : 0 : s.type === "flow-collection" && n.type === "document" && (s.indent = 0), s.type === "flow-collection" && Kr(s), n.type) {
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
        i && !i.sep && !i.value && i.start.length > 0 && Br(i.start) === -1 && (s.indent === 0 || i.start.every((r) => r.type !== "comment" || r.indent < s.indent)) && (n.type === "document" ? n.end = i.start : n.items.push({ start: i.start }), s.items.splice(-1, 1));
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
        Br(t.start) !== -1 ? (yield* this.pop(), yield* this.step()) : t.start.push(this.sourceToken);
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
      const s = Hs(this.peek(2)), n = Lt(s);
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
              cn(r, s.start), r.push(this.sourceToken), t.items.pop();
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
              else if (Pl(s.key) && !ht(s.sep, "newline")) {
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
              cn(r, s.start), r.push(this.sourceToken), t.items.pop();
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
        const i = Hs(n), r = Lt(i);
        Kr(t);
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
        const s = Hs(t), n = Lt(s);
        return n.push(this.sourceToken), {
          type: "block-map",
          offset: this.offset,
          indent: this.indent,
          items: [{ start: n, explicitKey: !0 }]
        };
      }
      case "map-value-ind": {
        this.onKeyLine = !0;
        const s = Hs(t), n = Lt(s);
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
      o.errors.push(new as(l.range.slice(0, 2), "MULTIPLE_DOCS", "Source contains multiple documents; please use YAML.parseAllDocuments()"));
      break;
    }
  return n && s && (o.errors.forEach(xr(e, s)), o.warnings.forEach(xr(e, s))), o;
}
function vu(e, t, s) {
  let n;
  const i = _u(e, s);
  if (!i)
    return null;
  if (i.warnings.forEach((r) => rl(i.options.logLevel, r)), i.errors.length > 0) {
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
  return Is(e) && !n ? e.toString(s) : new Ui(e, n, s).toString(s);
}
const Ou = { style: { display: "flex", "flex-direction": "column", gap: "8px", "font-size": "12px" } }, Tu = { style: { display: "flex", gap: "16px", "align-items": "center" } }, Au = { style: { display: "flex", gap: "6px", "align-items": "center" } }, Nu = { style: { display: "flex", gap: "6px", "align-items": "center" } }, Eu = {
  key: 0,
  style: { opacity: ".6", "font-style": "italic" }
}, Cu = { style: { display: "flex", "justify-content": "space-between", "align-items": "center" } }, Iu = { style: { display: "flex", gap: "10px", "align-items": "center" } }, Lu = ["onUpdate:modelValue"], $u = ["onUpdate:modelValue"], Mu = ["onClick"], Pu = {
  key: 0,
  style: { display: "flex", gap: "10px", "align-items": "center" }
}, xu = ["onUpdate:modelValue"], Du = ["onUpdate:modelValue"], Ru = {
  key: 1,
  style: { display: "flex", gap: "10px", "align-items": "center" }
}, ju = ["onUpdate:modelValue"], Bu = {
  key: 2,
  style: { display: "flex", gap: "10px", "align-items": "center" }
}, Ku = {
  key: 0,
  style: { display: "flex", gap: "6px", "align-items": "center" }
}, Fu = ["onUpdate:modelValue"], Uu = ["onUpdate:modelValue"], Vu = ["onUpdate:modelValue"], qu = { style: { display: "flex", gap: "10px", "align-items": "center" } }, Hu = ["onUpdate:modelValue"], Wu = { style: { display: "flex", gap: "10px", "align-items": "center" } }, Ju = ["disabled", "onClick"], Yu = { style: { opacity: ".7" } }, Gu = /* @__PURE__ */ Mc({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function i(b, h) {
      return {
        key: n++,
        name: b,
        provider: h.provider || "mssql",
        path: h.provider === "sqlite" ? h.file || "" : h.server || "",
        database: h.database || "",
        user: h.user || "",
        password: h.password || "",
        trustedConnection: h.trusted_connection ?? !0,
        description: h.description || "",
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
        password: b.password || void 0,
        trusted_connection: b.provider === "mssql" ? b.trustedConnection : void 0,
        description: b.description || void 0
      };
    }
    const o = (() => {
      try {
        return vu(s.api.getYaml() || "") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ er(o.default_connection || ""), c = /* @__PURE__ */ er(o.default_limit || 10), a = /* @__PURE__ */ gn(
      Object.entries(o.connections || {}).map(([b, h]) => i(b, h))
    );
    function f() {
      a.push(i(`db${a.length + 1}`, { provider: "mssql" }));
    }
    async function u(b) {
      b.testing = !0, b.testStatus = "Connecting...";
      try {
        const h = await s.api.invoke("plugin.action", {
          pluginId: "sql",
          action: "testConnection",
          valueJson: JSON.stringify(m(b))
        });
        if (h.ok && h.resultJson) {
          const y = JSON.parse(h.resultJson);
          b.testStatus = y.message;
        } else
          b.testStatus = "Failed: " + (h.error || "unknown error");
      } catch (h) {
        b.testStatus = "Failed: " + (h instanceof Error ? h.message : String(h));
      } finally {
        b.testing = !1;
      }
    }
    function m(b) {
      const h = r(b);
      return {
        provider: h.provider,
        server: h.server,
        database: h.database,
        user: h.user,
        password: h.password,
        trustedConnection: h.trusted_connection,
        file: h.file,
        description: h.description
      };
    }
    function g() {
      const b = {
        default_connection: l.value || void 0,
        default_limit: c.value || 10,
        connections: Object.fromEntries(
          a.filter((h) => h.name.trim()).map((h) => [h.name, r(h)])
        )
      };
      return ku(b);
    }
    return t({ toYaml: g }), (b, h) => (Ue(), Ze("div", Ou, [
      h[14] || (h[14] = U("div", { style: { opacity: ".7" } }, "Named database connections available to the SQL agent. Stored opaquely in the .spla project file.", -1)),
      U("div", Tu, [
        U("label", Au, [
          h[2] || (h[2] = U("span", { style: { opacity: ".7" } }, "Default connection", -1)),
          Ce(U("input", {
            "onUpdate:modelValue": h[0] || (h[0] = (y) => l.value = y),
            style: { width: "140px" }
          }, null, 512), [
            [et, l.value]
          ])
        ]),
        U("label", Nu, [
          h[3] || (h[3] = U("span", { style: { opacity: ".7" } }, "Default row limit", -1)),
          Ce(U("input", {
            "onUpdate:modelValue": h[1] || (h[1] = (y) => c.value = y),
            type: "number",
            min: "1",
            style: { width: "90px" }
          }, null, 512), [
            [
              et,
              c.value,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      U("button", {
        type: "button",
        style: { "align-self": "flex-start" },
        onClick: f
      }, "+ Add Connection"),
      a.length ? Ks("", !0) : (Ue(), Ze("div", Eu, 'No connections yet. Click "+ Add Connection".')),
      (Ue(!0), Ze(Le, null, Jc(a, (y, O) => (Ue(), Ze("div", {
        key: y.key,
        style: { border: "1px solid var(--panel-border,#444)", "border-radius": "4px", padding: "10px", display: "flex", "flex-direction": "column", gap: "6px" }
      }, [
        U("div", Cu, [
          U("div", Iu, [
            h[5] || (h[5] = U("span", { style: { opacity: ".7" } }, "Name", -1)),
            Ce(U("input", {
              "onUpdate:modelValue": (S) => y.name = S,
              style: { width: "140px" }
            }, null, 8, Lu), [
              [et, y.name]
            ]),
            h[6] || (h[6] = U("span", { style: { opacity: ".7" } }, "Provider", -1)),
            Ce(U("select", {
              "onUpdate:modelValue": (S) => y.provider = S
            }, [...h[4] || (h[4] = [
              U("option", { value: "mssql" }, "mssql", -1),
              U("option", { value: "postgres" }, "postgres", -1),
              U("option", { value: "sqlite" }, "sqlite", -1)
            ])], 8, $u), [
              [lf, y.provider]
            ])
          ]),
          U("button", {
            type: "button",
            onClick: (S) => a.splice(O, 1)
          }, "✕ Remove", 8, Mu)
        ]),
        y.provider !== "sqlite" ? (Ue(), Ze("div", Pu, [
          h[7] || (h[7] = U("span", { style: { opacity: ".7", width: "70px" } }, "Server", -1)),
          Ce(U("input", {
            "onUpdate:modelValue": (S) => y.path = S,
            placeholder: "sql01 or 192.168.1.10",
            style: { width: "220px" }
          }, null, 8, xu), [
            [et, y.path]
          ]),
          h[8] || (h[8] = U("span", { style: { opacity: ".7", width: "70px" } }, "Database", -1)),
          Ce(U("input", {
            "onUpdate:modelValue": (S) => y.database = S,
            style: { width: "160px" }
          }, null, 8, Du), [
            [et, y.database]
          ])
        ])) : (Ue(), Ze("div", Ru, [
          h[9] || (h[9] = U("span", { style: { opacity: ".7", width: "70px" } }, "File", -1)),
          Ce(U("input", {
            "onUpdate:modelValue": (S) => y.path = S,
            placeholder: "C:\\data\\mydb.sqlite",
            style: { width: "400px" }
          }, null, 8, ju), [
            [et, y.path]
          ])
        ])),
        y.provider !== "sqlite" ? (Ue(), Ze("div", Bu, [
          y.provider === "mssql" ? (Ue(), Ze("label", Ku, [
            Ce(U("input", {
              type: "checkbox",
              "onUpdate:modelValue": (S) => y.trustedConnection = S
            }, null, 8, Fu), [
              [of, y.trustedConnection]
            ]),
            h[10] || (h[10] = Ho(" Windows Auth (domain) ", -1))
          ])) : Ks("", !0),
          !y.trustedConnection || y.provider !== "mssql" ? (Ue(), Ze(Le, { key: 1 }, [
            h[11] || (h[11] = U("span", { style: { opacity: ".7" } }, "User", -1)),
            Ce(U("input", {
              "onUpdate:modelValue": (S) => y.user = S,
              style: { width: "130px" }
            }, null, 8, Uu), [
              [et, y.user]
            ]),
            h[12] || (h[12] = U("span", { style: { opacity: ".7" } }, "Password", -1)),
            Ce(U("input", {
              "onUpdate:modelValue": (S) => y.password = S,
              type: "password",
              placeholder: "or env:MY_VAR",
              style: { width: "130px" }
            }, null, 8, Vu), [
              [et, y.password]
            ])
          ], 64)) : Ks("", !0)
        ])) : Ks("", !0),
        U("div", qu, [
          h[13] || (h[13] = U("span", { style: { opacity: ".7", width: "70px" } }, "Description", -1)),
          Ce(U("input", {
            "onUpdate:modelValue": (S) => y.description = S,
            placeholder: "Shown to the AI — what this database contains",
            style: { width: "500px" }
          }, null, 8, Hu), [
            [et, y.description]
          ])
        ]),
        U("div", Wu, [
          U("button", {
            type: "button",
            disabled: y.testing,
            onClick: (S) => u(y)
          }, "Test Connection", 8, Ju),
          U("span", Yu, Gr(y.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
});
function Xu(e, t) {
  let s = ff(Gu, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toYaml(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  Xu as mount
};
