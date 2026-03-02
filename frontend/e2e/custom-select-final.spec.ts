import { test, expect } from '@playwright/test';

test.describe('CustomSelect Final Check', () => {
  test('Complete CustomSelect functionality', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // 1. Initial state
    await page.screenshot({ 
      path: 'screenshots/final-01-initial-state.png',
      fullPage: true 
    });

    // 2. Fill character name
    await page.fill('input#characterName', 'Test Character');

    // 3. Test Race select
    await page.click('#race');
    await page.waitForTimeout(500);
    
    await page.screenshot({ 
      path: 'screenshots/final-02-race-dropdown-open.png',
      fullPage: true 
    });

    await page.click('.custom-select-option >> text=Elf');
    await page.waitForTimeout(300);

    // 4. Test Class select
    await page.click('#class');
    await page.waitForTimeout(500);
    
    await page.screenshot({ 
      path: 'screenshots/final-03-class-dropdown-open.png',
      fullPage: true 
    });

    await page.click('.custom-select-option >> text=Wizard');
    await page.waitForTimeout(300);

    // 5. Test Language select
    await page.click('#gameLanguage');
    await page.waitForTimeout(500);
    
    await page.screenshot({ 
      path: 'screenshots/final-04-language-dropdown-open.png',
      fullPage: true 
    });

    await page.click('.custom-select-option >> text=Українська');
    await page.waitForTimeout(300);

    // 6. Final filled form
    await page.screenshot({ 
      path: 'screenshots/final-05-all-fields-filled.png',
      fullPage: true 
    });

    // Verify values are selected
    await expect(page.locator('#race >> text=Elf')).toBeVisible();
    await expect(page.locator('#class >> text=Wizard')).toBeVisible();
  });

  test('Mobile responsive check', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await page.screenshot({ 
      path: 'screenshots/final-06-mobile-initial.png',
      fullPage: true 
    });

    // Open select on mobile
    await page.click('#race');
    await page.waitForTimeout(500);

    await page.screenshot({ 
      path: 'screenshots/final-07-mobile-dropdown-open.png',
      fullPage: true 
    });
  });
});
