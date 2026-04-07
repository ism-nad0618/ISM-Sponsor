// placeholder javascript
function closeModal(){
    document.getElementById('modalContainer').innerHTML='';
}

// Mobile Navigation Toggle
document.addEventListener('DOMContentLoaded', function() {
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const sidebar = document.getElementById('sidebar');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    const sidebarClose = document.getElementById('sidebarClose');
    
    function openSidebar() {
        sidebar.classList.add('sidebar-open');
        sidebarOverlay.classList.add('active');
        mobileMenuToggle.setAttribute('aria-expanded', 'true');
        sidebar.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden'; // Prevent body scroll when menu is open
        
        // Focus first link in sidebar for accessibility
        const firstLink = sidebar.querySelector('a');
        if (firstLink) {
            setTimeout(() => firstLink.focus(), 100);
        }
    }
    
    function closeSidebar() {
        sidebar.classList.remove('sidebar-open');
        sidebarOverlay.classList.remove('active');
        mobileMenuToggle.setAttribute('aria-expanded', 'false');
        sidebar.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
        
        // Return focus to toggle button for accessibility
        mobileMenuToggle.focus();
    }
    
    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', function() {
            if (sidebar.classList.contains('sidebar-open')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        });
    }
    
    if (sidebarClose) {
        sidebarClose.addEventListener('click', closeSidebar);
    }
    
    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', closeSidebar);
    }
    
    // Close sidebar when navigation link is clicked (mobile)
    const navLinks = sidebar.querySelectorAll('a');
    navLinks.forEach(link => {
        link.addEventListener('click', function() {
            if (window.innerWidth < 980) {
                closeSidebar();
            }
        });
    });
    
    // Close sidebar on escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && sidebar.classList.contains('sidebar-open')) {
            closeSidebar();
            mobileMenuToggle.focus();
        }
    });
});
