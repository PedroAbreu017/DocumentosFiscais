// ============================================
// DARK MODE CONTROLLER
// ============================================

class ThemeController {
    constructor() {
        this.themes = ['light', 'dark'];
        this.currentTheme = this.getStoredTheme() || this.getSystemTheme();
        this.init();
    }

    init() {
        this.createToggleButton();
        this.applyTheme(this.currentTheme);
        this.bindEvents();
        this.watchSystemTheme();
    }

    // Cria o botÃ£o de toggle do tema (removido - nÃ£o criar mais)
    createToggleButton() {
        // Toggle removido da interface
        // Funcionalidade mantida apenas via atalho de teclado
        return;
    }

    // Atualiza o estado visual do toggle
    updateToggleState() {
        const toggle = document.querySelector('.theme-toggle');
        const lightOption = toggle?.querySelector('.theme-option.light');
        const darkOption = toggle?.querySelector('.theme-option.dark');

        if (!toggle) return;

        toggle.setAttribute('data-theme', this.currentTheme);
        
        // Remove active de todos
        lightOption?.classList.remove('active');
        darkOption?.classList.remove('active');
        
        // Adiciona active no atual
        if (this.currentTheme === 'dark') {
            darkOption?.classList.add('active');
        } else {
            lightOption?.classList.add('active');
        }
    }

    // Bind eventos
    bindEvents() {
        // Click no toggle
        document.addEventListener('click', (e) => {
            const toggle = e.target.closest('.theme-toggle');
            const option = e.target.closest('.theme-option');
            
            if (toggle && !option) {
                this.toggleTheme();
            } else if (option) {
                const theme = option.getAttribute('data-theme');
                this.setTheme(theme);
            }
        });

        // Keyboard navigation
        document.addEventListener('keydown', (e) => {
            const toggle = e.target.closest('.theme-toggle');
            if (!toggle) return;

            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.toggleTheme();
            } else if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
                e.preventDefault();
                this.toggleTheme();
            }
        });
    }

    // Observa mudanÃ§as no tema do sistema
    watchSystemTheme() {
        if (window.matchMedia) {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            
            mediaQuery.addEventListener('change', (e) => {
                // SÃ³ muda automaticamente se nÃ£o hÃ¡ preferÃªncia salva
                if (!localStorage.getItem('theme-preference')) {
                    this.setTheme(e.matches ? 'dark' : 'light');
                }
            });
        }
    }

    // ObtÃ©m tema do sistema
    getSystemTheme() {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        return 'light';
    }

    // ObtÃ©m tema armazenado
    getStoredTheme() {
        return localStorage.getItem('theme-preference');
    }

    // Armazena tema
    storeTheme(theme) {
        localStorage.setItem('theme-preference', theme);
    }

    // Aplica tema
    applyTheme(theme) {
        // Adiciona classe para desabilitar transiÃ§Ãµes durante mudanÃ§a
        document.body.classList.add('theme-transition-disable');
        
        // Remove tema anterior
        document.documentElement.removeAttribute('data-theme');
        
        // Aplica novo tema
        if (theme === 'dark') {
            document.documentElement.setAttribute('data-theme', 'dark');
        }
        
        // ForÃ§a reflow
        document.documentElement.offsetHeight;
        
        // Remove classe de transiÃ§Ã£o apÃ³s aplicar
        setTimeout(() => {
            document.body.classList.remove('theme-transition-disable');
        }, 10);

        this.currentTheme = theme;
        this.updateToggleState();
        this.updateCharts();
        this.triggerThemeChangeEvent();
    }

    // Define tema
    setTheme(theme) {
        if (!this.themes.includes(theme)) return;
        
        this.applyTheme(theme);
        this.storeTheme(theme);
    }

    // Alterna tema
    toggleTheme() {
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.setTheme(newTheme);
    }

    // Atualiza grÃ¡ficos para o novo tema
    updateCharts() {
        if (typeof Chart !== 'undefined') {
            Object.values(chartInstances || {}).forEach(chart => {
                const isDark = this.currentTheme === 'dark';
                
                // Atualiza cores dos grÃ¡ficos
                if (chart.options.plugins?.legend?.labels) {
                    chart.options.plugins.legend.labels.color = isDark ? '#f9fafb' : '#1f2937';
                }
                
                if (chart.options.scales) {
                    Object.keys(chart.options.scales).forEach(scaleKey => {
                        const scale = chart.options.scales[scaleKey];
                        if (scale.ticks) {
                            scale.ticks.color = isDark ? '#d1d5db' : '#6b7280';
                        }
                        if (scale.grid) {
                            scale.grid.color = isDark ? '#374151' : '#e5e7eb';
                        }
                    });
                }
                
                chart.update('none');
            });
        }
    }

    // Dispara evento customizado para outros componentes
    triggerThemeChangeEvent() {
        const event = new CustomEvent('themechange', {
            detail: { theme: this.currentTheme }
        });
        document.dispatchEvent(event);
    }

    // ObtÃ©m tema atual
    getCurrentTheme() {
        return this.currentTheme;
    }

    // Verifica se estÃ¡ no modo escuro
    isDarkMode() {
        return this.currentTheme === 'dark';
    }
}

// ============================================
// THEME UTILITIES
// ============================================

class ThemeUtils {
    // ObtÃ©m cor CSS atual
    static getCSSVariable(variable) {
        return getComputedStyle(document.documentElement)
            .getPropertyValue(variable)
            .trim();
    }

    // Define cor CSS
    static setCSSVariable(variable, value) {
        document.documentElement.style.setProperty(variable, value);
    }

    // Gera paleta de cores baseada no tema
    static getColorPalette() {
        const theme = window.themeController?.getCurrentTheme() || 'light';
        
        return {
            primary: this.getCSSVariable('--primary-color'),
            secondary: this.getCSSVariable('--secondary-color'),
            success: this.getCSSVariable('--success-color'),
            warning: this.getCSSVariable('--warning-color'),
            error: this.getCSSVariable('--error-color'),
            info: this.getCSSVariable('--info-color'),
            textPrimary: this.getCSSVariable('--text-primary'),
            textSecondary: this.getCSSVariable('--text-secondary'),
            bgPrimary: this.getCSSVariable('--bg-primary'),
            bgSecondary: this.getCSSVariable('--bg-secondary'),
            borderColor: this.getCSSVariable('--border-color')
        };
    }

    // Detecta se o usuÃ¡rio prefere modo escuro
    static prefersColorScheme() {
        if (window.matchMedia) {
            return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }
        return 'light';
    }

    // Verifica se hÃ¡ suporte a CSS variables
    static supportsCSSVariables() {
        return window.CSS && CSS.supports('color', 'var(--test)');
    }
}

// ============================================
// INICIALIZAÃ‡ÃƒO E INTEGRAÃ‡ÃƒO
// ============================================

// Inicializa quando DOM estiver pronto
document.addEventListener('DOMContentLoaded', function() {
    // SÃ³ inicializa se CSS variables forem suportadas
    if (ThemeUtils.supportsCSSVariables()) {
        window.themeController = new ThemeController();
        
        // Adiciona utilities globais
        window.themeUtils = ThemeUtils;
        
        console.log('âœ… Dark Mode initialized successfully');
    } else {
        console.warn('âš ï¸ CSS Variables not supported - Dark Mode disabled');
    }
});

// Event listener para mudanÃ§as de tema
document.addEventListener('themechange', function(e) {
    const theme = e.detail.theme;
    
    // Atualiza meta theme-color para mobile
    updateMetaThemeColor(theme);
    
    // Notifica outros componentes
    if (typeof window.showNotification === 'function') {
        const message = theme === 'dark' ? 'Tema escuro ativado' : 'Tema claro ativado';
        window.showNotification(message, 'info', 2000);
    }
    
    console.log(`ðŸŽ¨ Theme changed to: ${theme}`);
});

// Atualiza cor do tema no mobile
function updateMetaThemeColor(theme) {
    let metaThemeColor = document.querySelector('meta[name="theme-color"]');
    
    if (!metaThemeColor) {
        metaThemeColor = document.createElement('meta');
        metaThemeColor.name = 'theme-color';
        document.head.appendChild(metaThemeColor);
    }
    
    const color = theme === 'dark' ? '#0f172a' : '#ffffff';
    metaThemeColor.content = color;
}

// ============================================
// API PÃšBLICA
// ============================================

// Exporta funÃ§Ãµes Ãºteis para uso global
window.darkMode = {
    toggle: () => window.themeController?.toggleTheme(),
    setTheme: (theme) => window.themeController?.setTheme(theme),
    getCurrentTheme: () => window.themeController?.getCurrentTheme(),
    isDark: () => window.themeController?.isDarkMode(),
    getColors: () => ThemeUtils.getColorPalette()
};

// Atalho de teclado global (Ctrl/Cmd + Shift + D)
document.addEventListener('keydown', function(e) {
    if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'D') {
        e.preventDefault();
        window.darkMode.toggle();
    }
});

// ============================================
// INTEGRAÃ‡ÃƒO COM COMPONENTES EXISTENTES
// ============================================

// Atualiza notificaÃ§Ãµes para tema escuro
document.addEventListener('themechange', function(e) {
    const theme = e.detail.theme;
    
    // Atualiza estilos de notificaÃ§Ãµes se existirem
    const toastContainer = document.getElementById('toast-container');
    if (toastContainer) {
        toastContainer.setAttribute('data-theme', theme);
    }
});

// Compatibilidade com upload drag & drop
document.addEventListener('themechange', function(e) {
    const uploadAreas = document.querySelectorAll('.upload-area');
    uploadAreas.forEach(area => {
        area.setAttribute('data-theme', e.detail.theme);
    });
});

// PersistÃªncia durante navegaÃ§Ã£o SPA
window.addEventListener('beforeunload', function() {
    if (window.themeController) {
        localStorage.setItem('theme-preference', window.themeController.getCurrentTheme());
    }
});


