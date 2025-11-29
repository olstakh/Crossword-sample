// Configuration
const API_BASE_URL = window.location.origin;

class CryptogramPuzzle {
    constructor(data) {
        this.data = data;
        this.grid = [];
        this.currentCell = null;
        this.userAnswers = {}; // number -> letter mapping from user
        this.language = data.language || 'English';
        this.inputMode = localStorage.getItem('inputMode') || 'keyboard';
        this.difficultyMode = localStorage.getItem('difficultyMode') || 'easy';
        
        // Generate numbers and letter mapping from grid
        this.generateCryptogramData();
        this.init();
    }

    generateCryptogramData() {
        // Extract unique letters from grid (excluding '#')
        const uniqueLetters = new Set();
        this.data.grid.forEach(row => {
            row.forEach(cell => {
                if (cell !== '#') {
                    uniqueLetters.add(cell);
                }
            });
        });

        // Create a random letter-to-number mapping
        const letters = Array.from(uniqueLetters);
        const seed = this.data.id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
        const shuffledNumbers = this.shuffleArray([...Array(letters.length).keys()].map(i => i + 1), seed);
        
        // Create letterMapping: letter -> number
        const letterToNumber = {};
        const numberToLetter = {};
        letters.forEach((letter, index) => {
            const number = shuffledNumbers[index];
            letterToNumber[letter] = number;
            numberToLetter[number] = letter;
        });

        // Generate numbers grid
        this.data.numbers = this.data.grid.map(row =>
            row.map(cell => cell === '#' ? 0 : letterToNumber[cell])
        );

        // Store the mapping (number -> letter)
        this.data.letterMapping = numberToLetter;

        // Determine which letters to reveal
        if (this.data.revealedLetters && this.data.revealedLetters.length > 0) {
            // Use revealed letters from server - convert letters to their corresponding numbers
            this.data.initiallyRevealed = this.data.revealedLetters
                .map(letter => letterToNumber[letter])
                .filter(num => num !== undefined); // Filter out letters not in the puzzle
        } else {
            // Fallback: Reveal 25% of letters randomly (at least 2)
            const numbersToReveal = Math.max(2, Math.floor(letters.length * 0.25));
            this.data.initiallyRevealed = shuffledNumbers.slice(0, numbersToReveal);
        }
    }

    shuffleArray(array, seed) {
        // Seeded shuffle for consistent results
        const random = (seed) => {
            const x = Math.sin(seed++) * 10000;
            return x - Math.floor(x);
        };
        
        const result = [...array];
        for (let i = result.length - 1; i > 0; i--) {
            const j = Math.floor(random(seed + i) * (i + 1));
            [result[i], result[j]] = [result[j], result[i]];
        }
        return result;
    }

    getAlphabetForLanguage() {
        const alphabets = {
            'English': 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.split(''),
            'Russian': 'АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ'.split(''),
            'Ukrainian': 'АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ'.split('')
        };
        return alphabets[this.language] || alphabets['English'];
    }

    init() {
        this.renderGrid();
        this.renderAlphabetDecoder();
        this.setupEventListeners();
        this.updateInputMode();
        
        // Initialize with revealed letters
        if (this.data.initiallyRevealed) {
            this.data.initiallyRevealed.forEach(num => {
                const letter = this.data.letterMapping[num.toString()] || this.data.letterMapping[num];
                if (letter) {
                    this.userAnswers[num] = letter;
                }
            });
            this.updateAllCells();
            this.updateAlphabetDecoder();
        }
    }

    renderGrid() {
        const gridElement = document.getElementById('crossword-grid');
        gridElement.style.gridTemplateColumns = `repeat(${this.data.size.cols}, 50px)`;
        gridElement.style.gridTemplateRows = `repeat(${this.data.size.rows}, 50px)`;
        gridElement.innerHTML = '';

        for (let row = 0; row < this.data.size.rows; row++) {
            this.grid[row] = [];
            for (let col = 0; col < this.data.size.cols; col++) {
                const cell = this.createCell(row, col);
                gridElement.appendChild(cell);
                this.grid[row][col] = cell;
            }
        }
    }

    createCell(row, col) {
        const cellDiv = document.createElement('div');
        cellDiv.className = 'cell';
        cellDiv.dataset.row = row;
        cellDiv.dataset.col = col;

        const cellValue = this.data.grid[row][col];
        const cellNumber = this.data.numbers[row][col];

        if (cellValue === '#' || cellNumber === 0) {
            cellDiv.classList.add('black');
            return cellDiv;
        }

        // Add number in corner
        const numberSpan = document.createElement('span');
        numberSpan.className = 'cell-number';
        numberSpan.textContent = cellNumber;
        cellDiv.appendChild(numberSpan);

        // Add input for letter
        const input = document.createElement('input');
        input.type = 'text';
        input.maxLength = 1;
        input.dataset.answer = cellValue;
        input.dataset.number = cellNumber;
        
        // Check if this letter is initially revealed
        if (this.data.initiallyRevealed && this.data.initiallyRevealed.includes(cellNumber)) {
            input.value = cellValue;
            input.readOnly = true;
            cellDiv.classList.add('revealed');
        }

        cellDiv.appendChild(input);
        return cellDiv;
    }

    renderAlphabetDecoder() {
        const alphabetRow = document.getElementById('alphabet-row');
        alphabetRow.innerHTML = '';

        // Get unique numbers used in the puzzle, sorted
        const usedNumbers = new Set();
        for (let row = 0; row < this.data.numbers.length; row++) {
            for (let col = 0; col < this.data.numbers[row].length; col++) {
                const num = this.data.numbers[row][col];
                if (num > 0) {
                    usedNumbers.add(num);
                }
            }
        }

        const sortedNumbers = Array.from(usedNumbers).sort((a, b) => a - b);

        sortedNumbers.forEach(num => {
            const cell = document.createElement('div');
            cell.className = 'alphabet-cell';
            cell.dataset.number = num;

            const numberDiv = document.createElement('div');
            numberDiv.className = 'alphabet-number';
            numberDiv.textContent = num;

            const letterDiv = document.createElement('div');
            letterDiv.className = 'alphabet-letter';
            
            // Check if initially revealed
            if (this.data.initiallyRevealed && this.data.initiallyRevealed.includes(num)) {
                const letter = this.data.letterMapping[num.toString()] || this.data.letterMapping[num];
                letterDiv.textContent = letter || '?';
                cell.classList.add('revealed');
            } else {
                letterDiv.textContent = '?';
                letterDiv.classList.add('hidden');
            }

            cell.appendChild(numberDiv);
            cell.appendChild(letterDiv);
            alphabetRow.appendChild(cell);
        });
    }

    setupEventListeners() {
        const cells = document.querySelectorAll('.cell:not(.black)');

        cells.forEach(cell => {
            const input = cell.querySelector('input');
            if (!input) return;

            // Store original readonly state
            if (!input.hasAttribute('data-originally-readonly')) {
                input.setAttribute('data-originally-readonly', input.readOnly);
            }

            // Handle click events for mouse mode
            cell.addEventListener('click', (e) => {
                const originallyReadonly = input.getAttribute('data-originally-readonly') === 'true';
                if (this.inputMode === 'mouse' && !originallyReadonly) {
                    this.selectCell(cell);
                    this.showLetterPopup(input);
                }
            });

            input.addEventListener('focus', (e) => {
                if (this.inputMode === 'keyboard') {
                    this.selectCell(cell);
                }
            });

            input.addEventListener('input', (e) => {
                if (this.inputMode === 'keyboard') {
                    let value = e.target.value.toUpperCase();
                    
                    // Allow letters from the current language's alphabet
                    const alphabet = this.getAlphabetForLanguage().join('');
                    const regex = new RegExp(`[^${alphabet}]`, 'g');
                    value = value.replace(regex, '');
                    e.target.value = value;

                    if (value) {
                        const number = parseInt(e.target.dataset.number);
                        if (this.difficultyMode === 'easy') {
                            // Easy mode: update all cells with same number
                            this.userAnswers[number] = value;
                            this.updateAllCells();
                        }
                        // Hard mode: cell already has value from input event
                        this.checkNumberComplete(number);
                        this.moveToNextCell();
                    } else {
                        const number = parseInt(e.target.dataset.number);
                        if (this.difficultyMode === 'easy') {
                            delete this.userAnswers[number];
                            this.updateAllCells();
                        }
                        // Hard mode: cell already cleared from input event
                        this.updateAlphabetDecoder();
                    }
                }
            });

            input.addEventListener('keydown', (e) => {
                if (this.inputMode === 'keyboard') {
                    this.handleKeydown(e);
                }
            });
        });
    }

    selectCell(cell) {
        document.querySelectorAll('.cell').forEach(c => c.classList.remove('selected'));
        this.currentCell = cell;
        cell.classList.add('selected');
    }

    showLetterPopup(input) {
        // Remove any existing popup
        const existingPopup = document.querySelector('.letter-popup-overlay');
        if (existingPopup) {
            existingPopup.remove();
        }

        // Create overlay
        const overlay = document.createElement('div');
        overlay.className = 'letter-popup-overlay';

        // Create popup
        const popup = document.createElement('div');
        popup.className = 'letter-popup';

        // Add title
        const title = document.createElement('h3');
        title.textContent = 'Select a Letter';
        title.className = 'letter-popup-title';
        popup.appendChild(title);

        // Create alphabet grid
        const alphabetGrid = document.createElement('div');
        alphabetGrid.className = 'letter-popup-grid';

        const alphabet = this.getAlphabetForLanguage();

        alphabet.forEach(letter => {
            const button = document.createElement('button');
            button.className = 'letter-popup-button';
            button.textContent = letter;
            button.addEventListener('click', () => {
                const number = parseInt(input.dataset.number);
                if (this.difficultyMode === 'easy') {
                    // Easy mode: update all cells with same number
                    this.userAnswers[number] = letter;
                    this.updateAllCells();
                } else {
                    // Hard mode: only update current cell
                    input.value = letter;
                }
                this.checkNumberComplete(number);
                overlay.remove();
            });
            alphabetGrid.appendChild(button);
        });

        popup.appendChild(alphabetGrid);

        // Add clear button
        const clearButton = document.createElement('button');
        clearButton.className = 'letter-popup-button letter-popup-clear';
        clearButton.textContent = '⌫ Clear';
        clearButton.addEventListener('click', () => {
            const number = parseInt(input.dataset.number);
            if (this.difficultyMode === 'easy') {
                // Easy mode: clear all cells with same number
                delete this.userAnswers[number];
                this.updateAllCells();
            } else {
                // Hard mode: only clear current cell
                input.value = '';
            }
            this.updateAlphabetDecoder();
            overlay.remove();
        });
        alphabetGrid.appendChild(clearButton);

        // Add close button
        const closeButton = document.createElement('button');
        closeButton.className = 'letter-popup-close';
        closeButton.textContent = '✕';
        closeButton.addEventListener('click', () => {
            overlay.remove();
        });
        popup.appendChild(closeButton);

        overlay.appendChild(popup);
        document.body.appendChild(overlay);

        // Close on overlay click
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                overlay.remove();
            }
        });

        // Close on Escape key
        const escapeHandler = (e) => {
            if (e.key === 'Escape') {
                overlay.remove();
                document.removeEventListener('keydown', escapeHandler);
            }
        };
        document.addEventListener('keydown', escapeHandler);
    }

    updateInputMode() {
        // Update all inputs based on current mode
        const cells = document.querySelectorAll('.cell:not(.black) input');
        cells.forEach(input => {
            const wasReadOnly = input.getAttribute('data-originally-readonly') === 'true';
            
            if (this.inputMode === 'mouse') {
                if (!wasReadOnly) {
                    input.readOnly = true;
                    input.style.cursor = 'pointer';
                    input.parentElement.style.cursor = 'pointer';
                }
            } else {
                if (!wasReadOnly) {
                    input.readOnly = false;
                    input.style.cursor = 'text';
                    input.parentElement.style.cursor = 'default';
                }
            }
        });
    }

    setInputMode(mode) {
        this.inputMode = mode;
        localStorage.setItem('inputMode', mode);
        this.updateInputMode();
    }

    setDifficultyMode(mode) {
        this.difficultyMode = mode;
        localStorage.setItem('difficultyMode', mode);
    }

    updateAllCells() {
        // Update all cells with the same number (only in easy mode)
        if (this.difficultyMode === 'easy') {
            const inputs = document.querySelectorAll('.cell:not(.black) input');
            inputs.forEach(input => {
                // Skip originally readonly cells (revealed letters)
                const originallyReadonly = input.getAttribute('data-originally-readonly') === 'true';
                if (originallyReadonly) return;
                
                const number = parseInt(input.dataset.number);
                if (this.userAnswers[number]) {
                    input.value = this.userAnswers[number];
                } else {
                    // Clear cells if number was deleted from userAnswers
                    input.value = '';
                }
            });
        }
        // In hard mode, don't update other cells
    }

    checkNumberComplete(number) {
        // Simply update the alphabet decoder - it will check all cells and show what user typed
        this.updateAlphabetDecoder();
    }

    revealInAlphabet(number) {
        const alphabetCell = document.querySelector(`.alphabet-cell[data-number="${number}"]`);
        if (alphabetCell) {
            const letterDiv = alphabetCell.querySelector('.alphabet-letter');
            const letter = this.data.letterMapping[number.toString()] || this.data.letterMapping[number];
            letterDiv.textContent = letter || '?';
            letterDiv.classList.remove('hidden');
            alphabetCell.classList.add('revealed');
        }
    }

    updateAlphabetDecoder() {
        const alphabetCells = document.querySelectorAll('.alphabet-cell');
        alphabetCells.forEach(cell => {
            const number = parseInt(cell.dataset.number);
            const letterDiv = cell.querySelector('.alphabet-letter');
            
            // Check if this number was initially revealed
            if (this.data.initiallyRevealed && this.data.initiallyRevealed.includes(number)) {
                const letter = this.data.letterMapping[number.toString()] || this.data.letterMapping[number];
                letterDiv.textContent = letter || '?';
                cell.classList.add('revealed');
                return;
            }
            
            // Check all cells with this number in the actual grid
            const inputs = document.querySelectorAll(`input[data-number="${number}"]`);
            const originallyReadonly = inputs[0]?.getAttribute('data-originally-readonly') === 'true';
            if (originallyReadonly) return; // Skip if this was originally readonly
            
            let allFilled = true;
            let allSame = true;
            let firstValue = null;
            
            inputs.forEach(input => {
                if (!input.value) {
                    allFilled = false;
                } else {
                    if (firstValue === null) {
                        firstValue = input.value.toUpperCase();
                    } else if (input.value.toUpperCase() !== firstValue) {
                        allSame = false;
                    }
                }
            });
            
            // Only reveal if all cells are filled and have the same letter
            if (allFilled && allSame && firstValue) {
                letterDiv.textContent = firstValue;
                letterDiv.classList.remove('hidden');
            } else {
                letterDiv.textContent = '?';
                letterDiv.classList.add('hidden');
            }
        });
    }

    handleKeydown(e) {
        if (!this.currentCell) return;

        const row = parseInt(this.currentCell.dataset.row);
        const col = parseInt(this.currentCell.dataset.col);

        switch (e.key) {
            case 'ArrowRight':
                e.preventDefault();
                this.moveCell(row, col + 1);
                break;
            case 'ArrowLeft':
                e.preventDefault();
                this.moveCell(row, col - 1);
                break;
            case 'ArrowDown':
                e.preventDefault();
                this.moveCell(row + 1, col);
                break;
            case 'ArrowUp':
                e.preventDefault();
                this.moveCell(row - 1, col);
                break;
            case 'Backspace':
                if (!e.target.value && !e.target.readOnly) {
                    e.preventDefault();
                    this.moveToPreviousCell();
                }
                break;
        }
    }

    moveCell(row, col) {
        if (row < 0 || row >= this.data.size.rows || col < 0 || col >= this.data.size.cols) {
            return;
        }

        const cell = this.grid[row][col];
        if (cell.classList.contains('black')) {
            return;
        }

        const input = cell.querySelector('input');
        if (input && !input.readOnly) {
            input.focus();
        }
    }

    moveToNextCell() {
        if (!this.currentCell) return;

        const row = parseInt(this.currentCell.dataset.row);
        const col = parseInt(this.currentCell.dataset.col);

        // Try to move right first
        for (let c = col + 1; c < this.data.size.cols; c++) {
            const cell = this.grid[row][c];
            if (!cell.classList.contains('black')) {
                const input = cell.querySelector('input');
                if (input && !input.readOnly) {
                    input.focus();
                    return;
                }
            }
        }

        // Move to next row
        for (let r = row + 1; r < this.data.size.rows; r++) {
            for (let c = 0; c < this.data.size.cols; c++) {
                const cell = this.grid[r][c];
                if (!cell.classList.contains('black')) {
                    const input = cell.querySelector('input');
                    if (input && !input.readOnly) {
                        input.focus();
                        return;
                    }
                }
            }
        }
    }

    moveToPreviousCell() {
        if (!this.currentCell) return;

        const row = parseInt(this.currentCell.dataset.row);
        const col = parseInt(this.currentCell.dataset.col);

        // Try to move left first
        for (let c = col - 1; c >= 0; c--) {
            const cell = this.grid[row][c];
            if (!cell.classList.contains('black')) {
                const input = cell.querySelector('input');
                if (input && !input.readOnly) {
                    input.focus();
                    return;
                }
            }
        }

        // Move to previous row
        for (let r = row - 1; r >= 0; r--) {
            for (let c = this.data.size.cols - 1; c >= 0; c--) {
                const cell = this.grid[r][c];
                if (!cell.classList.contains('black')) {
                    const input = cell.querySelector('input');
                    if (input && !input.readOnly) {
                        input.focus();
                        return;
                    }
                }
            }
        }
    }

    checkAnswers() {
        const inputs = document.querySelectorAll('.cell:not(.black) input');
        let allCorrect = true;
        let allFilled = true;

        inputs.forEach(input => {
            const cell = input.parentElement;
            cell.classList.remove('correct', 'incorrect');

            // Check if cell was originally readonly (revealed), not current readonly state (which is true in mouse mode)
            const wasOriginallyReadonly = input.getAttribute('data-originally-readonly') === 'true';
            if (wasOriginallyReadonly) {
                cell.classList.add('correct');
                return;
            }

            if (input.value) {
                if (input.value.toUpperCase() === input.dataset.answer) {
                    cell.classList.add('correct');
                } else {
                    cell.classList.add('incorrect');
                    allCorrect = false;
                }
            } else {
                allFilled = false;
            }
        });

        if (allFilled && allCorrect) {
            setTimeout(() => {
                // Record solved puzzle
                if (typeof userAuth !== 'undefined') {
                    userAuth.recordSolved(this.data.id);
                } else {
                    alert('Congratulations! You solved the puzzle correctly!');
                }
            }, 100);
        } else if (allFilled) {
            setTimeout(() => {
                alert('Some answers are incorrect. Keep trying!');
            }, 100);
        } else {
            setTimeout(() => {
                alert('Please fill in all cells before checking.');
            }, 100);
        }
    }

    revealAnswers() {
        const inputs = document.querySelectorAll('.cell:not(.black) input');
        inputs.forEach(input => {
            // Check if cell was originally readonly (revealed), not current readonly state (which is true in mouse mode)
            const wasOriginallyReadonly = input.getAttribute('data-originally-readonly') === 'true';
            if (!wasOriginallyReadonly) {
                input.value = input.dataset.answer;
                const number = parseInt(input.dataset.number);
                this.userAnswers[number] = input.dataset.answer;
            }
            const cell = input.parentElement;
            cell.classList.remove('incorrect');
            cell.classList.add('correct');
        });

        // Reveal all in alphabet decoder
        const alphabetCells = document.querySelectorAll('.alphabet-cell');
        alphabetCells.forEach(cell => {
            const number = parseInt(cell.dataset.number);
            const letterDiv = cell.querySelector('.alphabet-letter');
            // Convert number to string key for dictionary lookup
            const letter = this.data.letterMapping[number.toString()] || this.data.letterMapping[number];
            if (letter) {
                letterDiv.textContent = letter;
                letterDiv.classList.remove('hidden');
                cell.classList.add('revealed');
            }
        });
    }

    clearGrid() {
        // Clear all cells that were not originally readonly (revealed letters)
        const inputs = document.querySelectorAll('.cell:not(.black) input');
        inputs.forEach(input => {
            const originallyReadonly = input.getAttribute('data-originally-readonly') === 'true';
            if (!originallyReadonly) {
                input.value = '';
                const cell = input.parentElement;
                cell.classList.remove('correct', 'incorrect');
            }
        });

        // Reset user answers except initially revealed
        this.userAnswers = {};
        if (this.data.initiallyRevealed) {
            this.data.initiallyRevealed.forEach(num => {
                const letter = this.data.letterMapping[num.toString()] || this.data.letterMapping[num];
                if (letter) {
                    this.userAnswers[num] = letter;
                }
            });
        }

        // Reset alphabet decoder
        this.updateAlphabetDecoder();
    }
}

// Show congratulations message in place of puzzle grid
function showAllPuzzlesSolvedMessage(message, size, language) {
    const gridElement = document.getElementById('crossword-grid');
    const alphabetElement = document.getElementById('alphabet-row');
    const puzzleSection = document.querySelector('.puzzle-section');
    
    if (gridElement) {
        gridElement.innerHTML = `
            <div class="all-solved-message">
                <div class="celebration-icon">🎉🏆🎊</div>
                <h2>Congratulations!</h2>
                <p>${message}</p>
                <div class="all-solved-actions">
                    <p>Try:</p>
                    <ul>
                        <li>Selecting a different <strong>size category</strong></li>
                        <li>Choosing a different <strong>language</strong></li>
                        <li>Replaying your favorite puzzles!</li>
                    </ul>
                </div>
            </div>
        `;
        gridElement.style.gridTemplateColumns = '1fr';
        gridElement.style.gridTemplateRows = '1fr';
    }
    
    if (alphabetElement) {
        alphabetElement.innerHTML = '';
    }
    
    // Fade in the message
    if (puzzleSection) {
        puzzleSection.style.opacity = '1';
    }
}

// Show user-friendly error message
function showErrorMessage(title, message, onRetry = null) {
    // Create modal overlay
    const overlay = document.createElement('div');
    overlay.className = 'error-overlay';
    
    const modal = document.createElement('div');
    modal.className = 'error-modal';
    
    modal.innerHTML = `
        <h2>${title}</h2>
        <p>${message}</p>
        <div class="error-actions">
            ${onRetry ? '<button class="btn-retry">Try Different Settings</button>' : ''}
            <button class="btn-close">OK</button>
        </div>
    `;
    
    overlay.appendChild(modal);
    document.body.appendChild(overlay);
    
    // Setup event listeners
    const closeBtn = modal.querySelector('.btn-close');
    const retryBtn = modal.querySelector('.btn-retry');
    
    const closeModal = () => {
        document.body.removeChild(overlay);
    };
    
    closeBtn.addEventListener('click', closeModal);
    if (retryBtn && onRetry) {
        retryBtn.addEventListener('click', () => {
            closeModal();
            onRetry();
        });
    }
    overlay.addEventListener('click', (e) => {
        if (e.target === overlay) closeModal();
    });
}

// Fetch puzzle data from the API
async function fetchPuzzle(puzzleId = 'puzzle1') {
    try {
        const response = await fetch(`${API_BASE_URL}/api/crossword/puzzle/${puzzleId}`);
        if (!response.ok) {
            if (response.status === 404) {
                const errorData = await response.json();
                const error = new Error(errorData.error || 'Puzzle not found');
                error.status = 404;
                throw error;
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Error fetching puzzle:', error);
        if (error.status !== 404) {
            showErrorMessage('Connection Error', 'Unable to connect to the server. Please make sure the server is running.');
        }
        throw error;
    }
}

async function fetchPuzzleBySize(size = 'medium', language = 'English', seed = null) {
    try {
        let url = `${API_BASE_URL}/api/crossword/puzzle?size=${size}&language=${language}`;
        if (seed) {
            url += `&seed=${seed}`;
        }
        
        // Add userId header if userAuth is available
        const headers = {};
        if (typeof userAuth !== 'undefined' && userAuth.userId) {
            headers['X-User-Id'] = userAuth.userId;
        }
        
        const response = await fetch(url, { headers });
        if (!response.ok) {
            if (response.status === 404) {
                const errorData = await response.json();
                const error = new Error(errorData.error || 'Puzzle not found');
                error.status = 404;
                error.requestedSize = size;
                error.requestedLanguage = language;
                error.isAllSolved = errorData.error && errorData.error.includes('solved all');
                throw error;
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Error fetching puzzle:', error);
        if (error.status !== 404) {
            showErrorMessage('Connection Error', 'Unable to connect to the server. Please make sure the server is running.');
        }
        throw error;
    }
}

// Global reference to current puzzle instance
let currentPuzzle = null;

async function loadPuzzle() {
    try {
        const urlParams = new URLSearchParams(window.location.search);
        const puzzleId = urlParams.get('puzzle');
        const size = urlParams.get('size');
        const language = urlParams.get('language') || 'English';
        const seed = urlParams.get('seed');
        
        let puzzleData;
        
        if (size) {
            // Load by size and language
            try {
                puzzleData = await fetchPuzzleBySize(size, language, seed);
            } catch (error) {
                if (error.status === 404) {
                    // Check if this is all-solved case
                    if (error.isAllSolved) {
                        showAllPuzzlesSolvedMessage(error.message, size, language);
                        // Update radio buttons to match
                        const radioButton = document.querySelector(`input[name="puzzleSize"][value="${size}"]`);
                        if (radioButton) {
                            radioButton.checked = true;
                        }
                        return;
                    }
                    
                    // Show user-friendly error with option to try English
                    const message = `${error.message}\n\nWould you like to try loading an English puzzle instead?`;
                    showErrorMessage('Puzzle Not Available', message, async () => {
                        // Fallback to English
                        try {
                            puzzleData = await fetchPuzzleBySize(size, 'English', seed);
                            if (puzzleData) {
                                document.getElementById('languageSelector').value = 'English';
                                initializePuzzle(puzzleData, size);
                            }
                        } catch (fallbackError) {
                            if (fallbackError.status === 404) {
                                if (fallbackError.isAllSolved) {
                                    showAllPuzzlesSolvedMessage(fallbackError.message, size, 'English');
                                } else {
                                    showErrorMessage('No Puzzles Available', 
                                        'Sorry, no puzzles are available for this size. Please try a different size.');
                                }
                            }
                        }
                    });
                    return;
                }
                throw error;
            }
            // Update radio buttons to match
            const radioButton = document.querySelector(`input[name="puzzleSize"][value="${size}"]`);
            if (radioButton) {
                radioButton.checked = true;
            }
        } else if (puzzleId) {
            // Load by specific ID
            try {
                puzzleData = await fetchPuzzle(puzzleId);
            } catch (error) {
                if (error.status === 404) {
                    showErrorMessage('Puzzle Not Found', 
                        `The puzzle "${puzzleId}" could not be found. Loading a default puzzle instead...`);
                    // Fallback to default
                    puzzleData = await fetchPuzzleBySize('medium', 'English');
                } else {
                    throw error;
                }
            }
        } else {
            // Default: load medium puzzle in selected language
            try {
                puzzleData = await fetchPuzzleBySize('medium', language);
            } catch (error) {
                if (error.status === 404) {
                    // Check if all-solved
                    if (error.isAllSolved) {
                        showAllPuzzlesSolvedMessage(error.message, 'medium', language);
                        return;
                    }
                    
                    // Try fallback to English
                    if (language !== 'English') {
                        try {
                            puzzleData = await fetchPuzzleBySize('medium', 'English');
                            document.getElementById('languageSelector').value = 'English';
                        } catch (fallbackError) {
                            if (fallbackError.isAllSolved) {
                                showAllPuzzlesSolvedMessage(fallbackError.message, 'medium', 'English');
                                return;
                            }
                            throw fallbackError;
                        }
                    } else {
                        throw error;
                    }
                } else {
                    throw error;
                }
            }
        }
        
        if (puzzleData) {
            initializePuzzle(puzzleData, size);
        }
    } catch (error) {
        console.error('Failed to initialize cryptogram:', error);
    }
}

function initializePuzzle(puzzleData, size = null) {
    // Update language selector to match
    const languageSelector = document.getElementById('languageSelector');
    if (languageSelector && puzzleData.language) {
        languageSelector.value = puzzleData.language;
    }
    
    // Update radio buttons if size provided
    if (size) {
        const radioButton = document.querySelector(`input[name="puzzleSize"][value="${size}"]`);
        if (radioButton) {
            radioButton.checked = true;
        }
    }
    
    // Update page title if puzzle has a title
    if (puzzleData.title) {
        document.querySelector('h1').textContent = puzzleData.title;
    }
    
    // Clear previous puzzle if exists
    if (currentPuzzle) {
        const gridElement = document.getElementById('crossword-grid');
        const alphabetElement = document.getElementById('alphabet-row');
        if (gridElement) gridElement.innerHTML = '';
        if (alphabetElement) alphabetElement.innerHTML = '';
    }
    
    currentPuzzle = new CryptogramPuzzle(puzzleData);
}

// Load a new puzzle without page reload (with smooth transition)
async function loadNewPuzzle(size, language) {
    const puzzleSection = document.querySelector('.puzzle-section');
    
    // Fade out
    if (puzzleSection) {
        puzzleSection.style.opacity = '0';
        puzzleSection.style.transition = 'opacity 0.3s ease-out';
    }
    
    // Wait for fade out
    await new Promise(resolve => setTimeout(resolve, 300));
    
    try {
        // Fetch new puzzle
        const seed = Date.now().toString(); // Use timestamp for variety
        const puzzleData = await fetchPuzzleBySize(size, language, seed);
        
        // Update URL without reload (optional - keeps URL in sync)
        const url = new URL(window.location);
        url.searchParams.set('size', size);
        url.searchParams.set('language', language);
        url.searchParams.set('seed', seed);
        url.searchParams.delete('puzzle');
        window.history.pushState({}, '', url.toString());
        
        // Initialize new puzzle
        initializePuzzle(puzzleData, size);
        
        // Fade in
        if (puzzleSection) {
            await new Promise(resolve => setTimeout(resolve, 50)); // Small delay
            puzzleSection.style.opacity = '1';
        }
    } catch (error) {
        // Restore opacity on error
        if (puzzleSection) {
            puzzleSection.style.opacity = '1';
        }
        
        // Handle all-puzzles-solved case with congratulatory message
        if (error.status === 404 && error.isAllSolved) {
            showAllPuzzlesSolvedMessage(error.message, size, language);
            return; // Don't re-throw, we handled it
        }
        
        throw error;
    }
}

// Initialize the crossword puzzle when the page loads
document.addEventListener('DOMContentLoaded', async () => {
    // Initialize userAuth and wait for progress to load
    if (typeof userAuth !== 'undefined') {
        await userAuth.init();
    }
    
    await loadPuzzle();
    
    // Setup button event listeners once (they reference currentPuzzle)
    const checkBtn = document.getElementById('checkBtn');
    const revealBtn = document.getElementById('revealBtn');
    const clearBtn = document.getElementById('clearBtn');
    
    if (checkBtn) {
        checkBtn.addEventListener('click', () => {
            if (currentPuzzle) currentPuzzle.checkAnswers();
        });
    }
    
    if (revealBtn) {
        revealBtn.addEventListener('click', () => {
            if (currentPuzzle) currentPuzzle.revealAnswers();
        });
    }
    
    if (clearBtn) {
        clearBtn.addEventListener('click', () => {
            if (currentPuzzle) currentPuzzle.clearGrid();
        });
    }
    
    // Setup new puzzle button
    const newPuzzleBtn = document.getElementById('newPuzzleBtn');
    
    if (newPuzzleBtn) {
        console.log('New Puzzle button found, setting up event listener');
        newPuzzleBtn.addEventListener('click', async (e) => {
            console.log('New Puzzle button clicked');
            e.preventDefault();
            const selectedRadio = document.querySelector('input[name="puzzleSize"]:checked');
            const languageSelector = document.getElementById('languageSelector');
            const selectedLanguage = languageSelector ? languageSelector.value : 'English';
            console.log('Selected radio:', selectedRadio);
            console.log('Selected language:', selectedLanguage);
            if (selectedRadio) {
                const selectedSize = selectedRadio.value;
                console.log('Selected size:', selectedSize);
                
                // Disable button and show loading state
                newPuzzleBtn.disabled = true;
                const originalText = newPuzzleBtn.textContent;
                newPuzzleBtn.textContent = 'Loading...';
                
                try {
                    await loadNewPuzzle(selectedSize, selectedLanguage);
                } catch (error) {
                    console.error('Error loading new puzzle:', error);
                    
                    // Only show generic error if it's not the all-solved case (which loadNewPuzzle already handled)
                    if (!(error.status === 404 && error.isAllSolved)) {
                        showErrorMessage('Error', 'Failed to load new puzzle. Please try again.');
                    }
                } finally {
                    // Re-enable button
                    newPuzzleBtn.disabled = false;
                    newPuzzleBtn.textContent = originalText;
                }
            }
        });
    } else {
        console.error('New Puzzle button not found!');
    }

    // Setup input mode toggle
    const inputModeToggle = document.getElementById('inputModeToggle');
    if (inputModeToggle) {
        // Initialize toggle state from localStorage
        const savedMode = localStorage.getItem('inputMode') || 'keyboard';
        inputModeToggle.checked = (savedMode === 'mouse');
        
        // Handle toggle changes
        inputModeToggle.addEventListener('change', (e) => {
            const newMode = e.target.checked ? 'mouse' : 'keyboard';
            if (currentPuzzle) {
                currentPuzzle.setInputMode(newMode);
            }
        });
    }

    // Setup difficulty mode toggle
    const difficultyModeToggle = document.getElementById('difficultyModeToggle');
    if (difficultyModeToggle) {
        // Initialize toggle state from localStorage
        const savedDifficulty = localStorage.getItem('difficultyMode') || 'easy';
        difficultyModeToggle.checked = (savedDifficulty === 'hard');
        
        // Handle toggle changes
        difficultyModeToggle.addEventListener('change', (e) => {
            const newMode = e.target.checked ? 'hard' : 'easy';
            if (currentPuzzle) {
                currentPuzzle.setDifficultyMode(newMode);
            }
        });
    }
});
