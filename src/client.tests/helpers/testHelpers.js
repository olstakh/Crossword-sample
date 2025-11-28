// Test helper to create puzzle data
export function createMockPuzzleData(overrides = {}) {
  return {
    id: 'test-puzzle',
    title: 'Test Puzzle',
    language: 'English',
    size: { rows: 3, cols: 3 },
    grid: [
      ['C', 'A', 'T'],
      ['O', '#', 'O'],
      ['D', 'O', 'G']
    ],
    ...overrides
  };
}

// Helper to configure localStorage mock
export function mockLocalStorage(values = {}) {
  const defaultValues = {
    inputMode: 'keyboard',
    difficultyMode: 'easy',
    ...values
  };
  
  global.localStorage.getItem = (key) => defaultValues[key] || null;
}

// Load the CryptogramPuzzle class from the client code
export async function loadPuzzleClass() {
  const fs = await import('fs');
  const path = await import('path');
  const { fileURLToPath } = await import('url');
  
  const __filename = fileURLToPath(import.meta.url);
  const __dirname = path.dirname(__filename);
  const scriptPath = path.join(__dirname, '..', 'client', 'wwwroot', 'script.js');
  const scriptContent = fs.readFileSync(scriptPath, 'utf-8');
  
  // Extract just the CryptogramPuzzle class
  const classMatch = scriptContent.match(/class CryptogramPuzzle \{[\s\S]*?\n\}/);
  if (!classMatch) {
    throw new Error('Could not extract CryptogramPuzzle class');
  }
  
  // Create a minimal context with required globals
  const context = {
    window: global.window,
    document: global.document,
    localStorage: global.localStorage,
    API_BASE_URL: 'http://localhost:5000'
  };
  
  // Evaluate the class in our context
  const classCode = `
    ${classMatch[0]}
    CryptogramPuzzle;
  `;
  
  const func = new Function('window', 'document', 'localStorage', 'API_BASE_URL', classCode);
  return func(context.window, context.document, context.localStorage, context.API_BASE_URL);
}

// Setup DOM for testing
export function setupTestDOM() {
  document.body.innerHTML = `
    <div class="container">
      <header>
        <div class="header-content">
          <h1>Cryptogram Puzzle</h1>
          <div id="userStats" class="user-stats"></div>
        </div>
        <div class="toggles-container">
          <div class="input-mode-toggle">
            <label class="toggle-label">
              <span class="toggle-text">⌨️ Keyboard</span>
              <input type="checkbox" id="inputModeToggle" class="toggle-checkbox">
              <span class="toggle-slider"></span>
              <span class="toggle-text">🖱️ Mouse</span>
            </label>
          </div>
          <div class="difficulty-mode-toggle">
            <label class="toggle-label">
              <span class="toggle-text">😊 Easy</span>
              <input type="checkbox" id="difficultyModeToggle" class="toggle-checkbox">
              <span class="toggle-slider difficulty-slider"></span>
              <span class="toggle-text">💪 Hard</span>
            </label>
          </div>
        </div>
        <div class="controls">
          <button id="checkBtn">Check Answers</button>
          <button id="revealBtn">Reveal Solution</button>
          <button id="clearBtn">Clear Grid</button>
        </div>
      </header>
      <main>
        <div class="puzzle-section">
          <div id="crossword-grid"></div>
          <div class="alphabet-decoder">
            <h2>Letter Decoder</h2>
            <div id="alphabet-row"></div>
          </div>
        </div>
      </main>
    </div>
  `;
}
