(function(){"use strict";try{if(typeof document<"u"){var a=document.createElement("style");a.appendChild(document.createTextNode(".cred-slot[data-v-c6a4a9b8]{min-width:0;flex:1}.w-260[data-v-c6a4a9b8]{width:260px}.ssh-set[data-v-f12a45b8]{display:flex;flex-direction:column;gap:8px;font-size:var(--fs-sm, 12px);color:var(--text, inherit)}.muted[data-v-f12a45b8]{color:var(--muted, #888)}.empty[data-v-f12a45b8]{font-style:italic}.row[data-v-f12a45b8]{display:flex;gap:8px;align-items:center;flex-wrap:wrap}.row.spread[data-v-f12a45b8]{justify-content:space-between}.self-start[data-v-f12a45b8]{align-self:flex-start}.grow[data-v-f12a45b8]{flex:1}.w-label[data-v-f12a45b8]{width:80px}.w-70[data-v-f12a45b8]{width:70px}.w-120[data-v-f12a45b8]{width:120px}.w-140[data-v-f12a45b8]{width:140px}.w-180[data-v-f12a45b8]{width:180px}.w-260[data-v-f12a45b8]{width:260px}.host-card[data-v-f12a45b8]{border:1px solid var(--border, #444);border-radius:var(--radius, 6px);padding:8px 10px;display:flex;flex-direction:column;gap:6px;background:var(--panel, transparent)}.chk[data-v-f12a45b8]{cursor:pointer}.chk input[data-v-f12a45b8]{height:auto}label[data-v-f12a45b8]{display:flex;gap:6px;align-items:center}input[data-v-f12a45b8],select[data-v-f12a45b8]{height:24px;padding:2px 6px;color:var(--text, inherit);background:var(--bg, transparent);border:1px solid var(--border, #444);border-radius:5px;font-family:inherit;font-size:inherit}button[data-v-f12a45b8]{padding:2px 10px;color:var(--text, inherit);background:var(--panel, transparent);border:1px solid var(--border, #444);border-radius:5px;cursor:pointer;font-size:inherit}button[data-v-f12a45b8]:hover:not(:disabled){border-color:var(--muted, #888)}button[data-v-f12a45b8]:disabled{opacity:.5;cursor:default}")),document.head.appendChild(a)}}catch(t){console.error("vite-plugin-css-injected-by-js",t)}})();
/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Ls(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const K = {}, rt = [], Oe = () => {
}, kn = () => !1, is = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), rs = (e) => e.startsWith("onUpdate:"), Q = Object.assign, Ks = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, ir = Object.prototype.hasOwnProperty, j = (e, t) => ir.call(e, t), P = Array.isArray, ot = (e) => Dt(e) === "[object Map]", dt = (e) => Dt(e) === "[object Set]", an = (e) => Dt(e) === "[object Date]", F = (e) => typeof e == "function", G = (e) => typeof e == "string", Pe = (e) => typeof e == "symbol", $ = (e) => e !== null && typeof e == "object", Jn = (e) => ($(e) || F(e)) && F(e.then) && F(e.catch), qn = Object.prototype.toString, Dt = (e) => qn.call(e), rr = (e) => Dt(e).slice(8, -1), Gn = (e) => Dt(e) === "[object Object]", Ws = (e) => G(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, St = /* @__PURE__ */ Ls(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), os = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, or = /-\w/g, de = os(
  (e) => e.replace(or, (t) => t.slice(1).toUpperCase())
), lr = /\B([A-Z])/g, st = os(
  (e) => e.replace(lr, "-$1").toLowerCase()
), Yn = os((e) => e.charAt(0).toUpperCase() + e.slice(1)), bs = os(
  (e) => e ? `on${Yn(e)}` : ""
), Ee = (e, t) => !Object.is(e, t), Jt = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, zn = (e, t, s, n = !1) => {
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
let dn;
const cs = () => dn || (dn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function Bs(e) {
  if (P(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], i = G(n) ? ar(n) : Bs(n);
      if (i)
        for (const r in i)
          t[r] = i[r];
    }
    return t;
  } else if (G(e) || $(e))
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
function ks(e) {
  let t = "";
  if (G(e))
    t = e;
  else if (P(e))
    for (let s = 0; s < e.length; s++) {
      const n = ks(e[s]);
      n && (t += n + " ");
    }
  else if ($(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const dr = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", pr = /* @__PURE__ */ Ls(dr);
function Xn(e) {
  return !!e || e === "";
}
function hr(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = pt(e[n], t[n]);
  return s;
}
function pt(e, t) {
  if (e === t) return !0;
  let s = an(e), n = an(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Pe(e), n = Pe(t), s || n)
    return e === t;
  if (s = P(e), n = P(t), s || n)
    return s && n ? hr(e, t) : !1;
  if (s = $(e), n = $(t), s || n) {
    if (!s || !n)
      return !1;
    const i = Object.keys(e).length, r = Object.keys(t).length;
    if (i !== r)
      return !1;
    for (const o in e) {
      const l = e.hasOwnProperty(o), f = t.hasOwnProperty(o);
      if (l && !f || !l && f || !pt(e[o], t[o]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function Js(e, t) {
  return e.findIndex((s) => pt(s, t));
}
const Zn = (e) => !!(e && e.__v_isRef === !0), Ps = (e) => G(e) ? e : e == null ? "" : P(e) || $(e) && (e.toString === qn || !F(e.toString)) ? Zn(e) ? Ps(e.value) : JSON.stringify(e, Qn, 2) : String(e), Qn = (e, t) => Zn(t) ? Qn(e, t.value) : ot(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, i], r) => (s[ys(n, r) + " =>"] = i, s),
    {}
  )
} : dt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => ys(s))
} : Pe(t) ? ys(t) : $(t) && !P(t) && !Gn(t) ? String(t) : t, ys = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Pe(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Z;
class gr {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && Z && (Z.active ? (this.parent = Z, this.index = (Z.scopes || (Z.scopes = [])).push(
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
      const s = Z;
      try {
        return Z = this, t();
      } finally {
        Z = s;
      }
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = Z, Z = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (Z === this)
        Z = this.prevScope;
      else {
        let t = Z;
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
function mr() {
  return Z;
}
let B;
const vs = /* @__PURE__ */ new WeakSet();
class ei {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, Z && (Z.active ? Z.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, vs.has(this) && (vs.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || si(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, pn(this), ni(this);
    const t = B, s = pe;
    B = this, pe = !0;
    try {
      return this.fn();
    } finally {
      ii(this), B = t, pe = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        Ys(t);
      this.deps = this.depsTail = void 0, pn(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? vs.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Ms(this) && this.run();
  }
  get dirty() {
    return Ms(this);
  }
}
let ti = 0, wt, Ct;
function si(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Ct, Ct = e;
    return;
  }
  e.next = wt, wt = e;
}
function qs() {
  ti++;
}
function Gs() {
  if (--ti > 0)
    return;
  if (Ct) {
    let t = Ct;
    for (Ct = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; wt; ) {
    let t = wt;
    for (wt = void 0; t; ) {
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
function ni(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function ii(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const i = n.prevDep;
    n.version === -1 ? (n === s && (s = i), Ys(n), _r(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = i;
  }
  e.deps = t, e.depsTail = s;
}
function Ms(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (ri(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function ri(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Pt) || (e.globalVersion = Pt, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Ms(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = B, n = pe;
  B = e, pe = !0;
  try {
    ni(e);
    const i = e.fn(e._value);
    (t.version === 0 || Ee(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    B = s, pe = n, ii(e), e.flags &= -3;
  }
}
function Ys(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: i } = e;
  if (n && (n.nextSub = i, e.prevSub = void 0), i && (i.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let r = s.computed.deps; r; r = r.nextDep)
      Ys(r, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function _r(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let pe = !0;
const oi = [];
function Me() {
  oi.push(pe), pe = !1;
}
function Ie() {
  const e = oi.pop();
  pe = e === void 0 ? !0 : e;
}
function pn(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const s = B;
    B = void 0;
    try {
      t();
    } finally {
      B = s;
    }
  }
}
let Pt = 0;
class br {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class zs {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!B || !pe || B === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== B)
      s = this.activeLink = new br(B, this), B.deps ? (s.prevDep = B.depsTail, B.depsTail.nextDep = s, B.depsTail = s) : B.deps = B.depsTail = s, li(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = B.depsTail, s.nextDep = void 0, B.depsTail.nextDep = s, B.depsTail = s, B.deps === s && (B.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, Pt++, this.notify(t);
  }
  notify(t) {
    qs();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      Gs();
    }
  }
}
function li(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let n = t.deps; n; n = n.nextDep)
        li(n);
    }
    const s = e.dep.subs;
    s !== e && (e.prevSub = s, s && (s.nextSub = e)), e.dep.subs = e;
  }
}
const Is = /* @__PURE__ */ new WeakMap(), et = /* @__PURE__ */ Symbol(
  ""
), Rs = /* @__PURE__ */ Symbol(
  ""
), Mt = /* @__PURE__ */ Symbol(
  ""
);
function ee(e, t, s) {
  if (pe && B) {
    let n = Is.get(e);
    n || Is.set(e, n = /* @__PURE__ */ new Map());
    let i = n.get(s);
    i || (n.set(s, i = new zs()), i.map = n, i.key = s), i.track();
  }
}
function Ne(e, t, s, n, i, r) {
  const o = Is.get(e);
  if (!o) {
    Pt++;
    return;
  }
  const l = (f) => {
    f && f.trigger();
  };
  if (qs(), t === "clear")
    o.forEach(l);
  else {
    const f = P(e), d = f && Ws(s);
    if (f && s === "length") {
      const a = Number(n);
      o.forEach((h, E) => {
        (E === "length" || E === Mt || !Pe(E) && E >= a) && l(h);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(Mt)), t) {
        case "add":
          f ? d && l(o.get("length")) : (l(o.get(et)), ot(e) && l(o.get(Rs)));
          break;
        case "delete":
          f || (l(o.get(et)), ot(e) && l(o.get(Rs)));
          break;
        case "set":
          ot(e) && l(o.get(et));
          break;
      }
  }
  Gs();
}
function nt(e) {
  const t = /* @__PURE__ */ N(e);
  return t === e ? t : (ee(t, "iterate", Mt), /* @__PURE__ */ ue(e) ? t : t.map(he));
}
function fs(e) {
  return ee(e = /* @__PURE__ */ N(e), "iterate", Mt), e;
}
function Ce(e, t) {
  return /* @__PURE__ */ $e(e) ? ft(/* @__PURE__ */ tt(e) ? he(t) : t) : he(t);
}
const yr = {
  __proto__: null,
  [Symbol.iterator]() {
    return xs(this, Symbol.iterator, (e) => Ce(this, e));
  },
  concat(...e) {
    return nt(this).concat(
      ...e.map((t) => P(t) ? nt(t) : t)
    );
  },
  entries() {
    return xs(this, "entries", (e) => (e[1] = Ce(this, e[1]), e));
  },
  every(e, t) {
    return Re(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return Re(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => Ce(this, n)),
      arguments
    );
  },
  find(e, t) {
    return Re(
      this,
      "find",
      e,
      t,
      (s) => Ce(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return Re(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return Re(
      this,
      "findLast",
      e,
      t,
      (s) => Ce(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return Re(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return Re(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return Ss(this, "includes", e);
  },
  indexOf(...e) {
    return Ss(this, "indexOf", e);
  },
  join(e) {
    return nt(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return Ss(this, "lastIndexOf", e);
  },
  map(e, t) {
    return Re(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return bt(this, "pop");
  },
  push(...e) {
    return bt(this, "push", e);
  },
  reduce(e, ...t) {
    return hn(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return hn(this, "reduceRight", e, t);
  },
  shift() {
    return bt(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return Re(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return bt(this, "splice", e);
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
    return bt(this, "unshift", e);
  },
  values() {
    return xs(this, "values", (e) => Ce(this, e));
  }
};
function xs(e, t, s) {
  const n = fs(e), i = n[t]();
  return n !== e && !/* @__PURE__ */ ue(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = s(r.value)), r;
  }), i;
}
const vr = Array.prototype;
function Re(e, t, s, n, i, r) {
  const o = fs(e), l = o !== e && !/* @__PURE__ */ ue(e), f = o[t];
  if (f !== vr[t]) {
    const h = f.apply(e, r);
    return l ? he(h) : h;
  }
  let d = s;
  o !== e && (l ? d = function(h, E) {
    return s.call(this, Ce(e, h), E, e);
  } : s.length > 2 && (d = function(h, E) {
    return s.call(this, h, E, e);
  }));
  const a = f.call(o, d, n);
  return l && i ? i(a) : a;
}
function hn(e, t, s, n) {
  const i = fs(e), r = i !== e && !/* @__PURE__ */ ue(e);
  let o = s, l = !1;
  i !== e && (r ? (l = n.length === 0, o = function(d, a, h) {
    return l && (l = !1, d = Ce(e, d)), s.call(this, d, Ce(e, a), h, e);
  }) : s.length > 3 && (o = function(d, a, h) {
    return s.call(this, d, a, h, e);
  }));
  const f = i[t](o, ...n);
  return l ? Ce(e, f) : f;
}
function Ss(e, t, s) {
  const n = /* @__PURE__ */ N(e);
  ee(n, "iterate", Mt);
  const i = n[t](...s);
  return (i === -1 || i === !1) && /* @__PURE__ */ Qs(s[0]) ? (s[0] = /* @__PURE__ */ N(s[0]), n[t](...s)) : i;
}
function bt(e, t, s = []) {
  Me(), qs();
  const n = (/* @__PURE__ */ N(e))[t].apply(e, s);
  return Gs(), Ie(), n;
}
const xr = /* @__PURE__ */ Ls("__proto__,__v_isRef,__isVue"), ci = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Pe)
);
function Sr(e) {
  Pe(e) || (e = String(e));
  const t = /* @__PURE__ */ N(this);
  return ee(t, "has", e), t.hasOwnProperty(e);
}
class fi {
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
      return n === (i ? r ? Rr : pi : r ? di : ai).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = P(t);
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
      /* @__PURE__ */ te(t) ? t : n
    );
    if ((Pe(s) ? ci.has(s) : xr(s)) || (i || ee(t, "get", s), r))
      return l;
    if (/* @__PURE__ */ te(l)) {
      const f = o && Ws(s) ? l : l.value;
      return i && $(f) ? /* @__PURE__ */ Vs(f) : f;
    }
    return $(l) ? i ? /* @__PURE__ */ Vs(l) : /* @__PURE__ */ us(l) : l;
  }
}
class ui extends fi {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, i) {
    let r = t[s];
    const o = P(t) && Ws(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ $e(r);
      if (!/* @__PURE__ */ ue(n) && !/* @__PURE__ */ $e(n) && (r = /* @__PURE__ */ N(r), n = /* @__PURE__ */ N(n)), !o && /* @__PURE__ */ te(r) && !/* @__PURE__ */ te(n))
        return d || (r.value = n), !0;
    }
    const l = o ? Number(s) < t.length : j(t, s), f = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ te(t) ? t : i
    );
    return t === /* @__PURE__ */ N(i) && f && (l ? Ee(n, r) && Ne(t, "set", s, n) : Ne(t, "add", s, n)), f;
  }
  deleteProperty(t, s) {
    const n = j(t, s);
    t[s];
    const i = Reflect.deleteProperty(t, s);
    return i && n && Ne(t, "delete", s, void 0), i;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Pe(s) || !ci.has(s)) && ee(t, "has", s), n;
  }
  ownKeys(t) {
    return ee(
      t,
      "iterate",
      P(t) ? "length" : et
    ), Reflect.ownKeys(t);
  }
}
class wr extends fi {
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
const Cr = /* @__PURE__ */ new ui(), Tr = /* @__PURE__ */ new wr(), Er = /* @__PURE__ */ new ui(!0);
const Fs = (e) => e, Wt = (e) => Reflect.getPrototypeOf(e);
function Or(e, t, s) {
  return function(...n) {
    const i = this.__v_raw, r = /* @__PURE__ */ N(i), o = ot(r), l = e === "entries" || e === Symbol.iterator && o, f = e === "keys" && o, d = i[e](...n), a = s ? Fs : t ? ft : he;
    return !t && ee(
      r,
      "iterate",
      f ? Rs : et
    ), Q(
      // inheriting all iterator properties
      Object.create(d),
      {
        // iterator protocol
        next() {
          const { value: h, done: E } = d.next();
          return E ? { value: h, done: E } : {
            value: l ? [a(h[0]), a(h[1])] : a(h),
            done: E
          };
        }
      }
    );
  };
}
function Bt(e) {
  return function(...t) {
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function Ar(e, t) {
  const s = {
    get(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ N(r), l = /* @__PURE__ */ N(i);
      e || (Ee(i, l) && ee(o, "get", i), ee(o, "get", l));
      const { has: f } = Wt(o), d = t ? Fs : e ? ft : he;
      if (f.call(o, i))
        return d(r.get(i));
      if (f.call(o, l))
        return d(r.get(l));
      r !== o && r.get(i);
    },
    get size() {
      const i = this.__v_raw;
      return !e && ee(/* @__PURE__ */ N(i), "iterate", et), i.size;
    },
    has(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ N(r), l = /* @__PURE__ */ N(i);
      return e || (Ee(i, l) && ee(o, "has", i), ee(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, f = /* @__PURE__ */ N(l), d = t ? Fs : e ? ft : he;
      return !e && ee(f, "iterate", et), l.forEach((a, h) => i.call(r, d(a), d(h), o));
    }
  };
  return Q(
    s,
    e ? {
      add: Bt("add"),
      set: Bt("set"),
      delete: Bt("delete"),
      clear: Bt("clear")
    } : {
      add(i) {
        const r = /* @__PURE__ */ N(this), o = Wt(r), l = /* @__PURE__ */ N(i), f = !t && !/* @__PURE__ */ ue(i) && !/* @__PURE__ */ $e(i) ? l : i;
        return o.has.call(r, f) || Ee(i, f) && o.has.call(r, i) || Ee(l, f) && o.has.call(r, l) || (r.add(f), Ne(r, "add", f, f)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ ue(r) && !/* @__PURE__ */ $e(r) && (r = /* @__PURE__ */ N(r));
        const o = /* @__PURE__ */ N(this), { has: l, get: f } = Wt(o);
        let d = l.call(o, i);
        d || (i = /* @__PURE__ */ N(i), d = l.call(o, i));
        const a = f.call(o, i);
        return o.set(i, r), d ? Ee(r, a) && Ne(o, "set", i, r) : Ne(o, "add", i, r), this;
      },
      delete(i) {
        const r = /* @__PURE__ */ N(this), { has: o, get: l } = Wt(r);
        let f = o.call(r, i);
        f || (i = /* @__PURE__ */ N(i), f = o.call(r, i)), l && l.call(r, i);
        const d = r.delete(i);
        return f && Ne(r, "delete", i, void 0), d;
      },
      clear() {
        const i = /* @__PURE__ */ N(this), r = i.size !== 0, o = i.clear();
        return r && Ne(
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
    s[i] = Or(i, e, t);
  }), s;
}
function Xs(e, t) {
  const s = Ar(e, t);
  return (n, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? n : Reflect.get(
    j(s, i) && i in n ? s : n,
    i,
    r
  );
}
const Pr = {
  get: /* @__PURE__ */ Xs(!1, !1)
}, Mr = {
  get: /* @__PURE__ */ Xs(!1, !0)
}, Ir = {
  get: /* @__PURE__ */ Xs(!0, !1)
};
const ai = /* @__PURE__ */ new WeakMap(), di = /* @__PURE__ */ new WeakMap(), pi = /* @__PURE__ */ new WeakMap(), Rr = /* @__PURE__ */ new WeakMap();
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
function us(e) {
  return /* @__PURE__ */ $e(e) ? e : Zs(
    e,
    !1,
    Cr,
    Pr,
    ai
  );
}
// @__NO_SIDE_EFFECTS__
function Vr(e) {
  return Zs(
    e,
    !1,
    Er,
    Mr,
    di
  );
}
// @__NO_SIDE_EFFECTS__
function Vs(e) {
  return Zs(
    e,
    !0,
    Tr,
    Ir,
    pi
  );
}
function Zs(e, t, s, n, i) {
  if (!$(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
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
function tt(e) {
  return /* @__PURE__ */ $e(e) ? /* @__PURE__ */ tt(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function $e(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function ue(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function Qs(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function N(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ N(t) : e;
}
function Dr(e) {
  return !j(e, "__v_skip") && Object.isExtensible(e) && zn(e, "__v_skip", !0), e;
}
const he = (e) => $(e) ? /* @__PURE__ */ us(e) : e, ft = (e) => $(e) ? /* @__PURE__ */ Vs(e) : e;
// @__NO_SIDE_EFFECTS__
function te(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function zt(e) {
  return Hr(e, !1);
}
function Hr(e, t) {
  return /* @__PURE__ */ te(e) ? e : new Nr(e, t);
}
class Nr {
  constructor(t, s) {
    this.dep = new zs(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ N(t), this._value = s ? t : he(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ ue(t) || /* @__PURE__ */ $e(t);
    t = n ? t : /* @__PURE__ */ N(t), Ee(t, s) && (this._rawValue = t, this._value = n ? t : he(t), this.dep.trigger());
  }
}
function jr(e) {
  return /* @__PURE__ */ te(e) ? e.value : e;
}
const Ur = {
  get: (e, t, s) => t === "__v_raw" ? e : jr(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const i = e[t];
    return /* @__PURE__ */ te(i) && !/* @__PURE__ */ te(s) ? (i.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function hi(e) {
  return /* @__PURE__ */ tt(e) ? e : new Proxy(e, Ur);
}
class $r {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new zs(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Pt - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    B !== this)
      return si(this, !0), !0;
  }
  get value() {
    const t = this.dep.track();
    return ri(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter && this.setter(t);
  }
}
// @__NO_SIDE_EFFECTS__
function Lr(e, t, s = !1) {
  let n, i;
  return F(e) ? n = e : (n = e.get, i = e.set), new $r(n, i, s);
}
const kt = {}, Xt = /* @__PURE__ */ new WeakMap();
let Qe;
function Kr(e, t = !1, s = Qe) {
  if (s) {
    let n = Xt.get(s);
    n || Xt.set(s, n = []), n.push(e);
  }
}
function Wr(e, t, s = K) {
  const { immediate: n, deep: i, once: r, scheduler: o, augmentJob: l, call: f } = s, d = (M) => i ? M : /* @__PURE__ */ ue(M) || i === !1 || i === 0 ? je(M, 1) : je(M);
  let a, h, E, y, v = !1, w = !1;
  if (/* @__PURE__ */ te(e) ? (h = () => e.value, v = /* @__PURE__ */ ue(e)) : /* @__PURE__ */ tt(e) ? (h = () => d(e), v = !0) : P(e) ? (w = !0, v = e.some((M) => /* @__PURE__ */ tt(M) || /* @__PURE__ */ ue(M)), h = () => e.map((M) => {
    if (/* @__PURE__ */ te(M))
      return M.value;
    if (/* @__PURE__ */ tt(M))
      return d(M);
    if (F(M))
      return f ? f(M, 2) : M();
  })) : F(e) ? t ? h = f ? () => f(e, 2) : e : h = () => {
    if (E) {
      Me();
      try {
        E();
      } finally {
        Ie();
      }
    }
    const M = Qe;
    Qe = a;
    try {
      return f ? f(e, 3, [y]) : e(y);
    } finally {
      Qe = M;
    }
  } : h = Oe, t && i) {
    const M = h, z = i === !0 ? 1 / 0 : i;
    h = () => je(M(), z);
  }
  const q = mr(), R = () => {
    a.stop(), q && q.active && Ks(q.effects, a);
  };
  if (r && t) {
    const M = t;
    t = (...z) => {
      const me = M(...z);
      return R(), me;
    };
  }
  let H = w ? new Array(e.length).fill(kt) : kt;
  const k = (M) => {
    if (!(!(a.flags & 1) || !a.dirty && !M))
      if (t) {
        const z = a.run();
        if (M || i || v || (w ? z.some((me, _e) => Ee(me, H[_e])) : Ee(z, H))) {
          E && E();
          const me = Qe;
          Qe = a;
          try {
            const _e = [
              z,
              // pass undefined as the old value when it's changed for the first time
              H === kt ? void 0 : w && H[0] === kt ? [] : H,
              y
            ];
            H = z, f ? f(t, 3, _e) : (
              // @ts-expect-error
              t(..._e)
            );
          } finally {
            Qe = me;
          }
        }
      } else
        a.run();
  };
  return l && l(k), a = new ei(h), a.scheduler = o ? () => o(k, !1) : k, y = (M) => Kr(M, !1, a), E = a.onStop = () => {
    const M = Xt.get(a);
    if (M) {
      if (f)
        f(M, 4);
      else
        for (const z of M) z();
      Xt.delete(a);
    }
  }, t ? n ? k(!0) : H = a.run() : o ? o(k.bind(null, !0), !0) : a.run(), R.pause = a.pause.bind(a), R.resume = a.resume.bind(a), R.stop = R, R;
}
function je(e, t = 1 / 0, s) {
  if (t <= 0 || !$(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ te(e))
    je(e.value, t, s);
  else if (P(e))
    for (let n = 0; n < e.length; n++)
      je(e[n], t, s);
  else if (dt(e) || ot(e))
    e.forEach((n) => {
      je(n, t, s);
    });
  else if (Gn(e)) {
    for (const n in e)
      je(e[n], t, s);
    for (const n of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, n) && je(e[n], t, s);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function Ht(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (i) {
    as(i, t, s);
  }
}
function ge(e, t, s, n) {
  if (F(e)) {
    const i = Ht(e, t, s, n);
    return i && Jn(i) && i.catch((r) => {
      as(r, t, s);
    }), i;
  }
  if (P(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(ge(e[r], t, s, n));
    return i;
  }
}
function as(e, t, s, n = !0) {
  const i = t ? t.vnode : null, { errorHandler: r, throwUnhandledErrorInProduction: o } = t && t.appContext.config || K;
  if (t) {
    let l = t.parent;
    const f = t.proxy, d = `https://vuejs.org/error-reference/#runtime-${s}`;
    for (; l; ) {
      const a = l.ec;
      if (a) {
        for (let h = 0; h < a.length; h++)
          if (a[h](e, f, d) === !1)
            return;
      }
      l = l.parent;
    }
    if (r) {
      Me(), Ht(r, null, 10, [
        e,
        f,
        d
      ]), Ie();
      return;
    }
  }
  Br(e, s, i, n, o);
}
function Br(e, t, s, n = !0, i = !1) {
  if (i)
    throw e;
  console.error(e);
}
const ie = [];
let we = -1;
const lt = [];
let We = null, it = 0;
const gi = /* @__PURE__ */ Promise.resolve();
let Zt = null;
function mi(e) {
  const t = Zt || gi;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function kr(e) {
  let t = we + 1, s = ie.length;
  for (; t < s; ) {
    const n = t + s >>> 1, i = ie[n], r = It(i);
    r < e || r === e && i.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function en(e) {
  if (!(e.flags & 1)) {
    const t = It(e), s = ie[ie.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= It(s) ? ie.push(e) : ie.splice(kr(t), 0, e), e.flags |= 1, _i();
  }
}
function _i() {
  Zt || (Zt = gi.then(yi));
}
function Jr(e) {
  P(e) ? lt.push(...e) : We && e.id === -1 ? We.splice(it + 1, 0, e) : e.flags & 1 || (lt.push(e), e.flags |= 1), _i();
}
function gn(e, t, s = we + 1) {
  for (; s < ie.length; s++) {
    const n = ie[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      ie.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function bi(e) {
  if (lt.length) {
    const t = [...new Set(lt)].sort(
      (s, n) => It(s) - It(n)
    );
    if (lt.length = 0, We) {
      We.push(...t);
      return;
    }
    for (We = t, it = 0; it < We.length; it++) {
      const s = We[it];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    We = null, it = 0;
  }
}
const It = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function yi(e) {
  try {
    for (we = 0; we < ie.length; we++) {
      const t = ie[we];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Ht(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; we < ie.length; we++) {
      const t = ie[we];
      t && (t.flags &= -2);
    }
    we = -1, ie.length = 0, bi(), Zt = null, (ie.length || lt.length) && yi();
  }
}
let fe = null, vi = null;
function Qt(e) {
  const t = fe;
  return fe = e, vi = e && e.type.__scopeId || null, t;
}
function qr(e, t = fe, s) {
  if (!t || e._n)
    return e;
  const n = (...i) => {
    n._d && On(-1);
    const r = Qt(t);
    let o;
    try {
      o = e(...i);
    } finally {
      Qt(r), n._d && On(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function Fe(e, t) {
  if (fe === null)
    return e;
  const s = gs(fe), n = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [r, o, l, f = K] = t[i];
    r && (F(r) && (r = {
      mounted: r,
      updated: r
    }), r.deep && je(o), n.push({
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
function ze(e, t, s, n) {
  const i = e.dirs, r = t && t.dirs;
  for (let o = 0; o < i.length; o++) {
    const l = i[o];
    r && (l.oldValue = r[o].value);
    let f = l.dir[n];
    f && (Me(), ge(f, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Ie());
  }
}
function Gr(e, t) {
  if (re) {
    let s = re.provides;
    const n = re.parent && re.parent.provides;
    n === s && (s = re.provides = Object.create(n)), s[e] = t;
  }
}
function qt(e, t, s = !1) {
  const n = qo();
  if (n || ct) {
    let i = ct ? ct._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return s && F(t) ? t.call(n && n.proxy) : t;
  }
}
const Yr = /* @__PURE__ */ Symbol.for("v-scx"), zr = () => qt(Yr);
function Gt(e, t, s) {
  return xi(e, t, s);
}
function xi(e, t, s = K) {
  const { immediate: n, deep: i, flush: r, once: o } = s, l = Q({}, s), f = t && n || !t && r !== "post";
  let d;
  if (Ft) {
    if (r === "sync") {
      const y = zr();
      d = y.__watcherHandles || (y.__watcherHandles = []);
    } else if (!f) {
      const y = () => {
      };
      return y.stop = Oe, y.resume = Oe, y.pause = Oe, y;
    }
  }
  const a = re;
  l.call = (y, v, w) => ge(y, a, v, w);
  let h = !1;
  r === "post" ? l.scheduler = (y) => {
    oe(y, a && a.suspense);
  } : r !== "sync" && (h = !0, l.scheduler = (y, v) => {
    v ? y() : en(y);
  }), l.augmentJob = (y) => {
    t && (y.flags |= 4), h && (y.flags |= 2, a && (y.id = a.uid, y.i = a));
  };
  const E = Wr(e, t, l);
  return Ft && (d ? d.push(E) : f && E()), E;
}
function Xr(e, t, s) {
  const n = this.proxy, i = G(e) ? e.includes(".") ? Si(n, e) : () => n[e] : e.bind(n, n);
  let r;
  F(t) ? r = t : (r = t.handler, s = t);
  const o = Nt(this), l = xi(i, r.bind(n), s);
  return o(), l;
}
function Si(e, t) {
  const s = t.split(".");
  return () => {
    let n = e;
    for (let i = 0; i < s.length && n; i++)
      n = n[s[i]];
    return n;
  };
}
const Zr = /* @__PURE__ */ Symbol("_vte"), Qr = (e) => e.__isTeleport, ws = /* @__PURE__ */ Symbol("_leaveCb");
function tn(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, tn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function wi(e, t) {
  return F(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    Q({ name: e.name }, t, { setup: e })
  ) : e;
}
function Ci(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function mn(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const es = /* @__PURE__ */ new WeakMap();
function Tt(e, t, s, n, i = !1) {
  if (P(e)) {
    e.forEach(
      (w, q) => Tt(
        w,
        t && (P(t) ? t[q] : t),
        s,
        n,
        i
      )
    );
    return;
  }
  if (Et(n) && !i) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Tt(e, t, s, n.component.subTree);
    return;
  }
  const r = n.shapeFlag & 4 ? gs(n.component) : n.el, o = i ? null : r, { i: l, r: f } = e, d = t && t.r, a = l.refs === K ? l.refs = {} : l.refs, h = l.setupState, E = /* @__PURE__ */ N(h), y = h === K ? kn : (w) => mn(a, w) ? !1 : j(E, w), v = (w, q) => !(q && mn(a, q));
  if (d != null && d !== f) {
    if (_n(t), G(d))
      a[d] = null, y(d) && (h[d] = null);
    else if (/* @__PURE__ */ te(d)) {
      const w = t;
      v(d, w.k) && (d.value = null), w.k && (a[w.k] = null);
    }
  }
  if (F(f)) {
    Me();
    try {
      Ht(f, l, 12, [o, a]);
    } finally {
      Ie();
    }
  } else {
    const w = G(f), q = /* @__PURE__ */ te(f);
    if (w || q) {
      const R = () => {
        if (e.f) {
          const H = w ? y(f) ? h[f] : a[f] : v() || !e.k ? f.value : a[e.k];
          if (i)
            P(H) && Ks(H, r);
          else if (P(H))
            H.includes(r) || H.push(r);
          else if (w)
            a[f] = [r], y(f) && (h[f] = a[f]);
          else {
            const k = [r];
            v(f, e.k) && (f.value = k), e.k && (a[e.k] = k);
          }
        } else w ? (a[f] = o, y(f) && (h[f] = o)) : q && (v(f, e.k) && (f.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const H = () => {
          R(), es.delete(e);
        };
        H.id = -1, es.set(e, H), oe(H, s);
      } else
        _n(e), R();
    }
  }
}
function _n(e) {
  const t = es.get(e);
  t && (t.flags |= 8, es.delete(e));
}
cs().requestIdleCallback;
cs().cancelIdleCallback;
const Et = (e) => !!e.type.__asyncLoader, Ti = (e) => e.type.__isKeepAlive;
function eo(e, t) {
  Ei(e, "a", t);
}
function to(e, t) {
  Ei(e, "da", t);
}
function Ei(e, t, s = re) {
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
      Ti(i.parent.vnode) && so(n, t, s, i), i = i.parent;
  }
}
function so(e, t, s, n) {
  const i = ds(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Pi(() => {
    Ks(n[t], i);
  }, s);
}
function ds(e, t, s = re, n = !1) {
  if (s) {
    const i = s[e] || (s[e] = []), r = t.__weh || (t.__weh = (...o) => {
      Me();
      const l = Nt(s), f = ge(t, s, e, o);
      return l(), Ie(), f;
    });
    return n ? i.unshift(r) : i.push(r), r;
  }
}
const Le = (e) => (t, s = re) => {
  (!Ft || e === "sp") && ds(e, (...n) => t(...n), s);
}, no = Le("bm"), Oi = Le("m"), io = Le(
  "bu"
), ro = Le("u"), Ai = Le(
  "bum"
), Pi = Le("um"), oo = Le(
  "sp"
), lo = Le("rtg"), co = Le("rtc");
function fo(e, t = re) {
  ds("ec", e, t);
}
const uo = /* @__PURE__ */ Symbol.for("v-ndc");
function bn(e, t, s, n) {
  let i;
  const r = s, o = P(e);
  if (o || G(e)) {
    const l = o && /* @__PURE__ */ tt(e);
    let f = !1, d = !1;
    l && (f = !/* @__PURE__ */ ue(e), d = /* @__PURE__ */ $e(e), e = fs(e)), i = new Array(e.length);
    for (let a = 0, h = e.length; a < h; a++)
      i[a] = t(
        f ? d ? ft(he(e[a])) : he(e[a]) : e[a],
        a,
        void 0,
        r
      );
  } else if (typeof e == "number") {
    i = new Array(e);
    for (let l = 0; l < e; l++)
      i[l] = t(l + 1, l, void 0, r);
  } else if ($(e))
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
const Ds = (e) => e ? Xi(e) ? gs(e) : Ds(e.parent) : null, Ot = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ Q(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => e.props,
    $attrs: (e) => e.attrs,
    $slots: (e) => e.slots,
    $refs: (e) => e.refs,
    $parent: (e) => Ds(e.parent),
    $root: (e) => Ds(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Ii(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      en(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = mi.bind(e.proxy)),
    $watch: (e) => Xr.bind(e)
  })
), Cs = (e, t) => e !== K && !e.__isScriptSetup && j(e, t), ao = {
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
        if (Cs(n, t))
          return o[t] = 1, n[t];
        if (i !== K && j(i, t))
          return o[t] = 2, i[t];
        if (j(r, t))
          return o[t] = 3, r[t];
        if (s !== K && j(s, t))
          return o[t] = 4, s[t];
        Hs && (o[t] = 0);
      }
    }
    const d = Ot[t];
    let a, h;
    if (d)
      return t === "$attrs" && ee(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== K && j(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      h = f.config.globalProperties, j(h, t)
    )
      return h[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: i, ctx: r } = e;
    return Cs(i, t) ? (i[t] = s, !0) : n !== K && j(n, t) ? (n[t] = s, !0) : j(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: i, props: r, type: o }
  }, l) {
    let f;
    return !!(s[l] || e !== K && l[0] !== "$" && j(e, l) || Cs(t, l) || j(r, l) || j(n, l) || j(Ot, l) || j(i.config.globalProperties, l) || (f = o.__cssModules) && f[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : j(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function yn(e) {
  return P(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let Hs = !0;
function po(e) {
  const t = Ii(e), s = e.proxy, n = e.ctx;
  Hs = !1, t.beforeCreate && vn(t.beforeCreate, e, "bc");
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
    beforeMount: h,
    mounted: E,
    beforeUpdate: y,
    updated: v,
    activated: w,
    deactivated: q,
    beforeDestroy: R,
    beforeUnmount: H,
    destroyed: k,
    unmounted: M,
    render: z,
    renderTracked: me,
    renderTriggered: _e,
    errorCaptured: Ke,
    serverPrefetch: jt,
    // public API
    expose: qe,
    inheritAttrs: ht,
    // assets
    components: Ut,
    directives: $t,
    filters: ms
  } = t;
  if (d && ho(d, n, null), o)
    for (const J in o) {
      const W = o[J];
      F(W) && (n[J] = W.bind(s));
    }
  if (i) {
    const J = i.call(s, s);
    $(J) && (e.data = /* @__PURE__ */ us(J));
  }
  if (Hs = !0, r)
    for (const J in r) {
      const W = r[J], Ge = F(W) ? W.bind(s, s) : F(W.get) ? W.get.bind(s, s) : Oe, Lt = !F(W) && F(W.set) ? W.set.bind(s) : Oe, Ye = Qo({
        get: Ge,
        set: Lt
      });
      Object.defineProperty(n, J, {
        enumerable: !0,
        configurable: !0,
        get: () => Ye.value,
        set: (be) => Ye.value = be
      });
    }
  if (l)
    for (const J in l)
      Mi(l[J], n, s, J);
  if (f) {
    const J = F(f) ? f.call(s) : f;
    Reflect.ownKeys(J).forEach((W) => {
      Gr(W, J[W]);
    });
  }
  a && vn(a, e, "c");
  function se(J, W) {
    P(W) ? W.forEach((Ge) => J(Ge.bind(s))) : W && J(W.bind(s));
  }
  if (se(no, h), se(Oi, E), se(io, y), se(ro, v), se(eo, w), se(to, q), se(fo, Ke), se(co, me), se(lo, _e), se(Ai, H), se(Pi, M), se(oo, jt), P(qe))
    if (qe.length) {
      const J = e.exposed || (e.exposed = {});
      qe.forEach((W) => {
        Object.defineProperty(J, W, {
          get: () => s[W],
          set: (Ge) => s[W] = Ge,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  z && e.render === Oe && (e.render = z), ht != null && (e.inheritAttrs = ht), Ut && (e.components = Ut), $t && (e.directives = $t), jt && Ci(e);
}
function ho(e, t, s = Oe) {
  P(e) && (e = Ns(e));
  for (const n in e) {
    const i = e[n];
    let r;
    $(i) ? "default" in i ? r = qt(
      i.from || n,
      i.default,
      !0
    ) : r = qt(i.from || n) : r = qt(i), /* @__PURE__ */ te(r) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => r.value,
      set: (o) => r.value = o
    }) : t[n] = r;
  }
}
function vn(e, t, s) {
  ge(
    P(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Mi(e, t, s, n) {
  let i = n.includes(".") ? Si(s, n) : () => s[n];
  if (G(e)) {
    const r = t[e];
    F(r) && Gt(i, r);
  } else if (F(e))
    Gt(i, e.bind(s));
  else if ($(e))
    if (P(e))
      e.forEach((r) => Mi(r, t, s, n));
    else {
      const r = F(e.handler) ? e.handler.bind(s) : t[e.handler];
      F(r) && Gt(i, r, e);
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
  ), ts(f, t, o)), $(t) && r.set(t, f), f;
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
  data: xn,
  props: Sn,
  emits: Sn,
  // objects
  methods: vt,
  computed: vt,
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
  components: vt,
  directives: vt,
  // watch
  watch: _o,
  // provide / inject
  provide: xn,
  inject: mo
};
function xn(e, t) {
  return t ? e ? function() {
    return Q(
      F(e) ? e.call(this, this) : e,
      F(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function mo(e, t) {
  return vt(Ns(e), Ns(t));
}
function Ns(e) {
  if (P(e)) {
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
function vt(e, t) {
  return e ? Q(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Sn(e, t) {
  return e ? P(e) && P(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : Q(
    /* @__PURE__ */ Object.create(null),
    yn(e),
    yn(t ?? {})
  ) : t;
}
function _o(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = Q(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = ne(e[n], t[n]);
  return s;
}
function Ri() {
  return {
    app: null,
    config: {
      isNativeTag: kn,
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
function yo(e, t) {
  return function(n, i = null) {
    F(n) || (n = Q({}, n)), i != null && !$(i) && (i = null);
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
      use(a, ...h) {
        return o.has(a) || (a && F(a.install) ? (o.add(a), a.install(d, ...h)) : F(a) && (o.add(a), a(d, ...h))), d;
      },
      mixin(a) {
        return r.mixins.includes(a) || r.mixins.push(a), d;
      },
      component(a, h) {
        return h ? (r.components[a] = h, d) : r.components[a];
      },
      directive(a, h) {
        return h ? (r.directives[a] = h, d) : r.directives[a];
      },
      mount(a, h, E) {
        if (!f) {
          const y = d._ceVNode || Ae(n, i);
          return y.appContext = r, E === !0 ? E = "svg" : E === !1 && (E = void 0), e(y, a, E), f = !0, d._container = a, a.__vue_app__ = d, gs(y.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        f && (ge(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, h) {
        return r.provides[a] = h, d;
      },
      runWithContext(a) {
        const h = ct;
        ct = d;
        try {
          return a();
        } finally {
          ct = h;
        }
      }
    };
    return d;
  };
}
let ct = null;
const vo = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${de(t)}Modifiers`] || e[`${st(t)}Modifiers`];
function xo(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || K;
  let i = s;
  const r = t.startsWith("update:"), o = r && vo(n, t.slice(7));
  o && (o.trim && (i = s.map((a) => G(a) ? a.trim() : a)), o.number && (i = s.map(ls)));
  let l, f = n[l = bs(t)] || // also try camelCase event handler (#2249)
  n[l = bs(de(t))];
  !f && r && (f = n[l = bs(st(t))]), f && ge(
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
    e.emitted[l] = !0, ge(
      d,
      e,
      6,
      i
    );
  }
}
const So = /* @__PURE__ */ new WeakMap();
function Fi(e, t, s = !1) {
  const n = s ? So : t.emitsCache, i = n.get(e);
  if (i !== void 0)
    return i;
  const r = e.emits;
  let o = {}, l = !1;
  if (!F(e)) {
    const f = (d) => {
      const a = Fi(d, t, !0);
      a && (l = !0, Q(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  return !r && !l ? ($(e) && n.set(e, null), null) : (P(r) ? r.forEach((f) => o[f] = null) : Q(o, r), $(e) && n.set(e, o), o);
}
function ps(e, t) {
  return !e || !is(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), j(e, t[0].toLowerCase() + t.slice(1)) || j(e, st(t)) || j(e, t));
}
function wn(e) {
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
    props: h,
    data: E,
    setupState: y,
    ctx: v,
    inheritAttrs: w
  } = e, q = Qt(e);
  let R, H;
  try {
    if (s.shapeFlag & 4) {
      const M = i || n, z = M;
      R = Te(
        d.call(
          z,
          M,
          a,
          h,
          y,
          E,
          v
        )
      ), H = l;
    } else {
      const M = t;
      R = Te(
        M.length > 1 ? M(
          h,
          { attrs: l, slots: o, emit: f }
        ) : M(
          h,
          null
        )
      ), H = t.props ? l : wo(l);
    }
  } catch (M) {
    At.length = 0, as(M, e, 1), R = Ae(Je);
  }
  let k = R;
  if (H && w !== !1) {
    const M = Object.keys(H), { shapeFlag: z } = k;
    M.length && z & 7 && (r && M.some(rs) && (H = Co(
      H,
      r
    )), k = ut(k, H, !1, !0));
  }
  return s.dirs && (k = ut(k, null, !1, !0), k.dirs = k.dirs ? k.dirs.concat(s.dirs) : s.dirs), s.transition && tn(k, s.transition), R = k, Qt(q), R;
}
const wo = (e) => {
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
      return n ? Cn(n, o, d) : !!o;
    if (f & 8) {
      const a = t.dynamicProps;
      for (let h = 0; h < a.length; h++) {
        const E = a[h];
        if (Vi(o, n, E) && !ps(d, E))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? Cn(n, o, d) : !0 : !!o;
  return !1;
}
function Cn(e, t, s) {
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
  return s === "style" && $(n) && $(i) ? !pt(n, i) : n !== i;
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
const Di = {}, Hi = () => Object.create(Di), Ni = (e) => Object.getPrototypeOf(e) === Di;
function Oo(e, t, s, n = !1) {
  const i = {}, r = Hi();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), ji(e, t, i, r);
  for (const o in e.propsOptions[0])
    o in i || (i[o] = void 0);
  s ? e.props = n ? i : /* @__PURE__ */ Vr(i) : e.type.props ? e.props = i : e.props = r, e.attrs = r;
}
function Ao(e, t, s, n) {
  const {
    props: i,
    attrs: r,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ N(i), [f] = e.propsOptions;
  let d = !1;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    (n || o > 0) && !(o & 16)
  ) {
    if (o & 8) {
      const a = e.vnode.dynamicProps;
      for (let h = 0; h < a.length; h++) {
        let E = a[h];
        if (ps(e.emitsOptions, E))
          continue;
        const y = t[E];
        if (f)
          if (j(r, E))
            y !== r[E] && (r[E] = y, d = !0);
          else {
            const v = de(E);
            i[v] = js(
              f,
              l,
              v,
              y,
              e,
              !1
            );
          }
        else
          y !== r[E] && (r[E] = y, d = !0);
      }
    }
  } else {
    ji(e, t, i, r) && (d = !0);
    let a;
    for (const h in l)
      (!t || // for camelCase
      !j(t, h) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = st(h)) === h || !j(t, a))) && (f ? s && // for camelCase
      (s[h] !== void 0 || // for kebab-case
      s[a] !== void 0) && (i[h] = js(
        f,
        l,
        h,
        void 0,
        e,
        !0
      )) : delete i[h]);
    if (r !== l)
      for (const h in r)
        (!t || !j(t, h)) && (delete r[h], d = !0);
  }
  d && Ne(e.attrs, "set", "");
}
function ji(e, t, s, n) {
  const [i, r] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let f in t) {
      if (St(f))
        continue;
      const d = t[f];
      let a;
      i && j(i, a = de(f)) ? !r || !r.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : ps(e.emitsOptions, f) || (!(f in n) || d !== n[f]) && (n[f] = d, o = !0);
    }
  if (r) {
    const f = /* @__PURE__ */ N(s), d = l || K;
    for (let a = 0; a < r.length; a++) {
      const h = r[a];
      s[h] = js(
        i,
        f,
        h,
        d[h],
        e,
        !j(d, h)
      );
    }
  }
  return o;
}
function js(e, t, s, n, i, r) {
  const o = e[s];
  if (o != null) {
    const l = j(o, "default");
    if (l && n === void 0) {
      const f = o.default;
      if (o.type !== Function && !o.skipFactory && F(f)) {
        const { propsDefaults: d } = i;
        if (s in d)
          n = d[s];
        else {
          const a = Nt(i);
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
function Ui(e, t, s = !1) {
  const n = s ? Po : t.propsCache, i = n.get(e);
  if (i)
    return i;
  const r = e.props, o = {}, l = [];
  let f = !1;
  if (!F(e)) {
    const a = (h) => {
      f = !0;
      const [E, y] = Ui(h, t, !0);
      Q(o, E), y && l.push(...y);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!r && !f)
    return $(e) && n.set(e, rt), rt;
  if (P(r))
    for (let a = 0; a < r.length; a++) {
      const h = de(r[a]);
      Tn(h) && (o[h] = K);
    }
  else if (r)
    for (const a in r) {
      const h = de(a);
      if (Tn(h)) {
        const E = r[a], y = o[h] = P(E) || F(E) ? { type: E } : Q({}, E), v = y.type;
        let w = !1, q = !0;
        if (P(v))
          for (let R = 0; R < v.length; ++R) {
            const H = v[R], k = F(H) && H.name;
            if (k === "Boolean") {
              w = !0;
              break;
            } else k === "String" && (q = !1);
          }
        else
          w = F(v) && v.name === "Boolean";
        y[
          0
          /* shouldCast */
        ] = w, y[
          1
          /* shouldCastTrue */
        ] = q, (w || j(y, "default")) && l.push(h);
      }
    }
  const d = [o, l];
  return $(e) && n.set(e, d), d;
}
function Tn(e) {
  return e[0] !== "$" && !St(e);
}
const sn = (e) => e === "_" || e === "_ctx" || e === "$stable", nn = (e) => P(e) ? e.map(Te) : [Te(e)], Mo = (e, t, s) => {
  if (t._n)
    return t;
  const n = qr((...i) => nn(t(...i)), s);
  return n._c = !1, n;
}, $i = (e, t, s) => {
  const n = e._ctx;
  for (const i in e) {
    if (sn(i)) continue;
    const r = e[i];
    if (F(r))
      t[i] = Mo(i, r, n);
    else if (r != null) {
      const o = nn(r);
      t[i] = () => o;
    }
  }
}, Li = (e, t) => {
  const s = nn(t);
  e.slots.default = () => s;
}, Ki = (e, t, s) => {
  for (const n in t)
    (s || !sn(n)) && (e[n] = t[n]);
}, Io = (e, t, s) => {
  const n = e.slots = Hi();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Ki(n, t, s), s && zn(n, "_", i, !0)) : $i(t, n);
  } else t && Li(e, t);
}, Ro = (e, t, s) => {
  const { vnode: n, slots: i } = e;
  let r = !0, o = K;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? r = !1 : Ki(i, t, s) : (r = !t.$stable, $i(t, i)), o = t;
  } else t && (Li(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !sn(l) && o[l] == null && delete i[l];
}, oe = No;
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
    parentNode: h,
    nextSibling: E,
    setScopeId: y = Oe,
    insertStaticContent: v
  } = e, w = (c, u, p, b = null, _ = null, g = null, C = void 0, S = null, x = !!u.dynamicChildren) => {
    if (c === u)
      return;
    c && !yt(c, u) && (b = Kt(c), be(c, _, g, !0), c = null), u.patchFlag === -2 && (x = !1, u.dynamicChildren = null);
    const { type: m, ref: A, shapeFlag: T } = u;
    switch (m) {
      case hs:
        q(c, u, p, b);
        break;
      case Je:
        R(c, u, p, b);
        break;
      case Es:
        c == null && H(u, p, b, C);
        break;
      case ae:
        Ut(
          c,
          u,
          p,
          b,
          _,
          g,
          C,
          S,
          x
        );
        break;
      default:
        T & 1 ? z(
          c,
          u,
          p,
          b,
          _,
          g,
          C,
          S,
          x
        ) : T & 6 ? $t(
          c,
          u,
          p,
          b,
          _,
          g,
          C,
          S,
          x
        ) : (T & 64 || T & 128) && m.process(
          c,
          u,
          p,
          b,
          _,
          g,
          C,
          S,
          x,
          mt
        );
    }
    A != null && _ ? Tt(A, c && c.ref, g, u || c, !u) : A == null && c && c.ref != null && Tt(c.ref, null, g, c, !0);
  }, q = (c, u, p, b) => {
    if (c == null)
      n(
        u.el = l(u.children),
        p,
        b
      );
    else {
      const _ = u.el = c.el;
      u.children !== c.children && d(_, u.children);
    }
  }, R = (c, u, p, b) => {
    c == null ? n(
      u.el = f(u.children || ""),
      p,
      b
    ) : u.el = c.el;
  }, H = (c, u, p, b) => {
    [c.el, c.anchor] = v(
      c.children,
      u,
      p,
      b,
      c.el,
      c.anchor
    );
  }, k = ({ el: c, anchor: u }, p, b) => {
    let _;
    for (; c && c !== u; )
      _ = E(c), n(c, p, b), c = _;
    n(u, p, b);
  }, M = ({ el: c, anchor: u }) => {
    let p;
    for (; c && c !== u; )
      p = E(c), i(c), c = p;
    i(u);
  }, z = (c, u, p, b, _, g, C, S, x) => {
    if (u.type === "svg" ? C = "svg" : u.type === "math" && (C = "mathml"), c == null)
      me(
        u,
        p,
        b,
        _,
        g,
        C,
        S,
        x
      );
    else {
      const m = c.el && c.el._isVueCE ? c.el : null;
      try {
        m && m._beginPatch(), jt(
          c,
          u,
          _,
          g,
          C,
          S,
          x
        );
      } finally {
        m && m._endPatch();
      }
    }
  }, me = (c, u, p, b, _, g, C, S) => {
    let x, m;
    const { props: A, shapeFlag: T, transition: O, dirs: I } = c;
    if (x = c.el = o(
      c.type,
      g,
      A && A.is,
      A
    ), T & 8 ? a(x, c.children) : T & 16 && Ke(
      c.children,
      x,
      null,
      b,
      _,
      Ts(c, g),
      C,
      S
    ), I && ze(c, null, b, "created"), _e(x, c, c.scopeId, C, b), A) {
      for (const L in A)
        L !== "value" && !St(L) && r(x, L, null, A[L], g, b);
      "value" in A && r(x, "value", null, A.value, g), (m = A.onVnodeBeforeMount) && Se(m, b, c);
    }
    I && ze(c, null, b, "beforeMount");
    const D = Do(_, O);
    D && O.beforeEnter(x), n(x, u, p), ((m = A && A.onVnodeMounted) || D || I) && oe(() => {
      try {
        m && Se(m, b, c), D && O.enter(x), I && ze(c, null, b, "mounted");
      } finally {
      }
    }, _);
  }, _e = (c, u, p, b, _) => {
    if (p && y(c, p), b)
      for (let g = 0; g < b.length; g++)
        y(c, b[g]);
    if (_) {
      let g = _.subTree;
      if (u === g || Ji(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const C = _.vnode;
        _e(
          c,
          C,
          C.scopeId,
          C.slotScopeIds,
          _.parent
        );
      }
    }
  }, Ke = (c, u, p, b, _, g, C, S, x = 0) => {
    for (let m = x; m < c.length; m++) {
      const A = c[m] = S ? He(c[m]) : Te(c[m]);
      w(
        null,
        A,
        u,
        p,
        b,
        _,
        g,
        C,
        S
      );
    }
  }, jt = (c, u, p, b, _, g, C) => {
    const S = u.el = c.el;
    let { patchFlag: x, dynamicChildren: m, dirs: A } = u;
    x |= c.patchFlag & 16;
    const T = c.props || K, O = u.props || K;
    let I;
    if (p && Xe(p, !1), (I = O.onVnodeBeforeUpdate) && Se(I, p, u, c), A && ze(u, c, p, "beforeUpdate"), p && Xe(p, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    m && (!c.dynamicChildren || c.dynamicChildren.length !== m.length) && (x = 0, C = !1, m = null), (T.innerHTML && O.innerHTML == null || T.textContent && O.textContent == null) && a(S, ""), m ? qe(
      c.dynamicChildren,
      m,
      S,
      p,
      b,
      Ts(u, _),
      g
    ) : C || W(
      c,
      u,
      S,
      null,
      p,
      b,
      Ts(u, _),
      g,
      !1
    ), x > 0) {
      if (x & 16)
        ht(S, T, O, p, _);
      else if (x & 2 && T.class !== O.class && r(S, "class", null, O.class, _), x & 4 && r(S, "style", T.style, O.style, _), x & 8) {
        const D = u.dynamicProps;
        for (let L = 0; L < D.length; L++) {
          const U = D[L], Y = T[U], X = O[U];
          (X !== Y || U === "value") && r(S, U, Y, X, _, p);
        }
      }
      x & 1 && c.children !== u.children && a(S, u.children);
    } else !C && m == null && ht(S, T, O, p, _);
    ((I = O.onVnodeUpdated) || A) && oe(() => {
      I && Se(I, p, u, c), A && ze(u, c, p, "updated");
    }, b);
  }, qe = (c, u, p, b, _, g, C) => {
    for (let S = 0; S < u.length; S++) {
      const x = c[S], m = u[S], A = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        x.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (x.type === ae || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !yt(x, m) || // - In the case of a component, it could contain anything.
        x.shapeFlag & 198) ? h(x.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          p
        )
      );
      w(
        x,
        m,
        A,
        null,
        b,
        _,
        g,
        C,
        !0
      );
    }
  }, ht = (c, u, p, b, _) => {
    if (u !== p) {
      if (u !== K)
        for (const g in u)
          !St(g) && !(g in p) && r(
            c,
            g,
            u[g],
            null,
            _,
            b
          );
      for (const g in p) {
        if (St(g)) continue;
        const C = p[g], S = u[g];
        C !== S && g !== "value" && r(c, g, S, C, _, b);
      }
      "value" in p && r(c, "value", u.value, p.value, _);
    }
  }, Ut = (c, u, p, b, _, g, C, S, x) => {
    const m = u.el = c ? c.el : l(""), A = u.anchor = c ? c.anchor : l("");
    let { patchFlag: T, dynamicChildren: O, slotScopeIds: I } = u;
    I && (S = S ? S.concat(I) : I), c == null ? (n(m, p, b), n(A, p, b), Ke(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      p,
      A,
      _,
      g,
      C,
      S,
      x
    )) : T > 0 && T & 64 && O && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    c.dynamicChildren && c.dynamicChildren.length === O.length ? (qe(
      c.dynamicChildren,
      O,
      p,
      _,
      g,
      C,
      S
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || _ && u === _.subTree) && Wi(
      c,
      u,
      !0
      /* shallow */
    )) : W(
      c,
      u,
      p,
      A,
      _,
      g,
      C,
      S,
      x
    );
  }, $t = (c, u, p, b, _, g, C, S, x) => {
    u.slotScopeIds = S, c == null ? u.shapeFlag & 512 ? _.ctx.activate(
      u,
      p,
      b,
      C,
      x
    ) : ms(
      u,
      p,
      b,
      _,
      g,
      C,
      x
    ) : rn(c, u, x);
  }, ms = (c, u, p, b, _, g, C) => {
    const S = c.component = Jo(
      c,
      b,
      _
    );
    if (Ti(c) && (S.ctx.renderer = mt), Go(S, !1, C), S.asyncDep) {
      if (_ && _.registerDep(S, se, C), !c.el) {
        const x = S.subTree = Ae(Je);
        R(null, x, u, p), c.placeholder = x.el;
      }
    } else
      se(
        S,
        c,
        u,
        p,
        _,
        g,
        C
      );
  }, rn = (c, u, p) => {
    const b = u.component = c.component;
    if (To(c, u, p))
      if (b.asyncDep && !b.asyncResolved) {
        J(b, u, p);
        return;
      } else
        b.next = u, b.update();
    else
      u.el = c.el, b.vnode = u;
  }, se = (c, u, p, b, _, g, C) => {
    const S = () => {
      if (c.isMounted) {
        let { next: T, bu: O, u: I, parent: D, vnode: L } = c;
        {
          const ve = Bi(c);
          if (ve) {
            T && (T.el = L.el, J(c, T, C)), ve.asyncDep.then(() => {
              oe(() => {
                c.isUnmounted || m();
              }, _);
            });
            return;
          }
        }
        let U = T, Y;
        Xe(c, !1), T ? (T.el = L.el, J(c, T, C)) : T = L, O && Jt(O), (Y = T.props && T.props.onVnodeBeforeUpdate) && Se(Y, D, T, L), Xe(c, !0);
        const X = wn(c), ye = c.subTree;
        c.subTree = X, w(
          ye,
          X,
          // parent may have changed if it's in a teleport
          h(ye.el),
          // anchor may have changed if it's in a fragment
          Kt(ye),
          c,
          _,
          g
        ), T.el = X.el, U === null && Eo(c, X.el), I && oe(I, _), (Y = T.props && T.props.onVnodeUpdated) && oe(
          () => Se(Y, D, T, L),
          _
        );
      } else {
        let T;
        const { el: O, props: I } = u, { bm: D, m: L, parent: U, root: Y, type: X } = c, ye = Et(u);
        Xe(c, !1), D && Jt(D), !ye && (T = I && I.onVnodeBeforeMount) && Se(T, U, u), Xe(c, !0);
        {
          Y.ce && Y.ce._hasShadowRoot() && Y.ce._injectChildStyle(
            X,
            c.parent ? c.parent.type : void 0
          );
          const ve = c.subTree = wn(c);
          w(
            null,
            ve,
            p,
            b,
            c,
            _,
            g
          ), u.el = ve.el;
        }
        if (L && oe(L, _), !ye && (T = I && I.onVnodeMounted)) {
          const ve = u;
          oe(
            () => Se(T, U, ve),
            _
          );
        }
        (u.shapeFlag & 256 || U && Et(U.vnode) && U.vnode.shapeFlag & 256) && c.a && oe(c.a, _), c.isMounted = !0, u = p = b = null;
      }
    };
    c.scope.on();
    const x = c.effect = new ei(S);
    c.scope.off();
    const m = c.update = x.run.bind(x), A = c.job = x.runIfDirty.bind(x);
    A.i = c, A.id = c.uid, x.scheduler = () => en(A), Xe(c, !0), m();
  }, J = (c, u, p) => {
    u.component = c;
    const b = c.vnode.props;
    c.vnode = u, c.next = null, Ao(c, u.props, b, p), Ro(c, u.children, p), Me(), gn(c), Ie();
  }, W = (c, u, p, b, _, g, C, S, x = !1) => {
    const m = c && c.children, A = c ? c.shapeFlag : 0, T = u.children, { patchFlag: O, shapeFlag: I } = u;
    if (O > 0) {
      if (O & 128) {
        Lt(
          m,
          T,
          p,
          b,
          _,
          g,
          C,
          S,
          x
        );
        return;
      } else if (O & 256) {
        Ge(
          m,
          T,
          p,
          b,
          _,
          g,
          C,
          S,
          x
        );
        return;
      }
    }
    I & 8 ? (A & 16 && gt(m, _, g), T !== m && a(p, T)) : A & 16 ? I & 16 ? Lt(
      m,
      T,
      p,
      b,
      _,
      g,
      C,
      S,
      x
    ) : gt(m, _, g, !0) : (A & 8 && a(p, ""), I & 16 && Ke(
      T,
      p,
      b,
      _,
      g,
      C,
      S,
      x
    ));
  }, Ge = (c, u, p, b, _, g, C, S, x) => {
    c = c || rt, u = u || rt;
    const m = c.length, A = u.length, T = Math.min(m, A);
    let O;
    for (O = 0; O < T; O++) {
      const I = u[O] = x ? He(u[O]) : Te(u[O]);
      w(
        c[O],
        I,
        p,
        null,
        _,
        g,
        C,
        S,
        x
      );
    }
    m > A ? gt(
      c,
      _,
      g,
      !0,
      !1,
      T
    ) : Ke(
      u,
      p,
      b,
      _,
      g,
      C,
      S,
      x,
      T
    );
  }, Lt = (c, u, p, b, _, g, C, S, x) => {
    let m = 0;
    const A = u.length;
    let T = c.length - 1, O = A - 1;
    for (; m <= T && m <= O; ) {
      const I = c[m], D = u[m] = x ? He(u[m]) : Te(u[m]);
      if (yt(I, D))
        w(
          I,
          D,
          p,
          null,
          _,
          g,
          C,
          S,
          x
        );
      else
        break;
      m++;
    }
    for (; m <= T && m <= O; ) {
      const I = c[T], D = u[O] = x ? He(u[O]) : Te(u[O]);
      if (yt(I, D))
        w(
          I,
          D,
          p,
          null,
          _,
          g,
          C,
          S,
          x
        );
      else
        break;
      T--, O--;
    }
    if (m > T) {
      if (m <= O) {
        const I = O + 1, D = I < A ? u[I].el : b;
        for (; m <= O; )
          w(
            null,
            u[m] = x ? He(u[m]) : Te(u[m]),
            p,
            D,
            _,
            g,
            C,
            S,
            x
          ), m++;
      }
    } else if (m > O)
      for (; m <= T; )
        be(c[m], _, g, !0), m++;
    else {
      const I = m, D = m, L = /* @__PURE__ */ new Map();
      for (m = D; m <= O; m++) {
        const le = u[m] = x ? He(u[m]) : Te(u[m]);
        le.key != null && L.set(le.key, m);
      }
      let U, Y = 0;
      const X = O - D + 1;
      let ye = !1, ve = 0;
      const _t = new Array(X);
      for (m = 0; m < X; m++) _t[m] = 0;
      for (m = I; m <= T; m++) {
        const le = c[m];
        if (Y >= X) {
          be(le, _, g, !0);
          continue;
        }
        let xe;
        if (le.key != null)
          xe = L.get(le.key);
        else
          for (U = D; U <= O; U++)
            if (_t[U - D] === 0 && yt(le, u[U])) {
              xe = U;
              break;
            }
        xe === void 0 ? be(le, _, g, !0) : (_t[xe - D] = m + 1, xe >= ve ? ve = xe : ye = !0, w(
          le,
          u[xe],
          p,
          null,
          _,
          g,
          C,
          S,
          x
        ), Y++);
      }
      const cn = ye ? Ho(_t) : rt;
      for (U = cn.length - 1, m = X - 1; m >= 0; m--) {
        const le = D + m, xe = u[le], fn = u[le + 1], un = le + 1 < A ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          fn.el || ki(fn)
        ) : b;
        _t[m] === 0 ? w(
          null,
          xe,
          p,
          un,
          _,
          g,
          C,
          S,
          x
        ) : ye && (U < 0 || m !== cn[U] ? Ye(xe, p, un, 2) : U--);
      }
    }
  }, Ye = (c, u, p, b, _ = null) => {
    const { el: g, type: C, transition: S, children: x, shapeFlag: m } = c;
    if (m & 6) {
      Ye(c.component.subTree, u, p, b);
      return;
    }
    if (m & 128) {
      c.suspense.move(u, p, b);
      return;
    }
    if (m & 64) {
      C.move(c, u, p, mt);
      return;
    }
    if (C === ae) {
      n(g, u, p);
      for (let T = 0; T < x.length; T++)
        Ye(x[T], u, p, b);
      n(c.anchor, u, p);
      return;
    }
    if (C === Es) {
      k(c, u, p);
      return;
    }
    if (b !== 2 && m & 1 && S)
      if (b === 0)
        S.persisted && !g[ws] ? n(g, u, p) : (S.beforeEnter(g), n(g, u, p), oe(() => S.enter(g), _));
      else {
        const { leave: T, delayLeave: O, afterLeave: I } = S, D = () => {
          c.ctx.isUnmounted ? i(g) : n(g, u, p);
        }, L = () => {
          const U = g._isLeaving || !!g[ws];
          g._isLeaving && g[ws](
            !0
            /* cancelled */
          ), S.persisted && !U ? D() : T(g, () => {
            D(), I && I();
          });
        };
        O ? O(g, D, L) : L();
      }
    else
      n(g, u, p);
  }, be = (c, u, p, b = !1, _ = !1) => {
    const {
      type: g,
      props: C,
      ref: S,
      children: x,
      dynamicChildren: m,
      shapeFlag: A,
      patchFlag: T,
      dirs: O,
      cacheIndex: I,
      memo: D
    } = c;
    if (T === -2 && (_ = !1), S != null && (Me(), Tt(S, null, p, c, !0), Ie()), I != null && (u.renderCache[I] = void 0), A & 256) {
      u.ctx.deactivate(c);
      return;
    }
    const L = A & 1 && O, U = !Et(c);
    let Y;
    if (U && (Y = C && C.onVnodeBeforeUnmount) && Se(Y, u, c), A & 6)
      nr(c.component, p, b);
    else {
      if (A & 128) {
        c.suspense.unmount(p, b);
        return;
      }
      L && ze(c, null, u, "beforeUnmount"), A & 64 ? c.type.remove(
        c,
        u,
        p,
        mt,
        b
      ) : m && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !m.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== ae || T > 0 && T & 64) ? gt(
        m,
        u,
        p,
        !1,
        !0
      ) : (g === ae && T & 384 || !_ && A & 16) && gt(x, u, p), b && on(c);
    }
    const X = D != null && I == null;
    (U && (Y = C && C.onVnodeUnmounted) || L || X) && oe(() => {
      Y && Se(Y, u, c), L && ze(c, null, u, "unmounted"), X && (c.el = null);
    }, p);
  }, on = (c) => {
    const { type: u, el: p, anchor: b, transition: _ } = c;
    if (u === ae) {
      sr(p, b);
      return;
    }
    if (u === Es) {
      M(c);
      return;
    }
    const g = () => {
      i(p), _ && !_.persisted && _.afterLeave && _.afterLeave();
    };
    if (c.shapeFlag & 1 && _ && !_.persisted) {
      const { leave: C, delayLeave: S } = _, x = () => C(p, g);
      S ? S(c.el, g, x) : x();
    } else
      g();
  }, sr = (c, u) => {
    let p;
    for (; c !== u; )
      p = E(c), i(c), c = p;
    i(u);
  }, nr = (c, u, p) => {
    const { bum: b, scope: _, job: g, subTree: C, um: S, m: x, a: m } = c;
    En(x), En(m), b && Jt(b), _.stop(), g && (g.flags |= 8, be(C, c, u, p)), S && oe(S, u), oe(() => {
      c.isUnmounted = !0;
    }, u);
  }, gt = (c, u, p, b = !1, _ = !1, g = 0) => {
    for (let C = g; C < c.length; C++)
      be(c[C], u, p, b, _);
  }, Kt = (c) => {
    if (c.shapeFlag & 6)
      return Kt(c.component.subTree);
    if (c.shapeFlag & 128)
      return c.suspense.next();
    const u = E(c.anchor || c.el), p = u && u[Zr];
    return p ? E(p) : u;
  };
  let _s = !1;
  const ln = (c, u, p) => {
    let b;
    c == null ? u._vnode && (be(u._vnode, null, null, !0), b = u._vnode.component) : w(
      u._vnode || null,
      c,
      u,
      null,
      null,
      null,
      p
    ), u._vnode = c, _s || (_s = !0, gn(b), bi(), _s = !1);
  }, mt = {
    p: w,
    um: be,
    m: Ye,
    r: on,
    mt: ms,
    mc: Ke,
    pc: W,
    pbc: qe,
    n: Kt,
    o: e
  };
  return {
    render: ln,
    hydrate: void 0,
    createApp: yo(ln)
  };
}
function Ts({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function Xe({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Do(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Wi(e, t, s = !1) {
  const n = e.children, i = t.children;
  if (P(n) && P(i))
    for (let r = 0; r < n.length; r++) {
      const o = n[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = He(i[r]), l.el = o.el), !s && l.patchFlag !== -2 && Wi(o, l)), l.type === hs && (l.patchFlag === -1 && (l = i[r] = He(l)), l.el = o.el), l.type === Je && !l.el && (l.el = o.el);
    }
}
function Ho(e) {
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
function En(e) {
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
const Ji = (e) => e.__isSuspense;
function No(e, t) {
  t && t.pendingBranch ? P(e) ? t.effects.push(...e) : t.effects.push(e) : Jr(e);
}
const ae = /* @__PURE__ */ Symbol.for("v-fgt"), hs = /* @__PURE__ */ Symbol.for("v-txt"), Je = /* @__PURE__ */ Symbol.for("v-cmt"), Es = /* @__PURE__ */ Symbol.for("v-stc"), At = [];
let ce = null;
function De(e = !1) {
  At.push(ce = e ? null : []);
}
function jo() {
  At.pop(), ce = At[At.length - 1] || null;
}
let Rt = 1;
function On(e, t = !1) {
  Rt += e, e < 0 && ce && t && (ce.hasOnce = !0);
}
function qi(e) {
  return e.dynamicChildren = Rt > 0 ? ce || rt : null, jo(), Rt > 0 && ce && ce.push(e), e;
}
function Be(e, t, s, n, i, r) {
  return qi(
    V(
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
function Uo(e, t, s, n, i) {
  return qi(
    Ae(
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
const Yi = ({ key: e }) => e ?? null, Yt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? G(e) || /* @__PURE__ */ te(e) || F(e) ? { i: fe, r: e, k: t, f: !!s } : e : null);
function V(e, t = null, s = null, n = 0, i = null, r = e === ae ? 0 : 1, o = !1, l = !1) {
  const f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Yi(t),
    ref: t && Yt(t),
    scopeId: vi,
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
    ctx: fe
  };
  return l ? (ss(f, s), r & 128 && e.normalize(f)) : s && (f.shapeFlag |= G(s) ? 8 : 16), Rt > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  ce && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (f.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  f.patchFlag !== 32 && ce.push(f), f;
}
const Ae = $o;
function $o(e, t = null, s = null, n = 0, i = null, r = !1) {
  if ((!e || e === uo) && (e = Je), Gi(e)) {
    const l = ut(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ss(l, s), Rt > 0 && !r && ce && (l.shapeFlag & 6 ? ce[ce.indexOf(e)] = l : ce.push(l)), l.patchFlag = -2, l;
  }
  if (Zo(e) && (e = e.__vccOpts), t) {
    t = Lo(t);
    let { class: l, style: f } = t;
    l && !G(l) && (t.class = ks(l)), $(f) && (/* @__PURE__ */ Qs(f) && !P(f) && (f = Q({}, f)), t.style = Bs(f));
  }
  const o = G(e) ? 1 : Ji(e) ? 128 : Qr(e) ? 64 : $(e) ? 4 : F(e) ? 2 : 0;
  return V(
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
  return e ? /* @__PURE__ */ Qs(e) || Ni(e) ? Q({}, e) : e : null;
}
function ut(e, t, s = !1, n = !1) {
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
      s && r ? P(r) ? r.concat(Yt(t)) : [r, Yt(t)] : Yt(t)
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
    patchFlag: t && e.type !== ae ? o === -1 ? 16 : o | 16 : o,
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
    ssContent: e.ssContent && ut(e.ssContent),
    ssFallback: e.ssFallback && ut(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return f && n && tn(
    a,
    f.clone(a)
  ), a;
}
function Ko(e = " ", t = 0) {
  return Ae(hs, null, e, t);
}
function zi(e = "", t = !1) {
  return t ? (De(), Uo(Je, null, e)) : Ae(Je, null, e);
}
function Te(e) {
  return e == null || typeof e == "boolean" ? Ae(Je) : P(e) ? Ae(
    ae,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Gi(e) ? He(e) : Ae(hs, null, String(e));
}
function He(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : ut(e);
}
function ss(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (P(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), ss(e, i()), i._c && (i._d = !0));
      return;
    } else {
      s = 32;
      const i = t._;
      !i && !Ni(t) ? t._ctx = fe : i === 3 && fe && (fe.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (F(t)) {
    if (n & 65) {
      ss(e, { default: t });
      return;
    }
    t = { default: t, _ctx: fe }, s = 32;
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
        t.class !== n.class && (t.class = ks([t.class, n.class]));
      else if (i === "style")
        t.style = Bs([t.style, n.style]);
      else if (is(i)) {
        const r = t[i], o = n[i];
        o && r !== o && !(P(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !rs(i) && (t[i] = o);
      } else i !== "" && (t[i] = n[i]);
  }
  return t;
}
function Se(e, t, s, n = null) {
  ge(e, t, 7, [
    s,
    n
  ]);
}
const Bo = Ri();
let ko = 0;
function Jo(e, t, s) {
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
    scope: new gr(
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
    propsOptions: Ui(n, i),
    emitsOptions: Fi(n, i),
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
  return r.ctx = { _: r }, r.root = t ? t.root : r, r.emit = xo.bind(null, r), e.ce && e.ce(r), r;
}
let re = null;
const qo = () => re || fe;
let ns, Us;
{
  const e = cs(), t = (s, n) => {
    let i;
    return (i = e[s]) || (i = e[s] = []), i.push(n), (r) => {
      i.length > 1 ? i.forEach((o) => o(r)) : i[0](r);
    };
  };
  ns = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => re = s
  ), Us = t(
    "__VUE_SSR_SETTERS__",
    (s) => Ft = s
  );
}
const Nt = (e) => {
  const t = re;
  return ns(e), e.scope.on(), () => {
    e.scope.off(), ns(t);
  };
}, An = () => {
  re && re.scope.off(), ns(null);
};
function Xi(e) {
  return e.vnode.shapeFlag & 4;
}
let Ft = !1;
function Go(e, t = !1, s = !1) {
  t && Us(t);
  const { props: n, children: i } = e.vnode, r = Xi(e);
  Oo(e, n, r, t), Io(e, i, s || t);
  const o = r ? Yo(e, t) : void 0;
  return t && Us(!1), o;
}
function Yo(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, ao);
  const { setup: n } = s;
  if (n) {
    Me();
    const i = e.setupContext = n.length > 1 ? Xo(e) : null, r = Nt(e), o = Ht(
      n,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = Jn(o);
    if (Ie(), r(), (l || e.sp) && !Et(e) && Ci(e), l) {
      if (o.then(An, An), t)
        return o.then((f) => {
          Pn(e, f);
        }).catch((f) => {
          as(f, e, 0);
        });
      e.asyncDep = o;
    } else
      Pn(e, o);
  } else
    Zi(e);
}
function Pn(e, t, s) {
  F(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : $(t) && (e.setupState = hi(t)), Zi(e);
}
function Zi(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Oe);
  {
    const i = Nt(e);
    Me();
    try {
      po(e);
    } finally {
      Ie(), i();
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
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(hi(Dr(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in Ot)
        return Ot[s](e);
    },
    has(t, s) {
      return s in t || s in Ot;
    }
  })) : e.proxy;
}
function Zo(e) {
  return F(e) && "__vccOpts" in e;
}
const Qo = (e, t) => /* @__PURE__ */ Lr(e, t, Ft), el = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let $s;
const Mn = typeof window < "u" && window.trustedTypes;
if (Mn)
  try {
    $s = /* @__PURE__ */ Mn.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Qi = $s ? (e) => $s.createHTML(e) : (e) => e, tl = "http://www.w3.org/2000/svg", sl = "http://www.w3.org/1998/Math/MathML", Ve = typeof document < "u" ? document : null, In = Ve && /* @__PURE__ */ Ve.createElement("template"), nl = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const i = t === "svg" ? Ve.createElementNS(tl, e) : t === "mathml" ? Ve.createElementNS(sl, e) : s ? Ve.createElement(e, { is: s }) : Ve.createElement(e);
    return e === "select" && n && n.multiple != null && i.setAttribute("multiple", n.multiple), i;
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
  insertStaticContent(e, t, s, n, i, r) {
    const o = s ? s.previousSibling : t.lastChild;
    if (i && (i === r || i.nextSibling))
      for (; t.insertBefore(i.cloneNode(!0), s), !(i === r || !(i = i.nextSibling)); )
        ;
    else {
      In.innerHTML = Qi(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = In.content;
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
const Rn = /* @__PURE__ */ Symbol("_vod"), ol = /* @__PURE__ */ Symbol("_vsh"), ll = /* @__PURE__ */ Symbol(""), cl = /(?:^|;)\s*display\s*:/;
function fl(e, t, s) {
  const n = e.style, i = G(s);
  let r = !1;
  if (s && !i) {
    if (t)
      if (G(t))
        for (const o of t.split(";")) {
          const l = o.slice(0, o.indexOf(":")).trim();
          s[l] == null && xt(n, l, "");
        }
      else
        for (const o in t)
          s[o] == null && xt(n, o, "");
    for (const o in s) {
      o === "display" && (r = !0);
      const l = s[o];
      l != null ? al(
        e,
        o,
        !G(t) && t ? t[o] : void 0,
        l
      ) || xt(n, o, l) : xt(n, o, "");
    }
  } else if (i) {
    if (t !== s) {
      const o = n[ll];
      o && (s += ";" + o), n.cssText = s, r = cl.test(s);
    }
  } else t && e.removeAttribute("style");
  Rn in e && (e[Rn] = r ? n.display : "", e[ol] && (n.display = "none"));
}
const Fn = /\s*!important$/;
function xt(e, t, s) {
  if (P(s))
    s.forEach((n) => xt(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = ul(e, t);
    Fn.test(s) ? e.setProperty(
      st(n),
      s.replace(Fn, ""),
      "important"
    ) : e[n] = s;
  }
}
const Vn = ["Webkit", "Moz", "ms"], Os = {};
function ul(e, t) {
  const s = Os[t];
  if (s)
    return s;
  let n = de(t);
  if (n !== "filter" && n in e)
    return Os[t] = n;
  n = Yn(n);
  for (let i = 0; i < Vn.length; i++) {
    const r = Vn[i] + n;
    if (r in e)
      return Os[t] = r;
  }
  return t;
}
function al(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && G(n) && s === n;
}
const Dn = "http://www.w3.org/1999/xlink";
function Hn(e, t, s, n, i, r = pr(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Dn, t.slice(6, t.length)) : e.setAttributeNS(Dn, t, s) : s == null || r && !Xn(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : Pe(s) ? String(s) : s
  );
}
function Nn(e, t, s, n, i) {
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
    l === "boolean" ? s = Xn(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(i || t);
}
function ke(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function dl(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const jn = /* @__PURE__ */ Symbol("_vei");
function pl(e, t, s, n, i = null) {
  const r = e[jn] || (e[jn] = {}), o = r[t];
  if (n && o)
    o.value = n;
  else {
    const [l, f] = ml(t);
    if (n) {
      const d = r[t] = yl(
        n,
        i
      );
      ke(e, l, d, f);
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
let As = 0;
const _l = /* @__PURE__ */ Promise.resolve(), bl = () => As || (_l.then(() => As = 0), As = Date.now());
function yl(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const i = s.value;
    if (P(i)) {
      const r = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        r.call(n), n._stopped = !0;
      };
      const o = i.slice(), l = [n];
      for (let f = 0; f < o.length && !n._stopped; f++) {
        const d = o[f];
        d && ge(
          d,
          t,
          5,
          l
        );
      }
    } else
      ge(
        i,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = bl(), s;
}
const Un = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, vl = (e, t, s, n, i, r) => {
  const o = i === "svg";
  t === "class" ? rl(e, n, o) : t === "style" ? fl(e, s, n) : is(t) ? rs(t) || pl(e, t, s, n, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : xl(e, t, n, o)) ? (Nn(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && Hn(e, t, n, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (Sl(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !G(n))) ? Nn(e, de(t), n, r, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), Hn(e, t, n, o));
};
function xl(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Un(t) && F(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return Un(t) && G(s) ? !1 : t in e;
}
function Sl(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = de(t);
  return Array.isArray(s) ? s.some((i) => de(i) === n) : Object.keys(s).some((i) => de(i) === n);
}
const at = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return P(t) ? (s) => Jt(t, s) : t;
};
function wl(e) {
  e.target.composing = !0;
}
function $n(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const Ue = /* @__PURE__ */ Symbol("_assign");
function Ln(e, t, s) {
  return t && (e = e.trim()), s && (e = ls(e)), e;
}
const Ze = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, i) {
    e[Ue] = at(i);
    const r = n || i.props && i.props.type === "number";
    ke(e, t ? "change" : "input", (o) => {
      o.target.composing || e[Ue](Ln(e.value, s, r));
    }), (s || r) && ke(e, "change", () => {
      e.value = Ln(e.value, s, r);
    }), t || (ke(e, "compositionstart", wl), ke(e, "compositionend", $n), ke(e, "change", $n));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: i, number: r } }, o) {
    if (e[Ue] = at(o), e.composing) return;
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
    e[Ue] = at(s), ke(e, "change", () => {
      const n = e._modelValue, i = Vt(e), r = e.checked, o = e[Ue];
      if (P(n)) {
        const l = Js(n, i), f = l !== -1;
        if (r && !f)
          o(n.concat(i));
        else if (!r && f) {
          const d = [...n];
          d.splice(l, 1), o(d);
        }
      } else if (dt(n)) {
        const l = new Set(n);
        r ? l.add(i) : l.delete(i), o(l);
      } else
        o(er(e, r));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: Kn,
  beforeUpdate(e, t, s) {
    e[Ue] = at(s), Kn(e, t, s);
  }
};
function Kn(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let i;
  if (P(t))
    i = Js(t, n.props.value) > -1;
  else if (dt(t))
    i = t.has(n.props.value);
  else {
    if (t === s) return;
    i = pt(t, er(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const Tl = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const i = dt(t);
    ke(e, "change", () => {
      const r = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? ls(Vt(o)) : Vt(o)
      );
      e[Ue](
        e.multiple ? i ? new Set(r) : r : r[0]
      ), e._assigning = !0, mi(() => {
        e._assigning = !1;
      });
    }), e[Ue] = at(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    Wn(e, t);
  },
  beforeUpdate(e, t, s) {
    e[Ue] = at(s);
  },
  updated(e, { value: t }) {
    e._assigning || Wn(e, t);
  }
};
function Wn(e, t) {
  const s = e.multiple, n = P(t);
  if (!(s && !n && !dt(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = Vt(o);
      if (s)
        if (n) {
          const f = typeof l;
          f === "string" || f === "number" ? o.selected = t.some((d) => String(d) === String(l)) : o.selected = Js(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (pt(Vt(o), t)) {
        e.selectedIndex !== i && (e.selectedIndex = i);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function Vt(e) {
  return "_value" in e ? e._value : e.value;
}
function er(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
}
const El = /* @__PURE__ */ Q({ patchProp: vl }, nl);
let Bn;
function Ol() {
  return Bn || (Bn = Fo(El));
}
const Al = ((...e) => {
  const t = Ol().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const i = Ml(n);
    if (!i) return;
    const r = t._component;
    !F(r) && !r.render && !r.template && (r.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
    const o = s(i, !1, Pl(i));
    return i instanceof Element && (i.removeAttribute("v-cloak"), i.setAttribute("data-v-app", "")), o;
  }, t;
});
function Pl(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function Ml(e) {
  return G(e) ? document.querySelector(e) : e;
}
const Il = ["value"], Rl = /* @__PURE__ */ wi({
  __name: "CredentialSlot",
  props: {
    api: {},
    modelValue: {}
  },
  emits: ["update:modelValue"],
  setup(e, { emit: t }) {
    const s = e, n = t, i = /* @__PURE__ */ zt(null), r = /* @__PURE__ */ zt(!1);
    let o = null;
    return Oi(() => {
      !i.value || !s.api.mountCredentialField || (o = s.api.mountCredentialField(i.value, {
        value: s.modelValue,
        noneLabel: "(none — use fields below)",
        onChange: (l) => n("update:modelValue", l)
      }), r.value = !0);
    }), Gt(() => s.modelValue, (l) => o == null ? void 0 : o.setValue(l)), Ai(() => o == null ? void 0 : o.destroy()), (l, f) => (De(), Be("div", {
      ref_key: "el",
      ref: i,
      class: "cred-slot"
    }, [
      r.value ? zi("", !0) : (De(), Be("input", {
        key: 0,
        value: e.modelValue,
        class: "w-260",
        spellcheck: "false",
        placeholder: "secret:<scope>:<entry>",
        onInput: f[0] || (f[0] = (d) => n("update:modelValue", d.target.value))
      }, null, 40, Il))
    ], 512));
  }
}), tr = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, i] of t)
    s[n] = i;
  return s;
}, Fl = /* @__PURE__ */ tr(Rl, [["__scopeId", "data-v-c6a4a9b8"]]), Vl = { class: "ssh-set" }, Dl = { class: "row" }, Hl = ["value"], Nl = {
  key: 0,
  class: "muted empty"
}, jl = { class: "row spread" }, Ul = { class: "row" }, $l = ["onUpdate:modelValue"], Ll = ["onUpdate:modelValue"], Kl = ["onUpdate:modelValue"], Wl = ["onClick"], Bl = { class: "row" }, kl = { class: "row" }, Jl = ["onUpdate:modelValue", "placeholder"], ql = ["onUpdate:modelValue"], Gl = { class: "row" }, Yl = ["onUpdate:modelValue"], zl = { class: "row" }, Xl = { class: "chk" }, Zl = ["onUpdate:modelValue"], Ql = { class: "row" }, ec = ["disabled", "onClick"], tc = { class: "muted" }, sc = /* @__PURE__ */ wi({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function i(y, v) {
      return {
        key: n++,
        name: y,
        host: v.host || "",
        port: v.port || 22,
        user: v.user || "",
        credential: v.credential || "",
        password: v.password || "",
        keyFile: v.key_file || "",
        keyPassphrase: v.key_passphrase || "",
        description: v.description || "",
        allowWrite: !!v.allow_write,
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
        return JSON.parse(s.api.getJson() || "null") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ zt(o.default_host || ""), f = /* @__PURE__ */ zt(o.timeout_seconds || 20), d = /* @__PURE__ */ us(
      Object.entries(o.hosts || {}).map(([y, v]) => i(y, v))
    );
    function a() {
      d.push(i(`host${d.length + 1}`, {}));
    }
    async function h(y) {
      y.testing = !0, y.testStatus = "Connecting…";
      try {
        const v = await s.api.invoke("plugin.action", {
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
        if (v.ok && v.resultJson) {
          const w = JSON.parse(v.resultJson);
          y.testStatus = w.message;
        } else
          y.testStatus = "Failed: " + (v.error || "unknown error");
      } catch (v) {
        y.testStatus = "Failed: " + (v instanceof Error ? v.message : String(v));
      } finally {
        y.testing = !1;
      }
    }
    function E() {
      const y = {
        default_host: l.value || void 0,
        timeout_seconds: f.value || 20,
        hosts: Object.fromEntries(
          d.filter((v) => v.name.trim()).map((v) => [v.name.trim(), r(v)])
        )
      };
      return JSON.stringify(y);
    }
    return t({ toJson: E }), (y, v) => (De(), Be("div", Vl, [
      v[14] || (v[14] = V("div", { class: "muted" }, " Named SSH hosts available to the agent and the terminal. Passwords/keys live in the secret store (Settings → Secrets); a host only references an entry by name. ", -1)),
      V("div", Dl, [
        V("label", null, [
          v[3] || (v[3] = V("span", { class: "muted" }, "Default host", -1)),
          Fe(V("select", {
            "onUpdate:modelValue": v[0] || (v[0] = (w) => l.value = w)
          }, [
            v[2] || (v[2] = V("option", { value: "" }, "(none)", -1)),
            (De(!0), Be(ae, null, bn(d, (w) => (De(), Be("option", {
              key: w.key,
              value: w.name
            }, Ps(w.name), 9, Hl))), 128))
          ], 512), [
            [Tl, l.value]
          ])
        ]),
        V("label", null, [
          v[4] || (v[4] = V("span", { class: "muted" }, "Timeout, s", -1)),
          Fe(V("input", {
            "onUpdate:modelValue": v[1] || (v[1] = (w) => f.value = w),
            type: "number",
            min: "5",
            max: "120",
            class: "w-70"
          }, null, 512), [
            [
              Ze,
              f.value,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      V("button", {
        type: "button",
        class: "self-start",
        onClick: a
      }, "+ Add host"),
      d.length ? zi("", !0) : (De(), Be("div", Nl, 'No hosts yet. Click "+ Add host".')),
      (De(!0), Be(ae, null, bn(d, (w, q) => (De(), Be("div", {
        key: w.key,
        class: "host-card"
      }, [
        V("div", jl, [
          V("div", Ul, [
            v[5] || (v[5] = V("span", { class: "muted" }, "Name", -1)),
            Fe(V("input", {
              "onUpdate:modelValue": (R) => w.name = R,
              class: "w-120",
              spellcheck: "false"
            }, null, 8, $l), [
              [Ze, w.name]
            ]),
            v[6] || (v[6] = V("span", { class: "muted" }, "Host", -1)),
            Fe(V("input", {
              "onUpdate:modelValue": (R) => w.host = R,
              placeholder: "10.0.0.5 or box.local",
              class: "w-180",
              spellcheck: "false"
            }, null, 8, Ll), [
              [Ze, w.host]
            ]),
            v[7] || (v[7] = V("span", { class: "muted" }, "Port", -1)),
            Fe(V("input", {
              "onUpdate:modelValue": (R) => w.port = R,
              type: "number",
              min: "1",
              max: "65535",
              class: "w-70"
            }, null, 8, Kl), [
              [
                Ze,
                w.port,
                void 0,
                { number: !0 }
              ]
            ])
          ]),
          V("button", {
            type: "button",
            onClick: (R) => d.splice(q, 1)
          }, "✕ Remove", 8, Wl)
        ]),
        V("div", Bl, [
          v[8] || (v[8] = V("span", { class: "muted w-label" }, "Credential", -1)),
          Ae(Fl, {
            api: e.api,
            modelValue: w.credential,
            "onUpdate:modelValue": (R) => w.credential = R
          }, null, 8, ["api", "modelValue", "onUpdate:modelValue"])
        ]),
        V("div", kl, [
          v[9] || (v[9] = V("span", { class: "muted w-label" }, "User", -1)),
          Fe(V("input", {
            "onUpdate:modelValue": (R) => w.user = R,
            placeholder: w.credential ? "(from credential)" : "login",
            class: "w-120",
            spellcheck: "false"
          }, null, 8, Jl), [
            [Ze, w.user]
          ]),
          v[10] || (v[10] = V("span", { class: "muted" }, "Key file", -1)),
          Fe(V("input", {
            "onUpdate:modelValue": (R) => w.keyFile = R,
            placeholder: "optional: C:\\Users\\me\\.ssh\\id_ed25519",
            class: "w-260",
            spellcheck: "false"
          }, null, 8, ql), [
            [Ze, w.keyFile]
          ])
        ]),
        V("div", Gl, [
          v[11] || (v[11] = V("span", { class: "muted w-label" }, "Description", -1)),
          Fe(V("input", {
            "onUpdate:modelValue": (R) => w.description = R,
            placeholder: "Shown to the AI — what this host is",
            class: "grow"
          }, null, 8, Yl), [
            [Ze, w.description]
          ])
        ]),
        V("div", zl, [
          V("label", Xl, [
            Fe(V("input", {
              "onUpdate:modelValue": (R) => w.allowWrite = R,
              type: "checkbox"
            }, null, 8, Zl), [
              [Cl, w.allowWrite]
            ]),
            v[12] || (v[12] = V("span", null, "Allow the agent to write (apt, systemctl, edit files…)", -1))
          ]),
          v[13] || (v[13] = V("span", { class: "muted" }, "off = read-only guard blocks mutating commands; human terminal is never guarded", -1))
        ]),
        V("div", Ql, [
          V("button", {
            type: "button",
            disabled: w.testing,
            onClick: (R) => h(w)
          }, "Test connection", 8, ec),
          V("span", tc, Ps(w.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
}), nc = /* @__PURE__ */ tr(sc, [["__scopeId", "data-v-f12a45b8"]]);
function rc(e, t) {
  let s = Al(nc, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toJson(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  rc as mount
};
