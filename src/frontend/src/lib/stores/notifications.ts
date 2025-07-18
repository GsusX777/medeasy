// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// MedEasy Notifications Store [CT]
// Verwaltet Benachrichtigungen und Statusmeldungen für die Benutzeroberfläche

import { writable, type Writable } from 'svelte/store';
import { v4 as uuidv4 } from 'uuid';

// Benachrichtigungstyp
export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number; // Millisekunden
  timestamp: Date;
}

// Initialer Zustand
const initialNotifications: Notification[] = [];

// Erstelle den Store
export const notifications: Writable<Notification[]> = writable(initialNotifications);

// Fügt eine Benachrichtigung hinzu
export function showNotification(params: {
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number;
}): string {
  const id = uuidv4();
  const notification: Notification = {
    id,
    type: params.type,
    message: params.message,
    duration: params.duration || 5000, // Standard: 5 Sekunden
    timestamp: new Date()
  };
  
  // Füge Benachrichtigung zum Store hinzu
  notifications.update(items => [...items, notification]);
  
  // Entferne Benachrichtigung nach Ablauf der Dauer
  if (params.duration !== 0) {
    setTimeout(() => {
      removeNotification(id);
    }, params.duration || 5000);
  }
  
  return id;
}

// Entfernt eine Benachrichtigung
export function removeNotification(id: string): void {
  notifications.update(items => items.filter(item => item.id !== id));
}

// Entfernt alle Benachrichtigungen
export function clearNotifications(): void {
  notifications.set([]);
}

// Zeigt eine Erfolgsmeldung an
export function showSuccess(message: string, duration?: number): string {
  return showNotification({
    type: 'success',
    message,
    duration
  });
}

// Zeigt eine Fehlermeldung an
export function showError(message: string, duration?: number): string {
  return showNotification({
    type: 'error',
    message,
    duration
  });
}

// Zeigt eine Warnmeldung an
export function showWarning(message: string, duration?: number): string {
  return showNotification({
    type: 'warning',
    message,
    duration
  });
}

// Zeigt eine Infomeldung an
export function showInfo(message: string, duration?: number): string {
  return showNotification({
    type: 'info',
    message,
    duration
  });
}
