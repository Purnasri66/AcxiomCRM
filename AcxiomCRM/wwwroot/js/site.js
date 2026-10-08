// AcxiomCRM Global JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // 1. Theme Toggle with localStorage persistence
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = document.getElementById('themeIcon');
    const html = document.documentElement;

    function applyTheme(theme) {
        html.setAttribute('data-bs-theme', theme);
        localStorage.setItem('acxiom_theme', theme);
        if (themeIcon) {
            themeIcon.className = theme === 'dark' ? 'bi bi-moon-stars-fill' : 'bi bi-sun-fill';
        }
    }

    const savedTheme = localStorage.getItem('acxiom_theme') || 'light';
    applyTheme(savedTheme);

    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            const currentTheme = html.getAttribute('data-bs-theme');
            applyTheme(currentTheme === 'dark' ? 'light' : 'dark');
        });
    }

    // 2. Mobile Sidebar Toggle
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });
    }

    // 3. Global Quick Search (Ctrl+K)
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            const searchModal = new bootstrap.Modal(document.getElementById('searchModal'));
            searchModal.show();
        }
    });

    const searchInput = document.getElementById('quickSearchInput');
    const searchResults = document.getElementById('quickSearchResults');
    let searchTimeout = null;

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            const query = this.value.trim();
            if (query.length < 2) {
                searchResults.innerHTML = '<div class="text-center text-muted py-4 small">Type at least 2 characters to search...</div>';
                return;
            }

            searchTimeout = setTimeout(function () {
                fetch('/Search/QuickSearch?q=' + encodeURIComponent(query))
                    .then(r => r.json())
                    .then(data => {
                        let html = '';
                        const hasResults = (data.customers && data.customers.length > 0) ||
                                           (data.leads && data.leads.length > 0) ||
                                           (data.opportunities && data.opportunities.length > 0);

                        if (!hasResults) {
                            searchResults.innerHTML = '<div class="text-center text-muted py-4 small">No results found for "' + query + '".</div>';
                            return;
                        }

                        if (data.customers && data.customers.length > 0) {
                            html += '<div class="fw-bold small text-muted text-uppercase mb-2">Customers</div>';
                            data.customers.forEach(item => {
                                html += `<a href="${item.url}" class="d-flex align-items-center justify-content-between p-2 mb-1 rounded text-decoration-none text-body bg-body-hover">
                                    <div><i class="bi bi-people text-primary me-2"></i><strong>${item.title}</strong><div class="text-muted smaller">${item.subtitle}</div></div>
                                    <span class="badge bg-primary-subtle text-primary-emphasis">Customer</span>
                                </a>`;
                            });
                        }

                        if (data.leads && data.leads.length > 0) {
                            html += '<div class="fw-bold small text-muted text-uppercase mt-3 mb-2">Leads</div>';
                            data.leads.forEach(item => {
                                html += `<a href="${item.url}" class="d-flex align-items-center justify-content-between p-2 mb-1 rounded text-decoration-none text-body bg-body-hover">
                                    <div><i class="bi bi-funnel text-warning me-2"></i><strong>${item.title}</strong><div class="text-muted smaller">${item.subtitle}</div></div>
                                    <span class="badge bg-warning-subtle text-warning-emphasis">Lead</span>
                                </a>`;
                            });
                        }

                        if (data.opportunities && data.opportunities.length > 0) {
                            html += '<div class="fw-bold small text-muted text-uppercase mt-3 mb-2">Opportunities</div>';
                            data.opportunities.forEach(item => {
                                html += `<a href="${item.url}" class="d-flex align-items-center justify-content-between p-2 mb-1 rounded text-decoration-none text-body bg-body-hover">
                                    <div><i class="bi bi-briefcase text-success me-2"></i><strong>${item.title}</strong><div class="text-muted smaller">${item.subtitle}</div></div>
                                    <span class="badge bg-success-subtle text-success-emphasis">Opportunity</span>
                                </a>`;
                            });
                        }

                        searchResults.innerHTML = html;
                    })
                    .catch(() => {
                        searchResults.innerHTML = '<div class="text-danger small py-3 text-center">Failed to execute search.</div>';
                    });
            }, 300);
        });
    }
});
