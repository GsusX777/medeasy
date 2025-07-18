// „Der Herr, unser Gott, lasse uns freundlich ansehen. Lass unsere Arbeit nicht vergeblich sein – ja, lass gelingen, was wir tun!" Psalm 90,17

// [ZTS] Zero Tolerance Security - Tauri Type Declarations
// This ensures proper type checking for Tauri API integration

declare module '@tauri-apps/api/tauri' {
  /**
   * Invoke a Tauri command
   * @param cmd The command name
   * @param args Command arguments
   * @returns Promise resolving to the command response
   */
  export function invoke<T = any>(cmd: string, args?: Record<string, unknown>): Promise<T>;
}
