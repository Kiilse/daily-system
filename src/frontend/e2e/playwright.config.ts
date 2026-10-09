// E2E tests against a deployed environment come with #22. Run with `npx playwright test -c e2e`.
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  forbidOnly: !!process.env['CI'],
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
