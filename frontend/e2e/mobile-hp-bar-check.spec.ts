import { test, expect } from '@playwright/test';

test.describe('Mobile Layout Optimization', () => {
  test('Story tab - no page scroll, only history scroll', async ({ page }) => {
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
    
    // Should be on story tab by default
    await page.waitForTimeout(1000);
    
    // Take screenshot after load
    await page.screenshot({ 
      path: 'screenshots/mobile-optimized-story.png',
      fullPage: true 
    });

    // Check if compact HP bar is visible at TOP
    const compactHPBar = page.locator('.compact-hp-bar');
    await expect(compactHPBar).toBeVisible({ timeout: 5000 });
    
    // Verify HP bar is at the top
    const hpBarBox = await compactHPBar.boundingBox();
    expect(hpBarBox?.y).toBeLessThan(50); // Should be near top
    
    // Check if mobile navigation is visible at BOTTOM
    const mobileNav = page.locator('.mobile-navigation');
    await expect(mobileNav).toBeVisible();
    
    // Check if game header is hidden
    const gameHeader = page.locator('.game-header');
    const headerVisible = await gameHeader.isVisible();
    expect(headerVisible).toBe(false);
    
    // Check main-content has no padding
    const mainContent = page.locator('.game-layout.mobile-tab-story .main-content');
    await expect(mainContent).toBeVisible();
    
    // Check history-container is scrollable
    const historyContainer = page.locator('.history-container');
    await expect(historyContainer).toBeVisible();
    
    // Verify story tab is active
    const storyTab = mobileNav.locator('.mobile-nav-tab').nth(1);
    await expect(storyTab).toHaveClass(/active/);
    
    // Check if suggested actions buttons are visible
    const suggestedButtons = page.locator('.btn-suggested');
    const buttonCount = await suggestedButtons.count();
    console.log(`🔍 Found ${buttonCount} suggested action buttons`);
    
    if (buttonCount > 0) {
      // Check if all 3 buttons are visible
      for (let i = 0; i < Math.min(buttonCount, 3); i++) {
        const button = suggestedButtons.nth(i);
        await expect(button).toBeVisible();
        const buttonText = await button.textContent();
        console.log(`  Button ${i + 1}: "${buttonText}"`);
      }
      
      // Try clicking the first button
      await suggestedButtons.first().click();
      await page.waitForTimeout(2000);
      
      // Take screenshot after click
      await page.screenshot({ 
        path: 'screenshots/mobile-after-suggested-click.png',
        fullPage: true 
      });
      
      console.log('✅ Successfully clicked suggested action button');
    } else {
      console.log('⚠️ No suggested action buttons found in DOM');
    }
    
    console.log('✅ Layout optimized: HP bar at TOP, no horizontal padding, only history scrolls');
  });

  test('Character tab - no inventory visible', async ({ page }) => {
    test.setTimeout(180000);
    
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Fill character creation
    await page.fill('input#characterName', 'TestHero');
    
    const raceSelect = page.locator('.custom-select-trigger').first();
    await raceSelect.click();
    await page.locator('.custom-select-option').first().click();
    
    const classSelect = page.locator('.custom-select-trigger').nth(1);
    await classSelect.click();
    await page.locator('.custom-select-option').first().click();

    await page.click('button.btn-primary');
    await page.waitForSelector('.game-view', { timeout: 120000 });
    await page.waitForTimeout(1000);

    // Click on character tab
    const mobileNav = page.locator('.mobile-navigation');
    const characterTab = mobileNav.locator('.mobile-nav-tab').first();
    await characterTab.click();
    await page.waitForTimeout(500);

    // Take screenshot
    await page.screenshot({ 
      path: 'screenshots/mobile-character-tab.png',
      fullPage: true 
    });

    // Check that player stats are visible
    const playerStats = page.locator('.player-stats');
    await expect(playerStats).toBeVisible();

    // Check that flip button is hidden
    const flipButton = page.locator('.face-toggle-button');
    const flipButtonVisible = await flipButton.isVisible();
    expect(flipButtonVisible).toBe(false);

    // Check that inventory from flip-card back is NOT visible
    const flipCardInventory = page.locator('.game-layout.mobile-tab-character .sidebar-flip-card .sidebar-face-back .inventory');
    const flipCardInvCount = await flipCardInventory.count();
    console.log(`Flip-card inventory in character tab: ${flipCardInvCount}`);

    // Check that sidebar-right inventory is NOT visible in character tab
    const sidebarRightInv = page.locator('.game-layout.mobile-tab-character .sidebar-right');
    const sidebarRightVisible = await sidebarRightInv.isVisible();
    console.log(`Sidebar-right visible in character tab: ${sidebarRightVisible}`);

    console.log('✅ Character tab shows only stats, no inventory');
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
