// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Auth Store [ZTS][ATV]
// Verwaltet den Authentifizierungszustand und Benutzerinformationen

import { writable, type Writable } from 'svelte/store';
import { invoke } from '@tauri-apps/api/tauri';

// Benutzertyp [MDL]
export interface User {
  id: string;
  username: string;
  role: 'arzt' | 'assistent' | 'admin'; // Medizinische Rollen [MDL][PT]
  lastLogin: Date;
}

// Initialer Zustand
const initialState: User | null = null;

// Erstelle den Store [ZTS]
export const currentUser: Writable<User | null> = writable(initialState);

// Login-Funktion [ATV]
export async function login(username: string, password: string): Promise<boolean> {
  try {
    // Authentifiziere Benutzer über Tauri-Befehl
    const user = await invoke<User>('authenticate_user', {
      username,
      password
    });
    
    // Setze Benutzer im Store
    currentUser.set(user);
    
    // Protokolliere Login (ohne Passwort!) [ATV]
    await invoke('log_audit_event', {
      category: 'authentication',
      action: 'login',
      details: `Benutzer ${username} angemeldet`,
      isSensitive: false
    });
    
    return true;
  } catch (error) {
    console.error('Login fehlgeschlagen:', error);
    
    // Protokolliere fehlgeschlagenen Login-Versuch [ATV]
    await invoke('log_audit_event', {
      category: 'authentication',
      action: 'login_failed',
      details: `Login fehlgeschlagen für Benutzer ${username}`,
      isSensitive: false
    });
    
    return false;
  }
}

// Logout-Funktion [ATV]
export async function logout(): Promise<void> {
  // Hole aktuellen Benutzer
  let username = 'unbekannt';
  currentUser.subscribe(user => {
    if (user) {
      username = user.username;
    }
  })();
  
  // Setze Store zurück
  currentUser.set(null);
  
  // Protokolliere Logout [ATV]
  await invoke('log_audit_event', {
    category: 'authentication',
    action: 'logout',
    details: `Benutzer ${username} abgemeldet`,
    isSensitive: false
  });
}

// Prüft, ob Benutzer angemeldet ist
export function isAuthenticated(): boolean {
  let authenticated = false;
  currentUser.subscribe(user => {
    authenticated = user !== null;
  })();
  return authenticated;
}

// Prüft, ob Benutzer bestimmte Rolle hat
export function hasRole(role: 'arzt' | 'assistent' | 'admin'): boolean {
  let hasRequiredRole = false;
  currentUser.subscribe(user => {
    if (user) {
      hasRequiredRole = user.role === role;
    }
  })();
  return hasRequiredRole;
}
