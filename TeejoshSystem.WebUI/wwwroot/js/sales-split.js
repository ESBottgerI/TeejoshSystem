window.teejoshSalesSplit = {
  attach: (root, separator) => {
    let dragging = false;
    const enabled = () => window.innerWidth >= 900 && window.matchMedia('(orientation: landscape)').matches;
    const move = event => {
      if (!dragging || !enabled()) return;
      const bounds = root.getBoundingClientRect();
      const minLeft = 352;
      const minRight = 320;
      const pixels = Math.max(minLeft, Math.min(event.clientX - bounds.left, bounds.width - minRight));
      root.style.setProperty('--sales-left', `${pixels}px`);
    };
    const up = () => { dragging = false; document.body.style.userSelect = ''; };
    const down = event => { if (!enabled() || event.button !== 0) return; dragging = true; document.body.style.userSelect = 'none'; event.preventDefault(); };
    separator.addEventListener('mousedown', down);
    document.addEventListener('mousemove', move);
    document.addEventListener('mouseup', up);
    return { dispose: () => { separator.removeEventListener('mousedown', down); document.removeEventListener('mousemove', move); document.removeEventListener('mouseup', up); up(); } };
  }
};
