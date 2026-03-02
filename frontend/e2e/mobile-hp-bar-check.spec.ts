import { test, expect } from '@playwright/test';

test.describe('Mobile HP Bar Check', () => {
  test('Check HP bar visibility on mobile after game start', async ({ page }) => {
    test.setTimeout(180000); // 3 minutes for backend cold start
    
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Fill character creation
    await page.fill('input#characterName', 'TestHero');
    
    // Select race using custom select
    const raceSelect = page.locator('.custom-select-trigger').first();
    await raceSelect.click();
    await page.locator('.custom-select-option').first().click();
    
    // Select class using custom select  
    const classSelect = page.locator('.custom-select-trigger').nth(1);
    await classSelect.click();
    await page.locator('.custom-select-option').first().click();

    // Start game
    await page.click('button.btn-primary');

    // Wait for game to start
    await page.waitForSelector('.game-view', { timeout: 120000 });
    
    // Take screenshot after load
    await page.screenshot({ 
      path: 'screenshots/mobile-hp-bar-after-load.png',
      fullPage: true 
    });

    // Check if compact HP bar is visible
    const compactHPBar = page.locator('.compact-hp-bar');
    await expect(compactHPBar).toBeVisible({ timeout: 5000 });
    
    // Check if mobile navigation is visible
    const mobileNav = page.locator('.mobile-navigation');
    await expect(mobileNav).toBeVisible();
    
    // Check if game header is hidden
    const gameHeader = page.locator('.game-header');
    const headerVisible = await gameHeader.isVisible();
    expect(headerVisible).toBe(false);
    
    // Check HP bar content
    await expect(compactHPBar.locator('.compact-hp-name')).toBeVisible();
    await expect(compactHPBar.locator('.compact-hp-level')).toBeVisible();
    await expect(compactHPBar.locator('.compact-hp-bar-container')).toBeVisible();
    
    console.log('✅ HP bar is visible and contains all elements');
  });

  test('Desktop - HP bar should be hidden', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await page.screenshot({ 
      path: 'screenshots/desktop-no-hp-bar.png',
      fullPage: true 
    });

    // Compact HP bar should NOT be visible on desktop
    const compactHPBar = page.locator('.compact-hp-bar');
    const isVisible = await compactHPBar.isVisible();
    expect(isVisible).toBe(false);
  });
});
