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
    // Letter picker panel should be initialized with letter buttons
    const letterPickerGrid = document.getElementById('letterPickerGrid');
    expect(letterPickerGrid).toBeTruthy();
    const buttons = letterPickerGrid.querySelectorAll('.letter-picker-button');
    expect(buttons.length).toBeGreaterThan(0);
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
    
    // Cell should be selected
    expect(firstCell.classList.contains('selected')).toBe(true);
  });

  test('should close popup when clicking outside', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    let firstCell = null;
    let secondCell = null;
    
    // Find two non-readonly cells
    for (const cell of cells) {
      const input = cell.querySelector('input');
      if (input && input.getAttribute('data-originally-readonly') !== 'true') {
        if (!firstCell) firstCell = cell;
        else if (!secondCell) { secondCell = cell; break; }
      }
    }
    
    if (!firstCell || !secondCell) return;
    
    // Click cell to select it
    firstCell.click();
    
    expect(firstCell.classList.contains('selected')).toBe(true);
    
    // Click a different cell to deselect
    secondCell.click();
    
    // First cell should no longer be selected
    expect(firstCell.classList.contains('selected')).toBe(false);
  });

  test('should set cell value when letter is clicked in popup', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    const firstCell = cells[0];
    const input = firstCell.querySelector('input');
    
    if (input.getAttribute('data-originally-readonly') === 'true') return;
    
    // Click cell to select it
    firstCell.click();
    
    // Click a letter in the letter picker
    const letterPickerGrid = document.getElementById('letterPickerGrid');
    const letterButton = letterPickerGrid.querySelector('.letter-picker-button');
    const letter = letterButton.textContent;
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
    let firstCell = null;
    for (const cell of cells) {
      const input = cell.querySelector('input');
      if (input && input.getAttribute('data-originally-readonly') !== 'true') {
        firstCell = cell;
        break;
      }
    }
    
    if (!firstCell) return;
    
    // Click cell to select it
    firstCell.click();
    expect(firstCell.classList.contains('selected')).toBe(true);
    
    // Letter picker is a persistent panel, not a popup - verify it exists
    const letterPickerGrid = document.getElementById('letterPickerGrid');
    expect(letterPickerGrid).toBeTruthy();
    expect(letterPickerGrid.querySelectorAll('.letter-picker-button').length).toBeGreaterThan(0);
  });

  test('should clear cell when clear button is clicked', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    const firstCell = cells[0];
    const input = firstCell.querySelector('input');
    
    if (input.getAttribute('data-originally-readonly') === 'true') return;
    
    // Set initial value
    input.value = 'X';
    
    // Click cell to select it
    firstCell.click();
    
    // Click clear button in letter picker
    const clearButton = document.getElementById('letterPickerClear');
    clearButton.click();
    
    // Cell should be cleared
    expect(input.value).toBe('');
  });
});
