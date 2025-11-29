// API endpoint
const API_BASE_URL = window.location.origin;

// State management
let users = [];

// DOM elements
let usersTableBody, userCount, messageBox, refreshBtn;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Get DOM elements
    usersTableBody = document.getElementById('usersTableBody');
    userCount = document.getElementById('userCount');
    messageBox = document.getElementById('messageBox');
    refreshBtn = document.getElementById('refreshBtn');
    
    // Event listeners
    refreshBtn.addEventListener('click', loadUsers);
    
    // Initial load
    loadUsers();
});

// Load all users from server
async function loadUsers() {
    try {
        showMessage('Loading users...', 'info');
        
        const response = await fetch(`${API_BASE_URL}/api/user/all`);
        
        if (!response.ok) {
            throw new Error(`Failed to load users: ${response.statusText}`);
        }
        
        users = await response.json();
        
        renderUsersTable();
        hideMessage();
        
    } catch (error) {
        console.error('Error loading users:', error);
        showMessage(`Error loading users: ${error.message}`, 'error');
        usersTableBody.innerHTML = `
            <tr>
                <td colspan="2" class="empty-cell">Failed to load users. Please try again.</td>
            </tr>
        `;
    }
}

// Render users table
function renderUsersTable() {
    if (users.length === 0) {
        usersTableBody.innerHTML = `
            <tr>
                <td colspan="2" class="empty-cell">No users found.</td>
            </tr>
        `;
        userCount.textContent = '0 users';
        return;
    }
    
    userCount.textContent = `${users.length} user${users.length !== 1 ? 's' : ''}`;
    
    usersTableBody.innerHTML = users.map(userId => `
        <tr data-user-id="${escapeHtml(userId)}">
            <td>
                <a href="user-detail.html?userId=${encodeURIComponent(userId)}" class="user-id-link">
                    ${escapeHtml(userId)}
                </a>
            </td>
            <td class="actions-cell">
                <a href="user-detail.html?userId=${encodeURIComponent(userId)}" class="btn-secondary btn-small">
                    View Details
                </a>
            </td>
        </tr>
    `).join('');
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
