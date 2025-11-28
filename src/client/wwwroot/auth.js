// User Authentication and Progress Tracking
// This module handles anonymous user IDs and tracks puzzle progress

class UserAuth {
    constructor() {
        this.userId = this.getOrCreateUserId();
        this.solvedPuzzles = new Set();
        this.totalSolved = 0;
        this.progressLoaded = false;
    }

    /**
     * Initialize and load progress from server
     * Must be called after construction
     */
    async init() {
        await this.loadProgress();
        return this;
    }

    /**
     * Get or create anonymous user ID
     * Stored in localStorage for persistence across sessions
     */
    getOrCreateUserId() {
        let userId = localStorage.getItem('cryptogram_user_id');
        
        if (!userId) {
            // Generate a unique anonymous ID
            userId = 'anon_' + this.generateUUID();
            localStorage.setItem('cryptogram_user_id', userId);
            console.log('Created new anonymous user ID:', userId);
        } else {
            console.log('Existing user ID:', userId);
        }
        
        return userId;
    }

    /**
     * Generate a UUID for anonymous users
     */
    generateUUID() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    /**
     * Load user progress from server
     */
    async loadProgress() {
        try {
            const response = await fetch(`${API_BASE_URL}/api/user/progress/${this.userId}`);
            if (response.ok) {
                const progress = await response.json();
                this.solvedPuzzles = new Set(progress.solvedPuzzleIds || []);
                this.totalSolved = progress.totalPuzzlesSolved || this.solvedPuzzles.size;
                this.progressLoaded = true;
                console.log(`Loaded progress: ${this.totalSolved} puzzles solved`);
                
                // Update UI to show stats
                this.updateProgressUI();
            }
        } catch (error) {
            console.error('Error loading progress:', error);
        }
    }

    /**
     * Record that user solved a puzzle
     */
    async recordSolved(puzzleId) {
        // Add to local set immediately
        this.solvedPuzzles.add(puzzleId);
        
        try {
            const response = await fetch(`${API_BASE_URL}/api/user/solved`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    userId: this.userId,
                    puzzleId: puzzleId
                })
            });
            
            if (response.ok) {
                console.log(`Puzzle ${puzzleId} marked as solved`);
                // Reload progress from server to get accurate count
                await this.loadProgress();
                this.showCongratulations();
            }
        } catch (error) {
            console.error('Error recording solved puzzle:', error);
        }
    }

    /**
     * Get available (unsolved) puzzles for current language
     */
    async getAvailablePuzzles(language = 'English') {
        try {
            const response = await fetch(
                `${API_BASE_URL}/api/user/available/${this.userId}?language=${language}`
            );
            
            if (response.ok) {
                const data = await response.json();
                return data;
            }
        } catch (error) {
            console.error('Error fetching available puzzles:', error);
        }
        
        return null;
    }

    /**
     * Check if user has solved a specific puzzle
     */
    hasSolved(puzzleId) {
        return this.solvedPuzzles.has(puzzleId);
    }

    /**
     * Update UI to show progress stats
     */
    updateProgressUI() {
        const statsElement = document.getElementById('userStats');
        if (statsElement) {
            statsElement.innerHTML = `
                <div class="stats-badge">
                    <span class="stats-icon">🏆</span>
                    <span class="stats-count">${this.totalSolved}</span>
                    <span class="stats-label">Solved</span>
                </div>
            `;
        }
    }

    /**
     * Show congratulations message when puzzle is solved
     */
    showCongratulations() {
        // Create toast notification
        const toast = document.createElement('div');
        toast.className = 'congrats-toast';
        
        toast.innerHTML = `
            <div class="toast-content">
                <span class="toast-icon">🎉</span>
                <div class="toast-text">
                    <strong>Puzzle Solved!</strong>
                    <span class="toast-count">Total: ${this.totalSolved} puzzle${this.totalSolved !== 1 ? 's' : ''}</span>
                </div>
            </div>
        `;
        
        document.body.appendChild(toast);
        
        // Animate in
        setTimeout(() => toast.classList.add('show'), 10);
        
        // Auto-dismiss after 3 seconds
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => {
                if (document.body.contains(toast)) {
                    document.body.removeChild(toast);
                }
            }, 300);
        }, 3000);
    }

    /**
     * Get a random unsolved puzzle
     */
    async getRandomUnsolvedPuzzle(size, language) {
        const available = await this.getAvailablePuzzles(language);
        
        if (!available || available.unsolvedPuzzleIds.length === 0) {
            return null;
        }
        
        // Filter by size if needed (this requires puzzle metadata)
        const randomIndex = Math.floor(Math.random() * available.unsolvedPuzzleIds.length);
        return available.unsolvedPuzzleIds[randomIndex];
    }
}

// Global user auth instance - will be initialized async
let userAuth = new UserAuth();
