/**
 * Tests for Easy Difficulty Mode
 */

import { describe, test, expect, beforeEach, jest } from '@jest/globals';
import { createMockPuzzleData, setupTestDOM, mockLocalStorage } from '../helpers/testHelpers.js';

describe('Easy Mode', () => {
  let CryptogramPuzzle;
  let puzzle;
  
  beforeEach(async () => {
    setupTestDOM();
    
    mockLocalStorage({ inputMode: 'keyboard', difficultyMode: 'easy' });
    
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

  test('should initialize in easy mode', () => {
    expect(puzzle.difficultyMode).toBe('easy');
  });

  test('should update all cells with same number in keyboard mode', () => {
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
    
    // All cells with same number should be updated
    const allWithSameNumber = document.querySelectorAll(`input[data-number="${number}"]`);
    allWithSameNumber.forEach(cell => {
      if (cell.getAttribute('data-originally-readonly') !== 'true') {
        expect(cell.value).toBe('X');
      }
    });
  });

  test('should update all cells with same number in mouse mode', () => {
    // Switch to mouse mode
    puzzle.setInputMode('mouse');
    
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
    
    // All cells with same number should be updated
    const number = input.dataset.number;
    const allWithSameNumber = document.querySelectorAll(`input[data-number="${number}"]`);
    
    allWithSameNumber.forEach(cell => {
      if (cell.getAttribute('data-originally-readonly') !== 'true') {
        expect(cell.value).toBe(letter);
      }
    });
  });

  test('should update Letter Decoder when all cells with same number match', () => {
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
    
    // Get the letter for this number from the mapping
    const letter = puzzle.letterMapping[number];
    
    // Type the correct letter
    inputCell.value = letter;
    inputCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Letter Decoder should show the letter
    const decoderCell = document.querySelector(`.alphabet-decoder-cell[data-number="${number}"] .letter`);
    if (decoderCell) {
      expect(decoderCell.textContent).toBe(letter);
    }
  });

  test('should clear all cells with same number when clearing', () => {
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
    
    // Set values
    inputCell.value = 'X';
    inputCell.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Verify all cells have value
    const allWithSameNumber = document.querySelectorAll(`input[data-number="${number}"]`);
    allWithSameNumber.forEach(cell => {
      if (cell.getAttribute('data-originally-readonly') !== 'true') {
        expect(cell.value).toBe('X');
      }
    });
    
    // Clear grid
    puzzle.clearGrid();
    
    // All cells should be empty
    allWithSameNumber.forEach(cell => {
      if (cell.getAttribute('data-originally-readonly') !== 'true') {
        expect(cell.value).toBe('');
      }
    });
  });
});
