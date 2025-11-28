# Client-Side Test Suite

This directory contains comprehensive Jest tests for the client-side JavaScript functionality of the cryptogram puzzle application.

## Prerequisites

To run these tests, you need to have Node.js and npm installed:

1. **Install Node.js**: Download and install from [nodejs.org](https://nodejs.org/)
   - Recommended version: Node.js 18.x or later
   - This will also install npm (Node Package Manager)

2. **Verify Installation**:
   ```powershell
   node --version
   npm --version
   ```

## Setup

1. **Install Dependencies**:
   ```powershell
   cd src\client.tests
   npm install
   ```

   This will install:
   - `jest`: Testing framework
   - `@testing-library/dom`: DOM testing utilities
   - `@testing-library/jest-dom`: Custom matchers for DOM assertions
   - `jest-environment-jsdom`: Browser-like environment for tests

## Running Tests

### Run All Tests
```powershell
npm test
```

### Run Specific Test Suite
```powershell
npm test keyboardMode.test.js
npm test mouseMode.test.js
npm test easyMode.test.js
npm test hardMode.test.js
npm test letterDecoder.test.js
npm test errorHandling.test.js
```

### Run Tests in Watch Mode (auto-rerun on file changes)
```powershell
npm test -- --watch
```

### Run Tests with Coverage Report
```powershell
npm test -- --coverage
```

## Test Structure

### Test Files

- **`keyboardMode.test.js`**: Tests keyboard input functionality
  - Initialization
  - Readonly state management
  - Input handling in easy mode
  - Arrow key navigation
  - Alphabet decoder updates

- **`mouseMode.test.js`**: Tests mouse input functionality
  - Popup display and interactions
  - Letter selection from alphabet
  - Clear button functionality
  - Popup closing behavior

- **`easyMode.test.js`**: Tests easy difficulty mode
  - All cells with same number sync in keyboard mode
  - All cells with same number sync in mouse mode
  - Letter Decoder updates when cells match
  - Clear grid behavior

- **`hardMode.test.js`**: Tests hard difficulty mode
  - Independent cell updates
  - Different letters allowed in cells with same number
  - Letter Decoder only updates when all cells match
  - Individual cell clearing

- **`letterDecoder.test.js`**: Tests Letter Decoder in all modes
  - Shows user input (not correct answer)
  - Works in easy keyboard mode
  - Works in easy mouse mode
  - Works in hard mode when cells match
  - Doesn't show letter when cells don't match
  - Preserves initially revealed letters
  - Clears when grid is cleared

- **`errorHandling.test.js`**: Tests error handling and edge cases
  - Network errors
  - HTTP error responses (404, 500)
  - Invalid JSON responses
  - Missing or empty grid data
  - Puzzle check success/failure
  - Invalid language fallback

### Helper Functions (`helpers/testHelpers.js`)

- `createMockPuzzleData()`: Creates mock puzzle data for testing
- `setupTestDOM()`: Sets up the required DOM structure for tests
- `loadPuzzleClass()`: Dynamically loads the CryptogramPuzzle class from the source file

## Test Configuration

- **`jest.config.js`**: Jest configuration
  - Uses `jsdom` environment to simulate browser
  - Runs setup file before each test
  - Collects coverage from `script.js`

- **`jest.setup.js`**: Test environment setup
  - Mocks `localStorage`
  - Mocks `fetch` for API calls
  - Imports custom matchers from `@testing-library/jest-dom`

## Coverage

After running tests with coverage, you can view the report in:
- Terminal output (summary)
- `coverage/lcov-report/index.html` (detailed HTML report)

## Troubleshooting

### Tests Fail to Load CryptogramPuzzle Class
- Ensure `src/client/wwwroot/script.js` exists and contains the `CryptogramPuzzle` class
- Check that the class definition is properly formatted

### Mock Fetch Errors
- If tests fail due to fetch calls, ensure `jest.setup.js` is properly configured
- The setup file should mock `global.fetch`

### DOM-Related Errors
- Ensure `setupTestDOM()` creates all required elements
- Check that element selectors match the actual HTML structure

## Adding New Tests

To add new tests:

1. Create a new test file in `__tests__/` directory
2. Import necessary utilities from `@jest/globals` and `testHelpers.js`
3. Follow the existing test structure:
   ```javascript
   import { describe, test, expect, beforeEach } from '@jest/globals';
   import { createMockPuzzleData, setupTestDOM } from '../helpers/testHelpers.js';

   describe('Feature Name', () => {
     beforeEach(async () => {
       setupTestDOM();
       // Setup code
     });

     test('should do something', () => {
       // Test code
       expect(something).toBe(expected);
     });
   });
   ```

## Continuous Integration

To run these tests in CI/CD:

```yaml
# Example GitHub Actions workflow
- name: Setup Node.js
  uses: actions/setup-node@v3
  with:
    node-version: '18'

- name: Install dependencies
  run: |
    cd src/client.tests
    npm install

- name: Run tests
  run: |
    cd src/client.tests
    npm test -- --coverage
```
