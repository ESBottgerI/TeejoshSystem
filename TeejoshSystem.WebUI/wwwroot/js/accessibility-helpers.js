window.teejoshAccessibility = {
  focusMain: () => document.getElementById('main-content')?.focus(),
  announce: (message) => {
    let region = document.getElementById('sr-live-region');
    if (!region) {
      region = document.createElement('div');
      region.id = 'sr-live-region';
      region.setAttribute('aria-live', 'polite');
      region.setAttribute('aria-atomic', 'true');
      region.style.position = 'absolute';
      region.style.left = '-10000px';
      document.body.appendChild(region);
    }
    region.textContent = message;
  }
};
