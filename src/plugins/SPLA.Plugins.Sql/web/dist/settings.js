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
const B = {}, ot = [], Ie = () => {
}, Jn = () => !1, is = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), rs = (e) => e.startsWith("onUpdate:"), Z = Object.assign, ks = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, sr = Object.prototype.hasOwnProperty, $ = (e, t) => sr.call(e, t), F = Array.isArray, lt = (e) => Nt(e) === "[object Map]", pt = (e) => Nt(e) === "[object Set]", hn = (e) => Nt(e) === "[object Date]", D = (e) => typeof e == "function", Y = (e) => typeof e == "string", Re = (e) => typeof e == "symbol", K = (e) => e !== null && typeof e == "object", Gn = (e) => (K(e) || D(e)) && D(e.then) && D(e.catch), Yn = Object.prototype.toString, Nt = (e) => Yn.call(e), nr = (e) => Nt(e).slice(8, -1), zn = (e) => Nt(e) === "[object Object]", qs = (e) => Y(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, Ct = /* @__PURE__ */ Bs(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), os = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, ir = /-\w/g, ge = os(
  (e) => e.replace(ir, (t) => t.slice(1).toUpperCase())
), rr = /\B([A-Z])/g, st = os(
  (e) => e.replace(rr, "-$1").toLowerCase()
), Xn = os((e) => e.charAt(0).toUpperCase() + e.slice(1)), bs = os(
  (e) => e ? `on${Xn(e)}` : ""
), Me = (e, t) => !Object.is(e, t), Gt = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Qn = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, ls = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let gn;
const cs = () => gn || (gn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function Js(e) {
  if (F(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], i = Y(n) ? fr(n) : Js(n);
      if (i)
        for (const r in i)
          t[r] = i[r];
    }
    return t;
  } else if (Y(e) || K(e))
    return e;
}
const or = /;(?![^(]*\))/g, lr = /:([^]+)/, cr = /\/\*[^]*?\*\//g;
function fr(e) {
  const t = {};
  return e.replace(cr, "").split(or).forEach((s) => {
    if (s) {
      const n = s.split(lr);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function Gs(e) {
  let t = "";
  if (Y(e))
    t = e;
  else if (F(e))
    for (let s = 0; s < e.length; s++) {
      const n = Gs(e[s]);
      n && (t += n + " ");
    }
  else if (K(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const ur = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", ar = /* @__PURE__ */ Bs(ur);
function Zn(e) {
  return !!e || e === "";
}
function dr(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = ht(e[n], t[n]);
  return s;
}
function ht(e, t) {
  if (e === t) return !0;
  let s = hn(e), n = hn(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Re(e), n = Re(t), s || n)
    return e === t;
  if (s = F(e), n = F(t), s || n)
    return s && n ? dr(e, t) : !1;
  if (s = K(e), n = K(t), s || n) {
    if (!s || !n)
      return !1;
    const i = Object.keys(e).length, r = Object.keys(t).length;
    if (i !== r)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), f = t.hasOwnProperty(o);
      if (l && !f || !l && f || !ht(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function Ys(e, t) {
  return e.findIndex((s) => ht(s, t));
}
const ei = (e) => !!(e && e.__v_isRef === !0), it = (e) => Y(e) ? e : e == null ? "" : F(e) || K(e) && (e.toString === Yn || !D(e.toString)) ? ei(e) ? it(e.value) : JSON.stringify(e, ti, 2) : String(e), ti = (e, t) => ei(t) ? ti(e, t.value) : lt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, i], r) => (s[vs(n, r) + " =>"] = i, s),
    {}
  )
} : pt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => vs(s))
} : Re(t) ? vs(t) : K(t) && !F(t) && !zn(t) ? String(t) : t, vs = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Re(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Q;
class pr {
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
        const i = this.parent.scopes.pop();
        i && i !== this && (this.parent.scopes[this.index] = i, i.index = this.index);
      }
      this.parent = void 0;
    }
  }
}
function hr() {
  return Q;
}
let q;
const ys = /* @__PURE__ */ new WeakSet();
class si {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, Q && (Q.active ? Q.effects.push(this) : this.flags &= -2);
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
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || ii(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, mn(this), ri(this);
    const t = q, s = me;
    q = this, me = !0;
    try {
      return this.fn();
    } finally {
      oi(this), q = t, me = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        Qs(t);
      this.deps = this.depsTail = void 0, mn(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? ys.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Fs(this) && this.run();
  }
  get dirty() {
    return Fs(this);
  }
}
let ni = 0, Tt, Et;
function ii(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Et, Et = e;
    return;
  }
  e.next = Tt, Tt = e;
}
function zs() {
  ni++;
}
function Xs() {
  if (--ni > 0)
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
function ri(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function oi(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const i = n.prevDep;
    n.version === -1 ? (n === s && (s = i), Qs(n), gr(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = i;
  }
  e.deps = t, e.depsTail = s;
}
function Fs(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (li(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function li(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === It) || (e.globalVersion = It, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Fs(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = q, n = me;
  q = e, me = !0;
  try {
    ri(e);
    const i = e.fn(e._value);
    (t.version === 0 || Me(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    q = s, me = n, oi(e), e.flags &= -3;
  }
}
function Qs(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: i } = e;
  if (n && (n.nextSub = i, e.prevSub = void 0), i && (i.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let r = s.computed.deps; r; r = r.nextDep)
      Qs(r, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function gr(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let me = !0;
const ci = [];
function Fe() {
  ci.push(me), me = !1;
}
function Ve() {
  const e = ci.pop();
  me = e === void 0 ? !0 : e;
}
function mn(e) {
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
let It = 0;
class mr {
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
    if (!q || !me || q === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== q)
      s = this.activeLink = new mr(q, this), q.deps ? (s.prevDep = q.depsTail, q.depsTail.nextDep = s, q.depsTail = s) : q.deps = q.depsTail = s, fi(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = q.depsTail, s.nextDep = void 0, q.depsTail.nextDep = s, q.depsTail = s, q.deps === s && (q.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, It++, this.notify(t);
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
const Vs = /* @__PURE__ */ new WeakMap(), et = /* @__PURE__ */ Symbol(
  ""
), Us = /* @__PURE__ */ Symbol(
  ""
), Rt = /* @__PURE__ */ Symbol(
  ""
);
function ee(e, t, s) {
  if (me && q) {
    let n = Vs.get(e);
    n || Vs.set(e, n = /* @__PURE__ */ new Map());
    let i = n.get(s);
    i || (n.set(s, i = new Zs()), i.map = n, i.key = s), i.track();
  }
}
function je(e, t, s, n, i, r) {
  const o = Vs.get(e);
  if (!o) {
    It++;
    return;
  }
  const l = (f) => {
    f && f.trigger();
  };
  if (zs(), t === "clear")
    o.forEach(l);
  else {
    const f = F(e), d = f && qs(s);
    if (f && s === "length") {
      const a = Number(n);
      o.forEach((g, E) => {
        (E === "length" || E === Rt || !Re(E) && E >= a) && l(g);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(Rt)), t) {
        case "add":
          f ? d && l(o.get("length")) : (l(o.get(et)), lt(e) && l(o.get(Us)));
          break;
        case "delete":
          f || (l(o.get(et)), lt(e) && l(o.get(Us)));
          break;
        case "set":
          lt(e) && l(o.get(et));
          break;
      }
  }
  Xs();
}
function nt(e) {
  const t = /* @__PURE__ */ H(e);
  return t === e ? t : (ee(t, "iterate", Rt), /* @__PURE__ */ he(e) ? t : t.map(_e));
}
function fs(e) {
  return ee(e = /* @__PURE__ */ H(e), "iterate", Rt), e;
}
function Ae(e, t) {
  return /* @__PURE__ */ Ke(e) ? ut(/* @__PURE__ */ tt(e) ? _e(t) : t) : _e(t);
}
const _r = {
  __proto__: null,
  [Symbol.iterator]() {
    return xs(this, Symbol.iterator, (e) => Ae(this, e));
  },
  concat(...e) {
    return nt(this).concat(
      ...e.map((t) => F(t) ? nt(t) : t)
    );
  },
  entries() {
    return xs(this, "entries", (e) => (e[1] = Ae(this, e[1]), e));
  },
  every(e, t) {
    return Ue(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return Ue(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => Ae(this, n)),
      arguments
    );
  },
  find(e, t) {
    return Ue(
      this,
      "find",
      e,
      t,
      (s) => Ae(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return Ue(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return Ue(
      this,
      "findLast",
      e,
      t,
      (s) => Ae(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return Ue(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return Ue(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return ws(this, "includes", e);
  },
  indexOf(...e) {
    return ws(this, "indexOf", e);
  },
  join(e) {
    return nt(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return ws(this, "lastIndexOf", e);
  },
  map(e, t) {
    return Ue(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return vt(this, "pop");
  },
  push(...e) {
    return vt(this, "push", e);
  },
  reduce(e, ...t) {
    return _n(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return _n(this, "reduceRight", e, t);
  },
  shift() {
    return vt(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return Ue(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return vt(this, "splice", e);
  },
  toReversed() {
    return nt(this).toReversed();
  },
  toSorted(e) {
    return nt(this).toSorted(e);
  },
  toSpliced(...e) {
    return nt(this).toSpliced(...e);
  },
  unshift(...e) {
    return vt(this, "unshift", e);
  },
  values() {
    return xs(this, "values", (e) => Ae(this, e));
  }
};
function xs(e, t, s) {
  const n = fs(e), i = n[t]();
  return n !== e && !/* @__PURE__ */ he(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = s(r.value)), r;
  }), i;
}
const br = Array.prototype;
function Ue(e, t, s, n, i, r) {
  const o = fs(e), l = o !== e && !/* @__PURE__ */ he(e), f = o[t];
  if (f !== br[t]) {
    const g = f.apply(e, r);
    return l ? _e(g) : g;
  }
  let d = s;
  o !== e && (l ? d = function(g, E) {
    return s.call(this, Ae(e, g), E, e);
  } : s.length > 2 && (d = function(g, E) {
    return s.call(this, g, E, e);
  }));
  const a = f.call(o, d, n);
  return l && i ? i(a) : a;
}
function _n(e, t, s, n) {
  const i = fs(e), r = i !== e && !/* @__PURE__ */ he(e);
  let o = s, l = !1;
  i !== e && (r ? (l = n.length === 0, o = function(d, a, g) {
    return l && (l = !1, d = Ae(e, d)), s.call(this, d, Ae(e, a), g, e);
  }) : s.length > 3 && (o = function(d, a, g) {
    return s.call(this, d, a, g, e);
  }));
  const f = i[t](o, ...n);
  return l ? Ae(e, f) : f;
}
function ws(e, t, s) {
  const n = /* @__PURE__ */ H(e);
  ee(n, "iterate", Rt);
  const i = n[t](...s);
  return (i === -1 || i === !1) && /* @__PURE__ */ sn(s[0]) ? (s[0] = /* @__PURE__ */ H(s[0]), n[t](...s)) : i;
}
function vt(e, t, s = []) {
  Fe(), zs();
  const n = (/* @__PURE__ */ H(e))[t].apply(e, s);
  return Xs(), Ve(), n;
}
const vr = /* @__PURE__ */ Bs("__proto__,__v_isRef,__isVue"), ui = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Re)
);
function yr(e) {
  Re(e) || (e = String(e));
  const t = /* @__PURE__ */ H(this);
  return ee(t, "has", e), t.hasOwnProperty(e);
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
      return n === (i ? r ? Mr : gi : r ? hi : pi).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = F(t);
    if (!i) {
      let f;
      if (o && (f = _r[s]))
        return f;
      if (s === "hasOwnProperty")
        return yr;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ te(t) ? t : n
    );
    if ((Re(s) ? ui.has(s) : vr(s)) || (i || ee(t, "get", s), r))
      return l;
    if (/* @__PURE__ */ te(l)) {
      const f = o && qs(s) ? l : l.value;
      return i && K(f) ? /* @__PURE__ */ Ns(f) : f;
    }
    return K(l) ? i ? /* @__PURE__ */ Ns(l) : /* @__PURE__ */ us(l) : l;
  }
}
class di extends ai {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, i) {
    let r = t[s];
    const o = F(t) && qs(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ Ke(r);
      if (!/* @__PURE__ */ he(n) && !/* @__PURE__ */ Ke(n) && (r = /* @__PURE__ */ H(r), n = /* @__PURE__ */ H(n)), !o && /* @__PURE__ */ te(r) && !/* @__PURE__ */ te(n))
        return d || (r.value = n), !0;
    }
    const l = o ? Number(s) < t.length : $(t, s), f = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ te(t) ? t : i
    );
    return t === /* @__PURE__ */ H(i) && f && (l ? Me(n, r) && je(t, "set", s, n) : je(t, "add", s, n)), f;
  }
  deleteProperty(t, s) {
    const n = $(t, s);
    t[s];
    const i = Reflect.deleteProperty(t, s);
    return i && n && je(t, "delete", s, void 0), i;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Re(s) || !ui.has(s)) && ee(t, "has", s), n;
  }
  ownKeys(t) {
    return ee(
      t,
      "iterate",
      F(t) ? "length" : et
    ), Reflect.ownKeys(t);
  }
}
class xr extends ai {
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
const wr = /* @__PURE__ */ new di(), Sr = /* @__PURE__ */ new xr(), Cr = /* @__PURE__ */ new di(!0);
const Ds = (e) => e, kt = (e) => Reflect.getPrototypeOf(e);
function Tr(e, t, s) {
  return function(...n) {
    const i = this.__v_raw, r = /* @__PURE__ */ H(i), o = lt(r), l = e === "entries" || e === Symbol.iterator && o, f = e === "keys" && o, d = i[e](...n), a = s ? Ds : t ? ut : _e;
    return !t && ee(
      r,
      "iterate",
      f ? Us : et
    ), Z(
      // inheriting all iterator properties
      Object.create(d),
      {
        // iterator protocol
        next() {
          const { value: g, done: E } = d.next();
          return E ? { value: g, done: E } : {
            value: l ? [a(g[0]), a(g[1])] : a(g),
            done: E
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
function Er(e, t) {
  const s = {
    get(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ H(r), l = /* @__PURE__ */ H(i);
      e || (Me(i, l) && ee(o, "get", i), ee(o, "get", l));
      const { has: f } = kt(o), d = t ? Ds : e ? ut : _e;
      if (f.call(o, i))
        return d(r.get(i));
      if (f.call(o, l))
        return d(r.get(l));
      r !== o && r.get(i);
    },
    get size() {
      const i = this.__v_raw;
      return !e && ee(/* @__PURE__ */ H(i), "iterate", et), i.size;
    },
    has(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ H(r), l = /* @__PURE__ */ H(i);
      return e || (Me(i, l) && ee(o, "has", i), ee(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, f = /* @__PURE__ */ H(l), d = t ? Ds : e ? ut : _e;
      return !e && ee(f, "iterate", et), l.forEach((a, g) => i.call(r, d(a), d(g), o));
    }
  };
  return Z(
    s,
    e ? {
      add: qt("add"),
      set: qt("set"),
      delete: qt("delete"),
      clear: qt("clear")
    } : {
      add(i) {
        const r = /* @__PURE__ */ H(this), o = kt(r), l = /* @__PURE__ */ H(i), f = !t && !/* @__PURE__ */ he(i) && !/* @__PURE__ */ Ke(i) ? l : i;
        return o.has.call(r, f) || Me(i, f) && o.has.call(r, i) || Me(l, f) && o.has.call(r, l) || (r.add(f), je(r, "add", f, f)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ he(r) && !/* @__PURE__ */ Ke(r) && (r = /* @__PURE__ */ H(r));
        const o = /* @__PURE__ */ H(this), { has: l, get: f } = kt(o);
        let d = l.call(o, i);
        d || (i = /* @__PURE__ */ H(i), d = l.call(o, i));
        const a = f.call(o, i);
        return o.set(i, r), d ? Me(r, a) && je(o, "set", i, r) : je(o, "add", i, r), this;
      },
      delete(i) {
        const r = /* @__PURE__ */ H(this), { has: o, get: l } = kt(r);
        let f = o.call(r, i);
        f || (i = /* @__PURE__ */ H(i), f = o.call(r, i)), l && l.call(r, i);
        const d = r.delete(i);
        return f && je(r, "delete", i, void 0), d;
      },
      clear() {
        const i = /* @__PURE__ */ H(this), r = i.size !== 0, o = i.clear();
        return r && je(
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
    s[i] = Tr(i, e, t);
  }), s;
}
function en(e, t) {
  const s = Er(e, t);
  return (n, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? n : Reflect.get(
    $(s, i) && i in n ? s : n,
    i,
    r
  );
}
const Or = {
  get: /* @__PURE__ */ en(!1, !1)
}, Ar = {
  get: /* @__PURE__ */ en(!1, !0)
}, Pr = {
  get: /* @__PURE__ */ en(!0, !1)
};
const pi = /* @__PURE__ */ new WeakMap(), hi = /* @__PURE__ */ new WeakMap(), gi = /* @__PURE__ */ new WeakMap(), Mr = /* @__PURE__ */ new WeakMap();
function Ir(e) {
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
function us(e) {
  return /* @__PURE__ */ Ke(e) ? e : tn(
    e,
    !1,
    wr,
    Or,
    pi
  );
}
// @__NO_SIDE_EFFECTS__
function Rr(e) {
  return tn(
    e,
    !1,
    Cr,
    Ar,
    hi
  );
}
// @__NO_SIDE_EFFECTS__
function Ns(e) {
  return tn(
    e,
    !0,
    Sr,
    Pr,
    gi
  );
}
function tn(e, t, s, n, i) {
  if (!K(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const r = i.get(e);
  if (r)
    return r;
  const o = Ir(nr(e));
  if (o === 0)
    return e;
  const l = new Proxy(
    e,
    o === 2 ? n : s
  );
  return i.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function tt(e) {
  return /* @__PURE__ */ Ke(e) ? /* @__PURE__ */ tt(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function Ke(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function he(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function sn(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function H(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ H(t) : e;
}
function Fr(e) {
  return !$(e, "__v_skip") && Object.isExtensible(e) && Qn(e, "__v_skip", !0), e;
}
const _e = (e) => K(e) ? /* @__PURE__ */ us(e) : e, ut = (e) => K(e) ? /* @__PURE__ */ Ns(e) : e;
// @__NO_SIDE_EFFECTS__
function te(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Ss(e) {
  return Vr(e, !1);
}
function Vr(e, t) {
  return /* @__PURE__ */ te(e) ? e : new Ur(e, t);
}
class Ur {
  constructor(t, s) {
    this.dep = new Zs(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ H(t), this._value = s ? t : _e(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ he(t) || /* @__PURE__ */ Ke(t);
    t = n ? t : /* @__PURE__ */ H(t), Me(t, s) && (this._rawValue = t, this._value = n ? t : _e(t), this.dep.trigger());
  }
}
function Dr(e) {
  return /* @__PURE__ */ te(e) ? e.value : e;
}
const Nr = {
  get: (e, t, s) => t === "__v_raw" ? e : Dr(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const i = e[t];
    return /* @__PURE__ */ te(i) && !/* @__PURE__ */ te(s) ? (i.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function mi(e) {
  return /* @__PURE__ */ tt(e) ? e : new Proxy(e, Nr);
}
class jr {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Zs(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = It - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    q !== this)
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
function Hr(e, t, s = !1) {
  let n, i;
  return D(e) ? n = e : (n = e.get, i = e.set), new jr(n, i, s);
}
const Jt = {}, Xt = /* @__PURE__ */ new WeakMap();
let Ze;
function $r(e, t = !1, s = Ze) {
  if (s) {
    let n = Xt.get(s);
    n || Xt.set(s, n = []), n.push(e);
  }
}
function Lr(e, t, s = B) {
  const { immediate: n, deep: i, once: r, scheduler: o, augmentJob: l, call: f } = s, d = (R) => i ? R : /* @__PURE__ */ he(R) || i === !1 || i === 0 ? He(R, 1) : He(R);
  let a, g, E, O, j = !1, U = !1;
  if (/* @__PURE__ */ te(e) ? (g = () => e.value, j = /* @__PURE__ */ he(e)) : /* @__PURE__ */ tt(e) ? (g = () => d(e), j = !0) : F(e) ? (U = !0, j = e.some((R) => /* @__PURE__ */ tt(R) || /* @__PURE__ */ he(R)), g = () => e.map((R) => {
    if (/* @__PURE__ */ te(R))
      return R.value;
    if (/* @__PURE__ */ tt(R))
      return d(R);
    if (D(R))
      return f ? f(R, 2) : R();
  })) : D(e) ? t ? g = f ? () => f(e, 2) : e : g = () => {
    if (E) {
      Fe();
      try {
        E();
      } finally {
        Ve();
      }
    }
    const R = Ze;
    Ze = a;
    try {
      return f ? f(e, 3, [O]) : e(O);
    } finally {
      Ze = R;
    }
  } : g = Ie, t && i) {
    const R = g, P = i === !0 ? 1 / 0 : i;
    g = () => He(R(), P);
  }
  const G = hr(), C = () => {
    a.stop(), G && G.active && ks(G.effects, a);
  };
  if (r && t) {
    const R = t;
    t = (...P) => {
      const ve = R(...P);
      return C(), ve;
    };
  }
  let h = U ? new Array(e.length).fill(Jt) : Jt;
  const w = (R) => {
    if (!(!(a.flags & 1) || !a.dirty && !R))
      if (t) {
        const P = a.run();
        if (R || i || j || (U ? P.some((ve, ye) => Me(ve, h[ye])) : Me(P, h))) {
          E && E();
          const ve = Ze;
          Ze = a;
          try {
            const ye = [
              P,
              // pass undefined as the old value when it's changed for the first time
              h === Jt ? void 0 : U && h[0] === Jt ? [] : h,
              O
            ];
            h = P, f ? f(t, 3, ye) : (
              // @ts-expect-error
              t(...ye)
            );
          } finally {
            Ze = ve;
          }
        }
      } else
        a.run();
  };
  return l && l(w), a = new si(g), a.scheduler = o ? () => o(w, !1) : w, O = (R) => $r(R, !1, a), E = a.onStop = () => {
    const R = Xt.get(a);
    if (R) {
      if (f)
        f(R, 4);
      else
        for (const P of R) P();
      Xt.delete(a);
    }
  }, t ? n ? w(!0) : h = a.run() : o ? o(w.bind(null, !0), !0) : a.run(), C.pause = a.pause.bind(a), C.resume = a.resume.bind(a), C.stop = C, C;
}
function He(e, t = 1 / 0, s) {
  if (t <= 0 || !K(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ te(e))
    He(e.value, t, s);
  else if (F(e))
    for (let n = 0; n < e.length; n++)
      He(e[n], t, s);
  else if (pt(e) || lt(e))
    e.forEach((n) => {
      He(n, t, s);
    });
  else if (zn(e)) {
    for (const n in e)
      He(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && He(e[n], t, s);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function jt(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (i) {
    as(i, t, s);
  }
}
function be(e, t, s, n) {
  if (D(e)) {
    const i = jt(e, t, s, n);
    return i && Gn(i) && i.catch((r) => {
      as(r, t, s);
    }), i;
  }
  if (F(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(be(e[r], t, s, n));
    return i;
  }
}
function as(e, t, s, n = !0) {
  const i = t ? t.vnode : null, { errorHandler: r, throwUnhandledErrorInProduction: o } = t && t.appContext.config || B;
  if (t) {
    let l = t.parent;
    const f = t.proxy, d = `https://vuejs.org/error-reference/#runtime-${s}`;
    for (; l; ) {
      const a = l.ec;
      if (a) {
        for (let g = 0; g < a.length; g++)
          if (a[g](e, f, d) === !1)
            return;
      }
      l = l.parent;
    }
    if (r) {
      Fe(), jt(r, null, 10, [
        e,
        f,
        d
      ]), Ve();
      return;
    }
  }
  Kr(e, s, i, n, o);
}
function Kr(e, t, s, n = !0, i = !1) {
  if (i)
    throw e;
  console.error(e);
}
const re = [];
let Oe = -1;
const ct = [];
let ke = null, rt = 0;
const _i = /* @__PURE__ */ Promise.resolve();
let Qt = null;
function bi(e) {
  const t = Qt || _i;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Wr(e) {
  let t = Oe + 1, s = re.length;
  for (; t < s; ) {
    const n = t + s >>> 1, i = re[n], r = Ft(i);
    r < e || r === e && i.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function nn(e) {
  if (!(e.flags & 1)) {
    const t = Ft(e), s = re[re.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Ft(s) ? re.push(e) : re.splice(Wr(t), 0, e), e.flags |= 1, vi();
  }
}
function vi() {
  Qt || (Qt = _i.then(xi));
}
function Br(e) {
  F(e) ? ct.push(...e) : ke && e.id === -1 ? ke.splice(rt + 1, 0, e) : e.flags & 1 || (ct.push(e), e.flags |= 1), vi();
}
function bn(e, t, s = Oe + 1) {
  for (; s < re.length; s++) {
    const n = re[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      re.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function yi(e) {
  if (ct.length) {
    const t = [...new Set(ct)].sort(
      (s, n) => Ft(s) - Ft(n)
    );
    if (ct.length = 0, ke) {
      ke.push(...t);
      return;
    }
    for (ke = t, rt = 0; rt < ke.length; rt++) {
      const s = ke[rt];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    ke = null, rt = 0;
  }
}
const Ft = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function xi(e) {
  try {
    for (Oe = 0; Oe < re.length; Oe++) {
      const t = re[Oe];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), jt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Oe < re.length; Oe++) {
      const t = re[Oe];
      t && (t.flags &= -2);
    }
    Oe = -1, re.length = 0, yi(), Qt = null, (re.length || ct.length) && xi();
  }
}
let pe = null, wi = null;
function Zt(e) {
  const t = pe;
  return pe = e, wi = e && e.type.__scopeId || null, t;
}
function kr(e, t = pe, s) {
  if (!t || e._n)
    return e;
  const n = (...i) => {
    n._d && Pn(-1);
    const r = Zt(t);
    let o;
    try {
      o = e(...i);
    } finally {
      Zt(r), n._d && Pn(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function le(e, t) {
  if (pe === null)
    return e;
  const s = gs(pe), n = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [r, o, l, f = B] = t[i];
    r && (D(r) && (r = {
      mounted: r,
      updated: r
    }), r.deep && He(o), n.push({
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
function Xe(e, t, s, n) {
  const i = e.dirs, r = t && t.dirs;
  for (let o = 0; o < i.length; o++) {
    const l = i[o];
    r && (l.oldValue = r[o].value);
    let f = l.dir[n];
    f && (Fe(), be(f, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Ve());
  }
}
function qr(e, t) {
  if (oe) {
    let s = oe.provides;
    const n = oe.parent && oe.parent.provides;
    n === s && (s = oe.provides = Object.create(n)), s[e] = t;
  }
}
function Yt(e, t, s = !1) {
  const n = Jo();
  if (n || ft) {
    let i = ft ? ft._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return s && D(t) ? t.call(n && n.proxy) : t;
  }
}
const Jr = /* @__PURE__ */ Symbol.for("v-scx"), Gr = () => Yt(Jr);
function Cs(e, t, s) {
  return Si(e, t, s);
}
function Si(e, t, s = B) {
  const { immediate: n, deep: i, flush: r, once: o } = s, l = Z({}, s), f = t && n || !t && r !== "post";
  let d;
  if (Ut) {
    if (r === "sync") {
      const O = Gr();
      d = O.__watcherHandles || (O.__watcherHandles = []);
    } else if (!f) {
      const O = () => {
      };
      return O.stop = Ie, O.resume = Ie, O.pause = Ie, O;
    }
  }
  const a = oe;
  l.call = (O, j, U) => be(O, a, j, U);
  let g = !1;
  r === "post" ? l.scheduler = (O) => {
    fe(O, a && a.suspense);
  } : r !== "sync" && (g = !0, l.scheduler = (O, j) => {
    j ? O() : nn(O);
  }), l.augmentJob = (O) => {
    t && (O.flags |= 4), g && (O.flags |= 2, a && (O.id = a.uid, O.i = a));
  };
  const E = Lr(e, t, l);
  return Ut && (d ? d.push(E) : f && E()), E;
}
function Yr(e, t, s) {
  const n = this.proxy, i = Y(e) ? e.includes(".") ? Ci(n, e) : () => n[e] : e.bind(n, n);
  let r;
  D(t) ? r = t : (r = t.handler, s = t);
  const o = Ht(this), l = Si(i, r.bind(n), s);
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
const zr = /* @__PURE__ */ Symbol("_vte"), Xr = (e) => e.__isTeleport, Ts = /* @__PURE__ */ Symbol("_leaveCb");
function rn(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, rn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Qr(e, t) {
  return D(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    Z({ name: e.name }, t, { setup: e })
  ) : e;
}
function Ti(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function vn(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const es = /* @__PURE__ */ new WeakMap();
function Ot(e, t, s, n, i = !1) {
  if (F(e)) {
    e.forEach(
      (U, G) => Ot(
        U,
        t && (F(t) ? t[G] : t),
        s,
        n,
        i
      )
    );
    return;
  }
  if (At(n) && !i) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Ot(e, t, s, n.component.subTree);
    return;
  }
  const r = n.shapeFlag & 4 ? gs(n.component) : n.el, o = i ? null : r, { i: l, r: f } = e, d = t && t.r, a = l.refs === B ? l.refs = {} : l.refs, g = l.setupState, E = /* @__PURE__ */ H(g), O = g === B ? Jn : (U) => vn(a, U) ? !1 : $(E, U), j = (U, G) => !(G && vn(a, G));
  if (d != null && d !== f) {
    if (yn(t), Y(d))
      a[d] = null, O(d) && (g[d] = null);
    else if (/* @__PURE__ */ te(d)) {
      const U = t;
      j(d, U.k) && (d.value = null), U.k && (a[U.k] = null);
    }
  }
  if (D(f)) {
    Fe();
    try {
      jt(f, l, 12, [o, a]);
    } finally {
      Ve();
    }
  } else {
    const U = Y(f), G = /* @__PURE__ */ te(f);
    if (U || G) {
      const C = () => {
        if (e.f) {
          const h = U ? O(f) ? g[f] : a[f] : j() || !e.k ? f.value : a[e.k];
          if (i)
            F(h) && ks(h, r);
          else if (F(h))
            h.includes(r) || h.push(r);
          else if (U)
            a[f] = [r], O(f) && (g[f] = a[f]);
          else {
            const w = [r];
            j(f, e.k) && (f.value = w), e.k && (a[e.k] = w);
          }
        } else U ? (a[f] = o, O(f) && (g[f] = o)) : G && (j(f, e.k) && (f.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const h = () => {
          C(), es.delete(e);
        };
        h.id = -1, es.set(e, h), fe(h, s);
      } else
        yn(e), C();
    }
  }
}
function yn(e) {
  const t = es.get(e);
  t && (t.flags |= 8, es.delete(e));
}
cs().requestIdleCallback;
cs().cancelIdleCallback;
const At = (e) => !!e.type.__asyncLoader, Ei = (e) => e.type.__isKeepAlive;
function Zr(e, t) {
  Oi(e, "a", t);
}
function eo(e, t) {
  Oi(e, "da", t);
}
function Oi(e, t, s = oe) {
  const n = e.__wdc || (e.__wdc = () => {
    let i = s;
    for (; i; ) {
      if (i.isDeactivated)
        return;
      i = i.parent;
    }
    return e();
  });
  if (ds(t, n, s), s) {
    let i = s.parent;
    for (; i && i.parent; )
      Ei(i.parent.vnode) && to(n, t, s, i), i = i.parent;
  }
}
function to(e, t, s, n) {
  const i = ds(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Pi(() => {
    ks(n[t], i);
  }, s);
}
function ds(e, t, s = oe, n = !1) {
  if (s) {
    const i = s[e] || (s[e] = []), r = t.__weh || (t.__weh = (...o) => {
      Fe();
      const l = Ht(s), f = be(t, s, e, o);
      return l(), Ve(), f;
    });
    return n ? i.unshift(r) : i.push(r), r;
  }
}
const We = (e) => (t, s = oe) => {
  (!Ut || e === "sp") && ds(e, (...n) => t(...n), s);
}, so = We("bm"), Ai = We("m"), no = We(
  "bu"
), io = We("u"), ro = We(
  "bum"
), Pi = We("um"), oo = We(
  "sp"
), lo = We("rtg"), co = We("rtc");
function fo(e, t = oe) {
  ds("ec", e, t);
}
const uo = /* @__PURE__ */ Symbol.for("v-ndc");
function Es(e, t, s, n) {
  let i;
  const r = s, o = F(e);
  if (o || Y(e)) {
    const l = o && /* @__PURE__ */ tt(e);
    let f = !1, d = !1;
    l && (f = !/* @__PURE__ */ he(e), d = /* @__PURE__ */ Ke(e), e = fs(e)), i = new Array(e.length);
    for (let a = 0, g = e.length; a < g; a++)
      i[a] = t(
        f ? d ? ut(_e(e[a])) : _e(e[a]) : e[a],
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
const js = (e) => e ? zi(e) ? gs(e) : js(e.parent) : null, Pt = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ Z(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => e.props,
    $attrs: (e) => e.attrs,
    $slots: (e) => e.slots,
    $refs: (e) => e.refs,
    $parent: (e) => js(e.parent),
    $root: (e) => js(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Ii(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      nn(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = bi.bind(e.proxy)),
    $watch: (e) => Yr.bind(e)
  })
), Os = (e, t) => e !== B && !e.__isScriptSetup && $(e, t), ao = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: i, props: r, accessCache: o, type: l, appContext: f } = e;
    if (t[0] !== "$") {
      const E = o[t];
      if (E !== void 0)
        switch (E) {
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
        if (Os(n, t))
          return o[t] = 1, n[t];
        if (i !== B && $(i, t))
          return o[t] = 2, i[t];
        if ($(r, t))
          return o[t] = 3, r[t];
        if (s !== B && $(s, t))
          return o[t] = 4, s[t];
        Hs && (o[t] = 0);
      }
    }
    const d = Pt[t];
    let a, g;
    if (d)
      return t === "$attrs" && ee(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== B && $(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      g = f.config.globalProperties, $(g, t)
    )
      return g[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: i, ctx: r } = e;
    return Os(i, t) ? (i[t] = s, !0) : n !== B && $(n, t) ? (n[t] = s, !0) : $(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: i, props: r, type: o }
  }, l) {
    let f;
    return !!(s[l] || e !== B && l[0] !== "$" && $(e, l) || Os(t, l) || $(r, l) || $(n, l) || $(Pt, l) || $(i.config.globalProperties, l) || (f = o.__cssModules) && f[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : $(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function xn(e) {
  return F(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let Hs = !0;
function po(e) {
  const t = Ii(e), s = e.proxy, n = e.ctx;
  Hs = !1, t.beforeCreate && wn(t.beforeCreate, e, "bc");
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
    beforeMount: g,
    mounted: E,
    beforeUpdate: O,
    updated: j,
    activated: U,
    deactivated: G,
    beforeDestroy: C,
    beforeUnmount: h,
    destroyed: w,
    unmounted: R,
    render: P,
    renderTracked: ve,
    renderTriggered: ye,
    errorCaptured: Be,
    serverPrefetch: $t,
    // public API
    expose: Ge,
    inheritAttrs: gt,
    // assets
    components: Lt,
    directives: Kt,
    filters: ms
  } = t;
  if (d && ho(d, n, null), o)
    for (const J in o) {
      const k = o[J];
      D(k) && (n[J] = k.bind(s));
    }
  if (i) {
    const J = i.call(s, s);
    K(J) && (e.data = /* @__PURE__ */ us(J));
  }
  if (Hs = !0, r)
    for (const J in r) {
      const k = r[J], Ye = D(k) ? k.bind(s, s) : D(k.get) ? k.get.bind(s, s) : Ie, Wt = !D(k) && D(k.set) ? k.set.bind(s) : Ie, ze = Zo({
        get: Ye,
        set: Wt
      });
      Object.defineProperty(n, J, {
        enumerable: !0,
        configurable: !0,
        get: () => ze.value,
        set: (xe) => ze.value = xe
      });
    }
  if (l)
    for (const J in l)
      Mi(l[J], n, s, J);
  if (f) {
    const J = D(f) ? f.call(s) : f;
    Reflect.ownKeys(J).forEach((k) => {
      qr(k, J[k]);
    });
  }
  a && wn(a, e, "c");
  function se(J, k) {
    F(k) ? k.forEach((Ye) => J(Ye.bind(s))) : k && J(k.bind(s));
  }
  if (se(so, g), se(Ai, E), se(no, O), se(io, j), se(Zr, U), se(eo, G), se(fo, Be), se(co, ve), se(lo, ye), se(ro, h), se(Pi, R), se(oo, $t), F(Ge))
    if (Ge.length) {
      const J = e.exposed || (e.exposed = {});
      Ge.forEach((k) => {
        Object.defineProperty(J, k, {
          get: () => s[k],
          set: (Ye) => s[k] = Ye,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  P && e.render === Ie && (e.render = P), gt != null && (e.inheritAttrs = gt), Lt && (e.components = Lt), Kt && (e.directives = Kt), $t && Ti(e);
}
function ho(e, t, s = Ie) {
  F(e) && (e = $s(e));
  for (const n in e) {
    const i = e[n];
    let r;
    K(i) ? "default" in i ? r = Yt(
      i.from || n,
      i.default,
      !0
    ) : r = Yt(i.from || n) : r = Yt(i), /* @__PURE__ */ te(r) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => r.value,
      set: (o) => r.value = o
    }) : t[n] = r;
  }
}
function wn(e, t, s) {
  be(
    F(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Mi(e, t, s, n) {
  let i = n.includes(".") ? Ci(s, n) : () => s[n];
  if (Y(e)) {
    const r = t[e];
    D(r) && Cs(i, r);
  } else if (D(e))
    Cs(i, e.bind(s));
  else if (K(e))
    if (F(e))
      e.forEach((r) => Mi(r, t, s, n));
    else {
      const r = D(e.handler) ? e.handler.bind(s) : t[e.handler];
      D(r) && Cs(i, r, e);
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
    (d) => ts(f, d, o, !0)
  ), ts(f, t, o)), K(t) && r.set(t, f), f;
}
function ts(e, t, s, n = !1) {
  const { mixins: i, extends: r } = t;
  r && ts(e, r, s, !0), i && i.forEach(
    (o) => ts(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = go[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const go = {
  data: Sn,
  props: Cn,
  emits: Cn,
  // objects
  methods: wt,
  computed: wt,
  // lifecycle
  beforeCreate: ne,
  created: ne,
  beforeMount: ne,
  mounted: ne,
  beforeUpdate: ne,
  updated: ne,
  beforeDestroy: ne,
  beforeUnmount: ne,
  destroyed: ne,
  unmounted: ne,
  activated: ne,
  deactivated: ne,
  errorCaptured: ne,
  serverPrefetch: ne,
  // assets
  components: wt,
  directives: wt,
  // watch
  watch: _o,
  // provide / inject
  provide: Sn,
  inject: mo
};
function Sn(e, t) {
  return t ? e ? function() {
    return Z(
      D(e) ? e.call(this, this) : e,
      D(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function mo(e, t) {
  return wt($s(e), $s(t));
}
function $s(e) {
  if (F(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++)
      t[e[s]] = e[s];
    return t;
  }
  return e;
}
function ne(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function wt(e, t) {
  return e ? Z(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Cn(e, t) {
  return e ? F(e) && F(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : Z(
    /* @__PURE__ */ Object.create(null),
    xn(e),
    xn(t ?? {})
  ) : t;
}
function _o(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = Z(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = ne(e[n], t[n]);
  return s;
}
function Ri() {
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
let bo = 0;
function vo(e, t) {
  return function(n, i = null) {
    D(n) || (n = Z({}, n)), i != null && !K(i) && (i = null);
    const r = Ri(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let f = !1;
    const d = r.app = {
      _uid: bo++,
      _component: n,
      _props: i,
      _container: null,
      _context: r,
      _instance: null,
      version: el,
      get config() {
        return r.config;
      },
      set config(a) {
      },
      use(a, ...g) {
        return o.has(a) || (a && D(a.install) ? (o.add(a), a.install(d, ...g)) : D(a) && (o.add(a), a(d, ...g))), d;
      },
      mixin(a) {
        return r.mixins.includes(a) || r.mixins.push(a), d;
      },
      component(a, g) {
        return g ? (r.components[a] = g, d) : r.components[a];
      },
      directive(a, g) {
        return g ? (r.directives[a] = g, d) : r.directives[a];
      },
      mount(a, g, E) {
        if (!f) {
          const O = d._ceVNode || $e(n, i);
          return O.appContext = r, E === !0 ? E = "svg" : E === !1 && (E = void 0), e(O, a, E), f = !0, d._container = a, a.__vue_app__ = d, gs(O.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        f && (be(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, g) {
        return r.provides[a] = g, d;
      },
      runWithContext(a) {
        const g = ft;
        ft = d;
        try {
          return a();
        } finally {
          ft = g;
        }
      }
    };
    return d;
  };
}
let ft = null;
const yo = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${ge(t)}Modifiers`] || e[`${st(t)}Modifiers`];
function xo(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || B;
  let i = s;
  const r = t.startsWith("update:"), o = r && yo(n, t.slice(7));
  o && (o.trim && (i = s.map((a) => Y(a) ? a.trim() : a)), o.number && (i = s.map(ls)));
  let l, f = n[l = bs(t)] || // also try camelCase event handler (#2249)
  n[l = bs(ge(t))];
  !f && r && (f = n[l = bs(st(t))]), f && be(
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
    e.emitted[l] = !0, be(
      d,
      e,
      6,
      i
    );
  }
}
const wo = /* @__PURE__ */ new WeakMap();
function Fi(e, t, s = !1) {
  const n = s ? wo : t.emitsCache, i = n.get(e);
  if (i !== void 0)
    return i;
  const r = e.emits;
  let o = {}, l = !1;
  if (!D(e)) {
    const f = (d) => {
      const a = Fi(d, t, !0);
      a && (l = !0, Z(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  return !r && !l ? (K(e) && n.set(e, null), null) : (F(r) ? r.forEach((f) => o[f] = null) : Z(o, r), K(e) && n.set(e, o), o);
}
function ps(e, t) {
  return !e || !is(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), $(e, t[0].toLowerCase() + t.slice(1)) || $(e, st(t)) || $(e, t));
}
function Tn(e) {
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
    props: g,
    data: E,
    setupState: O,
    ctx: j,
    inheritAttrs: U
  } = e, G = Zt(e);
  let C, h;
  try {
    if (s.shapeFlag & 4) {
      const R = i || n, P = R;
      C = Pe(
        d.call(
          P,
          R,
          a,
          g,
          O,
          E,
          j
        )
      ), h = l;
    } else {
      const R = t;
      C = Pe(
        R.length > 1 ? R(
          g,
          { attrs: l, slots: o, emit: f }
        ) : R(
          g,
          null
        )
      ), h = t.props ? l : So(l);
    }
  } catch (R) {
    Mt.length = 0, as(R, e, 1), C = $e(Je);
  }
  let w = C;
  if (h && U !== !1) {
    const R = Object.keys(h), { shapeFlag: P } = w;
    R.length && P & 7 && (r && R.some(rs) && (h = Co(
      h,
      r
    )), w = at(w, h, !1, !0));
  }
  return s.dirs && (w = at(w, null, !1, !0), w.dirs = w.dirs ? w.dirs.concat(s.dirs) : s.dirs), s.transition && rn(w, s.transition), C = w, Zt(G), C;
}
const So = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || is(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, Co = (e, t) => {
  const s = {};
  for (const n in e)
    (!rs(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function To(e, t, s) {
  const { props: n, children: i, component: r } = e, { props: o, children: l, patchFlag: f } = t, d = r.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (s && f >= 0) {
    if (f & 1024)
      return !0;
    if (f & 16)
      return n ? En(n, o, d) : !!o;
    if (f & 8) {
      const a = t.dynamicProps;
      for (let g = 0; g < a.length; g++) {
        const E = a[g];
        if (Vi(o, n, E) && !ps(d, E))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? En(n, o, d) : !0 : !!o;
  return !1;
}
function En(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < n.length; i++) {
    const r = n[i];
    if (Vi(t, e, r) && !ps(s, r))
      return !0;
  }
  return !1;
}
function Vi(e, t, s) {
  const n = e[s], i = t[s];
  return s === "style" && K(n) && K(i) ? !ht(n, i) : n !== i;
}
function Eo({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const i = t.subTree;
    if (i.suspense && i.suspense.activeBranch === e && (i.suspense.vnode.el = i.el = n, e = i), i === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const Ui = {}, Di = () => Object.create(Ui), Ni = (e) => Object.getPrototypeOf(e) === Ui;
function Oo(e, t, s, n = !1) {
  const i = {}, r = Di();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), ji(e, t, i, r);
  for (const o in e.propsOptions[0])
    o in i || (i[o] = void 0);
  s ? e.props = n ? i : /* @__PURE__ */ Rr(i) : e.type.props ? e.props = i : e.props = r, e.attrs = r;
}
function Ao(e, t, s, n) {
  const {
    props: i,
    attrs: r,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ H(i), [f] = e.propsOptions;
  let d = !1;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    (n || o > 0) && !(o & 16)
  ) {
    if (o & 8) {
      const a = e.vnode.dynamicProps;
      for (let g = 0; g < a.length; g++) {
        let E = a[g];
        if (ps(e.emitsOptions, E))
          continue;
        const O = t[E];
        if (f)
          if ($(r, E))
            O !== r[E] && (r[E] = O, d = !0);
          else {
            const j = ge(E);
            i[j] = Ls(
              f,
              l,
              j,
              O,
              e,
              !1
            );
          }
        else
          O !== r[E] && (r[E] = O, d = !0);
      }
    }
  } else {
    ji(e, t, i, r) && (d = !0);
    let a;
    for (const g in l)
      (!t || // for camelCase
      !$(t, g) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = st(g)) === g || !$(t, a))) && (f ? s && // for camelCase
      (s[g] !== void 0 || // for kebab-case
      s[a] !== void 0) && (i[g] = Ls(
        f,
        l,
        g,
        void 0,
        e,
        !0
      )) : delete i[g]);
    if (r !== l)
      for (const g in r)
        (!t || !$(t, g)) && (delete r[g], d = !0);
  }
  d && je(e.attrs, "set", "");
}
function ji(e, t, s, n) {
  const [i, r] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let f in t) {
      if (Ct(f))
        continue;
      const d = t[f];
      let a;
      i && $(i, a = ge(f)) ? !r || !r.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : ps(e.emitsOptions, f) || (!(f in n) || d !== n[f]) && (n[f] = d, o = !0);
    }
  if (r) {
    const f = /* @__PURE__ */ H(s), d = l || B;
    for (let a = 0; a < r.length; a++) {
      const g = r[a];
      s[g] = Ls(
        i,
        f,
        g,
        d[g],
        e,
        !$(d, g)
      );
    }
  }
  return o;
}
function Ls(e, t, s, n, i, r) {
  const o = e[s];
  if (o != null) {
    const l = $(o, "default");
    if (l && n === void 0) {
      const f = o.default;
      if (o.type !== Function && !o.skipFactory && D(f)) {
        const { propsDefaults: d } = i;
        if (s in d)
          n = d[s];
        else {
          const a = Ht(i);
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
    ] && (n === "" || n === st(s)) && (n = !0));
  }
  return n;
}
const Po = /* @__PURE__ */ new WeakMap();
function Hi(e, t, s = !1) {
  const n = s ? Po : t.propsCache, i = n.get(e);
  if (i)
    return i;
  const r = e.props, o = {}, l = [];
  let f = !1;
  if (!D(e)) {
    const a = (g) => {
      f = !0;
      const [E, O] = Hi(g, t, !0);
      Z(o, E), O && l.push(...O);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!r && !f)
    return K(e) && n.set(e, ot), ot;
  if (F(r))
    for (let a = 0; a < r.length; a++) {
      const g = ge(r[a]);
      On(g) && (o[g] = B);
    }
  else if (r)
    for (const a in r) {
      const g = ge(a);
      if (On(g)) {
        const E = r[a], O = o[g] = F(E) || D(E) ? { type: E } : Z({}, E), j = O.type;
        let U = !1, G = !0;
        if (F(j))
          for (let C = 0; C < j.length; ++C) {
            const h = j[C], w = D(h) && h.name;
            if (w === "Boolean") {
              U = !0;
              break;
            } else w === "String" && (G = !1);
          }
        else
          U = D(j) && j.name === "Boolean";
        O[
          0
          /* shouldCast */
        ] = U, O[
          1
          /* shouldCastTrue */
        ] = G, (U || $(O, "default")) && l.push(g);
      }
    }
  const d = [o, l];
  return K(e) && n.set(e, d), d;
}
function On(e) {
  return e[0] !== "$" && !Ct(e);
}
const on = (e) => e === "_" || e === "_ctx" || e === "$stable", ln = (e) => F(e) ? e.map(Pe) : [Pe(e)], Mo = (e, t, s) => {
  if (t._n)
    return t;
  const n = kr((...i) => ln(t(...i)), s);
  return n._c = !1, n;
}, $i = (e, t, s) => {
  const n = e._ctx;
  for (const i in e) {
    if (on(i)) continue;
    const r = e[i];
    if (D(r))
      t[i] = Mo(i, r, n);
    else if (r != null) {
      const o = ln(r);
      t[i] = () => o;
    }
  }
}, Li = (e, t) => {
  const s = ln(t);
  e.slots.default = () => s;
}, Ki = (e, t, s) => {
  for (const n in t)
    (s || !on(n)) && (e[n] = t[n]);
}, Io = (e, t, s) => {
  const n = e.slots = Di();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Ki(n, t, s), s && Qn(n, "_", i, !0)) : $i(t, n);
  } else t && Li(e, t);
}, Ro = (e, t, s) => {
  const { vnode: n, slots: i } = e;
  let r = !0, o = B;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? r = !1 : Ki(i, t, s) : (r = !t.$stable, $i(t, i)), o = t;
  } else t && (Li(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !on(l) && o[l] == null && delete i[l];
}, fe = No;
function Fo(e) {
  return Vo(e);
}
function Vo(e, t) {
  const s = cs();
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
    parentNode: g,
    nextSibling: E,
    setScopeId: O = Ie,
    insertStaticContent: j
  } = e, U = (c, u, p, v = null, b = null, m = null, S = void 0, x = null, y = !!u.dynamicChildren) => {
    if (c === u)
      return;
    c && !yt(c, u) && (v = Bt(c), xe(c, b, m, !0), c = null), u.patchFlag === -2 && (y = !1, u.dynamicChildren = null);
    const { type: _, ref: I, shapeFlag: T } = u;
    switch (_) {
      case hs:
        G(c, u, p, v);
        break;
      case Je:
        C(c, u, p, v);
        break;
      case Ps:
        c == null && h(u, p, v, S);
        break;
      case ue:
        Lt(
          c,
          u,
          p,
          v,
          b,
          m,
          S,
          x,
          y
        );
        break;
      default:
        T & 1 ? P(
          c,
          u,
          p,
          v,
          b,
          m,
          S,
          x,
          y
        ) : T & 6 ? Kt(
          c,
          u,
          p,
          v,
          b,
          m,
          S,
          x,
          y
        ) : (T & 64 || T & 128) && _.process(
          c,
          u,
          p,
          v,
          b,
          m,
          S,
          x,
          y,
          _t
        );
    }
    I != null && b ? Ot(I, c && c.ref, m, u || c, !u) : I == null && c && c.ref != null && Ot(c.ref, null, m, c, !0);
  }, G = (c, u, p, v) => {
    if (c == null)
      n(
        u.el = l(u.children),
        p,
        v
      );
    else {
      const b = u.el = c.el;
      u.children !== c.children && d(b, u.children);
    }
  }, C = (c, u, p, v) => {
    c == null ? n(
      u.el = f(u.children || ""),
      p,
      v
    ) : u.el = c.el;
  }, h = (c, u, p, v) => {
    [c.el, c.anchor] = j(
      c.children,
      u,
      p,
      v,
      c.el,
      c.anchor
    );
  }, w = ({ el: c, anchor: u }, p, v) => {
    let b;
    for (; c && c !== u; )
      b = E(c), n(c, p, v), c = b;
    n(u, p, v);
  }, R = ({ el: c, anchor: u }) => {
    let p;
    for (; c && c !== u; )
      p = E(c), i(c), c = p;
    i(u);
  }, P = (c, u, p, v, b, m, S, x, y) => {
    if (u.type === "svg" ? S = "svg" : u.type === "math" && (S = "mathml"), c == null)
      ve(
        u,
        p,
        v,
        b,
        m,
        S,
        x,
        y
      );
    else {
      const _ = c.el && c.el._isVueCE ? c.el : null;
      try {
        _ && _._beginPatch(), $t(
          c,
          u,
          b,
          m,
          S,
          x,
          y
        );
      } finally {
        _ && _._endPatch();
      }
    }
  }, ve = (c, u, p, v, b, m, S, x) => {
    let y, _;
    const { props: I, shapeFlag: T, transition: A, dirs: V } = c;
    if (y = c.el = o(
      c.type,
      m,
      I && I.is,
      I
    ), T & 8 ? a(y, c.children) : T & 16 && Be(
      c.children,
      y,
      null,
      v,
      b,
      As(c, m),
      S,
      x
    ), V && Xe(c, null, v, "created"), ye(y, c, c.scopeId, S, v), I) {
      for (const W in I)
        W !== "value" && !Ct(W) && r(y, W, null, I[W], m, v);
      "value" in I && r(y, "value", null, I.value, m), (_ = I.onVnodeBeforeMount) && Te(_, v, c);
    }
    V && Xe(c, null, v, "beforeMount");
    const N = Uo(b, A);
    N && A.beforeEnter(y), n(y, u, p), ((_ = I && I.onVnodeMounted) || N || V) && fe(() => {
      try {
        _ && Te(_, v, c), N && A.enter(y), V && Xe(c, null, v, "mounted");
      } finally {
      }
    }, b);
  }, ye = (c, u, p, v, b) => {
    if (p && O(c, p), v)
      for (let m = 0; m < v.length; m++)
        O(c, v[m]);
    if (b) {
      let m = b.subTree;
      if (u === m || qi(m.type) && (m.ssContent === u || m.ssFallback === u)) {
        const S = b.vnode;
        ye(
          c,
          S,
          S.scopeId,
          S.slotScopeIds,
          b.parent
        );
      }
    }
  }, Be = (c, u, p, v, b, m, S, x, y = 0) => {
    for (let _ = y; _ < c.length; _++) {
      const I = c[_] = x ? Ne(c[_]) : Pe(c[_]);
      U(
        null,
        I,
        u,
        p,
        v,
        b,
        m,
        S,
        x
      );
    }
  }, $t = (c, u, p, v, b, m, S) => {
    const x = u.el = c.el;
    let { patchFlag: y, dynamicChildren: _, dirs: I } = u;
    y |= c.patchFlag & 16;
    const T = c.props || B, A = u.props || B;
    let V;
    if (p && Qe(p, !1), (V = A.onVnodeBeforeUpdate) && Te(V, p, u, c), I && Xe(u, c, p, "beforeUpdate"), p && Qe(p, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    _ && (!c.dynamicChildren || c.dynamicChildren.length !== _.length) && (y = 0, S = !1, _ = null), (T.innerHTML && A.innerHTML == null || T.textContent && A.textContent == null) && a(x, ""), _ ? Ge(
      c.dynamicChildren,
      _,
      x,
      p,
      v,
      As(u, b),
      m
    ) : S || k(
      c,
      u,
      x,
      null,
      p,
      v,
      As(u, b),
      m,
      !1
    ), y > 0) {
      if (y & 16)
        gt(x, T, A, p, b);
      else if (y & 2 && T.class !== A.class && r(x, "class", null, A.class, b), y & 4 && r(x, "style", T.style, A.style, b), y & 8) {
        const N = u.dynamicProps;
        for (let W = 0; W < N.length; W++) {
          const L = N[W], z = T[L], X = A[L];
          (X !== z || L === "value") && r(x, L, z, X, b, p);
        }
      }
      y & 1 && c.children !== u.children && a(x, u.children);
    } else !S && _ == null && gt(x, T, A, p, b);
    ((V = A.onVnodeUpdated) || I) && fe(() => {
      V && Te(V, p, u, c), I && Xe(u, c, p, "updated");
    }, v);
  }, Ge = (c, u, p, v, b, m, S) => {
    for (let x = 0; x < u.length; x++) {
      const y = c[x], _ = u[x], I = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        y.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (y.type === ue || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !yt(y, _) || // - In the case of a component, it could contain anything.
        y.shapeFlag & 198) ? g(y.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          p
        )
      );
      U(
        y,
        _,
        I,
        null,
        v,
        b,
        m,
        S,
        !0
      );
    }
  }, gt = (c, u, p, v, b) => {
    if (u !== p) {
      if (u !== B)
        for (const m in u)
          !Ct(m) && !(m in p) && r(
            c,
            m,
            u[m],
            null,
            b,
            v
          );
      for (const m in p) {
        if (Ct(m)) continue;
        const S = p[m], x = u[m];
        S !== x && m !== "value" && r(c, m, x, S, b, v);
      }
      "value" in p && r(c, "value", u.value, p.value, b);
    }
  }, Lt = (c, u, p, v, b, m, S, x, y) => {
    const _ = u.el = c ? c.el : l(""), I = u.anchor = c ? c.anchor : l("");
    let { patchFlag: T, dynamicChildren: A, slotScopeIds: V } = u;
    V && (x = x ? x.concat(V) : V), c == null ? (n(_, p, v), n(I, p, v), Be(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      p,
      I,
      b,
      m,
      S,
      x,
      y
    )) : T > 0 && T & 64 && A && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    c.dynamicChildren && c.dynamicChildren.length === A.length ? (Ge(
      c.dynamicChildren,
      A,
      p,
      b,
      m,
      S,
      x
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || b && u === b.subTree) && Wi(
      c,
      u,
      !0
      /* shallow */
    )) : k(
      c,
      u,
      p,
      I,
      b,
      m,
      S,
      x,
      y
    );
  }, Kt = (c, u, p, v, b, m, S, x, y) => {
    u.slotScopeIds = x, c == null ? u.shapeFlag & 512 ? b.ctx.activate(
      u,
      p,
      v,
      S,
      y
    ) : ms(
      u,
      p,
      v,
      b,
      m,
      S,
      y
    ) : cn(c, u, y);
  }, ms = (c, u, p, v, b, m, S) => {
    const x = c.component = qo(
      c,
      v,
      b
    );
    if (Ei(c) && (x.ctx.renderer = _t), Go(x, !1, S), x.asyncDep) {
      if (b && b.registerDep(x, se, S), !c.el) {
        const y = x.subTree = $e(Je);
        C(null, y, u, p), c.placeholder = y.el;
      }
    } else
      se(
        x,
        c,
        u,
        p,
        b,
        m,
        S
      );
  }, cn = (c, u, p) => {
    const v = u.component = c.component;
    if (To(c, u, p))
      if (v.asyncDep && !v.asyncResolved) {
        J(v, u, p);
        return;
      } else
        v.next = u, v.update();
    else
      u.el = c.el, v.vnode = u;
  }, se = (c, u, p, v, b, m, S) => {
    const x = () => {
      if (c.isMounted) {
        let { next: T, bu: A, u: V, parent: N, vnode: W } = c;
        {
          const Se = Bi(c);
          if (Se) {
            T && (T.el = W.el, J(c, T, S)), Se.asyncDep.then(() => {
              fe(() => {
                c.isUnmounted || _();
              }, b);
            });
            return;
          }
        }
        let L = T, z;
        Qe(c, !1), T ? (T.el = W.el, J(c, T, S)) : T = W, A && Gt(A), (z = T.props && T.props.onVnodeBeforeUpdate) && Te(z, N, T, W), Qe(c, !0);
        const X = Tn(c), we = c.subTree;
        c.subTree = X, U(
          we,
          X,
          // parent may have changed if it's in a teleport
          g(we.el),
          // anchor may have changed if it's in a fragment
          Bt(we),
          c,
          b,
          m
        ), T.el = X.el, L === null && Eo(c, X.el), V && fe(V, b), (z = T.props && T.props.onVnodeUpdated) && fe(
          () => Te(z, N, T, W),
          b
        );
      } else {
        let T;
        const { el: A, props: V } = u, { bm: N, m: W, parent: L, root: z, type: X } = c, we = At(u);
        Qe(c, !1), N && Gt(N), !we && (T = V && V.onVnodeBeforeMount) && Te(T, L, u), Qe(c, !0);
        {
          z.ce && z.ce._hasShadowRoot() && z.ce._injectChildStyle(
            X,
            c.parent ? c.parent.type : void 0
          );
          const Se = c.subTree = Tn(c);
          U(
            null,
            Se,
            p,
            v,
            c,
            b,
            m
          ), u.el = Se.el;
        }
        if (W && fe(W, b), !we && (T = V && V.onVnodeMounted)) {
          const Se = u;
          fe(
            () => Te(T, L, Se),
            b
          );
        }
        (u.shapeFlag & 256 || L && At(L.vnode) && L.vnode.shapeFlag & 256) && c.a && fe(c.a, b), c.isMounted = !0, u = p = v = null;
      }
    };
    c.scope.on();
    const y = c.effect = new si(x);
    c.scope.off();
    const _ = c.update = y.run.bind(y), I = c.job = y.runIfDirty.bind(y);
    I.i = c, I.id = c.uid, y.scheduler = () => nn(I), Qe(c, !0), _();
  }, J = (c, u, p) => {
    u.component = c;
    const v = c.vnode.props;
    c.vnode = u, c.next = null, Ao(c, u.props, v, p), Ro(c, u.children, p), Fe(), bn(c), Ve();
  }, k = (c, u, p, v, b, m, S, x, y = !1) => {
    const _ = c && c.children, I = c ? c.shapeFlag : 0, T = u.children, { patchFlag: A, shapeFlag: V } = u;
    if (A > 0) {
      if (A & 128) {
        Wt(
          _,
          T,
          p,
          v,
          b,
          m,
          S,
          x,
          y
        );
        return;
      } else if (A & 256) {
        Ye(
          _,
          T,
          p,
          v,
          b,
          m,
          S,
          x,
          y
        );
        return;
      }
    }
    V & 8 ? (I & 16 && mt(_, b, m), T !== _ && a(p, T)) : I & 16 ? V & 16 ? Wt(
      _,
      T,
      p,
      v,
      b,
      m,
      S,
      x,
      y
    ) : mt(_, b, m, !0) : (I & 8 && a(p, ""), V & 16 && Be(
      T,
      p,
      v,
      b,
      m,
      S,
      x,
      y
    ));
  }, Ye = (c, u, p, v, b, m, S, x, y) => {
    c = c || ot, u = u || ot;
    const _ = c.length, I = u.length, T = Math.min(_, I);
    let A;
    for (A = 0; A < T; A++) {
      const V = u[A] = y ? Ne(u[A]) : Pe(u[A]);
      U(
        c[A],
        V,
        p,
        null,
        b,
        m,
        S,
        x,
        y
      );
    }
    _ > I ? mt(
      c,
      b,
      m,
      !0,
      !1,
      T
    ) : Be(
      u,
      p,
      v,
      b,
      m,
      S,
      x,
      y,
      T
    );
  }, Wt = (c, u, p, v, b, m, S, x, y) => {
    let _ = 0;
    const I = u.length;
    let T = c.length - 1, A = I - 1;
    for (; _ <= T && _ <= A; ) {
      const V = c[_], N = u[_] = y ? Ne(u[_]) : Pe(u[_]);
      if (yt(V, N))
        U(
          V,
          N,
          p,
          null,
          b,
          m,
          S,
          x,
          y
        );
      else
        break;
      _++;
    }
    for (; _ <= T && _ <= A; ) {
      const V = c[T], N = u[A] = y ? Ne(u[A]) : Pe(u[A]);
      if (yt(V, N))
        U(
          V,
          N,
          p,
          null,
          b,
          m,
          S,
          x,
          y
        );
      else
        break;
      T--, A--;
    }
    if (_ > T) {
      if (_ <= A) {
        const V = A + 1, N = V < I ? u[V].el : v;
        for (; _ <= A; )
          U(
            null,
            u[_] = y ? Ne(u[_]) : Pe(u[_]),
            p,
            N,
            b,
            m,
            S,
            x,
            y
          ), _++;
      }
    } else if (_ > A)
      for (; _ <= T; )
        xe(c[_], b, m, !0), _++;
    else {
      const V = _, N = _, W = /* @__PURE__ */ new Map();
      for (_ = N; _ <= A; _++) {
        const ae = u[_] = y ? Ne(u[_]) : Pe(u[_]);
        ae.key != null && W.set(ae.key, _);
      }
      let L, z = 0;
      const X = A - N + 1;
      let we = !1, Se = 0;
      const bt = new Array(X);
      for (_ = 0; _ < X; _++) bt[_] = 0;
      for (_ = V; _ <= T; _++) {
        const ae = c[_];
        if (z >= X) {
          xe(ae, b, m, !0);
          continue;
        }
        let Ce;
        if (ae.key != null)
          Ce = W.get(ae.key);
        else
          for (L = N; L <= A; L++)
            if (bt[L - N] === 0 && yt(ae, u[L])) {
              Ce = L;
              break;
            }
        Ce === void 0 ? xe(ae, b, m, !0) : (bt[Ce - N] = _ + 1, Ce >= Se ? Se = Ce : we = !0, U(
          ae,
          u[Ce],
          p,
          null,
          b,
          m,
          S,
          x,
          y
        ), z++);
      }
      const an = we ? Do(bt) : ot;
      for (L = an.length - 1, _ = X - 1; _ >= 0; _--) {
        const ae = N + _, Ce = u[ae], dn = u[ae + 1], pn = ae + 1 < I ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          dn.el || ki(dn)
        ) : v;
        bt[_] === 0 ? U(
          null,
          Ce,
          p,
          pn,
          b,
          m,
          S,
          x,
          y
        ) : we && (L < 0 || _ !== an[L] ? ze(Ce, p, pn, 2) : L--);
      }
    }
  }, ze = (c, u, p, v, b = null) => {
    const { el: m, type: S, transition: x, children: y, shapeFlag: _ } = c;
    if (_ & 6) {
      ze(c.component.subTree, u, p, v);
      return;
    }
    if (_ & 128) {
      c.suspense.move(u, p, v);
      return;
    }
    if (_ & 64) {
      S.move(c, u, p, _t);
      return;
    }
    if (S === ue) {
      n(m, u, p);
      for (let T = 0; T < y.length; T++)
        ze(y[T], u, p, v);
      n(c.anchor, u, p);
      return;
    }
    if (S === Ps) {
      w(c, u, p);
      return;
    }
    if (v !== 2 && _ & 1 && x)
      if (v === 0)
        x.persisted && !m[Ts] ? n(m, u, p) : (x.beforeEnter(m), n(m, u, p), fe(() => x.enter(m), b));
      else {
        const { leave: T, delayLeave: A, afterLeave: V } = x, N = () => {
          c.ctx.isUnmounted ? i(m) : n(m, u, p);
        }, W = () => {
          const L = m._isLeaving || !!m[Ts];
          m._isLeaving && m[Ts](
            !0
            /* cancelled */
          ), x.persisted && !L ? N() : T(m, () => {
            N(), V && V();
          });
        };
        A ? A(m, N, W) : W();
      }
    else
      n(m, u, p);
  }, xe = (c, u, p, v = !1, b = !1) => {
    const {
      type: m,
      props: S,
      ref: x,
      children: y,
      dynamicChildren: _,
      shapeFlag: I,
      patchFlag: T,
      dirs: A,
      cacheIndex: V,
      memo: N
    } = c;
    if (T === -2 && (b = !1), x != null && (Fe(), Ot(x, null, p, c, !0), Ve()), V != null && (u.renderCache[V] = void 0), I & 256) {
      u.ctx.deactivate(c);
      return;
    }
    const W = I & 1 && A, L = !At(c);
    let z;
    if (L && (z = S && S.onVnodeBeforeUnmount) && Te(z, u, c), I & 6)
      tr(c.component, p, v);
    else {
      if (I & 128) {
        c.suspense.unmount(p, v);
        return;
      }
      W && Xe(c, null, u, "beforeUnmount"), I & 64 ? c.type.remove(
        c,
        u,
        p,
        _t,
        v
      ) : _ && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !_.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (m !== ue || T > 0 && T & 64) ? mt(
        _,
        u,
        p,
        !1,
        !0
      ) : (m === ue && T & 384 || !b && I & 16) && mt(y, u, p), v && fn(c);
    }
    const X = N != null && V == null;
    (L && (z = S && S.onVnodeUnmounted) || W || X) && fe(() => {
      z && Te(z, u, c), W && Xe(c, null, u, "unmounted"), X && (c.el = null);
    }, p);
  }, fn = (c) => {
    const { type: u, el: p, anchor: v, transition: b } = c;
    if (u === ue) {
      er(p, v);
      return;
    }
    if (u === Ps) {
      R(c);
      return;
    }
    const m = () => {
      i(p), b && !b.persisted && b.afterLeave && b.afterLeave();
    };
    if (c.shapeFlag & 1 && b && !b.persisted) {
      const { leave: S, delayLeave: x } = b, y = () => S(p, m);
      x ? x(c.el, m, y) : y();
    } else
      m();
  }, er = (c, u) => {
    let p;
    for (; c !== u; )
      p = E(c), i(c), c = p;
    i(u);
  }, tr = (c, u, p) => {
    const { bum: v, scope: b, job: m, subTree: S, um: x, m: y, a: _ } = c;
    An(y), An(_), v && Gt(v), b.stop(), m && (m.flags |= 8, xe(S, c, u, p)), x && fe(x, u), fe(() => {
      c.isUnmounted = !0;
    }, u);
  }, mt = (c, u, p, v = !1, b = !1, m = 0) => {
    for (let S = m; S < c.length; S++)
      xe(c[S], u, p, v, b);
  }, Bt = (c) => {
    if (c.shapeFlag & 6)
      return Bt(c.component.subTree);
    if (c.shapeFlag & 128)
      return c.suspense.next();
    const u = E(c.anchor || c.el), p = u && u[zr];
    return p ? E(p) : u;
  };
  let _s = !1;
  const un = (c, u, p) => {
    let v;
    c == null ? u._vnode && (xe(u._vnode, null, null, !0), v = u._vnode.component) : U(
      u._vnode || null,
      c,
      u,
      null,
      null,
      null,
      p
    ), u._vnode = c, _s || (_s = !0, bn(v), yi(), _s = !1);
  }, _t = {
    p: U,
    um: xe,
    m: ze,
    r: fn,
    mt: ms,
    mc: Be,
    pc: k,
    pbc: Ge,
    n: Bt,
    o: e
  };
  return {
    render: un,
    hydrate: void 0,
    createApp: vo(un)
  };
}
function As({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function Qe({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Uo(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Wi(e, t, s = !1) {
  const n = e.children, i = t.children;
  if (F(n) && F(i))
    for (let r = 0; r < n.length; r++) {
      const o = n[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = Ne(i[r]), l.el = o.el), !s && l.patchFlag !== -2 && Wi(o, l)), l.type === hs && (l.patchFlag === -1 && (l = i[r] = Ne(l)), l.el = o.el), l.type === Je && !l.el && (l.el = o.el);
    }
}
function Do(e) {
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
function Bi(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : Bi(t);
}
function An(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function ki(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? ki(t.subTree) : null;
}
const qi = (e) => e.__isSuspense;
function No(e, t) {
  t && t.pendingBranch ? F(e) ? t.effects.push(...e) : t.effects.push(e) : Br(e);
}
const ue = /* @__PURE__ */ Symbol.for("v-fgt"), hs = /* @__PURE__ */ Symbol.for("v-txt"), Je = /* @__PURE__ */ Symbol.for("v-cmt"), Ps = /* @__PURE__ */ Symbol.for("v-stc"), Mt = [];
let de = null;
function ie(e = !1) {
  Mt.push(de = e ? null : []);
}
function jo() {
  Mt.pop(), de = Mt[Mt.length - 1] || null;
}
let Vt = 1;
function Pn(e, t = !1) {
  Vt += e, e < 0 && de && t && (de.hasOnce = !0);
}
function Ji(e) {
  return e.dynamicChildren = Vt > 0 ? de || ot : null, jo(), Vt > 0 && de && de.push(e), e;
}
function ce(e, t, s, n, i, r) {
  return Ji(
    M(
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
function Ho(e, t, s, n, i) {
  return Ji(
    $e(
      e,
      t,
      s,
      n,
      i,
      !0
    )
  );
}
function Gi(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function yt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const Yi = ({ key: e }) => e ?? null, zt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? Y(e) || /* @__PURE__ */ te(e) || D(e) ? { i: pe, r: e, k: t, f: !!s } : e : null);
function M(e, t = null, s = null, n = 0, i = null, r = e === ue ? 0 : 1, o = !1, l = !1) {
  const f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Yi(t),
    ref: t && zt(t),
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
    ctx: pe
  };
  return l ? (ss(f, s), r & 128 && e.normalize(f)) : s && (f.shapeFlag |= Y(s) ? 8 : 16), Vt > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  de && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (f.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  f.patchFlag !== 32 && de.push(f), f;
}
const $e = $o;
function $o(e, t = null, s = null, n = 0, i = null, r = !1) {
  if ((!e || e === uo) && (e = Je), Gi(e)) {
    const l = at(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ss(l, s), Vt > 0 && !r && de && (l.shapeFlag & 6 ? de[de.indexOf(e)] = l : de.push(l)), l.patchFlag = -2, l;
  }
  if (Qo(e) && (e = e.__vccOpts), t) {
    t = Lo(t);
    let { class: l, style: f } = t;
    l && !Y(l) && (t.class = Gs(l)), K(f) && (/* @__PURE__ */ sn(f) && !F(f) && (f = Z({}, f)), t.style = Js(f));
  }
  const o = Y(e) ? 1 : qi(e) ? 128 : Xr(e) ? 64 : K(e) ? 4 : D(e) ? 2 : 0;
  return M(
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
function Lo(e) {
  return e ? /* @__PURE__ */ sn(e) || Ni(e) ? Z({}, e) : e : null;
}
function at(e, t, s = !1, n = !1) {
  const { props: i, ref: r, patchFlag: o, children: l, transition: f } = e, d = t ? Wo(i || {}, t) : i, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && Yi(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && r ? F(r) ? r.concat(zt(t)) : [r, zt(t)] : zt(t)
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
    patchFlag: t && e.type !== ue ? o === -1 ? 16 : o | 16 : o,
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
    ssContent: e.ssContent && at(e.ssContent),
    ssFallback: e.ssFallback && at(e.ssFallback),
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
function Ko(e = " ", t = 0) {
  return $e(hs, null, e, t);
}
function xt(e = "", t = !1) {
  return t ? (ie(), Ho(Je, null, e)) : $e(Je, null, e);
}
function Pe(e) {
  return e == null || typeof e == "boolean" ? $e(Je) : F(e) ? $e(
    ue,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Gi(e) ? Ne(e) : $e(hs, null, String(e));
}
function Ne(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : at(e);
}
function ss(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (F(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), ss(e, i()), i._c && (i._d = !0));
      return;
    } else {
      s = 32;
      const i = t._;
      !i && !Ni(t) ? t._ctx = pe : i === 3 && pe && (pe.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (D(t)) {
    if (n & 65) {
      ss(e, { default: t });
      return;
    }
    t = { default: t, _ctx: pe }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [Ko(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function Wo(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const i in n)
      if (i === "class")
        t.class !== n.class && (t.class = Gs([t.class, n.class]));
      else if (i === "style")
        t.style = Js([t.style, n.style]);
      else if (is(i)) {
        const r = t[i], o = n[i];
        o && r !== o && !(F(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !rs(i) && (t[i] = o);
      } else i !== "" && (t[i] = n[i]);
  }
  return t;
}
function Te(e, t, s, n = null) {
  be(e, t, 7, [
    s,
    n
  ]);
}
const Bo = Ri();
let ko = 0;
function qo(e, t, s) {
  const n = e.type, i = (t ? t.appContext : e.appContext) || Bo, r = {
    uid: ko++,
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
    scope: new pr(
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
    propsOptions: Hi(n, i),
    emitsOptions: Fi(n, i),
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
  return r.ctx = { _: r }, r.root = t ? t.root : r, r.emit = xo.bind(null, r), e.ce && e.ce(r), r;
}
let oe = null;
const Jo = () => oe || pe;
let ns, Ks;
{
  const e = cs(), t = (s, n) => {
    let i;
    return (i = e[s]) || (i = e[s] = []), i.push(n), (r) => {
      i.length > 1 ? i.forEach((o) => o(r)) : i[0](r);
    };
  };
  ns = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => oe = s
  ), Ks = t(
    "__VUE_SSR_SETTERS__",
    (s) => Ut = s
  );
}
const Ht = (e) => {
  const t = oe;
  return ns(e), e.scope.on(), () => {
    e.scope.off(), ns(t);
  };
}, Mn = () => {
  oe && oe.scope.off(), ns(null);
};
function zi(e) {
  return e.vnode.shapeFlag & 4;
}
let Ut = !1;
function Go(e, t = !1, s = !1) {
  t && Ks(t);
  const { props: n, children: i } = e.vnode, r = zi(e);
  Oo(e, n, r, t), Io(e, i, s || t);
  const o = r ? Yo(e, t) : void 0;
  return t && Ks(!1), o;
}
function Yo(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, ao);
  const { setup: n } = s;
  if (n) {
    Fe();
    const i = e.setupContext = n.length > 1 ? Xo(e) : null, r = Ht(e), o = jt(
      n,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = Gn(o);
    if (Ve(), r(), (l || e.sp) && !At(e) && Ti(e), l) {
      if (o.then(Mn, Mn), t)
        return o.then((f) => {
          In(e, f);
        }).catch((f) => {
          as(f, e, 0);
        });
      e.asyncDep = o;
    } else
      In(e, o);
  } else
    Xi(e);
}
function In(e, t, s) {
  D(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : K(t) && (e.setupState = mi(t)), Xi(e);
}
function Xi(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Ie);
  {
    const i = Ht(e);
    Fe();
    try {
      po(e);
    } finally {
      Ve(), i();
    }
  }
}
const zo = {
  get(e, t) {
    return ee(e, "get", ""), e[t];
  }
};
function Xo(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, zo),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function gs(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(mi(Fr(e.exposed)), {
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
function Qo(e) {
  return D(e) && "__vccOpts" in e;
}
const Zo = (e, t) => /* @__PURE__ */ Hr(e, t, Ut), el = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Ws;
const Rn = typeof window < "u" && window.trustedTypes;
if (Rn)
  try {
    Ws = /* @__PURE__ */ Rn.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Qi = Ws ? (e) => Ws.createHTML(e) : (e) => e, tl = "http://www.w3.org/2000/svg", sl = "http://www.w3.org/1998/Math/MathML", De = typeof document < "u" ? document : null, Fn = De && /* @__PURE__ */ De.createElement("template"), nl = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const i = t === "svg" ? De.createElementNS(tl, e) : t === "mathml" ? De.createElementNS(sl, e) : s ? De.createElement(e, { is: s }) : De.createElement(e);
    return e === "select" && n && n.multiple != null && i.setAttribute("multiple", n.multiple), i;
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
  insertStaticContent(e, t, s, n, i, r) {
    const o = s ? s.previousSibling : t.lastChild;
    if (i && (i === r || i.nextSibling))
      for (; t.insertBefore(i.cloneNode(!0), s), !(i === r || !(i = i.nextSibling)); )
        ;
    else {
      Fn.innerHTML = Qi(
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
}, il = /* @__PURE__ */ Symbol("_vtc");
function rl(e, t, s) {
  const n = e[il];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const Vn = /* @__PURE__ */ Symbol("_vod"), ol = /* @__PURE__ */ Symbol("_vsh"), ll = /* @__PURE__ */ Symbol(""), cl = /(?:^|;)\s*display\s*:/;
function fl(e, t, s) {
  const n = e.style, i = Y(s);
  let r = !1;
  if (s && !i) {
    if (t)
      if (Y(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && St(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && St(n, o, "");
    for (const o in s) {
      o === "display" && (r = !0);
      const l = s[o];
      l != null ? al(
        e,
        o,
        !Y(t) && t ? t[o] : void 0,
        l
      ) || St(n, o, l) : St(n, o, "");
    }
  } else if (i) {
    if (t !== s) {
      const o = n[ll];
      o && (s += ";" + o), n.cssText = s, r = cl.test(s);
    }
  } else t && e.removeAttribute("style");
  Vn in e && (e[Vn] = r ? n.display : "", e[ol] && (n.display = "none"));
}
const Un = /\s*!important$/;
function St(e, t, s) {
  if (F(s))
    s.forEach((n) => St(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = ul(e, t);
    Un.test(s) ? e.setProperty(
      st(n),
      s.replace(Un, ""),
      "important"
    ) : e[n] = s;
  }
}
const Dn = ["Webkit", "Moz", "ms"], Ms = {};
function ul(e, t) {
  const s = Ms[t];
  if (s)
    return s;
  let n = ge(t);
  if (n !== "filter" && n in e)
    return Ms[t] = n;
  n = Xn(n);
  for (let i = 0; i < Dn.length; i++) {
    const r = Dn[i] + n;
    if (r in e)
      return Ms[t] = r;
  }
  return t;
}
function al(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && Y(n) && s === n;
}
const Nn = "http://www.w3.org/1999/xlink";
function jn(e, t, s, n, i, r = ar(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Nn, t.slice(6, t.length)) : e.setAttributeNS(Nn, t, s) : s == null || r && !Zn(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : Re(s) ? String(s) : s
  );
}
function Hn(e, t, s, n, i) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? Qi(s) : s);
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
    l === "boolean" ? s = Zn(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(i || t);
}
function qe(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function dl(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const $n = /* @__PURE__ */ Symbol("_vei");
function pl(e, t, s, n, i = null) {
  const r = e[$n] || (e[$n] = {}), o = r[t];
  if (n && o)
    o.value = n;
  else {
    const [l, f] = ml(t);
    if (n) {
      const d = r[t] = vl(
        n,
        i
      );
      qe(e, l, d, f);
    } else o && (dl(e, l, o, f), r[t] = void 0);
  }
}
const hl = /(Once|Passive|Capture)$/, gl = /^on:?(?:Once|Passive|Capture)$/;
function ml(e) {
  let t, s;
  for (; (s = e.match(hl)) && !gl.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : st(e.slice(2)), t];
}
let Is = 0;
const _l = /* @__PURE__ */ Promise.resolve(), bl = () => Is || (_l.then(() => Is = 0), Is = Date.now());
function vl(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const i = s.value;
    if (F(i)) {
      const r = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        r.call(n), n._stopped = !0;
      };
      const o = i.slice(), l = [n];
      for (let f = 0; f < o.length && !n._stopped; f++) {
        const d = o[f];
        d && be(
          d,
          t,
          5,
          l
        );
      }
    } else
      be(
        i,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = bl(), s;
}
const Ln = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, yl = (e, t, s, n, i, r) => {
  const o = i === "svg";
  t === "class" ? rl(e, n, o) : t === "style" ? fl(e, s, n) : is(t) ? rs(t) || pl(e, t, s, n, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : xl(e, t, n, o)) ? (Hn(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && jn(e, t, n, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (wl(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !Y(n))) ? Hn(e, ge(t), n, r, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), jn(e, t, n, o));
};
function xl(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Ln(t) && D(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return Ln(t) && Y(s) ? !1 : t in e;
}
function wl(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = ge(t);
  return Array.isArray(s) ? s.some((i) => ge(i) === n) : Object.keys(s).some((i) => ge(i) === n);
}
const dt = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return F(t) ? (s) => Gt(t, s) : t;
};
function Sl(e) {
  e.target.composing = !0;
}
function Kn(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const Le = /* @__PURE__ */ Symbol("_assign");
function Wn(e, t, s) {
  return t && (e = e.trim()), s && (e = ls(e)), e;
}
const Ee = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, i) {
    e[Le] = dt(i);
    const r = n || i.props && i.props.type === "number";
    qe(e, t ? "change" : "input", (o) => {
      o.target.composing || e[Le](Wn(e.value, s, r));
    }), (s || r) && qe(e, "change", () => {
      e.value = Wn(e.value, s, r);
    }), t || (qe(e, "compositionstart", Sl), qe(e, "compositionend", Kn), qe(e, "change", Kn));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: i, number: r } }, o) {
    if (e[Le] = dt(o), e.composing) return;
    const l = (r || e.type === "number") && !/^0\d/.test(e.value) ? ls(e.value) : e.value, f = t ?? "";
    if (l === f)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (n && t === s || i && e.value.trim() === f) || (e.value = f);
  }
}, Cl = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, s) {
    e[Le] = dt(s), qe(e, "change", () => {
      const n = e._modelValue, i = Dt(e), r = e.checked, o = e[Le];
      if (F(n)) {
        const l = Ys(n, i), f = l !== -1;
        if (r && !f)
          o(n.concat(i));
        else if (!r && f) {
          const d = [...n];
          d.splice(l, 1), o(d);
        }
      } else if (pt(n)) {
        const l = new Set(n);
        r ? l.add(i) : l.delete(i), o(l);
      } else
        o(Zi(e, r));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: Bn,
  beforeUpdate(e, t, s) {
    e[Le] = dt(s), Bn(e, t, s);
  }
};
function Bn(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let i;
  if (F(t))
    i = Ys(t, n.props.value) > -1;
  else if (pt(t))
    i = t.has(n.props.value);
  else {
    if (t === s) return;
    i = ht(t, Zi(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const Rs = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const i = pt(t);
    qe(e, "change", () => {
      const r = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? ls(Dt(o)) : Dt(o)
      );
      e[Le](
        e.multiple ? i ? new Set(r) : r : r[0]
      ), e._assigning = !0, bi(() => {
        e._assigning = !1;
      });
    }), e[Le] = dt(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    kn(e, t);
  },
  beforeUpdate(e, t, s) {
    e[Le] = dt(s);
  },
  updated(e, { value: t }) {
    e._assigning || kn(e, t);
  }
};
function kn(e, t) {
  const s = e.multiple, n = F(t);
  if (!(s && !n && !pt(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = Dt(o);
      if (s)
        if (n) {
          const f = typeof l;
          f === "string" || f === "number" ? o.selected = t.some((d) => String(d) === String(l)) : o.selected = Ys(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (ht(Dt(o), t)) {
        e.selectedIndex !== i && (e.selectedIndex = i);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function Dt(e) {
  return "_value" in e ? e._value : e.value;
}
function Zi(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
}
const Tl = /* @__PURE__ */ Z({ patchProp: yl }, nl);
let qn;
function El() {
  return qn || (qn = Fo(Tl));
}
const Ol = ((...e) => {
  const t = El().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const i = Pl(n);
    if (!i) return;
    const r = t._component;
    !D(r) && !r.render && !r.template && (r.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
    const o = s(i, !1, Al(i));
    return i instanceof Element && (i.removeAttribute("v-cloak"), i.setAttribute("data-v-app", "")), o;
  }, t;
});
function Al(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function Pl(e) {
  return Y(e) ? document.querySelector(e) : e;
}
const Ml = { class: "sql-set" }, Il = { class: "row" }, Rl = ["value"], Fl = {
  key: 0,
  class: "muted empty"
}, Vl = { class: "row spread" }, Ul = { class: "row" }, Dl = ["onUpdate:modelValue"], Nl = ["onUpdate:modelValue"], jl = ["onClick"], Hl = {
  key: 0,
  class: "row"
}, $l = ["onUpdate:modelValue"], Ll = ["onUpdate:modelValue"], Kl = {
  key: 1,
  class: "row"
}, Wl = ["onUpdate:modelValue"], Bl = { class: "row" }, kl = {
  key: 0,
  class: "chk"
}, ql = ["onUpdate:modelValue"], Jl = { class: "row" }, Gl = ["onUpdate:modelValue"], Yl = ["value"], zl = ["onClick"], Xl = {
  key: 0,
  class: "row new-cred"
}, Ql = ["onUpdate:modelValue"], Zl = ["onUpdate:modelValue"], ec = ["onUpdate:modelValue"], tc = ["disabled", "onClick"], sc = { class: "muted" }, nc = { class: "row" }, ic = ["onUpdate:modelValue", "placeholder"], rc = { class: "row" }, oc = ["onUpdate:modelValue"], lc = { class: "row" }, cc = ["disabled", "onClick"], fc = { class: "muted" }, uc = /* @__PURE__ */ Qr({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function i(C, h) {
      return {
        key: n++,
        name: C,
        provider: h.provider || "mssql",
        path: h.provider === "sqlite" ? h.file || "" : h.server || "",
        database: h.database || "",
        user: h.user || "",
        credential: h.credential || "",
        trustedConnection: h.trusted_connection ?? !0,
        description: h.description || "",
        newCred: !1,
        credName: "",
        credUser: "",
        credPassword: "",
        credStatus: "",
        testing: !1,
        testStatus: ""
      };
    }
    function r(C) {
      return {
        provider: C.provider,
        server: C.provider === "sqlite" ? void 0 : C.path || void 0,
        file: C.provider === "sqlite" && C.path || void 0,
        database: C.database || void 0,
        user: C.user || void 0,
        credential: C.credential || void 0,
        trusted_connection: C.provider === "mssql" ? C.trustedConnection : void 0,
        description: C.description || void 0
        // NOTE: no `password` — literals are written to the secret store via secret.set, never here.
      };
    }
    const o = (() => {
      try {
        return JSON.parse(s.api.getJson() || "null") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ Ss(o.default_connection || ""), f = /* @__PURE__ */ Ss(o.default_limit || 10), d = /* @__PURE__ */ us(
      Object.entries(o.connections || {}).map(([C, h]) => i(C, h))
    ), a = /* @__PURE__ */ Ss([]);
    async function g() {
      try {
        const C = await s.api.invoke("secret.list");
        a.value = [...new Set([...C.machine || [], ...C.project || []].map((h) => h.key))].sort();
      } catch {
      }
    }
    Ai(g);
    function E() {
      d.push(i(`db${d.length + 1}`, { provider: "mssql" }));
    }
    async function O(C) {
      C.credStatus = "Saving…";
      try {
        const h = { password: C.credPassword };
        C.credUser && (h.user = C.credUser), await s.api.invoke("secret.set", { key: C.credName.trim(), fields: h, scope: "machine" }), C.credential = C.credName.trim(), C.newCred = !1, C.credName = "", C.credUser = "", C.credPassword = "", C.credStatus = "", await g();
      } catch (h) {
        C.credStatus = "Failed: " + (h instanceof Error ? h.message : String(h));
      }
    }
    async function j(C) {
      C.testing = !0, C.testStatus = "Connecting...";
      try {
        const h = await s.api.invoke("plugin.action", {
          pluginId: "sql",
          action: "testConnection",
          valueJson: JSON.stringify(U(C))
        });
        if (h.ok && h.resultJson) {
          const w = JSON.parse(h.resultJson);
          C.testStatus = w.message;
        } else
          C.testStatus = "Failed: " + (h.error || "unknown error");
      } catch (h) {
        C.testStatus = "Failed: " + (h instanceof Error ? h.message : String(h));
      } finally {
        C.testing = !1;
      }
    }
    function U(C) {
      const h = r(C);
      return {
        provider: h.provider,
        server: h.server,
        database: h.database,
        user: h.user,
        credential: h.credential,
        trustedConnection: h.trusted_connection,
        file: h.file,
        description: h.description
      };
    }
    function G() {
      const C = {
        default_connection: l.value || void 0,
        default_limit: f.value || 10,
        connections: Object.fromEntries(
          d.filter((h) => h.name.trim()).map((h) => [h.name.trim(), r(h)])
        )
      };
      return JSON.stringify(C);
    }
    return t({ toJson: G }), (C, h) => (ie(), ce("div", Ml, [
      h[17] || (h[17] = M("div", { class: "muted" }, " Named database connections available to the SQL agent. Passwords live in the secret store (Settings → Secrets); a connection only references an entry by name. Stored in the .spla project file. ", -1)),
      M("div", Il, [
        M("label", null, [
          h[3] || (h[3] = M("span", { class: "muted" }, "Default connection", -1)),
          le(M("select", {
            "onUpdate:modelValue": h[0] || (h[0] = (w) => l.value = w)
          }, [
            h[2] || (h[2] = M("option", { value: "" }, "(none)", -1)),
            (ie(!0), ce(ue, null, Es(d, (w) => (ie(), ce("option", {
              key: w.key,
              value: w.name
            }, it(w.name), 9, Rl))), 128))
          ], 512), [
            [Rs, l.value]
          ])
        ]),
        M("label", null, [
          h[4] || (h[4] = M("span", { class: "muted" }, "Default row limit", -1)),
          le(M("input", {
            "onUpdate:modelValue": h[1] || (h[1] = (w) => f.value = w),
            type: "number",
            min: "1",
            class: "w-90"
          }, null, 512), [
            [
              Ee,
              f.value,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      M("button", {
        type: "button",
        class: "self-start",
        onClick: E
      }, "+ Add Connection"),
      d.length ? xt("", !0) : (ie(), ce("div", Fl, 'No connections yet. Click "+ Add Connection".')),
      (ie(!0), ce(ue, null, Es(d, (w, R) => (ie(), ce("div", {
        key: w.key,
        class: "conn-card"
      }, [
        M("div", Vl, [
          M("div", Ul, [
            h[6] || (h[6] = M("span", { class: "muted" }, "Name", -1)),
            le(M("input", {
              "onUpdate:modelValue": (P) => w.name = P,
              class: "w-140",
              spellcheck: "false"
            }, null, 8, Dl), [
              [Ee, w.name]
            ]),
            h[7] || (h[7] = M("span", { class: "muted" }, "Provider", -1)),
            le(M("select", {
              "onUpdate:modelValue": (P) => w.provider = P
            }, [...h[5] || (h[5] = [
              M("option", { value: "mssql" }, "mssql", -1),
              M("option", { value: "postgres" }, "postgres", -1),
              M("option", { value: "sqlite" }, "sqlite", -1)
            ])], 8, Nl), [
              [Rs, w.provider]
            ])
          ]),
          M("button", {
            type: "button",
            onClick: (P) => d.splice(R, 1)
          }, "✕ Remove", 8, jl)
        ]),
        w.provider !== "sqlite" ? (ie(), ce("div", Hl, [
          h[8] || (h[8] = M("span", { class: "muted w-70" }, "Server", -1)),
          le(M("input", {
            "onUpdate:modelValue": (P) => w.path = P,
            placeholder: "sql01 or 192.168.1.10",
            class: "w-220",
            spellcheck: "false"
          }, null, 8, $l), [
            [Ee, w.path]
          ]),
          h[9] || (h[9] = M("span", { class: "muted w-70" }, "Database", -1)),
          le(M("input", {
            "onUpdate:modelValue": (P) => w.database = P,
            class: "w-160",
            spellcheck: "false"
          }, null, 8, Ll), [
            [Ee, w.database]
          ])
        ])) : (ie(), ce("div", Kl, [
          h[10] || (h[10] = M("span", { class: "muted w-70" }, "File", -1)),
          le(M("input", {
            "onUpdate:modelValue": (P) => w.path = P,
            placeholder: "C:\\data\\mydb.sqlite",
            class: "w-400",
            spellcheck: "false"
          }, null, 8, Wl), [
            [Ee, w.path]
          ])
        ])),
        w.provider !== "sqlite" ? (ie(), ce(ue, { key: 2 }, [
          M("div", Bl, [
            w.provider === "mssql" ? (ie(), ce("label", kl, [
              le(M("input", {
                type: "checkbox",
                "onUpdate:modelValue": (P) => w.trustedConnection = P
              }, null, 8, ql), [
                [Cl, w.trustedConnection]
              ]),
              h[11] || (h[11] = M("span", null, "Windows Auth (domain)", -1))
            ])) : xt("", !0)
          ]),
          !w.trustedConnection || w.provider !== "mssql" ? (ie(), ce(ue, { key: 0 }, [
            M("div", Jl, [
              h[13] || (h[13] = M("span", { class: "muted w-70" }, "Credential", -1)),
              le(M("select", {
                "onUpdate:modelValue": (P) => w.credential = P
              }, [
                h[12] || (h[12] = M("option", { value: "" }, "(none — use fields below)", -1)),
                (ie(!0), ce(ue, null, Es(a.value, (P) => (ie(), ce("option", {
                  key: P,
                  value: P
                }, it(P), 9, Yl))), 128))
              ], 8, Gl), [
                [Rs, w.credential]
              ]),
              M("button", {
                type: "button",
                onClick: (P) => w.newCred = !w.newCred
              }, it(w.newCred ? "cancel" : "new…"), 9, zl),
              h[14] || (h[14] = M("span", { class: "muted" }, "entry in the secret store: user + password", -1))
            ]),
            w.newCred ? (ie(), ce("div", Xl, [
              le(M("input", {
                "onUpdate:modelValue": (P) => w.credName = P,
                placeholder: "entry name",
                class: "w-140",
                spellcheck: "false"
              }, null, 8, Ql), [
                [Ee, w.credName]
              ]),
              le(M("input", {
                "onUpdate:modelValue": (P) => w.credUser = P,
                placeholder: "user",
                class: "w-120",
                spellcheck: "false"
              }, null, 8, Zl), [
                [Ee, w.credUser]
              ]),
              le(M("input", {
                "onUpdate:modelValue": (P) => w.credPassword = P,
                type: "password",
                placeholder: "password",
                class: "w-140",
                autocomplete: "new-password"
              }, null, 8, ec), [
                [Ee, w.credPassword]
              ]),
              M("button", {
                type: "button",
                disabled: !w.credName || !w.credPassword,
                onClick: (P) => O(w)
              }, "Save to store", 8, tc),
              M("span", sc, it(w.credStatus), 1)
            ])) : xt("", !0),
            M("div", nc, [
              h[15] || (h[15] = M("span", { class: "muted w-70" }, "User", -1)),
              le(M("input", {
                "onUpdate:modelValue": (P) => w.user = P,
                placeholder: w.credential ? "(from credential)" : "login",
                class: "w-130",
                spellcheck: "false"
              }, null, 8, ic), [
                [Ee, w.user]
              ])
            ])
          ], 64)) : xt("", !0)
        ], 64)) : xt("", !0),
        M("div", rc, [
          h[16] || (h[16] = M("span", { class: "muted w-70" }, "Description", -1)),
          le(M("input", {
            "onUpdate:modelValue": (P) => w.description = P,
            placeholder: "Shown to the AI — what this database contains",
            class: "grow"
          }, null, 8, oc), [
            [Ee, w.description]
          ])
        ]),
        M("div", lc, [
          M("button", {
            type: "button",
            disabled: w.testing,
            onClick: (P) => j(w)
          }, "Test Connection", 8, cc),
          M("span", fc, it(w.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
}), ac = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, i] of t)
    s[n] = i;
  return s;
}, dc = /* @__PURE__ */ ac(uc, [["__scopeId", "data-v-32eded4c"]]);
function hc(e, t) {
  let s = Ol(dc, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toJson(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  hc as mount
};
