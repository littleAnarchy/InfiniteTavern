import { test } from '@playwright/test';

test.describe('CustomSelect Debug', () => {
  test('Debug CustomSelect rendering', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Take screenshot of initial state
    await page.screenshot({ 
      path: 'screenshots/debug-01-loaded.png',
      fullPage: true 
    });

    // Check if CustomSelect exists
    const customSelectExists = await page.locator('.custom-select').count();
    console.log('CustomSelect count:', customSelectExists);

    // Check for triggers
    const triggers = await page.locator('.custom-select-trigger').count();
    console.log('Triggers count:', triggers);

    // Get all trigger IDs
    const triggerIds = await page.locator('.custom-select-trigger').evaluateAll((elements) =>
      elements.map((el) => el.id)
    );
    console.log('Trigger IDs:', triggerIds);

    // Try to click first trigger
    if (triggers > 0) {
      await page.locator('.custom-select-trigger').first().click();
      await page.waitForTimeout(1000);

      await page.screenshot({ 
        path: 'screenshots/debug-02-after-click.png',
        fullPage: true 
      });

      // Check if dropdown appeared
      const dropdown = await page.locator('.custom-select-dropdown').count();
      console.log('Dropdown count:', dropdown);

      if (dropdown > 0) {
        // Get all options text
        const optionsText = await page.locator('.custom-select-option').evaluateAll((elements) =>
          elements.map((el) => el.textContent)
        );
        console.log('Options text:', optionsText);

        const optionsCount = await page.locator('.custom-select-option').count();
        console.log('Options count:', optionsCount);
      }
    }

    // Check page HTML
    const html = await page.content();
    console.log('Page includes "custom-select":', html.includes('custom-select'));
    console.log('Page includes "select#":', html.includes('select#'));
  });
});
