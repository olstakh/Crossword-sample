/**
 * Tests for Mouse Input Mode
 */

import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { createMockPuzzleData, setupTestDOM, mockLocalStorage } from '../helpers/testHelpers.js';

describe('Mouse Mode', () => {
  let CryptogramPuzzle;
  let puzzle;
  
  beforeEach(async () => {
    setupTestDOM();
    
    mockLocalStorage({ inputMode: 'mouse', difficultyMode: 'easy' });
    
    // Load CryptogramPuzzle class
    const fs = await import('fs/promises');
    const path = await import('path');
    const { fileURLToPath } = await import('url');
    
    const __filename = fileURLToPath(import.meta.url);
    const __dirname = path.dirname(__filename);
    const scriptPath = path.join(__dirname, '..', '..', 'client', 'wwwroot', 'script.js');
    const scriptContent = await fs.readFile(scriptPath, 'utf-8');
    
    const classStart = scriptContent.indexOf('class CryptogramPuzzle');
    const classBody = scriptContent.substring(classStart);
    const braceCount = { open: 0, close: 0, started: false };
    let classEnd = 0;
    
    for (let i = 0; i < classBody.length; i++) {
      if (classBody[i] === '{') {
        braceCount.open++;
        braceCount.started = true;
      } else if (classBody[i] === '}') {
        braceCount.close++;
        if (braceCount.started && braceCount.open === braceCount.close) {
          classEnd = classStart + i + 1;
          break;
        }
      }
    }
    
    const classCode = scriptContent.substring(classStart, classEnd);
    const createClass = new Function(`
      const API_BASE_URL = 'http://localhost:5000';
      ${classCode}
      return CryptogramPuzzle;
    `);
    
    CryptogramPuzzle = createClass();
    const puzzleData = createMockPuzzleData();
    puzzle = new CryptogramPuzzle(puzzleData);
  });

  test('should initialize in mouse mode', () => {
    expect(puzzle.inputMode).toBe('mouse');
  });

  test('should make inputs readonly in mouse mode', () => {
    const inputs = document.querySelectorAll('.cell:not(.black) input');
    const nonReadonlyInputs = Array.from(inputs).filter(
      input => input.getAttribute('data-originally-readonly') !== 'true'
    );
    
    nonReadonlyInputs.forEach(input => {
      expect(input.readOnly).toBe(true);
      expect(input.style.cursor).toBe('pointer');
    });
  });

  test('should show popup when cell is clicked', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    const firstCell = cells[0];
    const input = firstCell.querySelector('input');
    
    // Ensure it's not originally readonly
    if (input.getAttribute('data-originally-readonly') === 'true') return;
    
    // Click the cell
    firstCell.click();
    
    // Popup should be created
    const popup = document.querySelector('.letter-popup-overlay');
    expect(popup).toBeTruthy();
  });

  test('should close popup when clicking outside', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    const firstCell = cells[0];
    
    // Click cell to open popup
    firstCell.click();
    
    const popup = document.querySelector('.letter-popup-overlay');
    expect(popup).toBeTruthy();
    
    // Click the overlay
    popup.click();
    
    // Popup should be removed
    expect(document.querySelector('.letter-popup-overlay')).toBeFalsy();
  });

  test('should set cell value when letter is clicked in popup', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    const firstCell = cells[0];
    const input = firstCell.querySelector('input');
    
    if (input.getAttribute('data-originally-readonly') === 'true') return;
    
    // Click cell to open popup
    firstCell.click();
    
    const popup = document.querySelector('.letter-popup-overlay');
    const letterButton = popup.querySelector('.letter-popup-button');
    const letter = letterButton.textContent;
    
    // Click letter button
    letterButton.click();
    
    // In easy mode, all cells with same number should be updated
    const number = input.dataset.number;
    const allWithSameNumber = document.querySelectorAll(`input[data-number="${number}"]`);
    
    allWithSameNumber.forEach(cell => {
      if (cell.getAttribute('data-originally-readonly') !== 'true') {
        expect(cell.value).toBe(letter);
      }
    });
  });

  test('should close popup after letter selection', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    const firstCell = cells[0];
    
    // Click cell to open popup
    firstCell.click();
    
    const popup = document.querySelector('.letter-popup-overlay');
    const letterButton = popup.querySelector('.letter-popup-button');
    
    // Click letter button
    letterButton.click();
    
    // Popup should be removed
    expect(document.querySelector('.letter-popup-overlay')).toBeFalsy();
  });

  test('should clear cell when clear button is clicked', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    const firstCell = cells[0];
    const input = firstCell.querySelector('input');
    
    if (input.getAttribute('data-originally-readonly') === 'true') return;
    
    // Set initial value
    input.value = 'X';
    
    // Click cell to open popup
    firstCell.click();
    
    const popup = document.querySelector('.letter-popup-overlay');
    const clearButton = popup.querySelector('.letter-popup-clear');
    
    // Click clear button
    clearButton.click();
    
    // Cell should be cleared
    expect(input.value).toBe('');
  });
});
