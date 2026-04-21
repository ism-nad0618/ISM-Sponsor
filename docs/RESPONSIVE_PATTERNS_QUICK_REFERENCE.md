# Responsive Design Quick Reference

Quick guide for implementing responsive patterns in the ISM Sponsor Portal.

---

## 📱 Responsive Breakpoints

```css
/* Mobile-first approach */
@media (min-width: 1200px) { /* Large desktop */ }
@media (max-width: 1199px) { /* Standard desktop */ }
@media (max-width: 979px)  { /* Tablet */ }
@media (max-width: 767px)  { /* Mobile */ }
@media (max-width: 479px)  { /* Small mobile */ }
```

---

## 🍔 Mobile Navigation

### Already Implemented Globally
- Hamburger menu automatically appears < 980px
- Sidebar slides in from left
- Overlay darkens background
- No additional code needed in views

### Layout Structure
```html
<!-- In _Layout.cshtml -->
<button class="mobile-menu-toggle" id="mobileMenuToggle">
    <span class="hamburger-icon"></span>
</button>

<div class="sidebar-overlay" id="sidebarOverlay"></div>

<div class="sidebar" id="sidebar">
    <button class="sidebar-close" id="sidebarClose">×</button>
    <!-- Navigation items -->
</div>
```

---

## 📊 Responsive Tables

### How to Make Any Table Responsive

**Step 1**: Add `data-label` attributes to each `<td>`:

```html
<table>
    <thead>
        <tr>
            <th>Sponsor ID</th>
            <th>Name</th>
            <th>Status</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td data-label="Sponsor ID">ACME</td>
            <td data-label="Name">Acme Corp</td>
            <td data-label="Status">
                <span class="status-badge status-active">Active</span>
            </td>
            <td><!-- No data-label for action column -->
                <a href="#" class="link-edit">Edit</a>
                <a href="#" class="link-delete">Delete</a>
            </td>
        </tr>
    </tbody>
</table>
```

**That's it!** CSS automatically transforms the table at 768px breakpoint.

### Desktop View
- Normal table with headers, rows, cells

### Mobile View (< 768px)
- Headers hidden
- Rows become cards with border/shadow
- Cells stack vertically with labels
- Action column goes full-width

---

## 🎯 Touch-Friendly Controls

### Buttons
All buttons automatically have:
- ✅ Minimum 44x44px touch target
- ✅ 12px 24px padding
- ✅ 14px font size
- ✅ Active state animation
- ✅ No double-tap zoom

```html
<button class="btn-primary">Submit</button>
<a href="#" class="btn btn-secondary">Cancel</a>
```

### Form Inputs
All inputs automatically have:
- ✅ Minimum 44px height
- ✅ 14px font (prevents iOS auto-zoom)
- ✅ Prominent focus states
- ✅ Touch-optimized padding

```html
<input type="text" placeholder="Enter name" />
<select>
    <option>Option 1</option>
</select>
<textarea placeholder="Enter notes"></textarea>
```

---

## 📐 Grid Layouts

### Stat Cards Grid

```html
<div class="stat-grid">
    <div class="stat-card">
        <div class="stat-label">Total Sponsors</div>
        <div class="stat-value">42</div>
    </div>
    <!-- More stat cards -->
</div>
```

**Responsive behavior**:
- Desktop (1200px+): 4 columns
- Tablet (768-979px): 2 columns
- Mobile (<768px): 1 column

### Data Grid

```html
<div class="data-grid">
    <div>
        <span class="field-label">Sponsor Name</span>
        <p><strong>Acme Corporation</strong></p>
    </div>
    <!-- More data items -->
</div>
```

**Responsive behavior**:
- Desktop: Multi-column grid (auto-fit, min 220px)
- Mobile: Single column stack

---

## 📋 Form Layouts

### Multi-column Forms

```html
<div class="form-row">
    <div class="form-group">
        <label class="field-label">First Name</label>
        <input type="text" />
    </div>
    <div class="form-group">
        <label class="field-label">Last Name</label>
        <input type="text" />
    </div>
</div>
```

**Responsive behavior**:
- Desktop: 2-column horizontal
- Mobile (<768px): Stacked vertical

---

## 🎨 Utility Classes

### Status Badges
```html
<span class="status-badge status-active">Active</span>
<span class="status-badge status-inactive">Inactive</span>
```

### Button Variants
```html
<button class="btn-primary">Primary Action</button>
<button class="btn-secondary">Secondary</button>
<button class="btn-success">Success</button>
<button class="btn-warning">Warning</button>
<button class="btn-danger">Danger</button>
```

### Alerts
```html
<div class="alert alert-success">Success message</div>
<div class="alert alert-error">Error message</div>
```

---

## 📱 PWA Features

### Installability
App is PWA-ready with:
- ✅ manifest.webmanifest configured
- ✅ Icons (192x192, 512x512)
- ✅ Theme color (#0d5f3b ISM green)
- ✅ Shortcuts (Dashboard, Sponsors)

### Service Worker
Currently disabled. To enable:
1. Uncomment service worker registration in _Layout.cshtml
2. Update sw.js with caching strategy
3. Test offline functionality

---

## ♿ Accessibility Guidelines

### Keyboard Navigation
- Tab through all interactive elements
- Escape closes mobile menu
- Enter/Space activates buttons

### Focus States
All interactive elements have visible focus:
```css
button:focus,
.btn:focus,
input:focus {
    outline: 2px solid var(--ism-green);
    outline-offset: 2px;
}
```

### ARIA Labels
```html
<button aria-label="Toggle navigation menu">☰</button>
<select aria-label="Active school year">...</select>
```

---

## 🎯 Best Practices

### ✅ DO
- Use `data-label` on all table cells (except actions)
- Use semantic HTML (`<button>`, `<nav>`, `<main>`)
- Provide ARIA labels for icon-only buttons
- Test on real mobile devices
- Check touch target sizes (minimum 44x44px)

### ❌ DON'T
- Use pixel-based media queries for specific devices
- Remove focus outlines (accessibility issue)
- Use `user-scalable=no` (prevents zoom)
- Nest too many flex/grid containers
- Forget to test on actual mobile devices

---

## 🧪 Testing Responsive Design

### Browser DevTools
1. Open DevTools (F12)
2. Toggle device toolbar (Ctrl/Cmd + Shift + M)
3. Test different devices: iPhone SE, iPhone 14, iPad, Desktop

### Breakpoint Testing
- **1200px**: Large desktop (max features)
- **980px**: Desktop → Tablet transition (hamburger appears)
- **768px**: Tablet → Mobile transition (tables → cards)
- **480px**: Mobile → Small mobile (compressed layout)

### Manual Testing
Use real devices:
- iPhone SE (small mobile)
- iPhone 14 Pro (standard mobile)
- iPad (tablet)
- Desktop browser

---

## 🔧 Customization

### Change Breakpoints
Edit `site.css` media queries if needed:
```css
@media (max-width: YOUR_BREAKPOINT_HERE) {
    /* Your responsive styles */
}
```

### Change Sidebar Width
Update these values together:
```css
.sidebar {
    width: 270px; /* Change this */
    left: -270px; /* And this (negative) */
}

.main-content {
    margin-left: 270px; /* And this */
}
```

### Change Touch Targets
Update button/input minimum heights:
```css
button, .btn {
    min-height: 44px; /* Apple/Google guideline */
}

input, select, textarea {
    min-height: 44px;
}
```

---

## 📚 Further Reading

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Apple Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/)
- [Material Design Touch Targets](https://material.io/design/usability/accessibility.html#layout-and-typography)
- [MDN Responsive Design](https://developer.mozilla.org/en-US/docs/Learn/CSS/CSS_layout/Responsive_Design)

---

## 🆘 Common Issues

### Issue: Table not transforming on mobile
**Solution**: Ensure `data-label` attributes are present on `<td>` elements.

### Issue: Hamburger menu not appearing
**Solution**: Check viewport width. Menu appears at 979px and below.

### Issue: iOS text too small / auto-zoom on input
**Solution**: Use 14px+ font size on inputs. Already implemented globally.

### Issue: Touch target too small
**Solution**: Check `min-height` and `padding`. Should be at least 44x44px.

### Issue: Sidebar not sliding smoothly
**Solution**: Check CSS transitions. Should be `left 0.3s ease`.

---

**Last Updated**: Phase 6 Completion
**Maintained By**: ISM Sponsor Portal Development Team
