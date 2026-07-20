/**
* @vue/shared v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function ku(t) {
  const e = /* @__PURE__ */ Object.create(null);
  for (const r of t.split(",")) e[r] = 1;
  return (r) => r in e;
}
const Ge = {}, Jn = [], Er = () => {
}, kd = () => !1, Rs = (t) => t.charCodeAt(0) === 111 && t.charCodeAt(1) === 110 && // uppercase letter
(t.charCodeAt(2) > 122 || t.charCodeAt(2) < 97), Ms = (t) => t.startsWith("onUpdate:"), Tt = Object.assign, Bu = (t, e) => {
  const r = t.indexOf(e);
  r > -1 && t.splice(r, 1);
}, zp = Object.prototype.hasOwnProperty, Ve = (t, e) => zp.call(t, e), ke = Array.isArray, ea = (t) => ci(t) === "[object Map]", Ls = (t) => ci(t) === "[object Set]", xf = (t) => ci(t) === "[object Date]", Re = (t) => typeof t == "function", at = (t) => typeof t == "string", Cr = (t) => typeof t == "symbol", Ue = (t) => t !== null && typeof t == "object", Bd = (t) => (Ue(t) || Re(t)) && Re(t.then) && Re(t.catch), Rd = Object.prototype.toString, ci = (t) => Rd.call(t), Vp = (t) => ci(t).slice(8, -1), Md = (t) => ci(t) === "[object Object]", Ru = (t) => at(t) && t !== "NaN" && t[0] !== "-" && "" + parseInt(t, 10) === t, Va = /* @__PURE__ */ ku(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), Is = (t) => {
  const e = /* @__PURE__ */ Object.create(null);
  return ((r) => e[r] || (e[r] = t(r)));
}, qp = /-\w/g, Ft = Is(
  (t) => t.replace(qp, (e) => e.slice(1).toUpperCase())
), $p = /\B([A-Z])/g, Nn = Is(
  (t) => t.replace($p, "-$1").toLowerCase()
), Os = Is((t) => t.charAt(0).toUpperCase() + t.slice(1)), fo = Is(
  (t) => t ? `on${Os(t)}` : ""
), wr = (t, e) => !Object.is(t, e), Wi = (t, ...e) => {
  for (let r = 0; r < t.length; r++)
    t[r](...e);
}, Ld = (t, e, r, n = !1) => {
  Object.defineProperty(t, e, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: r
  });
}, Ns = (t) => {
  const e = parseFloat(t);
  return isNaN(e) ? t : e;
};
let Ef;
const _s = () => Ef || (Ef = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function Mu(t) {
  if (ke(t)) {
    const e = {};
    for (let r = 0; r < t.length; r++) {
      const n = t[r], a = at(n) ? Wp(n) : Mu(n);
      if (a)
        for (const i in a)
          e[i] = a[i];
    }
    return e;
  } else if (at(t) || Ue(t))
    return t;
}
const Hp = /;(?![^(]*\))/g, Up = /:([^]+)/, Gp = /\/\*[^]*?\*\//g;
function Wp(t) {
  const e = {};
  return t.replace(Gp, "").split(Hp).forEach((r) => {
    if (r) {
      const n = r.split(Up);
      n.length > 1 && (e[n[0].trim()] = n[1].trim());
    }
  }), e;
}
function la(t) {
  let e = "";
  if (at(t))
    e = t;
  else if (ke(t))
    for (let r = 0; r < t.length; r++) {
      const n = la(t[r]);
      n && (e += n + " ");
    }
  else if (Ue(t))
    for (const r in t)
      t[r] && (e += r + " ");
  return e.trim();
}
const Kp = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", Yp = /* @__PURE__ */ ku(Kp);
function Id(t) {
  return !!t || t === "";
}
function Xp(t, e) {
  if (t.length !== e.length) return !1;
  let r = !0;
  for (let n = 0; r && n < t.length; n++)
    r = vi(t[n], e[n]);
  return r;
}
function vi(t, e) {
  if (t === e) return !0;
  let r = xf(t), n = xf(e);
  if (r || n)
    return r && n ? t.getTime() === e.getTime() : !1;
  if (r = Cr(t), n = Cr(e), r || n)
    return t === e;
  if (r = ke(t), n = ke(e), r || n)
    return r && n ? Xp(t, e) : !1;
  if (r = Ue(t), n = Ue(e), r || n) {
    if (!r || !n)
      return !1;
    const a = Object.keys(t).length, i = Object.keys(e).length;
    if (a !== i)
      return !1;
    for (const s in t) {
      const o = t.hasOwnProperty(s), l = e.hasOwnProperty(s);
      if (o && !l || !o && l || !vi(t[s], e[s]))
        return !1;
    }
  }
  return String(t) === String(e);
}
function Zp(t, e) {
  return t.findIndex((r) => vi(r, e));
}
const Od = (t) => !!(t && t.__v_isRef === !0), Qe = (t) => at(t) ? t : t == null ? "" : ke(t) || Ue(t) && (t.toString === Rd || !Re(t.toString)) ? Od(t) ? Qe(t.value) : JSON.stringify(t, Nd, 2) : String(t), Nd = (t, e) => Od(e) ? Nd(t, e.value) : ea(e) ? {
  [`Map(${e.size})`]: [...e.entries()].reduce(
    (r, [n, a], i) => (r[co(n, i) + " =>"] = a, r),
    {}
  )
} : Ls(e) ? {
  [`Set(${e.size})`]: [...e.values()].map((r) => co(r))
} : Cr(e) ? co(e) : Ue(e) && !ke(e) && !Md(e) ? String(e) : e, co = (t, e = "") => {
  var r;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Cr(t) ? `Symbol(${(r = t.description) != null ? r : e})` : t
  );
};
/**
* @vue/reactivity v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let xt;
class Qp {
  // TODO isolatedDeclarations "__v_skip"
  constructor(e = !1) {
    this.detached = e, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !e && xt && (xt.active ? (this.parent = xt, this.index = (xt.scopes || (xt.scopes = [])).push(
      this
    ) - 1) : (this._active = !1, this._warnOnRun = !1));
  }
  get active() {
    return this._active;
  }
  pause() {
    if (this._active) {
      this._isPaused = !0;
      let e, r;
      if (this.scopes) {
        const n = this.scopes.slice();
        for (e = 0, r = n.length; e < r; e++)
          n[e].pause();
      }
      for (e = 0, r = this.effects.length; e < r; e++)
        this.effects[e].pause();
    }
  }
  /**
   * Resumes the effect scope, including all child scopes and effects.
   */
  resume() {
    if (this._active && this._isPaused) {
      this._isPaused = !1;
      let e, r;
      if (this.scopes) {
        const a = this.scopes.slice();
        for (e = 0, r = a.length; e < r; e++)
          a[e].resume();
      }
      const n = this.effects.slice();
      for (e = 0, r = n.length; e < r; e++)
        n[e].resume();
    }
  }
  run(e) {
    if (this._active) {
      const r = xt;
      try {
        return xt = this, e();
      } finally {
        xt = r;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = xt, xt = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (xt === this)
        xt = this.prevScope;
      else {
        let e = xt;
        for (; e; ) {
          if (e.prevScope === this) {
            e.prevScope = this.prevScope;
            break;
          }
          e = e.prevScope;
        }
      }
      this.prevScope = void 0;
    }
  }
  stop(e) {
    if (this._active) {
      this._active = !1;
      let r, n;
      for (r = 0, n = this.effects.length; r < n; r++)
        this.effects[r].stop();
      for (this.effects.length = 0, r = 0, n = this.cleanups.length; r < n; r++)
        this.cleanups[r]();
      if (this.cleanups.length = 0, this.scopes) {
        const a = this.scopes.slice();
        for (r = 0, n = a.length; r < n; r++)
          a[r].stop(!0);
        this.scopes.length = 0;
      }
      if (!this.detached && this.parent && !e) {
        const a = this.parent.scopes.pop();
        a && a !== this && (this.parent.scopes[this.index] = a, a.index = this.index);
      }
      this.parent = void 0;
    }
  }
}
function jp() {
  return xt;
}
let Ye;
const vo = /* @__PURE__ */ new WeakSet();
class _d {
  constructor(e) {
    this.fn = e, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, xt && (xt.active ? xt.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, vo.has(this) && (vo.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || zd(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, Cf(this), Vd(this);
    const e = Ye, r = sr;
    Ye = this, sr = !0;
    try {
      return this.fn();
    } finally {
      qd(this), Ye = e, sr = r, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let e = this.deps; e; e = e.nextDep)
        Ou(e);
      this.deps = this.depsTail = void 0, Cf(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? vo.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    jl(this) && this.run();
  }
  get dirty() {
    return jl(this);
  }
}
let Fd = 0, qa, $a;
function zd(t, e = !1) {
  if (t.flags |= 8, e) {
    t.next = $a, $a = t;
    return;
  }
  t.next = qa, qa = t;
}
function Lu() {
  Fd++;
}
function Iu() {
  if (--Fd > 0)
    return;
  if ($a) {
    let e = $a;
    for ($a = void 0; e; ) {
      const r = e.next;
      e.next = void 0, e.flags &= -9, e = r;
    }
  }
  let t;
  for (; qa; ) {
    let e = qa;
    for (qa = void 0; e; ) {
      const r = e.next;
      if (e.next = void 0, e.flags &= -9, e.flags & 1)
        try {
          e.trigger();
        } catch (n) {
          t || (t = n);
        }
      e = r;
    }
  }
  if (t) throw t;
}
function Vd(t) {
  for (let e = t.deps; e; e = e.nextDep)
    e.version = -1, e.prevActiveLink = e.dep.activeLink, e.dep.activeLink = e;
}
function qd(t) {
  let e, r = t.depsTail, n = r;
  for (; n; ) {
    const a = n.prevDep;
    n.version === -1 ? (n === r && (r = a), Ou(n), Jp(n)) : e = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = a;
  }
  t.deps = e, t.depsTail = r;
}
function jl(t) {
  for (let e = t.deps; e; e = e.nextDep)
    if (e.dep.version !== e.version || e.dep.computed && ($d(e.dep.computed) || e.dep.version !== e.version))
      return !0;
  return !!t._dirty;
}
function $d(t) {
  if (t.flags & 4 && !(t.flags & 16) || (t.flags &= -17, t.globalVersion === Xa) || (t.globalVersion = Xa, !t.isSSR && t.flags & 128 && (!t.deps && !t._dirty || !jl(t))))
    return;
  t.flags |= 2;
  const e = t.dep, r = Ye, n = sr;
  Ye = t, sr = !0;
  try {
    Vd(t);
    const a = t.fn(t._value);
    (e.version === 0 || wr(a, t._value)) && (t.flags |= 128, t._value = a, e.version++);
  } catch (a) {
    throw e.version++, a;
  } finally {
    Ye = r, sr = n, qd(t), t.flags &= -3;
  }
}
function Ou(t, e = !1) {
  const { dep: r, prevSub: n, nextSub: a } = t;
  if (n && (n.nextSub = a, t.prevSub = void 0), a && (a.prevSub = n, t.nextSub = void 0), r.subs === t && (r.subs = n, !n && r.computed)) {
    r.computed.flags &= -5;
    for (let i = r.computed.deps; i; i = i.nextDep)
      Ou(i, !0);
  }
  !e && !--r.sc && r.map && r.map.delete(r.key);
}
function Jp(t) {
  const { prevDep: e, nextDep: r } = t;
  e && (e.nextDep = r, t.prevDep = void 0), r && (r.prevDep = e, t.nextDep = void 0);
}
let sr = !0;
const Hd = [];
function Fr() {
  Hd.push(sr), sr = !1;
}
function zr() {
  const t = Hd.pop();
  sr = t === void 0 ? !0 : t;
}
function Cf(t) {
  const { cleanup: e } = t;
  if (t.cleanup = void 0, e) {
    const r = Ye;
    Ye = void 0;
    try {
      e();
    } finally {
      Ye = r;
    }
  }
}
let Xa = 0;
class ey {
  constructor(e, r) {
    this.sub = e, this.dep = r, this.version = r.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class Nu {
  // TODO isolatedDeclarations "__v_skip"
  constructor(e) {
    this.computed = e, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(e) {
    if (!Ye || !sr || Ye === this.computed)
      return;
    let r = this.activeLink;
    if (r === void 0 || r.sub !== Ye)
      r = this.activeLink = new ey(Ye, this), Ye.deps ? (r.prevDep = Ye.depsTail, Ye.depsTail.nextDep = r, Ye.depsTail = r) : Ye.deps = Ye.depsTail = r, Ud(r);
    else if (r.version === -1 && (r.version = this.version, r.nextDep)) {
      const n = r.nextDep;
      n.prevDep = r.prevDep, r.prevDep && (r.prevDep.nextDep = n), r.prevDep = Ye.depsTail, r.nextDep = void 0, Ye.depsTail.nextDep = r, Ye.depsTail = r, Ye.deps === r && (Ye.deps = n);
    }
    return r;
  }
  trigger(e) {
    this.version++, Xa++, this.notify(e);
  }
  notify(e) {
    Lu();
    try {
      for (let r = this.subs; r; r = r.prevSub)
        r.sub.notify() && r.sub.dep.notify();
    } finally {
      Iu();
    }
  }
}
function Ud(t) {
  if (t.dep.sc++, t.sub.flags & 4) {
    const e = t.dep.computed;
    if (e && !t.dep.subs) {
      e.flags |= 20;
      for (let n = e.deps; n; n = n.nextDep)
        Ud(n);
    }
    const r = t.dep.subs;
    r !== t && (t.prevSub = r, r && (r.nextSub = t)), t.dep.subs = t;
  }
}
const Jl = /* @__PURE__ */ new WeakMap(), Dn = /* @__PURE__ */ Symbol(
  ""
), eu = /* @__PURE__ */ Symbol(
  ""
), Za = /* @__PURE__ */ Symbol(
  ""
);
function At(t, e, r) {
  if (sr && Ye) {
    let n = Jl.get(t);
    n || Jl.set(t, n = /* @__PURE__ */ new Map());
    let a = n.get(r);
    a || (n.set(r, a = new Nu()), a.map = n, a.key = r), a.track();
  }
}
function Ir(t, e, r, n, a, i) {
  const s = Jl.get(t);
  if (!s) {
    Xa++;
    return;
  }
  const o = (l) => {
    l && l.trigger();
  };
  if (Lu(), e === "clear")
    s.forEach(o);
  else {
    const l = ke(t), u = l && Ru(r);
    if (l && r === "length") {
      const f = Number(n);
      s.forEach((c, v) => {
        (v === "length" || v === Za || !Cr(v) && v >= f) && o(c);
      });
    } else
      switch ((r !== void 0 || s.has(void 0)) && o(s.get(r)), u && o(s.get(Za)), e) {
        case "add":
          l ? u && o(s.get("length")) : (o(s.get(Dn)), ea(t) && o(s.get(eu)));
          break;
        case "delete":
          l || (o(s.get(Dn)), ea(t) && o(s.get(eu)));
          break;
        case "set":
          ea(t) && o(s.get(Dn));
          break;
      }
  }
  Iu();
}
function zn(t) {
  const e = /* @__PURE__ */ ze(t);
  return e === t ? e : (At(e, "iterate", Za), /* @__PURE__ */ er(t) ? e : e.map(lr));
}
function Fs(t) {
  return At(t = /* @__PURE__ */ ze(t), "iterate", Za), t;
}
function mr(t, e) {
  return /* @__PURE__ */ Vr(t) ? ua(/* @__PURE__ */ An(t) ? lr(e) : e) : lr(e);
}
const ty = {
  __proto__: null,
  [Symbol.iterator]() {
    return ho(this, Symbol.iterator, (t) => mr(this, t));
  },
  concat(...t) {
    return zn(this).concat(
      ...t.map((e) => ke(e) ? zn(e) : e)
    );
  },
  entries() {
    return ho(this, "entries", (t) => (t[1] = mr(this, t[1]), t));
  },
  every(t, e) {
    return Ar(this, "every", t, e, void 0, arguments);
  },
  filter(t, e) {
    return Ar(
      this,
      "filter",
      t,
      e,
      (r) => r.map((n) => mr(this, n)),
      arguments
    );
  },
  find(t, e) {
    return Ar(
      this,
      "find",
      t,
      e,
      (r) => mr(this, r),
      arguments
    );
  },
  findIndex(t, e) {
    return Ar(this, "findIndex", t, e, void 0, arguments);
  },
  findLast(t, e) {
    return Ar(
      this,
      "findLast",
      t,
      e,
      (r) => mr(this, r),
      arguments
    );
  },
  findLastIndex(t, e) {
    return Ar(this, "findLastIndex", t, e, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(t, e) {
    return Ar(this, "forEach", t, e, void 0, arguments);
  },
  includes(...t) {
    return go(this, "includes", t);
  },
  indexOf(...t) {
    return go(this, "indexOf", t);
  },
  join(t) {
    return zn(this).join(t);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...t) {
    return go(this, "lastIndexOf", t);
  },
  map(t, e) {
    return Ar(this, "map", t, e, void 0, arguments);
  },
  pop() {
    return Sa(this, "pop");
  },
  push(...t) {
    return Sa(this, "push", t);
  },
  reduce(t, ...e) {
    return Tf(this, "reduce", t, e);
  },
  reduceRight(t, ...e) {
    return Tf(this, "reduceRight", t, e);
  },
  shift() {
    return Sa(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(t, e) {
    return Ar(this, "some", t, e, void 0, arguments);
  },
  splice(...t) {
    return Sa(this, "splice", t);
  },
  toReversed() {
    return zn(this).toReversed();
  },
  toSorted(t) {
    return zn(this).toSorted(t);
  },
  toSpliced(...t) {
    return zn(this).toSpliced(...t);
  },
  unshift(...t) {
    return Sa(this, "unshift", t);
  },
  values() {
    return ho(this, "values", (t) => mr(this, t));
  }
};
function ho(t, e, r) {
  const n = Fs(t), a = n[e]();
  return n !== t && !/* @__PURE__ */ er(t) && (a._next = a.next, a.next = () => {
    const i = a._next();
    return i.done || (i.value = r(i.value)), i;
  }), a;
}
const ry = Array.prototype;
function Ar(t, e, r, n, a, i) {
  const s = Fs(t), o = s !== t && !/* @__PURE__ */ er(t), l = s[e];
  if (l !== ry[e]) {
    const c = l.apply(t, i);
    return o ? lr(c) : c;
  }
  let u = r;
  s !== t && (o ? u = function(c, v) {
    return r.call(this, mr(t, c), v, t);
  } : r.length > 2 && (u = function(c, v) {
    return r.call(this, c, v, t);
  }));
  const f = l.call(s, u, n);
  return o && a ? a(f) : f;
}
function Tf(t, e, r, n) {
  const a = Fs(t), i = a !== t && !/* @__PURE__ */ er(t);
  let s = r, o = !1;
  a !== t && (i ? (o = n.length === 0, s = function(u, f, c) {
    return o && (o = !1, u = mr(t, u)), r.call(this, u, mr(t, f), c, t);
  }) : r.length > 3 && (s = function(u, f, c) {
    return r.call(this, u, f, c, t);
  }));
  const l = a[e](s, ...n);
  return o ? mr(t, l) : l;
}
function go(t, e, r) {
  const n = /* @__PURE__ */ ze(t);
  At(n, "iterate", Za);
  const a = n[e](...r);
  return (a === -1 || a === !1) && /* @__PURE__ */ zu(r[0]) ? (r[0] = /* @__PURE__ */ ze(r[0]), n[e](...r)) : a;
}
function Sa(t, e, r = []) {
  Fr(), Lu();
  const n = (/* @__PURE__ */ ze(t))[e].apply(t, r);
  return Iu(), zr(), n;
}
const ny = /* @__PURE__ */ ku("__proto__,__v_isRef,__isVue"), Gd = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((t) => t !== "arguments" && t !== "caller").map((t) => Symbol[t]).filter(Cr)
);
function ay(t) {
  Cr(t) || (t = String(t));
  const e = /* @__PURE__ */ ze(this);
  return At(e, "has", t), e.hasOwnProperty(t);
}
class Wd {
  constructor(e = !1, r = !1) {
    this._isReadonly = e, this._isShallow = r;
  }
  get(e, r, n) {
    if (r === "__v_skip") return e.__v_skip;
    const a = this._isReadonly, i = this._isShallow;
    if (r === "__v_isReactive")
      return !a;
    if (r === "__v_isReadonly")
      return a;
    if (r === "__v_isShallow")
      return i;
    if (r === "__v_raw")
      return n === (a ? i ? hy : Zd : i ? Xd : Yd).get(e) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(e) === Object.getPrototypeOf(n) ? e : void 0;
    const s = ke(e);
    if (!a) {
      let l;
      if (s && (l = ty[r]))
        return l;
      if (r === "hasOwnProperty")
        return ay;
    }
    const o = Reflect.get(
      e,
      r,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ Bt(e) ? e : n
    );
    if ((Cr(r) ? Gd.has(r) : ny(r)) || (a || At(e, "get", r), i))
      return o;
    if (/* @__PURE__ */ Bt(o)) {
      const l = s && Ru(r) ? o : o.value;
      return a && Ue(l) ? /* @__PURE__ */ ru(l) : l;
    }
    return Ue(o) ? a ? /* @__PURE__ */ ru(o) : /* @__PURE__ */ Qa(o) : o;
  }
}
class Kd extends Wd {
  constructor(e = !1) {
    super(!1, e);
  }
  set(e, r, n, a) {
    let i = e[r];
    const s = ke(e) && Ru(r);
    if (!this._isShallow) {
      const u = /* @__PURE__ */ Vr(i);
      if (!/* @__PURE__ */ er(n) && !/* @__PURE__ */ Vr(n) && (i = /* @__PURE__ */ ze(i), n = /* @__PURE__ */ ze(n)), !s && /* @__PURE__ */ Bt(i) && !/* @__PURE__ */ Bt(n))
        return u || (i.value = n), !0;
    }
    const o = s ? Number(r) < e.length : Ve(e, r), l = Reflect.set(
      e,
      r,
      n,
      /* @__PURE__ */ Bt(e) ? e : a
    );
    return e === /* @__PURE__ */ ze(a) && l && (o ? wr(n, i) && Ir(e, "set", r, n) : Ir(e, "add", r, n)), l;
  }
  deleteProperty(e, r) {
    const n = Ve(e, r);
    e[r];
    const a = Reflect.deleteProperty(e, r);
    return a && n && Ir(e, "delete", r, void 0), a;
  }
  has(e, r) {
    const n = Reflect.has(e, r);
    return (!Cr(r) || !Gd.has(r)) && At(e, "has", r), n;
  }
  ownKeys(e) {
    return At(
      e,
      "iterate",
      ke(e) ? "length" : Dn
    ), Reflect.ownKeys(e);
  }
}
class iy extends Wd {
  constructor(e = !1) {
    super(!0, e);
  }
  set(e, r) {
    return !0;
  }
  deleteProperty(e, r) {
    return !0;
  }
}
const sy = /* @__PURE__ */ new Kd(), oy = /* @__PURE__ */ new iy(), ly = /* @__PURE__ */ new Kd(!0);
const tu = (t) => t, ki = (t) => Reflect.getPrototypeOf(t);
function uy(t, e, r) {
  return function(...n) {
    const a = this.__v_raw, i = /* @__PURE__ */ ze(a), s = ea(i), o = t === "entries" || t === Symbol.iterator && s, l = t === "keys" && s, u = a[t](...n), f = r ? tu : e ? ua : lr;
    return !e && At(
      i,
      "iterate",
      l ? eu : Dn
    ), Tt(
      // inheriting all iterator properties
      Object.create(u),
      {
        // iterator protocol
        next() {
          const { value: c, done: v } = u.next();
          return v ? { value: c, done: v } : {
            value: o ? [f(c[0]), f(c[1])] : f(c),
            done: v
          };
        }
      }
    );
  };
}
function Bi(t) {
  return function(...e) {
    return t === "delete" ? !1 : t === "clear" ? void 0 : this;
  };
}
function fy(t, e) {
  const r = {
    get(a) {
      const i = this.__v_raw, s = /* @__PURE__ */ ze(i), o = /* @__PURE__ */ ze(a);
      t || (wr(a, o) && At(s, "get", a), At(s, "get", o));
      const { has: l } = ki(s), u = e ? tu : t ? ua : lr;
      if (l.call(s, a))
        return u(i.get(a));
      if (l.call(s, o))
        return u(i.get(o));
      i !== s && i.get(a);
    },
    get size() {
      const a = this.__v_raw;
      return !t && At(/* @__PURE__ */ ze(a), "iterate", Dn), a.size;
    },
    has(a) {
      const i = this.__v_raw, s = /* @__PURE__ */ ze(i), o = /* @__PURE__ */ ze(a);
      return t || (wr(a, o) && At(s, "has", a), At(s, "has", o)), a === o ? i.has(a) : i.has(a) || i.has(o);
    },
    forEach(a, i) {
      const s = this, o = s.__v_raw, l = /* @__PURE__ */ ze(o), u = e ? tu : t ? ua : lr;
      return !t && At(l, "iterate", Dn), o.forEach((f, c) => a.call(i, u(f), u(c), s));
    }
  };
  return Tt(
    r,
    t ? {
      add: Bi("add"),
      set: Bi("set"),
      delete: Bi("delete"),
      clear: Bi("clear")
    } : {
      add(a) {
        const i = /* @__PURE__ */ ze(this), s = ki(i), o = /* @__PURE__ */ ze(a), l = !e && !/* @__PURE__ */ er(a) && !/* @__PURE__ */ Vr(a) ? o : a;
        return s.has.call(i, l) || wr(a, l) && s.has.call(i, a) || wr(o, l) && s.has.call(i, o) || (i.add(l), Ir(i, "add", l, l)), this;
      },
      set(a, i) {
        !e && !/* @__PURE__ */ er(i) && !/* @__PURE__ */ Vr(i) && (i = /* @__PURE__ */ ze(i));
        const s = /* @__PURE__ */ ze(this), { has: o, get: l } = ki(s);
        let u = o.call(s, a);
        u || (a = /* @__PURE__ */ ze(a), u = o.call(s, a));
        const f = l.call(s, a);
        return s.set(a, i), u ? wr(i, f) && Ir(s, "set", a, i) : Ir(s, "add", a, i), this;
      },
      delete(a) {
        const i = /* @__PURE__ */ ze(this), { has: s, get: o } = ki(i);
        let l = s.call(i, a);
        l || (a = /* @__PURE__ */ ze(a), l = s.call(i, a)), o && o.call(i, a);
        const u = i.delete(a);
        return l && Ir(i, "delete", a, void 0), u;
      },
      clear() {
        const a = /* @__PURE__ */ ze(this), i = a.size !== 0, s = a.clear();
        return i && Ir(
          a,
          "clear",
          void 0,
          void 0
        ), s;
      }
    }
  ), [
    "keys",
    "values",
    "entries",
    Symbol.iterator
  ].forEach((a) => {
    r[a] = uy(a, t, e);
  }), r;
}
function _u(t, e) {
  const r = fy(t, e);
  return (n, a, i) => a === "__v_isReactive" ? !t : a === "__v_isReadonly" ? t : a === "__v_raw" ? n : Reflect.get(
    Ve(r, a) && a in n ? r : n,
    a,
    i
  );
}
const cy = {
  get: /* @__PURE__ */ _u(!1, !1)
}, vy = {
  get: /* @__PURE__ */ _u(!1, !0)
}, dy = {
  get: /* @__PURE__ */ _u(!0, !1)
};
const Yd = /* @__PURE__ */ new WeakMap(), Xd = /* @__PURE__ */ new WeakMap(), Zd = /* @__PURE__ */ new WeakMap(), hy = /* @__PURE__ */ new WeakMap();
function gy(t) {
  switch (t) {
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
function Qa(t) {
  return /* @__PURE__ */ Vr(t) ? t : Fu(
    t,
    !1,
    sy,
    cy,
    Yd
  );
}
// @__NO_SIDE_EFFECTS__
function py(t) {
  return Fu(
    t,
    !1,
    ly,
    vy,
    Xd
  );
}
// @__NO_SIDE_EFFECTS__
function ru(t) {
  return Fu(
    t,
    !0,
    oy,
    dy,
    Zd
  );
}
function Fu(t, e, r, n, a) {
  if (!Ue(t) || t.__v_raw && !(e && t.__v_isReactive) || t.__v_skip || !Object.isExtensible(t))
    return t;
  const i = a.get(t);
  if (i)
    return i;
  const s = gy(Vp(t));
  if (s === 0)
    return t;
  const o = new Proxy(
    t,
    s === 2 ? n : r
  );
  return a.set(t, o), o;
}
// @__NO_SIDE_EFFECTS__
function An(t) {
  return /* @__PURE__ */ Vr(t) ? /* @__PURE__ */ An(t.__v_raw) : !!(t && t.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function Vr(t) {
  return !!(t && t.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function er(t) {
  return !!(t && t.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function zu(t) {
  return t ? !!t.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function ze(t) {
  const e = t && t.__v_raw;
  return e ? /* @__PURE__ */ ze(e) : t;
}
function yy(t) {
  return !Ve(t, "__v_skip") && Object.isExtensible(t) && Ld(t, "__v_skip", !0), t;
}
const lr = (t) => Ue(t) ? /* @__PURE__ */ Qa(t) : t, ua = (t) => Ue(t) ? /* @__PURE__ */ ru(t) : t;
// @__NO_SIDE_EFFECTS__
function Bt(t) {
  return t ? t.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function pt(t) {
  return my(t, !1);
}
function my(t, e) {
  return /* @__PURE__ */ Bt(t) ? t : new by(t, e);
}
class by {
  constructor(e, r) {
    this.dep = new Nu(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = r ? e : /* @__PURE__ */ ze(e), this._value = r ? e : lr(e), this.__v_isShallow = r;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(e) {
    const r = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ er(e) || /* @__PURE__ */ Vr(e);
    e = n ? e : /* @__PURE__ */ ze(e), wr(e, r) && (this._rawValue = e, this._value = n ? e : lr(e), this.dep.trigger());
  }
}
function wy(t) {
  return /* @__PURE__ */ Bt(t) ? t.value : t;
}
const xy = {
  get: (t, e, r) => e === "__v_raw" ? t : wy(Reflect.get(t, e, r)),
  set: (t, e, r, n) => {
    const a = t[e];
    return /* @__PURE__ */ Bt(a) && !/* @__PURE__ */ Bt(r) ? (a.value = r, !0) : Reflect.set(t, e, r, n);
  }
};
function Qd(t) {
  return /* @__PURE__ */ An(t) ? t : new Proxy(t, xy);
}
class Ey {
  constructor(e, r, n) {
    this.fn = e, this.setter = r, this._value = void 0, this.dep = new Nu(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Xa - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !r, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    Ye !== this)
      return zd(this, !0), !0;
  }
  get value() {
    const e = this.dep.track();
    return $d(this), e && (e.version = this.dep.version), this._value;
  }
  set value(e) {
    this.setter && this.setter(e);
  }
}
// @__NO_SIDE_EFFECTS__
function Cy(t, e, r = !1) {
  let n, a;
  return Re(t) ? n = t : (n = t.get, a = t.set), new Ey(n, a, r);
}
const Ri = {}, ls = /* @__PURE__ */ new WeakMap();
let wn;
function Ty(t, e = !1, r = wn) {
  if (r) {
    let n = ls.get(r);
    n || ls.set(r, n = []), n.push(t);
  }
}
function Sy(t, e, r = Ge) {
  const { immediate: n, deep: a, once: i, scheduler: s, augmentJob: o, call: l } = r, u = (w) => a ? w : /* @__PURE__ */ er(w) || a === !1 || a === 0 ? Or(w, 1) : Or(w);
  let f, c, v, d, h = !1, y = !1;
  if (/* @__PURE__ */ Bt(t) ? (c = () => t.value, h = /* @__PURE__ */ er(t)) : /* @__PURE__ */ An(t) ? (c = () => u(t), h = !0) : ke(t) ? (y = !0, h = t.some((w) => /* @__PURE__ */ An(w) || /* @__PURE__ */ er(w)), c = () => t.map((w) => {
    if (/* @__PURE__ */ Bt(w))
      return w.value;
    if (/* @__PURE__ */ An(w))
      return u(w);
    if (Re(w))
      return l ? l(w, 2) : w();
  })) : Re(t) ? e ? c = l ? () => l(t, 2) : t : c = () => {
    if (v) {
      Fr();
      try {
        v();
      } finally {
        zr();
      }
    }
    const w = wn;
    wn = f;
    try {
      return l ? l(t, 3, [d]) : t(d);
    } finally {
      wn = w;
    }
  } : c = Er, e && a) {
    const w = c, E = a === !0 ? 1 / 0 : a;
    c = () => Or(w(), E);
  }
  const g = jp(), p = () => {
    f.stop(), g && g.active && Bu(g.effects, f);
  };
  if (i && e) {
    const w = e;
    e = (...E) => {
      const T = w(...E);
      return p(), T;
    };
  }
  let m = y ? new Array(t.length).fill(Ri) : Ri;
  const b = (w) => {
    if (!(!(f.flags & 1) || !f.dirty && !w))
      if (e) {
        const E = f.run();
        if (w || a || h || (y ? E.some((T, x) => wr(T, m[x])) : wr(E, m))) {
          v && v();
          const T = wn;
          wn = f;
          try {
            const x = [
              E,
              // pass undefined as the old value when it's changed for the first time
              m === Ri ? void 0 : y && m[0] === Ri ? [] : m,
              d
            ];
            m = E, l ? l(e, 3, x) : (
              // @ts-expect-error
              e(...x)
            );
          } finally {
            wn = T;
          }
        }
      } else
        f.run();
  };
  return o && o(b), f = new _d(c), f.scheduler = s ? () => s(b, !1) : b, d = (w) => Ty(w, !1, f), v = f.onStop = () => {
    const w = ls.get(f);
    if (w) {
      if (l)
        l(w, 4);
      else
        for (const E of w) E();
      ls.delete(f);
    }
  }, e ? n ? b(!0) : m = f.run() : s ? s(b.bind(null, !0), !0) : f.run(), p.pause = f.pause.bind(f), p.resume = f.resume.bind(f), p.stop = p, p;
}
function Or(t, e = 1 / 0, r) {
  if (e <= 0 || !Ue(t) || t.__v_skip || (r = r || /* @__PURE__ */ new Map(), (r.get(t) || 0) >= e))
    return t;
  if (r.set(t, e), e--, /* @__PURE__ */ Bt(t))
    Or(t.value, e, r);
  else if (ke(t))
    for (let n = 0; n < t.length; n++)
      Or(t[n], e, r);
  else if (Ls(t) || ea(t))
    t.forEach((n) => {
      Or(n, e, r);
    });
  else if (Md(t)) {
    for (const n in t)
      Or(t[n], e, r);
    for (const n of Object.getOwnPropertySymbols(t))
      Object.prototype.propertyIsEnumerable.call(t, n) && Or(t[n], e, r);
  }
  return t;
}
/**
* @vue/runtime-core v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function di(t, e, r, n) {
  try {
    return n ? t(...n) : t();
  } catch (a) {
    zs(a, e, r);
  }
}
function ur(t, e, r, n) {
  if (Re(t)) {
    const a = di(t, e, r, n);
    return a && Bd(a) && a.catch((i) => {
      zs(i, e, r);
    }), a;
  }
  if (ke(t)) {
    const a = [];
    for (let i = 0; i < t.length; i++)
      a.push(ur(t[i], e, r, n));
    return a;
  }
}
function zs(t, e, r, n = !0) {
  const a = e ? e.vnode : null, { errorHandler: i, throwUnhandledErrorInProduction: s } = e && e.appContext.config || Ge;
  if (e) {
    let o = e.parent;
    const l = e.proxy, u = `https://vuejs.org/error-reference/#runtime-${r}`;
    for (; o; ) {
      const f = o.ec;
      if (f) {
        for (let c = 0; c < f.length; c++)
          if (f[c](t, l, u) === !1)
            return;
      }
      o = o.parent;
    }
    if (i) {
      Fr(), di(i, null, 10, [
        t,
        l,
        u
      ]), zr();
      return;
    }
  }
  Py(t, r, a, n, s);
}
function Py(t, e, r, n = !0, a = !1) {
  if (a)
    throw t;
  console.error(t);
}
const _t = [];
let pr = -1;
const ta = [];
let Xr = null, Yn = 0;
const jd = /* @__PURE__ */ Promise.resolve();
let us = null;
function Jd(t) {
  const e = us || jd;
  return t ? e.then(this ? t.bind(this) : t) : e;
}
function Dy(t) {
  let e = pr + 1, r = _t.length;
  for (; e < r; ) {
    const n = e + r >>> 1, a = _t[n], i = ja(a);
    i < t || i === t && a.flags & 2 ? e = n + 1 : r = n;
  }
  return e;
}
function Vu(t) {
  if (!(t.flags & 1)) {
    const e = ja(t), r = _t[_t.length - 1];
    !r || // fast path when the job id is larger than the tail
    !(t.flags & 2) && e >= ja(r) ? _t.push(t) : _t.splice(Dy(e), 0, t), t.flags |= 1, eh();
  }
}
function eh() {
  us || (us = jd.then(rh));
}
function Ay(t) {
  ke(t) ? ta.push(...t) : Xr && t.id === -1 ? Xr.splice(Yn + 1, 0, t) : t.flags & 1 || (ta.push(t), t.flags |= 1), eh();
}
function Sf(t, e, r = pr + 1) {
  for (; r < _t.length; r++) {
    const n = _t[r];
    if (n && n.flags & 2) {
      if (t && n.id !== t.uid)
        continue;
      _t.splice(r, 1), r--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function th(t) {
  if (ta.length) {
    const e = [...new Set(ta)].sort(
      (r, n) => ja(r) - ja(n)
    );
    if (ta.length = 0, Xr) {
      Xr.push(...e);
      return;
    }
    for (Xr = e, Yn = 0; Yn < Xr.length; Yn++) {
      const r = Xr[Yn];
      r.flags & 4 && (r.flags &= -2), r.flags & 8 || r(), r.flags &= -2;
    }
    Xr = null, Yn = 0;
  }
}
const ja = (t) => t.id == null ? t.flags & 2 ? -1 : 1 / 0 : t.id;
function rh(t) {
  try {
    for (pr = 0; pr < _t.length; pr++) {
      const e = _t[pr];
      e && !(e.flags & 8) && (e.flags & 4 && (e.flags &= -2), di(
        e,
        e.i,
        e.i ? 15 : 14
      ), e.flags & 4 || (e.flags &= -2));
    }
  } finally {
    for (; pr < _t.length; pr++) {
      const e = _t[pr];
      e && (e.flags &= -2);
    }
    pr = -1, _t.length = 0, th(), us = null, (_t.length || ta.length) && rh();
  }
}
let Kt = null, nh = null;
function fs(t) {
  const e = Kt;
  return Kt = t, nh = t && t.type.__scopeId || null, e;
}
function ky(t, e = Kt, r) {
  if (!e || t._n)
    return t;
  const n = (...a) => {
    n._d && _f(-1);
    const i = fs(e), s = kn.length;
    let o;
    try {
      o = t(...a);
    } finally {
      for (let l = kn.length; l > s; l--) Bh();
      fs(i), n._d && _f(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function Pa(t, e) {
  if (Kt === null)
    return t;
  const r = Hs(Kt), n = t.dirs || (t.dirs = []);
  for (let a = 0; a < e.length; a++) {
    let [i, s, o, l = Ge] = e[a];
    i && (Re(i) && (i = {
      mounted: i,
      updated: i
    }), i.deep && Or(s), n.push({
      dir: i,
      instance: r,
      value: s,
      oldValue: void 0,
      arg: o,
      modifiers: l
    }));
  }
  return t;
}
function pn(t, e, r, n) {
  const a = t.dirs, i = e && e.dirs;
  for (let s = 0; s < a.length; s++) {
    const o = a[s];
    i && (o.oldValue = i[s].value);
    let l = o.dir[n];
    l && (Fr(), ur(l, r, 8, [
      t.el,
      o,
      t,
      e
    ]), zr());
  }
}
function By(t, e) {
  if (kt) {
    let r = kt.provides;
    const n = kt.parent && kt.parent.provides;
    n === r && (r = kt.provides = Object.create(n)), r[t] = e;
  }
}
function Ki(t, e, r = !1) {
  const n = Am();
  if (n || ra) {
    let a = ra ? ra._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (a && t in a)
      return a[t];
    if (arguments.length > 1)
      return r && Re(e) ? e.call(n && n.proxy) : e;
  }
}
const Ry = /* @__PURE__ */ Symbol.for("v-scx"), My = () => Ki(Ry);
function Yi(t, e, r) {
  return ah(t, e, r);
}
function ah(t, e, r = Ge) {
  const { immediate: n, deep: a, flush: i, once: s } = r, o = Tt({}, r), l = e && n || !e && i !== "post";
  let u;
  if (ei) {
    if (i === "sync") {
      const d = My();
      u = d.__watcherHandles || (d.__watcherHandles = []);
    } else if (!l) {
      const d = () => {
      };
      return d.stop = Er, d.resume = Er, d.pause = Er, d;
    }
  }
  const f = kt;
  o.call = (d, h, y) => ur(d, f, h, y);
  let c = !1;
  i === "post" ? o.scheduler = (d) => {
    Ht(d, f && f.suspense);
  } : i !== "sync" && (c = !0, o.scheduler = (d, h) => {
    h ? d() : Vu(d);
  }), o.augmentJob = (d) => {
    e && (d.flags |= 4), c && (d.flags |= 2, f && (d.id = f.uid, d.i = f));
  };
  const v = Sy(t, e, o);
  return ei && (u ? u.push(v) : l && v()), v;
}
function Ly(t, e, r) {
  const n = this.proxy, a = at(t) ? t.includes(".") ? ih(n, t) : () => n[t] : t.bind(n, n);
  let i;
  Re(e) ? i = e : (i = e.handler, r = e);
  const s = hi(this), o = ah(a, i.bind(n), r);
  return s(), o;
}
function ih(t, e) {
  const r = e.split(".");
  return () => {
    let n = t;
    for (let a = 0; a < r.length && n; a++)
      n = n[r[a]];
    return n;
  };
}
const Iy = /* @__PURE__ */ Symbol("_vte"), Oy = (t) => t.__isTeleport, po = /* @__PURE__ */ Symbol("_leaveCb");
function qu(t, e) {
  t.shapeFlag & 6 && t.component ? (t.transition = e, qu(t.component.subTree, e)) : t.shapeFlag & 128 ? (t.ssContent.transition = e.clone(t.ssContent), t.ssFallback.transition = e.clone(t.ssFallback)) : t.transition = e;
}
// @__NO_SIDE_EFFECTS__
function sh(t, e) {
  return Re(t) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    Tt({ name: t.name }, e, { setup: t })
  ) : t;
}
function oh(t) {
  t.ids = [t.ids[0] + t.ids[2]++ + "-", 0, 0];
}
function Pf(t, e) {
  let r;
  return !!((r = Object.getOwnPropertyDescriptor(t, e)) && !r.configurable);
}
const cs = /* @__PURE__ */ new WeakMap();
function Ha(t, e, r, n, a = !1) {
  if (ke(t)) {
    t.forEach(
      (y, g) => Ha(
        y,
        e && (ke(e) ? e[g] : e),
        r,
        n,
        a
      )
    );
    return;
  }
  if (Ua(n) && !a) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Ha(t, e, r, n.component.subTree);
    return;
  }
  const i = n.shapeFlag & 4 ? Hs(n.component) : n.el, s = a ? null : i, { i: o, r: l } = t, u = e && e.r, f = o.refs === Ge ? o.refs = {} : o.refs, c = o.setupState, v = /* @__PURE__ */ ze(c), d = c === Ge ? kd : (y) => Pf(f, y) ? !1 : Ve(v, y), h = (y, g) => !(g && Pf(f, g));
  if (u != null && u !== l) {
    if (Df(e), at(u))
      f[u] = null, d(u) && (c[u] = null);
    else if (/* @__PURE__ */ Bt(u)) {
      const y = e;
      h(u, y.k) && (u.value = null), y.k && (f[y.k] = null);
    }
  }
  if (Re(l))
    di(l, o, 12, [s, f]);
  else {
    const y = at(l), g = /* @__PURE__ */ Bt(l);
    if (y || g) {
      const p = () => {
        if (t.f) {
          const m = y ? d(l) ? c[l] : f[l] : h() || !t.k ? l.value : f[t.k];
          if (a)
            ke(m) && Bu(m, i);
          else if (ke(m))
            m.includes(i) || m.push(i);
          else if (y)
            f[l] = [i], d(l) && (c[l] = f[l]);
          else {
            const b = [i];
            h(l, t.k) && (l.value = b), t.k && (f[t.k] = b);
          }
        } else y ? (f[l] = s, d(l) && (c[l] = s)) : g && (h(l, t.k) && (l.value = s), t.k && (f[t.k] = s));
      };
      if (s) {
        const m = () => {
          p(), cs.delete(t);
        };
        m.id = -1, cs.set(t, m), Ht(m, r);
      } else
        Df(t), p();
    }
  }
}
function Df(t) {
  const e = cs.get(t);
  e && (e.flags |= 8, cs.delete(t));
}
_s().requestIdleCallback;
_s().cancelIdleCallback;
const Ua = (t) => !!t.type.__asyncLoader, lh = (t) => t.type.__isKeepAlive;
function Ny(t, e) {
  uh(t, "a", e);
}
function _y(t, e) {
  uh(t, "da", e);
}
function uh(t, e, r = kt) {
  const n = t.__wdc || (t.__wdc = () => {
    let a = r;
    for (; a; ) {
      if (a.isDeactivated)
        return;
      a = a.parent;
    }
    return t();
  });
  if (Vs(e, n, r), r) {
    let a = r.parent;
    for (; a && a.parent; )
      lh(a.parent.vnode) && Fy(n, e, r, a), a = a.parent;
  }
}
function Fy(t, e, r, n) {
  const a = Vs(
    e,
    t,
    n,
    !0
    /* prepend */
  );
  vh(() => {
    Bu(n[e], a);
  }, r);
}
function Vs(t, e, r = kt, n = !1) {
  if (r) {
    const a = r[t] || (r[t] = []), i = e.__weh || (e.__weh = (...s) => {
      Fr();
      const o = hi(r), l = ur(e, r, t, s);
      return o(), zr(), l;
    });
    return n ? a.unshift(i) : a.push(i), i;
  }
}
const Hr = (t) => (e, r = kt) => {
  (!ei || t === "sp") && Vs(t, (...n) => e(...n), r);
}, zy = Hr("bm"), fh = Hr("m"), Vy = Hr(
  "bu"
), qy = Hr("u"), ch = Hr(
  "bum"
), vh = Hr("um"), $y = Hr(
  "sp"
), Hy = Hr("rtg"), Uy = Hr("rtc");
function Gy(t, e = kt) {
  Vs("ec", t, e);
}
const Wy = "components";
function Ky(t, e) {
  return Xy(Wy, t, !0, e) || t;
}
const Yy = /* @__PURE__ */ Symbol.for("v-ndc");
function Xy(t, e, r = !0, n = !1) {
  const a = Kt || kt;
  if (a) {
    const i = a.type;
    {
      const o = Lm(
        i,
        !1
      );
      if (o && (o === e || o === Ft(e) || o === Os(Ft(e))))
        return i;
    }
    const s = (
      // local registration
      // check instance[type] first which is resolved for options API
      Af(a[t] || i[t], e) || // global registration
      Af(a.appContext[t], e)
    );
    return !s && n ? i : s;
  }
}
function Af(t, e) {
  return t && (t[e] || t[Ft(e)] || t[Os(Ft(e))]);
}
function Xi(t, e, r, n) {
  let a;
  const i = r, s = ke(t);
  if (s || at(t)) {
    const o = s && /* @__PURE__ */ An(t);
    let l = !1, u = !1;
    o && (l = !/* @__PURE__ */ er(t), u = /* @__PURE__ */ Vr(t), t = Fs(t)), a = new Array(t.length);
    for (let f = 0, c = t.length; f < c; f++)
      a[f] = e(
        l ? u ? ua(lr(t[f])) : lr(t[f]) : t[f],
        f,
        void 0,
        i
      );
  } else if (typeof t == "number") {
    a = new Array(t);
    for (let o = 0; o < t; o++)
      a[o] = e(o + 1, o, void 0, i);
  } else if (Ue(t))
    if (t[Symbol.iterator])
      a = Array.from(
        t,
        (o, l) => e(o, l, void 0, i)
      );
    else {
      const o = Object.keys(t);
      a = new Array(o.length);
      for (let l = 0, u = o.length; l < u; l++) {
        const f = o[l];
        a[l] = e(t[f], f, l, i);
      }
    }
  else
    a = [];
  return a;
}
const nu = (t) => t ? Ih(t) ? Hs(t) : nu(t.parent) : null, Ga = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ Tt(/* @__PURE__ */ Object.create(null), {
    $: (t) => t,
    $el: (t) => t.vnode.el,
    $data: (t) => t.data,
    $props: (t) => t.props,
    $attrs: (t) => t.attrs,
    $slots: (t) => t.slots,
    $refs: (t) => t.refs,
    $parent: (t) => nu(t.parent),
    $root: (t) => nu(t.root),
    $host: (t) => t.ce,
    $emit: (t) => t.emit,
    $options: (t) => hh(t),
    $forceUpdate: (t) => t.f || (t.f = () => {
      Vu(t.update);
    }),
    $nextTick: (t) => t.n || (t.n = Jd.bind(t.proxy)),
    $watch: (t) => Ly.bind(t)
  })
), yo = (t, e) => t !== Ge && !t.__isScriptSetup && Ve(t, e), Zy = {
  get({ _: t }, e) {
    if (e === "__v_skip")
      return !0;
    const { ctx: r, setupState: n, data: a, props: i, accessCache: s, type: o, appContext: l } = t;
    if (e[0] !== "$") {
      const v = s[e];
      if (v !== void 0)
        switch (v) {
          case 1:
            return n[e];
          case 2:
            return a[e];
          case 4:
            return r[e];
          case 3:
            return i[e];
        }
      else {
        if (yo(n, e))
          return s[e] = 1, n[e];
        if (a !== Ge && Ve(a, e))
          return s[e] = 2, a[e];
        if (Ve(i, e))
          return s[e] = 3, i[e];
        if (r !== Ge && Ve(r, e))
          return s[e] = 4, r[e];
        au && (s[e] = 0);
      }
    }
    const u = Ga[e];
    let f, c;
    if (u)
      return e === "$attrs" && At(t.attrs, "get", ""), u(t);
    if (
      // css module (injected by vue-loader)
      (f = o.__cssModules) && (f = f[e])
    )
      return f;
    if (r !== Ge && Ve(r, e))
      return s[e] = 4, r[e];
    if (
      // global properties
      c = l.config.globalProperties, Ve(c, e)
    )
      return c[e];
  },
  set({ _: t }, e, r) {
    const { data: n, setupState: a, ctx: i } = t;
    return yo(a, e) ? (a[e] = r, !0) : n !== Ge && Ve(n, e) ? (n[e] = r, !0) : Ve(t.props, e) || e[0] === "$" && e.slice(1) in t ? !1 : (i[e] = r, !0);
  },
  has({
    _: { data: t, setupState: e, accessCache: r, ctx: n, appContext: a, props: i, type: s }
  }, o) {
    let l;
    return !!(r[o] || t !== Ge && o[0] !== "$" && Ve(t, o) || yo(e, o) || Ve(i, o) || Ve(n, o) || Ve(Ga, o) || Ve(a.config.globalProperties, o) || (l = s.__cssModules) && l[o]);
  },
  defineProperty(t, e, r) {
    return r.get != null ? t._.accessCache[e] = 0 : Ve(r, "value") && this.set(t, e, r.value, null), Reflect.defineProperty(t, e, r);
  }
};
function kf(t) {
  return ke(t) ? t.reduce(
    (e, r) => (e[r] = null, e),
    {}
  ) : t;
}
let au = !0;
function Qy(t) {
  const e = hh(t), r = t.proxy, n = t.ctx;
  au = !1, e.beforeCreate && Bf(e.beforeCreate, t, "bc");
  const {
    // state
    data: a,
    computed: i,
    methods: s,
    watch: o,
    provide: l,
    inject: u,
    // lifecycle
    created: f,
    beforeMount: c,
    mounted: v,
    beforeUpdate: d,
    updated: h,
    activated: y,
    deactivated: g,
    beforeDestroy: p,
    beforeUnmount: m,
    destroyed: b,
    unmounted: w,
    render: E,
    renderTracked: T,
    renderTriggered: x,
    errorCaptured: S,
    serverPrefetch: D,
    // public API
    expose: A,
    inheritAttrs: k,
    // assets
    components: R,
    directives: M,
    filters: I
  } = e;
  if (u && jy(u, n, null), s)
    for (const L in s) {
      const F = s[L];
      Re(F) && (n[L] = F.bind(r));
    }
  if (a) {
    const L = a.call(r, r);
    Ue(L) && (t.data = /* @__PURE__ */ Qa(L));
  }
  if (au = !0, i)
    for (const L in i) {
      const F = i[L], H = Re(F) ? F.bind(r, r) : Re(F.get) ? F.get.bind(r, r) : Er, V = !Re(F) && Re(F.set) ? F.set.bind(r) : Er, O = Om({
        get: H,
        set: V
      });
      Object.defineProperty(n, L, {
        enumerable: !0,
        configurable: !0,
        get: () => O.value,
        set: ($) => O.value = $
      });
    }
  if (o)
    for (const L in o)
      dh(o[L], n, r, L);
  if (l) {
    const L = Re(l) ? l.call(r) : l;
    Reflect.ownKeys(L).forEach((F) => {
      By(F, L[F]);
    });
  }
  f && Bf(f, t, "c");
  function N(L, F) {
    ke(F) ? F.forEach((H) => L(H.bind(r))) : F && L(F.bind(r));
  }
  if (N(zy, c), N(fh, v), N(Vy, d), N(qy, h), N(Ny, y), N(_y, g), N(Gy, S), N(Uy, T), N(Hy, x), N(ch, m), N(vh, w), N($y, D), ke(A))
    if (A.length) {
      const L = t.exposed || (t.exposed = {});
      A.forEach((F) => {
        Object.defineProperty(L, F, {
          get: () => r[F],
          set: (H) => r[F] = H,
          enumerable: !0
        });
      });
    } else t.exposed || (t.exposed = {});
  E && t.render === Er && (t.render = E), k != null && (t.inheritAttrs = k), R && (t.components = R), M && (t.directives = M), D && oh(t);
}
function jy(t, e, r = Er) {
  ke(t) && (t = iu(t));
  for (const n in t) {
    const a = t[n];
    let i;
    Ue(a) ? "default" in a ? i = Ki(
      a.from || n,
      a.default,
      !0
    ) : i = Ki(a.from || n) : i = Ki(a), /* @__PURE__ */ Bt(i) ? Object.defineProperty(e, n, {
      enumerable: !0,
      configurable: !0,
      get: () => i.value,
      set: (s) => i.value = s
    }) : e[n] = i;
  }
}
function Bf(t, e, r) {
  ur(
    ke(t) ? t.map((n) => n.bind(e.proxy)) : t.bind(e.proxy),
    e,
    r
  );
}
function dh(t, e, r, n) {
  let a = n.includes(".") ? ih(r, n) : () => r[n];
  if (at(t)) {
    const i = e[t];
    Re(i) && Yi(a, i);
  } else if (Re(t))
    Yi(a, t.bind(r));
  else if (Ue(t))
    if (ke(t))
      t.forEach((i) => dh(i, e, r, n));
    else {
      const i = Re(t.handler) ? t.handler.bind(r) : e[t.handler];
      Re(i) && Yi(a, i, t);
    }
}
function hh(t) {
  const e = t.type, { mixins: r, extends: n } = e, {
    mixins: a,
    optionsCache: i,
    config: { optionMergeStrategies: s }
  } = t.appContext, o = i.get(e);
  let l;
  return o ? l = o : !a.length && !r && !n ? l = e : (l = {}, a.length && a.forEach(
    (u) => vs(l, u, s, !0)
  ), vs(l, e, s)), Ue(e) && i.set(e, l), l;
}
function vs(t, e, r, n = !1) {
  const { mixins: a, extends: i } = e;
  i && vs(t, i, r, !0), a && a.forEach(
    (s) => vs(t, s, r, !0)
  );
  for (const s in e)
    if (!(n && s === "expose")) {
      const o = Jy[s] || r && r[s];
      t[s] = o ? o(t[s], e[s]) : e[s];
    }
  return t;
}
const Jy = {
  data: Rf,
  props: Mf,
  emits: Mf,
  // objects
  methods: Oa,
  computed: Oa,
  // lifecycle
  beforeCreate: It,
  created: It,
  beforeMount: It,
  mounted: It,
  beforeUpdate: It,
  updated: It,
  beforeDestroy: It,
  beforeUnmount: It,
  destroyed: It,
  unmounted: It,
  activated: It,
  deactivated: It,
  errorCaptured: It,
  serverPrefetch: It,
  // assets
  components: Oa,
  directives: Oa,
  // watch
  watch: tm,
  // provide / inject
  provide: Rf,
  inject: em
};
function Rf(t, e) {
  return e ? t ? function() {
    return Tt(
      Re(t) ? t.call(this, this) : t,
      Re(e) ? e.call(this, this) : e
    );
  } : e : t;
}
function em(t, e) {
  return Oa(iu(t), iu(e));
}
function iu(t) {
  if (ke(t)) {
    const e = {};
    for (let r = 0; r < t.length; r++)
      e[t[r]] = t[r];
    return e;
  }
  return t;
}
function It(t, e) {
  return t ? [...new Set([].concat(t, e))] : e;
}
function Oa(t, e) {
  return t ? Tt(/* @__PURE__ */ Object.create(null), t, e) : e;
}
function Mf(t, e) {
  return t ? ke(t) && ke(e) ? [.../* @__PURE__ */ new Set([...t, ...e])] : Tt(
    /* @__PURE__ */ Object.create(null),
    kf(t),
    kf(e ?? {})
  ) : e;
}
function tm(t, e) {
  if (!t) return e;
  if (!e) return t;
  const r = Tt(/* @__PURE__ */ Object.create(null), t);
  for (const n in e)
    r[n] = It(t[n], e[n]);
  return r;
}
function gh() {
  return {
    app: null,
    config: {
      isNativeTag: kd,
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
let rm = 0;
function nm(t, e) {
  return function(n, a = null) {
    Re(n) || (n = Tt({}, n)), a != null && !Ue(a) && (a = null);
    const i = gh(), s = /* @__PURE__ */ new WeakSet(), o = [];
    let l = !1;
    const u = i.app = {
      _uid: rm++,
      _component: n,
      _props: a,
      _container: null,
      _context: i,
      _instance: null,
      version: Nm,
      get config() {
        return i.config;
      },
      set config(f) {
      },
      use(f, ...c) {
        return s.has(f) || (f && Re(f.install) ? (s.add(f), f.install(u, ...c)) : Re(f) && (s.add(f), f(u, ...c))), u;
      },
      mixin(f) {
        return i.mixins.includes(f) || i.mixins.push(f), u;
      },
      component(f, c) {
        return c ? (i.components[f] = c, u) : i.components[f];
      },
      directive(f, c) {
        return c ? (i.directives[f] = c, u) : i.directives[f];
      },
      mount(f, c, v) {
        if (!l) {
          const d = u._ceVNode || _r(n, a);
          return d.appContext = i, v === !0 ? v = "svg" : v === !1 && (v = void 0), t(d, f, v), l = !0, u._container = f, f.__vue_app__ = u, Hs(d.component);
        }
      },
      onUnmount(f) {
        o.push(f);
      },
      unmount() {
        l && (ur(
          o,
          u._instance,
          16
        ), t(null, u._container), delete u._container.__vue_app__);
      },
      provide(f, c) {
        return i.provides[f] = c, u;
      },
      runWithContext(f) {
        const c = ra;
        ra = u;
        try {
          return f();
        } finally {
          ra = c;
        }
      }
    };
    return u;
  };
}
let ra = null;
const am = (t, e) => e === "modelValue" || e === "model-value" ? t.modelModifiers : t[`${e}Modifiers`] || t[`${Ft(e)}Modifiers`] || t[`${Nn(e)}Modifiers`];
function im(t, e, ...r) {
  if (t.isUnmounted) return;
  const n = t.vnode.props || Ge;
  let a = r;
  const i = e.startsWith("update:"), s = i && am(n, e.slice(7));
  s && (s.trim && (a = r.map((f) => at(f) ? f.trim() : f)), s.number && (a = r.map(Ns)));
  let o, l = n[o = fo(e)] || // also try camelCase event handler (#2249)
  n[o = fo(Ft(e))];
  !l && i && (l = n[o = fo(Nn(e))]), l && ur(
    l,
    t,
    6,
    a
  );
  const u = n[o + "Once"];
  if (u) {
    if (!t.emitted)
      t.emitted = {};
    else if (t.emitted[o])
      return;
    t.emitted[o] = !0, ur(
      u,
      t,
      6,
      a
    );
  }
}
const sm = /* @__PURE__ */ new WeakMap();
function ph(t, e, r = !1) {
  const n = r ? sm : e.emitsCache, a = n.get(t);
  if (a !== void 0)
    return a;
  const i = t.emits;
  let s = {}, o = !1;
  if (!Re(t)) {
    const l = (u) => {
      const f = ph(u, e, !0);
      f && (o = !0, Tt(s, f));
    };
    !r && e.mixins.length && e.mixins.forEach(l), t.extends && l(t.extends), t.mixins && t.mixins.forEach(l);
  }
  return !i && !o ? (Ue(t) && n.set(t, null), null) : (ke(i) ? i.forEach((l) => s[l] = null) : Tt(s, i), Ue(t) && n.set(t, s), s);
}
function qs(t, e) {
  return !t || !Rs(e) ? !1 : (e = e.slice(2), e = e === "Once" ? e : e.replace(/Once$/, ""), Ve(t, e[0].toLowerCase() + e.slice(1)) || Ve(t, Nn(e)) || Ve(t, e));
}
function Lf(t) {
  const {
    type: e,
    vnode: r,
    proxy: n,
    withProxy: a,
    propsOptions: [i],
    slots: s,
    attrs: o,
    emit: l,
    render: u,
    renderCache: f,
    props: c,
    data: v,
    setupState: d,
    ctx: h,
    inheritAttrs: y
  } = t, g = fs(t);
  let p, m;
  try {
    if (r.shapeFlag & 4) {
      const w = a || n, E = w;
      p = br(
        u.call(
          E,
          w,
          f,
          c,
          d,
          v,
          h
        )
      ), m = o;
    } else {
      const w = e;
      p = br(
        w.length > 1 ? w(
          c,
          { attrs: o, slots: s, emit: l }
        ) : w(
          c,
          null
        )
      ), m = e.props ? o : om(o);
    }
  } catch (w) {
    kn.length = 0, zs(w, t, 1), p = _r(rn);
  }
  let b = p;
  if (m && y !== !1) {
    const w = Object.keys(m), { shapeFlag: E } = b;
    w.length && E & 7 && (i && w.some(Ms) && (m = lm(
      m,
      i
    )), b = fa(b, m, !1, !0));
  }
  return r.dirs && (b = fa(b, null, !1, !0), b.dirs = b.dirs ? b.dirs.concat(r.dirs) : r.dirs), r.transition && qu(b, r.transition), p = b, fs(g), p;
}
const om = (t) => {
  let e;
  for (const r in t)
    (r === "class" || r === "style" || Rs(r)) && ((e || (e = {}))[r] = t[r]);
  return e;
}, lm = (t, e) => {
  const r = {};
  for (const n in t)
    (!Ms(n) || !(n.slice(9) in e)) && (r[n] = t[n]);
  return r;
};
function um(t, e, r) {
  const { props: n, children: a, component: i } = t, { props: s, children: o, patchFlag: l } = e, u = i.emitsOptions;
  if (e.dirs || e.transition)
    return !0;
  if (r && l >= 0) {
    if (l & 1024)
      return !0;
    if (l & 16)
      return n ? If(n, s, u) : !!s;
    if (l & 8) {
      const f = e.dynamicProps;
      for (let c = 0; c < f.length; c++) {
        const v = f[c];
        if (yh(s, n, v) && !qs(u, v))
          return !0;
      }
    }
  } else
    return (a || o) && (!o || !o.$stable) ? !0 : n === s ? !1 : n ? s ? If(n, s, u) : !0 : !!s;
  return !1;
}
function If(t, e, r) {
  const n = Object.keys(e);
  if (n.length !== Object.keys(t).length)
    return !0;
  for (let a = 0; a < n.length; a++) {
    const i = n[a];
    if (yh(e, t, i) && !qs(r, i))
      return !0;
  }
  return !1;
}
function yh(t, e, r) {
  const n = t[r], a = e[r];
  return r === "style" && Ue(n) && Ue(a) ? !vi(n, a) : n !== a;
}
function fm({ vnode: t, parent: e, suspense: r }, n) {
  for (; e; ) {
    const a = e.subTree;
    if (a.suspense && a.suspense.activeBranch === t && (a.suspense.vnode.el = a.el = n, t = a), a === t)
      (t = e.vnode).el = n, e = e.parent;
    else
      break;
  }
  r && r.activeBranch === t && (r.vnode.el = n);
}
const mh = {}, bh = () => Object.create(mh), wh = (t) => Object.getPrototypeOf(t) === mh;
function cm(t, e, r, n = !1) {
  const a = {}, i = bh();
  t.propsDefaults = /* @__PURE__ */ Object.create(null), xh(t, e, a, i);
  for (const s in t.propsOptions[0])
    s in a || (a[s] = void 0);
  r ? t.props = n ? a : /* @__PURE__ */ py(a) : t.type.props ? t.props = a : t.props = i, t.attrs = i;
}
function vm(t, e, r, n) {
  const {
    props: a,
    attrs: i,
    vnode: { patchFlag: s }
  } = t, o = /* @__PURE__ */ ze(a), [l] = t.propsOptions;
  let u = !1;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    (n || s > 0) && !(s & 16)
  ) {
    if (s & 8) {
      const f = t.vnode.dynamicProps;
      for (let c = 0; c < f.length; c++) {
        let v = f[c];
        if (qs(t.emitsOptions, v))
          continue;
        const d = e[v];
        if (l)
          if (Ve(i, v))
            d !== i[v] && (i[v] = d, u = !0);
          else {
            const h = Ft(v);
            a[h] = su(
              l,
              o,
              h,
              d,
              t,
              !1
            );
          }
        else
          d !== i[v] && (i[v] = d, u = !0);
      }
    }
  } else {
    xh(t, e, a, i) && (u = !0);
    let f;
    for (const c in o)
      (!e || // for camelCase
      !Ve(e, c) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((f = Nn(c)) === c || !Ve(e, f))) && (l ? r && // for camelCase
      (r[c] !== void 0 || // for kebab-case
      r[f] !== void 0) && (a[c] = su(
        l,
        o,
        c,
        void 0,
        t,
        !0
      )) : delete a[c]);
    if (i !== o)
      for (const c in i)
        (!e || !Ve(e, c)) && (delete i[c], u = !0);
  }
  u && Ir(t.attrs, "set", "");
}
function xh(t, e, r, n) {
  const [a, i] = t.propsOptions;
  let s = !1, o;
  if (e)
    for (let l in e) {
      if (Va(l))
        continue;
      const u = e[l];
      let f;
      a && Ve(a, f = Ft(l)) ? !i || !i.includes(f) ? r[f] = u : (o || (o = {}))[f] = u : qs(t.emitsOptions, l) || (!(l in n) || u !== n[l]) && (n[l] = u, s = !0);
    }
  if (i) {
    const l = /* @__PURE__ */ ze(r), u = o || Ge;
    for (let f = 0; f < i.length; f++) {
      const c = i[f];
      r[c] = su(
        a,
        l,
        c,
        u[c],
        t,
        !Ve(u, c)
      );
    }
  }
  return s;
}
function su(t, e, r, n, a, i) {
  const s = t[r];
  if (s != null) {
    const o = Ve(s, "default");
    if (o && n === void 0) {
      const l = s.default;
      if (s.type !== Function && !s.skipFactory && Re(l)) {
        const { propsDefaults: u } = a;
        if (r in u)
          n = u[r];
        else {
          const f = hi(a);
          n = u[r] = l.call(
            null,
            e
          ), f();
        }
      } else
        n = l;
      a.ce && a.ce._setProp(r, n);
    }
    s[
      0
      /* shouldCast */
    ] && (i && !o ? n = !1 : s[
      1
      /* shouldCastTrue */
    ] && (n === "" || n === Nn(r)) && (n = !0));
  }
  return n;
}
const dm = /* @__PURE__ */ new WeakMap();
function Eh(t, e, r = !1) {
  const n = r ? dm : e.propsCache, a = n.get(t);
  if (a)
    return a;
  const i = t.props, s = {}, o = [];
  let l = !1;
  if (!Re(t)) {
    const f = (c) => {
      l = !0;
      const [v, d] = Eh(c, e, !0);
      Tt(s, v), d && o.push(...d);
    };
    !r && e.mixins.length && e.mixins.forEach(f), t.extends && f(t.extends), t.mixins && t.mixins.forEach(f);
  }
  if (!i && !l)
    return Ue(t) && n.set(t, Jn), Jn;
  if (ke(i))
    for (let f = 0; f < i.length; f++) {
      const c = Ft(i[f]);
      Of(c) && (s[c] = Ge);
    }
  else if (i)
    for (const f in i) {
      const c = Ft(f);
      if (Of(c)) {
        const v = i[f], d = s[c] = ke(v) || Re(v) ? { type: v } : Tt({}, v), h = d.type;
        let y = !1, g = !0;
        if (ke(h))
          for (let p = 0; p < h.length; ++p) {
            const m = h[p], b = Re(m) && m.name;
            if (b === "Boolean") {
              y = !0;
              break;
            } else b === "String" && (g = !1);
          }
        else
          y = Re(h) && h.name === "Boolean";
        d[
          0
          /* shouldCast */
        ] = y, d[
          1
          /* shouldCastTrue */
        ] = g, (y || Ve(d, "default")) && o.push(c);
      }
    }
  const u = [s, o];
  return Ue(t) && n.set(t, u), u;
}
function Of(t) {
  return t[0] !== "$" && !Va(t);
}
const $u = (t) => t === "_" || t === "_ctx" || t === "$stable", Hu = (t) => ke(t) ? t.map(br) : [br(t)], hm = (t, e, r) => {
  if (e._n)
    return e;
  const n = ky((...a) => Hu(e(...a)), r);
  return n._c = !1, n;
}, Ch = (t, e, r) => {
  const n = t._ctx;
  for (const a in t) {
    if ($u(a)) continue;
    const i = t[a];
    if (Re(i))
      e[a] = hm(a, i, n);
    else if (i != null) {
      const s = Hu(i);
      e[a] = () => s;
    }
  }
}, Th = (t, e) => {
  const r = Hu(e);
  t.slots.default = () => r;
}, Sh = (t, e, r) => {
  for (const n in e)
    (r || !$u(n)) && (t[n] = e[n]);
}, gm = (t, e, r) => {
  const n = t.slots = bh();
  if (t.vnode.shapeFlag & 32) {
    const a = e._;
    a ? (Sh(n, e, r), r && Ld(n, "_", a, !0)) : Ch(e, n);
  } else e && Th(t, e);
}, pm = (t, e, r) => {
  const { vnode: n, slots: a } = t;
  let i = !0, s = Ge;
  if (n.shapeFlag & 32) {
    const o = e._;
    o ? r && o === 1 ? i = !1 : Sh(a, e, r) : (i = !e.$stable, Ch(e, a)), s = e;
  } else e && (Th(t, e), s = { default: 1 });
  if (i)
    for (const o in a)
      !$u(o) && s[o] == null && delete a[o];
}, Ht = xm;
function ym(t) {
  return mm(t);
}
function mm(t, e) {
  const r = _s();
  r.__VUE__ = !0;
  const {
    insert: n,
    remove: a,
    patchProp: i,
    createElement: s,
    createText: o,
    createComment: l,
    setText: u,
    setElementText: f,
    parentNode: c,
    nextSibling: v,
    setScopeId: d = Er,
    insertStaticContent: h
  } = t, y = (C, B, z, W = null, j = null, Z = null, ne = void 0, te = null, Y = !!B.dynamicChildren) => {
    if (C === B)
      return;
    C && !Da(C, B) && (W = ce(C), $(C, j, Z, !0), C = null), B.patchFlag === -2 && (Y = !1, B.dynamicChildren = null);
    const { type: K, ref: ue, shapeFlag: oe } = B;
    switch (K) {
      case $s:
        g(C, B, z, W);
        break;
      case rn:
        p(C, B, z, W);
        break;
      case bo:
        C == null && m(B, z, W, ne);
        break;
      case Wt:
        R(
          C,
          B,
          z,
          W,
          j,
          Z,
          ne,
          te,
          Y
        );
        break;
      default:
        oe & 1 ? E(
          C,
          B,
          z,
          W,
          j,
          Z,
          ne,
          te,
          Y
        ) : oe & 6 ? M(
          C,
          B,
          z,
          W,
          j,
          Z,
          ne,
          te,
          Y
        ) : (oe & 64 || oe & 128) && K.process(
          C,
          B,
          z,
          W,
          j,
          Z,
          ne,
          te,
          Y,
          U
        );
    }
    ue != null && j ? Ha(ue, C && C.ref, Z, B || C, !B) : ue == null && C && C.ref != null && Ha(C.ref, null, Z, C, !0);
  }, g = (C, B, z, W) => {
    if (C == null)
      n(
        B.el = o(B.children),
        z,
        W
      );
    else {
      const j = B.el = C.el;
      B.children !== C.children && u(j, B.children);
    }
  }, p = (C, B, z, W) => {
    C == null ? n(
      B.el = l(B.children || ""),
      z,
      W
    ) : B.el = C.el;
  }, m = (C, B, z, W) => {
    [C.el, C.anchor] = h(
      C.children,
      B,
      z,
      W,
      C.el,
      C.anchor
    );
  }, b = ({ el: C, anchor: B }, z, W) => {
    let j;
    for (; C && C !== B; )
      j = v(C), n(C, z, W), C = j;
    n(B, z, W);
  }, w = ({ el: C, anchor: B }) => {
    let z;
    for (; C && C !== B; )
      z = v(C), a(C), C = z;
    a(B);
  }, E = (C, B, z, W, j, Z, ne, te, Y) => {
    if (B.type === "svg" ? ne = "svg" : B.type === "math" && (ne = "mathml"), C == null)
      T(
        B,
        z,
        W,
        j,
        Z,
        ne,
        te,
        Y
      );
    else {
      const K = C.el && C.el._isVueCE ? C.el : null;
      try {
        K && K._beginPatch(), D(
          C,
          B,
          j,
          Z,
          ne,
          te,
          Y
        );
      } finally {
        K && K._endPatch();
      }
    }
  }, T = (C, B, z, W, j, Z, ne, te) => {
    let Y, K;
    const { props: ue, shapeFlag: oe, transition: ve, dirs: de } = C;
    if (Y = C.el = s(
      C.type,
      Z,
      ue && ue.is,
      ue
    ), oe & 8 ? f(Y, C.children) : oe & 16 && S(
      C.children,
      Y,
      null,
      W,
      j,
      mo(C, Z),
      ne,
      te
    ), de && pn(C, null, W, "created"), x(Y, C, C.scopeId, ne, W), ue) {
      for (const Te in ue)
        Te !== "value" && !Va(Te) && i(Y, Te, null, ue[Te], Z, W);
      "value" in ue && i(Y, "value", null, ue.value, Z), (K = ue.onVnodeBeforeMount) && vr(K, W, C);
    }
    de && pn(C, null, W, "beforeMount");
    const me = bm(j, ve);
    me && ve.beforeEnter(Y), n(Y, B, z), ((K = ue && ue.onVnodeMounted) || me || de) && Ht(() => {
      try {
        K && vr(K, W, C), me && ve.enter(Y), de && pn(C, null, W, "mounted");
      } finally {
      }
    }, j);
  }, x = (C, B, z, W, j) => {
    if (z && d(C, z), W)
      for (let Z = 0; Z < W.length; Z++)
        d(C, W[Z]);
    if (j) {
      let Z = j.subTree;
      if (B === Z || kh(Z.type) && (Z.ssContent === B || Z.ssFallback === B)) {
        const ne = j.vnode;
        x(
          C,
          ne,
          ne.scopeId,
          ne.slotScopeIds,
          j.parent
        );
      }
    }
  }, S = (C, B, z, W, j, Z, ne, te, Y = 0) => {
    for (let K = Y; K < C.length; K++) {
      const ue = C[K] = te ? Lr(C[K]) : br(C[K]);
      y(
        null,
        ue,
        B,
        z,
        W,
        j,
        Z,
        ne,
        te
      );
    }
  }, D = (C, B, z, W, j, Z, ne) => {
    const te = B.el = C.el;
    let { patchFlag: Y, dynamicChildren: K, dirs: ue } = B;
    Y |= C.patchFlag & 16;
    const oe = C.props || Ge, ve = B.props || Ge;
    let de;
    if (z && yn(z, !1), (de = ve.onVnodeBeforeUpdate) && vr(de, z, B, C), ue && pn(B, C, z, "beforeUpdate"), z && yn(z, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    K && (!C.dynamicChildren || C.dynamicChildren.length !== K.length) && (Y = 0, ne = !1, K = null), (oe.innerHTML && ve.innerHTML == null || oe.textContent && ve.textContent == null) && f(te, ""), K ? A(
      C.dynamicChildren,
      K,
      te,
      z,
      W,
      mo(B, j),
      Z
    ) : ne || F(
      C,
      B,
      te,
      null,
      z,
      W,
      mo(B, j),
      Z,
      !1
    ), Y > 0) {
      if (Y & 16)
        k(te, oe, ve, z, j);
      else if (Y & 2 && oe.class !== ve.class && i(te, "class", null, ve.class, j), Y & 4 && i(te, "style", oe.style, ve.style, j), Y & 8) {
        const me = B.dynamicProps;
        for (let Te = 0; Te < me.length; Te++) {
          const Ee = me[Te], Pe = oe[Ee], J = ve[Ee];
          (J !== Pe || Ee === "value") && i(te, Ee, Pe, J, j, z);
        }
      }
      Y & 1 && C.children !== B.children && f(te, B.children);
    } else !ne && K == null && k(te, oe, ve, z, j);
    ((de = ve.onVnodeUpdated) || ue) && Ht(() => {
      de && vr(de, z, B, C), ue && pn(B, C, z, "updated");
    }, W);
  }, A = (C, B, z, W, j, Z, ne) => {
    for (let te = 0; te < B.length; te++) {
      const Y = C[te], K = B[te], ue = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        Y.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (Y.type === Wt || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !Da(Y, K) || // - In the case of a component, it could contain anything.
        Y.shapeFlag & 198) ? c(Y.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          z
        )
      );
      y(
        Y,
        K,
        ue,
        null,
        W,
        j,
        Z,
        ne,
        !0
      );
    }
  }, k = (C, B, z, W, j) => {
    if (B !== z) {
      if (B !== Ge)
        for (const Z in B)
          !Va(Z) && !(Z in z) && i(
            C,
            Z,
            B[Z],
            null,
            j,
            W
          );
      for (const Z in z) {
        if (Va(Z)) continue;
        const ne = z[Z], te = B[Z];
        ne !== te && Z !== "value" && i(C, Z, te, ne, j, W);
      }
      "value" in z && i(C, "value", B.value, z.value, j);
    }
  }, R = (C, B, z, W, j, Z, ne, te, Y) => {
    const K = B.el = C ? C.el : o(""), ue = B.anchor = C ? C.anchor : o("");
    let { patchFlag: oe, dynamicChildren: ve, slotScopeIds: de } = B;
    de && (te = te ? te.concat(de) : de), C == null ? (n(K, z, W), n(ue, z, W), S(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      B.children || [],
      z,
      ue,
      j,
      Z,
      ne,
      te,
      Y
    )) : oe > 0 && oe & 64 && ve && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    C.dynamicChildren && C.dynamicChildren.length === ve.length ? (A(
      C.dynamicChildren,
      ve,
      z,
      j,
      Z,
      ne,
      te
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (B.key != null || j && B === j.subTree) && Ph(
      C,
      B,
      !0
      /* shallow */
    )) : F(
      C,
      B,
      z,
      ue,
      j,
      Z,
      ne,
      te,
      Y
    );
  }, M = (C, B, z, W, j, Z, ne, te, Y) => {
    B.slotScopeIds = te, C == null ? B.shapeFlag & 512 ? j.ctx.activate(
      B,
      z,
      W,
      ne,
      Y
    ) : I(
      B,
      z,
      W,
      j,
      Z,
      ne,
      Y
    ) : _(C, B, Y);
  }, I = (C, B, z, W, j, Z, ne) => {
    const te = C.component = Dm(
      C,
      W,
      j
    );
    if (lh(C) && (te.ctx.renderer = U), km(te, !1, ne), te.asyncDep) {
      if (j && j.registerDep(te, N, ne), !C.el) {
        const Y = te.subTree = _r(rn);
        p(null, Y, B, z), C.placeholder = Y.el;
      }
    } else
      N(
        te,
        C,
        B,
        z,
        j,
        Z,
        ne
      );
  }, _ = (C, B, z) => {
    const W = B.component = C.component;
    if (um(C, B, z))
      if (W.asyncDep && !W.asyncResolved) {
        L(W, B, z);
        return;
      } else
        W.next = B, W.update();
    else
      B.el = C.el, W.vnode = B;
  }, N = (C, B, z, W, j, Z, ne) => {
    const te = () => {
      if (C.isMounted) {
        let { next: oe, bu: ve, u: de, parent: me, vnode: Te } = C;
        {
          const q = Dh(C);
          if (q) {
            oe && (oe.el = Te.el, L(C, oe, ne)), q.asyncDep.then(() => {
              Ht(() => {
                C.isUnmounted || K();
              }, j);
            });
            return;
          }
        }
        let Ee = oe, Pe;
        yn(C, !1), oe ? (oe.el = Te.el, L(C, oe, ne)) : oe = Te, ve && Wi(ve), (Pe = oe.props && oe.props.onVnodeBeforeUpdate) && vr(Pe, me, oe, Te), yn(C, !0);
        const J = Lf(C), P = C.subTree;
        C.subTree = J, y(
          P,
          J,
          // parent may have changed if it's in a teleport
          c(P.el),
          // anchor may have changed if it's in a fragment
          ce(P),
          C,
          j,
          Z
        ), oe.el = J.el, Ee === null && fm(C, J.el), de && Ht(de, j), (Pe = oe.props && oe.props.onVnodeUpdated) && Ht(
          () => vr(Pe, me, oe, Te),
          j
        );
      } else {
        let oe;
        const { el: ve, props: de } = B, { bm: me, m: Te, parent: Ee, root: Pe, type: J } = C, P = Ua(B);
        yn(C, !1), me && Wi(me), !P && (oe = de && de.onVnodeBeforeMount) && vr(oe, Ee, B), yn(C, !0);
        {
          Pe.ce && Pe.ce._hasShadowRoot() && Pe.ce._injectChildStyle(
            J,
            C.parent ? C.parent.type : void 0
          );
          const q = C.subTree = Lf(C);
          y(
            null,
            q,
            z,
            W,
            C,
            j,
            Z
          ), B.el = q.el;
        }
        if (Te && Ht(Te, j), !P && (oe = de && de.onVnodeMounted)) {
          const q = B;
          Ht(
            () => vr(oe, Ee, q),
            j
          );
        }
        (B.shapeFlag & 256 || Ee && Ua(Ee.vnode) && Ee.vnode.shapeFlag & 256) && C.a && Ht(C.a, j), C.isMounted = !0, B = z = W = null;
      }
    };
    C.scope.on();
    const Y = C.effect = new _d(te);
    C.scope.off();
    const K = C.update = Y.run.bind(Y), ue = C.job = Y.runIfDirty.bind(Y);
    ue.i = C, ue.id = C.uid, Y.scheduler = () => Vu(ue), yn(C, !0), K();
  }, L = (C, B, z) => {
    B.component = C;
    const W = C.vnode.props;
    C.vnode = B, C.next = null, vm(C, B.props, W, z), pm(C, B.children, z), Fr(), Sf(C), zr();
  }, F = (C, B, z, W, j, Z, ne, te, Y = !1) => {
    const K = C && C.children, ue = C ? C.shapeFlag : 0, oe = B.children, { patchFlag: ve, shapeFlag: de } = B;
    if (ve > 0) {
      if (ve & 128) {
        V(
          K,
          oe,
          z,
          W,
          j,
          Z,
          ne,
          te,
          Y
        );
        return;
      } else if (ve & 256) {
        H(
          K,
          oe,
          z,
          W,
          j,
          Z,
          ne,
          te,
          Y
        );
        return;
      }
    }
    de & 8 ? (ue & 16 && le(K, j, Z), oe !== K && f(z, oe)) : ue & 16 ? de & 16 ? V(
      K,
      oe,
      z,
      W,
      j,
      Z,
      ne,
      te,
      Y
    ) : le(K, j, Z, !0) : (ue & 8 && f(z, ""), de & 16 && S(
      oe,
      z,
      W,
      j,
      Z,
      ne,
      te,
      Y
    ));
  }, H = (C, B, z, W, j, Z, ne, te, Y) => {
    C = C || Jn, B = B || Jn;
    const K = C.length, ue = B.length, oe = Math.min(K, ue);
    let ve;
    for (ve = 0; ve < oe; ve++) {
      const de = B[ve] = Y ? Lr(B[ve]) : br(B[ve]);
      y(
        C[ve],
        de,
        z,
        null,
        j,
        Z,
        ne,
        te,
        Y
      );
    }
    K > ue ? le(
      C,
      j,
      Z,
      !0,
      !1,
      oe
    ) : S(
      B,
      z,
      W,
      j,
      Z,
      ne,
      te,
      Y,
      oe
    );
  }, V = (C, B, z, W, j, Z, ne, te, Y) => {
    let K = 0;
    const ue = B.length;
    let oe = C.length - 1, ve = ue - 1;
    for (; K <= oe && K <= ve; ) {
      const de = C[K], me = B[K] = Y ? Lr(B[K]) : br(B[K]);
      if (Da(de, me))
        y(
          de,
          me,
          z,
          null,
          j,
          Z,
          ne,
          te,
          Y
        );
      else
        break;
      K++;
    }
    for (; K <= oe && K <= ve; ) {
      const de = C[oe], me = B[ve] = Y ? Lr(B[ve]) : br(B[ve]);
      if (Da(de, me))
        y(
          de,
          me,
          z,
          null,
          j,
          Z,
          ne,
          te,
          Y
        );
      else
        break;
      oe--, ve--;
    }
    if (K > oe) {
      if (K <= ve) {
        const de = ve + 1, me = de < ue ? B[de].el : W;
        for (; K <= ve; )
          y(
            null,
            B[K] = Y ? Lr(B[K]) : br(B[K]),
            z,
            me,
            j,
            Z,
            ne,
            te,
            Y
          ), K++;
      }
    } else if (K > ve)
      for (; K <= oe; )
        $(C[K], j, Z, !0), K++;
    else {
      const de = K, me = K, Te = /* @__PURE__ */ new Map();
      for (K = me; K <= ve; K++) {
        const ee = B[K] = Y ? Lr(B[K]) : br(B[K]);
        ee.key != null && Te.set(ee.key, K);
      }
      let Ee, Pe = 0;
      const J = ve - me + 1;
      let P = !1, q = 0;
      const G = new Array(J);
      for (K = 0; K < J; K++) G[K] = 0;
      for (K = de; K <= oe; K++) {
        const ee = C[K];
        if (Pe >= J) {
          $(ee, j, Z, !0);
          continue;
        }
        let ye;
        if (ee.key != null)
          ye = Te.get(ee.key);
        else
          for (Ee = me; Ee <= ve; Ee++)
            if (G[Ee - me] === 0 && Da(ee, B[Ee])) {
              ye = Ee;
              break;
            }
        ye === void 0 ? $(ee, j, Z, !0) : (G[ye - me] = K + 1, ye >= q ? q = ye : P = !0, y(
          ee,
          B[ye],
          z,
          null,
          j,
          Z,
          ne,
          te,
          Y
        ), Pe++);
      }
      const re = P ? wm(G) : Jn;
      for (Ee = re.length - 1, K = J - 1; K >= 0; K--) {
        const ee = me + K, ye = B[ee], fe = B[ee + 1], ge = ee + 1 < ue ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          fe.el || Ah(fe)
        ) : W;
        G[K] === 0 ? y(
          null,
          ye,
          z,
          ge,
          j,
          Z,
          ne,
          te,
          Y
        ) : P && (Ee < 0 || K !== re[Ee] ? O(ye, z, ge, 2) : Ee--);
      }
    }
  }, O = (C, B, z, W, j = null) => {
    const { el: Z, type: ne, transition: te, children: Y, shapeFlag: K } = C;
    if (K & 6) {
      O(C.component.subTree, B, z, W);
      return;
    }
    if (K & 128) {
      C.suspense.move(B, z, W);
      return;
    }
    if (K & 64) {
      ne.move(C, B, z, U);
      return;
    }
    if (ne === Wt) {
      n(Z, B, z);
      for (let oe = 0; oe < Y.length; oe++)
        O(Y[oe], B, z, W);
      n(C.anchor, B, z);
      return;
    }
    if (ne === bo) {
      b(C, B, z);
      return;
    }
    if (W !== 2 && K & 1 && te)
      if (W === 0)
        te.persisted && !Z[po] ? n(Z, B, z) : (te.beforeEnter(Z), n(Z, B, z), Ht(() => te.enter(Z), j));
      else {
        const { leave: oe, delayLeave: ve, afterLeave: de } = te, me = () => {
          C.ctx.isUnmounted ? a(Z) : n(Z, B, z);
        }, Te = () => {
          const Ee = Z._isLeaving || !!Z[po];
          Z._isLeaving && Z[po](
            !0
            /* cancelled */
          ), te.persisted && !Ee ? me() : oe(Z, () => {
            me(), de && de();
          });
        };
        ve ? ve(Z, me, Te) : Te();
      }
    else
      n(Z, B, z);
  }, $ = (C, B, z, W = !1, j = !1) => {
    const {
      type: Z,
      props: ne,
      ref: te,
      children: Y,
      dynamicChildren: K,
      shapeFlag: ue,
      patchFlag: oe,
      dirs: ve,
      cacheIndex: de,
      memo: me
    } = C;
    if (oe === -2 && (j = !1), te != null && (Fr(), Ha(te, null, z, C, !0), zr()), de != null && (B.renderCache[de] = void 0), ue & 256) {
      B.ctx.deactivate(C);
      return;
    }
    const Te = ue & 1 && ve, Ee = !Ua(C);
    let Pe;
    if (Ee && (Pe = ne && ne.onVnodeBeforeUnmount) && vr(Pe, B, C), ue & 6)
      ae(C.component, z, W);
    else {
      if (ue & 128) {
        C.suspense.unmount(z, W);
        return;
      }
      Te && pn(C, null, B, "beforeUnmount"), ue & 64 ? C.type.remove(
        C,
        B,
        z,
        U,
        W
      ) : K && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !K.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (Z !== Wt || oe > 0 && oe & 64) ? le(
        K,
        B,
        z,
        !1,
        !0
      ) : (Z === Wt && oe & 384 || !j && ue & 16) && le(Y, B, z), W && Q(C);
    }
    const J = me != null && de == null;
    (Ee && (Pe = ne && ne.onVnodeUnmounted) || Te || J) && Ht(() => {
      Pe && vr(Pe, B, C), Te && pn(C, null, B, "unmounted"), J && (C.el = null);
    }, z);
  }, Q = (C) => {
    const { type: B, el: z, anchor: W, transition: j } = C;
    if (B === Wt) {
      se(z, W);
      return;
    }
    if (B === bo) {
      w(C);
      return;
    }
    const Z = () => {
      a(z), j && !j.persisted && j.afterLeave && j.afterLeave();
    };
    if (C.shapeFlag & 1 && j && !j.persisted) {
      const { leave: ne, delayLeave: te } = j, Y = () => ne(z, Z);
      te ? te(C.el, Z, Y) : Y();
    } else
      Z();
  }, se = (C, B) => {
    let z;
    for (; C !== B; )
      z = v(C), a(C), C = z;
    a(B);
  }, ae = (C, B, z) => {
    const { bum: W, scope: j, job: Z, subTree: ne, um: te, m: Y, a: K } = C;
    Nf(Y), Nf(K), W && Wi(W), j.stop(), Z && (Z.flags |= 8, $(ne, C, B, z)), te && Ht(te, B), Ht(() => {
      C.isUnmounted = !0;
    }, B);
  }, le = (C, B, z, W = !1, j = !1, Z = 0) => {
    for (let ne = Z; ne < C.length; ne++)
      $(C[ne], B, z, W, j);
  }, ce = (C) => {
    if (C.shapeFlag & 6)
      return ce(C.component.subTree);
    if (C.shapeFlag & 128)
      return C.suspense.next();
    const B = v(C.anchor || C.el), z = B && B[Iy];
    return z ? v(z) : B;
  };
  let he = !1;
  const ie = (C, B, z) => {
    let W;
    C == null ? B._vnode && ($(B._vnode, null, null, !0), W = B._vnode.component) : y(
      B._vnode || null,
      C,
      B,
      null,
      null,
      null,
      z
    ), B._vnode = C, he || (he = !0, Sf(W), th(), he = !1);
  }, U = {
    p: y,
    um: $,
    m: O,
    r: Q,
    mt: I,
    mc: S,
    pc: F,
    pbc: A,
    n: ce,
    o: t
  };
  return {
    render: ie,
    hydrate: void 0,
    createApp: nm(ie)
  };
}
function mo({ type: t, props: e }, r) {
  return r === "svg" && t === "foreignObject" || r === "mathml" && t === "annotation-xml" && e && e.encoding && e.encoding.includes("html") ? void 0 : r;
}
function yn({ effect: t, job: e }, r) {
  r ? (t.flags |= 32, e.flags |= 4) : (t.flags &= -33, e.flags &= -5);
}
function bm(t, e) {
  return (!t || t && !t.pendingBranch) && e && !e.persisted;
}
function Ph(t, e, r = !1) {
  const n = t.children, a = e.children;
  if (ke(n) && ke(a))
    for (let i = 0; i < n.length; i++) {
      const s = n[i];
      let o = a[i];
      o.shapeFlag & 1 && !o.dynamicChildren && ((o.patchFlag <= 0 || o.patchFlag === 32) && (o = a[i] = Lr(a[i]), o.el = s.el), !r && o.patchFlag !== -2 && Ph(s, o)), o.type === $s && (o.patchFlag === -1 && (o = a[i] = Lr(o)), o.el = s.el), o.type === rn && !o.el && (o.el = s.el);
    }
}
function wm(t) {
  const e = t.slice(), r = [0];
  let n, a, i, s, o;
  const l = t.length;
  for (n = 0; n < l; n++) {
    const u = t[n];
    if (u !== 0) {
      if (a = r[r.length - 1], t[a] < u) {
        e[n] = a, r.push(n);
        continue;
      }
      for (i = 0, s = r.length - 1; i < s; )
        o = i + s >> 1, t[r[o]] < u ? i = o + 1 : s = o;
      u < t[r[i]] && (i > 0 && (e[n] = r[i - 1]), r[i] = n);
    }
  }
  for (i = r.length, s = r[i - 1]; i-- > 0; )
    r[i] = s, s = e[s];
  return r;
}
function Dh(t) {
  const e = t.subTree.component;
  if (e)
    return e.asyncDep && !e.asyncResolved ? e : Dh(e);
}
function Nf(t) {
  if (t)
    for (let e = 0; e < t.length; e++)
      t[e].flags |= 8;
}
function Ah(t) {
  if (t.placeholder)
    return t.placeholder;
  const e = t.component;
  return e ? Ah(e.subTree) : null;
}
const kh = (t) => t.__isSuspense;
function xm(t, e) {
  e && e.pendingBranch ? ke(t) ? e.effects.push(...t) : e.effects.push(t) : Ay(t);
}
const Wt = /* @__PURE__ */ Symbol.for("v-fgt"), $s = /* @__PURE__ */ Symbol.for("v-txt"), rn = /* @__PURE__ */ Symbol.for("v-cmt"), bo = /* @__PURE__ */ Symbol.for("v-stc"), kn = [];
let Yt = null;
function et(t = !1) {
  kn.push(Yt = t ? null : []);
}
function Bh() {
  kn.pop(), Yt = kn[kn.length - 1] || null;
}
let Ja = 1;
function _f(t, e = !1) {
  Ja += t, t < 0 && Yt && e && (Yt.hasOnce = !0);
}
function Rh(t) {
  return t.dynamicChildren = Ja > 0 ? Yt || Jn : null, Bh(), Ja > 0 && Yt && Yt.push(t), t;
}
function ot(t, e, r, n, a, i) {
  return Rh(
    Ce(
      t,
      e,
      r,
      n,
      a,
      i,
      !0
    )
  );
}
function Uu(t, e, r, n, a) {
  return Rh(
    _r(
      t,
      e,
      r,
      n,
      a,
      !0
    )
  );
}
function Mh(t) {
  return t ? t.__v_isVNode === !0 : !1;
}
function Da(t, e) {
  return t.type === e.type && t.key === e.key;
}
const Lh = ({ key: t }) => t ?? null, Zi = ({
  ref: t,
  ref_key: e,
  ref_for: r
}) => (typeof t == "number" && (t = "" + t), t != null ? at(t) || /* @__PURE__ */ Bt(t) || Re(t) ? { i: Kt, r: t, k: e, f: !!r } : t : null);
function Ce(t, e = null, r = null, n = 0, a = null, i = t === Wt ? 0 : 1, s = !1, o = !1) {
  const l = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: t,
    props: e,
    key: e && Lh(e),
    ref: e && Zi(e),
    scopeId: nh,
    slotScopeIds: null,
    children: r,
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
    dynamicProps: a,
    dynamicChildren: null,
    appContext: null,
    ctx: Kt
  };
  return o ? (ds(l, r), i & 128 && t.normalize(l)) : r && (l.shapeFlag |= at(r) ? 8 : 16), Ja > 0 && // avoid a block node from tracking itself
  !s && // has current parent block
  Yt && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (l.patchFlag > 0 || i & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  l.patchFlag !== 32 && Yt.push(l), l;
}
const _r = Em;
function Em(t, e = null, r = null, n = 0, a = null, i = !1) {
  if ((!t || t === Yy) && (t = rn), Mh(t)) {
    const o = fa(
      t,
      e,
      !0
      /* mergeRef: true */
    );
    return r && ds(o, r), Ja > 0 && !i && Yt && (o.shapeFlag & 6 ? Yt[Yt.indexOf(t)] = o : Yt.push(o)), o.patchFlag = -2, o;
  }
  if (Im(t) && (t = t.__vccOpts), e) {
    e = Cm(e);
    let { class: o, style: l } = e;
    o && !at(o) && (e.class = la(o)), Ue(l) && (/* @__PURE__ */ zu(l) && !ke(l) && (l = Tt({}, l)), e.style = Mu(l));
  }
  const s = at(t) ? 1 : kh(t) ? 128 : Oy(t) ? 64 : Ue(t) ? 4 : Re(t) ? 2 : 0;
  return Ce(
    t,
    e,
    r,
    n,
    a,
    s,
    i,
    !0
  );
}
function Cm(t) {
  return t ? /* @__PURE__ */ zu(t) || wh(t) ? Tt({}, t) : t : null;
}
function fa(t, e, r = !1, n = !1) {
  const { props: a, ref: i, patchFlag: s, children: o, transition: l } = t, u = e ? Tm(a || {}, e) : a, f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: t.type,
    props: u,
    key: u && Lh(u),
    ref: e && e.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      r && i ? ke(i) ? i.concat(Zi(e)) : [i, Zi(e)] : Zi(e)
    ) : i,
    scopeId: t.scopeId,
    slotScopeIds: t.slotScopeIds,
    children: o,
    target: t.target,
    targetStart: t.targetStart,
    targetAnchor: t.targetAnchor,
    staticCount: t.staticCount,
    shapeFlag: t.shapeFlag,
    // if the vnode is cloned with extra props, we can no longer assume its
    // existing patch flag to be reliable and need to add the FULL_PROPS flag.
    // note: preserve flag for fragments since they use the flag for children
    // fast paths only.
    patchFlag: e && t.type !== Wt ? s === -1 ? 16 : s | 16 : s,
    dynamicProps: t.dynamicProps,
    dynamicChildren: t.dynamicChildren,
    appContext: t.appContext,
    dirs: t.dirs,
    transition: l,
    // These should technically only be non-null on mounted VNodes. However,
    // they *should* be copied for kept-alive vnodes. So we just always copy
    // them since them being non-null during a mount doesn't affect the logic as
    // they will simply be overwritten.
    component: t.component,
    suspense: t.suspense,
    ssContent: t.ssContent && fa(t.ssContent),
    ssFallback: t.ssFallback && fa(t.ssFallback),
    placeholder: t.placeholder,
    el: t.el,
    anchor: t.anchor,
    ctx: t.ctx,
    ce: t.ce
  };
  return l && n && qu(
    f,
    l.clone(f)
  ), f;
}
function Br(t = " ", e = 0) {
  return _r($s, null, t, e);
}
function Rr(t = "", e = !1) {
  return e ? (et(), Uu(rn, null, t)) : _r(rn, null, t);
}
function br(t) {
  return t == null || typeof t == "boolean" ? _r(rn) : ke(t) ? _r(
    Wt,
    null,
    // #3666, avoid reference pollution when reusing vnode
    t.slice()
  ) : Mh(t) ? Lr(t) : _r($s, null, String(t));
}
function Lr(t) {
  return t.el === null && t.patchFlag !== -1 || t.memo ? t : fa(t);
}
function ds(t, e) {
  let r = 0;
  const { shapeFlag: n } = t;
  if (e == null)
    e = null;
  else if (ke(e))
    r = 16;
  else if (typeof e == "object")
    if (n & 65) {
      const a = e.default;
      a && (a._c && (a._d = !1), ds(t, a()), a._c && (a._d = !0));
      return;
    } else {
      r = 32;
      const a = e._;
      !a && !wh(e) ? e._ctx = Kt : a === 3 && Kt && (Kt.slots._ === 1 ? e._ = 1 : (e._ = 2, t.patchFlag |= 1024));
    }
  else if (Re(e)) {
    if (n & 65) {
      ds(t, { default: e });
      return;
    }
    e = { default: e, _ctx: Kt }, r = 32;
  } else
    e = String(e), n & 64 ? (r = 16, e = [Br(e)]) : r = 8;
  t.children = e, t.shapeFlag |= r;
}
function Tm(...t) {
  const e = {};
  for (let r = 0; r < t.length; r++) {
    const n = t[r];
    for (const a in n)
      if (a === "class")
        e.class !== n.class && (e.class = la([e.class, n.class]));
      else if (a === "style")
        e.style = Mu([e.style, n.style]);
      else if (Rs(a)) {
        const i = e[a], s = n[a];
        s && i !== s && !(ke(i) && i.includes(s)) ? e[a] = i ? [].concat(i, s) : s : s == null && i == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !Ms(a) && (e[a] = s);
      } else a !== "" && (e[a] = n[a]);
  }
  return e;
}
function vr(t, e, r, n = null) {
  ur(t, e, 7, [
    r,
    n
  ]);
}
const Sm = gh();
let Pm = 0;
function Dm(t, e, r) {
  const n = t.type, a = (e ? e.appContext : t.appContext) || Sm, i = {
    uid: Pm++,
    vnode: t,
    type: n,
    parent: e,
    appContext: a,
    root: null,
    // to be immediately set
    next: null,
    subTree: null,
    // will be set synchronously right after creation
    effect: null,
    update: null,
    // will be set synchronously right after creation
    job: null,
    scope: new Qp(
      !0
      /* detached */
    ),
    render: null,
    proxy: null,
    exposed: null,
    exposeProxy: null,
    withProxy: null,
    provides: e ? e.provides : Object.create(a.provides),
    ids: e ? e.ids : ["", 0, 0],
    accessCache: null,
    renderCache: [],
    // local resolved assets
    components: null,
    directives: null,
    // resolved props and emits options
    propsOptions: Eh(n, a),
    emitsOptions: ph(n, a),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: Ge,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: Ge,
    data: Ge,
    props: Ge,
    attrs: Ge,
    slots: Ge,
    refs: Ge,
    setupState: Ge,
    setupContext: null,
    // suspense related
    suspense: r,
    suspenseId: r ? r.pendingId : 0,
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
  return i.ctx = { _: i }, i.root = e ? e.root : i, i.emit = im.bind(null, i), t.ce && t.ce(i), i;
}
let kt = null;
const Am = () => kt || Kt;
let hs, ou;
{
  const t = _s(), e = (r, n) => {
    let a;
    return (a = t[r]) || (a = t[r] = []), a.push(n), (i) => {
      a.length > 1 ? a.forEach((s) => s(i)) : a[0](i);
    };
  };
  hs = e(
    "__VUE_INSTANCE_SETTERS__",
    (r) => kt = r
  ), ou = e(
    "__VUE_SSR_SETTERS__",
    (r) => ei = r
  );
}
const hi = (t) => {
  const e = kt;
  return hs(t), t.scope.on(), () => {
    t.scope.off(), hs(e);
  };
}, Ff = () => {
  kt && kt.scope.off(), hs(null);
};
function Ih(t) {
  return t.vnode.shapeFlag & 4;
}
let ei = !1;
function km(t, e = !1, r = !1) {
  e && ou(e);
  const { props: n, children: a } = t.vnode, i = Ih(t);
  cm(t, n, i, e), gm(t, a, r || e);
  const s = i ? Bm(t, e) : void 0;
  return e && ou(!1), s;
}
function Bm(t, e) {
  const r = t.type;
  t.accessCache = /* @__PURE__ */ Object.create(null), t.proxy = new Proxy(t.ctx, Zy);
  const { setup: n } = r;
  if (n) {
    Fr();
    const a = t.setupContext = n.length > 1 ? Mm(t) : null, i = hi(t), s = di(
      n,
      t,
      0,
      [
        t.props,
        a
      ]
    ), o = Bd(s);
    if (zr(), i(), (o || t.sp) && !Ua(t) && oh(t), o) {
      if (s.then(Ff, Ff), e)
        return s.then((l) => {
          zf(t, l);
        }).catch((l) => {
          zs(l, t, 0);
        });
      t.asyncDep = s;
    } else
      zf(t, s);
  } else
    Oh(t);
}
function zf(t, e, r) {
  Re(e) ? t.type.__ssrInlineRender ? t.ssrRender = e : t.render = e : Ue(e) && (t.setupState = Qd(e)), Oh(t);
}
function Oh(t, e, r) {
  const n = t.type;
  t.render || (t.render = n.render || Er);
  {
    const a = hi(t);
    Fr();
    try {
      Qy(t);
    } finally {
      zr(), a();
    }
  }
}
const Rm = {
  get(t, e) {
    return At(t, "get", ""), t[e];
  }
};
function Mm(t) {
  const e = (r) => {
    t.exposed = r || {};
  };
  return {
    attrs: new Proxy(t.attrs, Rm),
    slots: t.slots,
    emit: t.emit,
    expose: e
  };
}
function Hs(t) {
  return t.exposed ? t.exposeProxy || (t.exposeProxy = new Proxy(Qd(yy(t.exposed)), {
    get(e, r) {
      if (r in e)
        return e[r];
      if (r in Ga)
        return Ga[r](t);
    },
    has(e, r) {
      return r in e || r in Ga;
    }
  })) : t.proxy;
}
function Lm(t, e = !0) {
  return Re(t) ? t.displayName || t.name : t.name || e && t.__name;
}
function Im(t) {
  return Re(t) && "__vccOpts" in t;
}
const Om = (t, e) => /* @__PURE__ */ Cy(t, e, ei), Nm = "3.5.40";
/**
* @vue/runtime-dom v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let lu;
const Vf = typeof window < "u" && window.trustedTypes;
if (Vf)
  try {
    lu = /* @__PURE__ */ Vf.createPolicy("vue", {
      createHTML: (t) => t
    });
  } catch {
  }
const Nh = lu ? (t) => lu.createHTML(t) : (t) => t, _m = "http://www.w3.org/2000/svg", Fm = "http://www.w3.org/1998/Math/MathML", Mr = typeof document < "u" ? document : null, qf = Mr && /* @__PURE__ */ Mr.createElement("template"), zm = {
  insert: (t, e, r) => {
    e.insertBefore(t, r || null);
  },
  remove: (t) => {
    const e = t.parentNode;
    e && e.removeChild(t);
  },
  createElement: (t, e, r, n) => {
    const a = e === "svg" ? Mr.createElementNS(_m, t) : e === "mathml" ? Mr.createElementNS(Fm, t) : r ? Mr.createElement(t, { is: r }) : Mr.createElement(t);
    return t === "select" && n && n.multiple != null && a.setAttribute("multiple", n.multiple), a;
  },
  createText: (t) => Mr.createTextNode(t),
  createComment: (t) => Mr.createComment(t),
  setText: (t, e) => {
    t.nodeValue = e;
  },
  setElementText: (t, e) => {
    t.textContent = e;
  },
  parentNode: (t) => t.parentNode,
  nextSibling: (t) => t.nextSibling,
  querySelector: (t) => Mr.querySelector(t),
  setScopeId(t, e) {
    t.setAttribute(e, "");
  },
  // __UNSAFE__
  // Reason: innerHTML.
  // Static content here can only come from compiled templates.
  // As long as the user only uses trusted templates, this is safe.
  insertStaticContent(t, e, r, n, a, i) {
    const s = r ? r.previousSibling : e.lastChild;
    if (a && (a === i || a.nextSibling))
      for (; e.insertBefore(a.cloneNode(!0), r), !(a === i || !(a = a.nextSibling)); )
        ;
    else {
      qf.innerHTML = Nh(
        n === "svg" ? `<svg>${t}</svg>` : n === "mathml" ? `<math>${t}</math>` : t
      );
      const o = qf.content;
      if (n === "svg" || n === "mathml") {
        const l = o.firstChild;
        for (; l.firstChild; )
          o.appendChild(l.firstChild);
        o.removeChild(l);
      }
      e.insertBefore(o, r);
    }
    return [
      // first
      s ? s.nextSibling : e.firstChild,
      // last
      r ? r.previousSibling : e.lastChild
    ];
  }
}, Vm = /* @__PURE__ */ Symbol("_vtc");
function qm(t, e, r) {
  const n = t[Vm];
  n && (e = (e ? [e, ...n] : [...n]).join(" ")), e == null ? t.removeAttribute("class") : r ? t.setAttribute("class", e) : t.className = e;
}
const $f = /* @__PURE__ */ Symbol("_vod"), $m = /* @__PURE__ */ Symbol("_vsh"), Hm = /* @__PURE__ */ Symbol(""), Um = /(?:^|;)\s*display\s*:/;
function Gm(t, e, r) {
  const n = t.style, a = at(r);
  let i = !1;
  if (r && !a) {
    if (e)
      if (at(e))
        for (const s of e.split(";")) {
          const o = s.slice(0, s.indexOf(":")).trim();
          r[o] == null && Na(n, o, "");
        }
      else
        for (const s in e)
          r[s] == null && Na(n, s, "");
    for (const s in r) {
      s === "display" && (i = !0);
      const o = r[s];
      o != null ? Km(
        t,
        s,
        !at(e) && e ? e[s] : void 0,
        o
      ) || Na(n, s, o) : Na(n, s, "");
    }
  } else if (a) {
    if (e !== r) {
      const s = n[Hm];
      s && (r += ";" + s), n.cssText = r, i = Um.test(r);
    }
  } else e && t.removeAttribute("style");
  $f in t && (t[$f] = i ? n.display : "", t[$m] && (n.display = "none"));
}
const Hf = /\s*!important$/;
function Na(t, e, r) {
  if (ke(r))
    r.forEach((n) => Na(t, e, n));
  else if (r == null && (r = ""), e.startsWith("--"))
    t.setProperty(e, r);
  else {
    const n = Wm(t, e);
    Hf.test(r) ? t.setProperty(
      Nn(n),
      r.replace(Hf, ""),
      "important"
    ) : t[n] = r;
  }
}
const Uf = ["Webkit", "Moz", "ms"], wo = {};
function Wm(t, e) {
  const r = wo[e];
  if (r)
    return r;
  let n = Ft(e);
  if (n !== "filter" && n in t)
    return wo[e] = n;
  n = Os(n);
  for (let a = 0; a < Uf.length; a++) {
    const i = Uf[a] + n;
    if (i in t)
      return wo[e] = i;
  }
  return e;
}
function Km(t, e, r, n) {
  return t.tagName === "TEXTAREA" && (e === "width" || e === "height") && at(n) && r === n;
}
const Gf = "http://www.w3.org/1999/xlink";
function Wf(t, e, r, n, a, i = Yp(e)) {
  n && e.startsWith("xlink:") ? r == null ? t.removeAttributeNS(Gf, e.slice(6, e.length)) : t.setAttributeNS(Gf, e, r) : r == null || i && !Id(r) ? t.removeAttribute(e) : t.setAttribute(
    e,
    i ? "" : Cr(r) ? String(r) : r
  );
}
function Kf(t, e, r, n, a) {
  if (e === "innerHTML" || e === "textContent") {
    r != null && (t[e] = e === "innerHTML" ? Nh(r) : r);
    return;
  }
  const i = t.tagName;
  if (e === "value" && i !== "PROGRESS" && // custom elements may use _value internally
  !i.includes("-")) {
    const o = i === "OPTION" ? t.getAttribute("value") || "" : t.value, l = r == null ? (
      // #11647: value should be set as empty string for null and undefined,
      // but <input type="checkbox"> should be set as 'on'.
      t.type === "checkbox" ? "on" : ""
    ) : String(r);
    (o !== l || !("_value" in t)) && (t.value = l), r == null && t.removeAttribute(e), t._value = r;
    return;
  }
  let s = !1;
  if (r === "" || r == null) {
    const o = typeof t[e];
    o === "boolean" ? r = Id(r) : r == null && o === "string" ? (r = "", s = !0) : o === "number" && (r = 0, s = !0);
  }
  try {
    t[e] = r;
  } catch {
  }
  s && t.removeAttribute(a || e);
}
function Cn(t, e, r, n) {
  t.addEventListener(e, r, n);
}
function Ym(t, e, r, n) {
  t.removeEventListener(e, r, n);
}
const Yf = /* @__PURE__ */ Symbol("_vei");
function Xm(t, e, r, n, a = null) {
  const i = t[Yf] || (t[Yf] = {}), s = i[e];
  if (n && s)
    s.value = n;
  else {
    const [o, l] = jm(e);
    if (n) {
      const u = i[e] = t0(
        n,
        a
      );
      Cn(t, o, u, l);
    } else s && (Ym(t, o, s, l), i[e] = void 0);
  }
}
const Zm = /(Once|Passive|Capture)$/, Qm = /^on:?(?:Once|Passive|Capture)$/;
function jm(t) {
  let e, r;
  for (; (r = t.match(Zm)) && !Qm.test(t); )
    e || (e = {}), t = t.slice(0, t.length - r[1].length), e[r[1].toLowerCase()] = !0;
  return [t[2] === ":" ? t.slice(3) : Nn(t.slice(2)), e];
}
let xo = 0;
const Jm = /* @__PURE__ */ Promise.resolve(), e0 = () => xo || (Jm.then(() => xo = 0), xo = Date.now());
function t0(t, e) {
  const r = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= r.attached)
      return;
    const a = r.value;
    if (ke(a)) {
      const i = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        i.call(n), n._stopped = !0;
      };
      const s = a.slice(), o = [n];
      for (let l = 0; l < s.length && !n._stopped; l++) {
        const u = s[l];
        u && ur(
          u,
          e,
          5,
          o
        );
      }
    } else
      ur(
        a,
        e,
        5,
        [n]
      );
  };
  return r.value = t, r.attached = e0(), r;
}
const Xf = (t) => t.charCodeAt(0) === 111 && t.charCodeAt(1) === 110 && // lowercase letter
t.charCodeAt(2) > 96 && t.charCodeAt(2) < 123, r0 = (t, e, r, n, a, i) => {
  const s = a === "svg";
  e === "class" ? qm(t, n, s) : e === "style" ? Gm(t, r, n) : Rs(e) ? Ms(e) || Xm(t, e, r, n, i) : (e[0] === "." ? (e = e.slice(1), !0) : e[0] === "^" ? (e = e.slice(1), !1) : n0(t, e, n, s)) ? (Kf(t, e, n), !t.tagName.includes("-") && (e === "value" || e === "checked" || e === "selected") && Wf(t, e, n, s, i, e !== "value")) : /* #11081 force set props for possible async custom element */ t._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (a0(t, e) || // @ts-expect-error _def is private
  t._def.__asyncLoader && (/[A-Z]/.test(e) || !at(n))) ? Kf(t, Ft(e), n, i, e) : (e === "true-value" ? t._trueValue = n : e === "false-value" && (t._falseValue = n), Wf(t, e, n, s));
};
function n0(t, e, r, n) {
  if (n)
    return !!(e === "innerHTML" || e === "textContent" || e in t && Xf(e) && Re(r));
  if (e === "spellcheck" || e === "draggable" || e === "translate" || e === "autocorrect" || e === "sandbox" && t.tagName === "IFRAME" || e === "form" || e === "list" && t.tagName === "INPUT" || e === "type" && t.tagName === "TEXTAREA")
    return !1;
  if (e === "width" || e === "height") {
    const a = t.tagName;
    if (a === "IMG" || a === "VIDEO" || a === "CANVAS" || a === "SOURCE")
      return !1;
  }
  return Xf(e) && at(r) ? !1 : e in t;
}
function a0(t, e) {
  const r = (
    // @ts-expect-error _def is private
    t._def.props
  );
  if (!r)
    return !1;
  const n = Ft(e);
  return Array.isArray(r) ? r.some((a) => Ft(a) === n) : Object.keys(r).some((a) => Ft(a) === n);
}
const gs = (t) => {
  const e = t.props["onUpdate:modelValue"] || !1;
  return ke(e) ? (r) => Wi(e, r) : e;
};
function i0(t) {
  t.target.composing = !0;
}
function Zf(t) {
  const e = t.target;
  e.composing && (e.composing = !1, e.dispatchEvent(new Event("input")));
}
const na = /* @__PURE__ */ Symbol("_assign");
function Qf(t, e, r) {
  return e && (t = t.trim()), r && (t = Ns(t)), t;
}
const Mi = {
  created(t, { modifiers: { lazy: e, trim: r, number: n } }, a) {
    t[na] = gs(a);
    const i = n || a.props && a.props.type === "number";
    Cn(t, e ? "change" : "input", (s) => {
      s.target.composing || t[na](Qf(t.value, r, i));
    }), (r || i) && Cn(t, "change", () => {
      t.value = Qf(t.value, r, i);
    }), e || (Cn(t, "compositionstart", i0), Cn(t, "compositionend", Zf), Cn(t, "change", Zf));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(t, { value: e }) {
    t.value = e ?? "";
  },
  beforeUpdate(t, { value: e, oldValue: r, modifiers: { lazy: n, trim: a, number: i } }, s) {
    if (t[na] = gs(s), t.composing) return;
    const o = (i || t.type === "number") && !/^0\d/.test(t.value) ? Ns(t.value) : t.value, l = e ?? "";
    if (o === l)
      return;
    const u = t.getRootNode();
    (u instanceof Document || u instanceof ShadowRoot) && u.activeElement === t && t.type !== "range" && (n && e === r || a && t.value.trim() === l) || (t.value = l);
  }
}, s0 = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(t, { value: e, modifiers: { number: r } }, n) {
    t._modelValue = e, Cn(t, "change", () => {
      const a = Array.prototype.filter.call(t.options, (i) => i.selected).map(
        (i) => r ? Ns(ps(i)) : ps(i)
      );
      t[na](
        t.multiple ? Ls(t._modelValue) ? new Set(a) : a : a[0]
      ), t._assigning = !0, Jd(() => {
        t._assigning = !1;
      });
    }), t[na] = gs(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(t, { value: e }) {
    jf(t, e);
  },
  beforeUpdate(t, { value: e }, r) {
    t._modelValue = e, t[na] = gs(r);
  },
  updated(t, { value: e }) {
    t._assigning || jf(t, e);
  }
};
function jf(t, e) {
  const r = t.multiple, n = ke(e);
  if (!(r && !n && !Ls(e))) {
    for (let a = 0, i = t.options.length; a < i; a++) {
      const s = t.options[a], o = ps(s);
      if (r)
        if (n) {
          const l = typeof o;
          l === "string" || l === "number" ? s.selected = e.some((u) => String(u) === String(o)) : s.selected = Zp(e, o) > -1;
        } else
          s.selected = e.has(o);
      else if (vi(ps(s), e)) {
        t.selectedIndex !== a && (t.selectedIndex = a);
        return;
      }
    }
    !r && t.selectedIndex !== -1 && (t.selectedIndex = -1);
  }
}
function ps(t) {
  return "_value" in t ? t._value : t.value;
}
const o0 = ["ctrl", "shift", "alt", "meta"], l0 = {
  stop: (t) => t.stopPropagation(),
  prevent: (t) => t.preventDefault(),
  self: (t) => t.target !== t.currentTarget,
  ctrl: (t) => !t.ctrlKey,
  shift: (t) => !t.shiftKey,
  alt: (t) => !t.altKey,
  meta: (t) => !t.metaKey,
  left: (t) => "button" in t && t.button !== 0,
  middle: (t) => "button" in t && t.button !== 1,
  right: (t) => "button" in t && t.button !== 2,
  exact: (t, e) => o0.some((r) => t[`${r}Key`] && !e.includes(r))
}, u0 = (t, e) => {
  if (!t) return t;
  const r = t._withMods || (t._withMods = {}), n = e.join(".");
  return r[n] || (r[n] = ((a, ...i) => {
    for (let s = 0; s < e.length; s++) {
      const o = l0[e[s]];
      if (o && o(a, e)) return;
    }
    return t(a, ...i);
  }));
}, f0 = /* @__PURE__ */ Tt({ patchProp: r0 }, zm);
let Jf;
function c0() {
  return Jf || (Jf = ym(f0));
}
const v0 = ((...t) => {
  const e = c0().createApp(...t), { mount: r } = e;
  return e.mount = (n) => {
    const a = h0(n);
    if (!a) return;
    const i = e._component;
    !Re(i) && !i.render && !i.template && (i.template = a.innerHTML), a.nodeType === 1 && (a.textContent = "");
    const s = r(a, !1, d0(a));
    return a instanceof Element && (a.removeAttribute("v-cloak"), a.setAttribute("data-v-app", "")), s;
  }, e;
});
function d0(t) {
  if (t instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && t instanceof MathMLElement)
    return "mathml";
}
function h0(t) {
  return at(t) ? document.querySelector(t) : t;
}
function uu(t, e) {
  (e == null || e > t.length) && (e = t.length);
  for (var r = 0, n = Array(e); r < e; r++) n[r] = t[r];
  return n;
}
function g0(t) {
  if (Array.isArray(t)) return t;
}
function p0(t) {
  if (Array.isArray(t)) return uu(t);
}
function fn(t, e) {
  if (!(t instanceof e)) throw new TypeError("Cannot call a class as a function");
}
function y0(t, e) {
  for (var r = 0; r < e.length; r++) {
    var n = e[r];
    n.enumerable = n.enumerable || !1, n.configurable = !0, "value" in n && (n.writable = !0), Object.defineProperty(t, Fh(n.key), n);
  }
}
function cn(t, e, r) {
  return e && y0(t.prototype, e), Object.defineProperty(t, "prototype", {
    writable: !1
  }), t;
}
function Gt(t, e) {
  var r = typeof Symbol < "u" && t[Symbol.iterator] || t["@@iterator"];
  if (!r) {
    if (Array.isArray(t) || (r = Gu(t)) || e) {
      r && (t = r);
      var n = 0, a = function() {
      };
      return {
        s: a,
        n: function() {
          return n >= t.length ? {
            done: !0
          } : {
            done: !1,
            value: t[n++]
          };
        },
        e: function(l) {
          throw l;
        },
        f: a
      };
    }
    throw new TypeError(`Invalid attempt to iterate non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`);
  }
  var i, s = !0, o = !1;
  return {
    s: function() {
      r = r.call(t);
    },
    n: function() {
      var l = r.next();
      return s = l.done, l;
    },
    e: function(l) {
      o = !0, i = l;
    },
    f: function() {
      try {
        s || r.return == null || r.return();
      } finally {
        if (o) throw i;
      }
    }
  };
}
function _h(t, e, r) {
  return (e = Fh(e)) in t ? Object.defineProperty(t, e, {
    value: r,
    enumerable: !0,
    configurable: !0,
    writable: !0
  }) : t[e] = r, t;
}
function m0(t) {
  if (typeof Symbol < "u" && t[Symbol.iterator] != null || t["@@iterator"] != null) return Array.from(t);
}
function b0(t, e) {
  var r = t == null ? null : typeof Symbol < "u" && t[Symbol.iterator] || t["@@iterator"];
  if (r != null) {
    var n, a, i, s, o = [], l = !0, u = !1;
    try {
      if (i = (r = r.call(t)).next, e === 0) {
        if (Object(r) !== r) return;
        l = !1;
      } else for (; !(l = (n = i.call(r)).done) && (o.push(n.value), o.length !== e); l = !0) ;
    } catch (f) {
      u = !0, a = f;
    } finally {
      try {
        if (!l && r.return != null && (s = r.return(), Object(s) !== s)) return;
      } finally {
        if (u) throw a;
      }
    }
    return o;
  }
}
function w0() {
  throw new TypeError(`Invalid attempt to destructure non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`);
}
function x0() {
  throw new TypeError(`Invalid attempt to spread non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`);
}
function ut(t, e) {
  return g0(t) || b0(t, e) || Gu(t, e) || w0();
}
function ys(t) {
  return p0(t) || m0(t) || Gu(t) || x0();
}
function E0(t, e) {
  if (typeof t != "object" || !t) return t;
  var r = t[Symbol.toPrimitive];
  if (r !== void 0) {
    var n = r.call(t, e);
    if (typeof n != "object") return n;
    throw new TypeError("@@toPrimitive must return a primitive value.");
  }
  return String(t);
}
function Fh(t) {
  var e = E0(t, "string");
  return typeof e == "symbol" ? e : e + "";
}
function dt(t) {
  "@babel/helpers - typeof";
  return dt = typeof Symbol == "function" && typeof Symbol.iterator == "symbol" ? function(e) {
    return typeof e;
  } : function(e) {
    return e && typeof Symbol == "function" && e.constructor === Symbol && e !== Symbol.prototype ? "symbol" : typeof e;
  }, dt(t);
}
function Gu(t, e) {
  if (t) {
    if (typeof t == "string") return uu(t, e);
    var r = {}.toString.call(t).slice(8, -1);
    return r === "Object" && t.constructor && (r = t.constructor.name), r === "Map" || r === "Set" ? Array.from(t) : r === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(r) ? uu(t, e) : void 0;
  }
}
var ct = typeof window > "u" ? null : window, ec = ct ? ct.navigator : null;
ct && ct.document;
var C0 = dt(""), zh = dt({}), T0 = dt(function() {
}), S0 = typeof HTMLElement > "u" ? "undefined" : dt(HTMLElement), gi = function(e) {
  return e && e.instanceString && tt(e.instanceString) ? e.instanceString() : null;
}, Se = function(e) {
  return e != null && dt(e) == C0;
}, tt = function(e) {
  return e != null && dt(e) === T0;
}, We = function(e) {
  return !Xt(e) && (Array.isArray ? Array.isArray(e) : e != null && e instanceof Array);
}, _e = function(e) {
  return e != null && dt(e) === zh && !We(e) && e.constructor === Object;
}, P0 = function(e) {
  return e != null && dt(e) === zh;
}, pe = function(e) {
  return e != null && dt(e) === dt(1) && !isNaN(e);
}, D0 = function(e) {
  return pe(e) && Math.floor(e) === e;
}, ms = function(e) {
  if (S0 !== "undefined")
    return e != null && e instanceof HTMLElement;
}, Xt = function(e) {
  return pi(e) || Vh(e);
}, pi = function(e) {
  return gi(e) === "collection" && e._private.single;
}, Vh = function(e) {
  return gi(e) === "collection" && !e._private.single;
}, Wu = function(e) {
  return gi(e) === "core";
}, qh = function(e) {
  return gi(e) === "stylesheet";
}, A0 = function(e) {
  return gi(e) === "event";
}, nn = function(e) {
  return e == null ? !0 : !!(e === "" || e.match(/^\s+$/));
}, k0 = function(e) {
  return typeof HTMLElement > "u" ? !1 : e instanceof HTMLElement;
}, B0 = function(e) {
  return _e(e) && pe(e.x1) && pe(e.x2) && pe(e.y1) && pe(e.y2);
}, R0 = function(e) {
  return P0(e) && tt(e.then);
}, M0 = function() {
  return ec && ec.userAgent.match(/msie|trident|edge/i);
}, ca = function(e, r) {
  r || (r = function() {
    if (arguments.length === 1)
      return arguments[0];
    if (arguments.length === 0)
      return "undefined";
    for (var i = [], s = 0; s < arguments.length; s++)
      i.push(arguments[s]);
    return i.join("$");
  });
  var n = function() {
    var i = this, s = arguments, o, l = r.apply(i, s), u = n.cache;
    return (o = u[l]) || (o = u[l] = e.apply(i, s)), o;
  };
  return n.cache = {}, n;
}, Ku = ca(function(t) {
  return t.replace(/([A-Z])/g, function(e) {
    return "-" + e.toLowerCase();
  });
}), Us = ca(function(t) {
  return t.replace(/(-\w)/g, function(e) {
    return e[1].toUpperCase();
  });
}), $h = ca(function(t, e) {
  return t + e[0].toUpperCase() + e.substring(1);
}, function(t, e) {
  return t + "$" + e;
}), tc = function(e) {
  return nn(e) ? e : e.charAt(0).toUpperCase() + e.substring(1);
}, Qr = function(e, r) {
  return e.slice(-1 * r.length) === r;
}, vt = "(?:[-+]?(?:(?:\\d+|\\d*\\.\\d+)(?:[Ee][+-]?\\d+)?))", L0 = "rgb[a]?\\((" + vt + "[%]?)\\s*,\\s*(" + vt + "[%]?)\\s*,\\s*(" + vt + "[%]?)(?:\\s*,\\s*(" + vt + "))?\\)", I0 = "rgb[a]?\\((?:" + vt + "[%]?)\\s*,\\s*(?:" + vt + "[%]?)\\s*,\\s*(?:" + vt + "[%]?)(?:\\s*,\\s*(?:" + vt + "))?\\)", O0 = "hsl[a]?\\((" + vt + ")\\s*,\\s*(" + vt + "[%])\\s*,\\s*(" + vt + "[%])(?:\\s*,\\s*(" + vt + "))?\\)", N0 = "hsl[a]?\\((?:" + vt + ")\\s*,\\s*(?:" + vt + "[%])\\s*,\\s*(?:" + vt + "[%])(?:\\s*,\\s*(?:" + vt + "))?\\)", _0 = "\\#[0-9a-fA-F]{3}", F0 = "\\#[0-9a-fA-F]{6}", Hh = function(e, r) {
  return e < r ? -1 : e > r ? 1 : 0;
}, z0 = function(e, r) {
  return -1 * Hh(e, r);
}, Ae = Object.assign != null ? Object.assign.bind(Object) : function(t) {
  for (var e = arguments, r = 1; r < e.length; r++) {
    var n = e[r];
    if (n != null)
      for (var a = Object.keys(n), i = 0; i < a.length; i++) {
        var s = a[i];
        t[s] = n[s];
      }
  }
  return t;
}, V0 = function(e) {
  if (!(!(e.length === 4 || e.length === 7) || e[0] !== "#")) {
    var r = e.length === 4, n, a, i, s = 16;
    return r ? (n = parseInt(e[1] + e[1], s), a = parseInt(e[2] + e[2], s), i = parseInt(e[3] + e[3], s)) : (n = parseInt(e[1] + e[2], s), a = parseInt(e[3] + e[4], s), i = parseInt(e[5] + e[6], s)), [n, a, i];
  }
}, q0 = function(e) {
  var r, n, a, i, s, o, l, u;
  function f(h, y, g) {
    return g < 0 && (g += 1), g > 1 && (g -= 1), g < 1 / 6 ? h + (y - h) * 6 * g : g < 1 / 2 ? y : g < 2 / 3 ? h + (y - h) * (2 / 3 - g) * 6 : h;
  }
  var c = new RegExp("^" + O0 + "$").exec(e);
  if (c) {
    if (n = parseInt(c[1]), n < 0 ? n = (360 - -1 * n % 360) % 360 : n > 360 && (n = n % 360), n /= 360, a = parseFloat(c[2]), a < 0 || a > 100 || (a = a / 100, i = parseFloat(c[3]), i < 0 || i > 100) || (i = i / 100, s = c[4], s !== void 0 && (s = parseFloat(s), s < 0 || s > 1)))
      return;
    if (a === 0)
      o = l = u = Math.round(i * 255);
    else {
      var v = i < 0.5 ? i * (1 + a) : i + a - i * a, d = 2 * i - v;
      o = Math.round(255 * f(d, v, n + 1 / 3)), l = Math.round(255 * f(d, v, n)), u = Math.round(255 * f(d, v, n - 1 / 3));
    }
    r = [o, l, u, s];
  }
  return r;
}, $0 = function(e) {
  var r, n = new RegExp("^" + L0 + "$").exec(e);
  if (n) {
    r = [];
    for (var a = [], i = 1; i <= 3; i++) {
      var s = n[i];
      if (s[s.length - 1] === "%" && (a[i] = !0), s = parseFloat(s), a[i] && (s = s / 100 * 255), s < 0 || s > 255)
        return;
      r.push(Math.floor(s));
    }
    var o = a[1] || a[2] || a[3], l = a[1] && a[2] && a[3];
    if (o && !l)
      return;
    var u = n[4];
    if (u !== void 0) {
      if (u = parseFloat(u), u < 0 || u > 1)
        return;
      r.push(u);
    }
  }
  return r;
}, H0 = function(e) {
  return U0[e.toLowerCase()];
}, Uh = function(e) {
  return (We(e) ? e : null) || H0(e) || V0(e) || $0(e) || q0(e);
}, U0 = {
  // special colour names
  transparent: [0, 0, 0, 0],
  // NB alpha === 0
  // regular colours
  aliceblue: [240, 248, 255],
  antiquewhite: [250, 235, 215],
  aqua: [0, 255, 255],
  aquamarine: [127, 255, 212],
  azure: [240, 255, 255],
  beige: [245, 245, 220],
  bisque: [255, 228, 196],
  black: [0, 0, 0],
  blanchedalmond: [255, 235, 205],
  blue: [0, 0, 255],
  blueviolet: [138, 43, 226],
  brown: [165, 42, 42],
  burlywood: [222, 184, 135],
  cadetblue: [95, 158, 160],
  chartreuse: [127, 255, 0],
  chocolate: [210, 105, 30],
  coral: [255, 127, 80],
  cornflowerblue: [100, 149, 237],
  cornsilk: [255, 248, 220],
  crimson: [220, 20, 60],
  cyan: [0, 255, 255],
  darkblue: [0, 0, 139],
  darkcyan: [0, 139, 139],
  darkgoldenrod: [184, 134, 11],
  darkgray: [169, 169, 169],
  darkgreen: [0, 100, 0],
  darkgrey: [169, 169, 169],
  darkkhaki: [189, 183, 107],
  darkmagenta: [139, 0, 139],
  darkolivegreen: [85, 107, 47],
  darkorange: [255, 140, 0],
  darkorchid: [153, 50, 204],
  darkred: [139, 0, 0],
  darksalmon: [233, 150, 122],
  darkseagreen: [143, 188, 143],
  darkslateblue: [72, 61, 139],
  darkslategray: [47, 79, 79],
  darkslategrey: [47, 79, 79],
  darkturquoise: [0, 206, 209],
  darkviolet: [148, 0, 211],
  deeppink: [255, 20, 147],
  deepskyblue: [0, 191, 255],
  dimgray: [105, 105, 105],
  dimgrey: [105, 105, 105],
  dodgerblue: [30, 144, 255],
  firebrick: [178, 34, 34],
  floralwhite: [255, 250, 240],
  forestgreen: [34, 139, 34],
  fuchsia: [255, 0, 255],
  gainsboro: [220, 220, 220],
  ghostwhite: [248, 248, 255],
  gold: [255, 215, 0],
  goldenrod: [218, 165, 32],
  gray: [128, 128, 128],
  grey: [128, 128, 128],
  green: [0, 128, 0],
  greenyellow: [173, 255, 47],
  honeydew: [240, 255, 240],
  hotpink: [255, 105, 180],
  indianred: [205, 92, 92],
  indigo: [75, 0, 130],
  ivory: [255, 255, 240],
  khaki: [240, 230, 140],
  lavender: [230, 230, 250],
  lavenderblush: [255, 240, 245],
  lawngreen: [124, 252, 0],
  lemonchiffon: [255, 250, 205],
  lightblue: [173, 216, 230],
  lightcoral: [240, 128, 128],
  lightcyan: [224, 255, 255],
  lightgoldenrodyellow: [250, 250, 210],
  lightgray: [211, 211, 211],
  lightgreen: [144, 238, 144],
  lightgrey: [211, 211, 211],
  lightpink: [255, 182, 193],
  lightsalmon: [255, 160, 122],
  lightseagreen: [32, 178, 170],
  lightskyblue: [135, 206, 250],
  lightslategray: [119, 136, 153],
  lightslategrey: [119, 136, 153],
  lightsteelblue: [176, 196, 222],
  lightyellow: [255, 255, 224],
  lime: [0, 255, 0],
  limegreen: [50, 205, 50],
  linen: [250, 240, 230],
  magenta: [255, 0, 255],
  maroon: [128, 0, 0],
  mediumaquamarine: [102, 205, 170],
  mediumblue: [0, 0, 205],
  mediumorchid: [186, 85, 211],
  mediumpurple: [147, 112, 219],
  mediumseagreen: [60, 179, 113],
  mediumslateblue: [123, 104, 238],
  mediumspringgreen: [0, 250, 154],
  mediumturquoise: [72, 209, 204],
  mediumvioletred: [199, 21, 133],
  midnightblue: [25, 25, 112],
  mintcream: [245, 255, 250],
  mistyrose: [255, 228, 225],
  moccasin: [255, 228, 181],
  navajowhite: [255, 222, 173],
  navy: [0, 0, 128],
  oldlace: [253, 245, 230],
  olive: [128, 128, 0],
  olivedrab: [107, 142, 35],
  orange: [255, 165, 0],
  orangered: [255, 69, 0],
  orchid: [218, 112, 214],
  palegoldenrod: [238, 232, 170],
  palegreen: [152, 251, 152],
  paleturquoise: [175, 238, 238],
  palevioletred: [219, 112, 147],
  papayawhip: [255, 239, 213],
  peachpuff: [255, 218, 185],
  peru: [205, 133, 63],
  pink: [255, 192, 203],
  plum: [221, 160, 221],
  powderblue: [176, 224, 230],
  purple: [128, 0, 128],
  red: [255, 0, 0],
  rosybrown: [188, 143, 143],
  royalblue: [65, 105, 225],
  saddlebrown: [139, 69, 19],
  salmon: [250, 128, 114],
  sandybrown: [244, 164, 96],
  seagreen: [46, 139, 87],
  seashell: [255, 245, 238],
  sienna: [160, 82, 45],
  silver: [192, 192, 192],
  skyblue: [135, 206, 235],
  slateblue: [106, 90, 205],
  slategray: [112, 128, 144],
  slategrey: [112, 128, 144],
  snow: [255, 250, 250],
  springgreen: [0, 255, 127],
  steelblue: [70, 130, 180],
  tan: [210, 180, 140],
  teal: [0, 128, 128],
  thistle: [216, 191, 216],
  tomato: [255, 99, 71],
  turquoise: [64, 224, 208],
  violet: [238, 130, 238],
  wheat: [245, 222, 179],
  white: [255, 255, 255],
  whitesmoke: [245, 245, 245],
  yellow: [255, 255, 0],
  yellowgreen: [154, 205, 50]
}, Gh = function(e) {
  for (var r = e.map, n = e.keys, a = n.length, i = 0; i < a; i++) {
    var s = n[i];
    if (_e(s))
      throw Error("Tried to set map with object key");
    i < n.length - 1 ? (r[s] == null && (r[s] = {}), r = r[s]) : r[s] = e.value;
  }
}, Wh = function(e) {
  for (var r = e.map, n = e.keys, a = n.length, i = 0; i < a; i++) {
    var s = n[i];
    if (_e(s))
      throw Error("Tried to get map with object key");
    if (r = r[s], r == null)
      return r;
  }
  return r;
}, Li = typeof globalThis < "u" ? globalThis : typeof window < "u" ? window : typeof global < "u" ? global : typeof self < "u" ? self : {};
function yi(t) {
  return t && t.__esModule && Object.prototype.hasOwnProperty.call(t, "default") ? t.default : t;
}
var Eo, rc;
function mi() {
  if (rc) return Eo;
  rc = 1;
  function t(e) {
    var r = typeof e;
    return e != null && (r == "object" || r == "function");
  }
  return Eo = t, Eo;
}
var Co, nc;
function G0() {
  if (nc) return Co;
  nc = 1;
  var t = typeof Li == "object" && Li && Li.Object === Object && Li;
  return Co = t, Co;
}
var To, ac;
function Gs() {
  if (ac) return To;
  ac = 1;
  var t = G0(), e = typeof self == "object" && self && self.Object === Object && self, r = t || e || Function("return this")();
  return To = r, To;
}
var So, ic;
function W0() {
  if (ic) return So;
  ic = 1;
  var t = Gs(), e = function() {
    return t.Date.now();
  };
  return So = e, So;
}
var Po, sc;
function K0() {
  if (sc) return Po;
  sc = 1;
  var t = /\s/;
  function e(r) {
    for (var n = r.length; n-- && t.test(r.charAt(n)); )
      ;
    return n;
  }
  return Po = e, Po;
}
var Do, oc;
function Y0() {
  if (oc) return Do;
  oc = 1;
  var t = K0(), e = /^\s+/;
  function r(n) {
    return n && n.slice(0, t(n) + 1).replace(e, "");
  }
  return Do = r, Do;
}
var Ao, lc;
function Yu() {
  if (lc) return Ao;
  lc = 1;
  var t = Gs(), e = t.Symbol;
  return Ao = e, Ao;
}
var ko, uc;
function X0() {
  if (uc) return ko;
  uc = 1;
  var t = Yu(), e = Object.prototype, r = e.hasOwnProperty, n = e.toString, a = t ? t.toStringTag : void 0;
  function i(s) {
    var o = r.call(s, a), l = s[a];
    try {
      s[a] = void 0;
      var u = !0;
    } catch {
    }
    var f = n.call(s);
    return u && (o ? s[a] = l : delete s[a]), f;
  }
  return ko = i, ko;
}
var Bo, fc;
function Z0() {
  if (fc) return Bo;
  fc = 1;
  var t = Object.prototype, e = t.toString;
  function r(n) {
    return e.call(n);
  }
  return Bo = r, Bo;
}
var Ro, cc;
function Kh() {
  if (cc) return Ro;
  cc = 1;
  var t = Yu(), e = X0(), r = Z0(), n = "[object Null]", a = "[object Undefined]", i = t ? t.toStringTag : void 0;
  function s(o) {
    return o == null ? o === void 0 ? a : n : i && i in Object(o) ? e(o) : r(o);
  }
  return Ro = s, Ro;
}
var Mo, vc;
function Q0() {
  if (vc) return Mo;
  vc = 1;
  function t(e) {
    return e != null && typeof e == "object";
  }
  return Mo = t, Mo;
}
var Lo, dc;
function bi() {
  if (dc) return Lo;
  dc = 1;
  var t = Kh(), e = Q0(), r = "[object Symbol]";
  function n(a) {
    return typeof a == "symbol" || e(a) && t(a) == r;
  }
  return Lo = n, Lo;
}
var Io, hc;
function j0() {
  if (hc) return Io;
  hc = 1;
  var t = Y0(), e = mi(), r = bi(), n = NaN, a = /^[-+]0x[0-9a-f]+$/i, i = /^0b[01]+$/i, s = /^0o[0-7]+$/i, o = parseInt;
  function l(u) {
    if (typeof u == "number")
      return u;
    if (r(u))
      return n;
    if (e(u)) {
      var f = typeof u.valueOf == "function" ? u.valueOf() : u;
      u = e(f) ? f + "" : f;
    }
    if (typeof u != "string")
      return u === 0 ? u : +u;
    u = t(u);
    var c = i.test(u);
    return c || s.test(u) ? o(u.slice(2), c ? 2 : 8) : a.test(u) ? n : +u;
  }
  return Io = l, Io;
}
var Oo, gc;
function J0() {
  if (gc) return Oo;
  gc = 1;
  var t = mi(), e = W0(), r = j0(), n = "Expected a function", a = Math.max, i = Math.min;
  function s(o, l, u) {
    var f, c, v, d, h, y, g = 0, p = !1, m = !1, b = !0;
    if (typeof o != "function")
      throw new TypeError(n);
    l = r(l) || 0, t(u) && (p = !!u.leading, m = "maxWait" in u, v = m ? a(r(u.maxWait) || 0, l) : v, b = "trailing" in u ? !!u.trailing : b);
    function w(M) {
      var I = f, _ = c;
      return f = c = void 0, g = M, d = o.apply(_, I), d;
    }
    function E(M) {
      return g = M, h = setTimeout(S, l), p ? w(M) : d;
    }
    function T(M) {
      var I = M - y, _ = M - g, N = l - I;
      return m ? i(N, v - _) : N;
    }
    function x(M) {
      var I = M - y, _ = M - g;
      return y === void 0 || I >= l || I < 0 || m && _ >= v;
    }
    function S() {
      var M = e();
      if (x(M))
        return D(M);
      h = setTimeout(S, T(M));
    }
    function D(M) {
      return h = void 0, b && f ? w(M) : (f = c = void 0, d);
    }
    function A() {
      h !== void 0 && clearTimeout(h), g = 0, f = y = c = h = void 0;
    }
    function k() {
      return h === void 0 ? d : D(e());
    }
    function R() {
      var M = e(), I = x(M);
      if (f = arguments, c = this, y = M, I) {
        if (h === void 0)
          return E(y);
        if (m)
          return clearTimeout(h), h = setTimeout(S, l), w(y);
      }
      return h === void 0 && (h = setTimeout(S, l)), d;
    }
    return R.cancel = A, R.flush = k, R;
  }
  return Oo = s, Oo;
}
var eb = J0(), wi = /* @__PURE__ */ yi(eb), No = ct ? ct.performance : null, Yh = No && No.now ? function() {
  return No.now();
} : function() {
  return Date.now();
}, tb = (function() {
  if (ct) {
    if (ct.requestAnimationFrame)
      return function(t) {
        ct.requestAnimationFrame(t);
      };
    if (ct.mozRequestAnimationFrame)
      return function(t) {
        ct.mozRequestAnimationFrame(t);
      };
    if (ct.webkitRequestAnimationFrame)
      return function(t) {
        ct.webkitRequestAnimationFrame(t);
      };
    if (ct.msRequestAnimationFrame)
      return function(t) {
        ct.msRequestAnimationFrame(t);
      };
  }
  return function(t) {
    t && setTimeout(function() {
      t(Yh());
    }, 1e3 / 60);
  };
})(), bs = function(e) {
  return tb(e);
}, qr = Yh, Tn = 9261, Xh = 65599, Xn = 5381, Zh = function(e) {
  for (var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Tn, n = r, a; a = e.next(), !a.done; )
    n = n * Xh + a.value | 0;
  return n;
}, ti = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Tn;
  return r * Xh + e | 0;
}, ri = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Xn;
  return (r << 5) + r + e | 0;
}, rb = function(e, r) {
  return e * 2097152 + r;
}, Kr = function(e) {
  return e[0] * 2097152 + e[1];
}, Ii = function(e, r) {
  return [ti(e[0], r[0]), ri(e[1], r[1])];
}, pc = function(e, r) {
  var n = {
    value: 0,
    done: !1
  }, a = 0, i = e.length, s = {
    next: function() {
      return a < i ? n.value = e[a++] : n.done = !0, n;
    }
  };
  return Zh(s, r);
}, Bn = function(e, r) {
  var n = {
    value: 0,
    done: !1
  }, a = 0, i = e.length, s = {
    next: function() {
      return a < i ? n.value = e.charCodeAt(a++) : n.done = !0, n;
    }
  };
  return Zh(s, r);
}, Qh = function() {
  return nb(arguments);
}, nb = function(e) {
  for (var r, n = 0; n < e.length; n++) {
    var a = e[n];
    n === 0 ? r = Bn(a) : r = Bn(a, r);
  }
  return r;
};
function ab(t, e, r, n, a) {
  var i = a * Math.PI / 180, s = Math.cos(i) * (t - r) - Math.sin(i) * (e - n) + r, o = Math.sin(i) * (t - r) + Math.cos(i) * (e - n) + n;
  return {
    x: s,
    y: o
  };
}
var ib = function(e, r, n, a, i, s) {
  return {
    x: (e - n) * i + n,
    y: (r - a) * s + a
  };
};
function sb(t, e, r) {
  if (r === 0) return t;
  var n = (e.x1 + e.x2) / 2, a = (e.y1 + e.y2) / 2, i = e.w / e.h, s = 1 / i, o = ab(t.x, t.y, n, a, r), l = ib(o.x, o.y, n, a, i, s);
  return {
    x: l.x,
    y: l.y
  };
}
var yc = !0, ob = console.warn != null, lb = console.trace != null, Xu = Number.MAX_SAFE_INTEGER || 9007199254740991, jh = function() {
  return !0;
}, ws = function() {
  return !1;
}, mc = function() {
  return 0;
}, Zu = function() {
}, je = function(e) {
  throw new Error(e);
}, Jh = function(e) {
  if (e !== void 0)
    yc = !!e;
  else
    return yc;
}, $e = function(e) {
  Jh() && (ob ? console.warn(e) : (console.log(e), lb && console.trace()));
}, ub = function(e) {
  return Ae({}, e);
}, xr = function(e) {
  return e == null ? e : We(e) ? e.slice() : _e(e) ? ub(e) : e;
}, fb = function(e) {
  return e.slice();
}, eg = function(e, r) {
  for (
    // loop :)
    r = e = "";
    // b - result , a - numeric letiable
    e++ < 36;
    //
    r += e * 51 & 52 ? (
      //  return a random number or 4
      (e ^ 15 ? (
        // generate a random number from 0 to 15
        8 ^ Math.random() * (e ^ 20 ? 16 : 4)
      ) : 4).toString(16)
    ) : "-"
  ) ;
  return r;
}, cb = {}, tg = function() {
  return cb;
}, St = function(e) {
  var r = Object.keys(e);
  return function(n) {
    for (var a = {}, i = 0; i < r.length; i++) {
      var s = r[i], o = n == null ? void 0 : n[s];
      a[s] = o === void 0 ? e[s] : o;
    }
    return a;
  };
}, an = function(e, r, n) {
  for (var a = e.length - 1; a >= 0; a--)
    e[a] === r && e.splice(a, 1);
}, Qu = function(e) {
  e.splice(0, e.length);
}, vb = function(e, r) {
  for (var n = 0; n < r.length; n++) {
    var a = r[n];
    e.push(a);
  }
}, Nt = function(e, r, n) {
  return n && (r = $h(n, r)), e[r];
}, yr = function(e, r, n, a) {
  n && (r = $h(n, r)), e[r] = a;
}, db = /* @__PURE__ */ (function() {
  function t() {
    fn(this, t), this._obj = {};
  }
  return cn(t, [{
    key: "set",
    value: function(r, n) {
      return this._obj[r] = n, this;
    }
  }, {
    key: "delete",
    value: function(r) {
      return this._obj[r] = void 0, this;
    }
  }, {
    key: "clear",
    value: function() {
      this._obj = {};
    }
  }, {
    key: "has",
    value: function(r) {
      return this._obj[r] !== void 0;
    }
  }, {
    key: "get",
    value: function(r) {
      return this._obj[r];
    }
  }]);
})(), Nr = typeof Map < "u" ? Map : db, hb = "undefined", gb = /* @__PURE__ */ (function() {
  function t(e) {
    if (fn(this, t), this._obj = /* @__PURE__ */ Object.create(null), this.size = 0, e != null) {
      var r;
      e.instanceString != null && e.instanceString() === this.instanceString() ? r = e.toArray() : r = e;
      for (var n = 0; n < r.length; n++)
        this.add(r[n]);
    }
  }
  return cn(t, [{
    key: "instanceString",
    value: function() {
      return "set";
    }
  }, {
    key: "add",
    value: function(r) {
      var n = this._obj;
      n[r] !== 1 && (n[r] = 1, this.size++);
    }
  }, {
    key: "delete",
    value: function(r) {
      var n = this._obj;
      n[r] === 1 && (n[r] = 0, this.size--);
    }
  }, {
    key: "clear",
    value: function() {
      this._obj = /* @__PURE__ */ Object.create(null);
    }
  }, {
    key: "has",
    value: function(r) {
      return this._obj[r] === 1;
    }
  }, {
    key: "toArray",
    value: function() {
      var r = this;
      return Object.keys(this._obj).filter(function(n) {
        return r.has(n);
      });
    }
  }, {
    key: "forEach",
    value: function(r, n) {
      return this.toArray().forEach(r, n);
    }
  }]);
})(), pa = (typeof Set > "u" ? "undefined" : dt(Set)) !== hb ? Set : gb, Ws = function(e, r) {
  var n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : !0;
  if (e === void 0 || r === void 0 || !Wu(e)) {
    je("An element must have a core reference and parameters set");
    return;
  }
  var a = r.group;
  if (a == null && (r.data && r.data.source != null && r.data.target != null ? a = "edges" : a = "nodes"), a !== "nodes" && a !== "edges") {
    je("An element must be of type `nodes` or `edges`; you specified `" + a + "`");
    return;
  }
  this.length = 1, this[0] = this;
  var i = this._private = {
    cy: e,
    single: !0,
    // indicates this is an element
    data: r.data || {},
    // data object
    position: r.position || {
      x: 0,
      y: 0
    },
    // (x, y) position pair
    autoWidth: void 0,
    // width and height of nodes calculated by the renderer when set to special 'auto' value
    autoHeight: void 0,
    autoPadding: void 0,
    compoundBoundsClean: !1,
    // whether the compound dimensions need to be recalculated the next time dimensions are read
    listeners: [],
    // array of bound listeners
    group: a,
    // string; 'nodes' or 'edges'
    style: {},
    // properties as set by the style
    rstyle: {},
    // properties for style sent from the renderer to the core
    styleCxts: [],
    // applied style contexts from the styler
    styleKeys: {},
    // per-group keys of style property values
    removed: !0,
    // whether it's inside the vis; true if removed (set true here since we call restore)
    selected: !!r.selected,
    // whether it's selected
    selectable: r.selectable === void 0 ? !0 : !!r.selectable,
    // whether it's selectable
    locked: !!r.locked,
    // whether the element is locked (cannot be moved)
    grabbed: !1,
    // whether the element is grabbed by the mouse; renderer sets this privately
    grabbable: r.grabbable === void 0 ? !0 : !!r.grabbable,
    // whether the element can be grabbed
    pannable: r.pannable === void 0 ? a === "edges" : !!r.pannable,
    // whether the element has passthrough panning enabled
    active: !1,
    // whether the element is active from user interaction
    classes: new pa(),
    // map ( className => true )
    animation: {
      // object for currently-running animations
      current: [],
      queue: []
    },
    rscratch: {},
    // object in which the renderer can store information
    scratch: r.scratch || {},
    // scratch objects
    edges: [],
    // array of connected edges
    children: [],
    // array of children
    parent: r.parent && r.parent.isNode() ? r.parent : null,
    // parent ref
    traversalCache: {},
    // cache of output of traversal functions
    backgrounding: !1,
    // whether background images are loading
    bbCache: null,
    // cache of the current bounding box
    bbCacheShift: {
      x: 0,
      y: 0
    },
    // shift applied to cached bb to be applied on next get
    bodyBounds: null,
    // bounds cache of element body, w/o overlay
    overlayBounds: null,
    // bounds cache of element body, including overlay
    labelBounds: {
      // bounds cache of labels
      all: null,
      source: null,
      target: null,
      main: null
    },
    arrowBounds: {
      // bounds cache of edge arrows
      source: null,
      target: null,
      "mid-source": null,
      "mid-target": null
    }
  };
  if (i.position.x == null && (i.position.x = 0), i.position.y == null && (i.position.y = 0), r.renderedPosition) {
    var s = r.renderedPosition, o = e.pan(), l = e.zoom();
    i.position = {
      x: (s.x - o.x) / l,
      y: (s.y - o.y) / l
    };
  }
  var u = [];
  We(r.classes) ? u = r.classes : Se(r.classes) && (u = r.classes.split(/\s+/));
  for (var f = 0, c = u.length; f < c; f++) {
    var v = u[f];
    !v || v === "" || i.classes.add(v);
  }
  this.createEmitter(), (n === void 0 || n) && this.restore();
  var d = r.style || r.css;
  d && ($e("Setting a `style` bypass at element creation should be done only when absolutely necessary.  Try to use the stylesheet instead."), this.style(d));
}, bc = function(e) {
  return e = {
    bfs: e.bfs || !e.dfs,
    dfs: e.dfs || !e.bfs
  }, function(n, a, i) {
    var s;
    _e(n) && !Xt(n) && (s = n, n = s.roots || s.root, a = s.visit, i = s.directed), i = arguments.length === 2 && !tt(a) ? a : i, a = tt(a) ? a : function() {
    };
    for (var o = this._private.cy, l = n = Se(n) ? this.filter(n) : n, u = [], f = [], c = {}, v = {}, d = {}, h = 0, y, g = this.byGroup(), p = g.nodes, m = g.edges, b = 0; b < l.length; b++) {
      var w = l[b], E = w.id();
      w.isNode() && (u.unshift(w), e.bfs && (d[E] = !0, f.push(w)), v[E] = 0);
    }
    for (var T = function() {
      var M = e.bfs ? u.shift() : u.pop(), I = M.id();
      if (e.dfs) {
        if (d[I])
          return 0;
        d[I] = !0, f.push(M);
      }
      var _ = v[I], N = c[I], L = N != null ? N.source() : null, F = N != null ? N.target() : null, H = N == null ? void 0 : M.same(L) ? F[0] : L[0], V;
      if (V = a(M, N, H, h++, _), V === !0)
        return y = M, 1;
      if (V === !1)
        return 1;
      for (var O = M.connectedEdges().filter(function(le) {
        return (!i || le.source().same(M)) && m.has(le);
      }), $ = 0; $ < O.length; $++) {
        var Q = O[$], se = Q.connectedNodes().filter(function(le) {
          return !le.same(M) && p.has(le);
        }), ae = se.id();
        se.length !== 0 && !d[ae] && (se = se[0], u.push(se), e.bfs && (d[ae] = !0, f.push(se)), c[ae] = Q, v[ae] = v[I] + 1);
      }
    }, x; u.length !== 0 && (x = T(), !(x !== 0 && x === 1)); )
      ;
    for (var S = o.collection(), D = 0; D < f.length; D++) {
      var A = f[D], k = c[A.id()];
      k != null && S.push(k), S.push(A);
    }
    return {
      path: o.collection(S),
      found: o.collection(y)
    };
  };
}, ni = {
  breadthFirstSearch: bc({
    bfs: !0
  }),
  depthFirstSearch: bc({
    dfs: !0
  })
};
ni.bfs = ni.breadthFirstSearch;
ni.dfs = ni.depthFirstSearch;
var Qi = { exports: {} }, pb = Qi.exports, wc;
function yb() {
  return wc || (wc = 1, (function(t, e) {
    (function() {
      var r, n, a, i, s, o, l, u, f, c, v, d, h, y, g;
      a = Math.floor, c = Math.min, n = function(p, m) {
        return p < m ? -1 : p > m ? 1 : 0;
      }, f = function(p, m, b, w, E) {
        var T;
        if (b == null && (b = 0), E == null && (E = n), b < 0)
          throw new Error("lo must be non-negative");
        for (w == null && (w = p.length); b < w; )
          T = a((b + w) / 2), E(m, p[T]) < 0 ? w = T : b = T + 1;
        return [].splice.apply(p, [b, b - b].concat(m)), m;
      }, o = function(p, m, b) {
        return b == null && (b = n), p.push(m), y(p, 0, p.length - 1, b);
      }, s = function(p, m) {
        var b, w;
        return m == null && (m = n), b = p.pop(), p.length ? (w = p[0], p[0] = b, g(p, 0, m)) : w = b, w;
      }, u = function(p, m, b) {
        var w;
        return b == null && (b = n), w = p[0], p[0] = m, g(p, 0, b), w;
      }, l = function(p, m, b) {
        var w;
        return b == null && (b = n), p.length && b(p[0], m) < 0 && (w = [p[0], m], m = w[0], p[0] = w[1], g(p, 0, b)), m;
      }, i = function(p, m) {
        var b, w, E, T, x, S;
        for (m == null && (m = n), T = (function() {
          S = [];
          for (var D = 0, A = a(p.length / 2); 0 <= A ? D < A : D > A; 0 <= A ? D++ : D--)
            S.push(D);
          return S;
        }).apply(this).reverse(), x = [], w = 0, E = T.length; w < E; w++)
          b = T[w], x.push(g(p, b, m));
        return x;
      }, h = function(p, m, b) {
        var w;
        if (b == null && (b = n), w = p.indexOf(m), w !== -1)
          return y(p, 0, w, b), g(p, w, b);
      }, v = function(p, m, b) {
        var w, E, T, x, S;
        if (b == null && (b = n), E = p.slice(0, m), !E.length)
          return E;
        for (i(E, b), S = p.slice(m), T = 0, x = S.length; T < x; T++)
          w = S[T], l(E, w, b);
        return E.sort(b).reverse();
      }, d = function(p, m, b) {
        var w, E, T, x, S, D, A, k, R;
        if (b == null && (b = n), m * 10 <= p.length) {
          if (T = p.slice(0, m).sort(b), !T.length)
            return T;
          for (E = T[T.length - 1], A = p.slice(m), x = 0, D = A.length; x < D; x++)
            w = A[x], b(w, E) < 0 && (f(T, w, 0, null, b), T.pop(), E = T[T.length - 1]);
          return T;
        }
        for (i(p, b), R = [], S = 0, k = c(m, p.length); 0 <= k ? S < k : S > k; 0 <= k ? ++S : --S)
          R.push(s(p, b));
        return R;
      }, y = function(p, m, b, w) {
        var E, T, x;
        for (w == null && (w = n), E = p[b]; b > m; ) {
          if (x = b - 1 >> 1, T = p[x], w(E, T) < 0) {
            p[b] = T, b = x;
            continue;
          }
          break;
        }
        return p[b] = E;
      }, g = function(p, m, b) {
        var w, E, T, x, S;
        for (b == null && (b = n), E = p.length, S = m, T = p[m], w = 2 * m + 1; w < E; )
          x = w + 1, x < E && !(b(p[w], p[x]) < 0) && (w = x), p[m] = p[w], m = w, w = 2 * m + 1;
        return p[m] = T, y(p, S, m, b);
      }, r = (function() {
        p.push = o, p.pop = s, p.replace = u, p.pushpop = l, p.heapify = i, p.updateItem = h, p.nlargest = v, p.nsmallest = d;
        function p(m) {
          this.cmp = m ?? n, this.nodes = [];
        }
        return p.prototype.push = function(m) {
          return o(this.nodes, m, this.cmp);
        }, p.prototype.pop = function() {
          return s(this.nodes, this.cmp);
        }, p.prototype.peek = function() {
          return this.nodes[0];
        }, p.prototype.contains = function(m) {
          return this.nodes.indexOf(m) !== -1;
        }, p.prototype.replace = function(m) {
          return u(this.nodes, m, this.cmp);
        }, p.prototype.pushpop = function(m) {
          return l(this.nodes, m, this.cmp);
        }, p.prototype.heapify = function() {
          return i(this.nodes, this.cmp);
        }, p.prototype.updateItem = function(m) {
          return h(this.nodes, m, this.cmp);
        }, p.prototype.clear = function() {
          return this.nodes = [];
        }, p.prototype.empty = function() {
          return this.nodes.length === 0;
        }, p.prototype.size = function() {
          return this.nodes.length;
        }, p.prototype.clone = function() {
          var m;
          return m = new p(), m.nodes = this.nodes.slice(0), m;
        }, p.prototype.toArray = function() {
          return this.nodes.slice(0);
        }, p.prototype.insert = p.prototype.push, p.prototype.top = p.prototype.peek, p.prototype.front = p.prototype.peek, p.prototype.has = p.prototype.contains, p.prototype.copy = p.prototype.clone, p;
      })(), (function(p, m) {
        return t.exports = m();
      })(this, function() {
        return r;
      });
    }).call(pb);
  })(Qi)), Qi.exports;
}
var _o, xc;
function mb() {
  return xc || (xc = 1, _o = yb()), _o;
}
var bb = mb(), xi = /* @__PURE__ */ yi(bb), wb = St({
  root: null,
  weight: function(e) {
    return 1;
  },
  directed: !1
}), xb = {
  dijkstra: function(e) {
    if (!_e(e)) {
      var r = arguments;
      e = {
        root: r[0],
        weight: r[1],
        directed: r[2]
      };
    }
    var n = wb(e), a = n.root, i = n.weight, s = n.directed, o = this, l = i, u = Se(a) ? this.filter(a)[0] : a[0], f = {}, c = {}, v = {}, d = this.byGroup(), h = d.nodes, y = d.edges;
    y.unmergeBy(function(_) {
      return _.isLoop();
    });
    for (var g = function(N) {
      return f[N.id()];
    }, p = function(N, L) {
      f[N.id()] = L, m.updateItem(N);
    }, m = new xi(function(_, N) {
      return g(_) - g(N);
    }), b = 0; b < h.length; b++) {
      var w = h[b];
      f[w.id()] = w.same(u) ? 0 : 1 / 0, m.push(w);
    }
    for (var E = function(N, L) {
      for (var F = (s ? N.edgesTo(L) : N.edgesWith(L)).intersect(y), H = 1 / 0, V, O = 0; O < F.length; O++) {
        var $ = F[O], Q = l($);
        (Q < H || !V) && (H = Q, V = $);
      }
      return {
        edge: V,
        dist: H
      };
    }; m.size() > 0; ) {
      var T = m.pop(), x = g(T), S = T.id();
      if (v[S] = x, x !== 1 / 0)
        for (var D = T.neighborhood().intersect(h), A = 0; A < D.length; A++) {
          var k = D[A], R = k.id(), M = E(T, k), I = x + M.dist;
          I < g(k) && (p(k, I), c[R] = {
            node: T,
            edge: M.edge
          });
        }
    }
    return {
      distanceTo: function(N) {
        var L = Se(N) ? h.filter(N)[0] : N[0];
        return v[L.id()];
      },
      pathTo: function(N) {
        var L = Se(N) ? h.filter(N)[0] : N[0], F = [], H = L, V = H.id();
        if (L.length > 0)
          for (F.unshift(L); c[V]; ) {
            var O = c[V];
            F.unshift(O.edge), F.unshift(O.node), H = O.node, V = H.id();
          }
        return o.spawn(F);
      }
    };
  }
}, Eb = {
  // kruskal's algorithm (finds min spanning tree, assuming undirected graph)
  // implemented from pseudocode from wikipedia
  kruskal: function(e) {
    e = e || function(b) {
      return 1;
    };
    for (var r = this.byGroup(), n = r.nodes, a = r.edges, i = n.length, s = new Array(i), o = n, l = function(w) {
      for (var E = 0; E < s.length; E++) {
        var T = s[E];
        if (T.has(w))
          return E;
      }
    }, u = 0; u < i; u++)
      s[u] = this.spawn(n[u]);
    for (var f = a.sort(function(b, w) {
      return e(b) - e(w);
    }), c = 0; c < f.length; c++) {
      var v = f[c], d = v.source()[0], h = v.target()[0], y = l(d), g = l(h), p = s[y], m = s[g];
      y !== g && (o.merge(v), p.merge(m), s.splice(g, 1));
    }
    return o;
  }
}, Cb = St({
  root: null,
  goal: null,
  weight: function(e) {
    return 1;
  },
  heuristic: function(e) {
    return 0;
  },
  directed: !1
}), Tb = {
  // Implemented from pseudocode from wikipedia
  aStar: function(e) {
    var r = this.cy(), n = Cb(e), a = n.root, i = n.goal, s = n.heuristic, o = n.directed, l = n.weight;
    a = r.collection(a)[0], i = r.collection(i)[0];
    var u = a.id(), f = i.id(), c = {}, v = {}, d = {}, h = new xi(function(V, O) {
      return v[V.id()] - v[O.id()];
    }), y = new pa(), g = {}, p = {}, m = function(O, $) {
      h.push(O), y.add($);
    }, b, w, E = function() {
      b = h.pop(), w = b.id(), y.delete(w);
    }, T = function(O) {
      return y.has(O);
    };
    m(a, u), c[u] = 0, v[u] = s(a);
    for (var x = 0; h.size() > 0; ) {
      if (E(), x++, w === f) {
        for (var S = [], D = i, A = f, k = p[A]; S.unshift(D), k != null && S.unshift(k), D = g[A], D != null; )
          A = D.id(), k = p[A];
        return {
          found: !0,
          distance: c[w],
          path: this.spawn(S),
          steps: x
        };
      }
      d[w] = !0;
      for (var R = b._private.edges, M = 0; M < R.length; M++) {
        var I = R[M];
        if (this.hasElementWithId(I.id()) && !(o && I.data("source") !== w)) {
          var _ = I.source(), N = I.target(), L = _.id() !== w ? _ : N, F = L.id();
          if (this.hasElementWithId(F) && !d[F]) {
            var H = c[w] + l(I);
            if (!T(F)) {
              c[F] = H, v[F] = H + s(L), m(L, F), g[F] = b, p[F] = I;
              continue;
            }
            H < c[F] && (c[F] = H, v[F] = H + s(L), g[F] = b, p[F] = I);
          }
        }
      }
    }
    return {
      found: !1,
      distance: void 0,
      path: void 0,
      steps: x
    };
  }
}, Sb = St({
  weight: function(e) {
    return 1;
  },
  directed: !1
}), Pb = {
  // Implemented from pseudocode from wikipedia
  floydWarshall: function(e) {
    for (var r = this.cy(), n = Sb(e), a = n.weight, i = n.directed, s = a, o = this.byGroup(), l = o.nodes, u = o.edges, f = l.length, c = f * f, v = function(Q) {
      return l.indexOf(Q);
    }, d = function(Q) {
      return l[Q];
    }, h = new Array(c), y = 0; y < c; y++) {
      var g = y % f, p = (y - g) / f;
      p === g ? h[y] = 0 : h[y] = 1 / 0;
    }
    for (var m = new Array(c), b = new Array(c), w = 0; w < u.length; w++) {
      var E = u[w], T = E.source()[0], x = E.target()[0];
      if (T !== x) {
        var S = v(T), D = v(x), A = S * f + D, k = s(E);
        if (h[A] > k && (h[A] = k, m[A] = D, b[A] = E), !i) {
          var R = D * f + S;
          !i && h[R] > k && (h[R] = k, m[R] = S, b[R] = E);
        }
      }
    }
    for (var M = 0; M < f; M++)
      for (var I = 0; I < f; I++)
        for (var _ = I * f + M, N = 0; N < f; N++) {
          var L = I * f + N, F = M * f + N;
          h[_] + h[F] < h[L] && (h[L] = h[_] + h[F], m[L] = m[_]);
        }
    var H = function(Q) {
      return (Se(Q) ? r.filter(Q) : Q)[0];
    }, V = function(Q) {
      return v(H(Q));
    }, O = {
      distance: function(Q, se) {
        var ae = V(Q), le = V(se);
        return h[ae * f + le];
      },
      path: function(Q, se) {
        var ae = V(Q), le = V(se), ce = d(ae);
        if (ae === le)
          return ce.collection();
        if (m[ae * f + le] == null)
          return r.collection();
        var he = r.collection(), ie = ae, U;
        for (he.merge(ce); ae !== le; )
          ie = ae, ae = m[ae * f + le], U = b[ie * f + ae], he.merge(U), he.merge(d(ae));
        return he;
      }
    };
    return O;
  }
  // floydWarshall
}, Db = St({
  weight: function(e) {
    return 1;
  },
  directed: !1,
  root: null
}), Ab = {
  // Implemented from pseudocode from wikipedia
  bellmanFord: function(e) {
    var r = this, n = Db(e), a = n.weight, i = n.directed, s = n.root, o = a, l = this, u = this.cy(), f = this.byGroup(), c = f.edges, v = f.nodes, d = v.length, h = new Nr(), y = !1, g = [];
    s = u.collection(s)[0], c.unmergeBy(function(Z) {
      return Z.isLoop();
    });
    for (var p = c.length, m = function(ne) {
      var te = h.get(ne.id());
      return te || (te = {}, h.set(ne.id(), te)), te;
    }, b = function(ne) {
      return (Se(ne) ? u.$(ne) : ne)[0];
    }, w = function(ne) {
      return m(b(ne)).dist;
    }, E = function(ne) {
      for (var te = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : s, Y = b(ne), K = [], ue = Y; ; ) {
        if (ue == null)
          return r.spawn();
        var oe = m(ue), ve = oe.edge, de = oe.pred;
        if (K.unshift(ue[0]), ue.same(te) && K.length > 0)
          break;
        ve != null && K.unshift(ve), ue = de;
      }
      return l.spawn(K);
    }, T = 0; T < d; T++) {
      var x = v[T], S = m(x);
      x.same(s) ? S.dist = 0 : S.dist = 1 / 0, S.pred = null, S.edge = null;
    }
    for (var D = !1, A = function(ne, te, Y, K, ue, oe) {
      var ve = K.dist + oe;
      ve < ue.dist && !Y.same(K.edge) && (ue.dist = ve, ue.pred = ne, ue.edge = Y, D = !0);
    }, k = 1; k < d; k++) {
      D = !1;
      for (var R = 0; R < p; R++) {
        var M = c[R], I = M.source(), _ = M.target(), N = o(M), L = m(I), F = m(_);
        A(I, _, M, L, F, N), i || A(_, I, M, F, L, N);
      }
      if (!D)
        break;
    }
    if (D)
      for (var H = [], V = 0; V < p; V++) {
        var O = c[V], $ = O.source(), Q = O.target(), se = o(O), ae = m($).dist, le = m(Q).dist;
        if (ae + se < le || !i && le + se < ae)
          if (y || ($e("Graph contains a negative weight cycle for Bellman-Ford"), y = !0), e.findNegativeWeightCycles !== !1) {
            var ce = [];
            ae + se < le && ce.push($), !i && le + se < ae && ce.push(Q);
            for (var he = ce.length, ie = 0; ie < he; ie++) {
              var U = ce[ie], X = [U];
              X.push(m(U).edge);
              for (var C = m(U).pred; X.indexOf(C) === -1; )
                X.push(C), X.push(m(C).edge), C = m(C).pred;
              X = X.slice(X.indexOf(C));
              for (var B = X[0].id(), z = 0, W = 2; W < X.length; W += 2)
                X[W].id() < B && (B = X[W].id(), z = W);
              X = X.slice(z).concat(X.slice(0, z)), X.push(X[0]);
              var j = X.map(function(Z) {
                return Z.id();
              }).join(",");
              H.indexOf(j) === -1 && (g.push(l.spawn(X)), H.push(j));
            }
          } else
            break;
      }
    return {
      distanceTo: w,
      pathTo: E,
      hasNegativeWeightCycle: y,
      negativeWeightCycles: g
    };
  }
  // bellmanFord
}, kb = Math.sqrt(2), Bb = function(e, r, n) {
  n.length === 0 && je("Karger-Stein must be run on a connected (sub)graph");
  for (var a = n[e], i = a[1], s = a[2], o = r[i], l = r[s], u = n, f = u.length - 1; f >= 0; f--) {
    var c = u[f], v = c[1], d = c[2];
    (r[v] === o && r[d] === l || r[v] === l && r[d] === o) && u.splice(f, 1);
  }
  for (var h = 0; h < u.length; h++) {
    var y = u[h];
    y[1] === l ? (u[h] = y.slice(), u[h][1] = o) : y[2] === l && (u[h] = y.slice(), u[h][2] = o);
  }
  for (var g = 0; g < r.length; g++)
    r[g] === l && (r[g] = o);
  return u;
}, Fo = function(e, r, n, a) {
  for (; n > a; ) {
    var i = Math.floor(Math.random() * r.length);
    r = Bb(i, e, r), n--;
  }
  return r;
}, Rb = {
  // Computes the minimum cut of an undirected graph
  // Returns the correct answer with high probability
  kargerStein: function() {
    var e = this, r = this.byGroup(), n = r.nodes, a = r.edges;
    a.unmergeBy(function(F) {
      return F.isLoop();
    });
    var i = n.length, s = a.length, o = Math.ceil(Math.pow(Math.log(i) / Math.LN2, 2)), l = Math.floor(i / kb);
    if (i < 2) {
      je("At least 2 nodes are required for Karger-Stein algorithm");
      return;
    }
    for (var u = [], f = 0; f < s; f++) {
      var c = a[f];
      u.push([f, n.indexOf(c.source()), n.indexOf(c.target())]);
    }
    for (var v = 1 / 0, d = [], h = new Array(i), y = new Array(i), g = new Array(i), p = function(H, V) {
      for (var O = 0; O < i; O++)
        V[O] = H[O];
    }, m = 0; m <= o; m++) {
      for (var b = 0; b < i; b++)
        y[b] = b;
      var w = Fo(y, u.slice(), i, l), E = w.slice();
      p(y, g);
      var T = Fo(y, w, l, 2), x = Fo(g, E, l, 2);
      T.length <= x.length && T.length < v ? (v = T.length, d = T, p(y, h)) : x.length <= T.length && x.length < v && (v = x.length, d = x, p(g, h));
    }
    for (var S = this.spawn(d.map(function(F) {
      return a[F[0]];
    })), D = this.spawn(), A = this.spawn(), k = h[0], R = 0; R < h.length; R++) {
      var M = h[R], I = n[R];
      M === k ? D.merge(I) : A.merge(I);
    }
    var _ = function(H) {
      var V = e.spawn();
      return H.forEach(function(O) {
        V.merge(O), O.connectedEdges().forEach(function($) {
          e.contains($) && !S.contains($) && V.merge($);
        });
      }), V;
    }, N = [_(D), _(A)], L = {
      cut: S,
      components: N,
      // n.b. partitions are included to be compatible with the old api spec
      // (could be removed in a future major version)
      partition1: D,
      partition2: A
    };
    return L;
  }
}, zo, Mb = function(e) {
  return {
    x: e.x,
    y: e.y
  };
}, Ks = function(e, r, n) {
  return {
    x: e.x * r + n.x,
    y: e.y * r + n.y
  };
}, rg = function(e, r, n) {
  return {
    x: (e.x - n.x) / r,
    y: (e.y - n.y) / r
  };
}, Zn = function(e) {
  return {
    x: e[0],
    y: e[1]
  };
}, Lb = function(e) {
  for (var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0, n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : e.length, a = 1 / 0, i = r; i < n; i++) {
    var s = e[i];
    isFinite(s) && (a = Math.min(s, a));
  }
  return a;
}, Ib = function(e) {
  for (var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0, n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : e.length, a = -1 / 0, i = r; i < n; i++) {
    var s = e[i];
    isFinite(s) && (a = Math.max(s, a));
  }
  return a;
}, Ob = function(e) {
  for (var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0, n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : e.length, a = 0, i = 0, s = r; s < n; s++) {
    var o = e[s];
    isFinite(o) && (a += o, i++);
  }
  return a / i;
}, Nb = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0, n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : e.length, a = arguments.length > 3 && arguments[3] !== void 0 ? arguments[3] : !0, i = arguments.length > 4 && arguments[4] !== void 0 ? arguments[4] : !0, s = arguments.length > 5 && arguments[5] !== void 0 ? arguments[5] : !0;
  a ? e = e.slice(r, n) : (n < e.length && e.splice(n, e.length - n), r > 0 && e.splice(0, r));
  for (var o = 0, l = e.length - 1; l >= 0; l--) {
    var u = e[l];
    s ? isFinite(u) || (e[l] = -1 / 0, o++) : e.splice(l, 1);
  }
  i && e.sort(function(v, d) {
    return v - d;
  });
  var f = e.length, c = Math.floor(f / 2);
  return f % 2 !== 0 ? e[c + 1 + o] : (e[c - 1 + o] + e[c + o]) / 2;
}, _b = function(e) {
  return Math.PI * e / 180;
}, Oi = function(e, r) {
  return Math.atan2(r, e) - Math.PI / 2;
}, ju = Math.log2 || function(t) {
  return Math.log(t) / Math.log(2);
}, Ju = function(e) {
  return e > 0 ? 1 : e < 0 ? -1 : 0;
}, Rn = function(e, r) {
  return Math.sqrt(xn(e, r));
}, xn = function(e, r) {
  var n = r.x - e.x, a = r.y - e.y;
  return n * n + a * a;
}, Fb = function(e) {
  for (var r = e.length, n = 0, a = 0; a < r; a++)
    n += e[a];
  for (var i = 0; i < r; i++)
    e[i] = e[i] / n;
  return e;
}, yt = function(e, r, n, a) {
  return (1 - a) * (1 - a) * e + 2 * (1 - a) * a * r + a * a * n;
}, aa = function(e, r, n, a) {
  return {
    x: yt(e.x, r.x, n.x, a),
    y: yt(e.y, r.y, n.y, a)
  };
}, zb = function(e, r, n, a) {
  var i = {
    x: r.x - e.x,
    y: r.y - e.y
  }, s = Rn(e, r), o = {
    x: i.x / s,
    y: i.y / s
  };
  return n = n ?? 0, a = a ?? n * s, {
    x: e.x + o.x * a,
    y: e.y + o.y * a
  };
}, ai = function(e, r, n) {
  return Math.max(e, Math.min(n, r));
}, zt = function(e) {
  if (e == null)
    return {
      x1: 1 / 0,
      y1: 1 / 0,
      x2: -1 / 0,
      y2: -1 / 0,
      w: 0,
      h: 0
    };
  if (e.x1 != null && e.y1 != null) {
    if (e.x2 != null && e.y2 != null && e.x2 >= e.x1 && e.y2 >= e.y1)
      return {
        x1: e.x1,
        y1: e.y1,
        x2: e.x2,
        y2: e.y2,
        w: e.x2 - e.x1,
        h: e.y2 - e.y1
      };
    if (e.w != null && e.h != null && e.w >= 0 && e.h >= 0)
      return {
        x1: e.x1,
        y1: e.y1,
        x2: e.x1 + e.w,
        y2: e.y1 + e.h,
        w: e.w,
        h: e.h
      };
  }
}, Vb = function(e) {
  return {
    x1: e.x1,
    x2: e.x2,
    w: e.w,
    y1: e.y1,
    y2: e.y2,
    h: e.h
  };
}, qb = function(e) {
  e.x1 = 1 / 0, e.y1 = 1 / 0, e.x2 = -1 / 0, e.y2 = -1 / 0, e.w = 0, e.h = 0;
}, $b = function(e, r) {
  e.x1 = Math.min(e.x1, r.x1), e.x2 = Math.max(e.x2, r.x2), e.w = e.x2 - e.x1, e.y1 = Math.min(e.y1, r.y1), e.y2 = Math.max(e.y2, r.y2), e.h = e.y2 - e.y1;
}, ng = function(e, r, n) {
  e.x1 = Math.min(e.x1, r), e.x2 = Math.max(e.x2, r), e.w = e.x2 - e.x1, e.y1 = Math.min(e.y1, n), e.y2 = Math.max(e.y2, n), e.h = e.y2 - e.y1;
}, ji = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0;
  return e.x1 -= r, e.x2 += r, e.y1 -= r, e.y2 += r, e.w = e.x2 - e.x1, e.h = e.y2 - e.y1, e;
}, Ji = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : [0], n, a, i, s;
  if (r.length === 1)
    n = a = i = s = r[0];
  else if (r.length === 2)
    n = i = r[0], s = a = r[1];
  else if (r.length === 4) {
    var o = ut(r, 4);
    n = o[0], a = o[1], i = o[2], s = o[3];
  }
  return e.x1 -= s, e.x2 += a, e.y1 -= n, e.y2 += i, e.w = e.x2 - e.x1, e.h = e.y2 - e.y1, e;
}, Ec = function(e, r) {
  e.x1 = r.x1, e.y1 = r.y1, e.x2 = r.x2, e.y2 = r.y2, e.w = e.x2 - e.x1, e.h = e.y2 - e.y1;
}, ef = function(e, r) {
  return !(e.x1 > r.x2 || r.x1 > e.x2 || e.x2 < r.x1 || r.x2 < e.x1 || e.y2 < r.y1 || r.y2 < e.y1 || e.y1 > r.y2 || r.y1 > e.y2);
}, jr = function(e, r, n) {
  return e.x1 <= r && r <= e.x2 && e.y1 <= n && n <= e.y2;
}, Cc = function(e, r) {
  return jr(e, r.x, r.y);
}, ag = function(e, r) {
  return jr(e, r.x1, r.y1) && jr(e, r.x2, r.y2);
}, Hb = (zo = Math.hypot) !== null && zo !== void 0 ? zo : function(t, e) {
  return Math.sqrt(t * t + e * e);
};
function Ub(t, e) {
  if (t.length < 3)
    throw new Error("Need at least 3 vertices");
  var r = function(S, D) {
    return {
      x: S.x + D.x,
      y: S.y + D.y
    };
  }, n = function(S, D) {
    return {
      x: S.x - D.x,
      y: S.y - D.y
    };
  }, a = function(S, D) {
    return {
      x: S.x * D,
      y: S.y * D
    };
  }, i = function(S, D) {
    return S.x * D.y - S.y * D.x;
  }, s = function(S) {
    var D = Hb(S.x, S.y);
    return D === 0 ? {
      x: 0,
      y: 0
    } : {
      x: S.x / D,
      y: S.y / D
    };
  }, o = function(S) {
    for (var D = 0, A = 0; A < S.length; A++) {
      var k = S[A], R = S[(A + 1) % S.length];
      D += k.x * R.y - R.x * k.y;
    }
    return D / 2;
  }, l = function(S, D, A, k) {
    var R = n(D, S), M = n(k, A), I = i(R, M);
    if (Math.abs(I) < 1e-9)
      return r(S, a(R, 0.5));
    var _ = i(n(A, S), M) / I;
    return r(S, a(R, _));
  }, u = t.map(function(x) {
    return {
      x: x.x,
      y: x.y
    };
  });
  o(u) < 0 && u.reverse();
  for (var f = u.length, c = [], v = 0; v < f; v++) {
    var d = u[v], h = u[(v + 1) % f], y = n(h, d), g = s({
      x: y.y,
      y: -y.x
    });
    c.push(g);
  }
  for (var p = c.map(function(x, S) {
    var D = r(u[S], a(x, e)), A = r(u[(S + 1) % f], a(x, e));
    return {
      p1: D,
      p2: A
    };
  }), m = [], b = 0; b < f; b++) {
    var w = p[(b - 1 + f) % f], E = p[b], T = l(w.p1, w.p2, E.p1, E.p2);
    m.push(T);
  }
  return m;
}
function Gb(t, e, r, n, a, i) {
  var s = e1(t, e, r, n, a), o = Ub(s, i), l = zt();
  return o.forEach(function(u) {
    return ng(l, u.x, u.y);
  }), l;
}
var ig = function(e, r, n, a, i, s, o) {
  var l = arguments.length > 7 && arguments[7] !== void 0 ? arguments[7] : "auto", u = l === "auto" ? sn(i, s) : l, f = i / 2, c = s / 2;
  u = Math.min(u, f, c);
  var v = u !== f, d = u !== c, h;
  if (v) {
    var y = n - f + u - o, g = a - c - o, p = n + f - u + o, m = g;
    if (h = Jr(e, r, n, a, y, g, p, m, !1), h.length > 0)
      return h;
  }
  if (d) {
    var b = n + f + o, w = a - c + u - o, E = b, T = a + c - u + o;
    if (h = Jr(e, r, n, a, b, w, E, T, !1), h.length > 0)
      return h;
  }
  if (v) {
    var x = n - f + u - o, S = a + c + o, D = n + f - u + o, A = S;
    if (h = Jr(e, r, n, a, x, S, D, A, !1), h.length > 0)
      return h;
  }
  if (d) {
    var k = n - f - o, R = a - c + u - o, M = k, I = a + c - u + o;
    if (h = Jr(e, r, n, a, k, R, M, I, !1), h.length > 0)
      return h;
  }
  var _;
  {
    var N = n - f + u, L = a - c + u;
    if (_ = _a(e, r, n, a, N, L, u + o), _.length > 0 && _[0] <= N && _[1] <= L)
      return [_[0], _[1]];
  }
  {
    var F = n + f - u, H = a - c + u;
    if (_ = _a(e, r, n, a, F, H, u + o), _.length > 0 && _[0] >= F && _[1] <= H)
      return [_[0], _[1]];
  }
  {
    var V = n + f - u, O = a + c - u;
    if (_ = _a(e, r, n, a, V, O, u + o), _.length > 0 && _[0] >= V && _[1] >= O)
      return [_[0], _[1]];
  }
  {
    var $ = n - f + u, Q = a + c - u;
    if (_ = _a(e, r, n, a, $, Q, u + o), _.length > 0 && _[0] <= $ && _[1] >= Q)
      return [_[0], _[1]];
  }
  return [];
}, Wb = function(e, r, n, a, i, s, o) {
  var l = o, u = Math.min(n, i), f = Math.max(n, i), c = Math.min(a, s), v = Math.max(a, s);
  return u - l <= e && e <= f + l && c - l <= r && r <= v + l;
}, Kb = function(e, r, n, a, i, s, o, l, u) {
  var f = {
    x1: Math.min(n, o, i) - u,
    x2: Math.max(n, o, i) + u,
    y1: Math.min(a, l, s) - u,
    y2: Math.max(a, l, s) + u
  };
  return !(e < f.x1 || e > f.x2 || r < f.y1 || r > f.y2);
}, Yb = function(e, r, n, a) {
  n -= a;
  var i = r * r - 4 * e * n;
  if (i < 0)
    return [];
  var s = Math.sqrt(i), o = 2 * e, l = (-r + s) / o, u = (-r - s) / o;
  return [l, u];
}, Xb = function(e, r, n, a, i) {
  var s = 1e-5;
  e === 0 && (e = s), r /= e, n /= e, a /= e;
  var o, l, u, f, c, v, d, h;
  if (l = (3 * n - r * r) / 9, u = -(27 * a) + r * (9 * n - 2 * (r * r)), u /= 54, o = l * l * l + u * u, i[1] = 0, d = r / 3, o > 0) {
    c = u + Math.sqrt(o), c = c < 0 ? -Math.pow(-c, 1 / 3) : Math.pow(c, 1 / 3), v = u - Math.sqrt(o), v = v < 0 ? -Math.pow(-v, 1 / 3) : Math.pow(v, 1 / 3), i[0] = -d + c + v, d += (c + v) / 2, i[4] = i[2] = -d, d = Math.sqrt(3) * (-v + c) / 2, i[3] = d, i[5] = -d;
    return;
  }
  if (i[5] = i[3] = 0, o === 0) {
    h = u < 0 ? -Math.pow(-u, 1 / 3) : Math.pow(u, 1 / 3), i[0] = -d + 2 * h, i[4] = i[2] = -(h + d);
    return;
  }
  l = -l, f = l * l * l, f = Math.acos(u / Math.sqrt(f)), h = 2 * Math.sqrt(l), i[0] = -d + h * Math.cos(f / 3), i[2] = -d + h * Math.cos((f + 2 * Math.PI) / 3), i[4] = -d + h * Math.cos((f + 4 * Math.PI) / 3);
}, Zb = function(e, r, n, a, i, s, o, l) {
  var u = 1 * n * n - 4 * n * i + 2 * n * o + 4 * i * i - 4 * i * o + o * o + a * a - 4 * a * s + 2 * a * l + 4 * s * s - 4 * s * l + l * l, f = 9 * n * i - 3 * n * n - 3 * n * o - 6 * i * i + 3 * i * o + 9 * a * s - 3 * a * a - 3 * a * l - 6 * s * s + 3 * s * l, c = 3 * n * n - 6 * n * i + n * o - n * e + 2 * i * i + 2 * i * e - o * e + 3 * a * a - 6 * a * s + a * l - a * r + 2 * s * s + 2 * s * r - l * r, v = 1 * n * i - n * n + n * e - i * e + a * s - a * a + a * r - s * r, d = [];
  Xb(u, f, c, v, d);
  for (var h = 1e-7, y = [], g = 0; g < 6; g += 2)
    Math.abs(d[g + 1]) < h && d[g] >= 0 && d[g] <= 1 && y.push(d[g]);
  y.push(1), y.push(0);
  for (var p = -1, m, b, w, E = 0; E < y.length; E++)
    m = Math.pow(1 - y[E], 2) * n + 2 * (1 - y[E]) * y[E] * i + y[E] * y[E] * o, b = Math.pow(1 - y[E], 2) * a + 2 * (1 - y[E]) * y[E] * s + y[E] * y[E] * l, w = Math.pow(m - e, 2) + Math.pow(b - r, 2), p >= 0 ? w < p && (p = w) : p = w;
  return p;
}, Qb = function(e, r, n, a, i, s) {
  var o = [e - n, r - a], l = [i - n, s - a], u = l[0] * l[0] + l[1] * l[1], f = o[0] * o[0] + o[1] * o[1], c = o[0] * l[0] + o[1] * l[1], v = c * c / u;
  return c < 0 ? f : v > u ? (e - i) * (e - i) + (r - s) * (r - s) : f - v;
}, Ut = function(e, r, n) {
  for (var a, i, s, o, l, u = 0, f = 0; f < n.length / 2; f++)
    if (a = n[f * 2], i = n[f * 2 + 1], f + 1 < n.length / 2 ? (s = n[(f + 1) * 2], o = n[(f + 1) * 2 + 1]) : (s = n[(f + 1 - n.length / 2) * 2], o = n[(f + 1 - n.length / 2) * 2 + 1]), !(a == e && s == e)) if (a >= e && e >= s || a <= e && e <= s)
      l = (e - a) / (s - a) * (o - i) + i, l > r && u++;
    else
      continue;
  return u % 2 !== 0;
}, $r = function(e, r, n, a, i, s, o, l, u) {
  var f = new Array(n.length), c;
  l[0] != null ? (c = Math.atan(l[1] / l[0]), l[0] < 0 ? c = c + Math.PI / 2 : c = -c - Math.PI / 2) : c = l;
  for (var v = Math.cos(-c), d = Math.sin(-c), h = 0; h < f.length / 2; h++)
    f[h * 2] = s / 2 * (n[h * 2] * v - n[h * 2 + 1] * d), f[h * 2 + 1] = o / 2 * (n[h * 2 + 1] * v + n[h * 2] * d), f[h * 2] += a, f[h * 2 + 1] += i;
  var y;
  if (u > 0) {
    var g = Es(f, -u);
    y = xs(g);
  } else
    y = f;
  return Ut(e, r, y);
}, jb = function(e, r, n, a, i, s, o, l) {
  for (var u = new Array(n.length * 2), f = 0; f < l.length; f++) {
    var c = l[f];
    u[f * 4 + 0] = c.startX, u[f * 4 + 1] = c.startY, u[f * 4 + 2] = c.stopX, u[f * 4 + 3] = c.stopY;
    var v = Math.pow(c.cx - e, 2) + Math.pow(c.cy - r, 2);
    if (v <= Math.pow(c.radius, 2))
      return !0;
  }
  return Ut(e, r, u);
}, xs = function(e) {
  for (var r = new Array(e.length / 2), n, a, i, s, o, l, u, f, c = 0; c < e.length / 4; c++) {
    n = e[c * 4], a = e[c * 4 + 1], i = e[c * 4 + 2], s = e[c * 4 + 3], c < e.length / 4 - 1 ? (o = e[(c + 1) * 4], l = e[(c + 1) * 4 + 1], u = e[(c + 1) * 4 + 2], f = e[(c + 1) * 4 + 3]) : (o = e[0], l = e[1], u = e[2], f = e[3]);
    var v = Jr(n, a, i, s, o, l, u, f, !0);
    r[c * 2] = v[0], r[c * 2 + 1] = v[1];
  }
  return r;
}, Es = function(e, r) {
  for (var n = new Array(e.length * 2), a, i, s, o, l = 0; l < e.length / 2; l++) {
    a = e[l * 2], i = e[l * 2 + 1], l < e.length / 2 - 1 ? (s = e[(l + 1) * 2], o = e[(l + 1) * 2 + 1]) : (s = e[0], o = e[1]);
    var u = o - i, f = -(s - a), c = Math.sqrt(u * u + f * f), v = u / c, d = f / c;
    n[l * 4] = a + v * r, n[l * 4 + 1] = i + d * r, n[l * 4 + 2] = s + v * r, n[l * 4 + 3] = o + d * r;
  }
  return n;
}, Jb = function(e, r, n, a, i, s) {
  var o = n - e, l = a - r;
  o /= i, l /= s;
  var u = Math.sqrt(o * o + l * l), f = u - 1;
  if (f < 0)
    return [];
  var c = f / u;
  return [(n - e) * c + e, (a - r) * c + r];
}, Pn = function(e, r, n, a, i, s, o) {
  return e -= i, r -= s, e /= n / 2 + o, r /= a / 2 + o, e * e + r * r <= 1;
}, _a = function(e, r, n, a, i, s, o) {
  var l = [n - e, a - r], u = [e - i, r - s], f = l[0] * l[0] + l[1] * l[1], c = 2 * (u[0] * l[0] + u[1] * l[1]), v = u[0] * u[0] + u[1] * u[1] - o * o, d = c * c - 4 * f * v;
  if (d < 0)
    return [];
  var h = (-c + Math.sqrt(d)) / (2 * f), y = (-c - Math.sqrt(d)) / (2 * f), g = Math.min(h, y), p = Math.max(h, y), m = [];
  if (g >= 0 && g <= 1 && m.push(g), p >= 0 && p <= 1 && m.push(p), m.length === 0)
    return [];
  var b = m[0] * l[0] + e, w = m[0] * l[1] + r;
  if (m.length > 1) {
    if (m[0] == m[1])
      return [b, w];
    var E = m[1] * l[0] + e, T = m[1] * l[1] + r;
    return [b, w, E, T];
  } else
    return [b, w];
}, Vo = function(e, r, n) {
  return r <= e && e <= n || n <= e && e <= r ? e : e <= r && r <= n || n <= r && r <= e ? r : n;
}, Jr = function(e, r, n, a, i, s, o, l, u) {
  var f = e - i, c = n - e, v = o - i, d = r - s, h = a - r, y = l - s, g = v * d - y * f, p = c * d - h * f, m = y * c - v * h;
  if (m !== 0) {
    var b = g / m, w = p / m, E = 1e-3, T = 0 - E, x = 1 + E;
    return T <= b && b <= x && T <= w && w <= x ? [e + b * c, r + b * h] : u ? [e + b * c, r + b * h] : [];
  } else
    return g === 0 || p === 0 ? Vo(e, n, o) === o ? [o, l] : Vo(e, n, i) === i ? [i, s] : Vo(i, o, n) === n ? [n, a] : [] : [];
}, e1 = function(e, r, n, a, i) {
  var s = [], o = a / 2, l = i / 2, u = r, f = n;
  s.push({
    x: u + o * e[0],
    y: f + l * e[1]
  });
  for (var c = 1; c < e.length / 2; c++)
    s.push({
      x: u + o * e[c * 2],
      y: f + l * e[c * 2 + 1]
    });
  return s;
}, ii = function(e, r, n, a, i, s, o, l) {
  var u = [], f, c = new Array(n.length), v = !0;
  s == null && (v = !1);
  var d;
  if (v) {
    for (var h = 0; h < c.length / 2; h++)
      c[h * 2] = n[h * 2] * s + a, c[h * 2 + 1] = n[h * 2 + 1] * o + i;
    if (l > 0) {
      var y = Es(c, -l);
      d = xs(y);
    } else
      d = c;
  } else
    d = n;
  for (var g, p, m, b, w = 0; w < d.length / 2; w++)
    g = d[w * 2], p = d[w * 2 + 1], w < d.length / 2 - 1 ? (m = d[(w + 1) * 2], b = d[(w + 1) * 2 + 1]) : (m = d[0], b = d[1]), f = Jr(e, r, a, i, g, p, m, b), f.length !== 0 && u.push(f[0], f[1]);
  return u;
}, t1 = function(e, r, n, a, i, s, o, l, u) {
  var f = [], c, v = new Array(n.length * 2);
  u.forEach(function(m, b) {
    b === 0 ? (v[v.length - 2] = m.startX, v[v.length - 1] = m.startY) : (v[b * 4 - 2] = m.startX, v[b * 4 - 1] = m.startY), v[b * 4] = m.stopX, v[b * 4 + 1] = m.stopY, c = _a(e, r, a, i, m.cx, m.cy, m.radius), c.length !== 0 && f.push(c[0], c[1]);
  });
  for (var d = 0; d < v.length / 4; d++)
    c = Jr(e, r, a, i, v[d * 4], v[d * 4 + 1], v[d * 4 + 2], v[d * 4 + 3], !1), c.length !== 0 && f.push(c[0], c[1]);
  if (f.length > 2) {
    for (var h = [f[0], f[1]], y = Math.pow(h[0] - e, 2) + Math.pow(h[1] - r, 2), g = 1; g < f.length / 2; g++) {
      var p = Math.pow(f[g * 2] - e, 2) + Math.pow(f[g * 2 + 1] - r, 2);
      p <= y && (h[0] = f[g * 2], h[1] = f[g * 2 + 1], y = p);
    }
    return h;
  }
  return f;
}, Ni = function(e, r, n) {
  var a = [e[0] - r[0], e[1] - r[1]], i = Math.sqrt(a[0] * a[0] + a[1] * a[1]), s = (i - n) / i;
  return s < 0 && (s = 1e-5), [r[0] + s * a[0], r[1] + s * a[1]];
}, Ot = function(e, r) {
  var n = fu(e, r);
  return n = sg(n), n;
}, sg = function(e) {
  for (var r, n, a = e.length / 2, i = 1 / 0, s = 1 / 0, o = -1 / 0, l = -1 / 0, u = 0; u < a; u++)
    r = e[2 * u], n = e[2 * u + 1], i = Math.min(i, r), o = Math.max(o, r), s = Math.min(s, n), l = Math.max(l, n);
  for (var f = 2 / (o - i), c = 2 / (l - s), v = 0; v < a; v++)
    r = e[2 * v] = e[2 * v] * f, n = e[2 * v + 1] = e[2 * v + 1] * c, i = Math.min(i, r), o = Math.max(o, r), s = Math.min(s, n), l = Math.max(l, n);
  if (s < -1)
    for (var d = 0; d < a; d++)
      n = e[2 * d + 1] = e[2 * d + 1] + (-1 - s);
  return e;
}, fu = function(e, r) {
  var n = 1 / e * 2 * Math.PI, a = e % 2 === 0 ? Math.PI / 2 + n / 2 : Math.PI / 2;
  a += r;
  for (var i = new Array(e * 2), s, o = 0; o < e; o++)
    s = o * n + a, i[2 * o] = Math.cos(s), i[2 * o + 1] = Math.sin(-s);
  return i;
}, sn = function(e, r) {
  return Math.min(e / 4, r / 4, 8);
}, og = function(e, r) {
  return Math.min(e / 10, r / 10, 8);
}, tf = function() {
  return 8;
}, r1 = function(e, r, n) {
  return [e - 2 * r + n, 2 * (r - e), e];
}, cu = function(e, r) {
  return {
    heightOffset: Math.min(15, 0.05 * r),
    widthOffset: Math.min(100, 0.25 * e),
    ctrlPtOffsetPct: 0.05
  };
};
function qo(t, e) {
  function r(c) {
    for (var v = [], d = 0; d < c.length; d++) {
      var h = c[d], y = c[(d + 1) % c.length], g = {
        x: y.x - h.x,
        y: y.y - h.y
      }, p = {
        x: -g.y,
        y: g.x
      }, m = Math.sqrt(p.x * p.x + p.y * p.y);
      v.push({
        x: p.x / m,
        y: p.y / m
      });
    }
    return v;
  }
  function n(c, v) {
    var d = 1 / 0, h = -1 / 0, y = Gt(c), g;
    try {
      for (y.s(); !(g = y.n()).done; ) {
        var p = g.value, m = p.x * v.x + p.y * v.y;
        d = Math.min(d, m), h = Math.max(h, m);
      }
    } catch (b) {
      y.e(b);
    } finally {
      y.f();
    }
    return {
      min: d,
      max: h
    };
  }
  function a(c, v) {
    return !(c.max < v.min || v.max < c.min);
  }
  var i = [].concat(ys(r(t)), ys(r(e))), s = Gt(i), o;
  try {
    for (s.s(); !(o = s.n()).done; ) {
      var l = o.value, u = n(t, l), f = n(e, l);
      if (!a(u, f))
        return !1;
    }
  } catch (c) {
    s.e(c);
  } finally {
    s.f();
  }
  return !0;
}
var n1 = St({
  dampingFactor: 0.8,
  precision: 1e-6,
  iterations: 200,
  weight: function(e) {
    return 1;
  }
}), a1 = {
  pageRank: function(e) {
    for (var r = n1(e), n = r.dampingFactor, a = r.precision, i = r.iterations, s = r.weight, o = this._private.cy, l = this.byGroup(), u = l.nodes, f = l.edges, c = u.length, v = c * c, d = f.length, h = new Array(v), y = new Array(c), g = (1 - n) / c, p = 0; p < c; p++) {
      for (var m = 0; m < c; m++) {
        var b = p * c + m;
        h[b] = 0;
      }
      y[p] = 0;
    }
    for (var w = 0; w < d; w++) {
      var E = f[w], T = E.data("source"), x = E.data("target");
      if (T !== x) {
        var S = u.indexOfId(T), D = u.indexOfId(x), A = s(E), k = D * c + S;
        h[k] += A, y[S] += A;
      }
    }
    for (var R = 1 / c + g, M = 0; M < c; M++)
      if (y[M] === 0)
        for (var I = 0; I < c; I++) {
          var _ = I * c + M;
          h[_] = R;
        }
      else
        for (var N = 0; N < c; N++) {
          var L = N * c + M;
          h[L] = h[L] / y[M] + g;
        }
    for (var F = new Array(c), H = new Array(c), V, O = 0; O < c; O++)
      F[O] = 1;
    for (var $ = 0; $ < i; $++) {
      for (var Q = 0; Q < c; Q++)
        H[Q] = 0;
      for (var se = 0; se < c; se++)
        for (var ae = 0; ae < c; ae++) {
          var le = se * c + ae;
          H[se] += h[le] * F[ae];
        }
      Fb(H), V = F, F = H, H = V;
      for (var ce = 0, he = 0; he < c; he++) {
        var ie = V[he] - F[he];
        ce += ie * ie;
      }
      if (ce < a)
        break;
    }
    var U = {
      rank: function(C) {
        return C = o.collection(C)[0], F[u.indexOf(C)];
      }
    };
    return U;
  }
  // pageRank
}, Tc = St({
  root: null,
  weight: function(e) {
    return 1;
  },
  directed: !1,
  alpha: 0
}), ia = {
  degreeCentralityNormalized: function(e) {
    e = Tc(e);
    var r = this.cy(), n = this.nodes(), a = n.length;
    if (e.directed) {
      for (var f = {}, c = {}, v = 0, d = 0, h = 0; h < a; h++) {
        var y = n[h], g = y.id();
        e.root = y;
        var p = this.degreeCentrality(e);
        v < p.indegree && (v = p.indegree), d < p.outdegree && (d = p.outdegree), f[g] = p.indegree, c[g] = p.outdegree;
      }
      return {
        indegree: function(b) {
          return v == 0 ? 0 : (Se(b) && (b = r.filter(b)), f[b.id()] / v);
        },
        outdegree: function(b) {
          return d === 0 ? 0 : (Se(b) && (b = r.filter(b)), c[b.id()] / d);
        }
      };
    } else {
      for (var i = {}, s = 0, o = 0; o < a; o++) {
        var l = n[o];
        e.root = l;
        var u = this.degreeCentrality(e);
        s < u.degree && (s = u.degree), i[l.id()] = u.degree;
      }
      return {
        degree: function(b) {
          return s === 0 ? 0 : (Se(b) && (b = r.filter(b)), i[b.id()] / s);
        }
      };
    }
  },
  // degreeCentralityNormalized
  // Implemented from the algorithm in Opsahl's paper
  // "Node centrality in weighted networks: Generalizing degree and shortest paths"
  // check the heading 2 "Degree"
  degreeCentrality: function(e) {
    e = Tc(e);
    var r = this.cy(), n = this, a = e, i = a.root, s = a.weight, o = a.directed, l = a.alpha;
    if (i = r.collection(i)[0], o) {
      for (var d = i.connectedEdges(), h = d.filter(function(T) {
        return T.target().same(i) && n.has(T);
      }), y = d.filter(function(T) {
        return T.source().same(i) && n.has(T);
      }), g = h.length, p = y.length, m = 0, b = 0, w = 0; w < h.length; w++)
        m += s(h[w]);
      for (var E = 0; E < y.length; E++)
        b += s(y[E]);
      return {
        indegree: Math.pow(g, 1 - l) * Math.pow(m, l),
        outdegree: Math.pow(p, 1 - l) * Math.pow(b, l)
      };
    } else {
      for (var u = i.connectedEdges().intersection(n), f = u.length, c = 0, v = 0; v < u.length; v++)
        c += s(u[v]);
      return {
        degree: Math.pow(f, 1 - l) * Math.pow(c, l)
      };
    }
  }
  // degreeCentrality
};
ia.dc = ia.degreeCentrality;
ia.dcn = ia.degreeCentralityNormalised = ia.degreeCentralityNormalized;
var Sc = St({
  harmonic: !0,
  weight: function() {
    return 1;
  },
  directed: !1,
  root: null
}), sa = {
  closenessCentralityNormalized: function(e) {
    for (var r = Sc(e), n = r.harmonic, a = r.weight, i = r.directed, s = this.cy(), o = {}, l = 0, u = this.nodes(), f = this.floydWarshall({
      weight: a,
      directed: i
    }), c = 0; c < u.length; c++) {
      for (var v = 0, d = u[c], h = 0; h < u.length; h++)
        if (c !== h) {
          var y = f.distance(d, u[h]);
          n ? v += 1 / y : v += y;
        }
      n || (v = 1 / v), l < v && (l = v), o[d.id()] = v;
    }
    return {
      closeness: function(p) {
        return l == 0 ? 0 : (Se(p) ? p = s.filter(p)[0].id() : p = p.id(), o[p] / l);
      }
    };
  },
  // Implemented from pseudocode from wikipedia
  closenessCentrality: function(e) {
    var r = Sc(e), n = r.root, a = r.weight, i = r.directed, s = r.harmonic;
    n = this.filter(n)[0];
    for (var o = this.dijkstra({
      root: n,
      weight: a,
      directed: i
    }), l = 0, u = this.nodes(), f = 0; f < u.length; f++) {
      var c = u[f];
      if (!c.same(n)) {
        var v = o.distanceTo(c);
        s ? l += 1 / v : l += v;
      }
    }
    return s ? l : 1 / l;
  }
  // closenessCentrality
};
sa.cc = sa.closenessCentrality;
sa.ccn = sa.closenessCentralityNormalised = sa.closenessCentralityNormalized;
var i1 = St({
  weight: null,
  directed: !1
}), vu = {
  // Implemented from the algorithm in the paper "On Variants of Shortest-Path Betweenness Centrality and their Generic Computation" by Ulrik Brandes
  betweennessCentrality: function(e) {
    for (var r = i1(e), n = r.directed, a = r.weight, i = a != null, s = this.cy(), o = this.nodes(), l = {}, u = {}, f = 0, c = {
      set: function(b, w) {
        u[b] = w, w > f && (f = w);
      },
      get: function(b) {
        return u[b];
      }
    }, v = 0; v < o.length; v++) {
      var d = o[v], h = d.id();
      n ? l[h] = d.outgoers().nodes() : l[h] = d.openNeighborhood().nodes(), c.set(h, 0);
    }
    for (var y = function() {
      for (var b = o[g].id(), w = [], E = {}, T = {}, x = {}, S = new xi(function(se, ae) {
        return x[se] - x[ae];
      }), D = 0; D < o.length; D++) {
        var A = o[D].id();
        E[A] = [], T[A] = 0, x[A] = 1 / 0;
      }
      for (T[b] = 1, x[b] = 0, S.push(b); !S.empty(); ) {
        var k = S.pop();
        if (w.push(k), i)
          for (var R = 0; R < l[k].length; R++) {
            var M = l[k][R], I = s.getElementById(k), _ = void 0;
            I.edgesTo(M).length > 0 ? _ = I.edgesTo(M)[0] : _ = M.edgesTo(I)[0];
            var N = a(_);
            M = M.id(), x[M] > x[k] + N && (x[M] = x[k] + N, S.nodes.indexOf(M) < 0 ? S.push(M) : S.updateItem(M), T[M] = 0, E[M] = []), x[M] == x[k] + N && (T[M] = T[M] + T[k], E[M].push(k));
          }
        else
          for (var L = 0; L < l[k].length; L++) {
            var F = l[k][L].id();
            x[F] == 1 / 0 && (S.push(F), x[F] = x[k] + 1), x[F] == x[k] + 1 && (T[F] = T[F] + T[k], E[F].push(k));
          }
      }
      for (var H = {}, V = 0; V < o.length; V++)
        H[o[V].id()] = 0;
      for (; w.length > 0; ) {
        for (var O = w.pop(), $ = 0; $ < E[O].length; $++) {
          var Q = E[O][$];
          H[Q] = H[Q] + T[Q] / T[O] * (1 + H[O]);
        }
        O != o[g].id() && c.set(O, c.get(O) + H[O]);
      }
    }, g = 0; g < o.length; g++)
      y();
    var p = {
      betweenness: function(b) {
        var w = s.collection(b).id();
        return c.get(w);
      },
      betweennessNormalized: function(b) {
        if (f == 0)
          return 0;
        var w = s.collection(b).id();
        return c.get(w) / f;
      }
    };
    return p.betweennessNormalised = p.betweennessNormalized, p;
  }
  // betweennessCentrality
};
vu.bc = vu.betweennessCentrality;
var s1 = St({
  expandFactor: 2,
  // affects time of computation and cluster granularity to some extent: M * M
  inflateFactor: 2,
  // affects cluster granularity (the greater the value, the more clusters): M(i,j) / E(j)
  multFactor: 1,
  // optional self loops for each node. Use a neutral value to improve cluster computations.
  maxIterations: 20,
  // maximum number of iterations of the MCL algorithm in a single run
  attributes: [
    // attributes/features used to group nodes, ie. similarity values between nodes
    function(t) {
      return 1;
    }
  ]
}), o1 = function(e) {
  return s1(e);
}, l1 = function(e, r) {
  for (var n = 0, a = 0; a < r.length; a++)
    n += r[a](e);
  return n;
}, u1 = function(e, r, n) {
  for (var a = 0; a < r; a++)
    e[a * r + a] = n;
}, lg = function(e, r) {
  for (var n, a = 0; a < r; a++) {
    n = 0;
    for (var i = 0; i < r; i++)
      n += e[i * r + a];
    for (var s = 0; s < r; s++)
      e[s * r + a] = e[s * r + a] / n;
  }
}, f1 = function(e, r, n) {
  for (var a = new Array(n * n), i = 0; i < n; i++) {
    for (var s = 0; s < n; s++)
      a[i * n + s] = 0;
    for (var o = 0; o < n; o++)
      for (var l = 0; l < n; l++)
        a[i * n + l] += e[i * n + o] * r[o * n + l];
  }
  return a;
}, c1 = function(e, r, n) {
  for (var a = e.slice(0), i = 1; i < n; i++)
    e = f1(e, a, r);
  return e;
}, v1 = function(e, r, n) {
  for (var a = new Array(r * r), i = 0; i < r * r; i++)
    a[i] = Math.pow(e[i], n);
  return lg(a, r), a;
}, d1 = function(e, r, n, a) {
  for (var i = 0; i < n; i++) {
    var s = Math.round(e[i] * Math.pow(10, a)) / Math.pow(10, a), o = Math.round(r[i] * Math.pow(10, a)) / Math.pow(10, a);
    if (s !== o)
      return !1;
  }
  return !0;
}, h1 = function(e, r, n, a) {
  for (var i = [], s = 0; s < r; s++) {
    for (var o = [], l = 0; l < r; l++)
      Math.round(e[s * r + l] * 1e3) / 1e3 > 0 && o.push(n[l]);
    o.length !== 0 && i.push(a.collection(o));
  }
  return i;
}, g1 = function(e, r) {
  for (var n = 0; n < e.length; n++)
    if (!r[n] || e[n].id() !== r[n].id())
      return !1;
  return !0;
}, p1 = function(e) {
  for (var r = 0; r < e.length; r++)
    for (var n = 0; n < e.length; n++)
      r != n && g1(e[r], e[n]) && e.splice(n, 1);
  return e;
}, Pc = function(e) {
  for (var r = this.nodes(), n = this.edges(), a = this.cy(), i = o1(e), s = {}, o = 0; o < r.length; o++)
    s[r[o].id()] = o;
  for (var l = r.length, u = l * l, f = new Array(u), c, v = 0; v < u; v++)
    f[v] = 0;
  for (var d = 0; d < n.length; d++) {
    var h = n[d], y = s[h.source().id()], g = s[h.target().id()], p = l1(h, i.attributes);
    f[y * l + g] += p, f[g * l + y] += p;
  }
  u1(f, l, i.multFactor), lg(f, l);
  for (var m = !0, b = 0; m && b < i.maxIterations; )
    m = !1, c = c1(f, l, i.expandFactor), f = v1(c, l, i.inflateFactor), d1(f, c, u, 4) || (m = !0), b++;
  var w = h1(f, l, r, a);
  return w = p1(w), w;
}, y1 = {
  markovClustering: Pc,
  mcl: Pc
}, m1 = function(e) {
  return e;
}, ug = function(e, r) {
  return Math.abs(r - e);
}, Dc = function(e, r, n) {
  return e + ug(r, n);
}, Ac = function(e, r, n) {
  return e + Math.pow(n - r, 2);
}, b1 = function(e) {
  return Math.sqrt(e);
}, w1 = function(e, r, n) {
  return Math.max(e, ug(r, n));
}, Aa = function(e, r, n, a, i) {
  for (var s = arguments.length > 5 && arguments[5] !== void 0 ? arguments[5] : m1, o = a, l, u, f = 0; f < e; f++)
    l = r(f), u = n(f), o = i(o, l, u);
  return s(o);
}, va = {
  euclidean: function(e, r, n) {
    return e >= 2 ? Aa(e, r, n, 0, Ac, b1) : Aa(e, r, n, 0, Dc);
  },
  squaredEuclidean: function(e, r, n) {
    return Aa(e, r, n, 0, Ac);
  },
  manhattan: function(e, r, n) {
    return Aa(e, r, n, 0, Dc);
  },
  max: function(e, r, n) {
    return Aa(e, r, n, -1 / 0, w1);
  }
};
va["squared-euclidean"] = va.squaredEuclidean;
va.squaredeuclidean = va.squaredEuclidean;
function Ys(t, e, r, n, a, i) {
  var s;
  return tt(t) ? s = t : s = va[t] || va.euclidean, e === 0 && tt(t) ? s(a, i) : s(e, r, n, a, i);
}
var x1 = St({
  k: 2,
  m: 2,
  sensitivityThreshold: 1e-4,
  distance: "euclidean",
  maxIterations: 10,
  attributes: [],
  testMode: !1,
  testCentroids: null
}), rf = function(e) {
  return x1(e);
}, Cs = function(e, r, n, a, i) {
  var s = i !== "kMedoids", o = s ? function(c) {
    return n[c];
  } : function(c) {
    return a[c](n);
  }, l = function(v) {
    return a[v](r);
  }, u = n, f = r;
  return Ys(e, a.length, o, l, u, f);
}, $o = function(e, r, n) {
  for (var a = n.length, i = new Array(a), s = new Array(a), o = new Array(r), l = null, u = 0; u < a; u++)
    i[u] = e.min(n[u]).value, s[u] = e.max(n[u]).value;
  for (var f = 0; f < r; f++) {
    l = [];
    for (var c = 0; c < a; c++)
      l[c] = Math.random() * (s[c] - i[c]) + i[c];
    o[f] = l;
  }
  return o;
}, fg = function(e, r, n, a, i) {
  for (var s = 1 / 0, o = 0, l = 0; l < r.length; l++) {
    var u = Cs(n, e, r[l], a, i);
    u < s && (s = u, o = l);
  }
  return o;
}, cg = function(e, r, n) {
  for (var a = [], i = null, s = 0; s < r.length; s++)
    i = r[s], n[i.id()] === e && a.push(i);
  return a;
}, E1 = function(e, r, n) {
  return Math.abs(r - e) <= n;
}, C1 = function(e, r, n) {
  for (var a = 0; a < e.length; a++)
    for (var i = 0; i < e[a].length; i++) {
      var s = Math.abs(e[a][i] - r[a][i]);
      if (s > n)
        return !1;
    }
  return !0;
}, T1 = function(e, r, n) {
  for (var a = 0; a < n; a++)
    if (e === r[a]) return !0;
  return !1;
}, kc = function(e, r) {
  var n = new Array(r);
  if (e.length < 50)
    for (var a = 0; a < r; a++) {
      for (var i = e[Math.floor(Math.random() * e.length)]; T1(i, n, a); )
        i = e[Math.floor(Math.random() * e.length)];
      n[a] = i;
    }
  else
    for (var s = 0; s < r; s++)
      n[s] = e[Math.floor(Math.random() * e.length)];
  return n;
}, Bc = function(e, r, n) {
  for (var a = 0, i = 0; i < r.length; i++)
    a += Cs("manhattan", r[i], e, n, "kMedoids");
  return a;
}, S1 = function(e) {
  var r = this.cy(), n = this.nodes(), a = null, i = rf(e), s = new Array(i.k), o = {}, l;
  i.testMode ? typeof i.testCentroids == "number" ? (i.testCentroids, l = $o(n, i.k, i.attributes)) : dt(i.testCentroids) === "object" ? l = i.testCentroids : l = $o(n, i.k, i.attributes) : l = $o(n, i.k, i.attributes);
  for (var u = !0, f = 0; u && f < i.maxIterations; ) {
    for (var c = 0; c < n.length; c++)
      a = n[c], o[a.id()] = fg(a, l, i.distance, i.attributes, "kMeans");
    u = !1;
    for (var v = 0; v < i.k; v++) {
      var d = cg(v, n, o);
      if (d.length !== 0) {
        for (var h = i.attributes.length, y = l[v], g = new Array(h), p = new Array(h), m = 0; m < h; m++) {
          p[m] = 0;
          for (var b = 0; b < d.length; b++)
            a = d[b], p[m] += i.attributes[m](a);
          g[m] = p[m] / d.length, E1(g[m], y[m], i.sensitivityThreshold) || (u = !0);
        }
        l[v] = g, s[v] = r.collection(d);
      }
    }
    f++;
  }
  return s;
}, P1 = function(e) {
  var r = this.cy(), n = this.nodes(), a = null, i = rf(e), s = new Array(i.k), o, l = {}, u, f = new Array(i.k);
  i.testMode ? typeof i.testCentroids == "number" || (dt(i.testCentroids) === "object" ? o = i.testCentroids : o = kc(n, i.k)) : o = kc(n, i.k);
  for (var c = !0, v = 0; c && v < i.maxIterations; ) {
    for (var d = 0; d < n.length; d++)
      a = n[d], l[a.id()] = fg(a, o, i.distance, i.attributes, "kMedoids");
    c = !1;
    for (var h = 0; h < o.length; h++) {
      var y = cg(h, n, l);
      if (y.length !== 0) {
        f[h] = Bc(o[h], y, i.attributes);
        for (var g = 0; g < y.length; g++)
          u = Bc(y[g], y, i.attributes), u < f[h] && (f[h] = u, o[h] = y[g], c = !0);
        s[h] = r.collection(y);
      }
    }
    v++;
  }
  return s;
}, D1 = function(e, r, n, a, i) {
  for (var s, o, l = 0; l < r.length; l++)
    for (var u = 0; u < e.length; u++)
      a[l][u] = Math.pow(n[l][u], i.m);
  for (var f = 0; f < e.length; f++)
    for (var c = 0; c < i.attributes.length; c++) {
      s = 0, o = 0;
      for (var v = 0; v < r.length; v++)
        s += a[v][f] * i.attributes[c](r[v]), o += a[v][f];
      e[f][c] = s / o;
    }
}, A1 = function(e, r, n, a, i) {
  for (var s = 0; s < e.length; s++)
    r[s] = e[s].slice();
  for (var o, l, u, f = 2 / (i.m - 1), c = 0; c < n.length; c++)
    for (var v = 0; v < a.length; v++) {
      o = 0;
      for (var d = 0; d < n.length; d++)
        l = Cs(i.distance, a[v], n[c], i.attributes, "cmeans"), u = Cs(i.distance, a[v], n[d], i.attributes, "cmeans"), o += Math.pow(l / u, f);
      e[v][c] = 1 / o;
    }
}, k1 = function(e, r, n, a) {
  for (var i = new Array(n.k), s = 0; s < i.length; s++)
    i[s] = [];
  for (var o, l, u = 0; u < r.length; u++) {
    o = -1 / 0, l = -1;
    for (var f = 0; f < r[0].length; f++)
      r[u][f] > o && (o = r[u][f], l = f);
    i[l].push(e[u]);
  }
  for (var c = 0; c < i.length; c++)
    i[c] = a.collection(i[c]);
  return i;
}, Rc = function(e) {
  var r = this.cy(), n = this.nodes(), a = rf(e), i, s, o, l, u;
  l = new Array(n.length);
  for (var f = 0; f < n.length; f++)
    l[f] = new Array(a.k);
  o = new Array(n.length);
  for (var c = 0; c < n.length; c++)
    o[c] = new Array(a.k);
  for (var v = 0; v < n.length; v++) {
    for (var d = 0, h = 0; h < a.k; h++)
      o[v][h] = Math.random(), d += o[v][h];
    for (var y = 0; y < a.k; y++)
      o[v][y] = o[v][y] / d;
  }
  s = new Array(a.k);
  for (var g = 0; g < a.k; g++)
    s[g] = new Array(a.attributes.length);
  u = new Array(n.length);
  for (var p = 0; p < n.length; p++)
    u[p] = new Array(a.k);
  for (var m = !0, b = 0; m && b < a.maxIterations; )
    m = !1, D1(s, n, o, u, a), A1(o, l, s, n, a), C1(o, l, a.sensitivityThreshold) || (m = !0), b++;
  return i = k1(n, o, a, r), {
    clusters: i,
    degreeOfMembership: o
  };
}, B1 = {
  kMeans: S1,
  kMedoids: P1,
  fuzzyCMeans: Rc,
  fcm: Rc
}, R1 = St({
  distance: "euclidean",
  // distance metric to compare nodes
  linkage: "min",
  // linkage criterion : how to determine the distance between clusters of nodes
  mode: "threshold",
  // mode:'threshold' => clusters must be threshold distance apart
  threshold: 1 / 0,
  // the distance threshold
  // mode:'dendrogram' => the nodes are organised as leaves in a tree (siblings are close), merging makes clusters
  addDendrogram: !1,
  // whether to add the dendrogram to the graph for viz
  dendrogramDepth: 0,
  // depth at which dendrogram branches are merged into the returned clusters
  attributes: []
  // array of attr functions
}), M1 = {
  single: "min",
  complete: "max"
}, L1 = function(e) {
  var r = R1(e), n = M1[r.linkage];
  return n != null && (r.linkage = n), r;
}, Mc = function(e, r, n, a, i) {
  for (var s = 0, o = 1 / 0, l, u = i.attributes, f = function(D, A) {
    return Ys(i.distance, u.length, function(k) {
      return u[k](D);
    }, function(k) {
      return u[k](A);
    }, D, A);
  }, c = 0; c < e.length; c++) {
    var v = e[c].key, d = n[v][a[v]];
    d < o && (s = v, o = d);
  }
  if (i.mode === "threshold" && o >= i.threshold || i.mode === "dendrogram" && e.length === 1)
    return !1;
  var h = r[s], y = r[a[s]], g;
  i.mode === "dendrogram" ? g = {
    left: h,
    right: y,
    key: h.key
  } : g = {
    value: h.value.concat(y.value),
    key: h.key
  }, e[h.index] = g, e.splice(y.index, 1), r[h.key] = g;
  for (var p = 0; p < e.length; p++) {
    var m = e[p];
    h.key === m.key ? l = 1 / 0 : i.linkage === "min" ? (l = n[h.key][m.key], n[h.key][m.key] > n[y.key][m.key] && (l = n[y.key][m.key])) : i.linkage === "max" ? (l = n[h.key][m.key], n[h.key][m.key] < n[y.key][m.key] && (l = n[y.key][m.key])) : i.linkage === "mean" ? l = (n[h.key][m.key] * h.size + n[y.key][m.key] * y.size) / (h.size + y.size) : i.mode === "dendrogram" ? l = f(m.value, h.value) : l = f(m.value[0], h.value[0]), n[h.key][m.key] = n[m.key][h.key] = l;
  }
  for (var b = 0; b < e.length; b++) {
    var w = e[b].key;
    if (a[w] === h.key || a[w] === y.key) {
      for (var E = w, T = 0; T < e.length; T++) {
        var x = e[T].key;
        n[w][x] < n[w][E] && (E = x);
      }
      a[w] = E;
    }
    e[b].index = b;
  }
  return h.key = y.key = h.index = y.index = null, !0;
}, Qn = function(e, r, n) {
  e && (e.value ? r.push(e.value) : (e.left && Qn(e.left, r), e.right && Qn(e.right, r)));
}, du = function(e, r) {
  if (!e) return "";
  if (e.left && e.right) {
    var n = du(e.left, r), a = du(e.right, r), i = r.add({
      group: "nodes",
      data: {
        id: n + "," + a
      }
    });
    return r.add({
      group: "edges",
      data: {
        source: n,
        target: i.id()
      }
    }), r.add({
      group: "edges",
      data: {
        source: a,
        target: i.id()
      }
    }), i.id();
  } else if (e.value)
    return e.value.id();
}, hu = function(e, r, n) {
  if (!e) return [];
  var a = [], i = [], s = [];
  return r === 0 ? (e.left && Qn(e.left, a), e.right && Qn(e.right, i), s = a.concat(i), [n.collection(s)]) : r === 1 ? e.value ? [n.collection(e.value)] : (e.left && Qn(e.left, a), e.right && Qn(e.right, i), [n.collection(a), n.collection(i)]) : e.value ? [n.collection(e.value)] : (e.left && (a = hu(e.left, r - 1, n)), e.right && (i = hu(e.right, r - 1, n)), a.concat(i));
}, Lc = function(e) {
  for (var r = this.cy(), n = this.nodes(), a = L1(e), i = a.attributes, s = function(b, w) {
    return Ys(a.distance, i.length, function(E) {
      return i[E](b);
    }, function(E) {
      return i[E](w);
    }, b, w);
  }, o = [], l = [], u = [], f = [], c = 0; c < n.length; c++) {
    var v = {
      value: a.mode === "dendrogram" ? n[c] : [n[c]],
      key: c,
      index: c
    };
    o[c] = v, f[c] = v, l[c] = [], u[c] = 0;
  }
  for (var d = 0; d < o.length; d++)
    for (var h = 0; h <= d; h++) {
      var y = void 0;
      a.mode === "dendrogram" ? y = d === h ? 1 / 0 : s(o[d].value, o[h].value) : y = d === h ? 1 / 0 : s(o[d].value[0], o[h].value[0]), l[d][h] = y, l[h][d] = y, y < l[d][u[d]] && (u[d] = h);
    }
  for (var g = Mc(o, f, l, u, a); g; )
    g = Mc(o, f, l, u, a);
  var p;
  return a.mode === "dendrogram" ? (p = hu(o[0], a.dendrogramDepth, r), a.addDendrogram && du(o[0], r)) : (p = new Array(o.length), o.forEach(function(m, b) {
    m.key = m.index = null, p[b] = r.collection(m.value);
  })), p;
}, I1 = {
  hierarchicalClustering: Lc,
  hca: Lc
}, O1 = St({
  distance: "euclidean",
  // distance metric to compare attributes between two nodes
  preference: "median",
  // suitability of a data point to serve as an exemplar
  damping: 0.8,
  // damping factor between [0.5, 1)
  maxIterations: 1e3,
  // max number of iterations to run
  minIterations: 100,
  // min number of iterations to run in order for clustering to stop
  attributes: [
    // functions to quantify the similarity between any two points
    // e.g. node => node.data('weight')
  ]
}), N1 = function(e) {
  var r = e.damping, n = e.preference;
  0.5 <= r && r < 1 || je("Damping must range on [0.5, 1).  Got: ".concat(r));
  var a = ["median", "mean", "min", "max"];
  return a.some(function(i) {
    return i === n;
  }) || pe(n) || je("Preference must be one of [".concat(a.map(function(i) {
    return "'".concat(i, "'");
  }).join(", "), "] or a number.  Got: ").concat(n)), O1(e);
}, _1 = function(e, r, n, a) {
  var i = function(o, l) {
    return a[l](o);
  };
  return -Ys(e, a.length, function(s) {
    return i(r, s);
  }, function(s) {
    return i(n, s);
  }, r, n);
}, F1 = function(e, r) {
  var n = null;
  return r === "median" ? n = Nb(e) : r === "mean" ? n = Ob(e) : r === "min" ? n = Lb(e) : r === "max" ? n = Ib(e) : n = r, n;
}, z1 = function(e, r, n) {
  for (var a = [], i = 0; i < e; i++)
    r[i * e + i] + n[i * e + i] > 0 && a.push(i);
  return a;
}, Ic = function(e, r, n) {
  for (var a = [], i = 0; i < e; i++) {
    for (var s = -1, o = -1 / 0, l = 0; l < n.length; l++) {
      var u = n[l];
      r[i * e + u] > o && (s = u, o = r[i * e + u]);
    }
    s > 0 && a.push(s);
  }
  for (var f = 0; f < n.length; f++)
    a[n[f]] = n[f];
  return a;
}, V1 = function(e, r, n) {
  for (var a = Ic(e, r, n), i = 0; i < n.length; i++) {
    for (var s = [], o = 0; o < a.length; o++)
      a[o] === n[i] && s.push(o);
    for (var l = -1, u = -1 / 0, f = 0; f < s.length; f++) {
      for (var c = 0, v = 0; v < s.length; v++)
        c += r[s[v] * e + s[f]];
      c > u && (l = f, u = c);
    }
    n[i] = s[l];
  }
  return a = Ic(e, r, n), a;
}, Oc = function(e) {
  for (var r = this.cy(), n = this.nodes(), a = N1(e), i = {}, s = 0; s < n.length; s++)
    i[n[s].id()] = s;
  var o, l, u, f, c, v;
  o = n.length, l = o * o, u = new Array(l);
  for (var d = 0; d < l; d++)
    u[d] = -1 / 0;
  for (var h = 0; h < o; h++)
    for (var y = 0; y < o; y++)
      h !== y && (u[h * o + y] = _1(a.distance, n[h], n[y], a.attributes));
  f = F1(u, a.preference);
  for (var g = 0; g < o; g++)
    u[g * o + g] = f;
  c = new Array(l);
  for (var p = 0; p < l; p++)
    c[p] = 0;
  v = new Array(l);
  for (var m = 0; m < l; m++)
    v[m] = 0;
  for (var b = new Array(o), w = new Array(o), E = new Array(o), T = 0; T < o; T++)
    b[T] = 0, w[T] = 0, E[T] = 0;
  for (var x = new Array(o * a.minIterations), S = 0; S < x.length; S++)
    x[S] = 0;
  var D;
  for (D = 0; D < a.maxIterations; D++) {
    for (var A = 0; A < o; A++) {
      for (var k = -1 / 0, R = -1 / 0, M = -1, I = 0, _ = 0; _ < o; _++)
        b[_] = c[A * o + _], I = v[A * o + _] + u[A * o + _], I >= k ? (R = k, k = I, M = _) : I > R && (R = I);
      for (var N = 0; N < o; N++)
        c[A * o + N] = (1 - a.damping) * (u[A * o + N] - k) + a.damping * b[N];
      c[A * o + M] = (1 - a.damping) * (u[A * o + M] - R) + a.damping * b[M];
    }
    for (var L = 0; L < o; L++) {
      for (var F = 0, H = 0; H < o; H++)
        b[H] = v[H * o + L], w[H] = Math.max(0, c[H * o + L]), F += w[H];
      F -= w[L], w[L] = c[L * o + L], F += w[L];
      for (var V = 0; V < o; V++)
        v[V * o + L] = (1 - a.damping) * Math.min(0, F - w[V]) + a.damping * b[V];
      v[L * o + L] = (1 - a.damping) * (F - w[L]) + a.damping * b[L];
    }
    for (var O = 0, $ = 0; $ < o; $++) {
      var Q = v[$ * o + $] + c[$ * o + $] > 0 ? 1 : 0;
      x[D % a.minIterations * o + $] = Q, O += Q;
    }
    if (O > 0 && (D >= a.minIterations - 1 || D == a.maxIterations - 1)) {
      for (var se = 0, ae = 0; ae < o; ae++) {
        E[ae] = 0;
        for (var le = 0; le < a.minIterations; le++)
          E[ae] += x[le * o + ae];
        (E[ae] === 0 || E[ae] === a.minIterations) && se++;
      }
      if (se === o)
        break;
    }
  }
  for (var ce = z1(o, c, v), he = V1(o, u, ce), ie = {}, U = 0; U < ce.length; U++)
    ie[ce[U]] = [];
  for (var X = 0; X < n.length; X++) {
    var C = i[n[X].id()], B = he[C];
    B != null && ie[B].push(n[X]);
  }
  for (var z = new Array(ce.length), W = 0; W < ce.length; W++)
    z[W] = r.collection(ie[ce[W]]);
  return z;
}, q1 = {
  affinityPropagation: Oc,
  ap: Oc
}, $1 = St({
  root: void 0,
  directed: !1
}), H1 = {
  hierholzer: function(e) {
    if (!_e(e)) {
      var r = arguments;
      e = {
        root: r[0],
        directed: r[1]
      };
    }
    var n = $1(e), a = n.root, i = n.directed, s = this, o = !1, l, u, f;
    a && (f = Se(a) ? this.filter(a)[0].id() : a[0].id());
    var c = {}, v = {};
    i ? s.forEach(function(m) {
      var b = m.id();
      if (m.isNode()) {
        var w = m.indegree(!0), E = m.outdegree(!0), T = w - E, x = E - w;
        T == 1 ? l ? o = !0 : l = b : x == 1 ? u ? o = !0 : u = b : (x > 1 || T > 1) && (o = !0), c[b] = [], m.outgoers().forEach(function(S) {
          S.isEdge() && c[b].push(S.id());
        });
      } else
        v[b] = [void 0, m.target().id()];
    }) : s.forEach(function(m) {
      var b = m.id();
      if (m.isNode()) {
        var w = m.degree(!0);
        w % 2 && (l ? u ? o = !0 : u = b : l = b), c[b] = [], m.connectedEdges().forEach(function(E) {
          return c[b].push(E.id());
        });
      } else
        v[b] = [m.source().id(), m.target().id()];
    });
    var d = {
      found: !1,
      trail: void 0
    };
    if (o) return d;
    if (u && l)
      if (i) {
        if (f && u != f)
          return d;
        f = u;
      } else {
        if (f && u != f && l != f)
          return d;
        f || (f = u);
      }
    else
      f || (f = s[0].id());
    var h = function(b) {
      for (var w = b, E = [b], T, x, S; c[w].length; )
        T = c[w].shift(), x = v[T][0], S = v[T][1], w != S ? (c[S] = c[S].filter(function(D) {
          return D != T;
        }), w = S) : !i && w != x && (c[x] = c[x].filter(function(D) {
          return D != T;
        }), w = x), E.unshift(T), E.unshift(w);
      return E;
    }, y = [], g = [];
    for (g = h(f); g.length != 1; )
      c[g[0]].length == 0 ? (y.unshift(s.getElementById(g.shift())), y.unshift(s.getElementById(g.shift()))) : g = h(g.shift()).concat(g);
    y.unshift(s.getElementById(g.shift()));
    for (var p in c)
      if (c[p].length)
        return d;
    return d.found = !0, d.trail = this.spawn(y, !0), d;
  }
}, _i = function() {
  var e = this, r = {}, n = 0, a = 0, i = [], s = [], o = {}, l = function(v, d) {
    for (var h = s.length - 1, y = [], g = e.spawn(); s[h].x != v || s[h].y != d; )
      y.push(s.pop().edge), h--;
    y.push(s.pop().edge), y.forEach(function(p) {
      var m = p.connectedNodes().intersection(e);
      g.merge(p), m.forEach(function(b) {
        var w = b.id(), E = b.connectedEdges().intersection(e);
        g.merge(b), r[w].cutVertex ? g.merge(E.filter(function(T) {
          return T.isLoop();
        })) : g.merge(E);
      });
    }), i.push(g);
  }, u = function(v, d, h) {
    v === h && (a += 1), r[d] = {
      id: n,
      low: n++,
      cutVertex: !1
    };
    var y = e.getElementById(d).connectedEdges().intersection(e);
    if (y.size() === 0)
      i.push(e.spawn(e.getElementById(d)));
    else {
      var g, p, m, b;
      y.forEach(function(w) {
        g = w.source().id(), p = w.target().id(), m = g === d ? p : g, m !== h && (b = w.id(), o[b] || (o[b] = !0, s.push({
          x: d,
          y: m,
          edge: w
        })), m in r ? r[d].low = Math.min(r[d].low, r[m].id) : (u(v, m, d), r[d].low = Math.min(r[d].low, r[m].low), r[d].id <= r[m].low && (r[d].cutVertex = !0, l(d, m))));
      });
    }
  };
  e.forEach(function(c) {
    if (c.isNode()) {
      var v = c.id();
      v in r || (a = 0, u(v, v), r[v].cutVertex = a > 1);
    }
  });
  var f = Object.keys(r).filter(function(c) {
    return r[c].cutVertex;
  }).map(function(c) {
    return e.getElementById(c);
  });
  return {
    cut: e.spawn(f),
    components: i
  };
}, U1 = {
  hopcroftTarjanBiconnected: _i,
  htbc: _i,
  htb: _i,
  hopcroftTarjanBiconnectedComponents: _i
}, Fi = function() {
  var e = this, r = {}, n = 0, a = [], i = [], s = e.spawn(e), o = function(u) {
    i.push(u), r[u] = {
      index: n,
      low: n++,
      explored: !1
    };
    var f = e.getElementById(u).connectedEdges().intersection(e);
    if (f.forEach(function(y) {
      var g = y.target().id();
      g !== u && (g in r || o(g), r[g].explored || (r[u].low = Math.min(r[u].low, r[g].low)));
    }), r[u].index === r[u].low) {
      for (var c = e.spawn(); ; ) {
        var v = i.pop();
        if (c.merge(e.getElementById(v)), r[v].low = r[u].index, r[v].explored = !0, v === u)
          break;
      }
      var d = c.edgesWith(c), h = c.merge(d);
      a.push(h), s = s.difference(h);
    }
  };
  return e.forEach(function(l) {
    if (l.isNode()) {
      var u = l.id();
      u in r || o(u);
    }
  }), {
    cut: s,
    components: a
  };
}, G1 = {
  tarjanStronglyConnected: Fi,
  tsc: Fi,
  tscc: Fi,
  tarjanStronglyConnectedComponents: Fi
}, vg = {};
[ni, xb, Eb, Tb, Pb, Ab, Rb, a1, ia, sa, vu, y1, B1, I1, q1, H1, U1, G1].forEach(function(t) {
  Ae(vg, t);
});
/*!
Embeddable Minimum Strictly-Compliant Promises/A+ 1.1.1 Thenable
Copyright (c) 2013-2014 Ralf S. Engelschall (http://engelschall.com)
Licensed under The MIT License (http://opensource.org/licenses/MIT)
*/
var dg = 0, hg = 1, gg = 2, fr = function(e) {
  if (!(this instanceof fr)) return new fr(e);
  this.id = "Thenable/1.0.7", this.state = dg, this.fulfillValue = void 0, this.rejectReason = void 0, this.onFulfilled = [], this.onRejected = [], this.proxy = {
    then: this.then.bind(this)
  }, typeof e == "function" && e.call(this, this.fulfill.bind(this), this.reject.bind(this));
};
fr.prototype = {
  /*  promise resolving methods  */
  fulfill: function(e) {
    return Nc(this, hg, "fulfillValue", e);
  },
  reject: function(e) {
    return Nc(this, gg, "rejectReason", e);
  },
  /*  "The then Method" [Promises/A+ 1.1, 1.2, 2.2]  */
  then: function(e, r) {
    var n = this, a = new fr();
    return n.onFulfilled.push(Fc(e, a, "fulfill")), n.onRejected.push(Fc(r, a, "reject")), pg(n), a.proxy;
  }
};
var Nc = function(e, r, n, a) {
  return e.state === dg && (e.state = r, e[n] = a, pg(e)), e;
}, pg = function(e) {
  e.state === hg ? _c(e, "onFulfilled", e.fulfillValue) : e.state === gg && _c(e, "onRejected", e.rejectReason);
}, _c = function(e, r, n) {
  if (e[r].length !== 0) {
    var a = e[r];
    e[r] = [];
    var i = function() {
      for (var o = 0; o < a.length; o++) a[o](n);
    };
    typeof setImmediate == "function" ? setImmediate(i) : setTimeout(i, 0);
  }
}, Fc = function(e, r, n) {
  return function(a) {
    if (typeof e != "function")
      r[n].call(r, a);
    else {
      var i;
      try {
        i = e(a);
      } catch (s) {
        r.reject(s);
        return;
      }
      yg(r, i);
    }
  };
}, yg = function(e, r) {
  if (e === r || e.proxy === r) {
    e.reject(new TypeError("cannot resolve promise with itself"));
    return;
  }
  var n;
  if (dt(r) === "object" && r !== null || typeof r == "function")
    try {
      n = r.then;
    } catch (i) {
      e.reject(i);
      return;
    }
  if (typeof n == "function") {
    var a = !1;
    try {
      n.call(
        r,
        /*  resolvePromise  */
        /*  [Promises/A+ 2.3.3.3.1]  */
        function(i) {
          a || (a = !0, i === r ? e.reject(new TypeError("circular thenable chain")) : yg(e, i));
        },
        /*  rejectPromise  */
        /*  [Promises/A+ 2.3.3.3.2]  */
        function(i) {
          a || (a = !0, e.reject(i));
        }
      );
    } catch (i) {
      a || e.reject(i);
    }
    return;
  }
  e.fulfill(r);
};
fr.all = function(t) {
  return new fr(function(e, r) {
    for (var n = new Array(t.length), a = 0, i = function(l, u) {
      n[l] = u, a++, a === t.length && e(n);
    }, s = 0; s < t.length; s++)
      (function(o) {
        var l = t[o], u = l != null && l.then != null;
        if (u)
          l.then(function(c) {
            i(o, c);
          }, function(c) {
            r(c);
          });
        else {
          var f = l;
          i(o, f);
        }
      })(s);
  });
};
fr.resolve = function(t) {
  return new fr(function(e, r) {
    e(t);
  });
};
fr.reject = function(t) {
  return new fr(function(e, r) {
    r(t);
  });
};
var ya = typeof Promise < "u" ? Promise : fr, gu = function(e, r, n) {
  var a = Wu(e), i = !a, s = this._private = Ae({
    duration: 1e3
  }, r, n);
  if (s.target = e, s.style = s.style || s.css, s.started = !1, s.playing = !1, s.hooked = !1, s.applying = !1, s.progress = 0, s.completes = [], s.frames = [], s.complete && tt(s.complete) && s.completes.push(s.complete), i) {
    var o = e.position();
    s.startPosition = s.startPosition || {
      x: o.x,
      y: o.y
    }, s.startStyle = s.startStyle || e.cy().style().getAnimationStartStyle(e, s.style);
  }
  if (a) {
    var l = e.pan();
    s.startPan = {
      x: l.x,
      y: l.y
    }, s.startZoom = e.zoom();
  }
  this.length = 1, this[0] = this;
}, Mn = gu.prototype;
Ae(Mn, {
  instanceString: function() {
    return "animation";
  },
  hook: function() {
    var e = this._private;
    if (!e.hooked) {
      var r, n = e.target._private.animation;
      e.queue ? r = n.queue : r = n.current, r.push(this), Xt(e.target) && e.target.cy().addToAnimationPool(e.target), e.hooked = !0;
    }
    return this;
  },
  play: function() {
    var e = this._private;
    return e.progress === 1 && (e.progress = 0), e.playing = !0, e.started = !1, e.stopped = !1, this.hook(), this;
  },
  playing: function() {
    return this._private.playing;
  },
  apply: function() {
    var e = this._private;
    return e.applying = !0, e.started = !1, e.stopped = !1, this.hook(), this;
  },
  applying: function() {
    return this._private.applying;
  },
  pause: function() {
    var e = this._private;
    return e.playing = !1, e.started = !1, this;
  },
  stop: function() {
    var e = this._private;
    return e.playing = !1, e.started = !1, e.stopped = !0, this;
  },
  rewind: function() {
    return this.progress(0);
  },
  fastforward: function() {
    return this.progress(1);
  },
  time: function(e) {
    var r = this._private;
    return e === void 0 ? r.progress * r.duration : this.progress(e / r.duration);
  },
  progress: function(e) {
    var r = this._private, n = r.playing;
    return e === void 0 ? r.progress : (n && this.pause(), r.progress = e, r.started = !1, n && this.play(), this);
  },
  completed: function() {
    return this._private.progress === 1;
  },
  reverse: function() {
    var e = this._private, r = e.playing;
    r && this.pause(), e.progress = 1 - e.progress, e.started = !1;
    var n = function(u, f) {
      var c = e[u];
      c != null && (e[u] = e[f], e[f] = c);
    };
    if (n("zoom", "startZoom"), n("pan", "startPan"), n("position", "startPosition"), e.style)
      for (var a = 0; a < e.style.length; a++) {
        var i = e.style[a], s = i.name, o = e.startStyle[s];
        e.startStyle[s] = i, e.style[a] = o;
      }
    return r && this.play(), this;
  },
  promise: function(e) {
    var r = this._private, n;
    switch (e) {
      case "frame":
        n = r.frames;
        break;
      default:
      case "complete":
      case "completed":
        n = r.completes;
    }
    return new ya(function(a, i) {
      n.push(function() {
        a();
      });
    });
  }
});
Mn.complete = Mn.completed;
Mn.run = Mn.play;
Mn.running = Mn.playing;
var W1 = {
  animated: function() {
    return function() {
      var r = this, n = r.length !== void 0, a = n ? r : [r], i = this._private.cy || this;
      if (!i.styleEnabled())
        return !1;
      var s = a[0];
      if (s)
        return s._private.animation.current.length > 0;
    };
  },
  // animated
  clearQueue: function() {
    return function() {
      var r = this, n = r.length !== void 0, a = n ? r : [r], i = this._private.cy || this;
      if (!i.styleEnabled())
        return this;
      for (var s = 0; s < a.length; s++) {
        var o = a[s];
        o._private.animation.queue = [];
      }
      return this;
    };
  },
  // clearQueue
  delay: function() {
    return function(r, n) {
      var a = this._private.cy || this;
      return a.styleEnabled() ? this.animate({
        delay: r,
        duration: r,
        complete: n
      }) : this;
    };
  },
  // delay
  delayAnimation: function() {
    return function(r, n) {
      var a = this._private.cy || this;
      return a.styleEnabled() ? this.animation({
        delay: r,
        duration: r,
        complete: n
      }) : this;
    };
  },
  // delay
  animation: function() {
    return function(r, n) {
      var a = this, i = a.length !== void 0, s = i ? a : [a], o = this._private.cy || this, l = !i, u = !l;
      if (!o.styleEnabled())
        return this;
      var f = o.style();
      r = Ae({}, r, n);
      var c = Object.keys(r).length === 0;
      if (c)
        return new gu(s[0], r);
      switch (r.duration === void 0 && (r.duration = 400), r.duration) {
        case "slow":
          r.duration = 600;
          break;
        case "fast":
          r.duration = 200;
          break;
      }
      if (u && (r.style = f.getPropsList(r.style || r.css), r.css = void 0), u && r.renderedPosition != null) {
        var v = r.renderedPosition, d = o.pan(), h = o.zoom();
        r.position = rg(v, h, d);
      }
      if (l && r.panBy != null) {
        var y = r.panBy, g = o.pan();
        r.pan = {
          x: g.x + y.x,
          y: g.y + y.y
        };
      }
      var p = r.center || r.centre;
      if (l && p != null) {
        var m = o.getCenterPan(p.eles, r.zoom);
        m != null && (r.pan = m);
      }
      if (l && r.fit != null) {
        var b = r.fit, w = o.getFitViewport(b.eles || b.boundingBox, b.padding);
        w != null && (r.pan = w.pan, r.zoom = w.zoom);
      }
      if (l && _e(r.zoom)) {
        var E = o.getZoomedViewport(r.zoom);
        E != null ? (E.zoomed && (r.zoom = E.zoom), E.panned && (r.pan = E.pan)) : r.zoom = null;
      }
      return new gu(s[0], r);
    };
  },
  // animate
  animate: function() {
    return function(r, n) {
      var a = this, i = a.length !== void 0, s = i ? a : [a], o = this._private.cy || this;
      if (!o.styleEnabled())
        return this;
      n && (r = Ae({}, r, n));
      for (var l = 0; l < s.length; l++) {
        var u = s[l], f = u.animated() && (r.queue === void 0 || r.queue), c = u.animation(r, f ? {
          queue: !0
        } : void 0);
        c.play();
      }
      return this;
    };
  },
  // animate
  stop: function() {
    return function(r, n) {
      var a = this, i = a.length !== void 0, s = i ? a : [a], o = this._private.cy || this;
      if (!o.styleEnabled())
        return this;
      for (var l = 0; l < s.length; l++) {
        for (var u = s[l], f = u._private, c = f.animation.current, v = 0; v < c.length; v++) {
          var d = c[v], h = d._private;
          n && (h.duration = 0);
        }
        r && (f.animation.queue = []), n || (f.animation.current = []);
      }
      return o.notify("draw"), this;
    };
  }
  // stop
}, Ho, zc;
function Xs() {
  if (zc) return Ho;
  zc = 1;
  var t = Array.isArray;
  return Ho = t, Ho;
}
var Uo, Vc;
function K1() {
  if (Vc) return Uo;
  Vc = 1;
  var t = Xs(), e = bi(), r = /\.|\[(?:[^[\]]*|(["'])(?:(?!\1)[^\\]|\\.)*?\1)\]/, n = /^\w*$/;
  function a(i, s) {
    if (t(i))
      return !1;
    var o = typeof i;
    return o == "number" || o == "symbol" || o == "boolean" || i == null || e(i) ? !0 : n.test(i) || !r.test(i) || s != null && i in Object(s);
  }
  return Uo = a, Uo;
}
var Go, qc;
function Y1() {
  if (qc) return Go;
  qc = 1;
  var t = Kh(), e = mi(), r = "[object AsyncFunction]", n = "[object Function]", a = "[object GeneratorFunction]", i = "[object Proxy]";
  function s(o) {
    if (!e(o))
      return !1;
    var l = t(o);
    return l == n || l == a || l == r || l == i;
  }
  return Go = s, Go;
}
var Wo, $c;
function X1() {
  if ($c) return Wo;
  $c = 1;
  var t = Gs(), e = t["__core-js_shared__"];
  return Wo = e, Wo;
}
var Ko, Hc;
function Z1() {
  if (Hc) return Ko;
  Hc = 1;
  var t = X1(), e = (function() {
    var n = /[^.]+$/.exec(t && t.keys && t.keys.IE_PROTO || "");
    return n ? "Symbol(src)_1." + n : "";
  })();
  function r(n) {
    return !!e && e in n;
  }
  return Ko = r, Ko;
}
var Yo, Uc;
function Q1() {
  if (Uc) return Yo;
  Uc = 1;
  var t = Function.prototype, e = t.toString;
  function r(n) {
    if (n != null) {
      try {
        return e.call(n);
      } catch {
      }
      try {
        return n + "";
      } catch {
      }
    }
    return "";
  }
  return Yo = r, Yo;
}
var Xo, Gc;
function j1() {
  if (Gc) return Xo;
  Gc = 1;
  var t = Y1(), e = Z1(), r = mi(), n = Q1(), a = /[\\^$.*+?()[\]{}|]/g, i = /^\[object .+?Constructor\]$/, s = Function.prototype, o = Object.prototype, l = s.toString, u = o.hasOwnProperty, f = RegExp(
    "^" + l.call(u).replace(a, "\\$&").replace(/hasOwnProperty|(function).*?(?=\\\()| for .+?(?=\\\])/g, "$1.*?") + "$"
  );
  function c(v) {
    if (!r(v) || e(v))
      return !1;
    var d = t(v) ? f : i;
    return d.test(n(v));
  }
  return Xo = c, Xo;
}
var Zo, Wc;
function J1() {
  if (Wc) return Zo;
  Wc = 1;
  function t(e, r) {
    return e == null ? void 0 : e[r];
  }
  return Zo = t, Zo;
}
var Qo, Kc;
function nf() {
  if (Kc) return Qo;
  Kc = 1;
  var t = j1(), e = J1();
  function r(n, a) {
    var i = e(n, a);
    return t(i) ? i : void 0;
  }
  return Qo = r, Qo;
}
var jo, Yc;
function Zs() {
  if (Yc) return jo;
  Yc = 1;
  var t = nf(), e = t(Object, "create");
  return jo = e, jo;
}
var Jo, Xc;
function ew() {
  if (Xc) return Jo;
  Xc = 1;
  var t = Zs();
  function e() {
    this.__data__ = t ? t(null) : {}, this.size = 0;
  }
  return Jo = e, Jo;
}
var el, Zc;
function tw() {
  if (Zc) return el;
  Zc = 1;
  function t(e) {
    var r = this.has(e) && delete this.__data__[e];
    return this.size -= r ? 1 : 0, r;
  }
  return el = t, el;
}
var tl, Qc;
function rw() {
  if (Qc) return tl;
  Qc = 1;
  var t = Zs(), e = "__lodash_hash_undefined__", r = Object.prototype, n = r.hasOwnProperty;
  function a(i) {
    var s = this.__data__;
    if (t) {
      var o = s[i];
      return o === e ? void 0 : o;
    }
    return n.call(s, i) ? s[i] : void 0;
  }
  return tl = a, tl;
}
var rl, jc;
function nw() {
  if (jc) return rl;
  jc = 1;
  var t = Zs(), e = Object.prototype, r = e.hasOwnProperty;
  function n(a) {
    var i = this.__data__;
    return t ? i[a] !== void 0 : r.call(i, a);
  }
  return rl = n, rl;
}
var nl, Jc;
function aw() {
  if (Jc) return nl;
  Jc = 1;
  var t = Zs(), e = "__lodash_hash_undefined__";
  function r(n, a) {
    var i = this.__data__;
    return this.size += this.has(n) ? 0 : 1, i[n] = t && a === void 0 ? e : a, this;
  }
  return nl = r, nl;
}
var al, ev;
function iw() {
  if (ev) return al;
  ev = 1;
  var t = ew(), e = tw(), r = rw(), n = nw(), a = aw();
  function i(s) {
    var o = -1, l = s == null ? 0 : s.length;
    for (this.clear(); ++o < l; ) {
      var u = s[o];
      this.set(u[0], u[1]);
    }
  }
  return i.prototype.clear = t, i.prototype.delete = e, i.prototype.get = r, i.prototype.has = n, i.prototype.set = a, al = i, al;
}
var il, tv;
function sw() {
  if (tv) return il;
  tv = 1;
  function t() {
    this.__data__ = [], this.size = 0;
  }
  return il = t, il;
}
var sl, rv;
function mg() {
  if (rv) return sl;
  rv = 1;
  function t(e, r) {
    return e === r || e !== e && r !== r;
  }
  return sl = t, sl;
}
var ol, nv;
function Qs() {
  if (nv) return ol;
  nv = 1;
  var t = mg();
  function e(r, n) {
    for (var a = r.length; a--; )
      if (t(r[a][0], n))
        return a;
    return -1;
  }
  return ol = e, ol;
}
var ll, av;
function ow() {
  if (av) return ll;
  av = 1;
  var t = Qs(), e = Array.prototype, r = e.splice;
  function n(a) {
    var i = this.__data__, s = t(i, a);
    if (s < 0)
      return !1;
    var o = i.length - 1;
    return s == o ? i.pop() : r.call(i, s, 1), --this.size, !0;
  }
  return ll = n, ll;
}
var ul, iv;
function lw() {
  if (iv) return ul;
  iv = 1;
  var t = Qs();
  function e(r) {
    var n = this.__data__, a = t(n, r);
    return a < 0 ? void 0 : n[a][1];
  }
  return ul = e, ul;
}
var fl, sv;
function uw() {
  if (sv) return fl;
  sv = 1;
  var t = Qs();
  function e(r) {
    return t(this.__data__, r) > -1;
  }
  return fl = e, fl;
}
var cl, ov;
function fw() {
  if (ov) return cl;
  ov = 1;
  var t = Qs();
  function e(r, n) {
    var a = this.__data__, i = t(a, r);
    return i < 0 ? (++this.size, a.push([r, n])) : a[i][1] = n, this;
  }
  return cl = e, cl;
}
var vl, lv;
function cw() {
  if (lv) return vl;
  lv = 1;
  var t = sw(), e = ow(), r = lw(), n = uw(), a = fw();
  function i(s) {
    var o = -1, l = s == null ? 0 : s.length;
    for (this.clear(); ++o < l; ) {
      var u = s[o];
      this.set(u[0], u[1]);
    }
  }
  return i.prototype.clear = t, i.prototype.delete = e, i.prototype.get = r, i.prototype.has = n, i.prototype.set = a, vl = i, vl;
}
var dl, uv;
function vw() {
  if (uv) return dl;
  uv = 1;
  var t = nf(), e = Gs(), r = t(e, "Map");
  return dl = r, dl;
}
var hl, fv;
function dw() {
  if (fv) return hl;
  fv = 1;
  var t = iw(), e = cw(), r = vw();
  function n() {
    this.size = 0, this.__data__ = {
      hash: new t(),
      map: new (r || e)(),
      string: new t()
    };
  }
  return hl = n, hl;
}
var gl, cv;
function hw() {
  if (cv) return gl;
  cv = 1;
  function t(e) {
    var r = typeof e;
    return r == "string" || r == "number" || r == "symbol" || r == "boolean" ? e !== "__proto__" : e === null;
  }
  return gl = t, gl;
}
var pl, vv;
function js() {
  if (vv) return pl;
  vv = 1;
  var t = hw();
  function e(r, n) {
    var a = r.__data__;
    return t(n) ? a[typeof n == "string" ? "string" : "hash"] : a.map;
  }
  return pl = e, pl;
}
var yl, dv;
function gw() {
  if (dv) return yl;
  dv = 1;
  var t = js();
  function e(r) {
    var n = t(this, r).delete(r);
    return this.size -= n ? 1 : 0, n;
  }
  return yl = e, yl;
}
var ml, hv;
function pw() {
  if (hv) return ml;
  hv = 1;
  var t = js();
  function e(r) {
    return t(this, r).get(r);
  }
  return ml = e, ml;
}
var bl, gv;
function yw() {
  if (gv) return bl;
  gv = 1;
  var t = js();
  function e(r) {
    return t(this, r).has(r);
  }
  return bl = e, bl;
}
var wl, pv;
function mw() {
  if (pv) return wl;
  pv = 1;
  var t = js();
  function e(r, n) {
    var a = t(this, r), i = a.size;
    return a.set(r, n), this.size += a.size == i ? 0 : 1, this;
  }
  return wl = e, wl;
}
var xl, yv;
function bw() {
  if (yv) return xl;
  yv = 1;
  var t = dw(), e = gw(), r = pw(), n = yw(), a = mw();
  function i(s) {
    var o = -1, l = s == null ? 0 : s.length;
    for (this.clear(); ++o < l; ) {
      var u = s[o];
      this.set(u[0], u[1]);
    }
  }
  return i.prototype.clear = t, i.prototype.delete = e, i.prototype.get = r, i.prototype.has = n, i.prototype.set = a, xl = i, xl;
}
var El, mv;
function ww() {
  if (mv) return El;
  mv = 1;
  var t = bw(), e = "Expected a function";
  function r(n, a) {
    if (typeof n != "function" || a != null && typeof a != "function")
      throw new TypeError(e);
    var i = function() {
      var s = arguments, o = a ? a.apply(this, s) : s[0], l = i.cache;
      if (l.has(o))
        return l.get(o);
      var u = n.apply(this, s);
      return i.cache = l.set(o, u) || l, u;
    };
    return i.cache = new (r.Cache || t)(), i;
  }
  return r.Cache = t, El = r, El;
}
var Cl, bv;
function xw() {
  if (bv) return Cl;
  bv = 1;
  var t = ww(), e = 500;
  function r(n) {
    var a = t(n, function(s) {
      return i.size === e && i.clear(), s;
    }), i = a.cache;
    return a;
  }
  return Cl = r, Cl;
}
var Tl, wv;
function bg() {
  if (wv) return Tl;
  wv = 1;
  var t = xw(), e = /[^.[\]]+|\[(?:(-?\d+(?:\.\d+)?)|(["'])((?:(?!\2)[^\\]|\\.)*?)\2)\]|(?=(?:\.|\[\])(?:\.|\[\]|$))/g, r = /\\(\\)?/g, n = t(function(a) {
    var i = [];
    return a.charCodeAt(0) === 46 && i.push(""), a.replace(e, function(s, o, l, u) {
      i.push(l ? u.replace(r, "$1") : o || s);
    }), i;
  });
  return Tl = n, Tl;
}
var Sl, xv;
function wg() {
  if (xv) return Sl;
  xv = 1;
  function t(e, r) {
    for (var n = -1, a = e == null ? 0 : e.length, i = Array(a); ++n < a; )
      i[n] = r(e[n], n, e);
    return i;
  }
  return Sl = t, Sl;
}
var Pl, Ev;
function Ew() {
  if (Ev) return Pl;
  Ev = 1;
  var t = Yu(), e = wg(), r = Xs(), n = bi(), a = t ? t.prototype : void 0, i = a ? a.toString : void 0;
  function s(o) {
    if (typeof o == "string")
      return o;
    if (r(o))
      return e(o, s) + "";
    if (n(o))
      return i ? i.call(o) : "";
    var l = o + "";
    return l == "0" && 1 / o == -1 / 0 ? "-0" : l;
  }
  return Pl = s, Pl;
}
var Dl, Cv;
function xg() {
  if (Cv) return Dl;
  Cv = 1;
  var t = Ew();
  function e(r) {
    return r == null ? "" : t(r);
  }
  return Dl = e, Dl;
}
var Al, Tv;
function Eg() {
  if (Tv) return Al;
  Tv = 1;
  var t = Xs(), e = K1(), r = bg(), n = xg();
  function a(i, s) {
    return t(i) ? i : e(i, s) ? [i] : r(n(i));
  }
  return Al = a, Al;
}
var kl, Sv;
function af() {
  if (Sv) return kl;
  Sv = 1;
  var t = bi();
  function e(r) {
    if (typeof r == "string" || t(r))
      return r;
    var n = r + "";
    return n == "0" && 1 / r == -1 / 0 ? "-0" : n;
  }
  return kl = e, kl;
}
var Bl, Pv;
function Cw() {
  if (Pv) return Bl;
  Pv = 1;
  var t = Eg(), e = af();
  function r(n, a) {
    a = t(a, n);
    for (var i = 0, s = a.length; n != null && i < s; )
      n = n[e(a[i++])];
    return i && i == s ? n : void 0;
  }
  return Bl = r, Bl;
}
var Rl, Dv;
function Tw() {
  if (Dv) return Rl;
  Dv = 1;
  var t = Cw();
  function e(r, n, a) {
    var i = r == null ? void 0 : t(r, n);
    return i === void 0 ? a : i;
  }
  return Rl = e, Rl;
}
var Sw = Tw(), Pw = /* @__PURE__ */ yi(Sw), Ml, Av;
function Dw() {
  if (Av) return Ml;
  Av = 1;
  var t = nf(), e = (function() {
    try {
      var r = t(Object, "defineProperty");
      return r({}, "", {}), r;
    } catch {
    }
  })();
  return Ml = e, Ml;
}
var Ll, kv;
function Aw() {
  if (kv) return Ll;
  kv = 1;
  var t = Dw();
  function e(r, n, a) {
    n == "__proto__" && t ? t(r, n, {
      configurable: !0,
      enumerable: !0,
      value: a,
      writable: !0
    }) : r[n] = a;
  }
  return Ll = e, Ll;
}
var Il, Bv;
function kw() {
  if (Bv) return Il;
  Bv = 1;
  var t = Aw(), e = mg(), r = Object.prototype, n = r.hasOwnProperty;
  function a(i, s, o) {
    var l = i[s];
    (!(n.call(i, s) && e(l, o)) || o === void 0 && !(s in i)) && t(i, s, o);
  }
  return Il = a, Il;
}
var Ol, Rv;
function Bw() {
  if (Rv) return Ol;
  Rv = 1;
  var t = 9007199254740991, e = /^(?:0|[1-9]\d*)$/;
  function r(n, a) {
    var i = typeof n;
    return a = a ?? t, !!a && (i == "number" || i != "symbol" && e.test(n)) && n > -1 && n % 1 == 0 && n < a;
  }
  return Ol = r, Ol;
}
var Nl, Mv;
function Rw() {
  if (Mv) return Nl;
  Mv = 1;
  var t = kw(), e = Eg(), r = Bw(), n = mi(), a = af();
  function i(s, o, l, u) {
    if (!n(s))
      return s;
    o = e(o, s);
    for (var f = -1, c = o.length, v = c - 1, d = s; d != null && ++f < c; ) {
      var h = a(o[f]), y = l;
      if (h === "__proto__" || h === "constructor" || h === "prototype")
        return s;
      if (f != v) {
        var g = d[h];
        y = u ? u(g, h, d) : void 0, y === void 0 && (y = n(g) ? g : r(o[f + 1]) ? [] : {});
      }
      t(d, h, y), d = d[h];
    }
    return s;
  }
  return Nl = i, Nl;
}
var _l, Lv;
function Mw() {
  if (Lv) return _l;
  Lv = 1;
  var t = Rw();
  function e(r, n, a) {
    return r == null ? r : t(r, n, a);
  }
  return _l = e, _l;
}
var Lw = Mw(), Iw = /* @__PURE__ */ yi(Lw), Fl, Iv;
function Ow() {
  if (Iv) return Fl;
  Iv = 1;
  function t(e, r) {
    var n = -1, a = e.length;
    for (r || (r = Array(a)); ++n < a; )
      r[n] = e[n];
    return r;
  }
  return Fl = t, Fl;
}
var zl, Ov;
function Nw() {
  if (Ov) return zl;
  Ov = 1;
  var t = wg(), e = Ow(), r = Xs(), n = bi(), a = bg(), i = af(), s = xg();
  function o(l) {
    return r(l) ? t(l, i) : n(l) ? [l] : e(a(s(l)));
  }
  return zl = o, zl;
}
var _w = Nw(), Fw = /* @__PURE__ */ yi(_w), zw = {
  // access data field
  data: function(e) {
    var r = {
      field: "data",
      bindingEvent: "data",
      allowBinding: !1,
      allowSetting: !1,
      allowGetting: !1,
      settingEvent: "data",
      settingTriggersEvent: !1,
      triggerFnName: "trigger",
      immutableKeys: {},
      // key => true if immutable
      updateStyle: !1,
      beforeGet: function(a) {
      },
      beforeSet: function(a, i) {
      },
      onSet: function(a) {
      },
      canSet: function(a) {
        return !0;
      }
    };
    return e = Ae({}, r, e), function(a, i) {
      var s = e, o = this, l = o.length !== void 0, u = l ? o : [o], f = l ? o[0] : o;
      if (Se(a)) {
        var c = a.indexOf(".") !== -1, v = c && Fw(a);
        if (s.allowGetting && i === void 0) {
          var d;
          return f && (s.beforeGet(f), v && f._private[s.field][a] === void 0 ? d = Pw(f._private[s.field], v) : d = f._private[s.field][a]), d;
        } else if (s.allowSetting && i !== void 0) {
          var h = !s.immutableKeys[a];
          if (h) {
            var y = _h({}, a, i);
            s.beforeSet(o, y);
            for (var g = 0, p = u.length; g < p; g++) {
              var m = u[g];
              s.canSet(m) && (v && f._private[s.field][a] === void 0 ? Iw(m._private[s.field], v, i) : m._private[s.field][a] = i);
            }
            s.updateStyle && o.updateStyle(), s.onSet(o), s.settingTriggersEvent && o[s.triggerFnName](s.settingEvent);
          }
        }
      } else if (s.allowSetting && _e(a)) {
        var b = a, w, E, T = Object.keys(b);
        s.beforeSet(o, b);
        for (var x = 0; x < T.length; x++) {
          w = T[x], E = b[w];
          var S = !s.immutableKeys[w];
          if (S)
            for (var D = 0; D < u.length; D++) {
              var A = u[D];
              s.canSet(A) && (A._private[s.field][w] = E);
            }
        }
        s.updateStyle && o.updateStyle(), s.onSet(o), s.settingTriggersEvent && o[s.triggerFnName](s.settingEvent);
      } else if (s.allowBinding && tt(a)) {
        var k = a;
        o.on(s.bindingEvent, k);
      } else if (s.allowGetting && a === void 0) {
        var R;
        return f && (s.beforeGet(f), R = f._private[s.field]), R;
      }
      return o;
    };
  },
  // data
  // remove data field
  removeData: function(e) {
    var r = {
      field: "data",
      event: "data",
      triggerFnName: "trigger",
      triggerEvent: !1,
      immutableKeys: {}
      // key => true if immutable
    };
    return e = Ae({}, r, e), function(a) {
      var i = e, s = this, o = s.length !== void 0, l = o ? s : [s];
      if (Se(a)) {
        for (var u = a.split(/\s+/), f = u.length, c = 0; c < f; c++) {
          var v = u[c];
          if (!nn(v)) {
            var d = !i.immutableKeys[v];
            if (d)
              for (var h = 0, y = l.length; h < y; h++)
                l[h]._private[i.field][v] = void 0;
          }
        }
        i.triggerEvent && s[i.triggerFnName](i.event);
      } else if (a === void 0) {
        for (var g = 0, p = l.length; g < p; g++)
          for (var m = l[g]._private[i.field], b = Object.keys(m), w = 0; w < b.length; w++) {
            var E = b[w], T = !i.immutableKeys[E];
            T && (m[E] = void 0);
          }
        i.triggerEvent && s[i.triggerFnName](i.event);
      }
      return s;
    };
  }
  // removeData
}, Vw = {
  eventAliasesOn: function(e) {
    var r = e;
    r.addListener = r.listen = r.bind = r.on, r.unlisten = r.unbind = r.off = r.removeListener, r.trigger = r.emit, r.pon = r.promiseOn = function(n, a) {
      var i = this, s = Array.prototype.slice.call(arguments, 0);
      return new ya(function(o, l) {
        var u = function(d) {
          i.off.apply(i, c), o(d);
        }, f = s.concat([u]), c = f.concat([]);
        i.on.apply(i, f);
      });
    };
  }
}, qe = {};
[W1, zw, Vw].forEach(function(t) {
  Ae(qe, t);
});
var qw = {
  animate: qe.animate(),
  animation: qe.animation(),
  animated: qe.animated(),
  clearQueue: qe.clearQueue(),
  delay: qe.delay(),
  delayAnimation: qe.delayAnimation(),
  stop: qe.stop()
}, es = {
  classes: function(e) {
    var r = this;
    if (e === void 0) {
      var n = [];
      return r[0]._private.classes.forEach(function(h) {
        return n.push(h);
      }), n;
    } else We(e) || (e = (e || "").match(/\S+/g) || []);
    for (var a = [], i = new pa(e), s = 0; s < r.length; s++) {
      for (var o = r[s], l = o._private, u = l.classes, f = !1, c = 0; c < e.length; c++) {
        var v = e[c], d = u.has(v);
        if (!d) {
          f = !0;
          break;
        }
      }
      f || (f = u.size !== e.length), f && (l.classes = i, a.push(o));
    }
    return a.length > 0 && this.spawn(a).updateStyle().emit("class"), r;
  },
  addClass: function(e) {
    return this.toggleClass(e, !0);
  },
  hasClass: function(e) {
    var r = this[0];
    return r != null && r._private.classes.has(e);
  },
  toggleClass: function(e, r) {
    We(e) || (e = e.match(/\S+/g) || []);
    for (var n = this, a = r === void 0, i = [], s = 0, o = n.length; s < o; s++)
      for (var l = n[s], u = l._private.classes, f = !1, c = 0; c < e.length; c++) {
        var v = e[c], d = u.has(v), h = !1;
        r || a && !d ? (u.add(v), h = !0) : (!r || a && d) && (u.delete(v), h = !0), !f && h && (i.push(l), f = !0);
      }
    return i.length > 0 && this.spawn(i).updateStyle().emit("class"), n;
  },
  removeClass: function(e) {
    return this.toggleClass(e, !1);
  },
  flashClass: function(e, r) {
    var n = this;
    if (r == null)
      r = 250;
    else if (r === 0)
      return n;
    return n.addClass(e), setTimeout(function() {
      n.removeClass(e);
    }, r), n;
  }
};
es.className = es.classNames = es.classes;
var Ne = {
  metaChar: "[\\!\\\"\\#\\$\\%\\&\\'\\(\\)\\*\\+\\,\\.\\/\\:\\;\\<\\=\\>\\?\\@\\[\\]\\^\\`\\{\\|\\}\\~]",
  // chars we need to escape in let names, etc
  comparatorOp: "=|\\!=|>|>=|<|<=|\\$=|\\^=|\\*=",
  // binary comparison op (used in data selectors)
  boolOp: "\\?|\\!|\\^",
  // boolean (unary) operators (used in data selectors)
  string: `"(?:\\\\"|[^"])*"|'(?:\\\\'|[^'])*'`,
  // string literals (used in data selectors) -- doublequotes | singlequotes
  number: vt,
  // number literal (used in data selectors) --- e.g. 0.1234, 1234, 12e123
  meta: "degree|indegree|outdegree",
  // allowed metadata fields (i.e. allowed functions to use from Collection)
  separator: "\\s*,\\s*",
  // queries are separated by commas, e.g. edge[foo = 'bar'], node.someClass
  descendant: "\\s+",
  child: "\\s+>\\s+",
  subject: "\\$",
  group: "node|edge|\\*",
  directedEdge: "\\s+->\\s+",
  undirectedEdge: "\\s+<->\\s+"
};
Ne.variable = "(?:[\\w-.]|(?:\\\\" + Ne.metaChar + "))+";
Ne.className = "(?:[\\w-]|(?:\\\\" + Ne.metaChar + "))+";
Ne.value = Ne.string + "|" + Ne.number;
Ne.id = Ne.variable;
(function() {
  var t, e, r;
  for (t = Ne.comparatorOp.split("|"), r = 0; r < t.length; r++)
    e = t[r], Ne.comparatorOp += "|@" + e;
  for (t = Ne.comparatorOp.split("|"), r = 0; r < t.length; r++)
    e = t[r], !(e.indexOf("!") >= 0) && e !== "=" && (Ne.comparatorOp += "|\\!" + e);
})();
var He = function() {
  return {
    checks: []
  };
}, we = {
  /** E.g. node */
  GROUP: 0,
  /** A collection of elements */
  COLLECTION: 1,
  /** A filter(ele) function */
  FILTER: 2,
  /** E.g. [foo > 1] */
  DATA_COMPARE: 3,
  /** E.g. [foo] */
  DATA_EXIST: 4,
  /** E.g. [?foo] */
  DATA_BOOL: 5,
  /** E.g. [[degree > 2]] */
  META_COMPARE: 6,
  /** E.g. :selected */
  STATE: 7,
  /** E.g. #foo */
  ID: 8,
  /** E.g. .foo */
  CLASS: 9,
  /** E.g. #foo <-> #bar */
  UNDIRECTED_EDGE: 10,
  /** E.g. #foo -> #bar */
  DIRECTED_EDGE: 11,
  /** E.g. $#foo -> #bar */
  NODE_SOURCE: 12,
  /** E.g. #foo -> $#bar */
  NODE_TARGET: 13,
  /** E.g. $#foo <-> #bar */
  NODE_NEIGHBOR: 14,
  /** E.g. #foo > #bar */
  CHILD: 15,
  /** E.g. #foo #bar */
  DESCENDANT: 16,
  /** E.g. $#foo > #bar */
  PARENT: 17,
  /** E.g. $#foo #bar */
  ANCESTOR: 18,
  /** E.g. #foo > $bar > #baz */
  COMPOUND_SPLIT: 19,
  /** Always matches, useful placeholder for subject in `COMPOUND_SPLIT` */
  TRUE: 20
}, pu = [{
  selector: ":selected",
  matches: function(e) {
    return e.selected();
  }
}, {
  selector: ":unselected",
  matches: function(e) {
    return !e.selected();
  }
}, {
  selector: ":selectable",
  matches: function(e) {
    return e.selectable();
  }
}, {
  selector: ":unselectable",
  matches: function(e) {
    return !e.selectable();
  }
}, {
  selector: ":locked",
  matches: function(e) {
    return e.locked();
  }
}, {
  selector: ":unlocked",
  matches: function(e) {
    return !e.locked();
  }
}, {
  selector: ":visible",
  matches: function(e) {
    return e.visible();
  }
}, {
  selector: ":hidden",
  matches: function(e) {
    return !e.visible();
  }
}, {
  selector: ":transparent",
  matches: function(e) {
    return e.transparent();
  }
}, {
  selector: ":grabbed",
  matches: function(e) {
    return e.grabbed();
  }
}, {
  selector: ":free",
  matches: function(e) {
    return !e.grabbed();
  }
}, {
  selector: ":removed",
  matches: function(e) {
    return e.removed();
  }
}, {
  selector: ":inside",
  matches: function(e) {
    return !e.removed();
  }
}, {
  selector: ":grabbable",
  matches: function(e) {
    return e.grabbable();
  }
}, {
  selector: ":ungrabbable",
  matches: function(e) {
    return !e.grabbable();
  }
}, {
  selector: ":animated",
  matches: function(e) {
    return e.animated();
  }
}, {
  selector: ":unanimated",
  matches: function(e) {
    return !e.animated();
  }
}, {
  selector: ":parent",
  matches: function(e) {
    return e.isParent();
  }
}, {
  selector: ":childless",
  matches: function(e) {
    return e.isChildless();
  }
}, {
  selector: ":child",
  matches: function(e) {
    return e.isChild();
  }
}, {
  selector: ":orphan",
  matches: function(e) {
    return e.isOrphan();
  }
}, {
  selector: ":nonorphan",
  matches: function(e) {
    return e.isChild();
  }
}, {
  selector: ":compound",
  matches: function(e) {
    return e.isNode() ? e.isParent() : e.source().isParent() || e.target().isParent();
  }
}, {
  selector: ":loop",
  matches: function(e) {
    return e.isLoop();
  }
}, {
  selector: ":simple",
  matches: function(e) {
    return e.isSimple();
  }
}, {
  selector: ":active",
  matches: function(e) {
    return e.active();
  }
}, {
  selector: ":inactive",
  matches: function(e) {
    return !e.active();
  }
}, {
  selector: ":backgrounding",
  matches: function(e) {
    return e.backgrounding();
  }
}, {
  selector: ":nonbackgrounding",
  matches: function(e) {
    return !e.backgrounding();
  }
}].sort(function(t, e) {
  return z0(t.selector, e.selector);
}), $w = (function() {
  for (var t = {}, e, r = 0; r < pu.length; r++)
    e = pu[r], t[e.selector] = e.matches;
  return t;
})(), Hw = function(e, r) {
  return $w[e](r);
}, Uw = "(" + pu.map(function(t) {
  return t.selector;
}).join("|") + ")", Vn = function(e) {
  return e.replace(new RegExp("\\\\(" + Ne.metaChar + ")", "g"), function(r, n) {
    return n;
  });
}, Yr = function(e, r, n) {
  e[e.length - 1] = n;
}, yu = [{
  name: "group",
  // just used for identifying when debugging
  query: !0,
  regex: "(" + Ne.group + ")",
  populate: function(e, r, n) {
    var a = ut(n, 1), i = a[0];
    r.checks.push({
      type: we.GROUP,
      value: i === "*" ? i : i + "s"
    });
  }
}, {
  name: "state",
  query: !0,
  regex: Uw,
  populate: function(e, r, n) {
    var a = ut(n, 1), i = a[0];
    r.checks.push({
      type: we.STATE,
      value: i
    });
  }
}, {
  name: "id",
  query: !0,
  regex: "\\#(" + Ne.id + ")",
  populate: function(e, r, n) {
    var a = ut(n, 1), i = a[0];
    r.checks.push({
      type: we.ID,
      value: Vn(i)
    });
  }
}, {
  name: "className",
  query: !0,
  regex: "\\.(" + Ne.className + ")",
  populate: function(e, r, n) {
    var a = ut(n, 1), i = a[0];
    r.checks.push({
      type: we.CLASS,
      value: Vn(i)
    });
  }
}, {
  name: "dataExists",
  query: !0,
  regex: "\\[\\s*(" + Ne.variable + ")\\s*\\]",
  populate: function(e, r, n) {
    var a = ut(n, 1), i = a[0];
    r.checks.push({
      type: we.DATA_EXIST,
      field: Vn(i)
    });
  }
}, {
  name: "dataCompare",
  query: !0,
  regex: "\\[\\s*(" + Ne.variable + ")\\s*(" + Ne.comparatorOp + ")\\s*(" + Ne.value + ")\\s*\\]",
  populate: function(e, r, n) {
    var a = ut(n, 3), i = a[0], s = a[1], o = a[2], l = new RegExp("^" + Ne.string + "$").exec(o) != null;
    l ? o = o.substring(1, o.length - 1) : o = parseFloat(o), r.checks.push({
      type: we.DATA_COMPARE,
      field: Vn(i),
      operator: s,
      value: o
    });
  }
}, {
  name: "dataBool",
  query: !0,
  regex: "\\[\\s*(" + Ne.boolOp + ")\\s*(" + Ne.variable + ")\\s*\\]",
  populate: function(e, r, n) {
    var a = ut(n, 2), i = a[0], s = a[1];
    r.checks.push({
      type: we.DATA_BOOL,
      field: Vn(s),
      operator: i
    });
  }
}, {
  name: "metaCompare",
  query: !0,
  regex: "\\[\\[\\s*(" + Ne.meta + ")\\s*(" + Ne.comparatorOp + ")\\s*(" + Ne.number + ")\\s*\\]\\]",
  populate: function(e, r, n) {
    var a = ut(n, 3), i = a[0], s = a[1], o = a[2];
    r.checks.push({
      type: we.META_COMPARE,
      field: Vn(i),
      operator: s,
      value: parseFloat(o)
    });
  }
}, {
  name: "nextQuery",
  separator: !0,
  regex: Ne.separator,
  populate: function(e, r) {
    var n = e.currentSubject, a = e.edgeCount, i = e.compoundCount, s = e[e.length - 1];
    n != null && (s.subject = n, e.currentSubject = null), s.edgeCount = a, s.compoundCount = i, e.edgeCount = 0, e.compoundCount = 0;
    var o = e[e.length++] = He();
    return o;
  }
}, {
  name: "directedEdge",
  separator: !0,
  regex: Ne.directedEdge,
  populate: function(e, r) {
    if (e.currentSubject == null) {
      var n = He(), a = r, i = He();
      return n.checks.push({
        type: we.DIRECTED_EDGE,
        source: a,
        target: i
      }), Yr(e, r, n), e.edgeCount++, i;
    } else {
      var s = He(), o = r, l = He();
      return s.checks.push({
        type: we.NODE_SOURCE,
        source: o,
        target: l
      }), Yr(e, r, s), e.edgeCount++, l;
    }
  }
}, {
  name: "undirectedEdge",
  separator: !0,
  regex: Ne.undirectedEdge,
  populate: function(e, r) {
    if (e.currentSubject == null) {
      var n = He(), a = r, i = He();
      return n.checks.push({
        type: we.UNDIRECTED_EDGE,
        nodes: [a, i]
      }), Yr(e, r, n), e.edgeCount++, i;
    } else {
      var s = He(), o = r, l = He();
      return s.checks.push({
        type: we.NODE_NEIGHBOR,
        node: o,
        neighbor: l
      }), Yr(e, r, s), l;
    }
  }
}, {
  name: "child",
  separator: !0,
  regex: Ne.child,
  populate: function(e, r) {
    if (e.currentSubject == null) {
      var n = He(), a = He(), i = e[e.length - 1];
      return n.checks.push({
        type: we.CHILD,
        parent: i,
        child: a
      }), Yr(e, r, n), e.compoundCount++, a;
    } else if (e.currentSubject === r) {
      var s = He(), o = e[e.length - 1], l = He(), u = He(), f = He(), c = He();
      return s.checks.push({
        type: we.COMPOUND_SPLIT,
        left: o,
        right: l,
        subject: u
      }), u.checks = r.checks, r.checks = [{
        type: we.TRUE
      }], c.checks.push({
        type: we.TRUE
      }), l.checks.push({
        type: we.PARENT,
        // type is swapped on right side queries
        parent: c,
        child: f
        // empty for now
      }), Yr(e, o, s), e.currentSubject = u, e.compoundCount++, f;
    } else {
      var v = He(), d = He(), h = [{
        type: we.PARENT,
        parent: v,
        child: d
      }];
      return v.checks = r.checks, r.checks = h, e.compoundCount++, d;
    }
  }
}, {
  name: "descendant",
  separator: !0,
  regex: Ne.descendant,
  populate: function(e, r) {
    if (e.currentSubject == null) {
      var n = He(), a = He(), i = e[e.length - 1];
      return n.checks.push({
        type: we.DESCENDANT,
        ancestor: i,
        descendant: a
      }), Yr(e, r, n), e.compoundCount++, a;
    } else if (e.currentSubject === r) {
      var s = He(), o = e[e.length - 1], l = He(), u = He(), f = He(), c = He();
      return s.checks.push({
        type: we.COMPOUND_SPLIT,
        left: o,
        right: l,
        subject: u
      }), u.checks = r.checks, r.checks = [{
        type: we.TRUE
      }], c.checks.push({
        type: we.TRUE
      }), l.checks.push({
        type: we.ANCESTOR,
        // type is swapped on right side queries
        ancestor: c,
        descendant: f
        // empty for now
      }), Yr(e, o, s), e.currentSubject = u, e.compoundCount++, f;
    } else {
      var v = He(), d = He(), h = [{
        type: we.ANCESTOR,
        ancestor: v,
        descendant: d
      }];
      return v.checks = r.checks, r.checks = h, e.compoundCount++, d;
    }
  }
}, {
  name: "subject",
  modifier: !0,
  regex: Ne.subject,
  populate: function(e, r) {
    if (e.currentSubject != null && e.currentSubject !== r)
      return $e("Redefinition of subject in selector `" + e.toString() + "`"), !1;
    e.currentSubject = r;
    var n = e[e.length - 1], a = n.checks[0], i = a == null ? null : a.type;
    i === we.DIRECTED_EDGE ? a.type = we.NODE_TARGET : i === we.UNDIRECTED_EDGE && (a.type = we.NODE_NEIGHBOR, a.node = a.nodes[1], a.neighbor = a.nodes[0], a.nodes = null);
  }
}];
yu.forEach(function(t) {
  return t.regexObj = new RegExp("^" + t.regex);
});
var Gw = function(e) {
  for (var r, n, a, i = 0; i < yu.length; i++) {
    var s = yu[i], o = s.name, l = e.match(s.regexObj);
    if (l != null) {
      n = l, r = s, a = o;
      var u = l[0];
      e = e.substring(u.length);
      break;
    }
  }
  return {
    expr: r,
    match: n,
    name: a,
    remaining: e
  };
}, Ww = function(e) {
  var r = e.match(/^\s+/);
  if (r) {
    var n = r[0];
    e = e.substring(n.length);
  }
  return e;
}, Kw = function(e) {
  var r = this, n = r.inputText = e, a = r[0] = He();
  for (r.length = 1, n = Ww(n); ; ) {
    var i = Gw(n);
    if (i.expr == null)
      return $e("The selector `" + e + "`is invalid"), !1;
    var s = i.match.slice(1), o = i.expr.populate(r, a, s);
    if (o === !1)
      return !1;
    if (o != null && (a = o), n = i.remaining, n.match(/^\s*$/))
      break;
  }
  var l = r[r.length - 1];
  r.currentSubject != null && (l.subject = r.currentSubject), l.edgeCount = r.edgeCount, l.compoundCount = r.compoundCount;
  for (var u = 0; u < r.length; u++) {
    var f = r[u];
    if (f.compoundCount > 0 && f.edgeCount > 0)
      return $e("The selector `" + e + "` is invalid because it uses both a compound selector and an edge selector"), !1;
    if (f.edgeCount > 1)
      return $e("The selector `" + e + "` is invalid because it uses multiple edge selectors"), !1;
    f.edgeCount === 1 && $e("The selector `" + e + "` is deprecated.  Edge selectors do not take effect on changes to source and target nodes after an edge is added, for performance reasons.  Use a class or data selector on edges instead, updating the class or data of an edge when your app detects a change in source or target nodes.");
  }
  return !0;
}, Yw = function() {
  if (this.toStringCache != null)
    return this.toStringCache;
  for (var e = function(f) {
    return f ?? "";
  }, r = function(f) {
    return Se(f) ? '"' + f + '"' : e(f);
  }, n = function(f) {
    return " " + f + " ";
  }, a = function(f, c) {
    var v = f.type, d = f.value;
    switch (v) {
      case we.GROUP: {
        var h = e(d);
        return h.substring(0, h.length - 1);
      }
      case we.DATA_COMPARE: {
        var y = f.field, g = f.operator;
        return "[" + y + n(e(g)) + r(d) + "]";
      }
      case we.DATA_BOOL: {
        var p = f.operator, m = f.field;
        return "[" + e(p) + m + "]";
      }
      case we.DATA_EXIST: {
        var b = f.field;
        return "[" + b + "]";
      }
      case we.META_COMPARE: {
        var w = f.operator, E = f.field;
        return "[[" + E + n(e(w)) + r(d) + "]]";
      }
      case we.STATE:
        return d;
      case we.ID:
        return "#" + d;
      case we.CLASS:
        return "." + d;
      case we.PARENT:
      case we.CHILD:
        return i(f.parent, c) + n(">") + i(f.child, c);
      case we.ANCESTOR:
      case we.DESCENDANT:
        return i(f.ancestor, c) + " " + i(f.descendant, c);
      case we.COMPOUND_SPLIT: {
        var T = i(f.left, c), x = i(f.subject, c), S = i(f.right, c);
        return T + (T.length > 0 ? " " : "") + x + S;
      }
      case we.TRUE:
        return "";
    }
  }, i = function(f, c) {
    return f.checks.reduce(function(v, d, h) {
      return v + (c === f && h === 0 ? "$" : "") + a(d, c);
    }, "");
  }, s = "", o = 0; o < this.length; o++) {
    var l = this[o];
    s += i(l, l.subject), this.length > 1 && o < this.length - 1 && (s += ", ");
  }
  return this.toStringCache = s, s;
}, Xw = {
  parse: Kw,
  toString: Yw
}, Cg = function(e, r, n) {
  var a, i = Se(e), s = pe(e), o = Se(n), l, u, f = !1, c = !1, v = !1;
  switch (r.indexOf("!") >= 0 && (r = r.replace("!", ""), c = !0), r.indexOf("@") >= 0 && (r = r.replace("@", ""), f = !0), (i || o || f) && (l = !i && !s ? "" : "" + e, u = "" + n), f && (e = l = l.toLowerCase(), n = u = u.toLowerCase()), r) {
    case "*=":
      a = l.indexOf(u) >= 0;
      break;
    case "$=":
      a = l.indexOf(u, l.length - u.length) >= 0;
      break;
    case "^=":
      a = l.indexOf(u) === 0;
      break;
    case "=":
      a = e === n;
      break;
    case ">":
      v = !0, a = e > n;
      break;
    case ">=":
      v = !0, a = e >= n;
      break;
    case "<":
      v = !0, a = e < n;
      break;
    case "<=":
      v = !0, a = e <= n;
      break;
    default:
      a = !1;
      break;
  }
  return c && (e != null || !v) && (a = !a), a;
}, Zw = function(e, r) {
  switch (r) {
    case "?":
      return !!e;
    case "!":
      return !e;
    case "^":
      return e === void 0;
  }
}, Qw = function(e) {
  return e !== void 0;
}, sf = function(e, r) {
  return e.data(r);
}, jw = function(e, r) {
  return e[r]();
}, it = [], Ze = function(e, r) {
  return e.checks.every(function(n) {
    return it[n.type](n, r);
  });
};
it[we.GROUP] = function(t, e) {
  var r = t.value;
  return r === "*" || r === e.group();
};
it[we.STATE] = function(t, e) {
  var r = t.value;
  return Hw(r, e);
};
it[we.ID] = function(t, e) {
  var r = t.value;
  return e.id() === r;
};
it[we.CLASS] = function(t, e) {
  var r = t.value;
  return e.hasClass(r);
};
it[we.META_COMPARE] = function(t, e) {
  var r = t.field, n = t.operator, a = t.value;
  return Cg(jw(e, r), n, a);
};
it[we.DATA_COMPARE] = function(t, e) {
  var r = t.field, n = t.operator, a = t.value;
  return Cg(sf(e, r), n, a);
};
it[we.DATA_BOOL] = function(t, e) {
  var r = t.field, n = t.operator;
  return Zw(sf(e, r), n);
};
it[we.DATA_EXIST] = function(t, e) {
  var r = t.field;
  return t.operator, Qw(sf(e, r));
};
it[we.UNDIRECTED_EDGE] = function(t, e) {
  var r = t.nodes[0], n = t.nodes[1], a = e.source(), i = e.target();
  return Ze(r, a) && Ze(n, i) || Ze(n, a) && Ze(r, i);
};
it[we.NODE_NEIGHBOR] = function(t, e) {
  return Ze(t.node, e) && e.neighborhood().some(function(r) {
    return r.isNode() && Ze(t.neighbor, r);
  });
};
it[we.DIRECTED_EDGE] = function(t, e) {
  return Ze(t.source, e.source()) && Ze(t.target, e.target());
};
it[we.NODE_SOURCE] = function(t, e) {
  return Ze(t.source, e) && e.outgoers().some(function(r) {
    return r.isNode() && Ze(t.target, r);
  });
};
it[we.NODE_TARGET] = function(t, e) {
  return Ze(t.target, e) && e.incomers().some(function(r) {
    return r.isNode() && Ze(t.source, r);
  });
};
it[we.CHILD] = function(t, e) {
  return Ze(t.child, e) && Ze(t.parent, e.parent());
};
it[we.PARENT] = function(t, e) {
  return Ze(t.parent, e) && e.children().some(function(r) {
    return Ze(t.child, r);
  });
};
it[we.DESCENDANT] = function(t, e) {
  return Ze(t.descendant, e) && e.ancestors().some(function(r) {
    return Ze(t.ancestor, r);
  });
};
it[we.ANCESTOR] = function(t, e) {
  return Ze(t.ancestor, e) && e.descendants().some(function(r) {
    return Ze(t.descendant, r);
  });
};
it[we.COMPOUND_SPLIT] = function(t, e) {
  return Ze(t.subject, e) && Ze(t.left, e) && Ze(t.right, e);
};
it[we.TRUE] = function() {
  return !0;
};
it[we.COLLECTION] = function(t, e) {
  var r = t.value;
  return r.has(e);
};
it[we.FILTER] = function(t, e) {
  var r = t.value;
  return r(e);
};
var Jw = function(e) {
  var r = this;
  if (r.length === 1 && r[0].checks.length === 1 && r[0].checks[0].type === we.ID)
    return e.getElementById(r[0].checks[0].value).collection();
  var n = function(i) {
    for (var s = 0; s < r.length; s++) {
      var o = r[s];
      if (Ze(o, i))
        return !0;
    }
    return !1;
  };
  return r.text() == null && (n = function() {
    return !0;
  }), e.filter(n);
}, ex = function(e) {
  for (var r = this, n = 0; n < r.length; n++) {
    var a = r[n];
    if (Ze(a, e))
      return !0;
  }
  return !1;
}, tx = {
  matches: ex,
  filter: Jw
}, on = function(e) {
  this.inputText = e, this.currentSubject = null, this.compoundCount = 0, this.edgeCount = 0, this.length = 0, e == null || Se(e) && e.match(/^\s*$/) || (Xt(e) ? this.addQuery({
    checks: [{
      type: we.COLLECTION,
      value: e.collection()
    }]
  }) : tt(e) ? this.addQuery({
    checks: [{
      type: we.FILTER,
      value: e
    }]
  }) : Se(e) ? this.parse(e) || (this.invalid = !0) : je("A selector must be created from a string; found "));
}, ln = on.prototype;
[Xw, tx].forEach(function(t) {
  return Ae(ln, t);
});
ln.text = function() {
  return this.inputText;
};
ln.size = function() {
  return this.length;
};
ln.eq = function(t) {
  return this[t];
};
ln.sameText = function(t) {
  return !this.invalid && !t.invalid && this.text() === t.text();
};
ln.addQuery = function(t) {
  this[this.length++] = t;
};
ln.selector = ln.toString;
var en = {
  allAre: function(e) {
    var r = new on(e);
    return this.every(function(n) {
      return r.matches(n);
    });
  },
  is: function(e) {
    var r = new on(e);
    return this.some(function(n) {
      return r.matches(n);
    });
  },
  some: function(e, r) {
    for (var n = 0; n < this.length; n++) {
      var a = r ? e.apply(r, [this[n], n, this]) : e(this[n], n, this);
      if (a)
        return !0;
    }
    return !1;
  },
  every: function(e, r) {
    for (var n = 0; n < this.length; n++) {
      var a = r ? e.apply(r, [this[n], n, this]) : e(this[n], n, this);
      if (!a)
        return !1;
    }
    return !0;
  },
  same: function(e) {
    if (this === e)
      return !0;
    e = this.cy().collection(e);
    var r = this.length, n = e.length;
    return r !== n ? !1 : r === 1 ? this[0] === e[0] : this.every(function(a) {
      return e.hasElementWithId(a.id());
    });
  },
  anySame: function(e) {
    return e = this.cy().collection(e), this.some(function(r) {
      return e.hasElementWithId(r.id());
    });
  },
  allAreNeighbors: function(e) {
    e = this.cy().collection(e);
    var r = this.neighborhood();
    return e.every(function(n) {
      return r.hasElementWithId(n.id());
    });
  },
  contains: function(e) {
    e = this.cy().collection(e);
    var r = this;
    return e.every(function(n) {
      return r.hasElementWithId(n.id());
    });
  }
};
en.allAreNeighbours = en.allAreNeighbors;
en.has = en.contains;
en.equal = en.equals = en.same;
var Jt = function(e, r) {
  return function(a, i, s, o) {
    var l = a, u = this, f;
    if (l == null ? f = "" : Xt(l) && l.length === 1 && (f = l.id()), u.length === 1 && f) {
      var c = u[0]._private, v = c.traversalCache = c.traversalCache || {}, d = v[r] = v[r] || [], h = Bn(f), y = d[h];
      return y || (d[h] = e.call(u, a, i, s, o));
    } else
      return e.call(u, a, i, s, o);
  };
}, da = {
  parent: function(e) {
    var r = [];
    if (this.length === 1) {
      var n = this[0]._private.parent;
      if (n)
        return n;
    }
    for (var a = 0; a < this.length; a++) {
      var i = this[a], s = i._private.parent;
      s && r.push(s);
    }
    return this.spawn(r, !0).filter(e);
  },
  parents: function(e) {
    for (var r = [], n = this.parent(); n.nonempty(); ) {
      for (var a = 0; a < n.length; a++) {
        var i = n[a];
        r.push(i);
      }
      n = n.parent();
    }
    return this.spawn(r, !0).filter(e);
  },
  commonAncestors: function(e) {
    for (var r, n = 0; n < this.length; n++) {
      var a = this[n], i = a.parents();
      r = r || i, r = r.intersect(i);
    }
    return r.filter(e);
  },
  orphans: function(e) {
    return this.stdFilter(function(r) {
      return r.isOrphan();
    }).filter(e);
  },
  nonorphans: function(e) {
    return this.stdFilter(function(r) {
      return r.isChild();
    }).filter(e);
  },
  children: Jt(function(t) {
    for (var e = [], r = 0; r < this.length; r++)
      for (var n = this[r], a = n._private.children, i = 0; i < a.length; i++)
        e.push(a[i]);
    return this.spawn(e, !0).filter(t);
  }, "children"),
  siblings: function(e) {
    return this.parent().children().not(this).filter(e);
  },
  isParent: function() {
    var e = this[0];
    if (e)
      return e.isNode() && e._private.children.length !== 0;
  },
  isChildless: function() {
    var e = this[0];
    if (e)
      return e.isNode() && e._private.children.length === 0;
  },
  isChild: function() {
    var e = this[0];
    if (e)
      return e.isNode() && e._private.parent != null;
  },
  isOrphan: function() {
    var e = this[0];
    if (e)
      return e.isNode() && e._private.parent == null;
  },
  descendants: function(e) {
    var r = [];
    function n(a) {
      for (var i = 0; i < a.length; i++) {
        var s = a[i];
        r.push(s), s.children().nonempty() && n(s.children());
      }
    }
    return n(this.children()), this.spawn(r, !0).filter(e);
  }
};
function of(t, e, r, n) {
  for (var a = [], i = new pa(), s = t.cy(), o = s.hasCompoundNodes(), l = 0; l < t.length; l++) {
    var u = t[l];
    r ? a.push(u) : o && n(a, i, u);
  }
  for (; a.length > 0; ) {
    var f = a.shift();
    e(f), i.add(f.id()), o && n(a, i, f);
  }
  return t;
}
function Tg(t, e, r) {
  if (r.isParent())
    for (var n = r._private.children, a = 0; a < n.length; a++) {
      var i = n[a];
      e.has(i.id()) || t.push(i);
    }
}
da.forEachDown = function(t) {
  var e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0;
  return of(this, t, e, Tg);
};
function Sg(t, e, r) {
  if (r.isChild()) {
    var n = r._private.parent;
    e.has(n.id()) || t.push(n);
  }
}
da.forEachUp = function(t) {
  var e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0;
  return of(this, t, e, Sg);
};
function rx(t, e, r) {
  Sg(t, e, r), Tg(t, e, r);
}
da.forEachUpAndDown = function(t) {
  var e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0;
  return of(this, t, e, rx);
};
da.ancestors = da.parents;
var si, Pg;
si = Pg = {
  data: qe.data({
    field: "data",
    bindingEvent: "data",
    allowBinding: !0,
    allowSetting: !0,
    settingEvent: "data",
    settingTriggersEvent: !0,
    triggerFnName: "trigger",
    allowGetting: !0,
    immutableKeys: {
      id: !0,
      source: !0,
      target: !0,
      parent: !0
    },
    updateStyle: !0
  }),
  removeData: qe.removeData({
    field: "data",
    event: "data",
    triggerFnName: "trigger",
    triggerEvent: !0,
    immutableKeys: {
      id: !0,
      source: !0,
      target: !0,
      parent: !0
    },
    updateStyle: !0
  }),
  scratch: qe.data({
    field: "scratch",
    bindingEvent: "scratch",
    allowBinding: !0,
    allowSetting: !0,
    settingEvent: "scratch",
    settingTriggersEvent: !0,
    triggerFnName: "trigger",
    allowGetting: !0,
    updateStyle: !0
  }),
  removeScratch: qe.removeData({
    field: "scratch",
    event: "scratch",
    triggerFnName: "trigger",
    triggerEvent: !0,
    updateStyle: !0
  }),
  rscratch: qe.data({
    field: "rscratch",
    allowBinding: !1,
    allowSetting: !0,
    settingTriggersEvent: !1,
    allowGetting: !0
  }),
  removeRscratch: qe.removeData({
    field: "rscratch",
    triggerEvent: !1
  }),
  id: function() {
    var e = this[0];
    if (e)
      return e._private.data.id;
  }
};
si.attr = si.data;
si.removeAttr = si.removeData;
var nx = Pg, Js = {};
function Vl(t) {
  return function(e) {
    var r = this;
    if (e === void 0 && (e = !0), r.length !== 0)
      if (r.isNode() && !r.removed()) {
        for (var n = 0, a = r[0], i = a._private.edges, s = 0; s < i.length; s++) {
          var o = i[s];
          !e && o.isLoop() || (n += t(a, o));
        }
        return n;
      } else
        return;
  };
}
Ae(Js, {
  degree: Vl(function(t, e) {
    return e.source().same(e.target()) ? 2 : 1;
  }),
  indegree: Vl(function(t, e) {
    return e.target().same(t) ? 1 : 0;
  }),
  outdegree: Vl(function(t, e) {
    return e.source().same(t) ? 1 : 0;
  })
});
function qn(t, e) {
  return function(r) {
    for (var n, a = this.nodes(), i = 0; i < a.length; i++) {
      var s = a[i], o = s[t](r);
      o !== void 0 && (n === void 0 || e(o, n)) && (n = o);
    }
    return n;
  };
}
Ae(Js, {
  minDegree: qn("degree", function(t, e) {
    return t < e;
  }),
  maxDegree: qn("degree", function(t, e) {
    return t > e;
  }),
  minIndegree: qn("indegree", function(t, e) {
    return t < e;
  }),
  maxIndegree: qn("indegree", function(t, e) {
    return t > e;
  }),
  minOutdegree: qn("outdegree", function(t, e) {
    return t < e;
  }),
  maxOutdegree: qn("outdegree", function(t, e) {
    return t > e;
  })
});
Ae(Js, {
  totalDegree: function(e) {
    for (var r = 0, n = this.nodes(), a = 0; a < n.length; a++)
      r += n[a].degree(e);
    return r;
  }
});
var or, Dg, Ag = function(e, r, n) {
  for (var a = 0; a < e.length; a++) {
    var i = e[a];
    if (!i.locked()) {
      var s = i._private.position, o = {
        x: r.x != null ? r.x - s.x : 0,
        y: r.y != null ? r.y - s.y : 0
      };
      i.isParent() && !(o.x === 0 && o.y === 0) && i.children().shift(o, n), i.dirtyBoundingBoxCache();
    }
  }
}, Nv = {
  field: "position",
  bindingEvent: "position",
  allowBinding: !0,
  allowSetting: !0,
  settingEvent: "position",
  settingTriggersEvent: !0,
  triggerFnName: "emitAndNotify",
  allowGetting: !0,
  validKeys: ["x", "y"],
  beforeGet: function(e) {
    e.updateCompoundBounds();
  },
  beforeSet: function(e, r) {
    Ag(e, r, !1);
  },
  onSet: function(e) {
    e.dirtyCompoundBoundsCache();
  },
  canSet: function(e) {
    return !e.locked();
  }
};
or = Dg = {
  position: qe.data(Nv),
  // position but no notification to renderer
  silentPosition: qe.data(Ae({}, Nv, {
    allowBinding: !1,
    allowSetting: !0,
    settingTriggersEvent: !1,
    allowGetting: !1,
    beforeSet: function(e, r) {
      Ag(e, r, !0);
    },
    onSet: function(e) {
      e.dirtyCompoundBoundsCache();
    }
  })),
  positions: function(e, r) {
    if (_e(e))
      r ? this.silentPosition(e) : this.position(e);
    else if (tt(e)) {
      var n = e, a = this.cy();
      a.startBatch();
      for (var i = 0; i < this.length; i++) {
        var s = this[i], o = void 0;
        (o = n(s, i)) && (r ? s.silentPosition(o) : s.position(o));
      }
      a.endBatch();
    }
    return this;
  },
  silentPositions: function(e) {
    return this.positions(e, !0);
  },
  shift: function(e, r, n) {
    var a;
    if (_e(e) ? (a = {
      x: pe(e.x) ? e.x : 0,
      y: pe(e.y) ? e.y : 0
    }, n = r) : Se(e) && pe(r) && (a = {
      x: 0,
      y: 0
    }, a[e] = r), a != null) {
      var i = this.cy();
      i.startBatch();
      for (var s = 0; s < this.length; s++) {
        var o = this[s];
        if (!(i.hasCompoundNodes() && o.isChild() && o.ancestors().anySame(this))) {
          var l = o.position(), u = {
            x: l.x + a.x,
            y: l.y + a.y
          };
          n ? o.silentPosition(u) : o.position(u);
        }
      }
      i.endBatch();
    }
    return this;
  },
  silentShift: function(e, r) {
    return _e(e) ? this.shift(e, !0) : Se(e) && pe(r) && this.shift(e, r, !0), this;
  },
  // get/set the rendered (i.e. on screen) positon of the element
  renderedPosition: function(e, r) {
    var n = this[0], a = this.cy(), i = a.zoom(), s = a.pan(), o = _e(e) ? e : void 0, l = o !== void 0 || r !== void 0 && Se(e);
    if (n && n.isNode())
      if (l)
        for (var u = 0; u < this.length; u++) {
          var f = this[u];
          r !== void 0 ? f.position(e, (r - s[e]) / i) : o !== void 0 && f.position(rg(o, i, s));
        }
      else {
        var c = n.position();
        return o = Ks(c, i, s), e === void 0 ? o : o[e];
      }
    else if (!l)
      return;
    return this;
  },
  // get/set the position relative to the parent
  relativePosition: function(e, r) {
    var n = this[0], a = this.cy(), i = _e(e) ? e : void 0, s = i !== void 0 || r !== void 0 && Se(e), o = a.hasCompoundNodes();
    if (n && n.isNode())
      if (s)
        for (var l = 0; l < this.length; l++) {
          var u = this[l], f = o ? u.parent() : null, c = f && f.length > 0, v = c;
          c && (f = f[0]);
          var d = v ? f.position() : {
            x: 0,
            y: 0
          };
          r !== void 0 ? u.position(e, r + d[e]) : i !== void 0 && u.position({
            x: i.x + d.x,
            y: i.y + d.y
          });
        }
      else {
        var h = n.position(), y = o ? n.parent() : null, g = y && y.length > 0, p = g;
        g && (y = y[0]);
        var m = p ? y.position() : {
          x: 0,
          y: 0
        };
        return i = {
          x: h.x - m.x,
          y: h.y - m.y
        }, e === void 0 ? i : i[e];
      }
    else if (!s)
      return;
    return this;
  }
};
or.modelPosition = or.point = or.position;
or.modelPositions = or.points = or.positions;
or.renderedPoint = or.renderedPosition;
or.relativePoint = or.relativePosition;
var ax = Dg, ha = function(e) {
  switch (e) {
    case "left":
    case "right-inside":
      return "left";
    case "right":
    case "left-inside":
      return "right";
    default:
      return "center";
  }
}, ga = function(e) {
  switch (e) {
    case "top":
    case "bottom-inside":
      return "top";
    case "bottom":
    case "top-inside":
      return "bottom";
    default:
      return "center";
  }
}, ix = function(e) {
  switch (e) {
    case "left":
      return "right";
    case "right":
      return "left";
    case "left-inside":
      return "left";
    case "right-inside":
      return "right";
    default:
      return "center";
  }
}, oa, vn;
oa = vn = {};
vn.renderedBoundingBox = function(t) {
  var e = this.boundingBox(t), r = this.cy(), n = r.zoom(), a = r.pan(), i = e.x1 * n + a.x, s = e.x2 * n + a.x, o = e.y1 * n + a.y, l = e.y2 * n + a.y;
  return {
    x1: i,
    x2: s,
    y1: o,
    y2: l,
    w: s - i,
    h: l - o
  };
};
vn.dirtyCompoundBoundsCache = function() {
  var t = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : !1, e = this.cy();
  return !e.styleEnabled() || !e.hasCompoundNodes() ? this : (this.forEachUp(function(r) {
    if (r.isParent()) {
      var n = r._private;
      n.compoundBoundsClean = !1, n.bbCache = null, t || r.emitAndNotify("bounds");
    }
  }), this);
};
vn.updateCompoundBounds = function() {
  var t = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : !1, e = this.cy();
  if (!e.styleEnabled() || !e.hasCompoundNodes())
    return this;
  if (!t && e.batching())
    return this;
  function r(s) {
    if (!s.isParent())
      return;
    var o = s._private, l = s.children(), u = s.pstyle("compound-sizing-wrt-labels").value === "include", f = {
      width: {
        val: s.pstyle("min-width").pfValue,
        left: s.pstyle("min-width-bias-left"),
        right: s.pstyle("min-width-bias-right")
      },
      height: {
        val: s.pstyle("min-height").pfValue,
        top: s.pstyle("min-height-bias-top"),
        bottom: s.pstyle("min-height-bias-bottom")
      }
    }, c = l.boundingBox({
      includeLabels: u,
      includeOverlays: !1,
      // updating the compound bounds happens outside of the regular
      // cache cycle (i.e. before fired events)
      useCache: !1
    }), v = o.position;
    (c.w === 0 || c.h === 0) && (c = {
      w: s.pstyle("width").pfValue,
      h: s.pstyle("height").pfValue
    }, c.x1 = v.x - c.w / 2, c.x2 = v.x + c.w / 2, c.y1 = v.y - c.h / 2, c.y2 = v.y + c.h / 2);
    function d(D, A, k) {
      var R = 0, M = 0, I = A + k;
      return D > 0 && I > 0 && (R = A / I * D, M = k / I * D), {
        biasDiff: R,
        biasComplementDiff: M
      };
    }
    function h(D, A, k, R) {
      if (k.units === "%")
        switch (R) {
          case "width":
            return D > 0 ? k.pfValue * D : 0;
          case "height":
            return A > 0 ? k.pfValue * A : 0;
          case "average":
            return D > 0 && A > 0 ? k.pfValue * (D + A) / 2 : 0;
          case "min":
            return D > 0 && A > 0 ? D > A ? k.pfValue * A : k.pfValue * D : 0;
          case "max":
            return D > 0 && A > 0 ? D > A ? k.pfValue * D : k.pfValue * A : 0;
          default:
            return 0;
        }
      else return k.units === "px" ? k.pfValue : 0;
    }
    var y = f.width.left.value;
    f.width.left.units === "px" && f.width.val > 0 && (y = y * 100 / f.width.val);
    var g = f.width.right.value;
    f.width.right.units === "px" && f.width.val > 0 && (g = g * 100 / f.width.val);
    var p = f.height.top.value;
    f.height.top.units === "px" && f.height.val > 0 && (p = p * 100 / f.height.val);
    var m = f.height.bottom.value;
    f.height.bottom.units === "px" && f.height.val > 0 && (m = m * 100 / f.height.val);
    var b = d(f.width.val - c.w, y, g), w = b.biasDiff, E = b.biasComplementDiff, T = d(f.height.val - c.h, p, m), x = T.biasDiff, S = T.biasComplementDiff;
    o.autoPadding = h(c.w, c.h, s.pstyle("padding"), s.pstyle("padding-relative-to").value), o.autoWidth = Math.max(c.w, f.width.val), v.x = (-w + c.x1 + c.x2 + E) / 2, o.autoHeight = Math.max(c.h, f.height.val), v.y = (-x + c.y1 + c.y2 + S) / 2;
  }
  for (var n = 0; n < this.length; n++) {
    var a = this[n], i = a._private;
    (!i.compoundBoundsClean || t) && (r(a), e.batching() || (i.compoundBoundsClean = !0));
  }
  return this;
};
var jt = function(e) {
  return e === 1 / 0 || e === -1 / 0 ? 0 : e;
}, ir = function(e, r, n, a, i) {
  a - r === 0 || i - n === 0 || r == null || n == null || a == null || i == null || (e.x1 = r < e.x1 ? r : e.x1, e.x2 = a > e.x2 ? a : e.x2, e.y1 = n < e.y1 ? n : e.y1, e.y2 = i > e.y2 ? i : e.y2, e.w = e.x2 - e.x1, e.h = e.y2 - e.y1);
}, Zr = function(e, r) {
  return r == null ? e : ir(e, r.x1, r.y1, r.x2, r.y2);
}, ka = function(e, r, n) {
  return Nt(e, r, n);
}, zi = function(e, r, n) {
  if (!r.cy().headless()) {
    var a = r._private, i = a.rstyle, s = i.arrowWidth / 2, o = r.pstyle(n + "-arrow-shape").value, l, u;
    if (o !== "none") {
      n === "source" ? (l = i.srcX, u = i.srcY) : n === "target" ? (l = i.tgtX, u = i.tgtY) : (l = i.midX, u = i.midY);
      var f = a.arrowBounds = a.arrowBounds || {}, c = f[n] = f[n] || {};
      c.x1 = l - s, c.y1 = u - s, c.x2 = l + s, c.y2 = u + s, c.w = c.x2 - c.x1, c.h = c.y2 - c.y1, ji(c, 1), ir(e, c.x1, c.y1, c.x2, c.y2);
    }
  }
}, ql = function(e, r, n) {
  if (!r.cy().headless()) {
    var a;
    n ? a = n + "-" : a = "";
    var i = r._private, s = i.rstyle, o = r.pstyle(a + "label").strValue;
    if (o) {
      var l = r.pstyle("text-halign"), u = r.pstyle("text-valign"), f = ka(s, "labelWidth", n), c = ka(s, "labelHeight", n), v = ka(s, "labelX", n), d = ka(s, "labelY", n), h = r.pstyle(a + "text-margin-x").pfValue, y = r.pstyle(a + "text-margin-y").pfValue, g = r.isEdge(), p = r.pstyle(a + "text-rotation"), m = r.pstyle("text-outline-width").pfValue, b = r.pstyle("text-border-width").pfValue, w = b / 2, E = r.pstyle("text-background-padding").pfValue, T = 2, x = c, S = f, D = S / 2, A = x / 2, k, R, M, I;
      if (g)
        k = v - D, R = v + D, M = d - A, I = d + A;
      else {
        switch (ha(l.value)) {
          case "left":
            k = v - S, R = v;
            break;
          case "center":
            k = v - D, R = v + D;
            break;
          case "right":
            k = v, R = v + S;
            break;
        }
        switch (ga(u.value)) {
          case "top":
            M = d - x, I = d;
            break;
          case "center":
            M = d - A, I = d + A;
            break;
          case "bottom":
            M = d, I = d + x;
            break;
        }
      }
      var _ = h - Math.max(m, w) - E - T, N = h + Math.max(m, w) + E + T, L = y - Math.max(m, w) - E - T, F = y + Math.max(m, w) + E + T;
      k += _, R += N, M += L, I += F;
      var H = n || "main", V = i.labelBounds, O = V[H] = V[H] || {};
      O.x1 = k, O.y1 = M, O.x2 = R, O.y2 = I, O.w = R - k, O.h = I - M, O.leftPad = _, O.rightPad = N, O.topPad = L, O.botPad = F;
      var $ = g && p.strValue === "autorotate", Q = p.pfValue != null && p.pfValue !== 0;
      if ($ || Q) {
        var se = $ ? ka(i.rstyle, "labelAngle", n) : p.pfValue, ae = Math.cos(se), le = Math.sin(se), ce = (k + R) / 2, he = (M + I) / 2;
        if (!g) {
          switch (ha(l.value)) {
            case "left":
              ce = R;
              break;
            case "right":
              ce = k;
              break;
          }
          switch (ga(u.value)) {
            case "top":
              he = I;
              break;
            case "bottom":
              he = M;
              break;
          }
        }
        var ie = function(Z, ne) {
          return Z = Z - ce, ne = ne - he, {
            x: Z * ae - ne * le + ce,
            y: Z * le + ne * ae + he
          };
        }, U = ie(k, M), X = ie(k, I), C = ie(R, M), B = ie(R, I);
        k = Math.min(U.x, X.x, C.x, B.x), R = Math.max(U.x, X.x, C.x, B.x), M = Math.min(U.y, X.y, C.y, B.y), I = Math.max(U.y, X.y, C.y, B.y);
      }
      var z = H + "Rot", W = V[z] = V[z] || {};
      W.x1 = k, W.y1 = M, W.x2 = R, W.y2 = I, W.w = R - k, W.h = I - M, ir(e, k, M, R, I), ir(i.labelBounds.all, k, M, R, I);
    }
    return e;
  }
}, _v = function(e, r) {
  if (!r.cy().headless()) {
    var n = r.pstyle("outline-opacity").value, a = r.pstyle("outline-width").value, i = r.pstyle("outline-offset").value, s = a + i;
    kg(e, r, n, s, "outside", s / 2);
  }
}, kg = function(e, r, n, a, i, s) {
  if (!(n === 0 || a <= 0 || i === "inside")) {
    var o = r.cy(), l = o.renderer(), u = l.nodeShapes[l.getNodeShape(r)];
    if (u) {
      var f = r.position(), c = f.x, v = f.y, d = r.width(), h = r.height();
      if (u.hasMiterBounds) {
        i === "center" && (a /= 2);
        var y = u.miterBounds(c, v, d, h, a);
        Zr(e, y);
      } else s != null && s > 0 && Ji(e, [s, s, s, s]);
    }
  }
}, sx = function(e, r) {
  if (!r.cy().headless()) {
    var n = r.pstyle("border-opacity").value, a = r.pstyle("border-width").pfValue, i = r.pstyle("border-position").value;
    kg(e, r, n, a, i);
  }
}, ox = function(e, r) {
  var n = e._private.cy, a = n.styleEnabled(), i = n.headless(), s = zt(), o = e._private, l = e.isNode(), u = e.isEdge(), f, c, v, d, h, y, g = o.rstyle, p = l && a ? e.pstyle("bounds-expansion").pfValue : [0], m = function(j) {
    return j.pstyle("display").value !== "none";
  }, b = !a || m(e) && (!u || m(e.source()) && m(e.target()));
  if (b) {
    var w = 0, E = 0;
    a && r.includeOverlays && (w = e.pstyle("overlay-opacity").value, w !== 0 && (E = e.pstyle("overlay-padding").value));
    var T = 0, x = 0;
    a && r.includeUnderlays && (T = e.pstyle("underlay-opacity").value, T !== 0 && (x = e.pstyle("underlay-padding").value));
    var S = Math.max(E, x), D = 0, A = 0;
    if (a && (D = e.pstyle("width").pfValue, A = D / 2), l && r.includeNodes) {
      var k = e.position();
      h = k.x, y = k.y;
      var R = e.outerWidth(), M = R / 2, I = e.outerHeight(), _ = I / 2;
      f = h - M, c = h + M, v = y - _, d = y + _, ir(s, f, v, c, d), a && _v(s, e), a && r.includeOutlines && !i && _v(s, e), a && sx(s, e);
    } else if (u && r.includeEdges)
      if (a && !i) {
        var N = e.pstyle("curve-style").strValue;
        if (f = Math.min(g.srcX, g.midX, g.tgtX), c = Math.max(g.srcX, g.midX, g.tgtX), v = Math.min(g.srcY, g.midY, g.tgtY), d = Math.max(g.srcY, g.midY, g.tgtY), f -= A, c += A, v -= A, d += A, ir(s, f, v, c, d), N === "haystack") {
          var L = g.haystackPts;
          if (L && L.length === 2) {
            if (f = L[0].x, v = L[0].y, c = L[1].x, d = L[1].y, f > c) {
              var F = f;
              f = c, c = F;
            }
            if (v > d) {
              var H = v;
              v = d, d = H;
            }
            ir(s, f - A, v - A, c + A, d + A);
          }
        } else if (N === "bezier" || N === "unbundled-bezier" || Qr(N, "segments") || Qr(N, "taxi")) {
          var V;
          switch (N) {
            case "bezier":
            case "unbundled-bezier":
              V = g.bezierPts;
              break;
            case "segments":
            case "taxi":
            case "round-segments":
            case "round-taxi":
              V = g.linePts;
              break;
          }
          if (V != null)
            for (var O = 0; O < V.length; O++) {
              var $ = V[O];
              f = $.x - A, c = $.x + A, v = $.y - A, d = $.y + A, ir(s, f, v, c, d);
            }
        }
      } else {
        var Q = e.source(), se = Q.position(), ae = e.target(), le = ae.position();
        if (f = se.x, c = le.x, v = se.y, d = le.y, f > c) {
          var ce = f;
          f = c, c = ce;
        }
        if (v > d) {
          var he = v;
          v = d, d = he;
        }
        f -= A, c += A, v -= A, d += A, ir(s, f, v, c, d);
      }
    if (a && r.includeEdges && u && (zi(s, e, "mid-source"), zi(s, e, "mid-target"), zi(s, e, "source"), zi(s, e, "target")), a) {
      var ie = e.pstyle("ghost").value === "yes";
      if (ie) {
        var U = e.pstyle("ghost-offset-x").pfValue, X = e.pstyle("ghost-offset-y").pfValue;
        ir(s, s.x1 + U, s.y1 + X, s.x2 + U, s.y2 + X);
      }
    }
    var C = o.bodyBounds = o.bodyBounds || {};
    Ec(C, s), Ji(C, p), ji(C, 1), a && (f = s.x1, c = s.x2, v = s.y1, d = s.y2, ir(s, f - S, v - S, c + S, d + S));
    var B = o.overlayBounds = o.overlayBounds || {};
    Ec(B, s), Ji(B, p), ji(B, 1);
    var z = o.labelBounds = o.labelBounds || {};
    z.all != null ? qb(z.all) : z.all = zt(), a && r.includeLabels && (r.includeMainLabels && ql(s, e, null), u && (r.includeSourceLabels && ql(s, e, "source"), r.includeTargetLabels && ql(s, e, "target")));
  }
  return s.x1 = jt(s.x1), s.y1 = jt(s.y1), s.x2 = jt(s.x2), s.y2 = jt(s.y2), s.w = jt(s.x2 - s.x1), s.h = jt(s.y2 - s.y1), s.w > 0 && s.h > 0 && b && (Ji(s, p), ji(s, 1)), s;
}, Bg = function(e) {
  var r = 0, n = function(s) {
    return (s ? 1 : 0) << r++;
  }, a = 0;
  return a += n(e.incudeNodes), a += n(e.includeEdges), a += n(e.includeLabels), a += n(e.includeMainLabels), a += n(e.includeSourceLabels), a += n(e.includeTargetLabels), a += n(e.includeOverlays), a += n(e.includeOutlines), a;
}, Rg = function(e) {
  var r = function(o) {
    return Math.round(o);
  };
  if (e.isEdge()) {
    var n = e.source().position(), a = e.target().position();
    return pc([r(n.x), r(n.y), r(a.x), r(a.y)]);
  } else {
    var i = e.position();
    return pc([r(i.x), r(i.y)]);
  }
}, Fv = function(e, r) {
  var n = e._private, a, i = e.isEdge(), s = r == null ? zv : Bg(r), o = s === zv;
  if (n.bbCache == null ? (a = ox(e, oi), n.bbCache = a, n.bbCachePosKey = Rg(e)) : a = n.bbCache, !o) {
    var l = e.isNode();
    a = zt(), (r.includeNodes && l || r.includeEdges && !l) && (r.includeOverlays ? Zr(a, n.overlayBounds) : Zr(a, n.bodyBounds)), r.includeLabels && (r.includeMainLabels && (!i || r.includeSourceLabels && r.includeTargetLabels) ? Zr(a, n.labelBounds.all) : (r.includeMainLabels && Zr(a, n.labelBounds.mainRot), r.includeSourceLabels && Zr(a, n.labelBounds.sourceRot), r.includeTargetLabels && Zr(a, n.labelBounds.targetRot))), a.w = a.x2 - a.x1, a.h = a.y2 - a.y1;
  }
  return a;
}, oi = {
  includeNodes: !0,
  includeEdges: !0,
  includeLabels: !0,
  includeMainLabels: !0,
  includeSourceLabels: !0,
  includeTargetLabels: !0,
  includeOverlays: !0,
  includeUnderlays: !0,
  includeOutlines: !0,
  useCache: !0
}, zv = Bg(oi), Vv = St(oi);
vn.boundingBox = function(t) {
  var e, r = t === void 0 || t.useCache === void 0 || t.useCache === !0, n = ca(function(f) {
    var c = f._private;
    return c.bbCache == null || c.styleDirty || c.bbCachePosKey !== Rg(f);
  }, function(f) {
    return f.id();
  });
  if (r && this.length === 1 && !n(this[0]))
    t === void 0 ? t = oi : t = Vv(t), e = Fv(this[0], t);
  else {
    e = zt(), t = t || oi;
    var a = Vv(t), i = this, s = i.cy(), o = s.styleEnabled();
    this.edges().forEach(n), this.nodes().forEach(n), o && this.recalculateRenderedStyle(r), this.updateCompoundBounds(!r);
    for (var l = 0; l < i.length; l++) {
      var u = i[l];
      n(u) && u.dirtyBoundingBoxCache(), Zr(e, Fv(u, a));
    }
  }
  return e.x1 = jt(e.x1), e.y1 = jt(e.y1), e.x2 = jt(e.x2), e.y2 = jt(e.y2), e.w = jt(e.x2 - e.x1), e.h = jt(e.y2 - e.y1), e;
};
vn.dirtyBoundingBoxCache = function() {
  for (var t = 0; t < this.length; t++) {
    var e = this[t]._private;
    e.bbCache = null, e.bbCachePosKey = null, e.bodyBounds = null, e.overlayBounds = null, e.labelBounds.all = null, e.labelBounds.source = null, e.labelBounds.target = null, e.labelBounds.main = null, e.labelBounds.sourceRot = null, e.labelBounds.targetRot = null, e.labelBounds.mainRot = null, e.arrowBounds.source = null, e.arrowBounds.target = null, e.arrowBounds["mid-source"] = null, e.arrowBounds["mid-target"] = null;
  }
  return this.emitAndNotify("bounds"), this;
};
vn.boundingBoxAt = function(t) {
  var e = this.nodes(), r = this.cy(), n = r.hasCompoundNodes(), a = r.collection();
  if (n && (a = e.filter(function(u) {
    return u.isParent();
  }), e = e.not(a)), _e(t)) {
    var i = t;
    t = function() {
      return i;
    };
  }
  var s = function(f, c) {
    return f._private.bbAtOldPos = t(f, c);
  }, o = function(f) {
    return f._private.bbAtOldPos;
  };
  r.startBatch(), e.forEach(s).silentPositions(t), n && (a.dirtyCompoundBoundsCache(), a.dirtyBoundingBoxCache(), a.updateCompoundBounds(!0));
  var l = Vb(this.boundingBox({
    useCache: !1
  }));
  return e.silentPositions(o), n && (a.dirtyCompoundBoundsCache(), a.dirtyBoundingBoxCache(), a.updateCompoundBounds(!0)), r.endBatch(), l;
};
oa.boundingbox = oa.bb = oa.boundingBox;
oa.renderedBoundingbox = oa.renderedBoundingBox;
var lx = vn, Fa, Ei;
Fa = Ei = {};
var Mg = function(e) {
  e.uppercaseName = tc(e.name), e.autoName = "auto" + e.uppercaseName, e.labelName = "label" + e.uppercaseName, e.outerName = "outer" + e.uppercaseName, e.uppercaseOuterName = tc(e.outerName), Fa[e.name] = function() {
    var n = this[0], a = n._private, i = a.cy, s = i._private.styleEnabled;
    if (n)
      if (s) {
        if (n.isParent())
          return n.updateCompoundBounds(), a[e.autoName] || 0;
        var o = n.pstyle(e.name);
        switch (o.strValue) {
          case "label":
            return n.recalculateRenderedStyle(), a.rstyle[e.labelName] || 0;
          default:
            return o.pfValue;
        }
      } else
        return 1;
  }, Fa["outer" + e.uppercaseName] = function() {
    var n = this[0], a = n._private, i = a.cy, s = i._private.styleEnabled;
    if (n)
      if (s) {
        var o = n[e.name](), l = n.pstyle("border-position").value, u;
        l === "center" ? u = n.pstyle("border-width").pfValue : l === "outside" ? u = 2 * n.pstyle("border-width").pfValue : u = 0;
        var f = 2 * n.padding();
        return o + u + f;
      } else
        return 1;
  }, Fa["rendered" + e.uppercaseName] = function() {
    var n = this[0];
    if (n) {
      var a = n[e.name]();
      return a * this.cy().zoom();
    }
  }, Fa["rendered" + e.uppercaseOuterName] = function() {
    var n = this[0];
    if (n) {
      var a = n[e.outerName]();
      return a * this.cy().zoom();
    }
  };
};
Mg({
  name: "width"
});
Mg({
  name: "height"
});
Ei.padding = function() {
  var t = this[0], e = t._private;
  return t.isParent() ? (t.updateCompoundBounds(), e.autoPadding !== void 0 ? e.autoPadding : t.pstyle("padding").pfValue) : t.pstyle("padding").pfValue;
};
Ei.paddedHeight = function() {
  var t = this[0];
  return t.height() + 2 * t.padding();
};
Ei.paddedWidth = function() {
  var t = this[0];
  return t.width() + 2 * t.padding();
};
var ux = Ei, fx = function(e, r) {
  if (e.isEdge() && e.takesUpSpace())
    return r(e);
}, cx = function(e, r) {
  if (e.isEdge() && e.takesUpSpace()) {
    var n = e.cy();
    return Ks(r(e), n.zoom(), n.pan());
  }
}, vx = function(e, r) {
  if (e.isEdge() && e.takesUpSpace()) {
    var n = e.cy(), a = n.pan(), i = n.zoom();
    return r(e).map(function(s) {
      return Ks(s, i, a);
    });
  }
}, dx = function(e) {
  return e.renderer().getControlPoints(e);
}, hx = function(e) {
  return e.renderer().getSegmentPoints(e);
}, gx = function(e) {
  return e.renderer().getSourceEndpoint(e);
}, px = function(e) {
  return e.renderer().getTargetEndpoint(e);
}, yx = function(e) {
  return e.renderer().getEdgeMidpoint(e);
}, qv = {
  controlPoints: {
    get: dx,
    mult: !0
  },
  segmentPoints: {
    get: hx,
    mult: !0
  },
  sourceEndpoint: {
    get: gx
  },
  targetEndpoint: {
    get: px
  },
  midpoint: {
    get: yx
  }
}, mx = function(e) {
  return "rendered" + e[0].toUpperCase() + e.substr(1);
}, bx = Object.keys(qv).reduce(function(t, e) {
  var r = qv[e], n = mx(e);
  return t[e] = function() {
    return fx(this, r.get);
  }, r.mult ? t[n] = function() {
    return vx(this, r.get);
  } : t[n] = function() {
    return cx(this, r.get);
  }, t;
}, {}), wx = Ae({}, ax, lx, ux, bx);
/*!
Event object based on jQuery events, MIT license

https://jquery.org/license/
https://tldrlegal.com/license/mit-license
https://github.com/jquery/jquery/blob/master/src/event.js
*/
var Lg = function(e, r) {
  this.recycle(e, r);
};
function Ba() {
  return !1;
}
function Vi() {
  return !0;
}
Lg.prototype = {
  instanceString: function() {
    return "event";
  },
  recycle: function(e, r) {
    if (this.isImmediatePropagationStopped = this.isPropagationStopped = this.isDefaultPrevented = Ba, e != null && e.preventDefault ? (this.type = e.type, this.isDefaultPrevented = e.defaultPrevented ? Vi : Ba) : e != null && e.type ? r = e : this.type = e, r != null && (this.originalEvent = r.originalEvent, this.type = r.type != null ? r.type : this.type, this.cy = r.cy, this.target = r.target, this.position = r.position, this.renderedPosition = r.renderedPosition, this.namespace = r.namespace, this.layout = r.layout), this.cy != null && this.position != null && this.renderedPosition == null) {
      var n = this.position, a = this.cy.zoom(), i = this.cy.pan();
      this.renderedPosition = {
        x: n.x * a + i.x,
        y: n.y * a + i.y
      };
    }
    this.timeStamp = e && e.timeStamp || Date.now();
  },
  preventDefault: function() {
    this.isDefaultPrevented = Vi;
    var e = this.originalEvent;
    e && e.preventDefault && e.preventDefault();
  },
  stopPropagation: function() {
    this.isPropagationStopped = Vi;
    var e = this.originalEvent;
    e && e.stopPropagation && e.stopPropagation();
  },
  stopImmediatePropagation: function() {
    this.isImmediatePropagationStopped = Vi, this.stopPropagation();
  },
  isDefaultPrevented: Ba,
  isPropagationStopped: Ba,
  isImmediatePropagationStopped: Ba
};
var Ig = /^([^.]+)(\.(?:[^.]+))?$/, xx = ".*", Og = {
  qualifierCompare: function(e, r) {
    return e === r;
  },
  eventMatches: function() {
    return !0;
  },
  addEventFields: function() {
  },
  callbackContext: function(e) {
    return e;
  },
  beforeEmit: function() {
  },
  afterEmit: function() {
  },
  bubble: function() {
    return !1;
  },
  parent: function() {
    return null;
  },
  context: null
}, $v = Object.keys(Og), Ex = {};
function eo() {
  for (var t = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : Ex, e = arguments.length > 1 ? arguments[1] : void 0, r = 0; r < $v.length; r++) {
    var n = $v[r];
    this[n] = t[n] || Og[n];
  }
  this.context = e || this.context, this.listeners = [], this.emitting = 0;
}
var un = eo.prototype, Ng = function(e, r, n, a, i, s, o) {
  tt(a) && (i = a, a = null), o && (s == null ? s = o : s = Ae({}, s, o));
  for (var l = We(n) ? n : n.split(/\s+/), u = 0; u < l.length; u++) {
    var f = l[u];
    if (!nn(f)) {
      var c = f.match(Ig);
      if (c) {
        var v = c[1], d = c[2] ? c[2] : null, h = r(e, f, v, d, a, i, s);
        if (h === !1)
          break;
      }
    }
  }
}, Hv = function(e, r) {
  return e.addEventFields(e.context, r), new Lg(r.type, r);
}, Cx = function(e, r, n) {
  if (A0(n)) {
    r(e, n);
    return;
  } else if (_e(n)) {
    r(e, Hv(e, n));
    return;
  }
  for (var a = We(n) ? n : n.split(/\s+/), i = 0; i < a.length; i++) {
    var s = a[i];
    if (!nn(s)) {
      var o = s.match(Ig);
      if (o) {
        var l = o[1], u = o[2] ? o[2] : null, f = Hv(e, {
          type: l,
          namespace: u,
          target: e.context
        });
        r(e, f);
      }
    }
  }
};
un.on = un.addListener = function(t, e, r, n, a) {
  return Ng(this, function(i, s, o, l, u, f, c) {
    tt(f) && i.listeners.push({
      event: s,
      // full event string
      callback: f,
      // callback to run
      type: o,
      // the event type (e.g. 'click')
      namespace: l,
      // the event namespace (e.g. ".foo")
      qualifier: u,
      // a restriction on whether to match this emitter
      conf: c
      // additional configuration
    });
  }, t, e, r, n, a), this;
};
un.one = function(t, e, r, n) {
  return this.on(t, e, r, n, {
    one: !0
  });
};
un.removeListener = un.off = function(t, e, r, n) {
  var a = this;
  this.emitting !== 0 && (this.listeners = fb(this.listeners));
  for (var i = this.listeners, s = function(u) {
    var f = i[u];
    Ng(a, function(c, v, d, h, y, g) {
      if ((f.type === d || t === "*") && (!h && f.namespace !== ".*" || f.namespace === h) && (!y || c.qualifierCompare(f.qualifier, y)) && (!g || f.callback === g))
        return i.splice(u, 1), !1;
    }, t, e, r, n);
  }, o = i.length - 1; o >= 0; o--)
    s(o);
  return this;
};
un.removeAllListeners = function() {
  return this.removeListener("*");
};
un.emit = un.trigger = function(t, e, r) {
  var n = this.listeners, a = n.length;
  return this.emitting++, We(e) || (e = [e]), Cx(this, function(i, s) {
    r != null && (n = [{
      event: s.event,
      type: s.type,
      namespace: s.namespace,
      callback: r
    }], a = n.length);
    for (var o = function() {
      var f = n[l];
      if (f.type === s.type && (!f.namespace || f.namespace === s.namespace || f.namespace === xx) && i.eventMatches(i.context, f, s)) {
        var c = [s];
        e != null && vb(c, e), i.beforeEmit(i.context, f, s), f.conf && f.conf.one && (i.listeners = i.listeners.filter(function(h) {
          return h !== f;
        }));
        var v = i.callbackContext(i.context, f, s), d = f.callback.apply(v, c);
        i.afterEmit(i.context, f, s), d === !1 && (s.stopPropagation(), s.preventDefault());
      }
    }, l = 0; l < a; l++)
      o();
    i.bubble(i.context) && !s.isPropagationStopped() && i.parent(i.context).emit(s, e);
  }, t), this.emitting--, this;
};
var Tx = {
  qualifierCompare: function(e, r) {
    return e == null || r == null ? e == null && r == null : e.sameText(r);
  },
  eventMatches: function(e, r, n) {
    var a = r.qualifier;
    return a != null ? e !== n.target && pi(n.target) && a.matches(n.target) : !0;
  },
  addEventFields: function(e, r) {
    r.cy = e.cy(), r.target = e;
  },
  callbackContext: function(e, r, n) {
    return r.qualifier != null ? n.target : e;
  },
  beforeEmit: function(e, r) {
    r.conf && r.conf.once && r.conf.onceCollection.removeListener(r.event, r.qualifier, r.callback);
  },
  bubble: function() {
    return !0;
  },
  parent: function(e) {
    return e.isChild() ? e.parent() : e.cy();
  }
}, qi = function(e) {
  return Se(e) ? new on(e) : e;
}, _g = {
  createEmitter: function() {
    for (var e = 0; e < this.length; e++) {
      var r = this[e], n = r._private;
      n.emitter || (n.emitter = new eo(Tx, r));
    }
    return this;
  },
  emitter: function() {
    return this._private.emitter;
  },
  on: function(e, r, n) {
    for (var a = qi(r), i = 0; i < this.length; i++) {
      var s = this[i];
      s.emitter().on(e, a, n);
    }
    return this;
  },
  removeListener: function(e, r, n) {
    for (var a = qi(r), i = 0; i < this.length; i++) {
      var s = this[i];
      s.emitter().removeListener(e, a, n);
    }
    return this;
  },
  removeAllListeners: function() {
    for (var e = 0; e < this.length; e++) {
      var r = this[e];
      r.emitter().removeAllListeners();
    }
    return this;
  },
  one: function(e, r, n) {
    for (var a = qi(r), i = 0; i < this.length; i++) {
      var s = this[i];
      s.emitter().one(e, a, n);
    }
    return this;
  },
  once: function(e, r, n) {
    for (var a = qi(r), i = 0; i < this.length; i++) {
      var s = this[i];
      s.emitter().on(e, a, n, {
        once: !0,
        onceCollection: this
      });
    }
  },
  emit: function(e, r) {
    for (var n = 0; n < this.length; n++) {
      var a = this[n];
      a.emitter().emit(e, r);
    }
    return this;
  },
  emitAndNotify: function(e, r) {
    if (this.length !== 0)
      return this.cy().notify(e, this), this.emit(e, r), this;
  }
};
qe.eventAliasesOn(_g);
var Fg = {
  nodes: function(e) {
    return this.filter(function(r) {
      return r.isNode();
    }).filter(e);
  },
  edges: function(e) {
    return this.filter(function(r) {
      return r.isEdge();
    }).filter(e);
  },
  // internal helper to get nodes and edges as separate collections with single iteration over elements
  byGroup: function() {
    for (var e = this.spawn(), r = this.spawn(), n = 0; n < this.length; n++) {
      var a = this[n];
      a.isNode() ? e.push(a) : r.push(a);
    }
    return {
      nodes: e,
      edges: r
    };
  },
  filter: function(e, r) {
    if (e === void 0)
      return this;
    if (Se(e) || Xt(e))
      return new on(e).filter(this);
    if (tt(e)) {
      for (var n = this.spawn(), a = this, i = 0; i < a.length; i++) {
        var s = a[i], o = r ? e.apply(r, [s, i, a]) : e(s, i, a);
        o && n.push(s);
      }
      return n;
    }
    return this.spawn();
  },
  not: function(e) {
    if (e) {
      Se(e) && (e = this.filter(e));
      for (var r = this.spawn(), n = 0; n < this.length; n++) {
        var a = this[n], i = e.has(a);
        i || r.push(a);
      }
      return r;
    } else
      return this;
  },
  absoluteComplement: function() {
    var e = this.cy();
    return e.mutableElements().not(this);
  },
  intersect: function(e) {
    if (Se(e)) {
      var r = e;
      return this.filter(r);
    }
    for (var n = this.spawn(), a = this, i = e, s = this.length < e.length, o = s ? a : i, l = s ? i : a, u = 0; u < o.length; u++) {
      var f = o[u];
      l.has(f) && n.push(f);
    }
    return n;
  },
  xor: function(e) {
    var r = this._private.cy;
    Se(e) && (e = r.$(e));
    var n = this.spawn(), a = this, i = e, s = function(l, u) {
      for (var f = 0; f < l.length; f++) {
        var c = l[f], v = c._private.data.id, d = u.hasElementWithId(v);
        d || n.push(c);
      }
    };
    return s(a, i), s(i, a), n;
  },
  diff: function(e) {
    var r = this._private.cy;
    Se(e) && (e = r.$(e));
    var n = this.spawn(), a = this.spawn(), i = this.spawn(), s = this, o = e, l = function(f, c, v) {
      for (var d = 0; d < f.length; d++) {
        var h = f[d], y = h._private.data.id, g = c.hasElementWithId(y);
        g ? i.merge(h) : v.push(h);
      }
    };
    return l(s, o, n), l(o, s, a), {
      left: n,
      right: a,
      both: i
    };
  },
  add: function(e) {
    var r = this._private.cy;
    if (!e)
      return this;
    if (Se(e)) {
      var n = e;
      e = r.mutableElements().filter(n);
    }
    for (var a = this.spawnSelf(), i = 0; i < e.length; i++) {
      var s = e[i], o = !this.has(s);
      o && a.push(s);
    }
    return a;
  },
  // in place merge on calling collection
  merge: function(e) {
    var r = this._private, n = r.cy;
    if (!e)
      return this;
    if (e && Se(e)) {
      var a = e;
      e = n.mutableElements().filter(a);
    }
    for (var i = r.map, s = 0; s < e.length; s++) {
      var o = e[s], l = o._private.data.id, u = !i.has(l);
      if (u) {
        var f = this.length++;
        this[f] = o, i.set(l, {
          ele: o,
          index: f
        });
      }
    }
    return this;
  },
  unmergeAt: function(e) {
    var r = this[e], n = r.id(), a = this._private, i = a.map;
    this[e] = void 0, i.delete(n);
    var s = e === this.length - 1;
    if (this.length > 1 && !s) {
      var o = this.length - 1, l = this[o], u = l._private.data.id;
      this[o] = void 0, this[e] = l, i.set(u, {
        ele: l,
        index: e
      });
    }
    return this.length--, this;
  },
  // remove single ele in place in calling collection
  unmergeOne: function(e) {
    e = e[0];
    var r = this._private, n = e._private.data.id, a = r.map, i = a.get(n);
    if (!i)
      return this;
    var s = i.index;
    return this.unmergeAt(s), this;
  },
  // remove eles in place on calling collection
  unmerge: function(e) {
    var r = this._private.cy;
    if (!e)
      return this;
    if (e && Se(e)) {
      var n = e;
      e = r.mutableElements().filter(n);
    }
    for (var a = 0; a < e.length; a++)
      this.unmergeOne(e[a]);
    return this;
  },
  unmergeBy: function(e) {
    for (var r = this.length - 1; r >= 0; r--) {
      var n = this[r];
      e(n) && this.unmergeAt(r);
    }
    return this;
  },
  map: function(e, r) {
    for (var n = [], a = this, i = 0; i < a.length; i++) {
      var s = a[i], o = r ? e.apply(r, [s, i, a]) : e(s, i, a);
      n.push(o);
    }
    return n;
  },
  reduce: function(e, r) {
    for (var n = r, a = this, i = 0; i < a.length; i++)
      n = e(n, a[i], i, a);
    return n;
  },
  max: function(e, r) {
    for (var n = -1 / 0, a, i = this, s = 0; s < i.length; s++) {
      var o = i[s], l = r ? e.apply(r, [o, s, i]) : e(o, s, i);
      l > n && (n = l, a = o);
    }
    return {
      value: n,
      ele: a
    };
  },
  min: function(e, r) {
    for (var n = 1 / 0, a, i = this, s = 0; s < i.length; s++) {
      var o = i[s], l = r ? e.apply(r, [o, s, i]) : e(o, s, i);
      l < n && (n = l, a = o);
    }
    return {
      value: n,
      ele: a
    };
  }
}, Fe = Fg;
Fe.u = Fe["|"] = Fe["+"] = Fe.union = Fe.or = Fe.add;
Fe["\\"] = Fe["!"] = Fe["-"] = Fe.difference = Fe.relativeComplement = Fe.subtract = Fe.not;
Fe.n = Fe["&"] = Fe["."] = Fe.and = Fe.intersection = Fe.intersect;
Fe["^"] = Fe["(+)"] = Fe["(-)"] = Fe.symmetricDifference = Fe.symdiff = Fe.xor;
Fe.fnFilter = Fe.filterFn = Fe.stdFilter = Fe.filter;
Fe.complement = Fe.abscomp = Fe.absoluteComplement;
var Sx = {
  isNode: function() {
    return this.group() === "nodes";
  },
  isEdge: function() {
    return this.group() === "edges";
  },
  isLoop: function() {
    return this.isEdge() && this.source()[0] === this.target()[0];
  },
  isSimple: function() {
    return this.isEdge() && this.source()[0] !== this.target()[0];
  },
  group: function() {
    var e = this[0];
    if (e)
      return e._private.group;
  }
}, zg = function(e, r) {
  var n = e.cy(), a = n.hasCompoundNodes();
  function i(f) {
    var c = f.pstyle("z-compound-depth");
    return c.value === "auto" ? a ? f.zDepth() : 0 : c.value === "bottom" ? -1 : c.value === "top" ? Xu : 0;
  }
  var s = i(e) - i(r);
  if (s !== 0)
    return s;
  function o(f) {
    var c = f.pstyle("z-index-compare");
    return c.value === "auto" && f.isNode() ? 1 : 0;
  }
  var l = o(e) - o(r);
  if (l !== 0)
    return l;
  var u = e.pstyle("z-index").value - r.pstyle("z-index").value;
  return u !== 0 ? u : e.poolIndex() - r.poolIndex();
}, Ts = {
  forEach: function(e, r) {
    if (tt(e))
      for (var n = this.length, a = 0; a < n; a++) {
        var i = this[a], s = r ? e.apply(r, [i, a, this]) : e(i, a, this);
        if (s === !1)
          break;
      }
    return this;
  },
  toArray: function() {
    for (var e = [], r = 0; r < this.length; r++)
      e.push(this[r]);
    return e;
  },
  slice: function(e, r) {
    var n = [], a = this.length;
    r == null && (r = a), e == null && (e = 0), e < 0 && (e = a + e), r < 0 && (r = a + r);
    for (var i = e; i >= 0 && i < r && i < a; i++)
      n.push(this[i]);
    return this.spawn(n);
  },
  size: function() {
    return this.length;
  },
  eq: function(e) {
    return this[e] || this.spawn();
  },
  first: function() {
    return this[0] || this.spawn();
  },
  last: function() {
    return this[this.length - 1] || this.spawn();
  },
  empty: function() {
    return this.length === 0;
  },
  nonempty: function() {
    return !this.empty();
  },
  sort: function(e) {
    if (!tt(e))
      return this;
    var r = this.toArray().sort(e);
    return this.spawn(r);
  },
  sortByZIndex: function() {
    return this.sort(zg);
  },
  zDepth: function() {
    var e = this[0];
    if (e) {
      var r = e._private, n = r.group;
      if (n === "nodes") {
        var a = r.data.parent ? e.parents().size() : 0;
        return e.isParent() ? a : Xu - 1;
      } else {
        var i = r.source, s = r.target, o = i.zDepth(), l = s.zDepth();
        return Math.max(o, l, 0);
      }
    }
  }
};
Ts.each = Ts.forEach;
var Px = function() {
  var e = "undefined", r = (typeof Symbol > "u" ? "undefined" : dt(Symbol)) != e && dt(Symbol.iterator) != e;
  r && (Ts[Symbol.iterator] = function() {
    var n = this, a = {
      value: void 0,
      done: !1
    }, i = 0, s = this.length;
    return _h({
      next: function() {
        return i < s ? a.value = n[i++] : (a.value = void 0, a.done = !0), a;
      }
    }, Symbol.iterator, function() {
      return this;
    });
  });
};
Px();
var Dx = St({
  nodeDimensionsIncludeLabels: !1
}), ts = {
  // Calculates and returns node dimensions { x, y } based on options given
  layoutDimensions: function(e) {
    e = Dx(e);
    var r;
    if (!this.takesUpSpace())
      r = {
        w: 0,
        h: 0
      };
    else if (e.nodeDimensionsIncludeLabels) {
      var n = this.boundingBox();
      r = {
        w: n.w,
        h: n.h
      };
    } else
      r = {
        w: this.outerWidth(),
        h: this.outerHeight()
      };
    return (r.w === 0 || r.h === 0) && (r.w = r.h = 1), r;
  },
  // using standard layout options, apply position function (w/ or w/o animation)
  layoutPositions: function(e, r, n) {
    var a = this.nodes().filter(function(E) {
      return !E.isParent();
    }), i = this.cy(), s = r.eles, o = function(T) {
      return T.id();
    }, l = ca(n, o);
    e.emit({
      type: "layoutstart",
      layout: e
    }), e.animations = [];
    var u = function(T, x, S) {
      var D = {
        x: x.x1 + x.w / 2,
        y: x.y1 + x.h / 2
      }, A = {
        // scale from center of bounding box (not necessarily 0,0)
        x: (S.x - D.x) * T,
        y: (S.y - D.y) * T
      };
      return {
        x: D.x + A.x,
        y: D.y + A.y
      };
    }, f = r.spacingFactor && r.spacingFactor !== 1, c = function() {
      if (!f)
        return null;
      for (var T = zt(), x = 0; x < a.length; x++) {
        var S = a[x], D = l(S, x);
        ng(T, D.x, D.y);
      }
      return T;
    }, v = c(), d = ca(function(E, T) {
      var x = l(E, T);
      if (f) {
        var S = Math.abs(r.spacingFactor);
        x = u(S, v, x);
      }
      return r.transform != null && (x = r.transform(E, x)), x;
    }, o);
    if (r.animate) {
      for (var h = 0; h < a.length; h++) {
        var y = a[h], g = d(y, h), p = r.animateFilter == null || r.animateFilter(y, h);
        if (p) {
          var m = y.animation({
            position: g,
            duration: r.animationDuration,
            easing: r.animationEasing
          });
          e.animations.push(m);
        } else
          y.position(g);
      }
      if (r.fit) {
        var b = i.animation({
          fit: {
            boundingBox: s.boundingBoxAt(d),
            padding: r.padding
          },
          duration: r.animationDuration,
          easing: r.animationEasing
        });
        e.animations.push(b);
      } else if (r.zoom !== void 0 && r.pan !== void 0) {
        var w = i.animation({
          zoom: r.zoom,
          pan: r.pan,
          duration: r.animationDuration,
          easing: r.animationEasing
        });
        e.animations.push(w);
      }
      e.animations.forEach(function(E) {
        return E.play();
      }), e.one("layoutready", r.ready), e.emit({
        type: "layoutready",
        layout: e
      }), ya.all(e.animations.map(function(E) {
        return E.promise();
      })).then(function() {
        e.one("layoutstop", r.stop), e.emit({
          type: "layoutstop",
          layout: e
        });
      });
    } else
      a.positions(d), r.fit && i.fit(r.eles, r.padding), r.zoom != null && i.zoom(r.zoom), r.pan && i.pan(r.pan), e.one("layoutready", r.ready), e.emit({
        type: "layoutready",
        layout: e
      }), e.one("layoutstop", r.stop), e.emit({
        type: "layoutstop",
        layout: e
      });
    return this;
  },
  layout: function(e) {
    var r = this.cy();
    return r.makeLayout(Ae({}, e, {
      eles: this
    }));
  }
};
ts.createLayout = ts.makeLayout = ts.layout;
function Vg(t, e, r) {
  var n = r._private, a = n.styleCache = n.styleCache || [], i;
  return (i = a[t]) != null || (i = a[t] = e(r)), i;
}
function to(t, e) {
  return t = Bn(t), function(n) {
    return Vg(t, e, n);
  };
}
function ro(t, e) {
  t = Bn(t);
  var r = function(a) {
    return e.call(a);
  };
  return function() {
    var a = this[0];
    if (a)
      return Vg(t, r, a);
  };
}
var Et = {
  recalculateRenderedStyle: function(e) {
    var r = this.cy(), n = r.renderer(), a = r.styleEnabled();
    return n && a && n.recalculateRenderedStyle(this, e), this;
  },
  dirtyStyleCache: function() {
    var e = this.cy(), r = function(i) {
      return i._private.styleCache = null;
    };
    if (e.hasCompoundNodes()) {
      var n;
      n = this.spawnSelf().merge(this.descendants()).merge(this.parents()), n.merge(n.connectedEdges()), n.forEach(r);
    } else
      this.forEach(function(a) {
        r(a), a.connectedEdges().forEach(r);
      });
    return this;
  },
  // fully updates (recalculates) the style for the elements
  updateStyle: function(e) {
    var r = this._private.cy;
    if (!r.styleEnabled())
      return this;
    if (r.batching()) {
      var n = r._private.batchStyleEles;
      return n.merge(this), this;
    }
    var a = r.hasCompoundNodes(), i = this;
    e = !!(e || e === void 0), a && (i = this.spawnSelf().merge(this.descendants()).merge(this.parents()));
    var s = i;
    return e ? s.emitAndNotify("style") : s.emit("style"), i.forEach(function(o) {
      return o._private.styleDirty = !0;
    }), this;
  },
  // private: clears dirty flag and recalculates style
  cleanStyle: function() {
    var e = this.cy();
    if (e.styleEnabled())
      for (var r = 0; r < this.length; r++) {
        var n = this[r];
        n._private.styleDirty && (n._private.styleDirty = !1, e.style().apply(n));
      }
  },
  // get the internal parsed style object for the specified property
  parsedStyle: function(e) {
    var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0, n = this[0], a = n.cy();
    if (a.styleEnabled() && n) {
      n._private.styleDirty && (n._private.styleDirty = !1, a.style().apply(n));
      var i = n._private.style[e];
      return i ?? (r ? a.style().getDefaultProperty(e) : null);
    }
  },
  numericStyle: function(e) {
    var r = this[0];
    if (r.cy().styleEnabled() && r) {
      var n = r.pstyle(e);
      return n.pfValue !== void 0 ? n.pfValue : n.value;
    }
  },
  numericStyleUnits: function(e) {
    var r = this[0];
    if (r.cy().styleEnabled() && r)
      return r.pstyle(e).units;
  },
  // get the specified css property as a rendered value (i.e. on-screen value)
  // or get the whole rendered style if no property specified (NB doesn't allow setting)
  renderedStyle: function(e) {
    var r = this.cy();
    if (!r.styleEnabled())
      return this;
    var n = this[0];
    if (n)
      return r.style().getRenderedStyle(n, e);
  },
  // read the calculated css style of the element or override the style (via a bypass)
  style: function(e, r) {
    var n = this.cy();
    if (!n.styleEnabled())
      return this;
    var a = !1, i = n.style();
    if (_e(e)) {
      var s = e;
      i.applyBypass(this, s, a), this.emitAndNotify("style");
    } else if (Se(e))
      if (r === void 0) {
        var o = this[0];
        return o ? i.getStylePropertyValue(o, e) : void 0;
      } else
        i.applyBypass(this, e, r, a), this.emitAndNotify("style");
    else if (e === void 0) {
      var l = this[0];
      return l ? i.getRawStyle(l) : void 0;
    }
    return this;
  },
  removeStyle: function(e) {
    var r = this.cy();
    if (!r.styleEnabled())
      return this;
    var n = !1, a = r.style(), i = this;
    if (e === void 0)
      for (var s = 0; s < i.length; s++) {
        var o = i[s];
        a.removeAllBypasses(o, n);
      }
    else {
      e = e.split(/\s+/);
      for (var l = 0; l < i.length; l++) {
        var u = i[l];
        a.removeBypasses(u, e, n);
      }
    }
    return this.emitAndNotify("style"), this;
  },
  show: function() {
    return this.css("display", "element"), this;
  },
  hide: function() {
    return this.css("display", "none"), this;
  },
  effectiveOpacity: function() {
    var e = this.cy();
    if (!e.styleEnabled())
      return 1;
    var r = e.hasCompoundNodes(), n = this[0];
    if (n) {
      var a = n._private, i = n.pstyle("opacity").value;
      if (!r)
        return i;
      var s = a.data.parent ? n.parents() : null;
      if (s)
        for (var o = 0; o < s.length; o++) {
          var l = s[o], u = l.pstyle("opacity").value;
          i = u * i;
        }
      return i;
    }
  },
  transparent: function() {
    var e = this.cy();
    if (!e.styleEnabled())
      return !1;
    var r = this[0], n = r.cy().hasCompoundNodes();
    if (r)
      return n ? r.effectiveOpacity() === 0 : r.pstyle("opacity").value === 0;
  },
  backgrounding: function() {
    var e = this.cy();
    if (!e.styleEnabled())
      return !1;
    var r = this[0];
    return !!r._private.backgrounding;
  }
};
function $l(t, e) {
  var r = t._private, n = r.data.parent ? t.parents() : null;
  if (n)
    for (var a = 0; a < n.length; a++) {
      var i = n[a];
      if (!e(i))
        return !1;
    }
  return !0;
}
function lf(t) {
  var e = t.ok, r = t.edgeOkViaNode || t.ok, n = t.parentOk || t.ok;
  return function() {
    var a = this.cy();
    if (!a.styleEnabled())
      return !0;
    var i = this[0], s = a.hasCompoundNodes();
    if (i) {
      var o = i._private;
      if (!e(i))
        return !1;
      if (i.isNode())
        return !s || $l(i, n);
      var l = o.source, u = o.target;
      return r(l) && (!s || $l(l, r)) && (l === u || r(u) && (!s || $l(u, r)));
    }
  };
}
var ma = to("eleTakesUpSpace", function(t) {
  return t.pstyle("display").value === "element" && t.width() !== 0 && (t.isNode() ? t.height() !== 0 : !0);
});
Et.takesUpSpace = ro("takesUpSpace", lf({
  ok: ma
}));
var Ax = to("eleInteractive", function(t) {
  return t.pstyle("events").value === "yes" && t.pstyle("visibility").value === "visible" && ma(t);
}), kx = to("parentInteractive", function(t) {
  return t.pstyle("visibility").value === "visible" && ma(t);
});
Et.interactive = ro("interactive", lf({
  ok: Ax,
  parentOk: kx,
  edgeOkViaNode: ma
}));
Et.noninteractive = function() {
  var t = this[0];
  if (t)
    return !t.interactive();
};
var Bx = to("eleVisible", function(t) {
  return t.pstyle("visibility").value === "visible" && t.pstyle("opacity").pfValue !== 0 && ma(t);
}), Rx = ma;
Et.visible = ro("visible", lf({
  ok: Bx,
  edgeOkViaNode: Rx
}));
Et.hidden = function() {
  var t = this[0];
  if (t)
    return !t.visible();
};
Et.isBundledBezier = ro("isBundledBezier", function() {
  return this.cy().styleEnabled() ? !this.removed() && this.pstyle("curve-style").value === "bezier" && this.takesUpSpace() : !1;
});
Et.bypass = Et.css = Et.style;
Et.renderedCss = Et.renderedStyle;
Et.removeBypass = Et.removeCss = Et.removeStyle;
Et.pstyle = Et.parsedStyle;
var tn = {};
function Uv(t) {
  return function() {
    var e = arguments, r = [];
    if (e.length === 2) {
      var n = e[0], a = e[1];
      this.on(t.event, n, a);
    } else if (e.length === 1 && tt(e[0])) {
      var i = e[0];
      this.on(t.event, i);
    } else if (e.length === 0 || e.length === 1 && We(e[0])) {
      for (var s = e.length === 1 ? e[0] : null, o = 0; o < this.length; o++) {
        var l = this[o], u = !t.ableField || l._private[t.ableField], f = l._private[t.field] != t.value;
        if (t.overrideAble) {
          var c = t.overrideAble(l);
          if (c !== void 0 && (u = c, !c))
            return this;
        }
        u && (l._private[t.field] = t.value, f && r.push(l));
      }
      var v = this.spawn(r);
      v.updateStyle(), v.emit(t.event), s && v.emit(s);
    }
    return this;
  };
}
function ba(t) {
  tn[t.field] = function() {
    var e = this[0];
    if (e) {
      if (t.overrideField) {
        var r = t.overrideField(e);
        if (r !== void 0)
          return r;
      }
      return e._private[t.field];
    }
  }, tn[t.on] = Uv({
    event: t.on,
    field: t.field,
    ableField: t.ableField,
    overrideAble: t.overrideAble,
    value: !0
  }), tn[t.off] = Uv({
    event: t.off,
    field: t.field,
    ableField: t.ableField,
    overrideAble: t.overrideAble,
    value: !1
  });
}
ba({
  field: "locked",
  overrideField: function(e) {
    return e.cy().autolock() ? !0 : void 0;
  },
  on: "lock",
  off: "unlock"
});
ba({
  field: "grabbable",
  overrideField: function(e) {
    return e.cy().autoungrabify() || e.pannable() ? !1 : void 0;
  },
  on: "grabify",
  off: "ungrabify"
});
ba({
  field: "selected",
  ableField: "selectable",
  overrideAble: function(e) {
    return e.cy().autounselectify() ? !1 : void 0;
  },
  on: "select",
  off: "unselect"
});
ba({
  field: "selectable",
  overrideField: function(e) {
    return e.cy().autounselectify() ? !1 : void 0;
  },
  on: "selectify",
  off: "unselectify"
});
tn.deselect = tn.unselect;
tn.grabbed = function() {
  var t = this[0];
  if (t)
    return t._private.grabbed;
};
ba({
  field: "active",
  on: "activate",
  off: "unactivate"
});
ba({
  field: "pannable",
  on: "panify",
  off: "unpanify"
});
tn.inactive = function() {
  var t = this[0];
  if (t)
    return !t._private.active;
};
var Rt = {}, Gv = function(e) {
  return function(n) {
    for (var a = this, i = [], s = 0; s < a.length; s++) {
      var o = a[s];
      if (o.isNode()) {
        for (var l = !1, u = o.connectedEdges(), f = 0; f < u.length; f++) {
          var c = u[f], v = c.source(), d = c.target();
          if (e.noIncomingEdges && d === o && v !== o || e.noOutgoingEdges && v === o && d !== o) {
            l = !0;
            break;
          }
        }
        l || i.push(o);
      }
    }
    return this.spawn(i, !0).filter(n);
  };
}, Wv = function(e) {
  return function(r) {
    for (var n = this, a = [], i = 0; i < n.length; i++) {
      var s = n[i];
      if (s.isNode())
        for (var o = s.connectedEdges(), l = 0; l < o.length; l++) {
          var u = o[l], f = u.source(), c = u.target();
          e.outgoing && f === s ? (a.push(u), a.push(c)) : e.incoming && c === s && (a.push(u), a.push(f));
        }
    }
    return this.spawn(a, !0).filter(r);
  };
}, Kv = function(e) {
  return function(r) {
    for (var n = this, a = [], i = {}; ; ) {
      var s = e.outgoing ? n.outgoers() : n.incomers();
      if (s.length === 0)
        break;
      for (var o = !1, l = 0; l < s.length; l++) {
        var u = s[l], f = u.id();
        i[f] || (i[f] = !0, a.push(u), o = !0);
      }
      if (!o)
        break;
      n = s;
    }
    return this.spawn(a, !0).filter(r);
  };
};
Rt.clearTraversalCache = function() {
  for (var t = 0; t < this.length; t++)
    this[t]._private.traversalCache = null;
};
Ae(Rt, {
  // get the root nodes in the DAG
  roots: Gv({
    noIncomingEdges: !0
  }),
  // get the leaf nodes in the DAG
  leaves: Gv({
    noOutgoingEdges: !0
  }),
  // normally called children in graph theory
  // these nodes =edges=> outgoing nodes
  outgoers: Jt(Wv({
    outgoing: !0
  }), "outgoers"),
  // aka DAG descendants
  successors: Kv({
    outgoing: !0
  }),
  // normally called parents in graph theory
  // these nodes <=edges= incoming nodes
  incomers: Jt(Wv({
    incoming: !0
  }), "incomers"),
  // aka DAG ancestors
  predecessors: Kv({})
});
Ae(Rt, {
  neighborhood: Jt(function(t) {
    for (var e = [], r = this.nodes(), n = 0; n < r.length; n++)
      for (var a = r[n], i = a.connectedEdges(), s = 0; s < i.length; s++) {
        var o = i[s], l = o.source(), u = o.target(), f = a === l ? u : l;
        f.length > 0 && e.push(f[0]), e.push(o[0]);
      }
    return this.spawn(e, !0).filter(t);
  }, "neighborhood"),
  closedNeighborhood: function(e) {
    return this.neighborhood().add(this).filter(e);
  },
  openNeighborhood: function(e) {
    return this.neighborhood(e);
  }
});
Rt.neighbourhood = Rt.neighborhood;
Rt.closedNeighbourhood = Rt.closedNeighborhood;
Rt.openNeighbourhood = Rt.openNeighborhood;
Ae(Rt, {
  source: Jt(function(e) {
    var r = this[0], n;
    return r && (n = r._private.source || r.cy().collection()), n && e ? n.filter(e) : n;
  }, "source"),
  target: Jt(function(e) {
    var r = this[0], n;
    return r && (n = r._private.target || r.cy().collection()), n && e ? n.filter(e) : n;
  }, "target"),
  sources: Yv({
    attr: "source"
  }),
  targets: Yv({
    attr: "target"
  })
});
function Yv(t) {
  return function(r) {
    for (var n = [], a = 0; a < this.length; a++) {
      var i = this[a], s = i._private[t.attr];
      s && n.push(s);
    }
    return this.spawn(n, !0).filter(r);
  };
}
Ae(Rt, {
  edgesWith: Jt(Xv(), "edgesWith"),
  edgesTo: Jt(Xv({
    thisIsSrc: !0
  }), "edgesTo")
});
function Xv(t) {
  return function(r) {
    var n = [], a = this._private.cy, i = t || {};
    Se(r) && (r = a.$(r));
    for (var s = 0; s < r.length; s++)
      for (var o = r[s]._private.edges, l = 0; l < o.length; l++) {
        var u = o[l], f = u._private.data, c = this.hasElementWithId(f.source) && r.hasElementWithId(f.target), v = r.hasElementWithId(f.source) && this.hasElementWithId(f.target), d = c || v;
        d && ((i.thisIsSrc || i.thisIsTgt) && (i.thisIsSrc && !c || i.thisIsTgt && !v) || n.push(u));
      }
    return this.spawn(n, !0);
  };
}
Ae(Rt, {
  connectedEdges: Jt(function(t) {
    for (var e = [], r = this, n = 0; n < r.length; n++) {
      var a = r[n];
      if (a.isNode())
        for (var i = a._private.edges, s = 0; s < i.length; s++) {
          var o = i[s];
          e.push(o);
        }
    }
    return this.spawn(e, !0).filter(t);
  }, "connectedEdges"),
  connectedNodes: Jt(function(t) {
    for (var e = [], r = this, n = 0; n < r.length; n++) {
      var a = r[n];
      a.isEdge() && (e.push(a.source()[0]), e.push(a.target()[0]));
    }
    return this.spawn(e, !0).filter(t);
  }, "connectedNodes"),
  parallelEdges: Jt(Zv(), "parallelEdges"),
  codirectedEdges: Jt(Zv({
    codirected: !0
  }), "codirectedEdges")
});
function Zv(t) {
  var e = {
    codirected: !1
  };
  return t = Ae({}, e, t), function(n) {
    for (var a = [], i = this.edges(), s = t, o = 0; o < i.length; o++)
      for (var l = i[o], u = l._private, f = u.source, c = f._private.data.id, v = u.data.target, d = f._private.edges, h = 0; h < d.length; h++) {
        var y = d[h], g = y._private.data, p = g.target, m = g.source, b = p === v && m === c, w = c === p && v === m;
        (s.codirected && b || !s.codirected && (b || w)) && a.push(y);
      }
    return this.spawn(a, !0).filter(n);
  };
}
Ae(Rt, {
  components: function(e) {
    var r = this, n = r.cy(), a = n.collection(), i = e == null ? r.nodes() : e.nodes(), s = [];
    e != null && i.empty() && (i = e.sources());
    var o = function(f, c) {
      a.merge(f), i.unmerge(f), c.merge(f);
    };
    if (i.empty())
      return r.spawn();
    var l = function() {
      var f = n.collection();
      s.push(f);
      var c = i[0];
      o(c, f), r.bfs({
        directed: !1,
        roots: c,
        visit: function(d) {
          return o(d, f);
        }
      }), f.forEach(function(v) {
        v.connectedEdges().forEach(function(d) {
          r.has(d) && f.has(d.source()) && f.has(d.target()) && f.merge(d);
        });
      });
    };
    do
      l();
    while (i.length > 0);
    return s;
  },
  component: function() {
    var e = this[0];
    return e.cy().mutableElements().components(e)[0];
  }
});
Rt.componentsOf = Rt.components;
var Ct = function(e, r) {
  var n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : !1, a = arguments.length > 3 && arguments[3] !== void 0 ? arguments[3] : !1;
  if (e === void 0) {
    je("A collection must have a reference to the core");
    return;
  }
  var i = new Nr(), s = !1;
  if (!r)
    r = [];
  else if (r.length > 0 && _e(r[0]) && !pi(r[0])) {
    s = !0;
    for (var o = [], l = new pa(), u = 0, f = r.length; u < f; u++) {
      var c = r[u];
      c.data == null && (c.data = {});
      var v = c.data;
      if (v.id == null)
        v.id = eg();
      else if (e.hasElementWithId(v.id) || l.has(v.id))
        continue;
      var d = new Ws(e, c, !1);
      o.push(d), l.add(v.id);
    }
    r = o;
  }
  this.length = 0;
  for (var h = 0, y = r.length; h < y; h++) {
    var g = r[h][0];
    if (g != null) {
      var p = g._private.data.id;
      (!n || !i.has(p)) && (n && i.set(p, {
        index: this.length,
        ele: g
      }), this[this.length] = g, this.length++);
    }
  }
  this._private = {
    eles: this,
    cy: e,
    get map() {
      return this.lazyMap == null && this.rebuildMap(), this.lazyMap;
    },
    set map(m) {
      this.lazyMap = m;
    },
    rebuildMap: function() {
      for (var b = this.lazyMap = new Nr(), w = this.eles, E = 0; E < w.length; E++) {
        var T = w[E];
        b.set(T.id(), {
          index: E,
          ele: T
        });
      }
    }
  }, n && (this._private.map = i), s && !a && this.restore();
}, Xe = Ws.prototype = Ct.prototype = Object.create(Array.prototype);
Xe.instanceString = function() {
  return "collection";
};
Xe.spawn = function(t, e) {
  return new Ct(this.cy(), t, e);
};
Xe.spawnSelf = function() {
  return this.spawn(this);
};
Xe.cy = function() {
  return this._private.cy;
};
Xe.renderer = function() {
  return this._private.cy.renderer();
};
Xe.element = function() {
  return this[0];
};
Xe.collection = function() {
  return Vh(this) ? this : new Ct(this._private.cy, [this]);
};
Xe.unique = function() {
  return new Ct(this._private.cy, this, !0);
};
Xe.hasElementWithId = function(t) {
  return t = "" + t, this._private.map.has(t);
};
Xe.getElementById = function(t) {
  t = "" + t;
  var e = this._private.cy, r = this._private.map.get(t);
  return r ? r.ele : new Ct(e);
};
Xe.$id = Xe.getElementById;
Xe.poolIndex = function() {
  var t = this._private.cy, e = t._private.elements, r = this[0]._private.data.id;
  return e._private.map.get(r).index;
};
Xe.indexOf = function(t) {
  var e = t[0]._private.data.id;
  return this._private.map.get(e).index;
};
Xe.indexOfId = function(t) {
  return t = "" + t, this._private.map.get(t).index;
};
Xe.json = function(t) {
  var e = this.element(), r = this.cy();
  if (e == null && t)
    return this;
  if (e != null) {
    var n = e._private;
    if (_e(t)) {
      if (r.startBatch(), t.data) {
        e.data(t.data);
        var a = n.data;
        if (e.isEdge()) {
          var i = !1, s = {}, o = t.data.source, l = t.data.target;
          o != null && o != a.source && (s.source = "" + o, i = !0), l != null && l != a.target && (s.target = "" + l, i = !0), i && (e = e.move(s));
        } else {
          var u = "parent" in t.data, f = t.data.parent;
          u && (f != null || a.parent != null) && f != a.parent && (f === void 0 && (f = null), f != null && (f = "" + f), e = e.move({
            parent: f
          }));
        }
      }
      t.position && e.position(t.position);
      var c = function(y, g, p) {
        var m = t[y];
        m != null && m !== n[y] && (m ? e[g]() : e[p]());
      };
      return c("removed", "remove", "restore"), c("selected", "select", "unselect"), c("selectable", "selectify", "unselectify"), c("locked", "lock", "unlock"), c("grabbable", "grabify", "ungrabify"), c("pannable", "panify", "unpanify"), t.classes != null && e.classes(t.classes), r.endBatch(), this;
    } else if (t === void 0) {
      var v = {
        data: xr(n.data),
        position: xr(n.position),
        group: n.group,
        removed: n.removed,
        selected: n.selected,
        selectable: n.selectable,
        locked: n.locked,
        grabbable: n.grabbable,
        pannable: n.pannable,
        classes: null
      };
      v.classes = "";
      var d = 0;
      return n.classes.forEach(function(h) {
        return v.classes += d++ === 0 ? h : " " + h;
      }), v;
    }
  }
};
Xe.jsons = function() {
  for (var t = [], e = 0; e < this.length; e++) {
    var r = this[e], n = r.json();
    t.push(n);
  }
  return t;
};
Xe.clone = function() {
  for (var t = this.cy(), e = [], r = 0; r < this.length; r++) {
    var n = this[r], a = n.json(), i = new Ws(t, a, !1);
    e.push(i);
  }
  return new Ct(t, e);
};
Xe.copy = Xe.clone;
Xe.restore = function() {
  for (var t = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : !0, e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0, r = this, n = r.cy(), a = n._private, i = [], s = [], o, l = 0, u = r.length; l < u; l++) {
    var f = r[l];
    e && !f.removed() || (f.isNode() ? i.push(f) : s.push(f));
  }
  o = i.concat(s);
  var c, v = function() {
    o.splice(c, 1), c--;
  };
  for (c = 0; c < o.length; c++) {
    var d = o[c], h = d._private, y = h.data;
    if (d.clearTraversalCache(), !(!e && !h.removed)) {
      if (y.id === void 0)
        y.id = eg();
      else if (pe(y.id))
        y.id = "" + y.id;
      else if (nn(y.id) || !Se(y.id)) {
        je("Can not create element with invalid string ID `" + y.id + "`"), v();
        continue;
      } else if (n.hasElementWithId(y.id)) {
        je("Can not create second element with ID `" + y.id + "`"), v();
        continue;
      }
    }
    var g = y.id;
    if (d.isNode()) {
      var p = h.position;
      p.x == null && (p.x = 0), p.y == null && (p.y = 0);
    }
    if (d.isEdge()) {
      for (var m = d, b = ["source", "target"], w = b.length, E = !1, T = 0; T < w; T++) {
        var x = b[T], S = y[x];
        pe(S) && (S = y[x] = "" + y[x]), S == null || S === "" ? (je("Can not create edge `" + g + "` with unspecified " + x), E = !0) : n.hasElementWithId(S) || (je("Can not create edge `" + g + "` with nonexistent " + x + " `" + S + "`"), E = !0);
      }
      if (E) {
        v();
        continue;
      }
      var D = n.getElementById(y.source), A = n.getElementById(y.target);
      D.same(A) ? D._private.edges.push(m) : (D._private.edges.push(m), A._private.edges.push(m)), m._private.source = D, m._private.target = A;
    }
    h.map = new Nr(), h.map.set(g, {
      ele: d,
      index: 0
    }), h.removed = !1, e && n.addToPool(d);
  }
  for (var k = 0; k < i.length; k++) {
    var R = i[k], M = R._private.data;
    pe(M.parent) && (M.parent = "" + M.parent);
    var I = M.parent, _ = I != null;
    if (_ || R._private.parent) {
      var N = R._private.parent ? n.collection().merge(R._private.parent) : n.getElementById(I);
      if (N.empty())
        M.parent = void 0;
      else if (N[0].removed())
        $e("Node added with missing parent, reference to parent removed"), M.parent = void 0, R._private.parent = null;
      else {
        for (var L = !1, F = N; !F.empty(); ) {
          if (R.same(F)) {
            L = !0, M.parent = void 0;
            break;
          }
          F = F.parent();
        }
        L || (N[0]._private.children.push(R), R._private.parent = N[0], a.hasCompoundNodes = !0);
      }
    }
  }
  if (o.length > 0) {
    for (var H = o.length === r.length ? r : new Ct(n, o), V = 0; V < H.length; V++) {
      var O = H[V];
      O.isNode() || (O.parallelEdges().clearTraversalCache(), O.source().clearTraversalCache(), O.target().clearTraversalCache());
    }
    var $;
    a.hasCompoundNodes ? $ = n.collection().merge(H).merge(H.connectedNodes()).merge(H.parent()) : $ = H, $.dirtyCompoundBoundsCache().dirtyBoundingBoxCache().updateStyle(t), t ? H.emitAndNotify("add") : e && H.emit("add");
  }
  return r;
};
Xe.removed = function() {
  var t = this[0];
  return t && t._private.removed;
};
Xe.inside = function() {
  var t = this[0];
  return t && !t._private.removed;
};
Xe.remove = function() {
  var t = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : !0, e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0, r = this, n = [], a = {}, i = r._private.cy;
  function s(I) {
    for (var _ = I._private.edges, N = 0; N < _.length; N++)
      l(_[N]);
  }
  function o(I) {
    for (var _ = I._private.children, N = 0; N < _.length; N++)
      l(_[N]);
  }
  function l(I) {
    var _ = a[I.id()];
    e && I.removed() || _ || (a[I.id()] = !0, I.isNode() ? (n.push(I), s(I), o(I)) : n.unshift(I));
  }
  for (var u = 0, f = r.length; u < f; u++) {
    var c = r[u];
    l(c);
  }
  function v(I, _) {
    var N = I._private.edges;
    an(N, _), I.clearTraversalCache();
  }
  function d(I) {
    I.clearTraversalCache();
  }
  var h = [];
  h.ids = {};
  function y(I, _) {
    _ = _[0], I = I[0];
    var N = I._private.children, L = I.id();
    an(N, _), _._private.parent = null, h.ids[L] || (h.ids[L] = !0, h.push(I));
  }
  r.dirtyCompoundBoundsCache(), e && i.removeFromPool(n);
  for (var g = 0; g < n.length; g++) {
    var p = n[g];
    if (p.isEdge()) {
      var m = p.source()[0], b = p.target()[0];
      v(m, p), v(b, p);
      for (var w = p.parallelEdges(), E = 0; E < w.length; E++) {
        var T = w[E];
        d(T), T.isBundledBezier() && T.dirtyBoundingBoxCache();
      }
    } else {
      var x = p.parent();
      x.length !== 0 && y(x, p);
    }
    e && (p._private.removed = !0);
  }
  var S = i._private.elements;
  i._private.hasCompoundNodes = !1;
  for (var D = 0; D < S.length; D++) {
    var A = S[D];
    if (A.isParent()) {
      i._private.hasCompoundNodes = !0;
      break;
    }
  }
  var k = new Ct(this.cy(), n);
  k.size() > 0 && (t ? k.emitAndNotify("remove") : e && k.emit("remove"));
  for (var R = 0; R < h.length; R++) {
    var M = h[R];
    (!e || !M.removed()) && M.updateStyle();
  }
  return k;
};
Xe.move = function(t) {
  var e = this._private.cy, r = this, n = !1, a = !1, i = function(h) {
    return h == null ? h : "" + h;
  };
  if (t.source !== void 0 || t.target !== void 0) {
    var s = i(t.source), o = i(t.target), l = s != null && e.hasElementWithId(s), u = o != null && e.hasElementWithId(o);
    (l || u) && (e.batch(function() {
      r.remove(n, a), r.emitAndNotify("moveout");
      for (var d = 0; d < r.length; d++) {
        var h = r[d], y = h._private.data;
        h.isEdge() && (l && (y.source = s), u && (y.target = o));
      }
      r.restore(n, a);
    }), r.emitAndNotify("move"));
  } else if (t.parent !== void 0) {
    var f = i(t.parent), c = f === null || e.hasElementWithId(f);
    if (c) {
      var v = f === null ? void 0 : f;
      e.batch(function() {
        var d = r.remove(n, a);
        d.emitAndNotify("moveout");
        for (var h = 0; h < r.length; h++) {
          var y = r[h], g = y._private.data;
          y.isNode() && (g.parent = v);
        }
        d.restore(n, a);
      }), r.emitAndNotify("move");
    }
  }
  return this;
};
[vg, qw, es, en, da, nx, Js, wx, _g, Fg, Sx, Ts, ts, Et, tn, Rt].forEach(function(t) {
  Ae(Xe, t);
});
var Mx = {
  add: function(e) {
    var r, n = this;
    if (Xt(e)) {
      var a = e;
      if (a._private.cy === n)
        r = a.restore();
      else {
        for (var i = [], s = 0; s < a.length; s++) {
          var o = a[s];
          i.push(o.json());
        }
        r = new Ct(n, i);
      }
    } else if (We(e)) {
      var l = e;
      r = new Ct(n, l);
    } else if (_e(e) && (We(e.nodes) || We(e.edges))) {
      for (var u = e, f = [], c = ["nodes", "edges"], v = 0, d = c.length; v < d; v++) {
        var h = c[v], y = u[h];
        if (We(y))
          for (var g = 0, p = y.length; g < p; g++) {
            var m = Ae({
              group: h
            }, y[g]);
            f.push(m);
          }
      }
      r = new Ct(n, f);
    } else {
      var b = e;
      r = new Ws(n, b).collection();
    }
    return r;
  },
  remove: function(e) {
    if (!Xt(e)) {
      if (Se(e)) {
        var r = e;
        e = this.$(r);
      }
    }
    return e.remove();
  }
};
/*! Bezier curve function generator. Copyright Gaetan Renaudeau. MIT License: http://en.wikipedia.org/wiki/MIT_License */
function Lx(t, e, r, n) {
  var a = 4, i = 1e-3, s = 1e-7, o = 10, l = 11, u = 1 / (l - 1), f = typeof Float32Array < "u";
  if (arguments.length !== 4)
    return !1;
  for (var c = 0; c < 4; ++c)
    if (typeof arguments[c] != "number" || isNaN(arguments[c]) || !isFinite(arguments[c]))
      return !1;
  t = Math.min(t, 1), r = Math.min(r, 1), t = Math.max(t, 0), r = Math.max(r, 0);
  var v = f ? new Float32Array(l) : new Array(l);
  function d(A, k) {
    return 1 - 3 * k + 3 * A;
  }
  function h(A, k) {
    return 3 * k - 6 * A;
  }
  function y(A) {
    return 3 * A;
  }
  function g(A, k, R) {
    return ((d(k, R) * A + h(k, R)) * A + y(k)) * A;
  }
  function p(A, k, R) {
    return 3 * d(k, R) * A * A + 2 * h(k, R) * A + y(k);
  }
  function m(A, k) {
    for (var R = 0; R < a; ++R) {
      var M = p(k, t, r);
      if (M === 0)
        return k;
      var I = g(k, t, r) - A;
      k -= I / M;
    }
    return k;
  }
  function b() {
    for (var A = 0; A < l; ++A)
      v[A] = g(A * u, t, r);
  }
  function w(A, k, R) {
    var M, I, _ = 0;
    do
      I = k + (R - k) / 2, M = g(I, t, r) - A, M > 0 ? R = I : k = I;
    while (Math.abs(M) > s && ++_ < o);
    return I;
  }
  function E(A) {
    for (var k = 0, R = 1, M = l - 1; R !== M && v[R] <= A; ++R)
      k += u;
    --R;
    var I = (A - v[R]) / (v[R + 1] - v[R]), _ = k + I * u, N = p(_, t, r);
    return N >= i ? m(A, _) : N === 0 ? _ : w(A, k, k + u);
  }
  var T = !1;
  function x() {
    T = !0, (t !== e || r !== n) && b();
  }
  var S = function(k) {
    return T || x(), t === e && r === n ? k : k === 0 ? 0 : k === 1 ? 1 : g(E(k), e, n);
  };
  S.getControlPoints = function() {
    return [{
      x: t,
      y: e
    }, {
      x: r,
      y: n
    }];
  };
  var D = "generateBezier(" + [t, e, r, n] + ")";
  return S.toString = function() {
    return D;
  }, S;
}
/*! Runge-Kutta spring physics function generator. Adapted from Framer.js, copyright Koen Bok. MIT License: http://en.wikipedia.org/wiki/MIT_License */
var Ix = /* @__PURE__ */ (function() {
  function t(n) {
    return -n.tension * n.x - n.friction * n.v;
  }
  function e(n, a, i) {
    var s = {
      x: n.x + i.dx * a,
      v: n.v + i.dv * a,
      tension: n.tension,
      friction: n.friction
    };
    return {
      dx: s.v,
      dv: t(s)
    };
  }
  function r(n, a) {
    var i = {
      dx: n.v,
      dv: t(n)
    }, s = e(n, a * 0.5, i), o = e(n, a * 0.5, s), l = e(n, a, o), u = 1 / 6 * (i.dx + 2 * (s.dx + o.dx) + l.dx), f = 1 / 6 * (i.dv + 2 * (s.dv + o.dv) + l.dv);
    return n.x = n.x + u * a, n.v = n.v + f * a, n;
  }
  return function n(a, i, s) {
    var o = {
      x: -1,
      v: 0,
      tension: null,
      friction: null
    }, l = [0], u = 0, f = 1 / 1e4, c = 16 / 1e3, v, d, h;
    for (a = parseFloat(a) || 500, i = parseFloat(i) || 20, s = s || null, o.tension = a, o.friction = i, v = s !== null, v ? (u = n(a, i), d = u / s * c) : d = c; h = r(h || o, d), l.push(1 + h.x), u += 16, Math.abs(h.x) > f && Math.abs(h.v) > f; )
      ;
    return v ? function(y) {
      return l[y * (l.length - 1) | 0];
    } : u;
  };
})(), Ke = function(e, r, n, a) {
  var i = Lx(e, r, n, a);
  return function(s, o, l) {
    return s + (o - s) * i(l);
  };
}, rs = {
  linear: function(e, r, n) {
    return e + (r - e) * n;
  },
  // default easings
  ease: Ke(0.25, 0.1, 0.25, 1),
  "ease-in": Ke(0.42, 0, 1, 1),
  "ease-out": Ke(0, 0, 0.58, 1),
  "ease-in-out": Ke(0.42, 0, 0.58, 1),
  // sine
  "ease-in-sine": Ke(0.47, 0, 0.745, 0.715),
  "ease-out-sine": Ke(0.39, 0.575, 0.565, 1),
  "ease-in-out-sine": Ke(0.445, 0.05, 0.55, 0.95),
  // quad
  "ease-in-quad": Ke(0.55, 0.085, 0.68, 0.53),
  "ease-out-quad": Ke(0.25, 0.46, 0.45, 0.94),
  "ease-in-out-quad": Ke(0.455, 0.03, 0.515, 0.955),
  // cubic
  "ease-in-cubic": Ke(0.55, 0.055, 0.675, 0.19),
  "ease-out-cubic": Ke(0.215, 0.61, 0.355, 1),
  "ease-in-out-cubic": Ke(0.645, 0.045, 0.355, 1),
  // quart
  "ease-in-quart": Ke(0.895, 0.03, 0.685, 0.22),
  "ease-out-quart": Ke(0.165, 0.84, 0.44, 1),
  "ease-in-out-quart": Ke(0.77, 0, 0.175, 1),
  // quint
  "ease-in-quint": Ke(0.755, 0.05, 0.855, 0.06),
  "ease-out-quint": Ke(0.23, 1, 0.32, 1),
  "ease-in-out-quint": Ke(0.86, 0, 0.07, 1),
  // expo
  "ease-in-expo": Ke(0.95, 0.05, 0.795, 0.035),
  "ease-out-expo": Ke(0.19, 1, 0.22, 1),
  "ease-in-out-expo": Ke(1, 0, 0, 1),
  // circ
  "ease-in-circ": Ke(0.6, 0.04, 0.98, 0.335),
  "ease-out-circ": Ke(0.075, 0.82, 0.165, 1),
  "ease-in-out-circ": Ke(0.785, 0.135, 0.15, 0.86),
  // user param easings...
  spring: function(e, r, n) {
    if (n === 0)
      return rs.linear;
    var a = Ix(e, r, n);
    return function(i, s, o) {
      return i + (s - i) * a(o);
    };
  },
  "cubic-bezier": Ke
};
function Qv(t, e, r, n, a) {
  if (n === 1 || e === r)
    return r;
  var i = a(e, r, n);
  return t == null || ((t.roundValue || t.color) && (i = Math.round(i)), t.min !== void 0 && (i = Math.max(i, t.min)), t.max !== void 0 && (i = Math.min(i, t.max))), i;
}
function jv(t, e) {
  return t.pfValue != null || t.value != null ? t.pfValue != null && (e == null || e.type.units !== "%") ? t.pfValue : t.value : t;
}
function $n(t, e, r, n, a) {
  var i = a != null ? a.type : null;
  r < 0 ? r = 0 : r > 1 && (r = 1);
  var s = jv(t, a), o = jv(e, a);
  if (pe(s) && pe(o))
    return Qv(i, s, o, r, n);
  if (We(s) && We(o)) {
    for (var l = [], u = 0; u < o.length; u++) {
      var f = s[u], c = o[u];
      if (f != null && c != null) {
        var v = Qv(i, f, c, r, n);
        l.push(v);
      } else
        l.push(c);
    }
    return l;
  }
}
function Ox(t, e, r, n) {
  var a = !n, i = t._private, s = e._private, o = s.easing, l = s.startTime, u = n ? t : t.cy(), f = u.style();
  if (!s.easingImpl)
    if (o == null)
      s.easingImpl = rs.linear;
    else {
      var c;
      if (Se(o)) {
        var v = f.parse("transition-timing-function", o);
        c = v.value;
      } else
        c = o;
      var d, h;
      Se(c) ? (d = c, h = []) : (d = c[1], h = c.slice(2).map(function(H) {
        return +H;
      })), h.length > 0 ? (d === "spring" && h.push(s.duration), s.easingImpl = rs[d].apply(null, h)) : s.easingImpl = rs[d];
    }
  var y = s.easingImpl, g;
  if (s.duration === 0 ? g = 1 : g = (r - l) / s.duration, s.applying && (g = s.progress), g < 0 ? g = 0 : g > 1 && (g = 1), s.delay == null) {
    var p = s.startPosition, m = s.position;
    if (m && a && !t.locked()) {
      var b = {};
      Ra(p.x, m.x) && (b.x = $n(p.x, m.x, g, y)), Ra(p.y, m.y) && (b.y = $n(p.y, m.y, g, y)), t.position(b);
    }
    var w = s.startPan, E = s.pan, T = i.pan, x = E != null && n;
    x && (Ra(w.x, E.x) && (T.x = $n(w.x, E.x, g, y)), Ra(w.y, E.y) && (T.y = $n(w.y, E.y, g, y)), t.emit("pan"));
    var S = s.startZoom, D = s.zoom, A = D != null && n;
    A && (Ra(S, D) && (i.zoom = ai(i.minZoom, $n(S, D, g, y), i.maxZoom)), t.emit("zoom")), (x || A) && t.emit("viewport");
    var k = s.style;
    if (k && k.length > 0 && a) {
      for (var R = 0; R < k.length; R++) {
        var M = k[R], I = M.name, _ = M, N = s.startStyle[I], L = f.properties[N.name], F = $n(N, _, g, y, L);
        f.overrideBypass(t, I, F);
      }
      t.emit("style");
    }
  }
  return s.progress = g, g;
}
function Ra(t, e) {
  return t == null || e == null ? !1 : pe(t) && pe(e) ? !0 : !!(t && e);
}
function Nx(t, e, r, n) {
  var a = e._private;
  a.started = !0, a.startTime = r - a.progress * a.duration;
}
function Jv(t, e) {
  var r = e._private.aniEles, n = [];
  function a(f, c) {
    var v = f._private, d = v.animation.current, h = v.animation.queue, y = !1;
    if (d.length === 0) {
      var g = h.shift();
      g && d.push(g);
    }
    for (var p = function(T) {
      for (var x = T.length - 1; x >= 0; x--) {
        var S = T[x];
        S();
      }
      T.splice(0, T.length);
    }, m = d.length - 1; m >= 0; m--) {
      var b = d[m], w = b._private;
      if (w.stopped) {
        d.splice(m, 1), w.hooked = !1, w.playing = !1, w.started = !1, p(w.frames);
        continue;
      }
      !w.playing && !w.applying || (w.playing && w.applying && (w.applying = !1), w.started || Nx(f, b, t), Ox(f, b, t, c), w.applying && (w.applying = !1), p(w.frames), w.step != null && w.step(t), b.completed() && (d.splice(m, 1), w.hooked = !1, w.playing = !1, w.started = !1, p(w.completes)), y = !0);
    }
    return !c && d.length === 0 && h.length === 0 && n.push(f), y;
  }
  for (var i = !1, s = 0; s < r.length; s++) {
    var o = r[s], l = a(o);
    i = i || l;
  }
  var u = a(e, !0);
  (i || u) && (r.length > 0 ? e.notify("draw", r) : e.notify("draw")), r.unmerge(n), e.emit("step");
}
var _x = {
  // pull in animation functions
  animate: qe.animate(),
  animation: qe.animation(),
  animated: qe.animated(),
  clearQueue: qe.clearQueue(),
  delay: qe.delay(),
  delayAnimation: qe.delayAnimation(),
  stop: qe.stop(),
  addToAnimationPool: function(e) {
    var r = this;
    r.styleEnabled() && r._private.aniEles.merge(e);
  },
  stopAnimationLoop: function() {
    this._private.animationsRunning = !1;
  },
  startAnimationLoop: function() {
    var e = this;
    if (e._private.animationsRunning = !0, !e.styleEnabled())
      return;
    function r() {
      e._private.animationsRunning && bs(function(i) {
        Jv(i, e), r();
      });
    }
    var n = e.renderer();
    n && n.beforeRender ? n.beforeRender(function(i, s) {
      Jv(s, e);
    }, n.beforeRenderPriorities.animations) : r();
  }
}, Fx = {
  qualifierCompare: function(e, r) {
    return e == null || r == null ? e == null && r == null : e.sameText(r);
  },
  eventMatches: function(e, r, n) {
    var a = r.qualifier;
    return a != null ? e !== n.target && pi(n.target) && a.matches(n.target) : !0;
  },
  addEventFields: function(e, r) {
    r.cy = e, r.target = e;
  },
  callbackContext: function(e, r, n) {
    return r.qualifier != null ? n.target : e;
  }
}, $i = function(e) {
  return Se(e) ? new on(e) : e;
}, qg = {
  createEmitter: function() {
    var e = this._private;
    return e.emitter || (e.emitter = new eo(Fx, this)), this;
  },
  emitter: function() {
    return this._private.emitter;
  },
  on: function(e, r, n) {
    return this.emitter().on(e, $i(r), n), this;
  },
  removeListener: function(e, r, n) {
    return this.emitter().removeListener(e, $i(r), n), this;
  },
  removeAllListeners: function() {
    return this.emitter().removeAllListeners(), this;
  },
  one: function(e, r, n) {
    return this.emitter().one(e, $i(r), n), this;
  },
  once: function(e, r, n) {
    return this.emitter().one(e, $i(r), n), this;
  },
  emit: function(e, r) {
    return this.emitter().emit(e, r), this;
  },
  emitAndNotify: function(e, r) {
    return this.emit(e), this.notify(e, r), this;
  }
};
qe.eventAliasesOn(qg);
var mu = {
  png: function(e) {
    var r = this._private.renderer;
    return e = e || {}, r.png(e);
  },
  jpg: function(e) {
    var r = this._private.renderer;
    return e = e || {}, e.bg = e.bg || "#fff", r.jpg(e);
  }
};
mu.jpeg = mu.jpg;
var ns = {
  layout: function(e) {
    var r = this;
    if (e == null) {
      je("Layout options must be specified to make a layout");
      return;
    }
    if (e.name == null) {
      je("A `name` must be specified to make a layout");
      return;
    }
    var n = e.name, a = r.extension("layout", n);
    if (a == null) {
      je("No such layout `" + n + "` found.  Did you forget to import it and `cytoscape.use()` it?");
      return;
    }
    var i;
    Se(e.eles) ? i = r.$(e.eles) : i = e.eles != null ? e.eles : r.$();
    var s = new a(Ae({}, e, {
      cy: r,
      eles: i
    }));
    return s;
  }
};
ns.createLayout = ns.makeLayout = ns.layout;
var zx = {
  notify: function(e, r) {
    var n = this._private;
    if (this.batching()) {
      n.batchNotifications = n.batchNotifications || {};
      var a = n.batchNotifications[e] = n.batchNotifications[e] || this.collection();
      r != null && a.merge(r);
      return;
    }
    if (n.notificationsEnabled) {
      var i = this.renderer();
      this.destroyed() || !i || i.notify(e, r);
    }
  },
  notifications: function(e) {
    var r = this._private;
    return e === void 0 ? r.notificationsEnabled : (r.notificationsEnabled = !!e, this);
  },
  noNotifications: function(e) {
    this.notifications(!1), e(), this.notifications(!0);
  },
  batching: function() {
    return this._private.batchCount > 0;
  },
  startBatch: function() {
    var e = this._private;
    return e.batchCount == null && (e.batchCount = 0), e.batchCount === 0 && (e.batchStyleEles = this.collection(), e.batchNotifications = {}), e.batchCount++, this;
  },
  endBatch: function() {
    var e = this._private;
    if (e.batchCount === 0)
      return this;
    if (e.batchCount--, e.batchCount === 0) {
      e.batchStyleEles.updateStyle();
      var r = this.renderer();
      Object.keys(e.batchNotifications).forEach(function(n) {
        var a = e.batchNotifications[n];
        a.empty() ? r.notify(n) : r.notify(n, a);
      });
    }
    return this;
  },
  batch: function(e) {
    return this.startBatch(), e(), this.endBatch(), this;
  },
  // for backwards compatibility
  batchData: function(e) {
    var r = this;
    return this.batch(function() {
      for (var n = Object.keys(e), a = 0; a < n.length; a++) {
        var i = n[a], s = e[i], o = r.getElementById(i);
        o.data(s);
      }
    });
  }
}, Vx = St({
  hideEdgesOnViewport: !1,
  textureOnViewport: !1,
  motionBlur: !1,
  motionBlurOpacity: 0.05,
  pixelRatio: void 0,
  desktopTapThreshold: 4,
  touchTapThreshold: 8,
  wheelSensitivity: 1,
  debug: !1,
  showFps: !1,
  // webgl options
  webgl: !1,
  webglDebug: !1,
  webglDebugShowAtlases: !1,
  // defaults good for mobile
  webglTexSize: 2048,
  webglTexRows: 36,
  webglTexRowsNodes: 18,
  webglBatchSize: 2048,
  webglTexPerBatch: 14,
  webglBgColor: [255, 255, 255]
}), bu = {
  renderTo: function(e, r, n, a) {
    var i = this._private.renderer;
    return i.renderTo(e, r, n, a), this;
  },
  renderer: function() {
    return this._private.renderer;
  },
  forceRender: function() {
    return this.notify("draw"), this;
  },
  resize: function() {
    return this.invalidateSize(), this.emitAndNotify("resize"), this;
  },
  initRenderer: function(e) {
    var r = this, n = r.extension("renderer", e.name);
    if (n == null) {
      je("Can not initialise: No such renderer `".concat(e.name, "` found. Did you forget to import it and `cytoscape.use()` it?"));
      return;
    }
    e.wheelSensitivity !== void 0 && $e("You have set a custom wheel sensitivity.  This will make your app zoom unnaturally when using mainstream mice.  You should change this value from the default only if you can guarantee that all your users will use the same hardware and OS configuration as your current machine.");
    var a = Vx(e);
    a.cy = r, r._private.renderer = new n(a), this.notify("init");
  },
  destroyRenderer: function() {
    var e = this;
    e.notify("destroy");
    var r = e.container();
    if (r)
      for (r._cyreg = null; r.childNodes.length > 0; )
        r.removeChild(r.childNodes[0]);
    e._private.renderer = null, e.mutableElements().forEach(function(n) {
      var a = n._private;
      a.rscratch = {}, a.rstyle = {}, a.animation.current = [], a.animation.queue = [];
    });
  },
  onRender: function(e) {
    return this.on("render", e);
  },
  offRender: function(e) {
    return this.off("render", e);
  }
};
bu.invalidateDimensions = bu.resize;
var as = {
  // get a collection
  // - empty collection on no args
  // - collection of elements in the graph on selector arg
  // - guarantee a returned collection when elements or collection specified
  collection: function(e, r) {
    return Se(e) ? this.$(e) : Xt(e) ? e.collection() : We(e) ? (r || (r = {}), new Ct(this, e, r.unique, r.removed)) : new Ct(this);
  },
  nodes: function(e) {
    var r = this.$(function(n) {
      return n.isNode();
    });
    return e ? r.filter(e) : r;
  },
  edges: function(e) {
    var r = this.$(function(n) {
      return n.isEdge();
    });
    return e ? r.filter(e) : r;
  },
  // search the graph like jQuery
  $: function(e) {
    var r = this._private.elements;
    return e ? r.filter(e) : r.spawnSelf();
  },
  mutableElements: function() {
    return this._private.elements;
  }
};
as.elements = as.filter = as.$;
var bt = {}, Wa = "t", qx = "f";
bt.apply = function(t) {
  for (var e = this, r = e._private, n = r.cy, a = n.collection(), i = 0; i < t.length; i++) {
    var s = t[i], o = e.getContextMeta(s);
    if (!o.empty) {
      var l = e.getContextStyle(o), u = e.applyContextStyle(o, l, s);
      s._private.appliedInitStyle ? e.updateTransitions(s, u.diffProps) : s._private.appliedInitStyle = !0;
      var f = e.updateStyleHints(s);
      f && a.push(s);
    }
  }
  return a;
};
bt.getPropertiesDiff = function(t, e) {
  var r = this, n = r._private.propDiffs = r._private.propDiffs || {}, a = t + "-" + e, i = n[a];
  if (i)
    return i;
  for (var s = [], o = {}, l = 0; l < r.length; l++) {
    var u = r[l], f = t[l] === Wa, c = e[l] === Wa, v = f !== c, d = u.mappedProperties.length > 0;
    if (v || c && d) {
      var h = void 0;
      v && d || v ? h = u.properties : d && (h = u.mappedProperties);
      for (var y = 0; y < h.length; y++) {
        for (var g = h[y], p = g.name, m = !1, b = l + 1; b < r.length; b++) {
          var w = r[b], E = e[b] === Wa;
          if (E && (m = w.properties[g.name] != null, m))
            break;
        }
        !o[p] && !m && (o[p] = !0, s.push(p));
      }
    }
  }
  return n[a] = s, s;
};
bt.getContextMeta = function(t) {
  for (var e = this, r = "", n, a = t._private.styleCxtKey || "", i = 0; i < e.length; i++) {
    var s = e[i], o = s.selector && s.selector.matches(t);
    o ? r += Wa : r += qx;
  }
  return n = e.getPropertiesDiff(a, r), t._private.styleCxtKey = r, {
    key: r,
    diffPropNames: n,
    empty: n.length === 0
  };
};
bt.getContextStyle = function(t) {
  var e = t.key, r = this, n = this._private.contextStyles = this._private.contextStyles || {};
  if (n[e])
    return n[e];
  for (var a = {
    _private: {
      key: e
    }
  }, i = 0; i < r.length; i++) {
    var s = r[i], o = e[i] === Wa;
    if (o)
      for (var l = 0; l < s.properties.length; l++) {
        var u = s.properties[l];
        a[u.name] = u;
      }
  }
  return n[e] = a, a;
};
bt.applyContextStyle = function(t, e, r) {
  for (var n = this, a = t.diffPropNames, i = {}, s = n.types, o = 0; o < a.length; o++) {
    var l = a[o], u = e[l], f = r.pstyle(l);
    if (!u)
      if (f)
        f.bypass ? u = {
          name: l,
          deleteBypassed: !0
        } : u = {
          name: l,
          delete: !0
        };
      else continue;
    if (f !== u) {
      if (u.mapped === s.fn && f != null && f.mapping != null && f.mapping.value === u.value) {
        var c = f.mapping, v = c.fnValue = u.value(r);
        if (v === c.prevFnValue)
          continue;
      }
      var d = i[l] = {
        prev: f
      };
      n.applyParsedProperty(r, u), d.next = r.pstyle(l), d.next && d.next.bypass && (d.next = d.next.bypassed);
    }
  }
  return {
    diffProps: i
  };
};
bt.updateStyleHints = function(t) {
  var e = t._private, r = this, n = r.propertyGroupNames, a = r.propertyGroupKeys, i = function(C, B, z) {
    return r.getPropertiesHash(C, B, z);
  }, s = e.styleKey;
  if (t.removed())
    return !1;
  var o = e.group === "nodes", l = t._private.style;
  n = Object.keys(l);
  for (var u = 0; u < a.length; u++) {
    var f = a[u];
    e.styleKeys[f] = [Tn, Xn];
  }
  for (var c = function(C, B) {
    return e.styleKeys[B][0] = ti(C, e.styleKeys[B][0]);
  }, v = function(C, B) {
    return e.styleKeys[B][1] = ri(C, e.styleKeys[B][1]);
  }, d = function(C, B) {
    c(C, B), v(C, B);
  }, h = function(C, B) {
    for (var z = 0; z < C.length; z++) {
      var W = C.charCodeAt(z);
      c(W, B), v(W, B);
    }
  }, y = 2e9, g = function(C) {
    return -128 < C && C < 128 && Math.floor(C) !== C ? y - (C * 1024 | 0) : C;
  }, p = 0; p < n.length; p++) {
    var m = n[p], b = l[m];
    if (b != null) {
      var w = this.properties[m], E = w.type, T = w.groupKey, x = void 0;
      w.hashOverride != null ? x = w.hashOverride(t, b) : b.pfValue != null && (x = b.pfValue);
      var S = w.enums == null ? b.value : null, D = x != null, A = S != null, k = D || A, R = b.units;
      if (E.number && k && !E.multiple) {
        var M = D ? x : S;
        d(g(M), T), !D && R != null && h(R, T);
      } else
        h(b.strValue, T);
    }
  }
  for (var I = [Tn, Xn], _ = 0; _ < a.length; _++) {
    var N = a[_], L = e.styleKeys[N];
    I[0] = ti(L[0], I[0]), I[1] = ri(L[1], I[1]);
  }
  e.styleKey = rb(I[0], I[1]);
  var F = e.styleKeys;
  e.labelDimsKey = Kr(F.labelDimensions);
  var H = i(t, ["label"], F.labelDimensions);
  if (e.labelKey = Kr(H), e.labelStyleKey = Kr(Ii(F.commonLabel, H)), !o) {
    var V = i(t, ["source-label"], F.labelDimensions);
    e.sourceLabelKey = Kr(V), e.sourceLabelStyleKey = Kr(Ii(F.commonLabel, V));
    var O = i(t, ["target-label"], F.labelDimensions);
    e.targetLabelKey = Kr(O), e.targetLabelStyleKey = Kr(Ii(F.commonLabel, O));
  }
  if (o) {
    var $ = e.styleKeys, Q = $.nodeBody, se = $.nodeBorder, ae = $.nodeOutline, le = $.backgroundImage, ce = $.compound, he = $.pie, ie = $.stripe, U = [Q, se, ae, le, ce, he, ie].filter(function(X) {
      return X != null;
    }).reduce(Ii, [Tn, Xn]);
    e.nodeKey = Kr(U), e.hasPie = he != null && he[0] !== Tn && he[1] !== Xn, e.hasStripe = ie != null && ie[0] !== Tn && ie[1] !== Xn;
  }
  return s !== e.styleKey;
};
bt.clearStyleHints = function(t) {
  var e = t._private;
  e.styleCxtKey = "", e.styleKeys = {}, e.styleKey = null, e.labelKey = null, e.labelStyleKey = null, e.sourceLabelKey = null, e.sourceLabelStyleKey = null, e.targetLabelKey = null, e.targetLabelStyleKey = null, e.nodeKey = null, e.hasPie = null, e.hasStripe = null;
};
bt.applyParsedProperty = function(t, e) {
  var r = this, n = e, a = t._private.style, i, s = r.types, o = r.properties[n.name].type, l = n.bypass, u = a[n.name], f = u && u.bypass, c = t._private, v = "mapping", d = function(Q) {
    return Q == null ? null : Q.pfValue != null ? Q.pfValue : Q.value;
  }, h = function() {
    var Q = d(u), se = d(n);
    r.checkTriggers(t, n.name, Q, se);
  };
  if (e.name === "curve-style" && t.isEdge() && // loops must be bundled beziers
  (e.value !== "bezier" && t.isLoop() || // edges connected to compound nodes can not be haystacks
  e.value === "haystack" && (t.source().isParent() || t.target().isParent())) && (n = e = this.parse(e.name, "bezier", l)), n.delete)
    return a[n.name] = void 0, h(), !0;
  if (n.deleteBypassed)
    return u ? u.bypass ? (u.bypassed = void 0, h(), !0) : !1 : (h(), !0);
  if (n.deleteBypass)
    return u ? u.bypass ? (a[n.name] = u.bypassed, h(), !0) : !1 : (h(), !0);
  var y = function() {
    $e("Do not assign mappings to elements without corresponding data (i.e. ele `" + t.id() + "` has no mapping for property `" + n.name + "` with data field `" + n.field + "`); try a `[" + n.field + "]` selector to limit scope to elements with `" + n.field + "` defined");
  };
  switch (n.mapped) {
    // flatten the property if mapped
    case s.mapData: {
      for (var g = n.field.split("."), p = c.data, m = 0; m < g.length && p; m++) {
        var b = g[m];
        p = p[b];
      }
      if (p == null)
        return y(), !1;
      var w;
      if (pe(p)) {
        var E = n.fieldMax - n.fieldMin;
        E === 0 ? w = 0 : w = (p - n.fieldMin) / E;
      } else
        return $e("Do not use continuous mappers without specifying numeric data (i.e. `" + n.field + ": " + p + "` for `" + t.id() + "` is non-numeric)"), !1;
      if (w < 0 ? w = 0 : w > 1 && (w = 1), o.color) {
        var T = n.valueMin[0], x = n.valueMax[0], S = n.valueMin[1], D = n.valueMax[1], A = n.valueMin[2], k = n.valueMax[2], R = n.valueMin[3] == null ? 1 : n.valueMin[3], M = n.valueMax[3] == null ? 1 : n.valueMax[3], I = [Math.round(T + (x - T) * w), Math.round(S + (D - S) * w), Math.round(A + (k - A) * w), Math.round(R + (M - R) * w)];
        i = {
          // colours are simple, so just create the flat property instead of expensive string parsing
          bypass: n.bypass,
          // we're a bypass if the mapping property is a bypass
          name: n.name,
          value: I,
          strValue: "rgb(" + I[0] + ", " + I[1] + ", " + I[2] + ")"
        };
      } else if (o.number) {
        var _ = n.valueMin + (n.valueMax - n.valueMin) * w;
        i = this.parse(n.name, _, n.bypass, v);
      } else
        return !1;
      if (!i)
        return y(), !1;
      i.mapping = n, n = i;
      break;
    }
    // direct mapping
    case s.data: {
      for (var N = n.field.split("."), L = c.data, F = 0; F < N.length && L; F++) {
        var H = N[F];
        L = L[H];
      }
      if (L != null && (i = this.parse(n.name, L, n.bypass, v)), !i)
        return y(), !1;
      i.mapping = n, n = i;
      break;
    }
    case s.fn: {
      var V = n.value, O = n.fnValue != null ? n.fnValue : V(t);
      if (n.prevFnValue = O, O == null)
        return $e("Custom function mappers may not return null (i.e. `" + n.name + "` for ele `" + t.id() + "` is null)"), !1;
      if (i = this.parse(n.name, O, n.bypass, v), !i)
        return $e("Custom function mappers may not return invalid values for the property type (i.e. `" + n.name + "` for ele `" + t.id() + "` is invalid)"), !1;
      i.mapping = xr(n), n = i;
      break;
    }
    case void 0:
      break;
    // just set the property
    default:
      return !1;
  }
  return l ? (f ? n.bypassed = u.bypassed : n.bypassed = u, a[n.name] = n) : f ? u.bypassed = n : a[n.name] = n, h(), !0;
};
bt.cleanElements = function(t, e) {
  for (var r = 0; r < t.length; r++) {
    var n = t[r];
    if (this.clearStyleHints(n), n.dirtyCompoundBoundsCache(), n.dirtyBoundingBoxCache(), !e)
      n._private.style = {};
    else
      for (var a = n._private.style, i = Object.keys(a), s = 0; s < i.length; s++) {
        var o = i[s], l = a[o];
        l != null && (l.bypass ? l.bypassed = null : a[o] = null);
      }
  }
};
bt.update = function() {
  var t = this._private.cy, e = t.mutableElements();
  e.updateStyle();
};
bt.updateTransitions = function(t, e) {
  var r = this, n = t._private, a = t.pstyle("transition-property").value, i = t.pstyle("transition-duration").pfValue, s = t.pstyle("transition-delay").pfValue;
  if (a.length > 0 && i > 0) {
    for (var o = {}, l = !1, u = 0; u < a.length; u++) {
      var f = a[u], c = t.pstyle(f), v = e[f];
      if (v) {
        var d = v.prev, h = d, y = v.next != null ? v.next : c, g = !1, p = void 0, m = 1e-6;
        h && (pe(h.pfValue) && pe(y.pfValue) ? (g = y.pfValue - h.pfValue, p = h.pfValue + m * g) : pe(h.value) && pe(y.value) ? (g = y.value - h.value, p = h.value + m * g) : We(h.value) && We(y.value) && (g = h.value[0] !== y.value[0] || h.value[1] !== y.value[1] || h.value[2] !== y.value[2], p = h.strValue), g && (o[f] = y.strValue, this.applyBypass(t, f, p), l = !0));
      }
    }
    if (!l)
      return;
    n.transitioning = !0, new ya(function(b) {
      s > 0 ? t.delayAnimation(s).play().promise().then(b) : b();
    }).then(function() {
      return t.animation({
        style: o,
        duration: i,
        easing: t.pstyle("transition-timing-function").value,
        queue: !1
      }).play().promise();
    }).then(function() {
      r.removeBypasses(t, a), t.emitAndNotify("style"), n.transitioning = !1;
    });
  } else n.transitioning && (this.removeBypasses(t, a), t.emitAndNotify("style"), n.transitioning = !1);
};
bt.checkTrigger = function(t, e, r, n, a, i) {
  var s = this.properties[e], o = a(s);
  t.removed() || o != null && o(r, n, t) && i(s);
};
bt.checkZOrderTrigger = function(t, e, r, n) {
  var a = this;
  this.checkTrigger(t, e, r, n, function(i) {
    return i.triggersZOrder;
  }, function() {
    a._private.cy.notify("zorder", t);
  });
};
bt.checkBoundsTrigger = function(t, e, r, n) {
  this.checkTrigger(t, e, r, n, function(a) {
    return a.triggersBounds;
  }, function(a) {
    t.dirtyCompoundBoundsCache(), t.dirtyBoundingBoxCache();
  });
};
bt.checkConnectedEdgesBoundsTrigger = function(t, e, r, n) {
  this.checkTrigger(t, e, r, n, function(a) {
    return a.triggersBoundsOfConnectedEdges;
  }, function(a) {
    t.connectedEdges().forEach(function(i) {
      i.dirtyBoundingBoxCache();
    });
  });
};
bt.checkParallelEdgesBoundsTrigger = function(t, e, r, n) {
  this.checkTrigger(t, e, r, n, function(a) {
    return a.triggersBoundsOfParallelEdges;
  }, function(a) {
    t.parallelEdges().forEach(function(i) {
      i.dirtyBoundingBoxCache();
    });
  });
};
bt.checkTriggers = function(t, e, r, n) {
  t.dirtyStyleCache(), this.checkZOrderTrigger(t, e, r, n), this.checkBoundsTrigger(t, e, r, n), this.checkConnectedEdgesBoundsTrigger(t, e, r, n), this.checkParallelEdgesBoundsTrigger(t, e, r, n);
};
var Ci = {};
Ci.applyBypass = function(t, e, r, n) {
  var a = this, i = [], s = !0;
  if (e === "*" || e === "**") {
    if (r !== void 0)
      for (var o = 0; o < a.properties.length; o++) {
        var l = a.properties[o], u = l.name, f = this.parse(u, r, !0);
        f && i.push(f);
      }
  } else if (Se(e)) {
    var c = this.parse(e, r, !0);
    c && i.push(c);
  } else if (_e(e)) {
    var v = e;
    n = r;
    for (var d = Object.keys(v), h = 0; h < d.length; h++) {
      var y = d[h], g = v[y];
      if (g === void 0 && (g = v[Us(y)]), g !== void 0) {
        var p = this.parse(y, g, !0);
        p && i.push(p);
      }
    }
  } else
    return !1;
  if (i.length === 0)
    return !1;
  for (var m = !1, b = 0; b < t.length; b++) {
    for (var w = t[b], E = {}, T = void 0, x = 0; x < i.length; x++) {
      var S = i[x];
      if (n) {
        var D = w.pstyle(S.name);
        T = E[S.name] = {
          prev: D
        };
      }
      m = this.applyParsedProperty(w, xr(S)) || m, n && (T.next = w.pstyle(S.name));
    }
    m && this.updateStyleHints(w), n && this.updateTransitions(w, E, s);
  }
  return m;
};
Ci.overrideBypass = function(t, e, r) {
  e = Ku(e);
  for (var n = 0; n < t.length; n++) {
    var a = t[n], i = a._private.style[e], s = this.properties[e].type, o = s.color, l = s.mutiple, u = i ? i.pfValue != null ? i.pfValue : i.value : null;
    !i || !i.bypass ? this.applyBypass(a, e, r) : (i.value = r, i.pfValue != null && (i.pfValue = r), o ? i.strValue = "rgb(" + r.join(",") + ")" : l ? i.strValue = r.join(" ") : i.strValue = "" + r, this.updateStyleHints(a)), this.checkTriggers(a, e, u, r);
  }
};
Ci.removeAllBypasses = function(t, e) {
  return this.removeBypasses(t, this.propertyNames, e);
};
Ci.removeBypasses = function(t, e, r) {
  for (var n = !0, a = 0; a < t.length; a++) {
    for (var i = t[a], s = {}, o = 0; o < e.length; o++) {
      var l = e[o], u = this.properties[l], f = i.pstyle(u.name);
      if (!(!f || !f.bypass)) {
        var c = "", v = this.parse(l, c, !0), d = s[u.name] = {
          prev: f
        };
        this.applyParsedProperty(i, v), d.next = i.pstyle(u.name);
      }
    }
    this.updateStyleHints(i), r && this.updateTransitions(i, s, n);
  }
};
var uf = {};
uf.getEmSizeInPixels = function() {
  var t = this.containerCss("font-size");
  return t != null ? parseFloat(t) : 1;
};
uf.containerCss = function(t) {
  var e = this._private.cy, r = e.container(), n = e.window();
  if (n && r && n.getComputedStyle)
    return n.getComputedStyle(r).getPropertyValue(t);
};
var Tr = {};
Tr.getRenderedStyle = function(t, e) {
  return e ? this.getStylePropertyValue(t, e, !0) : this.getRawStyle(t, !0);
};
Tr.getRawStyle = function(t, e) {
  var r = this;
  if (t = t[0], t) {
    for (var n = {}, a = 0; a < r.properties.length; a++) {
      var i = r.properties[a], s = r.getStylePropertyValue(t, i.name, e);
      s != null && (n[i.name] = s, n[Us(i.name)] = s);
    }
    return n;
  }
};
Tr.getIndexedStyle = function(t, e, r, n) {
  var a = t.pstyle(e)[r][n];
  return a ?? t.cy().style().getDefaultProperty(e)[r][0];
};
Tr.getStylePropertyValue = function(t, e, r) {
  var n = this;
  if (t = t[0], t) {
    var a = n.properties[e];
    a.alias && (a = a.pointsTo);
    var i = a.type, s = t.pstyle(a.name);
    if (s) {
      var o = s.value, l = s.units, u = s.strValue;
      if (r && i.number && o != null && pe(o)) {
        var f = t.cy().zoom(), c = function(g) {
          return g * f;
        }, v = function(g, p) {
          return c(g) + p;
        }, d = We(o), h = d ? l.every(function(y) {
          return y != null;
        }) : l != null;
        return h ? d ? o.map(function(y, g) {
          return v(y, l[g]);
        }).join(" ") : v(o, l) : d ? o.map(function(y) {
          return Se(y) ? y : "" + c(y);
        }).join(" ") : "" + c(o);
      } else if (u != null)
        return u;
    }
    return null;
  }
};
Tr.getAnimationStartStyle = function(t, e) {
  for (var r = {}, n = 0; n < e.length; n++) {
    var a = e[n], i = a.name, s = t.pstyle(i);
    s !== void 0 && (_e(s) ? s = this.parse(i, s.strValue) : s = this.parse(i, s)), s && (r[i] = s);
  }
  return r;
};
Tr.getPropsList = function(t) {
  var e = this, r = [], n = t, a = e.properties;
  if (n)
    for (var i = Object.keys(n), s = 0; s < i.length; s++) {
      var o = i[s], l = n[o], u = a[o] || a[Ku(o)], f = this.parse(u.name, l);
      f && r.push(f);
    }
  return r;
};
Tr.getNonDefaultPropertiesHash = function(t, e, r) {
  var n = r.slice(), a, i, s, o, l, u;
  for (l = 0; l < e.length; l++)
    if (a = e[l], i = t.pstyle(a, !1), i != null)
      if (i.pfValue != null)
        n[0] = ti(o, n[0]), n[1] = ri(o, n[1]);
      else
        for (s = i.strValue, u = 0; u < s.length; u++)
          o = s.charCodeAt(u), n[0] = ti(o, n[0]), n[1] = ri(o, n[1]);
  return n;
};
Tr.getPropertiesHash = Tr.getNonDefaultPropertiesHash;
var no = {};
no.appendFromJson = function(t) {
  for (var e = this, r = 0; r < t.length; r++) {
    var n = t[r], a = n.selector, i = n.style || n.css, s = Object.keys(i);
    e.selector(a);
    for (var o = 0; o < s.length; o++) {
      var l = s[o], u = i[l];
      e.css(l, u);
    }
  }
  return e;
};
no.fromJson = function(t) {
  var e = this;
  return e.resetToDefault(), e.appendFromJson(t), e;
};
no.json = function() {
  for (var t = [], e = this.defaultLength; e < this.length; e++) {
    for (var r = this[e], n = r.selector, a = r.properties, i = {}, s = 0; s < a.length; s++) {
      var o = a[s];
      i[o.name] = o.strValue;
    }
    t.push({
      selector: n ? n.toString() : "core",
      style: i
    });
  }
  return t;
};
var ff = {};
ff.appendFromString = function(t) {
  var e = this, r = this, n = "" + t, a, i, s;
  n = n.replace(/[/][*](\s|.)+?[*][/]/g, "");
  function o() {
    n.length > a.length ? n = n.substr(a.length) : n = "";
  }
  function l() {
    i.length > s.length ? i = i.substr(s.length) : i = "";
  }
  for (; ; ) {
    var u = n.match(/^\s*$/);
    if (u)
      break;
    var f = n.match(/^\s*((?:.|\s)+?)\s*\{((?:.|\s)+?)\}/);
    if (!f) {
      $e("Halting stylesheet parsing: String stylesheet contains more to parse but no selector and block found in: " + n);
      break;
    }
    a = f[0];
    var c = f[1];
    if (c !== "core") {
      var v = new on(c);
      if (v.invalid) {
        $e("Skipping parsing of block: Invalid selector found in string stylesheet: " + c), o();
        continue;
      }
    }
    var d = f[2], h = !1;
    i = d;
    for (var y = []; ; ) {
      var g = i.match(/^\s*$/);
      if (g)
        break;
      var p = i.match(/^\s*(.+?)\s*:\s*(.+?)(?:\s*;|\s*$)/);
      if (!p) {
        $e("Skipping parsing of block: Invalid formatting of style property and value definitions found in:" + d), h = !0;
        break;
      }
      s = p[0];
      var m = p[1], b = p[2], w = e.properties[m];
      if (!w) {
        $e("Skipping property: Invalid property name in: " + s), l();
        continue;
      }
      var E = r.parse(m, b);
      if (!E) {
        $e("Skipping property: Invalid property definition in: " + s), l();
        continue;
      }
      y.push({
        name: m,
        val: b
      }), l();
    }
    if (h) {
      o();
      break;
    }
    r.selector(c);
    for (var T = 0; T < y.length; T++) {
      var x = y[T];
      r.css(x.name, x.val);
    }
    o();
  }
  return r;
};
ff.fromString = function(t) {
  var e = this;
  return e.resetToDefault(), e.appendFromString(t), e;
};
var lt = {};
(function() {
  var t = vt, e = I0, r = N0, n = _0, a = F0, i = function(X) {
    return "^" + X + "\\s*\\(\\s*([\\w\\.]+)\\s*\\)$";
  }, s = function(X) {
    var C = t + "|\\w+|" + e + "|" + r + "|" + n + "|" + a;
    return "^" + X + "\\s*\\(([\\w\\.]+)\\s*\\,\\s*(" + t + ")\\s*\\,\\s*(" + t + ")\\s*,\\s*(" + C + ")\\s*\\,\\s*(" + C + ")\\)$";
  }, o = [`^url\\s*\\(\\s*['"]?(.+?)['"]?\\s*\\)$`, "^(none)$", "^(.+)$"];
  lt.types = {
    time: {
      number: !0,
      min: 0,
      units: "s|ms",
      implicitUnits: "ms"
    },
    percent: {
      number: !0,
      min: 0,
      max: 100,
      units: "%",
      implicitUnits: "%"
    },
    percentages: {
      number: !0,
      min: 0,
      max: 100,
      units: "%",
      implicitUnits: "%",
      multiple: !0
    },
    zeroOneNumber: {
      number: !0,
      min: 0,
      max: 1,
      unitless: !0
    },
    zeroOneNumbers: {
      number: !0,
      min: 0,
      max: 1,
      unitless: !0,
      multiple: !0
    },
    nOneOneNumber: {
      number: !0,
      min: -1,
      max: 1,
      unitless: !0
    },
    nonNegativeInt: {
      number: !0,
      min: 0,
      integer: !0,
      unitless: !0
    },
    nonNegativeNumber: {
      number: !0,
      min: 0,
      unitless: !0
    },
    position: {
      enums: ["parent", "origin"]
    },
    nodeSize: {
      number: !0,
      min: 0,
      enums: ["label"]
    },
    number: {
      number: !0,
      unitless: !0
    },
    numbers: {
      number: !0,
      unitless: !0,
      multiple: !0
    },
    positiveNumber: {
      number: !0,
      unitless: !0,
      min: 0,
      strictMin: !0
    },
    size: {
      number: !0,
      min: 0
    },
    bidirectionalSize: {
      number: !0
    },
    // allows negative
    bidirectionalSizeMaybePercent: {
      number: !0,
      allowPercent: !0
    },
    // allows negative
    bidirectionalSizes: {
      number: !0,
      multiple: !0
    },
    // allows negative
    sizeMaybePercent: {
      number: !0,
      min: 0,
      allowPercent: !0
    },
    axisDirection: {
      enums: ["horizontal", "leftward", "rightward", "vertical", "upward", "downward", "auto"]
    },
    axisDirectionExplicit: {
      enums: ["leftward", "rightward", "upward", "downward"]
    },
    axisDirectionPrimary: {
      enums: ["horizontal", "vertical"]
    },
    paddingRelativeTo: {
      enums: ["width", "height", "average", "min", "max"]
    },
    bgWH: {
      number: !0,
      min: 0,
      allowPercent: !0,
      enums: ["auto"],
      multiple: !0
    },
    bgPos: {
      number: !0,
      allowPercent: !0,
      multiple: !0
    },
    bgRelativeTo: {
      enums: ["inner", "include-padding"],
      multiple: !0
    },
    bgRepeat: {
      enums: ["repeat", "repeat-x", "repeat-y", "no-repeat"],
      multiple: !0
    },
    bgFit: {
      enums: ["none", "contain", "cover"],
      multiple: !0
    },
    bgCrossOrigin: {
      enums: ["anonymous", "use-credentials", "null"],
      multiple: !0
    },
    bgClip: {
      enums: ["none", "node"],
      multiple: !0
    },
    bgContainment: {
      enums: ["inside", "over"],
      multiple: !0
    },
    boxSelection: {
      enums: ["contain", "overlap", "none"]
    },
    color: {
      color: !0
    },
    colors: {
      color: !0,
      multiple: !0
    },
    fill: {
      enums: ["solid", "linear-gradient", "radial-gradient"]
    },
    bool: {
      enums: ["yes", "no"]
    },
    bools: {
      enums: ["yes", "no"],
      multiple: !0
    },
    lineStyle: {
      enums: ["solid", "dotted", "dashed"]
    },
    lineCap: {
      enums: ["butt", "round", "square"]
    },
    linePosition: {
      enums: ["center", "inside", "outside"]
    },
    lineJoin: {
      enums: ["round", "bevel", "miter"]
    },
    borderStyle: {
      enums: ["solid", "dotted", "dashed", "double"]
    },
    curveStyle: {
      enums: ["bezier", "unbundled-bezier", "haystack", "segments", "straight", "straight-triangle", "taxi", "round-segments", "round-taxi"]
    },
    radiusType: {
      enums: ["arc-radius", "influence-radius"],
      multiple: !0
    },
    fontFamily: {
      regex: '^([\\w- \\"]+(?:\\s*,\\s*[\\w- \\"]+)*)$'
    },
    fontStyle: {
      enums: ["italic", "normal", "oblique"]
    },
    fontWeight: {
      enums: ["normal", "bold", "bolder", "lighter", "100", "200", "300", "400", "500", "600", "800", "900", 100, 200, 300, 400, 500, 600, 700, 800, 900]
    },
    textDecoration: {
      enums: ["none", "underline", "overline", "line-through"]
    },
    textTransform: {
      enums: ["none", "uppercase", "lowercase"]
    },
    textWrap: {
      enums: ["none", "wrap", "ellipsis"]
    },
    textOverflowWrap: {
      enums: ["whitespace", "anywhere"]
    },
    textBackgroundShape: {
      enums: ["rectangle", "roundrectangle", "round-rectangle", "circle"]
    },
    nodeShape: {
      enums: ["rectangle", "roundrectangle", "round-rectangle", "cutrectangle", "cut-rectangle", "bottomroundrectangle", "bottom-round-rectangle", "barrel", "ellipse", "triangle", "round-triangle", "square", "pentagon", "round-pentagon", "hexagon", "round-hexagon", "concavehexagon", "concave-hexagon", "heptagon", "round-heptagon", "octagon", "round-octagon", "tag", "round-tag", "star", "diamond", "round-diamond", "vee", "rhomboid", "right-rhomboid", "polygon"]
    },
    overlayShape: {
      enums: ["roundrectangle", "round-rectangle", "ellipse"]
    },
    cornerRadius: {
      number: !0,
      min: 0,
      units: "px|em",
      implicitUnits: "px",
      enums: ["auto"]
    },
    compoundIncludeLabels: {
      enums: ["include", "exclude"]
    },
    arrowShape: {
      enums: ["tee", "triangle", "triangle-tee", "circle-triangle", "triangle-cross", "triangle-backcurve", "vee", "square", "circle", "diamond", "chevron", "none"]
    },
    arrowFill: {
      enums: ["filled", "hollow"]
    },
    arrowWidth: {
      number: !0,
      units: "%|px|em",
      implicitUnits: "px",
      enums: ["match-line"]
    },
    display: {
      enums: ["element", "none"]
    },
    visibility: {
      enums: ["hidden", "visible"]
    },
    zCompoundDepth: {
      enums: ["bottom", "orphan", "auto", "top"]
    },
    zIndexCompare: {
      enums: ["auto", "manual"]
    },
    valign: {
      enums: ["top", "top-inside", "center", "bottom", "bottom-inside"]
    },
    halign: {
      enums: ["left", "left-inside", "center", "right", "right-inside"]
    },
    justification: {
      enums: ["left", "center", "right", "auto"]
    },
    textMetrics: {
      enums: ["font", "glyph"]
    },
    text: {
      string: !0
    },
    data: {
      mapping: !0,
      regex: i("data")
    },
    layoutData: {
      mapping: !0,
      regex: i("layoutData")
    },
    scratch: {
      mapping: !0,
      regex: i("scratch")
    },
    mapData: {
      mapping: !0,
      regex: s("mapData")
    },
    mapLayoutData: {
      mapping: !0,
      regex: s("mapLayoutData")
    },
    mapScratch: {
      mapping: !0,
      regex: s("mapScratch")
    },
    fn: {
      mapping: !0,
      fn: !0
    },
    url: {
      regexes: o,
      singleRegexMatchValue: !0
    },
    urls: {
      regexes: o,
      singleRegexMatchValue: !0,
      multiple: !0
    },
    propList: {
      propList: !0
    },
    angle: {
      number: !0,
      units: "deg|rad",
      implicitUnits: "rad"
    },
    textRotation: {
      number: !0,
      units: "deg|rad",
      implicitUnits: "rad",
      enums: ["none", "autorotate"]
    },
    polygonPointList: {
      number: !0,
      multiple: !0,
      evenMultiple: !0,
      min: -1,
      max: 1,
      unitless: !0
    },
    edgeDistances: {
      enums: ["intersection", "node-position", "endpoints"]
    },
    edgeEndpoint: {
      number: !0,
      multiple: !0,
      units: "%|px|em|deg|rad",
      implicitUnits: "px",
      enums: ["inside-to-node", "outside-to-node", "outside-to-node-or-label", "outside-to-line", "outside-to-line-or-label"],
      singleEnum: !0,
      validate: function(X, C) {
        switch (X.length) {
          case 2:
            return C[0] !== "deg" && C[0] !== "rad" && C[1] !== "deg" && C[1] !== "rad";
          case 1:
            return Se(X[0]) || C[0] === "deg" || C[0] === "rad";
          default:
            return !1;
        }
      }
    },
    easing: {
      regexes: ["^(spring)\\s*\\(\\s*(" + t + ")\\s*,\\s*(" + t + ")\\s*\\)$", "^(cubic-bezier)\\s*\\(\\s*(" + t + ")\\s*,\\s*(" + t + ")\\s*,\\s*(" + t + ")\\s*,\\s*(" + t + ")\\s*\\)$"],
      enums: ["linear", "ease", "ease-in", "ease-out", "ease-in-out", "ease-in-sine", "ease-out-sine", "ease-in-out-sine", "ease-in-quad", "ease-out-quad", "ease-in-out-quad", "ease-in-cubic", "ease-out-cubic", "ease-in-out-cubic", "ease-in-quart", "ease-out-quart", "ease-in-out-quart", "ease-in-quint", "ease-out-quint", "ease-in-out-quint", "ease-in-expo", "ease-out-expo", "ease-in-out-expo", "ease-in-circ", "ease-out-circ", "ease-in-out-circ"]
    },
    gradientDirection: {
      enums: [
        "to-bottom",
        "to-top",
        "to-left",
        "to-right",
        "to-bottom-right",
        "to-bottom-left",
        "to-top-right",
        "to-top-left",
        "to-right-bottom",
        "to-left-bottom",
        "to-right-top",
        "to-left-top"
        // different order
      ]
    },
    boundsExpansion: {
      number: !0,
      multiple: !0,
      min: 0,
      validate: function(X) {
        var C = X.length;
        return C === 1 || C === 2 || C === 4;
      }
    }
  };
  var l = {
    zeroNonZero: function(X, C) {
      return (X == null || C == null) && X !== C || X == 0 && C != 0 ? !0 : X != 0 && C == 0;
    },
    any: function(X, C) {
      return X != C;
    },
    emptyNonEmpty: function(X, C) {
      var B = nn(X), z = nn(C);
      return B && !z || !B && z;
    }
  }, u = lt.types, f = [{
    name: "label",
    type: u.text,
    triggersBounds: l.any,
    triggersZOrder: l.emptyNonEmpty
  }, {
    name: "text-rotation",
    type: u.textRotation,
    triggersBounds: l.any
  }, {
    name: "text-margin-x",
    type: u.bidirectionalSize,
    triggersBounds: l.any
  }, {
    name: "text-margin-y",
    type: u.bidirectionalSize,
    triggersBounds: l.any
  }], c = [{
    name: "source-label",
    type: u.text,
    triggersBounds: l.any
  }, {
    name: "source-text-rotation",
    type: u.textRotation,
    triggersBounds: l.any
  }, {
    name: "source-text-margin-x",
    type: u.bidirectionalSize,
    triggersBounds: l.any
  }, {
    name: "source-text-margin-y",
    type: u.bidirectionalSize,
    triggersBounds: l.any
  }, {
    name: "source-text-offset",
    type: u.size,
    triggersBounds: l.any
  }], v = [{
    name: "target-label",
    type: u.text,
    triggersBounds: l.any
  }, {
    name: "target-text-rotation",
    type: u.textRotation,
    triggersBounds: l.any
  }, {
    name: "target-text-margin-x",
    type: u.bidirectionalSize,
    triggersBounds: l.any
  }, {
    name: "target-text-margin-y",
    type: u.bidirectionalSize,
    triggersBounds: l.any
  }, {
    name: "target-text-offset",
    type: u.size,
    triggersBounds: l.any
  }], d = [{
    name: "font-family",
    type: u.fontFamily,
    triggersBounds: l.any
  }, {
    name: "font-style",
    type: u.fontStyle,
    triggersBounds: l.any
  }, {
    name: "font-weight",
    type: u.fontWeight,
    triggersBounds: l.any
  }, {
    name: "font-size",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "text-transform",
    type: u.textTransform,
    triggersBounds: l.any
  }, {
    name: "text-wrap",
    type: u.textWrap,
    triggersBounds: l.any
  }, {
    name: "text-overflow-wrap",
    type: u.textOverflowWrap,
    triggersBounds: l.any
  }, {
    name: "text-max-width",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "text-outline-width",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "line-height",
    type: u.positiveNumber,
    triggersBounds: l.any
  }], h = [{
    name: "text-valign",
    type: u.valign,
    triggersBounds: l.any
  }, {
    name: "text-halign",
    type: u.halign,
    triggersBounds: l.any
  }, {
    name: "color",
    type: u.color
  }, {
    name: "text-outline-color",
    type: u.color
  }, {
    name: "text-outline-opacity",
    type: u.zeroOneNumber
  }, {
    name: "text-background-color",
    type: u.color
  }, {
    name: "text-background-opacity",
    type: u.zeroOneNumber
  }, {
    name: "text-background-padding",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "text-border-opacity",
    type: u.zeroOneNumber
  }, {
    name: "text-border-color",
    type: u.color
  }, {
    name: "text-border-width",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "text-border-style",
    type: u.borderStyle,
    triggersBounds: l.any
  }, {
    name: "text-background-shape",
    type: u.textBackgroundShape,
    triggersBounds: l.any
  }, {
    name: "text-justification",
    type: u.justification
  }, {
    name: "text-metrics",
    type: u.textMetrics
  }, {
    name: "box-select-labels",
    type: u.bool,
    triggersBounds: l.any
  }], y = [{
    name: "events",
    type: u.bool,
    triggersZOrder: l.any
  }, {
    name: "text-events",
    type: u.bool,
    triggersZOrder: l.any
  }, {
    name: "box-selection",
    type: u.boxSelection,
    triggersZOrder: l.any
  }], g = [{
    name: "display",
    type: u.display,
    triggersZOrder: l.any,
    triggersBounds: l.any,
    triggersBoundsOfConnectedEdges: l.any,
    triggersBoundsOfParallelEdges: function(X, C, B) {
      return X === C ? !1 : B.pstyle("curve-style").value === "bezier";
    }
  }, {
    name: "visibility",
    type: u.visibility,
    triggersZOrder: l.any
  }, {
    name: "opacity",
    type: u.zeroOneNumber,
    triggersZOrder: l.zeroNonZero
  }, {
    name: "text-opacity",
    type: u.zeroOneNumber
  }, {
    name: "min-zoomed-font-size",
    type: u.size
  }, {
    name: "z-compound-depth",
    type: u.zCompoundDepth,
    triggersZOrder: l.any
  }, {
    name: "z-index-compare",
    type: u.zIndexCompare,
    triggersZOrder: l.any
  }, {
    name: "z-index",
    type: u.number,
    triggersZOrder: l.any
  }], p = [{
    name: "overlay-padding",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "overlay-color",
    type: u.color
  }, {
    name: "overlay-opacity",
    type: u.zeroOneNumber,
    triggersBounds: l.zeroNonZero
  }, {
    name: "overlay-shape",
    type: u.overlayShape,
    triggersBounds: l.any
  }, {
    name: "overlay-corner-radius",
    type: u.cornerRadius
  }], m = [{
    name: "underlay-padding",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "underlay-color",
    type: u.color
  }, {
    name: "underlay-opacity",
    type: u.zeroOneNumber,
    triggersBounds: l.zeroNonZero
  }, {
    name: "underlay-shape",
    type: u.overlayShape,
    triggersBounds: l.any
  }, {
    name: "underlay-corner-radius",
    type: u.cornerRadius
  }], b = [{
    name: "transition-property",
    type: u.propList
  }, {
    name: "transition-duration",
    type: u.time
  }, {
    name: "transition-delay",
    type: u.time
  }, {
    name: "transition-timing-function",
    type: u.easing
  }], w = function(X, C) {
    return C.value === "label" ? -X.poolIndex() : C.pfValue;
  }, E = [{
    name: "height",
    type: u.nodeSize,
    triggersBounds: l.any,
    hashOverride: w
  }, {
    name: "width",
    type: u.nodeSize,
    triggersBounds: l.any,
    hashOverride: w
  }, {
    name: "shape",
    type: u.nodeShape,
    triggersBounds: l.any
  }, {
    name: "shape-polygon-points",
    type: u.polygonPointList,
    triggersBounds: l.any
  }, {
    name: "corner-radius",
    type: u.cornerRadius
  }, {
    name: "background-color",
    type: u.color
  }, {
    name: "background-fill",
    type: u.fill
  }, {
    name: "background-opacity",
    type: u.zeroOneNumber
  }, {
    name: "background-blacken",
    type: u.nOneOneNumber
  }, {
    name: "background-gradient-stop-colors",
    type: u.colors
  }, {
    name: "background-gradient-stop-positions",
    type: u.percentages
  }, {
    name: "background-gradient-direction",
    type: u.gradientDirection
  }, {
    name: "padding",
    type: u.sizeMaybePercent,
    triggersBounds: l.any
  }, {
    name: "padding-relative-to",
    type: u.paddingRelativeTo,
    triggersBounds: l.any
  }, {
    name: "bounds-expansion",
    type: u.boundsExpansion,
    triggersBounds: l.any
  }], T = [{
    name: "border-color",
    type: u.color
  }, {
    name: "border-opacity",
    type: u.zeroOneNumber
  }, {
    name: "border-width",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "border-style",
    type: u.borderStyle
  }, {
    name: "border-cap",
    type: u.lineCap
  }, {
    name: "border-join",
    type: u.lineJoin
  }, {
    name: "border-dash-pattern",
    type: u.numbers
  }, {
    name: "border-dash-offset",
    type: u.number
  }, {
    name: "border-position",
    type: u.linePosition
  }], x = [{
    name: "outline-color",
    type: u.color
  }, {
    name: "outline-opacity",
    type: u.zeroOneNumber
  }, {
    name: "outline-width",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "outline-style",
    type: u.borderStyle
  }, {
    name: "outline-offset",
    type: u.size,
    triggersBounds: l.any
  }], S = [{
    name: "background-image",
    type: u.urls
  }, {
    name: "background-image-crossorigin",
    type: u.bgCrossOrigin
  }, {
    name: "background-image-opacity",
    type: u.zeroOneNumbers
  }, {
    name: "background-image-containment",
    type: u.bgContainment
  }, {
    name: "background-image-smoothing",
    type: u.bools
  }, {
    name: "background-position-x",
    type: u.bgPos
  }, {
    name: "background-position-y",
    type: u.bgPos
  }, {
    name: "background-width-relative-to",
    type: u.bgRelativeTo
  }, {
    name: "background-height-relative-to",
    type: u.bgRelativeTo
  }, {
    name: "background-repeat",
    type: u.bgRepeat
  }, {
    name: "background-fit",
    type: u.bgFit
  }, {
    name: "background-clip",
    type: u.bgClip
  }, {
    name: "background-width",
    type: u.bgWH
  }, {
    name: "background-height",
    type: u.bgWH
  }, {
    name: "background-offset-x",
    type: u.bgPos
  }, {
    name: "background-offset-y",
    type: u.bgPos
  }], D = [{
    name: "position",
    type: u.position,
    triggersBounds: l.any
  }, {
    name: "compound-sizing-wrt-labels",
    type: u.compoundIncludeLabels,
    triggersBounds: l.any
  }, {
    name: "min-width",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "min-width-bias-left",
    type: u.sizeMaybePercent,
    triggersBounds: l.any
  }, {
    name: "min-width-bias-right",
    type: u.sizeMaybePercent,
    triggersBounds: l.any
  }, {
    name: "min-height",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "min-height-bias-top",
    type: u.sizeMaybePercent,
    triggersBounds: l.any
  }, {
    name: "min-height-bias-bottom",
    type: u.sizeMaybePercent,
    triggersBounds: l.any
  }], A = [{
    name: "line-style",
    type: u.lineStyle
  }, {
    name: "line-color",
    type: u.color
  }, {
    name: "line-fill",
    type: u.fill
  }, {
    name: "line-cap",
    type: u.lineCap
  }, {
    name: "line-opacity",
    type: u.zeroOneNumber
  }, {
    name: "line-dash-pattern",
    type: u.numbers
  }, {
    name: "line-dash-offset",
    type: u.number
  }, {
    name: "line-outline-width",
    type: u.size
  }, {
    name: "line-outline-color",
    type: u.color
  }, {
    name: "line-gradient-stop-colors",
    type: u.colors
  }, {
    name: "line-gradient-stop-positions",
    type: u.percentages
  }, {
    name: "curve-style",
    type: u.curveStyle,
    triggersBounds: l.any,
    triggersBoundsOfParallelEdges: function(X, C) {
      return X === C ? !1 : X === "bezier" || // remove from bundle
      C === "bezier";
    }
  }, {
    name: "haystack-radius",
    type: u.zeroOneNumber,
    triggersBounds: l.any
  }, {
    name: "source-endpoint",
    type: u.edgeEndpoint,
    triggersBounds: l.any
  }, {
    name: "target-endpoint",
    type: u.edgeEndpoint,
    triggersBounds: l.any
  }, {
    name: "control-point-step-size",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "control-point-distances",
    type: u.bidirectionalSizes,
    triggersBounds: l.any
  }, {
    name: "control-point-weights",
    type: u.numbers,
    triggersBounds: l.any
  }, {
    name: "segment-distances",
    type: u.bidirectionalSizes,
    triggersBounds: l.any
  }, {
    name: "segment-weights",
    type: u.numbers,
    triggersBounds: l.any
  }, {
    name: "segment-radii",
    type: u.numbers,
    triggersBounds: l.any
  }, {
    name: "radius-type",
    type: u.radiusType,
    triggersBounds: l.any
  }, {
    name: "taxi-turn",
    type: u.bidirectionalSizeMaybePercent,
    triggersBounds: l.any
  }, {
    name: "taxi-turn-min-distance",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "taxi-direction",
    type: u.axisDirection,
    triggersBounds: l.any
  }, {
    name: "taxi-radius",
    type: u.number,
    triggersBounds: l.any
  }, {
    name: "edge-distances",
    type: u.edgeDistances,
    triggersBounds: l.any
  }, {
    name: "arrow-scale",
    type: u.positiveNumber,
    triggersBounds: l.any
  }, {
    name: "loop-direction",
    type: u.angle,
    triggersBounds: l.any
  }, {
    name: "loop-sweep",
    type: u.angle,
    triggersBounds: l.any
  }, {
    name: "source-distance-from-node",
    type: u.size,
    triggersBounds: l.any
  }, {
    name: "target-distance-from-node",
    type: u.size,
    triggersBounds: l.any
  }], k = [{
    name: "ghost",
    type: u.bool,
    triggersBounds: l.any
  }, {
    name: "ghost-offset-x",
    type: u.bidirectionalSize,
    triggersBounds: l.any
  }, {
    name: "ghost-offset-y",
    type: u.bidirectionalSize,
    triggersBounds: l.any
  }, {
    name: "ghost-opacity",
    type: u.zeroOneNumber
  }], R = [{
    name: "selection-box-color",
    type: u.color
  }, {
    name: "selection-box-opacity",
    type: u.zeroOneNumber
  }, {
    name: "selection-box-border-color",
    type: u.color
  }, {
    name: "selection-box-border-width",
    type: u.size
  }, {
    name: "active-bg-color",
    type: u.color
  }, {
    name: "active-bg-opacity",
    type: u.zeroOneNumber
  }, {
    name: "active-bg-size",
    type: u.size
  }, {
    name: "outside-texture-bg-color",
    type: u.color
  }, {
    name: "outside-texture-bg-opacity",
    type: u.zeroOneNumber
  }], M = [];
  lt.pieBackgroundN = 16, M.push({
    name: "pie-size",
    type: u.sizeMaybePercent
  }), M.push({
    name: "pie-hole",
    type: u.sizeMaybePercent
  }), M.push({
    name: "pie-start-angle",
    type: u.angle
  });
  for (var I = 1; I <= lt.pieBackgroundN; I++)
    M.push({
      name: "pie-" + I + "-background-color",
      type: u.color
    }), M.push({
      name: "pie-" + I + "-background-size",
      type: u.percent
    }), M.push({
      name: "pie-" + I + "-background-opacity",
      type: u.zeroOneNumber
    });
  var _ = [];
  lt.stripeBackgroundN = 16, _.push({
    name: "stripe-size",
    type: u.sizeMaybePercent
  }), _.push({
    name: "stripe-direction",
    type: u.axisDirectionPrimary
  });
  for (var N = 1; N <= lt.stripeBackgroundN; N++)
    _.push({
      name: "stripe-" + N + "-background-color",
      type: u.color
    }), _.push({
      name: "stripe-" + N + "-background-size",
      type: u.percent
    }), _.push({
      name: "stripe-" + N + "-background-opacity",
      type: u.zeroOneNumber
    });
  var L = [], F = lt.arrowPrefixes = ["source", "mid-source", "target", "mid-target"];
  [{
    name: "arrow-shape",
    type: u.arrowShape,
    triggersBounds: l.any
  }, {
    name: "arrow-color",
    type: u.color
  }, {
    name: "arrow-fill",
    type: u.arrowFill
  }, {
    name: "arrow-width",
    type: u.arrowWidth
  }].forEach(function(U) {
    F.forEach(function(X) {
      var C = X + "-" + U.name, B = U.type, z = U.triggersBounds;
      L.push({
        name: C,
        type: B,
        triggersBounds: z
      });
    });
  }, {});
  var H = lt.properties = [].concat(y, b, g, p, m, k, h, d, f, c, v, E, T, x, S, M, _, D, A, L, R), V = lt.propertyGroups = {
    // common to all eles
    behavior: y,
    transition: b,
    visibility: g,
    overlay: p,
    underlay: m,
    ghost: k,
    // labels
    commonLabel: h,
    labelDimensions: d,
    mainLabel: f,
    sourceLabel: c,
    targetLabel: v,
    // node props
    nodeBody: E,
    nodeBorder: T,
    nodeOutline: x,
    backgroundImage: S,
    pie: M,
    stripe: _,
    compound: D,
    // edge props
    edgeLine: A,
    edgeArrow: L,
    core: R
  }, O = lt.propertyGroupNames = {}, $ = lt.propertyGroupKeys = Object.keys(V);
  $.forEach(function(U) {
    O[U] = V[U].map(function(X) {
      return X.name;
    }), V[U].forEach(function(X) {
      return X.groupKey = U;
    });
  });
  var Q = lt.aliases = [{
    name: "content",
    pointsTo: "label"
  }, {
    name: "control-point-distance",
    pointsTo: "control-point-distances"
  }, {
    name: "control-point-weight",
    pointsTo: "control-point-weights"
  }, {
    name: "segment-distance",
    pointsTo: "segment-distances"
  }, {
    name: "segment-weight",
    pointsTo: "segment-weights"
  }, {
    name: "segment-radius",
    pointsTo: "segment-radii"
  }, {
    name: "edge-text-rotation",
    pointsTo: "text-rotation"
  }, {
    name: "padding-left",
    pointsTo: "padding"
  }, {
    name: "padding-right",
    pointsTo: "padding"
  }, {
    name: "padding-top",
    pointsTo: "padding"
  }, {
    name: "padding-bottom",
    pointsTo: "padding"
  }];
  lt.propertyNames = H.map(function(U) {
    return U.name;
  });
  for (var se = 0; se < H.length; se++) {
    var ae = H[se];
    H[ae.name] = ae;
  }
  for (var le = 0; le < Q.length; le++) {
    var ce = Q[le], he = H[ce.pointsTo], ie = {
      name: ce.name,
      alias: !0,
      pointsTo: he
    };
    H.push(ie), H[ce.name] = ie;
  }
})();
lt.getDefaultProperty = function(t) {
  return this.getDefaultProperties()[t];
};
lt.getDefaultProperties = function() {
  var t = this._private;
  if (t.defaultProperties != null)
    return t.defaultProperties;
  for (var e = Ae({
    // core props
    "selection-box-color": "#ddd",
    "selection-box-opacity": 0.65,
    "selection-box-border-color": "#aaa",
    "selection-box-border-width": 1,
    "active-bg-color": "black",
    "active-bg-opacity": 0.15,
    "active-bg-size": 30,
    "outside-texture-bg-color": "#000",
    "outside-texture-bg-opacity": 0.125,
    // common node/edge props
    events: "yes",
    "text-events": "no",
    "text-valign": "top",
    "text-halign": "center",
    "text-justification": "auto",
    "line-height": 1,
    color: "#000",
    "box-selection": "contain",
    "text-outline-color": "#000",
    "text-outline-width": 0,
    "text-outline-opacity": 1,
    "text-opacity": 1,
    "text-decoration": "none",
    "text-transform": "none",
    "text-wrap": "none",
    "text-overflow-wrap": "whitespace",
    "text-max-width": 9999,
    "text-background-color": "#000",
    "text-background-opacity": 0,
    "text-background-shape": "rectangle",
    "text-background-padding": 0,
    "text-border-opacity": 0,
    "text-border-width": 0,
    "text-border-style": "solid",
    "text-border-color": "#000",
    "font-family": "Helvetica Neue, Helvetica, sans-serif",
    "font-style": "normal",
    "font-weight": "normal",
    "font-size": 16,
    "min-zoomed-font-size": 0,
    "text-rotation": "none",
    "source-text-rotation": "none",
    "target-text-rotation": "none",
    visibility: "visible",
    display: "element",
    opacity: 1,
    "z-compound-depth": "auto",
    "z-index-compare": "auto",
    "z-index": 0,
    label: "",
    "text-margin-x": 0,
    "text-margin-y": 0,
    "source-label": "",
    "source-text-offset": 0,
    "source-text-margin-x": 0,
    "source-text-margin-y": 0,
    "target-label": "",
    "target-text-offset": 0,
    "target-text-margin-x": 0,
    "target-text-margin-y": 0,
    "overlay-opacity": 0,
    "overlay-color": "#000",
    "overlay-padding": 10,
    "overlay-shape": "round-rectangle",
    "overlay-corner-radius": "auto",
    "underlay-opacity": 0,
    "underlay-color": "#000",
    "underlay-padding": 10,
    "underlay-shape": "round-rectangle",
    "underlay-corner-radius": "auto",
    "text-metrics": "font",
    "transition-property": "none",
    "transition-duration": 0,
    "transition-delay": 0,
    "transition-timing-function": "linear",
    "box-select-labels": "no",
    // node props
    "background-blacken": 0,
    "background-color": "#999",
    "background-fill": "solid",
    "background-opacity": 1,
    "background-image": "none",
    "background-image-crossorigin": "anonymous",
    "background-image-opacity": 1,
    "background-image-containment": "inside",
    "background-image-smoothing": "yes",
    "background-position-x": "50%",
    "background-position-y": "50%",
    "background-offset-x": 0,
    "background-offset-y": 0,
    "background-width-relative-to": "include-padding",
    "background-height-relative-to": "include-padding",
    "background-repeat": "no-repeat",
    "background-fit": "none",
    "background-clip": "node",
    "background-width": "auto",
    "background-height": "auto",
    "border-color": "#000",
    "border-opacity": 1,
    "border-width": 0,
    "border-style": "solid",
    "border-dash-pattern": [4, 2],
    "border-dash-offset": 0,
    "border-cap": "butt",
    "border-join": "miter",
    "border-position": "center",
    "outline-color": "#999",
    "outline-opacity": 1,
    "outline-width": 0,
    "outline-offset": 0,
    "outline-style": "solid",
    height: 30,
    width: 30,
    shape: "ellipse",
    "shape-polygon-points": "-1, -1,   1, -1,   1, 1,   -1, 1",
    "corner-radius": "auto",
    "bounds-expansion": 0,
    // node gradient
    "background-gradient-direction": "to-bottom",
    "background-gradient-stop-colors": "#999",
    "background-gradient-stop-positions": "0%",
    // ghost props
    ghost: "no",
    "ghost-offset-y": 0,
    "ghost-offset-x": 0,
    "ghost-opacity": 0,
    // compound props
    padding: 0,
    "padding-relative-to": "width",
    position: "origin",
    "compound-sizing-wrt-labels": "include",
    "min-width": 0,
    "min-width-bias-left": 0,
    "min-width-bias-right": 0,
    "min-height": 0,
    "min-height-bias-top": 0,
    "min-height-bias-bottom": 0
  }, {
    // node pie bg
    "pie-size": "100%",
    "pie-hole": 0,
    "pie-start-angle": "0deg"
  }, [{
    name: "pie-{{i}}-background-color",
    value: "black"
  }, {
    name: "pie-{{i}}-background-size",
    value: "0%"
  }, {
    name: "pie-{{i}}-background-opacity",
    value: 1
  }].reduce(function(l, u) {
    for (var f = 1; f <= lt.pieBackgroundN; f++) {
      var c = u.name.replace("{{i}}", f), v = u.value;
      l[c] = v;
    }
    return l;
  }, {}), {
    // node stripes bg
    "stripe-size": "100%",
    "stripe-direction": "horizontal"
  }, [{
    name: "stripe-{{i}}-background-color",
    value: "black"
  }, {
    name: "stripe-{{i}}-background-size",
    value: "0%"
  }, {
    name: "stripe-{{i}}-background-opacity",
    value: 1
  }].reduce(function(l, u) {
    for (var f = 1; f <= lt.stripeBackgroundN; f++) {
      var c = u.name.replace("{{i}}", f), v = u.value;
      l[c] = v;
    }
    return l;
  }, {}), {
    // edge props
    "line-style": "solid",
    "line-color": "#999",
    "line-fill": "solid",
    "line-cap": "butt",
    "line-opacity": 1,
    "line-outline-width": 0,
    "line-outline-color": "#000",
    "line-gradient-stop-colors": "#999",
    "line-gradient-stop-positions": "0%",
    "control-point-step-size": 40,
    "control-point-weights": 0.5,
    "segment-weights": 0.5,
    "segment-distances": 20,
    "segment-radii": 15,
    "radius-type": "arc-radius",
    "taxi-turn": "50%",
    "taxi-radius": 15,
    "taxi-turn-min-distance": 10,
    "taxi-direction": "auto",
    "edge-distances": "intersection",
    "curve-style": "haystack",
    "haystack-radius": 0,
    "arrow-scale": 1,
    "loop-direction": "-45deg",
    "loop-sweep": "-90deg",
    "source-distance-from-node": 0,
    "target-distance-from-node": 0,
    "source-endpoint": "outside-to-node",
    "target-endpoint": "outside-to-node",
    "line-dash-pattern": [6, 3],
    "line-dash-offset": 0
  }, [{
    name: "arrow-shape",
    value: "none"
  }, {
    name: "arrow-color",
    value: "#999"
  }, {
    name: "arrow-fill",
    value: "filled"
  }, {
    name: "arrow-width",
    value: 1
  }].reduce(function(l, u) {
    return lt.arrowPrefixes.forEach(function(f) {
      var c = f + "-" + u.name, v = u.value;
      l[c] = v;
    }), l;
  }, {})), r = {}, n = 0; n < this.properties.length; n++) {
    var a = this.properties[n];
    if (!a.pointsTo) {
      var i = a.name, s = e[i], o = this.parse(i, s);
      r[i] = o;
    }
  }
  return t.defaultProperties = r, t.defaultProperties;
};
lt.addDefaultStylesheet = function() {
  this.selector(":parent").css({
    shape: "rectangle",
    padding: 10,
    "background-color": "#eee",
    "border-color": "#ccc",
    "border-width": 1
  }).selector("edge").css({
    width: 3
  }).selector(":loop").css({
    "curve-style": "bezier"
  }).selector("edge:compound").css({
    "curve-style": "bezier",
    "source-endpoint": "outside-to-line",
    "target-endpoint": "outside-to-line"
  }).selector(":selected").css({
    "background-color": "#0169D9",
    "line-color": "#0169D9",
    "source-arrow-color": "#0169D9",
    "target-arrow-color": "#0169D9",
    "mid-source-arrow-color": "#0169D9",
    "mid-target-arrow-color": "#0169D9"
  }).selector(":parent:selected").css({
    "background-color": "#CCE1F9",
    "border-color": "#aec8e5"
  }).selector(":active").css({
    "overlay-color": "black",
    "overlay-padding": 10,
    "overlay-opacity": 0.25
  }), this.defaultLength = this.length;
};
var ao = {};
ao.parse = function(t, e, r, n) {
  var a = this;
  if (tt(e))
    return a.parseImplWarn(t, e, r, n);
  var i = n === "mapping" || n === !0 || n === !1 || n == null ? "dontcare" : n, s = r ? "t" : "f", o = "" + e, l = Qh(t, o, s, i), u = a.propCache = a.propCache || [], f;
  return (f = u[l]) || (f = u[l] = a.parseImplWarn(t, e, r, n)), (r || n === "mapping") && (f = xr(f), f && (f.value = xr(f.value))), f;
};
ao.parseImplWarn = function(t, e, r, n) {
  var a = this.parseImpl(t, e, r, n);
  return !a && e != null && $e("The style property `".concat(t, ": ").concat(e, "` is invalid")), a && (a.name === "width" || a.name === "height") && e === "label" && $e("The style value of `label` is deprecated for `" + a.name + "`"), a;
};
ao.parseImpl = function(t, e, r, n) {
  var a = this;
  t = Ku(t);
  var i = a.properties[t], s = e, o = a.types;
  if (!i || e === void 0)
    return null;
  i.alias && (i = i.pointsTo, t = i.name);
  var l = Se(e);
  l && (e = e.trim());
  var u = i.type;
  if (!u)
    return null;
  if (r && (e === "" || e === null))
    return {
      name: t,
      value: e,
      bypass: !0,
      deleteBypass: !0
    };
  if (tt(e))
    return {
      name: t,
      value: e,
      strValue: "fn",
      mapped: o.fn,
      bypass: r
    };
  var f, c;
  if (!(!l || n || e.length < 7 || e[1] !== "a")) {
    if (e.length >= 7 && e[0] === "d" && (f = new RegExp(o.data.regex).exec(e))) {
      if (r)
        return !1;
      var v = o.data;
      return {
        name: t,
        value: f,
        strValue: "" + e,
        mapped: v,
        field: f[1],
        bypass: r
      };
    } else if (e.length >= 10 && e[0] === "m" && (c = new RegExp(o.mapData.regex).exec(e))) {
      if (r || u.multiple)
        return !1;
      var d = o.mapData;
      if (!(u.color || u.number))
        return !1;
      var h = this.parse(t, c[4]);
      if (!h || h.mapped)
        return !1;
      var y = this.parse(t, c[5]);
      if (!y || y.mapped)
        return !1;
      if (h.pfValue === y.pfValue || h.strValue === y.strValue)
        return $e("`" + t + ": " + e + "` is not a valid mapper because the output range is zero; converting to `" + t + ": " + h.strValue + "`"), this.parse(t, h.strValue);
      if (u.color) {
        var g = h.value, p = y.value, m = g[0] === p[0] && g[1] === p[1] && g[2] === p[2] && // optional alpha
        (g[3] === p[3] || (g[3] == null || g[3] === 1) && (p[3] == null || p[3] === 1));
        if (m)
          return !1;
      }
      return {
        name: t,
        value: c,
        strValue: "" + e,
        mapped: d,
        field: c[1],
        fieldMin: parseFloat(c[2]),
        // min & max are numeric
        fieldMax: parseFloat(c[3]),
        valueMin: h.value,
        valueMax: y.value,
        bypass: r
      };
    }
  }
  if (u.multiple && n !== "multiple") {
    var b;
    if (l ? b = e.split(/\s+/) : We(e) ? b = e : b = [e], u.evenMultiple && b.length % 2 !== 0)
      return null;
    for (var w = [], E = [], T = [], x = "", S = !1, D = 0; D < b.length; D++) {
      var A = a.parse(t, b[D], r, "multiple");
      S = S || Se(A.value), w.push(A.value), T.push(A.pfValue != null ? A.pfValue : A.value), E.push(A.units), x += (D > 0 ? " " : "") + A.strValue;
    }
    return u.validate && !u.validate(w, E) ? null : u.singleEnum && S ? w.length === 1 && Se(w[0]) ? {
      name: t,
      value: w[0],
      strValue: w[0],
      bypass: r
    } : null : {
      name: t,
      value: w,
      pfValue: T,
      strValue: x,
      bypass: r,
      units: E
    };
  }
  var k = function() {
    for (var ie = 0; ie < u.enums.length; ie++) {
      var U = u.enums[ie];
      if (U === e)
        return {
          name: t,
          value: e,
          strValue: "" + e,
          bypass: r
        };
    }
    return null;
  };
  if (u.number) {
    var R, M = "px";
    if (u.units && (R = u.units), u.implicitUnits && (M = u.implicitUnits), !u.unitless)
      if (l) {
        var I = "px|em" + (u.allowPercent ? "|\\%" : "");
        R && (I = R);
        var _ = e.match("^(" + vt + ")(" + I + ")?$");
        _ && (e = _[1], R = _[2] || M);
      } else (!R || u.implicitUnits) && (R = M);
    if (e = parseFloat(e), isNaN(e) && u.enums === void 0)
      return null;
    if (isNaN(e) && u.enums !== void 0)
      return e = s, k();
    if (u.integer && !D0(e) || u.min !== void 0 && (e < u.min || u.strictMin && e === u.min) || u.max !== void 0 && (e > u.max || u.strictMax && e === u.max))
      return null;
    var N = {
      name: t,
      value: e,
      strValue: "" + e + (R || ""),
      units: R,
      bypass: r
    };
    return u.unitless || R !== "px" && R !== "em" ? N.pfValue = e : N.pfValue = R === "px" || !R ? e : this.getEmSizeInPixels() * e, (R === "ms" || R === "s") && (N.pfValue = R === "ms" ? e : 1e3 * e), (R === "deg" || R === "rad") && (N.pfValue = R === "rad" ? e : _b(e)), R === "%" && (N.pfValue = e / 100), N;
  } else if (u.propList) {
    var L = [], F = "" + e;
    if (F !== "none") {
      for (var H = F.split(/\s*,\s*|\s+/), V = 0; V < H.length; V++) {
        var O = H[V].trim();
        a.properties[O] ? L.push(O) : $e("`" + O + "` is not a valid property name");
      }
      if (L.length === 0)
        return null;
    }
    return {
      name: t,
      value: L,
      strValue: L.length === 0 ? "none" : L.join(" "),
      bypass: r
    };
  } else if (u.color) {
    var $ = Uh(e);
    return $ ? {
      name: t,
      value: $,
      pfValue: $,
      strValue: "rgb(" + $[0] + "," + $[1] + "," + $[2] + ")",
      // n.b. no spaces b/c of multiple support
      bypass: r
    } : null;
  } else if (u.regex || u.regexes) {
    if (u.enums) {
      var Q = k();
      if (Q)
        return Q;
    }
    for (var se = u.regexes ? u.regexes : [u.regex], ae = 0; ae < se.length; ae++) {
      var le = new RegExp(se[ae]), ce = le.exec(e);
      if (ce)
        return {
          name: t,
          value: u.singleRegexMatchValue ? ce[1] : ce,
          strValue: "" + e,
          bypass: r
        };
    }
    return null;
  } else return u.string ? {
    name: t,
    value: "" + e,
    strValue: "" + e,
    bypass: r
  } : u.enums ? k() : null;
};
var mt = function(e) {
  if (!(this instanceof mt))
    return new mt(e);
  if (!Wu(e)) {
    je("A style must have a core reference");
    return;
  }
  this._private = {
    cy: e,
    coreStyle: {}
  }, this.length = 0, this.resetToDefault();
}, Mt = mt.prototype;
Mt.instanceString = function() {
  return "style";
};
Mt.clear = function() {
  for (var t = this._private, e = t.cy, r = e.elements(), n = 0; n < this.length; n++)
    this[n] = void 0;
  return this.length = 0, t.contextStyles = {}, t.propDiffs = {}, this.cleanElements(r, !0), r.forEach(function(a) {
    var i = a[0]._private;
    i.styleDirty = !0, i.appliedInitStyle = !1;
  }), this;
};
Mt.resetToDefault = function() {
  return this.clear(), this.addDefaultStylesheet(), this;
};
Mt.core = function(t) {
  return this._private.coreStyle[t] || this.getDefaultProperty(t);
};
Mt.selector = function(t) {
  var e = t === "core" ? null : new on(t), r = this.length++;
  return this[r] = {
    selector: e,
    properties: [],
    mappedProperties: [],
    index: r
  }, this;
};
Mt.css = function() {
  var t = this, e = arguments;
  if (e.length === 1)
    for (var r = e[0], n = 0; n < t.properties.length; n++) {
      var a = t.properties[n], i = r[a.name];
      i === void 0 && (i = r[Us(a.name)]), i !== void 0 && this.cssRule(a.name, i);
    }
  else e.length === 2 && this.cssRule(e[0], e[1]);
  return this;
};
Mt.style = Mt.css;
Mt.cssRule = function(t, e) {
  var r = this.parse(t, e);
  if (r) {
    var n = this.length - 1;
    this[n].properties.push(r), this[n].properties[r.name] = r, r.name.match(/pie-(\d+)-background-size/) && r.value && (this._private.hasPie = !0), r.name.match(/stripe-(\d+)-background-size/) && r.value && (this._private.hasStripe = !0), r.mapped && this[n].mappedProperties.push(r);
    var a = !this[n].selector;
    a && (this._private.coreStyle[r.name] = r);
  }
  return this;
};
Mt.append = function(t) {
  return qh(t) ? t.appendToStyle(this) : We(t) ? this.appendFromJson(t) : Se(t) && this.appendFromString(t), this;
};
mt.fromJson = function(t, e) {
  var r = new mt(t);
  return r.fromJson(e), r;
};
mt.fromString = function(t, e) {
  return new mt(t).fromString(e);
};
[bt, Ci, uf, Tr, no, ff, lt, ao].forEach(function(t) {
  Ae(Mt, t);
});
mt.types = Mt.types;
mt.properties = Mt.properties;
mt.propertyGroups = Mt.propertyGroups;
mt.propertyGroupNames = Mt.propertyGroupNames;
mt.propertyGroupKeys = Mt.propertyGroupKeys;
var $x = {
  style: function(e) {
    if (e) {
      var r = this.setStyle(e);
      r.update();
    }
    return this._private.style;
  },
  setStyle: function(e) {
    var r = this._private;
    return qh(e) ? r.style = e.generateStyle(this) : We(e) ? r.style = mt.fromJson(this, e) : Se(e) ? r.style = mt.fromString(this, e) : r.style = mt(this), r.style;
  },
  // e.g. cy.data() changed => recalc ele mappers
  updateStyle: function() {
    this.mutableElements().updateStyle();
  }
}, Hx = "single", Ln = {
  autolock: function(e) {
    if (e !== void 0)
      this._private.autolock = !!e;
    else
      return this._private.autolock;
    return this;
  },
  autoungrabify: function(e) {
    if (e !== void 0)
      this._private.autoungrabify = !!e;
    else
      return this._private.autoungrabify;
    return this;
  },
  autounselectify: function(e) {
    if (e !== void 0)
      this._private.autounselectify = !!e;
    else
      return this._private.autounselectify;
    return this;
  },
  selectionType: function(e) {
    var r = this._private;
    if (r.selectionType == null && (r.selectionType = Hx), e !== void 0)
      (e === "additive" || e === "single") && (r.selectionType = e);
    else
      return r.selectionType;
    return this;
  },
  panningEnabled: function(e) {
    if (e !== void 0)
      this._private.panningEnabled = !!e;
    else
      return this._private.panningEnabled;
    return this;
  },
  userPanningEnabled: function(e) {
    if (e !== void 0)
      this._private.userPanningEnabled = !!e;
    else
      return this._private.userPanningEnabled;
    return this;
  },
  zoomingEnabled: function(e) {
    if (e !== void 0)
      this._private.zoomingEnabled = !!e;
    else
      return this._private.zoomingEnabled;
    return this;
  },
  userZoomingEnabled: function(e) {
    if (e !== void 0)
      this._private.userZoomingEnabled = !!e;
    else
      return this._private.userZoomingEnabled;
    return this;
  },
  boxSelectionEnabled: function(e) {
    if (e !== void 0)
      this._private.boxSelectionEnabled = !!e;
    else
      return this._private.boxSelectionEnabled;
    return this;
  },
  pan: function() {
    var e = arguments, r = this._private.pan, n, a, i, s, o;
    switch (e.length) {
      case 0:
        return r;
      case 1:
        if (Se(e[0]))
          return n = e[0], r[n];
        if (_e(e[0])) {
          if (!this._private.panningEnabled)
            return this;
          i = e[0], s = i.x, o = i.y, pe(s) && (r.x = s), pe(o) && (r.y = o), this.emit("pan viewport");
        }
        break;
      case 2:
        if (!this._private.panningEnabled)
          return this;
        n = e[0], a = e[1], (n === "x" || n === "y") && pe(a) && (r[n] = a), this.emit("pan viewport");
        break;
    }
    return this.notify("viewport"), this;
  },
  panBy: function(e, r) {
    var n = arguments, a = this._private.pan, i, s, o, l, u;
    if (!this._private.panningEnabled)
      return this;
    switch (n.length) {
      case 1:
        _e(e) && (o = n[0], l = o.x, u = o.y, pe(l) && (a.x += l), pe(u) && (a.y += u), this.emit("pan viewport"));
        break;
      case 2:
        i = e, s = r, (i === "x" || i === "y") && pe(s) && (a[i] += s), this.emit("pan viewport");
        break;
    }
    return this.notify("viewport"), this;
  },
  gc: function() {
    this.notify("gc");
  },
  fit: function(e, r) {
    var n = this.getFitViewport(e, r);
    if (n) {
      var a = this._private;
      a.zoom = n.zoom, a.pan = n.pan, this.emit("pan zoom viewport"), this.notify("viewport");
    }
    return this;
  },
  getFitViewport: function(e, r) {
    if (pe(e) && r === void 0 && (r = e, e = void 0), !(!this._private.panningEnabled || !this._private.zoomingEnabled)) {
      var n;
      if (Se(e)) {
        var a = e;
        e = this.$(a);
      } else if (B0(e)) {
        var i = e;
        n = {
          x1: i.x1,
          y1: i.y1,
          x2: i.x2,
          y2: i.y2
        }, n.w = n.x2 - n.x1, n.h = n.y2 - n.y1;
      } else Xt(e) || (e = this.mutableElements());
      if (!(Xt(e) && e.empty())) {
        n = n || e.boundingBox();
        var s = this.width(), o = this.height(), l;
        if (r = pe(r) ? r : 0, !isNaN(s) && !isNaN(o) && s > 0 && o > 0 && !isNaN(n.w) && !isNaN(n.h) && n.w > 0 && n.h > 0) {
          l = Math.min((s - 2 * r) / n.w, (o - 2 * r) / n.h), l = l > this._private.maxZoom ? this._private.maxZoom : l, l = l < this._private.minZoom ? this._private.minZoom : l;
          var u = {
            // now pan to middle
            x: (s - l * (n.x1 + n.x2)) / 2,
            y: (o - l * (n.y1 + n.y2)) / 2
          };
          return {
            zoom: l,
            pan: u
          };
        }
      }
    }
  },
  zoomRange: function(e, r) {
    var n = this._private;
    if (r == null) {
      var a = e;
      e = a.min, r = a.max;
    }
    return pe(e) && pe(r) && e <= r ? (n.minZoom = e, n.maxZoom = r) : pe(e) && r === void 0 && e <= n.maxZoom ? n.minZoom = e : pe(r) && e === void 0 && r >= n.minZoom && (n.maxZoom = r), this;
  },
  minZoom: function(e) {
    return e === void 0 ? this._private.minZoom : this.zoomRange({
      min: e
    });
  },
  maxZoom: function(e) {
    return e === void 0 ? this._private.maxZoom : this.zoomRange({
      max: e
    });
  },
  getZoomedViewport: function(e) {
    var r = this._private, n = r.pan, a = r.zoom, i, s, o = !1;
    if (r.zoomingEnabled || (o = !0), pe(e) ? s = e : _e(e) && (s = e.level, e.position != null ? i = Ks(e.position, a, n) : e.renderedPosition != null && (i = e.renderedPosition), i != null && !r.panningEnabled && (o = !0)), s = s > r.maxZoom ? r.maxZoom : s, s = s < r.minZoom ? r.minZoom : s, o || !pe(s) || s === a || i != null && (!pe(i.x) || !pe(i.y)))
      return null;
    if (i != null) {
      var l = n, u = a, f = s, c = {
        x: -f / u * (i.x - l.x) + i.x,
        y: -f / u * (i.y - l.y) + i.y
      };
      return {
        zoomed: !0,
        panned: !0,
        zoom: f,
        pan: c
      };
    } else
      return {
        zoomed: !0,
        panned: !1,
        zoom: s,
        pan: n
      };
  },
  zoom: function(e) {
    if (e === void 0)
      return this._private.zoom;
    var r = this.getZoomedViewport(e), n = this._private;
    return r == null || !r.zoomed ? this : (n.zoom = r.zoom, r.panned && (n.pan.x = r.pan.x, n.pan.y = r.pan.y), this.emit("zoom" + (r.panned ? " pan" : "") + " viewport"), this.notify("viewport"), this);
  },
  viewport: function(e) {
    var r = this._private, n = !0, a = !0, i = [], s = !1, o = !1;
    if (!e)
      return this;
    if (pe(e.zoom) || (n = !1), _e(e.pan) || (a = !1), !n && !a)
      return this;
    if (n) {
      var l = e.zoom;
      l < r.minZoom || l > r.maxZoom || !r.zoomingEnabled ? s = !0 : (r.zoom = l, i.push("zoom"));
    }
    if (a && (!s || !e.cancelOnFailedZoom) && r.panningEnabled) {
      var u = e.pan;
      pe(u.x) && (r.pan.x = u.x, o = !1), pe(u.y) && (r.pan.y = u.y, o = !1), o || i.push("pan");
    }
    return i.length > 0 && (i.push("viewport"), this.emit(i.join(" ")), this.notify("viewport")), this;
  },
  center: function(e) {
    var r = this.getCenterPan(e);
    return r && (this._private.pan = r, this.emit("pan viewport"), this.notify("viewport")), this;
  },
  getCenterPan: function(e, r) {
    if (this._private.panningEnabled) {
      if (Se(e)) {
        var n = e;
        e = this.mutableElements().filter(n);
      } else Xt(e) || (e = this.mutableElements());
      if (e.length !== 0) {
        var a = e.boundingBox(), i = this.width(), s = this.height();
        r = r === void 0 ? this._private.zoom : r;
        var o = {
          // middle
          x: (i - r * (a.x1 + a.x2)) / 2,
          y: (s - r * (a.y1 + a.y2)) / 2
        };
        return o;
      }
    }
  },
  reset: function() {
    return !this._private.panningEnabled || !this._private.zoomingEnabled ? this : (this.viewport({
      pan: {
        x: 0,
        y: 0
      },
      zoom: 1
    }), this);
  },
  invalidateSize: function() {
    this._private.sizeCache = null;
  },
  size: function() {
    var e = this._private, r = e.container, n = this;
    return e.sizeCache = e.sizeCache || (r ? (function() {
      var a = n.window().getComputedStyle(r), i = function(o) {
        return parseFloat(a.getPropertyValue(o));
      };
      return {
        width: r.clientWidth - i("padding-left") - i("padding-right"),
        height: r.clientHeight - i("padding-top") - i("padding-bottom")
      };
    })() : {
      // fallback if no container (not 0 b/c can be used for dividing etc)
      width: 1,
      height: 1
    });
  },
  width: function() {
    return this.size().width;
  },
  height: function() {
    return this.size().height;
  },
  extent: function() {
    var e = this._private.pan, r = this._private.zoom, n = this.renderedExtent(), a = {
      x1: (n.x1 - e.x) / r,
      x2: (n.x2 - e.x) / r,
      y1: (n.y1 - e.y) / r,
      y2: (n.y2 - e.y) / r
    };
    return a.w = a.x2 - a.x1, a.h = a.y2 - a.y1, a;
  },
  renderedExtent: function() {
    var e = this.width(), r = this.height();
    return {
      x1: 0,
      y1: 0,
      x2: e,
      y2: r,
      w: e,
      h: r
    };
  },
  multiClickDebounceTime: function(e) {
    if (e) this._private.multiClickDebounceTime = e;
    else return this._private.multiClickDebounceTime;
    return this;
  }
};
Ln.centre = Ln.center;
Ln.autolockNodes = Ln.autolock;
Ln.autoungrabifyNodes = Ln.autoungrabify;
var li = {
  data: qe.data({
    field: "data",
    bindingEvent: "data",
    allowBinding: !0,
    allowSetting: !0,
    settingEvent: "data",
    settingTriggersEvent: !0,
    triggerFnName: "trigger",
    allowGetting: !0,
    updateStyle: !0
  }),
  removeData: qe.removeData({
    field: "data",
    event: "data",
    triggerFnName: "trigger",
    triggerEvent: !0,
    updateStyle: !0
  }),
  scratch: qe.data({
    field: "scratch",
    bindingEvent: "scratch",
    allowBinding: !0,
    allowSetting: !0,
    settingEvent: "scratch",
    settingTriggersEvent: !0,
    triggerFnName: "trigger",
    allowGetting: !0,
    updateStyle: !0
  }),
  removeScratch: qe.removeData({
    field: "scratch",
    event: "scratch",
    triggerFnName: "trigger",
    triggerEvent: !0,
    updateStyle: !0
  })
};
li.attr = li.data;
li.removeAttr = li.removeData;
var ui = function(e) {
  var r = this;
  e = Ae({}, e);
  var n = e.container;
  n && !ms(n) && ms(n[0]) && (n = n[0]);
  var a = n ? n._cyreg : null;
  a = a || {}, a && a.cy && (a.cy.destroy(), a = {});
  var i = a.readies = a.readies || [];
  n && (n._cyreg = a), a.cy = r;
  var s = ct !== void 0 && n !== void 0 && !e.headless, o = e;
  o.layout = Ae({
    name: s ? "grid" : "null"
  }, o.layout), o.renderer = Ae({
    name: s ? "canvas" : "null"
  }, o.renderer);
  var l = function(h, y, g) {
    return y !== void 0 ? y : g !== void 0 ? g : h;
  }, u = this._private = {
    container: n,
    // html dom ele container
    ready: !1,
    // whether ready has been triggered
    options: o,
    // cached options
    elements: new Ct(this),
    // elements in the graph
    listeners: [],
    // list of listeners
    aniEles: new Ct(this),
    // elements being animated
    data: o.data || {},
    // data for the core
    scratch: {},
    // scratch object for core
    layout: null,
    renderer: null,
    destroyed: !1,
    // whether destroy was called
    notificationsEnabled: !0,
    // whether notifications are sent to the renderer
    minZoom: 1e-50,
    maxZoom: 1e50,
    zoomingEnabled: l(!0, o.zoomingEnabled),
    userZoomingEnabled: l(!0, o.userZoomingEnabled),
    panningEnabled: l(!0, o.panningEnabled),
    userPanningEnabled: l(!0, o.userPanningEnabled),
    boxSelectionEnabled: l(!0, o.boxSelectionEnabled),
    autolock: l(!1, o.autolock, o.autolockNodes),
    autoungrabify: l(!1, o.autoungrabify, o.autoungrabifyNodes),
    autounselectify: l(!1, o.autounselectify),
    styleEnabled: o.styleEnabled === void 0 ? s : o.styleEnabled,
    zoom: pe(o.zoom) ? o.zoom : 1,
    pan: {
      x: _e(o.pan) && pe(o.pan.x) ? o.pan.x : 0,
      y: _e(o.pan) && pe(o.pan.y) ? o.pan.y : 0
    },
    animation: {
      // object for currently-running animations
      current: [],
      queue: []
    },
    hasCompoundNodes: !1,
    multiClickDebounceTime: l(250, o.multiClickDebounceTime)
  };
  this.createEmitter(), this.selectionType(o.selectionType), this.zoomRange({
    min: o.minZoom,
    max: o.maxZoom
  });
  var f = function(h, y) {
    var g = h.some(R0);
    if (g)
      return ya.all(h).then(y);
    y(h);
  };
  u.styleEnabled && r.setStyle([]);
  var c = Ae({}, o, o.renderer);
  r.initRenderer(c);
  var v = function(h, y, g) {
    r.notifications(!1);
    var p = r.mutableElements();
    p.length > 0 && p.remove(), h != null && (_e(h) || We(h)) && r.add(h), r.one("layoutready", function(b) {
      r.notifications(!0), r.emit(b), r.one("load", y), r.emitAndNotify("load");
    }).one("layoutstop", function() {
      r.one("done", g), r.emit("done");
    });
    var m = Ae({}, r._private.options.layout);
    m.eles = r.elements(), r.layout(m).run();
  };
  f([o.style, o.elements], function(d) {
    var h = d[0], y = d[1];
    u.styleEnabled && r.style().append(h), v(y, function() {
      r.startAnimationLoop(), u.ready = !0, tt(o.ready) && r.on("ready", o.ready);
      for (var g = 0; g < i.length; g++) {
        var p = i[g];
        r.on("ready", p);
      }
      a && (a.readies = []), r.emit("ready");
    }, o.done);
  });
}, Ss = ui.prototype;
Ae(Ss, {
  instanceString: function() {
    return "core";
  },
  isReady: function() {
    return this._private.ready;
  },
  destroyed: function() {
    return this._private.destroyed;
  },
  ready: function(e) {
    return this.isReady() ? this.emitter().emit("ready", [], e) : this.on("ready", e), this;
  },
  destroy: function() {
    var e = this;
    if (!e.destroyed())
      return e.stopAnimationLoop(), e.destroyRenderer(), this.emit("destroy"), e._private.destroyed = !0, e;
  },
  hasElementWithId: function(e) {
    return this._private.elements.hasElementWithId(e);
  },
  getElementById: function(e) {
    return this._private.elements.getElementById(e);
  },
  hasCompoundNodes: function() {
    return this._private.hasCompoundNodes;
  },
  headless: function() {
    return this._private.renderer.isHeadless();
  },
  styleEnabled: function() {
    return this._private.styleEnabled;
  },
  addToPool: function(e) {
    return this._private.elements.merge(e), this;
  },
  removeFromPool: function(e) {
    return this._private.elements.unmerge(e), this;
  },
  container: function() {
    return this._private.container || null;
  },
  window: function() {
    var e = this._private.container;
    if (e == null) return ct;
    var r = this._private.container.ownerDocument;
    return r === void 0 || r == null ? ct : r.defaultView || ct;
  },
  mount: function(e) {
    if (e != null) {
      var r = this, n = r._private, a = n.options;
      return !ms(e) && ms(e[0]) && (e = e[0]), r.stopAnimationLoop(), r.destroyRenderer(), n.container = e, n.styleEnabled = !0, r.invalidateSize(), r.initRenderer(Ae({}, a, a.renderer, {
        // allow custom renderer name to be re-used, otherwise use canvas
        name: a.renderer.name === "null" ? "canvas" : a.renderer.name
      })), r.startAnimationLoop(), r.style(a.style), r.emit("mount"), r;
    }
  },
  unmount: function() {
    var e = this;
    return e.stopAnimationLoop(), e.destroyRenderer(), e.initRenderer({
      name: "null"
    }), e.emit("unmount"), e;
  },
  options: function() {
    return xr(this._private.options);
  },
  json: function(e) {
    var r = this, n = r._private, a = r.mutableElements(), i = function(w) {
      return r.getElementById(w.id());
    };
    if (_e(e)) {
      if (r.startBatch(), e.elements) {
        var s = {}, o = function(w, E) {
          for (var T = [], x = [], S = 0; S < w.length; S++) {
            var D = w[S];
            if (!D.data.id) {
              $e("cy.json() cannot handle elements without an ID attribute");
              continue;
            }
            var A = "" + D.data.id, k = r.getElementById(A);
            s[A] = !0, k.length !== 0 ? x.push({
              ele: k,
              json: D
            }) : (E && (D.group = E), T.push(D));
          }
          r.add(T);
          for (var R = 0; R < x.length; R++) {
            var M = x[R], I = M.ele, _ = M.json;
            I.json(_);
          }
        };
        if (We(e.elements))
          o(e.elements);
        else
          for (var l = ["nodes", "edges"], u = 0; u < l.length; u++) {
            var f = l[u], c = e.elements[f];
            We(c) && o(c, f);
          }
        var v = r.collection();
        a.filter(function(b) {
          return !s[b.id()];
        }).forEach(function(b) {
          b.isParent() ? v.merge(b) : b.remove();
        }), v.forEach(function(b) {
          return b.children().move({
            parent: null
          });
        }), v.forEach(function(b) {
          return i(b).remove();
        });
      }
      e.style && r.style(e.style), e.zoom != null && e.zoom !== n.zoom && r.zoom(e.zoom), e.pan && (e.pan.x !== n.pan.x || e.pan.y !== n.pan.y) && r.pan(e.pan), e.data && r.data(e.data);
      for (var d = ["minZoom", "maxZoom", "zoomingEnabled", "userZoomingEnabled", "panningEnabled", "userPanningEnabled", "boxSelectionEnabled", "autolock", "autoungrabify", "autounselectify", "multiClickDebounceTime"], h = 0; h < d.length; h++) {
        var y = d[h];
        e[y] != null && r[y](e[y]);
      }
      return r.endBatch(), this;
    } else {
      var g = !!e, p = {};
      g ? p.elements = this.elements().map(function(b) {
        return b.json();
      }) : (p.elements = {}, a.forEach(function(b) {
        var w = b.group();
        p.elements[w] || (p.elements[w] = []), p.elements[w].push(b.json());
      })), this._private.styleEnabled && (p.style = r.style().json()), p.data = xr(r.data());
      var m = n.options;
      return p.zoomingEnabled = n.zoomingEnabled, p.userZoomingEnabled = n.userZoomingEnabled, p.zoom = n.zoom, p.minZoom = n.minZoom, p.maxZoom = n.maxZoom, p.panningEnabled = n.panningEnabled, p.userPanningEnabled = n.userPanningEnabled, p.pan = xr(n.pan), p.boxSelectionEnabled = n.boxSelectionEnabled, p.renderer = xr(m.renderer), p.hideEdgesOnViewport = m.hideEdgesOnViewport, p.textureOnViewport = m.textureOnViewport, p.wheelSensitivity = m.wheelSensitivity, p.motionBlur = m.motionBlur, p.multiClickDebounceTime = m.multiClickDebounceTime, p;
    }
  }
});
Ss.$id = Ss.getElementById;
[Mx, _x, qg, mu, ns, zx, bu, as, $x, Ln, li].forEach(function(t) {
  Ae(Ss, t);
});
var Ux = {
  fit: !0,
  // whether to fit the viewport to the graph
  directed: !1,
  // whether the tree is directed downwards (or edges can point in any direction if false)
  direction: "downward",
  // determines the direction in which the tree structure is drawn.  The possible values are 'downward', 'upward', 'rightward', or 'leftward'.
  padding: 30,
  // padding on fit
  circle: !1,
  // put depths in concentric circles if true, put depths top down if false
  grid: !1,
  // whether to create an even grid into which the DAG is placed (circle:false only)
  spacingFactor: 1.75,
  // positive spacing factor, larger => more space between nodes (N.B. n/a if causes overlap)
  boundingBox: void 0,
  // constrain layout bounds; { x1, y1, x2, y2 } or { x1, y1, w, h }
  avoidOverlap: !0,
  // prevents node overlap, may overflow boundingBox if not enough space
  nodeDimensionsIncludeLabels: !1,
  // Excludes the label when calculating node bounding boxes for the layout algorithm
  roots: void 0,
  // the roots of the trees
  depthSort: void 0,
  // a sorting function to order nodes at equal depth. e.g. function(a, b){ return a.data('weight') - b.data('weight') }
  animate: !1,
  // whether to transition the node positions
  animationDuration: 500,
  // duration of animation in ms if enabled
  animationEasing: void 0,
  // easing of animation if enabled,
  animateFilter: function(e, r) {
    return !0;
  },
  // a function that determines whether the node should be animated.  All nodes animated by default on animate enabled.  Non-animated nodes are positioned immediately when the layout starts
  ready: void 0,
  // callback on layoutready
  stop: void 0,
  // callback on layoutstop
  transform: function(e, r) {
    return r;
  }
  // transform a given node position. Useful for changing flow direction in discrete layouts
}, Gx = {
  maximal: !1,
  // whether to shift nodes down their natural BFS depths in order to avoid upwards edges (DAGS only); setting acyclic to true sets maximal to true also
  acyclic: !1
  // whether the tree is acyclic and thus a node could be shifted (due to the maximal option) multiple times without causing an infinite loop; setting to true sets maximal to true also; if you are uncertain whether a tree is acyclic, set to false to avoid potential infinite loops
}, Hn = function(e) {
  return e.scratch("breadthfirst");
}, ed = function(e, r) {
  return e.scratch("breadthfirst", r);
};
function $g(t) {
  this.options = Ae({}, Ux, Gx, t);
}
$g.prototype.run = function() {
  var t = this.options, e = t.cy, r = t.eles, n = r.nodes().filter(function(te) {
    return te.isChildless();
  }), a = r, i = t.directed, s = t.acyclic || t.maximal || t.maximalAdjustments > 0, o = !!t.boundingBox, l = zt(o ? t.boundingBox : structuredClone(e.extent())), u;
  if (Xt(t.roots))
    u = t.roots;
  else if (We(t.roots)) {
    for (var f = [], c = 0; c < t.roots.length; c++) {
      var v = t.roots[c], d = e.getElementById(v);
      f.push(d);
    }
    u = e.collection(f);
  } else if (Se(t.roots))
    u = e.$(t.roots);
  else if (i)
    u = n.roots();
  else {
    var h = r.components();
    u = e.collection();
    for (var y = function() {
      var Y = h[g], K = Y.maxDegree(!1), ue = Y.filter(function(oe) {
        return oe.degree(!1) === K;
      });
      u = u.add(ue);
    }, g = 0; g < h.length; g++)
      y();
  }
  var p = [], m = {}, b = function(Y, K) {
    p[K] == null && (p[K] = []);
    var ue = p[K].length;
    p[K].push(Y), ed(Y, {
      index: ue,
      depth: K
    });
  }, w = function(Y, K) {
    var ue = Hn(Y), oe = ue.depth, ve = ue.index;
    p[oe][ve] = null, Y.isChildless() && b(Y, K);
  };
  a.bfs({
    roots: u,
    directed: t.directed,
    visit: function(Y, K, ue, oe, ve) {
      var de = Y[0], me = de.id();
      de.isChildless() && b(de, ve), m[me] = !0;
    }
  });
  for (var E = [], T = 0; T < n.length; T++) {
    var x = n[T];
    m[x.id()] || E.push(x);
  }
  var S = function(Y) {
    for (var K = p[Y], ue = 0; ue < K.length; ue++) {
      var oe = K[ue];
      if (oe == null) {
        K.splice(ue, 1), ue--;
        continue;
      }
      ed(oe, {
        depth: Y,
        index: ue
      });
    }
  }, D = function(Y, K) {
    for (var ue = Hn(Y), oe = Y.incomers().filter(function(J) {
      return J.isNode() && r.has(J);
    }), ve = -1, de = Y.id(), me = 0; me < oe.length; me++) {
      var Te = oe[me], Ee = Hn(Te);
      ve = Math.max(ve, Ee.depth);
    }
    if (ue.depth <= ve) {
      if (!t.acyclic && K[de])
        return null;
      var Pe = ve + 1;
      return w(Y, Pe), K[de] = Pe, !0;
    }
    return !1;
  };
  if (i && s) {
    var A = [], k = {}, R = function(Y) {
      return A.push(Y);
    }, M = function() {
      return A.shift();
    };
    for (n.forEach(function(te) {
      return A.push(te);
    }); A.length > 0; ) {
      var I = M(), _ = D(I, k);
      if (_)
        I.outgoers().filter(function(te) {
          return te.isNode() && r.has(te);
        }).forEach(R);
      else if (_ === null) {
        $e("Detected double maximal shift for node `" + I.id() + "`.  Bailing maximal adjustment due to cycle.  Use `options.maximal: true` only on DAGs.");
        break;
      }
    }
  }
  var N = 0;
  if (t.avoidOverlap)
    for (var L = 0; L < n.length; L++) {
      var F = n[L], H = F.layoutDimensions(t), V = H.w, O = H.h;
      N = Math.max(N, V, O);
    }
  var $ = {}, Q = function(Y) {
    if ($[Y.id()])
      return $[Y.id()];
    for (var K = Hn(Y).depth, ue = Y.neighborhood(), oe = 0, ve = 0, de = 0; de < ue.length; de++) {
      var me = ue[de];
      if (!(me.isEdge() || me.isParent() || !n.has(me))) {
        var Te = Hn(me);
        if (Te != null) {
          var Ee = Te.index, Pe = Te.depth;
          if (!(Ee == null || Pe == null)) {
            var J = p[Pe].length;
            Pe < K && (oe += Ee / J, ve++);
          }
        }
      }
    }
    return ve = Math.max(1, ve), oe = oe / ve, ve === 0 && (oe = 0), $[Y.id()] = oe, oe;
  }, se = function(Y, K) {
    var ue = Q(Y), oe = Q(K), ve = ue - oe;
    return ve === 0 ? Hh(Y.id(), K.id()) : ve;
  };
  t.depthSort !== void 0 && (se = t.depthSort);
  for (var ae = p.length, le = 0; le < ae; le++)
    p[le].sort(se), S(le);
  for (var ce = [], he = 0; he < E.length; he++)
    ce.push(E[he]);
  var ie = function() {
    for (var Y = 0; Y < ae; Y++)
      S(Y);
  };
  ce.length && (p.unshift(ce), ae = p.length, ie());
  for (var U = 0, X = 0; X < ae; X++)
    U = Math.max(p[X].length, U);
  var C = {
    x: l.x1 + l.w / 2,
    y: l.y1 + l.h / 2
  }, B = n.reduce(function(te, Y) {
    return (function(K) {
      return {
        w: te.w === -1 ? K.w : (te.w + K.w) / 2,
        h: te.h === -1 ? K.h : (te.h + K.h) / 2
      };
    })(Y.boundingBox({
      includeLabels: t.nodeDimensionsIncludeLabels
    }));
  }, {
    w: -1,
    h: -1
  }), z = Math.max(
    // only one depth
    ae === 1 ? 0 : (
      // inside a bounding box, no need for top & bottom padding
      o ? (l.h - t.padding * 2 - B.h) / (ae - 1) : (l.h - t.padding * 2 - B.h) / (ae + 1)
    ),
    N
  ), W = p.reduce(function(te, Y) {
    return Math.max(te, Y.length);
  }, 0), j = function(Y) {
    var K = Hn(Y), ue = K.depth, oe = K.index;
    if (t.circle) {
      var ve = Math.min(l.w / 2 / ae, l.h / 2 / ae);
      ve = Math.max(ve, N);
      var de = ve * ue + ve - (ae > 0 && p[0].length <= 3 ? ve / 2 : 0), me = 2 * Math.PI / p[ue].length * oe;
      return ue === 0 && p[0].length === 1 && (de = 1), {
        x: C.x + de * Math.cos(me),
        y: C.y + de * Math.sin(me)
      };
    } else {
      var Te = p[ue].length, Ee = Math.max(
        // only one depth
        Te === 1 ? 0 : (
          // inside a bounding box, no need for left & right padding
          o ? (l.w - t.padding * 2 - B.w) / ((t.grid ? W : Te) - 1) : (l.w - t.padding * 2 - B.w) / ((t.grid ? W : Te) + 1)
        ),
        N
      ), Pe = {
        x: C.x + (oe + 1 - (Te + 1) / 2) * Ee,
        y: C.y + (ue + 1 - (ae + 1) / 2) * z
      };
      return Pe;
    }
  }, Z = {
    downward: 0,
    leftward: 90,
    upward: 180,
    rightward: -90
  };
  Object.keys(Z).indexOf(t.direction) === -1 && je("Invalid direction '".concat(t.direction, "' specified for breadthfirst layout. Valid values are: ").concat(Object.keys(Z).join(", ")));
  var ne = function(Y) {
    return sb(j(Y), l, Z[t.direction]);
  };
  return r.nodes().layoutPositions(this, t, ne), this;
};
var Wx = {
  fit: !0,
  // whether to fit the viewport to the graph
  padding: 30,
  // the padding on fit
  boundingBox: void 0,
  // constrain layout bounds; { x1, y1, x2, y2 } or { x1, y1, w, h }
  avoidOverlap: !0,
  // prevents node overlap, may overflow boundingBox and radius if not enough space
  nodeDimensionsIncludeLabels: !1,
  // Excludes the label when calculating node bounding boxes for the layout algorithm
  spacingFactor: void 0,
  // Applies a multiplicative factor (>0) to expand or compress the overall area that the nodes take up
  radius: void 0,
  // the radius of the circle
  startAngle: 3 / 2 * Math.PI,
  // where nodes start in radians
  sweep: void 0,
  // how many radians should be between the first and last node (defaults to full circle)
  clockwise: !0,
  // whether the layout should go clockwise (true) or counterclockwise/anticlockwise (false)
  sort: void 0,
  // a sorting function to order the nodes; e.g. function(a, b){ return a.data('weight') - b.data('weight') }
  animate: !1,
  // whether to transition the node positions
  animationDuration: 500,
  // duration of animation in ms if enabled
  animationEasing: void 0,
  // easing of animation if enabled
  animateFilter: function(e, r) {
    return !0;
  },
  // a function that determines whether the node should be animated.  All nodes animated by default on animate enabled.  Non-animated nodes are positioned immediately when the layout starts
  ready: void 0,
  // callback on layoutready
  stop: void 0,
  // callback on layoutstop
  transform: function(e, r) {
    return r;
  }
  // transform a given node position. Useful for changing flow direction in discrete layouts 
};
function Hg(t) {
  this.options = Ae({}, Wx, t);
}
Hg.prototype.run = function() {
  var t = this.options, e = t, r = t.cy, n = e.eles, a = e.counterclockwise !== void 0 ? !e.counterclockwise : e.clockwise, i = n.nodes().not(":parent");
  e.sort && (i = i.sort(e.sort));
  for (var s = zt(e.boundingBox ? e.boundingBox : {
    x1: 0,
    y1: 0,
    w: r.width(),
    h: r.height()
  }), o = {
    x: s.x1 + s.w / 2,
    y: s.y1 + s.h / 2
  }, l = e.sweep === void 0 ? 2 * Math.PI - 2 * Math.PI / i.length : e.sweep, u = l / Math.max(1, i.length - 1), f, c = 0, v = 0; v < i.length; v++) {
    var d = i[v], h = d.layoutDimensions(e), y = h.w, g = h.h;
    c = Math.max(c, y, g);
  }
  if (pe(e.radius) ? f = e.radius : i.length <= 1 ? f = 0 : f = Math.min(s.h, s.w) / 2 - c, i.length > 1 && e.avoidOverlap) {
    c *= 1.75;
    var p = Math.cos(u) - Math.cos(0), m = Math.sin(u) - Math.sin(0), b = Math.sqrt(c * c / (p * p + m * m));
    f = Math.max(b, f);
  }
  var w = function(T, x) {
    var S = e.startAngle + x * u * (a ? 1 : -1), D = f * Math.cos(S), A = f * Math.sin(S), k = {
      x: o.x + D,
      y: o.y + A
    };
    return k;
  };
  return n.nodes().layoutPositions(this, e, w), this;
};
var Kx = {
  fit: !0,
  // whether to fit the viewport to the graph
  padding: 30,
  // the padding on fit
  startAngle: 3 / 2 * Math.PI,
  // where nodes start in radians
  sweep: void 0,
  // how many radians should be between the first and last node (defaults to full circle)
  clockwise: !0,
  // whether the layout should go clockwise (true) or counterclockwise/anticlockwise (false)
  equidistant: !1,
  // whether levels have an equal radial distance betwen them, may cause bounding box overflow
  minNodeSpacing: 10,
  // min spacing between outside of nodes (used for radius adjustment)
  boundingBox: void 0,
  // constrain layout bounds; { x1, y1, x2, y2 } or { x1, y1, w, h }
  avoidOverlap: !0,
  // prevents node overlap, may overflow boundingBox if not enough space
  nodeDimensionsIncludeLabels: !1,
  // Excludes the label when calculating node bounding boxes for the layout algorithm
  height: void 0,
  // height of layout area (overrides container height)
  width: void 0,
  // width of layout area (overrides container width)
  spacingFactor: void 0,
  // Applies a multiplicative factor (>0) to expand or compress the overall area that the nodes take up
  concentric: function(e) {
    return e.degree();
  },
  levelWidth: function(e) {
    return e.maxDegree() / 4;
  },
  animate: !1,
  // whether to transition the node positions
  animationDuration: 500,
  // duration of animation in ms if enabled
  animationEasing: void 0,
  // easing of animation if enabled
  animateFilter: function(e, r) {
    return !0;
  },
  // a function that determines whether the node should be animated.  All nodes animated by default on animate enabled.  Non-animated nodes are positioned immediately when the layout starts
  ready: void 0,
  // callback on layoutready
  stop: void 0,
  // callback on layoutstop
  transform: function(e, r) {
    return r;
  }
  // transform a given node position. Useful for changing flow direction in discrete layouts
};
function Ug(t) {
  this.options = Ae({}, Kx, t);
}
Ug.prototype.run = function() {
  for (var t = this.options, e = t, r = e.counterclockwise !== void 0 ? !e.counterclockwise : e.clockwise, n = t.cy, a = e.eles, i = a.nodes().not(":parent"), s = zt(e.boundingBox ? e.boundingBox : {
    x1: 0,
    y1: 0,
    w: n.width(),
    h: n.height()
  }), o = {
    x: s.x1 + s.w / 2,
    y: s.y1 + s.h / 2
  }, l = [], u = 0, f = 0; f < i.length; f++) {
    var c = i[f], v = void 0;
    v = e.concentric(c), l.push({
      value: v,
      node: c
    }), c._private.scratch.concentric = v;
  }
  i.updateStyle();
  for (var d = 0; d < i.length; d++) {
    var h = i[d], y = h.layoutDimensions(e);
    u = Math.max(u, y.w, y.h);
  }
  l.sort(function(z, W) {
    return W.value - z.value;
  });
  for (var g = e.levelWidth(i), p = [[]], m = p[0], b = 0; b < l.length; b++) {
    var w = l[b];
    if (m.length > 0) {
      var E = Math.abs(m[0].value - w.value);
      E >= g && (m = [], p.push(m));
    }
    m.push(w);
  }
  var T = u + e.minNodeSpacing;
  if (!e.avoidOverlap) {
    var x = p.length > 0 && p[0].length > 1, S = Math.min(s.w, s.h) / 2 - T, D = S / (p.length + x ? 1 : 0);
    T = Math.min(T, D);
  }
  for (var A = 0, k = 0; k < p.length; k++) {
    var R = p[k], M = e.sweep === void 0 ? 2 * Math.PI - 2 * Math.PI / R.length : e.sweep, I = R.dTheta = M / Math.max(1, R.length - 1);
    if (R.length > 1 && e.avoidOverlap) {
      var _ = Math.cos(I) - Math.cos(0), N = Math.sin(I) - Math.sin(0), L = Math.sqrt(T * T / (_ * _ + N * N));
      A = Math.max(L, A);
    }
    R.r = A, A += T;
  }
  if (e.equidistant) {
    for (var F = 0, H = 0, V = 0; V < p.length; V++) {
      var O = p[V], $ = O.r - H;
      F = Math.max(F, $);
    }
    H = 0;
    for (var Q = 0; Q < p.length; Q++) {
      var se = p[Q];
      Q === 0 && (H = se.r), se.r = H, H += F;
    }
  }
  for (var ae = {}, le = 0; le < p.length; le++)
    for (var ce = p[le], he = ce.dTheta, ie = ce.r, U = 0; U < ce.length; U++) {
      var X = ce[U], C = e.startAngle + (r ? 1 : -1) * he * U, B = {
        x: o.x + ie * Math.cos(C),
        y: o.y + ie * Math.sin(C)
      };
      ae[X.node.id()] = B;
    }
  return a.nodes().layoutPositions(this, e, function(z) {
    var W = z.id();
    return ae[W];
  }), this;
};
var Hl, Yx = {
  // Called on `layoutready`
  ready: function() {
  },
  // Called on `layoutstop`
  stop: function() {
  },
  // Whether to animate while running the layout
  // true : Animate continuously as the layout is running
  // false : Just show the end result
  // 'end' : Animate with the end result, from the initial positions to the end positions
  animate: !0,
  // Easing of the animation for animate:'end'
  animationEasing: void 0,
  // The duration of the animation for animate:'end'
  animationDuration: void 0,
  // A function that determines whether the node should be animated
  // All nodes animated by default on animate enabled
  // Non-animated nodes are positioned immediately when the layout starts
  animateFilter: function(e, r) {
    return !0;
  },
  // The layout animates only after this many milliseconds for animate:true
  // (prevents flashing on fast runs)
  animationThreshold: 250,
  // Number of iterations between consecutive screen positions update
  refresh: 20,
  // Whether to fit the network view after when done
  fit: !0,
  // Padding on fit
  padding: 30,
  // Constrain layout bounds; { x1, y1, x2, y2 } or { x1, y1, w, h }
  boundingBox: void 0,
  // Excludes the label when calculating node bounding boxes for the layout algorithm
  nodeDimensionsIncludeLabels: !1,
  // Randomize the initial positions of the nodes (true) or use existing positions (false)
  randomize: !1,
  // Extra spacing between components in non-compound graphs
  componentSpacing: 40,
  // Node repulsion (non overlapping) multiplier
  nodeRepulsion: function(e) {
    return 2048;
  },
  // Node repulsion (overlapping) multiplier
  nodeOverlap: 4,
  // Ideal edge (non nested) length
  idealEdgeLength: function(e) {
    return 32;
  },
  // Divisor to compute edge forces
  edgeElasticity: function(e) {
    return 32;
  },
  // Nesting factor (multiplier) to compute ideal edge length for nested edges
  nestingFactor: 1.2,
  // Gravity force (constant)
  gravity: 1,
  // Maximum number of iterations to perform
  numIter: 1e3,
  // Initial temperature (maximum node displacement)
  initialTemp: 1e3,
  // Cooling factor (how the temperature is reduced between consecutive iterations
  coolingFactor: 0.99,
  // Lower temperature threshold (below this point the layout will end)
  minTemp: 1
};
function io(t) {
  this.options = Ae({}, Yx, t), this.options.layout = this;
  var e = this.options.eles.nodes(), r = this.options.eles.edges(), n = r.filter(function(a) {
    var i = a.source().data("id"), s = a.target().data("id"), o = e.some(function(u) {
      return u.data("id") === i;
    }), l = e.some(function(u) {
      return u.data("id") === s;
    });
    return !o || !l;
  });
  this.options.eles = this.options.eles.not(n);
}
io.prototype.run = function() {
  var t = this.options, e = t.cy, r = this;
  r.stopped = !1, (t.animate === !0 || t.animate === !1) && r.emit({
    type: "layoutstart",
    layout: r
  }), t.debug === !0 ? Hl = !0 : Hl = !1;
  var n = Xx(e, r, t);
  Hl && Qx(n), t.randomize && jx(n);
  var a = qr(), i = function() {
    Jx(n, e, t), t.fit === !0 && e.fit(t.padding);
  }, s = function(v) {
    return !(r.stopped || v >= t.numIter || (eE(n, t), n.temperature = n.temperature * t.coolingFactor, n.temperature < t.minTemp));
  }, o = function() {
    if (t.animate === !0 || t.animate === !1)
      i(), r.one("layoutstop", t.stop), r.emit({
        type: "layoutstop",
        layout: r
      });
    else {
      var v = t.eles.nodes(), d = Wg(n, t, v);
      v.layoutPositions(r, t, d);
    }
  }, l = 0, u = !0;
  if (t.animate === !0) {
    var f = function() {
      for (var v = 0; u && v < t.refresh; )
        u = s(l), l++, v++;
      if (!u)
        rd(n, t), o();
      else {
        var d = qr();
        d - a >= t.animationThreshold && i(), bs(f);
      }
    };
    f();
  } else {
    for (; u; )
      u = s(l), l++;
    rd(n, t), o();
  }
  return this;
};
io.prototype.stop = function() {
  return this.stopped = !0, this.thread && this.thread.stop(), this.emit("layoutstop"), this;
};
io.prototype.destroy = function() {
  return this.thread && this.thread.stop(), this;
};
var Xx = function(e, r, n) {
  for (var a = n.eles.edges(), i = n.eles.nodes(), s = zt(n.boundingBox ? n.boundingBox : {
    x1: 0,
    y1: 0,
    w: e.width(),
    h: e.height()
  }), o = {
    isCompound: e.hasCompoundNodes(),
    layoutNodes: [],
    idToIndex: {},
    nodeSize: i.size(),
    graphSet: [],
    indexToGraph: [],
    layoutEdges: [],
    edgeSize: a.size(),
    temperature: n.initialTemp,
    clientWidth: s.w,
    clientHeight: s.h,
    boundingBox: s
  }, l = n.eles.components(), u = {}, f = 0; f < l.length; f++)
    for (var c = l[f], v = 0; v < c.length; v++) {
      var d = c[v];
      u[d.id()] = f;
    }
  for (var f = 0; f < o.nodeSize; f++) {
    var h = i[f], y = h.layoutDimensions(n), g = {};
    g.isLocked = h.locked(), g.id = h.data("id"), g.parentId = h.data("parent"), g.cmptId = u[h.id()], g.children = [], g.positionX = h.position("x"), g.positionY = h.position("y"), g.offsetX = 0, g.offsetY = 0, g.height = y.w, g.width = y.h, g.maxX = g.positionX + g.width / 2, g.minX = g.positionX - g.width / 2, g.maxY = g.positionY + g.height / 2, g.minY = g.positionY - g.height / 2, g.padLeft = parseFloat(h.style("padding")), g.padRight = parseFloat(h.style("padding")), g.padTop = parseFloat(h.style("padding")), g.padBottom = parseFloat(h.style("padding")), g.nodeRepulsion = tt(n.nodeRepulsion) ? n.nodeRepulsion(h) : n.nodeRepulsion, o.layoutNodes.push(g), o.idToIndex[g.id] = f;
  }
  for (var p = [], m = 0, b = -1, w = [], f = 0; f < o.nodeSize; f++) {
    var h = o.layoutNodes[f], E = h.parentId;
    E != null ? o.layoutNodes[o.idToIndex[E]].children.push(h.id) : (p[++b] = h.id, w.push(h.id));
  }
  for (o.graphSet.push(w); m <= b; ) {
    var T = p[m++], x = o.idToIndex[T], d = o.layoutNodes[x], S = d.children;
    if (S.length > 0) {
      o.graphSet.push(S);
      for (var f = 0; f < S.length; f++)
        p[++b] = S[f];
    }
  }
  for (var f = 0; f < o.graphSet.length; f++)
    for (var D = o.graphSet[f], v = 0; v < D.length; v++) {
      var A = o.idToIndex[D[v]];
      o.indexToGraph[A] = f;
    }
  for (var f = 0; f < o.edgeSize; f++) {
    var k = a[f], R = {};
    R.id = k.data("id"), R.sourceId = k.data("source"), R.targetId = k.data("target");
    var M = tt(n.idealEdgeLength) ? n.idealEdgeLength(k) : n.idealEdgeLength, I = tt(n.edgeElasticity) ? n.edgeElasticity(k) : n.edgeElasticity, _ = o.idToIndex[R.sourceId], N = o.idToIndex[R.targetId], L = o.indexToGraph[_], F = o.indexToGraph[N];
    if (L != F) {
      for (var H = Zx(R.sourceId, R.targetId, o), V = o.graphSet[H], O = 0, g = o.layoutNodes[_]; V.indexOf(g.id) === -1; )
        g = o.layoutNodes[o.idToIndex[g.parentId]], O++;
      for (g = o.layoutNodes[N]; V.indexOf(g.id) === -1; )
        g = o.layoutNodes[o.idToIndex[g.parentId]], O++;
      M *= O * n.nestingFactor;
    }
    R.idealLength = M, R.elasticity = I, o.layoutEdges.push(R);
  }
  return o;
}, Zx = function(e, r, n) {
  var a = Gg(e, r, 0, n);
  return 2 > a.count ? 0 : a.graph;
}, Gg = function(e, r, n, a) {
  var i = a.graphSet[n];
  if (-1 < i.indexOf(e) && -1 < i.indexOf(r))
    return {
      count: 2,
      graph: n
    };
  for (var s = 0, o = 0; o < i.length; o++) {
    var l = i[o], u = a.idToIndex[l], f = a.layoutNodes[u].children;
    if (f.length !== 0) {
      var c = a.indexToGraph[a.idToIndex[f[0]]], v = Gg(e, r, c, a);
      if (v.count !== 0)
        if (v.count === 1) {
          if (s++, s === 2)
            break;
        } else
          return v;
    }
  }
  return {
    count: s,
    graph: n
  };
}, Qx, jx = function(e, r) {
  for (var n = e.clientWidth, a = e.clientHeight, i = 0; i < e.nodeSize; i++) {
    var s = e.layoutNodes[i];
    s.children.length === 0 && !s.isLocked && (s.positionX = Math.random() * n, s.positionY = Math.random() * a);
  }
}, Wg = function(e, r, n) {
  var a = e.boundingBox, i = {
    x1: 1 / 0,
    x2: -1 / 0,
    y1: 1 / 0,
    y2: -1 / 0
  };
  return r.boundingBox && (n.forEach(function(s) {
    var o = e.layoutNodes[e.idToIndex[s.data("id")]];
    i.x1 = Math.min(i.x1, o.positionX), i.x2 = Math.max(i.x2, o.positionX), i.y1 = Math.min(i.y1, o.positionY), i.y2 = Math.max(i.y2, o.positionY);
  }), i.w = i.x2 - i.x1, i.h = i.y2 - i.y1), function(s, o) {
    var l = e.layoutNodes[e.idToIndex[s.data("id")]];
    if (r.boundingBox) {
      var u = i.w === 0 ? 0.5 : (l.positionX - i.x1) / i.w, f = i.h === 0 ? 0.5 : (l.positionY - i.y1) / i.h;
      return {
        x: a.x1 + u * a.w,
        y: a.y1 + f * a.h
      };
    } else
      return {
        x: l.positionX,
        y: l.positionY
      };
  };
}, Jx = function(e, r, n) {
  var a = n.layout, i = n.eles.nodes(), s = Wg(e, n, i);
  i.positions(s), e.ready !== !0 && (e.ready = !0, a.one("layoutready", n.ready), a.emit({
    type: "layoutready",
    layout: this
  }));
}, eE = function(e, r, n) {
  tE(e, r), aE(e), iE(e, r), sE(e), oE(e);
}, tE = function(e, r) {
  for (var n = 0; n < e.graphSet.length; n++)
    for (var a = e.graphSet[n], i = a.length, s = 0; s < i; s++)
      for (var o = e.layoutNodes[e.idToIndex[a[s]]], l = s + 1; l < i; l++) {
        var u = e.layoutNodes[e.idToIndex[a[l]]];
        rE(o, u, e, r);
      }
}, td = function(e) {
  return -e + 2 * e * Math.random();
}, rE = function(e, r, n, a) {
  var i = e.cmptId, s = r.cmptId;
  if (!(i !== s && !n.isCompound)) {
    var o = r.positionX - e.positionX, l = r.positionY - e.positionY, u = 1;
    o === 0 && l === 0 && (o = td(u), l = td(u));
    var f = nE(e, r, o, l);
    if (f > 0)
      var c = a.nodeOverlap * f, v = Math.sqrt(o * o + l * l), d = c * o / v, h = c * l / v;
    else
      var y = Ps(e, o, l), g = Ps(r, -1 * o, -1 * l), p = g.x - y.x, m = g.y - y.y, b = p * p + m * m, v = Math.sqrt(b), c = (e.nodeRepulsion + r.nodeRepulsion) / b, d = c * p / v, h = c * m / v;
    e.isLocked || (e.offsetX -= d, e.offsetY -= h), r.isLocked || (r.offsetX += d, r.offsetY += h);
  }
}, nE = function(e, r, n, a) {
  if (n > 0)
    var i = e.maxX - r.minX;
  else
    var i = r.maxX - e.minX;
  if (a > 0)
    var s = e.maxY - r.minY;
  else
    var s = r.maxY - e.minY;
  return i >= 0 && s >= 0 ? Math.sqrt(i * i + s * s) : 0;
}, Ps = function(e, r, n) {
  var a = e.positionX, i = e.positionY, s = e.height || 1, o = e.width || 1, l = n / r, u = s / o, f = {};
  return r === 0 && 0 < n || r === 0 && 0 > n ? (f.x = a, f.y = i + s / 2, f) : 0 < r && -1 * u <= l && l <= u ? (f.x = a + o / 2, f.y = i + o * n / 2 / r, f) : 0 > r && -1 * u <= l && l <= u ? (f.x = a - o / 2, f.y = i - o * n / 2 / r, f) : 0 < n && (l <= -1 * u || l >= u) ? (f.x = a + s * r / 2 / n, f.y = i + s / 2, f) : (0 > n && (l <= -1 * u || l >= u) && (f.x = a - s * r / 2 / n, f.y = i - s / 2), f);
}, aE = function(e, r) {
  for (var n = 0; n < e.edgeSize; n++) {
    var a = e.layoutEdges[n], i = e.idToIndex[a.sourceId], s = e.layoutNodes[i], o = e.idToIndex[a.targetId], l = e.layoutNodes[o], u = l.positionX - s.positionX, f = l.positionY - s.positionY;
    if (!(u === 0 && f === 0)) {
      var c = Ps(s, u, f), v = Ps(l, -1 * u, -1 * f), d = v.x - c.x, h = v.y - c.y, y = Math.sqrt(d * d + h * h), g = Math.pow(a.idealLength - y, 2) / a.elasticity;
      if (y !== 0)
        var p = g * d / y, m = g * h / y;
      else
        var p = 0, m = 0;
      s.isLocked || (s.offsetX += p, s.offsetY += m), l.isLocked || (l.offsetX -= p, l.offsetY -= m);
    }
  }
}, iE = function(e, r) {
  if (r.gravity !== 0)
    for (var n = 1, a = 0; a < e.graphSet.length; a++) {
      var i = e.graphSet[a], s = i.length;
      if (a === 0)
        var o = e.clientHeight / 2, l = e.clientWidth / 2;
      else
        var u = e.layoutNodes[e.idToIndex[i[0]]], f = e.layoutNodes[e.idToIndex[u.parentId]], o = f.positionX, l = f.positionY;
      for (var c = 0; c < s; c++) {
        var v = e.layoutNodes[e.idToIndex[i[c]]];
        if (!v.isLocked) {
          var d = o - v.positionX, h = l - v.positionY, y = Math.sqrt(d * d + h * h);
          if (y > n) {
            var g = r.gravity * d / y, p = r.gravity * h / y;
            v.offsetX += g, v.offsetY += p;
          }
        }
      }
    }
}, sE = function(e, r) {
  var n = [], a = 0, i = -1;
  for (n.push.apply(n, e.graphSet[0]), i += e.graphSet[0].length; a <= i; ) {
    var s = n[a++], o = e.idToIndex[s], l = e.layoutNodes[o], u = l.children;
    if (0 < u.length && !l.isLocked) {
      for (var f = l.offsetX, c = l.offsetY, v = 0; v < u.length; v++) {
        var d = e.layoutNodes[e.idToIndex[u[v]]];
        d.offsetX += f, d.offsetY += c, n[++i] = u[v];
      }
      l.offsetX = 0, l.offsetY = 0;
    }
  }
}, oE = function(e, r) {
  for (var n = 0; n < e.nodeSize; n++) {
    var a = e.layoutNodes[n];
    0 < a.children.length && (a.maxX = void 0, a.minX = void 0, a.maxY = void 0, a.minY = void 0);
  }
  for (var n = 0; n < e.nodeSize; n++) {
    var a = e.layoutNodes[n];
    if (!(0 < a.children.length || a.isLocked)) {
      var i = lE(a.offsetX, a.offsetY, e.temperature);
      a.positionX += i.x, a.positionY += i.y, a.offsetX = 0, a.offsetY = 0, a.minX = a.positionX - a.width, a.maxX = a.positionX + a.width, a.minY = a.positionY - a.height, a.maxY = a.positionY + a.height, Kg(a, e);
    }
  }
  for (var n = 0; n < e.nodeSize; n++) {
    var a = e.layoutNodes[n];
    0 < a.children.length && !a.isLocked && (a.positionX = (a.maxX + a.minX) / 2, a.positionY = (a.maxY + a.minY) / 2, a.width = a.maxX - a.minX, a.height = a.maxY - a.minY);
  }
}, lE = function(e, r, n) {
  var a = Math.sqrt(e * e + r * r);
  if (a > n)
    var i = {
      x: n * e / a,
      y: n * r / a
    };
  else
    var i = {
      x: e,
      y: r
    };
  return i;
}, Kg = function(e, r) {
  var n = e.parentId;
  if (n != null) {
    var a = r.layoutNodes[r.idToIndex[n]], i = !1;
    if ((a.maxX == null || e.maxX + a.padRight > a.maxX) && (a.maxX = e.maxX + a.padRight, i = !0), (a.minX == null || e.minX - a.padLeft < a.minX) && (a.minX = e.minX - a.padLeft, i = !0), (a.maxY == null || e.maxY + a.padBottom > a.maxY) && (a.maxY = e.maxY + a.padBottom, i = !0), (a.minY == null || e.minY - a.padTop < a.minY) && (a.minY = e.minY - a.padTop, i = !0), i)
      return Kg(a, r);
  }
}, rd = function(e, r) {
  for (var n = e.layoutNodes, a = [], i = 0; i < n.length; i++) {
    var s = n[i], o = s.cmptId, l = a[o] = a[o] || [];
    l.push(s);
  }
  for (var u = 0, i = 0; i < a.length; i++) {
    var f = a[i];
    if (f) {
      f.x1 = 1 / 0, f.x2 = -1 / 0, f.y1 = 1 / 0, f.y2 = -1 / 0;
      for (var c = 0; c < f.length; c++) {
        var v = f[c];
        f.x1 = Math.min(f.x1, v.positionX - v.width / 2), f.x2 = Math.max(f.x2, v.positionX + v.width / 2), f.y1 = Math.min(f.y1, v.positionY - v.height / 2), f.y2 = Math.max(f.y2, v.positionY + v.height / 2);
      }
      f.w = f.x2 - f.x1, f.h = f.y2 - f.y1, u += f.w * f.h;
    }
  }
  a.sort(function(m, b) {
    return b.w * b.h - m.w * m.h;
  });
  for (var d = 0, h = 0, y = 0, g = 0, p = Math.sqrt(u) * e.clientWidth / e.clientHeight, i = 0; i < a.length; i++) {
    var f = a[i];
    if (f) {
      for (var c = 0; c < f.length; c++) {
        var v = f[c];
        v.isLocked || (v.positionX += d - f.x1, v.positionY += h - f.y1);
      }
      d += f.w + r.componentSpacing, y += f.w + r.componentSpacing, g = Math.max(g, f.h), y > p && (h += g + r.componentSpacing, d = 0, y = 0, g = 0);
    }
  }
}, uE = {
  fit: !0,
  // whether to fit the viewport to the graph
  padding: 30,
  // padding used on fit
  boundingBox: void 0,
  // constrain layout bounds; { x1, y1, x2, y2 } or { x1, y1, w, h }
  avoidOverlap: !0,
  // prevents node overlap, may overflow boundingBox if not enough space
  avoidOverlapPadding: 10,
  // extra spacing around nodes when avoidOverlap: true
  nodeDimensionsIncludeLabels: !1,
  // Excludes the label when calculating node bounding boxes for the layout algorithm
  spacingFactor: void 0,
  // Applies a multiplicative factor (>0) to expand or compress the overall area that the nodes take up
  condense: !1,
  // uses all available space on false, uses minimal space on true
  rows: void 0,
  // force num of rows in the grid
  cols: void 0,
  // force num of columns in the grid
  position: function(e) {
  },
  // returns { row, col } for element
  sort: void 0,
  // a sorting function to order the nodes; e.g. function(a, b){ return a.data('weight') - b.data('weight') }
  animate: !1,
  // whether to transition the node positions
  animationDuration: 500,
  // duration of animation in ms if enabled
  animationEasing: void 0,
  // easing of animation if enabled
  animateFilter: function(e, r) {
    return !0;
  },
  // a function that determines whether the node should be animated.  All nodes animated by default on animate enabled.  Non-animated nodes are positioned immediately when the layout starts
  ready: void 0,
  // callback on layoutready
  stop: void 0,
  // callback on layoutstop
  transform: function(e, r) {
    return r;
  }
  // transform a given node position. Useful for changing flow direction in discrete layouts 
};
function Yg(t) {
  this.options = Ae({}, uE, t);
}
Yg.prototype.run = function() {
  var t = this.options, e = t, r = t.cy, n = e.eles, a = n.nodes().not(":parent");
  e.sort && (a = a.sort(e.sort));
  var i = zt(e.boundingBox ? e.boundingBox : {
    x1: 0,
    y1: 0,
    w: r.width(),
    h: r.height()
  });
  if (i.h === 0 || i.w === 0)
    n.nodes().layoutPositions(this, e, function(Q) {
      return {
        x: i.x1,
        y: i.y1
      };
    });
  else {
    var s = a.size(), o = Math.sqrt(s * i.h / i.w), l = Math.round(o), u = Math.round(i.w / i.h * o), f = function(se) {
      if (se == null)
        return Math.min(l, u);
      var ae = Math.min(l, u);
      ae == l ? l = se : u = se;
    }, c = function(se) {
      if (se == null)
        return Math.max(l, u);
      var ae = Math.max(l, u);
      ae == l ? l = se : u = se;
    }, v = e.rows, d = e.cols != null ? e.cols : e.columns;
    if (v != null && d != null)
      l = v, u = d;
    else if (v != null && d == null)
      l = v, u = Math.ceil(s / l);
    else if (v == null && d != null)
      u = d, l = Math.ceil(s / u);
    else if (u * l > s) {
      var h = f(), y = c();
      (h - 1) * y >= s ? f(h - 1) : (y - 1) * h >= s && c(y - 1);
    } else
      for (; u * l < s; ) {
        var g = f(), p = c();
        (p + 1) * g >= s ? c(p + 1) : f(g + 1);
      }
    var m = i.w / u, b = i.h / l;
    if (e.condense && (m = 0, b = 0), e.avoidOverlap)
      for (var w = 0; w < a.length; w++) {
        var E = a[w], T = E._private.position;
        (T.x == null || T.y == null) && (T.x = 0, T.y = 0);
        var x = E.layoutDimensions(e), S = e.avoidOverlapPadding, D = x.w + S, A = x.h + S;
        m = Math.max(m, D), b = Math.max(b, A);
      }
    for (var k = {}, R = function(se, ae) {
      return !!k["c-" + se + "-" + ae];
    }, M = function(se, ae) {
      k["c-" + se + "-" + ae] = !0;
    }, I = 0, _ = 0, N = function() {
      _++, _ >= u && (_ = 0, I++);
    }, L = {}, F = 0; F < a.length; F++) {
      var H = a[F], V = e.position(H);
      if (V && (V.row !== void 0 || V.col !== void 0)) {
        var O = {
          row: V.row,
          col: V.col
        };
        if (O.col === void 0)
          for (O.col = 0; R(O.row, O.col); )
            O.col++;
        else if (O.row === void 0)
          for (O.row = 0; R(O.row, O.col); )
            O.row++;
        L[H.id()] = O, M(O.row, O.col);
      }
    }
    var $ = function(se, ae) {
      var le, ce;
      if (se.locked() || se.isParent())
        return !1;
      var he = L[se.id()];
      if (he)
        le = he.col * m + m / 2 + i.x1, ce = he.row * b + b / 2 + i.y1;
      else {
        for (; R(I, _); )
          N();
        le = _ * m + m / 2 + i.x1, ce = I * b + b / 2 + i.y1, M(I, _), N();
      }
      return {
        x: le,
        y: ce
      };
    };
    a.layoutPositions(this, e, $);
  }
  return this;
};
var fE = {
  ready: function() {
  },
  // on layoutready
  stop: function() {
  }
  // on layoutstop
};
function cf(t) {
  this.options = Ae({}, fE, t);
}
cf.prototype.run = function() {
  var t = this.options, e = t.eles, r = this;
  return t.cy, r.emit("layoutstart"), e.nodes().positions(function() {
    return {
      x: 0,
      y: 0
    };
  }), r.one("layoutready", t.ready), r.emit("layoutready"), r.one("layoutstop", t.stop), r.emit("layoutstop"), this;
};
cf.prototype.stop = function() {
  return this;
};
var cE = {
  positions: void 0,
  // map of (node id) => (position obj); or function(node){ return somPos; }
  zoom: void 0,
  // the zoom level to set (prob want fit = false if set)
  pan: void 0,
  // the pan level to set (prob want fit = false if set)
  fit: !0,
  // whether to fit to viewport
  padding: 30,
  // padding on fit
  spacingFactor: void 0,
  // Applies a multiplicative factor (>0) to expand or compress the overall area that the nodes take up
  animate: !1,
  // whether to transition the node positions
  animationDuration: 500,
  // duration of animation in ms if enabled
  animationEasing: void 0,
  // easing of animation if enabled
  animateFilter: function(e, r) {
    return !0;
  },
  // a function that determines whether the node should be animated.  All nodes animated by default on animate enabled.  Non-animated nodes are positioned immediately when the layout starts
  ready: void 0,
  // callback on layoutready
  stop: void 0,
  // callback on layoutstop
  transform: function(e, r) {
    return r;
  }
  // transform a given node position. Useful for changing flow direction in discrete layouts
};
function Xg(t) {
  this.options = Ae({}, cE, t);
}
Xg.prototype.run = function() {
  var t = this.options, e = t.eles, r = e.nodes(), n = tt(t.positions);
  function a(i) {
    if (t.positions == null)
      return Mb(i.position());
    if (n)
      return t.positions(i);
    var s = t.positions[i._private.data.id];
    return s ?? null;
  }
  return r.layoutPositions(this, t, function(i, s) {
    var o = a(i);
    return i.locked() || o == null ? !1 : o;
  }), this;
};
var vE = {
  fit: !0,
  // whether to fit to viewport
  padding: 30,
  // fit padding
  boundingBox: void 0,
  // constrain layout bounds; { x1, y1, x2, y2 } or { x1, y1, w, h }
  animate: !1,
  // whether to transition the node positions
  animationDuration: 500,
  // duration of animation in ms if enabled
  animationEasing: void 0,
  // easing of animation if enabled
  animateFilter: function(e, r) {
    return !0;
  },
  // a function that determines whether the node should be animated.  All nodes animated by default on animate enabled.  Non-animated nodes are positioned immediately when the layout starts
  ready: void 0,
  // callback on layoutready
  stop: void 0,
  // callback on layoutstop
  transform: function(e, r) {
    return r;
  }
  // transform a given node position. Useful for changing flow direction in discrete layouts 
};
function Zg(t) {
  this.options = Ae({}, vE, t);
}
Zg.prototype.run = function() {
  var t = this.options, e = t.cy, r = t.eles, n = zt(t.boundingBox ? t.boundingBox : {
    x1: 0,
    y1: 0,
    w: e.width(),
    h: e.height()
  }), a = function(s, o) {
    return {
      x: n.x1 + Math.round(Math.random() * n.w),
      y: n.y1 + Math.round(Math.random() * n.h)
    };
  };
  return r.nodes().layoutPositions(this, t, a), this;
};
var dE = [{
  name: "breadthfirst",
  impl: $g
}, {
  name: "circle",
  impl: Hg
}, {
  name: "concentric",
  impl: Ug
}, {
  name: "cose",
  impl: io
}, {
  name: "grid",
  impl: Yg
}, {
  name: "null",
  impl: cf
}, {
  name: "preset",
  impl: Xg
}, {
  name: "random",
  impl: Zg
}];
function Qg(t) {
  this.options = t, this.notifications = 0;
}
var nd = function() {
}, ad = function() {
  throw new Error("A headless instance can not render images");
};
Qg.prototype = {
  recalculateRenderedStyle: nd,
  notify: function() {
    this.notifications++;
  },
  init: nd,
  isHeadless: function() {
    return !0;
  },
  png: ad,
  jpg: ad
};
var vf = {};
vf.arrowShapeWidth = 0.3;
vf.registerArrowShapes = function() {
  var t = this.arrowShapes = {}, e = this, r = function(u, f, c, v, d, h, y) {
    var g = d.x - c / 2 - y, p = d.x + c / 2 + y, m = d.y - c / 2 - y, b = d.y + c / 2 + y, w = g <= u && u <= p && m <= f && f <= b;
    return w;
  }, n = function(u, f, c, v, d) {
    var h = u * Math.cos(v) - f * Math.sin(v), y = u * Math.sin(v) + f * Math.cos(v), g = h * c, p = y * c, m = g + d.x, b = p + d.y;
    return {
      x: m,
      y: b
    };
  }, a = function(u, f, c, v) {
    for (var d = [], h = 0; h < u.length; h += 2) {
      var y = u[h], g = u[h + 1];
      d.push(n(y, g, f, c, v));
    }
    return d;
  }, i = function(u) {
    for (var f = [], c = 0; c < u.length; c++) {
      var v = u[c];
      f.push(v.x, v.y);
    }
    return f;
  }, s = function(u) {
    return u.pstyle("width").pfValue * u.pstyle("arrow-scale").pfValue * 2;
  }, o = function(u, f) {
    Se(f) && (f = t[f]), t[u] = Ae({
      name: u,
      points: [-0.15, -0.3, 0.15, -0.3, 0.15, 0.3, -0.15, 0.3],
      collide: function(v, d, h, y, g, p) {
        var m = i(a(this.points, h + 2 * p, y, g)), b = Ut(v, d, m);
        return b;
      },
      roughCollide: r,
      draw: function(v, d, h, y) {
        var g = a(this.points, d, h, y);
        e.arrowShapeImpl("polygon")(v, g);
      },
      spacing: function(v) {
        return 0;
      },
      gap: s
    }, f);
  };
  o("none", {
    collide: ws,
    roughCollide: ws,
    draw: Zu,
    spacing: mc,
    gap: mc
  }), o("triangle", {
    points: [-0.15, -0.3, 0, 0, 0.15, -0.3]
  }), o("arrow", "triangle"), o("triangle-backcurve", {
    points: t.triangle.points,
    controlPoint: [0, -0.15],
    roughCollide: r,
    draw: function(u, f, c, v, d) {
      var h = a(this.points, f, c, v), y = this.controlPoint, g = n(y[0], y[1], f, c, v);
      e.arrowShapeImpl(this.name)(u, h, g);
    },
    gap: function(u) {
      return s(u) * 0.8;
    }
  }), o("triangle-tee", {
    points: [0, 0, 0.15, -0.3, -0.15, -0.3, 0, 0],
    pointsTee: [-0.15, -0.4, -0.15, -0.5, 0.15, -0.5, 0.15, -0.4],
    collide: function(u, f, c, v, d, h, y) {
      var g = i(a(this.points, c + 2 * y, v, d)), p = i(a(this.pointsTee, c + 2 * y, v, d)), m = Ut(u, f, g) || Ut(u, f, p);
      return m;
    },
    draw: function(u, f, c, v, d) {
      var h = a(this.points, f, c, v), y = a(this.pointsTee, f, c, v);
      e.arrowShapeImpl(this.name)(u, h, y);
    }
  }), o("circle-triangle", {
    radius: 0.15,
    pointsTr: [0, -0.15, 0.15, -0.45, -0.15, -0.45, 0, -0.15],
    collide: function(u, f, c, v, d, h, y) {
      var g = d, p = Math.pow(g.x - u, 2) + Math.pow(g.y - f, 2) <= Math.pow((c + 2 * y) * this.radius, 2), m = i(a(this.points, c + 2 * y, v, d));
      return Ut(u, f, m) || p;
    },
    draw: function(u, f, c, v, d) {
      var h = a(this.pointsTr, f, c, v);
      e.arrowShapeImpl(this.name)(u, h, v.x, v.y, this.radius * f);
    },
    spacing: function(u) {
      return e.getArrowWidth(u.pstyle("width").pfValue, u.pstyle("arrow-scale").value) * this.radius;
    }
  }), o("triangle-cross", {
    points: [0, 0, 0.15, -0.3, -0.15, -0.3, 0, 0],
    baseCrossLinePts: [
      -0.15,
      -0.4,
      // first half of the rectangle
      -0.15,
      -0.4,
      0.15,
      -0.4,
      // second half of the rectangle
      0.15,
      -0.4
    ],
    crossLinePts: function(u, f) {
      var c = this.baseCrossLinePts.slice(), v = f / u, d = 3, h = 5;
      return c[d] = c[d] - v, c[h] = c[h] - v, c;
    },
    collide: function(u, f, c, v, d, h, y) {
      var g = i(a(this.points, c + 2 * y, v, d)), p = i(a(this.crossLinePts(c, h), c + 2 * y, v, d)), m = Ut(u, f, g) || Ut(u, f, p);
      return m;
    },
    draw: function(u, f, c, v, d) {
      var h = a(this.points, f, c, v), y = a(this.crossLinePts(f, d), f, c, v);
      e.arrowShapeImpl(this.name)(u, h, y);
    }
  }), o("vee", {
    points: [-0.15, -0.3, 0, 0, 0.15, -0.3, 0, -0.15],
    gap: function(u) {
      return s(u) * 0.525;
    }
  }), o("circle", {
    radius: 0.15,
    collide: function(u, f, c, v, d, h, y) {
      var g = d, p = Math.pow(g.x - u, 2) + Math.pow(g.y - f, 2) <= Math.pow((c + 2 * y) * this.radius, 2);
      return p;
    },
    draw: function(u, f, c, v, d) {
      e.arrowShapeImpl(this.name)(u, v.x, v.y, this.radius * f);
    },
    spacing: function(u) {
      return e.getArrowWidth(u.pstyle("width").pfValue, u.pstyle("arrow-scale").value) * this.radius;
    }
  }), o("tee", {
    points: [-0.15, 0, -0.15, -0.1, 0.15, -0.1, 0.15, 0],
    spacing: function(u) {
      return 1;
    },
    gap: function(u) {
      return 1;
    }
  }), o("square", {
    points: [-0.15, 0, 0.15, 0, 0.15, -0.3, -0.15, -0.3]
  }), o("diamond", {
    points: [-0.15, -0.15, 0, -0.3, 0.15, -0.15, 0, 0],
    gap: function(u) {
      return u.pstyle("width").pfValue * u.pstyle("arrow-scale").value;
    }
  }), o("chevron", {
    points: [0, 0, -0.15, -0.15, -0.1, -0.2, 0, -0.1, 0.1, -0.2, 0.15, -0.15],
    gap: function(u) {
      return 0.95 * u.pstyle("width").pfValue * u.pstyle("arrow-scale").value;
    }
  });
};
var _n = {};
_n.projectIntoViewport = function(t, e) {
  var r = this.cy, n = this.findContainerClientCoords(), a = n[0], i = n[1], s = n[4], o = r.pan(), l = r.zoom(), u = ((t - a) / s - o.x) / l, f = ((e - i) / s - o.y) / l;
  return [u, f];
};
_n.findContainerClientCoords = function() {
  if (this.containerBB)
    return this.containerBB;
  var t = this.container, e = t.getBoundingClientRect(), r = this.cy.window().getComputedStyle(t), n = function(p) {
    return parseFloat(r.getPropertyValue(p));
  }, a = {
    left: n("padding-left"),
    right: n("padding-right"),
    top: n("padding-top"),
    bottom: n("padding-bottom")
  }, i = {
    left: n("border-left-width"),
    right: n("border-right-width"),
    top: n("border-top-width"),
    bottom: n("border-bottom-width")
  }, s = t.clientWidth, o = t.clientHeight, l = a.left + a.right, u = a.top + a.bottom, f = i.left + i.right, c = e.width / (s + f), v = s - l, d = o - u, h = e.left + a.left + i.left, y = e.top + a.top + i.top;
  return this.containerBB = [h, y, v, d, c];
};
_n.invalidateContainerClientCoordsCache = function() {
  this.containerBB = null;
};
_n.findNearestElement = function(t, e, r, n) {
  return this.findNearestElements(t, e, r, n)[0];
};
_n.findNearestElements = function(t, e, r, n) {
  var a = this, i = this, s = i.getCachedZSortedEles(), o = [], l = i.cy.zoom(), u = i.cy.hasCompoundNodes(), f = (n ? 24 : 8) / l, c = (n ? 8 : 2) / l, v = (n ? 8 : 2) / l, d = 1 / 0, h, y;
  r && (s = s.interactive);
  function g(x, S) {
    if (x.isNode()) {
      if (y)
        return;
      y = x, o.push(x);
    }
    if (x.isEdge() && (S == null || S < d))
      if (h) {
        if (h.pstyle("z-compound-depth").value === x.pstyle("z-compound-depth").value && h.pstyle("z-compound-depth").value === x.pstyle("z-compound-depth").value) {
          for (var D = 0; D < o.length; D++)
            if (o[D].isEdge()) {
              o[D] = x, h = x, d = S ?? d;
              break;
            }
        }
      } else
        o.push(x), h = x, d = S ?? d;
  }
  function p(x) {
    var S = x.outerWidth() + 2 * c, D = x.outerHeight() + 2 * c, A = S / 2, k = D / 2, R = x.position(), M = x.pstyle("corner-radius").value === "auto" ? "auto" : x.pstyle("corner-radius").pfValue, I = x._private.rscratch;
    if (R.x - A <= t && t <= R.x + A && R.y - k <= e && e <= R.y + k) {
      var _ = i.nodeShapes[a.getNodeShape(x)];
      if (_.checkPoint(t, e, 0, S, D, R.x, R.y, M, I))
        return g(x, 0), !0;
    }
  }
  function m(x) {
    var S = x._private, D = S.rscratch, A = x.pstyle("width").pfValue, k = x.pstyle("arrow-scale").value, R = A / 2 + f, M = R * R, I = R * 2, F = S.source, H = S.target, _;
    if (D.edgeType === "segments" || D.edgeType === "straight" || D.edgeType === "haystack") {
      for (var N = D.allpts, L = 0; L + 3 < N.length; L += 2)
        if (Wb(t, e, N[L], N[L + 1], N[L + 2], N[L + 3], I) && M > (_ = Qb(t, e, N[L], N[L + 1], N[L + 2], N[L + 3])))
          return g(x, _), !0;
    } else if (D.edgeType === "bezier" || D.edgeType === "multibezier" || D.edgeType === "self" || D.edgeType === "compound") {
      for (var N = D.allpts, L = 0; L + 5 < D.allpts.length; L += 4)
        if (Kb(t, e, N[L], N[L + 1], N[L + 2], N[L + 3], N[L + 4], N[L + 5], I) && M > (_ = Zb(t, e, N[L], N[L + 1], N[L + 2], N[L + 3], N[L + 4], N[L + 5])))
          return g(x, _), !0;
    }
    for (var F = F || S.source, H = H || S.target, V = a.getArrowWidth(A, k), O = [{
      name: "source",
      x: D.arrowStartX,
      y: D.arrowStartY,
      angle: D.srcArrowAngle
    }, {
      name: "target",
      x: D.arrowEndX,
      y: D.arrowEndY,
      angle: D.tgtArrowAngle
    }, {
      name: "mid-source",
      x: D.midX,
      y: D.midY,
      angle: D.midsrcArrowAngle
    }, {
      name: "mid-target",
      x: D.midX,
      y: D.midY,
      angle: D.midtgtArrowAngle
    }], L = 0; L < O.length; L++) {
      var $ = O[L], Q = i.arrowShapes[x.pstyle($.name + "-arrow-shape").value], se = x.pstyle("width").pfValue;
      if (Q.roughCollide(t, e, V, $.angle, {
        x: $.x,
        y: $.y
      }, se, f) && Q.collide(t, e, V, $.angle, {
        x: $.x,
        y: $.y
      }, se, f))
        return g(x), !0;
    }
    u && o.length > 0 && (p(F), p(H));
  }
  function b(x, S, D) {
    return Nt(x, S, D);
  }
  function w(x, S) {
    var D = x._private, A = v, k;
    S ? k = S + "-" : k = "", x.boundingBox();
    var R = D.labelBounds[S || "main"], M = x.pstyle(k + "label").value, I = x.pstyle("text-events").strValue === "yes";
    if (!(!I || !M)) {
      var _ = b(D.rscratch, "labelX", S), N = b(D.rscratch, "labelY", S), L = b(D.rscratch, "labelAngle", S), F = x.pstyle(k + "text-margin-x").pfValue, H = x.pstyle(k + "text-margin-y").pfValue, V = R.x1 - A - F, O = R.x2 + A - F, $ = R.y1 - A - H, Q = R.y2 + A - H;
      if (L) {
        var se = Math.cos(L), ae = Math.sin(L), le = function(B, z) {
          return B = B - _, z = z - N, {
            x: B * se - z * ae + _,
            y: B * ae + z * se + N
          };
        }, ce = le(V, $), he = le(V, Q), ie = le(O, $), U = le(O, Q), X = [
          // with the margin added after the rotation is applied
          ce.x + F,
          ce.y + H,
          ie.x + F,
          ie.y + H,
          U.x + F,
          U.y + H,
          he.x + F,
          he.y + H
        ];
        if (Ut(t, e, X))
          return g(x), !0;
      } else if (jr(R, t, e))
        return g(x), !0;
    }
  }
  for (var E = s.length - 1; E >= 0; E--) {
    var T = s[E];
    T.isNode() ? p(T) || w(T) : m(T) || w(T) || w(T, "source") || w(T, "target");
  }
  return o;
};
_n.getAllInBox = function(t, e, r, n) {
  var a = this.getCachedZSortedEles().interactive, i = this.cy.zoom(), s = 2 / i, o = [], l = Math.min(t, r), u = Math.max(t, r), f = Math.min(e, n), c = Math.max(e, n);
  t = l, r = u, e = f, n = c;
  var v = zt({
    x1: t,
    y1: e,
    x2: r,
    y2: n
  }), d = [{
    x: v.x1,
    y: v.y1
  }, {
    x: v.x2,
    y: v.y1
  }, {
    x: v.x2,
    y: v.y2
  }, {
    x: v.x1,
    y: v.y2
  }], h = [[d[0], d[1]], [d[1], d[2]], [d[2], d[3]], [d[3], d[0]]];
  function y(B, z, W) {
    return Nt(B, z, W);
  }
  function g(B, z) {
    var W = B._private, j = s, Z = "";
    B.boundingBox();
    var ne = W.labelBounds.main;
    if (!ne)
      return null;
    var te = y(W.rscratch, "labelX", z), Y = y(W.rscratch, "labelY", z), K = y(W.rscratch, "labelAngle", z), ue = B.pstyle(Z + "text-margin-x").pfValue, oe = B.pstyle(Z + "text-margin-y").pfValue, ve = ne.x1 - j - ue, de = ne.x2 + j - ue, me = ne.y1 - j - oe, Te = ne.y2 + j - oe;
    if (K) {
      var Ee = Math.cos(K), Pe = Math.sin(K), J = function(q, G) {
        return q = q - te, G = G - Y, {
          x: q * Ee - G * Pe + te,
          y: q * Pe + G * Ee + Y
        };
      };
      return [J(ve, me), J(de, me), J(de, Te), J(ve, Te)];
    } else
      return [{
        x: ve,
        y: me
      }, {
        x: de,
        y: me
      }, {
        x: de,
        y: Te
      }, {
        x: ve,
        y: Te
      }];
  }
  function p(B, z, W, j) {
    function Z(ne, te, Y) {
      return (Y.y - ne.y) * (te.x - ne.x) > (te.y - ne.y) * (Y.x - ne.x);
    }
    return Z(B, W, j) !== Z(z, W, j) && Z(B, z, W) !== Z(B, z, j);
  }
  for (var m = 0; m < a.length; m++) {
    var b = a[m];
    if (b.isNode()) {
      var w = b, E = w.pstyle("text-events").strValue === "yes", T = w.pstyle("box-selection").strValue, x = w.pstyle("box-select-labels").strValue === "yes";
      if (T === "none")
        continue;
      var S = (T === "overlap" || x) && E, D = w.boundingBox({
        includeNodes: !0,
        includeEdges: !1,
        includeLabels: S
      });
      if (T === "contain") {
        var A = !1;
        if (x && E) {
          var k = g(w);
          k && qo(k, d) && (o.push(w), A = !0);
        }
        !A && ag(v, D) && o.push(w);
      } else if (T === "overlap" && ef(v, D)) {
        var R = w.boundingBox({
          includeNodes: !0,
          includeEdges: !0,
          includeLabels: !1,
          includeMainLabels: !1,
          includeSourceLabels: !1,
          includeTargetLabels: !1
        }), M = [{
          x: R.x1,
          y: R.y1
        }, {
          x: R.x2,
          y: R.y1
        }, {
          x: R.x2,
          y: R.y2
        }, {
          x: R.x1,
          y: R.y2
        }];
        if (qo(M, d))
          o.push(w);
        else {
          var I = g(w);
          I && qo(I, d) && o.push(w);
        }
      }
    } else {
      var _ = b, N = _._private, L = N.rscratch, F = _.pstyle("box-selection").strValue;
      if (F === "none")
        continue;
      if (F === "contain") {
        if (L.startX != null && L.startY != null && !jr(v, L.startX, L.startY) || L.endX != null && L.endY != null && !jr(v, L.endX, L.endY))
          continue;
        if (L.edgeType === "bezier" || L.edgeType === "multibezier" || L.edgeType === "self" || L.edgeType === "compound" || L.edgeType === "segments" || L.edgeType === "haystack") {
          for (var H = N.rstyle.bezierPts || N.rstyle.linePts || N.rstyle.haystackPts, V = !0, O = 0; O < H.length; O++)
            if (!Cc(v, H[O])) {
              V = !1;
              break;
            }
          V && o.push(_);
        } else L.edgeType === "straight" && o.push(_);
      } else if (F === "overlap") {
        var $ = !1;
        if (L.startX != null && L.startY != null && L.endX != null && L.endY != null && (jr(v, L.startX, L.startY) || jr(v, L.endX, L.endY)))
          o.push(_), $ = !0;
        else if (!$ && L.edgeType === "haystack") {
          for (var Q = N.rstyle.haystackPts, se = 0; se < Q.length; se++)
            if (Cc(v, Q[se])) {
              o.push(_), $ = !0;
              break;
            }
        }
        if (!$) {
          var ae = N.rstyle.bezierPts || N.rstyle.linePts || N.rstyle.haystackPts;
          if ((!ae || ae.length < 2) && L.edgeType === "straight" && L.startX != null && L.startY != null && L.endX != null && L.endY != null && (ae = [{
            x: L.startX,
            y: L.startY
          }, {
            x: L.endX,
            y: L.endY
          }]), !ae || ae.length < 2) continue;
          for (var le = 0; le < ae.length - 1; le++) {
            for (var ce = ae[le], he = ae[le + 1], ie = 0; ie < h.length; ie++) {
              var U = ut(h[ie], 2), X = U[0], C = U[1];
              if (p(ce, he, X, C)) {
                o.push(_), $ = !0;
                break;
              }
            }
            if ($) break;
          }
        }
      }
    }
  }
  return o;
};
var Ds = {};
Ds.calculateArrowAngles = function(t) {
  var e = t._private.rscratch, r = e.edgeType === "haystack", n = e.edgeType === "bezier", a = e.edgeType === "multibezier", i = e.edgeType === "segments", s = e.edgeType === "compound", o = e.edgeType === "self", l, u, f, c, v, d, p, m;
  if (r ? (f = e.haystackPts[0], c = e.haystackPts[1], v = e.haystackPts[2], d = e.haystackPts[3]) : (f = e.arrowStartX, c = e.arrowStartY, v = e.arrowEndX, d = e.arrowEndY), p = e.midX, m = e.midY, i)
    l = f - e.segpts[0], u = c - e.segpts[1];
  else if (a || s || o || n) {
    var h = e.allpts, y = yt(h[0], h[2], h[4], 0.1), g = yt(h[1], h[3], h[5], 0.1);
    l = f - y, u = c - g;
  } else
    l = f - p, u = c - m;
  e.srcArrowAngle = Oi(l, u);
  var p = e.midX, m = e.midY;
  if (r && (p = (f + v) / 2, m = (c + d) / 2), l = v - f, u = d - c, i) {
    var h = e.allpts;
    if (h.length / 2 % 2 === 0) {
      var b = h.length / 2, w = b - 2;
      l = h[b] - h[w], u = h[b + 1] - h[w + 1];
    } else if (e.isRound)
      l = e.midVector[1], u = -e.midVector[0];
    else {
      var b = h.length / 2 - 1, w = b - 2;
      l = h[b] - h[w], u = h[b + 1] - h[w + 1];
    }
  } else if (a || s || o) {
    var h = e.allpts, E = e.ctrlpts, T, x, S, D;
    if (E.length / 2 % 2 === 0) {
      var A = h.length / 2 - 1, k = A + 2, R = k + 2;
      T = yt(h[A], h[k], h[R], 0), x = yt(h[A + 1], h[k + 1], h[R + 1], 0), S = yt(h[A], h[k], h[R], 1e-4), D = yt(h[A + 1], h[k + 1], h[R + 1], 1e-4);
    } else {
      var k = h.length / 2 - 1, A = k - 2, R = k + 2;
      T = yt(h[A], h[k], h[R], 0.4999), x = yt(h[A + 1], h[k + 1], h[R + 1], 0.4999), S = yt(h[A], h[k], h[R], 0.5), D = yt(h[A + 1], h[k + 1], h[R + 1], 0.5);
    }
    l = S - T, u = D - x;
  }
  if (e.midtgtArrowAngle = Oi(l, u), e.midDispX = l, e.midDispY = u, l *= -1, u *= -1, i) {
    var h = e.allpts;
    if (h.length / 2 % 2 !== 0) {
      if (!e.isRound) {
        var b = h.length / 2 - 1, M = b + 2;
        l = -(h[M] - h[b]), u = -(h[M + 1] - h[b + 1]);
      }
    }
  }
  if (e.midsrcArrowAngle = Oi(l, u), i)
    l = v - e.segpts[e.segpts.length - 2], u = d - e.segpts[e.segpts.length - 1];
  else if (a || s || o || n) {
    var h = e.allpts, I = h.length, y = yt(h[I - 6], h[I - 4], h[I - 2], 0.9), g = yt(h[I - 5], h[I - 3], h[I - 1], 0.9);
    l = v - y, u = d - g;
  } else
    l = v - p, u = d - m;
  e.tgtArrowAngle = Oi(l, u);
};
Ds.getArrowWidth = Ds.getArrowHeight = function(t, e) {
  var r = this.arrowWidthCache = this.arrowWidthCache || {}, n = r[t + ", " + e];
  return n || (n = Math.max(Math.pow(t * 13.37, 0.9), 29) * e, r[t + ", " + e] = n, n);
};
var wu, xu, gr = {}, Qt = {}, id, sd, Sn, is, kr, mn, En, dr, Un, Hi, jg, Jg, Eu, Cu, od, ld = function(e, r, n) {
  n.x = r.x - e.x, n.y = r.y - e.y, n.len = Math.sqrt(n.x * n.x + n.y * n.y), n.nx = n.x / n.len, n.ny = n.y / n.len, n.ang = Math.atan2(n.ny, n.nx);
}, hE = function(e, r) {
  r.x = e.x * -1, r.y = e.y * -1, r.nx = e.nx * -1, r.ny = e.ny * -1, r.ang = e.ang > 0 ? -(Math.PI - e.ang) : Math.PI + e.ang;
}, gE = function(e, r, n, a, i) {
  if (e !== od ? ld(r, e, gr) : hE(Qt, gr), ld(r, n, Qt), id = gr.nx * Qt.ny - gr.ny * Qt.nx, sd = gr.nx * Qt.nx - gr.ny * -Qt.ny, kr = Math.asin(Math.max(-1, Math.min(1, id))), Math.abs(kr) < 1e-6) {
    wu = r.x, xu = r.y, En = Un = 0;
    return;
  }
  Sn = 1, is = !1, sd < 0 ? kr < 0 ? kr = Math.PI + kr : (kr = Math.PI - kr, Sn = -1, is = !0) : kr > 0 && (Sn = -1, is = !0), r.radius !== void 0 ? Un = r.radius : Un = a, mn = kr / 2, Hi = Math.min(gr.len / 2, Qt.len / 2), i ? (dr = Math.abs(Math.cos(mn) * Un / Math.sin(mn)), dr > Hi ? (dr = Hi, En = Math.abs(dr * Math.sin(mn) / Math.cos(mn))) : En = Un) : (dr = Math.min(Hi, Un), En = Math.abs(dr * Math.sin(mn) / Math.cos(mn))), Eu = r.x + Qt.nx * dr, Cu = r.y + Qt.ny * dr, wu = Eu - Qt.ny * En * Sn, xu = Cu + Qt.nx * En * Sn, jg = r.x + gr.nx * dr, Jg = r.y + gr.ny * dr, od = r;
};
function ep(t, e) {
  e.radius === 0 ? t.lineTo(e.cx, e.cy) : t.arc(e.cx, e.cy, e.radius, e.startAngle, e.endAngle, e.counterClockwise);
}
function df(t, e, r, n) {
  var a = arguments.length > 4 && arguments[4] !== void 0 ? arguments[4] : !0;
  return n === 0 || e.radius === 0 ? {
    cx: e.x,
    cy: e.y,
    radius: 0,
    startX: e.x,
    startY: e.y,
    stopX: e.x,
    stopY: e.y,
    startAngle: void 0,
    endAngle: void 0,
    counterClockwise: void 0
  } : (gE(t, e, r, n, a), {
    cx: wu,
    cy: xu,
    radius: En,
    startX: jg,
    startY: Jg,
    stopX: Eu,
    stopY: Cu,
    startAngle: gr.ang + Math.PI / 2 * Sn,
    endAngle: Qt.ang - Math.PI / 2 * Sn,
    counterClockwise: is
  });
}
var fi = 0.01, pE = Math.sqrt(2 * fi), Lt = {};
Lt.findMidptPtsEtc = function(t, e) {
  var r = e.posPts, n = e.intersectionPts, a = e.vectorNormInverse, i, s = t.pstyle("source-endpoint"), o = t.pstyle("target-endpoint"), l = s.units != null && o.units != null, u = function(E, T, x, S) {
    var D = S - T, A = x - E, k = Math.sqrt(A * A + D * D);
    return {
      x: -D / k,
      y: A / k
    };
  }, f = t.pstyle("edge-distances").value;
  switch (f) {
    case "node-position":
      i = r;
      break;
    case "intersection":
      i = n;
      break;
    case "endpoints": {
      if (l) {
        var c = this.manualEndptToPx(t.source()[0], s), v = ut(c, 2), d = v[0], h = v[1], y = this.manualEndptToPx(t.target()[0], o), g = ut(y, 2), p = g[0], m = g[1], b = {
          x1: d,
          y1: h,
          x2: p,
          y2: m
        };
        a = u(d, h, p, m), i = b;
      } else
        $e("Edge ".concat(t.id(), " has edge-distances:endpoints specified without manual endpoints specified via source-endpoint and target-endpoint.  Falling back on edge-distances:intersection (default).")), i = n;
      break;
    }
  }
  return {
    midptPts: i,
    vectorNormInverse: a
  };
};
Lt.findHaystackPoints = function(t) {
  for (var e = 0; e < t.length; e++) {
    var r = t[e], n = r._private, a = n.rscratch;
    if (!a.haystack) {
      var i = Math.random() * 2 * Math.PI;
      a.source = {
        x: Math.cos(i),
        y: Math.sin(i)
      }, i = Math.random() * 2 * Math.PI, a.target = {
        x: Math.cos(i),
        y: Math.sin(i)
      };
    }
    var s = n.source, o = n.target, l = s.position(), u = o.position(), f = s.width(), c = o.width(), v = s.height(), d = o.height(), h = r.pstyle("haystack-radius").value, y = h / 2;
    a.haystackPts = a.allpts = [a.source.x * f * y + l.x, a.source.y * v * y + l.y, a.target.x * c * y + u.x, a.target.y * d * y + u.y], a.midX = (a.allpts[0] + a.allpts[2]) / 2, a.midY = (a.allpts[1] + a.allpts[3]) / 2, a.edgeType = "haystack", a.haystack = !0, this.storeEdgeProjections(r), this.calculateArrowAngles(r), this.recalculateEdgeLabelProjections(r), this.calculateLabelAngles(r);
  }
};
Lt.findSegmentsPoints = function(t, e) {
  var r = t._private.rscratch, n = t.pstyle("segment-weights"), a = t.pstyle("segment-distances"), i = t.pstyle("segment-radii"), s = t.pstyle("radius-type"), o = Math.min(n.pfValue.length, a.pfValue.length), l = i.pfValue[i.pfValue.length - 1], u = s.pfValue[s.pfValue.length - 1];
  r.edgeType = "segments", r.segpts = [], r.radii = [], r.isArcRadius = [];
  for (var f = 0; f < o; f++) {
    var c = n.pfValue[f], v = a.pfValue[f], d = 1 - c, h = c, y = this.findMidptPtsEtc(t, e), g = y.midptPts, p = y.vectorNormInverse, m = {
      x: g.x1 * d + g.x2 * h,
      y: g.y1 * d + g.y2 * h
    };
    r.segpts.push(m.x + p.x * v, m.y + p.y * v), r.radii.push(i.pfValue[f] !== void 0 ? i.pfValue[f] : l), r.isArcRadius.push((s.pfValue[f] !== void 0 ? s.pfValue[f] : u) === "arc-radius");
  }
};
Lt.findLoopPoints = function(t, e, r, n) {
  var a = t._private.rscratch, i = e.dirCounts, s = e.srcPos, o = t.pstyle("control-point-distances"), l = o ? o.pfValue[0] : void 0, u = t.pstyle("loop-direction").pfValue, f = t.pstyle("loop-sweep").pfValue, c = t.pstyle("control-point-step-size").pfValue;
  a.edgeType = "self";
  var v = r, d = c;
  n && (v = 0, d = l);
  var h = u - Math.PI / 2, y = h - f / 2, g = h + f / 2, p = u + "_" + f;
  v = i[p] === void 0 ? i[p] = 0 : ++i[p], a.ctrlpts = [s.x + Math.cos(y) * 1.4 * d * (v / 3 + 1), s.y + Math.sin(y) * 1.4 * d * (v / 3 + 1), s.x + Math.cos(g) * 1.4 * d * (v / 3 + 1), s.y + Math.sin(g) * 1.4 * d * (v / 3 + 1)];
};
Lt.findCompoundLoopPoints = function(t, e, r, n) {
  var a = t._private.rscratch;
  a.edgeType = "compound";
  var i = e.srcPos, s = e.tgtPos, o = e.srcW, l = e.srcH, u = e.tgtW, f = e.tgtH, c = t.pstyle("control-point-step-size").pfValue, v = t.pstyle("control-point-distances"), d = v ? v.pfValue[0] : void 0, h = r, y = c;
  n && (h = 0, y = d);
  var g = 50, p = {
    x: i.x - o / 2,
    y: i.y - l / 2
  }, m = {
    x: s.x - u / 2,
    y: s.y - f / 2
  }, b = {
    x: Math.min(p.x, m.x),
    y: Math.min(p.y, m.y)
  }, w = 0.5, E = Math.max(w, Math.log(o * fi)), T = Math.max(w, Math.log(u * fi));
  a.ctrlpts = [b.x, b.y - (1 + Math.pow(g, 1.12) / 100) * y * (h / 3 + 1) * E, b.x - (1 + Math.pow(g, 1.12) / 100) * y * (h / 3 + 1) * T, b.y];
};
Lt.findStraightEdgePoints = function(t) {
  t._private.rscratch.edgeType = "straight";
};
Lt.findBezierPoints = function(t, e, r, n, a) {
  var i = t._private.rscratch, s = t.pstyle("control-point-step-size").pfValue, o = t.pstyle("control-point-distances"), l = t.pstyle("control-point-weights"), u = o && l ? Math.min(o.value.length, l.value.length) : 1, f = o ? o.pfValue[0] : void 0, c = l.value[0], v = n;
  i.edgeType = v ? "multibezier" : "bezier", i.ctrlpts = [];
  for (var d = 0; d < u; d++) {
    var h = (0.5 - e.eles.length / 2 + r) * s * (a ? -1 : 1), y = void 0, g = Ju(h);
    v && (f = o ? o.pfValue[d] : s, c = l.value[d]), n ? y = f : y = f !== void 0 ? g * f : void 0;
    var p = y !== void 0 ? y : h, m = 1 - c, b = c, w = this.findMidptPtsEtc(t, e), E = w.midptPts, T = w.vectorNormInverse, x = {
      x: E.x1 * m + E.x2 * b,
      y: E.y1 * m + E.y2 * b
    };
    i.ctrlpts.push(x.x + T.x * p, x.y + T.y * p);
  }
};
Lt.findTaxiPoints = function(t, e) {
  var r = t._private.rscratch;
  r.edgeType = "segments";
  var n = "vertical", a = "horizontal", i = "leftward", s = "rightward", o = "downward", l = "upward", u = "auto", f = e.posPts, c = e.srcW, v = e.srcH, d = e.tgtW, h = e.tgtH, y = t.pstyle("edge-distances").value, g = y !== "node-position", p = t.pstyle("taxi-direction").value, m = p, b = t.pstyle("taxi-turn"), w = b.units === "%", E = b.pfValue, T = E < 0, x = t.pstyle("taxi-turn-min-distance").pfValue, S = g ? (c + d) / 2 : 0, D = g ? (v + h) / 2 : 0, A = f.x2 - f.x1, k = f.y2 - f.y1, R = function(G, re) {
    return G > 0 ? Math.max(G - re, 0) : Math.min(G + re, 0);
  }, M = R(A, S), I = R(k, D), _ = !1;
  m === u ? p = Math.abs(M) > Math.abs(I) ? a : n : m === l || m === o ? (p = n, _ = !0) : (m === i || m === s) && (p = a, _ = !0);
  var N = p === n, L = N ? I : M, F = N ? k : A, H = Ju(F), V = !1;
  !(_ && (w || T)) && (m === o && F < 0 || m === l && F > 0 || m === i && F > 0 || m === s && F < 0) && (H *= -1, L = H * Math.abs(L), V = !0);
  var O;
  if (w) {
    var $ = E < 0 ? 1 + E : E;
    O = $ * L;
  } else {
    var Q = E < 0 ? L : 0;
    O = Q + E * H;
  }
  var se = function(G) {
    return Math.abs(G) < x || Math.abs(G) >= Math.abs(L);
  }, ae = se(O), le = se(Math.abs(L) - Math.abs(O)), ce = ae || le;
  if (ce && !V)
    if (N) {
      var he = Math.abs(F) <= v / 2, ie = Math.abs(A) <= d / 2;
      if (he) {
        var U = (f.x1 + f.x2) / 2, X = f.y1, C = f.y2;
        r.segpts = [U, X, U, C];
      } else if (ie) {
        var B = (f.y1 + f.y2) / 2, z = f.x1, W = f.x2;
        r.segpts = [z, B, W, B];
      } else
        r.segpts = [f.x1, f.y2];
    } else {
      var j = Math.abs(F) <= c / 2, Z = Math.abs(k) <= h / 2;
      if (j) {
        var ne = (f.y1 + f.y2) / 2, te = f.x1, Y = f.x2;
        r.segpts = [te, ne, Y, ne];
      } else if (Z) {
        var K = (f.x1 + f.x2) / 2, ue = f.y1, oe = f.y2;
        r.segpts = [K, ue, K, oe];
      } else
        r.segpts = [f.x2, f.y1];
    }
  else if (N) {
    var ve = f.y1 + O + (g ? v / 2 * H : 0), de = f.x1, me = f.x2;
    r.segpts = [de, ve, me, ve];
  } else {
    var Te = f.x1 + O + (g ? c / 2 * H : 0), Ee = f.y1, Pe = f.y2;
    r.segpts = [Te, Ee, Te, Pe];
  }
  if (r.isRound) {
    var J = t.pstyle("taxi-radius").value, P = t.pstyle("radius-type").value[0] === "arc-radius";
    r.radii = new Array(r.segpts.length / 2).fill(J), r.isArcRadius = new Array(r.segpts.length / 2).fill(P);
  }
};
Lt.tryToCorrectInvalidPoints = function(t, e) {
  var r = t._private.rscratch;
  if (r.edgeType === "bezier") {
    var n = e.srcPos, a = e.tgtPos, i = e.srcW, s = e.srcH, o = e.tgtW, l = e.tgtH, u = e.srcShape, f = e.tgtShape, c = e.srcCornerRadius, v = e.tgtCornerRadius, d = e.srcRs, h = e.tgtRs, y = !pe(r.startX) || !pe(r.startY), g = !pe(r.arrowStartX) || !pe(r.arrowStartY), p = !pe(r.endX) || !pe(r.endY), m = !pe(r.arrowEndX) || !pe(r.arrowEndY), b = 3, w = this.getArrowWidth(t.pstyle("width").pfValue, t.pstyle("arrow-scale").value) * this.arrowShapeWidth, E = b * w, T = Rn({
      x: r.ctrlpts[0],
      y: r.ctrlpts[1]
    }, {
      x: r.startX,
      y: r.startY
    }), x = T < E, S = Rn({
      x: r.ctrlpts[0],
      y: r.ctrlpts[1]
    }, {
      x: r.endX,
      y: r.endY
    }), D = S < E, A = !1;
    if (y || g || x) {
      A = !0;
      var k = {
        // delta
        x: r.ctrlpts[0] - n.x,
        y: r.ctrlpts[1] - n.y
      }, R = Math.sqrt(k.x * k.x + k.y * k.y), M = {
        // normalised delta
        x: k.x / R,
        y: k.y / R
      }, I = Math.max(i, s), _ = {
        // *2 radius guarantees outside shape
        x: r.ctrlpts[0] + M.x * 2 * I,
        y: r.ctrlpts[1] + M.y * 2 * I
      }, N = u.intersectLine(n.x, n.y, i, s, _.x, _.y, 0, c, d);
      x ? (r.ctrlpts[0] = r.ctrlpts[0] + M.x * (E - T), r.ctrlpts[1] = r.ctrlpts[1] + M.y * (E - T)) : (r.ctrlpts[0] = N[0] + M.x * E, r.ctrlpts[1] = N[1] + M.y * E);
    }
    if (p || m || D) {
      A = !0;
      var L = {
        // delta
        x: r.ctrlpts[0] - a.x,
        y: r.ctrlpts[1] - a.y
      }, F = Math.sqrt(L.x * L.x + L.y * L.y), H = {
        // normalised delta
        x: L.x / F,
        y: L.y / F
      }, V = Math.max(i, s), O = {
        // *2 radius guarantees outside shape
        x: r.ctrlpts[0] + H.x * 2 * V,
        y: r.ctrlpts[1] + H.y * 2 * V
      }, $ = f.intersectLine(a.x, a.y, o, l, O.x, O.y, 0, v, h);
      D ? (r.ctrlpts[0] = r.ctrlpts[0] + H.x * (E - S), r.ctrlpts[1] = r.ctrlpts[1] + H.y * (E - S)) : (r.ctrlpts[0] = $[0] + H.x * E, r.ctrlpts[1] = $[1] + H.y * E);
    }
    A && this.findEndpoints(t);
  }
};
Lt.storeAllpts = function(t) {
  var e = t._private.rscratch;
  if (e.edgeType === "multibezier" || e.edgeType === "bezier" || e.edgeType === "self" || e.edgeType === "compound") {
    e.allpts = [], e.allpts.push(e.startX, e.startY);
    for (var r = 0; r + 1 < e.ctrlpts.length; r += 2)
      e.allpts.push(e.ctrlpts[r], e.ctrlpts[r + 1]), r + 3 < e.ctrlpts.length && e.allpts.push((e.ctrlpts[r] + e.ctrlpts[r + 2]) / 2, (e.ctrlpts[r + 1] + e.ctrlpts[r + 3]) / 2);
    e.allpts.push(e.endX, e.endY);
    var n, a;
    e.ctrlpts.length / 2 % 2 === 0 ? (n = e.allpts.length / 2 - 1, e.midX = e.allpts[n], e.midY = e.allpts[n + 1]) : (n = e.allpts.length / 2 - 3, a = 0.5, e.midX = yt(e.allpts[n], e.allpts[n + 2], e.allpts[n + 4], a), e.midY = yt(e.allpts[n + 1], e.allpts[n + 3], e.allpts[n + 5], a));
  } else if (e.edgeType === "straight")
    e.allpts = [e.startX, e.startY, e.endX, e.endY], e.midX = (e.startX + e.endX + e.arrowStartX + e.arrowEndX) / 4, e.midY = (e.startY + e.endY + e.arrowStartY + e.arrowEndY) / 4;
  else if (e.edgeType === "segments") {
    if (e.allpts = [], e.allpts.push(e.startX, e.startY), e.allpts.push.apply(e.allpts, e.segpts), e.allpts.push(e.endX, e.endY), e.isRound) {
      e.roundCorners = [];
      for (var i = 2; i + 3 < e.allpts.length; i += 2) {
        var s = e.radii[i / 2 - 1], o = e.isArcRadius[i / 2 - 1];
        e.roundCorners.push(df({
          x: e.allpts[i - 2],
          y: e.allpts[i - 1]
        }, {
          x: e.allpts[i],
          y: e.allpts[i + 1],
          radius: s
        }, {
          x: e.allpts[i + 2],
          y: e.allpts[i + 3]
        }, s, o));
      }
    }
    if (e.segpts.length % 4 === 0) {
      var l = e.segpts.length / 2, u = l - 2;
      e.midX = (e.segpts[u] + e.segpts[l]) / 2, e.midY = (e.segpts[u + 1] + e.segpts[l + 1]) / 2;
    } else {
      var f = e.segpts.length / 2 - 1;
      if (!e.isRound)
        e.midX = e.segpts[f], e.midY = e.segpts[f + 1];
      else {
        var c = {
          x: e.segpts[f],
          y: e.segpts[f + 1]
        }, v = e.roundCorners[f / 2];
        if (v.radius === 0) {
          var d = {
            x: e.segpts[f + 2],
            y: e.segpts[f + 3]
          };
          e.midX = c.x, e.midY = c.y, e.midVector = [c.y - d.y, d.x - c.x];
        } else {
          var h = [c.x - v.cx, c.y - v.cy], y = v.radius / Math.sqrt(Math.pow(h[0], 2) + Math.pow(h[1], 2));
          h = h.map(function(g) {
            return g * y;
          }), e.midX = v.cx + h[0], e.midY = v.cy + h[1], e.midVector = h;
        }
      }
    }
  }
};
Lt.checkForInvalidEdgeWarning = function(t) {
  var e = t[0]._private.rscratch;
  e.nodesOverlap || pe(e.startX) && pe(e.startY) && pe(e.endX) && pe(e.endY) ? e.loggedErr = !1 : e.loggedErr || (e.loggedErr = !0, $e("Edge `" + t.id() + "` has invalid endpoints and so it is impossible to draw.  Adjust your edge style (e.g. control points) accordingly or use an alternative edge type.  This is expected behaviour when the source node and the target node overlap."));
};
Lt.findEdgeControlPoints = function(t) {
  var e = this;
  if (!(!t || t.length === 0)) {
    for (var r = this, n = r.cy, a = n.hasCompoundNodes(), i = new Nr(), s = function(D, A) {
      return [].concat(ys(D), [A ? 1 : 0]).join("-");
    }, o = [], l = [], u = 0; u < t.length; u++) {
      var f = t[u], c = f._private, v = f.pstyle("curve-style").value;
      if (!(f.removed() || !f.takesUpSpace())) {
        if (v === "haystack") {
          l.push(f);
          continue;
        }
        var d = v === "unbundled-bezier" || Qr(v, "segments") || v === "straight" || v === "straight-triangle" || Qr(v, "taxi"), h = v === "unbundled-bezier" || v === "bezier", y = c.source, g = c.target, p = y.poolIndex(), m = g.poolIndex(), b = [p, m].sort(), w = s(b, d), E = i.get(w);
        E == null && (E = {
          eles: []
        }, o.push({
          pairId: b,
          edgeIsUnbundled: d
        }), i.set(w, E)), E.eles.push(f), d && (E.hasUnbundled = !0), h && (E.hasBezier = !0);
      }
    }
    for (var T = function() {
      var D = o[x], A = D.pairId, k = D.edgeIsUnbundled, R = s(A, k), M = i.get(R), I;
      if (!M.hasUnbundled) {
        var _ = M.eles[0].parallelEdges().filter(function(P) {
          return P.isBundledBezier();
        });
        Qu(M.eles), _.forEach(function(P) {
          return M.eles.push(P);
        }), M.eles.sort(function(P, q) {
          return P.poolIndex() - q.poolIndex();
        });
      }
      var N = M.eles[0], L = N.source(), F = N.target();
      if (L.poolIndex() > F.poolIndex()) {
        var H = L;
        L = F, F = H;
      }
      var V = M.srcPos = L.position(), O = M.tgtPos = F.position(), $ = M.srcW = L.outerWidth(), Q = M.srcH = L.outerHeight(), se = M.tgtW = F.outerWidth(), ae = M.tgtH = F.outerHeight(), le = M.srcShape = r.nodeShapes[e.getNodeShape(L)], ce = M.tgtShape = r.nodeShapes[e.getNodeShape(F)], he = M.srcCornerRadius = L.pstyle("corner-radius").value === "auto" ? "auto" : L.pstyle("corner-radius").pfValue, ie = M.tgtCornerRadius = F.pstyle("corner-radius").value === "auto" ? "auto" : F.pstyle("corner-radius").pfValue, U = M.tgtRs = F._private.rscratch, X = M.srcRs = L._private.rscratch;
      M.dirCounts = {
        north: 0,
        west: 0,
        south: 0,
        east: 0,
        northwest: 0,
        southwest: 0,
        northeast: 0,
        southeast: 0
      };
      for (var C = 0; C < M.eles.length; C++) {
        var B = M.eles[C], z = B[0]._private.rscratch, W = B.pstyle("curve-style").value, j = W === "unbundled-bezier" || Qr(W, "segments") || Qr(W, "taxi"), Z = !L.same(B.source());
        if (!M.calculatedIntersection && L !== F && (M.hasBezier || M.hasUnbundled)) {
          M.calculatedIntersection = !0;
          var ne = le.intersectLine(V.x, V.y, $, Q, O.x, O.y, 0, he, X), te = M.srcIntn = ne, Y = ce.intersectLine(O.x, O.y, se, ae, V.x, V.y, 0, ie, U), K = M.tgtIntn = Y, ue = M.intersectionPts = {
            x1: ne[0],
            x2: Y[0],
            y1: ne[1],
            y2: Y[1]
          }, oe = M.posPts = {
            x1: V.x,
            x2: O.x,
            y1: V.y,
            y2: O.y
          }, ve = Y[1] - ne[1], de = Y[0] - ne[0], me = Math.sqrt(de * de + ve * ve);
          pe(me) && me >= pE || (me = Math.sqrt(Math.max(de * de, fi) + Math.max(ve * ve, fi)));
          var Te = M.vector = {
            x: de,
            y: ve
          }, Ee = M.vectorNorm = {
            x: Te.x / me,
            y: Te.y / me
          }, Pe = {
            x: -Ee.y,
            y: Ee.x
          };
          M.nodesOverlap = !pe(me) || ce.checkPoint(ne[0], ne[1], 0, se, ae, O.x, O.y, ie, U) || le.checkPoint(Y[0], Y[1], 0, $, Q, V.x, V.y, he, X), M.vectorNormInverse = Pe, I = {
            nodesOverlap: M.nodesOverlap,
            dirCounts: M.dirCounts,
            calculatedIntersection: !0,
            hasBezier: M.hasBezier,
            hasUnbundled: M.hasUnbundled,
            eles: M.eles,
            srcPos: O,
            srcRs: U,
            tgtPos: V,
            tgtRs: X,
            srcW: se,
            srcH: ae,
            tgtW: $,
            tgtH: Q,
            srcIntn: K,
            tgtIntn: te,
            srcShape: ce,
            tgtShape: le,
            posPts: {
              x1: oe.x2,
              y1: oe.y2,
              x2: oe.x1,
              y2: oe.y1
            },
            intersectionPts: {
              x1: ue.x2,
              y1: ue.y2,
              x2: ue.x1,
              y2: ue.y1
            },
            vector: {
              x: -Te.x,
              y: -Te.y
            },
            vectorNorm: {
              x: -Ee.x,
              y: -Ee.y
            },
            vectorNormInverse: {
              x: -Pe.x,
              y: -Pe.y
            }
          };
        }
        var J = Z ? I : M;
        z.nodesOverlap = J.nodesOverlap, z.srcIntn = J.srcIntn, z.tgtIntn = J.tgtIntn, z.isRound = W.startsWith("round"), a && (L.isParent() || L.isChild() || F.isParent() || F.isChild()) && (L.parents().anySame(F) || F.parents().anySame(L) || L.same(F) && L.isParent()) ? e.findCompoundLoopPoints(B, J, C, j) : L === F ? e.findLoopPoints(B, J, C, j) : W.endsWith("segments") ? e.findSegmentsPoints(B, J) : W.endsWith("taxi") ? e.findTaxiPoints(B, J) : W === "straight" || !j && M.eles.length % 2 === 1 && C === Math.floor(M.eles.length / 2) ? e.findStraightEdgePoints(B) : e.findBezierPoints(B, J, C, j, Z), e.findEndpoints(B), e.tryToCorrectInvalidPoints(B, J), e.checkForInvalidEdgeWarning(B), e.storeAllpts(B), e.storeEdgeProjections(B), e.calculateArrowAngles(B), e.recalculateEdgeLabelProjections(B), e.calculateLabelAngles(B);
      }
    }, x = 0; x < o.length; x++)
      T();
    this.findHaystackPoints(l);
  }
};
function tp(t) {
  var e = [];
  if (t != null) {
    for (var r = 0; r < t.length; r += 2) {
      var n = t[r], a = t[r + 1];
      e.push({
        x: n,
        y: a
      });
    }
    return e;
  }
}
Lt.getSegmentPoints = function(t) {
  var e = t[0]._private.rscratch;
  this.recalculateRenderedStyle(t);
  var r = e.edgeType;
  if (r === "segments")
    return tp(e.segpts);
};
Lt.getControlPoints = function(t) {
  var e = t[0]._private.rscratch;
  this.recalculateRenderedStyle(t);
  var r = e.edgeType;
  if (r === "bezier" || r === "multibezier" || r === "self" || r === "compound")
    return tp(e.ctrlpts);
};
Lt.getEdgeMidpoint = function(t) {
  var e = t[0]._private.rscratch;
  return this.recalculateRenderedStyle(t), {
    x: e.midX,
    y: e.midY
  };
};
var Ti = {};
Ti.manualEndptToPx = function(t, e) {
  var r = this, n = t.position(), a = t.outerWidth(), i = t.outerHeight(), s = t._private.rscratch;
  if (e.value.length === 2) {
    var o = [e.pfValue[0], e.pfValue[1]];
    return e.units[0] === "%" && (o[0] = o[0] * a), e.units[1] === "%" && (o[1] = o[1] * i), o[0] += n.x, o[1] += n.y, o;
  } else {
    var l = e.pfValue[0];
    l = -Math.PI / 2 + l;
    var u = 2 * Math.max(a, i), f = [n.x + Math.cos(l) * u, n.y + Math.sin(l) * u];
    return r.nodeShapes[this.getNodeShape(t)].intersectLine(n.x, n.y, a, i, f[0], f[1], 0, t.pstyle("corner-radius").value === "auto" ? "auto" : t.pstyle("corner-radius").pfValue, s);
  }
};
Ti.findEndpoints = function(t) {
  var e, r, n, a, i = this, s, o = t.source()[0], l = t.target()[0], u = o.position(), f = l.position(), c = t.pstyle("target-arrow-shape").value, v = t.pstyle("source-arrow-shape").value, d = t.pstyle("target-distance-from-node").pfValue, h = t.pstyle("source-distance-from-node").pfValue, y = o._private.rscratch, g = l._private.rscratch, p = t.pstyle("curve-style").value, m = t._private.rscratch, b = m.edgeType, w = Qr(p, "taxi"), E = b === "self" || b === "compound", T = b === "bezier" || b === "multibezier" || E, x = b !== "bezier", S = b === "straight" || b === "segments", D = b === "segments", A = T || x || S, k = E || w, R = t.pstyle("source-endpoint"), M = k ? "outside-to-node" : R.value, I = o.pstyle("corner-radius").value === "auto" ? "auto" : o.pstyle("corner-radius").pfValue, _ = t.pstyle("target-endpoint"), N = k ? "outside-to-node" : _.value, L = l.pstyle("corner-radius").value === "auto" ? "auto" : l.pstyle("corner-radius").pfValue;
  m.srcManEndpt = R, m.tgtManEndpt = _;
  var F, H, V, O, $ = (e = (_ == null || (r = _.pfValue) === null || r === void 0 ? void 0 : r.length) === 2 ? _.pfValue : null) !== null && e !== void 0 ? e : [0, 0], Q = (n = (R == null || (a = R.pfValue) === null || a === void 0 ? void 0 : a.length) === 2 ? R.pfValue : null) !== null && n !== void 0 ? n : [0, 0];
  if (T) {
    var se = [m.ctrlpts[0], m.ctrlpts[1]], ae = x ? [m.ctrlpts[m.ctrlpts.length - 2], m.ctrlpts[m.ctrlpts.length - 1]] : se;
    F = ae, H = se;
  } else if (S) {
    var le = D ? m.segpts.slice(0, 2) : [f.x + $[0], f.y + $[1]], ce = D ? m.segpts.slice(m.segpts.length - 2) : [u.x + Q[0], u.y + Q[1]];
    F = ce, H = le;
  }
  if (N === "inside-to-node")
    s = [f.x, f.y];
  else if (_.units)
    s = this.manualEndptToPx(l, _);
  else if (N === "outside-to-line")
    s = m.tgtIntn;
  else if (N === "outside-to-node" || N === "outside-to-node-or-label" ? V = F : (N === "outside-to-line" || N === "outside-to-line-or-label") && (V = [u.x, u.y]), s = i.nodeShapes[this.getNodeShape(l)].intersectLine(f.x, f.y, l.outerWidth(), l.outerHeight(), V[0], V[1], 0, L, g), N === "outside-to-node-or-label" || N === "outside-to-line-or-label") {
    var he = l._private.rscratch, ie = he.labelWidth, U = he.labelHeight, X = he.labelX, C = he.labelY, B = ie / 2, z = U / 2, W = l.pstyle("text-valign").value;
    W = ga(W), W === "top" ? C -= z : W === "bottom" && (C += z);
    var j = l.pstyle("text-halign").value;
    j = ha(j), j === "left" ? X -= B : j === "right" && (X += B);
    var Z = ii(V[0], V[1], [X - B, C - z, X + B, C - z, X + B, C + z, X - B, C + z], f.x, f.y);
    if (Z.length > 0) {
      var ne = u, te = xn(ne, Zn(s)), Y = xn(ne, Zn(Z)), K = te;
      if (Y < te && (s = Z, K = Y), Z.length > 2) {
        var ue = xn(ne, {
          x: Z[2],
          y: Z[3]
        });
        ue < K && (s = [Z[2], Z[3]]);
      }
    }
  }
  var oe = Ni(s, F, i.arrowShapes[c].spacing(t) + d), ve = Ni(s, F, i.arrowShapes[c].gap(t) + d);
  if (m.endX = ve[0], m.endY = ve[1], m.arrowEndX = oe[0], m.arrowEndY = oe[1], M === "inside-to-node")
    s = [u.x, u.y];
  else if (R.units)
    s = this.manualEndptToPx(o, R);
  else if (M === "outside-to-line")
    s = m.srcIntn;
  else if (M === "outside-to-node" || M === "outside-to-node-or-label" ? O = H : (M === "outside-to-line" || M === "outside-to-line-or-label") && (O = [f.x, f.y]), s = i.nodeShapes[this.getNodeShape(o)].intersectLine(u.x, u.y, o.outerWidth(), o.outerHeight(), O[0], O[1], 0, I, y), M === "outside-to-node-or-label" || M === "outside-to-line-or-label") {
    var de = o._private.rscratch, me = de.labelWidth, Te = de.labelHeight, Ee = de.labelX, Pe = de.labelY, J = me / 2, P = Te / 2, q = o.pstyle("text-valign").value;
    q = ga(q), q === "top" ? Pe -= P : q === "bottom" && (Pe += P);
    var G = o.pstyle("text-halign").value;
    G = ha(G), G === "left" ? Ee -= J : G === "right" && (Ee += J);
    var re = ii(O[0], O[1], [Ee - J, Pe - P, Ee + J, Pe - P, Ee + J, Pe + P, Ee - J, Pe + P], u.x, u.y);
    if (re.length > 0) {
      var ee = f, ye = xn(ee, Zn(s)), fe = xn(ee, Zn(re)), ge = ye;
      if (fe < ye && (s = [re[0], re[1]], ge = fe), re.length > 2) {
        var be = xn(ee, {
          x: re[2],
          y: re[3]
        });
        be < ge && (s = [re[2], re[3]]);
      }
    }
  }
  var De = Ni(s, H, i.arrowShapes[v].spacing(t) + h), Be = Ni(s, H, i.arrowShapes[v].gap(t) + h);
  m.startX = Be[0], m.startY = Be[1], m.arrowStartX = De[0], m.arrowStartY = De[1], A && (!pe(m.startX) || !pe(m.startY) || !pe(m.endX) || !pe(m.endY) ? m.badLine = !0 : m.badLine = !1);
};
Ti.getSourceEndpoint = function(t) {
  var e = t[0]._private.rscratch;
  switch (this.recalculateRenderedStyle(t), e.edgeType) {
    case "haystack":
      return {
        x: e.haystackPts[0],
        y: e.haystackPts[1]
      };
    default:
      return {
        x: e.arrowStartX,
        y: e.arrowStartY
      };
  }
};
Ti.getTargetEndpoint = function(t) {
  var e = t[0]._private.rscratch;
  switch (this.recalculateRenderedStyle(t), e.edgeType) {
    case "haystack":
      return {
        x: e.haystackPts[2],
        y: e.haystackPts[3]
      };
    default:
      return {
        x: e.arrowEndX,
        y: e.arrowEndY
      };
  }
};
var hf = {};
function yE(t, e, r) {
  for (var n = function(u, f, c, v) {
    return yt(u, f, c, v);
  }, a = e._private, i = a.rstyle.bezierPts, s = 0; s < t.bezierProjPcts.length; s++) {
    var o = t.bezierProjPcts[s];
    i.push({
      x: n(r[0], r[2], r[4], o),
      y: n(r[1], r[3], r[5], o)
    });
  }
}
hf.storeEdgeProjections = function(t) {
  var e = t._private, r = e.rscratch, n = r.edgeType;
  if (e.rstyle.bezierPts = null, e.rstyle.linePts = null, e.rstyle.haystackPts = null, n === "multibezier" || n === "bezier" || n === "self" || n === "compound") {
    e.rstyle.bezierPts = [];
    for (var a = 0; a + 5 < r.allpts.length; a += 4)
      yE(this, t, r.allpts.slice(a, a + 6));
  } else if (n === "segments")
    for (var i = e.rstyle.linePts = [], a = 0; a + 1 < r.allpts.length; a += 2)
      i.push({
        x: r.allpts[a],
        y: r.allpts[a + 1]
      });
  else if (n === "haystack") {
    var s = r.haystackPts;
    e.rstyle.haystackPts = [{
      x: s[0],
      y: s[1]
    }, {
      x: s[2],
      y: s[3]
    }];
  }
  e.rstyle.arrowWidth = this.getArrowWidth(t.pstyle("width").pfValue, t.pstyle("arrow-scale").value) * this.arrowShapeWidth;
};
hf.recalculateEdgeProjections = function(t) {
  this.findEdgeControlPoints(t);
};
var Sr = {};
Sr.recalculateNodeLabelProjection = function(t) {
  var e = t.pstyle("label").strValue;
  if (!nn(e)) {
    var r, n, a = t._private, i = t.width(), s = t.height(), o = t.padding(), l = t.position(), u = t.pstyle("text-halign").strValue, f = t.pstyle("text-valign").strValue, c = a.rscratch, v = a.rstyle;
    switch (u) {
      case "left":
        r = l.x - i / 2 - o;
        break;
      case "left-inside":
        r = l.x - i / 2 + o;
        break;
      case "right":
        r = l.x + i / 2 + o;
        break;
      case "right-inside":
        r = l.x + i / 2 - o;
        break;
      default:
        r = l.x;
    }
    switch (f) {
      case "top":
        n = l.y - s / 2 - o;
        break;
      case "top-inside":
        n = l.y - s / 2 + o;
        break;
      case "bottom":
        n = l.y + s / 2 + o;
        break;
      case "bottom-inside":
        n = l.y + s / 2 - o;
        break;
      default:
        n = l.y;
    }
    c.labelX = r, c.labelY = n, v.labelX = r, v.labelY = n, this.calculateLabelAngles(t), this.applyLabelDimensions(t);
  }
};
var rp = function(e, r) {
  var n = Math.atan(r / e);
  return e === 0 && n < 0 && (n = n * -1), n;
}, np = function(e, r) {
  var n = r.x - e.x, a = r.y - e.y;
  return rp(n, a);
}, mE = function(e, r, n, a) {
  var i = ai(0, a - 1e-3, 1), s = ai(0, a + 1e-3, 1), o = aa(e, r, n, i), l = aa(e, r, n, s);
  return np(o, l);
};
Sr.recalculateEdgeLabelProjections = function(t) {
  var e, r = t._private, n = r.rscratch, a = this, i = {
    mid: t.pstyle("label").strValue,
    source: t.pstyle("source-label").strValue,
    target: t.pstyle("target-label").strValue
  };
  if (i.mid || i.source || i.target) {
    e = {
      x: n.midX,
      y: n.midY
    };
    var s = function(c, v, d) {
      yr(r.rscratch, c, v, d), yr(r.rstyle, c, v, d);
    };
    s("labelX", null, e.x), s("labelY", null, e.y);
    var o = rp(n.midDispX, n.midDispY);
    s("labelAutoAngle", null, o);
    var l = function() {
      if (l.cache)
        return l.cache;
      for (var c = [], v = 0; v + 5 < n.allpts.length; v += 4) {
        var d = {
          x: n.allpts[v],
          y: n.allpts[v + 1]
        }, h = {
          x: n.allpts[v + 2],
          y: n.allpts[v + 3]
        }, y = {
          x: n.allpts[v + 4],
          y: n.allpts[v + 5]
        };
        c.push({
          p0: d,
          p1: h,
          p2: y,
          startDist: 0,
          length: 0,
          segments: []
        });
      }
      var g = r.rstyle.bezierPts, p = a.bezierProjPcts.length;
      function m(x, S, D, A, k) {
        var R = Rn(S, D), M = x.segments[x.segments.length - 1], I = {
          p0: S,
          p1: D,
          t0: A,
          t1: k,
          startDist: M ? M.startDist + M.length : 0,
          length: R
        };
        x.segments.push(I), x.length += R;
      }
      for (var b = 0; b < c.length; b++) {
        var w = c[b], E = c[b - 1];
        E && (w.startDist = E.startDist + E.length), m(w, w.p0, g[b * p], 0, a.bezierProjPcts[0]);
        for (var T = 0; T < p - 1; T++)
          m(w, g[b * p + T], g[b * p + T + 1], a.bezierProjPcts[T], a.bezierProjPcts[T + 1]);
        m(w, g[b * p + p - 1], w.p2, a.bezierProjPcts[p - 1], 1);
      }
      return l.cache = c;
    }, u = function(c) {
      var v, d = c === "source";
      if (i[c]) {
        var h = t.pstyle(c + "-text-offset").pfValue;
        switch (n.edgeType) {
          case "self":
          case "compound":
          case "bezier":
          case "multibezier": {
            for (var y = l(), g, p = 0, m = 0, b = 0; b < y.length; b++) {
              for (var w = y[d ? b : y.length - 1 - b], E = 0; E < w.segments.length; E++) {
                var T = w.segments[d ? E : w.segments.length - 1 - E], x = b === y.length - 1 && E === w.segments.length - 1;
                if (p = m, m += T.length, m >= h || x) {
                  g = {
                    cp: w,
                    segment: T
                  };
                  break;
                }
              }
              if (g)
                break;
            }
            var S = g.cp, D = g.segment, A = (h - p) / D.length, k = D.t1 - D.t0, R = d ? D.t0 + k * A : D.t1 - k * A;
            R = ai(0, R, 1), e = aa(S.p0, S.p1, S.p2, R), v = mE(S.p0, S.p1, S.p2, R);
            break;
          }
          case "straight":
          case "segments":
          case "haystack": {
            for (var M = 0, I, _, N, L, F = n.allpts.length, H = 0; H + 3 < F && (d ? (N = {
              x: n.allpts[H],
              y: n.allpts[H + 1]
            }, L = {
              x: n.allpts[H + 2],
              y: n.allpts[H + 3]
            }) : (N = {
              x: n.allpts[F - 2 - H],
              y: n.allpts[F - 1 - H]
            }, L = {
              x: n.allpts[F - 4 - H],
              y: n.allpts[F - 3 - H]
            }), I = Rn(N, L), _ = M, M += I, !(M >= h)); H += 2)
              ;
            var V = h - _, O = V / I;
            O = ai(0, O, 1), e = zb(N, L, O), v = np(N, L);
            break;
          }
        }
        s("labelX", c, e.x), s("labelY", c, e.y), s("labelAutoAngle", c, v);
      }
    };
    u("source"), u("target"), this.applyLabelDimensions(t);
  }
};
Sr.applyLabelDimensions = function(t) {
  this.applyPrefixedLabelDimensions(t), t.isEdge() && (this.applyPrefixedLabelDimensions(t, "source"), this.applyPrefixedLabelDimensions(t, "target"));
};
Sr.applyPrefixedLabelDimensions = function(t, e) {
  var r = t._private, n = this.getLabelText(t, e), a = Bn(n, t._private.labelDimsKey);
  if (Nt(r.rscratch, "prefixedLabelDimsKey", e) !== a) {
    yr(r.rscratch, "prefixedLabelDimsKey", e, a);
    var i = this.calculateLabelDimensions(t, n), s = t.pstyle("line-height").pfValue, o = t.pstyle("font-size").pfValue, l = t.pstyle("text-wrap").strValue, u = Nt(r.rscratch, "labelWrapCachedLines", e) || [], f = l !== "wrap" ? 1 : Math.max(u.length, 1), c = o * s, v = i.width, d = i.height + (f - 1) * (s - 1) * o;
    yr(r.rstyle, "labelWidth", e, v), yr(r.rscratch, "labelWidth", e, v), yr(r.rstyle, "labelHeight", e, d), yr(r.rscratch, "labelHeight", e, d), yr(r.rscratch, "labelLineHeight", e, c), yr(r.rscratch, "labelActualDescent", e, i.labelActualDescent);
  }
};
Sr.getLabelText = function(t, e) {
  var r = t._private, n = e ? e + "-" : "", a = t.pstyle(n + "label").strValue, i = t.pstyle("text-transform").value, s = function(Q, se) {
    return se ? (yr(r.rscratch, Q, e, se), se) : Nt(r.rscratch, Q, e);
  };
  if (!a)
    return "";
  i == "none" || (i == "uppercase" ? a = a.toUpperCase() : i == "lowercase" && (a = a.toLowerCase()));
  var o = t.pstyle("text-wrap").value;
  if (o === "wrap") {
    var l = s("labelKey");
    if (l != null && s("labelWrapKey") === l)
      return s("labelWrapCachedText");
    for (var u = "​", f = a.split(`
`), c = t.pstyle("text-max-width").pfValue, v = t.pstyle("text-overflow-wrap").value, d = v === "anywhere", h = [], y = /[\s\u200b]+|$/g, g = 0; g < f.length; g++) {
      var p = f[g], m = this.calculateLabelDimensions(t, p), b = m.width;
      if (d) {
        var w = p.split("").join(u);
        p = w;
      }
      if (b > c) {
        var E = p.matchAll(y), T = "", x = 0, S = Gt(E), D;
        try {
          for (S.s(); !(D = S.n()).done; ) {
            var A = D.value, k = A[0], R = p.substring(x, A.index);
            x = A.index + k.length;
            var M = T.length === 0 ? R : T + R + k, I = this.calculateLabelDimensions(t, M), _ = I.width;
            _ <= c ? T += R + k : (T && h.push(T), T = R + k);
          }
        } catch ($) {
          S.e($);
        } finally {
          S.f();
        }
        T.match(/^[\s\u200b]+$/) || h.push(T);
      } else
        h.push(p);
    }
    s("labelWrapCachedLines", h), a = s("labelWrapCachedText", h.join(`
`)), s("labelWrapKey", l);
  } else if (o === "ellipsis") {
    var N = t.pstyle("text-max-width").pfValue, L = "", F = "…", H = !1;
    if (this.calculateLabelDimensions(t, a).width < N)
      return a;
    for (var V = 0; V < a.length; V++) {
      var O = this.calculateLabelDimensions(t, L + a[V] + F).width;
      if (O > N)
        break;
      L += a[V], V === a.length - 1 && (H = !0);
    }
    return H || (L += F), L;
  }
  return a;
};
Sr.getLabelJustification = function(t) {
  var e = t.pstyle("text-justification").strValue, r = t.pstyle("text-halign").strValue;
  return e === "auto" ? t.isNode() ? ix(r) : "center" : e;
};
Sr.calculateLabelDimensions = function(t, e) {
  var r = this, n = r.cy.window(), a = n.document, i = 0, s = t.pstyle("font-style").strValue, o = t.pstyle("font-size").pfValue, l = t.pstyle("font-family").strValue, u = t.pstyle("font-weight").strValue, f = t.pstyle("text-metrics").strValue || "font", c = this.labelCalcCanvas, v = this.labelCalcCanvasContext;
  if (!c) {
    c = this.labelCalcCanvas = a.createElement("canvas"), v = this.labelCalcCanvasContext = c.getContext("2d");
    var d = c.style;
    d.position = "absolute", d.left = "-9999px", d.top = "-9999px", d.zIndex = "-1", d.visibility = "hidden", d.pointerEvents = "none";
  }
  v.font = "".concat(s, " ").concat(u, " ").concat(o, "px ").concat(l);
  for (var h = 0, y = 0, g = e.split(`
`), p = g.length, m = 0, b = 0, w = 0; w < p; w++) {
    var E = g[w], T = v.measureText(E), x = Math.ceil(T.width), S = o;
    f === "glyph" && (w === 0 && (b = T.actualBoundingBoxAscent), w === p - 1 && (m = T.actualBoundingBoxDescent)), h = Math.max(x, h), y += S;
  }
  return f === "glyph" && (y -= o - b - m), h += i, y += i, {
    width: h,
    height: y,
    labelActualAscent: b,
    labelActualDescent: m
  };
};
Sr.calculateLabelAngle = function(t, e) {
  var r = t._private, n = r.rscratch, a = t.isEdge(), i = e ? e + "-" : "", s = t.pstyle(i + "text-rotation"), o = s.strValue;
  return o === "none" ? 0 : a && o === "autorotate" ? n.labelAutoAngle : o === "autorotate" ? 0 : s.pfValue;
};
Sr.calculateLabelAngles = function(t) {
  var e = this, r = t.isEdge(), n = t._private, a = n.rscratch;
  a.labelAngle = e.calculateLabelAngle(t), r && (a.sourceLabelAngle = e.calculateLabelAngle(t, "source"), a.targetLabelAngle = e.calculateLabelAngle(t, "target"));
};
var ap = {}, ud = 28, fd = !1;
ap.getNodeShape = function(t) {
  var e = this, r = t.pstyle("shape").value;
  if (r === "cutrectangle" && (t.width() < ud || t.height() < ud))
    return fd || ($e("The `cutrectangle` node shape can not be used at small sizes so `rectangle` is used instead"), fd = !0), "rectangle";
  if (t.isParent())
    return r === "rectangle" || r === "roundrectangle" || r === "round-rectangle" || r === "cutrectangle" || r === "cut-rectangle" || r === "barrel" ? r : "rectangle";
  if (r === "polygon") {
    var n = t.pstyle("shape-polygon-points").value;
    return e.nodeShapes.makePolygon(n).name;
  }
  return r;
};
var so = {};
so.registerCalculationListeners = function() {
  var t = this.cy, e = t.collection(), r = this, n = function(s) {
    var o = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0;
    if (e.merge(s), o)
      for (var l = 0; l < s.length; l++) {
        var u = s[l], f = u._private, c = f.rstyle;
        c.clean = !1, c.cleanConnected = !1;
      }
  };
  r.binder(t).on("bounds.* dirty.*", function(s) {
    var o = s.target;
    n(o);
  }).on("style.* background.*", function(s) {
    var o = s.target;
    n(o, !1);
  });
  var a = function(s) {
    if (s) {
      var o = r.onUpdateEleCalcsFns;
      e.cleanStyle();
      for (var l = 0; l < e.length; l++) {
        var u = e[l], f = u._private.rstyle;
        u.isNode() && !f.cleanConnected && (n(u.connectedEdges()), f.cleanConnected = !0);
      }
      if (o)
        for (var c = 0; c < o.length; c++) {
          var v = o[c];
          v(s, e);
        }
      r.recalculateRenderedStyle(e), e = t.collection();
    }
  };
  r.flushRenderedStyleQueue = function() {
    a(!0);
  }, r.beforeRender(a, r.beforeRenderPriorities.eleCalcs);
};
so.onUpdateEleCalcs = function(t) {
  var e = this.onUpdateEleCalcsFns = this.onUpdateEleCalcsFns || [];
  e.push(t);
};
so.recalculateRenderedStyle = function(t, e) {
  var r = function(w) {
    return w._private.rstyle.cleanConnected;
  };
  if (t.length !== 0) {
    var n = [], a = [];
    if (!this.destroyed) {
      e === void 0 && (e = !0);
      for (var i = 0; i < t.length; i++) {
        var s = t[i], o = s._private, l = o.rstyle;
        s.isEdge() && (!r(s.source()) || !r(s.target())) && (l.clean = !1), s.isEdge() && s.isBundledBezier() && s.parallelEdges().some(function(b) {
          return !b._private.rstyle.clean && b.isBundledBezier();
        }) && (l.clean = !1), !(e && l.clean || s.removed()) && s.pstyle("display").value !== "none" && (o.group === "nodes" ? a.push(s) : n.push(s), l.clean = !0);
      }
      for (var u = 0; u < a.length; u++) {
        var f = a[u], c = f._private, v = c.rstyle, d = f.position();
        this.recalculateNodeLabelProjection(f), v.nodeX = d.x, v.nodeY = d.y, v.nodeW = f.pstyle("width").pfValue, v.nodeH = f.pstyle("height").pfValue;
      }
      this.recalculateEdgeProjections(n);
      for (var h = 0; h < n.length; h++) {
        var y = n[h], g = y._private, p = g.rstyle, m = g.rscratch;
        p.srcX = m.arrowStartX, p.srcY = m.arrowStartY, p.tgtX = m.arrowEndX, p.tgtY = m.arrowEndY, p.midX = m.midX, p.midY = m.midY, p.labelAngle = m.labelAngle, p.sourceLabelAngle = m.sourceLabelAngle, p.targetLabelAngle = m.targetLabelAngle;
      }
    }
  }
};
var oo = {};
oo.updateCachedGrabbedEles = function() {
  var t = this.cachedZSortedEles;
  if (t) {
    t.drag = [], t.nondrag = [];
    for (var e = [], r = 0; r < t.length; r++) {
      var n = t[r], a = n._private.rscratch;
      n.grabbed() && !n.isParent() ? e.push(n) : a.inDragLayer ? t.drag.push(n) : t.nondrag.push(n);
    }
    for (var r = 0; r < e.length; r++) {
      var n = e[r];
      t.drag.push(n);
    }
  }
};
oo.invalidateCachedZSortedEles = function() {
  this.cachedZSortedEles = null;
};
oo.getCachedZSortedEles = function(t) {
  if (t || !this.cachedZSortedEles) {
    var e = this.cy.mutableElements().toArray();
    e.sort(zg), e.interactive = e.filter(function(r) {
      return r.interactive();
    }), this.cachedZSortedEles = e, this.updateCachedGrabbedEles();
  } else
    e = this.cachedZSortedEles;
  return e;
};
var ip = {};
[_n, Ds, Lt, Ti, hf, Sr, ap, so, oo].forEach(function(t) {
  Ae(ip, t);
});
var sp = {};
sp.getCachedImage = function(t, e, r) {
  var n = this, a = n.imageCache = n.imageCache || {}, i = a[t];
  if (i)
    return i.image.complete || i.image.addEventListener("load", r), i.image;
  i = a[t] = a[t] || {};
  var s = i.image = new Image();
  s.addEventListener("load", r), s.addEventListener("error", function() {
    s.error = !0;
  });
  var o = "data:", l = t.substring(0, o.length).toLowerCase() === o;
  return l || (e = e === "null" ? null : e, s.crossOrigin = e), s.src = t, s;
};
var op = function(e, r) {
  var n = e[0];
  !n || n._private.grabbed === r || (n._private.grabbed = r, e.updateStyle(!1));
}, bE = function(e) {
  op(e, !0);
}, wE = function(e) {
  op(e, !1);
}, wa = {};
wa.registerBinding = function(t, e, r, n) {
  var a = Array.prototype.slice.apply(arguments, [1]);
  if (Array.isArray(t)) {
    for (var i = [], s = 0; s < t.length; s++) {
      var o = t[s];
      if (o !== void 0) {
        var l = this.binder(o);
        i.push(l.on.apply(l, a));
      }
    }
    return i;
  }
  var l = this.binder(t);
  return l.on.apply(l, a);
};
wa.binder = function(t) {
  var e = this, r = e.cy.window(), n = t === r || t === r.document || t === r.document.body || k0(t);
  if (e.supportsPassiveEvents == null) {
    var a = !1;
    try {
      var i = Object.defineProperty({}, "passive", {
        get: function() {
          return a = !0, !0;
        }
      });
      r.addEventListener("test", null, i);
    } catch {
    }
    e.supportsPassiveEvents = a;
  }
  var s = function(l, u, f) {
    var c = Array.prototype.slice.call(arguments);
    return n && e.supportsPassiveEvents && (c[2] = {
      capture: f ?? !1,
      passive: !1,
      once: !1
    }), e.bindings.push({
      target: t,
      args: c
    }), (t.addEventListener || t.on).apply(t, c), this;
  };
  return {
    on: s,
    addEventListener: s,
    addListener: s,
    bind: s
  };
};
wa.nodeIsDraggable = function(t) {
  return t && t.isNode() && !t.locked() && t.grabbable();
};
wa.nodeIsGrabbable = function(t) {
  return this.nodeIsDraggable(t) && t.interactive();
};
wa.load = function() {
  var t = this, e = t.cy.window(), r = function(P) {
    return P.selected();
  }, n = function(P) {
    var q = P.getRootNode();
    if (q && q.nodeType === 11 && q.host !== void 0)
      return q;
  }, a = function(P, q, G, re) {
    P == null && (P = t.cy);
    for (var ee = 0; ee < q.length; ee++) {
      var ye = q[ee];
      P.emit({
        originalEvent: G,
        type: ye,
        position: re
      });
    }
  }, i = function(P) {
    return P.shiftKey || P.metaKey || P.ctrlKey;
  }, s = function(P, q) {
    var G = !0;
    if (t.cy.hasCompoundNodes() && P && P.pannable())
      for (var re = 0; q && re < q.length; re++) {
        var P = q[re];
        if (P.isNode() && P.isParent() && !P.pannable()) {
          G = !1;
          break;
        }
      }
    else
      G = !0;
    return G;
  }, o = function(P) {
    P[0]._private.rscratch.inDragLayer = !0;
  }, l = function(P) {
    P[0]._private.rscratch.inDragLayer = !1;
  }, u = function(P) {
    P[0]._private.rscratch.isGrabTarget = !0;
  }, f = function(P) {
    P[0]._private.rscratch.isGrabTarget = !1;
  }, c = function(P, q) {
    var G = q.addToList, re = G.has(P);
    !re && P.grabbable() && !P.locked() && (G.merge(P), bE(P));
  }, v = function(P, q) {
    if (P.cy().hasCompoundNodes() && !(q.inDragLayer == null && q.addToList == null)) {
      var G = P.descendants();
      q.inDragLayer && (G.forEach(o), G.connectedEdges().forEach(o)), q.addToList && c(G, q);
    }
  }, d = function(P, q) {
    q = q || {};
    var G = P.cy().hasCompoundNodes();
    q.inDragLayer && (P.forEach(o), P.neighborhood().stdFilter(function(re) {
      return !G || re.isEdge();
    }).forEach(o)), q.addToList && P.forEach(function(re) {
      c(re, q);
    }), v(P, q), g(P, {
      inDragLayer: q.inDragLayer
    }), t.updateCachedGrabbedEles();
  }, h = d, y = function(P) {
    P && (t.getCachedZSortedEles().forEach(function(q) {
      wE(q), l(q), f(q);
    }), t.updateCachedGrabbedEles());
  }, g = function(P, q) {
    if (!(q.inDragLayer == null && q.addToList == null) && P.cy().hasCompoundNodes()) {
      var G = P.ancestors().orphans();
      if (!G.same(P)) {
        var re = G.descendants().spawnSelf().merge(G).unmerge(P).unmerge(P.descendants()), ee = re.connectedEdges();
        q.inDragLayer && (ee.forEach(o), re.forEach(o)), q.addToList && re.forEach(function(ye) {
          c(ye, q);
        });
      }
    }
  }, p = function() {
    document.activeElement != null && document.activeElement.blur != null && document.activeElement.blur();
  }, m = typeof MutationObserver < "u", b = typeof ResizeObserver < "u";
  m ? (t.removeObserver = new MutationObserver(function(J) {
    for (var P = 0; P < J.length; P++) {
      var q = J[P], G = q.removedNodes;
      if (G)
        for (var re = 0; re < G.length; re++) {
          var ee = G[re];
          if (ee === t.container) {
            t.destroy();
            break;
          }
        }
    }
  }), t.container.parentNode && t.removeObserver.observe(t.container.parentNode, {
    childList: !0
  })) : t.registerBinding(t.container, "DOMNodeRemoved", function(J) {
    t.destroy();
  });
  var w = wi(function() {
    t.cy.resize();
  }, 100);
  m && (t.styleObserver = new MutationObserver(w), t.styleObserver.observe(t.container, {
    attributes: !0
  })), t.registerBinding(e, "resize", w), b && (t.resizeObserver = new ResizeObserver(w), t.resizeObserver.observe(t.container));
  var E = function(P, q) {
    for (; P != null; )
      q(P), P = P.parentNode;
  }, T = function() {
    t.invalidateContainerClientCoordsCache();
  };
  E(t.container, function(J) {
    t.registerBinding(J, "transitionend", T), t.registerBinding(J, "animationend", T), t.registerBinding(J, "scroll", T);
  }), t.registerBinding(t.container, "contextmenu", function(J) {
    J.preventDefault();
  });
  var x = function() {
    return t.selection[4] !== 0;
  }, S = function(P) {
    for (var q = t.findContainerClientCoords(), G = q[0], re = q[1], ee = q[2], ye = q[3], fe = P.touches ? P.touches : [P], ge = !1, be = 0; be < fe.length; be++) {
      var De = fe[be];
      if (G <= De.clientX && De.clientX <= G + ee && re <= De.clientY && De.clientY <= re + ye) {
        ge = !0;
        break;
      }
    }
    if (!ge)
      return !1;
    for (var Be = t.container, Me = P.target, xe = Me.parentNode, Ie = !1; xe; ) {
      if (xe === Be) {
        Ie = !0;
        break;
      }
      xe = xe.parentNode;
    }
    return !!Ie;
  };
  t.registerBinding(t.container, "mousedown", function(P) {
    if (S(P) && !(t.hoverData.which === 1 && P.which !== 1)) {
      P.preventDefault(), p(), t.hoverData.capture = !0, t.hoverData.which = P.which;
      var q = t.cy, G = [P.clientX, P.clientY], re = t.projectIntoViewport(G[0], G[1]), ee = t.selection, ye = t.findNearestElements(re[0], re[1], !0, !1), fe = ye[0], ge = t.dragData.possibleDragElements;
      t.hoverData.mdownPos = re, t.hoverData.mdownGPos = G;
      var be = function(Oe) {
        return {
          originalEvent: P,
          type: Oe,
          position: {
            x: re[0],
            y: re[1]
          }
        };
      }, De = function() {
        t.hoverData.tapholdCancelled = !1, clearTimeout(t.hoverData.tapholdTimeout), t.hoverData.tapholdTimeout = setTimeout(function() {
          if (!t.hoverData.tapholdCancelled) {
            var Oe = t.hoverData.down;
            Oe ? Oe.emit(be("taphold")) : q.emit(be("taphold"));
          }
        }, t.tapholdDuration);
      };
      if (P.which == 3) {
        t.hoverData.cxtStarted = !0;
        var Be = {
          originalEvent: P,
          type: "cxttapstart",
          position: {
            x: re[0],
            y: re[1]
          }
        };
        fe ? (fe.activate(), fe.emit(Be), t.hoverData.down = fe) : q.emit(Be), t.hoverData.downTime = (/* @__PURE__ */ new Date()).getTime(), t.hoverData.cxtDragged = !1;
      } else if (P.which == 1) {
        fe && fe.activate();
        {
          if (fe != null && t.nodeIsGrabbable(fe)) {
            var Me = function(Oe) {
              Oe.emit(be("grab"));
            };
            if (u(fe), !fe.selected())
              ge = t.dragData.possibleDragElements = q.collection(), h(fe, {
                addToList: ge
              }), fe.emit(be("grabon")).emit(be("grab"));
            else {
              ge = t.dragData.possibleDragElements = q.collection();
              var xe = q.$(function(Ie) {
                return Ie.isNode() && Ie.selected() && t.nodeIsGrabbable(Ie);
              });
              d(xe, {
                addToList: ge
              }), fe.emit(be("grabon")), xe.forEach(Me);
            }
            t.redrawHint("eles", !0), t.redrawHint("drag", !0);
          }
          t.hoverData.down = fe, t.hoverData.downs = ye, t.hoverData.downTime = (/* @__PURE__ */ new Date()).getTime();
        }
        a(fe, ["mousedown", "tapstart", "vmousedown"], P, {
          x: re[0],
          y: re[1]
        }), fe == null ? (ee[4] = 1, t.data.bgActivePosistion = {
          x: re[0],
          y: re[1]
        }, t.redrawHint("select", !0), t.redraw()) : fe.pannable() && (ee[4] = 1), De();
      }
      ee[0] = ee[2] = re[0], ee[1] = ee[3] = re[1];
    }
  }, !1);
  var D = n(t.container);
  t.registerBinding([e, D], "mousemove", function(P) {
    var q = t.hoverData.capture;
    if (!(!q && !S(P))) {
      var G = !1, re = t.cy, ee = re.zoom(), ye = [P.clientX, P.clientY], fe = t.projectIntoViewport(ye[0], ye[1]), ge = t.hoverData.mdownPos, be = t.hoverData.mdownGPos, De = t.selection, Be = null;
      !t.hoverData.draggingEles && !t.hoverData.dragging && !t.hoverData.selecting && (Be = t.findNearestElement(fe[0], fe[1], !0, !1));
      var Me = t.hoverData.last, xe = t.hoverData.down, Ie = [fe[0] - De[2], fe[1] - De[3]], Oe = t.dragData.possibleDragElements, gt;
      if (be) {
        var rt = ye[0] - be[0], tr = rt * rt, nt = ye[1] - be[1], st = nt * nt, ft = tr + st;
        t.hoverData.isOverThresholdDrag = gt = ft >= t.desktopTapThreshold2;
      }
      var wt = i(P);
      gt && (t.hoverData.tapholdCancelled = !0);
      var cr = function() {
        var Zt = t.hoverData.dragDelta = t.hoverData.dragDelta || [];
        Zt.length === 0 ? (Zt.push(Ie[0]), Zt.push(Ie[1])) : (Zt[0] += Ie[0], Zt[1] += Ie[1]);
      };
      G = !0, a(Be, ["mousemove", "vmousemove", "tapdrag"], P, {
        x: fe[0],
        y: fe[1]
      });
      var Je = function(Zt) {
        return {
          originalEvent: P,
          type: Zt,
          position: {
            x: fe[0],
            y: fe[1]
          }
        };
      }, Dr = function() {
        t.data.bgActivePosistion = void 0, t.hoverData.selecting || re.emit(Je("boxstart")), De[4] = 1, t.hoverData.selecting = !0, t.redrawHint("select", !0), t.redraw();
      };
      if (t.hoverData.which === 3) {
        if (gt) {
          var rr = Je("cxtdrag");
          xe ? xe.emit(rr) : re.emit(rr), t.hoverData.cxtDragged = !0, (!t.hoverData.cxtOver || Be !== t.hoverData.cxtOver) && (t.hoverData.cxtOver && t.hoverData.cxtOver.emit(Je("cxtdragout")), t.hoverData.cxtOver = Be, Be && Be.emit(Je("cxtdragover")));
        }
      } else if (t.hoverData.dragging) {
        if (G = !0, re.panningEnabled() && re.userPanningEnabled()) {
          var Wr;
          if (t.hoverData.justStartedPan) {
            var Di = t.hoverData.mdownPos;
            Wr = {
              x: (fe[0] - Di[0]) * ee,
              y: (fe[1] - Di[1]) * ee
            }, t.hoverData.justStartedPan = !1;
          } else
            Wr = {
              x: Ie[0] * ee,
              y: Ie[1] * ee
            };
          re.panBy(Wr), re.emit(Je("dragpan")), t.hoverData.dragged = !0;
        }
        fe = t.projectIntoViewport(P.clientX, P.clientY);
      } else if (De[4] == 1 && (xe == null || xe.pannable())) {
        if (gt) {
          if (!t.hoverData.dragging && re.boxSelectionEnabled() && (wt || !re.panningEnabled() || !re.userPanningEnabled()))
            Dr();
          else if (!t.hoverData.selecting && re.panningEnabled() && re.userPanningEnabled()) {
            var gn = s(xe, t.hoverData.downs);
            gn && (t.hoverData.dragging = !0, t.hoverData.justStartedPan = !0, De[4] = 0, t.data.bgActivePosistion = Zn(ge), t.redrawHint("select", !0), t.redraw());
          }
          xe && xe.pannable() && xe.active() && xe.unactivate();
        }
      } else {
        if (xe && xe.pannable() && xe.active() && xe.unactivate(), (!xe || !xe.grabbed()) && Be != Me && (Me && a(Me, ["mouseout", "tapdragout"], P, {
          x: fe[0],
          y: fe[1]
        }), Be && a(Be, ["mouseover", "tapdragover"], P, {
          x: fe[0],
          y: fe[1]
        }), t.hoverData.last = Be), xe)
          if (gt) {
            if (re.boxSelectionEnabled() && wt)
              xe && xe.grabbed() && (y(Oe), xe.emit(Je("freeon")), Oe.emit(Je("free")), t.dragData.didDrag && (xe.emit(Je("dragfreeon")), Oe.emit(Je("dragfree")))), Dr();
            else if (xe && xe.grabbed() && t.nodeIsDraggable(xe)) {
              var qt = !t.dragData.didDrag;
              qt && t.redrawHint("eles", !0), t.dragData.didDrag = !0, t.hoverData.draggingEles || d(Oe, {
                inDragLayer: !0
              });
              var Dt = {
                x: 0,
                y: 0
              };
              if (pe(Ie[0]) && pe(Ie[1]) && (Dt.x += Ie[0], Dt.y += Ie[1], qt)) {
                var $t = t.hoverData.dragDelta;
                $t && pe($t[0]) && pe($t[1]) && (Dt.x += $t[0], Dt.y += $t[1]);
              }
              t.hoverData.draggingEles = !0, Oe.silentShift(Dt).emit(Je("position")).emit(Je("drag")), t.redrawHint("drag", !0), t.redraw();
            }
          } else
            cr();
        G = !0;
      }
      if (De[2] = fe[0], De[3] = fe[1], G)
        return P.stopPropagation && P.stopPropagation(), P.preventDefault && P.preventDefault(), !1;
    }
  }, !1);
  var A, k, R;
  t.registerBinding(e, "mouseup", function(P) {
    if (!(t.hoverData.which === 1 && P.which !== 1 && t.hoverData.capture)) {
      var q = t.hoverData.capture;
      if (q) {
        t.hoverData.capture = !1;
        var G = t.cy, re = t.projectIntoViewport(P.clientX, P.clientY), ee = t.selection, ye = t.findNearestElement(re[0], re[1], !0, !1), fe = t.dragData.possibleDragElements, ge = t.hoverData.down, be = i(P);
        t.data.bgActivePosistion && (t.redrawHint("select", !0), t.redraw()), t.hoverData.tapholdCancelled = !0, t.data.bgActivePosistion = void 0, ge && ge.unactivate();
        var De = function(rt) {
          return {
            originalEvent: P,
            type: rt,
            position: {
              x: re[0],
              y: re[1]
            }
          };
        };
        if (t.hoverData.which === 3) {
          var Be = De("cxttapend");
          if (ge ? ge.emit(Be) : G.emit(Be), !t.hoverData.cxtDragged) {
            var Me = De("cxttap");
            ge ? ge.emit(Me) : G.emit(Me);
          }
          t.hoverData.cxtDragged = !1, t.hoverData.which = null;
        } else if (t.hoverData.which === 1) {
          if (a(ye, ["mouseup", "tapend", "vmouseup"], P, {
            x: re[0],
            y: re[1]
          }), !t.dragData.didDrag && // didn't move a node around
          !t.hoverData.dragged && // didn't pan
          !t.hoverData.selecting && // not box selection
          !t.hoverData.isOverThresholdDrag && (a(ge, ["click", "tap", "vclick"], P, {
            x: re[0],
            y: re[1]
          }), k = !1, P.timeStamp - R <= G.multiClickDebounceTime() ? (A && clearTimeout(A), k = !0, R = null, a(ge, ["dblclick", "dbltap", "vdblclick"], P, {
            x: re[0],
            y: re[1]
          })) : (A = setTimeout(function() {
            k || a(ge, ["oneclick", "onetap", "voneclick"], P, {
              x: re[0],
              y: re[1]
            });
          }, G.multiClickDebounceTime()), R = P.timeStamp)), ge == null && !t.dragData.didDrag && !t.hoverData.selecting && !t.hoverData.dragged && !i(P) && (G.$(r).unselect(["tapunselect"]), fe.length > 0 && t.redrawHint("eles", !0), t.dragData.possibleDragElements = fe = G.collection()), ye == ge && !t.dragData.didDrag && !t.hoverData.selecting && ye != null && ye._private.selectable && (t.hoverData.dragging || (G.selectionType() === "additive" || be ? ye.selected() ? ye.unselect(["tapunselect"]) : ye.select(["tapselect"]) : be || (G.$(r).unmerge(ye).unselect(["tapunselect"]), ye.select(["tapselect"]))), t.redrawHint("eles", !0)), t.hoverData.selecting) {
            var xe = G.collection(t.getAllInBox(ee[0], ee[1], ee[2], ee[3]));
            t.redrawHint("select", !0), xe.length > 0 && t.redrawHint("eles", !0), G.emit(De("boxend"));
            var Ie = function(rt) {
              return rt.selectable() && !rt.selected();
            };
            G.selectionType() === "additive" || be || G.$(r).unmerge(xe).unselect(), xe.emit(De("box")).stdFilter(Ie).select().emit(De("boxselect")), t.redraw();
          }
          if (t.hoverData.dragging && (t.hoverData.dragging = !1, t.redrawHint("select", !0), t.redrawHint("eles", !0), t.redraw()), !ee[4]) {
            t.redrawHint("drag", !0), t.redrawHint("eles", !0);
            var Oe = ge && ge.grabbed();
            y(fe), Oe && (ge.emit(De("freeon")), fe.emit(De("free")), t.dragData.didDrag && (ge.emit(De("dragfreeon")), fe.emit(De("dragfree"))));
          }
        }
        ee[4] = 0, t.hoverData.down = null, t.hoverData.cxtStarted = !1, t.hoverData.draggingEles = !1, t.hoverData.selecting = !1, t.hoverData.isOverThresholdDrag = !1, t.dragData.didDrag = !1, t.hoverData.dragged = !1, t.hoverData.dragDelta = [], t.hoverData.mdownPos = null, t.hoverData.mdownGPos = null, t.hoverData.which = null;
      }
    }
  }, !1);
  var M = [], I = 4, _, N = 1e5, L = function(P, q) {
    for (var G = 0; G < P.length; G++)
      if (P[G] % q !== 0)
        return !1;
    return !0;
  }, F = function(P) {
    for (var q = Math.abs(P[0]), G = 1; G < P.length; G++)
      if (Math.abs(P[G]) !== q)
        return !1;
    return !0;
  }, H = function(P) {
    var q = !1, G = P.deltaY;
    if (G == null && (P.wheelDeltaY != null ? G = P.wheelDeltaY / 4 : P.wheelDelta != null && (G = P.wheelDelta / 4)), G !== 0) {
      if (_ == null)
        if (M.length >= I) {
          var re = M;
          if (_ = L(re, 5), !_) {
            var ee = Math.abs(re[0]);
            _ = F(re) && ee > 5;
          }
          if (_)
            for (var ye = 0; ye < re.length; ye++)
              N = Math.min(Math.abs(re[ye]), N);
        } else
          M.push(G), q = !0;
      else _ && (N = Math.min(Math.abs(G), N));
      if (!t.scrollingPage) {
        var fe = t.cy, ge = fe.zoom(), be = fe.pan(), De = t.projectIntoViewport(P.clientX, P.clientY), Be = [De[0] * ge + be.x, De[1] * ge + be.y];
        if (t.hoverData.draggingEles || t.hoverData.dragging || t.hoverData.cxtStarted || x()) {
          P.preventDefault();
          return;
        }
        if (fe.panningEnabled() && fe.userPanningEnabled() && fe.zoomingEnabled() && fe.userZoomingEnabled()) {
          P.preventDefault(), t.data.wheelZooming = !0, clearTimeout(t.data.wheelTimeout), t.data.wheelTimeout = setTimeout(function() {
            t.data.wheelZooming = !1, t.redrawHint("eles", !0), t.redraw();
          }, 150);
          var Me;
          q && Math.abs(G) > 5 && (G = Ju(G) * 5), Me = G / -250, _ && (Me /= N, Me *= 3), Me = Me * t.wheelSensitivity;
          var xe = P.deltaMode === 1;
          xe && (Me *= 33);
          var Ie = fe.zoom() * Math.pow(10, Me);
          P.type === "gesturechange" && (Ie = t.gestureStartZoom * P.scale), fe.zoom({
            level: Ie,
            renderedPosition: {
              x: Be[0],
              y: Be[1]
            }
          }), fe.emit({
            type: P.type === "gesturechange" ? "pinchzoom" : "scrollzoom",
            originalEvent: P,
            position: {
              x: De[0],
              y: De[1]
            }
          });
        }
      }
    }
  };
  t.registerBinding(t.container, "wheel", H, !0), t.registerBinding(e, "scroll", function(P) {
    t.scrollingPage = !0, clearTimeout(t.scrollingPageTimeout), t.scrollingPageTimeout = setTimeout(function() {
      t.scrollingPage = !1;
    }, 250);
  }, !0), t.registerBinding(t.container, "gesturestart", function(P) {
    t.gestureStartZoom = t.cy.zoom(), t.hasTouchStarted || P.preventDefault();
  }, !0), t.registerBinding(t.container, "gesturechange", function(J) {
    t.hasTouchStarted || H(J);
  }, !0), t.registerBinding(t.container, "mouseout", function(P) {
    var q = t.projectIntoViewport(P.clientX, P.clientY);
    t.cy.emit({
      originalEvent: P,
      type: "mouseout",
      position: {
        x: q[0],
        y: q[1]
      }
    });
  }, !1), t.registerBinding(t.container, "mouseover", function(P) {
    var q = t.projectIntoViewport(P.clientX, P.clientY);
    t.cy.emit({
      originalEvent: P,
      type: "mouseover",
      position: {
        x: q[0],
        y: q[1]
      }
    });
  }, !1);
  var V, O, $, Q, se, ae, le, ce, he, ie, U, X, C, B = function(P, q, G, re) {
    return Math.sqrt((G - P) * (G - P) + (re - q) * (re - q));
  }, z = function(P, q, G, re) {
    return (G - P) * (G - P) + (re - q) * (re - q);
  }, W;
  t.registerBinding(t.container, "touchstart", W = function(P) {
    if (t.hasTouchStarted = !0, !!S(P)) {
      p(), t.touchData.capture = !0, t.data.bgActivePosistion = void 0;
      var q = t.cy, G = t.touchData.now, re = t.touchData.earlier;
      if (P.touches[0]) {
        var ee = t.projectIntoViewport(P.touches[0].clientX, P.touches[0].clientY);
        G[0] = ee[0], G[1] = ee[1];
      }
      if (P.touches[1]) {
        var ee = t.projectIntoViewport(P.touches[1].clientX, P.touches[1].clientY);
        G[2] = ee[0], G[3] = ee[1];
      }
      if (P.touches[2]) {
        var ee = t.projectIntoViewport(P.touches[2].clientX, P.touches[2].clientY);
        G[4] = ee[0], G[5] = ee[1];
      }
      var ye = function(wt) {
        return {
          originalEvent: P,
          type: wt,
          position: {
            x: G[0],
            y: G[1]
          }
        };
      };
      if (P.touches[1]) {
        t.touchData.singleTouchMoved = !0, y(t.dragData.touchDragEles);
        var fe = t.findContainerClientCoords();
        he = fe[0], ie = fe[1], U = fe[2], X = fe[3], V = P.touches[0].clientX - he, O = P.touches[0].clientY - ie, $ = P.touches[1].clientX - he, Q = P.touches[1].clientY - ie, C = 0 <= V && V <= U && 0 <= $ && $ <= U && 0 <= O && O <= X && 0 <= Q && Q <= X;
        var ge = q.pan(), be = q.zoom();
        se = B(V, O, $, Q), ae = z(V, O, $, Q), le = [(V + $) / 2, (O + Q) / 2], ce = [(le[0] - ge.x) / be, (le[1] - ge.y) / be];
        var De = 200, Be = De * De;
        if (ae < Be && !P.touches[2]) {
          var Me = t.findNearestElement(G[0], G[1], !0, !0), xe = t.findNearestElement(G[2], G[3], !0, !0);
          Me && Me.isNode() ? (Me.activate().emit(ye("cxttapstart")), t.touchData.start = Me) : xe && xe.isNode() ? (xe.activate().emit(ye("cxttapstart")), t.touchData.start = xe) : q.emit(ye("cxttapstart")), t.touchData.start && (t.touchData.start._private.grabbed = !1), t.touchData.cxt = !0, t.touchData.cxtDragged = !1, t.data.bgActivePosistion = void 0, t.redraw();
          return;
        }
      }
      if (P.touches[2])
        q.boxSelectionEnabled() && P.preventDefault();
      else if (!P.touches[1]) {
        if (P.touches[0]) {
          var Ie = t.findNearestElements(G[0], G[1], !0, !0), Oe = Ie[0];
          if (Oe != null && (Oe.activate(), t.touchData.start = Oe, t.touchData.starts = Ie, t.nodeIsGrabbable(Oe))) {
            var gt = t.dragData.touchDragEles = q.collection(), rt = null;
            t.redrawHint("eles", !0), t.redrawHint("drag", !0), Oe.selected() ? (rt = q.$(function(ft) {
              return ft.selected() && t.nodeIsGrabbable(ft);
            }), d(rt, {
              addToList: gt
            })) : h(Oe, {
              addToList: gt
            }), u(Oe), Oe.emit(ye("grabon")), rt ? rt.forEach(function(ft) {
              ft.emit(ye("grab"));
            }) : Oe.emit(ye("grab"));
          }
          a(Oe, ["touchstart", "tapstart", "vmousedown"], P, {
            x: G[0],
            y: G[1]
          }), Oe == null && (t.data.bgActivePosistion = {
            x: ee[0],
            y: ee[1]
          }, t.redrawHint("select", !0), t.redraw()), t.touchData.singleTouchMoved = !1, t.touchData.singleTouchStartTime = +/* @__PURE__ */ new Date(), clearTimeout(t.touchData.tapholdTimeout), t.touchData.tapholdTimeout = setTimeout(function() {
            t.touchData.singleTouchMoved === !1 && !t.pinching && !t.touchData.selecting && a(t.touchData.start, ["taphold"], P, {
              x: G[0],
              y: G[1]
            });
          }, t.tapholdDuration);
        }
      }
      if (P.touches.length >= 1) {
        for (var tr = t.touchData.startPosition = [null, null, null, null, null, null], nt = 0; nt < G.length; nt++)
          tr[nt] = re[nt] = G[nt];
        var st = P.touches[0];
        t.touchData.startGPosition = [st.clientX, st.clientY];
      }
    }
  }, !1);
  var j;
  t.registerBinding(e, "touchmove", j = function(P) {
    var q = t.touchData.capture;
    if (!(!q && !S(P))) {
      var G = t.selection, re = t.cy, ee = t.touchData.now, ye = t.touchData.earlier, fe = re.zoom();
      if (P.touches[0]) {
        var ge = t.projectIntoViewport(P.touches[0].clientX, P.touches[0].clientY);
        ee[0] = ge[0], ee[1] = ge[1];
      }
      if (P.touches[1]) {
        var ge = t.projectIntoViewport(P.touches[1].clientX, P.touches[1].clientY);
        ee[2] = ge[0], ee[3] = ge[1];
      }
      if (P.touches[2]) {
        var ge = t.projectIntoViewport(P.touches[2].clientX, P.touches[2].clientY);
        ee[4] = ge[0], ee[5] = ge[1];
      }
      var be = function(Fp) {
        return {
          originalEvent: P,
          type: Fp,
          position: {
            x: ee[0],
            y: ee[1]
          }
        };
      }, De = t.touchData.startGPosition, Be;
      if (q && P.touches[0] && De) {
        for (var Me = [], xe = 0; xe < ee.length; xe++)
          Me[xe] = ee[xe] - ye[xe];
        var Ie = P.touches[0].clientX - De[0], Oe = Ie * Ie, gt = P.touches[0].clientY - De[1], rt = gt * gt, tr = Oe + rt;
        Be = tr >= t.touchTapThreshold2;
      }
      if (q && t.touchData.cxt) {
        P.preventDefault();
        var nt = P.touches[0].clientX - he, st = P.touches[0].clientY - ie, ft = P.touches[1].clientX - he, wt = P.touches[1].clientY - ie, cr = z(nt, st, ft, wt), Je = cr / ae, Dr = 150, rr = Dr * Dr, Wr = 1.5, Di = Wr * Wr;
        if (Je >= Di || cr >= rr) {
          t.touchData.cxt = !1, t.data.bgActivePosistion = void 0, t.redrawHint("select", !0);
          var gn = be("cxttapend");
          t.touchData.start ? (t.touchData.start.unactivate().emit(gn), t.touchData.start = null) : re.emit(gn);
        }
      }
      if (q && t.touchData.cxt) {
        var gn = be("cxtdrag");
        t.data.bgActivePosistion = void 0, t.redrawHint("select", !0), t.touchData.start ? t.touchData.start.emit(gn) : re.emit(gn), t.touchData.start && (t.touchData.start._private.grabbed = !1), t.touchData.cxtDragged = !0;
        var qt = t.findNearestElement(ee[0], ee[1], !0, !0);
        (!t.touchData.cxtOver || qt !== t.touchData.cxtOver) && (t.touchData.cxtOver && t.touchData.cxtOver.emit(be("cxtdragout")), t.touchData.cxtOver = qt, qt && qt.emit(be("cxtdragover")));
      } else if (q && P.touches[2] && re.boxSelectionEnabled())
        P.preventDefault(), t.data.bgActivePosistion = void 0, this.lastThreeTouch = +/* @__PURE__ */ new Date(), t.touchData.selecting || re.emit(be("boxstart")), t.touchData.selecting = !0, t.touchData.didSelect = !0, G[4] = 1, !G || G.length === 0 || G[0] === void 0 ? (G[0] = (ee[0] + ee[2] + ee[4]) / 3, G[1] = (ee[1] + ee[3] + ee[5]) / 3, G[2] = (ee[0] + ee[2] + ee[4]) / 3 + 1, G[3] = (ee[1] + ee[3] + ee[5]) / 3 + 1) : (G[2] = (ee[0] + ee[2] + ee[4]) / 3, G[3] = (ee[1] + ee[3] + ee[5]) / 3), t.redrawHint("select", !0), t.redraw();
      else if (q && P.touches[1] && !t.touchData.didSelect && re.zoomingEnabled() && re.panningEnabled() && re.userZoomingEnabled() && re.userPanningEnabled()) {
        P.preventDefault(), t.data.bgActivePosistion = void 0, t.redrawHint("select", !0);
        var Dt = t.dragData.touchDragEles;
        if (Dt) {
          t.redrawHint("drag", !0);
          for (var $t = 0; $t < Dt.length; $t++) {
            var Ea = Dt[$t]._private;
            Ea.grabbed = !1, Ea.rscratch.inDragLayer = !1;
          }
        }
        var Zt = t.touchData.start, nt = P.touches[0].clientX - he, st = P.touches[0].clientY - ie, ft = P.touches[1].clientX - he, wt = P.touches[1].clientY - ie, yf = B(nt, st, ft, wt), kp = yf / se;
        if (C) {
          var Bp = nt - V, Rp = st - O, Mp = ft - $, Lp = wt - Q, Ip = (Bp + Mp) / 2, Op = (Rp + Lp) / 2, Ca = re.zoom(), lo = Ca * kp, Ai = re.pan(), mf = ce[0] * Ca + Ai.x, bf = ce[1] * Ca + Ai.y, Np = {
            x: -lo / Ca * (mf - Ai.x - Ip) + mf,
            y: -lo / Ca * (bf - Ai.y - Op) + bf
          };
          if (Zt && Zt.active()) {
            var Dt = t.dragData.touchDragEles;
            y(Dt), t.redrawHint("drag", !0), t.redrawHint("eles", !0), Zt.unactivate().emit(be("freeon")), Dt.emit(be("free")), t.dragData.didDrag && (Zt.emit(be("dragfreeon")), Dt.emit(be("dragfree")));
          }
          re.viewport({
            zoom: lo,
            pan: Np,
            cancelOnFailedZoom: !0
          }), re.emit(be("pinchzoom")), se = yf, V = nt, O = st, $ = ft, Q = wt, t.pinching = !0;
        }
        if (P.touches[0]) {
          var ge = t.projectIntoViewport(P.touches[0].clientX, P.touches[0].clientY);
          ee[0] = ge[0], ee[1] = ge[1];
        }
        if (P.touches[1]) {
          var ge = t.projectIntoViewport(P.touches[1].clientX, P.touches[1].clientY);
          ee[2] = ge[0], ee[3] = ge[1];
        }
        if (P.touches[2]) {
          var ge = t.projectIntoViewport(P.touches[2].clientX, P.touches[2].clientY);
          ee[4] = ge[0], ee[5] = ge[1];
        }
      } else if (P.touches[0] && !t.touchData.didSelect) {
        var nr = t.touchData.start, uo = t.touchData.last, qt;
        if (!t.hoverData.draggingEles && !t.swipePanning && (qt = t.findNearestElement(ee[0], ee[1], !0, !0)), q && nr != null && P.preventDefault(), q && nr != null && t.nodeIsDraggable(nr))
          if (Be) {
            var Dt = t.dragData.touchDragEles, wf = !t.dragData.didDrag;
            wf && d(Dt, {
              inDragLayer: !0
            }), t.dragData.didDrag = !0;
            var Ta = {
              x: 0,
              y: 0
            };
            if (pe(Me[0]) && pe(Me[1]) && (Ta.x += Me[0], Ta.y += Me[1], wf)) {
              t.redrawHint("eles", !0);
              var ar = t.touchData.dragDelta;
              ar && pe(ar[0]) && pe(ar[1]) && (Ta.x += ar[0], Ta.y += ar[1]);
            }
            t.hoverData.draggingEles = !0, Dt.silentShift(Ta).emit(be("position")).emit(be("drag")), t.redrawHint("drag", !0), t.touchData.startPosition[0] == ye[0] && t.touchData.startPosition[1] == ye[1] && t.redrawHint("eles", !0), t.redraw();
          } else {
            var ar = t.touchData.dragDelta = t.touchData.dragDelta || [];
            ar.length === 0 ? (ar.push(Me[0]), ar.push(Me[1])) : (ar[0] += Me[0], ar[1] += Me[1]);
          }
        if (a(nr || qt, ["touchmove", "tapdrag", "vmousemove"], P, {
          x: ee[0],
          y: ee[1]
        }), (!nr || !nr.grabbed()) && qt != uo && (uo && uo.emit(be("tapdragout")), qt && qt.emit(be("tapdragover"))), t.touchData.last = qt, q)
          for (var $t = 0; $t < ee.length; $t++)
            ee[$t] && t.touchData.startPosition[$t] && Be && (t.touchData.singleTouchMoved = !0);
        if (q && (nr == null || nr.pannable()) && re.panningEnabled() && re.userPanningEnabled()) {
          var _p = s(nr, t.touchData.starts);
          _p && (P.preventDefault(), t.data.bgActivePosistion || (t.data.bgActivePosistion = Zn(t.touchData.startPosition)), t.swipePanning ? (re.panBy({
            x: Me[0] * fe,
            y: Me[1] * fe
          }), re.emit(be("dragpan"))) : Be && (t.swipePanning = !0, re.panBy({
            x: Ie * fe,
            y: gt * fe
          }), re.emit(be("dragpan")), nr && (nr.unactivate(), t.redrawHint("select", !0), t.touchData.start = null)));
          var ge = t.projectIntoViewport(P.touches[0].clientX, P.touches[0].clientY);
          ee[0] = ge[0], ee[1] = ge[1];
        }
      }
      for (var xe = 0; xe < ee.length; xe++)
        ye[xe] = ee[xe];
      q && P.touches.length > 0 && !t.hoverData.draggingEles && !t.swipePanning && t.data.bgActivePosistion != null && (t.data.bgActivePosistion = void 0, t.redrawHint("select", !0), t.redraw());
    }
  }, !1);
  var Z;
  t.registerBinding(e, "touchcancel", Z = function(P) {
    var q = t.touchData.start;
    t.touchData.capture = !1, q && q.unactivate();
  });
  var ne, te, Y, K;
  if (t.registerBinding(e, "touchend", ne = function(P) {
    var q = t.touchData.start, G = t.touchData.capture;
    if (G)
      P.touches.length === 0 && (t.touchData.capture = !1), P.preventDefault();
    else
      return;
    var re = t.selection;
    t.swipePanning = !1, t.hoverData.draggingEles = !1;
    var ee = t.cy, ye = ee.zoom(), fe = t.touchData.now, ge = t.touchData.earlier;
    if (P.touches[0]) {
      var be = t.projectIntoViewport(P.touches[0].clientX, P.touches[0].clientY);
      fe[0] = be[0], fe[1] = be[1];
    }
    if (P.touches[1]) {
      var be = t.projectIntoViewport(P.touches[1].clientX, P.touches[1].clientY);
      fe[2] = be[0], fe[3] = be[1];
    }
    if (P.touches[2]) {
      var be = t.projectIntoViewport(P.touches[2].clientX, P.touches[2].clientY);
      fe[4] = be[0], fe[5] = be[1];
    }
    var De = function(rr) {
      return {
        originalEvent: P,
        type: rr,
        position: {
          x: fe[0],
          y: fe[1]
        }
      };
    };
    q && q.unactivate();
    var Be;
    if (t.touchData.cxt) {
      if (Be = De("cxttapend"), q ? q.emit(Be) : ee.emit(Be), !t.touchData.cxtDragged) {
        var Me = De("cxttap");
        q ? q.emit(Me) : ee.emit(Me);
      }
      t.touchData.start && (t.touchData.start._private.grabbed = !1), t.touchData.cxt = !1, t.touchData.start = null, t.redraw();
      return;
    }
    if (!P.touches[2] && ee.boxSelectionEnabled() && t.touchData.selecting) {
      t.touchData.selecting = !1;
      var xe = ee.collection(t.getAllInBox(re[0], re[1], re[2], re[3]));
      re[0] = void 0, re[1] = void 0, re[2] = void 0, re[3] = void 0, re[4] = 0, t.redrawHint("select", !0), ee.emit(De("boxend"));
      var Ie = function(rr) {
        return rr.selectable() && !rr.selected();
      };
      xe.emit(De("box")).stdFilter(Ie).select().emit(De("boxselect")), xe.nonempty() && t.redrawHint("eles", !0), t.redraw();
    }
    if (q != null && q.unactivate(), P.touches[2])
      t.data.bgActivePosistion = void 0, t.redrawHint("select", !0);
    else if (!P.touches[1]) {
      if (!P.touches[0]) {
        if (!P.touches[0]) {
          t.data.bgActivePosistion = void 0, t.redrawHint("select", !0);
          var Oe = t.dragData.touchDragEles;
          if (q != null) {
            var gt = q._private.grabbed;
            y(Oe), t.redrawHint("drag", !0), t.redrawHint("eles", !0), gt && (q.emit(De("freeon")), Oe.emit(De("free")), t.dragData.didDrag && (q.emit(De("dragfreeon")), Oe.emit(De("dragfree")))), a(q, ["touchend", "tapend", "vmouseup", "tapdragout"], P, {
              x: fe[0],
              y: fe[1]
            }), q.unactivate(), t.touchData.start = null;
          } else {
            var rt = t.findNearestElement(fe[0], fe[1], !0, !0);
            a(rt, ["touchend", "tapend", "vmouseup", "tapdragout"], P, {
              x: fe[0],
              y: fe[1]
            });
          }
          var tr = t.touchData.startPosition[0] - fe[0], nt = tr * tr, st = t.touchData.startPosition[1] - fe[1], ft = st * st, wt = nt + ft, cr = wt * ye * ye;
          t.touchData.singleTouchMoved || (q || ee.$(":selected").unselect(["tapunselect"]), a(q, ["tap", "vclick"], P, {
            x: fe[0],
            y: fe[1]
          }), te = !1, P.timeStamp - K <= ee.multiClickDebounceTime() ? (Y && clearTimeout(Y), te = !0, K = null, a(q, ["dbltap", "vdblclick"], P, {
            x: fe[0],
            y: fe[1]
          })) : (Y = setTimeout(function() {
            te || a(q, ["onetap", "voneclick"], P, {
              x: fe[0],
              y: fe[1]
            });
          }, ee.multiClickDebounceTime()), K = P.timeStamp)), q != null && !t.dragData.didDrag && q._private.selectable && cr < t.touchTapThreshold2 && !t.pinching && (ee.selectionType() === "single" ? (ee.$(r).unmerge(q).unselect(["tapunselect"]), q.select(["tapselect"])) : q.selected() ? q.unselect(["tapunselect"]) : q.select(["tapselect"]), t.redrawHint("eles", !0)), t.touchData.singleTouchMoved = !0;
        }
      }
    }
    for (var Je = 0; Je < fe.length; Je++)
      ge[Je] = fe[Je];
    t.dragData.didDrag = !1, P.touches.length === 0 && (t.touchData.dragDelta = [], t.touchData.startPosition = [null, null, null, null, null, null], t.touchData.startGPosition = null, t.touchData.didSelect = !1), P.touches.length < 2 && (P.touches.length === 1 && (t.touchData.startGPosition = [P.touches[0].clientX, P.touches[0].clientY]), t.pinching = !1, t.redrawHint("eles", !0), t.redraw());
  }, !1), typeof TouchEvent > "u") {
    var ue = [], oe = function(P) {
      return {
        clientX: P.clientX,
        clientY: P.clientY,
        force: 1,
        identifier: P.pointerId,
        pageX: P.pageX,
        pageY: P.pageY,
        radiusX: P.width / 2,
        radiusY: P.height / 2,
        screenX: P.screenX,
        screenY: P.screenY,
        target: P.target
      };
    }, ve = function(P) {
      return {
        event: P,
        touch: oe(P)
      };
    }, de = function(P) {
      ue.push(ve(P));
    }, me = function(P) {
      for (var q = 0; q < ue.length; q++) {
        var G = ue[q];
        if (G.event.pointerId === P.pointerId) {
          ue.splice(q, 1);
          return;
        }
      }
    }, Te = function(P) {
      var q = ue.filter(function(G) {
        return G.event.pointerId === P.pointerId;
      })[0];
      q.event = P, q.touch = oe(P);
    }, Ee = function(P) {
      P.touches = ue.map(function(q) {
        return q.touch;
      });
    }, Pe = function(P) {
      return P.pointerType === "mouse" || P.pointerType === 4;
    };
    t.registerBinding(t.container, "pointerdown", function(J) {
      Pe(J) || (J.preventDefault(), de(J), Ee(J), W(J));
    }), t.registerBinding(t.container, "pointerup", function(J) {
      Pe(J) || (me(J), Ee(J), ne(J));
    }), t.registerBinding(t.container, "pointercancel", function(J) {
      Pe(J) || (me(J), Ee(J), Z(J));
    }), t.registerBinding(t.container, "pointermove", function(J) {
      Pe(J) || (J.preventDefault(), Te(J), Ee(J), j(J));
    });
  }
};
var Ur = {};
Ur.generatePolygon = function(t, e) {
  return this.nodeShapes[t] = {
    renderer: this,
    name: t,
    points: e,
    draw: function(n, a, i, s, o, l) {
      this.renderer.nodeShapeImpl("polygon", n, a, i, s, o, this.points);
    },
    intersectLine: function(n, a, i, s, o, l, u, f) {
      return ii(o, l, this.points, n, a, i / 2, s / 2, u);
    },
    checkPoint: function(n, a, i, s, o, l, u, f) {
      return $r(n, a, this.points, l, u, s, o, [0, -1], i);
    },
    hasMiterBounds: t !== "rectangle",
    miterBounds: function(n, a, i, s, o, l) {
      return Gb(this.points, n, a, i, s, o);
    }
  };
};
Ur.generateEllipse = function() {
  return this.nodeShapes.ellipse = {
    renderer: this,
    name: "ellipse",
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i);
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      return Jb(i, s, e, r, n / 2 + o, a / 2 + o);
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      return Pn(e, r, a, i, s, o, n);
    }
  };
};
Ur.generateRoundPolygon = function(t, e) {
  return this.nodeShapes[t] = {
    renderer: this,
    name: t,
    points: e,
    getOrCreateCorners: function(n, a, i, s, o, l, u) {
      if (l[u] !== void 0 && l[u + "-cx"] === n && l[u + "-cy"] === a)
        return l[u];
      l[u] = new Array(e.length / 2), l[u + "-cx"] = n, l[u + "-cy"] = a;
      var f = i / 2, c = s / 2;
      o = o === "auto" ? og(i, s) : o;
      for (var v = new Array(e.length / 2), d = 0; d < e.length / 2; d++)
        v[d] = {
          x: n + f * e[d * 2],
          y: a + c * e[d * 2 + 1]
        };
      var h, y, g, p, m = v.length;
      for (y = v[m - 1], h = 0; h < m; h++)
        g = v[h % m], p = v[(h + 1) % m], l[u][h] = df(y, g, p, o), y = g, g = p;
      return l[u];
    },
    draw: function(n, a, i, s, o, l, u) {
      this.renderer.nodeShapeImpl("round-polygon", n, a, i, s, o, this.points, this.getOrCreateCorners(a, i, s, o, l, u, "drawCorners"));
    },
    intersectLine: function(n, a, i, s, o, l, u, f, c) {
      return t1(o, l, this.points, n, a, i, s, u, this.getOrCreateCorners(n, a, i, s, f, c, "corners"));
    },
    checkPoint: function(n, a, i, s, o, l, u, f, c) {
      return jb(n, a, this.points, l, u, s, o, this.getOrCreateCorners(l, u, s, o, f, c, "corners"));
    }
  };
};
Ur.generateRoundRectangle = function() {
  return this.nodeShapes["round-rectangle"] = this.nodeShapes.roundrectangle = {
    renderer: this,
    name: "round-rectangle",
    points: Ot(4, 0),
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i, this.points, s);
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      return ig(i, s, e, r, n, a, o, l);
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      var u = a / 2, f = i / 2;
      l = l === "auto" ? sn(a, i) : l, l = Math.min(u, f, l);
      var c = l * 2;
      return !!($r(e, r, this.points, s, o, a, i - c, [0, -1], n) || $r(e, r, this.points, s, o, a - c, i, [0, -1], n) || Pn(e, r, c, c, s - u + l, o - f + l, n) || Pn(e, r, c, c, s + u - l, o - f + l, n) || Pn(e, r, c, c, s + u - l, o + f - l, n) || Pn(e, r, c, c, s - u + l, o + f - l, n));
    }
  };
};
Ur.generateCutRectangle = function() {
  return this.nodeShapes["cut-rectangle"] = this.nodeShapes.cutrectangle = {
    renderer: this,
    name: "cut-rectangle",
    cornerLength: tf(),
    points: Ot(4, 0),
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i, null, s);
    },
    generateCutTrianglePts: function(e, r, n, a, i) {
      var s = i === "auto" ? this.cornerLength : i, o = r / 2, l = e / 2, u = n - l, f = n + l, c = a - o, v = a + o;
      return {
        topLeft: [u, c + s, u + s, c, u + s, c + s],
        topRight: [f - s, c, f, c + s, f - s, c + s],
        bottomRight: [f, v - s, f - s, v, f - s, v - s],
        bottomLeft: [u + s, v, u, v - s, u + s, v - s]
      };
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      var u = this.generateCutTrianglePts(n + 2 * o, a + 2 * o, e, r, l), f = [].concat.apply([], [u.topLeft.splice(0, 4), u.topRight.splice(0, 4), u.bottomRight.splice(0, 4), u.bottomLeft.splice(0, 4)]);
      return ii(i, s, f, e, r);
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      var u = l === "auto" ? this.cornerLength : l;
      if ($r(e, r, this.points, s, o, a, i - 2 * u, [0, -1], n) || $r(e, r, this.points, s, o, a - 2 * u, i, [0, -1], n))
        return !0;
      var f = this.generateCutTrianglePts(a, i, s, o);
      return Ut(e, r, f.topLeft) || Ut(e, r, f.topRight) || Ut(e, r, f.bottomRight) || Ut(e, r, f.bottomLeft);
    }
  };
};
Ur.generateBarrel = function() {
  return this.nodeShapes.barrel = {
    renderer: this,
    name: "barrel",
    points: Ot(4, 0),
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i);
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      var u = 0.15, f = 0.5, c = 0.85, v = this.generateBarrelBezierPts(n + 2 * o, a + 2 * o, e, r), d = function(g) {
        var p = aa({
          x: g[0],
          y: g[1]
        }, {
          x: g[2],
          y: g[3]
        }, {
          x: g[4],
          y: g[5]
        }, u), m = aa({
          x: g[0],
          y: g[1]
        }, {
          x: g[2],
          y: g[3]
        }, {
          x: g[4],
          y: g[5]
        }, f), b = aa({
          x: g[0],
          y: g[1]
        }, {
          x: g[2],
          y: g[3]
        }, {
          x: g[4],
          y: g[5]
        }, c);
        return [g[0], g[1], p.x, p.y, m.x, m.y, b.x, b.y, g[4], g[5]];
      }, h = [].concat(d(v.topLeft), d(v.topRight), d(v.bottomRight), d(v.bottomLeft));
      return ii(i, s, h, e, r);
    },
    generateBarrelBezierPts: function(e, r, n, a) {
      var i = r / 2, s = e / 2, o = n - s, l = n + s, u = a - i, f = a + i, c = cu(e, r), v = c.heightOffset, d = c.widthOffset, h = c.ctrlPtOffsetPct * e, y = {
        topLeft: [o, u + v, o + h, u, o + d, u],
        topRight: [l - d, u, l - h, u, l, u + v],
        bottomRight: [l, f - v, l - h, f, l - d, f],
        bottomLeft: [o + d, f, o + h, f, o, f - v]
      };
      return y.topLeft.isTop = !0, y.topRight.isTop = !0, y.bottomLeft.isBottom = !0, y.bottomRight.isBottom = !0, y;
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      var u = cu(a, i), f = u.heightOffset, c = u.widthOffset;
      if ($r(e, r, this.points, s, o, a, i - 2 * f, [0, -1], n) || $r(e, r, this.points, s, o, a - 2 * c, i, [0, -1], n))
        return !0;
      for (var v = this.generateBarrelBezierPts(a, i, s, o), d = function(S, D, A) {
        var k = A[4], R = A[2], M = A[0], I = A[5], _ = A[1], N = Math.min(k, M), L = Math.max(k, M), F = Math.min(I, _), H = Math.max(I, _);
        if (N <= S && S <= L && F <= D && D <= H) {
          var V = r1(k, R, M), O = Yb(V[0], V[1], V[2], S), $ = O.filter(function(Q) {
            return 0 <= Q && Q <= 1;
          });
          if ($.length > 0)
            return $[0];
        }
        return null;
      }, h = Object.keys(v), y = 0; y < h.length; y++) {
        var g = h[y], p = v[g], m = d(e, r, p);
        if (m != null) {
          var b = p[5], w = p[3], E = p[1], T = yt(b, w, E, m);
          if (p.isTop && T <= r || p.isBottom && r <= T)
            return !0;
        }
      }
      return !1;
    }
  };
};
Ur.generateBottomRoundrectangle = function() {
  return this.nodeShapes["bottom-round-rectangle"] = this.nodeShapes.bottomroundrectangle = {
    renderer: this,
    name: "bottom-round-rectangle",
    points: Ot(4, 0),
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i, this.points, s);
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      var u = e - (n / 2 + o), f = r - (a / 2 + o), c = f, v = e + (n / 2 + o), d = Jr(i, s, e, r, u, f, v, c, !1);
      return d.length > 0 ? d : ig(i, s, e, r, n, a, o, l);
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      l = l === "auto" ? sn(a, i) : l;
      var u = 2 * l;
      if ($r(e, r, this.points, s, o, a, i - u, [0, -1], n) || $r(e, r, this.points, s, o, a - u, i, [0, -1], n))
        return !0;
      var f = a / 2 + 2 * n, c = i / 2 + 2 * n, v = [s - f, o - c, s - f, o, s + f, o, s + f, o - c];
      return !!(Ut(e, r, v) || Pn(e, r, u, u, s + a / 2 - l, o + i / 2 - l, n) || Pn(e, r, u, u, s - a / 2 + l, o + i / 2 - l, n));
    }
  };
};
Ur.registerNodeShapes = function() {
  var t = this.nodeShapes = {}, e = this;
  this.generateEllipse(), this.generatePolygon("triangle", Ot(3, 0)), this.generateRoundPolygon("round-triangle", Ot(3, 0)), this.generatePolygon("rectangle", Ot(4, 0)), t.square = t.rectangle, this.generateRoundRectangle(), this.generateCutRectangle(), this.generateBarrel(), this.generateBottomRoundrectangle();
  {
    var r = [0, 1, 1, 0, 0, -1, -1, 0];
    this.generatePolygon("diamond", r), this.generateRoundPolygon("round-diamond", r);
  }
  this.generatePolygon("pentagon", Ot(5, 0)), this.generateRoundPolygon("round-pentagon", Ot(5, 0)), this.generatePolygon("hexagon", Ot(6, 0)), this.generateRoundPolygon("round-hexagon", Ot(6, 0)), this.generatePolygon("heptagon", Ot(7, 0)), this.generateRoundPolygon("round-heptagon", Ot(7, 0)), this.generatePolygon("octagon", Ot(8, 0)), this.generateRoundPolygon("round-octagon", Ot(8, 0));
  var n = new Array(20);
  {
    var a = fu(5, 0), i = fu(5, Math.PI / 5), s = 0.5 * (3 - Math.sqrt(5));
    s *= 1.57;
    for (var o = 0; o < i.length / 2; o++)
      i[o * 2] *= s, i[o * 2 + 1] *= s;
    for (var o = 0; o < 20 / 4; o++)
      n[o * 4] = a[o * 2], n[o * 4 + 1] = a[o * 2 + 1], n[o * 4 + 2] = i[o * 2], n[o * 4 + 3] = i[o * 2 + 1];
  }
  n = sg(n), this.generatePolygon("star", n), this.generatePolygon("vee", [-1, -1, 0, -0.333, 1, -1, 0, 1]), this.generatePolygon("rhomboid", [-1, -1, 0.333, -1, 1, 1, -0.333, 1]), this.generatePolygon("right-rhomboid", [-0.333, -1, 1, -1, 0.333, 1, -1, 1]), this.nodeShapes.concavehexagon = this.generatePolygon("concave-hexagon", [-1, -0.95, -0.75, 0, -1, 0.95, 1, 0.95, 0.75, 0, 1, -0.95]);
  {
    var l = [-1, -1, 0.25, -1, 1, 0, 0.25, 1, -1, 1];
    this.generatePolygon("tag", l), this.generateRoundPolygon("round-tag", l);
  }
  t.makePolygon = function(u) {
    var f = u.join("$"), c = "polygon-" + f, v;
    return (v = this[c]) ? v : e.generatePolygon(c, u);
  };
};
var Si = {};
Si.timeToRender = function() {
  return this.redrawTotalTime / this.redrawCount;
};
Si.redraw = function(t) {
  t = t || tg();
  var e = this;
  e.averageRedrawTime === void 0 && (e.averageRedrawTime = 0), e.lastRedrawTime === void 0 && (e.lastRedrawTime = 0), e.lastDrawTime === void 0 && (e.lastDrawTime = 0), e.requestedFrame = !0, e.renderOptions = t;
};
Si.beforeRender = function(t, e) {
  if (!this.destroyed) {
    e == null && je("Priority is not optional for beforeRender");
    var r = this.beforeRenderCallbacks;
    r.push({
      fn: t,
      priority: e
    }), r.sort(function(n, a) {
      return a.priority - n.priority;
    });
  }
};
var cd = function(e, r, n) {
  for (var a = e.beforeRenderCallbacks, i = 0; i < a.length; i++)
    a[i].fn(r, n);
};
Si.startRenderLoop = function() {
  var t = this, e = t.cy;
  if (!t.renderLoopStarted) {
    t.renderLoopStarted = !0;
    var r = function(a) {
      if (!t.destroyed) {
        if (!e.batching()) if (t.requestedFrame && !t.skipFrame) {
          cd(t, !0, a);
          var i = qr();
          t.render(t.renderOptions);
          var s = t.lastDrawTime = qr();
          t.averageRedrawTime === void 0 && (t.averageRedrawTime = s - i), t.redrawCount === void 0 && (t.redrawCount = 0), t.redrawCount++, t.redrawTotalTime === void 0 && (t.redrawTotalTime = 0);
          var o = s - i;
          t.redrawTotalTime += o, t.lastRedrawTime = o, t.averageRedrawTime = t.averageRedrawTime / 2 + o / 2, t.requestedFrame = !1;
        } else
          cd(t, !1, a);
        t.skipFrame = !1, bs(r);
      }
    };
    bs(r);
  }
};
var xE = function(e) {
  this.init(e);
}, lp = xE, xa = lp.prototype;
xa.clientFunctions = ["redrawHint", "render", "renderTo", "matchCanvasSize", "nodeShapeImpl", "arrowShapeImpl"];
xa.init = function(t) {
  var e = this;
  e.options = t, e.cy = t.cy;
  var r = e.container = t.cy.container(), n = e.cy.window();
  if (n) {
    var a = n.document, i = a.head, s = "__________cytoscape_stylesheet", o = "__________cytoscape_container", l = a.getElementById(s) != null;
    if (r.className.indexOf(o) < 0 && (r.className = (r.className || "") + " " + o), !l) {
      var u = a.createElement("style");
      u.id = s, u.textContent = "." + o + " { position: relative; }", i.insertBefore(u, i.children[0]);
    }
    var f = n.getComputedStyle(r), c = f.getPropertyValue("position");
    c === "static" && $e("A Cytoscape container has style position:static and so can not use UI extensions properly");
  }
  e.selection = [void 0, void 0, void 0, void 0, 0], e.bezierProjPcts = [0.05, 0.225, 0.4, 0.5, 0.6, 0.775, 0.95], e.hoverData = {
    down: null,
    last: null,
    downTime: null,
    triggerMode: null,
    dragging: !1,
    initialPan: [null, null],
    capture: !1
  }, e.dragData = {
    possibleDragElements: []
  }, e.touchData = {
    start: null,
    capture: !1,
    // These 3 fields related to tap, taphold events
    startPosition: [null, null, null, null, null, null],
    singleTouchStartTime: null,
    singleTouchMoved: !0,
    now: [null, null, null, null, null, null],
    earlier: [null, null, null, null, null, null]
  }, e.redraws = 0, e.showFps = t.showFps, e.debug = t.debug, e.webgl = t.webgl, e.hideEdgesOnViewport = t.hideEdgesOnViewport, e.textureOnViewport = t.textureOnViewport, e.wheelSensitivity = t.wheelSensitivity, e.motionBlurEnabled = t.motionBlur, e.forcedPixelRatio = pe(t.pixelRatio) ? t.pixelRatio : null, e.motionBlur = t.motionBlur, e.motionBlurOpacity = t.motionBlurOpacity, e.motionBlurTransparency = 1 - e.motionBlurOpacity, e.motionBlurPxRatio = 1, e.mbPxRBlurry = 1, e.minMbLowQualFrames = 4, e.fullQualityMb = !1, e.clearedForMotionBlur = [], e.desktopTapThreshold = t.desktopTapThreshold, e.desktopTapThreshold2 = t.desktopTapThreshold * t.desktopTapThreshold, e.touchTapThreshold = t.touchTapThreshold, e.touchTapThreshold2 = t.touchTapThreshold * t.touchTapThreshold, e.tapholdDuration = 500, e.bindings = [], e.beforeRenderCallbacks = [], e.beforeRenderPriorities = {
    // higher priority execs before lower one
    animations: 400,
    eleCalcs: 300,
    eleTxrDeq: 200,
    lyrTxrDeq: 150,
    lyrTxrSkip: 100
  }, e.registerNodeShapes(), e.registerArrowShapes(), e.registerCalculationListeners();
};
xa.notify = function(t, e) {
  var r = this, n = r.cy;
  if (!this.destroyed) {
    if (t === "init") {
      r.load();
      return;
    }
    if (t === "destroy") {
      r.destroy();
      return;
    }
    (t === "add" || t === "remove" || t === "move" && n.hasCompoundNodes() || t === "load" || t === "zorder" || t === "mount") && r.invalidateCachedZSortedEles(), t === "viewport" && r.redrawHint("select", !0), t === "gc" && r.redrawHint("gc", !0), (t === "load" || t === "resize" || t === "mount") && (r.invalidateContainerClientCoordsCache(), r.matchCanvasSize(r.container)), r.redrawHint("eles", !0), r.redrawHint("drag", !0), this.startRenderLoop(), this.redraw();
  }
};
xa.destroy = function() {
  var t = this;
  t.destroyed = !0, t.cy.stopAnimationLoop();
  for (var e = 0; e < t.bindings.length; e++) {
    var r = t.bindings[e], n = r, a = n.target;
    (a.off || a.removeEventListener).apply(a, n.args);
  }
  if (t.bindings = [], t.beforeRenderCallbacks = [], t.onUpdateEleCalcsFns = [], t.removeObserver && t.removeObserver.disconnect(), t.styleObserver && t.styleObserver.disconnect(), t.resizeObserver && t.resizeObserver.disconnect(), t.labelCalcDiv)
    try {
      document.body.removeChild(t.labelCalcDiv);
    } catch {
    }
};
xa.isHeadless = function() {
  return !1;
};
[vf, ip, sp, wa, Ur, Si].forEach(function(t) {
  Ae(xa, t);
});
var Ul = 1e3 / 60, up = {
  setupDequeueing: function(e) {
    return function() {
      var n = this, a = this.renderer;
      if (!n.dequeueingSetup) {
        n.dequeueingSetup = !0;
        var i = wi(function() {
          a.redrawHint("eles", !0), a.redrawHint("drag", !0), a.redraw();
        }, e.deqRedrawThreshold), s = function(u, f) {
          var c = qr(), v = a.averageRedrawTime, d = a.lastRedrawTime, h = [], y = a.cy.extent(), g = a.getPixelRatio();
          for (u || a.flushRenderedStyleQueue(); ; ) {
            var p = qr(), m = p - c, b = p - f;
            if (d < Ul) {
              var w = Ul - (u ? v : 0);
              if (b >= e.deqFastCost * w)
                break;
            } else if (u) {
              if (m >= e.deqCost * d || m >= e.deqAvgCost * v)
                break;
            } else if (b >= e.deqNoDrawCost * Ul)
              break;
            var E = e.deq(n, g, y);
            if (E.length > 0)
              for (var T = 0; T < E.length; T++)
                h.push(E[T]);
            else
              break;
          }
          h.length > 0 && (e.onDeqd(n, h), !u && e.shouldRedraw(n, h, g, y) && i());
        }, o = e.priority || Zu;
        a.beforeRender(s, o(n));
      }
    };
  }
}, EE = /* @__PURE__ */ (function() {
  function t(e) {
    var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : ws;
    fn(this, t), this.idsByKey = new Nr(), this.keyForId = new Nr(), this.cachesByLvl = new Nr(), this.lvls = [], this.getKey = e, this.doesEleInvalidateKey = r;
  }
  return cn(t, [{
    key: "getIdsFor",
    value: function(r) {
      r == null && je("Can not get id list for null key");
      var n = this.idsByKey, a = this.idsByKey.get(r);
      return a || (a = new pa(), n.set(r, a)), a;
    }
  }, {
    key: "addIdForKey",
    value: function(r, n) {
      r != null && this.getIdsFor(r).add(n);
    }
  }, {
    key: "deleteIdForKey",
    value: function(r, n) {
      r != null && this.getIdsFor(r).delete(n);
    }
  }, {
    key: "getNumberOfIdsForKey",
    value: function(r) {
      return r == null ? 0 : this.getIdsFor(r).size;
    }
  }, {
    key: "updateKeyMappingFor",
    value: function(r) {
      var n = r.id(), a = this.keyForId.get(n), i = this.getKey(r);
      this.deleteIdForKey(a, n), this.addIdForKey(i, n), this.keyForId.set(n, i);
    }
  }, {
    key: "deleteKeyMappingFor",
    value: function(r) {
      var n = r.id(), a = this.keyForId.get(n);
      this.deleteIdForKey(a, n), this.keyForId.delete(n);
    }
  }, {
    key: "keyHasChangedFor",
    value: function(r) {
      var n = r.id(), a = this.keyForId.get(n), i = this.getKey(r);
      return a !== i;
    }
  }, {
    key: "isInvalid",
    value: function(r) {
      return this.keyHasChangedFor(r) || this.doesEleInvalidateKey(r);
    }
  }, {
    key: "getCachesAt",
    value: function(r) {
      var n = this.cachesByLvl, a = this.lvls, i = n.get(r);
      return i || (i = new Nr(), n.set(r, i), a.push(r)), i;
    }
  }, {
    key: "getCache",
    value: function(r, n) {
      return this.getCachesAt(n).get(r);
    }
  }, {
    key: "get",
    value: function(r, n) {
      var a = this.getKey(r), i = this.getCache(a, n);
      return i != null && this.updateKeyMappingFor(r), i;
    }
  }, {
    key: "getForCachedKey",
    value: function(r, n) {
      var a = this.keyForId.get(r.id()), i = this.getCache(a, n);
      return i;
    }
  }, {
    key: "hasCache",
    value: function(r, n) {
      return this.getCachesAt(n).has(r);
    }
  }, {
    key: "has",
    value: function(r, n) {
      var a = this.getKey(r);
      return this.hasCache(a, n);
    }
  }, {
    key: "setCache",
    value: function(r, n, a) {
      a.key = r, this.getCachesAt(n).set(r, a);
    }
  }, {
    key: "set",
    value: function(r, n, a) {
      var i = this.getKey(r);
      this.setCache(i, n, a), this.updateKeyMappingFor(r);
    }
  }, {
    key: "deleteCache",
    value: function(r, n) {
      this.getCachesAt(n).delete(r);
    }
  }, {
    key: "delete",
    value: function(r, n) {
      var a = this.getKey(r);
      this.deleteCache(a, n);
    }
  }, {
    key: "invalidateKey",
    value: function(r) {
      var n = this;
      this.lvls.forEach(function(a) {
        return n.deleteCache(r, a);
      });
    }
    // returns true if no other eles reference the invalidated cache (n.b. other eles may need the cache with the same key)
  }, {
    key: "invalidate",
    value: function(r) {
      var n = r.id(), a = this.keyForId.get(n);
      this.deleteKeyMappingFor(r);
      var i = this.doesEleInvalidateKey(r);
      return i && this.invalidateKey(a), i || this.getNumberOfIdsForKey(a) === 0;
    }
  }]);
})(), vd = 25, Ui = 50, ss = -4, Tu = 3, fp = 7.99, CE = 8, TE = 1024, SE = 1024, PE = 1024, DE = 0.2, AE = 0.8, kE = 10, BE = 0.15, RE = 0.1, ME = 0.9, LE = 0.9, IE = 100, OE = 1, jn = {
  dequeue: "dequeue",
  downscale: "downscale",
  highQuality: "highQuality"
}, NE = St({
  getKey: null,
  doesEleInvalidateKey: ws,
  drawElement: null,
  getBoundingBox: null,
  getRotationPoint: null,
  getRotationOffset: null,
  isVisible: jh,
  allowEdgeTxrCaching: !0,
  allowParentTxrCaching: !0
}), za = function(e, r) {
  var n = this;
  n.renderer = e, n.onDequeues = [];
  var a = NE(r);
  Ae(n, a), n.lookup = new EE(a.getKey, a.doesEleInvalidateKey), n.setupDequeueing();
}, ht = za.prototype;
ht.reasons = jn;
ht.getTextureQueue = function(t) {
  var e = this;
  return e.eleImgCaches = e.eleImgCaches || {}, e.eleImgCaches[t] = e.eleImgCaches[t] || [];
};
ht.getRetiredTextureQueue = function(t) {
  var e = this, r = e.eleImgCaches.retired = e.eleImgCaches.retired || {}, n = r[t] = r[t] || [];
  return n;
};
ht.getElementQueue = function() {
  var t = this, e = t.eleCacheQueue = t.eleCacheQueue || new xi(function(r, n) {
    return n.reqs - r.reqs;
  });
  return e;
};
ht.getElementKeyToQueue = function() {
  var t = this, e = t.eleKeyToCacheQueue = t.eleKeyToCacheQueue || {};
  return e;
};
ht.getElement = function(t, e, r, n, a) {
  var i = this, s = this.renderer, o = s.cy.zoom(), l = this.lookup;
  if (!e || e.w === 0 || e.h === 0 || isNaN(e.w) || isNaN(e.h) || !t.visible() || t.removed() || !i.allowEdgeTxrCaching && t.isEdge() || !i.allowParentTxrCaching && t.isParent())
    return null;
  if (n == null && (n = Math.ceil(ju(o * r))), n < ss)
    n = ss;
  else if (o >= fp || n > Tu)
    return null;
  var u = Math.pow(2, n), f = e.h * u, c = e.w * u, v = s.eleTextBiggerThanMin(t, u);
  if (!this.isVisible(t, v))
    return null;
  var d = l.get(t, n);
  if (d && d.invalidated && (d.invalidated = !1, d.texture.invalidatedWidth -= d.width), d)
    return d;
  var h;
  if (f <= vd ? h = vd : f <= Ui ? h = Ui : h = Math.ceil(f / Ui) * Ui, f > PE || c > SE)
    return null;
  var y = i.getTextureQueue(h), g = y[y.length - 2], p = function() {
    return i.recycleTexture(h, c) || i.addTexture(h, c);
  };
  g || (g = y[y.length - 1]), g || (g = p()), g.width - g.usedWidth < c && (g = p());
  for (var m = function(N) {
    return N && N.scaledLabelShown === v;
  }, b = a && a === jn.dequeue, w = a && a === jn.highQuality, E = a && a === jn.downscale, T, x = n + 1; x <= Tu; x++) {
    var S = l.get(t, x);
    if (S) {
      T = S;
      break;
    }
  }
  var D = T && T.level === n + 1 ? T : null, A = function() {
    g.context.drawImage(D.texture.canvas, D.x, 0, D.width, D.height, g.usedWidth, 0, c, f);
  };
  if (g.context.setTransform(1, 0, 0, 1, 0, 0), g.context.clearRect(g.usedWidth, 0, c, h), m(D))
    A();
  else if (m(T))
    if (w) {
      for (var k = T.level; k > n; k--)
        D = i.getElement(t, e, r, k, jn.downscale);
      A();
    } else
      return i.queueElement(t, T.level - 1), T;
  else {
    var R;
    if (!b && !w && !E)
      for (var M = n - 1; M >= ss; M--) {
        var I = l.get(t, M);
        if (I) {
          R = I;
          break;
        }
      }
    if (m(R))
      return i.queueElement(t, n), R;
    g.context.translate(g.usedWidth, 0), g.context.scale(u, u), this.drawElement(g.context, t, e, v, !1), g.context.scale(1 / u, 1 / u), g.context.translate(-g.usedWidth, 0);
  }
  return d = {
    x: g.usedWidth,
    texture: g,
    level: n,
    scale: u,
    width: c,
    height: f,
    scaledLabelShown: v
  }, g.usedWidth += Math.ceil(c + CE), g.eleCaches.push(d), l.set(t, n, d), i.checkTextureFullness(g), d;
};
ht.invalidateElements = function(t) {
  for (var e = 0; e < t.length; e++)
    this.invalidateElement(t[e]);
};
ht.invalidateElement = function(t) {
  var e = this, r = e.lookup, n = [], a = r.isInvalid(t);
  if (a) {
    for (var i = ss; i <= Tu; i++) {
      var s = r.getForCachedKey(t, i);
      s && n.push(s);
    }
    var o = r.invalidate(t);
    if (o)
      for (var l = 0; l < n.length; l++) {
        var u = n[l], f = u.texture;
        f.invalidatedWidth += u.width, u.invalidated = !0, e.checkTextureUtility(f);
      }
    e.removeFromQueue(t);
  }
};
ht.checkTextureUtility = function(t) {
  t.invalidatedWidth >= DE * t.width && this.retireTexture(t);
};
ht.checkTextureFullness = function(t) {
  var e = this, r = e.getTextureQueue(t.height);
  t.usedWidth / t.width > AE && t.fullnessChecks >= kE ? an(r, t) : t.fullnessChecks++;
};
ht.retireTexture = function(t) {
  var e = this, r = t.height, n = e.getTextureQueue(r), a = this.lookup;
  an(n, t), t.retired = !0;
  for (var i = t.eleCaches, s = 0; s < i.length; s++) {
    var o = i[s];
    a.deleteCache(o.key, o.level);
  }
  Qu(i);
  var l = e.getRetiredTextureQueue(r);
  l.push(t);
};
ht.addTexture = function(t, e) {
  var r = this, n = r.getTextureQueue(t), a = {};
  return n.push(a), a.eleCaches = [], a.height = t, a.width = Math.max(TE, e), a.usedWidth = 0, a.invalidatedWidth = 0, a.fullnessChecks = 0, a.canvas = r.renderer.makeOffscreenCanvas(a.width, a.height), a.context = a.canvas.getContext("2d"), a;
};
ht.recycleTexture = function(t, e) {
  for (var r = this, n = r.getTextureQueue(t), a = r.getRetiredTextureQueue(t), i = 0; i < a.length; i++) {
    var s = a[i];
    if (s.width >= e)
      return s.retired = !1, s.usedWidth = 0, s.invalidatedWidth = 0, s.fullnessChecks = 0, Qu(s.eleCaches), s.context.setTransform(1, 0, 0, 1, 0, 0), s.context.clearRect(0, 0, s.width, s.height), an(a, s), n.push(s), s;
  }
};
ht.queueElement = function(t, e) {
  var r = this, n = r.getElementQueue(), a = r.getElementKeyToQueue(), i = this.getKey(t), s = a[i];
  if (s)
    s.level = Math.max(s.level, e), s.eles.merge(t), s.reqs++, n.updateItem(s);
  else {
    var o = {
      eles: t.spawn().merge(t),
      level: e,
      reqs: 1,
      key: i
    };
    n.push(o), a[i] = o;
  }
};
ht.dequeue = function(t) {
  for (var e = this, r = e.getElementQueue(), n = e.getElementKeyToQueue(), a = [], i = e.lookup, s = 0; s < OE && r.size() > 0; s++) {
    var o = r.pop(), l = o.key, u = o.eles[0], f = i.hasCache(u, o.level);
    if (n[l] = null, f)
      continue;
    a.push(o);
    var c = e.getBoundingBox(u);
    e.getElement(u, c, t, o.level, jn.dequeue);
  }
  return a;
};
ht.removeFromQueue = function(t) {
  var e = this, r = e.getElementQueue(), n = e.getElementKeyToQueue(), a = this.getKey(t), i = n[a];
  i != null && (i.eles.length === 1 ? (i.reqs = Xu, r.updateItem(i), r.pop(), n[a] = null) : i.eles.unmerge(t));
};
ht.onDequeue = function(t) {
  this.onDequeues.push(t);
};
ht.offDequeue = function(t) {
  an(this.onDequeues, t);
};
ht.setupDequeueing = up.setupDequeueing({
  deqRedrawThreshold: IE,
  deqCost: BE,
  deqAvgCost: RE,
  deqNoDrawCost: ME,
  deqFastCost: LE,
  deq: function(e, r, n) {
    return e.dequeue(r, n);
  },
  onDeqd: function(e, r) {
    for (var n = 0; n < e.onDequeues.length; n++) {
      var a = e.onDequeues[n];
      a(r);
    }
  },
  shouldRedraw: function(e, r, n, a) {
    for (var i = 0; i < r.length; i++)
      for (var s = r[i].eles, o = 0; o < s.length; o++) {
        var l = s[o].boundingBox();
        if (ef(l, a))
          return !0;
      }
    return !1;
  },
  priority: function(e) {
    return e.renderer.beforeRenderPriorities.eleTxrDeq;
  }
});
var _E = 1, Ka = -4, As = 2, FE = 3.99, zE = 50, VE = 50, qE = 0.15, $E = 0.1, HE = 0.9, UE = 0.9, GE = 1, dd = 250, WE = 4e3 * 4e3, hd = 32767, KE = !0, cp = function(e) {
  var r = this, n = r.renderer = e, a = n.cy;
  r.layersByLevel = {}, r.firstGet = !0, r.lastInvalidationTime = qr() - 2 * dd, r.skipping = !1, r.eleTxrDeqs = a.collection(), r.scheduleElementRefinement = wi(function() {
    r.refineElementTextures(r.eleTxrDeqs), r.eleTxrDeqs.unmerge(r.eleTxrDeqs);
  }, VE), n.beforeRender(function(s, o) {
    o - r.lastInvalidationTime <= dd ? r.skipping = !0 : r.skipping = !1;
  }, n.beforeRenderPriorities.lyrTxrSkip);
  var i = function(o, l) {
    return l.reqs - o.reqs;
  };
  r.layersQueue = new xi(i), r.setupDequeueing();
}, Pt = cp.prototype, gd = 0, YE = Math.pow(2, 53) - 1;
Pt.makeLayer = function(t, e) {
  var r = Math.pow(2, e), n = Math.ceil(t.w * r), a = Math.ceil(t.h * r), i = this.renderer.makeOffscreenCanvas(n, a), s = {
    id: gd = ++gd % YE,
    bb: t,
    level: e,
    width: n,
    height: a,
    canvas: i,
    context: i.getContext("2d"),
    eles: [],
    elesQueue: [],
    reqs: 0
  }, o = s.context, l = -s.bb.x1, u = -s.bb.y1;
  return o.scale(r, r), o.translate(l, u), s;
};
Pt.getLayers = function(t, e, r) {
  var n = this, a = n.renderer, i = a.cy, s = i.zoom(), o = n.firstGet;
  if (n.firstGet = !1, r == null) {
    if (r = Math.ceil(ju(s * e)), r < Ka)
      r = Ka;
    else if (s >= FE || r > As)
      return null;
  }
  n.validateLayersElesOrdering(r, t);
  var l = n.layersByLevel, u = Math.pow(2, r), f = l[r] = l[r] || [], c, v = n.levelIsComplete(r, t), d, h = function() {
    var A = function(_) {
      if (n.validateLayersElesOrdering(_, t), n.levelIsComplete(_, t))
        return d = l[_], !0;
    }, k = function(_) {
      if (!d)
        for (var N = r + _; Ka <= N && N <= As && !A(N); N += _)
          ;
    };
    k(1), k(-1);
    for (var R = f.length - 1; R >= 0; R--) {
      var M = f[R];
      M.invalid && an(f, M);
    }
  };
  if (!v)
    h();
  else
    return f;
  var y = function() {
    if (!c) {
      c = zt();
      for (var A = 0; A < t.length; A++)
        $b(c, t[A].boundingBox());
    }
    return c;
  }, g = function(A) {
    A = A || {};
    var k = A.after;
    y();
    var R = Math.ceil(c.w * u), M = Math.ceil(c.h * u);
    if (R > hd || M > hd)
      return null;
    var I = R * M;
    if (I > WE)
      return null;
    var _ = n.makeLayer(c, r);
    if (k != null) {
      var N = f.indexOf(k) + 1;
      f.splice(N, 0, _);
    } else (A.insert === void 0 || A.insert) && f.unshift(_);
    return _;
  };
  if (n.skipping && !o)
    return null;
  for (var p = null, m = t.length / _E, b = !o, w = 0; w < t.length; w++) {
    var E = t[w], T = E._private.rscratch, x = T.imgLayerCaches = T.imgLayerCaches || {}, S = x[r];
    if (S) {
      p = S;
      continue;
    }
    if ((!p || p.eles.length >= m || !ag(p.bb, E.boundingBox())) && (p = g({
      insert: !0,
      after: p
    }), !p))
      return null;
    d || b ? n.queueLayer(p, E) : n.drawEleInLayer(p, E, r, e), p.eles.push(E), x[r] = p;
  }
  return d || (b ? null : f);
};
Pt.getEleLevelForLayerLevel = function(t, e) {
  return t;
};
Pt.drawEleInLayer = function(t, e, r, n) {
  var a = this, i = this.renderer, s = t.context, o = e.boundingBox();
  o.w === 0 || o.h === 0 || !e.visible() || (r = a.getEleLevelForLayerLevel(r, n), i.setImgSmoothing(s, !1), i.drawCachedElement(s, e, null, null, r, KE), i.setImgSmoothing(s, !0));
};
Pt.levelIsComplete = function(t, e) {
  var r = this, n = r.layersByLevel[t];
  if (!n || n.length === 0)
    return !1;
  for (var a = 0, i = 0; i < n.length; i++) {
    var s = n[i];
    if (s.reqs > 0 || s.invalid)
      return !1;
    a += s.eles.length;
  }
  return a === e.length;
};
Pt.validateLayersElesOrdering = function(t, e) {
  var r = this.layersByLevel[t];
  if (r)
    for (var n = 0; n < r.length; n++) {
      for (var a = r[n], i = -1, s = 0; s < e.length; s++)
        if (a.eles[0] === e[s]) {
          i = s;
          break;
        }
      if (i < 0) {
        this.invalidateLayer(a);
        continue;
      }
      for (var o = i, s = 0; s < a.eles.length; s++)
        if (a.eles[s] !== e[o + s]) {
          this.invalidateLayer(a);
          break;
        }
    }
};
Pt.updateElementsInLayers = function(t, e) {
  for (var r = this, n = pi(t[0]), a = 0; a < t.length; a++)
    for (var i = n ? null : t[a], s = n ? t[a] : t[a].ele, o = s._private.rscratch, l = o.imgLayerCaches = o.imgLayerCaches || {}, u = Ka; u <= As; u++) {
      var f = l[u];
      f && (i && r.getEleLevelForLayerLevel(f.level) !== i.level || e(f, s, i));
    }
};
Pt.haveLayers = function() {
  for (var t = this, e = !1, r = Ka; r <= As; r++) {
    var n = t.layersByLevel[r];
    if (n && n.length > 0) {
      e = !0;
      break;
    }
  }
  return e;
};
Pt.invalidateElements = function(t) {
  var e = this;
  t.length !== 0 && (e.lastInvalidationTime = qr(), !(t.length === 0 || !e.haveLayers()) && e.updateElementsInLayers(t, function(n, a, i) {
    e.invalidateLayer(n);
  }));
};
Pt.invalidateLayer = function(t) {
  if (this.lastInvalidationTime = qr(), !t.invalid) {
    var e = t.level, r = t.eles, n = this.layersByLevel[e];
    an(n, t), t.elesQueue = [], t.invalid = !0, t.replacement && (t.replacement.invalid = !0);
    for (var a = 0; a < r.length; a++) {
      var i = r[a]._private.rscratch.imgLayerCaches;
      i && (i[e] = null);
    }
  }
};
Pt.refineElementTextures = function(t) {
  var e = this;
  e.updateElementsInLayers(t, function(n, a, i) {
    var s = n.replacement;
    if (s || (s = n.replacement = e.makeLayer(n.bb, n.level), s.replaces = n, s.eles = n.eles), !s.reqs)
      for (var o = 0; o < s.eles.length; o++)
        e.queueLayer(s, s.eles[o]);
  });
};
Pt.enqueueElementRefinement = function(t) {
  this.eleTxrDeqs.merge(t), this.scheduleElementRefinement();
};
Pt.queueLayer = function(t, e) {
  var r = this, n = r.layersQueue, a = t.elesQueue, i = a.hasId = a.hasId || {};
  if (!t.replacement) {
    if (e) {
      if (i[e.id()])
        return;
      a.push(e), i[e.id()] = !0;
    }
    t.reqs ? (t.reqs++, n.updateItem(t)) : (t.reqs = 1, n.push(t));
  }
};
Pt.dequeue = function(t) {
  for (var e = this, r = e.layersQueue, n = [], a = 0; a < GE && r.size() !== 0; ) {
    var i = r.peek();
    if (i.replacement) {
      r.pop();
      continue;
    }
    if (i.replaces && i !== i.replaces.replacement) {
      r.pop();
      continue;
    }
    if (i.invalid) {
      r.pop();
      continue;
    }
    var s = i.elesQueue.shift();
    s && (e.drawEleInLayer(i, s, i.level, t), a++), n.length === 0 && n.push(!0), i.elesQueue.length === 0 && (r.pop(), i.reqs = 0, i.replaces && e.applyLayerReplacement(i), e.requestRedraw());
  }
  return n;
};
Pt.applyLayerReplacement = function(t) {
  var e = this, r = e.layersByLevel[t.level], n = t.replaces, a = r.indexOf(n);
  if (!(a < 0 || n.invalid)) {
    r[a] = t;
    for (var i = 0; i < t.eles.length; i++) {
      var s = t.eles[i]._private, o = s.imgLayerCaches = s.imgLayerCaches || {};
      o && (o[t.level] = t);
    }
    e.requestRedraw();
  }
};
Pt.requestRedraw = wi(function() {
  var t = this.renderer;
  t.redrawHint("eles", !0), t.redrawHint("drag", !0), t.redraw();
}, 100);
Pt.setupDequeueing = up.setupDequeueing({
  deqRedrawThreshold: zE,
  deqCost: qE,
  deqAvgCost: $E,
  deqNoDrawCost: HE,
  deqFastCost: UE,
  deq: function(e, r) {
    return e.dequeue(r);
  },
  onDeqd: Zu,
  shouldRedraw: jh,
  priority: function(e) {
    return e.renderer.beforeRenderPriorities.lyrTxrDeq;
  }
});
var vp = {}, pd;
function XE(t, e) {
  for (var r = 0; r < e.length; r++) {
    var n = e[r];
    t.lineTo(n.x, n.y);
  }
}
function ZE(t, e, r) {
  for (var n, a = 0; a < e.length; a++) {
    var i = e[a];
    a === 0 && (n = i), t.lineTo(i.x, i.y);
  }
  t.quadraticCurveTo(r.x, r.y, n.x, n.y);
}
function yd(t, e, r) {
  t.beginPath && t.beginPath();
  for (var n = e, a = 0; a < n.length; a++) {
    var i = n[a];
    t.lineTo(i.x, i.y);
  }
  var s = r, o = r[0];
  t.moveTo(o.x, o.y);
  for (var a = 1; a < s.length; a++) {
    var i = s[a];
    t.lineTo(i.x, i.y);
  }
  t.closePath && t.closePath();
}
function QE(t, e, r, n, a) {
  t.beginPath && t.beginPath(), t.arc(r, n, a, 0, Math.PI * 2, !1);
  var i = e, s = i[0];
  t.moveTo(s.x, s.y);
  for (var o = 0; o < i.length; o++) {
    var l = i[o];
    t.lineTo(l.x, l.y);
  }
  t.closePath && t.closePath();
}
function jE(t, e, r, n) {
  t.arc(e, r, n, 0, Math.PI * 2, !1);
}
vp.arrowShapeImpl = function(t) {
  return (pd || (pd = {
    polygon: XE,
    "triangle-backcurve": ZE,
    "triangle-tee": yd,
    "circle-triangle": QE,
    "triangle-cross": yd,
    circle: jE
  }))[t];
};
var Pr = {};
Pr.drawElement = function(t, e, r, n, a, i) {
  var s = this;
  e.isNode() ? s.drawNode(t, e, r, n, a, i) : s.drawEdge(t, e, r, n, a, i);
};
Pr.drawElementOverlay = function(t, e) {
  var r = this;
  e.isNode() ? r.drawNodeOverlay(t, e) : r.drawEdgeOverlay(t, e);
};
Pr.drawElementUnderlay = function(t, e) {
  var r = this;
  e.isNode() ? r.drawNodeUnderlay(t, e) : r.drawEdgeUnderlay(t, e);
};
Pr.drawCachedElementPortion = function(t, e, r, n, a, i, s, o) {
  var l = this, u = r.getBoundingBox(e);
  if (!(u.w === 0 || u.h === 0)) {
    var f = r.getElement(e, u, n, a, i);
    if (f != null) {
      var c = o(l, e);
      if (c === 0)
        return;
      var v = s(l, e), d = u.x1, h = u.y1, y = u.w, g = u.h, p, m, b, w, E;
      if (v !== 0) {
        var T = r.getRotationPoint(e);
        b = T.x, w = T.y, t.translate(b, w), t.rotate(v), E = l.getImgSmoothing(t), E || l.setImgSmoothing(t, !0);
        var x = r.getRotationOffset(e);
        p = x.x, m = x.y;
      } else
        p = d, m = h;
      var S;
      c !== 1 && (S = t.globalAlpha, t.globalAlpha = S * c), t.drawImage(f.texture.canvas, f.x, 0, f.width, f.height, p, m, y, g), c !== 1 && (t.globalAlpha = S), v !== 0 && (t.rotate(-v), t.translate(-b, -w), E || l.setImgSmoothing(t, !1));
    } else
      r.drawElement(t, e);
  }
};
var JE = function() {
  return 0;
}, eC = function(e, r) {
  return e.getTextAngle(r, null);
}, tC = function(e, r) {
  return e.getTextAngle(r, "source");
}, rC = function(e, r) {
  return e.getTextAngle(r, "target");
}, nC = function(e, r) {
  return r.effectiveOpacity();
}, Gl = function(e, r) {
  return r.pstyle("text-opacity").pfValue * r.effectiveOpacity();
};
Pr.drawCachedElement = function(t, e, r, n, a, i) {
  var s = this, o = s.data, l = o.eleTxrCache, u = o.lblTxrCache, f = o.slbTxrCache, c = o.tlbTxrCache, v = e.boundingBox(), d = i === !0 ? l.reasons.highQuality : null;
  if (!(v.w === 0 || v.h === 0 || !e.visible()) && (!n || ef(v, n))) {
    var h = e.isEdge(), y = e.element()._private.rscratch.badLine;
    s.drawElementUnderlay(t, e), s.drawCachedElementPortion(t, e, l, r, a, d, JE, nC), (!h || !y) && s.drawCachedElementPortion(t, e, u, r, a, d, eC, Gl), h && !y && (s.drawCachedElementPortion(t, e, f, r, a, d, tC, Gl), s.drawCachedElementPortion(t, e, c, r, a, d, rC, Gl)), s.drawElementOverlay(t, e);
  }
};
Pr.drawElements = function(t, e) {
  for (var r = this, n = 0; n < e.length; n++) {
    var a = e[n];
    r.drawElement(t, a);
  }
};
Pr.drawCachedElements = function(t, e, r, n) {
  for (var a = this, i = 0; i < e.length; i++) {
    var s = e[i];
    a.drawCachedElement(t, s, r, n);
  }
};
Pr.drawCachedNodes = function(t, e, r, n) {
  for (var a = this, i = 0; i < e.length; i++) {
    var s = e[i];
    s.isNode() && a.drawCachedElement(t, s, r, n);
  }
};
Pr.drawLayeredElements = function(t, e, r, n) {
  var a = this, i = a.data.lyrTxrCache.getLayers(e, r);
  if (i)
    for (var s = 0; s < i.length; s++) {
      var o = i[s], l = o.bb;
      l.w === 0 || l.h === 0 || t.drawImage(o.canvas, l.x1, l.y1, l.w, l.h);
    }
  else
    a.drawCachedElements(t, e, r, n);
};
var Gr = {};
Gr.drawEdge = function(t, e, r) {
  var n = arguments.length > 3 && arguments[3] !== void 0 ? arguments[3] : !0, a = arguments.length > 4 && arguments[4] !== void 0 ? arguments[4] : !0, i = arguments.length > 5 && arguments[5] !== void 0 ? arguments[5] : !0, s = this, o = e._private.rscratch;
  if (!(i && !e.visible()) && !(o.badLine || o.allpts == null || isNaN(o.allpts[0]))) {
    var l;
    r && (l = r, t.translate(-l.x1, -l.y1));
    var u = i ? e.pstyle("opacity").value : 1, f = i ? e.pstyle("line-opacity").value : 1, c = e.pstyle("curve-style").value, v = e.pstyle("line-style").value, d = e.pstyle("width").pfValue, h = e.pstyle("line-cap").value, y = e.pstyle("line-outline-width").value, g = e.pstyle("line-outline-color").value, p = u * f, m = u * f, b = function() {
      var _ = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : p;
      c === "straight-triangle" ? (s.eleStrokeStyle(t, e, _), s.drawEdgeTrianglePath(e, t, o.allpts)) : (t.lineWidth = d, t.lineCap = h, s.eleStrokeStyle(t, e, _), s.drawEdgePath(e, t, o.allpts, v), t.lineCap = "butt");
    }, w = function() {
      var _ = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : p;
      if (t.lineWidth = d + y, t.lineCap = h, y > 0)
        s.colorStrokeStyle(t, g[0], g[1], g[2], _);
      else {
        t.lineCap = "butt";
        return;
      }
      c === "straight-triangle" ? s.drawEdgeTrianglePath(e, t, o.allpts) : (s.drawEdgePath(e, t, o.allpts, v), t.lineCap = "butt");
    }, E = function() {
      a && s.drawEdgeOverlay(t, e);
    }, T = function() {
      a && s.drawEdgeUnderlay(t, e);
    }, x = function() {
      var _ = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : m;
      s.drawArrowheads(t, e, _);
    }, S = function() {
      s.drawElementText(t, e, null, n);
    };
    t.lineJoin = "round";
    var D = e.pstyle("ghost").value === "yes";
    if (D) {
      var A = e.pstyle("ghost-offset-x").pfValue, k = e.pstyle("ghost-offset-y").pfValue, R = e.pstyle("ghost-opacity").value, M = p * R;
      t.translate(A, k), b(M), x(M), t.translate(-A, -k);
    } else
      w();
    T(), b(), x(), E(), S(), r && t.translate(l.x1, l.y1);
  }
};
var dp = function(e) {
  if (!["overlay", "underlay"].includes(e))
    throw new Error("Invalid state");
  return function(r, n) {
    if (n.visible()) {
      var a = n.pstyle("".concat(e, "-opacity")).value;
      if (a !== 0) {
        var i = this, s = i.usePaths(), o = n._private.rscratch, l = n.pstyle("".concat(e, "-padding")).pfValue, u = 2 * l, f = n.pstyle("".concat(e, "-color")).value;
        r.lineWidth = u, o.edgeType === "self" && !s ? r.lineCap = "butt" : r.lineCap = "round", i.colorStrokeStyle(r, f[0], f[1], f[2], a), i.drawEdgePath(n, r, o.allpts, "solid");
      }
    }
  };
};
Gr.drawEdgeOverlay = dp("overlay");
Gr.drawEdgeUnderlay = dp("underlay");
Gr.drawEdgePath = function(t, e, r, n) {
  var a = t._private.rscratch, i = e, s, o = !1, l = this.usePaths(), u = t.pstyle("line-dash-pattern").pfValue, f = t.pstyle("line-dash-offset").pfValue;
  if (l) {
    var c = r.join("$"), v = a.pathCacheKey && a.pathCacheKey === c;
    v ? (s = e = a.pathCache, o = !0) : (s = e = new Path2D(), a.pathCacheKey = c, a.pathCache = s);
  }
  if (i.setLineDash)
    switch (n) {
      case "dotted":
        i.setLineDash([1, 1]);
        break;
      case "dashed":
        i.setLineDash(u), i.lineDashOffset = f;
        break;
      case "solid":
        i.setLineDash([]);
        break;
    }
  if (!o && !a.badLine)
    switch (e.beginPath && e.beginPath(), e.moveTo(r[0], r[1]), a.edgeType) {
      case "bezier":
      case "self":
      case "compound":
      case "multibezier":
        for (var d = 2; d + 3 < r.length; d += 4)
          e.quadraticCurveTo(r[d], r[d + 1], r[d + 2], r[d + 3]);
        break;
      case "straight":
      case "haystack":
        for (var h = 2; h + 1 < r.length; h += 2)
          e.lineTo(r[h], r[h + 1]);
        break;
      case "segments":
        if (a.isRound) {
          var y = Gt(a.roundCorners), g;
          try {
            for (y.s(); !(g = y.n()).done; ) {
              var p = g.value;
              ep(e, p);
            }
          } catch (b) {
            y.e(b);
          } finally {
            y.f();
          }
          e.lineTo(r[r.length - 2], r[r.length - 1]);
        } else
          for (var m = 2; m + 1 < r.length; m += 2)
            e.lineTo(r[m], r[m + 1]);
        break;
    }
  e = i, l ? e.stroke(s) : e.stroke(), e.setLineDash && e.setLineDash([]);
};
Gr.drawEdgeTrianglePath = function(t, e, r) {
  e.fillStyle = e.strokeStyle;
  for (var n = t.pstyle("width").pfValue, a = 0; a + 1 < r.length; a += 2) {
    var i = [r[a + 2] - r[a], r[a + 3] - r[a + 1]], s = Math.sqrt(i[0] * i[0] + i[1] * i[1]), o = [i[1] / s, -i[0] / s], l = [o[0] * n / 2, o[1] * n / 2];
    e.beginPath(), e.moveTo(r[a] - l[0], r[a + 1] - l[1]), e.lineTo(r[a] + l[0], r[a + 1] + l[1]), e.lineTo(r[a + 2], r[a + 3]), e.closePath(), e.fill();
  }
};
Gr.drawArrowheads = function(t, e, r) {
  var n = e._private.rscratch, a = n.edgeType === "haystack";
  a || this.drawArrowhead(t, e, "source", n.arrowStartX, n.arrowStartY, n.srcArrowAngle, r), this.drawArrowhead(t, e, "mid-target", n.midX, n.midY, n.midtgtArrowAngle, r), this.drawArrowhead(t, e, "mid-source", n.midX, n.midY, n.midsrcArrowAngle, r), a || this.drawArrowhead(t, e, "target", n.arrowEndX, n.arrowEndY, n.tgtArrowAngle, r);
};
Gr.drawArrowhead = function(t, e, r, n, a, i, s) {
  if (!(isNaN(n) || n == null || isNaN(a) || a == null || isNaN(i) || i == null)) {
    var o = this, l = e.pstyle(r + "-arrow-shape").value;
    if (l !== "none") {
      var u = e.pstyle(r + "-arrow-fill").value === "hollow" ? "both" : "filled", f = e.pstyle(r + "-arrow-fill").value, c = e.pstyle("width").pfValue, v = e.pstyle(r + "-arrow-width"), d = v.value === "match-line" ? c : v.pfValue;
      v.units === "%" && (d *= c);
      var h = e.pstyle("opacity").value;
      s === void 0 && (s = h);
      var y = t.globalCompositeOperation;
      (s !== 1 || f === "hollow") && (t.globalCompositeOperation = "destination-out", o.colorFillStyle(t, 255, 255, 255, 1), o.colorStrokeStyle(t, 255, 255, 255, 1), o.drawArrowShape(e, t, u, c, l, d, n, a, i), t.globalCompositeOperation = y);
      var g = e.pstyle(r + "-arrow-color").value;
      o.colorFillStyle(t, g[0], g[1], g[2], s), o.colorStrokeStyle(t, g[0], g[1], g[2], s), o.drawArrowShape(e, t, f, c, l, d, n, a, i);
    }
  }
};
Gr.drawArrowShape = function(t, e, r, n, a, i, s, o, l) {
  var u = this, f = this.usePaths() && a !== "triangle-cross", c = !1, v, d = e, h = {
    x: s,
    y: o
  }, y = t.pstyle("arrow-scale").value, g = this.getArrowWidth(n, y), p = u.arrowShapes[a];
  if (f) {
    var m = u.arrowPathCache = u.arrowPathCache || [], b = Bn(a), w = m[b];
    w != null ? (v = e = w, c = !0) : (v = e = new Path2D(), m[b] = v);
  }
  c || (e.beginPath && e.beginPath(), f ? p.draw(e, 1, 0, {
    x: 0,
    y: 0
  }, 1) : p.draw(e, g, l, h, n), e.closePath && e.closePath()), e = d, f && (e.translate(s, o), e.rotate(l), e.scale(g, g)), (r === "filled" || r === "both") && (f ? e.fill(v) : e.fill()), (r === "hollow" || r === "both") && (e.lineWidth = i / (f ? g : 1), e.lineJoin = "miter", f ? e.stroke(v) : e.stroke()), f && (e.scale(1 / g, 1 / g), e.rotate(-l), e.translate(-s, -o));
};
var gf = {};
gf.safeDrawImage = function(t, e, r, n, a, i, s, o, l, u) {
  if (!(a <= 0 || i <= 0 || l <= 0 || u <= 0))
    try {
      t.drawImage(e, r, n, a, i, s, o, l, u);
    } catch (f) {
      $e(f);
    }
};
gf.drawInscribedImage = function(t, e, r, n, a) {
  var i = this, s = r.position(), o = s.x, l = s.y, u = r.cy().style(), f = u.getIndexedStyle.bind(u), c = f(r, "background-fit", "value", n), v = f(r, "background-repeat", "value", n), d = r.width(), h = r.height(), y = r.padding() * 2, g = d + (f(r, "background-width-relative-to", "value", n) === "inner" ? 0 : y), p = h + (f(r, "background-height-relative-to", "value", n) === "inner" ? 0 : y), m = r._private.rscratch, b = f(r, "background-clip", "value", n), w = b === "node", E = f(r, "background-image-opacity", "value", n) * a, T = f(r, "background-image-smoothing", "value", n), x = r.pstyle("corner-radius").value;
  x !== "auto" && (x = r.pstyle("corner-radius").pfValue);
  var S = e.width || e.cachedW, D = e.height || e.cachedH;
  (S == null || D == null) && (document.body.appendChild(e), S = e.cachedW = e.width || e.offsetWidth, D = e.cachedH = e.height || e.offsetHeight, document.body.removeChild(e));
  var A = S, k = D;
  if (f(r, "background-width", "value", n) !== "auto" && (f(r, "background-width", "units", n) === "%" ? A = f(r, "background-width", "pfValue", n) * g : A = f(r, "background-width", "pfValue", n)), f(r, "background-height", "value", n) !== "auto" && (f(r, "background-height", "units", n) === "%" ? k = f(r, "background-height", "pfValue", n) * p : k = f(r, "background-height", "pfValue", n)), !(A === 0 || k === 0)) {
    if (c === "contain") {
      var R = Math.min(g / A, p / k);
      A *= R, k *= R;
    } else if (c === "cover") {
      var R = Math.max(g / A, p / k);
      A *= R, k *= R;
    }
    var M = o - g / 2, I = f(r, "background-position-x", "units", n), _ = f(r, "background-position-x", "pfValue", n);
    I === "%" ? M += (g - A) * _ : M += _;
    var N = f(r, "background-offset-x", "units", n), L = f(r, "background-offset-x", "pfValue", n);
    N === "%" ? M += (g - A) * L : M += L;
    var F = l - p / 2, H = f(r, "background-position-y", "units", n), V = f(r, "background-position-y", "pfValue", n);
    H === "%" ? F += (p - k) * V : F += V;
    var O = f(r, "background-offset-y", "units", n), $ = f(r, "background-offset-y", "pfValue", n);
    O === "%" ? F += (p - k) * $ : F += $, m.pathCache && (M -= o, F -= l, o = 0, l = 0);
    var Q = t.globalAlpha;
    t.globalAlpha = E;
    var se = i.getImgSmoothing(t), ae = !1;
    if (T === "no" && se ? (i.setImgSmoothing(t, !1), ae = !0) : T === "yes" && !se && (i.setImgSmoothing(t, !0), ae = !0), v === "no-repeat")
      w && (t.save(), m.pathCache ? t.clip(m.pathCache) : (i.nodeShapes[i.getNodeShape(r)].draw(t, o, l, g, p, x, m), t.clip())), i.safeDrawImage(t, e, 0, 0, S, D, M, F, A, k), w && t.restore();
    else {
      var le = t.createPattern(e, v);
      t.fillStyle = le, i.nodeShapes[i.getNodeShape(r)].draw(t, o, l, g, p, x, m), t.translate(M, F), t.fill(), t.translate(-M, -F);
    }
    t.globalAlpha = Q, ae && i.setImgSmoothing(t, se);
  }
};
var Fn = {};
Fn.eleTextBiggerThanMin = function(t, e) {
  if (!e) {
    var r = t.cy().zoom(), n = this.getPixelRatio(), a = Math.ceil(ju(r * n));
    e = Math.pow(2, a);
  }
  var i = t.pstyle("font-size").pfValue * e, s = t.pstyle("min-zoomed-font-size").pfValue;
  return !(i < s);
};
Fn.drawElementText = function(t, e, r, n, a) {
  var i = arguments.length > 5 && arguments[5] !== void 0 ? arguments[5] : !0, s = this;
  if (n == null) {
    if (i && !s.eleTextBiggerThanMin(e))
      return;
  } else if (n === !1)
    return;
  if (e.isNode()) {
    var o = e.pstyle("label");
    if (!o || !o.value)
      return;
    var l = s.getLabelJustification(e), u = e.pstyle("text-metrics").strValue === "glyph";
    t.textAlign = l, t.textBaseline = u ? "alphabetic" : "bottom";
  } else {
    var f = e.element()._private.rscratch.badLine, c = e.pstyle("label"), v = e.pstyle("source-label"), d = e.pstyle("target-label");
    if (f || (!c || !c.value) && (!v || !v.value) && (!d || !d.value))
      return;
    t.textAlign = "center", t.textBaseline = "bottom";
  }
  var h = !r, y;
  r && (y = r, t.translate(-y.x1, -y.y1)), a == null ? (s.drawText(t, e, null, h, i), e.isEdge() && (s.drawText(t, e, "source", h, i), s.drawText(t, e, "target", h, i))) : s.drawText(t, e, a, h, i), r && t.translate(y.x1, y.y1);
};
Fn.getFontCache = function(t) {
  var e;
  this.fontCaches = this.fontCaches || [];
  for (var r = 0; r < this.fontCaches.length; r++)
    if (e = this.fontCaches[r], e.context === t)
      return e;
  return e = {
    context: t
  }, this.fontCaches.push(e), e;
};
Fn.setupTextStyle = function(t, e) {
  var r = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : !0, n = e.pstyle("font-style").strValue, a = e.pstyle("font-size").pfValue + "px", i = e.pstyle("font-family").strValue, s = e.pstyle("font-weight").strValue, o = r ? e.effectiveOpacity() * e.pstyle("text-opacity").value : 1, l = e.pstyle("text-outline-opacity").value * o, u = e.pstyle("color").value, f = e.pstyle("text-outline-color").value;
  t.font = n + " " + s + " " + a + " " + i, t.lineJoin = "round", this.colorFillStyle(t, u[0], u[1], u[2], o), this.colorStrokeStyle(t, f[0], f[1], f[2], l);
};
function aC(t, e, r, n, a) {
  var i = Math.min(n, a), s = i / 2, o = e + n / 2, l = r + a / 2;
  t.beginPath(), t.arc(o, l, s, 0, Math.PI * 2), t.closePath();
}
function md(t, e, r, n, a) {
  var i = arguments.length > 5 && arguments[5] !== void 0 ? arguments[5] : 5, s = Math.min(i, n / 2, a / 2);
  t.beginPath(), t.moveTo(e + s, r), t.lineTo(e + n - s, r), t.quadraticCurveTo(e + n, r, e + n, r + s), t.lineTo(e + n, r + a - s), t.quadraticCurveTo(e + n, r + a, e + n - s, r + a), t.lineTo(e + s, r + a), t.quadraticCurveTo(e, r + a, e, r + a - s), t.lineTo(e, r + s), t.quadraticCurveTo(e, r, e + s, r), t.closePath();
}
Fn.getTextAngle = function(t, e) {
  var r, n = t._private, a = n.rscratch, i = e ? e + "-" : "", s = t.pstyle(i + "text-rotation");
  if (s.strValue === "autorotate") {
    var o = Nt(a, "labelAngle", e);
    r = t.isEdge() ? o : 0;
  } else s.strValue === "none" ? r = 0 : r = s.pfValue;
  return r;
};
Fn.drawText = function(t, e, r) {
  var n = arguments.length > 3 && arguments[3] !== void 0 ? arguments[3] : !0, a = arguments.length > 4 && arguments[4] !== void 0 ? arguments[4] : !0, i = e._private, s = i.rscratch, o = a ? e.effectiveOpacity() : 1;
  if (!(a && (o === 0 || e.pstyle("text-opacity").value === 0))) {
    r === "main" && (r = null);
    var l = Nt(s, "labelX", r), u = Nt(s, "labelY", r), f, c, v = this.getLabelText(e, r);
    if (v != null && v !== "" && !isNaN(l) && !isNaN(u)) {
      this.setupTextStyle(t, e, a);
      var d = r ? r + "-" : "", h = Nt(s, "labelWidth", r), y = Nt(s, "labelHeight", r), g = Nt(s, "labelActualDescent", r), p = e.pstyle(d + "text-margin-x").pfValue, m = e.pstyle(d + "text-margin-y").pfValue, b = e.isEdge(), w = e.pstyle("text-halign").value, E = e.pstyle("text-valign").value;
      b && (w = "center", E = "center"), l += p, u += m;
      var T;
      n ? T = this.getTextAngle(e, r) : T = 0, T !== 0 && (f = l, c = u, t.translate(f, c), t.rotate(T), l = 0, u = 0);
      var x = ha(w), S = ga(E);
      switch (S) {
        case "top":
          break;
        case "center":
          u += y / 2;
          break;
        case "bottom":
          u += y;
          break;
      }
      var D = e.pstyle("text-background-opacity").value, A = e.pstyle("text-border-opacity").value, k = e.pstyle("text-border-width").pfValue, R = e.pstyle("text-background-padding").pfValue, M = e.pstyle("text-background-shape").strValue, I = M === "round-rectangle" || M === "roundrectangle", _ = M === "circle", N = 2;
      if (D > 0 || k > 0 && A > 0) {
        var L = t.fillStyle, F = t.strokeStyle, H = t.lineWidth, V = e.pstyle("text-background-color").value, O = e.pstyle("text-border-color").value, $ = e.pstyle("text-border-style").value, Q = D > 0, se = k > 0 && A > 0, ae = l - R;
        switch (x) {
          case "left":
            ae -= h;
            break;
          case "center":
            ae -= h / 2;
            break;
        }
        var le = u - y - R, ce = h + 2 * R, he = y + 2 * R;
        if (Q && (t.fillStyle = "rgba(".concat(V[0], ",").concat(V[1], ",").concat(V[2], ",").concat(D * o, ")")), se && (t.strokeStyle = "rgba(".concat(O[0], ",").concat(O[1], ",").concat(O[2], ",").concat(A * o, ")"), t.lineWidth = k, t.setLineDash))
          switch ($) {
            case "dotted":
              t.setLineDash([1, 1]);
              break;
            case "dashed":
              t.setLineDash([4, 2]);
              break;
            case "double":
              t.lineWidth = k / 4, t.setLineDash([]);
              break;
            case "solid":
            default:
              t.setLineDash([]);
              break;
          }
        if (I ? (t.beginPath(), md(t, ae, le, ce, he, N)) : _ ? (t.beginPath(), aC(t, ae, le, ce, he)) : (t.beginPath(), t.rect(ae, le, ce, he)), Q && t.fill(), se && t.stroke(), se && $ === "double") {
          var ie = k / 2;
          t.beginPath(), I ? md(t, ae + ie, le + ie, ce - 2 * ie, he - 2 * ie, N) : t.rect(ae + ie, le + ie, ce - 2 * ie, he - 2 * ie), t.stroke();
        }
        t.fillStyle = L, t.strokeStyle = F, t.lineWidth = H, t.setLineDash && t.setLineDash([]);
      }
      var U = 2 * e.pstyle("text-outline-width").pfValue;
      if (U > 0 && (t.lineWidth = U), u -= g, e.pstyle("text-wrap").value === "wrap") {
        var X = Nt(s, "labelWrapCachedLines", r), C = Nt(s, "labelLineHeight", r), B = h / 2, z = this.getLabelJustification(e);
        switch (z === "auto" || (x === "left" ? z === "left" ? l += -h : z === "center" && (l += -B) : x === "center" ? z === "left" ? l += -B : z === "right" && (l += B) : x === "right" && (z === "center" ? l += B : z === "right" && (l += h))), S) {
          case "top":
            u -= (X.length - 1) * C;
            break;
          case "center":
          case "bottom":
            u -= (X.length - 1) * C;
            break;
        }
        for (var W = 0; W < X.length; W++)
          U > 0 && t.strokeText(X[W], l, u), t.fillText(X[W], l, u), u += C;
      } else
        U > 0 && t.strokeText(v, l, u), t.fillText(v, l, u);
      T !== 0 && (t.rotate(-T), t.translate(-f, -c));
    }
  }
};
var dn = {};
dn.drawNode = function(t, e, r) {
  var n = arguments.length > 3 && arguments[3] !== void 0 ? arguments[3] : !0, a = arguments.length > 4 && arguments[4] !== void 0 ? arguments[4] : !0, i = arguments.length > 5 && arguments[5] !== void 0 ? arguments[5] : !0, s = this, o, l, u = e._private, f = u.rscratch, c = e.position();
  if (!(!pe(c.x) || !pe(c.y)) && !(i && !e.visible())) {
    var v = i ? e.effectiveOpacity() : 1, d = s.usePaths(), h, y = !1, g = e.padding();
    o = e.width() + 2 * g, l = e.height() + 2 * g;
    var p;
    r && (p = r, t.translate(-p.x1, -p.y1));
    for (var m = e.pstyle("background-image"), b = m.value, w = new Array(b.length), E = new Array(b.length), T = 0, x = 0; x < b.length; x++) {
      var S = b[x], D = w[x] = S != null && S !== "none";
      if (D) {
        var A = e.cy().style().getIndexedStyle(e, "background-image-crossorigin", "value", x);
        T++, E[x] = s.getCachedImage(S, A, function() {
          u.backgroundTimestamp = Date.now(), e.emitAndNotify("background");
        });
      }
    }
    var k = e.pstyle("background-blacken").value, R = e.pstyle("border-width").pfValue, M = e.pstyle("background-opacity").value * v, I = e.pstyle("border-color").value, _ = e.pstyle("border-style").value, N = e.pstyle("border-join").value, L = e.pstyle("border-cap").value, F = e.pstyle("border-position").value, H = e.pstyle("border-dash-pattern").pfValue, V = e.pstyle("border-dash-offset").pfValue, O = e.pstyle("border-opacity").value * v, $ = e.pstyle("outline-width").pfValue, Q = e.pstyle("outline-color").value, se = e.pstyle("outline-style").value, ae = e.pstyle("outline-opacity").value * v, le = e.pstyle("outline-offset").value, ce = e.pstyle("corner-radius").value;
    ce !== "auto" && (ce = e.pstyle("corner-radius").pfValue);
    var he = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : M;
      s.eleFillStyle(t, e, P);
    }, ie = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : O;
      s.colorStrokeStyle(t, I[0], I[1], I[2], P);
    }, U = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : ae;
      s.colorStrokeStyle(t, Q[0], Q[1], Q[2], P);
    }, X = function(P, q, G, re) {
      var ee = s.nodePathCache = s.nodePathCache || [], ye = Qh(G === "polygon" ? G + "," + re.join(",") : G, "" + q, "" + P, "" + ce), fe = ee[ye], ge, be = !1;
      return fe != null ? (ge = fe, be = !0, f.pathCache = ge) : (ge = new Path2D(), ee[ye] = f.pathCache = ge), {
        path: ge,
        cacheHit: be
      };
    }, C = e.pstyle("shape").strValue, B = e.pstyle("shape-polygon-points").pfValue;
    if (d) {
      t.translate(c.x, c.y);
      var z = X(o, l, C, B);
      h = z.path, y = z.cacheHit;
    }
    var W = function() {
      if (!y) {
        var P = c;
        d && (P = {
          x: 0,
          y: 0
        }), s.nodeShapes[s.getNodeShape(e)].draw(h || t, P.x, P.y, o, l, ce, f);
      }
      d ? t.fill(h) : t.fill();
    }, j = function() {
      for (var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : v, q = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0, G = u.backgrounding, re = 0, ee = 0; ee < E.length; ee++) {
        var ye = e.cy().style().getIndexedStyle(e, "background-image-containment", "value", ee);
        if (q && ye === "over" || !q && ye === "inside") {
          re++;
          continue;
        }
        w[ee] && E[ee].complete && !E[ee].error && (re++, s.drawInscribedImage(t, E[ee], e, ee, P));
      }
      u.backgrounding = re !== T, G !== u.backgrounding && e.updateStyle(!1);
    }, Z = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : !1, q = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : v;
      s.hasPie(e) && (s.drawPie(t, e, q), P && (d || s.nodeShapes[s.getNodeShape(e)].draw(t, c.x, c.y, o, l, ce, f)));
    }, ne = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : !1, q = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : v;
      s.hasStripe(e) && (t.save(), d ? t.clip(f.pathCache) : (s.nodeShapes[s.getNodeShape(e)].draw(t, c.x, c.y, o, l, ce, f), t.clip()), s.drawStripe(t, e, q), t.restore(), P && (d || s.nodeShapes[s.getNodeShape(e)].draw(t, c.x, c.y, o, l, ce, f)));
    }, te = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : v, q = (k > 0 ? k : -k) * P, G = k > 0 ? 0 : 255;
      k !== 0 && (s.colorFillStyle(t, G, G, G, q), d ? t.fill(h) : t.fill());
    }, Y = function() {
      if (R > 0) {
        if (t.lineWidth = R, t.lineCap = L, t.lineJoin = N, t.setLineDash)
          switch (_) {
            case "dotted":
              t.setLineDash([1, 1]);
              break;
            case "dashed":
              t.setLineDash(H), t.lineDashOffset = V;
              break;
            case "solid":
            case "double":
              t.setLineDash([]);
              break;
          }
        if (F !== "center") {
          if (t.save(), t.lineWidth *= 2, F === "inside")
            d ? t.clip(h) : t.clip();
          else {
            var P = new Path2D();
            P.rect(-o / 2 - R, -l / 2 - R, o + 2 * R, l + 2 * R), P.addPath(h), t.clip(P, "evenodd");
          }
          d ? t.stroke(h) : t.stroke(), t.restore();
        } else
          d ? t.stroke(h) : t.stroke();
        if (_ === "double") {
          t.lineWidth = R / 3;
          var q = t.globalCompositeOperation;
          t.globalCompositeOperation = "destination-out", d ? t.stroke(h) : t.stroke(), t.globalCompositeOperation = q;
        }
        t.setLineDash && t.setLineDash([]);
      }
    }, K = function() {
      if ($ > 0) {
        if (t.lineWidth = $, t.lineCap = "butt", t.setLineDash)
          switch (se) {
            case "dotted":
              t.setLineDash([1, 1]);
              break;
            case "dashed":
              t.setLineDash([4, 2]);
              break;
            case "solid":
            case "double":
              t.setLineDash([]);
              break;
          }
        var P = c;
        d && (P = {
          x: 0,
          y: 0
        });
        var q = s.getNodeShape(e), G = R;
        F === "inside" && (G = 0), F === "outside" && (G *= 2);
        var re = (o + G + ($ + le)) / o, ee = (l + G + ($ + le)) / l, ye = o * re, fe = l * ee, ge = s.nodeShapes[q].points, be;
        if (d) {
          var De = X(ye, fe, q, ge);
          be = De.path;
        }
        if (q === "ellipse")
          s.drawEllipsePath(be || t, P.x, P.y, ye, fe);
        else if (["round-diamond", "round-heptagon", "round-hexagon", "round-octagon", "round-pentagon", "round-polygon", "round-triangle", "round-tag"].includes(q)) {
          var Be = 0, Me = 0, xe = 0;
          q === "round-diamond" ? Be = (G + le + $) * 1.4 : q === "round-heptagon" ? (Be = (G + le + $) * 1.075, xe = -(G / 2 + le + $) / 35) : q === "round-hexagon" ? Be = (G + le + $) * 1.12 : q === "round-pentagon" ? (Be = (G + le + $) * 1.13, xe = -(G / 2 + le + $) / 15) : q === "round-tag" ? (Be = (G + le + $) * 1.12, Me = (G / 2 + $ + le) * 0.07) : q === "round-triangle" && (Be = (G + le + $) * (Math.PI / 2), xe = -(G + le / 2 + $) / Math.PI), Be !== 0 && (re = (o + Be) / o, ye = o * re, ["round-hexagon", "round-tag"].includes(q) || (ee = (l + Be) / l, fe = l * ee)), ce = ce === "auto" ? og(ye, fe) : ce;
          for (var Ie = ye / 2, Oe = fe / 2, gt = ce + (G + $ + le) / 2, rt = new Array(ge.length / 2), tr = new Array(ge.length / 2), nt = 0; nt < ge.length / 2; nt++)
            rt[nt] = {
              x: P.x + Me + Ie * ge[nt * 2],
              y: P.y + xe + Oe * ge[nt * 2 + 1]
            };
          var st, ft, wt, cr, Je = rt.length;
          for (ft = rt[Je - 1], st = 0; st < Je; st++)
            wt = rt[st % Je], cr = rt[(st + 1) % Je], tr[st] = df(ft, wt, cr, gt), ft = wt, wt = cr;
          s.drawRoundPolygonPath(be || t, P.x + Me, P.y + xe, o * re, l * ee, ge, tr);
        } else if (["roundrectangle", "round-rectangle"].includes(q))
          ce = ce === "auto" ? sn(ye, fe) : ce, s.drawRoundRectanglePath(be || t, P.x, P.y, ye, fe, ce + (G + $ + le) / 2);
        else if (["cutrectangle", "cut-rectangle"].includes(q))
          ce = ce === "auto" ? tf() : ce, s.drawCutRectanglePath(be || t, P.x, P.y, ye, fe, null, ce + (G + $ + le) / 4);
        else if (["bottomroundrectangle", "bottom-round-rectangle"].includes(q))
          ce = ce === "auto" ? sn(ye, fe) : ce, s.drawBottomRoundRectanglePath(be || t, P.x, P.y, ye, fe, ce + (G + $ + le) / 2);
        else if (q === "barrel")
          s.drawBarrelPath(be || t, P.x, P.y, ye, fe);
        else if (q.startsWith("polygon") || ["rhomboid", "right-rhomboid", "round-tag", "tag", "vee"].includes(q)) {
          var Dr = (G + $ + le) / o;
          ge = xs(Es(ge, Dr)), s.drawPolygonPath(be || t, P.x, P.y, o, l, ge);
        } else {
          var rr = (G + $ + le) / o;
          ge = xs(Es(ge, -rr)), s.drawPolygonPath(be || t, P.x, P.y, o, l, ge);
        }
        if (d ? t.stroke(be) : t.stroke(), se === "double") {
          t.lineWidth = G / 3;
          var Wr = t.globalCompositeOperation;
          t.globalCompositeOperation = "destination-out", d ? t.stroke(be) : t.stroke(), t.globalCompositeOperation = Wr;
        }
        t.setLineDash && t.setLineDash([]);
      }
    }, ue = function() {
      a && s.drawNodeOverlay(t, e, c, o, l);
    }, oe = function() {
      a && s.drawNodeUnderlay(t, e, c, o, l);
    }, ve = function() {
      s.drawElementText(t, e, null, n);
    }, de = e.pstyle("ghost").value === "yes";
    if (de) {
      var me = e.pstyle("ghost-offset-x").pfValue, Te = e.pstyle("ghost-offset-y").pfValue, Ee = e.pstyle("ghost-opacity").value, Pe = Ee * v;
      t.translate(me, Te), U(), K(), he(Ee * M), W(), j(Pe, !0), ie(Ee * O), Y(), Z(k !== 0 || R !== 0), ne(k !== 0 || R !== 0), j(Pe, !1), te(Pe), t.translate(-me, -Te);
    }
    d && t.translate(-c.x, -c.y), oe(), d && t.translate(c.x, c.y), U(), K(), he(), W(), j(v, !0), ie(), Y(), Z(k !== 0 || R !== 0), ne(k !== 0 || R !== 0), j(v, !1), te(), d && t.translate(-c.x, -c.y), ve(), ue(), r && t.translate(p.x1, p.y1);
  }
};
var hp = function(e) {
  if (!["overlay", "underlay"].includes(e))
    throw new Error("Invalid state");
  return function(r, n, a, i, s) {
    var o = this;
    if (n.visible()) {
      var l = n.pstyle("".concat(e, "-padding")).pfValue, u = n.pstyle("".concat(e, "-opacity")).value, f = n.pstyle("".concat(e, "-color")).value, c = n.pstyle("".concat(e, "-shape")).value, v = n.pstyle("".concat(e, "-corner-radius")).value;
      if (u > 0) {
        if (a = a || n.position(), i == null || s == null) {
          var d = n.padding();
          i = n.width() + 2 * d, s = n.height() + 2 * d;
        }
        o.colorFillStyle(r, f[0], f[1], f[2], u), o.nodeShapes[c].draw(r, a.x, a.y, i + l * 2, s + l * 2, v), r.fill();
      }
    }
  };
};
dn.drawNodeOverlay = hp("overlay");
dn.drawNodeUnderlay = hp("underlay");
dn.hasPie = function(t) {
  return t = t[0], t._private.hasPie;
};
dn.hasStripe = function(t) {
  return t = t[0], t._private.hasStripe;
};
dn.drawPie = function(t, e, r, n) {
  e = e[0], n = n || e.position();
  var a = e.cy().style(), i = e.pstyle("pie-size"), s = e.pstyle("pie-hole"), o = e.pstyle("pie-start-angle").pfValue, l = n.x, u = n.y, f = e.width(), c = e.height(), v = Math.min(f, c) / 2, d, h = 0, y = this.usePaths();
  if (y && (l = 0, u = 0), i.units === "%" ? v = v * i.pfValue : i.pfValue !== void 0 && (v = i.pfValue / 2), s.units === "%" ? d = v * s.pfValue : s.pfValue !== void 0 && (d = s.pfValue / 2), !(d >= v))
    for (var g = 1; g <= a.pieBackgroundN; g++) {
      var p = e.pstyle("pie-" + g + "-background-size").value, m = e.pstyle("pie-" + g + "-background-color").value, b = e.pstyle("pie-" + g + "-background-opacity").value * r, w = p / 100;
      w + h > 1 && (w = 1 - h);
      var E = 1.5 * Math.PI + 2 * Math.PI * h;
      E += o;
      var T = 2 * Math.PI * w, x = E + T;
      p === 0 || h >= 1 || h + w > 1 || (d === 0 ? (t.beginPath(), t.moveTo(l, u), t.arc(l, u, v, E, x), t.closePath()) : (t.beginPath(), t.arc(l, u, v, E, x), t.arc(l, u, d, x, E, !0), t.closePath()), this.colorFillStyle(t, m[0], m[1], m[2], b), t.fill(), h += w);
    }
};
dn.drawStripe = function(t, e, r, n) {
  e = e[0], n = n || e.position();
  var a = e.cy().style(), i = n.x, s = n.y, o = e.width(), l = e.height(), u = 0, f = this.usePaths();
  t.save();
  var c = e.pstyle("stripe-direction").value, v = e.pstyle("stripe-size");
  switch (c) {
    case "vertical":
      break;
    // default
    case "righward":
      t.rotate(-Math.PI / 2);
      break;
  }
  var d = o, h = l;
  v.units === "%" ? (d = d * v.pfValue, h = h * v.pfValue) : v.pfValue !== void 0 && (d = v.pfValue, h = v.pfValue), f && (i = 0, s = 0), s -= d / 2, i -= h / 2;
  for (var y = 1; y <= a.stripeBackgroundN; y++) {
    var g = e.pstyle("stripe-" + y + "-background-size").value, p = e.pstyle("stripe-" + y + "-background-color").value, m = e.pstyle("stripe-" + y + "-background-opacity").value * r, b = g / 100;
    b + u > 1 && (b = 1 - u), !(g === 0 || u >= 1 || u + b > 1) && (t.beginPath(), t.rect(i, s + h * u, d, h * b), t.closePath(), this.colorFillStyle(t, p[0], p[1], p[2], m), t.fill(), u += b);
  }
  t.restore();
};
var Vt = {}, iC = 100;
Vt.getPixelRatio = function() {
  var t = this.data.contexts[0];
  if (this.forcedPixelRatio != null)
    return this.forcedPixelRatio;
  var e = this.cy.window(), r = t.backingStorePixelRatio || t.webkitBackingStorePixelRatio || t.mozBackingStorePixelRatio || t.msBackingStorePixelRatio || t.oBackingStorePixelRatio || t.backingStorePixelRatio || 1;
  return (e.devicePixelRatio || 1) / r;
};
Vt.paintCache = function(t) {
  for (var e = this.paintCaches = this.paintCaches || [], r = !0, n, a = 0; a < e.length; a++)
    if (n = e[a], n.context === t) {
      r = !1;
      break;
    }
  return r && (n = {
    context: t
  }, e.push(n)), n;
};
Vt.createGradientStyleFor = function(t, e, r, n, a) {
  var i, s = this.usePaths(), o = r.pstyle(e + "-gradient-stop-colors").value, l = r.pstyle(e + "-gradient-stop-positions").pfValue;
  if (n === "radial-gradient")
    if (r.isEdge()) {
      var u = r.sourceEndpoint(), f = r.targetEndpoint(), c = r.midpoint(), v = Rn(u, c), d = Rn(f, c);
      i = t.createRadialGradient(c.x, c.y, 0, c.x, c.y, Math.max(v, d));
    } else {
      var h = s ? {
        x: 0,
        y: 0
      } : r.position(), y = r.paddedWidth(), g = r.paddedHeight();
      i = t.createRadialGradient(h.x, h.y, 0, h.x, h.y, Math.max(y, g));
    }
  else if (r.isEdge()) {
    var p = r.sourceEndpoint(), m = r.targetEndpoint();
    i = t.createLinearGradient(p.x, p.y, m.x, m.y);
  } else {
    var b = s ? {
      x: 0,
      y: 0
    } : r.position(), w = r.paddedWidth(), E = r.paddedHeight(), T = w / 2, x = E / 2, S = r.pstyle("background-gradient-direction").value;
    switch (S) {
      case "to-bottom":
        i = t.createLinearGradient(b.x, b.y - x, b.x, b.y + x);
        break;
      case "to-top":
        i = t.createLinearGradient(b.x, b.y + x, b.x, b.y - x);
        break;
      case "to-left":
        i = t.createLinearGradient(b.x + T, b.y, b.x - T, b.y);
        break;
      case "to-right":
        i = t.createLinearGradient(b.x - T, b.y, b.x + T, b.y);
        break;
      case "to-bottom-right":
      case "to-right-bottom":
        i = t.createLinearGradient(b.x - T, b.y - x, b.x + T, b.y + x);
        break;
      case "to-top-right":
      case "to-right-top":
        i = t.createLinearGradient(b.x - T, b.y + x, b.x + T, b.y - x);
        break;
      case "to-bottom-left":
      case "to-left-bottom":
        i = t.createLinearGradient(b.x + T, b.y - x, b.x - T, b.y + x);
        break;
      case "to-top-left":
      case "to-left-top":
        i = t.createLinearGradient(b.x + T, b.y + x, b.x - T, b.y - x);
        break;
    }
  }
  if (!i) return null;
  for (var D = l.length === o.length, A = o.length, k = 0; k < A; k++)
    i.addColorStop(D ? l[k] : k / (A - 1), "rgba(" + o[k][0] + "," + o[k][1] + "," + o[k][2] + "," + a + ")");
  return i;
};
Vt.gradientFillStyle = function(t, e, r, n) {
  var a = this.createGradientStyleFor(t, "background", e, r, n);
  if (!a) return null;
  t.fillStyle = a;
};
Vt.colorFillStyle = function(t, e, r, n, a) {
  t.fillStyle = "rgba(" + e + "," + r + "," + n + "," + a + ")";
};
Vt.eleFillStyle = function(t, e, r) {
  var n = e.pstyle("background-fill").value;
  if (n === "linear-gradient" || n === "radial-gradient")
    this.gradientFillStyle(t, e, n, r);
  else {
    var a = e.pstyle("background-color").value;
    this.colorFillStyle(t, a[0], a[1], a[2], r);
  }
};
Vt.gradientStrokeStyle = function(t, e, r, n) {
  var a = this.createGradientStyleFor(t, "line", e, r, n);
  if (!a) return null;
  t.strokeStyle = a;
};
Vt.colorStrokeStyle = function(t, e, r, n, a) {
  t.strokeStyle = "rgba(" + e + "," + r + "," + n + "," + a + ")";
};
Vt.eleStrokeStyle = function(t, e, r) {
  var n = e.pstyle("line-fill").value;
  if (n === "linear-gradient" || n === "radial-gradient")
    this.gradientStrokeStyle(t, e, n, r);
  else {
    var a = e.pstyle("line-color").value;
    this.colorStrokeStyle(t, a[0], a[1], a[2], r);
  }
};
Vt.matchCanvasSize = function(t) {
  var e = this, r = e.data, n = e.findContainerClientCoords(), a = n[2], i = n[3], s = e.getPixelRatio(), o = e.motionBlurPxRatio;
  (t === e.data.bufferCanvases[e.MOTIONBLUR_BUFFER_NODE] || t === e.data.bufferCanvases[e.MOTIONBLUR_BUFFER_DRAG]) && (s = o);
  var l = a * s, u = i * s, f;
  if (!(l === e.canvasWidth && u === e.canvasHeight)) {
    e.fontCaches = null;
    var c = r.canvasContainer;
    c.style.width = a + "px", c.style.height = i + "px";
    for (var v = 0; v < e.CANVAS_LAYERS; v++)
      f = r.canvases[v], f.width = l, f.height = u, f.style.width = a + "px", f.style.height = i + "px";
    for (var v = 0; v < e.BUFFER_COUNT; v++)
      f = r.bufferCanvases[v], f.width = l, f.height = u, f.style.width = a + "px", f.style.height = i + "px";
    e.textureMult = 1, s <= 1 && (f = r.bufferCanvases[e.TEXTURE_BUFFER], e.textureMult = 2, f.width = l * e.textureMult, f.height = u * e.textureMult), e.canvasWidth = l, e.canvasHeight = u, e.pixelRatio = s;
  }
};
Vt.renderTo = function(t, e, r, n) {
  this.render({
    forcedContext: t,
    forcedZoom: e,
    forcedPan: r,
    drawAllLayers: !0,
    forcedPxRatio: n
  });
};
Vt.clearCanvas = function() {
  var t = this, e = t.data;
  function r(n) {
    n.clearRect(0, 0, t.canvasWidth, t.canvasHeight);
  }
  r(e.contexts[t.NODE]), r(e.contexts[t.DRAG]);
};
Vt.render = function(t) {
  var e = this;
  t = t || tg();
  var r = e.cy, n = t.forcedContext, a = t.drawAllLayers, i = t.drawOnlyNodeLayer, s = t.forcedZoom, o = t.forcedPan, l = t.forcedPxRatio === void 0 ? this.getPixelRatio() : t.forcedPxRatio, u = e.data, f = u.canvasNeedsRedraw, c = e.textureOnViewport && !n && (e.pinching || e.hoverData.dragging || e.swipePanning || e.data.wheelZooming), v = t.motionBlur !== void 0 ? t.motionBlur : e.motionBlur, d = e.motionBlurPxRatio, h = r.hasCompoundNodes(), y = e.hoverData.draggingEles, g = !!(e.hoverData.selecting || e.touchData.selecting);
  v = v && !n && e.motionBlurEnabled && !g;
  var p = v;
  n || (e.prevPxRatio !== l && (e.invalidateContainerClientCoordsCache(), e.matchCanvasSize(e.container), e.redrawHint("eles", !0), e.redrawHint("drag", !0)), e.prevPxRatio = l), !n && e.motionBlurTimeout && clearTimeout(e.motionBlurTimeout), v && (e.mbFrames == null && (e.mbFrames = 0), e.mbFrames++, e.mbFrames < 3 && (p = !1), e.mbFrames > e.minMbLowQualFrames && (e.motionBlurPxRatio = e.mbPxRBlurry)), e.clearingMotionBlur && (e.motionBlurPxRatio = 1), e.textureDrawLastFrame && !c && (f[e.NODE] = !0, f[e.SELECT_BOX] = !0);
  var m = r.style(), b = r.zoom(), w = s !== void 0 ? s : b, E = r.pan(), T = {
    x: E.x,
    y: E.y
  }, x = {
    zoom: b,
    pan: {
      x: E.x,
      y: E.y
    }
  }, S = e.prevViewport, D = S === void 0 || x.zoom !== S.zoom || x.pan.x !== S.pan.x || x.pan.y !== S.pan.y;
  !D && !(y && !h) && (e.motionBlurPxRatio = 1), o && (T = o), w *= l, T.x *= l, T.y *= l;
  var A = e.getCachedZSortedEles();
  function k(ie, U, X, C, B) {
    var z = ie.globalCompositeOperation;
    ie.globalCompositeOperation = "destination-out", e.colorFillStyle(ie, 255, 255, 255, e.motionBlurTransparency), ie.fillRect(U, X, C, B), ie.globalCompositeOperation = z;
  }
  function R(ie, U) {
    var X, C, B, z;
    !e.clearingMotionBlur && (ie === u.bufferContexts[e.MOTIONBLUR_BUFFER_NODE] || ie === u.bufferContexts[e.MOTIONBLUR_BUFFER_DRAG]) ? (X = {
      x: E.x * d,
      y: E.y * d
    }, C = b * d, B = e.canvasWidth * d, z = e.canvasHeight * d) : (X = T, C = w, B = e.canvasWidth, z = e.canvasHeight), ie.setTransform(1, 0, 0, 1, 0, 0), U === "motionBlur" ? k(ie, 0, 0, B, z) : !n && (U === void 0 || U) && ie.clearRect(0, 0, B, z), a || (ie.translate(X.x, X.y), ie.scale(C, C)), o && ie.translate(o.x, o.y), s && ie.scale(s, s);
  }
  if (c || (e.textureDrawLastFrame = !1), c) {
    if (e.textureDrawLastFrame = !0, !e.textureCache) {
      e.textureCache = {}, e.textureCache.bb = r.mutableElements().boundingBox(), e.textureCache.texture = e.data.bufferCanvases[e.TEXTURE_BUFFER];
      var M = e.data.bufferContexts[e.TEXTURE_BUFFER];
      M.setTransform(1, 0, 0, 1, 0, 0), M.clearRect(0, 0, e.canvasWidth * e.textureMult, e.canvasHeight * e.textureMult), e.render({
        forcedContext: M,
        drawOnlyNodeLayer: !0,
        forcedPxRatio: l * e.textureMult
      });
      var x = e.textureCache.viewport = {
        zoom: r.zoom(),
        pan: r.pan(),
        width: e.canvasWidth,
        height: e.canvasHeight
      };
      x.mpan = {
        x: (0 - x.pan.x) / x.zoom,
        y: (0 - x.pan.y) / x.zoom
      };
    }
    f[e.DRAG] = !1, f[e.NODE] = !1;
    var I = u.contexts[e.NODE], _ = e.textureCache.texture, x = e.textureCache.viewport;
    I.setTransform(1, 0, 0, 1, 0, 0), v ? k(I, 0, 0, x.width, x.height) : I.clearRect(0, 0, x.width, x.height);
    var N = m.core("outside-texture-bg-color").value, L = m.core("outside-texture-bg-opacity").value;
    e.colorFillStyle(I, N[0], N[1], N[2], L), I.fillRect(0, 0, x.width, x.height);
    var b = r.zoom();
    R(I, !1), I.clearRect(x.mpan.x, x.mpan.y, x.width / x.zoom / l, x.height / x.zoom / l), I.drawImage(_, x.mpan.x, x.mpan.y, x.width / x.zoom / l, x.height / x.zoom / l);
  } else e.textureOnViewport && !n && (e.textureCache = null);
  var F = r.extent(), H = e.pinching || e.hoverData.dragging || e.swipePanning || e.data.wheelZooming || e.hoverData.draggingEles || e.cy.animated(), V = e.hideEdgesOnViewport && H, O = [];
  if (O[e.NODE] = !f[e.NODE] && v && !e.clearedForMotionBlur[e.NODE] || e.clearingMotionBlur, O[e.NODE] && (e.clearedForMotionBlur[e.NODE] = !0), O[e.DRAG] = !f[e.DRAG] && v && !e.clearedForMotionBlur[e.DRAG] || e.clearingMotionBlur, O[e.DRAG] && (e.clearedForMotionBlur[e.DRAG] = !0), f[e.NODE] || a || i || O[e.NODE]) {
    var $ = v && !O[e.NODE] && d !== 1, I = n || ($ ? e.data.bufferContexts[e.MOTIONBLUR_BUFFER_NODE] : u.contexts[e.NODE]), Q = v && !$ ? "motionBlur" : void 0;
    R(I, Q), V ? e.drawCachedNodes(I, A.nondrag, l, F) : e.drawLayeredElements(I, A.nondrag, l, F), e.debug && e.drawDebugPoints(I, A.nondrag), !a && !v && (f[e.NODE] = !1);
  }
  if (!i && (f[e.DRAG] || a || O[e.DRAG])) {
    var $ = v && !O[e.DRAG] && d !== 1, I = n || ($ ? e.data.bufferContexts[e.MOTIONBLUR_BUFFER_DRAG] : u.contexts[e.DRAG]);
    R(I, v && !$ ? "motionBlur" : void 0), V ? e.drawCachedNodes(I, A.drag, l, F) : e.drawCachedElements(I, A.drag, l, F), e.debug && e.drawDebugPoints(I, A.drag), !a && !v && (f[e.DRAG] = !1);
  }
  if (this.drawSelectionRectangle(t, R), v && d !== 1) {
    var se = u.contexts[e.NODE], ae = e.data.bufferCanvases[e.MOTIONBLUR_BUFFER_NODE], le = u.contexts[e.DRAG], ce = e.data.bufferCanvases[e.MOTIONBLUR_BUFFER_DRAG], he = function(U, X, C) {
      U.setTransform(1, 0, 0, 1, 0, 0), C || !p ? U.clearRect(0, 0, e.canvasWidth, e.canvasHeight) : k(U, 0, 0, e.canvasWidth, e.canvasHeight);
      var B = d;
      U.drawImage(
        X,
        // img
        0,
        0,
        // sx, sy
        e.canvasWidth * B,
        e.canvasHeight * B,
        // sw, sh
        0,
        0,
        // x, y
        e.canvasWidth,
        e.canvasHeight
        // w, h
      );
    };
    (f[e.NODE] || O[e.NODE]) && (he(se, ae, O[e.NODE]), f[e.NODE] = !1), (f[e.DRAG] || O[e.DRAG]) && (he(le, ce, O[e.DRAG]), f[e.DRAG] = !1);
  }
  e.prevViewport = x, e.clearingMotionBlur && (e.clearingMotionBlur = !1, e.motionBlurCleared = !0, e.motionBlur = !0), v && (e.motionBlurTimeout = setTimeout(function() {
    e.motionBlurTimeout = null, e.clearedForMotionBlur[e.NODE] = !1, e.clearedForMotionBlur[e.DRAG] = !1, e.motionBlur = !1, e.clearingMotionBlur = !c, e.mbFrames = 0, f[e.NODE] = !0, f[e.DRAG] = !0, e.redraw();
  }, iC)), n || r.emit("render");
};
var Ma;
Vt.drawSelectionRectangle = function(t, e) {
  var r = this, n = r.cy, a = r.data, i = n.style(), s = t.drawOnlyNodeLayer, o = t.drawAllLayers, l = a.canvasNeedsRedraw, u = t.forcedContext;
  if (r.showFps || !s && l[r.SELECT_BOX] && !o) {
    var f = u || a.contexts[r.SELECT_BOX];
    if (e(f), r.selection[4] == 1 && (r.hoverData.selecting || r.touchData.selecting)) {
      var c = r.cy.zoom(), v = i.core("selection-box-border-width").value / c;
      f.lineWidth = v, f.fillStyle = "rgba(" + i.core("selection-box-color").value[0] + "," + i.core("selection-box-color").value[1] + "," + i.core("selection-box-color").value[2] + "," + i.core("selection-box-opacity").value + ")", f.fillRect(r.selection[0], r.selection[1], r.selection[2] - r.selection[0], r.selection[3] - r.selection[1]), v > 0 && (f.strokeStyle = "rgba(" + i.core("selection-box-border-color").value[0] + "," + i.core("selection-box-border-color").value[1] + "," + i.core("selection-box-border-color").value[2] + "," + i.core("selection-box-opacity").value + ")", f.strokeRect(r.selection[0], r.selection[1], r.selection[2] - r.selection[0], r.selection[3] - r.selection[1]));
    }
    if (a.bgActivePosistion && !r.hoverData.selecting) {
      var c = r.cy.zoom(), d = a.bgActivePosistion;
      f.fillStyle = "rgba(" + i.core("active-bg-color").value[0] + "," + i.core("active-bg-color").value[1] + "," + i.core("active-bg-color").value[2] + "," + i.core("active-bg-opacity").value + ")", f.beginPath(), f.arc(d.x, d.y, i.core("active-bg-size").pfValue / c, 0, 2 * Math.PI), f.fill();
    }
    var h = r.lastRedrawTime;
    if (r.showFps && h) {
      h = Math.round(h);
      var y = Math.round(1e3 / h), g = "1 frame = " + h + " ms = " + y + " fps";
      if (f.setTransform(1, 0, 0, 1, 0, 0), f.fillStyle = "rgba(255, 0, 0, 0.75)", f.strokeStyle = "rgba(255, 0, 0, 0.75)", f.font = "30px Arial", !Ma) {
        var p = f.measureText(g);
        Ma = p.actualBoundingBoxAscent;
      }
      f.fillText(g, 0, Ma);
      var m = 60;
      f.strokeRect(0, Ma + 10, 250, 20), f.fillRect(0, Ma + 10, 250 * Math.min(y / m, 1), 20);
    }
    o || (l[r.SELECT_BOX] = !1);
  }
};
function bd(t, e, r) {
  var n = t.createShader(e);
  if (t.shaderSource(n, r), t.compileShader(n), !t.getShaderParameter(n, t.COMPILE_STATUS))
    throw new Error(t.getShaderInfoLog(n));
  return n;
}
function sC(t, e, r) {
  var n = bd(t, t.VERTEX_SHADER, e), a = bd(t, t.FRAGMENT_SHADER, r), i = t.createProgram();
  if (t.attachShader(i, n), t.attachShader(i, a), t.linkProgram(i), !t.getProgramParameter(i, t.LINK_STATUS))
    throw new Error("Could not initialize shaders");
  return i;
}
function oC(t, e, r) {
  r === void 0 && (r = e);
  var n = t.makeOffscreenCanvas(e, r), a = n.context = n.getContext("2d");
  return n.clear = function() {
    return a.clearRect(0, 0, n.width, n.height);
  }, n.clear(), n;
}
function pf(t) {
  var e = t.pixelRatio, r = t.cy.zoom(), n = t.cy.pan();
  return {
    zoom: r * e,
    pan: {
      x: n.x * e,
      y: n.y * e
    }
  };
}
function lC(t) {
  var e = t.pixelRatio, r = t.cy.zoom();
  return r * e;
}
function uC(t, e, r, n, a) {
  var i = n * r + e.x, s = a * r + e.y;
  return s = Math.round(t.canvasHeight - s), [i, s];
}
function fC(t, e) {
  return e.picking ? !0 : t.pstyle("background-fill").value !== "solid" || t.pstyle("background-image").strValue !== "none" ? !1 : t.pstyle("border-width").value === 0 || t.pstyle("border-opacity").value === 0 ? !0 : t.pstyle("border-style").value === "solid";
}
function cC(t, e) {
  if (t.length !== e.length)
    return !1;
  for (var r = 0; r < t.length; r++)
    if (t[r] !== e[r])
      return !1;
  return !0;
}
function bn(t, e, r) {
  var n = t[0] / 255, a = t[1] / 255, i = t[2] / 255, s = e, o = r || new Array(4);
  return o[0] = n * s, o[1] = a * s, o[2] = i * s, o[3] = s, o;
}
function Gn(t, e) {
  var r = e || new Array(4);
  return r[0] = (t >> 0 & 255) / 255, r[1] = (t >> 8 & 255) / 255, r[2] = (t >> 16 & 255) / 255, r[3] = (t >> 24 & 255) / 255, r;
}
function vC(t) {
  return t[0] + (t[1] << 8) + (t[2] << 16) + (t[3] << 24);
}
function dC(t, e) {
  var r = t.createTexture();
  return r.buffer = function(n) {
    t.bindTexture(t.TEXTURE_2D, r), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_WRAP_S, t.CLAMP_TO_EDGE), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_WRAP_T, t.CLAMP_TO_EDGE), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_MAG_FILTER, t.LINEAR), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_MIN_FILTER, t.LINEAR_MIPMAP_NEAREST), t.pixelStorei(t.UNPACK_PREMULTIPLY_ALPHA_WEBGL, !0), t.texImage2D(t.TEXTURE_2D, 0, t.RGBA, t.RGBA, t.UNSIGNED_BYTE, n), t.generateMipmap(t.TEXTURE_2D), t.bindTexture(t.TEXTURE_2D, null);
  }, r.deleteTexture = function() {
    t.deleteTexture(r);
  }, r;
}
function gp(t, e) {
  switch (e) {
    case "float":
      return [1, t.FLOAT, 4];
    case "vec2":
      return [2, t.FLOAT, 4];
    case "vec3":
      return [3, t.FLOAT, 4];
    case "vec4":
      return [4, t.FLOAT, 4];
    case "int":
      return [1, t.INT, 4];
    case "ivec2":
      return [2, t.INT, 4];
  }
}
function pp(t, e, r) {
  switch (e) {
    case t.FLOAT:
      return new Float32Array(r);
    case t.INT:
      return new Int32Array(r);
  }
}
function hC(t, e, r, n, a, i) {
  switch (e) {
    case t.FLOAT:
      return new Float32Array(r.buffer, i * n, a);
    case t.INT:
      return new Int32Array(r.buffer, i * n, a);
  }
}
function gC(t, e, r, n) {
  var a = gp(t, e), i = ut(a, 2), s = i[0], o = i[1], l = pp(t, o, n), u = t.createBuffer();
  return t.bindBuffer(t.ARRAY_BUFFER, u), t.bufferData(t.ARRAY_BUFFER, l, t.STATIC_DRAW), o === t.FLOAT ? t.vertexAttribPointer(r, s, o, !1, 0, 0) : o === t.INT && t.vertexAttribIPointer(r, s, o, 0, 0), t.enableVertexAttribArray(r), t.bindBuffer(t.ARRAY_BUFFER, null), u;
}
function hr(t, e, r, n) {
  var a = gp(t, r), i = ut(a, 3), s = i[0], o = i[1], l = i[2], u = pp(t, o, e * s), f = s * l, c = t.createBuffer();
  t.bindBuffer(t.ARRAY_BUFFER, c), t.bufferData(t.ARRAY_BUFFER, e * f, t.DYNAMIC_DRAW), t.enableVertexAttribArray(n), o === t.FLOAT ? t.vertexAttribPointer(n, s, o, !1, f, 0) : o === t.INT && t.vertexAttribIPointer(n, s, o, f, 0), t.vertexAttribDivisor(n, 1), t.bindBuffer(t.ARRAY_BUFFER, null);
  for (var v = new Array(e), d = 0; d < e; d++)
    v[d] = hC(t, o, u, f, s, d);
  return c.dataArray = u, c.stride = f, c.size = s, c.getView = function(h) {
    return v[h];
  }, c.setPoint = function(h, y, g) {
    var p = v[h];
    p[0] = y, p[1] = g;
  }, c.bufferSubData = function(h) {
    t.bindBuffer(t.ARRAY_BUFFER, c), h ? t.bufferSubData(t.ARRAY_BUFFER, 0, u, 0, h * s) : t.bufferSubData(t.ARRAY_BUFFER, 0, u);
  }, c;
}
function pC(t, e, r) {
  for (var n = 9, a = new Float32Array(e * n), i = new Array(e), s = 0; s < e; s++) {
    var o = s * n * 4;
    i[s] = new Float32Array(a.buffer, o, n);
  }
  var l = t.createBuffer();
  t.bindBuffer(t.ARRAY_BUFFER, l), t.bufferData(t.ARRAY_BUFFER, a.byteLength, t.DYNAMIC_DRAW);
  for (var u = 0; u < 3; u++) {
    var f = r + u;
    t.enableVertexAttribArray(f), t.vertexAttribPointer(f, 3, t.FLOAT, !1, 36, u * 12), t.vertexAttribDivisor(f, 1);
  }
  return t.bindBuffer(t.ARRAY_BUFFER, null), l.getMatrixView = function(c) {
    return i[c];
  }, l.setData = function(c, v) {
    i[v].set(c, 0);
  }, l.bufferSubData = function() {
    t.bindBuffer(t.ARRAY_BUFFER, l), t.bufferSubData(t.ARRAY_BUFFER, 0, a);
  }, l;
}
function yC(t) {
  var e = t.createFramebuffer();
  t.bindFramebuffer(t.FRAMEBUFFER, e);
  var r = t.createTexture();
  return t.bindTexture(t.TEXTURE_2D, r), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_MIN_FILTER, t.LINEAR), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_WRAP_S, t.CLAMP_TO_EDGE), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_WRAP_T, t.CLAMP_TO_EDGE), t.framebufferTexture2D(t.FRAMEBUFFER, t.COLOR_ATTACHMENT0, t.TEXTURE_2D, r, 0), t.bindFramebuffer(t.FRAMEBUFFER, null), e.setFramebufferAttachmentSizes = function(n, a) {
    t.bindTexture(t.TEXTURE_2D, r), t.texImage2D(t.TEXTURE_2D, 0, t.RGBA, n, a, 0, t.RGBA, t.UNSIGNED_BYTE, null);
  }, e;
}
var wd = typeof Float32Array < "u" ? Float32Array : Array;
Math.hypot || (Math.hypot = function() {
  for (var t = 0, e = arguments.length; e--; )
    t += arguments[e] * arguments[e];
  return Math.sqrt(t);
});
function Wl() {
  var t = new wd(9);
  return wd != Float32Array && (t[1] = 0, t[2] = 0, t[3] = 0, t[5] = 0, t[6] = 0, t[7] = 0), t[0] = 1, t[4] = 1, t[8] = 1, t;
}
function xd(t) {
  return t[0] = 1, t[1] = 0, t[2] = 0, t[3] = 0, t[4] = 1, t[5] = 0, t[6] = 0, t[7] = 0, t[8] = 1, t;
}
function mC(t, e, r) {
  var n = e[0], a = e[1], i = e[2], s = e[3], o = e[4], l = e[5], u = e[6], f = e[7], c = e[8], v = r[0], d = r[1], h = r[2], y = r[3], g = r[4], p = r[5], m = r[6], b = r[7], w = r[8];
  return t[0] = v * n + d * s + h * u, t[1] = v * a + d * o + h * f, t[2] = v * i + d * l + h * c, t[3] = y * n + g * s + p * u, t[4] = y * a + g * o + p * f, t[5] = y * i + g * l + p * c, t[6] = m * n + b * s + w * u, t[7] = m * a + b * o + w * f, t[8] = m * i + b * l + w * c, t;
}
function os(t, e, r) {
  var n = e[0], a = e[1], i = e[2], s = e[3], o = e[4], l = e[5], u = e[6], f = e[7], c = e[8], v = r[0], d = r[1];
  return t[0] = n, t[1] = a, t[2] = i, t[3] = s, t[4] = o, t[5] = l, t[6] = v * n + d * s + u, t[7] = v * a + d * o + f, t[8] = v * i + d * l + c, t;
}
function Ed(t, e, r) {
  var n = e[0], a = e[1], i = e[2], s = e[3], o = e[4], l = e[5], u = e[6], f = e[7], c = e[8], v = Math.sin(r), d = Math.cos(r);
  return t[0] = d * n + v * s, t[1] = d * a + v * o, t[2] = d * i + v * l, t[3] = d * s - v * n, t[4] = d * o - v * a, t[5] = d * l - v * i, t[6] = u, t[7] = f, t[8] = c, t;
}
function Su(t, e, r) {
  var n = r[0], a = r[1];
  return t[0] = n * e[0], t[1] = n * e[1], t[2] = n * e[2], t[3] = a * e[3], t[4] = a * e[4], t[5] = a * e[5], t[6] = e[6], t[7] = e[7], t[8] = e[8], t;
}
function bC(t, e, r) {
  return t[0] = 2 / e, t[1] = 0, t[2] = 0, t[3] = 0, t[4] = -2 / r, t[5] = 0, t[6] = -1, t[7] = 1, t[8] = 1, t;
}
var wC = /* @__PURE__ */ (function() {
  function t(e, r, n, a) {
    fn(this, t), this.debugID = Math.floor(Math.random() * 1e4), this.r = e, this.texSize = r, this.texRows = n, this.texHeight = Math.floor(r / n), this.enableWrapping = !0, this.locked = !1, this.texture = null, this.needsBuffer = !0, this.freePointer = {
      x: 0,
      row: 0
    }, this.keyToLocation = /* @__PURE__ */ new Map(), this.canvas = a(e, r, r), this.scratch = a(e, r, this.texHeight, "scratch");
  }
  return cn(t, [{
    key: "lock",
    value: function() {
      this.locked = !0;
    }
  }, {
    key: "getKeys",
    value: function() {
      return new Set(this.keyToLocation.keys());
    }
  }, {
    key: "getScale",
    value: function(r) {
      var n = r.w, a = r.h, i = this.texHeight, s = this.texSize, o = i / a, l = n * o, u = a * o;
      return l > s && (o = s / n, l = n * o, u = a * o), {
        scale: o,
        texW: l,
        texH: u
      };
    }
  }, {
    key: "draw",
    value: function(r, n, a) {
      var i = this;
      if (this.locked) throw new Error("can't draw, atlas is locked");
      var s = this.texSize, o = this.texRows, l = this.texHeight, u = this.getScale(n), f = u.scale, c = u.texW, v = u.texH, d = function(b, w) {
        if (a && w) {
          var E = w.context, T = b.x, x = b.row, S = T, D = l * x;
          E.save(), E.translate(S, D), E.scale(f, f), a(E, n), E.restore();
        }
      }, h = [null, null], y = function() {
        d(i.freePointer, i.canvas), h[0] = {
          x: i.freePointer.x,
          y: i.freePointer.row * l,
          w: c,
          h: v
        }, h[1] = {
          // create a second location with a width of 0, for convenience
          x: i.freePointer.x + c,
          y: i.freePointer.row * l,
          w: 0,
          h: v
        }, i.freePointer.x += c, i.freePointer.x == s && (i.freePointer.x = 0, i.freePointer.row++);
      }, g = function() {
        var b = i.scratch, w = i.canvas;
        b.clear(), d({
          x: 0,
          row: 0
        }, b);
        var E = s - i.freePointer.x, T = c - E, x = l;
        {
          var S = i.freePointer.x, D = i.freePointer.row * l, A = E;
          w.context.drawImage(b, 0, 0, A, x, S, D, A, x), h[0] = {
            x: S,
            y: D,
            w: A,
            h: v
          };
        }
        {
          var k = E, R = (i.freePointer.row + 1) * l, M = T;
          w && w.context.drawImage(b, k, 0, M, x, 0, R, M, x), h[1] = {
            x: 0,
            y: R,
            w: M,
            h: v
          };
        }
        i.freePointer.x = T, i.freePointer.row++;
      }, p = function() {
        i.freePointer.x = 0, i.freePointer.row++;
      };
      if (this.freePointer.x + c <= s)
        y();
      else {
        if (this.freePointer.row >= o - 1)
          return !1;
        this.freePointer.x === s ? (p(), y()) : this.enableWrapping ? g() : (p(), y());
      }
      return this.keyToLocation.set(r, h), this.needsBuffer = !0, h;
    }
  }, {
    key: "getOffsets",
    value: function(r) {
      return this.keyToLocation.get(r);
    }
  }, {
    key: "isEmpty",
    value: function() {
      return this.freePointer.x === 0 && this.freePointer.row === 0;
    }
  }, {
    key: "canFit",
    value: function(r) {
      if (this.locked) return !1;
      var n = this.texSize, a = this.texRows, i = this.getScale(r), s = i.texW;
      return this.freePointer.x + s > n ? this.freePointer.row < a - 1 : !0;
    }
    // called on every frame
  }, {
    key: "bufferIfNeeded",
    value: function(r) {
      this.texture || (this.texture = dC(r, this.debugID)), this.needsBuffer && (this.texture.buffer(this.canvas), this.needsBuffer = !1, this.locked && (this.canvas = null, this.scratch = null));
    }
  }, {
    key: "dispose",
    value: function() {
      this.texture && (this.texture.deleteTexture(), this.texture = null), this.canvas = null, this.scratch = null, this.locked = !0;
    }
  }]);
})(), xC = /* @__PURE__ */ (function() {
  function t(e, r, n, a) {
    fn(this, t), this.r = e, this.texSize = r, this.texRows = n, this.createTextureCanvas = a, this.atlases = [], this.styleKeyToAtlas = /* @__PURE__ */ new Map(), this.markedKeys = /* @__PURE__ */ new Set();
  }
  return cn(t, [{
    key: "getKeys",
    value: function() {
      return new Set(this.styleKeyToAtlas.keys());
    }
  }, {
    key: "_createAtlas",
    value: function() {
      var r = this.r, n = this.texSize, a = this.texRows, i = this.createTextureCanvas;
      return new wC(r, n, a, i);
    }
  }, {
    key: "_getScratchCanvas",
    value: function() {
      if (!this.scratch) {
        var r = this.r, n = this.texSize, a = this.texRows, i = this.createTextureCanvas, s = Math.floor(n / a);
        this.scratch = i(r, n, s, "scratch");
      }
      return this.scratch;
    }
  }, {
    key: "draw",
    value: function(r, n, a) {
      var i = this.styleKeyToAtlas.get(r);
      return i || (i = this.atlases[this.atlases.length - 1], (!i || !i.canFit(n)) && (i && i.lock(), i = this._createAtlas(), this.atlases.push(i)), i.draw(r, n, a), this.styleKeyToAtlas.set(r, i)), i;
    }
  }, {
    key: "getAtlas",
    value: function(r) {
      return this.styleKeyToAtlas.get(r);
    }
  }, {
    key: "hasAtlas",
    value: function(r) {
      return this.styleKeyToAtlas.has(r);
    }
  }, {
    key: "markKeyForGC",
    value: function(r) {
      this.markedKeys.add(r);
    }
  }, {
    key: "gc",
    value: function() {
      var r = this, n = this.markedKeys;
      if (n.size === 0) {
        console.log("nothing to garbage collect");
        return;
      }
      var a = [], i = /* @__PURE__ */ new Map(), s = null, o = Gt(this.atlases), l;
      try {
        var u = function() {
          var c = l.value, v = c.getKeys(), d = EC(n, v);
          if (d.size === 0)
            return a.push(c), v.forEach(function(E) {
              return i.set(E, c);
            }), 1;
          s || (s = r._createAtlas(), a.push(s));
          var h = Gt(v), y;
          try {
            for (h.s(); !(y = h.n()).done; ) {
              var g = y.value;
              if (!d.has(g)) {
                var p = c.getOffsets(g), m = ut(p, 2), b = m[0], w = m[1];
                s.canFit({
                  w: b.w + w.w,
                  h: b.h
                }) || (s.lock(), s = r._createAtlas(), a.push(s)), c.canvas && (r._copyTextureToNewAtlas(g, c, s), i.set(g, s));
              }
            }
          } catch (E) {
            h.e(E);
          } finally {
            h.f();
          }
          c.dispose();
        };
        for (o.s(); !(l = o.n()).done; )
          u();
      } catch (f) {
        o.e(f);
      } finally {
        o.f();
      }
      this.atlases = a, this.styleKeyToAtlas = i, this.markedKeys = /* @__PURE__ */ new Set();
    }
  }, {
    key: "_copyTextureToNewAtlas",
    value: function(r, n, a) {
      var i = n.getOffsets(r), s = ut(i, 2), o = s[0], l = s[1];
      if (l.w === 0)
        a.draw(r, o, function(v) {
          v.drawImage(n.canvas, o.x, o.y, o.w, o.h, 0, 0, o.w, o.h);
        });
      else {
        var u = this._getScratchCanvas();
        u.clear(), u.context.drawImage(n.canvas, o.x, o.y, o.w, o.h, 0, 0, o.w, o.h), u.context.drawImage(n.canvas, l.x, l.y, l.w, l.h, o.w, 0, l.w, l.h);
        var f = o.w + l.w, c = o.h;
        a.draw(r, {
          w: f,
          h: c
        }, function(v) {
          v.drawImage(
            u,
            0,
            0,
            f,
            c,
            0,
            0,
            f,
            c
            // the destination context has already been translated to the correct position
          );
        });
      }
    }
  }, {
    key: "getCounts",
    value: function() {
      return {
        keyCount: this.styleKeyToAtlas.size,
        atlasCount: new Set(this.styleKeyToAtlas.values()).size
      };
    }
  }]);
})();
function EC(t, e) {
  return t.intersection ? t.intersection(e) : new Set(ys(t).filter(function(r) {
    return e.has(r);
  }));
}
var CC = /* @__PURE__ */ (function() {
  function t(e, r) {
    fn(this, t), this.r = e, this.globalOptions = r, this.atlasSize = r.webglTexSize, this.maxAtlasesPerBatch = r.webglTexPerBatch, this.renderTypes = /* @__PURE__ */ new Map(), this.collections = /* @__PURE__ */ new Map(), this.typeAndIdToKey = /* @__PURE__ */ new Map();
  }
  return cn(t, [{
    key: "getAtlasSize",
    value: function() {
      return this.atlasSize;
    }
  }, {
    key: "addAtlasCollection",
    value: function(r, n) {
      var a = this.globalOptions, i = a.webglTexSize, s = a.createTextureCanvas, o = n.texRows, l = this._cacheScratchCanvas(s), u = new xC(this.r, i, o, l);
      this.collections.set(r, u);
    }
  }, {
    key: "addRenderType",
    value: function(r, n) {
      var a = n.collection;
      if (!this.collections.has(a)) throw new Error("invalid atlas collection name '".concat(a, "'"));
      var i = this.collections.get(a), s = Ae({
        type: r,
        atlasCollection: i
      }, n);
      this.renderTypes.set(r, s);
    }
  }, {
    key: "getRenderTypeOpts",
    value: function(r) {
      return this.renderTypes.get(r);
    }
  }, {
    key: "getAtlasCollection",
    value: function(r) {
      return this.collections.get(r);
    }
  }, {
    key: "_cacheScratchCanvas",
    value: function(r) {
      var n = -1, a = -1, i = null;
      return function(s, o, l, u) {
        return u ? ((!i || o != n || l != a) && (n = o, a = l, i = r(s, o, l)), i) : r(s, o, l);
      };
    }
  }, {
    key: "_key",
    value: function(r, n) {
      return "".concat(r, "-").concat(n);
    }
    /** Marks textues associated with the element for garbage collection. */
  }, {
    key: "invalidate",
    value: function(r) {
      var n = this, a = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : {}, i = a.forceRedraw, s = i === void 0 ? !1 : i, o = a.filterEle, l = o === void 0 ? function() {
        return !0;
      } : o, u = a.filterType, f = u === void 0 ? function() {
        return !0;
      } : u, c = !1, v = !1, d = Gt(r), h;
      try {
        for (d.s(); !(h = d.n()).done; ) {
          var y = h.value;
          if (l(y)) {
            var g = Gt(this.renderTypes.values()), p;
            try {
              var m = function() {
                var w = p.value, E = w.type;
                if (f(E)) {
                  var T = n.collections.get(w.collection), x = w.getKey(y), S = Array.isArray(x) ? x : [x];
                  if (s)
                    S.forEach(function(R) {
                      return T.markKeyForGC(R);
                    }), v = !0;
                  else {
                    var D = w.getID ? w.getID(y) : y.id(), A = n._key(E, D), k = n.typeAndIdToKey.get(A);
                    k !== void 0 && !cC(S, k) && (c = !0, n.typeAndIdToKey.delete(A), k.forEach(function(R) {
                      return T.markKeyForGC(R);
                    }));
                  }
                }
              };
              for (g.s(); !(p = g.n()).done; )
                m();
            } catch (b) {
              g.e(b);
            } finally {
              g.f();
            }
          }
        }
      } catch (b) {
        d.e(b);
      } finally {
        d.f();
      }
      return v && (this.gc(), c = !1), c;
    }
    /** Garbage collect */
  }, {
    key: "gc",
    value: function() {
      var r = Gt(this.collections.values()), n;
      try {
        for (r.s(); !(n = r.n()).done; ) {
          var a = n.value;
          a.gc();
        }
      } catch (i) {
        r.e(i);
      } finally {
        r.f();
      }
    }
  }, {
    key: "getOrCreateAtlas",
    value: function(r, n, a, i) {
      var s = this.renderTypes.get(n), o = this.collections.get(s.collection), l = !1, u = o.draw(i, a, function(v) {
        s.drawClipped ? (v.save(), v.beginPath(), v.rect(0, 0, a.w, a.h), v.clip(), s.drawElement(v, r, a, !0, !0), v.restore()) : s.drawElement(v, r, a, !0, !0), l = !0;
      });
      if (l) {
        var f = s.getID ? s.getID(r) : r.id(), c = this._key(n, f);
        this.typeAndIdToKey.has(c) ? this.typeAndIdToKey.get(c).push(i) : this.typeAndIdToKey.set(c, [i]);
      }
      return u;
    }
  }, {
    key: "getAtlasInfo",
    value: function(r, n) {
      var a = this, i = this.renderTypes.get(n), s = i.getKey(r), o = Array.isArray(s) ? s : [s];
      return o.map(function(l) {
        var u = i.getBoundingBox(r, l), f = a.getOrCreateAtlas(r, n, u, l), c = f.getOffsets(l), v = ut(c, 2), d = v[0], h = v[1];
        return {
          atlas: f,
          tex: d,
          tex1: d,
          tex2: h,
          bb: u
        };
      });
    }
  }, {
    key: "getDebugInfo",
    value: function() {
      var r = [], n = Gt(this.collections), a;
      try {
        for (n.s(); !(a = n.n()).done; ) {
          var i = ut(a.value, 2), s = i[0], o = i[1], l = o.getCounts(), u = l.keyCount, f = l.atlasCount;
          r.push({
            type: s,
            keyCount: u,
            atlasCount: f
          });
        }
      } catch (c) {
        n.e(c);
      } finally {
        n.f();
      }
      return r;
    }
  }]);
})(), TC = /* @__PURE__ */ (function() {
  function t(e) {
    fn(this, t), this.globalOptions = e, this.atlasSize = e.webglTexSize, this.maxAtlasesPerBatch = e.webglTexPerBatch, this.batchAtlases = [];
  }
  return cn(t, [{
    key: "getMaxAtlasesPerBatch",
    value: function() {
      return this.maxAtlasesPerBatch;
    }
  }, {
    key: "getAtlasSize",
    value: function() {
      return this.atlasSize;
    }
  }, {
    key: "getIndexArray",
    value: function() {
      return Array.from({
        length: this.maxAtlasesPerBatch
      }, function(r, n) {
        return n;
      });
    }
  }, {
    key: "startBatch",
    value: function() {
      this.batchAtlases = [];
    }
  }, {
    key: "getAtlasCount",
    value: function() {
      return this.batchAtlases.length;
    }
  }, {
    key: "getAtlases",
    value: function() {
      return this.batchAtlases;
    }
  }, {
    key: "canAddToCurrentBatch",
    value: function(r) {
      return this.batchAtlases.length === this.maxAtlasesPerBatch ? this.batchAtlases.includes(r) : !0;
    }
  }, {
    key: "getAtlasIndexForBatch",
    value: function(r) {
      var n = this.batchAtlases.indexOf(r);
      if (n < 0) {
        if (this.batchAtlases.length === this.maxAtlasesPerBatch)
          throw new Error("cannot add more atlases to batch");
        this.batchAtlases.push(r), n = this.batchAtlases.length - 1;
      }
      return n;
    }
  }]);
})(), SC = `
  float circleSD(vec2 p, float r) {
    return distance(vec2(0), p) - r; // signed distance
  }
`, PC = `
  float rectangleSD(vec2 p, vec2 b) {
    vec2 d = abs(p)-b;
    return distance(vec2(0),max(d,0.0)) + min(max(d.x,d.y),0.0);
  }
`, DC = `
  float roundRectangleSD(vec2 p, vec2 b, vec4 cr) {
    cr.xy = (p.x > 0.0) ? cr.xy : cr.zw;
    cr.x  = (p.y > 0.0) ? cr.x  : cr.y;
    vec2 q = abs(p) - b + cr.x;
    return min(max(q.x, q.y), 0.0) + distance(vec2(0), max(q, 0.0)) - cr.x;
  }
`, AC = `
  float ellipseSD(vec2 p, vec2 ab) {
    p = abs( p ); // symmetry

    // find root with Newton solver
    vec2 q = ab*(p-ab);
    float w = (q.x<q.y)? 1.570796327 : 0.0;
    for( int i=0; i<5; i++ ) {
      vec2 cs = vec2(cos(w),sin(w));
      vec2 u = ab*vec2( cs.x,cs.y);
      vec2 v = ab*vec2(-cs.y,cs.x);
      w = w + dot(p-u,v)/(dot(p-u,u)+dot(v,v));
    }
    
    // compute final point and distance
    float d = length(p-ab*vec2(cos(w),sin(w)));
    
    // return signed distance
    return (dot(p/ab,p/ab)>1.0) ? d : -d;
  }
`, Ya = {
  SCREEN: {
    name: "screen",
    screen: !0
  },
  PICKING: {
    name: "picking",
    picking: !0
  }
}, ks = {
  // render the texture just like in RENDER_TARGET.SCREEN mode
  IGNORE: 1,
  // don't render the texture at all
  USE_BB: 2
  // render the bounding box as an opaque rectangle
}, Kl = 0, Cd = 1, Td = 2, Yl = 3, Wn = 4, Gi = 5, La = 6, Ia = 7, kC = /* @__PURE__ */ (function() {
  function t(e, r, n) {
    fn(this, t), this.r = e, this.gl = r, this.maxInstances = n.webglBatchSize, this.atlasSize = n.webglTexSize, this.bgColor = n.bgColor, this.debug = n.webglDebug, this.batchDebugInfo = [], n.enableWrapping = !0, n.createTextureCanvas = oC, this.atlasManager = new CC(e, n), this.batchManager = new TC(n), this.simpleShapeOptions = /* @__PURE__ */ new Map(), this.program = this._createShaderProgram(Ya.SCREEN), this.pickingProgram = this._createShaderProgram(Ya.PICKING), this.vao = this._createVAO();
  }
  return cn(t, [{
    key: "addAtlasCollection",
    value: function(r, n) {
      this.atlasManager.addAtlasCollection(r, n);
    }
    /**
     * @typedef { Object } TextureRenderTypeOpts
     * @property { string } collection - name of atlas collection to render textures to
     * @property { function } getKey - returns the "style key" for an element, may be a single value or an array for multi-line lables
     * @property { function } drawElement - uses a canvas renderer to draw the element to the texture atlas
     * @property { boolean  } drawClipped - if true the context will be clipped to the bounding box before drawElement() is called, may affect performance
     * @property { function } getBoundingBox - returns the bounding box for an element
     * @property { function } getRotation
     * @property { function } getRotationPoint
     * @property { function } getRotationOffset
     * @property { function } isVisible - an extra check for visibility in addition to ele.visible()
     * @property { function } getTexPickingMode - returns a value from the TEX_PICKING_MODE enum
     */
    /**
     * @param { string } typeName
     * @param { TextureRenderTypeOpts } opts
     */
  }, {
    key: "addTextureAtlasRenderType",
    value: function(r, n) {
      this.atlasManager.addRenderType(r, n);
    }
    /**
     * @typedef { Object } SimpleShapeRenderTypeOpts
     * @property { function } getBoundingBox - returns the bounding box for an element
     * @property { function } isVisible - this is an extra check for visibility in addition to ele.visible()
     * @property { function } isSimple - check if element is a simple shape, or if it needs to fall back to texture rendering
     * @property { ShapeVisualProperties } shapeProps
     */
    /**
     * @typedef { Object } ShapeVisualProperties
     * @property { string } shape
     * @property { string } color
     * @property { string } opacity
     * @property { string } padding
     * @property { string } radius
     * @property { boolean } border
    */
    /**
     * @param { string } typeName
     * @param { SimpleShapeRenderTypeOpts } opts
     */
  }, {
    key: "addSimpleShapeRenderType",
    value: function(r, n) {
      this.simpleShapeOptions.set(r, n);
    }
    /**
     * Inform the atlasManager when element style keys may have changed.
     * The atlasManager can then mark unused textures for "garbage collection".
     */
  }, {
    key: "invalidate",
    value: function(r) {
      var n = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : {}, a = n.type, i = this.atlasManager;
      return a ? i.invalidate(r, {
        filterType: function(o) {
          return o === a;
        },
        forceRedraw: !0
      }) : i.invalidate(r);
    }
    /**
     * Run texture garbage collection.
     */
  }, {
    key: "gc",
    value: function() {
      this.atlasManager.gc();
    }
  }, {
    key: "_createShaderProgram",
    value: function(r) {
      var n = this.gl, a = `#version 300 es
      precision highp float;

      uniform mat3 uPanZoomMatrix;
      uniform int  uAtlasSize;
      
      // instanced
      in vec2 aPosition; // a vertex from the unit square
      
      in mat3 aTransform; // used to transform verticies, eg into a bounding box
      in int aVertType; // the type of thing we are rendering

      // the z-index that is output when using picking mode
      in vec4 aIndex;
      
      // For textures
      in int aAtlasId; // which shader unit/atlas to use
      in vec4 aTex; // x/y/w/h of texture in atlas

      // for edges
      in vec4 aPointAPointB;
      in vec4 aPointCPointD;
      in vec2 aLineWidth; // also used for node border width

      // simple shapes
      in vec4 aCornerRadius; // for round-rectangle [top-right, bottom-right, top-left, bottom-left]
      in vec4 aColor; // also used for edges
      in vec4 aBorderColor; // aLineWidth is used for border width

      // output values passed to the fragment shader
      out vec2 vTexCoord;
      out vec4 vColor;
      out vec2 vPosition;
      // flat values are not interpolated
      flat out int vAtlasId; 
      flat out int vVertType;
      flat out vec2 vTopRight;
      flat out vec2 vBotLeft;
      flat out vec4 vCornerRadius;
      flat out vec4 vBorderColor;
      flat out vec2 vBorderWidth;
      flat out vec4 vIndex;
      
      void main(void) {
        int vid = gl_VertexID;
        vec2 position = aPosition; // TODO make this a vec3, simplifies some code below

        if(aVertType == `.concat(Kl, `) {
          float texX = aTex.x; // texture coordinates
          float texY = aTex.y;
          float texW = aTex.z;
          float texH = aTex.w;

          if(vid == 1 || vid == 2 || vid == 4) {
            texX += texW;
          }
          if(vid == 2 || vid == 4 || vid == 5) {
            texY += texH;
          }

          float d = float(uAtlasSize);
          vTexCoord = vec2(texX / d, texY / d); // tex coords must be between 0 and 1

          gl_Position = vec4(uPanZoomMatrix * aTransform * vec3(position, 1.0), 1.0);
        }
        else if(aVertType == `).concat(Wn, " || aVertType == ").concat(Ia, ` 
             || aVertType == `).concat(Gi, " || aVertType == ").concat(La, `) { // simple shapes

          // the bounding box is needed by the fragment shader
          vBotLeft  = (aTransform * vec3(0, 0, 1)).xy; // flat
          vTopRight = (aTransform * vec3(1, 1, 1)).xy; // flat
          vPosition = (aTransform * vec3(position, 1)).xy; // will be interpolated

          // calculations are done in the fragment shader, just pass these along
          vColor = aColor;
          vCornerRadius = aCornerRadius;
          vBorderColor = aBorderColor;
          vBorderWidth = aLineWidth;

          gl_Position = vec4(uPanZoomMatrix * aTransform * vec3(position, 1.0), 1.0);
        }
        else if(aVertType == `).concat(Cd, `) {
          vec2 source = aPointAPointB.xy;
          vec2 target = aPointAPointB.zw;

          // adjust the geometry so that the line is centered on the edge
          position.y = position.y - 0.5;

          // stretch the unit square into a long skinny rectangle
          vec2 xBasis = target - source;
          vec2 yBasis = normalize(vec2(-xBasis.y, xBasis.x));
          vec2 point = source + xBasis * position.x + yBasis * aLineWidth[0] * position.y;

          gl_Position = vec4(uPanZoomMatrix * vec3(point, 1.0), 1.0);
          vColor = aColor;
        } 
        else if(aVertType == `).concat(Td, `) {
          vec2 pointA = aPointAPointB.xy;
          vec2 pointB = aPointAPointB.zw;
          vec2 pointC = aPointCPointD.xy;
          vec2 pointD = aPointCPointD.zw;

          // adjust the geometry so that the line is centered on the edge
          position.y = position.y - 0.5;

          vec2 p0, p1, p2, pos;
          if(position.x == 0.0) { // The left side of the unit square
            p0 = pointA;
            p1 = pointB;
            p2 = pointC;
            pos = position;
          } else { // The right side of the unit square, use same approach but flip the geometry upside down
            p0 = pointD;
            p1 = pointC;
            p2 = pointB;
            pos = vec2(0.0, -position.y);
          }

          vec2 p01 = p1 - p0;
          vec2 p12 = p2 - p1;
          vec2 p21 = p1 - p2;

          // Find the normal vector.
          vec2 tangent = normalize(normalize(p12) + normalize(p01));
          vec2 normal = vec2(-tangent.y, tangent.x);

          // Find the vector perpendicular to p0 -> p1.
          vec2 p01Norm = normalize(vec2(-p01.y, p01.x));

          // Determine the bend direction.
          float sigma = sign(dot(p01 + p21, normal));
          float width = aLineWidth[0];

          if(sign(pos.y) == -sigma) {
            // This is an intersecting vertex. Adjust the position so that there's no overlap.
            vec2 point = 0.5 * width * normal * -sigma / dot(normal, p01Norm);
            gl_Position = vec4(uPanZoomMatrix * vec3(p1 + point, 1.0), 1.0);
          } else {
            // This is a non-intersecting vertex. Treat it like a mitre join.
            vec2 point = 0.5 * width * normal * sigma * dot(normal, p01Norm);
            gl_Position = vec4(uPanZoomMatrix * vec3(p1 + point, 1.0), 1.0);
          }

          vColor = aColor;
        } 
        else if(aVertType == `).concat(Yl, ` && vid < 3) {
          // massage the first triangle into an edge arrow
          if(vid == 0)
            position = vec2(-0.15, -0.3);
          if(vid == 1)
            position = vec2(  0.0,  0.0);
          if(vid == 2)
            position = vec2( 0.15, -0.3);

          gl_Position = vec4(uPanZoomMatrix * aTransform * vec3(position, 1.0), 1.0);
          vColor = aColor;
        }
        else {
          gl_Position = vec4(2.0, 0.0, 0.0, 1.0); // discard vertex by putting it outside webgl clip space
        }

        vAtlasId = aAtlasId;
        vVertType = aVertType;
        vIndex = aIndex;
      }
    `), i = this.batchManager.getIndexArray(), s = `#version 300 es
      precision highp float;

      // declare texture unit for each texture atlas in the batch
      `.concat(i.map(function(u) {
        return "uniform sampler2D uTexture".concat(u, ";");
      }).join(`
	`), `

      uniform vec4 uBGColor;
      uniform float uZoom;

      in vec2 vTexCoord;
      in vec4 vColor;
      in vec2 vPosition; // model coordinates

      flat in int vAtlasId;
      flat in vec4 vIndex;
      flat in int vVertType;
      flat in vec2 vTopRight;
      flat in vec2 vBotLeft;
      flat in vec4 vCornerRadius;
      flat in vec4 vBorderColor;
      flat in vec2 vBorderWidth;

      out vec4 outColor;

      `).concat(SC, `
      `).concat(PC, `
      `).concat(DC, `
      `).concat(AC, `

      vec4 blend(vec4 top, vec4 bot) { // blend colors with premultiplied alpha
        return vec4( 
          top.rgb + (bot.rgb * (1.0 - top.a)),
          top.a   + (bot.a   * (1.0 - top.a)) 
        );
      }

      vec4 distInterp(vec4 cA, vec4 cB, float d) { // interpolate color using Signed Distance
        // scale to the zoom level so that borders don't look blurry when zoomed in
        // note 1.5 is an aribitrary value chosen because it looks good
        return mix(cA, cB, 1.0 - smoothstep(0.0, 1.5 / uZoom, abs(d))); 
      }

      void main(void) {
        if(vVertType == `).concat(Kl, `) {
          // look up the texel from the texture unit
          `).concat(i.map(function(u) {
        return "if(vAtlasId == ".concat(u, ") outColor = texture(uTexture").concat(u, ", vTexCoord);");
      }).join(`
	else `), `
        } 
        else if(vVertType == `).concat(Yl, `) {
          // mimics how canvas renderer uses context.globalCompositeOperation = 'destination-out';
          outColor = blend(vColor, uBGColor);
          outColor.a = 1.0; // make opaque, masks out line under arrow
        }
        else if(vVertType == `).concat(Wn, ` && vBorderWidth == vec2(0.0)) { // simple rectangle with no border
          outColor = vColor; // unit square is already transformed to the rectangle, nothing else needs to be done
        }
        else if(vVertType == `).concat(Wn, " || vVertType == ").concat(Ia, ` 
          || vVertType == `).concat(Gi, " || vVertType == ").concat(La, `) { // use SDF

          float outerBorder = vBorderWidth[0];
          float innerBorder = vBorderWidth[1];
          float borderPadding = outerBorder * 2.0;
          float w = vTopRight.x - vBotLeft.x - borderPadding;
          float h = vTopRight.y - vBotLeft.y - borderPadding;
          vec2 b = vec2(w/2.0, h/2.0); // half width, half height
          vec2 p = vPosition - vec2(vTopRight.x - b[0] - outerBorder, vTopRight.y - b[1] - outerBorder); // translate to center

          float d; // signed distance
          if(vVertType == `).concat(Wn, `) {
            d = rectangleSD(p, b);
          } else if(vVertType == `).concat(Ia, ` && w == h) {
            d = circleSD(p, b.x); // faster than ellipse
          } else if(vVertType == `).concat(Ia, `) {
            d = ellipseSD(p, b);
          } else {
            d = roundRectangleSD(p, b, vCornerRadius.wzyx);
          }

          // use the distance to interpolate a color to smooth the edges of the shape, doesn't need multisampling
          // we must smooth colors inwards, because we can't change pixels outside the shape's bounding box
          if(d > 0.0) {
            if(d > outerBorder) {
              discard;
            } else {
              outColor = distInterp(vBorderColor, vec4(0), d - outerBorder);
            }
          } else {
            if(d > innerBorder) {
              vec4 outerColor = outerBorder == 0.0 ? vec4(0) : vBorderColor;
              vec4 innerBorderColor = blend(vBorderColor, vColor);
              outColor = distInterp(innerBorderColor, outerColor, d);
            } 
            else {
              vec4 outerColor;
              if(innerBorder == 0.0 && outerBorder == 0.0) {
                outerColor = vec4(0);
              } else if(innerBorder == 0.0) {
                outerColor = vBorderColor;
              } else {
                outerColor = blend(vBorderColor, vColor);
              }
              outColor = distInterp(vColor, outerColor, d - innerBorder);
            }
          }
        }
        else {
          outColor = vColor;
        }

        `).concat(r.picking ? `if(outColor.a == 0.0) discard;
             else outColor = vIndex;` : "", `
      }
    `), o = sC(n, a, s);
      o.aPosition = n.getAttribLocation(o, "aPosition"), o.aIndex = n.getAttribLocation(o, "aIndex"), o.aVertType = n.getAttribLocation(o, "aVertType"), o.aTransform = n.getAttribLocation(o, "aTransform"), o.aAtlasId = n.getAttribLocation(o, "aAtlasId"), o.aTex = n.getAttribLocation(o, "aTex"), o.aPointAPointB = n.getAttribLocation(o, "aPointAPointB"), o.aPointCPointD = n.getAttribLocation(o, "aPointCPointD"), o.aLineWidth = n.getAttribLocation(o, "aLineWidth"), o.aColor = n.getAttribLocation(o, "aColor"), o.aCornerRadius = n.getAttribLocation(o, "aCornerRadius"), o.aBorderColor = n.getAttribLocation(o, "aBorderColor"), o.uPanZoomMatrix = n.getUniformLocation(o, "uPanZoomMatrix"), o.uAtlasSize = n.getUniformLocation(o, "uAtlasSize"), o.uBGColor = n.getUniformLocation(o, "uBGColor"), o.uZoom = n.getUniformLocation(o, "uZoom"), o.uTextures = [];
      for (var l = 0; l < this.batchManager.getMaxAtlasesPerBatch(); l++)
        o.uTextures.push(n.getUniformLocation(o, "uTexture".concat(l)));
      return o;
    }
  }, {
    key: "_createVAO",
    value: function() {
      var r = [0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1];
      this.vertexCount = r.length / 2;
      var n = this.maxInstances, a = this.gl, i = this.program, s = a.createVertexArray();
      return a.bindVertexArray(s), gC(a, "vec2", i.aPosition, r), this.transformBuffer = pC(a, n, i.aTransform), this.indexBuffer = hr(a, n, "vec4", i.aIndex), this.vertTypeBuffer = hr(a, n, "int", i.aVertType), this.atlasIdBuffer = hr(a, n, "int", i.aAtlasId), this.texBuffer = hr(a, n, "vec4", i.aTex), this.pointAPointBBuffer = hr(a, n, "vec4", i.aPointAPointB), this.pointCPointDBuffer = hr(a, n, "vec4", i.aPointCPointD), this.lineWidthBuffer = hr(a, n, "vec2", i.aLineWidth), this.colorBuffer = hr(a, n, "vec4", i.aColor), this.cornerRadiusBuffer = hr(a, n, "vec4", i.aCornerRadius), this.borderColorBuffer = hr(a, n, "vec4", i.aBorderColor), a.bindVertexArray(null), s;
    }
  }, {
    key: "buffers",
    get: function() {
      var r = this;
      return this._buffers || (this._buffers = Object.keys(this).filter(function(n) {
        return Qr(n, "Buffer");
      }).map(function(n) {
        return r[n];
      })), this._buffers;
    }
  }, {
    key: "startFrame",
    value: function(r) {
      var n = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Ya.SCREEN;
      this.panZoomMatrix = r, this.renderTarget = n, this.batchDebugInfo = [], this.wrappedCount = 0, this.simpleCount = 0, this.startBatch();
    }
  }, {
    key: "startBatch",
    value: function() {
      this.instanceCount = 0, this.batchManager.startBatch();
    }
  }, {
    key: "endFrame",
    value: function() {
      this.endBatch();
    }
  }, {
    key: "_isVisible",
    value: function(r, n) {
      return r.visible() ? n && n.isVisible ? n.isVisible(r) : !0 : !1;
    }
    /**
     * Draws a texture using the texture atlas.
     */
  }, {
    key: "drawTexture",
    value: function(r, n, a) {
      var i = this.atlasManager, s = this.batchManager, o = i.getRenderTypeOpts(a);
      if (this._isVisible(r, o) && !(r.isEdge() && !this._isValidEdge(r))) {
        if (this.renderTarget.picking && o.getTexPickingMode) {
          var l = o.getTexPickingMode(r);
          if (l === ks.IGNORE)
            return;
          if (l == ks.USE_BB) {
            this.drawPickingRectangle(r, n, a);
            return;
          }
        }
        var u = i.getAtlasInfo(r, a), f = Gt(u), c;
        try {
          for (f.s(); !(c = f.n()).done; ) {
            var v = c.value, d = v.atlas, h = v.tex1, y = v.tex2;
            s.canAddToCurrentBatch(d) || this.endBatch();
            for (var g = s.getAtlasIndexForBatch(d), p = 0, m = [[h, !0], [y, !1]]; p < m.length; p++) {
              var b = ut(m[p], 2), w = b[0], E = b[1];
              if (w.w != 0) {
                var T = this.instanceCount;
                this.vertTypeBuffer.getView(T)[0] = Kl;
                var x = this.indexBuffer.getView(T);
                Gn(n, x);
                var S = this.atlasIdBuffer.getView(T);
                S[0] = g;
                var D = this.texBuffer.getView(T);
                D[0] = w.x, D[1] = w.y, D[2] = w.w, D[3] = w.h;
                var A = this.transformBuffer.getMatrixView(T);
                this.setTransformMatrix(r, A, o, v, E), this.instanceCount++, E || this.wrappedCount++, this.instanceCount >= this.maxInstances && this.endBatch();
              }
            }
          }
        } catch (k) {
          f.e(k);
        } finally {
          f.f();
        }
      }
    }
    /**
     * matrix is expected to be a 9 element array
     * this function follows same pattern as CRp.drawCachedElementPortion(...)
     */
  }, {
    key: "setTransformMatrix",
    value: function(r, n, a, i) {
      var s = arguments.length > 4 && arguments[4] !== void 0 ? arguments[4] : !0, o = 0;
      if (a.shapeProps && a.shapeProps.padding && (o = r.pstyle(a.shapeProps.padding).pfValue), i) {
        var l = i.bb, u = i.tex1, f = i.tex2, c = u.w / (u.w + f.w);
        s || (c = 1 - c);
        var v = this._getAdjustedBB(l, o, s, c);
        this._applyTransformMatrix(n, v, a, r);
      } else {
        var d = a.getBoundingBox(r), h = this._getAdjustedBB(d, o, !0, 1);
        this._applyTransformMatrix(n, h, a, r);
      }
    }
  }, {
    key: "_applyTransformMatrix",
    value: function(r, n, a, i) {
      var s, o;
      xd(r);
      var l = a.getRotation ? a.getRotation(i) : 0;
      if (l !== 0) {
        var u = a.getRotationPoint(i), f = u.x, c = u.y;
        os(r, r, [f, c]), Ed(r, r, l);
        var v = a.getRotationOffset(i);
        s = v.x + (n.xOffset || 0), o = v.y + (n.yOffset || 0);
      } else
        s = n.x1, o = n.y1;
      os(r, r, [s, o]), Su(r, r, [n.w, n.h]);
    }
    /**
     * Adjusts a node or label BB to accomodate padding and split for wrapped textures.
     * @param bb - the original bounding box
     * @param padding - the padding to add to the bounding box
     * @param first - whether this is the first part of a wrapped texture
     * @param ratio - the ratio of the texture width of part of the text to the entire texture
     */
  }, {
    key: "_getAdjustedBB",
    value: function(r, n, a, i) {
      var s = r.x1, o = r.y1, l = r.w, u = r.h, f = r.yOffset;
      n && (s -= n, o -= n, l += 2 * n, u += 2 * n);
      var c = 0, v = l * i;
      return a && i < 1 ? l = v : !a && i < 1 && (c = l - v, s += c, l = v), {
        x1: s,
        y1: o,
        w: l,
        h: u,
        xOffset: c,
        yOffset: f
      };
    }
    /**
     * Draw a solid opaque rectangle matching the element's Bounding Box.
     * Used by the PICKING mode to make the entire BB of a label clickable.
     */
  }, {
    key: "drawPickingRectangle",
    value: function(r, n, a) {
      var i = this.atlasManager.getRenderTypeOpts(a), s = this.instanceCount;
      this.vertTypeBuffer.getView(s)[0] = Wn;
      var o = this.indexBuffer.getView(s);
      Gn(n, o);
      var l = this.colorBuffer.getView(s);
      bn([0, 0, 0], 1, l);
      var u = this.transformBuffer.getMatrixView(s);
      this.setTransformMatrix(r, u, i), this.simpleCount++, this.instanceCount++, this.instanceCount >= this.maxInstances && this.endBatch();
    }
    /**
     * Draw a node using either a texture or a "simple shape".
     */
  }, {
    key: "drawNode",
    value: function(r, n, a) {
      var i = this.simpleShapeOptions.get(a);
      if (this._isVisible(r, i)) {
        var s = i.shapeProps, o = this._getVertTypeForShape(r, s.shape);
        if (o === void 0 || i.isSimple && !i.isSimple(r, this.renderTarget)) {
          this.drawTexture(r, n, a);
          return;
        }
        var l = this.instanceCount;
        if (this.vertTypeBuffer.getView(l)[0] = o, o === Gi || o === La) {
          var u = i.getBoundingBox(r), f = this._getCornerRadius(r, s.radius, u), c = this.cornerRadiusBuffer.getView(l);
          c[0] = f, c[1] = f, c[2] = f, c[3] = f, o === La && (c[0] = 0, c[2] = 0);
        }
        var v = this.indexBuffer.getView(l);
        Gn(n, v);
        var d = this.renderTarget.picking ? 1 : a === "node-body" ? r.effectiveOpacity() : 1, h = this.renderTarget.picking ? 1 : r.pstyle(s.opacity).value * d, y = r.pstyle(s.color).value, g = this.colorBuffer.getView(l);
        bn(y, h, g);
        var p = this.lineWidthBuffer.getView(l);
        if (p[0] = 0, p[1] = 0, s.border) {
          var m = r.pstyle("border-width").value;
          if (m > 0) {
            var b = r.pstyle("border-color").value, w = d * r.pstyle("border-opacity").value, E = this.borderColorBuffer.getView(l);
            bn(b, w, E);
            var T = r.pstyle("border-position").value;
            if (T === "inside")
              p[0] = 0, p[1] = -m;
            else if (T === "outside")
              p[0] = m, p[1] = 0;
            else {
              var x = m / 2;
              p[0] = x, p[1] = -x;
            }
          }
        }
        var S = this.transformBuffer.getMatrixView(l);
        this.setTransformMatrix(r, S, i), this.simpleCount++, this.instanceCount++, this.instanceCount >= this.maxInstances && this.endBatch();
      }
    }
  }, {
    key: "_getVertTypeForShape",
    value: function(r, n) {
      var a = r.pstyle(n).value;
      switch (a) {
        case "rectangle":
          return Wn;
        case "ellipse":
          return Ia;
        case "roundrectangle":
        case "round-rectangle":
          return Gi;
        case "bottom-round-rectangle":
          return La;
        default:
          return;
      }
    }
  }, {
    key: "_getCornerRadius",
    value: function(r, n, a) {
      var i = a.w, s = a.h;
      if (r.pstyle(n).value === "auto")
        return sn(i, s);
      var o = r.pstyle(n).pfValue, l = i / 2, u = s / 2;
      return Math.min(o, u, l);
    }
    /**
     * Only supports drawing triangles at the moment.
     */
  }, {
    key: "drawEdgeArrow",
    value: function(r, n, a) {
      if (r.visible()) {
        var i = r._private.rscratch, s, o, l;
        if (a === "source" ? (s = i.arrowStartX, o = i.arrowStartY, l = i.srcArrowAngle) : (s = i.arrowEndX, o = i.arrowEndY, l = i.tgtArrowAngle), !(isNaN(s) || s == null || isNaN(o) || o == null || isNaN(l) || l == null)) {
          var u = r.pstyle(a + "-arrow-shape").value;
          if (u !== "none") {
            var f = r.pstyle(a + "-arrow-color").value, c = r.pstyle("opacity").value, v = r.pstyle("line-opacity").value, d = c * v, h = r.pstyle("width").pfValue, y = r.pstyle("arrow-scale").value, g = this.r.getArrowWidth(h, y), p = this.instanceCount, m = this.transformBuffer.getMatrixView(p);
            xd(m), os(m, m, [s, o]), Su(m, m, [g, g]), Ed(m, m, l), this.vertTypeBuffer.getView(p)[0] = Yl;
            var b = this.indexBuffer.getView(p);
            Gn(n, b);
            var w = this.colorBuffer.getView(p);
            bn(f, d, w), this.instanceCount++, this.instanceCount >= this.maxInstances && this.endBatch();
          }
        }
      }
    }
    /**
     * Draw straight-line or bezier curve edges.
     */
  }, {
    key: "drawEdgeLine",
    value: function(r, n) {
      if (r.visible()) {
        var a = this._getEdgePoints(r);
        if (a) {
          var i = r.pstyle("opacity").value, s = r.pstyle("line-opacity").value, o = r.pstyle("width").pfValue, l = r.pstyle("line-color").value, u = i * s;
          if (a.length / 2 + this.instanceCount > this.maxInstances && this.endBatch(), a.length == 4) {
            var f = this.instanceCount;
            this.vertTypeBuffer.getView(f)[0] = Cd;
            var c = this.indexBuffer.getView(f);
            Gn(n, c);
            var v = this.colorBuffer.getView(f);
            bn(l, u, v);
            var d = this.lineWidthBuffer.getView(f);
            d[0] = o;
            var h = this.pointAPointBBuffer.getView(f);
            h[0] = a[0], h[1] = a[1], h[2] = a[2], h[3] = a[3], this.instanceCount++, this.instanceCount >= this.maxInstances && this.endBatch();
          } else
            for (var y = 0; y < a.length - 2; y += 2) {
              var g = this.instanceCount;
              this.vertTypeBuffer.getView(g)[0] = Td;
              var p = this.indexBuffer.getView(g);
              Gn(n, p);
              var m = this.colorBuffer.getView(g);
              bn(l, u, m);
              var b = this.lineWidthBuffer.getView(g);
              b[0] = o;
              var w = a[y - 2], E = a[y - 1], T = a[y], x = a[y + 1], S = a[y + 2], D = a[y + 3], A = a[y + 4], k = a[y + 5];
              y == 0 && (w = 2 * T - S + 1e-3, E = 2 * x - D + 1e-3), y == a.length - 4 && (A = 2 * S - T + 1e-3, k = 2 * D - x + 1e-3);
              var R = this.pointAPointBBuffer.getView(g);
              R[0] = w, R[1] = E, R[2] = T, R[3] = x;
              var M = this.pointCPointDBuffer.getView(g);
              M[0] = S, M[1] = D, M[2] = A, M[3] = k, this.instanceCount++, this.instanceCount >= this.maxInstances && this.endBatch();
            }
        }
      }
    }
  }, {
    key: "_isValidEdge",
    value: function(r) {
      var n = r._private.rscratch;
      return !(n.badLine || n.allpts == null || isNaN(n.allpts[0]));
    }
  }, {
    key: "_getEdgePoints",
    value: function(r) {
      var n = r._private.rscratch;
      if (this._isValidEdge(r)) {
        var a = n.allpts;
        if (a.length == 4)
          return a;
        var i = this._getNumSegments(r);
        return this._getCurveSegmentPoints(a, i);
      }
    }
  }, {
    key: "_getNumSegments",
    value: function(r) {
      var n = 15;
      return Math.min(Math.max(n, 5), this.maxInstances);
    }
  }, {
    key: "_getCurveSegmentPoints",
    value: function(r, n) {
      if (r.length == 4)
        return r;
      for (var a = Array((n + 1) * 2), i = 0; i <= n; i++)
        if (i == 0)
          a[0] = r[0], a[1] = r[1];
        else if (i == n)
          a[i * 2] = r[r.length - 2], a[i * 2 + 1] = r[r.length - 1];
        else {
          var s = i / n;
          this._setCurvePoint(r, s, a, i * 2);
        }
      return a;
    }
  }, {
    key: "_setCurvePoint",
    value: function(r, n, a, i) {
      if (r.length <= 2)
        a[i] = r[0], a[i + 1] = r[1];
      else {
        for (var s = Array(r.length - 2), o = 0; o < s.length; o += 2) {
          var l = (1 - n) * r[o] + n * r[o + 2], u = (1 - n) * r[o + 1] + n * r[o + 3];
          s[o] = l, s[o + 1] = u;
        }
        return this._setCurvePoint(s, n, a, i);
      }
    }
  }, {
    key: "endBatch",
    value: function() {
      var r = this.gl, n = this.vao, a = this.vertexCount, i = this.instanceCount;
      if (i !== 0) {
        var s = this.renderTarget.picking ? this.pickingProgram : this.program;
        r.useProgram(s), r.bindVertexArray(n);
        var o = Gt(this.buffers), l;
        try {
          for (o.s(); !(l = o.n()).done; ) {
            var u = l.value;
            u.bufferSubData(i);
          }
        } catch (h) {
          o.e(h);
        } finally {
          o.f();
        }
        for (var f = this.batchManager.getAtlases(), c = 0; c < f.length; c++)
          f[c].bufferIfNeeded(r);
        for (var v = 0; v < f.length; v++)
          r.activeTexture(r.TEXTURE0 + v), r.bindTexture(r.TEXTURE_2D, f[v].texture), r.uniform1i(s.uTextures[v], v);
        r.uniform1f(s.uZoom, lC(this.r)), r.uniformMatrix3fv(s.uPanZoomMatrix, !1, this.panZoomMatrix), r.uniform1i(s.uAtlasSize, this.batchManager.getAtlasSize());
        var d = bn(this.bgColor, 1);
        r.uniform4fv(s.uBGColor, d), r.drawArraysInstanced(r.TRIANGLES, 0, a, i), r.bindVertexArray(null), r.bindTexture(r.TEXTURE_2D, null), this.debug && this.batchDebugInfo.push({
          count: i,
          // instance count
          atlasCount: f.length
        }), this.startBatch();
      }
    }
  }, {
    key: "getDebugInfo",
    value: function() {
      var r = this.atlasManager.getDebugInfo(), n = r.reduce(function(s, o) {
        return s + o.atlasCount;
      }, 0), a = this.batchDebugInfo, i = a.reduce(function(s, o) {
        return s + o.count;
      }, 0);
      return {
        atlasInfo: r,
        totalAtlases: n,
        wrappedCount: this.wrappedCount,
        simpleCount: this.simpleCount,
        batchCount: a.length,
        batchInfo: a,
        totalInstances: i
      };
    }
  }]);
})(), yp = {};
yp.initWebgl = function(t, e) {
  var r = this, n = r.data.contexts[r.WEBGL];
  t.bgColor = BC(r), t.webglTexSize = Math.min(t.webglTexSize, n.getParameter(n.MAX_TEXTURE_SIZE)), t.webglTexRows = Math.min(t.webglTexRows, 54), t.webglTexRowsNodes = Math.min(t.webglTexRowsNodes, 54), t.webglBatchSize = Math.min(t.webglBatchSize, 16384), t.webglTexPerBatch = Math.min(t.webglTexPerBatch, n.getParameter(n.MAX_TEXTURE_IMAGE_UNITS)), r.webglDebug = t.webglDebug, r.webglDebugShowAtlases = t.webglDebugShowAtlases, r.pickingFrameBuffer = yC(n), r.pickingFrameBuffer.needsDraw = !0, r.drawing = new kC(r, n, t);
  var a = function(c) {
    return function(v) {
      return r.getTextAngle(v, c);
    };
  }, i = function(c) {
    return function(v) {
      var d = v.pstyle(c);
      return d && d.value;
    };
  }, s = function(c) {
    return function(v) {
      return v.pstyle("".concat(c, "-opacity")).value > 0;
    };
  }, o = function(c) {
    var v = c.pstyle("text-events").strValue === "yes";
    return v ? ks.USE_BB : ks.IGNORE;
  }, l = function(c) {
    var v = c.position(), d = v.x, h = v.y, y = c.outerWidth(), g = c.outerHeight();
    return {
      w: y,
      h: g,
      x1: d - y / 2,
      y1: h - g / 2
    };
  };
  r.drawing.addAtlasCollection("node", {
    texRows: t.webglTexRowsNodes
  }), r.drawing.addAtlasCollection("label", {
    texRows: t.webglTexRows
  }), r.drawing.addTextureAtlasRenderType("node-body", {
    collection: "node",
    getKey: e.getStyleKey,
    getBoundingBox: e.getElementBox,
    drawElement: e.drawElement
  }), r.drawing.addSimpleShapeRenderType("node-body", {
    getBoundingBox: l,
    isSimple: fC,
    shapeProps: {
      shape: "shape",
      color: "background-color",
      opacity: "background-opacity",
      radius: "corner-radius",
      border: !0
    }
  }), r.drawing.addSimpleShapeRenderType("node-overlay", {
    getBoundingBox: l,
    isVisible: s("overlay"),
    shapeProps: {
      shape: "overlay-shape",
      color: "overlay-color",
      opacity: "overlay-opacity",
      padding: "overlay-padding",
      radius: "overlay-corner-radius"
    }
  }), r.drawing.addSimpleShapeRenderType("node-underlay", {
    getBoundingBox: l,
    isVisible: s("underlay"),
    shapeProps: {
      shape: "underlay-shape",
      color: "underlay-color",
      opacity: "underlay-opacity",
      padding: "underlay-padding",
      radius: "underlay-corner-radius"
    }
  }), r.drawing.addTextureAtlasRenderType("label", {
    // node label or edge mid label
    collection: "label",
    getTexPickingMode: o,
    getKey: Xl(e.getLabelKey, null),
    getBoundingBox: Zl(e.getLabelBox, null),
    drawClipped: !0,
    drawElement: e.drawLabel,
    getRotation: a(null),
    getRotationPoint: e.getLabelRotationPoint,
    getRotationOffset: e.getLabelRotationOffset,
    isVisible: i("label")
  }), r.drawing.addTextureAtlasRenderType("edge-source-label", {
    collection: "label",
    getTexPickingMode: o,
    getKey: Xl(e.getSourceLabelKey, "source"),
    getBoundingBox: Zl(e.getSourceLabelBox, "source"),
    drawClipped: !0,
    drawElement: e.drawSourceLabel,
    getRotation: a("source"),
    getRotationPoint: e.getSourceLabelRotationPoint,
    getRotationOffset: e.getSourceLabelRotationOffset,
    isVisible: i("source-label")
  }), r.drawing.addTextureAtlasRenderType("edge-target-label", {
    collection: "label",
    getTexPickingMode: o,
    getKey: Xl(e.getTargetLabelKey, "target"),
    getBoundingBox: Zl(e.getTargetLabelBox, "target"),
    drawClipped: !0,
    drawElement: e.drawTargetLabel,
    getRotation: a("target"),
    getRotationPoint: e.getTargetLabelRotationPoint,
    getRotationOffset: e.getTargetLabelRotationOffset,
    isVisible: i("target-label")
  });
  var u = wi(function() {
    console.log("garbage collect flag set"), r.data.gc = !0;
  }, 1e4);
  r.onUpdateEleCalcs(function(f, c) {
    var v = !1;
    c && c.length > 0 && (v |= r.drawing.invalidate(c)), v && u();
  }), RC(r);
};
function BC(t) {
  var e = t.cy.container(), r = e && e.style && e.style.backgroundColor || "white";
  return Uh(r);
}
function mp(t, e) {
  var r = t._private.rscratch;
  return Nt(r, "labelWrapCachedLines", e) || [];
}
var Xl = function(e, r) {
  return function(n) {
    var a = e(n), i = mp(n, r);
    return i.length > 1 ? i.map(function(s, o) {
      return "".concat(a, "_").concat(o);
    }) : a;
  };
}, Zl = function(e, r) {
  return function(n, a) {
    var i = e(n);
    if (typeof a == "string") {
      var s = a.indexOf("_");
      if (s > 0) {
        var o = Number(a.substring(s + 1)), l = mp(n, r), u = i.h / l.length, f = u * o, c = i.y1 + f;
        return {
          x1: i.x1,
          w: i.w,
          y1: c,
          h: u,
          yOffset: f
        };
      }
    }
    return i;
  };
};
function RC(t) {
  {
    var e = t.render;
    t.render = function(i) {
      i = i || {};
      var s = t.cy;
      t.webgl && (s.zoom() > fp ? (MC(t), e.call(t, i)) : (LC(t), wp(t, i, Ya.SCREEN)));
    };
  }
  {
    var r = t.matchCanvasSize;
    t.matchCanvasSize = function(i) {
      r.call(t, i), t.pickingFrameBuffer.setFramebufferAttachmentSizes(t.canvasWidth, t.canvasHeight), t.pickingFrameBuffer.needsDraw = !0;
    };
  }
  t.findNearestElements = function(i, s, o, l) {
    return zC(t, i, s);
  };
  {
    var n = t.invalidateCachedZSortedEles;
    t.invalidateCachedZSortedEles = function() {
      n.call(t), t.pickingFrameBuffer.needsDraw = !0;
    };
  }
  {
    var a = t.notify;
    t.notify = function(i, s) {
      a.call(t, i, s), i === "viewport" || i === "bounds" ? t.pickingFrameBuffer.needsDraw = !0 : i === "background" && t.drawing.invalidate(s, {
        type: "node-body"
      });
    };
  }
}
function MC(t) {
  var e = t.data.contexts[t.WEBGL];
  e.clear(e.COLOR_BUFFER_BIT | e.DEPTH_BUFFER_BIT);
}
function LC(t) {
  var e = function(n) {
    n.save(), n.setTransform(1, 0, 0, 1, 0, 0), n.clearRect(0, 0, t.canvasWidth, t.canvasHeight), n.restore();
  };
  e(t.data.contexts[t.NODE]), e(t.data.contexts[t.DRAG]);
}
function IC(t) {
  var e = t.canvasWidth, r = t.canvasHeight, n = pf(t), a = n.pan, i = n.zoom, s = Wl();
  os(s, s, [a.x, a.y]), Su(s, s, [i, i]);
  var o = Wl();
  bC(o, e, r);
  var l = Wl();
  return mC(l, o, s), l;
}
function bp(t, e) {
  var r = t.canvasWidth, n = t.canvasHeight, a = pf(t), i = a.pan, s = a.zoom;
  e.setTransform(1, 0, 0, 1, 0, 0), e.clearRect(0, 0, r, n), e.translate(i.x, i.y), e.scale(s, s);
}
function OC(t, e) {
  t.drawSelectionRectangle(e, function(r) {
    return bp(t, r);
  });
}
function NC(t) {
  var e = t.data.contexts[t.NODE];
  e.save(), bp(t, e), e.strokeStyle = "rgba(0, 0, 0, 0.3)", e.beginPath(), e.moveTo(-1e3, 0), e.lineTo(1e3, 0), e.stroke(), e.beginPath(), e.moveTo(0, -1e3), e.lineTo(0, 1e3), e.stroke(), e.restore();
}
function _C(t) {
  var e = function(a, i, s) {
    for (var o = a.atlasManager.getAtlasCollection(i), l = t.data.contexts[t.NODE], u = o.atlases, f = 0; f < u.length; f++) {
      var c = u[f], v = c.canvas;
      if (v) {
        var d = v.width, h = v.height, y = d * f, g = v.height * s, p = 0.4;
        l.save(), l.scale(p, p), l.drawImage(v, y, g), l.strokeStyle = "black", l.rect(y, g, d, h), l.stroke(), l.restore();
      }
    }
  }, r = 0;
  e(t.drawing, "node", r++), e(t.drawing, "label", r++);
}
function FC(t, e, r, n, a) {
  var i, s, o, l, u = pf(t), f = u.pan, c = u.zoom;
  {
    var v = uC(t, f, c, e, r), d = ut(v, 2), h = d[0], y = d[1], g = 6;
    i = h - g / 2, s = y - g / 2, o = g, l = g;
  }
  if (o === 0 || l === 0)
    return [];
  var p = t.data.contexts[t.WEBGL];
  p.bindFramebuffer(p.FRAMEBUFFER, t.pickingFrameBuffer), t.pickingFrameBuffer.needsDraw && (p.viewport(0, 0, p.canvas.width, p.canvas.height), wp(t, null, Ya.PICKING), t.pickingFrameBuffer.needsDraw = !1);
  var m = o * l, b = new Uint8Array(m * 4);
  p.readPixels(i, s, o, l, p.RGBA, p.UNSIGNED_BYTE, b), p.bindFramebuffer(p.FRAMEBUFFER, null);
  for (var w = /* @__PURE__ */ new Set(), E = 0; E < m; E++) {
    var T = b.slice(E * 4, E * 4 + 4), x = vC(T) - 1;
    x >= 0 && w.add(x);
  }
  return w;
}
function zC(t, e, r) {
  var n = FC(t, e, r), a = t.getCachedZSortedEles(), i, s, o = Gt(n), l;
  try {
    for (o.s(); !(l = o.n()).done; ) {
      var u = l.value, f = a[u];
      if (!i && f.isNode() && (i = f), !s && f.isEdge() && (s = f), i && s)
        break;
    }
  } catch (c) {
    o.e(c);
  } finally {
    o.f();
  }
  return [i, s].filter(Boolean);
}
function Ql(t, e, r) {
  var n = t.drawing;
  e += 1, r.isNode() ? (n.drawNode(r, e, "node-underlay"), n.drawNode(r, e, "node-body"), n.drawTexture(r, e, "label"), n.drawNode(r, e, "node-overlay")) : (n.drawEdgeLine(r, e), n.drawEdgeArrow(r, e, "source"), n.drawEdgeArrow(r, e, "target"), n.drawTexture(r, e, "label"), n.drawTexture(r, e, "edge-source-label"), n.drawTexture(r, e, "edge-target-label"));
}
function wp(t, e, r) {
  var n;
  t.webglDebug && (n = performance.now());
  var a = t.drawing, i = 0;
  if (r.screen && t.data.canvasNeedsRedraw[t.SELECT_BOX] && OC(t, e), t.data.canvasNeedsRedraw[t.NODE] || r.picking) {
    var s = t.data.contexts[t.WEBGL];
    r.screen ? (s.clearColor(0, 0, 0, 0), s.enable(s.BLEND), s.blendFunc(s.ONE, s.ONE_MINUS_SRC_ALPHA)) : s.disable(s.BLEND), s.clear(s.COLOR_BUFFER_BIT | s.DEPTH_BUFFER_BIT), s.viewport(0, 0, s.canvas.width, s.canvas.height);
    var o = IC(t), l = t.getCachedZSortedEles();
    if (i = l.length, a.startFrame(o, r), r.screen) {
      for (var u = 0; u < l.nondrag.length; u++)
        Ql(t, u, l.nondrag[u]);
      for (var f = 0; f < l.drag.length; f++)
        Ql(t, f, l.drag[f]);
    } else if (r.picking)
      for (var c = 0; c < l.length; c++)
        Ql(t, c, l[c]);
    a.endFrame(), r.screen && t.webglDebugShowAtlases && (NC(t), _C(t)), t.data.canvasNeedsRedraw[t.NODE] = !1, t.data.canvasNeedsRedraw[t.DRAG] = !1;
  }
  if (t.webglDebug) {
    var v = performance.now(), d = !1, h = Math.ceil(v - n), y = a.getDebugInfo(), g = ["".concat(i, " elements"), "".concat(y.totalInstances, " instances"), "".concat(y.batchCount, " batches"), "".concat(y.totalAtlases, " atlases"), "".concat(y.wrappedCount, " wrapped textures"), "".concat(y.simpleCount, " simple shapes")].join(", ");
    if (d)
      console.log("WebGL (".concat(r.name, ") - time ").concat(h, "ms, ").concat(g));
    else {
      console.log("WebGL (".concat(r.name, ") - frame time ").concat(h, "ms")), console.log("Totals:"), console.log("  ".concat(g)), console.log("Texture Atlases Used:");
      var p = y.atlasInfo, m = Gt(p), b;
      try {
        for (m.s(); !(b = m.n()).done; ) {
          var w = b.value;
          console.log("  ".concat(w.type, ": ").concat(w.keyCount, " keys, ").concat(w.atlasCount, " atlases"));
        }
      } catch (E) {
        m.e(E);
      } finally {
        m.f();
      }
      console.log("");
    }
  }
  t.data.gc && (console.log("Garbage Collect!"), t.data.gc = !1, a.gc());
}
var hn = {};
hn.drawPolygonPath = function(t, e, r, n, a, i) {
  var s = n / 2, o = a / 2;
  t.beginPath && t.beginPath(), t.moveTo(e + s * i[0], r + o * i[1]);
  for (var l = 1; l < i.length / 2; l++)
    t.lineTo(e + s * i[l * 2], r + o * i[l * 2 + 1]);
  t.closePath();
};
hn.drawRoundPolygonPath = function(t, e, r, n, a, i, s) {
  s.forEach(function(o) {
    return ep(t, o);
  }), t.closePath();
};
hn.drawRoundRectanglePath = function(t, e, r, n, a, i) {
  var s = n / 2, o = a / 2, l = i === "auto" ? sn(n, a) : Math.min(i, o, s);
  t.beginPath && t.beginPath(), t.moveTo(e, r - o), t.arcTo(e + s, r - o, e + s, r, l), t.arcTo(e + s, r + o, e, r + o, l), t.arcTo(e - s, r + o, e - s, r, l), t.arcTo(e - s, r - o, e, r - o, l), t.lineTo(e, r - o), t.closePath();
};
hn.drawBottomRoundRectanglePath = function(t, e, r, n, a, i) {
  var s = n / 2, o = a / 2, l = i === "auto" ? sn(n, a) : i;
  t.beginPath && t.beginPath(), t.moveTo(e, r - o), t.lineTo(e + s, r - o), t.lineTo(e + s, r), t.arcTo(e + s, r + o, e, r + o, l), t.arcTo(e - s, r + o, e - s, r, l), t.lineTo(e - s, r - o), t.lineTo(e, r - o), t.closePath();
};
hn.drawCutRectanglePath = function(t, e, r, n, a, i, s) {
  var o = n / 2, l = a / 2, u = s === "auto" ? tf() : s;
  t.beginPath && t.beginPath(), t.moveTo(e - o + u, r - l), t.lineTo(e + o - u, r - l), t.lineTo(e + o, r - l + u), t.lineTo(e + o, r + l - u), t.lineTo(e + o - u, r + l), t.lineTo(e - o + u, r + l), t.lineTo(e - o, r + l - u), t.lineTo(e - o, r - l + u), t.closePath();
};
hn.drawBarrelPath = function(t, e, r, n, a) {
  var i = n / 2, s = a / 2, o = e - i, l = e + i, u = r - s, f = r + s, c = cu(n, a), v = c.widthOffset, d = c.heightOffset, h = c.ctrlPtOffsetPct * v;
  t.beginPath && t.beginPath(), t.moveTo(o, u + d), t.lineTo(o, f - d), t.quadraticCurveTo(o + h, f, o + v, f), t.lineTo(l - v, f), t.quadraticCurveTo(l - h, f, l, f - d), t.lineTo(l, u + d), t.quadraticCurveTo(l - h, u, l - v, u), t.lineTo(o + v, u), t.quadraticCurveTo(o + h, u, o, u + d), t.closePath();
};
var Sd = Math.sin(0), Pd = Math.cos(0), Pu = {}, Du = {}, xp = Math.PI / 40;
for (var Kn = 0 * Math.PI; Kn < 2 * Math.PI; Kn += xp)
  Pu[Kn] = Math.sin(Kn), Du[Kn] = Math.cos(Kn);
hn.drawEllipsePath = function(t, e, r, n, a) {
  if (t.beginPath && t.beginPath(), t.ellipse)
    t.ellipse(e, r, n / 2, a / 2, 0, 0, 2 * Math.PI);
  else
    for (var i, s, o = n / 2, l = a / 2, u = 0 * Math.PI; u < 2 * Math.PI; u += xp)
      i = e - o * Pu[u] * Sd + o * Du[u] * Pd, s = r + l * Du[u] * Sd + l * Pu[u] * Pd, u === 0 ? t.moveTo(i, s) : t.lineTo(i, s);
  t.closePath();
};
var Pi = {};
Pi.createBuffer = function(t, e) {
  var r = document.createElement("canvas");
  return r.width = t, r.height = e, [r, r.getContext("2d")];
};
Pi.bufferCanvasImage = function(t) {
  var e = this.cy, r = e.mutableElements(), n = r.boundingBox(), a = this.findContainerClientCoords(), i = t.full ? Math.ceil(n.w) : a[2], s = t.full ? Math.ceil(n.h) : a[3], o = pe(t.maxWidth) || pe(t.maxHeight), l = this.getPixelRatio(), u = 1;
  if (t.scale !== void 0)
    i *= t.scale, s *= t.scale, u = t.scale;
  else if (o) {
    var f = 1 / 0, c = 1 / 0;
    pe(t.maxWidth) && (f = u * t.maxWidth / i), pe(t.maxHeight) && (c = u * t.maxHeight / s), u = Math.min(f, c), i *= u, s *= u;
  }
  o || (i *= l, s *= l, u *= l);
  var v = document.createElement("canvas");
  v.width = i, v.height = s, v.style.width = i + "px", v.style.height = s + "px";
  var d = v.getContext("2d");
  if (i > 0 && s > 0) {
    d.clearRect(0, 0, i, s), d.globalCompositeOperation = "source-over";
    var h = this.getCachedZSortedEles();
    if (t.full)
      d.translate(-n.x1 * u, -n.y1 * u), d.scale(u, u), this.drawElements(d, h), d.scale(1 / u, 1 / u), d.translate(n.x1 * u, n.y1 * u);
    else {
      var y = e.pan(), g = {
        x: y.x * u,
        y: y.y * u
      };
      u *= e.zoom(), d.translate(g.x, g.y), d.scale(u, u), this.drawElements(d, h), d.scale(1 / u, 1 / u), d.translate(-g.x, -g.y);
    }
    t.bg && (d.globalCompositeOperation = "destination-over", d.fillStyle = t.bg, d.rect(0, 0, i, s), d.fill());
  }
  return v;
};
function VC(t, e) {
  for (var r = atob(t), n = new ArrayBuffer(r.length), a = new Uint8Array(n), i = 0; i < r.length; i++)
    a[i] = r.charCodeAt(i);
  return new Blob([n], {
    type: e
  });
}
function Dd(t) {
  var e = t.indexOf(",");
  return t.substr(e + 1);
}
function Ep(t, e, r) {
  var n = function() {
    return e.toDataURL(r, t.quality);
  };
  switch (t.output) {
    case "blob-promise":
      return new ya(function(a, i) {
        try {
          e.toBlob(function(s) {
            s != null ? a(s) : i(new Error("`canvas.toBlob()` sent a null value in its callback"));
          }, r, t.quality);
        } catch (s) {
          i(s);
        }
      });
    case "blob":
      return VC(Dd(n()), r);
    case "base64":
      return Dd(n());
    case "base64uri":
    default:
      return n();
  }
}
Pi.png = function(t) {
  return Ep(t, this.bufferCanvasImage(t), "image/png");
};
Pi.jpg = function(t) {
  return Ep(t, this.bufferCanvasImage(t), "image/jpeg");
};
var Cp = {};
Cp.nodeShapeImpl = function(t, e, r, n, a, i, s, o) {
  switch (t) {
    case "ellipse":
      return this.drawEllipsePath(e, r, n, a, i);
    case "polygon":
      return this.drawPolygonPath(e, r, n, a, i, s);
    case "round-polygon":
      return this.drawRoundPolygonPath(e, r, n, a, i, s, o);
    case "roundrectangle":
    case "round-rectangle":
      return this.drawRoundRectanglePath(e, r, n, a, i, o);
    case "cutrectangle":
    case "cut-rectangle":
      return this.drawCutRectanglePath(e, r, n, a, i, s, o);
    case "bottomroundrectangle":
    case "bottom-round-rectangle":
      return this.drawBottomRoundRectanglePath(e, r, n, a, i, o);
    case "barrel":
      return this.drawBarrelPath(e, r, n, a, i);
  }
};
var qC = Tp, Le = Tp.prototype;
Le.CANVAS_LAYERS = 3;
Le.SELECT_BOX = 0;
Le.DRAG = 1;
Le.NODE = 2;
Le.WEBGL = 3;
Le.CANVAS_TYPES = ["2d", "2d", "2d", "webgl2"];
Le.BUFFER_COUNT = 3;
Le.TEXTURE_BUFFER = 0;
Le.MOTIONBLUR_BUFFER_NODE = 1;
Le.MOTIONBLUR_BUFFER_DRAG = 2;
function Tp(t) {
  var e = this, r = e.cy.window(), n = r.document;
  t.webgl && (Le.CANVAS_LAYERS = e.CANVAS_LAYERS = 4, console.log("webgl rendering enabled")), e.data = {
    canvases: new Array(Le.CANVAS_LAYERS),
    contexts: new Array(Le.CANVAS_LAYERS),
    canvasNeedsRedraw: new Array(Le.CANVAS_LAYERS),
    bufferCanvases: new Array(Le.BUFFER_COUNT),
    bufferContexts: new Array(Le.CANVAS_LAYERS)
  };
  var a = "-webkit-tap-highlight-color", i = "rgba(0,0,0,0)";
  e.data.canvasContainer = n.createElement("div");
  var s = e.data.canvasContainer.style;
  e.data.canvasContainer.style[a] = i, s.position = "relative", s.zIndex = "0", s.overflow = "hidden";
  var o = t.cy.container();
  o.appendChild(e.data.canvasContainer), o.style[a] = i;
  var l = {
    "-webkit-user-select": "none",
    "-moz-user-select": "-moz-none",
    "user-select": "none",
    "-webkit-tap-highlight-color": "rgba(0,0,0,0)",
    "outline-style": "none"
  };
  M0() && (l["-ms-touch-action"] = "none", l["touch-action"] = "none");
  for (var u = 0; u < Le.CANVAS_LAYERS; u++) {
    var f = e.data.canvases[u] = n.createElement("canvas"), c = Le.CANVAS_TYPES[u];
    e.data.contexts[u] = f.getContext(c), e.data.contexts[u] || je("Could not create canvas of type " + c), Object.keys(l).forEach(function(ie) {
      f.style[ie] = l[ie];
    }), f.style.position = "absolute", f.setAttribute("data-id", "layer" + u), f.style.zIndex = String(Le.CANVAS_LAYERS - u), e.data.canvasContainer.appendChild(f), e.data.canvasNeedsRedraw[u] = !1;
  }
  e.data.topCanvas = e.data.canvases[0], e.data.canvases[Le.NODE].setAttribute("data-id", "layer" + Le.NODE + "-node"), e.data.canvases[Le.SELECT_BOX].setAttribute("data-id", "layer" + Le.SELECT_BOX + "-selectbox"), e.data.canvases[Le.DRAG].setAttribute("data-id", "layer" + Le.DRAG + "-drag"), e.data.canvases[Le.WEBGL] && e.data.canvases[Le.WEBGL].setAttribute("data-id", "layer" + Le.WEBGL + "-webgl");
  for (var u = 0; u < Le.BUFFER_COUNT; u++)
    e.data.bufferCanvases[u] = n.createElement("canvas"), e.data.bufferContexts[u] = e.data.bufferCanvases[u].getContext("2d"), e.data.bufferCanvases[u].style.position = "absolute", e.data.bufferCanvases[u].setAttribute("data-id", "buffer" + u), e.data.bufferCanvases[u].style.zIndex = String(-u - 1), e.data.bufferCanvases[u].style.visibility = "hidden";
  e.pathsEnabled = !0;
  var v = zt(), d = function(U) {
    return {
      x: (U.x1 + U.x2) / 2,
      y: (U.y1 + U.y2) / 2
    };
  }, h = function(U) {
    return {
      x: -U.w / 2,
      y: -U.h / 2
    };
  }, y = function(U) {
    var X = U[0]._private, C = X.oldBackgroundTimestamp === X.backgroundTimestamp;
    return !C;
  }, g = function(U) {
    return U[0]._private.nodeKey;
  }, p = function(U) {
    return U[0]._private.labelStyleKey;
  }, m = function(U) {
    return U[0]._private.sourceLabelStyleKey;
  }, b = function(U) {
    return U[0]._private.targetLabelStyleKey;
  }, w = function(U, X, C, B, z) {
    return e.drawElement(U, X, C, !1, !1, z);
  }, E = function(U, X, C, B, z) {
    return e.drawElementText(U, X, C, B, "main", z);
  }, T = function(U, X, C, B, z) {
    return e.drawElementText(U, X, C, B, "source", z);
  }, x = function(U, X, C, B, z) {
    return e.drawElementText(U, X, C, B, "target", z);
  }, S = function(U) {
    return U.boundingBox(), U[0]._private.bodyBounds;
  }, D = function(U) {
    return U.boundingBox(), U[0]._private.labelBounds.main || v;
  }, A = function(U) {
    return U.boundingBox(), U[0]._private.labelBounds.source || v;
  }, k = function(U) {
    return U.boundingBox(), U[0]._private.labelBounds.target || v;
  }, R = function(U, X) {
    return X;
  }, M = function(U) {
    return d(S(U));
  }, I = function(U, X, C) {
    var B = U ? U + "-" : "";
    return {
      x: X.x + C.pstyle(B + "text-margin-x").pfValue,
      y: X.y + C.pstyle(B + "text-margin-y").pfValue
    };
  }, _ = function(U, X, C) {
    var B = U[0]._private.rscratch;
    return {
      x: B[X],
      y: B[C]
    };
  }, N = function(U) {
    return I("", _(U, "labelX", "labelY"), U);
  }, L = function(U) {
    return I("source", _(U, "sourceLabelX", "sourceLabelY"), U);
  }, F = function(U) {
    return I("target", _(U, "targetLabelX", "targetLabelY"), U);
  }, H = function(U) {
    return h(S(U));
  }, V = function(U) {
    return h(A(U));
  }, O = function(U) {
    return h(k(U));
  }, $ = function(U) {
    var X = D(U), C = h(D(U));
    if (U.isNode()) {
      switch (ha(U.pstyle("text-halign").value)) {
        case "left":
          C.x = -X.w - (X.leftPad || 0);
          break;
        case "right":
          C.x = -(X.rightPad || 0);
          break;
      }
      switch (ga(U.pstyle("text-valign").value)) {
        case "top":
          C.y = -X.h - (X.topPad || 0);
          break;
        case "bottom":
          C.y = -(X.botPad || 0);
          break;
      }
    }
    return C;
  }, Q = e.data.eleTxrCache = new za(e, {
    getKey: g,
    doesEleInvalidateKey: y,
    drawElement: w,
    getBoundingBox: S,
    getRotationPoint: M,
    getRotationOffset: H,
    allowEdgeTxrCaching: !1,
    allowParentTxrCaching: !1
  }), se = e.data.lblTxrCache = new za(e, {
    getKey: p,
    drawElement: E,
    getBoundingBox: D,
    getRotationPoint: N,
    getRotationOffset: $,
    isVisible: R
  }), ae = e.data.slbTxrCache = new za(e, {
    getKey: m,
    drawElement: T,
    getBoundingBox: A,
    getRotationPoint: L,
    getRotationOffset: V,
    isVisible: R
  }), le = e.data.tlbTxrCache = new za(e, {
    getKey: b,
    drawElement: x,
    getBoundingBox: k,
    getRotationPoint: F,
    getRotationOffset: O,
    isVisible: R
  }), ce = e.data.lyrTxrCache = new cp(e);
  e.onUpdateEleCalcs(function(U, X) {
    Q.invalidateElements(X), se.invalidateElements(X), ae.invalidateElements(X), le.invalidateElements(X), ce.invalidateElements(X);
    for (var C = 0; C < X.length; C++) {
      var B = X[C]._private;
      B.oldBackgroundTimestamp = B.backgroundTimestamp;
    }
  });
  var he = function(U) {
    for (var X = 0; X < U.length; X++)
      ce.enqueueElementRefinement(U[X].ele);
  };
  Q.onDequeue(he), se.onDequeue(he), ae.onDequeue(he), le.onDequeue(he), t.webgl && e.initWebgl(t, {
    getStyleKey: g,
    getLabelKey: p,
    getSourceLabelKey: m,
    getTargetLabelKey: b,
    drawElement: w,
    drawLabel: E,
    drawSourceLabel: T,
    drawTargetLabel: x,
    getElementBox: S,
    getLabelBox: D,
    getSourceLabelBox: A,
    getTargetLabelBox: k,
    getElementRotationPoint: M,
    getElementRotationOffset: H,
    getLabelRotationPoint: N,
    getSourceLabelRotationPoint: L,
    getTargetLabelRotationPoint: F,
    getLabelRotationOffset: $,
    getSourceLabelRotationOffset: V,
    getTargetLabelRotationOffset: O
  });
}
Le.redrawHint = function(t, e) {
  var r = this;
  switch (t) {
    case "eles":
      r.data.canvasNeedsRedraw[Le.NODE] = e;
      break;
    case "drag":
      r.data.canvasNeedsRedraw[Le.DRAG] = e;
      break;
    case "select":
      r.data.canvasNeedsRedraw[Le.SELECT_BOX] = e;
      break;
    case "gc":
      r.data.gc = !0;
      break;
  }
};
var $C = typeof Path2D < "u";
Le.path2dEnabled = function(t) {
  if (t === void 0)
    return this.pathsEnabled;
  this.pathsEnabled = !!t;
};
Le.usePaths = function() {
  return $C && this.pathsEnabled;
};
Le.setImgSmoothing = function(t, e) {
  t.imageSmoothingEnabled != null ? t.imageSmoothingEnabled = e : (t.webkitImageSmoothingEnabled = e, t.mozImageSmoothingEnabled = e, t.msImageSmoothingEnabled = e);
};
Le.getImgSmoothing = function(t) {
  return t.imageSmoothingEnabled != null ? t.imageSmoothingEnabled : t.webkitImageSmoothingEnabled || t.mozImageSmoothingEnabled || t.msImageSmoothingEnabled;
};
Le.makeOffscreenCanvas = function(t, e) {
  var r;
  if ((typeof OffscreenCanvas > "u" ? "undefined" : dt(OffscreenCanvas)) !== "undefined")
    r = new OffscreenCanvas(t, e);
  else {
    var n = this.cy.window(), a = n.document;
    r = a.createElement("canvas"), r.width = t, r.height = e;
  }
  return r;
};
[vp, Pr, Gr, gf, Fn, dn, Vt, yp, hn, Pi, Cp].forEach(function(t) {
  Ae(Le, t);
});
var HC = [{
  name: "null",
  impl: Qg
}, {
  name: "base",
  impl: lp
}, {
  name: "canvas",
  impl: qC
}], UC = [{
  type: "layout",
  extensions: dE
}, {
  type: "renderer",
  extensions: HC
}], Sp = {}, Pp = {};
function Dp(t, e, r) {
  var n = r, a = function(S) {
    $e("Can not register `" + e + "` for `" + t + "` since `" + S + "` already exists in the prototype and can not be overridden");
  };
  if (t === "core") {
    if (ui.prototype[e])
      return a(e);
    ui.prototype[e] = r;
  } else if (t === "collection") {
    if (Ct.prototype[e])
      return a(e);
    Ct.prototype[e] = r;
  } else if (t === "layout") {
    for (var i = function(S) {
      this.options = S, r.call(this, S), _e(this._private) || (this._private = {}), this._private.cy = S.cy, this._private.listeners = [], this.createEmitter();
    }, s = i.prototype = Object.create(r.prototype), o = [], l = 0; l < o.length; l++) {
      var u = o[l];
      s[u] = s[u] || function() {
        return this;
      };
    }
    s.start && !s.run ? s.run = function() {
      return this.start(), this;
    } : !s.start && s.run && (s.start = function() {
      return this.run(), this;
    });
    var f = r.prototype.stop;
    s.stop = function() {
      var x = this.options;
      if (x && x.animate) {
        var S = this.animations;
        if (S)
          for (var D = 0; D < S.length; D++)
            S[D].stop();
      }
      return f ? f.call(this) : this.emit("layoutstop"), this;
    }, s.destroy || (s.destroy = function() {
      return this;
    }), s.cy = function() {
      return this._private.cy;
    };
    var c = function(S) {
      return S._private.cy;
    }, v = {
      addEventFields: function(S, D) {
        D.layout = S, D.cy = c(S), D.target = S;
      },
      bubble: function() {
        return !0;
      },
      parent: function(S) {
        return c(S);
      }
    };
    Ae(s, {
      createEmitter: function() {
        return this._private.emitter = new eo(v, this), this;
      },
      emitter: function() {
        return this._private.emitter;
      },
      on: function(S, D) {
        return this.emitter().on(S, D), this;
      },
      one: function(S, D) {
        return this.emitter().one(S, D), this;
      },
      once: function(S, D) {
        return this.emitter().one(S, D), this;
      },
      removeListener: function(S, D) {
        return this.emitter().removeListener(S, D), this;
      },
      removeAllListeners: function() {
        return this.emitter().removeAllListeners(), this;
      },
      emit: function(S, D) {
        return this.emitter().emit(S, D), this;
      }
    }), qe.eventAliasesOn(s), n = i;
  } else if (t === "renderer" && e !== "null" && e !== "base") {
    var d = Ap("renderer", "base"), h = d.prototype, y = r, g = r.prototype, p = function() {
      d.apply(this, arguments), y.apply(this, arguments);
    }, m = p.prototype;
    for (var b in h) {
      var w = h[b], E = g[b] != null;
      if (E)
        return a(b);
      m[b] = w;
    }
    for (var T in g)
      m[T] = g[T];
    h.clientFunctions.forEach(function(x) {
      m[x] = m[x] || function() {
        je("Renderer does not implement `renderer." + x + "()` on its prototype");
      };
    }), n = p;
  } else if (t === "__proto__" || t === "constructor" || t === "prototype")
    return je(t + " is an illegal type to be registered, possibly lead to prototype pollutions");
  return Gh({
    map: Sp,
    keys: [t, e],
    value: n
  });
}
function Ap(t, e) {
  return Wh({
    map: Sp,
    keys: [t, e]
  });
}
function GC(t, e, r, n, a) {
  return Gh({
    map: Pp,
    keys: [t, e, r, n],
    value: a
  });
}
function WC(t, e, r, n) {
  return Wh({
    map: Pp,
    keys: [t, e, r, n]
  });
}
var Au = function() {
  if (arguments.length === 2)
    return Ap.apply(null, arguments);
  if (arguments.length === 3)
    return Dp.apply(null, arguments);
  if (arguments.length === 4)
    return WC.apply(null, arguments);
  if (arguments.length === 5)
    return GC.apply(null, arguments);
  je("Invalid extension access syntax");
};
ui.prototype.extension = Au;
UC.forEach(function(t) {
  t.extensions.forEach(function(e) {
    Dp(t.type, e.name, e.impl);
  });
});
var Bs = function() {
  if (!(this instanceof Bs))
    return new Bs();
  this.length = 0;
}, In = Bs.prototype;
In.instanceString = function() {
  return "stylesheet";
};
In.selector = function(t) {
  var e = this.length++;
  return this[e] = {
    selector: t,
    properties: []
  }, this;
};
In.css = function(t, e) {
  var r = this.length - 1;
  if (Se(t))
    this[r].properties.push({
      name: t,
      value: e
    });
  else if (_e(t))
    for (var n = t, a = Object.keys(n), i = 0; i < a.length; i++) {
      var s = a[i], o = n[s];
      if (o != null) {
        var l = mt.properties[s] || mt.properties[Us(s)];
        if (l != null) {
          var u = l.name, f = o;
          this[r].properties.push({
            name: u,
            value: f
          });
        }
      }
    }
  return this;
};
In.style = In.css;
In.generateStyle = function(t) {
  var e = new mt(t);
  return this.appendToStyle(e);
};
In.appendToStyle = function(t) {
  for (var e = 0; e < this.length; e++) {
    var r = this[e], n = r.selector, a = r.properties;
    t.selector(n);
    for (var i = 0; i < a.length; i++) {
      var s = a[i];
      t.css(s.name, s.value);
    }
  }
  return t;
};
var KC = "3.34.0", On = function(e) {
  if (e === void 0 && (e = {}), _e(e))
    return new ui(e);
  if (Se(e))
    return Au.apply(Au, arguments);
};
On.use = function(t) {
  var e = Array.prototype.slice.call(arguments, 1);
  return e.unshift(On), t.apply(null, e), this;
};
On.warnings = function(t) {
  return Jh(t);
};
On.version = KC;
On.stylesheet = On.Stylesheet = Bs;
const YC = { class: "onec-tree-node" }, XC = {
  key: 1,
  class: "onec-chevron"
}, ZC = { class: "onec-node-name" }, QC = { class: "onec-kind" }, jC = {
  key: 0,
  class: "onec-node-children"
}, JC = /* @__PURE__ */ sh({
  __name: "TreeNode",
  props: {
    node: {},
    selected: {}
  },
  emits: ["select"],
  setup(t, { emit: e }) {
    const r = t, n = e, a = /* @__PURE__ */ pt(!1);
    function i() {
      n("select", r.node);
    }
    return (s, o) => {
      const l = Ky("TreeNode", !0);
      return et(), ot("div", YC, [
        Ce("button", {
          class: la(["onec-node-head", { selected: t.selected === t.node.fullName }]),
          onClick: i
        }, [
          t.node.children.length ? (et(), ot("span", {
            key: 0,
            class: "onec-chevron",
            onClick: o[0] || (o[0] = u0((u) => a.value = !a.value, ["stop"]))
          }, Qe(a.value ? "▾" : "▸"), 1)) : (et(), ot("span", XC, "·")),
          Ce("span", ZC, Qe(t.node.name), 1),
          Ce("span", QC, Qe(t.node.kind), 1)
        ], 2),
        a.value && t.node.children.length ? (et(), ot("div", jC, [
          (et(!0), ot(Wt, null, Xi(t.node.children, (u) => (et(), Uu(l, {
            key: u.fullName,
            node: u,
            selected: t.selected,
            onSelect: o[1] || (o[1] = (f) => n("select", f))
          }, null, 8, ["node", "selected"]))), 128))
        ])) : Rr("", !0)
      ]);
    };
  }
}), eT = { class: "onec-browser" }, tT = { class: "onec-sidebar" }, rT = { class: "onec-card onec-explorer" }, nT = {
  key: 0,
  class: "onec-muted onec-inline-state"
}, aT = {
  key: 1,
  class: "onec-search-results"
}, iT = ["onClick"], sT = { class: "onec-tree" }, oT = ["onClick"], lT = {
  key: 0,
  class: "onec-section-objects"
}, uT = {
  key: 0,
  class: "onec-empty"
}, fT = { class: "onec-card onec-summary" }, cT = { class: "onec-card onec-rebuild" }, vT = { class: "onec-row" }, dT = ["disabled"], hT = { class: "onec-main onec-card" }, gT = { class: "onec-object-header" }, pT = {
  key: 0,
  class: "onec-kind"
}, yT = { class: "onec-graph-controls" }, mT = ["disabled"], bT = ["disabled"], wT = {
  key: 0,
  class: "onec-object-details"
}, xT = { class: "onec-graph-status" }, ET = { key: 0 }, CT = { key: 0 }, TT = /* @__PURE__ */ sh({
  __name: "BrowserPanel",
  props: {
    api: {}
  },
  setup(t) {
    const e = t, r = /* @__PURE__ */ Qa({ objectCount: 0, relationCount: 0, sectionCount: 0 }), n = /* @__PURE__ */ pt([]), a = /* @__PURE__ */ Qa({}), i = /* @__PURE__ */ pt(""), s = /* @__PURE__ */ pt(!1), o = /* @__PURE__ */ pt([]), l = /* @__PURE__ */ pt(null), u = /* @__PURE__ */ pt(""), f = /* @__PURE__ */ pt(!1), c = /* @__PURE__ */ pt(!1), v = /* @__PURE__ */ pt(""), d = /* @__PURE__ */ pt(!1), h = /* @__PURE__ */ pt("dependencies"), y = /* @__PURE__ */ pt(2), g = /* @__PURE__ */ pt(250), p = /* @__PURE__ */ pt(!1), m = /* @__PURE__ */ pt("Select an object and load a graph."), b = /* @__PURE__ */ pt(null), w = /* @__PURE__ */ pt(null);
    let E = null, T = null, x, S = 0;
    async function D(V, O = {}) {
      const $ = await e.api.invoke("plugin.action", {
        pluginId: "onec",
        action: V,
        valueJson: JSON.stringify(O)
      });
      if (!$.ok) throw new Error($.error || `OneC action '${V}' failed.`);
      return $.resultJson ? JSON.parse($.resultJson) : null;
    }
    async function A() {
      c.value = !0;
      try {
        const V = await D("overview");
        r.objectCount = V.objectCount, r.relationCount = V.relationCount, r.sectionCount = V.sectionCount, n.value = V.sections;
        for (const O of V.sections)
          O.kind in a || (a[O.kind] = !1);
        V.treeTruncated ? F("The navigation tree is limited to 20,000 indexed objects.") : v.value.startsWith("The navigation tree") && F("");
      } catch (V) {
        F(`Unable to load the OneC index: ${H(V)}`, !0);
      } finally {
        c.value = !1;
      }
    }
    Yi(i, (V) => {
      window.clearTimeout(x);
      const O = ++S;
      x = window.setTimeout(async () => {
        if (!V.trim()) {
          o.value = [], s.value = !1;
          return;
        }
        s.value = !0;
        try {
          const $ = await D("search", { query: V });
          O === S && (o.value = $.results);
        } catch ($) {
          O === S && F(`Search failed: ${H($)}`, !0);
        } finally {
          O === S && (s.value = !1);
        }
      }, 180);
    });
    async function k(V) {
      try {
        l.value = await D("object", { fullName: V }), b.value = null, m.value = l.value ? "Choose a graph mode." : "The selected object no longer exists.", E == null || E.elements().remove();
      } catch (O) {
        F(`Unable to load the object: ${H(O)}`, !0);
      }
    }
    async function R() {
      if (l.value) {
        p.value = !0;
        try {
          const V = await D("graph", {
            fullName: l.value.fullName,
            mode: h.value,
            depth: y.value,
            limit: g.value
          });
          b.value = V.summary, m.value = V.nodes.length ? "" : "No graph data was found for this object.", L(V);
        } catch (V) {
          m.value = `Graph failed: ${H(V)}`;
        } finally {
          p.value = !1;
        }
      }
    }
    async function M() {
      f.value = !0, F("Indexing the configuration…");
      try {
        const V = await D("rebuild", { path: u.value });
        F(
          `Index ready: +${V.objectsAdded} objects, ${V.objectsUpdated} updated, +${V.relationsAdded} relations, ${V.filesSkipped} skipped, ${V.filesWithErrors} errors in ${V.elapsedSeconds}s.`
        ), await A();
      } catch (V) {
        F(`Index build failed: ${H(V)}`, !0);
      } finally {
        f.value = !1;
      }
    }
    const I = {
      Document: "#6ea8fe",
      Catalog: "#64c487",
      AccumulationRegister: "#e8798a",
      InformationRegister: "#e6a15a",
      AccountingRegister: "#d67586",
      CalculationRegister: "#d67586",
      CommonModule: "#ad8be8",
      Report: "#d8bc68",
      Processing: "#d8bc68",
      Form: "#54b8ad",
      TabularSection: "#54b8ad"
    }, _ = {
      writes: "#e8798a",
      reads: "#6ea8fe",
      queries: "#e6a15a",
      calls: "#64c487",
      owns: "#7d8590",
      uses: "#ad8be8"
    }, N = [
      {
        selector: "node",
        style: {
          "background-color": (V) => I[String(V.data("kind"))] || "#7d8590",
          label: "data(label)",
          color: "#d8dee9",
          "font-size": 10,
          "text-valign": "bottom",
          "text-margin-y": 5,
          "text-outline-width": 2,
          "text-outline-color": "#20242b",
          width: 34,
          height: 34
        }
      },
      {
        selector: "node[?isCenter]",
        style: {
          width: 50,
          height: 50,
          "border-width": 3,
          "border-color": "#d8dee9",
          "font-size": 12,
          "font-weight": "bold"
        }
      },
      {
        selector: "edge",
        style: {
          "line-color": (V) => _[String(V.data("type"))] || "#7d8590",
          "target-arrow-color": (V) => _[String(V.data("type"))] || "#7d8590",
          "target-arrow-shape": "triangle",
          "curve-style": "bezier",
          width: 1.5,
          opacity: 0.82,
          label: "data(type)",
          "font-size": 8,
          color: "#8b949e",
          "text-rotation": "autorotate"
        }
      }
    ];
    function L(V) {
      if (!w.value) return;
      const O = [
        ...V.nodes.map(($) => ({ data: $ })),
        ...V.edges.map(($) => ({ data: $ }))
      ];
      E == null || E.destroy(), E = On({
        container: w.value,
        elements: O,
        style: N,
        minZoom: 0.05,
        maxZoom: 1.8,
        wheelSensitivity: 0.25,
        layout: {
          name: "cose",
          animate: !1,
          padding: 35,
          nodeRepulsion: () => 6500,
          idealEdgeLength: () => 120
        }
      }), window.setTimeout(() => E == null ? void 0 : E.fit(void 0, 30), 80);
    }
    function F(V, O = !1) {
      v.value = V, d.value = O;
    }
    function H(V) {
      return V instanceof Error ? V.message : String(V);
    }
    return fh(() => {
      A(), w.value && (T = new ResizeObserver(() => E == null ? void 0 : E.resize()), T.observe(w.value));
    }), ch(() => {
      window.clearTimeout(x), T == null || T.disconnect(), E == null || E.destroy();
    }), (V, O) => {
      var $;
      return et(), ot("div", eT, [
        Ce("aside", tT, [
          Ce("section", rT, [
            Ce("div", { class: "onec-heading" }, [
              O[6] || (O[6] = Ce("strong", null, "1C Configuration", -1)),
              Ce("button", {
                title: "Refresh index summary",
                onClick: A
              }, "↻")
            ]),
            Pa(Ce("input", {
              "onUpdate:modelValue": O[0] || (O[0] = (Q) => i.value = Q),
              class: "onec-search",
              placeholder: "Search objects…",
              spellcheck: "false"
            }, null, 512), [
              [Mi, i.value]
            ]),
            s.value ? (et(), ot("div", nT, "Searching…")) : o.value.length ? (et(), ot("ul", aT, [
              (et(!0), ot(Wt, null, Xi(o.value, (Q) => {
                var se;
                return et(), ot("li", {
                  key: Q.fullName
                }, [
                  Ce("button", {
                    class: la({ selected: ((se = l.value) == null ? void 0 : se.fullName) === Q.fullName }),
                    onClick: (ae) => k(Q.fullName)
                  }, [
                    Ce("span", null, Qe(Q.fullName), 1),
                    Ce("small", null, Qe(Q.kind), 1)
                  ], 10, iT)
                ]);
              }), 128))
            ])) : Rr("", !0),
            Ce("div", sT, [
              (et(!0), ot(Wt, null, Xi(n.value, (Q) => (et(), ot("section", {
                key: Q.kind,
                class: "onec-tree-section"
              }, [
                Ce("button", {
                  class: "onec-section-head",
                  onClick: (se) => a[Q.kind] = !a[Q.kind]
                }, [
                  Ce("span", null, Qe(a[Q.kind] ? "▾" : "▸"), 1),
                  Ce("strong", null, Qe(Q.kind), 1),
                  Ce("small", null, Qe(Q.count), 1)
                ], 8, oT),
                a[Q.kind] ? (et(), ot("div", lT, [
                  (et(!0), ot(Wt, null, Xi(Q.objects, (se) => {
                    var ae;
                    return et(), Uu(JC, {
                      key: se.fullName,
                      node: se,
                      selected: (ae = l.value) == null ? void 0 : ae.fullName,
                      onSelect: O[1] || (O[1] = (le) => k(le.fullName))
                    }, null, 8, ["node", "selected"]);
                  }), 128))
                ])) : Rr("", !0)
              ]))), 128)),
              !c.value && !n.value.length ? (et(), ot("div", uT, [...O[7] || (O[7] = [
                Br(" The OneC index is empty. Build it below or run ", -1),
                Ce("code", null, "onec_build_index", -1),
                Br(". ", -1)
              ])])) : Rr("", !0)
            ])
          ]),
          Ce("section", fT, [
            Ce("div", null, [
              O[8] || (O[8] = Ce("span", null, "Objects", -1)),
              Ce("strong", null, Qe(r.objectCount), 1)
            ]),
            Ce("div", null, [
              O[9] || (O[9] = Ce("span", null, "Relations", -1)),
              Ce("strong", null, Qe(r.relationCount), 1)
            ]),
            Ce("div", null, [
              O[10] || (O[10] = Ce("span", null, "Sections", -1)),
              Ce("strong", null, Qe(r.sectionCount), 1)
            ])
          ]),
          Ce("section", cT, [
            O[11] || (O[11] = Ce("strong", null, "Build index", -1)),
            O[12] || (O[12] = Ce("p", null, "Path inside the current project workspace.", -1)),
            Ce("div", vT, [
              Pa(Ce("input", {
                "onUpdate:modelValue": O[2] || (O[2] = (Q) => u.value = Q),
                class: "onec-grow",
                placeholder: "e.g. configuration/",
                spellcheck: "false"
              }, null, 512), [
                [Mi, u.value]
              ]),
              Ce("button", {
                disabled: f.value || !u.value.trim(),
                onClick: M
              }, Qe(f.value ? "Building…" : "Build"), 9, dT)
            ])
          ])
        ]),
        Ce("main", hT, [
          Ce("header", gT, [
            Ce("div", null, [
              Ce("strong", null, Qe((($ = l.value) == null ? void 0 : $.fullName) || "Choose an object"), 1),
              l.value ? (et(), ot("span", pT, Qe(l.value.kind), 1)) : Rr("", !0)
            ]),
            Ce("div", yT, [
              Pa(Ce("select", {
                "onUpdate:modelValue": O[3] || (O[3] = (Q) => h.value = Q),
                disabled: !l.value
              }, [...O[13] || (O[13] = [
                Ce("option", { value: "dependencies" }, "Dependencies", -1),
                Ce("option", { value: "references" }, "References", -1),
                Ce("option", { value: "dataflow" }, "Data flow", -1)
              ])], 8, mT), [
                [s0, h.value]
              ]),
              Ce("label", null, [
                O[14] || (O[14] = Br("Depth ", -1)),
                Pa(Ce("input", {
                  "onUpdate:modelValue": O[4] || (O[4] = (Q) => y.value = Q),
                  type: "number",
                  min: "1",
                  max: "8"
                }, null, 512), [
                  [
                    Mi,
                    y.value,
                    void 0,
                    { number: !0 }
                  ]
                ])
              ]),
              Ce("label", null, [
                O[15] || (O[15] = Br("Edges ", -1)),
                Pa(Ce("input", {
                  "onUpdate:modelValue": O[5] || (O[5] = (Q) => g.value = Q),
                  type: "number",
                  min: "1",
                  max: "1000",
                  step: "25"
                }, null, 512), [
                  [
                    Mi,
                    g.value,
                    void 0,
                    { number: !0 }
                  ]
                ])
              ]),
              Ce("button", {
                disabled: !l.value || p.value,
                onClick: R
              }, Qe(p.value ? "Loading…" : "Show graph"), 9, bT)
            ])
          ]),
          l.value ? (et(), ot("div", wT, [
            Ce("div", null, [
              O[16] || (O[16] = Ce("span", null, "Name", -1)),
              Br(Qe(l.value.name), 1)
            ]),
            Ce("div", null, [
              O[17] || (O[17] = Ce("span", null, "Path", -1)),
              Br(Qe(l.value.path || "—"), 1)
            ]),
            Ce("div", null, [
              O[18] || (O[18] = Ce("span", null, "Summary", -1)),
              Br(Qe(l.value.summary || "—"), 1)
            ])
          ])) : Rr("", !0),
          Ce("div", xT, [
            Ce("span", null, Qe(m.value), 1),
            b.value ? (et(), ot("span", ET, [
              Br(Qe(b.value.nodeCount) + " nodes · " + Qe(b.value.edgeCount) + " edges · depth " + Qe(b.value.depth) + " ", 1),
              b.value.truncated ? (et(), ot("b", CT, " · truncated")) : Rr("", !0)
            ])) : Rr("", !0)
          ]),
          Ce("div", {
            ref_key: "graphElement",
            ref: w,
            class: "onec-graph"
          }, null, 512),
          v.value ? (et(), ot("div", {
            key: 1,
            class: la(["onec-status", { error: d.value }])
          }, Qe(v.value), 3)) : Rr("", !0)
        ])
      ]);
    };
  }
}), ST = ".onec-browser{display:grid;grid-template-columns:minmax(280px,360px) minmax(420px,1fr);gap:10px;min-height:560px;color:var(--text, #d8dee9);font-size:var(--fs-sm, 12px)}.onec-sidebar{display:flex;min-width:0;flex-direction:column;gap:10px}.onec-card{min-width:0;border:1px solid var(--border, #3d444d);border-radius:var(--radius, 7px);background:var(--panel, #262b33);padding:10px}.onec-explorer{display:flex;min-height:310px;flex:1;flex-direction:column;gap:8px}.onec-heading,.onec-object-header,.onec-row,.onec-graph-controls,.onec-summary>div,.onec-graph-status{display:flex;align-items:center;gap:7px}.onec-heading,.onec-object-header,.onec-graph-status,.onec-summary>div{justify-content:space-between}.onec-heading button{margin-left:auto}.onec-browser button,.onec-browser input,.onec-browser select{min-height:26px;box-sizing:border-box;border:1px solid var(--border, #3d444d);border-radius:5px;color:var(--text, #d8dee9);background:var(--bg, #20242b);font:inherit}.onec-browser button{cursor:pointer;padding:2px 8px}.onec-browser button:hover:not(:disabled){border-color:var(--accent, #58a6ff);background:var(--accent-soft, #23364d)}.onec-browser button:disabled{cursor:default;opacity:.5}.onec-browser input,.onec-browser select{padding:3px 7px}.onec-search{width:100%}.onec-search-results{max-height:155px;margin:0;padding:0;overflow:auto;list-style:none;border:1px solid var(--border, #3d444d);border-radius:5px}.onec-search-results button{display:flex;width:100%;justify-content:space-between;gap:8px;border:0;border-radius:0;text-align:left}.onec-search-results button.selected,.onec-node-head.selected{color:var(--accent, #58a6ff);background:var(--accent-soft, #23364d)}.onec-search-results span,.onec-node-name{min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.onec-search-results small,.onec-kind,.onec-muted,.onec-rebuild p,.onec-graph-status{color:var(--muted, #8b949e)}.onec-inline-state{padding:3px 6px}.onec-tree{min-height:0;flex:1;overflow:auto}.onec-tree-section+.onec-tree-section{margin-top:2px}.onec-section-head,.onec-node-head{display:flex;width:100%;align-items:center;gap:5px;border:0!important;background:transparent!important;text-align:left}.onec-section-head small{margin-left:auto;color:var(--muted, #8b949e)}.onec-section-objects,.onec-node-children{padding-left:12px}.onec-chevron{width:13px;flex:0 0 13px;text-align:center}.onec-kind{margin-left:auto;font-size:var(--fs-xs, 10px)}.onec-empty{padding:18px 8px;color:var(--muted, #8b949e);text-align:center}.onec-summary{display:grid;grid-template-columns:repeat(3,1fr);gap:9px}.onec-summary>div{flex-direction:column;gap:2px}.onec-summary span,.onec-rebuild p{font-size:var(--fs-xs, 10px)}.onec-summary strong{font-size:16px}.onec-rebuild p{margin:3px 0 8px}.onec-grow{min-width:0;flex:1}.onec-main{display:flex;min-height:0;flex-direction:column;gap:9px}.onec-object-header{align-items:flex-start;flex-wrap:wrap}.onec-object-header>div:first-child{display:flex;min-width:0;flex-direction:column;gap:3px}.onec-object-header .onec-kind{margin-left:0}.onec-graph-controls{flex-wrap:wrap;justify-content:flex-end}.onec-graph-controls label{display:flex;align-items:center;gap:4px;color:var(--muted, #8b949e)}.onec-graph-controls input{width:58px}.onec-object-details{display:grid;grid-template-columns:1fr;gap:3px;padding:7px 9px;border:1px solid var(--border, #3d444d);border-radius:5px}.onec-object-details>div{overflow-wrap:anywhere}.onec-object-details span{display:inline-block;width:62px;color:var(--muted, #8b949e)}.onec-graph-status{min-height:18px;flex-wrap:wrap}.onec-graph-status b{color:#e6a15a}.onec-graph{min-height:360px;flex:1;overflow:hidden;border:1px solid var(--border, #3d444d);border-radius:6px;background:color-mix(in srgb,var(--bg, #20242b) 88%,black)}.onec-status{padding:6px 8px;border-radius:5px;background:var(--accent-soft, #23364d);white-space:pre-wrap}.onec-status.error{color:#ffb4a9;background:color-mix(in srgb,#f85149 15%,transparent)}@media(max-width:900px){.onec-browser{grid-template-columns:1fr}.onec-main{min-height:560px}}", Ad = "spla-onec-web-styles";
function PT() {
  if (document.getElementById(Ad)) return;
  const t = document.createElement("style");
  t.id = Ad, t.textContent = ST, document.head.appendChild(t);
}
function AT(t, e) {
  PT();
  let r = v0(TT, { api: e });
  return r.mount(t), {
    // The browser owns no plugin settings; Save must preserve the opaque host blob unchanged.
    save: () => e.getJson(),
    destroy: () => {
      r == null || r.unmount(), r = null;
    }
  };
}
export {
  AT as mount
};
