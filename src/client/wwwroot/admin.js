class PuzzleBuilder {
    constructor() {
        this.grid = [];
        this.rows = 5;
        this.cols = 5;
        this.currentCell = null;
        this.puzzleId = 'puzzle4';
        this.puzzleTitle = 'My Custom Puzzle';
        
        this.init();
    }
    
    init() {
        this.setupEventListeners();
        this.updateJsonPreview();
    }
    
    setupEventListeners() {
        // Settings inputs
        document.getElementById('puzzleId').addEventListener('input', (e) => {
            this.puzzleId = e.target.value;
            this.updateJsonPreview();
        });
        
        document.getElementById('puzzleTitle').addEventListener('input', (e) => {
            this.puzzleTitle = e.target.value;
            this.updateJsonPreview();
        });
        
        document.getElementById('gridRows').addEventListener('input', (e) => {
            this.rows = parseInt(e.target.value) || 5;
        });
        
        document.getElementById('gridCols').addEventListener('input', (e) => {
            this.cols = parseInt(e.target.value) || 5;
        });
        
        // Buttons
        document.getElementById('createGridBtn').addEventListener('click', () => {
            this.createGrid();
        });
        
        document.getElementById('clearGridBtn').addEventListener('click', () => {
            this.clearGrid();
        });
        
        document.getElementById('fillSampleBtn').addEventListener('click', () => {
            this.fillSampleData();
        });
        
        document.getElementById('copyJsonBtn').addEventListener('click', () => {
            this.copyJson();
        });
        
        document.getElementById('downloadJsonBtn').addEventListener('click', () => {
            this.downloadJson();
        });
        
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (this.currentCell && !e.target.matches('input[type="text"], input[type="number"]')) {
                this.handleKeydown(e);
            }
        });
    }
    
    createGrid() {
        // Initialize empty grid
        this.grid = [];
        for (let i = 0; i < this.rows; i++) {
            this.grid[i] = [];
            for (let j = 0; j < this.cols; j++) {
                this.grid[i][j] = '#';
            }
        }
        
        this.renderGrid();
        this.updateJsonPreview();
    }
    
    renderGrid() {
        const gridEditor = document.getElementById('gridEditor');
        gridEditor.innerHTML = '';
        gridEditor.className = 'grid-editor-container';
        
        // Create grid element
        const gridElement = document.createElement('div');
        gridElement.className = 'editor-grid';
        gridElement.style.gridTemplateColumns = `repeat(${this.cols}, 50px)`;
        gridElement.style.gridTemplateRows = `repeat(${this.rows}, 50px)`;
        
        for (let row = 0; row < this.rows; row++) {
            for (let col = 0; col < this.cols; col++) {
                const cell = this.createCell(row, col);
                gridElement.appendChild(cell);
            }
        }
        
        gridEditor.appendChild(gridElement);
    }
    
    createCell(row, col) {
        const cellDiv = document.createElement('div');
        cellDiv.className = 'editor-cell';
        cellDiv.dataset.row = row;
        cellDiv.dataset.col = col;
        cellDiv.tabIndex = 0;
        
        const value = this.grid[row][col];
        if (value === '#') {
            cellDiv.classList.add('black');
        } else {
            cellDiv.textContent = value;
        }
        
        // Click to select
        cellDiv.addEventListener('click', () => {
            this.selectCell(cellDiv);
        });
        
        // Keyboard input
        cellDiv.addEventListener('keydown', (e) => {
            this.handleCellKeydown(e, cellDiv);
        });
        
        return cellDiv;
    }
    
    selectCell(cell) {
        // Remove previous selection
        document.querySelectorAll('.editor-cell').forEach(c => {
            c.classList.remove('selected');
        });
        
        this.currentCell = cell;
        cell.classList.add('selected');
        cell.focus();
    }
    
    handleCellKeydown(e, cell) {
        const row = parseInt(cell.dataset.row);
        const col = parseInt(cell.dataset.col);
        
        // Check if it's a letter
        if (e.key.length === 1 && e.key.match(/[a-zA-Z]/)) {
            e.preventDefault();
            const letter = e.key.toUpperCase();
            this.setCellValue(row, col, letter);
            this.moveToNextCell(row, col);
        }
        // Check for black cell markers
        else if (e.key === '#' || e.key === ' ') {
            e.preventDefault();
            this.setCellValue(row, col, '#');
            this.moveToNextCell(row, col);
        }
        // Backspace to clear
        else if (e.key === 'Backspace') {
            e.preventDefault();
            this.setCellValue(row, col, '#');
        }
        // Arrow keys
        else if (e.key.startsWith('Arrow')) {
            e.preventDefault();
            this.handleArrowKey(e.key, row, col);
        }
    }
    
    handleKeydown(e) {
        if (!this.currentCell) return;
        
        const row = parseInt(this.currentCell.dataset.row);
        const col = parseInt(this.currentCell.dataset.col);
        
        if (e.key.startsWith('Arrow')) {
            e.preventDefault();
            this.handleArrowKey(e.key, row, col);
        }
    }
    
    handleArrowKey(key, row, col) {
        let newRow = row;
        let newCol = col;
        
        switch (key) {
            case 'ArrowUp':
                newRow = Math.max(0, row - 1);
                break;
            case 'ArrowDown':
                newRow = Math.min(this.rows - 1, row + 1);
                break;
            case 'ArrowLeft':
                newCol = Math.max(0, col - 1);
                break;
            case 'ArrowRight':
                newCol = Math.min(this.cols - 1, col + 1);
                break;
        }
        
        const nextCell = document.querySelector(`[data-row="${newRow}"][data-col="${newCol}"]`);
        if (nextCell) {
            this.selectCell(nextCell);
        }
    }
    
    moveToNextCell(row, col) {
        // Move right, or wrap to next row
        let nextRow = row;
        let nextCol = col + 1;
        
        if (nextCol >= this.cols) {
            nextCol = 0;
            nextRow = row + 1;
        }
        
        if (nextRow < this.rows) {
            const nextCell = document.querySelector(`[data-row="${nextRow}"][data-col="${nextCol}"]`);
            if (nextCell) {
                this.selectCell(nextCell);
            }
        }
    }
    
    setCellValue(row, col, value) {
        this.grid[row][col] = value;
        
        const cell = document.querySelector(`[data-row="${row}"][data-col="${col}"]`);
        if (cell) {
            if (value === '#') {
                cell.classList.add('black');
                cell.textContent = '';
            } else {
                cell.classList.remove('black');
                cell.textContent = value;
            }
        }
        
        this.updateJsonPreview();
    }
    
    clearGrid() {
        for (let row = 0; row < this.rows; row++) {
            for (let col = 0; col < this.cols; col++) {
                this.grid[row][col] = '#';
            }
        }
        this.renderGrid();
        this.updateJsonPreview();
    }
    
    fillSampleData() {
        // Fill with a simple sample pattern
        const samples = [
            ['C', 'A', 'T', 'S', '#'],
            ['O', '#', 'O', '#', 'D'],
            ['D', 'O', 'G', 'S', '#'],
            ['E', '#', '#', '#', 'A'],
            ['#', 'R', 'A', 'T', 'S']
        ];
        
        for (let row = 0; row < Math.min(this.rows, samples.length); row++) {
            for (let col = 0; col < Math.min(this.cols, samples[row].length); col++) {
                this.grid[row][col] = samples[row][col];
            }
        }
        
        this.renderGrid();
        this.updateJsonPreview();
    }
    
    getPuzzleObject() {
        return {
            Id: this.puzzleId,
            Title: this.puzzleTitle,
            Size: {
                Rows: this.rows,
                Cols: this.cols
            },
            Grid: this.grid
        };
    }
    
    updateJsonPreview() {
        const puzzleObj = this.getPuzzleObject();
        const jsonString = JSON.stringify(puzzleObj, null, 2);
        
        const previewElement = document.getElementById('jsonPreview');
        if (previewElement) {
            previewElement.textContent = jsonString;
        }
    }
    
    copyJson() {
        const puzzleObj = this.getPuzzleObject();
        const jsonString = JSON.stringify(puzzleObj, null, 2);
        
        navigator.clipboard.writeText(jsonString).then(() => {
            // Show success feedback
            const btn = document.getElementById('copyJsonBtn');
            const originalText = btn.textContent;
            btn.textContent = '✓ Copied!';
            btn.classList.add('success');
            
            setTimeout(() => {
                btn.textContent = originalText;
                btn.classList.remove('success');
            }, 2000);
        }).catch(err => {
            console.error('Failed to copy:', err);
            alert('Failed to copy to clipboard. Please copy manually from the preview.');
        });
    }
    
    downloadJson() {
        const puzzleObj = this.getPuzzleObject();
        const jsonString = JSON.stringify(puzzleObj, null, 2);
        
        // Create blob and download
        const blob = new Blob([jsonString], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.puzzleId}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        
        // Show success feedback
        const btn = document.getElementById('downloadJsonBtn');
        const originalText = btn.textContent;
        btn.textContent = '✓ Downloaded!';
        btn.classList.add('success');
        
        setTimeout(() => {
            btn.textContent = originalText;
            btn.classList.remove('success');
        }, 2000);
    }
}

// Initialize the puzzle builder when the page loads
document.addEventListener('DOMContentLoaded', () => {
    new PuzzleBuilder();
});
