(function () {
    'use strict';

    function redirectIfAuthenticated() {
        const currentPath = window.location.pathname;
        const isAuthPage = currentPath === '/login' || currentPath === '/register' || currentPath === '/';

        if (isAuthPage && document.cookie.includes('gh_access_token=')) {
            window.location.href = '/pedidos';
        }
    }

    function initScrollNav() {
        const nav = document.querySelector('.topnav');
        if (!nav) return;
        const onScroll = () => {
            nav.classList.toggle('topnav--scrolled', window.scrollY > 20);
        };
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    }
    function initEntranceAnimations() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });

        document.querySelectorAll('.animate-on-scroll').forEach((el, i) => {
            el.style.transitionDelay = `${i * 60}ms`;
            observer.observe(el);
        });
    }

    function init() {
        initScrollNav();
        initEntranceAnimations();
        redirectIfAuthenticated();
    }

    if (typeof Blazor !== 'undefined') {
        Blazor.addEventListener('enhancedload', init);
    }
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function setCookie(name, value, expiresUtc) {
        const expires = new Date(expiresUtc).toUTCString();
        document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires}; path=/; Secure; SameSite=Strict`;
    }

    function getCookie(name) {
        const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        return match ? decodeURIComponent(match[2]) : null;
    }

    function deleteCookie(name) {
        document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; Secure; SameSite=Strict`;
    }

    window.GoodHamburger = {
        scrollToTop: () => window.scrollTo({ top: 0, behavior: 'smooth' }),
        copyText: (text) => navigator.clipboard?.writeText(text),
        setCookie,
        getCookie,
        deleteCookie
    };
})();
