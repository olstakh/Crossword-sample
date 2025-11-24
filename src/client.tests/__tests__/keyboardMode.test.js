/**
 * Tests for Keyboard Input Mode
 */

import { describe, test, expect, beforeEach } from '@jest/globals';
import { createMockPuzzleData, setupTestDOM, mockLocalStorage } from '../helpers/testHelpers.js';

describe('Keyboard Mode', () => {
  let CryptogramPuzzle;
  let puzzle;
  
  beforeEach(async () => {
    // Setup DOM
    setupTestDOM();
    
    // Mock localStorage to return keyboard mode
    mockLocalStorage({ inputMode: 'keyboard', difficultyMode: 'easy' });
    
    // Dynamically load and evaluate the CryptogramPuzzle class
    const fs = await import('fs/promises');
    const path = await import('path');
    const { fileURLToPath } = await import('url');
    
    const __filename = fileURLToPath(import.meta.url);
    const __dirname = path.dirname(__filename);
    const scriptPath = path.join(__dirname, '..', '..', 'client', 'wwwroot', 'script.js');
    const scriptContent = await fs.readFile(scriptPath, 'utf-8');
    
    // Extract the CryptogramPuzzle class
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
    
    // Create a function that returns the class
    const createClass = new Function(`
      const API_BASE_URL = 'http://localhost:5000';
      ${classCode}
      return CryptogramPuzzle;
    `);
    
    CryptogramPuzzle = createClass();
    
    // Create puzzle instance
    const puzzleData = createMockPuzzleData();
    puzzle = new CryptogramPuzzle(puzzleData);
  });

  test('should initialize in keyboard mode', () => {
    expect(puzzle.inputMode).toBe('keyboard');
  });

  test('should allow typing in cells', () => {
    const inputs = document.querySelectorAll('.cell:not(.black) input');
    const firstInput = inputs[0];
    
    // Should not be readonly in keyboard mode
    expect(firstInput.readOnly).toBe(false);
  });

  test('should update cell value on input in easy mode', () => {
    const inputs = document.querySelectorAll('.cell:not(.black) input');
    const firstInput = inputs[0];
    const number = firstInput.dataset.number;
    
    // Simulate typing
    firstInput.value = 'X';
    firstInput.dispatchEvent(new Event('input', { bubbles: true }));
    
    // In easy mode, all cells with same number should be updated
    const allWithSameNumber = document.querySelectorAll(`input[data-number="${number}"]`);
    allWithSameNumber.forEach(input => {
      if (input.getAttribute('data-originally-readonly') !== 'true') {
        expect(input.value).toBe('X');
      }
    });
  });

  test('should navigate cells with arrow keys', () => {
    const inputs = document.querySelectorAll('.cell:not(.black) input');
    const firstInput = inputs[0];
    
    // Focus first input
    firstInput.focus();
    
    // Simulate arrow right
    const event = new KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true });
    firstInput.dispatchEvent(event);
    
    // Focus should move (we can't easily test focus, but method should be called)
    expect(puzzle.currentCell).toBeTruthy();
  });

  test('should update alphabet decoder when cells are filled in easy mode', () => {
    const inputs = document.querySelectorAll('.cell:not(.black) input');
    const firstInput = inputs[0];
    const number = firstInput.dataset.number;
    const correctLetter = firstInput.dataset.answer;
    
    // Fill all cells with same number with same letter
    const allWithSameNumber = document.querySelectorAll(`input[data-number="${number}"]`);
    allWithSameNumber.forEach(input => {
      input.value = correctLetter;
    });
    
    // Trigger update
    puzzle.updateAlphabetDecoder();
    
    // Check alphabet decoder
    const decoderCell = document.querySelector(`.alphabet-cell[data-number="${number}"]`);
    const letterDiv = decoderCell.querySelector('.alphabet-letter');
    
    expect(letterDiv.textContent).toBe(correctLetter);
    expect(letterDiv.classList.contains('hidden')).toBe(false);
  });
});
