(function(){"use strict";try{if(typeof document<"u"){var a=document.createElement("style");a.appendChild(document.createTextNode(".ssh-set[data-v-ca8fe92f]{display:flex;flex-direction:column;gap:8px;font-size:var(--fs-sm, 12px);color:var(--text, inherit)}.muted[data-v-ca8fe92f]{color:var(--muted, #888)}.empty[data-v-ca8fe92f]{font-style:italic}.row[data-v-ca8fe92f]{display:flex;gap:8px;align-items:center;flex-wrap:wrap}.row.spread[data-v-ca8fe92f]{justify-content:space-between}.self-start[data-v-ca8fe92f]{align-self:flex-start}.grow[data-v-ca8fe92f]{flex:1}.w-label[data-v-ca8fe92f]{width:80px}.w-70[data-v-ca8fe92f]{width:70px}.w-120[data-v-ca8fe92f]{width:120px}.w-140[data-v-ca8fe92f]{width:140px}.w-180[data-v-ca8fe92f]{width:180px}.w-260[data-v-ca8fe92f]{width:260px}.host-card[data-v-ca8fe92f]{border:1px solid var(--border, #444);border-radius:var(--radius, 6px);padding:8px 10px;display:flex;flex-direction:column;gap:6px;background:var(--panel, transparent)}.new-cred[data-v-ca8fe92f]{padding:4px 6px;border:1px dashed var(--border, #444);border-radius:5px}.chk[data-v-ca8fe92f]{cursor:pointer}.chk input[data-v-ca8fe92f]{height:auto}label[data-v-ca8fe92f]{display:flex;gap:6px;align-items:center}input[data-v-ca8fe92f],select[data-v-ca8fe92f]{height:24px;padding:2px 6px;color:var(--text, inherit);background:var(--bg, transparent);border:1px solid var(--border, #444);border-radius:5px;font-family:inherit;font-size:inherit}button[data-v-ca8fe92f]{padding:2px 10px;color:var(--text, inherit);background:var(--panel, transparent);border:1px solid var(--border, #444);border-radius:5px;cursor:pointer;font-size:inherit}button[data-v-ca8fe92f]:hover:not(:disabled){border-color:var(--muted, #888)}button[data-v-ca8fe92f]:disabled{opacity:.5;cursor:default}")),document.head.appendChild(a)}}catch(e){console.error("vite-plugin-css-injected-by-js",e)}})();
/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function di(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const Q = {}, jt = [], Ye = () => {
}, qr = () => !1, an = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), fn = (e) => e.startsWith("onUpdate:"), ae = Object.assign, pi = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, Bl = Object.prototype.hasOwnProperty, W = (e, t) => Bl.call(e, t), F = Array.isArray, Ft = (e) => Es(e) === "[object Map]", Yt = (e) => Es(e) === "[object Set]", zi = (e) => Es(e) === "[object Date]", U = (e) => typeof e == "function", se = (e) => typeof e == "string", Ge = (e) => typeof e == "symbol", Y = (e) => e !== null && typeof e == "object", Hr = (e) => (Y(e) || U(e)) && U(e.then) && U(e.catch), Wr = Object.prototype.toString, Es = (e) => Wr.call(e), Kl = (e) => Es(e).slice(8, -1), Jr = (e) => Es(e) === "[object Object]", gi = (e) => se(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, us = /* @__PURE__ */ di(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), un = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, xl = /-\w/g, $e = un(
  (e) => e.replace(xl, (t) => t.slice(1).toUpperCase())
), Ul = /\B([A-Z])/g, At = un(
  (e) => e.replace(Ul, "-$1").toLowerCase()
), Yr = un((e) => e.charAt(0).toUpperCase() + e.slice(1)), In = un(
  (e) => e ? `on${Yr(e)}` : ""
), Je = (e, t) => !Object.is(e, t), Ws = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Gr = (e, t, s, n = !1) => {
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
let Zi;
const dn = () => Zi || (Zi = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function mi(e) {
  if (F(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], i = se(n) ? Wl(n) : mi(n);
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
function yi(e) {
  let t = "";
  if (se(e))
    t = e;
  else if (F(e))
    for (let s = 0; s < e.length; s++) {
      const n = yi(e[s]);
      n && (t += n + " ");
    }
  else if (Y(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const Jl = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", Yl = /* @__PURE__ */ di(Jl);
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
  let s = zi(e), n = zi(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Ge(e), n = Ge(t), s || n)
    return e === t;
  if (s = F(e), n = F(t), s || n)
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
function bi(e, t) {
  return e.findIndex((s) => Gt(s, t));
}
const Xr = (e) => !!(e && e.__v_isRef === !0), $t = (e) => se(e) ? e : e == null ? "" : F(e) || Y(e) && (e.toString === Wr || !U(e.toString)) ? Xr(e) ? $t(e.value) : JSON.stringify(e, zr, 2) : String(e), zr = (e, t) => Xr(t) ? zr(e, t.value) : Ft(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, i], r) => (s[Ln(n, r) + " =>"] = i, s),
    {}
  )
} : Yt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => Ln(s))
} : Ge(t) ? Ln(t) : Y(t) && !F(t) && !Jr(t) ? String(t) : t, Ln = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Ge(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
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
const $n = /* @__PURE__ */ new WeakSet();
class Zr {
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
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || to(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, er(this), so(this);
    const t = z, s = Pe;
    z = this, Pe = !0;
    try {
      return this.fn();
    } finally {
      no(this), z = t, Pe = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        _i(t);
      this.deps = this.depsTail = void 0, er(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? $n.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Qn(this) && this.run();
  }
  get dirty() {
    return Qn(this);
  }
}
let eo = 0, hs, ds;
function to(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = ds, ds = e;
    return;
  }
  e.next = hs, hs = e;
}
function wi() {
  eo++;
}
function Si() {
  if (--eo > 0)
    return;
  if (ds) {
    let t = ds;
    for (ds = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; hs; ) {
    let t = hs;
    for (hs = void 0; t; ) {
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
    n.version === -1 ? (n === s && (s = i), _i(n), zl(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = i;
  }
  e.deps = t, e.depsTail = s;
}
function Qn(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (io(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function io(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Ss) || (e.globalVersion = Ss, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Qn(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = z, n = Pe;
  z = e, Pe = !0;
  try {
    so(e);
    const i = e.fn(e._value);
    (t.version === 0 || Je(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    z = s, Pe = n, no(e), e.flags &= -3;
  }
}
function _i(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: i } = e;
  if (n && (n.nextSub = i, e.prevSub = void 0), i && (i.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let r = s.computed.deps; r; r = r.nextDep)
      _i(r, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function zl(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let Pe = !0;
const ro = [];
function Qe() {
  ro.push(Pe), Pe = !1;
}
function Xe() {
  const e = ro.pop();
  Pe = e === void 0 ? !0 : e;
}
function er(e) {
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
let Ss = 0;
class Zl {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class ki {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!z || !Pe || z === this.computed)
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
    this.version++, Ss++, this.notify(t);
  }
  notify(t) {
    wi();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      Si();
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
const Xn = /* @__PURE__ */ new WeakMap(), Ot = /* @__PURE__ */ Symbol(
  ""
), zn = /* @__PURE__ */ Symbol(
  ""
), _s = /* @__PURE__ */ Symbol(
  ""
);
function he(e, t, s) {
  if (Pe && z) {
    let n = Xn.get(e);
    n || Xn.set(e, n = /* @__PURE__ */ new Map());
    let i = n.get(s);
    i || (n.set(s, i = new ki()), i.map = n, i.key = s), i.track();
  }
}
function nt(e, t, s, n, i, r) {
  const o = Xn.get(e);
  if (!o) {
    Ss++;
    return;
  }
  const l = (c) => {
    c && c.trigger();
  };
  if (wi(), t === "clear")
    o.forEach(l);
  else {
    const c = F(e), a = c && gi(s);
    if (c && s === "length") {
      const f = Number(n);
      o.forEach((u, g) => {
        (g === "length" || g === _s || !Ge(g) && g >= f) && l(u);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), a && l(o.get(_s)), t) {
        case "add":
          c ? a && l(o.get("length")) : (l(o.get(Ot)), Ft(e) && l(o.get(zn)));
          break;
        case "delete":
          c || (l(o.get(Ot)), Ft(e) && l(o.get(zn)));
          break;
        case "set":
          Ft(e) && l(o.get(Ot));
          break;
      }
  }
  Si();
}
function Ct(e) {
  const t = /* @__PURE__ */ H(e);
  return t === e ? t : (he(t, "iterate", _s), /* @__PURE__ */ Ee(e) ? t : t.map(Me));
}
function pn(e) {
  return he(e = /* @__PURE__ */ H(e), "iterate", _s), e;
}
function He(e, t) {
  return /* @__PURE__ */ at(e) ? Vt(/* @__PURE__ */ Tt(e) ? Me(t) : t) : Me(t);
}
const ec = {
  __proto__: null,
  [Symbol.iterator]() {
    return Pn(this, Symbol.iterator, (e) => He(this, e));
  },
  concat(...e) {
    return Ct(this).concat(
      ...e.map((t) => F(t) ? Ct(t) : t)
    );
  },
  entries() {
    return Pn(this, "entries", (e) => (e[1] = He(this, e[1]), e));
  },
  every(e, t) {
    return Ze(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return Ze(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => He(this, n)),
      arguments
    );
  },
  find(e, t) {
    return Ze(
      this,
      "find",
      e,
      t,
      (s) => He(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return Ze(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return Ze(
      this,
      "findLast",
      e,
      t,
      (s) => He(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return Ze(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return Ze(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return Mn(this, "includes", e);
  },
  indexOf(...e) {
    return Mn(this, "indexOf", e);
  },
  join(e) {
    return Ct(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return Mn(this, "lastIndexOf", e);
  },
  map(e, t) {
    return Ze(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return is(this, "pop");
  },
  push(...e) {
    return is(this, "push", e);
  },
  reduce(e, ...t) {
    return tr(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return tr(this, "reduceRight", e, t);
  },
  shift() {
    return is(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return Ze(this, "some", e, t, void 0, arguments);
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
    return Pn(this, "values", (e) => He(this, e));
  }
};
function Pn(e, t, s) {
  const n = pn(e), i = n[t]();
  return n !== e && !/* @__PURE__ */ Ee(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = s(r.value)), r;
  }), i;
}
const tc = Array.prototype;
function Ze(e, t, s, n, i, r) {
  const o = pn(e), l = o !== e && !/* @__PURE__ */ Ee(e), c = o[t];
  if (c !== tc[t]) {
    const u = c.apply(e, r);
    return l ? Me(u) : u;
  }
  let a = s;
  o !== e && (l ? a = function(u, g) {
    return s.call(this, He(e, u), g, e);
  } : s.length > 2 && (a = function(u, g) {
    return s.call(this, u, g, e);
  }));
  const f = c.call(o, a, n);
  return l && i ? i(f) : f;
}
function tr(e, t, s, n) {
  const i = pn(e), r = i !== e && !/* @__PURE__ */ Ee(e);
  let o = s, l = !1;
  i !== e && (r ? (l = n.length === 0, o = function(a, f, u) {
    return l && (l = !1, a = He(e, a)), s.call(this, a, He(e, f), u, e);
  }) : s.length > 3 && (o = function(a, f, u) {
    return s.call(this, a, f, u, e);
  }));
  const c = i[t](o, ...n);
  return l ? He(e, c) : c;
}
function Mn(e, t, s) {
  const n = /* @__PURE__ */ H(e);
  he(n, "iterate", _s);
  const i = n[t](...s);
  return (i === -1 || i === !1) && /* @__PURE__ */ Ti(s[0]) ? (s[0] = /* @__PURE__ */ H(s[0]), n[t](...s)) : i;
}
function is(e, t, s = []) {
  Qe(), wi();
  const n = (/* @__PURE__ */ H(e))[t].apply(e, s);
  return Si(), Xe(), n;
}
const sc = /* @__PURE__ */ di("__proto__,__v_isRef,__isVue"), lo = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Ge)
);
function nc(e) {
  Ge(e) || (e = String(e));
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
    const o = F(t);
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
    if ((Ge(s) ? lo.has(s) : sc(s)) || (i || he(t, "get", s), r))
      return l;
    if (/* @__PURE__ */ de(l)) {
      const c = o && gi(s) ? l : l.value;
      return i && Y(c) ? /* @__PURE__ */ ei(c) : c;
    }
    return Y(l) ? i ? /* @__PURE__ */ ei(l) : /* @__PURE__ */ gn(l) : l;
  }
}
class ao extends co {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, i) {
    let r = t[s];
    const o = F(t) && gi(s);
    if (!this._isShallow) {
      const a = /* @__PURE__ */ at(r);
      if (!/* @__PURE__ */ Ee(n) && !/* @__PURE__ */ at(n) && (r = /* @__PURE__ */ H(r), n = /* @__PURE__ */ H(n)), !o && /* @__PURE__ */ de(r) && !/* @__PURE__ */ de(n))
        return a || (r.value = n), !0;
    }
    const l = o ? Number(s) < t.length : W(t, s), c = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ de(t) ? t : i
    );
    return t === /* @__PURE__ */ H(i) && c && (l ? Je(n, r) && nt(t, "set", s, n) : nt(t, "add", s, n)), c;
  }
  deleteProperty(t, s) {
    const n = W(t, s);
    t[s];
    const i = Reflect.deleteProperty(t, s);
    return i && n && nt(t, "delete", s, void 0), i;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Ge(s) || !lo.has(s)) && he(t, "has", s), n;
  }
  ownKeys(t) {
    return he(
      t,
      "iterate",
      F(t) ? "length" : Ot
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
const Zn = (e) => e, Fs = (e) => Reflect.getPrototypeOf(e);
function cc(e, t, s) {
  return function(...n) {
    const i = this.__v_raw, r = /* @__PURE__ */ H(i), o = Ft(r), l = e === "entries" || e === Symbol.iterator && o, c = e === "keys" && o, a = i[e](...n), f = s ? Zn : t ? Vt : Me;
    return !t && he(
      r,
      "iterate",
      c ? zn : Ot
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
function Bs(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function ac(e, t) {
  const s = {
    get(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ H(r), l = /* @__PURE__ */ H(i);
      e || (Je(i, l) && he(o, "get", i), he(o, "get", l));
      const { has: c } = Fs(o), a = t ? Zn : e ? Vt : Me;
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
      return e || (Je(i, l) && he(o, "has", i), he(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, c = /* @__PURE__ */ H(l), a = t ? Zn : e ? Vt : Me;
      return !e && he(c, "iterate", Ot), l.forEach((f, u) => i.call(r, a(f), a(u), o));
    }
  };
  return ae(
    s,
    e ? {
      add: Bs("add"),
      set: Bs("set"),
      delete: Bs("delete"),
      clear: Bs("clear")
    } : {
      add(i) {
        const r = /* @__PURE__ */ H(this), o = Fs(r), l = /* @__PURE__ */ H(i), c = !t && !/* @__PURE__ */ Ee(i) && !/* @__PURE__ */ at(i) ? l : i;
        return o.has.call(r, c) || Je(i, c) && o.has.call(r, i) || Je(l, c) && o.has.call(r, l) || (r.add(c), nt(r, "add", c, c)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ Ee(r) && !/* @__PURE__ */ at(r) && (r = /* @__PURE__ */ H(r));
        const o = /* @__PURE__ */ H(this), { has: l, get: c } = Fs(o);
        let a = l.call(o, i);
        a || (i = /* @__PURE__ */ H(i), a = l.call(o, i));
        const f = c.call(o, i);
        return o.set(i, r), a ? Je(r, f) && nt(o, "set", i, r) : nt(o, "add", i, r), this;
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
function vi(e, t) {
  const s = ac(e, t);
  return (n, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? n : Reflect.get(
    W(s, i) && i in n ? s : n,
    i,
    r
  );
}
const fc = {
  get: /* @__PURE__ */ vi(!1, !1)
}, uc = {
  get: /* @__PURE__ */ vi(!1, !0)
}, hc = {
  get: /* @__PURE__ */ vi(!0, !1)
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
function gn(e) {
  return /* @__PURE__ */ at(e) ? e : Oi(
    e,
    !1,
    rc,
    fc,
    fo
  );
}
// @__NO_SIDE_EFFECTS__
function gc(e) {
  return Oi(
    e,
    !1,
    lc,
    uc,
    uo
  );
}
// @__NO_SIDE_EFFECTS__
function ei(e) {
  return Oi(
    e,
    !0,
    oc,
    hc,
    ho
  );
}
function Oi(e, t, s, n, i) {
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
function Ee(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function Ti(e) {
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
const Me = (e) => Y(e) ? /* @__PURE__ */ gn(e) : e, Vt = (e) => Y(e) ? /* @__PURE__ */ ei(e) : e;
// @__NO_SIDE_EFFECTS__
function de(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Dn(e) {
  return yc(e, !1);
}
function yc(e, t) {
  return /* @__PURE__ */ de(e) ? e : new bc(e, t);
}
class bc {
  constructor(t, s) {
    this.dep = new ki(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ H(t), this._value = s ? t : Me(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ Ee(t) || /* @__PURE__ */ at(t);
    t = n ? t : /* @__PURE__ */ H(t), Je(t, s) && (this._rawValue = t, this._value = n ? t : Me(t), this.dep.trigger());
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
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new ki(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Ss - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
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
function kc(e, t, s = !1) {
  let n, i;
  return U(e) ? n = e : (n = e.get, i = e.set), new _c(n, i, s);
}
const Ks = {}, zs = /* @__PURE__ */ new WeakMap();
let St;
function vc(e, t = !1, s = St) {
  if (s) {
    let n = zs.get(s);
    n || zs.set(s, n = []), n.push(e);
  }
}
function Oc(e, t, s = Q) {
  const { immediate: n, deep: i, once: r, scheduler: o, augmentJob: l, call: c } = s, a = (_) => i ? _ : /* @__PURE__ */ Ee(_) || i === !1 || i === 0 ? it(_, 1) : it(_);
  let f, u, g, p, v = !1, m = !1;
  if (/* @__PURE__ */ de(e) ? (u = () => e.value, v = /* @__PURE__ */ Ee(e)) : /* @__PURE__ */ Tt(e) ? (u = () => a(e), v = !0) : F(e) ? (m = !0, v = e.some((_) => /* @__PURE__ */ Tt(_) || /* @__PURE__ */ Ee(_)), u = () => e.map((_) => {
    if (/* @__PURE__ */ de(_))
      return _.value;
    if (/* @__PURE__ */ Tt(_))
      return a(_);
    if (U(_))
      return c ? c(_, 2) : _();
  })) : U(e) ? t ? u = c ? () => c(e, 2) : e : u = () => {
    if (g) {
      Qe();
      try {
        g();
      } finally {
        Xe();
      }
    }
    const _ = St;
    St = f;
    try {
      return c ? c(e, 3, [p]) : e(p);
    } finally {
      St = _;
    }
  } : u = Ye, t && i) {
    const _ = u, $ = i === !0 ? 1 / 0 : i;
    u = () => it(_(), $);
  }
  const y = Xl(), b = () => {
    f.stop(), y && y.active && pi(y.effects, f);
  };
  if (r && t) {
    const _ = t;
    t = (...$) => {
      const x = _(...$);
      return b(), x;
    };
  }
  let w = m ? new Array(e.length).fill(Ks) : Ks;
  const I = (_) => {
    if (!(!(f.flags & 1) || !f.dirty && !_))
      if (t) {
        const $ = f.run();
        if (_ || i || v || (m ? $.some((x, D) => Je(x, w[D])) : Je($, w))) {
          g && g();
          const x = St;
          St = f;
          try {
            const D = [
              $,
              // pass undefined as the old value when it's changed for the first time
              w === Ks ? void 0 : m && w[0] === Ks ? [] : w,
              p
            ];
            w = $, c ? c(t, 3, D) : (
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
  return l && l(I), f = new Zr(u), f.scheduler = o ? () => o(I, !1) : I, p = (_) => vc(_, !1, f), g = f.onStop = () => {
    const _ = zs.get(f);
    if (_) {
      if (c)
        c(_, 4);
      else
        for (const $ of _) $();
      zs.delete(f);
    }
  }, t ? n ? I(!0) : w = f.run() : o ? o(I.bind(null, !0), !0) : f.run(), b.pause = f.pause.bind(f), b.resume = f.resume.bind(f), b.stop = b, b;
}
function it(e, t = 1 / 0, s) {
  if (t <= 0 || !Y(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ de(e))
    it(e.value, t, s);
  else if (F(e))
    for (let n = 0; n < e.length; n++)
      it(e[n], t, s);
  else if (Yt(e) || Ft(e))
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
function Cs(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (i) {
    mn(i, t, s);
  }
}
function De(e, t, s, n) {
  if (U(e)) {
    const i = Cs(e, t, s, n);
    return i && Hr(i) && i.catch((r) => {
      mn(r, t, s);
    }), i;
  }
  if (F(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(De(e[r], t, s, n));
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
      Qe(), Cs(r, null, 10, [
        e,
        c,
        a
      ]), Xe();
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
const be = [];
let qe = -1;
const Bt = [];
let ut = null, Pt = 0;
const go = /* @__PURE__ */ Promise.resolve();
let Zs = null;
function mo(e) {
  const t = Zs || go;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Nc(e) {
  let t = qe + 1, s = be.length;
  for (; t < s; ) {
    const n = t + s >>> 1, i = be[n], r = ks(i);
    r < e || r === e && i.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function Ni(e) {
  if (!(e.flags & 1)) {
    const t = ks(e), s = be[be.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= ks(s) ? be.push(e) : be.splice(Nc(t), 0, e), e.flags |= 1, yo();
  }
}
function yo() {
  Zs || (Zs = go.then(wo));
}
function Ac(e) {
  F(e) ? Bt.push(...e) : ut && e.id === -1 ? ut.splice(Pt + 1, 0, e) : e.flags & 1 || (Bt.push(e), e.flags |= 1), yo();
}
function sr(e, t, s = qe + 1) {
  for (; s < be.length; s++) {
    const n = be[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      be.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function bo(e) {
  if (Bt.length) {
    const t = [...new Set(Bt)].sort(
      (s, n) => ks(s) - ks(n)
    );
    if (Bt.length = 0, ut) {
      ut.push(...t);
      return;
    }
    for (ut = t, Pt = 0; Pt < ut.length; Pt++) {
      const s = ut[Pt];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    ut = null, Pt = 0;
  }
}
const ks = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function wo(e) {
  try {
    for (qe = 0; qe < be.length; qe++) {
      const t = be[qe];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Cs(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; qe < be.length; qe++) {
      const t = be[qe];
      t && (t.flags &= -2);
    }
    qe = -1, be.length = 0, bo(), Zs = null, (be.length || Bt.length) && wo();
  }
}
let Ae = null, So = null;
function en(e) {
  const t = Ae;
  return Ae = e, So = e && e.type.__scopeId || null, t;
}
function Ec(e, t = Ae, s) {
  if (!t || e._n)
    return e;
  const n = (...i) => {
    n._d && dr(-1);
    const r = en(t);
    let o;
    try {
      o = e(...i);
    } finally {
      en(r), n._d && dr(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function ve(e, t) {
  if (Ae === null)
    return e;
  const s = Sn(Ae), n = e.dirs || (e.dirs = []);
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
    c && (Qe(), De(c, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Xe());
  }
}
function Cc(e, t) {
  if (we) {
    let s = we.provides;
    const n = we.parent && we.parent.provides;
    n === s && (s = we.provides = Object.create(n)), s[e] = t;
  }
}
function Js(e, t, s = !1) {
  const n = Ca();
  if (n || Kt) {
    let i = Kt ? Kt._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return s && U(t) ? t.call(n && n.proxy) : t;
  }
}
const Ic = /* @__PURE__ */ Symbol.for("v-scx"), Lc = () => Js(Ic);
function Rn(e, t, s) {
  return _o(e, t, s);
}
function _o(e, t, s = Q) {
  const { immediate: n, deep: i, flush: r, once: o } = s, l = ae({}, s), c = t && n || !t && r !== "post";
  let a;
  if (Os) {
    if (r === "sync") {
      const p = Lc();
      a = p.__watcherHandles || (p.__watcherHandles = []);
    } else if (!c) {
      const p = () => {
      };
      return p.stop = Ye, p.resume = Ye, p.pause = Ye, p;
    }
  }
  const f = we;
  l.call = (p, v, m) => De(p, f, v, m);
  let u = !1;
  r === "post" ? l.scheduler = (p) => {
    _e(p, f && f.suspense);
  } : r !== "sync" && (u = !0, l.scheduler = (p, v) => {
    v ? p() : Ni(p);
  }), l.augmentJob = (p) => {
    t && (p.flags |= 4), u && (p.flags |= 2, f && (p.id = f.uid, p.i = f));
  };
  const g = Oc(e, t, l);
  return Os && (a ? a.push(g) : c && g()), g;
}
function $c(e, t, s) {
  const n = this.proxy, i = se(e) ? e.includes(".") ? ko(n, e) : () => n[e] : e.bind(n, n);
  let r;
  U(t) ? r = t : (r = t.handler, s = t);
  const o = Is(this), l = _o(i, r.bind(n), s);
  return o(), l;
}
function ko(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let i = 0; i < s.length && n; i++)
      n = n[s[i]];
    return n;
  };
}
const Pc = /* @__PURE__ */ Symbol("_vte"), Mc = (e) => e.__isTeleport, jn = /* @__PURE__ */ Symbol("_leaveCb");
function Ai(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, Ai(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Dc(e, t) {
  return U(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    ae({ name: e.name }, t, { setup: e })
  ) : e;
}
function vo(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function nr(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const tn = /* @__PURE__ */ new WeakMap();
function ps(e, t, s, n, i = !1) {
  if (F(e)) {
    e.forEach(
      (m, y) => ps(
        m,
        t && (F(t) ? t[y] : t),
        s,
        n,
        i
      )
    );
    return;
  }
  if (gs(n) && !i) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && ps(e, t, s, n.component.subTree);
    return;
  }
  const r = n.shapeFlag & 4 ? Sn(n.component) : n.el, o = i ? null : r, { i: l, r: c } = e, a = t && t.r, f = l.refs === Q ? l.refs = {} : l.refs, u = l.setupState, g = /* @__PURE__ */ H(u), p = u === Q ? qr : (m) => nr(f, m) ? !1 : W(g, m), v = (m, y) => !(y && nr(f, y));
  if (a != null && a !== c) {
    if (ir(t), se(a))
      f[a] = null, p(a) && (u[a] = null);
    else if (/* @__PURE__ */ de(a)) {
      const m = t;
      v(a, m.k) && (a.value = null), m.k && (f[m.k] = null);
    }
  }
  if (U(c)) {
    Qe();
    try {
      Cs(c, l, 12, [o, f]);
    } finally {
      Xe();
    }
  } else {
    const m = se(c), y = /* @__PURE__ */ de(c);
    if (m || y) {
      const b = () => {
        if (e.f) {
          const w = m ? p(c) ? u[c] : f[c] : v() || !e.k ? c.value : f[e.k];
          if (i)
            F(w) && pi(w, r);
          else if (F(w))
            w.includes(r) || w.push(r);
          else if (m)
            f[c] = [r], p(c) && (u[c] = f[c]);
          else {
            const I = [r];
            v(c, e.k) && (c.value = I), e.k && (f[e.k] = I);
          }
        } else m ? (f[c] = o, p(c) && (u[c] = o)) : y && (v(c, e.k) && (c.value = o), e.k && (f[e.k] = o));
      };
      if (o) {
        const w = () => {
          b(), tn.delete(e);
        };
        w.id = -1, tn.set(e, w), _e(w, s);
      } else
        ir(e), b();
    }
  }
}
function ir(e) {
  const t = tn.get(e);
  t && (t.flags |= 8, tn.delete(e));
}
dn().requestIdleCallback;
dn().cancelIdleCallback;
const gs = (e) => !!e.type.__asyncLoader, Oo = (e) => e.type.__isKeepAlive;
function Rc(e, t) {
  To(e, "a", t);
}
function jc(e, t) {
  To(e, "da", t);
}
function To(e, t, s = we) {
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
      Oo(i.parent.vnode) && Fc(n, t, s, i), i = i.parent;
  }
}
function Fc(e, t, s, n) {
  const i = yn(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Ao(() => {
    pi(n[t], i);
  }, s);
}
function yn(e, t, s = we, n = !1) {
  if (s) {
    const i = s[e] || (s[e] = []), r = t.__weh || (t.__weh = (...o) => {
      Qe();
      const l = Is(s), c = De(t, s, e, o);
      return l(), Xe(), c;
    });
    return n ? i.unshift(r) : i.push(r), r;
  }
}
const ft = (e) => (t, s = we) => {
  (!Os || e === "sp") && yn(e, (...n) => t(...n), s);
}, Bc = ft("bm"), No = ft("m"), Kc = ft(
  "bu"
), xc = ft("u"), Uc = ft(
  "bum"
), Ao = ft("um"), Vc = ft(
  "sp"
), qc = ft("rtg"), Hc = ft("rtc");
function Wc(e, t = we) {
  yn("ec", e, t);
}
const Jc = /* @__PURE__ */ Symbol.for("v-ndc");
function Fn(e, t, s, n) {
  let i;
  const r = s, o = F(e);
  if (o || se(e)) {
    const l = o && /* @__PURE__ */ Tt(e);
    let c = !1, a = !1;
    l && (c = !/* @__PURE__ */ Ee(e), a = /* @__PURE__ */ at(e), e = pn(e)), i = new Array(e.length);
    for (let f = 0, u = e.length; f < u; f++)
      i[f] = t(
        c ? a ? Vt(Me(e[f])) : Me(e[f]) : e[f],
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
const ti = (e) => e ? Yo(e) ? Sn(e) : ti(e.parent) : null, ms = (
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
    $parent: (e) => ti(e.parent),
    $root: (e) => ti(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Co(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      Ni(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = mo.bind(e.proxy)),
    $watch: (e) => $c.bind(e)
  })
), Bn = (e, t) => e !== Q && !e.__isScriptSetup && W(e, t), Yc = {
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
        if (Bn(n, t))
          return o[t] = 1, n[t];
        if (i !== Q && W(i, t))
          return o[t] = 2, i[t];
        if (W(r, t))
          return o[t] = 3, r[t];
        if (s !== Q && W(s, t))
          return o[t] = 4, s[t];
        si && (o[t] = 0);
      }
    }
    const a = ms[t];
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
    return Bn(i, t) ? (i[t] = s, !0) : n !== Q && W(n, t) ? (n[t] = s, !0) : W(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: i, props: r, type: o }
  }, l) {
    let c;
    return !!(s[l] || e !== Q && l[0] !== "$" && W(e, l) || Bn(t, l) || W(r, l) || W(n, l) || W(ms, l) || W(i.config.globalProperties, l) || (c = o.__cssModules) && c[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : W(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function rr(e) {
  return F(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let si = !0;
function Gc(e) {
  const t = Co(e), s = e.proxy, n = e.ctx;
  si = !1, t.beforeCreate && or(t.beforeCreate, e, "bc");
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
    updated: v,
    activated: m,
    deactivated: y,
    beforeDestroy: b,
    beforeUnmount: w,
    destroyed: I,
    unmounted: _,
    render: $,
    renderTracked: x,
    renderTriggered: D,
    errorCaptured: P,
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
    Y(te) && (e.data = /* @__PURE__ */ gn(te));
  }
  if (si = !0, r)
    for (const te in r) {
      const X = r[te], mt = U(X) ? X.bind(s, s) : U(X.get) ? X.get.bind(s, s) : Ye, Rs = !U(X) && U(X.set) ? X.set.bind(s) : Ye, yt = Da({
        get: mt,
        set: Rs
      });
      Object.defineProperty(n, te, {
        enumerable: !0,
        configurable: !0,
        get: () => yt.value,
        set: (je) => yt.value = je
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
  f && or(f, e, "c");
  function ge(te, X) {
    F(X) ? X.forEach((mt) => te(mt.bind(s))) : X && te(X.bind(s));
  }
  if (ge(Bc, u), ge(No, g), ge(Kc, p), ge(xc, v), ge(Rc, m), ge(jc, y), ge(Wc, P), ge(Hc, x), ge(qc, D), ge(Uc, w), ge(Ao, _), ge(Vc, q), F(ee))
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
  $ && e.render === Ye && (e.render = $), pe != null && (e.inheritAttrs = pe), fe && (e.components = fe), ue && (e.directives = ue), q && vo(e);
}
function Qc(e, t, s = Ye) {
  F(e) && (e = ni(e));
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
function or(e, t, s) {
  De(
    F(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Eo(e, t, s, n) {
  let i = n.includes(".") ? ko(s, n) : () => s[n];
  if (se(e)) {
    const r = t[e];
    U(r) && Rn(i, r);
  } else if (U(e))
    Rn(i, e.bind(s));
  else if (Y(e))
    if (F(e))
      e.forEach((r) => Eo(r, t, s, n));
    else {
      const r = U(e.handler) ? e.handler.bind(s) : t[e.handler];
      U(r) && Rn(i, r, e);
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
  data: lr,
  props: cr,
  emits: cr,
  // objects
  methods: ls,
  computed: ls,
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
  components: ls,
  directives: ls,
  // watch
  watch: Zc,
  // provide / inject
  provide: lr,
  inject: zc
};
function lr(e, t) {
  return t ? e ? function() {
    return ae(
      U(e) ? e.call(this, this) : e,
      U(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function zc(e, t) {
  return ls(ni(e), ni(t));
}
function ni(e) {
  if (F(e)) {
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
function ls(e, t) {
  return e ? ae(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function cr(e, t) {
  return e ? F(e) && F(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : ae(
    /* @__PURE__ */ Object.create(null),
    rr(e),
    rr(t ?? {})
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
          return p.appContext = r, g === !0 ? g = "svg" : g === !1 && (g = void 0), e(p, f, g), c = !0, a._container = f, f.__vue_app__ = a, Sn(p.component);
        }
      },
      onUnmount(f) {
        l.push(f);
      },
      unmount() {
        c && (De(
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
const sa = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${$e(t)}Modifiers`] || e[`${At(t)}Modifiers`];
function na(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || Q;
  let i = s;
  const r = t.startsWith("update:"), o = r && sa(n, t.slice(7));
  o && (o.trim && (i = s.map((f) => se(f) ? f.trim() : f)), o.number && (i = s.map(hn)));
  let l, c = n[l = In(t)] || // also try camelCase event handler (#2249)
  n[l = In($e(t))];
  !c && r && (c = n[l = In(At(t))]), c && De(
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
    e.emitted[l] = !0, De(
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
  return !r && !l ? (Y(e) && n.set(e, null), null) : (F(r) ? r.forEach((c) => o[c] = null) : ae(o, r), Y(e) && n.set(e, o), o);
}
function bn(e, t) {
  return !e || !an(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), W(e, t[0].toLowerCase() + t.slice(1)) || W(e, At(t)) || W(e, t));
}
function ar(e) {
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
    ctx: v,
    inheritAttrs: m
  } = e, y = en(e);
  let b, w;
  try {
    if (s.shapeFlag & 4) {
      const _ = i || n, $ = _;
      b = We(
        a.call(
          $,
          _,
          f,
          u,
          p,
          g,
          v
        )
      ), w = l;
    } else {
      const _ = t;
      b = We(
        _.length > 1 ? _(
          u,
          { attrs: l, slots: o, emit: c }
        ) : _(
          u,
          null
        )
      ), w = t.props ? l : ra(l);
    }
  } catch (_) {
    ys.length = 0, mn(_, e, 1), b = ot(gt);
  }
  let I = b;
  if (w && m !== !1) {
    const _ = Object.keys(w), { shapeFlag: $ } = I;
    _.length && $ & 7 && (r && _.some(fn) && (w = oa(
      w,
      r
    )), I = qt(I, w, !1, !0));
  }
  return s.dirs && (I = qt(I, null, !1, !0), I.dirs = I.dirs ? I.dirs.concat(s.dirs) : s.dirs), s.transition && Ai(I, s.transition), b = I, en(y), b;
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
      return n ? fr(n, o, a) : !!o;
    if (c & 8) {
      const f = t.dynamicProps;
      for (let u = 0; u < f.length; u++) {
        const g = f[u];
        if ($o(o, n, g) && !bn(a, g))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? fr(n, o, a) : !0 : !!o;
  return !1;
}
function fr(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < n.length; i++) {
    const r = n[i];
    if ($o(t, e, r) && !bn(s, r))
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
const Po = {}, Mo = () => Object.create(Po), Do = (e) => Object.getPrototypeOf(e) === Po;
function aa(e, t, s, n = !1) {
  const i = {}, r = Mo();
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
        if (bn(e.emitsOptions, g))
          continue;
        const p = t[g];
        if (c)
          if (W(r, g))
            p !== r[g] && (r[g] = p, a = !0);
          else {
            const v = $e(g);
            i[v] = ii(
              c,
              l,
              v,
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
      s[f] !== void 0) && (i[u] = ii(
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
      if (us(c))
        continue;
      const a = t[c];
      let f;
      i && W(i, f = $e(c)) ? !r || !r.includes(f) ? s[f] = a : (l || (l = {}))[f] = a : bn(e.emitsOptions, c) || (!(c in n) || a !== n[c]) && (n[c] = a, o = !0);
    }
  if (r) {
    const c = /* @__PURE__ */ H(s), a = l || Q;
    for (let f = 0; f < r.length; f++) {
      const u = r[f];
      s[u] = ii(
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
function ii(e, t, s, n, i, r) {
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
          const f = Is(i);
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
  if (F(r))
    for (let f = 0; f < r.length; f++) {
      const u = $e(r[f]);
      ur(u) && (o[u] = Q);
    }
  else if (r)
    for (const f in r) {
      const u = $e(f);
      if (ur(u)) {
        const g = r[f], p = o[u] = F(g) || U(g) ? { type: g } : ae({}, g), v = p.type;
        let m = !1, y = !0;
        if (F(v))
          for (let b = 0; b < v.length; ++b) {
            const w = v[b], I = U(w) && w.name;
            if (I === "Boolean") {
              m = !0;
              break;
            } else I === "String" && (y = !1);
          }
        else
          m = U(v) && v.name === "Boolean";
        p[
          0
          /* shouldCast */
        ] = m, p[
          1
          /* shouldCastTrue */
        ] = y, (m || W(p, "default")) && l.push(u);
      }
    }
  const a = [o, l];
  return Y(e) && n.set(e, a), a;
}
function ur(e) {
  return e[0] !== "$" && !us(e);
}
const Ei = (e) => e === "_" || e === "_ctx" || e === "$stable", Ci = (e) => F(e) ? e.map(We) : [We(e)], ha = (e, t, s) => {
  if (t._n)
    return t;
  const n = Ec((...i) => Ci(t(...i)), s);
  return n._c = !1, n;
}, Fo = (e, t, s) => {
  const n = e._ctx;
  for (const i in e) {
    if (Ei(i)) continue;
    const r = e[i];
    if (U(r))
      t[i] = ha(i, r, n);
    else if (r != null) {
      const o = Ci(r);
      t[i] = () => o;
    }
  }
}, Bo = (e, t) => {
  const s = Ci(t);
  e.slots.default = () => s;
}, Ko = (e, t, s) => {
  for (const n in t)
    (s || !Ei(n)) && (e[n] = t[n]);
}, da = (e, t, s) => {
  const n = e.slots = Mo();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Ko(n, t, s), s && Gr(n, "_", i, !0)) : Fo(t, n);
  } else t && Bo(e, t);
}, pa = (e, t, s) => {
  const { vnode: n, slots: i } = e;
  let r = !0, o = Q;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? r = !1 : Ko(i, t, s) : (r = !t.$stable, Fo(t, i)), o = t;
  } else t && (Bo(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !Ei(l) && o[l] == null && delete i[l];
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
    nextSibling: g,
    setScopeId: p = Ye,
    insertStaticContent: v
  } = e, m = (h, d, S, N = null, T = null, k = null, C = void 0, E = null, A = !!d.dynamicChildren) => {
    if (h === d)
      return;
    h && !rs(h, d) && (N = js(h), je(h, T, k, !0), h = null), d.patchFlag === -2 && (A = !1, d.dynamicChildren = null);
    const { type: O, ref: R, shapeFlag: L } = d;
    switch (O) {
      case wn:
        y(h, d, S, N);
        break;
      case gt:
        b(h, d, S, N);
        break;
      case xn:
        h == null && w(d, S, N, C);
        break;
      case Te:
        fe(
          h,
          d,
          S,
          N,
          T,
          k,
          C,
          E,
          A
        );
        break;
      default:
        L & 1 ? $(
          h,
          d,
          S,
          N,
          T,
          k,
          C,
          E,
          A
        ) : L & 6 ? ue(
          h,
          d,
          S,
          N,
          T,
          k,
          C,
          E,
          A
        ) : (L & 64 || L & 128) && O.process(
          h,
          d,
          S,
          N,
          T,
          k,
          C,
          E,
          A,
          ss
        );
    }
    R != null && T ? ps(R, h && h.ref, k, d || h, !d) : R == null && h && h.ref != null && ps(h.ref, null, k, h, !0);
  }, y = (h, d, S, N) => {
    if (h == null)
      n(
        d.el = l(d.children),
        S,
        N
      );
    else {
      const T = d.el = h.el;
      d.children !== h.children && a(T, d.children);
    }
  }, b = (h, d, S, N) => {
    h == null ? n(
      d.el = c(d.children || ""),
      S,
      N
    ) : d.el = h.el;
  }, w = (h, d, S, N) => {
    [h.el, h.anchor] = v(
      h.children,
      d,
      S,
      N,
      h.el,
      h.anchor
    );
  }, I = ({ el: h, anchor: d }, S, N) => {
    let T;
    for (; h && h !== d; )
      T = g(h), n(h, S, N), h = T;
    n(d, S, N);
  }, _ = ({ el: h, anchor: d }) => {
    let S;
    for (; h && h !== d; )
      S = g(h), i(h), h = S;
    i(d);
  }, $ = (h, d, S, N, T, k, C, E, A) => {
    if (d.type === "svg" ? C = "svg" : d.type === "math" && (C = "mathml"), h == null)
      x(
        d,
        S,
        N,
        T,
        k,
        C,
        E,
        A
      );
    else {
      const O = h.el && h.el._isVueCE ? h.el : null;
      try {
        O && O._beginPatch(), q(
          h,
          d,
          T,
          k,
          C,
          E,
          A
        );
      } finally {
        O && O._endPatch();
      }
    }
  }, x = (h, d, S, N, T, k, C, E) => {
    let A, O;
    const { props: R, shapeFlag: L, transition: M, dirs: B } = h;
    if (A = h.el = o(
      h.type,
      k,
      R && R.is,
      R
    ), L & 8 ? f(A, h.children) : L & 16 && P(
      h.children,
      A,
      null,
      N,
      T,
      Kn(h, k),
      C,
      E
    ), B && bt(h, null, N, "created"), D(A, h, h.scopeId, C, N), R) {
      for (const G in R)
        G !== "value" && !us(G) && r(A, G, null, R[G], k, N);
      "value" in R && r(A, "value", null, R.value, k), (O = R.onVnodeBeforeMount) && xe(O, N, h);
    }
    B && bt(h, null, N, "beforeMount");
    const V = ya(T, M);
    V && M.beforeEnter(A), n(A, d, S), ((O = R && R.onVnodeMounted) || V || B) && _e(() => {
      try {
        O && xe(O, N, h), V && M.enter(A), B && bt(h, null, N, "mounted");
      } finally {
      }
    }, T);
  }, D = (h, d, S, N, T) => {
    if (S && p(h, S), N)
      for (let k = 0; k < N.length; k++)
        p(h, N[k]);
    if (T) {
      let k = T.subTree;
      if (d === k || qo(k.type) && (k.ssContent === d || k.ssFallback === d)) {
        const C = T.vnode;
        D(
          h,
          C,
          C.scopeId,
          C.slotScopeIds,
          T.parent
        );
      }
    }
  }, P = (h, d, S, N, T, k, C, E, A = 0) => {
    for (let O = A; O < h.length; O++) {
      const R = h[O] = E ? st(h[O]) : We(h[O]);
      m(
        null,
        R,
        d,
        S,
        N,
        T,
        k,
        C,
        E
      );
    }
  }, q = (h, d, S, N, T, k, C) => {
    const E = d.el = h.el;
    let { patchFlag: A, dynamicChildren: O, dirs: R } = d;
    A |= h.patchFlag & 16;
    const L = h.props || Q, M = d.props || Q;
    let B;
    if (S && wt(S, !1), (B = M.onVnodeBeforeUpdate) && xe(B, S, d, h), R && bt(d, h, S, "beforeUpdate"), S && wt(S, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    O && (!h.dynamicChildren || h.dynamicChildren.length !== O.length) && (A = 0, C = !1, O = null), (L.innerHTML && M.innerHTML == null || L.textContent && M.textContent == null) && f(E, ""), O ? ee(
      h.dynamicChildren,
      O,
      E,
      S,
      N,
      Kn(d, T),
      k
    ) : C || X(
      h,
      d,
      E,
      null,
      S,
      N,
      Kn(d, T),
      k,
      !1
    ), A > 0) {
      if (A & 16)
        pe(E, L, M, S, T);
      else if (A & 2 && L.class !== M.class && r(E, "class", null, M.class, T), A & 4 && r(E, "style", L.style, M.style, T), A & 8) {
        const V = d.dynamicProps;
        for (let G = 0; G < V.length; G++) {
          const J = V[G], oe = L[J], le = M[J];
          (le !== oe || J === "value") && r(E, J, oe, le, T, S);
        }
      }
      A & 1 && h.children !== d.children && f(E, d.children);
    } else !C && O == null && pe(E, L, M, S, T);
    ((B = M.onVnodeUpdated) || R) && _e(() => {
      B && xe(B, S, d, h), R && bt(d, h, S, "updated");
    }, N);
  }, ee = (h, d, S, N, T, k, C) => {
    for (let E = 0; E < d.length; E++) {
      const A = h[E], O = d[E], R = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        A.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (A.type === Te || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !rs(A, O) || // - In the case of a component, it could contain anything.
        A.shapeFlag & 198) ? u(A.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          S
        )
      );
      m(
        A,
        O,
        R,
        null,
        N,
        T,
        k,
        C,
        !0
      );
    }
  }, pe = (h, d, S, N, T) => {
    if (d !== S) {
      if (d !== Q)
        for (const k in d)
          !us(k) && !(k in S) && r(
            h,
            k,
            d[k],
            null,
            T,
            N
          );
      for (const k in S) {
        if (us(k)) continue;
        const C = S[k], E = d[k];
        C !== E && k !== "value" && r(h, k, E, C, T, N);
      }
      "value" in S && r(h, "value", d.value, S.value, T);
    }
  }, fe = (h, d, S, N, T, k, C, E, A) => {
    const O = d.el = h ? h.el : l(""), R = d.anchor = h ? h.anchor : l("");
    let { patchFlag: L, dynamicChildren: M, slotScopeIds: B } = d;
    B && (E = E ? E.concat(B) : B), h == null ? (n(O, S, N), n(R, S, N), P(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      d.children || [],
      S,
      R,
      T,
      k,
      C,
      E,
      A
    )) : L > 0 && L & 64 && M && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    h.dynamicChildren && h.dynamicChildren.length === M.length ? (ee(
      h.dynamicChildren,
      M,
      S,
      T,
      k,
      C,
      E
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
      S,
      R,
      T,
      k,
      C,
      E,
      A
    );
  }, ue = (h, d, S, N, T, k, C, E, A) => {
    d.slotScopeIds = E, h == null ? d.shapeFlag & 512 ? T.ctx.activate(
      d,
      S,
      N,
      C,
      A
    ) : Et(
      d,
      S,
      N,
      T,
      k,
      C,
      A
    ) : Wi(h, d, A);
  }, Et = (h, d, S, N, T, k, C) => {
    const E = h.component = Ea(
      h,
      N,
      T
    );
    if (Oo(h) && (E.ctx.renderer = ss), Ia(E, !1, C), E.asyncDep) {
      if (T && T.registerDep(E, ge, C), !h.el) {
        const A = E.subTree = ot(gt);
        b(null, A, d, S), h.placeholder = A.el;
      }
    } else
      ge(
        E,
        h,
        d,
        S,
        T,
        k,
        C
      );
  }, Wi = (h, d, S) => {
    const N = d.component = h.component;
    if (la(h, d, S))
      if (N.asyncDep && !N.asyncResolved) {
        te(N, d, S);
        return;
      } else
        N.next = d, N.update();
    else
      d.el = h.el, N.vnode = d;
  }, ge = (h, d, S, N, T, k, C) => {
    const E = () => {
      if (h.isMounted) {
        let { next: L, bu: M, u: B, parent: V, vnode: G } = h;
        {
          const Be = Uo(h);
          if (Be) {
            L && (L.el = G.el, te(h, L, C)), Be.asyncDep.then(() => {
              _e(() => {
                h.isUnmounted || O();
              }, T);
            });
            return;
          }
        }
        let J = L, oe;
        wt(h, !1), L ? (L.el = G.el, te(h, L, C)) : L = G, M && Ws(M), (oe = L.props && L.props.onVnodeBeforeUpdate) && xe(oe, V, L, G), wt(h, !0);
        const le = ar(h), Fe = h.subTree;
        h.subTree = le, m(
          Fe,
          le,
          // parent may have changed if it's in a teleport
          u(Fe.el),
          // anchor may have changed if it's in a fragment
          js(Fe),
          h,
          T,
          k
        ), L.el = le.el, J === null && ca(h, le.el), B && _e(B, T), (oe = L.props && L.props.onVnodeUpdated) && _e(
          () => xe(oe, V, L, G),
          T
        );
      } else {
        let L;
        const { el: M, props: B } = d, { bm: V, m: G, parent: J, root: oe, type: le } = h, Fe = gs(d);
        wt(h, !1), V && Ws(V), !Fe && (L = B && B.onVnodeBeforeMount) && xe(L, J, d), wt(h, !0);
        {
          oe.ce && oe.ce._hasShadowRoot() && oe.ce._injectChildStyle(
            le,
            h.parent ? h.parent.type : void 0
          );
          const Be = h.subTree = ar(h);
          m(
            null,
            Be,
            S,
            N,
            h,
            T,
            k
          ), d.el = Be.el;
        }
        if (G && _e(G, T), !Fe && (L = B && B.onVnodeMounted)) {
          const Be = d;
          _e(
            () => xe(L, J, Be),
            T
          );
        }
        (d.shapeFlag & 256 || J && gs(J.vnode) && J.vnode.shapeFlag & 256) && h.a && _e(h.a, T), h.isMounted = !0, d = S = N = null;
      }
    };
    h.scope.on();
    const A = h.effect = new Zr(E);
    h.scope.off();
    const O = h.update = A.run.bind(A), R = h.job = A.runIfDirty.bind(A);
    R.i = h, R.id = h.uid, A.scheduler = () => Ni(R), wt(h, !0), O();
  }, te = (h, d, S) => {
    d.component = h;
    const N = h.vnode.props;
    h.vnode = d, h.next = null, fa(h, d.props, N, S), pa(h, d.children, S), Qe(), sr(h), Xe();
  }, X = (h, d, S, N, T, k, C, E, A = !1) => {
    const O = h && h.children, R = h ? h.shapeFlag : 0, L = d.children, { patchFlag: M, shapeFlag: B } = d;
    if (M > 0) {
      if (M & 128) {
        Rs(
          O,
          L,
          S,
          N,
          T,
          k,
          C,
          E,
          A
        );
        return;
      } else if (M & 256) {
        mt(
          O,
          L,
          S,
          N,
          T,
          k,
          C,
          E,
          A
        );
        return;
      }
    }
    B & 8 ? (R & 16 && ts(O, T, k), L !== O && f(S, L)) : R & 16 ? B & 16 ? Rs(
      O,
      L,
      S,
      N,
      T,
      k,
      C,
      E,
      A
    ) : ts(O, T, k, !0) : (R & 8 && f(S, ""), B & 16 && P(
      L,
      S,
      N,
      T,
      k,
      C,
      E,
      A
    ));
  }, mt = (h, d, S, N, T, k, C, E, A) => {
    h = h || jt, d = d || jt;
    const O = h.length, R = d.length, L = Math.min(O, R);
    let M;
    for (M = 0; M < L; M++) {
      const B = d[M] = A ? st(d[M]) : We(d[M]);
      m(
        h[M],
        B,
        S,
        null,
        T,
        k,
        C,
        E,
        A
      );
    }
    O > R ? ts(
      h,
      T,
      k,
      !0,
      !1,
      L
    ) : P(
      d,
      S,
      N,
      T,
      k,
      C,
      E,
      A,
      L
    );
  }, Rs = (h, d, S, N, T, k, C, E, A) => {
    let O = 0;
    const R = d.length;
    let L = h.length - 1, M = R - 1;
    for (; O <= L && O <= M; ) {
      const B = h[O], V = d[O] = A ? st(d[O]) : We(d[O]);
      if (rs(B, V))
        m(
          B,
          V,
          S,
          null,
          T,
          k,
          C,
          E,
          A
        );
      else
        break;
      O++;
    }
    for (; O <= L && O <= M; ) {
      const B = h[L], V = d[M] = A ? st(d[M]) : We(d[M]);
      if (rs(B, V))
        m(
          B,
          V,
          S,
          null,
          T,
          k,
          C,
          E,
          A
        );
      else
        break;
      L--, M--;
    }
    if (O > L) {
      if (O <= M) {
        const B = M + 1, V = B < R ? d[B].el : N;
        for (; O <= M; )
          m(
            null,
            d[O] = A ? st(d[O]) : We(d[O]),
            S,
            V,
            T,
            k,
            C,
            E,
            A
          ), O++;
      }
    } else if (O > M)
      for (; O <= L; )
        je(h[O], T, k, !0), O++;
    else {
      const B = O, V = O, G = /* @__PURE__ */ new Map();
      for (O = V; O <= M; O++) {
        const ke = d[O] = A ? st(d[O]) : We(d[O]);
        ke.key != null && G.set(ke.key, O);
      }
      let J, oe = 0;
      const le = M - V + 1;
      let Fe = !1, Be = 0;
      const ns = new Array(le);
      for (O = 0; O < le; O++) ns[O] = 0;
      for (O = B; O <= L; O++) {
        const ke = h[O];
        if (oe >= le) {
          je(ke, T, k, !0);
          continue;
        }
        let Ke;
        if (ke.key != null)
          Ke = G.get(ke.key);
        else
          for (J = V; J <= M; J++)
            if (ns[J - V] === 0 && rs(ke, d[J])) {
              Ke = J;
              break;
            }
        Ke === void 0 ? je(ke, T, k, !0) : (ns[Ke - V] = O + 1, Ke >= Be ? Be = Ke : Fe = !0, m(
          ke,
          d[Ke],
          S,
          null,
          T,
          k,
          C,
          E,
          A
        ), oe++);
      }
      const Gi = Fe ? ba(ns) : jt;
      for (J = Gi.length - 1, O = le - 1; O >= 0; O--) {
        const ke = V + O, Ke = d[ke], Qi = d[ke + 1], Xi = ke + 1 < R ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          Qi.el || Vo(Qi)
        ) : N;
        ns[O] === 0 ? m(
          null,
          Ke,
          S,
          Xi,
          T,
          k,
          C,
          E,
          A
        ) : Fe && (J < 0 || O !== Gi[J] ? yt(Ke, S, Xi, 2) : J--);
      }
    }
  }, yt = (h, d, S, N, T = null) => {
    const { el: k, type: C, transition: E, children: A, shapeFlag: O } = h;
    if (O & 6) {
      yt(h.component.subTree, d, S, N);
      return;
    }
    if (O & 128) {
      h.suspense.move(d, S, N);
      return;
    }
    if (O & 64) {
      C.move(h, d, S, ss);
      return;
    }
    if (C === Te) {
      n(k, d, S);
      for (let L = 0; L < A.length; L++)
        yt(A[L], d, S, N);
      n(h.anchor, d, S);
      return;
    }
    if (C === xn) {
      I(h, d, S);
      return;
    }
    if (N !== 2 && O & 1 && E)
      if (N === 0)
        E.persisted && !k[jn] ? n(k, d, S) : (E.beforeEnter(k), n(k, d, S), _e(() => E.enter(k), T));
      else {
        const { leave: L, delayLeave: M, afterLeave: B } = E, V = () => {
          h.ctx.isUnmounted ? i(k) : n(k, d, S);
        }, G = () => {
          const J = k._isLeaving || !!k[jn];
          k._isLeaving && k[jn](
            !0
            /* cancelled */
          ), E.persisted && !J ? V() : L(k, () => {
            V(), B && B();
          });
        };
        M ? M(k, V, G) : G();
      }
    else
      n(k, d, S);
  }, je = (h, d, S, N = !1, T = !1) => {
    const {
      type: k,
      props: C,
      ref: E,
      children: A,
      dynamicChildren: O,
      shapeFlag: R,
      patchFlag: L,
      dirs: M,
      cacheIndex: B,
      memo: V
    } = h;
    if (L === -2 && (T = !1), E != null && (Qe(), ps(E, null, S, h, !0), Xe()), B != null && (d.renderCache[B] = void 0), R & 256) {
      d.ctx.deactivate(h);
      return;
    }
    const G = R & 1 && M, J = !gs(h);
    let oe;
    if (J && (oe = C && C.onVnodeBeforeUnmount) && xe(oe, d, h), R & 6)
      Fl(h.component, S, N);
    else {
      if (R & 128) {
        h.suspense.unmount(S, N);
        return;
      }
      G && bt(h, null, d, "beforeUnmount"), R & 64 ? h.type.remove(
        h,
        d,
        S,
        ss,
        N
      ) : O && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !O.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (k !== Te || L > 0 && L & 64) ? ts(
        O,
        d,
        S,
        !1,
        !0
      ) : (k === Te && L & 384 || !T && R & 16) && ts(A, d, S), N && Ji(h);
    }
    const le = V != null && B == null;
    (J && (oe = C && C.onVnodeUnmounted) || G || le) && _e(() => {
      oe && xe(oe, d, h), G && bt(h, null, d, "unmounted"), le && (h.el = null);
    }, S);
  }, Ji = (h) => {
    const { type: d, el: S, anchor: N, transition: T } = h;
    if (d === Te) {
      jl(S, N);
      return;
    }
    if (d === xn) {
      _(h);
      return;
    }
    const k = () => {
      i(S), T && !T.persisted && T.afterLeave && T.afterLeave();
    };
    if (h.shapeFlag & 1 && T && !T.persisted) {
      const { leave: C, delayLeave: E } = T, A = () => C(S, k);
      E ? E(h.el, k, A) : A();
    } else
      k();
  }, jl = (h, d) => {
    let S;
    for (; h !== d; )
      S = g(h), i(h), h = S;
    i(d);
  }, Fl = (h, d, S) => {
    const { bum: N, scope: T, job: k, subTree: C, um: E, m: A, a: O } = h;
    hr(A), hr(O), N && Ws(N), T.stop(), k && (k.flags |= 8, je(C, h, d, S)), E && _e(E, d), _e(() => {
      h.isUnmounted = !0;
    }, d);
  }, ts = (h, d, S, N = !1, T = !1, k = 0) => {
    for (let C = k; C < h.length; C++)
      je(h[C], d, S, N, T);
  }, js = (h) => {
    if (h.shapeFlag & 6)
      return js(h.component.subTree);
    if (h.shapeFlag & 128)
      return h.suspense.next();
    const d = g(h.anchor || h.el), S = d && d[Pc];
    return S ? g(S) : d;
  };
  let Cn = !1;
  const Yi = (h, d, S) => {
    let N;
    h == null ? d._vnode && (je(d._vnode, null, null, !0), N = d._vnode.component) : m(
      d._vnode || null,
      h,
      d,
      null,
      null,
      null,
      S
    ), d._vnode = h, Cn || (Cn = !0, sr(N), bo(), Cn = !1);
  }, ss = {
    p: m,
    um: je,
    m: yt,
    r: Ji,
    mt: Et,
    mc: P,
    pc: X,
    pbc: ee,
    n: js,
    o: e
  };
  return {
    render: Yi,
    hydrate: void 0,
    createApp: ta(Yi)
  };
}
function Kn({ type: e, props: t }, s) {
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
  if (F(n) && F(i))
    for (let r = 0; r < n.length; r++) {
      const o = n[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = st(i[r]), l.el = o.el), !s && l.patchFlag !== -2 && xo(o, l)), l.type === wn && (l.patchFlag === -1 && (l = i[r] = st(l)), l.el = o.el), l.type === gt && !l.el && (l.el = o.el);
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
function hr(e) {
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
  t && t.pendingBranch ? F(e) ? t.effects.push(...e) : t.effects.push(e) : Ac(e);
}
const Te = /* @__PURE__ */ Symbol.for("v-fgt"), wn = /* @__PURE__ */ Symbol.for("v-txt"), gt = /* @__PURE__ */ Symbol.for("v-cmt"), xn = /* @__PURE__ */ Symbol.for("v-stc"), ys = [];
let Oe = null;
function Ve(e = !1) {
  ys.push(Oe = e ? null : []);
}
function Sa() {
  ys.pop(), Oe = ys[ys.length - 1] || null;
}
let vs = 1;
function dr(e, t = !1) {
  vs += e, e < 0 && Oe && t && (Oe.hasOnce = !0);
}
function Ho(e) {
  return e.dynamicChildren = vs > 0 ? Oe || jt : null, Sa(), vs > 0 && Oe && Oe.push(e), e;
}
function et(e, t, s, n, i, r) {
  return Ho(
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
const Jo = ({ key: e }) => e ?? null, Ys = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? se(e) || /* @__PURE__ */ de(e) || U(e) ? { i: Ae, r: e, k: t, f: !!s } : e : null);
function j(e, t = null, s = null, n = 0, i = null, r = e === Te ? 0 : 1, o = !1, l = !1) {
  const c = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Jo(t),
    ref: t && Ys(t),
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
    ctx: Ae
  };
  return l ? (nn(c, s), r & 128 && e.normalize(c)) : s && (c.shapeFlag |= se(s) ? 8 : 16), vs > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  Oe && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (c.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  c.patchFlag !== 32 && Oe.push(c), c;
}
const ot = ka;
function ka(e, t = null, s = null, n = 0, i = null, r = !1) {
  if ((!e || e === Jc) && (e = gt), Wo(e)) {
    const l = qt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && nn(l, s), vs > 0 && !r && Oe && (l.shapeFlag & 6 ? Oe[Oe.indexOf(e)] = l : Oe.push(l)), l.patchFlag = -2, l;
  }
  if (Ma(e) && (e = e.__vccOpts), t) {
    t = va(t);
    let { class: l, style: c } = t;
    l && !se(l) && (t.class = yi(l)), Y(c) && (/* @__PURE__ */ Ti(c) && !F(c) && (c = ae({}, c)), t.style = mi(c));
  }
  const o = se(e) ? 1 : qo(e) ? 128 : Mc(e) ? 64 : Y(e) ? 4 : U(e) ? 2 : 0;
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
function va(e) {
  return e ? /* @__PURE__ */ Ti(e) || Do(e) ? ae({}, e) : e : null;
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
      s && r ? F(r) ? r.concat(Ys(t)) : [r, Ys(t)] : Ys(t)
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
  return c && n && Ai(
    f,
    c.clone(f)
  ), f;
}
function Oa(e = " ", t = 0) {
  return ot(wn, null, e, t);
}
function pr(e = "", t = !1) {
  return t ? (Ve(), _a(gt, null, e)) : ot(gt, null, e);
}
function We(e) {
  return e == null || typeof e == "boolean" ? ot(gt) : F(e) ? ot(
    Te,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Wo(e) ? st(e) : ot(wn, null, String(e));
}
function st(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : qt(e);
}
function nn(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (F(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), nn(e, i()), i._c && (i._d = !0));
      return;
    } else {
      s = 32;
      const i = t._;
      !i && !Do(t) ? t._ctx = Ae : i === 3 && Ae && (Ae.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (U(t)) {
    if (n & 65) {
      nn(e, { default: t });
      return;
    }
    t = { default: t, _ctx: Ae }, s = 32;
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
        t.class !== n.class && (t.class = yi([t.class, n.class]));
      else if (i === "style")
        t.style = mi([t.style, n.style]);
      else if (an(i)) {
        const r = t[i], o = n[i];
        o && r !== o && !(F(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !fn(i) && (t[i] = o);
      } else i !== "" && (t[i] = n[i]);
  }
  return t;
}
function xe(e, t, s, n = null) {
  De(e, t, 7, [
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
let we = null;
const Ca = () => we || Ae;
let rn, ri;
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
  ), ri = t(
    "__VUE_SSR_SETTERS__",
    (s) => Os = s
  );
}
const Is = (e) => {
  const t = we;
  return rn(e), e.scope.on(), () => {
    e.scope.off(), rn(t);
  };
}, gr = () => {
  we && we.scope.off(), rn(null);
};
function Yo(e) {
  return e.vnode.shapeFlag & 4;
}
let Os = !1;
function Ia(e, t = !1, s = !1) {
  t && ri(t);
  const { props: n, children: i } = e.vnode, r = Yo(e);
  aa(e, n, r, t), da(e, i, s || t);
  const o = r ? La(e, t) : void 0;
  return t && ri(!1), o;
}
function La(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, Yc);
  const { setup: n } = s;
  if (n) {
    Qe();
    const i = e.setupContext = n.length > 1 ? Pa(e) : null, r = Is(e), o = Cs(
      n,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = Hr(o);
    if (Xe(), r(), (l || e.sp) && !gs(e) && vo(e), l) {
      if (o.then(gr, gr), t)
        return o.then((c) => {
          mr(e, c);
        }).catch((c) => {
          mn(c, e, 0);
        });
      e.asyncDep = o;
    } else
      mr(e, o);
  } else
    Go(e);
}
function mr(e, t, s) {
  U(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : Y(t) && (e.setupState = po(t)), Go(e);
}
function Go(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Ye);
  {
    const i = Is(e);
    Qe();
    try {
      Gc(e);
    } finally {
      Xe(), i();
    }
  }
}
const $a = {
  get(e, t) {
    return he(e, "get", ""), e[t];
  }
};
function Pa(e) {
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
function Sn(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(po(mc(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in ms)
        return ms[s](e);
    },
    has(t, s) {
      return s in t || s in ms;
    }
  })) : e.proxy;
}
function Ma(e) {
  return U(e) && "__vccOpts" in e;
}
const Da = (e, t) => /* @__PURE__ */ kc(e, t, Os), Ra = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let oi;
const yr = typeof window < "u" && window.trustedTypes;
if (yr)
  try {
    oi = /* @__PURE__ */ yr.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Qo = oi ? (e) => oi.createHTML(e) : (e) => e, ja = "http://www.w3.org/2000/svg", Fa = "http://www.w3.org/1998/Math/MathML", tt = typeof document < "u" ? document : null, br = tt && /* @__PURE__ */ tt.createElement("template"), Ba = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const i = t === "svg" ? tt.createElementNS(ja, e) : t === "mathml" ? tt.createElementNS(Fa, e) : s ? tt.createElement(e, { is: s }) : tt.createElement(e);
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
      br.innerHTML = Qo(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = br.content;
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
const wr = /* @__PURE__ */ Symbol("_vod"), Ua = /* @__PURE__ */ Symbol("_vsh"), Va = /* @__PURE__ */ Symbol(""), qa = /(?:^|;)\s*display\s*:/;
function Ha(e, t, s) {
  const n = e.style, i = se(s);
  let r = !1;
  if (s && !i) {
    if (t)
      if (se(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && cs(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && cs(n, o, "");
    for (const o in s) {
      o === "display" && (r = !0);
      const l = s[o];
      l != null ? Ja(
        e,
        o,
        !se(t) && t ? t[o] : void 0,
        l
      ) || cs(n, o, l) : cs(n, o, "");
    }
  } else if (i) {
    if (t !== s) {
      const o = n[Va];
      o && (s += ";" + o), n.cssText = s, r = qa.test(s);
    }
  } else t && e.removeAttribute("style");
  wr in e && (e[wr] = r ? n.display : "", e[Ua] && (n.display = "none"));
}
const Sr = /\s*!important$/;
function cs(e, t, s) {
  if (F(s))
    s.forEach((n) => cs(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = Wa(e, t);
    Sr.test(s) ? e.setProperty(
      At(n),
      s.replace(Sr, ""),
      "important"
    ) : e[n] = s;
  }
}
const _r = ["Webkit", "Moz", "ms"], Un = {};
function Wa(e, t) {
  const s = Un[t];
  if (s)
    return s;
  let n = $e(t);
  if (n !== "filter" && n in e)
    return Un[t] = n;
  n = Yr(n);
  for (let i = 0; i < _r.length; i++) {
    const r = _r[i] + n;
    if (r in e)
      return Un[t] = r;
  }
  return t;
}
function Ja(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && se(n) && s === n;
}
const kr = "http://www.w3.org/1999/xlink";
function vr(e, t, s, n, i, r = Yl(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(kr, t.slice(6, t.length)) : e.setAttributeNS(kr, t, s) : s == null || r && !Qr(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : Ge(s) ? String(s) : s
  );
}
function Or(e, t, s, n, i) {
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
const Tr = /* @__PURE__ */ Symbol("_vei");
function Ga(e, t, s, n, i = null) {
  const r = e[Tr] || (e[Tr] = {}), o = r[t];
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
let Vn = 0;
const Za = /* @__PURE__ */ Promise.resolve(), ef = () => Vn || (Za.then(() => Vn = 0), Vn = Date.now());
function tf(e, t) {
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
      for (let c = 0; c < o.length && !n._stopped; c++) {
        const a = o[c];
        a && De(
          a,
          t,
          5,
          l
        );
      }
    } else
      De(
        i,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = ef(), s;
}
const Nr = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, sf = (e, t, s, n, i, r) => {
  const o = i === "svg";
  t === "class" ? xa(e, n, o) : t === "style" ? Ha(e, s, n) : an(t) ? fn(t) || Ga(e, t, s, n, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : nf(e, t, n, o)) ? (Or(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && vr(e, t, n, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (rf(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !se(n))) ? Or(e, $e(t), n, r, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), vr(e, t, n, o));
};
function nf(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Nr(t) && U(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return Nr(t) && se(s) ? !1 : t in e;
}
function rf(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = $e(t);
  return Array.isArray(s) ? s.some((i) => $e(i) === n) : Object.keys(s).some((i) => $e(i) === n);
}
const Ht = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return F(t) ? (s) => Ws(t, s) : t;
};
function of(e) {
  e.target.composing = !0;
}
function Ar(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const lt = /* @__PURE__ */ Symbol("_assign");
function Er(e, t, s) {
  return t && (e = e.trim()), s && (e = hn(e)), e;
}
const Ue = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, i) {
    e[lt] = Ht(i);
    const r = n || i.props && i.props.type === "number";
    dt(e, t ? "change" : "input", (o) => {
      o.target.composing || e[lt](Er(e.value, s, r));
    }), (s || r) && dt(e, "change", () => {
      e.value = Er(e.value, s, r);
    }), t || (dt(e, "compositionstart", of), dt(e, "compositionend", Ar), dt(e, "change", Ar));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: i, number: r } }, o) {
    if (e[lt] = Ht(o), e.composing) return;
    const l = (r || e.type === "number") && !/^0\d/.test(e.value) ? hn(e.value) : e.value, c = t ?? "";
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
      const n = e._modelValue, i = Ts(e), r = e.checked, o = e[lt];
      if (F(n)) {
        const l = bi(n, i), c = l !== -1;
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
  mounted: Cr,
  beforeUpdate(e, t, s) {
    e[lt] = Ht(s), Cr(e, t, s);
  }
};
function Cr(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let i;
  if (F(t))
    i = bi(t, n.props.value) > -1;
  else if (Yt(t))
    i = t.has(n.props.value);
  else {
    if (t === s) return;
    i = Gt(t, Xo(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const Ir = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const i = Yt(t);
    dt(e, "change", () => {
      const r = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? hn(Ts(o)) : Ts(o)
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
  const s = e.multiple, n = F(t);
  if (!(s && !n && !Yt(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = Ts(o);
      if (s)
        if (n) {
          const c = typeof l;
          c === "string" || c === "number" ? o.selected = t.some((a) => String(a) === String(l)) : o.selected = bi(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (Gt(Ts(o), t)) {
        e.selectedIndex !== i && (e.selectedIndex = i);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function Ts(e) {
  return "_value" in e ? e._value : e.value;
}
function Xo(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
}
const cf = /* @__PURE__ */ ae({ patchProp: sf }, Ba);
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
const Ii = Symbol.for("yaml.alias"), li = Symbol.for("yaml.document"), pt = Symbol.for("yaml.map"), zo = Symbol.for("yaml.pair"), ze = Symbol.for("yaml.scalar"), Qt = Symbol.for("yaml.seq"), Ie = Symbol.for("yaml.node.type"), Xt = (e) => !!e && typeof e == "object" && e[Ie] === Ii, Ls = (e) => !!e && typeof e == "object" && e[Ie] === li, $s = (e) => !!e && typeof e == "object" && e[Ie] === pt, re = (e) => !!e && typeof e == "object" && e[Ie] === zo, Z = (e) => !!e && typeof e == "object" && e[Ie] === ze, Ps = (e) => !!e && typeof e == "object" && e[Ie] === Qt;
function ne(e) {
  if (e && typeof e == "object")
    switch (e[Ie]) {
      case pt:
      case Qt:
        return !0;
    }
  return !1;
}
function ie(e) {
  if (e && typeof e == "object")
    switch (e[Ie]) {
      case Ii:
      case pt:
      case ze:
      case Qt:
        return !0;
    }
  return !1;
}
const Zo = (e) => (Z(e) || ne(e)) && !!e.anchor, _t = Symbol("break visit"), df = Symbol("skip children"), bs = Symbol("remove node");
function zt(e, t) {
  const s = pf(t);
  Ls(e) ? Mt(null, e.contents, s, Object.freeze([e])) === bs && (e.contents = null) : Mt(null, e, s, Object.freeze([]));
}
zt.BREAK = _t;
zt.SKIP = df;
zt.REMOVE = bs;
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
          o === bs && (t.items.splice(r, 1), r -= 1);
        }
      }
    } else if (re(t)) {
      n = Object.freeze(n.concat(t));
      const r = Mt("key", t.key, s, n);
      if (r === _t)
        return _t;
      r === bs && (t.key = null);
      const o = Mt("value", t.value, s, n);
      if (o === _t)
        return _t;
      o === bs && (t.value = null);
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
  if ($s(t))
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
  else if (Ls(n))
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
ye.defaultYaml = { explicit: !1, version: "1.2" };
ye.defaultTags = { "!!": "tag:yaml.org,2002:" };
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
function Ce(e, t, s) {
  if (Array.isArray(e))
    return e.map((n, i) => Ce(n, String(i), s));
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
class Li {
  constructor(t) {
    Object.defineProperty(this, Ie, { value: t });
  }
  /** Create a copy of this node.  */
  clone() {
    const t = Object.create(Object.getPrototypeOf(this), Object.getOwnPropertyDescriptors(this));
    return this.range && (t.range = this.range.slice()), t;
  }
  /** A plain JavaScript representation of this node. */
  toJS(t, { mapAsMap: s, maxAliasCount: n, onAnchor: i, reviver: r } = {}) {
    if (!Ls(t))
      throw new TypeError("A document argument is required");
    const o = {
      anchors: /* @__PURE__ */ new Map(),
      doc: t,
      keep: !0,
      mapAsMap: s === !0,
      mapKeyWarned: !1,
      maxAliasCount: typeof n == "number" ? n : 100
    }, l = Ce(this, "", o);
    if (typeof i == "function")
      for (const { count: c, res: a } of o.anchors.values())
        i(a, c);
    return typeof r == "function" ? Dt(r, { "": l }, "", l) : l;
  }
}
class $i extends Li {
  constructor(t) {
    super(Ii), this.source = t, Object.defineProperty(this, "tag", {
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
    if (l || (Ce(o, null, s), l = n.get(o)), (l == null ? void 0 : l.res) === void 0) {
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
function Gs(e, t, s) {
  if (Xt(t)) {
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
const nl = (e) => !e || typeof e != "function" && typeof e != "object";
class K extends Li {
  constructor(t) {
    super(ze), this.value = t;
  }
  toJSON(t, s) {
    return s != null && s.keep ? this.value : Ce(this.value, t, s);
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
function Ns(e, t, s) {
  var u, g, p;
  if (Ls(e) && (e = e.contents), ie(e))
    return e;
  if (re(e)) {
    const v = (g = (u = s.schema[pt]).createNode) == null ? void 0 : g.call(u, s.schema, null, s);
    return v.items.push(e), v;
  }
  (e instanceof String || e instanceof Number || e instanceof Boolean || typeof BigInt < "u" && e instanceof BigInt) && (e = e.valueOf());
  const { aliasDuplicateObjects: n, onAnchor: i, onTagObj: r, schema: o, sourceObjects: l } = s;
  let c;
  if (n && e && typeof e == "object") {
    if (c = l.get(e), c)
      return c.anchor ?? (c.anchor = i(e)), new $i(c.anchor);
    c = { anchor: null, node: null }, l.set(e, c);
  }
  t != null && t.startsWith("!!") && (t = Sf + t.slice(2));
  let a = _f(e, t, o.tags);
  if (!a) {
    if (e && typeof e.toJSON == "function" && (e = e.toJSON()), !e || typeof e != "object") {
      const v = new K(e);
      return c && (c.node = v), v;
    }
    a = e instanceof Map ? o[pt] : Symbol.iterator in Object(e) ? o[Qt] : o[pt];
  }
  r && (r(a), delete s.onTagObj);
  const f = a != null && a.createNode ? a.createNode(s.schema, e, s) : typeof ((p = a == null ? void 0 : a.nodeClass) == null ? void 0 : p.from) == "function" ? a.nodeClass.from(s.schema, e, s) : new K(e);
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
  return Ns(n, void 0, {
    aliasDuplicateObjects: !1,
    keepUndefined: !1,
    onAnchor: () => {
      throw new Error("This should not happen, please report a bug.");
    },
    schema: e,
    sourceObjects: /* @__PURE__ */ new Map()
  });
}
const as = (e) => e == null || typeof e == "object" && !!e[Symbol.iterator]().next().done;
class il extends Li {
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
    if (as(t))
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
const kf = (e) => e.replace(/^(?!$)(?: $)?/gm, "#");
function rt(e, t) {
  return /^\n+$/.test(e) ? e.substring(1) : t ? e.replace(/^(?! *$)/gm, t) : e;
}
const kt = (e, t, s) => e.endsWith(`
`) ? rt(s, t) : s.includes(`
`) ? `
` + rt(s, t) : (e.endsWith(" ") ? "" : " ") + s, rl = "flow", ci = "block", Qs = "quoted";
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
  let g, p, v = !1, m = -1, y = -1, b = -1;
  s === ci && (m = Pr(e, m, t.length), m !== -1 && (u = m + c));
  for (let I; I = e[m += 1]; ) {
    if (s === Qs && I === "\\") {
      switch (y = m, e[m + 1]) {
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
    if (I === `
`)
      s === ci && (m = Pr(e, m, t.length)), u = m + t.length + c, g = void 0;
    else {
      if (I === " " && p && p !== " " && p !== `
` && p !== "	") {
        const _ = e[m + 1];
        _ && _ !== " " && _ !== `
` && _ !== "	" && (g = m);
      }
      if (m >= u)
        if (g)
          a.push(g), u = g + c, g = void 0;
        else if (s === Qs) {
          for (; p === " " || p === "	"; )
            p = I, I = e[m += 1], v = !0;
          const _ = m > b + 1 ? m - 2 : y - 1;
          if (f[_])
            return e;
          a.push(_), f[_] = !0, u = _ + c, g = void 0;
        } else
          v = !0;
    }
    p = I;
  }
  if (v && l && l(), a.length === 0)
    return e;
  o && o();
  let w = e.slice(0, a[0]);
  for (let I = 0; I < a.length; ++I) {
    const _ = a[I], $ = a[I + 1] || e.length;
    _ === 0 ? w = `
${t}${e.slice(0, $)}` : (s === Qs && f[_] && (w += `${e[_]}\\`), w += `
${t}${e.slice(_ + 1, $)}`);
  }
  return w;
}
function Pr(e, t, s) {
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
}), vn = (e) => /^(%|---|\.\.\.)/m.test(e);
function vf(e, t, s) {
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
function ws(e, t) {
  const s = JSON.stringify(e);
  if (t.options.doubleQuotedAsJSON)
    return s;
  const { implicitKey: n } = t, i = t.options.doubleQuotedMinMultiLineLength, r = t.indent || (vn(e) ? "  " : "");
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
  return o = l ? o + s.slice(l) : s, n ? o : _n(o, r, Qs, kn(t, !1));
}
function ai(e, t) {
  if (t.options.singleQuote === !1 || t.implicitKey && e.includes(`
`) || /[ \t]\n|\n[ \t]/.test(e))
    return ws(e, t);
  const s = t.indent || (vn(e) ? "  " : ""), n = "'" + e.replace(/'/g, "''").replace(/\n+/g, `$&
${s}`) + "'";
  return t.implicitKey ? n : _n(n, s, rl, kn(t, !1));
}
function Rt(e, t) {
  const { singleQuote: s } = t.options;
  let n;
  if (s === !1)
    n = ws;
  else {
    const i = e.includes('"'), r = e.includes("'");
    i && !r ? n = ai : r && !i ? n = ws : n = s ? ai : ws;
  }
  return n(e, t);
}
let fi;
try {
  fi = new RegExp(`(^|(?<!
))
+(?!
|$)`, "g");
} catch {
  fi = /\n+(?!\n|$)/g;
}
function Xs({ comment: e, type: t, value: s }, n, i, r) {
  const { blockQuote: o, commentString: l, lineWidth: c } = n.options;
  if (!o || /\n[\t ]+$/.test(s))
    return Rt(s, n);
  const a = n.indent || (n.forceBlockIndent || vn(s) ? "  " : ""), f = o === "literal" ? !0 : o === "folded" || t === K.BLOCK_FOLDED ? !1 : t === K.BLOCK_LITERAL ? !0 : !vf(s, c, a.length);
  if (!s)
    return f ? `|
` : `>
`;
  let u, g;
  for (g = s.length; g > 0; --g) {
    const $ = s[g - 1];
    if ($ !== `
` && $ !== "	" && $ !== " ")
      break;
  }
  let p = s.substring(g);
  const v = p.indexOf(`
`);
  v === -1 ? u = "-" : s === p || v !== p.length - 1 ? (u = "+", r && r()) : u = "", p && (s = s.slice(0, -p.length), p[p.length - 1] === `
` && (p = p.slice(0, -1)), p = p.replace(fi, `$&${a}`));
  let m = !1, y, b = -1;
  for (y = 0; y < s.length; ++y) {
    const $ = s[y];
    if ($ === " ")
      m = !0;
    else if ($ === `
`)
      b = y;
    else
      break;
  }
  let w = s.substring(0, b < y ? b + 1 : y);
  w && (s = s.substring(w.length), w = w.replace(/\n+/g, `$&${a}`));
  let _ = (m ? a ? "2" : "1" : "") + u;
  if (e && (_ += " " + l(e.replace(/ ?[\r\n]+/g, " ")), i && i()), !f) {
    const $ = s.replace(/\n+/g, `
$&`).replace(/(?:^|\n)([\t ].*)(?:([\n\t ]*)\n(?![\n\t ]))?/g, "$1$2").replace(/\n+/g, `$&${a}`);
    let x = !1;
    const D = kn(n, !0);
    o !== "folded" && t !== K.BLOCK_FOLDED && (D.onOverflow = () => {
      x = !0;
    });
    const P = _n(`${w}${$}${p}`, a, ci, D);
    if (!x)
      return `>${_}
${a}${P}`;
  }
  return s = s.replace(/\n+/g, `$&${a}`), `|${_}
${a}${w}${s}${p}`;
}
function Of(e, t, s, n) {
  const { type: i, value: r } = e, { actualString: o, implicitKey: l, indent: c, indentStep: a, inFlow: f } = t;
  if (l && r.includes(`
`) || f && /[[\]{},]/.test(r))
    return Rt(r, t);
  if (/^[\n\t ,[\]{}#&*!|>'"%@`]|^[?-]$|^[?-][ \t]|[\n:][ \t]|[ \t]\n|[\n\t ]#|[\n\t :]$/.test(r))
    return l || f || !r.includes(`
`) ? Rt(r, t) : Xs(e, t, s, n);
  if (!l && !f && i !== K.PLAIN && r.includes(`
`))
    return Xs(e, t, s, n);
  if (vn(r)) {
    if (c === "")
      return t.forceBlockIndent = !0, Xs(e, t, s, n);
    if (l && c === a)
      return Rt(r, t);
  }
  const u = r.replace(/\n+/g, `$&
${c}`);
  if (o) {
    const g = (m) => {
      var y;
      return m.default && m.tag !== "tag:yaml.org,2002:str" && ((y = m.test) == null ? void 0 : y.test(u));
    }, { compat: p, tags: v } = t.doc.schema;
    if (v.some(g) || p != null && p.some(g))
      return Rt(r, t);
  }
  return l ? u : _n(u, c, rl, kn(t, !1));
}
function Pi(e, t, s, n) {
  const { implicitKey: i, inFlow: r } = t, o = typeof e.value == "string" ? e : Object.assign({}, e, { value: String(e.value) });
  let { type: l } = e;
  l !== K.QUOTE_DOUBLE && /[\x00-\x08\x0b-\x1f\x7f-\x9f\u{D800}-\u{DFFF}]/u.test(o.value) && (l = K.QUOTE_DOUBLE);
  const c = (f) => {
    switch (f) {
      case K.BLOCK_FOLDED:
      case K.BLOCK_LITERAL:
        return i || r ? Rt(o.value, t) : Xs(o, t, s, n);
      case K.QUOTE_DOUBLE:
        return ws(o.value, t);
      case K.QUOTE_SINGLE:
        return ai(o.value, t);
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
    commentString: kf,
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
  const l = typeof i.stringify == "function" ? i.stringify(r, t, s, n) : Z(r) ? Pi(r, t, s, n) : r.toString(t, s, n);
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
  let v = !1, m = !1, y = Wt(e, s, () => v = !0, () => m = !0);
  if (!p && !s.inFlow && y.length > 1024) {
    if (u)
      throw new Error("With simple keys, single line scalar must not span more than 1024 characters");
    p = !0;
  }
  if (s.inFlow) {
    if (r || t == null)
      return v && n && n(), y === "" ? "?" : p ? `? ${y}` : y;
  } else if (r && !u || t == null && p)
    return y = `? ${y}`, g && !v ? y += kt(y, s.indent, a(g)) : m && i && i(), y;
  v && (g = null), p ? (g && (y += kt(y, s.indent, a(g))), y = `? ${y}
${l}:`) : (y = `${y}:`, g && (y += kt(y, s.indent, a(g))));
  let b, w, I;
  ie(t) ? (b = !!t.spaceBefore, w = t.commentBefore, I = t.comment) : (b = !1, w = null, I = null, t && typeof t == "object" && (t = o.createNode(t))), s.implicitKey = !1, !p && !g && Z(t) && (s.indentAtStart = y.length + 1), m = !1, !f && c.length >= 2 && !s.inFlow && !p && Ps(t) && !t.flow && !t.tag && !t.anchor && (s.indent = s.indent.substring(2));
  let _ = !1;
  const $ = Wt(t, s, () => _ = !0, () => m = !0);
  let x = " ";
  if (g || b || w) {
    if (x = b ? `
` : "", w) {
      const D = a(w);
      x += `
${rt(D, s.indent)}`;
    }
    $ === "" && !s.inFlow ? x === `
` && I && (x = `

`) : x += `
${s.indent}`;
  } else if (!p && ne(t)) {
    const D = $[0], P = $.indexOf(`
`), q = P !== -1, ee = s.inFlow ?? t.flow ?? t.items.length === 0;
    if (q || !ee) {
      let pe = !1;
      if (q && (D === "&" || D === "!")) {
        let fe = $.indexOf(" ");
        D === "&" && fe !== -1 && fe < P && $[fe + 1] === "!" && (fe = $.indexOf(" ", fe + 1)), (fe === -1 || P < fe) && (pe = !0);
      }
      pe || (x = `
${s.indent}`);
    }
  } else ($ === "" || $[0] === `
`) && (x = "");
  return y += x + $, s.inFlow ? _ && n && n() : I && !_ ? y += kt(y, s.indent, a(I)) : m && i && i(), y;
}
function ll(e, t) {
  (e === "debug" || e === "warn") && console.warn(t);
}
const xs = "<<", ct = {
  identify: (e) => e === xs || typeof e == "symbol" && e.description === xs,
  default: "key",
  tag: "tag:yaml.org,2002:merge",
  test: /^<<$/,
  resolve: () => Object.assign(new K(Symbol(xs)), {
    addToJSMap: cl
  }),
  stringify: () => xs
}, Ef = (e, t) => (ct.identify(t) || Z(t) && (!t.type || t.type === K.PLAIN) && ct.identify(t.value)) && (e == null ? void 0 : e.doc.schema.tags.some((s) => s.tag === ct.tag && s.default));
function cl(e, t, s) {
  const n = al(e, s);
  if (Ps(n))
    for (const i of n.items)
      qn(e, t, i);
  else if (Array.isArray(n))
    for (const i of n)
      qn(e, t, i);
  else
    qn(e, t, n);
}
function qn(e, t, s) {
  const n = al(e, s);
  if (!$s(n))
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
    const i = Ce(s, "", e);
    if (t instanceof Map)
      t.set(i, Ce(n, i, e));
    else if (t instanceof Set)
      t.add(i);
    else {
      const r = Cf(s, i, e), o = Ce(n, r, e);
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
function Mi(e, t, s) {
  const n = Ns(e, void 0, s), i = Ns(t, void 0, s);
  return new Se(n, i);
}
class Se {
  constructor(t, s = null) {
    Object.defineProperty(this, Ie, { value: zo }), this.key = t, this.value = s;
  }
  clone(t) {
    let { key: s, value: n } = this;
    return ie(s) && (s = s.clone(t)), ie(n) && (n = n.clone(t)), new Se(s, n);
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
  for (let v = 0; v < t.length; ++v) {
    const m = t[v];
    let y = null;
    if (ie(m))
      !u && m.spaceBefore && g.push(""), ln(s, g, m.commentBefore, u), m.comment && (y = m.comment);
    else if (re(m)) {
      const w = ie(m.key) ? m.key : null;
      w && (!u && w.spaceBefore && g.push(""), ln(s, g, w.commentBefore, u));
    }
    u = !1;
    let b = Wt(m, f, () => y = null, () => u = !0);
    y && (b += kt(b, r, a(y))), u && y && (u = !1), g.push(n + b);
  }
  let p;
  if (g.length === 0)
    p = i.start + i.end;
  else {
    p = g[0];
    for (let v = 1; v < g.length; ++v) {
      const m = g[v];
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
  for (let v = 0; v < e.length; ++v) {
    const m = e[v];
    let y = null;
    if (ie(m))
      m.spaceBefore && u.push(""), ln(t, u, m.commentBefore, !1), m.comment && (y = m.comment);
    else if (re(m)) {
      const w = ie(m.key) ? m.key : null;
      w && (w.spaceBefore && u.push(""), ln(t, u, w.commentBefore, !1), w.comment && (a = !0));
      const I = ie(m.value) ? m.value : null;
      I ? (I.comment && (y = I.comment), I.commentBefore && (a = !0)) : m.value == null && (w != null && w.comment) && (y = w.comment);
    }
    y && (a = !0);
    let b = Wt(m, c, () => y = null);
    a || (a = u.length > f || b.includes(`
`)), v < e.length - 1 ? b += "," : t.options.trailingComma && (t.options.lineWidth > 0 && (a || (a = u.reduce((w, I) => w + I.length + 2, 2) + (b.length + 2) > t.options.lineWidth)), a && (b += ",")), y && (b += kt(b, n, l(y))), u.push(b), f = u.length;
  }
  const { start: g, end: p } = s;
  if (u.length === 0)
    return g + p;
  if (!a) {
    const v = u.reduce((m, y) => m + y.length + 2, 2);
    a = t.options.lineWidth > 0 && v > t.options.lineWidth;
  }
  if (a) {
    let v = g;
    for (const m of u)
      v += m ? `
${r}${i}${m}` : `
`;
    return `${v}
${i}${p}`;
  } else
    return `${g}${o}${u.join(" ")}${o}${p}`;
}
function ln({ indent: e, options: { commentString: t } }, s, n, i) {
  if (n && i && (n = n.replace(/^\n+/, "")), n) {
    const r = rt(t(n), e);
    s.push(r.trimStart());
  }
}
function vt(e, t) {
  const s = Z(t) ? t.value : t;
  for (const n of e)
    if (re(n) && (n.key === t || n.key === s || Z(n.key) && n.key.value === s))
      return n;
}
class Ne extends il {
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
      (a !== void 0 || i) && o.items.push(Mi(c, a, n));
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
    const i = vt(this.items, n.key), r = (o = this.schema) == null ? void 0 : o.sortMapEntries;
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
    const s = vt(this.items, t);
    return s ? this.items.splice(this.items.indexOf(s), 1).length > 0 : !1;
  }
  get(t, s) {
    const n = vt(this.items, t), i = n == null ? void 0 : n.value;
    return (!s && Z(i) ? i.value : i) ?? void 0;
  }
  has(t) {
    return !!vt(this.items, t);
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
  nodeClass: Ne,
  tag: "tag:yaml.org,2002:map",
  resolve(e, t) {
    return $s(e) || t("Expected a mapping for this tag"), e;
  },
  createNode: (e, t, s) => Ne.from(e, t, s)
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
    Z(i) && nl(s) ? i.value = s : this.items[n] = s;
  }
  toJSON(t, s) {
    const n = [];
    s != null && s.onCreate && s.onCreate(n);
    let i = 0;
    for (const r of this.items)
      n.push(Ce(r, String(i++), s));
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
        r.items.push(Ns(l, void 0, n));
      }
    }
    return r;
  }
}
function Us(e) {
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
}, On = {
  identify: (e) => typeof e == "string",
  default: !0,
  tag: "tag:yaml.org,2002:str",
  resolve: (e) => e,
  stringify(e, t, s, n) {
    return t = Object.assign({ actualString: !0 }, t), Pi(e, t, s, n);
  }
}, Tn = {
  identify: (e) => e == null,
  createNode: () => new K(null),
  default: !0,
  tag: "tag:yaml.org,2002:null",
  test: /^(?:~|[Nn]ull|NULL)?$/,
  resolve: () => new K(null),
  stringify: ({ source: e }, t) => typeof e == "string" && Tn.test.test(e) ? e : t.options.nullStr
}, Di = {
  identify: (e) => typeof e == "boolean",
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:[Tt]rue|TRUE|[Ff]alse|FALSE)$/,
  resolve: (e) => new K(e[0] === "t" || e[0] === "T"),
  stringify({ source: e, value: t }, s) {
    if (e && Di.test.test(e)) {
      const n = e[0] === "t" || e[0] === "T";
      if (t === n)
        return e;
    }
    return t ? s.options.trueStr : s.options.falseStr;
  }
};
function Re({ format: e, minFractionDigits: t, tag: s, value: n }) {
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
  stringify: Re
}, dl = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "EXP",
  test: /^[-+]?(?:\.[0-9]+|[0-9]+(?:\.[0-9]*)?)[eE][-+]?[0-9]+$/,
  resolve: (e) => parseFloat(e),
  stringify(e) {
    const t = Number(e.value);
    return isFinite(t) ? t.toExponential() : Re(e);
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
  stringify: Re
}, Nn = (e) => typeof e == "bigint" || Number.isInteger(e), Ri = (e, t, s, { intAsBigInt: n }) => n ? BigInt(e) : parseInt(e.substring(t), s);
function gl(e, t, s) {
  const { value: n } = e;
  return Nn(n) && n >= 0 ? s + n.toString(t) : Re(e);
}
const ml = {
  identify: (e) => Nn(e) && e >= 0,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "OCT",
  test: /^0o[0-7]+$/,
  resolve: (e, t, s) => Ri(e, 2, 8, s),
  stringify: (e) => gl(e, 8, "0o")
}, yl = {
  identify: Nn,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  test: /^[-+]?[0-9]+$/,
  resolve: (e, t, s) => Ri(e, 0, 10, s),
  stringify: Re
}, bl = {
  identify: (e) => Nn(e) && e >= 0,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "HEX",
  test: /^0x[0-9a-fA-F]+$/,
  resolve: (e, t, s) => Ri(e, 2, 16, s),
  stringify: (e) => gl(e, 16, "0x")
}, $f = [
  Zt,
  es,
  On,
  Tn,
  Di,
  ml,
  yl,
  bl,
  hl,
  dl,
  pl
];
function Mr(e) {
  return typeof e == "bigint" || Number.isInteger(e);
}
const Vs = ({ value: e }) => JSON.stringify(e), Pf = [
  {
    identify: (e) => typeof e == "string",
    default: !0,
    tag: "tag:yaml.org,2002:str",
    resolve: (e) => e,
    stringify: Vs
  },
  {
    identify: (e) => e == null,
    createNode: () => new K(null),
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
    identify: Mr,
    default: !0,
    tag: "tag:yaml.org,2002:int",
    test: /^-?(?:0|[1-9][0-9]*)$/,
    resolve: (e, t, { intAsBigInt: s }) => s ? BigInt(e) : parseInt(e, 10),
    stringify: ({ value: e }) => Mr(e) ? e.toString() : JSON.stringify(e)
  },
  {
    identify: (e) => typeof e == "number",
    default: !0,
    tag: "tag:yaml.org,2002:float",
    test: /^-?(?:0|[1-9][0-9]*)(?:\.[0-9]*)?(?:[eE][-+]?[0-9]+)?$/,
    resolve: (e) => parseFloat(e),
    stringify: Vs
  }
], Mf = {
  default: !0,
  tag: "",
  test: /^/,
  resolve(e, t) {
    return t(`Unresolved plain scalar ${JSON.stringify(e)}`), e;
  }
}, Df = [Zt, es].concat(Pf, Mf), ji = {
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
    return Pi({ comment: e, type: t, value: l }, n, i, r);
  }
};
function wl(e, t) {
  if (Ps(e))
    for (let s = 0; s < e.items.length; ++s) {
      let n = e.items[s];
      if (!re(n)) {
        if ($s(n)) {
          n.items.length > 1 && t("Each pair must have its own sequence indicator");
          const i = n.items[0] || new Se(new K(null));
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
      i.items.push(Mi(l, c, s));
    }
  return i;
}
const Fi = {
  collection: "seq",
  default: !1,
  tag: "tag:yaml.org,2002:pairs",
  resolve: wl,
  createNode: Sl
};
class xt extends Nt {
  constructor() {
    super(), this.add = Ne.prototype.add.bind(this), this.delete = Ne.prototype.delete.bind(this), this.get = Ne.prototype.get.bind(this), this.has = Ne.prototype.has.bind(this), this.set = Ne.prototype.set.bind(this), this.tag = xt.tag;
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
      if (re(i) ? (r = Ce(i.key, "", s), o = Ce(i.value, r, s)) : r = Ce(i, "", s), n.has(r))
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
const Bi = {
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
  return t && (e ? kl : vl).test.test(t) ? t : e ? s.options.trueStr : s.options.falseStr;
}
const kl = {
  identify: (e) => e === !0,
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:Y|y|[Yy]es|YES|[Tt]rue|TRUE|[Oo]n|ON)$/,
  resolve: () => new K(!0),
  stringify: _l
}, vl = {
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
  stringify: Re
}, jf = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "EXP",
  test: /^[-+]?(?:[0-9][0-9_]*)?(?:\.[0-9_]*)?[eE][-+]?[0-9]+$/,
  resolve: (e) => parseFloat(e.replace(/_/g, "")),
  stringify(e) {
    const t = Number(e.value);
    return isFinite(t) ? t.toExponential() : Re(e);
  }
}, Ff = {
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
  stringify: Re
}, Ms = (e) => typeof e == "bigint" || Number.isInteger(e);
function An(e, t, s, { intAsBigInt: n }) {
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
function Ki(e, t, s) {
  const { value: n } = e;
  if (Ms(n)) {
    const i = n.toString(t);
    return n < 0 ? "-" + s + i.substr(1) : s + i;
  }
  return Re(e);
}
const Bf = {
  identify: Ms,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "BIN",
  test: /^[-+]?0b[0-1_]+$/,
  resolve: (e, t, s) => An(e, 2, 2, s),
  stringify: (e) => Ki(e, 2, "0b")
}, Kf = {
  identify: Ms,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "OCT",
  test: /^[-+]?0[0-7_]+$/,
  resolve: (e, t, s) => An(e, 1, 8, s),
  stringify: (e) => Ki(e, 8, "0")
}, xf = {
  identify: Ms,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  test: /^[-+]?[0-9][0-9_]*$/,
  resolve: (e, t, s) => An(e, 0, 10, s),
  stringify: Re
}, Uf = {
  identify: Ms,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "HEX",
  test: /^[-+]?0x[0-9a-fA-F_]+$/,
  resolve: (e, t, s) => An(e, 2, 16, s),
  stringify: (e) => Ki(e, 16, "0x")
};
class Ut extends Ne {
  constructor(t) {
    super(t), this.tag = Ut.tag;
  }
  add(t) {
    let s;
    re(t) ? s = t : t && typeof t == "object" && "key" in t && "value" in t && t.value === null ? s = new Se(t.key, null) : s = new Se(t, null), vt(this.items, s.key) || this.items.push(s);
  }
  /**
   * If `keepPair` is `true`, returns the Pair matching `key`.
   * Otherwise, returns the value of that Pair's key.
   */
  get(t, s) {
    const n = vt(this.items, t);
    return !s && re(n) ? Z(n.key) ? n.key.value : n.key : n;
  }
  set(t, s) {
    if (typeof s != "boolean")
      throw new Error(`Expected boolean value for set(key, value) in a YAML set, not ${typeof s}`);
    const n = vt(this.items, t);
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
        typeof i == "function" && (o = i.call(s, o, o)), r.items.push(Mi(o, null, n));
    return r;
  }
}
Ut.tag = "tag:yaml.org,2002:set";
const xi = {
  collection: "map",
  identify: (e) => e instanceof Set,
  nodeClass: Ut,
  default: !1,
  tag: "tag:yaml.org,2002:set",
  createNode: (e, t, s) => Ut.from(e, t, s),
  resolve(e, t) {
    if ($s(e)) {
      if (e.hasAllNullValues(!0))
        return Object.assign(new Ut(), e);
      t("Set items must all have null values");
    } else
      t("Expected a mapping for this tag");
    return e;
  }
};
function Ui(e, t) {
  const s = e[0], n = s === "-" || s === "+" ? e.substring(1) : e, i = (o) => t ? BigInt(o) : Number(o), r = n.replace(/_/g, "").split(":").reduce((o, l) => o * i(60) + i(l), i(0));
  return s === "-" ? i(-1) * r : r;
}
function Ol(e) {
  let { value: t } = e, s = (o) => o;
  if (typeof t == "bigint")
    s = (o) => BigInt(o);
  else if (isNaN(t) || !isFinite(t))
    return Re(e);
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
  resolve: (e, t, { intAsBigInt: s }) => Ui(e, s),
  stringify: Ol
}, Nl = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "TIME",
  test: /^[-+]?[0-9][0-9_]*(?::[0-5]?[0-9])+\.[0-9_]*$/,
  resolve: (e) => Ui(e, !1),
  stringify: Ol
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
      let u = Ui(f, !1);
      Math.abs(u) < 30 && (u *= 60), a -= 6e4 * u;
    }
    return new Date(a);
  },
  stringify: ({ value: e }) => (e == null ? void 0 : e.toISOString().replace(/(T00:00:00)?\.000Z$/, "")) ?? ""
}, Dr = [
  Zt,
  es,
  On,
  Tn,
  kl,
  vl,
  Bf,
  Kf,
  xf,
  Uf,
  Rf,
  jf,
  Ff,
  ji,
  ct,
  Bi,
  Fi,
  xi,
  Tl,
  Nl,
  En
], Rr = /* @__PURE__ */ new Map([
  ["core", $f],
  ["failsafe", [Zt, es, On]],
  ["json", Df],
  ["yaml11", Dr],
  ["yaml-1.1", Dr]
]), jr = {
  binary: ji,
  bool: Di,
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
  null: Tn,
  omap: Bi,
  pairs: Fi,
  seq: es,
  set: xi,
  timestamp: En
}, Vf = {
  "tag:yaml.org,2002:binary": ji,
  "tag:yaml.org,2002:merge": ct,
  "tag:yaml.org,2002:omap": Bi,
  "tag:yaml.org,2002:pairs": Fi,
  "tag:yaml.org,2002:set": xi,
  "tag:yaml.org,2002:timestamp": En
};
function Hn(e, t, s) {
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
class Vi {
  constructor({ compat: t, customTags: s, merge: n, resolveKnownTags: i, schema: r, sortMapEntries: o, toStringDefaults: l }) {
    this.compat = Array.isArray(t) ? Hn(t, "compat") : t ? Hn(null, t) : null, this.name = typeof r == "string" && r || "core", this.knownTags = i ? Vf : {}, this.tags = Hn(s, this.name, n), this.toStringOptions = l ?? null, Object.defineProperty(this, pt, { value: Zt }), Object.defineProperty(this, ze, { value: On }), Object.defineProperty(this, Qt, { value: es }), this.sortMapEntries = typeof o == "function" ? o : o === !0 ? qf : null;
  }
  clone() {
    const t = Object.create(Vi.prototype, Object.getOwnPropertyDescriptors(this));
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
    l && (f += kt(f, "", r(l))), (f[0] === "|" || f[0] === ">") && s[s.length - 1] === "---" ? s[s.length - 1] = `--- ${f}` : s.push(f);
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
let qi = class Al {
  constructor(t, s, n) {
    this.commentBefore = null, this.comment = null, this.errors = [], this.warnings = [], Object.defineProperty(this, Ie, { value: li });
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
    const t = Object.create(Al.prototype, {
      [Ie]: { value: li }
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
    return new $i(t.anchor);
  }
  createNode(t, s, n) {
    let i;
    if (typeof s == "function")
      t = s.call({ "": t }, "", t), i = s;
    else if (Array.isArray(s)) {
      const y = (w) => typeof w == "number" || w instanceof String || w instanceof Number, b = s.filter(y).map(String);
      b.length > 0 && (s = s.concat(b)), i = s;
    } else n === void 0 && s && (n = s, s = void 0);
    const { aliasDuplicateObjects: r, anchorPrefix: o, flow: l, keepUndefined: c, onTagObj: a, tag: f } = n ?? {}, { onAnchor: u, setAnchors: g, sourceObjects: p } = wf(
      this,
      // eslint-disable-next-line @typescript-eslint/prefer-nullish-coalescing
      o || "a"
    ), v = {
      aliasDuplicateObjects: r ?? !0,
      keepUndefined: c ?? !1,
      onAnchor: u,
      onTagObj: a,
      replacer: i,
      schema: this.schema,
      sourceObjects: p
    }, m = Ns(t, f, v);
    return l && ne(m) && (m.flow = !0), g(), m;
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
    return as(t) ? this.contents == null ? !1 : (this.contents = null, !0) : It(this.contents) ? this.contents.deleteIn(t) : !1;
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
    return as(t) ? !s && Z(this.contents) ? this.contents.value : this.contents : ne(this.contents) ? this.contents.getIn(t, s) : void 0;
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
    return as(t) ? this.contents !== void 0 : ne(this.contents) ? this.contents.hasIn(t) : !1;
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
    as(t) ? this.contents = s : this.contents == null ? this.contents = on(this.schema, Array.from(t), s) : It(this.contents) && this.contents.setIn(t, s);
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
      this.schema = new Vi(Object.assign(n, s));
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
    }, c = Ce(this.contents, s ?? "", l);
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
class fs extends El {
  constructor(t, s, n) {
    super("YAMLParseError", t, s, n);
  }
}
class Wf extends El {
  constructor(t, s, n) {
    super("YAMLWarning", t, s, n);
  }
}
const Fr = (e, t) => (s) => {
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
  let c = !1, a = l, f = l, u = "", g = "", p = !1, v = !1, m = null, y = null, b = null, w = null, I = null, _ = null, $ = null;
  for (const P of e)
    switch (v && (P.type !== "space" && P.type !== "newline" && P.type !== "comma" && r(P.offset, "MISSING_CHAR", "Tags and anchors must be separated from the next token by white space"), v = !1), m && (a && P.type !== "comment" && P.type !== "newline" && r(m, "TAB_AS_INDENT", "Tabs are not allowed as indentation"), m = null), P.type) {
      case "space":
        !t && (s !== "doc-start" || (n == null ? void 0 : n.type) !== "flow-collection") && P.source.includes("	") && (m = P), f = !0;
        break;
      case "comment": {
        f || r(P, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters");
        const q = P.source.substring(1) || " ";
        u ? u += g + q : u = q, g = "", a = !1;
        break;
      }
      case "newline":
        a ? u ? u += P.source : (!_ || s !== "seq-item-ind") && (c = !0) : g += P.source, a = !0, p = !0, (y || b) && (w = P), f = !0;
        break;
      case "anchor":
        y && r(P, "MULTIPLE_ANCHORS", "A node can have at most one anchor"), P.source.endsWith(":") && r(P.offset + P.source.length - 1, "BAD_ALIAS", "Anchor ending in : is ambiguous", !0), y = P, $ ?? ($ = P.offset), a = !1, f = !1, v = !0;
        break;
      case "tag": {
        b && r(P, "MULTIPLE_TAGS", "A node can have at most one tag"), b = P, $ ?? ($ = P.offset), a = !1, f = !1, v = !0;
        break;
      }
      case s:
        (y || b) && r(P, "BAD_PROP_ORDER", `Anchors and tags must be after the ${P.source} indicator`), _ && r(P, "UNEXPECTED_TOKEN", `Unexpected ${P.source} in ${t ?? "collection"}`), _ = P, a = s === "seq-item-ind" || s === "explicit-key-ind", f = !1;
        break;
      case "comma":
        if (t) {
          I && r(P, "UNEXPECTED_TOKEN", `Unexpected , in ${t}`), I = P, a = !1, f = !1;
          break;
        }
      // else fallthrough
      default:
        r(P, "UNEXPECTED_TOKEN", `Unexpected ${P.type} token`), a = !1, f = !1;
    }
  const x = e[e.length - 1], D = x ? x.offset + x.source.length : i;
  return v && n && n.type !== "space" && n.type !== "newline" && n.type !== "comma" && (n.type !== "scalar" || n.source !== "") && r(n.offset, "MISSING_CHAR", "Tags and anchors must be separated from the next token by white space"), m && (a && m.indent <= o || (n == null ? void 0 : n.type) === "block-map" || (n == null ? void 0 : n.type) === "block-seq") && r(m, "TAB_AS_INDENT", "Tabs are not allowed as indentation"), {
    comma: I,
    found: _,
    spaceBefore: c,
    comment: u,
    hasNewline: p,
    anchor: y,
    tag: b,
    newlineAfterProp: w,
    end: D,
    start: $ ?? D
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
function ui(e, t, s) {
  if ((t == null ? void 0 : t.type) === "flow-collection") {
    const n = t.end[0];
    n.indent === e && (n.source === "]" || n.source === "}") && As(t) && s(n, "BAD_INDENT", "Flow end indicator should be more indented than parent", !0);
  }
}
function Cl(e, t, s) {
  const { uniqueKeys: n } = e.options;
  if (n === !1)
    return !1;
  const i = typeof n == "function" ? n : (r, o) => r === o || Z(r) && Z(o) && r.value === o.value;
  return t.some((r) => i(r.key, s));
}
const Br = "All mapping items must start at the same column";
function Jf({ composeNode: e, composeEmptyNode: t }, s, n, i, r) {
  var f;
  const o = (r == null ? void 0 : r.nodeClass) ?? Ne, l = new o(s.schema);
  s.atRoot && (s.atRoot = !1);
  let c = n.offset, a = null;
  for (const u of n.items) {
    const { start: g, key: p, sep: v, value: m } = u, y = Jt(g, {
      indicator: "explicit-key-ind",
      next: p ?? (v == null ? void 0 : v[0]),
      offset: c,
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !0
    }), b = !y.found;
    if (b) {
      if (p && (p.type === "block-seq" ? i(c, "BLOCK_AS_IMPLICIT_KEY", "A block sequence may not be used as an implicit map key") : "indent" in p && p.indent !== n.indent && i(c, "BAD_INDENT", Br)), !y.anchor && !y.tag && !v) {
        a = y.end, y.comment && (l.comment ? l.comment += `
` + y.comment : l.comment = y.comment);
        continue;
      }
      (y.newlineAfterProp || As(p)) && i(p ?? g[g.length - 1], "MULTILINE_IMPLICIT_KEY", "Implicit keys need to be on a single line");
    } else ((f = y.found) == null ? void 0 : f.indent) !== n.indent && i(c, "BAD_INDENT", Br);
    s.atKey = !0;
    const w = y.end, I = p ? e(s, p, y, i) : t(s, w, g, null, y, i);
    s.schema.compat && ui(n.indent, p, i), s.atKey = !1, Cl(s, l.items, I) && i(w, "DUPLICATE_KEY", "Map keys must be unique");
    const _ = Jt(v ?? [], {
      indicator: "map-value-ind",
      next: m,
      offset: I.range[2],
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !p || p.type === "block-scalar"
    });
    if (c = _.end, _.found) {
      b && ((m == null ? void 0 : m.type) === "block-map" && !_.hasNewline && i(c, "BLOCK_AS_IMPLICIT_KEY", "Nested mappings are not allowed in compact mappings"), s.options.strict && y.start < _.found.offset - 1024 && i(I.range, "KEY_OVER_1024_CHARS", "The : indicator must be at most 1024 chars after the start of an implicit block mapping key"));
      const $ = m ? e(s, m, _, i) : t(s, c, v, null, _, i);
      s.schema.compat && ui(n.indent, m, i), c = $.range[2];
      const x = new Se(I, $);
      s.options.keepSourceTokens && (x.srcToken = u), l.items.push(x);
    } else {
      b && i(I.range, "MISSING_CHAR", "Implicit map keys need to be followed by map values"), _.comment && (I.comment ? I.comment += `
` + _.comment : I.comment = _.comment);
      const $ = new Se(I);
      s.options.keepSourceTokens && ($.srcToken = u), l.items.push($);
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
    s.schema.compat && ui(n.indent, u, i), c = p.range[2], l.items.push(p);
  }
  return l.range = [n.offset, c, a ?? c], l;
}
function Ds(e, t, s, n) {
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
const Wn = "Block collections are not allowed within flow collections", Jn = (e) => e && (e.type === "block-map" || e.type === "block-seq");
function Gf({ composeNode: e, composeEmptyNode: t }, s, n, i, r) {
  var y;
  const o = n.start.source === "{", l = o ? "flow map" : "flow sequence", c = (r == null ? void 0 : r.nodeClass) ?? (o ? Ne : Nt), a = new c(s.schema);
  a.flow = !0;
  const f = s.atRoot;
  f && (s.atRoot = !1), s.atKey && (s.atKey = !1);
  let u = n.offset + n.start.source.length;
  for (let b = 0; b < n.items.length; ++b) {
    const w = n.items[b], { start: I, key: _, sep: $, value: x } = w, D = Jt(I, {
      flow: l,
      indicator: "explicit-key-ind",
      next: _ ?? ($ == null ? void 0 : $[0]),
      offset: u,
      onError: i,
      parentIndent: n.indent,
      startOnNewline: !1
    });
    if (!D.found) {
      if (!D.anchor && !D.tag && !$ && !x) {
        b === 0 && D.comma ? i(D.comma, "UNEXPECTED_TOKEN", `Unexpected , in ${l}`) : b < n.items.length - 1 && i(D.start, "UNEXPECTED_TOKEN", `Unexpected empty item in ${l}`), D.comment && (a.comment ? a.comment += `
` + D.comment : a.comment = D.comment), u = D.end;
        continue;
      }
      !o && s.options.strict && As(_) && i(
        _,
        // checked by containsNewline()
        "MULTILINE_IMPLICIT_KEY",
        "Implicit keys of flow sequence pairs need to be on a single line"
      );
    }
    if (b === 0)
      D.comma && i(D.comma, "UNEXPECTED_TOKEN", `Unexpected , in ${l}`);
    else if (D.comma || i(D.start, "MISSING_CHAR", `Missing , between ${l} items`), D.comment) {
      let P = "";
      e: for (const q of I)
        switch (q.type) {
          case "comma":
          case "space":
            break;
          case "comment":
            P = q.source.substring(1);
            break e;
          default:
            break e;
        }
      if (P) {
        let q = a.items[a.items.length - 1];
        re(q) && (q = q.value ?? q.key), q.comment ? q.comment += `
` + P : q.comment = P, D.comment = D.comment.substring(P.length + 1);
      }
    }
    if (!o && !$ && !D.found) {
      const P = x ? e(s, x, D, i) : t(s, D.end, $, null, D, i);
      a.items.push(P), u = P.range[2], Jn(x) && i(P.range, "BLOCK_IN_FLOW", Wn);
    } else {
      s.atKey = !0;
      const P = D.end, q = _ ? e(s, _, D, i) : t(s, P, I, null, D, i);
      Jn(_) && i(q.range, "BLOCK_IN_FLOW", Wn), s.atKey = !1;
      const ee = Jt($ ?? [], {
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
          if ($)
            for (const ue of $) {
              if (ue === ee.found)
                break;
              if (ue.type === "newline") {
                i(ue, "MULTILINE_IMPLICIT_KEY", "Implicit keys of flow sequence pairs need to be on a single line");
                break;
              }
            }
          D.start < ee.found.offset - 1024 && i(ee.found, "KEY_OVER_1024_CHARS", "The : indicator must be at most 1024 chars after the start of an implicit flow sequence key");
        }
      } else x && ("source" in x && ((y = x.source) == null ? void 0 : y[0]) === ":" ? i(x, "MISSING_CHAR", `Missing space after : in ${l}`) : i(ee.start, "MISSING_CHAR", `Missing , or : between ${l} items`));
      const pe = x ? e(s, x, ee, i) : ee.found ? t(s, ee.end, $, null, ee, i) : null;
      pe ? Jn(x) && i(pe.range, "BLOCK_IN_FLOW", Wn) : ee.comment && (q.comment ? q.comment += `
` + ee.comment : q.comment = ee.comment);
      const fe = new Se(q, pe);
      if (s.options.keepSourceTokens && (fe.srcToken = w), o) {
        const ue = a;
        Cl(s, ue.items, q) && i(P, "DUPLICATE_KEY", "Map keys must be unique"), ue.items.push(fe);
      } else {
        const ue = new Ne(s.schema);
        ue.flow = !0, ue.items.push(fe);
        const Et = (pe ?? q).range;
        ue.range = [q.range[0], Et[1], Et[2]], a.items.push(ue);
      }
      u = pe ? pe.range[2] : ee.end;
    }
  }
  const g = o ? "}" : "]", [p, ...v] = n.end;
  let m = u;
  if ((p == null ? void 0 : p.source) === g)
    m = p.offset + p.source.length;
  else {
    const b = l[0].toUpperCase() + l.substring(1), w = f ? `${b} must end with a ${g}` : `${b} in block collection must be sufficiently indented and end with a ${g}`;
    i(u, f ? "MISSING_CHAR" : "BAD_INDENT", w), p && p.source.length !== 1 && v.unshift(p);
  }
  if (v.length > 0) {
    const b = Ds(v, m, s.options.strict, i);
    b.comment && (a.comment ? a.comment += `
` + b.comment : a.comment = b.comment), a.range = [n.offset, m, b.offset];
  } else
    a.range = [n.offset, m, m];
  return a;
}
function Yn(e, t, s, n, i, r) {
  const o = s.type === "block-map" ? Jf(e, t, s, n, r) : s.type === "block-seq" ? Yf(e, t, s, n, r) : Gf(e, t, s, n, r), l = o.constructor;
  return i === "!" || i === l.tagName ? (o.tag = l.tagName, o) : (i && (o.tag = i), o);
}
function Qf(e, t, s, n, i) {
  var g;
  const r = n.tag, o = r ? t.directives.tagName(r.source, (p) => i(r, "TAG_RESOLVE_FAILED", p)) : null;
  if (s.type === "block-seq") {
    const { anchor: p, newlineAfterProp: v } = n, m = p && r ? p.offset > r.offset ? p : r : p ?? r;
    m && (!v || v.offset < m.offset) && i(m, "MISSING_CHAR", "Missing newline after block sequence props");
  }
  const l = s.type === "block-map" ? "map" : s.type === "block-seq" ? "seq" : s.start.source === "{" ? "map" : "seq";
  if (!r || !o || o === "!" || o === Ne.tagName && l === "map" || o === Nt.tagName && l === "seq")
    return Yn(e, t, s, i, o);
  let c = t.schema.tags.find((p) => p.tag === o && p.collection === l);
  if (!c) {
    const p = t.schema.knownTags[o];
    if ((p == null ? void 0 : p.collection) === l)
      t.schema.tags.push(Object.assign({}, p, { default: !1 })), c = p;
    else
      return p ? i(r, "BAD_COLLECTION_TYPE", `${p.tag} used for ${l} collection, but expects ${p.collection ?? "scalar"}`, !0) : i(r, "TAG_RESOLVE_FAILED", `Unresolved tag: ${o}`, !0), Yn(e, t, s, i, o);
  }
  const a = Yn(e, t, s, i, o, c), f = ((g = c.resolve) == null ? void 0 : g.call(c, a, (p) => i(r, "TAG_RESOLVE_FAILED", p), t.options)) ?? a, u = ie(f) ? f : new K(f);
  return u.range = a.range, u.tag = o, c != null && c.format && (u.format = c.format), u;
}
function Xf(e, t, s) {
  const n = t.offset, i = zf(t, e.options.strict, s);
  if (!i)
    return { value: "", type: null, comment: "", range: [n, n, n] };
  const r = i.mode === ">" ? K.BLOCK_FOLDED : K.BLOCK_LITERAL, o = t.source ? Zf(t.source) : [];
  let l = o.length;
  for (let m = o.length - 1; m >= 0; --m) {
    const y = o[m][1];
    if (y === "" || y === "\r")
      l = m;
    else
      break;
  }
  if (l === 0) {
    const m = i.chomp === "+" && o.length > 0 ? `
`.repeat(Math.max(1, o.length - 1)) : "";
    let y = n + i.length;
    return t.source && (y += t.source.length), { value: m, type: r, comment: i.comment, range: [n, y, y] };
  }
  let c = t.indent + i.indent, a = t.offset + i.length, f = 0;
  for (let m = 0; m < l; ++m) {
    const [y, b] = o[m];
    if (b === "" || b === "\r")
      i.indent === 0 && y.length > c && (c = y.length);
    else {
      y.length < c && s(a + y.length, "MISSING_CHAR", "Block scalars with more-indented leading empty lines must use an explicit indentation indicator"), i.indent === 0 && (c = y.length), f = m, c === 0 && !e.atRoot && s(a, "BAD_INDENT", "Block scalar values in collections must be indented");
      break;
    }
    a += y.length + b.length + 1;
  }
  for (let m = o.length - 1; m >= l; --m)
    o[m][0].length > c && (l = m + 1);
  let u = "", g = "", p = !1;
  for (let m = 0; m < f; ++m)
    u += o[m][0].slice(c) + `
`;
  for (let m = f; m < l; ++m) {
    let [y, b] = o[m];
    a += y.length + b.length + 1;
    const w = b[b.length - 1] === "\r";
    if (w && (b = b.slice(0, -1)), b && y.length < c) {
      const _ = `Block scalar lines must not be less indented than their ${i.indent ? "explicit indentation indicator" : "first line"}`;
      s(a - b.length - (w ? 2 : 1), "BAD_INDENT", _), y = "";
    }
    r === K.BLOCK_LITERAL ? (u += g + y.slice(c) + b, g = `
`) : y.length > c || b[0] === "	" ? (g === " " ? g = `
` : !p && g === `
` && (g = `

`), u += g + y.slice(c) + b, g = `
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
  const v = n + i.length + t.source.length;
  return { value: u, type: r, comment: i.comment, range: [n, v, v] };
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
      const v = Number(p);
      !o && v ? o = v : c === -1 && (c = e + g);
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
        const v = `Unexpected token in block scalar header: ${p.type}`;
        n(p, "UNEXPECTED_TOKEN", v);
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
  const a = (g, p, v) => s(n + g, p, v);
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
  const f = n + r.length, u = Ds(o, f, t, s);
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
  e.options.stringKeys && e.atKey ? a = e.schema[ze] : c ? a = lu(e.schema, i, c, s, n) : t.type === "scalar" ? a = cu(e, i, t, n) : a = e.schema[ze];
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
    return e[ze];
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
  return o && !o.collection ? (e.tags.push(Object.assign({}, o, { default: !1, test: void 0 })), o) : (i(n, "TAG_RESOLVE_FAILED", `Unresolved tag: ${s}`, s !== "tag:yaml.org,2002:str"), e[ze]);
}
function cu({ atKey: e, directives: t, schema: s }, n, i, r) {
  const o = s.tags.find((l) => {
    var c;
    return (l.default === !0 || e && l.default === "key") && ((c = l.test) == null ? void 0 : c.test(n));
  }) || s[ze];
  if (s.compat) {
    const l = s.compat.find((c) => {
      var a;
      return c.default && ((a = c.test) == null ? void 0 : a.test(n));
    }) ?? s[ze];
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
const fu = { composeNode: $l, composeEmptyNode: Hi };
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
  return a ?? (a = Hi(e, t.offset, void 0, null, s, n)), l && a.anchor === "" && n(l, "BAD_ALIAS", "Anchor cannot be an empty string"), i && e.options.stringKeys && (!Z(a) || typeof a.value != "string" || a.tag && a.tag !== "tag:yaml.org,2002:str") && n(c ?? t, "NON_STRING_KEY", "With stringKeys, all keys must be strings"), r && (a.spaceBefore = !0), o && (t.type === "scalar" && t.source === "" ? a.comment = o : a.commentBefore = o), e.options.keepSourceTokens && f && (a.srcToken = t), a;
}
function Hi(e, t, s, n, { spaceBefore: i, comment: r, anchor: o, tag: l, end: c }, a) {
  const f = {
    type: "scalar",
    offset: au(t, s, n),
    indent: -1,
    source: ""
  }, u = Ll(e, f, l, a);
  return o && (u.anchor = o.source.substring(1), u.anchor === "" && a(o, "BAD_ALIAS", "Anchor cannot be an empty string")), i && (u.spaceBefore = !0), r && (u.comment = r, u.range[2] = c), u;
}
function uu({ options: e }, { offset: t, source: s, end: n }, i) {
  const r = new $i(s.substring(1));
  r.source === "" && i(t, "BAD_ALIAS", "Alias cannot be an empty string"), r.source.endsWith(":") && i(t + s.length - 1, "BAD_ALIAS", "Alias ending in : is ambiguous", !0);
  const o = t + s.length, l = Ds(n, o, e.strict, i);
  return r.range = [t, o, l.offset], l.comment && (r.comment = l.comment), r;
}
function hu(e, t, { offset: s, start: n, value: i, end: r }, o) {
  const l = Object.assign({ _directives: t }, e), c = new qi(void 0, l), a = {
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
  f.found && (c.directives.docStart = !0, i && (i.type === "block-map" || i.type === "block-seq") && !f.hasNewline && o(f.end, "MISSING_CHAR", "Block collection cannot start on same line with directives-end marker")), c.contents = i ? $l(a, i, f, o) : Hi(a, f.end, n, null, f, o);
  const u = c.contents.range[2], g = Ds(r, u, !1, o);
  return g.comment && (c.comment = g.comment), c.range = [s, u, g.offset], c;
}
function os(e) {
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
      const o = os(s);
      r ? this.warnings.push(new Wf(o, n, i)) : this.errors.push(new fs(o, n, i));
    }, this.directives = new ye({ version: t.version || "1.2" }), this.options = t;
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
          const r = os(t);
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
        const s = t.source ? `${t.message}: ${JSON.stringify(t.source)}` : t.message, n = new fs(os(t), "UNEXPECTED_TOKEN", s);
        this.atDirectives || !this.doc ? this.errors.push(n) : this.doc.errors.push(n);
        break;
      }
      case "doc-end": {
        if (!this.doc) {
          const n = "Unexpected doc-end without preceding document";
          this.errors.push(new fs(os(t), "UNEXPECTED_TOKEN", n));
          break;
        }
        this.doc.directives.docEnd = !0;
        const s = Ds(t.end, t.offset + t.source.length, this.doc.options.strict, this.onError);
        if (this.decorate(this.doc, !0), s.comment) {
          const n = this.doc.comment;
          this.doc.comment = n ? `${n}
${s.comment}` : s.comment;
        }
        this.doc.range[2] = s.offset;
        break;
      }
      default:
        this.errors.push(new fs(os(t), "UNEXPECTED_TOKEN", `Unsupported token ${t.type}`));
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
      const n = Object.assign({ _directives: this.directives }, this.options), i = new qi(void 0, n);
      this.atDirectives && this.onError(s, "MISSING_CHAR", "Missing directives-end indicator line"), i.range = [0, s, s], this.decorate(i, !1), yield i;
    }
  }
}
const Pl = "\uFEFF", Ml = "", Dl = "", hi = "";
function pu(e) {
  switch (e) {
    case Pl:
      return "byte-order-mark";
    case Ml:
      return "doc-mode";
    case Dl:
      return "flow-error-end";
    case hi:
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
function Le(e) {
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
const xr = new Set("0123456789ABCDEFabcdef"), gu = new Set("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-#;/?:@&=+$_.!~*'()"), qs = new Set(",[]{}"), mu = new Set(` ,[]{}
\r	`), Gn = (e) => !e || mu.has(e);
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
      if ((n === "---" || n === "...") && Le(this.buffer[t + 3]))
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
    if (t[0] === Pl && (yield* this.pushCount(1), t = t.substring(1)), t[0] === "%") {
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
    return yield Ml, yield* this.parseLineStart();
  }
  *parseLineStart() {
    const t = this.charAt(0);
    if (!t && !this.atEnd)
      return this.setNext("line-start");
    if (t === "-" || t === ".") {
      if (!this.atEnd && !this.hasChars(4))
        return this.setNext("line-start");
      const s = this.peek(3);
      if ((s === "---" || s === "...") && Le(this.charAt(3)))
        return yield* this.pushCount(3), this.indentValue = 0, this.indentNext = 0, s === "---" ? "doc" : "stream";
    }
    return this.indentValue = yield* this.pushSpaces(!1), this.indentNext > this.indentValue && !Le(this.charAt(1)) && (this.indentNext = this.indentValue), yield* this.parseBlockStart();
  }
  *parseBlockStart() {
    const [t, s] = this.peek(2);
    if (!s && !this.atEnd)
      return this.setNext("block-start");
    if ((t === "-" || t === "?" || t === ":") && Le(s)) {
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
        return yield* this.pushUntil(Gn), "doc";
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
    if ((n !== -1 && n < this.indentNext && i[0] !== "#" || n === 0 && (i.startsWith("---") || i.startsWith("...")) && Le(i[3])) && !(n === this.indentNext - 1 && this.flowLevel === 1 && (i[0] === "]" || i[0] === "}")))
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
        return yield* this.pushUntil(Gn), "flow";
      case '"':
      case "'":
        return this.flowKey = !0, yield* this.parseQuotedScalar();
      case ":": {
        const o = this.charAt(1);
        if (this.flowKey || Le(o) || o === ",")
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
    return yield* this.pushUntil((s) => Le(s) || s === "#");
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
    return yield hi, yield* this.pushToIndex(t + 1, !0), yield* this.parseLineStart();
  }
  *parsePlainScalar() {
    const t = this.flowLevel > 0;
    let s = this.pos - 1, n = this.pos - 1, i;
    for (; i = this.buffer[++n]; )
      if (i === ":") {
        const r = this.buffer[n + 1];
        if (Le(r) || t && qs.has(r))
          break;
        s = n;
      } else if (Le(i)) {
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
    return !i && !this.atEnd ? this.setNext("plain-scalar") : (yield hi, yield* this.pushToIndex(s + 1, !0), t ? "flow" : "doc");
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
          t += yield* this.pushUntil(Gn), t += yield* this.pushSpaces(!0);
          continue e;
        case "-":
        // this is an error
        case "?":
        // this is an error outside flow collections
        case ":": {
          const s = this.flowLevel > 0, n = this.charAt(1);
          if (Le(n) || s && qs.has(n)) {
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
      for (; !Le(s) && s !== ">"; )
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
function Vr(e) {
  if (e.start.type === "flow-seq-start")
    for (const t of e.items)
      t.sep && !t.value && !ht(t.start, "explicit-key-ind") && !ht(t.sep, "map-value-ind") && (t.key && (t.value = t.key), delete t.key, Rl(t.value) ? t.value.end ? cn(t.value.end, t.sep) : t.value.end = t.sep : cn(t.start, t.sep), delete t.sep);
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
      o.errors.push(new fs(l.range.slice(0, 2), "MULTIPLE_DOCS", "Source contains multiple documents; please use YAML.parseAllDocuments()"));
      break;
    }
  return n && s && (o.errors.forEach(Fr(e, s)), o.warnings.forEach(Fr(e, s))), o;
}
function ku(e, t, s) {
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
function vu(e, t, s) {
  let n = null;
  if (Array.isArray(t) && (n = t), e === void 0) {
    const { keepUndefined: i } = {};
    if (!i)
      return;
  }
  return Ls(e) && !n ? e.toString(s) : new qi(e, n, s).toString(s);
}
const Ou = { class: "ssh-set" }, Tu = { class: "row" }, Nu = ["value"], Au = {
  key: 0,
  class: "muted empty"
}, Eu = { class: "row spread" }, Cu = { class: "row" }, Iu = ["onUpdate:modelValue"], Lu = ["onUpdate:modelValue"], $u = ["onUpdate:modelValue"], Pu = ["onClick"], Mu = { class: "row" }, Du = ["onUpdate:modelValue"], Ru = ["value"], ju = ["onClick"], Fu = {
  key: 0,
  class: "row new-cred"
}, Bu = ["onUpdate:modelValue"], Ku = ["onUpdate:modelValue"], xu = ["onUpdate:modelValue"], Uu = ["disabled", "onClick"], Vu = { class: "muted" }, qu = { class: "row" }, Hu = ["onUpdate:modelValue", "placeholder"], Wu = ["onUpdate:modelValue"], Ju = { class: "row" }, Yu = ["onUpdate:modelValue"], Gu = { class: "row" }, Qu = { class: "chk" }, Xu = ["onUpdate:modelValue"], zu = { class: "row" }, Zu = ["disabled", "onClick"], eh = { class: "muted" }, th = /* @__PURE__ */ Dc({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function i(y, b) {
      return {
        key: n++,
        name: y,
        host: b.host || "",
        port: b.port || 22,
        user: b.user || "",
        credential: b.credential || "",
        password: b.password || "",
        keyFile: b.key_file || "",
        keyPassphrase: b.key_passphrase || "",
        description: b.description || "",
        allowWrite: !!b.allow_write,
        newCred: !1,
        credName: "",
        credUser: "",
        credPassword: "",
        credStatus: "",
        testing: !1,
        testStatus: ""
      };
    }
    function r(y) {
      return {
        host: y.host || void 0,
        port: y.port === 22 ? void 0 : y.port,
        user: y.user || void 0,
        credential: y.credential || void 0,
        // Legacy single-value pointers stay untouched if they were in the blob and no credential is set.
        password: y.credential ? void 0 : y.password || void 0,
        key_file: y.keyFile || void 0,
        key_passphrase: y.credential ? void 0 : y.keyPassphrase || void 0,
        description: y.description || void 0,
        allow_write: y.allowWrite || void 0
      };
    }
    const o = (() => {
      try {
        return ku(s.api.getYaml() || "") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ Dn(o.default_host || ""), c = /* @__PURE__ */ Dn(o.timeout_seconds || 20), a = /* @__PURE__ */ gn(
      Object.entries(o.hosts || {}).map(([y, b]) => i(y, b))
    ), f = /* @__PURE__ */ Dn([]);
    async function u() {
      try {
        const y = await s.api.invoke("secret.list");
        f.value = [...new Set([...y.machine || [], ...y.project || []].map((b) => b.key))].sort();
      } catch {
      }
    }
    No(u);
    function g() {
      a.push(i(`host${a.length + 1}`, {}));
    }
    async function p(y) {
      y.credStatus = "Saving…";
      try {
        const b = { password: y.credPassword };
        y.credUser && (b.user = y.credUser), await s.api.invoke("secret.set", { key: y.credName.trim(), fields: b, scope: "machine" }), y.credential = y.credName.trim(), y.newCred = !1, y.credName = "", y.credUser = "", y.credPassword = "", y.credStatus = "", await u();
      } catch (b) {
        y.credStatus = "Failed: " + (b instanceof Error ? b.message : String(b));
      }
    }
    async function v(y) {
      y.testing = !0, y.testStatus = "Connecting…";
      try {
        const b = await s.api.invoke("plugin.action", {
          pluginId: "ssh",
          action: "testHost",
          valueJson: JSON.stringify({
            host: y.host,
            port: y.port,
            user: y.user || void 0,
            credential: y.credential || void 0,
            password: y.credential ? void 0 : y.password || void 0,
            keyFile: y.keyFile || void 0
          })
        });
        if (b.ok && b.resultJson) {
          const w = JSON.parse(b.resultJson);
          y.testStatus = w.message;
        } else
          y.testStatus = "Failed: " + (b.error || "unknown error");
      } catch (b) {
        y.testStatus = "Failed: " + (b instanceof Error ? b.message : String(b));
      } finally {
        y.testing = !1;
      }
    }
    function m() {
      const y = {
        default_host: l.value || void 0,
        timeout_seconds: c.value || 20,
        hosts: Object.fromEntries(
          a.filter((b) => b.name.trim()).map((b) => [b.name.trim(), r(b)])
        )
      };
      return vu(y);
    }
    return t({ toYaml: m }), (y, b) => (Ve(), et("div", Ou, [
      b[16] || (b[16] = j("div", { class: "muted" }, " Named SSH hosts available to the agent and the terminal. Passwords/keys live in the secret store (Settings → Secrets); a host only references an entry by name. ", -1)),
      j("div", Tu, [
        j("label", null, [
          b[3] || (b[3] = j("span", { class: "muted" }, "Default host", -1)),
          ve(j("select", {
            "onUpdate:modelValue": b[0] || (b[0] = (w) => l.value = w)
          }, [
            b[2] || (b[2] = j("option", { value: "" }, "(none)", -1)),
            (Ve(!0), et(Te, null, Fn(a, (w) => (Ve(), et("option", {
              key: w.key,
              value: w.name
            }, $t(w.name), 9, Nu))), 128))
          ], 512), [
            [Ir, l.value]
          ])
        ]),
        j("label", null, [
          b[4] || (b[4] = j("span", { class: "muted" }, "Timeout, s", -1)),
          ve(j("input", {
            "onUpdate:modelValue": b[1] || (b[1] = (w) => c.value = w),
            type: "number",
            min: "5",
            max: "120",
            class: "w-70"
          }, null, 512), [
            [
              Ue,
              c.value,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      j("button", {
        type: "button",
        class: "self-start",
        onClick: g
      }, "+ Add host"),
      a.length ? pr("", !0) : (Ve(), et("div", Au, 'No hosts yet. Click "+ Add host".')),
      (Ve(!0), et(Te, null, Fn(a, (w, I) => (Ve(), et("div", {
        key: w.key,
        class: "host-card"
      }, [
        j("div", Eu, [
          j("div", Cu, [
            b[5] || (b[5] = j("span", { class: "muted" }, "Name", -1)),
            ve(j("input", {
              "onUpdate:modelValue": (_) => w.name = _,
              class: "w-120",
              spellcheck: "false"
            }, null, 8, Iu), [
              [Ue, w.name]
            ]),
            b[6] || (b[6] = j("span", { class: "muted" }, "Host", -1)),
            ve(j("input", {
              "onUpdate:modelValue": (_) => w.host = _,
              placeholder: "10.0.0.5 or box.local",
              class: "w-180",
              spellcheck: "false"
            }, null, 8, Lu), [
              [Ue, w.host]
            ]),
            b[7] || (b[7] = j("span", { class: "muted" }, "Port", -1)),
            ve(j("input", {
              "onUpdate:modelValue": (_) => w.port = _,
              type: "number",
              min: "1",
              max: "65535",
              class: "w-70"
            }, null, 8, $u), [
              [
                Ue,
                w.port,
                void 0,
                { number: !0 }
              ]
            ])
          ]),
          j("button", {
            type: "button",
            onClick: (_) => a.splice(I, 1)
          }, "✕ Remove", 8, Pu)
        ]),
        j("div", Mu, [
          b[9] || (b[9] = j("span", { class: "muted w-label" }, "Credential", -1)),
          ve(j("select", {
            "onUpdate:modelValue": (_) => w.credential = _
          }, [
            b[8] || (b[8] = j("option", { value: "" }, "(none — use fields below)", -1)),
            (Ve(!0), et(Te, null, Fn(f.value, (_) => (Ve(), et("option", {
              key: _,
              value: _
            }, $t(_), 9, Ru))), 128))
          ], 8, Du), [
            [Ir, w.credential]
          ]),
          j("button", {
            type: "button",
            onClick: (_) => w.newCred = !w.newCred
          }, $t(w.newCred ? "cancel" : "new…"), 9, ju),
          b[10] || (b[10] = j("span", { class: "muted" }, "entry in the secret store: user + password or private_key", -1))
        ]),
        w.newCred ? (Ve(), et("div", Fu, [
          ve(j("input", {
            "onUpdate:modelValue": (_) => w.credName = _,
            placeholder: "entry name",
            class: "w-120",
            spellcheck: "false"
          }, null, 8, Bu), [
            [Ue, w.credName]
          ]),
          ve(j("input", {
            "onUpdate:modelValue": (_) => w.credUser = _,
            placeholder: "user",
            class: "w-120",
            spellcheck: "false"
          }, null, 8, Ku), [
            [Ue, w.credUser]
          ]),
          ve(j("input", {
            "onUpdate:modelValue": (_) => w.credPassword = _,
            type: "password",
            placeholder: "password",
            class: "w-140",
            autocomplete: "new-password"
          }, null, 8, xu), [
            [Ue, w.credPassword]
          ]),
          j("button", {
            type: "button",
            disabled: !w.credName || !w.credPassword,
            onClick: (_) => p(w)
          }, "Save to store", 8, Uu),
          j("span", Vu, $t(w.credStatus), 1)
        ])) : pr("", !0),
        j("div", qu, [
          b[11] || (b[11] = j("span", { class: "muted w-label" }, "User", -1)),
          ve(j("input", {
            "onUpdate:modelValue": (_) => w.user = _,
            placeholder: w.credential ? "(from credential)" : "login",
            class: "w-120",
            spellcheck: "false"
          }, null, 8, Hu), [
            [Ue, w.user]
          ]),
          b[12] || (b[12] = j("span", { class: "muted" }, "Key file", -1)),
          ve(j("input", {
            "onUpdate:modelValue": (_) => w.keyFile = _,
            placeholder: "optional: C:\\Users\\me\\.ssh\\id_ed25519",
            class: "w-260",
            spellcheck: "false"
          }, null, 8, Wu), [
            [Ue, w.keyFile]
          ])
        ]),
        j("div", Ju, [
          b[13] || (b[13] = j("span", { class: "muted w-label" }, "Description", -1)),
          ve(j("input", {
            "onUpdate:modelValue": (_) => w.description = _,
            placeholder: "Shown to the AI — what this host is",
            class: "grow"
          }, null, 8, Yu), [
            [Ue, w.description]
          ])
        ]),
        j("div", Gu, [
          j("label", Qu, [
            ve(j("input", {
              "onUpdate:modelValue": (_) => w.allowWrite = _,
              type: "checkbox"
            }, null, 8, Xu), [
              [lf, w.allowWrite]
            ]),
            b[14] || (b[14] = j("span", null, "Allow the agent to write (apt, systemctl, edit files…)", -1))
          ]),
          b[15] || (b[15] = j("span", { class: "muted" }, "off = read-only guard blocks mutating commands; human terminal is never guarded", -1))
        ]),
        j("div", zu, [
          j("button", {
            type: "button",
            disabled: w.testing,
            onClick: (_) => v(w)
          }, "Test connection", 8, Zu),
          j("span", eh, $t(w.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
}), sh = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, i] of t)
    s[n] = i;
  return s;
}, nh = /* @__PURE__ */ sh(th, [["__scopeId", "data-v-ca8fe92f"]]);
function rh(e, t) {
  let s = ff(nh, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toYaml(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  rh as mount
};
