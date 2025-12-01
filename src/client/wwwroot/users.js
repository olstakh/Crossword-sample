// API endpoint
const API_BASE_URL = window.location.origin;

// State management
let users = [];

// DOM elements
let usersTableBody, userCount, messageBox, refreshBtn, downloadDbBtn, uploadDbBtn, fileInput;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Get DOM elements
    usersTableBody = document.getElementById('usersTableBody');
    userCount = document.getElementById('userCount');
    messageBox = document.getElementById('messageBox');
    refreshBtn = document.getElementById('refreshBtn');
    downloadDbBtn = document.getElementById('downloadDbBtn');
    uploadDbBtn = document.getElementById('uploadDbBtn');
    fileInput = document.getElementById('fileInput');
    
    // Event listeners
    refreshBtn.addEventListener('click', loadUsers);
    downloadDbBtn.addEventListener('click', downloadDatabase);
    uploadDbBtn.addEventListener('click', () => fileInput.click());
    fileInput.addEventListener('change', handleFileUpload);
    
    // Initial load
    loadUsers();
});

// Load all users from server
async function loadUsers() {
    try {
        showMessage('Loading users...', 'info');
        
        const response = await authenticatedFetch(`${API_BASE_URL}/api/user/all`);
        
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

// Download database as JSON file
async function downloadDatabase() {
    try {
        showMessage('Downloading database...', 'info');
        downloadDbBtn.disabled = true;
        
        const response = await authenticatedFetch(`${API_BASE_URL}/api/user/progress/download`);
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.error || `Failed to download database: ${response.statusText}`);
        }
        
        const data = await response.json();
        
        // Create JSON file
        const jsonString = JSON.stringify(data, null, 2);
        const blob = new Blob([jsonString], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        
        // Create download link
        const a = document.createElement('a');
        a.href = url;
        a.download = `user-progress-${new Date().toISOString().split('T')[0]}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        
        showMessage(`Database downloaded successfully! (${data.length} records)`, 'success');
        setTimeout(hideMessage, 3000);
        
    } catch (error) {
        console.error('Error downloading database:', error);
        showMessage(`Error downloading database: ${error.message}`, 'error');
    } finally {
        downloadDbBtn.disabled = false;
    }
}

// Handle file upload
async function handleFileUpload(event) {
    const file = event.target.files[0];
    if (!file) return;
    
    try {
        showMessage('Reading file...', 'info');
        
        const text = await file.text();
        const data = JSON.parse(text);
        
        if (!Array.isArray(data)) {
            throw new Error('Invalid file format. Expected an array of user progress records.');
        }
        
        // Confirm upload
        const confirmed = confirm(
            `This will replace all existing user progress data with ${data.length} records from the file.\n\n` +
            'Are you sure you want to continue?'
        );
        
        if (!confirmed) {
            showMessage('Upload cancelled.', 'info');
            setTimeout(hideMessage, 2000);
            fileInput.value = '';
            return;
        }
        
        showMessage('Uploading database...', 'info');
        uploadDbBtn.disabled = true;
        
        const response = await authenticatedFetch(`${API_BASE_URL}/api/user/progress/upload`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.error || `Failed to upload database: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        showMessage(`Database uploaded successfully! (${result.count} records)`, 'success');
        setTimeout(() => {
            hideMessage();
            loadUsers(); // Refresh the user list
        }, 2000);
        
    } catch (error) {
        console.error('Error uploading database:', error);
        showMessage(`Error uploading database: ${error.message}`, 'error');
    } finally {
        uploadDbBtn.disabled = false;
        fileInput.value = ''; // Reset file input
    }
}
