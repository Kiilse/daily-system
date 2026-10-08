// Library boundary rules (ADR-015), checked through the ESLint API on in-memory files: run with `node --test`.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { ESLint } from 'eslint';

const eslint = new ESLint({ cwd: new URL('..', import.meta.url).pathname });

async function boundaryErrors(fromFile, importedAlias) {
  const code = `import * as dep from '${importedAlias}';\nexport const used = dep;\n`;
  const [result] = await eslint.lintText(code, { filePath: fromFile });
  return result.messages.filter((m) => m.ruleId?.startsWith('boundaries/'));
}

const cases = [
  ['projects/feature-menu/src/lib/x.ts', '@daily-system/feature-calendar', false],
  ['projects/feature-calendar/src/lib/x.ts', '@daily-system/feature-menu', false],
  ['projects/core/src/lib/x.ts', '@daily-system/feature-menu', false],
  ['projects/ui/src/lib/x.ts', '@daily-system/feature-menu', false],
  ['projects/feature-menu/src/lib/x.ts', '@daily-system/core', true],
  ['projects/feature-menu/src/lib/x.ts', '@daily-system/ui', true],
  ['projects/feature-menu/src/lib/x.ts', '@daily-system/api-clients', true],
  ['src/app/x.ts', '@daily-system/feature-menu', true],
];

for (const [from, to, allowed] of cases) {
  test(`${from} importing ${to} is ${allowed ? 'allowed' : 'rejected'}`, async () => {
    const errors = await boundaryErrors(from, to);
    assert.equal(errors.length === 0, allowed, JSON.stringify(errors));
    // A rejection must come from the feature boundary, not from a file classified under the wrong element.
    for (const e of errors) assert.match(e.message, /to elements of type "feature"/);
  });
}
