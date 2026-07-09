/**
* @vue/shared v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
// @__NO_SIDE_EFFECTS__
function bt(e) {
  const t = /* @__PURE__ */ Object.create(null);
  for (const n of e.split(",")) t[n] = 1;
  return (n) => n in t;
}
const ee = process.env.NODE_ENV !== "production" ? Object.freeze({}) : {}, Zt = process.env.NODE_ENV !== "production" ? Object.freeze([]) : [], pe = () => {
}, Rr = () => !1, Yn = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // uppercase letter
(e.charCodeAt(2) > 122 || e.charCodeAt(2) < 97), Rn = (e) => e.startsWith("onUpdate:"), le = Object.assign, Qi = (e, t) => {
  const n = e.indexOf(t);
  n > -1 && e.splice(n, 1);
}, Uc = Object.prototype.hasOwnProperty, Y = (e, t) => Uc.call(e, t), x = Array.isArray, xt = (e) => Gn(e) === "[object Map]", an = (e) => Gn(e) === "[object Set]", jo = (e) => Gn(e) === "[object Date]", F = (e) => typeof e == "function", se = (e) => typeof e == "string", Ke = (e) => typeof e == "symbol", Q = (e) => e !== null && typeof e == "object", Xi = (e) => (Q(e) || F(e)) && F(e.then) && F(e.catch), Fr = Object.prototype.toString, Gn = (e) => Fr.call(e), zi = (e) => Gn(e).slice(8, -1), Br = (e) => Gn(e) === "[object Object]", Zi = (e) => se(e) && e !== "NaN" && e[0] !== "-" && "" + parseInt(e, 10) === e, In = /* @__PURE__ */ bt(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
), qc = /* @__PURE__ */ bt(
  "bind,cloak,else-if,else,for,html,if,model,on,once,pre,show,slot,text,memo"
), Ks = (e) => {
  const t = /* @__PURE__ */ Object.create(null);
  return ((n) => t[n] || (t[n] = e(n)));
}, Hc = /-\w/g, Te = Ks(
  (e) => e.replace(Hc, (t) => t.slice(1).toUpperCase())
), Wc = /\B([A-Z])/g, Tt = Ks(
  (e) => e.replace(Wc, "-$1").toLowerCase()
), Us = Ks((e) => e.charAt(0).toUpperCase() + e.slice(1)), It = Ks(
  (e) => e ? `on${Us(e)}` : ""
), nt = (e, t) => !Object.is(e, t), Yt = (e, ...t) => {
  for (let n = 0; n < e.length; n++)
    e[n](...t);
}, Ts = (e, t, n, s = !1) => {
  Object.defineProperty(e, t, {
    configurable: !0,
    enumerable: !1,
    writable: s,
    value: n
  });
}, qs = (e) => {
  const t = parseFloat(e);
  return isNaN(t) ? e : t;
};
let Ro;
const Qn = () => Ro || (Ro = typeof globalThis < "u" ? globalThis : typeof self < "u" ? self : typeof window < "u" ? window : typeof global < "u" ? global : {});
function eo(e) {
  if (x(e)) {
    const t = {};
    for (let n = 0; n < e.length; n++) {
      const s = e[n], i = se(s) ? Qc(s) : eo(s);
      if (i)
        for (const o in i)
          t[o] = i[o];
    }
    return t;
  } else if (se(e) || Q(e))
    return e;
}
const Jc = /;(?![^(]*\))/g, Yc = /:([^]+)/, Gc = /\/\*[^]*?\*\//g;
function Qc(e) {
  const t = {};
  return e.replace(Gc, "").split(Jc).forEach((n) => {
    if (n) {
      const s = n.split(Yc);
      s.length > 1 && (t[s[0].trim()] = s[1].trim());
    }
  }), t;
}
function to(e) {
  let t = "";
  if (se(e))
    t = e;
  else if (x(e))
    for (let n = 0; n < e.length; n++) {
      const s = to(e[n]);
      s && (t += s + " ");
    }
  else if (Q(e))
    for (const n in e)
      e[n] && (t += n + " ");
  return t.trim();
}
const Xc = "html,body,base,head,link,meta,style,title,address,article,aside,footer,header,hgroup,h1,h2,h3,h4,h5,h6,nav,section,div,dd,dl,dt,figcaption,figure,picture,hr,img,li,main,ol,p,pre,ul,a,b,abbr,bdi,bdo,br,cite,code,data,dfn,em,i,kbd,mark,q,rp,rt,ruby,s,samp,small,span,strong,sub,sup,time,u,var,wbr,area,audio,map,track,video,embed,object,param,source,canvas,script,noscript,del,ins,caption,col,colgroup,table,thead,tbody,td,th,tr,button,datalist,fieldset,form,input,label,legend,meter,optgroup,option,output,progress,select,textarea,details,dialog,menu,summary,template,blockquote,iframe,tfoot", zc = "svg,animate,animateMotion,animateTransform,circle,clipPath,color-profile,defs,desc,discard,ellipse,feBlend,feColorMatrix,feComponentTransfer,feComposite,feConvolveMatrix,feDiffuseLighting,feDisplacementMap,feDistantLight,feDropShadow,feFlood,feFuncA,feFuncB,feFuncG,feFuncR,feGaussianBlur,feImage,feMerge,feMergeNode,feMorphology,feOffset,fePointLight,feSpecularLighting,feSpotLight,feTile,feTurbulence,filter,foreignObject,g,hatch,hatchpath,image,line,linearGradient,marker,mask,mesh,meshgradient,meshpatch,meshrow,metadata,mpath,path,pattern,polygon,polyline,radialGradient,rect,set,solidcolor,stop,switch,symbol,text,textPath,title,tspan,unknown,use,view", Zc = "annotation,annotation-xml,maction,maligngroup,malignmark,math,menclose,merror,mfenced,mfrac,mfraction,mglyph,mi,mlabeledtr,mlongdiv,mmultiscripts,mn,mo,mover,mpadded,mphantom,mprescripts,mroot,mrow,ms,mscarries,mscarry,msgroup,msline,mspace,msqrt,msrow,mstack,mstyle,msub,msubsup,msup,mtable,mtd,mtext,mtr,munder,munderover,none,semantics", ea = /* @__PURE__ */ bt(Xc), ta = /* @__PURE__ */ bt(zc), na = /* @__PURE__ */ bt(Zc), sa = "itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly", ia = /* @__PURE__ */ bt(sa);
function Kr(e) {
  return !!e || e === "";
}
function oa(e, t) {
  if (e.length !== t.length) return !1;
  let n = !0;
  for (let s = 0; n && s < e.length; s++)
    n = fn(e[s], t[s]);
  return n;
}
function fn(e, t) {
  if (e === t) return !0;
  let n = jo(e), s = jo(t);
  if (n || s)
    return n && s ? e.getTime() === t.getTime() : !1;
  if (n = Ke(e), s = Ke(t), n || s)
    return e === t;
  if (n = x(e), s = x(t), n || s)
    return n && s ? oa(e, t) : !1;
  if (n = Q(e), s = Q(t), n || s) {
    if (!n || !s)
      return !1;
    const i = Object.keys(e).length, o = Object.keys(t).length;
    if (i !== o)
      return !1;
    for (const r in e) {
      const l = e.hasOwnProperty(r), c = t.hasOwnProperty(r);
      if (l && !c || !l && c || !fn(e[r], t[r]))
        return !1;
    }
  }
  return String(e) === String(t);
}
function no(e, t) {
  return e.findIndex((n) => fn(n, t));
}
const Ur = (e) => !!(e && e.__v_isRef === !0), qr = (e) => se(e) ? e : e == null ? "" : x(e) || Q(e) && (e.toString === Fr || !F(e.toString)) ? Ur(e) ? qr(e.value) : JSON.stringify(e, Hr, 2) : String(e), Hr = (e, t) => Ur(t) ? Hr(e, t.value) : xt(t) ? {
  [`Map(${t.size})`]: [...t.entries()].reduce(
    (n, [s, i], o) => (n[ui(s, o) + " =>"] = i, n),
    {}
  )
} : an(t) ? {
  [`Set(${t.size})`]: [...t.values()].map((n) => ui(n))
} : Ke(t) ? ui(t) : Q(t) && !x(t) && !Br(t) ? String(t) : t, ui = (e, t = "") => {
  var n;
  return (
    // Symbol.description in es2019+ so we need to cast here to pass
    // the lib: es2016 check
    Ke(e) ? `Symbol(${(n = e.description) != null ? n : t})` : e
  );
};
/**
* @vue/reactivity v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function Ue(e, ...t) {
  console.warn(`[Vue warn] ${e}`, ...t);
}
let ge;
class ra {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t = !1) {
    this.detached = t, this._active = !0, this._on = 0, this.effects = [], this.cleanups = [], this._isPaused = !1, this._warnOnRun = !0, this.__v_skip = !0, !t && ge && (ge.active ? (this.parent = ge, this.index = (ge.scopes || (ge.scopes = [])).push(
      this
    ) - 1) : (this._active = !1, this._warnOnRun = !1));
  }
  get active() {
    return this._active;
  }
  pause() {
    if (this._active) {
      this._isPaused = !0;
      let t, n;
      if (this.scopes)
        for (t = 0, n = this.scopes.length; t < n; t++)
          this.scopes[t].pause();
      for (t = 0, n = this.effects.length; t < n; t++)
        this.effects[t].pause();
    }
  }
  /**
   * Resumes the effect scope, including all child scopes and effects.
   */
  resume() {
    if (this._active && this._isPaused) {
      this._isPaused = !1;
      let t, n;
      if (this.scopes)
        for (t = 0, n = this.scopes.length; t < n; t++)
          this.scopes[t].resume();
      for (t = 0, n = this.effects.length; t < n; t++)
        this.effects[t].resume();
    }
  }
  run(t) {
    if (this._active) {
      const n = ge;
      try {
        return ge = this, t();
      } finally {
        ge = n;
      }
    } else process.env.NODE_ENV !== "production" && this._warnOnRun && Ue("cannot run an inactive effect scope.");
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    ++this._on === 1 && (this.prevScope = ge, ge = this);
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    if (this._on > 0 && --this._on === 0) {
      if (ge === this)
        ge = this.prevScope;
      else {
        let t = ge;
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
      let n, s;
      for (n = 0, s = this.effects.length; n < s; n++)
        this.effects[n].stop();
      for (this.effects.length = 0, n = 0, s = this.cleanups.length; n < s; n++)
        this.cleanups[n]();
      if (this.cleanups.length = 0, this.scopes) {
        for (n = 0, s = this.scopes.length; n < s; n++)
          this.scopes[n].stop(!0);
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
function la() {
  return ge;
}
let Z;
const di = /* @__PURE__ */ new WeakSet();
class Wr {
  constructor(t) {
    this.fn = t, this.deps = void 0, this.depsTail = void 0, this.flags = 5, this.next = void 0, this.cleanup = void 0, this.scheduler = void 0, ge && (ge.active ? ge.effects.push(this) : this.flags &= -2);
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    this.flags & 64 && (this.flags &= -65, di.has(this) && (di.delete(this), this.trigger()));
  }
  /**
   * @internal
   */
  notify() {
    this.flags & 2 && !(this.flags & 32) || this.flags & 8 || Yr(this);
  }
  run() {
    if (!(this.flags & 1))
      return this.fn();
    this.flags |= 2, Fo(this), Gr(this);
    const t = Z, n = Be;
    Z = this, Be = !0;
    try {
      return this.fn();
    } finally {
      process.env.NODE_ENV !== "production" && Z !== this && Ue(
        "Active effect was not restored correctly - this is likely a Vue internal bug."
      ), Qr(this), Z = t, Be = n, this.flags &= -3;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let t = this.deps; t; t = t.nextDep)
        oo(t);
      this.deps = this.depsTail = void 0, Fo(this), this.onStop && this.onStop(), this.flags &= -2;
    }
  }
  trigger() {
    this.flags & 64 ? di.add(this) : this.scheduler ? this.scheduler() : this.runIfDirty();
  }
  /**
   * @internal
   */
  runIfDirty() {
    Ci(this) && this.run();
  }
  get dirty() {
    return Ci(this);
  }
}
let Jr = 0, $n, Vn;
function Yr(e, t = !1) {
  if (e.flags |= 8, t) {
    e.next = Vn, Vn = e;
    return;
  }
  e.next = $n, $n = e;
}
function so() {
  Jr++;
}
function io() {
  if (--Jr > 0)
    return;
  if (Vn) {
    let t = Vn;
    for (Vn = void 0; t; ) {
      const n = t.next;
      t.next = void 0, t.flags &= -9, t = n;
    }
  }
  let e;
  for (; $n; ) {
    let t = $n;
    for ($n = void 0; t; ) {
      const n = t.next;
      if (t.next = void 0, t.flags &= -9, t.flags & 1)
        try {
          t.trigger();
        } catch (s) {
          e || (e = s);
        }
      t = n;
    }
  }
  if (e) throw e;
}
function Gr(e) {
  for (let t = e.deps; t; t = t.nextDep)
    t.version = -1, t.prevActiveLink = t.dep.activeLink, t.dep.activeLink = t;
}
function Qr(e) {
  let t, n = e.depsTail, s = n;
  for (; s; ) {
    const i = s.prevDep;
    s.version === -1 ? (s === n && (n = i), oo(s), ca(s)) : t = s, s.dep.activeLink = s.prevActiveLink, s.prevActiveLink = void 0, s = i;
  }
  e.deps = t, e.depsTail = n;
}
function Ci(e) {
  for (let t = e.deps; t; t = t.nextDep)
    if (t.dep.version !== t.version || t.dep.computed && (Xr(t.dep.computed) || t.dep.version !== t.version))
      return !0;
  return !!e._dirty;
}
function Xr(e) {
  if (e.flags & 4 && !(e.flags & 16) || (e.flags &= -17, e.globalVersion === Fn) || (e.globalVersion = Fn, !e.isSSR && e.flags & 128 && (!e.deps && !e._dirty || !Ci(e))))
    return;
  e.flags |= 2;
  const t = e.dep, n = Z, s = Be;
  Z = e, Be = !0;
  try {
    Gr(e);
    const i = e.fn(e._value);
    (t.version === 0 || nt(i, e._value)) && (e.flags |= 128, e._value = i, t.version++);
  } catch (i) {
    throw t.version++, i;
  } finally {
    Z = n, Be = s, Qr(e), e.flags &= -3;
  }
}
function oo(e, t = !1) {
  const { dep: n, prevSub: s, nextSub: i } = e;
  if (s && (s.nextSub = i, e.prevSub = void 0), i && (i.prevSub = s, e.nextSub = void 0), process.env.NODE_ENV !== "production" && n.subsHead === e && (n.subsHead = i), n.subs === e && (n.subs = s, !s && n.computed)) {
    n.computed.flags &= -5;
    for (let o = n.computed.deps; o; o = o.nextDep)
      oo(o, !0);
  }
  !t && !--n.sc && n.map && n.map.delete(n.key);
}
function ca(e) {
  const { prevDep: t, nextDep: n } = e;
  t && (t.nextDep = n, e.prevDep = void 0), n && (n.prevDep = t, e.nextDep = void 0);
}
let Be = !0;
const zr = [];
function Le() {
  zr.push(Be), Be = !1;
}
function Me() {
  const e = zr.pop();
  Be = e === void 0 ? !0 : e;
}
function Fo(e) {
  const { cleanup: t } = e;
  if (e.cleanup = void 0, t) {
    const n = Z;
    Z = void 0;
    try {
      t();
    } finally {
      Z = n;
    }
  }
}
let Fn = 0;
class aa {
  constructor(t, n) {
    this.sub = t, this.dep = n, this.version = n.version, this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
}
class ro {
  // TODO isolatedDeclarations "__v_skip"
  constructor(t) {
    this.computed = t, this.version = 0, this.activeLink = void 0, this.subs = void 0, this.map = void 0, this.key = void 0, this.sc = 0, this.__v_skip = !0, process.env.NODE_ENV !== "production" && (this.subsHead = void 0);
  }
  track(t) {
    if (!Z || !Be || Z === this.computed)
      return;
    let n = this.activeLink;
    if (n === void 0 || n.sub !== Z)
      n = this.activeLink = new aa(Z, this), Z.deps ? (n.prevDep = Z.depsTail, Z.depsTail.nextDep = n, Z.depsTail = n) : Z.deps = Z.depsTail = n, Zr(n);
    else if (n.version === -1 && (n.version = this.version, n.nextDep)) {
      const s = n.nextDep;
      s.prevDep = n.prevDep, n.prevDep && (n.prevDep.nextDep = s), n.prevDep = Z.depsTail, n.nextDep = void 0, Z.depsTail.nextDep = n, Z.depsTail = n, Z.deps === n && (Z.deps = s);
    }
    return process.env.NODE_ENV !== "production" && Z.onTrack && Z.onTrack(
      le(
        {
          effect: Z
        },
        t
      )
    ), n;
  }
  trigger(t) {
    this.version++, Fn++, this.notify(t);
  }
  notify(t) {
    so();
    try {
      if (process.env.NODE_ENV !== "production")
        for (let n = this.subsHead; n; n = n.nextSub)
          n.sub.onTrigger && !(n.sub.flags & 8) && n.sub.onTrigger(
            le(
              {
                effect: n.sub
              },
              t
            )
          );
      for (let n = this.subs; n; n = n.prevSub)
        n.sub.notify() && n.sub.dep.notify();
    } finally {
      io();
    }
  }
}
function Zr(e) {
  if (e.dep.sc++, e.sub.flags & 4) {
    const t = e.dep.computed;
    if (t && !e.dep.subs) {
      t.flags |= 20;
      for (let s = t.deps; s; s = s.nextDep)
        Zr(s);
    }
    const n = e.dep.subs;
    n !== e && (e.prevSub = n, n && (n.nextSub = e)), process.env.NODE_ENV !== "production" && e.dep.subsHead === void 0 && (e.dep.subsHead = e), e.dep.subs = e;
  }
}
const Ai = /* @__PURE__ */ new WeakMap(), Pt = /* @__PURE__ */ Symbol(
  process.env.NODE_ENV !== "production" ? "Object iterate" : ""
), Ii = /* @__PURE__ */ Symbol(
  process.env.NODE_ENV !== "production" ? "Map keys iterate" : ""
), Bn = /* @__PURE__ */ Symbol(
  process.env.NODE_ENV !== "production" ? "Array iterate" : ""
);
function de(e, t, n) {
  if (Be && Z) {
    let s = Ai.get(e);
    s || Ai.set(e, s = /* @__PURE__ */ new Map());
    let i = s.get(n);
    i || (s.set(n, i = new ro()), i.map = s, i.key = n), process.env.NODE_ENV !== "production" ? i.track({
      target: e,
      type: t,
      key: n
    }) : i.track();
  }
}
function st(e, t, n, s, i, o) {
  const r = Ai.get(e);
  if (!r) {
    Fn++;
    return;
  }
  const l = (c) => {
    c && (process.env.NODE_ENV !== "production" ? c.trigger({
      target: e,
      type: t,
      key: n,
      newValue: s,
      oldValue: i,
      oldTarget: o
    }) : c.trigger());
  };
  if (so(), t === "clear")
    r.forEach(l);
  else {
    const c = x(e), a = c && Zi(n);
    if (c && n === "length") {
      const u = Number(s);
      r.forEach((f, h) => {
        (h === "length" || h === Bn || !Ke(h) && h >= u) && l(f);
      });
    } else
      switch ((n !== void 0 || r.has(void 0)) && l(r.get(n)), a && l(r.get(Bn)), t) {
        case "add":
          c ? a && l(r.get("length")) : (l(r.get(Pt)), xt(e) && l(r.get(Ii)));
          break;
        case "delete":
          c || (l(r.get(Pt)), xt(e) && l(r.get(Ii)));
          break;
        case "set":
          xt(e) && l(r.get(Pt));
          break;
      }
  }
  io();
}
function Ut(e) {
  const t = /* @__PURE__ */ q(e);
  return t === e ? t : (de(t, "iterate", Bn), /* @__PURE__ */ Ne(e) ? t : t.map(He));
}
function Hs(e) {
  return de(e = /* @__PURE__ */ q(e), "iterate", Bn), e;
}
function tt(e, t) {
  return /* @__PURE__ */ qe(e) ? on(/* @__PURE__ */ St(e) ? He(t) : t) : He(t);
}
const fa = {
  __proto__: null,
  [Symbol.iterator]() {
    return pi(this, Symbol.iterator, (e) => tt(this, e));
  },
  concat(...e) {
    return Ut(this).concat(
      ...e.map((t) => x(t) ? Ut(t) : t)
    );
  },
  entries() {
    return pi(this, "entries", (e) => (e[1] = tt(this, e[1]), e));
  },
  every(e, t) {
    return lt(this, "every", e, t, void 0, arguments);
  },
  filter(e, t) {
    return lt(
      this,
      "filter",
      e,
      t,
      (n) => n.map((s) => tt(this, s)),
      arguments
    );
  },
  find(e, t) {
    return lt(
      this,
      "find",
      e,
      t,
      (n) => tt(this, n),
      arguments
    );
  },
  findIndex(e, t) {
    return lt(this, "findIndex", e, t, void 0, arguments);
  },
  findLast(e, t) {
    return lt(
      this,
      "findLast",
      e,
      t,
      (n) => tt(this, n),
      arguments
    );
  },
  findLastIndex(e, t) {
    return lt(this, "findLastIndex", e, t, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(e, t) {
    return lt(this, "forEach", e, t, void 0, arguments);
  },
  includes(...e) {
    return hi(this, "includes", e);
  },
  indexOf(...e) {
    return hi(this, "indexOf", e);
  },
  join(e) {
    return Ut(this).join(e);
  },
  // keys() iterator only reads `length`, no optimization required
  lastIndexOf(...e) {
    return hi(this, "lastIndexOf", e);
  },
  map(e, t) {
    return lt(this, "map", e, t, void 0, arguments);
  },
  pop() {
    return vn(this, "pop");
  },
  push(...e) {
    return vn(this, "push", e);
  },
  reduce(e, ...t) {
    return Bo(this, "reduce", e, t);
  },
  reduceRight(e, ...t) {
    return Bo(this, "reduceRight", e, t);
  },
  shift() {
    return vn(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(e, t) {
    return lt(this, "some", e, t, void 0, arguments);
  },
  splice(...e) {
    return vn(this, "splice", e);
  },
  toReversed() {
    return Ut(this).toReversed();
  },
  toSorted(e) {
    return Ut(this).toSorted(e);
  },
  toSpliced(...e) {
    return Ut(this).toSpliced(...e);
  },
  unshift(...e) {
    return vn(this, "unshift", e);
  },
  values() {
    return pi(this, "values", (e) => tt(this, e));
  }
};
function pi(e, t, n) {
  const s = Hs(e), i = s[t]();
  return s !== e && !/* @__PURE__ */ Ne(e) && (i._next = i.next, i.next = () => {
    const o = i._next();
    return o.done || (o.value = n(o.value)), o;
  }), i;
}
const ua = Array.prototype;
function lt(e, t, n, s, i, o) {
  const r = Hs(e), l = r !== e && !/* @__PURE__ */ Ne(e), c = r[t];
  if (c !== ua[t]) {
    const f = c.apply(e, o);
    return l ? He(f) : f;
  }
  let a = n;
  r !== e && (l ? a = function(f, h) {
    return n.call(this, tt(e, f), h, e);
  } : n.length > 2 && (a = function(f, h) {
    return n.call(this, f, h, e);
  }));
  const u = c.call(r, a, s);
  return l && i ? i(u) : u;
}
function Bo(e, t, n, s) {
  const i = Hs(e), o = i !== e && !/* @__PURE__ */ Ne(e);
  let r = n, l = !1;
  i !== e && (o ? (l = s.length === 0, r = function(a, u, f) {
    return l && (l = !1, a = tt(e, a)), n.call(this, a, tt(e, u), f, e);
  }) : n.length > 3 && (r = function(a, u, f) {
    return n.call(this, a, u, f, e);
  }));
  const c = i[t](r, ...s);
  return l ? tt(e, c) : c;
}
function hi(e, t, n) {
  const s = /* @__PURE__ */ q(e);
  de(s, "iterate", Bn);
  const i = s[t](...n);
  return (i === -1 || i === !1) && /* @__PURE__ */ Ds(n[0]) ? (n[0] = /* @__PURE__ */ q(n[0]), s[t](...n)) : i;
}
function vn(e, t, n = []) {
  Le(), so();
  const s = (/* @__PURE__ */ q(e))[t].apply(e, n);
  return io(), Me(), s;
}
const da = /* @__PURE__ */ bt("__proto__,__v_isRef,__isVue"), el = new Set(
  /* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((e) => e !== "arguments" && e !== "caller").map((e) => Symbol[e]).filter(Ke)
);
function pa(e) {
  Ke(e) || (e = String(e));
  const t = /* @__PURE__ */ q(this);
  return de(t, "has", e), t.hasOwnProperty(e);
}
class tl {
  constructor(t = !1, n = !1) {
    this._isReadonly = t, this._isShallow = n;
  }
  get(t, n, s) {
    if (n === "__v_skip") return t.__v_skip;
    const i = this._isReadonly, o = this._isShallow;
    if (n === "__v_isReactive")
      return !i;
    if (n === "__v_isReadonly")
      return i;
    if (n === "__v_isShallow")
      return o;
    if (n === "__v_raw")
      return s === (i ? o ? ll : rl : o ? ol : il).get(t) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(t) === Object.getPrototypeOf(s) ? t : void 0;
    const r = x(t);
    if (!i) {
      let c;
      if (r && (c = fa[n]))
        return c;
      if (n === "hasOwnProperty")
        return pa;
    }
    const l = Reflect.get(
      t,
      n,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      /* @__PURE__ */ fe(t) ? t : s
    );
    if ((Ke(n) ? el.has(n) : da(n)) || (i || de(t, "get", n), o))
      return l;
    if (/* @__PURE__ */ fe(l)) {
      const c = r && Zi(n) ? l : l.value;
      return i && Q(c) ? /* @__PURE__ */ Vi(c) : c;
    }
    return Q(l) ? i ? /* @__PURE__ */ Vi(l) : /* @__PURE__ */ Js(l) : l;
  }
}
class nl extends tl {
  constructor(t = !1) {
    super(!1, t);
  }
  set(t, n, s, i) {
    let o = t[n];
    const r = x(t) && Zi(n);
    if (!this._isShallow) {
      const a = /* @__PURE__ */ qe(o);
      if (!/* @__PURE__ */ Ne(s) && !/* @__PURE__ */ qe(s) && (o = /* @__PURE__ */ q(o), s = /* @__PURE__ */ q(s)), !r && /* @__PURE__ */ fe(o) && !/* @__PURE__ */ fe(s))
        return a ? (process.env.NODE_ENV !== "production" && Ue(
          `Set operation on key "${String(n)}" failed: target is readonly.`,
          t[n]
        ), !0) : (o.value = s, !0);
    }
    const l = r ? Number(n) < t.length : Y(t, n), c = Reflect.set(
      t,
      n,
      s,
      /* @__PURE__ */ fe(t) ? t : i
    );
    return t === /* @__PURE__ */ q(i) && c && (l ? nt(s, o) && st(t, "set", n, s, o) : st(t, "add", n, s)), c;
  }
  deleteProperty(t, n) {
    const s = Y(t, n), i = t[n], o = Reflect.deleteProperty(t, n);
    return o && s && st(t, "delete", n, void 0, i), o;
  }
  has(t, n) {
    const s = Reflect.has(t, n);
    return (!Ke(n) || !el.has(n)) && de(t, "has", n), s;
  }
  ownKeys(t) {
    return de(
      t,
      "iterate",
      x(t) ? "length" : Pt
    ), Reflect.ownKeys(t);
  }
}
class sl extends tl {
  constructor(t = !1) {
    super(!0, t);
  }
  set(t, n) {
    return process.env.NODE_ENV !== "production" && Ue(
      `Set operation on key "${String(n)}" failed: target is readonly.`,
      t
    ), !0;
  }
  deleteProperty(t, n) {
    return process.env.NODE_ENV !== "production" && Ue(
      `Delete operation on key "${String(n)}" failed: target is readonly.`,
      t
    ), !0;
  }
}
const ha = /* @__PURE__ */ new nl(), ma = /* @__PURE__ */ new sl(), ga = /* @__PURE__ */ new nl(!0), ya = /* @__PURE__ */ new sl(!0), $i = (e) => e, cs = (e) => Reflect.getPrototypeOf(e);
function ba(e, t, n) {
  return function(...s) {
    const i = this.__v_raw, o = /* @__PURE__ */ q(i), r = xt(o), l = e === "entries" || e === Symbol.iterator && r, c = e === "keys" && r, a = i[e](...s), u = n ? $i : t ? on : He;
    return !t && de(
      o,
      "iterate",
      c ? Ii : Pt
    ), le(
      // inheriting all iterator properties
      Object.create(a),
      {
        // iterator protocol
        next() {
          const { value: f, done: h } = a.next();
          return h ? { value: f, done: h } : {
            value: l ? [u(f[0]), u(f[1])] : u(f),
            done: h
          };
        }
      }
    );
  };
}
function as(e) {
  return function(...t) {
    if (process.env.NODE_ENV !== "production") {
      const n = t[0] ? `on key "${t[0]}" ` : "";
      Ue(
        `${Us(e)} operation ${n}failed: target is readonly.`,
        /* @__PURE__ */ q(this)
      );
    }
    return e === "delete" ? !1 : e === "clear" ? void 0 : this;
  };
}
function _a(e, t) {
  const n = {
    get(i) {
      const o = this.__v_raw, r = /* @__PURE__ */ q(o), l = /* @__PURE__ */ q(i);
      e || (nt(i, l) && de(r, "get", i), de(r, "get", l));
      const { has: c } = cs(r), a = t ? $i : e ? on : He;
      if (c.call(r, i))
        return a(o.get(i));
      if (c.call(r, l))
        return a(o.get(l));
      o !== r && o.get(i);
    },
    get size() {
      const i = this.__v_raw;
      return !e && de(/* @__PURE__ */ q(i), "iterate", Pt), i.size;
    },
    has(i) {
      const o = this.__v_raw, r = /* @__PURE__ */ q(o), l = /* @__PURE__ */ q(i);
      return e || (nt(i, l) && de(r, "has", i), de(r, "has", l)), i === l ? o.has(i) : o.has(i) || o.has(l);
    },
    forEach(i, o) {
      const r = this, l = r.__v_raw, c = /* @__PURE__ */ q(l), a = t ? $i : e ? on : He;
      return !e && de(c, "iterate", Pt), l.forEach((u, f) => i.call(o, a(u), a(f), r));
    }
  };
  return le(
    n,
    e ? {
      add: as("add"),
      set: as("set"),
      delete: as("delete"),
      clear: as("clear")
    } : {
      add(i) {
        const o = /* @__PURE__ */ q(this), r = cs(o), l = /* @__PURE__ */ q(i), c = !t && !/* @__PURE__ */ Ne(i) && !/* @__PURE__ */ qe(i) ? l : i;
        return r.has.call(o, c) || nt(i, c) && r.has.call(o, i) || nt(l, c) && r.has.call(o, l) || (o.add(c), st(o, "add", c, c)), this;
      },
      set(i, o) {
        !t && !/* @__PURE__ */ Ne(o) && !/* @__PURE__ */ qe(o) && (o = /* @__PURE__ */ q(o));
        const r = /* @__PURE__ */ q(this), { has: l, get: c } = cs(r);
        let a = l.call(r, i);
        a ? process.env.NODE_ENV !== "production" && Ko(r, l, i) : (i = /* @__PURE__ */ q(i), a = l.call(r, i));
        const u = c.call(r, i);
        return r.set(i, o), a ? nt(o, u) && st(r, "set", i, o, u) : st(r, "add", i, o), this;
      },
      delete(i) {
        const o = /* @__PURE__ */ q(this), { has: r, get: l } = cs(o);
        let c = r.call(o, i);
        c ? process.env.NODE_ENV !== "production" && Ko(o, r, i) : (i = /* @__PURE__ */ q(i), c = r.call(o, i));
        const a = l ? l.call(o, i) : void 0, u = o.delete(i);
        return c && st(o, "delete", i, void 0, a), u;
      },
      clear() {
        const i = /* @__PURE__ */ q(this), o = i.size !== 0, r = process.env.NODE_ENV !== "production" ? xt(i) ? new Map(i) : new Set(i) : void 0, l = i.clear();
        return o && st(
          i,
          "clear",
          void 0,
          void 0,
          r
        ), l;
      }
    }
  ), [
    "keys",
    "values",
    "entries",
    Symbol.iterator
  ].forEach((i) => {
    n[i] = ba(i, e, t);
  }), n;
}
function Ws(e, t) {
  const n = _a(e, t);
  return (s, i, o) => i === "__v_isReactive" ? !e : i === "__v_isReadonly" ? e : i === "__v_raw" ? s : Reflect.get(
    Y(n, i) && i in s ? n : s,
    i,
    o
  );
}
const wa = {
  get: /* @__PURE__ */ Ws(!1, !1)
}, Ea = {
  get: /* @__PURE__ */ Ws(!1, !0)
}, va = {
  get: /* @__PURE__ */ Ws(!0, !1)
}, Na = {
  get: /* @__PURE__ */ Ws(!0, !0)
};
function Ko(e, t, n) {
  const s = /* @__PURE__ */ q(n);
  if (s !== n && t.call(e, s)) {
    const i = zi(e);
    Ue(
      `Reactive ${i} contains both the raw and reactive versions of the same object${i === "Map" ? " as keys" : ""}, which can lead to inconsistencies. Avoid differentiating between the raw and reactive versions of an object and only use the reactive version if possible.`
    );
  }
}
const il = /* @__PURE__ */ new WeakMap(), ol = /* @__PURE__ */ new WeakMap(), rl = /* @__PURE__ */ new WeakMap(), ll = /* @__PURE__ */ new WeakMap();
function Oa(e) {
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
function Js(e) {
  return /* @__PURE__ */ qe(e) ? e : Ys(
    e,
    !1,
    ha,
    wa,
    il
  );
}
// @__NO_SIDE_EFFECTS__
function Sa(e) {
  return Ys(
    e,
    !1,
    ga,
    Ea,
    ol
  );
}
// @__NO_SIDE_EFFECTS__
function Vi(e) {
  return Ys(
    e,
    !0,
    ma,
    va,
    rl
  );
}
// @__NO_SIDE_EFFECTS__
function it(e) {
  return Ys(
    e,
    !0,
    ya,
    Na,
    ll
  );
}
function Ys(e, t, n, s, i) {
  if (!Q(e))
    return process.env.NODE_ENV !== "production" && Ue(
      `value cannot be made ${t ? "readonly" : "reactive"}: ${String(
        e
      )}`
    ), e;
  if (e.__v_raw && !(t && e.__v_isReactive) || e.__v_skip || !Object.isExtensible(e))
    return e;
  const o = i.get(e);
  if (o)
    return o;
  const r = Oa(zi(e));
  if (r === 0)
    return e;
  const l = new Proxy(
    e,
    r === 2 ? s : n
  );
  return i.set(e, l), l;
}
// @__NO_SIDE_EFFECTS__
function St(e) {
  return /* @__PURE__ */ qe(e) ? /* @__PURE__ */ St(e.__v_raw) : !!(e && e.__v_isReactive);
}
// @__NO_SIDE_EFFECTS__
function qe(e) {
  return !!(e && e.__v_isReadonly);
}
// @__NO_SIDE_EFFECTS__
function Ne(e) {
  return !!(e && e.__v_isShallow);
}
// @__NO_SIDE_EFFECTS__
function Ds(e) {
  return e ? !!e.__v_raw : !1;
}
// @__NO_SIDE_EFFECTS__
function q(e) {
  const t = e && e.__v_raw;
  return t ? /* @__PURE__ */ q(t) : e;
}
function ka(e) {
  return !Y(e, "__v_skip") && Object.isExtensible(e) && Ts(e, "__v_skip", !0), e;
}
const He = (e) => Q(e) ? /* @__PURE__ */ Js(e) : e, on = (e) => Q(e) ? /* @__PURE__ */ Vi(e) : e;
// @__NO_SIDE_EFFECTS__
function fe(e) {
  return e ? e.__v_isRef === !0 : !1;
}
// @__NO_SIDE_EFFECTS__
function Uo(e) {
  return Ta(e, !1);
}
function Ta(e, t) {
  return /* @__PURE__ */ fe(e) ? e : new Da(e, t);
}
class Da {
  constructor(t, n) {
    this.dep = new ro(), this.__v_isRef = !0, this.__v_isShallow = !1, this._rawValue = n ? t : /* @__PURE__ */ q(t), this._value = n ? t : He(t), this.__v_isShallow = n;
  }
  get value() {
    return process.env.NODE_ENV !== "production" ? this.dep.track({
      target: this,
      type: "get",
      key: "value"
    }) : this.dep.track(), this._value;
  }
  set value(t) {
    const n = this._rawValue, s = this.__v_isShallow || /* @__PURE__ */ Ne(t) || /* @__PURE__ */ qe(t);
    t = s ? t : /* @__PURE__ */ q(t), nt(t, n) && (this._rawValue = t, this._value = s ? t : He(t), process.env.NODE_ENV !== "production" ? this.dep.trigger({
      target: this,
      type: "set",
      key: "value",
      newValue: t,
      oldValue: n
    }) : this.dep.trigger());
  }
}
function Ca(e) {
  return /* @__PURE__ */ fe(e) ? e.value : e;
}
const Aa = {
  get: (e, t, n) => t === "__v_raw" ? e : Ca(Reflect.get(e, t, n)),
  set: (e, t, n, s) => {
    const i = e[t];
    return /* @__PURE__ */ fe(i) && !/* @__PURE__ */ fe(n) ? (i.value = n, !0) : Reflect.set(e, t, n, s);
  }
};
function cl(e) {
  return /* @__PURE__ */ St(e) ? e : new Proxy(e, Aa);
}
class Ia {
  constructor(t, n, s) {
    this.fn = t, this.setter = n, this._value = void 0, this.dep = new ro(this), this.__v_isRef = !0, this.deps = void 0, this.depsTail = void 0, this.flags = 16, this.globalVersion = Fn - 1, this.next = void 0, this.effect = this, this.__v_isReadonly = !n, this.isSSR = s;
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags |= 16, !(this.flags & 8) && // avoid infinite self recursion
    Z !== this)
      return Yr(this, !0), !0;
    process.env.NODE_ENV;
  }
  get value() {
    const t = process.env.NODE_ENV !== "production" ? this.dep.track({
      target: this,
      type: "get",
      key: "value"
    }) : this.dep.track();
    return Xr(this), t && (t.version = this.dep.version), this._value;
  }
  set value(t) {
    this.setter ? this.setter(t) : process.env.NODE_ENV !== "production" && Ue("Write operation failed: computed value is readonly");
  }
}
// @__NO_SIDE_EFFECTS__
function $a(e, t, n = !1) {
  let s, i;
  F(e) ? s = e : (s = e.get, i = e.set);
  const o = new Ia(s, i, n);
  return process.env.NODE_ENV, o;
}
const fs = {}, Cs = /* @__PURE__ */ new WeakMap();
let $t;
function Va(e, t = !1, n = $t) {
  if (n) {
    let s = Cs.get(n);
    s || Cs.set(n, s = []), s.push(e);
  } else process.env.NODE_ENV !== "production" && !t && Ue(
    "onWatcherCleanup() was called when there was no active watcher to associate with."
  );
}
function La(e, t, n = ee) {
  const { immediate: s, deep: i, once: o, scheduler: r, augmentJob: l, call: c } = n, a = (S) => {
    (n.onWarn || Ue)(
      "Invalid watch source: ",
      S,
      "A watch source can only be a getter/effect function, a ref, a reactive object, or an array of these types."
    );
  }, u = (S) => i ? S : /* @__PURE__ */ Ne(S) || i === !1 || i === 0 ? pt(S, 1) : pt(S);
  let f, h, m, b, p = !1, y = !1;
  if (/* @__PURE__ */ fe(e) ? (h = () => e.value, p = /* @__PURE__ */ Ne(e)) : /* @__PURE__ */ St(e) ? (h = () => u(e), p = !0) : x(e) ? (y = !0, p = e.some((S) => /* @__PURE__ */ St(S) || /* @__PURE__ */ Ne(S)), h = () => e.map((S) => {
    if (/* @__PURE__ */ fe(S))
      return S.value;
    if (/* @__PURE__ */ St(S))
      return u(S);
    if (F(S))
      return c ? c(S, 2) : S();
    process.env.NODE_ENV !== "production" && a(S);
  })) : F(e) ? t ? h = c ? () => c(e, 2) : e : h = () => {
    if (m) {
      Le();
      try {
        m();
      } finally {
        Me();
      }
    }
    const S = $t;
    $t = f;
    try {
      return c ? c(e, 3, [b]) : e(b);
    } finally {
      $t = S;
    }
  } : (h = pe, process.env.NODE_ENV !== "production" && a(e)), t && i) {
    const S = h, j = i === !0 ? 1 / 0 : i;
    h = () => pt(S(), j);
  }
  const k = la(), E = () => {
    f.stop(), k && k.active && Qi(k.effects, f);
  };
  if (o && t) {
    const S = t;
    t = (...j) => {
      const M = S(...j);
      return E(), M;
    };
  }
  let I = y ? new Array(e.length).fill(fs) : fs;
  const L = (S) => {
    if (!(!(f.flags & 1) || !f.dirty && !S))
      if (t) {
        const j = f.run();
        if (S || i || p || (y ? j.some((M, V) => nt(M, I[V])) : nt(j, I))) {
          m && m();
          const M = $t;
          $t = f;
          try {
            const V = [
              j,
              // pass undefined as the old value when it's changed for the first time
              I === fs ? void 0 : y && I[0] === fs ? [] : I,
              b
            ];
            I = j, c ? c(t, 3, V) : (
              // @ts-expect-error
              t(...V)
            );
          } finally {
            $t = M;
          }
        }
      } else
        f.run();
  };
  return l && l(L), f = new Wr(h), f.scheduler = r ? () => r(L, !1) : L, b = (S) => Va(S, !1, f), m = f.onStop = () => {
    const S = Cs.get(f);
    if (S) {
      if (c)
        c(S, 4);
      else
        for (const j of S) j();
      Cs.delete(f);
    }
  }, process.env.NODE_ENV !== "production" && (f.onTrack = n.onTrack, f.onTrigger = n.onTrigger), t ? s ? L(!0) : I = f.run() : r ? r(L.bind(null, !0), !0) : f.run(), E.pause = f.pause.bind(f), E.resume = f.resume.bind(f), E.stop = E, E;
}
function pt(e, t = 1 / 0, n) {
  if (t <= 0 || !Q(e) || e.__v_skip || (n = n || /* @__PURE__ */ new Map(), (n.get(e) || 0) >= t))
    return e;
  if (n.set(e, t), t--, /* @__PURE__ */ fe(e))
    pt(e.value, t, n);
  else if (x(e))
    for (let s = 0; s < e.length; s++)
      pt(e[s], t, n);
  else if (an(e) || xt(e))
    e.forEach((s) => {
      pt(s, t, n);
    });
  else if (Br(e)) {
    for (const s in e)
      pt(e[s], t, n);
    for (const s of Object.getOwnPropertySymbols(e))
      Object.prototype.propertyIsEnumerable.call(e, s) && pt(e[s], t, n);
  }
  return e;
}
/**
* @vue/runtime-core v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
const jt = [];
function ys(e) {
  jt.push(e);
}
function bs() {
  jt.pop();
}
let mi = !1;
function C(e, ...t) {
  if (mi) return;
  mi = !0, Le();
  const n = jt.length ? jt[jt.length - 1].component : null, s = n && n.appContext.config.warnHandler, i = Ma();
  if (s)
    un(
      s,
      n,
      11,
      [
        // eslint-disable-next-line no-restricted-syntax
        e + t.map((o) => {
          var r, l;
          return (l = (r = o.toString) == null ? void 0 : r.call(o)) != null ? l : JSON.stringify(o);
        }).join(""),
        n && n.proxy,
        i.map(
          ({ vnode: o }) => `at <${ts(n, o.type)}>`
        ).join(`
`),
        i
      ]
    );
  else {
    const o = [`[Vue warn]: ${e}`, ...t];
    i.length && o.push(`
`, ...xa(i)), console.warn(...o);
  }
  Me(), mi = !1;
}
function Ma() {
  let e = jt[jt.length - 1];
  if (!e)
    return [];
  const t = [];
  for (; e; ) {
    const n = t[0];
    n && n.vnode === e ? n.recurseCount++ : t.push({
      vnode: e,
      recurseCount: 0
    });
    const s = e.component && e.component.parent;
    e = s && s.vnode;
  }
  return t;
}
function xa(e) {
  const t = [];
  return e.forEach((n, s) => {
    t.push(...s === 0 ? [] : [`
`], ...Pa(n));
  }), t;
}
function Pa({ vnode: e, recurseCount: t }) {
  const n = t > 0 ? `... (${t} recursive calls)` : "", s = e.component ? e.component.parent == null : !1, i = ` at <${ts(
    e.component,
    e.type,
    s
  )}`, o = ">" + n;
  return e.props ? [i, ...ja(e.props), o] : [i + o];
}
function ja(e) {
  const t = [], n = Object.keys(e);
  return n.slice(0, 3).forEach((s) => {
    t.push(...al(s, e[s]));
  }), n.length > 3 && t.push(" ..."), t;
}
function al(e, t, n) {
  return se(t) ? (t = JSON.stringify(t), n ? t : [`${e}=${t}`]) : typeof t == "number" || typeof t == "boolean" || t == null ? n ? t : [`${e}=${t}`] : /* @__PURE__ */ fe(t) ? (t = al(e, /* @__PURE__ */ q(t.value), !0), n ? t : [`${e}=Ref<`, t, ">"]) : F(t) ? [`${e}=fn${t.name ? `<${t.name}>` : ""}`] : (t = /* @__PURE__ */ q(t), n ? t : [`${e}=`, t]);
}
const lo = {
  sp: "serverPrefetch hook",
  bc: "beforeCreate hook",
  c: "created hook",
  bm: "beforeMount hook",
  m: "mounted hook",
  bu: "beforeUpdate hook",
  u: "updated",
  bum: "beforeUnmount hook",
  um: "unmounted hook",
  a: "activated hook",
  da: "deactivated hook",
  ec: "errorCaptured hook",
  rtc: "renderTracked hook",
  rtg: "renderTriggered hook",
  0: "setup function",
  1: "render function",
  2: "watcher getter",
  3: "watcher callback",
  4: "watcher cleanup function",
  5: "native event handler",
  6: "component event handler",
  7: "vnode hook",
  8: "directive hook",
  9: "transition hook",
  10: "app errorHandler",
  11: "app warnHandler",
  12: "ref function",
  13: "async component loader",
  14: "scheduler flush",
  15: "component update",
  16: "app unmount cleanup function"
};
function un(e, t, n, s) {
  try {
    return s ? e(...s) : e();
  } catch (i) {
    Xn(i, t, n);
  }
}
function We(e, t, n, s) {
  if (F(e)) {
    const i = un(e, t, n, s);
    return i && Xi(i) && i.catch((o) => {
      Xn(o, t, n);
    }), i;
  }
  if (x(e)) {
    const i = [];
    for (let o = 0; o < e.length; o++)
      i.push(We(e[o], t, n, s));
    return i;
  } else process.env.NODE_ENV !== "production" && C(
    `Invalid value type passed to callWithAsyncErrorHandling(): ${typeof e}`
  );
}
function Xn(e, t, n, s = !0) {
  const i = t ? t.vnode : null, { errorHandler: o, throwUnhandledErrorInProduction: r } = t && t.appContext.config || ee;
  if (t) {
    let l = t.parent;
    const c = t.proxy, a = process.env.NODE_ENV !== "production" ? lo[n] : `https://vuejs.org/error-reference/#runtime-${n}`;
    for (; l; ) {
      const u = l.ec;
      if (u) {
        for (let f = 0; f < u.length; f++)
          if (u[f](e, c, a) === !1)
            return;
      }
      l = l.parent;
    }
    if (o) {
      Le(), un(o, null, 10, [
        e,
        c,
        a
      ]), Me();
      return;
    }
  }
  Ra(e, n, i, s, r);
}
function Ra(e, t, n, s = !0, i = !1) {
  if (process.env.NODE_ENV !== "production") {
    const o = lo[t];
    if (n && ys(n), C(`Unhandled error${o ? ` during execution of ${o}` : ""}`), n && bs(), s)
      throw e;
    console.error(e);
  } else {
    if (i)
      throw e;
    console.error(e);
  }
}
const Ee = [];
let et = -1;
const en = [];
let vt = null, Gt = 0;
const fl = /* @__PURE__ */ Promise.resolve();
let As = null;
const Fa = 100;
function ul(e) {
  const t = As || fl;
  return e ? t.then(this ? e.bind(this) : e) : t;
}
function Ba(e) {
  let t = et + 1, n = Ee.length;
  for (; t < n; ) {
    const s = t + n >>> 1, i = Ee[s], o = Kn(i);
    o < e || o === e && i.flags & 2 ? t = s + 1 : n = s;
  }
  return t;
}
function Gs(e) {
  if (!(e.flags & 1)) {
    const t = Kn(e), n = Ee[Ee.length - 1];
    !n || // fast path when the job id is larger than the tail
    !(e.flags & 2) && t >= Kn(n) ? Ee.push(e) : Ee.splice(Ba(t), 0, e), e.flags |= 1, dl();
  }
}
function dl() {
  As || (As = fl.then(ml));
}
function pl(e) {
  x(e) ? en.push(...e) : vt && e.id === -1 ? vt.splice(Gt + 1, 0, e) : e.flags & 1 || (en.push(e), e.flags |= 1), dl();
}
function qo(e, t, n = et + 1) {
  for (process.env.NODE_ENV !== "production" && (t = t || /* @__PURE__ */ new Map()); n < Ee.length; n++) {
    const s = Ee[n];
    if (s && s.flags & 2) {
      if (e && s.id !== e.uid || process.env.NODE_ENV !== "production" && co(t, s))
        continue;
      Ee.splice(n, 1), n--, s.flags & 4 && (s.flags &= -2), s(), s.flags & 4 || (s.flags &= -2);
    }
  }
}
function hl(e) {
  if (en.length) {
    const t = [...new Set(en)].sort(
      (n, s) => Kn(n) - Kn(s)
    );
    if (en.length = 0, vt) {
      vt.push(...t);
      return;
    }
    for (vt = t, process.env.NODE_ENV !== "production" && (e = e || /* @__PURE__ */ new Map()), Gt = 0; Gt < vt.length; Gt++) {
      const n = vt[Gt];
      process.env.NODE_ENV !== "production" && co(e, n) || (n.flags & 4 && (n.flags &= -2), n.flags & 8 || n(), n.flags &= -2);
    }
    vt = null, Gt = 0;
  }
}
const Kn = (e) => e.id == null ? e.flags & 2 ? -1 : 1 / 0 : e.id;
function ml(e) {
  process.env.NODE_ENV !== "production" && (e = e || /* @__PURE__ */ new Map());
  const t = process.env.NODE_ENV !== "production" ? (n) => co(e, n) : pe;
  try {
    for (et = 0; et < Ee.length; et++) {
      const n = Ee[et];
      if (n && !(n.flags & 8)) {
        if (process.env.NODE_ENV !== "production" && t(n))
          continue;
        n.flags & 4 && (n.flags &= -2), un(
          n,
          n.i,
          n.i ? 15 : 14
        ), n.flags & 4 || (n.flags &= -2);
      }
    }
  } finally {
    for (; et < Ee.length; et++) {
      const n = Ee[et];
      n && (n.flags &= -2);
    }
    et = -1, Ee.length = 0, hl(e), As = null, (Ee.length || en.length) && ml(e);
  }
}
function co(e, t) {
  const n = e.get(t) || 0;
  if (n > Fa) {
    const s = t.i, i = s && zl(s.type);
    return Xn(
      `Maximum recursive updates exceeded${i ? ` in component <${i}>` : ""}. This means you have a reactive effect that is mutating its own dependencies and thus recursively triggering itself. Possible sources include component template, render function, updated hook or watcher source function.`,
      null,
      10
    ), !0;
  }
  return e.set(t, n + 1), !1;
}
let De = !1;
const Ho = (e) => {
  try {
    return De;
  } finally {
    De = e;
  }
}, _s = /* @__PURE__ */ new Map();
process.env.NODE_ENV !== "production" && (Qn().__VUE_HMR_RUNTIME__ = {
  createRecord: gi(gl),
  rerender: gi(qa),
  reload: gi(Ha)
});
const Ft = /* @__PURE__ */ new Map();
function Ka(e) {
  const t = e.type.__hmrId;
  let n = Ft.get(t);
  n || (gl(t, e.type), n = Ft.get(t)), n.instances.add(e);
}
function Ua(e) {
  Ft.get(e.type.__hmrId).instances.delete(e);
}
function gl(e, t) {
  return Ft.has(e) ? !1 : (Ft.set(e, {
    initialDef: Is(t),
    instances: /* @__PURE__ */ new Set()
  }), !0);
}
function Is(e) {
  return Zl(e) ? e.__vccOpts : e;
}
function qa(e, t) {
  const n = Ft.get(e);
  n && (n.initialDef.render = t, [...n.instances].forEach((s) => {
    t && (s.render = t, Is(s.type).render = t), s.renderCache = [], De = !0, s.job.flags & 8 || s.update(), De = !1;
  }));
}
function Ha(e, t) {
  const n = Ft.get(e);
  if (!n) return;
  t = Is(t), Wo(n.initialDef, t);
  const s = [...n.instances];
  for (let i = 0; i < s.length; i++) {
    const o = s[i], r = Is(o.type);
    let l = _s.get(r);
    l || (r !== n.initialDef && Wo(r, t), _s.set(r, l = /* @__PURE__ */ new Set())), l.add(o), o.appContext.propsCache.delete(o.type), o.appContext.emitsCache.delete(o.type), o.appContext.optionsCache.delete(o.type), o.ceReload ? (l.add(o), o.ceReload(t.styles), l.delete(o)) : o.parent ? Gs(() => {
      o.job.flags & 8 || (De = !0, o.parent.update(), De = !1, l.delete(o));
    }) : o.appContext.reload ? o.appContext.reload() : typeof window < "u" ? window.location.reload() : console.warn(
      "[HMR] Root or manually mounted instance modified. Full reload required."
    ), o.root.ce && o !== o.root && o.root.ce._removeChildStyle(r);
  }
  pl(() => {
    _s.clear();
  });
}
function Wo(e, t) {
  le(e, t);
  for (const n in e)
    n !== "__file" && !(n in t) && delete e[n];
}
function gi(e) {
  return (t, n) => {
    try {
      return e(t, n);
    } catch (s) {
      console.error(s), console.warn(
        "[HMR] Something went wrong during Vue component hot-reload. Full reload required."
      );
    }
  };
}
let Fe, kn = [], Li = !1;
function zn(e, ...t) {
  Fe ? Fe.emit(e, ...t) : Li || kn.push({ event: e, args: t });
}
function ao(e, t) {
  var n, s;
  Fe = e, Fe ? (Fe.enabled = !0, kn.forEach(({ event: i, args: o }) => Fe.emit(i, ...o)), kn = []) : /* handle late devtools injection - only do this if we are in an actual */ /* browser environment to avoid the timer handle stalling test runner exit */ /* (#4815) */ typeof window < "u" && // some envs mock window but not fully
  window.HTMLElement && // also exclude jsdom
  // eslint-disable-next-line no-restricted-syntax
  !((s = (n = window.navigator) == null ? void 0 : n.userAgent) != null && s.includes("jsdom")) ? ((t.__VUE_DEVTOOLS_HOOK_REPLAY__ = t.__VUE_DEVTOOLS_HOOK_REPLAY__ || []).push((o) => {
    ao(o, t);
  }), setTimeout(() => {
    Fe || (t.__VUE_DEVTOOLS_HOOK_REPLAY__ = null, Li = !0, kn = []);
  }, 3e3)) : (Li = !0, kn = []);
}
function Wa(e, t) {
  zn("app:init", e, t, {
    Fragment: Ie,
    Text: Zn,
    Comment: Ae,
    Static: vs
  });
}
function Ja(e) {
  zn("app:unmount", e);
}
const Ya = /* @__PURE__ */ fo(
  "component:added"
  /* COMPONENT_ADDED */
), yl = /* @__PURE__ */ fo(
  "component:updated"
  /* COMPONENT_UPDATED */
), Ga = /* @__PURE__ */ fo(
  "component:removed"
  /* COMPONENT_REMOVED */
), Qa = (e) => {
  Fe && typeof Fe.cleanupBuffer == "function" && // remove the component if it wasn't buffered
  !Fe.cleanupBuffer(e) && Ga(e);
};
// @__NO_SIDE_EFFECTS__
function fo(e) {
  return (t) => {
    zn(
      e,
      t.appContext.app,
      t.uid,
      t.parent ? t.parent.uid : void 0,
      t
    );
  };
}
const Xa = /* @__PURE__ */ bl(
  "perf:start"
  /* PERFORMANCE_START */
), za = /* @__PURE__ */ bl(
  "perf:end"
  /* PERFORMANCE_END */
);
function bl(e) {
  return (t, n, s) => {
    zn(e, t.appContext.app, t.uid, t, n, s);
  };
}
function Za(e, t, n) {
  zn(
    "component:emit",
    e.appContext.app,
    e,
    t,
    n
  );
}
let ve = null, _l = null;
function $s(e) {
  const t = ve;
  return ve = e, _l = e && e.type.__scopeId || null, t;
}
function ef(e, t = ve, n) {
  if (!t || e._n)
    return e;
  const s = (...i) => {
    s._d && lr(-1);
    const o = $s(t);
    let r;
    try {
      r = e(...i);
    } finally {
      $s(o), s._d && lr(1);
    }
    return process.env.NODE_ENV !== "production" && yl(t), r;
  };
  return s._n = !0, s._c = !0, s._d = !0, s;
}
function wl(e) {
  qc(e) && C("Do not use built-in directive ids as custom directive id: " + e);
}
function Pe(e, t) {
  if (ve === null)
    return process.env.NODE_ENV !== "production" && C("withDirectives can only be used inside render functions."), e;
  const n = Zs(ve), s = e.dirs || (e.dirs = []);
  for (let i = 0; i < t.length; i++) {
    let [o, r, l, c = ee] = t[i];
    o && (F(o) && (o = {
      mounted: o,
      updated: o
    }), o.deep && pt(r), s.push({
      dir: o,
      instance: n,
      value: r,
      oldValue: void 0,
      arg: l,
      modifiers: c
    }));
  }
  return e;
}
function Ct(e, t, n, s) {
  const i = e.dirs, o = t && t.dirs;
  for (let r = 0; r < i.length; r++) {
    const l = i[r];
    o && (l.oldValue = o[r].value);
    let c = l.dir[s];
    c && (Le(), We(c, n, 8, [
      e.el,
      l,
      e,
      t
    ]), Me());
  }
}
function tf(e, t) {
  if (process.env.NODE_ENV !== "production" && (!ue || ue.isMounted) && C("provide() can only be used inside setup()."), ue) {
    let n = ue.provides;
    const s = ue.parent && ue.parent.provides;
    s === n && (n = ue.provides = Object.create(s)), n[e] = t;
  }
}
function ws(e, t, n = !1) {
  const s = Gl();
  if (s || tn) {
    let i = tn ? tn._context.provides : s ? s.parent == null || s.ce ? s.vnode.appContext && s.vnode.appContext.provides : s.parent.provides : void 0;
    if (i && e in i)
      return i[e];
    if (arguments.length > 1)
      return n && F(t) ? t.call(s && s.proxy) : t;
    process.env.NODE_ENV !== "production" && C(`injection "${String(e)}" not found.`);
  } else process.env.NODE_ENV !== "production" && C("inject() can only be used inside setup() or functional components.");
}
const nf = /* @__PURE__ */ Symbol.for("v-scx"), sf = () => {
  {
    const e = ws(nf);
    return e || process.env.NODE_ENV !== "production" && C(
      "Server rendering context not provided. Make sure to only call useSSRContext() conditionally in the server build."
    ), e;
  }
};
function yi(e, t, n) {
  return process.env.NODE_ENV !== "production" && !F(t) && C(
    "`watch(fn, options?)` signature has been moved to a separate API. Use `watchEffect(fn, options?)` instead. `watch` now only supports `watch(source, cb, options?) signature."
  ), El(e, t, n);
}
function El(e, t, n = ee) {
  const { immediate: s, deep: i, flush: o, once: r } = n;
  process.env.NODE_ENV !== "production" && !t && (s !== void 0 && C(
    'watch() "immediate" option is only respected when using the watch(source, callback, options?) signature.'
  ), i !== void 0 && C(
    'watch() "deep" option is only respected when using the watch(source, callback, options?) signature.'
  ), r !== void 0 && C(
    'watch() "once" option is only respected when using the watch(source, callback, options?) signature.'
  ));
  const l = le({}, n);
  process.env.NODE_ENV !== "production" && (l.onWarn = C);
  const c = t && s || !t && o !== "post";
  let a;
  if (qn) {
    if (o === "sync") {
      const m = sf();
      a = m.__watcherHandles || (m.__watcherHandles = []);
    } else if (!c) {
      const m = () => {
      };
      return m.stop = pe, m.resume = pe, m.pause = pe, m;
    }
  }
  const u = ue;
  l.call = (m, b, p) => We(m, u, b, p);
  let f = !1;
  o === "post" ? l.scheduler = (m) => {
    ke(m, u && u.suspense);
  } : o !== "sync" && (f = !0, l.scheduler = (m, b) => {
    b ? m() : Gs(m);
  }), l.augmentJob = (m) => {
    t && (m.flags |= 4), f && (m.flags |= 2, u && (m.id = u.uid, m.i = u));
  };
  const h = La(e, t, l);
  return qn && (a ? a.push(h) : c && h()), h;
}
function of(e, t, n) {
  const s = this.proxy, i = se(e) ? e.includes(".") ? vl(s, e) : () => s[e] : e.bind(s, s);
  let o;
  F(t) ? o = t : (o = t.handler, n = t);
  const r = es(this), l = El(i, o.bind(s), n);
  return r(), l;
}
function vl(e, t) {
  const n = t.split(".");
  return () => {
    let s = e;
    for (let i = 0; i < n.length && s; i++)
      s = s[n[i]];
    return s;
  };
}
const rf = /* @__PURE__ */ Symbol("_vte"), lf = (e) => e.__isTeleport, bi = /* @__PURE__ */ Symbol("_leaveCb");
function uo(e, t) {
  e.shapeFlag & 6 && e.component ? (e.transition = t, uo(e.component.subTree, t)) : e.shapeFlag & 128 ? (e.ssContent.transition = t.clone(e.ssContent), e.ssFallback.transition = t.clone(e.ssFallback)) : e.transition = t;
}
// @__NO_SIDE_EFFECTS__
function cf(e, t) {
  return F(e) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    le({ name: e.name }, t, { setup: e })
  ) : e;
}
function Nl(e) {
  e.ids = [e.ids[0] + e.ids[2]++ + "-", 0, 0];
}
const Jo = /* @__PURE__ */ new WeakSet();
function Yo(e, t) {
  let n;
  return !!((n = Object.getOwnPropertyDescriptor(e, t)) && !n.configurable);
}
const Vs = /* @__PURE__ */ new WeakMap();
function Ln(e, t, n, s, i = !1) {
  if (x(e)) {
    e.forEach(
      (p, y) => Ln(
        p,
        t && (x(t) ? t[y] : t),
        n,
        s,
        i
      )
    );
    return;
  }
  if (Mn(s) && !i) {
    s.shapeFlag & 512 && s.type.__asyncResolved && s.component.subTree.component && Ln(e, t, n, s.component.subTree);
    return;
  }
  const o = s.shapeFlag & 4 ? Zs(s.component) : s.el, r = i ? null : o, { i: l, r: c } = e;
  if (process.env.NODE_ENV !== "production" && !l) {
    C(
      "Missing ref owner context. ref cannot be used on hoisted vnodes. A vnode with ref must be created inside the render function."
    );
    return;
  }
  const a = t && t.r, u = l.refs === ee ? l.refs = {} : l.refs, f = l.setupState, h = /* @__PURE__ */ q(f), m = f === ee ? Rr : (p) => process.env.NODE_ENV !== "production" && (Y(h, p) && !/* @__PURE__ */ fe(h[p]) && C(
    `Template ref "${p}" used on a non-ref value. It will not work in the production build.`
  ), Jo.has(h[p])) || Yo(u, p) ? !1 : Y(h, p), b = (p, y) => !(process.env.NODE_ENV !== "production" && Jo.has(p) || y && Yo(u, y));
  if (a != null && a !== c) {
    if (Go(t), se(a))
      u[a] = null, m(a) && (f[a] = null);
    else if (/* @__PURE__ */ fe(a)) {
      const p = t;
      b(a, p.k) && (a.value = null), p.k && (u[p.k] = null);
    }
  }
  if (F(c)) {
    Le();
    try {
      un(c, l, 12, [r, u]);
    } finally {
      Me();
    }
  } else {
    const p = se(c), y = /* @__PURE__ */ fe(c);
    if (p || y) {
      const k = () => {
        if (e.f) {
          const E = p ? m(c) ? f[c] : u[c] : b(c) || !e.k ? c.value : u[e.k];
          if (i)
            x(E) && Qi(E, o);
          else if (x(E))
            E.includes(o) || E.push(o);
          else if (p)
            u[c] = [o], m(c) && (f[c] = u[c]);
          else {
            const I = [o];
            b(c, e.k) && (c.value = I), e.k && (u[e.k] = I);
          }
        } else p ? (u[c] = r, m(c) && (f[c] = r)) : y ? (b(c, e.k) && (c.value = r), e.k && (u[e.k] = r)) : process.env.NODE_ENV !== "production" && C("Invalid template ref type:", c, `(${typeof c})`);
      };
      if (r) {
        const E = () => {
          k(), Vs.delete(e);
        };
        E.id = -1, Vs.set(e, E), ke(E, n);
      } else
        Go(e), k();
    } else process.env.NODE_ENV !== "production" && C("Invalid template ref type:", c, `(${typeof c})`);
  }
}
function Go(e) {
  const t = Vs.get(e);
  t && (t.flags |= 8, Vs.delete(e));
}
Qn().requestIdleCallback;
Qn().cancelIdleCallback;
const Mn = (e) => !!e.type.__asyncLoader, po = (e) => e.type.__isKeepAlive;
function af(e, t) {
  Ol(e, "a", t);
}
function ff(e, t) {
  Ol(e, "da", t);
}
function Ol(e, t, n = ue) {
  const s = e.__wdc || (e.__wdc = () => {
    let i = n;
    for (; i; ) {
      if (i.isDeactivated)
        return;
      i = i.parent;
    }
    return e();
  });
  if (Qs(t, s, n), n) {
    let i = n.parent;
    for (; i && i.parent; )
      po(i.parent.vnode) && uf(s, t, n, i), i = i.parent;
  }
}
function uf(e, t, n, s) {
  const i = Qs(
    t,
    e,
    s,
    !0
    /* prepend */
  );
  Sl(() => {
    Qi(s[t], i);
  }, n);
}
function Qs(e, t, n = ue, s = !1) {
  if (n) {
    const i = n[e] || (n[e] = []), o = t.__weh || (t.__weh = (...r) => {
      Le();
      const l = es(n), c = We(t, n, e, r);
      return l(), Me(), c;
    });
    return s ? i.unshift(o) : i.push(o), o;
  } else if (process.env.NODE_ENV !== "production") {
    const i = It(lo[e].replace(/ hook$/, ""));
    C(
      `${i} is called when there is no active component instance to be associated with. Lifecycle injection APIs can only be used during execution of setup(). If you are using async setup(), make sure to register lifecycle hooks before the first await statement.`
    );
  }
}
const _t = (e) => (t, n = ue) => {
  (!qn || e === "sp") && Qs(e, (...s) => t(...s), n);
}, df = _t("bm"), pf = _t("m"), hf = _t(
  "bu"
), mf = _t("u"), gf = _t(
  "bum"
), Sl = _t("um"), yf = _t(
  "sp"
), bf = _t("rtg"), _f = _t("rtc");
function wf(e, t = ue) {
  Qs("ec", e, t);
}
const Ef = /* @__PURE__ */ Symbol.for("v-ndc");
function vf(e, t, n, s) {
  let i;
  const o = n, r = x(e);
  if (r || se(e)) {
    const l = r && /* @__PURE__ */ St(e);
    let c = !1, a = !1;
    l && (c = !/* @__PURE__ */ Ne(e), a = /* @__PURE__ */ qe(e), e = Hs(e)), i = new Array(e.length);
    for (let u = 0, f = e.length; u < f; u++)
      i[u] = t(
        c ? a ? on(He(e[u])) : He(e[u]) : e[u],
        u,
        void 0,
        o
      );
  } else if (typeof e == "number")
    if (process.env.NODE_ENV !== "production" && (!Number.isInteger(e) || e < 0))
      C(
        `The v-for range expects a positive integer value but got ${e}.`
      ), i = [];
    else {
      i = new Array(e);
      for (let l = 0; l < e; l++)
        i[l] = t(l + 1, l, void 0, o);
    }
  else if (Q(e))
    if (e[Symbol.iterator])
      i = Array.from(
        e,
        (l, c) => t(l, c, void 0, o)
      );
    else {
      const l = Object.keys(e);
      i = new Array(l.length);
      for (let c = 0, a = l.length; c < a; c++) {
        const u = l[c];
        i[c] = t(e[u], u, c, o);
      }
    }
  else
    i = [];
  return i;
}
const Mi = (e) => e ? Ql(e) ? Zs(e) : Mi(e.parent) : null, Rt = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ le(/* @__PURE__ */ Object.create(null), {
    $: (e) => e,
    $el: (e) => e.vnode.el,
    $data: (e) => e.data,
    $props: (e) => process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(e.props) : e.props,
    $attrs: (e) => process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(e.attrs) : e.attrs,
    $slots: (e) => process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(e.slots) : e.slots,
    $refs: (e) => process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(e.refs) : e.refs,
    $parent: (e) => Mi(e.parent),
    $root: (e) => Mi(e.root),
    $host: (e) => e.ce,
    $emit: (e) => e.emit,
    $options: (e) => Dl(e),
    $forceUpdate: (e) => e.f || (e.f = () => {
      Gs(e.update);
    }),
    $nextTick: (e) => e.n || (e.n = ul.bind(e.proxy)),
    $watch: (e) => of.bind(e)
  })
), ho = (e) => e === "_" || e === "$", _i = (e, t) => e !== ee && !e.__isScriptSetup && Y(e, t), kl = {
  get({ _: e }, t) {
    if (t === "__v_skip")
      return !0;
    const { ctx: n, setupState: s, data: i, props: o, accessCache: r, type: l, appContext: c } = e;
    if (process.env.NODE_ENV !== "production" && t === "__isVue")
      return !0;
    if (t[0] !== "$") {
      const h = r[t];
      if (h !== void 0)
        switch (h) {
          case 1:
            return s[t];
          case 2:
            return i[t];
          case 4:
            return n[t];
          case 3:
            return o[t];
        }
      else {
        if (_i(s, t))
          return r[t] = 1, s[t];
        if (i !== ee && Y(i, t))
          return r[t] = 2, i[t];
        if (Y(o, t))
          return r[t] = 3, o[t];
        if (n !== ee && Y(n, t))
          return r[t] = 4, n[t];
        xi && (r[t] = 0);
      }
    }
    const a = Rt[t];
    let u, f;
    if (a)
      return t === "$attrs" ? (de(e.attrs, "get", ""), process.env.NODE_ENV !== "production" && Ms()) : process.env.NODE_ENV !== "production" && t === "$slots" && de(e, "get", t), a(e);
    if (
      // css module (injected by vue-loader)
      (u = l.__cssModules) && (u = u[t])
    )
      return u;
    if (n !== ee && Y(n, t))
      return r[t] = 4, n[t];
    if (
      // global properties
      f = c.config.globalProperties, Y(f, t)
    )
      return f[t];
    process.env.NODE_ENV !== "production" && ve && (!se(t) || // #1091 avoid internal isRef/isVNode checks on component instance leading
    // to infinite warning loop
    t.indexOf("__v") !== 0) && (i !== ee && ho(t[0]) && Y(i, t) ? C(
      `Property ${JSON.stringify(
        t
      )} must be accessed via $data because it starts with a reserved character ("$" or "_") and is not proxied on the render context.`
    ) : e === ve && C(
      `Property ${JSON.stringify(t)} was accessed during render but is not defined on instance.`
    ));
  },
  set({ _: e }, t, n) {
    const { data: s, setupState: i, ctx: o } = e;
    return _i(i, t) ? (i[t] = n, !0) : process.env.NODE_ENV !== "production" && i.__isScriptSetup && Y(i, t) ? (C(`Cannot mutate <script setup> binding "${t}" from Options API.`), !1) : s !== ee && Y(s, t) ? (s[t] = n, !0) : Y(e.props, t) ? (process.env.NODE_ENV !== "production" && C(`Attempting to mutate prop "${t}". Props are readonly.`), !1) : t[0] === "$" && t.slice(1) in e ? (process.env.NODE_ENV !== "production" && C(
      `Attempting to mutate public property "${t}". Properties starting with $ are reserved and readonly.`
    ), !1) : (process.env.NODE_ENV !== "production" && t in e.appContext.config.globalProperties ? Object.defineProperty(o, t, {
      enumerable: !0,
      configurable: !0,
      value: n
    }) : o[t] = n, !0);
  },
  has({
    _: { data: e, setupState: t, accessCache: n, ctx: s, appContext: i, props: o, type: r }
  }, l) {
    let c;
    return !!(n[l] || e !== ee && l[0] !== "$" && Y(e, l) || _i(t, l) || Y(o, l) || Y(s, l) || Y(Rt, l) || Y(i.config.globalProperties, l) || (c = r.__cssModules) && c[l]);
  },
  defineProperty(e, t, n) {
    return n.get != null ? e._.accessCache[t] = 0 : Y(n, "value") && this.set(e, t, n.value, null), Reflect.defineProperty(e, t, n);
  }
};
process.env.NODE_ENV !== "production" && (kl.ownKeys = (e) => (C(
  "Avoid app logic that relies on enumerating keys on a component instance. The keys will be empty in production mode to avoid performance overhead."
), Reflect.ownKeys(e)));
function Nf(e) {
  const t = {};
  return Object.defineProperty(t, "_", {
    configurable: !0,
    enumerable: !1,
    get: () => e
  }), Object.keys(Rt).forEach((n) => {
    Object.defineProperty(t, n, {
      configurable: !0,
      enumerable: !1,
      get: () => Rt[n](e),
      // intercepted by the proxy so no need for implementation,
      // but needed to prevent set errors
      set: pe
    });
  }), t;
}
function Of(e) {
  const {
    ctx: t,
    propsOptions: [n]
  } = e;
  n && Object.keys(n).forEach((s) => {
    Object.defineProperty(t, s, {
      enumerable: !0,
      configurable: !0,
      get: () => e.props[s],
      set: pe
    });
  });
}
function Sf(e) {
  const { ctx: t, setupState: n } = e;
  Object.keys(/* @__PURE__ */ q(n)).forEach((s) => {
    if (!n.__isScriptSetup) {
      if (ho(s[0])) {
        C(
          `setup() return property ${JSON.stringify(
            s
          )} should not start with "$" or "_" which are reserved prefixes for Vue internals.`
        );
        return;
      }
      Object.defineProperty(t, s, {
        enumerable: !0,
        configurable: !0,
        get: () => n[s],
        set: pe
      });
    }
  });
}
function Qo(e) {
  return x(e) ? e.reduce(
    (t, n) => (t[n] = null, t),
    {}
  ) : e;
}
function kf() {
  const e = /* @__PURE__ */ Object.create(null);
  return (t, n) => {
    e[n] ? C(`${t} property "${n}" is already defined in ${e[n]}.`) : e[n] = t;
  };
}
let xi = !0;
function Tf(e) {
  const t = Dl(e), n = e.proxy, s = e.ctx;
  xi = !1, t.beforeCreate && Xo(t.beforeCreate, e, "bc");
  const {
    // state
    data: i,
    computed: o,
    methods: r,
    watch: l,
    provide: c,
    inject: a,
    // lifecycle
    created: u,
    beforeMount: f,
    mounted: h,
    beforeUpdate: m,
    updated: b,
    activated: p,
    deactivated: y,
    beforeDestroy: k,
    beforeUnmount: E,
    destroyed: I,
    unmounted: L,
    render: S,
    renderTracked: j,
    renderTriggered: M,
    errorCaptured: V,
    serverPrefetch: U,
    // public API
    expose: ne,
    inheritAttrs: he,
    // assets
    components: ie,
    directives: ye,
    filters: yn
  } = t, wt = process.env.NODE_ENV !== "production" ? kf() : null;
  if (process.env.NODE_ENV !== "production") {
    const [J] = e.propsOptions;
    if (J)
      for (const W in J)
        wt("Props", W);
  }
  if (a && Df(a, s, wt), r)
    for (const J in r) {
      const W = r[J];
      F(W) ? (process.env.NODE_ENV !== "production" ? Object.defineProperty(s, J, {
        value: W.bind(n),
        configurable: !0,
        enumerable: !0,
        writable: !0
      }) : s[J] = W.bind(n), process.env.NODE_ENV !== "production" && wt("Methods", J)) : process.env.NODE_ENV !== "production" && C(
        `Method "${J}" has type "${typeof W}" in the component definition. Did you reference the function correctly?`
      );
    }
  if (i) {
    process.env.NODE_ENV !== "production" && !F(i) && C(
      "The data option must be a function. Plain object usage is no longer supported."
    );
    const J = i.call(n, n);
    if (process.env.NODE_ENV !== "production" && Xi(J) && C(
      "data() returned a Promise - note data() cannot be async; If you intend to perform data fetching before component renders, use async setup() + <Suspense>."
    ), !Q(J))
      process.env.NODE_ENV !== "production" && C("data() should return an object.");
    else if (e.data = /* @__PURE__ */ Js(J), process.env.NODE_ENV !== "production")
      for (const W in J)
        wt("Data", W), ho(W[0]) || Object.defineProperty(s, W, {
          configurable: !0,
          enumerable: !0,
          get: () => J[W],
          set: pe
        });
  }
  if (xi = !0, o)
    for (const J in o) {
      const W = o[J], Ye = F(W) ? W.bind(n, n) : F(W.get) ? W.get.bind(n, n) : pe;
      process.env.NODE_ENV !== "production" && Ye === pe && C(`Computed property "${J}" has no getter.`);
      const ci = !F(W) && F(W.set) ? W.set.bind(n) : process.env.NODE_ENV !== "production" ? () => {
        C(
          `Write operation failed: computed property "${J}" is readonly.`
        );
      } : pe, bn = vu({
        get: Ye,
        set: ci
      });
      Object.defineProperty(s, J, {
        enumerable: !0,
        configurable: !0,
        get: () => bn.value,
        set: (Kt) => bn.value = Kt
      }), process.env.NODE_ENV !== "production" && wt("Computed", J);
    }
  if (l)
    for (const J in l)
      Tl(l[J], s, n, J);
  if (c) {
    const J = F(c) ? c.call(n) : c;
    Reflect.ownKeys(J).forEach((W) => {
      tf(W, J[W]);
    });
  }
  u && Xo(u, e, "c");
  function Se(J, W) {
    x(W) ? W.forEach((Ye) => J(Ye.bind(n))) : W && J(W.bind(n));
  }
  if (Se(df, f), Se(pf, h), Se(hf, m), Se(mf, b), Se(af, p), Se(ff, y), Se(wf, V), Se(_f, j), Se(bf, M), Se(gf, E), Se(Sl, L), Se(yf, U), x(ne))
    if (ne.length) {
      const J = e.exposed || (e.exposed = {});
      ne.forEach((W) => {
        Object.defineProperty(J, W, {
          get: () => n[W],
          set: (Ye) => n[W] = Ye,
          enumerable: !0
        });
      });
    } else e.exposed || (e.exposed = {});
  S && e.render === pe && (e.render = S), he != null && (e.inheritAttrs = he), ie && (e.components = ie), ye && (e.directives = ye), U && Nl(e);
}
function Df(e, t, n = pe) {
  x(e) && (e = Pi(e));
  for (const s in e) {
    const i = e[s];
    let o;
    Q(i) ? "default" in i ? o = ws(
      i.from || s,
      i.default,
      !0
    ) : o = ws(i.from || s) : o = ws(i), /* @__PURE__ */ fe(o) ? Object.defineProperty(t, s, {
      enumerable: !0,
      configurable: !0,
      get: () => o.value,
      set: (r) => o.value = r
    }) : t[s] = o, process.env.NODE_ENV !== "production" && n("Inject", s);
  }
}
function Xo(e, t, n) {
  We(
    x(e) ? e.map((s) => s.bind(t.proxy)) : e.bind(t.proxy),
    t,
    n
  );
}
function Tl(e, t, n, s) {
  let i = s.includes(".") ? vl(n, s) : () => n[s];
  if (se(e)) {
    const o = t[e];
    F(o) ? yi(i, o) : process.env.NODE_ENV !== "production" && C(`Invalid watch handler specified by key "${e}"`, o);
  } else if (F(e))
    yi(i, e.bind(n));
  else if (Q(e))
    if (x(e))
      e.forEach((o) => Tl(o, t, n, s));
    else {
      const o = F(e.handler) ? e.handler.bind(n) : t[e.handler];
      F(o) ? yi(i, o, e) : process.env.NODE_ENV !== "production" && C(`Invalid watch handler specified by key "${e.handler}"`, o);
    }
  else process.env.NODE_ENV !== "production" && C(`Invalid watch option: "${s}"`, e);
}
function Dl(e) {
  const t = e.type, { mixins: n, extends: s } = t, {
    mixins: i,
    optionsCache: o,
    config: { optionMergeStrategies: r }
  } = e.appContext, l = o.get(t);
  let c;
  return l ? c = l : !i.length && !n && !s ? c = t : (c = {}, i.length && i.forEach(
    (a) => Ls(c, a, r, !0)
  ), Ls(c, t, r)), Q(t) && o.set(t, c), c;
}
function Ls(e, t, n, s = !1) {
  const { mixins: i, extends: o } = t;
  o && Ls(e, o, n, !0), i && i.forEach(
    (r) => Ls(e, r, n, !0)
  );
  for (const r in t)
    if (s && r === "expose")
      process.env.NODE_ENV !== "production" && C(
        '"expose" option is ignored when declared in mixins or extends. It should only be declared in the base component itself.'
      );
    else {
      const l = Cf[r] || n && n[r];
      e[r] = l ? l(e[r], t[r]) : t[r];
    }
  return e;
}
const Cf = {
  data: zo,
  props: Zo,
  emits: Zo,
  // objects
  methods: Tn,
  computed: Tn,
  // lifecycle
  beforeCreate: _e,
  created: _e,
  beforeMount: _e,
  mounted: _e,
  beforeUpdate: _e,
  updated: _e,
  beforeDestroy: _e,
  beforeUnmount: _e,
  destroyed: _e,
  unmounted: _e,
  activated: _e,
  deactivated: _e,
  errorCaptured: _e,
  serverPrefetch: _e,
  // assets
  components: Tn,
  directives: Tn,
  // watch
  watch: If,
  // provide / inject
  provide: zo,
  inject: Af
};
function zo(e, t) {
  return t ? e ? function() {
    return le(
      F(e) ? e.call(this, this) : e,
      F(t) ? t.call(this, this) : t
    );
  } : t : e;
}
function Af(e, t) {
  return Tn(Pi(e), Pi(t));
}
function Pi(e) {
  if (x(e)) {
    const t = {};
    for (let n = 0; n < e.length; n++)
      t[e[n]] = e[n];
    return t;
  }
  return e;
}
function _e(e, t) {
  return e ? [...new Set([].concat(e, t))] : t;
}
function Tn(e, t) {
  return e ? le(/* @__PURE__ */ Object.create(null), e, t) : t;
}
function Zo(e, t) {
  return e ? x(e) && x(t) ? [.../* @__PURE__ */ new Set([...e, ...t])] : le(
    /* @__PURE__ */ Object.create(null),
    Qo(e),
    Qo(t ?? {})
  ) : t;
}
function If(e, t) {
  if (!e) return t;
  if (!t) return e;
  const n = le(/* @__PURE__ */ Object.create(null), e);
  for (const s in t)
    n[s] = _e(e[s], t[s]);
  return n;
}
function Cl() {
  return {
    app: null,
    config: {
      isNativeTag: Rr,
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
let $f = 0;
function Vf(e, t) {
  return function(s, i = null) {
    F(s) || (s = le({}, s)), i != null && !Q(i) && (process.env.NODE_ENV !== "production" && C("root props passed to app.mount() must be an object."), i = null);
    const o = Cl(), r = /* @__PURE__ */ new WeakSet(), l = [];
    let c = !1;
    const a = o.app = {
      _uid: $f++,
      _component: s,
      _props: i,
      _container: null,
      _context: o,
      _instance: null,
      version: ur,
      get config() {
        return o.config;
      },
      set config(u) {
        process.env.NODE_ENV !== "production" && C(
          "app.config cannot be replaced. Modify individual options instead."
        );
      },
      use(u, ...f) {
        return r.has(u) ? process.env.NODE_ENV !== "production" && C("Plugin has already been applied to target app.") : u && F(u.install) ? (r.add(u), u.install(a, ...f)) : F(u) ? (r.add(u), u(a, ...f)) : process.env.NODE_ENV !== "production" && C(
          'A plugin must either be a function or an object with an "install" function.'
        ), a;
      },
      mixin(u) {
        return o.mixins.includes(u) ? process.env.NODE_ENV !== "production" && C(
          "Mixin has already been applied to target app" + (u.name ? `: ${u.name}` : "")
        ) : o.mixins.push(u), a;
      },
      component(u, f) {
        return process.env.NODE_ENV !== "production" && Ki(u, o.config), f ? (process.env.NODE_ENV !== "production" && o.components[u] && C(`Component "${u}" has already been registered in target app.`), o.components[u] = f, a) : o.components[u];
      },
      directive(u, f) {
        return process.env.NODE_ENV !== "production" && wl(u), f ? (process.env.NODE_ENV !== "production" && o.directives[u] && C(`Directive "${u}" has already been registered in target app.`), o.directives[u] = f, a) : o.directives[u];
      },
      mount(u, f, h) {
        if (c)
          process.env.NODE_ENV !== "production" && C(
            "App has already been mounted.\nIf you want to remount the same app, move your app creation logic into a factory function and create fresh app instances for each mount - e.g. `const createMyApp = () => createApp(App)`"
          );
        else {
          process.env.NODE_ENV !== "production" && u.__vue_app__ && C(
            "There is already an app instance mounted on the host container.\n If you want to mount another app on the same host container, you need to unmount the previous app by calling `app.unmount()` first."
          );
          const m = a._ceVNode || mt(s, i);
          return m.appContext = o, h === !0 ? h = "svg" : h === !1 && (h = void 0), process.env.NODE_ENV !== "production" && (o.reload = () => {
            const b = Dt(m);
            b.el = null, e(b, u, h);
          }), e(m, u, h), c = !0, a._container = u, u.__vue_app__ = a, process.env.NODE_ENV !== "production" && (a._instance = m.component, Wa(a, ur)), Zs(m.component);
        }
      },
      onUnmount(u) {
        process.env.NODE_ENV !== "production" && typeof u != "function" && C(
          `Expected function as first argument to app.onUnmount(), but got ${typeof u}`
        ), l.push(u);
      },
      unmount() {
        c ? (We(
          l,
          a._instance,
          16
        ), e(null, a._container), process.env.NODE_ENV !== "production" && (a._instance = null, Ja(a)), delete a._container.__vue_app__) : process.env.NODE_ENV !== "production" && C("Cannot unmount an app that is not mounted.");
      },
      provide(u, f) {
        return process.env.NODE_ENV !== "production" && u in o.provides && (Y(o.provides, u) ? C(
          `App already provides property with key "${String(u)}". It will be overwritten with the new value.`
        ) : C(
          `App already provides property with key "${String(u)}" inherited from its parent element. It will be overwritten with the new value.`
        )), o.provides[u] = f, a;
      },
      runWithContext(u) {
        const f = tn;
        tn = a;
        try {
          return u();
        } finally {
          tn = f;
        }
      }
    };
    return a;
  };
}
let tn = null;
const Lf = (e, t) => t === "modelValue" || t === "model-value" ? e.modelModifiers : e[`${t}Modifiers`] || e[`${Te(t)}Modifiers`] || e[`${Tt(t)}Modifiers`];
function Mf(e, t, ...n) {
  if (e.isUnmounted) return;
  const s = e.vnode.props || ee;
  if (process.env.NODE_ENV !== "production") {
    const {
      emitsOptions: u,
      propsOptions: [f]
    } = e;
    if (u)
      if (!(t in u))
        (!f || !(It(Te(t)) in f)) && C(
          `Component emitted event "${t}" but it is neither declared in the emits option nor as an "${It(Te(t))}" prop.`
        );
      else {
        const h = u[t];
        F(h) && (h(...n) || C(
          `Invalid event arguments: event validation failed for event "${t}".`
        ));
      }
  }
  let i = n;
  const o = t.startsWith("update:"), r = o && Lf(s, t.slice(7));
  if (r && (r.trim && (i = n.map((u) => se(u) ? u.trim() : u)), r.number && (i = n.map(qs))), process.env.NODE_ENV !== "production" && Za(e, t, i), process.env.NODE_ENV !== "production") {
    const u = t.toLowerCase();
    u !== t && s[It(u)] && C(
      `Event "${u}" is emitted in component ${ts(
        e,
        e.type
      )} but the handler is registered for "${t}". Note that HTML attributes are case-insensitive and you cannot use v-on to listen to camelCase events when using in-DOM templates. You should probably use "${Tt(
        t
      )}" instead of "${t}".`
    );
  }
  let l, c = s[l = It(t)] || // also try camelCase event handler (#2249)
  s[l = It(Te(t))];
  !c && o && (c = s[l = It(Tt(t))]), c && We(
    c,
    e,
    6,
    i
  );
  const a = s[l + "Once"];
  if (a) {
    if (!e.emitted)
      e.emitted = {};
    else if (e.emitted[l])
      return;
    e.emitted[l] = !0, We(
      a,
      e,
      6,
      i
    );
  }
}
const xf = /* @__PURE__ */ new WeakMap();
function Al(e, t, n = !1) {
  const s = n ? xf : t.emitsCache, i = s.get(e);
  if (i !== void 0)
    return i;
  const o = e.emits;
  let r = {}, l = !1;
  if (!F(e)) {
    const c = (a) => {
      const u = Al(a, t, !0);
      u && (l = !0, le(r, u));
    };
    !n && t.mixins.length && t.mixins.forEach(c), e.extends && c(e.extends), e.mixins && e.mixins.forEach(c);
  }
  return !o && !l ? (Q(e) && s.set(e, null), null) : (x(o) ? o.forEach((c) => r[c] = null) : le(r, o), Q(e) && s.set(e, r), r);
}
function Xs(e, t) {
  return !e || !Yn(t) ? !1 : (t = t.slice(2), t = t === "Once" ? t : t.replace(/Once$/, ""), Y(e, t[0].toLowerCase() + t.slice(1)) || Y(e, Tt(t)) || Y(e, t));
}
let ji = !1;
function Ms() {
  ji = !0;
}
function er(e) {
  const {
    type: t,
    vnode: n,
    proxy: s,
    withProxy: i,
    propsOptions: [o],
    slots: r,
    attrs: l,
    emit: c,
    render: a,
    renderCache: u,
    props: f,
    data: h,
    setupState: m,
    ctx: b,
    inheritAttrs: p
  } = e, y = $s(e);
  let k, E;
  process.env.NODE_ENV !== "production" && (ji = !1);
  try {
    if (n.shapeFlag & 4) {
      const S = i || s, j = process.env.NODE_ENV !== "production" && m.__isScriptSetup ? new Proxy(S, {
        get(M, V, U) {
          return C(
            `Property '${String(
              V
            )}' was accessed via 'this'. Avoid using 'this' in templates.`
          ), Reflect.get(M, V, U);
        }
      }) : S;
      k = Re(
        a.call(
          j,
          S,
          u,
          process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(f) : f,
          m,
          h,
          b
        )
      ), E = l;
    } else {
      const S = t;
      process.env.NODE_ENV !== "production" && l === f && Ms(), k = Re(
        S.length > 1 ? S(
          process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(f) : f,
          process.env.NODE_ENV !== "production" ? {
            get attrs() {
              return Ms(), /* @__PURE__ */ it(l);
            },
            slots: r,
            emit: c
          } : { attrs: l, slots: r, emit: c }
        ) : S(
          process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(f) : f,
          null
        )
      ), E = t.props ? l : Pf(l);
    }
  } catch (S) {
    xn.length = 0, Xn(S, e, 1), k = mt(Ae);
  }
  let I = k, L;
  if (process.env.NODE_ENV !== "production" && k.patchFlag > 0 && k.patchFlag & 2048 && ([I, L] = Il(k)), E && p !== !1) {
    const S = Object.keys(E), { shapeFlag: j } = I;
    if (S.length) {
      if (j & 7)
        o && S.some(Rn) && (E = jf(
          E,
          o
        )), I = Dt(I, E, !1, !0);
      else if (process.env.NODE_ENV !== "production" && !ji && I.type !== Ae) {
        const M = Object.keys(l), V = [], U = [];
        for (let ne = 0, he = M.length; ne < he; ne++) {
          const ie = M[ne];
          Yn(ie) ? Rn(ie) || V.push(ie[2].toLowerCase() + ie.slice(3)) : U.push(ie);
        }
        U.length && C(
          `Extraneous non-props attributes (${U.join(", ")}) were passed to component but could not be automatically inherited because component renders fragment or text or teleport root nodes.`
        ), V.length && C(
          `Extraneous non-emits event listeners (${V.join(", ")}) were passed to component but could not be automatically inherited because component renders fragment or text root nodes. If the listener is intended to be a component custom event listener only, declare it using the "emits" option.`
        );
      }
    }
  }
  return n.dirs && (process.env.NODE_ENV !== "production" && !tr(I) && C(
    "Runtime directive used on component with non-element root node. The directives will not function as intended."
  ), I = Dt(I, null, !1, !0), I.dirs = I.dirs ? I.dirs.concat(n.dirs) : n.dirs), n.transition && (process.env.NODE_ENV !== "production" && !tr(I) && C(
    "Component inside <Transition> renders non-element root node that cannot be animated."
  ), uo(I, n.transition)), process.env.NODE_ENV !== "production" && L ? L(I) : k = I, $s(y), k;
}
const Il = (e) => {
  const t = e.children, n = e.dynamicChildren, s = mo(t, !1);
  if (s) {
    if (process.env.NODE_ENV !== "production" && s.patchFlag > 0 && s.patchFlag & 2048)
      return Il(s);
  } else return [e, void 0];
  const i = t.indexOf(s), o = n ? n.indexOf(s) : -1, r = (l) => {
    t[i] = l, n && (o > -1 ? n[o] = l : l.patchFlag > 0 && (e.dynamicChildren = [...n, l]));
  };
  return [Re(s), r];
};
function mo(e, t = !0) {
  let n;
  for (let s = 0; s < e.length; s++) {
    const i = e[s];
    if (zs(i)) {
      if (i.type !== Ae || i.children === "v-if") {
        if (n)
          return;
        if (n = i, process.env.NODE_ENV !== "production" && t && n.patchFlag > 0 && n.patchFlag & 2048)
          return mo(n.children);
      }
    } else
      return;
  }
  return n;
}
const Pf = (e) => {
  let t;
  for (const n in e)
    (n === "class" || n === "style" || Yn(n)) && ((t || (t = {}))[n] = e[n]);
  return t;
}, jf = (e, t) => {
  const n = {};
  for (const s in e)
    (!Rn(s) || !(s.slice(9) in t)) && (n[s] = e[s]);
  return n;
}, tr = (e) => e.shapeFlag & 7 || e.type === Ae;
function Rf(e, t, n) {
  const { props: s, children: i, component: o } = e, { props: r, children: l, patchFlag: c } = t, a = o.emitsOptions;
  if (process.env.NODE_ENV !== "production" && (i || l) && De || t.dirs || t.transition)
    return !0;
  if (n && c >= 0) {
    if (c & 1024)
      return !0;
    if (c & 16)
      return s ? nr(s, r, a) : !!r;
    if (c & 8) {
      const u = t.dynamicProps;
      for (let f = 0; f < u.length; f++) {
        const h = u[f];
        if ($l(r, s, h) && !Xs(a, h))
          return !0;
      }
    }
  } else
    return (i || l) && (!l || !l.$stable) ? !0 : s === r ? !1 : s ? r ? nr(s, r, a) : !0 : !!r;
  return !1;
}
function nr(e, t, n) {
  const s = Object.keys(t);
  if (s.length !== Object.keys(e).length)
    return !0;
  for (let i = 0; i < s.length; i++) {
    const o = s[i];
    if ($l(t, e, o) && !Xs(n, o))
      return !0;
  }
  return !1;
}
function $l(e, t, n) {
  const s = e[n], i = t[n];
  return n === "style" && Q(s) && Q(i) ? !fn(s, i) : s !== i;
}
function Ff({ vnode: e, parent: t, suspense: n }, s) {
  for (; t; ) {
    const i = t.subTree;
    if (i.suspense && i.suspense.activeBranch === e && (i.suspense.vnode.el = i.el = s, e = i), i === e)
      (e = t.vnode).el = s, t = t.parent;
    else
      break;
  }
  n && n.activeBranch === e && (n.vnode.el = s);
}
const Vl = {}, Ll = () => Object.create(Vl), Ml = (e) => Object.getPrototypeOf(e) === Vl;
function Bf(e, t, n, s = !1) {
  const i = {}, o = Ll();
  e.propsDefaults = /* @__PURE__ */ Object.create(null), xl(e, t, i, o);
  for (const r in e.propsOptions[0])
    r in i || (i[r] = void 0);
  process.env.NODE_ENV !== "production" && jl(t || {}, i, e), n ? e.props = s ? i : /* @__PURE__ */ Sa(i) : e.type.props ? e.props = i : e.props = o, e.attrs = o;
}
function Kf(e) {
  for (; e; ) {
    if (e.type.__hmrId) return !0;
    e = e.parent;
  }
}
function Uf(e, t, n, s) {
  const {
    props: i,
    attrs: o,
    vnode: { patchFlag: r }
  } = e, l = /* @__PURE__ */ q(i), [c] = e.propsOptions;
  let a = !1;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    !(process.env.NODE_ENV !== "production" && Kf(e)) && (s || r > 0) && !(r & 16)
  ) {
    if (r & 8) {
      const u = e.vnode.dynamicProps;
      for (let f = 0; f < u.length; f++) {
        let h = u[f];
        if (Xs(e.emitsOptions, h))
          continue;
        const m = t[h];
        if (c)
          if (Y(o, h))
            m !== o[h] && (o[h] = m, a = !0);
          else {
            const b = Te(h);
            i[b] = Ri(
              c,
              l,
              b,
              m,
              e,
              !1
            );
          }
        else
          m !== o[h] && (o[h] = m, a = !0);
      }
    }
  } else {
    xl(e, t, i, o) && (a = !0);
    let u;
    for (const f in l)
      (!t || // for camelCase
      !Y(t, f) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((u = Tt(f)) === f || !Y(t, u))) && (c ? n && // for camelCase
      (n[f] !== void 0 || // for kebab-case
      n[u] !== void 0) && (i[f] = Ri(
        c,
        l,
        f,
        void 0,
        e,
        !0
      )) : delete i[f]);
    if (o !== l)
      for (const f in o)
        (!t || !Y(t, f)) && (delete o[f], a = !0);
  }
  a && st(e.attrs, "set", ""), process.env.NODE_ENV !== "production" && jl(t || {}, i, e);
}
function xl(e, t, n, s) {
  const [i, o] = e.propsOptions;
  let r = !1, l;
  if (t)
    for (let c in t) {
      if (In(c))
        continue;
      const a = t[c];
      let u;
      i && Y(i, u = Te(c)) ? !o || !o.includes(u) ? n[u] = a : (l || (l = {}))[u] = a : Xs(e.emitsOptions, c) || (!(c in s) || a !== s[c]) && (s[c] = a, r = !0);
    }
  if (o) {
    const c = /* @__PURE__ */ q(n), a = l || ee;
    for (let u = 0; u < o.length; u++) {
      const f = o[u];
      n[f] = Ri(
        i,
        c,
        f,
        a[f],
        e,
        !Y(a, f)
      );
    }
  }
  return r;
}
function Ri(e, t, n, s, i, o) {
  const r = e[n];
  if (r != null) {
    const l = Y(r, "default");
    if (l && s === void 0) {
      const c = r.default;
      if (r.type !== Function && !r.skipFactory && F(c)) {
        const { propsDefaults: a } = i;
        if (n in a)
          s = a[n];
        else {
          const u = es(i);
          s = a[n] = c.call(
            null,
            t
          ), u();
        }
      } else
        s = c;
      i.ce && i.ce._setProp(n, s);
    }
    r[
      0
      /* shouldCast */
    ] && (o && !l ? s = !1 : r[
      1
      /* shouldCastTrue */
    ] && (s === "" || s === Tt(n)) && (s = !0));
  }
  return s;
}
const qf = /* @__PURE__ */ new WeakMap();
function Pl(e, t, n = !1) {
  const s = n ? qf : t.propsCache, i = s.get(e);
  if (i)
    return i;
  const o = e.props, r = {}, l = [];
  let c = !1;
  if (!F(e)) {
    const u = (f) => {
      c = !0;
      const [h, m] = Pl(f, t, !0);
      le(r, h), m && l.push(...m);
    };
    !n && t.mixins.length && t.mixins.forEach(u), e.extends && u(e.extends), e.mixins && e.mixins.forEach(u);
  }
  if (!o && !c)
    return Q(e) && s.set(e, Zt), Zt;
  if (x(o))
    for (let u = 0; u < o.length; u++) {
      process.env.NODE_ENV !== "production" && !se(o[u]) && C("props must be strings when using array syntax.", o[u]);
      const f = Te(o[u]);
      sr(f) && (r[f] = ee);
    }
  else if (o) {
    process.env.NODE_ENV !== "production" && !Q(o) && C("invalid props options", o);
    for (const u in o) {
      const f = Te(u);
      if (sr(f)) {
        const h = o[u], m = r[f] = x(h) || F(h) ? { type: h } : le({}, h), b = m.type;
        let p = !1, y = !0;
        if (x(b))
          for (let k = 0; k < b.length; ++k) {
            const E = b[k], I = F(E) && E.name;
            if (I === "Boolean") {
              p = !0;
              break;
            } else I === "String" && (y = !1);
          }
        else
          p = F(b) && b.name === "Boolean";
        m[
          0
          /* shouldCast */
        ] = p, m[
          1
          /* shouldCastTrue */
        ] = y, (p || Y(m, "default")) && l.push(f);
      }
    }
  }
  const a = [r, l];
  return Q(e) && s.set(e, a), a;
}
function sr(e) {
  return e[0] !== "$" && !In(e) ? !0 : (process.env.NODE_ENV !== "production" && C(`Invalid prop name: "${e}" is a reserved property.`), !1);
}
function Hf(e) {
  return e === null ? "null" : typeof e == "function" ? e.name || "" : typeof e == "object" && e.constructor && e.constructor.name || "";
}
function jl(e, t, n) {
  const s = /* @__PURE__ */ q(t), i = n.propsOptions[0], o = Object.keys(e).map((r) => Te(r));
  for (const r in i) {
    let l = i[r];
    l != null && Wf(
      r,
      s[r],
      l,
      process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(s) : s,
      !o.includes(r)
    );
  }
}
function Wf(e, t, n, s, i) {
  const { type: o, required: r, validator: l, skipCheck: c } = n;
  if (r && i) {
    C('Missing required prop: "' + e + '"');
    return;
  }
  if (!(t == null && !r)) {
    if (o != null && o !== !0 && !c) {
      let a = !1;
      const u = x(o) ? o : [o], f = [];
      for (let h = 0; h < u.length && !a; h++) {
        const { valid: m, expectedType: b } = Yf(t, u[h]);
        f.push(b || ""), a = m;
      }
      if (!a) {
        C(Gf(e, t, f));
        return;
      }
    }
    l && !l(t, s) && C('Invalid prop: custom validator check failed for prop "' + e + '".');
  }
}
const Jf = /* @__PURE__ */ bt(
  "String,Number,Boolean,Function,Symbol,BigInt"
);
function Yf(e, t) {
  let n;
  const s = Hf(t);
  if (s === "null")
    n = e === null;
  else if (Jf(s)) {
    const i = typeof e;
    n = i === s.toLowerCase(), !n && i === "object" && (n = e instanceof t);
  } else s === "Object" ? n = Q(e) : s === "Array" ? n = x(e) : n = e instanceof t;
  return {
    valid: n,
    expectedType: s
  };
}
function Gf(e, t, n) {
  if (n.length === 0)
    return `Prop type [] for prop "${e}" won't match anything. Did you mean to use type Array instead?`;
  let s = `Invalid prop: type check failed for prop "${e}". Expected ${n.map(Us).join(" | ")}`;
  const i = n[0], o = zi(t), r = ir(t, i), l = ir(t, o);
  return n.length === 1 && or(i) && Qf(i, o) && (s += ` with value ${r}`), s += `, got ${o} `, or(o) && (s += `with value ${l}.`), s;
}
function ir(e, t) {
  return Ke(e) ? e.toString() : t === "String" ? `"${e}"` : t === "Number" ? `${Number(e)}` : `${e}`;
}
function or(e) {
  return ["string", "number", "boolean"].some((n) => e.toLowerCase() === n);
}
function Qf(...e) {
  return e.every((t) => {
    const n = t.toLowerCase();
    return n !== "boolean" && n !== "symbol";
  });
}
const go = (e) => e === "_" || e === "_ctx" || e === "$stable", yo = (e) => x(e) ? e.map(Re) : [Re(e)], Xf = (e, t, n) => {
  if (t._n)
    return t;
  const s = ef((...i) => (process.env.NODE_ENV !== "production" && ue && !(n === null && ve) && !(n && n.root !== ue.root) && C(
    `Slot "${e}" invoked outside of the render function: this will not track dependencies used in the slot. Invoke the slot function inside the render function instead.`
  ), yo(t(...i))), n);
  return s._c = !1, s;
}, Rl = (e, t, n) => {
  const s = e._ctx;
  for (const i in e) {
    if (go(i)) continue;
    const o = e[i];
    if (F(o))
      t[i] = Xf(i, o, s);
    else if (o != null) {
      process.env.NODE_ENV !== "production" && C(
        `Non-function value encountered for slot "${i}". Prefer function slots for better performance.`
      );
      const r = yo(o);
      t[i] = () => r;
    }
  }
}, Fl = (e, t) => {
  process.env.NODE_ENV !== "production" && !po(e.vnode) && C(
    "Non-function value encountered for default slot. Prefer function slots for better performance."
  );
  const n = yo(t);
  e.slots.default = () => n;
}, Fi = (e, t, n) => {
  for (const s in t)
    (n || !go(s)) && (e[s] = t[s]);
}, zf = (e, t, n) => {
  const s = e.slots = Ll();
  if (e.vnode.shapeFlag & 32) {
    const i = t._;
    i ? (Fi(s, t, n), n && Ts(s, "_", i, !0)) : Rl(t, s);
  } else t && Fl(e, t);
}, Zf = (e, t, n) => {
  const { vnode: s, slots: i } = e;
  let o = !0, r = ee;
  if (s.shapeFlag & 32) {
    const l = t._;
    l ? process.env.NODE_ENV !== "production" && De ? (Fi(i, t, n), st(e, "set", "$slots")) : n && l === 1 ? o = !1 : Fi(i, t, n) : (o = !t.$stable, Rl(t, i)), r = t;
  } else t && (Fl(e, t), r = { default: 1 });
  if (o)
    for (const l in i)
      !go(l) && r[l] == null && delete i[l];
};
let Nn, ut;
function qt(e, t) {
  e.appContext.config.performance && xs() && ut.mark(`vue-${t}-${e.uid}`), process.env.NODE_ENV !== "production" && Xa(e, t, xs() ? ut.now() : Date.now());
}
function Ht(e, t) {
  if (e.appContext.config.performance && xs()) {
    const n = `vue-${t}-${e.uid}`, s = n + ":end", i = `<${ts(e, e.type)}> ${t}`;
    ut.mark(s), ut.measure(i, n, s), ut.clearMeasures(i), ut.clearMarks(n), ut.clearMarks(s);
  }
  process.env.NODE_ENV !== "production" && za(e, t, xs() ? ut.now() : Date.now());
}
function xs() {
  return Nn !== void 0 || (typeof window < "u" && window.performance ? (Nn = !0, ut = window.performance) : Nn = !1), Nn;
}
function eu() {
  const e = [];
  if (process.env.NODE_ENV !== "production" && e.length) {
    const t = e.length > 1;
    console.warn(
      `Feature flag${t ? "s" : ""} ${e.join(", ")} ${t ? "are" : "is"} not explicitly defined. You are running the esm-bundler build of Vue, which expects these compile-time feature flags to be globally injected via the bundler config in order to get better tree-shaking in the production bundle.

For more details, see https://link.vuejs.org/feature-flags.`
    );
  }
}
const ke = ou;
function tu(e) {
  return nu(e);
}
function nu(e, t) {
  eu();
  const n = Qn();
  n.__VUE__ = !0, process.env.NODE_ENV !== "production" && ao(n.__VUE_DEVTOOLS_GLOBAL_HOOK__, n);
  const {
    insert: s,
    remove: i,
    patchProp: o,
    createElement: r,
    createText: l,
    createComment: c,
    setText: a,
    setElementText: u,
    parentNode: f,
    nextSibling: h,
    setScopeId: m = pe,
    insertStaticContent: b
  } = e, p = (d, g, _, O = null, v = null, w = null, A = void 0, D = null, T = process.env.NODE_ENV !== "production" && De ? !1 : !!g.dynamicChildren) => {
    if (d === g)
      return;
    d && !On(d, g) && (O = ls(d), Et(d, v, w, !0), d = null), g.patchFlag === -2 && (T = !1, g.dynamicChildren = null);
    const { type: N, ref: R, shapeFlag: $ } = g;
    switch (N) {
      case Zn:
        y(d, g, _, O);
        break;
      case Ae:
        k(d, g, _, O);
        break;
      case vs:
        d == null ? E(g, _, O, A) : process.env.NODE_ENV !== "production" && I(d, g, _, A);
        break;
      case Ie:
        ye(
          d,
          g,
          _,
          O,
          v,
          w,
          A,
          D,
          T
        );
        break;
      default:
        $ & 1 ? j(
          d,
          g,
          _,
          O,
          v,
          w,
          A,
          D,
          T
        ) : $ & 6 ? yn(
          d,
          g,
          _,
          O,
          v,
          w,
          A,
          D,
          T
        ) : $ & 64 || $ & 128 ? N.process(
          d,
          g,
          _,
          O,
          v,
          w,
          A,
          D,
          T,
          wn
        ) : process.env.NODE_ENV !== "production" && C("Invalid VNode type:", N, `(${typeof N})`);
    }
    R != null && v ? Ln(R, d && d.ref, w, g || d, !g) : R == null && d && d.ref != null && Ln(d.ref, null, w, d, !0);
  }, y = (d, g, _, O) => {
    if (d == null)
      s(
        g.el = l(g.children),
        _,
        O
      );
    else {
      const v = g.el = d.el;
      g.children !== d.children && a(v, g.children);
    }
  }, k = (d, g, _, O) => {
    d == null ? s(
      g.el = c(g.children || ""),
      _,
      O
    ) : g.el = d.el;
  }, E = (d, g, _, O) => {
    [d.el, d.anchor] = b(
      d.children,
      g,
      _,
      O,
      d.el,
      d.anchor
    );
  }, I = (d, g, _, O) => {
    if (g.children !== d.children) {
      const v = h(d.anchor);
      S(d), [g.el, g.anchor] = b(
        g.children,
        _,
        v,
        O
      );
    } else
      g.el = d.el, g.anchor = d.anchor;
  }, L = ({ el: d, anchor: g }, _, O) => {
    let v;
    for (; d && d !== g; )
      v = h(d), s(d, _, O), d = v;
    s(g, _, O);
  }, S = ({ el: d, anchor: g }) => {
    let _;
    for (; d && d !== g; )
      _ = h(d), i(d), d = _;
    i(g);
  }, j = (d, g, _, O, v, w, A, D, T) => {
    if (g.type === "svg" ? A = "svg" : g.type === "math" && (A = "mathml"), d == null)
      M(
        g,
        _,
        O,
        v,
        w,
        A,
        D,
        T
      );
    else {
      const N = d.el && d.el._isVueCE ? d.el : null;
      try {
        N && N._beginPatch(), ne(
          d,
          g,
          v,
          w,
          A,
          D,
          T
        );
      } finally {
        N && N._endPatch();
      }
    }
  }, M = (d, g, _, O, v, w, A, D) => {
    let T, N;
    const { props: R, shapeFlag: $, transition: P, dirs: B } = d;
    if (T = d.el = r(
      d.type,
      w,
      R && R.is,
      R
    ), $ & 8 ? u(T, d.children) : $ & 16 && U(
      d.children,
      T,
      null,
      O,
      v,
      wi(d, w),
      A,
      D
    ), B && Ct(d, null, O, "created"), V(T, d, d.scopeId, A, O), R) {
      for (const z in R)
        z !== "value" && !In(z) && o(T, z, null, R[z], w, O);
      "value" in R && o(T, "value", null, R.value, w), (N = R.onVnodeBeforeMount) && ze(N, O, d);
    }
    process.env.NODE_ENV !== "production" && (Ts(T, "__vnode", d, !0), Ts(T, "__vueParentComponent", O, !0)), B && Ct(d, null, O, "beforeMount");
    const G = su(v, P);
    if (G && P.beforeEnter(T), s(T, g, _), (N = R && R.onVnodeMounted) || G || B) {
      const z = process.env.NODE_ENV !== "production" && De;
      ke(() => {
        let X;
        process.env.NODE_ENV !== "production" && (X = Ho(z));
        try {
          N && ze(N, O, d), G && P.enter(T), B && Ct(d, null, O, "mounted");
        } finally {
          process.env.NODE_ENV !== "production" && Ho(X);
        }
      }, v);
    }
  }, V = (d, g, _, O, v) => {
    if (_ && m(d, _), O)
      for (let w = 0; w < O.length; w++)
        m(d, O[w]);
    if (v) {
      let w = v.subTree;
      if (process.env.NODE_ENV !== "production" && w.patchFlag > 0 && w.patchFlag & 2048 && (w = mo(w.children) || w), g === w || Ul(w.type) && (w.ssContent === g || w.ssFallback === g)) {
        const A = v.vnode;
        V(
          d,
          A,
          A.scopeId,
          A.slotScopeIds,
          v.parent
        );
      }
    }
  }, U = (d, g, _, O, v, w, A, D, T = 0) => {
    for (let N = T; N < d.length; N++) {
      const R = d[N] = D ? dt(d[N]) : Re(d[N]);
      p(
        null,
        R,
        g,
        _,
        O,
        v,
        w,
        A,
        D
      );
    }
  }, ne = (d, g, _, O, v, w, A) => {
    const D = g.el = d.el;
    process.env.NODE_ENV !== "production" && (D.__vnode = g);
    let { patchFlag: T, dynamicChildren: N, dirs: R } = g;
    T |= d.patchFlag & 16;
    const $ = d.props || ee, P = g.props || ee;
    let B;
    if (_ && At(_, !1), (B = P.onVnodeBeforeUpdate) && ze(B, _, g, d), R && Ct(g, d, _, "beforeUpdate"), _ && At(_, !0), // HMR updated, force full diff
    (process.env.NODE_ENV !== "production" && De || // #6385 the old vnode may be a user-wrapped non-isomorphic block
    // Force full diff when block metadata is unstable.
    N && (!d.dynamicChildren || d.dynamicChildren.length !== N.length)) && (T = 0, A = !1, N = null), ($.innerHTML && P.innerHTML == null || $.textContent && P.textContent == null) && u(D, ""), N ? (he(
      d.dynamicChildren,
      N,
      D,
      _,
      O,
      wi(g, v),
      w
    ), process.env.NODE_ENV !== "production" && Es(d, g)) : A || Ye(
      d,
      g,
      D,
      null,
      _,
      O,
      wi(g, v),
      w,
      !1
    ), T > 0) {
      if (T & 16)
        ie(D, $, P, _, v);
      else if (T & 2 && $.class !== P.class && o(D, "class", null, P.class, v), T & 4 && o(D, "style", $.style, P.style, v), T & 8) {
        const G = g.dynamicProps;
        for (let z = 0; z < G.length; z++) {
          const X = G[z], ae = $[X], me = P[X];
          (me !== ae || X === "value") && o(D, X, ae, me, v, _);
        }
      }
      T & 1 && d.children !== g.children && u(D, g.children);
    } else !A && N == null && ie(D, $, P, _, v);
    ((B = P.onVnodeUpdated) || R) && ke(() => {
      B && ze(B, _, g, d), R && Ct(g, d, _, "updated");
    }, O);
  }, he = (d, g, _, O, v, w, A) => {
    for (let D = 0; D < g.length; D++) {
      const T = d[D], N = g[D], R = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        T.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (T.type === Ie || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !On(T, N) || // - In the case of a component, it could contain anything.
        T.shapeFlag & 198) ? f(T.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          _
        )
      );
      p(
        T,
        N,
        R,
        null,
        O,
        v,
        w,
        A,
        !0
      );
    }
  }, ie = (d, g, _, O, v) => {
    if (g !== _) {
      if (g !== ee)
        for (const w in g)
          !In(w) && !(w in _) && o(
            d,
            w,
            g[w],
            null,
            v,
            O
          );
      for (const w in _) {
        if (In(w)) continue;
        const A = _[w], D = g[w];
        A !== D && w !== "value" && o(d, w, D, A, v, O);
      }
      "value" in _ && o(d, "value", g.value, _.value, v);
    }
  }, ye = (d, g, _, O, v, w, A, D, T) => {
    const N = g.el = d ? d.el : l(""), R = g.anchor = d ? d.anchor : l("");
    let { patchFlag: $, dynamicChildren: P, slotScopeIds: B } = g;
    process.env.NODE_ENV !== "production" && // #5523 dev root fragment may inherit directives
    (De || $ & 2048) && ($ = 0, T = !1, P = null), B && (D = D ? D.concat(B) : B), d == null ? (s(N, _, O), s(R, _, O), U(
      // #10007
      // such fragment like `<></>` will be compiled into
      // a fragment which doesn't have a children.
      // In this case fallback to an empty array
      g.children || [],
      _,
      R,
      v,
      w,
      A,
      D,
      T
    )) : $ > 0 && $ & 64 && P && // #2715 the previous fragment could've been a BAILed one as a result
    // of renderSlot() with no valid children
    d.dynamicChildren && d.dynamicChildren.length === P.length ? (he(
      d.dynamicChildren,
      P,
      _,
      v,
      w,
      A,
      D
    ), process.env.NODE_ENV !== "production" ? Es(d, g) : (
      // #2080 if the stable fragment has a key, it's a <template v-for> that may
      //  get moved around. Make sure all root level vnodes inherit el.
      // #2134 or if it's a component root, it may also get moved around
      // as the component is being moved.
      (g.key != null || v && g === v.subTree) && Es(
        d,
        g,
        !0
        /* shallow */
      )
    )) : Ye(
      d,
      g,
      _,
      R,
      v,
      w,
      A,
      D,
      T
    );
  }, yn = (d, g, _, O, v, w, A, D, T) => {
    g.slotScopeIds = D, d == null ? g.shapeFlag & 512 ? v.ctx.activate(
      g,
      _,
      O,
      A,
      T
    ) : wt(
      g,
      _,
      O,
      v,
      w,
      A,
      T
    ) : Se(d, g, T);
  }, wt = (d, g, _, O, v, w, A) => {
    const D = d.component = pu(
      d,
      O,
      v
    );
    if (process.env.NODE_ENV !== "production" && D.type.__hmrId && Ka(D), process.env.NODE_ENV !== "production" && (ys(d), qt(D, "mount")), po(d) && (D.ctx.renderer = wn), process.env.NODE_ENV !== "production" && qt(D, "init"), mu(D, !1, A), process.env.NODE_ENV !== "production" && Ht(D, "init"), process.env.NODE_ENV !== "production" && De && (d.el = null), D.asyncDep) {
      if (v && v.registerDep(D, J, A), !d.el) {
        const T = D.subTree = mt(Ae);
        k(null, T, g, _), d.placeholder = T.el;
      }
    } else
      J(
        D,
        d,
        g,
        _,
        v,
        w,
        A
      );
    process.env.NODE_ENV !== "production" && (bs(), Ht(D, "mount"));
  }, Se = (d, g, _) => {
    const O = g.component = d.component;
    if (Rf(d, g, _))
      if (O.asyncDep && !O.asyncResolved) {
        process.env.NODE_ENV !== "production" && ys(g), W(O, g, _), process.env.NODE_ENV !== "production" && bs();
        return;
      } else
        O.next = g, O.update();
    else
      g.el = d.el, O.vnode = g;
  }, J = (d, g, _, O, v, w, A) => {
    const D = () => {
      if (d.isMounted) {
        let { next: $, bu: P, u: B, parent: G, vnode: z } = d;
        {
          const Qe = Bl(d);
          if (Qe) {
            $ && ($.el = z.el, W(d, $, A)), Qe.asyncDep.then(() => {
              ke(() => {
                d.isUnmounted || N();
              }, v);
            });
            return;
          }
        }
        let X = $, ae;
        process.env.NODE_ENV !== "production" && ys($ || d.vnode), At(d, !1), $ ? ($.el = z.el, W(d, $, A)) : $ = z, P && Yt(P), (ae = $.props && $.props.onVnodeBeforeUpdate) && ze(ae, G, $, z), At(d, !0), process.env.NODE_ENV !== "production" && qt(d, "render");
        const me = er(d);
        process.env.NODE_ENV !== "production" && Ht(d, "render");
        const Ge = d.subTree;
        d.subTree = me, process.env.NODE_ENV !== "production" && qt(d, "patch"), p(
          Ge,
          me,
          // parent may have changed if it's in a teleport
          f(Ge.el),
          // anchor may have changed if it's in a fragment
          ls(Ge),
          d,
          v,
          w
        ), process.env.NODE_ENV !== "production" && Ht(d, "patch"), $.el = me.el, X === null && Ff(d, me.el), B && ke(B, v), (ae = $.props && $.props.onVnodeUpdated) && ke(
          () => ze(ae, G, $, z),
          v
        ), process.env.NODE_ENV !== "production" && yl(d), process.env.NODE_ENV !== "production" && bs();
      } else {
        let $;
        const { el: P, props: B } = g, { bm: G, m: z, parent: X, root: ae, type: me } = d, Ge = Mn(g);
        At(d, !1), G && Yt(G), !Ge && ($ = B && B.onVnodeBeforeMount) && ze($, X, g), At(d, !0);
        {
          ae.ce && ae.ce._hasShadowRoot() && ae.ce._injectChildStyle(
            me,
            d.parent ? d.parent.type : void 0
          ), process.env.NODE_ENV !== "production" && qt(d, "render");
          const Qe = d.subTree = er(d);
          process.env.NODE_ENV !== "production" && Ht(d, "render"), process.env.NODE_ENV !== "production" && qt(d, "patch"), p(
            null,
            Qe,
            _,
            O,
            d,
            v,
            w
          ), process.env.NODE_ENV !== "production" && Ht(d, "patch"), g.el = Qe.el;
        }
        if (z && ke(z, v), !Ge && ($ = B && B.onVnodeMounted)) {
          const Qe = g;
          ke(
            () => ze($, X, Qe),
            v
          );
        }
        (g.shapeFlag & 256 || X && Mn(X.vnode) && X.vnode.shapeFlag & 256) && d.a && ke(d.a, v), d.isMounted = !0, process.env.NODE_ENV !== "production" && Ya(d), g = _ = O = null;
      }
    };
    d.scope.on();
    const T = d.effect = new Wr(D);
    d.scope.off();
    const N = d.update = T.run.bind(T), R = d.job = T.runIfDirty.bind(T);
    R.i = d, R.id = d.uid, T.scheduler = () => Gs(R), At(d, !0), process.env.NODE_ENV !== "production" && (T.onTrack = d.rtc ? ($) => Yt(d.rtc, $) : void 0, T.onTrigger = d.rtg ? ($) => Yt(d.rtg, $) : void 0), N();
  }, W = (d, g, _) => {
    g.component = d;
    const O = d.vnode.props;
    d.vnode = g, d.next = null, Uf(d, g.props, O, _), Zf(d, g.children, _), Le(), qo(d), Me();
  }, Ye = (d, g, _, O, v, w, A, D, T = !1) => {
    const N = d && d.children, R = d ? d.shapeFlag : 0, $ = g.children, { patchFlag: P, shapeFlag: B } = g;
    if (P > 0) {
      if (P & 128) {
        bn(
          N,
          $,
          _,
          O,
          v,
          w,
          A,
          D,
          T
        );
        return;
      } else if (P & 256) {
        ci(
          N,
          $,
          _,
          O,
          v,
          w,
          A,
          D,
          T
        );
        return;
      }
    }
    B & 8 ? (R & 16 && _n(N, v, w), $ !== N && u(_, $)) : R & 16 ? B & 16 ? bn(
      N,
      $,
      _,
      O,
      v,
      w,
      A,
      D,
      T
    ) : _n(N, v, w, !0) : (R & 8 && u(_, ""), B & 16 && U(
      $,
      _,
      O,
      v,
      w,
      A,
      D,
      T
    ));
  }, ci = (d, g, _, O, v, w, A, D, T) => {
    d = d || Zt, g = g || Zt;
    const N = d.length, R = g.length, $ = Math.min(N, R);
    let P;
    for (P = 0; P < $; P++) {
      const B = g[P] = T ? dt(g[P]) : Re(g[P]);
      p(
        d[P],
        B,
        _,
        null,
        v,
        w,
        A,
        D,
        T
      );
    }
    N > R ? _n(
      d,
      v,
      w,
      !0,
      !1,
      $
    ) : U(
      g,
      _,
      O,
      v,
      w,
      A,
      D,
      T,
      $
    );
  }, bn = (d, g, _, O, v, w, A, D, T) => {
    let N = 0;
    const R = g.length;
    let $ = d.length - 1, P = R - 1;
    for (; N <= $ && N <= P; ) {
      const B = d[N], G = g[N] = T ? dt(g[N]) : Re(g[N]);
      if (On(B, G))
        p(
          B,
          G,
          _,
          null,
          v,
          w,
          A,
          D,
          T
        );
      else
        break;
      N++;
    }
    for (; N <= $ && N <= P; ) {
      const B = d[$], G = g[P] = T ? dt(g[P]) : Re(g[P]);
      if (On(B, G))
        p(
          B,
          G,
          _,
          null,
          v,
          w,
          A,
          D,
          T
        );
      else
        break;
      $--, P--;
    }
    if (N > $) {
      if (N <= P) {
        const B = P + 1, G = B < R ? g[B].el : O;
        for (; N <= P; )
          p(
            null,
            g[N] = T ? dt(g[N]) : Re(g[N]),
            _,
            G,
            v,
            w,
            A,
            D,
            T
          ), N++;
      }
    } else if (N > P)
      for (; N <= $; )
        Et(d[N], v, w, !0), N++;
    else {
      const B = N, G = N, z = /* @__PURE__ */ new Map();
      for (N = G; N <= P; N++) {
        const be = g[N] = T ? dt(g[N]) : Re(g[N]);
        be.key != null && (process.env.NODE_ENV !== "production" && z.has(be.key) && C(
          "Duplicate keys found during update:",
          JSON.stringify(be.key),
          "Make sure keys are unique."
        ), z.set(be.key, N));
      }
      let X, ae = 0;
      const me = P - G + 1;
      let Ge = !1, Qe = 0;
      const En = new Array(me);
      for (N = 0; N < me; N++) En[N] = 0;
      for (N = B; N <= $; N++) {
        const be = d[N];
        if (ae >= me) {
          Et(be, v, w, !0);
          continue;
        }
        let Xe;
        if (be.key != null)
          Xe = z.get(be.key);
        else
          for (X = G; X <= P; X++)
            if (En[X - G] === 0 && On(be, g[X])) {
              Xe = X;
              break;
            }
        Xe === void 0 ? Et(be, v, w, !0) : (En[Xe - G] = N + 1, Xe >= Qe ? Qe = Xe : Ge = !0, p(
          be,
          g[Xe],
          _,
          null,
          v,
          w,
          A,
          D,
          T
        ), ae++);
      }
      const Mo = Ge ? iu(En) : Zt;
      for (X = Mo.length - 1, N = me - 1; N >= 0; N--) {
        const be = G + N, Xe = g[be], xo = g[be + 1], Po = be + 1 < R ? (
          // #13559, #14173 fallback to el placeholder for unresolved async component
          xo.el || Kl(xo)
        ) : O;
        En[N] === 0 ? p(
          null,
          Xe,
          _,
          Po,
          v,
          w,
          A,
          D,
          T
        ) : Ge && (X < 0 || N !== Mo[X] ? Kt(Xe, _, Po, 2) : X--);
      }
    }
  }, Kt = (d, g, _, O, v = null) => {
    const { el: w, type: A, transition: D, children: T, shapeFlag: N } = d;
    if (N & 6) {
      Kt(d.component.subTree, g, _, O);
      return;
    }
    if (N & 128) {
      d.suspense.move(g, _, O);
      return;
    }
    if (N & 64) {
      A.move(d, g, _, wn);
      return;
    }
    if (A === Ie) {
      s(w, g, _);
      for (let $ = 0; $ < T.length; $++)
        Kt(T[$], g, _, O);
      s(d.anchor, g, _);
      return;
    }
    if (A === vs) {
      L(d, g, _);
      return;
    }
    if (O !== 2 && N & 1 && D)
      if (O === 0)
        D.persisted && !w[bi] ? s(w, g, _) : (D.beforeEnter(w), s(w, g, _), ke(() => D.enter(w), v));
      else {
        const { leave: $, delayLeave: P, afterLeave: B } = D, G = () => {
          d.ctx.isUnmounted ? i(w) : s(w, g, _);
        }, z = () => {
          const X = w._isLeaving || !!w[bi];
          w._isLeaving && w[bi](
            !0
            /* cancelled */
          ), D.persisted && !X ? G() : $(w, () => {
            G(), B && B();
          });
        };
        P ? P(w, G, z) : z();
      }
    else
      s(w, g, _);
  }, Et = (d, g, _, O = !1, v = !1) => {
    const {
      type: w,
      props: A,
      ref: D,
      children: T,
      dynamicChildren: N,
      shapeFlag: R,
      patchFlag: $,
      dirs: P,
      cacheIndex: B,
      memo: G
    } = d;
    if ($ === -2 && (v = !1), D != null && (Le(), Ln(D, null, _, d, !0), Me()), B != null && (g.renderCache[B] = void 0), R & 256) {
      g.ctx.deactivate(d);
      return;
    }
    const z = R & 1 && P, X = !Mn(d);
    let ae;
    if (X && (ae = A && A.onVnodeBeforeUnmount) && ze(ae, g, d), R & 6)
      Kc(d.component, _, O);
    else {
      if (R & 128) {
        d.suspense.unmount(_, O);
        return;
      }
      z && Ct(d, null, g, "beforeUnmount"), R & 64 ? d.type.remove(
        d,
        g,
        _,
        wn,
        O
      ) : N && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !N.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (w !== Ie || $ > 0 && $ & 64) ? _n(
        N,
        g,
        _,
        !1,
        !0
      ) : (w === Ie && $ & 384 || !v && R & 16) && _n(T, g, _), O && ai(d);
    }
    const me = G != null && B == null;
    (X && (ae = A && A.onVnodeUnmounted) || z || me) && ke(() => {
      ae && ze(ae, g, d), z && Ct(d, null, g, "unmounted"), me && (d.el = null);
    }, _);
  }, ai = (d) => {
    const { type: g, el: _, anchor: O, transition: v } = d;
    if (g === Ie) {
      process.env.NODE_ENV !== "production" && d.patchFlag > 0 && d.patchFlag & 2048 && v && !v.persisted ? d.children.forEach((A) => {
        A.type === Ae ? i(A.el) : ai(A);
      }) : Bc(_, O);
      return;
    }
    if (g === vs) {
      S(d);
      return;
    }
    const w = () => {
      i(_), v && !v.persisted && v.afterLeave && v.afterLeave();
    };
    if (d.shapeFlag & 1 && v && !v.persisted) {
      const { leave: A, delayLeave: D } = v, T = () => A(_, w);
      D ? D(d.el, w, T) : T();
    } else
      w();
  }, Bc = (d, g) => {
    let _;
    for (; d !== g; )
      _ = h(d), i(d), d = _;
    i(g);
  }, Kc = (d, g, _) => {
    process.env.NODE_ENV !== "production" && d.type.__hmrId && Ua(d);
    const { bum: O, scope: v, job: w, subTree: A, um: D, m: T, a: N } = d;
    rr(T), rr(N), O && Yt(O), v.stop(), w && (w.flags |= 8, Et(A, d, g, _)), D && ke(D, g), ke(() => {
      d.isUnmounted = !0;
    }, g), process.env.NODE_ENV !== "production" && Qa(d);
  }, _n = (d, g, _, O = !1, v = !1, w = 0) => {
    for (let A = w; A < d.length; A++)
      Et(d[A], g, _, O, v);
  }, ls = (d) => {
    if (d.shapeFlag & 6)
      return ls(d.component.subTree);
    if (d.shapeFlag & 128)
      return d.suspense.next();
    const g = h(d.anchor || d.el), _ = g && g[rf];
    return _ ? h(_) : g;
  };
  let fi = !1;
  const Lo = (d, g, _) => {
    let O;
    d == null ? g._vnode && (Et(g._vnode, null, null, !0), O = g._vnode.component) : p(
      g._vnode || null,
      d,
      g,
      null,
      null,
      null,
      _
    ), g._vnode = d, fi || (fi = !0, qo(O), hl(), fi = !1);
  }, wn = {
    p,
    um: Et,
    m: Kt,
    r: ai,
    mt: wt,
    mc: U,
    pc: Ye,
    pbc: he,
    n: ls,
    o: e
  };
  return {
    render: Lo,
    hydrate: void 0,
    createApp: Vf(Lo)
  };
}
function wi({ type: e, props: t }, n) {
  return n === "svg" && e === "foreignObject" || n === "mathml" && e === "annotation-xml" && t && t.encoding && t.encoding.includes("html") ? void 0 : n;
}
function At({ effect: e, job: t }, n) {
  n ? (e.flags |= 32, t.flags |= 4) : (e.flags &= -33, t.flags &= -5);
}
function su(e, t) {
  return (!e || e && !e.pendingBranch) && t && !t.persisted;
}
function Es(e, t, n = !1) {
  const s = e.children, i = t.children;
  if (x(s) && x(i))
    for (let o = 0; o < s.length; o++) {
      const r = s[o];
      let l = i[o];
      l.shapeFlag & 1 && !l.dynamicChildren && ((l.patchFlag <= 0 || l.patchFlag === 32) && (l = i[o] = dt(i[o]), l.el = r.el), !n && l.patchFlag !== -2 && Es(r, l)), l.type === Zn && (l.patchFlag === -1 && (l = i[o] = dt(l)), l.el = r.el), l.type === Ae && !l.el && (l.el = r.el), process.env.NODE_ENV !== "production" && l.el && (l.el.__vnode = l);
    }
}
function iu(e) {
  const t = e.slice(), n = [0];
  let s, i, o, r, l;
  const c = e.length;
  for (s = 0; s < c; s++) {
    const a = e[s];
    if (a !== 0) {
      if (i = n[n.length - 1], e[i] < a) {
        t[s] = i, n.push(s);
        continue;
      }
      for (o = 0, r = n.length - 1; o < r; )
        l = o + r >> 1, e[n[l]] < a ? o = l + 1 : r = l;
      a < e[n[o]] && (o > 0 && (t[s] = n[o - 1]), n[o] = s);
    }
  }
  for (o = n.length, r = n[o - 1]; o-- > 0; )
    n[o] = r, r = t[r];
  return n;
}
function Bl(e) {
  const t = e.subTree.component;
  if (t)
    return t.asyncDep && !t.asyncResolved ? t : Bl(t);
}
function rr(e) {
  if (e)
    for (let t = 0; t < e.length; t++)
      e[t].flags |= 8;
}
function Kl(e) {
  if (e.placeholder)
    return e.placeholder;
  const t = e.component;
  return t ? Kl(t.subTree) : null;
}
const Ul = (e) => e.__isSuspense;
function ou(e, t) {
  t && t.pendingBranch ? x(e) ? t.effects.push(...e) : t.effects.push(e) : pl(e);
}
const Ie = /* @__PURE__ */ Symbol.for("v-fgt"), Zn = /* @__PURE__ */ Symbol.for("v-txt"), Ae = /* @__PURE__ */ Symbol.for("v-cmt"), vs = /* @__PURE__ */ Symbol.for("v-stc"), xn = [];
let Ce = null;
function Ze(e = !1) {
  xn.push(Ce = e ? null : []);
}
function ru() {
  xn.pop(), Ce = xn[xn.length - 1] || null;
}
let Un = 1;
function lr(e, t = !1) {
  Un += e, e < 0 && Ce && t && (Ce.hasOnce = !0);
}
function ql(e) {
  return e.dynamicChildren = Un > 0 ? Ce || Zt : null, ru(), Un > 0 && Ce && Ce.push(e), e;
}
function ct(e, t, n, s, i, o) {
  return ql(
    H(
      e,
      t,
      n,
      s,
      i,
      o,
      !0
    )
  );
}
function lu(e, t, n, s, i) {
  return ql(
    mt(
      e,
      t,
      n,
      s,
      i,
      !0
    )
  );
}
function zs(e) {
  return e ? e.__v_isVNode === !0 : !1;
}
function On(e, t) {
  if (process.env.NODE_ENV !== "production" && t.shapeFlag & 6 && e.component) {
    const n = _s.get(t.type);
    if (n && n.has(e.component))
      return e.shapeFlag &= -257, t.shapeFlag &= -513, !1;
  }
  return e.type === t.type && e.key === t.key;
}
const cu = (...e) => Wl(
  ...e
), Hl = ({ key: e }) => e ?? null, Ns = ({
  ref: e,
  ref_key: t,
  ref_for: n
}) => (typeof e == "number" && (e = "" + e), e != null ? se(e) || /* @__PURE__ */ fe(e) || F(e) ? { i: ve, r: e, k: t, f: !!n } : e : null);
function H(e, t = null, n = null, s = 0, i = null, o = e === Ie ? 0 : 1, r = !1, l = !1) {
  const c = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e,
    props: t,
    key: t && Hl(t),
    ref: t && Ns(t),
    scopeId: _l,
    slotScopeIds: null,
    children: n,
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
    shapeFlag: o,
    patchFlag: s,
    dynamicProps: i,
    dynamicChildren: null,
    appContext: null,
    ctx: ve
  };
  return l ? (Ps(c, n), o & 128 && e.normalize(c)) : n && (c.shapeFlag |= se(n) ? 8 : 16), process.env.NODE_ENV !== "production" && c.key !== c.key && C("VNode created with invalid key (NaN). VNode type:", c.type), Un > 0 && // avoid a block node from tracking itself
  !r && // has current parent block
  Ce && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (c.patchFlag > 0 || o & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  c.patchFlag !== 32 && Ce.push(c), c;
}
const mt = process.env.NODE_ENV !== "production" ? cu : Wl;
function Wl(e, t = null, n = null, s = 0, i = null, o = !1) {
  if ((!e || e === Ef) && (process.env.NODE_ENV !== "production" && !e && C(`Invalid vnode type when creating vnode: ${e}.`), e = Ae), zs(e)) {
    const l = Dt(
      e,
      t,
      !0
      /* mergeRef: true */
    );
    return n && Ps(l, n), Un > 0 && !o && Ce && (l.shapeFlag & 6 ? Ce[Ce.indexOf(e)] = l : Ce.push(l)), l.patchFlag = -2, l;
  }
  if (Zl(e) && (e = e.__vccOpts), t) {
    t = au(t);
    let { class: l, style: c } = t;
    l && !se(l) && (t.class = to(l)), Q(c) && (/* @__PURE__ */ Ds(c) && !x(c) && (c = le({}, c)), t.style = eo(c));
  }
  const r = se(e) ? 1 : Ul(e) ? 128 : lf(e) ? 64 : Q(e) ? 4 : F(e) ? 2 : 0;
  return process.env.NODE_ENV !== "production" && r & 4 && /* @__PURE__ */ Ds(e) && (e = /* @__PURE__ */ q(e), C(
    "Vue received a Component that was made a reactive object. This can lead to unnecessary performance overhead and should be avoided by marking the component with `markRaw` or using `shallowRef` instead of `ref`.",
    `
Component that was made reactive: `,
    e
  )), H(
    e,
    t,
    n,
    s,
    i,
    r,
    o,
    !0
  );
}
function au(e) {
  return e ? /* @__PURE__ */ Ds(e) || Ml(e) ? le({}, e) : e : null;
}
function Dt(e, t, n = !1, s = !1) {
  const { props: i, ref: o, patchFlag: r, children: l, transition: c } = e, a = t ? fu(i || {}, t) : i, u = {
    __v_isVNode: !0,
    __v_skip: !0,
    type: e.type,
    props: a,
    key: a && Hl(a),
    ref: t && t.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      n && o ? x(o) ? o.concat(Ns(t)) : [o, Ns(t)] : Ns(t)
    ) : o,
    scopeId: e.scopeId,
    slotScopeIds: e.slotScopeIds,
    children: process.env.NODE_ENV !== "production" && r === -1 && x(l) ? l.map(Jl) : l,
    target: e.target,
    targetStart: e.targetStart,
    targetAnchor: e.targetAnchor,
    staticCount: e.staticCount,
    shapeFlag: e.shapeFlag,
    // if the vnode is cloned with extra props, we can no longer assume its
    // existing patch flag to be reliable and need to add the FULL_PROPS flag.
    // note: preserve flag for fragments since they use the flag for children
    // fast paths only.
    patchFlag: t && e.type !== Ie ? r === -1 ? 16 : r | 16 : r,
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
    ssContent: e.ssContent && Dt(e.ssContent),
    ssFallback: e.ssFallback && Dt(e.ssFallback),
    placeholder: e.placeholder,
    el: e.el,
    anchor: e.anchor,
    ctx: e.ctx,
    ce: e.ce
  };
  return c && s && uo(
    u,
    c.clone(u)
  ), u;
}
function Jl(e) {
  const t = Dt(e);
  return x(e.children) && (t.children = e.children.map(Jl)), t;
}
function Yl(e = " ", t = 0) {
  return mt(Zn, null, e, t);
}
function us(e = "", t = !1) {
  return t ? (Ze(), lu(Ae, null, e)) : mt(Ae, null, e);
}
function Re(e) {
  return e == null || typeof e == "boolean" ? mt(Ae) : x(e) ? mt(
    Ie,
    null,
    // #3666, avoid reference pollution when reusing vnode
    e.slice()
  ) : zs(e) ? dt(e) : mt(Zn, null, String(e));
}
function dt(e) {
  return e.el === null && e.patchFlag !== -1 || e.memo ? e : Dt(e);
}
function Ps(e, t) {
  let n = 0;
  const { shapeFlag: s } = e;
  if (t == null)
    t = null;
  else if (x(t))
    n = 16;
  else if (typeof t == "object")
    if (s & 65) {
      const i = t.default;
      i && (i._c && (i._d = !1), Ps(e, i()), i._c && (i._d = !0));
      return;
    } else {
      n = 32;
      const i = t._;
      !i && !Ml(t) ? t._ctx = ve : i === 3 && ve && (ve.slots._ === 1 ? t._ = 1 : (t._ = 2, e.patchFlag |= 1024));
    }
  else if (F(t)) {
    if (s & 65) {
      Ps(e, { default: t });
      return;
    }
    t = { default: t, _ctx: ve }, n = 32;
  } else
    t = String(t), s & 64 ? (n = 16, t = [Yl(t)]) : n = 8;
  e.children = t, e.shapeFlag |= n;
}
function fu(...e) {
  const t = {};
  for (let n = 0; n < e.length; n++) {
    const s = e[n];
    for (const i in s)
      if (i === "class")
        t.class !== s.class && (t.class = to([t.class, s.class]));
      else if (i === "style")
        t.style = eo([t.style, s.style]);
      else if (Yn(i)) {
        const o = t[i], r = s[i];
        r && o !== r && !(x(o) && o.includes(r)) ? t[i] = o ? [].concat(o, r) : r : r == null && o == null && // mergeProps({ 'onUpdate:modelValue': undefined }) should not retain
        // the model listener.
        !Rn(i) && (t[i] = r);
      } else i !== "" && (t[i] = s[i]);
  }
  return t;
}
function ze(e, t, n, s = null) {
  We(e, t, 7, [
    n,
    s
  ]);
}
const uu = Cl();
let du = 0;
function pu(e, t, n) {
  const s = e.type, i = (t ? t.appContext : e.appContext) || uu, o = {
    uid: du++,
    vnode: e,
    type: s,
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
    scope: new ra(
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
    propsOptions: Pl(s, i),
    emitsOptions: Al(s, i),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: ee,
    // inheritAttrs
    inheritAttrs: s.inheritAttrs,
    // state
    ctx: ee,
    data: ee,
    props: ee,
    attrs: ee,
    slots: ee,
    refs: ee,
    setupState: ee,
    setupContext: null,
    // suspense related
    suspense: n,
    suspenseId: n ? n.pendingId : 0,
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
  return process.env.NODE_ENV !== "production" ? o.ctx = Nf(o) : o.ctx = { _: o }, o.root = t ? t.root : o, o.emit = Mf.bind(null, o), e.ce && e.ce(o), o;
}
let ue = null;
const Gl = () => ue || ve;
let js, Bi;
{
  const e = Qn(), t = (n, s) => {
    let i;
    return (i = e[n]) || (i = e[n] = []), i.push(s), (o) => {
      i.length > 1 ? i.forEach((r) => r(o)) : i[0](o);
    };
  };
  js = t(
    "__VUE_INSTANCE_SETTERS__",
    (n) => ue = n
  ), Bi = t(
    "__VUE_SSR_SETTERS__",
    (n) => qn = n
  );
}
const es = (e) => {
  const t = ue;
  return js(e), e.scope.on(), () => {
    e.scope.off(), js(t);
  };
}, cr = () => {
  ue && ue.scope.off(), js(null);
}, hu = /* @__PURE__ */ bt("slot,component");
function Ki(e, { isNativeTag: t }) {
  (hu(e) || t(e)) && C(
    "Do not use built-in or reserved HTML elements as component id: " + e
  );
}
function Ql(e) {
  return e.vnode.shapeFlag & 4;
}
let qn = !1;
function mu(e, t = !1, n = !1) {
  t && Bi(t);
  const { props: s, children: i } = e.vnode, o = Ql(e);
  Bf(e, s, o, t), zf(e, i, n || t);
  const r = o ? gu(e, t) : void 0;
  return t && Bi(!1), r;
}
function gu(e, t) {
  const n = e.type;
  if (process.env.NODE_ENV !== "production") {
    if (n.name && Ki(n.name, e.appContext.config), n.components) {
      const i = Object.keys(n.components);
      for (let o = 0; o < i.length; o++)
        Ki(i[o], e.appContext.config);
    }
    if (n.directives) {
      const i = Object.keys(n.directives);
      for (let o = 0; o < i.length; o++)
        wl(i[o]);
    }
    n.compilerOptions && yu() && C(
      '"compilerOptions" is only supported when using a build of Vue that includes the runtime compiler. Since you are using a runtime-only build, the options should be passed via your build tool config instead.'
    );
  }
  e.accessCache = /* @__PURE__ */ Object.create(null), e.proxy = new Proxy(e.ctx, kl), process.env.NODE_ENV !== "production" && Of(e);
  const { setup: s } = n;
  if (s) {
    Le();
    const i = e.setupContext = s.length > 1 ? _u(e) : null, o = es(e), r = un(
      s,
      e,
      0,
      [
        process.env.NODE_ENV !== "production" ? /* @__PURE__ */ it(e.props) : e.props,
        i
      ]
    ), l = Xi(r);
    if (Me(), o(), (l || e.sp) && !Mn(e) && Nl(e), l) {
      if (r.then(cr, cr), t)
        return r.then((c) => {
          ar(e, c, t);
        }).catch((c) => {
          Xn(c, e, 0);
        });
      if (e.asyncDep = r, process.env.NODE_ENV !== "production" && !e.suspense) {
        const c = ts(e, n);
        C(
          `Component <${c}>: setup function returned a promise, but no <Suspense> boundary was found in the parent component tree. A component with async setup() must be nested in a <Suspense> in order to be rendered.`
        );
      }
    } else
      ar(e, r, t);
  } else
    Xl(e, t);
}
function ar(e, t, n) {
  F(t) ? e.type.__ssrInlineRender ? e.ssrRender = t : e.render = t : Q(t) ? (process.env.NODE_ENV !== "production" && zs(t) && C(
    "setup() should not return VNodes directly - return a render function instead."
  ), process.env.NODE_ENV !== "production" && (e.devtoolsRawSetupState = t), e.setupState = cl(t), process.env.NODE_ENV !== "production" && Sf(e)) : process.env.NODE_ENV !== "production" && t !== void 0 && C(
    `setup() should return an object. Received: ${t === null ? "null" : typeof t}`
  ), Xl(e, n);
}
const yu = () => !0;
function Xl(e, t, n) {
  const s = e.type;
  e.render || (e.render = s.render || pe);
  {
    const i = es(e);
    Le();
    try {
      Tf(e);
    } finally {
      Me(), i();
    }
  }
  process.env.NODE_ENV !== "production" && !s.render && e.render === pe && !t && (s.template ? C(
    'Component provided template option but runtime compilation is not supported in this build of Vue. Configure your bundler to alias "vue" to "vue/dist/vue.esm-bundler.js".'
  ) : C("Component is missing template or render function: ", s));
}
const fr = process.env.NODE_ENV !== "production" ? {
  get(e, t) {
    return Ms(), de(e, "get", ""), e[t];
  },
  set() {
    return C("setupContext.attrs is readonly."), !1;
  },
  deleteProperty() {
    return C("setupContext.attrs is readonly."), !1;
  }
} : {
  get(e, t) {
    return de(e, "get", ""), e[t];
  }
};
function bu(e) {
  return new Proxy(e.slots, {
    get(t, n) {
      return de(e, "get", "$slots"), t[n];
    }
  });
}
function _u(e) {
  const t = (n) => {
    if (process.env.NODE_ENV !== "production" && (e.exposed && C("expose() should be called only once per setup()."), n != null)) {
      let s = typeof n;
      s === "object" && (x(n) ? s = "array" : /* @__PURE__ */ fe(n) && (s = "ref")), s !== "object" && C(
        `expose() should be passed a plain object, received ${s}.`
      );
    }
    e.exposed = n || {};
  };
  if (process.env.NODE_ENV !== "production") {
    let n, s;
    return Object.freeze({
      get attrs() {
        return n || (n = new Proxy(e.attrs, fr));
      },
      get slots() {
        return s || (s = bu(e));
      },
      get emit() {
        return (i, ...o) => e.emit(i, ...o);
      },
      expose: t
    });
  } else
    return {
      attrs: new Proxy(e.attrs, fr),
      slots: e.slots,
      emit: e.emit,
      expose: t
    };
}
function Zs(e) {
  return e.exposed ? e.exposeProxy || (e.exposeProxy = new Proxy(cl(ka(e.exposed)), {
    get(t, n) {
      if (n in t)
        return t[n];
      if (n in Rt)
        return Rt[n](e);
    },
    has(t, n) {
      return n in t || n in Rt;
    }
  })) : e.proxy;
}
const wu = /(?:^|[-_])\w/g, Eu = (e) => e.replace(wu, (t) => t.toUpperCase()).replace(/[-_]/g, "");
function zl(e, t = !0) {
  return F(e) ? e.displayName || e.name : e.name || t && e.__name;
}
function ts(e, t, n = !1) {
  let s = zl(t);
  if (!s && t.__file) {
    const i = t.__file.match(/([^/\\]+)\.\w+$/);
    i && (s = i[1]);
  }
  if (!s && e) {
    const i = (o) => {
      for (const r in o)
        if (o[r] === t)
          return r;
    };
    s = i(e.components) || e.parent && i(
      e.parent.type.components
    ) || i(e.appContext.components);
  }
  return s ? Eu(s) : n ? "App" : "Anonymous";
}
function Zl(e) {
  return F(e) && "__vccOpts" in e;
}
const vu = (e, t) => {
  const n = /* @__PURE__ */ $a(e, t, qn);
  if (process.env.NODE_ENV !== "production") {
    const s = Gl();
    s && s.appContext.config.warnRecursiveComputed && (n._warnRecursive = !0);
  }
  return n;
};
function Nu() {
  if (process.env.NODE_ENV === "production" || typeof window > "u")
    return;
  const e = { style: "color:#3ba776" }, t = { style: "color:#1677ff" }, n = { style: "color:#f5222d" }, s = { style: "color:#eb2f96" }, i = {
    __vue_custom_formatter: !0,
    header(f) {
      if (!Q(f))
        return null;
      if (f.__isVue)
        return ["div", e, "VueInstance"];
      if (/* @__PURE__ */ fe(f)) {
        Le();
        const h = f.value;
        return Me(), [
          "div",
          {},
          ["span", e, u(f)],
          "<",
          l(h),
          ">"
        ];
      } else {
        if (/* @__PURE__ */ St(f))
          return [
            "div",
            {},
            ["span", e, /* @__PURE__ */ Ne(f) ? "ShallowReactive" : "Reactive"],
            "<",
            l(f),
            `>${/* @__PURE__ */ qe(f) ? " (readonly)" : ""}`
          ];
        if (/* @__PURE__ */ qe(f))
          return [
            "div",
            {},
            ["span", e, /* @__PURE__ */ Ne(f) ? "ShallowReadonly" : "Readonly"],
            "<",
            l(f),
            ">"
          ];
      }
      return null;
    },
    hasBody(f) {
      return f && f.__isVue;
    },
    body(f) {
      if (f && f.__isVue)
        return [
          "div",
          {},
          ...o(f.$)
        ];
    }
  };
  function o(f) {
    const h = [];
    f.type.props && f.props && h.push(r("props", /* @__PURE__ */ q(f.props))), f.setupState !== ee && h.push(r("setup", f.setupState)), f.data !== ee && h.push(r("data", /* @__PURE__ */ q(f.data)));
    const m = c(f, "computed");
    m && h.push(r("computed", m));
    const b = c(f, "inject");
    return b && h.push(r("injected", b)), h.push([
      "div",
      {},
      [
        "span",
        {
          style: s.style + ";opacity:0.66"
        },
        "$ (internal): "
      ],
      ["object", { object: f }]
    ]), h;
  }
  function r(f, h) {
    return h = le({}, h), Object.keys(h).length ? [
      "div",
      { style: "line-height:1.25em;margin-bottom:0.6em" },
      [
        "div",
        {
          style: "color:#476582"
        },
        f
      ],
      [
        "div",
        {
          style: "padding-left:1.25em"
        },
        ...Object.keys(h).map((m) => [
          "div",
          {},
          ["span", s, m + ": "],
          l(h[m], !1)
        ])
      ]
    ] : ["span", {}];
  }
  function l(f, h = !0) {
    return typeof f == "number" ? ["span", t, f] : typeof f == "string" ? ["span", n, JSON.stringify(f)] : typeof f == "boolean" ? ["span", s, f] : Q(f) ? ["object", { object: h ? /* @__PURE__ */ q(f) : f }] : ["span", n, String(f)];
  }
  function c(f, h) {
    const m = f.type;
    if (F(m))
      return;
    const b = {};
    for (const p in f.ctx)
      a(m, p, h) && (b[p] = f.ctx[p]);
    return b;
  }
  function a(f, h, m) {
    const b = f[m];
    if (x(b) && b.includes(h) || Q(b) && h in b || f.extends && a(f.extends, h, m) || f.mixins && f.mixins.some((p) => a(p, h, m)))
      return !0;
  }
  function u(f) {
    return /* @__PURE__ */ Ne(f) ? "ShallowRef" : f.effect ? "ComputedRef" : "Ref";
  }
  window.devtoolsFormatters ? window.devtoolsFormatters.push(i) : window.devtoolsFormatters = [i];
}
const ur = "3.5.39", ot = process.env.NODE_ENV !== "production" ? C : pe;
process.env.NODE_ENV;
process.env.NODE_ENV;
/**
* @vue/runtime-dom v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
let Ui;
const dr = typeof window < "u" && window.trustedTypes;
if (dr)
  try {
    Ui = /* @__PURE__ */ dr.createPolicy("vue", {
      createHTML: (e) => e
    });
  } catch (e) {
    process.env.NODE_ENV !== "production" && ot(`Error creating trusted types policy: ${e}`);
  }
const ec = Ui ? (e) => Ui.createHTML(e) : (e) => e, Ou = "http://www.w3.org/2000/svg", Su = "http://www.w3.org/1998/Math/MathML", ft = typeof document < "u" ? document : null, pr = ft && /* @__PURE__ */ ft.createElement("template"), ku = {
  insert: (e, t, n) => {
    t.insertBefore(e, n || null);
  },
  remove: (e) => {
    const t = e.parentNode;
    t && t.removeChild(e);
  },
  createElement: (e, t, n, s) => {
    const i = t === "svg" ? ft.createElementNS(Ou, e) : t === "mathml" ? ft.createElementNS(Su, e) : n ? ft.createElement(e, { is: n }) : ft.createElement(e);
    return e === "select" && s && s.multiple != null && i.setAttribute("multiple", s.multiple), i;
  },
  createText: (e) => ft.createTextNode(e),
  createComment: (e) => ft.createComment(e),
  setText: (e, t) => {
    e.nodeValue = t;
  },
  setElementText: (e, t) => {
    e.textContent = t;
  },
  parentNode: (e) => e.parentNode,
  nextSibling: (e) => e.nextSibling,
  querySelector: (e) => ft.querySelector(e),
  setScopeId(e, t) {
    e.setAttribute(t, "");
  },
  // __UNSAFE__
  // Reason: innerHTML.
  // Static content here can only come from compiled templates.
  // As long as the user only uses trusted templates, this is safe.
  insertStaticContent(e, t, n, s, i, o) {
    const r = n ? n.previousSibling : t.lastChild;
    if (i && (i === o || i.nextSibling))
      for (; t.insertBefore(i.cloneNode(!0), n), !(i === o || !(i = i.nextSibling)); )
        ;
    else {
      pr.innerHTML = ec(
        s === "svg" ? `<svg>${e}</svg>` : s === "mathml" ? `<math>${e}</math>` : e
      );
      const l = pr.content;
      if (s === "svg" || s === "mathml") {
        const c = l.firstChild;
        for (; c.firstChild; )
          l.appendChild(c.firstChild);
        l.removeChild(c);
      }
      t.insertBefore(l, n);
    }
    return [
      // first
      r ? r.nextSibling : t.firstChild,
      // last
      n ? n.previousSibling : t.lastChild
    ];
  }
}, Tu = /* @__PURE__ */ Symbol("_vtc");
function Du(e, t, n) {
  const s = e[Tu];
  s && (t = (t ? [t, ...s] : [...s]).join(" ")), t == null ? e.removeAttribute("class") : n ? e.setAttribute("class", t) : e.className = t;
}
const hr = /* @__PURE__ */ Symbol("_vod"), Cu = /* @__PURE__ */ Symbol("_vsh"), Au = /* @__PURE__ */ Symbol(process.env.NODE_ENV !== "production" ? "CSS_VAR_TEXT" : ""), Iu = /(?:^|;)\s*display\s*:/;
function $u(e, t, n) {
  const s = e.style, i = se(n);
  let o = !1;
  if (n && !i) {
    if (t)
      if (se(t))
        for (const r of t.split(";")) {
          const l = r.slice(0, r.indexOf(":")).trim();
          n[l] == null && Dn(s, l, "");
        }
      else
        for (const r in t)
          n[r] == null && Dn(s, r, "");
    for (const r in n) {
      r === "display" && (o = !0);
      const l = n[r];
      l != null ? Mu(
        e,
        r,
        !se(t) && t ? t[r] : void 0,
        l
      ) || Dn(s, r, l) : Dn(s, r, "");
    }
  } else if (i) {
    if (t !== n) {
      const r = s[Au];
      r && (n += ";" + r), s.cssText = n, o = Iu.test(n);
    }
  } else t && e.removeAttribute("style");
  hr in e && (e[hr] = o ? s.display : "", e[Cu] && (s.display = "none"));
}
const Vu = /[^\\];\s*$/, mr = /\s*!important$/;
function Dn(e, t, n) {
  if (x(n))
    n.forEach((s) => Dn(e, t, s));
  else if (n == null && (n = ""), process.env.NODE_ENV !== "production" && Vu.test(n) && ot(
    `Unexpected semicolon at the end of '${t}' style value: '${n}'`
  ), t.startsWith("--"))
    e.setProperty(t, n);
  else {
    const s = Lu(e, t);
    mr.test(n) ? e.setProperty(
      Tt(s),
      n.replace(mr, ""),
      "important"
    ) : e[s] = n;
  }
}
const gr = ["Webkit", "Moz", "ms"], Ei = {};
function Lu(e, t) {
  const n = Ei[t];
  if (n)
    return n;
  let s = Te(t);
  if (s !== "filter" && s in e)
    return Ei[t] = s;
  s = Us(s);
  for (let i = 0; i < gr.length; i++) {
    const o = gr[i] + s;
    if (o in e)
      return Ei[t] = o;
  }
  return t;
}
function Mu(e, t, n, s) {
  return e.tagName === "TEXTAREA" && (t === "width" || t === "height") && se(s) && n === s;
}
const yr = "http://www.w3.org/1999/xlink";
function br(e, t, n, s, i, o = ia(t)) {
  s && t.startsWith("xlink:") ? n == null ? e.removeAttributeNS(yr, t.slice(6, t.length)) : e.setAttributeNS(yr, t, n) : n == null || o && !Kr(n) ? e.removeAttribute(t) : e.setAttribute(
    t,
    o ? "" : Ke(n) ? String(n) : n
  );
}
function _r(e, t, n, s, i) {
  if (t === "innerHTML" || t === "textContent") {
    n != null && (e[t] = t === "innerHTML" ? ec(n) : n);
    return;
  }
  const o = e.tagName;
  if (t === "value" && o !== "PROGRESS" && // custom elements may use _value internally
  !o.includes("-")) {
    const l = o === "OPTION" ? e.getAttribute("value") || "" : e.value, c = n == null ? (
      // #11647: value should be set as empty string for null and undefined,
      // but <input type="checkbox"> should be set as 'on'.
      e.type === "checkbox" ? "on" : ""
    ) : String(n);
    (l !== c || !("_value" in e)) && (e.value = c), n == null && e.removeAttribute(t), e._value = n;
    return;
  }
  let r = !1;
  if (n === "" || n == null) {
    const l = typeof e[t];
    l === "boolean" ? n = Kr(n) : n == null && l === "string" ? (n = "", r = !0) : l === "number" && (n = 0, r = !0);
  }
  try {
    e[t] = n;
  } catch (l) {
    process.env.NODE_ENV !== "production" && !r && ot(
      `Failed setting prop "${t}" on <${o.toLowerCase()}>: value ${n} is invalid.`,
      l
    );
  }
  r && e.removeAttribute(i || t);
}
function Ot(e, t, n, s) {
  e.addEventListener(t, n, s);
}
function xu(e, t, n, s) {
  e.removeEventListener(t, n, s);
}
const wr = /* @__PURE__ */ Symbol("_vei");
function Pu(e, t, n, s, i = null) {
  const o = e[wr] || (e[wr] = {}), r = o[t];
  if (s && r)
    r.value = process.env.NODE_ENV !== "production" ? Er(s, t) : s;
  else {
    const [l, c] = Fu(t);
    if (s) {
      const a = o[t] = Uu(
        process.env.NODE_ENV !== "production" ? Er(s, t) : s,
        i
      );
      Ot(e, l, a, c);
    } else r && (xu(e, l, r, c), o[t] = void 0);
  }
}
const ju = /(Once|Passive|Capture)$/, Ru = /^on:?(?:Once|Passive|Capture)$/;
function Fu(e) {
  let t, n;
  for (; (n = e.match(ju)) && !Ru.test(e); )
    t || (t = {}), e = e.slice(0, e.length - n[1].length), t[n[1].toLowerCase()] = !0;
  return [e[2] === ":" ? e.slice(3) : Tt(e.slice(2)), t];
}
let vi = 0;
const Bu = /* @__PURE__ */ Promise.resolve(), Ku = () => vi || (Bu.then(() => vi = 0), vi = Date.now());
function Uu(e, t) {
  const n = (s) => {
    if (!s._vts)
      s._vts = Date.now();
    else if (s._vts <= n.attached)
      return;
    const i = n.value;
    if (x(i)) {
      const o = s.stopImmediatePropagation;
      s.stopImmediatePropagation = () => {
        o.call(s), s._stopped = !0;
      };
      const r = i.slice(), l = [s];
      for (let c = 0; c < r.length && !s._stopped; c++) {
        const a = r[c];
        a && We(
          a,
          t,
          5,
          l
        );
      }
    } else
      We(
        i,
        t,
        5,
        [s]
      );
  };
  return n.value = e, n.attached = Ku(), n;
}
function Er(e, t) {
  return F(e) || x(e) ? e : (ot(
    `Wrong type passed as event handler to ${t} - did you forget @ or : in front of your prop?
Expected function or array of functions, received type ${typeof e}.`
  ), pe);
}
const vr = (e) => e.charCodeAt(0) === 111 && e.charCodeAt(1) === 110 && // lowercase letter
e.charCodeAt(2) > 96 && e.charCodeAt(2) < 123, qu = (e, t, n, s, i, o) => {
  const r = i === "svg";
  t === "class" ? Du(e, s, r) : t === "style" ? $u(e, n, s) : Yn(t) ? Rn(t) || Pu(e, t, n, s, o) : (t[0] === "." ? (t = t.slice(1), !0) : t[0] === "^" ? (t = t.slice(1), !1) : Hu(e, t, s, r)) ? (_r(e, t, s), !e.tagName.includes("-") && (t === "value" || t === "checked" || t === "selected") && br(e, t, s, r, o, t !== "value")) : /* #11081 force set props for possible async custom element */ e._isVueCE && // #12408 check if it's declared prop or it's async custom element
  (Wu(e, t) || // @ts-expect-error _def is private
  e._def.__asyncLoader && (/[A-Z]/.test(t) || !se(s))) ? _r(e, Te(t), s, o, t) : (t === "true-value" ? e._trueValue = s : t === "false-value" && (e._falseValue = s), br(e, t, s, r));
};
function Hu(e, t, n, s) {
  if (s)
    return !!(t === "innerHTML" || t === "textContent" || t in e && vr(t) && F(n));
  if (t === "spellcheck" || t === "draggable" || t === "translate" || t === "autocorrect" || t === "sandbox" && e.tagName === "IFRAME" || t === "form" || t === "list" && e.tagName === "INPUT" || t === "type" && e.tagName === "TEXTAREA")
    return !1;
  if (t === "width" || t === "height") {
    const i = e.tagName;
    if (i === "IMG" || i === "VIDEO" || i === "CANVAS" || i === "SOURCE")
      return !1;
  }
  return vr(t) && se(n) ? !1 : t in e;
}
function Wu(e, t) {
  const n = (
    // @ts-expect-error _def is private
    e._def.props
  );
  if (!n)
    return !1;
  const s = Te(t);
  return Array.isArray(n) ? n.some((i) => Te(i) === s) : Object.keys(n).some((i) => Te(i) === s);
}
const rn = (e) => {
  const t = e.props["onUpdate:modelValue"] || !1;
  return x(t) ? (n) => Yt(t, n) : t;
};
function Ju(e) {
  e.target.composing = !0;
}
function Nr(e) {
  const t = e.target;
  t.composing && (t.composing = !1, t.dispatchEvent(new Event("input")));
}
const gt = /* @__PURE__ */ Symbol("_assign");
function Or(e, t, n) {
  return t && (e = e.trim()), n && (e = qs(e)), e;
}
const at = {
  created(e, { modifiers: { lazy: t, trim: n, number: s } }, i) {
    e[gt] = rn(i);
    const o = s || i.props && i.props.type === "number";
    Ot(e, t ? "change" : "input", (r) => {
      r.target.composing || e[gt](Or(e.value, n, o));
    }), (n || o) && Ot(e, "change", () => {
      e.value = Or(e.value, n, o);
    }), t || (Ot(e, "compositionstart", Ju), Ot(e, "compositionend", Nr), Ot(e, "change", Nr));
  },
  // set value on mounted so it's after min/max for type="range"
  mounted(e, { value: t }) {
    e.value = t ?? "";
  },
  beforeUpdate(e, { value: t, oldValue: n, modifiers: { lazy: s, trim: i, number: o } }, r) {
    if (e[gt] = rn(r), e.composing) return;
    const l = (o || e.type === "number") && !/^0\d/.test(e.value) ? qs(e.value) : e.value, c = t ?? "";
    if (l === c)
      return;
    const a = e.getRootNode();
    (a instanceof Document || a instanceof ShadowRoot) && a.activeElement === e && e.type !== "range" && (s && t === n || i && e.value.trim() === c) || (e.value = c);
  }
}, Yu = {
  // #4096 array checkboxes need to be deep traversed
  deep: !0,
  created(e, t, n) {
    e[gt] = rn(n), Ot(e, "change", () => {
      const s = e._modelValue, i = Hn(e), o = e.checked, r = e[gt];
      if (x(s)) {
        const l = no(s, i), c = l !== -1;
        if (o && !c)
          r(s.concat(i));
        else if (!o && c) {
          const a = [...s];
          a.splice(l, 1), r(a);
        }
      } else if (an(s)) {
        const l = new Set(s);
        o ? l.add(i) : l.delete(i), r(l);
      } else
        r(tc(e, o));
    });
  },
  // set initial checked on mount to wait for true-value/false-value
  mounted: Sr,
  beforeUpdate(e, t, n) {
    e[gt] = rn(n), Sr(e, t, n);
  }
};
function Sr(e, { value: t, oldValue: n }, s) {
  e._modelValue = t;
  let i;
  if (x(t))
    i = no(t, s.props.value) > -1;
  else if (an(t))
    i = t.has(s.props.value);
  else {
    if (t === n) return;
    i = fn(t, tc(e, !0));
  }
  e.checked !== i && (e.checked = i);
}
const Gu = {
  // <select multiple> value need to be deep traversed
  deep: !0,
  created(e, { value: t, modifiers: { number: n } }, s) {
    const i = an(t);
    Ot(e, "change", () => {
      const o = Array.prototype.filter.call(e.options, (r) => r.selected).map(
        (r) => n ? qs(Hn(r)) : Hn(r)
      );
      e[gt](
        e.multiple ? i ? new Set(o) : o : o[0]
      ), e._assigning = !0, ul(() => {
        e._assigning = !1;
      });
    }), e[gt] = rn(s);
  },
  // set value in mounted & updated because <select> relies on its children
  // <option>s.
  mounted(e, { value: t }) {
    kr(e, t);
  },
  beforeUpdate(e, t, n) {
    e[gt] = rn(n);
  },
  updated(e, { value: t }) {
    e._assigning || kr(e, t);
  }
};
function kr(e, t) {
  const n = e.multiple, s = x(t);
  if (n && !s && !an(t)) {
    process.env.NODE_ENV !== "production" && ot(
      `<select multiple v-model> expects an Array or Set value for its binding, but got ${Object.prototype.toString.call(t).slice(8, -1)}.`
    );
    return;
  }
  for (let i = 0, o = e.options.length; i < o; i++) {
    const r = e.options[i], l = Hn(r);
    if (n)
      if (s) {
        const c = typeof l;
        c === "string" || c === "number" ? r.selected = t.some((a) => String(a) === String(l)) : r.selected = no(t, l) > -1;
      } else
        r.selected = t.has(l);
    else if (fn(Hn(r), t)) {
      e.selectedIndex !== i && (e.selectedIndex = i);
      return;
    }
  }
  !n && e.selectedIndex !== -1 && (e.selectedIndex = -1);
}
function Hn(e) {
  return "_value" in e ? e._value : e.value;
}
function tc(e, t) {
  const n = t ? "_trueValue" : "_falseValue";
  return n in e ? e[n] : t;
}
const Qu = /* @__PURE__ */ le({ patchProp: qu }, ku);
let Tr;
function Xu() {
  return Tr || (Tr = tu(Qu));
}
const zu = ((...e) => {
  const t = Xu().createApp(...e);
  process.env.NODE_ENV !== "production" && (ed(t), td(t));
  const { mount: n } = t;
  return t.mount = (s) => {
    const i = nd(s);
    if (!i) return;
    const o = t._component;
    !F(o) && !o.render && !o.template && (o.template = i.innerHTML), i.nodeType === 1 && (i.textContent = "");
    const r = n(i, !1, Zu(i));
    return i instanceof Element && (i.removeAttribute("v-cloak"), i.setAttribute("data-v-app", "")), r;
  }, t;
});
function Zu(e) {
  if (e instanceof SVGElement)
    return "svg";
  if (typeof MathMLElement == "function" && e instanceof MathMLElement)
    return "mathml";
}
function ed(e) {
  Object.defineProperty(e.config, "isNativeTag", {
    value: (t) => ea(t) || ta(t) || na(t),
    writable: !1
  });
}
function td(e) {
  {
    const t = e.config.isCustomElement;
    Object.defineProperty(e.config, "isCustomElement", {
      get() {
        return t;
      },
      set() {
        ot(
          "The `isCustomElement` config option is deprecated. Use `compilerOptions.isCustomElement` instead."
        );
      }
    });
    const n = e.config.compilerOptions, s = 'The `compilerOptions` config option is only respected when using a build of Vue.js that includes the runtime compiler (aka "full build"). Since you are using the runtime-only build, `compilerOptions` must be passed to `@vue/compiler-dom` in the build setup instead.\n- For vue-loader: pass it via vue-loader\'s `compilerOptions` loader option.\n- For vue-cli: see https://cli.vuejs.org/guide/webpack.html#modifying-options-of-a-loader\n- For vite: pass it via @vitejs/plugin-vue options. See https://github.com/vitejs/vite-plugin-vue/tree/main/packages/plugin-vue#example-for-passing-options-to-vuecompiler-sfc';
    Object.defineProperty(e.config, "compilerOptions", {
      get() {
        return ot(s), n;
      },
      set() {
        ot(s);
      }
    });
  }
}
function nd(e) {
  if (se(e)) {
    const t = document.querySelector(e);
    return process.env.NODE_ENV !== "production" && !t && ot(
      `Failed to mount app: mount target selector "${e}" returned null.`
    ), t;
  }
  return process.env.NODE_ENV !== "production" && window.ShadowRoot && e instanceof window.ShadowRoot && e.mode === "closed" && ot(
    'mounting on a ShadowRoot with `{mode: "closed"}` may lead to unpredictable bugs'
  ), e;
}
/**
* vue v3.5.39
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
function sd() {
  Nu();
}
process.env.NODE_ENV !== "production" && sd();
const bo = Symbol.for("yaml.alias"), qi = Symbol.for("yaml.document"), kt = Symbol.for("yaml.map"), nc = Symbol.for("yaml.pair"), rt = Symbol.for("yaml.scalar"), dn = Symbol.for("yaml.seq"), xe = Symbol.for("yaml.node.type"), pn = (e) => !!e && typeof e == "object" && e[xe] === bo, ns = (e) => !!e && typeof e == "object" && e[xe] === qi, ss = (e) => !!e && typeof e == "object" && e[xe] === kt, ce = (e) => !!e && typeof e == "object" && e[xe] === nc, te = (e) => !!e && typeof e == "object" && e[xe] === rt, is = (e) => !!e && typeof e == "object" && e[xe] === dn;
function oe(e) {
  if (e && typeof e == "object")
    switch (e[xe]) {
      case kt:
      case dn:
        return !0;
    }
  return !1;
}
function re(e) {
  if (e && typeof e == "object")
    switch (e[xe]) {
      case bo:
      case kt:
      case rt:
      case dn:
        return !0;
    }
  return !1;
}
const sc = (e) => (te(e) || oe(e)) && !!e.anchor, Vt = Symbol("break visit"), id = Symbol("skip children"), Pn = Symbol("remove node");
function hn(e, t) {
  const n = od(t);
  ns(e) ? Qt(null, e.contents, n, Object.freeze([e])) === Pn && (e.contents = null) : Qt(null, e, n, Object.freeze([]));
}
hn.BREAK = Vt;
hn.SKIP = id;
hn.REMOVE = Pn;
function Qt(e, t, n, s) {
  const i = rd(e, t, n, s);
  if (re(i) || ce(i))
    return ld(e, s, i), Qt(e, i, n, s);
  if (typeof i != "symbol") {
    if (oe(t)) {
      s = Object.freeze(s.concat(t));
      for (let o = 0; o < t.items.length; ++o) {
        const r = Qt(o, t.items[o], n, s);
        if (typeof r == "number")
          o = r - 1;
        else {
          if (r === Vt)
            return Vt;
          r === Pn && (t.items.splice(o, 1), o -= 1);
        }
      }
    } else if (ce(t)) {
      s = Object.freeze(s.concat(t));
      const o = Qt("key", t.key, n, s);
      if (o === Vt)
        return Vt;
      o === Pn && (t.key = null);
      const r = Qt("value", t.value, n, s);
      if (r === Vt)
        return Vt;
      r === Pn && (t.value = null);
    }
  }
  return i;
}
function od(e) {
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
function rd(e, t, n, s) {
  var i, o, r, l, c;
  if (typeof n == "function")
    return n(e, t, s);
  if (ss(t))
    return (i = n.Map) == null ? void 0 : i.call(n, e, t, s);
  if (is(t))
    return (o = n.Seq) == null ? void 0 : o.call(n, e, t, s);
  if (ce(t))
    return (r = n.Pair) == null ? void 0 : r.call(n, e, t, s);
  if (te(t))
    return (l = n.Scalar) == null ? void 0 : l.call(n, e, t, s);
  if (pn(t))
    return (c = n.Alias) == null ? void 0 : c.call(n, e, t, s);
}
function ld(e, t, n) {
  const s = t[t.length - 1];
  if (oe(s))
    s.items[e] = n;
  else if (ce(s))
    e === "key" ? s.key = n : s.value = n;
  else if (ns(s))
    s.contents = n;
  else {
    const i = pn(s) ? "alias" : "scalar";
    throw new Error(`Cannot replace node with ${i} parent`);
  }
}
const cd = {
  "!": "%21",
  ",": "%2C",
  "[": "%5B",
  "]": "%5D",
  "{": "%7B",
  "}": "%7D"
}, ad = (e) => e.replace(/[!,[\]{}]/g, (t) => cd[t]);
class we {
  constructor(t, n) {
    this.docStart = null, this.docEnd = !1, this.yaml = Object.assign({}, we.defaultYaml, t), this.tags = Object.assign({}, we.defaultTags, n);
  }
  clone() {
    const t = new we(this.yaml, this.tags);
    return t.docStart = this.docStart, t;
  }
  /**
   * During parsing, get a Directives instance for the current document and
   * update the stream state according to the current version's spec.
   */
  atDocument() {
    const t = new we(this.yaml, this.tags);
    switch (this.yaml.version) {
      case "1.1":
        this.atNextDocument = !0;
        break;
      case "1.2":
        this.atNextDocument = !1, this.yaml = {
          explicit: we.defaultYaml.explicit,
          version: "1.2"
        }, this.tags = Object.assign({}, we.defaultTags);
        break;
    }
    return t;
  }
  /**
   * @param onError - May be called even if the action was successful
   * @returns `true` on success
   */
  add(t, n) {
    this.atNextDocument && (this.yaml = { explicit: we.defaultYaml.explicit, version: "1.1" }, this.tags = Object.assign({}, we.defaultTags), this.atNextDocument = !1);
    const s = t.trim().split(/[ \t]+/), i = s.shift();
    switch (i) {
      case "%TAG": {
        if (s.length !== 2 && (n(0, "%TAG directive should contain exactly two parts"), s.length < 2))
          return !1;
        const [o, r] = s;
        return this.tags[o] = r, !0;
      }
      case "%YAML": {
        if (this.yaml.explicit = !0, s.length !== 1)
          return n(0, "%YAML directive should contain exactly one part"), !1;
        const [o] = s;
        if (o === "1.1" || o === "1.2")
          return this.yaml.version = o, !0;
        {
          const r = /^\d+\.\d+$/.test(o);
          return n(6, `Unsupported YAML version ${o}`, r), !1;
        }
      }
      default:
        return n(0, `Unknown directive ${i}`, !0), !1;
    }
  }
  /**
   * Resolves a tag, matching handles to those defined in %TAG directives.
   *
   * @returns Resolved tag, which may also be the non-specific tag `'!'` or a
   *   `'!local'` tag, or `null` if unresolvable.
   */
  tagName(t, n) {
    if (t === "!")
      return "!";
    if (t[0] !== "!")
      return n(`Not a valid tag: ${t}`), null;
    if (t[1] === "<") {
      const r = t.slice(2, -1);
      return r === "!" || r === "!!" ? (n(`Verbatim tags aren't resolved, so ${t} is invalid.`), null) : (t[t.length - 1] !== ">" && n("Verbatim tags must end with a >"), r);
    }
    const [, s, i] = t.match(/^(.*!)([^!]*)$/s);
    i || n(`The ${t} tag has no suffix`);
    const o = this.tags[s];
    if (o)
      try {
        return o + decodeURIComponent(i);
      } catch (r) {
        return n(String(r)), null;
      }
    return s === "!" ? t : (n(`Could not resolve tag: ${t}`), null);
  }
  /**
   * Given a fully resolved tag, returns its printable string form,
   * taking into account current tag prefixes and defaults.
   */
  tagString(t) {
    for (const [n, s] of Object.entries(this.tags))
      if (t.startsWith(s))
        return n + ad(t.substring(s.length));
    return t[0] === "!" ? t : `!<${t}>`;
  }
  toString(t) {
    const n = this.yaml.explicit ? [`%YAML ${this.yaml.version || "1.2"}`] : [], s = Object.entries(this.tags);
    let i;
    if (t && s.length > 0 && re(t.contents)) {
      const o = {};
      hn(t.contents, (r, l) => {
        re(l) && l.tag && (o[l.tag] = !0);
      }), i = Object.keys(o);
    } else
      i = [];
    for (const [o, r] of s)
      o === "!!" && r === "tag:yaml.org,2002:" || (!t || i.some((l) => l.startsWith(r))) && n.push(`%TAG ${o} ${r}`);
    return n.join(`
`);
  }
}
we.defaultYaml = { explicit: !1, version: "1.2" };
we.defaultTags = { "!!": "tag:yaml.org,2002:" };
function ic(e) {
  if (/[\x00-\x19\s,[\]{}]/.test(e)) {
    const n = `Anchor must not contain whitespace or control characters: ${JSON.stringify(e)}`;
    throw new Error(n);
  }
  return !0;
}
function oc(e) {
  const t = /* @__PURE__ */ new Set();
  return hn(e, {
    Value(n, s) {
      s.anchor && t.add(s.anchor);
    }
  }), t;
}
function rc(e, t) {
  for (let n = 1; ; ++n) {
    const s = `${e}${n}`;
    if (!t.has(s))
      return s;
  }
}
function fd(e, t) {
  const n = [], s = /* @__PURE__ */ new Map();
  let i = null;
  return {
    onAnchor: (o) => {
      n.push(o), i ?? (i = oc(e));
      const r = rc(t, i);
      return i.add(r), r;
    },
    /**
     * With circular references, the source node is only resolved after all
     * of its child nodes are. This is why anchors are set only after all of
     * the nodes have been created.
     */
    setAnchors: () => {
      for (const o of n) {
        const r = s.get(o);
        if (typeof r == "object" && r.anchor && (te(r.node) || oe(r.node)))
          r.node.anchor = r.anchor;
        else {
          const l = new Error("Failed to resolve repeated object (this should not happen)");
          throw l.source = o, l;
        }
      }
    },
    sourceObjects: s
  };
}
function Xt(e, t, n, s) {
  if (s && typeof s == "object")
    if (Array.isArray(s))
      for (let i = 0, o = s.length; i < o; ++i) {
        const r = s[i], l = Xt(e, s, String(i), r);
        l === void 0 ? delete s[i] : l !== r && (s[i] = l);
      }
    else if (s instanceof Map)
      for (const i of Array.from(s.keys())) {
        const o = s.get(i), r = Xt(e, s, i, o);
        r === void 0 ? s.delete(i) : r !== o && s.set(i, r);
      }
    else if (s instanceof Set)
      for (const i of Array.from(s)) {
        const o = Xt(e, s, i, i);
        o === void 0 ? s.delete(i) : o !== i && (s.delete(i), s.add(o));
      }
    else
      for (const [i, o] of Object.entries(s)) {
        const r = Xt(e, s, i, o);
        r === void 0 ? delete s[i] : r !== o && (s[i] = r);
      }
  return e.call(t, n, s);
}
function Ve(e, t, n) {
  if (Array.isArray(e))
    return e.map((s, i) => Ve(s, String(i), n));
  if (e && typeof e.toJSON == "function") {
    if (!n || !sc(e))
      return e.toJSON(t, n);
    const s = { aliasCount: 0, count: 1, res: void 0 };
    n.anchors.set(e, s), n.onCreate = (o) => {
      s.res = o, delete n.onCreate;
    };
    const i = e.toJSON(t, n);
    return n.onCreate && n.onCreate(i), i;
  }
  return typeof e == "bigint" && !(n != null && n.keep) ? Number(e) : e;
}
class _o {
  constructor(t) {
    Object.defineProperty(this, xe, { value: t });
  }
  /** Create a copy of this node.  */
  clone() {
    const t = Object.create(Object.getPrototypeOf(this), Object.getOwnPropertyDescriptors(this));
    return this.range && (t.range = this.range.slice()), t;
  }
  /** A plain JavaScript representation of this node. */
  toJS(t, { mapAsMap: n, maxAliasCount: s, onAnchor: i, reviver: o } = {}) {
    if (!ns(t))
      throw new TypeError("A document argument is required");
    const r = {
      anchors: /* @__PURE__ */ new Map(),
      doc: t,
      keep: !0,
      mapAsMap: n === !0,
      mapKeyWarned: !1,
      maxAliasCount: typeof s == "number" ? s : 100
    }, l = Ve(this, "", r);
    if (typeof i == "function")
      for (const { count: c, res: a } of r.anchors.values())
        i(a, c);
    return typeof o == "function" ? Xt(o, { "": l }, "", l) : l;
  }
}
class wo extends _o {
  constructor(t) {
    super(bo), this.source = t, Object.defineProperty(this, "tag", {
      set() {
        throw new Error("Alias nodes cannot have tags");
      }
    });
  }
  /**
   * Resolve the value of this alias within `doc`, finding the last
   * instance of the `source` anchor before this node.
   */
  resolve(t, n) {
    if ((n == null ? void 0 : n.maxAliasCount) === 0)
      throw new ReferenceError("Alias resolution is disabled");
    let s;
    n != null && n.aliasResolveCache ? s = n.aliasResolveCache : (s = [], hn(t, {
      Node: (o, r) => {
        (pn(r) || sc(r)) && s.push(r);
      }
    }), n && (n.aliasResolveCache = s));
    let i;
    for (const o of s) {
      if (o === this)
        break;
      o.anchor === this.source && (i = o);
    }
    return i;
  }
  toJSON(t, n) {
    if (!n)
      return { source: this.source };
    const { anchors: s, doc: i, maxAliasCount: o } = n, r = this.resolve(i, n);
    if (!r) {
      const c = `Unresolved alias (the anchor must be set before the alias): ${this.source}`;
      throw new ReferenceError(c);
    }
    let l = s.get(r);
    if (l || (Ve(r, null, n), l = s.get(r)), (l == null ? void 0 : l.res) === void 0) {
      const c = "This should not happen: Alias anchor was not resolved?";
      throw new ReferenceError(c);
    }
    if (o >= 0 && (l.count += 1, l.aliasCount === 0 && (l.aliasCount = Os(i, r, s)), l.count * l.aliasCount > o)) {
      const c = "Excessive alias count indicates a resource exhaustion attack";
      throw new ReferenceError(c);
    }
    return l.res;
  }
  toString(t, n, s) {
    const i = `*${this.source}`;
    if (t) {
      if (ic(this.source), t.options.verifyAliasOrder && !t.anchors.has(this.source)) {
        const o = `Unresolved alias (the anchor must be set before the alias): ${this.source}`;
        throw new Error(o);
      }
      if (t.implicitKey)
        return `${i} `;
    }
    return i;
  }
}
function Os(e, t, n) {
  if (pn(t)) {
    const s = t.resolve(e), i = n && s && n.get(s);
    return i ? i.count * i.aliasCount : 0;
  } else if (oe(t)) {
    let s = 0;
    for (const i of t.items) {
      const o = Os(e, i, n);
      o > s && (s = o);
    }
    return s;
  } else if (ce(t)) {
    const s = Os(e, t.key, n), i = Os(e, t.value, n);
    return Math.max(s, i);
  }
  return 1;
}
const lc = (e) => !e || typeof e != "function" && typeof e != "object";
class K extends _o {
  constructor(t) {
    super(rt), this.value = t;
  }
  toJSON(t, n) {
    return n != null && n.keep ? this.value : Ve(this.value, t, n);
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
const ud = "tag:yaml.org,2002:";
function dd(e, t, n) {
  if (t) {
    const s = n.filter((o) => o.tag === t), i = s.find((o) => !o.format) ?? s[0];
    if (!i)
      throw new Error(`Tag ${t} not found`);
    return i;
  }
  return n.find((s) => {
    var i;
    return ((i = s.identify) == null ? void 0 : i.call(s, e)) && !s.format;
  });
}
function Wn(e, t, n) {
  var f, h, m;
  if (ns(e) && (e = e.contents), re(e))
    return e;
  if (ce(e)) {
    const b = (h = (f = n.schema[kt]).createNode) == null ? void 0 : h.call(f, n.schema, null, n);
    return b.items.push(e), b;
  }
  (e instanceof String || e instanceof Number || e instanceof Boolean || typeof BigInt < "u" && e instanceof BigInt) && (e = e.valueOf());
  const { aliasDuplicateObjects: s, onAnchor: i, onTagObj: o, schema: r, sourceObjects: l } = n;
  let c;
  if (s && e && typeof e == "object") {
    if (c = l.get(e), c)
      return c.anchor ?? (c.anchor = i(e)), new wo(c.anchor);
    c = { anchor: null, node: null }, l.set(e, c);
  }
  t != null && t.startsWith("!!") && (t = ud + t.slice(2));
  let a = dd(e, t, r.tags);
  if (!a) {
    if (e && typeof e.toJSON == "function" && (e = e.toJSON()), !e || typeof e != "object") {
      const b = new K(e);
      return c && (c.node = b), b;
    }
    a = e instanceof Map ? r[kt] : Symbol.iterator in Object(e) ? r[dn] : r[kt];
  }
  o && (o(a), delete n.onTagObj);
  const u = a != null && a.createNode ? a.createNode(n.schema, e, n) : typeof ((m = a == null ? void 0 : a.nodeClass) == null ? void 0 : m.from) == "function" ? a.nodeClass.from(n.schema, e, n) : new K(e);
  return t ? u.tag = t : a.default || (u.tag = a.tag), c && (c.node = u), u;
}
function Rs(e, t, n) {
  let s = n;
  for (let i = t.length - 1; i >= 0; --i) {
    const o = t[i];
    if (typeof o == "number" && Number.isInteger(o) && o >= 0) {
      const r = [];
      r[o] = s, s = r;
    } else
      s = /* @__PURE__ */ new Map([[o, s]]);
  }
  return Wn(s, void 0, {
    aliasDuplicateObjects: !1,
    keepUndefined: !1,
    onAnchor: () => {
      throw new Error("This should not happen, please report a bug.");
    },
    schema: e,
    sourceObjects: /* @__PURE__ */ new Map()
  });
}
const Cn = (e) => e == null || typeof e == "object" && !!e[Symbol.iterator]().next().done;
class cc extends _o {
  constructor(t, n) {
    super(t), Object.defineProperty(this, "schema", {
      value: n,
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
    const n = Object.create(Object.getPrototypeOf(this), Object.getOwnPropertyDescriptors(this));
    return t && (n.schema = t), n.items = n.items.map((s) => re(s) || ce(s) ? s.clone(t) : s), this.range && (n.range = this.range.slice()), n;
  }
  /**
   * Adds a value to the collection. For `!!map` and `!!omap` the value must
   * be a Pair instance or a `{ key, value }` object, which may not have a key
   * that already exists in the map.
   */
  addIn(t, n) {
    if (Cn(t))
      this.add(n);
    else {
      const [s, ...i] = t, o = this.get(s, !0);
      if (oe(o))
        o.addIn(i, n);
      else if (o === void 0 && this.schema)
        this.set(s, Rs(this.schema, i, n));
      else
        throw new Error(`Expected YAML collection at ${s}. Remaining path: ${i}`);
    }
  }
  /**
   * Removes a value from the collection.
   * @returns `true` if the item was found and removed.
   */
  deleteIn(t) {
    const [n, ...s] = t;
    if (s.length === 0)
      return this.delete(n);
    const i = this.get(n, !0);
    if (oe(i))
      return i.deleteIn(s);
    throw new Error(`Expected YAML collection at ${n}. Remaining path: ${s}`);
  }
  /**
   * Returns item at `key`, or `undefined` if not found. By default unwraps
   * scalar values from their surrounding node; to disable set `keepScalar` to
   * `true` (collections are always returned intact).
   */
  getIn(t, n) {
    const [s, ...i] = t, o = this.get(s, !0);
    return i.length === 0 ? !n && te(o) ? o.value : o : oe(o) ? o.getIn(i, n) : void 0;
  }
  hasAllNullValues(t) {
    return this.items.every((n) => {
      if (!ce(n))
        return !1;
      const s = n.value;
      return s == null || t && te(s) && s.value == null && !s.commentBefore && !s.comment && !s.tag;
    });
  }
  /**
   * Checks if the collection includes a value with the key `key`.
   */
  hasIn(t) {
    const [n, ...s] = t;
    if (s.length === 0)
      return this.has(n);
    const i = this.get(n, !0);
    return oe(i) ? i.hasIn(s) : !1;
  }
  /**
   * Sets a value in this collection. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   */
  setIn(t, n) {
    const [s, ...i] = t;
    if (i.length === 0)
      this.set(s, n);
    else {
      const o = this.get(s, !0);
      if (oe(o))
        o.setIn(i, n);
      else if (o === void 0 && this.schema)
        this.set(s, Rs(this.schema, i, n));
      else
        throw new Error(`Expected YAML collection at ${s}. Remaining path: ${i}`);
    }
  }
}
const pd = (e) => e.replace(/^(?!$)(?: $)?/gm, "#");
function ht(e, t) {
  return /^\n+$/.test(e) ? e.substring(1) : t ? e.replace(/^(?! *$)/gm, t) : e;
}
const Lt = (e, t, n) => e.endsWith(`
`) ? ht(n, t) : n.includes(`
`) ? `
` + ht(n, t) : (e.endsWith(" ") ? "" : " ") + n, ac = "flow", Hi = "block", Ss = "quoted";
function ei(e, t, n = "flow", { indentAtStart: s, lineWidth: i = 80, minContentWidth: o = 20, onFold: r, onOverflow: l } = {}) {
  if (!i || i < 0)
    return e;
  i < o && (o = 0);
  const c = Math.max(1 + o, 1 + i - t.length);
  if (e.length <= c)
    return e;
  const a = [], u = {};
  let f = i - t.length;
  typeof s == "number" && (s > i - Math.max(2, o) ? a.push(0) : f = i - s);
  let h, m, b = !1, p = -1, y = -1, k = -1;
  n === Hi && (p = Dr(e, p, t.length), p !== -1 && (f = p + c));
  for (let I; I = e[p += 1]; ) {
    if (n === Ss && I === "\\") {
      switch (y = p, e[p + 1]) {
        case "x":
          p += 3;
          break;
        case "u":
          p += 5;
          break;
        case "U":
          p += 9;
          break;
        default:
          p += 1;
      }
      k = p;
    }
    if (I === `
`)
      n === Hi && (p = Dr(e, p, t.length)), f = p + t.length + c, h = void 0;
    else {
      if (I === " " && m && m !== " " && m !== `
` && m !== "	") {
        const L = e[p + 1];
        L && L !== " " && L !== `
` && L !== "	" && (h = p);
      }
      if (p >= f)
        if (h)
          a.push(h), f = h + c, h = void 0;
        else if (n === Ss) {
          for (; m === " " || m === "	"; )
            m = I, I = e[p += 1], b = !0;
          const L = p > k + 1 ? p - 2 : y - 1;
          if (u[L])
            return e;
          a.push(L), u[L] = !0, f = L + c, h = void 0;
        } else
          b = !0;
    }
    m = I;
  }
  if (b && l && l(), a.length === 0)
    return e;
  r && r();
  let E = e.slice(0, a[0]);
  for (let I = 0; I < a.length; ++I) {
    const L = a[I], S = a[I + 1] || e.length;
    L === 0 ? E = `
${t}${e.slice(0, S)}` : (n === Ss && u[L] && (E += `${e[L]}\\`), E += `
${t}${e.slice(L + 1, S)}`);
  }
  return E;
}
function Dr(e, t, n) {
  let s = t, i = t + 1, o = e[i];
  for (; o === " " || o === "	"; )
    if (t < i + n)
      o = e[++t];
    else {
      do
        o = e[++t];
      while (o && o !== `
`);
      s = t, i = t + 1, o = e[i];
    }
  return s;
}
const ti = (e, t) => ({
  indentAtStart: t ? e.indent.length : e.indentAtStart,
  lineWidth: e.options.lineWidth,
  minContentWidth: e.options.minContentWidth
}), ni = (e) => /^(%|---|\.\.\.)/m.test(e);
function hd(e, t, n) {
  if (!t || t < 0)
    return !1;
  const s = t - n, i = e.length;
  if (i <= s)
    return !1;
  for (let o = 0, r = 0; o < i; ++o)
    if (e[o] === `
`) {
      if (o - r > s)
        return !0;
      if (r = o + 1, i - r <= s)
        return !1;
    }
  return !0;
}
function jn(e, t) {
  const n = JSON.stringify(e);
  if (t.options.doubleQuotedAsJSON)
    return n;
  const { implicitKey: s } = t, i = t.options.doubleQuotedMinMultiLineLength, o = t.indent || (ni(e) ? "  " : "");
  let r = "", l = 0;
  for (let c = 0, a = n[c]; a; a = n[++c])
    if (a === " " && n[c + 1] === "\\" && n[c + 2] === "n" && (r += n.slice(l, c) + "\\ ", c += 1, l = c, a = "\\"), a === "\\")
      switch (n[c + 1]) {
        case "u":
          {
            r += n.slice(l, c);
            const u = n.substr(c + 2, 4);
            switch (u) {
              case "0000":
                r += "\\0";
                break;
              case "0007":
                r += "\\a";
                break;
              case "000b":
                r += "\\v";
                break;
              case "001b":
                r += "\\e";
                break;
              case "0085":
                r += "\\N";
                break;
              case "00a0":
                r += "\\_";
                break;
              case "2028":
                r += "\\L";
                break;
              case "2029":
                r += "\\P";
                break;
              default:
                u.substr(0, 2) === "00" ? r += "\\x" + u.substr(2) : r += n.substr(c, 6);
            }
            c += 5, l = c + 1;
          }
          break;
        case "n":
          if (s || n[c + 2] === '"' || n.length < i)
            c += 1;
          else {
            for (r += n.slice(l, c) + `

`; n[c + 2] === "\\" && n[c + 3] === "n" && n[c + 4] !== '"'; )
              r += `
`, c += 2;
            r += o, n[c + 2] === " " && (r += "\\"), c += 1, l = c + 1;
          }
          break;
        default:
          c += 1;
      }
  return r = l ? r + n.slice(l) : n, s ? r : ei(r, o, Ss, ti(t, !1));
}
function Wi(e, t) {
  if (t.options.singleQuote === !1 || t.implicitKey && e.includes(`
`) || /[ \t]\n|\n[ \t]/.test(e))
    return jn(e, t);
  const n = t.indent || (ni(e) ? "  " : ""), s = "'" + e.replace(/'/g, "''").replace(/\n+/g, `$&
${n}`) + "'";
  return t.implicitKey ? s : ei(s, n, ac, ti(t, !1));
}
function zt(e, t) {
  const { singleQuote: n } = t.options;
  let s;
  if (n === !1)
    s = jn;
  else {
    const i = e.includes('"'), o = e.includes("'");
    i && !o ? s = Wi : o && !i ? s = jn : s = n ? Wi : jn;
  }
  return s(e, t);
}
let Ji;
try {
  Ji = new RegExp(`(^|(?<!
))
+(?!
|$)`, "g");
} catch {
  Ji = /\n+(?!\n|$)/g;
}
function ks({ comment: e, type: t, value: n }, s, i, o) {
  const { blockQuote: r, commentString: l, lineWidth: c } = s.options;
  if (!r || /\n[\t ]+$/.test(n))
    return zt(n, s);
  const a = s.indent || (s.forceBlockIndent || ni(n) ? "  " : ""), u = r === "literal" ? !0 : r === "folded" || t === K.BLOCK_FOLDED ? !1 : t === K.BLOCK_LITERAL ? !0 : !hd(n, c, a.length);
  if (!n)
    return u ? `|
` : `>
`;
  let f, h;
  for (h = n.length; h > 0; --h) {
    const S = n[h - 1];
    if (S !== `
` && S !== "	" && S !== " ")
      break;
  }
  let m = n.substring(h);
  const b = m.indexOf(`
`);
  b === -1 ? f = "-" : n === m || b !== m.length - 1 ? (f = "+", o && o()) : f = "", m && (n = n.slice(0, -m.length), m[m.length - 1] === `
` && (m = m.slice(0, -1)), m = m.replace(Ji, `$&${a}`));
  let p = !1, y, k = -1;
  for (y = 0; y < n.length; ++y) {
    const S = n[y];
    if (S === " ")
      p = !0;
    else if (S === `
`)
      k = y;
    else
      break;
  }
  let E = n.substring(0, k < y ? k + 1 : y);
  E && (n = n.substring(E.length), E = E.replace(/\n+/g, `$&${a}`));
  let L = (p ? a ? "2" : "1" : "") + f;
  if (e && (L += " " + l(e.replace(/ ?[\r\n]+/g, " ")), i && i()), !u) {
    const S = n.replace(/\n+/g, `
$&`).replace(/(?:^|\n)([\t ].*)(?:([\n\t ]*)\n(?![\n\t ]))?/g, "$1$2").replace(/\n+/g, `$&${a}`);
    let j = !1;
    const M = ti(s, !0);
    r !== "folded" && t !== K.BLOCK_FOLDED && (M.onOverflow = () => {
      j = !0;
    });
    const V = ei(`${E}${S}${m}`, a, Hi, M);
    if (!j)
      return `>${L}
${a}${V}`;
  }
  return n = n.replace(/\n+/g, `$&${a}`), `|${L}
${a}${E}${n}${m}`;
}
function md(e, t, n, s) {
  const { type: i, value: o } = e, { actualString: r, implicitKey: l, indent: c, indentStep: a, inFlow: u } = t;
  if (l && o.includes(`
`) || u && /[[\]{},]/.test(o))
    return zt(o, t);
  if (/^[\n\t ,[\]{}#&*!|>'"%@`]|^[?-]$|^[?-][ \t]|[\n:][ \t]|[ \t]\n|[\n\t ]#|[\n\t :]$/.test(o))
    return l || u || !o.includes(`
`) ? zt(o, t) : ks(e, t, n, s);
  if (!l && !u && i !== K.PLAIN && o.includes(`
`))
    return ks(e, t, n, s);
  if (ni(o)) {
    if (c === "")
      return t.forceBlockIndent = !0, ks(e, t, n, s);
    if (l && c === a)
      return zt(o, t);
  }
  const f = o.replace(/\n+/g, `$&
${c}`);
  if (r) {
    const h = (p) => {
      var y;
      return p.default && p.tag !== "tag:yaml.org,2002:str" && ((y = p.test) == null ? void 0 : y.test(f));
    }, { compat: m, tags: b } = t.doc.schema;
    if (b.some(h) || m != null && m.some(h))
      return zt(o, t);
  }
  return l ? f : ei(f, c, ac, ti(t, !1));
}
function Eo(e, t, n, s) {
  const { implicitKey: i, inFlow: o } = t, r = typeof e.value == "string" ? e : Object.assign({}, e, { value: String(e.value) });
  let { type: l } = e;
  l !== K.QUOTE_DOUBLE && /[\x00-\x08\x0b-\x1f\x7f-\x9f\u{D800}-\u{DFFF}]/u.test(r.value) && (l = K.QUOTE_DOUBLE);
  const c = (u) => {
    switch (u) {
      case K.BLOCK_FOLDED:
      case K.BLOCK_LITERAL:
        return i || o ? zt(r.value, t) : ks(r, t, n, s);
      case K.QUOTE_DOUBLE:
        return jn(r.value, t);
      case K.QUOTE_SINGLE:
        return Wi(r.value, t);
      case K.PLAIN:
        return md(r, t, n, s);
      default:
        return null;
    }
  };
  let a = c(l);
  if (a === null) {
    const { defaultKeyType: u, defaultStringType: f } = t.options, h = i && u || f;
    if (a = c(h), a === null)
      throw new Error(`Unsupported default string type ${h}`);
  }
  return a;
}
function fc(e, t) {
  const n = Object.assign({
    blockQuote: !0,
    commentString: pd,
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
  let s;
  switch (n.collectionStyle) {
    case "block":
      s = !1;
      break;
    case "flow":
      s = !0;
      break;
    default:
      s = null;
  }
  return {
    anchors: /* @__PURE__ */ new Set(),
    doc: e,
    flowCollectionPadding: n.flowCollectionPadding ? " " : "",
    indent: "",
    indentStep: typeof n.indent == "number" ? " ".repeat(n.indent) : "  ",
    inFlow: s,
    options: n
  };
}
function gd(e, t) {
  var i;
  if (t.tag) {
    const o = e.filter((r) => r.tag === t.tag);
    if (o.length > 0)
      return o.find((r) => r.format === t.format) ?? o[0];
  }
  let n, s;
  if (te(t)) {
    s = t.value;
    let o = e.filter((r) => {
      var l;
      return (l = r.identify) == null ? void 0 : l.call(r, s);
    });
    if (o.length > 1) {
      const r = o.filter((l) => l.test);
      r.length > 0 && (o = r);
    }
    n = o.find((r) => r.format === t.format) ?? o.find((r) => !r.format);
  } else
    s = t, n = e.find((o) => o.nodeClass && s instanceof o.nodeClass);
  if (!n) {
    const o = ((i = s == null ? void 0 : s.constructor) == null ? void 0 : i.name) ?? (s === null ? "null" : typeof s);
    throw new Error(`Tag not resolved for ${o} value`);
  }
  return n;
}
function yd(e, t, { anchors: n, doc: s }) {
  if (!s.directives)
    return "";
  const i = [], o = (te(e) || oe(e)) && e.anchor;
  o && ic(o) && (n.add(o), i.push(`&${o}`));
  const r = e.tag ?? (t.default ? null : t.tag);
  return r && i.push(s.directives.tagString(r)), i.join(" ");
}
function ln(e, t, n, s) {
  var c;
  if (ce(e))
    return e.toString(t, n, s);
  if (pn(e)) {
    if (t.doc.directives)
      return e.toString(t);
    if ((c = t.resolvedAliases) != null && c.has(e))
      throw new TypeError("Cannot stringify circular structure without alias nodes");
    t.resolvedAliases ? t.resolvedAliases.add(e) : t.resolvedAliases = /* @__PURE__ */ new Set([e]), e = e.resolve(t.doc);
  }
  let i;
  const o = re(e) ? e : t.doc.createNode(e, { onTagObj: (a) => i = a });
  i ?? (i = gd(t.doc.schema.tags, o));
  const r = yd(o, i, t);
  r.length > 0 && (t.indentAtStart = (t.indentAtStart ?? 0) + r.length + 1);
  const l = typeof i.stringify == "function" ? i.stringify(o, t, n, s) : te(o) ? Eo(o, t, n, s) : o.toString(t, n, s);
  return r ? te(o) || l[0] === "{" || l[0] === "[" ? `${r} ${l}` : `${r}
${t.indent}${l}` : l;
}
function bd({ key: e, value: t }, n, s, i) {
  const { allNullValues: o, doc: r, indent: l, indentStep: c, options: { commentString: a, indentSeq: u, simpleKeys: f } } = n;
  let h = re(e) && e.comment || null;
  if (f) {
    if (h)
      throw new Error("With simple keys, key nodes cannot have comments");
    if (oe(e) || !re(e) && typeof e == "object") {
      const M = "With simple keys, collection cannot be used as a key value";
      throw new Error(M);
    }
  }
  let m = !f && (!e || h && t == null && !n.inFlow || oe(e) || (te(e) ? e.type === K.BLOCK_FOLDED || e.type === K.BLOCK_LITERAL : typeof e == "object"));
  n = Object.assign({}, n, {
    allNullValues: !1,
    implicitKey: !m && (f || !o),
    indent: l + c
  });
  let b = !1, p = !1, y = ln(e, n, () => b = !0, () => p = !0);
  if (!m && !n.inFlow && y.length > 1024) {
    if (f)
      throw new Error("With simple keys, single line scalar must not span more than 1024 characters");
    m = !0;
  }
  if (n.inFlow) {
    if (o || t == null)
      return b && s && s(), y === "" ? "?" : m ? `? ${y}` : y;
  } else if (o && !f || t == null && m)
    return y = `? ${y}`, h && !b ? y += Lt(y, n.indent, a(h)) : p && i && i(), y;
  b && (h = null), m ? (h && (y += Lt(y, n.indent, a(h))), y = `? ${y}
${l}:`) : (y = `${y}:`, h && (y += Lt(y, n.indent, a(h))));
  let k, E, I;
  re(t) ? (k = !!t.spaceBefore, E = t.commentBefore, I = t.comment) : (k = !1, E = null, I = null, t && typeof t == "object" && (t = r.createNode(t))), n.implicitKey = !1, !m && !h && te(t) && (n.indentAtStart = y.length + 1), p = !1, !u && c.length >= 2 && !n.inFlow && !m && is(t) && !t.flow && !t.tag && !t.anchor && (n.indent = n.indent.substring(2));
  let L = !1;
  const S = ln(t, n, () => L = !0, () => p = !0);
  let j = " ";
  if (h || k || E) {
    if (j = k ? `
` : "", E) {
      const M = a(E);
      j += `
${ht(M, n.indent)}`;
    }
    S === "" && !n.inFlow ? j === `
` && I && (j = `

`) : j += `
${n.indent}`;
  } else if (!m && oe(t)) {
    const M = S[0], V = S.indexOf(`
`), U = V !== -1, ne = n.inFlow ?? t.flow ?? t.items.length === 0;
    if (U || !ne) {
      let he = !1;
      if (U && (M === "&" || M === "!")) {
        let ie = S.indexOf(" ");
        M === "&" && ie !== -1 && ie < V && S[ie + 1] === "!" && (ie = S.indexOf(" ", ie + 1)), (ie === -1 || V < ie) && (he = !0);
      }
      he || (j = `
${n.indent}`);
    }
  } else (S === "" || S[0] === `
`) && (j = "");
  return y += j + S, n.inFlow ? L && s && s() : I && !L ? y += Lt(y, n.indent, a(I)) : p && i && i(), y;
}
function uc(e, t) {
  (e === "debug" || e === "warn") && console.warn(t);
}
const ds = "<<", yt = {
  identify: (e) => e === ds || typeof e == "symbol" && e.description === ds,
  default: "key",
  tag: "tag:yaml.org,2002:merge",
  test: /^<<$/,
  resolve: () => Object.assign(new K(Symbol(ds)), {
    addToJSMap: dc
  }),
  stringify: () => ds
}, _d = (e, t) => (yt.identify(t) || te(t) && (!t.type || t.type === K.PLAIN) && yt.identify(t.value)) && (e == null ? void 0 : e.doc.schema.tags.some((n) => n.tag === yt.tag && n.default));
function dc(e, t, n) {
  const s = pc(e, n);
  if (is(s))
    for (const i of s.items)
      Ni(e, t, i);
  else if (Array.isArray(s))
    for (const i of s)
      Ni(e, t, i);
  else
    Ni(e, t, s);
}
function Ni(e, t, n) {
  const s = pc(e, n);
  if (!ss(s))
    throw new Error("Merge sources must be maps or map aliases");
  const i = s.toJSON(null, e, Map);
  for (const [o, r] of i)
    t instanceof Map ? t.has(o) || t.set(o, r) : t instanceof Set ? t.add(o) : Object.prototype.hasOwnProperty.call(t, o) || Object.defineProperty(t, o, {
      value: r,
      writable: !0,
      enumerable: !0,
      configurable: !0
    });
  return t;
}
function pc(e, t) {
  return e && pn(t) ? t.resolve(e.doc, e) : t;
}
function hc(e, t, { key: n, value: s }) {
  if (re(n) && n.addToJSMap)
    n.addToJSMap(e, t, s);
  else if (_d(e, n))
    dc(e, t, s);
  else {
    const i = Ve(n, "", e);
    if (t instanceof Map)
      t.set(i, Ve(s, i, e));
    else if (t instanceof Set)
      t.add(i);
    else {
      const o = wd(n, i, e), r = Ve(s, o, e);
      o in t ? Object.defineProperty(t, o, {
        value: r,
        writable: !0,
        enumerable: !0,
        configurable: !0
      }) : t[o] = r;
    }
  }
  return t;
}
function wd(e, t, n) {
  if (t === null)
    return "";
  if (typeof t != "object")
    return String(t);
  if (re(e) && (n != null && n.doc)) {
    const s = fc(n.doc, {});
    s.anchors = /* @__PURE__ */ new Set();
    for (const o of n.anchors.keys())
      s.anchors.add(o.anchor);
    s.inFlow = !0, s.inStringifyKey = !0;
    const i = e.toString(s);
    if (!n.mapKeyWarned) {
      let o = JSON.stringify(i);
      o.length > 40 && (o = o.substring(0, 36) + '..."'), uc(n.doc.options.logLevel, `Keys with collection values will be stringified due to JS Object restrictions: ${o}. Set mapAsMap: true to use object keys.`), n.mapKeyWarned = !0;
    }
    return i;
  }
  return JSON.stringify(t);
}
function vo(e, t, n) {
  const s = Wn(e, void 0, n), i = Wn(t, void 0, n);
  return new Oe(s, i);
}
class Oe {
  constructor(t, n = null) {
    Object.defineProperty(this, xe, { value: nc }), this.key = t, this.value = n;
  }
  clone(t) {
    let { key: n, value: s } = this;
    return re(n) && (n = n.clone(t)), re(s) && (s = s.clone(t)), new Oe(n, s);
  }
  toJSON(t, n) {
    const s = n != null && n.mapAsMap ? /* @__PURE__ */ new Map() : {};
    return hc(n, s, this);
  }
  toString(t, n, s) {
    return t != null && t.doc ? bd(this, t, n, s) : JSON.stringify(this);
  }
}
function mc(e, t, n) {
  return (t.inFlow ?? e.flow ? vd : Ed)(e, t, n);
}
function Ed({ comment: e, items: t }, n, { blockItemPrefix: s, flowChars: i, itemIndent: o, onChompKeep: r, onComment: l }) {
  const { indent: c, options: { commentString: a } } = n, u = Object.assign({}, n, { indent: o, type: null });
  let f = !1;
  const h = [];
  for (let b = 0; b < t.length; ++b) {
    const p = t[b];
    let y = null;
    if (re(p))
      !f && p.spaceBefore && h.push(""), Fs(n, h, p.commentBefore, f), p.comment && (y = p.comment);
    else if (ce(p)) {
      const E = re(p.key) ? p.key : null;
      E && (!f && E.spaceBefore && h.push(""), Fs(n, h, E.commentBefore, f));
    }
    f = !1;
    let k = ln(p, u, () => y = null, () => f = !0);
    y && (k += Lt(k, o, a(y))), f && y && (f = !1), h.push(s + k);
  }
  let m;
  if (h.length === 0)
    m = i.start + i.end;
  else {
    m = h[0];
    for (let b = 1; b < h.length; ++b) {
      const p = h[b];
      m += p ? `
${c}${p}` : `
`;
    }
  }
  return e ? (m += `
` + ht(a(e), c), l && l()) : f && r && r(), m;
}
function vd({ items: e }, t, { flowChars: n, itemIndent: s }) {
  const { indent: i, indentStep: o, flowCollectionPadding: r, options: { commentString: l } } = t;
  s += o;
  const c = Object.assign({}, t, {
    indent: s,
    inFlow: !0,
    type: null
  });
  let a = !1, u = 0;
  const f = [];
  for (let b = 0; b < e.length; ++b) {
    const p = e[b];
    let y = null;
    if (re(p))
      p.spaceBefore && f.push(""), Fs(t, f, p.commentBefore, !1), p.comment && (y = p.comment);
    else if (ce(p)) {
      const E = re(p.key) ? p.key : null;
      E && (E.spaceBefore && f.push(""), Fs(t, f, E.commentBefore, !1), E.comment && (a = !0));
      const I = re(p.value) ? p.value : null;
      I ? (I.comment && (y = I.comment), I.commentBefore && (a = !0)) : p.value == null && (E != null && E.comment) && (y = E.comment);
    }
    y && (a = !0);
    let k = ln(p, c, () => y = null);
    a || (a = f.length > u || k.includes(`
`)), b < e.length - 1 ? k += "," : t.options.trailingComma && (t.options.lineWidth > 0 && (a || (a = f.reduce((E, I) => E + I.length + 2, 2) + (k.length + 2) > t.options.lineWidth)), a && (k += ",")), y && (k += Lt(k, s, l(y))), f.push(k), u = f.length;
  }
  const { start: h, end: m } = n;
  if (f.length === 0)
    return h + m;
  if (!a) {
    const b = f.reduce((p, y) => p + y.length + 2, 2);
    a = t.options.lineWidth > 0 && b > t.options.lineWidth;
  }
  if (a) {
    let b = h;
    for (const p of f)
      b += p ? `
${o}${i}${p}` : `
`;
    return `${b}
${i}${m}`;
  } else
    return `${h}${r}${f.join(" ")}${r}${m}`;
}
function Fs({ indent: e, options: { commentString: t } }, n, s, i) {
  if (s && i && (s = s.replace(/^\n+/, "")), s) {
    const o = ht(t(s), e);
    n.push(o.trimStart());
  }
}
function Mt(e, t) {
  const n = te(t) ? t.value : t;
  for (const s of e)
    if (ce(s) && (s.key === t || s.key === n || te(s.key) && s.key.value === n))
      return s;
}
class $e extends cc {
  static get tagName() {
    return "tag:yaml.org,2002:map";
  }
  constructor(t) {
    super(kt, t), this.items = [];
  }
  /**
   * A generic collection parsing method that can be extended
   * to other node classes that inherit from YAMLMap
   */
  static from(t, n, s) {
    const { keepUndefined: i, replacer: o } = s, r = new this(t), l = (c, a) => {
      if (typeof o == "function")
        a = o.call(n, c, a);
      else if (Array.isArray(o) && !o.includes(c))
        return;
      (a !== void 0 || i) && r.items.push(vo(c, a, s));
    };
    if (n instanceof Map)
      for (const [c, a] of n)
        l(c, a);
    else if (n && typeof n == "object")
      for (const c of Object.keys(n))
        l(c, n[c]);
    return typeof t.sortMapEntries == "function" && r.items.sort(t.sortMapEntries), r;
  }
  /**
   * Adds a value to the collection.
   *
   * @param overwrite - If not set `true`, using a key that is already in the
   *   collection will throw. Otherwise, overwrites the previous value.
   */
  add(t, n) {
    var r;
    let s;
    ce(t) ? s = t : !t || typeof t != "object" || !("key" in t) ? s = new Oe(t, t == null ? void 0 : t.value) : s = new Oe(t.key, t.value);
    const i = Mt(this.items, s.key), o = (r = this.schema) == null ? void 0 : r.sortMapEntries;
    if (i) {
      if (!n)
        throw new Error(`Key ${s.key} already set`);
      te(i.value) && lc(s.value) ? i.value.value = s.value : i.value = s.value;
    } else if (o) {
      const l = this.items.findIndex((c) => o(s, c) < 0);
      l === -1 ? this.items.push(s) : this.items.splice(l, 0, s);
    } else
      this.items.push(s);
  }
  delete(t) {
    const n = Mt(this.items, t);
    return n ? this.items.splice(this.items.indexOf(n), 1).length > 0 : !1;
  }
  get(t, n) {
    const s = Mt(this.items, t), i = s == null ? void 0 : s.value;
    return (!n && te(i) ? i.value : i) ?? void 0;
  }
  has(t) {
    return !!Mt(this.items, t);
  }
  set(t, n) {
    this.add(new Oe(t, n), !0);
  }
  /**
   * @param ctx - Conversion context, originally set in Document#toJS()
   * @param {Class} Type - If set, forces the returned collection type
   * @returns Instance of Type, Map, or Object
   */
  toJSON(t, n, s) {
    const i = s ? new s() : n != null && n.mapAsMap ? /* @__PURE__ */ new Map() : {};
    n != null && n.onCreate && n.onCreate(i);
    for (const o of this.items)
      hc(n, i, o);
    return i;
  }
  toString(t, n, s) {
    if (!t)
      return JSON.stringify(this);
    for (const i of this.items)
      if (!ce(i))
        throw new Error(`Map items must all be pairs; found ${JSON.stringify(i)} instead`);
    return !t.allNullValues && this.hasAllNullValues(!1) && (t = Object.assign({}, t, { allNullValues: !0 })), mc(this, t, {
      blockItemPrefix: "",
      flowChars: { start: "{", end: "}" },
      itemIndent: t.indent || "",
      onChompKeep: s,
      onComment: n
    });
  }
}
const mn = {
  collection: "map",
  default: !0,
  nodeClass: $e,
  tag: "tag:yaml.org,2002:map",
  resolve(e, t) {
    return ss(e) || t("Expected a mapping for this tag"), e;
  },
  createNode: (e, t, n) => $e.from(e, t, n)
};
class Bt extends cc {
  static get tagName() {
    return "tag:yaml.org,2002:seq";
  }
  constructor(t) {
    super(dn, t), this.items = [];
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
    const n = ps(t);
    return typeof n != "number" ? !1 : this.items.splice(n, 1).length > 0;
  }
  get(t, n) {
    const s = ps(t);
    if (typeof s != "number")
      return;
    const i = this.items[s];
    return !n && te(i) ? i.value : i;
  }
  /**
   * Checks if the collection includes a value with the key `key`.
   *
   * `key` must contain a representation of an integer for this to succeed.
   * It may be wrapped in a `Scalar`.
   */
  has(t) {
    const n = ps(t);
    return typeof n == "number" && n < this.items.length;
  }
  /**
   * Sets a value in this collection. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   *
   * If `key` does not contain a representation of an integer, this will throw.
   * It may be wrapped in a `Scalar`.
   */
  set(t, n) {
    const s = ps(t);
    if (typeof s != "number")
      throw new Error(`Expected a valid index, not ${t}.`);
    const i = this.items[s];
    te(i) && lc(n) ? i.value = n : this.items[s] = n;
  }
  toJSON(t, n) {
    const s = [];
    n != null && n.onCreate && n.onCreate(s);
    let i = 0;
    for (const o of this.items)
      s.push(Ve(o, String(i++), n));
    return s;
  }
  toString(t, n, s) {
    return t ? mc(this, t, {
      blockItemPrefix: "- ",
      flowChars: { start: "[", end: "]" },
      itemIndent: (t.indent || "") + "  ",
      onChompKeep: s,
      onComment: n
    }) : JSON.stringify(this);
  }
  static from(t, n, s) {
    const { replacer: i } = s, o = new this(t);
    if (n && Symbol.iterator in Object(n)) {
      let r = 0;
      for (let l of n) {
        if (typeof i == "function") {
          const c = n instanceof Set ? l : String(r++);
          l = i.call(n, c, l);
        }
        o.items.push(Wn(l, void 0, s));
      }
    }
    return o;
  }
}
function ps(e) {
  let t = te(e) ? e.value : e;
  return t && typeof t == "string" && (t = Number(t)), typeof t == "number" && Number.isInteger(t) && t >= 0 ? t : null;
}
const gn = {
  collection: "seq",
  default: !0,
  nodeClass: Bt,
  tag: "tag:yaml.org,2002:seq",
  resolve(e, t) {
    return is(e) || t("Expected a sequence for this tag"), e;
  },
  createNode: (e, t, n) => Bt.from(e, t, n)
}, si = {
  identify: (e) => typeof e == "string",
  default: !0,
  tag: "tag:yaml.org,2002:str",
  resolve: (e) => e,
  stringify(e, t, n, s) {
    return t = Object.assign({ actualString: !0 }, t), Eo(e, t, n, s);
  }
}, ii = {
  identify: (e) => e == null,
  createNode: () => new K(null),
  default: !0,
  tag: "tag:yaml.org,2002:null",
  test: /^(?:~|[Nn]ull|NULL)?$/,
  resolve: () => new K(null),
  stringify: ({ source: e }, t) => typeof e == "string" && ii.test.test(e) ? e : t.options.nullStr
}, No = {
  identify: (e) => typeof e == "boolean",
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:[Tt]rue|TRUE|[Ff]alse|FALSE)$/,
  resolve: (e) => new K(e[0] === "t" || e[0] === "T"),
  stringify({ source: e, value: t }, n) {
    if (e && No.test.test(e)) {
      const s = e[0] === "t" || e[0] === "T";
      if (t === s)
        return e;
    }
    return t ? n.options.trueStr : n.options.falseStr;
  }
};
function Je({ format: e, minFractionDigits: t, tag: n, value: s }) {
  if (typeof s == "bigint")
    return String(s);
  const i = typeof s == "number" ? s : Number(s);
  if (!isFinite(i))
    return isNaN(i) ? ".nan" : i < 0 ? "-.inf" : ".inf";
  let o = Object.is(s, -0) ? "-0" : JSON.stringify(s);
  if (!e && t && (!n || n === "tag:yaml.org,2002:float") && /^-?\d/.test(o) && !o.includes("e")) {
    let r = o.indexOf(".");
    r < 0 && (r = o.length, o += ".");
    let l = t - (o.length - r - 1);
    for (; l-- > 0; )
      o += "0";
  }
  return o;
}
const gc = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^(?:[-+]?\.(?:inf|Inf|INF)|\.nan|\.NaN|\.NAN)$/,
  resolve: (e) => e.slice(-3).toLowerCase() === "nan" ? NaN : e[0] === "-" ? Number.NEGATIVE_INFINITY : Number.POSITIVE_INFINITY,
  stringify: Je
}, yc = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "EXP",
  test: /^[-+]?(?:\.[0-9]+|[0-9]+(?:\.[0-9]*)?)[eE][-+]?[0-9]+$/,
  resolve: (e) => parseFloat(e),
  stringify(e) {
    const t = Number(e.value);
    return isFinite(t) ? t.toExponential() : Je(e);
  }
}, bc = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^[-+]?(?:\.[0-9]+|[0-9]+\.[0-9]*)$/,
  resolve(e) {
    const t = new K(parseFloat(e)), n = e.indexOf(".");
    return n !== -1 && e[e.length - 1] === "0" && (t.minFractionDigits = e.length - n - 1), t;
  },
  stringify: Je
}, oi = (e) => typeof e == "bigint" || Number.isInteger(e), Oo = (e, t, n, { intAsBigInt: s }) => s ? BigInt(e) : parseInt(e.substring(t), n);
function _c(e, t, n) {
  const { value: s } = e;
  return oi(s) && s >= 0 ? n + s.toString(t) : Je(e);
}
const wc = {
  identify: (e) => oi(e) && e >= 0,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "OCT",
  test: /^0o[0-7]+$/,
  resolve: (e, t, n) => Oo(e, 2, 8, n),
  stringify: (e) => _c(e, 8, "0o")
}, Ec = {
  identify: oi,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  test: /^[-+]?[0-9]+$/,
  resolve: (e, t, n) => Oo(e, 0, 10, n),
  stringify: Je
}, vc = {
  identify: (e) => oi(e) && e >= 0,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "HEX",
  test: /^0x[0-9a-fA-F]+$/,
  resolve: (e, t, n) => Oo(e, 2, 16, n),
  stringify: (e) => _c(e, 16, "0x")
}, Nd = [
  mn,
  gn,
  si,
  ii,
  No,
  wc,
  Ec,
  vc,
  gc,
  yc,
  bc
];
function Cr(e) {
  return typeof e == "bigint" || Number.isInteger(e);
}
const hs = ({ value: e }) => JSON.stringify(e), Od = [
  {
    identify: (e) => typeof e == "string",
    default: !0,
    tag: "tag:yaml.org,2002:str",
    resolve: (e) => e,
    stringify: hs
  },
  {
    identify: (e) => e == null,
    createNode: () => new K(null),
    default: !0,
    tag: "tag:yaml.org,2002:null",
    test: /^null$/,
    resolve: () => null,
    stringify: hs
  },
  {
    identify: (e) => typeof e == "boolean",
    default: !0,
    tag: "tag:yaml.org,2002:bool",
    test: /^true$|^false$/,
    resolve: (e) => e === "true",
    stringify: hs
  },
  {
    identify: Cr,
    default: !0,
    tag: "tag:yaml.org,2002:int",
    test: /^-?(?:0|[1-9][0-9]*)$/,
    resolve: (e, t, { intAsBigInt: n }) => n ? BigInt(e) : parseInt(e, 10),
    stringify: ({ value: e }) => Cr(e) ? e.toString() : JSON.stringify(e)
  },
  {
    identify: (e) => typeof e == "number",
    default: !0,
    tag: "tag:yaml.org,2002:float",
    test: /^-?(?:0|[1-9][0-9]*)(?:\.[0-9]*)?(?:[eE][-+]?[0-9]+)?$/,
    resolve: (e) => parseFloat(e),
    stringify: hs
  }
], Sd = {
  default: !0,
  tag: "",
  test: /^/,
  resolve(e, t) {
    return t(`Unresolved plain scalar ${JSON.stringify(e)}`), e;
  }
}, kd = [mn, gn].concat(Od, Sd), So = {
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
      const n = atob(e.replace(/[\n\r]/g, "")), s = new Uint8Array(n.length);
      for (let i = 0; i < n.length; ++i)
        s[i] = n.charCodeAt(i);
      return s;
    } else
      return t("This environment does not support reading binary tags; either Buffer or atob is required"), e;
  },
  stringify({ comment: e, type: t, value: n }, s, i, o) {
    if (!n)
      return "";
    const r = n;
    let l;
    if (typeof btoa == "function") {
      let c = "";
      for (let a = 0; a < r.length; ++a)
        c += String.fromCharCode(r[a]);
      l = btoa(c);
    } else
      throw new Error("This environment does not support writing binary tags; either Buffer or btoa is required");
    if (t ?? (t = K.BLOCK_LITERAL), t !== K.QUOTE_DOUBLE) {
      const c = Math.max(s.options.lineWidth - s.indent.length, s.options.minContentWidth), a = Math.ceil(l.length / c), u = new Array(a);
      for (let f = 0, h = 0; f < a; ++f, h += c)
        u[f] = l.substr(h, c);
      l = u.join(t === K.BLOCK_LITERAL ? `
` : " ");
    }
    return Eo({ comment: e, type: t, value: l }, s, i, o);
  }
};
function Nc(e, t) {
  if (is(e))
    for (let n = 0; n < e.items.length; ++n) {
      let s = e.items[n];
      if (!ce(s)) {
        if (ss(s)) {
          s.items.length > 1 && t("Each pair must have its own sequence indicator");
          const i = s.items[0] || new Oe(new K(null));
          if (s.commentBefore && (i.key.commentBefore = i.key.commentBefore ? `${s.commentBefore}
${i.key.commentBefore}` : s.commentBefore), s.comment) {
            const o = i.value ?? i.key;
            o.comment = o.comment ? `${s.comment}
${o.comment}` : s.comment;
          }
          s = i;
        }
        e.items[n] = ce(s) ? s : new Oe(s);
      }
    }
  else
    t("Expected a sequence for this tag");
  return e;
}
function Oc(e, t, n) {
  const { replacer: s } = n, i = new Bt(e);
  i.tag = "tag:yaml.org,2002:pairs";
  let o = 0;
  if (t && Symbol.iterator in Object(t))
    for (let r of t) {
      typeof s == "function" && (r = s.call(t, String(o++), r));
      let l, c;
      if (Array.isArray(r))
        if (r.length === 2)
          l = r[0], c = r[1];
        else
          throw new TypeError(`Expected [key, value] tuple: ${r}`);
      else if (r && r instanceof Object) {
        const a = Object.keys(r);
        if (a.length === 1)
          l = a[0], c = r[l];
        else
          throw new TypeError(`Expected tuple with one key, not ${a.length} keys`);
      } else
        l = r;
      i.items.push(vo(l, c, n));
    }
  return i;
}
const ko = {
  collection: "seq",
  default: !1,
  tag: "tag:yaml.org,2002:pairs",
  resolve: Nc,
  createNode: Oc
};
class nn extends Bt {
  constructor() {
    super(), this.add = $e.prototype.add.bind(this), this.delete = $e.prototype.delete.bind(this), this.get = $e.prototype.get.bind(this), this.has = $e.prototype.has.bind(this), this.set = $e.prototype.set.bind(this), this.tag = nn.tag;
  }
  /**
   * If `ctx` is given, the return type is actually `Map<unknown, unknown>`,
   * but TypeScript won't allow widening the signature of a child method.
   */
  toJSON(t, n) {
    if (!n)
      return super.toJSON(t);
    const s = /* @__PURE__ */ new Map();
    n != null && n.onCreate && n.onCreate(s);
    for (const i of this.items) {
      let o, r;
      if (ce(i) ? (o = Ve(i.key, "", n), r = Ve(i.value, o, n)) : o = Ve(i, "", n), s.has(o))
        throw new Error("Ordered maps must not include duplicate keys");
      s.set(o, r);
    }
    return s;
  }
  static from(t, n, s) {
    const i = Oc(t, n, s), o = new this();
    return o.items = i.items, o;
  }
}
nn.tag = "tag:yaml.org,2002:omap";
const To = {
  collection: "seq",
  identify: (e) => e instanceof Map,
  nodeClass: nn,
  default: !1,
  tag: "tag:yaml.org,2002:omap",
  resolve(e, t) {
    const n = Nc(e, t), s = [];
    for (const { key: i } of n.items)
      te(i) && (s.includes(i.value) ? t(`Ordered maps must not include duplicate keys: ${i.value}`) : s.push(i.value));
    return Object.assign(new nn(), n);
  },
  createNode: (e, t, n) => nn.from(e, t, n)
};
function Sc({ value: e, source: t }, n) {
  return t && (e ? kc : Tc).test.test(t) ? t : e ? n.options.trueStr : n.options.falseStr;
}
const kc = {
  identify: (e) => e === !0,
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:Y|y|[Yy]es|YES|[Tt]rue|TRUE|[Oo]n|ON)$/,
  resolve: () => new K(!0),
  stringify: Sc
}, Tc = {
  identify: (e) => e === !1,
  default: !0,
  tag: "tag:yaml.org,2002:bool",
  test: /^(?:N|n|[Nn]o|NO|[Ff]alse|FALSE|[Oo]ff|OFF)$/,
  resolve: () => new K(!1),
  stringify: Sc
}, Td = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^(?:[-+]?\.(?:inf|Inf|INF)|\.nan|\.NaN|\.NAN)$/,
  resolve: (e) => e.slice(-3).toLowerCase() === "nan" ? NaN : e[0] === "-" ? Number.NEGATIVE_INFINITY : Number.POSITIVE_INFINITY,
  stringify: Je
}, Dd = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "EXP",
  test: /^[-+]?(?:[0-9][0-9_]*)?(?:\.[0-9_]*)?[eE][-+]?[0-9]+$/,
  resolve: (e) => parseFloat(e.replace(/_/g, "")),
  stringify(e) {
    const t = Number(e.value);
    return isFinite(t) ? t.toExponential() : Je(e);
  }
}, Cd = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  test: /^[-+]?(?:[0-9][0-9_]*)?\.[0-9_]*$/,
  resolve(e) {
    const t = new K(parseFloat(e.replace(/_/g, ""))), n = e.indexOf(".");
    if (n !== -1) {
      const s = e.substring(n + 1).replace(/_/g, "");
      s[s.length - 1] === "0" && (t.minFractionDigits = s.length);
    }
    return t;
  },
  stringify: Je
}, os = (e) => typeof e == "bigint" || Number.isInteger(e);
function ri(e, t, n, { intAsBigInt: s }) {
  const i = e[0];
  if ((i === "-" || i === "+") && (t += 1), e = e.substring(t).replace(/_/g, ""), s) {
    switch (n) {
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
    const r = BigInt(e);
    return i === "-" ? BigInt(-1) * r : r;
  }
  const o = parseInt(e, n);
  return i === "-" ? -1 * o : o;
}
function Do(e, t, n) {
  const { value: s } = e;
  if (os(s)) {
    const i = s.toString(t);
    return s < 0 ? "-" + n + i.substr(1) : n + i;
  }
  return Je(e);
}
const Ad = {
  identify: os,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "BIN",
  test: /^[-+]?0b[0-1_]+$/,
  resolve: (e, t, n) => ri(e, 2, 2, n),
  stringify: (e) => Do(e, 2, "0b")
}, Id = {
  identify: os,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "OCT",
  test: /^[-+]?0[0-7_]+$/,
  resolve: (e, t, n) => ri(e, 1, 8, n),
  stringify: (e) => Do(e, 8, "0")
}, $d = {
  identify: os,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  test: /^[-+]?[0-9][0-9_]*$/,
  resolve: (e, t, n) => ri(e, 0, 10, n),
  stringify: Je
}, Vd = {
  identify: os,
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "HEX",
  test: /^[-+]?0x[0-9a-fA-F_]+$/,
  resolve: (e, t, n) => ri(e, 2, 16, n),
  stringify: (e) => Do(e, 16, "0x")
};
class sn extends $e {
  constructor(t) {
    super(t), this.tag = sn.tag;
  }
  add(t) {
    let n;
    ce(t) ? n = t : t && typeof t == "object" && "key" in t && "value" in t && t.value === null ? n = new Oe(t.key, null) : n = new Oe(t, null), Mt(this.items, n.key) || this.items.push(n);
  }
  /**
   * If `keepPair` is `true`, returns the Pair matching `key`.
   * Otherwise, returns the value of that Pair's key.
   */
  get(t, n) {
    const s = Mt(this.items, t);
    return !n && ce(s) ? te(s.key) ? s.key.value : s.key : s;
  }
  set(t, n) {
    if (typeof n != "boolean")
      throw new Error(`Expected boolean value for set(key, value) in a YAML set, not ${typeof n}`);
    const s = Mt(this.items, t);
    s && !n ? this.items.splice(this.items.indexOf(s), 1) : !s && n && this.items.push(new Oe(t));
  }
  toJSON(t, n) {
    return super.toJSON(t, n, Set);
  }
  toString(t, n, s) {
    if (!t)
      return JSON.stringify(this);
    if (this.hasAllNullValues(!0))
      return super.toString(Object.assign({}, t, { allNullValues: !0 }), n, s);
    throw new Error("Set items must all have null values");
  }
  static from(t, n, s) {
    const { replacer: i } = s, o = new this(t);
    if (n && Symbol.iterator in Object(n))
      for (let r of n)
        typeof i == "function" && (r = i.call(n, r, r)), o.items.push(vo(r, null, s));
    return o;
  }
}
sn.tag = "tag:yaml.org,2002:set";
const Co = {
  collection: "map",
  identify: (e) => e instanceof Set,
  nodeClass: sn,
  default: !1,
  tag: "tag:yaml.org,2002:set",
  createNode: (e, t, n) => sn.from(e, t, n),
  resolve(e, t) {
    if (ss(e)) {
      if (e.hasAllNullValues(!0))
        return Object.assign(new sn(), e);
      t("Set items must all have null values");
    } else
      t("Expected a mapping for this tag");
    return e;
  }
};
function Ao(e, t) {
  const n = e[0], s = n === "-" || n === "+" ? e.substring(1) : e, i = (r) => t ? BigInt(r) : Number(r), o = s.replace(/_/g, "").split(":").reduce((r, l) => r * i(60) + i(l), i(0));
  return n === "-" ? i(-1) * o : o;
}
function Dc(e) {
  let { value: t } = e, n = (r) => r;
  if (typeof t == "bigint")
    n = (r) => BigInt(r);
  else if (isNaN(t) || !isFinite(t))
    return Je(e);
  let s = "";
  t < 0 && (s = "-", t *= n(-1));
  const i = n(60), o = [t % i];
  return t < 60 ? o.unshift(0) : (t = (t - o[0]) / i, o.unshift(t % i), t >= 60 && (t = (t - o[0]) / i, o.unshift(t))), s + o.map((r) => String(r).padStart(2, "0")).join(":").replace(/000000\d*$/, "");
}
const Cc = {
  identify: (e) => typeof e == "bigint" || Number.isInteger(e),
  default: !0,
  tag: "tag:yaml.org,2002:int",
  format: "TIME",
  test: /^[-+]?[0-9][0-9_]*(?::[0-5]?[0-9])+$/,
  resolve: (e, t, { intAsBigInt: n }) => Ao(e, n),
  stringify: Dc
}, Ac = {
  identify: (e) => typeof e == "number",
  default: !0,
  tag: "tag:yaml.org,2002:float",
  format: "TIME",
  test: /^[-+]?[0-9][0-9_]*(?::[0-5]?[0-9])+\.[0-9_]*$/,
  resolve: (e) => Ao(e, !1),
  stringify: Dc
}, li = {
  identify: (e) => e instanceof Date,
  default: !0,
  tag: "tag:yaml.org,2002:timestamp",
  // If the time zone is omitted, the timestamp is assumed to be specified in UTC. The time part
  // may be omitted altogether, resulting in a date format. In such a case, the time part is
  // assumed to be 00:00:00Z (start of day, UTC).
  test: RegExp("^([0-9]{4})-([0-9]{1,2})-([0-9]{1,2})(?:(?:t|T|[ \\t]+)([0-9]{1,2}):([0-9]{1,2}):([0-9]{1,2}(\\.[0-9]+)?)(?:[ \\t]*(Z|[-+][012]?[0-9](?::[0-9]{2})?))?)?$"),
  resolve(e) {
    const t = e.match(li.test);
    if (!t)
      throw new Error("!!timestamp expects a date, starting with yyyy-mm-dd");
    const [, n, s, i, o, r, l] = t.map(Number), c = t[7] ? Number((t[7] + "00").substr(1, 3)) : 0;
    let a = Date.UTC(n, s - 1, i, o || 0, r || 0, l || 0, c);
    const u = t[8];
    if (u && u !== "Z") {
      let f = Ao(u, !1);
      Math.abs(f) < 30 && (f *= 60), a -= 6e4 * f;
    }
    return new Date(a);
  },
  stringify: ({ value: e }) => (e == null ? void 0 : e.toISOString().replace(/(T00:00:00)?\.000Z$/, "")) ?? ""
}, Ar = [
  mn,
  gn,
  si,
  ii,
  kc,
  Tc,
  Ad,
  Id,
  $d,
  Vd,
  Td,
  Dd,
  Cd,
  So,
  yt,
  To,
  ko,
  Co,
  Cc,
  Ac,
  li
], Ir = /* @__PURE__ */ new Map([
  ["core", Nd],
  ["failsafe", [mn, gn, si]],
  ["json", kd],
  ["yaml11", Ar],
  ["yaml-1.1", Ar]
]), $r = {
  binary: So,
  bool: No,
  float: bc,
  floatExp: yc,
  floatNaN: gc,
  floatTime: Ac,
  int: Ec,
  intHex: vc,
  intOct: wc,
  intTime: Cc,
  map: mn,
  merge: yt,
  null: ii,
  omap: To,
  pairs: ko,
  seq: gn,
  set: Co,
  timestamp: li
}, Ld = {
  "tag:yaml.org,2002:binary": So,
  "tag:yaml.org,2002:merge": yt,
  "tag:yaml.org,2002:omap": To,
  "tag:yaml.org,2002:pairs": ko,
  "tag:yaml.org,2002:set": Co,
  "tag:yaml.org,2002:timestamp": li
};
function Oi(e, t, n) {
  const s = Ir.get(t);
  if (s && !e)
    return n && !s.includes(yt) ? s.concat(yt) : s.slice();
  let i = s;
  if (!i)
    if (Array.isArray(e))
      i = [];
    else {
      const o = Array.from(Ir.keys()).filter((r) => r !== "yaml11").map((r) => JSON.stringify(r)).join(", ");
      throw new Error(`Unknown schema "${t}"; use one of ${o} or define customTags array`);
    }
  if (Array.isArray(e))
    for (const o of e)
      i = i.concat(o);
  else typeof e == "function" && (i = e(i.slice()));
  return n && (i = i.concat(yt)), i.reduce((o, r) => {
    const l = typeof r == "string" ? $r[r] : r;
    if (!l) {
      const c = JSON.stringify(r), a = Object.keys($r).map((u) => JSON.stringify(u)).join(", ");
      throw new Error(`Unknown custom tag ${c}; use one of ${a}`);
    }
    return o.includes(l) || o.push(l), o;
  }, []);
}
const Md = (e, t) => e.key < t.key ? -1 : e.key > t.key ? 1 : 0;
class Io {
  constructor({ compat: t, customTags: n, merge: s, resolveKnownTags: i, schema: o, sortMapEntries: r, toStringDefaults: l }) {
    this.compat = Array.isArray(t) ? Oi(t, "compat") : t ? Oi(null, t) : null, this.name = typeof o == "string" && o || "core", this.knownTags = i ? Ld : {}, this.tags = Oi(n, this.name, s), this.toStringOptions = l ?? null, Object.defineProperty(this, kt, { value: mn }), Object.defineProperty(this, rt, { value: si }), Object.defineProperty(this, dn, { value: gn }), this.sortMapEntries = typeof r == "function" ? r : r === !0 ? Md : null;
  }
  clone() {
    const t = Object.create(Io.prototype, Object.getOwnPropertyDescriptors(this));
    return t.tags = this.tags.slice(), t;
  }
}
function xd(e, t) {
  var c;
  const n = [];
  let s = t.directives === !0;
  if (t.directives !== !1 && e.directives) {
    const a = e.directives.toString(e);
    a ? (n.push(a), s = !0) : e.directives.docStart && (s = !0);
  }
  s && n.push("---");
  const i = fc(e, t), { commentString: o } = i.options;
  if (e.commentBefore) {
    n.length !== 1 && n.unshift("");
    const a = o(e.commentBefore);
    n.unshift(ht(a, ""));
  }
  let r = !1, l = null;
  if (e.contents) {
    if (re(e.contents)) {
      if (e.contents.spaceBefore && s && n.push(""), e.contents.commentBefore) {
        const f = o(e.contents.commentBefore);
        n.push(ht(f, ""));
      }
      i.forceBlockIndent = !!e.comment, l = e.contents.comment;
    }
    const a = l ? void 0 : () => r = !0;
    let u = ln(e.contents, i, () => l = null, a);
    l && (u += Lt(u, "", o(l))), (u[0] === "|" || u[0] === ">") && n[n.length - 1] === "---" ? n[n.length - 1] = `--- ${u}` : n.push(u);
  } else
    n.push(ln(e.contents, i));
  if ((c = e.directives) != null && c.docEnd)
    if (e.comment) {
      const a = o(e.comment);
      a.includes(`
`) ? (n.push("..."), n.push(ht(a, ""))) : n.push(`... ${a}`);
    } else
      n.push("...");
  else {
    let a = e.comment;
    a && r && (a = a.replace(/^\n+/, "")), a && ((!r || l) && n[n.length - 1] !== "" && n.push(""), n.push(ht(o(a), "")));
  }
  return n.join(`
`) + `
`;
}
let $o = class Ic {
  constructor(t, n, s) {
    this.commentBefore = null, this.comment = null, this.errors = [], this.warnings = [], Object.defineProperty(this, xe, { value: qi });
    let i = null;
    typeof n == "function" || Array.isArray(n) ? i = n : s === void 0 && n && (s = n, n = void 0);
    const o = Object.assign({
      intAsBigInt: !1,
      keepSourceTokens: !1,
      logLevel: "warn",
      prettyErrors: !0,
      strict: !0,
      stringKeys: !1,
      uniqueKeys: !0,
      version: "1.2"
    }, s);
    this.options = o;
    let { version: r } = o;
    s != null && s._directives ? (this.directives = s._directives.atDocument(), this.directives.yaml.explicit && (r = this.directives.yaml.version)) : this.directives = new we({ version: r }), this.setSchema(r, s), this.contents = t === void 0 ? null : this.createNode(t, i, s);
  }
  /**
   * Create a deep copy of this Document and its contents.
   *
   * Custom Node values that inherit from `Object` still refer to their original instances.
   */
  clone() {
    const t = Object.create(Ic.prototype, {
      [xe]: { value: qi }
    });
    return t.commentBefore = this.commentBefore, t.comment = this.comment, t.errors = this.errors.slice(), t.warnings = this.warnings.slice(), t.options = Object.assign({}, this.options), this.directives && (t.directives = this.directives.clone()), t.schema = this.schema.clone(), t.contents = re(this.contents) ? this.contents.clone(t.schema) : this.contents, this.range && (t.range = this.range.slice()), t;
  }
  /** Adds a value to the document. */
  add(t) {
    Wt(this.contents) && this.contents.add(t);
  }
  /** Adds a value to the document. */
  addIn(t, n) {
    Wt(this.contents) && this.contents.addIn(t, n);
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
  createAlias(t, n) {
    if (!t.anchor) {
      const s = oc(this);
      t.anchor = // eslint-disable-next-line @typescript-eslint/prefer-nullish-coalescing
      !n || s.has(n) ? rc(n || "a", s) : n;
    }
    return new wo(t.anchor);
  }
  createNode(t, n, s) {
    let i;
    if (typeof n == "function")
      t = n.call({ "": t }, "", t), i = n;
    else if (Array.isArray(n)) {
      const y = (E) => typeof E == "number" || E instanceof String || E instanceof Number, k = n.filter(y).map(String);
      k.length > 0 && (n = n.concat(k)), i = n;
    } else s === void 0 && n && (s = n, n = void 0);
    const { aliasDuplicateObjects: o, anchorPrefix: r, flow: l, keepUndefined: c, onTagObj: a, tag: u } = s ?? {}, { onAnchor: f, setAnchors: h, sourceObjects: m } = fd(
      this,
      // eslint-disable-next-line @typescript-eslint/prefer-nullish-coalescing
      r || "a"
    ), b = {
      aliasDuplicateObjects: o ?? !0,
      keepUndefined: c ?? !1,
      onAnchor: f,
      onTagObj: a,
      replacer: i,
      schema: this.schema,
      sourceObjects: m
    }, p = Wn(t, u, b);
    return l && oe(p) && (p.flow = !0), h(), p;
  }
  /**
   * Convert a key and a value into a `Pair` using the current schema,
   * recursively wrapping all values as `Scalar` or `Collection` nodes.
   */
  createPair(t, n, s = {}) {
    const i = this.createNode(t, null, s), o = this.createNode(n, null, s);
    return new Oe(i, o);
  }
  /**
   * Removes a value from the document.
   * @returns `true` if the item was found and removed.
   */
  delete(t) {
    return Wt(this.contents) ? this.contents.delete(t) : !1;
  }
  /**
   * Removes a value from the document.
   * @returns `true` if the item was found and removed.
   */
  deleteIn(t) {
    return Cn(t) ? this.contents == null ? !1 : (this.contents = null, !0) : Wt(this.contents) ? this.contents.deleteIn(t) : !1;
  }
  /**
   * Returns item at `key`, or `undefined` if not found. By default unwraps
   * scalar values from their surrounding node; to disable set `keepScalar` to
   * `true` (collections are always returned intact).
   */
  get(t, n) {
    return oe(this.contents) ? this.contents.get(t, n) : void 0;
  }
  /**
   * Returns item at `path`, or `undefined` if not found. By default unwraps
   * scalar values from their surrounding node; to disable set `keepScalar` to
   * `true` (collections are always returned intact).
   */
  getIn(t, n) {
    return Cn(t) ? !n && te(this.contents) ? this.contents.value : this.contents : oe(this.contents) ? this.contents.getIn(t, n) : void 0;
  }
  /**
   * Checks if the document includes a value with the key `key`.
   */
  has(t) {
    return oe(this.contents) ? this.contents.has(t) : !1;
  }
  /**
   * Checks if the document includes a value at `path`.
   */
  hasIn(t) {
    return Cn(t) ? this.contents !== void 0 : oe(this.contents) ? this.contents.hasIn(t) : !1;
  }
  /**
   * Sets a value in this document. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   */
  set(t, n) {
    this.contents == null ? this.contents = Rs(this.schema, [t], n) : Wt(this.contents) && this.contents.set(t, n);
  }
  /**
   * Sets a value in this document. For `!!set`, `value` needs to be a
   * boolean to add/remove the item from the set.
   */
  setIn(t, n) {
    Cn(t) ? this.contents = n : this.contents == null ? this.contents = Rs(this.schema, Array.from(t), n) : Wt(this.contents) && this.contents.setIn(t, n);
  }
  /**
   * Change the YAML version and schema used by the document.
   * A `null` version disables support for directives, explicit tags, anchors, and aliases.
   * It also requires the `schema` option to be given as a `Schema` instance value.
   *
   * Overrides all previously set schema options.
   */
  setSchema(t, n = {}) {
    typeof t == "number" && (t = String(t));
    let s;
    switch (t) {
      case "1.1":
        this.directives ? this.directives.yaml.version = "1.1" : this.directives = new we({ version: "1.1" }), s = { resolveKnownTags: !1, schema: "yaml-1.1" };
        break;
      case "1.2":
      case "next":
        this.directives ? this.directives.yaml.version = t : this.directives = new we({ version: t }), s = { resolveKnownTags: !0, schema: "core" };
        break;
      case null:
        this.directives && delete this.directives, s = null;
        break;
      default: {
        const i = JSON.stringify(t);
        throw new Error(`Expected '1.1', '1.2' or null as first argument, but found: ${i}`);
      }
    }
    if (n.schema instanceof Object)
      this.schema = n.schema;
    else if (s)
      this.schema = new Io(Object.assign(s, n));
    else
      throw new Error("With a null YAML version, the { schema: Schema } option is required");
  }
  // json & jsonArg are only used from toJSON()
  toJS({ json: t, jsonArg: n, mapAsMap: s, maxAliasCount: i, onAnchor: o, reviver: r } = {}) {
    const l = {
      anchors: /* @__PURE__ */ new Map(),
      doc: this,
      keep: !t,
      mapAsMap: s === !0,
      mapKeyWarned: !1,
      maxAliasCount: typeof i == "number" ? i : 100
    }, c = Ve(this.contents, n ?? "", l);
    if (typeof o == "function")
      for (const { count: a, res: u } of l.anchors.values())
        o(u, a);
    return typeof r == "function" ? Xt(r, { "": c }, "", c) : c;
  }
  /**
   * A JSON representation of the document `contents`.
   *
   * @param jsonArg Used by `JSON.stringify` to indicate the array index or
   *   property name.
   */
  toJSON(t, n) {
    return this.toJS({ json: !0, jsonArg: t, mapAsMap: !1, onAnchor: n });
  }
  /** A YAML representation of the document. */
  toString(t = {}) {
    if (this.errors.length > 0)
      throw new Error("Document with errors cannot be stringified");
    if ("indent" in t && (!Number.isInteger(t.indent) || Number(t.indent) <= 0)) {
      const n = JSON.stringify(t.indent);
      throw new Error(`"indent" option must be a positive integer, not ${n}`);
    }
    return xd(this, t);
  }
};
function Wt(e) {
  if (oe(e))
    return !0;
  throw new Error("Expected a YAML collection as document contents");
}
class $c extends Error {
  constructor(t, n, s, i) {
    super(), this.name = t, this.code = s, this.message = i, this.pos = n;
  }
}
class An extends $c {
  constructor(t, n, s) {
    super("YAMLParseError", t, n, s);
  }
}
class Pd extends $c {
  constructor(t, n, s) {
    super("YAMLWarning", t, n, s);
  }
}
const Vr = (e, t) => (n) => {
  if (n.pos[0] === -1)
    return;
  n.linePos = n.pos.map((l) => t.linePos(l));
  const { line: s, col: i } = n.linePos[0];
  n.message += ` at line ${s}, column ${i}`;
  let o = i - 1, r = e.substring(t.lineStarts[s - 1], t.lineStarts[s]).replace(/[\n\r]+$/, "");
  if (o >= 60 && r.length > 80) {
    const l = Math.min(o - 39, r.length - 79);
    r = "…" + r.substring(l), o -= l - 1;
  }
  if (r.length > 80 && (r = r.substring(0, 79) + "…"), s > 1 && /^ *$/.test(r.substring(0, o))) {
    let l = e.substring(t.lineStarts[s - 2], t.lineStarts[s - 1]);
    l.length > 80 && (l = l.substring(0, 79) + `…
`), r = l + r;
  }
  if (/[^ ]/.test(r)) {
    let l = 1;
    const c = n.linePos[1];
    (c == null ? void 0 : c.line) === s && c.col > i && (l = Math.max(1, Math.min(c.col - i, 80 - o)));
    const a = " ".repeat(o) + "^".repeat(l);
    n.message += `:

${r}
${a}
`;
  }
};
function cn(e, { flow: t, indicator: n, next: s, offset: i, onError: o, parentIndent: r, startOnNewline: l }) {
  let c = !1, a = l, u = l, f = "", h = "", m = !1, b = !1, p = null, y = null, k = null, E = null, I = null, L = null, S = null;
  for (const V of e)
    switch (b && (V.type !== "space" && V.type !== "newline" && V.type !== "comma" && o(V.offset, "MISSING_CHAR", "Tags and anchors must be separated from the next token by white space"), b = !1), p && (a && V.type !== "comment" && V.type !== "newline" && o(p, "TAB_AS_INDENT", "Tabs are not allowed as indentation"), p = null), V.type) {
      case "space":
        !t && (n !== "doc-start" || (s == null ? void 0 : s.type) !== "flow-collection") && V.source.includes("	") && (p = V), u = !0;
        break;
      case "comment": {
        u || o(V, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters");
        const U = V.source.substring(1) || " ";
        f ? f += h + U : f = U, h = "", a = !1;
        break;
      }
      case "newline":
        a ? f ? f += V.source : (!L || n !== "seq-item-ind") && (c = !0) : h += V.source, a = !0, m = !0, (y || k) && (E = V), u = !0;
        break;
      case "anchor":
        y && o(V, "MULTIPLE_ANCHORS", "A node can have at most one anchor"), V.source.endsWith(":") && o(V.offset + V.source.length - 1, "BAD_ALIAS", "Anchor ending in : is ambiguous", !0), y = V, S ?? (S = V.offset), a = !1, u = !1, b = !0;
        break;
      case "tag": {
        k && o(V, "MULTIPLE_TAGS", "A node can have at most one tag"), k = V, S ?? (S = V.offset), a = !1, u = !1, b = !0;
        break;
      }
      case n:
        (y || k) && o(V, "BAD_PROP_ORDER", `Anchors and tags must be after the ${V.source} indicator`), L && o(V, "UNEXPECTED_TOKEN", `Unexpected ${V.source} in ${t ?? "collection"}`), L = V, a = n === "seq-item-ind" || n === "explicit-key-ind", u = !1;
        break;
      case "comma":
        if (t) {
          I && o(V, "UNEXPECTED_TOKEN", `Unexpected , in ${t}`), I = V, a = !1, u = !1;
          break;
        }
      // else fallthrough
      default:
        o(V, "UNEXPECTED_TOKEN", `Unexpected ${V.type} token`), a = !1, u = !1;
    }
  const j = e[e.length - 1], M = j ? j.offset + j.source.length : i;
  return b && s && s.type !== "space" && s.type !== "newline" && s.type !== "comma" && (s.type !== "scalar" || s.source !== "") && o(s.offset, "MISSING_CHAR", "Tags and anchors must be separated from the next token by white space"), p && (a && p.indent <= r || (s == null ? void 0 : s.type) === "block-map" || (s == null ? void 0 : s.type) === "block-seq") && o(p, "TAB_AS_INDENT", "Tabs are not allowed as indentation"), {
    comma: I,
    found: L,
    spaceBefore: c,
    comment: f,
    hasNewline: m,
    anchor: y,
    tag: k,
    newlineAfterProp: E,
    end: M,
    start: S ?? M
  };
}
function Jn(e) {
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
        for (const n of t.start)
          if (n.type === "newline")
            return !0;
        if (t.sep) {
          for (const n of t.sep)
            if (n.type === "newline")
              return !0;
        }
        if (Jn(t.key) || Jn(t.value))
          return !0;
      }
      return !1;
    default:
      return !0;
  }
}
function Yi(e, t, n) {
  if ((t == null ? void 0 : t.type) === "flow-collection") {
    const s = t.end[0];
    s.indent === e && (s.source === "]" || s.source === "}") && Jn(t) && n(s, "BAD_INDENT", "Flow end indicator should be more indented than parent", !0);
  }
}
function Vc(e, t, n) {
  const { uniqueKeys: s } = e.options;
  if (s === !1)
    return !1;
  const i = typeof s == "function" ? s : (o, r) => o === r || te(o) && te(r) && o.value === r.value;
  return t.some((o) => i(o.key, n));
}
const Lr = "All mapping items must start at the same column";
function jd({ composeNode: e, composeEmptyNode: t }, n, s, i, o) {
  var u;
  const r = (o == null ? void 0 : o.nodeClass) ?? $e, l = new r(n.schema);
  n.atRoot && (n.atRoot = !1);
  let c = s.offset, a = null;
  for (const f of s.items) {
    const { start: h, key: m, sep: b, value: p } = f, y = cn(h, {
      indicator: "explicit-key-ind",
      next: m ?? (b == null ? void 0 : b[0]),
      offset: c,
      onError: i,
      parentIndent: s.indent,
      startOnNewline: !0
    }), k = !y.found;
    if (k) {
      if (m && (m.type === "block-seq" ? i(c, "BLOCK_AS_IMPLICIT_KEY", "A block sequence may not be used as an implicit map key") : "indent" in m && m.indent !== s.indent && i(c, "BAD_INDENT", Lr)), !y.anchor && !y.tag && !b) {
        a = y.end, y.comment && (l.comment ? l.comment += `
` + y.comment : l.comment = y.comment);
        continue;
      }
      (y.newlineAfterProp || Jn(m)) && i(m ?? h[h.length - 1], "MULTILINE_IMPLICIT_KEY", "Implicit keys need to be on a single line");
    } else ((u = y.found) == null ? void 0 : u.indent) !== s.indent && i(c, "BAD_INDENT", Lr);
    n.atKey = !0;
    const E = y.end, I = m ? e(n, m, y, i) : t(n, E, h, null, y, i);
    n.schema.compat && Yi(s.indent, m, i), n.atKey = !1, Vc(n, l.items, I) && i(E, "DUPLICATE_KEY", "Map keys must be unique");
    const L = cn(b ?? [], {
      indicator: "map-value-ind",
      next: p,
      offset: I.range[2],
      onError: i,
      parentIndent: s.indent,
      startOnNewline: !m || m.type === "block-scalar"
    });
    if (c = L.end, L.found) {
      k && ((p == null ? void 0 : p.type) === "block-map" && !L.hasNewline && i(c, "BLOCK_AS_IMPLICIT_KEY", "Nested mappings are not allowed in compact mappings"), n.options.strict && y.start < L.found.offset - 1024 && i(I.range, "KEY_OVER_1024_CHARS", "The : indicator must be at most 1024 chars after the start of an implicit block mapping key"));
      const S = p ? e(n, p, L, i) : t(n, c, b, null, L, i);
      n.schema.compat && Yi(s.indent, p, i), c = S.range[2];
      const j = new Oe(I, S);
      n.options.keepSourceTokens && (j.srcToken = f), l.items.push(j);
    } else {
      k && i(I.range, "MISSING_CHAR", "Implicit map keys need to be followed by map values"), L.comment && (I.comment ? I.comment += `
` + L.comment : I.comment = L.comment);
      const S = new Oe(I);
      n.options.keepSourceTokens && (S.srcToken = f), l.items.push(S);
    }
  }
  return a && a < c && i(a, "IMPOSSIBLE", "Map comment with trailing content"), l.range = [s.offset, c, a ?? c], l;
}
function Rd({ composeNode: e, composeEmptyNode: t }, n, s, i, o) {
  const r = (o == null ? void 0 : o.nodeClass) ?? Bt, l = new r(n.schema);
  n.atRoot && (n.atRoot = !1), n.atKey && (n.atKey = !1);
  let c = s.offset, a = null;
  for (const { start: u, value: f } of s.items) {
    const h = cn(u, {
      indicator: "seq-item-ind",
      next: f,
      offset: c,
      onError: i,
      parentIndent: s.indent,
      startOnNewline: !0
    });
    if (!h.found)
      if (h.anchor || h.tag || f)
        (f == null ? void 0 : f.type) === "block-seq" ? i(h.end, "BAD_INDENT", "All sequence items must start at the same column") : i(c, "MISSING_CHAR", "Sequence item without - indicator");
      else {
        a = h.end, h.comment && (l.comment = h.comment);
        continue;
      }
    const m = f ? e(n, f, h, i) : t(n, h.end, u, null, h, i);
    n.schema.compat && Yi(s.indent, f, i), c = m.range[2], l.items.push(m);
  }
  return l.range = [s.offset, c, a ?? c], l;
}
function rs(e, t, n, s) {
  let i = "";
  if (e) {
    let o = !1, r = "";
    for (const l of e) {
      const { source: c, type: a } = l;
      switch (a) {
        case "space":
          o = !0;
          break;
        case "comment": {
          n && !o && s(l, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters");
          const u = c.substring(1) || " ";
          i ? i += r + u : i = u, r = "";
          break;
        }
        case "newline":
          i && (r += c), o = !0;
          break;
        default:
          s(l, "UNEXPECTED_TOKEN", `Unexpected ${a} at node end`);
      }
      t += c.length;
    }
  }
  return { comment: i, offset: t };
}
const Si = "Block collections are not allowed within flow collections", ki = (e) => e && (e.type === "block-map" || e.type === "block-seq");
function Fd({ composeNode: e, composeEmptyNode: t }, n, s, i, o) {
  var y;
  const r = s.start.source === "{", l = r ? "flow map" : "flow sequence", c = (o == null ? void 0 : o.nodeClass) ?? (r ? $e : Bt), a = new c(n.schema);
  a.flow = !0;
  const u = n.atRoot;
  u && (n.atRoot = !1), n.atKey && (n.atKey = !1);
  let f = s.offset + s.start.source.length;
  for (let k = 0; k < s.items.length; ++k) {
    const E = s.items[k], { start: I, key: L, sep: S, value: j } = E, M = cn(I, {
      flow: l,
      indicator: "explicit-key-ind",
      next: L ?? (S == null ? void 0 : S[0]),
      offset: f,
      onError: i,
      parentIndent: s.indent,
      startOnNewline: !1
    });
    if (!M.found) {
      if (!M.anchor && !M.tag && !S && !j) {
        k === 0 && M.comma ? i(M.comma, "UNEXPECTED_TOKEN", `Unexpected , in ${l}`) : k < s.items.length - 1 && i(M.start, "UNEXPECTED_TOKEN", `Unexpected empty item in ${l}`), M.comment && (a.comment ? a.comment += `
` + M.comment : a.comment = M.comment), f = M.end;
        continue;
      }
      !r && n.options.strict && Jn(L) && i(
        L,
        // checked by containsNewline()
        "MULTILINE_IMPLICIT_KEY",
        "Implicit keys of flow sequence pairs need to be on a single line"
      );
    }
    if (k === 0)
      M.comma && i(M.comma, "UNEXPECTED_TOKEN", `Unexpected , in ${l}`);
    else if (M.comma || i(M.start, "MISSING_CHAR", `Missing , between ${l} items`), M.comment) {
      let V = "";
      e: for (const U of I)
        switch (U.type) {
          case "comma":
          case "space":
            break;
          case "comment":
            V = U.source.substring(1);
            break e;
          default:
            break e;
        }
      if (V) {
        let U = a.items[a.items.length - 1];
        ce(U) && (U = U.value ?? U.key), U.comment ? U.comment += `
` + V : U.comment = V, M.comment = M.comment.substring(V.length + 1);
      }
    }
    if (!r && !S && !M.found) {
      const V = j ? e(n, j, M, i) : t(n, M.end, S, null, M, i);
      a.items.push(V), f = V.range[2], ki(j) && i(V.range, "BLOCK_IN_FLOW", Si);
    } else {
      n.atKey = !0;
      const V = M.end, U = L ? e(n, L, M, i) : t(n, V, I, null, M, i);
      ki(L) && i(U.range, "BLOCK_IN_FLOW", Si), n.atKey = !1;
      const ne = cn(S ?? [], {
        flow: l,
        indicator: "map-value-ind",
        next: j,
        offset: U.range[2],
        onError: i,
        parentIndent: s.indent,
        startOnNewline: !1
      });
      if (ne.found) {
        if (!r && !M.found && n.options.strict) {
          if (S)
            for (const ye of S) {
              if (ye === ne.found)
                break;
              if (ye.type === "newline") {
                i(ye, "MULTILINE_IMPLICIT_KEY", "Implicit keys of flow sequence pairs need to be on a single line");
                break;
              }
            }
          M.start < ne.found.offset - 1024 && i(ne.found, "KEY_OVER_1024_CHARS", "The : indicator must be at most 1024 chars after the start of an implicit flow sequence key");
        }
      } else j && ("source" in j && ((y = j.source) == null ? void 0 : y[0]) === ":" ? i(j, "MISSING_CHAR", `Missing space after : in ${l}`) : i(ne.start, "MISSING_CHAR", `Missing , or : between ${l} items`));
      const he = j ? e(n, j, ne, i) : ne.found ? t(n, ne.end, S, null, ne, i) : null;
      he ? ki(j) && i(he.range, "BLOCK_IN_FLOW", Si) : ne.comment && (U.comment ? U.comment += `
` + ne.comment : U.comment = ne.comment);
      const ie = new Oe(U, he);
      if (n.options.keepSourceTokens && (ie.srcToken = E), r) {
        const ye = a;
        Vc(n, ye.items, U) && i(V, "DUPLICATE_KEY", "Map keys must be unique"), ye.items.push(ie);
      } else {
        const ye = new $e(n.schema);
        ye.flow = !0, ye.items.push(ie);
        const yn = (he ?? U).range;
        ye.range = [U.range[0], yn[1], yn[2]], a.items.push(ye);
      }
      f = he ? he.range[2] : ne.end;
    }
  }
  const h = r ? "}" : "]", [m, ...b] = s.end;
  let p = f;
  if ((m == null ? void 0 : m.source) === h)
    p = m.offset + m.source.length;
  else {
    const k = l[0].toUpperCase() + l.substring(1), E = u ? `${k} must end with a ${h}` : `${k} in block collection must be sufficiently indented and end with a ${h}`;
    i(f, u ? "MISSING_CHAR" : "BAD_INDENT", E), m && m.source.length !== 1 && b.unshift(m);
  }
  if (b.length > 0) {
    const k = rs(b, p, n.options.strict, i);
    k.comment && (a.comment ? a.comment += `
` + k.comment : a.comment = k.comment), a.range = [s.offset, p, k.offset];
  } else
    a.range = [s.offset, p, p];
  return a;
}
function Ti(e, t, n, s, i, o) {
  const r = n.type === "block-map" ? jd(e, t, n, s, o) : n.type === "block-seq" ? Rd(e, t, n, s, o) : Fd(e, t, n, s, o), l = r.constructor;
  return i === "!" || i === l.tagName ? (r.tag = l.tagName, r) : (i && (r.tag = i), r);
}
function Bd(e, t, n, s, i) {
  var h;
  const o = s.tag, r = o ? t.directives.tagName(o.source, (m) => i(o, "TAG_RESOLVE_FAILED", m)) : null;
  if (n.type === "block-seq") {
    const { anchor: m, newlineAfterProp: b } = s, p = m && o ? m.offset > o.offset ? m : o : m ?? o;
    p && (!b || b.offset < p.offset) && i(p, "MISSING_CHAR", "Missing newline after block sequence props");
  }
  const l = n.type === "block-map" ? "map" : n.type === "block-seq" ? "seq" : n.start.source === "{" ? "map" : "seq";
  if (!o || !r || r === "!" || r === $e.tagName && l === "map" || r === Bt.tagName && l === "seq")
    return Ti(e, t, n, i, r);
  let c = t.schema.tags.find((m) => m.tag === r && m.collection === l);
  if (!c) {
    const m = t.schema.knownTags[r];
    if ((m == null ? void 0 : m.collection) === l)
      t.schema.tags.push(Object.assign({}, m, { default: !1 })), c = m;
    else
      return m ? i(o, "BAD_COLLECTION_TYPE", `${m.tag} used for ${l} collection, but expects ${m.collection ?? "scalar"}`, !0) : i(o, "TAG_RESOLVE_FAILED", `Unresolved tag: ${r}`, !0), Ti(e, t, n, i, r);
  }
  const a = Ti(e, t, n, i, r, c), u = ((h = c.resolve) == null ? void 0 : h.call(c, a, (m) => i(o, "TAG_RESOLVE_FAILED", m), t.options)) ?? a, f = re(u) ? u : new K(u);
  return f.range = a.range, f.tag = r, c != null && c.format && (f.format = c.format), f;
}
function Kd(e, t, n) {
  const s = t.offset, i = Ud(t, e.options.strict, n);
  if (!i)
    return { value: "", type: null, comment: "", range: [s, s, s] };
  const o = i.mode === ">" ? K.BLOCK_FOLDED : K.BLOCK_LITERAL, r = t.source ? qd(t.source) : [];
  let l = r.length;
  for (let p = r.length - 1; p >= 0; --p) {
    const y = r[p][1];
    if (y === "" || y === "\r")
      l = p;
    else
      break;
  }
  if (l === 0) {
    const p = i.chomp === "+" && r.length > 0 ? `
`.repeat(Math.max(1, r.length - 1)) : "";
    let y = s + i.length;
    return t.source && (y += t.source.length), { value: p, type: o, comment: i.comment, range: [s, y, y] };
  }
  let c = t.indent + i.indent, a = t.offset + i.length, u = 0;
  for (let p = 0; p < l; ++p) {
    const [y, k] = r[p];
    if (k === "" || k === "\r")
      i.indent === 0 && y.length > c && (c = y.length);
    else {
      y.length < c && n(a + y.length, "MISSING_CHAR", "Block scalars with more-indented leading empty lines must use an explicit indentation indicator"), i.indent === 0 && (c = y.length), u = p, c === 0 && !e.atRoot && n(a, "BAD_INDENT", "Block scalar values in collections must be indented");
      break;
    }
    a += y.length + k.length + 1;
  }
  for (let p = r.length - 1; p >= l; --p)
    r[p][0].length > c && (l = p + 1);
  let f = "", h = "", m = !1;
  for (let p = 0; p < u; ++p)
    f += r[p][0].slice(c) + `
`;
  for (let p = u; p < l; ++p) {
    let [y, k] = r[p];
    a += y.length + k.length + 1;
    const E = k[k.length - 1] === "\r";
    if (E && (k = k.slice(0, -1)), k && y.length < c) {
      const L = `Block scalar lines must not be less indented than their ${i.indent ? "explicit indentation indicator" : "first line"}`;
      n(a - k.length - (E ? 2 : 1), "BAD_INDENT", L), y = "";
    }
    o === K.BLOCK_LITERAL ? (f += h + y.slice(c) + k, h = `
`) : y.length > c || k[0] === "	" ? (h === " " ? h = `
` : !m && h === `
` && (h = `

`), f += h + y.slice(c) + k, h = `
`, m = !0) : k === "" ? h === `
` ? f += `
` : h = `
` : (f += h + k, h = " ", m = !1);
  }
  switch (i.chomp) {
    case "-":
      break;
    case "+":
      for (let p = l; p < r.length; ++p)
        f += `
` + r[p][0].slice(c);
      f[f.length - 1] !== `
` && (f += `
`);
      break;
    default:
      f += `
`;
  }
  const b = s + i.length + t.source.length;
  return { value: f, type: o, comment: i.comment, range: [s, b, b] };
}
function Ud({ offset: e, props: t }, n, s) {
  if (t[0].type !== "block-scalar-header")
    return s(t[0], "IMPOSSIBLE", "Block scalar header not found"), null;
  const { source: i } = t[0], o = i[0];
  let r = 0, l = "", c = -1;
  for (let h = 1; h < i.length; ++h) {
    const m = i[h];
    if (!l && (m === "-" || m === "+"))
      l = m;
    else {
      const b = Number(m);
      !r && b ? r = b : c === -1 && (c = e + h);
    }
  }
  c !== -1 && s(c, "UNEXPECTED_TOKEN", `Block scalar header includes extra characters: ${i}`);
  let a = !1, u = "", f = i.length;
  for (let h = 1; h < t.length; ++h) {
    const m = t[h];
    switch (m.type) {
      case "space":
        a = !0;
      // fallthrough
      case "newline":
        f += m.source.length;
        break;
      case "comment":
        n && !a && s(m, "MISSING_CHAR", "Comments must be separated from other tokens by white space characters"), f += m.source.length, u = m.source.substring(1);
        break;
      case "error":
        s(m, "UNEXPECTED_TOKEN", m.message), f += m.source.length;
        break;
      /* istanbul ignore next should not happen */
      default: {
        const b = `Unexpected token in block scalar header: ${m.type}`;
        s(m, "UNEXPECTED_TOKEN", b);
        const p = m.source;
        p && typeof p == "string" && (f += p.length);
      }
    }
  }
  return { mode: o, indent: r, chomp: l, comment: u, length: f };
}
function qd(e) {
  const t = e.split(/\n( *)/), n = t[0], s = n.match(/^( *)/), o = [s != null && s[1] ? [s[1], n.slice(s[1].length)] : ["", n]];
  for (let r = 1; r < t.length; r += 2)
    o.push([t[r], t[r + 1]]);
  return o;
}
function Hd(e, t, n) {
  const { offset: s, type: i, source: o, end: r } = e;
  let l, c;
  const a = (h, m, b) => n(s + h, m, b);
  switch (i) {
    case "scalar":
      l = K.PLAIN, c = Wd(o, a);
      break;
    case "single-quoted-scalar":
      l = K.QUOTE_SINGLE, c = Jd(o, a);
      break;
    case "double-quoted-scalar":
      l = K.QUOTE_DOUBLE, c = Yd(o, a);
      break;
    /* istanbul ignore next should not happen */
    default:
      return n(e, "UNEXPECTED_TOKEN", `Expected a flow scalar value, but found: ${i}`), {
        value: "",
        type: null,
        comment: "",
        range: [s, s + o.length, s + o.length]
      };
  }
  const u = s + o.length, f = rs(r, u, t, n);
  return {
    value: c,
    type: l,
    comment: f.comment,
    range: [s, u, f.offset]
  };
}
function Wd(e, t) {
  let n = "";
  switch (e[0]) {
    /* istanbul ignore next should not happen */
    case "	":
      n = "a tab character";
      break;
    case ",":
      n = "flow indicator character ,";
      break;
    case "%":
      n = "directive indicator character %";
      break;
    case "|":
    case ">": {
      n = `block scalar indicator ${e[0]}`;
      break;
    }
    case "@":
    case "`": {
      n = `reserved character ${e[0]}`;
      break;
    }
  }
  return n && t(0, "BAD_SCALAR_START", `Plain value cannot start with ${n}`), Lc(e);
}
function Jd(e, t) {
  return (e[e.length - 1] !== "'" || e.length === 1) && t(e.length, "MISSING_CHAR", "Missing closing 'quote"), Lc(e.slice(1, -1)).replace(/''/g, "'");
}
function Lc(e) {
  let t, n;
  try {
    t = new RegExp(`(.*?)(?<![ 	])[ 	]*\r?
`, "sy"), n = new RegExp(`[ 	]*(.*?)(?:(?<![ 	])[ 	]*)?\r?
`, "sy");
  } catch {
    t = /(.*?)[ \t]*\r?\n/sy, n = /[ \t]*(.*?)[ \t]*\r?\n/sy;
  }
  let s = t.exec(e);
  if (!s)
    return e;
  let i = s[1], o = " ", r = t.lastIndex;
  for (n.lastIndex = r; s = n.exec(e); )
    s[1] === "" ? o === `
` ? i += o : o = `
` : (i += o + s[1], o = " "), r = n.lastIndex;
  const l = /[ \t]*(.*)/sy;
  return l.lastIndex = r, s = l.exec(e), i + o + ((s == null ? void 0 : s[1]) ?? "");
}
function Yd(e, t) {
  let n = "";
  for (let s = 1; s < e.length - 1; ++s) {
    const i = e[s];
    if (!(i === "\r" && e[s + 1] === `
`))
      if (i === `
`) {
        const { fold: o, offset: r } = Gd(e, s);
        n += o, s = r;
      } else if (i === "\\") {
        let o = e[++s];
        const r = Qd[o];
        if (r)
          n += r;
        else if (o === `
`)
          for (o = e[s + 1]; o === " " || o === "	"; )
            o = e[++s + 1];
        else if (o === "\r" && e[s + 1] === `
`)
          for (o = e[++s + 1]; o === " " || o === "	"; )
            o = e[++s + 1];
        else if (o === "x" || o === "u" || o === "U") {
          const l = o === "x" ? 2 : o === "u" ? 4 : 8;
          n += Xd(e, s + 1, l, t), s += l;
        } else {
          const l = e.substr(s - 1, 2);
          t(s - 1, "BAD_DQ_ESCAPE", `Invalid escape sequence ${l}`), n += l;
        }
      } else if (i === " " || i === "	") {
        const o = s;
        let r = e[s + 1];
        for (; r === " " || r === "	"; )
          r = e[++s + 1];
        r !== `
` && !(r === "\r" && e[s + 2] === `
`) && (n += s > o ? e.slice(o, s + 1) : i);
      } else
        n += i;
  }
  return (e[e.length - 1] !== '"' || e.length === 1) && t(e.length, "MISSING_CHAR", 'Missing closing "quote'), n;
}
function Gd(e, t) {
  let n = "", s = e[t + 1];
  for (; (s === " " || s === "	" || s === `
` || s === "\r") && !(s === "\r" && e[t + 2] !== `
`); )
    s === `
` && (n += `
`), t += 1, s = e[t + 1];
  return n || (n = " "), { fold: n, offset: t };
}
const Qd = {
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
function Xd(e, t, n, s) {
  const i = e.substr(t, n), r = i.length === n && /^[0-9a-fA-F]+$/.test(i) ? parseInt(i, 16) : NaN;
  try {
    return String.fromCodePoint(r);
  } catch {
    const l = e.substr(t - 2, n + 2);
    return s(t - 2, "BAD_DQ_ESCAPE", `Invalid escape sequence ${l}`), l;
  }
}
function Mc(e, t, n, s) {
  const { value: i, type: o, comment: r, range: l } = t.type === "block-scalar" ? Kd(e, t, s) : Hd(t, e.options.strict, s), c = n ? e.directives.tagName(n.source, (f) => s(n, "TAG_RESOLVE_FAILED", f)) : null;
  let a;
  e.options.stringKeys && e.atKey ? a = e.schema[rt] : c ? a = zd(e.schema, i, c, n, s) : t.type === "scalar" ? a = Zd(e, i, t, s) : a = e.schema[rt];
  let u;
  try {
    const f = a.resolve(i, (h) => s(n ?? t, "TAG_RESOLVE_FAILED", h), e.options);
    u = te(f) ? f : new K(f);
  } catch (f) {
    const h = f instanceof Error ? f.message : String(f);
    s(n ?? t, "TAG_RESOLVE_FAILED", h), u = new K(i);
  }
  return u.range = l, u.source = i, o && (u.type = o), c && (u.tag = c), a.format && (u.format = a.format), r && (u.comment = r), u;
}
function zd(e, t, n, s, i) {
  var l;
  if (n === "!")
    return e[rt];
  const o = [];
  for (const c of e.tags)
    if (!c.collection && c.tag === n)
      if (c.default && c.test)
        o.push(c);
      else
        return c;
  for (const c of o)
    if ((l = c.test) != null && l.test(t))
      return c;
  const r = e.knownTags[n];
  return r && !r.collection ? (e.tags.push(Object.assign({}, r, { default: !1, test: void 0 })), r) : (i(s, "TAG_RESOLVE_FAILED", `Unresolved tag: ${n}`, n !== "tag:yaml.org,2002:str"), e[rt]);
}
function Zd({ atKey: e, directives: t, schema: n }, s, i, o) {
  const r = n.tags.find((l) => {
    var c;
    return (l.default === !0 || e && l.default === "key") && ((c = l.test) == null ? void 0 : c.test(s));
  }) || n[rt];
  if (n.compat) {
    const l = n.compat.find((c) => {
      var a;
      return c.default && ((a = c.test) == null ? void 0 : a.test(s));
    }) ?? n[rt];
    if (r.tag !== l.tag) {
      const c = t.tagString(r.tag), a = t.tagString(l.tag), u = `Value may be parsed as either ${c} or ${a}`;
      o(i, "TAG_RESOLVE_FAILED", u, !0);
    }
  }
  return r;
}
function ep(e, t, n) {
  if (t) {
    n ?? (n = t.length);
    for (let s = n - 1; s >= 0; --s) {
      let i = t[s];
      switch (i.type) {
        case "space":
        case "comment":
        case "newline":
          e -= i.source.length;
          continue;
      }
      for (i = t[++s]; (i == null ? void 0 : i.type) === "space"; )
        e += i.source.length, i = t[++s];
      break;
    }
  }
  return e;
}
const tp = { composeNode: xc, composeEmptyNode: Vo };
function xc(e, t, n, s) {
  const i = e.atKey, { spaceBefore: o, comment: r, anchor: l, tag: c } = n;
  let a, u = !0;
  switch (t.type) {
    case "alias":
      a = np(e, t, s), (l || c) && s(t, "ALIAS_PROPS", "An alias node must not specify any properties");
      break;
    case "scalar":
    case "single-quoted-scalar":
    case "double-quoted-scalar":
    case "block-scalar":
      a = Mc(e, t, c, s), l && (a.anchor = l.source.substring(1));
      break;
    case "block-map":
    case "block-seq":
    case "flow-collection":
      try {
        a = Bd(tp, e, t, n, s), l && (a.anchor = l.source.substring(1));
      } catch (f) {
        const h = f instanceof Error ? f.message : String(f);
        s(t, "RESOURCE_EXHAUSTION", h);
      }
      break;
    default: {
      const f = t.type === "error" ? t.message : `Unsupported token (type: ${t.type})`;
      s(t, "UNEXPECTED_TOKEN", f), u = !1;
    }
  }
  return a ?? (a = Vo(e, t.offset, void 0, null, n, s)), l && a.anchor === "" && s(l, "BAD_ALIAS", "Anchor cannot be an empty string"), i && e.options.stringKeys && (!te(a) || typeof a.value != "string" || a.tag && a.tag !== "tag:yaml.org,2002:str") && s(c ?? t, "NON_STRING_KEY", "With stringKeys, all keys must be strings"), o && (a.spaceBefore = !0), r && (t.type === "scalar" && t.source === "" ? a.comment = r : a.commentBefore = r), e.options.keepSourceTokens && u && (a.srcToken = t), a;
}
function Vo(e, t, n, s, { spaceBefore: i, comment: o, anchor: r, tag: l, end: c }, a) {
  const u = {
    type: "scalar",
    offset: ep(t, n, s),
    indent: -1,
    source: ""
  }, f = Mc(e, u, l, a);
  return r && (f.anchor = r.source.substring(1), f.anchor === "" && a(r, "BAD_ALIAS", "Anchor cannot be an empty string")), i && (f.spaceBefore = !0), o && (f.comment = o, f.range[2] = c), f;
}
function np({ options: e }, { offset: t, source: n, end: s }, i) {
  const o = new wo(n.substring(1));
  o.source === "" && i(t, "BAD_ALIAS", "Alias cannot be an empty string"), o.source.endsWith(":") && i(t + n.length - 1, "BAD_ALIAS", "Alias ending in : is ambiguous", !0);
  const r = t + n.length, l = rs(s, r, e.strict, i);
  return o.range = [t, r, l.offset], l.comment && (o.comment = l.comment), o;
}
function sp(e, t, { offset: n, start: s, value: i, end: o }, r) {
  const l = Object.assign({ _directives: t }, e), c = new $o(void 0, l), a = {
    atKey: !1,
    atRoot: !0,
    directives: c.directives,
    options: c.options,
    schema: c.schema
  }, u = cn(s, {
    indicator: "doc-start",
    next: i ?? (o == null ? void 0 : o[0]),
    offset: n,
    onError: r,
    parentIndent: 0,
    startOnNewline: !0
  });
  u.found && (c.directives.docStart = !0, i && (i.type === "block-map" || i.type === "block-seq") && !u.hasNewline && r(u.end, "MISSING_CHAR", "Block collection cannot start on same line with directives-end marker")), c.contents = i ? xc(a, i, u, r) : Vo(a, u.end, s, null, u, r);
  const f = c.contents.range[2], h = rs(o, f, !1, r);
  return h.comment && (c.comment = h.comment), c.range = [n, f, h.offset], c;
}
function Sn(e) {
  if (typeof e == "number")
    return [e, e + 1];
  if (Array.isArray(e))
    return e.length === 2 ? e : [e[0], e[1]];
  const { offset: t, source: n } = e;
  return [t, t + (typeof n == "string" ? n.length : 1)];
}
function Mr(e) {
  var i;
  let t = "", n = !1, s = !1;
  for (let o = 0; o < e.length; ++o) {
    const r = e[o];
    switch (r[0]) {
      case "#":
        t += (t === "" ? "" : s ? `

` : `
`) + (r.substring(1) || " "), n = !0, s = !1;
        break;
      case "%":
        ((i = e[o + 1]) == null ? void 0 : i[0]) !== "#" && (o += 1), n = !1;
        break;
      default:
        n || (s = !0), n = !1;
    }
  }
  return { comment: t, afterEmptyLine: s };
}
class ip {
  constructor(t = {}) {
    this.doc = null, this.atDirectives = !1, this.prelude = [], this.errors = [], this.warnings = [], this.onError = (n, s, i, o) => {
      const r = Sn(n);
      o ? this.warnings.push(new Pd(r, s, i)) : this.errors.push(new An(r, s, i));
    }, this.directives = new we({ version: t.version || "1.2" }), this.options = t;
  }
  decorate(t, n) {
    const { comment: s, afterEmptyLine: i } = Mr(this.prelude);
    if (s) {
      const o = t.contents;
      if (n)
        t.comment = t.comment ? `${t.comment}
${s}` : s;
      else if (i || t.directives.docStart || !o)
        t.commentBefore = s;
      else if (oe(o) && !o.flow && o.items.length > 0) {
        let r = o.items[0];
        ce(r) && (r = r.key);
        const l = r.commentBefore;
        r.commentBefore = l ? `${s}
${l}` : s;
      } else {
        const r = o.commentBefore;
        o.commentBefore = r ? `${s}
${r}` : s;
      }
    }
    if (n) {
      for (let o = 0; o < this.errors.length; ++o)
        t.errors.push(this.errors[o]);
      for (let o = 0; o < this.warnings.length; ++o)
        t.warnings.push(this.warnings[o]);
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
      comment: Mr(this.prelude).comment,
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
  *compose(t, n = !1, s = -1) {
    for (const i of t)
      yield* this.next(i);
    yield* this.end(n, s);
  }
  /** Advance the composer by one CST token. */
  *next(t) {
    switch (t.type) {
      case "directive":
        this.directives.add(t.source, (n, s, i) => {
          const o = Sn(t);
          o[0] += n, this.onError(o, "BAD_DIRECTIVE", s, i);
        }), this.prelude.push(t.source), this.atDirectives = !0;
        break;
      case "document": {
        const n = sp(this.options, this.directives, t, this.onError);
        this.atDirectives && !n.directives.docStart && this.onError(t, "MISSING_CHAR", "Missing directives-end/doc-start indicator line"), this.decorate(n, !1), this.doc && (yield this.doc), this.doc = n, this.atDirectives = !1;
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
        const n = t.source ? `${t.message}: ${JSON.stringify(t.source)}` : t.message, s = new An(Sn(t), "UNEXPECTED_TOKEN", n);
        this.atDirectives || !this.doc ? this.errors.push(s) : this.doc.errors.push(s);
        break;
      }
      case "doc-end": {
        if (!this.doc) {
          const s = "Unexpected doc-end without preceding document";
          this.errors.push(new An(Sn(t), "UNEXPECTED_TOKEN", s));
          break;
        }
        this.doc.directives.docEnd = !0;
        const n = rs(t.end, t.offset + t.source.length, this.doc.options.strict, this.onError);
        if (this.decorate(this.doc, !0), n.comment) {
          const s = this.doc.comment;
          this.doc.comment = s ? `${s}
${n.comment}` : n.comment;
        }
        this.doc.range[2] = n.offset;
        break;
      }
      default:
        this.errors.push(new An(Sn(t), "UNEXPECTED_TOKEN", `Unsupported token ${t.type}`));
    }
  }
  /**
   * Call at end of input to yield any remaining document.
   *
   * @param forceDoc - If the stream contains no document, still emit a final document including any comments and directives that would be applied to a subsequent document.
   * @param endOffset - Should be set if `forceDoc` is also set, to set the document range end and to indicate errors correctly.
   */
  *end(t = !1, n = -1) {
    if (this.doc)
      this.decorate(this.doc, !0), yield this.doc, this.doc = null;
    else if (t) {
      const s = Object.assign({ _directives: this.directives }, this.options), i = new $o(void 0, s);
      this.atDirectives && this.onError(n, "MISSING_CHAR", "Missing directives-end indicator line"), i.range = [0, n, n], this.decorate(i, !1), yield i;
    }
  }
}
const Pc = "\uFEFF", jc = "", Rc = "", Gi = "";
function op(e) {
  switch (e) {
    case Pc:
      return "byte-order-mark";
    case jc:
      return "doc-mode";
    case Rc:
      return "flow-error-end";
    case Gi:
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
function je(e) {
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
const xr = new Set("0123456789ABCDEFabcdef"), rp = new Set("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-#;/?:@&=+$_.!~*'()"), ms = new Set(",[]{}"), lp = new Set(` ,[]{}
\r	`), Di = (e) => !e || lp.has(e);
class cp {
  constructor() {
    this.atEnd = !1, this.blockScalarIndent = -1, this.blockScalarKeep = !1, this.buffer = "", this.flowKey = !1, this.flowLevel = 0, this.indentNext = 0, this.indentValue = 0, this.lineEndPos = null, this.next = null, this.pos = 0;
  }
  /**
   * Generate YAML tokens from the `source` string. If `incomplete`,
   * a part of the last line may be left as a buffer for the next call.
   *
   * @returns A generator of lexical tokens
   */
  *lex(t, n = !1) {
    if (t) {
      if (typeof t != "string")
        throw TypeError("source is not a string");
      this.buffer = this.buffer ? this.buffer + t : t, this.lineEndPos = null;
    }
    this.atEnd = !n;
    let s = this.next ?? "stream";
    for (; s && (n || this.hasChars(1)); )
      s = yield* this.parseNext(s);
  }
  atLineEnd() {
    let t = this.pos, n = this.buffer[t];
    for (; n === " " || n === "	"; )
      n = this.buffer[++t];
    return !n || n === "#" || n === `
` ? !0 : n === "\r" ? this.buffer[t + 1] === `
` : !1;
  }
  charAt(t) {
    return this.buffer[this.pos + t];
  }
  continueScalar(t) {
    let n = this.buffer[t];
    if (this.indentNext > 0) {
      let s = 0;
      for (; n === " "; )
        n = this.buffer[++s + t];
      if (n === "\r") {
        const i = this.buffer[s + t + 1];
        if (i === `
` || !i && !this.atEnd)
          return t + s + 1;
      }
      return n === `
` || s >= this.indentNext || !n && !this.atEnd ? t + s : -1;
    }
    if (n === "-" || n === ".") {
      const s = this.buffer.substr(t, 3);
      if ((s === "---" || s === "...") && je(this.buffer[t + 3]))
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
    if (t[0] === Pc && (yield* this.pushCount(1), t = t.substring(1)), t[0] === "%") {
      let n = t.length, s = t.indexOf("#");
      for (; s !== -1; ) {
        const o = t[s - 1];
        if (o === " " || o === "	") {
          n = s - 1;
          break;
        } else
          s = t.indexOf("#", s + 1);
      }
      for (; ; ) {
        const o = t[n - 1];
        if (o === " " || o === "	")
          n -= 1;
        else
          break;
      }
      const i = (yield* this.pushCount(n)) + (yield* this.pushSpaces(!0));
      return yield* this.pushCount(t.length - i), this.pushNewline(), "stream";
    }
    if (this.atLineEnd()) {
      const n = yield* this.pushSpaces(!0);
      return yield* this.pushCount(t.length - n), yield* this.pushNewline(), "stream";
    }
    return yield jc, yield* this.parseLineStart();
  }
  *parseLineStart() {
    const t = this.charAt(0);
    if (!t && !this.atEnd)
      return this.setNext("line-start");
    if (t === "-" || t === ".") {
      if (!this.atEnd && !this.hasChars(4))
        return this.setNext("line-start");
      const n = this.peek(3);
      if ((n === "---" || n === "...") && je(this.charAt(3)))
        return yield* this.pushCount(3), this.indentValue = 0, this.indentNext = 0, n === "---" ? "doc" : "stream";
    }
    return this.indentValue = yield* this.pushSpaces(!1), this.indentNext > this.indentValue && !je(this.charAt(1)) && (this.indentNext = this.indentValue), yield* this.parseBlockStart();
  }
  *parseBlockStart() {
    const [t, n] = this.peek(2);
    if (!n && !this.atEnd)
      return this.setNext("block-start");
    if ((t === "-" || t === "?" || t === ":") && je(n)) {
      const s = (yield* this.pushCount(1)) + (yield* this.pushSpaces(!0));
      return this.indentNext = this.indentValue + 1, this.indentValue += s, "block-start";
    }
    return "doc";
  }
  *parseDocument() {
    yield* this.pushSpaces(!0);
    const t = this.getLine();
    if (t === null)
      return this.setNext("doc");
    let n = yield* this.pushIndicators();
    switch (t[n]) {
      case "#":
        yield* this.pushCount(t.length - n);
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
        return yield* this.pushUntil(Di), "doc";
      case '"':
      case "'":
        return yield* this.parseQuotedScalar();
      case "|":
      case ">":
        return n += yield* this.parseBlockScalarHeader(), n += yield* this.pushSpaces(!0), yield* this.pushCount(t.length - n), yield* this.pushNewline(), yield* this.parseBlockScalar();
      default:
        return yield* this.parsePlainScalar();
    }
  }
  *parseFlowCollection() {
    let t, n, s = -1;
    do
      t = yield* this.pushNewline(), t > 0 ? (n = yield* this.pushSpaces(!1), this.indentValue = s = n) : n = 0, n += yield* this.pushSpaces(!0);
    while (t + n > 0);
    const i = this.getLine();
    if (i === null)
      return this.setNext("flow");
    if ((s !== -1 && s < this.indentNext && i[0] !== "#" || s === 0 && (i.startsWith("---") || i.startsWith("...")) && je(i[3])) && !(s === this.indentNext - 1 && this.flowLevel === 1 && (i[0] === "]" || i[0] === "}")))
      return this.flowLevel = 0, yield Rc, yield* this.parseLineStart();
    let o = 0;
    for (; i[o] === ","; )
      o += yield* this.pushCount(1), o += yield* this.pushSpaces(!0), this.flowKey = !1;
    switch (o += yield* this.pushIndicators(), i[o]) {
      case void 0:
        return "flow";
      case "#":
        return yield* this.pushCount(i.length - o), "flow";
      case "{":
      case "[":
        return yield* this.pushCount(1), this.flowKey = !1, this.flowLevel += 1, "flow";
      case "}":
      case "]":
        return yield* this.pushCount(1), this.flowKey = !0, this.flowLevel -= 1, this.flowLevel ? "flow" : "doc";
      case "*":
        return yield* this.pushUntil(Di), "flow";
      case '"':
      case "'":
        return this.flowKey = !0, yield* this.parseQuotedScalar();
      case ":": {
        const r = this.charAt(1);
        if (this.flowKey || je(r) || r === ",")
          return this.flowKey = !1, yield* this.pushCount(1), yield* this.pushSpaces(!0), "flow";
      }
      // fallthrough
      default:
        return this.flowKey = !1, yield* this.parsePlainScalar();
    }
  }
  *parseQuotedScalar() {
    const t = this.charAt(0);
    let n = this.buffer.indexOf(t, this.pos + 1);
    if (t === "'")
      for (; n !== -1 && this.buffer[n + 1] === "'"; )
        n = this.buffer.indexOf("'", n + 2);
    else
      for (; n !== -1; ) {
        let o = 0;
        for (; this.buffer[n - 1 - o] === "\\"; )
          o += 1;
        if (o % 2 === 0)
          break;
        n = this.buffer.indexOf('"', n + 1);
      }
    const s = this.buffer.substring(0, n);
    let i = s.indexOf(`
`, this.pos);
    if (i !== -1) {
      for (; i !== -1; ) {
        const o = this.continueScalar(i + 1);
        if (o === -1)
          break;
        i = s.indexOf(`
`, o);
      }
      i !== -1 && (n = i - (s[i - 1] === "\r" ? 2 : 1));
    }
    if (n === -1) {
      if (!this.atEnd)
        return this.setNext("quoted-scalar");
      n = this.buffer.length;
    }
    return yield* this.pushToIndex(n + 1, !1), this.flowLevel ? "flow" : "doc";
  }
  *parseBlockScalarHeader() {
    this.blockScalarIndent = -1, this.blockScalarKeep = !1;
    let t = this.pos;
    for (; ; ) {
      const n = this.buffer[++t];
      if (n === "+")
        this.blockScalarKeep = !0;
      else if (n > "0" && n <= "9")
        this.blockScalarIndent = Number(n) - 1;
      else if (n !== "-")
        break;
    }
    return yield* this.pushUntil((n) => je(n) || n === "#");
  }
  *parseBlockScalar() {
    let t = this.pos - 1, n = 0, s;
    e: for (let o = this.pos; s = this.buffer[o]; ++o)
      switch (s) {
        case " ":
          n += 1;
          break;
        case `
`:
          t = o, n = 0;
          break;
        case "\r": {
          const r = this.buffer[o + 1];
          if (!r && !this.atEnd)
            return this.setNext("block-scalar");
          if (r === `
`)
            break;
        }
        // fallthrough
        default:
          break e;
      }
    if (!s && !this.atEnd)
      return this.setNext("block-scalar");
    if (n >= this.indentNext) {
      this.blockScalarIndent === -1 ? this.indentNext = n : this.indentNext = this.blockScalarIndent + (this.indentNext === 0 ? 1 : this.indentNext);
      do {
        const o = this.continueScalar(t + 1);
        if (o === -1)
          break;
        t = this.buffer.indexOf(`
`, o);
      } while (t !== -1);
      if (t === -1) {
        if (!this.atEnd)
          return this.setNext("block-scalar");
        t = this.buffer.length;
      }
    }
    let i = t + 1;
    for (s = this.buffer[i]; s === " "; )
      s = this.buffer[++i];
    if (s === "	") {
      for (; s === "	" || s === " " || s === "\r" || s === `
`; )
        s = this.buffer[++i];
      t = i - 1;
    } else if (!this.blockScalarKeep)
      do {
        let o = t - 1, r = this.buffer[o];
        r === "\r" && (r = this.buffer[--o]);
        const l = o;
        for (; r === " "; )
          r = this.buffer[--o];
        if (r === `
` && o >= this.pos && o + 1 + n > l)
          t = o;
        else
          break;
      } while (!0);
    return yield Gi, yield* this.pushToIndex(t + 1, !0), yield* this.parseLineStart();
  }
  *parsePlainScalar() {
    const t = this.flowLevel > 0;
    let n = this.pos - 1, s = this.pos - 1, i;
    for (; i = this.buffer[++s]; )
      if (i === ":") {
        const o = this.buffer[s + 1];
        if (je(o) || t && ms.has(o))
          break;
        n = s;
      } else if (je(i)) {
        let o = this.buffer[s + 1];
        if (i === "\r" && (o === `
` ? (s += 1, i = `
`, o = this.buffer[s + 1]) : n = s), o === "#" || t && ms.has(o))
          break;
        if (i === `
`) {
          const r = this.continueScalar(s + 1);
          if (r === -1)
            break;
          s = Math.max(s, r - 2);
        }
      } else {
        if (t && ms.has(i))
          break;
        n = s;
      }
    return !i && !this.atEnd ? this.setNext("plain-scalar") : (yield Gi, yield* this.pushToIndex(n + 1, !0), t ? "flow" : "doc");
  }
  *pushCount(t) {
    return t > 0 ? (yield this.buffer.substr(this.pos, t), this.pos += t, t) : 0;
  }
  *pushToIndex(t, n) {
    const s = this.buffer.slice(this.pos, t);
    return s ? (yield s, this.pos += s.length, s.length) : (n && (yield ""), 0);
  }
  *pushIndicators() {
    let t = 0;
    e: for (; ; ) {
      switch (this.charAt(0)) {
        case "!":
          t += yield* this.pushTag(), t += yield* this.pushSpaces(!0);
          continue e;
        case "&":
          t += yield* this.pushUntil(Di), t += yield* this.pushSpaces(!0);
          continue e;
        case "-":
        // this is an error
        case "?":
        // this is an error outside flow collections
        case ":": {
          const n = this.flowLevel > 0, s = this.charAt(1);
          if (je(s) || n && ms.has(s)) {
            n ? this.flowKey && (this.flowKey = !1) : this.indentNext = this.indentValue + 1, t += yield* this.pushCount(1), t += yield* this.pushSpaces(!0);
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
      let t = this.pos + 2, n = this.buffer[t];
      for (; !je(n) && n !== ">"; )
        n = this.buffer[++t];
      return yield* this.pushToIndex(n === ">" ? t + 1 : t, !1);
    } else {
      let t = this.pos + 1, n = this.buffer[t];
      for (; n; )
        if (rp.has(n))
          n = this.buffer[++t];
        else if (n === "%" && xr.has(this.buffer[t + 1]) && xr.has(this.buffer[t + 2]))
          n = this.buffer[t += 3];
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
    let n = this.pos - 1, s;
    do
      s = this.buffer[++n];
    while (s === " " || t && s === "	");
    const i = n - this.pos;
    return i > 0 && (yield this.buffer.substr(this.pos, i), this.pos = n), i;
  }
  *pushUntil(t) {
    let n = this.pos, s = this.buffer[n];
    for (; !t(s); )
      s = this.buffer[++n];
    return yield* this.pushToIndex(n, !1);
  }
}
class ap {
  constructor() {
    this.lineStarts = [], this.addNewLine = (t) => this.lineStarts.push(t), this.linePos = (t) => {
      let n = 0, s = this.lineStarts.length;
      for (; n < s; ) {
        const o = n + s >> 1;
        this.lineStarts[o] < t ? n = o + 1 : s = o;
      }
      if (this.lineStarts[n] === t)
        return { line: n + 1, col: 1 };
      if (n === 0)
        return { line: 0, col: t };
      const i = this.lineStarts[n - 1];
      return { line: n, col: t - i + 1 };
    };
  }
}
function Nt(e, t) {
  for (let n = 0; n < e.length; ++n)
    if (e[n].type === t)
      return !0;
  return !1;
}
function Pr(e) {
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
function Fc(e) {
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
function gs(e) {
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
function Jt(e) {
  var n;
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
  for (; ((n = e[++t]) == null ? void 0 : n.type) === "space"; )
    ;
  return e.splice(t, e.length);
}
function Bs(e, t) {
  if (t.length < 1e5)
    Array.prototype.push.apply(e, t);
  else
    for (let n = 0; n < t.length; ++n)
      e.push(t[n]);
}
function jr(e) {
  if (e.start.type === "flow-seq-start")
    for (const t of e.items)
      t.sep && !t.value && !Nt(t.start, "explicit-key-ind") && !Nt(t.sep, "map-value-ind") && (t.key && (t.value = t.key), delete t.key, Fc(t.value) ? t.value.end ? Bs(t.value.end, t.sep) : t.value.end = t.sep : Bs(t.start, t.sep), delete t.sep);
}
class fp {
  /**
   * @param onNewLine - If defined, called separately with the start position of
   *   each new line (in `parse()`, including the start of input).
   */
  constructor(t) {
    this.atNewLine = !0, this.atScalar = !1, this.indent = 0, this.offset = 0, this.onKeyLine = !1, this.stack = [], this.source = "", this.type = "", this.lexer = new cp(), this.onNewLine = t;
  }
  /**
   * Parse `source` as a YAML stream.
   * If `incomplete`, a part of the last line may be left as a buffer for the next call.
   *
   * Errors are not thrown, but yielded as `{ type: 'error', message }` tokens.
   *
   * @returns A generator of tokens representing each directive, document, and other structure.
   */
  *parse(t, n = !1) {
    this.onNewLine && this.offset === 0 && this.onNewLine(0);
    for (const s of this.lexer.lex(t, n))
      yield* this.next(s);
    n || (yield* this.end());
  }
  /**
   * Advance the parser by the `source` of one lexical token.
   */
  *next(t) {
    if (this.source = t, this.atScalar) {
      this.atScalar = !1, yield* this.step(), this.offset += t.length;
      return;
    }
    const n = op(t);
    if (n)
      if (n === "scalar")
        this.atNewLine = !1, this.atScalar = !0, this.type = "scalar";
      else {
        switch (this.type = n, yield* this.step(), n) {
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
      const s = `Not a YAML token: ${t}`;
      yield* this.pop({ type: "error", offset: this.offset, message: s, source: t }), this.offset += t.length;
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
    const n = t ?? this.stack.pop();
    if (!n)
      yield { type: "error", offset: this.offset, source: "", message: "Tried to pop an empty stack" };
    else if (this.stack.length === 0)
      yield n;
    else {
      const s = this.peek(1);
      switch (n.type === "block-scalar" ? n.indent = "indent" in s ? s.indent : 0 : n.type === "flow-collection" && s.type === "document" && (n.indent = 0), n.type === "flow-collection" && jr(n), s.type) {
        case "document":
          s.value = n;
          break;
        case "block-scalar":
          s.props.push(n);
          break;
        case "block-map": {
          const i = s.items[s.items.length - 1];
          if (i.value) {
            s.items.push({ start: [], key: n, sep: [] }), this.onKeyLine = !0;
            return;
          } else if (i.sep)
            i.value = n;
          else {
            Object.assign(i, { key: n, sep: [] }), this.onKeyLine = !i.explicitKey;
            return;
          }
          break;
        }
        case "block-seq": {
          const i = s.items[s.items.length - 1];
          i.value ? s.items.push({ start: [], value: n }) : i.value = n;
          break;
        }
        case "flow-collection": {
          const i = s.items[s.items.length - 1];
          !i || i.value ? s.items.push({ start: [], key: n, sep: [] }) : i.sep ? i.value = n : Object.assign(i, { key: n, sep: [] });
          return;
        }
        /* istanbul ignore next should not happen */
        default:
          yield* this.pop(), yield* this.pop(n);
      }
      if ((s.type === "document" || s.type === "block-map" || s.type === "block-seq") && (n.type === "block-map" || n.type === "block-seq")) {
        const i = n.items[n.items.length - 1];
        i && !i.sep && !i.value && i.start.length > 0 && Pr(i.start) === -1 && (n.indent === 0 || i.start.every((o) => o.type !== "comment" || o.indent < n.indent)) && (s.type === "document" ? s.end = i.start : s.items.push({ start: i.start }), n.items.splice(-1, 1));
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
        Pr(t.start) !== -1 ? (yield* this.pop(), yield* this.step()) : t.start.push(this.sourceToken);
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
    const n = this.startBlockValue(t);
    n ? this.stack.push(n) : yield {
      type: "error",
      offset: this.offset,
      message: `Unexpected ${this.type} token in YAML document`,
      source: this.source
    };
  }
  *scalar(t) {
    if (this.type === "map-value-ind") {
      const n = gs(this.peek(2)), s = Jt(n);
      let i;
      t.end ? (i = t.end, i.push(this.sourceToken), delete t.end) : i = [this.sourceToken];
      const o = {
        type: "block-map",
        offset: t.offset,
        indent: t.indent,
        items: [{ start: s, key: t, sep: i }]
      };
      this.onKeyLine = !0, this.stack[this.stack.length - 1] = o;
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
          let n = this.source.indexOf(`
`) + 1;
          for (; n !== 0; )
            this.onNewLine(this.offset + n), n = this.source.indexOf(`
`, n) + 1;
        }
        yield* this.pop();
        break;
      /* istanbul ignore next should not happen */
      default:
        yield* this.pop(), yield* this.step();
    }
  }
  *blockMap(t) {
    var s;
    const n = t.items[t.items.length - 1];
    switch (this.type) {
      case "newline":
        if (this.onKeyLine = !1, n.value) {
          const i = "end" in n.value ? n.value.end : void 0, o = Array.isArray(i) ? i[i.length - 1] : void 0;
          (o == null ? void 0 : o.type) === "comment" ? i == null || i.push(this.sourceToken) : t.items.push({ start: [this.sourceToken] });
        } else n.sep ? n.sep.push(this.sourceToken) : n.start.push(this.sourceToken);
        return;
      case "space":
      case "comment":
        if (n.value)
          t.items.push({ start: [this.sourceToken] });
        else if (n.sep)
          n.sep.push(this.sourceToken);
        else {
          if (this.atIndentedComment(n.start, t.indent)) {
            const i = t.items[t.items.length - 2], o = (s = i == null ? void 0 : i.value) == null ? void 0 : s.end;
            if (Array.isArray(o)) {
              Bs(o, n.start), o.push(this.sourceToken), t.items.pop();
              return;
            }
          }
          n.start.push(this.sourceToken);
        }
        return;
    }
    if (this.indent >= t.indent) {
      const i = !this.onKeyLine && this.indent === t.indent, o = i && (n.sep || n.explicitKey) && this.type !== "seq-item-ind";
      let r = [];
      if (o && n.sep && !n.value) {
        const l = [];
        for (let c = 0; c < n.sep.length; ++c) {
          const a = n.sep[c];
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
        l.length >= 2 && (r = n.sep.splice(l[1]));
      }
      switch (this.type) {
        case "anchor":
        case "tag":
          o || n.value ? (r.push(this.sourceToken), t.items.push({ start: r }), this.onKeyLine = !0) : n.sep ? n.sep.push(this.sourceToken) : n.start.push(this.sourceToken);
          return;
        case "explicit-key-ind":
          !n.sep && !n.explicitKey ? (n.start.push(this.sourceToken), n.explicitKey = !0) : o || n.value ? (r.push(this.sourceToken), t.items.push({ start: r, explicitKey: !0 })) : this.stack.push({
            type: "block-map",
            offset: this.offset,
            indent: this.indent,
            items: [{ start: [this.sourceToken], explicitKey: !0 }]
          }), this.onKeyLine = !0;
          return;
        case "map-value-ind":
          if (n.explicitKey)
            if (n.sep)
              if (n.value)
                t.items.push({ start: [], key: null, sep: [this.sourceToken] });
              else if (Nt(n.sep, "map-value-ind"))
                this.stack.push({
                  type: "block-map",
                  offset: this.offset,
                  indent: this.indent,
                  items: [{ start: r, key: null, sep: [this.sourceToken] }]
                });
              else if (Fc(n.key) && !Nt(n.sep, "newline")) {
                const l = Jt(n.start), c = n.key, a = n.sep;
                a.push(this.sourceToken), delete n.key, delete n.sep, this.stack.push({
                  type: "block-map",
                  offset: this.offset,
                  indent: this.indent,
                  items: [{ start: l, key: c, sep: a }]
                });
              } else r.length > 0 ? n.sep = n.sep.concat(r, this.sourceToken) : n.sep.push(this.sourceToken);
            else if (Nt(n.start, "newline"))
              Object.assign(n, { key: null, sep: [this.sourceToken] });
            else {
              const l = Jt(n.start);
              this.stack.push({
                type: "block-map",
                offset: this.offset,
                indent: this.indent,
                items: [{ start: l, key: null, sep: [this.sourceToken] }]
              });
            }
          else
            n.sep ? n.value || o ? t.items.push({ start: r, key: null, sep: [this.sourceToken] }) : Nt(n.sep, "map-value-ind") ? this.stack.push({
              type: "block-map",
              offset: this.offset,
              indent: this.indent,
              items: [{ start: [], key: null, sep: [this.sourceToken] }]
            }) : n.sep.push(this.sourceToken) : Object.assign(n, { key: null, sep: [this.sourceToken] });
          this.onKeyLine = !0;
          return;
        case "alias":
        case "scalar":
        case "single-quoted-scalar":
        case "double-quoted-scalar": {
          const l = this.flowScalar(this.type);
          o || n.value ? (t.items.push({ start: r, key: l, sep: [] }), this.onKeyLine = !0) : n.sep ? this.stack.push(l) : (Object.assign(n, { key: l, sep: [] }), this.onKeyLine = !0);
          return;
        }
        default: {
          const l = this.startBlockValue(t);
          if (l) {
            if (l.type === "block-seq") {
              if (!n.explicitKey && n.sep && !Nt(n.sep, "newline")) {
                yield* this.pop({
                  type: "error",
                  offset: this.offset,
                  message: "Unexpected block-seq-ind on same line with key",
                  source: this.source
                });
                return;
              }
            } else i && t.items.push({ start: r });
            this.stack.push(l);
            return;
          }
        }
      }
    }
    yield* this.pop(), yield* this.step();
  }
  *blockSequence(t) {
    var s;
    const n = t.items[t.items.length - 1];
    switch (this.type) {
      case "newline":
        if (n.value) {
          const i = "end" in n.value ? n.value.end : void 0, o = Array.isArray(i) ? i[i.length - 1] : void 0;
          (o == null ? void 0 : o.type) === "comment" ? i == null || i.push(this.sourceToken) : t.items.push({ start: [this.sourceToken] });
        } else
          n.start.push(this.sourceToken);
        return;
      case "space":
      case "comment":
        if (n.value)
          t.items.push({ start: [this.sourceToken] });
        else {
          if (this.atIndentedComment(n.start, t.indent)) {
            const i = t.items[t.items.length - 2], o = (s = i == null ? void 0 : i.value) == null ? void 0 : s.end;
            if (Array.isArray(o)) {
              Bs(o, n.start), o.push(this.sourceToken), t.items.pop();
              return;
            }
          }
          n.start.push(this.sourceToken);
        }
        return;
      case "anchor":
      case "tag":
        if (n.value || this.indent <= t.indent)
          break;
        n.start.push(this.sourceToken);
        return;
      case "seq-item-ind":
        if (this.indent !== t.indent)
          break;
        n.value || Nt(n.start, "seq-item-ind") ? t.items.push({ start: [this.sourceToken] }) : n.start.push(this.sourceToken);
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
    const n = t.items[t.items.length - 1];
    if (this.type === "flow-error-end") {
      let s;
      do
        yield* this.pop(), s = this.peek(1);
      while ((s == null ? void 0 : s.type) === "flow-collection");
    } else if (t.end.length === 0) {
      switch (this.type) {
        case "comma":
        case "explicit-key-ind":
          !n || n.sep ? t.items.push({ start: [this.sourceToken] }) : n.start.push(this.sourceToken);
          return;
        case "map-value-ind":
          !n || n.value ? t.items.push({ start: [], key: null, sep: [this.sourceToken] }) : n.sep ? n.sep.push(this.sourceToken) : Object.assign(n, { key: null, sep: [this.sourceToken] });
          return;
        case "space":
        case "comment":
        case "newline":
        case "anchor":
        case "tag":
          !n || n.value ? t.items.push({ start: [this.sourceToken] }) : n.sep ? n.sep.push(this.sourceToken) : n.start.push(this.sourceToken);
          return;
        case "alias":
        case "scalar":
        case "single-quoted-scalar":
        case "double-quoted-scalar": {
          const i = this.flowScalar(this.type);
          !n || n.value ? t.items.push({ start: [], key: i, sep: [] }) : n.sep ? this.stack.push(i) : Object.assign(n, { key: i, sep: [] });
          return;
        }
        case "flow-map-end":
        case "flow-seq-end":
          t.end.push(this.sourceToken);
          return;
      }
      const s = this.startBlockValue(t);
      s ? this.stack.push(s) : (yield* this.pop(), yield* this.step());
    } else {
      const s = this.peek(2);
      if (s.type === "block-map" && (this.type === "map-value-ind" && s.indent === t.indent || this.type === "newline" && !s.items[s.items.length - 1].sep))
        yield* this.pop(), yield* this.step();
      else if (this.type === "map-value-ind" && s.type !== "flow-collection") {
        const i = gs(s), o = Jt(i);
        jr(t);
        const r = t.end.splice(1, t.end.length);
        r.push(this.sourceToken);
        const l = {
          type: "block-map",
          offset: t.offset,
          indent: t.indent,
          items: [{ start: o, key: t, sep: r }]
        };
        this.onKeyLine = !0, this.stack[this.stack.length - 1] = l;
      } else
        yield* this.lineEnd(t);
    }
  }
  flowScalar(t) {
    if (this.onNewLine) {
      let n = this.source.indexOf(`
`) + 1;
      for (; n !== 0; )
        this.onNewLine(this.offset + n), n = this.source.indexOf(`
`, n) + 1;
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
        const n = gs(t), s = Jt(n);
        return s.push(this.sourceToken), {
          type: "block-map",
          offset: this.offset,
          indent: this.indent,
          items: [{ start: s, explicitKey: !0 }]
        };
      }
      case "map-value-ind": {
        this.onKeyLine = !0;
        const n = gs(t), s = Jt(n);
        return {
          type: "block-map",
          offset: this.offset,
          indent: this.indent,
          items: [{ start: s, key: null, sep: [this.sourceToken] }]
        };
      }
    }
    return null;
  }
  atIndentedComment(t, n) {
    return this.type !== "comment" || this.indent <= n ? !1 : t.every((s) => s.type === "newline" || s.type === "space");
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
function up(e) {
  const t = e.prettyErrors !== !1;
  return { lineCounter: e.lineCounter || t && new ap() || null, prettyErrors: t };
}
function dp(e, t = {}) {
  const { lineCounter: n, prettyErrors: s } = up(t), i = new fp(n == null ? void 0 : n.addNewLine), o = new ip(t);
  let r = null;
  for (const l of o.compose(i.parse(e), !0, e.length))
    if (!r)
      r = l;
    else if (r.options.logLevel !== "silent") {
      r.errors.push(new An(l.range.slice(0, 2), "MULTIPLE_DOCS", "Source contains multiple documents; please use YAML.parseAllDocuments()"));
      break;
    }
  return s && n && (r.errors.forEach(Vr(e, n)), r.warnings.forEach(Vr(e, n))), r;
}
function pp(e, t, n) {
  let s;
  const i = dp(e, n);
  if (!i)
    return null;
  if (i.warnings.forEach((o) => uc(i.options.logLevel, o)), i.errors.length > 0) {
    if (i.options.logLevel !== "silent")
      throw i.errors[0];
    i.errors = [];
  }
  return i.toJS(Object.assign({ reviver: s }, n));
}
function hp(e, t, n) {
  let s = null;
  if (Array.isArray(t) && (s = t), e === void 0) {
    const { keepUndefined: i } = {};
    if (!i)
      return;
  }
  return ns(e) && !s ? e.toString(n) : new $o(e, s, n).toString(n);
}
const mp = { style: { display: "flex", "flex-direction": "column", gap: "8px", "font-size": "12px" } }, gp = { style: { display: "flex", gap: "16px", "align-items": "center" } }, yp = { style: { display: "flex", gap: "6px", "align-items": "center" } }, bp = { style: { display: "flex", gap: "6px", "align-items": "center" } }, _p = {
  key: 0,
  style: { opacity: ".6", "font-style": "italic" }
}, wp = { style: { display: "flex", "justify-content": "space-between", "align-items": "center" } }, Ep = { style: { display: "flex", gap: "10px", "align-items": "center" } }, vp = ["onUpdate:modelValue"], Np = ["onUpdate:modelValue"], Op = ["onClick"], Sp = {
  key: 0,
  style: { display: "flex", gap: "10px", "align-items": "center" }
}, kp = ["onUpdate:modelValue"], Tp = ["onUpdate:modelValue"], Dp = {
  key: 1,
  style: { display: "flex", gap: "10px", "align-items": "center" }
}, Cp = ["onUpdate:modelValue"], Ap = {
  key: 2,
  style: { display: "flex", gap: "10px", "align-items": "center" }
}, Ip = {
  key: 0,
  style: { display: "flex", gap: "6px", "align-items": "center" }
}, $p = ["onUpdate:modelValue"], Vp = ["onUpdate:modelValue"], Lp = ["onUpdate:modelValue"], Mp = { style: { display: "flex", gap: "10px", "align-items": "center" } }, xp = ["onUpdate:modelValue"], Pp = { style: { display: "flex", gap: "10px", "align-items": "center" } }, jp = ["disabled", "onClick"], Rp = { style: { opacity: ".7" } }, Fp = /* @__PURE__ */ cf({
  __name: "SettingsPanel",
  props: {
    api: {}
  },
  setup(e, { expose: t }) {
    const n = e;
    let s = 0;
    function i(b, p) {
      return {
        key: s++,
        name: b,
        provider: p.provider || "mssql",
        path: p.provider === "sqlite" ? p.file || "" : p.server || "",
        database: p.database || "",
        user: p.user || "",
        password: p.password || "",
        trustedConnection: p.trusted_connection ?? !0,
        description: p.description || "",
        testing: !1,
        testStatus: ""
      };
    }
    function o(b) {
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
    const r = (() => {
      try {
        return pp(n.api.getYaml() || "") || {};
      } catch {
        return {};
      }
    })(), l = /* @__PURE__ */ Uo(r.default_connection || ""), c = /* @__PURE__ */ Uo(r.default_limit || 10), a = /* @__PURE__ */ Js(
      Object.entries(r.connections || {}).map(([b, p]) => i(b, p))
    );
    function u() {
      a.push(i(`db${a.length + 1}`, { provider: "mssql" }));
    }
    async function f(b) {
      b.testing = !0, b.testStatus = "Connecting...";
      try {
        const p = await n.api.invoke("plugin.action", {
          pluginId: "sql",
          action: "testConnection",
          valueJson: JSON.stringify(h(b))
        });
        if (p.ok && p.resultJson) {
          const y = JSON.parse(p.resultJson);
          b.testStatus = y.message;
        } else
          b.testStatus = "Failed: " + (p.error || "unknown error");
      } catch (p) {
        b.testStatus = "Failed: " + (p instanceof Error ? p.message : String(p));
      } finally {
        b.testing = !1;
      }
    }
    function h(b) {
      const p = o(b);
      return {
        provider: p.provider,
        server: p.server,
        database: p.database,
        user: p.user,
        password: p.password,
        trustedConnection: p.trusted_connection,
        file: p.file,
        description: p.description
      };
    }
    function m() {
      const b = {
        default_connection: l.value || void 0,
        default_limit: c.value || 10,
        connections: Object.fromEntries(
          a.filter((p) => p.name.trim()).map((p) => [p.name, o(p)])
        )
      };
      return hp(b);
    }
    return t({ toYaml: m }), (b, p) => (Ze(), ct("div", mp, [
      p[14] || (p[14] = H("div", { style: { opacity: ".7" } }, "Named database connections available to the SQL agent. Stored opaquely in the .spla project file.", -1)),
      H("div", gp, [
        H("label", yp, [
          p[2] || (p[2] = H("span", { style: { opacity: ".7" } }, "Default connection", -1)),
          Pe(H("input", {
            "onUpdate:modelValue": p[0] || (p[0] = (y) => l.value = y),
            style: { width: "140px" }
          }, null, 512), [
            [at, l.value]
          ])
        ]),
        H("label", bp, [
          p[3] || (p[3] = H("span", { style: { opacity: ".7" } }, "Default row limit", -1)),
          Pe(H("input", {
            "onUpdate:modelValue": p[1] || (p[1] = (y) => c.value = y),
            type: "number",
            min: "1",
            style: { width: "90px" }
          }, null, 512), [
            [
              at,
              c.value,
              void 0,
              { number: !0 }
            ]
          ])
        ])
      ]),
      H("button", {
        type: "button",
        style: { "align-self": "flex-start" },
        onClick: u
      }, "+ Add Connection"),
      a.length ? us("", !0) : (Ze(), ct("div", _p, 'No connections yet. Click "+ Add Connection".')),
      (Ze(!0), ct(Ie, null, vf(a, (y, k) => (Ze(), ct("div", {
        key: y.key,
        style: { border: "1px solid var(--panel-border,#444)", "border-radius": "4px", padding: "10px", display: "flex", "flex-direction": "column", gap: "6px" }
      }, [
        H("div", wp, [
          H("div", Ep, [
            p[5] || (p[5] = H("span", { style: { opacity: ".7" } }, "Name", -1)),
            Pe(H("input", {
              "onUpdate:modelValue": (E) => y.name = E,
              style: { width: "140px" }
            }, null, 8, vp), [
              [at, y.name]
            ]),
            p[6] || (p[6] = H("span", { style: { opacity: ".7" } }, "Provider", -1)),
            Pe(H("select", {
              "onUpdate:modelValue": (E) => y.provider = E
            }, [...p[4] || (p[4] = [
              H("option", { value: "mssql" }, "mssql", -1),
              H("option", { value: "postgres" }, "postgres", -1),
              H("option", { value: "sqlite" }, "sqlite", -1)
            ])], 8, Np), [
              [Gu, y.provider]
            ])
          ]),
          H("button", {
            type: "button",
            onClick: (E) => a.splice(k, 1)
          }, "✕ Remove", 8, Op)
        ]),
        y.provider !== "sqlite" ? (Ze(), ct("div", Sp, [
          p[7] || (p[7] = H("span", { style: { opacity: ".7", width: "70px" } }, "Server", -1)),
          Pe(H("input", {
            "onUpdate:modelValue": (E) => y.path = E,
            placeholder: "sql01 or 192.168.1.10",
            style: { width: "220px" }
          }, null, 8, kp), [
            [at, y.path]
          ]),
          p[8] || (p[8] = H("span", { style: { opacity: ".7", width: "70px" } }, "Database", -1)),
          Pe(H("input", {
            "onUpdate:modelValue": (E) => y.database = E,
            style: { width: "160px" }
          }, null, 8, Tp), [
            [at, y.database]
          ])
        ])) : (Ze(), ct("div", Dp, [
          p[9] || (p[9] = H("span", { style: { opacity: ".7", width: "70px" } }, "File", -1)),
          Pe(H("input", {
            "onUpdate:modelValue": (E) => y.path = E,
            placeholder: "C:\\data\\mydb.sqlite",
            style: { width: "400px" }
          }, null, 8, Cp), [
            [at, y.path]
          ])
        ])),
        y.provider !== "sqlite" ? (Ze(), ct("div", Ap, [
          y.provider === "mssql" ? (Ze(), ct("label", Ip, [
            Pe(H("input", {
              type: "checkbox",
              "onUpdate:modelValue": (E) => y.trustedConnection = E
            }, null, 8, $p), [
              [Yu, y.trustedConnection]
            ]),
            p[10] || (p[10] = Yl(" Windows Auth (domain) ", -1))
          ])) : us("", !0),
          !y.trustedConnection || y.provider !== "mssql" ? (Ze(), ct(Ie, { key: 1 }, [
            p[11] || (p[11] = H("span", { style: { opacity: ".7" } }, "User", -1)),
            Pe(H("input", {
              "onUpdate:modelValue": (E) => y.user = E,
              style: { width: "130px" }
            }, null, 8, Vp), [
              [at, y.user]
            ]),
            p[12] || (p[12] = H("span", { style: { opacity: ".7" } }, "Password", -1)),
            Pe(H("input", {
              "onUpdate:modelValue": (E) => y.password = E,
              type: "password",
              placeholder: "or env:MY_VAR",
              style: { width: "130px" }
            }, null, 8, Lp), [
              [at, y.password]
            ])
          ], 64)) : us("", !0)
        ])) : us("", !0),
        H("div", Mp, [
          p[13] || (p[13] = H("span", { style: { opacity: ".7", width: "70px" } }, "Description", -1)),
          Pe(H("input", {
            "onUpdate:modelValue": (E) => y.description = E,
            placeholder: "Shown to the AI — what this database contains",
            style: { width: "500px" }
          }, null, 8, xp), [
            [at, y.description]
          ])
        ]),
        H("div", Pp, [
          H("button", {
            type: "button",
            disabled: y.testing,
            onClick: (E) => f(y)
          }, "Test Connection", 8, jp),
          H("span", Rp, qr(y.testStatus), 1)
        ])
      ]))), 128))
    ]));
  }
});
function Kp(e, t) {
  let n = zu(Fp, { api: t });
  const s = n.mount(e);
  return {
    save: () => s.toYaml(),
    destroy: () => {
      n == null || n.unmount(), n = null;
    }
  };
}
export {
  Kp as mount
};
