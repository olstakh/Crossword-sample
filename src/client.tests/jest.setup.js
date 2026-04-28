import { jest, beforeEach } from '@jest/globals';

// Create fresh mocks before each test
beforeEach(() => {
  localStorage.clear();
  
  global.fetch = jest.fn();
});
