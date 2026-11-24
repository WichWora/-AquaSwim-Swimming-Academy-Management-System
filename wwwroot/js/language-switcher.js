// Create a file: wwwroot/js/language-switcher.js
class LanguageSwitcher {
    constructor() {
        this.currentLang = localStorage.getItem('preferredLanguage') || 'en';
        this.translations = {
            en: {
                // Home Page
                "Welcome to AquaSwimming Academy": "Welcome to AquaSwimming Academy",
                "Professional swimming classes for all ages and skill levels. Master the waters with confidence.":
                    "Professional swimming classes for all ages and skill levels. Master the waters with confidence.",
                "Browse Classes": "Browse Classes",
                "Join Now": "Join Now",
                "Why Choose AquaSwimming?": "Why Choose AquaSwimming?",
                "Discover the perfect swimming program tailored to your needs and goals":
                    "Discover the perfect swimming program tailored to your needs and goals",
                "Expert Coaches": "Expert Coaches",
                "Learn from certified swimming instructors with years of competitive and teaching experience.":
                    "Learn from certified swimming instructors with years of competitive and teaching experience.",
                "Modern Facilities": "Modern Facilities",
                "Train in our Olympic-sized heated pools with state-of-the-art equipment and safety systems.":
                    "Train in our Olympic-sized heated pools with state-of-the-art equipment and safety systems.",
                "Progressive Levels": "Progressive Levels",
                "From beginner to competitive levels, we have a structured program for every swimmer.":
                    "From beginner to competitive levels, we have a structured program for every swimmer.",
                "Get Started Today": "Get Started Today",
                "Choose your path to swimming excellence": "Choose your path to swimming excellence",
                "Browse Classes": "Browse Classes",
                
                "View our wide range of swimming classes from beginner to advanced levels.":
                    "View our wide range of swimming classes from beginner to advanced levels.",
                "View Classes": "View Classes",
                "View Schedule":"View Schedule",
                "Check our class schedules and find a time that works for you.":
                    "Check our class schedules and find a time that works for you.",
                "View Calendar": "View Calendar",
                "Register Now":"Register Now",
                "Join our swimming academy and start your journey to becoming a better swimmer.":
                    "Join our swimming academy and start your journey to becoming a better swimmer.",
                "Login / Register": "Login / Register",
                "Quick Navigation": "Quick Navigation",
                "All Classes": "All Classes",
                "Weekly Schedule": "Weekly Schedule",
                "Monthly Calendar": "Monthly Calendar",
                "About Us": "About Us",
                "Contact": "Contact",

                 "About AquaSwimming Academy": "About AquaSwimming Academy",
                "Your journey to swimming excellence begins here": "Your journey to swimming excellence begins here",
                "Our Story": "Our Story",
                "Founded in 2010, AquaSwimming Academy began with a simple mission: to make swimming accessible, safe, and enjoyable for everyone. What started as a small community swimming program has grown into a premier aquatic education center.":
                    "Founded in 2010, AquaSwimming Academy began with a simple mission: to make swimming accessible, safe, and enjoyable for everyone. What started as a small community swimming program has grown into a premier aquatic education center.",
                "Over the past decade, we've taught thousands of students, from toddlers taking their first strokes to adults conquering their fear of water, and competitive swimmers honing their technique for national competitions.":
                    "Over the past decade, we've taught thousands of students, from toddlers taking their first strokes to adults conquering their fear of water, and competitive swimmers honing their technique for national competitions.",
                "Our academy is built on three core principles: Safety First, Progressive Learning, and Individual Attention.":
                    "Our academy is built on three core principles: Safety First, Progressive Learning, and Individual Attention.",
                "Our Facilities": "Our Facilities",
                "Two Olympic-sized heated pools": "Two Olympic-sized heated pools",
                "Temperature-controlled water year-round": "Temperature-controlled water year-round",
                "Advanced filtration and sanitation systems": "Advanced filtration and sanitation systems",
                "Separate beginner and advanced pools": "Separate beginner and advanced pools",
                "State-of-the-art safety equipment": "State-of-the-art safety equipment",
                "Spacious changing rooms and amenities": "Spacious changing rooms and amenities",
                "Our Team of Experts": "Our Team of Experts",
                "Certified Instructors": "Certified Instructors",
                "All our coaches are certified by national swimming associations with minimum 5 years of teaching experience.":
                    "All our coaches are certified by national swimming associations with minimum 5 years of teaching experience.",
                "Competitive Experience": "Competitive Experience",
                "Many of our coaches are former competitive swimmers with national and international competition experience.":
                    "Many of our coaches are former competitive swimmers with national and international competition experience.",
                "Safety First": "Safety First",
                "Every staff member is certified in CPR, First Aid, and lifeguarding to ensure maximum safety.":
                    "Every staff member is certified in CPR, First Aid, and lifeguarding to ensure maximum safety.",
                "Our Accreditations": "Our Accreditations",
                "Swim Australia": "Swim Australia",
                "Royal Life Saving": "Royal Life Saving",
                "5-Star Facility": "5-Star Facility",
                "ISO Certified": "ISO Certified",
                "Our Mission": "Our Mission",
                '"To provide a safe, supportive, and professional environment where individuals of all ages and abilities can develop swimming skills, build confidence, and foster a lifelong love for aquatic activities."':
                    '"To provide a safe, supportive, and professional environment where individuals of all ages and abilities can develop swimming skills, build confidence, and foster a lifelong love for aquatic activities."',
                "AquaSwimming Academy Team": "AquaSwimming Academy Team",
                "Contact Us": "Contact Us"
            },
            ms: {
                // Home Page
                "Welcome to AquaSwimming Academy": "Selamat Datang ke Akademi Renang AquaSwimming",
                "Professional swimming classes for all ages and skill levels. Master the waters with confidence.":
                    "Kelas renang profesional untuk semua peringkat umur dan kemahiran. Kuasai air dengan keyakinan.",
                "Browse Classes": "Lihat Kelas",
                "Join Now": "Sertai Sekarang",
                "Why Choose AquaSwimming?": "Kenapa Pilih AquaSwimming?",
                "Discover the perfect swimming program tailored to your needs and goals":
                    "Temui program renang yang sempurna disesuaikan dengan keperluan dan matlamat anda",
                "Expert Coaches": "Jurulatih Pakar",
                "Learn from certified swimming instructors with years of competitive and teaching experience.":
                    "Belajar daripada pengajar renang bertauliah dengan pengalaman bertanding dan mengajar bertahun-tahun.",
                "Modern Facilities": "Kemudahan Moden",
                "Train in our Olympic-sized heated pools with state-of-the-art equipment and safety systems.":
                    "Berlatih di kolam pemanasan bersaiz Olimpik kami dengan peralatan canggih dan sistem keselamatan.",
                "Progressive Levels": "Tahap Progresif",
                "From beginner to competitive levels, we have a structured program for every swimmer.":
                    "Dari peringkat pemula hingga kompetitif, kami mempunyai program berstruktur untuk setiap perenang.",
                "Get Started Today": "Mulakan Hari Ini",
                "Choose your path to swimming excellence": "Pilih laluan anda ke kecemerlangan berenang",
                "Browse Classes": "Semak Kelas",
                "View our wide range of swimming classes from beginner to advanced levels.":
                    "Lihat pelbagai kelas renang kami dari peringkat pemula hingga lanjutan.",
                "View Classes": "Lihat Kelas",
                "View Schedule": "Lihat Jadual",
                "Check our class schedules and find a time that works for you.":
                    "Semak jadual kelas kami dan cari masa yang sesuai untuk anda.",
                "View Calendar": "Lihat Kalendar",
                "Register Now": "Sertai Sekarang",
                "Join our swimming academy and start your journey to becoming a better swimmer.":
                    "Sertai akademi renang kami dan mulakan perjalanan anda untuk menjadi perenang yang lebih baik.",
                "Login / Register": "Log Masuk / Daftar",
                "Quick Navigation": "Navigasi Pantas",
                "All Classes": "Semua Kelas",
                "Weekly Schedule": "Jadual Mingguan",
                "Monthly Calendar": "Kalendar Bulanan",
                "About Us": "Tentang Kami",
                "Contact": "Hubungi",


                "About AquaSwimming Academy": "Tentang Akademi Renang AquaSwimming",
                "Your journey to swimming excellence begins here": "Perjalanan anda ke kecemerlangan berenang bermula di sini",
                "Our Story": "Kisah Kami",
                "Founded in 2010, AquaSwimming Academy began with a simple mission: to make swimming accessible, safe, and enjoyable for everyone. What started as a small community swimming program has grown into a premier aquatic education center.":
                    "Ditubuhkan pada 2010, Akademi Renang AquaSwimming bermula dengan misi mudah: menjadikan berenang mudah diakses, selamat, dan menyeronokkan untuk semua. Apa yang bermula sebagai program renang komuniti kecil telah berkembang menjadi pusat pendidikan akuatik terkemuka.",
                "Over the past decade, we've taught thousands of students, from toddlers taking their first strokes to adults conquering their fear of water, and competitive swimmers honing their technique for national competitions.":
                    "Sepanjang dekad lalu, kami telah mengajar ribuan pelajar, dari kanak-kanak yang mengambil kayuhan pertama mereka sehingga dewasa menakluki ketakutan mereka terhadap air, dan perenang kompetitif mengasah teknik mereka untuk pertandingan kebangsaan.",
                "Our academy is built on three core principles: Safety First, Progressive Learning, and Individual Attention.":
                    "Akademi kami dibina atas tiga prinsip teras: Keselamatan Dahulu, Pembelajaran Progresif, dan Perhatian Individu.",
                "Our Facilities": "Kemudahan Kami",
                "Two Olympic-sized heated pools": "Dua kolam pemanasan bersaiz Olimpik",
                "Temperature-controlled water year-round": "Air terkawal suhu sepanjang tahun",
                "Advanced filtration and sanitation systems": "Sistem penapisan dan sanitasi maju",
                "Separate beginner and advanced pools": "Kolam pemula dan lanjutan berasingan",
                "State-of-the-art safety equipment": "Peralatan keselamatan canggih",
                "Spacious changing rooms and amenities": "Bilik persalinan luas dan kemudahan",
                "Our Team of Experts": "Pasukan Pakar Kami",
                "Certified Instructors": "Pengajar Bertauliah",
                "All our coaches are certified by national swimming associations with minimum 5 years of teaching experience.":
                    "Semua jurulatih kami ditauliahkan oleh persatuan renang kebangsaan dengan pengalaman mengajar minimum 5 tahun.",
                "Competitive Experience": "Pengalaman Kompetitif",
                "Many of our coaches are former competitive swimmers with national and international competition experience.":
                    "Kebanyakan jurulatih kami adalah bekas perenang kompetitif dengan pengalaman pertandingan kebangsaan dan antarabangsa.",
                "Safety First": "Keselamatan Dahulu",
                "Every staff member is certified in CPR, First Aid, and lifeguarding to ensure maximum safety.":
                    "Setiap kakitangan ditauliahkan dalam CPR, Pertolongan Cemas, dan penyelamatan untuk memastikan keselamatan maksimum.",
                "Our Accreditations": "Pengiktirafan Kami",
                "Swim Australia": "Renang Australia",
                "Royal Life Saving": "Penyelamatan Nyawa Diraja",
                "5-Star Facility": "Kemudahan 5-Bintang",
                "ISO Certified": "Bertauliah ISO",
                "Our Mission": "Misi Kami",
                '"To provide a safe, supportive, and professional environment where individuals of all ages and abilities can develop swimming skills, build confidence, and foster a lifelong love for aquatic activities."':
                    '"Untuk menyediakan persekitaran yang selamat, menyokong, dan profesional di mana individu semua peringkat umur dan kebolehan dapat membangunkan kemahiran berenang, membina keyakinan, dan memupuk kecintaan seumur hidup untuk aktiviti akuatik."',
                "AquaSwimming Academy Team": "Pasukan Akademi Renang AquaSwimming",
                "Contact Us": "Hubungi Kami"



            }
        };

        this.init();
    }

    init() {
        console.log('Initializing language switcher...');

        // Apply translations immediately
        this.applyTranslations();

        // Setup event listeners for DROPDOWN BUTTON
        this.setupEventListeners();

        // Update UI
        this.updateUI();

        console.log('Language switcher initialized successfully');
    }

    setupEventListeners() {
        console.log('Setting up event listeners...');

        // ===== FIX 1: Add event listeners for dropdown items =====
        const languageOptions = document.querySelectorAll('.language-option');
        console.log('Found', languageOptions.length, 'language options');

        languageOptions.forEach(option => {
            option.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();

                const lang = option.getAttribute('data-lang');
                console.log('Language option clicked:', lang);
                this.setLanguage(lang);

                // Close dropdown after selection
                const dropdown = document.getElementById('languageDropdown');
                if (dropdown && bootstrap.Dropdown) {
                    const bsDropdown = bootstrap.Dropdown.getInstance(dropdown);
                    if (bsDropdown) {
                        bsDropdown.hide();
                    }
                }
            });
        });

        // ===== FIX 2: Also handle select element if it exists (for backward compatibility) =====
        const languageSelect = document.getElementById('languageSelect');
        if (languageSelect) {
            console.log('Found language select element');
            languageSelect.value = this.currentLang;
            languageSelect.addEventListener('change', (e) => {
                console.log('Select changed to:', e.target.value);
                this.setLanguage(e.target.value);
            });
        }

        console.log('Event listeners setup complete');
    }

    setLanguage(lang) {
        console.log('Setting language to:', lang);

        if (!this.translations[lang]) {
            console.error('Language not supported:', lang);
            return;
        }

        this.currentLang = lang;
        localStorage.setItem('preferredLanguage', lang);

        // Update HTML lang attribute
        document.documentElement.setAttribute('lang', lang);

        // Update UI
        this.updateUI();

        // Apply translations
        this.applyTranslations();

        // Show notification
        this.showNotification(lang);

        // Dispatch event for other components
        window.dispatchEvent(new CustomEvent('languageChanged', { detail: { language: lang } }));
    }

    updateUI() {
        console.log('Updating UI for language:', this.currentLang);

        // Update button text
        const currentLanguageSpan = document.getElementById('currentLanguage');
        if (currentLanguageSpan) {
            currentLanguageSpan.textContent = this.currentLang.toUpperCase();
            console.log('Updated button text to:', this.currentLang.toUpperCase());
        }

        // Update dropdown active state
        this.updateDropdownActiveState();

        // Update select element if exists
        const languageSelect = document.getElementById('languageSelect');
        if (languageSelect) {
            languageSelect.value = this.currentLang;
        }
    }

    updateDropdownActiveState() {
        console.log('Updating dropdown active state...');

        // Remove active class from all options
        document.querySelectorAll('.language-option').forEach(option => {
            option.classList.remove('active');

            // Remove check icons (except the flag/globe icons)
            const checkIcons = option.querySelectorAll('.fa-check:not(.me-2)');
            checkIcons.forEach(icon => {
                if (!icon.classList.contains('me-2')) {
                    icon.remove();
                }
            });
        });

        // Add active class to selected option
        const selectedOption = document.querySelector(`.language-option[data-lang="${this.currentLang}"]`);
        if (selectedOption) {
            selectedOption.classList.add('active');

            // Add check icon if not already present
            if (!selectedOption.querySelector('.fa-check:not(.me-2)')) {
                const checkIcon = document.createElement('i');
                checkIcon.className = 'fas fa-check ms-2 text-primary';
                selectedOption.appendChild(checkIcon);
            }
        }

        console.log('Dropdown active state updated');
    }

    applyTranslations() {
        console.log('Applying translations for language:', this.currentLang);
        const texts = this.translations[this.currentLang];

        if (!texts) {
            console.error('No translations found for language:', this.currentLang);
            return;
        }

        // Find all elements with data-translate attribute
        const elements = document.querySelectorAll('[data-translate]');
        console.log('Found', elements.length, 'elements to translate');

        let translatedCount = 0;
        let missingCount = 0;

        elements.forEach(element => {
            const key = element.getAttribute('data-translate');
            const translation = texts[key];

            if (translation) {
                if (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA') {
                    element.placeholder = translation;
                } else {
                    element.textContent = translation;
                }
                translatedCount++;
                console.log(`✓ [${this.currentLang}] "${key.substring(0, 30)}..." → translated`);
            } else {
                missingCount++;
                console.warn(`✗ [${this.currentLang}] No translation for: "${key}"`);
            }
        });

        console.log(`Translations applied: ${translatedCount} translated, ${missingCount} missing`);

        // Also translate specific elements by class
        this.translateSpecificElements(texts);
    }

    translateSpecificElements(texts) {
        // Translate specific elements by class or ID
        const elementsToTranslate = [
            { selector: '.hero-title', key: 'Welcome to AquaSwimming Academy' },
            { selector: '.hero-subtitle', key: 'Professional swimming classes for all ages and skill levels. Master the waters with confidence.' },
            { selector: '.page-title', key: 'Why Choose AquaSwimming?' },
            { selector: '.page-subtitle', key: 'Discover the perfect swimming program tailored to your needs and goals' },
        ];

        elementsToTranslate.forEach(item => {
            const element = document.querySelector(item.selector);
            if (element && texts[item.key]) {
                element.textContent = texts[item.key];
            }
        });
    }

    showNotification(lang) {
        const langName = lang === 'en' ? 'English' : 'Bahasa Melayu';

        // Create a simple notification
        const notification = document.createElement('div');
        notification.id = 'languageNotification';
        notification.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            background: #0d6efd;
            color: white;
            padding: 10px 20px;
            border-radius: 5px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 9999;
            opacity: 0;
            transform: translateY(-20px);
            transition: all 0.3s ease;
        `;

        notification.innerHTML = `
            <i class="fas fa-check-circle me-2"></i>
            Language changed to ${langName}
        `;

        document.body.appendChild(notification);

        // Show notification
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateY(0)';
        }, 10);

        // Hide after 3 seconds
        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 300);
        }, 3000);

        console.log(`Language changed to ${langName}`);
    }

    translate(key) {
        return this.translations[this.currentLang][key] || key;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    console.log('DOM fully loaded, initializing language switcher...');
    window.languageSwitcher = new LanguageSwitcher();
    console.log('Language switcher instance created:', window.languageSwitcher);
});