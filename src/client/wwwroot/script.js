// Configuration
const API_BASE_URL = window.location.origin;

class CrosswordPuzzle {
    constructor(data) {
        this.data = data;
        this.grid = [];
        this.currentCell = null;
        this.currentDirection = 'across';
        this.currentWord = null;
        this.init();
    }

    init() {
        this.renderGrid();
        this.renderClues();
        this.setupEventListeners();
    }

    renderGrid() {
        const gridElement = document.getElementById('crossword-grid');
        gridElement.style.gridTemplateColumns = `repeat(${this.data.size.cols}, 40px)`;
        gridElement.style.gridTemplateRows = `repeat(${this.data.size.rows}, 40px)`;

        // Create cell number mapping
        const cellNumbers = this.getCellNumbers();

        for (let row = 0; row < this.data.size.rows; row++) {
            this.grid[row] = [];
            for (let col = 0; col < this.data.size.cols; col++) {
                const cell = this.createCell(row, col, cellNumbers);
                gridElement.appendChild(cell);
                this.grid[row][col] = cell;
            }
        }
    }

    getCellNumbers() {
        const numbers = {};
        let currentNumber = 1;

        // Combine all clues and sort by position
        const allClues = [
            ...this.data.clues.across.map(c => ({ ...c, dir: 'across' })),
            ...this.data.clues.down.map(c => ({ ...c, dir: 'down' }))
        ].sort((a, b) => {
            if (a.row !== b.row) return a.row - b.row;
            return a.col - b.col;
        });

        const processed = new Set();

        for (const clue of allClues) {
            const key = `${clue.row}-${clue.col}`;
            if (!processed.has(key)) {
                numbers[key] = clue.number;
                processed.add(key);
            }
        }

        return numbers;
    }

    createCell(row, col, cellNumbers) {
        const cellDiv = document.createElement('div');
        cellDiv.className = 'cell';
        cellDiv.dataset.row = row;
        cellDiv.dataset.col = col;

        const cellValue = this.data.grid[row][col];

        if (cellValue === '#') {
            cellDiv.classList.add('black');
            return cellDiv;
        }

        const input = document.createElement('input');
        input.type = 'text';
        input.maxLength = 1;
        input.dataset.answer = cellValue;

        const cellKey = `${row}-${col}`;
        if (cellNumbers[cellKey]) {
            const numberSpan = document.createElement('span');
            numberSpan.className = 'cell-number';
            numberSpan.textContent = cellNumbers[cellKey];
            cellDiv.appendChild(numberSpan);
        }

        cellDiv.appendChild(input);
        return cellDiv;
    }

    renderClues() {
        const acrossClues = document.getElementById('across-clues');
        const downClues = document.getElementById('down-clues');

        this.data.clues.across.forEach(clue => {
            const clueElement = this.createClueElement(clue, 'across');
            acrossClues.appendChild(clueElement);
        });

        this.data.clues.down.forEach(clue => {
            const clueElement = this.createClueElement(clue, 'down');
            downClues.appendChild(clueElement);
        });
    }

    createClueElement(clue, direction) {
        const clueDiv = document.createElement('div');
        clueDiv.className = 'clue-item';
        clueDiv.dataset.number = clue.number;
        clueDiv.dataset.direction = direction;
        clueDiv.dataset.row = clue.row;
        clueDiv.dataset.col = clue.col;

        clueDiv.innerHTML = `
            <span class="clue-number">${clue.number}.</span>
            <span class="clue-text">${clue.clueText}</span>
        `;

        clueDiv.addEventListener('click', () => {
            this.selectClue(clue, direction);
        });

        return clueDiv;
    }

    setupEventListeners() {
        const cells = document.querySelectorAll('.cell:not(.black) input');

        cells.forEach(input => {
            input.addEventListener('focus', (e) => {
                const cell = e.target.parentElement;
                this.selectCell(cell);
            });

            input.addEventListener('input', (e) => {
                const value = e.target.value.toUpperCase();
                e.target.value = value;

                if (value) {
                    this.moveToNextCell();
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
        this.clearHighlights();
        this.currentCell = cell;
        cell.classList.add('selected');
        this.highlightCurrentWord();
        this.highlightCurrentClue();
    }

    selectClue(clue, direction) {
        this.currentDirection = direction;
        const cell = this.grid[clue.row][clue.col];
        const input = cell.querySelector('input');
        if (input) {
            input.focus();
        }
    }

    highlightCurrentWord() {
        if (!this.currentCell) return;

        const row = parseInt(this.currentCell.dataset.row);
        const col = parseInt(this.currentCell.dataset.col);

        const wordCells = this.getWordCells(row, col, this.currentDirection);
        wordCells.forEach(cell => {
            if (cell !== this.currentCell) {
                cell.classList.add('highlighted');
            }
        });
    }

    getWordCells(row, col, direction) {
        const cells = [];

        if (direction === 'across') {
            // Find start of word
            let startCol = col;
            while (startCol > 0 && this.data.grid[row][startCol - 1] !== '#') {
                startCol--;
            }

            // Get all cells in word
            let currentCol = startCol;
            while (currentCol < this.data.size.cols && this.data.grid[row][currentCol] !== '#') {
                cells.push(this.grid[row][currentCol]);
                currentCol++;
            }
        } else {
            // Find start of word
            let startRow = row;
            while (startRow > 0 && this.data.grid[startRow - 1][col] !== '#') {
                startRow--;
            }

            // Get all cells in word
            let currentRow = startRow;
            while (currentRow < this.data.size.rows && this.data.grid[currentRow][col] !== '#') {
                cells.push(this.grid[currentRow][col]);
                currentRow++;
            }
        }

        return cells;
    }

    highlightCurrentClue() {
        document.querySelectorAll('.clue-item').forEach(clue => clue.classList.remove('active'));

        if (!this.currentCell) return;

        const row = parseInt(this.currentCell.dataset.row);
        const col = parseInt(this.currentCell.dataset.col);

        // Find the clue that matches current position
        const clues = this.data.clues[this.currentDirection];
        const wordCells = this.getWordCells(row, col, this.currentDirection);
        
        if (wordCells.length > 0) {
            const startCell = wordCells[0];
            const startRow = parseInt(startCell.dataset.row);
            const startCol = parseInt(startCell.dataset.col);

            const matchingClue = clues.find(c => c.row === startRow && c.col === startCol);
            if (matchingClue) {
                const clueElement = document.querySelector(
                    `.clue-item[data-number="${matchingClue.number}"][data-direction="${this.currentDirection}"]`
                );
                if (clueElement) {
                    clueElement.classList.add('active');
                }
            }
        }
    }

    clearHighlights() {
        document.querySelectorAll('.cell').forEach(cell => {
            cell.classList.remove('selected', 'highlighted');
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
                this.currentDirection = 'across';
                break;
            case 'ArrowLeft':
                e.preventDefault();
                this.moveCell(row, col - 1);
                this.currentDirection = 'across';
                break;
            case 'ArrowDown':
                e.preventDefault();
                this.moveCell(row + 1, col);
                this.currentDirection = 'down';
                break;
            case 'ArrowUp':
                e.preventDefault();
                this.moveCell(row - 1, col);
                this.currentDirection = 'down';
                break;
            case 'Backspace':
                if (!e.target.value) {
                    e.preventDefault();
                    this.moveToPreviousCell();
                }
                break;
            case ' ':
                e.preventDefault();
                this.currentDirection = this.currentDirection === 'across' ? 'down' : 'across';
                this.clearHighlights();
                this.selectCell(this.currentCell);
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
        if (input) {
            input.focus();
        }
    }

    moveToNextCell() {
        if (!this.currentCell) return;

        const row = parseInt(this.currentCell.dataset.row);
        const col = parseInt(this.currentCell.dataset.col);

        if (this.currentDirection === 'across') {
            for (let c = col + 1; c < this.data.size.cols; c++) {
                if (this.data.grid[row][c] !== '#') {
                    this.moveCell(row, c);
                    return;
                }
            }
        } else {
            for (let r = row + 1; r < this.data.size.rows; r++) {
                if (this.data.grid[r][col] !== '#') {
                    this.moveCell(r, col);
                    return;
                }
            }
        }
    }

    moveToPreviousCell() {
        if (!this.currentCell) return;

        const row = parseInt(this.currentCell.dataset.row);
        const col = parseInt(this.currentCell.dataset.col);

        if (this.currentDirection === 'across') {
            for (let c = col - 1; c >= 0; c--) {
                if (this.data.grid[row][c] !== '#') {
                    this.moveCell(row, c);
                    return;
                }
            }
        } else {
            for (let r = row - 1; r >= 0; r--) {
                if (this.data.grid[r][col] !== '#') {
                    this.moveCell(r, col);
                    return;
                }
            }
        }
    }

    checkAnswers() {
        const inputs = document.querySelectorAll('.cell:not(.black) input');
        let allCorrect = true;

        inputs.forEach(input => {
            const cell = input.parentElement;
            cell.classList.remove('correct', 'incorrect');

            if (input.value) {
                if (input.value.toUpperCase() === input.dataset.answer) {
                    cell.classList.add('correct');
                } else {
                    cell.classList.add('incorrect');
                    allCorrect = false;
                }
            } else {
                allCorrect = false;
            }
        });

        if (allCorrect) {
            setTimeout(() => {
                alert('Congratulations! You solved the puzzle correctly!');
            }, 100);
        }
    }

    revealAnswers() {
        const inputs = document.querySelectorAll('.cell:not(.black) input');
        inputs.forEach(input => {
            input.value = input.dataset.answer;
            const cell = input.parentElement;
            cell.classList.remove('incorrect');
            cell.classList.add('correct');
        });
    }

    clearGrid() {
        const inputs = document.querySelectorAll('.cell:not(.black) input');
        inputs.forEach(input => {
            input.value = '';
            const cell = input.parentElement;
            cell.classList.remove('correct', 'incorrect');
        });
    }
}

// Fetch puzzle data from the API
async function fetchPuzzle(puzzleId = 'puzzle2') {
    try {
        const response = await fetch(`${API_BASE_URL}/api/crossword/puzzle/${puzzleId}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        return transformPuzzleData(data);
    } catch (error) {
        console.error('Error fetching puzzle:', error);
        alert('Error loading puzzle. Please make sure the server is running.');
        throw error;
    }
}

// Transform API response to match the expected format
function transformPuzzleData(apiData) {
    return {
        id: apiData.id,
        title: apiData.title,
        size: {
            rows: apiData.size.rows,
            cols: apiData.size.cols
        },
        grid: apiData.grid,
        clues: {
            across: apiData.clues.across,
            down: apiData.clues.down
        }
    };
}

// Initialize the crossword puzzle when the page loads
document.addEventListener('DOMContentLoaded', async () => {
    try {
        // Get puzzle ID from URL parameter or use default
        const urlParams = new URLSearchParams(window.location.search);
        const puzzleId = urlParams.get('puzzle') || 'puzzle2';
        
        const puzzleData = await fetchPuzzle(puzzleId);
        
        // Update page title if puzzle has a title
        if (puzzleData.title) {
            document.querySelector('h1').textContent = puzzleData.title;
        }
        
        new CrosswordPuzzle(puzzleData);
    } catch (error) {
        console.error('Failed to initialize crossword:', error);
    }
});
