document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('adminSidebar');
    const mainContent = document.getElementById('mainContent');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarOverlay = document.getElementById('sidebarOverlay');

    function toggleSidebar() {
        const isDesktop = window.innerWidth > 991;

        if (isDesktop) {
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
        } else {
            sidebar.classList.toggle('active');
            sidebarOverlay.classList.toggle('active');
        }
    }

    sidebarToggle?.addEventListener('click', toggleSidebar);
    sidebarOverlay?.addEventListener('click', toggleSidebar);
});
