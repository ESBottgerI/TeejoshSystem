window.teejoshInterop = {
  setTheme: (theme) => document.documentElement.setAttribute('data-theme', theme),
  getPreferredTheme: () => window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
};
