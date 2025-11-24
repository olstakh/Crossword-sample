/**
 * Tests for Hard Difficulty Mode
 */

import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { createMockPuzzleData, setupTestDOM, mockLocalStorage } from '../helpers/testHelpers.js';

describe('Hard Mode', () => {
  let CryptogramPuzzle;
  let puzzle;
  
  beforeEach(async () => {
    setupTestDOM();
    
    mockLocalStorage({ inputMode: 'keyboard', difficultyMode: 'hard' });
    
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

  test('should initialize in hard mode', () => {
    expect(puzzle.difficultyMode).toBe('hard');
  });

  test('should update only current cell in keyboard mode', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    let inputCell = null;
    let number = null;
    
    // Find first non-readonly cell
    for (const cell of cells) {
      const input = cell.querySelector('input');
      if (input && input.getAttribute('data-originally-readonly') !== 'true') {
        inputCell = input;
        number = input.dataset.number;
        break;
      }
    }
    
    if (!inputCell) return;
    
    // Type a letter
    inputCell.value = 'X';
    inputCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Only the current cell should be updated
    expect(inputCell.value).toBe('X');
    
    // Other cells with same number should NOT be updated
    const allWithSameNumber = document.querySelectorAll(`input[data-number="${number}"]`);
    let otherCells = 0;
    allWithSameNumber.forEach(cell => {
      if (cell !== inputCell && cell.getAttribute('data-originally-readonly') !== 'true') {
        expect(cell.value).toBe('');
        otherCells++;
      }
    });
    
    // Ensure there was at least one other cell to verify
    expect(otherCells).toBeGreaterThan(0);
  });

  test('should allow different letters in cells with same number', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    let firstCell = null;
    let secondCell = null;
    let number = null;
    
    // Find two cells with same number
    for (const cell of cells) {
      const input = cell.querySelector('input');
      if (input && input.getAttribute('data-originally-readonly') !== 'true') {
        const cellNumber = input.dataset.number;
        
        if (!firstCell) {
          firstCell = input;
          number = cellNumber;
        } else if (cellNumber === number && !secondCell) {
          secondCell = input;
          break;
        }
      }
    }
    
    if (!firstCell || !secondCell) return;
    
    // Type different letters
    firstCell.value = 'X';
    firstCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    secondCell.value = 'Y';
    secondCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Both should retain their different values
    expect(firstCell.value).toBe('X');
    expect(secondCell.value).toBe('Y');
  });

  test('should not update Letter Decoder when cells with same number have different values', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    let firstCell = null;
    let secondCell = null;
    let number = null;
    
    // Find two cells with same number
    for (const cell of cells) {
      const input = cell.querySelector('input');
      if (input && input.getAttribute('data-originally-readonly') !== 'true') {
        const cellNumber = input.dataset.number;
        
        if (!firstCell) {
          firstCell = input;
          number = cellNumber;
        } else if (cellNumber === number && !secondCell) {
          secondCell = input;
          break;
        }
      }
    }
    
    if (!firstCell || !secondCell) return;
    
    // Type different letters
    firstCell.value = 'X';
    firstCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    secondCell.value = 'Y';
    secondCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Letter Decoder should not show a letter (cells don't match)
    const decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe('');
    }
  });

  test('should update Letter Decoder when all cells with same number match', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    const cellsByNumber = {};
    
    // Group cells by number
    cells.forEach(cell => {
      const input = cell.querySelector('input');
      if (input && input.getAttribute('data-originally-readonly') !== 'true') {
        const number = input.dataset.number;
        if (!cellsByNumber[number]) cellsByNumber[number] = [];
        cellsByNumber[number].push(input);
      }
    });
    
    // Find a number with multiple cells
    let number = null;
    let cellsToUpdate = null;
    for (const [num, cells] of Object.entries(cellsByNumber)) {
      if (cells.length > 1) {
        number = num;
        cellsToUpdate = cells;
        break;
      }
    }
    
    if (!cellsToUpdate) return;
    
    // Type same letter in all cells manually (simulating hard mode behavior)
    cellsToUpdate.forEach(cell => {
      cell.value = 'X';
      cell.dispatchEvent(new Event('input', { bubbles: true }));
    });
    
    // Now Letter Decoder should show the letter
    const decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe('X');
    }
  });

  test('should clear all cells independently', () => {
    const cells = document.querySelectorAll('.cell:not(.black)');
    let firstCell = null;
    let secondCell = null;
    
    // Find two cells with same number
    for (const cell of cells) {
      const input = cell.querySelector('input');
      if (input && input.getAttribute('data-originally-readonly') !== 'true') {
        if (!firstCell) {
          firstCell = input;
        } else if (input.dataset.number === firstCell.dataset.number && !secondCell) {
          secondCell = input;
          break;
        }
      }
    }
    
    if (!firstCell || !secondCell) return;
    
    // Set different values
    firstCell.value = 'X';
    firstCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    secondCell.value = 'Y';
    secondCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Clear grid
    puzzle.clearGrid();
    
    // Both should be empty
    expect(firstCell.value).toBe('');
    expect(secondCell.value).toBe('');
  });
});
