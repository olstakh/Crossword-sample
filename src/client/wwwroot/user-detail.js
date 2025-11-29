// API endpoint
const API_BASE_URL = window.location.origin;

// State management
let userId = null;
let solvedPuzzles = [];
let selectedPuzzleIds = new Set();

// DOM elements
let puzzlesTableBody, selectAllCheckbox, deleteSelectedBtn, selectionCount, messageBox, refreshBtn, backBtn, userIdDisplay;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Get userId from URL
    const urlParams = new URLSearchParams(window.location.search);
    userId = urlParams.get('userId');
    
    if (!userId) {
        showMessage('No user ID specified', 'error');
        return;
    }
    
    // Get DOM elements
    puzzlesTableBody = document.getElementById('puzzlesTableBody');
    selectAllCheckbox = document.getElementById('selectAllCheckbox');
    deleteSelectedBtn = document.getElementById('deleteSelectedBtn');
    selectionCount = document.getElementById('selectionCount');
    messageBox = document.getElementById('messageBox');
    refreshBtn = document.getElementById('refreshBtn');
    backBtn = document.getElementById('backBtn');
    userIdDisplay = document.getElementById('userIdDisplay');
    
    // Display user ID
    userIdDisplay.textContent = `User: ${userId}`;
    
    // Event listeners
    selectAllCheckbox.addEventListener('change', handleSelectAll);
    deleteSelectedBtn.addEventListener('click', handleDeleteSelected);
    refreshBtn.addEventListener('click', loadSolvedPuzzles);
    backBtn.addEventListener('click', () => window.location.href = 'users.html');
    
    // Initial load
    loadSolvedPuzzles();
});

// Load solved puzzles for user
async function loadSolvedPuzzles() {
    try {
        showMessage('Loading solved puzzles...', 'info');
        
        const response = await fetch(`${API_BASE_URL}/api/user/progress`, {
            headers: {
                'X-User-Id': userId
            }
        });
        
        if (!response.ok) {
            throw new Error(`Failed to load progress: ${response.statusText}`);
        }
        
        const progress = await response.json();
        solvedPuzzles = progress.solvedPuzzleIds || [];
        selectedPuzzleIds.clear();
        
        renderPuzzlesTable();
        updateSelectionUI();
        hideMessage();
        
    } catch (error) {
        console.error('Error loading solved puzzles:', error);
        showMessage(`Error loading solved puzzles: ${error.message}`, 'error');
        puzzlesTableBody.innerHTML = `
            <tr>
                <td colspan="3" class="empty-cell">Failed to load solved puzzles. Please try again.</td>
            </tr>
        `;
    }
}

// Render puzzles table
function renderPuzzlesTable() {
    if (solvedPuzzles.length === 0) {
        puzzlesTableBody.innerHTML = `
            <tr>
                <td colspan="3" class="empty-cell">No solved puzzles yet.</td>
            </tr>
        `;
        selectAllCheckbox.disabled = true;
        return;
    }
    
    selectAllCheckbox.disabled = false;
    
    puzzlesTableBody.innerHTML = solvedPuzzles.map(puzzleId => `
        <tr data-puzzle-id="${escapeHtml(puzzleId)}">
            <td class="checkbox-cell">
                <input type="checkbox" class="puzzle-checkbox" data-puzzle-id="${escapeHtml(puzzleId)}">
            </td>
            <td><span class="puzzle-id">${escapeHtml(puzzleId)}</span></td>
            <td class="actions-cell">
                <button class="btn-danger btn-small forget-single-btn" data-puzzle-id="${escapeHtml(puzzleId)}">
                    Forget
                </button>
            </td>
        </tr>
    `).join('');
    
    // Add event listeners to checkboxes
    document.querySelectorAll('.puzzle-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', handleCheckboxChange);
    });
    
    // Add event listeners to forget buttons
    document.querySelectorAll('.forget-single-btn').forEach(btn => {
        btn.addEventListener('click', handleForgetSingle);
    });
}

// Handle select all checkbox
function handleSelectAll(e) {
    const isChecked = e.target.checked;
    
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
function handleCheckboxChange(e) {
    const puzzleId = e.target.dataset.puzzleId;
    
    if (e.target.checked) {
        selectedPuzzleIds.add(puzzleId);
    } else {
        selectedPuzzleIds.delete(puzzleId);
    }
    
    // Update select all checkbox state
    const allCheckboxes = document.querySelectorAll('.puzzle-checkbox');
    const checkedCheckboxes = document.querySelectorAll('.puzzle-checkbox:checked');
    selectAllCheckbox.checked = allCheckboxes.length === checkedCheckboxes.length && allCheckboxes.length > 0;
    
    updateSelectionUI();
}

// Update selection UI
function updateSelectionUI() {
    const count = selectedPuzzleIds.size;
    selectionCount.textContent = `${count} selected`;
    deleteSelectedBtn.disabled = count === 0;
}

// Handle forget single puzzle
async function handleForgetSingle(e) {
    const puzzleId = e.target.dataset.puzzleId;
    
    if (!confirm(`Are you sure you want to forget puzzle "${puzzleId}" for this user?`)) {
        return;
    }
    
    await forgetPuzzles([puzzleId]);
}

// Handle forget selected puzzles
async function handleDeleteSelected() {
    const count = selectedPuzzleIds.size;
    
    if (count === 0) return;
    
    if (!confirm(`Are you sure you want to forget ${count} puzzle(s) for this user?`)) {
        return;
    }
    
    await forgetPuzzles(Array.from(selectedPuzzleIds));
}

// Forget puzzles via API
async function forgetPuzzles(puzzleIds) {
    try {
        showMessage(`Forgetting ${puzzleIds.length} puzzle(s)...`, 'info');
        
        const response = await fetch(`${API_BASE_URL}/api/user/forget`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-User-Id': userId
            },
            body: JSON.stringify({ puzzleIds })
        });
        
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.error || `Failed to forget puzzles: ${response.statusText}`);
        }
        
        showMessage(`Successfully forgot ${puzzleIds.length} puzzle(s)`, 'success');
        
        // Reload the list
        await loadSolvedPuzzles();
        
    } catch (error) {
        console.error('Error forgetting puzzles:', error);
        showMessage(`Error: ${error.message}`, 'error');
    }
}

// Show message to user
function showMessage(text, type = 'info') {
    messageBox.textContent = text;
    messageBox.className = `message-box ${type}`;
    messageBox.classList.remove('hidden');
}

// Hide message
function hideMessage() {
    messageBox.classList.add('hidden');
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
