(function(){"use strict";try{if(typeof document<"u"){var a=document.createElement("style");a.appendChild(document.createTextNode(".ssh-set[data-v-9464f4c1]{display:flex;flex-direction:column;gap:8px;font-size:var(--fs-sm, 12px);color:var(--text, inherit)}.muted[data-v-9464f4c1]{color:var(--muted, #888)}.empty[data-v-9464f4c1]{font-style:italic}.row[data-v-9464f4c1]{display:flex;gap:8px;align-items:center;flex-wrap:wrap}.row.spread[data-v-9464f4c1]{justify-content:space-between}.self-start[data-v-9464f4c1]{align-self:flex-start}.grow[data-v-9464f4c1]{flex:1}.w-label[data-v-9464f4c1]{width:80px}.w-70[data-v-9464f4c1]{width:70px}.w-120[data-v-9464f4c1]{width:120px}.w-140[data-v-9464f4c1]{width:140px}.w-180[data-v-9464f4c1]{width:180px}.w-260[data-v-9464f4c1]{width:260px}.host-card[data-v-9464f4c1]{border:1px solid var(--border, #444);border-radius:var(--radius, 6px);padding:8px 10px;display:flex;flex-direction:column;gap:6px;background:var(--panel, transparent)}.new-cred[data-v-9464f4c1]{padding:4px 6px;border:1px dashed var(--border, #444);border-radius:5px}.chk[data-v-9464f4c1]{cursor:pointer}.chk input[data-v-9464f4c1]{height:auto}label[data-v-9464f4c1]{display:flex;gap:6px;align-items:center}input[data-v-9464f4c1],select[data-v-9464f4c1]{height:24px;padding:2px 6px;color:var(--text, inherit);background:var(--bg, transparent);border:1px solid var(--border, #444);border-radius:5px;font-family:inherit;font-size:inherit}button[data-v-9464f4c1]{padding:2px 10px;color:var(--text, inherit);background:var(--panel, transparent);border:1px solid var(--border, #444);border-radius:5px;cursor:pointer;font-size:inherit}button[data-v-9464f4c1]:hover:not(:disabled){border-color:var(--muted, #888)}button[data-v-9464f4c1]:disabled{opacity:.5;cursor:default}")),document.head.appendChild(a)}}catch(t){console.error("vite-plugin-css-injected-by-js",t)}})();
/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function Ws(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const s of e.split(",")) t[s] = 1;
  return (s) => s in t;
}
const k = {}, ot = [], Me = () => {
}, qn = () => !1, ns = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), rs = (e) => e.startsWith("onUpdate:"), Q = Object.assign, ks = (e, t) => {
  const s = e.indexOf(t);
  s > -1 && e.splice(s, 1);
}, si = Object.prototype.hasOwnProperty, H = (e, t) => si.call(e, t), R = Array.isArray, lt = (e) => Nt(e) === "[object Map]", pt = (e) => Nt(e) === "[object Set]", pn = (e) => Nt(e) === "[object Date]", U = (e) => typeof e == "function", G = (e) => typeof e == "string", Ie = (e) => typeof e == "symbol", L = (e) => e !== null && typeof e == "object", Gn = (e) => (L(e) || U(e)) && U(e.then) && U(e.catch), Yn = Object.prototype.toString, Nt = (e) => Yn.call(e), ni = (e) => Nt(e).slice(8, -1), zn = (e) => Nt(e) === "[object Object]", Bs = (e) => G(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, St = /* @__PURE__ */ Ws(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), is = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((s) => t[s] || (t[s] = e(s)));
}, ri = /-\w/g, pe = is(
  (e) => e.replace(ri, (t) => t.slice(1).toUpperCase())
), ii = /\B([A-Z])/g, st = is(
  (e) => e.replace(ii, "-$1").toLowerCase()
), Xn = is((e) => e.charAt(0).toUpperCase() + e.slice(1)), _s = is(
  (e) => e ? `on${Xn(e)}` : ""
), Pe = (e, t) => !Object.is(e, t), qt = (e, ...t) => {
  for (let s = 0; s < e.length; s++)
    e[s](...t);
}, Zn = (e, t, s, n = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: n,
    value: s
  });
}, os = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let hn;
const ls = () => hn || (hn = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function Js(e) {
  if (R(e)) {
    const t = {};
    for (let s = 0; s < e.length; s++) {
      const n = e[s], r = G(n) ? fi(n) : Js(n);
      if (r)
        for (const i in r)
          t[i] = r[i];
    }
    return t;
  } else if (G(e) || L(e))
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
function qs(e) {
  let t = "";
  if (G(e))
    t = e;
  else if (R(e))
    for (let s = 0; s < e.length; s++) {
      const n = qs(e[s]);
      n && (t += n + " ");
    }
  else if (L(e))
    for (const s in e)
      e[s] && (t += s + " ");
  return t.trim();
}
const ui = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", ai = /* @__PURE__ */ Ws(ui);
function Qn(e) {
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
  let s = pn(e), n = pn(t);
  if (s || n)
    return s && n ? e.getTime() === t.getTime() : !1;
  if (s = Ie(e), n = Ie(t), s || n)
    return e === t;
  if (s = R(e), n = R(t), s || n)
    return s && n ? di(e, t) : !1;
  if (s = L(e), n = L(t), s || n) {
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
function Gs(e, t) {
  return e.findIndex((s) => ht(s, t));
}
const er = (e) => !!(e && e.__v_isRef === !0), rt = (e) => G(e) ? e : e == null ? "" : R(e) || L(e) && (e.toString === Yn || !U(e.toString)) ? er(e) ? rt(e.value) : JSON.stringify(e, tr, 2) : String(e), tr = (e, t) => er(t) ? tr(e, t.value) : lt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (s, [n, r], i) => (s[bs(n, i) + " =>"] = r, s),
    {}
  )
} : pt(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((s) => bs(s))
} : Ie(t) ? bs(t) : L(t) && !R(t) && !zn(t) ? String(t) : t, bs = (e, t = "") => {
  var s;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Ie(e) ? `Symbol(${(s = e.description) != null ? s : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Z;
class pi {
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
        const r = this.parent.scopes.pop();
        r && r !== this && (this.parent.scopes[this.index] = r, r.index = this.index);
      }
      this.parent = void 0;
    }
  }
}
function hi() {
  return Z;
}
let J;
const ys = /* @__PURE__ */ new WeakSet();
class sr {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, Z && (Z.active ? Z.effects.push(this) : this.flags &= -2);
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
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || rr(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, gn(this), ir(this);
    const t = J, s = he;
    J = this, he = !0;
    try {
      return this.fn();
    } finally {
      or(this), J = t, he = s, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        Xs(t);
      this.deps = this.depsTail = void 0, gn(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? ys.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Rs(this) && this.run();
  }
  get dirty() {
    return Rs(this);
  }
}
let nr = 0, Ct, Tt;
function rr(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Tt, Tt = e;
    return;
  }
  e.next = Ct, Ct = e;
}
function Ys() {
  nr++;
}
function zs() {
  if (--nr > 0)
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
function ir(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function or(e) {
  let t, s = e.depsTail, n = s;
  for (; n; ) {
    const r = n.prevDep;
    n.version === -1 ? (n === s && (s = r), Xs(n), gi(n)) : t = n, n.dep.activeLink = n.prevActiveLink, n.prevActiveLink = void 0, n = r;
  }
  e.deps = t, e.depsTail = s;
}
function Rs(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (lr(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function lr(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Mt) || (e.globalVersion = Mt, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Rs(e))))
    return;
  e.flags |= 2;
  const t = e.dep, s = J, n = he;
  J = e, he = !0;
  try {
    ir(e);
    const r = e.fn(e._value);
    (t.version === 0 || Pe(r, e._value)) && (e.flags |= 128, e._value = r, t.version++);
  } catch (r) {
    throw t.version++, r;
  } finally {
    J = s, he = n, or(e), e.flags &= -3;
  }
}
function Xs(e, t = !1) {
  const { dep: s, prevSub: n, nextSub: r } = e;
  if (n && (n.nextSub = r, e.prevSub = void 0), r && (r.prevSub = n, e.nextSub = void 0), s.subs === e && (s.subs = n, !n && s.computed)) {
    s.computed.flags &= -5;
    for (let i = s.computed.deps; i; i = i.nextDep)
      Xs(i, !0);
  }
  !t && !--s.sc && s.map && s.map.delete(s.key);
}
function gi(e) {
  const { prevDep: t, nextDep: s } = e;
  t && (t.nextDep = s, e.prevDep = void 0), s && (s.prevDep = t, e.nextDep = void 0);
}
let he = !0;
const cr = [];
function Re() {
  cr.push(he), he = !1;
}
function Fe() {
  const e = cr.pop();
  he = e === void 0 ? !0 : e;
}
function gn(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const s = J;
    J = void 0;
    try {
      t();
    } finally {
      J = s;
    }
  }
}
let Mt = 0;
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
    if (!J || !he || J === this.computed)
      return;
    let s = this.activeLink;
    if (s === void 0 || s.sub !== J)
      s = this.activeLink = new mi(J, this), J.deps ? (s.prevDep = J.depsTail, J.depsTail.nextDep = s, J.depsTail = s) : J.deps = J.depsTail = s, fr(s);
    else if (s.version === -1 && (s.version = this.version, s.nextDep)) {
      const n = s.nextDep;
      n.prevDep = s.prevDep, s.prevDep && (s.prevDep.nextDep = n), s.prevDep = J.depsTail, s.nextDep = void 0, J.depsTail.nextDep = s, J.depsTail = s, J.deps === s && (J.deps = n);
    }
    return s;
  }
  trigger(t) {
    this.version++, Mt++, this.notify(t);
  }
  notify(t) {
    Ys();
    try {
      for (let s = this.subs; s; s = s.prevSub)
        s.sub.notify() && s.sub.dep.notify();
    } finally {
      zs();
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
const Fs = /* @__PURE__ */ new WeakMap(), et = /* @__PURE__ */ Symbol(
  ""
), Vs = /* @__PURE__ */ Symbol(
  ""
), It = /* @__PURE__ */ Symbol(
  ""
);
function ee(e, t, s) {
  if (he && J) {
    let n = Fs.get(e);
    n || Fs.set(e, n = /* @__PURE__ */ new Map());
    let r = n.get(s);
    r || (n.set(s, r = new Zs()), r.map = n, r.key = s), r.track();
  }
}
function je(e, t, s, n, r, i) {
  const o = Fs.get(e);
  if (!o) {
    Mt++;
    return;
  }
  const l = (f) => {
    f && f.trigger();
  };
  if (Ys(), t === "clear")
    o.forEach(l);
  else {
    const f = R(e), d = f && Bs(s);
    if (f && s === "length") {
      const a = Number(n);
      o.forEach((h, O) => {
        (O === "length" || O === It || !Ie(O) && O >= a) && l(h);
      });
    } else
      switch ((s !== void 0 || o.has(void 0)) && l(o.get(s)), d && l(o.get(It)), t) {
        case "add":
          f ? d && l(o.get("length")) : (l(o.get(et)), lt(e) && l(o.get(Vs)));
          break;
        case "delete":
          f || (l(o.get(et)), lt(e) && l(o.get(Vs)));
          break;
        case "set":
          lt(e) && l(o.get(et));
          break;
      }
  }
  zs();
}
function nt(e) {
  const t = /* @__PURE__ */ j(e);
  return t === e ? t : (ee(t, "iterate", It), /* @__PURE__ */ de(e) ? t : t.map(ge));
}
function cs(e) {
  return ee(e = /* @__PURE__ */ j(e), "iterate", It), e;
}
function Oe(e, t) {
  return /* @__PURE__ */ Ke(e) ? ut(/* @__PURE__ */ tt(e) ? ge(t) : t) : ge(t);
}
const _i = {
  __proto__: null,
  [Symbol.iterator]() {
    return vs(this, Symbol.iterator, (e) => Oe(this, e));
  },
  concat(...e) {
    return nt(this).concat(
      ...e.map((t) => R(t) ? nt(t) : t)
    );
  },
  entries() {
    return vs(this, "entries", (e) => (e[1] = Oe(this, e[1]), e));
  },
  every(e, t) {
    return Ve(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return Ve(
      this,
      "filter",
      e,
      t,
      (s) => s.map((n) => Oe(this, n)),
      arguments
    );
  },
  find(e, t) {
    return Ve(
      this,
      "find",
      e,
      t,
      (s) => Oe(this, s),
      arguments
    );
  },
  findIndex(e, t) {
    return Ve(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return Ve(
      this,
      "findLast",
      e,
      t,
      (s) => Oe(this, s),
      arguments
    );
  },
  findLastIndex(e, t) {
    return Ve(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return Ve(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return xs(this, "includes", e);
  },
  indexOf(...e) {
    return xs(this, "indexOf", e);
  },
  join(e) {
    return nt(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return xs(this, "lastIndexOf", e);
  },
  map(e, t) {
    return Ve(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return yt(this, "pop");
  },
  push(...e) {
    return yt(this, "push", e);
  },
  reduce(e, ...t) {
    return mn(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return mn(this, "reduceRight", e, t);
  },
  shift() {
    return yt(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return Ve(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return yt(this, "splice", e);
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
    return yt(this, "unshift", e);
  },
  values() {
    return vs(this, "values", (e) => Oe(this, e));
  }
};
function vs(e, t, s) {
  const n = cs(e), r = n[t]();
  return n !== e && !/* @__PURE__ */ de(e) && (r._next = r.next, r.next = () => {
    const i = r._next();
    return i.done || (i.value = s(i.value)), i;
  }), r;
}
const bi = Array.prototype;
function Ve(e, t, s, n, r, i) {
  const o = cs(e), l = o !== e && !/* @__PURE__ */ de(e), f = o[t];
  if (f !== bi[t]) {
    const h = f.apply(e, i);
    return l ? ge(h) : h;
  }
  let d = s;
  o !== e && (l ? d = function(h, O) {
    return s.call(this, Oe(e, h), O, e);
  } : s.length > 2 && (d = function(h, O) {
    return s.call(this, h, O, e);
  }));
  const a = f.call(o, d, n);
  return l && r ? r(a) : a;
}
function mn(e, t, s, n) {
  const r = cs(e), i = r !== e && !/* @__PURE__ */ de(e);
  let o = s, l = !1;
  r !== e && (i ? (l = n.length === 0, o = function(d, a, h) {
    return l && (l = !1, d = Oe(e, d)), s.call(this, d, Oe(e, a), h, e);
  }) : s.length > 3 && (o = function(d, a, h) {
    return s.call(this, d, a, h, e);
  }));
  const f = r[t](o, ...n);
  return l ? Oe(e, f) : f;
}
function xs(e, t, s) {
  const n = /* @__PURE__ */ j(e);
  ee(n, "iterate", It);
  const r = n[t](...s);
  return (r === -1 || r === !1) && /* @__PURE__ */ tn(s[0]) ? (s[0] = /* @__PURE__ */ j(s[0]), n[t](...s)) : r;
}
function yt(e, t, s = []) {
  Re(), Ys();
  const n = (/* @__PURE__ */ j(e))[t].apply(e, s);
  return zs(), Fe(), n;
}
const yi = /* @__PURE__ */ Ws("__proto__,__v_isRef,__isVue"), ur = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Ie)
);
function vi(e) {
  Ie(e) || (e = String(e));
  const t = /* @__PURE__ */ j(this);
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
    const o = R(t);
    if (!r) {
      let f;
      if (o && (f = _i[s]))
        return f;
      if (s === "hasOwnProperty")
        return vi;
    }
    const l = Reflect.get(
      t,
      s,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ te(t) ? t : n
    );
    if ((Ie(s) ? ur.has(s) : yi(s)) || (r || ee(t, "get", s), i))
      return l;
    if (/* @__PURE__ */ te(l)) {
      const f = o && Bs(s) ? l : l.value;
      return r && L(f) ? /* @__PURE__ */ Ns(f) : f;
    }
    return L(l) ? r ? /* @__PURE__ */ Ns(l) : /* @__PURE__ */ fs(l) : l;
  }
}
class dr extends ar {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, s, n, r) {
    let i = t[s];
    const o = R(t) && Bs(s);
    if (!this._isShallow) {
      const d = /* @__PURE__ */ Ke(i);
      if (!/* @__PURE__ */ de(n) && !/* @__PURE__ */ Ke(n) && (i = /* @__PURE__ */ j(i), n = /* @__PURE__ */ j(n)), !o && /* @__PURE__ */ te(i) && !/* @__PURE__ */ te(n))
        return d || (i.value = n), !0;
    }
    const l = o ? Number(s) < t.length : H(t, s), f = Reflect.set(
      t,
      s,
      n,
      /* @__PURE__ */ te(t) ? t : r
    );
    return t === /* @__PURE__ */ j(r) && f && (l ? Pe(n, i) && je(t, "set", s, n) : je(t, "add", s, n)), f;
  }
  deleteProperty(t, s) {
    const n = H(t, s);
    t[s];
    const r = Reflect.deleteProperty(t, s);
    return r && n && je(t, "delete", s, void 0), r;
  }
  has(t, s) {
    const n = Reflect.has(t, s);
    return (!Ie(s) || !ur.has(s)) && ee(t, "has", s), n;
  }
  ownKeys(t) {
    return ee(
      t,
      "iterate",
      R(t) ? "length" : et
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
const wi = /* @__PURE__ */ new dr(), Si = /* @__PURE__ */ new xi(), Ci = /* @__PURE__ */ new dr(!0);
const Us = (e) => e, kt = (e) => Reflect.getPrototypeOf(e);
function Ti(e, t, s) {
  return function(...n) {
    const r = this.__v_raw, i = /* @__PURE__ */ j(r), o = lt(i), l = e === "entries" || e === Symbol.iterator && o, f = e === "keys" && o, d = r[e](...n), a = s ? Us : t ? ut : ge;
    return !t && ee(
      i,
      "iterate",
      f ? Vs : et
    ), Q(
      // inheriting all iterator properties
      Object.create(d),
      {
        // iterator protocol
        next() {
          const { value: h, done: O } = d.next();
          return O ? { value: h, done: O } : {
            value: l ? [a(h[0]), a(h[1])] : a(h),
            done: O
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
function Ei(e, t) {
  const s = {
    get(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ j(i), l = /* @__PURE__ */ j(r);
      e || (Pe(r, l) && ee(o, "get", r), ee(o, "get", l));
      const { has: f } = kt(o), d = t ? Us : e ? ut : ge;
      if (f.call(o, r))
        return d(i.get(r));
      if (f.call(o, l))
        return d(i.get(l));
      i !== o && i.get(r);
    },
    get size() {
      const r = this.__v_raw;
      return !e && ee(/* @__PURE__ */ j(r), "iterate", et), r.size;
    },
    has(r) {
      const i = this.__v_raw, o = /* @__PURE__ */ j(i), l = /* @__PURE__ */ j(r);
      return e || (Pe(r, l) && ee(o, "has", r), ee(o, "has", l)), r === l ? i.has(r) : i.has(r) || i.has(l);
    },
    forEach(r, i) {
      const o = this, l = o.__v_raw, f = /* @__PURE__ */ j(l), d = t ? Us : e ? ut : ge;
      return !e && ee(f, "iterate", et), l.forEach((a, h) => r.call(i, d(a), d(h), o));
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
      add(r) {
        const i = /* @__PURE__ */ j(this), o = kt(i), l = /* @__PURE__ */ j(r), f = !t && !/* @__PURE__ */ de(r) && !/* @__PURE__ */ Ke(r) ? l : r;
        return o.has.call(i, f) || Pe(r, f) && o.has.call(i, r) || Pe(l, f) && o.has.call(i, l) || (i.add(f), je(i, "add", f, f)), this;
      },
      set(r, i) {
        !t && !/* @__PURE__ */ de(i) && !/* @__PURE__ */ Ke(i) && (i = /* @__PURE__ */ j(i));
        const o = /* @__PURE__ */ j(this), { has: l, get: f } = kt(o);
        let d = l.call(o, r);
        d || (r = /* @__PURE__ */ j(r), d = l.call(o, r));
        const a = f.call(o, r);
        return o.set(r, i), d ? Pe(i, a) && je(o, "set", r, i) : je(o, "add", r, i), this;
      },
      delete(r) {
        const i = /* @__PURE__ */ j(this), { has: o, get: l } = kt(i);
        let f = o.call(i, r);
        f || (r = /* @__PURE__ */ j(r), f = o.call(i, r)), l && l.call(i, r);
        const d = i.delete(r);
        return f && je(i, "delete", r, void 0), d;
      },
      clear() {
        const r = /* @__PURE__ */ j(this), i = r.size !== 0, o = r.clear();
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
function Qs(e, t) {
  const s = Ei(e, t);
  return (n, r, i) => r === "__v_isReactive" ? !e : r === "__v_isReadonly" ? e : r === "__v_raw" ? n : Reflect.get(
    H(s, r) && r in n ? s : n,
    r,
    i
  );
}
const Oi = {
  get: /* @__PURE__ */ Qs(!1, !1)
}, Ai = {
  get: /* @__PURE__ */ Qs(!1, !0)
}, Pi = {
  get: /* @__PURE__ */ Qs(!0, !1)
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
function fs(e) {
  return /* @__PURE__ */ Ke(e) ? e : en(
    e,
    !1,
    wi,
    Oi,
    pr
  );
}
// @__NO_SIDE_EFFECTS__
function Ri(e) {
  return en(
    e,
    !1,
    Ci,
    Ai,
    hr
  );
}
// @__NO_SIDE_EFFECTS__
function Ns(e) {
  return en(
    e,
    !0,
    Si,
    Pi,
    gr
  );
}
function en(e, t, s, n, r) {
  if (!L(e) || e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
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
function de(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function tn(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function j(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ j(t) : e;
}
function Fi(e) {
  return !H(e, "__v_skip") && Object.isExtensible(e) && Zn(e, "__v_skip", !0), e;
}
const ge = (e) => L(e) ? /* @__PURE__ */ fs(e) : e, ut = (e) => L(e) ? /* @__PURE__ */ Ns(e) : e;
// @__NO_SIDE_EFFECTS__
function te(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function ws(e) {
  return Vi(e, !1);
}
function Vi(e, t) {
  return /* @__PURE__ */ te(e) ? e : new Ui(e, t);
}
class Ui {
  constructor(t, s) {
    this.dep = new Zs(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = s ? t : /* @__PURE__ */ j(t), this._value = s ? t : ge(t), this.__v_isShallow = s;
  }
  get value() {
    return this.dep.track(), this._value;
  }
  set value(t) {
    const s = this._rawValue, n = this.__v_isShallow || /* @__PURE__ */ de(t) || /* @__PURE__ */ Ke(t);
    t = n ? t : /* @__PURE__ */ j(t), Pe(t, s) && (this._rawValue = t, this._value = n ? t : ge(t), this.dep.trigger());
  }
}
function Ni(e) {
  return /* @__PURE__ */ te(e) ? e.value : e;
}
const Di = {
  get: (e, t, s) => t === "__v_raw" ? e : Ni(Reflect.get(e, t, s)),
  set: (e, t, s, n) => {
    const r = e[t];
    return /* @__PURE__ */ te(r) && !/* @__PURE__ */ te(s) ? (r.value = s, !0) : Reflect.set(e, t, s, n);
  }
};
function mr(e) {
  return /* @__PURE__ */ tt(e) ? e : new Proxy(e, Di);
}
class ji {
  constructor(t, s, n) {
    this.fn = t, this.setter = s, this._value = void 0, this.dep = new Zs(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Mt - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !s, this.isSSR = n;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    J !== this)
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
function Hi(e, t, s = !1) {
  let n, r;
  return U(e) ? n = e : (n = e.get, r = e.set), new ji(n, r, s);
}
const Jt = {}, zt = /* @__PURE__ */ new WeakMap();
let Qe;
function $i(e, t = !1, s = Qe) {
  if (s) {
    let n = zt.get(s);
    n || zt.set(s, n = []), n.push(e);
  }
}
function Li(e, t, s = k) {
  const { immediate: n, deep: r, once: i, scheduler: o, augmentJob: l, call: f } = s, d = (S) => r ? S : /* @__PURE__ */ de(S) || r === !1 || r === 0 ? He(S, 1) : He(S);
  let a, h, O, A, D = !1, V = !1;
  if (/* @__PURE__ */ te(e) ? (h = () => e.value, D = /* @__PURE__ */ de(e)) : /* @__PURE__ */ tt(e) ? (h = () => d(e), D = !0) : R(e) ? (V = !0, D = e.some((S) => /* @__PURE__ */ tt(S) || /* @__PURE__ */ de(S)), h = () => e.map((S) => {
    if (/* @__PURE__ */ te(S))
      return S.value;
    if (/* @__PURE__ */ tt(S))
      return d(S);
    if (U(S))
      return f ? f(S, 2) : S();
  })) : U(e) ? t ? h = f ? () => f(e, 2) : e : h = () => {
    if (O) {
      Re();
      try {
        O();
      } finally {
        Fe();
      }
    }
    const S = Qe;
    Qe = a;
    try {
      return f ? f(e, 3, [A]) : e(A);
    } finally {
      Qe = S;
    }
  } : h = Me, t && r) {
    const S = h, z = r === !0 ? 1 / 0 : r;
    h = () => He(S(), z);
  }
  const C = hi(), _ = () => {
    a.stop(), C && C.active && ks(C.effects, a);
  };
  if (i && t) {
    const S = t;
    t = (...z) => {
      const _e = S(...z);
      return _(), _e;
    };
  }
  let v = V ? new Array(e.length).fill(Jt) : Jt;
  const K = (S) => {
    if (!(!(a.flags & 1) || !a.dirty && !S))
      if (t) {
        const z = a.run();
        if (S || r || D || (V ? z.some((_e, be) => Pe(_e, v[be])) : Pe(z, v))) {
          O && O();
          const _e = Qe;
          Qe = a;
          try {
            const be = [
              z,
              // pass undefined as the old value when it's changed for the first time
              v === Jt ? void 0 : V && v[0] === Jt ? [] : v,
              A
            ];
            v = z, f ? f(t, 3, be) : (
              // @ts-expect-error
              t(...be)
            );
          } finally {
            Qe = _e;
          }
        }
      } else
        a.run();
  };
  return l && l(K), a = new sr(h), a.scheduler = o ? () => o(K, !1) : K, A = (S) => $i(S, !1, a), O = a.onStop = () => {
    const S = zt.get(a);
    if (S) {
      if (f)
        f(S, 4);
      else
        for (const z of S) z();
      zt.delete(a);
    }
  }, t ? n ? K(!0) : v = a.run() : o ? o(K.bind(null, !0), !0) : a.run(), _.pause = a.pause.bind(a), _.resume = a.resume.bind(a), _.stop = _, _;
}
function He(e, t = 1 / 0, s) {
  if (t <= 0 || !L(e) || e.__v_skip || (s = s || /* @__PURE__ */ new Map(), (s.get(e) || 0) >= t))
    return e;
  if (s.set(e, t), t--, /* @__PURE__ */ te(e))
    He(e.value, t, s);
  else if (R(e))
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
function Dt(e, t, s, n) {
  try {
    return n ? e(...n) : e();
  } catch (r) {
    us(r, t, s);
  }
}
function me(e, t, s, n) {
  if (U(e)) {
    const r = Dt(e, t, s, n);
    return r && Gn(r) && r.catch((i) => {
      us(i, t, s);
    }), r;
  }
  if (R(e)) {
    const r = [];
    for (let i = 0; i < e.length; i++)
      r.push(me(e[i], t, s, n));
    return r;
  }
}
function us(e, t, s, n = !0) {
  const r = t ? t.vnode : null, { errorHandler: i, throwUnhandledErrorInProduction: o } = t && t.appContext.config || k;
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
    if (i) {
      Re(), Dt(i, null, 10, [
        e,
        f,
        d
      ]), Fe();
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
const re = [];
let Ee = -1;
const ct = [];
let Be = null, it = 0;
const _r = /* @__PURE__ */ Promise.resolve();
let Xt = null;
function br(e) {
  const t = Xt || _r;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Wi(e) {
  let t = Ee + 1, s = re.length;
  for (; t < s; ) {
    const n = t + s >>> 1, r = re[n], i = Rt(r);
    i < e || i === e && r.flags & 2 ? t = n + 1 : s = n;
  }
  return t;
}
function sn(e) {
  if (!(e.flags & 1)) {
    const t = Rt(e), s = re[re.length - 1];
    !s || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Rt(s) ? re.push(e) : re.splice(Wi(t), 0, e), e.flags |= 1, yr();
  }
}
function yr() {
  Xt || (Xt = _r.then(xr));
}
function ki(e) {
  R(e) ? ct.push(...e) : Be && e.id === -1 ? Be.splice(it + 1, 0, e) : e.flags & 1 || (ct.push(e), e.flags |= 1), yr();
}
function _n(e, t, s = Ee + 1) {
  for (; s < re.length; s++) {
    const n = re[s];
    if (n && n.flags & 2) {
      if (e && n.id !== e.uid)
        continue;
      re.splice(s, 1), s--, n.flags & 4 && (n.flags &= -2), n(), n.flags & 4 || (n.flags &= -2);
    }
  }
}
function vr(e) {
  if (ct.length) {
    const t = [...new Set(ct)].sort(
      (s, n) => Rt(s) - Rt(n)
    );
    if (ct.length = 0, Be) {
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
function xr(e) {
  try {
    for (Ee = 0; Ee < re.length; Ee++) {
      const t = re[Ee];
      t && !(t.flags & 8) && (t.flags & 4 && (t.flags &= -2), Dt(
        t,
        t.i,
        t.i ? 15 : 14
      ), t.flags & 4 || (t.flags &= -2));
    }
  } finally {
    for (; Ee < re.length; Ee++) {
      const t = re[Ee];
      t && (t.flags &= -2);
    }
    Ee = -1, re.length = 0, vr(), Xt = null, (re.length || ct.length) && xr();
  }
}
let ae = null, wr = null;
function Zt(e) {
  const t = ae;
  return ae = e, wr = e && e.type.__scopeId || null, t;
}
function Bi(e, t = ae, s) {
  if (!t || e._n)
    return e;
  const n = (...r) => {
    n._d && An(-1);
    const i = Zt(t);
    let o;
    try {
      o = e(...r);
    } finally {
      Zt(i), n._d && An(1);
    }
    return o;
  };
  return n._n = !0, n._c = !0, n._d = !0, n;
}
function oe(e, t) {
  if (ae === null)
    return e;
  const s = hs(ae), n = e.dirs || (e.dirs = []);
  for (let r = 0; r < t.length; r++) {
    let [i, o, l, f = k] = t[r];
    i && (U(i) && (i = {
      mounted: i,
      updated: i
    }), i.deep && He(o), n.push({
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
    f && (Re(), me(f, s, 8, [
      e.el,
      l,
      e,
      t
    ]), Fe());
  }
}
function Ji(e, t) {
  if (ie) {
    let s = ie.provides;
    const n = ie.parent && ie.parent.provides;
    n === s && (s = ie.provides = Object.create(n)), s[e] = t;
  }
}
function Gt(e, t, s = !1) {
  const n = qo();
  if (n || ft) {
    let r = ft ? ft._context.provides : n ? n.parent == null || n.ce ? n.vnode.appContext && n.vnode.appContext.provides : n.parent.provides : void 0;
    if (r && e in r)
      return r[e];
    if (arguments.length > 1)
      return s && U(t) ? t.call(n && n.proxy) : t;
  }
}
const qi = /* @__PURE__ */ Symbol.for("v-scx"), Gi = () => Gt(qi);
function Ss(e, t, s) {
  return Sr(e, t, s);
}
function Sr(e, t, s = k) {
  const { immediate: n, deep: r, flush: i, once: o } = s, l = Q({}, s), f = t && n || !t && i !== "post";
  let d;
  if (Vt) {
    if (i === "sync") {
      const A = Gi();
      d = A.__watcherHandles || (A.__watcherHandles = []);
    } else if (!f) {
      const A = () => {
      };
      return A.stop = Me, A.resume = Me, A.pause = Me, A;
    }
  }
  const a = ie;
  l.call = (A, D, V) => me(A, a, D, V);
  let h = !1;
  i === "post" ? l.scheduler = (A) => {
    le(A, a && a.suspense);
  } : i !== "sync" && (h = !0, l.scheduler = (A, D) => {
    D ? A() : sn(A);
  }), l.augmentJob = (A) => {
    t && (A.flags |= 4), h && (A.flags |= 2, a && (A.id = a.uid, A.i = a));
  };
  const O = Li(e, t, l);
  return Vt && (d ? d.push(O) : f && O()), O;
}
function Yi(e, t, s) {
  const n = this.proxy, r = G(e) ? e.includes(".") ? Cr(n, e) : () => n[e] : e.bind(n, n);
  let i;
  U(t) ? i = t : (i = t.handler, s = t);
  const o = jt(this), l = Sr(r, i.bind(n), s);
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
const zi = /* @__PURE__ */ Symbol("_vte"), Xi = (e) => e.__isTeleport, Cs = /* @__PURE__ */ Symbol("_leaveCb");
function nn(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, nn(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function Zi(e, t) {
  return U(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    Q({ name: e.name }, t, { setup: e })
  ) : e;
}
function Tr(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
function bn(e, t) {
  let s;
  return !!((s = Object.getOwnPropertyDescriptor(e, t)) && !s.configurable);
}
const Qt = /* @__PURE__ */ new WeakMap();
function Et(e, t, s, n, r = !1) {
  if (R(e)) {
    e.forEach(
      (V, C) => Et(
        V,
        t && (R(t) ? t[C] : t),
        s,
        n,
        r
      )
    );
    return;
  }
  if (Ot(n) && !r) {
    n.shapeFlag & 512 && n.type.__asyncResolved && n.component.subTree.component && Et(e, t, s, n.component.subTree);
    return;
  }
  const i = n.shapeFlag & 4 ? hs(n.component) : n.el, o = r ? null : i, { i: l, r: f } = e, d = t && t.r, a = l.refs === k ? l.refs = {} : l.refs, h = l.setupState, O = /* @__PURE__ */ j(h), A = h === k ? qn : (V) => bn(a, V) ? !1 : H(O, V), D = (V, C) => !(C && bn(a, C));
  if (d != null && d !== f) {
    if (yn(t), G(d))
      a[d] = null, A(d) && (h[d] = null);
    else if (/* @__PURE__ */ te(d)) {
      const V = t;
      D(d, V.k) && (d.value = null), V.k && (a[V.k] = null);
    }
  }
  if (U(f)) {
    Re();
    try {
      Dt(f, l, 12, [o, a]);
    } finally {
      Fe();
    }
  } else {
    const V = G(f), C = /* @__PURE__ */ te(f);
    if (V || C) {
      const _ = () => {
        if (e.f) {
          const v = V ? A(f) ? h[f] : a[f] : D() || !e.k ? f.value : a[e.k];
          if (r)
            R(v) && ks(v, i);
          else if (R(v))
            v.includes(i) || v.push(i);
          else if (V)
            a[f] = [i], A(f) && (h[f] = a[f]);
          else {
            const K = [i];
            D(f, e.k) && (f.value = K), e.k && (a[e.k] = K);
          }
        } else V ? (a[f] = o, A(f) && (h[f] = o)) : C && (D(f, e.k) && (f.value = o), e.k && (a[e.k] = o));
      };
      if (o) {
        const v = () => {
          _(), Qt.delete(e);
        };
        v.id = -1, Qt.set(e, v), le(v, s);
      } else
        yn(e), _();
    }
  }
}
function yn(e) {
  const t = Qt.get(e);
  t && (t.flags |= 8, Qt.delete(e));
}
ls().requestIdleCallback;
ls().cancelIdleCallback;
const Ot = (e) => !!e.type.__asyncLoader, Er = (e) => e.type.__isKeepAlive;
function Qi(e, t) {
  Or(e, "a", t);
}
function eo(e, t) {
  Or(e, "da", t);
}
function Or(e, t, s = ie) {
  const n = e.__wdc || (e.__wdc = () => {
    let r = s;
    for (; r; ) {
      if (r.isDeactivated)
        return;
      r = r.parent;
    }
    return e();
  });
  if (as(t, n, s), s) {
    let r = s.parent;
    for (; r && r.parent; )
      Er(r.parent.vnode) && to(n, t, s, r), r = r.parent;
  }
}
function to(e, t, s, n) {
  const r = as(
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
function as(e, t, s = ie, n = !1) {
  if (s) {
    const r = s[e] || (s[e] = []), i = t.__weh || (t.__weh = (...o) => {
      Re();
      const l = jt(s), f = me(t, s, e, o);
      return l(), Fe(), f;
    });
    return n ? r.unshift(i) : r.push(i), i;
  }
}
const We = (e) => (t, s = ie) => {
  (!Vt || e === "sp") && as(e, (...n) => t(...n), s);
}, so = We("bm"), Ar = We("m"), no = We(
  "bu"
), ro = We("u"), io = We(
  "bum"
), Pr = We("um"), oo = We(
  "sp"
), lo = We("rtg"), co = We("rtc");
function fo(e, t = ie) {
  as("ec", e, t);
}
const uo = /* @__PURE__ */ Symbol.for("v-ndc");
function Ts(e, t, s, n) {
  let r;
  const i = s, o = R(e);
  if (o || G(e)) {
    const l = o && /* @__PURE__ */ tt(e);
    let f = !1, d = !1;
    l && (f = !/* @__PURE__ */ de(e), d = /* @__PURE__ */ Ke(e), e = cs(e)), r = new Array(e.length);
    for (let a = 0, h = e.length; a < h; a++)
      r[a] = t(
        f ? d ? ut(ge(e[a])) : ge(e[a]) : e[a],
        a,
        void 0,
        i
      );
  } else if (typeof e == "number") {
    r = new Array(e);
    for (let l = 0; l < e; l++)
      r[l] = t(l + 1, l, void 0, i);
  } else if (L(e))
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
const Ds = (e) => e ? zr(e) ? hs(e) : Ds(e.parent) : null, At = (
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
    $options: (e) => Ir(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      sn(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = br.bind(e.proxy)),
    $watch: (e) => Yi.bind(e)
  })
), Es = (e, t) => e !== k && !e.__isScriptSetup && H(e, t), ao = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: s, setupState: n, data: r, props: i, accessCache: o, type: l, appContext: f } = e;
    if (t[0] !== "$") {
      const O = o[t];
      if (O !== void 0)
        switch (O) {
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
        if (Es(n, t))
          return o[t] = 1, n[t];
        if (r !== k && H(r, t))
          return o[t] = 2, r[t];
        if (H(i, t))
          return o[t] = 3, i[t];
        if (s !== k && H(s, t))
          return o[t] = 4, s[t];
        js && (o[t] = 0);
      }
    }
    const d = At[t];
    let a, h;
    if (d)
      return t === "$attrs" && ee(e.attrs, "get", ""), d(e);
    if (
      // css module (injected by vue-loader)
      (a = l.__cssModules) && (a = a[t])
    )
      return a;
    if (s !== k && H(s, t))
      return o[t] = 4, s[t];
    if (
      // global properties
      h = f.config.globalProperties, H(h, t)
    )
      return h[t];
  },
  set({ _: e }, t, s) {
    const { data: n, setupState: r, ctx: i } = e;
    return Es(r, t) ? (r[t] = s, !0) : n !== k && H(n, t) ? (n[t] = s, !0) : H(e.props, t) || t[0] === "$" && t.slice(1) in e ? !1 : (i[t] = s, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: s, ctx: n, appContext: r, props: i, type: o }
  }, l) {
    let f;
    return !!(s[l] || e !== k && l[0] !== "$" && H(e, l) || Es(t, l) || H(i, l) || H(n, l) || H(At, l) || H(r.config.globalProperties, l) || (f = o.__cssModules) && f[l]);
  },
  defineProperty(e, t, s) {
    return s.get != null ? e._.accessCache[t] = 0 : H(s, "value") && this.set(e, t, s.value, null), Reflect.defineProperty(e, t, s);
  }
};
function vn(e) {
  return R(e) ? e.reduce(
    (t, s) => (t[s] = null, t),
    {}
  ) : e;
}
let js = !0;
function po(e) {
  const t = Ir(e), s = e.proxy, n = e.ctx;
  js = !1, t.beforeCreate && xn(t.beforeCreate, e, "bc");
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
    beforeMount: h,
    mounted: O,
    beforeUpdate: A,
    updated: D,
    activated: V,
    deactivated: C,
    beforeDestroy: _,
    beforeUnmount: v,
    destroyed: K,
    unmounted: S,
    render: z,
    renderTracked: _e,
    renderTriggered: be,
    errorCaptured: ke,
    serverPrefetch: Ht,
    // public API
    expose: Ge,
    inheritAttrs: gt,
    // assets
    components: $t,
    directives: Lt,
    filters: gs
  } = t;
  if (d && ho(d, n, null), o)
    for (const q in o) {
      const B = o[q];
      U(B) && (n[q] = B.bind(s));
    }
  if (r) {
    const q = r.call(s, s);
    L(q) && (e.data = /* @__PURE__ */ fs(q));
  }
  if (js = !0, i)
    for (const q in i) {
      const B = i[q], Ye = U(B) ? B.bind(s, s) : U(B.get) ? B.get.bind(s, s) : Me, Kt = !U(B) && U(B.set) ? B.set.bind(s) : Me, ze = Qo({
        get: Ye,
        set: Kt
      });
      Object.defineProperty(n, q, {
        enumerable: !0,
        configurable: !0,
        get: () => ze.value,
        set: (ye) => ze.value = ye
      });
    }
  if (l)
    for (const q in l)
      Mr(l[q], n, s, q);
  if (f) {
    const q = U(f) ? f.call(s) : f;
    Reflect.ownKeys(q).forEach((B) => {
      Ji(B, q[B]);
    });
  }
  a && xn(a, e, "c");
  function se(q, B) {
    R(B) ? B.forEach((Ye) => q(Ye.bind(s))) : B && q(B.bind(s));
  }
  if (se(so, h), se(Ar, O), se(no, A), se(ro, D), se(Qi, V), se(eo, C), se(fo, ke), se(co, _e), se(lo, be), se(io, v), se(Pr, S), se(oo, Ht), R(Ge))
    if (Ge.length) {
      const q = e.exposed || (e.exposed = {});
      Ge.forEach((B) => {
        Object.defineProperty(q, B, {
          get: () => s[B],
          set: (Ye) => s[B] = Ye,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  z && e.render === Me && (e.render = z), gt != null && (e.inheritAttrs = gt), $t && (e.components = $t), Lt && (e.directives = Lt), Ht && Tr(e);
}
function ho(e, t, s = Me) {
  R(e) && (e = Hs(e));
  for (const n in e) {
    const r = e[n];
    let i;
    L(r) ? "default" in r ? i = Gt(
      r.from || n,
      r.default,
      !0
    ) : i = Gt(r.from || n) : i = Gt(r), /* @__PURE__ */ te(i) ? Object.defineProperty(t, n, {
      enumerable: !0,
      configurable: !0,
      get: () => i.value,
      set: (o) => i.value = o
    }) : t[n] = i;
  }
}
function xn(e, t, s) {
  me(
    R(e) ? e.map((n) => n.bind(t.proxy)) : e.bind(t.proxy),
    t,
    s
  );
}
function Mr(e, t, s, n) {
  let r = n.includes(".") ? Cr(s, n) : () => s[n];
  if (G(e)) {
    const i = t[e];
    U(i) && Ss(r, i);
  } else if (U(e))
    Ss(r, e.bind(s));
  else if (L(e))
    if (R(e))
      e.forEach((i) => Mr(i, t, s, n));
    else {
      const i = U(e.handler) ? e.handler.bind(s) : t[e.handler];
      U(i) && Ss(r, i, e);
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
    (d) => es(f, d, o, !0)
  ), es(f, t, o)), L(t) && i.set(t, f), f;
}
function es(e, t, s, n = !1) {
  const { mixins: r, extends: i } = t;
  i && es(e, i, s, !0), r && r.forEach(
    (o) => es(e, o, s, !0)
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
  props: Sn,
  emits: Sn,
  // objects
  methods: xt,
  computed: xt,
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
  components: xt,
  directives: xt,
  // watch
  watch: _o,
  // provide / inject
  provide: wn,
  inject: mo
};
function wn(e, t) {
  return t ? e ? function() {
    return Q(
      U(e) ? e.call(this, this) : e,
      U(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function mo(e, t) {
  return xt(Hs(e), Hs(t));
}
function Hs(e) {
  if (R(e)) {
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
function xt(e, t) {
  return e ? Q(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Sn(e, t) {
  return e ? R(e) && R(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : Q(
    /* @__PURE__ */ Object.create(null),
    vn(e),
    vn(t ?? {})
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
function Rr() {
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
let bo = 0;
function yo(e, t) {
  return function(n, r = null) {
    U(n) || (n = Q({}, n)), r != null && !L(r) && (r = null);
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
      use(a, ...h) {
        return o.has(a) || (a && U(a.install) ? (o.add(a), a.install(d, ...h)) : U(a) && (o.add(a), a(d, ...h))), d;
      },
      mixin(a) {
        return i.mixins.includes(a) || i.mixins.push(a), d;
      },
      component(a, h) {
        return h ? (i.components[a] = h, d) : i.components[a];
      },
      directive(a, h) {
        return h ? (i.directives[a] = h, d) : i.directives[a];
      },
      mount(a, h, O) {
        if (!f) {
          const A = d._ceVNode || $e(n, r);
          return A.appContext = i, O === !0 ? O = "svg" : O === !1 && (O = void 0), e(A, a, O), f = !0, d._container = a, a.__vue_app__ = d, hs(A.component);
        }
      },
      onUnmount(a) {
        l.push(a);
      },
      unmount() {
        f && (me(
          l,
          d._instance,
          16
        ), e(null, d._container), delete d._container.__vue_app__);
      },
      provide(a, h) {
        return i.provides[a] = h, d;
      },
      runWithContext(a) {
        const h = ft;
        ft = d;
        try {
          return a();
        } finally {
          ft = h;
        }
      }
    };
    return d;
  };
}
let ft = null;
const vo = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${pe(t)}Modifiers`] || e[`${st(t)}Modifiers`];
function xo(e, t, ...s) {
  if (e.isUnmounted) return;
  const n = e.vnode.props || k;
  let r = s;
  const i = t.startsWith("update:"), o = i && vo(n, t.slice(7));
  o && (o.trim && (r = s.map((a) => G(a) ? a.trim() : a)), o.number && (r = s.map(os)));
  let l, f = n[l = _s(t)] || // also try camelCase event handler (#2249)
  n[l = _s(pe(t))];
  !f && i && (f = n[l = _s(st(t))]), f && me(
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
    e.emitted[l] = !0, me(
      d,
      e,
      6,
      r
    );
  }
}
const wo = /* @__PURE__ */ new WeakMap();
function Fr(e, t, s = !1) {
  const n = s ? wo : t.emitsCache, r = n.get(e);
  if (r !== void 0)
    return r;
  const i = e.emits;
  let o = {}, l = !1;
  if (!U(e)) {
    const f = (d) => {
      const a = Fr(d, t, !0);
      a && (l = !0, Q(o, a));
    };
    !s && t.mixins.length && t.mixins.forEach(f), e.extends && f(e.extends), e.mixins && e.mixins.forEach(f);
  }
  return !i && !l ? (L(e) && n.set(e, null), null) : (R(i) ? i.forEach((f) => o[f] = null) : Q(o, i), L(e) && n.set(e, o), o);
}
function ds(e, t) {
  return !e || !ns(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), H(e, t[0].toLowerCase() + t.slice(1)) || H(e, st(t)) || H(e, t));
}
function Cn(e) {
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
    props: h,
    data: O,
    setupState: A,
    ctx: D,
    inheritAttrs: V
  } = e, C = Zt(e);
  let _, v;
  try {
    if (s.shapeFlag & 4) {
      const S = r || n, z = S;
      _ = Ae(
        d.call(
          z,
          S,
          a,
          h,
          A,
          O,
          D
        )
      ), v = l;
    } else {
      const S = t;
      _ = Ae(
        S.length > 1 ? S(
          h,
          { attrs: l, slots: o, emit: f }
        ) : S(
          h,
          null
        )
      ), v = t.props ? l : So(l);
    }
  } catch (S) {
    Pt.length = 0, us(S, e, 1), _ = $e(qe);
  }
  let K = _;
  if (v && V !== !1) {
    const S = Object.keys(v), { shapeFlag: z } = K;
    S.length && z & 7 && (i && S.some(rs) && (v = Co(
      v,
      i
    )), K = at(K, v, !1, !0));
  }
  return s.dirs && (K = at(K, null, !1, !0), K.dirs = K.dirs ? K.dirs.concat(s.dirs) : s.dirs), s.transition && nn(K, s.transition), _ = K, Zt(C), _;
}
const So = (e) => {
  let t;
  for (const s in e)
    (s === "class" || s === "style" || ns(s)) && ((t || (t = {}))[s] = e[s]);
  return t;
}, Co = (e, t) => {
  const s = {};
  for (const n in e)
    (!rs(n) || !(n.slice(9) in t)) && (s[n] = e[n]);
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
      return n ? Tn(n, o, d) : !!o;
    if (f & 8) {
      const a = t.dynamicProps;
      for (let h = 0; h < a.length; h++) {
        const O = a[h];
        if (Vr(o, n, O) && !ds(d, O))
          return !0;
      }
    }
  } else
    return (r || l) && (!l || !l.$stable) ? !0 : n === o ? !1 : n ? o ? Tn(n, o, d) : !0 : !!o;
  return !1;
}
function Tn(e, t, s) {
  const n = Object.keys(t);
  if (n.length !== Object.keys(e).length)
    return !0;
  for (let r = 0; r < n.length; r++) {
    const i = n[r];
    if (Vr(t, e, i) && !ds(s, i))
      return !0;
  }
  return !1;
}
function Vr(e, t, s) {
  const n = e[s], r = t[s];
  return s === "style" && L(n) && L(r) ? !ht(n, r) : n !== r;
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
const Ur = {}, Nr = () => Object.create(Ur), Dr = (e) => Object.getPrototypeOf(e) === Ur;
function Oo(e, t, s, n = !1) {
  const r = {}, i = Nr();
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
  } = e, l = /* @__PURE__ */ j(r), [f] = e.propsOptions;
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
        let O = a[h];
        if (ds(e.emitsOptions, O))
          continue;
        const A = t[O];
        if (f)
          if (H(i, O))
            A !== i[O] && (i[O] = A, d = !0);
          else {
            const D = pe(O);
            r[D] = $s(
              f,
              l,
              D,
              A,
              e,
              !1
            );
          }
        else
          A !== i[O] && (i[O] = A, d = !0);
      }
    }
  } else {
    jr(e, t, r, i) && (d = !0);
    let a;
    for (const h in l)
      (!t || // for camelCase
      !H(t, h) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((a = st(h)) === h || !H(t, a))) && (f ? s && // for camelCase
      (s[h] !== void 0 || // for kebab-case
      s[a] !== void 0) && (r[h] = $s(
        f,
        l,
        h,
        void 0,
        e,
        !0
      )) : delete r[h]);
    if (i !== l)
      for (const h in i)
        (!t || !H(t, h)) && (delete i[h], d = !0);
  }
  d && je(e.attrs, "set", "");
}
function jr(e, t, s, n) {
  const [r, i] = e.propsOptions;
  let o = !1, l;
  if (t)
    for (let f in t) {
      if (St(f))
        continue;
      const d = t[f];
      let a;
      r && H(r, a = pe(f)) ? !i || !i.includes(a) ? s[a] = d : (l || (l = {}))[a] = d : ds(e.emitsOptions, f) || (!(f in n) || d !== n[f]) && (n[f] = d, o = !0);
    }
  if (i) {
    const f = /* @__PURE__ */ j(s), d = l || k;
    for (let a = 0; a < i.length; a++) {
      const h = i[a];
      s[h] = $s(
        r,
        f,
        h,
        d[h],
        e,
        !H(d, h)
      );
    }
  }
  return o;
}
function $s(e, t, s, n, r, i) {
  const o = e[s];
  if (o != null) {
    const l = H(o, "default");
    if (l && n === void 0) {
      const f = o.default;
      if (o.type !== Function && !o.skipFactory && U(f)) {
        const { propsDefaults: d } = r;
        if (s in d)
          n = d[s];
        else {
          const a = jt(r);
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
function Hr(e, t, s = !1) {
  const n = s ? Po : t.propsCache, r = n.get(e);
  if (r)
    return r;
  const i = e.props, o = {}, l = [];
  let f = !1;
  if (!U(e)) {
    const a = (h) => {
      f = !0;
      const [O, A] = Hr(h, t, !0);
      Q(o, O), A && l.push(...A);
    };
    !s && t.mixins.length && t.mixins.forEach(a), e.extends && a(e.extends), e.mixins && e.mixins.forEach(a);
  }
  if (!i && !f)
    return L(e) && n.set(e, ot), ot;
  if (R(i))
    for (let a = 0; a < i.length; a++) {
      const h = pe(i[a]);
      En(h) && (o[h] = k);
    }
  else if (i)
    for (const a in i) {
      const h = pe(a);
      if (En(h)) {
        const O = i[a], A = o[h] = R(O) || U(O) ? { type: O } : Q({}, O), D = A.type;
        let V = !1, C = !0;
        if (R(D))
          for (let _ = 0; _ < D.length; ++_) {
            const v = D[_], K = U(v) && v.name;
            if (K === "Boolean") {
              V = !0;
              break;
            } else K === "String" && (C = !1);
          }
        else
          V = U(D) && D.name === "Boolean";
        A[
          0
          /* shouldCast */
        ] = V, A[
          1
          /* shouldCastTrue */
        ] = C, (V || H(A, "default")) && l.push(h);
      }
    }
  const d = [o, l];
  return L(e) && n.set(e, d), d;
}
function En(e) {
  return e[0] !== "$" && !St(e);
}
const rn = (e) => e === "_" || e === "_ctx" || e === "$stable", on = (e) => R(e) ? e.map(Ae) : [Ae(e)], Mo = (e, t, s) => {
  if (t._n)
    return t;
  const n = Bi((...r) => on(t(...r)), s);
  return n._c = !1, n;
}, $r = (e, t, s) => {
  const n = e._ctx;
  for (const r in e) {
    if (rn(r)) continue;
    const i = e[r];
    if (U(i))
      t[r] = Mo(r, i, n);
    else if (i != null) {
      const o = on(i);
      t[r] = () => o;
    }
  }
}, Lr = (e, t) => {
  const s = on(t);
  e.slots.default = () => s;
}, Kr = (e, t, s) => {
  for (const n in t)
    (s || !rn(n)) && (e[n] = t[n]);
}, Io = (e, t, s) => {
  const n = e.slots = Nr();
  if (e.vnode.shapeFlag & 32) {
    const r = t._;
    r ? (Kr(n, t, s), s && Zn(n, "_", r, !0)) : $r(t, n);
  } else t && Lr(e, t);
}, Ro = (e, t, s) => {
  const { vnode: n, slots: r } = e;
  let i = !0, o = k;
  if (n.shapeFlag & 32) {
    const l = t._;
    l ? s && l === 1 ? i = !1 : Kr(r, t, s) : (i = !t.$stable, $r(t, r)), o = t;
  } else t && (Lr(e, t), o = { default: 1 });
  if (i)
    for (const l in r)
      !rn(l) && o[l] == null && delete r[l];
}, le = Do;
function Fo(e) {
  return Vo(e);
}
function Vo(e, t) {
  const s = ls();
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
    parentNode: h,
    nextSibling: O,
    setScopeId: A = Me,
    insertStaticContent: D
  } = e, V = (c, u, p, y = null, b = null, g = null, T = void 0, w = null, x = !!u.dynamicChildren) => {
    if (c === u)
      return;
    c && !vt(c, u) && (y = Wt(c), ye(c, b, g, !0), c = null), u.patchFlag === -2 && (x = !1, u.dynamicChildren = null);
    const { type: m, ref: I, shapeFlag: E } = u;
    switch (m) {
      case ps:
        C(c, u, p, y);
        break;
      case qe:
        _(c, u, p, y);
        break;
      case As:
        c == null && v(u, p, y, T);
        break;
      case ue:
        $t(
          c,
          u,
          p,
          y,
          b,
          g,
          T,
          w,
          x
        );
        break;
      default:
        E & 1 ? z(
          c,
          u,
          p,
          y,
          b,
          g,
          T,
          w,
          x
        ) : E & 6 ? Lt(
          c,
          u,
          p,
          y,
          b,
          g,
          T,
          w,
          x
        ) : (E & 64 || E & 128) && m.process(
          c,
          u,
          p,
          y,
          b,
          g,
          T,
          w,
          x,
          _t
        );
    }
    I != null && b ? Et(I, c && c.ref, g, u || c, !u) : I == null && c && c.ref != null && Et(c.ref, null, g, c, !0);
  }, C = (c, u, p, y) => {
    if (c == null)
      n(
        u.el = l(u.children),
        p,
        y
      );
    else {
      const b = u.el = c.el;
      u.children !== c.children && d(b, u.children);
    }
  }, _ = (c, u, p, y) => {
    c == null ? n(
      u.el = f(u.children || ""),
      p,
      y
    ) : u.el = c.el;
  }, v = (c, u, p, y) => {
    [c.el, c.anchor] = D(
      c.children,
      u,
      p,
      y,
      c.el,
      c.anchor
    );
  }, K = ({ el: c, anchor: u }, p, y) => {
    let b;
    for (; c && c !== u; )
      b = O(c), n(c, p, y), c = b;
    n(u, p, y);
  }, S = ({ el: c, anchor: u }) => {
    let p;
    for (; c && c !== u; )
      p = O(c), r(c), c = p;
    r(u);
  }, z = (c, u, p, y, b, g, T, w, x) => {
    if (u.type === "svg" ? T = "svg" : u.type === "math" && (T = "mathml"), c == null)
      _e(
        u,
        p,
        y,
        b,
        g,
        T,
        w,
        x
      );
    else {
      const m = c.el && c.el._isVueCE ? c.el : null;
      try {
        m && m._beginPatch(), Ht(
          c,
          u,
          b,
          g,
          T,
          w,
          x
        );
      } finally {
        m && m._endPatch();
      }
    }
  }, _e = (c, u, p, y, b, g, T, w) => {
    let x, m;
    const { props: I, shapeFlag: E, transition: P, dirs: F } = c;
    if (x = c.el = o(
      c.type,
      g,
      I && I.is,
      I
    ), E & 8 ? a(x, c.children) : E & 16 && ke(
      c.children,
      x,
      null,
      y,
      b,
      Os(c, g),
      T,
      w
    ), F && Xe(c, null, y, "created"), be(x, c, c.scopeId, T, y), I) {
      for (const W in I)
        W !== "value" && !St(W) && i(x, W, null, I[W], g, y);
      "value" in I && i(x, "value", null, I.value, g), (m = I.onVnodeBeforeMount) && Se(m, y, c);
    }
    F && Xe(c, null, y, "beforeMount");
    const N = Uo(b, P);
    N && P.beforeEnter(x), n(x, u, p), ((m = I && I.onVnodeMounted) || N || F) && le(() => {
      try {
        m && Se(m, y, c), N && P.enter(x), F && Xe(c, null, y, "mounted");
      } finally {
      }
    }, b);
  }, be = (c, u, p, y, b) => {
    if (p && A(c, p), y)
      for (let g = 0; g < y.length; g++)
        A(c, y[g]);
    if (b) {
      let g = b.subTree;
      if (u === g || Jr(g.type) && (g.ssContent === u || g.ssFallback === u)) {
        const T = b.vnode;
        be(
          c,
          T,
          T.scopeId,
          T.slotScopeIds,
          b.parent
        );
      }
    }
  }, ke = (c, u, p, y, b, g, T, w, x = 0) => {
    for (let m = x; m < c.length; m++) {
      const I = c[m] = w ? De(c[m]) : Ae(c[m]);
      V(
        null,
        I,
        u,
        p,
        y,
        b,
        g,
        T,
        w
      );
    }
  }, Ht = (c, u, p, y, b, g, T) => {
    const w = u.el = c.el;
    let { patchFlag: x, dynamicChildren: m, dirs: I } = u;
    x |= c.patchFlag & 16;
    const E = c.props || k, P = u.props || k;
    let F;
    if (p && Ze(p, !1), (F = P.onVnodeBeforeUpdate) && Se(F, p, u, c), I && Xe(u, c, p, "beforeUpdate"), p && Ze(p, !0), // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    m && (!c.dynamicChildren || c.dynamicChildren.length !== m.length) && (x = 0, T = !1, m = null), (E.innerHTML && P.innerHTML == null || E.textContent && P.textContent == null) && a(w, ""), m ? Ge(
      c.dynamicChildren,
      m,
      w,
      p,
      y,
      Os(u, b),
      g
    ) : T || B(
      c,
      u,
      w,
      null,
      p,
      y,
      Os(u, b),
      g,
      !1
    ), x > 0) {
      if (x & 16)
        gt(w, E, P, p, b);
      else if (x & 2 && E.class !== P.class && i(w, "class", null, P.class, b), x & 4 && i(w, "style", E.style, P.style, b), x & 8) {
        const N = u.dynamicProps;
        for (let W = 0; W < N.length; W++) {
          const $ = N[W], Y = E[$], X = P[$];
          (X !== Y || $ === "value") && i(w, $, Y, X, b, p);
        }
      }
      x & 1 && c.children !== u.children && a(w, u.children);
    } else !T && m == null && gt(w, E, P, p, b);
    ((F = P.onVnodeUpdated) || I) && le(() => {
      F && Se(F, p, u, c), I && Xe(u, c, p, "updated");
    }, y);
  }, Ge = (c, u, p, y, b, g, T) => {
    for (let w = 0; w < u.length; w++) {
      const x = c[w], m = u[w], I = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        x.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (x.type === ue || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !vt(x, m) || // - In the case of a component, it could contain anything.
        x.shapeFlag & 198) ? h(x.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          p
        )
      );
      V(
        x,
        m,
        I,
        null,
        y,
        b,
        g,
        T,
        !0
      );
    }
  }, gt = (c, u, p, y, b) => {
    if (u !== p) {
      if (u !== k)
        for (const g in u)
          !St(g) && !(g in p) && i(
            c,
            g,
            u[g],
            null,
            b,
            y
          );
      for (const g in p) {
        if (St(g)) continue;
        const T = p[g], w = u[g];
        T !== w && g !== "value" && i(c, g, w, T, b, y);
      }
      "value" in p && i(c, "value", u.value, p.value, b);
    }
  }, $t = (c, u, p, y, b, g, T, w, x) => {
    const m = u.el = c ? c.el : l(""), I = u.anchor = c ? c.anchor : l("");
    let { patchFlag: E, dynamicChildren: P, slotScopeIds: F } = u;
    F && (w = w ? w.concat(F) : F), c == null ? (n(m, p, y), n(I, p, y), ke(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      u.children || [],
      p,
      I,
      b,
      g,
      T,
      w,
      x
    )) : E > 0 && E & 64 && P && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    c.dynamicChildren && c.dynamicChildren.length === P.length ? (Ge(
      c.dynamicChildren,
      P,
      p,
      b,
      g,
      T,
      w
    ), // #2080 if the stable fragment has a key, it's a <template v-for> that may
    //  get moved around. Make sure all root level vnodes inherit el.
    // #2134 or if it's a component root, it may also get moved around
    // as the component is being moved.
    (u.key != null || b && u === b.subTree) && Wr(
      c,
      u,
      !0
      /* shallow */
    )) : B(
      c,
      u,
      p,
      I,
      b,
      g,
      T,
      w,
      x
    );
  }, Lt = (c, u, p, y, b, g, T, w, x) => {
    u.slotScopeIds = w, c == null ? u.shapeFlag & 512 ? b.ctx.activate(
      u,
      p,
      y,
      T,
      x
    ) : gs(
      u,
      p,
      y,
      b,
      g,
      T,
      x
    ) : ln(c, u, x);
  }, gs = (c, u, p, y, b, g, T) => {
    const w = c.component = Jo(
      c,
      y,
      b
    );
    if (Er(c) && (w.ctx.renderer = _t), Go(w, !1, T), w.asyncDep) {
      if (b && b.registerDep(w, se, T), !c.el) {
        const x = w.subTree = $e(qe);
        _(null, x, u, p), c.placeholder = x.el;
      }
    } else
      se(
        w,
        c,
        u,
        p,
        b,
        g,
        T
      );
  }, ln = (c, u, p) => {
    const y = u.component = c.component;
    if (To(c, u, p))
      if (y.asyncDep && !y.asyncResolved) {
        q(y, u, p);
        return;
      } else
        y.next = u, y.update();
    else
      u.el = c.el, y.vnode = u;
  }, se = (c, u, p, y, b, g, T) => {
    const w = () => {
      if (c.isMounted) {
        let { next: E, bu: P, u: F, parent: N, vnode: W } = c;
        {
          const xe = kr(c);
          if (xe) {
            E && (E.el = W.el, q(c, E, T)), xe.asyncDep.then(() => {
              le(() => {
                c.isUnmounted || m();
              }, b);
            });
            return;
          }
        }
        let $ = E, Y;
        Ze(c, !1), E ? (E.el = W.el, q(c, E, T)) : E = W, P && qt(P), (Y = E.props && E.props.onVnodeBeforeUpdate) && Se(Y, N, E, W), Ze(c, !0);
        const X = Cn(c), ve = c.subTree;
        c.subTree = X, V(
          ve,
          X,
          // parent may have changed if it's in a teleport
          h(ve.el),
          // anchor may have changed if it's in a fragment
          Wt(ve),
          c,
          b,
          g
        ), E.el = X.el, $ === null && Eo(c, X.el), F && le(F, b), (Y = E.props && E.props.onVnodeUpdated) && le(
          () => Se(Y, N, E, W),
          b
        );
      } else {
        let E;
        const { el: P, props: F } = u, { bm: N, m: W, parent: $, root: Y, type: X } = c, ve = Ot(u);
        Ze(c, !1), N && qt(N), !ve && (E = F && F.onVnodeBeforeMount) && Se(E, $, u), Ze(c, !0);
        {
          Y.ce && Y.ce._hasShadowRoot() && Y.ce._injectChildStyle(
            X,
            c.parent ? c.parent.type : void 0
          );
          const xe = c.subTree = Cn(c);
          V(
            null,
            xe,
            p,
            y,
            c,
            b,
            g
          ), u.el = xe.el;
        }
        if (W && le(W, b), !ve && (E = F && F.onVnodeMounted)) {
          const xe = u;
          le(
            () => Se(E, $, xe),
            b
          );
        }
        (u.shapeFlag & 256 || $ && Ot($.vnode) && $.vnode.shapeFlag & 256) && c.a && le(c.a, b), c.isMounted = !0, u = p = y = null;
      }
    };
    c.scope.on();
    const x = c.effect = new sr(w);
    c.scope.off();
    const m = c.update = x.run.bind(x), I = c.job = x.runIfDirty.bind(x);
    I.i = c, I.id = c.uid, x.scheduler = () => sn(I), Ze(c, !0), m();
  }, q = (c, u, p) => {
    u.component = c;
    const y = c.vnode.props;
    c.vnode = u, c.next = null, Ao(c, u.props, y, p), Ro(c, u.children, p), Re(), _n(c), Fe();
  }, B = (c, u, p, y, b, g, T, w, x = !1) => {
    const m = c && c.children, I = c ? c.shapeFlag : 0, E = u.children, { patchFlag: P, shapeFlag: F } = u;
    if (P > 0) {
      if (P & 128) {
        Kt(
          m,
          E,
          p,
          y,
          b,
          g,
          T,
          w,
          x
        );
        return;
      } else if (P & 256) {
        Ye(
          m,
          E,
          p,
          y,
          b,
          g,
          T,
          w,
          x
        );
        return;
      }
    }
    F & 8 ? (I & 16 && mt(m, b, g), E !== m && a(p, E)) : I & 16 ? F & 16 ? Kt(
      m,
      E,
      p,
      y,
      b,
      g,
      T,
      w,
      x
    ) : mt(m, b, g, !0) : (I & 8 && a(p, ""), F & 16 && ke(
      E,
      p,
      y,
      b,
      g,
      T,
      w,
      x
    ));
  }, Ye = (c, u, p, y, b, g, T, w, x) => {
    c = c || ot, u = u || ot;
    const m = c.length, I = u.length, E = Math.min(m, I);
    let P;
    for (P = 0; P < E; P++) {
      const F = u[P] = x ? De(u[P]) : Ae(u[P]);
      V(
        c[P],
        F,
        p,
        null,
        b,
        g,
        T,
        w,
        x
      );
    }
    m > I ? mt(
      c,
      b,
      g,
      !0,
      !1,
      E
    ) : ke(
      u,
      p,
      y,
      b,
      g,
      T,
      w,
      x,
      E
    );
  }, Kt = (c, u, p, y, b, g, T, w, x) => {
    let m = 0;
    const I = u.length;
    let E = c.length - 1, P = I - 1;
    for (; m <= E && m <= P; ) {
      const F = c[m], N = u[m] = x ? De(u[m]) : Ae(u[m]);
      if (vt(F, N))
        V(
          F,
          N,
          p,
          null,
          b,
          g,
          T,
          w,
          x
        );
      else
        break;
      m++;
    }
    for (; m <= E && m <= P; ) {
      const F = c[E], N = u[P] = x ? De(u[P]) : Ae(u[P]);
      if (vt(F, N))
        V(
          F,
          N,
          p,
          null,
          b,
          g,
          T,
          w,
          x
        );
      else
        break;
      E--, P--;
    }
    if (m > E) {
      if (m <= P) {
        const F = P + 1, N = F < I ? u[F].el : y;
        for (; m <= P; )
          V(
            null,
            u[m] = x ? De(u[m]) : Ae(u[m]),
            p,
            N,
            b,
            g,
            T,
            w,
            x
          ), m++;
      }
    } else if (m > P)
      for (; m <= E; )
        ye(c[m], b, g, !0), m++;
    else {
      const F = m, N = m, W = /* @__PURE__ */ new Map();
      for (m = N; m <= P; m++) {
        const ce = u[m] = x ? De(u[m]) : Ae(u[m]);
        ce.key != null && W.set(ce.key, m);
      }
      let $, Y = 0;
      const X = P - N + 1;
      let ve = !1, xe = 0;
      const bt = new Array(X);
      for (m = 0; m < X; m++) bt[m] = 0;
      for (m = F; m <= E; m++) {
        const ce = c[m];
        if (Y >= X) {
          ye(ce, b, g, !0);
          continue;
        }
        let we;
        if (ce.key != null)
          we = W.get(ce.key);
        else
          for ($ = N; $ <= P; $++)
            if (bt[$ - N] === 0 && vt(ce, u[$])) {
              we = $;
              break;
            }
        we === void 0 ? ye(ce, b, g, !0) : (bt[we - N] = m + 1, we >= xe ? xe = we : ve = !0, V(
          ce,
          u[we],
          p,
          null,
          b,
          g,
          T,
          w,
          x
        ), Y++);
      }
      const un = ve ? No(bt) : ot;
      for ($ = un.length - 1, m = X - 1; m >= 0; m--) {
        const ce = N + m, we = u[ce], an = u[ce + 1], dn = ce + 1 < I ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          an.el || Br(an)
        ) : y;
        bt[m] === 0 ? V(
          null,
          we,
          p,
          dn,
          b,
          g,
          T,
          w,
          x
        ) : ve && ($ < 0 || m !== un[$] ? ze(we, p, dn, 2) : $--);
      }
    }
  }, ze = (c, u, p, y, b = null) => {
    const { el: g, type: T, transition: w, children: x, shapeFlag: m } = c;
    if (m & 6) {
      ze(c.component.subTree, u, p, y);
      return;
    }
    if (m & 128) {
      c.suspense.move(u, p, y);
      return;
    }
    if (m & 64) {
      T.move(c, u, p, _t);
      return;
    }
    if (T === ue) {
      n(g, u, p);
      for (let E = 0; E < x.length; E++)
        ze(x[E], u, p, y);
      n(c.anchor, u, p);
      return;
    }
    if (T === As) {
      K(c, u, p);
      return;
    }
    if (y !== 2 && m & 1 && w)
      if (y === 0)
        w.persisted && !g[Cs] ? n(g, u, p) : (w.beforeEnter(g), n(g, u, p), le(() => w.enter(g), b));
      else {
        const { leave: E, delayLeave: P, afterLeave: F } = w, N = () => {
          c.ctx.isUnmounted ? r(g) : n(g, u, p);
        }, W = () => {
          const $ = g._isLeaving || !!g[Cs];
          g._isLeaving && g[Cs](
            !0
            /* cancelled */
          ), w.persisted && !$ ? N() : E(g, () => {
            N(), F && F();
          });
        };
        P ? P(g, N, W) : W();
      }
    else
      n(g, u, p);
  }, ye = (c, u, p, y = !1, b = !1) => {
    const {
      type: g,
      props: T,
      ref: w,
      children: x,
      dynamicChildren: m,
      shapeFlag: I,
      patchFlag: E,
      dirs: P,
      cacheIndex: F,
      memo: N
    } = c;
    if (E === -2 && (b = !1), w != null && (Re(), Et(w, null, p, c, !0), Fe()), F != null && (u.renderCache[F] = void 0), I & 256) {
      u.ctx.deactivate(c);
      return;
    }
    const W = I & 1 && P, $ = !Ot(c);
    let Y;
    if ($ && (Y = T && T.onVnodeBeforeUnmount) && Se(Y, u, c), I & 6)
      ti(c.component, p, y);
    else {
      if (I & 128) {
        c.suspense.unmount(p, y);
        return;
      }
      W && Xe(c, null, u, "beforeUnmount"), I & 64 ? c.type.remove(
        c,
        u,
        p,
        _t,
        y
      ) : m && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !m.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (g !== ue || E > 0 && E & 64) ? mt(
        m,
        u,
        p,
        !1,
        !0
      ) : (g === ue && E & 384 || !b && I & 16) && mt(x, u, p), y && cn(c);
    }
    const X = N != null && F == null;
    ($ && (Y = T && T.onVnodeUnmounted) || W || X) && le(() => {
      Y && Se(Y, u, c), W && Xe(c, null, u, "unmounted"), X && (c.el = null);
    }, p);
  }, cn = (c) => {
    const { type: u, el: p, anchor: y, transition: b } = c;
    if (u === ue) {
      ei(p, y);
      return;
    }
    if (u === As) {
      S(c);
      return;
    }
    const g = () => {
      r(p), b && !b.persisted && b.afterLeave && b.afterLeave();
    };
    if (c.shapeFlag & 1 && b && !b.persisted) {
      const { leave: T, delayLeave: w } = b, x = () => T(p, g);
      w ? w(c.el, g, x) : x();
    } else
      g();
  }, ei = (c, u) => {
    let p;
    for (; c !== u; )
      p = O(c), r(c), c = p;
    r(u);
  }, ti = (c, u, p) => {
    const { bum: y, scope: b, job: g, subTree: T, um: w, m: x, a: m } = c;
    On(x), On(m), y && qt(y), b.stop(), g && (g.flags |= 8, ye(T, c, u, p)), w && le(w, u), le(() => {
      c.isUnmounted = !0;
    }, u);
  }, mt = (c, u, p, y = !1, b = !1, g = 0) => {
    for (let T = g; T < c.length; T++)
      ye(c[T], u, p, y, b);
  }, Wt = (c) => {
    if (c.shapeFlag & 6)
      return Wt(c.component.subTree);
    if (c.shapeFlag & 128)
      return c.suspense.next();
    const u = O(c.anchor || c.el), p = u && u[zi];
    return p ? O(p) : u;
  };
  let ms = !1;
  const fn = (c, u, p) => {
    let y;
    c == null ? u._vnode && (ye(u._vnode, null, null, !0), y = u._vnode.component) : V(
      u._vnode || null,
      c,
      u,
      null,
      null,
      null,
      p
    ), u._vnode = c, ms || (ms = !0, _n(y), vr(), ms = !1);
  }, _t = {
    p: V,
    um: ye,
    m: ze,
    r: cn,
    mt: gs,
    mc: ke,
    pc: B,
    pbc: Ge,
    n: Wt,
    o: e
  };
  return {
    render: fn,
    hydrate: void 0,
    createApp: yo(fn)
  };
}
function Os({ type: e, props: t }, s) {
  return s === "svg" && e === "foreignObject" || s === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : s;
}
function Ze({ effect: e, job: t }, s) {
  s ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function Uo(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Wr(e, t, s = !1) {
  const n = e.children, r = t.children;
  if (R(n) && R(r))
    for (let i = 0; i < n.length; i++) {
      const o = n[i];
      let l = r[i];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = r[i] = De(r[i]), l.el = o.el), !s && l.patchFlag !== -2 && Wr(o, l)), l.type === ps && (l.patchFlag === -1 && (l = r[i] = De(l)), l.el = o.el), l.type === qe && !l.el && (l.el = o.el);
    }
}
function No(e) {
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
function kr(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : kr(t);
}
function On(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function Br(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? Br(t.subTree) : null;
}
const Jr = (e) => e.__isSuspense;
function Do(e, t) {
  t && t.pendingBranch ? R(e) ? t.effects.push(...e) : t.effects.push(e) : ki(e);
}
const ue = /* @__PURE__ */ Symbol.for("v-fgt"), ps = /* @__PURE__ */ Symbol.for("v-txt"), qe = /* @__PURE__ */ Symbol.for("v-cmt"), As = /* @__PURE__ */ Symbol.for("v-stc"), Pt = [];
let fe = null;
function Te(e = !1) {
  Pt.push(fe = e ? null : []);
}
function jo() {
  Pt.pop(), fe = Pt[Pt.length - 1] || null;
}
let Ft = 1;
function An(e, t = !1) {
  Ft += e, e < 0 && fe && t && (fe.hasOnce = !0);
}
function qr(e) {
  return e.dynamicChildren = Ft > 0 ? fe || ot : null, jo(), Ft > 0 && fe && fe.push(e), e;
}
function Ue(e, t, s, n, r, i) {
  return qr(
    M(
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
function Ho(e, t, s, n, r) {
  return qr(
    $e(
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
function vt(e, t) {
  return e.type === t.type && e.key === t.key;
}
const Yr = ({ key: e }) => e ?? null, Yt = ({
  ref: e,
  ref_key: t,
  ref_for: s
}) => (typeof e == "number" && (e = "" + e), e != null ? G(e) || /* @__PURE__ */ te(e) || U(e) ? { i: ae, r: e, k: t, f: !!s } : e : null);
function M(e, t = null, s = null, n = 0, r = null, i = e === ue ? 0 : 1, o = !1, l = !1) {
  const f = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Yr(t),
    ref: t && Yt(t),
    scopeId: wr,
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
    ctx: ae
  };
  return l ? (ts(f, s), i & 128 && e.normalize(f)) : s && (f.shapeFlag |= G(s) ? 8 : 16), Ft > 0 && // avoid a block node from tracking itself
  !o && // has current parent block
  fe && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (f.patchFlag > 0 || i & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  f.patchFlag !== 32 && fe.push(f), f;
}
const $e = $o;
function $o(e, t = null, s = null, n = 0, r = null, i = !1) {
  if ((!e || e === uo) && (e = qe), Gr(e)) {
    const l = at(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return s && ts(l, s), Ft > 0 && !i && fe && (l.shapeFlag & 6 ? fe[fe.indexOf(e)] = l : fe.push(l)), l.patchFlag = -2, l;
  }
  if (Zo(e) && (e = e.__vccOpts), t) {
    t = Lo(t);
    let { class: l, style: f } = t;
    l && !G(l) && (t.class = qs(l)), L(f) && (/* @__PURE__ */ tn(f) && !R(f) && (f = Q({}, f)), t.style = Js(f));
  }
  const o = G(e) ? 1 : Jr(e) ? 128 : Xi(e) ? 64 : L(e) ? 4 : U(e) ? 2 : 0;
  return M(
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
  return e ? /* @__PURE__ */ tn(e) || Dr(e) ? Q({}, e) : e : null;
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
      s && i ? R(i) ? i.concat(Yt(t)) : [i, Yt(t)] : Yt(t)
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
  return f && n && nn(
    a,
    f.clone(a)
  ), a;
}
function Ko(e = " ", t = 0) {
  return $e(ps, null, e, t);
}
function Pn(e = "", t = !1) {
  return t ? (Te(), Ho(qe, null, e)) : $e(qe, null, e);
}
function Ae(e) {
  return e == null || typeof e == "boolean" ? $e(qe) : R(e) ? $e(
    ue,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : Gr(e) ? De(e) : $e(ps, null, String(e));
}
function De(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : at(e);
}
function ts(e, t) {
  let s = 0;
  const { shapeFlag: n } = e;
  if (t == null)
    t = null;
  else if (R(t))
    s = 16;
  else if (typeof t == "object")
    if (n & 65) {
      const r = t.default;
      r && (r._c && (r._d = !1), ts(e, r()), r._c && (r._d = !0));
      return;
    } else {
      s = 32;
      const r = t._;
      !r && !Dr(t) ? t._ctx = ae : r === 3 && ae && (ae.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (U(t)) {
    if (n & 65) {
      ts(e, { default: t });
      return;
    }
    t = { default: t, _ctx: ae }, s = 32;
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
        t.class !== n.class && (t.class = qs([t.class, n.class]));
      else if (r === "style")
        t.style = Js([t.style, n.style]);
      else if (ns(r)) {
        const i = t[r], o = n[r];
        o && i !== o && !(R(i) && i.includes(o)) ? t[r] = i ? [].concat(i, o) : o : o == null && i == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !rs(r) && (t[r] = o);
      } else r !== "" && (t[r] = n[r]);
  }
  return t;
}
function Se(e, t, s, n = null) {
  me(e, t, 7, [
    s,
    n
  ]);
}
const ko = Rr();
let Bo = 0;
function Jo(e, t, s) {
  const n = e.type, r = (t ? t.appContext : e.appContext) || ko, i = {
    uid: Bo++,
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
    propsOptions: Hr(n, r),
    emitsOptions: Fr(n, r),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: k,
    // inheritAttrs
    inheritAttrs: n.inheritAttrs,
    // state
    ctx: k,
    data: k,
    props: k,
    attrs: k,
    slots: k,
    refs: k,
    setupState: k,
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
let ie = null;
const qo = () => ie || ae;
let ss, Ls;
{
  const e = ls(), t = (s, n) => {
    let r;
    return (r = e[s]) || (r = e[s] = []), r.push(n), (i) => {
      r.length > 1 ? r.forEach((o) => o(i)) : r[0](i);
    };
  };
  ss = t(
    "__VUE_INSTANCE_SETTERS__",
    (s) => ie = s
  ), Ls = t(
    "__VUE_SSR_SETTERS__",
    (s) => Vt = s
  );
}
const jt = (e) => {
  const t = ie;
  return ss(e), e.scope.on(), () => {
    e.scope.off(), ss(t);
  };
}, Mn = () => {
  ie && ie.scope.off(), ss(null);
};
function zr(e) {
  return e.vnode.shapeFlag & 4;
}
let Vt = !1;
function Go(e, t = !1, s = !1) {
  t && Ls(t);
  const { props: n, children: r } = e.vnode, i = zr(e);
  Oo(e, n, i, t), Io(e, r, s || t);
  const o = i ? Yo(e, t) : void 0;
  return t && Ls(!1), o;
}
function Yo(e, t) {
  const s = e.type;
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, ao);
  const { setup: n } = s;
  if (n) {
    Re();
    const r = e.setupContext = n.length > 1 ? Xo(e) : null, i = jt(e), o = Dt(
      n,
      e,
      0,
      [
        e.props,
        r
      ]
    ), l = Gn(o);
    if (Fe(), i(), (l || e.sp) && !Ot(e) && Tr(e), l) {
      if (o.then(Mn, Mn), t)
        return o.then((f) => {
          In(e, f);
        }).catch((f) => {
          us(f, e, 0);
        });
      e.asyncDep = o;
    } else
      In(e, o);
  } else
    Xr(e);
}
function In(e, t, s) {
  U(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : L(t) && (e.setupState = mr(t)), Xr(e);
}
function Xr(e, t, s) {
  const n = e.type;
  e.render || (e.render = n.render || Me);
  {
    const r = jt(e);
    Re();
    try {
      po(e);
    } finally {
      Fe(), r();
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
function hs(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(mr(Fi(e.exposed)), {
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
  return U(e) && "__vccOpts" in e;
}
const Qo = (e, t) => /* @__PURE__ */ Hi(e, t, Vt), el = "3.5.39";
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Ks;
const Rn = typeof window < "u" && window.trustedTypes;
if (Rn)
  try {
    Ks = /* @__PURE__ */ Rn.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch {
  }
const Zr = Ks ? (e) => Ks.createHTML(e) : (e) => e, tl = "http://www.w3.org/2000/svg", sl = "http://www.w3.org/1998/Math/MathML", Ne = typeof document < "u" ? document : null, Fn = Ne && /* @__PURE__ */ Ne.createElement("template"), nl = {
  insert: (e, t, s) => {
    t.insertBefore(e, s || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, s, n) => {
    const r = t === "svg" ? Ne.createElementNS(tl, e) : t === "mathml" ? Ne.createElementNS(sl, e) : s ? Ne.createElement(e, { is: s }) : Ne.createElement(e);
    return e === "select" && n && n.multiple != null && r.setAttribute("multiple", n.multiple), r;
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
  insertStaticContent(e, t, s, n, r, i) {
    const o = s ? s.previousSibling : t.lastChild;
    if (r && (r === i || r.nextSibling))
      for (; t.insertBefore(r.cloneNode(!0), s), !(r === i || !(r = r.nextSibling)); )
        ;
    else {
      Fn.innerHTML = Zr(
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
  const n = e.style, r = G(s);
  let i = !1;
  if (s && !r) {
    if (t)
      if (G(t))
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
        !G(t) && t ? t[o] : void 0,
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
  if (R(s))
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
const Nn = ["Webkit", "Moz", "ms"], Ps = {};
function ul(e, t) {
  const s = Ps[t];
  if (s)
    return s;
  let n = pe(t);
  if (n !== "filter" && n in e)
    return Ps[t] = n;
  n = Xn(n);
  for (let r = 0; r < Nn.length; r++) {
    const i = Nn[r] + n;
    if (i in e)
      return Ps[t] = i;
  }
  return t;
}
function al(e, t, s, n) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && G(n) && s === n;
}
const Dn = "http://www.w3.org/1999/xlink";
function jn(e, t, s, n, r, i = ai(t)) {
  n && t.startsWith("xlink:") ? s == null ? e.removeAttributeNS(Dn, t.slice(6, t.length)) : e.setAttributeNS(Dn, t, s) : s == null || i && !Qn(s) ? e.removeAttribute(t) : e.setAttribute(
    t,
    i ? "" : Ie(s) ? String(s) : s
  );
}
function Hn(e, t, s, n, r) {
  if (t === "innerHTML" || t === "textContent") {
    s != null && (e[t] = t === "innerHTML" ? Zr(s) : s);
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
    l === "boolean" ? s = Qn(s) : s == null && l === "string" ? (s = "", o = !0) : l === "number" && (s = 0, o = !0);
  }
  try {
    e[t] = s;
  } catch {
  }
  o && e.removeAttribute(r || t);
}
function Je(e, t, s, n) {
  e.addEventListener(t, s, n);
}
function dl(e, t, s, n) {
  e.removeEventListener(t, s, n);
}
const $n = /* @__PURE__ */ Symbol("_vei");
function pl(e, t, s, n, r = null) {
  const i = e[$n] || (e[$n] = {}), o = i[t];
  if (n && o)
    o.value = n;
  else {
    const [l, f] = ml(t);
    if (n) {
      const d = i[t] = yl(
        n,
        r
      );
      Je(e, l, d, f);
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
let Ms = 0;
const _l = /* @__PURE__ */ Promise.resolve(), bl = () => Ms || (_l.then(() => Ms = 0), Ms = Date.now());
function yl(e, t) {
  const s = (n) => {
    if (!n._vts)
      n._vts = Date.now();
    else if (n._vts <= s.attached)
      return;
    const r = s.value;
    if (R(r)) {
      const i = n.stopImmediatePropagation;
      n.stopImmediatePropagation = () => {
        i.call(n), n._stopped = !0;
      };
      const o = r.slice(), l = [n];
      for (let f = 0; f < o.length && !n._stopped; f++) {
        const d = o[f];
        d && me(
          d,
          t,
          5,
          l
        );
      }
    } else
      me(
        r,
        t,
        5,
        [n]
      );
  };
  return s.value = e, s.attached = bl(), s;
}
const Ln = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, vl = (e, t, s, n, r, i) => {
  const o = r === "svg";
  t === "class" ? il(e, n, o) : t === "style" ? fl(e, s, n) : ns(t) ? rs(t) || pl(e, t, s, n, i) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : xl(e, t, n, o)) ? (Hn(e, t, n), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && jn(e, t, n, o, i, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (wl(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !G(n))) ? Hn(e, pe(t), n, i, t) : (t === "true-value" ? e._trueValue = n : t === "false-value" && (e._falseValue = n), jn(e, t, n, o));
};
function xl(e, t, s, n) {
  if (n)
    return !!(t === "innerHTML" || t === "textContent" || t in e && Ln(t) && U(s));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const r = e.tagName;
    if (r === "IMG" || r === "VIDEO" || r === "CANVAS" || r === "SOURCE")
      return !1;
  }
  return Ln(t) && G(s) ? !1 : t in e;
}
function wl(e, t) {
  const s = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!s)
    return !1;
  const n = pe(t);
  return Array.isArray(s) ? s.some((r) => pe(r) === n) : Object.keys(s).some((r) => pe(r) === n);
}
const dt = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return R(t) ? (s) => qt(t, s) : t;
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
  return t && (e = e.trim()), s && (e = os(e)), e;
}
const Ce = {
  created(e, { modifiers: { lazy: t, trim: s, number: n } }, r) {
    e[Le] = dt(r);
    const i = n || r.props && r.props.type === "number";
    Je(e, t ? "change" : "input", (o) => {
      o.target.composing || e[Le](Wn(e.value, s, i));
    }), (s || i) && Je(e, "change", () => {
      e.value = Wn(e.value, s, i);
    }), t || (Je(e, "compositionstart", Sl), Je(e, "compositionend", Kn), Je(e, "change", Kn));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: s, modifiers: { lazy: n, trim: r, number: i } }, o) {
    if (e[Le] = dt(o), e.composing) return;
    const l = (i || e.type === "number") && !/^0\d/.test(e.value) ? os(e.value) : e.value, f = t ?? "";
    if (l === f)
      return;
    const d = e.getRootNode();
    (d instanceof Document || d instanceof ShadowRoot) && d.activeElement === e && e.type !== "range" && (n && t === s || r && e.value.trim() === f) || (e.value = f);
  }
}, Cl = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, s) {
    e[Le] = dt(s), Je(e, "change", () => {
      const n = e._modelValue, r = Ut(e), i = e.checked, o = e[Le];
      if (R(n)) {
        const l = Gs(n, r), f = l !== -1;
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
        o(Qr(e, i));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: kn,
  beforeUpdate(e, t, s) {
    e[Le] = dt(s), kn(e, t, s);
  }
};
function kn(e, { value: t, oldValue: s }, n) {
  e._modelValue = t;
  let r;
  if (R(t))
    r = Gs(t, n.props.value) > -1;
  else if (pt(t))
    r = t.has(n.props.value);
  else {
    if (t === s) return;
    r = ht(t, Qr(e, !0));
  }
  e.checked !== r && (e.checked = r);
}
const Is = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: s } }, n) {
    const r = pt(t);
    Je(e, "change", () => {
      const i = Array.prototype.filter.call(e.options, (o) => o.selected).map(
        (o) => s ? os(Ut(o)) : Ut(o)
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
    Bn(e, t);
  },
  beforeUpdate(e, t, s) {
    e[Le] = dt(s);
  },
  updated(e, { value: t }) {
    e._assigning || Bn(e, t);
  }
};
function Bn(e, t) {
  const s = e.multiple, n = R(t);
  if (!(s && !n && !pt(t))) {
    for (let r = 0, i = e.options.length; r < i; r++) {
      const o = e.options[r], l = Ut(o);
      if (s)
        if (n) {
          const f = typeof l;
          f === "string" || f === "number" ? o.selected = t.some((d) => String(d) === String(l)) : o.selected = Gs(t, l) > -1;
        } else
          o.selected = t.has(l);
      else if (ht(Ut(o), t)) {
        e.selectedIndex !== r && (e.selectedIndex = r);
        return;
      }
    }
    !s && e.selectedIndex !== -1 && (e.selectedIndex = -1);
  }
}
function Ut(e) {
  return "_value" in e ? e._value : e.value;
}
function Qr(e, t) {
  const s = t ? "_trueValue" : "_falseValue";
  return s in e ? e[s] : t;
}
const Tl = /* @__PURE__ */ Q({ patchProp: vl }, nl);
let Jn;
function El() {
  return Jn || (Jn = Fo(Tl));
}
const Ol = ((...e) => {
  const t = El().createApp(...e), { mount: s } = t;
  return t.mount = (n) => {
    const r = Pl(n);
    if (!r) return;
    const i = t._component;
    !U(i) && !i.render && !i.template && (i.template = r.innerHTML), r.nodeType === 1 && (r.textContent = "");
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
  return G(e) ? document.querySelector(e) : e;
}
const Ml = { class: "ssh-set" }, Il = { class: "row" }, Rl = ["value"], Fl = {
  key: 0,
  class: "muted empty"
}, Vl = { class: "row spread" }, Ul = { class: "row" }, Nl = ["onUpdate:modelValue"], Dl = ["onUpdate:modelValue"], jl = ["onUpdate:modelValue"], Hl = ["onClick"], $l = { class: "row" }, Ll = ["onUpdate:modelValue"], Kl = ["value"], Wl = ["onClick"], kl = {
  key: 0,
  class: "row new-cred"
}, Bl = ["onUpdate:modelValue"], Jl = ["onUpdate:modelValue"], ql = ["onUpdate:modelValue"], Gl = ["onUpdate:modelValue"], Yl = ["disabled", "onClick"], zl = { class: "muted" }, Xl = { class: "row" }, Zl = ["onUpdate:modelValue", "placeholder"], Ql = ["onUpdate:modelValue"], ec = { class: "row" }, tc = ["onUpdate:modelValue"], sc = { class: "row" }, nc = { class: "chk" }, rc = ["onUpdate:modelValue"], ic = { class: "row" }, oc = ["disabled", "onClick"], lc = { class: "muted" }, cc = /* @__PURE__ */ Zi({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const s = e;
    let n = 0;
    function r(C, _) {
      return {
        key: n++,
        name: C,
        host: _.host || "",
        port: _.port || 22,
        user: _.user || "",
        credential: _.credential || "",
        password: _.password || "",
        keyFile: _.key_file || "",
        keyPassphrase: _.key_passphrase || "",
        description: _.description || "",
        allowWrite: !!_.allow_write,
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
    function i(C) {
      return {
        host: C.host || void 0,
        port: C.port === 22 ? void 0 : C.port,
        user: C.user || void 0,
        credential: C.credential || void 0,
        // Legacy single-value pointers stay untouched if they were in the blob and no credential is set.
        password: C.credential ? void 0 : C.password || void 0,
        key_file: C.keyFile || void 0,
        key_passphrase: C.credential ? void 0 : C.keyPassphrase || void 0,
        description: C.description || void 0,
        allow_write: C.allowWrite || void 0
      };
    }
    const o = (() => {
      try {
        return JSON.parse(s.api.getJson() || "null") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ ws(o.default_host || ""), f = /* @__PURE__ */ ws(o.timeout_seconds || 20), d = /* @__PURE__ */ fs(
      Object.entries(o.hosts || {}).map(([C, _]) => r(C, _))
    ), a = /* @__PURE__ */ ws([]);
    async function h() {
      try {
        const C = await s.api.invoke("secret.list");
        a.value = [...C.user || [], ...C.project || [], ...C.shared || []].map((_) => ({ reference: _.reference, label: `${_.key} · ${_.scope}` })).sort((_, v) => _.label.localeCompare(v.label));
      } catch {
      }
    }
    Ar(h);
    function O() {
      d.push(r(`host${d.length + 1}`, {}));
    }
    async function A(C) {
      C.credStatus = "Saving…";
      try {
        const _ = { password: C.credPassword };
        C.credUser && (_.user = C.credUser), await s.api.invoke("secret.set", { key: C.credName.trim(), fields: _, scope: C.credScope }), C.credential = `secret:${C.credScope}:${C.credName.trim()}`, C.newCred = !1, C.credName = "", C.credUser = "", C.credPassword = "", C.credScope = "", C.credStatus = "", await h();
      } catch (_) {
        C.credStatus = "Failed: " + (_ instanceof Error ? _.message : String(_));
      }
    }
    async function D(C) {
      C.testing = !0, C.testStatus = "Connecting…";
      try {
        const _ = await s.api.invoke("plugin.action", {
          pluginId: "ssh",
          action: "testHost",
          valueJson: JSON.stringify({
            host: C.host,
            port: C.port,
            user: C.user || void 0,
            credential: C.credential || void 0,
            password: C.credential ? void 0 : C.password || void 0,
            keyFile: C.keyFile || void 0
          })
        });
        if (_.ok && _.resultJson) {
          const v = JSON.parse(_.resultJson);
          C.testStatus = v.message;
        } else
          C.testStatus = "Failed: " + (_.error || "unknown error");
      } catch (_) {
        C.testStatus = "Failed: " + (_ instanceof Error ? _.message : String(_));
      } finally {
        C.testing = !1;
      }
    }
    function V() {
      const C = {
        default_host: l.value || void 0,
        timeout_seconds: f.value || 20,
        hosts: Object.fromEntries(
          d.filter((_) => _.name.trim()).map((_) => [_.name.trim(), i(_)])
        )
      };
      return JSON.stringify(C);
    }
    return t({ toJson: V }), (C, _) => (Te(), Ue("div", Ml, [
      _[17] || (_[17] = M("div", { class: "muted" }, " Named SSH hosts available to the agent and the terminal. Passwords/keys live in the secret store (Settings → Secrets); a host only references an entry by name. ", -1)),
      M("div", Il, [
        M("label", null, [
          _[3] || (_[3] = M("span", { class: "muted" }, "Default host", -1)),
          oe(M("select", {
            "onUpdate:modelValue": _[0] || (_[0] = (v) => l.value = v)
          }, [
            _[2] || (_[2] = M("option", { value: "" }, "(none)", -1)),
            (Te(!0), Ue(ue, null, Ts(d, (v) => (Te(), Ue("option", {
              key: v.key,
              value: v.name
            }, rt(v.name), 9, Rl))), 128))
          ], 512), [
            [Is, l.value]
          ])
        ]),
        M("label", null, [
          _[4] || (_[4] = M("span", { class: "muted" }, "Timeout, s", -1)),
          oe(M("input", {
            "onUpdate:modelValue": _[1] || (_[1] = (v) => f.value = v),
            type: "number",
            min: "5",
            max: "120",
            class: "w-70"
          }, null, 512), [
            [
              Ce,
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
        onClick: O
      }, "+ Add host"),
      d.length ? Pn("", !0) : (Te(), Ue("div", Fl, 'No hosts yet. Click "+ Add host".')),
      (Te(!0), Ue(ue, null, Ts(d, (v, K) => (Te(), Ue("div", {
        key: v.key,
        class: "host-card"
      }, [
        M("div", Vl, [
          M("div", Ul, [
            _[5] || (_[5] = M("span", { class: "muted" }, "Name", -1)),
            oe(M("input", {
              "onUpdate:modelValue": (S) => v.name = S,
              class: "w-120",
              spellcheck: "false"
            }, null, 8, Nl), [
              [Ce, v.name]
            ]),
            _[6] || (_[6] = M("span", { class: "muted" }, "Host", -1)),
            oe(M("input", {
              "onUpdate:modelValue": (S) => v.host = S,
              placeholder: "10.0.0.5 or box.local",
              class: "w-180",
              spellcheck: "false"
            }, null, 8, Dl), [
              [Ce, v.host]
            ]),
            _[7] || (_[7] = M("span", { class: "muted" }, "Port", -1)),
            oe(M("input", {
              "onUpdate:modelValue": (S) => v.port = S,
              type: "number",
              min: "1",
              max: "65535",
              class: "w-70"
            }, null, 8, jl), [
              [
                Ce,
                v.port,
                void 0,
                { number: !0 }
              ]
            ])
          ]),
          M("button", {
            type: "button",
            onClick: (S) => d.splice(K, 1)
          }, "✕ Remove", 8, Hl)
        ]),
        M("div", $l, [
          _[9] || (_[9] = M("span", { class: "muted w-label" }, "Credential", -1)),
          oe(M("select", {
            "onUpdate:modelValue": (S) => v.credential = S
          }, [
            _[8] || (_[8] = M("option", { value: "" }, "(none — use fields below)", -1)),
            (Te(!0), Ue(ue, null, Ts(a.value, (S) => (Te(), Ue("option", {
              key: S.reference,
              value: S.reference
            }, rt(S.label), 9, Kl))), 128))
          ], 8, Ll), [
            [Is, v.credential]
          ]),
          M("button", {
            type: "button",
            onClick: (S) => v.newCred = !v.newCred
          }, rt(v.newCred ? "cancel" : "new…"), 9, Wl),
          _[10] || (_[10] = M("span", { class: "muted" }, "entry in the secret store: user + password or private_key", -1))
        ]),
        v.newCred ? (Te(), Ue("div", kl, [
          oe(M("input", {
            "onUpdate:modelValue": (S) => v.credName = S,
            placeholder: "entry name",
            class: "w-120",
            spellcheck: "false"
          }, null, 8, Bl), [
            [Ce, v.credName]
          ]),
          oe(M("input", {
            "onUpdate:modelValue": (S) => v.credUser = S,
            placeholder: "user",
            class: "w-120",
            spellcheck: "false"
          }, null, 8, Jl), [
            [Ce, v.credUser]
          ]),
          oe(M("input", {
            "onUpdate:modelValue": (S) => v.credPassword = S,
            type: "password",
            placeholder: "password",
            class: "w-140",
            autocomplete: "new-password"
          }, null, 8, ql), [
            [Ce, v.credPassword]
          ]),
          oe(M("select", {
            "onUpdate:modelValue": (S) => v.credScope = S,
            title: "Where this credential is stored"
          }, [..._[11] || (_[11] = [
            M("option", { value: "" }, "scope…", -1),
            M("option", { value: "user" }, "user — mine only", -1),
            M("option", { value: "project" }, "project — travels with the project", -1),
            M("option", { value: "shared" }, "shared — administered", -1)
          ])], 8, Gl), [
            [Is, v.credScope]
          ]),
          M("button", {
            type: "button",
            disabled: !v.credName || !v.credPassword || !v.credScope,
            onClick: (S) => A(v)
          }, "Save to store", 8, Yl),
          M("span", zl, rt(v.credStatus), 1)
        ])) : Pn("", !0),
        M("div", Xl, [
          _[12] || (_[12] = M("span", { class: "muted w-label" }, "User", -1)),
          oe(M("input", {
            "onUpdate:modelValue": (S) => v.user = S,
            placeholder: v.credential ? "(from credential)" : "login",
            class: "w-120",
            spellcheck: "false"
          }, null, 8, Zl), [
            [Ce, v.user]
          ]),
          _[13] || (_[13] = M("span", { class: "muted" }, "Key file", -1)),
          oe(M("input", {
            "onUpdate:modelValue": (S) => v.keyFile = S,
            placeholder: "optional: C:\\Users\\me\\.ssh\\id_ed25519",
            class: "w-260",
            spellcheck: "false"
          }, null, 8, Ql), [
            [Ce, v.keyFile]
          ])
        ]),
        M("div", ec, [
          _[14] || (_[14] = M("span", { class: "muted w-label" }, "Description", -1)),
          oe(M("input", {
            "onUpdate:modelValue": (S) => v.description = S,
            placeholder: "Shown to the AI — what this host is",
            class: "grow"
          }, null, 8, tc), [
            [Ce, v.description]
          ])
        ]),
        M("div", sc, [
          M("label", nc, [
            oe(M("input", {
              "onUpdate:modelValue": (S) => v.allowWrite = S,
              type: "checkbox"
            }, null, 8, rc), [
              [Cl, v.allowWrite]
            ]),
            _[15] || (_[15] = M("span", null, "Allow the agent to write (apt, systemctl, edit files…)", -1))
          ]),
          _[16] || (_[16] = M("span", { class: "muted" }, "off = read-only guard blocks mutating commands; human terminal is never guarded", -1))
        ]),
        M("div", ic, [
          M("button", {
            type: "button",
            disabled: v.testing,
            onClick: (S) => D(v)
          }, "Test connection", 8, oc),
          M("span", lc, rt(v.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
}), fc = (e, t) => {
  const s = e.__vccOpts || e;
  for (const [n, r] of t)
    s[n] = r;
  return s;
}, uc = /* @__PURE__ */ fc(cc, [["__scopeId", "data-v-9464f4c1"]]);
function dc(e, t) {
  let s = Ol(uc, { api: t });
  const n = s.mount(e);
  return {
    save: () => n.toJson(),
    destroy: () => {
      s == null || s.unmount(), s = null;
    }
  };
}
export {
  dc as mount
};
