import { test, expect } from '@playwright/test';

test.describe('Select styling fix check', () => {
  test('Check custom select dropdowns styling', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Take initial screenshot
    await page.screenshot({ 
      path: 'screenshots/custom-select-01-initial.png',
      fullPage: true 
    });

    // Open race select (click on trigger)
    const raceTrigger = page.locator('#race.custom-select-trigger');
    await raceTrigger.click();
    
    // Wait for dropdown to open
    await page.waitForSelector('.custom-select-dropdown', { state: 'visible' });
    await page.waitForTimeout(300);
    
    // Screenshot with opened race dropdown
    await page.screenshot({ 
      path: 'screenshots/custom-select-02-race-open.png',
      fullPage: true 
    });

    // Select an option
    await page.locator('.custom-select-option:has-text("Elf")').first().click();
    await page.waitForTimeout(300);
    
    await page.screenshot({ 
      path: 'screenshots/custom-select-03-race-selected.png',
      fullPage: true 
    });

    // Open class select
    const classTrigger = page.locator('#class.custom-select-trigger');
    await classTrigger.click();
    await page.waitForSelector('.custom-select-dropdown', { state: 'visible' });
    await page.waitForTimeout(300);
    
    // Screenshot with opened class dropdown
    await page.screenshot({ 
      path: 'screenshots/custom-select-04-class-open.png',
      fullPage: true 
    });

    // Hover over option
    await page.locator('.custom-select-option:has-text("Wizard")').first().hover();
    await page.waitForTimeout(300);
    
    await page.screenshot({ 
      path: 'screenshots/custom-select-05-hover-state.png',
      fullPage: true 
    });

    // Select wizard
    await page.locator('.custom-select-option:has-text("Wizard")').first().click();
    await page.waitForTimeout(300);

    // Open language select
    const languageTrigger = page.locator('#gameLanguage.custom-select-trigger');
    await languageTrigger.click();
    await page.waitForSelector('.custom-select-dropdown', { state: 'visible' });
    await page.waitForTimeout(300);
    
    // Screenshot with opened language dropdown
    await page.screenshot({ 
      path: 'screenshots/custom-select-06-language-open.png',
      fullPage: true 
    });

    // Select Ukrainian
    await page.locator('.custom-select-option:has-text("Українська")').click();
    await page.waitForTimeout(300);
    
    await page.screenshot({ 
      path: 'screenshots/custom-select-07-ukrainian-selected.png',
      fullPage: true 
    });

    // Test keyboard navigation
    await page.keyboard.press('Tab'); // Focus next element
    await page.keyboard.press('Shift+Tab'); // Focus back to language select
    await page.keyboard.press('Enter'); // Open dropdown
    await page.waitForTimeout(300);
    
    await page.screenshot({ 
      path: 'screenshots/custom-select-08-keyboard-opened.png',
      fullPage: true 
    });

    await page.keyboard.press('ArrowDown'); // Navigate down
    await page.waitForTimeout(300);
    await page.keyboard.press('ArrowDown'); // Navigate down again
    await page.waitForTimeout(300);
    
    await page.screenshot({ 
      path: 'screenshots/custom-select-09-keyboard-navigation.png',
      fullPage: true 
    });

    await page.keyboard.press('Escape'); // Close dropdown
    await page.waitForTimeout(300);

    // Final screenshot
    await page.screenshot({ 
      path: 'screenshots/custom-select-10-final.png',
      fullPage: true 
    });

    // Verify all selects are properly rendered
    await expect(raceTrigger).toBeVisible();
    await expect(classTrigger).toBeVisible();
    await expect(languageTrigger).toBeVisible();
  });
});
