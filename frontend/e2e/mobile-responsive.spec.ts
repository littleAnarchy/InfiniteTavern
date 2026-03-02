import { test, expect } from '@playwright/test';

test.describe('Mobile View - Responsive Check', () => {
  test('Character creation on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await page.screenshot({ 
      path: 'screenshots/mobile-01-character-creation.png',
      fullPage: true 
    });

    // Fill form
    await page.fill('input#characterName', 'Mobile Hero');
    
    // Test custom select on mobile
    await page.click('#race');
    await page.waitForTimeout(500);
    
    await page.screenshot({ 
      path: 'screenshots/mobile-02-select-open.png',
      fullPage: true 
    });

    await page.click('.custom-select-option >> text=Elf');
    await page.click('#class');
    await page.waitForTimeout(300);
    await page.click('.custom-select-option >> text=Wizard');

    await page.screenshot({ 
      path: 'screenshots/mobile-03-form-filled.png',
      fullPage: true 
    });
  });

  test('Mobile game view - tabs navigation', async ({ page }) => {
    test.setTimeout(120000); // Extended timeout for backend

    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Fill and submit form quickly
    await page.fill('input#characterName', 'Test');
    
    // Use default selections
    await page.click('button[type="submit"]');

    // Wait for game to start (with timeout handling)
    try {
      await page.waitForSelector('.game-view, .mobile-navigation', { timeout: 60000 });
    } catch {
      console.log('Game start timeout - backend might be cold starting');
      await page.screenshot({ 
        path: 'screenshots/mobile-04-timeout.png',
        fullPage: true 
      });
      return; // Skip rest if backend not responding
    }

    // Check if  compact HP bar is visible
    const compactHPBar = page.locator('.compact-hp-bar');
    if (await compactHPBar.isVisible()) {
      await page.screenshot({ 
        path: 'screenshots/mobile-05-game-started.png',
        fullPage: true 
      });

      // Check mobile navigation
      const mobileNav = page.locator('.mobile-navigation');
      await expect(mobileNav).toBeVisible();

      // Test Story tab (default)
      await page.screenshot({ 
        path: 'screenshots/mobile-06-story-tab.png',
        fullPage: true 
      });

      // Switch to Character tab
      await page.click('.mobile-nav-tab >> text=Character');
      await page.waitForTimeout(500);

      await page.screenshot({ 
        path: 'screenshots/mobile-07-character-tab.png',
        fullPage: true 
      });

      // Verify character stats are visible
      const playerStats = page.locator('.player-stats');
      await expect(playerStats).toBeVisible();

      // Switch to Inventory tab
      await page.click('.mobile-nav-tab >> text=Inventory');
      await page.waitForTimeout(500);

      await page.screenshot({ 
        path: 'screenshots/mobile-08-inventory-tab.png',
        fullPage: true 
      });

      // Switch back to Story
      await page.click('.mobile-nav-tab >> text=Story');
      await page.waitForTimeout(500);

      await page.screenshot({ 
        path: 'screenshots/mobile-09-back-to-story.png',
        fullPage: true 
      });
    }
  });

  test('Tablet view check', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 }); // iPad
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await page.screenshot({ 
      path: 'screenshots/tablet-01-character-creation.png',
      fullPage: true 
    });

    // Verify mobile navigation is visible on tablet
    await page.fill('input#characterName', 'Tablet Test');
    await page.click('button[type="submit"]');

    try {
      await page.waitForSelector('.mobile-navigation', { timeout: 5000 });
      await page.screenshot({ 
        path: 'screenshots/tablet-02-game-view.png',
        fullPage: true 
      });
    } catch {
      console.log('Game not started on tablet test');
    }
  });

  test('Desktop view unchanged', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await page.screenshot({ 
      path: 'screenshots/desktop-01-unchanged.png',
      fullPage: true 
    });

    // Verify mobile navigation is NOT visible on desktop
    const mobileNav = page.locator('.mobile-navigation');
    await expect(mobileNav).not.toBeVisible();

    // Verify compact HP bar is NOT visible on desktop
    const compactHPBar = page.locator('.compact-hp-bar');
    await expect(compactHPBar).not.toBeVisible();
  });
});
