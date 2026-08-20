(function () {
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const progress = document.querySelector(".scroll-progress");
    const header = document.querySelector(".site-header");
    const root = document.documentElement;
    const translations = {
        en: {
            "brand": "Aura Commerce",
            "nav.home": "Home",
            "nav.products": "Products",
            "nav.orders": "Orders",
            "nav.admin": "Admin",
            "nav.adminPortal": "Admin Portal",
            "nav.cart": "Cart",
            "nav.profile": "Profile",
            "nav.login": "Login",
            "nav.logout": "Logout",
            "search.placeholder": "Search products",
            "footer.summary": "Premium everyday products powered by a clean MVC store platform.",
            "footer.shop": "Shop",
            "footer.allProducts": "All products",
            "footer.cart": "Cart",
            "footer.checkout": "Checkout",
            "footer.account": "Account",
            "footer.register": "Register",
            "footer.operations": "Operations",
            "footer.users": "Users",
            "login.mediaEyebrow": "Secure Storefront",
            "login.mediaTitle": "Manage carts, orders, and products from one modern MVC platform.",
            "login.featureSecure": "Secure session",
            "login.featureLanguage": "Arabic / English",
            "login.eyebrow": "Account Access",
            "login.title": "Welcome Back",
            "login.subtitle": "Sign in with a seeded user, for example mohammed@gmail.com / 123456.",
            "login.email": "Email",
            "login.emailPlaceholder": "name@example.com",
            "login.password": "Password",
            "login.passwordPlaceholder": "Password",
            "login.remember": "Remember me",
            "login.forgot": "Forgot password?",
            "login.submit": "Login",
            "login.noAccount": "No account?",
            "login.register": "Register",
            "login.demoTitle": "Demo accounts",
            "login.demoCustomer": "Customer account",
            "login.demoBuyer": "Buyer account",
            "login.demoAdmin": "Admin account - dashboard access",
            "register.mediaEyebrow": "Create your store account",
            "register.mediaTitle": "Save your details, track orders, and checkout faster every time.",
            "register.featureOrders": "Order tracking",
            "register.featureCart": "Personal cart",
            "register.eyebrow": "New Customer",
            "register.title": "Create Account",
            "register.subtitle": "Register once and use the same account for cart, checkout, profile, and orders.",
            "register.name": "Name",
            "register.namePlaceholder": "Full name",
            "register.phone": "Phone",
            "register.phonePlaceholder": "Phone number",
            "register.address": "Address",
            "register.addressPlaceholder": "Delivery address",
            "register.submit": "Create Account"
        },
        ar: {
            "brand": "أورا كومرس",
            "nav.home": "الرئيسية",
            "nav.products": "المنتجات",
            "nav.orders": "الطلبات",
            "nav.admin": "الإدارة",
            "nav.adminPortal": "بوابة الإدارة",
            "nav.cart": "السلة",
            "nav.profile": "الملف الشخصي",
            "nav.login": "تسجيل الدخول",
            "nav.logout": "تسجيل الخروج",
            "search.placeholder": "ابحث عن المنتجات",
            "footer.summary": "منتجات يومية مميزة ضمن منصة MVC عصرية ومنظمة.",
            "footer.shop": "المتجر",
            "footer.allProducts": "كل المنتجات",
            "footer.cart": "السلة",
            "footer.checkout": "الدفع",
            "footer.account": "الحساب",
            "footer.register": "إنشاء حساب",
            "footer.operations": "العمليات",
            "footer.users": "المستخدمون",
            "login.mediaEyebrow": "واجهة متجر آمنة",
            "login.mediaTitle": "إدارة السلة والطلبات والمنتجات من منصة MVC حديثة واحدة.",
            "login.featureSecure": "جلسة آمنة",
            "login.featureLanguage": "العربية / الإنجليزية",
            "login.eyebrow": "الوصول للحساب",
            "login.title": "أهلاً بعودتك",
            "login.subtitle": "سجل الدخول بمستخدم موجود مثل mohammed@gmail.com / 123456.",
            "login.email": "البريد الإلكتروني",
            "login.emailPlaceholder": "name@example.com",
            "login.password": "كلمة المرور",
            "login.passwordPlaceholder": "كلمة المرور",
            "login.remember": "تذكرني",
            "login.forgot": "نسيت كلمة المرور؟",
            "login.submit": "تسجيل الدخول",
            "login.noAccount": "ليس لديك حساب؟",
            "login.register": "إنشاء حساب",
            "login.demoTitle": "حسابات جاهزة",
            "login.demoCustomer": "حساب عميل",
            "login.demoBuyer": "حساب مشتري",
            "login.demoAdmin": "حساب مدير - دخول لوحة التحكم",
            "register.mediaEyebrow": "أنشئ حسابك في المتجر",
            "register.mediaTitle": "احفظ بياناتك وتابع طلباتك وأنهِ الشراء بشكل أسرع في كل مرة.",
            "register.featureOrders": "تتبع الطلبات",
            "register.featureCart": "سلة شخصية",
            "register.eyebrow": "عميل جديد",
            "register.title": "إنشاء حساب",
            "register.subtitle": "سجل مرة واحدة واستخدم نفس الحساب للسلة والدفع والملف الشخصي والطلبات.",
            "register.name": "الاسم",
            "register.namePlaceholder": "الاسم الكامل",
            "register.phone": "الهاتف",
            "register.phonePlaceholder": "رقم الهاتف",
            "register.address": "العنوان",
            "register.addressPlaceholder": "عنوان التوصيل",
            "register.submit": "إنشاء حساب"
        }
    };
    const textTranslations = {
        "Home": "الرئيسية",
        "Products": "المنتجات",
        "Orders": "الطلبات",
        "Admin": "الإدارة",
        "Admin Portal": "بوابة الإدارة",
        "Login": "تسجيل الدخول",
        "Register": "إنشاء حساب",
        "Profile": "الملف الشخصي",
        "Cart": "السلة",
        "Checkout": "الدفع",
        "All products": "كل المنتجات",
        "All Products": "كل المنتجات",
        "Shop": "المتجر",
        "Account": "الحساب",
        "Operations": "العمليات",
        "Users": "المستخدمون",
        "Everything you need, all in one store.": "كل ما تحتاجه في متجر واحد.",
        "Modern MVC Storefront": "واجهة متجر MVC حديثة",
        "Shop Now": "تسوق الآن",
        "Explore Categories": "استكشف الفئات",
        "Browse": "تصفح",
        "Shop by Category": "تسوق حسب الفئة",
        "View Catalog": "عرض الكتالوج",
        "Featured": "منتجات مميزة",
        "Selected Products": "منتجات مختارة",
        "View All": "عرض الكل",
        "Catalog": "الكتالوج",
        "Search": "بحث",
        "All": "الكل",
        "No products found": "لا توجد منتجات",
        "Try another search or category.": "جرب بحثاً أو فئة أخرى.",
        "Back to products": "الرجوع إلى المنتجات",
        "Add to Cart": "أضف إلى السلة",
        "Add Review": "إضافة تقييم",
        "Post Review": "نشر التقييم",
        "Customer Reviews": "تقييمات العملاء",
        "Your Cart": "سلتك",
        "Your cart is empty": "سلتك فارغة",
        "Browse Products": "تصفح المنتجات",
        "Product": "المنتج",
        "Qty": "الكمية",
        "Total": "المجموع",
        "Update": "تحديث",
        "Order Summary": "ملخص الطلب",
        "Subtotal": "المجموع الفرعي",
        "Shipping": "الشحن",
        "Tax": "الضريبة",
        "Complete Your Order": "أكمل طلبك",
        "Shipping Details": "بيانات الشحن",
        "FullName": "الاسم الكامل",
        "Email": "البريد الإلكتروني",
        "Phone": "الهاتف",
        "Address": "العنوان",
        "Review Order": "مراجعة الطلب",
        "Place Order": "تأكيد الطلب",
        "Order Confirmed": "تم تأكيد الطلب",
        "View Order": "عرض الطلب",
        "Continue Shopping": "متابعة التسوق",
        "My Orders": "طلباتي",
        "Order": "الطلب",
        "Date": "التاريخ",
        "Items": "العناصر",
        "Status": "الحالة",
        "Details": "التفاصيل",
        "Order Details": "تفاصيل الطلب",
        "Back to orders": "الرجوع إلى الطلبات",
        "Unit": "سعر الوحدة",
        "My Profile": "ملفي الشخصي",
        "Save Profile": "حفظ الملف",
        "Logout": "تسجيل الخروج",
        "Reviews": "التقييمات",
        "Member Since": "عضو منذ",
        "Dashboard": "لوحة التحكم",
        "Revenue": "الإيرادات",
        "Recent Orders": "أحدث الطلبات",
        "Manage": "إدارة",
        "Inventory Watch": "مراقبة المخزون",
        "Add Product": "إضافة منتج",
        "Edit Product": "تعديل المنتج",
        "Category": "الفئة",
        "Price": "السعر",
        "Stock": "المخزون",
        "Edit": "تعديل",
        "Delete": "حذف",
        "Save Product": "حفظ المنتج",
        "Cancel": "إلغاء",
        "Customer": "العميل",
        "Save": "حفظ",
        "Name": "الاسم",
        "Password": "كلمة المرور",
        "Create Account": "إنشاء حساب",
        "New Customer": "عميل جديد",
        "Welcome Back": "أهلاً بعودتك",
        "Account Access": "الوصول للحساب",
        "No account?": "ليس لديك حساب؟",
        "Remember me": "تذكرني",
        "Forgot password?": "نسيت كلمة المرور؟",
        "Electronics": "إلكترونيات",
        "Clothing": "ملابس",
        "Shoes": "أحذية",
        "Accessories": "إكسسوارات",
        "Electronic devices and accessories": "أجهزة إلكترونية وإكسسوارات",
        "Men and women clothing": "ملابس رجالية ونسائية",
        "Sports and casual shoes": "أحذية رياضية وكاجوال",
        "Useful accessories and gadgets": "إكسسوارات وأدوات مفيدة",
        "Wireless Mouse": "فأرة لاسلكية",
        "Mechanical Keyboard": "لوحة مفاتيح ميكانيكية",
        "USB-C Charger": "شاحن USB-C",
        "Classic T-Shirt": "تيشيرت كلاسيكي",
        "Hoodie": "هودي",
        "Running Shoes": "حذاء جري",
        "Casual Sneakers": "حذاء كاجوال",
        "Smart Watch": "ساعة ذكية",
        "Backpack": "حقيبة ظهر",
        "Phone Stand": "حامل هاتف",
        "Comfortable wireless mouse for everyday use": "فأرة لاسلكية مريحة للاستخدام اليومي",
        "RGB mechanical keyboard for gaming and work": "لوحة مفاتيح ميكانيكية بإضاءة RGB للألعاب والعمل",
        "Fast charging USB-C wall charger": "شاحن حائط USB-C سريع",
        "Comfortable cotton T-Shirt": "تيشيرت قطني مريح",
        "Warm casual hoodie for everyday wear": "هودي دافئ ومريح للاستخدام اليومي",
        "Lightweight running shoes for sports": "حذاء جري خفيف للرياضة",
        "Modern casual sneakers": "حذاء كاجوال عصري",
        "Smart watch with fitness tracking": "ساعة ذكية مع تتبع اللياقة",
        "Water resistant backpack for daily use": "حقيبة ظهر مقاومة للماء للاستخدام اليومي",
        "Adjustable desk phone stand": "حامل هاتف مكتبي قابل للتعديل",
        "Very good mouse and comfortable to use": "فأرة ممتازة ومريحة في الاستخدام",
        "Good smart watch with useful features": "ساعة ذكية جيدة بميزات مفيدة",
        "Very comfortable shoes": "أحذية مريحة جداً",
        "Good quality and comfortable": "جودة جيدة ومريحة",
        "Excellent keyboard for gaming": "لوحة مفاتيح ممتازة للألعاب",
        "Explore our complete collection of products.": "استكشف مجموعتنا الكاملة من المنتجات.",
        "Search catalog": "ابحث في الكتالوج",
        "Share your feedback": "شارك رأيك",
        "Everything You Need,": "كل ما تحتاجه،",
        "All in One Store": "في متجر واحد",
        "Discover a curated selection of premium electronics, fashion, and lifestyle essentials designed to elevate your everyday. Shop our modern collection today.": "اكتشف مجموعة مختارة من الإلكترونيات والموضة ومستلزمات الحياة اليومية بجودة عالية. تسوق مجموعتنا الحديثة اليوم.",
        "Premium everyday products powered by a clean MVC store platform.": "منتجات يومية مميزة ضمن منصة MVC عصرية ومنظمة.",
        "Providing premium products for a modern lifestyle. Quality and design in every detail.": "نوفر منتجات مميزة لأسلوب حياة عصري، بجودة وتصميم في كل تفصيل.",
        "Customer Service": "خدمة العملاء",
        "Contact Us": "تواصل معنا",
        "Shipping Info": "معلومات الشحن",
        "Returns": "الإرجاع",
        "FAQ": "الأسئلة الشائعة",
        "Legal": "قانوني",
        "Privacy Policy": "سياسة الخصوصية",
        "Terms of Service": "شروط الخدمة",
        "Subscribe": "اشترك",
        "Get the latest updates on new products and upcoming sales.": "احصل على أحدث المنتجات والعروض القادمة.",
        "SUBSCRIBE": "اشترك",
        "Pending": "قيد الانتظار",
        "Processing": "قيد المعالجة",
        "Shipped": "تم الشحن",
        "Delivered": "تم التسليم",
        "Cancelled": "ملغي"
        ,"Store overview": "نظرة عامة على المتجر"
        ,"Operations workspace": "مساحة العمليات"
        ,"Monitor sales, fulfilment, and inventory from one focused workspace.": "راقب المبيعات والتنفيذ والمخزون من مساحة عمل واحدة."
        ,"Overview": "نظرة عامة"
        ,"Customers": "العملاء"
        ,"Catalog management": "إدارة الكتالوج"
        ,"Manage product information, pricing, and availability.": "أدر معلومات المنتجات والأسعار والتوفر بسهولة."
        ,"Fulfilment queue": "قائمة التنفيذ"
        ,"Update each order as it moves through fulfilment.": "حدّث كل طلب أثناء انتقاله خلال مراحل التنفيذ."
        ,"Customer directory": "دليل العملاء"
        ,"Review account information and purchase activity.": "راجع معلومات الحساب ونشاط المشتريات."
        ,"Inventory is healthy": "المخزون بحالة جيدة"
        ,"No products are at or below 10 units.": "لا توجد منتجات بمخزون أقل من أو يساوي 10 وحدات."
        ,"Out of stock": "غير متوفر حالياً"
        ,"Please": "يرجى"
        ,"sign in as a customer": "تسجيل الدخول كعميل"
        ,"to share a review.": "لمشاركة تقييمك."
        ,"Something went wrong": "حدث خطأ ما"
        ,"We could not complete that request. Please return to the storefront and try again.": "تعذر إكمال الطلب. يرجى العودة إلى المتجر والمحاولة مرة أخرى."
        ,"Return home": "العودة للرئيسية"
        ,"Your privacy matters": "خصوصيتك مهمة"
        ,"Information we store": "المعلومات التي نخزنها"
        ,"How we use it": "كيف نستخدمها"
    };
    const originalTextNodes = new Map();

    function applyTheme(theme) {
        root.dataset.theme = theme;
        localStorage.setItem("store-theme", theme);
        document.querySelectorAll(".js-theme-toggle .material-symbols-outlined").forEach((icon) => {
            icon.textContent = theme === "dark" ? "light_mode" : "dark_mode";
        });
        document.querySelectorAll(".js-theme-toggle").forEach((button) => {
            const label = theme === "dark" ? "Switch to light mode" : "Switch to dark mode";
            button.setAttribute("aria-label", label);
            button.setAttribute("title", label);
        });
    }

    function applyLanguage(language) {
        const dictionary = translations[language] || translations.en;
        root.lang = language;
        root.dir = language === "ar" ? "rtl" : "ltr";
        localStorage.setItem("store-language", language);

        document.querySelectorAll("[data-i18n]").forEach((element) => {
            const key = element.dataset.i18n;
            if (key && dictionary[key]) {
                element.textContent = dictionary[key];
            }
        });

        document.querySelectorAll("[data-i18n-placeholder]").forEach((element) => {
            const key = element.dataset.i18nPlaceholder;
            if (key && dictionary[key]) {
                element.setAttribute("placeholder", dictionary[key]);
            }
        });

        document.querySelectorAll("[data-cart-count]").forEach((element) => {
            const count = element.dataset.cartCount || "0";
            const label = language === "ar" ? `السلة، ${count} منتجات` : `Cart, ${count} items`;
            element.setAttribute("aria-label", label);
        });

        document.querySelectorAll(".js-language-toggle").forEach((button) => {
            button.textContent = language === "ar" ? "EN" : "AR";
            button.setAttribute("aria-label", language === "ar" ? "Switch to English" : "التبديل إلى العربية");
        });

        translateStaticText(language);
    }

    function translateStaticText(language) {
        const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, {
            acceptNode(node) {
                if (!node.parentElement || node.parentElement.closest("script, style, textarea, option, [data-i18n]")) {
                    return NodeFilter.FILTER_REJECT;
                }

                return node.nodeValue.trim() ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_REJECT;
            }
        });

        const nodes = [];
        while (walker.nextNode()) {
            nodes.push(walker.currentNode);
        }

        nodes.forEach((node) => {
            if (!originalTextNodes.has(node)) {
                originalTextNodes.set(node, node.nodeValue);
            }

            const original = originalTextNodes.get(node);
            const trimmed = original.trim();
            const leading = original.match(/^\s*/)?.[0] ?? "";
            const trailing = original.match(/\s*$/)?.[0] ?? "";

            if (language === "ar" && textTranslations[trimmed]) {
                node.nodeValue = `${leading}${textTranslations[trimmed]}${trailing}`;
            } else {
                node.nodeValue = original;
            }
        });
    }

    applyTheme(localStorage.getItem("store-theme") || "light");
    applyLanguage(localStorage.getItem("store-language") || "en");

    document.querySelectorAll(".js-theme-toggle").forEach((button) => {
        button.addEventListener("click", () => {
            applyTheme(root.dataset.theme === "dark" ? "light" : "dark");
        });
    });

    document.querySelectorAll(".js-language-toggle").forEach((button) => {
        button.addEventListener("click", () => {
            applyLanguage(root.lang === "ar" ? "en" : "ar");
        });
    });

    function updateProgress() {
        if (progress) {
            const scrollable = document.documentElement.scrollHeight - window.innerHeight;
            const ratio = scrollable > 0 ? window.scrollY / scrollable : 0;
            progress.style.transform = `scaleX(${Math.min(Math.max(ratio, 0), 1)})`;
        }

        if (header) {
            header.classList.toggle("is-scrolled", window.scrollY > 8);
        }
    }

    window.addEventListener("scroll", updateProgress, { passive: true });
    updateProgress();

    if (!reducedMotion && "IntersectionObserver" in window) {
        const revealTargets = document.querySelectorAll(".product-card, .panel, .stat-card, .page-header .container, .hero-copy, .hero-media");
        revealTargets.forEach((element, index) => {
            element.classList.add("js-reveal");
            element.style.transitionDelay = `${Math.min(index % 8, 6) * 45}ms`;
        });

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.14, rootMargin: "0px 0px -40px 0px" });

        revealTargets.forEach((element) => observer.observe(element));
    }

    if (!reducedMotion) {
        document.querySelectorAll(".stat-card strong").forEach((element) => {
            const original = element.textContent.trim();
            const numeric = Number(original.replace(/[^0-9.]/g, ""));
            if (!Number.isFinite(numeric)) {
                return;
            }

            const hasCurrency = original.includes("$");
            const hasDecimal = original.includes(".");
            const duration = 850;
            const start = performance.now();

            function tick(now) {
                const elapsed = Math.min((now - start) / duration, 1);
                const eased = 1 - Math.pow(1 - elapsed, 3);
                const value = numeric * eased;
                element.textContent = hasCurrency
                    ? `$${value.toFixed(hasDecimal ? 2 : 0)}`
                    : Math.round(value).toString();

                if (elapsed < 1) {
                    requestAnimationFrame(tick);
                } else {
                    element.textContent = original;
                }
            }

            requestAnimationFrame(tick);
        });
    }

    document.querySelectorAll('form[action*="/Cart/Add"], form[action$="Cart/Add"]').forEach((form) => {
        form.addEventListener("submit", () => {
            const button = form.querySelector("button");
            if (!button) {
                return;
            }

            button.classList.add("is-added");
            const icon = button.querySelector(".material-symbols-outlined");
            if (icon) {
                icon.textContent = "check";
            }
        });
    });

    document.querySelectorAll(".js-fill-login").forEach((button) => {
        button.addEventListener("click", () => {
            const email = document.querySelector('input[name="Email"]');
            const password = document.querySelector('input[name="Password"]');

            if (email) {
                email.value = button.dataset.email || "";
                email.dispatchEvent(new Event("input", { bubbles: true }));
            }

            if (password) {
                password.value = button.dataset.password || "";
                password.dispatchEvent(new Event("input", { bubbles: true }));
            }
        });
    });
})();
