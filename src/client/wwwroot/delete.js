// API endpoint
const API_BASE_URL = '/api/admin';

// State management
let puzzles = [];
let selectedPuzzleIds = new Set();

// DOM elements
let puzzlesTableBody, selectAllCheckbox, deleteSelectedBtn, selectionCount, messageBox, refreshBtn;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Get DOM elements
    puzzlesTableBody = document.getElementById('puzzlesTableBody');
    selectAllCheckbox = document.getElementById('selectAllCheckbox');
    deleteSelectedBtn = document.getElementById('deleteSelectedBtn');
    selectionCount = document.getElementById('selectionCount');
    messageBox = document.getElementById('messageBox');
    refreshBtn = document.getElementById('refreshBtn');
    
    // Event listeners
    selectAllCheckbox.addEventListener('change', handleSelectAll);
    deleteSelectedBtn.addEventListener('click', handleDeleteSelected);
    refreshBtn.addEventListener('click', loadPuzzles);
    
    // Initial load
    loadPuzzles();
});

// Load all puzzles from server
async function loadPuzzles() {
    try {
        showMessage('Loading puzzles...', 'info');
        
        const response = await authenticatedFetch(`${API_BASE_URL}/puzzles`);
        
        if (!response.ok) {
            throw new Error(`Failed to load puzzles: ${response.statusText}`);
        }
        
        puzzles = await response.json();
        selectedPuzzleIds.clear();
        
        renderPuzzlesTable();
        updateSelectionUI();
        hideMessage();
        
    } catch (error) {
        console.error('Error loading puzzles:', error);
        showMessage(`Error loading puzzles: ${error.message}`, 'error');
        puzzlesTableBody.innerHTML = `
            <tr>
                <td colspan="7" class="empty-cell">Failed to load puzzles. Please try again.</td>
            </tr>
        `;
    }
}

// Render puzzles table
function renderPuzzlesTable() {
    if (puzzles.length === 0) {
        puzzlesTableBody.innerHTML = `
            <tr>
                <td colspan="7" class="empty-cell">No puzzles found. Create some puzzles first!</td>
            </tr>
        `;
        selectAllCheckbox.disabled = true;
        return;
    }
    
    selectAllCheckbox.disabled = false;
    
    puzzlesTableBody.innerHTML = puzzles.map(puzzle => `
        <tr data-puzzle-id="${puzzle.id}">
            <td class="checkbox-cell">
                <input type="checkbox" class="puzzle-checkbox" data-puzzle-id="${puzzle.id}">
            </td>
            <td><span class="puzzle-id">${puzzle.id}</span></td>
            <td>${escapeHtml(puzzle.title)}</td>
            <td>${escapeHtml(puzzle.language)}</td>
            <td class="size-cell">${puzzle.grid.length}</td>
            <td class="size-cell">${puzzle.grid[0]?.length || 0}</td>
            <td class="actions-cell">
                <button class="btn-danger btn-small delete-single-btn" data-puzzle-id="${puzzle.id}">
                    Delete
                </button>
            </td>
        </tr>
    `).join('');
    
    // Add event listeners to checkboxes
    document.querySelectorAll('.puzzle-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', handleCheckboxChange);
    });
    
    // Add event listeners to delete buttons
    document.querySelectorAll('.delete-single-btn').forEach(btn => {
        btn.addEventListener('click', handleDeleteSingle);
    });
}

// Handle select all checkbox
function handleSelectAll(event) {
    const isChecked = event.target.checked;
    
    document.querySelectorAll('.puzzle-checkbox').forEach(checkbox => {
        checkbox.checked = isChecked;
        const puzzleId = checkbox.dataset.puzzleId;
        
        if (isChecked) {
            selectedPuzzleIds.add(puzzleId);
        } else {
            selectedPuzzleIds.delete(puzzleId);
        }
    });
    
    updateSelectionUI();
}

// Handle individual checkbox change
function handleCheckboxChange(event) {
    const puzzleId = event.target.dataset.puzzleId;
    
    if (event.target.checked) {
        selectedPuzzleIds.add(puzzleId);
    } else {
        selectedPuzzleIds.delete(puzzleId);
        selectAllCheckbox.checked = false;
    }
    
    // Update select all if all are checked
    const allCheckboxes = document.querySelectorAll('.puzzle-checkbox');
    const checkedCheckboxes = document.querySelectorAll('.puzzle-checkbox:checked');
    selectAllCheckbox.checked = allCheckboxes.length === checkedCheckboxes.length && allCheckboxes.length > 0;
    
    updateSelectionUI();
}

// Handle delete single puzzle
async function handleDeleteSingle(event) {
    const puzzleId = event.target.dataset.puzzleId;
    const puzzle = puzzles.find(p => p.id === puzzleId);
    
    if (!confirm(`Are you sure you want to delete puzzle "${puzzle?.title || puzzleId}"?`)) {
        return;
    }
    
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/puzzles/${puzzleId}`, {
            method: 'DELETE'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to delete puzzle: ${response.statusText}`);
        }
        
        showMessage(`Successfully deleted puzzle: ${puzzleId}`, 'success');
        await loadPuzzles();
        
    } catch (error) {
        console.error('Error deleting puzzle:', error);
        showMessage(`Error deleting puzzle: ${error.message}`, 'error');
    }
}

// Handle delete selected puzzles
async function handleDeleteSelected() {
    if (selectedPuzzleIds.size === 0) {
        return;
    }
    
    const count = selectedPuzzleIds.size;
    if (!confirm(`Are you sure you want to delete ${count} puzzle${count > 1 ? 's' : ''}?`)) {
        return;
    }
    
    try {
        deleteSelectedBtn.disabled = true;
        showMessage(`Deleting ${count} puzzle${count > 1 ? 's' : ''}...`, 'info');
        
        const response = await authenticatedFetch(`${API_BASE_URL}/puzzles/delete-bulk`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                puzzleIds: Array.from(selectedPuzzleIds)
            })
        });
        
        if (!response.ok) {
            throw new Error(`Failed to delete puzzles: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        // Build success message
        let message = '';
        if (result.deletedIds && result.deletedIds.length > 0) {
            message += `Successfully deleted ${result.deletedIds.length} puzzle${result.deletedIds.length > 1 ? 's' : ''}`;
        }
        
        if (result.errors && result.errors.length > 0) {
            if (message) message += '. ';
            message += `Failed to delete ${result.errors.length}: ${result.errors.join(', ')}`;
            showMessage(message, 'error');
        } else {
            showMessage(message, 'success');
        }
        
        await loadPuzzles();
        
    } catch (error) {
        console.error('Error deleting puzzles:', error);
        showMessage(`Error deleting puzzles: ${error.message}`, 'error');
    } finally {
        deleteSelectedBtn.disabled = false;
    }
}

// Update selection UI
function updateSelectionUI() {
    const count = selectedPuzzleIds.size;
    selectionCount.textContent = `${count} selected`;
    deleteSelectedBtn.disabled = count === 0;
}

// Show message
function showMessage(text, type = 'info') {
    messageBox.textContent = text;
    messageBox.className = `message-box ${type}`;
}

// Hide message
function hideMessage() {
    messageBox.className = 'message-box hidden';
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
