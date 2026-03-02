import { test, expect } from '@playwright/test';

test.describe('InfiniteTavern Frontend UI', () => {
  test('Character creation screen - UI check', async ({ page }) => {
    // Navigate to the app
    await page.goto('/');

    // Wait for page to load
    await page.waitForLoadState('networkidle');

    // Take full page screenshot
    await page.screenshot({ 
      path: 'screenshots/01-character-creation-full.png',
      fullPage: true 
    });

    // Check if all form elements are visible
    await expect(page.locator('h1')).toBeVisible();
    
    // Check character name input
    const nameInput = page.locator('input#characterName');
    await expect(nameInput).toBeVisible();
    await nameInput.screenshot({ path: 'screenshots/02-name-input.png' });

    // Check race selector
    const raceSelect = page.locator('select#race');
    await expect(raceSelect).toBeVisible();
    
    // Check class selector
    const classSelect = page.locator('select#class');
    await expect(classSelect).toBeVisible();

    // Check language selector
    const languageSelect = page.locator('select#gameLanguage');
    await expect(languageSelect).toBeVisible();

    // Check campaign type options
    const campaignOptions = page.locator('.campaign-type-options');
    await expect(campaignOptions).toBeVisible();
    await campaignOptions.screenshot({ path: 'screenshots/03-campaign-options.png' });

    // Check submit button
    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toBeVisible();
    await expect(submitButton).toBeEnabled();

    // Take screenshot of the form section
    const form = page.locator('form');
    await form.screenshot({ path: 'screenshots/04-form-section.png' });

    // Check mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.screenshot({ 
      path: 'screenshots/05-mobile-view.png',
      fullPage: true 
    });

    // Check tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.screenshot({ 
      path: 'screenshots/06-tablet-view.png',
      fullPage: true 
    });

    // Check wide desktop viewport
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.screenshot({ 
      path: 'screenshots/07-desktop-wide.png',
      fullPage: true 
    });
  });

  test('Fill form and check visual states', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Fill the form
    await page.fill('input#characterName', 'Test Hero');
    await page.selectOption('select#race', 'Elf');
    await page.selectOption('select#class', 'Wizard');
    await page.selectOption('select#gameLanguage', 'Ukrainian');
    
    // Select random campaign
    await page.click('input[value="random"]');

    // Screenshot filled form
    await page.screenshot({ 
      path: 'screenshots/08-filled-form.png',
      fullPage: true 
    });

    // Check form validation - empty name
    await page.fill('input#characterName', '');
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();
    
    // Screenshot validation state
    await page.screenshot({ 
      path: 'screenshots/09-validation-state.png',
      fullPage: true 
    });
  });

  test('Check styling and layout issues', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Get computed styles of key elements
    const h1 = page.locator('h1');
    const h1Box = await h1.boundingBox();
    console.log('H1 position:', h1Box);

    const form = page.locator('form');
    const formBox = await form.boundingBox();
    console.log('Form position:', formBox);

    // Check for overlapping elements
    const allButtons = page.locator('button');
    const buttonCount = await allButtons.count();
    console.log('Button count:', buttonCount);

    // Check for text overflow
    const labels = page.locator('label');
    const labelCount = await labels.count();
    console.log('Label count:', labelCount);

    // Screenshot with element highlights
    await page.evaluate(() => {
      document.querySelectorAll('.form-group').forEach((el, i) => {
        (el as HTMLElement).style.outline = '2px solid red';
        (el as HTMLElement).style.outlineOffset = '2px';
      });
    });

    await page.screenshot({ 
      path: 'screenshots/10-layout-debug.png',
      fullPage: true 
    });
  });
});
