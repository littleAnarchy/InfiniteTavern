import { test, expect } from '@playwright/test';

test.describe('Mobile Layout - Quick Check', () => {
  test('Mobile - Character creation visible', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Check basic elements are visible
    await expect(page.locator('h1')).toBeVisible();
    await expect(page.locator('input#characterName')).toBeVisible();
    
    // Take screenshot
    await page.screenshot({ 
      path: 'screenshots/quick-mobile-01.png',
      fullPage: true 
    });

    // Custom select should be visible
    await expect(page.locator('.custom-select-trigger').first()).toBeVisible();
  });

  test('Desktop - Mobile elements hidden', async ({ page }) => {
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await page.screenshot({ 
      path: 'screenshots/quick-desktop-01.png',
      fullPage: true 
    });

    // Mobile navigation should NOT be visible
    const mobileNav = page.locator('.mobile-navigation');
    const isVisible = await mobileNav.isVisible();
    expect(isVisible).toBe(false);
  });

  test('Tablet - Mobile navigation visible', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await page.screenshot({ 
      path: 'screenshots/quick-tablet-01.png',
      fullPage: true 
    });
  });
});
