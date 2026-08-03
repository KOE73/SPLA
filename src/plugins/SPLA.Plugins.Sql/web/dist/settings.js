(function(){"use strict";try{if(typeof document<"u"){var a=document.createElement("style");a.appendChild(document.createTextNode(".cred-slot[data-v-c6a4a9b8]{min-width:0;flex:1}.w-260[data-v-c6a4a9b8]{width:260px}.sql-set[data-v-c94a9009]{display:flex;flex-direction:column;gap:8px;font-size:var(--fs-sm, 12px);color:var(--text, inherit)}.muted[data-v-c94a9009]{color:var(--muted, #888)}.empty[data-v-c94a9009]{font-style:italic}.row[data-v-c94a9009]{display:flex;gap:8px;align-items:center;flex-wrap:wrap}.row.spread[data-v-c94a9009]{justify-content:space-between}.self-start[data-v-c94a9009]{align-self:flex-start}.grow[data-v-c94a9009]{flex:1}.w-70[data-v-c94a9009]{width:70px}.w-90[data-v-c94a9009]{width:90px}.w-120[data-v-c94a9009]{width:120px}.w-130[data-v-c94a9009]{width:130px}.w-140[data-v-c94a9009]{width:140px}.w-160[data-v-c94a9009]{width:160px}.w-220[data-v-c94a9009]{width:220px}.w-400[data-v-c94a9009]{width:400px}.conn-card[data-v-c94a9009]{border:1px solid var(--border, #444);border-radius:var(--radius, 6px);padding:8px 10px;display:flex;flex-direction:column;gap:6px;background:var(--panel, transparent)}.chk[data-v-c94a9009]{cursor:pointer}.chk input[data-v-c94a9009]{height:auto}label[data-v-c94a9009]{display:flex;gap:6px;align-items:center}input[data-v-c94a9009],select[data-v-c94a9009]{height:24px;padding:2px 6px;color:var(--text, inherit);background:var(--bg, transparent);border:1px solid var(--border, #444);border-radius:5px;font-family:inherit;font-size:inherit}button[data-v-c94a9009]{padding:2px 10px;color:var(--text, inherit);background:var(--panel, transparent);border:1px solid var(--border, #444);border-radius:5px;cursor:pointer;font-size:inherit}button[data-v-c94a9009]:hover:not(:disabled){border-color:var(--muted, #888)}button[data-v-c94a9009]:disabled{opacity:.5;cursor:default}")),document.head.appendChild(a)}}catch(t){console.error("vite-plugin-css-injected-by-js",t)}})();
/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Ks(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const K = {}, rt = [], Me = () => {
}, Jn = () => !1, rs = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), os = (e) => e.startsWith("onUpdate:"), Z = Object.assign, Ws = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, rr = Object.prototype.hasOwnProperty, U = (e, t) => rr.call(e, t), I = Array.isArray, ot = (e) => Nt(e) === "[object Map]", dt = (e) => Nt(e) === "[object Set]", dn = (e) => Nt(e) === "[object Date]", D = (e) => typeof e == "function", G = (e) => typeof e == "string", Re = (e) => typeof e == "symbol", $ = (e) => e !== null && typeof e == "object", Gn = (e) => ($(e) || D(e)) && D(e.then) && D(e.catch), Yn = Object.prototype.toString, Nt = (e) => Yn.call(e), or = (e) => Nt(e).slice(8, -1), zn = (e) => Nt(e) === "[object Object]", Bs = (e) => G(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, wt = /* @__PURE__ */ Ks(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), ls = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, lr = /-\w/g, he = ls(
  (e) => e.replace(lr, (t) => t.slice(1).toUpperCase())
), cr = /\B([A-Z])/g, st = ls(
  (e) => e.replace(cr, "-$1").toLowerCase()
), Xn = ls((e) => e.charAt(0).toUpperCase() + e.slice(1)), vs = ls(
  (e) => e ? `on${Xn(e)}` : ""
), Pe = (e, t) => !Object.is(e, t), Jt = (e, ...t) => {
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
let pn;
const fs = () => pn || (pn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function qs(e) {
  if (I(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], i = G(n) ? dr(n) : qs(n);
      if (i)
        for (const r in i)
          t[r] = i[r];
    }
    return t;
  } else if (G(e) || $(e))
    return e;
}
const fr = /;(?![^(]*\))/g, ur = /:([^]+)/, ar = /\/\*[^]*?\*\//g;
function dr(e) {
  const t = {};
  return e.replace(ar, "").split(fr).forEach((s) => {
    if (s) {
      const n = s.split(ur);
      n.length > 1 && (t[n[0].trim()] = n[1].trim());
    }
  }), t;
}
function ks(e) {
  let t = "";
  if (G(e))
    t = e;
  else if (I(e))
    for (let s = 0; s < e.length; s++) {
      const n = ks(e[s]);
      n && (t += n + " ");
    }
  else if ($(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const pr = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", hr = /* @__PURE__ */ Ks(pr);
function Zn(e) {
  return !!e || e === "";
}
function gr(e, t) {
  if (e.length !== t.length) return !1;
  let s = !0;
  for (let n = 0; s && n < e.length; n++)
    s = pt(e[n], t[n]);
  return s;
}
function pt(e, t) {
  if (e === t) return !0;
  let s = dn(e), n = dn(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Re(e), n = Re(t), s || n)
    return e === t;
  if (s = I(e), n = I(t), s || n)
    return s && n ? gr(e, t) : !1;
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
const ei = (e) => !!(e && e.__v_isRef === !0), Ms = (e) => G(e) ? e : e == null ? "" : I(e) || $(e) && (e.toString === Yn || !D(e.toString)) ? ei(e) ? Ms(e.value) : JSON.stringify(e, ti, 2) : String(e), ti = (e, t) => ei(t) ? ti(e, t.value) : ot(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, i], r) => (s[ys(n, r) + " =>"] = i, s),
    {}
  )
} : dt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => ys(s))
} : Re(t) ? ys(t) : $(t) && !I(t) && !zn(t) ? String(t) : t, ys = (e, t = "") => {
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
class mr {
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
function _r() {
  return Q;
}
let q;
const xs = /* @__PURE__ */ new WeakSet();
class si {
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
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || ii(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, hn(this), ri(this);
    const t = q, s = ge;
    q = this, ge = !0;
    try {
      return this.fn();
    } finally {
      oi(this), q = t, ge = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        zs(t);
      this.deps = this.depsTail = void 0, hn(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? xs.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Is(this) && this.run();
  }
  get dirty() {
    return Is(this);
  }
}
let ni = 0, Ct, Tt;
function ii(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Tt, Tt = e;
    return;
  }
  e.next = Ct, Ct = e;
}
function Gs() {
  ni++;
}
function Ys() {
  if (--ni > 0)
    return;
  if (Tt) {
    let t = Tt;
    for (Tt = void 0; t; ) {
      const s = t.next;
      t.next = void 0, t.flags &= -9, t = s;
    }
  }
  let e;
  for (; Ct; ) {
    let t = Ct;
    for (Ct = void 0; t; ) {
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
    n.version === -1 ? (n === s && (s = i), zs(n), br(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = i;
  }
  e.deps = t, e.depsTail = s;
}
function Is(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (li(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function li(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Mt) || (e.globalVersion = Mt, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Is(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = q, n = ge;
  q = e, ge = !0;
  try {
    ri(e);
    const i = e.fn(e._value);
    (t.version === 0 || Pe(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    q = s, ge = n, oi(e), e.flags &= -3;
  }
}
function zs(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: i } = e;
  if (n && (n.nextSub = i, e.prevSub = void 0), i && (i.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let r = s.computed.deps; r; r = r.nextDep)
      zs(r, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function br(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let ge = !0;
const ci = [];
function Fe() {
  ci.push(ge), ge = !1;
}
function Ve() {
  const e = ci.pop();
  ge = e === void 0 ? !0 : e;
}
function hn(e) {
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
let Mt = 0;
class vr {
  constructor(t, s) {
    this.sub = t, this.dep = s, this.version = s.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class Xs {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0;
  }
  track(t) {
    if (!q || !ge || q === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== q)
      s = this.activeLink = new vr(q, this), q.deps ? (s.prevDep = q.depsTail, q.depsTail.nextDep = s, q.depsTail = s) : q.deps = q.depsTail = s, fi(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = q.depsTail, s.nextDep = void 0, q.depsTail.nextDep = s, q.depsTail = s, q.deps === s && (q.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, Mt++, this.notify(t);
  }
  notify(t) {
    Gs();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      Ys();
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
const Rs = /* @__PURE__ */ new WeakMap(), et = /* @__PURE__ */ Symbol(
  ""
), Fs = /* @__PURE__ */ Symbol(
  ""
), It = /* @__PURE__ */ Symbol(
  ""
);
function ee(e, t, s) {
  if (ge && q) {
    let n = Rs.get(e);
    n || Rs.set(e, n = /* @__PURE__ */ new Map());
    let i = n.get(s);
    i || (n.set(s, i = new Xs()), i.map = n, i.key = s), i.track();
  }
}
function Ue(e, t, s, n, i, r) {
  const o = Rs.get(e);
  if (!o) {
    Mt++;
    return;
  }
  const l = (f) => {
    f && f.trigger();
  };
  if (Gs(), t === "clear")
    o.forEach(l);
  else {
    const f = I(e), d = f && Bs(s);
    if (f && s === "length") {
      const a = Number(n);
      o.forEach((g, C) => {
        (C === "length" || C === It || !Re(C) && C >= a) && l(g);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(It)), t) {
        case "add":
          f ? d && l(o.get("length")) : (l(o.get(et)), ot(e) && l(o.get(Fs)));
          break;
        case "delete":
          f || (l(o.get(et)), ot(e) && l(o.get(Fs)));
          break;
        case "set":
          ot(e) && l(o.get(et));
          break;
      }
  }
  Ys();
}
function nt(e) {
  const t = /* @__PURE__ */ j(e);
  return t === e ? t : (ee(t, "iterate", It), /* @__PURE__ */ pe(e) ? t : t.map(me));
}
function us(e) {
  return ee(e = /* @__PURE__ */ j(e), "iterate", It), e;
}
function Oe(e, t) {
  return /* @__PURE__ */ Le(e) ? ft(/* @__PURE__ */ tt(e) ? me(t) : t) : me(t);
}
const yr = {
  __proto__: null,
  [Symbol.iterator]() {
    return Ss(this, Symbol.iterator, (e) => Oe(this, e));
  },
  concat(...e) {
    return nt(this).concat(
      ...e.map((t) => I(t) ? nt(t) : t)
    );
  },
  entries() {
    return Ss(this, "entries", (e) => (e[1] = Oe(this, e[1]), e));
  },
  every(e, t) {
    return De(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return De(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => Oe(this, n)),
      arguments
    );
  },
  find(e, t) {
    return De(
      this,
      "find",
      e,
      t,
      (s) => Oe(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return De(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return De(
      this,
      "findLast",
      e,
      t,
      (s) => Oe(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return De(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return De(this, "forEach", e, t, void 0, arguments);
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
    return De(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return bt(this, "pop");
  },
  push(...e) {
    return bt(this, "push", e);
  },
  reduce(e, ...t) {
    return gn(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return gn(this, "reduceRight", e, t);
  },
  shift() {
    return bt(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return De(this, "some", e, t, void 0, arguments);
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
    return Ss(this, "values", (e) => Oe(this, e));
  }
};
function Ss(e, t, s) {
  const n = us(e), i = n[t]();
  return n !== e && !/* @__PURE__ */ pe(e) && (i._next = i.next, i.next = () => {
    const r = i._next();
    return r.done || (r.value = s(r.value)), r;
  }), i;
}
const xr = Array.prototype;
function De(e, t, s, n, i, r) {
  const o = us(e), l = o !== e && !/* @__PURE__ */ pe(e), f = o[t];
  if (f !== xr[t]) {
    const g = f.apply(e, r);
    return l ? me(g) : g;
  }
  let d = s;
  o !== e && (l ? d = function(g, C) {
    return s.call(this, Oe(e, g), C, e);
  } : s.length > 2 && (d = function(g, C) {
    return s.call(this, g, C, e);
  }));
  const a = f.call(o, d, n);
  return l && i ? i(a) : a;
}
function gn(e, t, s, n) {
  const i = us(e), r = i !== e && !/* @__PURE__ */ pe(e);
  let o = s, l = !1;
  i !== e && (r ? (l = n.length === 0, o = function(d, a, g) {
    return l && (l = !1, d = Oe(e, d)), s.call(this, d, Oe(e, a), g, e);
  }) : s.length > 3 && (o = function(d, a, g) {
    return s.call(this, d, a, g, e);
  }));
  const f = i[t](o, ...n);
  return l ? Oe(e, f) : f;
}
function ws(e, t, s) {
  const n = /* @__PURE__ */ j(e);
  ee(n, "iterate", It);
  const i = n[t](...s);
  return (i === -1 || i === !1) && /* @__PURE__ */ en(s[0]) ? (s[0] = /* @__PURE__ */ j(s[0]), n[t](...s)) : i;
}
function bt(e, t, s = []) {
  Fe(), Gs();
  const n = (/* @__PURE__ */ j(e))[t].apply(e, s);
  return Ys(), Ve(), n;
}
const Sr = /* @__PURE__ */ Ks("__proto__,__v_isRef,__isVue"), ui = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Re)
);
function wr(e) {
  Re(e) || (e = String(e));
  const t = /* @__PURE__ */ j(this);
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
      return n === (i ? r ? Fr : gi : r ? hi : pi).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(n) ? t : void 0;
    const o = I(t);
    if (!i) {
      let f;
      if (o && (f = yr[s]))
        return f;
      if (s === "hasOwnProperty")
        return wr;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ te(t) ? t : n
    );
    if ((Re(s) ? ui.has(s) : Sr(s)) || (i || ee(t, "get", s), r))
      return l;
    if (/* @__PURE__ */ te(l)) {
      const f = o && Bs(s) ? l : l.value;
      return i && $(f) ? /* @__PURE__ */ Ds(f) : f;
    }
    return $(l) ? i ? /* @__PURE__ */ Ds(l) : /* @__PURE__ */ as(l) : l;
  }
}
class di extends ai {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, i) {
    let r = t[s];
    const o = I(t) && Bs(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ Le(r);
      if (!/* @__PURE__ */ pe(n) && !/* @__PURE__ */ Le(n) && (r = /* @__PURE__ */ j(r), n = /* @__PURE__ */ j(n)), !o && /* @__PURE__ */ te(r) && !/* @__PURE__ */ te(n))
        return d || (r.value = n), !0;
    }
    const l = o ? Number(s) < t.length : U(t, s), f = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ te(t) ? t : i
    );
    return t === /* @__PURE__ */ j(i) && f && (l ? Pe(n, r) && Ue(t, "set", s, n) : Ue(t, "add", s, n)), f;
  }
  deleteProperty(t, s) {
    const n = U(t, s);
    t[s];
    const i = Reflect.deleteProperty(t, s);
    return i && n && Ue(t, "delete", s, void 0), i;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Re(s) || !ui.has(s)) && ee(t, "has", s), n;
  }
  ownKeys(t) {
    return ee(
      t,
      "iterate",
      I(t) ? "length" : et
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
const Vs = (e) => e, Bt = (e) => Reflect.getPrototypeOf(e);
function Ar(e, t, s) {
  return function(...n) {
    const i = this.__v_raw, r = /* @__PURE__ */ j(i), o = ot(r), l = e === "entries" || e === Symbol.iterator && o, f = e === "keys" && o, d = i[e](...n), a = s ? Vs : t ? ft : me;
    return !t && ee(
      r,
      "iterate",
      f ? Fs : et
    ), Z(
      // inheriting all iterator properties
      Object.create(d),
      {
        // iterator protocol
        next() {
          const { value: g, done: C } = d.next();
          return C ? { value: g, done: C } : {
            value: l ? [a(g[0]), a(g[1])] : a(g),
            done: C
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
      const r = this.__v_raw, o = /* @__PURE__ */ j(r), l = /* @__PURE__ */ j(i);
      e || (Pe(i, l) && ee(o, "get", i), ee(o, "get", l));
      const { has: f } = Bt(o), d = t ? Vs : e ? ft : me;
      if (f.call(o, i))
        return d(r.get(i));
      if (f.call(o, l))
        return d(r.get(l));
      r !== o && r.get(i);
    },
    get size() {
      const i = this.__v_raw;
      return !e && ee(/* @__PURE__ */ j(i), "iterate", et), i.size;
    },
    has(i) {
      const r = this.__v_raw, o = /* @__PURE__ */ j(r), l = /* @__PURE__ */ j(i);
      return e || (Pe(i, l) && ee(o, "has", i), ee(o, "has", l)), i === l ? r.has(i) : r.has(i) || r.has(l);
    },
    forEach(i, r) {
      const o = this, l = o.__v_raw, f = /* @__PURE__ */ j(l), d = t ? Vs : e ? ft : me;
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
        const r = /* @__PURE__ */ j(this), o = Bt(r), l = /* @__PURE__ */ j(i), f = !t && !/* @__PURE__ */ pe(i) && !/* @__PURE__ */ Le(i) ? l : i;
        return o.has.call(r, f) || Pe(i, f) && o.has.call(r, i) || Pe(l, f) && o.has.call(r, l) || (r.add(f), Ue(r, "add", f, f)), this;
      },
      set(i, r) {
        !t && !/* @__PURE__ */ pe(r) && !/* @__PURE__ */ Le(r) && (r = /* @__PURE__ */ j(r));
        const o = /* @__PURE__ */ j(this), { has: l, get: f } = Bt(o);
        let d = l.call(o, i);
        d || (i = /* @__PURE__ */ j(i), d = l.call(o, i));
        const a = f.call(o, i);
        return o.set(i, r), d ? Pe(r, a) && Ue(o, "set", i, r) : Ue(o, "add", i, r), this;
      },
      delete(i) {
        const r = /* @__PURE__ */ j(this), { has: o, get: l } = Bt(r);
        let f = o.call(r, i);
        f || (i = /* @__PURE__ */ j(i), f = o.call(r, i)), l && l.call(r, i);
        const d = r.delete(i);
        return f && Ue(r, "delete", i, void 0), d;
      },
      clear() {
        const i = /* @__PURE__ */ j(this), r = i.size !== 0, o = i.clear();
        return r && Ue(
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
function Qs(e, t) {
  const s = Pr(e, t);
  return (n, i, r) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? n : Reflect.get(
    U(s, i) && i in n ? s : n,
    i,
    r
  );
}
const Mr = {
  get: /* @__PURE__ */ Qs(!1, !1)
}, Ir = {
  get: /* @__PURE__ */ Qs(!1, !0)
}, Rr = {
  get: /* @__PURE__ */ Qs(!0, !1)
};
const pi = /* @__PURE__ */ new WeakMap(), hi = /* @__PURE__ */ new WeakMap(), gi = /* @__PURE__ */ new WeakMap(), Fr = /* @__PURE__ */ new WeakMap();
function Vr(e) {
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
  return /* @__PURE__ */ Le(e) ? e : Zs(
    e,
    !1,
    Tr,
    Mr,
    pi
  );
}
// @__NO_SIDE_EFFECTS__
function Dr(e) {
  return Zs(
    e,
    !1,
    Or,
    Ir,
    hi
  );
}
// @__NO_SIDE_EFFECTS__
function Ds(e) {
  return Zs(
    e,
    !0,
    Er,
    Rr,
    gi
  );
}
function Zs(e, t, s, n, i) {
  if (!$(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const r = i.get(e);
  if (r)
    return r;
  const o = Vr(or(e));
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
  return /* @__PURE__ */ Le(e) ? /* @__PURE__ */ tt(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function Le(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function pe(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function en(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function j(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ j(t) : e;
}
function Nr(e) {
  return !U(e, "__v_skip") && Object.isExtensible(e) && Qn(e, "__v_skip", !0), e;
}
const me = (e) => $(e) ? /* @__PURE__ */ as(e) : e, ft = (e) => $(e) ? /* @__PURE__ */ Ds(e) : e;
// @__NO_SIDE_EFFECTS__
function te(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Xt(e) {
  return jr(e, !1);
}
function jr(e, t) {
  return /* @__PURE__ */ te(e) ? e : new Ur(e, t);
}
class Ur {
  constructor(t, s) {
    this.dep = new Xs(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ j(t), this._value = s ? t : me(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ pe(t) || /* @__PURE__ */ Le(t);
    t = n ? t : /* @__PURE__ */ j(t), Pe(t, s) && (this._rawValue = t, this._value = n ? t : me(t), this.dep.trigger());
  }
}
function Hr(e) {
  return /* @__PURE__ */ te(e) ? e.value : e;
}
const $r = {
  get: (e, t, s) => t === "__v_raw" ? e : Hr(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const i = e[t];
    return /* @__PURE__ */ te(i) && !/* @__PURE__ */ te(s) ? (i.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function mi(e) {
  return /* @__PURE__ */ tt(e) ? e : new Proxy(e, $r);
}
class Lr {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Xs(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Mt - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
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
function Kr(e, t, s = !1) {
  let n, i;
  return D(e) ? n = e : (n = e.get, i = e.set), new Lr(n, i, s);
}
const kt = {}, Qt = /* @__PURE__ */ new WeakMap();
let Ze;
function Wr(e, t = !1, s = Ze) {
  if (s) {
    let n = Qt.get(s);
    n || Qt.set(s, n = []), n.push(e);
  }
}
function Br(e, t, s = K) {
  const { immediate: n, deep: i, once: r, scheduler: o, augmentJob: l, call: f } = s, d = (R) => i ? R : /* @__PURE__ */ pe(R) || i === !1 || i === 0 ? He(R, 1) : He(R);
  let a, g, C, O, E = !1, h = !1;
  if (/* @__PURE__ */ te(e) ? (g = () => e.value, E = /* @__PURE__ */ pe(e)) : /* @__PURE__ */ tt(e) ? (g = () => d(e), E = !0) : I(e) ? (h = !0, E = e.some((R) => /* @__PURE__ */ tt(R) || /* @__PURE__ */ pe(R)), g = () => e.map((R) => {
    if (/* @__PURE__ */ te(R))
      return R.value;
    if (/* @__PURE__ */ tt(R))
      return d(R);
    if (D(R))
      return f ? f(R, 2) : R();
  })) : D(e) ? t ? g = f ? () => f(e, 2) : e : g = () => {
    if (C) {
      Fe();
      try {
        C();
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
  } : g = Me, t && i) {
    const R = g, z = i === !0 ? 1 / 0 : i;
    g = () => He(R(), z);
  }
  const T = _r(), W = () => {
    a.stop(), T && T.active && Ws(T.effects, a);
  };
  if (r && t) {
    const R = t;
    t = (...z) => {
      const be = R(...z);
      return W(), be;
    };
  }
  let P = h ? new Array(e.length).fill(kt) : kt;
  const k = (R) => {
    if (!(!(a.flags & 1) || !a.dirty && !R))
      if (t) {
        const z = a.run();
        if (R || i || E || (h ? z.some((be, ve) => Pe(be, P[ve])) : Pe(z, P))) {
          C && C();
          const be = Ze;
          Ze = a;
          try {
            const ve = [
              z,
              // pass undefined as the old value when it's changed for the first time
              P === kt ? void 0 : h && P[0] === kt ? [] : P,
              O
            ];
            P = z, f ? f(t, 3, ve) : (
              // @ts-expect-error
              t(...ve)
            );
          } finally {
            Ze = be;
          }
        }
      } else
        a.run();
  };
  return l && l(k), a = new si(g), a.scheduler = o ? () => o(k, !1) : k, O = (R) => Wr(R, !1, a), C = a.onStop = () => {
    const R = Qt.get(a);
    if (R) {
      if (f)
        f(R, 4);
      else
        for (const z of R) z();
      Qt.delete(a);
    }
  }, t ? n ? k(!0) : P = a.run() : o ? o(k.bind(null, !0), !0) : a.run(), W.pause = a.pause.bind(a), W.resume = a.resume.bind(a), W.stop = W, W;
}
function He(e, t = 1 / 0, s) {
  if (t <= 0 || !$(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ te(e))
    He(e.value, t, s);
  else if (I(e))
    for (let n = 0; n < e.length; n++)
      He(e[n], t, s);
  else if (dt(e) || ot(e))
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
    ds(i, t, s);
  }
}
function _e(e, t, s, n) {
  if (D(e)) {
    const i = jt(e, t, s, n);
    return i && Gn(i) && i.catch((r) => {
      ds(r, t, s);
    }), i;
  }
  if (I(e)) {
    const i = [];
    for (let r = 0; r < e.length; r++)
      i.push(_e(e[r], t, s, n));
    return i;
  }
}
function ds(e, t, s, n = !0) {
  const i = t ? t.vnode : null, { errorHandler: r, throwUnhandledErrorInProduction: o } = t && t.appContext.config || K;
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
  qr(e, s, i, n, o);
}
function qr(e, t, s, n = !0, i = !1) {
  if (i)
    throw e;
  console.error(e);
}
const ie = [];
let Ee = -1;
const lt = [];
let Be = null, it = 0;
const _i = /* @__PURE__ */ Promise.resolve();
let Zt = null;
function bi(e) {
  const t = Zt || _i;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function kr(e) {
  let t = Ee + 1, s = ie.length;
  for (; t < s; ) {
    const n = t + s >>> 1, i = ie[n], r = Rt(i);
    r < e || r === e && i.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function tn(e) {
  if (!(e.flags & 1)) {
    const t = Rt(e), s = ie[ie.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Rt(s) ? ie.push(e) : ie.splice(kr(t), 0, e), e.flags |= 1, vi();
  }
}
function vi() {
  Zt || (Zt = _i.then(xi));
}
function Jr(e) {
  I(e) ? lt.push(...e) : Be && e.id === -1 ? Be.splice(it + 1, 0, e) : e.flags & 1 || (lt.push(e), e.flags |= 1), vi();
}
function mn(e, t, s = Ee + 1) {
  for (; s < ie.length; s++) {
    const n = ie[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      ie.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function yi(e) {
  if (lt.length) {
    const t = [...new Set(lt)].sort(
      (s, n) => Rt(s) - Rt(n)
    );
    if (lt.length = 0, Be) {
      Be.push(...t);
      return;
    }
    for (Be = t, it = 0; it < Be.length; it++) {
      const s = Be[it];
      s.flags & 4 && (s.flags &= -2), s.flags & 8 || s(), s.flags &= -2;
    }
    Be = null, it = 0;
  }
}
const Rt = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function xi(e) {
  try {
    for (Ee = 0; Ee < ie.length; Ee++) {
      const t = ie[Ee];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), jt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Ee < ie.length; Ee++) {
      const t = ie[Ee];
      t && (t.flags &= -2);
    }
    Ee = -1, ie.length = 0, yi(), Zt = null, (ie.length || lt.length) && xi();
  }
}
let de = null, Si = null;
function es(e) {
  const t = de;
  return de = e, Si = e && e.type.__scopeId || null, t;
}
function Gr(e, t = de, s) {
  if (!t || e._n)
    return e;
  const n = (...i) => {
    n._d && An(-1);
    const r = es(t);
    let o;
    try {
      o = e(...i);
    } finally {
      es(r), n._d && An(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function Ce(e, t) {
  if (de === null)
    return e;
  const s = ms(de), n = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [r, o, l, f = K] = t[i];
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
function ze(e, t, s, n) {
  const i = e.dirs, r = t && t.dirs;
  for (let o = 0; o < i.length; o++) {
    const l = i[o];
    r && (l.oldValue = r[o].value);
    let f = l.dir[n];
    f && (Fe(), _e(f, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Ve());
  }
}
function Yr(e, t) {
  if (re) {
    let s = re.provides;
    const n = re.parent && re.parent.provides;
    n === s && (s = re.provides = Object.create(n)), s[e] = t;
  }
}
function Gt(e, t, s = !1) {
  const n = Go();
  if (n || ct) {
    let i = ct ? ct._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return s && D(t) ? t.call(n && n.proxy) : t;
  }
}
const zr = /* @__PURE__ */ Symbol.for("v-scx"), Xr = () => Gt(zr);
function Yt(e, t, s) {
  return wi(e, t, s);
}
function wi(e, t, s = K) {
  const { immediate: n, deep: i, flush: r, once: o } = s, l = Z({}, s), f = t && n || !t && r !== "post";
  let d;
  if (Vt) {
    if (r === "sync") {
      const O = Xr();
      d = O.__watcherHandles || (O.__watcherHandles = []);
    } else if (!f) {
      const O = () => {
      };
      return O.stop = Me, O.resume = Me, O.pause = Me, O;
    }
  }
  const a = re;
  l.call = (O, E, h) => _e(O, a, E, h);
  let g = !1;
  r === "post" ? l.scheduler = (O) => {
    oe(O, a && a.suspense);
  } : r !== "sync" && (g = !0, l.scheduler = (O, E) => {
    E ? O() : tn(O);
  }), l.augmentJob = (O) => {
    t && (O.flags |= 4), g && (O.flags |= 2, a && (O.id = a.uid, O.i = a));
  };
  const C = Br(e, t, l);
  return Vt && (d ? d.push(C) : f && C()), C;
}
function Qr(e, t, s) {
  const n = this.proxy, i = G(e) ? e.includes(".") ? Ci(n, e) : () => n[e] : e.bind(n, n);
  let r;
  D(t) ? r = t : (r = t.handler, s = t);
  const o = Ut(this), l = wi(i, r.bind(n), s);
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
const Zr = /* @__PURE__ */ Symbol("_vte"), eo = (e) => e.__isTeleport, Cs = /* @__PURE__ */ Symbol("_leaveCb");
function sn(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, sn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Ti(e, t) {
  return D(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    Z({ name: e.name }, t, { setup: e })
  ) : e;
}
function Ei(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function _n(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const ts = /* @__PURE__ */ new WeakMap();
function Et(e, t, s, n, i = !1) {
  if (I(e)) {
    e.forEach(
      (h, T) => Et(
        h,
        t && (I(t) ? t[T] : t),
        s,
        n,
        i
      )
    );
    return;
  }
  if (Ot(n) && !i) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Et(e, t, s, n.component.subTree);
    return;
  }
  const r = n.shapeFlag & 4 ? ms(n.component) : n.el, o = i ? null : r, { i: l, r: f } = e, d = t && t.r, a = l.refs === K ? l.refs = {} : l.refs, g = l.setupState, C = /* @__PURE__ */ j(g), O = g === K ? Jn : (h) => _n(a, h) ? !1 : U(C, h), E = (h, T) => !(T && _n(a, T));
  if (d != null && d !== f) {
    if (bn(t), G(d))
      a[d] = null, O(d) && (g[d] = null);
    else if (/* @__PURE__ */ te(d)) {
      const h = t;
      E(d, h.k) && (d.value = null), h.k && (a[h.k] = null);
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
    const h = G(f), T = /* @__PURE__ */ te(f);
    if (h || T) {
      const W = () => {
        if (e.f) {
          const P = h ? O(f) ? g[f] : a[f] : E() || !e.k ? f.value : a[e.k];
          if (i)
            I(P) && Ws(P, r);
          else if (I(P))
            P.includes(r) || P.push(r);
          else if (h)
            a[f] = [r], O(f) && (g[f] = a[f]);
          else {
            const k = [r];
            E(f, e.k) && (f.value = k), e.k && (a[e.k] = k);
          }
        } else h ? (a[f] = o, O(f) && (g[f] = o)) : T && (E(f, e.k) && (f.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const P = () => {
          W(), ts.delete(e);
        };
        P.id = -1, ts.set(e, P), oe(P, s);
      } else
        bn(e), W();
    }
  }
}
function bn(e) {
  const t = ts.get(e);
  t && (t.flags |= 8, ts.delete(e));
}
fs().requestIdleCallback;
fs().cancelIdleCallback;
const Ot = (e) => !!e.type.__asyncLoader, Oi = (e) => e.type.__isKeepAlive;
function to(e, t) {
  Ai(e, "a", t);
}
function so(e, t) {
  Ai(e, "da", t);
}
function Ai(e, t, s = re) {
  const n = e.__wdc || (e.__wdc = () => {
    let i = s;
    for (; i; ) {
      if (i.isDeactivated)
        return;
      i = i.parent;
    }
    return e();
  });
  if (ps(t, n, s), s) {
    let i = s.parent;
    for (; i && i.parent; )
      Oi(i.parent.vnode) && no(n, t, s, i), i = i.parent;
  }
}
function no(e, t, s, n) {
  const i = ps(
    t,
    e,
    n,
    !0
    /* prepend */
  );
  Ii(() => {
    Ws(n[t], i);
  }, s);
}
function ps(e, t, s = re, n = !1) {
  if (s) {
    const i = s[e] || (s[e] = []), r = t.__weh || (t.__weh = (...o) => {
      Fe();
      const l = Ut(s), f = _e(t, s, e, o);
      return l(), Ve(), f;
    });
    return n ? i.unshift(r) : i.push(r), r;
  }
}
const Ke = (e) => (t, s = re) => {
  (!Vt || e === "sp") && ps(e, (...n) => t(...n), s);
}, io = Ke("bm"), Pi = Ke("m"), ro = Ke(
  "bu"
), oo = Ke("u"), Mi = Ke(
  "bum"
), Ii = Ke("um"), lo = Ke(
  "sp"
), co = Ke("rtg"), fo = Ke("rtc");
function uo(e, t = re) {
  ps("ec", e, t);
}
const ao = /* @__PURE__ */ Symbol.for("v-ndc");
function vn(e, t, s, n) {
  let i;
  const r = s, o = I(e);
  if (o || G(e)) {
    const l = o && /* @__PURE__ */ tt(e);
    let f = !1, d = !1;
    l && (f = !/* @__PURE__ */ pe(e), d = /* @__PURE__ */ Le(e), e = us(e)), i = new Array(e.length);
    for (let a = 0, g = e.length; a < g; a++)
      i[a] = t(
        f ? d ? ft(me(e[a])) : me(e[a]) : e[a],
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
const Ns = (e) => e ? Qi(e) ? ms(e) : Ns(e.parent) : null, At = (
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
    $parent: (e) => Ns(e.parent),
    $root: (e) => Ns(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Fi(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      tn(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = bi.bind(e.proxy)),
    $watch: (e) => Qr.bind(e)
  })
), Ts = (e, t) => e !== K && !e.__isScriptSetup && U(e, t), po = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: i, props: r, accessCache: o, type: l, appContext: f } = e;
    if (t[0] !== "$") {
      const C = o[t];
      if (C !== void 0)
        switch (C) {
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
        if (Ts(n, t))
          return o[t] = 1, n[t];
        if (i !== K && U(i, t))
          return o[t] = 2, i[t];
        if (U(r, t))
          return o[t] = 3, r[t];
        if (s !== K && U(s, t))
          return o[t] = 4, s[t];
        js && (o[t] = 0);
      }
    }
    const d = At[t];
    let a, g;
    if (d)
      return t === "$attrs" && ee(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== K && U(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      g = f.config.globalProperties, U(g, t)
    )
      return g[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: i, ctx: r } = e;
    return Ts(i, t) ? (i[t] = s, !0) : n !== K && U(n, t) ? (n[t] = s, !0) : U(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (r[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: i, props: r, type: o }
  }, l) {
    let f;
    return !!(s[l] || e !== K && l[0] !== "$" && U(e, l) || Ts(t, l) || U(r, l) || U(n, l) || U(At, l) || U(i.config.globalProperties, l) || (f = o.__cssModules) && f[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : U(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function yn(e) {
  return I(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let js = !0;
function ho(e) {
  const t = Fi(e), s = e.proxy, n = e.ctx;
  js = !1, t.beforeCreate && xn(t.beforeCreate, e, "bc");
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
    mounted: C,
    beforeUpdate: O,
    updated: E,
    activated: h,
    deactivated: T,
    beforeDestroy: W,
    beforeUnmount: P,
    destroyed: k,
    unmounted: R,
    render: z,
    renderTracked: be,
    renderTriggered: ve,
    errorCaptured: We,
    serverPrefetch: Ht,
    // public API
    expose: Je,
    inheritAttrs: ht,
    // assets
    components: $t,
    directives: Lt,
    filters: _s
  } = t;
  if (d && go(d, n, null), o)
    for (const J in o) {
      const B = o[J];
      D(B) && (n[J] = B.bind(s));
    }
  if (i) {
    const J = i.call(s, s);
    $(J) && (e.data = /* @__PURE__ */ as(J));
  }
  if (js = !0, r)
    for (const J in r) {
      const B = r[J], Ge = D(B) ? B.bind(s, s) : D(B.get) ? B.get.bind(s, s) : Me, Kt = !D(B) && D(B.set) ? B.set.bind(s) : Me, Ye = el({
        get: Ge,
        set: Kt
      });
      Object.defineProperty(n, J, {
        enumerable: !0,
        configurable: !0,
        get: () => Ye.value,
        set: (ye) => Ye.value = ye
      });
    }
  if (l)
    for (const J in l)
      Ri(l[J], n, s, J);
  if (f) {
    const J = D(f) ? f.call(s) : f;
    Reflect.ownKeys(J).forEach((B) => {
      Yr(B, J[B]);
    });
  }
  a && xn(a, e, "c");
  function se(J, B) {
    I(B) ? B.forEach((Ge) => J(Ge.bind(s))) : B && J(B.bind(s));
  }
  if (se(io, g), se(Pi, C), se(ro, O), se(oo, E), se(to, h), se(so, T), se(uo, We), se(fo, be), se(co, ve), se(Mi, P), se(Ii, R), se(lo, Ht), I(Je))
    if (Je.length) {
      const J = e.exposed || (e.exposed = {});
      Je.forEach((B) => {
        Object.defineProperty(J, B, {
          get: () => s[B],
          set: (Ge) => s[B] = Ge,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  z && e.render === Me && (e.render = z), ht != null && (e.inheritAttrs = ht), $t && (e.components = $t), Lt && (e.directives = Lt), Ht && Ei(e);
}
function go(e, t, s = Me) {
  I(e) && (e = Us(e));
  for (const n in e) {
    const i = e[n];
    let r;
    $(i) ? "default" in i ? r = Gt(
      i.from || n,
      i.default,
      !0
    ) : r = Gt(i.from || n) : r = Gt(i), /* @__PURE__ */ te(r) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => r.value,
      set: (o) => r.value = o
    }) : t[n] = r;
  }
}
function xn(e, t, s) {
  _e(
    I(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Ri(e, t, s, n) {
  let i = n.includes(".") ? Ci(s, n) : () => s[n];
  if (G(e)) {
    const r = t[e];
    D(r) && Yt(i, r);
  } else if (D(e))
    Yt(i, e.bind(s));
  else if ($(e))
    if (I(e))
      e.forEach((r) => Ri(r, t, s, n));
    else {
      const r = D(e.handler) ? e.handler.bind(s) : t[e.handler];
      D(r) && Yt(i, r, e);
    }
}
function Fi(e) {
  const t = e.type, { mixins: s, extends: n } = t, {
    mixins: i,
    optionsCache: r,
    config: { optionMergeStrategies: o }
  } = e.appContext, l = r.get(t);
  let f;
  return l ? f = l : !i.length && !s && !n ? f = t : (f = {}, i.length && i.forEach(
    (d) => ss(f, d, o, !0)
  ), ss(f, t, o)), $(t) && r.set(t, f), f;
}
function ss(e, t, s, n = !1) {
  const { mixins: i, extends: r } = t;
  r && ss(e, r, s, !0), i && i.forEach(
    (o) => ss(e, o, s, !0)
  );
  for (const o in t)
    if (!(n && o === "expose")) {
      const l = mo[o] || s && s[o];
      e[o] = l ? l(e[o], t[o]) : t[o];
    }
  return e;
}
const mo = {
  data: Sn,
  props: wn,
  emits: wn,
  // objects
  methods: yt,
  computed: yt,
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
  components: yt,
  directives: yt,
  // watch
  watch: bo,
  // provide / inject
  provide: Sn,
  inject: _o
};
function Sn(e, t) {
  return t ? e ? function() {
    return Z(
      D(e) ? e.call(this, this) : e,
      D(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function _o(e, t) {
  return yt(Us(e), Us(t));
}
function Us(e) {
  if (I(e)) {
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
function yt(e, t) {
  return e ? Z(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function wn(e, t) {
  return e ? I(e) && I(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : Z(
    /* @__PURE__ */ Object.create(null),
    yn(e),
    yn(t ?? {})
  ) : t;
}
function bo(e, t) {
  if (!e) return t;
  if (!t) return e;
  const s = Z(/* @__PURE__ */ Object.create(null), e);
  for (const n in t)
    s[n] = ne(e[n], t[n]);
  return s;
}
function Vi() {
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
let vo = 0;
function yo(e, t) {
  return function(n, i = null) {
    D(n) || (n = Z({}, n)), i != null && !$(i) && (i = null);
    const r = Vi(), o = /* @__PURE__ */ new WeakSet(), l = [];
    let f = !1;
    const d = r.app = {
      _uid: vo++,
      _component: n,
      _props: i,
      _container: null,
      _context: r,
      _instance: null,
      version: tl,
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
      mount(a, g, C) {
        if (!f) {
          const O = d._ceVNode || Ie(n, i);
          return O.appContext = r, C === !0 ? C = "svg" : C === !1 && (C = void 0), e(O, a, C), f = !0, d._container = a, a.__vue_app__ = d, ms(O.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        f && (_e(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, g) {
        return r.provides[a] = g, d;
      },
      runWithContext(a) {
        const g = ct;
        ct = d;
        try {
          return a();
        } finally {
          ct = g;
        }
      }
    };
    return d;
  };
}
let ct = null;
const xo = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${he(t)}Modifiers`] || e[`${st(t)}Modifiers`];
function So(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || K;
  let i = s;
  const r = t.startsWith("update:"), o = r && xo(n, t.slice(7));
  o && (o.trim && (i = s.map((a) => G(a) ? a.trim() : a)), o.number && (i = s.map(cs)));
  let l, f = n[l = vs(t)] || // also try camelCase event handler (#2249)
  n[l = vs(he(t))];
  !f && r && (f = n[l = vs(st(t))]), f && _e(
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
    e.emitted[l] = !0, _e(
      d,
      e,
      6,
      i
    );
  }
}
const wo = /* @__PURE__ */ new WeakMap();
function Di(e, t, s = !1) {
  const n = s ? wo : t.emitsCache, i = n.get(e);
  if (i !== void 0)
    return i;
  const r = e.emits;
  let o = {}, l = !1;
  if (!D(e)) {
    const f = (d) => {
      const a = Di(d, t, !0);
      a && (l = !0, Z(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  return !r && !l ? ($(e) && n.set(e, null), null) : (I(r) ? r.forEach((f) => o[f] = null) : Z(o, r), $(e) && n.set(e, o), o);
}
function hs(e, t) {
  return !e || !rs(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), U(e, t[0].toLowerCase() + t.slice(1)) || U(e, st(t)) || U(e, t));
}
function Cn(e) {
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
    data: C,
    setupState: O,
    ctx: E,
    inheritAttrs: h
  } = e, T = es(e);
  let W, P;
  try {
    if (s.shapeFlag & 4) {
      const R = i || n, z = R;
      W = Ae(
        d.call(
          z,
          R,
          a,
          g,
          O,
          C,
          E
        )
      ), P = l;
    } else {
      const R = t;
      W = Ae(
        R.length > 1 ? R(
          g,
          { attrs: l, slots: o, emit: f }
        ) : R(
          g,
          null
        )
      ), P = t.props ? l : Co(l);
    }
  } catch (R) {
    Pt.length = 0, ds(R, e, 1), W = Ie(ke);
  }
  let k = W;
  if (P && h !== !1) {
    const R = Object.keys(P), { shapeFlag: z } = k;
    R.length && z & 7 && (r && R.some(os) && (P = To(
      P,
      r
    )), k = ut(k, P, !1, !0));
  }
  return s.dirs && (k = ut(k, null, !1, !0), k.dirs = k.dirs ? k.dirs.concat(s.dirs) : s.dirs), s.transition && sn(k, s.transition), W = k, es(T), W;
}
const Co = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || rs(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, To = (e, t) => {
  const s = {};
  for (const n in e)
    (!os(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
  return s;
};
function Eo(e, t, s) {
  const { props: n, children: i, component: r } = e, { props: o, children: l, patchFlag: f } = t, d = r.emitsOptions;
  if (t.dirs || t.transition)
    return !0;
  if (s && f >= 0) {
    if (f & 1024)
      return !0;
    if (f & 16)
      return n ? Tn(n, o, d) : !!o;
    if (f & 8) {
      const a = t.dynamicProps;
      for (let g = 0; g < a.length; g++) {
        const C = a[g];
        if (Ni(o, n, C) && !hs(d, C))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? Tn(n, o, d) : !0 : !!o;
  return !1;
}
function Tn(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < n.length; i++) {
    const r = n[i];
    if (Ni(t, e, r) && !hs(s, r))
      return !0;
  }
  return !1;
}
function Ni(e, t, s) {
  const n = e[s], i = t[s];
  return s === "style" && $(n) && $(i) ? !pt(n, i) : n !== i;
}
function Oo({ vnode: e, parent: t, suspense: s }, n) {
  for (; t; ) {
    const i = t.subTree;
    if (i.suspense && i.suspense.activeBranch === e && (i.suspense.vnode.el = i.el = n, e = i), i === e)
      (e = t.vnode).el = n, t = t.parent;
    else
      break;
  }
  s && s.activeBranch === e && (s.vnode.el = n);
}
const ji = {}, Ui = () => Object.create(ji), Hi = (e) => Object.getPrototypeOf(e) === ji;
function Ao(e, t, s, n = !1) {
  const i = {}, r = Ui();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), $i(e, t, i, r);
  for (const o in e.propsOptions[0])
    o in i || (i[o] = void 0);
  s ? e.props = n ? i : /* @__PURE__ */ Dr(i) : e.type.props ? e.props = i : e.props = r, e.attrs = r;
}
function Po(e, t, s, n) {
  const {
    props: i,
    attrs: r,
    vnode: { patchFlag: o }
  } = e, l = /* @__PURE__ */ j(i), [f] = e.propsOptions;
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
        let C = a[g];
        if (hs(e.emitsOptions, C))
          continue;
        const O = t[C];
        if (f)
          if (U(r, C))
            O !== r[C] && (r[C] = O, d = !0);
          else {
            const E = he(C);
            i[E] = Hs(
              f,
              l,
              E,
              O,
              e,
              !1
            );
          }
        else
          O !== r[C] && (r[C] = O, d = !0);
      }
    }
  } else {
    $i(e, t, i, r) && (d = !0);
    let a;
    for (const g in l)
      (!t || // for camelCase
      !U(t, g) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = st(g)) === g || !U(t, a))) && (f ? s && // for camelCase
      (s[g] !== void 0 || // for kebab-case
      s[a] !== void 0) && (i[g] = Hs(
        f,
        l,
        g,
        void 0,
        e,
        !0
      )) : delete i[g]);
    if (r !== l)
      for (const g in r)
        (!t || !U(t, g)) && (delete r[g], d = !0);
  }
  d && Ue(e.attrs, "set", "");
}
function $i(e, t, s, n) {
  const [i, r] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let f in t) {
      if (wt(f))
        continue;
      const d = t[f];
      let a;
      i && U(i, a = he(f)) ? !r || !r.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : hs(e.emitsOptions, f) || (!(f in n) || d !== n[f]) && (n[f] = d, o = !0);
    }
  if (r) {
    const f = /* @__PURE__ */ j(s), d = l || K;
    for (let a = 0; a < r.length; a++) {
      const g = r[a];
      s[g] = Hs(
        i,
        f,
        g,
        d[g],
        e,
        !U(d, g)
      );
    }
  }
  return o;
}
function Hs(e, t, s, n, i, r) {
  const o = e[s];
  if (o != null) {
    const l = U(o, "default");
    if (l && n === void 0) {
      const f = o.default;
      if (o.type !== Function && !o.skipFactory && D(f)) {
        const { propsDefaults: d } = i;
        if (s in d)
          n = d[s];
        else {
          const a = Ut(i);
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
const Mo = /* @__PURE__ */ new WeakMap();
function Li(e, t, s = !1) {
  const n = s ? Mo : t.propsCache, i = n.get(e);
  if (i)
    return i;
  const r = e.props, o = {}, l = [];
  let f = !1;
  if (!D(e)) {
    const a = (g) => {
      f = !0;
      const [C, O] = Li(g, t, !0);
      Z(o, C), O && l.push(...O);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!r && !f)
    return $(e) && n.set(e, rt), rt;
  if (I(r))
    for (let a = 0; a < r.length; a++) {
      const g = he(r[a]);
      En(g) && (o[g] = K);
    }
  else if (r)
    for (const a in r) {
      const g = he(a);
      if (En(g)) {
        const C = r[a], O = o[g] = I(C) || D(C) ? { type: C } : Z({}, C), E = O.type;
        let h = !1, T = !0;
        if (I(E))
          for (let W = 0; W < E.length; ++W) {
            const P = E[W], k = D(P) && P.name;
            if (k === "Boolean") {
              h = !0;
              break;
            } else k === "String" && (T = !1);
          }
        else
          h = D(E) && E.name === "Boolean";
        O[
          0
          /* shouldCast */
        ] = h, O[
          1
          /* shouldCastTrue */
        ] = T, (h || U(O, "default")) && l.push(g);
      }
    }
  const d = [o, l];
  return $(e) && n.set(e, d), d;
}
function En(e) {
  return e[0] !== "$" && !wt(e);
}
const nn = (e) => e === "_" || e === "_ctx" || e === "$stable", rn = (e) => I(e) ? e.map(Ae) : [Ae(e)], Io = (e, t, s) => {
  if (t._n)
    return t;
  const n = Gr((...i) => rn(t(...i)), s);
  return n._c = !1, n;
}, Ki = (e, t, s) => {
  const n = e._ctx;
  for (const i in e) {
    if (nn(i)) continue;
    const r = e[i];
    if (D(r))
      t[i] = Io(i, r, n);
    else if (r != null) {
      const o = rn(r);
      t[i] = () => o;
    }
  }
}, Wi = (e, t) => {
  const s = rn(t);
  e.slots.default = () => s;
}, Bi = (e, t, s) => {
  for (const n in t)
    (s || !nn(n)) && (e[n] = t[n]);
}, Ro = (e, t, s) => {
  const n = e.slots = Ui();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Bi(n, t, s), s && Qn(n, "_", i, !0)) : Ki(t, n);
  } else t && Wi(e, t);
}, Fo = (e, t, s) => {
  const { vnode: n, slots: i } = e;
  let r = !0, o = K;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? r = !1 : Bi(i, t, s) : (r = !t.$stable, Ki(t, i)), o = t;
  } else t && (Wi(e, t), o = { default: 1 });
  if (r)
    for (const l in i)
      !nn(l) && o[l] == null && delete i[l];
}, oe = Uo;
function Vo(e) {
  return Do(e);
}
function Do(e, t) {
  const s = fs();
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
    nextSibling: C,
    setScopeId: O = Me,
    insertStaticContent: E
  } = e, h = (c, u, p, v = null, b = null, m = null, S = void 0, x = null, y = !!u.dynamicChildren) => {
    if (c === u)
      return;
    c && !vt(c, u) && (v = Wt(c), ye(c, b, m, !0), c = null), u.patchFlag === -2 && (y = !1, u.dynamicChildren = null);
    const { type: _, ref: M, shapeFlag: w } = u;
    switch (_) {
      case gs:
        T(c, u, p, v);
        break;
      case ke:
        W(c, u, p, v);
        break;
      case Os:
        c == null && P(u, p, v, S);
        break;
      case ue:
        $t(
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
        w & 1 ? z(
          c,
          u,
          p,
          v,
          b,
          m,
          S,
          x,
          y
        ) : w & 6 ? Lt(
          c,
          u,
          p,
          v,
          b,
          m,
          S,
          x,
          y
        ) : (w & 64 || w & 128) && _.process(
          c,
          u,
          p,
          v,
          b,
          m,
          S,
          x,
          y,
          mt
        );
    }
    M != null && b ? Et(M, c && c.ref, m, u || c, !u) : M == null && c && c.ref != null && Et(c.ref, null, m, c, !0);
  }, T = (c, u, p, v) => {
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
  }, W = (c, u, p, v) => {
    c == null ? n(
      u.el = f(u.children || ""),
      p,
      v
    ) : u.el = c.el;
  }, P = (c, u, p, v) => {
    [c.el, c.anchor] = E(
      c.children,
      u,
      p,
      v,
      c.el,
      c.anchor
    );
  }, k = ({ el: c, anchor: u }, p, v) => {
    let b;
    for (; c && c !== u; )
      b = C(c), n(c, p, v), c = b;
    n(u, p, v);
  }, R = ({ el: c, anchor: u }) => {
    let p;
    for (; c && c !== u; )
      p = C(c), i(c), c = p;
    i(u);
  }, z = (c, u, p, v, b, m, S, x, y) => {
    if (u.type === "svg" ? S = "svg" : u.type === "math" && (S = "mathml"), c == null)
      be(
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
        _ && _._beginPatch(), Ht(
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
  }, be = (c, u, p, v, b, m, S, x) => {
    let y, _;
    const { props: M, shapeFlag: w, transition: A, dirs: F } = c;
    if (y = c.el = o(
      c.type,
      m,
      M && M.is,
      M
    ), w & 8 ? a(y, c.children) : w & 16 && We(
      c.children,
      y,
      null,
      v,
      b,
      Es(c, m),
      S,
      x
    ), F && ze(c, null, v, "created"), ve(y, c, c.scopeId, S, v), M) {
      for (const L in M)
        L !== "value" && !wt(L) && r(y, L, null, M[L], m, v);
      "value" in M && r(y, "value", null, M.value, m), (_ = M.onVnodeBeforeMount) && Te(_, v, c);
    }
    F && ze(c, null, v, "beforeMount");
    const N = No(b, A);
    N && A.beforeEnter(y), n(y, u, p), ((_ = M && M.onVnodeMounted) || N || F) && oe(() => {
      try {
        _ && Te(_, v, c), N && A.enter(y), F && ze(c, null, v, "mounted");
      } finally {
      }
    }, b);
  }, ve = (c, u, p, v, b) => {
    if (p && O(c, p), v)
      for (let m = 0; m < v.length; m++)
        O(c, v[m]);
    if (b) {
      let m = b.subTree;
      if (u === m || Gi(m.type) && (m.ssContent === u || m.ssFallback === u)) {
        const S = b.vnode;
        ve(
          c,
          S,
          S.scopeId,
          S.slotScopeIds,
          b.parent
        );
      }
    }
  }, We = (c, u, p, v, b, m, S, x, y = 0) => {
    for (let _ = y; _ < c.length; _++) {
      const M = c[_] = x ? je(c[_]) : Ae(c[_]);
      h(
        null,
        M,
        u,
        p,
        v,
        b,
        m,
        S,
        x
      );
    }
  }, Ht = (c, u, p, v, b, m, S) => {
    const x = u.el = c.el;
    let { patchFlag: y, dynamicChildren: _, dirs: M } = u;
    y |= c.patchFlag & 16;
    const w = c.props || K, A = u.props || K;
    let F;
    if (p && Xe(p, !1), (F = A.onVnodeBeforeUpdate) && Te(F, p, u, c), M && ze(u, c, p, "beforeUpdate"), p && Xe(p, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    _ && (!c.dynamicChildren || c.dynamicChildren.length !== _.length) && (y = 0, S = !1, _ = null), (w.innerHTML && A.innerHTML == null || w.textContent && A.textContent == null) && a(x, ""), _ ? Je(
      c.dynamicChildren,
      _,
      x,
      p,
      v,
      Es(u, b),
      m
    ) : S || B(
      c,
      u,
      x,
      null,
      p,
      v,
      Es(u, b),
      m,
      !1
    ), y > 0) {
      if (y & 16)
        ht(x, w, A, p, b);
      else if (y & 2 && w.class !== A.class && r(x, "class", null, A.class, b), y & 4 && r(x, "style", w.style, A.style, b), y & 8) {
        const N = u.dynamicProps;
        for (let L = 0; L < N.length; L++) {
          const H = N[L], Y = w[H], X = A[H];
          (X !== Y || H === "value") && r(x, H, Y, X, b, p);
        }
      }
      y & 1 && c.children !== u.children && a(x, u.children);
    } else !S && _ == null && ht(x, w, A, p, b);
    ((F = A.onVnodeUpdated) || M) && oe(() => {
      F && Te(F, p, u, c), M && ze(u, c, p, "updated");
    }, v);
  }, Je = (c, u, p, v, b, m, S) => {
    for (let x = 0; x < u.length; x++) {
      const y = c[x], _ = u[x], M = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        y.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (y.type === ue || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !vt(y, _) || // - In the case of a component, it could contain anything.
        y.shapeFlag & 198) ? g(y.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          p
        )
      );
      h(
        y,
        _,
        M,
        null,
        v,
        b,
        m,
        S,
        !0
      );
    }
  }, ht = (c, u, p, v, b) => {
    if (u !== p) {
      if (u !== K)
        for (const m in u)
          !wt(m) && !(m in p) && r(
            c,
            m,
            u[m],
            null,
            b,
            v
          );
      for (const m in p) {
        if (wt(m)) continue;
        const S = p[m], x = u[m];
        S !== x && m !== "value" && r(c, m, x, S, b, v);
      }
      "value" in p && r(c, "value", u.value, p.value, b);
    }
  }, $t = (c, u, p, v, b, m, S, x, y) => {
    const _ = u.el = c ? c.el : l(""), M = u.anchor = c ? c.anchor : l("");
    let { patchFlag: w, dynamicChildren: A, slotScopeIds: F } = u;
    F && (x = x ? x.concat(F) : F), c == null ? (n(_, p, v), n(M, p, v), We(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      p,
      M,
      b,
      m,
      S,
      x,
      y
    )) : w > 0 && w & 64 && A && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    c.dynamicChildren && c.dynamicChildren.length === A.length ? (Je(
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
    (u.key != null || b && u === b.subTree) && qi(
      c,
      u,
      !0
      /* shallow */
    )) : B(
      c,
      u,
      p,
      M,
      b,
      m,
      S,
      x,
      y
    );
  }, Lt = (c, u, p, v, b, m, S, x, y) => {
    u.slotScopeIds = x, c == null ? u.shapeFlag & 512 ? b.ctx.activate(
      u,
      p,
      v,
      S,
      y
    ) : _s(
      u,
      p,
      v,
      b,
      m,
      S,
      y
    ) : on(c, u, y);
  }, _s = (c, u, p, v, b, m, S) => {
    const x = c.component = Jo(
      c,
      v,
      b
    );
    if (Oi(c) && (x.ctx.renderer = mt), Yo(x, !1, S), x.asyncDep) {
      if (b && b.registerDep(x, se, S), !c.el) {
        const y = x.subTree = Ie(ke);
        W(null, y, u, p), c.placeholder = y.el;
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
  }, on = (c, u, p) => {
    const v = u.component = c.component;
    if (Eo(c, u, p))
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
        let { next: w, bu: A, u: F, parent: N, vnode: L } = c;
        {
          const Se = ki(c);
          if (Se) {
            w && (w.el = L.el, J(c, w, S)), Se.asyncDep.then(() => {
              oe(() => {
                c.isUnmounted || _();
              }, b);
            });
            return;
          }
        }
        let H = w, Y;
        Xe(c, !1), w ? (w.el = L.el, J(c, w, S)) : w = L, A && Jt(A), (Y = w.props && w.props.onVnodeBeforeUpdate) && Te(Y, N, w, L), Xe(c, !0);
        const X = Cn(c), xe = c.subTree;
        c.subTree = X, h(
          xe,
          X,
          // parent may have changed if it's in a teleport
          g(xe.el),
          // anchor may have changed if it's in a fragment
          Wt(xe),
          c,
          b,
          m
        ), w.el = X.el, H === null && Oo(c, X.el), F && oe(F, b), (Y = w.props && w.props.onVnodeUpdated) && oe(
          () => Te(Y, N, w, L),
          b
        );
      } else {
        let w;
        const { el: A, props: F } = u, { bm: N, m: L, parent: H, root: Y, type: X } = c, xe = Ot(u);
        Xe(c, !1), N && Jt(N), !xe && (w = F && F.onVnodeBeforeMount) && Te(w, H, u), Xe(c, !0);
        {
          Y.ce && Y.ce._hasShadowRoot() && Y.ce._injectChildStyle(
            X,
            c.parent ? c.parent.type : void 0
          );
          const Se = c.subTree = Cn(c);
          h(
            null,
            Se,
            p,
            v,
            c,
            b,
            m
          ), u.el = Se.el;
        }
        if (L && oe(L, b), !xe && (w = F && F.onVnodeMounted)) {
          const Se = u;
          oe(
            () => Te(w, H, Se),
            b
          );
        }
        (u.shapeFlag & 256 || H && Ot(H.vnode) && H.vnode.shapeFlag & 256) && c.a && oe(c.a, b), c.isMounted = !0, u = p = v = null;
      }
    };
    c.scope.on();
    const y = c.effect = new si(x);
    c.scope.off();
    const _ = c.update = y.run.bind(y), M = c.job = y.runIfDirty.bind(y);
    M.i = c, M.id = c.uid, y.scheduler = () => tn(M), Xe(c, !0), _();
  }, J = (c, u, p) => {
    u.component = c;
    const v = c.vnode.props;
    c.vnode = u, c.next = null, Po(c, u.props, v, p), Fo(c, u.children, p), Fe(), mn(c), Ve();
  }, B = (c, u, p, v, b, m, S, x, y = !1) => {
    const _ = c && c.children, M = c ? c.shapeFlag : 0, w = u.children, { patchFlag: A, shapeFlag: F } = u;
    if (A > 0) {
      if (A & 128) {
        Kt(
          _,
          w,
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
        Ge(
          _,
          w,
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
    F & 8 ? (M & 16 && gt(_, b, m), w !== _ && a(p, w)) : M & 16 ? F & 16 ? Kt(
      _,
      w,
      p,
      v,
      b,
      m,
      S,
      x,
      y
    ) : gt(_, b, m, !0) : (M & 8 && a(p, ""), F & 16 && We(
      w,
      p,
      v,
      b,
      m,
      S,
      x,
      y
    ));
  }, Ge = (c, u, p, v, b, m, S, x, y) => {
    c = c || rt, u = u || rt;
    const _ = c.length, M = u.length, w = Math.min(_, M);
    let A;
    for (A = 0; A < w; A++) {
      const F = u[A] = y ? je(u[A]) : Ae(u[A]);
      h(
        c[A],
        F,
        p,
        null,
        b,
        m,
        S,
        x,
        y
      );
    }
    _ > M ? gt(
      c,
      b,
      m,
      !0,
      !1,
      w
    ) : We(
      u,
      p,
      v,
      b,
      m,
      S,
      x,
      y,
      w
    );
  }, Kt = (c, u, p, v, b, m, S, x, y) => {
    let _ = 0;
    const M = u.length;
    let w = c.length - 1, A = M - 1;
    for (; _ <= w && _ <= A; ) {
      const F = c[_], N = u[_] = y ? je(u[_]) : Ae(u[_]);
      if (vt(F, N))
        h(
          F,
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
    for (; _ <= w && _ <= A; ) {
      const F = c[w], N = u[A] = y ? je(u[A]) : Ae(u[A]);
      if (vt(F, N))
        h(
          F,
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
      w--, A--;
    }
    if (_ > w) {
      if (_ <= A) {
        const F = A + 1, N = F < M ? u[F].el : v;
        for (; _ <= A; )
          h(
            null,
            u[_] = y ? je(u[_]) : Ae(u[_]),
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
      for (; _ <= w; )
        ye(c[_], b, m, !0), _++;
    else {
      const F = _, N = _, L = /* @__PURE__ */ new Map();
      for (_ = N; _ <= A; _++) {
        const ce = u[_] = y ? je(u[_]) : Ae(u[_]);
        ce.key != null && L.set(ce.key, _);
      }
      let H, Y = 0;
      const X = A - N + 1;
      let xe = !1, Se = 0;
      const _t = new Array(X);
      for (_ = 0; _ < X; _++) _t[_] = 0;
      for (_ = F; _ <= w; _++) {
        const ce = c[_];
        if (Y >= X) {
          ye(ce, b, m, !0);
          continue;
        }
        let we;
        if (ce.key != null)
          we = L.get(ce.key);
        else
          for (H = N; H <= A; H++)
            if (_t[H - N] === 0 && vt(ce, u[H])) {
              we = H;
              break;
            }
        we === void 0 ? ye(ce, b, m, !0) : (_t[we - N] = _ + 1, we >= Se ? Se = we : xe = !0, h(
          ce,
          u[we],
          p,
          null,
          b,
          m,
          S,
          x,
          y
        ), Y++);
      }
      const fn = xe ? jo(_t) : rt;
      for (H = fn.length - 1, _ = X - 1; _ >= 0; _--) {
        const ce = N + _, we = u[ce], un = u[ce + 1], an = ce + 1 < M ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          un.el || Ji(un)
        ) : v;
        _t[_] === 0 ? h(
          null,
          we,
          p,
          an,
          b,
          m,
          S,
          x,
          y
        ) : xe && (H < 0 || _ !== fn[H] ? Ye(we, p, an, 2) : H--);
      }
    }
  }, Ye = (c, u, p, v, b = null) => {
    const { el: m, type: S, transition: x, children: y, shapeFlag: _ } = c;
    if (_ & 6) {
      Ye(c.component.subTree, u, p, v);
      return;
    }
    if (_ & 128) {
      c.suspense.move(u, p, v);
      return;
    }
    if (_ & 64) {
      S.move(c, u, p, mt);
      return;
    }
    if (S === ue) {
      n(m, u, p);
      for (let w = 0; w < y.length; w++)
        Ye(y[w], u, p, v);
      n(c.anchor, u, p);
      return;
    }
    if (S === Os) {
      k(c, u, p);
      return;
    }
    if (v !== 2 && _ & 1 && x)
      if (v === 0)
        x.persisted && !m[Cs] ? n(m, u, p) : (x.beforeEnter(m), n(m, u, p), oe(() => x.enter(m), b));
      else {
        const { leave: w, delayLeave: A, afterLeave: F } = x, N = () => {
          c.ctx.isUnmounted ? i(m) : n(m, u, p);
        }, L = () => {
          const H = m._isLeaving || !!m[Cs];
          m._isLeaving && m[Cs](
            !0
            /* cancelled */
          ), x.persisted && !H ? N() : w(m, () => {
            N(), F && F();
          });
        };
        A ? A(m, N, L) : L();
      }
    else
      n(m, u, p);
  }, ye = (c, u, p, v = !1, b = !1) => {
    const {
      type: m,
      props: S,
      ref: x,
      children: y,
      dynamicChildren: _,
      shapeFlag: M,
      patchFlag: w,
      dirs: A,
      cacheIndex: F,
      memo: N
    } = c;
    if (w === -2 && (b = !1), x != null && (Fe(), Et(x, null, p, c, !0), Ve()), F != null && (u.renderCache[F] = void 0), M & 256) {
      u.ctx.deactivate(c);
      return;
    }
    const L = M & 1 && A, H = !Ot(c);
    let Y;
    if (H && (Y = S && S.onVnodeBeforeUnmount) && Te(Y, u, c), M & 6)
      ir(c.component, p, v);
    else {
      if (M & 128) {
        c.suspense.unmount(p, v);
        return;
      }
      L && ze(c, null, u, "beforeUnmount"), M & 64 ? c.type.remove(
        c,
        u,
        p,
        mt,
        v
      ) : _ && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !_.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (m !== ue || w > 0 && w & 64) ? gt(
        _,
        u,
        p,
        !1,
        !0
      ) : (m === ue && w & 384 || !b && M & 16) && gt(y, u, p), v && ln(c);
    }
    const X = N != null && F == null;
    (H && (Y = S && S.onVnodeUnmounted) || L || X) && oe(() => {
      Y && Te(Y, u, c), L && ze(c, null, u, "unmounted"), X && (c.el = null);
    }, p);
  }, ln = (c) => {
    const { type: u, el: p, anchor: v, transition: b } = c;
    if (u === ue) {
      nr(p, v);
      return;
    }
    if (u === Os) {
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
  }, nr = (c, u) => {
    let p;
    for (; c !== u; )
      p = C(c), i(c), c = p;
    i(u);
  }, ir = (c, u, p) => {
    const { bum: v, scope: b, job: m, subTree: S, um: x, m: y, a: _ } = c;
    On(y), On(_), v && Jt(v), b.stop(), m && (m.flags |= 8, ye(S, c, u, p)), x && oe(x, u), oe(() => {
      c.isUnmounted = !0;
    }, u);
  }, gt = (c, u, p, v = !1, b = !1, m = 0) => {
    for (let S = m; S < c.length; S++)
      ye(c[S], u, p, v, b);
  }, Wt = (c) => {
    if (c.shapeFlag & 6)
      return Wt(c.component.subTree);
    if (c.shapeFlag & 128)
      return c.suspense.next();
    const u = C(c.anchor || c.el), p = u && u[Zr];
    return p ? C(p) : u;
  };
  let bs = !1;
  const cn = (c, u, p) => {
    let v;
    c == null ? u._vnode && (ye(u._vnode, null, null, !0), v = u._vnode.component) : h(
      u._vnode || null,
      c,
      u,
      null,
      null,
      null,
      p
    ), u._vnode = c, bs || (bs = !0, mn(v), yi(), bs = !1);
  }, mt = {
    p: h,
    um: ye,
    m: Ye,
    r: ln,
    mt: _s,
    mc: We,
    pc: B,
    pbc: Je,
    n: Wt,
    o: e
  };
  return {
    render: cn,
    hydrate: void 0,
    createApp: yo(cn)
  };
}
function Es({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function Xe({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function No(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function qi(e, t, s = !1) {
  const n = e.children, i = t.children;
  if (I(n) && I(i))
    for (let r = 0; r < n.length; r++) {
      const o = n[r];
      let l = i[r];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[r] = je(i[r]), l.el = o.el), !s && l.patchFlag !== -2 && qi(o, l)), l.type === gs && (l.patchFlag === -1 && (l = i[r] = je(l)), l.el = o.el), l.type === ke && !l.el && (l.el = o.el);
    }
}
function jo(e) {
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
function On(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function Ji(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? Ji(t.subTree) : null;
}
const Gi = (e) => e.__isSuspense;
function Uo(e, t) {
  t && t.pendingBranch ? I(e) ? t.effects.push(...e) : t.effects.push(e) : Jr(e);
}
const ue = /* @__PURE__ */ Symbol.for("v-fgt"), gs = /* @__PURE__ */ Symbol.for("v-txt"), ke = /* @__PURE__ */ Symbol.for("v-cmt"), Os = /* @__PURE__ */ Symbol.for("v-stc"), Pt = [];
let ae = null;
function le(e = !1) {
  Pt.push(ae = e ? null : []);
}
function Ho() {
  Pt.pop(), ae = Pt[Pt.length - 1] || null;
}
let Ft = 1;
function An(e, t = !1) {
  Ft += e, e < 0 && ae && t && (ae.hasOnce = !0);
}
function Yi(e) {
  return e.dynamicChildren = Ft > 0 ? ae || rt : null, Ho(), Ft > 0 && ae && ae.push(e), e;
}
function fe(e, t, s, n, i, r) {
  return Yi(
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
function $o(e, t, s, n, i) {
  return Yi(
    Ie(
      e,
      t,
      s,
      n,
      i,
      !0
    )
  );
}
function zi(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function vt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const Xi = ({ key: e }) => e ?? null, zt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? G(e) || /* @__PURE__ */ te(e) || D(e) ? { i: de, r: e, k: t, f: !!s } : e : null);
function V(e, t = null, s = null, n = 0, i = null, r = e === ue ? 0 : 1, o = !1, l = !1) {
  const f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Xi(t),
    ref: t && zt(t),
    scopeId: Si,
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
    ctx: de
  };
  return l ? (ns(f, s), r & 128 && e.normalize(f)) : s && (f.shapeFlag |= G(s) ? 8 : 16), Ft > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  ae && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (f.patchFlag > 0 || r & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  f.patchFlag !== 32 && ae.push(f), f;
}
const Ie = Lo;
function Lo(e, t = null, s = null, n = 0, i = null, r = !1) {
  if ((!e || e === ao) && (e = ke), zi(e)) {
    const l = ut(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ns(l, s), Ft > 0 && !r && ae && (l.shapeFlag & 6 ? ae[ae.indexOf(e)] = l : ae.push(l)), l.patchFlag = -2, l;
  }
  if (Zo(e) && (e = e.__vccOpts), t) {
    t = Ko(t);
    let { class: l, style: f } = t;
    l && !G(l) && (t.class = ks(l)), $(f) && (/* @__PURE__ */ en(f) && !I(f) && (f = Z({}, f)), t.style = qs(f));
  }
  const o = G(e) ? 1 : Gi(e) ? 128 : eo(e) ? 64 : $(e) ? 4 : D(e) ? 2 : 0;
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
function Ko(e) {
  return e ? /* @__PURE__ */ en(e) || Hi(e) ? Z({}, e) : e : null;
}
function ut(e, t, s = !1, n = !1) {
  const { props: i, ref: r, patchFlag: o, children: l, transition: f } = e, d = t ? Bo(i || {}, t) : i, a = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: d,
    key: d && Xi(d),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      s && r ? I(r) ? r.concat(zt(t)) : [r, zt(t)] : zt(t)
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
    ssContent: e.ssContent && ut(e.ssContent),
    ssFallback: e.ssFallback && ut(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return f && n && sn(
    a,
    f.clone(a)
  ), a;
}
function Wo(e = " ", t = 0) {
  return Ie(gs, null, e, t);
}
function xt(e = "", t = !1) {
  return t ? (le(), $o(ke, null, e)) : Ie(ke, null, e);
}
function Ae(e) {
  return e == null || typeof e == "boolean" ? Ie(ke) : I(e) ? Ie(
    ue,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : zi(e) ? je(e) : Ie(gs, null, String(e));
}
function je(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : ut(e);
}
function ns(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (I(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), ns(e, i()), i._c && (i._d = !0));
      return;
    } else {
      s = 32;
      const i = t._;
      !i && !Hi(t) ? t._ctx = de : i === 3 && de && (de.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (D(t)) {
    if (n & 65) {
      ns(e, { default: t });
      return;
    }
    t = { default: t, _ctx: de }, s = 32;
  } else
    t = String(t), n & 64 ? (s = 16, t = [Wo(t)]) : s = 8;
  e.children = t, e.shapeFlag |= s;
}
function Bo(...e) {
  const t = {};
  for (let s = 0; s < e.length; s++) {
    const n = e[s];
    for (const i in n)
      if (i === "class")
        t.class !== n.class && (t.class = ks([t.class, n.class]));
      else if (i === "style")
        t.style = qs([t.style, n.style]);
      else if (rs(i)) {
        const r = t[i], o = n[i];
        o && r !== o && !(I(r) && r.includes(o)) ? t[i] = r ? [].concat(r, o) : o : o == null && r == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !os(i) && (t[i] = o);
      } else i !== "" && (t[i] = n[i]);
  }
  return t;
}
function Te(e, t, s, n = null) {
  _e(e, t, 7, [
    s,
    n
  ]);
}
const qo = Vi();
let ko = 0;
function Jo(e, t, s) {
  const n = e.type, i = (t ? t.appContext : e.appContext) || qo, r = {
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
    propsOptions: Li(n, i),
    emitsOptions: Di(n, i),
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
  return r.ctx = { _: r }, r.root = t ? t.root : r, r.emit = So.bind(null, r), e.ce && e.ce(r), r;
}
let re = null;
const Go = () => re || de;
let is, $s;
{
  const e = fs(), t = (s, n) => {
    let i;
    return (i = e[s]) || (i = e[s] = []), i.push(n), (r) => {
      i.length > 1 ? i.forEach((o) => o(r)) : i[0](r);
    };
  };
  is = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => re = s
  ), $s = t(
    "__VUE_SSR_SETTERS__",
    (s) => Vt = s
  );
}
const Ut = (e) => {
  const t = re;
  return is(e), e.scope.on(), () => {
    e.scope.off(), is(t);
  };
}, Pn = () => {
  re && re.scope.off(), is(null);
};
function Qi(e) {
  return e.vnode.shapeFlag & 4;
}
let Vt = !1;
function Yo(e, t = !1, s = !1) {
  t && $s(t);
  const { props: n, children: i } = e.vnode, r = Qi(e);
  Ao(e, n, r, t), Ro(e, i, s || t);
  const o = r ? zo(e, t) : void 0;
  return t && $s(!1), o;
}
function zo(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, po);
  const { setup: n } = s;
  if (n) {
    Fe();
    const i = e.setupContext = n.length > 1 ? Qo(e) : null, r = Ut(e), o = jt(
      n,
      e,
      0,
      [
        e.props,
        i
      ]
    ), l = Gn(o);
    if (Ve(), r(), (l || e.sp) && !Ot(e) && Ei(e), l) {
      if (o.then(Pn, Pn), t)
        return o.then((f) => {
          Mn(e, f);
        }).catch((f) => {
          ds(f, e, 0);
        });
      e.asyncDep = o;
    } else
      Mn(e, o);
  } else
    Zi(e);
}
function Mn(e, t, s) {
  D(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : $(t) && (e.setupState = mi(t)), Zi(e);
}
function Zi(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Me);
  {
    const i = Ut(e);
    Fe();
    try {
      ho(e);
    } finally {
      Ve(), i();
    }
  }
}
const Xo = {
  get(e, t) {
    return ee(e, "get", ""), e[t];
  }
};
function Qo(e) {
  const t = (s) => {
    e.exposed = s || {};
  };
  return {
    attrs: new Proxy(e.attrs, Xo),
    slots: e.slots,
    emit: e.emit,
    expose: t
  };
}
function ms(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(mi(Nr(e.exposed)), {
    get(t, s) {
      if (s in t)
        return t[s];
      if (s in At)
        return At[s](e);
    },
    has(t, s) {
      return s in t || s in At;
    }
  })) : e.proxy;
}
function Zo(e) {
  return D(e) && "__vccOpts" in e;
}
const el = (e, t) => /* @__PURE__ */ Kr(e, t, Vt), tl = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Ls;
const In = typeof window < "u" && window.trustedTypes;
if (In)
  try {
    Ls = /* @__PURE__ */ In.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const er = Ls ? (e) => Ls.createHTML(e) : (e) => e, sl = "http://www.w3.org/2000/svg", nl = "http://www.w3.org/1998/Math/MathML", Ne = typeof document < "u" ? document : null, Rn = Ne && /* @__PURE__ */ Ne.createElement("template"), il = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const i = t === "svg" ? Ne.createElementNS(sl, e) : t === "mathml" ? Ne.createElementNS(nl, e) : s ? Ne.createElement(e, { is: s }) : Ne.createElement(e);
    return e === "select" && n && n.multiple != null && i.setAttribute("multiple", n.multiple), i;
  },
  createText: (e) => Ne.createTextNode(e),
  createComment: (e) => Ne.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => Ne.querySelector(e),
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
      Rn.innerHTML = er(
        n === "svg" ? `<svg>${e}</svg>` : n === "mathml" ? `<math>${e}</math>` : e
      );
      const l = Rn.content;
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
function ol(e, t, s) {
  const n = e[rl];
  n && (t = (t ? [t, ...n] : [...n]).join(" ")), t == null ? e.removeAttribute("class") : s ? e.setAttribute("class", t) : e.className = t;
}
const Fn = /* @__PURE__ */ Symbol("_vod"), ll = /* @__PURE__ */ Symbol("_vsh"), cl = /* @__PURE__ */ Symbol(""), fl = /(?:^|;)\s*display\s*:/;
function ul(e, t, s) {
  const n = e.style, i = G(s);
  let r = !1;
  if (s && !i) {
    if (t)
      if (G(t))
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
      l != null ? dl(
        e,
        o,
        !G(t) && t ? t[o] : void 0,
        l
      ) || St(n, o, l) : St(n, o, "");
    }
  } else if (i) {
    if (t !== s) {
      const o = n[cl];
      o && (s += ";" + o), n.cssText = s, r = fl.test(s);
    }
  } else t && e.removeAttribute("style");
  Fn in e && (e[Fn] = r ? n.display : "", e[ll] && (n.display = "none"));
}
const Vn = /\s*!important$/;
function St(e, t, s) {
  if (I(s))
    s.forEach((n) => St(e, t, n));
  else if (s == null && (s = ""), t.startsWith("--"))
    e.setProperty(t, s);
  else {
    const n = al(e, t);
    Vn.test(s) ? e.setProperty(
      st(n),
      s.replace(Vn, ""),
      "important"
    ) : e[n] = s;
  }
}
const Dn = ["Webkit", "Moz", "ms"], As = {};
function al(e, t) {
  const s = As[t];
  if (s)
    return s;
  let n = he(t);
  if (n !== "filter" && n in e)
    return As[t] = n;
  n = Xn(n);
  for (let i = 0; i < Dn.length; i++) {
    const r = Dn[i] + n;
    if (r in e)
      return As[t] = r;
  }
  return t;
}
function dl(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && G(n) && s === n;
}
const Nn = "http://www.w3.org/1999/xlink";
function jn(e, t, s, n, i, r = hr(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Nn, t.slice(6, t.length)) : e.setAttributeNS(Nn, t, s) : s == null || r && !Zn(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    r ? "" : Re(s) ? String(s) : s
  );
}
function Un(e, t, s, n, i) {
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
function pl(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const Hn = /* @__PURE__ */ Symbol("_vei");
function hl(e, t, s, n, i = null) {
  const r = e[Hn] || (e[Hn] = {}), o = r[t];
  if (n && o)
    o.value = n;
  else {
    const [l, f] = _l(t);
    if (n) {
      const d = r[t] = yl(
        n,
        i
      );
      qe(e, l, d, f);
    } else o && (pl(e, l, o, f), r[t] = void 0);
  }
}
const gl = /(Once|Passive|Capture)$/, ml = /^on:?(?:Once|Passive|Capture)$/;
function _l(e) {
  let t, s;
  for (; (s = e.match(gl)) && !ml.test(e); )
    t || (t = {}), e = e.slice(0, e.length - s[1].length), t[s[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : st(e.slice(2)), t];
}
let Ps = 0;
const bl = /* @__PURE__ */ Promise.resolve(), vl = () => Ps || (bl.then(() => Ps = 0), Ps = Date.now());
function yl(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const i = s.value;
    if (I(i)) {
      const r = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        r.call(n), n._stopped = !0;
      };
      const o = i.slice(), l = [n];
      for (let f = 0; f < o.length && !n._stopped; f++) {
        const d = o[f];
        d && _e(
          d,
          t,
          5,
          l
        );
      }
    } else
      _e(
        i,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = vl(), s;
}
const $n = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, xl = (e, t, s, n, i, r) => {
  const o = i === "svg";
  t === "class" ? ol(e, n, o) : t === "style" ? ul(e, s, n) : rs(t) ? os(t) || hl(e, t, s, n, r) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : Sl(e, t, n, o)) ? (Un(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && jn(e, t, n, o, r, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (wl(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !G(n))) ? Un(e, he(t), n, r, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), jn(e, t, n, o));
};
function Sl(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && $n(t) && D(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return $n(t) && G(s) ? !1 : t in e;
}
function wl(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = he(t);
  return Array.isArray(s) ? s.some((i) => he(i) === n) : Object.keys(s).some((i) => he(i) === n);
}
const at = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return I(t) ? (s) => Jt(t, s) : t;
};
function Cl(e) {
  e.target.composing = !0;
}
function Ln(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const $e = /* @__PURE__ */ Symbol("_assign");
function Kn(e, t, s) {
  return t && (e = e.trim()), s && (e = cs(e)), e;
}
const Qe = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, i) {
    e[$e] = at(i);
    const r = n || i.props && i.props.type === "number";
    qe(e, t ? "change" : "input", (o) => {
      o.target.composing || e[$e](Kn(e.value, s, r));
    }), (s || r) && qe(e, "change", () => {
      e.value = Kn(e.value, s, r);
    }), t || (qe(e, "compositionstart", Cl), qe(e, "compositionend", Ln), qe(e, "change", Ln));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: i, number: r } }, o) {
    if (e[$e] = at(o), e.composing) return;
    const l = (r || e.type === "number") && !/^0\d/.test(e.value) ? cs(e.value) : e.value, f = t ?? "";
    if (l === f)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (n && t === s || i && e.value.trim() === f) || (e.value = f);
  }
}, Tl = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, s) {
    e[$e] = at(s), qe(e, "change", () => {
      const n = e._modelValue, i = Dt(e), r = e.checked, o = e[$e];
      if (I(n)) {
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
        o(tr(e, r));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: Wn,
  beforeUpdate(e, t, s) {
    e[$e] = at(s), Wn(e, t, s);
  }
};
function Wn(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let i;
  if (I(t))
    i = Js(t, n.props.value) > -1;
  else if (dt(t))
    i = t.has(n.props.value);
  else {
    if (t === s) return;
    i = pt(t, tr(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const Bn = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const i = dt(t);
    qe(e, "change", () => {
      const r = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? cs(Dt(o)) : Dt(o)
      );
      e[$e](
        e.multiple ? i ? new Set(r) : r : r[0]
      ), e._assigning = !0, bi(() => {
        e._assigning = !1;
      });
    }), e[$e] = at(n);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    qn(e, t);
  },
  beforeUpdate(e, t, s) {
    e[$e] = at(s);
  },
  updated(e, { value: t }) {
    e._assigning || qn(e, t);
  }
};
function qn(e, t) {
  const s = e.multiple, n = I(t);
  if (!(s && !n && !dt(t))) {
    for (let i = 0, r = e.options.length; i < r; i++) {
      const o = e.options[i], l = Dt(o);
      if (s)
        if (n) {
          const f = typeof l;
          f === "string" || f === "number" ? o.selected = t.some((d) => String(d) === String(l)) : o.selected = Js(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (pt(Dt(o), t)) {
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
function tr(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
}
const El = /* @__PURE__ */ Z({ patchProp: xl }, il);
let kn;
function Ol() {
  return kn || (kn = Vo(El));
}
const Al = ((...e) => {
  const t = Ol().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const i = Ml(n);
    if (!i) return;
    const r = t._component;
    !D(r) && !r.render && !r.template && (r.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
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
const Il = ["value"], Rl = /* @__PURE__ */ Ti({
  __name: "CredentialSlot",
  props: {
    api: {},
    modelValue: {}
  },
  emits: ["update:modelValue"],
  setup(e, { emit: t }) {
    const s = e, n = t, i = /* @__PURE__ */ Xt(null), r = /* @__PURE__ */ Xt(!1);
    let o = null;
    return Pi(() => {
      !i.value || !s.api.mountCredentialField || (o = s.api.mountCredentialField(i.value, {
        value: s.modelValue,
        noneLabel: "(none — use fields below)",
        onChange: (l) => n("update:modelValue", l)
      }), r.value = !0);
    }), Yt(() => s.modelValue, (l) => o == null ? void 0 : o.setValue(l)), Mi(() => o == null ? void 0 : o.destroy()), (l, f) => (le(), fe("div", {
      ref_key: "el",
      ref: i,
      class: "cred-slot"
    }, [
      r.value ? xt("", !0) : (le(), fe("input", {
        key: 0,
        value: e.modelValue,
        class: "w-260",
        spellcheck: "false",
        placeholder: "secret:<scope>:<entry>",
        onInput: f[0] || (f[0] = (d) => n("update:modelValue", d.target.value))
      }, null, 40, Il))
    ], 512));
  }
}), sr = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, i] of t)
    s[n] = i;
  return s;
}, Fl = /* @__PURE__ */ sr(Rl, [["__scopeId", "data-v-c6a4a9b8"]]), Vl = { class: "sql-set" }, Dl = { class: "row" }, Nl = ["value"], jl = {
  key: 0,
  class: "muted empty"
}, Ul = { class: "row spread" }, Hl = { class: "row" }, $l = ["onUpdate:modelValue"], Ll = ["onUpdate:modelValue"], Kl = ["onClick"], Wl = {
  key: 0,
  class: "row"
}, Bl = ["onUpdate:modelValue"], ql = ["onUpdate:modelValue"], kl = {
  key: 1,
  class: "row"
}, Jl = ["onUpdate:modelValue"], Gl = { class: "row" }, Yl = {
  key: 0,
  class: "chk"
}, zl = ["onUpdate:modelValue"], Xl = { class: "row" }, Ql = { class: "row" }, Zl = ["onUpdate:modelValue", "placeholder"], ec = { class: "row" }, tc = ["onUpdate:modelValue"], sc = { class: "row" }, nc = ["disabled", "onClick"], ic = { class: "muted" }, rc = /* @__PURE__ */ Ti({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function i(E, h) {
      return {
        key: n++,
        name: E,
        provider: h.provider || "mssql",
        path: h.provider === "sqlite" ? h.file || "" : h.server || "",
        database: h.database || "",
        user: h.user || "",
        credential: h.credential || "",
        trustedConnection: h.trusted_connection ?? !0,
        description: h.description || "",
        testing: !1,
        testStatus: ""
      };
    }
    function r(E) {
      return {
        provider: E.provider,
        server: E.provider === "sqlite" ? void 0 : E.path || void 0,
        file: E.provider === "sqlite" && E.path || void 0,
        database: E.database || void 0,
        user: E.user || void 0,
        credential: E.credential || void 0,
        trusted_connection: E.provider === "mssql" ? E.trustedConnection : void 0,
        description: E.description || void 0
        // NOTE: no `password` — literals are written to the secret store via secret.set, never here.
      };
    }
    const o = (() => {
      try {
        return JSON.parse(s.api.getJson() || "null") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ Xt(o.default_connection || ""), f = /* @__PURE__ */ Xt(o.default_limit || 10), d = /* @__PURE__ */ as(
      Object.entries(o.connections || {}).map(([E, h]) => i(E, h))
    );
    function a() {
      d.push(i(`db${d.length + 1}`, { provider: "mssql" }));
    }
    async function g(E) {
      E.testing = !0, E.testStatus = "Connecting...";
      try {
        const h = await s.api.invoke("plugin.action", {
          pluginId: "sql",
          action: "testConnection",
          valueJson: JSON.stringify(C(E))
        });
        if (h.ok && h.resultJson) {
          const T = JSON.parse(h.resultJson);
          E.testStatus = T.message;
        } else
          E.testStatus = "Failed: " + (h.error || "unknown error");
      } catch (h) {
        E.testStatus = "Failed: " + (h instanceof Error ? h.message : String(h));
      } finally {
        E.testing = !1;
      }
    }
    function C(E) {
      const h = r(E);
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
    function O() {
      const E = {
        default_connection: l.value || void 0,
        default_limit: f.value || 10,
        connections: Object.fromEntries(
          d.filter((h) => h.name.trim()).map((h) => [h.name.trim(), r(h)])
        )
      };
      return JSON.stringify(E);
    }
    return t({ toJson: O }), (E, h) => (le(), fe("div", Vl, [
      h[15] || (h[15] = V("div", { class: "muted" }, " Named database connections available to the SQL agent. Passwords live in the secret store (Settings → Secrets); a connection only references an entry by name. Stored in the .spla project file. ", -1)),
      V("div", Dl, [
        V("label", null, [
          h[3] || (h[3] = V("span", { class: "muted" }, "Default connection", -1)),
          Ce(V("select", {
            "onUpdate:modelValue": h[0] || (h[0] = (T) => l.value = T)
          }, [
            h[2] || (h[2] = V("option", { value: "" }, "(none)", -1)),
            (le(!0), fe(ue, null, vn(d, (T) => (le(), fe("option", {
              key: T.key,
              value: T.name
            }, Ms(T.name), 9, Nl))), 128))
          ], 512), [
            [Bn, l.value]
          ])
        ]),
        V("label", null, [
          h[4] || (h[4] = V("span", { class: "muted" }, "Default row limit", -1)),
          Ce(V("input", {
            "onUpdate:modelValue": h[1] || (h[1] = (T) => f.value = T),
            type: "number",
            min: "1",
            class: "w-90"
          }, null, 512), [
            [
              Qe,
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
      }, "+ Add Connection"),
      d.length ? xt("", !0) : (le(), fe("div", jl, 'No connections yet. Click "+ Add Connection".')),
      (le(!0), fe(ue, null, vn(d, (T, W) => (le(), fe("div", {
        key: T.key,
        class: "conn-card"
      }, [
        V("div", Ul, [
          V("div", Hl, [
            h[6] || (h[6] = V("span", { class: "muted" }, "Name", -1)),
            Ce(V("input", {
              "onUpdate:modelValue": (P) => T.name = P,
              class: "w-140",
              spellcheck: "false"
            }, null, 8, $l), [
              [Qe, T.name]
            ]),
            h[7] || (h[7] = V("span", { class: "muted" }, "Provider", -1)),
            Ce(V("select", {
              "onUpdate:modelValue": (P) => T.provider = P
            }, [...h[5] || (h[5] = [
              V("option", { value: "mssql" }, "mssql", -1),
              V("option", { value: "postgres" }, "postgres", -1),
              V("option", { value: "sqlite" }, "sqlite", -1)
            ])], 8, Ll), [
              [Bn, T.provider]
            ])
          ]),
          V("button", {
            type: "button",
            onClick: (P) => d.splice(W, 1)
          }, "✕ Remove", 8, Kl)
        ]),
        T.provider !== "sqlite" ? (le(), fe("div", Wl, [
          h[8] || (h[8] = V("span", { class: "muted w-70" }, "Server", -1)),
          Ce(V("input", {
            "onUpdate:modelValue": (P) => T.path = P,
            placeholder: "sql01 or 192.168.1.10",
            class: "w-220",
            spellcheck: "false"
          }, null, 8, Bl), [
            [Qe, T.path]
          ]),
          h[9] || (h[9] = V("span", { class: "muted w-70" }, "Database", -1)),
          Ce(V("input", {
            "onUpdate:modelValue": (P) => T.database = P,
            class: "w-160",
            spellcheck: "false"
          }, null, 8, ql), [
            [Qe, T.database]
          ])
        ])) : (le(), fe("div", kl, [
          h[10] || (h[10] = V("span", { class: "muted w-70" }, "File", -1)),
          Ce(V("input", {
            "onUpdate:modelValue": (P) => T.path = P,
            placeholder: "C:\\data\\mydb.sqlite",
            class: "w-400",
            spellcheck: "false"
          }, null, 8, Jl), [
            [Qe, T.path]
          ])
        ])),
        T.provider !== "sqlite" ? (le(), fe(ue, { key: 2 }, [
          V("div", Gl, [
            T.provider === "mssql" ? (le(), fe("label", Yl, [
              Ce(V("input", {
                type: "checkbox",
                "onUpdate:modelValue": (P) => T.trustedConnection = P
              }, null, 8, zl), [
                [Tl, T.trustedConnection]
              ]),
              h[11] || (h[11] = V("span", null, "Windows Auth (domain)", -1))
            ])) : xt("", !0)
          ]),
          !T.trustedConnection || T.provider !== "mssql" ? (le(), fe(ue, { key: 0 }, [
            V("div", Xl, [
              h[12] || (h[12] = V("span", { class: "muted w-70" }, "Credential", -1)),
              Ie(Fl, {
                api: e.api,
                modelValue: T.credential,
                "onUpdate:modelValue": (P) => T.credential = P
              }, null, 8, ["api", "modelValue", "onUpdate:modelValue"])
            ]),
            V("div", Ql, [
              h[13] || (h[13] = V("span", { class: "muted w-70" }, "User", -1)),
              Ce(V("input", {
                "onUpdate:modelValue": (P) => T.user = P,
                placeholder: T.credential ? "(from credential)" : "login",
                class: "w-130",
                spellcheck: "false"
              }, null, 8, Zl), [
                [Qe, T.user]
              ])
            ])
          ], 64)) : xt("", !0)
        ], 64)) : xt("", !0),
        V("div", ec, [
          h[14] || (h[14] = V("span", { class: "muted w-70" }, "Description", -1)),
          Ce(V("input", {
            "onUpdate:modelValue": (P) => T.description = P,
            placeholder: "Shown to the AI — what this database contains",
            class: "grow"
          }, null, 8, tc), [
            [Qe, T.description]
          ])
        ]),
        V("div", sc, [
          V("button", {
            type: "button",
            disabled: T.testing,
            onClick: (P) => g(T)
          }, "Test Connection", 8, nc),
          V("span", ic, Ms(T.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
}), oc = /* @__PURE__ */ sr(rc, [["__scopeId", "data-v-c94a9009"]]);
function cc(e, t) {
  let s = Al(oc, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toJson(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  cc as mount
};
