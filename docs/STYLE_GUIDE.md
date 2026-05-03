# ISM Sponsor Style Guide

This document outlines the visual design standards, color scheme, typography, and component styling guidelines for the ISM Sponsor application.

## Color Palette

### Primary Colors
- **ISM Green (Primary)**: `#0d5f3b` (`--ism-green`)
  - Dark variant: `#09482d` (`--ism-green-dark`)
  - Soft variant: `#e7f3ed` (`--ism-green-soft`)
  - Usage: Primary buttons, links, headers, brand identity

- **ISM Yellow (Accent)**: `#f4c542` (`--ism-yellow`)
  - Soft variant: `#fff8df` (`--ism-yellow-soft`)
  - Usage: Highlights, badges, warning states

### Neutral Colors
- **White**: `#ffffff` (`--white`)
- **Gray Scale**:
  - Gray 50: `#f7f8f7` (backgrounds)
  - Gray 100: `#eef1ef` (subtle backgrounds)
  - Gray 200: `#dde3df` (borders)
  - Gray 300: `#c6cec8` (input borders)
  - Gray 500: `#66726b` (secondary text)
  - Gray 700: `#2f3a34` (headings)
  - Gray 900: `#1e2621` (primary text)

### Semantic Colors
- **Success**: `#0f7a4c` (`--success`)
  - Usage: Approved status, success messages
- **Danger**: `#a13a2a` (`--danger`)
  - Usage: Rejected status, error messages, delete actions
- **Warning**: `#8a6800` (`--warning`)
  - Usage: Pending status, warning messages

## Typography

### Font Family
- **Primary Font**: `Montserrat` (Google Fonts)
- **Fallback**: `'Segoe UI', sans-serif`

### Font Weights
- Regular: `400`
- Medium: `500`
- SemiBold: `600`
- Bold: `700`
- ExtraBold: `800`

### Heading Styles
```css
h1 { font-size: 32px; font-weight: 800; }
h2 { font-size: 22px; font-weight: 700; }
h3 { font-size: 18px; font-weight: 700; }
```

### Body Text
- Base font size: `14px`
- Line height: `1.5`
- Color: `var(--gray-900)`

## Buttons

### Standard Button Classes

#### Primary Button (`.btn-primary`)
- Background: `var(--ism-green)` → Hover: `var(--ism-green-dark)`
- Color: `white`
- Border-radius: `25px` (pill shape)
- Padding: `12px 24px`
- Min-height: `44px` (touch-friendly)
- Font-size: `14px`
- Font-weight: `600`

```html
<button class="btn-primary">Primary Action</button>
```

#### Secondary Button (`.btn-secondary`)
- Background: `var(--gray-500)` → Hover: `var(--gray-700)`
- Color: `white`
- Same sizing as primary

```html
<button class="btn-secondary">Secondary Action</button>
```

#### Success Button (`.btn-success`)
- Background: `#16a34a` → Hover: `#15803d`
- Color: `white`

```html
<button class="btn-success">Approve</button>
```

#### Danger Button (`.btn-danger`)
- Background: `var(--danger)` → Hover: `#8b2e1f`
- Color: `white`

```html
<button class="btn-danger">Delete</button>
```

#### Warning Button (`.btn-warning`)
- Background: `#eab308` → Hover: `#ca8a04`
- Color: `var(--gray-900)`

```html
<button class="btn-warning">Warning Action</button>
```

#### Info Button (`.btn-info`)
- Background: `#22c55e` → Hover: `#16a34a`
- Color: `white`

```html
<button class="btn-info">Info Action</button>
```

### Button Modifiers
- `.btn-add` - For "Add" or "Create" actions
- `.btn-block` - Full width button
- `.btn-loading` - Shows loading spinner

### Link Buttons (`.btn-link`)
- No background, styled as text link
- ISM Green color
- Usage: Table actions, inline actions

```html
<a href="#" class="btn-link">View Details</a>
```

## Status Badges

### Badge Classes (`.status-badge`)
All badges use:
- Border-radius: `999px` (full rounded)
- Padding: `4px 10px`
- Font-size: `12px`
- Font-weight: `600`

### Status Colors

#### `.status-active`
- Background: `#d1fae5`
- Color: `#065f46`
- Usage: Active coverages, enabled items

#### `.status-inactive`
- Background: `#fee2e2`
- Color: `#991b1b`
- Usage: Inactive/disabled items

#### `.status-draft`
- Background: `#f3f4f6`
- Color: `#374151`
- Usage: Draft status

#### `.status-submitted`
- Background: `#dbeafe`
- Color: `#1e40af`
- Usage: Submitted for review

#### `.status-pending` / `.status-underreview`
- Background: `#fef3c7`
- Color: `#92400e`
- Usage: Pending approval

#### `.status-approved`
- Background: `#d1fae5`
- Color: `#065f46`
- Usage: Approved items

#### `.status-rejected`
- Background: `#fee2e2`
- Color: `#991b1b`
- Usage: Rejected items

```html
<span class="status-badge status-approved">Approved</span>
<span class="status-badge status-draft">Draft</span>
```

## Alerts

### Alert Classes (`.alert`)
- Border-radius: `10px`
- Padding: `12px 16px`
- Font-size: `14px`
- Border: `1px solid`

#### `.alert-success`
- Background: `#f0fdf4`
- Border: `#bbf7d0`
- Color: `#14532d`

#### `.alert-error`
- Background: `#fef2f2`
- Border: `#fecaca`
- Color: `#991b1b`

#### `.alert-warning`
- Background: `#fffbeb`
- Border: `#fde68a`
- Color: `#78350f`

#### `.alert-info`
- Background: `#eff6ff`
- Border: `#bfdbfe`
- Color: `#1e3a8a`

```html
<div class="alert alert-success">Operation successful!</div>
<div class="alert alert-error">An error occurred.</div>
```

## Cards

### Card Class (`.card`)
- Background: `white`
- Border: `1px solid var(--gray-200)`
- Border-radius: `12px`
- Padding: `24px`
- Margin-bottom: `24px`
- Box-shadow: `0 1px 3px rgba(0, 0, 0, 0.06)`

### Card Variations

#### Narrow Card (`.card-narrow`)
- Max-width: `720px`
- Usage: Forms, single-column content

```html
<div class="card">
    <h2 class="table-title">Card Title</h2>
    <p>Card content...</p>
</div>
```

## Forms

### Form Elements
All inputs use:
- Border: `1px solid var(--gray-300)`
- Border-radius: `8px`
- Padding: `12px 14px`
- Font-size: `14px`
- Min-height: `44px` (touch-friendly)

#### Input Fields
```html
<input type="text" placeholder="Enter text..." />
```

#### Select Dropdowns
- Custom dropdown arrow (ISM Green)
- Border-radius: `8px` (not rounded like buttons)

```html
<select>
    <option>Option 1</option>
    <option>Option 2</option>
</select>
```

#### Textareas
- Min-height: `100px`
- Resize: `vertical`

```html
<textarea rows="4" placeholder="Enter notes..."></textarea>
```

### Form Labels (`.field-label`)
- Font-size: `12px`
- Font-weight: `700`
- Color: `var(--gray-500)`
- Text-transform: `uppercase`
- Letter-spacing: `0.5px`

```html
<label class="field-label">Student Name <span class="required">*</span></label>
<input type="text" />
```

### Form Sections (`.form-section-title`)
- Font-size: `16px`
- Font-weight: `700`
- Color: `var(--ism-green)`
- Border-bottom: `2px solid var(--ism-green-soft)`

```html
<h3 class="form-section-title">Contact Information</h3>
```

### Data Grid (`.data-grid`)
- Display: `grid`
- Template: `repeat(auto-fit, minmax(220px, 1fr))`
- Gap: `14px`
- Usage: Read-only data display

```html
<div class="data-grid">
    <div>
        <p class="field-label">School Year</p>
        <p><strong>2025-2026</strong></p>
    </div>
    <div>
        <p class="field-label">Student ID</p>
        <p><strong>12345</strong></p>
    </div>
</div>
```

## Tables

### Table Structure
- Full width
- Border-collapse: `collapse`
- Background: `white`

### Table Headers (`th`)
- Background: `var(--gray-50)`
- Font-weight: `700`
- Color: `var(--gray-700)`
- Padding: `12px 14px`

### Table Cells (`td`)
- Padding: `12px 14px`
- Border-bottom: `1px solid var(--gray-200)`
- Font-size: `14px`

### Table Actions (`.action-cell`)
- Use `.btn-link` for action links
- Separate multiple actions with pipes: ` | `

```html
<table>
    <thead>
        <tr>
            <th>Name</th>
            <th>Status</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>John Doe</td>
            <td><span class="status-badge status-active">Active</span></td>
            <td class="action-cell">
                <a href="#" class="btn-link">View</a> | 
                <a href="#" class="btn-link">Edit</a>
            </td>
        </tr>
    </tbody>
</table>
```

## Modals

### Modal Structure (`.modal`)
- Position: `fixed`
- Background overlay: `rgba(16, 22, 18, 0.45)`
- Display: `flex` (centered)
- Padding: `18px`

### Modal Content (`.modal-content`)
- Max-width: Varies by content (typically `640px` or `900px`)
- Background: `white`
- Border-radius: `12px`
- Border: `1px solid var(--gray-200)`
- Padding: `16px`

### Modal Header (`.modal-header`)
- Display: `flex`
- Justify-content: `space-between`
- Margin-bottom: `10px`

### Close Button (`.modal-close`)
- Font-size: `28px`
- Color: `var(--gray-700)`
- Hover: Background `var(--gray-100)`

```html
<div id="myModal" class="modal">
    <div class="modal-content">
        <div class="modal-header">
            <h2>Modal Title</h2>
            <button class="modal-close" onclick="closeModal()">&times;</button>
        </div>
        <div class="modal-body">
            <!-- Modal content -->
        </div>
    </div>
</div>
```

## Navigation

### Top Bar (`.top-bar`)
- Position: `sticky`
- Height: `72px`
- Background: `linear-gradient(135deg, var(--ism-green-dark) 0%, #1a5c3a 100%)`
- Box-shadow: `0 2px 8px rgba(0, 0, 0, 0.15)`

### Sidebar (`.sidebar`)
- Width: `240px`
- Background: `white`
- Border-right: `1px solid var(--gray-200)`

### Navigation Items (`.nav-item`)
- use ISM Green for active state
- Gray for inactive states

## Stat Cards (`.stat-card`)

### Base Style
- Border: `1px solid var(--gray-200)`
- Border-left: `6px solid` (color varies)
- Border-radius: `10px`
- Padding: `16px`
- Background: `white`

### Variants
- `.stat-card-success` - Green left border
- `.stat-card-danger` - Red left border
- `.stat-card-warning` - Orange left border
- `.stat-card-info` - Blue left border

### Stat Elements
- `.stat-label` - 12px, uppercase, gray-500
- `.stat-value` - 28px, extra-bold, themed color
- `.stat-sublabel` - 12px, gray-500

```html
<div class="stat-card stat-card-success">
    <div class="stat-label">Total Students</div>
    <div class="stat-value">156</div>
    <div class="stat-sublabel">This school year</div>
</div>
```

## Spacing System

### Standard Spacing
- Extra Small: `4px`
- Small: `8px`
- Medium: `12px`
- Default: `16px`
- Large: `24px`
- Extra Large: `32px`

### Common Patterns
- Card padding: `24px`
- Form group margin: `16px`
- Section spacing: `24px`
- Page padding: `24px`

## Responsive Design

### Breakpoints
- Mobile: `< 768px`
- Tablet: `768px - 1024px`
- Desktop: `> 1024px`

### Touch Targets
- Minimum button height: `44px`
- Minimum tap target: `44px × 44px`

## Accessibility

### Focus States
- Outline: `2px solid var(--ism-green)`
- Outline offset: `2px`

### Color Contrast
- All text meets WCAG AA standards (4.5:1 for normal text)
- Status badges have sufficient contrast

## Animation & Transitions

### Standard Transition
```css
transition: all 0.2s ease;
```

### Common Animations
- Button hover: `0.2s ease`
- Modal fade: `0.3s ease`
- Loading spinner: `1s linear infinite`

### Hover States
- Buttons: Background color change + optional shadow
- Links: Underline + color change
- Cards: Subtle lift with shadow

```css
.card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(13, 95, 59, 0.1);
}
```

## Best Practices

### Do's ✅
- Use CSS variables for colors
- Maintain consistent border-radius (buttons: 25px, cards: 12px, inputs: 8px)
- Use semantic class names
- Follow the established color palette
- Ensure minimum touch targets of 44px
- Use ISM Green as primary brand color
- Keep buttons pill-shaped (border-radius: 25px)
- Use Montserrat font throughout

### Don'ts ❌
- Don't use arbitrary colors outside the palette
- Don't mix button styles (keep all buttons pill-shaped)
- Don't use inline styles when a class exists
- Don't ignore accessibility requirements
- Don't create duplicate CSS
- Don't use different font families

## Component Examples

### Filter Form
```html
<form class="filter-form">
    <div class="filter-group">
        <label class="field-label">Search</label>
        <input type="text" placeholder="Search..." />
    </div>
    <div class="filter-group">
        <label class="field-label">Status</label>
        <select>
            <option>All</option>
            <option>Active</option>
        </select>
    </div>
    <button type="submit" class="btn-primary">Apply</button>
    <a href="#" class="btn-secondary">Clear</a>
</form>
```

### Page Header
```html
<div class="page-header">
    <div class="page-header-actions">
        <div>
            <h1>Page Title</h1>
            <p class="page-subtitle">Page description</p>
        </div>
        <button class="btn-primary">+ New Item</button>
    </div>
</div>
```

### Empty State
```html
<div class="empty-state">
    <p>📭 No items found</p>
    <p>Create your first item to get started.</p>
    <button class="btn-primary">+ Create Item</button>
</div>
```

## Additional Resources

- **Font**: [Montserrat on Google Fonts](https://fonts.google.com/specimen/Montserrat)
- **Icons**: Unicode symbols or custom SVG icons
- **CSS File**: `/wwwroot/css/site.css`

## Maintenance

When adding new components:
1. Check if similar component exists
2. Use existing classes when possible
3. Follow naming conventions
4. Use CSS variables for colors
5. Maintain responsive behavior
6. Test accessibility
7. Document new patterns in this guide

---

**Last Updated**: May 3, 2026
**Version**: 1.0
