// ============================================
// JAVASCRIPT PARA RESPONSIVIDADE MOBILE
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    initMobileFeatures();
    initResponsiveTable();
    initTouchOptimizations();
});

// ============================================
// FUNCIONALIDADES MOBILE
// ============================================

function initMobileFeatures() {
    createMobileHeader();
    createSidebarOverlay();
    initMobileMenu();
    initSwipeGestures();
}

// Cria header mobile se nÃ£o existir
function createMobileHeader() {
    if (window.innerWidth <= 575 && !document.querySelector('.mobile-header')) {
        const mobileHeader = document.createElement('div');
        mobileHeader.className = 'mobile-header d-md-none';
        mobileHeader.innerHTML = `
            <button class="mobile-menu-btn" onclick="toggleMobileSidebar()">
                <i class="bi bi-list"></i>
            </button>
            <div class="fw-bold">Documentos Fiscais</div>
            <div style="width: 40px;"></div>
        `;
        
        const mainContent = document.querySelector('.main-content');
        if (mainContent) {
            mainContent.insertBefore(mobileHeader, mainContent.firstChild);
        }
    }
}

// Cria overlay para sidebar mobile
function createSidebarOverlay() {
    if (!document.querySelector('.sidebar-overlay')) {
        const overlay = document.createElement('div');
        overlay.className = 'sidebar-overlay';
        overlay.onclick = closeMobileSidebar;
        document.body.appendChild(overlay);
    }
}

// Inicializa menu mobile
function initMobileMenu() {
    const sidebar = document.querySelector('.sidebar');
    if (sidebar) {
        // Adiciona classe mobile quando necessÃ¡rio
        function updateSidebarClass() {
            if (window.innerWidth <= 575) {
                sidebar.classList.add('mobile-sidebar');
            } else {
                sidebar.classList.remove('mobile-sidebar', 'show');
                document.querySelector('.sidebar-overlay')?.classList.remove('show');
            }
        }
        
        updateSidebarClass();
        window.addEventListener('resize', updateSidebarClass);
    }
}

// Toggle sidebar mobile
function toggleMobileSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    
    if (sidebar && overlay) {
        const isOpen = sidebar.classList.contains('show');
        
        if (isOpen) {
            closeMobileSidebar();
        } else {
            openMobileSidebar();
        }
    }
}

// Abre sidebar mobile
function openMobileSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    
    if (sidebar && overlay) {
        sidebar.classList.add('show');
        overlay.classList.add('show');
        document.body.style.overflow = 'hidden';
    }
}

// Fecha sidebar mobile
function closeMobileSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    
    if (sidebar && overlay) {
        sidebar.classList.remove('show');
        overlay.classList.remove('show');
        document.body.style.overflow = '';
    }
}

// ============================================
// TABELAS RESPONSIVAS
// ============================================

function initResponsiveTable() {
    const tables = document.querySelectorAll('.modern-table');
    
    tables.forEach(table => {
        makeTableResponsive(table);
    });
    
    window.addEventListener('resize', () => {
        tables.forEach(table => {
            makeTableResponsive(table);
        });
    });
}

function makeTableResponsive(table) {
    if (window.innerWidth <= 767) {
        // Adiciona classes mobile-hide em colunas menos importantes
        const headers = table.querySelectorAll('th');
        const rows = table.querySelectorAll('tbody tr');
        
        // Define quais colunas esconder no mobile (exemplo)
        const hideIndexes = [2, 3]; // Ajuste conforme necessÃ¡rio
        
        hideIndexes.forEach(index => {
            if (headers[index]) {
                headers[index].classList.add('mobile-hide');
            }
            
            rows.forEach(row => {
                const cell = row.children[index];
                if (cell) {
                    cell.classList.add('mobile-hide');
                }
            });
        });
    } else {
        // Remove classes mobile-hide em telas maiores
        table.querySelectorAll('.mobile-hide').forEach(element => {
            element.classList.remove('mobile-hide');
        });
    }
}

// ============================================
// OTIMIZAÃ‡Ã•ES DE TOQUE
// ============================================

function initTouchOptimizations() {
    // Previne zoom duplo toque em iOS
    let lastTouchEnd = 0;
    document.addEventListener('touchend', function (event) {
        const now = (new Date()).getTime();
        if (now - lastTouchEnd <= 300) {
            event.preventDefault();
        }
        lastTouchEnd = now;
    }, false);
    
    // Adiciona feedback visual em elementos clicÃ¡veis
    const clickableElements = document.querySelectorAll('.btn, .modern-card, .sidebar-link');
    
    clickableElements.forEach(element => {
        element.addEventListener('touchstart', function() {
            this.style.transform = 'scale(0.98)';
        }, { passive: true });
        
        element.addEventListener('touchend', function() {
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
        }, { passive: true });
    });
}

// ============================================
// GESTOS DE SWIPE
// ============================================

function initSwipeGestures() {
    let startX = 0;
    let startY = 0;
    let endX = 0;
    let endY = 0;
    
    document.addEventListener('touchstart', function(e) {
        startX = e.touches[0].clientX;
        startY = e.touches[0].clientY;
    }, { passive: true });
    
    document.addEventListener('touchmove', function(e) {
        endX = e.touches[0].clientX;
        endY = e.touches[0].clientY;
    }, { passive: true });
    
    document.addEventListener('touchend', function(e) {
        handleSwipe();
    }, { passive: true });
    
    function handleSwipe() {
        const deltaX = endX - startX;
        const deltaY = endY - startY;
        const minSwipeDistance = 100;
        
        // Swipe horizontal maior que vertical
        if (Math.abs(deltaX) > Math.abs(deltaY) && Math.abs(deltaX) > minSwipeDistance) {
            if (deltaX > 0) {
                // Swipe right - abre sidebar
                if (window.innerWidth <= 575) {
                    openMobileSidebar();
                }
            } else {
                // Swipe left - fecha sidebar
                if (window.innerWidth <= 575) {
                    closeMobileSidebar();
                }
            }
        }
    }
}

// ============================================
// UPLOAD RESPONSIVO
// ============================================

function initResponsiveUpload() {
    const uploadArea = document.querySelector('.upload-area');
    if (!uploadArea) return;
    
    // Ajusta altura baseado no dispositivo
    function adjustUploadArea() {
        if (window.innerWidth <= 575) {
            uploadArea.style.minHeight = '150px';
        } else if (window.innerWidth <= 767) {
            uploadArea.style.minHeight = '200px';
        } else {
            uploadArea.style.minHeight = '300px';
        }
    }
    
    adjustUploadArea();
    window.addEventListener('resize', adjustUploadArea);
}

// ============================================
// MODAIS RESPONSIVOS
// ============================================

function makeModalResponsive(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) return;
    
    const modalDialog = modal.querySelector('.modal-dialog');
    
    function adjustModal() {
        if (window.innerWidth <= 575) {
            modalDialog.classList.add('modal-fullscreen-sm-down');
        } else {
            modalDialog.classList.remove('modal-fullscreen-sm-down');
        }
    }
    
    adjustModal();
    window.addEventListener('resize', adjustModal);
}

// ============================================
// GRÃFICOS RESPONSIVOS
// ============================================

function makeChartsResponsive() {
    // Para Chart.js
    if (typeof Chart !== 'undefined') {
        Chart.defaults.responsive = true;
        Chart.defaults.maintainAspectRatio = false;
        
        // Redimensiona grÃ¡ficos existentes
        Object.values(chartInstances || {}).forEach(chart => {
            chart.resize();
        });
    }
}

// ============================================
// NOTIFICAÃ‡Ã•ES MOBILE
// ============================================

function adjustNotificationsForMobile() {
    const style = document.createElement('style');
    style.textContent = `
        @media (max-width: 575px) {
            #toast-container {
                top: 10px;
                right: 10px;
                left: 10px;
            }
            
            .toast-notification {
                min-width: auto;
                max-width: none;
                margin-bottom: 8px;
            }
        }
    `;
    document.head.appendChild(style);
}

// ============================================
// UTILITÃRIOS MOBILE
// ============================================

// Detecta se Ã© dispositivo touch
function isTouchDevice() {
    return (('ontouchstart' in window) ||
            (navigator.maxTouchPoints > 0) ||
            (navigator.msMaxTouchPoints > 0));
}

// Detecta orientaÃ§Ã£o
function getOrientation() {
    return window.innerHeight > window.innerWidth ? 'portrait' : 'landscape';
}

// Ajusta interface baseado na orientaÃ§Ã£o
function handleOrientationChange() {
    const orientation = getOrientation();
    document.body.setAttribute('data-orientation', orientation);
    
    // Ajusta elementos especÃ­ficos
    if (orientation === 'landscape' && window.innerWidth <= 767) {
        document.body.classList.add('landscape-mobile');
    } else {
        document.body.classList.remove('landscape-mobile');
    }
}

// Event listeners
window.addEventListener('orientationchange', function() {
    setTimeout(handleOrientationChange, 100);
});

window.addEventListener('resize', function() {
    handleOrientationChange();
    makeChartsResponsive();
});

// InicializaÃ§Ã£o
document.addEventListener('DOMContentLoaded', function() {
    handleOrientationChange();
    adjustNotificationsForMobile();
    
    // Inicializa upload responsivo se existir
    initResponsiveUpload();
    
    // Adiciona classes CSS especÃ­ficas para dispositivos
    if (isTouchDevice()) {
        document.body.classList.add('touch-device');
    } else {
        document.body.classList.add('no-touch');
    }
});

// ============================================
// KEYBOARD NAVIGATION MOBILE
// ============================================

function initMobileKeyboardNavigation() {
    // Melhora navegaÃ§Ã£o por teclado em dispositivos mÃ³veis
    document.addEventListener('keydown', function(e) {
        // ESC fecha sidebar mobile
        if (e.key === 'Escape') {
            closeMobileSidebar();
        }
        
        // Enter/Space ativa elementos focusados
        if (e.key === 'Enter' || e.key === ' ') {
            const focused = document.activeElement;
            if (focused && focused.classList.contains('mobile-menu-btn')) {
                e.preventDefault();
                toggleMobileSidebar();
            }
        }
    });
}

// ============================================
// PERFORMANCE OPTIMIZATIONS
// ============================================

// Throttle function para eventos de resize
function throttle(func, limit) {
    let inThrottle;
    return function() {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    }
}

// Debounce function para busca
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// ============================================
// ACCESSIBILITY IMPROVEMENTS
// ============================================

function initMobileAccessibility() {
    // Adiciona role e aria-labels para elementos mobile
    const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
    if (mobileMenuBtn) {
        mobileMenuBtn.setAttribute('role', 'button');
        mobileMenuBtn.setAttribute('aria-label', 'Abrir menu de navegaÃ§Ã£o');
        mobileMenuBtn.setAttribute('aria-expanded', 'false');
    }
    
    const sidebar = document.querySelector('.sidebar');
    if (sidebar) {
        sidebar.setAttribute('role', 'navigation');
        sidebar.setAttribute('aria-label', 'Menu principal');
    }
    
    // Atualiza aria-expanded quando sidebar abre/fecha
    const originalToggle = window.toggleMobileSidebar;
    window.toggleMobileSidebar = function() {
        originalToggle();
        const isOpen = sidebar?.classList.contains('show');
        if (mobileMenuBtn) {
            mobileMenuBtn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        }
    };
}

// ============================================
// MOBILE-SPECIFIC UTILITIES
// ============================================

// FunÃ§Ã£o para detectar iOS
function isIOS() {
    return /iPad|iPhone|iPod/.test(navigator.userAgent);
}

// FunÃ§Ã£o para detectar Android
function isAndroid() {
    return /Android/.test(navigator.userAgent);
}

// FunÃ§Ã£o para detectar viewport height issues no mobile
function fixMobileViewportHeight() {
    // Fix para 100vh no mobile (especialmente iOS Safari)
    function setVH() {
        const vh = window.innerHeight * 0.01;
        document.documentElement.style.setProperty('--vh', `${vh}px`);
    }
    
    setVH();
    window.addEventListener('resize', throttle(setVH, 100));
    window.addEventListener('orientationchange', function() {
        setTimeout(setVH, 100);
    });
}

// ============================================
// FORM OPTIMIZATIONS FOR MOBILE
// ============================================

function optimizeFormsForMobile() {
    const forms = document.querySelectorAll('form');
    
    forms.forEach(form => {
        // Adiciona autocomplete e inputmode apropriados
        const inputs = form.querySelectorAll('input, select, textarea');
        
        inputs.forEach(input => {
            // Previne zoom em iOS
            if (input.type === 'email' || input.type === 'tel' || input.type === 'url') {
                input.style.fontSize = '16px';
            }
            
            // Adiciona inputmode para melhor teclado virtual
            if (input.type === 'email') {
                input.setAttribute('inputmode', 'email');
            } else if (input.type === 'tel') {
                input.setAttribute('inputmode', 'tel');
            } else if (input.type === 'number') {
                input.setAttribute('inputmode', 'numeric');
            }
        });
    });
}

// ============================================
// DRAG AND DROP MOBILE FALLBACK
// ============================================

function initMobileDragDropFallback() {
    const uploadArea = document.querySelector('.upload-area');
    if (!uploadArea) return;
    
    // Para dispositivos touch, adiciona botÃ£o de upload mais prominente
    if (isTouchDevice()) {
        const fileInput = uploadArea.querySelector('input[type="file"]');
        if (fileInput) {
            const mobileUploadBtn = document.createElement('button');
            mobileUploadBtn.type = 'button';
            mobileUploadBtn.className = 'btn-modern btn-primary-modern mobile-upload-btn';
            mobileUploadBtn.innerHTML = '<i class="bi bi-plus-circle"></i> Selecionar Arquivos';
            
            mobileUploadBtn.onclick = () => fileInput.click();
            
            // Insere o botÃ£o na Ã¡rea de upload
            const uploadText = uploadArea.querySelector('p');
            if (uploadText) {
                uploadText.parentNode.insertBefore(mobileUploadBtn, uploadText.nextSibling);
            }
        }
    }
}

// ============================================
// INITIALIZATION WITH ERROR HANDLING
// ============================================

try {
    document.addEventListener('DOMContentLoaded', function() {
        // Inicializa todas as funcionalidades mobile
        initMobileFeatures();
        initMobileKeyboardNavigation();
        initMobileAccessibility();
        fixMobileViewportHeight();
        optimizeFormsForMobile();
        initMobileDragDropFallback();
        
        console.log('âœ… Mobile optimizations loaded successfully');
    });
} catch (error) {
    console.error('âŒ Error initializing mobile features:', error);
}

// ============================================
// GLOBAL MOBILE UTILITIES
// ============================================

// Exporta funÃ§Ãµes Ãºteis para uso global
window.mobileUtils = {
    isTouchDevice,
    isIOS,
    isAndroid,
    getOrientation,
    toggleMobileSidebar,
    openMobileSidebar,
    closeMobileSidebar,
    makeModalResponsive,
    throttle,
    debounce
};


