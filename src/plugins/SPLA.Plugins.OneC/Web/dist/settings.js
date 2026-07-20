/**
* @vue/shared v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Gs(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const q = {}, ht = [], Fe = () => {
}, qn = () => !1, fs = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), us = (e) => e.startsWith("onUpdate:"), ie = Object.assign, Js = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, ir = Object.prototype.hasOwnProperty, U = (e, t) => ir.call(e, t), N = Array.isArray, pt = (e) => Kt(e) === "[object Map]", as = (e) => Kt(e) === "[object Set]", pn = (e) => Kt(e) === "[object Date]", F = (e) => typeof e == "function", Y = (e) => typeof e == "string", $e = (e) => typeof e == "symbol", k = (e) => e !== null && typeof e == "object", zn = (e) => (k(e) || F(e)) && F(e.then) && F(e.catch), Yn = Object.prototype.toString, Kt = (e) => Yn.call(e), rr = (e) => Kt(e).slice(8, -1), Xn = (e) => Kt(e) === "[object Object]", qs = (e) => Y(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, Pt = /* @__PURE__ */ Gs(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), ds = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, or = /-\w/g, ue = ds(
  (e) => e.replace(or, (t) => t.slice(1).toUpperCase())
), lr = /\B([A-Z])/g, ot = ds(
  (e) => e.replace(lr, "-$1").toLowerCase()
), hs = ds((e) => e.charAt(0).toUpperCase() + e.slice(1)), Ss = ds(
  (e) => e ? `on${hs(e)}` : ""
), Ne = (e, t) => !Object.is(e, t), Yt = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Zn = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, ps = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let gn;
const gs = () => gn || (gn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function zs(e) {
  if (N(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], i = Y(n) ? ar(n) : zs(n);
      if (i)
        for (const r in i)
          t[r] = i[r];
    }
    return t;
  } else if (Y(e) || k(e))
    return e;
}
const cr = /;(?![^(]*\))/g, fr = /:([^]+)/, ur = /\/\*[^]*?\*\//g;
function ar(e) {
  const t = {};
  return e.replace(ur, "").split(cr).forEach((s) => {
    if (s) {
      const n = s.split(fr);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function Ut(e) {
  let t = "";
  if (Y(e))
    t = e;
  else if (N(e))
    for (let s = 0; s < e.length; s++) {
      const n = Ut(e[s]);
      n && (t += n + " ");
    }
  else if (k(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const dr = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", hr = /* @__PURE__ */ Gs(dr);
function Qn(e) {
  return !!e || e === "";
}
function pr(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = Wt(e[n], t[n]);
  return s;
}
function Wt(e, t) {
  if (e === t) return !0;
  let s = pn(e), n = pn(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = $e(e), n = $e(t), s || n)
    return e === t;
  if (s = N(e), n = N(t), s || n)
    return s && n ? pr(e, t) : !1;
  if (s = k(e), n = k(t), s || n) {
    if (!s || !n)
      return !1;
    const i = Object.keys(e).length, r = Object.keys(t).length;
    if (i !== r)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), f = t.hasOwnProperty(o);
      if (l && !f || !l && f || !Wt(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function gr(e, t) {
  return e.findIndex((s) => Wt(s, t));
}
const ei = (e) => !!(e && e.__v_isRef === !0), te = (e) => Y(e) ? e : e == null ? "" : N(e) || k(e) && (e.toString === Yn || !F(e.toString)) ? ei(e) ? te(e.value) : JSON.stringify(e, ti, 2) : String(e), ti = (e, t) => ei(t) ? ti(e, t.value) : pt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, i], r) => (s[Cs(n, r) + " =>"] = i, s),
    {}
  )
} : as(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => Cs(s))
} : $e(t) ? Cs(t) : k(t) && !N(t) && !Xn(t) ? String(t) : t, Cs = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    $e(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let se;
class mr {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && se && (se.active ? (this.parent = se, this.index = (se.scopes || (se.scopes = [])).push(
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
        const i = this.scopes.slice();
        for (t = 0, s = i.length; t < s; t++)
          i[t].resume();
      }
      const n = this.effects.slice();
      for (t = 0, s = n.length; t < s; t++)
        n[t].resume();
    }
  }
  run(t) {
    if (this._active) {
      const s = se;
      try {
        return se = this, t();
      } finally {
        se = s;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = se, se = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (se === this)
        se = this.prevScope;
      else {
        let t = se;
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
        const i = this.scopes.slice();
        for (s = 0, n = i.length; s < n; s++)
          i[s].stop(!0);
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
function _r() {
  return se;
}
let z;
const Ts = /* @__PURE__ */ new WeakSet();
class si {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, se && (se.active ? se.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, Ts.has(this) && (Ts.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || ii(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, mn(this), ri(this);
    const t = z, s = xe;
    z = this, xe = !0;
    try {
      return this.fn();
    } finally {
      oi(this), z = t, xe = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        Zs(t);
      this.deps = this.depsTail = void 0, mn(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? Ts.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    $s(this) && this.run();
  }
  get dirty() {
    return $s(this);
  }
}
let ni = 0, Rt, Mt;
function ii(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Mt, Mt = e;
    return;
  }
  e.next = Rt, Rt = e;
}
function Ys() {
  ni++;
}
function Xs() {
  if (--ni > 0)
    return;
  if (Mt) {
    let t = Mt;
    for (Mt = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; Rt; ) {
    let t = Rt;
    for (Rt = void 0; t; ) {
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
function ri(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function oi(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const i = n.prevDep;
    n.version === -1 ? (n === s && (s = i), Zs(n), br(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = i;
  }
  e.deps = t, e.depsTail = s;
}
function $s(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (li(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function li(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === $t) || (e.globalVersion = $t, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !$s(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = z, n = xe;
  z = e, xe = !0;
  try {
    ri(e);
    const i = e.fn(e._value);
    (t.version === 0 || Ne(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    z = s, xe = n, oi(e), e.flags &= -3;
  }
}
function Zs(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: i } = e;
  if (n && (n.nextSub = i, e.prevSub = void 0), i && (i.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let r = s.computed.deps; r; r = r.nextDep)
      Zs(r, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function br(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let xe = !0;
const ci = [];
function We() {
  ci.push(xe), xe = !1;
}
function Be() {
  const e = ci.pop();
  xe = e === void 0 ? !0 : e;
}
function mn(e) {
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
let $t = 0;
class vr {
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
    if (!z || !xe || z === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== z)
      s = this.activeLink = new vr(z, this), z.deps ? (s.prevDep = z.depsTail, z.depsTail.nextDep = s, z.depsTail = s) : z.deps = z.depsTail = s, fi(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = z.depsTail, s.nextDep = void 0, z.depsTail.nextDep = s, z.depsTail = s, z.deps === s && (z.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, $t++, this.notify(t);
  }
  notify(t) {
    Ys();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      Xs();
    }
  }
}
function fi(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let n = t.deps; n; n = n.nextDep)
        fi(n);
    }
    const s = e.dep.subs;
    s !== e && (e.prevSub = s, s && (s.nextSub = e)), e.dep.subs = e;
  }
}
const Ds = /* @__PURE__ */ new WeakMap(), nt = /* @__PURE__ */ Symbol(
  ""
), js = /* @__PURE__ */ Symbol(
  ""
), Dt = /* @__PURE__ */ Symbol(
  ""
);
function re(e, t, s) {
  if (xe && z) {
    let n = Ds.get(e);
    n || Ds.set(e, n = /* @__PURE__ */ new Map());
    let i = n.get(s);
    i || (n.set(s, i = new Qs()), i.map = n, i.key = s), i.track();
  }
}
function Ve(e, t, s, n, i, r) {
  const o = Ds.get(e);
  if (!o) {
    $t++;
    return;
  }
  const l = (f) => {
    f && f.trigger();
  };
  if (Ys(), t === "clear")
    o.forEach(l);
  else {
    const f = N(e), d = f && qs(s);
    if (f && s === "length") {
      const a = Number(n);
      o.forEach((p, w) => {
        (w === "length" || w === Dt || !$e(w) && w >= a) && l(p);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(Dt)), t) {
        case "add":
          f ? d && l(o.get("length")) : (l(o.get(nt)), pt(e) && l(o.get(js)));
          break;
        case "delete":
          f || (l(o.get(nt)), pt(e) && l(o.get(js)));
          break;
        case "set":
          pt(e) && l(o.get(nt));
          break;
      }
  }
  Xs();
}
function ut(e) {
  const t = /* @__PURE__ */ K(e);
  return t === e ? t : (re(t, "iterate", Dt), /* @__PURE__ */ ye(e) ? t : t.map(we));
}
function ms(e) {
  return re(e = /* @__PURE__ */ K(e), "iterate", Dt), e;
}
function Me(e, t) {
  return /* @__PURE__ */ ke(e) ? bt(/* @__PURE__ */ it(e) ? we(t) : t) : we(t);
}
const yr = {
  __proto__: null,
  [Symbol.iterator]() {
    return Es(this, Symbol.iterator, (e) => Me(this, e));
  },
  concat(...e) {
    return ut(this).concat(
      ...e.map((t) => N(t) ? ut(t) : t)
    );
  },
  entries() {
    return Es(this, "entries", (e) => (e[1] = Me(this, e[1]), e));
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
      (s) => s.map((n) => Me(this, n)),
      arguments
    );
  },
  find(e, t) {
    return je(
      this,
      "find",
      e,
      t,
      (s) => Me(this, s),
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
      (s) => Me(this, s),
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
    return Os(this, "includes", e);
  },
  indexOf(...e) {
    return Os(this, "indexOf", e);
  },
  join(e) {
    return ut(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return Os(this, "lastIndexOf", e);
  },
  map(e, t) {
    return je(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return St(this, "pop");
  },
  push(...e) {
    return St(this, "push", e);
  },
  reduce(e, ...t) {
    return _n(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return _n(this, "reduceRight", e, t);
  },
  shift() {
    return St(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return je(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return St(this, "splice", e);
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
    return St(this, "unshift", e);
  },
  values() {
    return Es(this, "values", (e) => Me(this, e));
  }
};
function Es(e, t, s) {
  const n = ms(e), i = n[t]();
  return n !== e && !/* @__PURE__ */ ye(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = s(r.value)), r;
  }), i;
}
const xr = Array.prototype;
function je(e, t, s, n, i, r) {
  const o = ms(e), l = o !== e && !/* @__PURE__ */ ye(e), f = o[t];
  if (f !== xr[t]) {
    const p = f.apply(e, r);
    return l ? we(p) : p;
  }
  let d = s;
  o !== e && (l ? d = function(p, w) {
    return s.call(this, Me(e, p), w, e);
  } : s.length > 2 && (d = function(p, w) {
    return s.call(this, p, w, e);
  }));
  const a = f.call(o, d, n);
  return l && i ? i(a) : a;
}
function _n(e, t, s, n) {
  const i = ms(e), r = i !== e && !/* @__PURE__ */ ye(e);
  let o = s, l = !1;
  i !== e && (r ? (l = n.length === 0, o = function(d, a, p) {
    return l && (l = !1, d = Me(e, d)), s.call(this, d, Me(e, a), p, e);
  }) : s.length > 3 && (o = function(d, a, p) {
    return s.call(this, d, a, p, e);
  }));
  const f = i[t](o, ...n);
  return l ? Me(e, f) : f;
}
function Os(e, t, s) {
  const n = /* @__PURE__ */ K(e);
  re(n, "iterate", Dt);
  const i = n[t](...s);
  return (i === -1 || i === !1) && /* @__PURE__ */ sn(s[0]) ? (s[0] = /* @__PURE__ */ K(s[0]), n[t](...s)) : i;
}
function St(e, t, s = []) {
  We(), Ys();
  const n = (/* @__PURE__ */ K(e))[t].apply(e, s);
  return Xs(), Be(), n;
}
const wr = /* @__PURE__ */ Gs("__proto__,__v_isRef,__isVue"), ui = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter($e)
);
function Sr(e) {
  $e(e) || (e = String(e));
  const t = /* @__PURE__ */ K(this);
  return re(t, "has", e), t.hasOwnProperty(e);
}
class ai {
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
      return n === (i ? r ? Nr : gi : r ? pi : hi).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = N(t);
    if (!i) {
      let f;
      if (o && (f = yr[s]))
        return f;
      if (s === "hasOwnProperty")
        return Sr;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ le(t) ? t : n
    );
    if (($e(s) ? ui.has(s) : wr(s)) || (i || re(t, "get", s), r))
      return l;
    if (/* @__PURE__ */ le(l)) {
      const f = o && qs(s) ? l : l.value;
      return i && k(f) ? /* @__PURE__ */ Ls(f) : f;
    }
    return k(l) ? i ? /* @__PURE__ */ Ls(l) : /* @__PURE__ */ jt(l) : l;
  }
}
class di extends ai {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, i) {
    let r = t[s];
    const o = N(t) && qs(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ ke(r);
      if (!/* @__PURE__ */ ye(n) && !/* @__PURE__ */ ke(n) && (r = /* @__PURE__ */ K(r), n = /* @__PURE__ */ K(n)), !o && /* @__PURE__ */ le(r) && !/* @__PURE__ */ le(n))
        return d || (r.value = n), !0;
    }
    const l = o ? Number(s) < t.length : U(t, s), f = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ le(t) ? t : i
    );
    return t === /* @__PURE__ */ K(i) && f && (l ? Ne(n, r) && Ve(t, "set", s, n) : Ve(t, "add", s, n)), f;
  }
  deleteProperty(t, s) {
    const n = U(t, s);
    t[s];
    const i = Reflect.deleteProperty(t, s);
    return i && n && Ve(t, "delete", s, void 0), i;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!$e(s) || !ui.has(s)) && re(t, "has", s), n;
  }
  ownKeys(t) {
    return re(
      t,
      "iterate",
      N(t) ? "length" : nt
    ), Reflect.ownKeys(t);
  }
}
class Cr extends ai {
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
const Tr = /* @__PURE__ */ new di(), Er = /* @__PURE__ */ new Cr(), Or = /* @__PURE__ */ new di(!0);
const Hs = (e) => e, Jt = (e) => Reflect.getPrototypeOf(e);
function Ar(e, t, s) {
  return function(...n) {
    const i = this.__v_raw, r = /* @__PURE__ */ K(i), o = pt(r), l = e === "entries" || e === Symbol.iterator && o, f = e === "keys" && o, d = i[e](...n), a = s ? Hs : t ? bt : we;
    return !t && re(
      r,
      "iterate",
      f ? js : nt
    ), ie(
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
function Pr(e, t) {
  const s = {
    get(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ K(r), l = /* @__PURE__ */ K(i);
      e || (Ne(i, l) && re(o, "get", i), re(o, "get", l));
      const { has: f } = Jt(o), d = t ? Hs : e ? bt : we;
      if (f.call(o, i))
        return d(r.get(i));
      if (f.call(o, l))
        return d(r.get(l));
      r !== o && r.get(i);
    },
    get size() {
      const i = this.__v_raw;
      return !e && re(/* @__PURE__ */ K(i), "iterate", nt), i.size;
    },
    has(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ K(r), l = /* @__PURE__ */ K(i);
      return e || (Ne(i, l) && re(o, "has", i), re(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, f = /* @__PURE__ */ K(l), d = t ? Hs : e ? bt : we;
      return !e && re(f, "iterate", nt), l.forEach((a, p) => i.call(r, d(a), d(p), o));
    }
  };
  return ie(
    s,
    e ? {
      add: qt("add"),
      set: qt("set"),
      delete: qt("delete"),
      clear: qt("clear")
    } : {
      add(i) {
        const r = /* @__PURE__ */ K(this), o = Jt(r), l = /* @__PURE__ */ K(i), f = !t && !/* @__PURE__ */ ye(i) && !/* @__PURE__ */ ke(i) ? l : i;
        return o.has.call(r, f) || Ne(i, f) && o.has.call(r, i) || Ne(l, f) && o.has.call(r, l) || (r.add(f), Ve(r, "add", f, f)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ ye(r) && !/* @__PURE__ */ ke(r) && (r = /* @__PURE__ */ K(r));
        const o = /* @__PURE__ */ K(this), { has: l, get: f } = Jt(o);
        let d = l.call(o, i);
        d || (i = /* @__PURE__ */ K(i), d = l.call(o, i));
        const a = f.call(o, i);
        return o.set(i, r), d ? Ne(r, a) && Ve(o, "set", i, r) : Ve(o, "add", i, r), this;
      },
      delete(i) {
        const r = /* @__PURE__ */ K(this), { has: o, get: l } = Jt(r);
        let f = o.call(r, i);
        f || (i = /* @__PURE__ */ K(i), f = o.call(r, i)), l && l.call(r, i);
        const d = r.delete(i);
        return f && Ve(r, "delete", i, void 0), d;
      },
      clear() {
        const i = /* @__PURE__ */ K(this), r = i.size !== 0, o = i.clear();
        return r && Ve(
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
    s[i] = Ar(i, e, t);
  }), s;
}
function en(e, t) {
  const s = Pr(e, t);
  return (n, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? n : Reflect.get(
    U(s, i) && i in n ? s : n,
    i,
    r
  );
}
const Rr = {
  get: /* @__PURE__ */ en(!1, !1)
}, Mr = {
  get: /* @__PURE__ */ en(!1, !0)
}, Ir = {
  get: /* @__PURE__ */ en(!0, !1)
};
const hi = /* @__PURE__ */ new WeakMap(), pi = /* @__PURE__ */ new WeakMap(), gi = /* @__PURE__ */ new WeakMap(), Nr = /* @__PURE__ */ new WeakMap();
function Fr(e) {
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
function jt(e) {
  return /* @__PURE__ */ ke(e) ? e : tn(
    e,
    !1,
    Tr,
    Rr,
    hi
  );
}
// @__NO_SIDE_EFFECTS__
function $r(e) {
  return tn(
    e,
    !1,
    Or,
    Mr,
    pi
  );
}
// @__NO_SIDE_EFFECTS__
function Ls(e) {
  return tn(
    e,
    !0,
    Er,
    Ir,
    gi
  );
}
function tn(e, t, s, n, i) {
  if (!k(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const r = i.get(e);
  if (r)
    return r;
  const o = Fr(rr(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? n : s
  );
  return i.set(e, l), l;
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
function ye(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function sn(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function K(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ K(t) : e;
}
function Dr(e) {
  return !U(e, "__v_skip") && Object.isExtensible(e) && Zn(e, "__v_skip", !0), e;
}
const we = (e) => k(e) ? /* @__PURE__ */ jt(e) : e, bt = (e) => k(e) ? /* @__PURE__ */ Ls(e) : e;
// @__NO_SIDE_EFFECTS__
function le(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function _e(e) {
  return jr(e, !1);
}
function jr(e, t) {
  return /* @__PURE__ */ le(e) ? e : new Hr(e, t);
}
class Hr {
  constructor(t, s) {
    this.dep = new Qs(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ K(t), this._value = s ? t : we(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ ye(t) || /* @__PURE__ */ ke(t);
    t = n ? t : /* @__PURE__ */ K(t), Ne(t, s) && (this._rawValue = t, this._value = n ? t : we(t), this.dep.trigger());
  }
}
function Lr(e) {
  return /* @__PURE__ */ le(e) ? e.value : e;
}
const Vr = {
  get: (e, t, s) => t === "__v_raw" ? e : Lr(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const i = e[t];
    return /* @__PURE__ */ le(i) && !/* @__PURE__ */ le(s) ? (i.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function mi(e) {
  return /* @__PURE__ */ it(e) ? e : new Proxy(e, Vr);
}
class Kr {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Qs(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = $t - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    z !== this)
      return ii(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return li(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function Ur(e, t, s = !1) {
  let n, i;
  return F(e) ? n = e : (n = e.get, i = e.set), new Kr(n, i, s);
}
const zt = {}, es = /* @__PURE__ */ new WeakMap();
let tt;
function Wr(e, t = !1, s = tt) {
  if (s) {
    let n = es.get(s);
    n || es.set(s, n = []), n.push(e);
  }
}
function Br(e, t, s = q) {
  const { immediate: n, deep: i, once: r, scheduler: o, augmentJob: l, call: f } = s, d = (P) => i ? P : /* @__PURE__ */ ye(P) || i === !1 || i === 0 ? Ke(P, 1) : Ke(P);
  let a, p, w, E, L = !1, M = !1;
  if (/* @__PURE__ */ le(e) ? (p = () => e.value, L = /* @__PURE__ */ ye(e)) : /* @__PURE__ */ it(e) ? (p = () => d(e), L = !0) : N(e) ? (M = !0, L = e.some((P) => /* @__PURE__ */ it(P) || /* @__PURE__ */ ye(P)), p = () => e.map((P) => {
    if (/* @__PURE__ */ le(P))
      return P.value;
    if (/* @__PURE__ */ it(P))
      return d(P);
    if (F(P))
      return f ? f(P, 2) : P();
  })) : F(e) ? t ? p = f ? () => f(e, 2) : e : p = () => {
    if (w) {
      We();
      try {
        w();
      } finally {
        Be();
      }
    }
    const P = tt;
    tt = a;
    try {
      return f ? f(e, 3, [E]) : e(E);
    } finally {
      tt = P;
    }
  } : p = Fe, t && i) {
    const P = p, X = i === !0 ? 1 / 0 : i;
    p = () => Ke(P(), X);
  }
  const V = _r(), G = () => {
    a.stop(), V && V.active && Js(V.effects, a);
  };
  if (r && t) {
    const P = t;
    t = (...X) => {
      const ge = P(...X);
      return G(), ge;
    };
  }
  let $ = M ? new Array(e.length).fill(zt) : zt;
  const W = (P) => {
    if (!(!(a.flags & 1) || !a.dirty && !P))
      if (t) {
        const X = a.run();
        if (P || i || L || (M ? X.some((ge, ae) => Ne(ge, $[ae])) : Ne(X, $))) {
          w && w();
          const ge = tt;
          tt = a;
          try {
            const ae = [
              X,
              // pass undefined as the old value when it's changed for the first time
              $ === zt ? void 0 : M && $[0] === zt ? [] : $,
              E
            ];
            $ = X, f ? f(t, 3, ae) : (
              // @ts-expect-error
              t(...ae)
            );
          } finally {
            tt = ge;
          }
        }
      } else
        a.run();
  };
  return l && l(W), a = new si(p), a.scheduler = o ? () => o(W, !1) : W, E = (P) => Wr(P, !1, a), w = a.onStop = () => {
    const P = es.get(a);
    if (P) {
      if (f)
        f(P, 4);
      else
        for (const X of P) X();
      es.delete(a);
    }
  }, t ? n ? W(!0) : $ = a.run() : o ? o(W.bind(null, !0), !0) : a.run(), G.pause = a.pause.bind(a), G.resume = a.resume.bind(a), G.stop = G, G;
}
function Ke(e, t = 1 / 0, s) {
  if (t <= 0 || !k(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ le(e))
    Ke(e.value, t, s);
  else if (N(e))
    for (let n = 0; n < e.length; n++)
      Ke(e[n], t, s);
  else if (as(e) || pt(e))
    e.forEach((n) => {
      Ke(n, t, s);
    });
  else if (Xn(e)) {
    for (const n in e)
      Ke(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && Ke(e[n], t, s);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function Bt(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (i) {
    _s(i, t, s);
  }
}
function Se(e, t, s, n) {
  if (F(e)) {
    const i = Bt(e, t, s, n);
    return i && zn(i) && i.catch((r) => {
      _s(r, t, s);
    }), i;
  }
  if (N(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(Se(e[r], t, s, n));
    return i;
  }
}
function _s(e, t, s, n = !0) {
  const i = t ? t.vnode : null, { errorHandler: r, throwUnhandledErrorInProduction: o } = t && t.appContext.config || q;
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
    if (r) {
      We(), Bt(r, null, 10, [
        e,
        f,
        d
      ]), Be();
      return;
    }
  }
  kr(e, s, i, n, o);
}
function kr(e, t, s, n = !0, i = !1) {
  if (i)
    throw e;
  console.error(e);
}
const fe = [];
let Re = -1;
const gt = [];
let Ye = null, at = 0;
const _i = /* @__PURE__ */ Promise.resolve();
let ts = null;
function bi(e) {
  const t = ts || _i;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Gr(e) {
  let t = Re + 1, s = fe.length;
  for (; t < s; ) {
    const n = t + s >>> 1, i = fe[n], r = Ht(i);
    r < e || r === e && i.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function nn(e) {
  if (!(e.flags & 1)) {
    const t = Ht(e), s = fe[fe.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Ht(s) ? fe.push(e) : fe.splice(Gr(t), 0, e), e.flags |= 1, vi();
  }
}
function vi() {
  ts || (ts = _i.then(xi));
}
function Jr(e) {
  N(e) ? gt.push(...e) : Ye && e.id === -1 ? Ye.splice(at + 1, 0, e) : e.flags & 1 || (gt.push(e), e.flags |= 1), vi();
}
function bn(e, t, s = Re + 1) {
  for (; s < fe.length; s++) {
    const n = fe[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      fe.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function yi(e) {
  if (gt.length) {
    const t = [...new Set(gt)].sort(
      (s, n) => Ht(s) - Ht(n)
    );
    if (gt.length = 0, Ye) {
      Ye.push(...t);
      return;
    }
    for (Ye = t, at = 0; at < Ye.length; at++) {
      const s = Ye[at];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    Ye = null, at = 0;
  }
}
const Ht = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function xi(e) {
  try {
    for (Re = 0; Re < fe.length; Re++) {
      const t = fe[Re];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Bt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Re < fe.length; Re++) {
      const t = fe[Re];
      t && (t.flags &= -2);
    }
    Re = -1, fe.length = 0, yi(), ts = null, (fe.length || gt.length) && xi();
  }
}
let be = null, wi = null;
function ss(e) {
  const t = be;
  return be = e, wi = e && e.type.__scopeId || null, t;
}
function qr(e, t = be, s) {
  if (!t || e._n)
    return e;
  const n = (...i) => {
    n._d && Rn(-1);
    const r = ss(t), o = rt.length;
    let l;
    try {
      l = e(...i);
    } finally {
      for (let f = rt.length; f > o; f--) qi();
      ss(r), n._d && Rn(1);
    }
    return l;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function As(e, t) {
  if (be === null)
    return e;
  const s = xs(be), n = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [r, o, l, f = q] = t[i];
    r && (F(r) && (r = {
      mounted: r,
      updated: r
    }), r.deep && Ke(o), n.push({
      dir: r,
      instance: s,
      value: o,
      oldValue: void 0,
      arg: l,
      modifiers: f
    }));
  }
  return e;
}
function Qe(e, t, s, n) {
  const i = e.dirs, r = t && t.dirs;
  for (let o = 0; o < i.length; o++) {
    const l = i[o];
    r && (l.oldValue = r[o].value);
    let f = l.dir[n];
    f && (We(), Se(f, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Be());
  }
}
function zr(e, t) {
  if (oe) {
    let s = oe.provides;
    const n = oe.parent && oe.parent.provides;
    n === s && (s = oe.provides = Object.create(n)), s[e] = t;
  }
}
function Xt(e, t, s = !1) {
  const n = zo();
  if (n || mt) {
    let i = mt ? mt._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return s && F(t) ? t.call(n && n.proxy) : t;
  }
}
const Yr = /* @__PURE__ */ Symbol.for("v-scx"), Xr = () => Xt(Yr);
function Zt(e, t, s) {
  return Si(e, t, s);
}
function Si(e, t, s = q) {
  const { immediate: n, deep: i, flush: r, once: o } = s, l = ie({}, s), f = t && n || !t && r !== "post";
  let d;
  if (Vt) {
    if (r === "sync") {
      const E = Xr();
      d = E.__watcherHandles || (E.__watcherHandles = []);
    } else if (!f) {
      const E = () => {
      };
      return E.stop = Fe, E.resume = Fe, E.pause = Fe, E;
    }
  }
  const a = oe;
  l.call = (E, L, M) => Se(E, a, L, M);
  let p = !1;
  r === "post" ? l.scheduler = (E) => {
    he(E, a && a.suspense);
  } : r !== "sync" && (p = !0, l.scheduler = (E, L) => {
    L ? E() : nn(E);
  }), l.augmentJob = (E) => {
    t && (E.flags |= 4), p && (E.flags |= 2, a && (E.id = a.uid, E.i = a));
  };
  const w = Br(e, t, l);
  return Vt && (d ? d.push(w) : f && w()), w;
}
function Zr(e, t, s) {
  const n = this.proxy, i = Y(e) ? e.includes(".") ? Ci(n, e) : () => n[e] : e.bind(n, n);
  let r;
  F(t) ? r = t : (r = t.handler, s = t);
  const o = kt(this), l = Si(i, r.bind(n), s);
  return o(), l;
}
function Ci(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let i = 0; i < s.length && n; i++)
      n = n[s[i]];
    return n;
  };
}
const Qr = /* @__PURE__ */ Symbol("_vte"), eo = (e) => e.__isTeleport, Ps = /* @__PURE__ */ Symbol("_leaveCb");
function rn(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, rn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Ti(e, t) {
  return F(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    ie({ name: e.name }, t, { setup: e })
  ) : e;
}
function Ei(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function vn(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const ns = /* @__PURE__ */ new WeakMap();
function It(e, t, s, n, i = !1) {
  if (N(e)) {
    e.forEach(
      (M, V) => It(
        M,
        t && (N(t) ? t[V] : t),
        s,
        n,
        i
      )
    );
    return;
  }
  if (Nt(n) && !i) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && It(e, t, s, n.component.subTree);
    return;
  }
  const r = n.shapeFlag & 4 ? xs(n.component) : n.el, o = i ? null : r, { i: l, r: f } = e, d = t && t.r, a = l.refs === q ? l.refs = {} : l.refs, p = l.setupState, w = /* @__PURE__ */ K(p), E = p === q ? qn : (M) => vn(a, M) ? !1 : U(w, M), L = (M, V) => !(V && vn(a, V));
  if (d != null && d !== f) {
    if (yn(t), Y(d))
      a[d] = null, E(d) && (p[d] = null);
    else if (/* @__PURE__ */ le(d)) {
      const M = t;
      L(d, M.k) && (d.value = null), M.k && (a[M.k] = null);
    }
  }
  if (F(f))
    Bt(f, l, 12, [o, a]);
  else {
    const M = Y(f), V = /* @__PURE__ */ le(f);
    if (M || V) {
      const G = () => {
        if (e.f) {
          const $ = M ? E(f) ? p[f] : a[f] : L() || !e.k ? f.value : a[e.k];
          if (i)
            N($) && Js($, r);
          else if (N($))
            $.includes(r) || $.push(r);
          else if (M)
            a[f] = [r], E(f) && (p[f] = a[f]);
          else {
            const W = [r];
            L(f, e.k) && (f.value = W), e.k && (a[e.k] = W);
          }
        } else M ? (a[f] = o, E(f) && (p[f] = o)) : V && (L(f, e.k) && (f.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const $ = () => {
          G(), ns.delete(e);
        };
        $.id = -1, ns.set(e, $), he($, s);
      } else
        yn(e), G();
    }
  }
}
function yn(e) {
  const t = ns.get(e);
  t && (t.flags |= 8, ns.delete(e));
}
gs().requestIdleCallback;
gs().cancelIdleCallback;
const Nt = (e) => !!e.type.__asyncLoader, Oi = (e) => e.type.__isKeepAlive;
function to(e, t) {
  Ai(e, "a", t);
}
function so(e, t) {
  Ai(e, "da", t);
}
function Ai(e, t, s = oe) {
  const n = e.__wdc || (e.__wdc = () => {
    let i = s;
    for (; i; ) {
      if (i.isDeactivated)
        return;
      i = i.parent;
    }
    return e();
  });
  if (bs(t, n, s), s) {
    let i = s.parent;
    for (; i && i.parent; )
      Oi(i.parent.vnode) && no(n, t, s, i), i = i.parent;
  }
}
function no(e, t, s, n) {
  const i = bs(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Ri(() => {
    Js(n[t], i);
  }, s);
}
function bs(e, t, s = oe, n = !1) {
  if (s) {
    const i = s[e] || (s[e] = []), r = t.__weh || (t.__weh = (...o) => {
      We();
      const l = kt(s), f = Se(t, s, e, o);
      return l(), Be(), f;
    });
    return n ? i.unshift(r) : i.push(r), r;
  }
}
const Ge = (e) => (t, s = oe) => {
  (!Vt || e === "sp") && bs(e, (...n) => t(...n), s);
}, io = Ge("bm"), Pi = Ge("m"), ro = Ge(
  "bu"
), oo = Ge("u"), lo = Ge(
  "bum"
), Ri = Ge("um"), co = Ge(
  "sp"
), fo = Ge("rtg"), uo = Ge("rtc");
function ao(e, t = oe) {
  bs("ec", e, t);
}
const ho = "components";
function po(e, t) {
  return mo(ho, e, !0, t) || e;
}
const go = /* @__PURE__ */ Symbol.for("v-ndc");
function mo(e, t, s = !0, n = !1) {
  const i = be || oe;
  if (i) {
    const r = i.type;
    {
      const l = el(
        r,
        !1
      );
      if (l && (l === t || l === ue(t) || l === hs(ue(t))))
        return r;
    }
    const o = (
      // local registration
      // check instance[type] first which is resolved for options API
      xn(i[e] || r[e], t) || // global registration
      xn(i.appContext[e], t)
    );
    return !o && n ? r : o;
  }
}
function xn(e, t) {
  return e && (e[t] || e[ue(t)] || e[hs(ue(t))]);
}
function Tt(e, t, s, n) {
  let i;
  const r = s, o = N(e);
  if (o || Y(e)) {
    const l = o && /* @__PURE__ */ it(e);
    let f = !1, d = !1;
    l && (f = !/* @__PURE__ */ ye(e), d = /* @__PURE__ */ ke(e), e = ms(e)), i = new Array(e.length);
    for (let a = 0, p = e.length; a < p; a++)
      i[a] = t(
        f ? d ? bt(we(e[a])) : we(e[a]) : e[a],
        a,
        void 0,
        r
      );
  } else if (typeof e == "number") {
    i = new Array(e);
    for (let l = 0; l < e; l++)
      i[l] = t(l + 1, l, void 0, r);
  } else if (k(e))
    if (e[Symbol.iterator])
      i = Array.from(
        e,
        (l, f) => t(l, f, void 0, r)
      );
    else {
      const l = Object.keys(e);
      i = new Array(l.length);
      for (let f = 0, d = l.length; f < d; f++) {
        const a = l[f];
        i[f] = t(e[a], a, f, r);
      }
    }
  else
    i = [];
  return i;
}
const Vs = (e) => e ? Zi(e) ? xs(e) : Vs(e.parent) : null, Ft = (
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
    $parent: (e) => Vs(e.parent),
    $root: (e) => Vs(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Ii(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      nn(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = bi.bind(e.proxy)),
    $watch: (e) => Zr.bind(e)
  })
), Rs = (e, t) => e !== q && !e.__isScriptSetup && U(e, t), _o = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: i, props: r, accessCache: o, type: l, appContext: f } = e;
    if (t[0] !== "$") {
      const w = o[t];
      if (w !== void 0)
        switch (w) {
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
        if (Rs(n, t))
          return o[t] = 1, n[t];
        if (i !== q && U(i, t))
          return o[t] = 2, i[t];
        if (U(r, t))
          return o[t] = 3, r[t];
        if (s !== q && U(s, t))
          return o[t] = 4, s[t];
        Ks && (o[t] = 0);
      }
    }
    const d = Ft[t];
    let a, p;
    if (d)
      return t === "$attrs" && re(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== q && U(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      p = f.config.globalProperties, U(p, t)
    )
      return p[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: i, ctx: r } = e;
    return Rs(i, t) ? (i[t] = s, !0) : n !== q && U(n, t) ? (n[t] = s, !0) : U(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: i, props: r, type: o }
  }, l) {
    let f;
    return !!(s[l] || e !== q && l[0] !== "$" && U(e, l) || Rs(t, l) || U(r, l) || U(n, l) || U(Ft, l) || U(i.config.globalProperties, l) || (f = o.__cssModules) && f[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : U(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function wn(e) {
  return N(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let Ks = !0;
function bo(e) {
  const t = Ii(e), s = e.proxy, n = e.ctx;
  Ks = !1, t.beforeCreate && Sn(t.beforeCreate, e, "bc");
  const {
    // state
    data: i,
    computed: r,
    methods: o,
    watch: l,
    provide: f,
    inject: d,
    // lifecycle
    created: a,
    beforeMount: p,
    mounted: w,
    beforeUpdate: E,
    updated: L,
    activated: M,
    deactivated: V,
    beforeDestroy: G,
    beforeUnmount: $,
    destroyed: W,
    unmounted: P,
    render: X,
    renderTracked: ge,
    renderTriggered: ae,
    errorCaptured: Ce,
    serverPrefetch: lt,
    // public API
    expose: De,
    inheritAttrs: Je,
    // assets
    components: ct,
    directives: ft,
    filters: qe
  } = t;
  if (d && vo(d, n, null), o)
    for (const D in o) {
      const T = o[D];
      F(T) && (n[D] = T.bind(s));
    }
  if (i) {
    const D = i.call(s, s);
    k(D) && (e.data = /* @__PURE__ */ jt(D));
  }
  if (Ks = !0, r)
    for (const D in r) {
      const T = r[D], de = F(T) ? T.bind(s, s) : F(T.get) ? T.get.bind(s, s) : Fe, ze = !F(T) && F(T.set) ? T.set.bind(s) : Fe, Ze = sl({
        get: de,
        set: ze
      });
      Object.defineProperty(n, D, {
        enumerable: !0,
        configurable: !0,
        get: () => Ze.value,
        set: (Te) => Ze.value = Te
      });
    }
  if (l)
    for (const D in l)
      Mi(l[D], n, s, D);
  if (f) {
    const D = F(f) ? f.call(s) : f;
    Reflect.ownKeys(D).forEach((T) => {
      zr(T, D[T]);
    });
  }
  a && Sn(a, e, "c");
  function S(D, T) {
    N(T) ? T.forEach((de) => D(de.bind(s))) : T && D(T.bind(s));
  }
  if (S(io, p), S(Pi, w), S(ro, E), S(oo, L), S(to, M), S(so, V), S(ao, Ce), S(uo, ge), S(fo, ae), S(lo, $), S(Ri, P), S(co, lt), N(De))
    if (De.length) {
      const D = e.exposed || (e.exposed = {});
      De.forEach((T) => {
        Object.defineProperty(D, T, {
          get: () => s[T],
          set: (de) => s[T] = de,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  X && e.render === Fe && (e.render = X), Je != null && (e.inheritAttrs = Je), ct && (e.components = ct), ft && (e.directives = ft), lt && Ei(e);
}
function vo(e, t, s = Fe) {
  N(e) && (e = Us(e));
  for (const n in e) {
    const i = e[n];
    let r;
    k(i) ? "default" in i ? r = Xt(
      i.from || n,
      i.default,
      !0
    ) : r = Xt(i.from || n) : r = Xt(i), /* @__PURE__ */ le(r) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => r.value,
      set: (o) => r.value = o
    }) : t[n] = r;
  }
}
function Sn(e, t, s) {
  Se(
    N(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Mi(e, t, s, n) {
  let i = n.includes(".") ? Ci(s, n) : () => s[n];
  if (Y(e)) {
    const r = t[e];
    F(r) && Zt(i, r);
  } else if (F(e))
    Zt(i, e.bind(s));
  else if (k(e))
    if (N(e))
      e.forEach((r) => Mi(r, t, s, n));
    else {
      const r = F(e.handler) ? e.handler.bind(s) : t[e.handler];
      F(r) && Zt(i, r, e);
    }
}
function Ii(e) {
  const t = e.type, { mixins: s, extends: n } = t, {
    mixins: i,
    optionsCache: r,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = r.get(t);
  let f;
  return l ? f = l : !i.length && !s && !n ? f = t : (f = {}, i.length && i.forEach(
    (d) => is(f, d, o, !0)
  ), is(f, t, o)), k(t) && r.set(t, f), f;
}
function is(e, t, s, n = !1) {
  const { mixins: i, extends: r } = t;
  r && is(e, r, s, !0), i && i.forEach(
    (o) => is(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = yo[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const yo = {
  data: Cn,
  props: Tn,
  emits: Tn,
  // objects
  methods: Et,
  computed: Et,
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
  components: Et,
  directives: Et,
  // watch
  watch: wo,
  // provide / inject
  provide: Cn,
  inject: xo
};
function Cn(e, t) {
  return t ? e ? function() {
    return ie(
      F(e) ? e.call(this, this) : e,
      F(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function xo(e, t) {
  return Et(Us(e), Us(t));
}
function Us(e) {
  if (N(e)) {
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
function Et(e, t) {
  return e ? ie(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Tn(e, t) {
  return e ? N(e) && N(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : ie(
    /* @__PURE__ */ Object.create(null),
    wn(e),
    wn(t ?? {})
  ) : t;
}
function wo(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = ie(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = ce(e[n], t[n]);
  return s;
}
function Ni() {
  return {
    app: null,
    config: {
      isNativeTag: qn,
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
let So = 0;
function Co(e, t) {
  return function(n, i = null) {
    F(n) || (n = ie({}, n)), i != null && !k(i) && (i = null);
    const r = Ni(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let f = !1;
    const d = r.app = {
      _uid: So++,
      _component: n,
      _props: i,
      _container: null,
      _context: r,
      _instance: null,
      version: nl,
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
      mount(a, p, w) {
        if (!f) {
          const E = d._ceVNode || Ue(n, i);
          return E.appContext = r, w === !0 ? w = "svg" : w === !1 && (w = void 0), e(E, a, w), f = !0, d._container = a, a.__vue_app__ = d, xs(E.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        f && (Se(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, p) {
        return r.provides[a] = p, d;
      },
      runWithContext(a) {
        const p = mt;
        mt = d;
        try {
          return a();
        } finally {
          mt = p;
        }
      }
    };
    return d;
  };
}
let mt = null;
const To = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${ue(t)}Modifiers`] || e[`${ot(t)}Modifiers`];
function Eo(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || q;
  let i = s;
  const r = t.startsWith("update:"), o = r && To(n, t.slice(7));
  o && (o.trim && (i = s.map((a) => Y(a) ? a.trim() : a)), o.number && (i = s.map(ps)));
  let l, f = n[l = Ss(t)] || // also try camelCase event handler (#2249)
  n[l = Ss(ue(t))];
  !f && r && (f = n[l = Ss(ot(t))]), f && Se(
    f,
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
    e.emitted[l] = !0, Se(
      d,
      e,
      6,
      i
    );
  }
}
const Oo = /* @__PURE__ */ new WeakMap();
function Fi(e, t, s = !1) {
  const n = s ? Oo : t.emitsCache, i = n.get(e);
  if (i !== void 0)
    return i;
  const r = e.emits;
  let o = {}, l = !1;
  if (!F(e)) {
    const f = (d) => {
      const a = Fi(d, t, !0);
      a && (l = !0, ie(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  return !r && !l ? (k(e) && n.set(e, null), null) : (N(r) ? r.forEach((f) => o[f] = null) : ie(o, r), k(e) && n.set(e, o), o);
}
function vs(e, t) {
  return !e || !fs(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), U(e, t[0].toLowerCase() + t.slice(1)) || U(e, ot(t)) || U(e, t));
}
function En(e) {
  const {
    type: t,
    vnode: s,
    proxy: n,
    withProxy: i,
    propsOptions: [r],
    slots: o,
    attrs: l,
    emit: f,
    render: d,
    renderCache: a,
    props: p,
    data: w,
    setupState: E,
    ctx: L,
    inheritAttrs: M
  } = e, V = ss(e);
  let G, $;
  try {
    if (s.shapeFlag & 4) {
      const P = i || n, X = P;
      G = Ie(
        d.call(
          X,
          P,
          a,
          p,
          E,
          w,
          L
        )
      ), $ = l;
    } else {
      const P = t;
      G = Ie(
        P.length > 1 ? P(
          p,
          { attrs: l, slots: o, emit: f }
        ) : P(
          p,
          null
        )
      ), $ = t.props ? l : Ao(l);
    }
  } catch (P) {
    rt.length = 0, _s(P, e, 1), G = Ue(Xe);
  }
  let W = G;
  if ($ && M !== !1) {
    const P = Object.keys($), { shapeFlag: X } = W;
    P.length && X & 7 && (r && P.some(us) && ($ = Po(
      $,
      r
    )), W = vt(W, $, !1, !0));
  }
  return s.dirs && (W = vt(W, null, !1, !0), W.dirs = W.dirs ? W.dirs.concat(s.dirs) : s.dirs), s.transition && rn(W, s.transition), G = W, ss(V), G;
}
const Ao = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || fs(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, Po = (e, t) => {
  const s = {};
  for (const n in e)
    (!us(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function Ro(e, t, s) {
  const { props: n, children: i, component: r } = e, { props: o, children: l, patchFlag: f } = t, d = r.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (s && f >= 0) {
    if (f & 1024)
      return !0;
    if (f & 16)
      return n ? On(n, o, d) : !!o;
    if (f & 8) {
      const a = t.dynamicProps;
      for (let p = 0; p < a.length; p++) {
        const w = a[p];
        if ($i(o, n, w) && !vs(d, w))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? On(n, o, d) : !0 : !!o;
  return !1;
}
function On(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < n.length; i++) {
    const r = n[i];
    if ($i(t, e, r) && !vs(s, r))
      return !0;
  }
  return !1;
}
function $i(e, t, s) {
  const n = e[s], i = t[s];
  return s === "style" && k(n) && k(i) ? !Wt(n, i) : n !== i;
}
function Mo({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const i = t.subTree;
    if (i.suspense && i.suspense.activeBranch === e && (i.suspense.vnode.el = i.el = n, e = i), i === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const Di = {}, ji = () => Object.create(Di), Hi = (e) => Object.getPrototypeOf(e) === Di;
function Io(e, t, s, n = !1) {
  const i = {}, r = ji();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), Li(e, t, i, r);
  for (const o in e.propsOptions[0])
    o in i || (i[o] = void 0);
  s ? e.props = n ? i : /* @__PURE__ */ $r(i) : e.type.props ? e.props = i : e.props = r, e.attrs = r;
}
function No(e, t, s, n) {
  const {
    props: i,
    attrs: r,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ K(i), [f] = e.propsOptions;
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
        let w = a[p];
        if (vs(e.emitsOptions, w))
          continue;
        const E = t[w];
        if (f)
          if (U(r, w))
            E !== r[w] && (r[w] = E, d = !0);
          else {
            const L = ue(w);
            i[L] = Ws(
              f,
              l,
              L,
              E,
              e,
              !1
            );
          }
        else
          E !== r[w] && (r[w] = E, d = !0);
      }
    }
  } else {
    Li(e, t, i, r) && (d = !0);
    let a;
    for (const p in l)
      (!t || // for camelCase
      !U(t, p) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = ot(p)) === p || !U(t, a))) && (f ? s && // for camelCase
      (s[p] !== void 0 || // for kebab-case
      s[a] !== void 0) && (i[p] = Ws(
        f,
        l,
        p,
        void 0,
        e,
        !0
      )) : delete i[p]);
    if (r !== l)
      for (const p in r)
        (!t || !U(t, p)) && (delete r[p], d = !0);
  }
  d && Ve(e.attrs, "set", "");
}
function Li(e, t, s, n) {
  const [i, r] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let f in t) {
      if (Pt(f))
        continue;
      const d = t[f];
      let a;
      i && U(i, a = ue(f)) ? !r || !r.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : vs(e.emitsOptions, f) || (!(f in n) || d !== n[f]) && (n[f] = d, o = !0);
    }
  if (r) {
    const f = /* @__PURE__ */ K(s), d = l || q;
    for (let a = 0; a < r.length; a++) {
      const p = r[a];
      s[p] = Ws(
        i,
        f,
        p,
        d[p],
        e,
        !U(d, p)
      );
    }
  }
  return o;
}
function Ws(e, t, s, n, i, r) {
  const o = e[s];
  if (o != null) {
    const l = U(o, "default");
    if (l && n === void 0) {
      const f = o.default;
      if (o.type !== Function && !o.skipFactory && F(f)) {
        const { propsDefaults: d } = i;
        if (s in d)
          n = d[s];
        else {
          const a = kt(i);
          n = d[s] = f.call(
            null,
            t
          ), a();
        }
      } else
        n = f;
      i.ce && i.ce._setProp(s, n);
    }
    o[
      0
      /* shouldCast */
    ] && (r && !l ? n = !1 : o[
      1
      /* shouldCastTrue */
    ] && (n === "" || n === ot(s)) && (n = !0));
  }
  return n;
}
const Fo = /* @__PURE__ */ new WeakMap();
function Vi(e, t, s = !1) {
  const n = s ? Fo : t.propsCache, i = n.get(e);
  if (i)
    return i;
  const r = e.props, o = {}, l = [];
  let f = !1;
  if (!F(e)) {
    const a = (p) => {
      f = !0;
      const [w, E] = Vi(p, t, !0);
      ie(o, w), E && l.push(...E);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!r && !f)
    return k(e) && n.set(e, ht), ht;
  if (N(r))
    for (let a = 0; a < r.length; a++) {
      const p = ue(r[a]);
      An(p) && (o[p] = q);
    }
  else if (r)
    for (const a in r) {
      const p = ue(a);
      if (An(p)) {
        const w = r[a], E = o[p] = N(w) || F(w) ? { type: w } : ie({}, w), L = E.type;
        let M = !1, V = !0;
        if (N(L))
          for (let G = 0; G < L.length; ++G) {
            const $ = L[G], W = F($) && $.name;
            if (W === "Boolean") {
              M = !0;
              break;
            } else W === "String" && (V = !1);
          }
        else
          M = F(L) && L.name === "Boolean";
        E[
          0
          /* shouldCast */
        ] = M, E[
          1
          /* shouldCastTrue */
        ] = V, (M || U(E, "default")) && l.push(p);
      }
    }
  const d = [o, l];
  return k(e) && n.set(e, d), d;
}
function An(e) {
  return e[0] !== "$" && !Pt(e);
}
const on = (e) => e === "_" || e === "_ctx" || e === "$stable", ln = (e) => N(e) ? e.map(Ie) : [Ie(e)], $o = (e, t, s) => {
  if (t._n)
    return t;
  const n = qr((...i) => ln(t(...i)), s);
  return n._c = !1, n;
}, Ki = (e, t, s) => {
  const n = e._ctx;
  for (const i in e) {
    if (on(i)) continue;
    const r = e[i];
    if (F(r))
      t[i] = $o(i, r, n);
    else if (r != null) {
      const o = ln(r);
      t[i] = () => o;
    }
  }
}, Ui = (e, t) => {
  const s = ln(t);
  e.slots.default = () => s;
}, Wi = (e, t, s) => {
  for (const n in t)
    (s || !on(n)) && (e[n] = t[n]);
}, Do = (e, t, s) => {
  const n = e.slots = ji();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Wi(n, t, s), s && Zn(n, "_", i, !0)) : Ki(t, n);
  } else t && Ui(e, t);
}, jo = (e, t, s) => {
  const { vnode: n, slots: i } = e;
  let r = !0, o = q;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? r = !1 : Wi(i, t, s) : (r = !t.$stable, Ki(t, i)), o = t;
  } else t && (Ui(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !on(l) && o[l] == null && delete i[l];
}, he = Uo;
function Ho(e) {
  return Lo(e);
}
function Lo(e, t) {
  const s = gs();
  s.__VUE__ = !0;
  const {
    insert: n,
    remove: i,
    patchProp: r,
    createElement: o,
    createText: l,
    createComment: f,
    setText: d,
    setElementText: a,
    parentNode: p,
    nextSibling: w,
    setScopeId: E = Fe,
    insertStaticContent: L
  } = e, M = (c, u, h, b = null, _ = null, g = null, x = void 0, y = null, v = !!u.dynamicChildren) => {
    if (c === u)
      return;
    c && !Ct(c, u) && (b = Gt(c), Te(c, _, g, !0), c = null), u.patchFlag === -2 && (v = !1, u.dynamicChildren = null);
    const { type: m, ref: A, shapeFlag: C } = u;
    switch (m) {
      case ys:
        V(c, u, h, b);
        break;
      case Xe:
        G(c, u, h, b);
        break;
      case Is:
        c == null && $(u, h, b, x);
        break;
      case pe:
        ct(
          c,
          u,
          h,
          b,
          _,
          g,
          x,
          y,
          v
        );
        break;
      default:
        C & 1 ? X(
          c,
          u,
          h,
          b,
          _,
          g,
          x,
          y,
          v
        ) : C & 6 ? ft(
          c,
          u,
          h,
          b,
          _,
          g,
          x,
          y,
          v
        ) : (C & 64 || C & 128) && m.process(
          c,
          u,
          h,
          b,
          _,
          g,
          x,
          y,
          v,
          xt
        );
    }
    A != null && _ ? It(A, c && c.ref, g, u || c, !u) : A == null && c && c.ref != null && It(c.ref, null, g, c, !0);
  }, V = (c, u, h, b) => {
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
  }, G = (c, u, h, b) => {
    c == null ? n(
      u.el = f(u.children || ""),
      h,
      b
    ) : u.el = c.el;
  }, $ = (c, u, h, b) => {
    [c.el, c.anchor] = L(
      c.children,
      u,
      h,
      b,
      c.el,
      c.anchor
    );
  }, W = ({ el: c, anchor: u }, h, b) => {
    let _;
    for (; c && c !== u; )
      _ = w(c), n(c, h, b), c = _;
    n(u, h, b);
  }, P = ({ el: c, anchor: u }) => {
    let h;
    for (; c && c !== u; )
      h = w(c), i(c), c = h;
    i(u);
  }, X = (c, u, h, b, _, g, x, y, v) => {
    if (u.type === "svg" ? x = "svg" : u.type === "math" && (x = "mathml"), c == null)
      ge(
        u,
        h,
        b,
        _,
        g,
        x,
        y,
        v
      );
    else {
      const m = c.el && c.el._isVueCE ? c.el : null;
      try {
        m && m._beginPatch(), lt(
          c,
          u,
          _,
          g,
          x,
          y,
          v
        );
      } finally {
        m && m._endPatch();
      }
    }
  }, ge = (c, u, h, b, _, g, x, y) => {
    let v, m;
    const { props: A, shapeFlag: C, transition: O, dirs: I } = c;
    if (v = c.el = o(
      c.type,
      g,
      A && A.is,
      A
    ), C & 8 ? a(v, c.children) : C & 16 && Ce(
      c.children,
      v,
      null,
      b,
      _,
      Ms(c, g),
      x,
      y
    ), I && Qe(c, null, b, "created"), ae(v, c, c.scopeId, x, b), A) {
      for (const J in A)
        J !== "value" && !Pt(J) && r(v, J, null, A[J], g, b);
      "value" in A && r(v, "value", null, A.value, g), (m = A.onVnodeBeforeMount) && Pe(m, b, c);
    }
    I && Qe(c, null, b, "beforeMount");
    const H = Vo(_, O);
    H && O.beforeEnter(v), n(v, u, h), ((m = A && A.onVnodeMounted) || H || I) && he(() => {
      try {
        m && Pe(m, b, c), H && O.enter(v), I && Qe(c, null, b, "mounted");
      } finally {
      }
    }, _);
  }, ae = (c, u, h, b, _) => {
    if (h && E(c, h), b)
      for (let g = 0; g < b.length; g++)
        E(c, b[g]);
    if (_) {
      let g = _.subTree;
      if (u === g || Ji(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const x = _.vnode;
        ae(
          c,
          x,
          x.scopeId,
          x.slotScopeIds,
          _.parent
        );
      }
    }
  }, Ce = (c, u, h, b, _, g, x, y, v = 0) => {
    for (let m = v; m < c.length; m++) {
      const A = c[m] = y ? Le(c[m]) : Ie(c[m]);
      M(
        null,
        A,
        u,
        h,
        b,
        _,
        g,
        x,
        y
      );
    }
  }, lt = (c, u, h, b, _, g, x) => {
    const y = u.el = c.el;
    let { patchFlag: v, dynamicChildren: m, dirs: A } = u;
    v |= c.patchFlag & 16;
    const C = c.props || q, O = u.props || q;
    let I;
    if (h && et(h, !1), (I = O.onVnodeBeforeUpdate) && Pe(I, h, u, c), A && Qe(u, c, h, "beforeUpdate"), h && et(h, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    m && (!c.dynamicChildren || c.dynamicChildren.length !== m.length) && (v = 0, x = !1, m = null), (C.innerHTML && O.innerHTML == null || C.textContent && O.textContent == null) && a(y, ""), m ? De(
      c.dynamicChildren,
      m,
      y,
      h,
      b,
      Ms(u, _),
      g
    ) : x || T(
      c,
      u,
      y,
      null,
      h,
      b,
      Ms(u, _),
      g,
      !1
    ), v > 0) {
      if (v & 16)
        Je(y, C, O, h, _);
      else if (v & 2 && C.class !== O.class && r(y, "class", null, O.class, _), v & 4 && r(y, "style", C.style, O.style, _), v & 8) {
        const H = u.dynamicProps;
        for (let J = 0; J < H.length; J++) {
          const B = H[J], Z = C[B], ee = O[B];
          (ee !== Z || B === "value") && r(y, B, Z, ee, _, h);
        }
      }
      v & 1 && c.children !== u.children && a(y, u.children);
    } else !x && m == null && Je(y, C, O, h, _);
    ((I = O.onVnodeUpdated) || A) && he(() => {
      I && Pe(I, h, u, c), A && Qe(u, c, h, "updated");
    }, b);
  }, De = (c, u, h, b, _, g, x) => {
    for (let y = 0; y < u.length; y++) {
      const v = c[y], m = u[y], A = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        v.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (v.type === pe || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !Ct(v, m) || // - In the case of a component, it could contain anything.
        v.shapeFlag & 198) ? p(v.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          h
        )
      );
      M(
        v,
        m,
        A,
        null,
        b,
        _,
        g,
        x,
        !0
      );
    }
  }, Je = (c, u, h, b, _) => {
    if (u !== h) {
      if (u !== q)
        for (const g in u)
          !Pt(g) && !(g in h) && r(
            c,
            g,
            u[g],
            null,
            _,
            b
          );
      for (const g in h) {
        if (Pt(g)) continue;
        const x = h[g], y = u[g];
        x !== y && g !== "value" && r(c, g, y, x, _, b);
      }
      "value" in h && r(c, "value", u.value, h.value, _);
    }
  }, ct = (c, u, h, b, _, g, x, y, v) => {
    const m = u.el = c ? c.el : l(""), A = u.anchor = c ? c.anchor : l("");
    let { patchFlag: C, dynamicChildren: O, slotScopeIds: I } = u;
    I && (y = y ? y.concat(I) : I), c == null ? (n(m, h, b), n(A, h, b), Ce(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      h,
      A,
      _,
      g,
      x,
      y,
      v
    )) : C > 0 && C & 64 && O && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    c.dynamicChildren && c.dynamicChildren.length === O.length ? (De(
      c.dynamicChildren,
      O,
      h,
      _,
      g,
      x,
      y
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || _ && u === _.subTree) && Bi(
      c,
      u,
      !0
      /* shallow */
    )) : T(
      c,
      u,
      h,
      A,
      _,
      g,
      x,
      y,
      v
    );
  }, ft = (c, u, h, b, _, g, x, y, v) => {
    u.slotScopeIds = y, c == null ? u.shapeFlag & 512 ? _.ctx.activate(
      u,
      h,
      b,
      x,
      v
    ) : qe(
      u,
      h,
      b,
      _,
      g,
      x,
      v
    ) : R(c, u, v);
  }, qe = (c, u, h, b, _, g, x) => {
    const y = c.component = qo(
      c,
      b,
      _
    );
    if (Oi(c) && (y.ctx.renderer = xt), Yo(y, !1, x), y.asyncDep) {
      if (_ && _.registerDep(y, S, x), !c.el) {
        const v = y.subTree = Ue(Xe);
        G(null, v, u, h), c.placeholder = v.el;
      }
    } else
      S(
        y,
        c,
        u,
        h,
        _,
        g,
        x
      );
  }, R = (c, u, h) => {
    const b = u.component = c.component;
    if (Ro(c, u, h))
      if (b.asyncDep && !b.asyncResolved) {
        D(b, u, h);
        return;
      } else
        b.next = u, b.update();
    else
      u.el = c.el, b.vnode = u;
  }, S = (c, u, h, b, _, g, x) => {
    const y = () => {
      if (c.isMounted) {
        let { next: C, bu: O, u: I, parent: H, vnode: J } = c;
        {
          const Oe = ki(c);
          if (Oe) {
            C && (C.el = J.el, D(c, C, x)), Oe.asyncDep.then(() => {
              he(() => {
                c.isUnmounted || m();
              }, _);
            });
            return;
          }
        }
        let B = C, Z;
        et(c, !1), C ? (C.el = J.el, D(c, C, x)) : C = J, O && Yt(O), (Z = C.props && C.props.onVnodeBeforeUpdate) && Pe(Z, H, C, J), et(c, !0);
        const ee = En(c), Ee = c.subTree;
        c.subTree = ee, M(
          Ee,
          ee,
          // parent may have changed if it's in a teleport
          p(Ee.el),
          // anchor may have changed if it's in a fragment
          Gt(Ee),
          c,
          _,
          g
        ), C.el = ee.el, B === null && Mo(c, ee.el), I && he(I, _), (Z = C.props && C.props.onVnodeUpdated) && he(
          () => Pe(Z, H, C, J),
          _
        );
      } else {
        let C;
        const { el: O, props: I } = u, { bm: H, m: J, parent: B, root: Z, type: ee } = c, Ee = Nt(u);
        et(c, !1), H && Yt(H), !Ee && (C = I && I.onVnodeBeforeMount) && Pe(C, B, u), et(c, !0);
        {
          Z.ce && Z.ce._hasShadowRoot() && Z.ce._injectChildStyle(
            ee,
            c.parent ? c.parent.type : void 0
          );
          const Oe = c.subTree = En(c);
          M(
            null,
            Oe,
            h,
            b,
            c,
            _,
            g
          ), u.el = Oe.el;
        }
        if (J && he(J, _), !Ee && (C = I && I.onVnodeMounted)) {
          const Oe = u;
          he(
            () => Pe(C, B, Oe),
            _
          );
        }
        (u.shapeFlag & 256 || B && Nt(B.vnode) && B.vnode.shapeFlag & 256) && c.a && he(c.a, _), c.isMounted = !0, u = h = b = null;
      }
    };
    c.scope.on();
    const v = c.effect = new si(y);
    c.scope.off();
    const m = c.update = v.run.bind(v), A = c.job = v.runIfDirty.bind(v);
    A.i = c, A.id = c.uid, v.scheduler = () => nn(A), et(c, !0), m();
  }, D = (c, u, h) => {
    u.component = c;
    const b = c.vnode.props;
    c.vnode = u, c.next = null, No(c, u.props, b, h), jo(c, u.children, h), We(), bn(c), Be();
  }, T = (c, u, h, b, _, g, x, y, v = !1) => {
    const m = c && c.children, A = c ? c.shapeFlag : 0, C = u.children, { patchFlag: O, shapeFlag: I } = u;
    if (O > 0) {
      if (O & 128) {
        ze(
          m,
          C,
          h,
          b,
          _,
          g,
          x,
          y,
          v
        );
        return;
      } else if (O & 256) {
        de(
          m,
          C,
          h,
          b,
          _,
          g,
          x,
          y,
          v
        );
        return;
      }
    }
    I & 8 ? (A & 16 && yt(m, _, g), C !== m && a(h, C)) : A & 16 ? I & 16 ? ze(
      m,
      C,
      h,
      b,
      _,
      g,
      x,
      y,
      v
    ) : yt(m, _, g, !0) : (A & 8 && a(h, ""), I & 16 && Ce(
      C,
      h,
      b,
      _,
      g,
      x,
      y,
      v
    ));
  }, de = (c, u, h, b, _, g, x, y, v) => {
    c = c || ht, u = u || ht;
    const m = c.length, A = u.length, C = Math.min(m, A);
    let O;
    for (O = 0; O < C; O++) {
      const I = u[O] = v ? Le(u[O]) : Ie(u[O]);
      M(
        c[O],
        I,
        h,
        null,
        _,
        g,
        x,
        y,
        v
      );
    }
    m > A ? yt(
      c,
      _,
      g,
      !0,
      !1,
      C
    ) : Ce(
      u,
      h,
      b,
      _,
      g,
      x,
      y,
      v,
      C
    );
  }, ze = (c, u, h, b, _, g, x, y, v) => {
    let m = 0;
    const A = u.length;
    let C = c.length - 1, O = A - 1;
    for (; m <= C && m <= O; ) {
      const I = c[m], H = u[m] = v ? Le(u[m]) : Ie(u[m]);
      if (Ct(I, H))
        M(
          I,
          H,
          h,
          null,
          _,
          g,
          x,
          y,
          v
        );
      else
        break;
      m++;
    }
    for (; m <= C && m <= O; ) {
      const I = c[C], H = u[O] = v ? Le(u[O]) : Ie(u[O]);
      if (Ct(I, H))
        M(
          I,
          H,
          h,
          null,
          _,
          g,
          x,
          y,
          v
        );
      else
        break;
      C--, O--;
    }
    if (m > C) {
      if (m <= O) {
        const I = O + 1, H = I < A ? u[I].el : b;
        for (; m <= O; )
          M(
            null,
            u[m] = v ? Le(u[m]) : Ie(u[m]),
            h,
            H,
            _,
            g,
            x,
            y,
            v
          ), m++;
      }
    } else if (m > O)
      for (; m <= C; )
        Te(c[m], _, g, !0), m++;
    else {
      const I = m, H = m, J = /* @__PURE__ */ new Map();
      for (m = H; m <= O; m++) {
        const me = u[m] = v ? Le(u[m]) : Ie(u[m]);
        me.key != null && J.set(me.key, m);
      }
      let B, Z = 0;
      const ee = O - H + 1;
      let Ee = !1, Oe = 0;
      const wt = new Array(ee);
      for (m = 0; m < ee; m++) wt[m] = 0;
      for (m = I; m <= C; m++) {
        const me = c[m];
        if (Z >= ee) {
          Te(me, _, g, !0);
          continue;
        }
        let Ae;
        if (me.key != null)
          Ae = J.get(me.key);
        else
          for (B = H; B <= O; B++)
            if (wt[B - H] === 0 && Ct(me, u[B])) {
              Ae = B;
              break;
            }
        Ae === void 0 ? Te(me, _, g, !0) : (wt[Ae - H] = m + 1, Ae >= Oe ? Oe = Ae : Ee = !0, M(
          me,
          u[Ae],
          h,
          null,
          _,
          g,
          x,
          y,
          v
        ), Z++);
      }
      const an = Ee ? Ko(wt) : ht;
      for (B = an.length - 1, m = ee - 1; m >= 0; m--) {
        const me = H + m, Ae = u[me], dn = u[me + 1], hn = me + 1 < A ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          dn.el || Gi(dn)
        ) : b;
        wt[m] === 0 ? M(
          null,
          Ae,
          h,
          hn,
          _,
          g,
          x,
          y,
          v
        ) : Ee && (B < 0 || m !== an[B] ? Ze(Ae, h, hn, 2) : B--);
      }
    }
  }, Ze = (c, u, h, b, _ = null) => {
    const { el: g, type: x, transition: y, children: v, shapeFlag: m } = c;
    if (m & 6) {
      Ze(c.component.subTree, u, h, b);
      return;
    }
    if (m & 128) {
      c.suspense.move(u, h, b);
      return;
    }
    if (m & 64) {
      x.move(c, u, h, xt);
      return;
    }
    if (x === pe) {
      n(g, u, h);
      for (let C = 0; C < v.length; C++)
        Ze(v[C], u, h, b);
      n(c.anchor, u, h);
      return;
    }
    if (x === Is) {
      W(c, u, h);
      return;
    }
    if (b !== 2 && m & 1 && y)
      if (b === 0)
        y.persisted && !g[Ps] ? n(g, u, h) : (y.beforeEnter(g), n(g, u, h), he(() => y.enter(g), _));
      else {
        const { leave: C, delayLeave: O, afterLeave: I } = y, H = () => {
          c.ctx.isUnmounted ? i(g) : n(g, u, h);
        }, J = () => {
          const B = g._isLeaving || !!g[Ps];
          g._isLeaving && g[Ps](
            !0
            /* cancelled */
          ), y.persisted && !B ? H() : C(g, () => {
            H(), I && I();
          });
        };
        O ? O(g, H, J) : J();
      }
    else
      n(g, u, h);
  }, Te = (c, u, h, b = !1, _ = !1) => {
    const {
      type: g,
      props: x,
      ref: y,
      children: v,
      dynamicChildren: m,
      shapeFlag: A,
      patchFlag: C,
      dirs: O,
      cacheIndex: I,
      memo: H
    } = c;
    if (C === -2 && (_ = !1), y != null && (We(), It(y, null, h, c, !0), Be()), I != null && (u.renderCache[I] = void 0), A & 256) {
      u.ctx.deactivate(c);
      return;
    }
    const J = A & 1 && O, B = !Nt(c);
    let Z;
    if (B && (Z = x && x.onVnodeBeforeUnmount) && Pe(Z, u, c), A & 6)
      nr(c.component, h, b);
    else {
      if (A & 128) {
        c.suspense.unmount(h, b);
        return;
      }
      J && Qe(c, null, u, "beforeUnmount"), A & 64 ? c.type.remove(
        c,
        u,
        h,
        xt,
        b
      ) : m && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !m.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== pe || C > 0 && C & 64) ? yt(
        m,
        u,
        h,
        !1,
        !0
      ) : (g === pe && C & 384 || !_ && A & 16) && yt(v, u, h), b && fn(c);
    }
    const ee = H != null && I == null;
    (B && (Z = x && x.onVnodeUnmounted) || J || ee) && he(() => {
      Z && Pe(Z, u, c), J && Qe(c, null, u, "unmounted"), ee && (c.el = null);
    }, h);
  }, fn = (c) => {
    const { type: u, el: h, anchor: b, transition: _ } = c;
    if (u === pe) {
      sr(h, b);
      return;
    }
    if (u === Is) {
      P(c);
      return;
    }
    const g = () => {
      i(h), _ && !_.persisted && _.afterLeave && _.afterLeave();
    };
    if (c.shapeFlag & 1 && _ && !_.persisted) {
      const { leave: x, delayLeave: y } = _, v = () => x(h, g);
      y ? y(c.el, g, v) : v();
    } else
      g();
  }, sr = (c, u) => {
    let h;
    for (; c !== u; )
      h = w(c), i(c), c = h;
    i(u);
  }, nr = (c, u, h) => {
    const { bum: b, scope: _, job: g, subTree: x, um: y, m: v, a: m } = c;
    Pn(v), Pn(m), b && Yt(b), _.stop(), g && (g.flags |= 8, Te(x, c, u, h)), y && he(y, u), he(() => {
      c.isUnmounted = !0;
    }, u);
  }, yt = (c, u, h, b = !1, _ = !1, g = 0) => {
    for (let x = g; x < c.length; x++)
      Te(c[x], u, h, b, _);
  }, Gt = (c) => {
    if (c.shapeFlag & 6)
      return Gt(c.component.subTree);
    if (c.shapeFlag & 128)
      return c.suspense.next();
    const u = w(c.anchor || c.el), h = u && u[Qr];
    return h ? w(h) : u;
  };
  let ws = !1;
  const un = (c, u, h) => {
    let b;
    c == null ? u._vnode && (Te(u._vnode, null, null, !0), b = u._vnode.component) : M(
      u._vnode || null,
      c,
      u,
      null,
      null,
      null,
      h
    ), u._vnode = c, ws || (ws = !0, bn(b), yi(), ws = !1);
  }, xt = {
    p: M,
    um: Te,
    m: Ze,
    r: fn,
    mt: qe,
    mc: Ce,
    pc: T,
    pbc: De,
    n: Gt,
    o: e
  };
  return {
    render: un,
    hydrate: void 0,
    createApp: Co(un)
  };
}
function Ms({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function et({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Vo(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Bi(e, t, s = !1) {
  const n = e.children, i = t.children;
  if (N(n) && N(i))
    for (let r = 0; r < n.length; r++) {
      const o = n[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = Le(i[r]), l.el = o.el), !s && l.patchFlag !== -2 && Bi(o, l)), l.type === ys && (l.patchFlag === -1 && (l = i[r] = Le(l)), l.el = o.el), l.type === Xe && !l.el && (l.el = o.el);
    }
}
function Ko(e) {
  const t = e.slice(), s = [0];
  let n, i, r, o, l;
  const f = e.length;
  for (n = 0; n < f; n++) {
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
function ki(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : ki(t);
}
function Pn(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function Gi(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? Gi(t.subTree) : null;
}
const Ji = (e) => e.__isSuspense;
function Uo(e, t) {
  t && t.pendingBranch ? N(e) ? t.effects.push(...e) : t.effects.push(e) : Jr(e);
}
const pe = /* @__PURE__ */ Symbol.for("v-fgt"), ys = /* @__PURE__ */ Symbol.for("v-txt"), Xe = /* @__PURE__ */ Symbol.for("v-cmt"), Is = /* @__PURE__ */ Symbol.for("v-stc"), rt = [];
let ve = null;
function Q(e = !1) {
  rt.push(ve = e ? null : []);
}
function qi() {
  rt.pop(), ve = rt[rt.length - 1] || null;
}
let Lt = 1;
function Rn(e, t = !1) {
  Lt += e, e < 0 && ve && t && (ve.hasOnce = !0);
}
function zi(e) {
  return e.dynamicChildren = Lt > 0 ? ve || ht : null, qi(), Lt > 0 && ve && ve.push(e), e;
}
function ne(e, t, s, n, i, r) {
  return zi(
    j(
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
function cn(e, t, s, n, i) {
  return zi(
    Ue(
      e,
      t,
      s,
      n,
      i,
      !0
    )
  );
}
function Yi(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function Ct(e, t) {
  return e.type === t.type && e.key === t.key;
}
const Xi = ({ key: e }) => e ?? null, Qt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? Y(e) || /* @__PURE__ */ le(e) || F(e) ? { i: be, r: e, k: t, f: !!s } : e : null);
function j(e, t = null, s = null, n = 0, i = null, r = e === pe ? 0 : 1, o = !1, l = !1) {
  const f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Xi(t),
    ref: t && Qt(t),
    scopeId: wi,
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
    ctx: be
  };
  return l ? (rs(f, s), r & 128 && e.normalize(f)) : s && (f.shapeFlag |= Y(s) ? 8 : 16), Lt > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  ve && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (f.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  f.patchFlag !== 32 && ve.push(f), f;
}
const Ue = Wo;
function Wo(e, t = null, s = null, n = 0, i = null, r = !1) {
  if ((!e || e === go) && (e = Xe), Yi(e)) {
    const l = vt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && rs(l, s), Lt > 0 && !r && ve && (l.shapeFlag & 6 ? ve[ve.indexOf(e)] = l : ve.push(l)), l.patchFlag = -2, l;
  }
  if (tl(e) && (e = e.__vccOpts), t) {
    t = Bo(t);
    let { class: l, style: f } = t;
    l && !Y(l) && (t.class = Ut(l)), k(f) && (/* @__PURE__ */ sn(f) && !N(f) && (f = ie({}, f)), t.style = zs(f));
  }
  const o = Y(e) ? 1 : Ji(e) ? 128 : eo(e) ? 64 : k(e) ? 4 : F(e) ? 2 : 0;
  return j(
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
function Bo(e) {
  return e ? /* @__PURE__ */ sn(e) || Hi(e) ? ie({}, e) : e : null;
}
function vt(e, t, s = !1, n = !1) {
  const { props: i, ref: r, patchFlag: o, children: l, transition: f } = e, d = t ? ko(i || {}, t) : i, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && Xi(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && r ? N(r) ? r.concat(Qt(t)) : [r, Qt(t)] : Qt(t)
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
    transition: f,
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
  return f && n && rn(
    a,
    f.clone(a)
  ), a;
}
function dt(e = " ", t = 0) {
  return Ue(ys, null, e, t);
}
function Ot(e = "", t = !1) {
  return t ? (Q(), cn(Xe, null, e)) : Ue(Xe, null, e);
}
function Ie(e) {
  return e == null || typeof e == "boolean" ? Ue(Xe) : N(e) ? Ue(
    pe,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Yi(e) ? Le(e) : Ue(ys, null, String(e));
}
function Le(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : vt(e);
}
function rs(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (N(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), rs(e, i()), i._c && (i._d = !0));
      return;
    } else {
      s = 32;
      const i = t._;
      !i && !Hi(t) ? t._ctx = be : i === 3 && be && (be.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (F(t)) {
    if (n & 65) {
      rs(e, { default: t });
      return;
    }
    t = { default: t, _ctx: be }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [dt(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function ko(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const i in n)
      if (i === "class")
        t.class !== n.class && (t.class = Ut([t.class, n.class]));
      else if (i === "style")
        t.style = zs([t.style, n.style]);
      else if (fs(i)) {
        const r = t[i], o = n[i];
        o && r !== o && !(N(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !us(i) && (t[i] = o);
      } else i !== "" && (t[i] = n[i]);
  }
  return t;
}
function Pe(e, t, s, n = null) {
  Se(e, t, 7, [
    s,
    n
  ]);
}
const Go = Ni();
let Jo = 0;
function qo(e, t, s) {
  const n = e.type, i = (t ? t.appContext : e.appContext) || Go, r = {
    uid: Jo++,
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
    scope: new mr(
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
    propsOptions: Vi(n, i),
    emitsOptions: Fi(n, i),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: q,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: q,
    data: q,
    props: q,
    attrs: q,
    slots: q,
    refs: q,
    setupState: q,
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
  return r.ctx = { _: r }, r.root = t ? t.root : r, r.emit = Eo.bind(null, r), e.ce && e.ce(r), r;
}
let oe = null;
const zo = () => oe || be;
let os, Bs;
{
  const e = gs(), t = (s, n) => {
    let i;
    return (i = e[s]) || (i = e[s] = []), i.push(n), (r) => {
      i.length > 1 ? i.forEach((o) => o(r)) : i[0](r);
    };
  };
  os = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => oe = s
  ), Bs = t(
    "__VUE_SSR_SETTERS__",
    (s) => Vt = s
  );
}
const kt = (e) => {
  const t = oe;
  return os(e), e.scope.on(), () => {
    e.scope.off(), os(t);
  };
}, Mn = () => {
  oe && oe.scope.off(), os(null);
};
function Zi(e) {
  return e.vnode.shapeFlag & 4;
}
let Vt = !1;
function Yo(e, t = !1, s = !1) {
  t && Bs(t);
  const { props: n, children: i } = e.vnode, r = Zi(e);
  Io(e, n, r, t), Do(e, i, s || t);
  const o = r ? Xo(e, t) : void 0;
  return t && Bs(!1), o;
}
function Xo(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, _o);
  const { setup: n } = s;
  if (n) {
    We();
    const i = e.setupContext = n.length > 1 ? Qo(e) : null, r = kt(e), o = Bt(
      n,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = zn(o);
    if (Be(), r(), (l || e.sp) && !Nt(e) && Ei(e), l) {
      if (o.then(Mn, Mn), t)
        return o.then((f) => {
          In(e, f);
        }).catch((f) => {
          _s(f, e, 0);
        });
      e.asyncDep = o;
    } else
      In(e, o);
  } else
    Qi(e);
}
function In(e, t, s) {
  F(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : k(t) && (e.setupState = mi(t)), Qi(e);
}
function Qi(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Fe);
  {
    const i = kt(e);
    We();
    try {
      bo(e);
    } finally {
      Be(), i();
    }
  }
}
const Zo = {
  get(e, t) {
    return re(e, "get", ""), e[t];
  }
};
function Qo(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, Zo),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function xs(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(mi(Dr(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in Ft)
        return Ft[s](e);
    },
    has(t, s) {
      return s in t || s in Ft;
    }
  })) : e.proxy;
}
function el(e, t = !0) {
  return F(e) ? e.displayName || e.name : e.name || t && e.__name;
}
function tl(e) {
  return F(e) && "__vccOpts" in e;
}
const sl = (e, t) => /* @__PURE__ */ Ur(e, t, Vt), nl = "3.5.40";
/**
* @vue/runtime-dom v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let ks;
const Nn = typeof window < "u" && window.trustedTypes;
if (Nn)
  try {
    ks = /* @__PURE__ */ Nn.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const er = ks ? (e) => ks.createHTML(e) : (e) => e, il = "http://www.w3.org/2000/svg", rl = "http://www.w3.org/1998/Math/MathML", He = typeof document < "u" ? document : null, Fn = He && /* @__PURE__ */ He.createElement("template"), ol = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const i = t === "svg" ? He.createElementNS(il, e) : t === "mathml" ? He.createElementNS(rl, e) : s ? He.createElement(e, { is: s }) : He.createElement(e);
    return e === "select" && n && n.multiple != null && i.setAttribute("multiple", n.multiple), i;
  },
  createText: (e) => He.createTextNode(e),
  createComment: (e) => He.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => He.querySelector(e),
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
      Fn.innerHTML = er(
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
}, ll = /* @__PURE__ */ Symbol("_vtc");
function cl(e, t, s) {
  const n = e[ll];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const $n = /* @__PURE__ */ Symbol("_vod"), fl = /* @__PURE__ */ Symbol("_vsh"), ul = /* @__PURE__ */ Symbol(""), al = /(?:^|;)\s*display\s*:/;
function dl(e, t, s) {
  const n = e.style, i = Y(s);
  let r = !1;
  if (s && !i) {
    if (t)
      if (Y(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && At(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && At(n, o, "");
    for (const o in s) {
      o === "display" && (r = !0);
      const l = s[o];
      l != null ? pl(
        e,
        o,
        !Y(t) && t ? t[o] : void 0,
        l
      ) || At(n, o, l) : At(n, o, "");
    }
  } else if (i) {
    if (t !== s) {
      const o = n[ul];
      o && (s += ";" + o), n.cssText = s, r = al.test(s);
    }
  } else t && e.removeAttribute("style");
  $n in e && (e[$n] = r ? n.display : "", e[fl] && (n.display = "none"));
}
const Dn = /\s*!important$/;
function At(e, t, s) {
  if (N(s))
    s.forEach((n) => At(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = hl(e, t);
    Dn.test(s) ? e.setProperty(
      ot(n),
      s.replace(Dn, ""),
      "important"
    ) : e[n] = s;
  }
}
const jn = ["Webkit", "Moz", "ms"], Ns = {};
function hl(e, t) {
  const s = Ns[t];
  if (s)
    return s;
  let n = ue(t);
  if (n !== "filter" && n in e)
    return Ns[t] = n;
  n = hs(n);
  for (let i = 0; i < jn.length; i++) {
    const r = jn[i] + n;
    if (r in e)
      return Ns[t] = r;
  }
  return t;
}
function pl(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && Y(n) && s === n;
}
const Hn = "http://www.w3.org/1999/xlink";
function Ln(e, t, s, n, i, r = hr(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Hn, t.slice(6, t.length)) : e.setAttributeNS(Hn, t, s) : s == null || r && !Qn(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : $e(s) ? String(s) : s
  );
}
function Vn(e, t, s, n, i) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? er(s) : s);
    return;
  }
  const r = e.tagName;
  if (t === "value" && r !== "PROGRESS" && // custom elements may use _value internally
  !r.includes("-")) {
    const l = r === "OPTION" ? e.getAttribute("value") || "" : e.value, f = s == null ? (
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
    l === "boolean" ? s = Qn(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(i || t);
}
function st(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function gl(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const Kn = /* @__PURE__ */ Symbol("_vei");
function ml(e, t, s, n, i = null) {
  const r = e[Kn] || (e[Kn] = {}), o = r[t];
  if (n && o)
    o.value = n;
  else {
    const [l, f] = vl(t);
    if (n) {
      const d = r[t] = wl(
        n,
        i
      );
      st(e, l, d, f);
    } else o && (gl(e, l, o, f), r[t] = void 0);
  }
}
const _l = /(Once|Passive|Capture)$/, bl = /^on:?(?:Once|Passive|Capture)$/;
function vl(e) {
  let t, s;
  for (; (s = e.match(_l)) && !bl.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : ot(e.slice(2)), t];
}
let Fs = 0;
const yl = /* @__PURE__ */ Promise.resolve(), xl = () => Fs || (yl.then(() => Fs = 0), Fs = Date.now());
function wl(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const i = s.value;
    if (N(i)) {
      const r = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        r.call(n), n._stopped = !0;
      };
      const o = i.slice(), l = [n];
      for (let f = 0; f < o.length && !n._stopped; f++) {
        const d = o[f];
        d && Se(
          d,
          t,
          5,
          l
        );
      }
    } else
      Se(
        i,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = xl(), s;
}
const Un = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, Sl = (e, t, s, n, i, r) => {
  const o = i === "svg";
  t === "class" ? cl(e, n, o) : t === "style" ? dl(e, s, n) : fs(t) ? us(t) || ml(e, t, s, n, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : Cl(e, t, n, o)) ? (Vn(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Ln(e, t, n, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (Tl(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !Y(n))) ? Vn(e, ue(t), n, r, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), Ln(e, t, n, o));
};
function Cl(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Un(t) && F(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return Un(t) && Y(s) ? !1 : t in e;
}
function Tl(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = ue(t);
  return Array.isArray(s) ? s.some((i) => ue(i) === n) : Object.keys(s).some((i) => ue(i) === n);
}
const ls = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return N(t) ? (s) => Yt(t, s) : t;
};
function El(e) {
  e.target.composing = !0;
}
function Wn(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const _t = /* @__PURE__ */ Symbol("_assign");
function Bn(e, t, s) {
  return t && (e = e.trim()), s && (e = ps(e)), e;
}
const kn = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, i) {
    e[_t] = ls(i);
    const r = n || i.props && i.props.type === "number";
    st(e, t ? "change" : "input", (o) => {
      o.target.composing || e[_t](Bn(e.value, s, r));
    }), (s || r) && st(e, "change", () => {
      e.value = Bn(e.value, s, r);
    }), t || (st(e, "compositionstart", El), st(e, "compositionend", Wn), st(e, "change", Wn));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: i, number: r } }, o) {
    if (e[_t] = ls(o), e.composing) return;
    const l = (r || e.type === "number") && !/^0\d/.test(e.value) ? ps(e.value) : e.value, f = t ?? "";
    if (l === f)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (n && t === s || i && e.value.trim() === f) || (e.value = f);
  }
}, Ol = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    e._modelValue = t, st(e, "change", () => {
      const i = Array.prototype.filter.call(e.options, (r) => r.selected).map(
        (r) => s ? ps(cs(r)) : cs(r)
      );
      e[_t](
        e.multiple ? as(e._modelValue) ? new Set(i) : i : i[0]
      ), e._assigning = !0, bi(() => {
        e._assigning = !1;
      });
    }), e[_t] = ls(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    Gn(e, t);
  },
  beforeUpdate(e, { value: t }, s) {
    e._modelValue = t, e[_t] = ls(s);
  },
  updated(e, { value: t }) {
    e._assigning || Gn(e, t);
  }
};
function Gn(e, t) {
  const s = e.multiple, n = N(t);
  if (!(s && !n && !as(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = cs(o);
      if (s)
        if (n) {
          const f = typeof l;
          f === "string" || f === "number" ? o.selected = t.some((d) => String(d) === String(l)) : o.selected = gr(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (Wt(cs(o), t)) {
        e.selectedIndex !== i && (e.selectedIndex = i);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function cs(e) {
  return "_value" in e ? e._value : e.value;
}
const Al = ["ctrl", "shift", "alt", "meta"], Pl = {
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
  exact: (e, t) => Al.some((s) => e[`${s}Key`] && !t.includes(s))
}, Rl = (e, t) => {
  if (!e) return e;
  const s = e._withMods || (e._withMods = {}), n = t.join(".");
  return s[n] || (s[n] = ((i, ...r) => {
    for (let o = 0; o < t.length; o++) {
      const l = Pl[t[o]];
      if (l && l(i, t)) return;
    }
    return e(i, ...r);
  }));
}, Ml = /* @__PURE__ */ ie({ patchProp: Sl }, ol);
let Jn;
function Il() {
  return Jn || (Jn = Ho(Ml));
}
const Nl = ((...e) => {
  const t = Il().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const i = $l(n);
    if (!i) return;
    const r = t._component;
    !F(r) && !r.render && !r.template && (r.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
    const o = s(i, !1, Fl(i));
    return i instanceof Element && (i.removeAttribute("v-cloak"), i.setAttribute("data-v-app", "")), o;
  }, t;
});
function Fl(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function $l(e) {
  return Y(e) ? document.querySelector(e) : e;
}
const Dl = { class: "node" }, jl = {
  key: 1,
  class: "chev"
}, Hl = { class: "kind" }, Ll = {
  key: 0,
  class: "children"
}, Vl = /* @__PURE__ */ Ti({
  __name: "TreeNode",
  props: {
    node: {},
    selected: {}
  },
  emits: ["select"],
  setup(e, { emit: t }) {
    const s = e, n = t, i = /* @__PURE__ */ _e(!1);
    function r() {
      n("select", s.node);
    }
    return (o, l) => {
      const f = po("TreeNode", !0);
      return Q(), ne("div", Dl, [
        j("div", {
          class: Ut(["node-head", { sel: e.selected === e.node.fullName }]),
          onClick: r
        }, [
          e.node.children.length ? (Q(), ne("span", {
            key: 0,
            class: "chev",
            onClick: l[0] || (l[0] = Rl((d) => i.value = !i.value, ["stop"]))
          }, te(i.value ? "▾" : "▸"), 1)) : (Q(), ne("span", jl, "·")),
          dt(" " + te(e.node.name) + " ", 1),
          j("span", Hl, "[" + te(e.node.kind) + "]", 1)
        ], 2),
        i.value && e.node.children.length ? (Q(), ne("div", Ll, [
          (Q(!0), ne(pe, null, Tt(e.node.children, (d) => (Q(), cn(f, {
            key: d.fullName,
            node: d,
            selected: e.selected,
            onSelect: l[1] || (l[1] = (a) => o.$emit("select", a))
          }, null, 8, ["node", "selected"]))), 128))
        ])) : Ot("", !0)
      ]);
    };
  }
}), tr = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, i] of t)
    s[n] = i;
  return s;
}, Kl = /* @__PURE__ */ tr(Vl, [["__scopeId", "data-v-c476374e"]]), Ul = { class: "onec" }, Wl = { class: "col-left" }, Bl = { class: "card" }, kl = {
  key: 0,
  class: "search-results"
}, Gl = ["onClick"], Jl = { class: "tree" }, ql = ["onClick"], zl = { class: "chev" }, Yl = {
  key: 0,
  class: "tree-children"
}, Xl = { class: "card counters" }, Zl = { class: "card export" }, Ql = { class: "row" }, ec = ["value"], tc = ["disabled"], sc = { class: "row" }, nc = ["disabled"], ic = {
  key: 0,
  class: "muted status"
}, rc = { class: "col-right card" }, oc = { class: "obj-title" }, lc = { class: "row" }, cc = ["disabled"], fc = ["disabled"], uc = ["disabled"], ac = { class: "muted graph-summary" }, dc = {
  key: 0,
  class: "obj-details"
}, hc = /* @__PURE__ */ Ti({
  __name: "BrowserPanel",
  props: {
    api: {}
  },
  setup(e) {
    const t = e, s = /* @__PURE__ */ jt({ objectCount: 0, relationCount: 0, sectionCount: 0 }), n = /* @__PURE__ */ _e([]), i = /* @__PURE__ */ jt({}), r = /* @__PURE__ */ _e(""), o = /* @__PURE__ */ _e([]), l = /* @__PURE__ */ _e(null), f = /* @__PURE__ */ _e([]), d = /* @__PURE__ */ _e("full-name"), a = /* @__PURE__ */ _e(""), p = /* @__PURE__ */ _e(!1), w = /* @__PURE__ */ _e(""), E = /* @__PURE__ */ _e(""), L = /* @__PURE__ */ _e(null);
    let M = "", V = null;
    const G = /* @__PURE__ */ _e("");
    async function $(R, S = {}) {
      const D = await t.api.invoke("plugin.action", {
        pluginId: "onec",
        action: R,
        valueJson: JSON.stringify(S)
      });
      if (!D.ok) throw new Error(D.error || "action failed");
      return D.resultJson ? JSON.parse(D.resultJson) : null;
    }
    Pi(async () => {
      await W();
      try {
        const R = await $("formatters");
        f.value = R.formatters;
      } catch {
      }
    });
    async function W() {
      try {
        const R = await $("overview");
        s.objectCount = R.objectCount, s.relationCount = R.relationCount, s.sectionCount = R.sectionCount, n.value = R.tree;
        for (const S of R.tree) S.kind in i || (i[S.kind] = !1);
      } catch (R) {
        w.value = "Не удалось открыть индекс 1С: " + qe(R);
      }
    }
    let P;
    Zt(r, (R) => {
      clearTimeout(P), P = window.setTimeout(async () => {
        if (!R.trim()) {
          o.value = [];
          return;
        }
        try {
          const S = await $("search", { query: R });
          o.value = S.results;
        } catch {
          o.value = [];
        }
      }, 150);
    });
    function X(R) {
      i[R] = !i[R];
    }
    function ge(R) {
      $("object", { fullName: R.fullName }).then((S) => {
        l.value = S, E.value = "Выберите Dependencies, References или Data Flow.", G.value = S ? `kind: ${S.kind}
name: ${S.name}
full_name: ${S.fullName}
path: ${S.path}
summary: ${S.summary}` : "", M = "", V == null || V.elements().remove();
      });
    }
    async function ae(R) {
      if (l.value) {
        M = R;
        try {
          const S = await $("graph", { fullName: l.value.fullName, mode: R, depth: 3, limit: 400 });
          E.value = `mode: ${S.mode} · nodes: ${S.nodeCount} · edges: ${S.edgeCount} · depth: ${S.depth} · truncated: ${S.truncated}`, await ft(S.elements);
        } catch (S) {
          E.value = "Не удалось построить граф: " + qe(S);
        }
      }
    }
    async function Ce() {
      if (l.value)
        try {
          const R = await $("format", {
            formatterId: d.value,
            fullName: l.value.fullName,
            mode: M,
            depth: 3,
            limit: 400
          });
          await navigator.clipboard.writeText(R.text), w.value = "Скопировано в буфер обмена.";
        } catch (R) {
          w.value = "Копирование не удалось: " + qe(R);
        }
    }
    async function lt() {
      if (!a.value.trim()) {
        w.value = "Укажите путь к дампу конфигурации.";
        return;
      }
      p.value = !0, w.value = "Индексация запущена…";
      try {
        const R = await $("rebuild", { path: a.value.trim() });
        w.value = `Готово. Объектов: +${R.objectsAdded} ~${R.objectsUpdated}, связей: +${R.relationsAdded}, пропущено: ${R.filesSkipped}, ошибок: ${R.filesWithErrors}, за ${R.elapsedSeconds}с`, await W();
      } catch (R) {
        w.value = "Индексация не удалась: " + qe(R);
      } finally {
        p.value = !1;
      }
    }
    const De = {
      Document: "#89b4fa",
      Catalog: "#a6e3a1",
      AccumulationRegister: "#f38ba8",
      InformationRegister: "#fab387",
      AccountingRegister: "#f38ba8",
      CalculationRegister: "#f38ba8",
      CommonModule: "#cba6f7",
      Report: "#f9e2af",
      Processing: "#f9e2af",
      Form: "#94e2d5",
      TabularSection: "#94e2d5"
    }, Je = {
      writes: "#f38ba8",
      reads: "#89b4fa",
      queries: "#fab387",
      calls: "#a6e3a1",
      owns: "#6c7086",
      uses: "#cba6f7"
    };
    async function ct() {
      return window.cytoscape || await new Promise((R, S) => {
        const D = document.createElement("script");
        D.src = "https://cdnjs.cloudflare.com/ajax/libs/cytoscape/3.30.2/cytoscape.min.js", D.onload = () => R(), D.onerror = () => S(new Error("cytoscape CDN load failed")), document.head.appendChild(D);
      }), window.cytoscape;
    }
    async function ft(R) {
      const S = await ct();
      if (!L.value) return;
      const D = [
        ...R.nodes.map((T) => ({ data: T })),
        ...R.edges.map((T) => ({ data: T }))
      ];
      V && V.destroy(), V = S({
        container: L.value,
        elements: D,
        wheelSensitivity: 0.25,
        style: [
          { selector: "node", style: {
            "background-color": (T) => De[T.data("kind")] || "#6c7086",
            label: "data(label)",
            color: "#cdd6f4",
            "font-size": "10px",
            "text-valign": "bottom",
            "text-margin-y": 4,
            "text-outline-width": 2,
            "text-outline-color": "#1e1e2e",
            width: 34,
            height: 34
          } },
          { selector: "node[?isCenter]", style: {
            width: 52,
            height: 52,
            "border-width": 3,
            "border-color": "#cdd6f4",
            "font-size": "12px",
            "font-weight": "bold"
          } },
          { selector: "edge", style: {
            "line-color": (T) => Je[T.data("type")] || "#6c7086",
            "target-arrow-color": (T) => Je[T.data("type")] || "#6c7086",
            "target-arrow-shape": "triangle",
            "curve-style": "bezier",
            width: 1.6,
            opacity: 0.85,
            label: "data(type)",
            "font-size": 8,
            color: "#6c7086",
            "text-rotation": "autorotate"
          } }
        ],
        layout: { name: "cose", animate: !1, padding: 35, nodeRepulsion: 6500, idealEdgeLength: 120 }
      }), setTimeout(() => V == null ? void 0 : V.fit(void 0, 30), 100);
    }
    function qe(R) {
      return R instanceof Error ? R.message : String(R);
    }
    return (R, S) => {
      var D;
      return Q(), ne("div", Ul, [
        j("div", Wl, [
          j("div", Bl, [
            As(j("input", {
              "onUpdate:modelValue": S[0] || (S[0] = (T) => r.value = T),
              class: "search",
              placeholder: "Поиск объекта 1С…",
              spellcheck: "false"
            }, null, 512), [
              [kn, r.value]
            ]),
            o.value.length ? (Q(), ne("ul", kl, [
              (Q(!0), ne(pe, null, Tt(o.value, (T) => {
                var de;
                return Q(), ne("li", {
                  key: T.fullName,
                  class: Ut({ sel: ((de = l.value) == null ? void 0 : de.fullName) === T.fullName }),
                  onClick: (ze) => ge(T)
                }, te(T.fullName), 11, Gl);
              }), 128))
            ])) : Ot("", !0),
            j("div", Jl, [
              (Q(!0), ne(pe, null, Tt(n.value, (T) => (Q(), ne("div", {
                key: T.kind,
                class: "tree-section"
              }, [
                j("div", {
                  class: "tree-section-head",
                  onClick: (de) => X(T.kind)
                }, [
                  j("span", zl, te(i[T.kind] ? "▾" : "▸"), 1),
                  dt(te(T.section) + " (" + te(T.count) + ") ", 1)
                ], 8, ql),
                i[T.kind] ? (Q(), ne("div", Yl, [
                  (Q(!0), ne(pe, null, Tt(T.objects, (de) => {
                    var ze;
                    return Q(), cn(Kl, {
                      key: de.fullName,
                      node: de,
                      selected: (ze = l.value) == null ? void 0 : ze.fullName,
                      onSelect: ge
                    }, null, 8, ["node", "selected"]);
                  }), 128))
                ])) : Ot("", !0)
              ]))), 128))
            ])
          ]),
          j("div", Xl, [
            j("div", null, [
              S[6] || (S[6] = j("span", { class: "muted" }, "Objects", -1)),
              S[7] || (S[7] = dt()),
              j("b", null, te(s.objectCount), 1)
            ]),
            j("div", null, [
              S[8] || (S[8] = j("span", { class: "muted" }, "Relations", -1)),
              S[9] || (S[9] = dt()),
              j("b", null, te(s.relationCount), 1)
            ]),
            j("div", null, [
              S[10] || (S[10] = j("span", { class: "muted" }, "Sections", -1)),
              S[11] || (S[11] = dt()),
              j("b", null, te(s.sectionCount), 1)
            ])
          ]),
          j("div", Zl, [
            S[12] || (S[12] = j("div", { class: "muted" }, "Context export", -1)),
            j("div", Ql, [
              As(j("select", {
                "onUpdate:modelValue": S[1] || (S[1] = (T) => d.value = T)
              }, [
                (Q(!0), ne(pe, null, Tt(f.value, (T) => (Q(), ne("option", {
                  key: T.id,
                  value: T.id
                }, te(T.name), 9, ec))), 128))
              ], 512), [
                [Ol, d.value]
              ]),
              j("button", {
                onClick: Ce,
                disabled: !l.value
              }, "Copy", 8, tc)
            ]),
            S[13] || (S[13] = j("div", { class: "muted" }, "Rebuild index", -1)),
            j("div", sc, [
              As(j("input", {
                "onUpdate:modelValue": S[2] || (S[2] = (T) => a.value = T),
                class: "grow",
                placeholder: "Путь к дампу конфигурации 1С…",
                spellcheck: "false"
              }, null, 512), [
                [kn, a.value]
              ]),
              j("button", {
                onClick: lt,
                disabled: p.value
              }, te(p.value ? "…" : "Rebuild"), 9, nc)
            ]),
            w.value ? (Q(), ne("div", ic, te(w.value), 1)) : Ot("", !0)
          ])
        ]),
        j("div", rc, [
          j("div", oc, te(((D = l.value) == null ? void 0 : D.fullName) || "Выберите объект"), 1),
          j("div", lc, [
            j("button", {
              onClick: S[3] || (S[3] = (T) => ae("dependencies")),
              disabled: !l.value
            }, "Dependencies Graph", 8, cc),
            j("button", {
              onClick: S[4] || (S[4] = (T) => ae("references")),
              disabled: !l.value
            }, "References Graph", 8, fc),
            j("button", {
              onClick: S[5] || (S[5] = (T) => ae("dataflow")),
              disabled: !l.value
            }, "Data Flow Graph", 8, uc)
          ]),
          j("div", ac, te(E.value), 1),
          l.value ? (Q(), ne("pre", dc, te(G.value), 1)) : Ot("", !0),
          j("div", {
            ref_key: "cyEl",
            ref: L,
            class: "cy"
          }, null, 512)
        ])
      ]);
    };
  }
}), pc = /* @__PURE__ */ tr(hc, [["__scopeId", "data-v-b0fa540a"]]);
function mc(e, t) {
  let s = Nl(pc, { api: t });
  return s.mount(e), {
    // The OneC browser edits no settings blob — return the unchanged JSON so a Save is a no-op.
    save: () => t.getJson(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  mc as mount
};
