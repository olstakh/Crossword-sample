/**
 * Azure AD Authentication for Admin Pages
 * Automatically handles authentication when server requires it
 */

class AzureAuthManager {
    constructor() {
        this.msalInstance = null;
        this.config = null;
        this.initPromise = null;
    }

    /**
     * Initialize MSAL with configuration from server or defaults
     */
    async initialize() {
        if (this.initPromise) {
            return this.initPromise;
        }

        this.initPromise = (async () => {
            try {
                // Try to get config from server (you could add an endpoint for this)
                // For now, use embedded config
                this.config = {
                    auth: {
                        clientId: "400c2bdc-c965-413a-b3e9-b525afc37126",
                        authority: "https://login.microsoftonline.com/b2f2383b-bdc5-4372-b4c3-9b0308fcb4dc",
                        redirectUri: window.location.origin
                    },
                    cache: {
                        cacheLocation: "sessionStorage",
                        storeAuthStateInCookie: false
                    }
                };

                this.msalInstance = new msal.PublicClientApplication(this.config);
                await this.msalInstance.initialize();
                
                // Handle redirect promise (for popup/redirect flow)
                await this.msalInstance.handleRedirectPromise();
                
                console.log("Azure AD authentication initialized");
            } catch (error) {
                console.error("Failed to initialize Azure AD:", error);
                throw error;
            }
        })();

        return this.initPromise;
    }

    /**
     * Get access token silently or with user interaction
     */
    async getAccessToken() {
        if (!this.msalInstance) {
            await this.initialize();
        }

        const loginRequest = {
            scopes: [`${this.config.auth.clientId}/.default`]
        };

        try {
            const accounts = this.msalInstance.getAllAccounts();
            
            if (accounts.length === 0) {
                // No account found, need to login
                console.log("No account found, initiating login...");
                const response = await this.msalInstance.loginPopup(loginRequest);
                return response.accessToken;
            }

            // Try silent token acquisition
            const silentRequest = {
                ...loginRequest,
                account: accounts[0]
            };

            try {
                const response = await this.msalInstance.acquireTokenSilent(silentRequest);
                return response.accessToken;
            } catch (silentError) {
                // Silent acquisition failed, try interactive
                console.log("Silent token acquisition failed, using popup...");
                const response = await this.msalInstance.acquireTokenPopup(loginRequest);
                return response.accessToken;
            }
        } catch (error) {
            console.error("Failed to acquire token:", error);
            throw error;
        }
    }

    /**
     * Sign out the current user
     */
    async logout() {
        if (!this.msalInstance) {
            return;
        }

        const accounts = this.msalInstance.getAllAccounts();
        if (accounts.length > 0) {
            await this.msalInstance.logoutPopup({
                account: accounts[0]
            });
        }
    }

    /**
     * Check if user is logged in
     */
    isLoggedIn() {
        if (!this.msalInstance) {
            return false;
        }
        return this.msalInstance.getAllAccounts().length > 0;
    }

    /**
     * Get current user info
     */
    getCurrentUser() {
        if (!this.msalInstance) {
            return null;
        }
        
        const accounts = this.msalInstance.getAllAccounts();
        if (accounts.length > 0) {
            return {
                username: accounts[0].username,
                name: accounts[0].name,
                id: accounts[0].localAccountId
            };
        }
        return null;
    }
}

/**
 * Enhanced fetch wrapper that handles authentication automatically
 * Usage: authenticatedFetch('/api/admin/puzzles')
 */
const authManager = new AzureAuthManager();

async function authenticatedFetch(url, options = {}) {
    // First attempt: Try without authentication
    let response = await fetch(url, options);

    // If unauthorized, acquire token and retry
    if (response.status === 401) {
        console.log("Received 401, attempting Azure AD authentication...");
        
        try {
            const token = await authManager.getAccessToken();
            
            // Retry with Bearer token
            const authOptions = {
                ...options,
                headers: {
                    ...options.headers,
                    'Authorization': `Bearer ${token}`
                }
            };

            response = await fetch(url, authOptions);
            
            if (response.status === 401) {
                // Still unauthorized after authentication
                throw new Error("Access denied. You may not have the required Admin role.");
            }
        } catch (authError) {
            console.error("Authentication failed:", authError);
            throw new Error(`Authentication failed: ${authError.message}`);
        }
    }

    return response;
}

/**
 * Convenience method for JSON responses with authentication
 */
async function authenticatedFetchJSON(url, options = {}) {
    const response = await authenticatedFetch(url, options);
    
    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`API error (${response.status}): ${errorText}`);
    }
    
    return response.json();
}

// Export for use in other scripts
window.authManager = authManager;
window.authenticatedFetch = authenticatedFetch;
window.authenticatedFetchJSON = authenticatedFetchJSON;
