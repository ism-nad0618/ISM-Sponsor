# Preview.app Annotation Quick Start

## Opening Screenshot in Preview
✅ **Already opened:** `admin_01_login_page.png`

## Enabling Annotation Tools in Preview

1. **Show Markup Toolbar**: Click the toolbox icon (🔧) in the top-right, or press `Shift+Cmd+A`
2. The markup toolbar will appear with annotation tools

## Key Tools You'll Use

### 🔴 Red Arrow (Primary Actions/Buttons)
1. Click the **Arrow** tool in markup toolbar
2. Click **Color** button → Choose **Red**
3. Draw arrow pointing to button/field
4. Adjust arrow position by dragging
5. Make arrow thick: Click **Border Width** → Choose thick line

### 🟡 Yellow Highlight (Important Info)
1. Click the **Rectangle/Shape** tool
2. Choose **Rectangle**
3. Click **Fill Color** → Choose **Yellow** with 30% opacity
4. Draw rectangle around important area
5. Make border invisible: Set **Border Color** to transparent

### 🔵 Blue Box (Section Groupings)
1. Same as yellow highlight but choose **Blue** color
2. Use thicker border: **Border Width** → Medium thickness
3. No fill or light blue fill (20% opacity)

### 📝 Text Callout (Labels/Instructions)
1. Click the **Text** tool (T)
2. Click where you want text
3. Type label (keep it 3-5 words max)
4. Format text:
   - Font size: **14pt minimum** (select text → Format menu → Size)
   - Bold recommended for visibility
   - Color: Match the annotation element (red for actions, black for info)
5. Add speech bubble (optional): Click **Shapes** → **Speech Bubble**

### 🔢 Numbered Steps (Sequential Actions)
1. Click the **Shapes** tool → Choose **Circle**
2. Fill with solid color (red for actions)
3. Add white text inside: Click **Text** tool → Type number → Make text white
4. Or use **Text** tool with numbered list

## Color Palette (Match These Exactly)

**Red** (Primary Actions):
- RGB: (255, 0, 0) or Hex: #FF0000
- Use for: Buttons to click, required fields, action arrows

**Yellow** (Highlights):
- RGB: (255, 255, 0) or Hex: #FFFF00
- Use for: Important information, search boxes, key data
- Set opacity to 30-40% for see-through highlighting

**Blue** (Sections):
- RGB: (0, 120, 255) or Hex: #0078FF
- Use for: Section groupings, table columns, reference areas

**Green** (Success):
- RGB: (0, 200, 0) or Hex: #00C800
- Use for: Success states, confirmations

**Orange** (Warnings):
- RGB: (255, 150, 0) or Hex: #FF9600
- Use for: Warnings, cautions

## Workflow for Each Screenshot

### Step-by-Step Process:

1. **Open** screenshot in Preview (already done for first one)
2. **Enable** Markup Toolbar (`Shift+Cmd+A`)
3. **Refer** to SCREENSHOT_ANNOTATION_GUIDE.md for specific requirements
4. **Add annotations** per specification:
   - Start with arrows (red for actions)
   - Add highlights (yellow for important areas)
   - Add boxes (blue for sections)
   - Add text labels (brief, 3-5 words)
5. **Save** (`Cmd+S`) - overwrites original PNG
6. **Close** and move to next screenshot

### For the Current Screenshot (admin_01_login_page.png):

According to SCREENSHOT_ANNOTATION_GUIDE.md, add:

1. 🔴 **Red arrow** → Username field with label "Enter: admin"
2. 🔴 **Red arrow** → Password field with label "Enter: Admin@123"
3. 🔴 **Red box** → "Login" button with label "Click here"
4. 🟡 **Yellow highlight** → "Forgot Password" link (if visible)

## Tips for Clean Annotations

- **Arrow thickness**: Use thick arrows (3-5px) so they're visible
- **Text size**: Minimum 14pt, bold recommended
- **Spacing**: Don't overlap annotations - leave breathing room
- **Consistency**: Use the same arrow style, text size throughout
- **Test visibility**: Zoom out to see if annotations are clear at normal size

## Saving Your Work

- **Save**: `Cmd+S` (overwrites original - this is what we want)
- **Don't** export - we want to replace the original PNG
- Preview automatically saves in PNG format

## Opening Next Screenshot

After saving, open the next screenshot:

```bash
# In terminal, run:
open -a Preview /Users/cruzr/Documents/ISM\ Sponsor/docs/screenshots/[next-file].png
```

Or use Finder to navigate to `docs/screenshots/` and double-click files.

## Full Screenshot List (In Recommended Order)

### Phase 1: Login Pages (4 screenshots - practice here!)
1. ✅ `admin/logs/admin_01_login_page.png` (currently open)
2. ⏳ `admissions/logs/admissions_01_login_page.png`
3. ⏳ `cashier/logs/cashier_01_login_page.png`
4. ⏳ `sponsor/sponsors/sponsor_01_login_page.png`

### Phase 2: Dashboards (4 screenshots)
5. ⏳ `admin/dashboard/admin_02_dashboard_overview.png`
6. ⏳ `admissions/dashboard/admissions_02_dashboard.png`
7. ⏳ `cashier/dashboard/cashier_02_dashboard.png`
8. ⏳ `sponsor/dashboard/sponsor_02_portal_dashboard.png`

### Phase 3: Admin Screenshots (remaining 9)
9-21. See SCREENSHOT_ANNOTATION_GUIDE.md for full list

### Phase 4: Admissions Screenshots (remaining 8)
22-29. See guide

### Phase 5: Cashier Screenshots (remaining 4)
30-33. See guide

### Phase 6: Sponsor Screenshots (remaining 7)
34-38. See guide

## Time Estimate

- **Per screenshot**: 3-5 minutes
- **Total time**: 2-3 hours for all 38 screenshots
- **Breaks**: Take 5-minute break every 10 screenshots

## Keyboard Shortcuts

- `Shift+Cmd+A` - Show/hide Markup Toolbar
- `Cmd+S` - Save
- `Cmd+W` - Close window
- `Cmd+Z` - Undo last annotation
- `Delete` - Remove selected annotation element

## If You Need Help

- Preview has built-in help: Help menu → Preview Help
- Or refer back to SCREENSHOT_ANNOTATION_GUIDE.md for specifications

---

**Ready to Start?** 

The first screenshot (`admin_01_login_page.png`) is already open in Preview. 

Press `Shift+Cmd+A` to enable the Markup Toolbar and begin annotating!
