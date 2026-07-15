window.teejoshInterop = {
  setTheme: (theme) => document.documentElement.setAttribute('data-theme', theme),
  getPreferredTheme: () => window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light',
  getDashboardPreference: () => {
    try { return JSON.parse(localStorage.getItem('teejosh.dashboard.range.v1')); } catch { return null; }
  },
  setDashboardPreference: (value) => localStorage.setItem('teejosh.dashboard.range.v1', JSON.stringify(value)),
  createObjectUrl: (base64, contentType) => {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let index = 0; index < binary.length; index++) bytes[index] = binary.charCodeAt(index);
    return URL.createObjectURL(new Blob([bytes], { type: contentType }));
  },
  revokeObjectUrl: (url) => { if (url?.startsWith('blob:')) URL.revokeObjectURL(url); },
  focus: (element) => element?.focus()
};
