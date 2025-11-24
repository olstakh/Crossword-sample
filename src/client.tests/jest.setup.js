import { jest, beforeEach } from '@jest/globals';

// Create fresh mocks before each test
beforeEach(() => {
  global.localStorage = {
    getItem: jest.fn(() => null),
    setItem: jest.fn(),
    removeItem: jest.fn(),
    clear: jest.fn(),
  };
  
  global.fetch = jest.fn();
});
