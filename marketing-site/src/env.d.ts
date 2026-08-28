interface Window {
  /** Plausible Analytics queue/proxy. Present even before the script loads (shim in BaseLayout). */
  plausible?: ((
    event: string,
    options?: { props?: Record<string, string | number | boolean>; callback?: () => void },
  ) => void) & { q?: unknown[] };
}
