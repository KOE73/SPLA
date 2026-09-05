/**
* @vue/shared v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Mu(t) {
  const e = /* @__PURE__ */ Object.create(null);
  for (const r of t.split(",")) e[r] = 1;
  return (r) => r in e;
}
const We = {}, ta = [], Tr = () => {
}, Md = () => !1, Ls = (t) => t.charCodeAt(0) === 111 && t.charCodeAt(1) === 110 && // uppercase letter
(t.charCodeAt(2) > 122 || t.charCodeAt(2) < 97), Is = (t) => t.startsWith("onUpdate:"), Pt = Object.assign, Lu = (t, e) => {
  const r = t.indexOf(e);
  r > -1 && t.splice(r, 1);
}, $p = Object.prototype.hasOwnProperty, qe = (t, e) => $p.call(t, e), ke = Array.isArray, ra = (t) => di(t) === "[object Map]", Os = (t) => di(t) === "[object Set]", Tf = (t) => di(t) === "[object Date]", Re = (t) => typeof t == "function", at = (t) => typeof t == "string", Sr = (t) => typeof t == "symbol", Ge = (t) => t !== null && typeof t == "object", Ld = (t) => (Ge(t) || Re(t)) && Re(t.then) && Re(t.catch), Id = Object.prototype.toString, di = (t) => Id.call(t), Hp = (t) => di(t).slice(8, -1), Od = (t) => di(t) === "[object Object]", Iu = (t) => at(t) && t !== "NaN" && t[0] !== "-" && "" + parseInt(t, 10) === t, $a = /* @__PURE__ */ Mu(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), _s = (t) => {
  const e = /* @__PURE__ */ Object.create(null);
  return ((r) => e[r] || (e[r] = t(r)));
}, Up = /-\w/g, Vt = _s(
  (t) => t.replace(Up, (e) => e.slice(1).toUpperCase())
), Gp = /\B([A-Z])/g, Fn = _s(
  (t) => t.replace(Gp, "-$1").toLowerCase()
), Ns = _s((t) => t.charAt(0).toUpperCase() + t.slice(1)), vo = _s(
  (t) => t ? `on${Ns(t)}` : ""
), Er = (t, e) => !Object.is(t, e), Yi = (t, ...e) => {
  for (let r = 0; r < t.length; r++)
    t[r](...e);
}, _d = (t, e, r, n = !1) => {
  Object.defineProperty(t, e, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: r
  });
}, Fs = (t) => {
  const e = parseFloat(t);
  return isNaN(e) ? t : e;
};
let Sf;
const zs = () => Sf || (Sf = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function Ou(t) {
  if (ke(t)) {
    const e = {};
    for (let r = 0; r < t.length; r++) {
      const n = t[r], a = at(n) ? Xp(n) : Ou(n);
      if (a)
        for (const i in a)
          e[i] = a[i];
    }
    return e;
  } else if (at(t) || Ge(t))
    return t;
}
const Wp = /;(?![^(]*\))/g, Kp = /:([^]+)/, Yp = /\/\*[^]*?\*\//g;
function Xp(t) {
  const e = {};
  return t.replace(Yp, "").split(Wp).forEach((r) => {
    if (r) {
      const n = r.split(Kp);
      n.length > 1 && (e[n[0].trim()] = n[1].trim());
    }
  }), e;
}
function fa(t) {
  let e = "";
  if (at(t))
    e = t;
  else if (ke(t))
    for (let r = 0; r < t.length; r++) {
      const n = fa(t[r]);
      n && (e += n + " ");
    }
  else if (Ge(t))
    for (const r in t)
      t[r] && (e += r + " ");
  return e.trim();
}
const Zp = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", Qp = /* @__PURE__ */ Mu(Zp);
function Nd(t) {
  return !!t || t === "";
}
function jp(t, e) {
  if (t.length !== e.length) return !1;
  let r = !0;
  for (let n = 0; r && n < t.length; n++)
    r = hi(t[n], e[n]);
  return r;
}
function hi(t, e) {
  if (t === e) return !0;
  let r = Tf(t), n = Tf(e);
  if (r || n)
    return r && n ? t.getTime() === e.getTime() : !1;
  if (r = Sr(t), n = Sr(e), r || n)
    return t === e;
  if (r = ke(t), n = ke(e), r || n)
    return r && n ? jp(t, e) : !1;
  if (r = Ge(t), n = Ge(e), r || n) {
    if (!r || !n)
      return !1;
    const a = Object.keys(t).length, i = Object.keys(e).length;
    if (a !== i)
      return !1;
    for (const s in t) {
      const o = t.hasOwnProperty(s), l = e.hasOwnProperty(s);
      if (o && !l || !o && l || !hi(t[s], e[s]))
        return !1;
    }
  }
  return String(t) === String(e);
}
function Jp(t, e) {
  return t.findIndex((r) => hi(r, e));
}
const Fd = (t) => !!(t && t.__v_isRef === !0), Le = (t) => at(t) ? t : t == null ? "" : ke(t) || Ge(t) && (t.toString === Id || !Re(t.toString)) ? Fd(t) ? Le(t.value) : JSON.stringify(t, zd, 2) : String(t), zd = (t, e) => Fd(e) ? zd(t, e.value) : ra(e) ? {
  [`Map(${e.size})`]: [...e.entries()].reduce(
    (r, [n, a], i) => (r[ho(n, i) + " =>"] = a, r),
    {}
  )
} : Os(e) ? {
  [`Set(${e.size})`]: [...e.values()].map((r) => ho(r))
} : Sr(e) ? ho(e) : Ge(e) && !ke(e) && !Od(e) ? String(e) : e, ho = (t, e = "") => {
  var r;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Sr(t) ? `Symbol(${(r = t.description) != null ? r : e})` : t
  );
};
/**
* @vue/reactivity v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Ct;
class ey {
  // TODO isolatedDeclarations "__v_skip"
  constructor(e = !1) {
    this.detached = e, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !e && Ct && (Ct.active ? (this.parent = Ct, this.index = (Ct.scopes || (Ct.scopes = [])).push(
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
      const r = Ct;
      try {
        return Ct = this, e();
      } finally {
        Ct = r;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = Ct, Ct = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (Ct === this)
        Ct = this.prevScope;
      else {
        let e = Ct;
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
function ty() {
  return Ct;
}
let Xe;
const go = /* @__PURE__ */ new WeakSet();
class Vd {
  constructor(e) {
    this.fn = e, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, Ct && (Ct.active ? Ct.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, go.has(this) && (go.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || $d(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, Pf(this), Hd(this);
    const e = Xe, r = lr;
    Xe = this, lr = !0;
    try {
      return this.fn();
    } finally {
      Ud(this), Xe = e, lr = r, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let e = this.deps; e; e = e.nextDep)
        Fu(e);
      this.deps = this.depsTail = void 0, Pf(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? go.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    eu(this) && this.run();
  }
  get dirty() {
    return eu(this);
  }
}
let qd = 0, Ha, Ua;
function $d(t, e = !1) {
  if (t.flags |= 8, e) {
    t.next = Ua, Ua = t;
    return;
  }
  t.next = Ha, Ha = t;
}
function _u() {
  qd++;
}
function Nu() {
  if (--qd > 0)
    return;
  if (Ua) {
    let e = Ua;
    for (Ua = void 0; e; ) {
      const r = e.next;
      e.next = void 0, e.flags &= -9, e = r;
    }
  }
  let t;
  for (; Ha; ) {
    let e = Ha;
    for (Ha = void 0; e; ) {
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
function Hd(t) {
  for (let e = t.deps; e; e = e.nextDep)
    e.version = -1, e.prevActiveLink = e.dep.activeLink, e.dep.activeLink = e;
}
function Ud(t) {
  let e, r = t.depsTail, n = r;
  for (; n; ) {
    const a = n.prevDep;
    n.version === -1 ? (n === r && (r = a), Fu(n), ry(n)) : e = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = a;
  }
  t.deps = e, t.depsTail = r;
}
function eu(t) {
  for (let e = t.deps; e; e = e.nextDep)
    if (e.dep.version !== e.version || e.dep.computed && (Gd(e.dep.computed) || e.dep.version !== e.version))
      return !0;
  return !!t._dirty;
}
function Gd(t) {
  if (t.flags & 4 && !(t.flags & 16) || (t.flags &= -17, t.globalVersion === Qa) || (t.globalVersion = Qa, !t.isSSR && t.flags & 128 && (!t.deps && !t._dirty || !eu(t))))
    return;
  t.flags |= 2;
  const e = t.dep, r = Xe, n = lr;
  Xe = t, lr = !0;
  try {
    Hd(t);
    const a = t.fn(t._value);
    (e.version === 0 || Er(a, t._value)) && (t.flags |= 128, t._value = a, e.version++);
  } catch (a) {
    throw e.version++, a;
  } finally {
    Xe = r, lr = n, Ud(t), t.flags &= -3;
  }
}
function Fu(t, e = !1) {
  const { dep: r, prevSub: n, nextSub: a } = t;
  if (n && (n.nextSub = a, t.prevSub = void 0), a && (a.prevSub = n, t.nextSub = void 0), r.subs === t && (r.subs = n, !n && r.computed)) {
    r.computed.flags &= -5;
    for (let i = r.computed.deps; i; i = i.nextDep)
      Fu(i, !0);
  }
  !e && !--r.sc && r.map && r.map.delete(r.key);
}
function ry(t) {
  const { prevDep: e, nextDep: r } = t;
  e && (e.nextDep = r, t.prevDep = void 0), r && (r.prevDep = e, t.nextDep = void 0);
}
let lr = !0;
const Wd = [];
function Vr() {
  Wd.push(lr), lr = !1;
}
function qr() {
  const t = Wd.pop();
  lr = t === void 0 ? !0 : t;
}
function Pf(t) {
  const { cleanup: e } = t;
  if (t.cleanup = void 0, e) {
    const r = Xe;
    Xe = void 0;
    try {
      e();
    } finally {
      Xe = r;
    }
  }
}
let Qa = 0;
class ny {
  constructor(e, r) {
    this.sub = e, this.dep = r, this.version = r.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class zu {
  // TODO isolatedDeclarations "__v_skip"
  constructor(e) {
    this.computed = e, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(e) {
    if (!Xe || !lr || Xe === this.computed)
      return;
    let r = this.activeLink;
    if (r === void 0 || r.sub !== Xe)
      r = this.activeLink = new ny(Xe, this), Xe.deps ? (r.prevDep = Xe.depsTail, Xe.depsTail.nextDep = r, Xe.depsTail = r) : Xe.deps = Xe.depsTail = r, Kd(r);
    else if (r.version === -1 && (r.version = this.version, r.nextDep)) {
      const n = r.nextDep;
      n.prevDep = r.prevDep, r.prevDep && (r.prevDep.nextDep = n), r.prevDep = Xe.depsTail, r.nextDep = void 0, Xe.depsTail.nextDep = r, Xe.depsTail = r, Xe.deps === r && (Xe.deps = n);
    }
    return r;
  }
  trigger(e) {
    this.version++, Qa++, this.notify(e);
  }
  notify(e) {
    _u();
    try {
      for (let r = this.subs; r; r = r.prevSub)
        r.sub.notify() && r.sub.dep.notify();
    } finally {
      Nu();
    }
  }
}
function Kd(t) {
  if (t.dep.sc++, t.sub.flags & 4) {
    const e = t.dep.computed;
    if (e && !t.dep.subs) {
      e.flags |= 20;
      for (let n = e.deps; n; n = n.nextDep)
        Kd(n);
    }
    const r = t.dep.subs;
    r !== t && (t.prevSub = r, r && (r.nextSub = t)), t.dep.subs = t;
  }
}
const tu = /* @__PURE__ */ new WeakMap(), kn = /* @__PURE__ */ Symbol(
  ""
), ru = /* @__PURE__ */ Symbol(
  ""
), ja = /* @__PURE__ */ Symbol(
  ""
);
function Bt(t, e, r) {
  if (lr && Xe) {
    let n = tu.get(t);
    n || tu.set(t, n = /* @__PURE__ */ new Map());
    let a = n.get(r);
    a || (n.set(r, a = new zu()), a.map = n, a.key = r), a.track();
  }
}
function _r(t, e, r, n, a, i) {
  const s = tu.get(t);
  if (!s) {
    Qa++;
    return;
  }
  const o = (l) => {
    l && l.trigger();
  };
  if (_u(), e === "clear")
    s.forEach(o);
  else {
    const l = ke(t), u = l && Iu(r);
    if (l && r === "length") {
      const f = Number(n);
      s.forEach((c, v) => {
        (v === "length" || v === ja || !Sr(v) && v >= f) && o(c);
      });
    } else
      switch ((r !== void 0 || s.has(void 0)) && o(s.get(r)), u && o(s.get(ja)), e) {
        case "add":
          l ? u && o(s.get("length")) : (o(s.get(kn)), ra(t) && o(s.get(ru)));
          break;
        case "delete":
          l || (o(s.get(kn)), ra(t) && o(s.get(ru)));
          break;
        case "set":
          ra(t) && o(s.get(kn));
          break;
      }
  }
  Nu();
}
function qn(t) {
  const e = /* @__PURE__ */ Ve(t);
  return e === t ? e : (Bt(e, "iterate", ja), /* @__PURE__ */ rr(t) ? e : e.map(fr));
}
function Vs(t) {
  return Bt(t = /* @__PURE__ */ Ve(t), "iterate", ja), t;
}
function wr(t, e) {
  return /* @__PURE__ */ $r(t) ? ca(/* @__PURE__ */ Bn(t) ? fr(e) : e) : fr(e);
}
const ay = {
  __proto__: null,
  [Symbol.iterator]() {
    return po(this, Symbol.iterator, (t) => wr(this, t));
  },
  concat(...t) {
    return qn(this).concat(
      ...t.map((e) => ke(e) ? qn(e) : e)
    );
  },
  entries() {
    return po(this, "entries", (t) => (t[1] = wr(this, t[1]), t));
  },
  every(t, e) {
    return Br(this, "every", t, e, void 0, arguments);
  },
  filter(t, e) {
    return Br(
      this,
      "filter",
      t,
      e,
      (r) => r.map((n) => wr(this, n)),
      arguments
    );
  },
  find(t, e) {
    return Br(
      this,
      "find",
      t,
      e,
      (r) => wr(this, r),
      arguments
    );
  },
  findIndex(t, e) {
    return Br(this, "findIndex", t, e, void 0, arguments);
  },
  findLast(t, e) {
    return Br(
      this,
      "findLast",
      t,
      e,
      (r) => wr(this, r),
      arguments
    );
  },
  findLastIndex(t, e) {
    return Br(this, "findLastIndex", t, e, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(t, e) {
    return Br(this, "forEach", t, e, void 0, arguments);
  },
  includes(...t) {
    return yo(this, "includes", t);
  },
  indexOf(...t) {
    return yo(this, "indexOf", t);
  },
  join(t) {
    return qn(this).join(t);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...t) {
    return yo(this, "lastIndexOf", t);
  },
  map(t, e) {
    return Br(this, "map", t, e, void 0, arguments);
  },
  pop() {
    return Da(this, "pop");
  },
  push(...t) {
    return Da(this, "push", t);
  },
  reduce(t, ...e) {
    return Df(this, "reduce", t, e);
  },
  reduceRight(t, ...e) {
    return Df(this, "reduceRight", t, e);
  },
  shift() {
    return Da(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(t, e) {
    return Br(this, "some", t, e, void 0, arguments);
  },
  splice(...t) {
    return Da(this, "splice", t);
  },
  toReversed() {
    return qn(this).toReversed();
  },
  toSorted(t) {
    return qn(this).toSorted(t);
  },
  toSpliced(...t) {
    return qn(this).toSpliced(...t);
  },
  unshift(...t) {
    return Da(this, "unshift", t);
  },
  values() {
    return po(this, "values", (t) => wr(this, t));
  }
};
function po(t, e, r) {
  const n = Vs(t), a = n[e]();
  return n !== t && !/* @__PURE__ */ rr(t) && (a._next = a.next, a.next = () => {
    const i = a._next();
    return i.done || (i.value = r(i.value)), i;
  }), a;
}
const iy = Array.prototype;
function Br(t, e, r, n, a, i) {
  const s = Vs(t), o = s !== t && !/* @__PURE__ */ rr(t), l = s[e];
  if (l !== iy[e]) {
    const c = l.apply(t, i);
    return o ? fr(c) : c;
  }
  let u = r;
  s !== t && (o ? u = function(c, v) {
    return r.call(this, wr(t, c), v, t);
  } : r.length > 2 && (u = function(c, v) {
    return r.call(this, c, v, t);
  }));
  const f = l.call(s, u, n);
  return o && a ? a(f) : f;
}
function Df(t, e, r, n) {
  const a = Vs(t), i = a !== t && !/* @__PURE__ */ rr(t);
  let s = r, o = !1;
  a !== t && (i ? (o = n.length === 0, s = function(u, f, c) {
    return o && (o = !1, u = wr(t, u)), r.call(this, u, wr(t, f), c, t);
  }) : r.length > 3 && (s = function(u, f, c) {
    return r.call(this, u, f, c, t);
  }));
  const l = a[e](s, ...n);
  return o ? wr(t, l) : l;
}
function yo(t, e, r) {
  const n = /* @__PURE__ */ Ve(t);
  Bt(n, "iterate", ja);
  const a = n[e](...r);
  return (a === -1 || a === !1) && /* @__PURE__ */ $u(r[0]) ? (r[0] = /* @__PURE__ */ Ve(r[0]), n[e](...r)) : a;
}
function Da(t, e, r = []) {
  Vr(), _u();
  const n = (/* @__PURE__ */ Ve(t))[e].apply(t, r);
  return Nu(), qr(), n;
}
const sy = /* @__PURE__ */ Mu("__proto__,__v_isRef,__isVue"), Yd = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((t) => t !== "arguments" && t !== "caller").map((t) => Symbol[t]).filter(Sr)
);
function oy(t) {
  Sr(t) || (t = String(t));
  const e = /* @__PURE__ */ Ve(this);
  return Bt(e, "has", t), e.hasOwnProperty(t);
}
class Xd {
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
      return n === (a ? i ? yy : Jd : i ? jd : Qd).get(e) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(e) === Object.getPrototypeOf(n) ? e : void 0;
    const s = ke(e);
    if (!a) {
      let l;
      if (s && (l = ay[r]))
        return l;
      if (r === "hasOwnProperty")
        return oy;
    }
    const o = Reflect.get(
      e,
      r,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ Mt(e) ? e : n
    );
    if ((Sr(r) ? Yd.has(r) : sy(r)) || (a || Bt(e, "get", r), i))
      return o;
    if (/* @__PURE__ */ Mt(o)) {
      const l = s && Iu(r) ? o : o.value;
      return a && Ge(l) ? /* @__PURE__ */ au(l) : l;
    }
    return Ge(o) ? a ? /* @__PURE__ */ au(o) : /* @__PURE__ */ Ja(o) : o;
  }
}
class Zd extends Xd {
  constructor(e = !1) {
    super(!1, e);
  }
  set(e, r, n, a) {
    let i = e[r];
    const s = ke(e) && Iu(r);
    if (!this._isShallow) {
      const u = /* @__PURE__ */ $r(i);
      if (!/* @__PURE__ */ rr(n) && !/* @__PURE__ */ $r(n) && (i = /* @__PURE__ */ Ve(i), n = /* @__PURE__ */ Ve(n)), !s && /* @__PURE__ */ Mt(i) && !/* @__PURE__ */ Mt(n))
        return u || (i.value = n), !0;
    }
    const o = s ? Number(r) < e.length : qe(e, r), l = Reflect.set(
      e,
      r,
      n,
      /* @__PURE__ */ Mt(e) ? e : a
    );
    return e === /* @__PURE__ */ Ve(a) && l && (o ? Er(n, i) && _r(e, "set", r, n) : _r(e, "add", r, n)), l;
  }
  deleteProperty(e, r) {
    const n = qe(e, r);
    e[r];
    const a = Reflect.deleteProperty(e, r);
    return a && n && _r(e, "delete", r, void 0), a;
  }
  has(e, r) {
    const n = Reflect.has(e, r);
    return (!Sr(r) || !Yd.has(r)) && Bt(e, "has", r), n;
  }
  ownKeys(e) {
    return Bt(
      e,
      "iterate",
      ke(e) ? "length" : kn
    ), Reflect.ownKeys(e);
  }
}
class ly extends Xd {
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
const uy = /* @__PURE__ */ new Zd(), fy = /* @__PURE__ */ new ly(), cy = /* @__PURE__ */ new Zd(!0);
const nu = (t) => t, Ri = (t) => Reflect.getPrototypeOf(t);
function vy(t, e, r) {
  return function(...n) {
    const a = this.__v_raw, i = /* @__PURE__ */ Ve(a), s = ra(i), o = t === "entries" || t === Symbol.iterator && s, l = t === "keys" && s, u = a[t](...n), f = r ? nu : e ? ca : fr;
    return !e && Bt(
      i,
      "iterate",
      l ? ru : kn
    ), Pt(
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
function Mi(t) {
  return function(...e) {
    return t === "delete" ? !1 : t === "clear" ? void 0 : this;
  };
}
function dy(t, e) {
  const r = {
    get(a) {
      const i = this.__v_raw, s = /* @__PURE__ */ Ve(i), o = /* @__PURE__ */ Ve(a);
      t || (Er(a, o) && Bt(s, "get", a), Bt(s, "get", o));
      const { has: l } = Ri(s), u = e ? nu : t ? ca : fr;
      if (l.call(s, a))
        return u(i.get(a));
      if (l.call(s, o))
        return u(i.get(o));
      i !== s && i.get(a);
    },
    get size() {
      const a = this.__v_raw;
      return !t && Bt(/* @__PURE__ */ Ve(a), "iterate", kn), a.size;
    },
    has(a) {
      const i = this.__v_raw, s = /* @__PURE__ */ Ve(i), o = /* @__PURE__ */ Ve(a);
      return t || (Er(a, o) && Bt(s, "has", a), Bt(s, "has", o)), a === o ? i.has(a) : i.has(a) || i.has(o);
    },
    forEach(a, i) {
      const s = this, o = s.__v_raw, l = /* @__PURE__ */ Ve(o), u = e ? nu : t ? ca : fr;
      return !t && Bt(l, "iterate", kn), o.forEach((f, c) => a.call(i, u(f), u(c), s));
    }
  };
  return Pt(
    r,
    t ? {
      add: Mi("add"),
      set: Mi("set"),
      delete: Mi("delete"),
      clear: Mi("clear")
    } : {
      add(a) {
        const i = /* @__PURE__ */ Ve(this), s = Ri(i), o = /* @__PURE__ */ Ve(a), l = !e && !/* @__PURE__ */ rr(a) && !/* @__PURE__ */ $r(a) ? o : a;
        return s.has.call(i, l) || Er(a, l) && s.has.call(i, a) || Er(o, l) && s.has.call(i, o) || (i.add(l), _r(i, "add", l, l)), this;
      },
      set(a, i) {
        !e && !/* @__PURE__ */ rr(i) && !/* @__PURE__ */ $r(i) && (i = /* @__PURE__ */ Ve(i));
        const s = /* @__PURE__ */ Ve(this), { has: o, get: l } = Ri(s);
        let u = o.call(s, a);
        u || (a = /* @__PURE__ */ Ve(a), u = o.call(s, a));
        const f = l.call(s, a);
        return s.set(a, i), u ? Er(i, f) && _r(s, "set", a, i) : _r(s, "add", a, i), this;
      },
      delete(a) {
        const i = /* @__PURE__ */ Ve(this), { has: s, get: o } = Ri(i);
        let l = s.call(i, a);
        l || (a = /* @__PURE__ */ Ve(a), l = s.call(i, a)), o && o.call(i, a);
        const u = i.delete(a);
        return l && _r(i, "delete", a, void 0), u;
      },
      clear() {
        const a = /* @__PURE__ */ Ve(this), i = a.size !== 0, s = a.clear();
        return i && _r(
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
    r[a] = vy(a, t, e);
  }), r;
}
function Vu(t, e) {
  const r = dy(t, e);
  return (n, a, i) => a === "__v_isReactive" ? !t : a === "__v_isReadonly" ? t : a === "__v_raw" ? n : Reflect.get(
    qe(r, a) && a in n ? r : n,
    a,
    i
  );
}
const hy = {
  get: /* @__PURE__ */ Vu(!1, !1)
}, gy = {
  get: /* @__PURE__ */ Vu(!1, !0)
}, py = {
  get: /* @__PURE__ */ Vu(!0, !1)
};
const Qd = /* @__PURE__ */ new WeakMap(), jd = /* @__PURE__ */ new WeakMap(), Jd = /* @__PURE__ */ new WeakMap(), yy = /* @__PURE__ */ new WeakMap();
function my(t) {
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
function Ja(t) {
  return /* @__PURE__ */ $r(t) ? t : qu(
    t,
    !1,
    uy,
    hy,
    Qd
  );
}
// @__NO_SIDE_EFFECTS__
function by(t) {
  return qu(
    t,
    !1,
    cy,
    gy,
    jd
  );
}
// @__NO_SIDE_EFFECTS__
function au(t) {
  return qu(
    t,
    !0,
    fy,
    py,
    Jd
  );
}
function qu(t, e, r, n, a) {
  if (!Ge(t) || t.__v_raw && !(e && t.__v_isReactive) || t.__v_skip || !Object.isExtensible(t))
    return t;
  const i = a.get(t);
  if (i)
    return i;
  const s = my(Hp(t));
  if (s === 0)
    return t;
  const o = new Proxy(
    t,
    s === 2 ? n : r
  );
  return a.set(t, o), o;
}
// @__NO_SIDE_EFFECTS__
function Bn(t) {
  return /* @__PURE__ */ $r(t) ? /* @__PURE__ */ Bn(t.__v_raw) : !!(t && t.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function $r(t) {
  return !!(t && t.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function rr(t) {
  return !!(t && t.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function $u(t) {
  return t ? !!t.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function Ve(t) {
  const e = t && t.__v_raw;
  return e ? /* @__PURE__ */ Ve(e) : t;
}
function wy(t) {
  return !qe(t, "__v_skip") && Object.isExtensible(t) && _d(t, "__v_skip", !0), t;
}
const fr = (t) => Ge(t) ? /* @__PURE__ */ Ja(t) : t, ca = (t) => Ge(t) ? /* @__PURE__ */ au(t) : t;
// @__NO_SIDE_EFFECTS__
function Mt(t) {
  return t ? t.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function mt(t) {
  return xy(t, !1);
}
function xy(t, e) {
  return /* @__PURE__ */ Mt(t) ? t : new Ey(t, e);
}
class Ey {
  constructor(e, r) {
    this.dep = new zu(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = r ? e : /* @__PURE__ */ Ve(e), this._value = r ? e : fr(e), this.__v_isShallow = r;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(e) {
    const r = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ rr(e) || /* @__PURE__ */ $r(e);
    e = n ? e : /* @__PURE__ */ Ve(e), Er(e, r) && (this._rawValue = e, this._value = n ? e : fr(e), this.dep.trigger());
  }
}
function st(t) {
  return /* @__PURE__ */ Mt(t) ? t.value : t;
}
const Cy = {
  get: (t, e, r) => e === "__v_raw" ? t : st(Reflect.get(t, e, r)),
  set: (t, e, r, n) => {
    const a = t[e];
    return /* @__PURE__ */ Mt(a) && !/* @__PURE__ */ Mt(r) ? (a.value = r, !0) : Reflect.set(t, e, r, n);
  }
};
function eh(t) {
  return /* @__PURE__ */ Bn(t) ? t : new Proxy(t, Cy);
}
class Ty {
  constructor(e, r, n) {
    this.fn = e, this.setter = r, this._value = void 0, this.dep = new zu(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Qa - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !r, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    Xe !== this)
      return $d(this, !0), !0;
  }
  get value() {
    const e = this.dep.track();
    return Gd(this), e && (e.version = this.dep.version), this._value;
  }
  set value(e) {
    this.setter && this.setter(e);
  }
}
// @__NO_SIDE_EFFECTS__
function Sy(t, e, r = !1) {
  let n, a;
  return Re(t) ? n = t : (n = t.get, a = t.set), new Ty(n, a, r);
}
const Li = {}, fs = /* @__PURE__ */ new WeakMap();
let En;
function Py(t, e = !1, r = En) {
  if (r) {
    let n = fs.get(r);
    n || fs.set(r, n = []), n.push(t);
  }
}
function Dy(t, e, r = We) {
  const { immediate: n, deep: a, once: i, scheduler: s, augmentJob: o, call: l } = r, u = (w) => a ? w : /* @__PURE__ */ rr(w) || a === !1 || a === 0 ? Nr(w, 1) : Nr(w);
  let f, c, v, d, h = !1, y = !1;
  if (/* @__PURE__ */ Mt(t) ? (c = () => t.value, h = /* @__PURE__ */ rr(t)) : /* @__PURE__ */ Bn(t) ? (c = () => u(t), h = !0) : ke(t) ? (y = !0, h = t.some((w) => /* @__PURE__ */ Bn(w) || /* @__PURE__ */ rr(w)), c = () => t.map((w) => {
    if (/* @__PURE__ */ Mt(w))
      return w.value;
    if (/* @__PURE__ */ Bn(w))
      return u(w);
    if (Re(w))
      return l ? l(w, 2) : w();
  })) : Re(t) ? e ? c = l ? () => l(t, 2) : t : c = () => {
    if (v) {
      Vr();
      try {
        v();
      } finally {
        qr();
      }
    }
    const w = En;
    En = f;
    try {
      return l ? l(t, 3, [d]) : t(d);
    } finally {
      En = w;
    }
  } : c = Tr, e && a) {
    const w = c, E = a === !0 ? 1 / 0 : a;
    c = () => Nr(w(), E);
  }
  const g = ty(), p = () => {
    f.stop(), g && g.active && Lu(g.effects, f);
  };
  if (i && e) {
    const w = e;
    e = (...E) => {
      const T = w(...E);
      return p(), T;
    };
  }
  let m = y ? new Array(t.length).fill(Li) : Li;
  const b = (w) => {
    if (!(!(f.flags & 1) || !f.dirty && !w))
      if (e) {
        const E = f.run();
        if (w || a || h || (y ? E.some((T, x) => Er(T, m[x])) : Er(E, m))) {
          v && v();
          const T = En;
          En = f;
          try {
            const x = [
              E,
              // pass undefined as the old value when it's changed for the first time
              m === Li ? void 0 : y && m[0] === Li ? [] : m,
              d
            ];
            m = E, l ? l(e, 3, x) : (
              // @ts-expect-error
              e(...x)
            );
          } finally {
            En = T;
          }
        }
      } else
        f.run();
  };
  return o && o(b), f = new Vd(c), f.scheduler = s ? () => s(b, !1) : b, d = (w) => Py(w, !1, f), v = f.onStop = () => {
    const w = fs.get(f);
    if (w) {
      if (l)
        l(w, 4);
      else
        for (const E of w) E();
      fs.delete(f);
    }
  }, e ? n ? b(!0) : m = f.run() : s ? s(b.bind(null, !0), !0) : f.run(), p.pause = f.pause.bind(f), p.resume = f.resume.bind(f), p.stop = p, p;
}
function Nr(t, e = 1 / 0, r) {
  if (e <= 0 || !Ge(t) || t.__v_skip || (r = r || /* @__PURE__ */ new Map(), (r.get(t) || 0) >= e))
    return t;
  if (r.set(t, e), e--, /* @__PURE__ */ Mt(t))
    Nr(t.value, e, r);
  else if (ke(t))
    for (let n = 0; n < t.length; n++)
      Nr(t[n], e, r);
  else if (Os(t) || ra(t))
    t.forEach((n) => {
      Nr(n, e, r);
    });
  else if (Od(t)) {
    for (const n in t)
      Nr(t[n], e, r);
    for (const n of Object.getOwnPropertySymbols(t))
      Object.prototype.propertyIsEnumerable.call(t, n) && Nr(t[n], e, r);
  }
  return t;
}
/**
* @vue/runtime-core v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function gi(t, e, r, n) {
  try {
    return n ? t(...n) : t();
  } catch (a) {
    qs(a, e, r);
  }
}
function cr(t, e, r, n) {
  if (Re(t)) {
    const a = gi(t, e, r, n);
    return a && Ld(a) && a.catch((i) => {
      qs(i, e, r);
    }), a;
  }
  if (ke(t)) {
    const a = [];
    for (let i = 0; i < t.length; i++)
      a.push(cr(t[i], e, r, n));
    return a;
  }
}
function qs(t, e, r, n = !0) {
  const a = e ? e.vnode : null, { errorHandler: i, throwUnhandledErrorInProduction: s } = e && e.appContext.config || We;
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
      Vr(), gi(i, null, 10, [
        t,
        l,
        u
      ]), qr();
      return;
    }
  }
  Ay(t, r, a, n, s);
}
function Ay(t, e, r, n = !0, a = !1) {
  if (a)
    throw t;
  console.error(t);
}
const zt = [];
let mr = -1;
const na = [];
let Qr = null, Zn = 0;
const th = /* @__PURE__ */ Promise.resolve();
let cs = null;
function rh(t) {
  const e = cs || th;
  return t ? e.then(this ? t.bind(this) : t) : e;
}
function ky(t) {
  let e = mr + 1, r = zt.length;
  for (; e < r; ) {
    const n = e + r >>> 1, a = zt[n], i = ei(a);
    i < t || i === t && a.flags & 2 ? e = n + 1 : r = n;
  }
  return e;
}
function Hu(t) {
  if (!(t.flags & 1)) {
    const e = ei(t), r = zt[zt.length - 1];
    !r || // fast path when the job id is larger than the tail
    !(t.flags & 2) && e >= ei(r) ? zt.push(t) : zt.splice(ky(e), 0, t), t.flags |= 1, nh();
  }
}
function nh() {
  cs || (cs = th.then(ih));
}
function By(t) {
  ke(t) ? na.push(...t) : Qr && t.id === -1 ? Qr.splice(Zn + 1, 0, t) : t.flags & 1 || (na.push(t), t.flags |= 1), nh();
}
function Af(t, e, r = mr + 1) {
  for (; r < zt.length; r++) {
    const n = zt[r];
    if (n && n.flags & 2) {
      if (t && n.id !== t.uid)
        continue;
      zt.splice(r, 1), r--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function ah(t) {
  if (na.length) {
    const e = [...new Set(na)].sort(
      (r, n) => ei(r) - ei(n)
    );
    if (na.length = 0, Qr) {
      Qr.push(...e);
      return;
    }
    for (Qr = e, Zn = 0; Zn < Qr.length; Zn++) {
      const r = Qr[Zn];
      r.flags & 4 && (r.flags &= -2), r.flags & 8 || r(), r.flags &= -2;
    }
    Qr = null, Zn = 0;
  }
}
const ei = (t) => t.id == null ? t.flags & 2 ? -1 : 1 / 0 : t.id;
function ih(t) {
  try {
    for (mr = 0; mr < zt.length; mr++) {
      const e = zt[mr];
      e && !(e.flags & 8) && (e.flags & 4 && (e.flags &= -2), gi(
        e,
        e.i,
        e.i ? 15 : 14
      ), e.flags & 4 || (e.flags &= -2));
    }
  } finally {
    for (; mr < zt.length; mr++) {
      const e = zt[mr];
      e && (e.flags &= -2);
    }
    mr = -1, zt.length = 0, ah(), cs = null, (zt.length || na.length) && ih();
  }
}
let Xt = null, sh = null;
function vs(t) {
  const e = Xt;
  return Xt = t, sh = t && t.type.__scopeId || null, e;
}
function Ry(t, e = Xt, r) {
  if (!e || t._n)
    return t;
  const n = (...a) => {
    n._d && Vf(-1);
    const i = vs(e), s = Rn.length;
    let o;
    try {
      o = t(...a);
    } finally {
      for (let l = Rn.length; l > s; l--) Lh();
      vs(i), n._d && Vf(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function Aa(t, e) {
  if (Xt === null)
    return t;
  const r = Gs(Xt), n = t.dirs || (t.dirs = []);
  for (let a = 0; a < e.length; a++) {
    let [i, s, o, l = We] = e[a];
    i && (Re(i) && (i = {
      mounted: i,
      updated: i
    }), i.deep && Nr(s), n.push({
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
function mn(t, e, r, n) {
  const a = t.dirs, i = e && e.dirs;
  for (let s = 0; s < a.length; s++) {
    const o = a[s];
    i && (o.oldValue = i[s].value);
    let l = o.dir[n];
    l && (Vr(), cr(l, r, 8, [
      t.el,
      o,
      t,
      e
    ]), qr());
  }
}
function My(t, e) {
  if (Rt) {
    let r = Rt.provides;
    const n = Rt.parent && Rt.parent.provides;
    n === r && (r = Rt.provides = Object.create(n)), r[t] = e;
  }
}
function Xi(t, e, r = !1) {
  const n = Bm();
  if (n || aa) {
    let a = aa ? aa._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (a && t in a)
      return a[t];
    if (arguments.length > 1)
      return r && Re(e) ? e.call(n && n.proxy) : e;
  }
}
const Ly = /* @__PURE__ */ Symbol.for("v-scx"), Iy = () => Xi(Ly);
function Zi(t, e, r) {
  return oh(t, e, r);
}
function oh(t, e, r = We) {
  const { immediate: n, deep: a, flush: i, once: s } = r, o = Pt({}, r), l = e && n || !e && i !== "post";
  let u;
  if (ri) {
    if (i === "sync") {
      const d = Iy();
      u = d.__watcherHandles || (d.__watcherHandles = []);
    } else if (!l) {
      const d = () => {
      };
      return d.stop = Tr, d.resume = Tr, d.pause = Tr, d;
    }
  }
  const f = Rt;
  o.call = (d, h, y) => cr(d, f, h, y);
  let c = !1;
  i === "post" ? o.scheduler = (d) => {
    Gt(d, f && f.suspense);
  } : i !== "sync" && (c = !0, o.scheduler = (d, h) => {
    h ? d() : Hu(d);
  }), o.augmentJob = (d) => {
    e && (d.flags |= 4), c && (d.flags |= 2, f && (d.id = f.uid, d.i = f));
  };
  const v = Dy(t, e, o);
  return ri && (u ? u.push(v) : l && v()), v;
}
function Oy(t, e, r) {
  const n = this.proxy, a = at(t) ? t.includes(".") ? lh(n, t) : () => n[t] : t.bind(n, n);
  let i;
  Re(e) ? i = e : (i = e.handler, r = e);
  const s = pi(this), o = oh(a, i.bind(n), r);
  return s(), o;
}
function lh(t, e) {
  const r = e.split(".");
  return () => {
    let n = t;
    for (let a = 0; a < r.length && n; a++)
      n = n[r[a]];
    return n;
  };
}
const _y = /* @__PURE__ */ Symbol("_vte"), Ny = (t) => t.__isTeleport, mo = /* @__PURE__ */ Symbol("_leaveCb");
function Uu(t, e) {
  t.shapeFlag & 6 && t.component ? (t.transition = e, Uu(t.component.subTree, e)) : t.shapeFlag & 128 ? (t.ssContent.transition = e.clone(t.ssContent), t.ssFallback.transition = e.clone(t.ssFallback)) : t.transition = e;
}
// @__NO_SIDE_EFFECTS__
function uh(t, e) {
  return Re(t) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    Pt({ name: t.name }, e, { setup: t })
  ) : t;
}
function fh(t) {
  t.ids = [t.ids[0] + t.ids[2]++ + "-", 0, 0];
}
function kf(t, e) {
  let r;
  return !!((r = Object.getOwnPropertyDescriptor(t, e)) && !r.configurable);
}
const ds = /* @__PURE__ */ new WeakMap();
function Ga(t, e, r, n, a = !1) {
  if (ke(t)) {
    t.forEach(
      (y, g) => Ga(
        y,
        e && (ke(e) ? e[g] : e),
        r,
        n,
        a
      )
    );
    return;
  }
  if (Wa(n) && !a) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Ga(t, e, r, n.component.subTree);
    return;
  }
  const i = n.shapeFlag & 4 ? Gs(n.component) : n.el, s = a ? null : i, { i: o, r: l } = t, u = e && e.r, f = o.refs === We ? o.refs = {} : o.refs, c = o.setupState, v = /* @__PURE__ */ Ve(c), d = c === We ? Md : (y) => kf(f, y) ? !1 : qe(v, y), h = (y, g) => !(g && kf(f, g));
  if (u != null && u !== l) {
    if (Bf(e), at(u))
      f[u] = null, d(u) && (c[u] = null);
    else if (/* @__PURE__ */ Mt(u)) {
      const y = e;
      h(u, y.k) && (u.value = null), y.k && (f[y.k] = null);
    }
  }
  if (Re(l))
    gi(l, o, 12, [s, f]);
  else {
    const y = at(l), g = /* @__PURE__ */ Mt(l);
    if (y || g) {
      const p = () => {
        if (t.f) {
          const m = y ? d(l) ? c[l] : f[l] : h() || !t.k ? l.value : f[t.k];
          if (a)
            ke(m) && Lu(m, i);
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
          p(), ds.delete(t);
        };
        m.id = -1, ds.set(t, m), Gt(m, r);
      } else
        Bf(t), p();
    }
  }
}
function Bf(t) {
  const e = ds.get(t);
  e && (e.flags |= 8, ds.delete(t));
}
zs().requestIdleCallback;
zs().cancelIdleCallback;
const Wa = (t) => !!t.type.__asyncLoader, ch = (t) => t.type.__isKeepAlive;
function Fy(t, e) {
  vh(t, "a", e);
}
function zy(t, e) {
  vh(t, "da", e);
}
function vh(t, e, r = Rt) {
  const n = t.__wdc || (t.__wdc = () => {
    let a = r;
    for (; a; ) {
      if (a.isDeactivated)
        return;
      a = a.parent;
    }
    return t();
  });
  if ($s(e, n, r), r) {
    let a = r.parent;
    for (; a && a.parent; )
      ch(a.parent.vnode) && Vy(n, e, r, a), a = a.parent;
  }
}
function Vy(t, e, r, n) {
  const a = $s(
    e,
    t,
    n,
    !0
    /* prepend */
  );
  gh(() => {
    Lu(n[e], a);
  }, r);
}
function $s(t, e, r = Rt, n = !1) {
  if (r) {
    const a = r[t] || (r[t] = []), i = e.__weh || (e.__weh = (...s) => {
      Vr();
      const o = pi(r), l = cr(e, r, t, s);
      return o(), qr(), l;
    });
    return n ? a.unshift(i) : a.push(i), i;
  }
}
const Gr = (t) => (e, r = Rt) => {
  (!ri || t === "sp") && $s(t, (...n) => e(...n), r);
}, qy = Gr("bm"), dh = Gr("m"), $y = Gr(
  "bu"
), Hy = Gr("u"), hh = Gr(
  "bum"
), gh = Gr("um"), Uy = Gr(
  "sp"
), Gy = Gr("rtg"), Wy = Gr("rtc");
function Ky(t, e = Rt) {
  $s("ec", t, e);
}
const Yy = "components";
function Xy(t, e) {
  return Qy(Yy, t, !0, e) || t;
}
const Zy = /* @__PURE__ */ Symbol.for("v-ndc");
function Qy(t, e, r = !0, n = !1) {
  const a = Xt || Rt;
  if (a) {
    const i = a.type;
    {
      const o = Om(
        i,
        !1
      );
      if (o && (o === e || o === Vt(e) || o === Ns(Vt(e))))
        return i;
    }
    const s = (
      // local registration
      // check instance[type] first which is resolved for options API
      Rf(a[t] || i[t], e) || // global registration
      Rf(a.appContext[t], e)
    );
    return !s && n ? i : s;
  }
}
function Rf(t, e) {
  return t && (t[e] || t[Vt(e)] || t[Ns(Vt(e))]);
}
function Qi(t, e, r, n) {
  let a;
  const i = r, s = ke(t);
  if (s || at(t)) {
    const o = s && /* @__PURE__ */ Bn(t);
    let l = !1, u = !1;
    o && (l = !/* @__PURE__ */ rr(t), u = /* @__PURE__ */ $r(t), t = Vs(t)), a = new Array(t.length);
    for (let f = 0, c = t.length; f < c; f++)
      a[f] = e(
        l ? u ? ca(fr(t[f])) : fr(t[f]) : t[f],
        f,
        void 0,
        i
      );
  } else if (typeof t == "number") {
    a = new Array(t);
    for (let o = 0; o < t; o++)
      a[o] = e(o + 1, o, void 0, i);
  } else if (Ge(t))
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
const iu = (t) => t ? Nh(t) ? Gs(t) : iu(t.parent) : null, Ka = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ Pt(/* @__PURE__ */ Object.create(null), {
    $: (t) => t,
    $el: (t) => t.vnode.el,
    $data: (t) => t.data,
    $props: (t) => t.props,
    $attrs: (t) => t.attrs,
    $slots: (t) => t.slots,
    $refs: (t) => t.refs,
    $parent: (t) => iu(t.parent),
    $root: (t) => iu(t.root),
    $host: (t) => t.ce,
    $emit: (t) => t.emit,
    $options: (t) => yh(t),
    $forceUpdate: (t) => t.f || (t.f = () => {
      Hu(t.update);
    }),
    $nextTick: (t) => t.n || (t.n = rh.bind(t.proxy)),
    $watch: (t) => Oy.bind(t)
  })
), bo = (t, e) => t !== We && !t.__isScriptSetup && qe(t, e), jy = {
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
        if (bo(n, e))
          return s[e] = 1, n[e];
        if (a !== We && qe(a, e))
          return s[e] = 2, a[e];
        if (qe(i, e))
          return s[e] = 3, i[e];
        if (r !== We && qe(r, e))
          return s[e] = 4, r[e];
        su && (s[e] = 0);
      }
    }
    const u = Ka[e];
    let f, c;
    if (u)
      return e === "$attrs" && Bt(t.attrs, "get", ""), u(t);
    if (
      // css module (injected by vue-loader)
      (f = o.__cssModules) && (f = f[e])
    )
      return f;
    if (r !== We && qe(r, e))
      return s[e] = 4, r[e];
    if (
      // global properties
      c = l.config.globalProperties, qe(c, e)
    )
      return c[e];
  },
  set({ _: t }, e, r) {
    const { data: n, setupState: a, ctx: i } = t;
    return bo(a, e) ? (a[e] = r, !0) : n !== We && qe(n, e) ? (n[e] = r, !0) : qe(t.props, e) || e[0] === "$" && e.slice(1) in t ? !1 : (i[e] = r, !0);
  },
  has({
    _: { data: t, setupState: e, accessCache: r, ctx: n, appContext: a, props: i, type: s }
  }, o) {
    let l;
    return !!(r[o] || t !== We && o[0] !== "$" && qe(t, o) || bo(e, o) || qe(i, o) || qe(n, o) || qe(Ka, o) || qe(a.config.globalProperties, o) || (l = s.__cssModules) && l[o]);
  },
  defineProperty(t, e, r) {
    return r.get != null ? t._.accessCache[e] = 0 : qe(r, "value") && this.set(t, e, r.value, null), Reflect.defineProperty(t, e, r);
  }
};
function Mf(t) {
  return ke(t) ? t.reduce(
    (e, r) => (e[r] = null, e),
    {}
  ) : t;
}
let su = !0;
function Jy(t) {
  const e = yh(t), r = t.proxy, n = t.ctx;
  su = !1, e.beforeCreate && Lf(e.beforeCreate, t, "bc");
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
  if (u && em(u, n, null), s)
    for (const L in s) {
      const N = s[L];
      Re(N) && (n[L] = N.bind(r));
    }
  if (a) {
    const L = a.call(r, r);
    Ge(L) && (t.data = /* @__PURE__ */ Ja(L));
  }
  if (su = !0, i)
    for (const L in i) {
      const N = i[L], H = Re(N) ? N.bind(r, r) : Re(N.get) ? N.get.bind(r, r) : Tr, V = !Re(N) && Re(N.set) ? N.set.bind(r) : Tr, F = Nm({
        get: H,
        set: V
      });
      Object.defineProperty(n, L, {
        enumerable: !0,
        configurable: !0,
        get: () => F.value,
        set: ($) => F.value = $
      });
    }
  if (o)
    for (const L in o)
      ph(o[L], n, r, L);
  if (l) {
    const L = Re(l) ? l.call(r) : l;
    Reflect.ownKeys(L).forEach((N) => {
      My(N, L[N]);
    });
  }
  f && Lf(f, t, "c");
  function O(L, N) {
    ke(N) ? N.forEach((H) => L(H.bind(r))) : N && L(N.bind(r));
  }
  if (O(qy, c), O(dh, v), O($y, d), O(Hy, h), O(Fy, y), O(zy, g), O(Ky, S), O(Wy, T), O(Gy, x), O(hh, m), O(gh, w), O(Uy, D), ke(A))
    if (A.length) {
      const L = t.exposed || (t.exposed = {});
      A.forEach((N) => {
        Object.defineProperty(L, N, {
          get: () => r[N],
          set: (H) => r[N] = H,
          enumerable: !0
        });
      });
    } else t.exposed || (t.exposed = {});
  E && t.render === Tr && (t.render = E), k != null && (t.inheritAttrs = k), R && (t.components = R), M && (t.directives = M), D && fh(t);
}
function em(t, e, r = Tr) {
  ke(t) && (t = ou(t));
  for (const n in t) {
    const a = t[n];
    let i;
    Ge(a) ? "default" in a ? i = Xi(
      a.from || n,
      a.default,
      !0
    ) : i = Xi(a.from || n) : i = Xi(a), /* @__PURE__ */ Mt(i) ? Object.defineProperty(e, n, {
      enumerable: !0,
      configurable: !0,
      get: () => i.value,
      set: (s) => i.value = s
    }) : e[n] = i;
  }
}
function Lf(t, e, r) {
  cr(
    ke(t) ? t.map((n) => n.bind(e.proxy)) : t.bind(e.proxy),
    e,
    r
  );
}
function ph(t, e, r, n) {
  let a = n.includes(".") ? lh(r, n) : () => r[n];
  if (at(t)) {
    const i = e[t];
    Re(i) && Zi(a, i);
  } else if (Re(t))
    Zi(a, t.bind(r));
  else if (Ge(t))
    if (ke(t))
      t.forEach((i) => ph(i, e, r, n));
    else {
      const i = Re(t.handler) ? t.handler.bind(r) : e[t.handler];
      Re(i) && Zi(a, i, t);
    }
}
function yh(t) {
  const e = t.type, { mixins: r, extends: n } = e, {
    mixins: a,
    optionsCache: i,
    config: { optionMergeStrategies: s }
  } = t.appContext, o = i.get(e);
  let l;
  return o ? l = o : !a.length && !r && !n ? l = e : (l = {}, a.length && a.forEach(
    (u) => hs(l, u, s, !0)
  ), hs(l, e, s)), Ge(e) && i.set(e, l), l;
}
function hs(t, e, r, n = !1) {
  const { mixins: a, extends: i } = e;
  i && hs(t, i, r, !0), a && a.forEach(
    (s) => hs(t, s, r, !0)
  );
  for (const s in e)
    if (!(n && s === "expose")) {
      const o = tm[s] || r && r[s];
      t[s] = o ? o(t[s], e[s]) : e[s];
    }
  return t;
}
const tm = {
  data: If,
  props: Of,
  emits: Of,
  // objects
  methods: Na,
  computed: Na,
  // lifecycle
  beforeCreate: _t,
  created: _t,
  beforeMount: _t,
  mounted: _t,
  beforeUpdate: _t,
  updated: _t,
  beforeDestroy: _t,
  beforeUnmount: _t,
  destroyed: _t,
  unmounted: _t,
  activated: _t,
  deactivated: _t,
  errorCaptured: _t,
  serverPrefetch: _t,
  // assets
  components: Na,
  directives: Na,
  // watch
  watch: nm,
  // provide / inject
  provide: If,
  inject: rm
};
function If(t, e) {
  return e ? t ? function() {
    return Pt(
      Re(t) ? t.call(this, this) : t,
      Re(e) ? e.call(this, this) : e
    );
  } : e : t;
}
function rm(t, e) {
  return Na(ou(t), ou(e));
}
function ou(t) {
  if (ke(t)) {
    const e = {};
    for (let r = 0; r < t.length; r++)
      e[t[r]] = t[r];
    return e;
  }
  return t;
}
function _t(t, e) {
  return t ? [...new Set([].concat(t, e))] : e;
}
function Na(t, e) {
  return t ? Pt(/* @__PURE__ */ Object.create(null), t, e) : e;
}
function Of(t, e) {
  return t ? ke(t) && ke(e) ? [.../* @__PURE__ */ new Set([...t, ...e])] : Pt(
    /* @__PURE__ */ Object.create(null),
    Mf(t),
    Mf(e ?? {})
  ) : e;
}
function nm(t, e) {
  if (!t) return e;
  if (!e) return t;
  const r = Pt(/* @__PURE__ */ Object.create(null), t);
  for (const n in e)
    r[n] = _t(t[n], e[n]);
  return r;
}
function mh() {
  return {
    app: null,
    config: {
      isNativeTag: Md,
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
let am = 0;
function im(t, e) {
  return function(n, a = null) {
    Re(n) || (n = Pt({}, n)), a != null && !Ge(a) && (a = null);
    const i = mh(), s = /* @__PURE__ */ new WeakSet(), o = [];
    let l = !1;
    const u = i.app = {
      _uid: am++,
      _component: n,
      _props: a,
      _container: null,
      _context: i,
      _instance: null,
      version: Fm,
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
          const d = u._ceVNode || zr(n, a);
          return d.appContext = i, v === !0 ? v = "svg" : v === !1 && (v = void 0), t(d, f, v), l = !0, u._container = f, f.__vue_app__ = u, Gs(d.component);
        }
      },
      onUnmount(f) {
        o.push(f);
      },
      unmount() {
        l && (cr(
          o,
          u._instance,
          16
        ), t(null, u._container), delete u._container.__vue_app__);
      },
      provide(f, c) {
        return i.provides[f] = c, u;
      },
      runWithContext(f) {
        const c = aa;
        aa = u;
        try {
          return f();
        } finally {
          aa = c;
        }
      }
    };
    return u;
  };
}
let aa = null;
const sm = (t, e) => e === "modelValue" || e === "model-value" ? t.modelModifiers : t[`${e}Modifiers`] || t[`${Vt(e)}Modifiers`] || t[`${Fn(e)}Modifiers`];
function om(t, e, ...r) {
  if (t.isUnmounted) return;
  const n = t.vnode.props || We;
  let a = r;
  const i = e.startsWith("update:"), s = i && sm(n, e.slice(7));
  s && (s.trim && (a = r.map((f) => at(f) ? f.trim() : f)), s.number && (a = r.map(Fs)));
  let o, l = n[o = vo(e)] || // also try camelCase event handler (#2249)
  n[o = vo(Vt(e))];
  !l && i && (l = n[o = vo(Fn(e))]), l && cr(
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
    t.emitted[o] = !0, cr(
      u,
      t,
      6,
      a
    );
  }
}
const lm = /* @__PURE__ */ new WeakMap();
function bh(t, e, r = !1) {
  const n = r ? lm : e.emitsCache, a = n.get(t);
  if (a !== void 0)
    return a;
  const i = t.emits;
  let s = {}, o = !1;
  if (!Re(t)) {
    const l = (u) => {
      const f = bh(u, e, !0);
      f && (o = !0, Pt(s, f));
    };
    !r && e.mixins.length && e.mixins.forEach(l), t.extends && l(t.extends), t.mixins && t.mixins.forEach(l);
  }
  return !i && !o ? (Ge(t) && n.set(t, null), null) : (ke(i) ? i.forEach((l) => s[l] = null) : Pt(s, i), Ge(t) && n.set(t, s), s);
}
function Hs(t, e) {
  return !t || !Ls(e) ? !1 : (e = e.slice(2), e = e === "Once" ? e : e.replace(/Once$/, ""), qe(t, e[0].toLowerCase() + e.slice(1)) || qe(t, Fn(e)) || qe(t, e));
}
function _f(t) {
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
  } = t, g = vs(t);
  let p, m;
  try {
    if (r.shapeFlag & 4) {
      const w = a || n, E = w;
      p = xr(
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
      p = xr(
        w.length > 1 ? w(
          c,
          { attrs: o, slots: s, emit: l }
        ) : w(
          c,
          null
        )
      ), m = e.props ? o : um(o);
    }
  } catch (w) {
    Rn.length = 0, qs(w, t, 1), p = zr(an);
  }
  let b = p;
  if (m && y !== !1) {
    const w = Object.keys(m), { shapeFlag: E } = b;
    w.length && E & 7 && (i && w.some(Is) && (m = fm(
      m,
      i
    )), b = va(b, m, !1, !0));
  }
  return r.dirs && (b = va(b, null, !1, !0), b.dirs = b.dirs ? b.dirs.concat(r.dirs) : r.dirs), r.transition && Uu(b, r.transition), p = b, vs(g), p;
}
const um = (t) => {
  let e;
  for (const r in t)
    (r === "class" || r === "style" || Ls(r)) && ((e || (e = {}))[r] = t[r]);
  return e;
}, fm = (t, e) => {
  const r = {};
  for (const n in t)
    (!Is(n) || !(n.slice(9) in e)) && (r[n] = t[n]);
  return r;
};
function cm(t, e, r) {
  const { props: n, children: a, component: i } = t, { props: s, children: o, patchFlag: l } = e, u = i.emitsOptions;
  if (e.dirs || e.transition)
    return !0;
  if (r && l >= 0) {
    if (l & 1024)
      return !0;
    if (l & 16)
      return n ? Nf(n, s, u) : !!s;
    if (l & 8) {
      const f = e.dynamicProps;
      for (let c = 0; c < f.length; c++) {
        const v = f[c];
        if (wh(s, n, v) && !Hs(u, v))
          return !0;
      }
    }
  } else
    return (a || o) && (!o || !o.$stable) ? !0 : n === s ? !1 : n ? s ? Nf(n, s, u) : !0 : !!s;
  return !1;
}
function Nf(t, e, r) {
  const n = Object.keys(e);
  if (n.length !== Object.keys(t).length)
    return !0;
  for (let a = 0; a < n.length; a++) {
    const i = n[a];
    if (wh(e, t, i) && !Hs(r, i))
      return !0;
  }
  return !1;
}
function wh(t, e, r) {
  const n = t[r], a = e[r];
  return r === "style" && Ge(n) && Ge(a) ? !hi(n, a) : n !== a;
}
function vm({ vnode: t, parent: e, suspense: r }, n) {
  for (; e; ) {
    const a = e.subTree;
    if (a.suspense && a.suspense.activeBranch === t && (a.suspense.vnode.el = a.el = n, t = a), a === t)
      (t = e.vnode).el = n, e = e.parent;
    else
      break;
  }
  r && r.activeBranch === t && (r.vnode.el = n);
}
const xh = {}, Eh = () => Object.create(xh), Ch = (t) => Object.getPrototypeOf(t) === xh;
function dm(t, e, r, n = !1) {
  const a = {}, i = Eh();
  t.propsDefaults = /* @__PURE__ */ Object.create(null), Th(t, e, a, i);
  for (const s in t.propsOptions[0])
    s in a || (a[s] = void 0);
  r ? t.props = n ? a : /* @__PURE__ */ by(a) : t.type.props ? t.props = a : t.props = i, t.attrs = i;
}
function hm(t, e, r, n) {
  const {
    props: a,
    attrs: i,
    vnode: { patchFlag: s }
  } = t, o = /* @__PURE__ */ Ve(a), [l] = t.propsOptions;
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
        if (Hs(t.emitsOptions, v))
          continue;
        const d = e[v];
        if (l)
          if (qe(i, v))
            d !== i[v] && (i[v] = d, u = !0);
          else {
            const h = Vt(v);
            a[h] = lu(
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
    Th(t, e, a, i) && (u = !0);
    let f;
    for (const c in o)
      (!e || // for camelCase
      !qe(e, c) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((f = Fn(c)) === c || !qe(e, f))) && (l ? r && // for camelCase
      (r[c] !== void 0 || // for kebab-case
      r[f] !== void 0) && (a[c] = lu(
        l,
        o,
        c,
        void 0,
        t,
        !0
      )) : delete a[c]);
    if (i !== o)
      for (const c in i)
        (!e || !qe(e, c)) && (delete i[c], u = !0);
  }
  u && _r(t.attrs, "set", "");
}
function Th(t, e, r, n) {
  const [a, i] = t.propsOptions;
  let s = !1, o;
  if (e)
    for (let l in e) {
      if ($a(l))
        continue;
      const u = e[l];
      let f;
      a && qe(a, f = Vt(l)) ? !i || !i.includes(f) ? r[f] = u : (o || (o = {}))[f] = u : Hs(t.emitsOptions, l) || (!(l in n) || u !== n[l]) && (n[l] = u, s = !0);
    }
  if (i) {
    const l = /* @__PURE__ */ Ve(r), u = o || We;
    for (let f = 0; f < i.length; f++) {
      const c = i[f];
      r[c] = lu(
        a,
        l,
        c,
        u[c],
        t,
        !qe(u, c)
      );
    }
  }
  return s;
}
function lu(t, e, r, n, a, i) {
  const s = t[r];
  if (s != null) {
    const o = qe(s, "default");
    if (o && n === void 0) {
      const l = s.default;
      if (s.type !== Function && !s.skipFactory && Re(l)) {
        const { propsDefaults: u } = a;
        if (r in u)
          n = u[r];
        else {
          const f = pi(a);
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
    ] && (n === "" || n === Fn(r)) && (n = !0));
  }
  return n;
}
const gm = /* @__PURE__ */ new WeakMap();
function Sh(t, e, r = !1) {
  const n = r ? gm : e.propsCache, a = n.get(t);
  if (a)
    return a;
  const i = t.props, s = {}, o = [];
  let l = !1;
  if (!Re(t)) {
    const f = (c) => {
      l = !0;
      const [v, d] = Sh(c, e, !0);
      Pt(s, v), d && o.push(...d);
    };
    !r && e.mixins.length && e.mixins.forEach(f), t.extends && f(t.extends), t.mixins && t.mixins.forEach(f);
  }
  if (!i && !l)
    return Ge(t) && n.set(t, ta), ta;
  if (ke(i))
    for (let f = 0; f < i.length; f++) {
      const c = Vt(i[f]);
      Ff(c) && (s[c] = We);
    }
  else if (i)
    for (const f in i) {
      const c = Vt(f);
      if (Ff(c)) {
        const v = i[f], d = s[c] = ke(v) || Re(v) ? { type: v } : Pt({}, v), h = d.type;
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
        ] = g, (y || qe(d, "default")) && o.push(c);
      }
    }
  const u = [s, o];
  return Ge(t) && n.set(t, u), u;
}
function Ff(t) {
  return t[0] !== "$" && !$a(t);
}
const Gu = (t) => t === "_" || t === "_ctx" || t === "$stable", Wu = (t) => ke(t) ? t.map(xr) : [xr(t)], pm = (t, e, r) => {
  if (e._n)
    return e;
  const n = Ry((...a) => Wu(e(...a)), r);
  return n._c = !1, n;
}, Ph = (t, e, r) => {
  const n = t._ctx;
  for (const a in t) {
    if (Gu(a)) continue;
    const i = t[a];
    if (Re(i))
      e[a] = pm(a, i, n);
    else if (i != null) {
      const s = Wu(i);
      e[a] = () => s;
    }
  }
}, Dh = (t, e) => {
  const r = Wu(e);
  t.slots.default = () => r;
}, Ah = (t, e, r) => {
  for (const n in e)
    (r || !Gu(n)) && (t[n] = e[n]);
}, ym = (t, e, r) => {
  const n = t.slots = Eh();
  if (t.vnode.shapeFlag & 32) {
    const a = e._;
    a ? (Ah(n, e, r), r && _d(n, "_", a, !0)) : Ph(e, n);
  } else e && Dh(t, e);
}, mm = (t, e, r) => {
  const { vnode: n, slots: a } = t;
  let i = !0, s = We;
  if (n.shapeFlag & 32) {
    const o = e._;
    o ? r && o === 1 ? i = !1 : Ah(a, e, r) : (i = !e.$stable, Ph(e, a)), s = e;
  } else e && (Dh(t, e), s = { default: 1 });
  if (i)
    for (const o in a)
      !Gu(o) && s[o] == null && delete a[o];
}, Gt = Cm;
function bm(t) {
  return wm(t);
}
function wm(t, e) {
  const r = zs();
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
    setScopeId: d = Tr,
    insertStaticContent: h
  } = t, y = (C, B, z, W = null, j = null, Z = null, ne = void 0, te = null, Y = !!B.dynamicChildren) => {
    if (C === B)
      return;
    C && !ka(C, B) && (W = ce(C), $(C, j, Z, !0), C = null), B.patchFlag === -2 && (Y = !1, B.dynamicChildren = null);
    const { type: K, ref: ue, shapeFlag: oe } = B;
    switch (K) {
      case Us:
        g(C, B, z, W);
        break;
      case an:
        p(C, B, z, W);
        break;
      case xo:
        C == null && m(B, z, W, ne);
        break;
      case Yt:
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
    ue != null && j ? Ga(ue, C && C.ref, Z, B || C, !B) : ue == null && C && C.ref != null && Ga(C.ref, null, Z, C, !0);
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
      wo(C, Z),
      ne,
      te
    ), de && mn(C, null, W, "created"), x(Y, C, C.scopeId, ne, W), ue) {
      for (const Te in ue)
        Te !== "value" && !$a(Te) && i(Y, Te, null, ue[Te], Z, W);
      "value" in ue && i(Y, "value", null, ue.value, Z), (K = ue.onVnodeBeforeMount) && hr(K, W, C);
    }
    de && mn(C, null, W, "beforeMount");
    const me = xm(j, ve);
    me && ve.beforeEnter(Y), n(Y, B, z), ((K = ue && ue.onVnodeMounted) || me || de) && Gt(() => {
      try {
        K && hr(K, W, C), me && ve.enter(Y), de && mn(C, null, W, "mounted");
      } finally {
      }
    }, j);
  }, x = (C, B, z, W, j) => {
    if (z && d(C, z), W)
      for (let Z = 0; Z < W.length; Z++)
        d(C, W[Z]);
    if (j) {
      let Z = j.subTree;
      if (B === Z || Mh(Z.type) && (Z.ssContent === B || Z.ssFallback === B)) {
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
      const ue = C[K] = te ? Or(C[K]) : xr(C[K]);
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
    const oe = C.props || We, ve = B.props || We;
    let de;
    if (z && bn(z, !1), (de = ve.onVnodeBeforeUpdate) && hr(de, z, B, C), ue && mn(B, C, z, "beforeUpdate"), z && bn(z, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    K && (!C.dynamicChildren || C.dynamicChildren.length !== K.length) && (Y = 0, ne = !1, K = null), (oe.innerHTML && ve.innerHTML == null || oe.textContent && ve.textContent == null) && f(te, ""), K ? A(
      C.dynamicChildren,
      K,
      te,
      z,
      W,
      wo(B, j),
      Z
    ) : ne || N(
      C,
      B,
      te,
      null,
      z,
      W,
      wo(B, j),
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
    ((de = ve.onVnodeUpdated) || ue) && Gt(() => {
      de && hr(de, z, B, C), ue && mn(B, C, z, "updated");
    }, W);
  }, A = (C, B, z, W, j, Z, ne) => {
    for (let te = 0; te < B.length; te++) {
      const Y = C[te], K = B[te], ue = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        Y.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (Y.type === Yt || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !ka(Y, K) || // - In the case of a component, it could contain anything.
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
      if (B !== We)
        for (const Z in B)
          !$a(Z) && !(Z in z) && i(
            C,
            Z,
            B[Z],
            null,
            j,
            W
          );
      for (const Z in z) {
        if ($a(Z)) continue;
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
    (B.key != null || j && B === j.subTree) && kh(
      C,
      B,
      !0
      /* shallow */
    )) : N(
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
    const te = C.component = km(
      C,
      W,
      j
    );
    if (ch(C) && (te.ctx.renderer = U), Rm(te, !1, ne), te.asyncDep) {
      if (j && j.registerDep(te, O, ne), !C.el) {
        const Y = te.subTree = zr(an);
        p(null, Y, B, z), C.placeholder = Y.el;
      }
    } else
      O(
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
    if (cm(C, B, z))
      if (W.asyncDep && !W.asyncResolved) {
        L(W, B, z);
        return;
      } else
        W.next = B, W.update();
    else
      B.el = C.el, W.vnode = B;
  }, O = (C, B, z, W, j, Z, ne) => {
    const te = () => {
      if (C.isMounted) {
        let { next: oe, bu: ve, u: de, parent: me, vnode: Te } = C;
        {
          const q = Bh(C);
          if (q) {
            oe && (oe.el = Te.el, L(C, oe, ne)), q.asyncDep.then(() => {
              Gt(() => {
                C.isUnmounted || K();
              }, j);
            });
            return;
          }
        }
        let Ee = oe, Pe;
        bn(C, !1), oe ? (oe.el = Te.el, L(C, oe, ne)) : oe = Te, ve && Yi(ve), (Pe = oe.props && oe.props.onVnodeBeforeUpdate) && hr(Pe, me, oe, Te), bn(C, !0);
        const J = _f(C), P = C.subTree;
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
        ), oe.el = J.el, Ee === null && vm(C, J.el), de && Gt(de, j), (Pe = oe.props && oe.props.onVnodeUpdated) && Gt(
          () => hr(Pe, me, oe, Te),
          j
        );
      } else {
        let oe;
        const { el: ve, props: de } = B, { bm: me, m: Te, parent: Ee, root: Pe, type: J } = C, P = Wa(B);
        bn(C, !1), me && Yi(me), !P && (oe = de && de.onVnodeBeforeMount) && hr(oe, Ee, B), bn(C, !0);
        {
          Pe.ce && Pe.ce._hasShadowRoot() && Pe.ce._injectChildStyle(
            J,
            C.parent ? C.parent.type : void 0
          );
          const q = C.subTree = _f(C);
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
        if (Te && Gt(Te, j), !P && (oe = de && de.onVnodeMounted)) {
          const q = B;
          Gt(
            () => hr(oe, Ee, q),
            j
          );
        }
        (B.shapeFlag & 256 || Ee && Wa(Ee.vnode) && Ee.vnode.shapeFlag & 256) && C.a && Gt(C.a, j), C.isMounted = !0, B = z = W = null;
      }
    };
    C.scope.on();
    const Y = C.effect = new Vd(te);
    C.scope.off();
    const K = C.update = Y.run.bind(Y), ue = C.job = Y.runIfDirty.bind(Y);
    ue.i = C, ue.id = C.uid, Y.scheduler = () => Hu(ue), bn(C, !0), K();
  }, L = (C, B, z) => {
    B.component = C;
    const W = C.vnode.props;
    C.vnode = B, C.next = null, hm(C, B.props, W, z), mm(C, B.children, z), Vr(), Af(C), qr();
  }, N = (C, B, z, W, j, Z, ne, te, Y = !1) => {
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
    C = C || ta, B = B || ta;
    const K = C.length, ue = B.length, oe = Math.min(K, ue);
    let ve;
    for (ve = 0; ve < oe; ve++) {
      const de = B[ve] = Y ? Or(B[ve]) : xr(B[ve]);
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
      const de = C[K], me = B[K] = Y ? Or(B[K]) : xr(B[K]);
      if (ka(de, me))
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
      const de = C[oe], me = B[ve] = Y ? Or(B[ve]) : xr(B[ve]);
      if (ka(de, me))
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
            B[K] = Y ? Or(B[K]) : xr(B[K]),
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
        const ee = B[K] = Y ? Or(B[K]) : xr(B[K]);
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
            if (G[Ee - me] === 0 && ka(ee, B[Ee])) {
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
      const re = P ? Em(G) : ta;
      for (Ee = re.length - 1, K = J - 1; K >= 0; K--) {
        const ee = me + K, ye = B[ee], fe = B[ee + 1], ge = ee + 1 < ue ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          fe.el || Rh(fe)
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
        ) : P && (Ee < 0 || K !== re[Ee] ? F(ye, z, ge, 2) : Ee--);
      }
    }
  }, F = (C, B, z, W, j = null) => {
    const { el: Z, type: ne, transition: te, children: Y, shapeFlag: K } = C;
    if (K & 6) {
      F(C.component.subTree, B, z, W);
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
    if (ne === Yt) {
      n(Z, B, z);
      for (let oe = 0; oe < Y.length; oe++)
        F(Y[oe], B, z, W);
      n(C.anchor, B, z);
      return;
    }
    if (ne === xo) {
      b(C, B, z);
      return;
    }
    if (W !== 2 && K & 1 && te)
      if (W === 0)
        te.persisted && !Z[mo] ? n(Z, B, z) : (te.beforeEnter(Z), n(Z, B, z), Gt(() => te.enter(Z), j));
      else {
        const { leave: oe, delayLeave: ve, afterLeave: de } = te, me = () => {
          C.ctx.isUnmounted ? a(Z) : n(Z, B, z);
        }, Te = () => {
          const Ee = Z._isLeaving || !!Z[mo];
          Z._isLeaving && Z[mo](
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
    if (oe === -2 && (j = !1), te != null && (Vr(), Ga(te, null, z, C, !0), qr()), de != null && (B.renderCache[de] = void 0), ue & 256) {
      B.ctx.deactivate(C);
      return;
    }
    const Te = ue & 1 && ve, Ee = !Wa(C);
    let Pe;
    if (Ee && (Pe = ne && ne.onVnodeBeforeUnmount) && hr(Pe, B, C), ue & 6)
      ae(C.component, z, W);
    else {
      if (ue & 128) {
        C.suspense.unmount(z, W);
        return;
      }
      Te && mn(C, null, B, "beforeUnmount"), ue & 64 ? C.type.remove(
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
      (Z !== Yt || oe > 0 && oe & 64) ? le(
        K,
        B,
        z,
        !1,
        !0
      ) : (Z === Yt && oe & 384 || !j && ue & 16) && le(Y, B, z), W && Q(C);
    }
    const J = me != null && de == null;
    (Ee && (Pe = ne && ne.onVnodeUnmounted) || Te || J) && Gt(() => {
      Pe && hr(Pe, B, C), Te && mn(C, null, B, "unmounted"), J && (C.el = null);
    }, z);
  }, Q = (C) => {
    const { type: B, el: z, anchor: W, transition: j } = C;
    if (B === Yt) {
      se(z, W);
      return;
    }
    if (B === xo) {
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
    zf(Y), zf(K), W && Yi(W), j.stop(), Z && (Z.flags |= 8, $(ne, C, B, z)), te && Gt(te, B), Gt(() => {
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
    const B = v(C.anchor || C.el), z = B && B[_y];
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
    ), B._vnode = C, he || (he = !0, Af(W), ah(), he = !1);
  }, U = {
    p: y,
    um: $,
    m: F,
    r: Q,
    mt: I,
    mc: S,
    pc: N,
    pbc: A,
    n: ce,
    o: t
  };
  return {
    render: ie,
    hydrate: void 0,
    createApp: im(ie)
  };
}
function wo({ type: t, props: e }, r) {
  return r === "svg" && t === "foreignObject" || r === "mathml" && t === "annotation-xml" && e && e.encoding && e.encoding.includes("html") ? void 0 : r;
}
function bn({ effect: t, job: e }, r) {
  r ? (t.flags |= 32, e.flags |= 4) : (t.flags &= -33, e.flags &= -5);
}
function xm(t, e) {
  return (!t || t && !t.pendingBranch) && e && !e.persisted;
}
function kh(t, e, r = !1) {
  const n = t.children, a = e.children;
  if (ke(n) && ke(a))
    for (let i = 0; i < n.length; i++) {
      const s = n[i];
      let o = a[i];
      o.shapeFlag & 1 && !o.dynamicChildren && ((o.patchFlag <= 0 || o.patchFlag === 32) && (o = a[i] = Or(a[i]), o.el = s.el), !r && o.patchFlag !== -2 && kh(s, o)), o.type === Us && (o.patchFlag === -1 && (o = a[i] = Or(o)), o.el = s.el), o.type === an && !o.el && (o.el = s.el);
    }
}
function Em(t) {
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
function Bh(t) {
  const e = t.subTree.component;
  if (e)
    return e.asyncDep && !e.asyncResolved ? e : Bh(e);
}
function zf(t) {
  if (t)
    for (let e = 0; e < t.length; e++)
      t[e].flags |= 8;
}
function Rh(t) {
  if (t.placeholder)
    return t.placeholder;
  const e = t.component;
  return e ? Rh(e.subTree) : null;
}
const Mh = (t) => t.__isSuspense;
function Cm(t, e) {
  e && e.pendingBranch ? ke(t) ? e.effects.push(...t) : e.effects.push(t) : By(t);
}
const Yt = /* @__PURE__ */ Symbol.for("v-fgt"), Us = /* @__PURE__ */ Symbol.for("v-txt"), an = /* @__PURE__ */ Symbol.for("v-cmt"), xo = /* @__PURE__ */ Symbol.for("v-stc"), Rn = [];
let Zt = null;
function et(t = !1) {
  Rn.push(Zt = t ? null : []);
}
function Lh() {
  Rn.pop(), Zt = Rn[Rn.length - 1] || null;
}
let ti = 1;
function Vf(t, e = !1) {
  ti += t, t < 0 && Zt && e && (Zt.hasOnce = !0);
}
function Ih(t) {
  return t.dynamicChildren = ti > 0 ? Zt || ta : null, Lh(), ti > 0 && Zt && Zt.push(t), t;
}
function ut(t, e, r, n, a, i) {
  return Ih(
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
function Ku(t, e, r, n, a) {
  return Ih(
    zr(
      t,
      e,
      r,
      n,
      a,
      !0
    )
  );
}
function Oh(t) {
  return t ? t.__v_isVNode === !0 : !1;
}
function ka(t, e) {
  return t.type === e.type && t.key === e.key;
}
const _h = ({ key: t }) => t ?? null, ji = ({
  ref: t,
  ref_key: e,
  ref_for: r
}) => (typeof t == "number" && (t = "" + t), t != null ? at(t) || /* @__PURE__ */ Mt(t) || Re(t) ? { i: Xt, r: t, k: e, f: !!r } : t : null);
function Ce(t, e = null, r = null, n = 0, a = null, i = t === Yt ? 0 : 1, s = !1, o = !1) {
  const l = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: t,
    props: e,
    key: e && _h(e),
    ref: e && ji(e),
    scopeId: sh,
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
    ctx: Xt
  };
  return o ? (gs(l, r), i & 128 && t.normalize(l)) : r && (l.shapeFlag |= at(r) ? 8 : 16), ti > 0 && // avoid a block node from tracking itself
  !s && // has current parent block
  Zt && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (l.patchFlag > 0 || i & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  l.patchFlag !== 32 && Zt.push(l), l;
}
const zr = Tm;
function Tm(t, e = null, r = null, n = 0, a = null, i = !1) {
  if ((!t || t === Zy) && (t = an), Oh(t)) {
    const o = va(
      t,
      e,
      !0
      /* mergeRef: true */
    );
    return r && gs(o, r), ti > 0 && !i && Zt && (o.shapeFlag & 6 ? Zt[Zt.indexOf(t)] = o : Zt.push(o)), o.patchFlag = -2, o;
  }
  if (_m(t) && (t = t.__vccOpts), e) {
    e = Sm(e);
    let { class: o, style: l } = e;
    o && !at(o) && (e.class = fa(o)), Ge(l) && (/* @__PURE__ */ $u(l) && !ke(l) && (l = Pt({}, l)), e.style = Ou(l));
  }
  const s = at(t) ? 1 : Mh(t) ? 128 : Ny(t) ? 64 : Ge(t) ? 4 : Re(t) ? 2 : 0;
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
function Sm(t) {
  return t ? /* @__PURE__ */ $u(t) || Ch(t) ? Pt({}, t) : t : null;
}
function va(t, e, r = !1, n = !1) {
  const { props: a, ref: i, patchFlag: s, children: o, transition: l } = t, u = e ? Pm(a || {}, e) : a, f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: t.type,
    props: u,
    key: u && _h(u),
    ref: e && e.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      r && i ? ke(i) ? i.concat(ji(e)) : [i, ji(e)] : ji(e)
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
    patchFlag: e && t.type !== Yt ? s === -1 ? 16 : s | 16 : s,
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
    ssContent: t.ssContent && va(t.ssContent),
    ssFallback: t.ssFallback && va(t.ssFallback),
    placeholder: t.placeholder,
    el: t.el,
    anchor: t.anchor,
    ctx: t.ctx,
    ce: t.ce
  };
  return l && n && Uu(
    f,
    l.clone(f)
  ), f;
}
function Mr(t = " ", e = 0) {
  return zr(Us, null, t, e);
}
function Lr(t = "", e = !1) {
  return e ? (et(), Ku(an, null, t)) : zr(an, null, t);
}
function xr(t) {
  return t == null || typeof t == "boolean" ? zr(an) : ke(t) ? zr(
    Yt,
    null,
    // #3666, avoid reference pollution when reusing vnode
    t.slice()
  ) : Oh(t) ? Or(t) : zr(Us, null, String(t));
}
function Or(t) {
  return t.el === null && t.patchFlag !== -1 || t.memo ? t : va(t);
}
function gs(t, e) {
  let r = 0;
  const { shapeFlag: n } = t;
  if (e == null)
    e = null;
  else if (ke(e))
    r = 16;
  else if (typeof e == "object")
    if (n & 65) {
      const a = e.default;
      a && (a._c && (a._d = !1), gs(t, a()), a._c && (a._d = !0));
      return;
    } else {
      r = 32;
      const a = e._;
      !a && !Ch(e) ? e._ctx = Xt : a === 3 && Xt && (Xt.slots._ === 1 ? e._ = 1 : (e._ = 2, t.patchFlag |= 1024));
    }
  else if (Re(e)) {
    if (n & 65) {
      gs(t, { default: e });
      return;
    }
    e = { default: e, _ctx: Xt }, r = 32;
  } else
    e = String(e), n & 64 ? (r = 16, e = [Mr(e)]) : r = 8;
  t.children = e, t.shapeFlag |= r;
}
function Pm(...t) {
  const e = {};
  for (let r = 0; r < t.length; r++) {
    const n = t[r];
    for (const a in n)
      if (a === "class")
        e.class !== n.class && (e.class = fa([e.class, n.class]));
      else if (a === "style")
        e.style = Ou([e.style, n.style]);
      else if (Ls(a)) {
        const i = e[a], s = n[a];
        s && i !== s && !(ke(i) && i.includes(s)) ? e[a] = i ? [].concat(i, s) : s : s == null && i == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !Is(a) && (e[a] = s);
      } else a !== "" && (e[a] = n[a]);
  }
  return e;
}
function hr(t, e, r, n = null) {
  cr(t, e, 7, [
    r,
    n
  ]);
}
const Dm = mh();
let Am = 0;
function km(t, e, r) {
  const n = t.type, a = (e ? e.appContext : t.appContext) || Dm, i = {
    uid: Am++,
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
    scope: new ey(
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
    propsOptions: Sh(n, a),
    emitsOptions: bh(n, a),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: We,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: We,
    data: We,
    props: We,
    attrs: We,
    slots: We,
    refs: We,
    setupState: We,
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
  return i.ctx = { _: i }, i.root = e ? e.root : i, i.emit = om.bind(null, i), t.ce && t.ce(i), i;
}
let Rt = null;
const Bm = () => Rt || Xt;
let ps, uu;
{
  const t = zs(), e = (r, n) => {
    let a;
    return (a = t[r]) || (a = t[r] = []), a.push(n), (i) => {
      a.length > 1 ? a.forEach((s) => s(i)) : a[0](i);
    };
  };
  ps = e(
    "__VUE_INSTANCE_SETTERS__",
    (r) => Rt = r
  ), uu = e(
    "__VUE_SSR_SETTERS__",
    (r) => ri = r
  );
}
const pi = (t) => {
  const e = Rt;
  return ps(t), t.scope.on(), () => {
    t.scope.off(), ps(e);
  };
}, qf = () => {
  Rt && Rt.scope.off(), ps(null);
};
function Nh(t) {
  return t.vnode.shapeFlag & 4;
}
let ri = !1;
function Rm(t, e = !1, r = !1) {
  e && uu(e);
  const { props: n, children: a } = t.vnode, i = Nh(t);
  dm(t, n, i, e), ym(t, a, r || e);
  const s = i ? Mm(t, e) : void 0;
  return e && uu(!1), s;
}
function Mm(t, e) {
  const r = t.type;
  t.accessCache = /* @__PURE__ */ Object.create(null), t.proxy = new Proxy(t.ctx, jy);
  const { setup: n } = r;
  if (n) {
    Vr();
    const a = t.setupContext = n.length > 1 ? Im(t) : null, i = pi(t), s = gi(
      n,
      t,
      0,
      [
        t.props,
        a
      ]
    ), o = Ld(s);
    if (qr(), i(), (o || t.sp) && !Wa(t) && fh(t), o) {
      if (s.then(qf, qf), e)
        return s.then((l) => {
          $f(t, l);
        }).catch((l) => {
          qs(l, t, 0);
        });
      t.asyncDep = s;
    } else
      $f(t, s);
  } else
    Fh(t);
}
function $f(t, e, r) {
  Re(e) ? t.type.__ssrInlineRender ? t.ssrRender = e : t.render = e : Ge(e) && (t.setupState = eh(e)), Fh(t);
}
function Fh(t, e, r) {
  const n = t.type;
  t.render || (t.render = n.render || Tr);
  {
    const a = pi(t);
    Vr();
    try {
      Jy(t);
    } finally {
      qr(), a();
    }
  }
}
const Lm = {
  get(t, e) {
    return Bt(t, "get", ""), t[e];
  }
};
function Im(t) {
  const e = (r) => {
    t.exposed = r || {};
  };
  return {
    attrs: new Proxy(t.attrs, Lm),
    slots: t.slots,
    emit: t.emit,
    expose: e
  };
}
function Gs(t) {
  return t.exposed ? t.exposeProxy || (t.exposeProxy = new Proxy(eh(wy(t.exposed)), {
    get(e, r) {
      if (r in e)
        return e[r];
      if (r in Ka)
        return Ka[r](t);
    },
    has(e, r) {
      return r in e || r in Ka;
    }
  })) : t.proxy;
}
function Om(t, e = !0) {
  return Re(t) ? t.displayName || t.name : t.name || e && t.__name;
}
function _m(t) {
  return Re(t) && "__vccOpts" in t;
}
const Nm = (t, e) => /* @__PURE__ */ Sy(t, e, ri), Fm = "3.5.40";
/**
* @vue/runtime-dom v3.5.40
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let fu;
const Hf = typeof window < "u" && window.trustedTypes;
if (Hf)
  try {
    fu = /* @__PURE__ */ Hf.createPolicy("vue", {
      createHTML: (t) => t
    });
  } catch {
  }
const zh = fu ? (t) => fu.createHTML(t) : (t) => t, zm = "http://www.w3.org/2000/svg", Vm = "http://www.w3.org/1998/Math/MathML", Ir = typeof document < "u" ? document : null, Uf = Ir && /* @__PURE__ */ Ir.createElement("template"), qm = {
  insert: (t, e, r) => {
    e.insertBefore(t, r || null);
  },
  remove: (t) => {
    const e = t.parentNode;
    e && e.removeChild(t);
  },
  createElement: (t, e, r, n) => {
    const a = e === "svg" ? Ir.createElementNS(zm, t) : e === "mathml" ? Ir.createElementNS(Vm, t) : r ? Ir.createElement(t, { is: r }) : Ir.createElement(t);
    return t === "select" && n && n.multiple != null && a.setAttribute("multiple", n.multiple), a;
  },
  createText: (t) => Ir.createTextNode(t),
  createComment: (t) => Ir.createComment(t),
  setText: (t, e) => {
    t.nodeValue = e;
  },
  setElementText: (t, e) => {
    t.textContent = e;
  },
  parentNode: (t) => t.parentNode,
  nextSibling: (t) => t.nextSibling,
  querySelector: (t) => Ir.querySelector(t),
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
      Uf.innerHTML = zh(
        n === "svg" ? `<svg>${t}</svg>` : n === "mathml" ? `<math>${t}</math>` : t
      );
      const o = Uf.content;
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
}, $m = /* @__PURE__ */ Symbol("_vtc");
function Hm(t, e, r) {
  const n = t[$m];
  n && (e = (e ? [e, ...n] : [...n]).join(" ")), e == null ? t.removeAttribute("class") : r ? t.setAttribute("class", e) : t.className = e;
}
const Gf = /* @__PURE__ */ Symbol("_vod"), Um = /* @__PURE__ */ Symbol("_vsh"), Gm = /* @__PURE__ */ Symbol(""), Wm = /(?:^|;)\s*display\s*:/;
function Km(t, e, r) {
  const n = t.style, a = at(r);
  let i = !1;
  if (r && !a) {
    if (e)
      if (at(e))
        for (const s of e.split(";")) {
          const o = s.slice(0, s.indexOf(":")).trim();
          r[o] == null && Fa(n, o, "");
        }
      else
        for (const s in e)
          r[s] == null && Fa(n, s, "");
    for (const s in r) {
      s === "display" && (i = !0);
      const o = r[s];
      o != null ? Xm(
        t,
        s,
        !at(e) && e ? e[s] : void 0,
        o
      ) || Fa(n, s, o) : Fa(n, s, "");
    }
  } else if (a) {
    if (e !== r) {
      const s = n[Gm];
      s && (r += ";" + s), n.cssText = r, i = Wm.test(r);
    }
  } else e && t.removeAttribute("style");
  Gf in t && (t[Gf] = i ? n.display : "", t[Um] && (n.display = "none"));
}
const Wf = /\s*!important$/;
function Fa(t, e, r) {
  if (ke(r))
    r.forEach((n) => Fa(t, e, n));
  else if (r == null && (r = ""), e.startsWith("--"))
    t.setProperty(e, r);
  else {
    const n = Ym(t, e);
    Wf.test(r) ? t.setProperty(
      Fn(n),
      r.replace(Wf, ""),
      "important"
    ) : t[n] = r;
  }
}
const Kf = ["Webkit", "Moz", "ms"], Eo = {};
function Ym(t, e) {
  const r = Eo[e];
  if (r)
    return r;
  let n = Vt(e);
  if (n !== "filter" && n in t)
    return Eo[e] = n;
  n = Ns(n);
  for (let a = 0; a < Kf.length; a++) {
    const i = Kf[a] + n;
    if (i in t)
      return Eo[e] = i;
  }
  return e;
}
function Xm(t, e, r, n) {
  return t.tagName === "TEXTAREA" && (e === "width" || e === "height") && at(n) && r === n;
}
const Yf = "http://www.w3.org/1999/xlink";
function Xf(t, e, r, n, a, i = Qp(e)) {
  n && e.startsWith("xlink:") ? r == null ? t.removeAttributeNS(Yf, e.slice(6, e.length)) : t.setAttributeNS(Yf, e, r) : r == null || i && !Nd(r) ? t.removeAttribute(e) : t.setAttribute(
    e,
    i ? "" : Sr(r) ? String(r) : r
  );
}
function Zf(t, e, r, n, a) {
  if (e === "innerHTML" || e === "textContent") {
    r != null && (t[e] = e === "innerHTML" ? zh(r) : r);
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
    o === "boolean" ? r = Nd(r) : r == null && o === "string" ? (r = "", s = !0) : o === "number" && (r = 0, s = !0);
  }
  try {
    t[e] = r;
  } catch {
  }
  s && t.removeAttribute(a || e);
}
function Sn(t, e, r, n) {
  t.addEventListener(e, r, n);
}
function Zm(t, e, r, n) {
  t.removeEventListener(e, r, n);
}
const Qf = /* @__PURE__ */ Symbol("_vei");
function Qm(t, e, r, n, a = null) {
  const i = t[Qf] || (t[Qf] = {}), s = i[e];
  if (n && s)
    s.value = n;
  else {
    const [o, l] = e0(e);
    if (n) {
      const u = i[e] = n0(
        n,
        a
      );
      Sn(t, o, u, l);
    } else s && (Zm(t, o, s, l), i[e] = void 0);
  }
}
const jm = /(Once|Passive|Capture)$/, Jm = /^on:?(?:Once|Passive|Capture)$/;
function e0(t) {
  let e, r;
  for (; (r = t.match(jm)) && !Jm.test(t); )
    e || (e = {}), t = t.slice(0, t.length - r[1].length), e[r[1].toLowerCase()] = !0;
  return [t[2] === ":" ? t.slice(3) : Fn(t.slice(2)), e];
}
let Co = 0;
const t0 = /* @__PURE__ */ Promise.resolve(), r0 = () => Co || (t0.then(() => Co = 0), Co = Date.now());
function n0(t, e) {
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
        u && cr(
          u,
          e,
          5,
          o
        );
      }
    } else
      cr(
        a,
        e,
        5,
        [n]
      );
  };
  return r.value = t, r.attached = r0(), r;
}
const jf = (t) => t.charCodeAt(0) === 111 && t.charCodeAt(1) === 110 && // lowercase letter
t.charCodeAt(2) > 96 && t.charCodeAt(2) < 123, a0 = (t, e, r, n, a, i) => {
  const s = a === "svg";
  e === "class" ? Hm(t, n, s) : e === "style" ? Km(t, r, n) : Ls(e) ? Is(e) || Qm(t, e, r, n, i) : (e[0] === "." ? (e = e.slice(1), !0) : e[0] === "^" ? (e = e.slice(1), !1) : i0(t, e, n, s)) ? (Zf(t, e, n), !t.tagName.includes("-") && (e === "value" || e === "checked" || e === "selected") && Xf(t, e, n, s, i, e !== "value")) : /* #11081 force set props for possible async custom element */ t._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (s0(t, e) || // @ts-expect-error _def is private
  t._def.__asyncLoader && (/[A-Z]/.test(e) || !at(n))) ? Zf(t, Vt(e), n, i, e) : (e === "true-value" ? t._trueValue = n : e === "false-value" && (t._falseValue = n), Xf(t, e, n, s));
};
function i0(t, e, r, n) {
  if (n)
    return !!(e === "innerHTML" || e === "textContent" || e in t && jf(e) && Re(r));
  if (e === "spellcheck" || e === "draggable" || e === "translate" || e === "autocorrect" || e === "sandbox" && t.tagName === "IFRAME" || e === "form" || e === "list" && t.tagName === "INPUT" || e === "type" && t.tagName === "TEXTAREA")
    return !1;
  if (e === "width" || e === "height") {
    const a = t.tagName;
    if (a === "IMG" || a === "VIDEO" || a === "CANVAS" || a === "SOURCE")
      return !1;
  }
  return jf(e) && at(r) ? !1 : e in t;
}
function s0(t, e) {
  const r = (
    // @ts-expect-error _def is private
    t._def.props
  );
  if (!r)
    return !1;
  const n = Vt(e);
  return Array.isArray(r) ? r.some((a) => Vt(a) === n) : Object.keys(r).some((a) => Vt(a) === n);
}
const ys = (t) => {
  const e = t.props["onUpdate:modelValue"] || !1;
  return ke(e) ? (r) => Yi(e, r) : e;
};
function o0(t) {
  t.target.composing = !0;
}
function Jf(t) {
  const e = t.target;
  e.composing && (e.composing = !1, e.dispatchEvent(new Event("input")));
}
const ia = /* @__PURE__ */ Symbol("_assign");
function ec(t, e, r) {
  return e && (t = t.trim()), r && (t = Fs(t)), t;
}
const Ii = {
  created(t, { modifiers: { lazy: e, trim: r, number: n } }, a) {
    t[ia] = ys(a);
    const i = n || a.props && a.props.type === "number";
    Sn(t, e ? "change" : "input", (s) => {
      s.target.composing || t[ia](ec(t.value, r, i));
    }), (r || i) && Sn(t, "change", () => {
      t.value = ec(t.value, r, i);
    }), e || (Sn(t, "compositionstart", o0), Sn(t, "compositionend", Jf), Sn(t, "change", Jf));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(t, { value: e }) {
    t.value = e ?? "";
  },
  beforeUpdate(t, { value: e, oldValue: r, modifiers: { lazy: n, trim: a, number: i } }, s) {
    if (t[ia] = ys(s), t.composing) return;
    const o = (i || t.type === "number") && !/^0\d/.test(t.value) ? Fs(t.value) : t.value, l = e ?? "";
    if (o === l)
      return;
    const u = t.getRootNode();
    (u instanceof Document || u instanceof ShadowRoot) && u.activeElement === t && t.type !== "range" && (n && e === r || a && t.value.trim() === l) || (t.value = l);
  }
}, l0 = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(t, { value: e, modifiers: { number: r } }, n) {
    t._modelValue = e, Sn(t, "change", () => {
      const a = Array.prototype.filter.call(t.options, (i) => i.selected).map(
        (i) => r ? Fs(ms(i)) : ms(i)
      );
      t[ia](
        t.multiple ? Os(t._modelValue) ? new Set(a) : a : a[0]
      ), t._assigning = !0, rh(() => {
        t._assigning = !1;
      });
    }), t[ia] = ys(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(t, { value: e }) {
    tc(t, e);
  },
  beforeUpdate(t, { value: e }, r) {
    t._modelValue = e, t[ia] = ys(r);
  },
  updated(t, { value: e }) {
    t._assigning || tc(t, e);
  }
};
function tc(t, e) {
  const r = t.multiple, n = ke(e);
  if (!(r && !n && !Os(e))) {
    for (let a = 0, i = t.options.length; a < i; a++) {
      const s = t.options[a], o = ms(s);
      if (r)
        if (n) {
          const l = typeof o;
          l === "string" || l === "number" ? s.selected = e.some((u) => String(u) === String(o)) : s.selected = Jp(e, o) > -1;
        } else
          s.selected = e.has(o);
      else if (hi(ms(s), e)) {
        t.selectedIndex !== a && (t.selectedIndex = a);
        return;
      }
    }
    !r && t.selectedIndex !== -1 && (t.selectedIndex = -1);
  }
}
function ms(t) {
  return "_value" in t ? t._value : t.value;
}
const u0 = ["ctrl", "shift", "alt", "meta"], f0 = {
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
  exact: (t, e) => u0.some((r) => t[`${r}Key`] && !e.includes(r))
}, c0 = (t, e) => {
  if (!t) return t;
  const r = t._withMods || (t._withMods = {}), n = e.join(".");
  return r[n] || (r[n] = ((a, ...i) => {
    for (let s = 0; s < e.length; s++) {
      const o = f0[e[s]];
      if (o && o(a, e)) return;
    }
    return t(a, ...i);
  }));
}, v0 = /* @__PURE__ */ Pt({ patchProp: a0 }, qm);
let rc;
function d0() {
  return rc || (rc = bm(v0));
}
const h0 = ((...t) => {
  const e = d0().createApp(...t), { mount: r } = e;
  return e.mount = (n) => {
    const a = p0(n);
    if (!a) return;
    const i = e._component;
    !Re(i) && !i.render && !i.template && (i.template = a.innerHTML), a.nodeType === 1 && (a.textContent = "");
    const s = r(a, !1, g0(a));
    return a instanceof Element && (a.removeAttribute("v-cloak"), a.setAttribute("data-v-app", "")), s;
  }, e;
});
function g0(t) {
  if (t instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && t instanceof MathMLElement)
    return "mathml";
}
function p0(t) {
  return at(t) ? document.querySelector(t) : t;
}
let cu = null;
function y0(t) {
  cu = t ?? null;
}
function lt(t, e) {
  return cu ? cu(t, e) : t;
}
function vu(t, e) {
  (e == null || e > t.length) && (e = t.length);
  for (var r = 0, n = Array(e); r < e; r++) n[r] = t[r];
  return n;
}
function m0(t) {
  if (Array.isArray(t)) return t;
}
function b0(t) {
  if (Array.isArray(t)) return vu(t);
}
function vn(t, e) {
  if (!(t instanceof e)) throw new TypeError("Cannot call a class as a function");
}
function w0(t, e) {
  for (var r = 0; r < e.length; r++) {
    var n = e[r];
    n.enumerable = n.enumerable || !1, n.configurable = !0, "value" in n && (n.writable = !0), Object.defineProperty(t, qh(n.key), n);
  }
}
function dn(t, e, r) {
  return e && w0(t.prototype, e), Object.defineProperty(t, "prototype", {
    writable: !1
  }), t;
}
function Kt(t, e) {
  var r = typeof Symbol < "u" && t[Symbol.iterator] || t["@@iterator"];
  if (!r) {
    if (Array.isArray(t) || (r = Yu(t)) || e) {
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
function Vh(t, e, r) {
  return (e = qh(e)) in t ? Object.defineProperty(t, e, {
    value: r,
    enumerable: !0,
    configurable: !0,
    writable: !0
  }) : t[e] = r, t;
}
function x0(t) {
  if (typeof Symbol < "u" && t[Symbol.iterator] != null || t["@@iterator"] != null) return Array.from(t);
}
function E0(t, e) {
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
function C0() {
  throw new TypeError(`Invalid attempt to destructure non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`);
}
function T0() {
  throw new TypeError(`Invalid attempt to spread non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`);
}
function ct(t, e) {
  return m0(t) || E0(t, e) || Yu(t, e) || C0();
}
function bs(t) {
  return b0(t) || x0(t) || Yu(t) || T0();
}
function S0(t, e) {
  if (typeof t != "object" || !t) return t;
  var r = t[Symbol.toPrimitive];
  if (r !== void 0) {
    var n = r.call(t, e);
    if (typeof n != "object") return n;
    throw new TypeError("@@toPrimitive must return a primitive value.");
  }
  return String(t);
}
function qh(t) {
  var e = S0(t, "string");
  return typeof e == "symbol" ? e : e + "";
}
function gt(t) {
  "@babel/helpers - typeof";
  return gt = typeof Symbol == "function" && typeof Symbol.iterator == "symbol" ? function(e) {
    return typeof e;
  } : function(e) {
    return e && typeof Symbol == "function" && e.constructor === Symbol && e !== Symbol.prototype ? "symbol" : typeof e;
  }, gt(t);
}
function Yu(t, e) {
  if (t) {
    if (typeof t == "string") return vu(t, e);
    var r = {}.toString.call(t).slice(8, -1);
    return r === "Object" && t.constructor && (r = t.constructor.name), r === "Map" || r === "Set" ? Array.from(t) : r === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(r) ? vu(t, e) : void 0;
  }
}
var dt = typeof window > "u" ? null : window, nc = dt ? dt.navigator : null;
dt && dt.document;
var P0 = gt(""), $h = gt({}), D0 = gt(function() {
}), A0 = typeof HTMLElement > "u" ? "undefined" : gt(HTMLElement), yi = function(e) {
  return e && e.instanceString && tt(e.instanceString) ? e.instanceString() : null;
}, Se = function(e) {
  return e != null && gt(e) == P0;
}, tt = function(e) {
  return e != null && gt(e) === D0;
}, Ke = function(e) {
  return !Qt(e) && (Array.isArray ? Array.isArray(e) : e != null && e instanceof Array);
}, Fe = function(e) {
  return e != null && gt(e) === $h && !Ke(e) && e.constructor === Object;
}, k0 = function(e) {
  return e != null && gt(e) === $h;
}, pe = function(e) {
  return e != null && gt(e) === gt(1) && !isNaN(e);
}, B0 = function(e) {
  return pe(e) && Math.floor(e) === e;
}, ws = function(e) {
  if (A0 !== "undefined")
    return e != null && e instanceof HTMLElement;
}, Qt = function(e) {
  return mi(e) || Hh(e);
}, mi = function(e) {
  return yi(e) === "collection" && e._private.single;
}, Hh = function(e) {
  return yi(e) === "collection" && !e._private.single;
}, Xu = function(e) {
  return yi(e) === "core";
}, Uh = function(e) {
  return yi(e) === "stylesheet";
}, R0 = function(e) {
  return yi(e) === "event";
}, sn = function(e) {
  return e == null ? !0 : !!(e === "" || e.match(/^\s+$/));
}, M0 = function(e) {
  return typeof HTMLElement > "u" ? !1 : e instanceof HTMLElement;
}, L0 = function(e) {
  return Fe(e) && pe(e.x1) && pe(e.x2) && pe(e.y1) && pe(e.y2);
}, I0 = function(e) {
  return k0(e) && tt(e.then);
}, O0 = function() {
  return nc && nc.userAgent.match(/msie|trident|edge/i);
}, da = function(e, r) {
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
}, Zu = da(function(t) {
  return t.replace(/([A-Z])/g, function(e) {
    return "-" + e.toLowerCase();
  });
}), Ws = da(function(t) {
  return t.replace(/(-\w)/g, function(e) {
    return e[1].toUpperCase();
  });
}), Gh = da(function(t, e) {
  return t + e[0].toUpperCase() + e.substring(1);
}, function(t, e) {
  return t + "$" + e;
}), ac = function(e) {
  return sn(e) ? e : e.charAt(0).toUpperCase() + e.substring(1);
}, Jr = function(e, r) {
  return e.slice(-1 * r.length) === r;
}, ht = "(?:[-+]?(?:(?:\\d+|\\d*\\.\\d+)(?:[Ee][+-]?\\d+)?))", _0 = "rgb[a]?\\((" + ht + "[%]?)\\s*,\\s*(" + ht + "[%]?)\\s*,\\s*(" + ht + "[%]?)(?:\\s*,\\s*(" + ht + "))?\\)", N0 = "rgb[a]?\\((?:" + ht + "[%]?)\\s*,\\s*(?:" + ht + "[%]?)\\s*,\\s*(?:" + ht + "[%]?)(?:\\s*,\\s*(?:" + ht + "))?\\)", F0 = "hsl[a]?\\((" + ht + ")\\s*,\\s*(" + ht + "[%])\\s*,\\s*(" + ht + "[%])(?:\\s*,\\s*(" + ht + "))?\\)", z0 = "hsl[a]?\\((?:" + ht + ")\\s*,\\s*(?:" + ht + "[%])\\s*,\\s*(?:" + ht + "[%])(?:\\s*,\\s*(?:" + ht + "))?\\)", V0 = "\\#[0-9a-fA-F]{3}", q0 = "\\#[0-9a-fA-F]{6}", Wh = function(e, r) {
  return e < r ? -1 : e > r ? 1 : 0;
}, $0 = function(e, r) {
  return -1 * Wh(e, r);
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
}, H0 = function(e) {
  if (!(!(e.length === 4 || e.length === 7) || e[0] !== "#")) {
    var r = e.length === 4, n, a, i, s = 16;
    return r ? (n = parseInt(e[1] + e[1], s), a = parseInt(e[2] + e[2], s), i = parseInt(e[3] + e[3], s)) : (n = parseInt(e[1] + e[2], s), a = parseInt(e[3] + e[4], s), i = parseInt(e[5] + e[6], s)), [n, a, i];
  }
}, U0 = function(e) {
  var r, n, a, i, s, o, l, u;
  function f(h, y, g) {
    return g < 0 && (g += 1), g > 1 && (g -= 1), g < 1 / 6 ? h + (y - h) * 6 * g : g < 1 / 2 ? y : g < 2 / 3 ? h + (y - h) * (2 / 3 - g) * 6 : h;
  }
  var c = new RegExp("^" + F0 + "$").exec(e);
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
}, G0 = function(e) {
  var r, n = new RegExp("^" + _0 + "$").exec(e);
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
}, W0 = function(e) {
  return K0[e.toLowerCase()];
}, Kh = function(e) {
  return (Ke(e) ? e : null) || W0(e) || H0(e) || G0(e) || U0(e);
}, K0 = {
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
}, Yh = function(e) {
  for (var r = e.map, n = e.keys, a = n.length, i = 0; i < a; i++) {
    var s = n[i];
    if (Fe(s))
      throw Error("Tried to set map with object key");
    i < n.length - 1 ? (r[s] == null && (r[s] = {}), r = r[s]) : r[s] = e.value;
  }
}, Xh = function(e) {
  for (var r = e.map, n = e.keys, a = n.length, i = 0; i < a; i++) {
    var s = n[i];
    if (Fe(s))
      throw Error("Tried to get map with object key");
    if (r = r[s], r == null)
      return r;
  }
  return r;
}, Oi = typeof globalThis < "u" ? globalThis : typeof window < "u" ? window : typeof global < "u" ? global : typeof self < "u" ? self : {};
function bi(t) {
  return t && t.__esModule && Object.prototype.hasOwnProperty.call(t, "default") ? t.default : t;
}
var To, ic;
function wi() {
  if (ic) return To;
  ic = 1;
  function t(e) {
    var r = typeof e;
    return e != null && (r == "object" || r == "function");
  }
  return To = t, To;
}
var So, sc;
function Y0() {
  if (sc) return So;
  sc = 1;
  var t = typeof Oi == "object" && Oi && Oi.Object === Object && Oi;
  return So = t, So;
}
var Po, oc;
function Ks() {
  if (oc) return Po;
  oc = 1;
  var t = Y0(), e = typeof self == "object" && self && self.Object === Object && self, r = t || e || Function("return this")();
  return Po = r, Po;
}
var Do, lc;
function X0() {
  if (lc) return Do;
  lc = 1;
  var t = Ks(), e = function() {
    return t.Date.now();
  };
  return Do = e, Do;
}
var Ao, uc;
function Z0() {
  if (uc) return Ao;
  uc = 1;
  var t = /\s/;
  function e(r) {
    for (var n = r.length; n-- && t.test(r.charAt(n)); )
      ;
    return n;
  }
  return Ao = e, Ao;
}
var ko, fc;
function Q0() {
  if (fc) return ko;
  fc = 1;
  var t = Z0(), e = /^\s+/;
  function r(n) {
    return n && n.slice(0, t(n) + 1).replace(e, "");
  }
  return ko = r, ko;
}
var Bo, cc;
function Qu() {
  if (cc) return Bo;
  cc = 1;
  var t = Ks(), e = t.Symbol;
  return Bo = e, Bo;
}
var Ro, vc;
function j0() {
  if (vc) return Ro;
  vc = 1;
  var t = Qu(), e = Object.prototype, r = e.hasOwnProperty, n = e.toString, a = t ? t.toStringTag : void 0;
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
  return Ro = i, Ro;
}
var Mo, dc;
function J0() {
  if (dc) return Mo;
  dc = 1;
  var t = Object.prototype, e = t.toString;
  function r(n) {
    return e.call(n);
  }
  return Mo = r, Mo;
}
var Lo, hc;
function Zh() {
  if (hc) return Lo;
  hc = 1;
  var t = Qu(), e = j0(), r = J0(), n = "[object Null]", a = "[object Undefined]", i = t ? t.toStringTag : void 0;
  function s(o) {
    return o == null ? o === void 0 ? a : n : i && i in Object(o) ? e(o) : r(o);
  }
  return Lo = s, Lo;
}
var Io, gc;
function eb() {
  if (gc) return Io;
  gc = 1;
  function t(e) {
    return e != null && typeof e == "object";
  }
  return Io = t, Io;
}
var Oo, pc;
function xi() {
  if (pc) return Oo;
  pc = 1;
  var t = Zh(), e = eb(), r = "[object Symbol]";
  function n(a) {
    return typeof a == "symbol" || e(a) && t(a) == r;
  }
  return Oo = n, Oo;
}
var _o, yc;
function tb() {
  if (yc) return _o;
  yc = 1;
  var t = Q0(), e = wi(), r = xi(), n = NaN, a = /^[-+]0x[0-9a-f]+$/i, i = /^0b[01]+$/i, s = /^0o[0-7]+$/i, o = parseInt;
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
  return _o = l, _o;
}
var No, mc;
function rb() {
  if (mc) return No;
  mc = 1;
  var t = wi(), e = X0(), r = tb(), n = "Expected a function", a = Math.max, i = Math.min;
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
      var I = M - y, _ = M - g, O = l - I;
      return m ? i(O, v - _) : O;
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
  return No = s, No;
}
var nb = rb(), Ei = /* @__PURE__ */ bi(nb), Fo = dt ? dt.performance : null, Qh = Fo && Fo.now ? function() {
  return Fo.now();
} : function() {
  return Date.now();
}, ab = (function() {
  if (dt) {
    if (dt.requestAnimationFrame)
      return function(t) {
        dt.requestAnimationFrame(t);
      };
    if (dt.mozRequestAnimationFrame)
      return function(t) {
        dt.mozRequestAnimationFrame(t);
      };
    if (dt.webkitRequestAnimationFrame)
      return function(t) {
        dt.webkitRequestAnimationFrame(t);
      };
    if (dt.msRequestAnimationFrame)
      return function(t) {
        dt.msRequestAnimationFrame(t);
      };
  }
  return function(t) {
    t && setTimeout(function() {
      t(Qh());
    }, 1e3 / 60);
  };
})(), xs = function(e) {
  return ab(e);
}, Hr = Qh, Pn = 9261, jh = 65599, Qn = 5381, Jh = function(e) {
  for (var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Pn, n = r, a; a = e.next(), !a.done; )
    n = n * jh + a.value | 0;
  return n;
}, ni = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Pn;
  return r * jh + e | 0;
}, ai = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Qn;
  return (r << 5) + r + e | 0;
}, ib = function(e, r) {
  return e * 2097152 + r;
}, Xr = function(e) {
  return e[0] * 2097152 + e[1];
}, _i = function(e, r) {
  return [ni(e[0], r[0]), ai(e[1], r[1])];
}, bc = function(e, r) {
  var n = {
    value: 0,
    done: !1
  }, a = 0, i = e.length, s = {
    next: function() {
      return a < i ? n.value = e[a++] : n.done = !0, n;
    }
  };
  return Jh(s, r);
}, Mn = function(e, r) {
  var n = {
    value: 0,
    done: !1
  }, a = 0, i = e.length, s = {
    next: function() {
      return a < i ? n.value = e.charCodeAt(a++) : n.done = !0, n;
    }
  };
  return Jh(s, r);
}, eg = function() {
  return sb(arguments);
}, sb = function(e) {
  for (var r, n = 0; n < e.length; n++) {
    var a = e[n];
    n === 0 ? r = Mn(a) : r = Mn(a, r);
  }
  return r;
};
function ob(t, e, r, n, a) {
  var i = a * Math.PI / 180, s = Math.cos(i) * (t - r) - Math.sin(i) * (e - n) + r, o = Math.sin(i) * (t - r) + Math.cos(i) * (e - n) + n;
  return {
    x: s,
    y: o
  };
}
var lb = function(e, r, n, a, i, s) {
  return {
    x: (e - n) * i + n,
    y: (r - a) * s + a
  };
};
function ub(t, e, r) {
  if (r === 0) return t;
  var n = (e.x1 + e.x2) / 2, a = (e.y1 + e.y2) / 2, i = e.w / e.h, s = 1 / i, o = ob(t.x, t.y, n, a, r), l = lb(o.x, o.y, n, a, i, s);
  return {
    x: l.x,
    y: l.y
  };
}
var wc = !0, fb = console.warn != null, cb = console.trace != null, ju = Number.MAX_SAFE_INTEGER || 9007199254740991, tg = function() {
  return !0;
}, Es = function() {
  return !1;
}, xc = function() {
  return 0;
}, Ju = function() {
}, je = function(e) {
  throw new Error(e);
}, rg = function(e) {
  if (e !== void 0)
    wc = !!e;
  else
    return wc;
}, He = function(e) {
  rg() && (fb ? console.warn(e) : (console.log(e), cb && console.trace()));
}, vb = function(e) {
  return Ae({}, e);
}, Cr = function(e) {
  return e == null ? e : Ke(e) ? e.slice() : Fe(e) ? vb(e) : e;
}, db = function(e) {
  return e.slice();
}, ng = function(e, r) {
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
}, hb = {}, ag = function() {
  return hb;
}, Dt = function(e) {
  var r = Object.keys(e);
  return function(n) {
    for (var a = {}, i = 0; i < r.length; i++) {
      var s = r[i], o = n == null ? void 0 : n[s];
      a[s] = o === void 0 ? e[s] : o;
    }
    return a;
  };
}, on = function(e, r, n) {
  for (var a = e.length - 1; a >= 0; a--)
    e[a] === r && e.splice(a, 1);
}, ef = function(e) {
  e.splice(0, e.length);
}, gb = function(e, r) {
  for (var n = 0; n < r.length; n++) {
    var a = r[n];
    e.push(a);
  }
}, Ft = function(e, r, n) {
  return n && (r = Gh(n, r)), e[r];
}, br = function(e, r, n, a) {
  n && (r = Gh(n, r)), e[r] = a;
}, pb = /* @__PURE__ */ (function() {
  function t() {
    vn(this, t), this._obj = {};
  }
  return dn(t, [{
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
})(), Fr = typeof Map < "u" ? Map : pb, yb = "undefined", mb = /* @__PURE__ */ (function() {
  function t(e) {
    if (vn(this, t), this._obj = /* @__PURE__ */ Object.create(null), this.size = 0, e != null) {
      var r;
      e.instanceString != null && e.instanceString() === this.instanceString() ? r = e.toArray() : r = e;
      for (var n = 0; n < r.length; n++)
        this.add(r[n]);
    }
  }
  return dn(t, [{
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
})(), ma = (typeof Set > "u" ? "undefined" : gt(Set)) !== yb ? Set : mb, Ys = function(e, r) {
  var n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : !0;
  if (e === void 0 || r === void 0 || !Xu(e)) {
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
    classes: new ma(),
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
  Ke(r.classes) ? u = r.classes : Se(r.classes) && (u = r.classes.split(/\s+/));
  for (var f = 0, c = u.length; f < c; f++) {
    var v = u[f];
    !v || v === "" || i.classes.add(v);
  }
  this.createEmitter(), (n === void 0 || n) && this.restore();
  var d = r.style || r.css;
  d && (He("Setting a `style` bypass at element creation should be done only when absolutely necessary.  Try to use the stylesheet instead."), this.style(d));
}, Ec = function(e) {
  return e = {
    bfs: e.bfs || !e.dfs,
    dfs: e.dfs || !e.bfs
  }, function(n, a, i) {
    var s;
    Fe(n) && !Qt(n) && (s = n, n = s.roots || s.root, a = s.visit, i = s.directed), i = arguments.length === 2 && !tt(a) ? a : i, a = tt(a) ? a : function() {
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
      var _ = v[I], O = c[I], L = O != null ? O.source() : null, N = O != null ? O.target() : null, H = O == null ? void 0 : M.same(L) ? N[0] : L[0], V;
      if (V = a(M, O, H, h++, _), V === !0)
        return y = M, 1;
      if (V === !1)
        return 1;
      for (var F = M.connectedEdges().filter(function(le) {
        return (!i || le.source().same(M)) && m.has(le);
      }), $ = 0; $ < F.length; $++) {
        var Q = F[$], se = Q.connectedNodes().filter(function(le) {
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
}, ii = {
  breadthFirstSearch: Ec({
    bfs: !0
  }),
  depthFirstSearch: Ec({
    dfs: !0
  })
};
ii.bfs = ii.breadthFirstSearch;
ii.dfs = ii.depthFirstSearch;
var Ji = { exports: {} }, bb = Ji.exports, Cc;
function wb() {
  return Cc || (Cc = 1, (function(t, e) {
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
    }).call(bb);
  })(Ji)), Ji.exports;
}
var zo, Tc;
function xb() {
  return Tc || (Tc = 1, zo = wb()), zo;
}
var Eb = xb(), Ci = /* @__PURE__ */ bi(Eb), Cb = Dt({
  root: null,
  weight: function(e) {
    return 1;
  },
  directed: !1
}), Tb = {
  dijkstra: function(e) {
    if (!Fe(e)) {
      var r = arguments;
      e = {
        root: r[0],
        weight: r[1],
        directed: r[2]
      };
    }
    var n = Cb(e), a = n.root, i = n.weight, s = n.directed, o = this, l = i, u = Se(a) ? this.filter(a)[0] : a[0], f = {}, c = {}, v = {}, d = this.byGroup(), h = d.nodes, y = d.edges;
    y.unmergeBy(function(_) {
      return _.isLoop();
    });
    for (var g = function(O) {
      return f[O.id()];
    }, p = function(O, L) {
      f[O.id()] = L, m.updateItem(O);
    }, m = new Ci(function(_, O) {
      return g(_) - g(O);
    }), b = 0; b < h.length; b++) {
      var w = h[b];
      f[w.id()] = w.same(u) ? 0 : 1 / 0, m.push(w);
    }
    for (var E = function(O, L) {
      for (var N = (s ? O.edgesTo(L) : O.edgesWith(L)).intersect(y), H = 1 / 0, V, F = 0; F < N.length; F++) {
        var $ = N[F], Q = l($);
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
      distanceTo: function(O) {
        var L = Se(O) ? h.filter(O)[0] : O[0];
        return v[L.id()];
      },
      pathTo: function(O) {
        var L = Se(O) ? h.filter(O)[0] : O[0], N = [], H = L, V = H.id();
        if (L.length > 0)
          for (N.unshift(L); c[V]; ) {
            var F = c[V];
            N.unshift(F.edge), N.unshift(F.node), H = F.node, V = H.id();
          }
        return o.spawn(N);
      }
    };
  }
}, Sb = {
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
}, Pb = Dt({
  root: null,
  goal: null,
  weight: function(e) {
    return 1;
  },
  heuristic: function(e) {
    return 0;
  },
  directed: !1
}), Db = {
  // Implemented from pseudocode from wikipedia
  aStar: function(e) {
    var r = this.cy(), n = Pb(e), a = n.root, i = n.goal, s = n.heuristic, o = n.directed, l = n.weight;
    a = r.collection(a)[0], i = r.collection(i)[0];
    var u = a.id(), f = i.id(), c = {}, v = {}, d = {}, h = new Ci(function(V, F) {
      return v[V.id()] - v[F.id()];
    }), y = new ma(), g = {}, p = {}, m = function(F, $) {
      h.push(F), y.add($);
    }, b, w, E = function() {
      b = h.pop(), w = b.id(), y.delete(w);
    }, T = function(F) {
      return y.has(F);
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
          var _ = I.source(), O = I.target(), L = _.id() !== w ? _ : O, N = L.id();
          if (this.hasElementWithId(N) && !d[N]) {
            var H = c[w] + l(I);
            if (!T(N)) {
              c[N] = H, v[N] = H + s(L), m(L, N), g[N] = b, p[N] = I;
              continue;
            }
            H < c[N] && (c[N] = H, v[N] = H + s(L), g[N] = b, p[N] = I);
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
}, Ab = Dt({
  weight: function(e) {
    return 1;
  },
  directed: !1
}), kb = {
  // Implemented from pseudocode from wikipedia
  floydWarshall: function(e) {
    for (var r = this.cy(), n = Ab(e), a = n.weight, i = n.directed, s = a, o = this.byGroup(), l = o.nodes, u = o.edges, f = l.length, c = f * f, v = function(Q) {
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
        for (var _ = I * f + M, O = 0; O < f; O++) {
          var L = I * f + O, N = M * f + O;
          h[_] + h[N] < h[L] && (h[L] = h[_] + h[N], m[L] = m[_]);
        }
    var H = function(Q) {
      return (Se(Q) ? r.filter(Q) : Q)[0];
    }, V = function(Q) {
      return v(H(Q));
    }, F = {
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
    return F;
  }
  // floydWarshall
}, Bb = Dt({
  weight: function(e) {
    return 1;
  },
  directed: !1,
  root: null
}), Rb = {
  // Implemented from pseudocode from wikipedia
  bellmanFord: function(e) {
    var r = this, n = Bb(e), a = n.weight, i = n.directed, s = n.root, o = a, l = this, u = this.cy(), f = this.byGroup(), c = f.edges, v = f.nodes, d = v.length, h = new Fr(), y = !1, g = [];
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
        var M = c[R], I = M.source(), _ = M.target(), O = o(M), L = m(I), N = m(_);
        A(I, _, M, L, N, O), i || A(_, I, M, N, L, O);
      }
      if (!D)
        break;
    }
    if (D)
      for (var H = [], V = 0; V < p; V++) {
        var F = c[V], $ = F.source(), Q = F.target(), se = o(F), ae = m($).dist, le = m(Q).dist;
        if (ae + se < le || !i && le + se < ae)
          if (y || (He("Graph contains a negative weight cycle for Bellman-Ford"), y = !0), e.findNegativeWeightCycles !== !1) {
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
}, Mb = Math.sqrt(2), Lb = function(e, r, n) {
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
}, Vo = function(e, r, n, a) {
  for (; n > a; ) {
    var i = Math.floor(Math.random() * r.length);
    r = Lb(i, e, r), n--;
  }
  return r;
}, Ib = {
  // Computes the minimum cut of an undirected graph
  // Returns the correct answer with high probability
  kargerStein: function() {
    var e = this, r = this.byGroup(), n = r.nodes, a = r.edges;
    a.unmergeBy(function(N) {
      return N.isLoop();
    });
    var i = n.length, s = a.length, o = Math.ceil(Math.pow(Math.log(i) / Math.LN2, 2)), l = Math.floor(i / Mb);
    if (i < 2) {
      je("At least 2 nodes are required for Karger-Stein algorithm");
      return;
    }
    for (var u = [], f = 0; f < s; f++) {
      var c = a[f];
      u.push([f, n.indexOf(c.source()), n.indexOf(c.target())]);
    }
    for (var v = 1 / 0, d = [], h = new Array(i), y = new Array(i), g = new Array(i), p = function(H, V) {
      for (var F = 0; F < i; F++)
        V[F] = H[F];
    }, m = 0; m <= o; m++) {
      for (var b = 0; b < i; b++)
        y[b] = b;
      var w = Vo(y, u.slice(), i, l), E = w.slice();
      p(y, g);
      var T = Vo(y, w, l, 2), x = Vo(g, E, l, 2);
      T.length <= x.length && T.length < v ? (v = T.length, d = T, p(y, h)) : x.length <= T.length && x.length < v && (v = x.length, d = x, p(g, h));
    }
    for (var S = this.spawn(d.map(function(N) {
      return a[N[0]];
    })), D = this.spawn(), A = this.spawn(), k = h[0], R = 0; R < h.length; R++) {
      var M = h[R], I = n[R];
      M === k ? D.merge(I) : A.merge(I);
    }
    var _ = function(H) {
      var V = e.spawn();
      return H.forEach(function(F) {
        V.merge(F), F.connectedEdges().forEach(function($) {
          e.contains($) && !S.contains($) && V.merge($);
        });
      }), V;
    }, O = [_(D), _(A)], L = {
      cut: S,
      components: O,
      // n.b. partitions are included to be compatible with the old api spec
      // (could be removed in a future major version)
      partition1: D,
      partition2: A
    };
    return L;
  }
}, qo, Ob = function(e) {
  return {
    x: e.x,
    y: e.y
  };
}, Xs = function(e, r, n) {
  return {
    x: e.x * r + n.x,
    y: e.y * r + n.y
  };
}, ig = function(e, r, n) {
  return {
    x: (e.x - n.x) / r,
    y: (e.y - n.y) / r
  };
}, jn = function(e) {
  return {
    x: e[0],
    y: e[1]
  };
}, _b = function(e) {
  for (var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0, n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : e.length, a = 1 / 0, i = r; i < n; i++) {
    var s = e[i];
    isFinite(s) && (a = Math.min(s, a));
  }
  return a;
}, Nb = function(e) {
  for (var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0, n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : e.length, a = -1 / 0, i = r; i < n; i++) {
    var s = e[i];
    isFinite(s) && (a = Math.max(s, a));
  }
  return a;
}, Fb = function(e) {
  for (var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0, n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : e.length, a = 0, i = 0, s = r; s < n; s++) {
    var o = e[s];
    isFinite(o) && (a += o, i++);
  }
  return a / i;
}, zb = function(e) {
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
}, Vb = function(e) {
  return Math.PI * e / 180;
}, Ni = function(e, r) {
  return Math.atan2(r, e) - Math.PI / 2;
}, tf = Math.log2 || function(t) {
  return Math.log(t) / Math.log(2);
}, rf = function(e) {
  return e > 0 ? 1 : e < 0 ? -1 : 0;
}, Ln = function(e, r) {
  return Math.sqrt(Cn(e, r));
}, Cn = function(e, r) {
  var n = r.x - e.x, a = r.y - e.y;
  return n * n + a * a;
}, qb = function(e) {
  for (var r = e.length, n = 0, a = 0; a < r; a++)
    n += e[a];
  for (var i = 0; i < r; i++)
    e[i] = e[i] / n;
  return e;
}, bt = function(e, r, n, a) {
  return (1 - a) * (1 - a) * e + 2 * (1 - a) * a * r + a * a * n;
}, sa = function(e, r, n, a) {
  return {
    x: bt(e.x, r.x, n.x, a),
    y: bt(e.y, r.y, n.y, a)
  };
}, $b = function(e, r, n, a) {
  var i = {
    x: r.x - e.x,
    y: r.y - e.y
  }, s = Ln(e, r), o = {
    x: i.x / s,
    y: i.y / s
  };
  return n = n ?? 0, a = a ?? n * s, {
    x: e.x + o.x * a,
    y: e.y + o.y * a
  };
}, si = function(e, r, n) {
  return Math.max(e, Math.min(n, r));
}, qt = function(e) {
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
}, Hb = function(e) {
  return {
    x1: e.x1,
    x2: e.x2,
    w: e.w,
    y1: e.y1,
    y2: e.y2,
    h: e.h
  };
}, Ub = function(e) {
  e.x1 = 1 / 0, e.y1 = 1 / 0, e.x2 = -1 / 0, e.y2 = -1 / 0, e.w = 0, e.h = 0;
}, Gb = function(e, r) {
  e.x1 = Math.min(e.x1, r.x1), e.x2 = Math.max(e.x2, r.x2), e.w = e.x2 - e.x1, e.y1 = Math.min(e.y1, r.y1), e.y2 = Math.max(e.y2, r.y2), e.h = e.y2 - e.y1;
}, sg = function(e, r, n) {
  e.x1 = Math.min(e.x1, r), e.x2 = Math.max(e.x2, r), e.w = e.x2 - e.x1, e.y1 = Math.min(e.y1, n), e.y2 = Math.max(e.y2, n), e.h = e.y2 - e.y1;
}, es = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : 0;
  return e.x1 -= r, e.x2 += r, e.y1 -= r, e.y2 += r, e.w = e.x2 - e.x1, e.h = e.y2 - e.y1, e;
}, ts = function(e) {
  var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : [0], n, a, i, s;
  if (r.length === 1)
    n = a = i = s = r[0];
  else if (r.length === 2)
    n = i = r[0], s = a = r[1];
  else if (r.length === 4) {
    var o = ct(r, 4);
    n = o[0], a = o[1], i = o[2], s = o[3];
  }
  return e.x1 -= s, e.x2 += a, e.y1 -= n, e.y2 += i, e.w = e.x2 - e.x1, e.h = e.y2 - e.y1, e;
}, Sc = function(e, r) {
  e.x1 = r.x1, e.y1 = r.y1, e.x2 = r.x2, e.y2 = r.y2, e.w = e.x2 - e.x1, e.h = e.y2 - e.y1;
}, nf = function(e, r) {
  return !(e.x1 > r.x2 || r.x1 > e.x2 || e.x2 < r.x1 || r.x2 < e.x1 || e.y2 < r.y1 || r.y2 < e.y1 || e.y1 > r.y2 || r.y1 > e.y2);
}, en = function(e, r, n) {
  return e.x1 <= r && r <= e.x2 && e.y1 <= n && n <= e.y2;
}, Pc = function(e, r) {
  return en(e, r.x, r.y);
}, og = function(e, r) {
  return en(e, r.x1, r.y1) && en(e, r.x2, r.y2);
}, Wb = (qo = Math.hypot) !== null && qo !== void 0 ? qo : function(t, e) {
  return Math.sqrt(t * t + e * e);
};
function Kb(t, e) {
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
    var D = Wb(S.x, S.y);
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
function Yb(t, e, r, n, a, i) {
  var s = n1(t, e, r, n, a), o = Kb(s, i), l = qt();
  return o.forEach(function(u) {
    return sg(l, u.x, u.y);
  }), l;
}
var lg = function(e, r, n, a, i, s, o) {
  var l = arguments.length > 7 && arguments[7] !== void 0 ? arguments[7] : "auto", u = l === "auto" ? ln(i, s) : l, f = i / 2, c = s / 2;
  u = Math.min(u, f, c);
  var v = u !== f, d = u !== c, h;
  if (v) {
    var y = n - f + u - o, g = a - c - o, p = n + f - u + o, m = g;
    if (h = tn(e, r, n, a, y, g, p, m, !1), h.length > 0)
      return h;
  }
  if (d) {
    var b = n + f + o, w = a - c + u - o, E = b, T = a + c - u + o;
    if (h = tn(e, r, n, a, b, w, E, T, !1), h.length > 0)
      return h;
  }
  if (v) {
    var x = n - f + u - o, S = a + c + o, D = n + f - u + o, A = S;
    if (h = tn(e, r, n, a, x, S, D, A, !1), h.length > 0)
      return h;
  }
  if (d) {
    var k = n - f - o, R = a - c + u - o, M = k, I = a + c - u + o;
    if (h = tn(e, r, n, a, k, R, M, I, !1), h.length > 0)
      return h;
  }
  var _;
  {
    var O = n - f + u, L = a - c + u;
    if (_ = za(e, r, n, a, O, L, u + o), _.length > 0 && _[0] <= O && _[1] <= L)
      return [_[0], _[1]];
  }
  {
    var N = n + f - u, H = a - c + u;
    if (_ = za(e, r, n, a, N, H, u + o), _.length > 0 && _[0] >= N && _[1] <= H)
      return [_[0], _[1]];
  }
  {
    var V = n + f - u, F = a + c - u;
    if (_ = za(e, r, n, a, V, F, u + o), _.length > 0 && _[0] >= V && _[1] >= F)
      return [_[0], _[1]];
  }
  {
    var $ = n - f + u, Q = a + c - u;
    if (_ = za(e, r, n, a, $, Q, u + o), _.length > 0 && _[0] <= $ && _[1] >= Q)
      return [_[0], _[1]];
  }
  return [];
}, Xb = function(e, r, n, a, i, s, o) {
  var l = o, u = Math.min(n, i), f = Math.max(n, i), c = Math.min(a, s), v = Math.max(a, s);
  return u - l <= e && e <= f + l && c - l <= r && r <= v + l;
}, Zb = function(e, r, n, a, i, s, o, l, u) {
  var f = {
    x1: Math.min(n, o, i) - u,
    x2: Math.max(n, o, i) + u,
    y1: Math.min(a, l, s) - u,
    y2: Math.max(a, l, s) + u
  };
  return !(e < f.x1 || e > f.x2 || r < f.y1 || r > f.y2);
}, Qb = function(e, r, n, a) {
  n -= a;
  var i = r * r - 4 * e * n;
  if (i < 0)
    return [];
  var s = Math.sqrt(i), o = 2 * e, l = (-r + s) / o, u = (-r - s) / o;
  return [l, u];
}, jb = function(e, r, n, a, i) {
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
}, Jb = function(e, r, n, a, i, s, o, l) {
  var u = 1 * n * n - 4 * n * i + 2 * n * o + 4 * i * i - 4 * i * o + o * o + a * a - 4 * a * s + 2 * a * l + 4 * s * s - 4 * s * l + l * l, f = 9 * n * i - 3 * n * n - 3 * n * o - 6 * i * i + 3 * i * o + 9 * a * s - 3 * a * a - 3 * a * l - 6 * s * s + 3 * s * l, c = 3 * n * n - 6 * n * i + n * o - n * e + 2 * i * i + 2 * i * e - o * e + 3 * a * a - 6 * a * s + a * l - a * r + 2 * s * s + 2 * s * r - l * r, v = 1 * n * i - n * n + n * e - i * e + a * s - a * a + a * r - s * r, d = [];
  jb(u, f, c, v, d);
  for (var h = 1e-7, y = [], g = 0; g < 6; g += 2)
    Math.abs(d[g + 1]) < h && d[g] >= 0 && d[g] <= 1 && y.push(d[g]);
  y.push(1), y.push(0);
  for (var p = -1, m, b, w, E = 0; E < y.length; E++)
    m = Math.pow(1 - y[E], 2) * n + 2 * (1 - y[E]) * y[E] * i + y[E] * y[E] * o, b = Math.pow(1 - y[E], 2) * a + 2 * (1 - y[E]) * y[E] * s + y[E] * y[E] * l, w = Math.pow(m - e, 2) + Math.pow(b - r, 2), p >= 0 ? w < p && (p = w) : p = w;
  return p;
}, e1 = function(e, r, n, a, i, s) {
  var o = [e - n, r - a], l = [i - n, s - a], u = l[0] * l[0] + l[1] * l[1], f = o[0] * o[0] + o[1] * o[1], c = o[0] * l[0] + o[1] * l[1], v = c * c / u;
  return c < 0 ? f : v > u ? (e - i) * (e - i) + (r - s) * (r - s) : f - v;
}, Wt = function(e, r, n) {
  for (var a, i, s, o, l, u = 0, f = 0; f < n.length / 2; f++)
    if (a = n[f * 2], i = n[f * 2 + 1], f + 1 < n.length / 2 ? (s = n[(f + 1) * 2], o = n[(f + 1) * 2 + 1]) : (s = n[(f + 1 - n.length / 2) * 2], o = n[(f + 1 - n.length / 2) * 2 + 1]), !(a == e && s == e)) if (a >= e && e >= s || a <= e && e <= s)
      l = (e - a) / (s - a) * (o - i) + i, l > r && u++;
    else
      continue;
  return u % 2 !== 0;
}, Ur = function(e, r, n, a, i, s, o, l, u) {
  var f = new Array(n.length), c;
  l[0] != null ? (c = Math.atan(l[1] / l[0]), l[0] < 0 ? c = c + Math.PI / 2 : c = -c - Math.PI / 2) : c = l;
  for (var v = Math.cos(-c), d = Math.sin(-c), h = 0; h < f.length / 2; h++)
    f[h * 2] = s / 2 * (n[h * 2] * v - n[h * 2 + 1] * d), f[h * 2 + 1] = o / 2 * (n[h * 2 + 1] * v + n[h * 2] * d), f[h * 2] += a, f[h * 2 + 1] += i;
  var y;
  if (u > 0) {
    var g = Ts(f, -u);
    y = Cs(g);
  } else
    y = f;
  return Wt(e, r, y);
}, t1 = function(e, r, n, a, i, s, o, l) {
  for (var u = new Array(n.length * 2), f = 0; f < l.length; f++) {
    var c = l[f];
    u[f * 4 + 0] = c.startX, u[f * 4 + 1] = c.startY, u[f * 4 + 2] = c.stopX, u[f * 4 + 3] = c.stopY;
    var v = Math.pow(c.cx - e, 2) + Math.pow(c.cy - r, 2);
    if (v <= Math.pow(c.radius, 2))
      return !0;
  }
  return Wt(e, r, u);
}, Cs = function(e) {
  for (var r = new Array(e.length / 2), n, a, i, s, o, l, u, f, c = 0; c < e.length / 4; c++) {
    n = e[c * 4], a = e[c * 4 + 1], i = e[c * 4 + 2], s = e[c * 4 + 3], c < e.length / 4 - 1 ? (o = e[(c + 1) * 4], l = e[(c + 1) * 4 + 1], u = e[(c + 1) * 4 + 2], f = e[(c + 1) * 4 + 3]) : (o = e[0], l = e[1], u = e[2], f = e[3]);
    var v = tn(n, a, i, s, o, l, u, f, !0);
    r[c * 2] = v[0], r[c * 2 + 1] = v[1];
  }
  return r;
}, Ts = function(e, r) {
  for (var n = new Array(e.length * 2), a, i, s, o, l = 0; l < e.length / 2; l++) {
    a = e[l * 2], i = e[l * 2 + 1], l < e.length / 2 - 1 ? (s = e[(l + 1) * 2], o = e[(l + 1) * 2 + 1]) : (s = e[0], o = e[1]);
    var u = o - i, f = -(s - a), c = Math.sqrt(u * u + f * f), v = u / c, d = f / c;
    n[l * 4] = a + v * r, n[l * 4 + 1] = i + d * r, n[l * 4 + 2] = s + v * r, n[l * 4 + 3] = o + d * r;
  }
  return n;
}, r1 = function(e, r, n, a, i, s) {
  var o = n - e, l = a - r;
  o /= i, l /= s;
  var u = Math.sqrt(o * o + l * l), f = u - 1;
  if (f < 0)
    return [];
  var c = f / u;
  return [(n - e) * c + e, (a - r) * c + r];
}, An = function(e, r, n, a, i, s, o) {
  return e -= i, r -= s, e /= n / 2 + o, r /= a / 2 + o, e * e + r * r <= 1;
}, za = function(e, r, n, a, i, s, o) {
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
}, $o = function(e, r, n) {
  return r <= e && e <= n || n <= e && e <= r ? e : e <= r && r <= n || n <= r && r <= e ? r : n;
}, tn = function(e, r, n, a, i, s, o, l, u) {
  var f = e - i, c = n - e, v = o - i, d = r - s, h = a - r, y = l - s, g = v * d - y * f, p = c * d - h * f, m = y * c - v * h;
  if (m !== 0) {
    var b = g / m, w = p / m, E = 1e-3, T = 0 - E, x = 1 + E;
    return T <= b && b <= x && T <= w && w <= x ? [e + b * c, r + b * h] : u ? [e + b * c, r + b * h] : [];
  } else
    return g === 0 || p === 0 ? $o(e, n, o) === o ? [o, l] : $o(e, n, i) === i ? [i, s] : $o(i, o, n) === n ? [n, a] : [] : [];
}, n1 = function(e, r, n, a, i) {
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
}, oi = function(e, r, n, a, i, s, o, l) {
  var u = [], f, c = new Array(n.length), v = !0;
  s == null && (v = !1);
  var d;
  if (v) {
    for (var h = 0; h < c.length / 2; h++)
      c[h * 2] = n[h * 2] * s + a, c[h * 2 + 1] = n[h * 2 + 1] * o + i;
    if (l > 0) {
      var y = Ts(c, -l);
      d = Cs(y);
    } else
      d = c;
  } else
    d = n;
  for (var g, p, m, b, w = 0; w < d.length / 2; w++)
    g = d[w * 2], p = d[w * 2 + 1], w < d.length / 2 - 1 ? (m = d[(w + 1) * 2], b = d[(w + 1) * 2 + 1]) : (m = d[0], b = d[1]), f = tn(e, r, a, i, g, p, m, b), f.length !== 0 && u.push(f[0], f[1]);
  return u;
}, a1 = function(e, r, n, a, i, s, o, l, u) {
  var f = [], c, v = new Array(n.length * 2);
  u.forEach(function(m, b) {
    b === 0 ? (v[v.length - 2] = m.startX, v[v.length - 1] = m.startY) : (v[b * 4 - 2] = m.startX, v[b * 4 - 1] = m.startY), v[b * 4] = m.stopX, v[b * 4 + 1] = m.stopY, c = za(e, r, a, i, m.cx, m.cy, m.radius), c.length !== 0 && f.push(c[0], c[1]);
  });
  for (var d = 0; d < v.length / 4; d++)
    c = tn(e, r, a, i, v[d * 4], v[d * 4 + 1], v[d * 4 + 2], v[d * 4 + 3], !1), c.length !== 0 && f.push(c[0], c[1]);
  if (f.length > 2) {
    for (var h = [f[0], f[1]], y = Math.pow(h[0] - e, 2) + Math.pow(h[1] - r, 2), g = 1; g < f.length / 2; g++) {
      var p = Math.pow(f[g * 2] - e, 2) + Math.pow(f[g * 2 + 1] - r, 2);
      p <= y && (h[0] = f[g * 2], h[1] = f[g * 2 + 1], y = p);
    }
    return h;
  }
  return f;
}, Fi = function(e, r, n) {
  var a = [e[0] - r[0], e[1] - r[1]], i = Math.sqrt(a[0] * a[0] + a[1] * a[1]), s = (i - n) / i;
  return s < 0 && (s = 1e-5), [r[0] + s * a[0], r[1] + s * a[1]];
}, Nt = function(e, r) {
  var n = du(e, r);
  return n = ug(n), n;
}, ug = function(e) {
  for (var r, n, a = e.length / 2, i = 1 / 0, s = 1 / 0, o = -1 / 0, l = -1 / 0, u = 0; u < a; u++)
    r = e[2 * u], n = e[2 * u + 1], i = Math.min(i, r), o = Math.max(o, r), s = Math.min(s, n), l = Math.max(l, n);
  for (var f = 2 / (o - i), c = 2 / (l - s), v = 0; v < a; v++)
    r = e[2 * v] = e[2 * v] * f, n = e[2 * v + 1] = e[2 * v + 1] * c, i = Math.min(i, r), o = Math.max(o, r), s = Math.min(s, n), l = Math.max(l, n);
  if (s < -1)
    for (var d = 0; d < a; d++)
      n = e[2 * d + 1] = e[2 * d + 1] + (-1 - s);
  return e;
}, du = function(e, r) {
  var n = 1 / e * 2 * Math.PI, a = e % 2 === 0 ? Math.PI / 2 + n / 2 : Math.PI / 2;
  a += r;
  for (var i = new Array(e * 2), s, o = 0; o < e; o++)
    s = o * n + a, i[2 * o] = Math.cos(s), i[2 * o + 1] = Math.sin(-s);
  return i;
}, ln = function(e, r) {
  return Math.min(e / 4, r / 4, 8);
}, fg = function(e, r) {
  return Math.min(e / 10, r / 10, 8);
}, af = function() {
  return 8;
}, i1 = function(e, r, n) {
  return [e - 2 * r + n, 2 * (r - e), e];
}, hu = function(e, r) {
  return {
    heightOffset: Math.min(15, 0.05 * r),
    widthOffset: Math.min(100, 0.25 * e),
    ctrlPtOffsetPct: 0.05
  };
};
function Ho(t, e) {
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
    var d = 1 / 0, h = -1 / 0, y = Kt(c), g;
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
  var i = [].concat(bs(r(t)), bs(r(e))), s = Kt(i), o;
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
var s1 = Dt({
  dampingFactor: 0.8,
  precision: 1e-6,
  iterations: 200,
  weight: function(e) {
    return 1;
  }
}), o1 = {
  pageRank: function(e) {
    for (var r = s1(e), n = r.dampingFactor, a = r.precision, i = r.iterations, s = r.weight, o = this._private.cy, l = this.byGroup(), u = l.nodes, f = l.edges, c = u.length, v = c * c, d = f.length, h = new Array(v), y = new Array(c), g = (1 - n) / c, p = 0; p < c; p++) {
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
        for (var O = 0; O < c; O++) {
          var L = O * c + M;
          h[L] = h[L] / y[M] + g;
        }
    for (var N = new Array(c), H = new Array(c), V, F = 0; F < c; F++)
      N[F] = 1;
    for (var $ = 0; $ < i; $++) {
      for (var Q = 0; Q < c; Q++)
        H[Q] = 0;
      for (var se = 0; se < c; se++)
        for (var ae = 0; ae < c; ae++) {
          var le = se * c + ae;
          H[se] += h[le] * N[ae];
        }
      qb(H), V = N, N = H, H = V;
      for (var ce = 0, he = 0; he < c; he++) {
        var ie = V[he] - N[he];
        ce += ie * ie;
      }
      if (ce < a)
        break;
    }
    var U = {
      rank: function(C) {
        return C = o.collection(C)[0], N[u.indexOf(C)];
      }
    };
    return U;
  }
  // pageRank
}, Dc = Dt({
  root: null,
  weight: function(e) {
    return 1;
  },
  directed: !1,
  alpha: 0
}), oa = {
  degreeCentralityNormalized: function(e) {
    e = Dc(e);
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
    e = Dc(e);
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
oa.dc = oa.degreeCentrality;
oa.dcn = oa.degreeCentralityNormalised = oa.degreeCentralityNormalized;
var Ac = Dt({
  harmonic: !0,
  weight: function() {
    return 1;
  },
  directed: !1,
  root: null
}), la = {
  closenessCentralityNormalized: function(e) {
    for (var r = Ac(e), n = r.harmonic, a = r.weight, i = r.directed, s = this.cy(), o = {}, l = 0, u = this.nodes(), f = this.floydWarshall({
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
    var r = Ac(e), n = r.root, a = r.weight, i = r.directed, s = r.harmonic;
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
la.cc = la.closenessCentrality;
la.ccn = la.closenessCentralityNormalised = la.closenessCentralityNormalized;
var l1 = Dt({
  weight: null,
  directed: !1
}), gu = {
  // Implemented from the algorithm in the paper "On Variants of Shortest-Path Betweenness Centrality and their Generic Computation" by Ulrik Brandes
  betweennessCentrality: function(e) {
    for (var r = l1(e), n = r.directed, a = r.weight, i = a != null, s = this.cy(), o = this.nodes(), l = {}, u = {}, f = 0, c = {
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
      for (var b = o[g].id(), w = [], E = {}, T = {}, x = {}, S = new Ci(function(se, ae) {
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
            var O = a(_);
            M = M.id(), x[M] > x[k] + O && (x[M] = x[k] + O, S.nodes.indexOf(M) < 0 ? S.push(M) : S.updateItem(M), T[M] = 0, E[M] = []), x[M] == x[k] + O && (T[M] = T[M] + T[k], E[M].push(k));
          }
        else
          for (var L = 0; L < l[k].length; L++) {
            var N = l[k][L].id();
            x[N] == 1 / 0 && (S.push(N), x[N] = x[k] + 1), x[N] == x[k] + 1 && (T[N] = T[N] + T[k], E[N].push(k));
          }
      }
      for (var H = {}, V = 0; V < o.length; V++)
        H[o[V].id()] = 0;
      for (; w.length > 0; ) {
        for (var F = w.pop(), $ = 0; $ < E[F].length; $++) {
          var Q = E[F][$];
          H[Q] = H[Q] + T[Q] / T[F] * (1 + H[F]);
        }
        F != o[g].id() && c.set(F, c.get(F) + H[F]);
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
gu.bc = gu.betweennessCentrality;
var u1 = Dt({
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
}), f1 = function(e) {
  return u1(e);
}, c1 = function(e, r) {
  for (var n = 0, a = 0; a < r.length; a++)
    n += r[a](e);
  return n;
}, v1 = function(e, r, n) {
  for (var a = 0; a < r; a++)
    e[a * r + a] = n;
}, cg = function(e, r) {
  for (var n, a = 0; a < r; a++) {
    n = 0;
    for (var i = 0; i < r; i++)
      n += e[i * r + a];
    for (var s = 0; s < r; s++)
      e[s * r + a] = e[s * r + a] / n;
  }
}, d1 = function(e, r, n) {
  for (var a = new Array(n * n), i = 0; i < n; i++) {
    for (var s = 0; s < n; s++)
      a[i * n + s] = 0;
    for (var o = 0; o < n; o++)
      for (var l = 0; l < n; l++)
        a[i * n + l] += e[i * n + o] * r[o * n + l];
  }
  return a;
}, h1 = function(e, r, n) {
  for (var a = e.slice(0), i = 1; i < n; i++)
    e = d1(e, a, r);
  return e;
}, g1 = function(e, r, n) {
  for (var a = new Array(r * r), i = 0; i < r * r; i++)
    a[i] = Math.pow(e[i], n);
  return cg(a, r), a;
}, p1 = function(e, r, n, a) {
  for (var i = 0; i < n; i++) {
    var s = Math.round(e[i] * Math.pow(10, a)) / Math.pow(10, a), o = Math.round(r[i] * Math.pow(10, a)) / Math.pow(10, a);
    if (s !== o)
      return !1;
  }
  return !0;
}, y1 = function(e, r, n, a) {
  for (var i = [], s = 0; s < r; s++) {
    for (var o = [], l = 0; l < r; l++)
      Math.round(e[s * r + l] * 1e3) / 1e3 > 0 && o.push(n[l]);
    o.length !== 0 && i.push(a.collection(o));
  }
  return i;
}, m1 = function(e, r) {
  for (var n = 0; n < e.length; n++)
    if (!r[n] || e[n].id() !== r[n].id())
      return !1;
  return !0;
}, b1 = function(e) {
  for (var r = 0; r < e.length; r++)
    for (var n = 0; n < e.length; n++)
      r != n && m1(e[r], e[n]) && e.splice(n, 1);
  return e;
}, kc = function(e) {
  for (var r = this.nodes(), n = this.edges(), a = this.cy(), i = f1(e), s = {}, o = 0; o < r.length; o++)
    s[r[o].id()] = o;
  for (var l = r.length, u = l * l, f = new Array(u), c, v = 0; v < u; v++)
    f[v] = 0;
  for (var d = 0; d < n.length; d++) {
    var h = n[d], y = s[h.source().id()], g = s[h.target().id()], p = c1(h, i.attributes);
    f[y * l + g] += p, f[g * l + y] += p;
  }
  v1(f, l, i.multFactor), cg(f, l);
  for (var m = !0, b = 0; m && b < i.maxIterations; )
    m = !1, c = h1(f, l, i.expandFactor), f = g1(c, l, i.inflateFactor), p1(f, c, u, 4) || (m = !0), b++;
  var w = y1(f, l, r, a);
  return w = b1(w), w;
}, w1 = {
  markovClustering: kc,
  mcl: kc
}, x1 = function(e) {
  return e;
}, vg = function(e, r) {
  return Math.abs(r - e);
}, Bc = function(e, r, n) {
  return e + vg(r, n);
}, Rc = function(e, r, n) {
  return e + Math.pow(n - r, 2);
}, E1 = function(e) {
  return Math.sqrt(e);
}, C1 = function(e, r, n) {
  return Math.max(e, vg(r, n));
}, Ba = function(e, r, n, a, i) {
  for (var s = arguments.length > 5 && arguments[5] !== void 0 ? arguments[5] : x1, o = a, l, u, f = 0; f < e; f++)
    l = r(f), u = n(f), o = i(o, l, u);
  return s(o);
}, ha = {
  euclidean: function(e, r, n) {
    return e >= 2 ? Ba(e, r, n, 0, Rc, E1) : Ba(e, r, n, 0, Bc);
  },
  squaredEuclidean: function(e, r, n) {
    return Ba(e, r, n, 0, Rc);
  },
  manhattan: function(e, r, n) {
    return Ba(e, r, n, 0, Bc);
  },
  max: function(e, r, n) {
    return Ba(e, r, n, -1 / 0, C1);
  }
};
ha["squared-euclidean"] = ha.squaredEuclidean;
ha.squaredeuclidean = ha.squaredEuclidean;
function Zs(t, e, r, n, a, i) {
  var s;
  return tt(t) ? s = t : s = ha[t] || ha.euclidean, e === 0 && tt(t) ? s(a, i) : s(e, r, n, a, i);
}
var T1 = Dt({
  k: 2,
  m: 2,
  sensitivityThreshold: 1e-4,
  distance: "euclidean",
  maxIterations: 10,
  attributes: [],
  testMode: !1,
  testCentroids: null
}), sf = function(e) {
  return T1(e);
}, Ss = function(e, r, n, a, i) {
  var s = i !== "kMedoids", o = s ? function(c) {
    return n[c];
  } : function(c) {
    return a[c](n);
  }, l = function(v) {
    return a[v](r);
  }, u = n, f = r;
  return Zs(e, a.length, o, l, u, f);
}, Uo = function(e, r, n) {
  for (var a = n.length, i = new Array(a), s = new Array(a), o = new Array(r), l = null, u = 0; u < a; u++)
    i[u] = e.min(n[u]).value, s[u] = e.max(n[u]).value;
  for (var f = 0; f < r; f++) {
    l = [];
    for (var c = 0; c < a; c++)
      l[c] = Math.random() * (s[c] - i[c]) + i[c];
    o[f] = l;
  }
  return o;
}, dg = function(e, r, n, a, i) {
  for (var s = 1 / 0, o = 0, l = 0; l < r.length; l++) {
    var u = Ss(n, e, r[l], a, i);
    u < s && (s = u, o = l);
  }
  return o;
}, hg = function(e, r, n) {
  for (var a = [], i = null, s = 0; s < r.length; s++)
    i = r[s], n[i.id()] === e && a.push(i);
  return a;
}, S1 = function(e, r, n) {
  return Math.abs(r - e) <= n;
}, P1 = function(e, r, n) {
  for (var a = 0; a < e.length; a++)
    for (var i = 0; i < e[a].length; i++) {
      var s = Math.abs(e[a][i] - r[a][i]);
      if (s > n)
        return !1;
    }
  return !0;
}, D1 = function(e, r, n) {
  for (var a = 0; a < n; a++)
    if (e === r[a]) return !0;
  return !1;
}, Mc = function(e, r) {
  var n = new Array(r);
  if (e.length < 50)
    for (var a = 0; a < r; a++) {
      for (var i = e[Math.floor(Math.random() * e.length)]; D1(i, n, a); )
        i = e[Math.floor(Math.random() * e.length)];
      n[a] = i;
    }
  else
    for (var s = 0; s < r; s++)
      n[s] = e[Math.floor(Math.random() * e.length)];
  return n;
}, Lc = function(e, r, n) {
  for (var a = 0, i = 0; i < r.length; i++)
    a += Ss("manhattan", r[i], e, n, "kMedoids");
  return a;
}, A1 = function(e) {
  var r = this.cy(), n = this.nodes(), a = null, i = sf(e), s = new Array(i.k), o = {}, l;
  i.testMode ? typeof i.testCentroids == "number" ? (i.testCentroids, l = Uo(n, i.k, i.attributes)) : gt(i.testCentroids) === "object" ? l = i.testCentroids : l = Uo(n, i.k, i.attributes) : l = Uo(n, i.k, i.attributes);
  for (var u = !0, f = 0; u && f < i.maxIterations; ) {
    for (var c = 0; c < n.length; c++)
      a = n[c], o[a.id()] = dg(a, l, i.distance, i.attributes, "kMeans");
    u = !1;
    for (var v = 0; v < i.k; v++) {
      var d = hg(v, n, o);
      if (d.length !== 0) {
        for (var h = i.attributes.length, y = l[v], g = new Array(h), p = new Array(h), m = 0; m < h; m++) {
          p[m] = 0;
          for (var b = 0; b < d.length; b++)
            a = d[b], p[m] += i.attributes[m](a);
          g[m] = p[m] / d.length, S1(g[m], y[m], i.sensitivityThreshold) || (u = !0);
        }
        l[v] = g, s[v] = r.collection(d);
      }
    }
    f++;
  }
  return s;
}, k1 = function(e) {
  var r = this.cy(), n = this.nodes(), a = null, i = sf(e), s = new Array(i.k), o, l = {}, u, f = new Array(i.k);
  i.testMode ? typeof i.testCentroids == "number" || (gt(i.testCentroids) === "object" ? o = i.testCentroids : o = Mc(n, i.k)) : o = Mc(n, i.k);
  for (var c = !0, v = 0; c && v < i.maxIterations; ) {
    for (var d = 0; d < n.length; d++)
      a = n[d], l[a.id()] = dg(a, o, i.distance, i.attributes, "kMedoids");
    c = !1;
    for (var h = 0; h < o.length; h++) {
      var y = hg(h, n, l);
      if (y.length !== 0) {
        f[h] = Lc(o[h], y, i.attributes);
        for (var g = 0; g < y.length; g++)
          u = Lc(y[g], y, i.attributes), u < f[h] && (f[h] = u, o[h] = y[g], c = !0);
        s[h] = r.collection(y);
      }
    }
    v++;
  }
  return s;
}, B1 = function(e, r, n, a, i) {
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
}, R1 = function(e, r, n, a, i) {
  for (var s = 0; s < e.length; s++)
    r[s] = e[s].slice();
  for (var o, l, u, f = 2 / (i.m - 1), c = 0; c < n.length; c++)
    for (var v = 0; v < a.length; v++) {
      o = 0;
      for (var d = 0; d < n.length; d++)
        l = Ss(i.distance, a[v], n[c], i.attributes, "cmeans"), u = Ss(i.distance, a[v], n[d], i.attributes, "cmeans"), o += Math.pow(l / u, f);
      e[v][c] = 1 / o;
    }
}, M1 = function(e, r, n, a) {
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
}, Ic = function(e) {
  var r = this.cy(), n = this.nodes(), a = sf(e), i, s, o, l, u;
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
    m = !1, B1(s, n, o, u, a), R1(o, l, s, n, a), P1(o, l, a.sensitivityThreshold) || (m = !0), b++;
  return i = M1(n, o, a, r), {
    clusters: i,
    degreeOfMembership: o
  };
}, L1 = {
  kMeans: A1,
  kMedoids: k1,
  fuzzyCMeans: Ic,
  fcm: Ic
}, I1 = Dt({
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
}), O1 = {
  single: "min",
  complete: "max"
}, _1 = function(e) {
  var r = I1(e), n = O1[r.linkage];
  return n != null && (r.linkage = n), r;
}, Oc = function(e, r, n, a, i) {
  for (var s = 0, o = 1 / 0, l, u = i.attributes, f = function(D, A) {
    return Zs(i.distance, u.length, function(k) {
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
}, Jn = function(e, r, n) {
  e && (e.value ? r.push(e.value) : (e.left && Jn(e.left, r), e.right && Jn(e.right, r)));
}, pu = function(e, r) {
  if (!e) return "";
  if (e.left && e.right) {
    var n = pu(e.left, r), a = pu(e.right, r), i = r.add({
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
}, yu = function(e, r, n) {
  if (!e) return [];
  var a = [], i = [], s = [];
  return r === 0 ? (e.left && Jn(e.left, a), e.right && Jn(e.right, i), s = a.concat(i), [n.collection(s)]) : r === 1 ? e.value ? [n.collection(e.value)] : (e.left && Jn(e.left, a), e.right && Jn(e.right, i), [n.collection(a), n.collection(i)]) : e.value ? [n.collection(e.value)] : (e.left && (a = yu(e.left, r - 1, n)), e.right && (i = yu(e.right, r - 1, n)), a.concat(i));
}, _c = function(e) {
  for (var r = this.cy(), n = this.nodes(), a = _1(e), i = a.attributes, s = function(b, w) {
    return Zs(a.distance, i.length, function(E) {
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
  for (var g = Oc(o, f, l, u, a); g; )
    g = Oc(o, f, l, u, a);
  var p;
  return a.mode === "dendrogram" ? (p = yu(o[0], a.dendrogramDepth, r), a.addDendrogram && pu(o[0], r)) : (p = new Array(o.length), o.forEach(function(m, b) {
    m.key = m.index = null, p[b] = r.collection(m.value);
  })), p;
}, N1 = {
  hierarchicalClustering: _c,
  hca: _c
}, F1 = Dt({
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
}), z1 = function(e) {
  var r = e.damping, n = e.preference;
  0.5 <= r && r < 1 || je("Damping must range on [0.5, 1).  Got: ".concat(r));
  var a = ["median", "mean", "min", "max"];
  return a.some(function(i) {
    return i === n;
  }) || pe(n) || je("Preference must be one of [".concat(a.map(function(i) {
    return "'".concat(i, "'");
  }).join(", "), "] or a number.  Got: ").concat(n)), F1(e);
}, V1 = function(e, r, n, a) {
  var i = function(o, l) {
    return a[l](o);
  };
  return -Zs(e, a.length, function(s) {
    return i(r, s);
  }, function(s) {
    return i(n, s);
  }, r, n);
}, q1 = function(e, r) {
  var n = null;
  return r === "median" ? n = zb(e) : r === "mean" ? n = Fb(e) : r === "min" ? n = _b(e) : r === "max" ? n = Nb(e) : n = r, n;
}, $1 = function(e, r, n) {
  for (var a = [], i = 0; i < e; i++)
    r[i * e + i] + n[i * e + i] > 0 && a.push(i);
  return a;
}, Nc = function(e, r, n) {
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
}, H1 = function(e, r, n) {
  for (var a = Nc(e, r, n), i = 0; i < n.length; i++) {
    for (var s = [], o = 0; o < a.length; o++)
      a[o] === n[i] && s.push(o);
    for (var l = -1, u = -1 / 0, f = 0; f < s.length; f++) {
      for (var c = 0, v = 0; v < s.length; v++)
        c += r[s[v] * e + s[f]];
      c > u && (l = f, u = c);
    }
    n[i] = s[l];
  }
  return a = Nc(e, r, n), a;
}, Fc = function(e) {
  for (var r = this.cy(), n = this.nodes(), a = z1(e), i = {}, s = 0; s < n.length; s++)
    i[n[s].id()] = s;
  var o, l, u, f, c, v;
  o = n.length, l = o * o, u = new Array(l);
  for (var d = 0; d < l; d++)
    u[d] = -1 / 0;
  for (var h = 0; h < o; h++)
    for (var y = 0; y < o; y++)
      h !== y && (u[h * o + y] = V1(a.distance, n[h], n[y], a.attributes));
  f = q1(u, a.preference);
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
      for (var O = 0; O < o; O++)
        c[A * o + O] = (1 - a.damping) * (u[A * o + O] - k) + a.damping * b[O];
      c[A * o + M] = (1 - a.damping) * (u[A * o + M] - R) + a.damping * b[M];
    }
    for (var L = 0; L < o; L++) {
      for (var N = 0, H = 0; H < o; H++)
        b[H] = v[H * o + L], w[H] = Math.max(0, c[H * o + L]), N += w[H];
      N -= w[L], w[L] = c[L * o + L], N += w[L];
      for (var V = 0; V < o; V++)
        v[V * o + L] = (1 - a.damping) * Math.min(0, N - w[V]) + a.damping * b[V];
      v[L * o + L] = (1 - a.damping) * (N - w[L]) + a.damping * b[L];
    }
    for (var F = 0, $ = 0; $ < o; $++) {
      var Q = v[$ * o + $] + c[$ * o + $] > 0 ? 1 : 0;
      x[D % a.minIterations * o + $] = Q, F += Q;
    }
    if (F > 0 && (D >= a.minIterations - 1 || D == a.maxIterations - 1)) {
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
  for (var ce = $1(o, c, v), he = H1(o, u, ce), ie = {}, U = 0; U < ce.length; U++)
    ie[ce[U]] = [];
  for (var X = 0; X < n.length; X++) {
    var C = i[n[X].id()], B = he[C];
    B != null && ie[B].push(n[X]);
  }
  for (var z = new Array(ce.length), W = 0; W < ce.length; W++)
    z[W] = r.collection(ie[ce[W]]);
  return z;
}, U1 = {
  affinityPropagation: Fc,
  ap: Fc
}, G1 = Dt({
  root: void 0,
  directed: !1
}), W1 = {
  hierholzer: function(e) {
    if (!Fe(e)) {
      var r = arguments;
      e = {
        root: r[0],
        directed: r[1]
      };
    }
    var n = G1(e), a = n.root, i = n.directed, s = this, o = !1, l, u, f;
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
}, zi = function() {
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
}, K1 = {
  hopcroftTarjanBiconnected: zi,
  htbc: zi,
  htb: zi,
  hopcroftTarjanBiconnectedComponents: zi
}, Vi = function() {
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
}, Y1 = {
  tarjanStronglyConnected: Vi,
  tsc: Vi,
  tscc: Vi,
  tarjanStronglyConnectedComponents: Vi
}, gg = {};
[ii, Tb, Sb, Db, kb, Rb, Ib, o1, oa, la, gu, w1, L1, N1, U1, W1, K1, Y1].forEach(function(t) {
  Ae(gg, t);
});
/*!
Embeddable Minimum Strictly-Compliant Promises/A+ 1.1.1 Thenable
Copyright (c) 2013-2014 Ralf S. Engelschall (http://engelschall.com)
Licensed under The MIT License (http://opensource.org/licenses/MIT)
*/
var pg = 0, yg = 1, mg = 2, vr = function(e) {
  if (!(this instanceof vr)) return new vr(e);
  this.id = "Thenable/1.0.7", this.state = pg, this.fulfillValue = void 0, this.rejectReason = void 0, this.onFulfilled = [], this.onRejected = [], this.proxy = {
    then: this.then.bind(this)
  }, typeof e == "function" && e.call(this, this.fulfill.bind(this), this.reject.bind(this));
};
vr.prototype = {
  /*  promise resolving methods  */
  fulfill: function(e) {
    return zc(this, yg, "fulfillValue", e);
  },
  reject: function(e) {
    return zc(this, mg, "rejectReason", e);
  },
  /*  "The then Method" [Promises/A+ 1.1, 1.2, 2.2]  */
  then: function(e, r) {
    var n = this, a = new vr();
    return n.onFulfilled.push(qc(e, a, "fulfill")), n.onRejected.push(qc(r, a, "reject")), bg(n), a.proxy;
  }
};
var zc = function(e, r, n, a) {
  return e.state === pg && (e.state = r, e[n] = a, bg(e)), e;
}, bg = function(e) {
  e.state === yg ? Vc(e, "onFulfilled", e.fulfillValue) : e.state === mg && Vc(e, "onRejected", e.rejectReason);
}, Vc = function(e, r, n) {
  if (e[r].length !== 0) {
    var a = e[r];
    e[r] = [];
    var i = function() {
      for (var o = 0; o < a.length; o++) a[o](n);
    };
    typeof setImmediate == "function" ? setImmediate(i) : setTimeout(i, 0);
  }
}, qc = function(e, r, n) {
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
      wg(r, i);
    }
  };
}, wg = function(e, r) {
  if (e === r || e.proxy === r) {
    e.reject(new TypeError("cannot resolve promise with itself"));
    return;
  }
  var n;
  if (gt(r) === "object" && r !== null || typeof r == "function")
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
          a || (a = !0, i === r ? e.reject(new TypeError("circular thenable chain")) : wg(e, i));
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
vr.all = function(t) {
  return new vr(function(e, r) {
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
vr.resolve = function(t) {
  return new vr(function(e, r) {
    e(t);
  });
};
vr.reject = function(t) {
  return new vr(function(e, r) {
    r(t);
  });
};
var ba = typeof Promise < "u" ? Promise : vr, mu = function(e, r, n) {
  var a = Xu(e), i = !a, s = this._private = Ae({
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
}, In = mu.prototype;
Ae(In, {
  instanceString: function() {
    return "animation";
  },
  hook: function() {
    var e = this._private;
    if (!e.hooked) {
      var r, n = e.target._private.animation;
      e.queue ? r = n.queue : r = n.current, r.push(this), Qt(e.target) && e.target.cy().addToAnimationPool(e.target), e.hooked = !0;
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
    return new ba(function(a, i) {
      n.push(function() {
        a();
      });
    });
  }
});
In.complete = In.completed;
In.run = In.play;
In.running = In.playing;
var X1 = {
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
        return new mu(s[0], r);
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
        r.position = ig(v, h, d);
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
      if (l && Fe(r.zoom)) {
        var E = o.getZoomedViewport(r.zoom);
        E != null ? (E.zoomed && (r.zoom = E.zoom), E.panned && (r.pan = E.pan)) : r.zoom = null;
      }
      return new mu(s[0], r);
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
}, Go, $c;
function Qs() {
  if ($c) return Go;
  $c = 1;
  var t = Array.isArray;
  return Go = t, Go;
}
var Wo, Hc;
function Z1() {
  if (Hc) return Wo;
  Hc = 1;
  var t = Qs(), e = xi(), r = /\.|\[(?:[^[\]]*|(["'])(?:(?!\1)[^\\]|\\.)*?\1)\]/, n = /^\w*$/;
  function a(i, s) {
    if (t(i))
      return !1;
    var o = typeof i;
    return o == "number" || o == "symbol" || o == "boolean" || i == null || e(i) ? !0 : n.test(i) || !r.test(i) || s != null && i in Object(s);
  }
  return Wo = a, Wo;
}
var Ko, Uc;
function Q1() {
  if (Uc) return Ko;
  Uc = 1;
  var t = Zh(), e = wi(), r = "[object AsyncFunction]", n = "[object Function]", a = "[object GeneratorFunction]", i = "[object Proxy]";
  function s(o) {
    if (!e(o))
      return !1;
    var l = t(o);
    return l == n || l == a || l == r || l == i;
  }
  return Ko = s, Ko;
}
var Yo, Gc;
function j1() {
  if (Gc) return Yo;
  Gc = 1;
  var t = Ks(), e = t["__core-js_shared__"];
  return Yo = e, Yo;
}
var Xo, Wc;
function J1() {
  if (Wc) return Xo;
  Wc = 1;
  var t = j1(), e = (function() {
    var n = /[^.]+$/.exec(t && t.keys && t.keys.IE_PROTO || "");
    return n ? "Symbol(src)_1." + n : "";
  })();
  function r(n) {
    return !!e && e in n;
  }
  return Xo = r, Xo;
}
var Zo, Kc;
function ew() {
  if (Kc) return Zo;
  Kc = 1;
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
  return Zo = r, Zo;
}
var Qo, Yc;
function tw() {
  if (Yc) return Qo;
  Yc = 1;
  var t = Q1(), e = J1(), r = wi(), n = ew(), a = /[\\^$.*+?()[\]{}|]/g, i = /^\[object .+?Constructor\]$/, s = Function.prototype, o = Object.prototype, l = s.toString, u = o.hasOwnProperty, f = RegExp(
    "^" + l.call(u).replace(a, "\\$&").replace(/hasOwnProperty|(function).*?(?=\\\()| for .+?(?=\\\])/g, "$1.*?") + "$"
  );
  function c(v) {
    if (!r(v) || e(v))
      return !1;
    var d = t(v) ? f : i;
    return d.test(n(v));
  }
  return Qo = c, Qo;
}
var jo, Xc;
function rw() {
  if (Xc) return jo;
  Xc = 1;
  function t(e, r) {
    return e == null ? void 0 : e[r];
  }
  return jo = t, jo;
}
var Jo, Zc;
function of() {
  if (Zc) return Jo;
  Zc = 1;
  var t = tw(), e = rw();
  function r(n, a) {
    var i = e(n, a);
    return t(i) ? i : void 0;
  }
  return Jo = r, Jo;
}
var el, Qc;
function js() {
  if (Qc) return el;
  Qc = 1;
  var t = of(), e = t(Object, "create");
  return el = e, el;
}
var tl, jc;
function nw() {
  if (jc) return tl;
  jc = 1;
  var t = js();
  function e() {
    this.__data__ = t ? t(null) : {}, this.size = 0;
  }
  return tl = e, tl;
}
var rl, Jc;
function aw() {
  if (Jc) return rl;
  Jc = 1;
  function t(e) {
    var r = this.has(e) && delete this.__data__[e];
    return this.size -= r ? 1 : 0, r;
  }
  return rl = t, rl;
}
var nl, ev;
function iw() {
  if (ev) return nl;
  ev = 1;
  var t = js(), e = "__lodash_hash_undefined__", r = Object.prototype, n = r.hasOwnProperty;
  function a(i) {
    var s = this.__data__;
    if (t) {
      var o = s[i];
      return o === e ? void 0 : o;
    }
    return n.call(s, i) ? s[i] : void 0;
  }
  return nl = a, nl;
}
var al, tv;
function sw() {
  if (tv) return al;
  tv = 1;
  var t = js(), e = Object.prototype, r = e.hasOwnProperty;
  function n(a) {
    var i = this.__data__;
    return t ? i[a] !== void 0 : r.call(i, a);
  }
  return al = n, al;
}
var il, rv;
function ow() {
  if (rv) return il;
  rv = 1;
  var t = js(), e = "__lodash_hash_undefined__";
  function r(n, a) {
    var i = this.__data__;
    return this.size += this.has(n) ? 0 : 1, i[n] = t && a === void 0 ? e : a, this;
  }
  return il = r, il;
}
var sl, nv;
function lw() {
  if (nv) return sl;
  nv = 1;
  var t = nw(), e = aw(), r = iw(), n = sw(), a = ow();
  function i(s) {
    var o = -1, l = s == null ? 0 : s.length;
    for (this.clear(); ++o < l; ) {
      var u = s[o];
      this.set(u[0], u[1]);
    }
  }
  return i.prototype.clear = t, i.prototype.delete = e, i.prototype.get = r, i.prototype.has = n, i.prototype.set = a, sl = i, sl;
}
var ol, av;
function uw() {
  if (av) return ol;
  av = 1;
  function t() {
    this.__data__ = [], this.size = 0;
  }
  return ol = t, ol;
}
var ll, iv;
function xg() {
  if (iv) return ll;
  iv = 1;
  function t(e, r) {
    return e === r || e !== e && r !== r;
  }
  return ll = t, ll;
}
var ul, sv;
function Js() {
  if (sv) return ul;
  sv = 1;
  var t = xg();
  function e(r, n) {
    for (var a = r.length; a--; )
      if (t(r[a][0], n))
        return a;
    return -1;
  }
  return ul = e, ul;
}
var fl, ov;
function fw() {
  if (ov) return fl;
  ov = 1;
  var t = Js(), e = Array.prototype, r = e.splice;
  function n(a) {
    var i = this.__data__, s = t(i, a);
    if (s < 0)
      return !1;
    var o = i.length - 1;
    return s == o ? i.pop() : r.call(i, s, 1), --this.size, !0;
  }
  return fl = n, fl;
}
var cl, lv;
function cw() {
  if (lv) return cl;
  lv = 1;
  var t = Js();
  function e(r) {
    var n = this.__data__, a = t(n, r);
    return a < 0 ? void 0 : n[a][1];
  }
  return cl = e, cl;
}
var vl, uv;
function vw() {
  if (uv) return vl;
  uv = 1;
  var t = Js();
  function e(r) {
    return t(this.__data__, r) > -1;
  }
  return vl = e, vl;
}
var dl, fv;
function dw() {
  if (fv) return dl;
  fv = 1;
  var t = Js();
  function e(r, n) {
    var a = this.__data__, i = t(a, r);
    return i < 0 ? (++this.size, a.push([r, n])) : a[i][1] = n, this;
  }
  return dl = e, dl;
}
var hl, cv;
function hw() {
  if (cv) return hl;
  cv = 1;
  var t = uw(), e = fw(), r = cw(), n = vw(), a = dw();
  function i(s) {
    var o = -1, l = s == null ? 0 : s.length;
    for (this.clear(); ++o < l; ) {
      var u = s[o];
      this.set(u[0], u[1]);
    }
  }
  return i.prototype.clear = t, i.prototype.delete = e, i.prototype.get = r, i.prototype.has = n, i.prototype.set = a, hl = i, hl;
}
var gl, vv;
function gw() {
  if (vv) return gl;
  vv = 1;
  var t = of(), e = Ks(), r = t(e, "Map");
  return gl = r, gl;
}
var pl, dv;
function pw() {
  if (dv) return pl;
  dv = 1;
  var t = lw(), e = hw(), r = gw();
  function n() {
    this.size = 0, this.__data__ = {
      hash: new t(),
      map: new (r || e)(),
      string: new t()
    };
  }
  return pl = n, pl;
}
var yl, hv;
function yw() {
  if (hv) return yl;
  hv = 1;
  function t(e) {
    var r = typeof e;
    return r == "string" || r == "number" || r == "symbol" || r == "boolean" ? e !== "__proto__" : e === null;
  }
  return yl = t, yl;
}
var ml, gv;
function eo() {
  if (gv) return ml;
  gv = 1;
  var t = yw();
  function e(r, n) {
    var a = r.__data__;
    return t(n) ? a[typeof n == "string" ? "string" : "hash"] : a.map;
  }
  return ml = e, ml;
}
var bl, pv;
function mw() {
  if (pv) return bl;
  pv = 1;
  var t = eo();
  function e(r) {
    var n = t(this, r).delete(r);
    return this.size -= n ? 1 : 0, n;
  }
  return bl = e, bl;
}
var wl, yv;
function bw() {
  if (yv) return wl;
  yv = 1;
  var t = eo();
  function e(r) {
    return t(this, r).get(r);
  }
  return wl = e, wl;
}
var xl, mv;
function ww() {
  if (mv) return xl;
  mv = 1;
  var t = eo();
  function e(r) {
    return t(this, r).has(r);
  }
  return xl = e, xl;
}
var El, bv;
function xw() {
  if (bv) return El;
  bv = 1;
  var t = eo();
  function e(r, n) {
    var a = t(this, r), i = a.size;
    return a.set(r, n), this.size += a.size == i ? 0 : 1, this;
  }
  return El = e, El;
}
var Cl, wv;
function Ew() {
  if (wv) return Cl;
  wv = 1;
  var t = pw(), e = mw(), r = bw(), n = ww(), a = xw();
  function i(s) {
    var o = -1, l = s == null ? 0 : s.length;
    for (this.clear(); ++o < l; ) {
      var u = s[o];
      this.set(u[0], u[1]);
    }
  }
  return i.prototype.clear = t, i.prototype.delete = e, i.prototype.get = r, i.prototype.has = n, i.prototype.set = a, Cl = i, Cl;
}
var Tl, xv;
function Cw() {
  if (xv) return Tl;
  xv = 1;
  var t = Ew(), e = "Expected a function";
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
  return r.Cache = t, Tl = r, Tl;
}
var Sl, Ev;
function Tw() {
  if (Ev) return Sl;
  Ev = 1;
  var t = Cw(), e = 500;
  function r(n) {
    var a = t(n, function(s) {
      return i.size === e && i.clear(), s;
    }), i = a.cache;
    return a;
  }
  return Sl = r, Sl;
}
var Pl, Cv;
function Eg() {
  if (Cv) return Pl;
  Cv = 1;
  var t = Tw(), e = /[^.[\]]+|\[(?:(-?\d+(?:\.\d+)?)|(["'])((?:(?!\2)[^\\]|\\.)*?)\2)\]|(?=(?:\.|\[\])(?:\.|\[\]|$))/g, r = /\\(\\)?/g, n = t(function(a) {
    var i = [];
    return a.charCodeAt(0) === 46 && i.push(""), a.replace(e, function(s, o, l, u) {
      i.push(l ? u.replace(r, "$1") : o || s);
    }), i;
  });
  return Pl = n, Pl;
}
var Dl, Tv;
function Cg() {
  if (Tv) return Dl;
  Tv = 1;
  function t(e, r) {
    for (var n = -1, a = e == null ? 0 : e.length, i = Array(a); ++n < a; )
      i[n] = r(e[n], n, e);
    return i;
  }
  return Dl = t, Dl;
}
var Al, Sv;
function Sw() {
  if (Sv) return Al;
  Sv = 1;
  var t = Qu(), e = Cg(), r = Qs(), n = xi(), a = t ? t.prototype : void 0, i = a ? a.toString : void 0;
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
  return Al = s, Al;
}
var kl, Pv;
function Tg() {
  if (Pv) return kl;
  Pv = 1;
  var t = Sw();
  function e(r) {
    return r == null ? "" : t(r);
  }
  return kl = e, kl;
}
var Bl, Dv;
function Sg() {
  if (Dv) return Bl;
  Dv = 1;
  var t = Qs(), e = Z1(), r = Eg(), n = Tg();
  function a(i, s) {
    return t(i) ? i : e(i, s) ? [i] : r(n(i));
  }
  return Bl = a, Bl;
}
var Rl, Av;
function lf() {
  if (Av) return Rl;
  Av = 1;
  var t = xi();
  function e(r) {
    if (typeof r == "string" || t(r))
      return r;
    var n = r + "";
    return n == "0" && 1 / r == -1 / 0 ? "-0" : n;
  }
  return Rl = e, Rl;
}
var Ml, kv;
function Pw() {
  if (kv) return Ml;
  kv = 1;
  var t = Sg(), e = lf();
  function r(n, a) {
    a = t(a, n);
    for (var i = 0, s = a.length; n != null && i < s; )
      n = n[e(a[i++])];
    return i && i == s ? n : void 0;
  }
  return Ml = r, Ml;
}
var Ll, Bv;
function Dw() {
  if (Bv) return Ll;
  Bv = 1;
  var t = Pw();
  function e(r, n, a) {
    var i = r == null ? void 0 : t(r, n);
    return i === void 0 ? a : i;
  }
  return Ll = e, Ll;
}
var Aw = Dw(), kw = /* @__PURE__ */ bi(Aw), Il, Rv;
function Bw() {
  if (Rv) return Il;
  Rv = 1;
  var t = of(), e = (function() {
    try {
      var r = t(Object, "defineProperty");
      return r({}, "", {}), r;
    } catch {
    }
  })();
  return Il = e, Il;
}
var Ol, Mv;
function Rw() {
  if (Mv) return Ol;
  Mv = 1;
  var t = Bw();
  function e(r, n, a) {
    n == "__proto__" && t ? t(r, n, {
      configurable: !0,
      enumerable: !0,
      value: a,
      writable: !0
    }) : r[n] = a;
  }
  return Ol = e, Ol;
}
var _l, Lv;
function Mw() {
  if (Lv) return _l;
  Lv = 1;
  var t = Rw(), e = xg(), r = Object.prototype, n = r.hasOwnProperty;
  function a(i, s, o) {
    var l = i[s];
    (!(n.call(i, s) && e(l, o)) || o === void 0 && !(s in i)) && t(i, s, o);
  }
  return _l = a, _l;
}
var Nl, Iv;
function Lw() {
  if (Iv) return Nl;
  Iv = 1;
  var t = 9007199254740991, e = /^(?:0|[1-9]\d*)$/;
  function r(n, a) {
    var i = typeof n;
    return a = a ?? t, !!a && (i == "number" || i != "symbol" && e.test(n)) && n > -1 && n % 1 == 0 && n < a;
  }
  return Nl = r, Nl;
}
var Fl, Ov;
function Iw() {
  if (Ov) return Fl;
  Ov = 1;
  var t = Mw(), e = Sg(), r = Lw(), n = wi(), a = lf();
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
  return Fl = i, Fl;
}
var zl, _v;
function Ow() {
  if (_v) return zl;
  _v = 1;
  var t = Iw();
  function e(r, n, a) {
    return r == null ? r : t(r, n, a);
  }
  return zl = e, zl;
}
var _w = Ow(), Nw = /* @__PURE__ */ bi(_w), Vl, Nv;
function Fw() {
  if (Nv) return Vl;
  Nv = 1;
  function t(e, r) {
    var n = -1, a = e.length;
    for (r || (r = Array(a)); ++n < a; )
      r[n] = e[n];
    return r;
  }
  return Vl = t, Vl;
}
var ql, Fv;
function zw() {
  if (Fv) return ql;
  Fv = 1;
  var t = Cg(), e = Fw(), r = Qs(), n = xi(), a = Eg(), i = lf(), s = Tg();
  function o(l) {
    return r(l) ? t(l, i) : n(l) ? [l] : e(a(s(l)));
  }
  return ql = o, ql;
}
var Vw = zw(), qw = /* @__PURE__ */ bi(Vw), $w = {
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
        var c = a.indexOf(".") !== -1, v = c && qw(a);
        if (s.allowGetting && i === void 0) {
          var d;
          return f && (s.beforeGet(f), v && f._private[s.field][a] === void 0 ? d = kw(f._private[s.field], v) : d = f._private[s.field][a]), d;
        } else if (s.allowSetting && i !== void 0) {
          var h = !s.immutableKeys[a];
          if (h) {
            var y = Vh({}, a, i);
            s.beforeSet(o, y);
            for (var g = 0, p = u.length; g < p; g++) {
              var m = u[g];
              s.canSet(m) && (v && f._private[s.field][a] === void 0 ? Nw(m._private[s.field], v, i) : m._private[s.field][a] = i);
            }
            s.updateStyle && o.updateStyle(), s.onSet(o), s.settingTriggersEvent && o[s.triggerFnName](s.settingEvent);
          }
        }
      } else if (s.allowSetting && Fe(a)) {
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
          if (!sn(v)) {
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
}, Hw = {
  eventAliasesOn: function(e) {
    var r = e;
    r.addListener = r.listen = r.bind = r.on, r.unlisten = r.unbind = r.off = r.removeListener, r.trigger = r.emit, r.pon = r.promiseOn = function(n, a) {
      var i = this, s = Array.prototype.slice.call(arguments, 0);
      return new ba(function(o, l) {
        var u = function(d) {
          i.off.apply(i, c), o(d);
        }, f = s.concat([u]), c = f.concat([]);
        i.on.apply(i, f);
      });
    };
  }
}, $e = {};
[X1, $w, Hw].forEach(function(t) {
  Ae($e, t);
});
var Uw = {
  animate: $e.animate(),
  animation: $e.animation(),
  animated: $e.animated(),
  clearQueue: $e.clearQueue(),
  delay: $e.delay(),
  delayAnimation: $e.delayAnimation(),
  stop: $e.stop()
}, rs = {
  classes: function(e) {
    var r = this;
    if (e === void 0) {
      var n = [];
      return r[0]._private.classes.forEach(function(h) {
        return n.push(h);
      }), n;
    } else Ke(e) || (e = (e || "").match(/\S+/g) || []);
    for (var a = [], i = new ma(e), s = 0; s < r.length; s++) {
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
    Ke(e) || (e = e.match(/\S+/g) || []);
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
rs.className = rs.classNames = rs.classes;
var Ne = {
  metaChar: "[\\!\\\"\\#\\$\\%\\&\\'\\(\\)\\*\\+\\,\\.\\/\\:\\;\\<\\=\\>\\?\\@\\[\\]\\^\\`\\{\\|\\}\\~]",
  // chars we need to escape in let names, etc
  comparatorOp: "=|\\!=|>|>=|<|<=|\\$=|\\^=|\\*=",
  // binary comparison op (used in data selectors)
  boolOp: "\\?|\\!|\\^",
  // boolean (unary) operators (used in data selectors)
  string: `"(?:\\\\"|[^"])*"|'(?:\\\\'|[^'])*'`,
  // string literals (used in data selectors) -- doublequotes | singlequotes
  number: ht,
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
var Ue = function() {
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
}, bu = [{
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
  return $0(t.selector, e.selector);
}), Gw = (function() {
  for (var t = {}, e, r = 0; r < bu.length; r++)
    e = bu[r], t[e.selector] = e.matches;
  return t;
})(), Ww = function(e, r) {
  return Gw[e](r);
}, Kw = "(" + bu.map(function(t) {
  return t.selector;
}).join("|") + ")", $n = function(e) {
  return e.replace(new RegExp("\\\\(" + Ne.metaChar + ")", "g"), function(r, n) {
    return n;
  });
}, Zr = function(e, r, n) {
  e[e.length - 1] = n;
}, wu = [{
  name: "group",
  // just used for identifying when debugging
  query: !0,
  regex: "(" + Ne.group + ")",
  populate: function(e, r, n) {
    var a = ct(n, 1), i = a[0];
    r.checks.push({
      type: we.GROUP,
      value: i === "*" ? i : i + "s"
    });
  }
}, {
  name: "state",
  query: !0,
  regex: Kw,
  populate: function(e, r, n) {
    var a = ct(n, 1), i = a[0];
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
    var a = ct(n, 1), i = a[0];
    r.checks.push({
      type: we.ID,
      value: $n(i)
    });
  }
}, {
  name: "className",
  query: !0,
  regex: "\\.(" + Ne.className + ")",
  populate: function(e, r, n) {
    var a = ct(n, 1), i = a[0];
    r.checks.push({
      type: we.CLASS,
      value: $n(i)
    });
  }
}, {
  name: "dataExists",
  query: !0,
  regex: "\\[\\s*(" + Ne.variable + ")\\s*\\]",
  populate: function(e, r, n) {
    var a = ct(n, 1), i = a[0];
    r.checks.push({
      type: we.DATA_EXIST,
      field: $n(i)
    });
  }
}, {
  name: "dataCompare",
  query: !0,
  regex: "\\[\\s*(" + Ne.variable + ")\\s*(" + Ne.comparatorOp + ")\\s*(" + Ne.value + ")\\s*\\]",
  populate: function(e, r, n) {
    var a = ct(n, 3), i = a[0], s = a[1], o = a[2], l = new RegExp("^" + Ne.string + "$").exec(o) != null;
    l ? o = o.substring(1, o.length - 1) : o = parseFloat(o), r.checks.push({
      type: we.DATA_COMPARE,
      field: $n(i),
      operator: s,
      value: o
    });
  }
}, {
  name: "dataBool",
  query: !0,
  regex: "\\[\\s*(" + Ne.boolOp + ")\\s*(" + Ne.variable + ")\\s*\\]",
  populate: function(e, r, n) {
    var a = ct(n, 2), i = a[0], s = a[1];
    r.checks.push({
      type: we.DATA_BOOL,
      field: $n(s),
      operator: i
    });
  }
}, {
  name: "metaCompare",
  query: !0,
  regex: "\\[\\[\\s*(" + Ne.meta + ")\\s*(" + Ne.comparatorOp + ")\\s*(" + Ne.number + ")\\s*\\]\\]",
  populate: function(e, r, n) {
    var a = ct(n, 3), i = a[0], s = a[1], o = a[2];
    r.checks.push({
      type: we.META_COMPARE,
      field: $n(i),
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
    var o = e[e.length++] = Ue();
    return o;
  }
}, {
  name: "directedEdge",
  separator: !0,
  regex: Ne.directedEdge,
  populate: function(e, r) {
    if (e.currentSubject == null) {
      var n = Ue(), a = r, i = Ue();
      return n.checks.push({
        type: we.DIRECTED_EDGE,
        source: a,
        target: i
      }), Zr(e, r, n), e.edgeCount++, i;
    } else {
      var s = Ue(), o = r, l = Ue();
      return s.checks.push({
        type: we.NODE_SOURCE,
        source: o,
        target: l
      }), Zr(e, r, s), e.edgeCount++, l;
    }
  }
}, {
  name: "undirectedEdge",
  separator: !0,
  regex: Ne.undirectedEdge,
  populate: function(e, r) {
    if (e.currentSubject == null) {
      var n = Ue(), a = r, i = Ue();
      return n.checks.push({
        type: we.UNDIRECTED_EDGE,
        nodes: [a, i]
      }), Zr(e, r, n), e.edgeCount++, i;
    } else {
      var s = Ue(), o = r, l = Ue();
      return s.checks.push({
        type: we.NODE_NEIGHBOR,
        node: o,
        neighbor: l
      }), Zr(e, r, s), l;
    }
  }
}, {
  name: "child",
  separator: !0,
  regex: Ne.child,
  populate: function(e, r) {
    if (e.currentSubject == null) {
      var n = Ue(), a = Ue(), i = e[e.length - 1];
      return n.checks.push({
        type: we.CHILD,
        parent: i,
        child: a
      }), Zr(e, r, n), e.compoundCount++, a;
    } else if (e.currentSubject === r) {
      var s = Ue(), o = e[e.length - 1], l = Ue(), u = Ue(), f = Ue(), c = Ue();
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
      }), Zr(e, o, s), e.currentSubject = u, e.compoundCount++, f;
    } else {
      var v = Ue(), d = Ue(), h = [{
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
      var n = Ue(), a = Ue(), i = e[e.length - 1];
      return n.checks.push({
        type: we.DESCENDANT,
        ancestor: i,
        descendant: a
      }), Zr(e, r, n), e.compoundCount++, a;
    } else if (e.currentSubject === r) {
      var s = Ue(), o = e[e.length - 1], l = Ue(), u = Ue(), f = Ue(), c = Ue();
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
      }), Zr(e, o, s), e.currentSubject = u, e.compoundCount++, f;
    } else {
      var v = Ue(), d = Ue(), h = [{
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
      return He("Redefinition of subject in selector `" + e.toString() + "`"), !1;
    e.currentSubject = r;
    var n = e[e.length - 1], a = n.checks[0], i = a == null ? null : a.type;
    i === we.DIRECTED_EDGE ? a.type = we.NODE_TARGET : i === we.UNDIRECTED_EDGE && (a.type = we.NODE_NEIGHBOR, a.node = a.nodes[1], a.neighbor = a.nodes[0], a.nodes = null);
  }
}];
wu.forEach(function(t) {
  return t.regexObj = new RegExp("^" + t.regex);
});
var Yw = function(e) {
  for (var r, n, a, i = 0; i < wu.length; i++) {
    var s = wu[i], o = s.name, l = e.match(s.regexObj);
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
}, Xw = function(e) {
  var r = e.match(/^\s+/);
  if (r) {
    var n = r[0];
    e = e.substring(n.length);
  }
  return e;
}, Zw = function(e) {
  var r = this, n = r.inputText = e, a = r[0] = Ue();
  for (r.length = 1, n = Xw(n); ; ) {
    var i = Yw(n);
    if (i.expr == null)
      return He("The selector `" + e + "`is invalid"), !1;
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
      return He("The selector `" + e + "` is invalid because it uses both a compound selector and an edge selector"), !1;
    if (f.edgeCount > 1)
      return He("The selector `" + e + "` is invalid because it uses multiple edge selectors"), !1;
    f.edgeCount === 1 && He("The selector `" + e + "` is deprecated.  Edge selectors do not take effect on changes to source and target nodes after an edge is added, for performance reasons.  Use a class or data selector on edges instead, updating the class or data of an edge when your app detects a change in source or target nodes.");
  }
  return !0;
}, Qw = function() {
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
}, jw = {
  parse: Zw,
  toString: Qw
}, Pg = function(e, r, n) {
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
}, Jw = function(e, r) {
  switch (r) {
    case "?":
      return !!e;
    case "!":
      return !e;
    case "^":
      return e === void 0;
  }
}, ex = function(e) {
  return e !== void 0;
}, uf = function(e, r) {
  return e.data(r);
}, tx = function(e, r) {
  return e[r]();
}, it = [], Qe = function(e, r) {
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
  return Ww(r, e);
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
  return Pg(tx(e, r), n, a);
};
it[we.DATA_COMPARE] = function(t, e) {
  var r = t.field, n = t.operator, a = t.value;
  return Pg(uf(e, r), n, a);
};
it[we.DATA_BOOL] = function(t, e) {
  var r = t.field, n = t.operator;
  return Jw(uf(e, r), n);
};
it[we.DATA_EXIST] = function(t, e) {
  var r = t.field;
  return t.operator, ex(uf(e, r));
};
it[we.UNDIRECTED_EDGE] = function(t, e) {
  var r = t.nodes[0], n = t.nodes[1], a = e.source(), i = e.target();
  return Qe(r, a) && Qe(n, i) || Qe(n, a) && Qe(r, i);
};
it[we.NODE_NEIGHBOR] = function(t, e) {
  return Qe(t.node, e) && e.neighborhood().some(function(r) {
    return r.isNode() && Qe(t.neighbor, r);
  });
};
it[we.DIRECTED_EDGE] = function(t, e) {
  return Qe(t.source, e.source()) && Qe(t.target, e.target());
};
it[we.NODE_SOURCE] = function(t, e) {
  return Qe(t.source, e) && e.outgoers().some(function(r) {
    return r.isNode() && Qe(t.target, r);
  });
};
it[we.NODE_TARGET] = function(t, e) {
  return Qe(t.target, e) && e.incomers().some(function(r) {
    return r.isNode() && Qe(t.source, r);
  });
};
it[we.CHILD] = function(t, e) {
  return Qe(t.child, e) && Qe(t.parent, e.parent());
};
it[we.PARENT] = function(t, e) {
  return Qe(t.parent, e) && e.children().some(function(r) {
    return Qe(t.child, r);
  });
};
it[we.DESCENDANT] = function(t, e) {
  return Qe(t.descendant, e) && e.ancestors().some(function(r) {
    return Qe(t.ancestor, r);
  });
};
it[we.ANCESTOR] = function(t, e) {
  return Qe(t.ancestor, e) && e.descendants().some(function(r) {
    return Qe(t.descendant, r);
  });
};
it[we.COMPOUND_SPLIT] = function(t, e) {
  return Qe(t.subject, e) && Qe(t.left, e) && Qe(t.right, e);
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
var rx = function(e) {
  var r = this;
  if (r.length === 1 && r[0].checks.length === 1 && r[0].checks[0].type === we.ID)
    return e.getElementById(r[0].checks[0].value).collection();
  var n = function(i) {
    for (var s = 0; s < r.length; s++) {
      var o = r[s];
      if (Qe(o, i))
        return !0;
    }
    return !1;
  };
  return r.text() == null && (n = function() {
    return !0;
  }), e.filter(n);
}, nx = function(e) {
  for (var r = this, n = 0; n < r.length; n++) {
    var a = r[n];
    if (Qe(a, e))
      return !0;
  }
  return !1;
}, ax = {
  matches: nx,
  filter: rx
}, un = function(e) {
  this.inputText = e, this.currentSubject = null, this.compoundCount = 0, this.edgeCount = 0, this.length = 0, e == null || Se(e) && e.match(/^\s*$/) || (Qt(e) ? this.addQuery({
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
}, fn = un.prototype;
[jw, ax].forEach(function(t) {
  return Ae(fn, t);
});
fn.text = function() {
  return this.inputText;
};
fn.size = function() {
  return this.length;
};
fn.eq = function(t) {
  return this[t];
};
fn.sameText = function(t) {
  return !this.invalid && !t.invalid && this.text() === t.text();
};
fn.addQuery = function(t) {
  this[this.length++] = t;
};
fn.selector = fn.toString;
var rn = {
  allAre: function(e) {
    var r = new un(e);
    return this.every(function(n) {
      return r.matches(n);
    });
  },
  is: function(e) {
    var r = new un(e);
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
rn.allAreNeighbours = rn.allAreNeighbors;
rn.has = rn.contains;
rn.equal = rn.equals = rn.same;
var tr = function(e, r) {
  return function(a, i, s, o) {
    var l = a, u = this, f;
    if (l == null ? f = "" : Qt(l) && l.length === 1 && (f = l.id()), u.length === 1 && f) {
      var c = u[0]._private, v = c.traversalCache = c.traversalCache || {}, d = v[r] = v[r] || [], h = Mn(f), y = d[h];
      return y || (d[h] = e.call(u, a, i, s, o));
    } else
      return e.call(u, a, i, s, o);
  };
}, ga = {
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
  children: tr(function(t) {
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
function ff(t, e, r, n) {
  for (var a = [], i = new ma(), s = t.cy(), o = s.hasCompoundNodes(), l = 0; l < t.length; l++) {
    var u = t[l];
    r ? a.push(u) : o && n(a, i, u);
  }
  for (; a.length > 0; ) {
    var f = a.shift();
    e(f), i.add(f.id()), o && n(a, i, f);
  }
  return t;
}
function Dg(t, e, r) {
  if (r.isParent())
    for (var n = r._private.children, a = 0; a < n.length; a++) {
      var i = n[a];
      e.has(i.id()) || t.push(i);
    }
}
ga.forEachDown = function(t) {
  var e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0;
  return ff(this, t, e, Dg);
};
function Ag(t, e, r) {
  if (r.isChild()) {
    var n = r._private.parent;
    e.has(n.id()) || t.push(n);
  }
}
ga.forEachUp = function(t) {
  var e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0;
  return ff(this, t, e, Ag);
};
function ix(t, e, r) {
  Ag(t, e, r), Dg(t, e, r);
}
ga.forEachUpAndDown = function(t) {
  var e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0;
  return ff(this, t, e, ix);
};
ga.ancestors = ga.parents;
var li, kg;
li = kg = {
  data: $e.data({
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
  removeData: $e.removeData({
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
  scratch: $e.data({
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
  removeScratch: $e.removeData({
    field: "scratch",
    event: "scratch",
    triggerFnName: "trigger",
    triggerEvent: !0,
    updateStyle: !0
  }),
  rscratch: $e.data({
    field: "rscratch",
    allowBinding: !1,
    allowSetting: !0,
    settingTriggersEvent: !1,
    allowGetting: !0
  }),
  removeRscratch: $e.removeData({
    field: "rscratch",
    triggerEvent: !1
  }),
  id: function() {
    var e = this[0];
    if (e)
      return e._private.data.id;
  }
};
li.attr = li.data;
li.removeAttr = li.removeData;
var sx = kg, to = {};
function $l(t) {
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
Ae(to, {
  degree: $l(function(t, e) {
    return e.source().same(e.target()) ? 2 : 1;
  }),
  indegree: $l(function(t, e) {
    return e.target().same(t) ? 1 : 0;
  }),
  outdegree: $l(function(t, e) {
    return e.source().same(t) ? 1 : 0;
  })
});
function Hn(t, e) {
  return function(r) {
    for (var n, a = this.nodes(), i = 0; i < a.length; i++) {
      var s = a[i], o = s[t](r);
      o !== void 0 && (n === void 0 || e(o, n)) && (n = o);
    }
    return n;
  };
}
Ae(to, {
  minDegree: Hn("degree", function(t, e) {
    return t < e;
  }),
  maxDegree: Hn("degree", function(t, e) {
    return t > e;
  }),
  minIndegree: Hn("indegree", function(t, e) {
    return t < e;
  }),
  maxIndegree: Hn("indegree", function(t, e) {
    return t > e;
  }),
  minOutdegree: Hn("outdegree", function(t, e) {
    return t < e;
  }),
  maxOutdegree: Hn("outdegree", function(t, e) {
    return t > e;
  })
});
Ae(to, {
  totalDegree: function(e) {
    for (var r = 0, n = this.nodes(), a = 0; a < n.length; a++)
      r += n[a].degree(e);
    return r;
  }
});
var ur, Bg, Rg = function(e, r, n) {
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
}, zv = {
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
    Rg(e, r, !1);
  },
  onSet: function(e) {
    e.dirtyCompoundBoundsCache();
  },
  canSet: function(e) {
    return !e.locked();
  }
};
ur = Bg = {
  position: $e.data(zv),
  // position but no notification to renderer
  silentPosition: $e.data(Ae({}, zv, {
    allowBinding: !1,
    allowSetting: !0,
    settingTriggersEvent: !1,
    allowGetting: !1,
    beforeSet: function(e, r) {
      Rg(e, r, !0);
    },
    onSet: function(e) {
      e.dirtyCompoundBoundsCache();
    }
  })),
  positions: function(e, r) {
    if (Fe(e))
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
    if (Fe(e) ? (a = {
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
    return Fe(e) ? this.shift(e, !0) : Se(e) && pe(r) && this.shift(e, r, !0), this;
  },
  // get/set the rendered (i.e. on screen) positon of the element
  renderedPosition: function(e, r) {
    var n = this[0], a = this.cy(), i = a.zoom(), s = a.pan(), o = Fe(e) ? e : void 0, l = o !== void 0 || r !== void 0 && Se(e);
    if (n && n.isNode())
      if (l)
        for (var u = 0; u < this.length; u++) {
          var f = this[u];
          r !== void 0 ? f.position(e, (r - s[e]) / i) : o !== void 0 && f.position(ig(o, i, s));
        }
      else {
        var c = n.position();
        return o = Xs(c, i, s), e === void 0 ? o : o[e];
      }
    else if (!l)
      return;
    return this;
  },
  // get/set the position relative to the parent
  relativePosition: function(e, r) {
    var n = this[0], a = this.cy(), i = Fe(e) ? e : void 0, s = i !== void 0 || r !== void 0 && Se(e), o = a.hasCompoundNodes();
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
ur.modelPosition = ur.point = ur.position;
ur.modelPositions = ur.points = ur.positions;
ur.renderedPoint = ur.renderedPosition;
ur.relativePoint = ur.relativePosition;
var ox = Bg, pa = function(e) {
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
}, ya = function(e) {
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
}, lx = function(e) {
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
}, ua, hn;
ua = hn = {};
hn.renderedBoundingBox = function(t) {
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
hn.dirtyCompoundBoundsCache = function() {
  var t = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : !1, e = this.cy();
  return !e.styleEnabled() || !e.hasCompoundNodes() ? this : (this.forEachUp(function(r) {
    if (r.isParent()) {
      var n = r._private;
      n.compoundBoundsClean = !1, n.bbCache = null, t || r.emitAndNotify("bounds");
    }
  }), this);
};
hn.updateCompoundBounds = function() {
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
var er = function(e) {
  return e === 1 / 0 || e === -1 / 0 ? 0 : e;
}, or = function(e, r, n, a, i) {
  a - r === 0 || i - n === 0 || r == null || n == null || a == null || i == null || (e.x1 = r < e.x1 ? r : e.x1, e.x2 = a > e.x2 ? a : e.x2, e.y1 = n < e.y1 ? n : e.y1, e.y2 = i > e.y2 ? i : e.y2, e.w = e.x2 - e.x1, e.h = e.y2 - e.y1);
}, jr = function(e, r) {
  return r == null ? e : or(e, r.x1, r.y1, r.x2, r.y2);
}, Ra = function(e, r, n) {
  return Ft(e, r, n);
}, qi = function(e, r, n) {
  if (!r.cy().headless()) {
    var a = r._private, i = a.rstyle, s = i.arrowWidth / 2, o = r.pstyle(n + "-arrow-shape").value, l, u;
    if (o !== "none") {
      n === "source" ? (l = i.srcX, u = i.srcY) : n === "target" ? (l = i.tgtX, u = i.tgtY) : (l = i.midX, u = i.midY);
      var f = a.arrowBounds = a.arrowBounds || {}, c = f[n] = f[n] || {};
      c.x1 = l - s, c.y1 = u - s, c.x2 = l + s, c.y2 = u + s, c.w = c.x2 - c.x1, c.h = c.y2 - c.y1, es(c, 1), or(e, c.x1, c.y1, c.x2, c.y2);
    }
  }
}, Hl = function(e, r, n) {
  if (!r.cy().headless()) {
    var a;
    n ? a = n + "-" : a = "";
    var i = r._private, s = i.rstyle, o = r.pstyle(a + "label").strValue;
    if (o) {
      var l = r.pstyle("text-halign"), u = r.pstyle("text-valign"), f = Ra(s, "labelWidth", n), c = Ra(s, "labelHeight", n), v = Ra(s, "labelX", n), d = Ra(s, "labelY", n), h = r.pstyle(a + "text-margin-x").pfValue, y = r.pstyle(a + "text-margin-y").pfValue, g = r.isEdge(), p = r.pstyle(a + "text-rotation"), m = r.pstyle("text-outline-width").pfValue, b = r.pstyle("text-border-width").pfValue, w = b / 2, E = r.pstyle("text-background-padding").pfValue, T = 2, x = c, S = f, D = S / 2, A = x / 2, k, R, M, I;
      if (g)
        k = v - D, R = v + D, M = d - A, I = d + A;
      else {
        switch (pa(l.value)) {
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
        switch (ya(u.value)) {
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
      var _ = h - Math.max(m, w) - E - T, O = h + Math.max(m, w) + E + T, L = y - Math.max(m, w) - E - T, N = y + Math.max(m, w) + E + T;
      k += _, R += O, M += L, I += N;
      var H = n || "main", V = i.labelBounds, F = V[H] = V[H] || {};
      F.x1 = k, F.y1 = M, F.x2 = R, F.y2 = I, F.w = R - k, F.h = I - M, F.leftPad = _, F.rightPad = O, F.topPad = L, F.botPad = N;
      var $ = g && p.strValue === "autorotate", Q = p.pfValue != null && p.pfValue !== 0;
      if ($ || Q) {
        var se = $ ? Ra(i.rstyle, "labelAngle", n) : p.pfValue, ae = Math.cos(se), le = Math.sin(se), ce = (k + R) / 2, he = (M + I) / 2;
        if (!g) {
          switch (pa(l.value)) {
            case "left":
              ce = R;
              break;
            case "right":
              ce = k;
              break;
          }
          switch (ya(u.value)) {
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
      W.x1 = k, W.y1 = M, W.x2 = R, W.y2 = I, W.w = R - k, W.h = I - M, or(e, k, M, R, I), or(i.labelBounds.all, k, M, R, I);
    }
    return e;
  }
}, Vv = function(e, r) {
  if (!r.cy().headless()) {
    var n = r.pstyle("outline-opacity").value, a = r.pstyle("outline-width").value, i = r.pstyle("outline-offset").value, s = a + i;
    Mg(e, r, n, s, "outside", s / 2);
  }
}, Mg = function(e, r, n, a, i, s) {
  if (!(n === 0 || a <= 0 || i === "inside")) {
    var o = r.cy(), l = o.renderer(), u = l.nodeShapes[l.getNodeShape(r)];
    if (u) {
      var f = r.position(), c = f.x, v = f.y, d = r.width(), h = r.height();
      if (u.hasMiterBounds) {
        i === "center" && (a /= 2);
        var y = u.miterBounds(c, v, d, h, a);
        jr(e, y);
      } else s != null && s > 0 && ts(e, [s, s, s, s]);
    }
  }
}, ux = function(e, r) {
  if (!r.cy().headless()) {
    var n = r.pstyle("border-opacity").value, a = r.pstyle("border-width").pfValue, i = r.pstyle("border-position").value;
    Mg(e, r, n, a, i);
  }
}, fx = function(e, r) {
  var n = e._private.cy, a = n.styleEnabled(), i = n.headless(), s = qt(), o = e._private, l = e.isNode(), u = e.isEdge(), f, c, v, d, h, y, g = o.rstyle, p = l && a ? e.pstyle("bounds-expansion").pfValue : [0], m = function(j) {
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
      f = h - M, c = h + M, v = y - _, d = y + _, or(s, f, v, c, d), a && Vv(s, e), a && r.includeOutlines && !i && Vv(s, e), a && ux(s, e);
    } else if (u && r.includeEdges)
      if (a && !i) {
        var O = e.pstyle("curve-style").strValue;
        if (f = Math.min(g.srcX, g.midX, g.tgtX), c = Math.max(g.srcX, g.midX, g.tgtX), v = Math.min(g.srcY, g.midY, g.tgtY), d = Math.max(g.srcY, g.midY, g.tgtY), f -= A, c += A, v -= A, d += A, or(s, f, v, c, d), O === "haystack") {
          var L = g.haystackPts;
          if (L && L.length === 2) {
            if (f = L[0].x, v = L[0].y, c = L[1].x, d = L[1].y, f > c) {
              var N = f;
              f = c, c = N;
            }
            if (v > d) {
              var H = v;
              v = d, d = H;
            }
            or(s, f - A, v - A, c + A, d + A);
          }
        } else if (O === "bezier" || O === "unbundled-bezier" || Jr(O, "segments") || Jr(O, "taxi")) {
          var V;
          switch (O) {
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
            for (var F = 0; F < V.length; F++) {
              var $ = V[F];
              f = $.x - A, c = $.x + A, v = $.y - A, d = $.y + A, or(s, f, v, c, d);
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
        f -= A, c += A, v -= A, d += A, or(s, f, v, c, d);
      }
    if (a && r.includeEdges && u && (qi(s, e, "mid-source"), qi(s, e, "mid-target"), qi(s, e, "source"), qi(s, e, "target")), a) {
      var ie = e.pstyle("ghost").value === "yes";
      if (ie) {
        var U = e.pstyle("ghost-offset-x").pfValue, X = e.pstyle("ghost-offset-y").pfValue;
        or(s, s.x1 + U, s.y1 + X, s.x2 + U, s.y2 + X);
      }
    }
    var C = o.bodyBounds = o.bodyBounds || {};
    Sc(C, s), ts(C, p), es(C, 1), a && (f = s.x1, c = s.x2, v = s.y1, d = s.y2, or(s, f - S, v - S, c + S, d + S));
    var B = o.overlayBounds = o.overlayBounds || {};
    Sc(B, s), ts(B, p), es(B, 1);
    var z = o.labelBounds = o.labelBounds || {};
    z.all != null ? Ub(z.all) : z.all = qt(), a && r.includeLabels && (r.includeMainLabels && Hl(s, e, null), u && (r.includeSourceLabels && Hl(s, e, "source"), r.includeTargetLabels && Hl(s, e, "target")));
  }
  return s.x1 = er(s.x1), s.y1 = er(s.y1), s.x2 = er(s.x2), s.y2 = er(s.y2), s.w = er(s.x2 - s.x1), s.h = er(s.y2 - s.y1), s.w > 0 && s.h > 0 && b && (ts(s, p), es(s, 1)), s;
}, Lg = function(e) {
  var r = 0, n = function(s) {
    return (s ? 1 : 0) << r++;
  }, a = 0;
  return a += n(e.incudeNodes), a += n(e.includeEdges), a += n(e.includeLabels), a += n(e.includeMainLabels), a += n(e.includeSourceLabels), a += n(e.includeTargetLabels), a += n(e.includeOverlays), a += n(e.includeOutlines), a;
}, Ig = function(e) {
  var r = function(o) {
    return Math.round(o);
  };
  if (e.isEdge()) {
    var n = e.source().position(), a = e.target().position();
    return bc([r(n.x), r(n.y), r(a.x), r(a.y)]);
  } else {
    var i = e.position();
    return bc([r(i.x), r(i.y)]);
  }
}, qv = function(e, r) {
  var n = e._private, a, i = e.isEdge(), s = r == null ? $v : Lg(r), o = s === $v;
  if (n.bbCache == null ? (a = fx(e, ui), n.bbCache = a, n.bbCachePosKey = Ig(e)) : a = n.bbCache, !o) {
    var l = e.isNode();
    a = qt(), (r.includeNodes && l || r.includeEdges && !l) && (r.includeOverlays ? jr(a, n.overlayBounds) : jr(a, n.bodyBounds)), r.includeLabels && (r.includeMainLabels && (!i || r.includeSourceLabels && r.includeTargetLabels) ? jr(a, n.labelBounds.all) : (r.includeMainLabels && jr(a, n.labelBounds.mainRot), r.includeSourceLabels && jr(a, n.labelBounds.sourceRot), r.includeTargetLabels && jr(a, n.labelBounds.targetRot))), a.w = a.x2 - a.x1, a.h = a.y2 - a.y1;
  }
  return a;
}, ui = {
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
}, $v = Lg(ui), Hv = Dt(ui);
hn.boundingBox = function(t) {
  var e, r = t === void 0 || t.useCache === void 0 || t.useCache === !0, n = da(function(f) {
    var c = f._private;
    return c.bbCache == null || c.styleDirty || c.bbCachePosKey !== Ig(f);
  }, function(f) {
    return f.id();
  });
  if (r && this.length === 1 && !n(this[0]))
    t === void 0 ? t = ui : t = Hv(t), e = qv(this[0], t);
  else {
    e = qt(), t = t || ui;
    var a = Hv(t), i = this, s = i.cy(), o = s.styleEnabled();
    this.edges().forEach(n), this.nodes().forEach(n), o && this.recalculateRenderedStyle(r), this.updateCompoundBounds(!r);
    for (var l = 0; l < i.length; l++) {
      var u = i[l];
      n(u) && u.dirtyBoundingBoxCache(), jr(e, qv(u, a));
    }
  }
  return e.x1 = er(e.x1), e.y1 = er(e.y1), e.x2 = er(e.x2), e.y2 = er(e.y2), e.w = er(e.x2 - e.x1), e.h = er(e.y2 - e.y1), e;
};
hn.dirtyBoundingBoxCache = function() {
  for (var t = 0; t < this.length; t++) {
    var e = this[t]._private;
    e.bbCache = null, e.bbCachePosKey = null, e.bodyBounds = null, e.overlayBounds = null, e.labelBounds.all = null, e.labelBounds.source = null, e.labelBounds.target = null, e.labelBounds.main = null, e.labelBounds.sourceRot = null, e.labelBounds.targetRot = null, e.labelBounds.mainRot = null, e.arrowBounds.source = null, e.arrowBounds.target = null, e.arrowBounds["mid-source"] = null, e.arrowBounds["mid-target"] = null;
  }
  return this.emitAndNotify("bounds"), this;
};
hn.boundingBoxAt = function(t) {
  var e = this.nodes(), r = this.cy(), n = r.hasCompoundNodes(), a = r.collection();
  if (n && (a = e.filter(function(u) {
    return u.isParent();
  }), e = e.not(a)), Fe(t)) {
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
  var l = Hb(this.boundingBox({
    useCache: !1
  }));
  return e.silentPositions(o), n && (a.dirtyCompoundBoundsCache(), a.dirtyBoundingBoxCache(), a.updateCompoundBounds(!0)), r.endBatch(), l;
};
ua.boundingbox = ua.bb = ua.boundingBox;
ua.renderedBoundingbox = ua.renderedBoundingBox;
var cx = hn, Va, Ti;
Va = Ti = {};
var Og = function(e) {
  e.uppercaseName = ac(e.name), e.autoName = "auto" + e.uppercaseName, e.labelName = "label" + e.uppercaseName, e.outerName = "outer" + e.uppercaseName, e.uppercaseOuterName = ac(e.outerName), Va[e.name] = function() {
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
  }, Va["outer" + e.uppercaseName] = function() {
    var n = this[0], a = n._private, i = a.cy, s = i._private.styleEnabled;
    if (n)
      if (s) {
        var o = n[e.name](), l = n.pstyle("border-position").value, u;
        l === "center" ? u = n.pstyle("border-width").pfValue : l === "outside" ? u = 2 * n.pstyle("border-width").pfValue : u = 0;
        var f = 2 * n.padding();
        return o + u + f;
      } else
        return 1;
  }, Va["rendered" + e.uppercaseName] = function() {
    var n = this[0];
    if (n) {
      var a = n[e.name]();
      return a * this.cy().zoom();
    }
  }, Va["rendered" + e.uppercaseOuterName] = function() {
    var n = this[0];
    if (n) {
      var a = n[e.outerName]();
      return a * this.cy().zoom();
    }
  };
};
Og({
  name: "width"
});
Og({
  name: "height"
});
Ti.padding = function() {
  var t = this[0], e = t._private;
  return t.isParent() ? (t.updateCompoundBounds(), e.autoPadding !== void 0 ? e.autoPadding : t.pstyle("padding").pfValue) : t.pstyle("padding").pfValue;
};
Ti.paddedHeight = function() {
  var t = this[0];
  return t.height() + 2 * t.padding();
};
Ti.paddedWidth = function() {
  var t = this[0];
  return t.width() + 2 * t.padding();
};
var vx = Ti, dx = function(e, r) {
  if (e.isEdge() && e.takesUpSpace())
    return r(e);
}, hx = function(e, r) {
  if (e.isEdge() && e.takesUpSpace()) {
    var n = e.cy();
    return Xs(r(e), n.zoom(), n.pan());
  }
}, gx = function(e, r) {
  if (e.isEdge() && e.takesUpSpace()) {
    var n = e.cy(), a = n.pan(), i = n.zoom();
    return r(e).map(function(s) {
      return Xs(s, i, a);
    });
  }
}, px = function(e) {
  return e.renderer().getControlPoints(e);
}, yx = function(e) {
  return e.renderer().getSegmentPoints(e);
}, mx = function(e) {
  return e.renderer().getSourceEndpoint(e);
}, bx = function(e) {
  return e.renderer().getTargetEndpoint(e);
}, wx = function(e) {
  return e.renderer().getEdgeMidpoint(e);
}, Uv = {
  controlPoints: {
    get: px,
    mult: !0
  },
  segmentPoints: {
    get: yx,
    mult: !0
  },
  sourceEndpoint: {
    get: mx
  },
  targetEndpoint: {
    get: bx
  },
  midpoint: {
    get: wx
  }
}, xx = function(e) {
  return "rendered" + e[0].toUpperCase() + e.substr(1);
}, Ex = Object.keys(Uv).reduce(function(t, e) {
  var r = Uv[e], n = xx(e);
  return t[e] = function() {
    return dx(this, r.get);
  }, r.mult ? t[n] = function() {
    return gx(this, r.get);
  } : t[n] = function() {
    return hx(this, r.get);
  }, t;
}, {}), Cx = Ae({}, ox, cx, vx, Ex);
/*!
Event object based on jQuery events, MIT license

https://jquery.org/license/
https://tldrlegal.com/license/mit-license
https://github.com/jquery/jquery/blob/master/src/event.js
*/
var _g = function(e, r) {
  this.recycle(e, r);
};
function Ma() {
  return !1;
}
function $i() {
  return !0;
}
_g.prototype = {
  instanceString: function() {
    return "event";
  },
  recycle: function(e, r) {
    if (this.isImmediatePropagationStopped = this.isPropagationStopped = this.isDefaultPrevented = Ma, e != null && e.preventDefault ? (this.type = e.type, this.isDefaultPrevented = e.defaultPrevented ? $i : Ma) : e != null && e.type ? r = e : this.type = e, r != null && (this.originalEvent = r.originalEvent, this.type = r.type != null ? r.type : this.type, this.cy = r.cy, this.target = r.target, this.position = r.position, this.renderedPosition = r.renderedPosition, this.namespace = r.namespace, this.layout = r.layout), this.cy != null && this.position != null && this.renderedPosition == null) {
      var n = this.position, a = this.cy.zoom(), i = this.cy.pan();
      this.renderedPosition = {
        x: n.x * a + i.x,
        y: n.y * a + i.y
      };
    }
    this.timeStamp = e && e.timeStamp || Date.now();
  },
  preventDefault: function() {
    this.isDefaultPrevented = $i;
    var e = this.originalEvent;
    e && e.preventDefault && e.preventDefault();
  },
  stopPropagation: function() {
    this.isPropagationStopped = $i;
    var e = this.originalEvent;
    e && e.stopPropagation && e.stopPropagation();
  },
  stopImmediatePropagation: function() {
    this.isImmediatePropagationStopped = $i, this.stopPropagation();
  },
  isDefaultPrevented: Ma,
  isPropagationStopped: Ma,
  isImmediatePropagationStopped: Ma
};
var Ng = /^([^.]+)(\.(?:[^.]+))?$/, Tx = ".*", Fg = {
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
}, Gv = Object.keys(Fg), Sx = {};
function ro() {
  for (var t = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : Sx, e = arguments.length > 1 ? arguments[1] : void 0, r = 0; r < Gv.length; r++) {
    var n = Gv[r];
    this[n] = t[n] || Fg[n];
  }
  this.context = e || this.context, this.listeners = [], this.emitting = 0;
}
var cn = ro.prototype, zg = function(e, r, n, a, i, s, o) {
  tt(a) && (i = a, a = null), o && (s == null ? s = o : s = Ae({}, s, o));
  for (var l = Ke(n) ? n : n.split(/\s+/), u = 0; u < l.length; u++) {
    var f = l[u];
    if (!sn(f)) {
      var c = f.match(Ng);
      if (c) {
        var v = c[1], d = c[2] ? c[2] : null, h = r(e, f, v, d, a, i, s);
        if (h === !1)
          break;
      }
    }
  }
}, Wv = function(e, r) {
  return e.addEventFields(e.context, r), new _g(r.type, r);
}, Px = function(e, r, n) {
  if (R0(n)) {
    r(e, n);
    return;
  } else if (Fe(n)) {
    r(e, Wv(e, n));
    return;
  }
  for (var a = Ke(n) ? n : n.split(/\s+/), i = 0; i < a.length; i++) {
    var s = a[i];
    if (!sn(s)) {
      var o = s.match(Ng);
      if (o) {
        var l = o[1], u = o[2] ? o[2] : null, f = Wv(e, {
          type: l,
          namespace: u,
          target: e.context
        });
        r(e, f);
      }
    }
  }
};
cn.on = cn.addListener = function(t, e, r, n, a) {
  return zg(this, function(i, s, o, l, u, f, c) {
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
cn.one = function(t, e, r, n) {
  return this.on(t, e, r, n, {
    one: !0
  });
};
cn.removeListener = cn.off = function(t, e, r, n) {
  var a = this;
  this.emitting !== 0 && (this.listeners = db(this.listeners));
  for (var i = this.listeners, s = function(u) {
    var f = i[u];
    zg(a, function(c, v, d, h, y, g) {
      if ((f.type === d || t === "*") && (!h && f.namespace !== ".*" || f.namespace === h) && (!y || c.qualifierCompare(f.qualifier, y)) && (!g || f.callback === g))
        return i.splice(u, 1), !1;
    }, t, e, r, n);
  }, o = i.length - 1; o >= 0; o--)
    s(o);
  return this;
};
cn.removeAllListeners = function() {
  return this.removeListener("*");
};
cn.emit = cn.trigger = function(t, e, r) {
  var n = this.listeners, a = n.length;
  return this.emitting++, Ke(e) || (e = [e]), Px(this, function(i, s) {
    r != null && (n = [{
      event: s.event,
      type: s.type,
      namespace: s.namespace,
      callback: r
    }], a = n.length);
    for (var o = function() {
      var f = n[l];
      if (f.type === s.type && (!f.namespace || f.namespace === s.namespace || f.namespace === Tx) && i.eventMatches(i.context, f, s)) {
        var c = [s];
        e != null && gb(c, e), i.beforeEmit(i.context, f, s), f.conf && f.conf.one && (i.listeners = i.listeners.filter(function(h) {
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
var Dx = {
  qualifierCompare: function(e, r) {
    return e == null || r == null ? e == null && r == null : e.sameText(r);
  },
  eventMatches: function(e, r, n) {
    var a = r.qualifier;
    return a != null ? e !== n.target && mi(n.target) && a.matches(n.target) : !0;
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
}, Hi = function(e) {
  return Se(e) ? new un(e) : e;
}, Vg = {
  createEmitter: function() {
    for (var e = 0; e < this.length; e++) {
      var r = this[e], n = r._private;
      n.emitter || (n.emitter = new ro(Dx, r));
    }
    return this;
  },
  emitter: function() {
    return this._private.emitter;
  },
  on: function(e, r, n) {
    for (var a = Hi(r), i = 0; i < this.length; i++) {
      var s = this[i];
      s.emitter().on(e, a, n);
    }
    return this;
  },
  removeListener: function(e, r, n) {
    for (var a = Hi(r), i = 0; i < this.length; i++) {
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
    for (var a = Hi(r), i = 0; i < this.length; i++) {
      var s = this[i];
      s.emitter().one(e, a, n);
    }
    return this;
  },
  once: function(e, r, n) {
    for (var a = Hi(r), i = 0; i < this.length; i++) {
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
$e.eventAliasesOn(Vg);
var qg = {
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
    if (Se(e) || Qt(e))
      return new un(e).filter(this);
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
}, ze = qg;
ze.u = ze["|"] = ze["+"] = ze.union = ze.or = ze.add;
ze["\\"] = ze["!"] = ze["-"] = ze.difference = ze.relativeComplement = ze.subtract = ze.not;
ze.n = ze["&"] = ze["."] = ze.and = ze.intersection = ze.intersect;
ze["^"] = ze["(+)"] = ze["(-)"] = ze.symmetricDifference = ze.symdiff = ze.xor;
ze.fnFilter = ze.filterFn = ze.stdFilter = ze.filter;
ze.complement = ze.abscomp = ze.absoluteComplement;
var Ax = {
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
}, $g = function(e, r) {
  var n = e.cy(), a = n.hasCompoundNodes();
  function i(f) {
    var c = f.pstyle("z-compound-depth");
    return c.value === "auto" ? a ? f.zDepth() : 0 : c.value === "bottom" ? -1 : c.value === "top" ? ju : 0;
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
}, Ps = {
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
    return this.sort($g);
  },
  zDepth: function() {
    var e = this[0];
    if (e) {
      var r = e._private, n = r.group;
      if (n === "nodes") {
        var a = r.data.parent ? e.parents().size() : 0;
        return e.isParent() ? a : ju - 1;
      } else {
        var i = r.source, s = r.target, o = i.zDepth(), l = s.zDepth();
        return Math.max(o, l, 0);
      }
    }
  }
};
Ps.each = Ps.forEach;
var kx = function() {
  var e = "undefined", r = (typeof Symbol > "u" ? "undefined" : gt(Symbol)) != e && gt(Symbol.iterator) != e;
  r && (Ps[Symbol.iterator] = function() {
    var n = this, a = {
      value: void 0,
      done: !1
    }, i = 0, s = this.length;
    return Vh({
      next: function() {
        return i < s ? a.value = n[i++] : (a.value = void 0, a.done = !0), a;
      }
    }, Symbol.iterator, function() {
      return this;
    });
  });
};
kx();
var Bx = Dt({
  nodeDimensionsIncludeLabels: !1
}), ns = {
  // Calculates and returns node dimensions { x, y } based on options given
  layoutDimensions: function(e) {
    e = Bx(e);
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
    }, l = da(n, o);
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
      for (var T = qt(), x = 0; x < a.length; x++) {
        var S = a[x], D = l(S, x);
        sg(T, D.x, D.y);
      }
      return T;
    }, v = c(), d = da(function(E, T) {
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
      }), ba.all(e.animations.map(function(E) {
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
ns.createLayout = ns.makeLayout = ns.layout;
function Hg(t, e, r) {
  var n = r._private, a = n.styleCache = n.styleCache || [], i;
  return (i = a[t]) != null || (i = a[t] = e(r)), i;
}
function no(t, e) {
  return t = Mn(t), function(n) {
    return Hg(t, e, n);
  };
}
function ao(t, e) {
  t = Mn(t);
  var r = function(a) {
    return e.call(a);
  };
  return function() {
    var a = this[0];
    if (a)
      return Hg(t, r, a);
  };
}
var Tt = {
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
    if (Fe(e)) {
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
function Ul(t, e) {
  var r = t._private, n = r.data.parent ? t.parents() : null;
  if (n)
    for (var a = 0; a < n.length; a++) {
      var i = n[a];
      if (!e(i))
        return !1;
    }
  return !0;
}
function cf(t) {
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
        return !s || Ul(i, n);
      var l = o.source, u = o.target;
      return r(l) && (!s || Ul(l, r)) && (l === u || r(u) && (!s || Ul(u, r)));
    }
  };
}
var wa = no("eleTakesUpSpace", function(t) {
  return t.pstyle("display").value === "element" && t.width() !== 0 && (t.isNode() ? t.height() !== 0 : !0);
});
Tt.takesUpSpace = ao("takesUpSpace", cf({
  ok: wa
}));
var Rx = no("eleInteractive", function(t) {
  return t.pstyle("events").value === "yes" && t.pstyle("visibility").value === "visible" && wa(t);
}), Mx = no("parentInteractive", function(t) {
  return t.pstyle("visibility").value === "visible" && wa(t);
});
Tt.interactive = ao("interactive", cf({
  ok: Rx,
  parentOk: Mx,
  edgeOkViaNode: wa
}));
Tt.noninteractive = function() {
  var t = this[0];
  if (t)
    return !t.interactive();
};
var Lx = no("eleVisible", function(t) {
  return t.pstyle("visibility").value === "visible" && t.pstyle("opacity").pfValue !== 0 && wa(t);
}), Ix = wa;
Tt.visible = ao("visible", cf({
  ok: Lx,
  edgeOkViaNode: Ix
}));
Tt.hidden = function() {
  var t = this[0];
  if (t)
    return !t.visible();
};
Tt.isBundledBezier = ao("isBundledBezier", function() {
  return this.cy().styleEnabled() ? !this.removed() && this.pstyle("curve-style").value === "bezier" && this.takesUpSpace() : !1;
});
Tt.bypass = Tt.css = Tt.style;
Tt.renderedCss = Tt.renderedStyle;
Tt.removeBypass = Tt.removeCss = Tt.removeStyle;
Tt.pstyle = Tt.parsedStyle;
var nn = {};
function Kv(t) {
  return function() {
    var e = arguments, r = [];
    if (e.length === 2) {
      var n = e[0], a = e[1];
      this.on(t.event, n, a);
    } else if (e.length === 1 && tt(e[0])) {
      var i = e[0];
      this.on(t.event, i);
    } else if (e.length === 0 || e.length === 1 && Ke(e[0])) {
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
function xa(t) {
  nn[t.field] = function() {
    var e = this[0];
    if (e) {
      if (t.overrideField) {
        var r = t.overrideField(e);
        if (r !== void 0)
          return r;
      }
      return e._private[t.field];
    }
  }, nn[t.on] = Kv({
    event: t.on,
    field: t.field,
    ableField: t.ableField,
    overrideAble: t.overrideAble,
    value: !0
  }), nn[t.off] = Kv({
    event: t.off,
    field: t.field,
    ableField: t.ableField,
    overrideAble: t.overrideAble,
    value: !1
  });
}
xa({
  field: "locked",
  overrideField: function(e) {
    return e.cy().autolock() ? !0 : void 0;
  },
  on: "lock",
  off: "unlock"
});
xa({
  field: "grabbable",
  overrideField: function(e) {
    return e.cy().autoungrabify() || e.pannable() ? !1 : void 0;
  },
  on: "grabify",
  off: "ungrabify"
});
xa({
  field: "selected",
  ableField: "selectable",
  overrideAble: function(e) {
    return e.cy().autounselectify() ? !1 : void 0;
  },
  on: "select",
  off: "unselect"
});
xa({
  field: "selectable",
  overrideField: function(e) {
    return e.cy().autounselectify() ? !1 : void 0;
  },
  on: "selectify",
  off: "unselectify"
});
nn.deselect = nn.unselect;
nn.grabbed = function() {
  var t = this[0];
  if (t)
    return t._private.grabbed;
};
xa({
  field: "active",
  on: "activate",
  off: "unactivate"
});
xa({
  field: "pannable",
  on: "panify",
  off: "unpanify"
});
nn.inactive = function() {
  var t = this[0];
  if (t)
    return !t._private.active;
};
var Lt = {}, Yv = function(e) {
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
}, Xv = function(e) {
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
}, Zv = function(e) {
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
Lt.clearTraversalCache = function() {
  for (var t = 0; t < this.length; t++)
    this[t]._private.traversalCache = null;
};
Ae(Lt, {
  // get the root nodes in the DAG
  roots: Yv({
    noIncomingEdges: !0
  }),
  // get the leaf nodes in the DAG
  leaves: Yv({
    noOutgoingEdges: !0
  }),
  // normally called children in graph theory
  // these nodes =edges=> outgoing nodes
  outgoers: tr(Xv({
    outgoing: !0
  }), "outgoers"),
  // aka DAG descendants
  successors: Zv({
    outgoing: !0
  }),
  // normally called parents in graph theory
  // these nodes <=edges= incoming nodes
  incomers: tr(Xv({
    incoming: !0
  }), "incomers"),
  // aka DAG ancestors
  predecessors: Zv({})
});
Ae(Lt, {
  neighborhood: tr(function(t) {
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
Lt.neighbourhood = Lt.neighborhood;
Lt.closedNeighbourhood = Lt.closedNeighborhood;
Lt.openNeighbourhood = Lt.openNeighborhood;
Ae(Lt, {
  source: tr(function(e) {
    var r = this[0], n;
    return r && (n = r._private.source || r.cy().collection()), n && e ? n.filter(e) : n;
  }, "source"),
  target: tr(function(e) {
    var r = this[0], n;
    return r && (n = r._private.target || r.cy().collection()), n && e ? n.filter(e) : n;
  }, "target"),
  sources: Qv({
    attr: "source"
  }),
  targets: Qv({
    attr: "target"
  })
});
function Qv(t) {
  return function(r) {
    for (var n = [], a = 0; a < this.length; a++) {
      var i = this[a], s = i._private[t.attr];
      s && n.push(s);
    }
    return this.spawn(n, !0).filter(r);
  };
}
Ae(Lt, {
  edgesWith: tr(jv(), "edgesWith"),
  edgesTo: tr(jv({
    thisIsSrc: !0
  }), "edgesTo")
});
function jv(t) {
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
Ae(Lt, {
  connectedEdges: tr(function(t) {
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
  connectedNodes: tr(function(t) {
    for (var e = [], r = this, n = 0; n < r.length; n++) {
      var a = r[n];
      a.isEdge() && (e.push(a.source()[0]), e.push(a.target()[0]));
    }
    return this.spawn(e, !0).filter(t);
  }, "connectedNodes"),
  parallelEdges: tr(Jv(), "parallelEdges"),
  codirectedEdges: tr(Jv({
    codirected: !0
  }), "codirectedEdges")
});
function Jv(t) {
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
Ae(Lt, {
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
Lt.componentsOf = Lt.components;
var St = function(e, r) {
  var n = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : !1, a = arguments.length > 3 && arguments[3] !== void 0 ? arguments[3] : !1;
  if (e === void 0) {
    je("A collection must have a reference to the core");
    return;
  }
  var i = new Fr(), s = !1;
  if (!r)
    r = [];
  else if (r.length > 0 && Fe(r[0]) && !mi(r[0])) {
    s = !0;
    for (var o = [], l = new ma(), u = 0, f = r.length; u < f; u++) {
      var c = r[u];
      c.data == null && (c.data = {});
      var v = c.data;
      if (v.id == null)
        v.id = ng();
      else if (e.hasElementWithId(v.id) || l.has(v.id))
        continue;
      var d = new Ys(e, c, !1);
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
      for (var b = this.lazyMap = new Fr(), w = this.eles, E = 0; E < w.length; E++) {
        var T = w[E];
        b.set(T.id(), {
          index: E,
          ele: T
        });
      }
    }
  }, n && (this._private.map = i), s && !a && this.restore();
}, Ze = Ys.prototype = St.prototype = Object.create(Array.prototype);
Ze.instanceString = function() {
  return "collection";
};
Ze.spawn = function(t, e) {
  return new St(this.cy(), t, e);
};
Ze.spawnSelf = function() {
  return this.spawn(this);
};
Ze.cy = function() {
  return this._private.cy;
};
Ze.renderer = function() {
  return this._private.cy.renderer();
};
Ze.element = function() {
  return this[0];
};
Ze.collection = function() {
  return Hh(this) ? this : new St(this._private.cy, [this]);
};
Ze.unique = function() {
  return new St(this._private.cy, this, !0);
};
Ze.hasElementWithId = function(t) {
  return t = "" + t, this._private.map.has(t);
};
Ze.getElementById = function(t) {
  t = "" + t;
  var e = this._private.cy, r = this._private.map.get(t);
  return r ? r.ele : new St(e);
};
Ze.$id = Ze.getElementById;
Ze.poolIndex = function() {
  var t = this._private.cy, e = t._private.elements, r = this[0]._private.data.id;
  return e._private.map.get(r).index;
};
Ze.indexOf = function(t) {
  var e = t[0]._private.data.id;
  return this._private.map.get(e).index;
};
Ze.indexOfId = function(t) {
  return t = "" + t, this._private.map.get(t).index;
};
Ze.json = function(t) {
  var e = this.element(), r = this.cy();
  if (e == null && t)
    return this;
  if (e != null) {
    var n = e._private;
    if (Fe(t)) {
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
        data: Cr(n.data),
        position: Cr(n.position),
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
Ze.jsons = function() {
  for (var t = [], e = 0; e < this.length; e++) {
    var r = this[e], n = r.json();
    t.push(n);
  }
  return t;
};
Ze.clone = function() {
  for (var t = this.cy(), e = [], r = 0; r < this.length; r++) {
    var n = this[r], a = n.json(), i = new Ys(t, a, !1);
    e.push(i);
  }
  return new St(t, e);
};
Ze.copy = Ze.clone;
Ze.restore = function() {
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
        y.id = ng();
      else if (pe(y.id))
        y.id = "" + y.id;
      else if (sn(y.id) || !Se(y.id)) {
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
    h.map = new Fr(), h.map.set(g, {
      ele: d,
      index: 0
    }), h.removed = !1, e && n.addToPool(d);
  }
  for (var k = 0; k < i.length; k++) {
    var R = i[k], M = R._private.data;
    pe(M.parent) && (M.parent = "" + M.parent);
    var I = M.parent, _ = I != null;
    if (_ || R._private.parent) {
      var O = R._private.parent ? n.collection().merge(R._private.parent) : n.getElementById(I);
      if (O.empty())
        M.parent = void 0;
      else if (O[0].removed())
        He("Node added with missing parent, reference to parent removed"), M.parent = void 0, R._private.parent = null;
      else {
        for (var L = !1, N = O; !N.empty(); ) {
          if (R.same(N)) {
            L = !0, M.parent = void 0;
            break;
          }
          N = N.parent();
        }
        L || (O[0]._private.children.push(R), R._private.parent = O[0], a.hasCompoundNodes = !0);
      }
    }
  }
  if (o.length > 0) {
    for (var H = o.length === r.length ? r : new St(n, o), V = 0; V < H.length; V++) {
      var F = H[V];
      F.isNode() || (F.parallelEdges().clearTraversalCache(), F.source().clearTraversalCache(), F.target().clearTraversalCache());
    }
    var $;
    a.hasCompoundNodes ? $ = n.collection().merge(H).merge(H.connectedNodes()).merge(H.parent()) : $ = H, $.dirtyCompoundBoundsCache().dirtyBoundingBoxCache().updateStyle(t), t ? H.emitAndNotify("add") : e && H.emit("add");
  }
  return r;
};
Ze.removed = function() {
  var t = this[0];
  return t && t._private.removed;
};
Ze.inside = function() {
  var t = this[0];
  return t && !t._private.removed;
};
Ze.remove = function() {
  var t = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : !0, e = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : !0, r = this, n = [], a = {}, i = r._private.cy;
  function s(I) {
    for (var _ = I._private.edges, O = 0; O < _.length; O++)
      l(_[O]);
  }
  function o(I) {
    for (var _ = I._private.children, O = 0; O < _.length; O++)
      l(_[O]);
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
    var O = I._private.edges;
    on(O, _), I.clearTraversalCache();
  }
  function d(I) {
    I.clearTraversalCache();
  }
  var h = [];
  h.ids = {};
  function y(I, _) {
    _ = _[0], I = I[0];
    var O = I._private.children, L = I.id();
    on(O, _), _._private.parent = null, h.ids[L] || (h.ids[L] = !0, h.push(I));
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
  var k = new St(this.cy(), n);
  k.size() > 0 && (t ? k.emitAndNotify("remove") : e && k.emit("remove"));
  for (var R = 0; R < h.length; R++) {
    var M = h[R];
    (!e || !M.removed()) && M.updateStyle();
  }
  return k;
};
Ze.move = function(t) {
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
[gg, Uw, rs, rn, ga, sx, to, Cx, Vg, qg, Ax, Ps, ns, Tt, nn, Lt].forEach(function(t) {
  Ae(Ze, t);
});
var Ox = {
  add: function(e) {
    var r, n = this;
    if (Qt(e)) {
      var a = e;
      if (a._private.cy === n)
        r = a.restore();
      else {
        for (var i = [], s = 0; s < a.length; s++) {
          var o = a[s];
          i.push(o.json());
        }
        r = new St(n, i);
      }
    } else if (Ke(e)) {
      var l = e;
      r = new St(n, l);
    } else if (Fe(e) && (Ke(e.nodes) || Ke(e.edges))) {
      for (var u = e, f = [], c = ["nodes", "edges"], v = 0, d = c.length; v < d; v++) {
        var h = c[v], y = u[h];
        if (Ke(y))
          for (var g = 0, p = y.length; g < p; g++) {
            var m = Ae({
              group: h
            }, y[g]);
            f.push(m);
          }
      }
      r = new St(n, f);
    } else {
      var b = e;
      r = new Ys(n, b).collection();
    }
    return r;
  },
  remove: function(e) {
    if (!Qt(e)) {
      if (Se(e)) {
        var r = e;
        e = this.$(r);
      }
    }
    return e.remove();
  }
};
/*! Bezier curve function generator. Copyright Gaetan Renaudeau. MIT License: http://en.wikipedia.org/wiki/MIT_License */
function _x(t, e, r, n) {
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
    var I = (A - v[R]) / (v[R + 1] - v[R]), _ = k + I * u, O = p(_, t, r);
    return O >= i ? m(A, _) : O === 0 ? _ : w(A, k, k + u);
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
var Nx = /* @__PURE__ */ (function() {
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
})(), Ye = function(e, r, n, a) {
  var i = _x(e, r, n, a);
  return function(s, o, l) {
    return s + (o - s) * i(l);
  };
}, as = {
  linear: function(e, r, n) {
    return e + (r - e) * n;
  },
  // default easings
  ease: Ye(0.25, 0.1, 0.25, 1),
  "ease-in": Ye(0.42, 0, 1, 1),
  "ease-out": Ye(0, 0, 0.58, 1),
  "ease-in-out": Ye(0.42, 0, 0.58, 1),
  // sine
  "ease-in-sine": Ye(0.47, 0, 0.745, 0.715),
  "ease-out-sine": Ye(0.39, 0.575, 0.565, 1),
  "ease-in-out-sine": Ye(0.445, 0.05, 0.55, 0.95),
  // quad
  "ease-in-quad": Ye(0.55, 0.085, 0.68, 0.53),
  "ease-out-quad": Ye(0.25, 0.46, 0.45, 0.94),
  "ease-in-out-quad": Ye(0.455, 0.03, 0.515, 0.955),
  // cubic
  "ease-in-cubic": Ye(0.55, 0.055, 0.675, 0.19),
  "ease-out-cubic": Ye(0.215, 0.61, 0.355, 1),
  "ease-in-out-cubic": Ye(0.645, 0.045, 0.355, 1),
  // quart
  "ease-in-quart": Ye(0.895, 0.03, 0.685, 0.22),
  "ease-out-quart": Ye(0.165, 0.84, 0.44, 1),
  "ease-in-out-quart": Ye(0.77, 0, 0.175, 1),
  // quint
  "ease-in-quint": Ye(0.755, 0.05, 0.855, 0.06),
  "ease-out-quint": Ye(0.23, 1, 0.32, 1),
  "ease-in-out-quint": Ye(0.86, 0, 0.07, 1),
  // expo
  "ease-in-expo": Ye(0.95, 0.05, 0.795, 0.035),
  "ease-out-expo": Ye(0.19, 1, 0.22, 1),
  "ease-in-out-expo": Ye(1, 0, 0, 1),
  // circ
  "ease-in-circ": Ye(0.6, 0.04, 0.98, 0.335),
  "ease-out-circ": Ye(0.075, 0.82, 0.165, 1),
  "ease-in-out-circ": Ye(0.785, 0.135, 0.15, 0.86),
  // user param easings...
  spring: function(e, r, n) {
    if (n === 0)
      return as.linear;
    var a = Nx(e, r, n);
    return function(i, s, o) {
      return i + (s - i) * a(o);
    };
  },
  "cubic-bezier": Ye
};
function ed(t, e, r, n, a) {
  if (n === 1 || e === r)
    return r;
  var i = a(e, r, n);
  return t == null || ((t.roundValue || t.color) && (i = Math.round(i)), t.min !== void 0 && (i = Math.max(i, t.min)), t.max !== void 0 && (i = Math.min(i, t.max))), i;
}
function td(t, e) {
  return t.pfValue != null || t.value != null ? t.pfValue != null && (e == null || e.type.units !== "%") ? t.pfValue : t.value : t;
}
function Un(t, e, r, n, a) {
  var i = a != null ? a.type : null;
  r < 0 ? r = 0 : r > 1 && (r = 1);
  var s = td(t, a), o = td(e, a);
  if (pe(s) && pe(o))
    return ed(i, s, o, r, n);
  if (Ke(s) && Ke(o)) {
    for (var l = [], u = 0; u < o.length; u++) {
      var f = s[u], c = o[u];
      if (f != null && c != null) {
        var v = ed(i, f, c, r, n);
        l.push(v);
      } else
        l.push(c);
    }
    return l;
  }
}
function Fx(t, e, r, n) {
  var a = !n, i = t._private, s = e._private, o = s.easing, l = s.startTime, u = n ? t : t.cy(), f = u.style();
  if (!s.easingImpl)
    if (o == null)
      s.easingImpl = as.linear;
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
      })), h.length > 0 ? (d === "spring" && h.push(s.duration), s.easingImpl = as[d].apply(null, h)) : s.easingImpl = as[d];
    }
  var y = s.easingImpl, g;
  if (s.duration === 0 ? g = 1 : g = (r - l) / s.duration, s.applying && (g = s.progress), g < 0 ? g = 0 : g > 1 && (g = 1), s.delay == null) {
    var p = s.startPosition, m = s.position;
    if (m && a && !t.locked()) {
      var b = {};
      La(p.x, m.x) && (b.x = Un(p.x, m.x, g, y)), La(p.y, m.y) && (b.y = Un(p.y, m.y, g, y)), t.position(b);
    }
    var w = s.startPan, E = s.pan, T = i.pan, x = E != null && n;
    x && (La(w.x, E.x) && (T.x = Un(w.x, E.x, g, y)), La(w.y, E.y) && (T.y = Un(w.y, E.y, g, y)), t.emit("pan"));
    var S = s.startZoom, D = s.zoom, A = D != null && n;
    A && (La(S, D) && (i.zoom = si(i.minZoom, Un(S, D, g, y), i.maxZoom)), t.emit("zoom")), (x || A) && t.emit("viewport");
    var k = s.style;
    if (k && k.length > 0 && a) {
      for (var R = 0; R < k.length; R++) {
        var M = k[R], I = M.name, _ = M, O = s.startStyle[I], L = f.properties[O.name], N = Un(O, _, g, y, L);
        f.overrideBypass(t, I, N);
      }
      t.emit("style");
    }
  }
  return s.progress = g, g;
}
function La(t, e) {
  return t == null || e == null ? !1 : pe(t) && pe(e) ? !0 : !!(t && e);
}
function zx(t, e, r, n) {
  var a = e._private;
  a.started = !0, a.startTime = r - a.progress * a.duration;
}
function rd(t, e) {
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
      !w.playing && !w.applying || (w.playing && w.applying && (w.applying = !1), w.started || zx(f, b, t), Fx(f, b, t, c), w.applying && (w.applying = !1), p(w.frames), w.step != null && w.step(t), b.completed() && (d.splice(m, 1), w.hooked = !1, w.playing = !1, w.started = !1, p(w.completes)), y = !0);
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
var Vx = {
  // pull in animation functions
  animate: $e.animate(),
  animation: $e.animation(),
  animated: $e.animated(),
  clearQueue: $e.clearQueue(),
  delay: $e.delay(),
  delayAnimation: $e.delayAnimation(),
  stop: $e.stop(),
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
      e._private.animationsRunning && xs(function(i) {
        rd(i, e), r();
      });
    }
    var n = e.renderer();
    n && n.beforeRender ? n.beforeRender(function(i, s) {
      rd(s, e);
    }, n.beforeRenderPriorities.animations) : r();
  }
}, qx = {
  qualifierCompare: function(e, r) {
    return e == null || r == null ? e == null && r == null : e.sameText(r);
  },
  eventMatches: function(e, r, n) {
    var a = r.qualifier;
    return a != null ? e !== n.target && mi(n.target) && a.matches(n.target) : !0;
  },
  addEventFields: function(e, r) {
    r.cy = e, r.target = e;
  },
  callbackContext: function(e, r, n) {
    return r.qualifier != null ? n.target : e;
  }
}, Ui = function(e) {
  return Se(e) ? new un(e) : e;
}, Ug = {
  createEmitter: function() {
    var e = this._private;
    return e.emitter || (e.emitter = new ro(qx, this)), this;
  },
  emitter: function() {
    return this._private.emitter;
  },
  on: function(e, r, n) {
    return this.emitter().on(e, Ui(r), n), this;
  },
  removeListener: function(e, r, n) {
    return this.emitter().removeListener(e, Ui(r), n), this;
  },
  removeAllListeners: function() {
    return this.emitter().removeAllListeners(), this;
  },
  one: function(e, r, n) {
    return this.emitter().one(e, Ui(r), n), this;
  },
  once: function(e, r, n) {
    return this.emitter().one(e, Ui(r), n), this;
  },
  emit: function(e, r) {
    return this.emitter().emit(e, r), this;
  },
  emitAndNotify: function(e, r) {
    return this.emit(e), this.notify(e, r), this;
  }
};
$e.eventAliasesOn(Ug);
var xu = {
  png: function(e) {
    var r = this._private.renderer;
    return e = e || {}, r.png(e);
  },
  jpg: function(e) {
    var r = this._private.renderer;
    return e = e || {}, e.bg = e.bg || "#fff", r.jpg(e);
  }
};
xu.jpeg = xu.jpg;
var is = {
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
is.createLayout = is.makeLayout = is.layout;
var $x = {
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
}, Hx = Dt({
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
}), Eu = {
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
    e.wheelSensitivity !== void 0 && He("You have set a custom wheel sensitivity.  This will make your app zoom unnaturally when using mainstream mice.  You should change this value from the default only if you can guarantee that all your users will use the same hardware and OS configuration as your current machine.");
    var a = Hx(e);
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
Eu.invalidateDimensions = Eu.resize;
var ss = {
  // get a collection
  // - empty collection on no args
  // - collection of elements in the graph on selector arg
  // - guarantee a returned collection when elements or collection specified
  collection: function(e, r) {
    return Se(e) ? this.$(e) : Qt(e) ? e.collection() : Ke(e) ? (r || (r = {}), new St(this, e, r.unique, r.removed)) : new St(this);
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
ss.elements = ss.filter = ss.$;
var xt = {}, Ya = "t", Ux = "f";
xt.apply = function(t) {
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
xt.getPropertiesDiff = function(t, e) {
  var r = this, n = r._private.propDiffs = r._private.propDiffs || {}, a = t + "-" + e, i = n[a];
  if (i)
    return i;
  for (var s = [], o = {}, l = 0; l < r.length; l++) {
    var u = r[l], f = t[l] === Ya, c = e[l] === Ya, v = f !== c, d = u.mappedProperties.length > 0;
    if (v || c && d) {
      var h = void 0;
      v && d || v ? h = u.properties : d && (h = u.mappedProperties);
      for (var y = 0; y < h.length; y++) {
        for (var g = h[y], p = g.name, m = !1, b = l + 1; b < r.length; b++) {
          var w = r[b], E = e[b] === Ya;
          if (E && (m = w.properties[g.name] != null, m))
            break;
        }
        !o[p] && !m && (o[p] = !0, s.push(p));
      }
    }
  }
  return n[a] = s, s;
};
xt.getContextMeta = function(t) {
  for (var e = this, r = "", n, a = t._private.styleCxtKey || "", i = 0; i < e.length; i++) {
    var s = e[i], o = s.selector && s.selector.matches(t);
    o ? r += Ya : r += Ux;
  }
  return n = e.getPropertiesDiff(a, r), t._private.styleCxtKey = r, {
    key: r,
    diffPropNames: n,
    empty: n.length === 0
  };
};
xt.getContextStyle = function(t) {
  var e = t.key, r = this, n = this._private.contextStyles = this._private.contextStyles || {};
  if (n[e])
    return n[e];
  for (var a = {
    _private: {
      key: e
    }
  }, i = 0; i < r.length; i++) {
    var s = r[i], o = e[i] === Ya;
    if (o)
      for (var l = 0; l < s.properties.length; l++) {
        var u = s.properties[l];
        a[u.name] = u;
      }
  }
  return n[e] = a, a;
};
xt.applyContextStyle = function(t, e, r) {
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
xt.updateStyleHints = function(t) {
  var e = t._private, r = this, n = r.propertyGroupNames, a = r.propertyGroupKeys, i = function(C, B, z) {
    return r.getPropertiesHash(C, B, z);
  }, s = e.styleKey;
  if (t.removed())
    return !1;
  var o = e.group === "nodes", l = t._private.style;
  n = Object.keys(l);
  for (var u = 0; u < a.length; u++) {
    var f = a[u];
    e.styleKeys[f] = [Pn, Qn];
  }
  for (var c = function(C, B) {
    return e.styleKeys[B][0] = ni(C, e.styleKeys[B][0]);
  }, v = function(C, B) {
    return e.styleKeys[B][1] = ai(C, e.styleKeys[B][1]);
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
  for (var I = [Pn, Qn], _ = 0; _ < a.length; _++) {
    var O = a[_], L = e.styleKeys[O];
    I[0] = ni(L[0], I[0]), I[1] = ai(L[1], I[1]);
  }
  e.styleKey = ib(I[0], I[1]);
  var N = e.styleKeys;
  e.labelDimsKey = Xr(N.labelDimensions);
  var H = i(t, ["label"], N.labelDimensions);
  if (e.labelKey = Xr(H), e.labelStyleKey = Xr(_i(N.commonLabel, H)), !o) {
    var V = i(t, ["source-label"], N.labelDimensions);
    e.sourceLabelKey = Xr(V), e.sourceLabelStyleKey = Xr(_i(N.commonLabel, V));
    var F = i(t, ["target-label"], N.labelDimensions);
    e.targetLabelKey = Xr(F), e.targetLabelStyleKey = Xr(_i(N.commonLabel, F));
  }
  if (o) {
    var $ = e.styleKeys, Q = $.nodeBody, se = $.nodeBorder, ae = $.nodeOutline, le = $.backgroundImage, ce = $.compound, he = $.pie, ie = $.stripe, U = [Q, se, ae, le, ce, he, ie].filter(function(X) {
      return X != null;
    }).reduce(_i, [Pn, Qn]);
    e.nodeKey = Xr(U), e.hasPie = he != null && he[0] !== Pn && he[1] !== Qn, e.hasStripe = ie != null && ie[0] !== Pn && ie[1] !== Qn;
  }
  return s !== e.styleKey;
};
xt.clearStyleHints = function(t) {
  var e = t._private;
  e.styleCxtKey = "", e.styleKeys = {}, e.styleKey = null, e.labelKey = null, e.labelStyleKey = null, e.sourceLabelKey = null, e.sourceLabelStyleKey = null, e.targetLabelKey = null, e.targetLabelStyleKey = null, e.nodeKey = null, e.hasPie = null, e.hasStripe = null;
};
xt.applyParsedProperty = function(t, e) {
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
    He("Do not assign mappings to elements without corresponding data (i.e. ele `" + t.id() + "` has no mapping for property `" + n.name + "` with data field `" + n.field + "`); try a `[" + n.field + "]` selector to limit scope to elements with `" + n.field + "` defined");
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
        return He("Do not use continuous mappers without specifying numeric data (i.e. `" + n.field + ": " + p + "` for `" + t.id() + "` is non-numeric)"), !1;
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
      for (var O = n.field.split("."), L = c.data, N = 0; N < O.length && L; N++) {
        var H = O[N];
        L = L[H];
      }
      if (L != null && (i = this.parse(n.name, L, n.bypass, v)), !i)
        return y(), !1;
      i.mapping = n, n = i;
      break;
    }
    case s.fn: {
      var V = n.value, F = n.fnValue != null ? n.fnValue : V(t);
      if (n.prevFnValue = F, F == null)
        return He("Custom function mappers may not return null (i.e. `" + n.name + "` for ele `" + t.id() + "` is null)"), !1;
      if (i = this.parse(n.name, F, n.bypass, v), !i)
        return He("Custom function mappers may not return invalid values for the property type (i.e. `" + n.name + "` for ele `" + t.id() + "` is invalid)"), !1;
      i.mapping = Cr(n), n = i;
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
xt.cleanElements = function(t, e) {
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
xt.update = function() {
  var t = this._private.cy, e = t.mutableElements();
  e.updateStyle();
};
xt.updateTransitions = function(t, e) {
  var r = this, n = t._private, a = t.pstyle("transition-property").value, i = t.pstyle("transition-duration").pfValue, s = t.pstyle("transition-delay").pfValue;
  if (a.length > 0 && i > 0) {
    for (var o = {}, l = !1, u = 0; u < a.length; u++) {
      var f = a[u], c = t.pstyle(f), v = e[f];
      if (v) {
        var d = v.prev, h = d, y = v.next != null ? v.next : c, g = !1, p = void 0, m = 1e-6;
        h && (pe(h.pfValue) && pe(y.pfValue) ? (g = y.pfValue - h.pfValue, p = h.pfValue + m * g) : pe(h.value) && pe(y.value) ? (g = y.value - h.value, p = h.value + m * g) : Ke(h.value) && Ke(y.value) && (g = h.value[0] !== y.value[0] || h.value[1] !== y.value[1] || h.value[2] !== y.value[2], p = h.strValue), g && (o[f] = y.strValue, this.applyBypass(t, f, p), l = !0));
      }
    }
    if (!l)
      return;
    n.transitioning = !0, new ba(function(b) {
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
xt.checkTrigger = function(t, e, r, n, a, i) {
  var s = this.properties[e], o = a(s);
  t.removed() || o != null && o(r, n, t) && i(s);
};
xt.checkZOrderTrigger = function(t, e, r, n) {
  var a = this;
  this.checkTrigger(t, e, r, n, function(i) {
    return i.triggersZOrder;
  }, function() {
    a._private.cy.notify("zorder", t);
  });
};
xt.checkBoundsTrigger = function(t, e, r, n) {
  this.checkTrigger(t, e, r, n, function(a) {
    return a.triggersBounds;
  }, function(a) {
    t.dirtyCompoundBoundsCache(), t.dirtyBoundingBoxCache();
  });
};
xt.checkConnectedEdgesBoundsTrigger = function(t, e, r, n) {
  this.checkTrigger(t, e, r, n, function(a) {
    return a.triggersBoundsOfConnectedEdges;
  }, function(a) {
    t.connectedEdges().forEach(function(i) {
      i.dirtyBoundingBoxCache();
    });
  });
};
xt.checkParallelEdgesBoundsTrigger = function(t, e, r, n) {
  this.checkTrigger(t, e, r, n, function(a) {
    return a.triggersBoundsOfParallelEdges;
  }, function(a) {
    t.parallelEdges().forEach(function(i) {
      i.dirtyBoundingBoxCache();
    });
  });
};
xt.checkTriggers = function(t, e, r, n) {
  t.dirtyStyleCache(), this.checkZOrderTrigger(t, e, r, n), this.checkBoundsTrigger(t, e, r, n), this.checkConnectedEdgesBoundsTrigger(t, e, r, n), this.checkParallelEdgesBoundsTrigger(t, e, r, n);
};
var Si = {};
Si.applyBypass = function(t, e, r, n) {
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
  } else if (Fe(e)) {
    var v = e;
    n = r;
    for (var d = Object.keys(v), h = 0; h < d.length; h++) {
      var y = d[h], g = v[y];
      if (g === void 0 && (g = v[Ws(y)]), g !== void 0) {
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
      m = this.applyParsedProperty(w, Cr(S)) || m, n && (T.next = w.pstyle(S.name));
    }
    m && this.updateStyleHints(w), n && this.updateTransitions(w, E, s);
  }
  return m;
};
Si.overrideBypass = function(t, e, r) {
  e = Zu(e);
  for (var n = 0; n < t.length; n++) {
    var a = t[n], i = a._private.style[e], s = this.properties[e].type, o = s.color, l = s.mutiple, u = i ? i.pfValue != null ? i.pfValue : i.value : null;
    !i || !i.bypass ? this.applyBypass(a, e, r) : (i.value = r, i.pfValue != null && (i.pfValue = r), o ? i.strValue = "rgb(" + r.join(",") + ")" : l ? i.strValue = r.join(" ") : i.strValue = "" + r, this.updateStyleHints(a)), this.checkTriggers(a, e, u, r);
  }
};
Si.removeAllBypasses = function(t, e) {
  return this.removeBypasses(t, this.propertyNames, e);
};
Si.removeBypasses = function(t, e, r) {
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
var vf = {};
vf.getEmSizeInPixels = function() {
  var t = this.containerCss("font-size");
  return t != null ? parseFloat(t) : 1;
};
vf.containerCss = function(t) {
  var e = this._private.cy, r = e.container(), n = e.window();
  if (n && r && n.getComputedStyle)
    return n.getComputedStyle(r).getPropertyValue(t);
};
var Pr = {};
Pr.getRenderedStyle = function(t, e) {
  return e ? this.getStylePropertyValue(t, e, !0) : this.getRawStyle(t, !0);
};
Pr.getRawStyle = function(t, e) {
  var r = this;
  if (t = t[0], t) {
    for (var n = {}, a = 0; a < r.properties.length; a++) {
      var i = r.properties[a], s = r.getStylePropertyValue(t, i.name, e);
      s != null && (n[i.name] = s, n[Ws(i.name)] = s);
    }
    return n;
  }
};
Pr.getIndexedStyle = function(t, e, r, n) {
  var a = t.pstyle(e)[r][n];
  return a ?? t.cy().style().getDefaultProperty(e)[r][0];
};
Pr.getStylePropertyValue = function(t, e, r) {
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
        }, d = Ke(o), h = d ? l.every(function(y) {
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
Pr.getAnimationStartStyle = function(t, e) {
  for (var r = {}, n = 0; n < e.length; n++) {
    var a = e[n], i = a.name, s = t.pstyle(i);
    s !== void 0 && (Fe(s) ? s = this.parse(i, s.strValue) : s = this.parse(i, s)), s && (r[i] = s);
  }
  return r;
};
Pr.getPropsList = function(t) {
  var e = this, r = [], n = t, a = e.properties;
  if (n)
    for (var i = Object.keys(n), s = 0; s < i.length; s++) {
      var o = i[s], l = n[o], u = a[o] || a[Zu(o)], f = this.parse(u.name, l);
      f && r.push(f);
    }
  return r;
};
Pr.getNonDefaultPropertiesHash = function(t, e, r) {
  var n = r.slice(), a, i, s, o, l, u;
  for (l = 0; l < e.length; l++)
    if (a = e[l], i = t.pstyle(a, !1), i != null)
      if (i.pfValue != null)
        n[0] = ni(o, n[0]), n[1] = ai(o, n[1]);
      else
        for (s = i.strValue, u = 0; u < s.length; u++)
          o = s.charCodeAt(u), n[0] = ni(o, n[0]), n[1] = ai(o, n[1]);
  return n;
};
Pr.getPropertiesHash = Pr.getNonDefaultPropertiesHash;
var io = {};
io.appendFromJson = function(t) {
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
io.fromJson = function(t) {
  var e = this;
  return e.resetToDefault(), e.appendFromJson(t), e;
};
io.json = function() {
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
var df = {};
df.appendFromString = function(t) {
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
      He("Halting stylesheet parsing: String stylesheet contains more to parse but no selector and block found in: " + n);
      break;
    }
    a = f[0];
    var c = f[1];
    if (c !== "core") {
      var v = new un(c);
      if (v.invalid) {
        He("Skipping parsing of block: Invalid selector found in string stylesheet: " + c), o();
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
        He("Skipping parsing of block: Invalid formatting of style property and value definitions found in:" + d), h = !0;
        break;
      }
      s = p[0];
      var m = p[1], b = p[2], w = e.properties[m];
      if (!w) {
        He("Skipping property: Invalid property name in: " + s), l();
        continue;
      }
      var E = r.parse(m, b);
      if (!E) {
        He("Skipping property: Invalid property definition in: " + s), l();
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
df.fromString = function(t) {
  var e = this;
  return e.resetToDefault(), e.appendFromString(t), e;
};
var ft = {};
(function() {
  var t = ht, e = N0, r = z0, n = V0, a = q0, i = function(X) {
    return "^" + X + "\\s*\\(\\s*([\\w\\.]+)\\s*\\)$";
  }, s = function(X) {
    var C = t + "|\\w+|" + e + "|" + r + "|" + n + "|" + a;
    return "^" + X + "\\s*\\(([\\w\\.]+)\\s*\\,\\s*(" + t + ")\\s*\\,\\s*(" + t + ")\\s*,\\s*(" + C + ")\\s*\\,\\s*(" + C + ")\\)$";
  }, o = [`^url\\s*\\(\\s*['"]?(.+?)['"]?\\s*\\)$`, "^(none)$", "^(.+)$"];
  ft.types = {
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
      var B = sn(X), z = sn(C);
      return B && !z || !B && z;
    }
  }, u = ft.types, f = [{
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
  ft.pieBackgroundN = 16, M.push({
    name: "pie-size",
    type: u.sizeMaybePercent
  }), M.push({
    name: "pie-hole",
    type: u.sizeMaybePercent
  }), M.push({
    name: "pie-start-angle",
    type: u.angle
  });
  for (var I = 1; I <= ft.pieBackgroundN; I++)
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
  ft.stripeBackgroundN = 16, _.push({
    name: "stripe-size",
    type: u.sizeMaybePercent
  }), _.push({
    name: "stripe-direction",
    type: u.axisDirectionPrimary
  });
  for (var O = 1; O <= ft.stripeBackgroundN; O++)
    _.push({
      name: "stripe-" + O + "-background-color",
      type: u.color
    }), _.push({
      name: "stripe-" + O + "-background-size",
      type: u.percent
    }), _.push({
      name: "stripe-" + O + "-background-opacity",
      type: u.zeroOneNumber
    });
  var L = [], N = ft.arrowPrefixes = ["source", "mid-source", "target", "mid-target"];
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
    N.forEach(function(X) {
      var C = X + "-" + U.name, B = U.type, z = U.triggersBounds;
      L.push({
        name: C,
        type: B,
        triggersBounds: z
      });
    });
  }, {});
  var H = ft.properties = [].concat(y, b, g, p, m, k, h, d, f, c, v, E, T, x, S, M, _, D, A, L, R), V = ft.propertyGroups = {
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
  }, F = ft.propertyGroupNames = {}, $ = ft.propertyGroupKeys = Object.keys(V);
  $.forEach(function(U) {
    F[U] = V[U].map(function(X) {
      return X.name;
    }), V[U].forEach(function(X) {
      return X.groupKey = U;
    });
  });
  var Q = ft.aliases = [{
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
  ft.propertyNames = H.map(function(U) {
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
ft.getDefaultProperty = function(t) {
  return this.getDefaultProperties()[t];
};
ft.getDefaultProperties = function() {
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
    for (var f = 1; f <= ft.pieBackgroundN; f++) {
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
    for (var f = 1; f <= ft.stripeBackgroundN; f++) {
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
    return ft.arrowPrefixes.forEach(function(f) {
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
ft.addDefaultStylesheet = function() {
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
var so = {};
so.parse = function(t, e, r, n) {
  var a = this;
  if (tt(e))
    return a.parseImplWarn(t, e, r, n);
  var i = n === "mapping" || n === !0 || n === !1 || n == null ? "dontcare" : n, s = r ? "t" : "f", o = "" + e, l = eg(t, o, s, i), u = a.propCache = a.propCache || [], f;
  return (f = u[l]) || (f = u[l] = a.parseImplWarn(t, e, r, n)), (r || n === "mapping") && (f = Cr(f), f && (f.value = Cr(f.value))), f;
};
so.parseImplWarn = function(t, e, r, n) {
  var a = this.parseImpl(t, e, r, n);
  return !a && e != null && He("The style property `".concat(t, ": ").concat(e, "` is invalid")), a && (a.name === "width" || a.name === "height") && e === "label" && He("The style value of `label` is deprecated for `" + a.name + "`"), a;
};
so.parseImpl = function(t, e, r, n) {
  var a = this;
  t = Zu(t);
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
        return He("`" + t + ": " + e + "` is not a valid mapper because the output range is zero; converting to `" + t + ": " + h.strValue + "`"), this.parse(t, h.strValue);
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
    if (l ? b = e.split(/\s+/) : Ke(e) ? b = e : b = [e], u.evenMultiple && b.length % 2 !== 0)
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
        var _ = e.match("^(" + ht + ")(" + I + ")?$");
        _ && (e = _[1], R = _[2] || M);
      } else (!R || u.implicitUnits) && (R = M);
    if (e = parseFloat(e), isNaN(e) && u.enums === void 0)
      return null;
    if (isNaN(e) && u.enums !== void 0)
      return e = s, k();
    if (u.integer && !B0(e) || u.min !== void 0 && (e < u.min || u.strictMin && e === u.min) || u.max !== void 0 && (e > u.max || u.strictMax && e === u.max))
      return null;
    var O = {
      name: t,
      value: e,
      strValue: "" + e + (R || ""),
      units: R,
      bypass: r
    };
    return u.unitless || R !== "px" && R !== "em" ? O.pfValue = e : O.pfValue = R === "px" || !R ? e : this.getEmSizeInPixels() * e, (R === "ms" || R === "s") && (O.pfValue = R === "ms" ? e : 1e3 * e), (R === "deg" || R === "rad") && (O.pfValue = R === "rad" ? e : Vb(e)), R === "%" && (O.pfValue = e / 100), O;
  } else if (u.propList) {
    var L = [], N = "" + e;
    if (N !== "none") {
      for (var H = N.split(/\s*,\s*|\s+/), V = 0; V < H.length; V++) {
        var F = H[V].trim();
        a.properties[F] ? L.push(F) : He("`" + F + "` is not a valid property name");
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
    var $ = Kh(e);
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
var wt = function(e) {
  if (!(this instanceof wt))
    return new wt(e);
  if (!Xu(e)) {
    je("A style must have a core reference");
    return;
  }
  this._private = {
    cy: e,
    coreStyle: {}
  }, this.length = 0, this.resetToDefault();
}, It = wt.prototype;
It.instanceString = function() {
  return "style";
};
It.clear = function() {
  for (var t = this._private, e = t.cy, r = e.elements(), n = 0; n < this.length; n++)
    this[n] = void 0;
  return this.length = 0, t.contextStyles = {}, t.propDiffs = {}, this.cleanElements(r, !0), r.forEach(function(a) {
    var i = a[0]._private;
    i.styleDirty = !0, i.appliedInitStyle = !1;
  }), this;
};
It.resetToDefault = function() {
  return this.clear(), this.addDefaultStylesheet(), this;
};
It.core = function(t) {
  return this._private.coreStyle[t] || this.getDefaultProperty(t);
};
It.selector = function(t) {
  var e = t === "core" ? null : new un(t), r = this.length++;
  return this[r] = {
    selector: e,
    properties: [],
    mappedProperties: [],
    index: r
  }, this;
};
It.css = function() {
  var t = this, e = arguments;
  if (e.length === 1)
    for (var r = e[0], n = 0; n < t.properties.length; n++) {
      var a = t.properties[n], i = r[a.name];
      i === void 0 && (i = r[Ws(a.name)]), i !== void 0 && this.cssRule(a.name, i);
    }
  else e.length === 2 && this.cssRule(e[0], e[1]);
  return this;
};
It.style = It.css;
It.cssRule = function(t, e) {
  var r = this.parse(t, e);
  if (r) {
    var n = this.length - 1;
    this[n].properties.push(r), this[n].properties[r.name] = r, r.name.match(/pie-(\d+)-background-size/) && r.value && (this._private.hasPie = !0), r.name.match(/stripe-(\d+)-background-size/) && r.value && (this._private.hasStripe = !0), r.mapped && this[n].mappedProperties.push(r);
    var a = !this[n].selector;
    a && (this._private.coreStyle[r.name] = r);
  }
  return this;
};
It.append = function(t) {
  return Uh(t) ? t.appendToStyle(this) : Ke(t) ? this.appendFromJson(t) : Se(t) && this.appendFromString(t), this;
};
wt.fromJson = function(t, e) {
  var r = new wt(t);
  return r.fromJson(e), r;
};
wt.fromString = function(t, e) {
  return new wt(t).fromString(e);
};
[xt, Si, vf, Pr, io, df, ft, so].forEach(function(t) {
  Ae(It, t);
});
wt.types = It.types;
wt.properties = It.properties;
wt.propertyGroups = It.propertyGroups;
wt.propertyGroupNames = It.propertyGroupNames;
wt.propertyGroupKeys = It.propertyGroupKeys;
var Gx = {
  style: function(e) {
    if (e) {
      var r = this.setStyle(e);
      r.update();
    }
    return this._private.style;
  },
  setStyle: function(e) {
    var r = this._private;
    return Uh(e) ? r.style = e.generateStyle(this) : Ke(e) ? r.style = wt.fromJson(this, e) : Se(e) ? r.style = wt.fromString(this, e) : r.style = wt(this), r.style;
  },
  // e.g. cy.data() changed => recalc ele mappers
  updateStyle: function() {
    this.mutableElements().updateStyle();
  }
}, Wx = "single", On = {
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
    if (r.selectionType == null && (r.selectionType = Wx), e !== void 0)
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
        if (Fe(e[0])) {
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
        Fe(e) && (o = n[0], l = o.x, u = o.y, pe(l) && (a.x += l), pe(u) && (a.y += u), this.emit("pan viewport"));
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
      } else if (L0(e)) {
        var i = e;
        n = {
          x1: i.x1,
          y1: i.y1,
          x2: i.x2,
          y2: i.y2
        }, n.w = n.x2 - n.x1, n.h = n.y2 - n.y1;
      } else Qt(e) || (e = this.mutableElements());
      if (!(Qt(e) && e.empty())) {
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
    if (r.zoomingEnabled || (o = !0), pe(e) ? s = e : Fe(e) && (s = e.level, e.position != null ? i = Xs(e.position, a, n) : e.renderedPosition != null && (i = e.renderedPosition), i != null && !r.panningEnabled && (o = !0)), s = s > r.maxZoom ? r.maxZoom : s, s = s < r.minZoom ? r.minZoom : s, o || !pe(s) || s === a || i != null && (!pe(i.x) || !pe(i.y)))
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
    if (pe(e.zoom) || (n = !1), Fe(e.pan) || (a = !1), !n && !a)
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
      } else Qt(e) || (e = this.mutableElements());
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
On.centre = On.center;
On.autolockNodes = On.autolock;
On.autoungrabifyNodes = On.autoungrabify;
var fi = {
  data: $e.data({
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
  removeData: $e.removeData({
    field: "data",
    event: "data",
    triggerFnName: "trigger",
    triggerEvent: !0,
    updateStyle: !0
  }),
  scratch: $e.data({
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
  removeScratch: $e.removeData({
    field: "scratch",
    event: "scratch",
    triggerFnName: "trigger",
    triggerEvent: !0,
    updateStyle: !0
  })
};
fi.attr = fi.data;
fi.removeAttr = fi.removeData;
var ci = function(e) {
  var r = this;
  e = Ae({}, e);
  var n = e.container;
  n && !ws(n) && ws(n[0]) && (n = n[0]);
  var a = n ? n._cyreg : null;
  a = a || {}, a && a.cy && (a.cy.destroy(), a = {});
  var i = a.readies = a.readies || [];
  n && (n._cyreg = a), a.cy = r;
  var s = dt !== void 0 && n !== void 0 && !e.headless, o = e;
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
    elements: new St(this),
    // elements in the graph
    listeners: [],
    // list of listeners
    aniEles: new St(this),
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
      x: Fe(o.pan) && pe(o.pan.x) ? o.pan.x : 0,
      y: Fe(o.pan) && pe(o.pan.y) ? o.pan.y : 0
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
    var g = h.some(I0);
    if (g)
      return ba.all(h).then(y);
    y(h);
  };
  u.styleEnabled && r.setStyle([]);
  var c = Ae({}, o, o.renderer);
  r.initRenderer(c);
  var v = function(h, y, g) {
    r.notifications(!1);
    var p = r.mutableElements();
    p.length > 0 && p.remove(), h != null && (Fe(h) || Ke(h)) && r.add(h), r.one("layoutready", function(b) {
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
}, Ds = ci.prototype;
Ae(Ds, {
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
    if (e == null) return dt;
    var r = this._private.container.ownerDocument;
    return r === void 0 || r == null ? dt : r.defaultView || dt;
  },
  mount: function(e) {
    if (e != null) {
      var r = this, n = r._private, a = n.options;
      return !ws(e) && ws(e[0]) && (e = e[0]), r.stopAnimationLoop(), r.destroyRenderer(), n.container = e, n.styleEnabled = !0, r.invalidateSize(), r.initRenderer(Ae({}, a, a.renderer, {
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
    return Cr(this._private.options);
  },
  json: function(e) {
    var r = this, n = r._private, a = r.mutableElements(), i = function(w) {
      return r.getElementById(w.id());
    };
    if (Fe(e)) {
      if (r.startBatch(), e.elements) {
        var s = {}, o = function(w, E) {
          for (var T = [], x = [], S = 0; S < w.length; S++) {
            var D = w[S];
            if (!D.data.id) {
              He("cy.json() cannot handle elements without an ID attribute");
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
        if (Ke(e.elements))
          o(e.elements);
        else
          for (var l = ["nodes", "edges"], u = 0; u < l.length; u++) {
            var f = l[u], c = e.elements[f];
            Ke(c) && o(c, f);
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
      })), this._private.styleEnabled && (p.style = r.style().json()), p.data = Cr(r.data());
      var m = n.options;
      return p.zoomingEnabled = n.zoomingEnabled, p.userZoomingEnabled = n.userZoomingEnabled, p.zoom = n.zoom, p.minZoom = n.minZoom, p.maxZoom = n.maxZoom, p.panningEnabled = n.panningEnabled, p.userPanningEnabled = n.userPanningEnabled, p.pan = Cr(n.pan), p.boxSelectionEnabled = n.boxSelectionEnabled, p.renderer = Cr(m.renderer), p.hideEdgesOnViewport = m.hideEdgesOnViewport, p.textureOnViewport = m.textureOnViewport, p.wheelSensitivity = m.wheelSensitivity, p.motionBlur = m.motionBlur, p.multiClickDebounceTime = m.multiClickDebounceTime, p;
    }
  }
});
Ds.$id = Ds.getElementById;
[Ox, Vx, Ug, xu, is, $x, Eu, ss, Gx, On, fi].forEach(function(t) {
  Ae(Ds, t);
});
var Kx = {
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
}, Yx = {
  maximal: !1,
  // whether to shift nodes down their natural BFS depths in order to avoid upwards edges (DAGS only); setting acyclic to true sets maximal to true also
  acyclic: !1
  // whether the tree is acyclic and thus a node could be shifted (due to the maximal option) multiple times without causing an infinite loop; setting to true sets maximal to true also; if you are uncertain whether a tree is acyclic, set to false to avoid potential infinite loops
}, Gn = function(e) {
  return e.scratch("breadthfirst");
}, nd = function(e, r) {
  return e.scratch("breadthfirst", r);
};
function Gg(t) {
  this.options = Ae({}, Kx, Yx, t);
}
Gg.prototype.run = function() {
  var t = this.options, e = t.cy, r = t.eles, n = r.nodes().filter(function(te) {
    return te.isChildless();
  }), a = r, i = t.directed, s = t.acyclic || t.maximal || t.maximalAdjustments > 0, o = !!t.boundingBox, l = qt(o ? t.boundingBox : structuredClone(e.extent())), u;
  if (Qt(t.roots))
    u = t.roots;
  else if (Ke(t.roots)) {
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
    p[K].push(Y), nd(Y, {
      index: ue,
      depth: K
    });
  }, w = function(Y, K) {
    var ue = Gn(Y), oe = ue.depth, ve = ue.index;
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
      nd(oe, {
        depth: Y,
        index: ue
      });
    }
  }, D = function(Y, K) {
    for (var ue = Gn(Y), oe = Y.incomers().filter(function(J) {
      return J.isNode() && r.has(J);
    }), ve = -1, de = Y.id(), me = 0; me < oe.length; me++) {
      var Te = oe[me], Ee = Gn(Te);
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
        He("Detected double maximal shift for node `" + I.id() + "`.  Bailing maximal adjustment due to cycle.  Use `options.maximal: true` only on DAGs.");
        break;
      }
    }
  }
  var O = 0;
  if (t.avoidOverlap)
    for (var L = 0; L < n.length; L++) {
      var N = n[L], H = N.layoutDimensions(t), V = H.w, F = H.h;
      O = Math.max(O, V, F);
    }
  var $ = {}, Q = function(Y) {
    if ($[Y.id()])
      return $[Y.id()];
    for (var K = Gn(Y).depth, ue = Y.neighborhood(), oe = 0, ve = 0, de = 0; de < ue.length; de++) {
      var me = ue[de];
      if (!(me.isEdge() || me.isParent() || !n.has(me))) {
        var Te = Gn(me);
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
    return ve === 0 ? Wh(Y.id(), K.id()) : ve;
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
    O
  ), W = p.reduce(function(te, Y) {
    return Math.max(te, Y.length);
  }, 0), j = function(Y) {
    var K = Gn(Y), ue = K.depth, oe = K.index;
    if (t.circle) {
      var ve = Math.min(l.w / 2 / ae, l.h / 2 / ae);
      ve = Math.max(ve, O);
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
        O
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
    return ub(j(Y), l, Z[t.direction]);
  };
  return r.nodes().layoutPositions(this, t, ne), this;
};
var Xx = {
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
function Wg(t) {
  this.options = Ae({}, Xx, t);
}
Wg.prototype.run = function() {
  var t = this.options, e = t, r = t.cy, n = e.eles, a = e.counterclockwise !== void 0 ? !e.counterclockwise : e.clockwise, i = n.nodes().not(":parent");
  e.sort && (i = i.sort(e.sort));
  for (var s = qt(e.boundingBox ? e.boundingBox : {
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
var Zx = {
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
function Kg(t) {
  this.options = Ae({}, Zx, t);
}
Kg.prototype.run = function() {
  for (var t = this.options, e = t, r = e.counterclockwise !== void 0 ? !e.counterclockwise : e.clockwise, n = t.cy, a = e.eles, i = a.nodes().not(":parent"), s = qt(e.boundingBox ? e.boundingBox : {
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
      var _ = Math.cos(I) - Math.cos(0), O = Math.sin(I) - Math.sin(0), L = Math.sqrt(T * T / (_ * _ + O * O));
      A = Math.max(L, A);
    }
    R.r = A, A += T;
  }
  if (e.equidistant) {
    for (var N = 0, H = 0, V = 0; V < p.length; V++) {
      var F = p[V], $ = F.r - H;
      N = Math.max(N, $);
    }
    H = 0;
    for (var Q = 0; Q < p.length; Q++) {
      var se = p[Q];
      Q === 0 && (H = se.r), se.r = H, H += N;
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
var Gl, Qx = {
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
function oo(t) {
  this.options = Ae({}, Qx, t), this.options.layout = this;
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
oo.prototype.run = function() {
  var t = this.options, e = t.cy, r = this;
  r.stopped = !1, (t.animate === !0 || t.animate === !1) && r.emit({
    type: "layoutstart",
    layout: r
  }), t.debug === !0 ? Gl = !0 : Gl = !1;
  var n = jx(e, r, t);
  Gl && eE(n), t.randomize && tE(n);
  var a = Hr(), i = function() {
    rE(n, e, t), t.fit === !0 && e.fit(t.padding);
  }, s = function(v) {
    return !(r.stopped || v >= t.numIter || (nE(n, t), n.temperature = n.temperature * t.coolingFactor, n.temperature < t.minTemp));
  }, o = function() {
    if (t.animate === !0 || t.animate === !1)
      i(), r.one("layoutstop", t.stop), r.emit({
        type: "layoutstop",
        layout: r
      });
    else {
      var v = t.eles.nodes(), d = Xg(n, t, v);
      v.layoutPositions(r, t, d);
    }
  }, l = 0, u = !0;
  if (t.animate === !0) {
    var f = function() {
      for (var v = 0; u && v < t.refresh; )
        u = s(l), l++, v++;
      if (!u)
        id(n, t), o();
      else {
        var d = Hr();
        d - a >= t.animationThreshold && i(), xs(f);
      }
    };
    f();
  } else {
    for (; u; )
      u = s(l), l++;
    id(n, t), o();
  }
  return this;
};
oo.prototype.stop = function() {
  return this.stopped = !0, this.thread && this.thread.stop(), this.emit("layoutstop"), this;
};
oo.prototype.destroy = function() {
  return this.thread && this.thread.stop(), this;
};
var jx = function(e, r, n) {
  for (var a = n.eles.edges(), i = n.eles.nodes(), s = qt(n.boundingBox ? n.boundingBox : {
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
    var M = tt(n.idealEdgeLength) ? n.idealEdgeLength(k) : n.idealEdgeLength, I = tt(n.edgeElasticity) ? n.edgeElasticity(k) : n.edgeElasticity, _ = o.idToIndex[R.sourceId], O = o.idToIndex[R.targetId], L = o.indexToGraph[_], N = o.indexToGraph[O];
    if (L != N) {
      for (var H = Jx(R.sourceId, R.targetId, o), V = o.graphSet[H], F = 0, g = o.layoutNodes[_]; V.indexOf(g.id) === -1; )
        g = o.layoutNodes[o.idToIndex[g.parentId]], F++;
      for (g = o.layoutNodes[O]; V.indexOf(g.id) === -1; )
        g = o.layoutNodes[o.idToIndex[g.parentId]], F++;
      M *= F * n.nestingFactor;
    }
    R.idealLength = M, R.elasticity = I, o.layoutEdges.push(R);
  }
  return o;
}, Jx = function(e, r, n) {
  var a = Yg(e, r, 0, n);
  return 2 > a.count ? 0 : a.graph;
}, Yg = function(e, r, n, a) {
  var i = a.graphSet[n];
  if (-1 < i.indexOf(e) && -1 < i.indexOf(r))
    return {
      count: 2,
      graph: n
    };
  for (var s = 0, o = 0; o < i.length; o++) {
    var l = i[o], u = a.idToIndex[l], f = a.layoutNodes[u].children;
    if (f.length !== 0) {
      var c = a.indexToGraph[a.idToIndex[f[0]]], v = Yg(e, r, c, a);
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
}, eE, tE = function(e, r) {
  for (var n = e.clientWidth, a = e.clientHeight, i = 0; i < e.nodeSize; i++) {
    var s = e.layoutNodes[i];
    s.children.length === 0 && !s.isLocked && (s.positionX = Math.random() * n, s.positionY = Math.random() * a);
  }
}, Xg = function(e, r, n) {
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
}, rE = function(e, r, n) {
  var a = n.layout, i = n.eles.nodes(), s = Xg(e, n, i);
  i.positions(s), e.ready !== !0 && (e.ready = !0, a.one("layoutready", n.ready), a.emit({
    type: "layoutready",
    layout: this
  }));
}, nE = function(e, r, n) {
  aE(e, r), oE(e), lE(e, r), uE(e), fE(e);
}, aE = function(e, r) {
  for (var n = 0; n < e.graphSet.length; n++)
    for (var a = e.graphSet[n], i = a.length, s = 0; s < i; s++)
      for (var o = e.layoutNodes[e.idToIndex[a[s]]], l = s + 1; l < i; l++) {
        var u = e.layoutNodes[e.idToIndex[a[l]]];
        iE(o, u, e, r);
      }
}, ad = function(e) {
  return -e + 2 * e * Math.random();
}, iE = function(e, r, n, a) {
  var i = e.cmptId, s = r.cmptId;
  if (!(i !== s && !n.isCompound)) {
    var o = r.positionX - e.positionX, l = r.positionY - e.positionY, u = 1;
    o === 0 && l === 0 && (o = ad(u), l = ad(u));
    var f = sE(e, r, o, l);
    if (f > 0)
      var c = a.nodeOverlap * f, v = Math.sqrt(o * o + l * l), d = c * o / v, h = c * l / v;
    else
      var y = As(e, o, l), g = As(r, -1 * o, -1 * l), p = g.x - y.x, m = g.y - y.y, b = p * p + m * m, v = Math.sqrt(b), c = (e.nodeRepulsion + r.nodeRepulsion) / b, d = c * p / v, h = c * m / v;
    e.isLocked || (e.offsetX -= d, e.offsetY -= h), r.isLocked || (r.offsetX += d, r.offsetY += h);
  }
}, sE = function(e, r, n, a) {
  if (n > 0)
    var i = e.maxX - r.minX;
  else
    var i = r.maxX - e.minX;
  if (a > 0)
    var s = e.maxY - r.minY;
  else
    var s = r.maxY - e.minY;
  return i >= 0 && s >= 0 ? Math.sqrt(i * i + s * s) : 0;
}, As = function(e, r, n) {
  var a = e.positionX, i = e.positionY, s = e.height || 1, o = e.width || 1, l = n / r, u = s / o, f = {};
  return r === 0 && 0 < n || r === 0 && 0 > n ? (f.x = a, f.y = i + s / 2, f) : 0 < r && -1 * u <= l && l <= u ? (f.x = a + o / 2, f.y = i + o * n / 2 / r, f) : 0 > r && -1 * u <= l && l <= u ? (f.x = a - o / 2, f.y = i - o * n / 2 / r, f) : 0 < n && (l <= -1 * u || l >= u) ? (f.x = a + s * r / 2 / n, f.y = i + s / 2, f) : (0 > n && (l <= -1 * u || l >= u) && (f.x = a - s * r / 2 / n, f.y = i - s / 2), f);
}, oE = function(e, r) {
  for (var n = 0; n < e.edgeSize; n++) {
    var a = e.layoutEdges[n], i = e.idToIndex[a.sourceId], s = e.layoutNodes[i], o = e.idToIndex[a.targetId], l = e.layoutNodes[o], u = l.positionX - s.positionX, f = l.positionY - s.positionY;
    if (!(u === 0 && f === 0)) {
      var c = As(s, u, f), v = As(l, -1 * u, -1 * f), d = v.x - c.x, h = v.y - c.y, y = Math.sqrt(d * d + h * h), g = Math.pow(a.idealLength - y, 2) / a.elasticity;
      if (y !== 0)
        var p = g * d / y, m = g * h / y;
      else
        var p = 0, m = 0;
      s.isLocked || (s.offsetX += p, s.offsetY += m), l.isLocked || (l.offsetX -= p, l.offsetY -= m);
    }
  }
}, lE = function(e, r) {
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
}, uE = function(e, r) {
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
}, fE = function(e, r) {
  for (var n = 0; n < e.nodeSize; n++) {
    var a = e.layoutNodes[n];
    0 < a.children.length && (a.maxX = void 0, a.minX = void 0, a.maxY = void 0, a.minY = void 0);
  }
  for (var n = 0; n < e.nodeSize; n++) {
    var a = e.layoutNodes[n];
    if (!(0 < a.children.length || a.isLocked)) {
      var i = cE(a.offsetX, a.offsetY, e.temperature);
      a.positionX += i.x, a.positionY += i.y, a.offsetX = 0, a.offsetY = 0, a.minX = a.positionX - a.width, a.maxX = a.positionX + a.width, a.minY = a.positionY - a.height, a.maxY = a.positionY + a.height, Zg(a, e);
    }
  }
  for (var n = 0; n < e.nodeSize; n++) {
    var a = e.layoutNodes[n];
    0 < a.children.length && !a.isLocked && (a.positionX = (a.maxX + a.minX) / 2, a.positionY = (a.maxY + a.minY) / 2, a.width = a.maxX - a.minX, a.height = a.maxY - a.minY);
  }
}, cE = function(e, r, n) {
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
}, Zg = function(e, r) {
  var n = e.parentId;
  if (n != null) {
    var a = r.layoutNodes[r.idToIndex[n]], i = !1;
    if ((a.maxX == null || e.maxX + a.padRight > a.maxX) && (a.maxX = e.maxX + a.padRight, i = !0), (a.minX == null || e.minX - a.padLeft < a.minX) && (a.minX = e.minX - a.padLeft, i = !0), (a.maxY == null || e.maxY + a.padBottom > a.maxY) && (a.maxY = e.maxY + a.padBottom, i = !0), (a.minY == null || e.minY - a.padTop < a.minY) && (a.minY = e.minY - a.padTop, i = !0), i)
      return Zg(a, r);
  }
}, id = function(e, r) {
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
}, vE = {
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
function Qg(t) {
  this.options = Ae({}, vE, t);
}
Qg.prototype.run = function() {
  var t = this.options, e = t, r = t.cy, n = e.eles, a = n.nodes().not(":parent");
  e.sort && (a = a.sort(e.sort));
  var i = qt(e.boundingBox ? e.boundingBox : {
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
    }, I = 0, _ = 0, O = function() {
      _++, _ >= u && (_ = 0, I++);
    }, L = {}, N = 0; N < a.length; N++) {
      var H = a[N], V = e.position(H);
      if (V && (V.row !== void 0 || V.col !== void 0)) {
        var F = {
          row: V.row,
          col: V.col
        };
        if (F.col === void 0)
          for (F.col = 0; R(F.row, F.col); )
            F.col++;
        else if (F.row === void 0)
          for (F.row = 0; R(F.row, F.col); )
            F.row++;
        L[H.id()] = F, M(F.row, F.col);
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
          O();
        le = _ * m + m / 2 + i.x1, ce = I * b + b / 2 + i.y1, M(I, _), O();
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
var dE = {
  ready: function() {
  },
  // on layoutready
  stop: function() {
  }
  // on layoutstop
};
function hf(t) {
  this.options = Ae({}, dE, t);
}
hf.prototype.run = function() {
  var t = this.options, e = t.eles, r = this;
  return t.cy, r.emit("layoutstart"), e.nodes().positions(function() {
    return {
      x: 0,
      y: 0
    };
  }), r.one("layoutready", t.ready), r.emit("layoutready"), r.one("layoutstop", t.stop), r.emit("layoutstop"), this;
};
hf.prototype.stop = function() {
  return this;
};
var hE = {
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
function jg(t) {
  this.options = Ae({}, hE, t);
}
jg.prototype.run = function() {
  var t = this.options, e = t.eles, r = e.nodes(), n = tt(t.positions);
  function a(i) {
    if (t.positions == null)
      return Ob(i.position());
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
var gE = {
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
function Jg(t) {
  this.options = Ae({}, gE, t);
}
Jg.prototype.run = function() {
  var t = this.options, e = t.cy, r = t.eles, n = qt(t.boundingBox ? t.boundingBox : {
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
var pE = [{
  name: "breadthfirst",
  impl: Gg
}, {
  name: "circle",
  impl: Wg
}, {
  name: "concentric",
  impl: Kg
}, {
  name: "cose",
  impl: oo
}, {
  name: "grid",
  impl: Qg
}, {
  name: "null",
  impl: hf
}, {
  name: "preset",
  impl: jg
}, {
  name: "random",
  impl: Jg
}];
function ep(t) {
  this.options = t, this.notifications = 0;
}
var sd = function() {
}, od = function() {
  throw new Error("A headless instance can not render images");
};
ep.prototype = {
  recalculateRenderedStyle: sd,
  notify: function() {
    this.notifications++;
  },
  init: sd,
  isHeadless: function() {
    return !0;
  },
  png: od,
  jpg: od
};
var gf = {};
gf.arrowShapeWidth = 0.3;
gf.registerArrowShapes = function() {
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
        var m = i(a(this.points, h + 2 * p, y, g)), b = Wt(v, d, m);
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
    collide: Es,
    roughCollide: Es,
    draw: Ju,
    spacing: xc,
    gap: xc
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
      var g = i(a(this.points, c + 2 * y, v, d)), p = i(a(this.pointsTee, c + 2 * y, v, d)), m = Wt(u, f, g) || Wt(u, f, p);
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
      return Wt(u, f, m) || p;
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
      var g = i(a(this.points, c + 2 * y, v, d)), p = i(a(this.crossLinePts(c, h), c + 2 * y, v, d)), m = Wt(u, f, g) || Wt(u, f, p);
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
var zn = {};
zn.projectIntoViewport = function(t, e) {
  var r = this.cy, n = this.findContainerClientCoords(), a = n[0], i = n[1], s = n[4], o = r.pan(), l = r.zoom(), u = ((t - a) / s - o.x) / l, f = ((e - i) / s - o.y) / l;
  return [u, f];
};
zn.findContainerClientCoords = function() {
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
zn.invalidateContainerClientCoordsCache = function() {
  this.containerBB = null;
};
zn.findNearestElement = function(t, e, r, n) {
  return this.findNearestElements(t, e, r, n)[0];
};
zn.findNearestElements = function(t, e, r, n) {
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
    var S = x._private, D = S.rscratch, A = x.pstyle("width").pfValue, k = x.pstyle("arrow-scale").value, R = A / 2 + f, M = R * R, I = R * 2, N = S.source, H = S.target, _;
    if (D.edgeType === "segments" || D.edgeType === "straight" || D.edgeType === "haystack") {
      for (var O = D.allpts, L = 0; L + 3 < O.length; L += 2)
        if (Xb(t, e, O[L], O[L + 1], O[L + 2], O[L + 3], I) && M > (_ = e1(t, e, O[L], O[L + 1], O[L + 2], O[L + 3])))
          return g(x, _), !0;
    } else if (D.edgeType === "bezier" || D.edgeType === "multibezier" || D.edgeType === "self" || D.edgeType === "compound") {
      for (var O = D.allpts, L = 0; L + 5 < D.allpts.length; L += 4)
        if (Zb(t, e, O[L], O[L + 1], O[L + 2], O[L + 3], O[L + 4], O[L + 5], I) && M > (_ = Jb(t, e, O[L], O[L + 1], O[L + 2], O[L + 3], O[L + 4], O[L + 5])))
          return g(x, _), !0;
    }
    for (var N = N || S.source, H = H || S.target, V = a.getArrowWidth(A, k), F = [{
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
    }], L = 0; L < F.length; L++) {
      var $ = F[L], Q = i.arrowShapes[x.pstyle($.name + "-arrow-shape").value], se = x.pstyle("width").pfValue;
      if (Q.roughCollide(t, e, V, $.angle, {
        x: $.x,
        y: $.y
      }, se, f) && Q.collide(t, e, V, $.angle, {
        x: $.x,
        y: $.y
      }, se, f))
        return g(x), !0;
    }
    u && o.length > 0 && (p(N), p(H));
  }
  function b(x, S, D) {
    return Ft(x, S, D);
  }
  function w(x, S) {
    var D = x._private, A = v, k;
    S ? k = S + "-" : k = "", x.boundingBox();
    var R = D.labelBounds[S || "main"], M = x.pstyle(k + "label").value, I = x.pstyle("text-events").strValue === "yes";
    if (!(!I || !M)) {
      var _ = b(D.rscratch, "labelX", S), O = b(D.rscratch, "labelY", S), L = b(D.rscratch, "labelAngle", S), N = x.pstyle(k + "text-margin-x").pfValue, H = x.pstyle(k + "text-margin-y").pfValue, V = R.x1 - A - N, F = R.x2 + A - N, $ = R.y1 - A - H, Q = R.y2 + A - H;
      if (L) {
        var se = Math.cos(L), ae = Math.sin(L), le = function(B, z) {
          return B = B - _, z = z - O, {
            x: B * se - z * ae + _,
            y: B * ae + z * se + O
          };
        }, ce = le(V, $), he = le(V, Q), ie = le(F, $), U = le(F, Q), X = [
          // with the margin added after the rotation is applied
          ce.x + N,
          ce.y + H,
          ie.x + N,
          ie.y + H,
          U.x + N,
          U.y + H,
          he.x + N,
          he.y + H
        ];
        if (Wt(t, e, X))
          return g(x), !0;
      } else if (en(R, t, e))
        return g(x), !0;
    }
  }
  for (var E = s.length - 1; E >= 0; E--) {
    var T = s[E];
    T.isNode() ? p(T) || w(T) : m(T) || w(T) || w(T, "source") || w(T, "target");
  }
  return o;
};
zn.getAllInBox = function(t, e, r, n) {
  var a = this.getCachedZSortedEles().interactive, i = this.cy.zoom(), s = 2 / i, o = [], l = Math.min(t, r), u = Math.max(t, r), f = Math.min(e, n), c = Math.max(e, n);
  t = l, r = u, e = f, n = c;
  var v = qt({
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
    return Ft(B, z, W);
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
          k && Ho(k, d) && (o.push(w), A = !0);
        }
        !A && og(v, D) && o.push(w);
      } else if (T === "overlap" && nf(v, D)) {
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
        if (Ho(M, d))
          o.push(w);
        else {
          var I = g(w);
          I && Ho(I, d) && o.push(w);
        }
      }
    } else {
      var _ = b, O = _._private, L = O.rscratch, N = _.pstyle("box-selection").strValue;
      if (N === "none")
        continue;
      if (N === "contain") {
        if (L.startX != null && L.startY != null && !en(v, L.startX, L.startY) || L.endX != null && L.endY != null && !en(v, L.endX, L.endY))
          continue;
        if (L.edgeType === "bezier" || L.edgeType === "multibezier" || L.edgeType === "self" || L.edgeType === "compound" || L.edgeType === "segments" || L.edgeType === "haystack") {
          for (var H = O.rstyle.bezierPts || O.rstyle.linePts || O.rstyle.haystackPts, V = !0, F = 0; F < H.length; F++)
            if (!Pc(v, H[F])) {
              V = !1;
              break;
            }
          V && o.push(_);
        } else L.edgeType === "straight" && o.push(_);
      } else if (N === "overlap") {
        var $ = !1;
        if (L.startX != null && L.startY != null && L.endX != null && L.endY != null && (en(v, L.startX, L.startY) || en(v, L.endX, L.endY)))
          o.push(_), $ = !0;
        else if (!$ && L.edgeType === "haystack") {
          for (var Q = O.rstyle.haystackPts, se = 0; se < Q.length; se++)
            if (Pc(v, Q[se])) {
              o.push(_), $ = !0;
              break;
            }
        }
        if (!$) {
          var ae = O.rstyle.bezierPts || O.rstyle.linePts || O.rstyle.haystackPts;
          if ((!ae || ae.length < 2) && L.edgeType === "straight" && L.startX != null && L.startY != null && L.endX != null && L.endY != null && (ae = [{
            x: L.startX,
            y: L.startY
          }, {
            x: L.endX,
            y: L.endY
          }]), !ae || ae.length < 2) continue;
          for (var le = 0; le < ae.length - 1; le++) {
            for (var ce = ae[le], he = ae[le + 1], ie = 0; ie < h.length; ie++) {
              var U = ct(h[ie], 2), X = U[0], C = U[1];
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
var ks = {};
ks.calculateArrowAngles = function(t) {
  var e = t._private.rscratch, r = e.edgeType === "haystack", n = e.edgeType === "bezier", a = e.edgeType === "multibezier", i = e.edgeType === "segments", s = e.edgeType === "compound", o = e.edgeType === "self", l, u, f, c, v, d, p, m;
  if (r ? (f = e.haystackPts[0], c = e.haystackPts[1], v = e.haystackPts[2], d = e.haystackPts[3]) : (f = e.arrowStartX, c = e.arrowStartY, v = e.arrowEndX, d = e.arrowEndY), p = e.midX, m = e.midY, i)
    l = f - e.segpts[0], u = c - e.segpts[1];
  else if (a || s || o || n) {
    var h = e.allpts, y = bt(h[0], h[2], h[4], 0.1), g = bt(h[1], h[3], h[5], 0.1);
    l = f - y, u = c - g;
  } else
    l = f - p, u = c - m;
  e.srcArrowAngle = Ni(l, u);
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
      T = bt(h[A], h[k], h[R], 0), x = bt(h[A + 1], h[k + 1], h[R + 1], 0), S = bt(h[A], h[k], h[R], 1e-4), D = bt(h[A + 1], h[k + 1], h[R + 1], 1e-4);
    } else {
      var k = h.length / 2 - 1, A = k - 2, R = k + 2;
      T = bt(h[A], h[k], h[R], 0.4999), x = bt(h[A + 1], h[k + 1], h[R + 1], 0.4999), S = bt(h[A], h[k], h[R], 0.5), D = bt(h[A + 1], h[k + 1], h[R + 1], 0.5);
    }
    l = S - T, u = D - x;
  }
  if (e.midtgtArrowAngle = Ni(l, u), e.midDispX = l, e.midDispY = u, l *= -1, u *= -1, i) {
    var h = e.allpts;
    if (h.length / 2 % 2 !== 0) {
      if (!e.isRound) {
        var b = h.length / 2 - 1, M = b + 2;
        l = -(h[M] - h[b]), u = -(h[M + 1] - h[b + 1]);
      }
    }
  }
  if (e.midsrcArrowAngle = Ni(l, u), i)
    l = v - e.segpts[e.segpts.length - 2], u = d - e.segpts[e.segpts.length - 1];
  else if (a || s || o || n) {
    var h = e.allpts, I = h.length, y = bt(h[I - 6], h[I - 4], h[I - 2], 0.9), g = bt(h[I - 5], h[I - 3], h[I - 1], 0.9);
    l = v - y, u = d - g;
  } else
    l = v - p, u = d - m;
  e.tgtArrowAngle = Ni(l, u);
};
ks.getArrowWidth = ks.getArrowHeight = function(t, e) {
  var r = this.arrowWidthCache = this.arrowWidthCache || {}, n = r[t + ", " + e];
  return n || (n = Math.max(Math.pow(t * 13.37, 0.9), 29) * e, r[t + ", " + e] = n, n);
};
var Cu, Tu, yr = {}, Jt = {}, ld, ud, Dn, os, Rr, wn, Tn, gr, Wn, Gi, tp, rp, Su, Pu, fd, cd = function(e, r, n) {
  n.x = r.x - e.x, n.y = r.y - e.y, n.len = Math.sqrt(n.x * n.x + n.y * n.y), n.nx = n.x / n.len, n.ny = n.y / n.len, n.ang = Math.atan2(n.ny, n.nx);
}, yE = function(e, r) {
  r.x = e.x * -1, r.y = e.y * -1, r.nx = e.nx * -1, r.ny = e.ny * -1, r.ang = e.ang > 0 ? -(Math.PI - e.ang) : Math.PI + e.ang;
}, mE = function(e, r, n, a, i) {
  if (e !== fd ? cd(r, e, yr) : yE(Jt, yr), cd(r, n, Jt), ld = yr.nx * Jt.ny - yr.ny * Jt.nx, ud = yr.nx * Jt.nx - yr.ny * -Jt.ny, Rr = Math.asin(Math.max(-1, Math.min(1, ld))), Math.abs(Rr) < 1e-6) {
    Cu = r.x, Tu = r.y, Tn = Wn = 0;
    return;
  }
  Dn = 1, os = !1, ud < 0 ? Rr < 0 ? Rr = Math.PI + Rr : (Rr = Math.PI - Rr, Dn = -1, os = !0) : Rr > 0 && (Dn = -1, os = !0), r.radius !== void 0 ? Wn = r.radius : Wn = a, wn = Rr / 2, Gi = Math.min(yr.len / 2, Jt.len / 2), i ? (gr = Math.abs(Math.cos(wn) * Wn / Math.sin(wn)), gr > Gi ? (gr = Gi, Tn = Math.abs(gr * Math.sin(wn) / Math.cos(wn))) : Tn = Wn) : (gr = Math.min(Gi, Wn), Tn = Math.abs(gr * Math.sin(wn) / Math.cos(wn))), Su = r.x + Jt.nx * gr, Pu = r.y + Jt.ny * gr, Cu = Su - Jt.ny * Tn * Dn, Tu = Pu + Jt.nx * Tn * Dn, tp = r.x + yr.nx * gr, rp = r.y + yr.ny * gr, fd = r;
};
function np(t, e) {
  e.radius === 0 ? t.lineTo(e.cx, e.cy) : t.arc(e.cx, e.cy, e.radius, e.startAngle, e.endAngle, e.counterClockwise);
}
function pf(t, e, r, n) {
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
  } : (mE(t, e, r, n, a), {
    cx: Cu,
    cy: Tu,
    radius: Tn,
    startX: tp,
    startY: rp,
    stopX: Su,
    stopY: Pu,
    startAngle: yr.ang + Math.PI / 2 * Dn,
    endAngle: Jt.ang - Math.PI / 2 * Dn,
    counterClockwise: os
  });
}
var vi = 0.01, bE = Math.sqrt(2 * vi), Ot = {};
Ot.findMidptPtsEtc = function(t, e) {
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
        var c = this.manualEndptToPx(t.source()[0], s), v = ct(c, 2), d = v[0], h = v[1], y = this.manualEndptToPx(t.target()[0], o), g = ct(y, 2), p = g[0], m = g[1], b = {
          x1: d,
          y1: h,
          x2: p,
          y2: m
        };
        a = u(d, h, p, m), i = b;
      } else
        He("Edge ".concat(t.id(), " has edge-distances:endpoints specified without manual endpoints specified via source-endpoint and target-endpoint.  Falling back on edge-distances:intersection (default).")), i = n;
      break;
    }
  }
  return {
    midptPts: i,
    vectorNormInverse: a
  };
};
Ot.findHaystackPoints = function(t) {
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
Ot.findSegmentsPoints = function(t, e) {
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
Ot.findLoopPoints = function(t, e, r, n) {
  var a = t._private.rscratch, i = e.dirCounts, s = e.srcPos, o = t.pstyle("control-point-distances"), l = o ? o.pfValue[0] : void 0, u = t.pstyle("loop-direction").pfValue, f = t.pstyle("loop-sweep").pfValue, c = t.pstyle("control-point-step-size").pfValue;
  a.edgeType = "self";
  var v = r, d = c;
  n && (v = 0, d = l);
  var h = u - Math.PI / 2, y = h - f / 2, g = h + f / 2, p = u + "_" + f;
  v = i[p] === void 0 ? i[p] = 0 : ++i[p], a.ctrlpts = [s.x + Math.cos(y) * 1.4 * d * (v / 3 + 1), s.y + Math.sin(y) * 1.4 * d * (v / 3 + 1), s.x + Math.cos(g) * 1.4 * d * (v / 3 + 1), s.y + Math.sin(g) * 1.4 * d * (v / 3 + 1)];
};
Ot.findCompoundLoopPoints = function(t, e, r, n) {
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
  }, w = 0.5, E = Math.max(w, Math.log(o * vi)), T = Math.max(w, Math.log(u * vi));
  a.ctrlpts = [b.x, b.y - (1 + Math.pow(g, 1.12) / 100) * y * (h / 3 + 1) * E, b.x - (1 + Math.pow(g, 1.12) / 100) * y * (h / 3 + 1) * T, b.y];
};
Ot.findStraightEdgePoints = function(t) {
  t._private.rscratch.edgeType = "straight";
};
Ot.findBezierPoints = function(t, e, r, n, a) {
  var i = t._private.rscratch, s = t.pstyle("control-point-step-size").pfValue, o = t.pstyle("control-point-distances"), l = t.pstyle("control-point-weights"), u = o && l ? Math.min(o.value.length, l.value.length) : 1, f = o ? o.pfValue[0] : void 0, c = l.value[0], v = n;
  i.edgeType = v ? "multibezier" : "bezier", i.ctrlpts = [];
  for (var d = 0; d < u; d++) {
    var h = (0.5 - e.eles.length / 2 + r) * s * (a ? -1 : 1), y = void 0, g = rf(h);
    v && (f = o ? o.pfValue[d] : s, c = l.value[d]), n ? y = f : y = f !== void 0 ? g * f : void 0;
    var p = y !== void 0 ? y : h, m = 1 - c, b = c, w = this.findMidptPtsEtc(t, e), E = w.midptPts, T = w.vectorNormInverse, x = {
      x: E.x1 * m + E.x2 * b,
      y: E.y1 * m + E.y2 * b
    };
    i.ctrlpts.push(x.x + T.x * p, x.y + T.y * p);
  }
};
Ot.findTaxiPoints = function(t, e) {
  var r = t._private.rscratch;
  r.edgeType = "segments";
  var n = "vertical", a = "horizontal", i = "leftward", s = "rightward", o = "downward", l = "upward", u = "auto", f = e.posPts, c = e.srcW, v = e.srcH, d = e.tgtW, h = e.tgtH, y = t.pstyle("edge-distances").value, g = y !== "node-position", p = t.pstyle("taxi-direction").value, m = p, b = t.pstyle("taxi-turn"), w = b.units === "%", E = b.pfValue, T = E < 0, x = t.pstyle("taxi-turn-min-distance").pfValue, S = g ? (c + d) / 2 : 0, D = g ? (v + h) / 2 : 0, A = f.x2 - f.x1, k = f.y2 - f.y1, R = function(G, re) {
    return G > 0 ? Math.max(G - re, 0) : Math.min(G + re, 0);
  }, M = R(A, S), I = R(k, D), _ = !1;
  m === u ? p = Math.abs(M) > Math.abs(I) ? a : n : m === l || m === o ? (p = n, _ = !0) : (m === i || m === s) && (p = a, _ = !0);
  var O = p === n, L = O ? I : M, N = O ? k : A, H = rf(N), V = !1;
  !(_ && (w || T)) && (m === o && N < 0 || m === l && N > 0 || m === i && N > 0 || m === s && N < 0) && (H *= -1, L = H * Math.abs(L), V = !0);
  var F;
  if (w) {
    var $ = E < 0 ? 1 + E : E;
    F = $ * L;
  } else {
    var Q = E < 0 ? L : 0;
    F = Q + E * H;
  }
  var se = function(G) {
    return Math.abs(G) < x || Math.abs(G) >= Math.abs(L);
  }, ae = se(F), le = se(Math.abs(L) - Math.abs(F)), ce = ae || le;
  if (ce && !V)
    if (O) {
      var he = Math.abs(N) <= v / 2, ie = Math.abs(A) <= d / 2;
      if (he) {
        var U = (f.x1 + f.x2) / 2, X = f.y1, C = f.y2;
        r.segpts = [U, X, U, C];
      } else if (ie) {
        var B = (f.y1 + f.y2) / 2, z = f.x1, W = f.x2;
        r.segpts = [z, B, W, B];
      } else
        r.segpts = [f.x1, f.y2];
    } else {
      var j = Math.abs(N) <= c / 2, Z = Math.abs(k) <= h / 2;
      if (j) {
        var ne = (f.y1 + f.y2) / 2, te = f.x1, Y = f.x2;
        r.segpts = [te, ne, Y, ne];
      } else if (Z) {
        var K = (f.x1 + f.x2) / 2, ue = f.y1, oe = f.y2;
        r.segpts = [K, ue, K, oe];
      } else
        r.segpts = [f.x2, f.y1];
    }
  else if (O) {
    var ve = f.y1 + F + (g ? v / 2 * H : 0), de = f.x1, me = f.x2;
    r.segpts = [de, ve, me, ve];
  } else {
    var Te = f.x1 + F + (g ? c / 2 * H : 0), Ee = f.y1, Pe = f.y2;
    r.segpts = [Te, Ee, Te, Pe];
  }
  if (r.isRound) {
    var J = t.pstyle("taxi-radius").value, P = t.pstyle("radius-type").value[0] === "arc-radius";
    r.radii = new Array(r.segpts.length / 2).fill(J), r.isArcRadius = new Array(r.segpts.length / 2).fill(P);
  }
};
Ot.tryToCorrectInvalidPoints = function(t, e) {
  var r = t._private.rscratch;
  if (r.edgeType === "bezier") {
    var n = e.srcPos, a = e.tgtPos, i = e.srcW, s = e.srcH, o = e.tgtW, l = e.tgtH, u = e.srcShape, f = e.tgtShape, c = e.srcCornerRadius, v = e.tgtCornerRadius, d = e.srcRs, h = e.tgtRs, y = !pe(r.startX) || !pe(r.startY), g = !pe(r.arrowStartX) || !pe(r.arrowStartY), p = !pe(r.endX) || !pe(r.endY), m = !pe(r.arrowEndX) || !pe(r.arrowEndY), b = 3, w = this.getArrowWidth(t.pstyle("width").pfValue, t.pstyle("arrow-scale").value) * this.arrowShapeWidth, E = b * w, T = Ln({
      x: r.ctrlpts[0],
      y: r.ctrlpts[1]
    }, {
      x: r.startX,
      y: r.startY
    }), x = T < E, S = Ln({
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
      }, O = u.intersectLine(n.x, n.y, i, s, _.x, _.y, 0, c, d);
      x ? (r.ctrlpts[0] = r.ctrlpts[0] + M.x * (E - T), r.ctrlpts[1] = r.ctrlpts[1] + M.y * (E - T)) : (r.ctrlpts[0] = O[0] + M.x * E, r.ctrlpts[1] = O[1] + M.y * E);
    }
    if (p || m || D) {
      A = !0;
      var L = {
        // delta
        x: r.ctrlpts[0] - a.x,
        y: r.ctrlpts[1] - a.y
      }, N = Math.sqrt(L.x * L.x + L.y * L.y), H = {
        // normalised delta
        x: L.x / N,
        y: L.y / N
      }, V = Math.max(i, s), F = {
        // *2 radius guarantees outside shape
        x: r.ctrlpts[0] + H.x * 2 * V,
        y: r.ctrlpts[1] + H.y * 2 * V
      }, $ = f.intersectLine(a.x, a.y, o, l, F.x, F.y, 0, v, h);
      D ? (r.ctrlpts[0] = r.ctrlpts[0] + H.x * (E - S), r.ctrlpts[1] = r.ctrlpts[1] + H.y * (E - S)) : (r.ctrlpts[0] = $[0] + H.x * E, r.ctrlpts[1] = $[1] + H.y * E);
    }
    A && this.findEndpoints(t);
  }
};
Ot.storeAllpts = function(t) {
  var e = t._private.rscratch;
  if (e.edgeType === "multibezier" || e.edgeType === "bezier" || e.edgeType === "self" || e.edgeType === "compound") {
    e.allpts = [], e.allpts.push(e.startX, e.startY);
    for (var r = 0; r + 1 < e.ctrlpts.length; r += 2)
      e.allpts.push(e.ctrlpts[r], e.ctrlpts[r + 1]), r + 3 < e.ctrlpts.length && e.allpts.push((e.ctrlpts[r] + e.ctrlpts[r + 2]) / 2, (e.ctrlpts[r + 1] + e.ctrlpts[r + 3]) / 2);
    e.allpts.push(e.endX, e.endY);
    var n, a;
    e.ctrlpts.length / 2 % 2 === 0 ? (n = e.allpts.length / 2 - 1, e.midX = e.allpts[n], e.midY = e.allpts[n + 1]) : (n = e.allpts.length / 2 - 3, a = 0.5, e.midX = bt(e.allpts[n], e.allpts[n + 2], e.allpts[n + 4], a), e.midY = bt(e.allpts[n + 1], e.allpts[n + 3], e.allpts[n + 5], a));
  } else if (e.edgeType === "straight")
    e.allpts = [e.startX, e.startY, e.endX, e.endY], e.midX = (e.startX + e.endX + e.arrowStartX + e.arrowEndX) / 4, e.midY = (e.startY + e.endY + e.arrowStartY + e.arrowEndY) / 4;
  else if (e.edgeType === "segments") {
    if (e.allpts = [], e.allpts.push(e.startX, e.startY), e.allpts.push.apply(e.allpts, e.segpts), e.allpts.push(e.endX, e.endY), e.isRound) {
      e.roundCorners = [];
      for (var i = 2; i + 3 < e.allpts.length; i += 2) {
        var s = e.radii[i / 2 - 1], o = e.isArcRadius[i / 2 - 1];
        e.roundCorners.push(pf({
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
Ot.checkForInvalidEdgeWarning = function(t) {
  var e = t[0]._private.rscratch;
  e.nodesOverlap || pe(e.startX) && pe(e.startY) && pe(e.endX) && pe(e.endY) ? e.loggedErr = !1 : e.loggedErr || (e.loggedErr = !0, He("Edge `" + t.id() + "` has invalid endpoints and so it is impossible to draw.  Adjust your edge style (e.g. control points) accordingly or use an alternative edge type.  This is expected behaviour when the source node and the target node overlap."));
};
Ot.findEdgeControlPoints = function(t) {
  var e = this;
  if (!(!t || t.length === 0)) {
    for (var r = this, n = r.cy, a = n.hasCompoundNodes(), i = new Fr(), s = function(D, A) {
      return [].concat(bs(D), [A ? 1 : 0]).join("-");
    }, o = [], l = [], u = 0; u < t.length; u++) {
      var f = t[u], c = f._private, v = f.pstyle("curve-style").value;
      if (!(f.removed() || !f.takesUpSpace())) {
        if (v === "haystack") {
          l.push(f);
          continue;
        }
        var d = v === "unbundled-bezier" || Jr(v, "segments") || v === "straight" || v === "straight-triangle" || Jr(v, "taxi"), h = v === "unbundled-bezier" || v === "bezier", y = c.source, g = c.target, p = y.poolIndex(), m = g.poolIndex(), b = [p, m].sort(), w = s(b, d), E = i.get(w);
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
        ef(M.eles), _.forEach(function(P) {
          return M.eles.push(P);
        }), M.eles.sort(function(P, q) {
          return P.poolIndex() - q.poolIndex();
        });
      }
      var O = M.eles[0], L = O.source(), N = O.target();
      if (L.poolIndex() > N.poolIndex()) {
        var H = L;
        L = N, N = H;
      }
      var V = M.srcPos = L.position(), F = M.tgtPos = N.position(), $ = M.srcW = L.outerWidth(), Q = M.srcH = L.outerHeight(), se = M.tgtW = N.outerWidth(), ae = M.tgtH = N.outerHeight(), le = M.srcShape = r.nodeShapes[e.getNodeShape(L)], ce = M.tgtShape = r.nodeShapes[e.getNodeShape(N)], he = M.srcCornerRadius = L.pstyle("corner-radius").value === "auto" ? "auto" : L.pstyle("corner-radius").pfValue, ie = M.tgtCornerRadius = N.pstyle("corner-radius").value === "auto" ? "auto" : N.pstyle("corner-radius").pfValue, U = M.tgtRs = N._private.rscratch, X = M.srcRs = L._private.rscratch;
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
        var B = M.eles[C], z = B[0]._private.rscratch, W = B.pstyle("curve-style").value, j = W === "unbundled-bezier" || Jr(W, "segments") || Jr(W, "taxi"), Z = !L.same(B.source());
        if (!M.calculatedIntersection && L !== N && (M.hasBezier || M.hasUnbundled)) {
          M.calculatedIntersection = !0;
          var ne = le.intersectLine(V.x, V.y, $, Q, F.x, F.y, 0, he, X), te = M.srcIntn = ne, Y = ce.intersectLine(F.x, F.y, se, ae, V.x, V.y, 0, ie, U), K = M.tgtIntn = Y, ue = M.intersectionPts = {
            x1: ne[0],
            x2: Y[0],
            y1: ne[1],
            y2: Y[1]
          }, oe = M.posPts = {
            x1: V.x,
            x2: F.x,
            y1: V.y,
            y2: F.y
          }, ve = Y[1] - ne[1], de = Y[0] - ne[0], me = Math.sqrt(de * de + ve * ve);
          pe(me) && me >= bE || (me = Math.sqrt(Math.max(de * de, vi) + Math.max(ve * ve, vi)));
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
          M.nodesOverlap = !pe(me) || ce.checkPoint(ne[0], ne[1], 0, se, ae, F.x, F.y, ie, U) || le.checkPoint(Y[0], Y[1], 0, $, Q, V.x, V.y, he, X), M.vectorNormInverse = Pe, I = {
            nodesOverlap: M.nodesOverlap,
            dirCounts: M.dirCounts,
            calculatedIntersection: !0,
            hasBezier: M.hasBezier,
            hasUnbundled: M.hasUnbundled,
            eles: M.eles,
            srcPos: F,
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
        z.nodesOverlap = J.nodesOverlap, z.srcIntn = J.srcIntn, z.tgtIntn = J.tgtIntn, z.isRound = W.startsWith("round"), a && (L.isParent() || L.isChild() || N.isParent() || N.isChild()) && (L.parents().anySame(N) || N.parents().anySame(L) || L.same(N) && L.isParent()) ? e.findCompoundLoopPoints(B, J, C, j) : L === N ? e.findLoopPoints(B, J, C, j) : W.endsWith("segments") ? e.findSegmentsPoints(B, J) : W.endsWith("taxi") ? e.findTaxiPoints(B, J) : W === "straight" || !j && M.eles.length % 2 === 1 && C === Math.floor(M.eles.length / 2) ? e.findStraightEdgePoints(B) : e.findBezierPoints(B, J, C, j, Z), e.findEndpoints(B), e.tryToCorrectInvalidPoints(B, J), e.checkForInvalidEdgeWarning(B), e.storeAllpts(B), e.storeEdgeProjections(B), e.calculateArrowAngles(B), e.recalculateEdgeLabelProjections(B), e.calculateLabelAngles(B);
      }
    }, x = 0; x < o.length; x++)
      T();
    this.findHaystackPoints(l);
  }
};
function ap(t) {
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
Ot.getSegmentPoints = function(t) {
  var e = t[0]._private.rscratch;
  this.recalculateRenderedStyle(t);
  var r = e.edgeType;
  if (r === "segments")
    return ap(e.segpts);
};
Ot.getControlPoints = function(t) {
  var e = t[0]._private.rscratch;
  this.recalculateRenderedStyle(t);
  var r = e.edgeType;
  if (r === "bezier" || r === "multibezier" || r === "self" || r === "compound")
    return ap(e.ctrlpts);
};
Ot.getEdgeMidpoint = function(t) {
  var e = t[0]._private.rscratch;
  return this.recalculateRenderedStyle(t), {
    x: e.midX,
    y: e.midY
  };
};
var Pi = {};
Pi.manualEndptToPx = function(t, e) {
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
Pi.findEndpoints = function(t) {
  var e, r, n, a, i = this, s, o = t.source()[0], l = t.target()[0], u = o.position(), f = l.position(), c = t.pstyle("target-arrow-shape").value, v = t.pstyle("source-arrow-shape").value, d = t.pstyle("target-distance-from-node").pfValue, h = t.pstyle("source-distance-from-node").pfValue, y = o._private.rscratch, g = l._private.rscratch, p = t.pstyle("curve-style").value, m = t._private.rscratch, b = m.edgeType, w = Jr(p, "taxi"), E = b === "self" || b === "compound", T = b === "bezier" || b === "multibezier" || E, x = b !== "bezier", S = b === "straight" || b === "segments", D = b === "segments", A = T || x || S, k = E || w, R = t.pstyle("source-endpoint"), M = k ? "outside-to-node" : R.value, I = o.pstyle("corner-radius").value === "auto" ? "auto" : o.pstyle("corner-radius").pfValue, _ = t.pstyle("target-endpoint"), O = k ? "outside-to-node" : _.value, L = l.pstyle("corner-radius").value === "auto" ? "auto" : l.pstyle("corner-radius").pfValue;
  m.srcManEndpt = R, m.tgtManEndpt = _;
  var N, H, V, F, $ = (e = (_ == null || (r = _.pfValue) === null || r === void 0 ? void 0 : r.length) === 2 ? _.pfValue : null) !== null && e !== void 0 ? e : [0, 0], Q = (n = (R == null || (a = R.pfValue) === null || a === void 0 ? void 0 : a.length) === 2 ? R.pfValue : null) !== null && n !== void 0 ? n : [0, 0];
  if (T) {
    var se = [m.ctrlpts[0], m.ctrlpts[1]], ae = x ? [m.ctrlpts[m.ctrlpts.length - 2], m.ctrlpts[m.ctrlpts.length - 1]] : se;
    N = ae, H = se;
  } else if (S) {
    var le = D ? m.segpts.slice(0, 2) : [f.x + $[0], f.y + $[1]], ce = D ? m.segpts.slice(m.segpts.length - 2) : [u.x + Q[0], u.y + Q[1]];
    N = ce, H = le;
  }
  if (O === "inside-to-node")
    s = [f.x, f.y];
  else if (_.units)
    s = this.manualEndptToPx(l, _);
  else if (O === "outside-to-line")
    s = m.tgtIntn;
  else if (O === "outside-to-node" || O === "outside-to-node-or-label" ? V = N : (O === "outside-to-line" || O === "outside-to-line-or-label") && (V = [u.x, u.y]), s = i.nodeShapes[this.getNodeShape(l)].intersectLine(f.x, f.y, l.outerWidth(), l.outerHeight(), V[0], V[1], 0, L, g), O === "outside-to-node-or-label" || O === "outside-to-line-or-label") {
    var he = l._private.rscratch, ie = he.labelWidth, U = he.labelHeight, X = he.labelX, C = he.labelY, B = ie / 2, z = U / 2, W = l.pstyle("text-valign").value;
    W = ya(W), W === "top" ? C -= z : W === "bottom" && (C += z);
    var j = l.pstyle("text-halign").value;
    j = pa(j), j === "left" ? X -= B : j === "right" && (X += B);
    var Z = oi(V[0], V[1], [X - B, C - z, X + B, C - z, X + B, C + z, X - B, C + z], f.x, f.y);
    if (Z.length > 0) {
      var ne = u, te = Cn(ne, jn(s)), Y = Cn(ne, jn(Z)), K = te;
      if (Y < te && (s = Z, K = Y), Z.length > 2) {
        var ue = Cn(ne, {
          x: Z[2],
          y: Z[3]
        });
        ue < K && (s = [Z[2], Z[3]]);
      }
    }
  }
  var oe = Fi(s, N, i.arrowShapes[c].spacing(t) + d), ve = Fi(s, N, i.arrowShapes[c].gap(t) + d);
  if (m.endX = ve[0], m.endY = ve[1], m.arrowEndX = oe[0], m.arrowEndY = oe[1], M === "inside-to-node")
    s = [u.x, u.y];
  else if (R.units)
    s = this.manualEndptToPx(o, R);
  else if (M === "outside-to-line")
    s = m.srcIntn;
  else if (M === "outside-to-node" || M === "outside-to-node-or-label" ? F = H : (M === "outside-to-line" || M === "outside-to-line-or-label") && (F = [f.x, f.y]), s = i.nodeShapes[this.getNodeShape(o)].intersectLine(u.x, u.y, o.outerWidth(), o.outerHeight(), F[0], F[1], 0, I, y), M === "outside-to-node-or-label" || M === "outside-to-line-or-label") {
    var de = o._private.rscratch, me = de.labelWidth, Te = de.labelHeight, Ee = de.labelX, Pe = de.labelY, J = me / 2, P = Te / 2, q = o.pstyle("text-valign").value;
    q = ya(q), q === "top" ? Pe -= P : q === "bottom" && (Pe += P);
    var G = o.pstyle("text-halign").value;
    G = pa(G), G === "left" ? Ee -= J : G === "right" && (Ee += J);
    var re = oi(F[0], F[1], [Ee - J, Pe - P, Ee + J, Pe - P, Ee + J, Pe + P, Ee - J, Pe + P], u.x, u.y);
    if (re.length > 0) {
      var ee = f, ye = Cn(ee, jn(s)), fe = Cn(ee, jn(re)), ge = ye;
      if (fe < ye && (s = [re[0], re[1]], ge = fe), re.length > 2) {
        var be = Cn(ee, {
          x: re[2],
          y: re[3]
        });
        be < ge && (s = [re[2], re[3]]);
      }
    }
  }
  var De = Fi(s, H, i.arrowShapes[v].spacing(t) + h), Be = Fi(s, H, i.arrowShapes[v].gap(t) + h);
  m.startX = Be[0], m.startY = Be[1], m.arrowStartX = De[0], m.arrowStartY = De[1], A && (!pe(m.startX) || !pe(m.startY) || !pe(m.endX) || !pe(m.endY) ? m.badLine = !0 : m.badLine = !1);
};
Pi.getSourceEndpoint = function(t) {
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
Pi.getTargetEndpoint = function(t) {
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
var yf = {};
function wE(t, e, r) {
  for (var n = function(u, f, c, v) {
    return bt(u, f, c, v);
  }, a = e._private, i = a.rstyle.bezierPts, s = 0; s < t.bezierProjPcts.length; s++) {
    var o = t.bezierProjPcts[s];
    i.push({
      x: n(r[0], r[2], r[4], o),
      y: n(r[1], r[3], r[5], o)
    });
  }
}
yf.storeEdgeProjections = function(t) {
  var e = t._private, r = e.rscratch, n = r.edgeType;
  if (e.rstyle.bezierPts = null, e.rstyle.linePts = null, e.rstyle.haystackPts = null, n === "multibezier" || n === "bezier" || n === "self" || n === "compound") {
    e.rstyle.bezierPts = [];
    for (var a = 0; a + 5 < r.allpts.length; a += 4)
      wE(this, t, r.allpts.slice(a, a + 6));
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
yf.recalculateEdgeProjections = function(t) {
  this.findEdgeControlPoints(t);
};
var Dr = {};
Dr.recalculateNodeLabelProjection = function(t) {
  var e = t.pstyle("label").strValue;
  if (!sn(e)) {
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
var ip = function(e, r) {
  var n = Math.atan(r / e);
  return e === 0 && n < 0 && (n = n * -1), n;
}, sp = function(e, r) {
  var n = r.x - e.x, a = r.y - e.y;
  return ip(n, a);
}, xE = function(e, r, n, a) {
  var i = si(0, a - 1e-3, 1), s = si(0, a + 1e-3, 1), o = sa(e, r, n, i), l = sa(e, r, n, s);
  return sp(o, l);
};
Dr.recalculateEdgeLabelProjections = function(t) {
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
      br(r.rscratch, c, v, d), br(r.rstyle, c, v, d);
    };
    s("labelX", null, e.x), s("labelY", null, e.y);
    var o = ip(n.midDispX, n.midDispY);
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
        var R = Ln(S, D), M = x.segments[x.segments.length - 1], I = {
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
            R = si(0, R, 1), e = sa(S.p0, S.p1, S.p2, R), v = xE(S.p0, S.p1, S.p2, R);
            break;
          }
          case "straight":
          case "segments":
          case "haystack": {
            for (var M = 0, I, _, O, L, N = n.allpts.length, H = 0; H + 3 < N && (d ? (O = {
              x: n.allpts[H],
              y: n.allpts[H + 1]
            }, L = {
              x: n.allpts[H + 2],
              y: n.allpts[H + 3]
            }) : (O = {
              x: n.allpts[N - 2 - H],
              y: n.allpts[N - 1 - H]
            }, L = {
              x: n.allpts[N - 4 - H],
              y: n.allpts[N - 3 - H]
            }), I = Ln(O, L), _ = M, M += I, !(M >= h)); H += 2)
              ;
            var V = h - _, F = V / I;
            F = si(0, F, 1), e = $b(O, L, F), v = sp(O, L);
            break;
          }
        }
        s("labelX", c, e.x), s("labelY", c, e.y), s("labelAutoAngle", c, v);
      }
    };
    u("source"), u("target"), this.applyLabelDimensions(t);
  }
};
Dr.applyLabelDimensions = function(t) {
  this.applyPrefixedLabelDimensions(t), t.isEdge() && (this.applyPrefixedLabelDimensions(t, "source"), this.applyPrefixedLabelDimensions(t, "target"));
};
Dr.applyPrefixedLabelDimensions = function(t, e) {
  var r = t._private, n = this.getLabelText(t, e), a = Mn(n, t._private.labelDimsKey);
  if (Ft(r.rscratch, "prefixedLabelDimsKey", e) !== a) {
    br(r.rscratch, "prefixedLabelDimsKey", e, a);
    var i = this.calculateLabelDimensions(t, n), s = t.pstyle("line-height").pfValue, o = t.pstyle("font-size").pfValue, l = t.pstyle("text-wrap").strValue, u = Ft(r.rscratch, "labelWrapCachedLines", e) || [], f = l !== "wrap" ? 1 : Math.max(u.length, 1), c = o * s, v = i.width, d = i.height + (f - 1) * (s - 1) * o;
    br(r.rstyle, "labelWidth", e, v), br(r.rscratch, "labelWidth", e, v), br(r.rstyle, "labelHeight", e, d), br(r.rscratch, "labelHeight", e, d), br(r.rscratch, "labelLineHeight", e, c), br(r.rscratch, "labelActualDescent", e, i.labelActualDescent);
  }
};
Dr.getLabelText = function(t, e) {
  var r = t._private, n = e ? e + "-" : "", a = t.pstyle(n + "label").strValue, i = t.pstyle("text-transform").value, s = function(Q, se) {
    return se ? (br(r.rscratch, Q, e, se), se) : Ft(r.rscratch, Q, e);
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
        var E = p.matchAll(y), T = "", x = 0, S = Kt(E), D;
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
    var O = t.pstyle("text-max-width").pfValue, L = "", N = "…", H = !1;
    if (this.calculateLabelDimensions(t, a).width < O)
      return a;
    for (var V = 0; V < a.length; V++) {
      var F = this.calculateLabelDimensions(t, L + a[V] + N).width;
      if (F > O)
        break;
      L += a[V], V === a.length - 1 && (H = !0);
    }
    return H || (L += N), L;
  }
  return a;
};
Dr.getLabelJustification = function(t) {
  var e = t.pstyle("text-justification").strValue, r = t.pstyle("text-halign").strValue;
  return e === "auto" ? t.isNode() ? lx(r) : "center" : e;
};
Dr.calculateLabelDimensions = function(t, e) {
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
Dr.calculateLabelAngle = function(t, e) {
  var r = t._private, n = r.rscratch, a = t.isEdge(), i = e ? e + "-" : "", s = t.pstyle(i + "text-rotation"), o = s.strValue;
  return o === "none" ? 0 : a && o === "autorotate" ? n.labelAutoAngle : o === "autorotate" ? 0 : s.pfValue;
};
Dr.calculateLabelAngles = function(t) {
  var e = this, r = t.isEdge(), n = t._private, a = n.rscratch;
  a.labelAngle = e.calculateLabelAngle(t), r && (a.sourceLabelAngle = e.calculateLabelAngle(t, "source"), a.targetLabelAngle = e.calculateLabelAngle(t, "target"));
};
var op = {}, vd = 28, dd = !1;
op.getNodeShape = function(t) {
  var e = this, r = t.pstyle("shape").value;
  if (r === "cutrectangle" && (t.width() < vd || t.height() < vd))
    return dd || (He("The `cutrectangle` node shape can not be used at small sizes so `rectangle` is used instead"), dd = !0), "rectangle";
  if (t.isParent())
    return r === "rectangle" || r === "roundrectangle" || r === "round-rectangle" || r === "cutrectangle" || r === "cut-rectangle" || r === "barrel" ? r : "rectangle";
  if (r === "polygon") {
    var n = t.pstyle("shape-polygon-points").value;
    return e.nodeShapes.makePolygon(n).name;
  }
  return r;
};
var lo = {};
lo.registerCalculationListeners = function() {
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
lo.onUpdateEleCalcs = function(t) {
  var e = this.onUpdateEleCalcsFns = this.onUpdateEleCalcsFns || [];
  e.push(t);
};
lo.recalculateRenderedStyle = function(t, e) {
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
var uo = {};
uo.updateCachedGrabbedEles = function() {
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
uo.invalidateCachedZSortedEles = function() {
  this.cachedZSortedEles = null;
};
uo.getCachedZSortedEles = function(t) {
  if (t || !this.cachedZSortedEles) {
    var e = this.cy.mutableElements().toArray();
    e.sort($g), e.interactive = e.filter(function(r) {
      return r.interactive();
    }), this.cachedZSortedEles = e, this.updateCachedGrabbedEles();
  } else
    e = this.cachedZSortedEles;
  return e;
};
var lp = {};
[zn, ks, Ot, Pi, yf, Dr, op, lo, uo].forEach(function(t) {
  Ae(lp, t);
});
var up = {};
up.getCachedImage = function(t, e, r) {
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
var fp = function(e, r) {
  var n = e[0];
  !n || n._private.grabbed === r || (n._private.grabbed = r, e.updateStyle(!1));
}, EE = function(e) {
  fp(e, !0);
}, CE = function(e) {
  fp(e, !1);
}, Ea = {};
Ea.registerBinding = function(t, e, r, n) {
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
Ea.binder = function(t) {
  var e = this, r = e.cy.window(), n = t === r || t === r.document || t === r.document.body || M0(t);
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
Ea.nodeIsDraggable = function(t) {
  return t && t.isNode() && !t.locked() && t.grabbable();
};
Ea.nodeIsGrabbable = function(t) {
  return this.nodeIsDraggable(t) && t.interactive();
};
Ea.load = function() {
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
    !re && P.grabbable() && !P.locked() && (G.merge(P), EE(P));
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
      CE(q), l(q), f(q);
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
  var w = Ei(function() {
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
    for (var Be = t.container, Me = P.target, xe = Me.parentNode, Oe = !1; xe; ) {
      if (xe === Be) {
        Oe = !0;
        break;
      }
      xe = xe.parentNode;
    }
    return !!Oe;
  };
  t.registerBinding(t.container, "mousedown", function(P) {
    if (S(P) && !(t.hoverData.which === 1 && P.which !== 1)) {
      P.preventDefault(), p(), t.hoverData.capture = !0, t.hoverData.which = P.which;
      var q = t.cy, G = [P.clientX, P.clientY], re = t.projectIntoViewport(G[0], G[1]), ee = t.selection, ye = t.findNearestElements(re[0], re[1], !0, !1), fe = ye[0], ge = t.dragData.possibleDragElements;
      t.hoverData.mdownPos = re, t.hoverData.mdownGPos = G;
      var be = function(_e) {
        return {
          originalEvent: P,
          type: _e,
          position: {
            x: re[0],
            y: re[1]
          }
        };
      }, De = function() {
        t.hoverData.tapholdCancelled = !1, clearTimeout(t.hoverData.tapholdTimeout), t.hoverData.tapholdTimeout = setTimeout(function() {
          if (!t.hoverData.tapholdCancelled) {
            var _e = t.hoverData.down;
            _e ? _e.emit(be("taphold")) : q.emit(be("taphold"));
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
            var Me = function(_e) {
              _e.emit(be("grab"));
            };
            if (u(fe), !fe.selected())
              ge = t.dragData.possibleDragElements = q.collection(), h(fe, {
                addToList: ge
              }), fe.emit(be("grabon")).emit(be("grab"));
            else {
              ge = t.dragData.possibleDragElements = q.collection();
              var xe = q.$(function(Oe) {
                return Oe.isNode() && Oe.selected() && t.nodeIsGrabbable(Oe);
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
      var Me = t.hoverData.last, xe = t.hoverData.down, Oe = [fe[0] - De[2], fe[1] - De[3]], _e = t.dragData.possibleDragElements, yt;
      if (be) {
        var rt = ye[0] - be[0], nr = rt * rt, nt = ye[1] - be[1], ot = nt * nt, vt = nr + ot;
        t.hoverData.isOverThresholdDrag = yt = vt >= t.desktopTapThreshold2;
      }
      var Et = i(P);
      yt && (t.hoverData.tapholdCancelled = !0);
      var dr = function() {
        var jt = t.hoverData.dragDelta = t.hoverData.dragDelta || [];
        jt.length === 0 ? (jt.push(Oe[0]), jt.push(Oe[1])) : (jt[0] += Oe[0], jt[1] += Oe[1]);
      };
      G = !0, a(Be, ["mousemove", "vmousemove", "tapdrag"], P, {
        x: fe[0],
        y: fe[1]
      });
      var Je = function(jt) {
        return {
          originalEvent: P,
          type: jt,
          position: {
            x: fe[0],
            y: fe[1]
          }
        };
      }, kr = function() {
        t.data.bgActivePosistion = void 0, t.hoverData.selecting || re.emit(Je("boxstart")), De[4] = 1, t.hoverData.selecting = !0, t.redrawHint("select", !0), t.redraw();
      };
      if (t.hoverData.which === 3) {
        if (yt) {
          var ar = Je("cxtdrag");
          xe ? xe.emit(ar) : re.emit(ar), t.hoverData.cxtDragged = !0, (!t.hoverData.cxtOver || Be !== t.hoverData.cxtOver) && (t.hoverData.cxtOver && t.hoverData.cxtOver.emit(Je("cxtdragout")), t.hoverData.cxtOver = Be, Be && Be.emit(Je("cxtdragover")));
        }
      } else if (t.hoverData.dragging) {
        if (G = !0, re.panningEnabled() && re.userPanningEnabled()) {
          var Yr;
          if (t.hoverData.justStartedPan) {
            var ki = t.hoverData.mdownPos;
            Yr = {
              x: (fe[0] - ki[0]) * ee,
              y: (fe[1] - ki[1]) * ee
            }, t.hoverData.justStartedPan = !1;
          } else
            Yr = {
              x: Oe[0] * ee,
              y: Oe[1] * ee
            };
          re.panBy(Yr), re.emit(Je("dragpan")), t.hoverData.dragged = !0;
        }
        fe = t.projectIntoViewport(P.clientX, P.clientY);
      } else if (De[4] == 1 && (xe == null || xe.pannable())) {
        if (yt) {
          if (!t.hoverData.dragging && re.boxSelectionEnabled() && (Et || !re.panningEnabled() || !re.userPanningEnabled()))
            kr();
          else if (!t.hoverData.selecting && re.panningEnabled() && re.userPanningEnabled()) {
            var yn = s(xe, t.hoverData.downs);
            yn && (t.hoverData.dragging = !0, t.hoverData.justStartedPan = !0, De[4] = 0, t.data.bgActivePosistion = jn(ge), t.redrawHint("select", !0), t.redraw());
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
          if (yt) {
            if (re.boxSelectionEnabled() && Et)
              xe && xe.grabbed() && (y(_e), xe.emit(Je("freeon")), _e.emit(Je("free")), t.dragData.didDrag && (xe.emit(Je("dragfreeon")), _e.emit(Je("dragfree")))), kr();
            else if (xe && xe.grabbed() && t.nodeIsDraggable(xe)) {
              var Ht = !t.dragData.didDrag;
              Ht && t.redrawHint("eles", !0), t.dragData.didDrag = !0, t.hoverData.draggingEles || d(_e, {
                inDragLayer: !0
              });
              var kt = {
                x: 0,
                y: 0
              };
              if (pe(Oe[0]) && pe(Oe[1]) && (kt.x += Oe[0], kt.y += Oe[1], Ht)) {
                var Ut = t.hoverData.dragDelta;
                Ut && pe(Ut[0]) && pe(Ut[1]) && (kt.x += Ut[0], kt.y += Ut[1]);
              }
              t.hoverData.draggingEles = !0, _e.silentShift(kt).emit(Je("position")).emit(Je("drag")), t.redrawHint("drag", !0), t.redraw();
            }
          } else
            dr();
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
            var Oe = function(rt) {
              return rt.selectable() && !rt.selected();
            };
            G.selectionType() === "additive" || be || G.$(r).unmerge(xe).unselect(), xe.emit(De("box")).stdFilter(Oe).select().emit(De("boxselect")), t.redraw();
          }
          if (t.hoverData.dragging && (t.hoverData.dragging = !1, t.redrawHint("select", !0), t.redrawHint("eles", !0), t.redraw()), !ee[4]) {
            t.redrawHint("drag", !0), t.redrawHint("eles", !0);
            var _e = ge && ge.grabbed();
            y(fe), _e && (ge.emit(De("freeon")), fe.emit(De("free")), t.dragData.didDrag && (ge.emit(De("dragfreeon")), fe.emit(De("dragfree"))));
          }
        }
        ee[4] = 0, t.hoverData.down = null, t.hoverData.cxtStarted = !1, t.hoverData.draggingEles = !1, t.hoverData.selecting = !1, t.hoverData.isOverThresholdDrag = !1, t.dragData.didDrag = !1, t.hoverData.dragged = !1, t.hoverData.dragDelta = [], t.hoverData.mdownPos = null, t.hoverData.mdownGPos = null, t.hoverData.which = null;
      }
    }
  }, !1);
  var M = [], I = 4, _, O = 1e5, L = function(P, q) {
    for (var G = 0; G < P.length; G++)
      if (P[G] % q !== 0)
        return !1;
    return !0;
  }, N = function(P) {
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
            _ = N(re) && ee > 5;
          }
          if (_)
            for (var ye = 0; ye < re.length; ye++)
              O = Math.min(Math.abs(re[ye]), O);
        } else
          M.push(G), q = !0;
      else _ && (O = Math.min(Math.abs(G), O));
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
          q && Math.abs(G) > 5 && (G = rf(G) * 5), Me = G / -250, _ && (Me /= O, Me *= 3), Me = Me * t.wheelSensitivity;
          var xe = P.deltaMode === 1;
          xe && (Me *= 33);
          var Oe = fe.zoom() * Math.pow(10, Me);
          P.type === "gesturechange" && (Oe = t.gestureStartZoom * P.scale), fe.zoom({
            level: Oe,
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
  var V, F, $, Q, se, ae, le, ce, he, ie, U, X, C, B = function(P, q, G, re) {
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
      var ye = function(Et) {
        return {
          originalEvent: P,
          type: Et,
          position: {
            x: G[0],
            y: G[1]
          }
        };
      };
      if (P.touches[1]) {
        t.touchData.singleTouchMoved = !0, y(t.dragData.touchDragEles);
        var fe = t.findContainerClientCoords();
        he = fe[0], ie = fe[1], U = fe[2], X = fe[3], V = P.touches[0].clientX - he, F = P.touches[0].clientY - ie, $ = P.touches[1].clientX - he, Q = P.touches[1].clientY - ie, C = 0 <= V && V <= U && 0 <= $ && $ <= U && 0 <= F && F <= X && 0 <= Q && Q <= X;
        var ge = q.pan(), be = q.zoom();
        se = B(V, F, $, Q), ae = z(V, F, $, Q), le = [(V + $) / 2, (F + Q) / 2], ce = [(le[0] - ge.x) / be, (le[1] - ge.y) / be];
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
          var Oe = t.findNearestElements(G[0], G[1], !0, !0), _e = Oe[0];
          if (_e != null && (_e.activate(), t.touchData.start = _e, t.touchData.starts = Oe, t.nodeIsGrabbable(_e))) {
            var yt = t.dragData.touchDragEles = q.collection(), rt = null;
            t.redrawHint("eles", !0), t.redrawHint("drag", !0), _e.selected() ? (rt = q.$(function(vt) {
              return vt.selected() && t.nodeIsGrabbable(vt);
            }), d(rt, {
              addToList: yt
            })) : h(_e, {
              addToList: yt
            }), u(_e), _e.emit(ye("grabon")), rt ? rt.forEach(function(vt) {
              vt.emit(ye("grab"));
            }) : _e.emit(ye("grab"));
          }
          a(_e, ["touchstart", "tapstart", "vmousedown"], P, {
            x: G[0],
            y: G[1]
          }), _e == null && (t.data.bgActivePosistion = {
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
        for (var nr = t.touchData.startPosition = [null, null, null, null, null, null], nt = 0; nt < G.length; nt++)
          nr[nt] = re[nt] = G[nt];
        var ot = P.touches[0];
        t.touchData.startGPosition = [ot.clientX, ot.clientY];
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
      var be = function(qp) {
        return {
          originalEvent: P,
          type: qp,
          position: {
            x: ee[0],
            y: ee[1]
          }
        };
      }, De = t.touchData.startGPosition, Be;
      if (q && P.touches[0] && De) {
        for (var Me = [], xe = 0; xe < ee.length; xe++)
          Me[xe] = ee[xe] - ye[xe];
        var Oe = P.touches[0].clientX - De[0], _e = Oe * Oe, yt = P.touches[0].clientY - De[1], rt = yt * yt, nr = _e + rt;
        Be = nr >= t.touchTapThreshold2;
      }
      if (q && t.touchData.cxt) {
        P.preventDefault();
        var nt = P.touches[0].clientX - he, ot = P.touches[0].clientY - ie, vt = P.touches[1].clientX - he, Et = P.touches[1].clientY - ie, dr = z(nt, ot, vt, Et), Je = dr / ae, kr = 150, ar = kr * kr, Yr = 1.5, ki = Yr * Yr;
        if (Je >= ki || dr >= ar) {
          t.touchData.cxt = !1, t.data.bgActivePosistion = void 0, t.redrawHint("select", !0);
          var yn = be("cxttapend");
          t.touchData.start ? (t.touchData.start.unactivate().emit(yn), t.touchData.start = null) : re.emit(yn);
        }
      }
      if (q && t.touchData.cxt) {
        var yn = be("cxtdrag");
        t.data.bgActivePosistion = void 0, t.redrawHint("select", !0), t.touchData.start ? t.touchData.start.emit(yn) : re.emit(yn), t.touchData.start && (t.touchData.start._private.grabbed = !1), t.touchData.cxtDragged = !0;
        var Ht = t.findNearestElement(ee[0], ee[1], !0, !0);
        (!t.touchData.cxtOver || Ht !== t.touchData.cxtOver) && (t.touchData.cxtOver && t.touchData.cxtOver.emit(be("cxtdragout")), t.touchData.cxtOver = Ht, Ht && Ht.emit(be("cxtdragover")));
      } else if (q && P.touches[2] && re.boxSelectionEnabled())
        P.preventDefault(), t.data.bgActivePosistion = void 0, this.lastThreeTouch = +/* @__PURE__ */ new Date(), t.touchData.selecting || re.emit(be("boxstart")), t.touchData.selecting = !0, t.touchData.didSelect = !0, G[4] = 1, !G || G.length === 0 || G[0] === void 0 ? (G[0] = (ee[0] + ee[2] + ee[4]) / 3, G[1] = (ee[1] + ee[3] + ee[5]) / 3, G[2] = (ee[0] + ee[2] + ee[4]) / 3 + 1, G[3] = (ee[1] + ee[3] + ee[5]) / 3 + 1) : (G[2] = (ee[0] + ee[2] + ee[4]) / 3, G[3] = (ee[1] + ee[3] + ee[5]) / 3), t.redrawHint("select", !0), t.redraw();
      else if (q && P.touches[1] && !t.touchData.didSelect && re.zoomingEnabled() && re.panningEnabled() && re.userZoomingEnabled() && re.userPanningEnabled()) {
        P.preventDefault(), t.data.bgActivePosistion = void 0, t.redrawHint("select", !0);
        var kt = t.dragData.touchDragEles;
        if (kt) {
          t.redrawHint("drag", !0);
          for (var Ut = 0; Ut < kt.length; Ut++) {
            var Ta = kt[Ut]._private;
            Ta.grabbed = !1, Ta.rscratch.inDragLayer = !1;
          }
        }
        var jt = t.touchData.start, nt = P.touches[0].clientX - he, ot = P.touches[0].clientY - ie, vt = P.touches[1].clientX - he, Et = P.touches[1].clientY - ie, wf = B(nt, ot, vt, Et), Mp = wf / se;
        if (C) {
          var Lp = nt - V, Ip = ot - F, Op = vt - $, _p = Et - Q, Np = (Lp + Op) / 2, Fp = (Ip + _p) / 2, Sa = re.zoom(), fo = Sa * Mp, Bi = re.pan(), xf = ce[0] * Sa + Bi.x, Ef = ce[1] * Sa + Bi.y, zp = {
            x: -fo / Sa * (xf - Bi.x - Np) + xf,
            y: -fo / Sa * (Ef - Bi.y - Fp) + Ef
          };
          if (jt && jt.active()) {
            var kt = t.dragData.touchDragEles;
            y(kt), t.redrawHint("drag", !0), t.redrawHint("eles", !0), jt.unactivate().emit(be("freeon")), kt.emit(be("free")), t.dragData.didDrag && (jt.emit(be("dragfreeon")), kt.emit(be("dragfree")));
          }
          re.viewport({
            zoom: fo,
            pan: zp,
            cancelOnFailedZoom: !0
          }), re.emit(be("pinchzoom")), se = wf, V = nt, F = ot, $ = vt, Q = Et, t.pinching = !0;
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
        var ir = t.touchData.start, co = t.touchData.last, Ht;
        if (!t.hoverData.draggingEles && !t.swipePanning && (Ht = t.findNearestElement(ee[0], ee[1], !0, !0)), q && ir != null && P.preventDefault(), q && ir != null && t.nodeIsDraggable(ir))
          if (Be) {
            var kt = t.dragData.touchDragEles, Cf = !t.dragData.didDrag;
            Cf && d(kt, {
              inDragLayer: !0
            }), t.dragData.didDrag = !0;
            var Pa = {
              x: 0,
              y: 0
            };
            if (pe(Me[0]) && pe(Me[1]) && (Pa.x += Me[0], Pa.y += Me[1], Cf)) {
              t.redrawHint("eles", !0);
              var sr = t.touchData.dragDelta;
              sr && pe(sr[0]) && pe(sr[1]) && (Pa.x += sr[0], Pa.y += sr[1]);
            }
            t.hoverData.draggingEles = !0, kt.silentShift(Pa).emit(be("position")).emit(be("drag")), t.redrawHint("drag", !0), t.touchData.startPosition[0] == ye[0] && t.touchData.startPosition[1] == ye[1] && t.redrawHint("eles", !0), t.redraw();
          } else {
            var sr = t.touchData.dragDelta = t.touchData.dragDelta || [];
            sr.length === 0 ? (sr.push(Me[0]), sr.push(Me[1])) : (sr[0] += Me[0], sr[1] += Me[1]);
          }
        if (a(ir || Ht, ["touchmove", "tapdrag", "vmousemove"], P, {
          x: ee[0],
          y: ee[1]
        }), (!ir || !ir.grabbed()) && Ht != co && (co && co.emit(be("tapdragout")), Ht && Ht.emit(be("tapdragover"))), t.touchData.last = Ht, q)
          for (var Ut = 0; Ut < ee.length; Ut++)
            ee[Ut] && t.touchData.startPosition[Ut] && Be && (t.touchData.singleTouchMoved = !0);
        if (q && (ir == null || ir.pannable()) && re.panningEnabled() && re.userPanningEnabled()) {
          var Vp = s(ir, t.touchData.starts);
          Vp && (P.preventDefault(), t.data.bgActivePosistion || (t.data.bgActivePosistion = jn(t.touchData.startPosition)), t.swipePanning ? (re.panBy({
            x: Me[0] * fe,
            y: Me[1] * fe
          }), re.emit(be("dragpan"))) : Be && (t.swipePanning = !0, re.panBy({
            x: Oe * fe,
            y: yt * fe
          }), re.emit(be("dragpan")), ir && (ir.unactivate(), t.redrawHint("select", !0), t.touchData.start = null)));
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
    var De = function(ar) {
      return {
        originalEvent: P,
        type: ar,
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
      var Oe = function(ar) {
        return ar.selectable() && !ar.selected();
      };
      xe.emit(De("box")).stdFilter(Oe).select().emit(De("boxselect")), xe.nonempty() && t.redrawHint("eles", !0), t.redraw();
    }
    if (q != null && q.unactivate(), P.touches[2])
      t.data.bgActivePosistion = void 0, t.redrawHint("select", !0);
    else if (!P.touches[1]) {
      if (!P.touches[0]) {
        if (!P.touches[0]) {
          t.data.bgActivePosistion = void 0, t.redrawHint("select", !0);
          var _e = t.dragData.touchDragEles;
          if (q != null) {
            var yt = q._private.grabbed;
            y(_e), t.redrawHint("drag", !0), t.redrawHint("eles", !0), yt && (q.emit(De("freeon")), _e.emit(De("free")), t.dragData.didDrag && (q.emit(De("dragfreeon")), _e.emit(De("dragfree")))), a(q, ["touchend", "tapend", "vmouseup", "tapdragout"], P, {
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
          var nr = t.touchData.startPosition[0] - fe[0], nt = nr * nr, ot = t.touchData.startPosition[1] - fe[1], vt = ot * ot, Et = nt + vt, dr = Et * ye * ye;
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
          }, ee.multiClickDebounceTime()), K = P.timeStamp)), q != null && !t.dragData.didDrag && q._private.selectable && dr < t.touchTapThreshold2 && !t.pinching && (ee.selectionType() === "single" ? (ee.$(r).unmerge(q).unselect(["tapunselect"]), q.select(["tapselect"])) : q.selected() ? q.unselect(["tapunselect"]) : q.select(["tapselect"]), t.redrawHint("eles", !0)), t.touchData.singleTouchMoved = !0;
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
var Wr = {};
Wr.generatePolygon = function(t, e) {
  return this.nodeShapes[t] = {
    renderer: this,
    name: t,
    points: e,
    draw: function(n, a, i, s, o, l) {
      this.renderer.nodeShapeImpl("polygon", n, a, i, s, o, this.points);
    },
    intersectLine: function(n, a, i, s, o, l, u, f) {
      return oi(o, l, this.points, n, a, i / 2, s / 2, u);
    },
    checkPoint: function(n, a, i, s, o, l, u, f) {
      return Ur(n, a, this.points, l, u, s, o, [0, -1], i);
    },
    hasMiterBounds: t !== "rectangle",
    miterBounds: function(n, a, i, s, o, l) {
      return Yb(this.points, n, a, i, s, o);
    }
  };
};
Wr.generateEllipse = function() {
  return this.nodeShapes.ellipse = {
    renderer: this,
    name: "ellipse",
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i);
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      return r1(i, s, e, r, n / 2 + o, a / 2 + o);
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      return An(e, r, a, i, s, o, n);
    }
  };
};
Wr.generateRoundPolygon = function(t, e) {
  return this.nodeShapes[t] = {
    renderer: this,
    name: t,
    points: e,
    getOrCreateCorners: function(n, a, i, s, o, l, u) {
      if (l[u] !== void 0 && l[u + "-cx"] === n && l[u + "-cy"] === a)
        return l[u];
      l[u] = new Array(e.length / 2), l[u + "-cx"] = n, l[u + "-cy"] = a;
      var f = i / 2, c = s / 2;
      o = o === "auto" ? fg(i, s) : o;
      for (var v = new Array(e.length / 2), d = 0; d < e.length / 2; d++)
        v[d] = {
          x: n + f * e[d * 2],
          y: a + c * e[d * 2 + 1]
        };
      var h, y, g, p, m = v.length;
      for (y = v[m - 1], h = 0; h < m; h++)
        g = v[h % m], p = v[(h + 1) % m], l[u][h] = pf(y, g, p, o), y = g, g = p;
      return l[u];
    },
    draw: function(n, a, i, s, o, l, u) {
      this.renderer.nodeShapeImpl("round-polygon", n, a, i, s, o, this.points, this.getOrCreateCorners(a, i, s, o, l, u, "drawCorners"));
    },
    intersectLine: function(n, a, i, s, o, l, u, f, c) {
      return a1(o, l, this.points, n, a, i, s, u, this.getOrCreateCorners(n, a, i, s, f, c, "corners"));
    },
    checkPoint: function(n, a, i, s, o, l, u, f, c) {
      return t1(n, a, this.points, l, u, s, o, this.getOrCreateCorners(l, u, s, o, f, c, "corners"));
    }
  };
};
Wr.generateRoundRectangle = function() {
  return this.nodeShapes["round-rectangle"] = this.nodeShapes.roundrectangle = {
    renderer: this,
    name: "round-rectangle",
    points: Nt(4, 0),
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i, this.points, s);
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      return lg(i, s, e, r, n, a, o, l);
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      var u = a / 2, f = i / 2;
      l = l === "auto" ? ln(a, i) : l, l = Math.min(u, f, l);
      var c = l * 2;
      return !!(Ur(e, r, this.points, s, o, a, i - c, [0, -1], n) || Ur(e, r, this.points, s, o, a - c, i, [0, -1], n) || An(e, r, c, c, s - u + l, o - f + l, n) || An(e, r, c, c, s + u - l, o - f + l, n) || An(e, r, c, c, s + u - l, o + f - l, n) || An(e, r, c, c, s - u + l, o + f - l, n));
    }
  };
};
Wr.generateCutRectangle = function() {
  return this.nodeShapes["cut-rectangle"] = this.nodeShapes.cutrectangle = {
    renderer: this,
    name: "cut-rectangle",
    cornerLength: af(),
    points: Nt(4, 0),
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
      return oi(i, s, f, e, r);
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      var u = l === "auto" ? this.cornerLength : l;
      if (Ur(e, r, this.points, s, o, a, i - 2 * u, [0, -1], n) || Ur(e, r, this.points, s, o, a - 2 * u, i, [0, -1], n))
        return !0;
      var f = this.generateCutTrianglePts(a, i, s, o);
      return Wt(e, r, f.topLeft) || Wt(e, r, f.topRight) || Wt(e, r, f.bottomRight) || Wt(e, r, f.bottomLeft);
    }
  };
};
Wr.generateBarrel = function() {
  return this.nodeShapes.barrel = {
    renderer: this,
    name: "barrel",
    points: Nt(4, 0),
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i);
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      var u = 0.15, f = 0.5, c = 0.85, v = this.generateBarrelBezierPts(n + 2 * o, a + 2 * o, e, r), d = function(g) {
        var p = sa({
          x: g[0],
          y: g[1]
        }, {
          x: g[2],
          y: g[3]
        }, {
          x: g[4],
          y: g[5]
        }, u), m = sa({
          x: g[0],
          y: g[1]
        }, {
          x: g[2],
          y: g[3]
        }, {
          x: g[4],
          y: g[5]
        }, f), b = sa({
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
      return oi(i, s, h, e, r);
    },
    generateBarrelBezierPts: function(e, r, n, a) {
      var i = r / 2, s = e / 2, o = n - s, l = n + s, u = a - i, f = a + i, c = hu(e, r), v = c.heightOffset, d = c.widthOffset, h = c.ctrlPtOffsetPct * e, y = {
        topLeft: [o, u + v, o + h, u, o + d, u],
        topRight: [l - d, u, l - h, u, l, u + v],
        bottomRight: [l, f - v, l - h, f, l - d, f],
        bottomLeft: [o + d, f, o + h, f, o, f - v]
      };
      return y.topLeft.isTop = !0, y.topRight.isTop = !0, y.bottomLeft.isBottom = !0, y.bottomRight.isBottom = !0, y;
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      var u = hu(a, i), f = u.heightOffset, c = u.widthOffset;
      if (Ur(e, r, this.points, s, o, a, i - 2 * f, [0, -1], n) || Ur(e, r, this.points, s, o, a - 2 * c, i, [0, -1], n))
        return !0;
      for (var v = this.generateBarrelBezierPts(a, i, s, o), d = function(S, D, A) {
        var k = A[4], R = A[2], M = A[0], I = A[5], _ = A[1], O = Math.min(k, M), L = Math.max(k, M), N = Math.min(I, _), H = Math.max(I, _);
        if (O <= S && S <= L && N <= D && D <= H) {
          var V = i1(k, R, M), F = Qb(V[0], V[1], V[2], S), $ = F.filter(function(Q) {
            return 0 <= Q && Q <= 1;
          });
          if ($.length > 0)
            return $[0];
        }
        return null;
      }, h = Object.keys(v), y = 0; y < h.length; y++) {
        var g = h[y], p = v[g], m = d(e, r, p);
        if (m != null) {
          var b = p[5], w = p[3], E = p[1], T = bt(b, w, E, m);
          if (p.isTop && T <= r || p.isBottom && r <= T)
            return !0;
        }
      }
      return !1;
    }
  };
};
Wr.generateBottomRoundrectangle = function() {
  return this.nodeShapes["bottom-round-rectangle"] = this.nodeShapes.bottomroundrectangle = {
    renderer: this,
    name: "bottom-round-rectangle",
    points: Nt(4, 0),
    draw: function(e, r, n, a, i, s) {
      this.renderer.nodeShapeImpl(this.name, e, r, n, a, i, this.points, s);
    },
    intersectLine: function(e, r, n, a, i, s, o, l) {
      var u = e - (n / 2 + o), f = r - (a / 2 + o), c = f, v = e + (n / 2 + o), d = tn(i, s, e, r, u, f, v, c, !1);
      return d.length > 0 ? d : lg(i, s, e, r, n, a, o, l);
    },
    checkPoint: function(e, r, n, a, i, s, o, l) {
      l = l === "auto" ? ln(a, i) : l;
      var u = 2 * l;
      if (Ur(e, r, this.points, s, o, a, i - u, [0, -1], n) || Ur(e, r, this.points, s, o, a - u, i, [0, -1], n))
        return !0;
      var f = a / 2 + 2 * n, c = i / 2 + 2 * n, v = [s - f, o - c, s - f, o, s + f, o, s + f, o - c];
      return !!(Wt(e, r, v) || An(e, r, u, u, s + a / 2 - l, o + i / 2 - l, n) || An(e, r, u, u, s - a / 2 + l, o + i / 2 - l, n));
    }
  };
};
Wr.registerNodeShapes = function() {
  var t = this.nodeShapes = {}, e = this;
  this.generateEllipse(), this.generatePolygon("triangle", Nt(3, 0)), this.generateRoundPolygon("round-triangle", Nt(3, 0)), this.generatePolygon("rectangle", Nt(4, 0)), t.square = t.rectangle, this.generateRoundRectangle(), this.generateCutRectangle(), this.generateBarrel(), this.generateBottomRoundrectangle();
  {
    var r = [0, 1, 1, 0, 0, -1, -1, 0];
    this.generatePolygon("diamond", r), this.generateRoundPolygon("round-diamond", r);
  }
  this.generatePolygon("pentagon", Nt(5, 0)), this.generateRoundPolygon("round-pentagon", Nt(5, 0)), this.generatePolygon("hexagon", Nt(6, 0)), this.generateRoundPolygon("round-hexagon", Nt(6, 0)), this.generatePolygon("heptagon", Nt(7, 0)), this.generateRoundPolygon("round-heptagon", Nt(7, 0)), this.generatePolygon("octagon", Nt(8, 0)), this.generateRoundPolygon("round-octagon", Nt(8, 0));
  var n = new Array(20);
  {
    var a = du(5, 0), i = du(5, Math.PI / 5), s = 0.5 * (3 - Math.sqrt(5));
    s *= 1.57;
    for (var o = 0; o < i.length / 2; o++)
      i[o * 2] *= s, i[o * 2 + 1] *= s;
    for (var o = 0; o < 20 / 4; o++)
      n[o * 4] = a[o * 2], n[o * 4 + 1] = a[o * 2 + 1], n[o * 4 + 2] = i[o * 2], n[o * 4 + 3] = i[o * 2 + 1];
  }
  n = ug(n), this.generatePolygon("star", n), this.generatePolygon("vee", [-1, -1, 0, -0.333, 1, -1, 0, 1]), this.generatePolygon("rhomboid", [-1, -1, 0.333, -1, 1, 1, -0.333, 1]), this.generatePolygon("right-rhomboid", [-0.333, -1, 1, -1, 0.333, 1, -1, 1]), this.nodeShapes.concavehexagon = this.generatePolygon("concave-hexagon", [-1, -0.95, -0.75, 0, -1, 0.95, 1, 0.95, 0.75, 0, 1, -0.95]);
  {
    var l = [-1, -1, 0.25, -1, 1, 0, 0.25, 1, -1, 1];
    this.generatePolygon("tag", l), this.generateRoundPolygon("round-tag", l);
  }
  t.makePolygon = function(u) {
    var f = u.join("$"), c = "polygon-" + f, v;
    return (v = this[c]) ? v : e.generatePolygon(c, u);
  };
};
var Di = {};
Di.timeToRender = function() {
  return this.redrawTotalTime / this.redrawCount;
};
Di.redraw = function(t) {
  t = t || ag();
  var e = this;
  e.averageRedrawTime === void 0 && (e.averageRedrawTime = 0), e.lastRedrawTime === void 0 && (e.lastRedrawTime = 0), e.lastDrawTime === void 0 && (e.lastDrawTime = 0), e.requestedFrame = !0, e.renderOptions = t;
};
Di.beforeRender = function(t, e) {
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
var hd = function(e, r, n) {
  for (var a = e.beforeRenderCallbacks, i = 0; i < a.length; i++)
    a[i].fn(r, n);
};
Di.startRenderLoop = function() {
  var t = this, e = t.cy;
  if (!t.renderLoopStarted) {
    t.renderLoopStarted = !0;
    var r = function(a) {
      if (!t.destroyed) {
        if (!e.batching()) if (t.requestedFrame && !t.skipFrame) {
          hd(t, !0, a);
          var i = Hr();
          t.render(t.renderOptions);
          var s = t.lastDrawTime = Hr();
          t.averageRedrawTime === void 0 && (t.averageRedrawTime = s - i), t.redrawCount === void 0 && (t.redrawCount = 0), t.redrawCount++, t.redrawTotalTime === void 0 && (t.redrawTotalTime = 0);
          var o = s - i;
          t.redrawTotalTime += o, t.lastRedrawTime = o, t.averageRedrawTime = t.averageRedrawTime / 2 + o / 2, t.requestedFrame = !1;
        } else
          hd(t, !1, a);
        t.skipFrame = !1, xs(r);
      }
    };
    xs(r);
  }
};
var TE = function(e) {
  this.init(e);
}, cp = TE, Ca = cp.prototype;
Ca.clientFunctions = ["redrawHint", "render", "renderTo", "matchCanvasSize", "nodeShapeImpl", "arrowShapeImpl"];
Ca.init = function(t) {
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
    c === "static" && He("A Cytoscape container has style position:static and so can not use UI extensions properly");
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
Ca.notify = function(t, e) {
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
Ca.destroy = function() {
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
Ca.isHeadless = function() {
  return !1;
};
[gf, lp, up, Ea, Wr, Di].forEach(function(t) {
  Ae(Ca, t);
});
var Wl = 1e3 / 60, vp = {
  setupDequeueing: function(e) {
    return function() {
      var n = this, a = this.renderer;
      if (!n.dequeueingSetup) {
        n.dequeueingSetup = !0;
        var i = Ei(function() {
          a.redrawHint("eles", !0), a.redrawHint("drag", !0), a.redraw();
        }, e.deqRedrawThreshold), s = function(u, f) {
          var c = Hr(), v = a.averageRedrawTime, d = a.lastRedrawTime, h = [], y = a.cy.extent(), g = a.getPixelRatio();
          for (u || a.flushRenderedStyleQueue(); ; ) {
            var p = Hr(), m = p - c, b = p - f;
            if (d < Wl) {
              var w = Wl - (u ? v : 0);
              if (b >= e.deqFastCost * w)
                break;
            } else if (u) {
              if (m >= e.deqCost * d || m >= e.deqAvgCost * v)
                break;
            } else if (b >= e.deqNoDrawCost * Wl)
              break;
            var E = e.deq(n, g, y);
            if (E.length > 0)
              for (var T = 0; T < E.length; T++)
                h.push(E[T]);
            else
              break;
          }
          h.length > 0 && (e.onDeqd(n, h), !u && e.shouldRedraw(n, h, g, y) && i());
        }, o = e.priority || Ju;
        a.beforeRender(s, o(n));
      }
    };
  }
}, SE = /* @__PURE__ */ (function() {
  function t(e) {
    var r = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Es;
    vn(this, t), this.idsByKey = new Fr(), this.keyForId = new Fr(), this.cachesByLvl = new Fr(), this.lvls = [], this.getKey = e, this.doesEleInvalidateKey = r;
  }
  return dn(t, [{
    key: "getIdsFor",
    value: function(r) {
      r == null && je("Can not get id list for null key");
      var n = this.idsByKey, a = this.idsByKey.get(r);
      return a || (a = new ma(), n.set(r, a)), a;
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
      return i || (i = new Fr(), n.set(r, i), a.push(r)), i;
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
})(), gd = 25, Wi = 50, ls = -4, Du = 3, dp = 7.99, PE = 8, DE = 1024, AE = 1024, kE = 1024, BE = 0.2, RE = 0.8, ME = 10, LE = 0.15, IE = 0.1, OE = 0.9, _E = 0.9, NE = 100, FE = 1, ea = {
  dequeue: "dequeue",
  downscale: "downscale",
  highQuality: "highQuality"
}, zE = Dt({
  getKey: null,
  doesEleInvalidateKey: Es,
  drawElement: null,
  getBoundingBox: null,
  getRotationPoint: null,
  getRotationOffset: null,
  isVisible: tg,
  allowEdgeTxrCaching: !0,
  allowParentTxrCaching: !0
}), qa = function(e, r) {
  var n = this;
  n.renderer = e, n.onDequeues = [];
  var a = zE(r);
  Ae(n, a), n.lookup = new SE(a.getKey, a.doesEleInvalidateKey), n.setupDequeueing();
}, pt = qa.prototype;
pt.reasons = ea;
pt.getTextureQueue = function(t) {
  var e = this;
  return e.eleImgCaches = e.eleImgCaches || {}, e.eleImgCaches[t] = e.eleImgCaches[t] || [];
};
pt.getRetiredTextureQueue = function(t) {
  var e = this, r = e.eleImgCaches.retired = e.eleImgCaches.retired || {}, n = r[t] = r[t] || [];
  return n;
};
pt.getElementQueue = function() {
  var t = this, e = t.eleCacheQueue = t.eleCacheQueue || new Ci(function(r, n) {
    return n.reqs - r.reqs;
  });
  return e;
};
pt.getElementKeyToQueue = function() {
  var t = this, e = t.eleKeyToCacheQueue = t.eleKeyToCacheQueue || {};
  return e;
};
pt.getElement = function(t, e, r, n, a) {
  var i = this, s = this.renderer, o = s.cy.zoom(), l = this.lookup;
  if (!e || e.w === 0 || e.h === 0 || isNaN(e.w) || isNaN(e.h) || !t.visible() || t.removed() || !i.allowEdgeTxrCaching && t.isEdge() || !i.allowParentTxrCaching && t.isParent())
    return null;
  if (n == null && (n = Math.ceil(tf(o * r))), n < ls)
    n = ls;
  else if (o >= dp || n > Du)
    return null;
  var u = Math.pow(2, n), f = e.h * u, c = e.w * u, v = s.eleTextBiggerThanMin(t, u);
  if (!this.isVisible(t, v))
    return null;
  var d = l.get(t, n);
  if (d && d.invalidated && (d.invalidated = !1, d.texture.invalidatedWidth -= d.width), d)
    return d;
  var h;
  if (f <= gd ? h = gd : f <= Wi ? h = Wi : h = Math.ceil(f / Wi) * Wi, f > kE || c > AE)
    return null;
  var y = i.getTextureQueue(h), g = y[y.length - 2], p = function() {
    return i.recycleTexture(h, c) || i.addTexture(h, c);
  };
  g || (g = y[y.length - 1]), g || (g = p()), g.width - g.usedWidth < c && (g = p());
  for (var m = function(O) {
    return O && O.scaledLabelShown === v;
  }, b = a && a === ea.dequeue, w = a && a === ea.highQuality, E = a && a === ea.downscale, T, x = n + 1; x <= Du; x++) {
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
        D = i.getElement(t, e, r, k, ea.downscale);
      A();
    } else
      return i.queueElement(t, T.level - 1), T;
  else {
    var R;
    if (!b && !w && !E)
      for (var M = n - 1; M >= ls; M--) {
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
  }, g.usedWidth += Math.ceil(c + PE), g.eleCaches.push(d), l.set(t, n, d), i.checkTextureFullness(g), d;
};
pt.invalidateElements = function(t) {
  for (var e = 0; e < t.length; e++)
    this.invalidateElement(t[e]);
};
pt.invalidateElement = function(t) {
  var e = this, r = e.lookup, n = [], a = r.isInvalid(t);
  if (a) {
    for (var i = ls; i <= Du; i++) {
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
pt.checkTextureUtility = function(t) {
  t.invalidatedWidth >= BE * t.width && this.retireTexture(t);
};
pt.checkTextureFullness = function(t) {
  var e = this, r = e.getTextureQueue(t.height);
  t.usedWidth / t.width > RE && t.fullnessChecks >= ME ? on(r, t) : t.fullnessChecks++;
};
pt.retireTexture = function(t) {
  var e = this, r = t.height, n = e.getTextureQueue(r), a = this.lookup;
  on(n, t), t.retired = !0;
  for (var i = t.eleCaches, s = 0; s < i.length; s++) {
    var o = i[s];
    a.deleteCache(o.key, o.level);
  }
  ef(i);
  var l = e.getRetiredTextureQueue(r);
  l.push(t);
};
pt.addTexture = function(t, e) {
  var r = this, n = r.getTextureQueue(t), a = {};
  return n.push(a), a.eleCaches = [], a.height = t, a.width = Math.max(DE, e), a.usedWidth = 0, a.invalidatedWidth = 0, a.fullnessChecks = 0, a.canvas = r.renderer.makeOffscreenCanvas(a.width, a.height), a.context = a.canvas.getContext("2d"), a;
};
pt.recycleTexture = function(t, e) {
  for (var r = this, n = r.getTextureQueue(t), a = r.getRetiredTextureQueue(t), i = 0; i < a.length; i++) {
    var s = a[i];
    if (s.width >= e)
      return s.retired = !1, s.usedWidth = 0, s.invalidatedWidth = 0, s.fullnessChecks = 0, ef(s.eleCaches), s.context.setTransform(1, 0, 0, 1, 0, 0), s.context.clearRect(0, 0, s.width, s.height), on(a, s), n.push(s), s;
  }
};
pt.queueElement = function(t, e) {
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
pt.dequeue = function(t) {
  for (var e = this, r = e.getElementQueue(), n = e.getElementKeyToQueue(), a = [], i = e.lookup, s = 0; s < FE && r.size() > 0; s++) {
    var o = r.pop(), l = o.key, u = o.eles[0], f = i.hasCache(u, o.level);
    if (n[l] = null, f)
      continue;
    a.push(o);
    var c = e.getBoundingBox(u);
    e.getElement(u, c, t, o.level, ea.dequeue);
  }
  return a;
};
pt.removeFromQueue = function(t) {
  var e = this, r = e.getElementQueue(), n = e.getElementKeyToQueue(), a = this.getKey(t), i = n[a];
  i != null && (i.eles.length === 1 ? (i.reqs = ju, r.updateItem(i), r.pop(), n[a] = null) : i.eles.unmerge(t));
};
pt.onDequeue = function(t) {
  this.onDequeues.push(t);
};
pt.offDequeue = function(t) {
  on(this.onDequeues, t);
};
pt.setupDequeueing = vp.setupDequeueing({
  deqRedrawThreshold: NE,
  deqCost: LE,
  deqAvgCost: IE,
  deqNoDrawCost: OE,
  deqFastCost: _E,
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
        if (nf(l, a))
          return !0;
      }
    return !1;
  },
  priority: function(e) {
    return e.renderer.beforeRenderPriorities.eleTxrDeq;
  }
});
var VE = 1, Xa = -4, Bs = 2, qE = 3.99, $E = 50, HE = 50, UE = 0.15, GE = 0.1, WE = 0.9, KE = 0.9, YE = 1, pd = 250, XE = 4e3 * 4e3, yd = 32767, ZE = !0, hp = function(e) {
  var r = this, n = r.renderer = e, a = n.cy;
  r.layersByLevel = {}, r.firstGet = !0, r.lastInvalidationTime = Hr() - 2 * pd, r.skipping = !1, r.eleTxrDeqs = a.collection(), r.scheduleElementRefinement = Ei(function() {
    r.refineElementTextures(r.eleTxrDeqs), r.eleTxrDeqs.unmerge(r.eleTxrDeqs);
  }, HE), n.beforeRender(function(s, o) {
    o - r.lastInvalidationTime <= pd ? r.skipping = !0 : r.skipping = !1;
  }, n.beforeRenderPriorities.lyrTxrSkip);
  var i = function(o, l) {
    return l.reqs - o.reqs;
  };
  r.layersQueue = new Ci(i), r.setupDequeueing();
}, At = hp.prototype, md = 0, QE = Math.pow(2, 53) - 1;
At.makeLayer = function(t, e) {
  var r = Math.pow(2, e), n = Math.ceil(t.w * r), a = Math.ceil(t.h * r), i = this.renderer.makeOffscreenCanvas(n, a), s = {
    id: md = ++md % QE,
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
At.getLayers = function(t, e, r) {
  var n = this, a = n.renderer, i = a.cy, s = i.zoom(), o = n.firstGet;
  if (n.firstGet = !1, r == null) {
    if (r = Math.ceil(tf(s * e)), r < Xa)
      r = Xa;
    else if (s >= qE || r > Bs)
      return null;
  }
  n.validateLayersElesOrdering(r, t);
  var l = n.layersByLevel, u = Math.pow(2, r), f = l[r] = l[r] || [], c, v = n.levelIsComplete(r, t), d, h = function() {
    var A = function(_) {
      if (n.validateLayersElesOrdering(_, t), n.levelIsComplete(_, t))
        return d = l[_], !0;
    }, k = function(_) {
      if (!d)
        for (var O = r + _; Xa <= O && O <= Bs && !A(O); O += _)
          ;
    };
    k(1), k(-1);
    for (var R = f.length - 1; R >= 0; R--) {
      var M = f[R];
      M.invalid && on(f, M);
    }
  };
  if (!v)
    h();
  else
    return f;
  var y = function() {
    if (!c) {
      c = qt();
      for (var A = 0; A < t.length; A++)
        Gb(c, t[A].boundingBox());
    }
    return c;
  }, g = function(A) {
    A = A || {};
    var k = A.after;
    y();
    var R = Math.ceil(c.w * u), M = Math.ceil(c.h * u);
    if (R > yd || M > yd)
      return null;
    var I = R * M;
    if (I > XE)
      return null;
    var _ = n.makeLayer(c, r);
    if (k != null) {
      var O = f.indexOf(k) + 1;
      f.splice(O, 0, _);
    } else (A.insert === void 0 || A.insert) && f.unshift(_);
    return _;
  };
  if (n.skipping && !o)
    return null;
  for (var p = null, m = t.length / VE, b = !o, w = 0; w < t.length; w++) {
    var E = t[w], T = E._private.rscratch, x = T.imgLayerCaches = T.imgLayerCaches || {}, S = x[r];
    if (S) {
      p = S;
      continue;
    }
    if ((!p || p.eles.length >= m || !og(p.bb, E.boundingBox())) && (p = g({
      insert: !0,
      after: p
    }), !p))
      return null;
    d || b ? n.queueLayer(p, E) : n.drawEleInLayer(p, E, r, e), p.eles.push(E), x[r] = p;
  }
  return d || (b ? null : f);
};
At.getEleLevelForLayerLevel = function(t, e) {
  return t;
};
At.drawEleInLayer = function(t, e, r, n) {
  var a = this, i = this.renderer, s = t.context, o = e.boundingBox();
  o.w === 0 || o.h === 0 || !e.visible() || (r = a.getEleLevelForLayerLevel(r, n), i.setImgSmoothing(s, !1), i.drawCachedElement(s, e, null, null, r, ZE), i.setImgSmoothing(s, !0));
};
At.levelIsComplete = function(t, e) {
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
At.validateLayersElesOrdering = function(t, e) {
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
At.updateElementsInLayers = function(t, e) {
  for (var r = this, n = mi(t[0]), a = 0; a < t.length; a++)
    for (var i = n ? null : t[a], s = n ? t[a] : t[a].ele, o = s._private.rscratch, l = o.imgLayerCaches = o.imgLayerCaches || {}, u = Xa; u <= Bs; u++) {
      var f = l[u];
      f && (i && r.getEleLevelForLayerLevel(f.level) !== i.level || e(f, s, i));
    }
};
At.haveLayers = function() {
  for (var t = this, e = !1, r = Xa; r <= Bs; r++) {
    var n = t.layersByLevel[r];
    if (n && n.length > 0) {
      e = !0;
      break;
    }
  }
  return e;
};
At.invalidateElements = function(t) {
  var e = this;
  t.length !== 0 && (e.lastInvalidationTime = Hr(), !(t.length === 0 || !e.haveLayers()) && e.updateElementsInLayers(t, function(n, a, i) {
    e.invalidateLayer(n);
  }));
};
At.invalidateLayer = function(t) {
  if (this.lastInvalidationTime = Hr(), !t.invalid) {
    var e = t.level, r = t.eles, n = this.layersByLevel[e];
    on(n, t), t.elesQueue = [], t.invalid = !0, t.replacement && (t.replacement.invalid = !0);
    for (var a = 0; a < r.length; a++) {
      var i = r[a]._private.rscratch.imgLayerCaches;
      i && (i[e] = null);
    }
  }
};
At.refineElementTextures = function(t) {
  var e = this;
  e.updateElementsInLayers(t, function(n, a, i) {
    var s = n.replacement;
    if (s || (s = n.replacement = e.makeLayer(n.bb, n.level), s.replaces = n, s.eles = n.eles), !s.reqs)
      for (var o = 0; o < s.eles.length; o++)
        e.queueLayer(s, s.eles[o]);
  });
};
At.enqueueElementRefinement = function(t) {
  this.eleTxrDeqs.merge(t), this.scheduleElementRefinement();
};
At.queueLayer = function(t, e) {
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
At.dequeue = function(t) {
  for (var e = this, r = e.layersQueue, n = [], a = 0; a < YE && r.size() !== 0; ) {
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
At.applyLayerReplacement = function(t) {
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
At.requestRedraw = Ei(function() {
  var t = this.renderer;
  t.redrawHint("eles", !0), t.redrawHint("drag", !0), t.redraw();
}, 100);
At.setupDequeueing = vp.setupDequeueing({
  deqRedrawThreshold: $E,
  deqCost: UE,
  deqAvgCost: GE,
  deqNoDrawCost: WE,
  deqFastCost: KE,
  deq: function(e, r) {
    return e.dequeue(r);
  },
  onDeqd: Ju,
  shouldRedraw: tg,
  priority: function(e) {
    return e.renderer.beforeRenderPriorities.lyrTxrDeq;
  }
});
var gp = {}, bd;
function jE(t, e) {
  for (var r = 0; r < e.length; r++) {
    var n = e[r];
    t.lineTo(n.x, n.y);
  }
}
function JE(t, e, r) {
  for (var n, a = 0; a < e.length; a++) {
    var i = e[a];
    a === 0 && (n = i), t.lineTo(i.x, i.y);
  }
  t.quadraticCurveTo(r.x, r.y, n.x, n.y);
}
function wd(t, e, r) {
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
function eC(t, e, r, n, a) {
  t.beginPath && t.beginPath(), t.arc(r, n, a, 0, Math.PI * 2, !1);
  var i = e, s = i[0];
  t.moveTo(s.x, s.y);
  for (var o = 0; o < i.length; o++) {
    var l = i[o];
    t.lineTo(l.x, l.y);
  }
  t.closePath && t.closePath();
}
function tC(t, e, r, n) {
  t.arc(e, r, n, 0, Math.PI * 2, !1);
}
gp.arrowShapeImpl = function(t) {
  return (bd || (bd = {
    polygon: jE,
    "triangle-backcurve": JE,
    "triangle-tee": wd,
    "circle-triangle": eC,
    "triangle-cross": wd,
    circle: tC
  }))[t];
};
var Ar = {};
Ar.drawElement = function(t, e, r, n, a, i) {
  var s = this;
  e.isNode() ? s.drawNode(t, e, r, n, a, i) : s.drawEdge(t, e, r, n, a, i);
};
Ar.drawElementOverlay = function(t, e) {
  var r = this;
  e.isNode() ? r.drawNodeOverlay(t, e) : r.drawEdgeOverlay(t, e);
};
Ar.drawElementUnderlay = function(t, e) {
  var r = this;
  e.isNode() ? r.drawNodeUnderlay(t, e) : r.drawEdgeUnderlay(t, e);
};
Ar.drawCachedElementPortion = function(t, e, r, n, a, i, s, o) {
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
var rC = function() {
  return 0;
}, nC = function(e, r) {
  return e.getTextAngle(r, null);
}, aC = function(e, r) {
  return e.getTextAngle(r, "source");
}, iC = function(e, r) {
  return e.getTextAngle(r, "target");
}, sC = function(e, r) {
  return r.effectiveOpacity();
}, Kl = function(e, r) {
  return r.pstyle("text-opacity").pfValue * r.effectiveOpacity();
};
Ar.drawCachedElement = function(t, e, r, n, a, i) {
  var s = this, o = s.data, l = o.eleTxrCache, u = o.lblTxrCache, f = o.slbTxrCache, c = o.tlbTxrCache, v = e.boundingBox(), d = i === !0 ? l.reasons.highQuality : null;
  if (!(v.w === 0 || v.h === 0 || !e.visible()) && (!n || nf(v, n))) {
    var h = e.isEdge(), y = e.element()._private.rscratch.badLine;
    s.drawElementUnderlay(t, e), s.drawCachedElementPortion(t, e, l, r, a, d, rC, sC), (!h || !y) && s.drawCachedElementPortion(t, e, u, r, a, d, nC, Kl), h && !y && (s.drawCachedElementPortion(t, e, f, r, a, d, aC, Kl), s.drawCachedElementPortion(t, e, c, r, a, d, iC, Kl)), s.drawElementOverlay(t, e);
  }
};
Ar.drawElements = function(t, e) {
  for (var r = this, n = 0; n < e.length; n++) {
    var a = e[n];
    r.drawElement(t, a);
  }
};
Ar.drawCachedElements = function(t, e, r, n) {
  for (var a = this, i = 0; i < e.length; i++) {
    var s = e[i];
    a.drawCachedElement(t, s, r, n);
  }
};
Ar.drawCachedNodes = function(t, e, r, n) {
  for (var a = this, i = 0; i < e.length; i++) {
    var s = e[i];
    s.isNode() && a.drawCachedElement(t, s, r, n);
  }
};
Ar.drawLayeredElements = function(t, e, r, n) {
  var a = this, i = a.data.lyrTxrCache.getLayers(e, r);
  if (i)
    for (var s = 0; s < i.length; s++) {
      var o = i[s], l = o.bb;
      l.w === 0 || l.h === 0 || t.drawImage(o.canvas, l.x1, l.y1, l.w, l.h);
    }
  else
    a.drawCachedElements(t, e, r, n);
};
var Kr = {};
Kr.drawEdge = function(t, e, r) {
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
var pp = function(e) {
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
Kr.drawEdgeOverlay = pp("overlay");
Kr.drawEdgeUnderlay = pp("underlay");
Kr.drawEdgePath = function(t, e, r, n) {
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
          var y = Kt(a.roundCorners), g;
          try {
            for (y.s(); !(g = y.n()).done; ) {
              var p = g.value;
              np(e, p);
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
Kr.drawEdgeTrianglePath = function(t, e, r) {
  e.fillStyle = e.strokeStyle;
  for (var n = t.pstyle("width").pfValue, a = 0; a + 1 < r.length; a += 2) {
    var i = [r[a + 2] - r[a], r[a + 3] - r[a + 1]], s = Math.sqrt(i[0] * i[0] + i[1] * i[1]), o = [i[1] / s, -i[0] / s], l = [o[0] * n / 2, o[1] * n / 2];
    e.beginPath(), e.moveTo(r[a] - l[0], r[a + 1] - l[1]), e.lineTo(r[a] + l[0], r[a + 1] + l[1]), e.lineTo(r[a + 2], r[a + 3]), e.closePath(), e.fill();
  }
};
Kr.drawArrowheads = function(t, e, r) {
  var n = e._private.rscratch, a = n.edgeType === "haystack";
  a || this.drawArrowhead(t, e, "source", n.arrowStartX, n.arrowStartY, n.srcArrowAngle, r), this.drawArrowhead(t, e, "mid-target", n.midX, n.midY, n.midtgtArrowAngle, r), this.drawArrowhead(t, e, "mid-source", n.midX, n.midY, n.midsrcArrowAngle, r), a || this.drawArrowhead(t, e, "target", n.arrowEndX, n.arrowEndY, n.tgtArrowAngle, r);
};
Kr.drawArrowhead = function(t, e, r, n, a, i, s) {
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
Kr.drawArrowShape = function(t, e, r, n, a, i, s, o, l) {
  var u = this, f = this.usePaths() && a !== "triangle-cross", c = !1, v, d = e, h = {
    x: s,
    y: o
  }, y = t.pstyle("arrow-scale").value, g = this.getArrowWidth(n, y), p = u.arrowShapes[a];
  if (f) {
    var m = u.arrowPathCache = u.arrowPathCache || [], b = Mn(a), w = m[b];
    w != null ? (v = e = w, c = !0) : (v = e = new Path2D(), m[b] = v);
  }
  c || (e.beginPath && e.beginPath(), f ? p.draw(e, 1, 0, {
    x: 0,
    y: 0
  }, 1) : p.draw(e, g, l, h, n), e.closePath && e.closePath()), e = d, f && (e.translate(s, o), e.rotate(l), e.scale(g, g)), (r === "filled" || r === "both") && (f ? e.fill(v) : e.fill()), (r === "hollow" || r === "both") && (e.lineWidth = i / (f ? g : 1), e.lineJoin = "miter", f ? e.stroke(v) : e.stroke()), f && (e.scale(1 / g, 1 / g), e.rotate(-l), e.translate(-s, -o));
};
var mf = {};
mf.safeDrawImage = function(t, e, r, n, a, i, s, o, l, u) {
  if (!(a <= 0 || i <= 0 || l <= 0 || u <= 0))
    try {
      t.drawImage(e, r, n, a, i, s, o, l, u);
    } catch (f) {
      He(f);
    }
};
mf.drawInscribedImage = function(t, e, r, n, a) {
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
    var O = f(r, "background-offset-x", "units", n), L = f(r, "background-offset-x", "pfValue", n);
    O === "%" ? M += (g - A) * L : M += L;
    var N = l - p / 2, H = f(r, "background-position-y", "units", n), V = f(r, "background-position-y", "pfValue", n);
    H === "%" ? N += (p - k) * V : N += V;
    var F = f(r, "background-offset-y", "units", n), $ = f(r, "background-offset-y", "pfValue", n);
    F === "%" ? N += (p - k) * $ : N += $, m.pathCache && (M -= o, N -= l, o = 0, l = 0);
    var Q = t.globalAlpha;
    t.globalAlpha = E;
    var se = i.getImgSmoothing(t), ae = !1;
    if (T === "no" && se ? (i.setImgSmoothing(t, !1), ae = !0) : T === "yes" && !se && (i.setImgSmoothing(t, !0), ae = !0), v === "no-repeat")
      w && (t.save(), m.pathCache ? t.clip(m.pathCache) : (i.nodeShapes[i.getNodeShape(r)].draw(t, o, l, g, p, x, m), t.clip())), i.safeDrawImage(t, e, 0, 0, S, D, M, N, A, k), w && t.restore();
    else {
      var le = t.createPattern(e, v);
      t.fillStyle = le, i.nodeShapes[i.getNodeShape(r)].draw(t, o, l, g, p, x, m), t.translate(M, N), t.fill(), t.translate(-M, -N);
    }
    t.globalAlpha = Q, ae && i.setImgSmoothing(t, se);
  }
};
var Vn = {};
Vn.eleTextBiggerThanMin = function(t, e) {
  if (!e) {
    var r = t.cy().zoom(), n = this.getPixelRatio(), a = Math.ceil(tf(r * n));
    e = Math.pow(2, a);
  }
  var i = t.pstyle("font-size").pfValue * e, s = t.pstyle("min-zoomed-font-size").pfValue;
  return !(i < s);
};
Vn.drawElementText = function(t, e, r, n, a) {
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
Vn.getFontCache = function(t) {
  var e;
  this.fontCaches = this.fontCaches || [];
  for (var r = 0; r < this.fontCaches.length; r++)
    if (e = this.fontCaches[r], e.context === t)
      return e;
  return e = {
    context: t
  }, this.fontCaches.push(e), e;
};
Vn.setupTextStyle = function(t, e) {
  var r = arguments.length > 2 && arguments[2] !== void 0 ? arguments[2] : !0, n = e.pstyle("font-style").strValue, a = e.pstyle("font-size").pfValue + "px", i = e.pstyle("font-family").strValue, s = e.pstyle("font-weight").strValue, o = r ? e.effectiveOpacity() * e.pstyle("text-opacity").value : 1, l = e.pstyle("text-outline-opacity").value * o, u = e.pstyle("color").value, f = e.pstyle("text-outline-color").value;
  t.font = n + " " + s + " " + a + " " + i, t.lineJoin = "round", this.colorFillStyle(t, u[0], u[1], u[2], o), this.colorStrokeStyle(t, f[0], f[1], f[2], l);
};
function oC(t, e, r, n, a) {
  var i = Math.min(n, a), s = i / 2, o = e + n / 2, l = r + a / 2;
  t.beginPath(), t.arc(o, l, s, 0, Math.PI * 2), t.closePath();
}
function xd(t, e, r, n, a) {
  var i = arguments.length > 5 && arguments[5] !== void 0 ? arguments[5] : 5, s = Math.min(i, n / 2, a / 2);
  t.beginPath(), t.moveTo(e + s, r), t.lineTo(e + n - s, r), t.quadraticCurveTo(e + n, r, e + n, r + s), t.lineTo(e + n, r + a - s), t.quadraticCurveTo(e + n, r + a, e + n - s, r + a), t.lineTo(e + s, r + a), t.quadraticCurveTo(e, r + a, e, r + a - s), t.lineTo(e, r + s), t.quadraticCurveTo(e, r, e + s, r), t.closePath();
}
Vn.getTextAngle = function(t, e) {
  var r, n = t._private, a = n.rscratch, i = e ? e + "-" : "", s = t.pstyle(i + "text-rotation");
  if (s.strValue === "autorotate") {
    var o = Ft(a, "labelAngle", e);
    r = t.isEdge() ? o : 0;
  } else s.strValue === "none" ? r = 0 : r = s.pfValue;
  return r;
};
Vn.drawText = function(t, e, r) {
  var n = arguments.length > 3 && arguments[3] !== void 0 ? arguments[3] : !0, a = arguments.length > 4 && arguments[4] !== void 0 ? arguments[4] : !0, i = e._private, s = i.rscratch, o = a ? e.effectiveOpacity() : 1;
  if (!(a && (o === 0 || e.pstyle("text-opacity").value === 0))) {
    r === "main" && (r = null);
    var l = Ft(s, "labelX", r), u = Ft(s, "labelY", r), f, c, v = this.getLabelText(e, r);
    if (v != null && v !== "" && !isNaN(l) && !isNaN(u)) {
      this.setupTextStyle(t, e, a);
      var d = r ? r + "-" : "", h = Ft(s, "labelWidth", r), y = Ft(s, "labelHeight", r), g = Ft(s, "labelActualDescent", r), p = e.pstyle(d + "text-margin-x").pfValue, m = e.pstyle(d + "text-margin-y").pfValue, b = e.isEdge(), w = e.pstyle("text-halign").value, E = e.pstyle("text-valign").value;
      b && (w = "center", E = "center"), l += p, u += m;
      var T;
      n ? T = this.getTextAngle(e, r) : T = 0, T !== 0 && (f = l, c = u, t.translate(f, c), t.rotate(T), l = 0, u = 0);
      var x = pa(w), S = ya(E);
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
      var D = e.pstyle("text-background-opacity").value, A = e.pstyle("text-border-opacity").value, k = e.pstyle("text-border-width").pfValue, R = e.pstyle("text-background-padding").pfValue, M = e.pstyle("text-background-shape").strValue, I = M === "round-rectangle" || M === "roundrectangle", _ = M === "circle", O = 2;
      if (D > 0 || k > 0 && A > 0) {
        var L = t.fillStyle, N = t.strokeStyle, H = t.lineWidth, V = e.pstyle("text-background-color").value, F = e.pstyle("text-border-color").value, $ = e.pstyle("text-border-style").value, Q = D > 0, se = k > 0 && A > 0, ae = l - R;
        switch (x) {
          case "left":
            ae -= h;
            break;
          case "center":
            ae -= h / 2;
            break;
        }
        var le = u - y - R, ce = h + 2 * R, he = y + 2 * R;
        if (Q && (t.fillStyle = "rgba(".concat(V[0], ",").concat(V[1], ",").concat(V[2], ",").concat(D * o, ")")), se && (t.strokeStyle = "rgba(".concat(F[0], ",").concat(F[1], ",").concat(F[2], ",").concat(A * o, ")"), t.lineWidth = k, t.setLineDash))
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
        if (I ? (t.beginPath(), xd(t, ae, le, ce, he, O)) : _ ? (t.beginPath(), oC(t, ae, le, ce, he)) : (t.beginPath(), t.rect(ae, le, ce, he)), Q && t.fill(), se && t.stroke(), se && $ === "double") {
          var ie = k / 2;
          t.beginPath(), I ? xd(t, ae + ie, le + ie, ce - 2 * ie, he - 2 * ie, O) : t.rect(ae + ie, le + ie, ce - 2 * ie, he - 2 * ie), t.stroke();
        }
        t.fillStyle = L, t.strokeStyle = N, t.lineWidth = H, t.setLineDash && t.setLineDash([]);
      }
      var U = 2 * e.pstyle("text-outline-width").pfValue;
      if (U > 0 && (t.lineWidth = U), u -= g, e.pstyle("text-wrap").value === "wrap") {
        var X = Ft(s, "labelWrapCachedLines", r), C = Ft(s, "labelLineHeight", r), B = h / 2, z = this.getLabelJustification(e);
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
var gn = {};
gn.drawNode = function(t, e, r) {
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
    var k = e.pstyle("background-blacken").value, R = e.pstyle("border-width").pfValue, M = e.pstyle("background-opacity").value * v, I = e.pstyle("border-color").value, _ = e.pstyle("border-style").value, O = e.pstyle("border-join").value, L = e.pstyle("border-cap").value, N = e.pstyle("border-position").value, H = e.pstyle("border-dash-pattern").pfValue, V = e.pstyle("border-dash-offset").pfValue, F = e.pstyle("border-opacity").value * v, $ = e.pstyle("outline-width").pfValue, Q = e.pstyle("outline-color").value, se = e.pstyle("outline-style").value, ae = e.pstyle("outline-opacity").value * v, le = e.pstyle("outline-offset").value, ce = e.pstyle("corner-radius").value;
    ce !== "auto" && (ce = e.pstyle("corner-radius").pfValue);
    var he = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : M;
      s.eleFillStyle(t, e, P);
    }, ie = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : F;
      s.colorStrokeStyle(t, I[0], I[1], I[2], P);
    }, U = function() {
      var P = arguments.length > 0 && arguments[0] !== void 0 ? arguments[0] : ae;
      s.colorStrokeStyle(t, Q[0], Q[1], Q[2], P);
    }, X = function(P, q, G, re) {
      var ee = s.nodePathCache = s.nodePathCache || [], ye = eg(G === "polygon" ? G + "," + re.join(",") : G, "" + q, "" + P, "" + ce), fe = ee[ye], ge, be = !1;
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
        if (t.lineWidth = R, t.lineCap = L, t.lineJoin = O, t.setLineDash)
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
        if (N !== "center") {
          if (t.save(), t.lineWidth *= 2, N === "inside")
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
        N === "inside" && (G = 0), N === "outside" && (G *= 2);
        var re = (o + G + ($ + le)) / o, ee = (l + G + ($ + le)) / l, ye = o * re, fe = l * ee, ge = s.nodeShapes[q].points, be;
        if (d) {
          var De = X(ye, fe, q, ge);
          be = De.path;
        }
        if (q === "ellipse")
          s.drawEllipsePath(be || t, P.x, P.y, ye, fe);
        else if (["round-diamond", "round-heptagon", "round-hexagon", "round-octagon", "round-pentagon", "round-polygon", "round-triangle", "round-tag"].includes(q)) {
          var Be = 0, Me = 0, xe = 0;
          q === "round-diamond" ? Be = (G + le + $) * 1.4 : q === "round-heptagon" ? (Be = (G + le + $) * 1.075, xe = -(G / 2 + le + $) / 35) : q === "round-hexagon" ? Be = (G + le + $) * 1.12 : q === "round-pentagon" ? (Be = (G + le + $) * 1.13, xe = -(G / 2 + le + $) / 15) : q === "round-tag" ? (Be = (G + le + $) * 1.12, Me = (G / 2 + $ + le) * 0.07) : q === "round-triangle" && (Be = (G + le + $) * (Math.PI / 2), xe = -(G + le / 2 + $) / Math.PI), Be !== 0 && (re = (o + Be) / o, ye = o * re, ["round-hexagon", "round-tag"].includes(q) || (ee = (l + Be) / l, fe = l * ee)), ce = ce === "auto" ? fg(ye, fe) : ce;
          for (var Oe = ye / 2, _e = fe / 2, yt = ce + (G + $ + le) / 2, rt = new Array(ge.length / 2), nr = new Array(ge.length / 2), nt = 0; nt < ge.length / 2; nt++)
            rt[nt] = {
              x: P.x + Me + Oe * ge[nt * 2],
              y: P.y + xe + _e * ge[nt * 2 + 1]
            };
          var ot, vt, Et, dr, Je = rt.length;
          for (vt = rt[Je - 1], ot = 0; ot < Je; ot++)
            Et = rt[ot % Je], dr = rt[(ot + 1) % Je], nr[ot] = pf(vt, Et, dr, yt), vt = Et, Et = dr;
          s.drawRoundPolygonPath(be || t, P.x + Me, P.y + xe, o * re, l * ee, ge, nr);
        } else if (["roundrectangle", "round-rectangle"].includes(q))
          ce = ce === "auto" ? ln(ye, fe) : ce, s.drawRoundRectanglePath(be || t, P.x, P.y, ye, fe, ce + (G + $ + le) / 2);
        else if (["cutrectangle", "cut-rectangle"].includes(q))
          ce = ce === "auto" ? af() : ce, s.drawCutRectanglePath(be || t, P.x, P.y, ye, fe, null, ce + (G + $ + le) / 4);
        else if (["bottomroundrectangle", "bottom-round-rectangle"].includes(q))
          ce = ce === "auto" ? ln(ye, fe) : ce, s.drawBottomRoundRectanglePath(be || t, P.x, P.y, ye, fe, ce + (G + $ + le) / 2);
        else if (q === "barrel")
          s.drawBarrelPath(be || t, P.x, P.y, ye, fe);
        else if (q.startsWith("polygon") || ["rhomboid", "right-rhomboid", "round-tag", "tag", "vee"].includes(q)) {
          var kr = (G + $ + le) / o;
          ge = Cs(Ts(ge, kr)), s.drawPolygonPath(be || t, P.x, P.y, o, l, ge);
        } else {
          var ar = (G + $ + le) / o;
          ge = Cs(Ts(ge, -ar)), s.drawPolygonPath(be || t, P.x, P.y, o, l, ge);
        }
        if (d ? t.stroke(be) : t.stroke(), se === "double") {
          t.lineWidth = G / 3;
          var Yr = t.globalCompositeOperation;
          t.globalCompositeOperation = "destination-out", d ? t.stroke(be) : t.stroke(), t.globalCompositeOperation = Yr;
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
      t.translate(me, Te), U(), K(), he(Ee * M), W(), j(Pe, !0), ie(Ee * F), Y(), Z(k !== 0 || R !== 0), ne(k !== 0 || R !== 0), j(Pe, !1), te(Pe), t.translate(-me, -Te);
    }
    d && t.translate(-c.x, -c.y), oe(), d && t.translate(c.x, c.y), U(), K(), he(), W(), j(v, !0), ie(), Y(), Z(k !== 0 || R !== 0), ne(k !== 0 || R !== 0), j(v, !1), te(), d && t.translate(-c.x, -c.y), ve(), ue(), r && t.translate(p.x1, p.y1);
  }
};
var yp = function(e) {
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
gn.drawNodeOverlay = yp("overlay");
gn.drawNodeUnderlay = yp("underlay");
gn.hasPie = function(t) {
  return t = t[0], t._private.hasPie;
};
gn.hasStripe = function(t) {
  return t = t[0], t._private.hasStripe;
};
gn.drawPie = function(t, e, r, n) {
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
gn.drawStripe = function(t, e, r, n) {
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
var $t = {}, lC = 100;
$t.getPixelRatio = function() {
  var t = this.data.contexts[0];
  if (this.forcedPixelRatio != null)
    return this.forcedPixelRatio;
  var e = this.cy.window(), r = t.backingStorePixelRatio || t.webkitBackingStorePixelRatio || t.mozBackingStorePixelRatio || t.msBackingStorePixelRatio || t.oBackingStorePixelRatio || t.backingStorePixelRatio || 1;
  return (e.devicePixelRatio || 1) / r;
};
$t.paintCache = function(t) {
  for (var e = this.paintCaches = this.paintCaches || [], r = !0, n, a = 0; a < e.length; a++)
    if (n = e[a], n.context === t) {
      r = !1;
      break;
    }
  return r && (n = {
    context: t
  }, e.push(n)), n;
};
$t.createGradientStyleFor = function(t, e, r, n, a) {
  var i, s = this.usePaths(), o = r.pstyle(e + "-gradient-stop-colors").value, l = r.pstyle(e + "-gradient-stop-positions").pfValue;
  if (n === "radial-gradient")
    if (r.isEdge()) {
      var u = r.sourceEndpoint(), f = r.targetEndpoint(), c = r.midpoint(), v = Ln(u, c), d = Ln(f, c);
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
$t.gradientFillStyle = function(t, e, r, n) {
  var a = this.createGradientStyleFor(t, "background", e, r, n);
  if (!a) return null;
  t.fillStyle = a;
};
$t.colorFillStyle = function(t, e, r, n, a) {
  t.fillStyle = "rgba(" + e + "," + r + "," + n + "," + a + ")";
};
$t.eleFillStyle = function(t, e, r) {
  var n = e.pstyle("background-fill").value;
  if (n === "linear-gradient" || n === "radial-gradient")
    this.gradientFillStyle(t, e, n, r);
  else {
    var a = e.pstyle("background-color").value;
    this.colorFillStyle(t, a[0], a[1], a[2], r);
  }
};
$t.gradientStrokeStyle = function(t, e, r, n) {
  var a = this.createGradientStyleFor(t, "line", e, r, n);
  if (!a) return null;
  t.strokeStyle = a;
};
$t.colorStrokeStyle = function(t, e, r, n, a) {
  t.strokeStyle = "rgba(" + e + "," + r + "," + n + "," + a + ")";
};
$t.eleStrokeStyle = function(t, e, r) {
  var n = e.pstyle("line-fill").value;
  if (n === "linear-gradient" || n === "radial-gradient")
    this.gradientStrokeStyle(t, e, n, r);
  else {
    var a = e.pstyle("line-color").value;
    this.colorStrokeStyle(t, a[0], a[1], a[2], r);
  }
};
$t.matchCanvasSize = function(t) {
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
$t.renderTo = function(t, e, r, n) {
  this.render({
    forcedContext: t,
    forcedZoom: e,
    forcedPan: r,
    drawAllLayers: !0,
    forcedPxRatio: n
  });
};
$t.clearCanvas = function() {
  var t = this, e = t.data;
  function r(n) {
    n.clearRect(0, 0, t.canvasWidth, t.canvasHeight);
  }
  r(e.contexts[t.NODE]), r(e.contexts[t.DRAG]);
};
$t.render = function(t) {
  var e = this;
  t = t || ag();
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
    var O = m.core("outside-texture-bg-color").value, L = m.core("outside-texture-bg-opacity").value;
    e.colorFillStyle(I, O[0], O[1], O[2], L), I.fillRect(0, 0, x.width, x.height);
    var b = r.zoom();
    R(I, !1), I.clearRect(x.mpan.x, x.mpan.y, x.width / x.zoom / l, x.height / x.zoom / l), I.drawImage(_, x.mpan.x, x.mpan.y, x.width / x.zoom / l, x.height / x.zoom / l);
  } else e.textureOnViewport && !n && (e.textureCache = null);
  var N = r.extent(), H = e.pinching || e.hoverData.dragging || e.swipePanning || e.data.wheelZooming || e.hoverData.draggingEles || e.cy.animated(), V = e.hideEdgesOnViewport && H, F = [];
  if (F[e.NODE] = !f[e.NODE] && v && !e.clearedForMotionBlur[e.NODE] || e.clearingMotionBlur, F[e.NODE] && (e.clearedForMotionBlur[e.NODE] = !0), F[e.DRAG] = !f[e.DRAG] && v && !e.clearedForMotionBlur[e.DRAG] || e.clearingMotionBlur, F[e.DRAG] && (e.clearedForMotionBlur[e.DRAG] = !0), f[e.NODE] || a || i || F[e.NODE]) {
    var $ = v && !F[e.NODE] && d !== 1, I = n || ($ ? e.data.bufferContexts[e.MOTIONBLUR_BUFFER_NODE] : u.contexts[e.NODE]), Q = v && !$ ? "motionBlur" : void 0;
    R(I, Q), V ? e.drawCachedNodes(I, A.nondrag, l, N) : e.drawLayeredElements(I, A.nondrag, l, N), e.debug && e.drawDebugPoints(I, A.nondrag), !a && !v && (f[e.NODE] = !1);
  }
  if (!i && (f[e.DRAG] || a || F[e.DRAG])) {
    var $ = v && !F[e.DRAG] && d !== 1, I = n || ($ ? e.data.bufferContexts[e.MOTIONBLUR_BUFFER_DRAG] : u.contexts[e.DRAG]);
    R(I, v && !$ ? "motionBlur" : void 0), V ? e.drawCachedNodes(I, A.drag, l, N) : e.drawCachedElements(I, A.drag, l, N), e.debug && e.drawDebugPoints(I, A.drag), !a && !v && (f[e.DRAG] = !1);
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
    (f[e.NODE] || F[e.NODE]) && (he(se, ae, F[e.NODE]), f[e.NODE] = !1), (f[e.DRAG] || F[e.DRAG]) && (he(le, ce, F[e.DRAG]), f[e.DRAG] = !1);
  }
  e.prevViewport = x, e.clearingMotionBlur && (e.clearingMotionBlur = !1, e.motionBlurCleared = !0, e.motionBlur = !0), v && (e.motionBlurTimeout = setTimeout(function() {
    e.motionBlurTimeout = null, e.clearedForMotionBlur[e.NODE] = !1, e.clearedForMotionBlur[e.DRAG] = !1, e.motionBlur = !1, e.clearingMotionBlur = !c, e.mbFrames = 0, f[e.NODE] = !0, f[e.DRAG] = !0, e.redraw();
  }, lC)), n || r.emit("render");
};
var Ia;
$t.drawSelectionRectangle = function(t, e) {
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
      if (f.setTransform(1, 0, 0, 1, 0, 0), f.fillStyle = "rgba(255, 0, 0, 0.75)", f.strokeStyle = "rgba(255, 0, 0, 0.75)", f.font = "30px Arial", !Ia) {
        var p = f.measureText(g);
        Ia = p.actualBoundingBoxAscent;
      }
      f.fillText(g, 0, Ia);
      var m = 60;
      f.strokeRect(0, Ia + 10, 250, 20), f.fillRect(0, Ia + 10, 250 * Math.min(y / m, 1), 20);
    }
    o || (l[r.SELECT_BOX] = !1);
  }
};
function Ed(t, e, r) {
  var n = t.createShader(e);
  if (t.shaderSource(n, r), t.compileShader(n), !t.getShaderParameter(n, t.COMPILE_STATUS))
    throw new Error(t.getShaderInfoLog(n));
  return n;
}
function uC(t, e, r) {
  var n = Ed(t, t.VERTEX_SHADER, e), a = Ed(t, t.FRAGMENT_SHADER, r), i = t.createProgram();
  if (t.attachShader(i, n), t.attachShader(i, a), t.linkProgram(i), !t.getProgramParameter(i, t.LINK_STATUS))
    throw new Error("Could not initialize shaders");
  return i;
}
function fC(t, e, r) {
  r === void 0 && (r = e);
  var n = t.makeOffscreenCanvas(e, r), a = n.context = n.getContext("2d");
  return n.clear = function() {
    return a.clearRect(0, 0, n.width, n.height);
  }, n.clear(), n;
}
function bf(t) {
  var e = t.pixelRatio, r = t.cy.zoom(), n = t.cy.pan();
  return {
    zoom: r * e,
    pan: {
      x: n.x * e,
      y: n.y * e
    }
  };
}
function cC(t) {
  var e = t.pixelRatio, r = t.cy.zoom();
  return r * e;
}
function vC(t, e, r, n, a) {
  var i = n * r + e.x, s = a * r + e.y;
  return s = Math.round(t.canvasHeight - s), [i, s];
}
function dC(t, e) {
  return e.picking ? !0 : t.pstyle("background-fill").value !== "solid" || t.pstyle("background-image").strValue !== "none" ? !1 : t.pstyle("border-width").value === 0 || t.pstyle("border-opacity").value === 0 ? !0 : t.pstyle("border-style").value === "solid";
}
function hC(t, e) {
  if (t.length !== e.length)
    return !1;
  for (var r = 0; r < t.length; r++)
    if (t[r] !== e[r])
      return !1;
  return !0;
}
function xn(t, e, r) {
  var n = t[0] / 255, a = t[1] / 255, i = t[2] / 255, s = e, o = r || new Array(4);
  return o[0] = n * s, o[1] = a * s, o[2] = i * s, o[3] = s, o;
}
function Kn(t, e) {
  var r = e || new Array(4);
  return r[0] = (t >> 0 & 255) / 255, r[1] = (t >> 8 & 255) / 255, r[2] = (t >> 16 & 255) / 255, r[3] = (t >> 24 & 255) / 255, r;
}
function gC(t) {
  return t[0] + (t[1] << 8) + (t[2] << 16) + (t[3] << 24);
}
function pC(t, e) {
  var r = t.createTexture();
  return r.buffer = function(n) {
    t.bindTexture(t.TEXTURE_2D, r), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_WRAP_S, t.CLAMP_TO_EDGE), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_WRAP_T, t.CLAMP_TO_EDGE), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_MAG_FILTER, t.LINEAR), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_MIN_FILTER, t.LINEAR_MIPMAP_NEAREST), t.pixelStorei(t.UNPACK_PREMULTIPLY_ALPHA_WEBGL, !0), t.texImage2D(t.TEXTURE_2D, 0, t.RGBA, t.RGBA, t.UNSIGNED_BYTE, n), t.generateMipmap(t.TEXTURE_2D), t.bindTexture(t.TEXTURE_2D, null);
  }, r.deleteTexture = function() {
    t.deleteTexture(r);
  }, r;
}
function mp(t, e) {
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
function bp(t, e, r) {
  switch (e) {
    case t.FLOAT:
      return new Float32Array(r);
    case t.INT:
      return new Int32Array(r);
  }
}
function yC(t, e, r, n, a, i) {
  switch (e) {
    case t.FLOAT:
      return new Float32Array(r.buffer, i * n, a);
    case t.INT:
      return new Int32Array(r.buffer, i * n, a);
  }
}
function mC(t, e, r, n) {
  var a = mp(t, e), i = ct(a, 2), s = i[0], o = i[1], l = bp(t, o, n), u = t.createBuffer();
  return t.bindBuffer(t.ARRAY_BUFFER, u), t.bufferData(t.ARRAY_BUFFER, l, t.STATIC_DRAW), o === t.FLOAT ? t.vertexAttribPointer(r, s, o, !1, 0, 0) : o === t.INT && t.vertexAttribIPointer(r, s, o, 0, 0), t.enableVertexAttribArray(r), t.bindBuffer(t.ARRAY_BUFFER, null), u;
}
function pr(t, e, r, n) {
  var a = mp(t, r), i = ct(a, 3), s = i[0], o = i[1], l = i[2], u = bp(t, o, e * s), f = s * l, c = t.createBuffer();
  t.bindBuffer(t.ARRAY_BUFFER, c), t.bufferData(t.ARRAY_BUFFER, e * f, t.DYNAMIC_DRAW), t.enableVertexAttribArray(n), o === t.FLOAT ? t.vertexAttribPointer(n, s, o, !1, f, 0) : o === t.INT && t.vertexAttribIPointer(n, s, o, f, 0), t.vertexAttribDivisor(n, 1), t.bindBuffer(t.ARRAY_BUFFER, null);
  for (var v = new Array(e), d = 0; d < e; d++)
    v[d] = yC(t, o, u, f, s, d);
  return c.dataArray = u, c.stride = f, c.size = s, c.getView = function(h) {
    return v[h];
  }, c.setPoint = function(h, y, g) {
    var p = v[h];
    p[0] = y, p[1] = g;
  }, c.bufferSubData = function(h) {
    t.bindBuffer(t.ARRAY_BUFFER, c), h ? t.bufferSubData(t.ARRAY_BUFFER, 0, u, 0, h * s) : t.bufferSubData(t.ARRAY_BUFFER, 0, u);
  }, c;
}
function bC(t, e, r) {
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
function wC(t) {
  var e = t.createFramebuffer();
  t.bindFramebuffer(t.FRAMEBUFFER, e);
  var r = t.createTexture();
  return t.bindTexture(t.TEXTURE_2D, r), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_MIN_FILTER, t.LINEAR), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_WRAP_S, t.CLAMP_TO_EDGE), t.texParameteri(t.TEXTURE_2D, t.TEXTURE_WRAP_T, t.CLAMP_TO_EDGE), t.framebufferTexture2D(t.FRAMEBUFFER, t.COLOR_ATTACHMENT0, t.TEXTURE_2D, r, 0), t.bindFramebuffer(t.FRAMEBUFFER, null), e.setFramebufferAttachmentSizes = function(n, a) {
    t.bindTexture(t.TEXTURE_2D, r), t.texImage2D(t.TEXTURE_2D, 0, t.RGBA, n, a, 0, t.RGBA, t.UNSIGNED_BYTE, null);
  }, e;
}
var Cd = typeof Float32Array < "u" ? Float32Array : Array;
Math.hypot || (Math.hypot = function() {
  for (var t = 0, e = arguments.length; e--; )
    t += arguments[e] * arguments[e];
  return Math.sqrt(t);
});
function Yl() {
  var t = new Cd(9);
  return Cd != Float32Array && (t[1] = 0, t[2] = 0, t[3] = 0, t[5] = 0, t[6] = 0, t[7] = 0), t[0] = 1, t[4] = 1, t[8] = 1, t;
}
function Td(t) {
  return t[0] = 1, t[1] = 0, t[2] = 0, t[3] = 0, t[4] = 1, t[5] = 0, t[6] = 0, t[7] = 0, t[8] = 1, t;
}
function xC(t, e, r) {
  var n = e[0], a = e[1], i = e[2], s = e[3], o = e[4], l = e[5], u = e[6], f = e[7], c = e[8], v = r[0], d = r[1], h = r[2], y = r[3], g = r[4], p = r[5], m = r[6], b = r[7], w = r[8];
  return t[0] = v * n + d * s + h * u, t[1] = v * a + d * o + h * f, t[2] = v * i + d * l + h * c, t[3] = y * n + g * s + p * u, t[4] = y * a + g * o + p * f, t[5] = y * i + g * l + p * c, t[6] = m * n + b * s + w * u, t[7] = m * a + b * o + w * f, t[8] = m * i + b * l + w * c, t;
}
function us(t, e, r) {
  var n = e[0], a = e[1], i = e[2], s = e[3], o = e[4], l = e[5], u = e[6], f = e[7], c = e[8], v = r[0], d = r[1];
  return t[0] = n, t[1] = a, t[2] = i, t[3] = s, t[4] = o, t[5] = l, t[6] = v * n + d * s + u, t[7] = v * a + d * o + f, t[8] = v * i + d * l + c, t;
}
function Sd(t, e, r) {
  var n = e[0], a = e[1], i = e[2], s = e[3], o = e[4], l = e[5], u = e[6], f = e[7], c = e[8], v = Math.sin(r), d = Math.cos(r);
  return t[0] = d * n + v * s, t[1] = d * a + v * o, t[2] = d * i + v * l, t[3] = d * s - v * n, t[4] = d * o - v * a, t[5] = d * l - v * i, t[6] = u, t[7] = f, t[8] = c, t;
}
function Au(t, e, r) {
  var n = r[0], a = r[1];
  return t[0] = n * e[0], t[1] = n * e[1], t[2] = n * e[2], t[3] = a * e[3], t[4] = a * e[4], t[5] = a * e[5], t[6] = e[6], t[7] = e[7], t[8] = e[8], t;
}
function EC(t, e, r) {
  return t[0] = 2 / e, t[1] = 0, t[2] = 0, t[3] = 0, t[4] = -2 / r, t[5] = 0, t[6] = -1, t[7] = 1, t[8] = 1, t;
}
var CC = /* @__PURE__ */ (function() {
  function t(e, r, n, a) {
    vn(this, t), this.debugID = Math.floor(Math.random() * 1e4), this.r = e, this.texSize = r, this.texRows = n, this.texHeight = Math.floor(r / n), this.enableWrapping = !0, this.locked = !1, this.texture = null, this.needsBuffer = !0, this.freePointer = {
      x: 0,
      row: 0
    }, this.keyToLocation = /* @__PURE__ */ new Map(), this.canvas = a(e, r, r), this.scratch = a(e, r, this.texHeight, "scratch");
  }
  return dn(t, [{
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
      this.texture || (this.texture = pC(r, this.debugID)), this.needsBuffer && (this.texture.buffer(this.canvas), this.needsBuffer = !1, this.locked && (this.canvas = null, this.scratch = null));
    }
  }, {
    key: "dispose",
    value: function() {
      this.texture && (this.texture.deleteTexture(), this.texture = null), this.canvas = null, this.scratch = null, this.locked = !0;
    }
  }]);
})(), TC = /* @__PURE__ */ (function() {
  function t(e, r, n, a) {
    vn(this, t), this.r = e, this.texSize = r, this.texRows = n, this.createTextureCanvas = a, this.atlases = [], this.styleKeyToAtlas = /* @__PURE__ */ new Map(), this.markedKeys = /* @__PURE__ */ new Set();
  }
  return dn(t, [{
    key: "getKeys",
    value: function() {
      return new Set(this.styleKeyToAtlas.keys());
    }
  }, {
    key: "_createAtlas",
    value: function() {
      var r = this.r, n = this.texSize, a = this.texRows, i = this.createTextureCanvas;
      return new CC(r, n, a, i);
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
      var a = [], i = /* @__PURE__ */ new Map(), s = null, o = Kt(this.atlases), l;
      try {
        var u = function() {
          var c = l.value, v = c.getKeys(), d = SC(n, v);
          if (d.size === 0)
            return a.push(c), v.forEach(function(E) {
              return i.set(E, c);
            }), 1;
          s || (s = r._createAtlas(), a.push(s));
          var h = Kt(v), y;
          try {
            for (h.s(); !(y = h.n()).done; ) {
              var g = y.value;
              if (!d.has(g)) {
                var p = c.getOffsets(g), m = ct(p, 2), b = m[0], w = m[1];
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
      var i = n.getOffsets(r), s = ct(i, 2), o = s[0], l = s[1];
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
function SC(t, e) {
  return t.intersection ? t.intersection(e) : new Set(bs(t).filter(function(r) {
    return e.has(r);
  }));
}
var PC = /* @__PURE__ */ (function() {
  function t(e, r) {
    vn(this, t), this.r = e, this.globalOptions = r, this.atlasSize = r.webglTexSize, this.maxAtlasesPerBatch = r.webglTexPerBatch, this.renderTypes = /* @__PURE__ */ new Map(), this.collections = /* @__PURE__ */ new Map(), this.typeAndIdToKey = /* @__PURE__ */ new Map();
  }
  return dn(t, [{
    key: "getAtlasSize",
    value: function() {
      return this.atlasSize;
    }
  }, {
    key: "addAtlasCollection",
    value: function(r, n) {
      var a = this.globalOptions, i = a.webglTexSize, s = a.createTextureCanvas, o = n.texRows, l = this._cacheScratchCanvas(s), u = new TC(this.r, i, o, l);
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
      } : u, c = !1, v = !1, d = Kt(r), h;
      try {
        for (d.s(); !(h = d.n()).done; ) {
          var y = h.value;
          if (l(y)) {
            var g = Kt(this.renderTypes.values()), p;
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
                    k !== void 0 && !hC(S, k) && (c = !0, n.typeAndIdToKey.delete(A), k.forEach(function(R) {
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
      var r = Kt(this.collections.values()), n;
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
        var u = i.getBoundingBox(r, l), f = a.getOrCreateAtlas(r, n, u, l), c = f.getOffsets(l), v = ct(c, 2), d = v[0], h = v[1];
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
      var r = [], n = Kt(this.collections), a;
      try {
        for (n.s(); !(a = n.n()).done; ) {
          var i = ct(a.value, 2), s = i[0], o = i[1], l = o.getCounts(), u = l.keyCount, f = l.atlasCount;
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
})(), DC = /* @__PURE__ */ (function() {
  function t(e) {
    vn(this, t), this.globalOptions = e, this.atlasSize = e.webglTexSize, this.maxAtlasesPerBatch = e.webglTexPerBatch, this.batchAtlases = [];
  }
  return dn(t, [{
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
})(), AC = `
  float circleSD(vec2 p, float r) {
    return distance(vec2(0), p) - r; // signed distance
  }
`, kC = `
  float rectangleSD(vec2 p, vec2 b) {
    vec2 d = abs(p)-b;
    return distance(vec2(0),max(d,0.0)) + min(max(d.x,d.y),0.0);
  }
`, BC = `
  float roundRectangleSD(vec2 p, vec2 b, vec4 cr) {
    cr.xy = (p.x > 0.0) ? cr.xy : cr.zw;
    cr.x  = (p.y > 0.0) ? cr.x  : cr.y;
    vec2 q = abs(p) - b + cr.x;
    return min(max(q.x, q.y), 0.0) + distance(vec2(0), max(q, 0.0)) - cr.x;
  }
`, RC = `
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
`, Za = {
  SCREEN: {
    name: "screen",
    screen: !0
  },
  PICKING: {
    name: "picking",
    picking: !0
  }
}, Rs = {
  // render the texture just like in RENDER_TARGET.SCREEN mode
  IGNORE: 1,
  // don't render the texture at all
  USE_BB: 2
  // render the bounding box as an opaque rectangle
}, Xl = 0, Pd = 1, Dd = 2, Zl = 3, Yn = 4, Ki = 5, Oa = 6, _a = 7, MC = /* @__PURE__ */ (function() {
  function t(e, r, n) {
    vn(this, t), this.r = e, this.gl = r, this.maxInstances = n.webglBatchSize, this.atlasSize = n.webglTexSize, this.bgColor = n.bgColor, this.debug = n.webglDebug, this.batchDebugInfo = [], n.enableWrapping = !0, n.createTextureCanvas = fC, this.atlasManager = new PC(e, n), this.batchManager = new DC(n), this.simpleShapeOptions = /* @__PURE__ */ new Map(), this.program = this._createShaderProgram(Za.SCREEN), this.pickingProgram = this._createShaderProgram(Za.PICKING), this.vao = this._createVAO();
  }
  return dn(t, [{
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

        if(aVertType == `.concat(Xl, `) {
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
        else if(aVertType == `).concat(Yn, " || aVertType == ").concat(_a, ` 
             || aVertType == `).concat(Ki, " || aVertType == ").concat(Oa, `) { // simple shapes

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
        else if(aVertType == `).concat(Pd, `) {
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
        else if(aVertType == `).concat(Dd, `) {
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
        else if(aVertType == `).concat(Zl, ` && vid < 3) {
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

      `).concat(AC, `
      `).concat(kC, `
      `).concat(BC, `
      `).concat(RC, `

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
        if(vVertType == `).concat(Xl, `) {
          // look up the texel from the texture unit
          `).concat(i.map(function(u) {
        return "if(vAtlasId == ".concat(u, ") outColor = texture(uTexture").concat(u, ", vTexCoord);");
      }).join(`
	else `), `
        } 
        else if(vVertType == `).concat(Zl, `) {
          // mimics how canvas renderer uses context.globalCompositeOperation = 'destination-out';
          outColor = blend(vColor, uBGColor);
          outColor.a = 1.0; // make opaque, masks out line under arrow
        }
        else if(vVertType == `).concat(Yn, ` && vBorderWidth == vec2(0.0)) { // simple rectangle with no border
          outColor = vColor; // unit square is already transformed to the rectangle, nothing else needs to be done
        }
        else if(vVertType == `).concat(Yn, " || vVertType == ").concat(_a, ` 
          || vVertType == `).concat(Ki, " || vVertType == ").concat(Oa, `) { // use SDF

          float outerBorder = vBorderWidth[0];
          float innerBorder = vBorderWidth[1];
          float borderPadding = outerBorder * 2.0;
          float w = vTopRight.x - vBotLeft.x - borderPadding;
          float h = vTopRight.y - vBotLeft.y - borderPadding;
          vec2 b = vec2(w/2.0, h/2.0); // half width, half height
          vec2 p = vPosition - vec2(vTopRight.x - b[0] - outerBorder, vTopRight.y - b[1] - outerBorder); // translate to center

          float d; // signed distance
          if(vVertType == `).concat(Yn, `) {
            d = rectangleSD(p, b);
          } else if(vVertType == `).concat(_a, ` && w == h) {
            d = circleSD(p, b.x); // faster than ellipse
          } else if(vVertType == `).concat(_a, `) {
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
    `), o = uC(n, a, s);
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
      return a.bindVertexArray(s), mC(a, "vec2", i.aPosition, r), this.transformBuffer = bC(a, n, i.aTransform), this.indexBuffer = pr(a, n, "vec4", i.aIndex), this.vertTypeBuffer = pr(a, n, "int", i.aVertType), this.atlasIdBuffer = pr(a, n, "int", i.aAtlasId), this.texBuffer = pr(a, n, "vec4", i.aTex), this.pointAPointBBuffer = pr(a, n, "vec4", i.aPointAPointB), this.pointCPointDBuffer = pr(a, n, "vec4", i.aPointCPointD), this.lineWidthBuffer = pr(a, n, "vec2", i.aLineWidth), this.colorBuffer = pr(a, n, "vec4", i.aColor), this.cornerRadiusBuffer = pr(a, n, "vec4", i.aCornerRadius), this.borderColorBuffer = pr(a, n, "vec4", i.aBorderColor), a.bindVertexArray(null), s;
    }
  }, {
    key: "buffers",
    get: function() {
      var r = this;
      return this._buffers || (this._buffers = Object.keys(this).filter(function(n) {
        return Jr(n, "Buffer");
      }).map(function(n) {
        return r[n];
      })), this._buffers;
    }
  }, {
    key: "startFrame",
    value: function(r) {
      var n = arguments.length > 1 && arguments[1] !== void 0 ? arguments[1] : Za.SCREEN;
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
          if (l === Rs.IGNORE)
            return;
          if (l == Rs.USE_BB) {
            this.drawPickingRectangle(r, n, a);
            return;
          }
        }
        var u = i.getAtlasInfo(r, a), f = Kt(u), c;
        try {
          for (f.s(); !(c = f.n()).done; ) {
            var v = c.value, d = v.atlas, h = v.tex1, y = v.tex2;
            s.canAddToCurrentBatch(d) || this.endBatch();
            for (var g = s.getAtlasIndexForBatch(d), p = 0, m = [[h, !0], [y, !1]]; p < m.length; p++) {
              var b = ct(m[p], 2), w = b[0], E = b[1];
              if (w.w != 0) {
                var T = this.instanceCount;
                this.vertTypeBuffer.getView(T)[0] = Xl;
                var x = this.indexBuffer.getView(T);
                Kn(n, x);
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
      Td(r);
      var l = a.getRotation ? a.getRotation(i) : 0;
      if (l !== 0) {
        var u = a.getRotationPoint(i), f = u.x, c = u.y;
        us(r, r, [f, c]), Sd(r, r, l);
        var v = a.getRotationOffset(i);
        s = v.x + (n.xOffset || 0), o = v.y + (n.yOffset || 0);
      } else
        s = n.x1, o = n.y1;
      us(r, r, [s, o]), Au(r, r, [n.w, n.h]);
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
      this.vertTypeBuffer.getView(s)[0] = Yn;
      var o = this.indexBuffer.getView(s);
      Kn(n, o);
      var l = this.colorBuffer.getView(s);
      xn([0, 0, 0], 1, l);
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
        if (this.vertTypeBuffer.getView(l)[0] = o, o === Ki || o === Oa) {
          var u = i.getBoundingBox(r), f = this._getCornerRadius(r, s.radius, u), c = this.cornerRadiusBuffer.getView(l);
          c[0] = f, c[1] = f, c[2] = f, c[3] = f, o === Oa && (c[0] = 0, c[2] = 0);
        }
        var v = this.indexBuffer.getView(l);
        Kn(n, v);
        var d = this.renderTarget.picking ? 1 : a === "node-body" ? r.effectiveOpacity() : 1, h = this.renderTarget.picking ? 1 : r.pstyle(s.opacity).value * d, y = r.pstyle(s.color).value, g = this.colorBuffer.getView(l);
        xn(y, h, g);
        var p = this.lineWidthBuffer.getView(l);
        if (p[0] = 0, p[1] = 0, s.border) {
          var m = r.pstyle("border-width").value;
          if (m > 0) {
            var b = r.pstyle("border-color").value, w = d * r.pstyle("border-opacity").value, E = this.borderColorBuffer.getView(l);
            xn(b, w, E);
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
          return Yn;
        case "ellipse":
          return _a;
        case "roundrectangle":
        case "round-rectangle":
          return Ki;
        case "bottom-round-rectangle":
          return Oa;
        default:
          return;
      }
    }
  }, {
    key: "_getCornerRadius",
    value: function(r, n, a) {
      var i = a.w, s = a.h;
      if (r.pstyle(n).value === "auto")
        return ln(i, s);
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
            Td(m), us(m, m, [s, o]), Au(m, m, [g, g]), Sd(m, m, l), this.vertTypeBuffer.getView(p)[0] = Zl;
            var b = this.indexBuffer.getView(p);
            Kn(n, b);
            var w = this.colorBuffer.getView(p);
            xn(f, d, w), this.instanceCount++, this.instanceCount >= this.maxInstances && this.endBatch();
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
            this.vertTypeBuffer.getView(f)[0] = Pd;
            var c = this.indexBuffer.getView(f);
            Kn(n, c);
            var v = this.colorBuffer.getView(f);
            xn(l, u, v);
            var d = this.lineWidthBuffer.getView(f);
            d[0] = o;
            var h = this.pointAPointBBuffer.getView(f);
            h[0] = a[0], h[1] = a[1], h[2] = a[2], h[3] = a[3], this.instanceCount++, this.instanceCount >= this.maxInstances && this.endBatch();
          } else
            for (var y = 0; y < a.length - 2; y += 2) {
              var g = this.instanceCount;
              this.vertTypeBuffer.getView(g)[0] = Dd;
              var p = this.indexBuffer.getView(g);
              Kn(n, p);
              var m = this.colorBuffer.getView(g);
              xn(l, u, m);
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
        var o = Kt(this.buffers), l;
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
        r.uniform1f(s.uZoom, cC(this.r)), r.uniformMatrix3fv(s.uPanZoomMatrix, !1, this.panZoomMatrix), r.uniform1i(s.uAtlasSize, this.batchManager.getAtlasSize());
        var d = xn(this.bgColor, 1);
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
})(), wp = {};
wp.initWebgl = function(t, e) {
  var r = this, n = r.data.contexts[r.WEBGL];
  t.bgColor = LC(r), t.webglTexSize = Math.min(t.webglTexSize, n.getParameter(n.MAX_TEXTURE_SIZE)), t.webglTexRows = Math.min(t.webglTexRows, 54), t.webglTexRowsNodes = Math.min(t.webglTexRowsNodes, 54), t.webglBatchSize = Math.min(t.webglBatchSize, 16384), t.webglTexPerBatch = Math.min(t.webglTexPerBatch, n.getParameter(n.MAX_TEXTURE_IMAGE_UNITS)), r.webglDebug = t.webglDebug, r.webglDebugShowAtlases = t.webglDebugShowAtlases, r.pickingFrameBuffer = wC(n), r.pickingFrameBuffer.needsDraw = !0, r.drawing = new MC(r, n, t);
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
    return v ? Rs.USE_BB : Rs.IGNORE;
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
    isSimple: dC,
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
    getKey: Ql(e.getLabelKey, null),
    getBoundingBox: jl(e.getLabelBox, null),
    drawClipped: !0,
    drawElement: e.drawLabel,
    getRotation: a(null),
    getRotationPoint: e.getLabelRotationPoint,
    getRotationOffset: e.getLabelRotationOffset,
    isVisible: i("label")
  }), r.drawing.addTextureAtlasRenderType("edge-source-label", {
    collection: "label",
    getTexPickingMode: o,
    getKey: Ql(e.getSourceLabelKey, "source"),
    getBoundingBox: jl(e.getSourceLabelBox, "source"),
    drawClipped: !0,
    drawElement: e.drawSourceLabel,
    getRotation: a("source"),
    getRotationPoint: e.getSourceLabelRotationPoint,
    getRotationOffset: e.getSourceLabelRotationOffset,
    isVisible: i("source-label")
  }), r.drawing.addTextureAtlasRenderType("edge-target-label", {
    collection: "label",
    getTexPickingMode: o,
    getKey: Ql(e.getTargetLabelKey, "target"),
    getBoundingBox: jl(e.getTargetLabelBox, "target"),
    drawClipped: !0,
    drawElement: e.drawTargetLabel,
    getRotation: a("target"),
    getRotationPoint: e.getTargetLabelRotationPoint,
    getRotationOffset: e.getTargetLabelRotationOffset,
    isVisible: i("target-label")
  });
  var u = Ei(function() {
    console.log("garbage collect flag set"), r.data.gc = !0;
  }, 1e4);
  r.onUpdateEleCalcs(function(f, c) {
    var v = !1;
    c && c.length > 0 && (v |= r.drawing.invalidate(c)), v && u();
  }), IC(r);
};
function LC(t) {
  var e = t.cy.container(), r = e && e.style && e.style.backgroundColor || "white";
  return Kh(r);
}
function xp(t, e) {
  var r = t._private.rscratch;
  return Ft(r, "labelWrapCachedLines", e) || [];
}
var Ql = function(e, r) {
  return function(n) {
    var a = e(n), i = xp(n, r);
    return i.length > 1 ? i.map(function(s, o) {
      return "".concat(a, "_").concat(o);
    }) : a;
  };
}, jl = function(e, r) {
  return function(n, a) {
    var i = e(n);
    if (typeof a == "string") {
      var s = a.indexOf("_");
      if (s > 0) {
        var o = Number(a.substring(s + 1)), l = xp(n, r), u = i.h / l.length, f = u * o, c = i.y1 + f;
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
function IC(t) {
  {
    var e = t.render;
    t.render = function(i) {
      i = i || {};
      var s = t.cy;
      t.webgl && (s.zoom() > dp ? (OC(t), e.call(t, i)) : (_C(t), Cp(t, i, Za.SCREEN)));
    };
  }
  {
    var r = t.matchCanvasSize;
    t.matchCanvasSize = function(i) {
      r.call(t, i), t.pickingFrameBuffer.setFramebufferAttachmentSizes(t.canvasWidth, t.canvasHeight), t.pickingFrameBuffer.needsDraw = !0;
    };
  }
  t.findNearestElements = function(i, s, o, l) {
    return $C(t, i, s);
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
function OC(t) {
  var e = t.data.contexts[t.WEBGL];
  e.clear(e.COLOR_BUFFER_BIT | e.DEPTH_BUFFER_BIT);
}
function _C(t) {
  var e = function(n) {
    n.save(), n.setTransform(1, 0, 0, 1, 0, 0), n.clearRect(0, 0, t.canvasWidth, t.canvasHeight), n.restore();
  };
  e(t.data.contexts[t.NODE]), e(t.data.contexts[t.DRAG]);
}
function NC(t) {
  var e = t.canvasWidth, r = t.canvasHeight, n = bf(t), a = n.pan, i = n.zoom, s = Yl();
  us(s, s, [a.x, a.y]), Au(s, s, [i, i]);
  var o = Yl();
  EC(o, e, r);
  var l = Yl();
  return xC(l, o, s), l;
}
function Ep(t, e) {
  var r = t.canvasWidth, n = t.canvasHeight, a = bf(t), i = a.pan, s = a.zoom;
  e.setTransform(1, 0, 0, 1, 0, 0), e.clearRect(0, 0, r, n), e.translate(i.x, i.y), e.scale(s, s);
}
function FC(t, e) {
  t.drawSelectionRectangle(e, function(r) {
    return Ep(t, r);
  });
}
function zC(t) {
  var e = t.data.contexts[t.NODE];
  e.save(), Ep(t, e), e.strokeStyle = "rgba(0, 0, 0, 0.3)", e.beginPath(), e.moveTo(-1e3, 0), e.lineTo(1e3, 0), e.stroke(), e.beginPath(), e.moveTo(0, -1e3), e.lineTo(0, 1e3), e.stroke(), e.restore();
}
function VC(t) {
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
function qC(t, e, r, n, a) {
  var i, s, o, l, u = bf(t), f = u.pan, c = u.zoom;
  {
    var v = vC(t, f, c, e, r), d = ct(v, 2), h = d[0], y = d[1], g = 6;
    i = h - g / 2, s = y - g / 2, o = g, l = g;
  }
  if (o === 0 || l === 0)
    return [];
  var p = t.data.contexts[t.WEBGL];
  p.bindFramebuffer(p.FRAMEBUFFER, t.pickingFrameBuffer), t.pickingFrameBuffer.needsDraw && (p.viewport(0, 0, p.canvas.width, p.canvas.height), Cp(t, null, Za.PICKING), t.pickingFrameBuffer.needsDraw = !1);
  var m = o * l, b = new Uint8Array(m * 4);
  p.readPixels(i, s, o, l, p.RGBA, p.UNSIGNED_BYTE, b), p.bindFramebuffer(p.FRAMEBUFFER, null);
  for (var w = /* @__PURE__ */ new Set(), E = 0; E < m; E++) {
    var T = b.slice(E * 4, E * 4 + 4), x = gC(T) - 1;
    x >= 0 && w.add(x);
  }
  return w;
}
function $C(t, e, r) {
  var n = qC(t, e, r), a = t.getCachedZSortedEles(), i, s, o = Kt(n), l;
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
function Jl(t, e, r) {
  var n = t.drawing;
  e += 1, r.isNode() ? (n.drawNode(r, e, "node-underlay"), n.drawNode(r, e, "node-body"), n.drawTexture(r, e, "label"), n.drawNode(r, e, "node-overlay")) : (n.drawEdgeLine(r, e), n.drawEdgeArrow(r, e, "source"), n.drawEdgeArrow(r, e, "target"), n.drawTexture(r, e, "label"), n.drawTexture(r, e, "edge-source-label"), n.drawTexture(r, e, "edge-target-label"));
}
function Cp(t, e, r) {
  var n;
  t.webglDebug && (n = performance.now());
  var a = t.drawing, i = 0;
  if (r.screen && t.data.canvasNeedsRedraw[t.SELECT_BOX] && FC(t, e), t.data.canvasNeedsRedraw[t.NODE] || r.picking) {
    var s = t.data.contexts[t.WEBGL];
    r.screen ? (s.clearColor(0, 0, 0, 0), s.enable(s.BLEND), s.blendFunc(s.ONE, s.ONE_MINUS_SRC_ALPHA)) : s.disable(s.BLEND), s.clear(s.COLOR_BUFFER_BIT | s.DEPTH_BUFFER_BIT), s.viewport(0, 0, s.canvas.width, s.canvas.height);
    var o = NC(t), l = t.getCachedZSortedEles();
    if (i = l.length, a.startFrame(o, r), r.screen) {
      for (var u = 0; u < l.nondrag.length; u++)
        Jl(t, u, l.nondrag[u]);
      for (var f = 0; f < l.drag.length; f++)
        Jl(t, f, l.drag[f]);
    } else if (r.picking)
      for (var c = 0; c < l.length; c++)
        Jl(t, c, l[c]);
    a.endFrame(), r.screen && t.webglDebugShowAtlases && (zC(t), VC(t)), t.data.canvasNeedsRedraw[t.NODE] = !1, t.data.canvasNeedsRedraw[t.DRAG] = !1;
  }
  if (t.webglDebug) {
    var v = performance.now(), d = !1, h = Math.ceil(v - n), y = a.getDebugInfo(), g = ["".concat(i, " elements"), "".concat(y.totalInstances, " instances"), "".concat(y.batchCount, " batches"), "".concat(y.totalAtlases, " atlases"), "".concat(y.wrappedCount, " wrapped textures"), "".concat(y.simpleCount, " simple shapes")].join(", ");
    if (d)
      console.log("WebGL (".concat(r.name, ") - time ").concat(h, "ms, ").concat(g));
    else {
      console.log("WebGL (".concat(r.name, ") - frame time ").concat(h, "ms")), console.log("Totals:"), console.log("  ".concat(g)), console.log("Texture Atlases Used:");
      var p = y.atlasInfo, m = Kt(p), b;
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
var pn = {};
pn.drawPolygonPath = function(t, e, r, n, a, i) {
  var s = n / 2, o = a / 2;
  t.beginPath && t.beginPath(), t.moveTo(e + s * i[0], r + o * i[1]);
  for (var l = 1; l < i.length / 2; l++)
    t.lineTo(e + s * i[l * 2], r + o * i[l * 2 + 1]);
  t.closePath();
};
pn.drawRoundPolygonPath = function(t, e, r, n, a, i, s) {
  s.forEach(function(o) {
    return np(t, o);
  }), t.closePath();
};
pn.drawRoundRectanglePath = function(t, e, r, n, a, i) {
  var s = n / 2, o = a / 2, l = i === "auto" ? ln(n, a) : Math.min(i, o, s);
  t.beginPath && t.beginPath(), t.moveTo(e, r - o), t.arcTo(e + s, r - o, e + s, r, l), t.arcTo(e + s, r + o, e, r + o, l), t.arcTo(e - s, r + o, e - s, r, l), t.arcTo(e - s, r - o, e, r - o, l), t.lineTo(e, r - o), t.closePath();
};
pn.drawBottomRoundRectanglePath = function(t, e, r, n, a, i) {
  var s = n / 2, o = a / 2, l = i === "auto" ? ln(n, a) : i;
  t.beginPath && t.beginPath(), t.moveTo(e, r - o), t.lineTo(e + s, r - o), t.lineTo(e + s, r), t.arcTo(e + s, r + o, e, r + o, l), t.arcTo(e - s, r + o, e - s, r, l), t.lineTo(e - s, r - o), t.lineTo(e, r - o), t.closePath();
};
pn.drawCutRectanglePath = function(t, e, r, n, a, i, s) {
  var o = n / 2, l = a / 2, u = s === "auto" ? af() : s;
  t.beginPath && t.beginPath(), t.moveTo(e - o + u, r - l), t.lineTo(e + o - u, r - l), t.lineTo(e + o, r - l + u), t.lineTo(e + o, r + l - u), t.lineTo(e + o - u, r + l), t.lineTo(e - o + u, r + l), t.lineTo(e - o, r + l - u), t.lineTo(e - o, r - l + u), t.closePath();
};
pn.drawBarrelPath = function(t, e, r, n, a) {
  var i = n / 2, s = a / 2, o = e - i, l = e + i, u = r - s, f = r + s, c = hu(n, a), v = c.widthOffset, d = c.heightOffset, h = c.ctrlPtOffsetPct * v;
  t.beginPath && t.beginPath(), t.moveTo(o, u + d), t.lineTo(o, f - d), t.quadraticCurveTo(o + h, f, o + v, f), t.lineTo(l - v, f), t.quadraticCurveTo(l - h, f, l, f - d), t.lineTo(l, u + d), t.quadraticCurveTo(l - h, u, l - v, u), t.lineTo(o + v, u), t.quadraticCurveTo(o + h, u, o, u + d), t.closePath();
};
var Ad = Math.sin(0), kd = Math.cos(0), ku = {}, Bu = {}, Tp = Math.PI / 40;
for (var Xn = 0 * Math.PI; Xn < 2 * Math.PI; Xn += Tp)
  ku[Xn] = Math.sin(Xn), Bu[Xn] = Math.cos(Xn);
pn.drawEllipsePath = function(t, e, r, n, a) {
  if (t.beginPath && t.beginPath(), t.ellipse)
    t.ellipse(e, r, n / 2, a / 2, 0, 0, 2 * Math.PI);
  else
    for (var i, s, o = n / 2, l = a / 2, u = 0 * Math.PI; u < 2 * Math.PI; u += Tp)
      i = e - o * ku[u] * Ad + o * Bu[u] * kd, s = r + l * Bu[u] * Ad + l * ku[u] * kd, u === 0 ? t.moveTo(i, s) : t.lineTo(i, s);
  t.closePath();
};
var Ai = {};
Ai.createBuffer = function(t, e) {
  var r = document.createElement("canvas");
  return r.width = t, r.height = e, [r, r.getContext("2d")];
};
Ai.bufferCanvasImage = function(t) {
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
function HC(t, e) {
  for (var r = atob(t), n = new ArrayBuffer(r.length), a = new Uint8Array(n), i = 0; i < r.length; i++)
    a[i] = r.charCodeAt(i);
  return new Blob([n], {
    type: e
  });
}
function Bd(t) {
  var e = t.indexOf(",");
  return t.substr(e + 1);
}
function Sp(t, e, r) {
  var n = function() {
    return e.toDataURL(r, t.quality);
  };
  switch (t.output) {
    case "blob-promise":
      return new ba(function(a, i) {
        try {
          e.toBlob(function(s) {
            s != null ? a(s) : i(new Error("`canvas.toBlob()` sent a null value in its callback"));
          }, r, t.quality);
        } catch (s) {
          i(s);
        }
      });
    case "blob":
      return HC(Bd(n()), r);
    case "base64":
      return Bd(n());
    case "base64uri":
    default:
      return n();
  }
}
Ai.png = function(t) {
  return Sp(t, this.bufferCanvasImage(t), "image/png");
};
Ai.jpg = function(t) {
  return Sp(t, this.bufferCanvasImage(t), "image/jpeg");
};
var Pp = {};
Pp.nodeShapeImpl = function(t, e, r, n, a, i, s, o) {
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
var UC = Dp, Ie = Dp.prototype;
Ie.CANVAS_LAYERS = 3;
Ie.SELECT_BOX = 0;
Ie.DRAG = 1;
Ie.NODE = 2;
Ie.WEBGL = 3;
Ie.CANVAS_TYPES = ["2d", "2d", "2d", "webgl2"];
Ie.BUFFER_COUNT = 3;
Ie.TEXTURE_BUFFER = 0;
Ie.MOTIONBLUR_BUFFER_NODE = 1;
Ie.MOTIONBLUR_BUFFER_DRAG = 2;
function Dp(t) {
  var e = this, r = e.cy.window(), n = r.document;
  t.webgl && (Ie.CANVAS_LAYERS = e.CANVAS_LAYERS = 4, console.log("webgl rendering enabled")), e.data = {
    canvases: new Array(Ie.CANVAS_LAYERS),
    contexts: new Array(Ie.CANVAS_LAYERS),
    canvasNeedsRedraw: new Array(Ie.CANVAS_LAYERS),
    bufferCanvases: new Array(Ie.BUFFER_COUNT),
    bufferContexts: new Array(Ie.CANVAS_LAYERS)
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
  O0() && (l["-ms-touch-action"] = "none", l["touch-action"] = "none");
  for (var u = 0; u < Ie.CANVAS_LAYERS; u++) {
    var f = e.data.canvases[u] = n.createElement("canvas"), c = Ie.CANVAS_TYPES[u];
    e.data.contexts[u] = f.getContext(c), e.data.contexts[u] || je("Could not create canvas of type " + c), Object.keys(l).forEach(function(ie) {
      f.style[ie] = l[ie];
    }), f.style.position = "absolute", f.setAttribute("data-id", "layer" + u), f.style.zIndex = String(Ie.CANVAS_LAYERS - u), e.data.canvasContainer.appendChild(f), e.data.canvasNeedsRedraw[u] = !1;
  }
  e.data.topCanvas = e.data.canvases[0], e.data.canvases[Ie.NODE].setAttribute("data-id", "layer" + Ie.NODE + "-node"), e.data.canvases[Ie.SELECT_BOX].setAttribute("data-id", "layer" + Ie.SELECT_BOX + "-selectbox"), e.data.canvases[Ie.DRAG].setAttribute("data-id", "layer" + Ie.DRAG + "-drag"), e.data.canvases[Ie.WEBGL] && e.data.canvases[Ie.WEBGL].setAttribute("data-id", "layer" + Ie.WEBGL + "-webgl");
  for (var u = 0; u < Ie.BUFFER_COUNT; u++)
    e.data.bufferCanvases[u] = n.createElement("canvas"), e.data.bufferContexts[u] = e.data.bufferCanvases[u].getContext("2d"), e.data.bufferCanvases[u].style.position = "absolute", e.data.bufferCanvases[u].setAttribute("data-id", "buffer" + u), e.data.bufferCanvases[u].style.zIndex = String(-u - 1), e.data.bufferCanvases[u].style.visibility = "hidden";
  e.pathsEnabled = !0;
  var v = qt(), d = function(U) {
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
  }, O = function(U) {
    return I("", _(U, "labelX", "labelY"), U);
  }, L = function(U) {
    return I("source", _(U, "sourceLabelX", "sourceLabelY"), U);
  }, N = function(U) {
    return I("target", _(U, "targetLabelX", "targetLabelY"), U);
  }, H = function(U) {
    return h(S(U));
  }, V = function(U) {
    return h(A(U));
  }, F = function(U) {
    return h(k(U));
  }, $ = function(U) {
    var X = D(U), C = h(D(U));
    if (U.isNode()) {
      switch (pa(U.pstyle("text-halign").value)) {
        case "left":
          C.x = -X.w - (X.leftPad || 0);
          break;
        case "right":
          C.x = -(X.rightPad || 0);
          break;
      }
      switch (ya(U.pstyle("text-valign").value)) {
        case "top":
          C.y = -X.h - (X.topPad || 0);
          break;
        case "bottom":
          C.y = -(X.botPad || 0);
          break;
      }
    }
    return C;
  }, Q = e.data.eleTxrCache = new qa(e, {
    getKey: g,
    doesEleInvalidateKey: y,
    drawElement: w,
    getBoundingBox: S,
    getRotationPoint: M,
    getRotationOffset: H,
    allowEdgeTxrCaching: !1,
    allowParentTxrCaching: !1
  }), se = e.data.lblTxrCache = new qa(e, {
    getKey: p,
    drawElement: E,
    getBoundingBox: D,
    getRotationPoint: O,
    getRotationOffset: $,
    isVisible: R
  }), ae = e.data.slbTxrCache = new qa(e, {
    getKey: m,
    drawElement: T,
    getBoundingBox: A,
    getRotationPoint: L,
    getRotationOffset: V,
    isVisible: R
  }), le = e.data.tlbTxrCache = new qa(e, {
    getKey: b,
    drawElement: x,
    getBoundingBox: k,
    getRotationPoint: N,
    getRotationOffset: F,
    isVisible: R
  }), ce = e.data.lyrTxrCache = new hp(e);
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
    getLabelRotationPoint: O,
    getSourceLabelRotationPoint: L,
    getTargetLabelRotationPoint: N,
    getLabelRotationOffset: $,
    getSourceLabelRotationOffset: V,
    getTargetLabelRotationOffset: F
  });
}
Ie.redrawHint = function(t, e) {
  var r = this;
  switch (t) {
    case "eles":
      r.data.canvasNeedsRedraw[Ie.NODE] = e;
      break;
    case "drag":
      r.data.canvasNeedsRedraw[Ie.DRAG] = e;
      break;
    case "select":
      r.data.canvasNeedsRedraw[Ie.SELECT_BOX] = e;
      break;
    case "gc":
      r.data.gc = !0;
      break;
  }
};
var GC = typeof Path2D < "u";
Ie.path2dEnabled = function(t) {
  if (t === void 0)
    return this.pathsEnabled;
  this.pathsEnabled = !!t;
};
Ie.usePaths = function() {
  return GC && this.pathsEnabled;
};
Ie.setImgSmoothing = function(t, e) {
  t.imageSmoothingEnabled != null ? t.imageSmoothingEnabled = e : (t.webkitImageSmoothingEnabled = e, t.mozImageSmoothingEnabled = e, t.msImageSmoothingEnabled = e);
};
Ie.getImgSmoothing = function(t) {
  return t.imageSmoothingEnabled != null ? t.imageSmoothingEnabled : t.webkitImageSmoothingEnabled || t.mozImageSmoothingEnabled || t.msImageSmoothingEnabled;
};
Ie.makeOffscreenCanvas = function(t, e) {
  var r;
  if ((typeof OffscreenCanvas > "u" ? "undefined" : gt(OffscreenCanvas)) !== "undefined")
    r = new OffscreenCanvas(t, e);
  else {
    var n = this.cy.window(), a = n.document;
    r = a.createElement("canvas"), r.width = t, r.height = e;
  }
  return r;
};
[gp, Ar, Kr, mf, Vn, gn, $t, wp, pn, Ai, Pp].forEach(function(t) {
  Ae(Ie, t);
});
var WC = [{
  name: "null",
  impl: ep
}, {
  name: "base",
  impl: cp
}, {
  name: "canvas",
  impl: UC
}], KC = [{
  type: "layout",
  extensions: pE
}, {
  type: "renderer",
  extensions: WC
}], Ap = {}, kp = {};
function Bp(t, e, r) {
  var n = r, a = function(S) {
    He("Can not register `" + e + "` for `" + t + "` since `" + S + "` already exists in the prototype and can not be overridden");
  };
  if (t === "core") {
    if (ci.prototype[e])
      return a(e);
    ci.prototype[e] = r;
  } else if (t === "collection") {
    if (St.prototype[e])
      return a(e);
    St.prototype[e] = r;
  } else if (t === "layout") {
    for (var i = function(S) {
      this.options = S, r.call(this, S), Fe(this._private) || (this._private = {}), this._private.cy = S.cy, this._private.listeners = [], this.createEmitter();
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
        return this._private.emitter = new ro(v, this), this;
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
    }), $e.eventAliasesOn(s), n = i;
  } else if (t === "renderer" && e !== "null" && e !== "base") {
    var d = Rp("renderer", "base"), h = d.prototype, y = r, g = r.prototype, p = function() {
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
  return Yh({
    map: Ap,
    keys: [t, e],
    value: n
  });
}
function Rp(t, e) {
  return Xh({
    map: Ap,
    keys: [t, e]
  });
}
function YC(t, e, r, n, a) {
  return Yh({
    map: kp,
    keys: [t, e, r, n],
    value: a
  });
}
function XC(t, e, r, n) {
  return Xh({
    map: kp,
    keys: [t, e, r, n]
  });
}
var Ru = function() {
  if (arguments.length === 2)
    return Rp.apply(null, arguments);
  if (arguments.length === 3)
    return Bp.apply(null, arguments);
  if (arguments.length === 4)
    return XC.apply(null, arguments);
  if (arguments.length === 5)
    return YC.apply(null, arguments);
  je("Invalid extension access syntax");
};
ci.prototype.extension = Ru;
KC.forEach(function(t) {
  t.extensions.forEach(function(e) {
    Bp(t.type, e.name, e.impl);
  });
});
var Ms = function() {
  if (!(this instanceof Ms))
    return new Ms();
  this.length = 0;
}, _n = Ms.prototype;
_n.instanceString = function() {
  return "stylesheet";
};
_n.selector = function(t) {
  var e = this.length++;
  return this[e] = {
    selector: t,
    properties: []
  }, this;
};
_n.css = function(t, e) {
  var r = this.length - 1;
  if (Se(t))
    this[r].properties.push({
      name: t,
      value: e
    });
  else if (Fe(t))
    for (var n = t, a = Object.keys(n), i = 0; i < a.length; i++) {
      var s = a[i], o = n[s];
      if (o != null) {
        var l = wt.properties[s] || wt.properties[Ws(s)];
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
_n.style = _n.css;
_n.generateStyle = function(t) {
  var e = new wt(t);
  return this.appendToStyle(e);
};
_n.appendToStyle = function(t) {
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
var ZC = "3.34.0", Nn = function(e) {
  if (e === void 0 && (e = {}), Fe(e))
    return new ci(e);
  if (Se(e))
    return Ru.apply(Ru, arguments);
};
Nn.use = function(t) {
  var e = Array.prototype.slice.call(arguments, 1);
  return e.unshift(Nn), t.apply(null, e), this;
};
Nn.warnings = function(t) {
  return rg(t);
};
Nn.version = ZC;
Nn.stylesheet = Nn.Stylesheet = Ms;
const QC = { class: "onec-tree-node" }, jC = {
  key: 1,
  class: "onec-chevron"
}, JC = { class: "onec-node-name" }, eT = { class: "onec-kind" }, tT = {
  key: 0,
  class: "onec-node-children"
}, rT = /* @__PURE__ */ uh({
  __name: "TreeNode",
  props: {
    node: {},
    selected: {}
  },
  emits: ["select"],
  setup(t, { emit: e }) {
    const r = t, n = e, a = /* @__PURE__ */ mt(!1);
    function i() {
      n("select", r.node);
    }
    return (s, o) => {
      const l = Xy("TreeNode", !0);
      return et(), ut("div", QC, [
        Ce("button", {
          class: fa(["onec-node-head", { selected: t.selected === t.node.fullName }]),
          onClick: i
        }, [
          t.node.children.length ? (et(), ut("span", {
            key: 0,
            class: "onec-chevron",
            onClick: o[0] || (o[0] = c0((u) => a.value = !a.value, ["stop"]))
          }, Le(a.value ? "▾" : "▸"), 1)) : (et(), ut("span", jC, "·")),
          Ce("span", JC, Le(t.node.name), 1),
          Ce("span", eT, Le(t.node.kind), 1)
        ], 2),
        a.value && t.node.children.length ? (et(), ut("div", tT, [
          (et(!0), ut(Yt, null, Qi(t.node.children, (u) => (et(), Ku(l, {
            key: u.fullName,
            node: u,
            selected: t.selected,
            onSelect: o[1] || (o[1] = (f) => n("select", f))
          }, null, 8, ["node", "selected"]))), 128))
        ])) : Lr("", !0)
      ]);
    };
  }
}), nT = { class: "onec-browser" }, aT = { class: "onec-sidebar" }, iT = { class: "onec-card onec-explorer" }, sT = { class: "onec-heading" }, oT = ["title"], lT = ["placeholder"], uT = {
  key: 0,
  class: "onec-muted onec-inline-state"
}, fT = {
  key: 1,
  class: "onec-search-results"
}, cT = ["onClick"], vT = { class: "onec-tree" }, dT = ["onClick"], hT = {
  key: 0,
  class: "onec-section-objects"
}, gT = {
  key: 0,
  class: "onec-empty"
}, pT = { class: "onec-card onec-summary" }, yT = { class: "onec-card onec-rebuild" }, mT = { class: "onec-row" }, bT = ["placeholder"], wT = ["disabled"], xT = { class: "onec-main onec-card" }, ET = { class: "onec-object-header" }, CT = {
  key: 0,
  class: "onec-kind"
}, TT = { class: "onec-graph-controls" }, ST = ["disabled"], PT = { value: "dependencies" }, DT = { value: "references" }, AT = { value: "dataflow" }, kT = ["disabled"], BT = {
  key: 0,
  class: "onec-object-details"
}, RT = { class: "onec-graph-status" }, MT = { key: 0 }, LT = { key: 0 }, IT = /* @__PURE__ */ uh({
  __name: "BrowserPanel",
  props: {
    api: {}
  },
  setup(t) {
    const e = t, r = /* @__PURE__ */ Ja({ objectCount: 0, relationCount: 0, sectionCount: 0 }), n = /* @__PURE__ */ mt([]), a = /* @__PURE__ */ Ja({}), i = /* @__PURE__ */ mt(""), s = /* @__PURE__ */ mt(!1), o = /* @__PURE__ */ mt([]), l = /* @__PURE__ */ mt(null), u = /* @__PURE__ */ mt(""), f = /* @__PURE__ */ mt(!1), c = /* @__PURE__ */ mt(!1), v = /* @__PURE__ */ mt(""), d = /* @__PURE__ */ mt(!1), h = /* @__PURE__ */ mt("dependencies"), y = /* @__PURE__ */ mt(2), g = /* @__PURE__ */ mt(250), p = /* @__PURE__ */ mt(!1), m = /* @__PURE__ */ mt("Select an object and load a graph."), b = /* @__PURE__ */ mt(null), w = /* @__PURE__ */ mt(null);
    let E = null, T = null, x, S = 0;
    async function D(V, F = {}) {
      const $ = await e.api.invoke("plugin.action", {
        pluginId: "onec",
        action: V,
        valueJson: JSON.stringify(F)
      });
      if (!$.ok) throw new Error($.error || `OneC action '${V}' failed.`);
      return $.resultJson ? JSON.parse($.resultJson) : null;
    }
    async function A() {
      c.value = !0;
      try {
        const V = await D("overview");
        r.objectCount = V.objectCount, r.relationCount = V.relationCount, r.sectionCount = V.sectionCount, n.value = V.sections;
        for (const F of V.sections)
          F.kind in a || (a[F.kind] = !1);
        V.treeTruncated ? N("The navigation tree is limited to 20,000 indexed objects.") : v.value.startsWith("The navigation tree") && N("");
      } catch (V) {
        N(`Unable to load the OneC index: ${H(V)}`, !0);
      } finally {
        c.value = !1;
      }
    }
    Zi(i, (V) => {
      window.clearTimeout(x);
      const F = ++S;
      x = window.setTimeout(async () => {
        if (!V.trim()) {
          o.value = [], s.value = !1;
          return;
        }
        s.value = !0;
        try {
          const $ = await D("search", { query: V });
          F === S && (o.value = $.results);
        } catch ($) {
          F === S && N(`Search failed: ${H($)}`, !0);
        } finally {
          F === S && (s.value = !1);
        }
      }, 180);
    });
    async function k(V) {
      try {
        l.value = await D("object", { fullName: V }), b.value = null, m.value = l.value ? "Choose a graph mode." : "The selected object no longer exists.", E == null || E.elements().remove();
      } catch (F) {
        N(`Unable to load the object: ${H(F)}`, !0);
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
      f.value = !0, N("Indexing the configuration…");
      try {
        const V = await D("rebuild", { path: u.value });
        N(
          `Index ready: +${V.objectsAdded} objects, ${V.objectsUpdated} updated, +${V.relationsAdded} relations, ${V.filesSkipped} skipped, ${V.filesWithErrors} errors in ${V.elapsedSeconds}s.`
        ), await A();
      } catch (V) {
        N(`Index build failed: ${H(V)}`, !0);
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
    }, O = [
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
      const F = [
        ...V.nodes.map(($) => ({ data: $ })),
        ...V.edges.map(($) => ({ data: $ }))
      ];
      E == null || E.destroy(), E = Nn({
        container: w.value,
        elements: F,
        style: O,
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
    function N(V, F = !1) {
      v.value = V, d.value = F;
    }
    function H(V) {
      return V instanceof Error ? V.message : String(V);
    }
    return dh(() => {
      A(), w.value && (T = new ResizeObserver(() => E == null ? void 0 : E.resize()), T.observe(w.value));
    }), hh(() => {
      window.clearTimeout(x), T == null || T.disconnect(), E == null || E.destroy();
    }), (V, F) => {
      var $;
      return et(), ut("div", nT, [
        Ce("aside", aT, [
          Ce("section", iT, [
            Ce("div", sT, [
              Ce("strong", null, Le(st(lt)("1C Configuration")), 1),
              Ce("button", {
                title: st(lt)("Refresh index summary"),
                onClick: A
              }, "↻", 8, oT)
            ]),
            Aa(Ce("input", {
              "onUpdate:modelValue": F[0] || (F[0] = (Q) => i.value = Q),
              class: "onec-search",
              placeholder: st(lt)("Search objects…"),
              spellcheck: "false"
            }, null, 8, lT), [
              [Ii, i.value]
            ]),
            s.value ? (et(), ut("div", uT, Le(st(lt)("Searching…")), 1)) : o.value.length ? (et(), ut("ul", fT, [
              (et(!0), ut(Yt, null, Qi(o.value, (Q) => {
                var se;
                return et(), ut("li", {
                  key: Q.fullName
                }, [
                  Ce("button", {
                    class: fa({ selected: ((se = l.value) == null ? void 0 : se.fullName) === Q.fullName }),
                    onClick: (ae) => k(Q.fullName)
                  }, [
                    Ce("span", null, Le(Q.fullName), 1),
                    Ce("small", null, Le(Q.kind), 1)
                  ], 10, cT)
                ]);
              }), 128))
            ])) : Lr("", !0),
            Ce("div", vT, [
              (et(!0), ut(Yt, null, Qi(n.value, (Q) => (et(), ut("section", {
                key: Q.kind,
                class: "onec-tree-section"
              }, [
                Ce("button", {
                  class: "onec-section-head",
                  onClick: (se) => a[Q.kind] = !a[Q.kind]
                }, [
                  Ce("span", null, Le(a[Q.kind] ? "▾" : "▸"), 1),
                  Ce("strong", null, Le(Q.kind), 1),
                  Ce("small", null, Le(Q.count), 1)
                ], 8, dT),
                a[Q.kind] ? (et(), ut("div", hT, [
                  (et(!0), ut(Yt, null, Qi(Q.objects, (se) => {
                    var ae;
                    return et(), Ku(rT, {
                      key: se.fullName,
                      node: se,
                      selected: (ae = l.value) == null ? void 0 : ae.fullName,
                      onSelect: F[1] || (F[1] = (le) => k(le.fullName))
                    }, null, 8, ["node", "selected"]);
                  }), 128))
                ])) : Lr("", !0)
              ]))), 128)),
              !c.value && !n.value.length ? (et(), ut("div", gT, [
                Mr(Le(st(lt)("The OneC index is empty. Build it below or run")) + " ", 1),
                F[6] || (F[6] = Ce("code", null, "onec_build_index", -1)),
                F[7] || (F[7] = Mr(". ", -1))
              ])) : Lr("", !0)
            ])
          ]),
          Ce("section", pT, [
            Ce("div", null, [
              Ce("span", null, Le(st(lt)("Objects")), 1),
              Ce("strong", null, Le(r.objectCount), 1)
            ]),
            Ce("div", null, [
              Ce("span", null, Le(st(lt)("Relations")), 1),
              Ce("strong", null, Le(r.relationCount), 1)
            ]),
            Ce("div", null, [
              Ce("span", null, Le(st(lt)("Sections")), 1),
              Ce("strong", null, Le(r.sectionCount), 1)
            ])
          ]),
          Ce("section", yT, [
            Ce("strong", null, Le(st(lt)("Build index")), 1),
            Ce("p", null, Le(st(lt)("Path inside the current project workspace.")), 1),
            Ce("div", mT, [
              Aa(Ce("input", {
                "onUpdate:modelValue": F[2] || (F[2] = (Q) => u.value = Q),
                class: "onec-grow",
                placeholder: st(lt)("e.g. configuration/"),
                spellcheck: "false"
              }, null, 8, bT), [
                [Ii, u.value]
              ]),
              Ce("button", {
                disabled: f.value || !u.value.trim(),
                onClick: M
              }, Le(f.value ? "Building…" : "Build"), 9, wT)
            ])
          ])
        ]),
        Ce("main", xT, [
          Ce("header", ET, [
            Ce("div", null, [
              Ce("strong", null, Le((($ = l.value) == null ? void 0 : $.fullName) || "Choose an object"), 1),
              l.value ? (et(), ut("span", CT, Le(l.value.kind), 1)) : Lr("", !0)
            ]),
            Ce("div", TT, [
              Aa(Ce("select", {
                "onUpdate:modelValue": F[3] || (F[3] = (Q) => h.value = Q),
                disabled: !l.value
              }, [
                Ce("option", PT, Le(st(lt)("Dependencies")), 1),
                Ce("option", DT, Le(st(lt)("References")), 1),
                Ce("option", AT, Le(st(lt)("Data flow")), 1)
              ], 8, ST), [
                [l0, h.value]
              ]),
              Ce("label", null, [
                Mr(Le(st(lt)("Depth")) + " ", 1),
                Aa(Ce("input", {
                  "onUpdate:modelValue": F[4] || (F[4] = (Q) => y.value = Q),
                  type: "number",
                  min: "1",
                  max: "8"
                }, null, 512), [
                  [
                    Ii,
                    y.value,
                    void 0,
                    { number: !0 }
                  ]
                ])
              ]),
              Ce("label", null, [
                Mr(Le(st(lt)("Edges")) + " ", 1),
                Aa(Ce("input", {
                  "onUpdate:modelValue": F[5] || (F[5] = (Q) => g.value = Q),
                  type: "number",
                  min: "1",
                  max: "1000",
                  step: "25"
                }, null, 512), [
                  [
                    Ii,
                    g.value,
                    void 0,
                    { number: !0 }
                  ]
                ])
              ]),
              Ce("button", {
                disabled: !l.value || p.value,
                onClick: R
              }, Le(p.value ? "Loading…" : "Show graph"), 9, kT)
            ])
          ]),
          l.value ? (et(), ut("div", BT, [
            Ce("div", null, [
              Ce("span", null, Le(st(lt)("Name")), 1),
              Mr(Le(l.value.name), 1)
            ]),
            Ce("div", null, [
              Ce("span", null, Le(st(lt)("Path")), 1),
              Mr(Le(l.value.path || "—"), 1)
            ]),
            Ce("div", null, [
              Ce("span", null, Le(st(lt)("Summary")), 1),
              Mr(Le(l.value.summary || "—"), 1)
            ])
          ])) : Lr("", !0),
          Ce("div", RT, [
            Ce("span", null, Le(m.value), 1),
            b.value ? (et(), ut("span", MT, [
              Mr(Le(b.value.nodeCount) + " nodes · " + Le(b.value.edgeCount) + " edges · depth " + Le(b.value.depth) + " ", 1),
              b.value.truncated ? (et(), ut("b", LT, Le(st(lt)("· truncated")), 1)) : Lr("", !0)
            ])) : Lr("", !0)
          ]),
          Ce("div", {
            ref_key: "graphElement",
            ref: w,
            class: "onec-graph"
          }, null, 512),
          v.value ? (et(), ut("div", {
            key: 1,
            class: fa(["onec-status", { error: d.value }])
          }, Le(v.value), 3)) : Lr("", !0)
        ])
      ]);
    };
  }
}), OT = ".onec-browser{display:grid;grid-template-columns:minmax(280px,360px) minmax(420px,1fr);gap:10px;min-height:560px;color:var(--text, #d8dee9);font-size:var(--fs-sm, 12px)}.onec-sidebar{display:flex;min-width:0;flex-direction:column;gap:10px}.onec-card{min-width:0;border:1px solid var(--border, #3d444d);border-radius:var(--radius, 7px);background:var(--panel, #262b33);padding:10px}.onec-explorer{display:flex;min-height:310px;flex:1;flex-direction:column;gap:8px}.onec-heading,.onec-object-header,.onec-row,.onec-graph-controls,.onec-summary>div,.onec-graph-status{display:flex;align-items:center;gap:7px}.onec-heading,.onec-object-header,.onec-graph-status,.onec-summary>div{justify-content:space-between}.onec-heading button{margin-left:auto}.onec-browser button,.onec-browser input,.onec-browser select{min-height:26px;box-sizing:border-box;border:1px solid var(--border, #3d444d);border-radius:5px;color:var(--text, #d8dee9);background:var(--bg, #20242b);font:inherit}.onec-browser button{cursor:pointer;padding:2px 8px}.onec-browser button:hover:not(:disabled){border-color:var(--accent, #58a6ff);background:var(--accent-soft, #23364d)}.onec-browser button:disabled{cursor:default;opacity:.5}.onec-browser input,.onec-browser select{padding:3px 7px}.onec-search{width:100%}.onec-search-results{max-height:155px;margin:0;padding:0;overflow:auto;list-style:none;border:1px solid var(--border, #3d444d);border-radius:5px}.onec-search-results button{display:flex;width:100%;justify-content:space-between;gap:8px;border:0;border-radius:0;text-align:left}.onec-search-results button.selected,.onec-node-head.selected{color:var(--accent, #58a6ff);background:var(--accent-soft, #23364d)}.onec-search-results span,.onec-node-name{min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.onec-search-results small,.onec-kind,.onec-muted,.onec-rebuild p,.onec-graph-status{color:var(--muted, #8b949e)}.onec-inline-state{padding:3px 6px}.onec-tree{min-height:0;flex:1;overflow:auto}.onec-tree-section+.onec-tree-section{margin-top:2px}.onec-section-head,.onec-node-head{display:flex;width:100%;align-items:center;gap:5px;border:0!important;background:transparent!important;text-align:left}.onec-section-head small{margin-left:auto;color:var(--muted, #8b949e)}.onec-section-objects,.onec-node-children{padding-left:12px}.onec-chevron{width:13px;flex:0 0 13px;text-align:center}.onec-kind{margin-left:auto;font-size:var(--fs-xs, 10px)}.onec-empty{padding:18px 8px;color:var(--muted, #8b949e);text-align:center}.onec-summary{display:grid;grid-template-columns:repeat(3,1fr);gap:9px}.onec-summary>div{flex-direction:column;gap:2px}.onec-summary span,.onec-rebuild p{font-size:var(--fs-xs, 10px)}.onec-summary strong{font-size:16px}.onec-rebuild p{margin:3px 0 8px}.onec-grow{min-width:0;flex:1}.onec-main{display:flex;min-height:0;flex-direction:column;gap:9px}.onec-object-header{align-items:flex-start;flex-wrap:wrap}.onec-object-header>div:first-child{display:flex;min-width:0;flex-direction:column;gap:3px}.onec-object-header .onec-kind{margin-left:0}.onec-graph-controls{flex-wrap:wrap;justify-content:flex-end}.onec-graph-controls label{display:flex;align-items:center;gap:4px;color:var(--muted, #8b949e)}.onec-graph-controls input{width:58px}.onec-object-details{display:grid;grid-template-columns:1fr;gap:3px;padding:7px 9px;border:1px solid var(--border, #3d444d);border-radius:5px}.onec-object-details>div{overflow-wrap:anywhere}.onec-object-details span{display:inline-block;width:62px;color:var(--muted, #8b949e)}.onec-graph-status{min-height:18px;flex-wrap:wrap}.onec-graph-status b{color:#e6a15a}.onec-graph{min-height:360px;flex:1;overflow:hidden;border:1px solid var(--border, #3d444d);border-radius:6px;background:color-mix(in srgb,var(--bg, #20242b) 88%,black)}.onec-status{padding:6px 8px;border-radius:5px;background:var(--accent-soft, #23364d);white-space:pre-wrap}.onec-status.error{color:#ffb4a9;background:color-mix(in srgb,#f85149 15%,transparent)}@media(max-width:900px){.onec-browser{grid-template-columns:1fr}.onec-main{min-height:560px}}", Rd = "spla-onec-web-styles";
function _T() {
  if (document.getElementById(Rd)) return;
  const t = document.createElement("style");
  t.id = Rd, t.textContent = OT, document.head.appendChild(t);
}
function FT(t, e) {
  var n;
  y0((n = e.t) == null ? void 0 : n.bind(e)), _T();
  let r = h0(IT, { api: e });
  return r.mount(t), {
    // The browser owns no plugin settings; Save must preserve the opaque host blob unchanged.
    save: () => e.getJson(),
    destroy: () => {
      r == null || r.unmount(), r = null;
    }
  };
}
export {
  FT as mount
};
