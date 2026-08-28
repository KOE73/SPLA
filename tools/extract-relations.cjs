const fs = require('fs');
const path = require('path');

function getFiles(dir) {
  let results = [];
  const list = fs.readdirSync(dir);
  list.forEach(file => {
    const full = path.join(dir, file);
    const stat = fs.statSync(full);
    if (stat && stat.isDirectory()) {
      if (!full.includes('bin') && !full.includes('obj') && !full.includes('.git') && !full.includes('Test')) {
        results = results.concat(getFiles(full));
      }
    } else if (file.endsWith('.cs')) {
      results.push(full);
    }
  });
  return results;
}

const csFiles = getFiles('src');
console.log(`Found ${csFiles.length} source C# files`);

// 1. Pass 1: find all declared types
const typeDeclRegex = /^\s*(?:public|internal|protected|private)?\s*(?:(?:static|sealed|abstract|partial|readonly|ref|unsafe|new|file)\s+)*(class|interface|record|struct|enum)\s+(?:class\s+|struct\s+)?([A-Za-z0-9_]+)(?:\s*<[^>{]*>)?(?:\s*\([^()]*\))?(?:\s*:\s*([A-Za-z0-9_,\s<>\.\?\[\]]+?))?(?:\s*\{|\s*where|\s*;|\s*$)/gm;

const declaredTypes = new Map(); // Name -> { kind, name, bases, file, id }

for (const filePath of csFiles) {
  const content = fs.readFileSync(filePath, 'utf8');
  const relPath = path.relative('.', filePath).replace(/\\/g, '/');

  let match;
  while ((match = typeDeclRegex.exec(content)) !== null) {
    const kind = match[1];
    const name = match[2];
    const rawBases = match[3] || '';

    let bases = [];
    if (rawBases) {
      bases = rawBases.split(',').map(b => b.trim().split('<')[0].split('where')[0].trim()).filter(b => b && b !== 'where');
    }

    const id = `n_${name.toLowerCase()}`;
    declaredTypes.set(name, {
      kind: kind.charAt(0).toUpperCase() + kind.slice(1),
      name,
      bases,
      file: relPath,
      id
    });
  }
}

console.log(`Extracted ${declaredTypes.size} declared types from C# source`);

// Update entities.json
const existingEntitiesPath = 'docs/diagrams/projects/full_core/entities.json';
let entitiesObj = { entities: [] };
if (fs.existsSync(existingEntitiesPath)) {
  entitiesObj = JSON.parse(fs.readFileSync(existingEntitiesPath, 'utf8'));
}

const entityMap = new Map();
for (const e of entitiesObj.entities) {
  entityMap.set(e.id, e);
}

for (const [name, t] of declaredTypes.entries()) {
  if (!entityMap.has(t.id)) {
    entityMap.set(t.id, {
      id: t.id,
      name: t.name,
      kind: t.kind,
      origin: "code",
      status: "present",
      namespace: "",
      codeRef: t.file,
      members: []
    });
  }
}

entitiesObj.entities = Array.from(entityMap.values());
fs.writeFileSync(existingEntitiesPath, JSON.stringify(entitiesObj, null, 2), 'utf8');
console.log(`Updated ${existingEntitiesPath} with ${entitiesObj.entities.length} entities`);

// 2. Pass 2: find relationships
const relations = [];
const seenRel = new Set();

function addRelation(fromName, toName, type, label = '') {
  if (fromName === toName) return;
  const fromType = declaredTypes.get(fromName);
  const toType = declaredTypes.get(toName);
  if (!fromType || !toType) return;

  const key = `${fromType.id}->${toType.id}:${type}`;
  if (seenRel.has(key)) return;
  seenRel.add(key);

  const relId = `r_${fromName.toLowerCase()}_${toName.toLowerCase()}_${type}`;
  relations.push({
    id: relId,
    from: fromType.id,
    to: toType.id,
    type,
    relation: type,
    label,
    origin: "code",
    status: "present",
    evidence: [
      {
        codeRef: fromType.file
      }
    ]
  });
}

// Check inheritance
for (const [name, t] of declaredTypes.entries()) {
  for (const base of t.bases) {
    if (declaredTypes.has(base)) {
      const baseType = declaredTypes.get(base);
      const relType = (baseType.kind.toLowerCase() === 'interface' || base.startsWith('I')) ? 'implements' : 'extends';
      addRelation(name, base, relType);
    }
  }
}

// Field, Property & Constructor injections
for (const filePath of csFiles) {
  const content = fs.readFileSync(filePath, 'utf8');
  
  const typesInFile = [];
  for (const [name, t] of declaredTypes.entries()) {
    if (t.file.toLowerCase() === path.relative('.', filePath).replace(/\\/g, '/').toLowerCase()) {
      typesInFile.push(name);
    }
  }
  if (typesInFile.length === 0) continue;
  const primaryTypeName = typesInFile[0];

  for (const [targetName, targetType] of declaredTypes.entries()) {
    if (targetName === primaryTypeName) continue;

    const typeUsageRegex = new RegExp(`(?:readonly\\s+|private\\s+|protected\\s+|public\\s+)(?:IReadOnlyList<|IEnumerable<|List<|Task<|ValueTask<)?\\b${targetName}\\b(?:>)?\\s+[A-Za-z0-9_]+`, 'g');
    if (typeUsageRegex.test(content)) {
      addRelation(primaryTypeName, targetName, 'composes');
    }

    if (content.includes(`(${targetName} `) || content.includes(`, ${targetName} `) || content.includes(`,${targetName} `)) {
      addRelation(primaryTypeName, targetName, 'composes');
    }

    const callRegex = new RegExp(`\\b${targetName}\\b\\.[A-Z][A-Za-z0-9_]+`, 'g');
    if (callRegex.test(content)) {
      addRelation(primaryTypeName, targetName, 'call');
    }
  }
}

console.log(`Generated ${relations.length} relationships from C# codebase!`);

const fullCoreRelationsPath = 'docs/diagrams/projects/full_core/relations.json';
fs.writeFileSync(fullCoreRelationsPath, JSON.stringify({
  relations: relations
}, null, 2), 'utf8');

console.log(`Saved ${relations.length} relations to ${fullCoreRelationsPath}`);
