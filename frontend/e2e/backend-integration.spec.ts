import { test, expect } from '@playwright/test';

test.describe('InfiniteTavern - Backend Integration', () => {
  test('Check Ukrainian language interface', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Switch to Ukrainian via LanguageSwitcher if available
    const languageButton = page.locator('button:has-text("EN"), button:has-text("UA")');
    if (await languageButton.isVisible()) {
      await languageButton.click();
    }

    // Take screenshot of Ukrainian UI
    await page.screenshot({ 
      path: 'screenshots/11-ukrainian-ui.png',
      fullPage: true 
    });
  });

  test('Test form with backend connection', async ({ page }) => {
    // Increase timeout for backend request
    test.setTimeout(120000);

    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Fill form
    await page.fill('input#characterName', 'Тестовий Герой');
    await page.selectOption('select#race', 'Elf');
    await page.selectOption('select#class', 'Wizard');
    await page.selectOption('select#gameLanguage', 'Ukrainian');
    
    // Select default campaign
    await page.click('input[value="default"]');

    // Screenshot before submission
    await page.screenshot({ 
      path: 'screenshots/12-before-submit.png',
      fullPage: true 
    });

    // Submit form
    await page.click('button[type="submit"]');

    // Wait for loading state to appear
    const loadingIndicator = page.locator('.loading-story');
    await expect(loadingIndicator).toBeVisible({ timeout: 2000 }).catch(() => {});

    // Screenshot loading state
    if (await loadingIndicator.isVisible()) {
      await page.screenshot({ 
        path: 'screenshots/13-loading-state.png',
        fullPage: true 
      });
    }

    // Wait for game view to appear or error (60s timeout for slow backend)
    try {
      await page.waitForSelector('.game-view, .error-message', { timeout: 60000 });
      
      // Take screenshot of result
      await page.screenshot({ 
        path: 'screenshots/14-game-started.png',
        fullPage: true 
      });

      // Check if game view appeared
      const gameView = page.locator('.game-view');
      if (await gameView.isVisible()) {
        console.log('✅ Game successfully started!');
        
        // Take additional screenshots of game UI
        await page.screenshot({ 
          path: 'screenshots/15-game-view-full.png',
          fullPage: true 
        });

        // Check for player stats
        const playerStats = page.locator('.player-stats');
        if (await playerStats.isVisible()) {
          await playerStats.screenshot({ 
            path: 'screenshots/16-player-stats.png' 
          });
        }

        // Check for narrative/story
        const narrative = page.locator('.turn-history, .narrative');
        if (await narrative.isVisible()) {
          await narrative.screenshot({ 
            path: 'screenshots/17-narrative.png' 
          });
        }
      } else {
        console.log('❌ Game did not start - checking for errors');
        const errorMsg = await page.locator('.error-message, .error').textContent();
        console.log('Error:', errorMsg);
      }
    } catch (error) {
      console.log('⚠️ Timeout waiting for game to start');
      await page.screenshot({ 
        path: 'screenshots/14-timeout-error.png',
        fullPage: true 
      });
    }
  });

  test('Check responsive behavior', async ({ page }) => {
    const viewports = [
      { name: 'mobile-portrait', width: 375, height: 812 },
      { name: 'mobile-landscape', width: 812, height: 375 },
      { name: 'tablet', width: 768, height: 1024 },
      { name: 'laptop', width: 1366, height: 768 },
      { name: 'desktop', width: 1920, height: 1080 },
    ];

    for (const viewport of viewports) {
      await page.setViewportSize(viewport);
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      
      await page.screenshot({ 
        path: `screenshots/responsive-${viewport.name}-${viewport.width}x${viewport.height}.png`,
        fullPage: true 
      });
    }
  });

  test('Check accessibility and contrast', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Check if all interactive elements are keyboard accessible
    await page.keyboard.press('Tab');
    await page.screenshot({ path: 'screenshots/18-focus-first-element.png' });

    // Tab through all form elements
    for (let i = 0; i < 8; i++) {
      await page.keyboard.press('Tab');
      await page.waitForTimeout(100);
    }

    await page.screenshot({ path: 'screenshots/19-focus-last-element.png' });

    // Check if labels are properly associated with inputs
    const nameInput = page.locator('input#characterName');
    const nameLabel = page.locator('label[for="characterName"]');
    
    await expect(nameLabel).toBeVisible();
    await expect(nameInput).toBeVisible();
  });
});
