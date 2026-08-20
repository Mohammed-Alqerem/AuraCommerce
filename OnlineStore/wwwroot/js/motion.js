import { gsap } from "https://cdn.jsdelivr.net/npm/gsap@3.13.0/+esm";
import { ScrollTrigger } from "https://cdn.jsdelivr.net/npm/gsap@3.13.0/ScrollTrigger/+esm";

const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

if (!reduceMotion.matches) {
    gsap.registerPlugin(ScrollTrigger);

    const animate = (targets, vars) => {
        const elements = gsap.utils.toArray(targets).filter((element) => !element.dataset.motionReady);
        elements.forEach((element) => { element.dataset.motionReady = "true"; });
        if (elements.length) {
            gsap.from(elements, vars);
        }
    };

    const context = gsap.context(() => {
        const pageLead = document.querySelector(".hero-copy, .page-header > .container, .auth-card, .product-detail");
        const navItems = document.querySelectorAll(".site-header .navbar-brand, .site-header .nav-item, .site-header .nav-search, .site-header .nav-actions > *");

        gsap.timeline({ defaults: { ease: "power3.out" } })
            .from(navItems, { autoAlpha: 0, y: -14, duration: 0.42, stagger: 0.035 })
            .from(pageLead, { autoAlpha: 0, y: 28, duration: 0.7 }, "-=0.18");

        const heroMedia = document.querySelector(".hero-media");
        if (heroMedia) {
            gsap.from(heroMedia, { autoAlpha: 0, scale: 0.94, y: 28, duration: 0.9, ease: "power3.out", delay: 0.14 });
            gsap.to(heroMedia, {
                yPercent: -5,
                ease: "none",
                scrollTrigger: { trigger: ".hero", start: "top top", end: "bottom top", scrub: 0.7 }
            });
        }

        gsap.utils.toArray(".page-section").forEach((section) => {
            const heading = section.querySelector(".toolbar, .admin-subnav");
            if (heading && !heading.dataset.motionReady) {
                heading.dataset.motionReady = "true";
                gsap.from(heading, {
                    autoAlpha: 0, y: 20, duration: 0.55, ease: "power3.out",
                    scrollTrigger: { trigger: section, start: "top 84%", once: true }
                });
            }
        });

        animate(".product-card", {
            autoAlpha: 0, y: 30, duration: 0.58, ease: "power3.out", stagger: 0.07,
            scrollTrigger: { trigger: ".product-grid", start: "top 84%", once: true }
        });
        animate(".stats-grid .stat-card", {
            autoAlpha: 0, y: 24, scale: 0.97, duration: 0.52, ease: "back.out(1.3)", stagger: 0.075,
            scrollTrigger: { trigger: ".stats-grid", start: "top 84%", once: true }
        });
        animate(".panel:not(.product-card), .inventory-row, .order-card, .cart-item", {
            autoAlpha: 0, y: 22, duration: 0.52, ease: "power3.out", stagger: 0.055,
            scrollTrigger: { trigger: "main", start: "top 82%", once: true }
        });

        gsap.utils.toArray(".btn, .icon-button, .filter-chip, .admin-subnav-link, .category-pill").forEach((control) => {
            if (control.matches("[disabled]")) return;
            control.addEventListener("pointerenter", () => gsap.to(control, { y: -2, duration: 0.22, ease: "power2.out", overwrite: "auto" }));
            control.addEventListener("pointerleave", () => gsap.to(control, { y: 0, duration: 0.3, ease: "power2.out", overwrite: "auto" }));
        });

        const toast = document.querySelector(".store-toast");
        if (toast) {
            gsap.fromTo(toast, { autoAlpha: 0, y: 20, scale: 0.96 }, { autoAlpha: 1, y: 0, scale: 1, duration: 0.5, ease: "back.out(1.4)", delay: 0.3 });
        }

        gsap.from(".site-footer > .container > div", {
            autoAlpha: 0, y: 18, duration: 0.5, stagger: 0.08, ease: "power3.out",
            scrollTrigger: { trigger: ".site-footer", start: "top 90%", once: true }
        });

        ScrollTrigger.refresh();
    });

    window.addEventListener("beforeunload", () => context.revert(), { once: true });
}
