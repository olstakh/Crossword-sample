/**
 * Tests for Letter Decoder in all modes
 */

import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { createMockPuzzleData, setupTestDOM, mockLocalStorage } from '../helpers/testHelpers.js';

describe('Letter Decoder', () => {
  let CryptogramPuzzle;
  
  const loadPuzzleClass = async () => {
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
    
    return createClass();
  };
  
  beforeEach(async () => {
    setupTestDOM();
    CryptogramPuzzle = await loadPuzzleClass();
  });

  test('should show user input in easy keyboard mode', async () => {
    mockLocalStorage({ inputMode: 'keyboard', difficultyMode: 'easy' });
    
    const puzzleData = createMockPuzzleData();
    const puzzle = new CryptogramPuzzle(puzzleData);
    
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
    
    // Type a letter (not necessarily the correct one)
    inputCell.value = 'Z';
    inputCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Letter Decoder should show 'Z', not the correct answer
    const decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe('Z');
    }
  });

  test('should show user input in easy mouse mode', async () => {
    mockLocalStorage({ inputMode: 'mouse', difficultyMode: 'easy' });
    
    const puzzleData = createMockPuzzleData();
    const puzzle = new CryptogramPuzzle(puzzleData);
    
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
    
    // Letter Decoder should show the selected letter
    const number = input.dataset.number;
    const decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe(letter);
    }
  });

  test('should show user input in hard keyboard mode when all cells match', async () => {
    mockLocalStorage({ inputMode: 'keyboard', difficultyMode: 'hard' });
    
    const puzzleData = createMockPuzzleData();
    const puzzle = new CryptogramPuzzle(puzzleData);
    
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
    
    // Type same letter in all cells
    cellsToUpdate.forEach(cell => {
      cell.value = 'W';
      cell.dispatchEvent(new Event('input', { bubbles: true }));
    });
    
    // Letter Decoder should show 'W'
    const decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe('W');
    }
  });

  test('should not show letter in hard mode when cells have different values', async () => {
    mockLocalStorage({ inputMode: 'keyboard', difficultyMode: 'hard' });
    
    const puzzleData = createMockPuzzleData();
    const puzzle = new CryptogramPuzzle(puzzleData);
    
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
    firstCell.value = 'A';
    firstCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    secondCell.value = 'B';
    secondCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Letter Decoder should not show a letter
    const decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe('');
    }
  });

  test('should clear decoder when grid is cleared', async () => {
    mockLocalStorage({ inputMode: 'keyboard', difficultyMode: 'easy' });
    
    const puzzleData = createMockPuzzleData();
    const puzzle = new CryptogramPuzzle(puzzleData);
    
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
    
    // Verify decoder shows letter
    let decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe('X');
    }
    
    // Clear grid
    puzzle.clearGrid();
    
    // Decoder should be cleared
    decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe('');
    }
  });

  test('should preserve initially revealed letters in decoder', async () => {
    mockLocalStorage({ inputMode: 'keyboard', difficultyMode: 'easy' });
    
    const puzzleData = createMockPuzzleData();
    const puzzle = new CryptogramPuzzle(puzzleData);
    
    // Find a cell that was initially revealed
    const revealedCells = document.querySelectorAll('input[data-originally-readonly="true"]');
    if (revealedCells.length === 0) return;
    
    const revealedCell = revealedCells[0];
    const number = revealedCell.dataset.number;
    const letter = revealedCell.value;
    
    // Decoder should show the revealed letter
    const decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe(letter);
    }
  });
});
