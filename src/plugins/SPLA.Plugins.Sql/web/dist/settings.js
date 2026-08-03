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
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), os = (e) => e.startsWith("onUpdate:"), Z = Object.assign, ks = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, si = Object.prototype.hasOwnProperty, H = (e, t) => si.call(e, t), F = Array.isArray, lt = (e) => Nt(e) === "[object Map]", pt = (e) => Nt(e) === "[object Set]", hn = (e) => Nt(e) === "[object Date]", D = (e) => typeof e == "function", Y = (e) => typeof e == "string", Re = (e) => typeof e == "symbol", K = (e) => e !== null && typeof e == "object", Gn = (e) => (K(e) || D(e)) && D(e.then) && D(e.catch), Yn = Object.prototype.toString, Nt = (e) => Yn.call(e), ni = (e) => Nt(e).slice(8, -1), zn = (e) => Nt(e) === "[object Object]", qs = (e) => Y(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, Ct = /* @__PURE__ */ Bs(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), ls = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, ri = /-\w/g, ge = ls(
  (e) => e.replace(ri, (t) => t.slice(1).toUpperCase())
), ii = /\B([A-Z])/g, st = ls(
  (e) => e.replace(ii, "-$1").toLowerCase()
), Xn = ls((e) => e.charAt(0).toUpperCase() + e.slice(1)), vs = ls(
  (e) => e ? `on${Xn(e)}` : ""
), Me = (e, t) => !Object.is(e, t), Yt = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Qn = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, cs = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let gn;
const fs = () => gn || (gn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function Js(e) {
  if (F(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], r = Y(n) ? fi(n) : Js(n);
      if (r)
        for (const i in r)
          t[i] = r[i];
    }
    return t;
  } else if (Y(e) || K(e))
    return e;
}
const oi = /;(?![^(]*\))/g, li = /:([^]+)/, ci = /\/\*[^]*?\*\//g;
function fi(e) {
  const t = {};
  return e.replace(ci, "").split(oi).forEach((s) => {
    if (s) {
      const n = s.split(li);
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
const ui = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", ai = /* @__PURE__ */ Bs(ui);
function Zn(e) {
  return !!e || e === "";
}
function di(e, t) {
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
    return s && n ? di(e, t) : !1;
  if (s = K(e), n = K(t), s || n) {
    if (!s || !n)
      return !1;
    const r = Object.keys(e).length, i = Object.keys(t).length;
    if (r !== i)
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
const er = (e) => !!(e && e.__v_isRef === !0), rt = (e) => Y(e) ? e : e == null ? "" : F(e) || K(e) && (e.toString === Yn || !D(e.toString)) ? er(e) ? rt(e.value) : JSON.stringify(e, tr, 2) : String(e), tr = (e, t) => er(t) ? tr(e, t.value) : lt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, r], i) => (s[ys(n, i) + " =>"] = r, s),
    {}
  )
} : pt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => ys(s))
} : Re(t) ? ys(t) : K(t) && !F(t) && !zn(t) ? String(t) : t, ys = (e, t = "") => {
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
function hi() {
  return Q;
}
let q;
const xs = /* @__PURE__ */ new WeakSet();
class sr {
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
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || rr(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, mn(this), ir(this);
    const t = q, s = me;
    q = this, me = !0;
    try {
      return this.fn();
    } finally {
      or(this), q = t, me = s, this.flags &= -3;
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
    this.flags & 64 ? xs.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
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
let nr = 0, Tt, Et;
function rr(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Et, Et = e;
    return;
  }
  e.next = Tt, Tt = e;
}
function zs() {
  nr++;
}
function Xs() {
  if (--nr > 0)
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
function ir(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function or(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const r = n.prevDep;
    n.version === -1 ? (n === s && (s = r), Qs(n), gi(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = r;
  }
  e.deps = t, e.depsTail = s;
}
function Fs(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (lr(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function lr(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === It) || (e.globalVersion = It, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Fs(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = q, n = me;
  q = e, me = !0;
  try {
    ir(e);
    const r = e.fn(e._value);
    (t.version === 0 || Me(r, e._value)) && (e.flags |= 128, e._value = r, t.version++);
  } catch (r) {
    throw t.version++, r;
  } finally {
    q = s, me = n, or(e), e.flags &= -3;
  }
}
function Qs(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: r } = e;
  if (n && (n.nextSub = r, e.prevSub = void 0), r && (r.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let i = s.computed.deps; i; i = i.nextDep)
      Qs(i, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function gi(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let me = !0;
const cr = [];
function Fe() {
  cr.push(me), me = !1;
}
function Ve() {
  const e = cr.pop();
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
    if (!q || !me || q === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== q)
      s = this.activeLink = new mi(q, this), q.deps ? (s.prevDep = q.depsTail, q.depsTail.nextDep = s, q.depsTail = s) : q.deps = q.depsTail = s, fr(s);
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
    let r = n.get(s);
    r || (n.set(s, r = new Zs()), r.map = n, r.key = s), r.track();
  }
}
function je(e, t, s, n, r, i) {
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
  const t = /* @__PURE__ */ $(e);
  return t === e ? t : (ee(t, "iterate", Rt), /* @__PURE__ */ he(e) ? t : t.map(_e));
}
function us(e) {
  return ee(e = /* @__PURE__ */ $(e), "iterate", Rt), e;
}
function Ae(e, t) {
  return /* @__PURE__ */ Ke(e) ? ut(/* @__PURE__ */ tt(e) ? _e(t) : t) : _e(t);
}
const _i = {
  __proto__: null,
  [Symbol.iterator]() {
    return Ss(this, Symbol.iterator, (e) => Ae(this, e));
  },
  concat(...e) {
    return nt(this).concat(
      ...e.map((t) => F(t) ? nt(t) : t)
    );
  },
  entries() {
    return Ss(this, "entries", (e) => (e[1] = Ae(this, e[1]), e));
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
    return Ss(this, "values", (e) => Ae(this, e));
  }
};
function Ss(e, t, s) {
  const n = us(e), r = n[t]();
  return n !== e && !/* @__PURE__ */ he(e) && (r._next = r.next, r.next = () => {
    const i = r._next();
    return i.done || (i.value = s(i.value)), i;
  }), r;
}
const bi = Array.prototype;
function Ue(e, t, s, n, r, i) {
  const o = us(e), l = o !== e && !/* @__PURE__ */ he(e), f = o[t];
  if (f !== bi[t]) {
    const g = f.apply(e, i);
    return l ? _e(g) : g;
  }
  let d = s;
  o !== e && (l ? d = function(g, E) {
    return s.call(this, Ae(e, g), E, e);
  } : s.length > 2 && (d = function(g, E) {
    return s.call(this, g, E, e);
  }));
  const a = f.call(o, d, n);
  return l && r ? r(a) : a;
}
function _n(e, t, s, n) {
  const r = us(e), i = r !== e && !/* @__PURE__ */ he(e);
  let o = s, l = !1;
  r !== e && (i ? (l = n.length === 0, o = function(d, a, g) {
    return l && (l = !1, d = Ae(e, d)), s.call(this, d, Ae(e, a), g, e);
  }) : s.length > 3 && (o = function(d, a, g) {
    return s.call(this, d, a, g, e);
  }));
  const f = r[t](o, ...n);
  return l ? Ae(e, f) : f;
}
function ws(e, t, s) {
  const n = /* @__PURE__ */ $(e);
  ee(n, "iterate", Rt);
  const r = n[t](...s);
  return (r === -1 || r === !1) && /* @__PURE__ */ sn(s[0]) ? (s[0] = /* @__PURE__ */ $(s[0]), n[t](...s)) : r;
}
function vt(e, t, s = []) {
  Fe(), zs();
  const n = (/* @__PURE__ */ $(e))[t].apply(e, s);
  return Xs(), Ve(), n;
}
const vi = /* @__PURE__ */ Bs("__proto__,__v_isRef,__isVue"), ur = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Re)
);
function yi(e) {
  Re(e) || (e = String(e));
  const t = /* @__PURE__ */ $(this);
  return ee(t, "has", e), t.hasOwnProperty(e);
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
      return n === (r ? i ? Mi : gr : i ? hr : pr).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = F(t);
    if (!r) {
      let f;
      if (o && (f = _i[s]))
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
      /* @__PURE__ */ te(t) ? t : n
    );
    if ((Re(s) ? ur.has(s) : vi(s)) || (r || ee(t, "get", s), i))
      return l;
    if (/* @__PURE__ */ te(l)) {
      const f = o && qs(s) ? l : l.value;
      return r && K(f) ? /* @__PURE__ */ Ns(f) : f;
    }
    return K(l) ? r ? /* @__PURE__ */ Ns(l) : /* @__PURE__ */ as(l) : l;
  }
}
class dr extends ar {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, r) {
    let i = t[s];
    const o = F(t) && qs(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ Ke(i);
      if (!/* @__PURE__ */ he(n) && !/* @__PURE__ */ Ke(n) && (i = /* @__PURE__ */ $(i), n = /* @__PURE__ */ $(n)), !o && /* @__PURE__ */ te(i) && !/* @__PURE__ */ te(n))
        return d || (i.value = n), !0;
    }
    const l = o ? Number(s) < t.length : H(t, s), f = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ te(t) ? t : r
    );
    return t === /* @__PURE__ */ $(r) && f && (l ? Me(n, i) && je(t, "set", s, n) : je(t, "add", s, n)), f;
  }
  deleteProperty(t, s) {
    const n = H(t, s);
    t[s];
    const r = Reflect.deleteProperty(t, s);
    return r && n && je(t, "delete", s, void 0), r;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Re(s) || !ur.has(s)) && ee(t, "has", s), n;
  }
  ownKeys(t) {
    return ee(
      t,
      "iterate",
      F(t) ? "length" : et
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
const Si = /* @__PURE__ */ new dr(), wi = /* @__PURE__ */ new xi(), Ci = /* @__PURE__ */ new dr(!0);
const Ds = (e) => e, kt = (e) => Reflect.getPrototypeOf(e);
function Ti(e, t, s) {
  return function(...n) {
    const r = this.__v_raw, i = /* @__PURE__ */ $(r), o = lt(i), l = e === "entries" || e === Symbol.iterator && o, f = e === "keys" && o, d = r[e](...n), a = s ? Ds : t ? ut : _e;
    return !t && ee(
      i,
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
function Ei(e, t) {
  const s = {
    get(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ $(i), l = /* @__PURE__ */ $(r);
      e || (Me(r, l) && ee(o, "get", r), ee(o, "get", l));
      const { has: f } = kt(o), d = t ? Ds : e ? ut : _e;
      if (f.call(o, r))
        return d(i.get(r));
      if (f.call(o, l))
        return d(i.get(l));
      i !== o && i.get(r);
    },
    get size() {
      const r = this.__v_raw;
      return !e && ee(/* @__PURE__ */ $(r), "iterate", et), r.size;
    },
    has(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ $(i), l = /* @__PURE__ */ $(r);
      return e || (Me(r, l) && ee(o, "has", r), ee(o, "has", l)), r === l ? i.has(r) : i.has(r) || i.has(l);
    },
    forEach(r, i) {
      const o = this, l = o.__v_raw, f = /* @__PURE__ */ $(l), d = t ? Ds : e ? ut : _e;
      return !e && ee(f, "iterate", et), l.forEach((a, g) => r.call(i, d(a), d(g), o));
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
      add(r) {
        const i = /* @__PURE__ */ $(this), o = kt(i), l = /* @__PURE__ */ $(r), f = !t && !/* @__PURE__ */ he(r) && !/* @__PURE__ */ Ke(r) ? l : r;
        return o.has.call(i, f) || Me(r, f) && o.has.call(i, r) || Me(l, f) && o.has.call(i, l) || (i.add(f), je(i, "add", f, f)), this;
      },
      set(r, i) {
        !t && !/* @__PURE__ */ he(i) && !/* @__PURE__ */ Ke(i) && (i = /* @__PURE__ */ $(i));
        const o = /* @__PURE__ */ $(this), { has: l, get: f } = kt(o);
        let d = l.call(o, r);
        d || (r = /* @__PURE__ */ $(r), d = l.call(o, r));
        const a = f.call(o, r);
        return o.set(r, i), d ? Me(i, a) && je(o, "set", r, i) : je(o, "add", r, i), this;
      },
      delete(r) {
        const i = /* @__PURE__ */ $(this), { has: o, get: l } = kt(i);
        let f = o.call(i, r);
        f || (r = /* @__PURE__ */ $(r), f = o.call(i, r)), l && l.call(i, r);
        const d = i.delete(r);
        return f && je(i, "delete", r, void 0), d;
      },
      clear() {
        const r = /* @__PURE__ */ $(this), i = r.size !== 0, o = r.clear();
        return i && je(
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
    s[r] = Ti(r, e, t);
  }), s;
}
function en(e, t) {
  const s = Ei(e, t);
  return (n, r, i) => r === "__v_isReactive" ? !e : r === "__v_isReadonly" ? e : r === "__v_raw" ? n : Reflect.get(
    H(s, r) && r in n ? s : n,
    r,
    i
  );
}
const Oi = {
  get: /* @__PURE__ */ en(!1, !1)
}, Ai = {
  get: /* @__PURE__ */ en(!1, !0)
}, Pi = {
  get: /* @__PURE__ */ en(!0, !1)
};
const pr = /* @__PURE__ */ new WeakMap(), hr = /* @__PURE__ */ new WeakMap(), gr = /* @__PURE__ */ new WeakMap(), Mi = /* @__PURE__ */ new WeakMap();
function Ii(e) {
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
function as(e) {
  return /* @__PURE__ */ Ke(e) ? e : tn(
    e,
    !1,
    Si,
    Oi,
    pr
  );
}
// @__NO_SIDE_EFFECTS__
function Ri(e) {
  return tn(
    e,
    !1,
    Ci,
    Ai,
    hr
  );
}
// @__NO_SIDE_EFFECTS__
function Ns(e) {
  return tn(
    e,
    !0,
    wi,
    Pi,
    gr
  );
}
function tn(e, t, s, n, r) {
  if (!K(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const i = r.get(e);
  if (i)
    return i;
  const o = Ii(ni(e));
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
function $(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ $(t) : e;
}
function Fi(e) {
  return !H(e, "__v_skip") && Object.isExtensible(e) && Qn(e, "__v_skip", !0), e;
}
const _e = (e) => K(e) ? /* @__PURE__ */ as(e) : e, ut = (e) => K(e) ? /* @__PURE__ */ Ns(e) : e;
// @__NO_SIDE_EFFECTS__
function te(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Cs(e) {
  return Vi(e, !1);
}
function Vi(e, t) {
  return /* @__PURE__ */ te(e) ? e : new Ui(e, t);
}
class Ui {
  constructor(t, s) {
    this.dep = new Zs(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ $(t), this._value = s ? t : _e(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ he(t) || /* @__PURE__ */ Ke(t);
    t = n ? t : /* @__PURE__ */ $(t), Me(t, s) && (this._rawValue = t, this._value = n ? t : _e(t), this.dep.trigger());
  }
}
function Di(e) {
  return /* @__PURE__ */ te(e) ? e.value : e;
}
const Ni = {
  get: (e, t, s) => t === "__v_raw" ? e : Di(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const r = e[t];
    return /* @__PURE__ */ te(r) && !/* @__PURE__ */ te(s) ? (r.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function mr(e) {
  return /* @__PURE__ */ tt(e) ? e : new Proxy(e, Ni);
}
class ji {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Zs(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = It - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
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
function $i(e, t, s = !1) {
  let n, r;
  return D(e) ? n = e : (n = e.get, r = e.set), new ji(n, r, s);
}
const Jt = {}, Qt = /* @__PURE__ */ new WeakMap();
let Ze;
function Hi(e, t = !1, s = Ze) {
  if (s) {
    let n = Qt.get(s);
    n || Qt.set(s, n = []), n.push(e);
  }
}
function Li(e, t, s = B) {
  const { immediate: n, deep: r, once: i, scheduler: o, augmentJob: l, call: f } = s, d = (R) => r ? R : /* @__PURE__ */ he(R) || r === !1 || r === 0 ? $e(R, 1) : $e(R);
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
  } : g = Ie, t && r) {
    const R = g, P = r === !0 ? 1 / 0 : r;
    g = () => $e(R(), P);
  }
  const G = hi(), w = () => {
    a.stop(), G && G.active && ks(G.effects, a);
  };
  if (i && t) {
    const R = t;
    t = (...P) => {
      const ve = R(...P);
      return w(), ve;
    };
  }
  let h = U ? new Array(e.length).fill(Jt) : Jt;
  const y = (R) => {
    if (!(!(a.flags & 1) || !a.dirty && !R))
      if (t) {
        const P = a.run();
        if (R || r || j || (U ? P.some((ve, ye) => Me(ve, h[ye])) : Me(P, h))) {
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
  return l && l(y), a = new sr(g), a.scheduler = o ? () => o(y, !1) : y, O = (R) => Hi(R, !1, a), E = a.onStop = () => {
    const R = Qt.get(a);
    if (R) {
      if (f)
        f(R, 4);
      else
        for (const P of R) P();
      Qt.delete(a);
    }
  }, t ? n ? y(!0) : h = a.run() : o ? o(y.bind(null, !0), !0) : a.run(), w.pause = a.pause.bind(a), w.resume = a.resume.bind(a), w.stop = w, w;
}
function $e(e, t = 1 / 0, s) {
  if (t <= 0 || !K(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ te(e))
    $e(e.value, t, s);
  else if (F(e))
    for (let n = 0; n < e.length; n++)
      $e(e[n], t, s);
  else if (pt(e) || lt(e))
    e.forEach((n) => {
      $e(n, t, s);
    });
  else if (zn(e)) {
    for (const n in e)
      $e(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && $e(e[n], t, s);
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
  } catch (r) {
    ds(r, t, s);
  }
}
function be(e, t, s, n) {
  if (D(e)) {
    const r = jt(e, t, s, n);
    return r && Gn(r) && r.catch((i) => {
      ds(i, t, s);
    }), r;
  }
  if (F(e)) {
    const r = [];
    for (let i = 0; i < e.length; i++)
      r.push(be(e[i], t, s, n));
    return r;
  }
}
function ds(e, t, s, n = !0) {
  const r = t ? t.vnode : null, { errorHandler: i, throwUnhandledErrorInProduction: o } = t && t.appContext.config || B;
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
    if (i) {
      Fe(), jt(i, null, 10, [
        e,
        f,
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
const oe = [];
let Oe = -1;
const ct = [];
let ke = null, it = 0;
const _r = /* @__PURE__ */ Promise.resolve();
let Zt = null;
function br(e) {
  const t = Zt || _r;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Wi(e) {
  let t = Oe + 1, s = oe.length;
  for (; t < s; ) {
    const n = t + s >>> 1, r = oe[n], i = Ft(r);
    i < e || i === e && r.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function nn(e) {
  if (!(e.flags & 1)) {
    const t = Ft(e), s = oe[oe.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Ft(s) ? oe.push(e) : oe.splice(Wi(t), 0, e), e.flags |= 1, vr();
  }
}
function vr() {
  Zt || (Zt = _r.then(xr));
}
function Bi(e) {
  F(e) ? ct.push(...e) : ke && e.id === -1 ? ke.splice(it + 1, 0, e) : e.flags & 1 || (ct.push(e), e.flags |= 1), vr();
}
function bn(e, t, s = Oe + 1) {
  for (; s < oe.length; s++) {
    const n = oe[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      oe.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function yr(e) {
  if (ct.length) {
    const t = [...new Set(ct)].sort(
      (s, n) => Ft(s) - Ft(n)
    );
    if (ct.length = 0, ke) {
      ke.push(...t);
      return;
    }
    for (ke = t, it = 0; it < ke.length; it++) {
      const s = ke[it];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    ke = null, it = 0;
  }
}
const Ft = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function xr(e) {
  try {
    for (Oe = 0; Oe < oe.length; Oe++) {
      const t = oe[Oe];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), jt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Oe < oe.length; Oe++) {
      const t = oe[Oe];
      t && (t.flags &= -2);
    }
    Oe = -1, oe.length = 0, yr(), Zt = null, (oe.length || ct.length) && xr();
  }
}
let pe = null, Sr = null;
function es(e) {
  const t = pe;
  return pe = e, Sr = e && e.type.__scopeId || null, t;
}
function ki(e, t = pe, s) {
  if (!t || e._n)
    return e;
  const n = (...r) => {
    n._d && Pn(-1);
    const i = es(t);
    let o;
    try {
      o = e(...r);
    } finally {
      es(i), n._d && Pn(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function ne(e, t) {
  if (pe === null)
    return e;
  const s = ms(pe), n = e.dirs || (e.dirs = []);
  for (let r = 0; r < t.length; r++) {
    let [i, o, l, f = B] = t[r];
    i && (D(i) && (i = {
      mounted: i,
      updated: i
    }), i.deep && $e(o), n.push({
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
function Xe(e, t, s, n) {
  const r = e.dirs, i = t && t.dirs;
  for (let o = 0; o < r.length; o++) {
    const l = r[o];
    i && (l.oldValue = i[o].value);
    let f = l.dir[n];
    f && (Fe(), be(f, s, 8, [
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
  const n = Jo();
  if (n || ft) {
    let r = ft ? ft._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (r && e in r)
      return r[e];
    if (arguments.length > 1)
      return s && D(t) ? t.call(n && n.proxy) : t;
  }
}
const Ji = /* @__PURE__ */ Symbol.for("v-scx"), Gi = () => zt(Ji);
function Ts(e, t, s) {
  return wr(e, t, s);
}
function wr(e, t, s = B) {
  const { immediate: n, deep: r, flush: i, once: o } = s, l = Z({}, s), f = t && n || !t && i !== "post";
  let d;
  if (Ut) {
    if (i === "sync") {
      const O = Gi();
      d = O.__watcherHandles || (O.__watcherHandles = []);
    } else if (!f) {
      const O = () => {
      };
      return O.stop = Ie, O.resume = Ie, O.pause = Ie, O;
    }
  }
  const a = le;
  l.call = (O, j, U) => be(O, a, j, U);
  let g = !1;
  i === "post" ? l.scheduler = (O) => {
    fe(O, a && a.suspense);
  } : i !== "sync" && (g = !0, l.scheduler = (O, j) => {
    j ? O() : nn(O);
  }), l.augmentJob = (O) => {
    t && (O.flags |= 4), g && (O.flags |= 2, a && (O.id = a.uid, O.i = a));
  };
  const E = Li(e, t, l);
  return Ut && (d ? d.push(E) : f && E()), E;
}
function Yi(e, t, s) {
  const n = this.proxy, r = Y(e) ? e.includes(".") ? Cr(n, e) : () => n[e] : e.bind(n, n);
  let i;
  D(t) ? i = t : (i = t.handler, s = t);
  const o = $t(this), l = wr(r, i.bind(n), s);
  return o(), l;
}
function Cr(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let r = 0; r < s.length && n; r++)
      n = n[s[r]];
    return n;
  };
}
const zi = /* @__PURE__ */ Symbol("_vte"), Xi = (e) => e.__isTeleport, Es = /* @__PURE__ */ Symbol("_leaveCb");
function rn(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, rn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Qi(e, t) {
  return D(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    Z({ name: e.name }, t, { setup: e })
  ) : e;
}
function Tr(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function vn(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const ts = /* @__PURE__ */ new WeakMap();
function Ot(e, t, s, n, r = !1) {
  if (F(e)) {
    e.forEach(
      (U, G) => Ot(
        U,
        t && (F(t) ? t[G] : t),
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
  const i = n.shapeFlag & 4 ? ms(n.component) : n.el, o = r ? null : i, { i: l, r: f } = e, d = t && t.r, a = l.refs === B ? l.refs = {} : l.refs, g = l.setupState, E = /* @__PURE__ */ $(g), O = g === B ? Jn : (U) => vn(a, U) ? !1 : H(E, U), j = (U, G) => !(G && vn(a, G));
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
      const w = () => {
        if (e.f) {
          const h = U ? O(f) ? g[f] : a[f] : j() || !e.k ? f.value : a[e.k];
          if (r)
            F(h) && ks(h, i);
          else if (F(h))
            h.includes(i) || h.push(i);
          else if (U)
            a[f] = [i], O(f) && (g[f] = a[f]);
          else {
            const y = [i];
            j(f, e.k) && (f.value = y), e.k && (a[e.k] = y);
          }
        } else U ? (a[f] = o, O(f) && (g[f] = o)) : G && (j(f, e.k) && (f.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const h = () => {
          w(), ts.delete(e);
        };
        h.id = -1, ts.set(e, h), fe(h, s);
      } else
        yn(e), w();
    }
  }
}
function yn(e) {
  const t = ts.get(e);
  t && (t.flags |= 8, ts.delete(e));
}
fs().requestIdleCallback;
fs().cancelIdleCallback;
const At = (e) => !!e.type.__asyncLoader, Er = (e) => e.type.__isKeepAlive;
function Zi(e, t) {
  Or(e, "a", t);
}
function eo(e, t) {
  Or(e, "da", t);
}
function Or(e, t, s = le) {
  const n = e.__wdc || (e.__wdc = () => {
    let r = s;
    for (; r; ) {
      if (r.isDeactivated)
        return;
      r = r.parent;
    }
    return e();
  });
  if (ps(t, n, s), s) {
    let r = s.parent;
    for (; r && r.parent; )
      Er(r.parent.vnode) && to(n, t, s, r), r = r.parent;
  }
}
function to(e, t, s, n) {
  const r = ps(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Pr(() => {
    ks(n[t], r);
  }, s);
}
function ps(e, t, s = le, n = !1) {
  if (s) {
    const r = s[e] || (s[e] = []), i = t.__weh || (t.__weh = (...o) => {
      Fe();
      const l = $t(s), f = be(t, s, e, o);
      return l(), Ve(), f;
    });
    return n ? r.unshift(i) : r.push(i), i;
  }
}
const We = (e) => (t, s = le) => {
  (!Ut || e === "sp") && ps(e, (...n) => t(...n), s);
}, so = We("bm"), Ar = We("m"), no = We(
  "bu"
), ro = We("u"), io = We(
  "bum"
), Pr = We("um"), oo = We(
  "sp"
), lo = We("rtg"), co = We("rtc");
function fo(e, t = le) {
  ps("ec", e, t);
}
const uo = /* @__PURE__ */ Symbol.for("v-ndc");
function Os(e, t, s, n) {
  let r;
  const i = s, o = F(e);
  if (o || Y(e)) {
    const l = o && /* @__PURE__ */ tt(e);
    let f = !1, d = !1;
    l && (f = !/* @__PURE__ */ he(e), d = /* @__PURE__ */ Ke(e), e = us(e)), r = new Array(e.length);
    for (let a = 0, g = e.length; a < g; a++)
      r[a] = t(
        f ? d ? ut(_e(e[a])) : _e(e[a]) : e[a],
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
const js = (e) => e ? zr(e) ? ms(e) : js(e.parent) : null, Pt = (
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
    $options: (e) => Ir(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      nn(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = br.bind(e.proxy)),
    $watch: (e) => Yi.bind(e)
  })
), As = (e, t) => e !== B && !e.__isScriptSetup && H(e, t), ao = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: r, props: i, accessCache: o, type: l, appContext: f } = e;
    if (t[0] !== "$") {
      const E = o[t];
      if (E !== void 0)
        switch (E) {
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
        if (As(n, t))
          return o[t] = 1, n[t];
        if (r !== B && H(r, t))
          return o[t] = 2, r[t];
        if (H(i, t))
          return o[t] = 3, i[t];
        if (s !== B && H(s, t))
          return o[t] = 4, s[t];
        $s && (o[t] = 0);
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
    if (s !== B && H(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      g = f.config.globalProperties, H(g, t)
    )
      return g[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: r, ctx: i } = e;
    return As(r, t) ? (r[t] = s, !0) : n !== B && H(n, t) ? (n[t] = s, !0) : H(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (i[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: r, props: i, type: o }
  }, l) {
    let f;
    return !!(s[l] || e !== B && l[0] !== "$" && H(e, l) || As(t, l) || H(i, l) || H(n, l) || H(Pt, l) || H(r.config.globalProperties, l) || (f = o.__cssModules) && f[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : H(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function xn(e) {
  return F(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let $s = !0;
function po(e) {
  const t = Ir(e), s = e.proxy, n = e.ctx;
  $s = !1, t.beforeCreate && Sn(t.beforeCreate, e, "bc");
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
    beforeMount: g,
    mounted: E,
    beforeUpdate: O,
    updated: j,
    activated: U,
    deactivated: G,
    beforeDestroy: w,
    beforeUnmount: h,
    destroyed: y,
    unmounted: R,
    render: P,
    renderTracked: ve,
    renderTriggered: ye,
    errorCaptured: Be,
    serverPrefetch: Ht,
    // public API
    expose: Ge,
    inheritAttrs: gt,
    // assets
    components: Lt,
    directives: Kt,
    filters: _s
  } = t;
  if (d && ho(d, n, null), o)
    for (const J in o) {
      const k = o[J];
      D(k) && (n[J] = k.bind(s));
    }
  if (r) {
    const J = r.call(s, s);
    K(J) && (e.data = /* @__PURE__ */ as(J));
  }
  if ($s = !0, i)
    for (const J in i) {
      const k = i[J], Ye = D(k) ? k.bind(s, s) : D(k.get) ? k.get.bind(s, s) : Ie, Wt = !D(k) && D(k.set) ? k.set.bind(s) : Ie, ze = Zo({
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
      Mr(l[J], n, s, J);
  if (f) {
    const J = D(f) ? f.call(s) : f;
    Reflect.ownKeys(J).forEach((k) => {
      qi(k, J[k]);
    });
  }
  a && Sn(a, e, "c");
  function se(J, k) {
    F(k) ? k.forEach((Ye) => J(Ye.bind(s))) : k && J(k.bind(s));
  }
  if (se(so, g), se(Ar, E), se(no, O), se(ro, j), se(Zi, U), se(eo, G), se(fo, Be), se(co, ve), se(lo, ye), se(io, h), se(Pr, R), se(oo, Ht), F(Ge))
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
  P && e.render === Ie && (e.render = P), gt != null && (e.inheritAttrs = gt), Lt && (e.components = Lt), Kt && (e.directives = Kt), Ht && Tr(e);
}
function ho(e, t, s = Ie) {
  F(e) && (e = Hs(e));
  for (const n in e) {
    const r = e[n];
    let i;
    K(r) ? "default" in r ? i = zt(
      r.from || n,
      r.default,
      !0
    ) : i = zt(r.from || n) : i = zt(r), /* @__PURE__ */ te(i) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => i.value,
      set: (o) => i.value = o
    }) : t[n] = i;
  }
}
function Sn(e, t, s) {
  be(
    F(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Mr(e, t, s, n) {
  let r = n.includes(".") ? Cr(s, n) : () => s[n];
  if (Y(e)) {
    const i = t[e];
    D(i) && Ts(r, i);
  } else if (D(e))
    Ts(r, e.bind(s));
  else if (K(e))
    if (F(e))
      e.forEach((i) => Mr(i, t, s, n));
    else {
      const i = D(e.handler) ? e.handler.bind(s) : t[e.handler];
      D(i) && Ts(r, i, e);
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
      const l = go[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const go = {
  data: wn,
  props: Cn,
  emits: Cn,
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
  watch: _o,
  // provide / inject
  provide: wn,
  inject: mo
};
function wn(e, t) {
  return t ? e ? function() {
    return Z(
      D(e) ? e.call(this, this) : e,
      D(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function mo(e, t) {
  return St(Hs(e), Hs(t));
}
function Hs(e) {
  if (F(e)) {
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
    s[n] = re(e[n], t[n]);
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
let bo = 0;
function vo(e, t) {
  return function(n, r = null) {
    D(n) || (n = Z({}, n)), r != null && !K(r) && (r = null);
    const i = Rr(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let f = !1;
    const d = i.app = {
      _uid: bo++,
      _component: n,
      _props: r,
      _container: null,
      _context: i,
      _instance: null,
      version: el,
      get config() {
        return i.config;
      },
      set config(a) {
      },
      use(a, ...g) {
        return o.has(a) || (a && D(a.install) ? (o.add(a), a.install(d, ...g)) : D(a) && (o.add(a), a(d, ...g))), d;
      },
      mixin(a) {
        return i.mixins.includes(a) || i.mixins.push(a), d;
      },
      component(a, g) {
        return g ? (i.components[a] = g, d) : i.components[a];
      },
      directive(a, g) {
        return g ? (i.directives[a] = g, d) : i.directives[a];
      },
      mount(a, g, E) {
        if (!f) {
          const O = d._ceVNode || He(n, r);
          return O.appContext = i, E === !0 ? E = "svg" : E === !1 && (E = void 0), e(O, a, E), f = !0, d._container = a, a.__vue_app__ = d, ms(O.component);
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
        return i.provides[a] = g, d;
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
  let r = s;
  const i = t.startsWith("update:"), o = i && yo(n, t.slice(7));
  o && (o.trim && (r = s.map((a) => Y(a) ? a.trim() : a)), o.number && (r = s.map(cs)));
  let l, f = n[l = vs(t)] || // also try camelCase event handler (#2249)
  n[l = vs(ge(t))];
  !f && i && (f = n[l = vs(st(t))]), f && be(
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
    e.emitted[l] = !0, be(
      d,
      e,
      6,
      r
    );
  }
}
const So = /* @__PURE__ */ new WeakMap();
function Fr(e, t, s = !1) {
  const n = s ? So : t.emitsCache, r = n.get(e);
  if (r !== void 0)
    return r;
  const i = e.emits;
  let o = {}, l = !1;
  if (!D(e)) {
    const f = (d) => {
      const a = Fr(d, t, !0);
      a && (l = !0, Z(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  return !i && !l ? (K(e) && n.set(e, null), null) : (F(i) ? i.forEach((f) => o[f] = null) : Z(o, i), K(e) && n.set(e, o), o);
}
function hs(e, t) {
  return !e || !is(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), H(e, t[0].toLowerCase() + t.slice(1)) || H(e, st(t)) || H(e, t));
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
    props: g,
    data: E,
    setupState: O,
    ctx: j,
    inheritAttrs: U
  } = e, G = es(e);
  let w, h;
  try {
    if (s.shapeFlag & 4) {
      const R = r || n, P = R;
      w = Pe(
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
      w = Pe(
        R.length > 1 ? R(
          g,
          { attrs: l, slots: o, emit: f }
        ) : R(
          g,
          null
        )
      ), h = t.props ? l : wo(l);
    }
  } catch (R) {
    Mt.length = 0, ds(R, e, 1), w = He(Je);
  }
  let y = w;
  if (h && U !== !1) {
    const R = Object.keys(h), { shapeFlag: P } = y;
    R.length && P & 7 && (i && R.some(os) && (h = Co(
      h,
      i
    )), y = at(y, h, !1, !0));
  }
  return s.dirs && (y = at(y, null, !1, !0), y.dirs = y.dirs ? y.dirs.concat(s.dirs) : s.dirs), s.transition && rn(y, s.transition), w = y, es(G), w;
}
const wo = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || is(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, Co = (e, t) => {
  const s = {};
  for (const n in e)
    (!os(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function To(e, t, s) {
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
      for (let g = 0; g < a.length; g++) {
        const E = a[g];
        if (Vr(o, n, E) && !hs(d, E))
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
    if (Vr(t, e, i) && !hs(s, i))
      return !0;
  }
  return !1;
}
function Vr(e, t, s) {
  const n = e[s], r = t[s];
  return s === "style" && K(n) && K(r) ? !ht(n, r) : n !== r;
}
function Eo({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const r = t.subTree;
    if (r.suspense && r.suspense.activeBranch === e && (r.suspense.vnode.el = r.el = n, e = r), r === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const Ur = {}, Dr = () => Object.create(Ur), Nr = (e) => Object.getPrototypeOf(e) === Ur;
function Oo(e, t, s, n = !1) {
  const r = {}, i = Dr();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), jr(e, t, r, i);
  for (const o in e.propsOptions[0])
    o in r || (r[o] = void 0);
  s ? e.props = n ? r : /* @__PURE__ */ Ri(r) : e.type.props ? e.props = r : e.props = i, e.attrs = i;
}
function Ao(e, t, s, n) {
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
      for (let g = 0; g < a.length; g++) {
        let E = a[g];
        if (hs(e.emitsOptions, E))
          continue;
        const O = t[E];
        if (f)
          if (H(i, E))
            O !== i[E] && (i[E] = O, d = !0);
          else {
            const j = ge(E);
            r[j] = Ls(
              f,
              l,
              j,
              O,
              e,
              !1
            );
          }
        else
          O !== i[E] && (i[E] = O, d = !0);
      }
    }
  } else {
    jr(e, t, r, i) && (d = !0);
    let a;
    for (const g in l)
      (!t || // for camelCase
      !H(t, g) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = st(g)) === g || !H(t, a))) && (f ? s && // for camelCase
      (s[g] !== void 0 || // for kebab-case
      s[a] !== void 0) && (r[g] = Ls(
        f,
        l,
        g,
        void 0,
        e,
        !0
      )) : delete r[g]);
    if (i !== l)
      for (const g in i)
        (!t || !H(t, g)) && (delete i[g], d = !0);
  }
  d && je(e.attrs, "set", "");
}
function jr(e, t, s, n) {
  const [r, i] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let f in t) {
      if (Ct(f))
        continue;
      const d = t[f];
      let a;
      r && H(r, a = ge(f)) ? !i || !i.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : hs(e.emitsOptions, f) || (!(f in n) || d !== n[f]) && (n[f] = d, o = !0);
    }
  if (i) {
    const f = /* @__PURE__ */ $(s), d = l || B;
    for (let a = 0; a < i.length; a++) {
      const g = i[a];
      s[g] = Ls(
        r,
        f,
        g,
        d[g],
        e,
        !H(d, g)
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
      const f = o.default;
      if (o.type !== Function && !o.skipFactory && D(f)) {
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
    ] && (n === "" || n === st(s)) && (n = !0));
  }
  return n;
}
const Po = /* @__PURE__ */ new WeakMap();
function $r(e, t, s = !1) {
  const n = s ? Po : t.propsCache, r = n.get(e);
  if (r)
    return r;
  const i = e.props, o = {}, l = [];
  let f = !1;
  if (!D(e)) {
    const a = (g) => {
      f = !0;
      const [E, O] = $r(g, t, !0);
      Z(o, E), O && l.push(...O);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!i && !f)
    return K(e) && n.set(e, ot), ot;
  if (F(i))
    for (let a = 0; a < i.length; a++) {
      const g = ge(i[a]);
      On(g) && (o[g] = B);
    }
  else if (i)
    for (const a in i) {
      const g = ge(a);
      if (On(g)) {
        const E = i[a], O = o[g] = F(E) || D(E) ? { type: E } : Z({}, E), j = O.type;
        let U = !1, G = !0;
        if (F(j))
          for (let w = 0; w < j.length; ++w) {
            const h = j[w], y = D(h) && h.name;
            if (y === "Boolean") {
              U = !0;
              break;
            } else y === "String" && (G = !1);
          }
        else
          U = D(j) && j.name === "Boolean";
        O[
          0
          /* shouldCast */
        ] = U, O[
          1
          /* shouldCastTrue */
        ] = G, (U || H(O, "default")) && l.push(g);
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
  const n = ki((...r) => ln(t(...r)), s);
  return n._c = !1, n;
}, Hr = (e, t, s) => {
  const n = e._ctx;
  for (const r in e) {
    if (on(r)) continue;
    const i = e[r];
    if (D(i))
      t[r] = Mo(r, i, n);
    else if (i != null) {
      const o = ln(i);
      t[r] = () => o;
    }
  }
}, Lr = (e, t) => {
  const s = ln(t);
  e.slots.default = () => s;
}, Kr = (e, t, s) => {
  for (const n in t)
    (s || !on(n)) && (e[n] = t[n]);
}, Io = (e, t, s) => {
  const n = e.slots = Dr();
  if (e.vnode.shapeFlag & 32) {
    const r = t._;
    r ? (Kr(n, t, s), s && Qn(n, "_", r, !0)) : Hr(t, n);
  } else t && Lr(e, t);
}, Ro = (e, t, s) => {
  const { vnode: n, slots: r } = e;
  let i = !0, o = B;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? i = !1 : Kr(r, t, s) : (i = !t.$stable, Hr(t, r)), o = t;
  } else t && (Lr(e, t), o = { default: 1 });
  if (i)
    for (const l in r)
      !on(l) && o[l] == null && delete r[l];
}, fe = No;
function Fo(e) {
  return Vo(e);
}
function Vo(e, t) {
  const s = fs();
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
    parentNode: g,
    nextSibling: E,
    setScopeId: O = Ie,
    insertStaticContent: j
  } = e, U = (c, u, p, v = null, b = null, m = null, C = void 0, S = null, x = !!u.dynamicChildren) => {
    if (c === u)
      return;
    c && !yt(c, u) && (v = Bt(c), xe(c, b, m, !0), c = null), u.patchFlag === -2 && (x = !1, u.dynamicChildren = null);
    const { type: _, ref: I, shapeFlag: T } = u;
    switch (_) {
      case gs:
        G(c, u, p, v);
        break;
      case Je:
        w(c, u, p, v);
        break;
      case Ms:
        c == null && h(u, p, v, C);
        break;
      case ue:
        Lt(
          c,
          u,
          p,
          v,
          b,
          m,
          C,
          S,
          x
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
          C,
          S,
          x
        ) : T & 6 ? Kt(
          c,
          u,
          p,
          v,
          b,
          m,
          C,
          S,
          x
        ) : (T & 64 || T & 128) && _.process(
          c,
          u,
          p,
          v,
          b,
          m,
          C,
          S,
          x,
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
  }, w = (c, u, p, v) => {
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
  }, y = ({ el: c, anchor: u }, p, v) => {
    let b;
    for (; c && c !== u; )
      b = E(c), n(c, p, v), c = b;
    n(u, p, v);
  }, R = ({ el: c, anchor: u }) => {
    let p;
    for (; c && c !== u; )
      p = E(c), r(c), c = p;
    r(u);
  }, P = (c, u, p, v, b, m, C, S, x) => {
    if (u.type === "svg" ? C = "svg" : u.type === "math" && (C = "mathml"), c == null)
      ve(
        u,
        p,
        v,
        b,
        m,
        C,
        S,
        x
      );
    else {
      const _ = c.el && c.el._isVueCE ? c.el : null;
      try {
        _ && _._beginPatch(), Ht(
          c,
          u,
          b,
          m,
          C,
          S,
          x
        );
      } finally {
        _ && _._endPatch();
      }
    }
  }, ve = (c, u, p, v, b, m, C, S) => {
    let x, _;
    const { props: I, shapeFlag: T, transition: M, dirs: V } = c;
    if (x = c.el = o(
      c.type,
      m,
      I && I.is,
      I
    ), T & 8 ? a(x, c.children) : T & 16 && Be(
      c.children,
      x,
      null,
      v,
      b,
      Ps(c, m),
      C,
      S
    ), V && Xe(c, null, v, "created"), ye(x, c, c.scopeId, C, v), I) {
      for (const W in I)
        W !== "value" && !Ct(W) && i(x, W, null, I[W], m, v);
      "value" in I && i(x, "value", null, I.value, m), (_ = I.onVnodeBeforeMount) && Te(_, v, c);
    }
    V && Xe(c, null, v, "beforeMount");
    const N = Uo(b, M);
    N && M.beforeEnter(x), n(x, u, p), ((_ = I && I.onVnodeMounted) || N || V) && fe(() => {
      try {
        _ && Te(_, v, c), N && M.enter(x), V && Xe(c, null, v, "mounted");
      } finally {
      }
    }, b);
  }, ye = (c, u, p, v, b) => {
    if (p && O(c, p), v)
      for (let m = 0; m < v.length; m++)
        O(c, v[m]);
    if (b) {
      let m = b.subTree;
      if (u === m || qr(m.type) && (m.ssContent === u || m.ssFallback === u)) {
        const C = b.vnode;
        ye(
          c,
          C,
          C.scopeId,
          C.slotScopeIds,
          b.parent
        );
      }
    }
  }, Be = (c, u, p, v, b, m, C, S, x = 0) => {
    for (let _ = x; _ < c.length; _++) {
      const I = c[_] = S ? Ne(c[_]) : Pe(c[_]);
      U(
        null,
        I,
        u,
        p,
        v,
        b,
        m,
        C,
        S
      );
    }
  }, Ht = (c, u, p, v, b, m, C) => {
    const S = u.el = c.el;
    let { patchFlag: x, dynamicChildren: _, dirs: I } = u;
    x |= c.patchFlag & 16;
    const T = c.props || B, M = u.props || B;
    let V;
    if (p && Qe(p, !1), (V = M.onVnodeBeforeUpdate) && Te(V, p, u, c), I && Xe(u, c, p, "beforeUpdate"), p && Qe(p, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    _ && (!c.dynamicChildren || c.dynamicChildren.length !== _.length) && (x = 0, C = !1, _ = null), (T.innerHTML && M.innerHTML == null || T.textContent && M.textContent == null) && a(S, ""), _ ? Ge(
      c.dynamicChildren,
      _,
      S,
      p,
      v,
      Ps(u, b),
      m
    ) : C || k(
      c,
      u,
      S,
      null,
      p,
      v,
      Ps(u, b),
      m,
      !1
    ), x > 0) {
      if (x & 16)
        gt(S, T, M, p, b);
      else if (x & 2 && T.class !== M.class && i(S, "class", null, M.class, b), x & 4 && i(S, "style", T.style, M.style, b), x & 8) {
        const N = u.dynamicProps;
        for (let W = 0; W < N.length; W++) {
          const L = N[W], z = T[L], X = M[L];
          (X !== z || L === "value") && i(S, L, z, X, b, p);
        }
      }
      x & 1 && c.children !== u.children && a(S, u.children);
    } else !C && _ == null && gt(S, T, M, p, b);
    ((V = M.onVnodeUpdated) || I) && fe(() => {
      V && Te(V, p, u, c), I && Xe(u, c, p, "updated");
    }, v);
  }, Ge = (c, u, p, v, b, m, C) => {
    for (let S = 0; S < u.length; S++) {
      const x = c[S], _ = u[S], I = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        x.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (x.type === ue || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !yt(x, _) || // - In the case of a component, it could contain anything.
        x.shapeFlag & 198) ? g(x.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          p
        )
      );
      U(
        x,
        _,
        I,
        null,
        v,
        b,
        m,
        C,
        !0
      );
    }
  }, gt = (c, u, p, v, b) => {
    if (u !== p) {
      if (u !== B)
        for (const m in u)
          !Ct(m) && !(m in p) && i(
            c,
            m,
            u[m],
            null,
            b,
            v
          );
      for (const m in p) {
        if (Ct(m)) continue;
        const C = p[m], S = u[m];
        C !== S && m !== "value" && i(c, m, S, C, b, v);
      }
      "value" in p && i(c, "value", u.value, p.value, b);
    }
  }, Lt = (c, u, p, v, b, m, C, S, x) => {
    const _ = u.el = c ? c.el : l(""), I = u.anchor = c ? c.anchor : l("");
    let { patchFlag: T, dynamicChildren: M, slotScopeIds: V } = u;
    V && (S = S ? S.concat(V) : V), c == null ? (n(_, p, v), n(I, p, v), Be(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      p,
      I,
      b,
      m,
      C,
      S,
      x
    )) : T > 0 && T & 64 && M && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    c.dynamicChildren && c.dynamicChildren.length === M.length ? (Ge(
      c.dynamicChildren,
      M,
      p,
      b,
      m,
      C,
      S
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || b && u === b.subTree) && Wr(
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
      C,
      S,
      x
    );
  }, Kt = (c, u, p, v, b, m, C, S, x) => {
    u.slotScopeIds = S, c == null ? u.shapeFlag & 512 ? b.ctx.activate(
      u,
      p,
      v,
      C,
      x
    ) : _s(
      u,
      p,
      v,
      b,
      m,
      C,
      x
    ) : cn(c, u, x);
  }, _s = (c, u, p, v, b, m, C) => {
    const S = c.component = qo(
      c,
      v,
      b
    );
    if (Er(c) && (S.ctx.renderer = _t), Go(S, !1, C), S.asyncDep) {
      if (b && b.registerDep(S, se, C), !c.el) {
        const x = S.subTree = He(Je);
        w(null, x, u, p), c.placeholder = x.el;
      }
    } else
      se(
        S,
        c,
        u,
        p,
        b,
        m,
        C
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
  }, se = (c, u, p, v, b, m, C) => {
    const S = () => {
      if (c.isMounted) {
        let { next: T, bu: M, u: V, parent: N, vnode: W } = c;
        {
          const we = Br(c);
          if (we) {
            T && (T.el = W.el, J(c, T, C)), we.asyncDep.then(() => {
              fe(() => {
                c.isUnmounted || _();
              }, b);
            });
            return;
          }
        }
        let L = T, z;
        Qe(c, !1), T ? (T.el = W.el, J(c, T, C)) : T = W, M && Yt(M), (z = T.props && T.props.onVnodeBeforeUpdate) && Te(z, N, T, W), Qe(c, !0);
        const X = Tn(c), Se = c.subTree;
        c.subTree = X, U(
          Se,
          X,
          // parent may have changed if it's in a teleport
          g(Se.el),
          // anchor may have changed if it's in a fragment
          Bt(Se),
          c,
          b,
          m
        ), T.el = X.el, L === null && Eo(c, X.el), V && fe(V, b), (z = T.props && T.props.onVnodeUpdated) && fe(
          () => Te(z, N, T, W),
          b
        );
      } else {
        let T;
        const { el: M, props: V } = u, { bm: N, m: W, parent: L, root: z, type: X } = c, Se = At(u);
        Qe(c, !1), N && Yt(N), !Se && (T = V && V.onVnodeBeforeMount) && Te(T, L, u), Qe(c, !0);
        {
          z.ce && z.ce._hasShadowRoot() && z.ce._injectChildStyle(
            X,
            c.parent ? c.parent.type : void 0
          );
          const we = c.subTree = Tn(c);
          U(
            null,
            we,
            p,
            v,
            c,
            b,
            m
          ), u.el = we.el;
        }
        if (W && fe(W, b), !Se && (T = V && V.onVnodeMounted)) {
          const we = u;
          fe(
            () => Te(T, L, we),
            b
          );
        }
        (u.shapeFlag & 256 || L && At(L.vnode) && L.vnode.shapeFlag & 256) && c.a && fe(c.a, b), c.isMounted = !0, u = p = v = null;
      }
    };
    c.scope.on();
    const x = c.effect = new sr(S);
    c.scope.off();
    const _ = c.update = x.run.bind(x), I = c.job = x.runIfDirty.bind(x);
    I.i = c, I.id = c.uid, x.scheduler = () => nn(I), Qe(c, !0), _();
  }, J = (c, u, p) => {
    u.component = c;
    const v = c.vnode.props;
    c.vnode = u, c.next = null, Ao(c, u.props, v, p), Ro(c, u.children, p), Fe(), bn(c), Ve();
  }, k = (c, u, p, v, b, m, C, S, x = !1) => {
    const _ = c && c.children, I = c ? c.shapeFlag : 0, T = u.children, { patchFlag: M, shapeFlag: V } = u;
    if (M > 0) {
      if (M & 128) {
        Wt(
          _,
          T,
          p,
          v,
          b,
          m,
          C,
          S,
          x
        );
        return;
      } else if (M & 256) {
        Ye(
          _,
          T,
          p,
          v,
          b,
          m,
          C,
          S,
          x
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
      C,
      S,
      x
    ) : mt(_, b, m, !0) : (I & 8 && a(p, ""), V & 16 && Be(
      T,
      p,
      v,
      b,
      m,
      C,
      S,
      x
    ));
  }, Ye = (c, u, p, v, b, m, C, S, x) => {
    c = c || ot, u = u || ot;
    const _ = c.length, I = u.length, T = Math.min(_, I);
    let M;
    for (M = 0; M < T; M++) {
      const V = u[M] = x ? Ne(u[M]) : Pe(u[M]);
      U(
        c[M],
        V,
        p,
        null,
        b,
        m,
        C,
        S,
        x
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
      C,
      S,
      x,
      T
    );
  }, Wt = (c, u, p, v, b, m, C, S, x) => {
    let _ = 0;
    const I = u.length;
    let T = c.length - 1, M = I - 1;
    for (; _ <= T && _ <= M; ) {
      const V = c[_], N = u[_] = x ? Ne(u[_]) : Pe(u[_]);
      if (yt(V, N))
        U(
          V,
          N,
          p,
          null,
          b,
          m,
          C,
          S,
          x
        );
      else
        break;
      _++;
    }
    for (; _ <= T && _ <= M; ) {
      const V = c[T], N = u[M] = x ? Ne(u[M]) : Pe(u[M]);
      if (yt(V, N))
        U(
          V,
          N,
          p,
          null,
          b,
          m,
          C,
          S,
          x
        );
      else
        break;
      T--, M--;
    }
    if (_ > T) {
      if (_ <= M) {
        const V = M + 1, N = V < I ? u[V].el : v;
        for (; _ <= M; )
          U(
            null,
            u[_] = x ? Ne(u[_]) : Pe(u[_]),
            p,
            N,
            b,
            m,
            C,
            S,
            x
          ), _++;
      }
    } else if (_ > M)
      for (; _ <= T; )
        xe(c[_], b, m, !0), _++;
    else {
      const V = _, N = _, W = /* @__PURE__ */ new Map();
      for (_ = N; _ <= M; _++) {
        const ae = u[_] = x ? Ne(u[_]) : Pe(u[_]);
        ae.key != null && W.set(ae.key, _);
      }
      let L, z = 0;
      const X = M - N + 1;
      let Se = !1, we = 0;
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
          for (L = N; L <= M; L++)
            if (bt[L - N] === 0 && yt(ae, u[L])) {
              Ce = L;
              break;
            }
        Ce === void 0 ? xe(ae, b, m, !0) : (bt[Ce - N] = _ + 1, Ce >= we ? we = Ce : Se = !0, U(
          ae,
          u[Ce],
          p,
          null,
          b,
          m,
          C,
          S,
          x
        ), z++);
      }
      const an = Se ? Do(bt) : ot;
      for (L = an.length - 1, _ = X - 1; _ >= 0; _--) {
        const ae = N + _, Ce = u[ae], dn = u[ae + 1], pn = ae + 1 < I ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          dn.el || kr(dn)
        ) : v;
        bt[_] === 0 ? U(
          null,
          Ce,
          p,
          pn,
          b,
          m,
          C,
          S,
          x
        ) : Se && (L < 0 || _ !== an[L] ? ze(Ce, p, pn, 2) : L--);
      }
    }
  }, ze = (c, u, p, v, b = null) => {
    const { el: m, type: C, transition: S, children: x, shapeFlag: _ } = c;
    if (_ & 6) {
      ze(c.component.subTree, u, p, v);
      return;
    }
    if (_ & 128) {
      c.suspense.move(u, p, v);
      return;
    }
    if (_ & 64) {
      C.move(c, u, p, _t);
      return;
    }
    if (C === ue) {
      n(m, u, p);
      for (let T = 0; T < x.length; T++)
        ze(x[T], u, p, v);
      n(c.anchor, u, p);
      return;
    }
    if (C === Ms) {
      y(c, u, p);
      return;
    }
    if (v !== 2 && _ & 1 && S)
      if (v === 0)
        S.persisted && !m[Es] ? n(m, u, p) : (S.beforeEnter(m), n(m, u, p), fe(() => S.enter(m), b));
      else {
        const { leave: T, delayLeave: M, afterLeave: V } = S, N = () => {
          c.ctx.isUnmounted ? r(m) : n(m, u, p);
        }, W = () => {
          const L = m._isLeaving || !!m[Es];
          m._isLeaving && m[Es](
            !0
            /* cancelled */
          ), S.persisted && !L ? N() : T(m, () => {
            N(), V && V();
          });
        };
        M ? M(m, N, W) : W();
      }
    else
      n(m, u, p);
  }, xe = (c, u, p, v = !1, b = !1) => {
    const {
      type: m,
      props: C,
      ref: S,
      children: x,
      dynamicChildren: _,
      shapeFlag: I,
      patchFlag: T,
      dirs: M,
      cacheIndex: V,
      memo: N
    } = c;
    if (T === -2 && (b = !1), S != null && (Fe(), Ot(S, null, p, c, !0), Ve()), V != null && (u.renderCache[V] = void 0), I & 256) {
      u.ctx.deactivate(c);
      return;
    }
    const W = I & 1 && M, L = !At(c);
    let z;
    if (L && (z = C && C.onVnodeBeforeUnmount) && Te(z, u, c), I & 6)
      ti(c.component, p, v);
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
      ) : (m === ue && T & 384 || !b && I & 16) && mt(x, u, p), v && fn(c);
    }
    const X = N != null && V == null;
    (L && (z = C && C.onVnodeUnmounted) || W || X) && fe(() => {
      z && Te(z, u, c), W && Xe(c, null, u, "unmounted"), X && (c.el = null);
    }, p);
  }, fn = (c) => {
    const { type: u, el: p, anchor: v, transition: b } = c;
    if (u === ue) {
      ei(p, v);
      return;
    }
    if (u === Ms) {
      R(c);
      return;
    }
    const m = () => {
      r(p), b && !b.persisted && b.afterLeave && b.afterLeave();
    };
    if (c.shapeFlag & 1 && b && !b.persisted) {
      const { leave: C, delayLeave: S } = b, x = () => C(p, m);
      S ? S(c.el, m, x) : x();
    } else
      m();
  }, ei = (c, u) => {
    let p;
    for (; c !== u; )
      p = E(c), r(c), c = p;
    r(u);
  }, ti = (c, u, p) => {
    const { bum: v, scope: b, job: m, subTree: C, um: S, m: x, a: _ } = c;
    An(x), An(_), v && Yt(v), b.stop(), m && (m.flags |= 8, xe(C, c, u, p)), S && fe(S, u), fe(() => {
      c.isUnmounted = !0;
    }, u);
  }, mt = (c, u, p, v = !1, b = !1, m = 0) => {
    for (let C = m; C < c.length; C++)
      xe(c[C], u, p, v, b);
  }, Bt = (c) => {
    if (c.shapeFlag & 6)
      return Bt(c.component.subTree);
    if (c.shapeFlag & 128)
      return c.suspense.next();
    const u = E(c.anchor || c.el), p = u && u[zi];
    return p ? E(p) : u;
  };
  let bs = !1;
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
    ), u._vnode = c, bs || (bs = !0, bn(v), yr(), bs = !1);
  }, _t = {
    p: U,
    um: xe,
    m: ze,
    r: fn,
    mt: _s,
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
function Ps({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function Qe({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Uo(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Wr(e, t, s = !1) {
  const n = e.children, r = t.children;
  if (F(n) && F(r))
    for (let i = 0; i < n.length; i++) {
      const o = n[i];
      let l = r[i];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = r[i] = Ne(r[i]), l.el = o.el), !s && l.patchFlag !== -2 && Wr(o, l)), l.type === gs && (l.patchFlag === -1 && (l = r[i] = Ne(l)), l.el = o.el), l.type === Je && !l.el && (l.el = o.el);
    }
}
function Do(e) {
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
function Br(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : Br(t);
}
function An(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function kr(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? kr(t.subTree) : null;
}
const qr = (e) => e.__isSuspense;
function No(e, t) {
  t && t.pendingBranch ? F(e) ? t.effects.push(...e) : t.effects.push(e) : Bi(e);
}
const ue = /* @__PURE__ */ Symbol.for("v-fgt"), gs = /* @__PURE__ */ Symbol.for("v-txt"), Je = /* @__PURE__ */ Symbol.for("v-cmt"), Ms = /* @__PURE__ */ Symbol.for("v-stc"), Mt = [];
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
function Jr(e) {
  return e.dynamicChildren = Vt > 0 ? de || ot : null, jo(), Vt > 0 && de && de.push(e), e;
}
function ce(e, t, s, n, r, i) {
  return Jr(
    A(
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
function $o(e, t, s, n, r) {
  return Jr(
    He(
      e,
      t,
      s,
      n,
      r,
      !0
    )
  );
}
function Gr(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function yt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const Yr = ({ key: e }) => e ?? null, Xt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? Y(e) || /* @__PURE__ */ te(e) || D(e) ? { i: pe, r: e, k: t, f: !!s } : e : null);
function A(e, t = null, s = null, n = 0, r = null, i = e === ue ? 0 : 1, o = !1, l = !1) {
  const f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Yr(t),
    ref: t && Xt(t),
    scopeId: Sr,
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
    ctx: pe
  };
  return l ? (ns(f, s), i & 128 && e.normalize(f)) : s && (f.shapeFlag |= Y(s) ? 8 : 16), Vt > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  de && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (f.patchFlag > 0 || i & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  f.patchFlag !== 32 && de.push(f), f;
}
const He = Ho;
function Ho(e, t = null, s = null, n = 0, r = null, i = !1) {
  if ((!e || e === uo) && (e = Je), Gr(e)) {
    const l = at(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ns(l, s), Vt > 0 && !i && de && (l.shapeFlag & 6 ? de[de.indexOf(e)] = l : de.push(l)), l.patchFlag = -2, l;
  }
  if (Qo(e) && (e = e.__vccOpts), t) {
    t = Lo(t);
    let { class: l, style: f } = t;
    l && !Y(l) && (t.class = Gs(l)), K(f) && (/* @__PURE__ */ sn(f) && !F(f) && (f = Z({}, f)), t.style = Js(f));
  }
  const o = Y(e) ? 1 : qr(e) ? 128 : Xi(e) ? 64 : K(e) ? 4 : D(e) ? 2 : 0;
  return A(
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
function Lo(e) {
  return e ? /* @__PURE__ */ sn(e) || Nr(e) ? Z({}, e) : e : null;
}
function at(e, t, s = !1, n = !1) {
  const { props: r, ref: i, patchFlag: o, children: l, transition: f } = e, d = t ? Wo(r || {}, t) : r, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && Yr(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && i ? F(i) ? i.concat(Xt(t)) : [i, Xt(t)] : Xt(t)
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
  return He(gs, null, e, t);
}
function xt(e = "", t = !1) {
  return t ? (ie(), $o(Je, null, e)) : He(Je, null, e);
}
function Pe(e) {
  return e == null || typeof e == "boolean" ? He(Je) : F(e) ? He(
    ue,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Gr(e) ? Ne(e) : He(gs, null, String(e));
}
function Ne(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : at(e);
}
function ns(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (F(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const r = t.default;
      r && (r._c && (r._d = !1), ns(e, r()), r._c && (r._d = !0));
      return;
    } else {
      s = 32;
      const r = t._;
      !r && !Nr(t) ? t._ctx = pe : r === 3 && pe && (pe.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (D(t)) {
    if (n & 65) {
      ns(e, { default: t });
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
    for (const r in n)
      if (r === "class")
        t.class !== n.class && (t.class = Gs([t.class, n.class]));
      else if (r === "style")
        t.style = Js([t.style, n.style]);
      else if (is(r)) {
        const i = t[r], o = n[r];
        o && i !== o && !(F(i) && i.includes(o)) ? t[r] = i ? [].concat(i, o) : o : o == null && i == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !os(r) && (t[r] = o);
      } else r !== "" && (t[r] = n[r]);
  }
  return t;
}
function Te(e, t, s, n = null) {
  be(e, t, 7, [
    s,
    n
  ]);
}
const Bo = Rr();
let ko = 0;
function qo(e, t, s) {
  const n = e.type, r = (t ? t.appContext : e.appContext) || Bo, i = {
    uid: ko++,
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
    propsOptions: $r(n, r),
    emitsOptions: Fr(n, r),
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
  return i.ctx = { _: i }, i.root = t ? t.root : i, i.emit = xo.bind(null, i), e.ce && e.ce(i), i;
}
let le = null;
const Jo = () => le || pe;
let rs, Ks;
{
  const e = fs(), t = (s, n) => {
    let r;
    return (r = e[s]) || (r = e[s] = []), r.push(n), (i) => {
      r.length > 1 ? r.forEach((o) => o(i)) : r[0](i);
    };
  };
  rs = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => le = s
  ), Ks = t(
    "__VUE_SSR_SETTERS__",
    (s) => Ut = s
  );
}
const $t = (e) => {
  const t = le;
  return rs(e), e.scope.on(), () => {
    e.scope.off(), rs(t);
  };
}, Mn = () => {
  le && le.scope.off(), rs(null);
};
function zr(e) {
  return e.vnode.shapeFlag & 4;
}
let Ut = !1;
function Go(e, t = !1, s = !1) {
  t && Ks(t);
  const { props: n, children: r } = e.vnode, i = zr(e);
  Oo(e, n, i, t), Io(e, r, s || t);
  const o = i ? Yo(e, t) : void 0;
  return t && Ks(!1), o;
}
function Yo(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, ao);
  const { setup: n } = s;
  if (n) {
    Fe();
    const r = e.setupContext = n.length > 1 ? Xo(e) : null, i = $t(e), o = jt(
      n,
      e,
      0,
      [
        e.props,
        r
      ]
    ), l = Gn(o);
    if (Ve(), i(), (l || e.sp) && !At(e) && Tr(e), l) {
      if (o.then(Mn, Mn), t)
        return o.then((f) => {
          In(e, f);
        }).catch((f) => {
          ds(f, e, 0);
        });
      e.asyncDep = o;
    } else
      In(e, o);
  } else
    Xr(e);
}
function In(e, t, s) {
  D(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : K(t) && (e.setupState = mr(t)), Xr(e);
}
function Xr(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Ie);
  {
    const r = $t(e);
    Fe();
    try {
      po(e);
    } finally {
      Ve(), r();
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
function ms(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(mr(Fi(e.exposed)), {
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
const Zo = (e, t) => /* @__PURE__ */ $i(e, t, Ut), el = "3.5.39";
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
const Qr = Ws ? (e) => Ws.createHTML(e) : (e) => e, tl = "http://www.w3.org/2000/svg", sl = "http://www.w3.org/1998/Math/MathML", De = typeof document < "u" ? document : null, Fn = De && /* @__PURE__ */ De.createElement("template"), nl = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const r = t === "svg" ? De.createElementNS(tl, e) : t === "mathml" ? De.createElementNS(sl, e) : s ? De.createElement(e, { is: s }) : De.createElement(e);
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
      Fn.innerHTML = Qr(
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
}, rl = /* @__PURE__ */ Symbol("_vtc");
function il(e, t, s) {
  const n = e[rl];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const Vn = /* @__PURE__ */ Symbol("_vod"), ol = /* @__PURE__ */ Symbol("_vsh"), ll = /* @__PURE__ */ Symbol(""), cl = /(?:^|;)\s*display\s*:/;
function fl(e, t, s) {
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
      l != null ? al(
        e,
        o,
        !Y(t) && t ? t[o] : void 0,
        l
      ) || wt(n, o, l) : wt(n, o, "");
    }
  } else if (r) {
    if (t !== s) {
      const o = n[ll];
      o && (s += ";" + o), n.cssText = s, i = cl.test(s);
    }
  } else t && e.removeAttribute("style");
  Vn in e && (e[Vn] = i ? n.display : "", e[ol] && (n.display = "none"));
}
const Un = /\s*!important$/;
function wt(e, t, s) {
  if (F(s))
    s.forEach((n) => wt(e, t, n));
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
const Dn = ["Webkit", "Moz", "ms"], Is = {};
function ul(e, t) {
  const s = Is[t];
  if (s)
    return s;
  let n = ge(t);
  if (n !== "filter" && n in e)
    return Is[t] = n;
  n = Xn(n);
  for (let r = 0; r < Dn.length; r++) {
    const i = Dn[r] + n;
    if (i in e)
      return Is[t] = i;
  }
  return t;
}
function al(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && Y(n) && s === n;
}
const Nn = "http://www.w3.org/1999/xlink";
function jn(e, t, s, n, r, i = ai(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Nn, t.slice(6, t.length)) : e.setAttributeNS(Nn, t, s) : s == null || i && !Zn(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    i ? "" : Re(s) ? String(s) : s
  );
}
function $n(e, t, s, n, r) {
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
    l === "boolean" ? s = Zn(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(r || t);
}
function qe(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function dl(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const Hn = /* @__PURE__ */ Symbol("_vei");
function pl(e, t, s, n, r = null) {
  const i = e[Hn] || (e[Hn] = {}), o = i[t];
  if (n && o)
    o.value = n;
  else {
    const [l, f] = ml(t);
    if (n) {
      const d = i[t] = vl(
        n,
        r
      );
      qe(e, l, d, f);
    } else o && (dl(e, l, o, f), i[t] = void 0);
  }
}
const hl = /(Once|Passive|Capture)$/, gl = /^on:?(?:Once|Passive|Capture)$/;
function ml(e) {
  let t, s;
  for (; (s = e.match(hl)) && !gl.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : st(e.slice(2)), t];
}
let Rs = 0;
const _l = /* @__PURE__ */ Promise.resolve(), bl = () => Rs || (_l.then(() => Rs = 0), Rs = Date.now());
function vl(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const r = s.value;
    if (F(r)) {
      const i = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        i.call(n), n._stopped = !0;
      };
      const o = r.slice(), l = [n];
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
        r,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = bl(), s;
}
const Ln = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, yl = (e, t, s, n, r, i) => {
  const o = r === "svg";
  t === "class" ? il(e, n, o) : t === "style" ? fl(e, s, n) : is(t) ? os(t) || pl(e, t, s, n, i) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : xl(e, t, n, o)) ? ($n(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && jn(e, t, n, o, i, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (Sl(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !Y(n))) ? $n(e, ge(t), n, i, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), jn(e, t, n, o));
};
function xl(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Ln(t) && D(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const r = e.tagName;
    if (r === "IMG" || r === "VIDEO" || r === "CANVAS" || r === "SOURCE")
      return !1;
  }
  return Ln(t) && Y(s) ? !1 : t in e;
}
function Sl(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = ge(t);
  return Array.isArray(s) ? s.some((r) => ge(r) === n) : Object.keys(s).some((r) => ge(r) === n);
}
const dt = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return F(t) ? (s) => Yt(t, s) : t;
};
function wl(e) {
  e.target.composing = !0;
}
function Kn(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const Le = /* @__PURE__ */ Symbol("_assign");
function Wn(e, t, s) {
  return t && (e = e.trim()), s && (e = cs(e)), e;
}
const Ee = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, r) {
    e[Le] = dt(r);
    const i = n || r.props && r.props.type === "number";
    qe(e, t ? "change" : "input", (o) => {
      o.target.composing || e[Le](Wn(e.value, s, i));
    }), (s || i) && qe(e, "change", () => {
      e.value = Wn(e.value, s, i);
    }), t || (qe(e, "compositionstart", wl), qe(e, "compositionend", Kn), qe(e, "change", Kn));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: r, number: i } }, o) {
    if (e[Le] = dt(o), e.composing) return;
    const l = (i || e.type === "number") && !/^0\d/.test(e.value) ? cs(e.value) : e.value, f = t ?? "";
    if (l === f)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (n && t === s || r && e.value.trim() === f) || (e.value = f);
  }
}, Cl = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, s) {
    e[Le] = dt(s), qe(e, "change", () => {
      const n = e._modelValue, r = Dt(e), i = e.checked, o = e[Le];
      if (F(n)) {
        const l = Ys(n, r), f = l !== -1;
        if (i && !f)
          o(n.concat(r));
        else if (!i && f) {
          const d = [...n];
          d.splice(l, 1), o(d);
        }
      } else if (pt(n)) {
        const l = new Set(n);
        i ? l.add(r) : l.delete(r), o(l);
      } else
        o(Zr(e, i));
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
  let r;
  if (F(t))
    r = Ys(t, n.props.value) > -1;
  else if (pt(t))
    r = t.has(n.props.value);
  else {
    if (t === s) return;
    r = ht(t, Zr(e, !0));
  }
  e.checked !== r && (e.checked = r);
}
const Gt = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const r = pt(t);
    qe(e, "change", () => {
      const i = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? cs(Dt(o)) : Dt(o)
      );
      e[Le](
        e.multiple ? r ? new Set(i) : i : i[0]
      ), e._assigning = !0, br(() => {
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
    for (let r = 0, i = e.options.length; r < i; r++) {
      const o = e.options[r], l = Dt(o);
      if (s)
        if (n) {
          const f = typeof l;
          f === "string" || f === "number" ? o.selected = t.some((d) => String(d) === String(l)) : o.selected = Ys(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (ht(Dt(o), t)) {
        e.selectedIndex !== r && (e.selectedIndex = r);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function Dt(e) {
  return "_value" in e ? e._value : e.value;
}
function Zr(e, t) {
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
    const r = Pl(n);
    if (!r) return;
    const i = t._component;
    !D(i) && !i.render && !i.template && (i.template = r.innerHTML), r.nodeType === 1 && (r.textContent = "");
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
  return Y(e) ? document.querySelector(e) : e;
}
const Ml = { class: "sql-set" }, Il = { class: "row" }, Rl = ["value"], Fl = {
  key: 0,
  class: "muted empty"
}, Vl = { class: "row spread" }, Ul = { class: "row" }, Dl = ["onUpdate:modelValue"], Nl = ["onUpdate:modelValue"], jl = ["onClick"], $l = {
  key: 0,
  class: "row"
}, Hl = ["onUpdate:modelValue"], Ll = ["onUpdate:modelValue"], Kl = {
  key: 1,
  class: "row"
}, Wl = ["onUpdate:modelValue"], Bl = { class: "row" }, kl = {
  key: 0,
  class: "chk"
}, ql = ["onUpdate:modelValue"], Jl = { class: "row" }, Gl = ["onUpdate:modelValue"], Yl = ["value"], zl = ["onClick"], Xl = {
  key: 0,
  class: "row new-cred"
}, Ql = ["onUpdate:modelValue"], Zl = ["onUpdate:modelValue"], ec = ["onUpdate:modelValue"], tc = ["onUpdate:modelValue"], sc = ["disabled", "onClick"], nc = { class: "muted" }, rc = { class: "row" }, ic = ["onUpdate:modelValue", "placeholder"], oc = { class: "row" }, lc = ["onUpdate:modelValue"], cc = { class: "row" }, fc = ["disabled", "onClick"], uc = { class: "muted" }, ac = /* @__PURE__ */ Qi({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function r(w, h) {
      return {
        key: n++,
        name: w,
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
        credScope: "",
        credStatus: "",
        testing: !1,
        testStatus: ""
      };
    }
    function i(w) {
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
        return JSON.parse(s.api.getJson() || "null") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ Cs(o.default_connection || ""), f = /* @__PURE__ */ Cs(o.default_limit || 10), d = /* @__PURE__ */ as(
      Object.entries(o.connections || {}).map(([w, h]) => r(w, h))
    ), a = /* @__PURE__ */ Cs([]);
    async function g() {
      try {
        const w = await s.api.invoke("secret.list");
        a.value = [...w.user || [], ...w.project || [], ...w.shared || []].map((h) => ({ reference: h.reference, label: `${h.key} · ${h.scope}` })).sort((h, y) => h.label.localeCompare(y.label));
      } catch {
      }
    }
    Ar(g);
    function E() {
      d.push(r(`db${d.length + 1}`, { provider: "mssql" }));
    }
    async function O(w) {
      w.credStatus = "Saving…";
      try {
        const h = { password: w.credPassword };
        w.credUser && (h.user = w.credUser), await s.api.invoke("secret.set", { key: w.credName.trim(), fields: h, scope: w.credScope }), w.credential = `secret:${w.credScope}:${w.credName.trim()}`, w.newCred = !1, w.credName = "", w.credUser = "", w.credPassword = "", w.credScope = "", w.credStatus = "", await g();
      } catch (h) {
        w.credStatus = "Failed: " + (h instanceof Error ? h.message : String(h));
      }
    }
    async function j(w) {
      w.testing = !0, w.testStatus = "Connecting...";
      try {
        const h = await s.api.invoke("plugin.action", {
          pluginId: "sql",
          action: "testConnection",
          valueJson: JSON.stringify(U(w))
        });
        if (h.ok && h.resultJson) {
          const y = JSON.parse(h.resultJson);
          w.testStatus = y.message;
        } else
          w.testStatus = "Failed: " + (h.error || "unknown error");
      } catch (h) {
        w.testStatus = "Failed: " + (h instanceof Error ? h.message : String(h));
      } finally {
        w.testing = !1;
      }
    }
    function U(w) {
      const h = i(w);
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
      const w = {
        default_connection: l.value || void 0,
        default_limit: f.value || 10,
        connections: Object.fromEntries(
          d.filter((h) => h.name.trim()).map((h) => [h.name.trim(), i(h)])
        )
      };
      return JSON.stringify(w);
    }
    return t({ toJson: G }), (w, h) => (ie(), ce("div", Ml, [
      h[18] || (h[18] = A("div", { class: "muted" }, " Named database connections available to the SQL agent. Passwords live in the secret store (Settings → Secrets); a connection only references an entry by name. Stored in the .spla project file. ", -1)),
      A("div", Il, [
        A("label", null, [
          h[3] || (h[3] = A("span", { class: "muted" }, "Default connection", -1)),
          ne(A("select", {
            "onUpdate:modelValue": h[0] || (h[0] = (y) => l.value = y)
          }, [
            h[2] || (h[2] = A("option", { value: "" }, "(none)", -1)),
            (ie(!0), ce(ue, null, Os(d, (y) => (ie(), ce("option", {
              key: y.key,
              value: y.name
            }, rt(y.name), 9, Rl))), 128))
          ], 512), [
            [Gt, l.value]
          ])
        ]),
        A("label", null, [
          h[4] || (h[4] = A("span", { class: "muted" }, "Default row limit", -1)),
          ne(A("input", {
            "onUpdate:modelValue": h[1] || (h[1] = (y) => f.value = y),
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
      A("button", {
        type: "button",
        class: "self-start",
        onClick: E
      }, "+ Add Connection"),
      d.length ? xt("", !0) : (ie(), ce("div", Fl, 'No connections yet. Click "+ Add Connection".')),
      (ie(!0), ce(ue, null, Os(d, (y, R) => (ie(), ce("div", {
        key: y.key,
        class: "conn-card"
      }, [
        A("div", Vl, [
          A("div", Ul, [
            h[6] || (h[6] = A("span", { class: "muted" }, "Name", -1)),
            ne(A("input", {
              "onUpdate:modelValue": (P) => y.name = P,
              class: "w-140",
              spellcheck: "false"
            }, null, 8, Dl), [
              [Ee, y.name]
            ]),
            h[7] || (h[7] = A("span", { class: "muted" }, "Provider", -1)),
            ne(A("select", {
              "onUpdate:modelValue": (P) => y.provider = P
            }, [...h[5] || (h[5] = [
              A("option", { value: "mssql" }, "mssql", -1),
              A("option", { value: "postgres" }, "postgres", -1),
              A("option", { value: "sqlite" }, "sqlite", -1)
            ])], 8, Nl), [
              [Gt, y.provider]
            ])
          ]),
          A("button", {
            type: "button",
            onClick: (P) => d.splice(R, 1)
          }, "✕ Remove", 8, jl)
        ]),
        y.provider !== "sqlite" ? (ie(), ce("div", $l, [
          h[8] || (h[8] = A("span", { class: "muted w-70" }, "Server", -1)),
          ne(A("input", {
            "onUpdate:modelValue": (P) => y.path = P,
            placeholder: "sql01 or 192.168.1.10",
            class: "w-220",
            spellcheck: "false"
          }, null, 8, Hl), [
            [Ee, y.path]
          ]),
          h[9] || (h[9] = A("span", { class: "muted w-70" }, "Database", -1)),
          ne(A("input", {
            "onUpdate:modelValue": (P) => y.database = P,
            class: "w-160",
            spellcheck: "false"
          }, null, 8, Ll), [
            [Ee, y.database]
          ])
        ])) : (ie(), ce("div", Kl, [
          h[10] || (h[10] = A("span", { class: "muted w-70" }, "File", -1)),
          ne(A("input", {
            "onUpdate:modelValue": (P) => y.path = P,
            placeholder: "C:\\data\\mydb.sqlite",
            class: "w-400",
            spellcheck: "false"
          }, null, 8, Wl), [
            [Ee, y.path]
          ])
        ])),
        y.provider !== "sqlite" ? (ie(), ce(ue, { key: 2 }, [
          A("div", Bl, [
            y.provider === "mssql" ? (ie(), ce("label", kl, [
              ne(A("input", {
                type: "checkbox",
                "onUpdate:modelValue": (P) => y.trustedConnection = P
              }, null, 8, ql), [
                [Cl, y.trustedConnection]
              ]),
              h[11] || (h[11] = A("span", null, "Windows Auth (domain)", -1))
            ])) : xt("", !0)
          ]),
          !y.trustedConnection || y.provider !== "mssql" ? (ie(), ce(ue, { key: 0 }, [
            A("div", Jl, [
              h[13] || (h[13] = A("span", { class: "muted w-70" }, "Credential", -1)),
              ne(A("select", {
                "onUpdate:modelValue": (P) => y.credential = P
              }, [
                h[12] || (h[12] = A("option", { value: "" }, "(none — use fields below)", -1)),
                (ie(!0), ce(ue, null, Os(a.value, (P) => (ie(), ce("option", {
                  key: P,
                  value: P
                }, rt(P), 9, Yl))), 128))
              ], 8, Gl), [
                [Gt, y.credential]
              ]),
              A("button", {
                type: "button",
                onClick: (P) => y.newCred = !y.newCred
              }, rt(y.newCred ? "cancel" : "new…"), 9, zl),
              h[14] || (h[14] = A("span", { class: "muted" }, "entry in the secret store: user + password", -1))
            ]),
            y.newCred ? (ie(), ce("div", Xl, [
              ne(A("input", {
                "onUpdate:modelValue": (P) => y.credName = P,
                placeholder: "entry name",
                class: "w-140",
                spellcheck: "false"
              }, null, 8, Ql), [
                [Ee, y.credName]
              ]),
              ne(A("input", {
                "onUpdate:modelValue": (P) => y.credUser = P,
                placeholder: "user",
                class: "w-120",
                spellcheck: "false"
              }, null, 8, Zl), [
                [Ee, y.credUser]
              ]),
              ne(A("input", {
                "onUpdate:modelValue": (P) => y.credPassword = P,
                type: "password",
                placeholder: "password",
                class: "w-140",
                autocomplete: "new-password"
              }, null, 8, ec), [
                [Ee, y.credPassword]
              ]),
              ne(A("select", {
                "onUpdate:modelValue": (P) => y.credScope = P,
                title: "Where this credential is stored"
              }, [...h[15] || (h[15] = [
                A("option", { value: "" }, "scope…", -1),
                A("option", { value: "user" }, "user — mine only", -1),
                A("option", { value: "project" }, "project — travels with the project", -1),
                A("option", { value: "shared" }, "shared — administered", -1)
              ])], 8, tc), [
                [Gt, y.credScope]
              ]),
              A("button", {
                type: "button",
                disabled: !y.credName || !y.credPassword || !y.credScope,
                onClick: (P) => O(y)
              }, "Save to store", 8, sc),
              A("span", nc, rt(y.credStatus), 1)
            ])) : xt("", !0),
            A("div", rc, [
              h[16] || (h[16] = A("span", { class: "muted w-70" }, "User", -1)),
              ne(A("input", {
                "onUpdate:modelValue": (P) => y.user = P,
                placeholder: y.credential ? "(from credential)" : "login",
                class: "w-130",
                spellcheck: "false"
              }, null, 8, ic), [
                [Ee, y.user]
              ])
            ])
          ], 64)) : xt("", !0)
        ], 64)) : xt("", !0),
        A("div", oc, [
          h[17] || (h[17] = A("span", { class: "muted w-70" }, "Description", -1)),
          ne(A("input", {
            "onUpdate:modelValue": (P) => y.description = P,
            placeholder: "Shown to the AI — what this database contains",
            class: "grow"
          }, null, 8, lc), [
            [Ee, y.description]
          ])
        ]),
        A("div", cc, [
          A("button", {
            type: "button",
            disabled: y.testing,
            onClick: (P) => j(y)
          }, "Test Connection", 8, fc),
          A("span", uc, rt(y.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
}), dc = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, r] of t)
    s[n] = r;
  return s;
}, pc = /* @__PURE__ */ dc(ac, [["__scopeId", "data-v-6d76f2b4"]]);
function gc(e, t) {
  let s = Ol(pc, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toJson(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  gc as mount
};
