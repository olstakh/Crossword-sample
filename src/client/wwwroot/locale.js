// Locale management for the crossword application
// Handles language selection and persistence

class LocaleManager {
    constructor() {
        this.supportedLocales = {
            'English': { code: 'en', flag: '🇺🇸', name: 'English' },
            'Russian': { code: 'ru', flag: '🇷🇺', name: 'Русский' },
            'Ukrainian': { code: 'uk', flag: '🇺🇦', name: 'Українська' }
        };
        
        this.currentLocale = this.getStoredLocale() || 'English';
        this.listeners = [];
        this.translations = {};
        this.loadTranslations();
    }

    /**
     * Load translations for current locale
     */
    async loadTranslations() {
        const localeCode = this.getAcceptLanguageHeader();
        try {
            const response = await fetch(`/locales/${localeCode}.json`);
            if (response.ok) {
                this.translations = await response.json();
            } else {
                console.warn(`Failed to load translations for ${localeCode}, using defaults`);
                this.translations = {};
            }
        } catch (error) {
            console.error('Error loading translations:', error);
            this.translations = {};
        }
    }

    /**
     * Get translated string by key
     */
    t(key, defaultValue = '') {
        return this.translations[key] || defaultValue || key;
    }

    /**
     * Get the current locale
     */
    getLocale() {
        return this.currentLocale;
    }

    /**
     * Get locale info by name
     */
    getLocaleInfo(localeName) {
        return this.supportedLocales[localeName];
    }

    /**
     * Set the current locale
     */
    setLocale(locale) {
        if (!this.supportedLocales[locale]) {
            console.warn(`Unsupported locale: ${locale}, falling back to English`);
            locale = 'English';
        }

        const oldLocale = this.currentLocale;
        this.currentLocale = locale;
        
        // Store in localStorage
        localStorage.setItem('puzzleLocale', locale);
        
        // Notify listeners if locale changed
        if (oldLocale !== locale) {
            this.notifyListeners(locale, oldLocale);
        }
    }

    /**
     * Get stored locale from localStorage
     */
    getStoredLocale() {
        return localStorage.getItem('puzzleLocale');
    }

    /**
     * Get the Accept-Language header value for the current locale
     */
    getAcceptLanguageHeader() {
        const localeInfo = this.supportedLocales[this.currentLocale];
        return localeInfo ? localeInfo.code : 'en';
    }

    /**
     * Get headers object with Accept-Language set to current locale
     */
    getLocaleHeaders(additionalHeaders = {}) {
        return {
            'Accept-Language': this.getAcceptLanguageHeader(),
            ...additionalHeaders
        };
    }

    /**
     * Register a listener for locale changes
     */
    onChange(callback) {
        this.listeners.push(callback);
    }

    /**
     * Notify all listeners of locale change
     */
    notifyListeners(newLocale, oldLocale) {
        this.listeners.forEach(callback => {
            try {
                callback(newLocale, oldLocale);
            } catch (error) {
                console.error('Error in locale change listener:', error);
            }
        });
    }

    /**
     * Initialize the locale selector UI
     */
    initializeUI() {
        const container = document.getElementById('localeSelector');
        if (!container) {
            console.warn('Locale selector container not found');
            return;
        }

        // Create flag buttons
        Object.entries(this.supportedLocales).forEach(([localeName, info]) => {
            const button = document.createElement('button');
            button.className = 'locale-flag-btn';
            button.dataset.locale = localeName;
            button.title = info.name;
            button.innerHTML = info.flag;
            
            // Mark current locale as active
            if (localeName === this.currentLocale) {
                button.classList.add('active');
            }

            // Add click handler
            button.addEventListener('click', async () => {
                const oldLocale = this.currentLocale;
                if (oldLocale === localeName) {
                    return; // No change needed
                }
                
                this.setLocale(localeName);
                this.updateActiveButton();
                
                // Clear puzzleId from URL before reloading
                const url = new URL(window.location);
                url.searchParams.delete('puzzleId');
                url.searchParams.delete('seed');
                window.history.pushState({}, '', url);
                
                // Reload the page to fetch content in new locale
                // This will trigger server requests with new Accept-Language header
                // and allow for future localization of UI text
                window.location.reload();
            });

            container.appendChild(button);
        });
        
        // Apply translations to UI elements
        this.applyTranslations();
    }
    
    /**
     * Apply translations to all elements with data-i18n attribute
     */
    applyTranslations() {
        document.querySelectorAll('[data-i18n]').forEach(el => {
            const key = el.getAttribute('data-i18n');
            const translated = this.t(key);
            
            // For buttons and some elements, keep emoji/symbols
            if (el.textContent.match(/^[😊💪🎉🏆🎊⌫]/)) {
                const emoji = el.textContent.match(/^[😊💪🎉🏆🎊⌫]+/)[0];
                el.textContent = emoji + ' ' + translated;
            } else {
                el.textContent = translated;
            }
        });
    }

    /**
     * Update the active state of flag buttons
     */
    updateActiveButton() {
        const buttons = document.querySelectorAll('.locale-flag-btn');
        buttons.forEach(button => {
            if (button.dataset.locale === this.currentLocale) {
                button.classList.add('active');
            } else {
                button.classList.remove('active');
            }
        });
    }

    /**
     * Get all supported locales
     */
    getSupportedLocales() {
        return Object.keys(this.supportedLocales);
    }
}

// Create global instance
const localeManager = new LocaleManager();

// Initialize UI when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', async () => {
        await localeManager.loadTranslations();
        localeManager.initializeUI();
    });
} else {
    (async () => {
        await localeManager.loadTranslations();
        localeManager.initializeUI();
    })();
}
