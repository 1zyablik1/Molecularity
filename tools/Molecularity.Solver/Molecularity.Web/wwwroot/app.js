const $ = (selector) => document.querySelector(selector);
const state = { levels: [], configs: new Map(), engine: null, game: null, selectedItem: null, targets: [], busy: false };
const itemInfo = {
  RevealAll: { icon: '◉', name: 'Проявитель', desc: 'Открыть все значения', targets: 0 },
  PlusOneAll: { icon: '+1', name: 'Катализатор', desc: '+1 каждой молекуле', targets: 0 },
  Freeze: { icon: '✳', name: 'Крио-импульс', desc: 'Заморозить одну цель', targets: 1 },
  ChainBreak: { icon: '⌁', name: 'Разрыв связи', desc: 'Выбрать связанную пару', targets: 2 },
  Undo: { icon: '↶', name: 'Откат', desc: 'Отменить последний ход', targets: 0 }
};

class StaticGameEngine {
  constructor(config) {
    this.config = config;
    this.balance = { baseDecrement: -1, shieldTurns: 2, freezeTurns: 3, anchorDecrement: -2, anchorHeal: 1, ...(config.balance || {}) };
    this.molecules = config.molecules.map(m => ({
      id: m.id, type: m.type, value: m.initialValue, isRevealed: m.isInitiallyRevealed,
      isAlive: true, shieldLeft: m.type === 'Shield' ? this.balance.shieldTurns : null, freezes: []
    }));
    this.connections = config.connections.map(c => ({ ...c }));
    this.inventory = Object.fromEntries(Object.keys(itemInfo).map(type => [type, 10]));
    this.status = 'InProgress'; this.turns = 0; this.itemsUsed = 0;
    this.previousSnapshot = null; this.lastActionWasUndo = false; this.culpritId = null;
  }

  get canUndo() { return !!this.previousSnapshot && !this.lastActionWasUndo && this.status === 'InProgress' && this.inventory.Undo > 0; }
  alive() { return this.molecules.filter(m => m.isAlive); }
  molecule(id) { const m = this.molecules.find(x => x.id === id); if (!m) throw new Error(`Молекула ${id} не найдена`); return m; }
  neighbors(id) {
    const ids = this.connections.flatMap(c => c.fromId === id ? [c.toId] : c.toId === id ? [c.fromId] : []);
    return this.alive().filter(m => ids.includes(m.id));
  }
  snapshot() { return { molecules: structuredClone(this.molecules), connections: structuredClone(this.connections) }; }
  restore(s) { this.molecules = structuredClone(s.molecules); this.connections = structuredClone(s.connections); }
  ensurePlaying() { if (this.status !== 'InProgress') throw new Error('Игра уже завершена. Перезапустите уровень.'); }

  takeTurn(id) {
    this.ensurePlaying();
    const target = this.molecule(id);
    if (!target.isAlive) throw new Error('Эта молекула уже удалена.');
    this.previousSnapshot = this.snapshot(); this.lastActionWasUndo = false;

    if (target.type === 'Anchor') this.neighbors(id).forEach(m => m.value += this.balance.anchorHeal);
    const exposed = this.neighbors(id);
    target.isAlive = false;
    exposed.forEach(m => m.isRevealed = true);
    this.connections = this.connections.filter(c => c.fromId !== id && c.toId !== id);

    this.alive().forEach(m => {
      let delta = this.balance.baseDecrement;
      if (m.type === 'Parasite') delta = -this.neighbors(m.id).length;
      if (m.type === 'Anchor') delta = this.balance.anchorDecrement;
      if (m.type === 'Shield' && m.shieldLeft > 0) delta = 0;
      if (m.freezes.length) delta = 0;
      m.value += delta;
      if (m.type === 'Shield' && m.shieldLeft > 0) m.shieldLeft--;
      m.freezes = m.freezes.map(turns => turns - 1).filter(turns => turns > 0);
    });
    this.turns++;

    const culprit = this.alive().find(m => m.value <= 0);
    if (culprit) { this.status = 'Lose'; this.culpritId = culprit.id; }
    else if (!this.alive().length) this.status = 'Win';
    return this.response(this.status === 'Win' ? 'Уровень пройден' : this.status === 'Lose' ? 'Молекула распалась. Попробуйте ещё раз' : `Молекула ${id} удалена`);
  }

  useItem(type, targets) {
    this.ensurePlaying();
    if (!this.inventory[type]) throw new Error('Предметы этого типа закончились.');
    if (type === 'Undo') {
      if (!this.canUndo) throw new Error('Откат пока недоступен');
      this.inventory.Undo--; this.restore(this.previousSnapshot); this.previousSnapshot = null;
      this.lastActionWasUndo = true; this.itemsUsed++; this.turns--;
      return this.response('Последний ход отменён');
    }
    if (type === 'Freeze') {
      const target = this.molecule(targets[0]);
      if (!target.isAlive) throw new Error('Цель уже удалена.');
      target.freezes.push(this.balance.freezeTurns);
    } else if (type === 'ChainBreak') {
      const [from, to] = targets;
      const index = this.connections.findIndex(c => (c.fromId === from && c.toId === to) || (c.fromId === to && c.toId === from));
      if (index < 0) throw new Error('Между выбранными молекулами нет связи.');
      this.connections.splice(index, 1);
    } else if (type === 'RevealAll') {
      this.alive().forEach(m => m.isRevealed = true);
    } else if (type === 'PlusOneAll') {
      this.alive().forEach(m => m.value++);
    } else throw new Error('Неизвестный предмет.');
    this.inventory[type]--; this.itemsUsed++;
    return this.response(`Использован предмет ${type}`);
  }

  response(message) {
    return {
      gameId: 'static', levelId: this.config.levelId, status: this.status, turns: this.turns,
      itemsUsed: this.itemsUsed, canUndo: this.canUndo, message, culpritId: this.culpritId,
      molecules: this.alive().map(({ id, type, value, isRevealed }) => ({ id, type, value, isRevealed })),
      connections: this.connections.map(c => ({ ...c })), inventory: { ...this.inventory }
    };
  }
}

async function loadLevels() {
  try {
    const manifest = await fetch('./levels/index.json').then(response => {
      if (!response.ok) throw new Error('Не удалось загрузить список уровней');
      return response.json();
    });
    const configs = await Promise.all(manifest.map(id => fetch(`./levels/level_${id}.json`).then(r => r.json())));
    configs.forEach(config => state.configs.set(config.levelId, config));
    state.levels = configs.map(config => ({
      id: config.levelId, molecules: config.molecules.length, connections: config.connections.length,
      types: [...new Set(config.molecules.map(m => m.type))]
    }));
    $('#levelsGrid').innerHTML = state.levels.map(level => `
      <button class="level-card" data-level="${level.id}">
        <span class="index">ОБРАЗЕЦ / ${String(level.id).padStart(2, '0')}</span>
        ${miniGraph(level)}
        <strong>${String(level.id).padStart(2, '0')}</strong>
        <span class="meta">${level.molecules} УЗЛОВ · ${level.connections} СВЯЗЕЙ</span>
      </button>`).join('');
    document.querySelectorAll('[data-level]').forEach(el => el.addEventListener('click', () => startLevel(+el.dataset.level)));
  } catch (error) { toast(error.message); }
}

function miniGraph(level) {
  const count = Math.min(level.molecules, 6);
  const points = Array.from({ length: count }, (_, i) => {
    const a = i * Math.PI * 2 / count - Math.PI / 2;
    return [35 + Math.cos(a) * 25, 35 + Math.sin(a) * 25];
  });
  const lines = points.slice(1).map((p, i) => `<line x1="${points[i][0]}" y1="${points[i][1]}" x2="${p[0]}" y2="${p[1]}"/>`).join('');
  return `<svg class="mini-graph" viewBox="0 0 70 70"><g stroke="#527267" stroke-width="1">${lines}</g>${points.map(p => `<circle cx="${p[0]}" cy="${p[1]}" r="3" fill="#b8f24a"/>`).join('')}</svg>`;
}

async function startLevel(levelId) {
  if (state.busy) return;
  state.busy = true;
  try {
    const config = state.configs.get(levelId);
    if (!config) throw new Error('Уровень не найден');
    state.engine = new StaticGameEngine(config);
    state.game = state.engine.response('Уровень начат');
    state.selectedItem = null; state.targets = [];
    $('#levelsView').hidden = true; $('#gameView').hidden = false; $('#topStats').hidden = false;
    $('#levelNumber').textContent = String(levelId).padStart(2, '0');
    renderGame();
  } catch (error) { toast(error.message); }
  finally { state.busy = false; }
}

async function takeTurn(id) {
  if (state.busy || state.game.status !== 'InProgress') return;
  if (state.selectedItem) return selectTarget(id);
  await mutate(() => state.engine.takeTurn(id));
}

async function chooseItem(type) {
  if (state.busy || state.game.status !== 'InProgress' || !state.game.inventory[type]) return;
  if (type === 'Undo' && !state.game.canUndo) return toast('Откат пока недоступен');
  const info = itemInfo[type];
  if (info.targets === 0) return useItem(type, []);
  state.selectedItem = state.selectedItem === type ? null : type;
  state.targets = [];
  updateHint(); renderInventory(); renderGraph();
}

async function selectTarget(id) {
  if (state.targets.includes(id)) { state.targets = state.targets.filter(x => x !== id); renderGraph(); return; }
  if (state.selectedItem === 'ChainBreak' && state.targets.length === 1) {
    const first = state.targets[0];
    if (!state.game.connections.some(c => (c.fromId === first && c.toId === id) || (c.toId === first && c.fromId === id))) return toast('Выберите молекулу, связанную с первой');
  }
  state.targets.push(id); renderGraph();
  if (state.targets.length === itemInfo[state.selectedItem].targets) await useItem(state.selectedItem, state.targets);
}

async function useItem(type, targets) {
  await mutate(() => state.engine.useItem(type, targets));
  state.selectedItem = null; state.targets = [];
}

async function mutate(action) {
  state.busy = true;
  try {
    state.game = action();
    renderGame();
  } catch (error) { toast(error.message); }
  finally { state.busy = false; }
}

function renderGame() {
  $('#turnCount').textContent = state.game.turns;
  $('#usedCount').textContent = state.game.itemsUsed;
  $('#protocolText').textContent = state.game.message;
  renderInventory(); renderGraph(); updateHint(); renderResult();
}

function renderInventory() {
  $('#items').innerHTML = Object.entries(itemInfo).map(([type, info]) => {
    const count = state.game.inventory[type] || 0;
    const disabled = !count || state.game.status !== 'InProgress' || (type === 'Undo' && !state.game.canUndo);
    return `<button class="item-button ${state.selectedItem === type ? 'active' : ''}" data-item="${type}" ${disabled ? 'disabled' : ''}>
      <span class="item-icon">${info.icon}</span><span><span class="item-name">${info.name}</span><span class="item-desc">${info.desc}</span></span><span class="item-count">×${count}</span>
    </button>`;
  }).join('');
  document.querySelectorAll('[data-item]').forEach(el => el.addEventListener('click', () => chooseItem(el.dataset.item)));
}

function layoutGraph(nodes, links) {
  const pos = new Map(nodes.map((n, i) => {
    const a = (i * 2.399963 + n.id * .17);
    const r = 90 + 25 * Math.sqrt(i);
    return [n.id, { x: 500 + Math.cos(a) * r, y: 325 + Math.sin(a) * r }];
  }));
  for (let step = 0; step < 170; step++) {
    const force = new Map(nodes.map(n => [n.id, { x: 0, y: 0 }]));
    for (let i = 0; i < nodes.length; i++) for (let j = i + 1; j < nodes.length; j++) {
      const a = pos.get(nodes[i].id), b = pos.get(nodes[j].id); let dx = a.x-b.x, dy=a.y-b.y; const d2=Math.max(2500, dx*dx+dy*dy); const f=9000/d2;
      force.get(nodes[i].id).x += dx*f; force.get(nodes[i].id).y += dy*f; force.get(nodes[j].id).x -= dx*f; force.get(nodes[j].id).y -= dy*f;
    }
    links.forEach(l => { const a=pos.get(l.fromId), b=pos.get(l.toId); if(!a||!b)return; const dx=b.x-a.x,dy=b.y-a.y,d=Math.max(1,Math.hypot(dx,dy)),f=(d-190)*.018; force.get(l.fromId).x+=dx/d*f;force.get(l.fromId).y+=dy/d*f;force.get(l.toId).x-=dx/d*f;force.get(l.toId).y-=dy/d*f; });
    nodes.forEach(n => { const p=pos.get(n.id), f=force.get(n.id); p.x=Math.max(80,Math.min(920,p.x+f.x+(500-p.x)*.003));p.y=Math.max(75,Math.min(575,p.y+f.y+(325-p.y)*.003)); });
  }
  return pos;
}

function renderGraph() {
  const graph = $('#graph'), g = state.game, pos = layoutGraph(g.molecules, g.connections);
  const lines = g.connections.map(c => { const a=pos.get(c.fromId),b=pos.get(c.toId); const active=state.selectedItem==='ChainBreak'&&state.targets.length===1&&(c.fromId===state.targets[0]||c.toId===state.targets[0]); return `<line class="edge ${active?'active':''}" x1="${a.x}" y1="${a.y}" x2="${b.x}" y2="${b.y}"/>`; }).join('');
  const nodes = g.molecules.map(n => { const p=pos.get(n.id), selected=state.targets.includes(n.id), culprit=g.culpritId===n.id; return `<g class="node ${n.type.toLowerCase()} ${selected?'selected':''} ${culprit?'culprit':''}" data-node="${n.id}" tabindex="0" role="button" aria-label="${n.type}, ${n.isRevealed?n.value:'значение скрыто'}" transform="translate(${p.x} ${p.y})">
    <circle class="halo" r="61"/><circle class="core" r="47"/><text class="value" y="7">${n.isRevealed?n.value:'??'}</text><text class="type" y="72">${typeLabel(n.type)}</text></g>`; }).join('');
  graph.innerHTML = `<g>${lines}</g><g>${nodes}</g>`;
  document.querySelectorAll('[data-node]').forEach(el => { const act=()=>takeTurn(+el.dataset.node); el.addEventListener('click',act); el.addEventListener('keydown',e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();act();}}); });
}

function typeLabel(type) { return ({Simple:'ПРОСТАЯ',Parasite:'ПАРАЗИТ',Shield:'ЩИТ',Anchor:'ЯКОРЬ'})[type] || type.toUpperCase(); }
function updateHint() { const info = itemInfo[state.selectedItem]; $('#boardHint').textContent = info ? (state.targets.length ? 'ВЫБЕРИТЕ ВТОРУЮ СВЯЗАННУЮ МОЛЕКУЛУ' : `ЦЕЛЬ: ${info.name.toUpperCase()}`) : 'ВЫБЕРИТЕ МОЛЕКУЛУ, ЧТОБЫ УДАЛИТЬ'; }
function renderResult() { const over=state.game.status!=='InProgress', win=state.game.status==='Win'; $('#resultOverlay').hidden=!over; if(!over)return; $('#resultCode').textContent=win?'РЕАКЦИЯ ЗАВЕРШЕНА':'КРИТИЧЕСКАЯ НЕСТАБИЛЬНОСТЬ'; $('#resultTitle').textContent=win?'Сеть очищена':'Распад'; $('#resultText').textContent=win?`Уровень пройден за ${state.game.turns} ходов`:'Одна из молекул достигла нулевого значения'; }
function showLevels() { $('#gameView').hidden=true;$('#levelsView').hidden=false;$('#topStats').hidden=true;state.game=null;window.scrollTo({top:0,behavior:'smooth'}); }
function toast(message) { const el=$('#toast');el.textContent=message;el.classList.add('show');clearTimeout(toast.timer);toast.timer=setTimeout(()=>el.classList.remove('show'),2600); }

$('#homeButton').addEventListener('click', showLevels); $('#backButton').addEventListener('click', showLevels);
$('#restartButton').addEventListener('click', () => startLevel(state.game.levelId)); $('#overlayRestart').addEventListener('click', () => startLevel(state.game.levelId)); $('#overlayLevels').addEventListener('click', showLevels);
loadLevels();
