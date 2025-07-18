// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Tests [KP100]
// Testsuite für kritische Sicherheitsfunktionen

pub mod encryption_tests;
pub mod database_tests;
pub mod repository_tests;
pub mod audit_tests;
pub mod key_rotation_tests;
pub mod test_helpers;  // Isolierte Test-Fixtures [ZTS][TR][FSD]
