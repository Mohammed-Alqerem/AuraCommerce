import * as THREE from "https://cdn.jsdelivr.net/npm/three@0.181.0/build/three.module.js";

const container = document.querySelector("#commerce-scene");
const hero = container?.closest(".hero");
const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
const lowPowerDevice = navigator.deviceMemory && navigator.deviceMemory < 2;

if (container && !reducedMotion && !lowPowerDevice) {
    try {
        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(42, 1, 0.1, 100);
        camera.position.set(0, 0, 8);

        const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true, powerPreference: "low-power" });
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5));
        renderer.setClearColor(0x000000, 0);
        container.appendChild(renderer.domElement);

        const cluster = new THREE.Group();
        cluster.position.set(2.65, 0.1, 0);
        scene.add(cluster);

        const orbGeometry = new THREE.IcosahedronGeometry(1.7, 2);
        const orbMaterial = new THREE.MeshBasicMaterial({ color: 0x087ed2, wireframe: true, transparent: true, opacity: 0.2 });
        const orb = new THREE.Mesh(orbGeometry, orbMaterial);
        cluster.add(orb);

        const haloGeometry = new THREE.TorusGeometry(2.15, 0.018, 8, 90);
        const haloMaterial = new THREE.MeshBasicMaterial({ color: 0x69b7ff, transparent: true, opacity: 0.38 });
        const halo = new THREE.Mesh(haloGeometry, haloMaterial);
        halo.rotation.set(0.85, 0.2, -0.45);
        cluster.add(halo);

        const particleCount = 120;
        const positions = new Float32Array(particleCount * 3);
        for (let index = 0; index < particleCount; index += 1) {
            const offset = index * 3;
            positions[offset] = (Math.random() - 0.5) * 11;
            positions[offset + 1] = (Math.random() - 0.5) * 5.6;
            positions[offset + 2] = (Math.random() - 0.5) * 3;
        }

        const particlesGeometry = new THREE.BufferGeometry();
        particlesGeometry.setAttribute("position", new THREE.BufferAttribute(positions, 3));
        const particlesMaterial = new THREE.PointsMaterial({ color: 0x4ba8ff, size: 0.035, transparent: true, opacity: 0.62, sizeAttenuation: true });
        const particles = new THREE.Points(particlesGeometry, particlesMaterial);
        scene.add(particles);

        let pointerX = 0;
        let pointerY = 0;
        let frameId = 0;
        let visible = true;

        function applyTheme() {
            const isDark = document.documentElement.dataset.theme === "dark";
            orbMaterial.color.setHex(isDark ? 0x79beff : 0x087ed2);
            haloMaterial.color.setHex(isDark ? 0xaad7ff : 0x53adf4);
            particlesMaterial.color.setHex(isDark ? 0x72b9ff : 0x238de5);
        }

        function resize() {
            const { width, height } = container.getBoundingClientRect();
            if (!width || !height) return;
            renderer.setSize(width, height, false);
            camera.aspect = width / height;
            camera.updateProjectionMatrix();
        }

        function render(time) {
            frameId = window.requestAnimationFrame(render);
            if (!visible) return;

            const elapsed = time * 0.001;
            cluster.rotation.y += (pointerX * 0.35 - cluster.rotation.y) * 0.025;
            cluster.rotation.x += (-pointerY * 0.18 - cluster.rotation.x) * 0.025;
            orb.rotation.z = elapsed * 0.12;
            halo.rotation.z = -0.45 + elapsed * 0.08;
            particles.rotation.y = elapsed * 0.018;
            particles.position.y = Math.sin(elapsed * 0.55) * 0.12;
            renderer.render(scene, camera);
        }

        const resizeObserver = new ResizeObserver(resize);
        resizeObserver.observe(container);
        hero?.addEventListener("pointermove", (event) => {
            const rect = container.getBoundingClientRect();
            pointerX = ((event.clientX - rect.left) / rect.width - 0.5) * 2;
            pointerY = ((event.clientY - rect.top) / rect.height - 0.5) * 2;
        });
        document.addEventListener("visibilitychange", () => { visible = !document.hidden; });
        new MutationObserver(applyTheme).observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });

        applyTheme();
        resize();
        frameId = window.requestAnimationFrame(render);

        window.addEventListener("beforeunload", () => {
            window.cancelAnimationFrame(frameId);
            resizeObserver.disconnect();
            orbGeometry.dispose();
            orbMaterial.dispose();
            haloGeometry.dispose();
            haloMaterial.dispose();
            particlesGeometry.dispose();
            particlesMaterial.dispose();
            renderer.dispose();
        }, { once: true });
    } catch {
        container.remove();
    }
}
