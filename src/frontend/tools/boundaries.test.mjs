// Library boundary rules (ADR-015), checked through the ESLint API on in-memory files: run with `node --test`.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { ESLint } from 'eslint';

const eslint = new ESLint({ cwd: fileURLToPath(new URL('..', import.meta.url)) });

async function boundaryErrors(fromFile, importedAlias) {
  const code = `import * as dep from '${importedAlias}';\nexport const used = dep;\n`;
  const [result] = await eslint.lintText(code, { filePath: fromFile });
  return result.messages.filter((m) => m.ruleId?.startsWith('boundaries/'));
}

// [importing file, imported alias, element type of the target, or null when the import is allowed]
const cases = [
  ['projects/feature-menu/src/lib/x.ts', '@daily-system/feature-calendar', 'feature'],
  ['projects/feature-calendar/src/lib/x.ts', '@daily-system/feature-menu', 'feature'],
  ['projects/core/src/lib/x.ts', '@daily-system/feature-menu', 'feature'],
  ['projects/core/src/lib/x.ts', '@daily-system/ui', 'ui'],
  ['projects/ui/src/lib/x.ts', '@daily-system/feature-menu', 'feature'],
  ['projects/ui/src/lib/x.ts', '@daily-system/core', 'core'],
  ['projects/api-clients/src/lib/x.ts', '@daily-system/core', 'core'],
  ['projects/feature-menu/src/lib/x.ts', '@daily-system/core', null],
  ['projects/feature-menu/src/lib/x.ts', '@daily-system/ui', null],
  ['projects/feature-menu/src/lib/x.ts', '@daily-system/api-clients', null],
  ['projects/core/src/lib/x.ts', '@daily-system/api-clients', null],
  ['src/app/x.ts', '@daily-system/feature-menu', null],
];

for (const [from, to, rejectedType] of cases) {
  test(`${from} importing ${to} is ${rejectedType ? 'rejected' : 'allowed'}`, async () => {
    const errors = await boundaryErrors(from, to);
    assert.equal(errors.length > 0, rejectedType !== null, JSON.stringify(errors));
    // A rejection must name the right target element, not come from a file classified under the wrong one.
    for (const e of errors) assert.match(e.message, new RegExp(`to elements of type "${rejectedType}"`));
  });
}
