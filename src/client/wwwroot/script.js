// Configuration
const API_BASE_URL = window.location.origin;

class CryptogramPuzzle {
    constructor(data) {
        this.data = data;
        this.grid = [];
        this.currentCell = null;
        this.userAnswers = {}; // number -> letter mapping from user
        this.init();
    }

    init() {
        this.renderGrid();
        this.renderAlphabetDecoder();
        this.setupEventListeners();
        
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
        const cells = document.querySelectorAll('.cell:not(.black) input');

        cells.forEach(input => {
            input.addEventListener('focus', (e) => {
                const cell = e.target.parentElement;
                this.selectCell(cell);
            });

            input.addEventListener('input', (e) => {
                let value = e.target.value.toUpperCase();
                
                // Only allow letters A-Z
                value = value.replace(/[^A-Z]/g, '');
                e.target.value = value;

                if (value) {
                    const number = parseInt(e.target.dataset.number);
                    this.userAnswers[number] = value;
                    this.updateAllCells();
                    this.checkNumberComplete(number);
                    this.moveToNextCell();
                } else {
                    const number = parseInt(e.target.dataset.number);
                    delete this.userAnswers[number];
                    this.updateAllCells();
                    this.updateAlphabetDecoder();
                }
            });

            input.addEventListener('keydown', (e) => {
                this.handleKeydown(e);
            });
        });

        document.getElementById('checkBtn').addEventListener('click', () => this.checkAnswers());
        document.getElementById('revealBtn').addEventListener('click', () => this.revealAnswers());
        document.getElementById('clearBtn').addEventListener('click', () => this.clearGrid());
    }

    selectCell(cell) {
        document.querySelectorAll('.cell').forEach(c => c.classList.remove('selected'));
        this.currentCell = cell;
        cell.classList.add('selected');
    }

    updateAllCells() {
        // Update all cells with the same number
        const inputs = document.querySelectorAll('.cell:not(.black) input:not([readonly])');
        inputs.forEach(input => {
            const number = parseInt(input.dataset.number);
            if (this.userAnswers[number]) {
                input.value = this.userAnswers[number];
            }
        });
    }

    checkNumberComplete(number) {
        // Check if all instances of this number are correctly filled
        const inputs = document.querySelectorAll(`input[data-number="${number}"]:not([readonly])`);
        let allCorrect = true;
        let allFilled = true;

        inputs.forEach(input => {
            if (!input.value) {
                allFilled = false;
            } else if (input.value.toUpperCase() !== input.dataset.answer) {
                allCorrect = false;
            }
        });

        if (allFilled && allCorrect) {
            // Reveal in alphabet decoder
            this.revealInAlphabet(number);
        } else {
            this.updateAlphabetDecoder();
        }
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
        const alphabetCells = document.querySelectorAll('.alphabet-cell:not(.revealed)');
        alphabetCells.forEach(cell => {
            const number = parseInt(cell.dataset.number);
            const letterDiv = cell.querySelector('.alphabet-letter');
            
            if (this.userAnswers[number]) {
                letterDiv.textContent = this.userAnswers[number];
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

            if (input.readOnly) {
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
                alert('Congratulations! You solved the puzzle correctly!');
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
            if (!input.readOnly) {
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
        const inputs = document.querySelectorAll('.cell:not(.black) input:not([readonly])');
        inputs.forEach(input => {
            input.value = '';
            const cell = input.parentElement;
            cell.classList.remove('correct', 'incorrect');
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
        const alphabetCells = document.querySelectorAll('.alphabet-cell');
        alphabetCells.forEach(cell => {
            const number = parseInt(cell.dataset.number);
            const letterDiv = cell.querySelector('.alphabet-letter');
            
            if (this.data.initiallyRevealed && this.data.initiallyRevealed.includes(number)) {
                const letter = this.data.letterMapping[number.toString()] || this.data.letterMapping[number];
                letterDiv.textContent = letter || '?';
                cell.classList.add('revealed');
            } else {
                letterDiv.textContent = '?';
                letterDiv.classList.add('hidden');
                cell.classList.remove('revealed');
            }
        });
    }
}

// Fetch puzzle data from the API
async function fetchPuzzle(puzzleId = 'puzzle1') {
    try {
        const response = await fetch(`${API_BASE_URL}/api/crossword/puzzle/${puzzleId}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Error fetching puzzle:', error);
        alert('Error loading puzzle. Please make sure the server is running.');
        throw error;
    }
}

async function fetchPuzzleBySize(size = 'medium', seed = null) {
    try {
        const url = seed 
            ? `${API_BASE_URL}/api/crossword/puzzle/size/${size}?seed=${seed}`
            : `${API_BASE_URL}/api/crossword/puzzle/size/${size}`;
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Error fetching puzzle:', error);
        alert('Error loading puzzle. Please make sure the server is running.');
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
        const seed = urlParams.get('seed');
        
        let puzzleData;
        
        if (size) {
            // Load by size
            puzzleData = await fetchPuzzleBySize(size, seed);
            // Update radio buttons to match
            const radioButton = document.querySelector(`input[name="puzzleSize"][value="${size}"]`);
            if (radioButton) {
                radioButton.checked = true;
            }
        } else if (puzzleId) {
            // Load by specific ID
            puzzleData = await fetchPuzzle(puzzleId);
        } else {
            // Default: load medium puzzle
            puzzleData = await fetchPuzzleBySize('medium');
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
    } catch (error) {
        console.error('Failed to initialize cryptogram:', error);
    }
}

// Initialize the crossword puzzle when the page loads
document.addEventListener('DOMContentLoaded', async () => {
    await loadPuzzle();
    
    // Setup new puzzle button
    const newPuzzleBtn = document.getElementById('newPuzzleBtn');
    
    if (newPuzzleBtn) {
        console.log('New Puzzle button found, setting up event listener');
        newPuzzleBtn.addEventListener('click', (e) => {
            console.log('New Puzzle button clicked');
            e.preventDefault();
            const selectedRadio = document.querySelector('input[name="puzzleSize"]:checked');
            console.log('Selected radio:', selectedRadio);
            if (selectedRadio) {
                const selectedSize = selectedRadio.value;
                console.log('Selected size:', selectedSize);
                // Update URL and reload
                const url = new URL(window.location);
                url.searchParams.set('size', selectedSize);
                url.searchParams.delete('puzzle'); // Remove puzzle param if exists
                url.searchParams.delete('seed'); // Remove seed to get a new puzzle
                console.log('Navigating to:', url.toString());
                window.location.href = url.toString();
            }
        });
    } else {
        console.error('New Puzzle button not found!');
    }
});
