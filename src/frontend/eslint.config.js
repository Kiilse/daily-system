// @ts-check
const eslint = require('@eslint/js');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');
const boundaries = require('eslint-plugin-boundaries');

module.exports = defineConfig([
  // Library boundaries (ADR-015), mirroring the backend modules. Tested by tools/boundaries.test.mjs.
  {
    files: ['**/*.ts'],
    plugins: { boundaries },
    settings: {
      'import/resolver': { typescript: { project: `${__dirname}/tsconfig.json` } },
      'boundaries/elements': [
        { type: 'app', pattern: 'src/app' },
        { type: 'core', pattern: 'projects/core' },
        { type: 'ui', pattern: 'projects/ui' },
        { type: 'api-clients', pattern: 'projects/api-clients' },
        { type: 'feature', pattern: 'projects/feature-*' },
      ],
    },
    rules: {
      'boundaries/dependencies': [
        'error',
        {
          default: 'disallow',
          policies: [
            { from: { element: { type: 'app' } }, allow: { to: { element: { types: { anyOf: ['core', 'ui', 'api-clients', 'feature'] } } } } },
            { from: { element: { type: 'feature' } }, allow: { to: { element: { types: { anyOf: ['core', 'ui', 'api-clients'] } } } } },
            { from: { element: { type: 'core' } }, allow: { to: { element: { type: 'api-clients' } } } },
          ],
        },
      ],
    },
  },
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommended,
      tseslint.configs.stylistic,
      angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/directive-selector': [
        'error',
        {
          type: 'attribute',
          prefix: 'app',
          style: 'camelCase',
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'app',
          style: 'kebab-case',
        },
      ],
    },
  },
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {},
  },
]);
